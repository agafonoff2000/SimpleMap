using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Drawing;
using System.Windows.Forms;
using ProgramMain.Framework;
using ProgramMain.Framework.WorkerThread;
using ProgramMain.Framework.WorkerThread.Types;
using ProgramMain.Map;
using ProgramMain.Map.Google;

namespace ProgramMain.Layers
{
    public class MapLayer : GraphicLayer
    {
        private const int MaxCacheSize = 240;

        private Rectangle _blockView;
        private readonly Bitmap _emptyBlock;       

        protected class MapWorkerEvent : WorkerEvent
        {
            public GoogleBlock Block;

            public MapWorkerEvent(WorkerEventType eventType, GoogleBlock block, EventPriorityType priorityType)
                : base(eventType, true, priorityType)
            {
                Block = block;
            }
            
            override public int CompareTo(object obj)
            {
                var res = base.CompareTo(obj);
                if (res == 0)
                {
                    if (obj != null)
                    {
                        res = ((MapWorkerEvent) obj).Block.CompareTo(Block);
                    }
                    else
                    {
                        res = -1;
                    }
                }
                return res;
            }
        }

        private struct MapCacheItem
        {
            public long Timestamp;
            public Bitmap Bmp;
        };

        private static readonly SortedDictionary<GoogleBlock, MapCacheItem> MapCache = new SortedDictionary<GoogleBlock, MapCacheItem>();

        public MapLayer(int width, int height, Coordinate centerCoordinate, int level, Control delegateControl, PixelFormat piFormat)
            : base(width, height, centerCoordinate, level, delegateControl, piFormat)
        {
            _emptyBlock = CreateCompatibleBitmap(null, GoogleBlock.BlockSize, GoogleBlock.BlockSize, piFormat);
        }

        override protected void TranslateCoords()
        {
            base.TranslateCoords();

            _blockView = ScreenView.BlockView;
        }
        
        private void PutMapThreadEvent(WorkerEventType eventType, GoogleBlock block, EventPriorityType priorityType)
        {
            PutWorkerThreadEvent(new MapWorkerEvent(eventType, block, priorityType));
        }

        override protected void DrawLayer(Rectangle clipRectangle)
        {
            try
            {
                SwapDrawBuffer();

                var localBlockView = new Rectangle(_blockView.Location, _blockView.Size);
                var localScreenView = (GoogleRectangle)ScreenView.Clone();
                
                while (DrawImages(localBlockView, localScreenView) == false)
                {
                    DropWorkerThreadEvents(WorkerEventType.RedrawLayer);
                    
                    localBlockView = new Rectangle(_blockView.Location, _blockView.Size);
                    localScreenView = (GoogleRectangle)ScreenView.Clone();
                }
            }
            finally
            {
                SwapDrawBuffer();
            }
            FireIvalidateLayer(clipRectangle);
        }

        private bool DrawImages(Rectangle localBlockView, GoogleRectangle localScreenView)
        {
            for (var x = localBlockView.Left; x <= localBlockView.Right; x++)
            {
                for (var y = localBlockView.Top; y <= localBlockView.Bottom; y++)
                {
                    var block = new GoogleBlock(x, y, Level);
                    var pt = ((GoogleCoordinate)block).GetScreenPoint(localScreenView);

                    var bmp = FindImage(block);
                    if (bmp != null)
                    {
                        DrawBitmap(bmp, pt);
                    }
                    else
                    {
                        DrawBitmap(_emptyBlock, pt);
                        PutMapThreadEvent(WorkerEventType.DownloadImage, block, EventPriorityType.Idle);
                    }

                    if (Terminating) return true;

                    if (localScreenView.CompareTo(ScreenView) != 0) return false;
                }
            }
            return true;
        }

        private static bool PointContains(Point pt, Rectangle rect)
        {
            //!!!standard function doesnt work on borders
            return pt.X >= rect.Left && pt.X <= rect.Right
                && pt.Y >= rect.Top && pt.Y <= rect.Bottom;
        }

        private void DrawImage(GoogleBlock block)
        {
            if (Terminating) return;

            if (block.Level == Level && PointContains(block.Pt, _blockView))
            {
                var bmp = FindImage(block);
                if (bmp != null)
                {
                    var rect = ((GoogleRectangle)block).GetScreenRect(ScreenView);
                    DrawBitmap(bmp, rect.Location);

                    FireIvalidateLayer(rect);
                }
            }
        }

        private static Bitmap FindImage(GoogleBlock block)
        {
            if (MapCache.ContainsKey(block))
            {
                var dimg = MapCache[block];
                dimg.Timestamp = DateTime.Now.Ticks;
                return dimg.Bmp;
            }
            return null;
        }

        protected override bool SetCenterCoordinate(Coordinate center, int level)
        {
            var res = base.SetCenterCoordinate(center, level);
            
            if (res)
                DropWorkerThreadEvents(WorkerEventType.DownloadImage);

            return res;
        }

        protected override bool DispatchThreadEvents(WorkerEvent workerEvent)
        {
            var res = base.DispatchThreadEvents(workerEvent);

            if (!res && workerEvent is MapWorkerEvent)
            {
                switch (workerEvent.EventType)
                {
                    case WorkerEventType.DownloadImage:
                        {
                            DownloadImage(((MapWorkerEvent)workerEvent).Block);
                            return true;
                        }
                    case WorkerEventType.DrawImage:
                        {
                            DrawImage(((MapWorkerEvent)workerEvent).Block);
                            return true;
                        }
                }
            }

            return res;
        }

        private void DownloadImage(GoogleBlock block)
        {
            if (MapCache.ContainsKey(block))
            {
                var dimg = MapCache[block];
                if (dimg.Bmp != null) //to turn off compile warning
                {
                    dimg.Timestamp = DateTime.Now.Ticks;
                }
            }
            else
            {
                var bmp = DownloadImageFromFile(block) ?? DownloadImageFromGoogle(block, true);

                if (bmp != null)
                {
                    bmp = CreateCompatibleBitmap(bmp, GoogleBlock.BlockSize, GoogleBlock.BlockSize, PiFormat);

                    var dimg = new MapCacheItem { Timestamp = DateTime.Now.Ticks, Bmp = bmp };
                    MapCache[block] = dimg;

                    TruncateImageCache(block);

                    PutMapThreadEvent(WorkerEventType.DrawImage, block, EventPriorityType.Low);
                }
            }
        }

        private void TruncateImageCache(GoogleBlock newCacheItem)
        {
            while (MapCache.Count > MaxCacheSize)
            {
                var mt = GoogleBlock.Empty;
                var lTicks = DateTime.Now.Ticks;

                foreach (var lt in MapCache)
                {
                    if (lt.Value.Timestamp < lTicks && lt.Key.CompareTo(newCacheItem) != 0)
                    {
                        mt = lt.Key;
                        lTicks = lt.Value.Timestamp;
                    }
                }

                if (mt != GoogleBlock.Empty)
                    MapCache.Remove(mt);
            }
        }

        public static Bitmap DownloadImageFromGoogle(GoogleBlock block, bool getBitmap)
        {
            try
            {
                var oRequest = GoogleMapUtilities.CreateGoogleWebRequest(block);
                var oResponse = (HttpWebResponse) oRequest.GetResponse();

                var bmpStream = new MemoryStream();
                var oStream = oResponse.GetResponseStream();
                if (oStream != null) oStream.CopyTo(bmpStream);
                oResponse.Close();
                if (bmpStream.Length > 0)
                {
                    WriteImageToFile(block, bmpStream);
                    return getBitmap ? (Bitmap) Image.FromStream(bmpStream) : null;
                }
            }
            catch (Exception ex)
            {
                //do nothing
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }
            return null;
        }

        public static Bitmap DownloadImageFromFile(GoogleBlock block)
        {
            try
            {
                var fileName = Properties.Settings.GetMapFileName(block);
                if (File.Exists(fileName))
                {
                    var bmp = (Bitmap)Image.FromFile(fileName);
                    return bmp;
                }
            }
            catch (Exception ex)
            {
                //do nothing
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }
            return null;
        }

        private static void WriteImageToFile(GoogleBlock block, Stream bmpStream)
        {
            var fileName = Properties.Settings.GetMapFileName(block);
            try
            {
                if (!File.Exists(fileName))
                {
                    var path = Path.GetDirectoryName(fileName) ?? "";
                    var destdir = new DirectoryInfo(path);
                    if (!destdir.Exists)
                    {
                        destdir.Create();
                    }
                    var fileStream = File.Create(fileName);
                    try
                    {
                        bmpStream.Seek(0, SeekOrigin.Begin);
                        bmpStream.CopyTo(fileStream);
                    }
                    finally
                    {
                        fileStream.Flush();
                        fileStream.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                //do nothing
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }
        }
    }
}
