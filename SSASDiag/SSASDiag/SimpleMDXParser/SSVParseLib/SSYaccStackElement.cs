namespace SSVParseLib
{
    using System;

    public class SSYaccStackElement
    {
        public SSLexLexeme m_lexeme = null;
        public int m_state = 0;
        public SSYaccStackElement[] m_subTree = null;
        public int m_subTreeSize;

        public bool addSubTree(int q_index, SSYaccStackElement q_ele)
        {
            if ((q_index < 0) || (q_index >= this.m_subTreeSize))
            {
                return false;
            }
            this.m_subTree[q_index] = q_ele;
            return true;
        }

        public void createSubTree(int q_size)
        {
            this.m_subTreeSize = q_size;
            if (q_size > 0)
            {
                this.m_subTree = new SSYaccStackElement[q_size];
            }
        }

        public SSYaccStackElement getSubTree(int q_index)
        {
            if ((q_index >= 0) && (q_index < this.m_subTreeSize))
            {
                return this.m_subTree[q_index];
            }
            return null;
        }

        public int getSubTreeSize()
        {
            return this.m_subTreeSize;
        }

        public SSLexLexeme lexeme()
        {
            return this.m_lexeme;
        }

        public void setLexeme(SSLexLexeme q_lexeme)
        {
            this.m_lexeme = q_lexeme;
        }

        public void setState(int q_state)
        {
            this.m_state = q_state;
        }

        public int state()
        {
            return this.m_state;
        }
    }
}

