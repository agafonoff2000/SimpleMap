using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Windows.Forms;
using ProgramMain.ExampleDb;
using ProgramMain.Framework;
using ProgramMain.Layers;
using ProgramMain.Map;
using ProgramMain.Map.Google;
using ProgramMain.Properties;

namespace ProgramMain.ExampleForms.Controls
{
    public partial class MapCtl : UserControl
    {
        private readonly MapLayer _mapLayer;
        private readonly NetLayer _netLayer;
     
        public int Level
        {
            get
            {
                return _mapLayer.Level;
            }
            set
            {
                if (value != _mapLayer.Level)
                {
                    _mapLayer.Level = value;
                    _netLayer.Level = value;
                    if (LevelValueChanged != null)
                        LevelValueChanged(this, new ButtonPanelCtl.LevelValueArgs(value));
                }
            }
        }
        public event EventHandler<ButtonPanelCtl.LevelValueArgs> LevelValueChanged;

        public PixelFormat PiFormat
        {
            get { return _mapLayer.PiFormat; }    
        }

        public Coordinate CenterCoordinate
        {
            get
            {
                return _mapLayer.CenterCoordinate;
            }
            set
            {
                _mapLayer.CenterCoordinate = value;
                _netLayer.CenterCoordinate = value;
            }
        }

        private bool _mouseMoveMap;
        private Point _mousePreviousLocation;
        private Coordinate _coordinatePreviosLocation;
        private bool ShiftKey { get; set; }
        private int _menuObjectId;

        //protected override CreateParams CreateParams
        //{
        //    get
        //    {
        //        var handleParam = base.CreateParams;
        //        handleParam.ExStyle |= 0x02000000;   // WS_EX_COMPOSITED       
        //        return handleParam;
        //    }
        //}

        public MapCtl()
        {
            InitializeComponent();

            var piFormat = GraphicLayer.ObtainCompatiblePixelFormat(this);

            var coordinate = new Coordinate(Settings.Default.StartLongitude, Settings.Default.StartLatitude);

            _mapLayer = new MapLayer(Width, Height, coordinate, Settings.Default.StartZoomLevel, this, piFormat);
            _netLayer = new NetLayer(Width, Height, coordinate, Settings.Default.StartZoomLevel, this, piFormat);
            _mapLayer.DrawLayerBuffer += Layer_DrawBufferChanged;
            _netLayer.DrawLayerBuffer += Layer_DrawBufferChanged;

            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.Opaque, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, false);
            
            MouseWheel += MapCtl_MouseWheel;

            RefreshControl();
        }

        private void MapCtl_Resize(object sender, EventArgs e)
        {
            if (_mapLayer.Terminating) return;

            _mapLayer.Resize(Width, Height);
            _netLayer.Resize(Width, Height);
        }

        private void DrawLayers(Graphics clientDC, Rectangle clipRectangle)
        {
            if (_mapLayer.Terminating) return;

            //clientDC.Clear(Color.White);
            
            _mapLayer.DrawBufferToScreen(clientDC, clipRectangle);
            _netLayer.DrawBufferToScreen(clientDC, clipRectangle);
        }

        public void RefreshControl()
        {
            _mapLayer.Update(new Rectangle(0, 0, Width, Height));
            
            //_netLayer.ReloadData();
            GenerateSampleData();
        }

        private void GenerateSampleData()
        {
            _netLayer.ClearData();

            //--sample for show smothness
            var rnd = new Random();
            var rangeX = Convert.ToInt32((Settings.Default.RightMapBound - Settings.Default.LeftMapBound) * 100000);
            var rangeY = Convert.ToInt32((Settings.Default.TopMapBound - Settings.Default.BottomMapBound) * 100000);

            var longitude1 = Convert.ToDecimal(Settings.Default.LeftMapBound + (double) rnd.Next(0, rangeX)/100000);
            var latitude1 = Convert.ToDecimal(Settings.Default.BottomMapBound + (double)rnd.Next(0, rangeY) / 100000);

            var cableDbRows = new SimpleMapDb.CablesDataTable();
            var vertexDbRows = new SimpleMapDb.VertexesDataTable();
            while (cableDbRows.Count < 200)
            {
                var cableRow = cableDbRows.NewCablesRow();

                cableRow.Longitude1 = longitude1;
                cableRow.Latitude1 = latitude1;
                cableRow.Longitude2 = Convert.ToDecimal(Settings.Default.LeftMapBound + (double)rnd.Next(0, rangeX) / 100000);
                cableRow.Latitude2 = Convert.ToDecimal(Settings.Default.BottomMapBound + (double)rnd.Next(0, rangeY) / 100000);
                var rect = new CoordinateRectangle(cableRow.Longitude1, cableRow.Latitude1, cableRow.Longitude2, cableRow.Latitude2);
                cableRow.Length = Convert.ToDecimal(rect.LineLength);
                if (cableRow.Length <= 5000 && cableRow.Length > 200)
                {
                    longitude1 = cableRow.Longitude2;
                    latitude1 = cableRow.Latitude2;
                    cableRow.Caption = rect.ToString();
                    cableDbRows.AddCablesRow(cableRow);

                    var vertexRow = vertexDbRows.NewVertexesRow();

                    vertexRow.Longitude = longitude1;
                    vertexRow.Latitude = latitude1;

                    var pt = new Coordinate(vertexRow.Longitude, vertexRow.Latitude);
                    vertexRow.Caption = pt.ToString();
                    vertexDbRows.AddVertexesRow(vertexRow);
                }
            }
            _netLayer.MergeData(vertexDbRows);
            _netLayer.MergeData(cableDbRows);
            //--end sample
        }

        private void MapCtl_Paint(object sender, PaintEventArgs e)
        {
            if (_mapLayer.Terminating) return;

            var g = e.Graphics;
            
            g.InterpolationMode = InterpolationMode.Low;
            g.CompositingQuality = CompositingQuality.HighSpeed;
            g.SmoothingMode = SmoothingMode.HighSpeed;

            DrawLayers(g, e.ClipRectangle);
        }

        public Bitmap GetMapImageForPrint(int mapWidth, int mapHeight)
        {
            var image = GraphicLayer.CreateCompatibleBitmap(null, mapWidth, mapHeight, PiFormat);
            var g = Graphics.FromImage(image);
            var r = new Rectangle(new Point(0, 0), image.Size);
            DrawLayers(g, r);
            return image;
        }

        public Bitmap GetMapImageForPrint()
        {
            return GetMapImageForPrint(Width, Height);
        }

        private void Layer_DrawBufferChanged(object sender, GraphicLayer.InvalidateLayerEventArgs e)
        {
            if (_mapLayer.Terminating) return;
            if (e == WorkerMessageThread.OwnerEventArgs.Empty)
                Invalidate();

            else
                Invalidate(e.ClipRectangle);
        }

        public void ControlClosing()
        {
            _mapLayer.Terminating = true;
            _netLayer.Terminating = true;

            _mouseMoveMap = false;
        }

        public void SetCenterMap()
        {
            if (_mapLayer.Terminating) return;

            CenterCoordinate = new Coordinate(Settings.Default.StartLongitude, Settings.Default.StartLatitude);
        }

        public void MoveCenterMapObject(decimal longitude, decimal latitude)
        {
            if (_mapLayer.Terminating) return;

            CenterCoordinate = new Coordinate(longitude, latitude);
        }

        private bool FindMapObjects(Point location, out KeyValuePair<double, int>[] vertexRows, out KeyValuePair<double, int>[] cableRows)
        {
            KeyValuePair<double, int>[] vertexR = null;
            KeyValuePair<double, int>[] cableR = null;
            try
            {
                var coordinate = Coordinate.CoordinateFromScreen(_netLayer.GoogleScreenView, location);
                var tasks = new List<Task>
                    {
                        Task.Factory.StartNew(() => vertexR = _netLayer.FindNearestVertex(coordinate)),
                        Task.Factory.StartNew(() => cableR = _netLayer.FindNearestCable(coordinate))
                    };

                Task.WaitAll(tasks.ToArray());
            }
            catch (Exception)
            {
                vertexR = null;
                cableR = null;
            }
            vertexRows = vertexR;
            cableRows = cableR;

            return (vertexRows != null && vertexRows.Length > 0) || (cableRows != null && cableRows.Length > 0);
        }

        private void MapCtl_MouseDown(object sender, MouseEventArgs e)
        {
            if (_mapLayer.Terminating) return;

            if (_mouseMoveMap) return;

            if (e.Button == MouseButtons.Left)
            {

                KeyValuePair<double, int>[] vertexRows;
                KeyValuePair<double, int>[] cableRows;

                if (!FindMapObjects(e.Location, out vertexRows, out cableRows))
                {
                    _mousePreviousLocation = new Point(e.X, e.Y);

                    if (ShiftKey)
                    {
                        MapCtl_MouseWheel(this, new MouseEventArgs(e.Button, e.Clicks, e.X, e.Y, 1));
                        CenterCoordinate = Coordinate.CoordinateFromScreen(_netLayer.GoogleScreenView, _mousePreviousLocation);
                    }
                    else
                    {
                        _mouseMoveMap = true;
                        _coordinatePreviosLocation = CenterCoordinate;
                    }
                }
                else
                {
                    if (vertexRows.Length > 0)
                    {
                        var vertex = _netLayer.GetVertex(vertexRows[0].Value);
                        if (vertex != null) MessageBox.Show(vertex.Caption, "Vertex found!");
                    }
                    else if (cableRows.Length > 0)
                    {
                        var cable = _netLayer.GetCable(cableRows[0].Value);
                        if (cable != null) MessageBox.Show(cable.Caption, "Cable found!");
                    }
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                KeyValuePair<double, int>[] vertexRows;
                KeyValuePair<double, int>[] cableRows;

                if (!FindMapObjects(e.Location, out vertexRows, out cableRows))
                {
                    _mousePreviousLocation = new Point(e.X, e.Y);

                    if (ShiftKey)
                    {
                        MapCtl_MouseWheel(this, new MouseEventArgs(e.Button, e.Clicks, e.X, e.Y, -1));
                        CenterCoordinate = Coordinate.CoordinateFromScreen(_netLayer.GoogleScreenView, _mousePreviousLocation);
                    }
                }
                else
                {
                    if (vertexRows.Length > 0)
                    {
                        _menuObjectId = vertexRows[0].Value;
                        var c = new ContextMenu();
                        c.MenuItems.Add("Delete Vertex", MapCtl_DeleteVertexClick);
                        c.Show(this, e.Location);
                    }
                    else if (cableRows.Length > 0)
                    {
                        _menuObjectId = cableRows[0].Value;
                        var c = new ContextMenu();
                        c.MenuItems.Add("Delete Cable", MapCtl_DeleteCableClick);
                        c.Show(this, e.Location);
                    }
                }
            }
        }

        private void MapCtl_DeleteVertexClick(object sender, EventArgs e)
        {
            _netLayer.RemoveVertex(_menuObjectId);
            _menuObjectId = 0;
        }

        private void MapCtl_DeleteCableClick(object sender, EventArgs e)
        {
            _netLayer.RemoveCable(_menuObjectId);
            _menuObjectId = 0;
        }

        private void MapCtl_DoubleClick(object sender, EventArgs e)
        {
            if (_mapLayer.Terminating) return;

            KeyValuePair<double, int>[] vertexRows;
            KeyValuePair<double, int>[] cableRows;

            if (!ShiftKey && !FindMapObjects(_mousePreviousLocation, out vertexRows, out cableRows))
            {
                CenterCoordinate = Coordinate.CoordinateFromScreen(_netLayer.GoogleScreenView, _mousePreviousLocation);
            }
        }

        private void MapCtl_MouseUp(object sender, MouseEventArgs e)
        {
            _mouseMoveMap = false;
            _mousePreviousLocation = new Point(e.X, e.Y);
        }

        private void MapCtl_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta != 0)
            {
                var newZoom = Level;
                newZoom += (e.Delta > 0) ? 1 : -1;
                if (newZoom < Settings.Default.MinZoomLevel)
                    newZoom = Settings.Default.MinZoomLevel;
                else if (newZoom > Settings.Default.MaxZoomLevel)
                    newZoom = Settings.Default.MaxZoomLevel;
                Level = newZoom;
            }
        }

        private void MapCtl_MouseMove(object sender, MouseEventArgs e)
        {
            if (_mapLayer.Terminating) return;

            if (_mouseMoveMap)
            {
                var deltaX = _mousePreviousLocation.X - e.X;
                var deltaY = _mousePreviousLocation.Y - e.Y;
                
                CenterCoordinate = _coordinatePreviosLocation + new GoogleCoordinate(deltaX, deltaY, Level);
            }
            else
            {
                if (String.IsNullOrEmpty(toolTip1.GetToolTip(this)))
                {
                    KeyValuePair<double, int>[] vertexRows;
                    KeyValuePair<double, int>[] cableRows;
                    FindMapObjects(e.Location, out vertexRows, out cableRows);

                    if (vertexRows.Length > 0)
                    {
                        var vertex = _netLayer.GetVertex(vertexRows[0].Value);

                        if (vertex != null)
                        {
                            toolTip1.RemoveAll();
                            toolTip1.Show(vertex.Caption, this, e.Location, 3000);
                        }
                    }
                    else if (cableRows.Length > 0)
                    {
                        var cable = _netLayer.GetCable(cableRows[0].Value);
                        if (cable != null)
                        {
                            toolTip1.RemoveAll();
                            toolTip1.Show(cable.Caption, this, e.Location, 3000);
                        }
                    }
                }
            }
        }

        private void MapCtl_KeyDown(object sender, KeyEventArgs e)
        {
            ShiftKey = e.KeyCode == Keys.ShiftKey;
        }

        private void MapCtl_KeyUp(object sender, KeyEventArgs e)
        {
            ShiftKey = false;
        }
    }
}
