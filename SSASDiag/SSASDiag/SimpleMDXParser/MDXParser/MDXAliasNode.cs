namespace SimpleMDXParser
{
    using System;
    using System.Text;

    internal class MDXAliasNode : MDXExpNode
    {
        private string m_Alias;
        private MDXExpNode m_Exp;

        internal MDXAliasNode(MDXExpNode exp, string alias)
        {
            this.m_Exp = exp;
            this.m_Alias = alias;
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
            this.m_Exp.AppendMDX(mdx, f, indent, comma);
            mdx.AppendFormat(" {0} {1}", f.Keyword("AS"), this.m_Alias);
        }

        internal override void FillParseTree(MDXTreeNodeCollection Nodes)
        {
            this.m_Exp.FillParseTree(Nodes);
        }

        public override string GetLabel()
        {
            return this.m_Alias;
        }

        public override MDXDataType GetMDXType()
        {
            return MDXDataType.Set;
        }
    }
}

