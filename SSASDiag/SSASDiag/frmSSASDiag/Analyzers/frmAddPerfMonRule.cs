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

namespace SSASDiag
{
    public partial class frmAddPerfMonRule : Form
    {
        TreeView tvCounters;
        ucASPerfMonAnalyzer HostControl = ((Program.MainForm.tcCollectionAnalysisTabs.TabPages[1].Controls["tcAnalysis"] as TabControl).TabPages["Performance Logs"].Controls[0] as ucASPerfMonAnalyzer);

        public frmAddPerfMonRule()
        {
            InitializeComponent();

            // 
            // tvCounters
            // 
            tvCounters = new TreeView();
            tvCounters.CheckBoxes = false;
            tvCounters.Dock = System.Windows.Forms.DockStyle.Fill;
            tvCounters.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            tvCounters.Name = "tvCounters";
            tvCounters.TabIndex = 0;
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
        }



        private void frmAddPerfMonRule_Load(object sender, EventArgs e)
        {

        }





        /* Drag & Drop */
        private Rectangle dragBoxFromMouseDown;
        private object valueFromMouseDown;

        private void dgdSelectedCounters_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void TvCounters_DragDrop(object sender, DragEventArgs e)
        {
            DataGridViewRow r = (DataGridViewRow)e.Data.GetData("System.Windows.Forms.DataGridViewRow");
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
            dgdSelectedCounters.Rows.Remove(r);
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
                r.Tag = newNode;
                if (newNode.Parent == null)
                    tvCounters.Nodes.Remove(newNode);
                else
                    newNode.Parent.Nodes.Remove(newNode);
                r.Cells[0].Value = CounterPath;
            }
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

        List<TreeNode> ListNodes(TreeView tv)
        {
            List<TreeNode> nodes = new List<TreeNode>();
            foreach (TreeNode subnode in tv.Nodes)
                nodes.AddRange(ListNodes(subnode));
            return nodes;
        }
        List<TreeNode> ListNodes(TreeNode node)
        {
            List<TreeNode> nodes = new List<TreeNode>();
            foreach (TreeNode subnode in node.Nodes)
                nodes.AddRange(ListNodes(subnode));
            return nodes;
        }
    }
}
