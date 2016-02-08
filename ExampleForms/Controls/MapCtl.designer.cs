namespace ProgramMain.ExampleForms.Controls
{
    partial class MapCtl
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
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // MapCtl
            // 
            this.Name = "MapCtl";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.MapCtl_Paint);
            this.DoubleClick += new System.EventHandler(this.MapCtl_DoubleClick);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MapCtl_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.MapCtl_KeyUp);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MapCtl_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MapCtl_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MapCtl_MouseUp);
            this.Resize += new System.EventHandler(this.MapCtl_Resize);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip1;



    }
}
