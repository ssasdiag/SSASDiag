namespace SSVParseLib
{
    using System;

    public class SSLexFinalState
    {
        public static int m_flagContextEnd = 0x80;
        public static int m_flagContextStart = 1;
        public static int m_flagFinal = 0x10;
        public static int m_flagIgnore = 0x40;
        public static int m_flagKeyword = 0x200;
        public static int m_flagParseToken = 0x400;
        public static int m_flagPop = 8;
        public static int m_flagPush = 0x20;
        public static int m_flagReduce = 0x100;
        public int m_flags;
        public static int m_flagStartOfLine = 2;
        public int m_pushIndex;
        public int m_token;

        public SSLexFinalState(int[] q_final, int q_index)
        {
            this.m_token = q_final[q_index];
            this.m_pushIndex = q_final[q_index + 1];
            this.m_flags = q_final[q_index + 2];
        }

        public bool isContextEnd()
        {
            return ((this.m_flags & m_flagContextEnd) != 0);
        }

        public bool isContextStart()
        {
            return ((this.m_flags & m_flagContextStart) != 0);
        }

        public bool isFinal()
        {
            return ((this.m_flags & m_flagFinal) != 0);
        }

        public bool isIgnore()
        {
            return ((this.m_flags & m_flagIgnore) != 0);
        }

        public bool isKeyword()
        {
            return ((this.m_flags & m_flagKeyword) != 0);
        }

        public bool isParseToken()
        {
            return ((this.m_flags & m_flagParseToken) != 0);
        }

        public bool isPop()
        {
            return ((this.m_flags & m_flagPop) != 0);
        }

        public bool isPush()
        {
            return ((this.m_flags & m_flagPush) != 0);
        }

        public bool isReduce()
        {
            return ((this.m_flags & m_flagReduce) != 0);
        }

        public bool isStartOfLine()
        {
            return ((this.m_flags & m_flagStartOfLine) != 0);
        }

        public int pushIndex()
        {
            return this.m_pushIndex;
        }

        public int token()
        {
            return this.m_token;
        }
    }
}

