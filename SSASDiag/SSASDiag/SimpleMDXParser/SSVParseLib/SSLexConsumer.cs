namespace SSVParseLib
{
    using System;

    public abstract class SSLexConsumer
    {
        public char m_bof;
        public char[] m_buffer = new char[0x1000];
        public int m_bufferIndex;
        public int m_consumed;
        public char m_current;
        public bool m_endOfData;
        public int m_index;
        public int m_length;
        public int m_line;
        public int m_offset;
        public int m_scanLine;
        public int m_scanOffset;
        public int m_start;

        protected SSLexConsumer()
        {
        }

        public int absoluteOffset()
        {
            return this.m_start;
        }

        public void flushEndOfLine(SSLexMark q_mark)
        {
            q_mark.m_line--;
            q_mark.m_index--;
        }

        public void flushLexeme()
        {
            this.m_start = this.m_index;
            this.m_line += this.m_scanLine;
            this.m_offset = this.m_scanOffset;
            this.m_scanLine = 0;
            this.m_bufferIndex = 0;
        }

        public virtual void flushLexeme(SSLexMark q_mark)
        {
            this.m_start = this.m_index = q_mark.m_index;
            this.m_line += q_mark.m_line;
            this.m_offset = q_mark.m_offset;
            this.m_scanLine = 0;
            this.m_scanOffset = q_mark.m_offset;
            this.m_bufferIndex = 0;
        }

        public void flushStartOfLine(SSLexMark q_mark)
        {
            this.m_line++;
            this.m_start++;
            q_mark.m_line--;
            this.m_offset = 1;
        }

        public char getCurrent()
        {
            return this.m_current;
        }

        public abstract bool getNext();
        public int index()
        {
            return this.m_index;
        }

        public char[] lexemeBuffer()
        {
            char[] chArray = new char[this.lexemeLength()];
            for (int i = 0; i < (this.m_bufferIndex - 1); i++)
            {
                chArray[i] = this.m_buffer[i];
            }
            return chArray;
        }

        public char[] lexemeBuffer(SSLexMark q_mark)
        {
            int num = this.lexemeLength(q_mark);
            char[] chArray = new char[num];
            for (int i = 0; i < num; i++)
            {
                chArray[i] = this.m_buffer[i];
            }
            return chArray;
        }

        public int lexemeLength()
        {
            return (this.m_index - this.m_start);
        }

        public int lexemeLength(SSLexMark q_mark)
        {
            return (q_mark.index() - this.m_start);
        }

        public int line()
        {
            return this.m_line;
        }

        public SSLexMark mark()
        {
            return new SSLexMark(this.m_scanLine, this.m_scanOffset, this.m_index);
        }

        public bool next()
        {
            if (this.m_endOfData)
            {
                return false;
            }
            this.m_current = this.m_bof;
            if (this.m_current != '\0')
            {
                this.m_bof = '\0';
                return true;
            }
            if (this.m_bufferIndex >= this.m_buffer.Length)
            {
                if (!this.getNext())
                {
                    this.m_index++;
                    this.m_bufferIndex++;
                    this.m_endOfData = true;
                    return false;
                }
                this.m_index++;
                this.m_bufferIndex++;
                char[] array = new char[this.m_buffer.Length + 0x1000];
                this.m_buffer.CopyTo(array, 0);
                this.m_buffer = array;
                this.m_buffer[this.m_bufferIndex] = this.m_current;
            }
            else
            {
                if (!this.getNext())
                {
                    this.m_index++;
                    this.m_bufferIndex++;
                    this.m_endOfData = true;
                    return false;
                }
                this.m_index++;
                this.m_buffer[this.m_bufferIndex++] = this.m_current;
            }
            if (this.m_current == '\n')
            {
                this.m_scanLine++;
                this.m_scanOffset = 1;
            }
            else
            {
                this.m_scanOffset++;
            }
            return true;
        }

        public int offset()
        {
            return this.m_offset;
        }
    }
}

