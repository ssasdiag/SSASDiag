namespace SSASDiag
{
    partial class ucASPerfMonAnalyzer
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            this.splitLogList = new System.Windows.Forms.SplitContainer();
            this.splitLogDetails = new System.Windows.Forms.SplitContainer();
            this.dgdLogList = new System.Windows.Forms.DataGridView();
            this.rtLogDetails = new System.Windows.Forms.RichTextBox();
            this.btnAnalyzeLogs = new System.Windows.Forms.Button();
            this.splitAnalysis = new System.Windows.Forms.SplitContainer();
            this.splitPerfMonCountersAndChart = new System.Windows.Forms.SplitContainer();
            this.dgdGrouping = new System.Windows.Forms.DataGridView();
            this.Counter = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Instance = new System.Windows.Forms.DataGridViewTextBoxColumn();
            //this.tvCounters = new System.Windows.Forms.TreeView();
            this.tvCounters = new TriStateTreeView();
            this.chartPerfMon = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.pnServers = new System.Windows.Forms.Panel();
            this.lblCounterGrouping = new System.Windows.Forms.Label();
            this.cmbServers = new System.Windows.Forms.ComboBox();
            this.pnAnalyses = new System.Windows.Forms.Panel();
            this.lblAnalysesStatus = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitLogList)).BeginInit();
            this.splitLogList.Panel1.SuspendLayout();
            this.splitLogList.Panel2.SuspendLayout();
            this.splitLogList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitLogDetails)).BeginInit();
            this.splitLogDetails.Panel1.SuspendLayout();
            this.splitLogDetails.Panel2.SuspendLayout();
            this.splitLogDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgdLogList)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitAnalysis)).BeginInit();
            this.splitAnalysis.Panel1.SuspendLayout();
            this.splitAnalysis.Panel2.SuspendLayout();
            this.splitAnalysis.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitPerfMonCountersAndChart)).BeginInit();
            this.splitPerfMonCountersAndChart.Panel1.SuspendLayout();
            this.splitPerfMonCountersAndChart.Panel2.SuspendLayout();
            this.splitPerfMonCountersAndChart.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgdGrouping)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartPerfMon)).BeginInit();
            this.pnServers.SuspendLayout();
            this.pnAnalyses.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitLogList
            // 
            this.splitLogList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitLogList.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitLogList.Location = new System.Drawing.Point(0, 0);
            this.splitLogList.Name = "splitLogList";
            // 
            // splitLogList.Panel1
            // 
            this.splitLogList.Panel1.Controls.Add(this.splitLogDetails);
            this.splitLogList.Panel1.Controls.Add(this.btnAnalyzeLogs);
            // 
            // splitLogList.Panel2
            // 
            this.splitLogList.Panel2.Controls.Add(this.splitAnalysis);
            this.splitLogList.Size = new System.Drawing.Size(1000, 545);
            this.splitLogList.SplitterDistance = 227;
            this.splitLogList.TabIndex = 1;
            // 
            // splitLogDetails
            // 
            this.splitLogDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitLogDetails.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitLogDetails.Location = new System.Drawing.Point(0, 21);
            this.splitLogDetails.Name = "splitLogDetails";
            this.splitLogDetails.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitLogDetails.Panel1
            // 
            this.splitLogDetails.Panel1.Controls.Add(this.dgdLogList);
            // 
            // splitLogDetails.Panel2
            // 
            this.splitLogDetails.Panel2.Controls.Add(this.rtLogDetails);
            this.splitLogDetails.Panel2MinSize = 100;
            this.splitLogDetails.Size = new System.Drawing.Size(227, 524);
            this.splitLogDetails.SplitterDistance = 366;
            this.splitLogDetails.TabIndex = 3;
            // 
            // dgdLogList
            // 
            this.dgdLogList.AllowUserToAddRows = false;
            this.dgdLogList.AllowUserToDeleteRows = false;
            this.dgdLogList.AllowUserToResizeRows = false;
            this.dgdLogList.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgdLogList.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.dgdLogList.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this.dgdLogList.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dgdLogList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgdLogList.ColumnHeadersVisible = false;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgdLogList.DefaultCellStyle = dataGridViewCellStyle1;
            this.dgdLogList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgdLogList.Location = new System.Drawing.Point(0, 0);
            this.dgdLogList.Name = "dgdLogList";
            this.dgdLogList.ReadOnly = true;
            this.dgdLogList.RowHeadersVisible = false;
            this.dgdLogList.Size = new System.Drawing.Size(227, 366);
            this.dgdLogList.TabIndex = 3;
            this.dgdLogList.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.DgdLogList_DataBindingComplete);
            // 
            // rtLogDetails
            // 
            this.rtLogDetails.BackColor = System.Drawing.Color.Black;
            this.rtLogDetails.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtLogDetails.DetectUrls = false;
            this.rtLogDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtLogDetails.Font = new System.Drawing.Font("Consolas", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtLogDetails.ForeColor = System.Drawing.Color.LightSkyBlue;
            this.rtLogDetails.Location = new System.Drawing.Point(0, 0);
            this.rtLogDetails.Name = "rtLogDetails";
            this.rtLogDetails.ReadOnly = true;
            this.rtLogDetails.Size = new System.Drawing.Size(227, 154);
            this.rtLogDetails.TabIndex = 40;
            this.rtLogDetails.Text = "";
            // 
            // btnAnalyzeLogs
            // 
            this.btnAnalyzeLogs.BackColor = System.Drawing.SystemColors.Control;
            this.btnAnalyzeLogs.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnAnalyzeLogs.Enabled = false;
            this.btnAnalyzeLogs.FlatAppearance.BorderSize = 0;
            this.btnAnalyzeLogs.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.btnAnalyzeLogs.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnAnalyzeLogs.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAnalyzeLogs.Location = new System.Drawing.Point(0, 0);
            this.btnAnalyzeLogs.Name = "btnAnalyzeLogs";
            this.btnAnalyzeLogs.Size = new System.Drawing.Size(227, 21);
            this.btnAnalyzeLogs.TabIndex = 1;
            this.btnAnalyzeLogs.UseVisualStyleBackColor = false;
            this.btnAnalyzeLogs.Click += new System.EventHandler(this.btnAnalyzeLogs_Click);
            // 
            // splitAnalysis
            // 
            this.splitAnalysis.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitAnalysis.Location = new System.Drawing.Point(0, 0);
            this.splitAnalysis.Name = "splitAnalysis";
            this.splitAnalysis.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.splitAnalysis.SplitterMoved += SplitAnalysis_SplitterMoved;
            // 
            // splitAnalysis.Panel1
            // 
            this.splitAnalysis.Panel1.Controls.Add(this.splitPerfMonCountersAndChart);
            this.splitAnalysis.Panel1.Controls.Add(this.pnServers);
            // 
            // splitAnalysis.Panel2
            // 
            this.splitAnalysis.Panel2.Controls.Add(this.pnAnalyses);
            this.splitAnalysis.Size = new System.Drawing.Size(769, 545);
            this.splitAnalysis.SplitterDistance = 360;
            this.splitAnalysis.TabIndex = 0;
            // 
            // splitPerfMonCountersAndChart
            // 
            this.splitPerfMonCountersAndChart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitPerfMonCountersAndChart.Location = new System.Drawing.Point(0, 21);
            this.splitPerfMonCountersAndChart.Name = "splitPerfMonCountersAndChart";
            // 
            // splitPerfMonCountersAndChart.Panel1
            // 
            this.splitPerfMonCountersAndChart.Panel1.Controls.Add(this.dgdGrouping);
            this.splitPerfMonCountersAndChart.Panel1.Controls.Add(this.tvCounters);
            // 
            // splitPerfMonCountersAndChart.Panel2
            // 
            this.splitPerfMonCountersAndChart.Panel2.Controls.Add(this.chartPerfMon);
            this.splitPerfMonCountersAndChart.Size = new System.Drawing.Size(769, 339);
            this.splitPerfMonCountersAndChart.SplitterDistance = 340;
            this.splitPerfMonCountersAndChart.TabIndex = 47;
            // 
            // dgdGrouping
            // 
            this.dgdGrouping.AllowUserToOrderColumns = true;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgdGrouping.ColumnDisplayIndexChanged += DgdGrouping_ColumnDisplayIndexChanged;
            this.dgdGrouping.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dgdGrouping.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgdGrouping.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Counter,
            this.Instance});
            this.dgdGrouping.Dock = System.Windows.Forms.DockStyle.Top;
            this.dgdGrouping.Location = new System.Drawing.Point(0, 0);
            this.dgdGrouping.Name = "dgdGrouping";
            this.dgdGrouping.RowHeadersVisible = false;
            this.dgdGrouping.Size = new System.Drawing.Size(340, 21);
            this.dgdGrouping.TabIndex = 1;
            // 
            // Counter
            // 
            this.Counter.HeaderText = "Counter";
            this.Counter.Name = "Counter";
            this.Counter.ReadOnly = true;
            // 
            // Instance
            // 
            this.Instance.HeaderText = "Instance";
            this.Instance.Name = "Instance";
            this.Instance.ReadOnly = true;
            // 
            // tvCounters
            // 
            this.tvCounters.CheckBoxes = true;
            this.tvCounters.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tvCounters.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tvCounters.Location = new System.Drawing.Point(0, 101);
            this.tvCounters.Name = "tvCounters";
            this.tvCounters.Size = new System.Drawing.Size(340, 238);
            this.tvCounters.TabIndex = 0;
            // 
            // chartPerfMon
            // 
            chartArea1.Name = "ChartArea1";
            this.chartPerfMon.ChartAreas.Add(chartArea1);
            this.chartPerfMon.Dock = System.Windows.Forms.DockStyle.Fill;
            legend1.AutoFitMinFontSize = 6;
            legend1.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.25F);
            legend1.IsTextAutoFit = false;
            legend1.Name = "Legend";
            legend1.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 6.25F, System.Drawing.FontStyle.Bold);
            this.chartPerfMon.Legends.Add(legend1);
            this.chartPerfMon.Location = new System.Drawing.Point(0, 0);
            this.chartPerfMon.Name = "chartPerfMon";
            this.chartPerfMon.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.Bright;
            this.chartPerfMon.Size = new System.Drawing.Size(425, 339);
            this.chartPerfMon.TabIndex = 0;
            this.chartPerfMon.Text = "Counter Data";
            // 
            // pnServers
            // 
            this.pnServers.Controls.Add(this.lblCounterGrouping);
            this.pnServers.Controls.Add(this.cmbServers);
            this.pnServers.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnServers.Location = new System.Drawing.Point(0, 0);
            this.pnServers.Margin = new System.Windows.Forms.Padding(0);
            this.pnServers.Name = "pnServers";
            this.pnServers.Size = new System.Drawing.Size(769, 21);
            this.pnServers.TabIndex = 46;
            // 
            // lblCounterGrouping
            // 
            this.lblCounterGrouping.AutoSize = true;
            this.lblCounterGrouping.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCounterGrouping.Location = new System.Drawing.Point(4, 5);
            this.lblCounterGrouping.Name = "lblCounterGrouping";
            this.lblCounterGrouping.Size = new System.Drawing.Size(146, 12);
            this.lblCounterGrouping.TabIndex = 2;
            this.lblCounterGrouping.Text = "Drag headers to reorder groupings:";
            this.lblCounterGrouping.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cmbServers
            // 
            this.cmbServers.Dock = System.Windows.Forms.DockStyle.Right;
            this.cmbServers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbServers.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbServers.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbServers.FormattingEnabled = true;
            this.cmbServers.ItemHeight = 12;
            this.cmbServers.Location = new System.Drawing.Point(620, 0);
            this.cmbServers.Name = "cmbServers";
            this.cmbServers.Size = new System.Drawing.Size(149, 20);
            this.cmbServers.TabIndex = 1;
            this.cmbServers.Visible = false;
            this.cmbServers.SelectedIndexChanged += new System.EventHandler(this.cmbServers_SelectedIndexChanged);
            // 
            // pnAnalyses
            // 
            this.pnAnalyses.Controls.Add(this.lblAnalysesStatus);
            this.pnAnalyses.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnAnalyses.Location = new System.Drawing.Point(0, 0);
            this.pnAnalyses.Name = "pnAnalyses";
            this.pnAnalyses.Size = new System.Drawing.Size(769, 18);
            this.pnAnalyses.TabIndex = 2;
            // 
            // lblAnalysesStatus
            // 
            this.lblAnalysesStatus.AutoSize = true;
            this.lblAnalysesStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAnalysesStatus.Location = new System.Drawing.Point(4, 3);
            this.lblAnalysesStatus.Name = "lblAnalysesStatus";
            this.lblAnalysesStatus.Size = new System.Drawing.Size(0, 12);
            this.lblAnalysesStatus.TabIndex = 4;
            this.lblAnalysesStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ucASPerfMonAnalyzer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitLogList);
            this.Name = "ucASPerfMonAnalyzer";
            this.Size = new System.Drawing.Size(1000, 545);
            this.SizeChanged += new System.EventHandler(this.ucASPerfMonAnalyzer_SizeChanged);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.ucASPerfMonAnalyzer_Paint);
            this.splitLogList.Panel1.ResumeLayout(false);
            this.splitLogList.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitLogList)).EndInit();
            this.splitLogList.ResumeLayout(false);
            this.splitLogDetails.Panel1.ResumeLayout(false);
            this.splitLogDetails.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitLogDetails)).EndInit();
            this.splitLogDetails.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgdLogList)).EndInit();
            this.splitAnalysis.Panel1.ResumeLayout(false);
            this.splitAnalysis.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitAnalysis)).EndInit();
            this.splitAnalysis.ResumeLayout(false);
            this.splitPerfMonCountersAndChart.Panel1.ResumeLayout(false);
            this.splitPerfMonCountersAndChart.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitPerfMonCountersAndChart)).EndInit();
            this.splitPerfMonCountersAndChart.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgdGrouping)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartPerfMon)).EndInit();
            this.pnServers.ResumeLayout(false);
            this.pnServers.PerformLayout();
            this.pnAnalyses.ResumeLayout(false);
            this.pnAnalyses.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitLogList;
        private System.Windows.Forms.SplitContainer splitLogDetails;
        private System.Windows.Forms.DataGridView dgdLogList;
        private System.Windows.Forms.RichTextBox rtLogDetails;
        public System.Windows.Forms.Button btnAnalyzeLogs;
        private System.Windows.Forms.SplitContainer splitAnalysis;
        private System.Windows.Forms.Panel pnServers;
        private System.Windows.Forms.Label lblCounterGrouping;
        private System.Windows.Forms.ComboBox cmbServers;
        private System.Windows.Forms.Panel pnAnalyses;
        private System.Windows.Forms.Label lblAnalysesStatus;
        private System.Windows.Forms.SplitContainer splitPerfMonCountersAndChart;
        private TriStateTreeView tvCounters;
        //private System.Windows.Forms.TreeView tvCounters;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartPerfMon;
        private System.Windows.Forms.DataGridView dgdGrouping;
        private System.Windows.Forms.DataGridViewTextBoxColumn Counter;
        private System.Windows.Forms.DataGridViewTextBoxColumn Instance;
    }
}
