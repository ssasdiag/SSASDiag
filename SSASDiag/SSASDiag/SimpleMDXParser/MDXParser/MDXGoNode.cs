namespace SimpleMDXParser
{
    using System;
    using System.Text;

    public class MDXGoNode : MDXStatementNode
    {
        internal override void AppendMDX(StringBuilder mdx, Formatter f, int indent, bool comma)
        {
            f.AppendIndent(mdx, indent, comma);
            mdx.Append("GO");
        }

        internal override void FillParseTree(TreeNodeCollection Nodes)
        {
        }
    }
}

