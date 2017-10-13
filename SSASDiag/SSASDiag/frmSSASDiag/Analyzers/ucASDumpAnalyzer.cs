using System;
using System.Net;
using System.Runtime.InteropServices;
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

        private RichTextBoxEx rtbStack;

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

            public void IdentifyOwningThreadsAfterAllStacksLoaded()
            {
                List<Stack> PXSessionStacks = _stacks.Where(st => st.PXSessionAddyOnThisStack != null).ToList();
                foreach (Stack s in _stacks)
                {
                    if (s.PXSessionAddyReferencedFromRemoteOwningThread != null && s.PXSessionAddyReferencedFromRemoteOwningThread != "")
                    {
                        Stack owner = _stacks.Find(st => st.PXSessionAddyOnThisStack == s.PXSessionAddyReferencedFromRemoteOwningThread);
                        s.RemoteOwningThreadID = owner.ThreadID;
                        if (owner.OwnedThreads == null)
                            owner.OwnedThreads = new List<int>();
                        owner.OwnedThreads.Add(s.ThreadID);
                    }
                }
            }
        }

        private class Stack
        {
            public int ThreadID { get; set; }
            public string CallStack { get; set; }
            public string Query { get; set; }
            public bool ExceptionThread { get; set; }
            public string PXSessionAddyOnThisStack { get; set; }
            public string PXSessionAddyReferencedFromRemoteOwningThread { get; set; }
            public int? RemoteOwningThreadID { get; set; }
            public List<int> OwnedThreads { get; set; }
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

            // 
            // rtbStack
            // 
            rtbStack = new RichTextBoxEx();
            rtbStack.BackColor = System.Drawing.Color.Black;
            rtbStack.BorderStyle = System.Windows.Forms.BorderStyle.None;
            rtbStack.Dock = System.Windows.Forms.DockStyle.Fill;
            rtbStack.Font = new System.Drawing.Font("Consolas", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            rtbStack.ForeColor = System.Drawing.Color.LightSkyBlue;
            rtbStack.Location = new System.Drawing.Point(0, 21);
            rtbStack.Name = "rtbStack";
            rtbStack.ReadOnly = true;
            rtbStack.Size = new System.Drawing.Size(407, 97);
            rtbStack.TabIndex = 45;
            rtbStack.Text = "";
            rtbStack.LinkClicked += RtbStack_LinkClicked;
            splitDumpOutput.Panel1.Controls.Add(rtbStack);
            rtbStack.BringToFront();

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
                                + " ([DumpPath][nvarchar](max) NOT NULL, [DumpProcessID] [nvarchar](10) NOT NULL, [ThreadID] [int] NOT NULL, [Stack] [nvarchar] (max) NOT NULL, [Query] [nvarchar] (max) NULL, [ExceptionThread] [bit] NOT NULL, [OwningThread] [int] NULL, [OwnedThreads] [nvarchar] (max) NULL) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]";
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
                        List<int> ownedThreads = new List<int>();
                        if (dr["OwnedThreads"] as string != null && dr["OwnedThreads"] as string != "")
                            foreach (string s in (dr["OwnedThreads"] as string).Split(','))
                                ownedThreads.Add(Convert.ToInt32(s));
                        int? OwningThread = dr["OwningThread"] as int?;
                        d.Stacks.Add(new Stack()
                        {
                            CallStack = dr["Stack"] as string,
                            ThreadID = (int)dr["ThreadID"],
                            Query = dr["Query"] as string,
                            ExceptionThread = (bool)dr["ExceptionThread"],
                            OwnedThreads = ownedThreads,
                            RemoteOwningThreadID = OwningThread
                        });
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

            frmSSASDiag.LogFeatureUse("Dump Analysis", "Dump analysis initalized for " + DumpFiles.Count + " dumps, " + DumpFiles.Where(d => !d.Analyzed).Count() + " of which still require analysis.");
        }

        private void RtbStack_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            cmbThreads.SelectedIndex = cmbThreads.FindString("~" + e.LinkText);
            rtbStack.SelectionStart = 0;
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
            ValidateSymbolResolution();
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
                                    AnalyzeDump(d.DumpPath);
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
                                    p.Close();
                                    p = new Process();
                                }
                            }
                        }
                        if (!bCancel)
                        {
                            Invoke(new System.Action(() =>
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

        private void GetOwningPXSessionFromStack(Stack s)
        {
            List<string> Lines = new List<string>(s.CallStack.Split(new char[] { '\r', '\n' }));
            if (Lines.Where(c => c.Contains("PFThreadPool::ExecuteJob")).Count() > 0 &&
                Lines.Where(c => c.Contains("PXSession")).Count() == 0)
            {
                string res = SubmitDebuggerCommand("~" + s.ThreadID + "s");
                string PFThreadPool_ExecuteJobLine = Lines.Where(l => l.Contains("PFThreadPool::ExecuteJob")).First();
                res = SubmitDebuggerCommand(".frame " + PFThreadPool_ExecuteJobLine.Substring(0, PFThreadPool_ExecuteJobLine.IndexOf(" ")));
                res = SubmitDebuggerCommand("dt in_pThreadContext m_pParentEC->m_spSession->p");  // obtains the pointer to PXSession for the thread
                res = res.Substring(res.LastIndexOf(": ") + ": ".Length);
                string PXSessionAddy = res.Substring(0, res.LastIndexOf(" "));
                s.PXSessionAddyReferencedFromRemoteOwningThread = PXSessionAddy;
            }
        }

        private void GetQueryFromStack(Stack s)
        {
            string qry = "";
            List<string> Lines = new List<string>(s.CallStack.Split(new char[] { '\r', '\n' }));
            if (Lines.Where(c => c.Contains("PXSession")).Count() > 0)
            {
                string res = SubmitDebuggerCommand("~" + s.ThreadID + "s");
                string PXSessionLine = Lines.Where(l => l.Contains("PXSession")).Last();
                res = SubmitDebuggerCommand(".frame " + PXSessionLine.Substring(0, PXSessionLine.IndexOf(" ")));
                res = SubmitDebuggerCommand("dt this m_strLastRequest");
                string addy = res.Substring(res.IndexOf("PXSession*\r\n") + "PXSession*\r\n".Length);
                addy = addy.Substring(0, addy.IndexOf(" "));
                s.PXSessionAddyOnThisStack = addy;
                addy = res.Substring(res.IndexOf("Type PXSession*\r\n") + "Type PXSession*\r\n".Length);
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
            s.Query = qry;
        }

        public void AnalyzeDump(string path)
        {
            LastResponse = "";
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = connDB;
            p.OutputDataReceived += P_OutputDataReceived;
            p.ErrorDataReceived += P_ErrorDataReceived;
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

                    d.Stacks = new List<Stack>();
                    string stk = SubmitDebuggerCommand("kN").Trim();
                    int tid = Convert.ToInt32((CurrentPrompt.Replace(">", "").Replace(" ", "").Replace(":", "")));
                    Stack s = new Stack() { CallStack = stk, ThreadID = tid, ExceptionThread = true };
                    GetQueryFromStack(s);
                    GetOwningPXSessionFromStack(s);

                    if (bCancel)
                        return;
                    
                    d.Stacks.Add(s);
                    frmSSASDiag.LogFeatureUse("Dump Analysis", ("Analysis of dump " + path + " shows the following exception stack:\r\n" + stk.Replace("'", "''")));

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
                                    s = new Stack() { ExceptionThread = false };
                                    s.ThreadID = Convert.ToInt32(AllThreads[i - 1].Substring(0, AllThreads[i - 1].IndexOf(" Id: ")).TrimStart());
                                    int j = i;
                                    for (; AllThreads[j] != ""; j++)
                                        s.CallStack += AllThreads[j] + "\r\n";
                                    i = j;
                                    s.CallStack = s.CallStack.TrimEnd();
                                    GetQueryFromStack(s);
                                    if (d.Stacks.Find(st => st.ThreadID == s.ThreadID) == null && !bCancel)
                                        d.Stacks.Add(s);
                                }
                            }
                        }
                    }

                    d.IdentifyOwningThreadsAfterAllStacksLoaded();

                    // Wait to do db inserts until we really have all data.

                    cmd.CommandText = "INSERT INTO Dumps VALUES('" + d.DumpPath + "', '" + pid + "', '" + d.ASVersion + "', '" + dt.ToString() + "', " + (d.Crash ? 1 : 0) + ", '" + d.DumpException + "')";
                    cmd.ExecuteNonQuery();

                    foreach (Stack st in d.Stacks)
                    {
                        cmd.CommandText = "INSERT INTO StacksAndQueries VALUES('" + 
                                d.DumpPath + "', '" + 
                                pid + "', '" + 
                                st.ThreadID + "', '" + 
                                st.CallStack.Replace("'", "''") + "', " + 
                                (st.Query == null ? "NULL" : "'" + st.Query.Replace("'", "''") + "'") + ", " + 
                                (st.ExceptionThread == true ? 1 : 0) + ", " + 
                                (st.RemoteOwningThreadID == null ? "NULL" : st.RemoteOwningThreadID.ToString()) + ", " + 
                                (st.OwnedThreads == null ? "NULL" : "'" + String.Join(", ", st.OwnedThreads) + "'") + 
                            ")";
                        cmd.ExecuteNonQuery();
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

        private void P_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            LastResponse += "ERROR: " + e.Data;
            ResultReady.Set();
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
            rtbStack.Text = "";
            if (s.OwnedThreads != null && s.OwnedThreads.Count > 0)
            {
                if (s.OwnedThreads.Count == 1)
                    rtbStack.SelectedText = "Another thread was launched from this thread: ";
                else
                    rtbStack.SelectedText = "Other threads were launched from this thread: ";
                foreach (int tid in s.OwnedThreads)
                {
                    if (!rtbStack.Text.EndsWith(": "))
                        rtbStack.SelectedText = ", ";
                    rtbStack.InsertLink(tid.ToString());
                }
                rtbStack.SelectedText = "\r\n\r\n";
            }
            if (s.RemoteOwningThreadID != null)
            {
                rtbStack.SelectedText = "This thread was launched from the originating job thread ";
                rtbStack.InsertLink(s.RemoteOwningThreadID.ToString());
                rtbStack.SelectedText = ".\r\n\r\n";
            }
            rtbStack.SelectedText = s.CallStack;
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

    public class RichTextBoxEx : RichTextBox
    {
        #region Interop-Defines
        [StructLayout(LayoutKind.Sequential)]
        private struct CHARFORMAT2_STRUCT
        {
            public UInt32 cbSize;
            public UInt32 dwMask;
            public UInt32 dwEffects;
            public Int32 yHeight;
            public Int32 yOffset;
            public Int32 crTextColor;
            public byte bCharSet;
            public byte bPitchAndFamily;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public char[] szFaceName;
            public UInt16 wWeight;
            public UInt16 sSpacing;
            public int crBackColor; // Color.ToArgb() -> int
            public int lcid;
            public int dwReserved;
            public Int16 sStyle;
            public Int16 wKerning;
            public byte bUnderlineType;
            public byte bAnimation;
            public byte bRevAuthor;
            public byte bReserved1;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private const int WM_USER = 0x0400;
        private const int EM_GETCHARFORMAT = WM_USER + 58;
        private const int EM_SETCHARFORMAT = WM_USER + 68;

        private const int SCF_SELECTION = 0x0001;
        private const int SCF_WORD = 0x0002;
        private const int SCF_ALL = 0x0004;

        #region CHARFORMAT2 Flags
        private const UInt32 CFE_BOLD = 0x0001;
        private const UInt32 CFE_ITALIC = 0x0002;
        private const UInt32 CFE_UNDERLINE = 0x0004;
        private const UInt32 CFE_STRIKEOUT = 0x0008;
        private const UInt32 CFE_PROTECTED = 0x0010;
        private const UInt32 CFE_LINK = 0x0020;
        private const UInt32 CFE_AUTOCOLOR = 0x40000000;
        private const UInt32 CFE_SUBSCRIPT = 0x00010000;        /* Superscript and subscript are */
        private const UInt32 CFE_SUPERSCRIPT = 0x00020000;      /*  mutually exclusive			 */

        private const int CFM_SMALLCAPS = 0x0040;           /* (*)	*/
        private const int CFM_ALLCAPS = 0x0080;         /* Displayed by 3.0	*/
        private const int CFM_HIDDEN = 0x0100;          /* Hidden by 3.0 */
        private const int CFM_OUTLINE = 0x0200;         /* (*)	*/
        private const int CFM_SHADOW = 0x0400;          /* (*)	*/
        private const int CFM_EMBOSS = 0x0800;          /* (*)	*/
        private const int CFM_IMPRINT = 0x1000;         /* (*)	*/
        private const int CFM_DISABLED = 0x2000;
        private const int CFM_REVISED = 0x4000;

        private const int CFM_BACKCOLOR = 0x04000000;
        private const int CFM_LCID = 0x02000000;
        private const int CFM_UNDERLINETYPE = 0x00800000;       /* Many displayed by 3.0 */
        private const int CFM_WEIGHT = 0x00400000;
        private const int CFM_SPACING = 0x00200000;     /* Displayed by 3.0	*/
        private const int CFM_KERNING = 0x00100000;     /* (*)	*/
        private const int CFM_STYLE = 0x00080000;       /* (*)	*/
        private const int CFM_ANIMATION = 0x00040000;       /* (*)	*/
        private const int CFM_REVAUTHOR = 0x00008000;


        private const UInt32 CFM_BOLD = 0x00000001;
        private const UInt32 CFM_ITALIC = 0x00000002;
        private const UInt32 CFM_UNDERLINE = 0x00000004;
        private const UInt32 CFM_STRIKEOUT = 0x00000008;
        private const UInt32 CFM_PROTECTED = 0x00000010;
        private const UInt32 CFM_LINK = 0x00000020;
        private const UInt32 CFM_SIZE = 0x80000000;
        private const UInt32 CFM_COLOR = 0x40000000;
        private const UInt32 CFM_FACE = 0x20000000;
        private const UInt32 CFM_OFFSET = 0x10000000;
        private const UInt32 CFM_CHARSET = 0x08000000;
        private const UInt32 CFM_SUBSCRIPT = CFE_SUBSCRIPT | CFE_SUPERSCRIPT;
        private const UInt32 CFM_SUPERSCRIPT = CFM_SUBSCRIPT;

        private const byte CFU_UNDERLINENONE = 0x00000000;
        private const byte CFU_UNDERLINE = 0x00000001;
        private const byte CFU_UNDERLINEWORD = 0x00000002; /* (*) displayed as ordinary underline	*/
        private const byte CFU_UNDERLINEDOUBLE = 0x00000003; /* (*) displayed as ordinary underline	*/
        private const byte CFU_UNDERLINEDOTTED = 0x00000004;
        private const byte CFU_UNDERLINEDASH = 0x00000005;
        private const byte CFU_UNDERLINEDASHDOT = 0x00000006;
        private const byte CFU_UNDERLINEDASHDOTDOT = 0x00000007;
        private const byte CFU_UNDERLINEWAVE = 0x00000008;
        private const byte CFU_UNDERLINETHICK = 0x00000009;
        private const byte CFU_UNDERLINEHAIRLINE = 0x0000000A; /* (*) displayed as ordinary underline	*/

        #endregion

        #endregion

        public RichTextBoxEx()
        {
            // Otherwise, non-standard links get lost when user starts typing
            // next to a non-standard link
            this.DetectUrls = false;
        }

        [DefaultValue(false)]
        public new bool DetectUrls
        {
            get { return base.DetectUrls; }
            set { base.DetectUrls = value; }
        }

        /// <summary>
        /// Insert a given text as a link into the RichTextBox at the current insert position.
        /// </summary>
        /// <param name="text">Text to be inserted</param>
        public void InsertLink(string text)
        {
            InsertLink(text, this.SelectionStart);
        }

        /// <summary>
        /// Insert a given text at a given position as a link. 
        /// </summary>
        /// <param name="text">Text to be inserted</param>
        /// <param name="position">Insert position</param>
        public void InsertLink(string text, int position)
        {
            if (position < 0 || position > this.Text.Length)
                throw new ArgumentOutOfRangeException("position");

            this.SelectionStart = position;
            this.SelectedText = text;
            this.Select(position, text.Length);
            this.SetSelectionLink(true);
            this.Select(position + text.Length, 0);
        }

        /// <summary>
        /// Insert a given text at at the current input position as a link.
        /// The link text is followed by a hash (#) and the given hyperlink text, both of
        /// them invisible.
        /// When clicked on, the whole link text and hyperlink string are given in the
        /// LinkClickedEventArgs.
        /// </summary>
        /// <param name="text">Text to be inserted</param>
        /// <param name="hyperlink">Invisible hyperlink string to be inserted</param>
        public void InsertLink(string text, string hyperlink)
        {
            InsertLink(text, hyperlink, this.SelectionStart);
        }

        /// <summary>
        /// Insert a given text at a given position as a link. The link text is followed by
        /// a hash (#) and the given hyperlink text, both of them invisible.
        /// When clicked on, the whole link text and hyperlink string are given in the
        /// LinkClickedEventArgs.
        /// </summary>
        /// <param name="text">Text to be inserted</param>
        /// <param name="hyperlink">Invisible hyperlink string to be inserted</param>
        /// <param name="position">Insert position</param>
        public void InsertLink(string text, string hyperlink, int position)
        {
            if (position < 0 || position > this.Text.Length)
                throw new ArgumentOutOfRangeException("position");

            this.SelectionStart = position;
            this.SelectedRtf = @"{\rtf1\ansi " + text + @"\v #" + hyperlink + @"\v0}";
            this.Select(position, text.Length + hyperlink.Length + 1);
            this.SetSelectionLink(true);
            this.Select(position + text.Length + hyperlink.Length + 1, 0);
        }

        /// <summary>
        /// Set the current selection's link style
        /// </summary>
        /// <param name="link">true: set link style, false: clear link style</param>
        public void SetSelectionLink(bool link)
        {
            SetSelectionStyle(CFM_LINK, link ? CFE_LINK : 0);
        }
        /// <summary>
        /// Get the link style for the current selection
        /// </summary>
        /// <returns>0: link style not set, 1: link style set, -1: mixed</returns>
        public int GetSelectionLink()
        {
            return GetSelectionStyle(CFM_LINK, CFE_LINK);
        }


        private void SetSelectionStyle(UInt32 mask, UInt32 effect)
        {
            CHARFORMAT2_STRUCT cf = new CHARFORMAT2_STRUCT();
            cf.cbSize = (UInt32)Marshal.SizeOf(cf);
            cf.dwMask = mask;
            cf.dwEffects = effect;

            IntPtr wpar = new IntPtr(SCF_SELECTION);
            IntPtr lpar = Marshal.AllocCoTaskMem(Marshal.SizeOf(cf));
            Marshal.StructureToPtr(cf, lpar, false);

            IntPtr res = SendMessage(Handle, EM_SETCHARFORMAT, wpar, lpar);

            Marshal.FreeCoTaskMem(lpar);
        }

        private int GetSelectionStyle(UInt32 mask, UInt32 effect)
        {
            CHARFORMAT2_STRUCT cf = new CHARFORMAT2_STRUCT();
            cf.cbSize = (UInt32)Marshal.SizeOf(cf);
            cf.szFaceName = new char[32];

            IntPtr wpar = new IntPtr(SCF_SELECTION);
            IntPtr lpar = Marshal.AllocCoTaskMem(Marshal.SizeOf(cf));
            Marshal.StructureToPtr(cf, lpar, false);

            IntPtr res = SendMessage(Handle, EM_GETCHARFORMAT, wpar, lpar);

            cf = (CHARFORMAT2_STRUCT)Marshal.PtrToStructure(lpar, typeof(CHARFORMAT2_STRUCT));

            int state;
            // dwMask holds the information which properties are consistent throughout the selection:
            if ((cf.dwMask & mask) == mask)
            {
                if ((cf.dwEffects & effect) == effect)
                    state = 1;
                else
                    state = 0;
            }
            else
            {
                state = -1;
            }

            Marshal.FreeCoTaskMem(lpar);
            return state;
        }
    }

}
