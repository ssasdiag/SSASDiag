namespace SimpleMDXParser
{
    using System;
    using System.Text;

    internal class MDXAxisNode : MDXNode
    {
        private MDXExpListNode m_DimProps;
        private MDXExpNode m_Exp;
        private MDXExpNode m_Having;
        private string m_Name;

        internal MDXAxisNode(string name, MDXExpNode exp, MDXExpListNode dimprops, MDXExpNode having)
        {
            this.m_Name = name;
            this.m_Exp = exp;
            this.m_Having = having;
            this.m_DimProps = dimprops;
            if (this.m_Having != null)
            {
                this.m_Having.SetOuterIterator(this.m_Exp);
            }
        }

        internal override void Analyze(Analyzer analyzer)
        {
            base.Analyze(analyzer);
            if (this.m_Exp.GetType() == typeof(MDXFunctionNode))
            {
                MDXFunctionNode exp = this.m_Exp as MDXFunctionNode;
                if (exp.m_Function.Equals("NonEmpty", StringComparison.InvariantCultureIgnoreCase))
                {
                    Message m = new Message(this.m_Exp) {
                        Id = 6,
                        Text = "Are you sure you want to use NonEmpty function and not NON EMPTY clause ? The results are different.",
                        Severity = 1
                    };
                    analyzer.Add(m);
                }
            }
            this.m_Exp.Analyze(analyzer);
        }

        internal override void AppendMDX(StringBuilder mdx, Formatter f, int indent, bool comma)
        {
            base.AppendComment(mdx, f, indent);
            this.m_Exp.AppendMDX(mdx, f, indent, comma);
            if (this.m_Having != null)
            {
                mdx.AppendFormat(" HAVING {0}", this.m_Having.GetMDX(f, indent));
            }
            if (this.m_DimProps != null)
            {
                f.AppendIndent(mdx, indent);
                mdx.Append("DIMENSION PROPERTIES ");
                string mDX = this.m_DimProps.GetMDX(f, -1);
                if (this.m_DimProps.Count > 1)
                {
                    this.m_DimProps.AppendMDX(mdx, f, f.Indent(indent));
                    f.AppendIndent(mdx, indent);
                }
                else
                {
                    mdx.AppendFormat("{0} ", mDX);
                }
            }
            string s = String.Format(" ON {0}", this.m_Name.ToUpper());
            mdx.Append(f.Keyword(s));
        }

        internal void ApplyNonEmpty(MDXAxesListNode axes)
        {
            if (this.m_Exp.GetType() == typeof(MDXNonEmptyNode))
            {
                MDXNonEmptyNode exp = this.m_Exp as MDXNonEmptyNode;
                foreach (MDXAxisNode node2 in axes)
                {
                    if (node2 != this)
                    {
                        MDXExpNode set = node2.GetSet();
                        if (set.GetType() == typeof(MDXNonEmptyNode))
                        {
                            set = (set as MDXNonEmptyNode).GetSet();
                        }
                        exp.AddFilter(set);
                    }
                }
            }
        }

        internal override void FillParseTree(MDXTreeNodeCollection Nodes)
        {
            MDXTreeNode node = Nodes.Add(string.Format("Axis({0})", this.m_Name));
            node.Tag = this.m_Exp;
            node.ImageKey = node.SelectedImageKey = "NamedSet.ico";
            this.m_Exp.FillParseTree(node.Nodes);
            if (this.m_Having != null)
            {
                MDXTreeNode node2 = node.Nodes.Add("HAVING");
                node2.Tag = this.m_Having;
                node2.ImageKey = node2.SelectedImageKey = "MeasureCalculated.ico";
                this.m_Having.FillParseTree(node2.Nodes);
            }
        }

        internal MDXExpNode GetSet()
        {
            return this.m_Exp;
        }
    }
}

