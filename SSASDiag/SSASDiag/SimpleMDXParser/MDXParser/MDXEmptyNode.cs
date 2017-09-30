namespace SimpleMDXParser
{
    using System;
    using System.Text;

    internal class MDXEmptyNode : MDXExpNode
    {
        internal MDXEmptyNode()
        {
        }

        internal override TreeNode AddParseNode(TreeNodeCollection Nodes)
        {
            TreeNode node = Nodes.Add(this.GetLabel());
            node.ImageKey = node.SelectedImageKey = "Clear.ico";
            return node;
        }

        internal override void AppendMDX(StringBuilder mdx, Formatter f, int indent, bool comma)
        {
            if (comma && !f.Options.CommaBeforeNewLine)
            {
                mdx.Append(",");
            }
        }

        public override string GetLabel()
        {
            return "";
        }

        public override MDXDataType GetMDXType()
        {
            return MDXDataType.Missing;
        }
    }
}

