namespace SimpleMDXParser
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text;

    public class MDXListNode<T> : MDXNode, IEnumerable<T>, IEnumerable where T: MDXNode
    {
        protected List<T> m_List;

        internal MDXListNode()
        {
            this.m_List = new List<T>();
        }

        internal void Add(T t)
        {
            this.m_List.Add(t);
        }

        internal override void Analyze(Analyzer analyzer)
        {
            base.Analyze(analyzer);
            foreach (T local in this.m_List)
            {
                local.Analyze(analyzer);
            }
        }

        internal override void AppendMDX(StringBuilder mdx, Formatter f, int indent, bool comma)
        {
            base.AppendComment(mdx, f, indent);
            bool flag = true;
            foreach (T local in this.m_List)
            {
                f.AppendCommaAtTheEndOfLine(mdx, !flag);
                local.AppendMDX(mdx, f, indent, comma || !flag);
                flag = false;
            }
        }

        internal override void FillParseTree(TreeNodeCollection Nodes)
        {
            foreach (T local in this.m_List)
            {
                local.FillParseTree(Nodes);
            }
        }

        internal T Get(int i)
        {
            return this.m_List[i];
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.m_List.GetEnumerator();
        }

        internal void Insert(int index, T t)
        {
            this.m_List.Insert(index, t);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.m_List.GetEnumerator();
        }

        internal int Count
        {
            get
            {
                return this.m_List.Count;
            }
        }

        internal T this[int Index]
        {
            get
            {
                return this.m_List[Index];
            }
        }
    }
}

