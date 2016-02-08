using ProgramMain.ExampleForms.Controls;

namespace ProgramMain.ExampleForms
{
    partial class FrmOpticMap
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.printDocument1 = new System.Drawing.Printing.PrintDocument();
            this.printDialog1 = new System.Windows.Forms.PrintDialog();
            this.mapCtl1 = new MapCtl();
            this.buttonPanelCtl1 = new ButtonPanelCtl();
            this.SuspendLayout();
            // 
            // printDocument1
            // 
            this.printDocument1.DocumentName = "Simple Map";
            this.printDocument1.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(this.Document_PrintPage);
            // 
            // printDialog1
            // 
            this.printDialog1.Document = this.printDocument1;
            this.printDialog1.UseEXDialog = true;
            // 
            // mapCtl1
            // 
            this.mapCtl1.AllowDrop = true;
            this.mapCtl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mapCtl1.Level = 13;
            this.mapCtl1.Location = new System.Drawing.Point(0, 38);
            this.mapCtl1.Name = "mapCtl1";
            this.mapCtl1.Size = new System.Drawing.Size(680, 426);
            this.mapCtl1.TabIndex = 0;
            this.mapCtl1.LevelValueChanged += new System.EventHandler<ButtonPanelCtl.LevelValueArgs>(this.mapCtl1_LevelValueChanged);
            // 
            // buttonPanelCtl1
            // 
            this.buttonPanelCtl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.buttonPanelCtl1.Level = 12;
            this.buttonPanelCtl1.Location = new System.Drawing.Point(0, 0);
            this.buttonPanelCtl1.MaximumSize = new System.Drawing.Size(2000, 38);
            this.buttonPanelCtl1.MinimumSize = new System.Drawing.Size(350, 38);
            this.buttonPanelCtl1.Name = "buttonPanelCtl1";
            this.buttonPanelCtl1.Size = new System.Drawing.Size(680, 38);
            this.buttonPanelCtl1.TabIndex = 1;
            this.buttonPanelCtl1.LevelValueChanged += new System.EventHandler<ButtonPanelCtl.LevelValueArgs>(this.buttonPanelCtl1_LevelValueChanged);
            this.buttonPanelCtl1.CenterMapClicked += new System.EventHandler(this.buttonPanelCtl1_CenterMapClicked);
            this.buttonPanelCtl1.PrintMapClicked += new System.EventHandler(this.buttonPanelCtl1_PrintMapClicked);
            this.buttonPanelCtl1.RefreshMapClicked += new System.EventHandler(this.buttonPanelCtl1_RefreshMapClicked);
            this.buttonPanelCtl1.SaveAllMapClicked += new System.EventHandler(this.buttonPanelCtl1_SaveAllMapClicked);
            this.buttonPanelCtl1.SaveMapAsImageClicked += new System.EventHandler(this.buttonPanelCtl1_SaveMapAsImageClicked);
            // 
            // FrmOpticMap
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(680, 464);
            this.Controls.Add(this.mapCtl1);
            this.Controls.Add(this.buttonPanelCtl1);
            this.DoubleBuffered = true;
            this.Name = "FrmOpticMap";
            this.Text = "Map";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmOpticMap_FormClosing);
            this.Load += new System.EventHandler(this.FrmOpticMap_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private MapCtl mapCtl1;
        private ButtonPanelCtl buttonPanelCtl1;
        private System.Drawing.Printing.PrintDocument printDocument1;
        private System.Windows.Forms.PrintDialog printDialog1;
    }
}