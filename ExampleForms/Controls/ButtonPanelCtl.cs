using System;
using System.Windows.Forms;

namespace ProgramMain.ExampleForms.Controls
{
    public partial class ButtonPanelCtl : UserControl
    {
        public ButtonPanelCtl()
        {
            InitializeComponent();
        }

        public int Level
        {
            get
            {
                return zoomLevel.Value;
            }
            set
            {
                zoomLevel.Value = value;
            }
        }

        public class LevelValueArgs : EventArgs
        {
            public int Level { get; private set; }

            public LevelValueArgs(int level)
            {
                Level = level;
            }
        }
        public event EventHandler<LevelValueArgs> LevelValueChanged;

        public event EventHandler CenterMapClicked;

        public event EventHandler PrintMapClicked;

        public event EventHandler RefreshMapClicked;

        public event EventHandler SaveAllMapClicked;

        public event EventHandler SaveMapAsImageClicked;

        private void FrmDesignPanel_Load(object sender, EventArgs e)
        {
            zoomLevel.Maximum = Properties.Settings.Default.MaxZoomLevel;
            zoomLevel.Minimum = Properties.Settings.Default.MinZoomLevel;
            zoomLevel.Value = Properties.Settings.Default.StartZoomLevel;
        }

        private void zoomLevel_ValueChanged(object sender, EventArgs e)
        {
            if (LevelValueChanged != null && zoomLevel.Value >= Properties.Settings.Default.MinZoomLevel
                && zoomLevel.Value <= Properties.Settings.Default.MaxZoomLevel)
            {
                LevelValueChanged(this, new LevelValueArgs(zoomLevel.Value));
            }
        }

        private void btnCenterMap_Click(object sender, EventArgs e)
        {
            if (CenterMapClicked != null) CenterMapClicked(this, e);
        }

        private void btnRefreshMap_Click(object sender, EventArgs e)
        {
            if (RefreshMapClicked != null) RefreshMapClicked(this, e);
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            if (PrintMapClicked != null) PrintMapClicked(this, EventArgs.Empty);
        }

        private void btnCacheAllMap_Click(object sender, EventArgs  e)
        {
            if (SaveAllMapClicked != null) SaveAllMapClicked(this, EventArgs.Empty);
        }

        private void btnSaveMapAsImage_Click(object sender, EventArgs e)
        {
            if (SaveMapAsImageClicked != null) SaveMapAsImageClicked(this, EventArgs.Empty);
        }
    }
}