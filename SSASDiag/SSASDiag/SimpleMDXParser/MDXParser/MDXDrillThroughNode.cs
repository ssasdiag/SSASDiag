namespace SimpleMDXParser
{
    using System;
    using System.Text;

    internal class MDXDrillThroughNode : MDXStatementNode
    {
        private MDXExpListNode m_ReturnAttrs;
        private MDXSelectNode m_Select;

        internal MDXDrillThroughNode(MDXSelectNode select, MDXExpListNode returnattrs)
        {
            this.m_Select = select;
            this.m_ReturnAttrs = returnattrs;
        }

        internal override void AppendMDX(StringBuilder mdx, Formatter f, int indent, bool comma)
        {
            f.AppendIndent(mdx, indent, comma);
            mdx.Append("DRILLTHROUGH ");
            this.m_Select.AppendMDX(mdx, f, f.Indent(indent));
            if (this.m_ReturnAttrs != null)
            {
                f.AppendIndent(mdx, f.Indent(indent));
                mdx.Append("RETURN ");
                this.m_ReturnAttrs.AppendMDX(mdx, f, f.Indent(f.Indent(indent)));
            }
        }

        internal override void FillParseTree(TreeNodeCollection Nodes)
        {
            TreeNode node = Nodes.Add("DRILLTHROUGH");
            node.Tag = this;
            node.ImageKey = node.SelectedImageKey = "MDXQuery.ico";
            this.m_Select.FillParseTree(node.Nodes);
            if (this.m_ReturnAttrs != null)
            {
                this.m_ReturnAttrs.FillParseTree(node.Nodes);
            }
        }
    }
}

