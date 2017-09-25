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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            this.ttStatus = new System.Windows.Forms.ToolTip(this.components);
            this.lkFeedback = new System.Windows.Forms.LinkLabel();
            this.lkBugs = new System.Windows.Forms.LinkLabel();
            this.lkDiscussion = new System.Windows.Forms.LinkLabel();
            this.lkAbout = new System.Windows.Forms.LinkLabel();
            this.label3 = new System.Windows.Forms.Label();
            this.txtSaveLocation = new System.Windows.Forms.TextBox();
            this.btnSaveLocation = new System.Windows.Forms.Button();
            this.lblBAK = new System.Windows.Forms.Label();
            this.lblABF = new System.Windows.Forms.Label();
            this.lblXMLA = new System.Windows.Forms.Label();
            this.chkBAK = new System.Windows.Forms.CheckBox();
            this.chkGetConfigDetails = new System.Windows.Forms.CheckBox();
            this.chkABF = new System.Windows.Forms.CheckBox();
            this.chkProfilerPerfDetails = new System.Windows.Forms.CheckBox();
            this.chkGetNetwork = new System.Windows.Forms.CheckBox();
            this.chkXMLA = new System.Windows.Forms.CheckBox();
            this.btnHangDumps = new System.Windows.Forms.Button();
            this.tcCollectionAnalysisTabs = new System.Windows.Forms.TabControl();
            this.tbCollection = new System.Windows.Forms.TabPage();
            this.splitCollectionUI = new System.Windows.Forms.SplitContainer();
            this.chkDeleteRaw = new System.Windows.Forms.CheckBox();
            this.chkZip = new System.Windows.Forms.CheckBox();
            this.grpDiagsToCapture = new System.Windows.Forms.GroupBox();
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
            this.chkHangDumps = new System.Windows.Forms.CheckBox();
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
            this.lblInterval = new System.Windows.Forms.Label();
            this.lblInterval2 = new System.Windows.Forms.Label();
            this.txtStatus = new System.Windows.Forms.RichTextBox();
            this.tbAnalysis = new System.Windows.Forms.TabPage();
            this.lblInitialAnalysisPrompt = new System.Windows.Forms.Label();
            this.btnAnalysisFolder = new System.Windows.Forms.Button();
            this.txtFolderZipForAnalysis = new System.Windows.Forms.TextBox();
            this.lblFolderZipPrompt = new System.Windows.Forms.Label();
            this.tcAnalysis = new System.Windows.Forms.TabControl();
            this.tbProfilerTraces = new System.Windows.Forms.TabPage();
            this.btnImportProfilerTrace = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.ProfilerTraceStatusTextBox = new System.Windows.Forms.TextBox();
            this.splitProfilerAnalysis = new System.Windows.Forms.SplitContainer();
            this.cmbProfilerAnalyses = new System.Windows.Forms.ComboBox();
            this.txtProfilerAnalysisDescription = new System.Windows.Forms.TextBox();
            this.lblAnalysisQueries = new System.Windows.Forms.Label();
            this.pnlProfilerAnalysisStatus = new System.Windows.Forms.Panel();
            this.lblProfilerAnalysisStatusCenter = new System.Windows.Forms.Label();
            this.lblProfilerAnalysisStatusLeft = new System.Windows.Forms.Label();
            this.lblProfilerAnalysisStatusRight = new System.Windows.Forms.Label();
            this.chkDettachProfilerAnalysisDBWhenDone = new System.Windows.Forms.CheckBox();
            this.dgdProfilerAnalyses = new System.Windows.Forms.DataGridView();
            this.imgAnalyzerIcons = new System.Windows.Forms.ImageList(this.components);
            this.btnSettings = new System.Windows.Forms.Button();
            this.ctxSettings = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.automaticallyCheckForUpdatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.enableAnonymousUsageStatisticCollectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.enableOpenWithToolStripItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tcCollectionAnalysisTabs.SuspendLayout();
            this.tbCollection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitCollectionUI)).BeginInit();
            this.splitCollectionUI.Panel1.SuspendLayout();
            this.splitCollectionUI.Panel2.SuspendLayout();
            this.splitCollectionUI.SuspendLayout();
            this.grpDiagsToCapture.SuspendLayout();
            this.tcSimpleAdvanced.SuspendLayout();
            this.tabGuided.SuspendLayout();
            this.pnlSimple.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbLevelOfData)).BeginInit();
            this.tabAdvanced.SuspendLayout();
            this.pnlDiagnosticsToCollect.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udInterval)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udRollover)).BeginInit();
            this.tbAnalysis.SuspendLayout();
            this.tcAnalysis.SuspendLayout();
            this.tbProfilerTraces.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitProfilerAnalysis)).BeginInit();
            this.splitProfilerAnalysis.Panel1.SuspendLayout();
            this.splitProfilerAnalysis.Panel2.SuspendLayout();
            this.splitProfilerAnalysis.SuspendLayout();
            this.pnlProfilerAnalysisStatus.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgdProfilerAnalyses)).BeginInit();
            this.ctxSettings.SuspendLayout();
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
            this.lkFeedback.Location = new System.Drawing.Point(1, 479);
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
            this.lkBugs.Location = new System.Drawing.Point(84, 479);
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
            this.lkDiscussion.Location = new System.Drawing.Point(143, 479);
            this.lkDiscussion.Name = "lkDiscussion";
            this.lkDiscussion.Padding = new System.Windows.Forms.Padding(2);
            this.lkDiscussion.Size = new System.Drawing.Size(111, 17);
            this.lkDiscussion.TabIndex = 20;
            this.lkDiscussion.TabStop = true;
            this.lkDiscussion.Text = "      Discussion/Ideas";
            this.ttStatus.SetToolTip(this.lkDiscussion, "Make it better!");
            this.lkDiscussion.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lkDiscussion_LinkClicked);
            // 
            // lkAbout
            // 
            this.lkAbout.AutoSize = true;
            this.lkAbout.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lkAbout.ForeColor = System.Drawing.SystemColors.Highlight;
            this.lkAbout.Image = ((System.Drawing.Image)(resources.GetObject("lkAbout.Image")));
            this.lkAbout.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lkAbout.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.lkAbout.Location = new System.Drawing.Point(260, 479);
            this.lkAbout.Name = "lkAbout";
            this.lkAbout.Padding = new System.Windows.Forms.Padding(2);
            this.lkAbout.Size = new System.Drawing.Size(57, 17);
            this.lkAbout.TabIndex = 22;
            this.lkAbout.TabStop = true;
            this.lkAbout.Text = "      About";
            this.ttStatus.SetToolTip(this.lkAbout, "Who we are");
            this.lkAbout.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lkAbout_LinkClicked);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(2, 47);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(121, 13);
            this.label3.TabIndex = 66;
            this.label3.Text = "Data collection location:";
            this.ttStatus.SetToolTip(this.label3, resources.GetString("label3.ToolTip"));
            // 
            // txtSaveLocation
            // 
            this.txtSaveLocation.Location = new System.Drawing.Point(26, 63);
            this.txtSaveLocation.Name = "txtSaveLocation";
            this.txtSaveLocation.ReadOnly = true;
            this.txtSaveLocation.Size = new System.Drawing.Size(231, 20);
            this.txtSaveLocation.TabIndex = 65;
            this.ttStatus.SetToolTip(this.txtSaveLocation, resources.GetString("txtSaveLocation.ToolTip"));
            // 
            // btnSaveLocation
            // 
            this.btnSaveLocation.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSaveLocation.Location = new System.Drawing.Point(3, 64);
            this.btnSaveLocation.Margin = new System.Windows.Forms.Padding(0);
            this.btnSaveLocation.Name = "btnSaveLocation";
            this.btnSaveLocation.Size = new System.Drawing.Size(22, 19);
            this.btnSaveLocation.TabIndex = 64;
            this.btnSaveLocation.Text = "...";
            this.ttStatus.SetToolTip(this.btnSaveLocation, resources.GetString("btnSaveLocation.ToolTip"));
            this.btnSaveLocation.UseVisualStyleBackColor = true;
            this.btnSaveLocation.Click += new System.EventHandler(this.btnSaveLocation_Click);
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
            this.chkProfilerPerfDetails.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkProfilerPerfDetails.Location = new System.Drawing.Point(23, 71);
            this.chkProfilerPerfDetails.Name = "chkProfilerPerfDetails";
            this.chkProfilerPerfDetails.Size = new System.Drawing.Size(160, 17);
            this.chkProfilerPerfDetails.TabIndex = 3;
            this.chkProfilerPerfDetails.Text = "Verbose performance details";
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
            // btnHangDumps
            // 
            this.btnHangDumps.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.btnHangDumps.FlatAppearance.BorderColor = System.Drawing.SystemColors.ScrollBar;
            this.btnHangDumps.FlatAppearance.BorderSize = 2;
            this.btnHangDumps.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.HotTrack;
            this.btnHangDumps.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnHangDumps.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnHangDumps.Font = new System.Drawing.Font("Consolas", 6.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnHangDumps.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnHangDumps.Image = global::SSASDiag.Properties.Resources.Dump;
            this.btnHangDumps.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnHangDumps.Location = new System.Drawing.Point(271, 168);
            this.btnHangDumps.Margin = new System.Windows.Forms.Padding(0);
            this.btnHangDumps.Name = "btnHangDumps";
            this.btnHangDumps.Size = new System.Drawing.Size(66, 50);
            this.btnHangDumps.TabIndex = 67;
            this.btnHangDumps.Text = "Capture Dumps Now";
            this.btnHangDumps.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.ttStatus.SetToolTip(this.btnHangDumps, "Captures 3 consecutive hang dumps from the service on-demand when clicked.");
            this.btnHangDumps.UseVisualStyleBackColor = false;
            this.btnHangDumps.Visible = false;
            this.btnHangDumps.Click += new System.EventHandler(this.btnHangDumps_Click);
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
            this.tbCollection.Controls.Add(this.splitCollectionUI);
            this.tbCollection.Location = new System.Drawing.Point(4, 22);
            this.tbCollection.Name = "tbCollection";
            this.tbCollection.Size = new System.Drawing.Size(593, 439);
            this.tbCollection.TabIndex = 0;
            this.tbCollection.Text = "Collection";
            // 
            // splitCollectionUI
            // 
            this.splitCollectionUI.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitCollectionUI.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitCollectionUI.Location = new System.Drawing.Point(0, 0);
            this.splitCollectionUI.Name = "splitCollectionUI";
            this.splitCollectionUI.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitCollectionUI.Panel1
            // 
            this.splitCollectionUI.Panel1.Controls.Add(this.btnHangDumps);
            this.splitCollectionUI.Panel1.Controls.Add(this.label3);
            this.splitCollectionUI.Panel1.Controls.Add(this.txtSaveLocation);
            this.splitCollectionUI.Panel1.Controls.Add(this.btnSaveLocation);
            this.splitCollectionUI.Panel1.Controls.Add(this.chkDeleteRaw);
            this.splitCollectionUI.Panel1.Controls.Add(this.chkZip);
            this.splitCollectionUI.Panel1.Controls.Add(this.grpDiagsToCapture);
            this.splitCollectionUI.Panel1.Controls.Add(this.dtStopTime);
            this.splitCollectionUI.Panel1.Controls.Add(this.chkStopTime);
            this.splitCollectionUI.Panel1.Controls.Add(this.dtStartTime);
            this.splitCollectionUI.Panel1.Controls.Add(this.chkStartTime);
            this.splitCollectionUI.Panel1.Controls.Add(this.udInterval);
            this.splitCollectionUI.Panel1.Controls.Add(this.udRollover);
            this.splitCollectionUI.Panel1.Controls.Add(this.chkAutoRestart);
            this.splitCollectionUI.Panel1.Controls.Add(this.chkRollover);
            this.splitCollectionUI.Panel1.Controls.Add(this.btnCapture);
            this.splitCollectionUI.Panel1.Controls.Add(this.label1);
            this.splitCollectionUI.Panel1.Controls.Add(this.lblInstanceDetails);
            this.splitCollectionUI.Panel1.Controls.Add(this.cbInstances);
            this.splitCollectionUI.Panel1.Controls.Add(this.label2);
            this.splitCollectionUI.Panel1.Controls.Add(this.lblInterval);
            this.splitCollectionUI.Panel1.Controls.Add(this.lblInterval2);
            this.splitCollectionUI.Panel1.Resize += new System.EventHandler(this.splitCollectionUI_Panel1_Resize);
            this.splitCollectionUI.Panel1MinSize = 220;
            // 
            // splitCollectionUI.Panel2
            // 
            this.splitCollectionUI.Panel2.Controls.Add(this.txtStatus);
            this.splitCollectionUI.Panel2MinSize = 26;
            this.splitCollectionUI.Size = new System.Drawing.Size(593, 439);
            this.splitCollectionUI.SplitterDistance = 220;
            this.splitCollectionUI.SplitterIncrement = 13;
            this.splitCollectionUI.TabIndex = 46;
            // 
            // chkDeleteRaw
            // 
            this.chkDeleteRaw.AutoSize = true;
            this.chkDeleteRaw.Location = new System.Drawing.Point(117, 89);
            this.chkDeleteRaw.Name = "chkDeleteRaw";
            this.chkDeleteRaw.Size = new System.Drawing.Size(144, 17);
            this.chkDeleteRaw.TabIndex = 48;
            this.chkDeleteRaw.Text = "Delete raw data after zip.";
            this.chkDeleteRaw.UseVisualStyleBackColor = true;
            this.chkDeleteRaw.CheckedChanged += new System.EventHandler(this.chkDeleteRaw_CheckedChanged);
            // 
            // chkZip
            // 
            this.chkZip.AutoSize = true;
            this.chkZip.Checked = true;
            this.chkZip.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkZip.Location = new System.Drawing.Point(5, 89);
            this.chkZip.Name = "chkZip";
            this.chkZip.Size = new System.Drawing.Size(106, 17);
            this.chkZip.TabIndex = 47;
            this.chkZip.Text = "Compress to .zip.";
            this.chkZip.UseVisualStyleBackColor = true;
            this.chkZip.CheckedChanged += new System.EventHandler(this.chkZip_CheckedChanged);
            // 
            // grpDiagsToCapture
            // 
            this.grpDiagsToCapture.Controls.Add(this.tcSimpleAdvanced);
            this.grpDiagsToCapture.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.grpDiagsToCapture.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpDiagsToCapture.Location = new System.Drawing.Point(351, 50);
            this.grpDiagsToCapture.Margin = new System.Windows.Forms.Padding(0);
            this.grpDiagsToCapture.Name = "grpDiagsToCapture";
            this.grpDiagsToCapture.Padding = new System.Windows.Forms.Padding(0);
            this.grpDiagsToCapture.Size = new System.Drawing.Size(239, 171);
            this.grpDiagsToCapture.TabIndex = 57;
            this.grpDiagsToCapture.TabStop = false;
            this.grpDiagsToCapture.Text = "Diagnostics to Capture:";
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
            this.tcSimpleAdvanced.Size = new System.Drawing.Size(239, 158);
            this.tcSimpleAdvanced.TabIndex = 1;
            this.tcSimpleAdvanced.SelectedIndexChanged += new System.EventHandler(this.tcSimpleAdvanced_SelectedIndexChanged);
            // 
            // tabGuided
            // 
            this.tabGuided.BackColor = System.Drawing.SystemColors.Control;
            this.tabGuided.Controls.Add(this.pnlSimple);
            this.tabGuided.Location = new System.Drawing.Point(4, 22);
            this.tabGuided.Margin = new System.Windows.Forms.Padding(0);
            this.tabGuided.Name = "tabGuided";
            this.tabGuided.Size = new System.Drawing.Size(231, 132);
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
            this.pnlSimple.Size = new System.Drawing.Size(231, 132);
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
            "",
            "Performance",
            "Errors/Hangs (non-connectivity)",
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
            this.tabAdvanced.Size = new System.Drawing.Size(231, 132);
            this.tabAdvanced.TabIndex = 1;
            this.tabAdvanced.Text = "Advanced";
            // 
            // pnlDiagnosticsToCollect
            // 
            this.pnlDiagnosticsToCollect.AutoScroll = true;
            this.pnlDiagnosticsToCollect.BackColor = System.Drawing.SystemColors.Control;
            this.pnlDiagnosticsToCollect.Controls.Add(this.chkHangDumps);
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
            this.pnlDiagnosticsToCollect.Size = new System.Drawing.Size(231, 132);
            this.pnlDiagnosticsToCollect.TabIndex = 38;
            // 
            // chkHangDumps
            // 
            this.chkHangDumps.AutoSize = true;
            this.chkHangDumps.Location = new System.Drawing.Point(3, 186);
            this.chkHangDumps.Name = "chkHangDumps";
            this.chkHangDumps.Size = new System.Drawing.Size(154, 17);
            this.chkHangDumps.TabIndex = 8;
            this.chkHangDumps.Text = "Enable hang dump capture";
            this.chkHangDumps.UseVisualStyleBackColor = true;
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
            this.dtStopTime.Location = new System.Drawing.Point(76, 157);
            this.dtStopTime.Name = "dtStopTime";
            this.dtStopTime.Size = new System.Drawing.Size(181, 20);
            this.dtStopTime.TabIndex = 54;
            // 
            // chkStopTime
            // 
            this.chkStopTime.AutoSize = true;
            this.chkStopTime.Location = new System.Drawing.Point(5, 158);
            this.chkStopTime.Name = "chkStopTime";
            this.chkStopTime.Size = new System.Drawing.Size(73, 17);
            this.chkStopTime.TabIndex = 53;
            this.chkStopTime.Text = "Stop time:";
            this.chkStopTime.UseVisualStyleBackColor = true;
            this.chkStopTime.CheckedChanged += new System.EventHandler(this.chkStopTime_CheckedChanged);
            // 
            // dtStartTime
            // 
            this.dtStartTime.CustomFormat = "MM/dd/yyyy HH:mm:ss UTC";
            this.dtStartTime.Enabled = false;
            this.dtStartTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtStartTime.Location = new System.Drawing.Point(76, 134);
            this.dtStartTime.Name = "dtStartTime";
            this.dtStartTime.Size = new System.Drawing.Size(181, 20);
            this.dtStartTime.TabIndex = 52;
            // 
            // chkStartTime
            // 
            this.chkStartTime.AutoSize = true;
            this.chkStartTime.Location = new System.Drawing.Point(5, 135);
            this.chkStartTime.Name = "chkStartTime";
            this.chkStartTime.Size = new System.Drawing.Size(73, 17);
            this.chkStartTime.TabIndex = 51;
            this.chkStartTime.Text = "Start time:";
            this.chkStartTime.UseVisualStyleBackColor = true;
            this.chkStartTime.CheckedChanged += new System.EventHandler(this.chkStartTime_CheckedChanged);
            // 
            // udInterval
            // 
            this.udInterval.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.udInterval.Location = new System.Drawing.Point(152, 201);
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
            this.udInterval.Size = new System.Drawing.Size(45, 18);
            this.udInterval.TabIndex = 56;
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
            this.udRollover.Location = new System.Drawing.Point(165, 110);
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
            this.udRollover.TabIndex = 50;
            this.udRollover.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.udRollover.ThousandsSeparator = true;
            this.udRollover.UpDownAlign = System.Windows.Forms.LeftRightAlignment.Left;
            this.udRollover.Value = new decimal(new int[] {
            64,
            0,
            0,
            0});
            // 
            // chkAutoRestart
            // 
            this.chkAutoRestart.AutoSize = true;
            this.chkAutoRestart.Location = new System.Drawing.Point(5, 181);
            this.chkAutoRestart.Name = "chkAutoRestart";
            this.chkAutoRestart.Size = new System.Drawing.Size(206, 17);
            this.chkAutoRestart.TabIndex = 55;
            this.chkAutoRestart.Text = "Restart profiler trace if service restarts.";
            this.chkAutoRestart.UseVisualStyleBackColor = true;
            this.chkAutoRestart.CheckedChanged += new System.EventHandler(this.chkAutoRestart_CheckedChanged);
            // 
            // chkRollover
            // 
            this.chkRollover.AutoSize = true;
            this.chkRollover.Checked = true;
            this.chkRollover.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkRollover.Location = new System.Drawing.Point(5, 112);
            this.chkRollover.Name = "chkRollover";
            this.chkRollover.Size = new System.Drawing.Size(162, 17);
            this.chkRollover.TabIndex = 49;
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
            this.btnCapture.Location = new System.Drawing.Point(269, 76);
            this.btnCapture.Name = "btnCapture";
            this.btnCapture.Size = new System.Drawing.Size(68, 68);
            this.btnCapture.TabIndex = 58;
            this.btnCapture.UseVisualStyleBackColor = false;
            this.btnCapture.Click += new System.EventHandler(this.btnCapture_Click);
            this.btnCapture.MouseEnter += new System.EventHandler(this.btnCapture_MouseEnter);
            this.btnCapture.MouseLeave += new System.EventHandler(this.btnCapture_MouseLeave);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(2, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(238, 13);
            this.label1.TabIndex = 61;
            this.label1.Text = "Select an SSAS instance from the local machine:";
            // 
            // lblInstanceDetails
            // 
            this.lblInstanceDetails.AutoSize = true;
            this.lblInstanceDetails.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInstanceDetails.Location = new System.Drawing.Point(271, 20);
            this.lblInstanceDetails.Name = "lblInstanceDetails";
            this.lblInstanceDetails.Size = new System.Drawing.Size(0, 12);
            this.lblInstanceDetails.TabIndex = 62;
            // 
            // cbInstances
            // 
            this.cbInstances.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbInstances.FormattingEnabled = true;
            this.cbInstances.Location = new System.Drawing.Point(5, 23);
            this.cbInstances.Name = "cbInstances";
            this.cbInstances.Size = new System.Drawing.Size(252, 21);
            this.cbInstances.TabIndex = 46;
            this.cbInstances.SelectedIndexChanged += new System.EventHandler(this.cbInstances_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(221, 113);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 63;
            this.label2.Text = "MB.";
            // 
            // lblInterval
            // 
            this.lblInterval.AutoSize = true;
            this.lblInterval.Location = new System.Drawing.Point(22, 204);
            this.lblInterval.Name = "lblInterval";
            this.lblInterval.Size = new System.Drawing.Size(124, 13);
            this.lblInterval.TabIndex = 60;
            this.lblInterval.Text = "Performance log interval:";
            // 
            // lblInterval2
            // 
            this.lblInterval2.AutoSize = true;
            this.lblInterval2.Location = new System.Drawing.Point(204, 204);
            this.lblInterval2.Name = "lblInterval2";
            this.lblInterval2.Size = new System.Drawing.Size(50, 13);
            this.lblInterval2.TabIndex = 59;
            this.lblInterval2.Text = "seconds.";
            // 
            // txtStatus
            // 
            this.txtStatus.BackColor = System.Drawing.Color.Black;
            this.txtStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtStatus.DetectUrls = false;
            this.txtStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtStatus.Font = new System.Drawing.Font("Consolas", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtStatus.ForeColor = System.Drawing.Color.LightSkyBlue;
            this.txtStatus.Location = new System.Drawing.Point(0, 0);
            this.txtStatus.Name = "txtStatus";
            this.txtStatus.ReadOnly = true;
            this.txtStatus.Size = new System.Drawing.Size(593, 215);
            this.txtStatus.TabIndex = 37;
            this.txtStatus.Text = "";
            this.txtStatus.SizeChanged += new System.EventHandler(this.txtStatus_SizeChanged);
            this.txtStatus.MouseDown += new System.Windows.Forms.MouseEventHandler(this.txtStatus_MouseDown);
            // 
            // tbAnalysis
            // 
            this.tbAnalysis.BackColor = System.Drawing.SystemColors.Control;
            this.tbAnalysis.Controls.Add(this.lblInitialAnalysisPrompt);
            this.tbAnalysis.Controls.Add(this.btnAnalysisFolder);
            this.tbAnalysis.Controls.Add(this.txtFolderZipForAnalysis);
            this.tbAnalysis.Controls.Add(this.lblFolderZipPrompt);
            this.tbAnalysis.Controls.Add(this.tcAnalysis);
            this.tbAnalysis.Location = new System.Drawing.Point(4, 22);
            this.tbAnalysis.Margin = new System.Windows.Forms.Padding(0);
            this.tbAnalysis.Name = "tbAnalysis";
            this.tbAnalysis.Padding = new System.Windows.Forms.Padding(3);
            this.tbAnalysis.Size = new System.Drawing.Size(593, 439);
            this.tbAnalysis.TabIndex = 1;
            this.tbAnalysis.Text = "Analysis";
            // 
            // lblInitialAnalysisPrompt
            // 
            this.lblInitialAnalysisPrompt.AutoSize = true;
            this.lblInitialAnalysisPrompt.BackColor = System.Drawing.SystemColors.Window;
            this.lblInitialAnalysisPrompt.Location = new System.Drawing.Point(10, 64);
            this.lblInitialAnalysisPrompt.Name = "lblInitialAnalysisPrompt";
            this.lblInitialAnalysisPrompt.Size = new System.Drawing.Size(298, 104);
            this.lblInitialAnalysisPrompt.TabIndex = 34;
            this.lblInitialAnalysisPrompt.Text = resources.GetString("lblInitialAnalysisPrompt.Text");
            this.lblInitialAnalysisPrompt.Visible = false;
            // 
            // btnAnalysisFolder
            // 
            this.btnAnalysisFolder.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAnalysisFolder.Location = new System.Drawing.Point(115, 5);
            this.btnAnalysisFolder.Name = "btnAnalysisFolder";
            this.btnAnalysisFolder.Size = new System.Drawing.Size(22, 19);
            this.btnAnalysisFolder.TabIndex = 0;
            this.btnAnalysisFolder.Text = "...";
            this.btnAnalysisFolder.UseVisualStyleBackColor = true;
            this.btnAnalysisFolder.Click += new System.EventHandler(this.btnAnalysisFolder_Click);
            // 
            // txtFolderZipForAnalysis
            // 
            this.txtFolderZipForAnalysis.Location = new System.Drawing.Point(137, 4);
            this.txtFolderZipForAnalysis.Name = "txtFolderZipForAnalysis";
            this.txtFolderZipForAnalysis.ReadOnly = true;
            this.txtFolderZipForAnalysis.Size = new System.Drawing.Size(453, 20);
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
            // tcAnalysis
            // 
            this.tcAnalysis.Controls.Add(this.tbProfilerTraces);
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
            // tbProfilerTraces
            // 
            this.tbProfilerTraces.BackColor = System.Drawing.SystemColors.Control;
            this.tbProfilerTraces.Controls.Add(this.btnImportProfilerTrace);
            this.tbProfilerTraces.Controls.Add(this.label4);
            this.tbProfilerTraces.Controls.Add(this.ProfilerTraceStatusTextBox);
            this.tbProfilerTraces.Controls.Add(this.splitProfilerAnalysis);
            this.tbProfilerTraces.ImageIndex = 5;
            this.tbProfilerTraces.Location = new System.Drawing.Point(4, 30);
            this.tbProfilerTraces.Name = "tbProfilerTraces";
            this.tbProfilerTraces.Size = new System.Drawing.Size(579, 371);
            this.tbProfilerTraces.TabIndex = 0;
            this.tbProfilerTraces.Text = "Profiler Traces";
            // 
            // btnImportProfilerTrace
            // 
            this.btnImportProfilerTrace.Location = new System.Drawing.Point(235, 90);
            this.btnImportProfilerTrace.Name = "btnImportProfilerTrace";
            this.btnImportProfilerTrace.Size = new System.Drawing.Size(108, 26);
            this.btnImportProfilerTrace.TabIndex = 33;
            this.btnImportProfilerTrace.Text = "Import and &Analyze";
            this.btnImportProfilerTrace.Click += new System.EventHandler(this.btnImportProfilerTrace_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(289, 167);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(0, 12);
            this.label4.TabIndex = 32;
            // 
            // ProfilerTraceStatusTextBox
            // 
            this.ProfilerTraceStatusTextBox.BackColor = System.Drawing.Color.Black;
            this.ProfilerTraceStatusTextBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.ProfilerTraceStatusTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ProfilerTraceStatusTextBox.ForeColor = System.Drawing.Color.LightSkyBlue;
            this.ProfilerTraceStatusTextBox.Location = new System.Drawing.Point(0, 0);
            this.ProfilerTraceStatusTextBox.Multiline = true;
            this.ProfilerTraceStatusTextBox.Name = "ProfilerTraceStatusTextBox";
            this.ProfilerTraceStatusTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.ProfilerTraceStatusTextBox.Size = new System.Drawing.Size(579, 78);
            this.ProfilerTraceStatusTextBox.TabIndex = 0;
            // 
            // splitProfilerAnalysis
            // 
            this.splitProfilerAnalysis.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.splitProfilerAnalysis.Location = new System.Drawing.Point(0, 79);
            this.splitProfilerAnalysis.Name = "splitProfilerAnalysis";
            this.splitProfilerAnalysis.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitProfilerAnalysis.Panel1
            // 
            this.splitProfilerAnalysis.Panel1.Controls.Add(this.cmbProfilerAnalyses);
            this.splitProfilerAnalysis.Panel1.Controls.Add(this.txtProfilerAnalysisDescription);
            this.splitProfilerAnalysis.Panel1.Controls.Add(this.lblAnalysisQueries);
            this.splitProfilerAnalysis.Panel1.SizeChanged += new System.EventHandler(this.splitProfilerAnalysis_Panel1_SizeChanged);
            this.splitProfilerAnalysis.Panel1MinSize = 40;
            // 
            // splitProfilerAnalysis.Panel2
            // 
            this.splitProfilerAnalysis.Panel2.Controls.Add(this.pnlProfilerAnalysisStatus);
            this.splitProfilerAnalysis.Panel2.Controls.Add(this.dgdProfilerAnalyses);
            this.splitProfilerAnalysis.Panel2.SizeChanged += new System.EventHandler(this.splitProfilerAnalysis_Panel2_SizeChanged);
            this.splitProfilerAnalysis.Size = new System.Drawing.Size(579, 292);
            this.splitProfilerAnalysis.SplitterDistance = 103;
            this.splitProfilerAnalysis.TabIndex = 28;
            // 
            // cmbProfilerAnalyses
            // 
            this.cmbProfilerAnalyses.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbProfilerAnalyses.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbProfilerAnalyses.FormattingEnabled = true;
            this.cmbProfilerAnalyses.Location = new System.Drawing.Point(3, 17);
            this.cmbProfilerAnalyses.Name = "cmbProfilerAnalyses";
            this.cmbProfilerAnalyses.Size = new System.Drawing.Size(209, 21);
            this.cmbProfilerAnalyses.TabIndex = 28;
            this.cmbProfilerAnalyses.SelectedIndexChanged += new System.EventHandler(this.cmbProfilerAnalyses_SelectedIndexChanged);
            // 
            // txtProfilerAnalysisDescription
            // 
            this.txtProfilerAnalysisDescription.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtProfilerAnalysisDescription.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtProfilerAnalysisDescription.Location = new System.Drawing.Point(3, 44);
            this.txtProfilerAnalysisDescription.Multiline = true;
            this.txtProfilerAnalysisDescription.Name = "txtProfilerAnalysisDescription";
            this.txtProfilerAnalysisDescription.ReadOnly = true;
            this.txtProfilerAnalysisDescription.Size = new System.Drawing.Size(209, 47);
            this.txtProfilerAnalysisDescription.TabIndex = 31;
            // 
            // lblAnalysisQueries
            // 
            this.lblAnalysisQueries.AutoSize = true;
            this.lblAnalysisQueries.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAnalysisQueries.Location = new System.Drawing.Point(3, 0);
            this.lblAnalysisQueries.Name = "lblAnalysisQueries";
            this.lblAnalysisQueries.Size = new System.Drawing.Size(130, 13);
            this.lblAnalysisQueries.TabIndex = 30;
            this.lblAnalysisQueries.Text = "Choose an analysis query:";
            // 
            // pnlProfilerAnalysisStatus
            // 
            this.pnlProfilerAnalysisStatus.BackColor = System.Drawing.Color.Black;
            this.pnlProfilerAnalysisStatus.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnlProfilerAnalysisStatus.Controls.Add(this.lblProfilerAnalysisStatusCenter);
            this.pnlProfilerAnalysisStatus.Controls.Add(this.lblProfilerAnalysisStatusLeft);
            this.pnlProfilerAnalysisStatus.Controls.Add(this.lblProfilerAnalysisStatusRight);
            this.pnlProfilerAnalysisStatus.Controls.Add(this.chkDettachProfilerAnalysisDBWhenDone);
            this.pnlProfilerAnalysisStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlProfilerAnalysisStatus.Location = new System.Drawing.Point(0, 150);
            this.pnlProfilerAnalysisStatus.Name = "pnlProfilerAnalysisStatus";
            this.pnlProfilerAnalysisStatus.Size = new System.Drawing.Size(579, 35);
            this.pnlProfilerAnalysisStatus.TabIndex = 35;
            // 
            // lblProfilerAnalysisStatusCenter
            // 
            this.lblProfilerAnalysisStatusCenter.AutoSize = true;
            this.lblProfilerAnalysisStatusCenter.BackColor = System.Drawing.Color.Transparent;
            this.lblProfilerAnalysisStatusCenter.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblProfilerAnalysisStatusCenter.ForeColor = System.Drawing.Color.LightSkyBlue;
            this.lblProfilerAnalysisStatusCenter.Location = new System.Drawing.Point(279, 0);
            this.lblProfilerAnalysisStatusCenter.Name = "lblProfilerAnalysisStatusCenter";
            this.lblProfilerAnalysisStatusCenter.Size = new System.Drawing.Size(0, 12);
            this.lblProfilerAnalysisStatusCenter.TabIndex = 36;
            this.lblProfilerAnalysisStatusCenter.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblProfilerAnalysisStatusLeft
            // 
            this.lblProfilerAnalysisStatusLeft.AutoSize = true;
            this.lblProfilerAnalysisStatusLeft.BackColor = System.Drawing.Color.Transparent;
            this.lblProfilerAnalysisStatusLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblProfilerAnalysisStatusLeft.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblProfilerAnalysisStatusLeft.ForeColor = System.Drawing.Color.LightSkyBlue;
            this.lblProfilerAnalysisStatusLeft.Location = new System.Drawing.Point(0, 0);
            this.lblProfilerAnalysisStatusLeft.Name = "lblProfilerAnalysisStatusLeft";
            this.lblProfilerAnalysisStatusLeft.Size = new System.Drawing.Size(0, 12);
            this.lblProfilerAnalysisStatusLeft.TabIndex = 35;
            // 
            // lblProfilerAnalysisStatusRight
            // 
            this.lblProfilerAnalysisStatusRight.AutoSize = true;
            this.lblProfilerAnalysisStatusRight.BackColor = System.Drawing.Color.Transparent;
            this.lblProfilerAnalysisStatusRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblProfilerAnalysisStatusRight.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblProfilerAnalysisStatusRight.ForeColor = System.Drawing.Color.LightSkyBlue;
            this.lblProfilerAnalysisStatusRight.Location = new System.Drawing.Point(575, 0);
            this.lblProfilerAnalysisStatusRight.Name = "lblProfilerAnalysisStatusRight";
            this.lblProfilerAnalysisStatusRight.Size = new System.Drawing.Size(0, 12);
            this.lblProfilerAnalysisStatusRight.TabIndex = 34;
            this.lblProfilerAnalysisStatusRight.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // chkDettachProfilerAnalysisDBWhenDone
            // 
            this.chkDettachProfilerAnalysisDBWhenDone.BackColor = System.Drawing.Color.Transparent;
            this.chkDettachProfilerAnalysisDBWhenDone.Checked = true;
            this.chkDettachProfilerAnalysisDBWhenDone.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDettachProfilerAnalysisDBWhenDone.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.chkDettachProfilerAnalysisDBWhenDone.FlatAppearance.BorderSize = 0;
            this.chkDettachProfilerAnalysisDBWhenDone.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkDettachProfilerAnalysisDBWhenDone.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkDettachProfilerAnalysisDBWhenDone.ForeColor = System.Drawing.Color.LightSkyBlue;
            this.chkDettachProfilerAnalysisDBWhenDone.Location = new System.Drawing.Point(0, 15);
            this.chkDettachProfilerAnalysisDBWhenDone.Name = "chkDettachProfilerAnalysisDBWhenDone";
            this.chkDettachProfilerAnalysisDBWhenDone.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.chkDettachProfilerAnalysisDBWhenDone.Size = new System.Drawing.Size(575, 16);
            this.chkDettachProfilerAnalysisDBWhenDone.TabIndex = 30;
            this.chkDettachProfilerAnalysisDBWhenDone.Text = "Detach trace database imported for analysis when leaving analysis pane or exiting" +
    " tool";
            this.chkDettachProfilerAnalysisDBWhenDone.UseVisualStyleBackColor = true;
            // 
            // dgdProfilerAnalyses
            // 
            this.dgdProfilerAnalyses.AllowUserToAddRows = false;
            this.dgdProfilerAnalyses.AllowUserToDeleteRows = false;
            this.dgdProfilerAnalyses.AllowUserToOrderColumns = true;
            this.dgdProfilerAnalyses.AllowUserToResizeRows = false;
            dataGridViewCellStyle7.BackColor = System.Drawing.Color.AliceBlue;
            this.dgdProfilerAnalyses.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle7;
            this.dgdProfilerAnalyses.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
            this.dgdProfilerAnalyses.BackgroundColor = System.Drawing.SystemColors.ControlDark;
            this.dgdProfilerAnalyses.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgdProfilerAnalyses.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.dgdProfilerAnalyses.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle8.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle8.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle8.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle8.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle8.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgdProfilerAnalyses.DefaultCellStyle = dataGridViewCellStyle8;
            this.dgdProfilerAnalyses.Dock = System.Windows.Forms.DockStyle.Top;
            this.dgdProfilerAnalyses.Location = new System.Drawing.Point(0, 0);
            this.dgdProfilerAnalyses.Margin = new System.Windows.Forms.Padding(0);
            this.dgdProfilerAnalyses.Name = "dgdProfilerAnalyses";
            this.dgdProfilerAnalyses.ReadOnly = true;
            this.dgdProfilerAnalyses.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dgdProfilerAnalyses.RowHeadersWidth = 16;
            this.dgdProfilerAnalyses.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.dgdProfilerAnalyses.RowTemplate.Height = 18;
            this.dgdProfilerAnalyses.RowTemplate.ReadOnly = true;
            this.dgdProfilerAnalyses.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dgdProfilerAnalyses.Size = new System.Drawing.Size(579, 152);
            this.dgdProfilerAnalyses.TabIndex = 25;
            this.dgdProfilerAnalyses.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgdProfilerAnalyses_CellMouseClick);
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
            // btnSettings
            // 
            this.btnSettings.FlatAppearance.BorderColor = System.Drawing.SystemColors.ButtonFace;
            this.btnSettings.FlatAppearance.BorderSize = 0;
            this.btnSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSettings.Image = ((System.Drawing.Image)(resources.GetObject("btnSettings.Image")));
            this.btnSettings.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSettings.Location = new System.Drawing.Point(535, -2);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(68, 22);
            this.btnSettings.TabIndex = 48;
            this.btnSettings.Text = "Settings";
            this.btnSettings.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            this.btnSettings.UseVisualStyleBackColor = true;
            this.btnSettings.Click += new System.EventHandler(this.ctxSettings_Click);
            // 
            // ctxSettings
            // 
            this.ctxSettings.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.automaticallyCheckForUpdatesToolStripMenuItem,
            this.enableAnonymousUsageStatisticCollectionToolStripMenuItem,
            this.enableOpenWithToolStripItem});
            this.ctxSettings.Name = "contextMenuStrip1";
            this.ctxSettings.ShowCheckMargin = true;
            this.ctxSettings.ShowImageMargin = false;
            this.ctxSettings.Size = new System.Drawing.Size(313, 70);
            // 
            // automaticallyCheckForUpdatesToolStripMenuItem
            // 
            this.automaticallyCheckForUpdatesToolStripMenuItem.CheckOnClick = true;
            this.automaticallyCheckForUpdatesToolStripMenuItem.Name = "automaticallyCheckForUpdatesToolStripMenuItem";
            this.automaticallyCheckForUpdatesToolStripMenuItem.Size = new System.Drawing.Size(312, 22);
            this.automaticallyCheckForUpdatesToolStripMenuItem.Text = "Automatically check for updates";
            this.automaticallyCheckForUpdatesToolStripMenuItem.CheckedChanged += new System.EventHandler(this.chkAutoUpdate_CheckedChanged);
            // 
            // enableAnonymousUsageStatisticCollectionToolStripMenuItem
            // 
            this.enableAnonymousUsageStatisticCollectionToolStripMenuItem.CheckOnClick = true;
            this.enableAnonymousUsageStatisticCollectionToolStripMenuItem.Name = "enableAnonymousUsageStatisticCollectionToolStripMenuItem";
            this.enableAnonymousUsageStatisticCollectionToolStripMenuItem.Size = new System.Drawing.Size(312, 22);
            this.enableAnonymousUsageStatisticCollectionToolStripMenuItem.Text = "Enable anonymous usage statistic collection";
            this.enableAnonymousUsageStatisticCollectionToolStripMenuItem.ToolTipText = resources.GetString("enableAnonymousUsageStatisticCollectionToolStripMenuItem.ToolTipText");
            this.enableAnonymousUsageStatisticCollectionToolStripMenuItem.CheckedChanged += new System.EventHandler(this.chkAllowUsageStatsCollection_CheckedChanged);
            // 
            // enableOpenWithToolStripItem
            // 
            this.enableOpenWithToolStripItem.Checked = true;
            this.enableOpenWithToolStripItem.CheckOnClick = true;
            this.enableOpenWithToolStripItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.enableOpenWithToolStripItem.Name = "enableOpenWithToolStripItem";
            this.enableOpenWithToolStripItem.Size = new System.Drawing.Size(312, 22);
            this.enableOpenWithToolStripItem.Text = "Enable context menu to Open With SSASDiag";
            this.enableOpenWithToolStripItem.ToolTipText = "Current supported file types include .zip, .trc, .cap, .etl.";
            this.enableOpenWithToolStripItem.CheckedChanged += new System.EventHandler(this.enableOpenWithToolStripItem_CheckedChanged);
            // 
            // frmSSASDiag
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(601, 496);
            this.Controls.Add(this.btnSettings);
            this.Controls.Add(this.lkAbout);
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
            this.Shown += new System.EventHandler(this.frmSSASDiag_Shown);
            this.ResizeBegin += new System.EventHandler(this.frmSSASDiag_ResizeBegin);
            this.ResizeEnd += new System.EventHandler(this.frmSSASDiag_ResizeEnd);
            this.SizeChanged += new System.EventHandler(this.frmSSASDiag_SizeChanged);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.frmSSASDiag_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.frmSSASDiag_DragEnter);
            this.Resize += new System.EventHandler(this.frmSSASDiag_Resize);
            this.tcCollectionAnalysisTabs.ResumeLayout(false);
            this.tbCollection.ResumeLayout(false);
            this.splitCollectionUI.Panel1.ResumeLayout(false);
            this.splitCollectionUI.Panel1.PerformLayout();
            this.splitCollectionUI.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitCollectionUI)).EndInit();
            this.splitCollectionUI.ResumeLayout(false);
            this.grpDiagsToCapture.ResumeLayout(false);
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
            this.tcAnalysis.ResumeLayout(false);
            this.tbProfilerTraces.ResumeLayout(false);
            this.tbProfilerTraces.PerformLayout();
            this.splitProfilerAnalysis.Panel1.ResumeLayout(false);
            this.splitProfilerAnalysis.Panel1.PerformLayout();
            this.splitProfilerAnalysis.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitProfilerAnalysis)).EndInit();
            this.splitProfilerAnalysis.ResumeLayout(false);
            this.pnlProfilerAnalysisStatus.ResumeLayout(false);
            this.pnlProfilerAnalysisStatus.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgdProfilerAnalyses)).EndInit();
            this.ctxSettings.ResumeLayout(false);
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
        private System.Windows.Forms.TabPage tbAnalysis;
        private System.Windows.Forms.Button btnAnalysisFolder;
        private System.Windows.Forms.TextBox txtFolderZipForAnalysis;
        private System.Windows.Forms.Label lblFolderZipPrompt;
        private System.Windows.Forms.TabControl tcAnalysis;
        private System.Windows.Forms.ImageList imgAnalyzerIcons;
        private System.Windows.Forms.TabPage tbProfilerTraces;
        private System.Windows.Forms.TextBox ProfilerTraceStatusTextBox;
        private System.Windows.Forms.LinkLabel lkAbout;
        private System.Windows.Forms.SplitContainer splitProfilerAnalysis;
        private System.Windows.Forms.Label lblAnalysisQueries;
        private System.Windows.Forms.ComboBox cmbProfilerAnalyses;
        private System.Windows.Forms.DataGridView dgdProfilerAnalyses;
        private System.Windows.Forms.TextBox txtProfilerAnalysisDescription;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel pnlProfilerAnalysisStatus;
        private System.Windows.Forms.Label lblProfilerAnalysisStatusCenter;
        private System.Windows.Forms.Label lblProfilerAnalysisStatusLeft;
        private System.Windows.Forms.Label lblProfilerAnalysisStatusRight;
        private System.Windows.Forms.CheckBox chkDettachProfilerAnalysisDBWhenDone;
        private System.Windows.Forms.Button btnImportProfilerTrace;
        private System.Windows.Forms.SplitContainer splitCollectionUI;
        private System.Windows.Forms.RichTextBox txtStatus;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtSaveLocation;
        private System.Windows.Forms.Button btnSaveLocation;
        private System.Windows.Forms.CheckBox chkDeleteRaw;
        private System.Windows.Forms.CheckBox chkZip;
        private System.Windows.Forms.GroupBox grpDiagsToCapture;
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
        private System.Windows.Forms.NumericUpDown udInterval;
        private System.Windows.Forms.NumericUpDown udRollover;
        private System.Windows.Forms.CheckBox chkAutoRestart;
        private System.Windows.Forms.CheckBox chkRollover;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblInstanceDetails;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblInterval;
        private System.Windows.Forms.Label lblInterval2;
        public System.Windows.Forms.Button btnCapture;
        public System.Windows.Forms.ComboBox cbInstances;
        private System.Windows.Forms.Button btnHangDumps;
        private System.Windows.Forms.CheckBox chkHangDumps;
        private System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.ContextMenuStrip ctxSettings;
        private System.Windows.Forms.ToolStripMenuItem automaticallyCheckForUpdatesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem enableAnonymousUsageStatisticCollectionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem enableOpenWithToolStripItem;
        private System.Windows.Forms.Label lblInitialAnalysisPrompt;
    }
}

