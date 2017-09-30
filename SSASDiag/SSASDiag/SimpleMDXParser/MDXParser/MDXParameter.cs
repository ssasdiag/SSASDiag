namespace SimpleMDXParser
{
    using System;

    public class MDXParameter
    {
        internal string m_Name;
        internal string m_Value;

        internal MDXParameter(string name, string value)
        {
            this.m_Name = name;
            this.m_Value = value;
        }

        public string Name
        {
            get
            {
                return this.m_Name;
            }
        }

        public string Value
        {
            get
            {
                return this.m_Value;
            }
        }
    }
}

