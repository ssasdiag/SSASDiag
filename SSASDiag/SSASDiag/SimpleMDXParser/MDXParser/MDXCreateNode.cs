namespace SimpleMDXParser
{
    using System;
    using System.Text;

    internal class MDXCreateNode : MDXStatementNode
    {
        private bool m_Hidden;
        private MDXWithListNode m_Withs;

        internal MDXCreateNode(MDXWithListNode withs, bool hidden)
        {
            this.m_Withs = withs;
            this.m_Hidden = hidden;
        }

        internal override void Analyze(Analyzer analyzer)
        {
            base.Analyze(analyzer);
            this.m_Withs.Analyze(analyzer);
        }

        internal override void AppendMDX(StringBuilder mdx, Formatter f, int indent, bool comma)
        {
            base.AppendComment(mdx, f, indent);
            if (this.m_Withs.Count > 0)
            {
                this.m_Withs.AppendComment(mdx, f, indent);
                f.AppendIndent(mdx, indent, comma);
                mdx.Append(f.Keyword("CREATE "));
                if (this.m_Hidden)
                {
                    mdx.Append(f.Keyword("HIDDEN "));
                }
                this.m_Withs.AppendMDX(mdx, f, f.Indent(indent));
            }
        }

        public override void BuildDependencyGraph(DependencyGraph g, Vertex root)
        {
            foreach (MDXWithNode node in this.m_Withs)
            {
                node.BuildDependencyGraph(g, root);
            }
        }

        internal override void FillParseTree(MDXTreeNodeCollection Nodes)
        {
            this.m_Withs.FillParseTree(Nodes);
        }
    }
}

