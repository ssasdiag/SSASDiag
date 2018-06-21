namespace SimpleMDXParser
{
    using System;
    using System.Web.UI.WebControls;

    public class WebTreeNode : MDXTreeNode
    {
        private System.Web.UI.WebControls.TreeNode m_Node;

        public WebTreeNode(System.Web.UI.WebControls.TreeNode node)
        {
            node.NavigateUrl = "#";
            this.m_Node = node;
        }

        public override string ImageKey
        {
            get
            {
                return this.m_Node.ImageUrl;
            }
            set
            {
                this.m_Node.ImageUrl = string.Format(@"Icons\{0}", value.ToString());
            }
        }

        public override MDXTreeNodeCollection Nodes
        {
            get
            {
                return new WebTreeNodeCollection(this.m_Node.ChildNodes);
            }
        }

        public override MDXTreeNode Parent
        {
            get
            {
                return new WebTreeNode(this.m_Node.Parent);
            }
        }

        public override string SelectedImageKey
        {
            get
            {
                return "";
            }
            set
            {
            }
        }

        public override object Tag
        {
            get
            {
                return this.m_Node.Value;
            }
            set
            {
                this.m_Node.Value = value.ToString();
            }
        }
    }
}

