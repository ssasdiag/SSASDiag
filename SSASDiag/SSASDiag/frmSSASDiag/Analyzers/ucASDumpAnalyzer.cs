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
            public List<Stack> Stacks { get; set; }
        }

        private class Stack
        {
            public int ThreadID { get; set; }
            public string CallStack { get; set; }
            public string Query { get; set; }
            public bool ExceptionThread { get; set; }
        }


        public ucASDumpAnalyzer(string dumpPath, SqlConnection conndb)
        {
            InitializeComponent();
            DumpPath = dumpPath;
            connDB = new SqlConnection(conndb.ConnectionString);
            connDB.Open();
            Shown += UcASDumpAnalyzer_Shown;
            HandleDestroyed += UcASDumpAnalyzer_HandleDestroyed;


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

            if (File.Exists(AnalysisPath + "\\DumpAnalysisId.txt"))
                DumpAnalysisId = File.ReadAllText(AnalysisPath + "\\DumpAnalysisId.txt");
            else
            {
                DumpAnalysisId = Guid.NewGuid().ToString();
                File.WriteAllText(AnalysisPath + "\\DumpAnalysisId.txt", DumpAnalysisId);
            }

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
                    d.Stacks.Add(new Stack() { CallStack = dr["Stack"] as string, ThreadID = (int)dr["ThreadID"], Query = dr["Query"] as string, ExceptionThread = (bool)dr["ExceptionThread"] });
            }
            dr.Close();
            dgdDumpList.DataSource = DumpFiles;
            dgdDumpList.DataBindingComplete += DgdDumpList_DataBindingComplete;
        }

        int DataBindingCompletions = 0;
        private void DgdDumpList_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            DataBindingCompletions++;
            if (DataBindingCompletions == 4)
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
            }
        }

        private void UcASDumpAnalyzer_HandleDestroyed(object sender, EventArgs e)
        {
            connDB.ChangeDatabase("master");
            SqlCommand cmd = new SqlCommand("IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'" + DBName() + "') ALTER DATABASE [" + DBName() + "] SET SINGLE_USER WITH ROLLBACK IMMEDIATE", connDB);
            cmd.ExecuteNonQuery();
            cmd = new SqlCommand("IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'" + DBName() + "') EXEC master.dbo.sp_detach_db @dbname = N'" + DBName() + "'", connDB);
            cmd.ExecuteNonQuery();
            connDB.Close();
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

        public void ConnectToDump(string path)
        {
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
            p.StandardInput.WriteLine(".echo"); // Ensures we can detect end of output, when this is processed and input prompt is displayed in console output...
            new Thread(new ThreadStart(() =>
                {
                    ResultReady.Reset();
                    ResultReady.WaitOne();
                    string init = LastResponse;
                    string debugTime = init.Split(new char[] { '\r', '\n' }).Where(f => f.StartsWith("Debug session time: ")).First().Replace("Debug session time: ", "").Trim();
                    string ver = SubmitDebuggerCommand("lmvm msmdsrv").Split(new char[] { '\r', '\n' }).Where(f => f.Contains("Product version:")).First().Replace("    Product version:  ", "");
                    bool crash = init.Split(new char[] { '\r', '\n' }).Where(f => f.StartsWith("This dump file has an exception of interest stored in it.")).Count() > 0;
                    string pid = init.Substring(init.IndexOf("This dump file has an exception of interest stored in it."));
                    pid = pid.Substring(pid.IndexOf("\r\n") + 2);
                    pid = pid.Substring(pid.IndexOf("\r\n") + 2);
                    pid = pid.Substring(0, pid.IndexOf("\r\n"));
                    string exc = pid.Substring(pid.IndexOf("): ") + "): ".Length);
                    pid = pid.Substring(0, pid.IndexOf("): ")).Replace("(", "");
                    pid = pid.Substring(0, pid.IndexOf("."));
                    string[] timeparts = debugTime.Replace("   ", " ").Replace("  ", " ").Split(new char[] { ' ' });
                    string properTime = timeparts[1] + " " + timeparts[2] + ", " + timeparts[4] + " " + timeparts[3] + " " + timeparts[6] + timeparts[7].Replace(")", "");
                    DateTime dt;
                    DateTime.TryParseExact(properTime, "MMM d, yyyy HH:mm:ss.fff zzz", null, System.Globalization.DateTimeStyles.AssumeLocal, out dt);
                    cmd.CommandText = "INSERT INTO Dumps VALUES('" + path + "', '" + pid + "', '" + ver + "', '" + dt.ToString() + "', " + (crash ? 1 : 0) + ", '" + exc + "')";
                    cmd.ExecuteNonQuery();

                    string stk = SubmitDebuggerCommand("kN");
                    string qry = "";

                    List<string> Lines = new List<string>(stk.Split(new char[] { '\r', '\n' }));
                    if (Lines.Where(c => c.Contains("PXSession")).Count() > 0)
                    {
                        string PXSessionLine = Lines.Where(l => l.Contains("PXSession")).First();
                        string res = SubmitDebuggerCommand(".frame " + PXSessionLine.Substring(0, PXSessionLine.IndexOf(" ")));
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
                        }
                        catch (Exception) { /* This could fail if memory can't be read so just move on. */ }
                    }
                    SubmitDebuggerCommand("q");
                    cmd.CommandText = "INSERT INTO StacksAndQueries VALUES('" + path + "', '" + pid + "'," + CurrentPrompt.Replace(">", "").Replace(" ", "").Replace(":", "") + ", '" + stk.Replace("'", "''") + "', '" + qry.Replace("'", "''") + "', 1)";
                    cmd.ExecuteNonQuery();
                    //res = SubmitDebuggerCommand("~*kN");


                }
                )).Start();
        }

        private string SubmitDebuggerCommand(string cmd)
        {
            LastResponse = "";
            p.StandardInput.WriteLine(cmd);
            txtStatus.Invoke(new System.Action(() => txtStatus.Text += CurrentPrompt + " " + cmd + "\r\n"));
            if (cmd != "q")
            {
                p.StandardInput.WriteLine(".echo");
                ResultReady.Reset();
                ResultReady.WaitOne();
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
        private void P_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                if (e.Data.EndsWith("> "))  // this signals the input prompt has been shown
                {
                    if (e.Data.Length > 10)
                    {
                        LastResponse = e.Data.Replace(CurrentPrompt, "");
                        txtStatus.Invoke(new System.Action(() => txtStatus.AppendText(LastResponse + "\r\n")));
                    }
                    else
                        CurrentPrompt = e.Data;
                    ResultReady.Set();
                }
                else
                {
                    // trim the current prompt from the start of data if both are not zero length strings
                    string output = e.Data.Length > 0 && CurrentPrompt.Length > 0 ? e.Data.Replace(CurrentPrompt, "") : e.Data;
                    txtStatus.Invoke(new System.Action(() => txtStatus.AppendText(output + "\r\n")));
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

        private void btnAnalyzeDumps_Click(object sender, EventArgs e)
        {
            new Thread(new ThreadStart(() =>
            {
                foreach (DataGridViewRow r in dgdDumpList.Rows)
                {
                    if (r.Cells[1].Selected)
                    {
                        ConnectToDump(r.Cells[0].Value as string);
                        while (!p.HasExited)
                            Thread.Sleep(500);
                        p.Close();
                        p.ErrorDataReceived -= P_ErrorDataReceived;
                        p.OutputDataReceived -= P_OutputDataReceived;
                        p.Exited -= P_Exited;
                        p.Dispose();
                        p = new Process();
                    }
                }
            })).Start();
        }

        private void dgdDumpList_SelectionChanged(object sender, EventArgs e)
        {
            dgdDumpList.SuspendLayout();
            Dump dComp = null;
            int AnalyzedCount = 0;
            int CrashedCount = 0;
            int selCount = dgdDumpList.SelectedCells.Count;
            foreach (DataGridViewCell c in dgdDumpList.SelectedCells)
            {
                Dump d = c.OwningRow.DataBoundItem as Dump;                
                if (d.Analyzed)
                { 
                    AnalyzedCount++;
                
                    if (dComp == null)
                        dComp = d.Clone();
                    else
                        dComp.DumpPath = "\\Multiple dumps selected";
                    if (d.Crash)
                        CrashedCount++;
                    if (d.ASVersion != dComp.ASVersion)
                        dComp.ASVersion = "<multiple versions>";
                    if (d.DumpException != dComp.DumpException)
                        dComp.DumpException = "<multiple exceptions>";
                }
            }
            if (selCount > 0)
            {
                if (dComp == null)
                {
                    rtDumpDetails.Text = "The selected dump has not been analyzed yet.";
                }
                else
                {
                    if (AnalyzedCount < selCount)
                        btnAnalyzeDumps.Enabled = true;
                    else
                        btnAnalyzeDumps.Enabled = false;
                    string pluralize = (selCount > 1 ? "s: " : ": ");
                    rtDumpDetails.Text = "Dump file" + pluralize + dComp.DumpName + "\r\nAS Version" + pluralize + dComp.ASVersion + "\r\n" +
                        "Dump time: " + (selCount > 1 ? "<multiple dumps selected>" : dComp.DumpTime.ToString("MM/dd/yyyy HH:mm:ss UTCzzz")) + "\r\n" +
                        (AnalyzedCount < selCount ?
                            "Analysis exists for " + AnalyzedCount + " of " + selCount + " selected dumps.\r\n" :
                            (selCount == 1 ? "" : "Analysis exists for all of the " + selCount + " selected dumps.\r\n")) +
                        (CrashedCount < selCount ?
                            (selCount == 1 ? "This is a hang dump." : CrashedCount + " selected dumps were crash dumps and " + (AnalyzedCount - CrashedCount) + " were hang dumps.") :
                            (selCount == 1 ? "This is a crash dump." : CrashedCount + " selected dumps were crash dumps and " + (AnalyzedCount - CrashedCount) + " were hang dumps.")) +
                        "\r\n" +
                        ((CrashedCount == selCount) ? "Dump Exception" + pluralize + "\r\n" + dComp.DumpException : "");
                    rtDumpDetails.Rtf = rtDumpDetails.Rtf.Replace("<", "\\i<").Replace(">", "\\i0>");
                }
            }
            if ((selCount & AnalyzedCount) == 1)
            {
                lblThreads.Text = dComp.Stacks.Count + " threads were found in the dump.  Select to view:";
                cmbThreads.DataSource = dComp.Stacks;
                cmbThreads.DisplayMember = "ThreadID";
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
            btnAnalyzeDumps.Enabled = (AnalyzedCount < selCount);
            btnAnalyzeDumps.BackColor = (AnalyzedCount < selCount) ? Color.DarkSeaGreen : SystemColors.ControlLight;
            btnAnalyzeDumps.Text = (AnalyzedCount < selCount) ? "Analyze Selection" : "";
            spDumpDetails.Panel2Collapsed = selCount == 0;
            dgdDumpList.ResumeLayout();
        }

        private void cmbThreads_SelectedIndexChanged(object sender, EventArgs e)
        {
            Stack s = (cmbThreads.SelectedItem as Stack);
            rtbStack.Text = s.CallStack;
            if (s.Query != "")
            {
                splitDumpOutput.SuspendLayout();
                splitDumpOutput.Panel2Collapsed = false;
                mdxQuery.SuspendLayout();
                mdxQuery.Text = s.Query;
                mdxQuery.ZoomFactor = .75F;
                mdxQuery.ResumeLayout();
                splitDumpOutput.ResumeLayout();
            }
            else
                splitDumpOutput.Panel2Collapsed = true;
        }

        private void ucASDumpAnalyzer_SizeChanged(object sender, EventArgs e)
        {
            splitDumpOutput.Height = splitDumpList.Panel2.Height - pnStacks.Height;
        }
    }
}
