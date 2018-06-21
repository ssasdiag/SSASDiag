namespace SimpleMDXParser
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public abstract class MDXNode
    {
        internal static int IndentWidth = 2;
        private List<string> m_Comments;
        private Locator m_Locator = new Locator();
        private Source m_Source;
        private Vertex m_Vertex;

        internal MDXNode()
        {
        }

        internal virtual void Analyze(Analyzer analyzer)
        {
        }

        internal void AppendComment(StringBuilder sb, Formatter f, int indent)
        {
            if (this.m_Comments != null)
            {
                bool flag = true;
                foreach (string str in this.m_Comments)
                {
                    if (flag)
                    {
                        f.AppendIndent(sb, indent);
                    }
                    sb.Append(f.Comment(str));
                    if (str.Equals("/*"))
                    {
                        flag = false;
                    }
                    if (str.Equals("*/"))
                    {
                        flag = true;
                    }
                }
                if (indent < 0)
                {
                    sb.Append(f.Newline());
                }
            }
        }

        internal void AppendCommentAndIndent(StringBuilder sb, Formatter f, int indent, bool comma)
        {
            this.AppendComment(sb, f, indent);
            f.AppendIndent(sb, indent, comma);
        }

        internal void AppendMDX(StringBuilder mdx, Formatter f, int indent)
        {
            this.AppendMDX(mdx, f, indent, false);
        }

        internal abstract void AppendMDX(StringBuilder mdx, Formatter f, int indent, bool comma);
        public virtual void BuildDependencyGraph(DependencyGraph g, Vertex root)
        {
        }

        internal void CheckType(Type type)
        {
            if (base.GetType() != type)
            {
                throw new Exception("Unrecognized MDX property");
            }
        }

        internal abstract void FillParseTree(MDXTreeNodeCollection Nodes);
        internal virtual MDXConstruct GetConstruct()
        {
            return MDXConstruct.Unknown;
        }

        public virtual string GetDebugMDX()
        {
            return this.GetMDX(-1);
        }

        public virtual MDXExpNode GetExpNode()
        {
            return null;
        }

        public virtual string GetFormatMDX()
        {
            return this.GetMDX(0);
        }

        public override int GetHashCode()
        {
            return this.GetDebugMDX().GetHashCode();
        }

        internal string GetMDX(int indent)
        {
            Formatter f = new Formatter();
            return this.GetMDX(f, indent);
        }

        internal string GetMDX(Formatter f, int indent)
        {
            StringBuilder mdx = new StringBuilder();
            this.AppendMDX(mdx, f, indent);
            return mdx.ToString();
        }

        public virtual string GetNormalizedMDX()
        {
            return this.GetMDX(-1);
        }

        internal static bool IsConstantNode(Type t)
        {
            if (((t != typeof(MDXIntegerConstNode)) && (t != typeof(MDXFloatConstNode))) && (t != typeof(MDXStringConstNode)))
            {
                return false;
            }
            return true;
        }

        internal List<string> Comments
        {
            get
            {
                return this.m_Comments;
            }
            set
            {
                this.m_Comments = value;
            }
        }

        public Vertex GraphVertex
        {
            get
            {
                return this.m_Vertex;
            }
            set
            {
                this.m_Vertex = value;
            }
        }

        public Locator Locator
        {
            get
            {
                return this.m_Locator;
            }
        }

        public Source Source
        {
            get
            {
                return this.m_Source;
            }
            set
            {
                this.m_Source = value;
            }
        }
    }
}

