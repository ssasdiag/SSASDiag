namespace SSVParseLib
{
    using System;

    public class SSLex
    {
        public SSLexConsumer m_consumer;
        public char[] m_currentChar;
        public int m_state;
        public SSLexTable m_table;

        public SSLex(SSLexTable q_table, SSLexConsumer q_consumer)
        {
            this.m_table = q_table;
            this.m_consumer = q_consumer;
            this.m_currentChar = new char[1];
        }

        public virtual bool complete(SSLexLexeme q_lexeme)
        {
            return true;
        }

        public SSLexConsumer consumer()
        {
            return this.m_consumer;
        }

        public virtual bool error(SSLexLexeme q_lexeme)
        {
            return false;
        }

        public SSLexLexeme next()
        {
            bool flag = false;
            SSLexLexeme lexeme = null;
            SSLexFinalState state = null;
        Label_0006:
            this.m_state = 0;
            flag = false;
            SSLexMark mark = null;
            state = this.m_table.lookupFinal(this.m_state);
            if (state.isFinal())
            {
                this.m_consumer.mark();
            }
            while (this.m_consumer.next())
            {
                flag = true;
                this.m_currentChar[0] = this.m_consumer.getCurrent();
                this.m_table.translateClass(this.m_currentChar);
                this.m_state = this.m_table.lookup(this.m_state, this.m_currentChar[0]);
                if (this.m_state == -1)
                {
                    break;
                }
                SSLexFinalState state2 = this.m_table.lookupFinal(this.m_state);
                if (state2.isFinal())
                {
                    mark = this.m_consumer.mark();
                    state = state2;
                }
                if (state2.isContextStart())
                {
                    this.m_consumer.mark();
                }
            }
            if (flag)
            {
                if (state.isContextEnd() && (mark != null))
                {
                    this.m_consumer.flushEndOfLine(mark);
                }
                if (state.isIgnore() && (mark != null))
                {
                    this.m_consumer.flushLexeme(mark);
                    if (state.isPop() && state.isPush())
                    {
                        this.m_table.gotoSubtable(state.pushIndex());
                    }
                    else if (state.isPop())
                    {
                        this.m_table.popSubtable();
                    }
                    else if (state.isPush())
                    {
                        this.m_table.pushSubtable(state.pushIndex());
                    }
                    goto Label_0006;
                }
                if (!state.isFinal() || (mark == null))
                {
                    lexeme = new SSLexLexeme(this.m_consumer);
                    if (this.error(lexeme))
                    {
                        return lexeme;
                    }
                    this.m_consumer.flushLexeme();
                    lexeme = null;
                    goto Label_0006;
                }
                if (state.isPop() && state.isPush())
                {
                    this.m_table.gotoSubtable(state.pushIndex());
                }
                else if (state.isPop())
                {
                    this.m_table.popSubtable();
                }
                else if (state.isPush())
                {
                    this.m_table.pushSubtable(state.pushIndex());
                }
                if ((state.isStartOfLine() && (this.m_consumer.line() != 0)) && (this.m_consumer.offset() != 0))
                {
                    this.m_consumer.flushStartOfLine(mark);
                }
                lexeme = new SSLexLexeme(this.m_consumer, state, mark);
                if (state.isKeyword())
                {
                    this.m_table.findKeyword(lexeme);
                }
                this.m_consumer.flushLexeme(mark);
                if (!this.complete(lexeme))
                {
                    lexeme = null;
                    goto Label_0006;
                }
            }
            return lexeme;
        }

        public SSLexTable table()
        {
            return this.m_table;
        }
    }
}

