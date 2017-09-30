namespace SimpleMDXParser
{
    using System;
    using System.Text;

    public class MDXCalcPropNode : MDXNode
    {
        private MDXExpNode m_PropExp;
        private string m_PropName;

        internal MDXCalcPropNode(string propname, MDXExpNode exp)
        {
            this.m_PropName = propname;
            this.m_PropExp = exp;
        }

        internal override void AppendMDX(StringBuilder mdx, Formatter f, int indent, bool comma)
        {
            base.AppendCommentAndIndent(mdx, f, indent, comma);
            mdx.AppendFormat("{0} = {1} ", this.m_PropName, this.m_PropExp.GetMDX(f, indent));
        }

        internal override void FillParseTree(TreeNodeCollection Nodes)
        {
        }

        internal MDXExpNode PropertyExp
        {
            get
            {
                return this.m_PropExp;
            }
        }

        internal string PropertyName
        {
            get
            {
                return this.m_PropName;
            }
        }
    }
}

