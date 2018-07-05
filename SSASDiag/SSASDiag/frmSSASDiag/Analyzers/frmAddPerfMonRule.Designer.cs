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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmAddPerfMonRule));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
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
            this.panel2 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
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
            this.splitExpressionsAndRule.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitExpressions)).BeginInit();
            this.splitExpressions.Panel1.SuspendLayout();
            this.splitExpressions.Panel2.SuspendLayout();
            this.splitExpressions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgdExpressions)).BeginInit();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
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
            // 
            // splitCountersAndRule.Panel2
            // 
            this.splitCountersAndRule.Panel2.Controls.Add(this.splitExpressionsAndRule);
            this.splitCountersAndRule.Size = new System.Drawing.Size(636, 496);
            this.splitCountersAndRule.SplitterDistance = 136;
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
            this.splitCounters.Size = new System.Drawing.Size(636, 136);
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
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgdSelectedCounters.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgdSelectedCounters.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgdSelectedCounters.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Counter,
            this.ShowInChart,
            this.HighlightInChart,
            this.WildcardWithTotal});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgdSelectedCounters.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgdSelectedCounters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgdSelectedCounters.Location = new System.Drawing.Point(0, 0);
            this.dgdSelectedCounters.Margin = new System.Windows.Forms.Padding(0);
            this.dgdSelectedCounters.Name = "dgdSelectedCounters";
            this.dgdSelectedCounters.RowHeadersVisible = false;
            this.dgdSelectedCounters.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgdSelectedCounters.Size = new System.Drawing.Size(372, 136);
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
            this.splitExpressionsAndRule.Panel1.Controls.Add(this.panel2);
            this.splitExpressionsAndRule.Panel1MinSize = 215;
            this.splitExpressionsAndRule.Size = new System.Drawing.Size(636, 356);
            this.splitExpressionsAndRule.SplitterDistance = 215;
            this.splitExpressionsAndRule.TabIndex = 0;
            // 
            // splitExpressions
            // 
            this.splitExpressions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitExpressions.Location = new System.Drawing.Point(0, 30);
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
            this.splitExpressions.Size = new System.Drawing.Size(636, 185);
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
            this.label3.Size = new System.Drawing.Size(241, 180);
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
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgdExpressions.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dgdExpressions.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgdExpressions.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ExpressionName,
            this.Expression});
            this.dgdExpressions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgdExpressions.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dgdExpressions.GridColor = System.Drawing.SystemColors.Window;
            this.dgdExpressions.Location = new System.Drawing.Point(0, 0);
            this.dgdExpressions.Margin = new System.Windows.Forms.Padding(0);
            this.dgdExpressions.MultiSelect = false;
            this.dgdExpressions.Name = "dgdExpressions";
            this.dgdExpressions.RowHeadersVisible = false;
            this.dgdExpressions.RowHeadersWidth = 25;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F);
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            this.dgdExpressions.RowsDefaultCellStyle = dataGridViewCellStyle4;
            this.dgdExpressions.RowTemplate.DefaultCellStyle.ForeColor = System.Drawing.SystemColors.ControlText;
            this.dgdExpressions.RowTemplate.DefaultCellStyle.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            this.dgdExpressions.RowTemplate.DefaultCellStyle.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            this.dgdExpressions.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgdExpressions.Size = new System.Drawing.Size(372, 185);
            this.dgdExpressions.TabIndex = 3;
            this.dgdExpressions.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgdExpressions_CellEndEdit);
            this.dgdExpressions.DragDrop += new System.Windows.Forms.DragEventHandler(this.dgdExpressions_DragDrop);
            this.dgdExpressions.DragEnter += new System.Windows.Forms.DragEventHandler(this.dgdExpressions_DragEnter);
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
            // panel2
            // 
            this.panel2.Controls.Add(this.label2);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(636, 30);
            this.panel2.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F);
            this.label2.Location = new System.Drawing.Point(3, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(184, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "Define any expressions required for the rule:";
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
            // frmAddPerfMonRule
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(636, 527);
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
            ((System.ComponentModel.ISupportInitialize)(this.splitExpressionsAndRule)).EndInit();
            this.splitExpressionsAndRule.ResumeLayout(false);
            this.splitExpressions.Panel1.ResumeLayout(false);
            this.splitExpressions.Panel1.PerformLayout();
            this.splitExpressions.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitExpressions)).EndInit();
            this.splitExpressions.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgdExpressions)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
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
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label2;
    }
}