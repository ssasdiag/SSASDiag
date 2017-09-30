namespace SSVParseLib
{
    using System;

    public class SSYacc
    {
        public bool m_abort;
        public int m_action;
        public int m_cache = 0;
        public SSYaccStackElement m_element;
        public SSLexLexeme m_endLexeme;
        public bool m_endOfInput;
        public const int m_eofToken = -1;
        public bool m_error;
        public const int m_errorToken = -2;
        public SSLexLexeme m_larLookahead;
        public int m_leftside;
        public SSLex m_lex;
        public SSYaccCache m_lexemeCache;
        public SSLexSubtable m_lexSubtable;
        public SSLexLexeme m_lookahead;
        public int m_production;
        public int m_productionSize;
        public SSYaccStack m_stack;
        public int m_state;
        public SSYaccTable m_table;
        public SSYaccStackElement m_treeRoot;
        public const int SSYaccActionAccept = 3;
        public const int SSYaccActionConflict = 4;
        public const int SSYaccActionError = 1;
        public const int SSYaccActionReduce = 2;
        public const int SSYaccActionShift = 0;
        public const int SSYaccLexemeCacheMax = -1;

        public SSYacc(SSYaccTable q_table, SSLex q_lex)
        {
            this.m_lex = q_lex;
            this.m_abort = false;
            this.m_error = false;
            this.m_table = q_table;
            this.m_endOfInput = false;
            this.m_endLexeme = new SSLexLexeme("eof", -1);
            this.m_endLexeme.setToken(-1);
            this.m_stack = new SSYaccStack(5, 5);
            this.m_lexemeCache = new SSYaccCache();
            this.m_element = this.stackElement();
            this.push();
        }

        public SSYaccStackElement addSubTree()
        {
            SSYaccStackElement element = this.stackElement();
            element.createSubTree(this.m_productionSize);
            for (int i = 0; i < this.m_productionSize; i++)
            {
                element.addSubTree(i, this.elementFromProduction(i));
            }
            return element;
        }

        public bool doConflict()
        {
            this.m_cache = 0;
            int num = 0;
            num = this.m_lexSubtable.lookup(num, this.m_lookahead.token());
            while ((this.m_larLookahead = this.getLexemeCache()) != null)
            {
                num = this.m_lexSubtable.lookup(num, this.m_larLookahead.token());
                if (num == -1)
                {
                    break;
                }
                SSLexFinalState state = this.m_lexSubtable.lookupFinal(num);
                if (state.isFinal())
                {
                    if (state.isReduce())
                    {
                        this.m_production = state.token();
                        SSYaccTableProd prod = this.m_table.lookupProd(this.m_production);
                        this.m_leftside = prod.leftside();
                        this.m_productionSize = prod.size();
                        return this.doReduce();
                    }
                    this.m_state = state.token();
                    return this.doShift();
                }
            }
            return this.doLarError();
        }

        public bool doError()
        {
            this.m_error = true;
            return this.error(this.m_state, this.m_lookahead);
        }

        public bool doGetLexeme(bool q_look)
        {
            this.m_lookahead = this.m_lexemeCache.remove();
            if (this.m_lookahead == null)
            {
                return this.getLexeme(q_look);
            }
            if (this.larLookahead(this.m_lookahead))
            {
                return true;
            }
            if (q_look)
            {
                this.lookupAction(this.m_state, this.m_lookahead.token());
            }
            return false;
        }

        public bool doLarError()
        {
            this.m_error = true;
            return this.larError(this.m_state, this.m_lookahead, this.m_larLookahead);
        }

        public bool doReduce()
        {
            this.m_element = this.reduce(this.m_production, this.m_productionSize);
            if (this.m_element == null)
            {
                return true;
            }
            this.pop(this.m_productionSize);
            return this.goTo(this.m_leftside);
        }

        public bool doShift()
        {
            this.m_element = this.shift(this.m_lookahead);
            if (this.m_element == null)
            {
                return true;
            }
            this.m_element.setLexeme(this.m_lookahead);
            this.m_element.setState(this.m_state);
            this.push();
            return this.doGetLexeme(true);
        }

        public SSYaccStackElement elementFromProduction(int q_index)
        {
            int index = (this.m_stack.getSize() - this.m_productionSize) + q_index;
            if ((index < 0) || (index >= this.m_stack.getSize()))
            {
                return null;
            }
            return (SSYaccStackElement) this.m_stack.elementAt(index);
        }

        public virtual bool error(int q_state, SSLexLexeme q_look)
        {
            return this.syncErr();
        }

        public bool getLexeme(bool q_look)
        {
            if (this.m_endOfInput)
            {
                return true;
            }
            this.m_lookahead = this.nextLexeme();
            if (this.m_lookahead == null)
            {
                this.m_endOfInput = true;
                this.m_lookahead = this.m_endLexeme;
            }
            if (q_look)
            {
                this.lookupAction(this.m_state, this.m_lookahead.token());
            }
            return false;
        }

        public SSLexLexeme getLexemeCache()
        {
            SSLexLexeme endLexeme = null;
            if ((this.m_cache != -1) && this.m_lexemeCache.hasElements())
            {
                endLexeme = (SSLexLexeme) this.m_lexemeCache.Dequeue();
            }
            if (endLexeme == null)
            {
                this.m_cache = -1;
                endLexeme = this.nextLexeme();
                if (endLexeme == null)
                {
                    endLexeme = this.m_endLexeme;
                }
                this.m_lexemeCache.Enqueue(endLexeme);
            }
            return endLexeme;
        }

        public bool goTo(int q_goto)
        {
            if (this.lookupGoto(this.m_state, this.m_leftside))
            {
                return true;
            }
            this.m_element.setState(this.m_state);
            this.push();
            this.lookupAction(this.m_state, this.m_lookahead.token());
            return false;
        }

        public bool larError(int q_state, SSLexLexeme q_look, SSLexLexeme q_larLook)
        {
            return this.error(q_state, q_look);
        }

        public bool larLookahead(SSLexLexeme q_lex)
        {
            return false;
        }

        public void lookupAction(int q_state, int q_token)
        {
            SSYaccTableRowEntry entry = this.m_table.lookupRow(q_state).lookupAction(q_token);
            if (entry == null)
            {
                this.m_action = 1;
            }
            else
            {
                switch ((this.m_action = entry.action()))
                {
                    case 0:
                        this.m_state = entry.entry();
                        return;

                    case 1:
                    case 3:
                        break;

                    case 2:
                    {
                        SSYaccTableProd prod = this.m_table.lookupProd(entry.entry());
                        this.m_production = entry.entry();
                        this.m_leftside = prod.leftside();
                        this.m_productionSize = prod.size();
                        return;
                    }
                    case 4:
                        this.m_lexSubtable = this.m_table.larTable(entry.entry());
                        break;

                    default:
                        return;
                }
            }
        }

        public bool lookupGoto(int q_state, int q_token)
        {
            SSYaccTableRowEntry entry = this.m_table.lookupRow(q_state).lookupGoto(q_token);
            if (entry == null)
            {
                return true;
            }
            this.m_state = entry.entry();
            return false;
        }

        public virtual SSLexLexeme nextLexeme()
        {
            return this.m_lex.next();
        }

        public bool parse()
        {
        
            if (this.doGetLexeme(true))
            {
                return true;
            }
        Label_000B:
            if (!this.m_abort)
            {
                switch (this.m_action)
                {
                    case 0:
                        if (!this.doShift())
                        {
                            goto Label_000B;
                        }
                        return true;

                    case 1:
                        if (!this.doError())
                        {
                            goto Label_000B;
                        }
                        if (!this.m_endOfInput)
                           return false;// goto TryAgainAfterError;
                        return true;

                    case 2:
                        if (!this.doReduce())
                        {
                            goto Label_000B;
                        }
                        return true;

                    case 3:
                        this.m_treeRoot = this.m_element;
                        return this.m_error;

                    case 4:
                        if (!this.doConflict())
                        {
                            goto Label_000B;
                        }
                        return true;
                }
            }
            return true;
        }

        public bool pop(int q_pop)
        {
            for (int i = 0; i < q_pop; i++)
            {
                this.m_stack.pop();
            }
            this.m_state = ((SSYaccStackElement) this.m_stack.peek()).state();
            return false;
        }

        public bool push()
        {
            this.m_stack.push(this.m_element);
            return true;
        }

        public bool push(SSYaccStackElement q_element)
        {
            this.m_stack.push(q_element);
            return true;
        }

        public virtual SSYaccStackElement reduce(int q_prod, int q_length)
        {
            return this.stackElement();
        }

        public void setAbort()
        {
            this.m_abort = true;
        }

        public virtual SSYaccStackElement shift(SSLexLexeme q_lexeme)
        {
            return this.stackElement();
        }

        public virtual SSYaccStackElement stackElement()
        {
            return new SSYaccStackElement();
        }

        public bool syncErr()
        {
            SSYaccTableRow row3;
            SSYaccSet set = new SSYaccSet();
            for (int i = 0; i < this.m_stack.getSize(); i++)
            {
                int num2 = ((SSYaccStackElement) this.m_stack.elementAt(i)).state();
                SSYaccTableRow row = this.m_table.lookupRow(num2);
                if (row.hasSync() || row.hasSyncAll())
                {
                    for (int j = 0; j < row.action(); j++)
                    {
                        SSYaccTableRowEntry entry = row.lookupEntry(j);
                        if (row.hasSyncAll() || entry.hasSync())
                        {
                            int num4 = entry.token();
                            set.add(num4);
                        }
                    }
                }
                if (row.hasError())
                {
                    SSYaccTableRow row2 = this.m_table.lookupRow(row.lookupError().entry());
                    for (int k = 0; k < row2.action(); k++)
                    {
                        int num6 = row2.lookupEntry(k).token();
                        set.add(num6);
                    }
                }
            }
            if (set.Count == 0)
            {
                return true;
            }
            while (!set.locate(this.m_lookahead.token()))
            {
                if (this.doGetLexeme(false))
                {
                    return true;
                }
            }
        Label_012B:
            row3 = this.m_table.lookupRow(this.m_state);
            if (row3.hasError())
            {
                this.lookupAction(row3.lookupError().entry(), this.m_lookahead.token());
                if (this.m_action != 1)
                {
                    SSLexLexeme lexeme = new SSLexLexeme("%error", -2);
                    this.m_element = this.stackElement();
                    this.m_element.setLexeme(lexeme);
                    this.m_element.setState(row3.lookupError().entry());
                    this.push();
                    goto Label_0226;
                }
            }
            if (row3.hasSyncAll())
            {
                this.lookupAction(this.m_state, this.m_lookahead.token());
                if (this.m_action == 1)
                {
                    goto Label_0219;
                }
                goto Label_0226;
            }
            if (row3.hasSync() && (row3.lookupAction(this.m_lookahead.token()) != null))
            {
                this.lookupAction(this.m_state, this.m_lookahead.token());
                goto Label_0226;
            }
        Label_0219:
            this.pop(1);
            goto Label_012B;
        Label_0226:
            return false;
        }

        public SSYaccStackElement treeRoot()
        {
            return this.m_treeRoot;
        }

        public bool wasAborted()
        {
            return this.m_abort;
        }

        public bool wasError()
        {
            return this.m_error;
        }
    }
}

