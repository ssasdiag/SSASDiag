namespace SSVParseLib
{
    using System;

    public class SSLexKeyTable
    {
        public int[] m_index;
        public string[] m_keys;

        public SSLexKeyTable(int[] q_index, string[] q_keys)
        {
            this.m_index = q_index;
            this.m_keys = q_keys;
        }
    }
}

