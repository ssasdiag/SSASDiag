namespace SimpleMDXParser
{
    using SSVParseLib;
    using System;

    public class Message
    {
        private bool m_Hide;
        private int m_Id;
        private Locator m_Location;
        private MDXNode m_Node;
        private int m_Severity;
        private Source m_Source;
        private string m_Text;
        private MessageType m_Type;
        private string m_URL;

        internal Message()
        {
            this.m_Location = new Locator();
            this.m_Type = MessageType.Informational;
        }

        internal Message(MDXNode n)
        {
            this.m_Location = new Locator(n.Locator);
            this.m_Node = n;
            this.m_Type = MessageType.Informational;
        }

        public Message(SSLexLexeme lexeme)
        {
            this.m_Location = new Locator(lexeme);
        }

        public bool Hide
        {
            get
            {
                return this.m_Hide;
            }
            set
            {
                this.m_Hide = value;
            }
        }

        public int Id
        {
            get
            {
                return this.m_Id;
            }
            set
            {
                this.m_Id = value;
            }
        }

        public Locator Location
        {
            get
            {
                return this.m_Location;
            }
            set
            {
                this.m_Location = value;
            }
        }

        public MDXNode Node
        {
            get
            {
                return this.m_Node;
            }
            set
            {
                this.m_Node = value;
            }
        }

        public int Severity
        {
            get
            {
                return this.m_Severity;
            }
            set
            {
                this.m_Severity = value;
            }
        }

        public Source Source
        {
            get
            {
                return this.m_Source;
            }
            set
            {
                this.m_Source = value;
            }
        }

        public string Text
        {
            get
            {
                return this.m_Text;
            }
            set
            {
                this.m_Text = value;
            }
        }

        public MessageType Type
        {
            get
            {
                return this.m_Type;
            }
            set
            {
                this.m_Type = value;
            }
        }

        public string URL
        {
            get
            {
                return this.m_URL;
            }
            set
            {
                this.m_URL = value;
            }
        }
    }
}

