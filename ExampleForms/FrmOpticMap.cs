using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ProgramMain.ExampleForms.Controls;

namespace ProgramMain.ExampleForms
{
    public partial class FrmOpticMap : Form
    {
        public FrmOpticMap()
        {
            InitializeComponent();

            Icon = Miscellaneous.DefaultIcon();
        }
        
        //protected override CreateParams CreateParams
        //{
        //    get
        //    {
        //        var handleParam = base.CreateParams;
        //        handleParam.ExStyle |= 0x02000000;   // WS_EX_COMPOSITED       
        //        return handleParam;
        //    }
        //}

        protected override void OnResizeBegin(EventArgs e)
        {
            SuspendLayout();
            base.OnResizeBegin(e);
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            ResumeLayout();
            base.OnResizeEnd(e);
        }

        private void FrmOpticMap_Load(object sender, EventArgs e)
        {
            Text = Miscellaneous.GetAssemblyTitle();
        }

        private void FrmOpticMap_FormClosing(object sender, FormClosingEventArgs e)
        {
            mapCtl1.ControlClosing();
        }

        public void RefreshForm()
        {
            mapCtl1.RefreshControl();
        }

        public void SetCenterMapObject(decimal longitude, decimal latitude)
        {
            mapCtl1.MoveCenterMapObject(longitude, latitude);
        }

        private void buttonPanelCtl1_RefreshMapClicked(object sender, EventArgs e)
        {
            RefreshForm();
        }

        private void buttonPanelCtl1_LevelValueChanged(object sender, ButtonPanelCtl.LevelValueArgs e)
        {
            mapCtl1.Level = e.Level;
        }

        private void buttonPanelCtl1_CenterMapClicked(object sender, EventArgs e)
        {
            mapCtl1.SetCenterMap();
        }

        public int Level
        {
            set { buttonPanelCtl1.Level = value; }
        }

        private void mapCtl1_LevelValueChanged(object sender, ButtonPanelCtl.LevelValueArgs e)
        {
            buttonPanelCtl1.Level = e.Level;
        }


        private void buttonPanelCtl1_PrintMapClicked(object sender, EventArgs e)
        {
            if (printDialog1.ShowDialog() == DialogResult.OK)
                printDocument1.Print();
        }

        void Document_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            e.HasMorePages = false;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
            try
            {
                var imageMap = mapCtl1.GetMapImageForPrint();
                var printSize = e.PageBounds.Size;
                var k1 = (double)imageMap.Width / printSize.Width;
                var k2 = (double)imageMap.Height / printSize.Height;
                var k = (k1 > k2) ? k1 : k2;
                var newSize = new Size((int)(imageMap.Size.Width / k), (int)(imageMap.Size.Height / k));

                var screnCenter = new Point(printSize.Width / 2, printSize.Height / 2);
                var mapCenter = new Point(newSize.Width / 2, newSize.Height / 2);
                var shift = new Size(screnCenter.X - mapCenter.X, screnCenter.Y - mapCenter.Y);
                var p = new Point(0, 0) + shift;

                var rectangle = new Rectangle(p, newSize);
                e.Graphics.DrawImage(imageMap, rectangle);
            }
            catch (Exception ex)
            {
                //do nothing
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }
        }

        private void buttonPanelCtl1_SaveAllMapClicked(object sender, EventArgs e)
        {
            FrmMapDownloader.DownloadMap();
        }

        private void buttonPanelCtl1_SaveMapAsImageClicked(object sender, EventArgs e)
        {
            FrmMapDownloader.SaveMapAsImage(mapCtl1.PiFormat, mapCtl1.Level);
        }
    }
}
