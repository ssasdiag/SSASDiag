namespace SimpleMDXParser
{
    using SSVParseLib;
    using System;

    internal class MDXStackElem : SSYaccStackElement
    {
        private MDXNode m_Node;

        public MDXNode GetNode()
        {
            return this.m_Node;
        }

        public void SetNode(MDXNode Node)
        {
            this.m_Node = Node;
        }
    }
}

