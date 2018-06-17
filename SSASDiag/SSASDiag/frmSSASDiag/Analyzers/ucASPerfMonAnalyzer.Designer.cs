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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.splitLogList = new System.Windows.Forms.SplitContainer();
            this.splitLogDetails = new System.Windows.Forms.SplitContainer();
            this.dgdLogList = new System.Windows.Forms.DataGridView();
            this.rtLogDetails = new System.Windows.Forms.RichTextBox();
            this.btnAnalyzeLogs = new System.Windows.Forms.Button();
            this.splitAnalysis = new System.Windows.Forms.SplitContainer();
            this.pnServers = new System.Windows.Forms.Panel();
            this.lblOutputStatus = new System.Windows.Forms.Label();
            this.cmbServers = new System.Windows.Forms.ComboBox();
            this.pnAnalyses = new System.Windows.Forms.Panel();
            this.lblAnalysesStatus = new System.Windows.Forms.Label();
            this.splitPerfMonCountersAndChart = new System.Windows.Forms.SplitContainer();
            this.tvCounters = new System.Windows.Forms.TreeView();
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
            this.pnServers.SuspendLayout();
            this.pnAnalyses.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitPerfMonCountersAndChart)).BeginInit();
            this.splitPerfMonCountersAndChart.Panel1.SuspendLayout();
            this.splitPerfMonCountersAndChart.SuspendLayout();
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
            this.splitLogList.Size = new System.Drawing.Size(638, 432);
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
            this.splitLogDetails.Size = new System.Drawing.Size(227, 411);
            this.splitLogDetails.SplitterDistance = 253;
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
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgdLogList.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgdLogList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgdLogList.Location = new System.Drawing.Point(0, 0);
            this.dgdLogList.Name = "dgdLogList";
            this.dgdLogList.ReadOnly = true;
            this.dgdLogList.RowHeadersVisible = false;
            this.dgdLogList.Size = new System.Drawing.Size(227, 253);
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
            // 
            // splitAnalysis.Panel1
            // 
            this.splitAnalysis.Panel1.Controls.Add(this.splitPerfMonCountersAndChart);
            this.splitAnalysis.Panel1.Controls.Add(this.pnServers);
            // 
            // splitAnalysis.Panel2
            // 
            this.splitAnalysis.Panel2.Controls.Add(this.pnAnalyses);
            this.splitAnalysis.Size = new System.Drawing.Size(407, 432);
            this.splitAnalysis.SplitterDistance = 286;
            this.splitAnalysis.TabIndex = 0;
            // 
            // pnServers
            // 
            this.pnServers.Controls.Add(this.lblOutputStatus);
            this.pnServers.Controls.Add(this.cmbServers);
            this.pnServers.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnServers.Location = new System.Drawing.Point(0, 0);
            this.pnServers.Margin = new System.Windows.Forms.Padding(0);
            this.pnServers.Name = "pnServers";
            this.pnServers.Size = new System.Drawing.Size(407, 21);
            this.pnServers.TabIndex = 46;
            // 
            // lblOutputStatus
            // 
            this.lblOutputStatus.AutoSize = true;
            this.lblOutputStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOutputStatus.Location = new System.Drawing.Point(4, 5);
            this.lblOutputStatus.Name = "lblOutputStatus";
            this.lblOutputStatus.Size = new System.Drawing.Size(0, 12);
            this.lblOutputStatus.TabIndex = 2;
            this.lblOutputStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cmbServers
            // 
            this.cmbServers.Dock = System.Windows.Forms.DockStyle.Right;
            this.cmbServers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbServers.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbServers.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbServers.FormattingEnabled = true;
            this.cmbServers.ItemHeight = 12;
            this.cmbServers.Location = new System.Drawing.Point(258, 0);
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
            this.pnAnalyses.Size = new System.Drawing.Size(407, 18);
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
            // splitPerfMonCountersAndChart
            // 
            this.splitPerfMonCountersAndChart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitPerfMonCountersAndChart.Location = new System.Drawing.Point(0, 21);
            this.splitPerfMonCountersAndChart.Name = "splitPerfMonCountersAndChart";
            // 
            // splitPerfMonCountersAndChart.Panel1
            // 
            this.splitPerfMonCountersAndChart.Panel1.Controls.Add(this.tvCounters);
            this.splitPerfMonCountersAndChart.Size = new System.Drawing.Size(407, 265);
            this.splitPerfMonCountersAndChart.SplitterDistance = 180;
            this.splitPerfMonCountersAndChart.TabIndex = 47;
            // 
            // tvCounters
            // 
            this.tvCounters.CheckBoxes = true;
            this.tvCounters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvCounters.Location = new System.Drawing.Point(0, 0);
            this.tvCounters.Name = "tvCounters";
            this.tvCounters.Size = new System.Drawing.Size(180, 265);
            this.tvCounters.TabIndex = 0;
            // 
            // ucASPerfMonAnalyzer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitLogList);
            this.Name = "ucASPerfMonAnalyzer";
            this.Size = new System.Drawing.Size(638, 432);
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
            this.pnServers.ResumeLayout(false);
            this.pnServers.PerformLayout();
            this.pnAnalyses.ResumeLayout(false);
            this.pnAnalyses.PerformLayout();
            this.splitPerfMonCountersAndChart.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitPerfMonCountersAndChart)).EndInit();
            this.splitPerfMonCountersAndChart.ResumeLayout(false);
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
        private System.Windows.Forms.Label lblOutputStatus;
        private System.Windows.Forms.ComboBox cmbServers;
        private System.Windows.Forms.Panel pnAnalyses;
        private System.Windows.Forms.Label lblAnalysesStatus;
        private System.Windows.Forms.SplitContainer splitPerfMonCountersAndChart;
        private System.Windows.Forms.TreeView tvCounters;
    }
}
