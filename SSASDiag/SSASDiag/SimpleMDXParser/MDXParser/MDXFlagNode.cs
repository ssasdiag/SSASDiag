namespace SimpleMDXParser
{
    using System;
    using System.Text;

    internal class MDXFlagNode : MDXExpNode
    {
        private string m_Flag;

        internal MDXFlagNode(string flag)
        {
            this.m_Flag = flag;
        }

        internal override void AppendMDX(StringBuilder mdx, Formatter f, int indent, bool comma)
        {
            f.AppendIndent(mdx, indent, comma);
            mdx.Append(f.Keyword(this.m_Flag.ToUpper()));
        }

        public override string GetLabel()
        {
            return this.m_Flag;
        }

        public override MDXDataType GetMDXType()
        {
            return MDXDataType.Flag;
        }

        internal override bool IsCacheTrivial()
        {
            return true;
        }
    }
}

