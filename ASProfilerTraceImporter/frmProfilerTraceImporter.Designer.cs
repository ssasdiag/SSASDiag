namespace ASProfilerTraceImporter
{
    partial class frmProfilerTraceImporter
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmProfilerTraceImporter));
            this.lblImporting = new System.Windows.Forms.Label();
            this.btnImport = new System.Windows.Forms.Button();
            this.txtFile = new System.Windows.Forms.TextBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnBrowseTrace = new System.Windows.Forms.Button();
            this.lblStatus2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtConn = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtTable = new System.Windows.Forms.TextBox();
            this.btnConnDlg = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblImporting
            // 
            this.lblImporting.AutoSize = true;
            this.lblImporting.Location = new System.Drawing.Point(12, 9);
            this.lblImporting.Name = "lblImporting";
            this.lblImporting.Size = new System.Drawing.Size(69, 13);
            this.lblImporting.TabIndex = 6;
            this.lblImporting.Text = "Importing file:";
            // 
            // btnImport
            // 
            this.btnImport.Location = new System.Drawing.Point(85, 103);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(80, 33);
            this.btnImport.TabIndex = 5;
            this.btnImport.Text = "Import";
            this.btnImport.UseVisualStyleBackColor = true;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // txtFile
            // 
            this.txtFile.Location = new System.Drawing.Point(12, 25);
            this.txtFile.Name = "txtFile";
            this.txtFile.ReadOnly = true;
            this.txtFile.Size = new System.Drawing.Size(401, 20);
            this.txtFile.TabIndex = 0;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(180, 105);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(0, 13);
            this.lblStatus.TabIndex = 10;
            // 
            // btnBrowseTrace
            // 
            this.btnBrowseTrace.Location = new System.Drawing.Point(419, 23);
            this.btnBrowseTrace.Name = "btnBrowseTrace";
            this.btnBrowseTrace.Size = new System.Drawing.Size(25, 23);
            this.btnBrowseTrace.TabIndex = 1;
            this.btnBrowseTrace.Text = "...";
            this.btnBrowseTrace.UseVisualStyleBackColor = true;
            this.btnBrowseTrace.Click += new System.EventHandler(this.BrowseForTrace_Click);
            // 
            // lblStatus2
            // 
            this.lblStatus2.AutoSize = true;
            this.lblStatus2.Location = new System.Drawing.Point(180, 120);
            this.lblStatus2.Name = "lblStatus2";
            this.lblStatus2.Size = new System.Drawing.Size(0, 13);
            this.lblStatus2.TabIndex = 11;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 54);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(64, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "Connection:";
            // 
            // txtConn
            // 
            this.txtConn.Location = new System.Drawing.Point(85, 51);
            this.txtConn.Name = "txtConn";
            this.txtConn.Size = new System.Drawing.Size(328, 20);
            this.txtConn.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(39, 80);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(37, 13);
            this.label1.TabIndex = 15;
            this.label1.Text = "Table:";
            // 
            // txtTable
            // 
            this.txtTable.Location = new System.Drawing.Point(85, 77);
            this.txtTable.Name = "txtTable";
            this.txtTable.Size = new System.Drawing.Size(328, 20);
            this.txtTable.TabIndex = 4;
            // 
            // btnConnDlg
            // 
            this.btnConnDlg.Location = new System.Drawing.Point(419, 49);
            this.btnConnDlg.Name = "btnConnDlg";
            this.btnConnDlg.Size = new System.Drawing.Size(25, 23);
            this.btnConnDlg.TabIndex = 3;
            this.btnConnDlg.Text = "...";
            this.btnConnDlg.UseVisualStyleBackColor = true;
            this.btnConnDlg.Click += new System.EventHandler(this.btnConnDlg_Click);
            // 
            // frmProfilerTraceImporter
            // 
            this.AcceptButton = this.btnImport;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(450, 145);
            this.Controls.Add(this.txtTable);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnConnDlg);
            this.Controls.Add(this.txtConn);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lblStatus2);
            this.Controls.Add(this.btnBrowseTrace);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.txtFile);
            this.Controls.Add(this.btnImport);
            this.Controls.Add(this.lblImporting);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.HelpButton = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmProfilerTraceImporter";
            this.Text = "AS Profiler Trace Importer";
            this.HelpButtonClicked += new System.ComponentModel.CancelEventHandler(this.frmProfilerTraceImporter_HelpButtonClicked);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmProfilerTraceImporter_FormClosing);
            this.Shown += new System.EventHandler(this.frmProfilerTraceImporter_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblImporting;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.TextBox txtFile;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnBrowseTrace;
        private System.Windows.Forms.Label lblStatus2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtConn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtTable;
        private System.Windows.Forms.Button btnConnDlg;
    }
}

