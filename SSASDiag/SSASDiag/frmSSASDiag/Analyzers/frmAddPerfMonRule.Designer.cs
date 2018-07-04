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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmAddPerfMonRule));
            this.splitCountersAndRule = new System.Windows.Forms.SplitContainer();
            this.splitCounters = new System.Windows.Forms.SplitContainer();
            this.dgdSelectedCounters = new System.Windows.Forms.DataGridView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.tt = new System.Windows.Forms.ToolTip(this.components);
            this.Counter = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ShowInChart = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.HighlightInChart = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.WildcardWithTotal = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.splitCountersAndRule)).BeginInit();
            this.splitCountersAndRule.Panel1.SuspendLayout();
            this.splitCountersAndRule.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitCounters)).BeginInit();
            this.splitCounters.Panel2.SuspendLayout();
            this.splitCounters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgdSelectedCounters)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitCountersAndRule
            // 
            this.splitCountersAndRule.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitCountersAndRule.Location = new System.Drawing.Point(0, 37);
            this.splitCountersAndRule.Name = "splitCountersAndRule";
            this.splitCountersAndRule.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitCountersAndRule.Panel1
            // 
            this.splitCountersAndRule.Panel1.Controls.Add(this.splitCounters);
            this.splitCountersAndRule.Size = new System.Drawing.Size(800, 413);
            this.splitCountersAndRule.SplitterDistance = 286;
            this.splitCountersAndRule.TabIndex = 0;
            // 
            // splitCounters
            // 
            this.splitCounters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitCounters.Location = new System.Drawing.Point(0, 0);
            this.splitCounters.Name = "splitCounters";
            // 
            // splitCounters.Panel2
            // 
            this.splitCounters.Panel2.Controls.Add(this.dgdSelectedCounters);
            this.splitCounters.Size = new System.Drawing.Size(800, 286);
            this.splitCounters.SplitterDistance = 400;
            this.splitCounters.TabIndex = 0;
            // 
            // dgdSelectedCounters
            // 
            this.dgdSelectedCounters.AllowDrop = true;
            this.dgdSelectedCounters.AllowUserToAddRows = false;
            this.dgdSelectedCounters.AllowUserToResizeRows = false;
            this.dgdSelectedCounters.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgdSelectedCounters.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgdSelectedCounters.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgdSelectedCounters.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dgdSelectedCounters.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgdSelectedCounters.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Counter,
            this.ShowInChart,
            this.HighlightInChart,
            this.WildcardWithTotal});
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F);
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgdSelectedCounters.DefaultCellStyle = dataGridViewCellStyle4;
            this.dgdSelectedCounters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgdSelectedCounters.Location = new System.Drawing.Point(0, 0);
            this.dgdSelectedCounters.Margin = new System.Windows.Forms.Padding(0);
            this.dgdSelectedCounters.Name = "dgdSelectedCounters";
            this.dgdSelectedCounters.RowHeadersVisible = false;
            this.dgdSelectedCounters.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgdSelectedCounters.Size = new System.Drawing.Size(396, 286);
            this.dgdSelectedCounters.TabIndex = 0;
            this.dgdSelectedCounters.DragDrop += new System.Windows.Forms.DragEventHandler(this.dgdSelectedCounters_DragDrop);
            this.dgdSelectedCounters.DragEnter += new System.Windows.Forms.DragEventHandler(this.dgdSelectedCounters_DragEnter);
            this.dgdSelectedCounters.MouseDown += new System.Windows.Forms.MouseEventHandler(this.dgdSelectedCounters_MouseDown);
            this.dgdSelectedCounters.MouseMove += new System.Windows.Forms.MouseEventHandler(this.dgdSelectedCounters_MouseMove);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(800, 37);
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
            // Counter
            // 
            this.Counter.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Counter.HeaderText = "Counter Name";
            this.Counter.Name = "Counter";
            this.Counter.ReadOnly = true;
            // 
            // ShowInChart
            // 
            this.ShowInChart.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.ShowInChart.HeaderText = "Display";
            this.ShowInChart.Name = "ShowInChart";
            this.ShowInChart.Width = 42;
            // 
            // HighlightInChart
            // 
            this.HighlightInChart.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.HighlightInChart.HeaderText = "Highlight";
            this.HighlightInChart.Name = "HighlightInChart";
            this.HighlightInChart.Width = 47;
            // 
            // WildcardWithTotal
            // 
            this.WildcardWithTotal.HeaderText = "Include _Total in *";
            this.WildcardWithTotal.Name = "WildcardWithTotal";
            this.WildcardWithTotal.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.WildcardWithTotal.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.WildcardWithTotal.ToolTipText = "If the counter features a wildcard, include _Total in the child nodes.";
            // 
            // frmAddPerfMonRule
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.splitCountersAndRule);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmAddPerfMonRule";
            this.Text = "Create New Rule";
            this.Load += new System.EventHandler(this.frmAddPerfMonRule_Load);
            this.splitCountersAndRule.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitCountersAndRule)).EndInit();
            this.splitCountersAndRule.ResumeLayout(false);
            this.splitCounters.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitCounters)).EndInit();
            this.splitCounters.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgdSelectedCounters)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitCountersAndRule;
        private System.Windows.Forms.SplitContainer splitCounters;
        private System.Windows.Forms.DataGridView dgdSelectedCounters;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolTip tt;
        private System.Windows.Forms.DataGridViewTextBoxColumn Counter;
        private System.Windows.Forms.DataGridViewCheckBoxColumn ShowInChart;
        private System.Windows.Forms.DataGridViewCheckBoxColumn HighlightInChart;
        private System.Windows.Forms.DataGridViewCheckBoxColumn WildcardWithTotal;
    }
}