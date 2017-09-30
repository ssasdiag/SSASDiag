namespace SimpleMDXParser
{
    using System;
    using System.Windows.Forms;

    public class WinFormsTreeNode : TreeNode
    {
        private System.Windows.Forms.TreeNode m_Node;

        public WinFormsTreeNode(System.Windows.Forms.TreeNode node)
        {
            this.m_Node = node;
        }

        public override string ImageKey
        {
            get
            {
                return this.m_Node.ImageKey;
            }
            set
            {
                this.m_Node.ImageKey = value;
            }
        }

        public override TreeNodeCollection Nodes
        {
            get
            {
                return new WinFormsTreeNodeCollection(this.m_Node.Nodes);
            }
        }

        public override TreeNode Parent
        {
            get
            {
                return new WinFormsTreeNode(this.m_Node.Parent);
            }
        }

        public override string SelectedImageKey
        {
            get
            {
                return this.m_Node.SelectedImageKey;
            }
            set
            {
                this.m_Node.SelectedImageKey = value;
            }
        }

        public override object Tag
        {
            get
            {
                return this.m_Node.Tag;
            }
            set
            {
                this.m_Node.Tag = value;
            }
        }
    }
}

