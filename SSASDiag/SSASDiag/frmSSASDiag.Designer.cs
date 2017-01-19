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
            this.chkStartTime = new System.Windows.Forms.CheckBox();
            this.dtStartTime = new System.Windows.Forms.DateTimePicker();
            this.lblRightClick = new System.Windows.Forms.Label();
            this.ttStatus = new System.Windows.Forms.ToolTip(this.components);
            this.lkFeedback = new System.Windows.Forms.LinkLabel();
            this.lkBugs = new System.Windows.Forms.LinkLabel();
            this.lkDiscussion = new System.Windows.Forms.LinkLabel();
            this.timerPerfMon = new System.Windows.Forms.Timer(this.components);
            this.dtStopTime = new System.Windows.Forms.DateTimePicker();
            this.chkStopTime = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chkGetNetwork = new System.Windows.Forms.CheckBox();
            this.chkGetProfiler = new System.Windows.Forms.CheckBox();
            this.chkGetPerfMon = new System.Windows.Forms.CheckBox();
            this.chkGetConfigDetails = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.udRollover)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udInterval)).BeginInit();
            this.groupBox1.SuspendLayout();
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
            this.lblInstanceDetails.Location = new System.Drawing.Point(267, 23);
            this.lblInstanceDetails.Name = "lblInstanceDetails";
            this.lblInstanceDetails.Size = new System.Drawing.Size(0, 13);
            this.lblInstanceDetails.TabIndex = 22;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(238, 13);
            this.label1.TabIndex = 21;
            this.label1.Text = "Select an SSAS instance from the local machine:";
            // 
            // btnCapture
            // 
            this.btnCapture.BackColor = System.Drawing.Color.Transparent;
            this.btnCapture.Enabled = false;
            this.btnCapture.FlatAppearance.BorderSize = 0;
            this.btnCapture.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btnCapture.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btnCapture.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCapture.Image = global::SSASDiag.Properties.Resources.play;
            this.btnCapture.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnCapture.Location = new System.Drawing.Point(270, 65);
            this.btnCapture.Name = "btnCapture";
            this.btnCapture.Size = new System.Drawing.Size(103, 88);
            this.btnCapture.TabIndex = 14;
            this.btnCapture.Text = "Start Capture";
            this.btnCapture.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnCapture.UseVisualStyleBackColor = false;
            this.btnCapture.Click += new System.EventHandler(this.btnCapture_Click);
            // 
            // lbStatus
            // 
            this.lbStatus.BackColor = System.Drawing.SystemColors.InfoText;
            this.lbStatus.ForeColor = System.Drawing.Color.LightSkyBlue;
            this.lbStatus.FormattingEnabled = true;
            this.lbStatus.HorizontalScrollbar = true;
            this.lbStatus.Location = new System.Drawing.Point(12, 197);
            this.lbStatus.Name = "lbStatus";
            this.lbStatus.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lbStatus.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.lbStatus.Size = new System.Drawing.Size(577, 160);
            this.lbStatus.TabIndex = 15;
            this.lbStatus.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lbStatus_MouseClick);
            // 
            // chkRollover
            // 
            this.chkRollover.AutoSize = true;
            this.chkRollover.Location = new System.Drawing.Point(12, 65);
            this.chkRollover.Name = "chkRollover";
            this.chkRollover.Size = new System.Drawing.Size(115, 17);
            this.chkRollover.TabIndex = 1;
            this.chkRollover.Text = "Rollover log files at";
            this.chkRollover.UseVisualStyleBackColor = true;
            this.chkRollover.CheckedChanged += new System.EventHandler(this.chkRollover_CheckedChanged);
            // 
            // chkAutoRestart
            // 
            this.chkAutoRestart.AutoSize = true;
            this.chkAutoRestart.Location = new System.Drawing.Point(12, 133);
            this.chkAutoRestart.Name = "chkAutoRestart";
            this.chkAutoRestart.Size = new System.Drawing.Size(206, 17);
            this.chkAutoRestart.TabIndex = 7;
            this.chkAutoRestart.Text = "Restart profiler trace if service restarts.";
            this.chkAutoRestart.UseVisualStyleBackColor = true;
            this.chkAutoRestart.CheckedChanged += new System.EventHandler(this.chkAutoRestart_CheckedChanged);
            // 
            // udRollover
            // 
            this.udRollover.Enabled = false;
            this.udRollover.Location = new System.Drawing.Point(124, 64);
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
            this.udRollover.TabIndex = 2;
            this.udRollover.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.udRollover.ThousandsSeparator = true;
            this.udRollover.UpDownAlign = System.Windows.Forms.LeftRightAlignment.Left;
            this.udRollover.Value = new decimal(new int[] {
            128,
            0,
            0,
            0});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(177, 66);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 23;
            this.label2.Text = "MB.";
            // 
            // udInterval
            // 
            this.udInterval.Location = new System.Drawing.Point(152, 154);
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
            this.udInterval.TabIndex = 8;
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
            this.lblInterval.Location = new System.Drawing.Point(28, 156);
            this.lblInterval.Name = "lblInterval";
            this.lblInterval.Size = new System.Drawing.Size(124, 13);
            this.lblInterval.TabIndex = 19;
            this.lblInterval.Text = "Performance log interval:";
            // 
            // lblInterval2
            // 
            this.lblInterval2.AutoSize = true;
            this.lblInterval2.Location = new System.Drawing.Point(197, 156);
            this.lblInterval2.Name = "lblInterval2";
            this.lblInterval2.Size = new System.Drawing.Size(50, 13);
            this.lblInterval2.TabIndex = 18;
            this.lblInterval2.Text = "seconds.";
            // 
            // chkStartTime
            // 
            this.chkStartTime.AutoSize = true;
            this.chkStartTime.Location = new System.Drawing.Point(12, 88);
            this.chkStartTime.Name = "chkStartTime";
            this.chkStartTime.Size = new System.Drawing.Size(73, 17);
            this.chkStartTime.TabIndex = 3;
            this.chkStartTime.Text = "Start time:";
            this.chkStartTime.UseVisualStyleBackColor = true;
            this.chkStartTime.CheckedChanged += new System.EventHandler(this.chkStartTime_CheckedChanged);
            // 
            // dtStartTime
            // 
            this.dtStartTime.CustomFormat = "MM/dd/yyyy HH:mm:ss UTC";
            this.dtStartTime.Enabled = false;
            this.dtStartTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtStartTime.Location = new System.Drawing.Point(83, 86);
            this.dtStartTime.Name = "dtStartTime";
            this.dtStartTime.Size = new System.Drawing.Size(167, 20);
            this.dtStartTime.TabIndex = 4;
            // 
            // lblRightClick
            // 
            this.lblRightClick.AutoSize = true;
            this.lblRightClick.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRightClick.ForeColor = System.Drawing.SystemColors.Highlight;
            this.lblRightClick.Location = new System.Drawing.Point(360, 360);
            this.lblRightClick.Name = "lblRightClick";
            this.lblRightClick.Size = new System.Drawing.Size(229, 13);
            this.lblRightClick.TabIndex = 17;
            this.lblRightClick.Text = "*Right click the output area to copy to clipboard.";
            // 
            // lkFeedback
            // 
            this.lkFeedback.AutoSize = true;
            this.lkFeedback.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lkFeedback.ForeColor = System.Drawing.SystemColors.Highlight;
            this.lkFeedback.Image = ((System.Drawing.Image)(resources.GetObject("lkFeedback.Image")));
            this.lkFeedback.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lkFeedback.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.lkFeedback.Location = new System.Drawing.Point(12, 360);
            this.lkFeedback.Name = "lkFeedback";
            this.lkFeedback.Padding = new System.Windows.Forms.Padding(2);
            this.lkFeedback.Size = new System.Drawing.Size(77, 17);
            this.lkFeedback.TabIndex = 16;
            this.lkFeedback.TabStop = true;
            this.lkFeedback.Text = "      Feedback";
            this.ttStatus.SetToolTip(this.lkFeedback, "Share the love!");
            this.lkFeedback.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lkFeedback_LinkClicked);
            // 
            // lkBugs
            // 
            this.lkBugs.AutoSize = true;
            this.lkBugs.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lkBugs.ForeColor = System.Drawing.SystemColors.Highlight;
            this.lkBugs.Image = ((System.Drawing.Image)(resources.GetObject("lkBugs.Image")));
            this.lkBugs.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lkBugs.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.lkBugs.Location = new System.Drawing.Point(95, 360);
            this.lkBugs.Name = "lkBugs";
            this.lkBugs.Padding = new System.Windows.Forms.Padding(2);
            this.lkBugs.Size = new System.Drawing.Size(53, 17);
            this.lkBugs.TabIndex = 17;
            this.lkBugs.TabStop = true;
            this.lkBugs.Text = "      Bugs";
            this.ttStatus.SetToolTip(this.lkBugs, "Fix it!");
            this.lkBugs.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lkBugs_LinkClicked);
            // 
            // lkDiscussion
            // 
            this.lkDiscussion.AutoSize = true;
            this.lkDiscussion.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lkDiscussion.ForeColor = System.Drawing.SystemColors.Highlight;
            this.lkDiscussion.Image = ((System.Drawing.Image)(resources.GetObject("lkDiscussion.Image")));
            this.lkDiscussion.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lkDiscussion.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.lkDiscussion.Location = new System.Drawing.Point(154, 360);
            this.lkDiscussion.Name = "lkDiscussion";
            this.lkDiscussion.Padding = new System.Windows.Forms.Padding(2);
            this.lkDiscussion.Size = new System.Drawing.Size(111, 17);
            this.lkDiscussion.TabIndex = 18;
            this.lkDiscussion.TabStop = true;
            this.lkDiscussion.Text = "      Discussion/Ideas";
            this.ttStatus.SetToolTip(this.lkDiscussion, "Make it better!");
            this.lkDiscussion.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lkDiscussion_LinkClicked);
            // 
            // timerPerfMon
            // 
            this.timerPerfMon.Interval = 1000;
            this.timerPerfMon.Tag = "0";
            this.timerPerfMon.Tick += new System.EventHandler(this.timerPerfMon_Tick);
            // 
            // dtStopTime
            // 
            this.dtStopTime.CustomFormat = "MM/dd/yyyy HH:mm:ss UTC";
            this.dtStopTime.Enabled = false;
            this.dtStopTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtStopTime.Location = new System.Drawing.Point(83, 109);
            this.dtStopTime.Name = "dtStopTime";
            this.dtStopTime.Size = new System.Drawing.Size(167, 20);
            this.dtStopTime.TabIndex = 6;
            // 
            // chkStopTime
            // 
            this.chkStopTime.AutoSize = true;
            this.chkStopTime.Location = new System.Drawing.Point(12, 111);
            this.chkStopTime.Name = "chkStopTime";
            this.chkStopTime.Size = new System.Drawing.Size(73, 17);
            this.chkStopTime.TabIndex = 5;
            this.chkStopTime.Text = "Stop time:";
            this.chkStopTime.UseVisualStyleBackColor = true;
            this.chkStopTime.CheckedChanged += new System.EventHandler(this.chkStopTime_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.chkGetNetwork);
            this.groupBox1.Controls.Add(this.chkGetProfiler);
            this.groupBox1.Controls.Add(this.chkGetPerfMon);
            this.groupBox1.Controls.Add(this.chkGetConfigDetails);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(395, 66);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(194, 116);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Diagnostics to Capture:";
            // 
            // chkGetNetwork
            // 
            this.chkGetNetwork.AutoSize = true;
            this.chkGetNetwork.Enabled = false;
            this.chkGetNetwork.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkGetNetwork.Location = new System.Drawing.Point(14, 90);
            this.chkGetNetwork.Name = "chkGetNetwork";
            this.chkGetNetwork.Size = new System.Drawing.Size(102, 17);
            this.chkGetNetwork.TabIndex = 13;
            this.chkGetNetwork.Text = "Network Traces";
            this.chkGetNetwork.UseVisualStyleBackColor = true;
            this.chkGetNetwork.CheckedChanged += new System.EventHandler(this.chkGetNetwork_CheckedChanged);
            // 
            // chkGetProfiler
            // 
            this.chkGetProfiler.AutoSize = true;
            this.chkGetProfiler.Checked = true;
            this.chkGetProfiler.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkGetProfiler.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkGetProfiler.Location = new System.Drawing.Point(14, 67);
            this.chkGetProfiler.Name = "chkGetProfiler";
            this.chkGetProfiler.Size = new System.Drawing.Size(118, 17);
            this.chkGetProfiler.TabIndex = 12;
            this.chkGetProfiler.Text = "SQL Profiler Traces";
            this.chkGetProfiler.UseVisualStyleBackColor = true;
            this.chkGetProfiler.CheckedChanged += new System.EventHandler(this.chkGetProfiler_CheckedChanged);
            // 
            // chkGetPerfMon
            // 
            this.chkGetPerfMon.AutoSize = true;
            this.chkGetPerfMon.Checked = true;
            this.chkGetPerfMon.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkGetPerfMon.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkGetPerfMon.Location = new System.Drawing.Point(14, 44);
            this.chkGetPerfMon.Name = "chkGetPerfMon";
            this.chkGetPerfMon.Size = new System.Drawing.Size(150, 17);
            this.chkGetPerfMon.TabIndex = 11;
            this.chkGetPerfMon.Text = "Performance Monitor Logs";
            this.chkGetPerfMon.UseVisualStyleBackColor = true;
            this.chkGetPerfMon.CheckedChanged += new System.EventHandler(this.chkGetPerfMon_CheckedChanged);
            // 
            // chkGetConfigDetails
            // 
            this.chkGetConfigDetails.AutoSize = true;
            this.chkGetConfigDetails.Checked = true;
            this.chkGetConfigDetails.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkGetConfigDetails.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkGetConfigDetails.Location = new System.Drawing.Point(14, 22);
            this.chkGetConfigDetails.Name = "chkGetConfigDetails";
            this.chkGetConfigDetails.Size = new System.Drawing.Size(179, 17);
            this.chkGetConfigDetails.TabIndex = 10;
            this.chkGetConfigDetails.Text = "Instance Configuration and Logs";
            this.chkGetConfigDetails.UseVisualStyleBackColor = true;
            // 
            // frmSSASDiag
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(601, 388);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.lkDiscussion);
            this.Controls.Add(this.lkBugs);
            this.Controls.Add(this.lkFeedback);
            this.Controls.Add(this.dtStopTime);
            this.Controls.Add(this.chkStopTime);
            this.Controls.Add(this.lblRightClick);
            this.Controls.Add(this.dtStartTime);
            this.Controls.Add(this.chkStartTime);
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
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(599, 350);
            this.Name = "frmSSASDiag";
            this.Text = "SSAS Diagnostics Collector";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmSSASDiag_FormClosing);
            this.Load += new System.EventHandler(this.frmSSASDiag_Load);
            this.Resize += new System.EventHandler(this.frmSSASDiag_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.udRollover)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udInterval)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
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
        private System.Windows.Forms.CheckBox chkStartTime;
        private System.Windows.Forms.DateTimePicker dtStartTime;
        private System.Windows.Forms.Label lblRightClick;
        private System.Windows.Forms.ToolTip ttStatus;
        private System.Windows.Forms.Timer timerPerfMon;
        private System.Windows.Forms.DateTimePicker dtStopTime;
        private System.Windows.Forms.CheckBox chkStopTime;
        private System.Windows.Forms.LinkLabel lkFeedback;
        private System.Windows.Forms.LinkLabel lkBugs;
        private System.Windows.Forms.LinkLabel lkDiscussion;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox chkGetNetwork;
        private System.Windows.Forms.CheckBox chkGetProfiler;
        private System.Windows.Forms.CheckBox chkGetPerfMon;
        private System.Windows.Forms.CheckBox chkGetConfigDetails;
    }
}

