using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ProgramMain.Layers;
using ProgramMain.Map;
using ProgramMain.Map.Google;
using ProgramMain.Framework;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace ProgramMain.ExampleForms
{
    public partial class FrmMapDownloader : Form
    {
        private readonly CancellationTokenSource _tokenSource;
        private Task _mapTask;

        protected delegate void ProgressEventDelegate(int progress, int mapBlockNumber, int mapBlockCount);

        protected event ProgressEventDelegate ProgressEvent;

        protected delegate void SaveMapEventDelegate(Bitmap image);

        protected event SaveMapEventDelegate SaveMapEvent;

        private enum WorkMode
        {
            Download,
            SaveToImageFile
        };

        private WorkMode _workMode;

        private PixelFormat _mapPiFormat;
        private int _mapLevel;

        public static void DownloadMap()
        {
            var frm = new FrmMapDownloader
                {
                    _workMode = WorkMode.Download,
                    Text = @"Download Map",
                    labelInfo = {Text = @"Cache all map on local disk"},
                };
            frm.ShowDialog();
        }

        public static void SaveMapAsImage(PixelFormat mapPiFormat, int mapLevel)
        {
            var frm = new FrmMapDownloader
                {
                    _workMode = WorkMode.SaveToImageFile,
                    _mapPiFormat = mapPiFormat,
                    _mapLevel = mapLevel,
                    Text = @"Save Map As Image",
                    labelInfo = {Text = @"Save big map as one image"}
                };

            frm.ShowDialog();
        }

        private FrmMapDownloader()
        {
            InitializeComponent();

            _tokenSource = new CancellationTokenSource();
            ProgressEvent += OnProgress;
            SaveMapEvent += OnSaveMap;
        }

        private void FrmMapDownloader_Load(object sender, EventArgs e)
        {
            var token = _tokenSource.Token;
            switch (_workMode)
            {
                case WorkMode.Download:
                    _mapTask = Task.Factory.StartNew(() => DownloadThread(token), token);
                    break;
                case WorkMode.SaveToImageFile:
                    _mapTask = Task.Factory.StartNew(() => GetFullMapThread(token), token);
                    break;
            }
        }

        private void FrmMapDownloader_FormClosing(object sender, FormClosingEventArgs e)
        {
            _tokenSource.Cancel();
            Task.WaitAll(_mapTask);
            e.Cancel = false;
        }

        protected void OnProgress(int progress, int mapBlockNumber, int mapBlockCount)
        {
            if (progress > 100)
            {
                Close();
                return;
            }
            if (mapBlockNumber % 10 == 0 || progress >= 100)
            {
                progressBar1.Value = progress;
                labProceed.Text = mapBlockNumber.ToString(CultureInfo.InvariantCulture);
                labTotal.Text = mapBlockCount.ToString(CultureInfo.InvariantCulture);
            }
        }

        protected void OnSaveMap(Bitmap image)
        {
            try
            {
                if (saveFileDialog1.ShowDialog(this) == DialogResult.OK)
                {
                    /*var palette = BitmapPalettes.Halftone256;

                    var idxImage = new RenderTargetBitmap(
                        image.Width,
                        image.Height,
                        Screen.PrimaryScreen.BitsPerPixel,
                        Screen.PrimaryScreen.BitsPerPixel,
                        PixelFormats.Indexed8);
                    var visual = new DrawingVisual();
                    var context = visual.RenderOpen();
                    context.DrawImage(image, );
                    idxImage.Render();
                    Graphics.FromImage(idxImage);             

                    */
                    {
                        image.Save(saveFileDialog1.FileName, ImageFormat.Png);
                    }
                }
            }
            finally
            {
                Close();
            }
        }

        private void DownloadThread(CancellationToken ct)
        {
            var leftBound = new Coordinate(Properties.Settings.Default.LeftMapBound, Properties.Settings.Default.TopMapBound);
            var rightBound = new Coordinate(Properties.Settings.Default.RightMapBound, Properties.Settings.Default.BottomMapBound);

            var rectBound = new CoordinateRectangle(leftBound, rightBound);

            var mapBlockCount = 0;
            for (var mapLevel = Properties.Settings.Default.MinZoomLevel; mapLevel <= Properties.Settings.Default.MaxZoomLevel; mapLevel++)
            {
                var mapWidth = Convert.ToInt32((new GoogleCoordinate(rectBound.RightTop, mapLevel)).X - (new GoogleCoordinate(rectBound.LeftTop, mapLevel)).X) + 2 * GoogleBlock.BlockSize;
                var mapHeight = Convert.ToInt32((new GoogleCoordinate(rectBound.LeftBottom, mapLevel)).Y - (new GoogleCoordinate(rectBound.LeftTop, mapLevel)).Y) + 2 * GoogleBlock.BlockSize;

                var viewBound = rectBound.LineMiddlePoint.GetScreenViewFromCenter(mapWidth, mapHeight, mapLevel);
                var blockView = viewBound.BlockView;
                mapBlockCount += (blockView.Right - blockView.Left + 1) * (blockView.Bottom - blockView.Top + 1);
            }

            var mapBlockNumber = 0;
            BeginInvoke(ProgressEvent, new Object[] {mapBlockNumber * 100 / mapBlockCount, mapBlockNumber, mapBlockCount});

            for (var mapLevel = Properties.Settings.Default.MinZoomLevel; mapLevel <= Properties.Settings.Default.MaxZoomLevel; mapLevel++)
            {
                var mapWidth = Convert.ToInt32((new GoogleCoordinate(rectBound.RightTop, mapLevel)).X - (new GoogleCoordinate(rectBound.LeftTop, mapLevel)).X) + 2 * GoogleBlock.BlockSize;
                var mapHeight = Convert.ToInt32((new GoogleCoordinate(rectBound.LeftBottom, mapLevel)).Y - (new GoogleCoordinate(rectBound.LeftTop, mapLevel)).Y) + 2 * GoogleBlock.BlockSize;

                var viewBound = rectBound.LineMiddlePoint.GetScreenViewFromCenter(mapWidth, mapHeight, mapLevel);
                var blockView = viewBound.BlockView;

                for (var x = blockView.Left; x <= blockView.Right; x++)
                {
                    for (var y = blockView.Top; y <= blockView.Bottom; y++)
                    {
                        var block = new GoogleBlock(x, y, mapLevel);

                        var fileName = Properties.Settings.GetMapFileName(block);
                        if (!File.Exists(fileName))
                            MapLayer.DownloadImageFromGoogle(block, false);

                        mapBlockNumber++;

                        BeginInvoke(ProgressEvent, new Object[] {mapBlockNumber * 100 / mapBlockCount, mapBlockNumber, mapBlockCount});

                        if (ct.IsCancellationRequested)
                            return;
                    }
                }
            }
            BeginInvoke(ProgressEvent, new Object[] {101, mapBlockNumber, mapBlockCount});
        }

        private void GetFullMapThread(CancellationToken ct)
        {
            var leftBound = new Coordinate(Properties.Settings.Default.LeftMapBound, Properties.Settings.Default.TopMapBound);
            var rightBound = new Coordinate(Properties.Settings.Default.RightMapBound, Properties.Settings.Default.BottomMapBound);

            var rectBound = new CoordinateRectangle(leftBound, rightBound);

            try
            {
                var mapWidth = Convert.ToInt32((new GoogleCoordinate(rectBound.RightTop, _mapLevel)).X - (new GoogleCoordinate(rectBound.LeftTop, _mapLevel)).X) + 2 * GoogleBlock.BlockSize;
                var mapHeight = Convert.ToInt32((new GoogleCoordinate(rectBound.LeftBottom, _mapLevel)).Y - (new GoogleCoordinate(rectBound.LeftTop, _mapLevel)).Y) + 2 * GoogleBlock.BlockSize;

                var image = GraphicLayer.CreateCompatibleBitmap(null, mapWidth, mapHeight, _mapPiFormat);
                var graphics = Graphics.FromImage(image);

                var viewBound = rectBound.LineMiddlePoint.GetScreenViewFromCenter(mapWidth, mapHeight, _mapLevel);
                var blockView = viewBound.BlockView;
                var mapBlockCount = (blockView.Right - blockView.Left + 1) * (blockView.Bottom - blockView.Top + 1);
                var mapBlockNumber = 0;
            
                BeginInvoke(ProgressEvent, new Object[] {mapBlockNumber * 100 / mapBlockCount, mapBlockNumber, mapBlockCount});

                for (var x = blockView.Left; x <= blockView.Right; x++)
                {
                    for (var y = blockView.Top; y <= blockView.Bottom; y++)
                    {
                        var block = new GoogleBlock(x, y, _mapLevel);
                        var bmp = GraphicLayer.CreateCompatibleBitmap(
                            MapLayer.DownloadImageFromFile(block) ?? MapLayer.DownloadImageFromGoogle(block, true),
                            GoogleBlock.BlockSize, GoogleBlock.BlockSize, _mapPiFormat);

                        var rect = ((GoogleRectangle) block).GetScreenRect(viewBound);
                        graphics.DrawImageUnscaled(bmp, rect.Location.X, rect.Location.Y);

                        mapBlockNumber++;

                        BeginInvoke(ProgressEvent, new Object[]{mapBlockNumber * 100 / mapBlockCount, mapBlockNumber, mapBlockCount});

                        if (ct.IsCancellationRequested)
                            return;
                    }
                }

                BeginInvoke(SaveMapEvent, new Object[] {image});
            }
            catch (Exception e)
            {
                BeginInvoke(ProgressEvent, new Object[] { 101, 0, 0 });
                MessageBox.Show(e.Message, @"Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
