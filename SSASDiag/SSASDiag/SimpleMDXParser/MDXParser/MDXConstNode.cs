namespace SimpleMDXParser
{
    using System;
    using System.Text;

    internal abstract class MDXConstNode : MDXExpNode
    {
        protected MDXConstNode()
        {
        }

        internal override void AppendMDX(StringBuilder mdx, Formatter f, int indent, bool comma)
        {
            if (comma)
            {
                f.AppendIndent(mdx, indent, comma);
            }
            mdx.Append(this.GetLabel());
        }

        public override MDXDataType GetMDXType()
        {
            return MDXDataType.Number;
        }

        internal override bool IsCacheTrivial()
        {
            return true;
        }
    }
}

