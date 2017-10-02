using System;
using System.Net;
using System.Security.AccessControl;
using System.Management;
using System.ServiceProcess;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Data.SqlClient;
using System.IO;
using SimpleMDXParser;
using FastColoredTextBoxNS;

namespace SSASDiag
{
    public partial class ucASDumpAnalyzer : UserControl
    {
        public string DumpPath { get; set; }
        ManualResetEvent ResultReady = new ManualResetEvent(false);
        SqlConnection connDB;
        List<Dump> DumpFiles = new List<Dump>();
        string AnalysisPath = "";
        string DumpAnalysisId;
        Process p = new Process();

        private class Dump
        {
            public Dump Clone()
            {
                return (Dump)MemberwiseClone();
            }

            public string DumpPath { get; set; }
            public string DumpName
            {
                get { return DumpPath.Substring(DumpPath.LastIndexOf("\\") + 1); }
            }
            public bool Crash { get; set; }
            public bool Analyzed { get; set; }
            public string ASVersion { get; set; }
            public string DumpException { get; set; }
            public DateTime DumpTime { get; set; }
            public string ProcessID { get; set; }
            private List<Stack> _stacks;
            public List<Stack> Stacks { get { return _stacks; } set { _stacks = value; } }
            public List<Stack> OrderedStacks
            {
                get
                {
                    return _stacks.OrderBy(s => s.SortOrder).ToList();
                }
                set
                {
                    _stacks = value;
                }
            }
        }

        private class Stack
        {
            public int ThreadID { get; set; }
            public string CallStack { get; set; }
            public string Query { get; set; }
            public bool ExceptionThread { get; set; }
            public string SortOrder
            {
                get
                {
                    string prefix = "";
                    if (FormattedThreadID.Contains("exception"))
                        prefix = "0";
                    else if (FormattedThreadID.Contains("command"))
                        prefix = "1";
                    else if (FormattedThreadID.Contains("missing query"))
                        prefix = "3";
                    else if (FormattedThreadID.Contains(" "))
                        prefix = "2";
                    else
                        prefix = "4";
                    return prefix + FormattedThreadID;
                }
            }
            public string FormattedThreadID
            {
                get
                {
                    string q = Query.Trim();
                    return "~" + 
                           ThreadID + 
                           (q == "" ? "" : (q.StartsWith("There") ? ", missing query" : (q.StartsWith("<") ? ", XMLA command" : q.StartsWith("{") ? ", JSON command" : ", MDX query"))) + 
                           (ExceptionThread ? ", exception" : "");
                }
            }
        }


        public ucASDumpAnalyzer(string dumpPath, SqlConnection conndb)
        {
            InitializeComponent();
            DumpPath = dumpPath;
            connDB = new SqlConnection(conndb.ConnectionString);
            connDB.Open();
            Shown += UcASDumpAnalyzer_Shown;
            HandleDestroyed += UcASDumpAnalyzer_HandleDestroyed;

            splitDumpOutput.Panel2Collapsed = true;
            splitDebugger.Panel2Collapsed = true;

            if (Directory.Exists(dumpPath))
            {
                List<string> dumpfiles = Directory.GetFiles(DumpPath, "*.mdmp", SearchOption.AllDirectories).ToList();
                foreach (string f in dumpfiles)
                    DumpFiles.Add(new Dump() { DumpPath = f, Analyzed = false, Crash = false });
                AnalysisPath = DumpPath + "\\Analysis";
            }
            else
            {
                AnalysisPath = DumpPath.Substring(0, DumpPath.LastIndexOf("\\") + 1) + "Analysis";
                DumpFiles.Add(new Dump() { DumpPath = dumpPath, Analyzed = false, Crash = false });
            }

            if (!Directory.Exists(AnalysisPath))
                Directory.CreateDirectory(AnalysisPath);

            string[] DumpAnalyses = Directory.GetFiles(AnalysisPath, "SSASDiag_MemoryDump_Analysis_*.mdf");
            if (DumpAnalyses.Count() > 0)
                DumpAnalysisId = DumpAnalyses[0].Replace(AnalysisPath, "").Replace("SSASDiag_MemoryDump_Analysis_", "").Replace(".mdf", "").Replace("\\", "");
            else
                DumpAnalysisId = Guid.NewGuid().ToString();

            string sSvcUser = "";
            ServiceController[] services = ServiceController.GetServices();
            foreach (ServiceController s in services.OrderBy(ob => ob.DisplayName))
                if (s.DisplayName.Contains("SQL Server (" + (connDB.DataSource.Contains("\\") ? connDB.DataSource.Substring(connDB.DataSource.IndexOf("\\")) : "MSSQLSERVER")))
                {
                    SelectQuery sQuery = new SelectQuery("select name, startname, pathname from Win32_Service where name = \"" + s.ServiceName + "\"");
                    ManagementObjectSearcher mgmtSearcher = new ManagementObjectSearcher(sQuery);

                    foreach (ManagementObject svc in mgmtSearcher.Get())
                        sSvcUser = svc["startname"] as string;
                    if (sSvcUser.Contains(".")) sSvcUser = sSvcUser.Replace(".", Environment.UserDomainName);
                    if (sSvcUser == "LocalSystem") sSvcUser = "NT AUTHORITY\\SYSTEM";
                }

            DirectoryInfo dirInfo = new DirectoryInfo(AnalysisPath);
            DirectorySecurity dirSec = dirInfo.GetAccessControl();
            dirSec.AddAccessRule(new FileSystemAccessRule(sSvcUser, FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.None, AccessControlType.Allow));
            dirInfo.SetAccessControl(dirSec);

            SqlCommand cmd;
            if (File.Exists(MDFPath()))
            {
                cmd = new SqlCommand("IF NOT EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'" + DBName() + "') CREATE DATABASE [" + DBName() + "] ON (FILENAME = N'" + MDFPath() + "'),"
                                            + "(FILENAME = N'" + LDFPath() + "') "
                                            + "FOR ATTACH", connDB);
            }
            else
            {
                cmd = new SqlCommand(Properties.Resources.CreateDBSQLScript.
                                    Replace("<mdfpath/>", MDFPath()).
                                    Replace("<ldfpath/>", LDFPath()).
                                    Replace("<dbname/>", DBName())
                                    , connDB);
            }
            int ret = cmd.ExecuteNonQuery();
            cmd.CommandText = "USE [" + DBName() + "]";
            cmd.ExecuteNonQuery();
            cmd.CommandText = "if not exists (select * from sysobjects where name='StacksAndQueries' and xtype='U') CREATE TABLE[dbo].[StacksAndQueries]"
                                + " ([DumpPath][nvarchar](max) NOT NULL, [DumpProcessID] [nvarchar](10) NOT NULL, [ThreadID] [int] NOT NULL, [Stack] [nvarchar] (max) NOT NULL, [Query] [nvarchar] (max) NULL, [ExceptionThread] [bit] NOT NULL) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]";
            cmd.ExecuteNonQuery();
            cmd.CommandText = "if not exists(select* from sysobjects where name= 'Dumps' and xtype = 'U') CREATE TABLE[dbo].[Dumps]"
                                + " ([DumpPath] [nvarchar] (max) NOT NULL, [DumpProcessID] [nvarchar](10) NOT NULL, [ASVersion] [nvarchar] (15) NOT NULL, [DumpTime] [datetime] NULL, [CrashDump] [bit] NOT NULL, [DumpException] [nvarchar](max) NULL) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]";
            cmd.ExecuteNonQuery();

            
            cmd.CommandText = "select * from Dumps";
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Dump d = DumpFiles.Find(df => df.DumpPath == dr["DumpPath"] as string);
                if (d != null)
                {
                    d.Analyzed = true;
                    d.ASVersion = dr["ASVersion"] as string;
                    d.DumpTime = (DateTime)dr["DumpTime"];
                    d.Crash = (bool)dr["CrashDump"];
                    d.DumpException = dr["DumpException"] as string;
                    d.ProcessID = dr["DumpProcessID"] as string;
                    d.Stacks = new List<Stack>();
                }
            }
            dr.Close();
            cmd.CommandText = "select * from StacksAndQueries";
            dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                Dump d = DumpFiles.Find(df => df.DumpPath == dr["DumpPath"] as string);
                if (d != null)
                {
                    try
                    {
                        d.Stacks.Add(new Stack() { CallStack = dr["Stack"] as string, ThreadID = (int)dr["ThreadID"], Query = dr["Query"] as string, ExceptionThread = (bool)dr["ExceptionThread"] });
                    }
                    catch
                    {
                        // This would fail only if there was some corruption in the database and needn't be handled, but better not to fail in that case, so adding empty try/catch...
                    }
                }
            }
            dr.Close();
            dgdDumpList.DataSource = DumpFiles;
            dgdDumpList.DataBindingComplete += DgdDumpList_DataBindingComplete;

            xmlQuery.Zoom = 75;
            xmlQuery.Language = Language.XML;
            xmlQuery.Dock = DockStyle.Fill;
            xmlQuery.ShowLineNumbers = false;
            xmlQuery.Visible = false;
            splitDumpOutput.Panel2.Controls.Add(xmlQuery);
            xmlQuery.BringToFront();

            frmSSASDiag.LogFeatureUse("Dump Analysis", "Dumps analysis initalized for " + DumpFiles.Count + " dumps, " + DumpFiles.Where(d => !d.Analyzed).Count() + " of which still require analysis.");
            ValidateSymbolResolution();
        }

        int DataBindingCompletions = 0;
        private void DgdDumpList_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            DataBindingCompletions++;
            if (DataBindingCompletions > 3)  // Skip the first three binding messages when we are initializing...
            {
                dgdDumpList.Columns[0].Visible = false;
                dgdDumpList.Columns[2].Visible = false;
                dgdDumpList.Columns[3].Visible = false;
                dgdDumpList.Columns[4].Visible = false;
                dgdDumpList.Columns[5].Visible = false;
                dgdDumpList.Columns[6].Visible = false;
                dgdDumpList.Columns[7].Visible = false;

                foreach (DataGridViewRow r in dgdDumpList.Rows)
                {
                    try
                    {
                        Dump d = r.DataBoundItem as Dump;
                        if (d.Analyzed == false)
                        {
                            r.DefaultCellStyle.ForeColor = SystemColors.GrayText;
                            r.Cells[1].ToolTipText = "This dump has not been analyzed yet.  Select, then click Analyze Selection.";
                        }
                        else
                        if (d.Crash == false)
                        {
                            r.DefaultCellStyle.BackColor = SystemColors.ControlDark;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
                dgdDumpList.ClearSelection();
                dgdDumpList.SelectionChanged += dgdDumpList_SelectionChanged;
                rtDumpDetails.Text = "Memory dumps found: " + DumpFiles.Count + "\r\nDump(s) with existing analysis: " + DumpFiles.Where(d => d.Analyzed).Count();
            }
        }

        private async Task<bool> PrivateSymbolServerAccessible()
        {
            WebRequest wr = WebRequest.Create("http://symweb");
            WebResponse wres = await wr.GetResponseAsync();
            string res = (new StreamReader(wres.GetResponseStream())).ReadToEnd();
            return res.StartsWith("<html>");
        }

        private bool SomeSymDirExists(string SymDirList)
        {
            bool SymDirExists = false;
            foreach (string sym in SymDirList.Split(';'))
                if (Directory.Exists(sym))
                    SymDirExists = true;
            return SymDirExists;
        }

        frmStatusFloater StatusFloater = new frmStatusFloater();
        private void ValidateSymbolResolution()
        {
            
            StatusFloater.lblStatus.Text = "Checking symbol resolution for dump analysis...";
            StatusFloater.Left = Program.MainForm.Left + Program.MainForm.Width / 2 - StatusFloater.Width / 2;
            StatusFloater.Top = Program.MainForm.Top + Program.MainForm.Height / 2 - StatusFloater.Height / 2;
            Visible = false;
            StatusFloater.Show(Program.MainForm);
            new Thread(new ThreadStart(() =>
            {
                bool symbolsResolved = false;
                string sympath = Environment.GetEnvironmentVariable("_NT_SYMBOL_PATH");
                if (sympath != null && (sympath.Contains("http://symweb") || sympath.Contains("https://symweb")))
                {
                    if (PrivateSymbolServerAccessible().Result)
                        symbolsResolved = true;
                }
                else
                {
                    if (PrivateSymbolServerAccessible().Result)
                    {
                        Directory.CreateDirectory(AnalysisPath + "\\Symbols");
                        Environment.SetEnvironmentVariable("_NT_SYMBOL_PATH", "srv*" + AnalysisPath + "\\Symbols*http://symweb", EnvironmentVariableTarget.Machine);
                        symbolsResolved = true;
                    }
                }
                StatusFloater.Invoke(new System.Action(() =>
                {
                    StatusFloater.Close();
                    StatusFloater = null;
                }));
                if (!symbolsResolved)
                {
                    if (sympath != null)
                    {
                        sympath = sympath.Replace("*http://symweb/", "").Replace("*https://symweb/", "").Replace("*http://symweb", "").Replace("*https://symweb", "").Replace("srv*", "");
                        if (SomeSymDirExists(sympath))
                        {
                            if (MessageBox.Show("Private symbol server could not be resolved. Microsoft private network required. " +
                                       "The client has a local cache and could contain private symbols for dump analaysis.\r\n\r\n" +
                                       "Proceed to attempt analysis with the local cache?\r\n\r\nNOTE: Analysis requires valid private symbol resolution.", "Symbol Server Resolution Failed", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
                                == DialogResult.Yes)
                                symbolsResolved = true;
                        }
                    }
                    if (!symbolsResolved)
                    {
                        MessageBox.Show("Symbol resolution failed. Dumps require private symbol resolution. " +
                                        "Create a SYSTEM environment variable called _NT_SYMBOL_PATH that resolves private symbols for msmdsrv.exe.\r\n\r\n" +
                                        "Dump analysis disabled for now.", "Dump Symbol Resolution Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        txtStatus.Text = "Dump analysis disabled because symbol resolution failed.\r\n" +
                            "Set a SYSTEM environment variable called _NT_SYMBOL_PATH.\r\nUse a valid path to private symbols for msmdsrv.exe.";
                        splitDebugger.Panel1Collapsed = true;
                    }
                }
                Invoke(new System.Action(() =>Visible = true));
            })).Start();
        }

        private void UcASDumpAnalyzer_HandleDestroyed(object sender, EventArgs e)
        {
            try
            {
                frmSSASDiag.LogFeatureUse("Dump Analysis", "Detatching from dump analysis database on exit.");
                connDB.ChangeDatabase("master");
                SqlCommand cmd = new SqlCommand("IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'" + DBName() + "') ALTER DATABASE [" + DBName() + "] SET SINGLE_USER WITH ROLLBACK IMMEDIATE", connDB);
                cmd.ExecuteNonQuery();
                cmd = new SqlCommand("IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'" + DBName() + "') EXEC master.dbo.sp_detach_db @dbname = N'" + DBName() + "'", connDB);
                cmd.ExecuteNonQuery();
                connDB.Close();
            }
            catch
            {
                // Closing connection could fail if the database is in use or something.  Just ignore - we're closing, don't notify user...
            }
        }

        private string DBName()
        {
            return "SSASDiag_MemoryDump_Analysis_" + DumpAnalysisId;
        }

        private string MDFPath()
        {
            return AnalysisPath + "\\" + DBName() + ".mdf";
        }
        private string LDFPath()
        {
            return AnalysisPath + "\\" + DBName() + ".ldf";
        }

        private void UcASDumpAnalyzer_Shown(object sender, EventArgs e)
        {

        }

        public bool bCancel = false;
        int DumpCountAnalyzedInCurrentRun = 0;
        private void btnAnalyzeDumps_Click(object sender, EventArgs e)
        {
            if (btnAnalyzeDumps.Text == "Analyze Selection")
            {
                DumpCountAnalyzedInCurrentRun = 0;
                bCancel = false;
                btnAnalyzeDumps.BackColor = Color.Pink;
                btnAnalyzeDumps.FlatAppearance.MouseDownBackColor = Color.IndianRed;
                btnAnalyzeDumps.FlatAppearance.MouseOverBackColor = Color.LightCoral;
                btnAnalyzeDumps.Text = "Cancel Analysis In-Progress...";
                txtStatus.SelectionStart = txtStatus.TextLength;
                txtStatus.ScrollToCaret();
                new Thread(new ThreadStart(() =>
                {
                    int TotalSelectedDumpsCount = 0;
                    foreach (DataGridViewRow r in dgdDumpList.Rows)
                        if (!(r.DataBoundItem as Dump).Analyzed && r.Cells[1].Selected) TotalSelectedDumpsCount++;
                    if (TotalSelectedDumpsCount > 0)
                    {
                        rtDumpDetails.Invoke(new System.Action(() => rtDumpDetails.Text = "Analyzing " + TotalSelectedDumpsCount + " dump" + (TotalSelectedDumpsCount == 1 ? "" : "s") + "."));
                        List<DataGridViewRow> DumpsToProcess = new List<DataGridViewRow>();
                        foreach (DataGridViewRow r in dgdDumpList.Rows)
                            if (r.Cells[1].Selected)
                                DumpsToProcess.Add(r);
                        int DumpsRequiringAnalysis = DumpsToProcess.Where(drow => !(drow.DataBoundItem as Dump).Analyzed).Count();
                        frmSSASDiag.LogFeatureUse("Dump Analysis", "Analysis started on " + DumpsRequiringAnalysis + " dump" + (DumpsRequiringAnalysis > 1 ? "s." : "."));
                        foreach (DataGridViewRow r in DumpsToProcess)
                        {
                            if (!bCancel)
                            {
                                Dump d = r.DataBoundItem as Dump;
                                DataGridViewCell c = r.Cells[1];
                                if (!d.Analyzed)
                                {
                                    splitDebugger.Invoke(new System.Action(() =>
                                    {
                                        splitDebugger.Panel2Collapsed = false;
                                        lblDebugger.Text = "Analyzing " + d.DumpName + ", dump " + (DumpCountAnalyzedInCurrentRun + 1) + " of " + TotalSelectedDumpsCount + " to be analyzed.";
                                        txtStatus.Text = "Starting analysis of the memory dump at " + d.DumpPath + ".";
                                    }));
                                    ConnectToDump(d.DumpPath);
                                    while (!bCancel && !p.HasExited)
                                        Thread.Sleep(500);
                                    // Clean up the old process and reinitialize.
                                    if (bCancel)
                                    {
                                        p.CancelOutputRead();
                                        p.CancelErrorRead();
                                        if (!p.HasExited)
                                            p.Kill();
                                        ResultReady.Set();
                                    }
                                    else
                                    {
                                        dgdDumpList.Invoke(new System.Action(() =>
                                        {
                                            c.Style.ForeColor = SystemColors.ControlText;
                                            c.ToolTipText = "";
                                        }));
                                        DumpCountAnalyzedInCurrentRun++;
                                    }
                                    p.OutputDataReceived -= P_OutputDataReceived;
                                    p.ErrorDataReceived -= P_ErrorDataReceived;
                                    p.Exited -= P_Exited;
                                    p.Close();
                                    p = new Process();
                                }
                            }
                        }
                        if (!bCancel)
                        {
                            dgdDumpList.Invoke(new System.Action(() =>
                            {
                                SuspendLayout();
                                lblDebugger.Text = rtDumpDetails.Text = "Analyzed " + TotalSelectedDumpsCount + " memory dump" + (TotalSelectedDumpsCount != 1 ? "s." : ".");
                                btnAnalyzeDumps.Text = "";
                                btnAnalyzeDumps.BackColor = SystemColors.Control;
                                btnAnalyzeDumps.Enabled = false;
                                dgdDumpList_SelectionChanged(null, null);
                                splitDebugger.Panel2Collapsed = false;
                                ResumeLayout();
                                frmSSASDiag.LogFeatureUse("Dump Analysis", "Completed analysis of " + DumpsRequiringAnalysis + " dump" + (DumpsRequiringAnalysis > 1 ? "s." : "."));
                            }));
                        }
                    }
                })).Start();
            }
            else
            {
                bCancel = true;
                btnAnalyzeDumps.Text = "Analyze Selection";
                btnAnalyzeDumps.BackColor = Color.DarkSeaGreen;
                btnAnalyzeDumps.FlatAppearance.MouseDownBackColor = Color.FromArgb(128, 255, 128);
                btnAnalyzeDumps.FlatAppearance.MouseOverBackColor = Color.FromArgb(192, 255, 192);
                lblDebugger.Text = rtDumpDetails.Text = "Analyzed " + DumpCountAnalyzedInCurrentRun + " memory dump" + (DumpCountAnalyzedInCurrentRun != 1 ? "s" : "") + " before user cancelled.";
                frmSSASDiag.LogFeatureUse("Dump Analysis", "Dump analysis cancelled after " + DumpCountAnalyzedInCurrentRun + " dump" + (DumpCountAnalyzedInCurrentRun != 1 ? "s" : "") + " were analyzed successfully.");
            }
        }

        private string GetQueryFromStack(string stk, int tid)
        {
            string qry = "";
            List<string> Lines = new List<string>(stk.Split(new char[] { '\r', '\n' }));
            if (Lines.Where(c => c.Contains("PXSession")).Count() > 0)
            {
                string res = SubmitDebuggerCommand("~" + tid + "s");
                string PXSessionLine = Lines.Where(l => l.Contains("PXSession")).First();
                res = SubmitDebuggerCommand(".frame " + PXSessionLine.Substring(0, PXSessionLine.IndexOf(" ")));
                res = SubmitDebuggerCommand("dt this m_strLastRequest");
                string addy = res.Substring(res.IndexOf("Type PXSession*\r\n") + "Type PXSession*\r\n".Length);
                addy = addy.Substring(0, addy.IndexOf(" "));
                string offset = res.Substring(res.IndexOf(addy) + addy.Length);
                offset = offset.Substring(offset.IndexOf("+"));
                offset = offset.Substring(0, offset.IndexOf(" "));
                res = SubmitDebuggerCommand("dt PFString " + addy + offset);
                addy = res.Substring(res.IndexOf(" : ") + " : ".Length);
                try
                {
                    addy = addy.Substring(0, addy.IndexOf("PFData<") - 1);
                    qry = SubmitDebuggerCommand(".printf \"%mu\", " + addy);
                    int PromptLocation = qry.LastIndexOf(CurrentPrompt);
                    if (PromptLocation != -1)
                        qry = qry.Substring(0, qry.LastIndexOf(CurrentPrompt));
                }
                catch
                {
                    // I should insert backup approach to try dU if .printf fails as can happen if partial memory is present so no null-terminator is found on the string...
                    qry = "There was a query on this thread but its memory could not be read (possibly not captured in this minidump).";
                }
            }
            return qry;
        }

        public void ConnectToDump(string path)
        {
            LastResponse = "";
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = connDB;
            p.OutputDataReceived += P_OutputDataReceived;
            p.ErrorDataReceived += P_ErrorDataReceived;
            p.Exited += P_Exited;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardInput = true;
            p.EnableRaisingEvents = true;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = "/c \"\"" + Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\CDB\\cdb.exe\" -z \"" + path + "\"\"";
            p.Start();
            p.BeginErrorReadLine();
            p.BeginOutputReadLine();
            p.StandardInput.WriteLine(".echo \"EndOfData\""); // Ensures we can detect end of output, when this is processed and input prompt is displayed in console output...
            new Thread(new ThreadStart(() =>
                {
                    Dump d = DumpFiles.Find(df => df.DumpPath == path);
                    ResultReady.Reset();
                    ResultReady.WaitOne();
                    ResultReady.Reset();
                    string init = LastResponse;
                    if (bCancel)
                        return;
                    d.ASVersion = SubmitDebuggerCommand("lmvm msmdsrv").Split(new char[] { '\r', '\n' }).Where(f => f.Contains("Product version:")).First().Replace("    Product version:  ", "");
                    d.Crash = init.Split(new char[] { '\r', '\n' }).Where(f => f.StartsWith("This dump file has an exception of interest stored in it.")).Count() > 0;
                    string pid = init.Substring(init.IndexOf("This dump file has an exception of interest stored in it."));
                    pid = pid.Substring(pid.IndexOf("\r\n") + 2);
                    pid = pid.Substring(pid.IndexOf("\r\n") + 2);
                    pid = pid.Substring(0, pid.IndexOf("\r\n"));
                    d.DumpException = pid.Substring(pid.IndexOf("): ") + "): ".Length);
                    pid = pid.Substring(0, pid.IndexOf("): ")).Replace("(", "");
                    pid = pid.Substring(0, pid.IndexOf("."));
                    d.ProcessID = pid;
                    string debugTime = init.Split(new char[] { '\r', '\n' }).Where(f => f.StartsWith("Debug session time: ")).First().Replace("Debug session time: ", "").Trim();
                    string[] timeparts = debugTime.Replace("   ", " ").Replace("  ", " ").Split(new char[] { ' ' });
                    string properTime = timeparts[1] + " " + timeparts[2] + ", " + timeparts[4] + " " + timeparts[3] + " " + timeparts[6] + timeparts[7].Replace(")", "");
                    DateTime dt;
                    DateTime.TryParseExact(properTime, "MMM d, yyyy HH:mm:ss.fff zzz", null, System.Globalization.DateTimeStyles.AssumeLocal, out dt);
                    d.DumpTime = dt;

                    if (bCancel)
                        return;
                    cmd.CommandText = "INSERT INTO Dumps VALUES('" + d.DumpPath + "', '" + pid + "', '" + d.ASVersion + "', '" + dt.ToString() + "', " + (d.Crash ? 1 : 0) + ", '" + d.DumpException + "')";
                    cmd.ExecuteNonQuery();

                    d.Stacks = new List<Stack>();
                    string stk = SubmitDebuggerCommand("kN").Trim();
                    int tid = Convert.ToInt32((CurrentPrompt.Replace(">", "").Replace(" ", "").Replace(":", "")));
                    string qry = GetQueryFromStack(stk, tid);

                    if (bCancel)
                        return;
                    
                    d.Stacks.Add(new Stack() { CallStack = stk, ThreadID = tid, Query = qry, ExceptionThread = true });
                    frmSSASDiag.LogFeatureUse("Dump Analysis", ("Analysis of dump " + path + " shows the following exception stack:\r\n" + stk.Replace("'", "''")));
                    cmd.CommandText = "INSERT INTO StacksAndQueries VALUES('" + d.DumpPath + "', '" + pid + "'," + CurrentPrompt.Replace(">", "").Replace(" ", "").Replace(":", "") + ", '" + stk.Replace("'", "''") + "', '" + qry.Replace("'", "''") + "', 1)";
                    cmd.ExecuteNonQuery();

                    // Process non-exception threads
                    List<string> AllThreads = new List<string>();
                    while (AllThreads.Count <= 1 && !bCancel)  // hack - sometimes it doesn't execute the first try, don't know why.
                        AllThreads = SubmitDebuggerCommand("~*kN").Replace("\n", "").Split(new char[] { '\r'}).ToList();
                    if (!bCancel)
                    {
                        for (int i = 0; i < AllThreads.Count; i++)
                        {
                            if (!bCancel)
                            {
                                string l = AllThreads[i];
                                if (l.StartsWith(" # Child-SP") && !AllThreads[i - 1].StartsWith("#"))  // The exception thread already captured is denoted in output with # so we can skip it...
                                {
                                    Stack s = new Stack() { ExceptionThread = false };
                                    s.ThreadID = Convert.ToInt32(AllThreads[i - 1].Substring(0, AllThreads[i - 1].IndexOf(" Id: ")).TrimStart());
                                    int j = i;
                                    for (; AllThreads[j] != ""; j++)
                                        s.CallStack += AllThreads[j] + "\r\n";
                                    i = j;
                                    s.CallStack = s.CallStack.TrimEnd();
                                    s.Query = GetQueryFromStack(s.CallStack, s.ThreadID);
                                    if (d.Stacks.Find(st => st.ThreadID == s.ThreadID) == null && !bCancel)
                                    {
                                        d.Stacks.Add(s);
                                        cmd.CommandText = "INSERT INTO StacksAndQueries VALUES('" + d.DumpPath + "', '" + pid + "', '" + s.ThreadID + "', '" + s.CallStack.Replace("'", "''") + "', '" + s.Query.Replace("'", "''") + "', 0)";
                                        cmd.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                    }

                    if (bCancel)
                    {
                        // When we were cancelled don't leave any orphaned rows in the data...
                        cmd.CommandText = "DELETE from Dumps WHERE DumpPath = '" + d.DumpPath + "' AND DumpProcessID = '" + d.ProcessID + "'";
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "DELETE from StacksAndQueries WHERE DumpPath = '" + d.DumpPath + "' AND DumpProcessID = '" + d.ProcessID + "'";
                        cmd.ExecuteNonQuery();
                        // Also clear out these fields to be as consistent as possible although we shouldn't be using these ever if the Analyzed field remains false as it does.
                        d.DumpException = "";
                        d.ASVersion = "";
                        d.ProcessID = "";
                        d.Stacks = null;
                        d.Stacks = new List<Stack>();
                    }
                    else
                        d.Analyzed = true;

                    SubmitDebuggerCommand("q");
                }
                )).Start();
        }

        private string SubmitDebuggerCommand(string cmd)
        {
            LastResponse = "";
            if (p.StartInfo.RedirectStandardInput)
            {
                p.StandardInput.WriteLine(cmd);
                txtStatus.Invoke(new System.Action(() => txtStatus.Text += CurrentPrompt + " " + cmd + "\r\n"));
                if (cmd != "q")
                {
                    p.StandardInput.WriteLine(".echo \"EndOfData\"");
                    ResultReady.Reset();
                    ResultReady.WaitOne();
                }
            }
            return LastResponse;
        }

        private void P_Exited(object sender, EventArgs e)
        {

        }

        private void P_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {

        }

        string LastResponse = "";
        string CurrentPrompt = "";
        int OutputChunksSinceLastDump = 0;
        private void P_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (bCancel)
            {
                ResultReady.Set();
                return;
            }
            if (e.Data != null)
            {
                if (e.Data.EndsWith("EndOfData"))  // this signals the input prompt has been shown
                {
                    CurrentPrompt = e.Data.Replace("EndOfData", "");
                    LastResponse += CurrentPrompt.Substring(CurrentPrompt.IndexOf("> ") + "> ".Length);
                    CurrentPrompt = CurrentPrompt.Substring(0, CurrentPrompt.IndexOf("> ") + 1);
                    ResultReady.Set();
                }
                else
                {
                    // trim the current prompt from the start of data if both are not zero length strings
                    string output = e.Data.Length > 0 && CurrentPrompt.Length > 0 ? e.Data.Replace(CurrentPrompt, "") : e.Data;
                    // Skip adding output to text window for basic debugger header text...
                    if ((!LastResponse.StartsWith("\r\nMicrosoft (R) Windows Debugger Version") && !output.StartsWith("Microsoft (R) Windows Debugger Version")) || output.StartsWith("Loading Dump File "))
                    {
                        try
                        {
                            OutputChunksSinceLastDump++;
                            if (txtStatus.IsHandleCreated)
                                txtStatus.Invoke(new System.Action(() =>
                                {
                                    txtStatus.AppendText(output + "\r\n");
                                    if (OutputChunksSinceLastDump == 3)
                                    {
                                        txtStatus.SelectionStart = txtStatus.TextLength;
                                        txtStatus.ScrollToCaret();
                                        OutputChunksSinceLastDump = 0;
                                        
                                    }
                                }));
                        }
                        catch
                        {
                            // May fail if the thread was destroyed at close time, so no remediation needed, just ignore and let it move on then.
                        }
                    }
                    else
                    {
                        // When we are initializing the dump, we skip all the header stuff.  Zero it out once we get to our real first line then...
                        if (output.StartsWith("Loading Dump File "))
                            LastResponse = "";
                    }

                    LastResponse += output + "\r\n";
                }
            }
        }

        public event EventHandler Shown;
        bool wasShown = false;
        private void ucASDumpAnalyzer_Paint(object sender, PaintEventArgs e)
        {
            if (!wasShown)
            {
                wasShown = true;
                if (Shown != null)
                    Shown(this, EventArgs.Empty);
            }
        }

        private void ucASDumpAnalyzer_Load(object sender, EventArgs e)
        {
           
        }

        private void checkboxHeader_CheckedChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < dgdDumpList.RowCount; i++)
            {
                dgdDumpList[0, i].Value = ((CheckBox)dgdDumpList.Controls.Find("checkboxHeader", true)[0]).Checked;
            }
            dgdDumpList.EndEdit();
        }

        private void dgdDumpList_SelectionChanged(object sender, EventArgs e)
        {
            dgdDumpList.SuspendLayout();
            Dump dComp = null;
            int AnalyzedCount = 0;
            int CrashedCount = 0;
            int selCount = dgdDumpList.SelectedCells.Count;
            if (btnAnalyzeDumps.Text == "")
                splitDebugger.Panel2Collapsed = true;
            foreach (DataGridViewCell c in dgdDumpList.SelectedCells)
            {
                Dump d = c.OwningRow.DataBoundItem as Dump;                
                if (d.Analyzed)
                { 
                    AnalyzedCount++;
                
                    if (dComp == null)
                        dComp = d.Clone();
                    else
                        dComp.DumpPath = "\\<multiple>";
                    if (d.Crash)
                        CrashedCount++;
                    if (d.ASVersion != dComp.ASVersion)
                        dComp.ASVersion = "<multiple>";
                    if (d.DumpException != dComp.DumpException)
                        dComp.DumpException = "<multiple>";
                }
            }
            if (selCount > 0)
            {
                if (dComp == null)
                {
                    rtDumpDetails.Text = "Selection requires initial analysis.";
                }
                else
                {
                    string pluralize = (selCount > 1 ? "s: " : ": ");
                    rtDumpDetails.Text = "Dump file" + pluralize + dComp.DumpName + "\r\nAS Version" + pluralize + dComp.ASVersion + "\r\n" +
                        "Dump date: " + (selCount > 1 ? "<multiple>" : dComp.DumpTime.ToString("MM/dd/yyyy")) + "\r\n" +
                        "Dump time: " + (selCount > 1 ? "<multiple>" : dComp.DumpTime.ToString("HH:mm:ss UTCzzz")) + "\r\n" +
                        "Process id: " + (selCount > 1 ? "<multiple>" : dComp.ProcessID) + "\r\n" +
                        (AnalyzedCount < selCount ?
                            "Analysis found for " + AnalyzedCount + " of " + selCount + " dumps.\r\n" :
                            (selCount == 1 ? "" : "Analysis found for " + selCount + " dumps.\r\n")) +
                        (CrashedCount < selCount ?
                            (selCount == 1 ? "This is a hang dump." : CrashedCount + " dumps were crashes." + (AnalyzedCount - CrashedCount > 0 ? "\r\n" + (AnalyzedCount - CrashedCount) + " were hang dumps." : "")) :
                            (selCount == 1 ? "This is a crash dump." : CrashedCount + " dumps were crashes." + (AnalyzedCount - CrashedCount > 0 ? "\r\n" + (AnalyzedCount - CrashedCount) + " were hang dumps." : "")));
                    rtDumpDetails.Rtf = rtDumpDetails.Rtf.Replace("<", "\\i<").Replace(">", "\\i0>").Trim().TrimEnd('}') + 
                        (dComp.DumpException == "<multiple>" ?
                            "Dump Exceptions: <multiple>\\par}" :
                            ((CrashedCount == selCount) ? "Dump Exception" + pluralize + "\\par\\i\\b\\fs15" + dComp.DumpException : "\\b0\\i0\\par}"));
                }
            }
            if ((selCount & AnalyzedCount) == 1)
            {
                lblThreads.Text = dComp.Stacks.Count + " threads were found in the dump.";
                cmbThreads.DataSource = dComp.OrderedStacks;
                cmbThreads.DisplayMember = "FormattedThreadID";
                cmbThreads.Visible = true;
                cmbThreads_SelectedIndexChanged(null, null);
            }
            else
            {
                cmbThreads.Visible = false;
                lblThreads.Text = "";
                rtbStack.Text = "";
                splitDumpOutput.Panel2Collapsed = true;
            }
            if (btnAnalyzeDumps.Text == "Analyze Selection" || btnAnalyzeDumps.Text == "")
            {
                btnAnalyzeDumps.Enabled = (AnalyzedCount < selCount);
                btnAnalyzeDumps.BackColor = (AnalyzedCount < selCount) ? Color.DarkSeaGreen : SystemColors.Control;
                btnAnalyzeDumps.Text = (AnalyzedCount < selCount) ? "Analyze Selection" : "";
            }
            dgdDumpList.ResumeLayout();
        }


        FastColoredTextBox xmlQuery = new FastColoredTextBox();
        private void cmbThreads_SelectedIndexChanged(object sender, EventArgs e)
        {
            Stack s = (cmbThreads.SelectedItem as Stack);
            rtbStack.Text = s.CallStack;
            if (s.Query != "")
            {
                lblQuery.Text = "A query was found on the thread.";
                splitDumpOutput.Panel2Collapsed = false;
                if (s.Query.Trim().StartsWith("<") || s.Query.Trim().StartsWith("{") || s.Query == "There was a query on this thread but its memory could not be read (possibly not captured in this minidump).")
                {
                    xmlQuery.Text = s.Query;
                    mdxQuery.Visible = false;
                    xmlQuery.Visible = true;
                }
                else
                {
                    xmlQuery.Visible = false;
                    mdxQuery.Visible = true;
                    mdxQuery.SuspendLayout();
                    mdxQuery.Text = s.Query;
                    mdxQuery.ZoomFactor = 1;
                    mdxQuery.ZoomFactor = .75F;
                    mdxQuery.ResumeLayout();
                }
            }
            else
                splitDumpOutput.Panel2Collapsed = true;
        }
    }
}
