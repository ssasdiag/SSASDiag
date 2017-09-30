namespace SSVParseLib
{
    using System;

    public class SSYaccTableHeader
    {
        public int numRows;
        public int prodOffset;
        public int prodOrLar;
        public int rowOffset;
        public int type;

        public int numLars()
        {
            long num = (this.prodOrLar >> 0x10) & 0xffff;
            return (int) num;
        }

        public int numProds()
        {
            return (this.prodOrLar & 0xffff);
        }
    }
}

