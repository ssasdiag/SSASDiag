namespace SSASDiag
{
    partial class frmSimpleSQLServerPrompt
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
            label1 = new System.Windows.Forms.Label();
            btnOK = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            cmbServer = new System.Windows.Forms.ComboBox();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(22, 30);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(250, 13);
            label1.TabIndex = 0;
            label1.Text = "Choose a local SQL instance to process trace data:";
            // 
            // btnOK
            // 
            btnOK.Location = new System.Drawing.Point(116, 88);
            btnOK.Name = "btnOK";
            btnOK.Size = new System.Drawing.Size(75, 23);
            btnOK.TabIndex = 2;
            btnOK.Text = "&OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += new System.EventHandler(btnOK_Click);
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btnCancel.Location = new System.Drawing.Point(197, 88);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(75, 23);
            btnCancel.TabIndex = 3;
            btnCancel.Text = "&Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += new System.EventHandler(btnCancel_Click);
            // 
            // cmbServer
            // 
            cmbServer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbServer.FormattingEnabled = true;
            cmbServer.Location = new System.Drawing.Point(25, 45);
            cmbServer.Name = "cmbServer";
            cmbServer.Size = new System.Drawing.Size(247, 21);
            cmbServer.TabIndex = 5;
            // 
            // frmSimpleSQLServerPrompt
            // 
            AcceptButton = btnOK;
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new System.Drawing.Size(299, 123);
            Controls.Add(cmbServer);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(label1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmSimpleSQLServerPrompt";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Analysis requires SQL server";
            Load += new System.EventHandler(frmSimpleSQLServerPrompt_Load);
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        public System.Windows.Forms.ComboBox cmbServer;
    }
}