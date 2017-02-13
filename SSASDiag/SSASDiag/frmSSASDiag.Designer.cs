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
            this.ttStatus = new System.Windows.Forms.ToolTip(this.components);
            this.lkFeedback = new System.Windows.Forms.LinkLabel();
            this.lkBugs = new System.Windows.Forms.LinkLabel();
            this.lkDiscussion = new System.Windows.Forms.LinkLabel();
            this.lblBAK = new System.Windows.Forms.Label();
            this.lblABF = new System.Windows.Forms.Label();
            this.lblXMLA = new System.Windows.Forms.Label();
            this.chkBAK = new System.Windows.Forms.CheckBox();
            this.chkGetConfigDetails = new System.Windows.Forms.CheckBox();
            this.chkABF = new System.Windows.Forms.CheckBox();
            this.chkProfilerPerfDetails = new System.Windows.Forms.CheckBox();
            this.chkGetNetwork = new System.Windows.Forms.CheckBox();
            this.chkXMLA = new System.Windows.Forms.CheckBox();
            this.tcCollectionAnalysisTabs = new System.Windows.Forms.TabControl();
            this.tbCollection = new System.Windows.Forms.TabPage();
            this.chkDeleteRaw = new System.Windows.Forms.CheckBox();
            this.chkZip = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tcSimpleAdvanced = new System.Windows.Forms.TabControl();
            this.tabGuided = new System.Windows.Forms.TabPage();
            this.pnlSimple = new System.Windows.Forms.Panel();
            this.rtbProblemDescription = new System.Windows.Forms.RichTextBox();
            this.cmbProblemType = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.lblLevelOfReproData = new System.Windows.Forms.Label();
            this.tbLevelOfData = new System.Windows.Forms.TrackBar();
            this.tabAdvanced = new System.Windows.Forms.TabPage();
            this.pnlDiagnosticsToCollect = new System.Windows.Forms.Panel();
            this.chkGetProfiler = new System.Windows.Forms.CheckBox();
            this.chkGetPerfMon = new System.Windows.Forms.CheckBox();
            this.dtStopTime = new System.Windows.Forms.DateTimePicker();
            this.chkStopTime = new System.Windows.Forms.CheckBox();
            this.dtStartTime = new System.Windows.Forms.DateTimePicker();
            this.chkStartTime = new System.Windows.Forms.CheckBox();
            this.udInterval = new System.Windows.Forms.NumericUpDown();
            this.udRollover = new System.Windows.Forms.NumericUpDown();
            this.chkAutoRestart = new System.Windows.Forms.CheckBox();
            this.chkRollover = new System.Windows.Forms.CheckBox();
            this.btnCapture = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.lblInstanceDetails = new System.Windows.Forms.Label();
            this.cbInstances = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtStatus = new System.Windows.Forms.RichTextBox();
            this.lblInterval = new System.Windows.Forms.Label();
            this.lblInterval2 = new System.Windows.Forms.Label();
            this.tbAnalysis = new System.Windows.Forms.TabPage();
            this.tcAnalysis = new System.Windows.Forms.TabControl();
            this.imgAnalyzerIcons = new System.Windows.Forms.ImageList(this.components);
            this.btnAnalysisFolder = new System.Windows.Forms.Button();
            this.txtFolderZipForAnalysis = new System.Windows.Forms.TextBox();
            this.lblFolderZipPrompt = new System.Windows.Forms.Label();
            this.tcCollectionAnalysisTabs.SuspendLayout();
            this.tbCollection.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tcSimpleAdvanced.SuspendLayout();
            this.tabGuided.SuspendLayout();
            this.pnlSimple.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbLevelOfData)).BeginInit();
            this.tabAdvanced.SuspendLayout();
            this.pnlDiagnosticsToCollect.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udInterval)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udRollover)).BeginInit();
            this.tbAnalysis.SuspendLayout();
            this.SuspendLayout();
            // 
            // lkFeedback
            // 
            this.lkFeedback.AutoSize = true;
            this.lkFeedback.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lkFeedback.ForeColor = System.Drawing.SystemColors.Highlight;
            this.lkFeedback.Image = ((System.Drawing.Image)(resources.GetObject("lkFeedback.Image")));
            this.lkFeedback.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lkFeedback.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.lkFeedback.Location = new System.Drawing.Point(7, 468);
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
            this.lkBugs.Location = new System.Drawing.Point(90, 468);
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
            this.lkDiscussion.Location = new System.Drawing.Point(149, 468);
            this.lkDiscussion.Name = "lkDiscussion";
            this.lkDiscussion.Padding = new System.Windows.Forms.Padding(2);
            this.lkDiscussion.Size = new System.Drawing.Size(111, 17);
            this.lkDiscussion.TabIndex = 20;
            this.lkDiscussion.TabStop = true;
            this.lkDiscussion.Text = "      Discussion/Ideas";
            this.ttStatus.SetToolTip(this.lkDiscussion, "Make it better!");
            this.lkDiscussion.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lkDiscussion_LinkClicked);
            // 
            // lblBAK
            // 
            this.lblBAK.AutoSize = true;
            this.lblBAK.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBAK.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.lblBAK.Location = new System.Drawing.Point(136, 45);
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
            this.lblABF.Location = new System.Drawing.Point(75, 45);
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
            // chkBAK
            // 
            this.chkBAK.AutoSize = true;
            this.chkBAK.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkBAK.ForeColor = System.Drawing.SystemColors.ControlText;
            this.chkBAK.Location = new System.Drawing.Point(41, 117);
            this.chkBAK.Name = "chkBAK";
            this.chkBAK.Size = new System.Drawing.Size(150, 17);
            this.chkBAK.TabIndex = 5;
            this.chkBAK.Text = "SQL data source backups";
            this.ttStatus.SetToolTip(this.chkBAK, "Optimal dataset allowing any degree of experimentation including changes to data " +
        "structures requiring reprocessing.");
            this.chkBAK.UseVisualStyleBackColor = true;
            this.chkBAK.CheckedChanged += new System.EventHandler(this.chkBAK_CheckedChanged);
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
            this.chkGetConfigDetails.TabIndex = 0;
            this.chkGetConfigDetails.Text = "Instance Configuration and Logs";
            this.ttStatus.SetToolTip(this.chkGetConfigDetails, "*Includes event logs and service account SPNs if available.");
            this.chkGetConfigDetails.UseVisualStyleBackColor = true;
            this.chkGetConfigDetails.CheckedChanged += new System.EventHandler(this.chkGetConfigDetails_CheckedChanged);
            // 
            // chkABF
            // 
            this.chkABF.AutoSize = true;
            this.chkABF.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkABF.Location = new System.Drawing.Point(23, 140);
            this.chkABF.Name = "chkABF";
            this.chkABF.Size = new System.Drawing.Size(168, 17);
            this.chkABF.TabIndex = 6;
            this.chkABF.Text = "Traced AS database backups";
            this.ttStatus.SetToolTip(this.chkABF, "Allows execution of queries and modification of calculations, but not changes to " +
        "data structures requiring reprocessing.");
            this.chkABF.UseVisualStyleBackColor = true;
            this.chkABF.CheckedChanged += new System.EventHandler(this.chkABF_CheckedChanged);
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
            this.chkProfilerPerfDetails.TabIndex = 3;
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
            this.chkGetNetwork.TabIndex = 7;
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
            this.chkXMLA.TabIndex = 4;
            this.chkXMLA.Text = "Traced AS database definitions";
            this.ttStatus.SetToolTip(this.chkXMLA, "Allows review of data structures and calculations but no actual data is included." +
        "");
            this.chkXMLA.UseVisualStyleBackColor = true;
            this.chkXMLA.CheckedChanged += new System.EventHandler(this.chkXMLA_CheckedChanged);
            // 
            // tcCollectionAnalysisTabs
            // 
            this.tcCollectionAnalysisTabs.Controls.Add(this.tbCollection);
            this.tcCollectionAnalysisTabs.Controls.Add(this.tbAnalysis);
            this.tcCollectionAnalysisTabs.Dock = System.Windows.Forms.DockStyle.Top;
            this.tcCollectionAnalysisTabs.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.tcCollectionAnalysisTabs.Location = new System.Drawing.Point(0, 0);
            this.tcCollectionAnalysisTabs.Name = "tcCollectionAnalysisTabs";
            this.tcCollectionAnalysisTabs.SelectedIndex = 0;
            this.tcCollectionAnalysisTabs.Size = new System.Drawing.Size(601, 465);
            this.tcCollectionAnalysisTabs.TabIndex = 21;
            this.tcCollectionAnalysisTabs.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.tcCollectionAnalysisTabs_DrawItem);
            this.tcCollectionAnalysisTabs.SelectedIndexChanged += new System.EventHandler(this.tcCollectionAnalysisTabs_SelectedIndexChanged);
            this.tcCollectionAnalysisTabs.Selecting += new System.Windows.Forms.TabControlCancelEventHandler(this.tcCollectionAnalysisTabs_Selecting);
            // 
            // tbCollection
            // 
            this.tbCollection.BackColor = System.Drawing.SystemColors.Control;
            this.tbCollection.Controls.Add(this.chkDeleteRaw);
            this.tbCollection.Controls.Add(this.chkZip);
            this.tbCollection.Controls.Add(this.groupBox1);
            this.tbCollection.Controls.Add(this.dtStopTime);
            this.tbCollection.Controls.Add(this.chkStopTime);
            this.tbCollection.Controls.Add(this.dtStartTime);
            this.tbCollection.Controls.Add(this.chkStartTime);
            this.tbCollection.Controls.Add(this.udInterval);
            this.tbCollection.Controls.Add(this.udRollover);
            this.tbCollection.Controls.Add(this.chkAutoRestart);
            this.tbCollection.Controls.Add(this.chkRollover);
            this.tbCollection.Controls.Add(this.btnCapture);
            this.tbCollection.Controls.Add(this.label1);
            this.tbCollection.Controls.Add(this.lblInstanceDetails);
            this.tbCollection.Controls.Add(this.cbInstances);
            this.tbCollection.Controls.Add(this.label2);
            this.tbCollection.Controls.Add(this.txtStatus);
            this.tbCollection.Controls.Add(this.lblInterval);
            this.tbCollection.Controls.Add(this.lblInterval2);
            this.tbCollection.Location = new System.Drawing.Point(4, 22);
            this.tbCollection.Name = "tbCollection";
            this.tbCollection.Size = new System.Drawing.Size(593, 439);
            this.tbCollection.TabIndex = 0;
            this.tbCollection.Text = "Collection";
            // 
            // chkDeleteRaw
            // 
            this.chkDeleteRaw.AutoSize = true;
            this.chkDeleteRaw.Location = new System.Drawing.Point(120, 58);
            this.chkDeleteRaw.Name = "chkDeleteRaw";
            this.chkDeleteRaw.Size = new System.Drawing.Size(144, 17);
            this.chkDeleteRaw.TabIndex = 26;
            this.chkDeleteRaw.Text = "Delete raw data after zip.";
            this.chkDeleteRaw.UseVisualStyleBackColor = true;
            this.chkDeleteRaw.CheckedChanged += new System.EventHandler(this.chkDeleteRaw_CheckedChanged);
            // 
            // chkZip
            // 
            this.chkZip.AutoSize = true;
            this.chkZip.Checked = true;
            this.chkZip.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkZip.Location = new System.Drawing.Point(8, 58);
            this.chkZip.Name = "chkZip";
            this.chkZip.Size = new System.Drawing.Size(106, 17);
            this.chkZip.TabIndex = 25;
            this.chkZip.Text = "Compress to .zip.";
            this.chkZip.UseVisualStyleBackColor = true;
            this.chkZip.CheckedChanged += new System.EventHandler(this.chkZip_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tcSimpleAdvanced);
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(354, 52);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(0);
            this.groupBox1.Size = new System.Drawing.Size(239, 147);
            this.groupBox1.TabIndex = 35;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Diagnostics to Capture:";
            // 
            // tcSimpleAdvanced
            // 
            this.tcSimpleAdvanced.Controls.Add(this.tabGuided);
            this.tcSimpleAdvanced.Controls.Add(this.tabAdvanced);
            this.tcSimpleAdvanced.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcSimpleAdvanced.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tcSimpleAdvanced.Location = new System.Drawing.Point(0, 13);
            this.tcSimpleAdvanced.Margin = new System.Windows.Forms.Padding(0);
            this.tcSimpleAdvanced.Name = "tcSimpleAdvanced";
            this.tcSimpleAdvanced.Padding = new System.Drawing.Point(0, 0);
            this.tcSimpleAdvanced.SelectedIndex = 0;
            this.tcSimpleAdvanced.Size = new System.Drawing.Size(239, 134);
            this.tcSimpleAdvanced.TabIndex = 1;
            // 
            // tabGuided
            // 
            this.tabGuided.BackColor = System.Drawing.SystemColors.Control;
            this.tabGuided.Controls.Add(this.pnlSimple);
            this.tabGuided.Location = new System.Drawing.Point(4, 22);
            this.tabGuided.Margin = new System.Windows.Forms.Padding(0);
            this.tabGuided.Name = "tabGuided";
            this.tabGuided.Size = new System.Drawing.Size(231, 108);
            this.tabGuided.TabIndex = 0;
            this.tabGuided.Text = "Guided";
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
            this.pnlSimple.Controls.Add(this.lblLevelOfReproData);
            this.pnlSimple.Controls.Add(this.tbLevelOfData);
            this.pnlSimple.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSimple.Location = new System.Drawing.Point(0, 0);
            this.pnlSimple.Name = "pnlSimple";
            this.pnlSimple.Size = new System.Drawing.Size(231, 108);
            this.pnlSimple.TabIndex = 33;
            // 
            // rtbProblemDescription
            // 
            this.rtbProblemDescription.BackColor = System.Drawing.SystemColors.Control;
            this.rtbProblemDescription.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbProblemDescription.Location = new System.Drawing.Point(5, 112);
            this.rtbProblemDescription.Name = "rtbProblemDescription";
            this.rtbProblemDescription.Size = new System.Drawing.Size(206, 240);
            this.rtbProblemDescription.TabIndex = 120;
            this.rtbProblemDescription.Text = "";
            // 
            // cmbProblemType
            // 
            this.cmbProblemType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbProblemType.Items.AddRange(new object[] {
            "Performance",
            "Errors (non-connectivity)",
            "Incorrect Query Results",
            "Connectivity Failures",
            "Connectivity (client/middle-tier only)",
            "Data Corruption"});
            this.cmbProblemType.Location = new System.Drawing.Point(5, 81);
            this.cmbProblemType.Name = "cmbProblemType";
            this.cmbProblemType.Size = new System.Drawing.Size(206, 21);
            this.cmbProblemType.TabIndex = 8;
            this.cmbProblemType.SelectedIndexChanged += new System.EventHandler(this.cmbProblemType_SelectedIndexChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(66, 64);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(82, 13);
            this.label7.TabIndex = 7;
            this.label7.Text = "Problem Type";
            // 
            // lblLevelOfReproData
            // 
            this.lblLevelOfReproData.AutoSize = true;
            this.lblLevelOfReproData.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLevelOfReproData.Location = new System.Drawing.Point(34, 5);
            this.lblLevelOfReproData.Name = "lblLevelOfReproData";
            this.lblLevelOfReproData.Size = new System.Drawing.Size(146, 13);
            this.lblLevelOfReproData.TabIndex = 3;
            this.lblLevelOfReproData.Text = "Level of Data to Provide";
            // 
            // tbLevelOfData
            // 
            this.tbLevelOfData.LargeChange = 1;
            this.tbLevelOfData.Location = new System.Drawing.Point(5, 16);
            this.tbLevelOfData.Maximum = 2;
            this.tbLevelOfData.Name = "tbLevelOfData";
            this.tbLevelOfData.Size = new System.Drawing.Size(199, 45);
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
            this.tabAdvanced.Size = new System.Drawing.Size(231, 108);
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
            this.pnlDiagnosticsToCollect.Size = new System.Drawing.Size(231, 108);
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
            this.chkGetProfiler.TabIndex = 2;
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
            this.chkGetPerfMon.TabIndex = 1;
            this.chkGetPerfMon.Text = "Performance Monitor Logs";
            this.chkGetPerfMon.UseVisualStyleBackColor = true;
            this.chkGetPerfMon.CheckedChanged += new System.EventHandler(this.chkGetPerfMon_CheckedChanged);
            // 
            // dtStopTime
            // 
            this.dtStopTime.CustomFormat = "MM/dd/yyyy HH:mm:ss UTC";
            this.dtStopTime.Enabled = false;
            this.dtStopTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtStopTime.Location = new System.Drawing.Point(79, 128);
            this.dtStopTime.Name = "dtStopTime";
            this.dtStopTime.Size = new System.Drawing.Size(184, 20);
            this.dtStopTime.TabIndex = 32;
            // 
            // chkStopTime
            // 
            this.chkStopTime.AutoSize = true;
            this.chkStopTime.Location = new System.Drawing.Point(8, 127);
            this.chkStopTime.Name = "chkStopTime";
            this.chkStopTime.Size = new System.Drawing.Size(73, 17);
            this.chkStopTime.TabIndex = 31;
            this.chkStopTime.Text = "Stop time:";
            this.chkStopTime.UseVisualStyleBackColor = true;
            this.chkStopTime.CheckedChanged += new System.EventHandler(this.chkStopTime_CheckedChanged);
            // 
            // dtStartTime
            // 
            this.dtStartTime.CustomFormat = "MM/dd/yyyy HH:mm:ss UTC";
            this.dtStartTime.Enabled = false;
            this.dtStartTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtStartTime.Location = new System.Drawing.Point(79, 105);
            this.dtStartTime.Name = "dtStartTime";
            this.dtStartTime.Size = new System.Drawing.Size(184, 20);
            this.dtStartTime.TabIndex = 30;
            // 
            // chkStartTime
            // 
            this.chkStartTime.AutoSize = true;
            this.chkStartTime.Location = new System.Drawing.Point(8, 104);
            this.chkStartTime.Name = "chkStartTime";
            this.chkStartTime.Size = new System.Drawing.Size(73, 17);
            this.chkStartTime.TabIndex = 29;
            this.chkStartTime.Text = "Start time:";
            this.chkStartTime.UseVisualStyleBackColor = true;
            this.chkStartTime.CheckedChanged += new System.EventHandler(this.chkStartTime_CheckedChanged);
            // 
            // udInterval
            // 
            this.udInterval.Location = new System.Drawing.Point(149, 174);
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
            this.udInterval.TabIndex = 34;
            this.udInterval.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.udInterval.ThousandsSeparator = true;
            this.udInterval.UpDownAlign = System.Windows.Forms.LeftRightAlignment.Left;
            this.udInterval.Value = new decimal(new int[] {
            15,
            0,
            0,
            0});
            // 
            // udRollover
            // 
            this.udRollover.Enabled = false;
            this.udRollover.Location = new System.Drawing.Point(169, 83);
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
            this.udRollover.TabIndex = 28;
            this.udRollover.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.udRollover.ThousandsSeparator = true;
            this.udRollover.UpDownAlign = System.Windows.Forms.LeftRightAlignment.Left;
            this.udRollover.Value = new decimal(new int[] {
            128,
            0,
            0,
            0});
            // 
            // chkAutoRestart
            // 
            this.chkAutoRestart.AutoSize = true;
            this.chkAutoRestart.Location = new System.Drawing.Point(8, 150);
            this.chkAutoRestart.Name = "chkAutoRestart";
            this.chkAutoRestart.Size = new System.Drawing.Size(206, 17);
            this.chkAutoRestart.TabIndex = 33;
            this.chkAutoRestart.Text = "Restart profiler trace if service restarts.";
            this.chkAutoRestart.UseVisualStyleBackColor = true;
            this.chkAutoRestart.CheckedChanged += new System.EventHandler(this.chkAutoRestart_CheckedChanged);
            // 
            // chkRollover
            // 
            this.chkRollover.AutoSize = true;
            this.chkRollover.Location = new System.Drawing.Point(8, 81);
            this.chkRollover.Name = "chkRollover";
            this.chkRollover.Size = new System.Drawing.Size(162, 17);
            this.chkRollover.TabIndex = 27;
            this.chkRollover.Text = "Rollover log/trace/zip files at";
            this.chkRollover.UseVisualStyleBackColor = true;
            this.chkRollover.CheckedChanged += new System.EventHandler(this.chkRollover_CheckedChanged);
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
            this.btnCapture.Location = new System.Drawing.Point(276, 76);
            this.btnCapture.Name = "btnCapture";
            this.btnCapture.Size = new System.Drawing.Size(68, 68);
            this.btnCapture.TabIndex = 37;
            this.btnCapture.UseVisualStyleBackColor = false;
            this.btnCapture.Click += new System.EventHandler(this.btnCapture_Click);
            this.btnCapture.MouseEnter += new System.EventHandler(this.btnCapture_MouseEnter);
            this.btnCapture.MouseLeave += new System.EventHandler(this.btnCapture_MouseLeave);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(238, 13);
            this.label1.TabIndex = 40;
            this.label1.Text = "Select an SSAS instance from the local machine:";
            // 
            // lblInstanceDetails
            // 
            this.lblInstanceDetails.AutoSize = true;
            this.lblInstanceDetails.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInstanceDetails.Location = new System.Drawing.Point(274, 20);
            this.lblInstanceDetails.Name = "lblInstanceDetails";
            this.lblInstanceDetails.Size = new System.Drawing.Size(0, 12);
            this.lblInstanceDetails.TabIndex = 41;
            // 
            // cbInstances
            // 
            this.cbInstances.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbInstances.FormattingEnabled = true;
            this.cbInstances.Location = new System.Drawing.Point(8, 23);
            this.cbInstances.Name = "cbInstances";
            this.cbInstances.Size = new System.Drawing.Size(252, 21);
            this.cbInstances.TabIndex = 24;
            this.cbInstances.SelectedIndexChanged += new System.EventHandler(this.cbInstances_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(226, 82);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 42;
            this.label2.Text = "MB.";
            // 
            // txtStatus
            // 
            this.txtStatus.BackColor = System.Drawing.Color.Black;
            this.txtStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtStatus.DetectUrls = false;
            this.txtStatus.Font = new System.Drawing.Font("Consolas", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtStatus.ForeColor = System.Drawing.Color.LightSkyBlue;
            this.txtStatus.Location = new System.Drawing.Point(3, 200);
            this.txtStatus.Name = "txtStatus";
            this.txtStatus.ReadOnly = true;
            this.txtStatus.Size = new System.Drawing.Size(587, 238);
            this.txtStatus.TabIndex = 36;
            this.txtStatus.Text = "";
            this.txtStatus.MouseDown += new System.Windows.Forms.MouseEventHandler(this.txtStatus_MouseDown);
            // 
            // lblInterval
            // 
            this.lblInterval.AutoSize = true;
            this.lblInterval.Location = new System.Drawing.Point(24, 173);
            this.lblInterval.Name = "lblInterval";
            this.lblInterval.Size = new System.Drawing.Size(124, 13);
            this.lblInterval.TabIndex = 39;
            this.lblInterval.Text = "Performance log interval:";
            // 
            // lblInterval2
            // 
            this.lblInterval2.AutoSize = true;
            this.lblInterval2.Location = new System.Drawing.Point(195, 173);
            this.lblInterval2.Name = "lblInterval2";
            this.lblInterval2.Size = new System.Drawing.Size(50, 13);
            this.lblInterval2.TabIndex = 38;
            this.lblInterval2.Text = "seconds.";
            // 
            // tbAnalysis
            // 
            this.tbAnalysis.BackColor = System.Drawing.SystemColors.Control;
            this.tbAnalysis.Controls.Add(this.tcAnalysis);
            this.tbAnalysis.Controls.Add(this.btnAnalysisFolder);
            this.tbAnalysis.Controls.Add(this.txtFolderZipForAnalysis);
            this.tbAnalysis.Controls.Add(this.lblFolderZipPrompt);
            this.tbAnalysis.Location = new System.Drawing.Point(4, 22);
            this.tbAnalysis.Name = "tbAnalysis";
            this.tbAnalysis.Padding = new System.Windows.Forms.Padding(3);
            this.tbAnalysis.Size = new System.Drawing.Size(593, 439);
            this.tbAnalysis.TabIndex = 1;
            this.tbAnalysis.Text = "Analysis";
            // 
            // tcAnalysis
            // 
            this.tcAnalysis.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tcAnalysis.ImageList = this.imgAnalyzerIcons;
            this.tcAnalysis.ItemSize = new System.Drawing.Size(0, 26);
            this.tcAnalysis.Location = new System.Drawing.Point(3, 31);
            this.tcAnalysis.Margin = new System.Windows.Forms.Padding(0);
            this.tcAnalysis.Name = "tcAnalysis";
            this.tcAnalysis.Padding = new System.Drawing.Point(0, 0);
            this.tcAnalysis.SelectedIndex = 0;
            this.tcAnalysis.Size = new System.Drawing.Size(587, 405);
            this.tcAnalysis.TabIndex = 23;
            // 
            // imgAnalyzerIcons
            // 
            this.imgAnalyzerIcons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgAnalyzerIcons.ImageStream")));
            this.imgAnalyzerIcons.TransparentColor = System.Drawing.Color.Transparent;
            this.imgAnalyzerIcons.Images.SetKeyName(0, "Config.ico");
            this.imgAnalyzerIcons.Images.SetKeyName(1, "CrashDump.ico");
            this.imgAnalyzerIcons.Images.SetKeyName(2, "EventViewer.ico");
            this.imgAnalyzerIcons.Images.SetKeyName(3, "NetworkTrace.ico");
            this.imgAnalyzerIcons.Images.SetKeyName(4, "PerfMon.ico");
            this.imgAnalyzerIcons.Images.SetKeyName(5, "Profiler.ico");
            // 
            // btnAnalysisFolder
            // 
            this.btnAnalysisFolder.Location = new System.Drawing.Point(116, 4);
            this.btnAnalysisFolder.Name = "btnAnalysisFolder";
            this.btnAnalysisFolder.Size = new System.Drawing.Size(28, 20);
            this.btnAnalysisFolder.TabIndex = 0;
            this.btnAnalysisFolder.Text = "...";
            this.btnAnalysisFolder.UseVisualStyleBackColor = true;
            this.btnAnalysisFolder.Click += new System.EventHandler(this.btnAnalysisFolder_Click);
            // 
            // txtFolderZipForAnalysis
            // 
            this.txtFolderZipForAnalysis.Location = new System.Drawing.Point(150, 5);
            this.txtFolderZipForAnalysis.Name = "txtFolderZipForAnalysis";
            this.txtFolderZipForAnalysis.ReadOnly = true;
            this.txtFolderZipForAnalysis.Size = new System.Drawing.Size(437, 20);
            this.txtFolderZipForAnalysis.TabIndex = 1;
            // 
            // lblFolderZipPrompt
            // 
            this.lblFolderZipPrompt.AutoSize = true;
            this.lblFolderZipPrompt.Location = new System.Drawing.Point(3, 8);
            this.lblFolderZipPrompt.Name = "lblFolderZipPrompt";
            this.lblFolderZipPrompt.Size = new System.Drawing.Size(115, 13);
            this.lblFolderZipPrompt.TabIndex = 0;
            this.lblFolderZipPrompt.Text = "Analyze Folder/Zip File";
            // 
            // frmSSASDiag
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(601, 489);
            this.Controls.Add(this.tcCollectionAnalysisTabs);
            this.Controls.Add(this.lkDiscussion);
            this.Controls.Add(this.lkBugs);
            this.Controls.Add(this.lkFeedback);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(617, 427);
            this.Name = "frmSSASDiag";
            this.Text = "SSAS Diagnostics Tool";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmSSASDiag_FormClosing);
            this.Load += new System.EventHandler(this.frmSSASDiag_Load);
            this.Resize += new System.EventHandler(this.frmSSASDiag_Resize);
            this.tcCollectionAnalysisTabs.ResumeLayout(false);
            this.tbCollection.ResumeLayout(false);
            this.tbCollection.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.tcSimpleAdvanced.ResumeLayout(false);
            this.tabGuided.ResumeLayout(false);
            this.pnlSimple.ResumeLayout(false);
            this.pnlSimple.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbLevelOfData)).EndInit();
            this.tabAdvanced.ResumeLayout(false);
            this.pnlDiagnosticsToCollect.ResumeLayout(false);
            this.pnlDiagnosticsToCollect.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udInterval)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udRollover)).EndInit();
            this.tbAnalysis.ResumeLayout(false);
            this.tbAnalysis.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ToolTip ttStatus;
        private System.Windows.Forms.LinkLabel lkFeedback;
        private System.Windows.Forms.LinkLabel lkBugs;
        private System.Windows.Forms.LinkLabel lkDiscussion;
        private System.Windows.Forms.TabControl tcCollectionAnalysisTabs;
        private System.Windows.Forms.TabPage tbCollection;
        private System.Windows.Forms.CheckBox chkDeleteRaw;
        private System.Windows.Forms.CheckBox chkZip;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TabControl tcSimpleAdvanced;
        private System.Windows.Forms.TabPage tabGuided;
        private System.Windows.Forms.Panel pnlSimple;
        private System.Windows.Forms.RichTextBox rtbProblemDescription;
        private System.Windows.Forms.ComboBox cmbProblemType;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label lblBAK;
        private System.Windows.Forms.Label lblABF;
        private System.Windows.Forms.Label lblXMLA;
        private System.Windows.Forms.Label lblLevelOfReproData;
        private System.Windows.Forms.TrackBar tbLevelOfData;
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
        private System.Windows.Forms.DateTimePicker dtStopTime;
        private System.Windows.Forms.CheckBox chkStopTime;
        private System.Windows.Forms.DateTimePicker dtStartTime;
        private System.Windows.Forms.CheckBox chkStartTime;
        private System.Windows.Forms.Label lblInterval2;
        private System.Windows.Forms.NumericUpDown udInterval;
        private System.Windows.Forms.NumericUpDown udRollover;
        private System.Windows.Forms.CheckBox chkAutoRestart;
        private System.Windows.Forms.CheckBox chkRollover;
        private System.Windows.Forms.Button btnCapture;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblInstanceDetails;
        private System.Windows.Forms.ComboBox cbInstances;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RichTextBox txtStatus;
        private System.Windows.Forms.Label lblInterval;
        private System.Windows.Forms.TabPage tbAnalysis;
        private System.Windows.Forms.Button btnAnalysisFolder;
        private System.Windows.Forms.TextBox txtFolderZipForAnalysis;
        private System.Windows.Forms.Label lblFolderZipPrompt;
        private System.Windows.Forms.TabControl tcAnalysis;
        private System.Windows.Forms.ImageList imgAnalyzerIcons;
    }
}

