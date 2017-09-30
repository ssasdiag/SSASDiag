namespace SSASDiag
{
    partial class ucASDumpAnalyzer
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.splitDebugger = new System.Windows.Forms.SplitContainer();
            this.splitDumpList = new System.Windows.Forms.SplitContainer();
            this.spDumpDetails = new System.Windows.Forms.SplitContainer();
            this.dgdDumpList = new System.Windows.Forms.DataGridView();
            this.rtDumpDetails = new System.Windows.Forms.RichTextBox();
            this.btnAnalyzeDumps = new System.Windows.Forms.Button();
            this.pnStacks = new System.Windows.Forms.Panel();
            this.lblThreads = new System.Windows.Forms.Label();
            this.cmbThreads = new System.Windows.Forms.ComboBox();
            this.splitDumpOutput = new System.Windows.Forms.SplitContainer();
            this.rtbStack = new System.Windows.Forms.RichTextBox();
            this.mdxQuery = new SimpleMDXParser.SimpleMDXEditor();
            this.txtStatus = new System.Windows.Forms.RichTextBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splitDebugger)).BeginInit();
            this.splitDebugger.Panel1.SuspendLayout();
            this.splitDebugger.Panel2.SuspendLayout();
            this.splitDebugger.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitDumpList)).BeginInit();
            this.splitDumpList.Panel1.SuspendLayout();
            this.splitDumpList.Panel2.SuspendLayout();
            this.splitDumpList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spDumpDetails)).BeginInit();
            this.spDumpDetails.Panel1.SuspendLayout();
            this.spDumpDetails.Panel2.SuspendLayout();
            this.spDumpDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgdDumpList)).BeginInit();
            this.pnStacks.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitDumpOutput)).BeginInit();
            this.splitDumpOutput.Panel1.SuspendLayout();
            this.splitDumpOutput.Panel2.SuspendLayout();
            this.splitDumpOutput.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitDebugger
            // 
            this.splitDebugger.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitDebugger.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitDebugger.Location = new System.Drawing.Point(0, 0);
            this.splitDebugger.Name = "splitDebugger";
            this.splitDebugger.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitDebugger.Panel1
            // 
            this.splitDebugger.Panel1.Controls.Add(this.splitDumpList);
            // 
            // splitDebugger.Panel2
            // 
            this.splitDebugger.Panel2.Controls.Add(this.txtStatus);
            this.splitDebugger.Size = new System.Drawing.Size(638, 432);
            this.splitDebugger.SplitterDistance = 321;
            this.splitDebugger.TabIndex = 39;
            // 
            // splitDumpList
            // 
            this.splitDumpList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitDumpList.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitDumpList.Location = new System.Drawing.Point(0, 0);
            this.splitDumpList.Name = "splitDumpList";
            // 
            // splitDumpList.Panel1
            // 
            this.splitDumpList.Panel1.Controls.Add(this.spDumpDetails);
            this.splitDumpList.Panel1.Controls.Add(this.btnAnalyzeDumps);
            // 
            // splitDumpList.Panel2
            // 
            this.splitDumpList.Panel2.Controls.Add(this.pnStacks);
            this.splitDumpList.Panel2.Controls.Add(this.splitDumpOutput);
            this.splitDumpList.Size = new System.Drawing.Size(638, 321);
            this.splitDumpList.SplitterDistance = 275;
            this.splitDumpList.TabIndex = 0;
            // 
            // spDumpDetails
            // 
            this.spDumpDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spDumpDetails.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.spDumpDetails.Location = new System.Drawing.Point(0, 21);
            this.spDumpDetails.Name = "spDumpDetails";
            this.spDumpDetails.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // spDumpDetails.Panel1
            // 
            this.spDumpDetails.Panel1.Controls.Add(this.dgdDumpList);
            // 
            // spDumpDetails.Panel2
            // 
            this.spDumpDetails.Panel2.Controls.Add(this.rtDumpDetails);
            this.spDumpDetails.Panel2MinSize = 100;
            this.spDumpDetails.Size = new System.Drawing.Size(275, 300);
            this.spDumpDetails.SplitterDistance = 167;
            this.spDumpDetails.TabIndex = 3;
            // 
            // dgdDumpList
            // 
            this.dgdDumpList.AllowUserToAddRows = false;
            this.dgdDumpList.AllowUserToDeleteRows = false;
            this.dgdDumpList.AllowUserToResizeRows = false;
            this.dgdDumpList.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgdDumpList.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.dgdDumpList.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this.dgdDumpList.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dgdDumpList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgdDumpList.ColumnHeadersVisible = false;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgdDumpList.DefaultCellStyle = dataGridViewCellStyle1;
            this.dgdDumpList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgdDumpList.Location = new System.Drawing.Point(0, 0);
            this.dgdDumpList.Name = "dgdDumpList";
            this.dgdDumpList.ReadOnly = true;
            this.dgdDumpList.RowHeadersVisible = false;
            this.dgdDumpList.Size = new System.Drawing.Size(275, 167);
            this.dgdDumpList.TabIndex = 3;
            // 
            // rtDumpDetails
            // 
            this.rtDumpDetails.BackColor = System.Drawing.Color.Black;
            this.rtDumpDetails.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtDumpDetails.DetectUrls = false;
            this.rtDumpDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtDumpDetails.Font = new System.Drawing.Font("Consolas", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtDumpDetails.ForeColor = System.Drawing.Color.LightSkyBlue;
            this.rtDumpDetails.Location = new System.Drawing.Point(0, 0);
            this.rtDumpDetails.Name = "rtDumpDetails";
            this.rtDumpDetails.ReadOnly = true;
            this.rtDumpDetails.Size = new System.Drawing.Size(275, 129);
            this.rtDumpDetails.TabIndex = 40;
            this.rtDumpDetails.Text = "";
            // 
            // btnAnalyzeDumps
            // 
            this.btnAnalyzeDumps.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.btnAnalyzeDumps.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnAnalyzeDumps.FlatAppearance.BorderSize = 0;
            this.btnAnalyzeDumps.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.btnAnalyzeDumps.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnAnalyzeDumps.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAnalyzeDumps.Location = new System.Drawing.Point(0, 0);
            this.btnAnalyzeDumps.Name = "btnAnalyzeDumps";
            this.btnAnalyzeDumps.Size = new System.Drawing.Size(275, 21);
            this.btnAnalyzeDumps.TabIndex = 1;
            this.btnAnalyzeDumps.Text = "Analyze Selection";
            this.btnAnalyzeDumps.UseVisualStyleBackColor = false;
            this.btnAnalyzeDumps.Click += new System.EventHandler(this.btnAnalyzeDumps_Click);
            // 
            // pnStacks
            // 
            this.pnStacks.Controls.Add(this.lblThreads);
            this.pnStacks.Controls.Add(this.cmbThreads);
            this.pnStacks.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnStacks.Location = new System.Drawing.Point(0, 0);
            this.pnStacks.Name = "pnStacks";
            this.pnStacks.Size = new System.Drawing.Size(359, 21);
            this.pnStacks.TabIndex = 5;
            // 
            // lblThreads
            // 
            this.lblThreads.AutoSize = true;
            this.lblThreads.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblThreads.Location = new System.Drawing.Point(4, 5);
            this.lblThreads.Name = "lblThreads";
            this.lblThreads.Size = new System.Drawing.Size(0, 12);
            this.lblThreads.TabIndex = 2;
            this.lblThreads.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cmbThreads
            // 
            this.cmbThreads.Dock = System.Windows.Forms.DockStyle.Right;
            this.cmbThreads.FormattingEnabled = true;
            this.cmbThreads.Location = new System.Drawing.Point(287, 0);
            this.cmbThreads.Name = "cmbThreads";
            this.cmbThreads.Size = new System.Drawing.Size(72, 21);
            this.cmbThreads.TabIndex = 1;
            this.cmbThreads.Visible = false;
            this.cmbThreads.SelectedIndexChanged += new System.EventHandler(this.cmbThreads_SelectedIndexChanged);
            // 
            // splitDumpOutput
            // 
            this.splitDumpOutput.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.splitDumpOutput.Location = new System.Drawing.Point(0, 20);
            this.splitDumpOutput.Name = "splitDumpOutput";
            this.splitDumpOutput.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitDumpOutput.Panel1
            // 
            this.splitDumpOutput.Panel1.Controls.Add(this.rtbStack);
            // 
            // splitDumpOutput.Panel2
            // 
            this.splitDumpOutput.Panel2.Controls.Add(this.mdxQuery);
            this.splitDumpOutput.Panel2Collapsed = true;
            this.splitDumpOutput.Size = new System.Drawing.Size(359, 301);
            this.splitDumpOutput.SplitterDistance = 112;
            this.splitDumpOutput.TabIndex = 0;
            // 
            // rtbStack
            // 
            this.rtbStack.BackColor = System.Drawing.Color.Black;
            this.rtbStack.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbStack.DetectUrls = false;
            this.rtbStack.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbStack.Font = new System.Drawing.Font("Consolas", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtbStack.ForeColor = System.Drawing.Color.LightSkyBlue;
            this.rtbStack.Location = new System.Drawing.Point(0, 0);
            this.rtbStack.Name = "rtbStack";
            this.rtbStack.ReadOnly = true;
            this.rtbStack.Size = new System.Drawing.Size(359, 301);
            this.rtbStack.TabIndex = 45;
            this.rtbStack.Text = "";
            // 
            // mdxQuery
            // 
            this.mdxQuery.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mdxQuery.Location = new System.Drawing.Point(0, 0);
            this.mdxQuery.Name = "mdxQuery";
            this.mdxQuery.Size = new System.Drawing.Size(150, 46);
            this.mdxQuery.TabIndex = 1;
            this.mdxQuery.Text = "";
            // 
            // txtStatus
            // 
            this.txtStatus.BackColor = System.Drawing.Color.Black;
            this.txtStatus.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtStatus.DetectUrls = false;
            this.txtStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtStatus.Font = new System.Drawing.Font("Consolas", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtStatus.ForeColor = System.Drawing.Color.LightSkyBlue;
            this.txtStatus.Location = new System.Drawing.Point(0, 0);
            this.txtStatus.Name = "txtStatus";
            this.txtStatus.ReadOnly = true;
            this.txtStatus.Size = new System.Drawing.Size(638, 107);
            this.txtStatus.TabIndex = 39;
            this.txtStatus.Text = "";
            // 
            // ucASDumpAnalyzer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitDebugger);
            this.Name = "ucASDumpAnalyzer";
            this.Size = new System.Drawing.Size(638, 432);
            this.Load += new System.EventHandler(this.ucASDumpAnalyzer_Load);
            this.SizeChanged += new System.EventHandler(this.ucASDumpAnalyzer_SizeChanged);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.ucASDumpAnalyzer_Paint);
            this.splitDebugger.Panel1.ResumeLayout(false);
            this.splitDebugger.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitDebugger)).EndInit();
            this.splitDebugger.ResumeLayout(false);
            this.splitDumpList.Panel1.ResumeLayout(false);
            this.splitDumpList.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitDumpList)).EndInit();
            this.splitDumpList.ResumeLayout(false);
            this.spDumpDetails.Panel1.ResumeLayout(false);
            this.spDumpDetails.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spDumpDetails)).EndInit();
            this.spDumpDetails.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgdDumpList)).EndInit();
            this.pnStacks.ResumeLayout(false);
            this.pnStacks.PerformLayout();
            this.splitDumpOutput.Panel1.ResumeLayout(false);
            this.splitDumpOutput.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitDumpOutput)).EndInit();
            this.splitDumpOutput.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitDebugger;
        private System.Windows.Forms.SplitContainer splitDumpList;
        private System.Windows.Forms.Button btnAnalyzeDumps;
        private System.Windows.Forms.RichTextBox txtStatus;
        private System.Windows.Forms.SplitContainer spDumpDetails;
        private System.Windows.Forms.DataGridView dgdDumpList;
        private System.Windows.Forms.RichTextBox rtDumpDetails;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Panel pnStacks;
        private System.Windows.Forms.Label lblThreads;
        private System.Windows.Forms.ComboBox cmbThreads;
        private System.Windows.Forms.SplitContainer splitDumpOutput;
        private System.Windows.Forms.RichTextBox rtbStack;
        private SimpleMDXParser.SimpleMDXEditor mdxQuery;
    }
}
