namespace SimpleMDXParser
{
    using System;
    using System.Text;

    internal class MDXAssignmentNode : MDXStatementNode
    {
        private MDXExpNode m_LValue;
        private string m_Prop;
        private MDXExpNode m_RValue;

        internal MDXAssignmentNode(string prop, MDXExpNode lvalue, MDXExpNode rvalue)
        {
            this.m_Prop = prop;
            this.m_LValue = lvalue;
            this.m_RValue = rvalue;
        }

        internal override void Analyze(Analyzer analyzer)
        {
            base.Analyze(analyzer);
            if (this.m_Prop != null)
            {
                if (this.m_Prop.Equals("NON_EMPTY_BEHAVIOR", StringComparison.InvariantCultureIgnoreCase))
                {
                    Message m = new Message(this) {
                        Id = 7,
                        Text = string.Format("AS2008 only: Consider removing NON_EMPTY_BEHAVIOR property", new object[0]),
                        Severity = 2
                    };
                    analyzer.Add(m);
                }
                else
                {
                    Type t = this.m_RValue.GetType();
                    if (!MDXNode.IsConstantNode(t) && (t != typeof(MDXIDNode)))
                    {
                        Message message2 = new Message(this) {
                            Id = 8,
                            Text = string.Format("Calculation property '{0}' is not a constant or member reference. Therefore it will not benefit from neiter block computation mode, nor caching", this.m_Prop),
                            Severity = 4
                        };
                        analyzer.Add(message2);
                    }
                }
            }
            try
            {
                analyzer.InLHS = true;
                this.m_LValue.Analyze(analyzer);
            }
            finally
            {
                analyzer.InLHS = false;
            }
            try
            {
                analyzer.InRHS = true;
                this.m_RValue.Analyze(analyzer);
            }
            finally
            {
                analyzer.InRHS = false;
            }
            if ((this.m_Prop == null) && (this.m_RValue.GetMDXType() == MDXDataType.String))
            {
                Message message3 = new Message(this.m_RValue) {
                    Id = 9,
                    Text = string.Format("RHS of the assignment evaluates to a string. Strings are difficult to cache - consider using FORMAT_STRING property for value formatting", new object[0]),
                    Severity = 2
                };
                analyzer.Add(message3);
            }
        }

        internal override void AppendMDX(StringBuilder mdx, Formatter f, int indent, bool comma)
        {
            base.AppendComment(mdx, f, indent);
            int num = indent;
            if ((this.m_Prop != null) && this.m_Prop.Equals("LANGUAGE", StringComparison.CurrentCultureIgnoreCase))
            {
                num = -1;
            }
            if (this.m_Prop != null)
            {
                f.AppendIndent(mdx, indent, comma);
                mdx.Append(this.m_Prop);
                f.AppendIndent(mdx, num);
                mdx.Append("(");
            }
            this.m_LValue.AppendMDX(mdx, f, num);
            if (this.m_Prop != null)
            {
                f.AppendIndent(mdx, num);
                mdx.Append(")");
            }
            mdx.Append(" = ");
            this.m_RValue.AppendMDX(mdx, f, f.Indent(num));
        }

        internal override void FillParseTree(TreeNodeCollection Nodes)
        {
            TreeNode node = Nodes.Add("=");
            node.Tag = this;
            node.ImageKey = node.SelectedImageKey = "Script.ico";
            string label = "Value";
            if (this.m_Prop != null)
            {
                label = this.m_Prop;
            }
            TreeNode node2 = node.Nodes.Add(label);
            node2.Tag = this.m_LValue;
            node2.ImageKey = node2.SelectedImageKey = "CellCalculation.ico";
            this.m_LValue.FillParseTree(node2.Nodes);
            this.m_RValue.FillParseTree(node.Nodes);
        }
    }
}

