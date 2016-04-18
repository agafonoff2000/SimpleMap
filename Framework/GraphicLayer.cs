using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Threading;
using System.Windows.Forms;
using ProgramMain.Framework.WorkerThread;
using ProgramMain.Framework.WorkerThread.Types;
using ProgramMain.Map;
using ProgramMain.Map.Google;

namespace ProgramMain.Framework
{
    public class GraphicLayer : WorkerMessageThread
    {
        private readonly Bitmap[] _offScreen = {null, null};
        private readonly Graphics[] _offScreenDc = {null, null};

        private readonly SemaphoreSlim _lockDc = new SemaphoreSlim(1, 1);
        private enum ActiveDrawBuffer {DC0 = 0, DC1 = 1};
        private ActiveDrawBuffer _activeDC = ActiveDrawBuffer.DC0;

        private Coordinate _centerCoordinate;
        private int _level = Properties.Settings.Default.StartZoomLevel;

        public Coordinate CenterCoordinate
        {
            get
            {
                return (Coordinate)_centerCoordinate.Clone();
            }
            set
            {
                SetCenterCoordinate(value, _level);
            }
        }
        public int Level
        {
            get
            {
                return _level;
            }
            set
            {
                SetCenterCoordinate(_centerCoordinate, value);
            }
        }

        private GoogleRectangle _screenView;
        public GoogleRectangle ScreenView
        {
            get
            {
                return (GoogleRectangle)_screenView.Clone();
            }
        }
        private CoordinateRectangle _coordinateView;
        protected CoordinateRectangle CoordinateView
        {
            get
            {
                return (CoordinateRectangle)_coordinateView.Clone();
            }
        }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public PixelFormat PiFormat { get; private set; }

        public GraphicLayer(int pWidth, int pHeight, Coordinate centerCoordinate, int pLevel, 
            Control delegateControl, PixelFormat piFormat) :
            base(delegateControl)
        {
            Width = pWidth;
            Height = pHeight;

            _centerCoordinate = centerCoordinate;
            _level = pLevel;

            PiFormat = piFormat;

            Resize(false);
            _drawLayerEvent += OnIvalidateLayer;
        }

        public class InvalidateLayerEventArgs : OwnerEventArgs
        {
            public InvalidateLayerEventArgs()
            {

            }

            public InvalidateLayerEventArgs(Rectangle pRect)
            {
                ClipRectangle = pRect;
            }

            public readonly Rectangle ClipRectangle;
        }

        public event OwnerEventHandler<InvalidateLayerEventArgs> DrawLayerBuffer;

        private readonly OwnerEventDelegate<InvalidateLayerEventArgs> _drawLayerEvent;

        private void OnIvalidateLayer(InvalidateLayerEventArgs eventArgs)
        {
            if (DrawLayerBuffer != null)
            {
                DrawLayerBuffer(this, eventArgs);
            }
        }

        protected void FireIvalidateLayer(Rectangle clipRectangle)
        {
            FireOwnerEvent(_drawLayerEvent, new InvalidateLayerEventArgs(clipRectangle));
        }

        protected override bool DispatchThreadEvents(WorkerEvent workerEvent)
        {
            switch (workerEvent.EventType)
            {
                case WorkerEventType.RedrawLayer:
                    {
                        DrawLayer(new Rectangle(0, 0, Width, Height));
                        return true;
                    }
            }
            return base.DispatchThreadEvents(workerEvent);
        }

        protected virtual bool SetCenterCoordinate(Coordinate center, int level)
        {
            if (_level != level
                || new GoogleCoordinate(_centerCoordinate, level).CompareTo(
                new GoogleCoordinate(center, level)) != 0)
            {
                _centerCoordinate = center;
                _level = level;
                TranslateCoords();
                Update(new Rectangle(0, 0, Width, Height));
                return true;
            }
            return false;
        }

        public static PixelFormat ObtainCompatiblePixelFormat(Control control)
        {
            var screenContext = control.CreateGraphics();
            var screenTestBmp = new Bitmap(1, 1, screenContext);
            return screenTestBmp.PixelFormat; //to obtain fast image draw into GDI context
        }

        protected static Bitmap CreateCompatibleBitmap(Bitmap org, PixelFormat piFormat)
        {
            var gdiBmp = new Bitmap(org.Width, org.Height, piFormat);
            var gdiContext = Graphics.FromImage(gdiBmp);
            gdiContext.DrawImage(org, 0, 0, org.Width, org.Height);

            return gdiBmp;
        }

        public static Bitmap CreateCompatibleBitmap(Bitmap org, int width, int height, PixelFormat piFormat)
        {
            var gdibmp = new Bitmap(width, height, piFormat);
            var gdiContext = Graphics.FromImage(gdibmp);
            if (org != null)
            {
                gdiContext.DrawImageUnscaledAndClipped(org, new Rectangle(0, 0, width, height));
            }
            else
            {
                gdiContext.Clear(Color.White);
            }
            return gdibmp;
        }

        private void Resize(bool bUpdate)
        {
            try
            {
                _lockDc.Wait();
                
                if (Width > 0 && Height > 0 && !Terminating)
                {
                    for (var i = 0; i <= 1; i++)
                    {
                        _offScreen[i] = new Bitmap(Width, Height, PiFormat);
                        _offScreenDc[i] = Graphics.FromImage(_offScreen[i]);

                        _offScreenDc[i].InterpolationMode = InterpolationMode.Low;
                        _offScreenDc[i].CompositingQuality = CompositingQuality.HighSpeed;
                        _offScreenDc[i].SmoothingMode = SmoothingMode.HighSpeed;
                    }
                    _activeDC = ActiveDrawBuffer.DC0;
                    
                    TranslateCoords();

                    if (bUpdate)
                        Update(new Rectangle(0, 0, Width, Height));
                }
                else
                {
                    _offScreen[0] = null;
                    _offScreenDc[0] = null;
                    _offScreen[1] = null;
                    _offScreenDc[1] = null;
                }
            }
            finally
            {
                _lockDc.Release();
            }
        }

        public void Resize(int pWidth, int pHeight)
        {
            if (pWidth != Width || pHeight != Height)
            {
                Width = pWidth;
                Height = pHeight;
                Resize(true);
            }
        }

        virtual protected void TranslateCoords()
        {
            _screenView = _centerCoordinate.GetScreenViewFromCenter(Width, Height, _level);
            _coordinateView = ScreenView;
        }

        public void Update()
        {
            Update(Rectangle.Empty);
        }

        public void Update(Rectangle clipRectangle)
        {
            if (_offScreenDc != null && !Terminating)
            {
                PutWorkerThreadEvent(WorkerEventType.RedrawLayer, true, EventPriorityType.BelowNormal);
            }
        }

        virtual protected void DrawLayer(Rectangle clipRectangle)
        {
        }

        protected void DrawBitmap(Bitmap bmp, int x, int y)
        {
            try
            {
                _lockDc.Wait();
                _offScreenDc[0].DrawImageUnscaled(bmp, x, y);
            }
            finally
            {
                _lockDc.Release();
            }
        }

        protected void DrawBitmap(Bitmap bmp, Point point)
        {
            DrawBitmap(bmp, point.X, point.Y);
        }

        protected void DrawString(string caption, int size, int x, int y, Brush brush)
        {
            var font = new Font("Arial", size,  FontStyle.Regular);
            try
            {
                _lockDc.Wait();
                _offScreenDc[0].TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
                _offScreenDc[0].DrawString(caption, font, brush, x, y);
            }
            finally
            {
                _lockDc.Release();
            }
        }

        protected void DrawString(string caption, int size, Point pt)
        {
            var brush = new SolidBrush(Color.Black);
            DrawString(caption, size, pt.X, pt.Y, brush);
        }

        public void DrawLine(Rectangle lineRectangle, int thickness, Color color)
        {
            var pen = new Pen(color, thickness);
            try
            {
                _lockDc.Wait();
                _offScreenDc[0].DrawLine(pen, lineRectangle.Left, lineRectangle.Top, lineRectangle.Right, lineRectangle.Bottom);
            }
            finally
            {
                _lockDc.Release();
            }
        }

        protected void FillColor(Color color)
        {
            try
            {
                _lockDc.Wait();
                _offScreenDc[0].Clear(color);
            }
            finally
            {
                _lockDc.Release();
            }
        }

        protected void FillTransparent()
        {
            FillColor(Color.FromArgb(0, 255, 255, 255));
        }

        public void DrawBufferToScreen(Graphics clientDC, Rectangle clipRectangle)
        {
            try
            {
                _lockDc.Wait();
                if (Width == clipRectangle.Width && Height == clipRectangle.Height)
                    clientDC.DrawImageUnscaled(_offScreen[(int)_activeDC], 0, 0);
                else
                    clientDC.DrawImage(
                        _offScreen[(int)_activeDC], 
                        clipRectangle.X, clipRectangle.Y, 
                        clipRectangle, GraphicsUnit.Pixel);
            }
            finally
            {
                _lockDc.Release();
            }
        }
        
        protected void SwapDrawBuffer()
        {
            try
            {
                _lockDc.Wait();
                
                if (_activeDC == ActiveDrawBuffer.DC0)
                {
                    var tmpScreen = _offScreen[0];
                    var tmpScreenDC = _offScreenDc[0];
                    _offScreen[0] = _offScreen[1];
                    _offScreenDc[0] = _offScreenDc[1];
                    _offScreen[1] = tmpScreen;
                    _offScreenDc[1] = tmpScreenDC;

                    _activeDC = ActiveDrawBuffer.DC1;
                }
                else
                {
                    _activeDC = ActiveDrawBuffer.DC0;
                }
            }
            finally
            {
                _lockDc.Release();
            }
        }
    }
}
