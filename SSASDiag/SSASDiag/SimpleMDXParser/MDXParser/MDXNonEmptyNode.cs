namespace SimpleMDXParser
{
    using System;
    using System.Text;

    internal class MDXNonEmptyNode : MDXExpNode
    {
        private MDXExpListNode m_Filters = new MDXExpListNode();
        private MDXExpNode m_Set;

        internal MDXNonEmptyNode()
        {
        }

        internal void AddFilter(MDXExpNode filter)
        {
            this.m_Filters.Add(filter);
        }

        internal override void Analyze(Analyzer analyzer)
        {
            base.Analyze(analyzer);
            if (this.m_Set.GetType() == typeof(MDXFunctionNode))
            {
                MDXFunctionNode set = this.m_Set as MDXFunctionNode;
                if (set.m_Function.Equals("Filter", StringComparison.InvariantCultureIgnoreCase))
                {
                    Message m = new Message(this) {
                        Id = 0x11,
                        Text = "'NON EMPTY Filter(set, cond)' can be rewritten more efficiently as 'NON EMPTY set HAVING cond'",
                        Severity = 2
                    };
                    analyzer.Add(m);
                }
            }
            this.m_Set.Analyze(analyzer);
        }

        internal override void AppendMDX(StringBuilder mdx, Formatter f, int indent, bool comma)
        {
            f.AppendIndent(mdx, indent, comma);
            mdx.Append("NON EMPTY ");
            this.m_Set.AppendMDX(mdx, f, f.Indent(indent));
        }

        internal override void FillParseTree(MDXTreeNodeCollection Nodes)
        {
            MDXTreeNode node = this.AddParseNode(Nodes);
            this.m_Set.FillParseTree(node.Nodes);
        }

        public override string GetDebugMDX()
        {
            StringBuilder mdx = new StringBuilder();
            Formatter f = new Formatter();
            mdx.Append("NonEmpty(");
            this.m_Set.AppendMDX(mdx, f, -1);
            bool flag = true;
            foreach (MDXExpNode node in this.m_Filters)
            {
                if (flag)
                {
                    mdx.Append(", (");
                }
                else
                {
                    mdx.Append(",");
                }
                flag = false;
                node.AppendMDX(mdx, f, -1);
            }
            if (!flag)
            {
                mdx.Append(")");
            }
            mdx.Append(")");
            return mdx.ToString();
        }

        public override string GetLabel()
        {
            return "NON EMPTY";
        }

        public override MDXDataType GetMDXType()
        {
            return MDXDataType.Set;
        }

        internal MDXExpNode GetSet()
        {
            return this.m_Set;
        }

        internal void SetSet(MDXExpNode set)
        {
            this.m_Set = set;
        }
    }
}

