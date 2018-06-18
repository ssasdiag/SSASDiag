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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
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
            this.tvCounters = new System.Windows.Forms.TreeView();
            this.pnlSeriesDetails = new System.Windows.Forms.TableLayoutPanel();
            this.label6 = new System.Windows.Forms.Label();
            this.txtDur = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtAvg = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtMin = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtMax = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
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
            this.pnlSeriesDetails.SuspendLayout();
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
            this.splitLogList.SplitterDistance = 189;
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
            this.splitLogDetails.Size = new System.Drawing.Size(189, 524);
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
            this.dgdLogList.Size = new System.Drawing.Size(189, 366);
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
            this.rtLogDetails.Size = new System.Drawing.Size(189, 154);
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
            this.btnAnalyzeLogs.Size = new System.Drawing.Size(189, 21);
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
            this.splitAnalysis.Size = new System.Drawing.Size(807, 545);
            this.splitAnalysis.SplitterDistance = 389;
            this.splitAnalysis.TabIndex = 0;
            this.splitAnalysis.Visible = false;
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
            this.splitPerfMonCountersAndChart.Panel2.Controls.Add(this.pnlSeriesDetails);
            this.splitPerfMonCountersAndChart.Panel2.Controls.Add(this.chartPerfMon);
            this.splitPerfMonCountersAndChart.Size = new System.Drawing.Size(807, 368);
            this.splitPerfMonCountersAndChart.SplitterDistance = 200;
            this.splitPerfMonCountersAndChart.TabIndex = 47;
            // 
            // dgdGrouping
            // 
            this.dgdGrouping.AllowUserToOrderColumns = true;
            this.dgdGrouping.AllowUserToResizeColumns = false;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgdGrouping.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dgdGrouping.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgdGrouping.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Counter,
            this.Instance});
            this.dgdGrouping.Dock = System.Windows.Forms.DockStyle.Top;
            this.dgdGrouping.Location = new System.Drawing.Point(0, 0);
            this.dgdGrouping.Name = "dgdGrouping";
            this.dgdGrouping.RowHeadersVisible = false;
            this.dgdGrouping.Size = new System.Drawing.Size(200, 21);
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
            this.tvCounters.Location = new System.Drawing.Point(0, 130);
            this.tvCounters.Name = "tvCounters";
            this.tvCounters.Size = new System.Drawing.Size(200, 238);
            this.tvCounters.TabIndex = 0;
            // 
            // pnlSeriesDetails
            // 
            this.pnlSeriesDetails.ColumnCount = 10;
            this.pnlSeriesDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.pnlSeriesDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            this.pnlSeriesDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.pnlSeriesDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            this.pnlSeriesDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.pnlSeriesDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            this.pnlSeriesDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.pnlSeriesDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            this.pnlSeriesDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.pnlSeriesDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.pnlSeriesDetails.Controls.Add(this.label6);
            this.pnlSeriesDetails.Controls.Add(this.txtDur);
            this.pnlSeriesDetails.Controls.Add(this.label3);
            this.pnlSeriesDetails.Controls.Add(this.txtAvg);
            this.pnlSeriesDetails.Controls.Add(this.label2);
            this.pnlSeriesDetails.Controls.Add(this.txtMin);
            this.pnlSeriesDetails.Controls.Add(this.label1);
            this.pnlSeriesDetails.Controls.Add(this.txtMax);
            this.pnlSeriesDetails.Controls.Add(this.label4);
            this.pnlSeriesDetails.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlSeriesDetails.Location = new System.Drawing.Point(0, 248);
            this.pnlSeriesDetails.Name = "pnlSeriesDetails";
            this.pnlSeriesDetails.RowCount = 2;
            this.pnlSeriesDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.pnlSeriesDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.pnlSeriesDetails.Size = new System.Drawing.Size(603, 80);
            this.pnlSeriesDetails.TabIndex = 2;
            // 
            // label6
            // 
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(0, 0);
            this.label6.Margin = new System.Windows.Forms.Padding(0);
            this.label6.Name = "label6";
            this.label6.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.label6.Size = new System.Drawing.Size(50, 20);
            this.label6.TabIndex = 6;
            this.label6.Text = "Duration";
            // 
            // txtDur
            // 
            this.txtDur.BackColor = System.Drawing.SystemColors.ControlLight;
            this.txtDur.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtDur.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDur.Location = new System.Drawing.Point(50, 0);
            this.txtDur.Margin = new System.Windows.Forms.Padding(0);
            this.txtDur.Name = "txtDur";
            this.txtDur.ReadOnly = true;
            this.txtDur.Size = new System.Drawing.Size(80, 18);
            this.txtDur.TabIndex = 4;
            this.txtDur.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(140, 2);
            this.label3.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(26, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Avg";
            // 
            // txtAvg
            // 
            this.txtAvg.BackColor = System.Drawing.SystemColors.ControlLight;
            this.txtAvg.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtAvg.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtAvg.Location = new System.Drawing.Point(170, 0);
            this.txtAvg.Margin = new System.Windows.Forms.Padding(0);
            this.txtAvg.Name = "txtAvg";
            this.txtAvg.ReadOnly = true;
            this.txtAvg.Size = new System.Drawing.Size(100, 18);
            this.txtAvg.TabIndex = 4;
            this.txtAvg.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(280, 2);
            this.label2.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(24, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Min";
            // 
            // txtMin
            // 
            this.txtMin.BackColor = System.Drawing.SystemColors.ControlLight;
            this.txtMin.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtMin.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMin.Location = new System.Drawing.Point(310, 0);
            this.txtMin.Margin = new System.Windows.Forms.Padding(0);
            this.txtMin.Name = "txtMin";
            this.txtMin.ReadOnly = true;
            this.txtMin.Size = new System.Drawing.Size(100, 18);
            this.txtMin.TabIndex = 2;
            this.txtMin.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(420, 2);
            this.label1.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(27, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Max";
            // 
            // txtMax
            // 
            this.txtMax.BackColor = System.Drawing.SystemColors.ControlLight;
            this.txtMax.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtMax.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMax.Location = new System.Drawing.Point(450, 0);
            this.txtMax.Margin = new System.Windows.Forms.Padding(0);
            this.txtMax.Name = "txtMax";
            this.txtMax.ReadOnly = true;
            this.txtMax.Size = new System.Drawing.Size(100, 18);
            this.txtMax.TabIndex = 0;
            this.txtMax.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(563, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(1, 20);
            this.label4.TabIndex = 0;
            // 
            // chartPerfMon
            // 
            chartArea2.AxisX.LabelStyle.Angle = 90;
            chartArea2.AxisX.LabelStyle.Format = "yyyy-MM-dd hh:mm:ss tt";
            chartArea2.AxisX.Title = "Date/Time UTC";
            chartArea2.AxisX.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 6.5F);
            chartArea2.Name = "ChartArea1";
            this.chartPerfMon.ChartAreas.Add(chartArea2);
            this.chartPerfMon.Dock = System.Windows.Forms.DockStyle.Top;
            legend2.AutoFitMinFontSize = 6;
            legend2.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.25F);
            legend2.IsTextAutoFit = false;
            legend2.Name = "Legend";
            legend2.TitleFont = new System.Drawing.Font("Microsoft Sans Serif", 6.25F, System.Drawing.FontStyle.Bold);
            this.chartPerfMon.Legends.Add(legend2);
            this.chartPerfMon.Location = new System.Drawing.Point(0, 0);
            this.chartPerfMon.Margin = new System.Windows.Forms.Padding(0);
            this.chartPerfMon.Name = "chartPerfMon";
            this.chartPerfMon.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.Bright;
            this.chartPerfMon.Size = new System.Drawing.Size(603, 323);
            this.chartPerfMon.SuppressExceptions = true;
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
            this.pnServers.Size = new System.Drawing.Size(807, 21);
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
            this.cmbServers.Location = new System.Drawing.Point(658, 0);
            this.cmbServers.Name = "cmbServers";
            this.cmbServers.Size = new System.Drawing.Size(149, 20);
            this.cmbServers.TabIndex = 1;
            this.cmbServers.SelectedIndexChanged += new System.EventHandler(this.cmbServers_SelectedIndexChanged);
            // 
            // pnAnalyses
            // 
            this.pnAnalyses.Controls.Add(this.lblAnalysesStatus);
            this.pnAnalyses.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnAnalyses.Location = new System.Drawing.Point(0, 0);
            this.pnAnalyses.Name = "pnAnalyses";
            this.pnAnalyses.Size = new System.Drawing.Size(807, 18);
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
            this.pnlSeriesDetails.ResumeLayout(false);
            this.pnlSeriesDetails.PerformLayout();
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
        //private TriStateTreeView tvCounters;
        private System.Windows.Forms.TreeView tvCounters;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartPerfMon;
        private System.Windows.Forms.DataGridView dgdGrouping;
        private System.Windows.Forms.DataGridViewTextBoxColumn Counter;
        private System.Windows.Forms.DataGridViewTextBoxColumn Instance;
        private System.Windows.Forms.TableLayoutPanel pnlSeriesDetails;
        private System.Windows.Forms.TextBox txtMax;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtMin;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtAvg;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;

        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtDur;
    }
}
