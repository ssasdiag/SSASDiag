namespace SSVParseLib
{
    using System;

    public class SSLexSubtable
    {
        public SSLexFinalState[] m_final;
        public SSLexTableRow[] m_rows;
        public int m_size;
        public const int SSLexStateInvalid = -1;

        public SSLexSubtable(int q_numRows, int[] q_rows, int[] q_final)
        {
            this.m_size = q_numRows;
            int num = 0;
            int num2 = 0;
            this.m_rows = new SSLexTableRow[this.m_size];
            this.m_final = new SSLexFinalState[this.m_size];
            for (int i = 0; i < this.m_size; i++)
            {
                this.m_rows[i] = new SSLexTableRow(q_rows, num);
                this.m_final[i] = new SSLexFinalState(q_final, num2);
                num2 += 3;
                num += (q_rows[num] * 3) + 1;
            }
        }

        public int lookup(int q_state, int q_next)
        {
            return this.m_rows[q_state].lookup(q_next);
        }

        public SSLexFinalState lookupFinal(int q_state)
        {
            return this.m_final[q_state];
        }
    }
}

