namespace SSVParseLib
{
    using System;
    using System.Collections;
    using System.IO;

    public class SSLexTable
    {
        public SSLexCharacterClass[] m_charClassTables;
        public int m_classes;
        public int m_classMax;
        public int m_classMin;
        public SSLexFinalState[] m_final;
        public SSLexKeyTable[] m_keyTables;
        public Stack m_stack;
        public SSLexSubtable[] m_subTables;
        public int SSLexDfaClassTableEntryHeaderSize;
        public int SSLexDfaClassTableHeaderSize;
        public int SSLexDfaKeywordTableHeaderSize;
        public int SSLexDfaTableHeaderSize;
        public const int SSLexStateInvalid = -1;
        public int SSLexTableHeaderSize;

        public SSLexTable()
        {
            this.SSLexTableHeaderSize = 0x24;
            this.SSLexDfaTableHeaderSize = 40;
            this.SSLexDfaKeywordTableHeaderSize = 40;
            this.SSLexDfaClassTableHeaderSize = 12;
            this.SSLexDfaClassTableEntryHeaderSize = 8;
            this.m_stack = new Stack();
        }

        public SSLexTable(string q_file)
        {
            this.SSLexTableHeaderSize = 0x24;
            this.SSLexDfaTableHeaderSize = 40;
            this.SSLexDfaKeywordTableHeaderSize = 40;
            this.SSLexDfaClassTableHeaderSize = 12;
            this.SSLexDfaClassTableEntryHeaderSize = 8;
            this.m_classMin = 0x7fffffff;
            this.m_classMax = -134217726;
            this.m_stack = new Stack();
            FileStream stream = File.Open(q_file, FileMode.Open, FileAccess.Read, FileShare.Read);
            byte[] buffer = new byte[0x200];
            int length = stream.Read(buffer, 0, this.SSLexTableHeaderSize);
            SSLexTableHeader header = new SSLexTableHeader {
                size = this.convertInt(buffer, 0),
                type = this.convertInt(buffer, 4)
            };
            for (int i = 0; i < header.reserved.GetLength(0); i++)
            {
                header.reserved[i] = this.convertInt(buffer, (i * 4) + 8);
            }
            this.m_subTables = new SSLexSubtable[header.size];
            for (int j = 0; j < header.size; j++)
            {
                SSLexDfaTableHeader header2 = new SSLexDfaTableHeader();
                length = stream.Read(buffer, 0, this.SSLexDfaTableHeaderSize);
                header2.type = this.convertInt(buffer, 0);
                header2.size = this.convertInt(buffer, 4);
                for (int n = 0; n < header2.reserved.GetLength(0); n++)
                {
                    header2.reserved[n] = this.convertInt(buffer, (n * 4) + 8);
                }
                byte[] buffer2 = new byte[header2.size];
                stream.Read(buffer2, 0, header2.size - this.SSLexDfaTableHeaderSize);
                int num5 = this.convertInt(buffer2, 0);
                this.convertInt(buffer2, 4);
                int num6 = this.convertInt(buffer2, 8);
                int num7 = this.convertInt(buffer2, 12);
                num6 -= this.SSLexDfaTableHeaderSize;
                num7 -= this.SSLexDfaTableHeaderSize;
                int[] numArray = new int[num5 * 3];
                int num8 = 0;
                int offset = num6;
                for (int num10 = 0; num10 < num5; num10++)
                {
                    int num11 = this.convertInt(buffer2, offset) - this.SSLexDfaTableHeaderSize;
                    num8 += (this.convertInt(buffer2, num11) * 3) + 1;
                    offset += 4;
                }
                int[] numArray2 = new int[num8];
                offset = num6;
                int num12 = 0;
                for (int num13 = 0; num13 < num5; num13++)
                {
                    int num14 = this.convertInt(buffer2, offset) - this.SSLexDfaTableHeaderSize;
                    num8 = this.convertInt(buffer2, num14);
                    num14 += 4;
                    numArray2[num12++] = num8;
                    for (int num15 = 0; num15 < num8; num15++)
                    {
                        numArray2[num12++] = this.convertInt(buffer2, num14);
                        num14 += 4;
                        numArray2[num12++] = this.convertInt(buffer2, num14);
                        num14 += 4;
                        numArray2[num12++] = this.convertInt(buffer2, num14);
                        num14 += 4;
                    }
                    offset += 4;
                }
                int num16 = num7;
                int num17 = 0;
                for (int num18 = 0; num18 < num5; num18++)
                {
                    int num19 = num16;
                    numArray[num17++] = this.convertInt(buffer2, num19);
                    num19 += 4;
                    numArray[num17++] = this.convertInt(buffer2, num19);
                    num19 += 4;
                    numArray[num17++] = this.convertInt(buffer2, num19);
                    num16 += 0x1c;
                }
                this.m_subTables[j] = new SSLexSubtable(num5, numArray2, numArray);
            }
            int num20 = header.reserved[0];
            if (num20 > 0)
            {
                this.m_keyTables = new SSLexKeyTable[num20];
            }
            for (int k = 0; k < num20; k++)
            {
                length = stream.Read(buffer, 0, this.SSLexDfaKeywordTableHeaderSize);
                int num22 = this.convertInt(buffer, 0);
                int num23 = this.convertInt(buffer, 4);
                byte[] buffer3 = new byte[num22];
                length = stream.Read(buffer3, 0, num22 - this.SSLexDfaKeywordTableHeaderSize);
                int num24 = 40;
                int[] numArray3 = new int[num23 * 3];
                string[] strArray = new string[num23];
                for (int num25 = 0; num25 < num23; num25++)
                {
                    int index = 3 * num25;
                    numArray3[index] = this.convertInt(buffer3, num24);
                    if (buffer3[num24 + 4] == 0)
                    {
                        numArray3[index + 1] = 0;
                    }
                    else
                    {
                        numArray3[index + 1] = 1;
                    }
                    numArray3[index + 2] = this.convertInt(buffer3, num24 + 13);
                    int startIndex = this.convertInt(buffer3, num24 + 5) - this.SSLexDfaKeywordTableHeaderSize;
                    length = 0;
                    for (int num28 = startIndex; buffer3[num28] != 0; num28++)
                    {
                        length++;
                    }
                    char[] chArray = new char[length];
                    k = 0;
                    while (k < length)
                    {
                        chArray[k] = (char) buffer3[k];
                        k++;
                    }
                    strArray[num25] = new string(chArray, startIndex, length);
                    num24 += 0x29;
                }
                this.m_keyTables[k] = new SSLexKeyTable(numArray3, strArray);
            }
            this.m_classes = header.reserved[1];
            if (this.m_classes != 0)
            {
                this.m_charClassTables = new SSLexCharacterClass[this.m_classes];
            }
            for (int m = 0; m < this.m_classes; m++)
            {
                length = stream.Read(buffer, 0, this.SSLexDfaClassTableHeaderSize);
                int num30 = this.convertInt(buffer, 0);
                int num31 = this.convertInt(buffer, 4);
                int num32 = this.convertInt(buffer, 8);
                int count = num32 * this.SSLexDfaClassTableEntryHeaderSize;
                int num34 = num32 * 2;
                byte[] buffer4 = new byte[count];
                length = stream.Read(buffer4, 0, count);
                int[] numArray4 = new int[num34];
                for (int num35 = 0; num35 < num34; num35++)
                {
                    numArray4[num35] = this.convertInt(buffer4, num35 * 4);
                }
                this.m_charClassTables[m] = new SSLexCharacterClass(num32, num30, num31, numArray4);
                if (num30 < this.m_classMin)
                {
                    this.m_classMin = num30;
                }
                if (num30 > this.m_classMax)
                {
                    this.m_classMax = num30;
                }
                if (num31 < this.m_classMin)
                {
                    this.m_classMax = num31;
                }
                if (num31 > this.m_classMax)
                {
                    this.m_classMax = num31;
                }
            }
            this.pushSubtable(0);
        }

        public int convertInt(byte[] b, int offset)
        {
            long num = (b[offset + 3] << 0x18) & ((long) 0xff000000L);
            long num2 = (b[offset + 2] << 0x10) & 0xff0000;
            long num3 = (b[offset + 1] << 8) & 0xff00;
            long num4 = b[offset];
            return (int) (((ulong) (((num | num2) | num3) | num4)) & 0xffffffffL);
        }

        public void findKeyword(SSLexLexeme z_lexeme)
        {
            int length = 0;
            int num2 = 0;
            length = this.m_keyTables[0].m_keys.Length;
            while (length > num2)
            {
                int num3;
                int index = num2 + ((length - num2) / 2);
                string strB = this.m_keyTables[0].m_keys[index];
                string str2 = new string(z_lexeme.lexeme());
                if (this.m_keyTables[0].m_index[(index * 3) + 1] == 1)
                {
                    num3 = str2.ToLower().CompareTo(strB);
                }
                else
                {
                    num3 = str2.CompareTo(strB);
                }
                if (num3 < 0)
                {
                    if (num2 == index)
                    {
                        return;
                    }
                    length = index;
                }
                else
                {
                    if (num3 == 0)
                    {
                        z_lexeme.setToken(this.m_keyTables[0].m_index[index * 3]);
                        return;
                    }
                    if (length == (index + 1))
                    {
                        return;
                    }
                    num2 = index;
                }
            }
        }

        public void gotoSubtable(int q_index)
        {
            this.m_stack.Pop();
            this.m_stack.Push(this.m_subTables[q_index]);
        }

        public int lookup(int q_state, int q_next)
        {
            SSLexSubtable subtable = (SSLexSubtable) this.m_stack.Peek();
            return subtable.lookup(q_state, q_next);
        }

        public SSLexFinalState lookupFinal(int q_state)
        {
            SSLexSubtable subtable = (SSLexSubtable) this.m_stack.Peek();
            return subtable.lookupFinal(q_state);
        }

        public void popSubtable()
        {
            this.m_stack.Pop();
        }

        public void pushSubtable(int q_index)
        {
            this.m_stack.Push(this.m_subTables[q_index]);
        }

        public bool translateClass(char[] q_char)
        {
            char ch = q_char[0];
            if ((ch >= this.m_classMin) && (ch <= this.m_classMax))
            {
                for (int i = 0; i < this.m_classes; i++)
                {
                    if (this.m_charClassTables[i].translate(q_char))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}

