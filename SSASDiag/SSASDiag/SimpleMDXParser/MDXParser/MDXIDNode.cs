namespace SimpleMDXParser
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class MDXIDNode : MDXExpNode
    {
        private List<string> m_Components;
        private string m_ID;
        private MDXDataType m_MDXType;

        internal MDXIDNode(string LexString)
        {
            this.m_ID = LexString;
            this.m_Components = new List<string>();
            this.m_Components.Add(LexString);
            this.m_MDXType = MDXDataType.Member;
        }

        internal override void Analyze(Analyzer analyzer)
        {
            base.Analyze(analyzer);
            if (this.m_Components.Count > 1)
            {
                foreach (string str in this.m_Components)
                {
                    if ((!str.StartsWith("[") && !str.StartsWith("&")) && !str.Equals("Measures", StringComparison.InvariantCultureIgnoreCase))
                    {
                        Message m = new Message(this) {
                            Id = 11,
                            Text = string.Format("Looks like object '{0}' is referenced not by its unique name. Use of unique names in MDX is recommended", this.GetLabel()),
                            Severity = 1
                        };
                        analyzer.Add(m);
                        break;
                    }
                }
            }
        }

        internal override void AppendMDX(StringBuilder mdx, Formatter f, int indent, bool comma)
        {
            if (comma || !this.m_ID.Equals("NULL", StringComparison.CurrentCultureIgnoreCase))
            {
                base.AppendCommentAndIndent(mdx, f, indent, comma);
                mdx.Append(this.m_ID);
            }
            else
            {
                mdx.Append(f.Keyword("NULL"));
            }
        }

        internal void AppendName(string term)
        {
            this.m_Components.Add(term);
            this.m_ID = this.m_ID + "." + term;
        }

        public override void BuildDependencyGraph(DependencyGraph g, Vertex root)
        {
            if (!this.GetLabel().Equals("NULL", StringComparison.InvariantCultureIgnoreCase))
            {
                string label = this.GetLabel();
                Vertex to = g.AddVertex(this, label);
                base.GraphVertex = to;
                g.AddEdge(root, to);
            }
        }

        internal string GetAllButLastComponents()
        {
            string str = "";
            for (int i = 0; i < (this.m_Components.Count - 1); i++)
            {
                if (!this.m_Components[i].Equals("currentcube", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (str.Length > 0)
                    {
                        str = str + ".";
                    }
                    str = str + this.m_Components[i];
                }
            }
            return str;
        }

        public override string GetLabel()
        {
            return this.m_ID;
        }

        internal string GetLastComponent()
        {
            if (this.m_Components.Count == 0)
            {
                return "";
            }
            return this.m_Components[this.m_Components.Count - 1];
        }

        public override MDXDataType GetMDXType()
        {
            return this.m_MDXType;
        }

        internal override bool IsCacheTrivial()
        {
            return true;
        }

        internal void SetMDXType(MDXDataType dt)
        {
            this.m_MDXType = dt;
        }
    }
}

