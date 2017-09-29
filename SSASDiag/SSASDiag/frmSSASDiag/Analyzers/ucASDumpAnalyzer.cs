using System;
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

namespace SSASDiag
{
    public partial class ucASDumpAnalyzer : UserControl
    {
        public string DumpPath { get; set; }
        ManualResetEvent ResultReady = new ManualResetEvent(false);
        SqlConnection connDB;
        List<string> DumpFiles = new List<string>();
        string AnalysisPath = "";
        string DumpAnalysisId;
        Process p = new Process();

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
                DumpFiles = Directory.GetFiles(DumpPath, "*.mdmp", SearchOption.AllDirectories).ToList();
                AnalysisPath = DumpPath + "\\Analysis";
            }
            else
            {
                AnalysisPath = DumpPath.Substring(0, DumpPath.LastIndexOf("\\") + 1) + "Analysis";
                DumpFiles.Add(DumpPath);
            }           
           
            if (!Directory.Exists(AnalysisPath))
                Directory.CreateDirectory(AnalysisPath);

            if (File.Exists(AnalysisPath + "DumpAnalysisId.txt"))
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
            new Thread(new ThreadStart(() =>
            {
                foreach (string f in DumpFiles)
                {
                    ConnectToDump(f);
                    while (!p.HasExited)
                        Thread.Sleep(500);
                    p = new Process();
                }
            })).Start();
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
            new Thread(new ThreadStart(()=>
                {
                    ResultReady.Reset();
                    ResultReady.WaitOne();
                    string init = LastResponse;
                    string debugTime = init.Split(new char[] { '\r', '\n' }).Where(f => f.StartsWith("Debug session time: ")).First().Replace("Debug session time: ", "").Trim();
                    string ver = SubmitDebuggerCommand("lmvm msmdsrv").Split(new char[] { '\r', '\n'}).Where(f=>f.Contains("Product version:")).First().Replace("    Product version:  ", "");
                    bool crash = init.Split(new char[] { '\r', '\n' }).Where(f => f.StartsWith("This dump file has an exception of interest stored in it.")).Count() > 0;
                    string pid = init.Substring(init.IndexOf("This dump file has an exception of interest stored in it."));
                    pid = pid.Substring(pid.IndexOf("\r\n") + 2);
                    pid = pid.Substring(pid.IndexOf("\r\n") + 2);
                    pid = pid.Substring(0, pid.IndexOf("\r\n"));
                    string exc = pid.Substring(pid.IndexOf("): ") + "): ".Length);
                    pid = pid.Substring(0, pid.IndexOf("): ")).Replace("(", "");
                    pid = pid.Substring(0, pid.IndexOf("."));
                    string[] timeparts = debugTime.Replace("   ", " ").Replace("  ", " ").Split(new char[] {' '});
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
            txtStatus.Invoke(new System.Action(()=> txtStatus.Text += CurrentPrompt + " " + cmd + "\r\n"));
            p.StandardInput.WriteLine(".echo");
            ResultReady.Reset();
            ResultReady.WaitOne();
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
                        txtStatus.Invoke(new System.Action(() => txtStatus.Text += LastResponse + "\r\n"));
                    }
                    else
                        CurrentPrompt = e.Data;
                    ResultReady.Set();
                }
                else
                {
                    // trim the current prompt from the start of data if both are not zero length strings
                    string output = e.Data.Length > 0 && CurrentPrompt.Length > 0 ? e.Data.Replace(CurrentPrompt, "") : e.Data;
                    txtStatus.Invoke(new System.Action(() => txtStatus.Text += output + "\r\n"));
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
    }
}
