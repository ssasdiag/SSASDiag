namespace SimpleMDXParser
{
    using System;
    using System.Text;

    internal class MDXBinOpNode : MDXExpNode
    {
        internal MDXExpNode m_Exp1;
        internal MDXExpNode m_Exp2;
        internal string m_Op;
        private string m_UpstreamOp;

        internal MDXBinOpNode(string LexString, MDXExpNode exp1, MDXExpNode exp2)
        {
            this.m_Op = LexString.ToUpper();
            this.m_Exp1 = exp1;
            this.m_Exp2 = exp2;
            if (this.m_Exp1.GetType() == typeof(MDXBinOpNode))
            {
                (this.m_Exp1 as MDXBinOpNode).SetUpstreamOp(this.m_Op);
            }
            if (this.m_Exp2.GetType() == typeof(MDXBinOpNode))
            {
                (this.m_Exp2 as MDXBinOpNode).SetUpstreamOp(this.m_Op);
            }
        }

        internal override void Analyze(Analyzer analyzer)
        {
            base.Analyze(analyzer);
            try
            {
                base.StartAnalyze(analyzer);
                if (!analyzer.InCommonSubExpr)
                {
                    this.m_Exp1.Analyze(analyzer);
                    if (this.m_Op.Equals("=") || this.m_Op.Equals("<>"))
                    {
                        if (IsNameFunction(this.m_Exp1) || IsNameFunction(this.m_Exp2))
                        {
                            Message m = new Message(this) {
                                Id = 12,
                                Text = "Use IS operator to compare objects instead of comparing them by name",
                                URL = "http://sqlblog.com/blogs/mosha/archive/2004/11/04/comparing-members-in-mdx.aspx",
                                Severity = 3
                            };
                            analyzer.Add(m);
                        }
                        if (!this.m_Exp1.IsScalar() && !this.m_Exp2.IsScalar())
                        {
                            Message message2 = new Message(this) {
                                Id = 13,
                                Text = string.Format("Operator '{0}' compares cell values. Consider comparing objects instead with IS operator", this.m_Op),
                                Severity = 4
                            };
                            analyzer.Add(message2);
                        }
                        if (this.m_Exp1.GetLabel().Equals("NULL", StringComparison.InvariantCultureIgnoreCase) || this.m_Exp2.GetLabel().Equals("NULL", StringComparison.InvariantCultureIgnoreCase))
                        {
                            Message message3 = new Message(this) {
                                Id = 14,
                                Text = "If you want to avoid division by zero - use 'value = 0' check, if you want to check whether cell is empty - use IsEmpty function, if you want to check that MDX member or tuple exists - use 'obj IS NULL' check",
                                URL = "http://sqlblog.com/blogs/mosha/archive/2005/06/30/how-to-check-if-cell-is-empty-in-mdx.aspx",
                                Severity = 3
                            };
                            analyzer.Add(message3);
                        }
                    }
                    if (this.m_Op.Equals("IS", StringComparison.InvariantCultureIgnoreCase) && (this.m_Exp1.GetLabel().Equals("NULL", StringComparison.InvariantCultureIgnoreCase) || this.m_Exp2.GetLabel().Equals("NULL", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        Message message4 = new Message(this) {
                            Id = 15,
                            Text = "If you want to avoid division by zero - use 'value = 0' check, if you want to check whether cell is empty - use IsEmpty function, if you want to check that MDX member or tuple exists - use 'obj IS NULL' check",
                            URL = "http://sqlblog.com/blogs/mosha/archive/2005/06/30/how-to-check-if-cell-is-empty-in-mdx.aspx",
                            Severity = 3
                        };
                        analyzer.Add(message4);
                    }
                    if (this.m_Op.Equals("*") && (this.m_Exp1.GetLabel().Equals("100") || this.m_Exp2.GetLabel().Equals("100")))
                    {
                        Message message5 = new Message(this) {
                            Id = 0x10,
                            Text = "Are you trying to convert to percentage by multiplying by 100 ? If yes, consider using FORMAT_STRING='Percent' instead",
                            Severity = 1
                        };
                        analyzer.Add(message5);
                    }
                    if (this.IsCompareOp() && ((IsFunction(this.m_Exp1, "Rank") && IsConstant(this.m_Exp2, 0)) || (IsFunction(this.m_Exp2, "Rank") && IsConstant(this.m_Exp1, 0))))
                    {
                        Message message6 = new Message(this) {
                            Id = 0x2c,
                            Text = "Check inclusion of the element in the set can be done more efficiently with Intersect rather than with Rank",
                            Severity = 1
                        };
                        analyzer.Add(message6);
                    }
                    this.m_Exp2.Analyze(analyzer);
                }
            }
            finally
            {
                base.EndAnalyze(analyzer);
            }
        }

        internal override void AppendMDX(StringBuilder mdx, Formatter f, int indent, bool comma)
        {
            bool flag = false;
            if (this.m_UpstreamOp != null)
            {
                int precedence = GetPrecedence(this.m_UpstreamOp);
                int num2 = GetPrecedence(this.m_Op);
                if ((precedence < 0) || (num2 < 0))
                {
                    flag = true;
                }
                else
                {
                    flag = precedence > num2;
                }
            }
            if (flag || comma)
            {
                f.AppendIndent(mdx, indent, comma);
            }
            MDXExpListNode exps = new MDXExpListNode();
            this.FlattenCJ(exps);
            if (exps.Count > 2)
            {
                bool flag2 = true;
                foreach (MDXExpNode node2 in exps)
                {
                    if (!flag2)
                    {
                        mdx.Append("*");
                    }
                    node2.AppendMDX(mdx, f, f.Indent(indent));
                    flag2 = false;
                }
            }
            else
            {
                string str = (this.m_Op.Length > 2) ? f.Keyword(this.m_Op) : this.m_Op;
                string mDX = this.m_Exp1.GetMDX(f, -1);
                string str3 = this.m_Exp2.GetMDX(f, -1);
                if ((indent >= 0) && ((mDX.Length + str3.Length) > 70))
                {
                    if (flag)
                    {
                        mdx.Append("(");
                    }
                    this.m_Exp1.AppendMDX(mdx, f, f.Indent(indent));
                    base.AppendCommentAndIndent(mdx, f, indent, false);
                    mdx.AppendFormat("{0} ", str);
                    this.m_Exp2.AppendMDX(mdx, f, f.Indent(indent));
                    if (flag)
                    {
                        f.AppendIndent(mdx, indent);
                        mdx.Append(")");
                    }
                }
                else
                {
                    base.AppendCommentAndIndent(mdx, f, indent, false);
                    if (flag)
                    {
                        mdx.AppendFormat("({0} {1} {2})", mDX, str, str3);
                    }
                    else
                    {
                        mdx.AppendFormat("{0} {1} {2}", mDX, str, str3);
                    }
                }
            }
        }

        public override void BuildDependencyGraph(DependencyGraph g, Vertex root)
        {
            this.m_Exp1.BuildDependencyGraph(g, root);
            this.m_Exp2.BuildDependencyGraph(g, root);
        }

        internal override void FillParseTree(MDXTreeNodeCollection Nodes)
        {
            MDXTreeNode node = this.AddParseNode(Nodes);
            this.m_Exp1.FillParseTree(node.Nodes);
            this.m_Exp2.FillParseTree(node.Nodes);
        }

        internal void FlattenCJ(MDXExpListNode exps)
        {
            if (!this.m_Op.Equals("*"))
            {
                exps.Add(this);
            }
            else
            {
                if (this.m_Exp1.GetType() != typeof(MDXBinOpNode))
                {
                    exps.Add(this.m_Exp1);
                }
                else
                {
                    (this.m_Exp1 as MDXBinOpNode).FlattenCJ(exps);
                }
                if (this.m_Exp2.GetType() != typeof(MDXBinOpNode))
                {
                    exps.Add(this.m_Exp2);
                }
                else
                {
                    (this.m_Exp2 as MDXBinOpNode).FlattenCJ(exps);
                }
            }
        }

        public override string GetLabel()
        {
            return this.m_Op;
        }

        public override MDXDataType GetMDXType()
        {
            if (!this.m_Op.Equals(":"))
            {
                if ((this.m_Exp1.GetMDXType() == MDXDataType.Set) || (this.m_Exp2.GetMDXType() == MDXDataType.Set))
                {
                    return MDXDataType.Set;
                }
                if (!this.m_Op.Equals("*") || ((this.m_Exp1.GetMDXType() != MDXDataType.Level) && (this.m_Exp2.GetMDXType() != MDXDataType.Level)))
                {
                    return MDXDataType.Number;
                }
            }
            return MDXDataType.Set;
        }

        public override string GetNormalizedMDX()
        {
            string upstreamOp = this.m_UpstreamOp;
            string normalizedMDX = null;
            try
            {
                this.m_UpstreamOp = null;
                normalizedMDX = base.GetNormalizedMDX();
            }
            finally
            {
                this.m_UpstreamOp = upstreamOp;
            }
            return normalizedMDX;
        }

        private static int GetPrecedence(string op)
        {
            switch (op.ToUpper())
            {
                case ",":
                    return 1;

                case "OR":
                case "XOR":
                    return 3;

                case "AND":
                    return 4;

                case "NOT":
                    return 5;

                case "<":
                case ">":
                case "<=":
                case ">=":
                case "=":
                case "<>":
                    return 6;

                case "EXISTING":
                case "DISTINCT":
                    return 7;

                case "+":
                case "-":
                    return 8;

                case "*":
                case "/":
                    return 9;

                case "^":
                    return 10;

                case "AS":
                    return 13;

                case "IS":
                    return 14;

                case ".":
                    return 15;
            }
            return -1;
        }

        internal override bool IsCacheTrivial()
        {
            return (MDXExpNode.IsCachePrimitive(this.m_Exp1) && MDXExpNode.IsCachePrimitive(this.m_Exp2));
        }

        private bool IsCompareOp()
        {
            if (((!this.m_Op.Equals("=") && !this.m_Op.Equals("<>")) && (!this.m_Op.Equals(">") && !this.m_Op.Equals("<"))) && !this.m_Op.Equals("<="))
            {
                return this.m_Op.Equals(">=");
            }
            return true;
        }

        private static bool IsConstant(MDXExpNode e, int c)
        {
            if (e.GetType() == typeof(MDXIntegerConstNode))
            {
                MDXIntegerConstNode node = e as MDXIntegerConstNode;
                if (node.m_Int == c)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsFunction(MDXExpNode e, string fname)
        {
            if (e.GetType() == typeof(MDXFunctionNode))
            {
                MDXFunctionNode node = e as MDXFunctionNode;
                if (node.m_Function.Equals(fname, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsNameFunction(MDXExpNode e)
        {
            if (e.GetType() == typeof(MDXPropertyNode))
            {
                MDXPropertyNode node = e as MDXPropertyNode;
                if ((node.m_Function.Equals("Name", StringComparison.InvariantCultureIgnoreCase) || node.m_Function.Equals("UniqueName", StringComparison.InvariantCultureIgnoreCase)) || node.m_Function.Equals("Member_Caption", StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        internal override void SetOuterIterator(MDXNode iter)
        {
            base.m_OuterIterator = iter;
            this.m_Exp1.SetOuterIterator(iter);
            this.m_Exp2.SetOuterIterator(iter);
        }

        internal void SetUpstreamOp(string upstreamop)
        {
            this.m_UpstreamOp = upstreamop;
        }
    }
}

