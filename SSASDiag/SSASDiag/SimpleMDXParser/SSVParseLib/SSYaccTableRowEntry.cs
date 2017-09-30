namespace SSVParseLib
{
    using System;

    public class SSYaccTableRowEntry
    {
        public int m_action;
        public int m_entry;
        public bool m_sync;
        public int m_token;
        public const int SSYaccActionAccept = 3;
        public const int SSYaccActionConflict = 4;
        public const int SSYaccActionError = 1;
        public const int SSYaccActionReduce = 2;
        public const int SSYaccActionShift = 0;
        public const int SSYaccTableEntryFlagAccept = 0x10000000;
        public const int SSYaccTableEntryFlagConflict = 0x8000000;
        public const uint SSYaccTableEntryFlagMask = 0xf8000000;
        public const int SSYaccTableEntryFlagReduce = 0x20000000;
        public const int SSYaccTableEntryFlagShift = 0x40000000;
        public const uint SSYaccTableEntryFlagSync = 0x80000000;

        public SSYaccTableRowEntry(int q_entry, int q_token)
        {
            this.m_token = q_token;
            this.m_entry = q_entry & ((int) 0x7ffffffL);
            if ((q_entry & 0x40000000) != 0)
            {
                this.m_action = 0;
            }
            else if ((q_entry & 0x20000000) != 0)
            {
                this.m_action = 2;
            }
            else if ((q_entry & 0x10000000) != 0)
            {
                this.m_action = 3;
            }
            else if ((q_entry & 0x8000000) != 0)
            {
                this.m_action = 4;
            }
            this.m_sync = (q_entry & 0x80000000L) != 0L;
        }

        public SSYaccTableRowEntry(int q_token, int q_entry, int q_action, int q_sync)
        {
            this.m_token = q_token;
            this.m_entry = q_entry;
            this.m_action = q_action;
            this.m_sync = q_sync != 0;
        }

        public int action()
        {
            return this.m_action;
        }

        public int entry()
        {
            return this.m_entry;
        }

        public bool hasSync()
        {
            return this.m_sync;
        }

        public int token()
        {
            return this.m_token;
        }
    }
}

