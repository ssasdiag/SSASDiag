namespace SSASDiag
{
    partial class frmAddPerfMonRule
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmAddPerfMonRule));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            this.splitCountersAndRule = new System.Windows.Forms.SplitContainer();
            this.splitCounters = new System.Windows.Forms.SplitContainer();
            this.dgdSelectedCounters = new System.Windows.Forms.DataGridView();
            this.Counter = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ShowInChart = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.HighlightInChart = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.WildcardWithTotal = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.splitExpressionsAndRule = new System.Windows.Forms.SplitContainer();
            this.splitExpressions = new System.Windows.Forms.SplitContainer();
            this.label3 = new System.Windows.Forms.Label();
            this.dgdExpressions = new System.Windows.Forms.DataGridView();
            this.ExpressionName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Expression = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.tt = new System.Windows.Forms.ToolTip(this.components);
            this.panel2 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbValueToCheck = new System.Windows.Forms.ComboBox();
            this.lblSeriesFunction = new System.Windows.Forms.Label();
            this.cmbSeriesFunction = new System.Windows.Forms.ComboBox();
            this.lblPctMatchCheck = new System.Windows.Forms.Label();
            this.udPctMatchCheck = new System.Windows.Forms.NumericUpDown();
            this.pnlHigh = new System.Windows.Forms.Panel();
            this.pnlMed = new System.Windows.Forms.Panel();
            this.pnlLow = new System.Windows.Forms.Panel();
            this.cmbCheckAboveOrBelow = new System.Windows.Forms.ComboBox();
            this.cmbValHigh = new System.Windows.Forms.ComboBox();
            this.cmbValMed = new System.Windows.Forms.ComboBox();
            this.cmbValLow = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitCountersAndRule)).BeginInit();
            this.splitCountersAndRule.Panel1.SuspendLayout();
            this.splitCountersAndRule.Panel2.SuspendLayout();
            this.splitCountersAndRule.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitCounters)).BeginInit();
            this.splitCounters.Panel2.SuspendLayout();
            this.splitCounters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgdSelectedCounters)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitExpressionsAndRule)).BeginInit();
            this.splitExpressionsAndRule.Panel1.SuspendLayout();
            this.splitExpressionsAndRule.Panel2.SuspendLayout();
            this.splitExpressionsAndRule.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitExpressions)).BeginInit();
            this.splitExpressions.Panel1.SuspendLayout();
            this.splitExpressions.Panel2.SuspendLayout();
            this.splitExpressions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgdExpressions)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udPctMatchCheck)).BeginInit();
            this.pnlHigh.SuspendLayout();
            this.pnlMed.SuspendLayout();
            this.pnlLow.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitCountersAndRule
            // 
            this.splitCountersAndRule.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitCountersAndRule.Location = new System.Drawing.Point(0, 31);
            this.splitCountersAndRule.Name = "splitCountersAndRule";
            this.splitCountersAndRule.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitCountersAndRule.Panel1
            // 
            this.splitCountersAndRule.Panel1.Controls.Add(this.splitCounters);
            this.splitCountersAndRule.Panel1MinSize = 220;
            // 
            // splitCountersAndRule.Panel2
            // 
            this.splitCountersAndRule.Panel2.Controls.Add(this.splitExpressionsAndRule);
            this.splitCountersAndRule.Size = new System.Drawing.Size(636, 651);
            this.splitCountersAndRule.SplitterDistance = 220;
            this.splitCountersAndRule.TabIndex = 0;
            // 
            // splitCounters
            // 
            this.splitCounters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitCounters.Location = new System.Drawing.Point(0, 0);
            this.splitCounters.Name = "splitCounters";
            this.splitCounters.Panel1MinSize = 260;
            // 
            // splitCounters.Panel2
            // 
            this.splitCounters.Panel2.Controls.Add(this.dgdSelectedCounters);
            this.splitCounters.Size = new System.Drawing.Size(636, 220);
            this.splitCounters.SplitterDistance = 260;
            this.splitCounters.TabIndex = 0;
            this.splitCounters.SplitterMoving += new System.Windows.Forms.SplitterCancelEventHandler(this.splitCounters_SplitterMoving);
            // 
            // dgdSelectedCounters
            // 
            this.dgdSelectedCounters.AllowDrop = true;
            this.dgdSelectedCounters.AllowUserToAddRows = false;
            this.dgdSelectedCounters.AllowUserToResizeRows = false;
            this.dgdSelectedCounters.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgdSelectedCounters.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgdSelectedCounters.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F);
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgdSelectedCounters.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle5;
            this.dgdSelectedCounters.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgdSelectedCounters.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Counter,
            this.ShowInChart,
            this.HighlightInChart,
            this.WildcardWithTotal});
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F);
            dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgdSelectedCounters.DefaultCellStyle = dataGridViewCellStyle6;
            this.dgdSelectedCounters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgdSelectedCounters.Location = new System.Drawing.Point(0, 0);
            this.dgdSelectedCounters.Margin = new System.Windows.Forms.Padding(0);
            this.dgdSelectedCounters.Name = "dgdSelectedCounters";
            this.dgdSelectedCounters.RowHeadersVisible = false;
            this.dgdSelectedCounters.RowTemplate.Height = 30;
            this.dgdSelectedCounters.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgdSelectedCounters.Size = new System.Drawing.Size(372, 220);
            this.dgdSelectedCounters.TabIndex = 0;
            this.dgdSelectedCounters.DragDrop += new System.Windows.Forms.DragEventHandler(this.dgdSelectedCounters_DragDrop);
            this.dgdSelectedCounters.DragEnter += new System.Windows.Forms.DragEventHandler(this.dgdSelectedCounters_DragEnter);
            this.dgdSelectedCounters.MouseDown += new System.Windows.Forms.MouseEventHandler(this.dgdSelectedCounters_MouseDown);
            this.dgdSelectedCounters.MouseMove += new System.Windows.Forms.MouseEventHandler(this.dgdSelectedCounters_MouseMove);
            // 
            // Counter
            // 
            this.Counter.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Counter.HeaderText = "Counter Name";
            this.Counter.Name = "Counter";
            this.Counter.ReadOnly = true;
            // 
            // ShowInChart
            // 
            this.ShowInChart.HeaderText = "Display";
            this.ShowInChart.Name = "ShowInChart";
            this.ShowInChart.Width = 50;
            // 
            // HighlightInChart
            // 
            this.HighlightInChart.HeaderText = "Highlight";
            this.HighlightInChart.Name = "HighlightInChart";
            this.HighlightInChart.Width = 50;
            // 
            // WildcardWithTotal
            // 
            this.WildcardWithTotal.HeaderText = "* Includes _Total";
            this.WildcardWithTotal.Name = "WildcardWithTotal";
            this.WildcardWithTotal.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.WildcardWithTotal.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.WildcardWithTotal.ToolTipText = "If the counter features a wildcard, include _Total in the child nodes.";
            // 
            // splitExpressionsAndRule
            // 
            this.splitExpressionsAndRule.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitExpressionsAndRule.Location = new System.Drawing.Point(0, 0);
            this.splitExpressionsAndRule.Name = "splitExpressionsAndRule";
            this.splitExpressionsAndRule.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitExpressionsAndRule.Panel1
            // 
            this.splitExpressionsAndRule.Panel1.Controls.Add(this.splitExpressions);
            this.splitExpressionsAndRule.Panel1MinSize = 215;
            // 
            // splitExpressionsAndRule.Panel2
            // 
            this.splitExpressionsAndRule.Panel2.Controls.Add(this.panel2);
            this.splitExpressionsAndRule.Size = new System.Drawing.Size(636, 427);
            this.splitExpressionsAndRule.SplitterDistance = 215;
            this.splitExpressionsAndRule.TabIndex = 0;
            // 
            // splitExpressions
            // 
            this.splitExpressions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitExpressions.Location = new System.Drawing.Point(0, 0);
            this.splitExpressions.Name = "splitExpressions";
            // 
            // splitExpressions.Panel1
            // 
            this.splitExpressions.Panel1.Controls.Add(this.label3);
            this.splitExpressions.Panel1MinSize = 260;
            // 
            // splitExpressions.Panel2
            // 
            this.splitExpressions.Panel2.Controls.Add(this.dgdExpressions);
            this.splitExpressions.Size = new System.Drawing.Size(636, 215);
            this.splitExpressions.SplitterDistance = 260;
            this.splitExpressions.TabIndex = 3;
            this.splitExpressions.SplitterMoving += new System.Windows.Forms.SplitterCancelEventHandler(this.splitExpressions_SplitterMoving);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F);
            this.label3.Location = new System.Drawing.Point(3, 1);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(241, 204);
            this.label3.TabIndex = 2;
            this.label3.Text = resources.GetString("label3.Text");
            // 
            // dgdExpressions
            // 
            this.dgdExpressions.AllowDrop = true;
            this.dgdExpressions.AllowUserToResizeRows = false;
            this.dgdExpressions.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dgdExpressions.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgdExpressions.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgdExpressions.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle7.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle7.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F);
            dataGridViewCellStyle7.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgdExpressions.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle7;
            this.dgdExpressions.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgdExpressions.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ExpressionName,
            this.Expression});
            this.dgdExpressions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgdExpressions.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnKeystroke;
            this.dgdExpressions.GridColor = System.Drawing.SystemColors.Window;
            this.dgdExpressions.Location = new System.Drawing.Point(0, 0);
            this.dgdExpressions.Margin = new System.Windows.Forms.Padding(0);
            this.dgdExpressions.MultiSelect = false;
            this.dgdExpressions.Name = "dgdExpressions";
            this.dgdExpressions.RowHeadersWidth = 24;
            dataGridViewCellStyle8.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F);
            dataGridViewCellStyle8.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle8.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle8.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgdExpressions.RowsDefaultCellStyle = dataGridViewCellStyle8;
            this.dgdExpressions.RowTemplate.DefaultCellStyle.ForeColor = System.Drawing.SystemColors.ControlText;
            this.dgdExpressions.RowTemplate.DefaultCellStyle.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            this.dgdExpressions.RowTemplate.DefaultCellStyle.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            this.dgdExpressions.RowTemplate.Height = 30;
            this.dgdExpressions.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgdExpressions.Size = new System.Drawing.Size(372, 215);
            this.dgdExpressions.TabIndex = 3;
            this.dgdExpressions.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgdExpressions_CellEndEdit);
            this.dgdExpressions.RowsRemoved += new System.Windows.Forms.DataGridViewRowsRemovedEventHandler(this.dgdExpressions_RowsRemoved);
            this.dgdExpressions.DragDrop += new System.Windows.Forms.DragEventHandler(this.dgdExpressions_DragDrop);
            this.dgdExpressions.DragEnter += new System.Windows.Forms.DragEventHandler(this.dgdExpressions_DragEnter);
            this.dgdExpressions.MouseClick += new System.Windows.Forms.MouseEventHandler(this.dgdExpressions_MouseClick);
            // 
            // ExpressionName
            // 
            this.ExpressionName.HeaderText = "Name";
            this.ExpressionName.Name = "ExpressionName";
            // 
            // Expression
            // 
            this.Expression.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Expression.HeaderText = "Expression";
            this.Expression.Name = "Expression";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(636, 31);
            this.panel1.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F);
            this.label1.Location = new System.Drawing.Point(3, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(438, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "Include counters in the rule by dragging available counters on the left onto the " +
    "rule\'s counter list on the right.";
            // 
            // panel2
            // 
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.cmbCheckAboveOrBelow);
            this.panel2.Controls.Add(this.pnlMed);
            this.panel2.Controls.Add(this.pnlLow);
            this.panel2.Controls.Add(this.pnlHigh);
            this.panel2.Controls.Add(this.udPctMatchCheck);
            this.panel2.Controls.Add(this.lblPctMatchCheck);
            this.panel2.Controls.Add(this.lblSeriesFunction);
            this.panel2.Controls.Add(this.cmbSeriesFunction);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.cmbValueToCheck);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(636, 208);
            this.panel2.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F);
            this.label2.Location = new System.Drawing.Point(50, 4);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(95, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "Value/Series to check";
            // 
            // cmbValueToCheck
            // 
            this.cmbValueToCheck.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbValueToCheck.Enabled = false;
            this.cmbValueToCheck.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F);
            this.cmbValueToCheck.FormattingEnabled = true;
            this.cmbValueToCheck.Location = new System.Drawing.Point(7, 24);
            this.cmbValueToCheck.Name = "cmbValueToCheck";
            this.cmbValueToCheck.Size = new System.Drawing.Size(181, 20);
            this.cmbValueToCheck.TabIndex = 2;
            this.cmbValueToCheck.SelectedIndexChanged += new System.EventHandler(this.cmbValueToCheck_SelectedIndexChanged);
            // 
            // lblSeriesFunction
            // 
            this.lblSeriesFunction.AutoSize = true;
            this.lblSeriesFunction.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F);
            this.lblSeriesFunction.Location = new System.Drawing.Point(27, 52);
            this.lblSeriesFunction.Name = "lblSeriesFunction";
            this.lblSeriesFunction.Size = new System.Drawing.Size(140, 12);
            this.lblSeriesFunction.TabIndex = 5;
            this.lblSeriesFunction.Text = "Function to Compare from Series";
            this.lblSeriesFunction.Visible = false;
            // 
            // cmbSeriesFunction
            // 
            this.cmbSeriesFunction.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSeriesFunction.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F);
            this.cmbSeriesFunction.FormattingEnabled = true;
            this.cmbSeriesFunction.Items.AddRange(new object[] {
            "First",
            "Last",
            "Max",
            "Min",
            "Avg (ignore nulls)",
            "Avg (ignore nulls and zeros)",
            "Avg (include all values)",
            "X% of values to warn/error"});
            this.cmbSeriesFunction.Location = new System.Drawing.Point(7, 72);
            this.cmbSeriesFunction.Name = "cmbSeriesFunction";
            this.cmbSeriesFunction.Size = new System.Drawing.Size(181, 20);
            this.cmbSeriesFunction.TabIndex = 4;
            this.cmbSeriesFunction.Visible = false;
            this.cmbSeriesFunction.SelectedIndexChanged += new System.EventHandler(this.cmbSeriesFunction_SelectedIndexChanged);
            // 
            // lblPctMatchCheck
            // 
            this.lblPctMatchCheck.AutoSize = true;
            this.lblPctMatchCheck.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F);
            this.lblPctMatchCheck.Location = new System.Drawing.Point(35, 100);
            this.lblPctMatchCheck.Name = "lblPctMatchCheck";
            this.lblPctMatchCheck.Size = new System.Drawing.Size(124, 12);
            this.lblPctMatchCheck.TabIndex = 6;
            this.lblPctMatchCheck.Text = "% to match for error/warning:";
            this.lblPctMatchCheck.Visible = false;
            // 
            // udPctMatchCheck
            // 
            this.udPctMatchCheck.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F);
            this.udPctMatchCheck.Location = new System.Drawing.Point(72, 120);
            this.udPctMatchCheck.Name = "udPctMatchCheck";
            this.udPctMatchCheck.Size = new System.Drawing.Size(50, 17);
            this.udPctMatchCheck.TabIndex = 7;
            this.udPctMatchCheck.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.udPctMatchCheck.Value = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.udPctMatchCheck.Visible = false;
            // 
            // pnlHigh
            // 
            this.pnlHigh.Controls.Add(this.cmbValHigh);
            this.pnlHigh.Location = new System.Drawing.Point(227, 32);
            this.pnlHigh.Name = "pnlHigh";
            this.pnlHigh.Size = new System.Drawing.Size(408, 60);
            this.pnlHigh.TabIndex = 8;
            // 
            // pnlMed
            // 
            this.pnlMed.Controls.Add(this.cmbValMed);
            this.pnlMed.Location = new System.Drawing.Point(227, 89);
            this.pnlMed.Name = "pnlMed";
            this.pnlMed.Size = new System.Drawing.Size(408, 60);
            this.pnlMed.TabIndex = 9;
            // 
            // pnlLow
            // 
            this.pnlLow.Controls.Add(this.cmbValLow);
            this.pnlLow.Location = new System.Drawing.Point(227, 147);
            this.pnlLow.Name = "pnlLow";
            this.pnlLow.Size = new System.Drawing.Size(408, 60);
            this.pnlLow.TabIndex = 9;
            // 
            // cmbCheckAboveOrBelow
            // 
            this.cmbCheckAboveOrBelow.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCheckAboveOrBelow.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F);
            this.cmbCheckAboveOrBelow.FormattingEnabled = true;
            this.cmbCheckAboveOrBelow.Items.AddRange(new object[] {
            "Check for values above thresholds",
            "Check for values below thresholds"});
            this.cmbCheckAboveOrBelow.Location = new System.Drawing.Point(227, 2);
            this.cmbCheckAboveOrBelow.Name = "cmbCheckAboveOrBelow";
            this.cmbCheckAboveOrBelow.Size = new System.Drawing.Size(169, 20);
            this.cmbCheckAboveOrBelow.TabIndex = 0;
            this.cmbCheckAboveOrBelow.SelectedIndexChanged += new System.EventHandler(this.cmbCheckAboveOrBelow_SelectedIndexChanged);
            // 
            // cmbValHigh
            // 
            this.cmbValHigh.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbValHigh.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F);
            this.cmbValHigh.FormattingEnabled = true;
            this.cmbValHigh.Location = new System.Drawing.Point(14, 20);
            this.cmbValHigh.Name = "cmbValHigh";
            this.cmbValHigh.Size = new System.Drawing.Size(121, 20);
            this.cmbValHigh.TabIndex = 0;
            // 
            // cmbValMed
            // 
            this.cmbValMed.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbValMed.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F);
            this.cmbValMed.FormattingEnabled = true;
            this.cmbValMed.Location = new System.Drawing.Point(14, 21);
            this.cmbValMed.Name = "cmbValMed";
            this.cmbValMed.Size = new System.Drawing.Size(121, 20);
            this.cmbValMed.TabIndex = 1;
            // 
            // cmbValLow
            // 
            this.cmbValLow.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbValLow.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F);
            this.cmbValLow.FormattingEnabled = true;
            this.cmbValLow.Location = new System.Drawing.Point(14, 21);
            this.cmbValLow.Name = "cmbValLow";
            this.cmbValLow.Size = new System.Drawing.Size(121, 20);
            this.cmbValLow.TabIndex = 1;
            // 
            // frmAddPerfMonRule
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(636, 682);
            this.Controls.Add(this.splitCountersAndRule);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmAddPerfMonRule";
            this.ShowInTaskbar = false;
            this.Text = "Create Rule";
            this.Load += new System.EventHandler(this.frmAddPerfMonRule_Load);
            this.splitCountersAndRule.Panel1.ResumeLayout(false);
            this.splitCountersAndRule.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitCountersAndRule)).EndInit();
            this.splitCountersAndRule.ResumeLayout(false);
            this.splitCounters.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitCounters)).EndInit();
            this.splitCounters.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgdSelectedCounters)).EndInit();
            this.splitExpressionsAndRule.Panel1.ResumeLayout(false);
            this.splitExpressionsAndRule.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitExpressionsAndRule)).EndInit();
            this.splitExpressionsAndRule.ResumeLayout(false);
            this.splitExpressions.Panel1.ResumeLayout(false);
            this.splitExpressions.Panel1.PerformLayout();
            this.splitExpressions.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitExpressions)).EndInit();
            this.splitExpressions.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgdExpressions)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udPctMatchCheck)).EndInit();
            this.pnlHigh.ResumeLayout(false);
            this.pnlMed.ResumeLayout(false);
            this.pnlLow.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitCountersAndRule;
        private System.Windows.Forms.SplitContainer splitCounters;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolTip tt;
        private System.Windows.Forms.DataGridView dgdSelectedCounters;
        private System.Windows.Forms.DataGridViewTextBoxColumn Counter;
        private System.Windows.Forms.DataGridViewCheckBoxColumn ShowInChart;
        private System.Windows.Forms.DataGridViewCheckBoxColumn HighlightInChart;
        private System.Windows.Forms.DataGridViewCheckBoxColumn WildcardWithTotal;
        private System.Windows.Forms.SplitContainer splitExpressionsAndRule;
        private System.Windows.Forms.SplitContainer splitExpressions;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DataGridView dgdExpressions;
        private System.Windows.Forms.DataGridViewTextBoxColumn ExpressionName;
        private System.Windows.Forms.DataGridViewTextBoxColumn Expression;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbValueToCheck;
        private System.Windows.Forms.Label lblSeriesFunction;
        private System.Windows.Forms.ComboBox cmbSeriesFunction;
        private System.Windows.Forms.NumericUpDown udPctMatchCheck;
        private System.Windows.Forms.Label lblPctMatchCheck;
        private System.Windows.Forms.Panel pnlMed;
        private System.Windows.Forms.Panel pnlLow;
        private System.Windows.Forms.Panel pnlHigh;
        private System.Windows.Forms.ComboBox cmbCheckAboveOrBelow;
        private System.Windows.Forms.ComboBox cmbValMed;
        private System.Windows.Forms.ComboBox cmbValLow;
        private System.Windows.Forms.ComboBox cmbValHigh;
    }
}