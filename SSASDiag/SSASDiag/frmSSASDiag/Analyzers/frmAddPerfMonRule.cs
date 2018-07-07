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
using System.Text.RegularExpressions;
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

namespace SSASDiag
{
    public partial class frmAddPerfMonRule : Form
    {
        TreeView tvCounters;
        ucASPerfMonAnalyzer.Rule originRule = null;
        ucASPerfMonAnalyzer HostControl = ((Program.MainForm.tcCollectionAnalysisTabs.TabPages[1].Controls["tcAnalysis"] as TabControl).TabPages["Performance Logs"].Controls[0] as ucASPerfMonAnalyzer);
        Color WarnColor, ErrorColor, PassColor = Color.Empty;

        public frmAddPerfMonRule(ucASPerfMonAnalyzer.Rule r = null)
        {
            originRule = r;

            InitializeComponent();

            // 
            // tvCounters
            // 
            tvCounters = new TreeView();
            tvCounters.CheckBoxes = false;
            tvCounters.Dock = System.Windows.Forms.DockStyle.Fill;
            tvCounters.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            tvCounters.Name = "tvCounters";
            tvCounters.TabIndex = 3;
            tvCounters.Margin = new Padding(0);
            tvCounters.ShowNodeToolTips = true;
            tvCounters.AllowDrop = true;
            tvCounters.ItemDrag += TvCounters_ItemDrag;
            tvCounters.DragEnter += TvCounters_DragEnter;
            tvCounters.DragDrop += TvCounters_DragDrop;
            splitCounters.Panel1.Controls.Add(tvCounters);
            foreach (TreeNode node in HostControl.tvCounters.Nodes)
                tvCounters.Nodes.Add(node.Clone() as TreeNode);
            tt.SetToolTip(tvCounters, "Tip: To select counters by alternate groupings, exit this dialog, reorder headers above the counter treeview in the main browser, then reopen this dialog.");

            dgdExpressions.Columns[0].CellTemplate = new DataGridViewTextBoxColumnWithExpandedEditArea();
            dgdExpressions.Columns[1].CellTemplate = new DataGridViewTextBoxColumnWithExpandedEditArea();
            dgdExpressions.Rows.Clear();
            WarnColor = Color.FromArgb(255, Color.Khaki);
            ErrorColor = Color.FromArgb(255, Color.Pink);
            PassColor = Color.FromArgb(255, Color.LightGreen);
            cmbFailIfValueAboveBelow.SelectedIndex = 0;
        }

        /* Drag & Drop */
        #region
        private Rectangle dragBoxFromMouseDown;
        private object valueFromMouseDown;

        private void dgdSelectedCounters_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void TvCounters_DragDrop(object sender, DragEventArgs e)
        {
            DataGridViewRow r = (DataGridViewRow)e.Data.GetData("System.Windows.Forms.DataGridViewRow");
            AddCountersBackToTreeViewFromGrid(r);
            dgdSelectedCounters.Rows.Remove(r);
            foreach (DataGridViewRow row in dgdExpressions.Rows)
                if (row.Index != dgdExpressions.Rows.Count - 1)
                    foreach (DataGridViewCell c in r.Cells)
                        dgdExpressions_CellEndEdit(dgdExpressions, new DataGridViewCellEventArgs(c.ColumnIndex, row.Index));
            UpdateExpressionsAndCountersCombo();
        }

        private void AddCountersBackToTreeViewFromGrid(DataGridViewRow r)
        {
            if (r.Tag != null)
            {
                TreeNode node = r.Tag as TreeNode;
                string[] parts = (r.Cells[0].Value as string).Split('\\');
                TreeNode newNode = tvCounters.Nodes[parts[0]];
                for (int i = 1; i < parts.Count() - 2; i++)
                    if (parts[i] != "*")
                        newNode = newNode.Nodes[parts[i]];
                if (newNode == null)
                {
                    for (int i = 0; i < tvCounters.Nodes.Count; i++)
                    {
                        if (tvCounters.Nodes[i].Name.CompareTo(node.Name) == 1)
                        {
                            tvCounters.Nodes.Insert(i, node);
                            break;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < newNode.Nodes.Count; i++)
                    {
                        if (newNode.Nodes[i].Name.CompareTo(node.Name) == 1)
                        {
                            newNode.Nodes.Insert(i, node);
                            break;
                        }
                    }
                }
            }
        }

        private void TvCounters_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void TvCounters_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void dgdSelectedCounters_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("System.Windows.Forms.TreeNode", false))
            {
                TreeNode node = (TreeNode)e.Data.GetData("System.Windows.Forms.TreeNode");
                string CounterPath = node.FullPath;
                string[] parts = CounterPath.Split('\\');
                if (node.Nodes.Count > 0)
                {
                    CounterPath += "\\*";
                    if (node.Nodes[0].Nodes.Count > 0)
                        CounterPath += "\\*";
                }
                TreeNode newNode = tvCounters.Nodes[parts[0]];
                for (int i = 1; i < parts.Count(); i++)
                    newNode = newNode.Nodes[parts[i]];
                dgdSelectedCounters.Rows.Add();
                DataGridViewRow r = dgdSelectedCounters.Rows[dgdSelectedCounters.Rows.Count - 1];
                if (!CounterPath.Contains("*"))
                    r.Cells[3].ReadOnly = true;
                r.Cells[1].Value = true;
                r.Tag = newNode;
                if (newNode.Parent == null)
                    tvCounters.Nodes.Remove(newNode);
                else
                    newNode.Parent.Nodes.Remove(newNode);
                r.Cells[0].Value = CounterPath;
                UpdateExpressionsAndCountersCombo();
            }
        }

        private void ValidateExpressionCombosAfterUpdate(ComboBox cb)
        {
            
            for (int i = 0; i < cb.Items.Count; i++)
            {
                bool bValExists = false;
                string cbi = cb.Items[i] as string;
                foreach (DataGridViewRow expr in dgdExpressions.Rows.Cast<DataGridViewRow>().Where(r => r.Cells[0].Value != null))
                    if (expr.Cells[0].Value as string == cbi && expr.Cells[1].ErrorText == "")
                    {
                        bValExists = true;
                        break;
                    }
                if (!bValExists)
                {
                    if (cb.SelectedIndex == i)
                        cb.SelectedIndex = -1;
                    cb.Items.RemoveAt(i);
                    i--;
                }
            }
        }

        private void UpdateExpressionsAndCountersCombo()
        {
            btnSaveRule.Enabled = false;
            foreach (DataGridViewRow r in dgdSelectedCounters.Rows)
                if (r.Cells[0].Value != null && !cmbValueToCheck.Items.Contains(r.Cells[0].Value as string))
                    cmbValueToCheck.Items.Add(r.Cells[0].Value as string);
            foreach (DataGridViewRow r in dgdExpressions.Rows)
            {
                if (r.Cells[0].Value != null && r.Cells[0].ErrorText == "" && r.Cells[1].ErrorText == "" && !cmbValueToCheck.Items.Contains(r.Cells[0].Value as string))
                {
                    cmbValueToCheck.Items.Add(r.Cells[0].Value as string);
                    cmbValueToCheck.Enabled = true;
                }
                if (r.Tag as string == "Scalar")
                {
                    if (!cmbValLow.Items.Contains(r.Cells[0].Value as string))
                    {
                        cmbValLow.Items.Add(r.Cells[0].Value as string);
                        cmbValHigh.Items.Add(r.Cells[0].Value as string);
                        cmbWarnExpr.Items.Add(r.Cells[0].Value as string);
                    }
                    cmbValLow.Enabled = cmbWarnExpr.Enabled = cmbValHigh.Enabled = true;
                }
                else if (r.Tag as string == "Series")
                {
                    if (cmbValLow.Items.Contains(r.Cells[0].Value as string))
                    {
                        cmbValLow.Items.Remove(r.Cells[0].Value as string);
                        cmbValHigh.Items.Remove(r.Cells[0].Value as string);
                        cmbWarnExpr.Items.Remove(r.Cells[0].Value as string);
                    }
                    cmbValLow.Enabled = cmbWarnExpr.Enabled = cmbValHigh.Enabled = false;
                }
            }

            // Update the value if we removed an item.
            bool bValExists;
            for (int i = 0; i < cmbValueToCheck.Items.Count; i++)
            {
                string cbi = cmbValueToCheck.Items[i] as string;
                bValExists = false;
                foreach (string ctr in dgdSelectedCounters.Rows.Cast<DataGridViewRow>().Select(r => r.Cells[0].Value as string))
                    if (ctr == cbi)
                    {
                        bValExists = true;
                        break;
                    }
                foreach (DataGridViewRow expr in dgdExpressions.Rows.Cast<DataGridViewRow>().Where(r=>r.Cells[0].Value != null))
                    if (expr.Cells[0].Value as string == cbi && expr.Cells[1].ErrorText == "")
                    {
                        bValExists = true;
                        break;
                    }
                if (!bValExists)
                {
                    if (cmbValueToCheck.SelectedIndex == i)
                        cmbValueToCheck.SelectedIndex = -1;
                    cmbValueToCheck.Items.RemoveAt(i);
                    i--;
                }
            }

            // Also update the error/warn/pass expression combos...
            ValidateExpressionCombosAfterUpdate(cmbValHigh);
            ValidateExpressionCombosAfterUpdate(cmbWarnExpr);
            ValidateExpressionCombosAfterUpdate(cmbValLow);

            if (cmbValueToCheck.Items.Count == 0)
                cmbValueToCheck.Enabled = false;
            else
                cmbValueToCheck.Enabled = true;

            if (cmbValHigh.Items.Count == 0)
                cmbValLow.Enabled = cmbValHigh.Enabled = cmbWarnExpr.Enabled = false;
            else
                cmbValLow.Enabled = cmbValHigh.Enabled = cmbWarnExpr.Enabled = true;
            lblSeriesFunction.Visible = cmbSeriesFunction.Visible = cmbValueToCheck.SelectedIndex != -1 && dgdExpressions.Rows.Cast<DataGridViewRow>().Where(r => r.Cells[0].Value as string == cmbValueToCheck.SelectedItem as string).Count() == 0;
            lblPctMatchCheck.Visible = udPctMatchCheck.Visible = cmbSeriesFunction.Visible && cmbSeriesFunction.SelectedIndex == cmbSeriesFunction.Items.Count - 1;
            btnSaveRule.Enabled = IsRuleComplete();
            ValidateChildren();
        }

        private void dgdSelectedCounters_MouseDown(object sender, MouseEventArgs e)
        {
            var hittestInfo = dgdSelectedCounters.HitTest(e.X, e.Y);

            if (hittestInfo.RowIndex != -1 && hittestInfo.ColumnIndex != -1)
            {
                valueFromMouseDown = dgdSelectedCounters.Rows[hittestInfo.RowIndex];
                if (valueFromMouseDown != null)
                {
                    Size dragSize = SystemInformation.DragSize;
                    dragBoxFromMouseDown = new Rectangle(new Point(e.X - (dragSize.Width / 2), e.Y - (dragSize.Height / 2)), dragSize);
                }
            }
            else
                dragBoxFromMouseDown = Rectangle.Empty;
        }

        private void dgdSelectedCounters_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                // If the mouse moves outside the rectangle, start the drag.
                if (dragBoxFromMouseDown != Rectangle.Empty && !dragBoxFromMouseDown.Contains(e.X, e.Y))
                {
                    // Proceed with the drag and drop, passing in the list item.                    
                    DragDropEffects dropEffect = dgdSelectedCounters.DoDragDrop(valueFromMouseDown, DragDropEffects.Move);
                }
            }
        }

        private void dgdExpressions_DragDrop(object sender, DragEventArgs e)
        {
            DataGridViewRow r = (DataGridViewRow)e.Data.GetData("System.Windows.Forms.DataGridViewRow");
            if (r != null)
            {
                if (dgdExpressions.CurrentRow == null)
                {
                    if (dgdExpressions.Rows.Count == 0)
                        dgdExpressions.Rows.Add();
                    dgdExpressions.CurrentCell = dgdExpressions.Rows[dgdExpressions.Rows.Count - 1].Cells[1];
                }
                string existingVal = (dgdExpressions.CurrentRow.Cells[1].Value as string);
                if (existingVal != null)
                    existingVal = existingVal.Trim();
                else
                    existingVal = "";

                if (dgdExpressions.CurrentRow.Cells[0].Value == null)
                    dgdExpressions.CurrentRow.Cells[0].Value = "Expr" + dgdExpressions.CurrentRow.Index;
                dgdExpressions.CurrentRow.Cells[1].Value = existingVal + (existingVal == "" ? "" : " ") + "[" + r.Cells[0].Value + "]";
                dgdExpressions.CurrentCell = dgdExpressions.CurrentRow.Cells[1];
                dgdExpressions.BeginEdit(false);
                SendKeys.Send(" {BS}");
            }
        }

        private void dgdExpressions_DragEnter(object sender, DragEventArgs e)
        {
            DataGridViewRow r = (DataGridViewRow)e.Data.GetData("System.Windows.Forms.DataGridViewRow");
            if (r != null)
                e.Effect = DragDropEffects.Move;
            else
                e.Effect = DragDropEffects.None;
        }
        #endregion

        private bool IsRuleComplete()
        {
            if (txtName.Text.Trim() == "" || txtDescription.Text.Trim() == "" || txtCategory.Text.Trim() == "" ||
                txtHighRegion.Text.Trim() == "" || txtHighResult.Text.Trim() == "" ||
                txtLowRegion.Text.Trim() == "" || txtLowResult.Text.Trim() == "" ||
                dgdSelectedCounters.Rows.Count == 0 || dgdExpressions.Rows.Count == 0 ||
                cmbValueToCheck.SelectedIndex == -1 ||
                (cmbSeriesFunction.Visible && cmbSeriesFunction.SelectedIndex == -1) ||
                (cmbFailIfValueAboveBelow.SelectedIndex == 1 && cmbValHigh.SelectedIndex == -1)  ||
                (cmbFailIfValueAboveBelow.SelectedIndex == 0 && cmbValLow.SelectedIndex == -1) ||
                (cmbWarnExpr.SelectedIndex >= 0 && (txtWarnRegion.Text.Trim() == "" || txtWarnResult.Text.Trim() == "")))
                return false;
            foreach (DataGridViewRow r in dgdExpressions.Rows)
                if (r.Cells[0].ErrorText != "" || r.Cells[1].ErrorText != "")
                    return false;
            return true;
        }

        List<TreeNode> ListNodes(TreeView tv)
        {
            List<TreeNode> nodes = new List<TreeNode>();
            foreach (TreeNode subnode in tv.Nodes)
            {
                if (subnode.Nodes.Count > 0)
                    nodes.AddRange(ListNodes(subnode));
                else
                    nodes.Add(subnode);
            }
            return nodes;
        }
        List<TreeNode> ListNodes(TreeNode node)
        {
            List<TreeNode> nodes = new List<TreeNode>();
            if (node.Nodes.Count == 0) nodes.Add(node);
            foreach (TreeNode subnode in node.Nodes)
                nodes.AddRange(ListNodes(subnode));
            return nodes;
        }

        string BreakdownExpression(string expr, int CurrentRowIndex)
        {
            int iCurPos = 0;
            dgdExpressions.Rows[CurrentRowIndex].Tag = "Scalar";
            MatchCollection Counters = Regex.Matches(expr, "(\\[.*?\\])");
            foreach (Match c in Counters)
            {
                if (dgdSelectedCounters.Rows.Cast<DataGridViewRow>().Where(r => "[" + r.Cells[0].Value as string + "]" == c.Value as string).Count() == 0)
                    return "Invalid counter expression.\n\"Square brackets\" [ ] should only surround valid counter names from the rule counters chosen above.\nDrag rule counters onto the expression to add them directly.";
                iCurPos = expr.IndexOf(c.Value);
                while (iCurPos != -1)
                {
                    iCurPos += c.Value.Length;
                    string function = "";
                    if (expr.Length > iCurPos && expr.Substring(iCurPos++, 1) == ".")
                    {
                        char breakchar = '\0';
                        while (breakchar != ' ' && iCurPos < expr.Length)
                            breakchar = expr.Substring(iCurPos++, 1)[0];

                        function = expr.Substring(expr.IndexOf(c.Value), iCurPos - expr.IndexOf(c.Value)).Replace(c.Value + ".", "").Replace(" ", "").Replace("(", "").Replace(")", "");
                        if (!function.ToLower().In(new string[] { "first", "last", "max", "min", "avg", "avgincludenull", "avgexcludezero", "count" }))
                            return "Invalid counter function at: ." + function;
                    }
                    else
                        dgdExpressions.Rows[CurrentRowIndex].Tag = "Series";
                    string RestOfExpression = "";
                    if (expr.Length > expr.IndexOf(c.Value) + c.Value.Length + function.Length + 1)
                        RestOfExpression = expr.Substring(expr.IndexOf(c.Value) + c.Value.Length + function.Length + 1);
                    expr = expr.Substring(0, expr.IndexOf(c.Value)) + (12345.67890 + iCurPos).ToString() + RestOfExpression; // plug in a number nobody will actually ever use in an expression.  big hack but sufficient to cover our simple uses without ever failing unless user created a very intentional expression, and then worst case we just crash or error.
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
                            {
                                iCurPos++;
                                break;
                            }
                            if (dgdExpressions.Rows.Cast<DataGridViewRow>().Where(r => r.Cells[0].Value != null && (r.Cells[0].Value as string).ToLower() == subExpr.ToLower() && r.Index < CurrentRowIndex && r.Cells[1].ErrorText == "").Count() > 0)
                                expr = expr.Replace(subExpr, (12345.67890 + iCurPos).ToString());
                            else
                            {
                                if (dgdExpressions.Rows.Cast<DataGridViewRow>().Where(r => r.Cells[0].Value != null && (r.Cells[0].Value as string).ToLower() == subExpr.ToLower() && r.Cells[1].ErrorText == "").Count() > 0)
                                    return "Expression '" + subExpr.TrimStart().TrimEnd() + "' invalid.\nExpressions may only refer to prior expressions in the list.";
                                else if (dgdExpressions.Rows.Cast<DataGridViewRow>().Where(r => r.Cells[0].Value != null && (r.Cells[0].Value as string).ToLower() == subExpr.ToLower()).Count() > 0)
                                    return "Expression '" + subExpr.TrimStart().TrimEnd() + "' is invalid.";
                                else
                                    return "Invalid token at: '" + subExpr + "'.";
                            }

                            if (Regex.Matches(expr, @"[a-zA-Z]").Count == 0)
                                try { new DataTable().Compute(expr, ""); }
                                catch { return "Invalid token at end of expression."; }
                            iCurPos = -1;
                            iWordStart = -1;
                        }
                        iCurPos++;
                    }
                }
            }
            if (iWordStart != -1)
            {
                string exp = expr.Substring(iWordStart, iCurPos - iWordStart).ToLower();
                double d;
                if (!double.TryParse(exp, out d))
                {
                    if (dgdExpressions.Rows.Cast<DataGridViewRow>().Where(r => r.Cells[0].Value != null && (r.Cells[0].Value as string).ToLower() == exp && r.Index < CurrentRowIndex).Count() > 0)
                        expr = expr.Replace(exp, (12345.67890 + iCurPos).ToString());
                    else
                    {
                        if (dgdExpressions.Rows.Cast<DataGridViewRow>().Where(r => r.Cells[0].Value != null && (r.Cells[0].Value as string).ToLower() == exp).Count() > 0)
                            return "Expression '" + exp.TrimStart().TrimEnd() + "' invalid.\nExpressions may only refer to prior expressions in the list.";
                        else
                            return "Invalid token at: '" + exp + "'.";
                    }
                }
            }
            if (Regex.Matches(expr, @"[a-zA-Z]").Count == 0)
                try { new DataTable().Compute(expr, ""); }
                catch { return "Invalid token at end of expression."; }  // If the expression fails to parse after replacing all subexpressions and counters with dummy values, then it is a bad expression.
            else
            {
                // Occurs if expression was series.  Does it matter and we can remove the whole clause?
            }
            return "";
        }

        private void dgdExpressions_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex != -1 && e.ColumnIndex != -1 && e.RowIndex != dgdExpressions.Rows.Count - 1)
            {
                dgdExpressions.EndEdit();
                DataGridViewCell cell = dgdExpressions.Rows[e.RowIndex].Cells[1] as DataGridViewCell;
                cell.ErrorText = "";
                if (cell.Value == null)
                    cell.ErrorText = "Expression required.";
                else
                    cell.ErrorText = BreakdownExpression(cell.Value as string, e.RowIndex);
                UpdateExpressionsAndCountersCombo();
                cell = dgdExpressions.Rows[e.RowIndex].Cells[0] as DataGridViewCell;
                cell.ErrorText = "";
                if (cell.Value == null)
                    cell.ErrorText = "Name required.";
                else
                {
                    string val = cell.Value as string;
                    val = val.TrimStart().TrimEnd();
                    if (!char.IsLetter(val[0]))
                        cell.ErrorText = "Expression names must start with an alphabetic character.";
                    foreach (char c in val)
                        if (!char.IsLetterOrDigit(c))
                            cell.ErrorText = "Expression names can only contain alphanumeric characters.";
                }
                
                if (sender != null)
                {
                    foreach (DataGridViewRow r in dgdExpressions.Rows)
                        if (r.Index != dgdExpressions.Rows.Count - 1)
                            foreach (DataGridViewCell c in r.Cells)
                            {
                                if (!(c.RowIndex == e.RowIndex && c.ColumnIndex == e.ColumnIndex))
                                    dgdExpressions_CellEndEdit(null, new DataGridViewCellEventArgs(c.ColumnIndex, c.RowIndex));
                            }
                }
            }
        }

        private void dgdExpressions_MouseClick(object sender, MouseEventArgs e)
        {
            if (dgdExpressions.HitTest(e.X, e.Y).Type == DataGridViewHitTestType.RowHeader)
            {
                dgdExpressions.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;
                dgdExpressions.EndEdit();
            }
            else
                dgdExpressions.EditMode = DataGridViewEditMode.EditOnEnter;
        }

        private void dgdExpressions_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            foreach (DataGridViewRow r in dgdExpressions.Rows)
                dgdExpressions_CellEndEdit(sender, new DataGridViewCellEventArgs(1, r.Index));
            UpdateExpressionsAndCountersCombo();
        }

        private void cmbCheckAboveOrBelow_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbFailIfValueAboveBelow.SelectedIndex == 0)
            {
                pnlHigh.BackColor = PassColor;
                pnlMed.BackColor = WarnColor;
                pnlLow.BackColor = ErrorColor;
                lblHighVal.Visible = cmbValHigh.Visible = false;
                lblLowVal.Visible = cmbValLow.Visible = true;
                lblHighRegion.Text = "Pass region label";
                lblHighResult.Text = "Pass text";
                lblLowRegion.Text = "Error region label";
                lblLowResultText.Text = "Failure text";
                lblWarnVal.Text = "Warn below value";
            }
            else
            {
                pnlHigh.BackColor = ErrorColor;
                pnlMed.BackColor = WarnColor;
                pnlLow.BackColor = PassColor;
                lblHighVal.Visible = cmbValHigh.Visible = true;
                lblLowVal.Visible = cmbValLow.Visible = false;
                lblHighRegion.Text = "Error region label";
                lblHighResult.Text = "Failure text";
                lblLowRegion.Text = "Pass region label";
                lblLowResultText.Text = "Pass text";
                lblWarnVal.Text = "Warn above value";
            }
            btnSaveRule.Enabled = IsRuleComplete();
        }

        private void frmAddPerfMonRule_SizeChanged(object sender, EventArgs e)
        {
            pnlHigh.Width = pnlLow.Width = pnlMed.Width = Width - pnlHigh.Left;
            txtLowRegion.Width = txtLowResult.Width = txtWarnRegion.Width = txtWarnResult.Width = txtHighRegion.Width = txtHighResult.Width = pnlHigh.Width - txtHighResult.Left - 38;
            btnCancel.Left = Width - btnCancel.Width - 20;
            btnSaveRule.Left = btnCancel.Left - btnSaveRule.Width - 6;
        }

        private void txtName_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsLetter(txtName.Text[0]))
            {
                e.Handled = true;
                tt.Show("Rule names must start with a letter.", txtName, 0, -20, 1000);
            }
            btnSaveRule.Enabled = IsRuleComplete();
        }

        private void dgdSelectedCounters_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            AddCountersBackToTreeViewFromGrid(e.Row);
            foreach (DataGridViewRow r in dgdExpressions.Rows)
                dgdExpressions_CellEndEdit(dgdExpressions, new DataGridViewCellEventArgs(1, r.Index));
            UpdateExpressionsAndCountersCombo();
        }

        private void cmbErrorWarnVals_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (sender == cmbWarnExpr)
                ValidateChildren();
            btnSaveRule.Enabled = IsRuleComplete();
        }

        private void txtErrorWarnRegions_TextChanged(object sender, EventArgs e)
        {
            if (sender == txtWarnRegion || sender == txtWarnResult)
                ValidateChildren();
            btnSaveRule.Enabled = IsRuleComplete();
        }

        private void RequireRuleElements_Validating(object sender, CancelEventArgs e)
        {
            if (sender is ComboBox)
            {
                ComboBox cb = sender as ComboBox;
                if (cb == cmbWarnExpr && txtWarnRegion.Text.Trim() == "" && txtWarnResult.Text.Trim() == "")
                    errorProvider1.SetError(cb, "");
                else
                {
                    if (cb.SelectedIndex == -1)
                    {
                        if (cb == cmbWarnExpr)
                            errorProvider1.SetError(cb, "Warning value required when a warning region label and result text are set.");
                        else
                            errorProvider1.SetError(cb, "Selection is required for a valid rule.");
                    }
                    else
                        errorProvider1.SetError(cb, "");
                }
            }
            else if (sender is TextBox)
            {
                TextBox tb = sender as TextBox;
                if ((tb == txtWarnResult || tb == txtWarnRegion) && cmbWarnExpr.SelectedIndex == -1  && txtWarnResult.Text.Trim() == "" && txtWarnRegion.Text.Trim() == "")
                    errorProvider1.SetError(tb, "");
                else
                {
                    if (tb.Text.Trim() == "")
                    {
                        if (tb == txtWarnRegion || tb == txtWarnResult)
                            errorProvider1.SetError(tb, "Warning region label and result text are required when a warning value is set.");
                        else
                            errorProvider1.SetError(tb, "This item is required for a valid rule.");
                    }
                    else
                        errorProvider1.SetError(tb, "");
                }
            }
        }

        private void frmAddPerfMonRule_Shown(object sender, EventArgs e)
        {
            if (originRule != null)
            {
                DataGridViewRow row = null;
                txtName.Text = originRule.Name;
                txtCategory.Text = originRule.Category;
                txtDescription.Text = originRule.Description;
                foreach (string ctr in originRule.Counters.Select(c => c.WildcardPath).Distinct())
                {
                    dgdSelectedCounters.Rows.Add();
                    row = dgdSelectedCounters.Rows[dgdSelectedCounters.Rows.Count - 1];
                    row.Cells[0].Value = ctr;
                    ucASPerfMonAnalyzer.RuleCounter rc = originRule.Counters.Where(c => c.WildcardPath == ctr).First();
                    row.Cells[1].Value = rc.ShowInChart;
                    row.Cells[2].Value = rc.HighlightInChart;
                    row.Cells[3].Value = rc.Include_TotalSeriesInWildcard;
                }
                RegistryKey key = Registry.LocalMachine.CreateSubKey("SOFTWARE\\SSASDiag\\PerfMonRules\\" + originRule.Name + "\\Expressions", RegistryKeyPermissionCheck.ReadSubTree);
                List<ucASPerfMonAnalyzer.RuleExpression> exprs = new List<ucASPerfMonAnalyzer.RuleExpression>();
                foreach (string sk in key.GetSubKeyNames())
                {
                    RegistryKey subkey = key.OpenSubKey(sk);
                    exprs.Add(new ucASPerfMonAnalyzer.RuleExpression(sk, subkey.GetValue("Expression") as string, (int)subkey.GetValue("Index"), Convert.ToBoolean(subkey.GetValue("Display")), Convert.ToBoolean(subkey.GetValue("Highlight"))));
                }
                foreach (ucASPerfMonAnalyzer.RuleExpression ex in exprs.OrderBy(expr => expr.Index))
                {
                    row = dgdExpressions.Rows[0].Clone() as DataGridViewRow;
                    row.Cells[0].Value = ex.Name;
                    row.Cells[1].Value = ex.Expression;
                    row.Cells[2].Value = ex.Display;
                    row.Cells[3].Value = ex.Highlight;
                    dgdExpressions.Rows.Add(row);
                }

                UpdateExpressionsAndCountersCombo();
                key.Close();
                key = Registry.LocalMachine.CreateSubKey("SOFTWARE\\SSASDiag\\PerfMonRules\\" + originRule.Name, RegistryKeyPermissionCheck.ReadSubTree);
                cmbValueToCheck.SelectedIndex = cmbValueToCheck.FindStringExact(key.GetValue("ValueOrSeriesToCheck") as string);
                cmbSeriesFunction.SelectedIndex = cmbSeriesFunction.FindStringExact(key.GetValue("SeriesFunction") as string);
                udPctMatchCheck.Value = (int)key.GetValue("PctRequiredToMatchWarnError", 15);
                bool bFailIfValuesBelowWarnError = Convert.ToBoolean(key.GetValue("FailIfBelowWarnError"));
                string WarnExpr = key.GetValue("WarnExpr", "") as string;
                if (WarnExpr != "")
                {
                    cmbWarnExpr.SelectedIndex = cmbWarnExpr.FindStringExact(WarnExpr);
                    txtWarnRegion.Text = key.GetValue("WarnRegionLabel") as string;
                    txtWarnResult.Text = key.GetValue("WarningText") as string;
                }
                if (bFailIfValuesBelowWarnError)
                {
                    cmbFailIfValueAboveBelow.SelectedIndex = 0;
                    cmbValLow.SelectedIndex = cmbValLow.FindStringExact(key.GetValue("ErrorExpr") as string);
                    txtLowRegion.Text = key.GetValue("ErrorRegionLabel") as string;
                    txtLowResult.Text = key.GetValue("ErrorText") as string;
                    txtHighRegion.Text = key.GetValue("PassRegionLabel") as string;
                    txtHighResult.Text = key.GetValue("PassText") as string;
                }
                else

                {
                    cmbFailIfValueAboveBelow.SelectedIndex = 1;
                    cmbValHigh.SelectedIndex = cmbValLow.FindStringExact(key.GetValue("ErrorExpr") as string);
                    txtHighRegion.Text = key.GetValue("ErrorRegionLabel") as string;
                    txtHighResult.Text = key.GetValue("ErrorText") as string;
                    txtLowRegion.Text = key.GetValue("PassRegionLabel") as string;
                    txtLowResult.Text = key.GetValue("PassText") as string;
                }
                key.Close();
            }

            ValidateChildren();
            txtName.Focus();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnSaveRule_Click(object sender, EventArgs e)
        {
            bool bExistingRule = false;
            RegistryKey rules = Registry.LocalMachine.CreateSubKey("SOFTWARE\\SSASDiag\\PerfMonRules", RegistryKeyPermissionCheck.ReadWriteSubTree);
            foreach (string ruleName in rules.GetSubKeyNames())
                if (ruleName.ToLower() == txtName.Text.ToLower().Trim())
                {
                    if (MessageBox.Show(this, "A rule with this name already exists!\nReplace existing rule?", "Existing rule", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.No)
                    {
                        errorProvider1.SetError(txtName, "A rule with this name already exists!");
                        return;
                    }
                    bExistingRule = true;
                }
            RegistryKey rule = rules.CreateSubKey(txtName.Text.Trim(), RegistryKeyPermissionCheck.ReadWriteSubTree);
            rule.DeleteSubKeyTree("Counters", false);
            RegistryKey counters = rule.CreateSubKey("Counters", RegistryKeyPermissionCheck.ReadWriteSubTree);
            int iCurPos = 0;
            foreach (DataGridViewRow r in dgdSelectedCounters.Rows)
            {
                RegistryKey counter = counters.CreateSubKey((r.Cells[0].Value as string).Replace("\\", "{SLASH}"), RegistryKeyPermissionCheck.ReadWriteSubTree);
                counter.SetValue("Display", r.Cells[1].Value == null ? 0 : Convert.ToInt32(r.Cells[1].Value));
                counter.SetValue("Highlight", r.Cells[2].Value == null ? 0 : Convert.ToInt32(r.Cells[2].Value));
                counter.SetValue("WildcardIncludes_Total", r.Cells[3].Value == null ? 0 : Convert.ToInt32(r.Cells[3].Value));
                counter.SetValue("Index", iCurPos++);
                counter.Close();
            }
            counters.Close();
            rule.DeleteSubKeyTree("Expressions", false);
            RegistryKey expressions = rule.CreateSubKey("Expressions", RegistryKeyPermissionCheck.ReadWriteSubTree);
            iCurPos = 0;
            foreach (DataGridViewRow r in dgdExpressions.Rows)
            {
                if (r.Cells[0].Value != null)
                {
                    RegistryKey expr = expressions.CreateSubKey(r.Cells[0].Value as string, RegistryKeyPermissionCheck.ReadWriteSubTree);
                    expr.SetValue("Display", r.Cells[2].Value == null ? 0 : Convert.ToInt32(r.Cells[2].Value));
                    expr.SetValue("Highlight", r.Cells[3].Value == null ? 0 : Convert.ToInt32(r.Cells[3].Value));
                    expr.SetValue("Expression", r.Cells[1].Value as string);
                    expr.SetValue("Index", iCurPos++);
                    expr.Close();
                }
            }
            expressions.Close();
            rule.SetValue("ValueOrSeriesToCheck", cmbValueToCheck.SelectedItem as string);
            rule.SetValue("Description", txtDescription.Text.Trim());
            rule.SetValue("Category", txtCategory.Text.Trim());
            if (cmbSeriesFunction.Visible)
            {
                rule.SetValue("SeriesFunction", cmbSeriesFunction.SelectedItem == null ? "" : cmbSeriesFunction.SelectedItem as string);
                if (udPctMatchCheck.Visible)
                    rule.SetValue("PctRequiredToMatchWarnError", (int)udPctMatchCheck.Value);
            }
            rule.SetValue("FailIfBelowWarnError", Math.Abs(cmbFailIfValueAboveBelow.SelectedIndex - 1));
            if (cmbFailIfValueAboveBelow.SelectedIndex == 1)
            {
                rule.SetValue("ErrorExpr", cmbValHigh.SelectedItem as string);
                rule.SetValue("ErrorRegionLabel", txtHighRegion.Text.Trim());
                rule.SetValue("ErrorText", txtHighResult.Text.Trim());
                rule.SetValue("PassRegionLabel", txtLowRegion.Text.Trim());
                rule.SetValue("PassText", txtLowResult.Text.Trim());
            }
            else
            {
                rule.SetValue("ErrorExpr", cmbValLow.SelectedItem as string);
                rule.SetValue("ErrorRegionLabel", txtLowRegion.Text.Trim());
                rule.SetValue("ErrorText", txtLowResult.Text.Trim());
                rule.SetValue("PassRegionLabel", txtHighRegion.Text.Trim());
                rule.SetValue("PassText", txtHighResult.Text.Trim());
            }
            if (cmbWarnExpr.SelectedIndex > -1)
            {
                rule.SetValue("WarnExpr", cmbWarnExpr.SelectedItem as string);
                rule.SetValue("WarnRegionLabel", txtWarnRegion.Text.Trim());
                rule.SetValue("WarningText", txtWarnResult.Text.Trim());
            }
            rule.Close();
            if (bExistingRule)
                DialogResult = DialogResult.Ignore;
            else
                DialogResult = DialogResult.OK;
            Close();
        }

        private void cmbValueToCheck_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool bSeries = false;
            foreach (DataGridViewRow r in dgdSelectedCounters.Rows)
                if (r.Cells[0].Value as string == cmbValueToCheck.SelectedItem as string)
                    bSeries = true;
            cmbSeriesFunction.SelectedIndex = -1;
            lblPctMatchCheck.Visible = udPctMatchCheck.Visible = false;
            if (bSeries)
                cmbSeriesFunction.Visible = lblSeriesFunction.Visible = true;
            else
                cmbSeriesFunction.Visible = lblSeriesFunction.Visible = false;
            btnSaveRule.Enabled = IsRuleComplete();
        }

        private void cmbSeriesFunction_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblPctMatchCheck.Visible = udPctMatchCheck.Visible = cmbSeriesFunction.SelectedIndex == (cmbSeriesFunction.Items.Count - 1);
            btnSaveRule.Enabled = IsRuleComplete();
        }

        private void txtRuleDetailsChanged(object sender, EventArgs e)
        {
            btnSaveRule.Enabled = IsRuleComplete();
        }

        private void splitExpressions_SplitterMoving(object sender, SplitterCancelEventArgs e)
        {
            splitCounters.SplitterDistance = e.SplitX;
        }

        private void splitCounters_SplitterMoving(object sender, SplitterCancelEventArgs e)
        {
            splitExpressions.SplitterDistance = e.SplitX;
        }
    }

    public class DataGridViewTextBoxColumnWithExpandedEditArea : DataGridViewTextBoxCell
    {
        public override void PositionEditingControl(bool setLocation, bool setSize,
            Rectangle cellBounds, Rectangle cellClip, DataGridViewCellStyle cellStyle,
            bool singleVerticalBorderAdded, bool singleHorizontalBorderAdded,
            bool isFirstDisplayedColumn, bool isFirstDisplayedRow)
        {
            cellClip.Height = cellClip.Height;
            cellBounds.Height = cellBounds.Height;
            var r = base.PositionEditingPanel(cellBounds, cellClip, cellStyle,
                singleVerticalBorderAdded, singleHorizontalBorderAdded,
                isFirstDisplayedColumn, isFirstDisplayedRow);
            this.DataGridView.EditingControl.Location = r.Location;
            this.DataGridView.EditingControl.Size = r.Size;
        }
        public override void InitializeEditingControl(int rowIndex,
            object initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle)
        {
            base.InitializeEditingControl(rowIndex, initialFormattedValue,
                dataGridViewCellStyle);
            ((TextBox)this.DataGridView.EditingControl).Multiline = false;
            ((TextBox)this.DataGridView.EditingControl).BorderStyle = BorderStyle.Fixed3D;
        }
    }
}
