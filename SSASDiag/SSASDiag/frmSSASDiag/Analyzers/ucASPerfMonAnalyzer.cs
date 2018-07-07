using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Windows.Forms.DataVisualization.Charting;

namespace SSASDiag
{
    public partial class ucASPerfMonAnalyzer : UserControl
    {
        #region locals
        frmStatusFloater StatusFloater = null;
        string LogPath, AnalysisPath, PerfMonAnalysisId;
        SqlConnection connDB;
        List<PerfMonLog> LogFiles = new List<PerfMonLog>();
        List<TreeNode> NodesRemaingingToProcessInBatch = new List<TreeNode>();
        ImageList legend = new ImageList();
        TimeRangeBar trTimeRange = new TimeRangeBar();
        public TriStateTreeView tvCounters;
        private StripLine stripLine = new StripLine();
        int LogCountAnalyzedInCurrentRun = 0;
        public bool bCancel = false;
        public event EventHandler Shown;
        bool wasShown = false;
        public DataTable Counters = new DataTable();
        Point? prevPosition = null;
        ToolTip tooltip = new ToolTip();
        string CurrentSeriesUnderMouse = "";
        TreeNode ParentNodeOfUpdateBatch = null;
        SortableBindingList<Rule> Rules = new SortableBindingList<Rule>();

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
        #endregion locals

        public ucASPerfMonAnalyzer(string logPath, SqlConnection conndb, frmStatusFloater statusFloater)
        {
            InitializeComponent();

            splitLogList.Visible = false;
            this.Shown += UcASPerfMonAnalyzer_Shown;
            #region non-designer controls

            stripLine.Interval = 0;
            stripLine.StripWidth = 0;
            stripLine.IntervalOffset = 0;
            // pick you color etc ... before adding the stripline to the axis
            stripLine.BackColor = Color.FromArgb(200, Color.LightBlue);
            chartPerfMon.ChartAreas[0].AxisX.StripLines.Add(stripLine);

            trTimeRange.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            trTimeRange.BackColor = System.Drawing.SystemColors.Control;
            trTimeRange.DivisionNum = 15;
            trTimeRange.HeightOfBar = 12;
            trTimeRange.HeightOfMark = 16;
            trTimeRange.HeightOfTick = 4;
            trTimeRange.Height = 56;
            trTimeRange.InnerColor = Color.LightBlue;
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
            tvCounters.Dock = System.Windows.Forms.DockStyle.Fill;
            tvCounters.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            tvCounters.Location = new System.Drawing.Point(0, 130);
            tvCounters.Name = "tvCounters";
            tvCounters.Size = new System.Drawing.Size(200, 238);
            tvCounters.TabIndex = 0;
            tvCounters.Margin = new Padding(0);
            tvCounters.TriStateStyleProperty = TriStateTreeView.TriStateStyles.Installer;
            tvCounters.AfterCheck += TvCounters_AfterCheck;
            tvCounters.AfterSelect += TvCounters_AfterSelect;
            tvCounters.BeforeCheck += TvCounters_BeforeCheck;
            tvCounters.ImageList = legend;
            tvCounters.ShowNodeToolTips = true;
            pnlCounters.Controls.Add(tvCounters);

            legend.ImageSize = new Size(16, 16);
            legend.TransparentColor = System.Drawing.Color.Transparent;
            legend.ColorDepth = ColorDepth.Depth32Bit;
            Bitmap bmp = new Bitmap(16, 16);
            Graphics g = Graphics.FromImage(bmp);
            g.FillRectangle(Brushes.Transparent, 0, 0, 16, 16);
            legend.Images.Add(bmp);

            splitHideFilesButton.SplitterDistance = 15;
            dgdGrouping.ColumnDisplayIndexChanged += DgdGrouping_ColumnDisplayIndexChanged;
            cmbServers.SelectedIndexChanged += cmbServers_SelectedIndexChanged;

            #endregion non-designer controls
            chartPerfMon.Legends[0].BackColor = Color.FromArgb(210, SystemColors.Window);
            chartPerfMon.Legends[0].Font = txtDur.Font;
            StatusFloater = statusFloater;
            StatusFloater.lblStatus.Text = StatusFloater.lblSubStatus.Text = "";
            LogPath = logPath;
            connDB = new SqlConnection(conndb.ConnectionString);
            connDB.Open();
            HandleDestroyed += UcASPerfMonAnalyzer_HandleDestroyed;
            ChartArea c = chartPerfMon.ChartAreas[0];
            c.IsSameFontSizeForAllAxes = true;
            c.AxisX.LabelAutoFitMaxFontSize = c.AxisY.LabelAutoFitMaxFontSize = c.AxisX2.LabelAutoFitMaxFontSize = c.AxisY2.LabelAutoFitMaxFontSize = 7;
            this.dgdGrouping.Columns[0].SortMode = this.dgdGrouping.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;
            splitAnalysis.SplitterMoved += SplitAnalysis_SplitterMoved;
        }

        double BreakdownExpression(string expr, Rule rule, List<RuleExpression> ruleExpressions)
        {
            int iCurPos = 0;
            MatchCollection Counters = Regex.Matches(expr, "(\\[.*?\\])");
            foreach(Match c in Counters)
            {
                iCurPos = expr.IndexOf(c.Value);
                while (iCurPos != -1)
                {
                    iCurPos += c.Value.Length;
                    char breakchar = '\0';
                    while (breakchar != ' ' && breakchar != '(' && iCurPos < expr.Length)
                        breakchar = expr.Substring(iCurPos++, 1)[0];
                    if (breakchar == '(')
                        while (breakchar != ')')
                            breakchar = expr.Substring(iCurPos++, 1)[0];
                    List<Series> series = rule.Counters.Where(cc => cc.WildcardPath == c.Value.Replace("[", "").Replace("]", "") && (cc.Include_TotalSeriesInWildcard ? true : !cc.Path.Contains("_Total"))).Select(rc=>rc.ChartSeries).ToList();
                    string function = expr.Substring(expr.IndexOf(c.Value), iCurPos - expr.IndexOf(c.Value)).Replace(c.Value + ".", "").ToLower().Replace(" ", "").Replace(")", "");
                    double val = 0;
                    switch (function)
                    {
                        case "first":
                            val = series.First().Points[0].YValues[0];
                            break;
                        case "last":
                            val = series.Last().Points.Last().YValues[0];
                            break;
                        case "max":
                            val = series.Max(sr=>sr.Points.FindMaxByValue().YValues[0]);
                            break;
                        case "min":
                            val = series.Min(sr=>sr.Points.FindMinByValue().YValues[0]);
                            break;
                        case "avg":
                            val = series.Average(sr=>sr.Points.AverageValue());
                            break;
                        case "avgexcludezero":
                            val = series.Average(sr=>sr.Points.AverageValue(false, false));
                            break;
                        case "avgincludenull":
                            val = series.Average(sr=>sr.Points.AverageValue(true));
                            break;
                        case "count":
                            val = series.Count();
                            break;
                    }
                    expr = expr.Substring(0, expr.IndexOf(c.Value)) + val + expr.Substring(expr.IndexOf(c.Value) + c.Value.Length + function.Length + 1);
                    iCurPos = expr.IndexOf(c.Value);
                }
            }
            iCurPos = 0;
            int iWordStart = -1;
            while (iCurPos < expr.Length)
            {
                if (char.IsLetter(expr[iCurPos]))
                {
                    if (iWordStart == -1)
                        iWordStart = iCurPos;
                    iCurPos++;
                }
                else
                {
                    if (char.IsLetterOrDigit(expr[iCurPos]) || expr[iCurPos] == '-' || expr[iCurPos] == '_')
                        iCurPos++;
                    else
                    {
                        if (iWordStart != -1)
                        {
                            string subExpr = expr.Substring(iWordStart, iCurPos - iWordStart);
                            double d;
                            if (double.TryParse(subExpr, out d))
                                return d;
                            d = ruleExpressions.Where(re => re.Name == subExpr).First().Value;
                            expr = expr.Replace(subExpr, d.ToString());
                            if (Regex.Matches(expr, @"[a-zA-Z]").Count == 0)
                                return Convert.ToDouble(new DataTable().Compute(expr, ""));
                            iCurPos = -1;
                            iWordStart = -1;
                        }
                        iCurPos++;
                    }
                }
            }
            return Convert.ToDouble(new DataTable().Compute(expr, ""));
        }

        private void LoadRuleFromRegistry(string rule)
        {
            RegistryKey rules = Registry.LocalMachine.CreateSubKey("SOFTWARE\\SSASDiag\\PerfMonRules", RegistryKeyPermissionCheck.ReadSubTree);
            RegistryKey r = rules.OpenSubKey(rule, false);
            string desc = r.GetValue("Description") as string, cat = r.GetValue("Category") as string;
            Rule NewRule = new Rule(rule, cat, desc);
            RegistryKey counters = r.OpenSubKey("Counters", false);
            foreach (string c in counters.GetSubKeyNames())
            {
                RegistryKey ctr = counters.OpenSubKey(c);
                List<RuleCounter> ctrs = RuleCounter.CountersFromPath(c.Replace("{SLASH}", "\\"), Convert.ToBoolean(ctr.GetValue("Display")), Convert.ToBoolean(ctr.GetValue("Highlight")), Convert.ToBoolean(ctr.GetValue("WildcardIncludes_Total")));
                if (ctrs.Count == 0)
                {
                    NewRule.RuleResult = RuleResultEnum.CountersUnavailable;
                }
                NewRule.Counters.AddRange(ctrs);
                ctr.Close();
            }
            counters.Close();
            RegistryKey expressions = r.OpenSubKey("Expressions", false);
            List<RuleExpression> NewRuleExpressions = new List<RuleExpression>();
            foreach (string e in expressions.GetSubKeyNames())
            {
                RegistryKey expr = expressions.OpenSubKey(e);
                NewRuleExpressions.Add(new RuleExpression(e, expr.GetValue("Expression") as string, Convert.ToInt32(expr.GetValue("Index")), Convert.ToBoolean(expr.GetValue("Display")), Convert.ToBoolean(expr.GetValue("Highlight")), Convert.ToBoolean(expr.GetValue("WildcardIncludes_Total"))));
            }
            expressions.Close();
            r.Close();
            
            NewRule.RuleFunction = new Action(() =>
            {
                NewRule.RuleResult = RuleResultEnum.Pass;
                r = rules.OpenSubKey(rule, false);
                foreach (RuleExpression re in NewRuleExpressions.OrderBy(x => x.Index))
                {
                    double d;
                    if (Double.TryParse(re.Expression, out d))
                        re.Value = d;
                    else
                        re.Value = BreakdownExpression(re.Expression, NewRule, NewRuleExpressions);
                    if (re.Display)
                    {
                        StripLine sl = new StripLine();
                        Random rand = new Random((int)DateTime.Now.Ticks);
                        int color = rand.Next((int)indexcolors.Length - 1);
                        ColorConverter c = new ColorConverter();
                        sl.BackColor = sl.BorderColor = (c.ConvertFromString(indexcolors[color]) as Color?).Value;
                        sl.ForeColor = Color.Transparent;
                        sl.IntervalOffset = re.Value;
                        sl.BorderWidth = re.Highlight ? 3 : 1;
                        sl.Text = re.Name;
                        NewRule.CustomStripLines.Add(sl);
                    }
                }
                string ValOrSeriesToCheck = r.GetValue("ValueOrSeriesToCheck") as string;
                if (NewRule.Counters.Where(c => c.WildcardPath == ValOrSeriesToCheck).Count() > 0 || NewRuleExpressions.Where(e=>e.Name == ValOrSeriesToCheck).Count() > 0)
                {
                    string SeriesFunction = r.GetValue("SeriesFunction") as string;
                    if (NewRule.Counters.Where(c => c.WildcardPath == ValOrSeriesToCheck && (c.Include_TotalSeriesInWildcard ? true : !c.Path.Contains("_Total"))).Select(rc => rc.ChartSeries).Count() > 0)
                        NewRule.ValidateThresholdRule(NewRule.Counters.Where(c => c.WildcardPath == ValOrSeriesToCheck && (c.Include_TotalSeriesInWildcard ? true : !c.Path.Contains("_Total"))).Select(rc => rc.ChartSeries).ToList(),
                                                        r.GetValue("WarnExpr") == null ? -1 : NewRuleExpressions.Where(re => re.Name == r.GetValue("WarnExpr") as string).First().Value,
                                                        NewRuleExpressions.Where(re => re.Name == r.GetValue("ErrorExpr") as string).First().Value,
                                                        r.GetValue("WarnRegionLabel") == null ? null : r.GetValue("WarnRegionLabel") as string,
                                                        r.GetValue("ErrorRegionLabel") as string,
                                                        r.GetValue("PassRegionLabel") as string,
                                                        Convert.ToBoolean(r.GetValue("FailIfBelowWarnError")),
                                                        SeriesFunction.StartsWith("Avg"), SeriesFunction.Contains("ignore nulls and zeros"), SeriesFunction.Contains("include all values"),
                                                        Convert.ToInt32(r.GetValue("PctRequiredToMatchWarnError")));
                    else
                        NewRule.ValidateThresholdRule(NewRuleExpressions.Where(ex=>ex.Name == ValOrSeriesToCheck).First().Value,
                                                            r.GetValue("WarnExpr") == null ? -1 : NewRuleExpressions.Where(re => re.Name == r.GetValue("WarnExpr") as string).First().Value,
                                                            NewRuleExpressions.Where(re => re.Name == r.GetValue("ErrorExpr") as string).First().Value,
                                                            r.GetValue("WarnRegionLabel") == null ? null : r.GetValue("WarnRegionLabel") as string,
                                                            r.GetValue("ErrorRegionLabel") as string,
                                                            r.GetValue("PassRegionLabel") as string,
                                                            Convert.ToBoolean(r.GetValue("FailIfBelowWarnError")),
                                                            false, false, false,
                                                            Convert.ToInt32(r.GetValue("PctRequiredToMatchWarnError")));
                    if (NewRule.RuleResult == RuleResultEnum.Fail)
                        NewRule.ResultDescription = "Fail: " + r.GetValue("ErrorText") as string;
                    else if (NewRule.RuleResult == RuleResultEnum.Warn)
                        NewRule.ResultDescription = "Warn: " + r.GetValue("WarningText") as string;
                    else if (NewRule.RuleResult == RuleResultEnum.Pass)
                        NewRule.ResultDescription = "Pass: " + r.GetValue("PassText") as string;
                    else
                        NewRule.ResultDescription = "";
                }

                r.Close();
            });
            if (InvokeRequired)
                Invoke(new Action(()=>Rules.Add(NewRule)));
            else
                Rules.Add(NewRule);
        }

        private void LoadRulesFromRegistry()
        {
            RegistryKey rules = Registry.LocalMachine.CreateSubKey("SOFTWARE\\SSASDiag\\PerfMonRules", RegistryKeyPermissionCheck.ReadSubTree);
            foreach (string r in rules.GetSubKeyNames())
                LoadRuleFromRegistry(r);
            rules.Close();
            Rules = new SortableBindingList<Rule>(Rules.OrderBy(r => r.Category).ThenBy(r => r.Name).ToList());
            SqlDataReader dr = new SqlCommand("select * from RuleResults", connDB).ExecuteReader();
            while (dr.Read())
            {
                Rule r = Rules.Where(rr => rr.Name == dr["RuleName"] as string).First();
                r.RuleResult = (RuleResultEnum)dr["Result"];
                if (dr["ResultDescription"] != null)
                    r.ResultDescription = dr["ResultDescription"] as string;
            }
            dr.Close();

            BindingSource b = new BindingSource();
            Invoke(new Action(() =>
            {
                b.DataSource = Rules;
                cmbRuleFilter.Items.Clear();
                cmbRuleFilter.Items.Add("<Show all rules>");
                cmbRuleFilter.Items.AddRange(Rules.OrderBy(r=>r.Category).Select(r => r.Category).Distinct().ToArray());
                cmbRuleFilter.SelectedIndex = 0;
                dgdRules.DataSource = b;
                dgdRules.Focus();
            }));
        }

        private void UcASPerfMonAnalyzer_Shown(object sender, EventArgs e)
        {
            if (Directory.Exists(LogPath))
            {
                List<string> logfiles = new List<string>();
                logfiles.AddRange(Directory.GetFiles(LogPath, "*.blg", SearchOption.TopDirectoryOnly));
                foreach (string dir in Directory.EnumerateDirectories(LogPath))
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
                LogFiles.Add(new PerfMonLog() { LogPath = LogPath, Analyzed = false });
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
            cmd.CommandText = "if not exists(select* from sysobjects where name= 'RuleResults' and xtype = 'U') CREATE TABLE[dbo].[RuleResults]"
                                + " ([RuleName] [nvarchar] (max) NOT NULL, [MachineName] [nvarchar] (max), [Result] int NOT NULL, [ResultDescription] [nvarchar] (max) NULL) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]";
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
            catch (Exception ex)
            {
                if (ex.Message.Contains("CounterDetails"))
                    splitAnalysis.Visible = false;
            }

            dgdLogList.DataBindingComplete += DgdLogList_DataBindingComplete;
            dgdLogList.DataSource = LogFiles;

            Program.MainForm.BringToFront();
            splitLogList.Visible = true;

            frmSSASDiag.LogFeatureUse("PerfMon Analysis", "PerfMon analysis initalized for " + LogFiles.Count + " logs, " + LogFiles.Where(d => !d.Analyzed).Count() + " of which still require import for analysis.");
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
            //try
            //{
            //    string AnalysisZipFile = Directory.GetParent(Directory.GetParent(AnalysisPath).FullName).FullName + "\\" + Directory.GetParent(AnalysisPath).Name + ".zip";
            //    if (File.Exists(AnalysisZipFile))
            //    {
            //        frmStatusFloater f = new frmStatusFloater();
            //        f.Top = Screen.PrimaryScreen.WorkingArea.Height / 2 - f.Height / 2;
            //        f.Left = Screen.PrimaryScreen.WorkingArea.Width / 2 - f.Width / 2;
            //        f.lblStatus.Text = "Adding PerfMon analysis database to zip...";
            //        f.lblSubStatus.Text = "(Esc to cancel)";
            //        f.EscapePressed = false;
            //        f.Show();

            //        ZipFile z = new ZipFile(AnalysisZipFile);
            //        z.UseZip64WhenSaving = Ionic.Zip.Zip64Option.Always;
            //        z.ParallelDeflateThreshold = -1;
            //        z.AddFiles(new string[] { MDFPath(), LDFPath() }, Directory.GetParent(AnalysisPath).Name + "/Analysis");
            //        CancellationTokenSource cts = new CancellationTokenSource();
            //        Task t = Task.Run(()=>z.Save(), cts.Token);
            //        System.Runtime.CompilerServices.TaskAwaiter ta = t.GetAwaiter();
            //        while (!ta.IsCompleted)
            //        {
            //            Thread.Sleep(100);
            //            if (f.EscapePressed)
            //            {
            //                cts.Cancel();
            //                f.Close();
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Trace.WriteLine(Program.CurrentFormattedLocalDateTime() + ": Exception adding PerfMon analysis to zip folder: " + ex.Message);
            //}
        }

        private void TvCounters_BeforeCheck(object sender, TreeViewCancelEventArgs e)
        {
            if (tvCounters.GetLeafNodes(e.Node, false).Where(n => n.SelectedImageIndex > 0 || !n.Checked).Count() > 256 &&
                e.Node.Checked == false)
            {
                Point p = System.Windows.Forms.Cursor.Position;
                Point tvCounterLoc = tvCounters.Parent.PointToScreen(tvCounters.Location);
                p.Offset(-tvCounterLoc.X, -tvCounterLoc.Y + 16);
                tooltip.Show("This node covers too many counters for group selection.", this, p, 1200);
                e.Cancel = true;
            }
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
            if (chartPerfMon.ChartAreas[0].AxisX.Maximum == trTimeRange.RangeMaximum.ToOADate() && chartPerfMon.ChartAreas[0].AxisX.Minimum == trTimeRange.RangeMinimum.ToOADate())
                return;
            stripLine.StripWidth = 0;
            chartPerfMon.ChartAreas[0].AxisX.Minimum = trTimeRange.RangeMinimum.ToOADate();
            chartPerfMon.ChartAreas[0].AxisX.Maximum = trTimeRange.RangeMaximum.ToOADate();
            txtDur.Text = (trTimeRange.RangeMaximum - trTimeRange.RangeMinimum).ToString("dd\\:hh\\:mm\\:ss");
        }

        private void DgdLogList_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
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
                        dsnKey.SetValue("Server", connDB.ConnectionString.Split(';').ToList().Where(s => s.ToLower().StartsWith("server") || s.ToLower().StartsWith("data source")).First().Split('=')[1]);
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
                                        p.StartInfo.Arguments = "\"" + l.LogPath + "\" -f SQL -o SQL:SSASDiagPerfMonDSN!" + l.LogName.Replace(" ", "") + " -y";
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
                                            new SqlCommand("delete from RuleResults", connDB).ExecuteNonQuery();
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

                        SqlDataReader dr = null;
                        try
                        {
                            dr = new SqlCommand("select distinct MachineName from CounterDetails order by MachineName", connDB).ExecuteReader();
                        }
                        catch (SqlException ex)
                        {
                            if (ex.Message == "Invalid object name 'CounterDetails'.")
                            {
                                MessageBox.Show("This performance monitor log was empty or corrupt.", "Invalid log file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                connDB.ChangeDatabase("master");
                                new SqlCommand("alter database [" + DBName() + "] set single_user with rollback immediate", connDB).ExecuteNonQuery();
                                new SqlCommand("drop database [" + DBName() + "]", connDB).ExecuteNonQuery();
                                Invoke(new Action(()=>Visible = false));
                                return;
                            }
                        }
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
        
        private void ucASPerfMonAnalyzer_Paint(object sender, PaintEventArgs e)
        {
            if (!wasShown)
            {
                wasShown = true;
                if (Shown != null)
                    Shown(this, EventArgs.Empty);
            }
        }

        private void DgdGrouping_ColumnDisplayIndexChanged(object sender, System.Windows.Forms.DataGridViewColumnEventArgs e)
        {
            if (e.Column.DisplayIndex == 0)
            {
                tvCounters.AfterCheck -= TvCounters_AfterCheck;
                tvCounters.AfterSelect -= TvCounters_AfterSelect;

                Form f = Program.MainForm;
                DrawingControl.SuspendDrawing(f);
                f.Enabled = false;

                new Thread(new ThreadStart(() =>
                {
                    StatusFloater.Invoke(new Action(() =>
                    {
                        StatusFloater.Top = f.Top + f.Height / 2 - StatusFloater.Height / 2;
                        StatusFloater.Left = f.Left + f.Width / 2 - StatusFloater.Width / 2;
                        StatusFloater.lblStatus.Text = "Loading counter hierarchy...";
                        StatusFloater.lblSubStatus.Text = "";
                        StatusFloater.lblTime.Text = "";
                        StatusFloater.AutoUpdateDuration = false;
                        StatusFloater.Visible = true;
                    }));
                    List<TreeNode> nodes = tvCounters.GetLeafNodes();
                    List<TreeNode> clonedOldNodes = nodes.Select(
                        x =>
                        {
                            TreeNode n = x.Clone() as TreeNode;
                            n.Tag = x.FullPath;
                            return n;
                        }
                        ).ToList();

                    if (tvCounters.InvokeRequired)
                        tvCounters.Invoke(new Action(() =>
                    {
                        foreach (TreeNode n in tvCounters.Nodes)
                        {
                            n.Nodes.Clear();
                            n.Checked = false;
                            n.Collapse();
                        }
                    }));

                    string CounterName = "";
                    string InstancePath = "";
                    int? InstanceIndex = null;

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
                                    TreeNode n = null;
                                    tvCounters.Invoke(new Action(() => n = tvCounters.Nodes[rows[i]["ObjectName"] as string].Nodes.Add(CounterName, CounterName)));
                                    n.Tag = Convert.ToString(rows[i]["CounterID"]);
                                    InstancePath = "";
                                    while (i < rows.Count() && rows[i]["CounterName"] as string == CounterName)
                                    {
                                        if (rows[i]["InstancePath"] as string != null && InstancePath != rows[i]["InstancePath"] as string)
                                        {
                                            InstancePath = rows[i]["InstancePath"] as string;
                                            TreeNode nn = null;
                                            tvCounters.Invoke(new Action(() => nn = n.Nodes.Add(InstancePath, InstancePath)));
                                            nn.Tag = Convert.ToString(rows[i]["CounterID"]);
                                            InstanceIndex = null;
                                            while (i < rows.Count() && rows[i]["InstancePath"] as string == InstancePath)
                                            {
                                                if (rows[i]["InstanceIndex"] as int? != null && rows[i]["InstanceIndex"] as int? != InstanceIndex)
                                                {
                                                    InstanceIndex = rows[i]["InstanceIndex"] as int?;
                                                    tvCounters.Invoke(new Action(() => nn.Nodes.Add(InstanceIndex.ToString(), InstanceIndex.ToString()).Tag = Convert.ToString(rows[i]["CounterID"])));
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
                                    TreeNode n = null;
                                    if (InstancePath != "")
                                    {
                                        tvCounters.Invoke(new Action(() => n = tvCounters.Nodes[rows[i]["ObjectName"] as string].Nodes.Add(InstancePath, InstancePath)));
                                        n.Tag = Convert.ToString(rows[i]["CounterID"]);
                                        InstanceIndex = null;
                                        while (i < rows.Count() && rows[i]["InstancePath"] as string == InstancePath)
                                        {
                                            if (rows[i]["InstanceIndex"] as int? != null && rows[i]["InstanceIndex"] as int? != InstanceIndex)
                                            {
                                                InstanceIndex = rows[i]["InstanceIndex"] as int?;
                                                TreeNode nn = null;
                                                tvCounters.Invoke(new Action(() => nn = n.Nodes.Add(InstanceIndex.ToString(), InstanceIndex.ToString())));
                                                while (i < rows.Count() && rows[i]["InstanceIndex"] as int? == InstanceIndex)
                                                {
                                                    CounterName = rows[i]["CounterName"] as string;
                                                    tvCounters.Invoke(new Action(() => nn.Nodes.Add(CounterName, CounterName)));
                                                    nn.Tag = Convert.ToString(rows[i]["CounterID"]);
                                                    i++;
                                                }
                                                if (i == rows.Count()) break;
                                                i--;
                                            }
                                            else
                                            {
                                                CounterName = rows[i]["CounterName"] as string;
                                                tvCounters.Invoke(new Action(() => n.Nodes.Add(CounterName, CounterName).Tag = Convert.ToString(rows[i]["CounterID"])));
                                            }
                                            i++;
                                        }
                                    }
                                    else
                                    {
                                        CounterName = rows[i]["CounterName"] as string;
                                        tvCounters.Invoke(new Action(() => tvCounters.Nodes[rows[i]["ObjectName"] as string].Nodes.Add(CounterName, CounterName).Tag = Convert.ToString(rows[i]["CounterID"])));
                                        i++;
                                    }
                                    if (i == rows.Count()) break;
                                    i--;
                                }
                                i++;
                            }
                        }
                    }
                    foreach (TreeNode n in clonedOldNodes)
                    {
                        string[] levels = (n.Tag as string).Split('\\');
                        TreeNode tmpnode = tvCounters.Nodes[levels[0]].Nodes[levels[levels.Length - 1]];
                        for (int i = levels.Length - 2; i > 0; i--)
                            tmpnode = tmpnode.Nodes[levels[i]];
                        tvCounters.Invoke(new Action(() =>
                        {
                            tmpnode.Checked = true;
                            Series s = chartPerfMon.Series[n.Tag as string];
                            s.Name = s.LegendText = tmpnode.FullPath;
                            Pen pen = new Pen(s.Color);
                            pen.Width = 6;
                            Bitmap bmp = new Bitmap(16, 16);
                            using (Graphics g = Graphics.FromImage(bmp))
                            {
                                g.FillRectangle(Brushes.Transparent, 0, 0, 32, 32);
                                g.DrawLine(pen, 0, 6, 16, 6);
                            }
                            legend.Images.Add(bmp);
                            tmpnode.SelectedImageIndex = tmpnode.ImageIndex = legend.Images.Count - 1;
                            if (n.IsSelected && n.Nodes.Count == 0)
                                tvCounters.SelectedNodes.Add(n);
                        }));
                    }
                    tvCounters.AfterCheck += TvCounters_AfterCheck;
                    tvCounters.AfterSelect += TvCounters_AfterSelect;
                    Invoke(new Action(() =>
                    {
                        DrawingControl.ResumeDrawing(f);
                        tvCounters.CollapseAll();
                        tvCounters.Update();
                        tvCounters.ResumeLayout(true);
                        StatusFloater.Visible = false;
                        f.BringToFront();
                        f.Enabled = true;
                        Rules.Clear();
                    }));
                    LoadRulesFromRegistry();                   
                })).Start();
            }
        }

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
                if (result.ChartElementType != ChartElementType.Nothing)
                {
                    var prop = result.Object as DataPoint;
                    if (prop != null)
                    {
                        tooltip.Show(prop.LegendText + "\n" + DateTime.FromOADate(prop.XValue) + "\nValue: " + ((double)prop.Tag).ToString("#,###.############"), this.chartPerfMon, pos.X + 10, pos.Y);
                        CurrentSeriesUnderMouse = prop.LegendText;
                    }
                }
                else
                    CurrentSeriesUnderMouse = "";
            }
        }

        private void HighlightSeriesFromSelection()
        {
            int j = 0;
            for (int i = 0; i < chartPerfMon.Series.Count - j; i++)
            {
                Series s = chartPerfMon.Series[i];
                TreeNode nSeriesNode = tvCounters.SelectedNodes.ToList().Find(node => node.FullPath == s.Name);
                if (nSeriesNode != null)
                {
                    j++;
                    i--;
                    chartPerfMon.Series.Remove(s);
                    chartPerfMon.Series.Add(s);
                    s.BorderWidth = 3;
                    
                    while (nSeriesNode.Parent != null)
                    {
                        nSeriesNode.Expand();
                        nSeriesNode = nSeriesNode.Parent;
                    }

                }
                else
                    s.BorderWidth = 1;
            }
        }

        private void ChartPerfMon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (CurrentSeriesUnderMouse == "")
            {
                var results = chartPerfMon.HitTest(e.Location.X, e.Location.Y, false, ChartElementType.LegendItem);
                foreach (var result in results)
                {
                    var LegendItem = result.Object as LegendItem;
                    if (LegendItem != null)
                        CurrentSeriesUnderMouse = LegendItem.SeriesName;
                    else
                    {
                        if (ModifierKeys != Keys.Control && ModifierKeys != Keys.Shift)
                        {
                            txtMax.Text = txtAvg.Text = txtMin.Text = "--";
                            tvCounters.SelectedNodes.Clear();
                            CurrentSeriesUnderMouse = "";
                            HighlightSeriesFromSelection();
                        }
                    }
                    break;
                }
            }
            if (CurrentSeriesUnderMouse != "")
            {
                if (e.Button == MouseButtons.Left)
                {
                    Series sr = chartPerfMon.Series[CurrentSeriesUnderMouse];
                    if (CurrentRuleCustomSeries.Where(s=>s == CurrentSeriesUnderMouse).Count() > 0)
                    {
                        if (sr.BorderWidth == 1)
                            sr.BorderWidth = 3;
                        else
                            sr.BorderWidth = 1;
                    }
                    else
                    {
                        string[] NodeList = sr.LegendText.Split('\\');
                        TreeNode n = tvCounters.Nodes[NodeList[0]];
                        for (int i = 1; i < NodeList.Length; i++)
                            n = n.Nodes[NodeList[i]];
                        if (ModifierKeys == Keys.Control || ModifierKeys == Keys.Shift)
                        {
                            if (!tvCounters.SelectedNodes.Contains(n))
                                tvCounters.SelectedNodes.Add(n);
                            else
                                tvCounters.SelectedNodes.Remove(n);
                        }
                        else
                            tvCounters.SelectedNode = n;
                        TvCounters_AfterSelect(sender, new TreeViewEventArgs(n));
                    }
                }
                else if (e.Button == MouseButtons.Right)
                {
                    var pos = e.Location;
                    tooltip.RemoveAll();
                    prevPosition = pos;
                    var results = chartPerfMon.HitTest(pos.X, pos.Y, false, ChartElementType.DataPoint);
                    foreach (var result in results)
                    {
                        if (result.ChartElementType != ChartElementType.Nothing)
                        {
                            var prop = result.Object as DataPoint;
                            if (prop != null)
                            {
                                string clipboardText = prop.LegendText + "\n" + DateTime.FromOADate(prop.XValue) + "\nValue: " + (double)prop.Tag;
                                Clipboard.SetText(clipboardText);
                                tooltip.Show(clipboardText + "\n(copied to clipboard)", this.chartPerfMon, pos.X + 10, pos.Y, 1500);
                                CurrentSeriesUnderMouse = prop.LegendText;
                            }
                        }
                        else
                            CurrentSeriesUnderMouse = "";
                    }
                }
            }
            CurrentSeriesUnderMouse = "";
        }

        private void TvCounters_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Nodes.Count == 0)
            {
                chartPerfMon.SuspendLayout();

                HighlightSeriesFromSelection();

                if (tvCounters.SelectedNodes.Count == 1)
                {
                    if (tvCounters.SelectedNodes.Where(n => n.Checked).Count() == 1)
                    {
                        SqlDataReader dr = new SqlCommand(@"select avg(countervalue), max(countervalue), min(countervalue) from CounterData where CounterID in
                                (
                                select bb.CounterID from (select * from CounterDetails where CounterID = " + e.Node.Tag as string + @") aa, CounterDetails bb where 
                                    bb.CounterName = aa.CounterName and 
	                                (bb.InstanceName = aa.InstanceName or (bb.InstanceName is null and aa.InstanceName is null)) and 
	                                (bb.InstanceIndex = aa.InstanceIndex or (bb.InstanceIndex is null and aa.InstanceIndex is null)) and 
	                                (bb.ParentName = aa.ParentName or (bb.ParentName is null and aa.ParentName is null)) 
	                                )", connDB).ExecuteReader();
                        dr.Read();
                        if (dr[0] as double? != null)
                        {
                            txtAvg.Text = (dr[0] as double?).Value.ToString();
                            txtMin.Text = (dr[2] as double?).Value.ToString();
                            txtMax.Text = (dr[1] as double?).Value.ToString();
                        }
                        else
                            txtAvg.Text = txtMin.Text = txtMax.Text = "-";
                        dr.Close();
                    }
                    else
                        txtAvg.Text = txtMin.Text = txtMax.Text = "--";
                }
                else
                    txtAvg.Text = txtMin.Text = txtMax.Text = "--";

                chartPerfMon.ResumeLayout();
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
                            max(CounterID) CounterID, ObjectName, CounterName,
                            case 
                                when ParentName is null then

                                    concat(InstanceName, case when InstanceIndex is not null then concat(' (', InstanceIndex, ')') end)
	                            else
                                    concat(ParentName, case when InstanceIndex is not null then concat(' (', InstanceIndex, ')') end)
                            end InstancePath,
                            convert(int, case when ParentName is not null and ParentName <> InstanceName then InstanceName end) InstanceIndex
                            from counterdetails where MachineName = '" + cmbServers.SelectedItem + @"'
                            group by ObjectName, CounterName, ParentName, InstanceName, InstanceIndex";
            Counters.Load(new SqlCommand(qry,
                                            connDB).ExecuteReader());
            qry = @"select 
                        format(convert(int,format(a.intervaloffset, 'dd')) - 1, '00') + 
                        ':' + 
                        format(a.intervaloffset, 'hh:mm:ss') Duration, StartTime, StopTime 
                    from 
                    (select convert(datetime, max(LogStopTime)) - convert(datetime, min(LogStartTime)) IntervalOffset, 
                            dateadd(mi, min(MinutesToUTC), convert(datetime, min(LogStartTime))) StartTime, 
                            dateadd(mi, min(MinutesToUTC), convert(datetime, max(LogStopTime))) StopTime 
                     from DisplayToID 
                     where GUID in
                     (
                        select distinct GUID from CounterData where CounterID in 
                        (                     
                            select distinct CounterID from CounterDetails 
                            where MachineName = '" + cmbServers.SelectedItem + @"'
                        )
                     )
                    )a";
            dr = new SqlCommand(qry, connDB).ExecuteReader();
            dr.Read();
            txtDur.Text = dr["Duration"] as string;
            trTimeRange.SetRangeLimit((dr["StartTime"] as DateTime?).Value, (dr["StopTime"] as DateTime?).Value);
            trTimeRange.SelectRange((dr["StartTime"] as DateTime?).Value, (dr["StopTime"] as DateTime?).Value);
            chartPerfMon.ChartAreas[0].AxisX.Maximum = (dr["StopTime"] as DateTime?).Value.ToOADate();
            chartPerfMon.ChartAreas[0].AxisX.Maximum = ((dr["StopTime"] as DateTime?).Value).ToOADate();
            dr.Close();

            DgdGrouping_ColumnDisplayIndexChanged(sender, new DataGridViewColumnEventArgs(dgdGrouping.Columns[0]));
        }

        private double ScaleSeries(Series s)
        {
            double max = (double)s.Tag;
            foreach (DataPoint p in s.Points)
                p.YValues[0] = chkAutoScale.Checked ?
                                ((double)p.Tag) * 100 / ((Math.Pow(10, (int)Math.Ceiling(Math.Log10(max))))) :
                                ((double)p.Tag);
            return max;
        }

        private void chkAutoScale_CheckedChanged(object sender, EventArgs e)
        {
            chartPerfMon.SuspendLayout();
            double max = 0;
            foreach (Series s in chartPerfMon.Series)
            {
                if ((double)s.Tag > max)
                    max = (double)s.Tag;
                ScaleSeries(s);
            }
            chartPerfMon.ChartAreas[0].AxisY.LabelStyle.Enabled = !chkAutoScale.Checked;
            chartPerfMon.ChartAreas[0].AxisY.Maximum = chkAutoScale.Checked ? 100 : max;
            chartPerfMon.ChartAreas[0].AxisY.Minimum = 0;
            chartPerfMon.ResumeLayout();
        }

        private void TvCounters_AfterCheck(object sender, TreeViewEventArgs e)
        {
            for (int i = 0; i < CurrentRuleCustomSeries.Count; i++)
                chartPerfMon.Series.Remove(chartPerfMon.Series[CurrentRuleCustomSeries[i]]);
            CurrentRuleCustomSeries.Clear();
            CurrentRuleCustomStripLines.Clear();
            chartPerfMon.ChartAreas[0].AxisY.StripLines.Clear();
            chartPerfMon.Legends[0].CustomItems.Clear();
            chkAutoScale.Enabled = true;
            if (e.Node.Nodes.Count == 0 && e.Node.Parent != null)
            {
                if (e.Node.Checked && e.Node.Nodes.Count == 0)
                {
                    if (!NodesRemaingingToProcessInBatch.Contains(e.Node))
                    {
                        if (ParentNodeOfUpdateBatch == null)
                            ParentNodeOfUpdateBatch = e.Node;
                        else
                            return;
                    }
                    else
                        NodesRemaingingToProcessInBatch.Remove(e.Node);
                        
                    if (NodesRemaingingToProcessInBatch.Count == 0)
                    {
                        new Thread(new ThreadStart(() =>
                        {
                            AddCounters();
                            ParentNodeOfUpdateBatch = null;
                            ;
                        })).Start();
                    }
                }
                else
                {
                    Series s = chartPerfMon.Series.FindByName(e.Node.FullPath);
                    if (s != null)
                    {
                        chartPerfMon.Series.Remove(s);
                        if (!chkAutoScale.Checked)
                            chartPerfMon.ChartAreas[0].AxisY.Maximum = double.NaN;
                        e.Node.SelectedImageIndex = e.Node.ImageIndex = 0;
                        TvCounters_AfterSelect(sender, e);
                    }
                }
            }
            else
            {
                if (e.Node.Checked && NodesRemaingingToProcessInBatch.Count == 0)
                {
                    NodesRemaingingToProcessInBatch = tvCounters.GetLeafNodes(e.Node, false).Where(n=>n.Checked != e.Node.Checked).ToList();
                    if (NodesRemaingingToProcessInBatch.Count > 0)
                    {
                        ParentNodeOfUpdateBatch = e.Node;
                        Program.MainForm.Invoke(new Action(() => DrawingControl.SuspendDrawing(Program.MainForm)));
                    }
                }
            }
            if (chartPerfMon.Series.Count == 0)
                chartPerfMon.ChartAreas[0].AxisY.Maximum = 0;
        }
        
        private void AddCounters()
        {
            AddCounters(new List<KeyValuePair<string, string>>(), null);
        }
        private void AddCounters(List<KeyValuePair<string, string>> HiddenCountersToLookup, Rule rule)
        {
            List<KeyValuePair<string, string>> CountersToUpdate = null;
            if (ParentNodeOfUpdateBatch != null)
                CountersToUpdate = tvCounters.GetLeafNodes(ParentNodeOfUpdateBatch, false)
                                             .Where(n => n.SelectedImageIndex < 1 || n.Checked != ParentNodeOfUpdateBatch.Checked)
                                             .Select(n => new KeyValuePair<string, string>(n.Tag as string, n.FullPath)).ToList();
            else
                CountersToUpdate = tvCounters.GetLeafNodes()
                                             .Where(n => n.SelectedImageIndex < 1 || n.Checked)
                                             .Select(n => new KeyValuePair<string, string>(n.Tag as string, n.FullPath)).ToList();
            CountersToUpdate.AddRange(HiddenCountersToLookup);
            CurrentRuleHiddenSeries.Clear();

            if (CountersToUpdate.Count != 0)
            {

                Random rand = new Random((int)DateTime.Now.Ticks);
                int randomColorOffset = rand.Next(1023);
                int iCurColor = 0;

                List<TreeNode> nodes = tvCounters.GetLeafNodes().Where(n => n.ImageIndex < 1).ToList();
                int iCurNode = 1;

                if (rule == null)
                    StatusFloater.Invoke(new Action(() =>
                    {
                        Form f = Program.MainForm;
                        StatusFloater.Top = f.Top + f.Height / 2 - StatusFloater.Height / 2;
                        StatusFloater.Left = f.Left + f.Width / 2 - StatusFloater.Width / 2;
                        StatusFloater.lblStatus.Text = "Loading data for " + CountersToUpdate.Count + " counters...";
                        StatusFloater.lblSubStatus.Text = "";
                        StatusFloater.lblTime.Text = "00:00";
                        StatusFloater.EscapePressed = false;
                        StatusFloater.AutoUpdateDuration = true;
                        StatusFloater.Visible = true;
                        f.Enabled = false;
                    }));

                string qry = @" select a.*, b.MinutesToUTC, OriginalCounterID from CounterData a, DisplayToID b,
                            (select aa.CounterID as OriginalCounterID, bb.CounterID from (select * from CounterDetails where CounterID in (" + string.Join(", ", CountersToUpdate.Select(c => c.Key)) + @")) aa, CounterDetails bb where 
                                bb.CounterName = aa.CounterName and 
	                            (bb.InstanceName = aa.InstanceName or (bb.InstanceName is null and aa.InstanceName is null)) and 
	                            (bb.InstanceIndex = aa.InstanceIndex or (bb.InstanceIndex is null and aa.InstanceIndex is null)) and 
	                            (bb.ParentName = aa.ParentName or (bb.ParentName is null and aa.ParentName is null)) 
	                            ) c
                            where a.GUID = b.GUID and a.CounterID = c.CounterID 
                             and
                            dateadd(mi, MinutesToUTC, convert(datetime, convert(nvarchar(23), CounterDateTime, 121))) >= convert(datetime, '" + trTimeRange.TotalMinimum.ToString("yyyy-MM-dd HH:mm:ss.fff") + @"', 121) and
                            dateadd(mi, MinutesToUTC, convert(datetime, convert(nvarchar(23), CounterDateTime, 121))) <= convert(datetime, '" + trTimeRange.TotalMaximum.ToString("yyyy-MM-dd HH:mm:ss.fff") + @"', 121)
                            order by OriginalCounterID, CounterDateTime asc";
                SqlDataReader dr = null;
                System.Runtime.CompilerServices.TaskAwaiter<SqlDataReader> tdr = (new SqlCommand(qry, connDB)).ExecuteReaderAsync().GetAwaiter();
                while (!tdr.IsCompleted)
                    Application.DoEvents();
                DataTable dt = new DataTable();
                dr = tdr.GetResult();
                dt.Load(dr);
                dr.Close();

                // parallelize loading as much as possible
                List<Thread> threadList = new List<Thread>();
                int threadcount = Environment.ProcessorCount > 2 ? Environment.ProcessorCount - 2 : 1;
                Semaphore sem = new Semaphore(threadcount, threadcount);

                StatusFloater.EscapePressed = false;

                foreach (KeyValuePair<string, string> counter in CountersToUpdate)
                {
                    threadList.Add(new Thread(new ThreadStart(() =>
                        {
                            sem.WaitOne();
                            if (!StatusFloater.EscapePressed)
                            {
                                Series s = new Series();
                                s.ChartType = SeriesChartType.Line;
                                s.Name = counter.Value;
                                s.XValueType = ChartValueType.DateTime;
                                s.LegendText = counter.Value;

                                s.EmptyPointStyle.BorderWidth = 0;

                                var val = dt.Compute("max([CounterValue])", "OriginalCounterID = " + counter.Key);
                                double max = 0;
                                if (!Convert.IsDBNull(val))
                                    max = (double)val;
                                s.Tag = max;

                                DataRow[] rows = dt.Select("OriginalCounterID = " + counter.Key);

                                foreach (DataRow r in rows)
                                {
                                    double scaledValue = 0;
                                    if (r["CounterValue"] != null)
                                        scaledValue = chkAutoScale.Checked ? (double)r["CounterValue"] / ((Math.Pow(10, (int)Math.Ceiling(Math.Log10(max))))) * 100 : (double)r["CounterValue"];
                                    s.Points.AddXY(DateTime.Parse((r["CounterDateTime"] as string).Trim('\0')).AddMinutes((r["MinutesToUTC"] as int?).Value), scaledValue);
                                    if (r["CounterValue"] == null)
                                        s.Points.Last().IsEmpty = true;
                                    s.Points.Last().Tag = (double?)r["CounterValue"];
                                }

                                TreeNode node = tvCounters.FindNodeByPath(s.Name);
                                if (node != null && tvCounters.SelectedNodes.Contains(node))
                                    s.BorderWidth = 3;
                                Color counterColor;
                                List<RuleCounter> MatchingCountersInRuleWithColor = new List<RuleCounter>();
                                if (rule != null)
                                    MatchingCountersInRuleWithColor = rule.Counters.Where(c => c.Path == s.Name || c.Path == FullPathAlternateHierarchy(s.Name)).ToList();
                                if (rule == null || (MatchingCountersInRuleWithColor.Count() == 0 || (MatchingCountersInRuleWithColor.Count() > 0 && MatchingCountersInRuleWithColor.First().CounterColor == null)))
                                {
                                    int colorIndex = iCurColor++ + randomColorOffset;
                                    if (colorIndex > indexcolors.Length - 1) colorIndex %= (indexcolors.Length - 1);
                                    ColorConverter c = new ColorConverter();
                                    counterColor = (Color)c.ConvertFromString(indexcolors[colorIndex]);
                                }
                                else
                                    counterColor = rule.Counters.Where(c => c.Path == s.Name || c.Path == FullPathAlternateHierarchy(s.Name)).First().CounterColor.Value;
                                if (rule != null && rule.Counters.Where(ctr => ctr.Path == s.Name).Count() > 0 && rule.Counters.Where(ctr => ctr.Path == s.Name).First().HighlightInChart)
                                    s.BorderWidth = 3;
                                s.Color = s.BorderColor = counterColor;
                                    
                                Pen pen = new Pen(s.BorderColor);
                                pen.Width = 6;
                                Bitmap bmp = new Bitmap(16, 16);
                                bmp.Tag = s.Name;
                                using (Graphics g = Graphics.FromImage(bmp))
                                {
                                    g.FillRectangle(Brushes.Transparent, 0, 0, 16, 16);
                                    g.DrawLine(pen, 0, 7, 16, 7);
                                }

                                if (StatusFloater.EscapePressed && rule == null)
                                    Invoke(new Action(() => tvCounters.FindNodeByPath(counter.Value).Checked = false));
                                else
                                {
                                    Invoke(new System.Action(() =>
                                    {
                                        if (rule != null)
                                            chartPerfMon.Legends[0].Enabled = true;
                                        else
                                            chartPerfMon.Legends[0].Enabled = false;
                                        legend.Images.Add(s.Name, bmp);
                                        if (HiddenCountersToLookup.Where(kv => kv.Value == s.Name).Count() > 0)
                                            CurrentRuleHiddenSeries.Add(s);
                                        else
                                        {
                                            if (chartPerfMon.ChartAreas[0].AxisY.Maximum < max)
                                                chartPerfMon.ChartAreas[0].AxisY.Maximum = chkAutoScale.Checked ? 100 : max;
                                            chartPerfMon.ChartAreas[0].RecalculateAxesScale();
                                            node = tvCounters.FindNodeByPath(s.Name);
                                            node.SelectedImageIndex = node.ImageIndex = legend.Images.Count - 1;
                                            if (!chartPerfMon.Series.Select(sr=>sr.Name).Contains(s.Name))
                                                chartPerfMon.Series.Add(s);
                                        }
                                        chartPerfMon.Update();
                                        Application.DoEvents();
                                    }));

                                    if (StatusFloater.Visible && rule == null)
                                        StatusFloater.Invoke(new Action(() =>
                                        {
                                            StatusFloater.lblStatus.Text = "Loaded " + iCurNode + " of " + nodes.Count + " counters...";
                                            StatusFloater.lblSubStatus.Text = "(Esc to cancel)";
                                            StatusFloater.Invalidate(true);
                                            StatusFloater.Update();
                                            Application.DoEvents();
                                        }));
                                    iCurNode++;
                                }
                            }
                            else
                                Invoke(new Action(() => tvCounters.FindNodeByPath(counter.Value).Checked = false));

                            sem.Release();
                        })));
                    threadList.Last().Start();
                }

                foreach (Thread t in threadList)
                    t.Join();

                chartPerfMon.Invoke(new Action(() =>
                    {
                        chartPerfMon.ChartAreas[0].AxisY.Minimum = 0;
                        chartPerfMon.ChartAreas[0].AxisX.Minimum = trTimeRange.RangeMinimum.ToOADate();
                        chartPerfMon.ChartAreas[0].AxisX.Maximum = trTimeRange.RangeMaximum.ToOADate();
                        chartPerfMon.ChartAreas[0].RecalculateAxesScale();
                        chartPerfMon.Update();
                    }));
                if (rule == null)
                    StatusFloater.Invoke(new Action(() =>
                        {
                            StatusFloater.EscapePressed = false;
                            StatusFloater.Visible = false;
                            Program.MainForm.Enabled = true;
                        }));                
                tvCounters.ResumeLayout();
            }

            Program.MainForm.Invoke(new Action(() => DrawingControl.ResumeDrawing(Program.MainForm)));
        }

        public double AddCustomSeries(Series s)
        {
            double max = s.Points.FindMaxByValue().YValues[0];
            s.Tag = max;
            foreach (DataPoint p in s.Points)
                p.YValues[0] = p.YValues[0] ;
            s.LegendText = s.Name;
            CurrentRuleCustomSeries.Add(s.Name);

            chartPerfMon.Series.Add(s);
            return max;
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

        private void RunRule(Rule rule, bool ShouldUIUpdate = true)
        {
            List<KeyValuePair<string, string>> HiddenCountersToLookup = new List<KeyValuePair<string, string>>();

            Invoke(new Action(() =>
            {
                StatusFloater.lblSubStatus.Text = rule.Name;
                StatusFloater.Invalidate();
                Application.DoEvents();
            }));

            if (ShouldUIUpdate)
            {
                Invoke(new Action(() =>
                {
                    chkAutoScale.Checked = false;
                    chkAutoScale.Enabled = false;
                    foreach (TreeNode node in tvCounters.GetLeafNodes())
                        node.Checked = false;
                }));
                tvCounters.AfterCheck -= TvCounters_AfterCheck;
            }

            TreeNode n = null;
            foreach (RuleCounter c in rule.Counters)
            {
                n = tvCounters.FindNodeByPath(c.Path);
                if (n == null)
                    n = tvCounters.FindNodeByPath(FullPathAlternateHierarchy(c.Path));
                if (ShouldUIUpdate)
                {
                    tvCounters.Invoke(new Action(() =>
                    {
                        if (c.ShowInChart)
                            n.Checked = true;
                        else
                            HiddenCountersToLookup.Add(new KeyValuePair<string, string>(n.Tag as string, n.FullPath));
                    }));
                }
                else
                    HiddenCountersToLookup.Add(new KeyValuePair<string, string>(n.Tag as string, n.FullPath));
            }
            if (ShouldUIUpdate)
                tvCounters.AfterCheck += TvCounters_AfterCheck;

            AddCounters(HiddenCountersToLookup, rule);

            foreach (Series s in CurrentRuleHiddenSeries.Concat(chartPerfMon.Series))
            {
                RuleCounter rc = rule.Counters.Where(c => c.Path == s.Name || c.Path == FullPathAlternateHierarchy(s.Name)).FirstOrDefault();
                if (rc != null)
                    rc.ChartSeries = s;
            }

            bool bNeverBeenRunBefore = true;
            if (rule.RuleResult != RuleResultEnum.NotRun)
                bNeverBeenRunBefore = false;

            Invoke(new Action(() => rule.RuleFunction()));

            double max = chartPerfMon.ChartAreas[0].AxisY.Maximum;

            if (bNeverBeenRunBefore)
            {
                string qry = "";
                Invoke(new Action(() => qry = "insert into RuleResults values ('" + rule.Name.Replace("'", "''") + "', '" + cmbServers.SelectedItem + "', " + (int)rule.RuleResult + ", '" + rule.ResultDescription.Replace("'", "''") + "')"));
                new SqlCommand(qry, connDB).ExecuteNonQuery();
            }
            else
            {
                string qry = "";
                Invoke(new Action(() => qry = "update RuleResults set Result = " + (int)rule.RuleResult + ", ResultDescription = '" + rule.ResultDescription + "' where MachineName = '" + cmbServers.SelectedItem + "' and RuleName = '" + rule.Name.Replace("'", "''") + "'"));
                new SqlCommand(qry, connDB).ExecuteNonQuery();
            }

            if (ShouldUIUpdate)
            {
                Invoke(new Action(() =>
                {
                    foreach (Series s in rule.CustomSeries)
                    {
                        double smax = AddCustomSeries(s);
                        if (smax > max)
                            max = smax;
                    }
                    foreach (StripLine s in rule.CustomStripLines)
                    {
                        LegendItem li = new LegendItem(s.Text, s.BackColor, "");
                        chartPerfMon.Legends[0].CustomItems.Add(li);
                        if (s.IntervalOffset > max)
                            max = s.IntervalOffset;
                        CurrentRuleCustomStripLines.Add(s);
                        chartPerfMon.ChartAreas[0].AxisY.StripLines.Add(s);
                    }
                    chartPerfMon.ChartAreas[0].AxisY.Maximum = max * 1.05;
                }));
            }
        }

        private void btnRunRules_Click(object sender, EventArgs e)
        {
            new Thread(new ThreadStart(() =>
            {
                List<DataGridViewRow> RulesToRun = new List<DataGridViewRow>();
                foreach (DataGridViewRow row in dgdRules.Rows)
                    if (row.Visible)
                        if ((row.DataBoundItem as Rule).RuleResult != RuleResultEnum.CountersUnavailable && 
                            (row.DataBoundItem as Rule).Counters[0].ChartSeries == null)
                            RulesToRun.Add(row);
                if (RulesToRun.Count > 0)
                {
                    Form f = Program.MainForm;
                    Invoke(new Action(() =>
                    {
                        chkAutoScale.Checked = false;
                        chkAutoScale.Enabled = false;
                        foreach (TreeNode node in tvCounters.GetLeafNodes())
                            node.Checked = false;
                        f.Enabled = false;
                        StatusFloater.Top = f.Top + f.Height / 2 - StatusFloater.Height / 2;
                        StatusFloater.Left = f.Left + f.Width / 2 - StatusFloater.Width / 2;
                        StatusFloater.lblStatus.Text = "Running " + RulesToRun.Count + " rule" + (RulesToRun.Count > 1 ? "s..." : "...");
                        StatusFloater.lblSubStatus.Text = "";
                        StatusFloater.lblTime.Text = "00:00";
                        StatusFloater.AutoUpdateDuration = true;
                        StatusFloater.Show(f);
                    }));
                    foreach (DataGridViewRow r in RulesToRun)
                        RunRule(r.DataBoundItem as Rule, dgdRules.SelectedRows.Contains(r));
                    Invoke(new Action(() =>
                    {
                        dgdRules_CellClick(dgdRules, new DataGridViewCellEventArgs(0, dgdRules.SelectedRows.Count > 0 ? dgdRules.SelectedRows[0].Index : 0));
                        StatusFloater.Hide();
                        f.Enabled = true;
                    }));
                }
            })).Start();
        }

        public static string FullPathAlternateHierarchy(string Path)
        {
            string[] pathParts = Path.Split('\\');
            if (pathParts.Length == 1)
                return pathParts[0];
            string sAlternatePath = pathParts[0] + "\\";
            for (int i = 1; i < pathParts.Length - 1; i++)
                sAlternatePath += (pathParts[i + 1] + "\\");
            sAlternatePath += (pathParts[1]);
            return sAlternatePath;
        }

        List<Series> CurrentRuleHiddenSeries = new List<Series>();
        List<string> CurrentRuleCustomSeries = new List<string>();
        List<StripLine> CurrentRuleCustomStripLines = new List<StripLine>();
        private void dgdRules_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                if ((dgdRules.Rows[e.RowIndex].DataBoundItem as Rule).RuleResult != RuleResultEnum.CountersUnavailable) new Thread(new ThreadStart(() =>
                {
                    CurrentRuleCustomSeries.Clear();
                    CurrentRuleHiddenSeries.Clear();
                    CurrentRuleCustomStripLines.Clear();
                    Invoke(new Action(() =>
                    {
                        chartPerfMon.Legends[0].CustomItems.Clear();
                        chartPerfMon.ChartAreas[0].AxisY.StripLines.Clear();
                        chartPerfMon.Series.Clear();
                    }));
                    Rule rule = dgdRules.Rows[e.RowIndex].DataBoundItem as Rule;
                    Form f = Program.MainForm;
                    if (rule.Counters[0].ChartSeries == null)
                    {
                        Invoke(new Action(() =>
                        {
                            f.Enabled = false;
                            StatusFloater.Top = f.Top + f.Height / 2 - StatusFloater.Height / 2;
                            StatusFloater.Left = f.Left + f.Width / 2 - StatusFloater.Width / 2;
                            StatusFloater.lblStatus.Text = "Running 1 rule...";
                            StatusFloater.lblSubStatus.Text = "";
                            StatusFloater.lblTime.Text = "00:00";
                            StatusFloater.AutoUpdateDuration = true;
                            StatusFloater.Show(f);
                        }));
                        RunRule(rule);
                    }
                    else
                    {
                        Invoke(new Action(() =>
                        {
                            tvCounters.SuspendLayout();
                            chartPerfMon.SuspendLayout();

                            foreach (TreeNode node in tvCounters.GetLeafNodes())
                            {
                                node.Checked = false;
                                node.ImageIndex = node.SelectedImageIndex = 0;
                            }
                            tvCounters.AfterCheck -= TvCounters_AfterCheck;
                            while (chartPerfMon.Series.Count > 0)
                                chartPerfMon.Series.RemoveAt(0);
                            double max = double.MinValue;
                            foreach (RuleCounter rc in rule.Counters.Where(rc => rc.ShowInChart))
                            {
                                if ((double)rc.ChartSeries.Tag > max)
                                    max = (double)rc.ChartSeries.Tag;
                                ScaleSeries(rc.ChartSeries);
                                TreeNode node = tvCounters.FindNodeByPath(rc.Path);
                                if (tvCounters.SelectedNodes.Contains(node))
                                    rc.ChartSeries.BorderWidth = 3;
                                else
                                    rc.ChartSeries.BorderWidth = 1;
                                chartPerfMon.Series.Add(rc.ChartSeries);
                                if (node == null) node = tvCounters.FindNodeByPath(FullPathAlternateHierarchy(rc.Path));
                                node.ImageIndex = node.SelectedImageIndex = legend.Images.IndexOfKey(rc.Path);
                                node.Checked = true;
                            }
                            foreach (Series s in rule.CustomSeries)
                            {
                                if ((double)s.Tag > max)
                                    max = (double)s.Tag;
                                ScaleSeries(s);
                                CurrentRuleCustomSeries.Add(s.Name);
                                chartPerfMon.Series.Add(s);
                            }
                            foreach (StripLine s in rule.CustomStripLines)
                            {
                                if (s.IntervalOffset > max)
                                    max = s.IntervalOffset;
                                LegendItem li = new LegendItem(s.Text, s.BackColor, "");
                                chartPerfMon.Legends[0].CustomItems.Add(li);
                                chartPerfMon.ChartAreas[0].AxisY.StripLines.Add(s);
                                CurrentRuleCustomStripLines.Add(s);
                            }
                            chartPerfMon.ChartAreas[0].AxisY.Maximum = max * 1.05;
                            chartPerfMon.ChartAreas[0].AxisY.Minimum = 0;
                            chartPerfMon.Legends[0].Enabled = true;
                            chartPerfMon.ChartAreas[0].RecalculateAxesScale();
                            tvCounters.AfterCheck += TvCounters_AfterCheck;
                            chartPerfMon.ResumeLayout();
                            tvCounters.ResumeLayout();
                        }));
                    }
                    Invoke(new Action(() =>
                    {
                        chkAutoScale.Checked = false;
                        chkAutoScale.Enabled = false;
                        StatusFloater.AutoUpdateDuration = false;
                        StatusFloater.Visible = false;
                        f.Enabled = true;
                    }));
                })).Start();
            }
        }

        class DrawingControl
        {
            [DllImport("user32.dll")]
            public static extern int SendMessage(IntPtr hWnd, Int32 wMsg, bool wParam, Int32 lParam);

            private const int WM_SETREDRAW = 11;

            public static void SuspendDrawing(Control parent)
            {
                SendMessage(parent.Handle, WM_SETREDRAW, false, 0);
            }

            public static void ResumeDrawing(Control parent)
            {
                SendMessage(parent.Handle, WM_SETREDRAW, true, 0);
                parent.Refresh();
            }
        }

        private void SummaryTextBoxes_Enter(object sender, MouseEventArgs e)
        {
            (sender as TextBox).SelectAll();
        }

        private void btnAddRule_Click(object sender, EventArgs e)
        {
            frmAddPerfMonRule AddPerfMon = new frmAddPerfMonRule();
            if (AddPerfMon.ShowDialog(Program.MainForm) == DialogResult.OK)
                LoadRuleFromRegistry(AddPerfMon.txtName.Text);
        }

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

        private void btnEditRule_Click(object sender, EventArgs e)
        {
            Rule r = dgdRules.SelectedRows[0].DataBoundItem as Rule;
            frmAddPerfMonRule f = new frmAddPerfMonRule(r);
            DialogResult res = f.ShowDialog();
            if (res == DialogResult.OK)
                LoadRuleFromRegistry(f.txtName.Text);
            if (res == DialogResult.Ignore)
            {
                Rules.Clear();
                LoadRulesFromRegistry();
            }             
        }

        private void dgdRules_SelectionChanged(object sender, EventArgs e)
        {
            btnDeleteRule.Enabled = btnEditRule.Enabled = dgdRules.SelectedRows.Count == 1;
        }

        private void btnDeleteRule_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "Delete rule '" + dgdRules.CurrentRow.Cells[1].Value as string + "'?", "Delete Rule", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                RegistryKey k = Registry.LocalMachine.OpenSubKey("SOFTWARE\\SSASDiag\\PerfMonRules\\", RegistryKeyPermissionCheck.ReadWriteSubTree);
                k.DeleteSubKeyTree(dgdRules.CurrentRow.Cells[1].Value as string);
                Rules.Clear();
                LoadRulesFromRegistry();
            }
        }

        private void cmbRuleFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (dgdRules.DataSource != null)
            {
                CurrencyManager cm = (CurrencyManager)BindingContext[dgdRules.DataSource];
                cm.SuspendBinding();
                foreach (DataGridViewRow r in dgdRules.Rows)
                    if (cmbRuleFilter.SelectedIndex == 0)
                        r.Visible = true;
                    else
                        r.Visible = r.Cells["Category"].Value as string == cmbRuleFilter.SelectedItem as string;
                cm.ResumeBinding();
            }
        }

        private static int KEY_READ = 131097;
        private static UIntPtr HKEY_LOCAL_MACHINE = new UIntPtr(0x80000002u);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern int RegCloseKey(UIntPtr hKey);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern int RegOpenKeyEx(UIntPtr hKey, string subKey, int ulOptions, int samDesired, out UIntPtr hkResult);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern uint RegSaveKey(UIntPtr hKey, string lpFile, IntPtr lpSecurityAttributes);

        private void btnExport_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Choose a folder to export rules. A new top level folder is create with subfolders for each category.";
            fbd.RootFolder = Environment.SpecialFolder.Desktop;
            fbd.SelectedPath = Program.MainForm.txtSaveLocation.Text;
            if (fbd.ShowDialog(this) == DialogResult.OK)
            {
                Directory.CreateDirectory(fbd.SelectedPath + "\\PerfMonRules");
                UIntPtr res;
                if (cmbRuleFilter.SelectedIndex == 0)
                    foreach(string cat in Rules.Select(rl=>rl.Category).Distinct())
                        Directory.CreateDirectory(fbd.SelectedPath + "\\PerfMonRules\\" + cat);
                else
                    Directory.CreateDirectory(fbd.SelectedPath + "\\PerfMonRules\\" + cmbRuleFilter.SelectedItem);
                foreach (DataGridViewRow row in dgdRules.Rows)
                {
                    if (row.Visible)
                    {
                        Rule r = row.DataBoundItem as Rule;
                        RegOpenKeyEx(HKEY_LOCAL_MACHINE, "SOFTWARE\\SSASDiag\\PerfMonRules\\" + r.Name, 0, KEY_READ, out res);
                        RegSaveKey(res, fbd.SelectedPath + "\\PerfMonRules\\" + r.Category + "\\" + r.Name + ".reg", IntPtr.Zero);
                        RegCloseKey(res);
                    }
                }
            }
        }

        public class RuleExpression
        {
            public RuleExpression(string Name, string Expression, int Index, bool Display = false, bool Highlight = false, bool Include_TotalSeriesInWildcard = true)
            {
                this.Name = Name; this.Expression = Expression; this.Index = Index; this.Display = Display; this.Highlight = Highlight; this.Include_TotalSeriesInWildcard = Include_TotalSeriesInWildcard;
            }

            public string Name;
            public string Expression;
            public bool Display;
            public bool Highlight;
            public double Value;
            public int Index;
            public bool Include_TotalSeriesInWildcard;
        }

        public class SortableBindingList<T> : BindingList<T> where T : class
        {
            private bool _isSorted;
            private ListSortDirection _sortDirection = ListSortDirection.Ascending;
            private PropertyDescriptor _sortProperty;

            /// <summary>
            /// Initializes a new instance of the <see cref="SortableBindingList{T}"/> class.
            /// </summary>
            public SortableBindingList()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="SortableBindingList{T}"/> class.
            /// </summary>
            /// <param name="list">An <see cref="T:System.Collections.Generic.IList`1" /> of items to be contained in the <see cref="T:System.ComponentModel.BindingList`1" />.</param>
            public SortableBindingList(IList<T> list)
                : base(list)
            {
            }

            /// <summary>
            /// Gets a value indicating whether the list supports sorting.
            /// </summary>
            protected override bool SupportsSortingCore
            {
                get { return true; }
            }

            /// <summary>
            /// Gets a value indicating whether the list is sorted.
            /// </summary>
            protected override bool IsSortedCore
            {
                get { return _isSorted; }
            }

            /// <summary>
            /// Gets the direction the list is sorted.
            /// </summary>
            protected override ListSortDirection SortDirectionCore
            {
                get { return _sortDirection; }
            }

            /// <summary>
            /// Gets the property descriptor that is used for sorting the list if sorting is implemented in a derived class; otherwise, returns null
            /// </summary>
            protected override PropertyDescriptor SortPropertyCore
            {
                get { return _sortProperty; }
            }

            /// <summary>
            /// Removes any sort applied with ApplySortCore if sorting is implemented
            /// </summary>
            protected override void RemoveSortCore()
            {
                _sortDirection = ListSortDirection.Ascending;
                _sortProperty = null;
                _isSorted = false; //thanks Luca
            }

            /// <summary>
            /// Sorts the items if overridden in a derived class
            /// </summary>
            /// <param name="prop"></param>
            /// <param name="direction"></param>
            protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
            {
                _sortProperty = prop;
                _sortDirection = direction;

                List<T> list = Items as List<T>;
                if (list == null) return;

                list.Sort(Compare);

                _isSorted = true;
                //fire an event that the list has been changed.
                OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
            }


            private int Compare(T lhs, T rhs)
            {
                var result = OnComparison(lhs, rhs);
                //invert if descending
                if (_sortDirection == ListSortDirection.Descending)
                    result = -result;
                return result;
            }

            private int OnComparison(T lhs, T rhs)
            {
                object lhsValue = lhs == null ? null : _sortProperty.GetValue(lhs);
                object rhsValue = rhs == null ? null : _sortProperty.GetValue(rhs);
                if (lhsValue == null)
                {
                    return (rhsValue == null) ? 0 : -1; //nulls are equal
                }
                if (rhsValue == null)
                {
                    return 1; //first has value, second doesn't
                }
                if (lhsValue is IComparable)
                {
                    return ((IComparable)lhsValue).CompareTo(rhsValue);
                }
                if (lhsValue.Equals(rhsValue))
                {
                    return 0; //both are the same
                }
                //not comparable, compare ToString
                return lhsValue.ToString().CompareTo(rhsValue.ToString());
            }
        }

        public string[] indexcolors = new string[]{
        "#000000", "#FFFF00", "#1CE6FF", "#FF34FF", "#FF4A46", "#008941", "#006FA6", "#A30059",
        "#FFDBE5", "#7A4900", "#0000A6", "#63FFAC", "#B79762", "#004D43", "#8FB0FF", "#997D87",
        "#5A0007", "#809693", "#FEFFE6", "#1B4400", "#4FC601", "#3B5DFF", "#4A3B53", "#FF2F80",
        "#61615A", "#BA0900", "#6B7900", "#00C2A0", "#FFAA92", "#FF90C9", "#B903AA", "#D16100",
        "#DDEFFF", "#000035", "#7B4F4B", "#A1C299", "#300018", "#0AA6D8", "#013349", "#00846F",
        "#372101", "#FFB500", "#C2FFED", "#A079BF", "#CC0744", "#C0B9B2", "#C2FF99", "#001E09",
        "#00489C", "#6F0062", "#0CBD66", "#EEC3FF", "#456D75", "#B77B68", "#7A87A1", "#788D66",
        "#885578", "#FAD09F", "#FF8A9A", "#D157A0", "#BEC459", "#456648", "#0086ED", "#886F4C",
        "#34362D", "#B4A8BD", "#00A6AA", "#452C2C", "#636375", "#A3C8C9", "#FF913F", "#938A81",
        "#575329", "#00FECF", "#B05B6F", "#8CD0FF", "#3B9700", "#04F757", "#C8A1A1", "#1E6E00",
        "#7900D7", "#A77500", "#6367A9", "#A05837", "#6B002C", "#772600", "#D790FF", "#9B9700",
        "#549E79", "#FFF69F", "#201625", "#72418F", "#BC23FF", "#99ADC0", "#3A2465", "#922329",
        "#5B4534", "#FDE8DC", "#404E55", "#0089A3", "#CB7E98", "#A4E804", "#324E72", "#6A3A4C",
        "#83AB58", "#001C1E", "#D1F7CE", "#004B28", "#C8D0F6", "#A3A489", "#806C66", "#222800",
        "#BF5650", "#E83000", "#66796D", "#DA007C", "#FF1A59", "#8ADBB4", "#1E0200", "#5B4E51",
        "#C895C5", "#320033", "#FF6832", "#66E1D3", "#CFCDAC", "#D0AC94", "#7ED379", "#012C58",
        "#7A7BFF", "#D68E01", "#353339", "#78AFA1", "#FEB2C6", "#75797C", "#837393", "#943A4D",
        "#B5F4FF", "#D2DCD5", "#9556BD", "#6A714A", "#001325", "#02525F", "#0AA3F7", "#E98176",
        "#DBD5DD", "#5EBCD1", "#3D4F44", "#7E6405", "#02684E", "#962B75", "#8D8546", "#9695C5",
        "#E773CE", "#D86A78", "#3E89BE", "#CA834E", "#518A87", "#5B113C", "#55813B", "#E704C4",
        "#00005F", "#A97399", "#4B8160", "#59738A", "#FF5DA7", "#F7C9BF", "#643127", "#513A01",
        "#6B94AA", "#51A058", "#A45B02", "#1D1702", "#E20027", "#E7AB63", "#4C6001", "#9C6966",
        "#64547B", "#97979E", "#006A66", "#391406", "#F4D749", "#0045D2", "#006C31", "#DDB6D0",
        "#7C6571", "#9FB2A4", "#00D891", "#15A08A", "#BC65E9", "#FFFFFE", "#C6DC99", "#203B3C",
        "#671190", "#6B3A64", "#F5E1FF", "#FFA0F2", "#CCAA35", "#374527", "#8BB400", "#797868",
        "#C6005A", "#3B000A", "#C86240", "#29607C", "#402334", "#7D5A44", "#CCB87C", "#B88183",
        "#AA5199", "#B5D6C3", "#A38469", "#9F94F0", "#A74571", "#B894A6", "#71BB8C", "#00B433",
        "#789EC9", "#6D80BA", "#953F00", "#5EFF03", "#E4FFFC", "#1BE177", "#BCB1E5", "#76912F",
        "#003109", "#0060CD", "#D20096", "#895563", "#29201D", "#5B3213", "#A76F42", "#89412E",
        "#1A3A2A", "#494B5A", "#A88C85", "#F4ABAA", "#A3F3AB", "#00C6C8", "#EA8B66", "#958A9F",
        "#BDC9D2", "#9FA064", "#BE4700", "#658188", "#83A485", "#453C23", "#47675D", "#3A3F00",
        "#061203", "#DFFB71", "#868E7E", "#98D058", "#6C8F7D", "#D7BFC2", "#3C3E6E", "#D83D66",
        "#2F5D9B", "#6C5E46", "#D25B88", "#5B656C", "#00B57F", "#545C46", "#866097", "#365D25",
        "#252F99", "#00CCFF", "#674E60", "#FC009C", "#92896B", "#1E2324", "#DEC9B2", "#9D4948",
        "#85ABB4", "#342142", "#D09685", "#A4ACAC", "#00FFFF", "#AE9C86", "#742A33", "#0E72C5",
        "#AFD8EC", "#C064B9", "#91028C", "#FEEDBF", "#FFB789", "#9CB8E4", "#AFFFD1", "#2A364C",
        "#4F4A43", "#647095", "#34BBFF", "#807781", "#920003", "#B3A5A7", "#018615", "#F1FFC8",
        "#976F5C", "#FF3BC1", "#FF5F6B", "#077D84", "#F56D93", "#5771DA", "#4E1E2A", "#830055",
        "#02D346", "#BE452D", "#00905E", "#BE0028", "#6E96E3", "#007699", "#FEC96D", "#9C6A7D",
        "#3FA1B8", "#893DE3", "#79B4D6", "#7FD4D9", "#6751BB", "#B28D2D", "#E27A05", "#DD9CB8",
        "#AABC7A", "#980034", "#561A02", "#8F7F00", "#635000", "#CD7DAE", "#8A5E2D", "#FFB3E1",
        "#6B6466", "#C6D300", "#0100E2", "#88EC69", "#8FCCBE", "#21001C", "#511F4D", "#E3F6E3",
        "#FF8EB1", "#6B4F29", "#A37F46", "#6A5950", "#1F2A1A", "#04784D", "#101835", "#E6E0D0",
        "#FF74FE", "#00A45F", "#8F5DF8", "#4B0059", "#412F23", "#D8939E", "#DB9D72", "#604143",
        "#B5BACE", "#989EB7", "#D2C4DB", "#A587AF", "#77D796", "#7F8C94", "#FF9B03", "#555196",
        "#31DDAE", "#74B671", "#802647", "#2A373F", "#014A68", "#696628", "#4C7B6D", "#002C27",
        "#7A4522", "#3B5859", "#E5D381", "#FFF3FF", "#679FA0", "#261300", "#2C5742", "#9131AF",
        "#AF5D88", "#C7706A", "#61AB1F", "#8CF2D4", "#C5D9B8", "#9FFFFB", "#BF45CC", "#493941",
        "#863B60", "#B90076", "#003177", "#C582D2", "#C1B394", "#602B70", "#887868", "#BABFB0",
        "#030012", "#D1ACFE", "#7FDEFE", "#4B5C71", "#A3A097", "#E66D53", "#637B5D", "#92BEA5",
        "#00F8B3", "#BEDDFF", "#3DB5A7", "#DD3248", "#B6E4DE", "#427745", "#598C5A", "#B94C59",
        "#8181D5", "#94888B", "#FED6BD", "#536D31", "#6EFF92", "#E4E8FF", "#20E200", "#FFD0F2",
        "#4C83A1", "#BD7322", "#915C4E", "#8C4787", "#025117", "#A2AA45", "#2D1B21", "#A9DDB0",
        "#FF4F78", "#528500", "#009A2E", "#17FCE4", "#71555A", "#525D82", "#00195A", "#967874",
        "#555558", "#0B212C", "#1E202B", "#EFBFC4", "#6F9755", "#6F7586", "#501D1D", "#372D00",
        "#741D16", "#5EB393", "#B5B400", "#DD4A38", "#363DFF", "#AD6552", "#6635AF", "#836BBA",
        "#98AA7F", "#464836", "#322C3E", "#7CB9BA", "#5B6965", "#707D3D", "#7A001D", "#6E4636",
        "#443A38", "#AE81FF", "#489079", "#897334", "#009087", "#DA713C", "#361618", "#FF6F01",
        "#006679", "#370E77", "#4B3A83", "#C9E2E6", "#C44170", "#FF4526", "#73BE54", "#C4DF72",
        "#ADFF60", "#00447D", "#DCCEC9", "#BD9479", "#656E5B", "#EC5200", "#FF6EC2", "#7A617E",
        "#DDAEA2", "#77837F", "#A53327", "#608EFF", "#B599D7", "#A50149", "#4E0025", "#C9B1A9",
        "#03919A", "#1B2A25", "#E500F1", "#982E0B", "#B67180", "#E05859", "#006039", "#578F9B",
        "#305230", "#CE934C", "#B3C2BE", "#C0BAC0", "#B506D3", "#170C10", "#4C534F", "#224451",
        "#3E4141", "#78726D", "#B6602B", "#200441", "#DDB588", "#497200", "#C5AAB6", "#033C61",
        "#71B2F5", "#A9E088", "#4979B0", "#A2C3DF", "#784149", "#2D2B17", "#3E0E2F", "#57344C",
        "#0091BE", "#E451D1", "#4B4B6A", "#5C011A", "#7C8060", "#FF9491", "#4C325D", "#005C8B",
        "#E5FDA4", "#68D1B6", "#032641", "#140023", "#8683A9", "#CFFF00", "#A72C3E", "#34475A",
        "#B1BB9A", "#B4A04F", "#8D918E", "#A168A6", "#813D3A", "#425218", "#DA8386", "#776133",
        "#563930", "#8498AE", "#90C1D3", "#B5666B", "#9B585E", "#856465", "#AD7C90", "#E2BC00",
        "#E3AAE0", "#B2C2FE", "#FD0039", "#009B75", "#FFF46D", "#E87EAC", "#DFE3E6", "#848590",
        "#AA9297", "#83A193", "#577977", "#3E7158", "#C64289", "#EA0072", "#C4A8CB", "#55C899",
        "#E78FCF", "#004547", "#F6E2E3", "#966716", "#378FDB", "#435E6A", "#DA0004", "#1B000F",
        "#5B9C8F", "#6E2B52", "#011115", "#E3E8C4", "#AE3B85", "#EA1CA9", "#FF9E6B", "#457D8B",
        "#92678B", "#00CDBB", "#9CCC04", "#002E38", "#96C57F", "#CFF6B4", "#492818", "#766E52",
        "#20370E", "#E3D19F", "#2E3C30", "#B2EACE", "#F3BDA4", "#A24E3D", "#976FD9", "#8C9FA8",
        "#7C2B73", "#4E5F37", "#5D5462", "#90956F", "#6AA776", "#DBCBF6", "#DA71FF", "#987C95",
        "#52323C", "#BB3C42", "#584D39", "#4FC15F", "#A2B9C1", "#79DB21", "#1D5958", "#BD744E",
        "#160B00", "#20221A", "#6B8295", "#00E0E4", "#102401", "#1B782A", "#DAA9B5", "#B0415D",
        "#859253", "#97A094", "#06E3C4", "#47688C", "#7C6755", "#075C00", "#7560D5", "#7D9F00",
        "#C36D96", "#4D913E", "#5F4276", "#FCE4C8", "#303052", "#4F381B", "#E5A532", "#706690",
        "#AA9A92", "#237363", "#73013E", "#FF9079", "#A79A74", "#029BDB", "#FF0169", "#C7D2E7",
        "#CA8869", "#80FFCD", "#BB1F69", "#90B0AB", "#7D74A9", "#FCC7DB", "#99375B", "#00AB4D",
        "#ABAED1", "#BE9D91", "#E6E5A7", "#332C22", "#DD587B", "#F5FFF7", "#5D3033", "#6D3800",
        "#FF0020", "#B57BB3", "#D7FFE6", "#C535A9", "#260009", "#6A8781", "#A8ABB4", "#D45262",
        "#794B61", "#4621B2", "#8DA4DB", "#C7C890", "#6FE9AD", "#A243A7", "#B2B081", "#181B00",
        "#286154", "#4CA43B", "#6A9573", "#A8441D", "#5C727B", "#738671", "#D0CFCB", "#897B77",
        "#1F3F22", "#4145A7", "#DA9894", "#A1757A", "#63243C", "#ADAAFF", "#00CDE2", "#DDBC62",
        "#698EB1", "#208462", "#00B7E0", "#614A44", "#9BBB57", "#7A5C54", "#857A50", "#766B7E",
        "#014833", "#FF8347", "#7A8EBA", "#274740", "#946444", "#EBD8E6", "#646241", "#373917",
        "#6AD450", "#81817B", "#D499E3", "#979440", "#011A12", "#526554", "#B5885C", "#A499A5",
        "#03AD89", "#B3008B", "#E3C4B5", "#96531F", "#867175", "#74569E", "#617D9F", "#E70452",
        "#067EAF", "#A697B6", "#B787A8", "#9CFF93", "#311D19", "#3A9459", "#6E746E", "#B0C5AE",
        "#84EDF7", "#ED3488", "#754C78", "#384644", "#C7847B", "#00B6C5", "#7FA670", "#C1AF9E",
        "#2A7FFF", "#72A58C", "#FFC07F", "#9DEBDD", "#D97C8E", "#7E7C93", "#62E674", "#B5639E",
        "#FFA861", "#C2A580", "#8D9C83", "#B70546", "#372B2E", "#0098FF", "#985975", "#20204C",
        "#FF6C60", "#445083", "#8502AA", "#72361F", "#9676A3", "#484449", "#CED6C2", "#3B164A",
        "#CCA763", "#2C7F77", "#02227B", "#A37E6F", "#CDE6DC", "#CDFFFB", "#BE811A", "#F77183",
        "#EDE6E2", "#CDC6B4", "#FFE09E", "#3A7271", "#FF7B59", "#4E4E01", "#4AC684", "#8BC891",
        "#BC8A96", "#CF6353", "#DCDE5C", "#5EAADD", "#F6A0AD", "#E269AA", "#A3DAE4", "#436E83",
        "#002E17", "#ECFBFF", "#A1C2B6", "#50003F", "#71695B", "#67C4BB", "#536EFF", "#5D5A48",
        "#890039", "#969381", "#371521", "#5E4665", "#AA62C3", "#8D6F81", "#2C6135", "#410601",
        "#564620", "#E69034", "#6DA6BD", "#E58E56", "#E3A68B", "#48B176", "#D27D67", "#B5B268",
        "#7F8427", "#FF84E6", "#435740", "#EAE408", "#F4F5FF", "#325800", "#4B6BA5", "#ADCEFF",
        "#9B8ACC", "#885138", "#5875C1", "#7E7311", "#FEA5CA", "#9F8B5B", "#A55B54", "#89006A",
        "#AF756F", "#2A2000", "#7499A1", "#FFB550", "#00011E", "#D1511C", "#688151", "#BC908A",
        "#78C8EB", "#8502FF", "#483D30", "#C42221", "#5EA7FF", "#785715", "#0CEA91", "#FFFAED",
        "#B3AF9D", "#3E3D52", "#5A9BC2", "#9C2F90", "#8D5700", "#ADD79C", "#00768B", "#337D00",
        "#C59700", "#3156DC", "#944575", "#ECFFDC", "#D24CB2", "#97703C", "#4C257F", "#9E0366",
        "#88FFEC", "#B56481", "#396D2B", "#56735F", "#988376", "#9BB195", "#A9795C", "#E4C5D3",
        "#9F4F67", "#1E2B39", "#664327", "#AFCE78", "#322EDF", "#86B487", "#C23000", "#ABE86B",
        "#96656D", "#250E35", "#A60019", "#0080CF", "#CAEFFF", "#323F61", "#A449DC", "#6A9D3B",
        "#FF5AE4", "#636A01", "#D16CDA", "#736060", "#FFBAAD", "#D369B4", "#FFDED6", "#6C6D74",
        "#927D5E", "#845D70", "#5B62C1", "#2F4A36", "#E45F35", "#FF3B53", "#AC84DD", "#762988",
        "#70EC98", "#408543", "#2C3533", "#2E182D", "#323925", "#19181B", "#2F2E2C", "#023C32",
        "#9B9EE2", "#58AFAD", "#5C424D", "#7AC5A6", "#685D75", "#B9BCBD", "#834357", "#1A7B42",
        "#2E57AA", "#E55199", "#316E47", "#CD00C5", "#6A004D", "#7FBBEC", "#F35691", "#D7C54A",
        "#62ACB7", "#CBA1BC", "#A28A9A", "#6C3F3B", "#FFE47D", "#DCBAE3", "#5F816D", "#3A404A",
        "#7DBF32", "#E6ECDC", "#852C19", "#285366", "#B8CB9C", "#0E0D00", "#4B5D56", "#6B543F",
        "#E27172", "#0568EC", "#2EB500", "#D21656", "#EFAFFF", "#682021", "#2D2011", "#DA4CFF",
        "#70968E", "#FF7B7D", "#4A1930", "#E8C282", "#E7DBBC", "#A68486", "#1F263C", "#36574E",
        "#52CE79", "#ADAAA9", "#8A9F45", "#6542D2", "#00FB8C", "#5D697B", "#CCD27F", "#94A5A1",
        "#790229", "#E383E6", "#7EA4C1", "#4E4452", "#4B2C00", "#620B70", "#314C1E", "#874AA6",
        "#E30091", "#66460A", "#EB9A8B", "#EAC3A3", "#98EAB3", "#AB9180", "#B8552F", "#1A2B2F",
        "#94DDC5", "#9D8C76", "#9C8333", "#94A9C9", "#392935", "#8C675E", "#CCE93A", "#917100",
        "#01400B", "#449896", "#1CA370", "#E08DA7", "#8B4A4E", "#667776", "#4692AD", "#67BDA8",
        "#69255C", "#D3BFFF", "#4A5132", "#7E9285", "#77733C", "#E7A0CC", "#51A288", "#2C656A",
        "#4D5C5E", "#C9403A", "#DDD7F3", "#005844", "#B4A200", "#488F69", "#858182", "#D4E9B9",
        "#3D7397", "#CAE8CE", "#D60034", "#AA6746", "#9E5585", "#BA6200"};
    }
}
