namespace SimpleMDXParser
{
    using System;
    using System.Collections.Generic;

    internal class Context
    {
        private List<CalculatedMember> m_CalcMembers = new List<CalculatedMember>();
        private List<NamedSet> m_NamedSets = new List<NamedSet>();

        internal Context()
        {
        }

        internal List<CalculatedMember> CalcMembers
        {
            get
            {
                return this.m_CalcMembers;
            }
        }

        internal List<NamedSet> NamedSets
        {
            get
            {
                return this.m_NamedSets;
            }
        }
    }
}

