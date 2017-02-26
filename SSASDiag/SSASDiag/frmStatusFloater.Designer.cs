namespace SSASDiag
{
    partial class frmStatusFloater
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
            pictureBox1 = new System.Windows.Forms.PictureBox();
            lblStatus = new System.Windows.Forms.Label();
            lblTime = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(pictureBox1)).BeginInit();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.Image = global::SSASDiag.Properties.Resources.Progress;
            pictureBox1.Location = new System.Drawing.Point(0, 0);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new System.Drawing.Size(80, 80);
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            lblStatus.ForeColor = System.Drawing.Color.White;
            lblStatus.Location = new System.Drawing.Point(99, 29);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new System.Drawing.Size(156, 20);
            lblStatus.TabIndex = 1;
            lblStatus.Text = "Status text goes here...";
            lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblTime
            // 
            lblTime.AutoSize = true;
            lblTime.BackColor = System.Drawing.Color.Transparent;
            lblTime.Font = new System.Drawing.Font("Segoe UI", 7.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            lblTime.ForeColor = System.Drawing.Color.LightSteelBlue;
            lblTime.Location = new System.Drawing.Point(28, 34);
            lblTime.Name = "lblTime";
            lblTime.Size = new System.Drawing.Size(27, 12);
            lblTime.TabIndex = 2;
            lblTime.Text = "00:00";
            lblTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lblTime.Visible = false;
            // 
            // frmStatusFloater
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.SystemColors.ControlDarkDark;
            ClientSize = new System.Drawing.Size(502, 80);
            Controls.Add(lblStatus);
            Controls.Add(lblTime);
            Controls.Add(pictureBox1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            Name = "frmStatusFloater";
            Opacity = 0.9D;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            Text = "frmStatusFloater";
            KeyPress += new System.Windows.Forms.KeyPressEventHandler(frmStatusFloater_KeyPress);
            ((System.ComponentModel.ISupportInitialize)(pictureBox1)).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        public System.Windows.Forms.Label lblStatus;
        public System.Windows.Forms.Label lblTime;
    }
}