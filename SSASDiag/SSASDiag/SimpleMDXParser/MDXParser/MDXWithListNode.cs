namespace SimpleMDXParser
{
    using System;
    using System.Text;

    public class MDXWithListNode : MDXListNode<MDXWithNode>
    {
        internal override void AppendMDX(StringBuilder mdx, Formatter f, int indent, bool comma)
        {
            foreach (MDXWithNode node in base.m_List)
            {
                node.AppendMDX(mdx, f, indent);
            }
        }
    }
}

