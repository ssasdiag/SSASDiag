using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Trace;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;

namespace ASProfilerTraceImporter
{
    public partial class frmProfilerTraceImporter : Form
    {
        public frmProfilerTraceImporter()
        {
            InitializeComponent();
        }

        string TraceFilePath = "";
        string SQLDBServer = "";
        string SQLDB = "";
        string SQLTable = "";

        private void cmbDatabase_DropDown(object sender, System.EventArgs e)
        {
            SqlConnection conn = new System.Data.SqlClient.SqlConnection("Server="+txtServer.Text+";Initial Catalog=master;Persist Security Info=False;Integrated Security=true;");
            conn.Open();
            SqlCommand cmd = new System.Data.SqlClient.SqlCommand("select * from sys.databases", conn);
            SqlDataReader rdr = cmd.ExecuteReader();
            cmbDatabase.Items.Clear();
            while (rdr.Read())
                cmbDatabase.Items.Add(rdr.GetString(0));
            rdr.Close();
            conn.Close();            
        }

        public delegate void TraceSetTextDelegate(string text);
        public void SetTextCallback(string text)
        {
            lblStatus.Invoke(new Action(() => { lblStatus.Text = text; }));
            if (text.Length > "Done loading".Length && text.Substring(0, "Done loading".Length) == "Done loading")
                btnImport.Invoke(new Action(() => { btnImport.Text = "Import"; }));
        }
        public void SetTextCallback2(string text)
        {
            lblStatus2.Invoke(new Action(() => { lblStatus2.Text = text; }));
        }

        public delegate void TraceUpdateRowCountsDelegate();

        public void UpdateRowCounts()
        {
            SetTextCallback("Loaded " + String.Format("{0:#,##0}", RowCount) + " rows from " + tfps.Count + " files...");
        }

        static public bool bCancel = false;
        static public string cols = "";
        public List<TraceFileProcessor> tfps = new List<TraceFileProcessor>();

        public int RowCount
        {
            get
            {
                int TotalRowCounts = 0;
                for (int i = 0; i < tfps.Count; i++)
                    TotalRowCounts += tfps[i].RowCount;
                return TotalRowCounts;
            }
        }

        public SqlConnectionInfo cib = new SqlConnectionInfo();

        private void btnImport_Click(object sender, System.EventArgs e)
        {
            DateTime startTime = DateTime.Now;
            if (btnImport.Text == "Cancel...")
            {
                bCancel = true;
                btnImport.Text = "Import";
            }
            else
            {
                Properties.Settings.Default.Database = cmbDatabase.Text;
                Properties.Settings.Default.Server = txtServer.Text;
                Properties.Settings.Default.Table = txtTable.Text;
                Properties.Settings.Default.Save();

                cib.DatabaseName = Properties.Settings.Default.Database;
                cib.ServerName = Properties.Settings.Default.Server;
                cib.UseIntegratedSecurity = true;

                string path = txtFile.Text.Substring(0, txtFile.Text.LastIndexOf('\\'));
                string trcnum = "";
                string trcbase = txtFile.Text.Substring(txtFile.Text.LastIndexOf('\\') + 1);
                // Remove numbers trailing from trace file name (if they are there) as well as file extension...
                trcbase = trcbase.Substring(0, trcbase.Length - 4); int j = trcbase.Length - 1; while (Char.IsDigit(trcbase[j])) j--; trcnum = trcbase.Substring(j + 1); trcbase = trcbase.Substring(0, j + 1);
                FileInfo fi = new FileInfo(txtFile.Text);
                var fileInfos = Directory.GetFiles(path, trcbase + "*.trc", SearchOption.TopDirectoryOnly).Select(p => new FileInfo(p))
                    .OrderBy(x => x.CreationTime).Where(x => x.CreationTime >= fi.CreationTime).ToList();
                List<string> files = fileInfos.Select(x => x.FullName).ToList();

                bCancel = false;
                btnImport.Text = "Cancel...";
                
                Thread th = new Thread(() =>
                {
                    TraceSetTextDelegate SetText = new TraceSetTextDelegate(SetTextCallback);
                    TraceSetTextDelegate SetText2 = new TraceSetTextDelegate(SetTextCallback2);
                    List<Thread> workers = new List<Thread>();
                    int CurFile = -1;
                    cols = "";
                    tfps = new List<TraceFileProcessor>();
                    SetText2("");
                    SetText("");

                    Semaphore s = new Semaphore(1, System.Environment.ProcessorCount * 2); // throttles simultaneous threads to 2 * number of processors, starts with just 1 free thread until cols are initialized
                    foreach (string f in files)
                    {
                        if (!bCancel)
                        {
                            CurFile++;
                            TraceFileProcessor tfp = new TraceFileProcessor();
                            tfp.CurFile = CurFile;
                            tfp.FileName = f;
                            tfp.Table = txtTable.Text;
                            tfp.s = s;
                            tfp.tIn = new TraceFile();
                            tfp.tOut = new TraceTable();
                            tfp.frmProcessingParent = this;
                            tfps.Add(tfp);
                            workers.Add(new Thread(() => 
                                tfp.ProcessTraceFile()
                                ));
                            workers.Last().Start();
                        }
                    }
//                    foreach (Thread wrkr in workers) wrkr.Join();  // wait for all threads before merging
                    DateTime loadedTime = DateTime.Now;
                    if (!bCancel)
                    {
                        SqlConnection conn2 = new System.Data.SqlClient.SqlConnection(cib.ConnectionString);
                        conn2.Open();
                        for (int i = 0; i <= CurFile; i++)
                        {
                            workers[i].Join();
                            if (!bCancel)
                            {
                                if (i > 0)
                                {
                                    new SqlCommand("delete from [##" + txtTable.Text + "_" + i + "] where eventclass = 65528", conn2).ExecuteNonQuery();
                                    new SqlCommand("insert into [" + txtTable.Text + "] (" + cols + ") select " + cols + " from [##" + txtTable.Text + "_" + i + "]", conn2).ExecuteNonQuery();
                                    SetText2("Merging file " + (i + 1).ToString() + "...");
                                }
                            }
                            tfps[i].tIn.Close();
                            tfps[i].tOut.Close();
                        }
                        if (!bCancel)
                        {
                            SetText2("Merged " + (CurFile + 1).ToString() + " files.");
                            SetText("Done loading " + String.Format("{0:#,##0}", (RowCount + 1)) + " rows.");
                        }
                        conn2.Close();

                    }
                    if (bCancel)
                        SetText("Cancelled loading after reading " + String.Format("{0:#,##0}", (RowCount + 1)) + " rows.");

                    DateTime endTime = DateTime.Now;
                    TimeSpan timeToLoad = loadedTime.Subtract(startTime);
                    Debug.WriteLine("Load time: " + timeToLoad + "s");
                    TimeSpan timeToMerge = endTime.Subtract(loadedTime);
                    Debug.WriteLine("Merge time: " + timeToMerge + "s");
                });
                th.Start();
            }
        }

        public class TraceFileProcessor
        {
            public int RowCount = 0;
            public string Table;
            public int CurFile = 0;
            public string FileName = "";
            public Semaphore s;
            public TraceFile tIn;
            public TraceTable tOut;
            public frmProfilerTraceImporter frmProcessingParent;

            public void ProcessTraceFile()
            {
                s.WaitOne();  // throttle execution based on semaphore
                tIn.InitializeAsReader(FileName);
                tOut.InitializeAsWriter(tIn, frmProcessingParent.cib, CurFile == 0 ? Table : "##" + Table + "_" + CurFile);
                SqlConnection conn = new System.Data.SqlClient.SqlConnection(frmProcessingParent.cib.ConnectionString);
                conn.Open();
                bool bFirstFile = false;
                if (cols == "")
                {
                    // get column list from first trace file...
                    cols = new SqlCommand("SELECT SUBSTRING((SELECT ', ' + QUOTENAME(COLUMN_NAME) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + Table + "' AND COLUMN_NAME <> 'RowNumber' ORDER BY ORDINAL_POSITION FOR XML path('')), 3, 200000);", conn).ExecuteScalar() as string;
                    bFirstFile = true;
                    s.Release((System.Environment.ProcessorCount * 2));  // We blocked everything until we got initial cols, now we release them all to run...
                }
                else
                    if (cols != new SqlCommand("SELECT SUBSTRING((SELECT ', ' + QUOTENAME(COLUMN_NAME) FROM tempdb.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '##" + Table + "_" + CurFile + "' AND COLUMN_NAME <> 'RowNumber' ORDER BY ORDINAL_POSITION FOR XML path('')), 3, 200000);", conn).ExecuteScalar() as string)
                        // only happens if there is column mismatch between files - we won't read the whole file, just skip over...
                        return;
                conn.Close();
                TraceUpdateRowCountsDelegate UpdateRowCounts = new TraceUpdateRowCountsDelegate(frmProcessingParent.UpdateRowCounts);
                while (tOut.Write() && !bCancel)
                    if (RowCount++ % 256 == 0) UpdateRowCounts();
                // release this code for semaphore
                if (!bFirstFile) s.Release(); 
            }
        }

        private void BrowseForTrace_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Choose trace to import to table...";
            ofd.DefaultExt = "trc";
            ofd.Filter = "Profiler Traces|*.trc";
            ofd.Multiselect = false;
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                TraceFilePath = ofd.FileName;
                txtFile.Text = ofd.FileName;
            }
            this.Text = "AS Profiler Trace Importer - " + ofd.SafeFileName;
            if (txtFile.Text.Trim() == "") btnImport.Enabled = false; else btnImport.Enabled = true;
        }

        private void frmProfilerTraceImporter_Shown(object sender, EventArgs e)
        {
            if (SQLDBServer != "") txtServer.Text = SQLDBServer; else txtServer.Text = Properties.Settings.Default.Server;
            if (SQLDB != "") cmbDatabase.Text = SQLDB; else cmbDatabase.Text = Properties.Settings.Default.Database;
            if (SQLTable != "") txtTable.Text = SQLTable; else txtTable.Text = Properties.Settings.Default.Table;

            if (Environment.GetCommandLineArgs().Length == 1)
            {
                BrowseForTrace_Click(sender, e);
            }
            else
            {
                if (File.Exists(Environment.GetCommandLineArgs()[1]) && Environment.GetCommandLineArgs()[1].Substring(Environment.GetCommandLineArgs()[1].Length - 4) == ".trc")
                    TraceFilePath = Environment.GetCommandLineArgs()[1];
                else
                    BrowseForTrace_Click(sender, e);

                txtFile.Text = TraceFilePath;
                this.Text = "AS Profiler Trace Importer - " + TraceFilePath.Substring(TraceFilePath.LastIndexOf("\\"));
                
                if (Environment.GetCommandLineArgs().Length > 2)
                    SQLDBServer = Environment.GetCommandLineArgs()[2];
                if (Environment.GetCommandLineArgs().Length > 3)
                    SQLDB = Environment.GetCommandLineArgs()[3];
                if (Environment.GetCommandLineArgs().Length > 4)
                    SQLTable = Environment.GetCommandLineArgs()[4];
            }            
        }

        private void frmProfilerTraceImporter_HelpButtonClicked(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Process.Start("http://asprofilertraceimporter.codeplex.com");
        }
    }
}
