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
using Microsoft.Data.ConnectionUI;
using System.Text.RegularExpressions;
using System.Reflection;

namespace ASProfilerTraceImporter
{
    public partial class frmProfilerTraceImporter : Form
    {
        public frmProfilerTraceImporter()
        {
            InitializeComponent();
        }

        string TraceFilePath = "";
        string ConnStr = "";
        string Table = "";
        static public bool bCancel = false;
        static public string cols = "";
        public SqlConnectionInfo2 cib = new SqlConnectionInfo2();
        public List<TraceFileProcessor> tfps = new List<TraceFileProcessor>();

        public delegate void TraceSetTextDelegate(string text);
        public void SetTextCallback(string text)
        {
            lblStatus.Invoke(new Action(() => { lblStatus.Text = text; }));
            if (text.StartsWith("Done loading"))
                btnImport.Invoke(new Action(() => { btnImport.Text = "Import"; }));
        }
        public void SetTextCallback2(string text)
        {
            lblStatus2.Invoke(new Action(() => { lblStatus2.Text = text; }));
        }

        public class SqlConnectionInfo2 : SqlConnectionInfo
        {
            public void SetConnectionString(string conn)
            {
                System.Security.SecureString s = new System.Security.SecureString();
                foreach (char c in conn) s.AppendChar(c);
                this.ConnectionStringInternal= s;
                this.RebuildConnectionStringInternal = false;
            }
        }

        public delegate void TraceUpdateRowCountsDelegate();

        public void UpdateRowCounts()
        {
            SetTextCallback("Loaded " + String.Format("{0:#,##0}", RowCount) + " rows from " + tfps.Count + " files...");
        }

        public int RowCount
        {
            get
            {
                int TotalRowCounts = 0;
                for (int i = 0; i < tfps.Count; i++) TotalRowCounts += tfps[i].RowCount;
                return TotalRowCounts;
            }
        }

        int ExtractNumberPartFromText(string text)
        {
            Match match = Regex.Match(text, @"(\d+)");
            if (match == null) return 0;
            int value;
            if (!int.TryParse(match.Value, out value)) return 0;
            return value;
        }

        private void btnImport_Click(object sender, System.EventArgs e)
        {
            try
            {
                DateTime startTime = DateTime.Now;
                if (btnImport.Text == "Cancel...")
                {
                    bCancel = true;
                    btnImport.Text = "Import";
                }
                else
                {
                    Properties.Settings.Default.ConnStr = txtConn.Text;
                    Properties.Settings.Default.Table = txtTable.Text;
                    Properties.Settings.Default.Save();
                    cib.SetConnectionString(txtConn.Text);

                    SqlConnection conn = new SqlConnection(txtConn.Text);
                    conn.Open();
                    if (new SqlCommand("select count(*) from sys.tables where name = '" + txtTable.Text + "'", conn).ExecuteScalar() as int? > 0)
                        if (MessageBox.Show("The table already exists.  Overwrite?", "Table Exists", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1) == System.Windows.Forms.DialogResult.No)
                            return;
                    conn.Close();

                    string path = txtFile.Text.Substring(0, txtFile.Text.LastIndexOf('\\') + 1);
                    string trcbase = txtFile.Text.Substring(txtFile.Text.LastIndexOf('\\') + 1);
                    // Remove numbers trailing from trace file name (if they are there) as well as file extension...
                    trcbase = trcbase.Substring(0, trcbase.Length - 4); int j = trcbase.Length - 1; while (Char.IsDigit(trcbase[j])) j--; trcbase = trcbase.Substring(0, j + 1);
                    FileInfo fi = new FileInfo(txtFile.Text);
                    var fileInfos = Directory.GetFiles(path, trcbase + "*.trc", SearchOption.TopDirectoryOnly).Select(p => new FileInfo(p))
                        .OrderBy(x => x.CreationTime).Where(x => x.CreationTime >= fi.CreationTime.Subtract(new TimeSpan(0, 0, 45))).ToList();
                    List<string> files = fileInfos.Select(x => x.Name.Substring(trcbase.Length)).ToList();
                    files.Sort((x, y) => ExtractNumberPartFromText(x).CompareTo(ExtractNumberPartFromText(y)));
                    if (txtFile.Text != path + trcbase + files[0])
                        files.RemoveRange(0, files.IndexOf(txtFile.Text.Substring((path + trcbase).Length)));
                    
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

                        Semaphore s = new Semaphore(1, System.Environment.ProcessorCount); // throttles simultaneous threads to number of processors, starts with just 1 free thread until cols are initialized
                        foreach (string f in files)
                            if (!bCancel)
                            {
                                CurFile++;
                                TraceFileProcessor tfp = new TraceFileProcessor(CurFile, path + trcbase + f, txtTable.Text, s, new TraceFile(), new TraceTable(), this);
                                tfps.Add(tfp);
                                workers.Add(new Thread(() => tfp.ProcessTraceFile()));
                                workers.Last().Start();
                            }

                        if (!bCancel)
                        {
                            SqlConnection conn2 = new System.Data.SqlClient.SqlConnection(cib.ConnectionString);
                            conn2.Open();
                            for (int i = 0; i <= CurFile; i++)
                            {
                                workers[i].Join();
                                if (!bCancel)
                                    if (i > 0)
                                    {
                                        new SqlCommand("delete from [##" + txtTable.Text + "_" + i + "] where eventclass = 65528", conn2).ExecuteNonQuery();
                                        new SqlCommand("insert into [" + txtTable.Text + "] (" + cols + ") select " + cols + " from [##" + txtTable.Text + "_" + i + "]", conn2).ExecuteNonQuery();
                                        SetText2("Merging file " + (i + 1).ToString() + "...");
                                    }

                                tfps[i].tIn.Close();
                                tfps[i].tOut.Close();
                            }
                            if (!bCancel)
                            {
                                BuildEventClassSubclassTables();
                                SetText2("Merged " + (CurFile + 1).ToString() + " files.");
                                SetText("Done loading " + String.Format("{0:#,##0}", (RowCount)) + " rows in " + Math.Round((DateTime.Now - startTime).TotalSeconds, 1) + "s.");
                                cols = new SqlCommand("SELECT SUBSTRING((SELECT ', t.' + QUOTENAME(COLUMN_NAME) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + Table + "' AND COLUMN_NAME <> 'RowNumber' ORDER BY ORDINAL_POSITION FOR XML path('')), 3, 200000);", conn2).ExecuteScalar() as string;
                                new SqlCommand("if exists(select * from sys.views where name = '" + Table + "v') drop view [" + Table + "v];", conn2).ExecuteNonQuery();
                                new SqlCommand("create view [" + Table + "v] as select " + cols.Replace("t.[EventClass]", "c.[Name] as EventClass, c.EventClassID").Replace("t.[EventSubclass]", "s.[Name] as EventSubclass, s.EventSubclassID") + " from " + Table + "  t left outer join ProfilerEventClass c on c.EventClassID = t.EventClass left outer join ProfilerEventSubclass s on s.EventClassID = t.EventClass and s.EventSubclassID = t.EventSubclass;", conn2).ExecuteNonQuery();
                            }
                            conn2.Close();
                        }
                        if (bCancel)
                        {
                            
                            SetText("Cancelled loading after reading " + String.Format("{0:#,##0}", (RowCount + 1)) + " rows.");
                            SetText2("");
                        }
                        foreach (Thread w in workers)
                            w.Join();
                    });
                    th.Start();
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.ToString(), "Oops!  Exception...");
            }
        }

        public class TraceFileProcessor
        {
            public int RowCount = 0;
            public string Table;
            public int CurFile = 0;
            public string FileName = "";
            public Semaphore Sem;
            public TraceFile tIn;
            public TraceTable tOut;
            public frmProfilerTraceImporter frmProcessingParent;

            public TraceFileProcessor(int curFile, string fileName, string table, Semaphore sem, TraceFile tin, TraceTable tout, frmProfilerTraceImporter parent)
            { CurFile = curFile; FileName = fileName; Table = table; Sem = sem; tIn = tin; tOut = tout; frmProcessingParent = parent; }

            public void ProcessTraceFile()
            {
                try
                {
                    Sem.WaitOne();  // throttle execution based on semaphore
                    tIn.InitializeAsReader(FileName);
                    tOut.InitializeAsWriter(tIn, frmProcessingParent.cib, CurFile == 0 ? Table : "##" + Table + "_" + CurFile);
                    SqlConnection conn = new System.Data.SqlClient.SqlConnection(frmProcessingParent.ConnStr);
                    conn.Open();
                    bool bFirstFile = false;
                    if (cols == "")
                    {
                        // get column list from first trace file...
                        cols = new SqlCommand("SELECT SUBSTRING((SELECT ', ' + QUOTENAME(COLUMN_NAME) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + Table + "' AND COLUMN_NAME <> 'RowNumber' ORDER BY ORDINAL_POSITION FOR XML path('')), 3, 200000);", conn).ExecuteScalar() as string;
                        bFirstFile = true;
                        Sem.Release(System.Environment.ProcessorCount);  // We blocked everything until we got initial cols, now we release them all to run...
                    }
                    else
                        if (cols != new SqlCommand("SELECT SUBSTRING((SELECT ', ' + QUOTENAME(COLUMN_NAME) FROM tempdb.INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '##" + Table + "_" + CurFile + "' AND COLUMN_NAME <> 'RowNumber' ORDER BY ORDINAL_POSITION FOR XML path('')), 3, 200000);", conn).ExecuteScalar() as string)
                            return;  // only happens if there is column mismatch between files - we won't read the whole file, just skip over...
                    conn.Close();
                    TraceUpdateRowCountsDelegate UpdateRowCounts = new TraceUpdateRowCountsDelegate(frmProcessingParent.UpdateRowCounts);
                    try
                    {
                        while (tOut.Write() && !bCancel)
                            if (RowCount++ % 256 == 0) UpdateRowCounts();
                    }
                    catch(SqlTraceException ste)
                    {

                    }
                    if (!bFirstFile) Sem.Release(); // release this code for next thread waiting on the semaphore 
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString() + "\r\n\r\nException occurred while loading file " + FileName + ".");
                }
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
            if (!File.Exists(Environment.CurrentDirectory + "\\Microsoft.Data.ConnectionUI.dll")) File.WriteAllBytes(Environment.CurrentDirectory + "\\Microsoft.Data.ConnectionUI.dll", Properties.Resources.Microsoft_Data_ConnectionUI);
            if (!File.Exists(Environment.CurrentDirectory + "\\Microsoft.Data.ConnectionUI.Dialog.dll")) File.WriteAllBytes(Environment.CurrentDirectory + "\\Microsoft.Data.ConnectionUI.Dialog.dll", Properties.Resources.Microsoft_Data_ConnectionUI_Dialog);
            txtConn.Text = ConnStr = Properties.Settings.Default.ConnStr;
            txtTable.Text = Table = Properties.Settings.Default.Table;

            if (Environment.GetCommandLineArgs().Length == 1)
                BrowseForTrace_Click(sender, e);
            else
            {
                if (File.Exists(Environment.GetCommandLineArgs()[1]) && Environment.GetCommandLineArgs()[1].Substring(Environment.GetCommandLineArgs()[1].Length - 4) == ".trc")
                    TraceFilePath = Environment.GetCommandLineArgs()[1];
                else
                    BrowseForTrace_Click(sender, e);

                txtFile.Text = TraceFilePath;
                this.Text = "AS Profiler Trace Importer - " + TraceFilePath.Substring(TraceFilePath.LastIndexOf("\\"));
                
                if (Environment.GetCommandLineArgs().Length > 2) ConnStr = Environment.GetCommandLineArgs()[2];
            }            
        }

        private void frmProfilerTraceImporter_HelpButtonClicked(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Process.Start("http://asprofilertraceimporter.codeplex.com");
        }

        private void btnConnDlg_Click(object sender, EventArgs e)
        {                
            DataConnectionDialog dcd = new DataConnectionDialog();
            DataSource sql = new DataSource("MicrosoftSqlServer", "Microsoft SQL Server");
            sql.Providers.Add(DataProvider.SqlDataProvider);
            dcd.DataSources.Add(sql);
            dcd.SelectedDataProvider = DataProvider.SqlDataProvider;
            dcd.SelectedDataSource = sql;
            dcd.ConnectionString = txtConn.Text;
            if (DataConnectionDialog.Show(dcd) == System.Windows.Forms.DialogResult.OK);
                txtConn.Text = ConnStr = dcd.ConnectionString;
            dcd.Close();
        }

        private void BuildEventClassSubclassTables()
        {
            SqlConnection conn = new SqlConnection(ConnStr);
            conn.Open();
            new SqlCommand(Properties.Resources.EventClassSubClassTablesScript, conn).ExecuteNonQuery();
            conn.Close();
        }
    }
}
