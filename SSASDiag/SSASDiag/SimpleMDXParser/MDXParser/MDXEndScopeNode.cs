namespace SimpleMDXParser
{
    using System;
    using System.Text;

    internal class MDXEndScopeNode : MDXNode
    {
        internal override void AppendMDX(StringBuilder mdx, Formatter f, int indent, bool comma)
        {
            base.AppendCommentAndIndent(mdx, f, indent, comma);
            mdx.Append(f.Keyword("END SCOPE"));
        }

        internal override void FillParseTree(TreeNodeCollection Nodes)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}

