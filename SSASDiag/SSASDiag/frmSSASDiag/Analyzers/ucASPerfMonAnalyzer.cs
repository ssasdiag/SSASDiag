using Microsoft.Win32;
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
using System.Windows.Forms.DataVisualization.Charting;
using Ionic.Zip;

namespace SSASDiag
{
    public partial class ucASPerfMonAnalyzer : UserControl
    {
        frmStatusFloater StatusFloater = null;
        string LogPath, AnalysisPath, PerfMonAnalysisId;
        SqlConnection connDB;
        List<PerfMonLog> LogFiles = new List<PerfMonLog>();

        private class PerfMonLog
        {
            public PerfMonLog Clone()
            {
                return (PerfMonLog)MemberwiseClone();
            }

            public string LogPath { get; set; }
            public string LogName
            {
                get { return LogPath != null ? LogPath.Substring(LogPath.LastIndexOf("\\") + 1) : ""; }
            }
            public bool Analyzed { get; set; }
        }

        TimeRangeBar trTimeRange = new TimeRangeBar();
        private TriStateTreeView tvCounters;
        private StripLine stripLine = new StripLine();

        public ucASPerfMonAnalyzer(string logPath, SqlConnection conndb, frmStatusFloater statusFloater)
        {
            InitializeComponent();


            #region non-designer controls

            stripLine.Interval = 0;
            stripLine.StripWidth = 0;
            stripLine.IntervalOffset = 0;
            // pick you color etc ... before adding the stripline to the axis
            stripLine.BackColor = Color.FromArgb(128, Color.DarkGray);
            chartPerfMon.ChartAreas[0].AxisX.StripLines.Add(stripLine);

            trTimeRange.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            trTimeRange.BackColor = System.Drawing.SystemColors.Control;
            trTimeRange.DivisionNum = 15;
            trTimeRange.HeightOfBar = 6;
            trTimeRange.HeightOfMark = 16;
            trTimeRange.HeightOfTick = 4;
            trTimeRange.Height = 60;
            trTimeRange.Margin = new Padding(3, 0, 3, 0);
            trTimeRange.InnerColor = System.Drawing.Color.LightCyan;
            trTimeRange.Orientation = TimeRangeBar.RangeBarOrientation.horizontal;
            trTimeRange.ScaleOrientation = TimeRangeBar.TopBottomOrientation.bottom;
            trTimeRange.Dock = DockStyle.Top;
            trTimeRange.RangeChanged += TrTimeRange_RangeChanged;
            trTimeRange.RangeSliderMoving += TrTimeRange_RangeSliderMoving;
            this.pnlSeriesDetails.Controls.Add(trTimeRange);

            // 
            // tvCounters
            // 
            tvCounters = new TriStateTreeView();
            tvCounters.CheckBoxes = true;
            tvCounters.Dock = System.Windows.Forms.DockStyle.Bottom;
            tvCounters.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            tvCounters.Location = new System.Drawing.Point(0, 130);
            tvCounters.Name = "tvCounters";
            tvCounters.Size = new System.Drawing.Size(200, 238);
            tvCounters.TabIndex = 0;
            tvCounters.Margin = new Padding(0);
            tvCounters.AfterCheck += TvCounters_AfterCheck;
            tvCounters.AfterSelect += TvCounters_AfterSelect;
            splitPerfMonCountersAndChart.Panel1.Controls.Add(tvCounters);
            tvCounters.TriStateStyleProperty = TriStateTreeView.TriStateStyles.Installer;

            splitHideFilesButton.SplitterDistance = 15;
            dgdGrouping.ColumnDisplayIndexChanged += DgdGrouping_ColumnDisplayIndexChanged;
            cmbServers.SelectedIndexChanged += cmbServers_SelectedIndexChanged;

            #endregion non-designer controls

            StatusFloater = statusFloater;
            LogPath = logPath;
            connDB = new SqlConnection(conndb.ConnectionString);
            connDB.Open();
            HandleDestroyed += UcASPerfMonAnalyzer_HandleDestroyed;
            
            ChartArea c = chartPerfMon.ChartAreas[0];
            c.IsSameFontSizeForAllAxes = true;
            c.AxisX.LabelAutoFitMaxFontSize = c.AxisY.LabelAutoFitMaxFontSize = c.AxisX2.LabelAutoFitMaxFontSize = c.AxisY2.LabelAutoFitMaxFontSize = 7;
            this.chartPerfMon.MouseClick += ChartPerfMon_MouseClick;
            this.chartPerfMon.MouseMove += ChartPerfMon_MouseMove;
            this.dgdGrouping.Columns[0].SortMode = this.dgdGrouping.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;
            
            splitAnalysis.SplitterMoved += SplitAnalysis_SplitterMoved;

            if (Directory.Exists(logPath))
            {
                List<string> logfiles = new List<string>();
                logfiles.AddRange(Directory.GetFiles(logPath, "*.blg", SearchOption.TopDirectoryOnly));
                foreach (string dir in Directory.EnumerateDirectories(logPath))
                    if (!dir.Contains("\\$RECYCLE.BIN") && !dir.Contains("\\System Volume Information"))
                    {
                        try { logfiles.AddRange(Directory.GetFiles(dir, "*.blg", SearchOption.AllDirectories)); }
                        catch (Exception ex) { Trace.WriteLine(Program.CurrentFormattedLocalDateTime() + ": Exception enumerating logs from subdirectories: " + ex.Message); }
                    }
                foreach (string f in logfiles)
                    LogFiles.Add(new PerfMonLog() { LogPath = f, Analyzed = false });
                AnalysisPath = LogPath + "\\Analysis";
            }
            else 
            {
                AnalysisPath = LogPath.Substring(0, LogPath.LastIndexOf("\\") + 1) + "Analysis";
                LogFiles.Add(new PerfMonLog() { LogPath = logPath, Analyzed = false });
            }

            if (!Directory.Exists(AnalysisPath))
                Directory.CreateDirectory(AnalysisPath);

            string[] PerfMonAnalyses = Directory.GetFiles(AnalysisPath, "SSASDiag_PerfMon_Analysis_*.mdf");
            if (PerfMonAnalyses.Count() > 0)
                PerfMonAnalysisId = PerfMonAnalyses[0].Replace(AnalysisPath, "").Replace("SSASDiag_PerfMon_Analysis_", "").Replace(".mdf", "").Replace("\\", "");
            else
                PerfMonAnalysisId = Guid.NewGuid().ToString();

            string sSvcUser = "";
            ServiceController[] services = ServiceController.GetServices();
            foreach (ServiceController s in services.OrderBy(ob => ob.DisplayName))
                if (s.DisplayName.Contains("SQL Server (" + (connDB.DataSource.Contains("\\") ? connDB.DataSource.Substring(connDB.DataSource.IndexOf("\\") + 1) : "MSSQLSERVER")))
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
                cmd = new SqlCommand("IF NOT EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'" + DBName() + "') CREATE DATABASE [" + DBName() + "] ON (FILENAME = N'" + MDFPath().Replace("'", "''") + "'),"
                                            + "(FILENAME = N'" + LDFPath().Replace("'", "''") + "') "
                                            + "FOR ATTACH", connDB);
            }
            else
            {
                cmd = new SqlCommand(Properties.Resources.CreateDBSQLScript.
                                    Replace("<mdfpath/>", MDFPath().Replace("'", "''")).
                                    Replace("<ldfpath/>", LDFPath().Replace("'", "''")).
                                    Replace("<dbname/>", DBName())
                                    , connDB);
            }
            int ret = cmd.ExecuteNonQuery();
            cmd.CommandText = "USE [" + DBName() + "]";
            cmd.ExecuteNonQuery();
            cmd.CommandText = "if not exists(select* from sysobjects where name= 'PerfMonLogs' and xtype = 'U') CREATE TABLE[dbo].[PerfMonLogs]"
                                + " ([LogPath] [nvarchar] (max) NOT NULL) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]";
            cmd.ExecuteNonQuery();

            
            cmd.CommandText = "select * from PerfMonLogs";
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                PerfMonLog l = LogFiles.Find(df => df.LogPath == dr["LogPath"] as string);
                if (l != null)
                {
                    l.Analyzed = true;
                }
                else
                {
                    l = new PerfMonLog();
                    LogFiles.Add(l);
                }
            }
            dr.Close();

            try
            {
                dr = new SqlCommand("select distinct MachineName from CounterDetails order by MachineName", connDB).ExecuteReader();
                cmbServers.Items.Clear();
                while (dr.Read())
                    cmbServers.Items.Add(dr["MachineName"]);
                dr.Close();
                if (cmbServers.Items.Count > 0)
                {
                    cmbServers.SelectedIndex = 0;
                    splitAnalysis.Visible = true;
                    if (!splitLogList.Panel2Collapsed)
                        tableLayoutPanel1_Click(null, null);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("CounterDetails"))
                    splitAnalysis.Visible = false;
            }

            dgdLogList.DataSource = LogFiles;
            dgdLogList.DataBindingComplete += DgdLogList_DataBindingComplete;

            frmSSASDiag.LogFeatureUse("PerfMon Analysis", "PerfMon analysis initalized for " + LogFiles.Count + " logs, " + LogFiles.Where(d => !d.Analyzed).Count() + " of which still require import for analysis.");
        }

        private void TrTimeRange_RangeSliderMoving(bool bLeftButton, DateTime curPos)
        {           
            if (bLeftButton)
            {
                stripLine.StripWidth = chartPerfMon.ChartAreas[0].AxisX.Maximum - curPos.ToOADate();
                stripLine.IntervalOffset = curPos.ToOADate();
            }
            else
            {
                stripLine.StripWidth = curPos.ToOADate();
                stripLine.IntervalOffset = 0;
            }
        }

        private void TrTimeRange_RangeChanged(object sender, EventArgs e)
        {
            stripLine.StripWidth = 0;
            chartPerfMon.Series.Clear();
            foreach (System.Windows.Forms.TreeNode n in tvCounters.GetCheckedLeafNodes())
                TvCounters_AfterCheck(sender, new TreeViewEventArgs(n));
        }

        int DataBindingCompletions = 0;
        private void DgdLogList_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            DataBindingCompletions++;
            if (DataBindingCompletions > 3)  // Skip the first three binding messages when we are initializing...
            {
                dgdLogList.Columns[0].Visible = false;
                dgdLogList.Columns[2].Visible = false;

                foreach (DataGridViewRow r in dgdLogList.Rows)
                {
                    try
                    {
                        PerfMonLog l = r.DataBoundItem as PerfMonLog;
                        if (l.Analyzed == false)
                        {
                            r.DefaultCellStyle.ForeColor = SystemColors.GrayText;
                            r.Cells[1].ToolTipText = "This log has not been imported yet.  Select, then click Import Selection.";
                        }
                        else
                            r.DefaultCellStyle.BackColor = SystemColors.ControlDark;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
                dgdLogList.ClearSelection();
                dgdLogList.SelectionChanged += dgdLogList_SelectionChanged;
                rtLogDetails.Text = "Performance logs found: " + LogFiles.Count + "\r\nLog(s) already imported: " + LogFiles.Where(l => l.Analyzed).Count();
            }
        }

        private void UcASPerfMonAnalyzer_HandleDestroyed(object sender, EventArgs e)
        {
            try
            {
                frmSSASDiag.LogFeatureUse("PerfMon Analysis", "Detatching from perfmon analysis database on exit.");
                connDB.ChangeDatabase("master");
                SqlCommand cmd = new SqlCommand("IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'" + DBName() + "') ALTER DATABASE [" + DBName() + "] SET SINGLE_USER WITH ROLLBACK IMMEDIATE", connDB);
                cmd.ExecuteNonQuery();
                cmd = new SqlCommand("IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'" + DBName() + "') EXEC master.dbo.sp_detach_db @dbname = N'" + DBName() + "'", connDB);
                cmd.ExecuteNonQuery();
                connDB.Close();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(Program.CurrentFormattedLocalDateTime() + ": Exception detaching PerfMon analysis database on exit: " + ex.Message);
                // Closing connection could fail if the database is otherwise in use or something.  Just ignore - we're closing, don't notify user...
            }
            try
            {
                string AnalysisZipFile = Directory.GetParent(Directory.GetParent(AnalysisPath).FullName).FullName + "\\" + Directory.GetParent(AnalysisPath).Name + ".zip";
                if (File.Exists(AnalysisZipFile))
                {
                    ZipFile z = new ZipFile(AnalysisZipFile);
                    z.UseZip64WhenSaving = Ionic.Zip.Zip64Option.Always;
                    z.ParallelDeflateThreshold = -1;
                    z.AddFiles(new string[] { MDFPath(), LDFPath() }, Directory.GetParent(AnalysisPath).Name + "/Analysis");
                    z.Save();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(Program.CurrentFormattedLocalDateTime() + ": Exception adding PerfMon analysis to zip folder: " + ex.Message);
            }
        }
        private string DBName()
        {
            return "SSASDiag_PerfMon_Analysis_" + PerfMonAnalysisId;
        }
        private string MDFPath()
        {
            return AnalysisPath + "\\" + DBName() + ".mdf";
        }
        private string LDFPath()
        {
            return AnalysisPath + "\\" + DBName() + ".ldf";
        }
        public bool bCancel = false;
        int LogCountAnalyzedInCurrentRun = 0;
        
        private void btnAnalyzeLogs_Click(object sender, EventArgs e)
        {
            if (btnAnalyzeLogs.Text == "Import Selection")
            {
                LogCountAnalyzedInCurrentRun = 0;
                bCancel = false;
                btnAnalyzeLogs.BackColor = Color.Pink;
                btnAnalyzeLogs.FlatAppearance.MouseDownBackColor = Color.IndianRed;
                btnAnalyzeLogs.FlatAppearance.MouseOverBackColor = Color.LightCoral;
                btnAnalyzeLogs.Text = "Cancel Import In-Progress...";

                new Thread(new ThreadStart(() =>
                {
                    int TotalSelectedLogsCount = 0;
                    foreach (DataGridViewRow r in dgdLogList.Rows)
                        if (!(r.DataBoundItem as PerfMonLog).Analyzed && r.Cells[1].Selected) TotalSelectedLogsCount++;
                    if (TotalSelectedLogsCount > 0)
                    {
                        rtLogDetails.Invoke(new System.Action(() => rtLogDetails.Text = "Importing " + TotalSelectedLogsCount + " log" + (TotalSelectedLogsCount == 1 ? "" : "s") + "."));
                        List<DataGridViewRow> LogsToProcess = new List<DataGridViewRow>();
                        foreach (DataGridViewRow r in dgdLogList.Rows)
                            if (r.Cells[1].Selected)
                                LogsToProcess.Add(r);
                        int LogsRequiringAnalysis = LogsToProcess.Where(drow => !(drow.DataBoundItem as PerfMonLog).Analyzed).Count();
                        frmSSASDiag.LogFeatureUse("PerfMon Analysis", "Importing " + LogsRequiringAnalysis + " log" + (LogsRequiringAnalysis > 1 ? "s." : "."));

                        // Create SQL DSN for relog
                        Registry.LocalMachine.CreateSubKey("SOFTWARE\\ODBC\\ODBC.INI\\ODBC Data Sources").SetValue("SSASDiagPerfMonDSN", "SQL Server");
                        RegistryKey dsnKey = Registry.LocalMachine.CreateSubKey("SOFTWARE\\ODBC\\ODBC.INI\\SSASDiagPerfMonDSN");
                        dsnKey.SetValue("Database", "SSASDiagPerfMonDSN");
                        dsnKey.SetValue("Description", "SSASDiag PerfMon Relog DSN");
                        dsnKey.SetValue("Driver", Environment.GetFolderPath(Environment.SpecialFolder.System) + "\\sqlsrv32.dll");
                        dsnKey.SetValue("Server", connDB.ConnectionString.Split(';').ToList().Where(s=>s.ToLower().StartsWith("server") || s.ToLower().StartsWith("data source")).First().Split('=')[1]);
                        dsnKey.SetValue("Database", DBName());
                        dsnKey.SetValue("Trusted_Connection", "Yes");

                        List<Thread> importthreads = new List<Thread>();

                        rtLogDetails.Invoke(new System.Action(() => rtLogDetails.Text = "Importing " + TotalSelectedLogsCount + " logs in parallel. "));

                        DateTime lastUIUpdate = DateTime.Now;
                        bool bUpdatedUI = false;
                        
                        foreach (DataGridViewRow r in LogsToProcess)
                        {
                            if (!bCancel)
                            {
                                PerfMonLog l = r.DataBoundItem as PerfMonLog;
                                DataGridViewCell c = r.Cells[1];
                                if (!l.Analyzed)
                                {
                                    importthreads.Add(new Thread(new ThreadStart(() =>
                                    {
                                        Process p = new Process();
                                        p.StartInfo.UseShellExecute = false;
                                        p.StartInfo.CreateNoWindow = true;
                                        p.StartInfo.FileName = "relog.exe";
                                        p.StartInfo.Arguments = "\"" + l.LogPath + "\" -f SQL -o SQL:SSASDiagPerfMonDSN!logfile -y";
                                        p.Start();

                                        while (!bCancel && !p.HasExited)
                                        {
                                            Thread.Sleep(500);
                                            if (DateTime.Now - lastUIUpdate > TimeSpan.FromSeconds(1.5))
                                            {
                                                if (!bUpdatedUI)
                                                {
                                                    bUpdatedUI = true;
                                                    lastUIUpdate = DateTime.Now;
                                                    rtLogDetails.Invoke(new System.Action(() => { rtLogDetails.AppendText("."); }));
                                                }
                                            }
                                        }
                                        // Clean up the old process and reinitialize.
                                        if (bCancel)
                                        {
                                            if (!p.HasExited)
                                                p.Kill();
                                        }
                                        else
                                        {
                                            SqlConnection connLocal = new SqlConnection(connDB.ConnectionString);
                                            connLocal.Open();
                                            new SqlCommand("USE[" + DBName() + "]", connLocal).ExecuteNonQuery();
                                            new SqlCommand("insert into PerfMonLogs values ('" + l.LogPath + "')", connLocal).ExecuteNonQuery();
                                            l.Analyzed = true;
                                            dgdLogList.Invoke(new System.Action(() =>
                                            {
                                                c.Style.ForeColor = SystemColors.ControlText;
                                                c.ToolTipText = "";
                                            }));
                                            connLocal.Close();
                                            LogCountAnalyzedInCurrentRun++;
                                        }
                                        p.Close();
                                    })));
                                    importthreads.Last().Start();
                                }
                            }
                        }

                        while (importthreads.Any(t => t.ThreadState != System.Threading.ThreadState.Stopped))
                        {
                            Thread.Sleep(1500);
                            rtLogDetails.Invoke(new System.Action(() => rtLogDetails.AppendText(".")));
                        }

                        Registry.LocalMachine.CreateSubKey("SOFTWARE\\ODBC\\ODBC.INI").DeleteSubKey("SSASDiagPerfMonDSN");
                        Registry.LocalMachine.CreateSubKey("SOFTWARE\\ODBC\\ODBC.INI\\ODBC Data Sources").DeleteValue("SSASDiagPerfMonDSN");

                        if (!bCancel)
                        {
                            Invoke(new System.Action(() =>
                            {
                                SuspendLayout();
                                rtLogDetails.Text = "Imported " + TotalSelectedLogsCount + " log file" + (TotalSelectedLogsCount != 1 ? "s." : ".");
                                btnAnalyzeLogs.Text = "";
                                btnAnalyzeLogs.BackColor = SystemColors.Control;
                                btnAnalyzeLogs.Enabled = false;
                                dgdLogList_SelectionChanged(null, null);
                                splitAnalysis.Panel2Collapsed = false;
                                ResumeLayout();
                                frmSSASDiag.LogFeatureUse("PerfMon Analysis", "Completed import of " + LogsRequiringAnalysis + " log" + (LogsRequiringAnalysis > 1 ? "s." : "."));
                            }));
                        }

                        SqlDataReader dr = new SqlCommand("select distinct MachineName from CounterDetails order by MachineName", connDB).ExecuteReader();
                        cmbServers.Invoke(new System.Action(() =>
                        {
                            cmbServers.Items.Clear();
                            while (dr.Read())
                                cmbServers.Items.Add(dr["MachineName"]);
                            dr.Close();
                            if (cmbServers.Items.Count > 0)
                            {
                                splitAnalysis.Visible = true;
                                cmbServers.SelectedIndex = 0;
                                if (!splitLogList.Panel1Collapsed)
                                    tableLayoutPanel1_Click(null, null);
                            }
                        }));
                    }
                })).Start();
            }
            else
            {
                bCancel = true;
                btnAnalyzeLogs.Text = "Import Selection";
                btnAnalyzeLogs.BackColor = Color.DarkSeaGreen;
                btnAnalyzeLogs.FlatAppearance.MouseDownBackColor = Color.FromArgb(128, 255, 128);
                btnAnalyzeLogs.FlatAppearance.MouseOverBackColor = Color.FromArgb(192, 255, 192);
                rtLogDetails.Text = "Imported " + LogCountAnalyzedInCurrentRun + " log" + (LogCountAnalyzedInCurrentRun != 1 ? "s" : "") + " before user cancelled.";
                frmSSASDiag.LogFeatureUse("PerfMon Analysis", "Dump analysis cancelled after " + LogCountAnalyzedInCurrentRun + " log" + (LogCountAnalyzedInCurrentRun != 1 ? "s" : "") + " were imported successfully.");
            }
        }

        public event EventHandler Shown;
        bool wasShown = false;
        private void ucASPerfMonAnalyzer_Paint(object sender, PaintEventArgs e)
        {
            if (!wasShown)
            {
                wasShown = true;
                if (Shown != null)
                    Shown(this, EventArgs.Empty);
            }
        }

        private void checkboxHeader_CheckedChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < dgdLogList.RowCount; i++)
            {
                dgdLogList[0, i].Value = ((CheckBox)dgdLogList.Controls.Find("checkboxHeader", true)[0]).Checked;
            }
            dgdLogList.EndEdit();
        }

        DataTable Counters = new DataTable();


        private void DgdGrouping_ColumnDisplayIndexChanged(object sender, System.Windows.Forms.DataGridViewColumnEventArgs e)
        {
            if (e.Column.DisplayIndex == 0)
            {
                tvCounters.SuspendLayout();
                List<System.Windows.Forms.TreeNode> nodes = tvCounters.GetCheckedLeafNodes();
                List<System.Windows.Forms.TreeNode> clonedOldNodes = nodes.Select(
                    x =>
                    {
                        System.Windows.Forms.TreeNode n = x.Clone() as System.Windows.Forms.TreeNode;
                        n.Tag = x.FullPath;
                        return n;
                    }
                    ).ToList();

                foreach (System.Windows.Forms.TreeNode n in tvCounters.Nodes)
                {
                    n.Nodes.Clear();
                    n.Checked = false;
                    n.Collapse();
                }

                string CounterName = "";
                string InstancePath = "";
                int? InstanceIndex = null;

                try
                {

                    if (Counters.Rows.Count > 0)
                    {
                        if (dgdGrouping.Columns["Counter"].DisplayIndex == 0)
                        {
                            DataRow[] rows = Counters.Select("", "objectname, countername, instancepath, instanceindex");

                            int i = 0;
                            while (i < rows.Count())
                            {
                                if (CounterName != rows[i]["CounterName"] as string)
                                {
                                    CounterName = rows[i]["CounterName"] as string;
                                    System.Windows.Forms.TreeNode n = tvCounters.Nodes[rows[i]["ObjectName"] as string].Nodes.Add(CounterName, CounterName);
                                    n.Tag = Convert.ToString(rows[i]["CounterID"]) + "," + rows[i]["DefaultScale"] as string;
                                    InstancePath = "";
                                    while (i < rows.Count() && rows[i]["CounterName"] as string == CounterName)
                                    {
                                        if (rows[i]["InstancePath"] as string != null && InstancePath != rows[i]["InstancePath"] as string)
                                        {
                                            InstancePath = rows[i]["InstancePath"] as string;
                                            System.Windows.Forms.TreeNode nn = n.Nodes.Add(InstancePath, InstancePath);
                                            nn.Tag = Convert.ToString(rows[i]["CounterID"]) + "," + rows[i]["DefaultScale"] as string;
                                            InstanceIndex = null;
                                            while (i < rows.Count() && rows[i]["InstancePath"] as string == InstancePath)
                                            {
                                                if (rows[i]["InstanceIndex"] as int? != null && rows[i]["InstanceIndex"] as int? != InstanceIndex)
                                                {
                                                    InstanceIndex = rows[i]["InstanceIndex"] as int?;
                                                    nn.Nodes.Add(InstanceIndex.ToString(), InstanceIndex.ToString()).Tag = Convert.ToString(rows[i]["CounterID"]) + "," + rows[i]["DefaultScale"] as string;
                                                }
                                                i++;
                                            }
                                            if (i == rows.Count()) break;
                                            i--;
                                        }
                                        i++;
                                    }
                                    if (i == rows.Count()) break;
                                    i--;
                                }
                                i++;
                            }
                        }
                        else
                        {
                            DataRow[] rows = Counters.Select("", "objectname, instancepath, instanceindex, countername");

                            int i = 0;
                            InstancePath = "";
                            while (i < rows.Count())
                            {
                                if (InstancePath != rows[i]["InstancePath"] as string || rows[i]["InstancePath"] as string == "")
                                {
                                    InstancePath = rows[i]["InstancePath"] as string;
                                    System.Windows.Forms.TreeNode n = null;
                                    if (InstancePath != "")
                                    {
                                        n = tvCounters.Nodes[rows[i]["ObjectName"] as string].Nodes.Add(InstancePath, InstancePath);
                                        n.Tag = Convert.ToString(rows[i]["CounterID"]) + "," + rows[i]["DefaultScale"] as string;
                                        InstanceIndex = null;
                                        while (i < rows.Count() && rows[i]["InstancePath"] as string == InstancePath)
                                        {
                                            if (rows[i]["InstanceIndex"] as int? != null && rows[i]["InstanceIndex"] as int? != InstanceIndex)
                                            {
                                                InstanceIndex = rows[i]["InstanceIndex"] as int?;
                                                System.Windows.Forms.TreeNode nn = n.Nodes.Add(InstanceIndex.ToString(), InstanceIndex.ToString());
                                                while (i < rows.Count() && rows[i]["InstanceIndex"] as int? == InstanceIndex)
                                                {
                                                    CounterName = rows[i]["CounterName"] as string;
                                                    nn.Nodes.Add(CounterName, CounterName);
                                                    nn.Tag = Convert.ToString(rows[i]["CounterID"]) + "," + rows[i]["DefaultScale"] as string;
                                                    i++;
                                                }
                                                if (i == rows.Count()) break;
                                                i--;
                                            }
                                            else
                                            {
                                                CounterName = rows[i]["CounterName"] as string;
                                                n.Nodes.Add(CounterName, CounterName).Tag = Convert.ToString(rows[i]["CounterID"]) + "," + rows[i]["DefaultScale"] as string;
                                            }
                                            i++;
                                        }
                                    }
                                    else
                                    {
                                        CounterName = rows[i]["CounterName"] as string;
                                        tvCounters.Nodes[rows[i]["ObjectName"] as string].Nodes.Add(CounterName, CounterName).Tag = Convert.ToString(rows[i]["CounterID"]) + "," + rows[i]["DefaultScale"] as string;
                                        i++;
                                    }
                                    if (i == rows.Count()) break;
                                    i--;
                                }
                                i++;
                            }
                        }
                    }
                    tvCounters.AfterCheck -= TvCounters_AfterCheck;
                    tvCounters.AfterSelect -= TvCounters_AfterSelect;
                    foreach (System.Windows.Forms.TreeNode n in clonedOldNodes)
                    {
                        string[] levels = (n.Tag as string).Split('\\');
                        System.Windows.Forms.TreeNode tmpnode = tvCounters.Nodes[levels[0]].Nodes[levels[levels.Length - 1]];
                        for (int i = levels.Length - 2; i > 0; i--)
                            tmpnode = tmpnode.Nodes[levels[i]];
                        tmpnode.Checked = true;
                        if (n.IsSelected)
                            tvCounters.SelectedNode = n;
                        tmpnode.Collapse();
                        tmpnode.Expand();
                    }
                    tvCounters.AfterCheck += TvCounters_AfterCheck;
                    tvCounters.AfterSelect += TvCounters_AfterSelect;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception: " + ex.Message);
                }
                tvCounters.ResumeLayout();
                tvCounters.Update();
            }
        }

        Point? prevPosition = null;
        ToolTip tooltip = new ToolTip();
        private void ChartPerfMon_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            var pos = e.Location;
            if (prevPosition.HasValue && pos == prevPosition.Value)
                return;
            tooltip.RemoveAll();
            prevPosition = pos;
            var results = chartPerfMon.HitTest(pos.X, pos.Y, false, ChartElementType.DataPoint);
            foreach (var result in results)
            {
                if (result.ChartElementType == ChartElementType.DataPoint)
                {
                    var prop = result.Object as DataPoint;
                    if (prop != null)
                    {
                        var pointXPixel = result.ChartArea.AxisX.ValueToPixelPosition(prop.XValue);
                        var pointYPixel = result.ChartArea.AxisY.ValueToPixelPosition(prop.YValues[0]);
                        tooltip.Show(prop.LegendText + "\n" + DateTime.FromOADate(prop.XValue) + "\nValue: " + prop.YValues[0], this.chartPerfMon, pos.X + 10, pos.Y);
                        CurrentSeriesUnderMouse = prop.LegendText;
                    }
                }
                else
                    CurrentSeriesUnderMouse = "";
            }
        }

        string CurrentSeriesUnderMouse = "";
        private void ChartPerfMon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (CurrentSeriesUnderMouse == "")
            {
                var results = chartPerfMon.HitTest(e.Location.X, e.Location.Y, false, ChartElementType.LegendItem);
                foreach (var result in results)
                {
                    var LegendItem = result.Object as LegendItem;
                    CurrentSeriesUnderMouse = LegendItem.SeriesName;
                    break;
                }
            }
            if (CurrentSeriesUnderMouse != "")
            {
                foreach (Series s in chartPerfMon.Series)
                    s.BorderWidth = 1;
                Series sr = chartPerfMon.Series[CurrentSeriesUnderMouse];
                sr.BorderWidth = 4;
                string[] NodeList = sr.LegendText.Split('\\');
                System.Windows.Forms.TreeNode n = tvCounters.Nodes[NodeList[0]];
                for (int i = 1; i < NodeList.Length; i++)
                    n = n.Nodes[NodeList[i]];
                tvCounters.SelectedNode = n;
            }
            CurrentSeriesUnderMouse = "";
        }
        private void TvCounters_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
        {
            if (e.Node.Nodes.Count == 0 && e.Node.Checked)
            {
                foreach (Series s in chartPerfMon.Series)
                    s.BorderWidth = 1;
                txtAvg.Text = txtMin.Text = txtMax.Text = "";
                chartPerfMon.Series[e.Node.FullPath].BorderWidth = 4;
                SqlDataReader dr = new SqlCommand(@"select avg(countervalue), max(countervalue), min(countervalue) from CounterData where CounterID in
                    (
                                    select bb.CounterID from (select * from CounterDetails where CounterID = " + (e.Node.Tag as string).Split(',')[0] + @") aa, CounterDetails bb where 
                                        bb.CounterName = aa.CounterName and 
	                                    (bb.InstanceName = aa.InstanceName or (bb.InstanceName is null and aa.InstanceName is null)) and 
	                                    (bb.InstanceIndex = aa.InstanceIndex or (bb.InstanceIndex is null and aa.InstanceIndex is null)) and 
	                                    (bb.ParentName = aa.ParentName or (bb.ParentName is null and aa.ParentName is null)) 
	                                    )", connDB).ExecuteReader();
                dr.Read();
                txtAvg.Text = (dr[0] as double?).Value.ToString();
                txtMin.Text = (dr[2] as double?).Value.ToString();
                txtMax.Text = (dr[1] as double?).Value.ToString();
                dr.Close();
            }
        }


        private void cmbServers_SelectedIndexChanged(object sender, EventArgs e)
        {
            tvCounters.Nodes.Clear();
            SqlDataReader dr = new SqlCommand("select distinct ObjectName from CounterDetails where MachineName = '" + cmbServers.SelectedItem + "' order by ObjectName", connDB).ExecuteReader();
            while (dr.Read())
                tvCounters.Nodes.Add(dr["ObjectName"] as string, dr["ObjectName"] as string);
            dr.Close();


            Counters.Clear();
            string qry = @" select
                                            max(CounterID) CounterID, ObjectName, CounterName, DefaultScale,
                                            case 

                                                when ParentName is null then

                                                    concat(InstanceName, case when InstanceIndex is not null then concat(' (', InstanceIndex, ')') end)
	                                            else

                                                    concat(ParentName, case when InstanceIndex is not null then concat(' (', InstanceIndex, ')') end)
                                            end InstancePath,
                                            convert(int, case when ParentName is not null and ParentName <> InstanceName then InstanceName end) InstanceIndex
                                            from counterdetails where MachineName = '" + cmbServers.SelectedItem + @"'
                                            group by ObjectName, CounterName, DefaultScale, ParentName, InstanceName, InstanceIndex";
            Counters.Load(new SqlCommand(qry,
                                            connDB).ExecuteReader());
            dr = new SqlCommand(@"  select 
                                    format(convert(int,format(a.intervaloffset, 'dd')) - 1, '00') + 
                                    ':' + 
                                    format(a.intervaloffset, 'HH:mm:ss.fff') Duration, StartTime, StopTime 
                                    from (
                                     select convert(datetime, LogStopTime) - convert(datetime, LogStartTime) IntervalOffset, 
                                     dateadd(mi, MinutesToUTC, convert(datetime, LogStartTime)) StartTime, 
                                     dateadd(mi, MinutesToUTC, convert(datetime, LogStopTime)) StopTime 
                                     from DisplayToID 
                                     where GUID = 
                                     (select top 1 GUID from CounterData 
                                      where CounterID = ( 
                                        select top 1 CounterID from CounterDetails 
				                        where MachineName = '" + cmbServers.SelectedItem + @"' 
				                        and CounterID = (select top 1 CounterID from CounterData)
				                      )))a", connDB).ExecuteReader();
            dr.Read();
            txtDur.Text = dr["Duration"] as string;

            trTimeRange.SetRangeLimit((dr["StartTime"] as DateTime?).Value, (dr["StopTime"] as DateTime?).Value);
            trTimeRange.SelectRange((dr["StartTime"] as DateTime?).Value, (dr["StopTime"] as DateTime?).Value);
            dr.Close();

            DgdGrouping_ColumnDisplayIndexChanged(sender, new DataGridViewColumnEventArgs(dgdGrouping.Columns[0]));

        }

        private void TvCounters_AfterCheck(object sender, System.Windows.Forms.TreeViewEventArgs e)
        {
            if (e.Node.Nodes.Count == 0)
            {
                if (e.Node.Checked && e.Node.Nodes.Count == 0)
                {
                    Series s = chartPerfMon.Series.FindByName(e.Node.FullPath);
                    if (s == null)
                    {
                        new SqlCommand("use [" + DBName() + "]", connDB).ExecuteNonQuery();
                        s = new Series(e.Node.FullPath);
                        s.ChartType = SeriesChartType.Line;
                        s.Name = e.Node.FullPath;
                        s.XValueType = ChartValueType.DateTime;
                        s.LegendText = e.Node.FullPath;
                        // Not ideal but Tag contains "CounterID,DefaultScale"...  I will update to pull scale in the query instead in the future.
                        string qry = @" select a.*, b.MinutesToUTC from CounterData a, DisplayToID b where a.GUID = b.GUID and CounterID in 
                                    (
                                    select bb.CounterID from (select * from CounterDetails where CounterID = " + (e.Node.Tag as string).Split(',')[0] + @") aa, CounterDetails bb where 
                                        bb.CounterName = aa.CounterName and 
	                                    (bb.InstanceName = aa.InstanceName or (bb.InstanceName is null and aa.InstanceName is null)) and 
	                                    (bb.InstanceIndex = aa.InstanceIndex or (bb.InstanceIndex is null and aa.InstanceIndex is null)) and 
	                                    (bb.ParentName = aa.ParentName or (bb.ParentName is null and aa.ParentName is null)) 
	                                    ) and
                                    dateadd(mi, MinutesToUTC, convert(datetime, convert(nvarchar(23), CounterDateTime, 121))) >= convert(datetime, '" + trTimeRange.RangeMinimum.ToString("yyyy-MM-dd HH:mm:ss.fff") + @"', 121) and
                                    dateadd(mi, MinutesToUTC, convert(datetime, convert(nvarchar(23), CounterDateTime, 121))) <= convert(datetime, '" + trTimeRange.RangeMaximum.ToString("yyyy-MM-dd HH:mm:ss.fff") + @"', 121)
                                    order by CounterDateTime asc";
                        SqlDataReader dr = new SqlCommand(qry, connDB).ExecuteReader();
                        while (dr.Read())
                            s.Points.AddXY(DateTime.Parse((dr["CounterDateTime"] as string).Trim('\0')).AddMinutes((dr["MinutesToUTC"] as int?).Value), (double)dr["CounterValue"] * Math.Pow(10, Convert.ToInt32((e.Node.Tag as string).Split(',')[1])));
                        dr.Close();
                        chartPerfMon.Invoke(new System.Action(() => chartPerfMon.Series.Add(s)));
                    }
                }
                else
                {
                    Series s = chartPerfMon.Series.FindByName(e.Node.FullPath);
                    if (s != null)
                        chartPerfMon.Series.Remove(s);
                }
            }
        }

        private void ucASPerfMonAnalyzer_SizeChanged(object sender, EventArgs e)
        {
            tvCounters.Height = splitPerfMonCountersAndChart.Height - dgdGrouping.Height;
            chartPerfMon.Height = splitPerfMonCountersAndChart.Height - pnlSeriesDetails.Height;
        }

        private void SplitAnalysis_SplitterMoved(object sender, System.Windows.Forms.SplitterEventArgs e)
        {
            ucASPerfMonAnalyzer_SizeChanged(sender, e);
        }

        private void tableLayoutPanel1_MouseMove(object sender, MouseEventArgs e)
        {
            tableLayoutPanel1.BackColor = SystemColors.ControlLight;
        }

        private void tableLayoutPanel1_MouseLeave(object sender, EventArgs e)
        {
            tableLayoutPanel1.BackColor = SystemColors.Control;
        }

        private void tableLayoutPanel1_MouseHover(object sender, EventArgs e)
        {
            if (lblHideFilesButton.Text == "≪")
                tooltip.Show("Hide log file details", Program.MainForm, System.Windows.Forms.Cursor.Position.X - Program.MainForm.Left + 5, System.Windows.Forms.Cursor.Position.Y - 18 - Program.MainForm.Top, 1000);
            else
                tooltip.Show("Show log file details", Program.MainForm, System.Windows.Forms.Cursor.Position.X - Program.MainForm.Left + 5, System.Windows.Forms.Cursor.Position.Y - 18 - Program.MainForm.Top, 1000);

        }

        private void tableLayoutPanel1_Click(object sender, EventArgs e)
        {
            tooltip.Hide(Program.MainForm);
            if (lblHideFilesButton.Text == "≪")
            {
                lblHideFilesButton.Text = "≫";
                splitLogList.Panel1Collapsed = true;
            }
            else
            {
                lblHideFilesButton.Text = "≪";
                splitLogList.Panel1Collapsed = false;
            }

        }

        private void dgdLogList_SelectionChanged(object sender, EventArgs e)
        {
            dgdLogList.SuspendLayout();
            PerfMonLog lComp = null;
            int AnalyzedCount = 0;
            int selCount = (dgdLogList.AreAllCellsSelected(true) ? dgdLogList.RowCount : dgdLogList.SelectedCells.Count);
            foreach (DataGridViewCell c in dgdLogList.SelectedCells)
            {
                if (c.Visible)
                {
                    PerfMonLog d = c.OwningRow.DataBoundItem as PerfMonLog;
                    if (d.Analyzed)
                    {
                        AnalyzedCount++;

                        if (lComp == null)
                            lComp = d.Clone();
                    }
                }
            }
            if (selCount > 0)
            {
                if (lComp == null)
                {
                    rtLogDetails.Text = "Selection requires initial import.";
                }
                else
                {
                    
                    string pluralize = (selCount > 1 ? "s: " : ": ");
                    rtLogDetails.Text = "Log file" + pluralize + (dgdLogList.SelectedCells.Count > 1 ? "<multiple>" : lComp.LogPath) + "\r\n" +
                        (AnalyzedCount < selCount ?
                            AnalyzedCount + " of " + selCount + " logs already imported for analysis.\r\n" :
                            (selCount == 1 ? "This log has been imported already." : selCount + " logs already imported for analysis.\r\n"));
                }
            }

            if (btnAnalyzeLogs.Text == "Import Selection" || btnAnalyzeLogs.Text == "")
            {
                btnAnalyzeLogs.Enabled = (AnalyzedCount < selCount);
                btnAnalyzeLogs.BackColor = (AnalyzedCount < selCount) ? Color.DarkSeaGreen : SystemColors.Control;
                btnAnalyzeLogs.Text = (AnalyzedCount < selCount) ? "Import Selection" : "";
            }
            dgdLogList.ResumeLayout();
        }

        private void dgdLogList_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ContextMenu cm = new ContextMenu();
                RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"Applications\windbg.exe\shell\open\command");
                string WinDbgPath = "";
                if (key != null)
                {
                    WinDbgPath = key.GetValue("") as string;
                    WinDbgPath = WinDbgPath.Substring(0, WinDbgPath.IndexOf(".exe") + ".exe".Length).Replace("\"", "");
                }
                if (WinDbgPath != "")
                {
                    PerfMonLog l = (dgdLogList.Rows[e.RowIndex].DataBoundItem as PerfMonLog);
                    cm.MenuItems.Add(new MenuItem("Open " + l.LogName + " in WinDbg for further analysis...",
                        new EventHandler((object o, EventArgs ea) =>
                            Process.Start(WinDbgPath, "-z \"" + l.LogPath + "\""))
                        ));
                    cm.Show(ParentForm, new Point(MousePosition.X - ParentForm.Left - 10, MousePosition.Y - ParentForm.Top - 26));
                }
            }
        }
    }



}
