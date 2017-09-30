namespace SimpleMDXParser
{
    using System;

    internal class NamedObject : CalcObject
    {
        private string m_Name;

        internal string Name
        {
            get
            {
                return this.m_Name;
            }
            set
            {
                this.m_Name = value;
            }
        }
    }
}

