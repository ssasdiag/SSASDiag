namespace SimpleMDXParser
{
    using System;
    using System.Text;

    internal class MDXWhereNode : MDXNode
    {
        private MDXExpNode m_Exp;

        internal override void Analyze(Analyzer analyzer)
        {
            base.Analyze(analyzer);
            this.m_Exp.Analyze(analyzer);
        }

        internal override void AppendMDX(StringBuilder mdx, Formatter f, int indent, bool comma)
        {
            this.m_Exp.AppendMDX(mdx, f, indent, comma);
        }

        internal override void FillParseTree(TreeNodeCollection Nodes)
        {
            TreeNode node = Nodes.Add("WHERE");
            node.ImageKey = node.SelectedImageKey = "Level2.ico";
            this.m_Exp.FillParseTree(node.Nodes);
        }

        internal void Set(MDXExpNode exp)
        {
            this.m_Exp = exp;
        }
    }
}

