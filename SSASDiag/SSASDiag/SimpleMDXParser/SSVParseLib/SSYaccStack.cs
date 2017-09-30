namespace SSVParseLib
{
    using System;
    using System.Collections;

    public class SSYaccStack : ArrayList
    {
        public int m_size = 0;

        public SSYaccStack(int q_size, int q_inc)
        {
        }

        public object elementAt(int index)
        {
            return this[index];
        }

        public int getSize()
        {
            return this.Count;
        }

        public object peek()
        {
            return this.elementAt(this.m_size - 1);
        }

        public void pop()
        {
            this.m_size--;
            this.RemoveAt(this.m_size);
        }

        public void push(object q_ele)
        {
            this.Add(q_ele);
            this.m_size++;
        }
    }
}

