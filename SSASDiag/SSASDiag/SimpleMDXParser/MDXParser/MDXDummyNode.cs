namespace SimpleMDXParser
{
    using System;
    using System.Text;

    internal class MDXDummyNode : MDXNode
    {
        internal override void AppendMDX(StringBuilder mdx, Formatter f, int indent, bool comma)
        {
            throw new Exception("internal error: unrecognized node");
        }

        internal override void FillParseTree(MDXTreeNodeCollection Nodes)
        {
            throw new Exception("internal error: unrecognized node");
        }
    }
}

