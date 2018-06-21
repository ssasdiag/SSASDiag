namespace SimpleMDXParser
{
    using System;
    using System.Text;

    internal class MDXScopeNode : MDXStatementNode
    {
        private MDXEndScopeNode m_EndScope;
        private MDXExpNode m_Scope;
        private MDXScriptNode m_Script;

        internal MDXScopeNode(MDXExpNode scope, MDXScriptNode script, MDXEndScopeNode endscope)
        {
            this.m_Scope = scope;
            this.m_Script = script;
            this.m_EndScope = endscope;
        }

        internal override void Analyze(Analyzer analyzer)
        {
            base.Analyze(analyzer);
            try
            {
                analyzer.InLHS = true;
                this.m_Scope.Analyze(analyzer);
            }
            finally
            {
                analyzer.InLHS = false;
            }
            this.m_Script.Analyze(analyzer);
        }

        internal override void AppendMDX(StringBuilder mdx, Formatter f, int indent, bool comma)
        {
            base.AppendCommentAndIndent(mdx, f, indent, comma);
            mdx.Append(f.Keyword("SCOPE "));
            this.m_Scope.AppendMDX(mdx, f, f.Indent(indent));
            mdx.Append(";");
            this.m_Script.AppendMDX(mdx, f, f.Indent(indent));
            this.m_EndScope.AppendMDX(mdx, f, indent);
        }

        internal override void FillParseTree(MDXTreeNodeCollection Nodes)
        {
            MDXTreeNode node = Nodes.Add("SCOPE");
            node.Tag = this;
            node.ImageKey = node.SelectedImageKey = "Scope.ico";
            MDXTreeNode node2 = node.Nodes.Add("Subcube");
            node2.Tag = this.m_Scope;
            node2.ImageKey = node2.SelectedImageKey = "CellCalculation.ico";
            this.m_Scope.FillParseTree(node2.Nodes);
            this.m_Script.FillParseTree(node.Nodes);
        }
    }
}

