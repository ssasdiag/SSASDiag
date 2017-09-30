namespace SimpleMDXParser
{
    using System;
    using System.Text;

    internal class MDXParamNode : MDXExpNode
    {
        private string m_Param;

        internal MDXParamNode(string literal)
        {
            this.m_Param = literal;
        }

        internal override void AppendMDX(StringBuilder mdx, Formatter f, int indent, bool comma)
        {
            mdx.Append(f.Highlight(this.m_Param));
        }

        internal override void FillParseTree(TreeNodeCollection Nodes)
        {
            TreeNode node = Nodes.Add(this.GetLabel());
            node.Tag = this;
            node.ImageKey = node.SelectedImageKey = "FolderClosed.ico";
        }

        public override string GetDebugMDX()
        {
            return string.Format("\"{0}\"", this.m_Param);
        }

        public override string GetLabel()
        {
            return this.m_Param.Remove(0, 1);
        }

        public override MDXDataType GetMDXType()
        {
            return MDXDataType.Number;
        }

        internal override bool IsCacheTrivial()
        {
            return true;
        }
    }
}

