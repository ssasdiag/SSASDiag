namespace SimpleMDXParser
{
    using System;

    internal abstract class MDXBaseFunctionNode : MDXExpNode
    {
        internal MDXExpListNode m_Arguments;
        internal string m_Function;
        private bool m_IsBuiltin;
        internal MDXObject m_Object;

        protected MDXBaseFunctionNode(string FunctionName, MDXExpListNode ExpList, MDXObject obj, bool builtin)
        {
            this.m_Function = FunctionName;
            this.m_Arguments = ExpList;
            this.m_Object = obj;
            this.m_IsBuiltin = builtin;
        }

        internal override void Analyze(Analyzer analyzer)
        {
            try
            {
                base.StartAnalyze(analyzer);
                if (!analyzer.InCommonSubExpr)
                {
                    base.Analyze(analyzer);
                    if (this.m_Object != null)
                    {
                        if (this.m_Object.Optimization == MDXFunctionOpt.Deprecated)
                        {
                            Message m = new Message(this) {
                                Id = 0x12,
                                Text = string.Format("MDX function '{0}' is deprecated", this.m_Function),
                                Severity = 5
                            };
                            analyzer.Add(m);
                        }
                        else if (this.m_Object.Optimization == MDXFunctionOpt.Bad)
                        {
                            Message message2 = new Message(this) {
                                Id = 0x13,
                                Text = string.Format("MDX function '{0}' generaly has very bad performance and should be avoided", this.m_Function),
                                Severity = 5
                            };
                            analyzer.Add(message2);
                        }
                        else if (((this.m_Object.Optimization == MDXFunctionOpt.Normal) && (this.m_Object.ReturnType != MDXDataType.Set)) && analyzer.InRHS)
                        {
                            Message message3 = new Message(this) {
                                Id = 20,
                                Text = string.Format("MDX function '{0}' is not optimized for block computation mode", this.m_Function),
                                Severity = 3,
                                URL = "http://crawlmsdn.microsoft.com/en-us/library/bb934106(SQL.100).aspx"
                            };
                            analyzer.Add(message3);
                        }
                    }
                    if ((this.m_Function.Equals("Username", StringComparison.InvariantCultureIgnoreCase) || this.m_Function.Equals("Member_Caption", StringComparison.InvariantCultureIgnoreCase)) || this.m_Function.Equals("CustomData", StringComparison.InvariantCultureIgnoreCase))
                    {
                        Message message4 = new Message(this) {
                            Id = 0x15,
                            Text = string.Format("MDX function '{0}' prevents cache sharing between different users", this.m_Function),
                            Severity = 2
                        };
                        analyzer.Add(message4);
                    }
                    if (this.m_Function.Equals("VisualTotals", StringComparison.InvariantCultureIgnoreCase) || this.m_Function.Equals("Axis", StringComparison.InvariantCultureIgnoreCase))
                    {
                        Message message5 = new Message(this) {
                            Id = 0x16,
                            Text = string.Format("MDX function '{0}' prevents use of global formula engine cache", this.m_Function),
                            Severity = 4
                        };
                        analyzer.Add(message5);
                    }
                    if (analyzer.InLHS && this.m_Function.Equals("MemberValue", StringComparison.InvariantCultureIgnoreCase))
                    {
                        Message message6 = new Message(this) {
                            Id = 0x17,
                            Text = string.Format("MDX function '{0}' used in named set or SCOPE prevents use of global formula engine cache", this.m_Function),
                            Severity = 4
                        };
                        analyzer.Add(message6);
                    }
                    if (((this.m_Function.Equals("Aggregate", StringComparison.InvariantCultureIgnoreCase) || this.m_Function.Equals("Sum", StringComparison.InvariantCultureIgnoreCase)) || ((this.m_Function.Equals("Min", StringComparison.InvariantCultureIgnoreCase) || this.m_Function.Equals("Max", StringComparison.InvariantCultureIgnoreCase)) || this.m_Function.Equals("Avg", StringComparison.InvariantCultureIgnoreCase))) && (this.m_Arguments.Count > 0))
                    {
                        MDXExpNode n = this.m_Arguments[0];
                        if ((n.GetMDXType() != MDXDataType.Set) && (n.GetMDXType() != MDXDataType.Level))
                        {
                            if ((n.GetType() == typeof(MDXIDNode)) && !(n as MDXIDNode).GetLabel().Contains("."))
                            {
                                Message message7 = new Message(this) {
                                    Id = 0x2e,
                                    Text = string.Format("Applying aggregation funciton {0} over named set {1} - this disables block computation mode", this.m_Function, n.GetLabel()),
                                    Severity = 1,
                                    URL = "http://msdn.microsoft.com/en-us/library/bb934106.aspx"
                                };
                                analyzer.Add(message7);
                            }
                            else
                            {
                                Message message8 = new Message(n) {
                                    Id = 0x18,
                                    Text = string.Format("It looks like MDX aggregation function '{0}' is called over set with single element. Is that the intent ?", this.m_Function),
                                    Severity = 1
                                };
                                analyzer.Add(message8);
                            }
                        }
                        else
                        {
                            this.CheckSetForAggregateFunction(n, analyzer);
                        }
                    }
                    if ((analyzer.InRHS && !analyzer.IsInExisting()) && ((this.m_Function.Equals("MEMBERS", StringComparison.InvariantCultureIgnoreCase) || this.m_Function.Equals("Siblings", StringComparison.InvariantCultureIgnoreCase)) || (this.XofY("Children", ".Parent") || this.XofY("Descendants", ".Parent"))))
                    {
                        Message message9 = new Message(this) {
                            Id = 0x19,
                            Text = string.Format("This set causes the expression to evaluate to the same value over different coordinates. Consider redirecting to coordinate with non-varying attribute to take advantage of cache", new object[0]),
                            Severity = 2,
                            URL = "http://sqlblog.com/blogs/mosha/archive/2008/03/28/take-advantage-of-fe-caching-to-optimize-mdx-performance.aspx"
                        };
                        analyzer.Add(message9);
                    }
                    if (analyzer.InRHS && ((((this.m_Function.Equals("CurrentMember", StringComparison.InvariantCultureIgnoreCase) || this.m_Function.Equals("Parent", StringComparison.InvariantCultureIgnoreCase)) || (this.m_Function.Equals("PrevMember", StringComparison.InvariantCultureIgnoreCase) || this.m_Function.Equals("NextMember", StringComparison.InvariantCultureIgnoreCase))) || ((this.m_Function.Equals("Lag", StringComparison.InvariantCultureIgnoreCase) || this.m_Function.Equals("Lead", StringComparison.InvariantCultureIgnoreCase)) || (this.m_Function.Equals("FirstChild", StringComparison.InvariantCultureIgnoreCase) || this.m_Function.Equals("LastChild", StringComparison.InvariantCultureIgnoreCase)))) || (((this.m_Function.Equals("FirstSibling", StringComparison.InvariantCultureIgnoreCase) || this.m_Function.Equals("LastSibling", StringComparison.InvariantCultureIgnoreCase)) || (this.m_Function.Equals("ParallelPeriod", StringComparison.InvariantCultureIgnoreCase) || this.m_Function.Equals("Cousin", StringComparison.InvariantCultureIgnoreCase))) || (((this.m_Function.Equals("PeriodsToDate", StringComparison.InvariantCultureIgnoreCase) || this.m_Function.Equals("YTD", StringComparison.InvariantCultureIgnoreCase)) || (this.m_Function.Equals("QTD", StringComparison.InvariantCultureIgnoreCase) || this.m_Function.Equals("MTD", StringComparison.InvariantCultureIgnoreCase))) || ((this.m_Function.Equals("WTD", StringComparison.InvariantCultureIgnoreCase) || this.m_Function.Equals("Ancestor", StringComparison.InvariantCultureIgnoreCase)) || this.m_Function.Equals("Ancestors", StringComparison.InvariantCultureIgnoreCase))))))
                    {
                        Message message10 = new Message(this) {
                            Id = 0x1a,
                            Text = string.Format("MDX function '{0}' may raise an error or produce non-desired result when user applies multiselect", this.m_Function),
                            Severity = 1,
                            URL = "http://sqlblog.com/blogs/mosha/archive/2007/01/13/multiselect-friendly-mdx-for-calculations-looking-at-current-coordinate.aspx"
                        };
                        analyzer.Add(message10);
                    }
                    this.m_Arguments.Analyze(analyzer);
                }
            }
            finally
            {
                base.EndAnalyze(analyzer);
            }
        }

        public override void BuildDependencyGraph(DependencyGraph g, Vertex root)
        {
            foreach (MDXExpNode node in this.m_Arguments)
            {
                node.BuildDependencyGraph(g, root);
            }
        }

        private void CheckSetForAggregateFunction(MDXExpNode e, Analyzer analyzer)
        {
            if (e.GetMDXType() == MDXDataType.Set)
            {
                Type type = e.GetType();
                if ((type == typeof(MDXFunctionNode)) || (type == typeof(MDXPropertyNode)))
                {
                    MDXBaseFunctionNode n = e as MDXBaseFunctionNode;
                    if ((((!n.m_Function.Equals("MTD", StringComparison.InvariantCultureIgnoreCase) && !n.m_Function.Equals("QTD", StringComparison.InvariantCultureIgnoreCase)) && (!n.m_Function.Equals("YTD", StringComparison.InvariantCultureIgnoreCase) && !n.m_Function.Equals("WTD", StringComparison.InvariantCultureIgnoreCase))) && ((!n.m_Function.Equals("PeriodsToDate", StringComparison.InvariantCultureIgnoreCase) && !n.m_Function.Equals("AddCalculatedMembers", StringComparison.InvariantCultureIgnoreCase)) && (!n.m_Function.Equals("PeriodsToDate", StringComparison.InvariantCultureIgnoreCase) && !n.m_Function.Equals("Tail", StringComparison.InvariantCultureIgnoreCase)))) && (((!n.m_Function.Equals("Descendants", StringComparison.InvariantCultureIgnoreCase) && !n.m_Function.Equals("Distinct", StringComparison.InvariantCultureIgnoreCase)) && (!n.m_Function.Equals("Unorder", StringComparison.InvariantCultureIgnoreCase) && !n.m_Function.Equals("Hierarchize", StringComparison.InvariantCultureIgnoreCase))) && ((!n.m_Function.Equals("Children", StringComparison.InvariantCultureIgnoreCase) && !n.m_Function.Equals("MEMBERS", StringComparison.InvariantCultureIgnoreCase)) && (!n.m_Function.Equals("Siblings", StringComparison.InvariantCultureIgnoreCase) && !n.m_Function.Equals("ALLMEMBERS", StringComparison.InvariantCultureIgnoreCase)))))
                    {
                        if (!n.m_Function.Equals("StrToSet", StringComparison.InvariantCultureIgnoreCase))
                        {
                            if ((!n.m_Function.Equals("Except", StringComparison.InvariantCultureIgnoreCase) && !n.m_Function.Equals("Union", StringComparison.InvariantCultureIgnoreCase)) && !n.m_Function.Equals("Intersect", StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (!n.m_Function.Equals("CrossJoin", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    Message m = new Message(n) {
                                        Id = 0x1b,
                                        Severity = 5,
                                        Text = string.Format("Function '{0}' was used inside aggregation function - this disables block computation mode", n.m_Function),
                                        URL = "http://msdn.microsoft.com/en-us/library/bb934106.aspx"
                                    };
                                    analyzer.Add(m);
                                }
                                else
                                {
                                    foreach (MDXExpNode node2 in n.m_Arguments)
                                    {
                                        this.CheckSetForAggregateFunction(node2, analyzer);
                                    }
                                }
                            }
                            else
                            {
                                if (n.m_Arguments.Count > 1)
                                {
                                    this.CheckSetForAggregateFunction(n.m_Arguments[0], analyzer);
                                }
                                if (n.m_Arguments.Count > 2)
                                {
                                    this.CheckSetForAggregateFunction(n.m_Arguments[1], analyzer);
                                }
                            }
                        }
                    }
                    else if (n.m_Arguments.Count > 1)
                    {
                        this.CheckSetForAggregateFunction(n.m_Arguments[0], analyzer);
                    }
                }
                else if (type == typeof(MDXBinOpNode))
                {
                    MDXBinOpNode node3 = e as MDXBinOpNode;
                    if ((!node3.m_Op.Equals("-") && !node3.m_Op.Equals("*")) && (!node3.m_Op.Equals(":") && !node3.m_Op.Equals("+")))
                    {
                        Message message2 = new Message(node3) {
                            Id = 0x1c,
                            Severity = 5,
                            Text = string.Format("Operator '{0}' was used inside aggregation function - this disables block computation mode", node3.m_Op),
                            URL = "http://crawlmsdn.microsoft.com/en-us/library/bb934106(SQL.100).aspx"
                        };
                        analyzer.Add(message2);
                    }
                    if (!node3.Equals(":"))
                    {
                        this.CheckSetForAggregateFunction(node3.m_Exp1, analyzer);
                        this.CheckSetForAggregateFunction(node3.m_Exp2, analyzer);
                    }
                }
                else if (type == typeof(MDXUnaryOpNode))
                {
                    MDXUnaryOpNode node4 = e as MDXUnaryOpNode;
                    if (!node4.m_Op.Equals("-"))
                    {
                        Message message3 = new Message(node4) {
                            Id = 0x1d,
                            Severity = 5,
                            Text = string.Format("Operator '{0}' was used inside aggregation function - this disables block computation mode", node4.m_Op),
                            URL = "http://crawlmsdn.microsoft.com/en-us/library/bb934106(SQL.100).aspx"
                        };
                        analyzer.Add(message3);
                    }
                    this.CheckSetForAggregateFunction(node4.m_Exp, analyzer);
                }
                else if (type == typeof(MDXTupleNode))
                {
                    MDXTupleNode node5 = e as MDXTupleNode;
                    foreach (MDXExpNode node6 in node5.m_List)
                    {
                        this.CheckSetForAggregateFunction(node6, analyzer);
                    }
                }
                else if (type == typeof(MDXEnumSetNode))
                {
                    MDXEnumSetNode node7 = e as MDXEnumSetNode;
                    foreach (MDXExpNode node8 in node7.m_List)
                    {
                        this.CheckSetForAggregateFunction(node8, analyzer);
                    }
                }
            }
        }

        internal override void FillParseTree(TreeNodeCollection Nodes)
        {
            TreeNode node = this.AddParseNode(Nodes);
            foreach (object obj2 in this.m_Arguments)
            {
                ((MDXNode) obj2).FillParseTree(node.Nodes);
            }
        }

        public override string GetLabel()
        {
            return this.m_Function;
        }

        internal override bool IsCacheTrivial()
        {
            return ((this.m_Arguments.Count == 0) || ((base.GetType() == typeof(MDXPropertyNode)) && MDXExpNode.IsCachePrimitive(this.m_Arguments[0])));
        }

        internal override void SetOuterIterator(MDXNode iter)
        {
            base.m_OuterIterator = iter;
            foreach (object obj2 in this.m_Arguments)
            {
                ((MDXExpNode) obj2).SetOuterIterator(iter);
            }
        }

        protected bool XofY(string x, string y)
        {
            Type type = y.StartsWith(".") ? typeof(MDXPropertyNode) : typeof(MDXFunctionNode);
            if (y.StartsWith("."))
            {
                y = y.Replace(".", "");
            }
            if (this.m_Function.Equals(x, StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (MDXExpNode node in this.m_Arguments)
                {
                    if ((node.GetType() == type) && (node as MDXBaseFunctionNode).m_Function.Equals(y))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal bool IsBuiltin
        {
            get
            {
                return this.m_IsBuiltin;
            }
        }
    }
}

