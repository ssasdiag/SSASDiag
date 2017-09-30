namespace SimpleMDXParser
{
    using System;
    using System.Text;

    public class MDXWithMemberNode : MDXWithNode
    {
        internal MDXWithMemberNode(MDXExpNode name, MDXExpNode exp, MDXListNode<MDXCalcPropNode> calcprops, bool newsyntax) : base(name, exp, calcprops, newsyntax)
        {
        }

        internal override void Analyze(Analyzer analyzer)
        {
            if (base.Exp.GetMDXType() == MDXDataType.String)
            {
                Message m = new Message(base.Exp) {
                    Id = 5,
                    Text = string.Format("Calculated member '{0}' evaluates to a string. Strings are difficult to cache - consider using FORMAT_STRING property for value formatting", base.Name.GetLabel()),
                    Severity = 2
                };
                analyzer.Add(m);
            }
            try
            {
                analyzer.InRHS = true;
                base.Exp.Analyze(analyzer);
            }
            finally
            {
                analyzer.InRHS = false;
            }
            base.Analyze(analyzer);
        }

        internal override void AppendMDX(StringBuilder mdx, Formatter f, int indent, bool comma)
        {
            base.AppendCommentAndIndent(mdx, f, indent, comma);
            if (!base.m_NewSyntax)
            {
                mdx.AppendFormat("{0} {1} {2} {3} ", new object[] { f.Keyword("MEMBER"), base.Name.GetLabel(), f.Keyword("AS"), base.Exp.GetMDX(f, f.Indent(indent)) });
                if ((base.CalcProps != null) && (base.CalcProps.Count > 0))
                {
                    f.AppendCommaAtTheEndOfLine(mdx, true);
                    base.CalcProps.AppendMDX(mdx, f, f.Indent(indent), true);
                }
            }
            else if ((base.Exp.GetType() == typeof(MDXIDNode)) && (base.Exp as MDXIDNode).GetLabel().Equals("NULL", StringComparison.InvariantCultureIgnoreCase))
            {
                mdx.AppendFormat("{0} ", base.Name.GetLabel());
            }
            else
            {
                mdx.AppendFormat("{0} = {1}", base.Name.GetLabel(), base.Exp.GetMDX(f, f.Indent(indent)));
            }
        }

        internal override void FillParseTree(TreeNodeCollection Nodes)
        {
            TreeNode node = Nodes.Add(base.GetCMName());
            node.Tag = this;
            node.ImageKey = node.SelectedImageKey = "MeasureCalculated.ico";
            base.Exp.FillParseTree(node.Nodes);
            if (base.CalcProps != null)
            {
                foreach (MDXCalcPropNode node2 in base.CalcProps)
                {
                    TreeNode node3 = node.Nodes.Add(node2.PropertyName);
                    node3.ImageKey = node3.SelectedImageKey = "FolderClosed.ico";
                    node3.Tag = node2;
                    node2.PropertyExp.FillParseTree(node3.Nodes);
                }
            }
        }

        public override MDXExpNode GetExpNode()
        {
            return base.Exp;
        }

        public override string GetMDXWithCube(string cube)
        {
            return string.Format("MEMBER [{0}].{1} AS {2}", cube, base.Name.GetMDX(-1), base.Exp.GetMDX(-1));
        }
    }
}

