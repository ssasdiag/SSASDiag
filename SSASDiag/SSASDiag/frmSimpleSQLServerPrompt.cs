using System.Management;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.ServiceProcess;

namespace SSASDiag
{
    public partial class frmSimpleSQLServerPrompt : Form
    {
        public SqlConnection conn = new SqlConnection();

        public frmSimpleSQLServerPrompt()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {

            btnOK.Enabled = true;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            if (conn.State != ConnectionState.Closed)
                conn.Close();
            conn.Dispose();
            Close();
        }

        DataTable Servers = new DataTable();

        private void frmSimpleSQLServerPrompt_Load(object sender, EventArgs e)
        {
            cmbServer.Items.Clear();
            ServiceController[] services = ServiceController.GetServices();
            foreach (ServiceController s in services.OrderBy(ob => ob.DisplayName))
                if (s.DisplayName.Contains("SQL Server ("))
                {
                    string InstanceName = s.DisplayName.Replace("SQL Server (", "").Replace(")", "").Replace("MSSQLSERVER", "");
                    SqlConnection conn = new SqlConnection("Data Source=." + (InstanceName == "" ? "" : "\\" + InstanceName) + ";Integrated Security=true;Persist Security Info=false;Connection Timeout=1;");
                    try {
                        conn.Open();
                        cmbServer.Items.Add(Environment.MachineName + (InstanceName == "" ? "" : "\\" + InstanceName));
                        
                    } catch { }
                    finally
                    {
                        conn.Close();
                    }
                }

            if (cmbServer.Items.Count == 0)
            {
                cmbServer.Items.Add("No local SQL instances found.");
                btnOK.Enabled = false;
            }
            cmbServer.SelectedIndex = 0;
        }
    }
}
