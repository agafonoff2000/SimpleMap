namespace ProgramMain.ExampleForms
{
    partial class FrmMapDownloader
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
            this.labelInfo = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.labProceed = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.labTotal = new System.Windows.Forms.Label();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.SuspendLayout();
            // 
            // labelInfo
            // 
            this.labelInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelInfo.Location = new System.Drawing.Point(12, 13);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(487, 23);
            this.labelInfo.TabIndex = 0;
            this.labelInfo.Text = "Cache all map on local disk";
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(12, 39);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(487, 23);
            this.progressBar1.TabIndex = 1;
            this.progressBar1.Value = 20;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(423, 102);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 69);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(120, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Proceed Block Number:";
            // 
            // labProceed
            // 
            this.labProceed.AutoSize = true;
            this.labProceed.Location = new System.Drawing.Point(139, 69);
            this.labProceed.Name = "labProceed";
            this.labProceed.Size = new System.Drawing.Size(14, 13);
            this.labProceed.TabIndex = 4;
            this.labProceed.Text = "~";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 94);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(69, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Total Blocks:";
            // 
            // labTotal
            // 
            this.labTotal.AutoSize = true;
            this.labTotal.Location = new System.Drawing.Point(139, 94);
            this.labTotal.Name = "labTotal";
            this.labTotal.Size = new System.Drawing.Size(14, 13);
            this.labTotal.TabIndex = 6;
            this.labTotal.Text = "~";
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.DefaultExt = "PNG file (*.png)|*.png";
            this.saveFileDialog1.FileName = "SimpleMap.png";
            // 
            // FrmMapDownloader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(511, 143);
            this.Controls.Add(this.labTotal);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.labProceed);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.labelInfo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmMapDownloader";
            this.Text = "Map Downloader";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmMapDownloader_FormClosing);
            this.Load += new System.EventHandler(this.FrmMapDownloader_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label labProceed;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label labTotal;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
    }
}