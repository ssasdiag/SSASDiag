namespace SimpleMDXParser
{
    using System;
    using System.Collections.Generic;

    public class Analyzer
    {
        private bool m_IsInCommonSubExpr = false;
        private bool m_IsInLHS = false;
        private bool m_IsInRHS = false;
        private MessageCollection m_Messages = new MessageCollection();
        private Dictionary<string, MDXExpNode> m_SeenExprs = new Dictionary<string, MDXExpNode>();

        internal Analyzer()
        {
        }

        internal void Add(Message m)
        {
            this.m_Messages.Add(m);
        }

        internal bool IsInExisting()
        {
            foreach (MDXExpNode node in this.m_SeenExprs.Values)
            {
                if (node.GetType() == typeof(MDXUnaryOpNode))
                {
                    MDXUnaryOpNode node2 = node as MDXUnaryOpNode;
                    if (node2.m_Op.Equals("EXISTING", StringComparison.InvariantCultureIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal bool InCommonSubExpr
        {
            get
            {
                return this.m_IsInCommonSubExpr;
            }
            set
            {
                this.m_IsInCommonSubExpr = value;
            }
        }

        internal bool InLHS
        {
            get
            {
                return this.m_IsInLHS;
            }
            set
            {
                this.m_IsInLHS = value;
            }
        }

        internal bool InRHS
        {
            get
            {
                return this.m_IsInRHS;
            }
            set
            {
                this.m_IsInRHS = value;
            }
        }

        public MessageCollection Messages
        {
            get
            {
                return this.m_Messages;
            }
        }

        internal Dictionary<string, MDXExpNode> SeenExprs
        {
            get
            {
                return this.m_SeenExprs;
            }
        }
    }
}

