using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.AccessControl;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;

namespace SSASDiag
{
    public partial class frmSSASDiag : Form
    {
        #region ProfilerAnalysisLocals

        bool bCancelProfilerImport = false;
        Process ASProfilerTraceImporterProcess;       
        bool bProfilerEventClassSublcassViewPresent = false, 
             bProfilerTraceDbAttached = false;
        SqlConnection connSqlDb = new SqlConnection();

        #endregion ProfilerAnalysisLocals

        #region TraceLoader

        private void btnImportProfilerTrace_Click(object sender, EventArgs e)
        {
            if (btnImportProfilerTrace.Text == "Import and &Analyze")
            {
                bCancelProfilerImport = false;
                tbCollection.ForeColor = SystemColors.ControlDark;
                tcCollectionAnalysisTabs.Refresh();
                tbCollection.Enabled = false;
                btnAnalysisFolder.Enabled = false;
                btnImportProfilerTrace.Text = "&Cancel Import";
                LogFeatureUse("Profiler Analysis", "Started");
                if (!ValidateProfilerTraceDBConnectionStatus())
                {
                    tbCollection.ForeColor = SystemColors.ControlText;
                    tcCollectionAnalysisTabs.Refresh();
                    tbCollection.Enabled = true;
                    btnAnalysisFolder.Enabled = true;
                    btnImportProfilerTrace.Enabled = true;
                    btnImportProfilerTrace.Text = "Import and &Analyze";
                    return;
                }
                AnalysisMessagePumpTimer.Interval = 1000;
                AnalysisMessagePumpTimer.Start();
                try
                {
                    BackgroundWorker bg = new BackgroundWorker();
                    bg.DoWork += bgImportProfilerTrace;
                    bg.RunWorkerCompleted += bgImportProfilerTraceComplete;
                    bg.RunWorkerAsync(new object[] { ProfilerTraceStatusTextBox, "Initial Catalog=" + AnalysisTraceID + ";Persist Security Info=False;" + connSqlDb.ConnectionString });
                }
                catch (SqlException ex)
                {
                    LogException(ex);
                    if (ex.Message == "ExecuteNonQuery requires an open and available Connection. The connection's current state is closed.")
                    {
                        ProfilerTraceStatusTextBox.Text = "Trace file is not yet imported to database table for analysis.  No SQL Server was available to perform import.";
                        btnImportProfilerTrace.Visible = true;
                    }
                    else
                        ProfilerTraceStatusTextBox.Text = "Error loading profiler trace: \r\n" + ex.Message;

                    ProfilerTraceStatusTextBox.AppendText("\r\nImport trace to database failed.  Dropping trace database...");
                    LogFeatureUse("Profiler Analysis", "Failed: " + ex.Message);
                    BackgroundWorker bgCancelTrace = new BackgroundWorker();
                    bgCancelTrace.DoWork += BgCancelTrace_DoWork;
                    bgCancelTrace.RunWorkerCompleted += BgCancelTrace_RunWorkerCompleted;
                    bgCancelTrace.RunWorkerAsync();
                }
            }
            else
            {
                bCancelProfilerImport = true;
                btnImportProfilerTrace.Enabled = false;
                ProfilerTraceStatusTextBox.AppendText("\r\nUser cancelled loading of trace to table.  Dropping trace database...");
                LogFeatureUse("Profiler Analysis", "User cancelled import");
                BackgroundWorker bgCancelTrace = new BackgroundWorker();
                bgCancelTrace.DoWork += BgCancelTrace_DoWork;
                bgCancelTrace.RunWorkerCompleted += BgCancelTrace_RunWorkerCompleted;
                bgCancelTrace.RunWorkerAsync();
            }
        }

        public void SetAnalysisFolderPermissionsAndCreateDB(string path)
        {
            if (File.Exists(path))
                path = path.Substring(0, path.LastIndexOf("\\"));

            if (!File.Exists(path) && !Directory.Exists(path))
                Directory.CreateDirectory(path);
            if (!Directory.Exists(path + "\\Analysis"))
                Directory.CreateDirectory(path + "\\Analysis");

            string sSvcUser = "";
            ServiceController[] services = ServiceController.GetServices();
            string instance = "";
            if (connSqlDb.DataSource.ToUpper() == Environment.MachineName.ToUpper())
                instance = "MSSQLSERVER";
            else
                instance = (connSqlDb.DataSource.Contains("\\") ? connSqlDb.DataSource.Substring(connSqlDb.DataSource.IndexOf("\\") + 1) : "MSSQLSERVER");

            foreach (ServiceController s in services.OrderBy(ob => ob.DisplayName))
                if (s.DisplayName.Contains("SQL Server (" + instance + ")"))
                {
                    SelectQuery sQuery = new SelectQuery("select name, startname, pathname from Win32_Service where name = \"" + s.ServiceName + "\"");
                    ManagementObjectSearcher mgmtSearcher = new ManagementObjectSearcher(sQuery);

                    foreach (ManagementObject svc in mgmtSearcher.Get())
                        sSvcUser = svc["startname"] as string;
                    if (sSvcUser.Contains(".")) sSvcUser = sSvcUser.Replace(".", Environment.UserDomainName);
                    if (sSvcUser == "LocalSystem") sSvcUser = "NT AUTHORITY\\SYSTEM";
                }

            DirectoryInfo dirInfo = new DirectoryInfo(path + "\\Analysis");
            DirectorySecurity dirSec = dirInfo.GetAccessControl();
            dirSec.AddAccessRule(new FileSystemAccessRule(sSvcUser, FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.None, AccessControlType.Allow));
            dirInfo.SetAccessControl(dirSec);

            if (AnalysisTraceID == "")
                AnalysisTraceID = path.Substring(path.LastIndexOf("\\") + 1).Replace("_SSASDiagOutput", "_SSASDiag");
            SqlCommand cmd = new SqlCommand(Properties.Resources.CreateDBSQLScript.
                                Replace("<mdfpath/>", (path + "\\Analysis\\" + AnalysisTraceID + ".mdf").Replace("'", "''")).
                                Replace("<ldfpath/>", (path + "\\Analysis\\" + AnalysisTraceID + ".ldf").Replace("'", "''")).
                                Replace("<dbname/>", AnalysisTraceID)
                                , connSqlDb);
            int ret = cmd.ExecuteNonQuery();
        }
        private void bgImportProfilerTrace(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (m_analysisPath.EndsWith(".trc"))
                    m_analysisPath = m_analysisPath.Substring(0, m_analysisPath.LastIndexOf("\\") + 1);

                SetAnalysisFolderPermissionsAndCreateDB(m_analysisPath);

                ProfilerTraceStatusTextBox.Invoke(new System.Action(() =>
                    ProfilerTraceStatusTextBox.Text = "Importing profiler trace to database [" + AnalysisTraceID + "] on SQL instance: [" + (connSqlDb.DataSource == "." ? Environment.MachineName : connSqlDb.DataSource) + "]."));
                string connstr = (e.Argument as object[])[1] as string;
                if (!connstr.Contains("Initial Catalog")) connstr += (connstr.EndsWith(";") ? "" : ";") + "Initial Catalog=" + AnalysisTraceID + ";";
                if (connstr.Contains("Initial Catalog=;")) connstr = connstr.Replace("Initial Catalog=;", "Initial Catalog=" + AnalysisTraceID + ";");

                
                ASProfilerTraceImporterProcess = new Process();
                ASProfilerTraceImporterProcess.StartInfo.UseShellExecute = false;
                ASProfilerTraceImporterProcess.StartInfo.CreateNoWindow = true;
                ASProfilerTraceImporterProcess.StartInfo.RedirectStandardOutput = true;
                ASProfilerTraceImporterProcess.StartInfo.FileName = Program.TempPath + "ASProfilerTraceImporterCmd.exe";
                ASProfilerTraceImporterProcess.StartInfo.Arguments = "\"" + Directory.GetFiles(m_analysisPath, AnalysisTraceID + "*.trc")[0] + "\" \"" + connstr + "\" \"" + AnalysisTraceID + "\"";
                Trace.WriteLine("Starting AS trace import with command arguments: " + ASProfilerTraceImporterProcess.StartInfo.Arguments);
                ASProfilerTraceImporterProcess.Start();
                while (!ASProfilerTraceImporterProcess.HasExited)
                {
                    string sOut = ASProfilerTraceImporterProcess.StandardOutput.ReadLine();
                    System.Diagnostics.Trace.WriteLine(sOut);
                    if (sOut != null)
                        if (sOut.StartsWith("Loaded "))
                            ProfilerTraceStatusTextBox.Invoke(new System.Action(() =>
                            {
                                List<string> loadedline = ProfilerTraceStatusTextBox.Lines.Where(l => l.StartsWith("Loaded ")).ToList();
                                if (loadedline.Count > 0)
                                    ProfilerTraceStatusTextBox.Text = ProfilerTraceStatusTextBox.Text.Replace(loadedline.First(), sOut);
                                else
                                    ProfilerTraceStatusTextBox.AppendText((ProfilerTraceStatusTextBox.Text == "" ? "" : "\r\n") + sOut);
                            }
                            ));
                        else if (sOut == "Database prepared for analysis." || sOut == "Import of profiler trace cancelled.")
                            break;
                        else
                            ProfilerTraceStatusTextBox.Invoke(new System.Action(() => ProfilerTraceStatusTextBox.AppendText((ProfilerTraceStatusTextBox.Text == "" ? "" : "\r\n") + sOut)));
                }
                if (!bCancelProfilerImport)
                {
                    ProfilerTraceStatusTextBox.Invoke(new System.Action(() => ProfilerTraceStatusTextBox.AppendText((ProfilerTraceStatusTextBox.Text.EndsWith("\r\n") ? "" : "\r\n") + "Adding profiler database to collection data...")));
                    DettachProfilerTraceDB(false);
                    AddFileFromFolderIfAnlyzingZip(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + ".mdf");
                    AddFileFromFolderIfAnlyzingZip(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + ".ldf");
                    if (txtFolderZipForAnalysis.Text.EndsWith(".zip"))
                    {
                        ProfilerTraceStatusTextBox.Invoke(new System.Action(() => ProfilerTraceStatusTextBox.AppendText("\r\nDeleting redundant profiler trace files from data extraction location...")));
                        foreach (string file in Directory.EnumerateFiles(m_analysisPath, AnalysisTraceID + "*.trc"))
                            File.Delete(file);
                    }
                    AttachProfilerTraceDB();
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
                if (ex.Message == "ExecuteNonQuery requires an open and available Connection. The connection's current state is closed.")
                {
                    ProfilerTraceStatusTextBox.Invoke(new System.Action(() => ProfilerTraceStatusTextBox.Text = "Trace file is not yet imported to database table for analysis.  No SQL Server was available to perform import.\r\n"));
                    btnImportProfilerTrace.Invoke(new System.Action(() => btnImportProfilerTrace.Visible = true));
                }
                else
                    ProfilerTraceStatusTextBox.Invoke(new System.Action(() => ProfilerTraceStatusTextBox.Text = "Error loading trace: " + ex.Message + "\r\n"));
            }
        }
        private void bgImportProfilerTraceComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!bCancelProfilerImport)  // We take care of cleanup and completion in cancellation worker if we're cancelled.
            {
                Debug.WriteLine("Import Trace Complete");
                if (File.Exists(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + ".mdf"))
                {
                    btnImportProfilerTrace.Visible = false;
                    splitProfilerAnalysis.Visible = true;
                }
                tbCollection.ForeColor = SystemColors.ControlText;
                tcCollectionAnalysisTabs.Refresh();
                tbCollection.Enabled = true;
                btnAnalysisFolder.Enabled = true;
                btnImportProfilerTrace.Enabled = true;
                btnImportProfilerTrace.Text = "Import and &Analyze";
                LogFeatureUse("Profiler Analysis", "Completed");
                AnalysisMessagePumpTimer.Stop();
            }
        }
        private void BgCancelTrace_DoWork(object sender, DoWorkEventArgs e)
        {
            EventWaitHandle doneWithInit = new EventWaitHandle(false, EventResetMode.ManualReset, "ASProfilerTraceImporterCmdCancelSignal");
            doneWithInit.Set();
            doneWithInit.Close();
            bool bExited = ASProfilerTraceImporterProcess.WaitForExit(1000);
            connSqlDb.ChangeDatabase("master");
            SqlCommand cmd = new SqlCommand("IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'" + AnalysisTraceID + "') ALTER DATABASE [" + AnalysisTraceID + "] SET SINGLE_USER WITH ROLLBACK IMMEDIATE", connSqlDb);
            cmd.ExecuteNonQuery();
            cmd = new SqlCommand("IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'" + AnalysisTraceID + "') DROP DATABASE [" + AnalysisTraceID + "]", connSqlDb);
            cmd.ExecuteNonQuery();
            connSqlDb.Close();
            if (m_analysisPath.EndsWith(".mdf"))
            {
                File.Delete(m_analysisPath);
                File.Delete(m_analysisPath.Replace(".mdf", ".ldf"));
            }
            else
            {
                File.Delete(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + ".mdf");
                File.Delete(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + ".ldf");
            }
            if (!bExited)
                Process.GetProcessesByName("ASProfilerTraceImporterCmd").First().Kill();
        }
        private void BgCancelTrace_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ProfilerTraceStatusTextBox.AppendText("\r\nDropped trace database and deleted files successfully.\r\nTrace file is not yet imported to database table for analysis.  Import to perform analysis.\r\n");
            AnalysisMessagePumpTimer.Stop();
            tbCollection.ForeColor = SystemColors.ControlText;
            tcCollectionAnalysisTabs.Refresh();
            tbCollection.Enabled = true;
            btnAnalysisFolder.Enabled = true;
            btnImportProfilerTrace.Enabled = true;
            btnImportProfilerTrace.Text = "Import and &Analyze";
        }

        #endregion TraceLoader

        #region AttachDettach

        private bool ValidateProfilerTraceDBConnectionStatus()
        {
            if (connSqlDb.State != ConnectionState.Open)
            {
                string sqlForTraces = Properties.Settings.Default["SqlForProfilerTraceAnalysis"] as string;
                string exMsg = "";
                if (sqlForTraces != "")
                {
                    connSqlDb = new SqlConnection("Data Source=" + sqlForTraces + ";Integrated Security=true;Connection Timeout=2;");
                    try
                    {
                        connSqlDb.Open();
                    }
                    catch (Exception ex)
                    {
                        if (!ex.Message.StartsWith("A network-related or instance-specific error occurred while establishing a connection to SQL Server."))
                        {
                            LogException(ex);
                        }
                        sqlForTraces = "";
                        exMsg = ex.Message;
                    }
                }
                if (sqlForTraces == "")
                {
                    DialogResult r = DialogResult.Abort;
                    Invoke(new System.Action(() =>
                    {
                        frmSimpleSQLServerPrompt sqlprompt = new frmSimpleSQLServerPrompt();
                        sqlprompt.cmbServer.Text = sqlForTraces;
                        r = sqlprompt.ShowDialog(this);
                        if (r == DialogResult.OK)
                        {
                            Properties.Settings.Default["SqlForProfilerTraceAnalysis"] = sqlForTraces = sqlprompt.cmbServer.Text;
                            Properties.Settings.Default.Save();
                        }
                    }));
                    if (r == DialogResult.OK)
                    {
                        connSqlDb = new SqlConnection("Data Source=" + Properties.Settings.Default["SqlForProfilerTraceAnalysis"] + ";Integrated Security=true;Persist Security Info=false;");
                        try { connSqlDb.Open(); }
                        catch (Exception ex) { LogException(ex); }
                    }
                    else
                    {
                        Invoke(new System.Action(()=>
                        {
                            ProfilerTraceStatusTextBox.AppendText((ProfilerTraceStatusTextBox.Text.EndsWith("\r\n") ? "" : "\r\n") + "Failure attaching to trace database: " + exMsg + "\r\n");
                            btnImportProfilerTrace.Visible = true;
                        }));
                        
                        return false;
                    }
                }
            }
            return true;
        }
        private void ValidateProfilerTraceViews()
        {
            SqlConnection conn = new SqlConnection(connSqlDb.ConnectionString.Replace("Connection Timeout=2;", ""));
            conn.Open();
            conn.ChangeDatabase(AnalysisTraceID);
            SqlCommand cmd = new SqlCommand("SELECT TOP 1 name FROM sys.views WHERE name = N'" + AnalysisTraceID + "_v'", conn);
            if (cmd.ExecuteScalar() != null)
                bProfilerEventClassSublcassViewPresent = true;
            else
                bProfilerEventClassSublcassViewPresent = false;
            ProfilerTraceStatusTextBox.Invoke(new System.Action(() => ProfilerTraceStatusTextBox.AppendText((ProfilerTraceStatusTextBox.Text.EndsWith("\r\n") ? "" : "\r\n") + "Confirmed event class/subclass view is " + (bProfilerEventClassSublcassViewPresent ? "present." : "not present."))));
            cmd.CommandText = "SELECT MAX(CurrentTime) FROM [" + AnalysisTraceID + "]";
            new Thread(new ThreadStart(() =>
            {
                if (cmd.Connection.State == ConnectionState.Open)
                {
                    // This isn't immediately required and we save 1-2s by doing it off this thread.
                    EndOfTrace = DateTime.MinValue;
                    object dt = cmd.ExecuteScalar();
                    if (!Convert.IsDBNull(dt))
                    {
                        EndOfTrace = Convert.ToDateTime(dt);
                        System.Diagnostics.Trace.WriteLine("End of trace [" + AnalysisTraceID + "] noted at " + EndOfTrace);
                    }
                    conn.Close();
                }
            })).Start();
            Invoke(new System.Action(() =>
            {
                cmbProfilerAnalyses.DataSource = ProfilerTraceAnalysisQueries.Where(q => q.QueryType == (bProfilerEventClassSublcassViewPresent ? ProfilerQueryTypes.QueriesWithEventClassSubclassNames :
                                                                                                    ProfilerQueryTypes.BaseQuery)
                                                                                            || q.Name == "").ToList();
                cmbProfilerAnalyses.Refresh();
            }));
        }
        private void ExecuteProfilerAttachSQL(SqlCommand cmd)
        {
            string mdfPath = m_analysisPath.EndsWith(".mdf") ?
                                m_analysisPath.Substring(0, m_analysisPath.LastIndexOf("\\") + 1) :
                                m_analysisPath.EndsWith(".trc") ?
                                    mdfPath = m_analysisPath.Substring(0, m_analysisPath.LastIndexOf("\\") + 1) + "Analysis\\" :
                                    m_analysisPath + "\\Analysis\\";

            cmd = new SqlCommand("SELECT COUNT(*) FROM master.dbo.sysdatabases WHERE name = N'" + AnalysisTraceID + "'", connSqlDb);
            if (Convert.ToInt32(cmd.ExecuteScalar()) == 1)
                // The database was already attached - change default to leave as it was before.
                chkDettachProfilerAnalysisDBWhenDone.Invoke(new System.Action(()=>chkDettachProfilerAnalysisDBWhenDone.Checked = false));
            else
                chkDettachProfilerAnalysisDBWhenDone.Invoke(new System.Action(() => chkDettachProfilerAnalysisDBWhenDone.Checked = true));

            cmd = new SqlCommand("IF NOT EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'" + AnalysisTraceID + "') CREATE DATABASE [" + AnalysisTraceID + "] ON (FILENAME = N'" + mdfPath.Replace("'", "''") + AnalysisTraceID + ".mdf'),"
                                                + "(FILENAME = N'" + mdfPath.Replace("'", "''") + AnalysisTraceID + ".ldf') "
                                                + "FOR ATTACH", connSqlDb);
            cmd.ExecuteNonQuery();
            bProfilerTraceDbAttached = true;

            ValidateProfilerTraceViews();

            Invoke(new System.Action(() =>
            {
                splitProfilerAnalysis.Visible = true;
                ttStatus.SetToolTip(chkDettachProfilerAnalysisDBWhenDone, "Profiler traces were imported into a trace database in the file:\r\n" + AnalysisTraceID
                                                                    + ".mdf\r\n\r\nLocated at:\r\n" + mdfPath + "\\Analysis\r\n\r\n"
                                                                    + "Uncheck this checkbox if the scenario requires further analysis offline, to leave the database attached when exiting this tool.\r\n\r\n"
                                                                    + "NOTE:  While attached the SQL data source at [" + (connSqlDb.DataSource.StartsWith(".") ? Environment.MachineName + connSqlDb.DataSource.Substring(1) : connSqlDb.DataSource) + "] locks these files from deletion.");
                ProfilerTraceStatusTextBox.AppendText((ProfilerTraceStatusTextBox.Text.EndsWith("\r\n") ? "" : "\r\n") + "Attached trace database [" + AnalysisTraceID + "]\r\nto SQL instance [" + (connSqlDb.DataSource.StartsWith(".") ? Environment.MachineName + connSqlDb.DataSource.Substring(1) : connSqlDb.DataSource) + "]\r\nfor analysis at " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") + ".\r\n");
            }));

        }

        List<string> CurrentProfilerTraceColumnList = new List<string>();

        private void AttachProfilerTraceDB()
        {
            CurrentProfilerTraceColumnList = new List<string>();
            splitProfilerAnalysis.Invoke(new System.Action(() => splitProfilerAnalysis.Visible = splitProfilerAnalysis.Enabled = btnAnalysisFolder.Enabled = false));
            tcAnalysis.Invoke(new System.Action(() => ProfilerTraceStatusTextBox.Text += (ProfilerTraceStatusTextBox.Text.EndsWith("\r\n") || ProfilerTraceStatusTextBox.Text == "" ? "" : "\r\n") + "Attaching profiler trace database..."));
            ValidateProfilerTraceDBConnectionStatus();           
            if (connSqlDb.State == ConnectionState.Open)
            {
                connSqlDb.ChangeDatabase("master");
                SqlCommand cmd = new SqlCommand("IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'" + AnalysisTraceID + "') ALTER DATABASE [" + AnalysisTraceID + "] SET MULTI_USER", connSqlDb);
                cmd.ExecuteNonQuery();
                try {
                    ExecuteProfilerAttachSQL(cmd);
                    cmd.CommandText = "select column_name from [" + AnalysisTraceID + "].INFORMATION_SCHEMA.COLUMNS where table_name = N'" + AnalysisTraceID + "'";
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                        CurrentProfilerTraceColumnList.Add((dr["column_name"] as string).ToLower());
                    dr.Close();
                }
                catch (SqlException ex)
                {
                    LogException(ex);
                    Invoke(new System.Action(() => cmbProfilerAnalyses.Enabled = txtProfilerAnalysisQuery.Enabled = false));

                    if (ex.Message.Contains("cannot be opened because it is version"))
                    {
                        MessageBox.Show("Unable to attach to database since it was created with a later version of SQL than the selected server.", "Select another instance", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        frmSimpleSQLServerPrompt sqlprompt = new frmSimpleSQLServerPrompt();
                        while (true)
                        {
                            if (sqlprompt.ShowDialog(this) == DialogResult.OK)
                            {
                                Properties.Settings.Default["SqlForProfilerTraceAnalysis"] = sqlprompt.cmbServer.Text;
                                Properties.Settings.Default.Save();
                                connSqlDb = new SqlConnection("Data Source=" + sqlprompt.cmbServer.Text + ";Integrated Security=true;Persist Security Info=false;");
                                connSqlDb.Open();
                                cmd.Connection = connSqlDb;
                                try
                                {
                                    ExecuteProfilerAttachSQL(cmd);
                                    cmd.CommandText = "select column_name from [" + AnalysisTraceID + "].INFORMATION_SCHEMA.COLUMNS where table_name = N'" + AnalysisTraceID + "'";
                                    SqlDataReader dr = cmd.ExecuteReader();
                                    while (dr.Read())
                                        CurrentProfilerTraceColumnList.Add((dr["column_name"] as string).ToLower());
                                    dr.Close();
                                    break;
                                }
                                catch (Exception ex2)
                                {
                                    LogException(ex2);
                                    MessageBox.Show("Unable to attach to database since it was created with a later version of SQL than the selected server.", "Select another instance", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                            else
                            {
                                Invoke(new System.Action(() => ProfilerTraceStatusTextBox.AppendText((ProfilerTraceStatusTextBox.Text.EndsWith("\r\n") ? "" : "\r\n") + "Unable to load trace data to SQL table.  No local instance able to host the data is available.\r\nTry opening again on a machine with a local SQL instance running.")));
                                return;
                            }
                        }
                    }
                    else if (ex.Message.Contains("Unable to open the physical file") || ex.Message.Contains("The path specified by"))
                    {
                        ProfilerTraceStatusTextBox.Invoke(new System.Action(() =>
                        {
                            ProfilerTraceStatusTextBox.Text = (ProfilerTraceStatusTextBox.Text.EndsWith("\r\n") && !String.IsNullOrWhiteSpace(ProfilerTraceStatusTextBox.Text) ? "" : "\r\n") + "Trace file is not yet imported to database table for analysis.  Import to perform analysis.\r\n";
                            btnImportProfilerTrace.Visible = true;
                        }));
                        return;
                    }
                    else
                    {
                        ProfilerTraceStatusTextBox.Invoke(new System.Action(() => ProfilerTraceStatusTextBox.AppendText((ProfilerTraceStatusTextBox.Text.EndsWith("\r\n") ? "" : "\r\n") + "Unable to attach to database due to exception:\r\n" + ex.Message)));
                        ProfilerTraceStatusTextBox.Invoke(new System.Action(() =>
                        {
                            ProfilerTraceStatusTextBox.Text = (ProfilerTraceStatusTextBox.Text.EndsWith("\r\n") && !String.IsNullOrWhiteSpace(ProfilerTraceStatusTextBox.Text) ? "" : "\r\n") + "Trace file is not yet imported to database table for analysis.  Import to perform analysis.\r\n";
                            btnImportProfilerTrace.Visible = true;
                        }));
                    }
                }
            }
            btnAnalysisFolder.Invoke(new System.Action(() => btnAnalysisFolder.Enabled = splitProfilerAnalysis.Enabled = true));
        }
        private void DettachProfilerTraceDB(bool bClearText = true)
        {
            try
            {
                bProfilerTraceDbAttached = false;
                // Dettach without blocking for existing sessions...
                if (connSqlDb.State == ConnectionState.Open)

                {
                    connSqlDb.ChangeDatabase("master");
                    SqlCommand cmd = new SqlCommand("IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'" + AnalysisTraceID + "') ALTER DATABASE [" + AnalysisTraceID + "] SET SINGLE_USER WITH ROLLBACK IMMEDIATE", connSqlDb);
                    cmd.ExecuteNonQuery();
                    cmd = new SqlCommand("IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'" + AnalysisTraceID + "') EXEC master.dbo.sp_detach_db @dbname = N'" + AnalysisTraceID + "'", connSqlDb);
                    cmd.ExecuteNonQuery();
                    connSqlDb.Close();
                }
                Invoke(new System.Action(() =>
                {
                    splitProfilerAnalysis.Visible = false;
                    btnImportProfilerTrace.Visible = true;
                    if (bClearText)
                        ProfilerTraceStatusTextBox.Text = "";
                }));
            }
            catch (Exception ex) { LogException(ex); }  // could fail if service stopped, no biggie just move on...
        }

        #endregion AttachDettach

        #region UtilityTypesAndFunctions

        enum ProfilerQueryTypes
        {
            BaseQuery = 1,
            QueriesWithEventClassSubclassNames,
            AllQueries
        }

        class ProfilerTraceQuery
        {
            public ProfilerTraceQuery(string name, string query, string desc, ProfilerQueryTypes queryType)
            { Name = name; Query = query; QueryType = queryType; Description = desc; }
            public ProfilerTraceQuery(ProfilerTraceQuery p, ProfilerQueryTypes t)
            {
                Name = p.Name;
                Query = p.Query;
                Description = p.Description;
                QueryType = t;
            }
            public string Name { get; set; }
            public string Query { get; set; }
            public string Description { get; set; }
            public ProfilerQueryTypes QueryType { get; set; }
        }

        private string ConvertProfilerEventClassSubclassViewQueryToSimpleTableQuery(string qry)
        {
            return qry.Replace("[Table_v]", "[Table]").Replace("EventClassName, ", "").Replace("EventSubclassName, ", "").Replace("EventClassName", "").Replace("EventSubclassName", "").Replace("[Table", "[" + AnalysisTraceID);
        }
        private List<ProfilerTraceQuery> InitializeProfilerTraceAnalysisQueries()
        {
            List<ProfilerTraceQuery> q = new List<ProfilerTraceQuery>();
            q.Add(new ProfilerTraceQuery("", "", "", ProfilerQueryTypes.BaseQuery));

            // Basic details
            q.Add(new ProfilerTraceQuery("Basic trace summary",
                                         Properties.Resources.QueryBasicTraceSummary,
                                         "The basic trace summary gives a very high level overview of the contents of the trace.",
                                         ProfilerQueryTypes.BaseQuery));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.AllQueries));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithEventClassSubclassNames));

            // Query FE/SE Stats
            q.Add(new ProfilerTraceQuery("Formula/Storage engine statistics",
                                         Properties.Resources.QueryFESEStats,
                                         "Statistics calculated showing percentage of time spent in formula engine (calculations) vs. storage engine (IO) if Query Subcube events available.",
                                         ProfilerQueryTypes.QueriesWithEventClassSubclassNames));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.AllQueries));

            // Longest running queries
            q.Add(new ProfilerTraceQuery("Longest running queries captured",
                                         ConvertProfilerEventClassSubclassViewQueryToSimpleTableQuery(Properties.Resources.QueryLongestRunningQueries),
                                         "Reports the longest running queries in the trace.  Includes calculated durations for queries started but not completed in the trace, up to the point of capture stop.",
                                         ProfilerQueryTypes.BaseQuery));
            q.Add(new ProfilerTraceQuery("Longest running queries captured",
                                         Properties.Resources.QueryLongestRunningQueries,
                                         "Reports the longest running queries in the trace.  Includes calculated durations for queries started but not completed in the trace, up to the point of capture stop.",
                                         ProfilerQueryTypes.AllQueries));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithEventClassSubclassNames));


            // Longest running queries
            q.Add(new ProfilerTraceQuery("Longest running commands captured",
                                         ConvertProfilerEventClassSubclassViewQueryToSimpleTableQuery(Properties.Resources.QueryLongestRunningCommands),
                                         "Reports the longest running commands in the trace.  Includes calculated durations for commands started but not completed in the trace, up to the point of capture stop.",
                                         ProfilerQueryTypes.BaseQuery));
            q.Add(new ProfilerTraceQuery("Longest running commands captured",
                                         Properties.Resources.QueryLongestRunningCommands,
                                         "Reports the longest running commands in the trace.  Includes calculated durations for commands started but not completed in the trace, up to the point of capture stop.",
                                         ProfilerQueryTypes.AllQueries));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithEventClassSubclassNames));


            // Most collectively expensive events
            q.Add(new ProfilerTraceQuery("Most collectively expensive events",
                                         ConvertProfilerEventClassSubclassViewQueryToSimpleTableQuery(Properties.Resources.QueryMostCollectivelyExpensiveEvents),
                                         "Summarizes identical events' durations to show the most cumulatively expensive type of activity in the trace.",
                                         ProfilerQueryTypes.BaseQuery));
            q.Add(new ProfilerTraceQuery("Most collectively expensive events",
                                         Properties.Resources.QueryMostCollectivelyExpensiveEvents,
                                         "Summarizes identical events' durations to show the most cumulatively expensive type of activity in the trace.",
                                         ProfilerQueryTypes.AllQueries));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithEventClassSubclassNames));

            // Most collectively expensive queries
            q.Add(new ProfilerTraceQuery("Most collectively expensive queries",
                                         ConvertProfilerEventClassSubclassViewQueryToSimpleTableQuery(Properties.Resources.QueryMostCollectivelyExpensiveQueries),
                                         "Summarizes identical queries to show the most cummulatively expensive queries in the trace.  Sometimes fast but frequently run queries may still be the culprit in a server encountering degradation.",
                                         ProfilerQueryTypes.BaseQuery));
            q.Add(new ProfilerTraceQuery("Most collectively expensive queries",
                                         Properties.Resources.QueryMostCollectivelyExpensiveQueries,
                                         "Summarizes identical queries to show the most cummulatively expensive queries in the trace.  Sometimes fast but frequently run queries may still be the culprit in a server encountering degradation.",
                                         ProfilerQueryTypes.AllQueries));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithEventClassSubclassNames));

            // Most collectively expensive commands
            q.Add(new ProfilerTraceQuery("Most collectively expensive commands",
                                         ConvertProfilerEventClassSubclassViewQueryToSimpleTableQuery(Properties.Resources.QueryMostCollectivelyExpensiveCommands),
                                         "Summarizes identical queries to show the most cummulatively expensive commands in the trace.  Sometimes fast but frequently run jobs may still be the culprit in a server encountering degradation.",
                                         ProfilerQueryTypes.BaseQuery));
            q.Add(new ProfilerTraceQuery("Most collectively expensive commands",
                                         Properties.Resources.QueryMostCollectivelyExpensiveCommands,
                                         "Summarizes identical queries to show the most cummulatively expensive commands in the trace.  Sometimes fast but frequently run jobs may still be the culprit in a server encountering degradation.",
                                         ProfilerQueryTypes.AllQueries));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithEventClassSubclassNames));

            // Errors
            q.Add(new ProfilerTraceQuery("Queries/commands with errors",
                                         ConvertProfilerEventClassSubclassViewQueryToSimpleTableQuery(Properties.Resources.QueryQueriesCommandsWithErrors),
                                         "Reports queries and commands with errors, and related error rows.",
                                         ProfilerQueryTypes.BaseQuery));
            q.Add(new ProfilerTraceQuery("Queries/commands with errors",
                                         Properties.Resources.QueryQueriesCommandsWithErrors,
                                         "Reports queries and commands with errors, and related error rows.",
                                         ProfilerQueryTypes.AllQueries));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithEventClassSubclassNames));

            // Most impactful queries/commands
            q.Add(new ProfilerTraceQuery("Most impactful queries/commands",
                                         ConvertProfilerEventClassSubclassViewQueryToSimpleTableQuery(Properties.Resources.QueryMostImpactfulQueriesCommands),
                                         "Calculates the \"most impactful\" queries and commands based on the number of other queries and commands that overlap.  Includes queries and commands that start but do not complete within the trace, or start before the trace but complete during its capture.",
                                         ProfilerQueryTypes.BaseQuery));
            q.Add(new ProfilerTraceQuery("Most impactful queries/commands",
                                         Properties.Resources.QueryMostImpactfulQueriesCommands,
                                         "Calculates the \"most impactful\" queries and commands based on the number of other queries and commands that overlap.  Includes queries and commands that start but do not complete within the trace, or start before the trace but complete during its capture.",
                                         ProfilerQueryTypes.AllQueries));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithEventClassSubclassNames));

            // Queries/Commands not completed during trace
            q.Add(new ProfilerTraceQuery("Queries/commands not completed",
                                         ConvertProfilerEventClassSubclassViewQueryToSimpleTableQuery(Properties.Resources.QueryQueriesCommandsNotCompleted),
                                         "Explicitly finds queries and commands started but not completed withing the trace.",
                                         ProfilerQueryTypes.BaseQuery));
            q.Add(new ProfilerTraceQuery("Queries/commands not completed",
                                         Properties.Resources.QueryQueriesCommandsNotCompleted,
                                         "Explicitly finds queries and commands started but not completed withing the trace.",
                                         ProfilerQueryTypes.AllQueries));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithEventClassSubclassNames));

            // Possible runaway sessions
            q.Add(new ProfilerTraceQuery("Possible runaway sessions",
                                         Properties.Resources.QueryPossibleRunawaySessions,
                                         "Lists sessions with no command or query begin or end events found in the trace.  These may be executing runaway requests started before the trace and not completed during the trace either.",
                                         ProfilerQueryTypes.AllQueries));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.BaseQuery));
            q.Add(new ProfilerTraceQuery(q.Last(), ProfilerQueryTypes.QueriesWithEventClassSubclassNames));

            return q;
        }
        private bool Validate2017ManagementComponents()
        {
            if (Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Classes\\ssms.sql.14.0") != null)
                return true;
            else
                if (MessageBox.Show("Profiler analysis requires SQL 2017 Management Studio Components.  Download now to install?", "SQL 2017 Management Components Missing", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                    Process.Start("https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms");
            return false;    
        }

        #endregion UtilityTypesAndFunctions
    }
}