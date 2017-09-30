namespace SimpleMDXParser
{
    using System;
    using System.Web.UI.WebControls;

    public class WebTreeNodeCollection : TreeNodeCollection
    {
        private System.Web.UI.WebControls.TreeNodeCollection m_Nodes;

        public WebTreeNodeCollection(System.Web.UI.WebControls.TreeNodeCollection nodes)
        {
            this.m_Nodes = nodes;
        }

        public override TreeNode Add(string label)
        {
            System.Web.UI.WebControls.TreeNode child = new System.Web.UI.WebControls.TreeNode(label);
            this.m_Nodes.Add(child);
            return new WebTreeNode(child);
        }
    }
}

