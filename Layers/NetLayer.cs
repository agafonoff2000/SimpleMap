using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ProgramMain.ExampleDb;
using ProgramMain.Framework;
using ProgramMain.Framework.WorkerThread;
using ProgramMain.Framework.WorkerThread.Types;
using ProgramMain.Layers.MapObjects;
using ProgramMain.Map;
using ProgramMain.Map.Google;

using HashItem = System.Collections.Generic.KeyValuePair<double, int>;
using NearestSet = System.Collections.Generic.HashSet<System.Collections.Generic.KeyValuePair<double, int>>;

namespace ProgramMain.Layers
{
    public class NetLayer : GraphicLayer
    {
        private VertexTree _rVertexTree;
        private CableTree _rCableTree;

        private readonly ReaderWriterLockSlim _lockVr = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private readonly ReaderWriterLockSlim _lockCr = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        protected class NetWorkerEvent : WorkerEvent
        {
            public NetWorkerEvent(WorkerEventType eventType, EventPriorityType priorityType)
                : base(eventType, true, priorityType)
            {
            }

            override public int CompareTo(object obj)
            {
                var res = base.CompareTo(obj);
                return res;
            }
        }

        private static readonly Bitmap[] VertexDrawValues = { null, null, null };
        public Bitmap Vertex
        {
            get
            {
                if (VertexDrawValues[0] == null)
                {
                    VertexDrawValues[0] = CreateCompatibleBitmap(Properties.Resources.vertex_8, PiFormat);
                    VertexDrawValues[1] = CreateCompatibleBitmap(Properties.Resources.vertex_16, PiFormat);
                    VertexDrawValues[2] = CreateCompatibleBitmap(Properties.Resources.vertex_24, PiFormat);
                }
                if (Level <= 14)
                    return VertexDrawValues[0];
                if (Level > 16)
                    return VertexDrawValues[2];
                
                return VertexDrawValues[1];
            }
        }

        private readonly static Size[] HalfVertexValues = { new Size(4, 4), new Size(8, 8), new Size(12, 12) };
        public Size HalfVertexSize
        {
            get
            {
                if (Level <= 14)
                    return HalfVertexValues[0];
                if (Level > 16)
                    return HalfVertexValues[2];
                
                return HalfVertexValues[1];
            }
        }

        public double CoordinateTolerance { get; private set; }

        public NetLayer(int width, int height, Coordinate centerCoordinate, int level, Control delegateControl, PixelFormat piFormat)
            : base(width, height, centerCoordinate, level, delegateControl, piFormat)
        {
            CoordinateTolerance = 0;

            _rVertexTree = new VertexTree();
            _rCableTree = new CableTree();
        }

        override protected void TranslateCoords()
        {
            base.TranslateCoords();

            //Fault distance on mouse click to detect click on objects 
            CoordinateTolerance = CenterCoordinate.Distance(
                CenterCoordinate + new GoogleCoordinate(HalfVertexSize.Width, 0/*HalfVertexSize.Height*/, Level));
        }

        protected override bool DispatchThreadEvents(WorkerEvent workerEvent)
        {
            var res = base.DispatchThreadEvents(workerEvent);

            if (!res && workerEvent is NetWorkerEvent)
            {
                switch (workerEvent.EventType)
                {
                    case WorkerEventType.ReloadData:
                        {
                            ReadDbData();
                            return true;
                        }
                }
            }

            return res;
        }

        public void ReloadData()
        {
            PutWorkerThreadEvent(new NetWorkerEvent(WorkerEventType.ReloadData, EventPriorityType.Normal));
        }

        override protected void DrawLayer(Rectangle clipRectangle)
        {
            try
            {
                SwapDrawBuffer();

                //!!!Draw all objects after FillTransparent (*ClipRectangle)
                FillTransparent();

                var localScreenView = (GoogleRectangle)ScreenView.Clone();

                RedrawCables(localScreenView);
                RedrawVertexes(localScreenView);
            }
            finally
            {
                SwapDrawBuffer();
            }
            FireIvalidateLayer(clipRectangle);
        }

        private void ReadDbData()
        {
            var tasks = new List<Task>
                {
                    Task.Factory.StartNew(ReadVertexes),
                    Task.Factory.StartNew(ReadCables)
                };
            Task.WaitAll(tasks.ToArray());
            
            Update(new Rectangle(0, 0, Width, Height));
        }

        private void ReadVertexes()
        {
            var vertexTree = new VertexTree();
            vertexTree.LoadData();

            try
            {
                _lockVr.EnterWriteLock();
                _rVertexTree = vertexTree;
            }
            finally
            {
                _lockVr.ExitWriteLock();
            }
        }

        private void ReadCables()
        {
            var cableTree = new CableTree();
            cableTree.LoadData();

            try
            {
                _lockCr.EnterWriteLock();
                _rCableTree = cableTree;
            }
            finally
            {
                _lockCr.ExitWriteLock();
            }
        }

        private void RedrawVertexes(GoogleRectangle localScreenView)
        {
            try
            {
                _lockVr.EnterReadLock();
                if (_rVertexTree.NodeCount == 0) return;

                var res = _rVertexTree.Query(localScreenView);

                foreach (var node in res)
                {
                    var row = (SimpleMapDb.VertexesRow) node.Row;

                    var coordinate = new Coordinate(row.Longitude, row.Latitude);
                    var pt = coordinate.GetScreenPoint(localScreenView);

                    DrawBitmap(Vertex, Point.Subtract(pt, HalfVertexSize));
                    var caption = row.Caption;
                    if (!String.IsNullOrEmpty(caption))
                        DrawString(caption, HalfVertexSize.Height, Point.Add(pt, HalfVertexSize));
                }
            }
            catch (Exception ex)
            {
                //do nothing
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }
            finally
            {
                _lockVr.ExitReadLock();
            }
        }

        private void RedrawCables(GoogleRectangle localScreenView)
        {
            try
             {
                _lockCr.EnterReadLock();

                if (_rCableTree.NodeCount == 0) return;

                var res = _rCableTree.Query(localScreenView);
                foreach (var node in res)
                {
                    var row = (SimpleMapDb.CablesRow) node.Row;

                    var cabRect = new CoordinateRectangle(row.Longitude1, row.Latitude1, row.Longitude2, row.Latitude2);
                    var rect = cabRect.GetScreenRect(localScreenView);

                    DrawLine(rect, 2, Color.Blue);

                    var coordinate = cabRect.LineMiddlePoint;
                    var caption = row.Caption;
                    var point = coordinate.GetScreenPoint(localScreenView);
                    if (!String.IsNullOrEmpty(caption))
                        DrawString(caption, HalfVertexSize.Height, Point.Add(point, HalfVertexSize));
                }
             }
             catch(Exception ex)
             {
                 //do nothing
                 System.Diagnostics.Trace.WriteLine(ex.Message);
             }
            finally
            {
                _lockCr.ExitReadLock();
            }
        }

        public SimpleMapDb.VertexesRow GetVertex(int objectId)
        {
            try
            {
                _lockVr.EnterReadLock();

                return _rVertexTree.GetVertex(objectId);
            }
            finally
            {
                _lockVr.ExitReadLock();
            }
        }

        public void MergeData(SimpleMapDb.VertexesDataTable vertexes)
        {
            try
            {
                _lockVr.EnterWriteLock();

                _rVertexTree.MergeData(vertexes);
            }
            finally
            {
                _lockVr.ExitWriteLock();
            }
            Update(new Rectangle(0, 0, Width, Height));
        }

        public void RemoveVertex(int objectId)
        {
            bool bUpdate;
            try
            {
                _lockVr.EnterWriteLock();

                if (_rVertexTree.NodeCount == 0) return;

                bUpdate = _rVertexTree.RemoveVertex(objectId);
            }
            finally
            {
                _lockVr.ExitWriteLock();
            }
            if (bUpdate)
                Update(new Rectangle(0, 0, Width, Height));
        }

        public SimpleMapDb.CablesRow GetCable(int objectId)
        {
            try
            {
                _lockCr.EnterReadLock();

                return _rCableTree.GetCable(objectId);
            }
            finally
            {
                _lockCr.ExitReadLock();
            }
        }

        public void MergeData(SimpleMapDb.CablesDataTable cables)
        {
            try
            {
                _lockCr.EnterWriteLock();

                _rCableTree.MergeData(cables);
            }
            finally
            {
                _lockCr.ExitWriteLock();
            }
            Update(new Rectangle(0, 0, Width, Height));
        }

        public void RemoveCable(int objectId)
        {
            bool bUpdate;
            try
            {
                _lockCr.EnterWriteLock();
                
                if (_rCableTree.NodeCount == 0) return;

                bUpdate = _rCableTree.RemoveCable(objectId);
            }
            finally
            {
                _lockCr.ExitWriteLock();
            }
            if (bUpdate)
                Update(new Rectangle(0, 0, Width, Height));
        }

        public HashItem[] FindNearestVertex(Coordinate pt)
        {
            var list = new NearestSet();
            try
            {
                _lockVr.EnterReadLock();

                if (_rVertexTree.NodeCount == 0)
                    return list.ToArray();

                var res = _rVertexTree.Distance(pt, CoordinateTolerance);
                foreach (var node in res)
                {
                    var row = (SimpleMapDb.VertexesRow)node.Row;

                    var coordinate = new Coordinate(row.Longitude, row.Latitude);

                    var distance = coordinate.Distance(pt);
                    list.Add(new HashItem(distance, row.ID));
                }
            }
            finally
            {
                _lockVr.ExitReadLock();
            }
            return list.OrderBy(item => item.Key).ToArray();
        }

        public HashItem[] FindNearestCable(Coordinate pt)
        {
            var list = new NearestSet();
            try
            {
                _lockCr.EnterReadLock();

                if (_rCableTree.NodeCount == 0)
                    return list.ToArray();

                var res = _rCableTree.Distance(pt, CoordinateTolerance);
                foreach (var node in res)
                {
                    var row = (SimpleMapDb.CablesRow)node.Row;

                    var cableRect = new CoordinateRectangle(row.Longitude1, row.Latitude1, row.Longitude2, row.Latitude2);

                    var distance = cableRect.LineDistance(pt);
                    list.Add(new HashItem(distance, row.ID));
                }
            }
            finally
            {
                _lockCr.ExitReadLock();
            }
            
            return list.OrderBy(item => item.Key).ToArray();
        }

        public void ClearData()
        {
            try
            {
                _lockCr.EnterWriteLock();

                _rCableTree = new CableTree();
            }
            finally
            {
                _lockCr.ExitWriteLock();
            }
            try
            {
                _lockVr.EnterWriteLock();

                _rVertexTree = new VertexTree();
            }
            finally
            {
                _lockVr.ExitWriteLock();
            }
            Update(new Rectangle(0, 0, Width, Height));
        }
    }
}
