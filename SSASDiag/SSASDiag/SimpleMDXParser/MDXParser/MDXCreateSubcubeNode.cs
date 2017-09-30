namespace SimpleMDXParser
{
    using System;
    using System.Text;

    internal class MDXCreateSubcubeNode : MDXStatementNode
    {
        private string m_Name;
        private MDXSelectNode m_Select;

        internal MDXCreateSubcubeNode(string name, MDXSelectNode select)
        {
            this.m_Name = name;
            this.m_Select = select;
        }

        internal override void Analyze(Analyzer analyzer)
        {
            base.Analyze(analyzer);
            this.m_Select.Analyze(analyzer);
        }

        internal override void AppendMDX(StringBuilder mdx, Formatter f, int indent, bool comma)
        {
            f.AppendIndent(mdx, indent, comma);
            mdx.AppendFormat("CREATE SUBCUBE {0} AS ", this.m_Name);
            this.m_Select.AppendMDX(mdx, f, f.Indent(indent));
        }

        internal override void FillParseTree(TreeNodeCollection Nodes)
        {
            TreeNode node = Nodes.Add("CREATE SUBCUBE");
            this.m_Select.FillParseTree(node.Nodes);
        }
    }
}

