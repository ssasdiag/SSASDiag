namespace SSVParseLib
{
    using System;
    using System.Collections;

    public class SSYaccSet : Queue
    {
        public bool add(object q_object)
        {
            if (!this.Contains(q_object))
            {
                this.Enqueue(q_object);
                return true;
            }
            return false;
        }

        public bool locate(int q_locate)
        {
            return this.Contains(q_locate);
        }
    }
}

