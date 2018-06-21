namespace SimpleMDXParser
{
    using System;
    using System.Text;

    internal class MDXCalculateNode : MDXStatementNode
    {
        internal override void AppendMDX(StringBuilder mdx, Formatter f, int indent, bool comma)
        {
            base.AppendCommentAndIndent(mdx, f, indent, comma);
            mdx.Append(f.Keyword("CALCULATE"));
        }

        internal override void FillParseTree(MDXTreeNodeCollection Nodes)
        {
            MDXTreeNode node = Nodes.Add("CALCULATE");
            node.ImageKey = node.SelectedImageKey = "Script.ico";
        }
    }
}

