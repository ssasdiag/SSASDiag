namespace SSVParseLib
{
    using System;

    public class SSYaccTableProd
    {
        public int m_leftside;
        public int m_size;

        public SSYaccTableProd(int q_size, int q_leftside)
        {
            this.m_size = q_size;
            this.m_leftside = q_leftside;
        }

        public int leftside()
        {
            return this.m_leftside;
        }

        public int size()
        {
            return this.m_size;
        }
    }
}

