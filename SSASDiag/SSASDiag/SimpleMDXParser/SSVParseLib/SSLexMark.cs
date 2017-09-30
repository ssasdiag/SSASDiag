namespace SSVParseLib
{
    using System;

    public class SSLexMark
    {
        public int m_index;
        public int m_line;
        public int m_offset;

        public SSLexMark()
        {
        }

        public SSLexMark(int q_line, int q_offset, int q_index)
        {
            this.m_index = q_index;
            this.m_line = q_line;
            this.m_offset = q_offset;
        }

        public int index()
        {
            return this.m_index;
        }
    }
}

