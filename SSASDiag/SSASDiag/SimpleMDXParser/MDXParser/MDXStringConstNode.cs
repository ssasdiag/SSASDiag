namespace SimpleMDXParser
{
    using System;
    using System.Text;

    internal class MDXStringConstNode : MDXConstNode
    {
        private string m_String;
        internal bool SingleQuote;

        internal MDXStringConstNode(string LexString)
        {
            this.m_String = LexString;
            this.SingleQuote = false;
        }

        internal override void AppendMDX(StringBuilder mdx, Formatter f, int indent, bool comma)
        {
            if (comma)
            {
                f.AppendIndent(mdx, indent, comma);
            }
            mdx.Append(f.StringConst(this.GetLabel()));
        }

        public override string GetLabel()
        {
            if (this.SingleQuote)
            {
                return string.Format("'{0}'", this.m_String);
            }
            return string.Format("\"{0}\"", this.m_String);
        }

        public override MDXDataType GetMDXType()
        {
            return MDXDataType.String;
        }

        internal string GetString()
        {
            return this.m_String;
        }
    }
}

