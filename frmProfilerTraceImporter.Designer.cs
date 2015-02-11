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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtTable = new System.Windows.Forms.TextBox();
            this.txtServer = new System.Windows.Forms.TextBox();
            this.lblImporting = new System.Windows.Forms.Label();
            this.cmbDatabase = new System.Windows.Forms.ComboBox();
            this.btnImport = new System.Windows.Forms.Button();
            this.txtFile = new System.Windows.Forms.TextBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(27, 62);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Server";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 88);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Database";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(31, 115);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(34, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Table";
            // 
            // txtTable
            // 
            this.txtTable.Location = new System.Drawing.Point(76, 112);
            this.txtTable.Name = "txtTable";
            this.txtTable.Size = new System.Drawing.Size(337, 20);
            this.txtTable.TabIndex = 4;
            // 
            // txtServer
            // 
            this.txtServer.Location = new System.Drawing.Point(76, 59);
            this.txtServer.Name = "txtServer";
            this.txtServer.Size = new System.Drawing.Size(337, 20);
            this.txtServer.TabIndex = 2;
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
            // cmbDatabase
            // 
            this.cmbDatabase.FormattingEnabled = true;
            this.cmbDatabase.Location = new System.Drawing.Point(76, 85);
            this.cmbDatabase.Name = "cmbDatabase";
            this.cmbDatabase.Size = new System.Drawing.Size(337, 21);
            this.cmbDatabase.TabIndex = 3;
            this.cmbDatabase.DropDown += new System.EventHandler(this.cmbDatabase_DropDown);
            // 
            // btnImport
            // 
            this.btnImport.Location = new System.Drawing.Point(76, 143);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(80, 23);
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
            this.lblStatus.Location = new System.Drawing.Point(174, 148);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(0, 13);
            this.lblStatus.TabIndex = 10;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(419, 23);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(25, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "...";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.BrowseForTrace_Click);
            // 
            // frmProfilerTraceImporter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(453, 175);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.txtFile);
            this.Controls.Add(this.btnImport);
            this.Controls.Add(this.cmbDatabase);
            this.Controls.Add(this.lblImporting);
            this.Controls.Add(this.txtServer);
            this.Controls.Add(this.txtTable);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.HelpButton = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "frmProfilerTraceImporter";
            this.Text = "AS Profiler Trace Importer";
            this.HelpButtonClicked += new System.ComponentModel.CancelEventHandler(this.frmProfilerTraceImporter_HelpButtonClicked);
            this.Shown += new System.EventHandler(this.frmProfilerTraceImporter_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtTable;
        private System.Windows.Forms.TextBox txtServer;
        private System.Windows.Forms.Label lblImporting;
        private System.Windows.Forms.ComboBox cmbDatabase;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.TextBox txtFile;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button button2;
    }
}

