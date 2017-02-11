﻿namespace SSASDiag
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
            this.chkRollover = new System.Windows.Forms.CheckBox();
            this.chkAutoRestart = new System.Windows.Forms.CheckBox();
            this.udRollover = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.udInterval = new System.Windows.Forms.NumericUpDown();
            this.lblInterval = new System.Windows.Forms.Label();
            this.lblInterval2 = new System.Windows.Forms.Label();
            this.chkStartTime = new System.Windows.Forms.CheckBox();
            this.dtStartTime = new System.Windows.Forms.DateTimePicker();
            this.ttStatus = new System.Windows.Forms.ToolTip(this.components);
            this.lkFeedback = new System.Windows.Forms.LinkLabel();
            this.lkBugs = new System.Windows.Forms.LinkLabel();
            this.lkDiscussion = new System.Windows.Forms.LinkLabel();
            this.chkBAK = new System.Windows.Forms.CheckBox();
            this.chkABF = new System.Windows.Forms.CheckBox();
            this.chkProfilerPerfDetails = new System.Windows.Forms.CheckBox();
            this.chkGetNetwork = new System.Windows.Forms.CheckBox();
            this.chkXMLA = new System.Windows.Forms.CheckBox();
            this.chkGetConfigDetails = new System.Windows.Forms.CheckBox();
            this.lblBAK = new System.Windows.Forms.Label();
            this.lblABF = new System.Windows.Forms.Label();
            this.lblXMLA = new System.Windows.Forms.Label();
            this.dtStopTime = new System.Windows.Forms.DateTimePicker();
            this.chkStopTime = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabSimple = new System.Windows.Forms.TabPage();
            this.pnlSimple = new System.Windows.Forms.Panel();
            this.rtbProblemDescription = new System.Windows.Forms.RichTextBox();
            this.cmbProblemType = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tbLevelOfData = new System.Windows.Forms.TrackBar();
            this.tabAdvanced = new System.Windows.Forms.TabPage();
            this.pnlDiagnosticsToCollect = new System.Windows.Forms.Panel();
            this.chkGetProfiler = new System.Windows.Forms.CheckBox();
            this.chkGetPerfMon = new System.Windows.Forms.CheckBox();
            this.chkDeleteRaw = new System.Windows.Forms.CheckBox();
            this.chkZip = new System.Windows.Forms.CheckBox();
            this.txtStatus = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.udRollover)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udInterval)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabSimple.SuspendLayout();
            this.pnlSimple.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbLevelOfData)).BeginInit();
            this.tabAdvanced.SuspendLayout();
            this.pnlDiagnosticsToCollect.SuspendLayout();
            this.SuspendLayout();
            // 
            // cbInstances
            // 
            this.cbInstances.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbInstances.FormattingEnabled = true;
            this.cbInstances.Location = new System.Drawing.Point(12, 26);
            this.cbInstances.Name = "cbInstances";
            this.cbInstances.Size = new System.Drawing.Size(252, 21);
            this.cbInstances.TabIndex = 0;
            this.cbInstances.SelectedIndexChanged += new System.EventHandler(this.cbInstances_SelectedIndexChanged);
            // 
            // lblInstanceDetails
            // 
            this.lblInstanceDetails.AutoSize = true;
            this.lblInstanceDetails.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInstanceDetails.Location = new System.Drawing.Point(278, 23);
            this.lblInstanceDetails.Name = "lblInstanceDetails";
            this.lblInstanceDetails.Size = new System.Drawing.Size(0, 12);
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
            this.btnCapture.FlatAppearance.BorderColor = System.Drawing.SystemColors.ButtonFace;
            this.btnCapture.FlatAppearance.BorderSize = 0;
            this.btnCapture.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btnCapture.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btnCapture.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCapture.Image = global::SSASDiag.Properties.Resources.play;
            this.btnCapture.Location = new System.Drawing.Point(280, 80);
            this.btnCapture.Name = "btnCapture";
            this.btnCapture.Size = new System.Drawing.Size(68, 68);
            this.btnCapture.TabIndex = 17;
            this.btnCapture.UseVisualStyleBackColor = false;
            this.btnCapture.Click += new System.EventHandler(this.btnCapture_Click);
            this.btnCapture.MouseEnter += new System.EventHandler(this.btnCapture_MouseEnter);
            this.btnCapture.MouseLeave += new System.EventHandler(this.btnCapture_MouseLeave);
            // 
            // chkRollover
            // 
            this.chkRollover.AutoSize = true;
            this.chkRollover.Location = new System.Drawing.Point(12, 81);
            this.chkRollover.Name = "chkRollover";
            this.chkRollover.Size = new System.Drawing.Size(162, 17);
            this.chkRollover.TabIndex = 3;
            this.chkRollover.Text = "Rollover log/trace/zip files at";
            this.chkRollover.UseVisualStyleBackColor = true;
            this.chkRollover.CheckedChanged += new System.EventHandler(this.chkRollover_CheckedChanged);
            // 
            // chkAutoRestart
            // 
            this.chkAutoRestart.AutoSize = true;
            this.chkAutoRestart.Location = new System.Drawing.Point(12, 150);
            this.chkAutoRestart.Name = "chkAutoRestart";
            this.chkAutoRestart.Size = new System.Drawing.Size(206, 17);
            this.chkAutoRestart.TabIndex = 9;
            this.chkAutoRestart.Text = "Restart profiler trace if service restarts.";
            this.chkAutoRestart.UseVisualStyleBackColor = true;
            this.chkAutoRestart.CheckedChanged += new System.EventHandler(this.chkAutoRestart_CheckedChanged);
            // 
            // udRollover
            // 
            this.udRollover.Enabled = false;
            this.udRollover.Location = new System.Drawing.Point(170, 80);
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
            this.udRollover.TabIndex = 4;
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
            this.label2.Location = new System.Drawing.Point(230, 82);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 23;
            this.label2.Text = "MB.";
            // 
            // udInterval
            // 
            this.udInterval.Location = new System.Drawing.Point(150, 171);
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
            this.udInterval.TabIndex = 10;
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
            this.lblInterval.Location = new System.Drawing.Point(28, 173);
            this.lblInterval.Name = "lblInterval";
            this.lblInterval.Size = new System.Drawing.Size(124, 13);
            this.lblInterval.TabIndex = 19;
            this.lblInterval.Text = "Performance log interval:";
            // 
            // lblInterval2
            // 
            this.lblInterval2.AutoSize = true;
            this.lblInterval2.Location = new System.Drawing.Point(195, 173);
            this.lblInterval2.Name = "lblInterval2";
            this.lblInterval2.Size = new System.Drawing.Size(50, 13);
            this.lblInterval2.TabIndex = 18;
            this.lblInterval2.Text = "seconds.";
            // 
            // chkStartTime
            // 
            this.chkStartTime.AutoSize = true;
            this.chkStartTime.Location = new System.Drawing.Point(12, 104);
            this.chkStartTime.Name = "chkStartTime";
            this.chkStartTime.Size = new System.Drawing.Size(73, 17);
            this.chkStartTime.TabIndex = 5;
            this.chkStartTime.Text = "Start time:";
            this.chkStartTime.UseVisualStyleBackColor = true;
            this.chkStartTime.CheckedChanged += new System.EventHandler(this.chkStartTime_CheckedChanged);
            // 
            // dtStartTime
            // 
            this.dtStartTime.CustomFormat = "MM/dd/yyyy HH:mm:ss UTC";
            this.dtStartTime.Enabled = false;
            this.dtStartTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtStartTime.Location = new System.Drawing.Point(80, 102);
            this.dtStartTime.Name = "dtStartTime";
            this.dtStartTime.Size = new System.Drawing.Size(184, 20);
            this.dtStartTime.TabIndex = 6;
            // 
            // lkFeedback
            // 
            this.lkFeedback.AutoSize = true;
            this.lkFeedback.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lkFeedback.ForeColor = System.Drawing.SystemColors.Highlight;
            this.lkFeedback.Image = ((System.Drawing.Image)(resources.GetObject("lkFeedback.Image")));
            this.lkFeedback.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lkFeedback.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.lkFeedback.Location = new System.Drawing.Point(12, 445);
            this.lkFeedback.Name = "lkFeedback";
            this.lkFeedback.Padding = new System.Windows.Forms.Padding(2);
            this.lkFeedback.Size = new System.Drawing.Size(77, 17);
            this.lkFeedback.TabIndex = 18;
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
            this.lkBugs.Location = new System.Drawing.Point(95, 445);
            this.lkBugs.Name = "lkBugs";
            this.lkBugs.Padding = new System.Windows.Forms.Padding(2);
            this.lkBugs.Size = new System.Drawing.Size(53, 17);
            this.lkBugs.TabIndex = 19;
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
            this.lkDiscussion.Location = new System.Drawing.Point(154, 445);
            this.lkDiscussion.Name = "lkDiscussion";
            this.lkDiscussion.Padding = new System.Windows.Forms.Padding(2);
            this.lkDiscussion.Size = new System.Drawing.Size(111, 17);
            this.lkDiscussion.TabIndex = 20;
            this.lkDiscussion.TabStop = true;
            this.lkDiscussion.Text = "      Discussion/Ideas";
            this.ttStatus.SetToolTip(this.lkDiscussion, "Make it better!");
            this.lkDiscussion.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lkDiscussion_LinkClicked);
            // 
            // chkBAK
            // 
            this.chkBAK.AutoSize = true;
            this.chkBAK.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkBAK.ForeColor = System.Drawing.SystemColors.ControlText;
            this.chkBAK.Location = new System.Drawing.Point(41, 117);
            this.chkBAK.Name = "chkBAK";
            this.chkBAK.Size = new System.Drawing.Size(150, 17);
            this.chkBAK.TabIndex = 27;
            this.chkBAK.Text = "SQL data source backups";
            this.ttStatus.SetToolTip(this.chkBAK, "Optimal dataset allowing any degree of experimentation including changes to data " +
        "structures requiring reprocessing.");
            this.chkBAK.UseVisualStyleBackColor = true;
            this.chkBAK.CheckedChanged += new System.EventHandler(this.chkBAK_CheckedChanged);
            // 
            // chkABF
            // 
            this.chkABF.AutoSize = true;
            this.chkABF.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkABF.Location = new System.Drawing.Point(23, 140);
            this.chkABF.Name = "chkABF";
            this.chkABF.Size = new System.Drawing.Size(168, 17);
            this.chkABF.TabIndex = 25;
            this.chkABF.Text = "Traced AS database backups";
            this.ttStatus.SetToolTip(this.chkABF, "Allows execution of queries and modification of calculations, but not changes to " +
        "data structures requiring reprocessing.");
            this.chkABF.UseVisualStyleBackColor = true;
            // 
            // chkProfilerPerfDetails
            // 
            this.chkProfilerPerfDetails.AutoSize = true;
            this.chkProfilerPerfDetails.Checked = true;
            this.chkProfilerPerfDetails.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkProfilerPerfDetails.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkProfilerPerfDetails.Location = new System.Drawing.Point(23, 71);
            this.chkProfilerPerfDetails.Name = "chkProfilerPerfDetails";
            this.chkProfilerPerfDetails.Size = new System.Drawing.Size(160, 17);
            this.chkProfilerPerfDetails.TabIndex = 22;
            this.chkProfilerPerfDetails.Text = "Performance relevant details";
            this.ttStatus.SetToolTip(this.chkProfilerPerfDetails, "Additional Profiler trace details increase size and weight of trace but allow dee" +
        "per investigation of performance bottlenecks.");
            this.chkProfilerPerfDetails.UseVisualStyleBackColor = true;
            this.chkProfilerPerfDetails.CheckedChanged += new System.EventHandler(this.chkProfilerPerfDetails_CheckedChanged);
            // 
            // chkGetNetwork
            // 
            this.chkGetNetwork.AutoSize = true;
            this.chkGetNetwork.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkGetNetwork.Location = new System.Drawing.Point(3, 163);
            this.chkGetNetwork.Name = "chkGetNetwork";
            this.chkGetNetwork.Size = new System.Drawing.Size(102, 17);
            this.chkGetNetwork.TabIndex = 23;
            this.chkGetNetwork.Text = "Network Traces";
            this.ttStatus.SetToolTip(this.chkGetNetwork, "Network traces are useful for connectivity investigations most often.");
            this.chkGetNetwork.UseVisualStyleBackColor = true;
            this.chkGetNetwork.CheckedChanged += new System.EventHandler(this.chkGetNetwork_CheckedChanged);
            // 
            // chkXMLA
            // 
            this.chkXMLA.AutoSize = true;
            this.chkXMLA.Checked = true;
            this.chkXMLA.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkXMLA.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkXMLA.Location = new System.Drawing.Point(23, 94);
            this.chkXMLA.Name = "chkXMLA";
            this.chkXMLA.Size = new System.Drawing.Size(174, 17);
            this.chkXMLA.TabIndex = 24;
            this.chkXMLA.Text = "Traced AS database definitions";
            this.ttStatus.SetToolTip(this.chkXMLA, "Allows review of data structures and calculations but no actual data is included." +
        "");
            this.chkXMLA.UseVisualStyleBackColor = true;
            this.chkXMLA.CheckedChanged += new System.EventHandler(this.chkXMLA_CheckedChanged);
            // 
            // chkGetConfigDetails
            // 
            this.chkGetConfigDetails.AutoSize = true;
            this.chkGetConfigDetails.Checked = true;
            this.chkGetConfigDetails.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkGetConfigDetails.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkGetConfigDetails.Location = new System.Drawing.Point(3, 3);
            this.chkGetConfigDetails.Name = "chkGetConfigDetails";
            this.chkGetConfigDetails.Size = new System.Drawing.Size(179, 17);
            this.chkGetConfigDetails.TabIndex = 26;
            this.chkGetConfigDetails.Text = "Instance Configuration and Logs";
            this.ttStatus.SetToolTip(this.chkGetConfigDetails, "*Includes event logs and service account SPNs if available.");
            this.chkGetConfigDetails.UseVisualStyleBackColor = true;
            this.chkGetConfigDetails.CheckedChanged += new System.EventHandler(this.chkGetConfigDetails_CheckedChanged);
            // 
            // lblBAK
            // 
            this.lblBAK.AutoSize = true;
            this.lblBAK.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBAK.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.lblBAK.Location = new System.Drawing.Point(129, 45);
            this.lblBAK.Name = "lblBAK";
            this.lblBAK.Size = new System.Drawing.Size(75, 12);
            this.lblBAK.TabIndex = 6;
            this.lblBAK.Text = "Full data sources";
            this.ttStatus.SetToolTip(this.lblBAK, "Allows all experimentation including processing.");
            // 
            // lblABF
            // 
            this.lblABF.AutoSize = true;
            this.lblABF.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblABF.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.lblABF.Location = new System.Drawing.Point(72, 45);
            this.lblABF.Name = "lblABF";
            this.lblABF.Size = new System.Drawing.Size(55, 12);
            this.lblABF.TabIndex = 5;
            this.lblABF.Text = "AS backups";
            this.ttStatus.SetToolTip(this.lblABF, "Allows review of data structures and calculations, and execution of queries, but " +
        "not processing.");
            // 
            // lblXMLA
            // 
            this.lblXMLA.AutoSize = true;
            this.lblXMLA.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblXMLA.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblXMLA.Location = new System.Drawing.Point(1, 45);
            this.lblXMLA.Name = "lblXMLA";
            this.lblXMLA.Size = new System.Drawing.Size(68, 12);
            this.lblXMLA.TabIndex = 4;
            this.lblXMLA.Text = "Only definitions";
            this.ttStatus.SetToolTip(this.lblXMLA, "Allows only review of data structures and calculations.  Queries cannot be execut" +
        "ed.");
            // 
            // dtStopTime
            // 
            this.dtStopTime.CustomFormat = "MM/dd/yyyy HH:mm:ss UTC";
            this.dtStopTime.Enabled = false;
            this.dtStopTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtStopTime.Location = new System.Drawing.Point(80, 125);
            this.dtStopTime.Name = "dtStopTime";
            this.dtStopTime.Size = new System.Drawing.Size(184, 20);
            this.dtStopTime.TabIndex = 8;
            // 
            // chkStopTime
            // 
            this.chkStopTime.AutoSize = true;
            this.chkStopTime.Location = new System.Drawing.Point(12, 127);
            this.chkStopTime.Name = "chkStopTime";
            this.chkStopTime.Size = new System.Drawing.Size(73, 17);
            this.chkStopTime.TabIndex = 7;
            this.chkStopTime.Text = "Stop time:";
            this.chkStopTime.UseVisualStyleBackColor = true;
            this.chkStopTime.CheckedChanged += new System.EventHandler(this.chkStopTime_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tabControl1);
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(363, 55);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(0);
            this.groupBox1.Size = new System.Drawing.Size(229, 147);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Diagnostics to Capture:";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabSimple);
            this.tabControl1.Controls.Add(this.tabAdvanced);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabControl1.Location = new System.Drawing.Point(0, 13);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.Padding = new System.Drawing.Point(0, 0);
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(229, 134);
            this.tabControl1.TabIndex = 1;
            // 
            // tabSimple
            // 
            this.tabSimple.BackColor = System.Drawing.SystemColors.Control;
            this.tabSimple.Controls.Add(this.pnlSimple);
            this.tabSimple.Location = new System.Drawing.Point(4, 22);
            this.tabSimple.Margin = new System.Windows.Forms.Padding(0);
            this.tabSimple.Name = "tabSimple";
            this.tabSimple.Size = new System.Drawing.Size(221, 108);
            this.tabSimple.TabIndex = 0;
            this.tabSimple.Text = "Simple";
            // 
            // pnlSimple
            // 
            this.pnlSimple.AutoScroll = true;
            this.pnlSimple.Controls.Add(this.rtbProblemDescription);
            this.pnlSimple.Controls.Add(this.cmbProblemType);
            this.pnlSimple.Controls.Add(this.label7);
            this.pnlSimple.Controls.Add(this.lblBAK);
            this.pnlSimple.Controls.Add(this.lblABF);
            this.pnlSimple.Controls.Add(this.lblXMLA);
            this.pnlSimple.Controls.Add(this.label3);
            this.pnlSimple.Controls.Add(this.tbLevelOfData);
            this.pnlSimple.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSimple.Location = new System.Drawing.Point(0, 0);
            this.pnlSimple.Name = "pnlSimple";
            this.pnlSimple.Size = new System.Drawing.Size(221, 108);
            this.pnlSimple.TabIndex = 33;
            // 
            // rtbProblemDescription
            // 
            this.rtbProblemDescription.BackColor = System.Drawing.SystemColors.Control;
            this.rtbProblemDescription.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbProblemDescription.Location = new System.Drawing.Point(5, 112);
            this.rtbProblemDescription.Name = "rtbProblemDescription";
            this.rtbProblemDescription.Size = new System.Drawing.Size(196, 240);
            this.rtbProblemDescription.TabIndex = 120;
            this.rtbProblemDescription.Text = "";
            // 
            // cmbProblemType
            // 
            this.cmbProblemType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbProblemType.Items.AddRange(new object[] {
            "Performance",
            "Errors (non-connectivity)",
            "Connectivity Failures",
            "Incorrect Query Results",
            "Data Corruption"});
            this.cmbProblemType.Location = new System.Drawing.Point(5, 81);
            this.cmbProblemType.Name = "cmbProblemType";
            this.cmbProblemType.Size = new System.Drawing.Size(196, 21);
            this.cmbProblemType.TabIndex = 8;
            this.cmbProblemType.SelectedIndexChanged += new System.EventHandler(this.cmbProblemType_SelectedIndexChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(61, 64);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(82, 13);
            this.label7.TabIndex = 7;
            this.label7.Text = "Problem Type";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(29, 5);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(146, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Level of Data to Provide";
            // 
            // tbLevelOfData
            // 
            this.tbLevelOfData.LargeChange = 1;
            this.tbLevelOfData.Location = new System.Drawing.Point(21, 16);
            this.tbLevelOfData.Maximum = 2;
            this.tbLevelOfData.Name = "tbLevelOfData";
            this.tbLevelOfData.Size = new System.Drawing.Size(163, 45);
            this.tbLevelOfData.TabIndex = 1;
            this.tbLevelOfData.Scroll += new System.EventHandler(this.tbLevelOfData_Scroll);
            this.tbLevelOfData.ValueChanged += new System.EventHandler(this.tbLevelOfData_ValueChanged);
            // 
            // tabAdvanced
            // 
            this.tabAdvanced.BackColor = System.Drawing.SystemColors.Control;
            this.tabAdvanced.Controls.Add(this.pnlDiagnosticsToCollect);
            this.tabAdvanced.Location = new System.Drawing.Point(4, 22);
            this.tabAdvanced.Margin = new System.Windows.Forms.Padding(0);
            this.tabAdvanced.Name = "tabAdvanced";
            this.tabAdvanced.Size = new System.Drawing.Size(221, 108);
            this.tabAdvanced.TabIndex = 1;
            this.tabAdvanced.Text = "Advanced";
            // 
            // pnlDiagnosticsToCollect
            // 
            this.pnlDiagnosticsToCollect.AutoScroll = true;
            this.pnlDiagnosticsToCollect.BackColor = System.Drawing.SystemColors.Control;
            this.pnlDiagnosticsToCollect.Controls.Add(this.chkBAK);
            this.pnlDiagnosticsToCollect.Controls.Add(this.chkGetConfigDetails);
            this.pnlDiagnosticsToCollect.Controls.Add(this.chkABF);
            this.pnlDiagnosticsToCollect.Controls.Add(this.chkProfilerPerfDetails);
            this.pnlDiagnosticsToCollect.Controls.Add(this.chkGetNetwork);
            this.pnlDiagnosticsToCollect.Controls.Add(this.chkGetProfiler);
            this.pnlDiagnosticsToCollect.Controls.Add(this.chkGetPerfMon);
            this.pnlDiagnosticsToCollect.Controls.Add(this.chkXMLA);
            this.pnlDiagnosticsToCollect.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDiagnosticsToCollect.Location = new System.Drawing.Point(0, 0);
            this.pnlDiagnosticsToCollect.Margin = new System.Windows.Forms.Padding(0);
            this.pnlDiagnosticsToCollect.Name = "pnlDiagnosticsToCollect";
            this.pnlDiagnosticsToCollect.Size = new System.Drawing.Size(221, 108);
            this.pnlDiagnosticsToCollect.TabIndex = 38;
            // 
            // chkGetProfiler
            // 
            this.chkGetProfiler.AutoSize = true;
            this.chkGetProfiler.Checked = true;
            this.chkGetProfiler.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkGetProfiler.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkGetProfiler.Location = new System.Drawing.Point(3, 48);
            this.chkGetProfiler.Name = "chkGetProfiler";
            this.chkGetProfiler.Size = new System.Drawing.Size(118, 17);
            this.chkGetProfiler.TabIndex = 21;
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
            this.chkGetPerfMon.Location = new System.Drawing.Point(3, 25);
            this.chkGetPerfMon.Name = "chkGetPerfMon";
            this.chkGetPerfMon.Size = new System.Drawing.Size(150, 17);
            this.chkGetPerfMon.TabIndex = 20;
            this.chkGetPerfMon.Text = "Performance Monitor Logs";
            this.chkGetPerfMon.UseVisualStyleBackColor = true;
            this.chkGetPerfMon.CheckedChanged += new System.EventHandler(this.chkGetPerfMon_CheckedChanged);
            // 
            // chkDeleteRaw
            // 
            this.chkDeleteRaw.AutoSize = true;
            this.chkDeleteRaw.Checked = true;
            this.chkDeleteRaw.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDeleteRaw.Location = new System.Drawing.Point(124, 58);
            this.chkDeleteRaw.Name = "chkDeleteRaw";
            this.chkDeleteRaw.Size = new System.Drawing.Size(144, 17);
            this.chkDeleteRaw.TabIndex = 2;
            this.chkDeleteRaw.Text = "Delete raw data after zip.";
            this.chkDeleteRaw.UseVisualStyleBackColor = true;
            this.chkDeleteRaw.CheckedChanged += new System.EventHandler(this.chkDeleteRaw_CheckedChanged);
            // 
            // chkZip
            // 
            this.chkZip.AutoSize = true;
            this.chkZip.Checked = true;
            this.chkZip.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkZip.Location = new System.Drawing.Point(12, 58);
            this.chkZip.Name = "chkZip";
            this.chkZip.Size = new System.Drawing.Size(106, 17);
            this.chkZip.TabIndex = 1;
            this.chkZip.Text = "Compress to .zip.";
            this.chkZip.UseVisualStyleBackColor = true;
            this.chkZip.CheckedChanged += new System.EventHandler(this.chkZip_CheckedChanged);
            // 
            // txtStatus
            // 
            this.txtStatus.BackColor = System.Drawing.Color.Black;
            this.txtStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtStatus.DetectUrls = false;
            this.txtStatus.Font = new System.Drawing.Font("Consolas", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtStatus.ForeColor = System.Drawing.Color.LightSkyBlue;
            this.txtStatus.Location = new System.Drawing.Point(12, 205);
            this.txtStatus.Name = "txtStatus";
            this.txtStatus.ReadOnly = true;
            this.txtStatus.Size = new System.Drawing.Size(578, 238);
            this.txtStatus.TabIndex = 17;
            this.txtStatus.Text = "";
            this.txtStatus.MouseDown += new System.Windows.Forms.MouseEventHandler(this.txtStatus_MouseDown);
            // 
            // frmSSASDiag
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(601, 464);
            this.Controls.Add(this.chkDeleteRaw);
            this.Controls.Add(this.chkZip);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.dtStopTime);
            this.Controls.Add(this.chkStopTime);
            this.Controls.Add(this.dtStartTime);
            this.Controls.Add(this.chkStartTime);
            this.Controls.Add(this.lblInterval2);
            this.Controls.Add(this.udInterval);
            this.Controls.Add(this.udRollover);
            this.Controls.Add(this.chkAutoRestart);
            this.Controls.Add(this.chkRollover);
            this.Controls.Add(this.btnCapture);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblInstanceDetails);
            this.Controls.Add(this.cbInstances);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lkDiscussion);
            this.Controls.Add(this.lkBugs);
            this.Controls.Add(this.lkFeedback);
            this.Controls.Add(this.txtStatus);
            this.Controls.Add(this.lblInterval);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(617, 427);
            this.Name = "frmSSASDiag";
            this.Text = "SSAS Diagnostics Collector";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmSSASDiag_FormClosing);
            this.Load += new System.EventHandler(this.frmSSASDiag_Load);
            this.Resize += new System.EventHandler(this.frmSSASDiag_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.udRollover)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udInterval)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabSimple.ResumeLayout(false);
            this.pnlSimple.ResumeLayout(false);
            this.pnlSimple.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbLevelOfData)).EndInit();
            this.tabAdvanced.ResumeLayout(false);
            this.pnlDiagnosticsToCollect.ResumeLayout(false);
            this.pnlDiagnosticsToCollect.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbInstances;
        private System.Windows.Forms.Label lblInstanceDetails;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnCapture;
        private System.Windows.Forms.CheckBox chkRollover;
        private System.Windows.Forms.CheckBox chkAutoRestart;
        private System.Windows.Forms.NumericUpDown udRollover;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown udInterval;
        private System.Windows.Forms.Label lblInterval;
        private System.Windows.Forms.Label lblInterval2;
        private System.Windows.Forms.CheckBox chkStartTime;
        private System.Windows.Forms.DateTimePicker dtStartTime;
        private System.Windows.Forms.ToolTip ttStatus;
        private System.Windows.Forms.DateTimePicker dtStopTime;
        private System.Windows.Forms.CheckBox chkStopTime;
        private System.Windows.Forms.LinkLabel lkFeedback;
        private System.Windows.Forms.LinkLabel lkBugs;
        private System.Windows.Forms.LinkLabel lkDiscussion;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox chkDeleteRaw;
        private System.Windows.Forms.CheckBox chkZip;
        private System.Windows.Forms.RichTextBox txtStatus;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabSimple;
        private System.Windows.Forms.TabPage tabAdvanced;
        private System.Windows.Forms.Panel pnlDiagnosticsToCollect;
        private System.Windows.Forms.CheckBox chkBAK;
        private System.Windows.Forms.CheckBox chkGetConfigDetails;
        private System.Windows.Forms.CheckBox chkABF;
        private System.Windows.Forms.CheckBox chkProfilerPerfDetails;
        private System.Windows.Forms.CheckBox chkGetNetwork;
        private System.Windows.Forms.CheckBox chkGetProfiler;
        private System.Windows.Forms.CheckBox chkGetPerfMon;
        private System.Windows.Forms.CheckBox chkXMLA;
        private System.Windows.Forms.Panel pnlSimple;
        private System.Windows.Forms.RichTextBox rtbProblemDescription;
        private System.Windows.Forms.ComboBox cmbProblemType;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label lblBAK;
        private System.Windows.Forms.Label lblABF;
        private System.Windows.Forms.Label lblXMLA;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TrackBar tbLevelOfData;
    }
}

