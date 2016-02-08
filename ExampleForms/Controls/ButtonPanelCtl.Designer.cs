namespace ProgramMain.ExampleForms.Controls
{
    partial class ButtonPanelCtl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.labelControl1 = new System.Windows.Forms.Label();
            this.btnRefreshMap = new System.Windows.Forms.Button();
            this.btnPrint = new System.Windows.Forms.Button();
            this.zoomLevel = new System.Windows.Forms.TrackBar();
            this.btnCenterMap = new System.Windows.Forms.Button();
            this.btnCacheAllMap = new System.Windows.Forms.Button();
            this.btnSaveMapAsImage = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.zoomLevel)).BeginInit();
            this.SuspendLayout();
            // 
            // labelControl1
            // 
            this.labelControl1.Location = new System.Drawing.Point(105, 13);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(40, 13);
            this.labelControl1.TabIndex = 3;
            this.labelControl1.Text = "Zoom:";
            // 
            // btnRefreshMap
            // 
            this.btnRefreshMap.Image = global::ProgramMain.Properties.Resources.refresh_16;
            this.btnRefreshMap.Location = new System.Drawing.Point(3, 3);
            this.btnRefreshMap.Name = "btnRefreshMap";
            this.btnRefreshMap.Size = new System.Drawing.Size(33, 32);
            this.btnRefreshMap.TabIndex = 8;
            this.toolTip1.SetToolTip(this.btnRefreshMap, "Refresh Sample Tree");
            this.btnRefreshMap.UseCompatibleTextRendering = true;
            this.btnRefreshMap.UseVisualStyleBackColor = true;
            this.btnRefreshMap.Click += new System.EventHandler(this.btnRefreshMap_Click);
            // 
            // btnPrint
            // 
            this.btnPrint.Image = global::ProgramMain.Properties.Resources.print_24;
            this.btnPrint.Location = new System.Drawing.Point(42, 3);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(33, 32);
            this.btnPrint.TabIndex = 9;
            this.toolTip1.SetToolTip(this.btnPrint, "Print Map");
            this.btnPrint.UseVisualStyleBackColor = true;
            this.btnPrint.Click += new System.EventHandler(this.btnPrint_Click);
            // 
            // zoomLevel
            // 
            this.zoomLevel.LargeChange = 2;
            this.zoomLevel.Location = new System.Drawing.Point(151, 0);
            this.zoomLevel.Maximum = 20;
            this.zoomLevel.Minimum = 10;
            this.zoomLevel.Name = "zoomLevel";
            this.zoomLevel.Size = new System.Drawing.Size(226, 45);
            this.zoomLevel.TabIndex = 10;
            this.toolTip1.SetToolTip(this.zoomLevel, "Change map zoom level");
            this.zoomLevel.Value = 12;
            this.zoomLevel.ValueChanged += new System.EventHandler(this.zoomLevel_ValueChanged);
            // 
            // btnCenterMap
            // 
            this.btnCenterMap.Image = global::ProgramMain.Properties.Resources.centermap_24;
            this.btnCenterMap.Location = new System.Drawing.Point(383, 3);
            this.btnCenterMap.Name = "btnCenterMap";
            this.btnCenterMap.Size = new System.Drawing.Size(33, 32);
            this.btnCenterMap.TabIndex = 11;
            this.toolTip1.SetToolTip(this.btnCenterMap, "Center map");
            this.btnCenterMap.UseVisualStyleBackColor = true;
            this.btnCenterMap.Click += new System.EventHandler(this.btnCenterMap_Click);
            // 
            // btnCacheAllMap
            // 
            this.btnCacheAllMap.Image = global::ProgramMain.Properties.Resources.save_2_16x16;
            this.btnCacheAllMap.Location = new System.Drawing.Point(422, 3);
            this.btnCacheAllMap.Name = "btnCacheAllMap";
            this.btnCacheAllMap.Size = new System.Drawing.Size(33, 32);
            this.btnCacheAllMap.TabIndex = 12;
            this.toolTip1.SetToolTip(this.btnCacheAllMap, "Download whole map to local disk cache");
            this.btnCacheAllMap.UseVisualStyleBackColor = true;
            this.btnCacheAllMap.Click += new System.EventHandler(this.btnCacheAllMap_Click);
            // 
            // btnSaveMapAsImage
            // 
            this.btnSaveMapAsImage.Image = global::ProgramMain.Properties.Resources.program_24;
            this.btnSaveMapAsImage.Location = new System.Drawing.Point(461, 3);
            this.btnSaveMapAsImage.Name = "btnSaveMapAsImage";
            this.btnSaveMapAsImage.Size = new System.Drawing.Size(33, 32);
            this.btnSaveMapAsImage.TabIndex = 13;
            this.toolTip1.SetToolTip(this.btnSaveMapAsImage, "Save map as image to file");
            this.btnSaveMapAsImage.UseVisualStyleBackColor = true;
            this.btnSaveMapAsImage.Click += new System.EventHandler(this.btnSaveMapAsImage_Click);
            // 
            // ButtonPanelCtl
            // 
            this.Controls.Add(this.btnSaveMapAsImage);
            this.Controls.Add(this.btnCacheAllMap);
            this.Controls.Add(this.btnCenterMap);
            this.Controls.Add(this.zoomLevel);
            this.Controls.Add(this.btnPrint);
            this.Controls.Add(this.btnRefreshMap);
            this.Controls.Add(this.labelControl1);
            this.MaximumSize = new System.Drawing.Size(20000, 38);
            this.MinimumSize = new System.Drawing.Size(325, 38);
            this.Name = "ButtonPanelCtl";
            this.Size = new System.Drawing.Size(600, 38);
            this.Load += new System.EventHandler(this.FrmDesignPanel_Load);
            ((System.ComponentModel.ISupportInitialize)(this.zoomLevel)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelControl1;
        private System.Windows.Forms.Button btnCenterMap;
        private System.Windows.Forms.TrackBar zoomLevel;
        private System.Windows.Forms.Button btnPrint;
        private System.Windows.Forms.Button btnRefreshMap;
        private System.Windows.Forms.Button btnCacheAllMap;
        private System.Windows.Forms.Button btnSaveMapAsImage;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}
