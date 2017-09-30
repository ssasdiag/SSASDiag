namespace SimpleMDXParser
{
    using System;
    using System.Text;

    internal class MDXPropertyNode : MDXBaseFunctionNode
    {
        internal MDXPropertyNode(string funcname, MDXExpListNode args, MDXObject obj) : base(funcname, args, obj, true)
        {
        }

        internal override void Analyze(Analyzer analyzer)
        {
            base.Analyze(analyzer);
        }

        internal override void AppendMDX(StringBuilder mdx, Formatter f, int indent, bool comma)
        {
            base.m_Arguments[0].AppendMDX(mdx, f, indent, comma);
            string str = f.Options.ColorFunctionNames ? f.Keyword(base.m_Function) : base.m_Function;
            mdx.AppendFormat(".{0}", str);
            if (base.m_Arguments.Count > 1)
            {
                mdx.Append("(");
            }
            bool flag = true;
            bool flag2 = true;
            foreach (object obj2 in base.m_Arguments)
            {
                if (flag)
                {
                    flag = false;
                }
                else
                {
                    f.AppendCommaAtTheEndOfLine(mdx, !flag2);
                    (obj2 as MDXNode).AppendMDX(mdx, f, indent, !flag2);
                    flag2 = false;
                }
            }
            if (base.m_Arguments.Count > 1)
            {
                mdx.Append(")");
            }
        }

        public override string GetLabel()
        {
            return base.m_Function;
        }

        public override MDXDataType GetMDXType()
        {
            if (base.m_Object != null)
            {
                return base.m_Object.ReturnType;
            }
            return MDXDataType.Member;
        }
    }
}

