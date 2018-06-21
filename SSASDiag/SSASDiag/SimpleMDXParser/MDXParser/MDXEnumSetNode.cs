namespace SimpleMDXParser
{
    using System;
    using System.Text;

    internal class MDXEnumSetNode : MDXExpNode
    {
        internal MDXExpListNode m_List;

        internal MDXEnumSetNode(MDXExpListNode list)
        {
            this.m_List = list;
        }

        internal override void Analyze(Analyzer analyzer)
        {
            base.Analyze(analyzer);
            this.m_List.Analyze(analyzer);
        }

        internal override void AppendMDX(StringBuilder mdx, Formatter f, int indent, bool comma)
        {
            base.AppendCommentAndIndent(mdx, f, indent, comma);
            int num = f.Indent(indent);
            if (this.m_List.Count == 1)
            {
                string mDX = this.m_List[0].GetMDX(f, -1);
                if (mDX.Length < 60)
                {
                    mdx.AppendFormat("{0}{1}{2}", f.OpenBrace(), mDX, f.CloseBrace());
                    return;
                }
            }
            mdx.Append(f.OpenBrace());
            bool flag = true;
            if (this.m_List != null)
            {
                foreach (object obj2 in this.m_List)
                {
                    f.AppendCommaAtTheEndOfLine(mdx, !flag);
                    ((MDXNode) obj2).AppendMDX(mdx, f, num, !flag);
                    flag = false;
                }
            }
            if (this.m_List.Count != 0)
            {
                f.AppendIndent(mdx, indent);
            }
            mdx.Append(f.CloseBrace());
        }

        internal override void FillParseTree(MDXTreeNodeCollection Nodes)
        {
            MDXTreeNode node = this.AddParseNode(Nodes);
            if (this.m_List != null)
            {
                foreach (object obj2 in this.m_List)
                {
                    ((MDXNode) obj2).FillParseTree(node.Nodes);
                }
            }
        }

        public override string GetLabel()
        {
            return "{...}";
        }

        public override MDXDataType GetMDXType()
        {
            return MDXDataType.Set;
        }
    }
}

