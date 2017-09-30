namespace SimpleMDXParser
{
    using System;

    public class MDXParserException : Exception
    {
        private MessageCollection m_Messages;

        internal MDXParserException(Message m)
        {
            this.m_Messages = new MessageCollection();
            this.m_Messages.Add(m);
        }

        internal MDXParserException(MessageCollection ms)
        {
            this.m_Messages = ms;
        }

        public MessageCollection Messages
        {
            get
            {
                return this.m_Messages;
            }
        }
    }
}

