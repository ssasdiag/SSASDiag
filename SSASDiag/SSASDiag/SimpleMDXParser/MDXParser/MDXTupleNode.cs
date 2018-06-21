namespace SimpleMDXParser
{
    using System;
    using System.Text;

    internal class MDXTupleNode : MDXExpNode
    {
        internal MDXExpListNode m_List;

        internal MDXTupleNode(MDXExpListNode list)
        {
            this.m_List = list;
        }

        internal override void Analyze(Analyzer analyzer)
        {
            try
            {
                base.Analyze(analyzer);
                base.StartAnalyze(analyzer);
                if (!analyzer.InCommonSubExpr)
                {
                    this.m_List.Analyze(analyzer);
                }
            }
            finally
            {
                base.EndAnalyze(analyzer);
            }
        }

        internal override void AppendMDX(StringBuilder mdx, Formatter f, int indent, bool comma)
        {
            int num = indent;
            if (this.m_List.Count > 1)
            {
                base.AppendCommentAndIndent(mdx, f, indent, comma);
                num = f.Indent(indent);
            }
            else if (comma)
            {
                base.AppendCommentAndIndent(mdx, f, indent, comma);
            }
            mdx.Append("(");
            bool flag = true;
            foreach (object obj2 in this.m_List)
            {
                f.AppendCommaAtTheEndOfLine(mdx, !flag);
                ((MDXNode) obj2).AppendMDX(mdx, f, num, !flag);
                flag = false;
            }
            if (this.m_List.Count > 1)
            {
                f.AppendIndent(mdx, indent);
            }
            mdx.Append(")");
        }

        public override void BuildDependencyGraph(DependencyGraph g, Vertex root)
        {
            foreach (MDXExpNode node in this.m_List)
            {
                node.BuildDependencyGraph(g, root);
            }
        }

        internal override void FillParseTree(MDXTreeNodeCollection Nodes)
        {
            MDXTreeNode node = this.AddParseNode(Nodes);
            foreach (object obj2 in this.m_List)
            {
                ((MDXNode) obj2).FillParseTree(node.Nodes);
            }
        }

        public override string GetLabel()
        {
            foreach (MDXExpNode node in this.m_List)
            {
                if (MDXDataType.Set == node.GetMDXType())
                {
                    return "(crossjoin)";
                }
            }
            return "(tuple)";
        }

        public override MDXDataType GetMDXType()
        {
            foreach (MDXExpNode node in this.m_List)
            {
                if (MDXDataType.Set == node.GetMDXType())
                {
                    return MDXDataType.Set;
                }
            }
            return MDXDataType.Tuple;
        }

        internal override bool IsCacheTrivial()
        {
            foreach (MDXExpNode node in this.m_List)
            {
                if (MDXExpNode.IsCachePrimitive(node))
                {
                    return false;
                }
            }
            return true;
        }

        internal override void SetOuterIterator(MDXNode iter)
        {
            base.m_OuterIterator = iter;
            foreach (MDXExpNode node in this.m_List)
            {
                node.SetOuterIterator(iter);
            }
        }
    }
}

