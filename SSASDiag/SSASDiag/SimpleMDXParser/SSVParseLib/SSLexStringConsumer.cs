namespace SSVParseLib
{
    using System;

    public class SSLexStringConsumer : SSLexConsumer
    {
        public char[] m_string;

        public SSLexStringConsumer(string q_string)
        {
            this.m_string = q_string.ToCharArray();
            base.m_length = this.m_string.Length;
        }

        public override bool getNext()
        {
            bool flag = false;
            if (base.m_index < this.m_string.Length)
            {
                flag = true;
                base.m_current = this.m_string[base.m_index];
            }
            return flag;
        }
    }
}

