namespace SSVParseLib
{
    using System;

    public class SSLexCharacterClass
    {
        public int[] m_array;
        public int m_max;
        public int m_min;
        public int m_size;

        public SSLexCharacterClass(int[] q_array)
        {
            this.m_min = q_array[0];
            this.m_max = q_array[1];
            this.m_size = q_array[2];
            this.m_array = new int[this.m_size * 2];
            for (int i = 0; i < (this.m_size * 2); i++)
            {
                this.m_array[i] = q_array[i + 3];
            }
        }

        public SSLexCharacterClass(int q_size, int q_min, int q_max, int[] q_array)
        {
            this.m_size = q_size;
            this.m_min = q_min;
            this.m_max = q_max;
            this.m_array = q_array;
        }

        public bool translate(char[] q_char)
        {
            char ch = q_char[0];
            if ((ch >= this.m_min) && (ch <= this.m_max))
            {
                for (int i = 0; i < this.m_size; i++)
                {
                    if ((ch >= this.m_array[i * 2]) && (ch <= this.m_array[(i * 2) + 1]))
                    {
                        q_char[0] = (char) this.m_min;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}

