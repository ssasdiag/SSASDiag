namespace SimpleMDXParser
{
    using System;
    using System.Text;

    internal class MDXWithSetNode : MDXWithNode
    {
        internal MDXWithSetNode(MDXExpNode name, MDXExpNode exp, MDXListNode<MDXCalcPropNode> calcprops, bool newsyntax) : base(name, exp, calcprops, newsyntax)
        {
        }

        internal override void Analyze(Analyzer analyzer)
        {
            try
            {
                analyzer.InLHS = true;
                base.Exp.Analyze(analyzer);
            }
            finally
            {
                analyzer.InLHS = false;
            }
            base.Analyze(analyzer);
        }

        internal override void AppendMDX(StringBuilder mdx, Formatter f, int indent, bool comma)
        {
            base.AppendCommentAndIndent(mdx, f, indent, comma);
            mdx.AppendFormat("{0} {1} {2} {3} ", new object[] { f.Keyword("SET"), base.Name.GetMDX(f, -1), f.Keyword("AS"), base.Exp.GetMDX(f, f.Indent(indent)) });
        }

        internal override void FillParseTree(TreeNodeCollection Nodes)
        {
            TreeNode node = Nodes.Add(base.GetJustName());
            node.Tag = this;
            node.ImageKey = node.SelectedImageKey = "NamedSet.ico";
            base.Exp.FillParseTree(node.Nodes);
        }

        public override MDXExpNode GetExpNode()
        {
            return base.Exp;
        }

        public override string GetMDXWithCube(string cube)
        {
            return string.Format("SET [{0}].{1} AS {2} ", cube, base.Name.GetMDX(-1), base.Exp.GetMDX(-1));
        }
    }
}

