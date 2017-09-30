namespace SSVParseLib
{
    using System;
    using System.IO;

    public class SSLexFileConsumer : SSLexConsumer
    {
        public FileStream m_fileStream;
        public bool m_first = true;
        public bool m_reversedUnicode;
        public bool m_unicode;

        public SSLexFileConsumer(string q_name, bool q_unicode)
        {
            this.m_unicode = q_unicode;
            this.m_reversedUnicode = true;
            this.m_fileStream = File.Open(q_name, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public override void flushLexeme(SSLexMark q_mark)
        {
            base.m_start = base.m_index = q_mark.m_index;
            base.m_line += q_mark.m_line;
            base.m_offset = q_mark.m_offset;
            base.m_scanLine = 0;
            base.m_scanOffset = q_mark.m_offset;
            base.m_bufferIndex = 0;
            this.m_fileStream.Position = base.m_index;
        }

        public override bool getNext()
        {
            if (this.m_first && this.m_unicode)
            {
                this.m_first = false;
                if (!this.readByte())
                {
                    return false;
                }
                if ((base.m_current == '\x00ff') || (base.m_current == '\x00fe'))
                {
                    char current = base.m_current;
                    if (!this.readByte())
                    {
                        return false;
                    }
                    if ((current == '\x00ff') && (base.m_current == '\x00fe'))
                    {
                        this.m_reversedUnicode = true;
                        return this.readUnicodeByte();
                    }
                    if ((current == '\x00fe') && (base.m_current == '\x00ff'))
                    {
                        this.m_reversedUnicode = false;
                        return this.readUnicodeByte();
                    }
                    base.m_current = (char) ((base.m_current << 8) | current);
                }
                return true;
            }
            if (this.m_unicode)
            {
                return this.readUnicodeByte();
            }
            return this.readByte();
        }

        public bool readByte()
        {
            try
            {
                int num = this.m_fileStream.ReadByte();
                if (num == -1)
                {
                    return false;
                }
                base.m_current = (char) num;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool readUnicodeByte()
        {
            if (!this.readByte())
            {
                return false;
            }
            char current = base.m_current;
            if (!this.readByte())
            {
                return false;
            }
            if (this.m_reversedUnicode)
            {
                base.m_current = (char) ((base.m_current << 8) | current);
            }
            else
            {
                base.m_current = (char) ((current << 8) | base.m_current);
            }
            return true;
        }
    }
}

