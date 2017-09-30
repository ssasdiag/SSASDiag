namespace SimpleMDXParser
{
    using System;

    public abstract class MDXWithNode : MDXNode
    {
        private MDXListNode<MDXCalcPropNode> m_CalcProps;
        private MDXExpNode m_Exp;
        private bool m_ExpressionUsedSingleQuotes;
        private MDXIDNode m_Name;
        protected bool m_NewSyntax;

        internal MDXWithNode(MDXExpNode name, MDXExpNode exp, MDXListNode<MDXCalcPropNode> calcprops, bool newsyntax)
        {
            if (name.GetType() != typeof(MDXIDNode))
            {
                throw new Exception("Syntax error: Cannot use '" + name.GetMDX(-1) + "' in the WITH clause");
            }
            this.m_Name = name as MDXIDNode;
            this.m_Exp = exp;
            this.m_CalcProps = calcprops;
            this.m_NewSyntax = newsyntax;
        }

        internal override void Analyze(Analyzer analyzer)
        {
            base.Analyze(analyzer);
            if (this.m_ExpressionUsedSingleQuotes)
            {
                Message m = new Message(this.m_Exp) {
                    Id = 1,
                    Text = string.Format("Don't use single quotes for definitions of calculation {0}", this.m_Name.GetLabel()),
                    Severity = 0,
                    URL = "http://sqlblog.com/blogs/mosha/archive/2005/04/02/to-quote-or-not-to-quote-in-expressions-of-mdx-calculations.aspx"
                };
                analyzer.Add(m);
            }
            if (this.m_CalcProps != null)
            {
                foreach (MDXCalcPropNode node in this.m_CalcProps)
                {
                    if (node.PropertyName.Equals("NON_EMPTY_BEHAVIOR", StringComparison.InvariantCultureIgnoreCase))
                    {
                        Message message2 = new Message(node) {
                            Id = 2,
                            Text = string.Format("AS2008 only: Consider removing NON_EMPTY_BEHAVIOR property for '{0}'", this.m_Name.GetLabel()),
                            Severity = 2
                        };
                        analyzer.Add(message2);
                    }
                    else
                    {
                        Type t = node.PropertyExp.GetType();
                        if ((!node.PropertyName.Equals("SOLVE_ORDER", StringComparison.InvariantCultureIgnoreCase) && !MDXNode.IsConstantNode(t)) && (t != typeof(MDXIDNode)))
                        {
                            Message message3 = new Message(node) {
                                Id = 3,
                                Text = string.Format("Calculation property '{0}' of '{1}' is not a constant or member reference. Therefore it will not benefit from neiter block computation mode, nor caching", node.PropertyName, this.m_Name.GetLabel()),
                                Severity = 4
                            };
                            analyzer.Add(message3);
                        }
                    }
                    if (node.PropertyName.Equals("VISIBLE", StringComparison.InvariantCultureIgnoreCase))
                    {
                        Message message4 = new Message(node) {
                            Id = 4,
                            Severity = 0,
                            URL = "http://technet.microsoft.com/en-us/library/ms144787(SQL.100).aspx"
                        };
                        if (node.PropertyExp.GetLabel().Equals("0") || node.PropertyExp.GetLabel().Equals("false", StringComparison.InvariantCultureIgnoreCase))
                        {
                            message4.Text = "Consider using 'CREATE HIDDEN ...' syntax instead of ',VISIBLE=0'";
                        }
                        else
                        {
                            message4.Text = string.Format("'VISIBLE={0}' is redundant, since {1} is visible by default", node.PropertyExp.GetMDX(-1), this.m_Name.GetLabel());
                        }
                        analyzer.Add(message4);
                    }
                    node.PropertyExp.Analyze(analyzer);
                }
            }
        }

        public override void BuildDependencyGraph(DependencyGraph g, Vertex root)
        {
            Vertex vertex = g.AddVertex(this.Name, this.Name.GetLabel());
            base.GraphVertex = vertex;
            this.m_Exp.BuildDependencyGraph(g, vertex);
        }

        internal string GetCMName()
        {
            string allButLastComponents = this.m_Name.GetAllButLastComponents();
            string lastComponent = this.m_Name.GetLastComponent();
            if ((!(allButLastComponents == "") && !allButLastComponents.Equals("measures", StringComparison.InvariantCultureIgnoreCase)) && !allButLastComponents.Equals("[measures]", StringComparison.InvariantCultureIgnoreCase))
            {
                return string.Format("{0}.{1}", allButLastComponents, lastComponent);
            }
            return lastComponent;
        }

        public string GetJustName()
        {
            return this.m_Name.GetLastComponent();
        }

        public abstract string GetMDXWithCube(string cube);
        public string GetParent()
        {
            string allButLastComponents = this.m_Name.GetAllButLastComponents();
            if (allButLastComponents == "")
            {
                return "[Measures]";
            }
            return allButLastComponents;
        }

        internal void HandleSingleQuotes()
        {
            this.m_ExpressionUsedSingleQuotes = false;
            if (this.m_Exp.GetType() == typeof(MDXStringConstNode))
            {
                MDXStringConstNode exp = (MDXStringConstNode) this.m_Exp;
                if (exp.SingleQuote)
                {
                    string mdx = exp.GetString().Replace("''", "'");
                    Source src = exp.Source.Clone();
                    src.StartLocation = exp.Locator;
                    Locator startLocation = src.StartLocation;
                    startLocation.Position++;
                    Locator locator2 = src.StartLocation;
                    locator2.Column++;
                    MDXParser parser = new MDXParser(mdx, src, null);
                    parser.ParseExpression();
                    this.m_Exp = parser.GetNode().GetExpNode();
                    if (this.m_Exp == null)
                    {
                        throw new Exception(string.Format("Error parsing MDX: {0}: MDX expression expected", exp.GetString()));
                    }
                    this.m_Exp.Locator.Adjust(src.StartLocation);
                    this.m_ExpressionUsedSingleQuotes = true;
                }
            }
        }

        protected MDXListNode<MDXCalcPropNode> CalcProps
        {
            get
            {
                return this.m_CalcProps;
            }
        }

        protected MDXExpNode Exp
        {
            get
            {
                return this.m_Exp;
            }
        }

        protected MDXIDNode Name
        {
            get
            {
                return this.m_Name;
            }
        }
    }
}

