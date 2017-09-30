namespace SimpleMDXParser
{
    using System;
    using System.Text;

    internal class MDXEmptyStmtNode : MDXStatementNode
    {
        internal override void AppendMDX(StringBuilder mdx, Formatter f, int indent, bool comma)
        {
        }

        internal override void FillParseTree(TreeNodeCollection Nodes)
        {
        }
    }
}

