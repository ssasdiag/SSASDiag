namespace SimpleMDXParser
{
    using System;
    using System.Text;

    internal class MDXCaseNode : MDXExpNode
    {
        private MDXExpNode m_Else;
        private MDXListNode<MDXWhenNode> m_WhenList;

        internal MDXCaseNode(MDXListNode<MDXWhenNode> whenlist, MDXExpNode elseexp)
        {
            this.m_WhenList = whenlist;
            this.m_Else = elseexp;
        }

        internal override void Analyze(Analyzer analyzer)
        {
            try
            {
                base.Analyze(analyzer);
                base.StartAnalyze(analyzer);
                if (!analyzer.InCommonSubExpr)
                {
                    this.m_WhenList.Analyze(analyzer);
                    if (this.m_Else != null)
                    {
                        this.m_Else.Analyze(analyzer);
                    }
                }
            }
            finally
            {
                base.EndAnalyze(analyzer);
            }
        }

        internal override void AppendMDX(StringBuilder mdx, Formatter f, int indent, bool comma)
        {
            f.AppendIndent(mdx, indent, comma);
            mdx.Append(f.Keyword("CASE "));
            foreach (MDXWhenNode node in this.m_WhenList)
            {
                node.AppendMDX(mdx, f, f.Indent(indent));
            }
            if (this.m_Else != null)
            {
                f.AppendIndent(mdx, f.Indent(indent));
                mdx.AppendFormat("{0} {1}", f.Keyword("ELSE"), this.m_Else.GetMDX(f, f.Indent(f.Indent(indent))));
            }
            f.AppendIndent(mdx, indent);
            mdx.Append(f.Keyword("END"));
        }

        public override void BuildDependencyGraph(DependencyGraph g, Vertex root)
        {
            foreach (MDXWhenNode node in this.m_WhenList)
            {
                node.BuildDependencyGraph(g, root);
            }
            if (this.m_Else != null)
            {
                this.m_Else.BuildDependencyGraph(g, root);
            }
        }

        internal override void FillParseTree(MDXTreeNodeCollection Nodes)
        {
            MDXTreeNode node = this.AddParseNode(Nodes);
            this.m_WhenList.FillParseTree(node.Nodes);
            if (this.m_Else != null)
            {
                MDXTreeNode node2 = node.Nodes.Add("ELSE");
                node2.Tag = this.m_Else;
                this.m_Else.FillParseTree(node2.Nodes);
            }
        }

        public override string GetLabel()
        {
            return "CASE";
        }

        public override MDXDataType GetMDXType()
        {
            return this.m_WhenList.Get(0).GetThen().GetMDXType();
        }

        internal override void SetOuterIterator(MDXNode iter)
        {
            base.m_OuterIterator = iter;
            if (this.m_Else != null)
            {
                this.m_Else.SetOuterIterator(iter);
            }
        }
    }
}

