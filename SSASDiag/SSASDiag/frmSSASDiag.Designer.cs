namespace SSASDiag
{
    partial class frmSSASDiag
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSSASDiag));
            this.cbInstances = new System.Windows.Forms.ComboBox();
            this.lblInstanceDetails = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnCapture = new System.Windows.Forms.Button();
            this.lbStatus = new System.Windows.Forms.ListBox();
            this.chkRollover = new System.Windows.Forms.CheckBox();
            this.chkAutoRestart = new System.Windows.Forms.CheckBox();
            this.udRollover = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.udInterval = new System.Windows.Forms.NumericUpDown();
            this.lblInterval = new System.Windows.Forms.Label();
            this.lblInterval2 = new System.Windows.Forms.Label();
            this.chkStopTime = new System.Windows.Forms.CheckBox();
            this.dtStopTime = new System.Windows.Forms.DateTimePicker();
            this.lblRightClick = new System.Windows.Forms.Label();
            this.ttStatus = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.udRollover)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udInterval)).BeginInit();
            this.SuspendLayout();
            // 
            // cbInstances
            // 
            this.cbInstances.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbInstances.FormattingEnabled = true;
            this.cbInstances.Location = new System.Drawing.Point(12, 26);
            this.cbInstances.Name = "cbInstances";
            this.cbInstances.Size = new System.Drawing.Size(232, 21);
            this.cbInstances.TabIndex = 0;
            this.cbInstances.SelectedIndexChanged += new System.EventHandler(this.cbInstances_SelectedIndexChanged);
            // 
            // lblInstanceDetails
            // 
            this.lblInstanceDetails.AutoSize = true;
            this.lblInstanceDetails.Location = new System.Drawing.Point(275, 23);
            this.lblInstanceDetails.Name = "lblInstanceDetails";
            this.lblInstanceDetails.Size = new System.Drawing.Size(0, 13);
            this.lblInstanceDetails.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(238, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Select an SSAS instance from the local machine:";
            // 
            // btnCapture
            // 
            this.btnCapture.Enabled = false;
            this.btnCapture.Location = new System.Drawing.Point(384, 75);
            this.btnCapture.Name = "btnCapture";
            this.btnCapture.Size = new System.Drawing.Size(128, 46);
            this.btnCapture.TabIndex = 3;
            this.btnCapture.Text = "Start Capture";
            this.btnCapture.UseVisualStyleBackColor = true;
            this.btnCapture.Click += new System.EventHandler(this.btnCapture_Click);
            // 
            // lbStatus
            // 
            this.lbStatus.BackColor = System.Drawing.SystemColors.InfoText;
            this.lbStatus.ForeColor = System.Drawing.Color.YellowGreen;
            this.lbStatus.FormattingEnabled = true;
            this.lbStatus.Location = new System.Drawing.Point(12, 150);
            this.lbStatus.Name = "lbStatus";
            this.lbStatus.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lbStatus.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.lbStatus.Size = new System.Drawing.Size(559, 160);
            this.lbStatus.TabIndex = 4;
            this.lbStatus.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lbStatus_MouseClick);
            // 
            // chkRollover
            // 
            this.chkRollover.AutoSize = true;
            this.chkRollover.Location = new System.Drawing.Point(12, 53);
            this.chkRollover.Name = "chkRollover";
            this.chkRollover.Size = new System.Drawing.Size(115, 17);
            this.chkRollover.TabIndex = 5;
            this.chkRollover.Text = "Rollover log files at";
            this.chkRollover.UseVisualStyleBackColor = true;
            this.chkRollover.CheckedChanged += new System.EventHandler(this.chkRollover_CheckedChanged_1);
            // 
            // chkAutoRestart
            // 
            this.chkAutoRestart.AutoSize = true;
            this.chkAutoRestart.Location = new System.Drawing.Point(12, 76);
            this.chkAutoRestart.Name = "chkAutoRestart";
            this.chkAutoRestart.Size = new System.Drawing.Size(227, 17);
            this.chkAutoRestart.TabIndex = 6;
            this.chkAutoRestart.Text = "Restart profiler trace when service restarts.";
            this.chkAutoRestart.UseVisualStyleBackColor = true;
            this.chkAutoRestart.CheckedChanged += new System.EventHandler(this.chkAutoRestart_CheckedChanged);
            // 
            // udRollover
            // 
            this.udRollover.Enabled = false;
            this.udRollover.Location = new System.Drawing.Point(124, 52);
            this.udRollover.Maximum = new decimal(new int[] {
            2048,
            0,
            0,
            0});
            this.udRollover.Minimum = new decimal(new int[] {
            16,
            0,
            0,
            0});
            this.udRollover.Name = "udRollover";
            this.udRollover.Size = new System.Drawing.Size(54, 20);
            this.udRollover.TabIndex = 7;
            this.udRollover.TabStop = false;
            this.udRollover.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.udRollover.ThousandsSeparator = true;
            this.udRollover.UpDownAlign = System.Windows.Forms.LeftRightAlignment.Left;
            this.udRollover.Value = new decimal(new int[] {
            512,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(177, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "MB.";
            // 
            // udInterval
            // 
            this.udInterval.Location = new System.Drawing.Point(133, 122);
            this.udInterval.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.udInterval.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.udInterval.Name = "udInterval";
            this.udInterval.Size = new System.Drawing.Size(45, 20);
            this.udInterval.TabIndex = 9;
            this.udInterval.TabStop = false;
            this.udInterval.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.udInterval.ThousandsSeparator = true;
            this.udInterval.UpDownAlign = System.Windows.Forms.LeftRightAlignment.Left;
            this.udInterval.Value = new decimal(new int[] {
            15,
            0,
            0,
            0});
            // 
            // lblInterval
            // 
            this.lblInterval.AutoSize = true;
            this.lblInterval.Location = new System.Drawing.Point(9, 124);
            this.lblInterval.Name = "lblInterval";
            this.lblInterval.Size = new System.Drawing.Size(124, 13);
            this.lblInterval.TabIndex = 10;
            this.lblInterval.Text = "Performance log interval:";
            // 
            // lblInterval2
            // 
            this.lblInterval2.AutoSize = true;
            this.lblInterval2.Location = new System.Drawing.Point(178, 124);
            this.lblInterval2.Name = "lblInterval2";
            this.lblInterval2.Size = new System.Drawing.Size(50, 13);
            this.lblInterval2.TabIndex = 11;
            this.lblInterval2.Text = "seconds.";
            // 
            // chkStopTime
            // 
            this.chkStopTime.AutoSize = true;
            this.chkStopTime.Location = new System.Drawing.Point(12, 99);
            this.chkStopTime.Name = "chkStopTime";
            this.chkStopTime.Size = new System.Drawing.Size(73, 17);
            this.chkStopTime.TabIndex = 12;
            this.chkStopTime.Text = "Stop time:";
            this.chkStopTime.UseVisualStyleBackColor = true;
            this.chkStopTime.CheckedChanged += new System.EventHandler(this.chkStopTime_CheckedChanged);
            // 
            // dtStopTime
            // 
            this.dtStopTime.CustomFormat = "MM/dd/yyyy HH:mm:ss UTC";
            this.dtStopTime.Enabled = false;
            this.dtStopTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtStopTime.Location = new System.Drawing.Point(86, 96);
            this.dtStopTime.Name = "dtStopTime";
            this.dtStopTime.Size = new System.Drawing.Size(180, 20);
            this.dtStopTime.TabIndex = 13;
            // 
            // lblRightClick
            // 
            this.lblRightClick.AutoSize = true;
            this.lblRightClick.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRightClick.Location = new System.Drawing.Point(390, 313);
            this.lblRightClick.Name = "lblRightClick";
            this.lblRightClick.Size = new System.Drawing.Size(181, 12);
            this.lblRightClick.TabIndex = 14;
            this.lblRightClick.Text = "*Right click status box to copy to clipboard.";
            // 
            // frmSSASDiag
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(583, 326);
            this.Controls.Add(this.lblRightClick);
            this.Controls.Add(this.dtStopTime);
            this.Controls.Add(this.chkStopTime);
            this.Controls.Add(this.lblInterval2);
            this.Controls.Add(this.lblInterval);
            this.Controls.Add(this.udInterval);
            this.Controls.Add(this.udRollover);
            this.Controls.Add(this.chkAutoRestart);
            this.Controls.Add(this.chkRollover);
            this.Controls.Add(this.lbStatus);
            this.Controls.Add(this.btnCapture);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblInstanceDetails);
            this.Controls.Add(this.cbInstances);
            this.Controls.Add(this.label2);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmSSASDiag";
            this.Text = "SSAS Diagnostics Collector";
            this.Load += new System.EventHandler(this.frmSSASDiag_Load);
            ((System.ComponentModel.ISupportInitialize)(this.udRollover)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udInterval)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbInstances;
        private System.Windows.Forms.Label lblInstanceDetails;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnCapture;
        private System.Windows.Forms.ListBox lbStatus;
        private System.Windows.Forms.CheckBox chkRollover;
        private System.Windows.Forms.CheckBox chkAutoRestart;
        private System.Windows.Forms.NumericUpDown udRollover;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown udInterval;
        private System.Windows.Forms.Label lblInterval;
        private System.Windows.Forms.Label lblInterval2;
        private System.Windows.Forms.CheckBox chkStopTime;
        private System.Windows.Forms.DateTimePicker dtStopTime;
        private System.Windows.Forms.Label lblRightClick;
        private System.Windows.Forms.ToolTip ttStatus;
    }
}

