namespace SimpleMDXParser
{
    using System;
    using System.Text;

    internal class MDXUnaryOpNode : MDXExpNode
    {
        internal MDXExpNode m_Exp;
        internal string m_Op;

        internal MDXUnaryOpNode(string LexString, MDXExpNode exp)
        {
            this.m_Op = LexString.ToUpper();
            this.m_Exp = exp;
            if (this.m_Exp.GetType() == typeof(MDXBinOpNode))
            {
                (this.m_Exp as MDXBinOpNode).SetUpstreamOp(this.m_Op);
            }
        }

        internal override void Analyze(Analyzer analyzer)
        {
            base.Analyze(analyzer);
            try
            {
                base.StartAnalyze(analyzer);
                if (!analyzer.InCommonSubExpr)
                {
                    this.m_Exp.Analyze(analyzer);
                }
            }
            finally
            {
                base.EndAnalyze(analyzer);
            }
        }

        internal override void AppendMDX(StringBuilder mdx, Formatter f, int indent, bool comma)
        {
            base.AppendCommentAndIndent(mdx, f, indent, comma);
            if (this.m_Op == "-")
            {
                mdx.AppendFormat("-{0}", this.m_Exp.GetMDX(f, f.Indent(indent)));
            }
            else
            {
                mdx.AppendFormat("({0} {1})", f.Keyword(this.m_Op), this.m_Exp.GetMDX(f, f.Indent(indent)));
            }
        }

        internal override void FillParseTree(TreeNodeCollection Nodes)
        {
            TreeNode node = this.AddParseNode(Nodes);
            this.m_Exp.FillParseTree(node.Nodes);
        }

        public override string GetLabel()
        {
            return this.m_Op;
        }

        public override MDXDataType GetMDXType()
        {
            if (this.m_Op.Equals("existing", StringComparison.InvariantCultureIgnoreCase) || this.m_Op.Equals("distinct", StringComparison.InvariantCultureIgnoreCase))
            {
                return MDXDataType.Set;
            }
            if (this.m_Exp.GetMDXType() == MDXDataType.Set)
            {
                return MDXDataType.Set;
            }
            return MDXDataType.Number;
        }

        internal override bool IsCacheTrivial()
        {
            return this.m_Exp.IsCacheTrivial();
        }

        internal override void SetOuterIterator(MDXNode iter)
        {
            base.m_OuterIterator = iter;
            this.m_Exp.SetOuterIterator(iter);
        }
    }
}

