using Microsoft.AnalysisServices;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading;
using System.IO;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Configuration;

namespace SSASDiag
{
    public partial class frmSSASDiag : Form
    {
        #region AnalysisUI
        private void btnAnalysisFolder_Click(object sender, EventArgs e)
        {
            BrowseForFolder bff = new BrowseForFolder();
            bff.Filters.Add("zip");
            string strPath = AppDomain.CurrentDomain.GetData("originalbinlocation") as string;
            strPath = bff.SelectFolder(lblFolderZipPrompt.Text, txtFolderZipForAnalysis.Text == "" ? strPath : txtFolderZipForAnalysis.Text, this.Handle);
            if (strPath != null && strPath != "")
            {
                txtFolderZipForAnalysis.Text = m_analysisPath = strPath;
                PopulateAnalysisTabs();
            }
        }

        private void AddFileFromFolderIfAnlyzingZip(string file)
        {
            if (txtFolderZipForAnalysis.Text.EndsWith(".zip"))
            {
                Ionic.Zip.ZipFile z = new Ionic.Zip.ZipFile(txtFolderZipForAnalysis.Text);
                if (z.Entries.Where(f => f.FileName.Substring(f.FileName.LastIndexOf("/") + 1) == file.Substring(file.LastIndexOf("\\") + 1)).Count() == 0)
                {
                    z.AddFile(file, "Analysis");
                    z.Save();
                }
            }
        }

        private void PopulateAnalysisTabs()
        {
            foreach (TabPage t in tcAnalysis.TabPages)
                HiddenTabPages.Add(t);
            tcAnalysis.TabPages.Clear();

            if (m_analysisPath != null)
            {
                if (m_analysisPath.EndsWith(".zip"))
                {
                    Ionic.Zip.ZipFile z = new Ionic.Zip.ZipFile(m_analysisPath);
                    // Always extract directly into the current running location.
                    // This ensures we don't accidentally fill up a temp drive or something with large files.
                    m_analysisPath = AppDomain.CurrentDomain.GetData("originalbinlocation") + "\\" + m_analysisPath.Substring(m_analysisPath.LastIndexOf("\\") + 1).Replace(".zip", "");
                    AnalysisTraceID = m_analysisPath.Substring(m_analysisPath.LastIndexOf("\\") + 1).Replace("_SSASDiagOutput", "_SSASDiag");

                    if (!Directory.Exists(m_analysisPath))
                        Directory.CreateDirectory(m_analysisPath);
                    if (!Directory.Exists(m_analysisPath + "\\Analysis"))
                        Directory.CreateDirectory(m_analysisPath + "\\Analysis");

                    if (z.Entries.Where(f => f.FileName == "Analysis\\" + AnalysisTraceID + ".mdf").Count() > 0)
                    {
                        z.Entries.Where(f => f.FileName == "Analysis\\" + AnalysisTraceID + ".mdf").First().Extract(m_analysisPath + "\\Analysis", Ionic.Zip.ExtractExistingFileAction.DoNotOverwrite);
                        z.Entries.Where(f => f.FileName == "Analysis\\" + AnalysisTraceID + ".ldf").First().Extract(m_analysisPath + "\\Analysis", Ionic.Zip.ExtractExistingFileAction.DoNotOverwrite);
                    }
                    else
                       if (z.Entries.Where(f => f.FileName.Contains(".trc")).Count() > 0)
                        foreach (Ionic.Zip.ZipEntry e in z.Entries.Where(f => f.FileName.Contains(".trc")))
                            e.Extract(m_analysisPath, Ionic.Zip.ExtractExistingFileAction.DoNotOverwrite);
                    if (z.Entries.Where(f => f.FileName == "Analysis\\" + AnalysisTraceID + "_NetworkAnalysis.log").Count() > 0)
                        z.Entries.Where(f => f.FileName == "Analysis\\" + AnalysisTraceID + "_NetworkAnalysis.log").First().Extract(m_analysisPath + "\\Analysis", Ionic.Zip.ExtractExistingFileAction.DoNotOverwrite);
                    else
                    {
                        if (z.Entries.Where(f => f.FileName == AnalysisTraceID + ".etl").Count() > 0)
                            z.Entries.Where(f => f.FileName == AnalysisTraceID + ".etl").First().Extract(m_analysisPath, Ionic.Zip.ExtractExistingFileAction.DoNotOverwrite);
                        if (z.Entries.Where(f => f.FileName == AnalysisTraceID + ".cab").Count() > 0)
                            z.Entries.Where(f => f.FileName == AnalysisTraceID + ".cab").First().Extract(m_analysisPath, Ionic.Zip.ExtractExistingFileAction.DoNotOverwrite);
                    }
                    if (z.Entries.Where(f => f.FileName.Contains(".blg")).Count() > 0)
                        foreach (Ionic.Zip.ZipEntry e in z.Entries.Where(f => f.FileName.Contains(".blg")))
                            e.Extract(m_analysisPath, Ionic.Zip.ExtractExistingFileAction.DoNotOverwrite);
                    if (z.Entries.Where(f => f.FileName.Contains(".mdmp")).Count() > 0)
                        foreach (Ionic.Zip.ZipEntry e in z.Entries.Where(f => f.FileName.Contains(".mdmp")))
                            e.Extract(m_analysisPath, Ionic.Zip.ExtractExistingFileAction.DoNotOverwrite);
                    if (z.Entries.Where(f => f.FileName.Contains(".evtx")).Count() > 0)
                        foreach (Ionic.Zip.ZipEntry e in z.Entries.Where(f => f.FileName.Contains(".evtx")))
                            e.Extract(m_analysisPath, Ionic.Zip.ExtractExistingFileAction.DoNotOverwrite);
                    if (z.Entries.Where(f => f.FileName == "msmdsrv.ini").Count() > 0)
                        z.Entries.Where(f => f.FileName == "msmdsrv.ini").First().Extract(m_analysisPath, Ionic.Zip.ExtractExistingFileAction.DoNotOverwrite);
                    z.Dispose();
                }

                if (File.Exists(m_analysisPath + "\\msmdsrv.ini"))
                {
                    tcAnalysis.TabPages.Add(new TabPage("Configuration") { ImageIndex = 0, Name = "Configuration" });
                    tcAnalysis.TabPages["Configuration"].Controls.Add(GetStatusTextBox("Check back soon for automated analysis of configuration details."));
                }
                if (Directory.GetFiles(m_analysisPath, "*.mdmp", SearchOption.AllDirectories).Count() > 0)
                {
                    tcAnalysis.TabPages.Add(new TabPage("Crash Dumps") { ImageIndex = 1, Name = "Crash Dumps" });
                    tcAnalysis.TabPages["Crash Dumps"].Controls.Add(GetStatusTextBox("Check back soon for automated analysis of crash dumps."));
                }
                if (File.Exists(m_analysisPath + "\\" + AnalysisTraceID + "_Application.evtx") ||
                    File.Exists(m_analysisPath + "\\" + AnalysisTraceID + "_System.evtx"))
                {
                    tcAnalysis.TabPages.Add(new TabPage("Event Logs") { ImageIndex = 2, Name = "Event Logs" });
                    tcAnalysis.TabPages["Event Logs"].Controls.Add(GetStatusTextBox("Check back soon for automated analysis of event logs."));
                }
                if (File.Exists(m_analysisPath + "\\" + AnalysisTraceID + ".etl") || File.Exists(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + "_NetworkAnalysis.diag.log"))
                {
                    tcAnalysis.TabPages.Add(new TabPage("Network Traces") { ImageIndex = 3, Name = "Network Traces" });
                    TextBox txtNetworkAnalysis = GetStatusTextBox();
                    tcAnalysis.TabPages["Network Traces"].Controls.Add(txtNetworkAnalysis);
                    if (File.Exists(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + "_NetworkAnalysis.log"))
                    {
                        txtNetworkAnalysis.Text = File.ReadAllText(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + "_NetworkAnalysis.log");
                        AddFileFromFolderIfAnlyzingZip(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + "_NetworkAnalysis.log");
                        AddFileFromFolderIfAnlyzingZip(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + "_NetworkAnalysis.diag.log");
                    }
                    else
                    {
                        new Thread(new ThreadStart(new System.Action(() =>
                        {
                            Process p = new Process();
                            p.StartInfo.UseShellExecute = false;
                            p.StartInfo.CreateNoWindow = true;
                            p.StartInfo.RedirectStandardOutput = true;
                            p.StartInfo.FileName = Environment.GetEnvironmentVariable("temp") + "\\SSASDiag\\sqlna.exe";
                            p.StartInfo.Arguments = "\"" + m_analysisPath + "\\" + AnalysisTraceID + ".etl\" /output \"" + m_analysisPath + "\\Analysis\\" + AnalysisTraceID + "_NetworkAnalysis.log\"";
                            p.Start();
                            string sOut = p.StandardOutput.ReadToEnd();
                            p.WaitForExit();
                            List<string> sNetworkAnalysis = new List<string>(File.ReadAllLines(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + "_NetworkAnalysis.log"));
                            sNetworkAnalysis.RemoveRange(0, 6);
                            sNetworkAnalysis.RemoveRange(sNetworkAnalysis.Count - 6, 6);
                            for (int i = 0; i < sNetworkAnalysis.Count; i++)
                                sNetworkAnalysis[i] = sNetworkAnalysis[i].TrimStart(' ');
                            File.WriteAllLines(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + "_NetworkAnalysis.log", sNetworkAnalysis);
                            txtNetworkAnalysis.Invoke(new System.Action(() => txtNetworkAnalysis.Text = string.Join("\r\n", sNetworkAnalysis.ToArray())));
                            AddFileFromFolderIfAnlyzingZip(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + "_NetworkAnalysis.log");
                            AddFileFromFolderIfAnlyzingZip(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + "_NetworkAnalysis.diag.log");
                            if (txtFolderZipForAnalysis.Text.EndsWith(".zip"))
                            {
                                foreach (string file in Directory.EnumerateFiles(m_analysisPath, AnalysisTraceID + "*.etl"))
                                    File.Delete(file);
                                foreach (string file in Directory.EnumerateFiles(m_analysisPath, AnalysisTraceID + "*.cab"))
                                    File.Delete(file);
                            }
                        }))).Start();
                    }
                }
                if (File.Exists(m_analysisPath + "\\" + AnalysisTraceID + ".blg"))
                {
                    tcAnalysis.TabPages.Add(new TabPage("Performance Logs") { ImageIndex = 4, Name = "Performance Logs" });
                    tcAnalysis.TabPages["Performance Logs"].Controls.Add(GetStatusTextBox("Check back soon for automated analysis of performance logs."));
                }
                if (File.Exists(m_analysisPath + "\\" + AnalysisTraceID + "1.trc") || File.Exists(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + ".mdf"))
                {
                    tcAnalysis.TabPages.Add(HiddenTabPages.Where(t => t.Text == "Profiler Traces").First());
                    HiddenTabPages.Remove(HiddenTabPages.Where(t => t.Text == "Profiler Traces").First());
                    string sqlForTraces = Properties.Settings.Default["SqlForProfilerTraceAnalysis"] as string;

                    if (File.Exists(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + ".mdf"))
                    {
                        AddFileFromFolderIfAnlyzingZip(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + ".mdf");
                        AddFileFromFolderIfAnlyzingZip(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + ".ldf");
                        ProfilerTraceStatusTextBox.AppendText("Using trace data loaded into SQL .mdf at " + m_analysisPath + "\\Analysis\\.\r\n");
                        ttStatus.SetToolTip(chkDettachProfilerAnalysisDBWhenDone, "Profiler traces were imported into a trace database in the file:\r\n" + AnalysisTraceID
                                                                                + ".mdf\r\n\r\nLocated at:\r\n" + m_analysisPath + "\\Analysis\r\n\r\n"
                                                                                + "Uncheck this checkbox if the scenario requires further analysis.\r\n\r\n"
                                                                                + "Note:  Whie attached the SQL data source at [" + connSqlDb.DataSource + "] locks these files from deletion while started.");
                        btnImportProfilerTrace.Visible = false;
                    }
                }
            }
        }

        private void btnImportProfilerTrace_Click(object sender, EventArgs e)
        {
            btnImportProfilerTrace.Enabled = false;
            ValidateProfilerTraceDBConnectionStatus();
            try
            {
                BackgroundWorker bg = new BackgroundWorker();
                bg.DoWork += bgImportProfilerTrace;
                bg.RunWorkerCompleted += bgImportProfilerTraceComplete;
                bg.RunWorkerAsync(new object[] { ProfilerTraceStatusTextBox, "Initial Catalog=" + AnalysisTraceID + ";Persist Security Info=False;" + connSqlDb.ConnectionString });
            }
            catch (SqlException ex)
            {
                ProfilerTraceStatusTextBox.Text = "Error loading profiler trace: \r\n" + ex.Message;
            }
        }


        private void bgImportProfilerTrace(object sender, DoWorkEventArgs e)
        {
            try
            {
                SqlCommand cmd = new SqlCommand(Properties.Resources.CreateDBSQLScript.
                                    Replace("<mdfpath/>", m_analysisPath + "\\Analysis\\" + AnalysisTraceID + ".mdf").
                                    Replace("<ldfpath/>", m_analysisPath + "\\Analysis\\" + AnalysisTraceID + ".ldf").
                                    Replace("<dbname/>", AnalysisTraceID)
                                    , connSqlDb);
                int ret = cmd.ExecuteNonQuery();

                TextBox ProfilerTraceStatusTextBox = (e.Argument as object[])[0] as TextBox;
                string connstr = (e.Argument as object[])[1] as string;
                Process p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.FileName = Environment.GetEnvironmentVariable("temp") + "\\SSASDiag\\ASProfilerTraceImporterCmd.exe";
                p.StartInfo.Arguments = "\"" + m_analysisPath + "\\" + AnalysisTraceID + "1.trc\" \"" + connstr + "\" \"" + AnalysisTraceID + "\"";
                p.Start();
                while (!p.HasExited)
                    ProfilerTraceStatusTextBox.Invoke(new System.Action(() => ProfilerTraceStatusTextBox.AppendText((ProfilerTraceStatusTextBox.Text == "" ? "" : "\r\n") + p.StandardOutput.ReadLine())));
                SqlConnection conn = new SqlConnection(connstr.Replace("Initial Catalog=" + AnalysisTraceID, "Initial Catalog=master"));
                conn.Open();
                cmd = new SqlCommand("EXEC master.dbo.sp_detach_db @dbname = N'" + AnalysisTraceID + "'", conn);
                ret = cmd.ExecuteNonQuery();
                AddFileFromFolderIfAnlyzingZip(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + ".mdf");
                AddFileFromFolderIfAnlyzingZip(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + ".ldf");
                if (txtFolderZipForAnalysis.Text.EndsWith(".zip"))
                    foreach (string file in Directory.EnumerateFiles(m_analysisPath, AnalysisTraceID + "*.trc"))
                        File.Delete(file);
            }
            catch (Exception ex)
            {
                ProfilerTraceStatusTextBox.Text = "Error loading trace: " + ex.Message;
            }
        }

        private void bgImportProfilerTraceComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            if (File.Exists(m_analysisPath + "\\Analysis\\" + AnalysisTraceID + ".mdf"))
            {
                btnImportProfilerTrace.Visible = false;
                tcAnalysis_SelectedIndexChanged(sender, e);
            }
            btnImportProfilerTrace.Enabled = true;
        }

        private TextBox GetStatusTextBox(string Text = "")
        {
            TextBox txtStatus = new TextBox();
            txtStatus.Name = "StatusTextbox";
            txtStatus.Multiline = true;
            txtStatus.ReadOnly = true;
            txtStatus.BackColor = SystemColors.ControlText;
            txtStatus.ForeColor = Color.LightSkyBlue;
            txtStatus.Font = (HiddenTabPages.Find(t => t.Name == "tbProfilerTraces") == null ? tcAnalysis.TabPages["tbProfilerTraces"] : HiddenTabPages.Find(t => t.Name == "tbProfilerTraces")).Font;
            txtStatus.Dock = DockStyle.Fill;
            txtStatus.WordWrap = false;
            txtStatus.ScrollBars = ScrollBars.Both;
            txtStatus.Text = Text;
            return txtStatus;
        }
        private void tcCollectionAnalysisTabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tcCollectionAnalysisTabs.SelectedIndex == 1 && tbAnalysis.Enabled)
            {
                if (dc != null)
                {
                    if (Directory.Exists(AppDomain.CurrentDomain.GetData("originalbinlocation") + "\\" + dc.TraceID + "Output"))
                    {
                        m_analysisPath = txtFolderZipForAnalysis.Text = AppDomain.CurrentDomain.GetData("originalbinlocation") + "\\" + dc.TraceID + "Output";
                        PopulateAnalysisTabs();
                    }
                    else
                    {
                        if (File.Exists(AppDomain.CurrentDomain.GetData("originalbinlocation") + "\\" + dc.TraceID + ".zip"))
                        {
                            m_analysisPath = txtFolderZipForAnalysis.Text = AppDomain.CurrentDomain.GetData("originalbinlocation") + "\\" + dc.TraceID + ".zip";
                            PopulateAnalysisTabs();
                        }
                    }
                }
            }
        }
        private void ValidateProfilerTraceDBConnectionStatus()
        {
            if (connSqlDb.State != ConnectionState.Open)
            {
                string sqlForTraces = Properties.Settings.Default["SqlForProfilerTraceAnalysis"] as string;
                if (sqlForTraces != "")
                {
                    connSqlDb = new SqlConnection("Data Source=" + sqlForTraces + ";Integrated Security=true;Connection Timeout=2;");
                    try
                    {
                        connSqlDb.Open();
                    }
                    catch
                    {
                        sqlForTraces = "";
                    }
                }
                if (sqlForTraces == "")
                {
                    frmSimpleSQLServerPrompt sqlprompt = new frmSimpleSQLServerPrompt();
                    sqlprompt.cmbServer.Text = sqlForTraces;
                    if (sqlprompt.ShowDialog(this) == DialogResult.OK)
                    {
                        Properties.Settings.Default["SqlForProfilerTraceAnalysis"] = sqlForTraces = sqlprompt.cmbServer.Text;
                        Properties.Settings.Default.Save();
                        connSqlDb = new SqlConnection("Data Source=" + sqlprompt.cmbServer.Text + ";Integrated Security=true;Persist Security Info=false;");
                        connSqlDb.Open();
                    }
                    else
                    {
                        ProfilerTraceStatusTextBox.AppendText("Unable to load trace data to SQL table.  No local instance available.");
                        return;
                    }
                }
            }
        }

        private void tcAnalysis_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tcAnalysis.SelectedTab != null)
            {
                if (tcAnalysis.SelectedTab.Name == "tbProfilerTraces" && !bProfilerTraceDbAttached && chkDettachProfilerAnalysisDBWhenDone.Checked)
                {
                    ValidateProfilerTraceDBConnectionStatus();
                    if (connSqlDb.State == ConnectionState.Open)
                    {
                        SqlCommand cmd = new SqlCommand("IF NOT EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'" + AnalysisTraceID + "') CREATE DATABASE [" + AnalysisTraceID + "] ON (FILENAME = N'" + m_analysisPath + "\\Analysis\\" + AnalysisTraceID + ".mdf'),"
                                                        + "(FILENAME = N'" + m_analysisPath + "\\Analysis\\" + AnalysisTraceID + ".ldf') "
                                                        + "FOR ATTACH", connSqlDb);
                        try
                        {
                            cmd.ExecuteNonQuery();
                            bProfilerTraceDbAttached = true;
                            (tcAnalysis.SelectedTab.Controls["ProfilerTraceStatusTextbox"] as TextBox).AppendText("\r\nAttached trace database [" + AnalysisTraceID + "]\r\nto SQL instance [" + connSqlDb.DataSource + "]\r\nfor analysis at " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") + ".\r\n");
                        }
                        catch (SqlException ex)
                        {
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
                                        try
                                        {
                                            cmd.Connection = connSqlDb;
                                            cmd.ExecuteNonQuery();
                                            bProfilerTraceDbAttached = true;
                                            tcAnalysis.SelectedTab.Controls["ProfilerTraceStatusTextbox"].Text += "\r\nAttached trace database [" + AnalysisTraceID + "]\r\nto SQL instance [" + connSqlDb.DataSource + "]\r\nfor analysis at " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") + ".\r\n";
                                            break;
                                        }
                                        catch
                                        {
                                            MessageBox.Show("Unable to attach to database since it was created with a later version of SQL than the selected server.", "Select another instance", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        }
                                    }
                                    else
                                    {
                                        ProfilerTraceStatusTextBox.AppendText("Unable to load trace data to SQL table.  No local instance able to host the data is available.");
                                        return;
                                    }
                                }
                            }
                            else if (ex.Message.Contains("Unable to open the physical file"))
                            {
                                ProfilerTraceStatusTextBox.Text = "Trace file is not yet imported to database table for analysis.  Import to perform analysis.";
                                return;
                            }
                            else
                                ProfilerTraceStatusTextBox.AppendText("Unable to attach to database due to exception:\r\n" + ex.Message + "\r\n\r\nDeleting .mdf at " + m_analysisPath + " and attempting analysis again will recreate from original captured .trc file if it is still available.\r\n\r\n");
                        }
                    }
                }
                else if (bProfilerTraceDbAttached && chkDettachProfilerAnalysisDBWhenDone.Checked && connSqlDb.State == ConnectionState.Open)
                {
                    SqlCommand cmd = new SqlCommand("IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'" + AnalysisTraceID + "') EXEC master.dbo.sp_detach_db @dbname = N'" + AnalysisTraceID + "'", connSqlDb);
                    try { cmd.ExecuteNonQuery(); } catch (Exception ex) { Debug.WriteLine(ex.Message); }  // could fail if service stopped, no biggie just move on...
                    bProfilerTraceDbAttached = false;
                    if (tcAnalysis.TabPages["tbProfilerTraces"] != null)
                    {
                        (tcAnalysis.TabPages["tbProfilerTraces"].Controls["ProfilerTraceStatusTextbox"] as TextBox).AppendText("\r\nDetached trace database [" + AnalysisTraceID + "]\r\nfrom SQL instance [" + connSqlDb.DataSource + "]\r\nat " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss UTCzzz") + ".\r\n");
                    }
                }
            }
        }
        #endregion AnalysisUI
    }
}