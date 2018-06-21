namespace SimpleMDXParser
{
    using System;
    using System.Text;

    internal class MDXWithCalcCellNode : MDXWithNode
    {
        private string m_Scope;

        internal MDXWithCalcCellNode(MDXExpNode name, string scope, MDXExpNode exp, MDXListNode<MDXCalcPropNode> calcprops) : base(name, exp, calcprops, false)
        {
            this.m_Scope = scope;
        }

        internal override void AppendMDX(StringBuilder mdx, Formatter f, int indent, bool comma)
        {
            base.AppendCommentAndIndent(mdx, f, indent, comma);
            mdx.AppendFormat("CELL CALCULATION {0} ", base.Name.GetMDX(f, -1));
            f.AppendIndent(mdx, f.Indent(indent));
            mdx.AppendFormat("FOR {0} ", this.m_Scope);
            f.AppendIndent(mdx, f.Indent(indent));
            mdx.Append(f.Keyword("AS "));
            base.Exp.AppendMDX(mdx, f, f.Indent(f.Indent(indent)));
        }

        internal override void FillParseTree(MDXTreeNodeCollection Nodes)
        {
            MDXTreeNode node = Nodes.Add(base.GetJustName());
            node.Tag = this;
            node.ImageKey = node.SelectedImageKey = "Script.ico";
        }

        public override MDXExpNode GetExpNode()
        {
            return null;
        }

        public override string GetMDXWithCube(string cube)
        {
            return "?";
        }
    }
}

