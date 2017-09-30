namespace SimpleMDXParser
{
    using System;

    internal class CalcObject
    {
        private MDXNode m_Node;

        internal MDXNode Node
        {
            get
            {
                return this.m_Node;
            }
            set
            {
                this.m_Node = value;
            }
        }
    }
}

