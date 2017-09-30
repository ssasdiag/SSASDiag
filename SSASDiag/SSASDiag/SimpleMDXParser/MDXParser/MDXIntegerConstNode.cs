namespace SimpleMDXParser
{
    using System;

    internal class MDXIntegerConstNode : MDXConstNode
    {
        internal int m_Int;

        internal MDXIntegerConstNode(string LexString)
        {
            this.m_Int = Convert.ToInt32(LexString);
        }

        public override string GetLabel()
        {
            return this.m_Int.ToString();
        }
    }
}

