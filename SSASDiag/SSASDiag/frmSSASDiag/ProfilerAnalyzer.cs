using FastColoredTextBoxNS;
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
        #region ProfilerAnalyzerLocals

        ContextMenu mnuProfilerAnalysisContext;
        SqlCommand ProfilerAnalysisQueryCmd;
        DateTime EndOfTrace = DateTime.MinValue;
        System.Windows.Forms.Timer AnalysisQueryExecutionPumpTimer = new System.Windows.Forms.Timer();
        List<ProfilerTraceQuery> ProfilerTraceAnalysisQueries;
        FastColoredTextBox txtProfilerAnalysisQuery = new FastColoredTextBox();

        #endregion ProfilerAnalyzerLocals

        private void cmbProfilerAnalyses_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                txtProfilerAnalysisQuery.Text = (cmbProfilerAnalyses.DataSource as List<ProfilerTraceQuery>).First(kv => kv.Name == cmbProfilerAnalyses.Text).Query.Replace("[Table", "[" + AnalysisTraceID);
                txtProfilerAnalysisDescription.Text = (cmbProfilerAnalyses.DataSource as List<ProfilerTraceQuery>).First(kv => kv.Name == cmbProfilerAnalyses.Text).Description;
                txtProfilerAnalysisQuery.ShowLineNumbers = true;

                if (txtProfilerAnalysisQuery.Text != "")
                {
                    Enabled = false;
                    BackgroundWorker bgLoadProfilerAnalysis = new BackgroundWorker();
                    bgLoadProfilerAnalysis.DoWork += BgLoadProfilerAnalysis_DoWork;
                    bgLoadProfilerAnalysis.RunWorkerCompleted += BgLoadProfilerAnalysis_RunWorkerCompleted;
                    StatusFloater.lblStatus.Text = "Running analysis query. (Esc to cancel...)";
                    StatusFloater.Left = Left + Width / 2 - StatusFloater.Width / 2;
                    StatusFloater.Top = Top + Height / 2 - StatusFloater.Height / 2;
                    StatusFloater.lblTime.Visible = true;
                    StatusFloater.lblTime.Text = "00:00";
                    StatusFloater.EscapePressed = false;
                    lblProfilerAnalysisStatusRight.Text = lblProfilerAnalysisStatusLeft.Text = lblProfilerAnalysisStatusCenter.Text = "";
                    AnalysisQueryExecutionPumpTimer.Interval = 1000;
                    AnalysisQueryExecutionPumpTimer.Start();
                    if (!StatusFloater.Visible)
                        StatusFloater.Show(this);

                    SuspendLayout();
                    bgLoadProfilerAnalysis.RunWorkerAsync();
                }
                else
                {
                    txtProfilerAnalysisQuery.ShowLineNumbers = false;
                    dgdProfilerAnalyses.DataSource = null;
                    dgdProfilerAnalyses.Refresh();
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }
        private void BgLoadProfilerAnalysis_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                connSqlDb.ChangeDatabase(AnalysisTraceID);
                ProfilerAnalysisQueryCmd = new SqlCommand(txtProfilerAnalysisQuery.Text, connSqlDb);
                ProfilerAnalysisQueryCmd.CommandTimeout = 0;
                SqlDataReader dr = ProfilerAnalysisQueryCmd.ExecuteReader();
                DataTable dt = new DataTable();
                dt.Load(dr);
                dgdProfilerAnalyses.Invoke(new System.Action(() =>
                {
                    dgdProfilerAnalyses.AutoGenerateColumns = true;
                    dgdProfilerAnalyses.DataSource = null;
                    dgdProfilerAnalyses.Columns.Clear();
                    dgdProfilerAnalyses.DataSource = dt;
                    dgdProfilerAnalyses.Columns[dgdProfilerAnalyses.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    foreach (DataGridViewColumn c in dgdProfilerAnalyses.Columns)
                        if (c.Width > 300)
                        {
                            c.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                            c.Width = 300;
                        }
                    if (dgdProfilerAnalyses.Columns[dgdProfilerAnalyses.Columns.Count - 1].AutoSizeMode == DataGridViewAutoSizeColumnMode.None)
                        dgdProfilerAnalyses.Columns[dgdProfilerAnalyses.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    if (dgdProfilerAnalyses.Columns[dgdProfilerAnalyses.Columns.Count - 1].Width < 80)
                        dgdProfilerAnalyses.Columns[dgdProfilerAnalyses.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                    dgdProfilerAnalyses.Refresh();
                    int lastCellFullHeaderWidth = dgdProfilerAnalyses.Columns[dgdProfilerAnalyses.Columns.Count - 1].Width;
                    dgdProfilerAnalyses.Columns[dgdProfilerAnalyses.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    dgdProfilerAnalyses.Columns[dgdProfilerAnalyses.Columns.Count - 1].Width = lastCellFullHeaderWidth;
                    lblProfilerAnalysisStatusRight.Text = dt.Rows.Count + " row" + (dt.Rows.Count > 1 ? "s" : "") + " returned.";
                    Int64 TotalDuration = 0;
                    DateTime minStart = DateTime.MaxValue, maxEnd = DateTime.MinValue;
                    if (dgdProfilerAnalyses.Columns["Duration"] != null || dgdProfilerAnalyses.Columns["StartTime"] != null || dgdProfilerAnalyses.Columns["EndTime"] != null 
                        || dgdProfilerAnalyses.Columns["CurrentTime"] != null || dgdProfilerAnalyses.Columns["Requests Completed"] != null)
                    {
                        foreach (DataGridViewRow r in dgdProfilerAnalyses.Rows)
                        {
                            if (dgdProfilerAnalyses.Columns["Duration"] != null && r.Cells["Duration"].FormattedValue as string != "") TotalDuration += Convert.ToInt64(r.Cells["Duration"].Value);
                            if (dgdProfilerAnalyses.Columns["StartTime"] != null && r.Cells["StartTime"].FormattedValue as string != "")
                                if (Convert.ToDateTime(r.Cells["StartTime"].Value) < minStart)
                                    minStart = Convert.ToDateTime(r.Cells["StartTime"].Value);
                            DateTime rowTime = DateTime.MinValue;
                            if (dgdProfilerAnalyses.Columns["EndTime"] != null && r.Cells["EndTime"].FormattedValue as string != "")
                                if (!DateTime.TryParse(r.Cells["EndTime"].Value.ToString(), out rowTime) || rowTime != DateTime.MinValue)
                                    if (rowTime == DateTime.MinValue)  // When we have a row that never finished
                                    {
                                        maxEnd = EndOfTrace;
                                        r.Cells["EndTime"].Style.ForeColor = Color.Red;
                                        r.Cells["Duration"].Style.ForeColor = Color.Red;
                                        r.Cells["Duration"].ToolTipText = "This duration is calculated only until the end of the trace since the request never completed.";
                                    }
                                    else
                                        maxEnd = rowTime;
                            if (dgdProfilerAnalyses.Columns["EndRow"] != null && r.Cells["EndRow"].FormattedValue as string == "")
                            {
                                maxEnd = EndOfTrace;
                                r.Cells["Duration"].Style.ForeColor = Color.Red;
                                r.Cells["Duration"].ToolTipText = "This duration is calculated only until the end of the trace since the request never completed.";
                            }
                            if (dgdProfilerAnalyses.Columns["Requests Completed"] != null && 
                                Convert.ToInt32(r.Cells["Requests Completed"].FormattedValue as string) < Convert.ToInt32(r.Cells["Execution Count"].FormattedValue as string))
                            {
                                r.Cells["Execution Count"].Style.ForeColor = r.Cells["Requests Completed"].Style.ForeColor = Color.Red;
                                r.Cells["Total Duration"].Style.ForeColor = Color.Red;
                                r.Cells["Total Duration"].ToolTipText = "This duration is calculated only until the end of the trace for some requests since not all of these requests completed.";
                            }
                            if (dgdProfilerAnalyses.Columns["CurrentTime"] != null && r.Cells["CurrentTime"].FormattedValue as string != "")
                                if (!DateTime.TryParse(r.Cells["CurrentTime"].Value.ToString(), out rowTime) || rowTime != DateTime.MinValue)
                                    if (rowTime == DateTime.MinValue)  // When we have a row that never finished
                                    {
                                        maxEnd = EndOfTrace;
                                        r.Cells["CurrentTime"].Style.ForeColor = Color.Red;
                                        r.Cells["Duration"].Style.ForeColor = Color.Red;
                                        r.Cells["Duration"].ToolTipText = "This duration is calculated only until the end of the trace since the request never completed.";
                                    }
                                    else
                                        maxEnd = rowTime;
                        }
                        if (TotalDuration > 0)
                            lblProfilerAnalysisStatusLeft.Text = "Total time: " + TimeSpan.FromMilliseconds(TotalDuration).ToString("hh\\:mm\\:ss") + ", Avg: " + TimeSpan.FromMilliseconds(Convert.ToDouble(TotalDuration / dt.Rows.Count)).ToString("hh\\:mm\\:ss");
                        if (minStart < DateTime.MaxValue && maxEnd > DateTime.MinValue)
                        {
                            lblProfilerAnalysisStatusCenter.Text = "Covers " + minStart.ToString("yyyy-MM-dd HH:mm:ss") + " to " + maxEnd.ToString("yyyy-MM-dd HH:mm:ss");
                            lblProfilerAnalysisStatusCenter.Left = Width / 2 - lblProfilerAnalysisStatusCenter.Width / 2;
                        }
                    }
                    lblProfilerAnalysisStatusRight.Left = Width - lblProfilerAnalysisStatusRight.Width - 41;

                    // Finally setup context menus after loading analysis query.
                    mnuProfilerAnalysisContext = new ContextMenu();
                    mnuProfilerAnalysisContext.MenuItems.Add(new MenuItem("Copy", ProfilerAnalysisContextMenu_Click));
                    mnuProfilerAnalysisContext.MenuItems.Add(new MenuItem("Copy with Headers", ProfilerAnalysisContextMenu_Click));

                    if (dt.Columns.Contains("RowNumber") || dt.Columns.Contains("StartRow") || dt.Columns.Contains("EndRow"))
                    {
                        mnuProfilerAnalysisContext.MenuItems.Add(new MenuItem("-"));
                        mnuProfilerAnalysisContext.MenuItems.Add(new MenuItem(string.Format("Find all queries/commands overlapping with selection"), ProfilerAnalysisContextMenu_Click));
                        if (cmbProfilerAnalyses.Text.ToLower().Contains("quer") && bProfilerQueryStatsPresent)
                            mnuProfilerAnalysisContext.MenuItems.Add(new MenuItem(string.Format("Lookup query statistics for selected queries"), ProfilerAnalysisContextMenu_Click));
                        mnuProfilerAnalysisContext.MenuItems.Add(new MenuItem(string.Format("Lookup detail rows for selected queries/commands"), ProfilerAnalysisContextMenu_Click));
                    }
                }));
            }
            catch (Exception ex)
            {
                LogException(ex);
                if (ex.Message.Contains("cancelled by user"))
                {
                    connSqlDb.Close();
                    connSqlDb.Open();
                }
            }
        }
        private void AnalysisQueryExecutionPumpTimer_Tick(object sender, EventArgs e)
        {
            string curTime = StatusFloater.lblTime.Text;
            string[] timeparts = StatusFloater.lblTime.Text.Split(':');
            TimeSpan newTime = (TimeSpan.FromMinutes(Convert.ToDouble(timeparts[0])) + TimeSpan.FromSeconds(Convert.ToDouble(timeparts[1])).Add(TimeSpan.FromSeconds(1)));
            StatusFloater.lblTime.Text = newTime.ToString("mm\\:ss");
            if (StatusFloater.EscapePressed)
                ProfilerAnalysisQueryCmd.Cancel();
        }
        private void BgLoadProfilerAnalysis_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Invoke(new System.Action(() =>
            {
                if (StatusFloater.EscapePressed)
                {
                    cmbProfilerAnalyses.SelectedIndex = 0;
                    lblProfilerAnalysisStatusRight.Text = "Last query was cancelled.";
                    lblProfilerAnalysisStatusRight.Left = Width - lblProfilerAnalysisStatusRight.Width - 41;
                }
                Enabled = true;
                StatusFloater.Visible = false;
                StatusFloater.lblTime.Visible = false;
                StatusFloater.EscapePressed = false;
                dgdProfilerAnalyses.ClearSelection();
                AnalysisQueryExecutionPumpTimer.Stop();
                ResumeLayout();
            }));
        }
        private void dgdProfilerAnalyses_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (e.ColumnIndex != -1 && e.RowIndex != -1)
                {

                    if (dgdProfilerAnalyses.SelectedCells.Count == 0)
                        dgdProfilerAnalyses.CurrentCell = dgdProfilerAnalyses[e.ColumnIndex, e.RowIndex];
                    Rectangle rect = dgdProfilerAnalyses.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
                    mnuProfilerAnalysisContext.Show(dgdProfilerAnalyses, new Point(rect.X, rect.Y));
                }
            }
        }
        private bool ProcessCopyMenuClicks(object sender)
        {
            string sOut = "";
            int PriorRowNum = -1;
            if ((sender as MenuItem).Text == "Copy")
            {
                List<DataGridViewCell> cells = new List<DataGridViewCell>();
                foreach (DataGridViewCell c in dgdProfilerAnalyses.SelectedCells)
                    cells.Add(c);
                cells = cells.OrderBy(c => c.ColumnIndex).OrderBy(c => c.RowIndex).ToList();
                foreach (DataGridViewCell c in cells)
                {
                    if (PriorRowNum != c.RowIndex && PriorRowNum != -1)
                        sOut += "\r\n" + (c.Value is DBNull ? "NULL" : c.Value + "");
                    else
                        if (PriorRowNum == -1)
                        sOut = (c.Value is DBNull ? "NULL" : c.Value + "");
                    else
                        sOut += ", " + (c.Value is DBNull ? "NULL" : c.Value + "");
                    PriorRowNum = c.RowIndex;
                }
                Clipboard.SetText(sOut);
                return true;
            }
            if ((sender as MenuItem).Text == "Copy with Headers")
            {
                List<DataGridViewCell> cells = new List<DataGridViewCell>();
                foreach (DataGridViewCell c in dgdProfilerAnalyses.SelectedCells)
                    cells.Add(c);
                cells = cells.OrderBy(c => c.ColumnIndex).OrderBy(c => c.RowIndex).ToList();
                string headers = "", sLine = "", priorHeaders = "";
                foreach (DataGridViewCell c in cells)
                {
                    if (PriorRowNum != c.RowIndex || PriorRowNum == -1)
                    {
                        if (PriorRowNum != -1)
                        {
                            sOut += "\r\n" + (headers != priorHeaders ? headers + "\r\n" : "") + sLine;
                            priorHeaders = headers;
                            sLine = "";
                            headers = "";
                        }
                        headers = dgdProfilerAnalyses.Columns[c.ColumnIndex].HeaderCell.Value + "";
                        sLine = (c.Value is DBNull ? "NULL" : c.Value + "");
                    }
                    else
                    {
                        headers += ", " + dgdProfilerAnalyses.Columns[c.ColumnIndex].HeaderCell.Value + "";
                        sLine += ", " + (c.Value is DBNull ? "NULL" : c.Value + "");
                    }
                    PriorRowNum = c.RowIndex;
                }
                sOut += "\r\n" + (headers != priorHeaders ? headers + "\r\n" : "") + sLine;
                Clipboard.SetText(sOut.TrimStart(new char[] { '\r', '\n' }));
                return true;
            }
            return false;
        }
        private void ProfilerAnalysisContextMenu_Click(object sender, EventArgs e)
        {
            if (ProcessCopyMenuClicks(sender))
                return;

            List<int?> rows = new List<int?>();
            foreach (DataGridViewCell c in dgdProfilerAnalyses.SelectedCells)
            {
                if (
                        (
                            (sender as MenuItem).Text.ToLower().Contains("query statistics") &&
                            dgdProfilerAnalyses.Columns.Contains("EventClass") &&
                            (dgdProfilerAnalyses.Rows[c.RowIndex].Cells["EventClass"].Value as int? == 9 ||
                            dgdProfilerAnalyses.Rows[c.RowIndex].Cells["EventClass"].Value as int? == 10 ||
                            dgdProfilerAnalyses.Rows[c.RowIndex].Cells["EventClass"].Value as int? == 15 ||
                            dgdProfilerAnalyses.Rows[c.RowIndex].Cells["EventClass"].Value as int? == 16)
                        ) ||
                        !(sender as MenuItem).Text.ToLower().Contains("query statistics")
                    )
                {
                    if (dgdProfilerAnalyses.Columns.Contains("RowNumber"))
                        rows.Add(dgdProfilerAnalyses.Rows[c.RowIndex].Cells["RowNumber"].Value as int?);
                    else if (dgdProfilerAnalyses.Columns.Contains("EndRow") && dgdProfilerAnalyses.Rows[c.RowIndex].Cells["EndRow"].Value != null)
                        rows.Add(dgdProfilerAnalyses.Rows[c.RowIndex].Cells["EndRow"].Value as int?);
                    else if (dgdProfilerAnalyses.Columns.Contains("StartRow"))
                        rows.Add(dgdProfilerAnalyses.Rows[c.RowIndex].Cells["StartRow"].Value as int?);
                }
            }
            rows = rows.Distinct().ToList();
            if (rows.Count == 0)
                return;
            string strQry = "";
            cmbProfilerAnalyses.SelectedIndex = 0;
            if ((sender as MenuItem).Text == "Find all queries/commands overlapping with selection")
            {
                foreach (int row in rows)
                {
                    strQry += (strQry == "" ? ("select a.RowNumber, a.Duration, a.EventClass, a.EventClassName, a.CurrentTime, a.StartTime, a.ConnectionID, a.NTUserName, a.NTDomainName, a.DatabaseName, a.TextData, a.ClientProcessID, a.ApplicationName, a.CPUTime, a.EventSubclass, a.SPID, convert(nvarchar(max), a.RequestParameters), convert(nvarchar(max), a.RequestProperties)\r\nfrom [Table_v] a,\r\n(select StartTime, CurrentTime from [Table] where RowNumber = " + row + ") b\r\nwhere a.eventclass in (10, 16)\r\nand a.CurrentTime >= b.StartTime and a.CurrentTime <= b.CurrentTime").Replace("[Table", "[" + AnalysisTraceID)
                                 : ("\r\nunion\r\nselect a.RowNumber, a.Duration, a.EventClass, a.EventClassName, a.CurrentTime, a.StartTime, a.ConnectionID, a.NTUserName, a.NTDomainName, a.DatabaseName, a.TextData, a.ClientProcessID, a.ApplicationName, a.CPUTime, a.EventSubclass, a.SPID, convert(nvarchar(max), a.RequestParameters), convert(nvarchar(max), a.RequestProperties)\r\nfrom [Table_v] a,\r\n(select StartTime, CurrentTime from [Table] where RowNumber = " + row + ") b\r\nwhere a.eventclass in (10, 16)\r\nand a.CurrentTime >= b.StartTime and a.CurrentTime <= b.CurrentTime").Replace("[Table", "[" + AnalysisTraceID));
                }
                txtProfilerAnalysisQuery.Text = "--All queries started or finished during the execution of the quer" + (rows.Count > 1 ? "ies" : "y") + " at row" + (rows.Count > 1 ? "s " : " ") + String.Join(", ", rows.ToArray()) + ".\r\n\r\n" + strQry + "\r\norder by duration desc, starttime desc";
                if (txtProfilerAnalysisQuery.Text != "")
                {
                    BackgroundWorker bgLoadProfilerAnalysis = new BackgroundWorker();
                    bgLoadProfilerAnalysis.DoWork += BgLoadProfilerAnalysis_DoWork;
                    bgLoadProfilerAnalysis.RunWorkerCompleted += BgLoadProfilerAnalysis_RunWorkerCompleted;
                    StatusFloater.lblStatus.Text = "Running analysis query...";
                    StatusFloater.Left = Left + Width / 2 - StatusFloater.Width / 2;
                    StatusFloater.Top = Top + Height / 2 - StatusFloater.Height / 2;
                    StatusFloater.Show(this);
                    SuspendLayout();
                    bgLoadProfilerAnalysis.RunWorkerAsync();
                }
                else
                {
                    dgdProfilerAnalyses.DataSource = null;
                    dgdProfilerAnalyses.Refresh();
                }
            }
            else if ((sender as MenuItem).Text == "Lookup query statistics for selected queries")
            {
                foreach (int row in rows)
                {
                    string strBase = Properties.Resources.QueryFESEStats.Replace("[Table", "[" + AnalysisTraceID).Replace("ORDER BY QueryDuration", "") + "WHERE StartRow = " + row + " OR EndRow = " + row;
                    strQry += (strQry == "" ? strBase : "\r\nunion\r\n" + strBase);
                }
                txtProfilerAnalysisQuery.Text = strQry;
                if (txtProfilerAnalysisQuery.Text != "")
                {
                    BackgroundWorker bgLoadProfilerAnalysis = new BackgroundWorker();
                    bgLoadProfilerAnalysis.DoWork += BgLoadProfilerAnalysis_DoWork;
                    bgLoadProfilerAnalysis.RunWorkerCompleted += BgLoadProfilerAnalysis_RunWorkerCompleted;
                    StatusFloater.lblStatus.Text = "Running analysis query...";
                    StatusFloater.Left = Left + Width / 2 - StatusFloater.Width / 2;
                    StatusFloater.Top = Top + Height / 2 - StatusFloater.Height / 2;
                    StatusFloater.Show(this);
                    SuspendLayout();
                    bgLoadProfilerAnalysis.RunWorkerAsync();
                }
                else
                {
                    dgdProfilerAnalyses.DataSource = null;
                    dgdProfilerAnalyses.Refresh();
                }
            }
            else if ((sender as MenuItem).Text == "Lookup detail rows for selected queries/commands")
            {
                foreach (int row in rows)
                {
                    string strBase = "";
                    if (!bProfilerQueryStatsPresent)
                        strBase = ConvertProfilerEventClassSubclassViewQueryToSimpleTableQuery(Properties.Resources.DrillThroughQueryAllRowsForQueryOrCommand).Replace("<RowNumber/>", Convert.ToString(row));
                    else
                        strBase = Properties.Resources.DrillThroughQueryAllRowsForQueryOrCommand.Replace("[Table", "[" + AnalysisTraceID).Replace("<RowNumber/>", Convert.ToString(row));
                    strQry += (strQry == "" ? strBase : "\r\nunion\r\n" + strBase);
                }
                txtProfilerAnalysisQuery.Text = strQry + "\r\norder by RowNumber";
                if (txtProfilerAnalysisQuery.Text != "")
                {
                    BackgroundWorker bgLoadProfilerAnalysis = new BackgroundWorker();
                    bgLoadProfilerAnalysis.DoWork += BgLoadProfilerAnalysis_DoWork;
                    bgLoadProfilerAnalysis.RunWorkerCompleted += BgLoadProfilerAnalysis_RunWorkerCompleted;
                    StatusFloater.lblStatus.Text = "Running analysis query...";
                    StatusFloater.Left = Left + Width / 2 - StatusFloater.Width / 2;
                    StatusFloater.Top = Top + Height / 2 - StatusFloater.Height / 2;
                    StatusFloater.Show(this);
                    SuspendLayout();
                    bgLoadProfilerAnalysis.RunWorkerAsync();
                }
                else
                {
                    dgdProfilerAnalyses.DataSource = null;
                    dgdProfilerAnalyses.Refresh();
                }
            }
        }
        private void splitProfilerAnalysis_Panel2_SizeChanged(object sender, System.EventArgs e)
        {
            dgdProfilerAnalyses.Height = splitProfilerAnalysis.Panel2.Height - pnlProfilerAnalysisStatus.Height;
        }
        private void splitProfilerAnalysis_Panel1_SizeChanged(object sender, System.EventArgs e)
        {
            txtProfilerAnalysisDescription.Height = splitProfilerAnalysis.Panel1.Height - 56;
        }
        private void SetupSQLTextbox()
        {
            //txtProfilerAnalysisQuery.Language = Language.SQL;

            splitProfilerAnalysis.Panel1.Controls.Add(txtProfilerAnalysisQuery);
            txtProfilerAnalysisQuery.Dock = System.Windows.Forms.DockStyle.Right;
            txtProfilerAnalysisQuery.ChangeFontSize(-2);
            txtProfilerAnalysisQuery.Location = new System.Drawing.Point(216, 0);
            txtProfilerAnalysisQuery.Multiline = true;
            txtProfilerAnalysisQuery.BackColor = SystemColors.Control;
            txtProfilerAnalysisQuery.TextChanged += TxtProfilerAnalysisQuery_TextChanged;
            txtProfilerAnalysisQuery.WordWrapMode = WordWrapMode.WordWrapControlWidth;
            txtProfilerAnalysisQuery.ShowLineNumbers = false;
            txtProfilerAnalysisQuery.WordWrapAutoIndent = true;
            txtProfilerAnalysisQuery.WordWrap = true;
            txtProfilerAnalysisQuery.WordWrapIndent = 3;
            txtProfilerAnalysisQuery.ReadOnly = true;
            txtProfilerAnalysisQuery.ShowScrollBars = true;
            txtProfilerAnalysisQuery.Size = new System.Drawing.Size(363, 103);
            txtProfilerAnalysisQuery.TabIndex = 29;
        }
        TextStyle bracketsStyle = new TextStyle(Brushes.Black, Brushes.LightGray, FontStyle.Regular);
        private void TxtProfilerAnalysisQuery_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtProfilerAnalysisQuery.SyntaxHighlighter.SQLSyntaxHighlight(txtProfilerAnalysisQuery.Range);
            e.ChangedRange.ClearStyle(bracketsStyle);
            e.ChangedRange.SetStyle(bracketsStyle, "(?<=\\[)(.*?)(?=\\])");
            txtProfilerAnalysisQuery.Language = Language.SQL | Language.Custom;
        }
    }
}