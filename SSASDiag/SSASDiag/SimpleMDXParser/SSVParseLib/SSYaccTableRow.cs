namespace SSVParseLib
{
    using System;

    public class SSYaccTableRow
    {
        public int m_action;
        public SSYaccTableRowEntry[] m_entries;
        public bool m_error;
        public int m_flags;
        public int m_goto;
        public bool m_sync;
        public bool m_syncAll;
        public int SSYaccTableEntrySize;
        public int SSYaccTableRowFlagError;
        public int SSYaccTableRowFlagSync;
        public int SSYaccTableRowFlagSyncAll;

        public SSYaccTableRow(int[] q_data, int q_index)
        {
            this.SSYaccTableEntrySize = 8;
            this.SSYaccTableRowFlagSync = 1;
            this.SSYaccTableRowFlagError = 2;
            this.SSYaccTableRowFlagSyncAll = 4;
            this.m_action = q_data[q_index];
            this.m_goto = q_data[q_index + 1];
            this.m_error = q_data[q_index + 2] != 0;
            this.m_syncAll = q_data[q_index + 3] != 0;
            this.m_sync = q_data[q_index + 4] != 0;
            this.m_entries = new SSYaccTableRowEntry[this.numEntries()];
            q_index += 5;
            for (int i = 0; i < this.numEntries(); i++)
            {
                this.m_entries[i] = new SSYaccTableRowEntry(q_data[q_index], q_data[q_index + 1], q_data[q_index + 2], q_data[q_index + 3]);
                q_index += 4;
            }
        }

        public SSYaccTableRow(int q_flags, int q_goto, int q_action, byte[] q_b, int q_index)
        {
            this.SSYaccTableEntrySize = 8;
            this.SSYaccTableRowFlagSync = 1;
            this.SSYaccTableRowFlagError = 2;
            this.SSYaccTableRowFlagSyncAll = 4;
            this.m_action = q_action;
            this.m_goto = q_goto;
            this.m_error = (q_flags & this.SSYaccTableRowFlagError) != 0;
            this.m_syncAll = (q_flags & this.SSYaccTableRowFlagSyncAll) != 0;
            this.m_sync = (q_flags & this.SSYaccTableRowFlagSync) != 0;
            this.m_entries = new SSYaccTableRowEntry[this.numEntries()];
            int num = this.numEntries();
            for (int i = 0; i < num; i++)
            {
                int num3 = this.convertInt(q_b, q_index);
                int num4 = this.convertInt(q_b, q_index + 4);
                this.m_entries[i] = new SSYaccTableRowEntry(num3, num4);
                q_index += this.SSYaccTableEntrySize;
            }
        }

        public int action()
        {
            return this.m_action;
        }

        public int convertInt(byte[] b, int offset)
        {
            long num = (b[offset + 3] << 0x18) & ((long) 0xff000000L);
            long num2 = (b[offset + 2] << 0x10) & 0xff0000;
            long num3 = (b[offset + 1] << 8) & 0xff00;
            long num4 = b[offset];
            return (int) (((ulong) (((num | num2) | num3) | num4)) & 0xffffffffL);
        }

        public bool hasError()
        {
            return this.m_error;
        }

        public bool hasSync()
        {
            return this.m_sync;
        }

        public bool hasSyncAll()
        {
            return this.m_syncAll;
        }

        public SSYaccTableRowEntry lookupAction(int q_index)
        {
            for (int i = 0; i < this.m_action; i++)
            {
                if (this.m_entries[i].token() == q_index)
                {
                    return this.m_entries[i];
                }
            }
            return null;
        }

        public SSYaccTableRowEntry lookupEntry(int q_index)
        {
            return this.m_entries[q_index];
        }

        public SSYaccTableRowEntry lookupError()
        {
            if (!this.hasError())
            {
                return null;
            }
            return this.m_entries[this.m_goto + this.m_action];
        }

        public SSYaccTableRowEntry lookupGoto(int q_index)
        {
            for (int i = this.m_action; i < (this.m_action + this.m_goto); i++)
            {
                if (this.m_entries[i].token() == q_index)
                {
                    return this.m_entries[i];
                }
            }
            return null;
        }

        public int numEntries()
        {
            int num = this.hasError() ? 1 : 0;
            return ((this.m_goto + this.m_action) + num);
        }
    }
}

