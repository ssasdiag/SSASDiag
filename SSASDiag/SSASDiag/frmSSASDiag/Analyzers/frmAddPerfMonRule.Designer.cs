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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle21 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle22 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmAddPerfMonRule));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle23 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle24 = new System.Windows.Forms.DataGridViewCellStyle();
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
            this.pnlRuleDetails = new System.Windows.Forms.Panel();
            this.cmbCheckAboveOrBelow = new System.Windows.Forms.ComboBox();
            this.pnlMed = new System.Windows.Forms.Panel();
            this.lblMedVal = new System.Windows.Forms.Label();
            this.txtMedResult = new System.Windows.Forms.TextBox();
            this.txtMedRegion = new System.Windows.Forms.TextBox();
            this.lblMedResultText = new System.Windows.Forms.Label();
            this.cmbValMed = new System.Windows.Forms.ComboBox();
            this.lblMedRegion = new System.Windows.Forms.Label();
            this.pnlLow = new System.Windows.Forms.Panel();
            this.lblLowVal = new System.Windows.Forms.Label();
            this.txtLowResult = new System.Windows.Forms.TextBox();
            this.txtLowRegion = new System.Windows.Forms.TextBox();
            this.lblLowResultText = new System.Windows.Forms.Label();
            this.cmbValLow = new System.Windows.Forms.ComboBox();
            this.lblLowRegion = new System.Windows.Forms.Label();
            this.pnlHigh = new System.Windows.Forms.Panel();
            this.lblHighVal = new System.Windows.Forms.Label();
            this.txtHighResult = new System.Windows.Forms.TextBox();
            this.txtHighRegion = new System.Windows.Forms.TextBox();
            this.cmbValHigh = new System.Windows.Forms.ComboBox();
            this.lblHighResultText = new System.Windows.Forms.Label();
            this.lblHighRegion = new System.Windows.Forms.Label();
            this.udPctMatchCheck = new System.Windows.Forms.NumericUpDown();
            this.lblPctMatchCheck = new System.Windows.Forms.Label();
            this.lblSeriesFunction = new System.Windows.Forms.Label();
            this.cmbSeriesFunction = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbValueToCheck = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.btnSaveRule = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.tt = new System.Windows.Forms.ToolTip(this.components);
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
            this.pnlRuleDetails.SuspendLayout();
            this.pnlMed.SuspendLayout();
            this.pnlLow.SuspendLayout();
            this.pnlHigh.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udPctMatchCheck)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitCountersAndRule
            // 
            this.splitCountersAndRule.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitCountersAndRule.Location = new System.Drawing.Point(0, 55);
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
            this.splitCountersAndRule.Size = new System.Drawing.Size(637, 650);
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
            this.splitCounters.Size = new System.Drawing.Size(637, 220);
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
            dataGridViewCellStyle21.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle21.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle21.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F);
            dataGridViewCellStyle21.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle21.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle21.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle21.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgdSelectedCounters.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle21;
            this.dgdSelectedCounters.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgdSelectedCounters.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Counter,
            this.ShowInChart,
            this.HighlightInChart,
            this.WildcardWithTotal});
            dataGridViewCellStyle22.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle22.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle22.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F);
            dataGridViewCellStyle22.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle22.SelectionBackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle22.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle22.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgdSelectedCounters.DefaultCellStyle = dataGridViewCellStyle22;
            this.dgdSelectedCounters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgdSelectedCounters.Location = new System.Drawing.Point(0, 0);
            this.dgdSelectedCounters.Margin = new System.Windows.Forms.Padding(0);
            this.dgdSelectedCounters.Name = "dgdSelectedCounters";
            this.dgdSelectedCounters.RowHeadersVisible = false;
            this.dgdSelectedCounters.RowTemplate.Height = 30;
            this.dgdSelectedCounters.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgdSelectedCounters.Size = new System.Drawing.Size(373, 220);
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
            this.splitExpressionsAndRule.Panel2.Controls.Add(this.pnlRuleDetails);
            this.splitExpressionsAndRule.Size = new System.Drawing.Size(637, 426);
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
            this.splitExpressions.Size = new System.Drawing.Size(637, 215);
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
            dataGridViewCellStyle23.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle23.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle23.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F);
            dataGridViewCellStyle23.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle23.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle23.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle23.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgdExpressions.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle23;
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
            dataGridViewCellStyle24.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F);
            dataGridViewCellStyle24.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle24.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle24.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle24.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgdExpressions.RowsDefaultCellStyle = dataGridViewCellStyle24;
            this.dgdExpressions.RowTemplate.DefaultCellStyle.ForeColor = System.Drawing.SystemColors.ControlText;
            this.dgdExpressions.RowTemplate.DefaultCellStyle.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            this.dgdExpressions.RowTemplate.DefaultCellStyle.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            this.dgdExpressions.RowTemplate.Height = 30;
            this.dgdExpressions.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgdExpressions.Size = new System.Drawing.Size(373, 215);
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
            // pnlRuleDetails
            // 
            this.pnlRuleDetails.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlRuleDetails.Controls.Add(this.cmbCheckAboveOrBelow);
            this.pnlRuleDetails.Controls.Add(this.pnlMed);
            this.pnlRuleDetails.Controls.Add(this.pnlLow);
            this.pnlRuleDetails.Controls.Add(this.pnlHigh);
            this.pnlRuleDetails.Controls.Add(this.udPctMatchCheck);
            this.pnlRuleDetails.Controls.Add(this.lblPctMatchCheck);
            this.pnlRuleDetails.Controls.Add(this.lblSeriesFunction);
            this.pnlRuleDetails.Controls.Add(this.cmbSeriesFunction);
            this.pnlRuleDetails.Controls.Add(this.label2);
            this.pnlRuleDetails.Controls.Add(this.cmbValueToCheck);
            this.pnlRuleDetails.Controls.Add(this.label4);
            this.pnlRuleDetails.Controls.Add(this.label5);
            this.pnlRuleDetails.Controls.Add(this.label6);
            this.pnlRuleDetails.Controls.Add(this.label10);
            this.pnlRuleDetails.Controls.Add(this.label9);
            this.pnlRuleDetails.Controls.Add(this.label8);
            this.pnlRuleDetails.Controls.Add(this.label7);
            this.pnlRuleDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRuleDetails.Location = new System.Drawing.Point(0, 0);
            this.pnlRuleDetails.Name = "pnlRuleDetails";
            this.pnlRuleDetails.Size = new System.Drawing.Size(637, 207);
            this.pnlRuleDetails.TabIndex = 0;
            // 
            // cmbCheckAboveOrBelow
            // 
            this.cmbCheckAboveOrBelow.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCheckAboveOrBelow.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F);
            this.cmbCheckAboveOrBelow.FormattingEnabled = true;
            this.cmbCheckAboveOrBelow.Items.AddRange(new object[] {
            "Values pass when above thresholds",
            "Values pass when below thresholds"});
            this.cmbCheckAboveOrBelow.Location = new System.Drawing.Point(229, 5);
            this.cmbCheckAboveOrBelow.Name = "cmbCheckAboveOrBelow";
            this.cmbCheckAboveOrBelow.Size = new System.Drawing.Size(169, 20);
            this.cmbCheckAboveOrBelow.TabIndex = 0;
            this.cmbCheckAboveOrBelow.SelectedIndexChanged += new System.EventHandler(this.cmbCheckAboveOrBelow_SelectedIndexChanged);
            // 
            // pnlMed
            // 
            this.pnlMed.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlMed.Controls.Add(this.lblMedVal);
            this.pnlMed.Controls.Add(this.txtMedResult);
            this.pnlMed.Controls.Add(this.txtMedRegion);
            this.pnlMed.Controls.Add(this.lblMedResultText);
            this.pnlMed.Controls.Add(this.cmbValMed);
            this.pnlMed.Controls.Add(this.lblMedRegion);
            this.pnlMed.Location = new System.Drawing.Point(227, 87);
            this.pnlMed.Name = "pnlMed";
            this.pnlMed.Size = new System.Drawing.Size(408, 60);
            this.pnlMed.TabIndex = 9;
            // 
            // lblMedVal
            // 
            this.lblMedVal.AutoSize = true;
            this.lblMedVal.BackColor = System.Drawing.Color.Transparent;
            this.lblMedVal.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMedVal.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblMedVal.Location = new System.Drawing.Point(32, 10);
            this.lblMedVal.Name = "lblMedVal";
            this.lblMedVal.Size = new System.Drawing.Size(52, 12);
            this.lblMedVal.TabIndex = 8;
            this.lblMedVal.Text = "Warn value";
            // 
            // txtMedResult
            // 
            this.txtMedResult.Location = new System.Drawing.Point(198, 32);
            this.txtMedResult.Name = "txtMedResult";
            this.txtMedResult.Size = new System.Drawing.Size(206, 20);
            this.txtMedResult.TabIndex = 7;
            this.tt.SetToolTip(this.txtMedResult, "Leave warn values empty to check only pass/fail.");
            // 
            // txtMedRegion
            // 
            this.txtMedRegion.Location = new System.Drawing.Point(198, 6);
            this.txtMedRegion.Name = "txtMedRegion";
            this.txtMedRegion.Size = new System.Drawing.Size(206, 20);
            this.txtMedRegion.TabIndex = 6;
            this.tt.SetToolTip(this.txtMedRegion, "Leave warn values empty to check only pass/fail.");
            // 
            // lblMedResultText
            // 
            this.lblMedResultText.AutoSize = true;
            this.lblMedResultText.BackColor = System.Drawing.Color.Transparent;
            this.lblMedResultText.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMedResultText.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblMedResultText.Location = new System.Drawing.Point(153, 36);
            this.lblMedResultText.Name = "lblMedResultText";
            this.lblMedResultText.Size = new System.Drawing.Size(45, 12);
            this.lblMedResultText.TabIndex = 10;
            this.lblMedResultText.Text = "Warn text";
            // 
            // cmbValMed
            // 
            this.cmbValMed.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbValMed.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F);
            this.cmbValMed.FormattingEnabled = true;
            this.cmbValMed.Location = new System.Drawing.Point(4, 28);
            this.cmbValMed.Name = "cmbValMed";
            this.cmbValMed.Size = new System.Drawing.Size(121, 20);
            this.cmbValMed.TabIndex = 1;
            this.tt.SetToolTip(this.cmbValMed, "Leave warn values empty to check only pass/fail.");
            // 
            // lblMedRegion
            // 
            this.lblMedRegion.AutoSize = true;
            this.lblMedRegion.BackColor = System.Drawing.Color.Transparent;
            this.lblMedRegion.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMedRegion.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblMedRegion.Location = new System.Drawing.Point(124, 12);
            this.lblMedRegion.Name = "lblMedRegion";
            this.lblMedRegion.Size = new System.Drawing.Size(75, 12);
            this.lblMedRegion.TabIndex = 9;
            this.lblMedRegion.Text = "Warn region label";
            // 
            // pnlLow
            // 
            this.pnlLow.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlLow.Controls.Add(this.lblLowVal);
            this.pnlLow.Controls.Add(this.txtLowResult);
            this.pnlLow.Controls.Add(this.txtLowRegion);
            this.pnlLow.Controls.Add(this.lblLowResultText);
            this.pnlLow.Controls.Add(this.cmbValLow);
            this.pnlLow.Controls.Add(this.lblLowRegion);
            this.pnlLow.Location = new System.Drawing.Point(227, 145);
            this.pnlLow.Name = "pnlLow";
            this.pnlLow.Size = new System.Drawing.Size(408, 60);
            this.pnlLow.TabIndex = 9;
            // 
            // lblLowVal
            // 
            this.lblLowVal.AutoSize = true;
            this.lblLowVal.BackColor = System.Drawing.Color.Transparent;
            this.lblLowVal.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLowVal.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblLowVal.Location = new System.Drawing.Point(32, 12);
            this.lblLowVal.Name = "lblLowVal";
            this.lblLowVal.Size = new System.Drawing.Size(50, 12);
            this.lblLowVal.TabIndex = 8;
            this.lblLowVal.Text = "Error value";
            // 
            // txtLowResult
            // 
            this.txtLowResult.Location = new System.Drawing.Point(198, 32);
            this.txtLowResult.Name = "txtLowResult";
            this.txtLowResult.Size = new System.Drawing.Size(206, 20);
            this.txtLowResult.TabIndex = 7;
            // 
            // txtLowRegion
            // 
            this.txtLowRegion.Location = new System.Drawing.Point(198, 6);
            this.txtLowRegion.Name = "txtLowRegion";
            this.txtLowRegion.Size = new System.Drawing.Size(206, 20);
            this.txtLowRegion.TabIndex = 6;
            // 
            // lblLowResultText
            // 
            this.lblLowResultText.AutoSize = true;
            this.lblLowResultText.BackColor = System.Drawing.Color.Transparent;
            this.lblLowResultText.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLowResultText.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblLowResultText.Location = new System.Drawing.Point(148, 36);
            this.lblLowResultText.Name = "lblLowResultText";
            this.lblLowResultText.Size = new System.Drawing.Size(51, 12);
            this.lblLowResultText.TabIndex = 10;
            this.lblLowResultText.Text = "Failure text";
            // 
            // cmbValLow
            // 
            this.cmbValLow.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbValLow.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F);
            this.cmbValLow.FormattingEnabled = true;
            this.cmbValLow.Location = new System.Drawing.Point(4, 28);
            this.cmbValLow.Name = "cmbValLow";
            this.cmbValLow.Size = new System.Drawing.Size(121, 20);
            this.cmbValLow.TabIndex = 1;
            // 
            // lblLowRegion
            // 
            this.lblLowRegion.AutoSize = true;
            this.lblLowRegion.BackColor = System.Drawing.Color.Transparent;
            this.lblLowRegion.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLowRegion.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblLowRegion.Location = new System.Drawing.Point(126, 12);
            this.lblLowRegion.Name = "lblLowRegion";
            this.lblLowRegion.Size = new System.Drawing.Size(73, 12);
            this.lblLowRegion.TabIndex = 9;
            this.lblLowRegion.Text = "Error region label";
            // 
            // pnlHigh
            // 
            this.pnlHigh.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlHigh.Controls.Add(this.lblHighVal);
            this.pnlHigh.Controls.Add(this.txtHighResult);
            this.pnlHigh.Controls.Add(this.txtHighRegion);
            this.pnlHigh.Controls.Add(this.cmbValHigh);
            this.pnlHigh.Controls.Add(this.lblHighResultText);
            this.pnlHigh.Controls.Add(this.lblHighRegion);
            this.pnlHigh.Location = new System.Drawing.Point(227, 30);
            this.pnlHigh.Name = "pnlHigh";
            this.pnlHigh.Size = new System.Drawing.Size(408, 60);
            this.pnlHigh.TabIndex = 8;
            // 
            // lblHighVal
            // 
            this.lblHighVal.AutoSize = true;
            this.lblHighVal.BackColor = System.Drawing.Color.Transparent;
            this.lblHighVal.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHighVal.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblHighVal.Location = new System.Drawing.Point(33, 11);
            this.lblHighVal.Name = "lblHighVal";
            this.lblHighVal.Size = new System.Drawing.Size(50, 12);
            this.lblHighVal.TabIndex = 3;
            this.lblHighVal.Text = "Error value";
            // 
            // txtHighResult
            // 
            this.txtHighResult.Location = new System.Drawing.Point(198, 31);
            this.txtHighResult.Name = "txtHighResult";
            this.txtHighResult.Size = new System.Drawing.Size(206, 20);
            this.txtHighResult.TabIndex = 2;
            // 
            // txtHighRegion
            // 
            this.txtHighRegion.Location = new System.Drawing.Point(198, 5);
            this.txtHighRegion.Name = "txtHighRegion";
            this.txtHighRegion.Size = new System.Drawing.Size(206, 20);
            this.txtHighRegion.TabIndex = 1;
            // 
            // cmbValHigh
            // 
            this.cmbValHigh.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbValHigh.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F);
            this.cmbValHigh.FormattingEnabled = true;
            this.cmbValHigh.Location = new System.Drawing.Point(3, 28);
            this.cmbValHigh.Name = "cmbValHigh";
            this.cmbValHigh.Size = new System.Drawing.Size(121, 20);
            this.cmbValHigh.TabIndex = 0;
            // 
            // lblHighResultText
            // 
            this.lblHighResultText.AutoSize = true;
            this.lblHighResultText.BackColor = System.Drawing.Color.Transparent;
            this.lblHighResultText.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHighResultText.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblHighResultText.Location = new System.Drawing.Point(145, 35);
            this.lblHighResultText.Name = "lblHighResultText";
            this.lblHighResultText.Size = new System.Drawing.Size(51, 12);
            this.lblHighResultText.TabIndex = 5;
            this.lblHighResultText.Text = "Failure text";
            // 
            // lblHighRegion
            // 
            this.lblHighRegion.AutoSize = true;
            this.lblHighRegion.BackColor = System.Drawing.Color.Transparent;
            this.lblHighRegion.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHighRegion.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblHighRegion.Location = new System.Drawing.Point(126, 11);
            this.lblHighRegion.Name = "lblHighRegion";
            this.lblHighRegion.Size = new System.Drawing.Size(73, 12);
            this.lblHighRegion.TabIndex = 4;
            this.lblHighRegion.Text = "Error region label";
            // 
            // udPctMatchCheck
            // 
            this.udPctMatchCheck.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F);
            this.udPctMatchCheck.Location = new System.Drawing.Point(82, 163);
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
            // lblPctMatchCheck
            // 
            this.lblPctMatchCheck.AutoSize = true;
            this.lblPctMatchCheck.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F);
            this.lblPctMatchCheck.Location = new System.Drawing.Point(45, 143);
            this.lblPctMatchCheck.Name = "lblPctMatchCheck";
            this.lblPctMatchCheck.Size = new System.Drawing.Size(124, 12);
            this.lblPctMatchCheck.TabIndex = 6;
            this.lblPctMatchCheck.Text = "% to match for error/warning:";
            this.lblPctMatchCheck.Visible = false;
            // 
            // lblSeriesFunction
            // 
            this.lblSeriesFunction.AutoSize = true;
            this.lblSeriesFunction.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F);
            this.lblSeriesFunction.Location = new System.Drawing.Point(37, 95);
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
            this.cmbSeriesFunction.Location = new System.Drawing.Point(17, 115);
            this.cmbSeriesFunction.Name = "cmbSeriesFunction";
            this.cmbSeriesFunction.Size = new System.Drawing.Size(181, 20);
            this.cmbSeriesFunction.TabIndex = 4;
            this.cmbSeriesFunction.Visible = false;
            this.cmbSeriesFunction.SelectedIndexChanged += new System.EventHandler(this.cmbSeriesFunction_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F);
            this.label2.Location = new System.Drawing.Point(60, 47);
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
            this.cmbValueToCheck.Location = new System.Drawing.Point(17, 67);
            this.cmbValueToCheck.Name = "cmbValueToCheck";
            this.cmbValueToCheck.Size = new System.Drawing.Size(181, 20);
            this.cmbValueToCheck.TabIndex = 2;
            this.cmbValueToCheck.SelectedIndexChanged += new System.EventHandler(this.cmbValueToCheck_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F);
            this.label4.Location = new System.Drawing.Point(211, 193);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(20, 12);
            this.label4.TabIndex = 10;
            this.label4.Text = "0__";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.Transparent;
            this.label5.Location = new System.Drawing.Point(212, 134);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(19, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "__";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.BackColor = System.Drawing.Color.Transparent;
            this.label6.Location = new System.Drawing.Point(212, 76);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(19, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "__";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.BackColor = System.Drawing.Color.Transparent;
            this.label10.Location = new System.Drawing.Point(218, 163);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(13, 13);
            this.label10.TabIndex = 16;
            this.label10.Text = "_";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.BackColor = System.Drawing.Color.Transparent;
            this.label9.Location = new System.Drawing.Point(218, 47);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(13, 13);
            this.label9.TabIndex = 15;
            this.label9.Text = "_";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.BackColor = System.Drawing.Color.Transparent;
            this.label8.Location = new System.Drawing.Point(218, 105);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(13, 13);
            this.label8.TabIndex = 14;
            this.label8.Text = "_";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.BackColor = System.Drawing.Color.Transparent;
            this.label7.Location = new System.Drawing.Point(212, 18);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(19, 13);
            this.label7.TabIndex = 13;
            this.label7.Text = "__";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.txtDescription);
            this.panel1.Controls.Add(this.label12);
            this.panel1.Controls.Add(this.txtName);
            this.panel1.Controls.Add(this.label11);
            this.panel1.Controls.Add(this.btnSaveRule);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(637, 55);
            this.panel1.TabIndex = 1;
            // 
            // txtDescription
            // 
            this.txtDescription.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F);
            this.txtDescription.Location = new System.Drawing.Point(323, 30);
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(310, 17);
            this.txtDescription.TabIndex = 5;
            this.txtDescription.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtDescription_KeyPress);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.55F);
            this.label12.Location = new System.Drawing.Point(266, 33);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(52, 12);
            this.label12.TabIndex = 4;
            this.label12.Text = "Description";
            // 
            // txtName
            // 
            this.txtName.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F);
            this.txtName.Location = new System.Drawing.Point(63, 30);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(196, 17);
            this.txtName.TabIndex = 3;
            this.txtName.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtName_KeyPress);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.55F);
            this.label11.Location = new System.Drawing.Point(6, 33);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(51, 12);
            this.label11.TabIndex = 2;
            this.label11.Text = "Rule Name";
            // 
            // btnSaveRule
            // 
            this.btnSaveRule.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F);
            this.btnSaveRule.Location = new System.Drawing.Point(558, 3);
            this.btnSaveRule.Name = "btnSaveRule";
            this.btnSaveRule.Size = new System.Drawing.Size(75, 23);
            this.btnSaveRule.TabIndex = 1;
            this.btnSaveRule.Text = "&Save";
            this.btnSaveRule.UseVisualStyleBackColor = true;
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
            // frmAddPerfMonRule
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(637, 705);
            this.Controls.Add(this.splitCountersAndRule);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1000, 744);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(650, 744);
            this.Name = "frmAddPerfMonRule";
            this.ShowInTaskbar = false;
            this.Text = "Create Rule";
            this.SizeChanged += new System.EventHandler(this.frmAddPerfMonRule_SizeChanged);
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
            this.pnlRuleDetails.ResumeLayout(false);
            this.pnlRuleDetails.PerformLayout();
            this.pnlMed.ResumeLayout(false);
            this.pnlMed.PerformLayout();
            this.pnlLow.ResumeLayout(false);
            this.pnlLow.PerformLayout();
            this.pnlHigh.ResumeLayout(false);
            this.pnlHigh.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udPctMatchCheck)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
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
        private System.Windows.Forms.Panel pnlRuleDetails;
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
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label lblMedRegion;
        private System.Windows.Forms.Label lblMedVal;
        private System.Windows.Forms.TextBox txtMedResult;
        private System.Windows.Forms.TextBox txtMedRegion;
        private System.Windows.Forms.Label lblMedResultText;
        private System.Windows.Forms.Label lblLowRegion;
        private System.Windows.Forms.Label lblLowVal;
        private System.Windows.Forms.TextBox txtLowResult;
        private System.Windows.Forms.TextBox txtLowRegion;
        private System.Windows.Forms.Label lblLowResultText;
        private System.Windows.Forms.Label lblHighRegion;
        private System.Windows.Forms.Label lblHighVal;
        private System.Windows.Forms.TextBox txtHighResult;
        private System.Windows.Forms.TextBox txtHighRegion;
        private System.Windows.Forms.Label lblHighResultText;
        private System.Windows.Forms.Button btnSaveRule;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label label11;
    }
}