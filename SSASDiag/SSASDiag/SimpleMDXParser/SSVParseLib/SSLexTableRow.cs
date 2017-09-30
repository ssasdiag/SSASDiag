namespace SSVParseLib
{
    using System;

    public class SSLexTableRow
    {
        public SSLexTableRowEntry[] m_entries;
        public int m_size;

        public SSLexTableRow(int[] q_row, int q_index)
        {
            this.m_size = q_row[q_index];
            if (this.m_size > 0)
            {
                this.m_entries = new SSLexTableRowEntry[this.m_size];
            }
            q_index++;
            for (int i = 0; i < this.m_size; i++)
            {
                this.m_entries[i] = new SSLexTableRowEntry(q_row[q_index], q_row[q_index + 1], q_row[q_index + 2]);
                q_index += 3;
            }
        }

        public int lookup(int q_code)
        {
            for (int i = 0; i < this.m_size; i++)
            {
                SSLexTableRowEntry entry = this.m_entries[i];
                if (q_code < entry.start())
                {
                    return -1;
                }
                if (q_code <= entry.end())
                {
                    return entry.state();
                }
            }
            return -1;
        }
    }
}

