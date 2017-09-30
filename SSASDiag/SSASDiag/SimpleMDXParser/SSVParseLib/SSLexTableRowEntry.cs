namespace SSVParseLib
{
    using System;

    public class SSLexTableRowEntry
    {
        public int m_end;
        public int m_start;
        public int m_state;

        public SSLexTableRowEntry(int q_start, int q_end, int q_state)
        {
            this.m_end = q_end;
            this.m_start = q_start;
            this.m_state = q_state;
        }

        public int end()
        {
            return this.m_end;
        }

        public int start()
        {
            return this.m_start;
        }

        public int state()
        {
            return this.m_state;
        }
    }
}

