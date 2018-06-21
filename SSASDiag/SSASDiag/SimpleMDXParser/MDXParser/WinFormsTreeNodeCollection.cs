namespace SimpleMDXParser
{
    using System;
    using System.Windows.Forms;

    public class WinFormsTreeNodeCollection : MDXTreeNodeCollection
    {
        private System.Windows.Forms.TreeNodeCollection m_Nodes;

        public WinFormsTreeNodeCollection(System.Windows.Forms.TreeNodeCollection nodes)
        {
            this.m_Nodes = nodes;
        }

        public override MDXTreeNode Add(string label)
        {
            return new WinFormsTreeNode(this.m_Nodes.Add(label));
        }
    }
}

