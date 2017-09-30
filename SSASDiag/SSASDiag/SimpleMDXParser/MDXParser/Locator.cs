namespace SimpleMDXParser
{
    using SSVParseLib;
    using System;

    public class Locator
    {
        private int m_Column;
        private int m_Length;
        private int m_Line;
        private int m_Position;
        private object m_Tag;

        public Locator()
        {
        }

        public Locator(Locator l)
        {
            this.Line = l.Line;
            this.Column = l.Column;
            this.Position = l.Position;
            this.Length = l.Length;
            this.Tag = l.Tag;
        }

        public Locator(SSLexLexeme lexeme)
        {
            this.Line = lexeme.line() + 1;
            this.Column = lexeme.offset() + 1;
            this.Position = (lexeme.index() - lexeme.length()) - 1;
            if (this.Position < 0)
            {
                this.Position = 0;
            }
            this.Length = lexeme.length();
        }

        public void Adjust(Locator l)
        {
            if (l.Position != 0)
            {
                if (this.Line <= 1)
                {
                    this.Column += l.Column;
                }
                this.Line += l.Line - 1;
                this.Position += l.Position;
            }
        }

        public int Column
        {
            get
            {
                return this.m_Column;
            }
            set
            {
                this.m_Column = value;
            }
        }

        public int Length
        {
            get
            {
                return this.m_Length;
            }
            set
            {
                this.m_Length = value;
            }
        }

        public int Line
        {
            get
            {
                return this.m_Line;
            }
            set
            {
                this.m_Line = value;
            }
        }

        public int Position
        {
            get
            {
                return this.m_Position;
            }
            set
            {
                this.m_Position = value;
            }
        }

        public object Tag
        {
            get
            {
                return this.m_Tag;
            }
            set
            {
                this.m_Tag = value;
            }
        }
    }
}

