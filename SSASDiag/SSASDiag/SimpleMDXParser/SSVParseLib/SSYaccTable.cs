namespace SSVParseLib
{
    using System;
    using System.IO;

    public class SSYaccTable
    {
        public SSLexSubtable[] m_lexSubtables;
        public SSYaccTableProd[] m_prods;
        public SSYaccTableRow[] m_rows;
        public int SSYaccTableEntrySize;
        public int SSYaccTableHeaderSize;
        public int SSYaccTableRowSize;

        public SSYaccTable()
        {
            this.SSYaccTableHeaderSize = 20;
            this.SSYaccTableEntrySize = 8;
            this.SSYaccTableRowSize = 12;
        }

        public SSYaccTable(string q_file)
        {
            this.SSYaccTableHeaderSize = 20;
            this.SSYaccTableEntrySize = 8;
            this.SSYaccTableRowSize = 12;
            FileStream stream = File.Open(q_file, FileMode.Open, FileAccess.Read, FileShare.Read);
            int length = (int) stream.Length;
            byte[] buffer = new byte[length];
            length = stream.Read(buffer, 0, length);
            SSYaccTableHeader header = new SSYaccTableHeader {
                type = this.convertInt(buffer, 0),
                prodOrLar = this.convertInt(buffer, 4),
                numRows = this.convertInt(buffer, 8),
                rowOffset = this.convertInt(buffer, 12),
                prodOffset = this.convertInt(buffer, 0x10)
            };
            int num2 = header.numProds();
            this.m_prods = new SSYaccTableProd[num2];
            int prodOffset = header.prodOffset;
            for (int i = 0; i < num2; i++)
            {
                int num5 = this.convertInt(buffer, prodOffset);
                int num6 = this.convertInt(buffer, prodOffset + 4);
                this.m_prods[i] = new SSYaccTableProd(num5, num6);
                prodOffset += 8;
            }
            this.m_rows = new SSYaccTableRow[header.numRows];
            int rowOffset = header.rowOffset;
            for (int j = 0; j < header.numRows; j++)
            {
                int num9 = this.convertInt(buffer, rowOffset);
                int num10 = this.convertInt(buffer, rowOffset + 4);
                int num11 = this.convertInt(buffer, rowOffset + 8);
                this.m_rows[j] = new SSYaccTableRow(num9, num10, num11, buffer, rowOffset + 12);
                rowOffset += this.SSYaccTableRowSize + (this.SSYaccTableEntrySize * this.m_rows[j].numEntries());
            }
        }

        public int convertInt(byte[] b, int offset)
        {
            long num = (b[offset + 3] << 0x18) & ((long) 0xff000000L);
            long num2 = (b[offset + 2] << 0x10) & 0xff0000;
            long num3 = (b[offset + 1] << 8) & 0xff00;
            long num4 = b[offset];
            return (int) (((ulong) (((num | num2) | num3) | num4)) & 0xffffffffL);
        }

        public SSLexSubtable larTable(int q_entry)
        {
            return this.m_lexSubtables[q_entry];
        }

        public SSYaccTableProd lookupProd(int q_index)
        {
            return this.m_prods[q_index];
        }

        public SSYaccTableRow lookupRow(int q_state)
        {
            return this.m_rows[q_state];
        }
    }
}

