namespace SimpleMDXParser
{
    using System;
    using System.Text;

    internal class MDXFreezeNode : MDXStatementNode
    {
        internal override void AppendMDX(StringBuilder mdx, Formatter f, int indent, bool comma)
        {
            base.AppendCommentAndIndent(mdx, f, indent, comma);
            mdx.Append(f.Keyword("FREEZE"));
        }

        internal override void FillParseTree(TreeNodeCollection Nodes)
        {
            TreeNode node = Nodes.Add("FREEZE");
            node.ImageKey = node.SelectedImageKey = "Script.ico";
        }
    }
}

