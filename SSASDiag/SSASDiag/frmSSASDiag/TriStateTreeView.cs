using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace SSASDiag
{
    public class TriStateTreeView : TreeView
    {
        public enum CheckedState : int { UnInitialised = -1, UnChecked, Checked, Mixed };
        public int IgnoreClickAction = 0;
        public enum TriStateStyles : int { Standard = 0, Installer };
        private TriStateStyles TriStateStyle = TriStateStyles.Standard;

        public TriStateStyles TriStateStyleProperty
        {
            get { return TriStateStyle; }
            set { TriStateStyle = value; }
        }

        #region Selected Node(s) Properties

        private ObservableCollection<TreeNode> m_SelectedNodes = null;
        public ObservableCollection<TreeNode> SelectedNodes
        {
            get
            {
                return m_SelectedNodes;
            }
            set
            {
                ClearSelectedNodes();
                if (value != null)
                {
                    foreach (TreeNode node in value)
                    {
                        SelectNode(node);
                    }
                }
            }
        }

        // Note we use the new keyword to Hide the native treeview's SelectedNode property.
        private TreeNode m_SelectedNode;
        public new TreeNode SelectedNode
        {
            get { return m_SelectedNode; }
            set
            {
                ClearSelectedNodes();
                if (value != null)
                {
                    SelectNode(value);
                }
            }
        }

        #endregion

        public TriStateTreeView() : base()
        {
            DrawMode = TreeViewDrawMode.OwnerDrawText;
            m_SelectedNodes = new ObservableCollection<TreeNode>();
            m_SelectedNodes.CollectionChanged += M_SelectedNodes_CollectionChanged;
            base.SelectedNode = null;
            StateImageList = new System.Windows.Forms.ImageList();
            for (int i = 0; i < 3; i++)
            {

                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(18, 16);
                System.Drawing.Graphics chkGraphics = System.Drawing.Graphics.FromImage(bmp);
                switch (i)
                {
                    case 0:
                        System.Windows.Forms.CheckBoxRenderer.DrawCheckBox(chkGraphics, new System.Drawing.Point(0, 0), System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedNormal);
                        break;
                    case 1:
                        System.Windows.Forms.CheckBoxRenderer.DrawCheckBox(chkGraphics, new System.Drawing.Point(0, 0), System.Windows.Forms.VisualStyles.CheckBoxState.CheckedNormal);
                        break;
                    case 2:
                        System.Windows.Forms.CheckBoxRenderer.DrawCheckBox(chkGraphics, new System.Drawing.Point(0, 0), System.Windows.Forms.VisualStyles.CheckBoxState.MixedNormal);
                        break;
                }

                StateImageList.Images.Add(bmp);
            }
            
        }

        private void M_SelectedNodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                foreach (TreeNode n in e.NewItems)
                    ToggleNode(n, true);
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                foreach (TreeNode n in e.OldItems)
                    ToggleNode(n, false);
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
                foreach (TreeNode n in Nodes)
                {
                    ToggleNode(n, false);
                    foreach (TreeNode nn in GetNodeBranch(n))
                        ToggleNode(nn, false);
                }   
        }

        #region Overridden Events

        protected override void OnDrawNode(DrawTreeNodeEventArgs e)
        {
            if (!bNoRedraw)
            {
                if (e.State == TreeNodeStates.Focused)
                {
                    e.Graphics.FillRectangle(SystemBrushes.Window, e.Node.Bounds);
                    if (ModifierKeys != Keys.Shift && ModifierKeys != Keys.Control)
                        ClearSelectedNodes();
                    SelectNode(e.Node);
                    OnAfterSelect(new TreeViewEventArgs(e.Node));    
                }

                Rectangle bounds = e.Node.Bounds;
                if (bounds.Top == 0 && bounds.Left == 0 && bounds.Height == 0 && bounds.Width == 0)
                    return;
                e.DrawDefault = false;
                try
                {
                    SizeF textsize = e.Graphics.MeasureString(e.Node.Name, Font);
                    Brush brush = SystemBrushes.Window;
                    int rightWidth = e.Node.Bounds.Width + 17;
                    if (SelectedNodes.Contains(e.Node))
                    {
                        brush = SystemBrushes.MenuHighlight;
                        rightWidth = (int)Math.Round(textsize.Width, 0) + 4;
                    }
                    Rectangle selectRect = new Rectangle(e.Node.Bounds.Left - 1 - (e.Node.ImageIndex > 0 ? 0 : 16), e.Node.Bounds.Top, rightWidth, e.Node.Bounds.Height );
                    e.Graphics.FillRectangle(brush, selectRect);
                    e.Graphics.DrawString(e.Node.Name, Font, SystemBrushes.WindowText,
                        e.Node.Bounds.Left - (e.Node.ImageIndex > 0 ? 0 : 16),
                        e.Node.Bounds.Top + 2);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                
            }
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            CheckBoxes = false;

            IgnoreClickAction++;
            UpdateChildState(this.Nodes, (int)CheckedState.UnChecked, false, true);
            IgnoreClickAction--;
        }

        protected override void OnGotFocus(EventArgs e)
        {
            try
            {
                base.OnGotFocus(e);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        bool bNoRedraw = false;
        protected override void OnAfterCheck(TreeViewEventArgs e)
        {
            base.OnAfterCheck(e);
            if (IgnoreClickAction > 0)
            {
                return;
            }

            IgnoreClickAction++;
            TreeNode tn = e.Node;
            tn.StateImageIndex = tn.Checked ? (int)CheckedState.Checked : (int)CheckedState.UnChecked;
            bNoRedraw = true;
            UpdateChildState(e.Node.Nodes, e.Node.StateImageIndex, e.Node.Checked, false);
            bNoRedraw = false;
            UpdateParentState(e.Node.Parent);
            IgnoreClickAction--;
        }

        protected override void OnAfterExpand(TreeViewEventArgs e)
        {
            base.OnAfterExpand(e);

            IgnoreClickAction++;
            UpdateChildState(e.Node.Nodes, e.Node.StateImageIndex, e.Node.Checked, true);
            IgnoreClickAction--;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            // Handle all possible key strokes for the control.
            // including navigation, selection, etc.

            base.OnKeyDown(e);

            if (e.KeyCode == Keys.ShiftKey) return;

            //this.BeginUpdate();
            bool bShift = (ModifierKeys == Keys.Shift);

            try
            {
                // Nothing is selected in the tree, this isn't a good state
                // select the top node
                if (m_SelectedNode == null && this.TopNode != null)
                {
                    ToggleNode(this.TopNode, true);
                }

                // Nothing is still selected in the tree, this isn't a good state, leave.
                if (m_SelectedNode == null) return;

                if (e.KeyCode == Keys.Left)
                {
                    if (m_SelectedNode.IsExpanded && m_SelectedNode.Nodes.Count > 0)
                    {
                        // Collapse an expanded node that has children
                        m_SelectedNode.Collapse();
                    }
                    else if (m_SelectedNode.Parent != null)
                    {
                        // Node is already collapsed, try to select its parent.
                        SelectSingleNode(m_SelectedNode.Parent);
                    }
                }
                else if (e.KeyCode == Keys.Right)
                {
                    if (!m_SelectedNode.IsExpanded)
                    {
                        // Expand a collpased node's children
                        m_SelectedNode.Expand();
                    }
                    else
                    {
                        // Node was already expanded, select the first child
                        SelectSingleNode(m_SelectedNode.FirstNode);
                    }
                }
                else if (e.KeyCode == Keys.Up)
                {
                    // Select the previous node
                    if (m_SelectedNode.PrevVisibleNode != null)
                    {
                        SelectNode(m_SelectedNode.PrevVisibleNode);
                    }
                }
                else if (e.KeyCode == Keys.Down)
                {
                    // Select the next node
                    if (m_SelectedNode.NextVisibleNode != null)
                    {
                        SelectNode(m_SelectedNode.NextVisibleNode);
                    }
                }
                else if (e.KeyCode == Keys.Home)
                {
                    if (bShift)
                    {
                        if (m_SelectedNode.Parent == null)
                        {
                            // Select all of the root nodes up to this point 
                            if (this.Nodes.Count > 0)
                            {
                                SelectNode(this.Nodes[0]);
                            }
                        }
                        else
                        {
                            // Select all of the nodes up to this point under this nodes parent
                            SelectNode(m_SelectedNode.Parent.FirstNode);
                        }
                    }
                    else
                    {
                        // Select this first node in the tree
                        if (this.Nodes.Count > 0)
                        {
                            SelectSingleNode(this.Nodes[0]);
                        }
                    }
                }
                else if (e.KeyCode == Keys.End)
                {
                    if (bShift)
                    {
                        if (m_SelectedNode.Parent == null)
                        {
                            // Select the last ROOT node in the tree
                            if (this.Nodes.Count > 0)
                            {
                                SelectNode(this.Nodes[this.Nodes.Count - 1]);
                            }
                        }
                        else
                        {
                            // Select the last node in this branch
                            SelectNode(m_SelectedNode.Parent.LastNode);
                        }
                    }
                    else
                    {
                        if (this.Nodes.Count > 0)
                        {
                            // Select the last node visible node in the tree.
                            // Don't expand branches incase the tree is virtual
                            TreeNode ndLast = this.Nodes[0].LastNode;
                            while (ndLast.IsExpanded && (ndLast.LastNode != null))
                            {
                                ndLast = ndLast.LastNode;
                            }
                            SelectSingleNode(ndLast);
                        }
                    }
                }
                else if (e.KeyCode == Keys.PageUp)
                {
                    // Select the highest node in the display
                    int nCount = this.VisibleCount;
                    TreeNode ndCurrent = m_SelectedNode;
                    while ((nCount) > 0 && (ndCurrent.PrevVisibleNode != null))
                    {
                        ndCurrent = ndCurrent.PrevVisibleNode;
                        nCount--;
                    }
                    SelectSingleNode(ndCurrent);
                }
                else if (e.KeyCode == Keys.PageDown)
                {
                    // Select the lowest node in the display
                    int nCount = this.VisibleCount;
                    TreeNode ndCurrent = m_SelectedNode;
                    while ((nCount) > 0 && (ndCurrent.NextVisibleNode != null))
                    {
                        ndCurrent = ndCurrent.NextVisibleNode;
                        nCount--;
                    }
                    SelectSingleNode(ndCurrent);
                }
                else
                {
                    // Assume this is a search character a-z, A-Z, 0-9, etc.
                    // Select the first node after the current node that 
                    // starts with this character
                    string sSearch = ((char)e.KeyValue).ToString();

                    TreeNode ndCurrent = m_SelectedNode;
                    while ((ndCurrent.NextVisibleNode != null))
                    {
                        ndCurrent = ndCurrent.NextVisibleNode;
                        if (ndCurrent.Text.StartsWith(sSearch))
                        {
                            SelectSingleNode(ndCurrent);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            finally
            {
                this.EndUpdate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            // If the clicked on a node that WAS previously
            // selected then, reselect it now. This will clear
            // any other selected nodes. e.g. A B C D are selected
            // the user clicks on B, now A C & D are no longer selected.
            try
            {
                // Check to see if a node was clicked on 
                TreeNode node = this.GetNodeAt(e.Location);
                if (node != null)
                {
                    if (ModifierKeys == Keys.None && m_SelectedNodes.Contains(node))
                    {
                        int leftBound = node.Bounds.X; // -20; // Allow user to click on image
                        int rightBound = node.Bounds.Right + 10; // Give a little extra room
                        if (e.Location.X > leftBound && e.Location.X < rightBound)
                        {

                            SelectNode(node);
                        }
                    }
                }

                base.OnMouseUp(e);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            // If the user clicks on a node that was not
            // previously selected, select it now.

            try
            {
                base.SelectedNode = null;

                TreeNode node = this.GetNodeAt(e.Location);
                if (node != null)
                {
                    int leftBound = node.Bounds.X; // - 20; // Allow user to click on image
                    int rightBound = node.Bounds.Right + 10; // Give a little extra room
                    if (e.Location.X > leftBound && e.Location.X < rightBound)
                    {
                        if (ModifierKeys == Keys.None && (m_SelectedNodes.Contains(node)))
                        {
                            // Potential Drag Operation
                            // Let Mouse Up do select
                        }
                        else
                        {
                            SelectNode(node);
                        }
                    }
                }

                base.OnMouseDown(e);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        protected override void OnItemDrag(ItemDragEventArgs e)
        {
            // If the user drags a node and the node being dragged is NOT
            // selected, then clear the active selection, select the
            // node being dragged and drag it. Otherwise if the node being
            // dragged is selected, drag the entire selection.
            try
            {
                TreeNode node = e.Item as TreeNode;

                if (node != null)
                {
                    if (!m_SelectedNodes.Contains(node))
                    {
                        SelectSingleNode(node);
                        ToggleNode(node, true);
                    }
                }

                base.OnItemDrag(e);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        protected override void OnNodeMouseClick(TreeNodeMouseClickEventArgs e)
        {
            base.OnNodeMouseClick(e);

            TreeViewHitTestInfo info = HitTest(e.X, e.Y);
            if (info == null || info.Location != TreeViewHitTestLocations.StateImage)
            {
                return;
            }

            TreeNode tn = e.Node;
            tn.Checked = !tn.Checked;
        }

        protected override void OnBeforeSelect(TreeViewCancelEventArgs e)
        {
            // Never allow base.SelectedNode to be set!
            try
            {
                base.SelectedNode = null;
                e.Cancel = true;

                base.OnBeforeSelect(e);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        protected override void OnAfterSelect(TreeViewEventArgs e)
        {
            // Never allow base.SelectedNode to be set!
            try
            {
                base.OnAfterSelect(e);
                foreach (TreeNode n in e.Node.Nodes)
                {
                    if (SelectedNodes.Where(nn => nn.FullPath == e.Node.FullPath) != null)
                    {
                        ToggleNode(n, true);
                        OnAfterSelect(new TreeViewEventArgs(n));
                    }
                    else
                        ToggleNode(n, false);
                }
                base.SelectedNode = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

#endregion Overridden Events


        #region Helper Methods

        protected bool isParent(TreeNode parentNode, TreeNode childNode)
        {
            if (parentNode == childNode)
                return true;

            TreeNode n = childNode;
            bool bFound = false;
            while (!bFound && n != null)
            {
                n = n.Parent;
                bFound = (n == parentNode);
            }
            return bFound;
        }

        private IEnumerable<TreeNode> GetNodeBranch(TreeNode node)
        {
            yield return node;

            foreach (TreeNode child in node.Nodes)
                foreach (var childChild in GetNodeBranch(child))
                    yield return childChild;
        }

        public List<TreeNode> GetLeafNodes(bool CheckedOnly = true)
        {
            return GetLeafNodes(this.Nodes);
        }

        public List<TreeNode> GetLeafNodes(TreeNode node, bool CheckedOnly = true)
        {
            List<TreeNode> cn = new List<TreeNode>();
            if (node.Nodes.Count != 0)
                cn.AddRange(GetLeafNodes(node.Nodes, CheckedOnly));
            else
            {
                if (CheckedOnly)
                {
                    if (node.Checked)
                        cn.Add(node);
                }
                else
                    cn.Add(node);
            }
            return cn;
        }

        private List<TreeNode> GetLeafNodes(TreeNodeCollection nodes, bool CheckedOnly = true)
        {
            List<TreeNode> cn = new List<TreeNode>();
            foreach (TreeNode aNode in nodes)
                cn.AddRange(GetLeafNodes(aNode, CheckedOnly));
            return cn;
        }

        protected void UpdateChildState(TreeNodeCollection Nodes, int StateImageIndex, bool Checked, bool ChangeUninitialisedNodesOnly)
        {
            foreach (TreeNode tnChild in Nodes)
            {
                if (!ChangeUninitialisedNodesOnly || tnChild.StateImageIndex == -1)
                {
                    tnChild.StateImageIndex = StateImageIndex;
                    tnChild.Checked = Checked;

                    if (tnChild.Nodes.Count > 0)
                    {
                        UpdateChildState(tnChild.Nodes, StateImageIndex, Checked, ChangeUninitialisedNodesOnly);
                    }
                }
            }
        }

        public void UpdateParentState(TreeNode tn)
        {
            if (tn == null)
                return;

            int OrigStateImageIndex = tn.StateImageIndex;

            int UnCheckedNodes = 0, CheckedNodes = 0, MixedNodes = 0;

            foreach (TreeNode tnChild in tn.Nodes)
            {
                if (tnChild.StateImageIndex == (int)CheckedState.Checked)
                    CheckedNodes++;
                else if (tnChild.StateImageIndex == (int)CheckedState.Mixed)
                {
                    MixedNodes++;
                    break;
                }
                else
                    UnCheckedNodes++;
            }

            if (TriStateStyle == TriStateStyles.Installer)
            {
                if (MixedNodes == 0)
                {
                    if (UnCheckedNodes == 0)
                    {
                        tn.Checked = true;
                    }
                    else
                    {
                        tn.Checked = false;
                    }
                }
            }

            if (MixedNodes > 0)
            {
                tn.StateImageIndex = (int)CheckedState.Mixed;
            }
            else if (CheckedNodes > 0 && UnCheckedNodes == 0)
            {
                if (tn.Checked)
                    tn.StateImageIndex = (int)CheckedState.Checked;
                else
                    tn.StateImageIndex = (int)CheckedState.Mixed;
            }
            else if (CheckedNodes > 0)
            {
                tn.StateImageIndex = (int)CheckedState.Mixed;
            }
            else
            {
                if (tn.Checked)
                    tn.StateImageIndex = (int)CheckedState.Mixed;
                else
                    tn.StateImageIndex = (int)CheckedState.UnChecked;
            }

            if (OrigStateImageIndex != tn.StateImageIndex && tn.Parent != null)
            {
                UpdateParentState(tn.Parent);
            }
        }

        private void SelectNode(TreeNode node)
        {
            try
            {
                this.BeginUpdate();

                if (m_SelectedNode == null || ModifierKeys == Keys.Control)
                {
                    // Ctrl+Click selects an unselected node, or unselects a selected node.
                    bool bIsSelected = m_SelectedNodes.Contains(node);
                    ToggleNode(node, !bIsSelected);
                }
                else if (ModifierKeys == Keys.Shift)
                {
                    // Shift+Click selects nodes between the selected node and here.
                    TreeNode ndStart = m_SelectedNode;
                    TreeNode ndEnd = node;

                    if (ndStart.Parent == ndEnd.Parent)
                    {
                        // Selected node and clicked node have same parent, easy case.
                        if (ndStart.Index < ndEnd.Index)
                        {
                            // If the selected node is beneath the clicked node walk down
                            // selecting each Visible node until we reach the end.
                            while (ndStart != ndEnd)
                            {
                                ndStart = ndStart.NextVisibleNode;
                                if (ndStart == null) break;
                                ToggleNode(ndStart, true);
                            }
                        }
                        else if (ndStart.Index == ndEnd.Index)
                        {
                            // Clicked same node, do nothing
                        }
                        else
                        {
                            // If the selected node is above the clicked node walk up
                            // selecting each Visible node until we reach the end.
                            while (ndStart != ndEnd)
                            {
                                ndStart = ndStart.PrevVisibleNode;
                                if (ndStart == null) break;
                                ToggleNode(ndStart, true);
                            }
                        }
                    }
                    else
                    {
                        // Selected node and clicked node have same parent, hard case.
                        // We need to find a common parent to determine if we need
                        // to walk down selecting, or walk up selecting.

                        TreeNode ndStartP = ndStart;
                        TreeNode ndEndP = ndEnd;
                        int startDepth = Math.Min(ndStartP.Level, ndEndP.Level);

                        // Bring lower node up to common depth
                        while (ndStartP.Level > startDepth)
                        {
                            ndStartP = ndStartP.Parent;
                        }

                        // Bring lower node up to common depth
                        while (ndEndP.Level > startDepth)
                        {
                            ndEndP = ndEndP.Parent;
                        }

                        // Walk up the tree until we find the common parent
                        while (ndStartP.Parent != ndEndP.Parent)
                        {
                            ndStartP = ndStartP.Parent;
                            ndEndP = ndEndP.Parent;
                        }

                        // Select the node
                        if (ndStartP.Index < ndEndP.Index)
                        {
                            // If the selected node is beneath the clicked node walk down
                            // selecting each Visible node until we reach the end.
                            while (ndStart != ndEnd)
                            {
                                ndStart = ndStart.NextVisibleNode;
                                if (ndStart == null) break;
                                ToggleNode(ndStart, true);
                            }
                        }
                        else if (ndStartP.Index == ndEndP.Index)
                        {
                            if (ndStart.Level < ndEnd.Level)
                            {
                                while (ndStart != ndEnd)
                                {
                                    ndStart = ndStart.NextVisibleNode;
                                    if (ndStart == null) break;
                                    ToggleNode(ndStart, true);
                                }
                            }
                            else
                            {
                                while (ndStart != ndEnd)
                                {
                                    ndStart = ndStart.PrevVisibleNode;
                                    if (ndStart == null) break;
                                    ToggleNode(ndStart, true);
                                }
                            }
                        }
                        else
                        {
                            // If the selected node is above the clicked node walk up
                            // selecting each Visible node until we reach the end.
                            while (ndStart != ndEnd)
                            {
                                ndStart = ndStart.PrevVisibleNode;
                                if (ndStart == null) break;
                                ToggleNode(ndStart, true);
                            }
                        }
                    }
                }
                else
                {
                    // Just clicked a node, select it
                    SelectSingleNode(node);
                }

                OnAfterSelect(new TreeViewEventArgs(m_SelectedNode));
            }
            finally
            {
                this.EndUpdate();
            }
        }

        public void ClearSelectedNodes()
        {
            try
            {
                foreach (TreeNode node in m_SelectedNodes)
                {
                    node.BackColor = this.BackColor;
                    node.ForeColor = this.ForeColor;
                }
            }
            finally
            {
                m_SelectedNodes.Clear();
                m_SelectedNode = null;
            }
        }

        public TreeNode FindNodeByPath(string path)
        {
            string[] levels = path.Split('\\');
            if (levels.Length > 0)
            {
                TreeNode n = Nodes[levels[0]];
                for (int i = 1; i < levels.Length; i++)
                {
                    if (n.Nodes[levels[i]] != null)
                        n = n.Nodes[levels[i]];
                    else
                        return null;
                }
                return n;
            }
            else return null;
        }

        private void SelectSingleNode(TreeNode node)
        {
            if (node == null)
            {
                return;
            }

            ClearSelectedNodes();
            ToggleNode(node, true);
            node.EnsureVisible();
        }

        private void ToggleNode(TreeNode node, bool bSelectNode)
        {
            if (bSelectNode)
            {
                m_SelectedNode = node;
                if (!m_SelectedNodes.Contains(node))
                {
                    m_SelectedNodes.Add(node);
                }
                node.BackColor = SystemColors.Highlight;
                node.ForeColor = SystemColors.HighlightText;
            }
            else
            {
                m_SelectedNodes.Remove(node);
                node.BackColor = this.BackColor;
                node.ForeColor = this.ForeColor;
            }
        }

        private void HandleException(Exception ex)
        {
            // Perform some error handling here.
            // We don't want to bubble errors to the CLR. 
            MessageBox.Show(ex.Message);
        }

        #endregion

        
    }
}
