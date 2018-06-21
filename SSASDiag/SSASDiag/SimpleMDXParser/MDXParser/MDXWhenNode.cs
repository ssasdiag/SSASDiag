namespace SimpleMDXParser
{
    using System;
    using System.Text;

    internal class MDXWhenNode : MDXNode
    {
        private MDXExpNode m_Then;
        private MDXExpNode m_When;

        internal MDXWhenNode(MDXExpNode when, MDXExpNode then)
        {
            this.m_When = when;
            this.m_Then = then;
        }

        internal override void Analyze(Analyzer analyzer)
        {
            base.Analyze(analyzer);
            this.m_When.Analyze(analyzer);
            this.m_Then.Analyze(analyzer);
        }

        internal override void AppendMDX(StringBuilder mdx, Formatter f, int indent, bool comma)
        {
            f.AppendIndent(mdx, indent, comma);
            if (indent < 0)
            {
                mdx.Append(" ");
            }
            mdx.AppendFormat("{0} {1} ", f.Keyword("WHEN"), this.m_When.GetMDX(f, f.Indent(indent)));
            f.AppendIndent(mdx, indent);
            if (indent < 0)
            {
                mdx.Append(" ");
            }
            mdx.AppendFormat("{0} {1}", f.Keyword("THEN"), this.m_Then.GetMDX(f, f.Indent(indent)));
        }

        internal override void FillParseTree(MDXTreeNodeCollection Nodes)
        {
            MDXTreeNode node = Nodes.Add("WHEN");
            node.Tag = this;
            this.m_When.FillParseTree(node.Nodes);
            MDXTreeNode node2 = node.Nodes.Add("THEN");
            node2.Tag = this.m_Then;
            this.m_Then.FillParseTree(node2.Nodes);
        }

        internal MDXExpNode GetThen()
        {
            return this.m_Then;
        }
    }
}

