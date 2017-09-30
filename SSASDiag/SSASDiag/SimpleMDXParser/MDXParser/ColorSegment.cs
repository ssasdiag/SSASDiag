namespace SimpleMDXParser
{
    using System;
    using System.Drawing;

    public class ColorSegment
    {
        private System.Drawing.Color m_Color;
        private int m_Length;
        private int m_Start;

        public System.Drawing.Color Color
        {
            get
            {
                return this.m_Color;
            }
            set
            {
                this.m_Color = value;
            }
        }

        public int End
        {
            get
            {
                return ((this.m_Start + this.m_Length) - 1);
            }
            set
            {
                this.m_Length = (value - this.m_Start) + 1;
            }
        }

        public int Length
        {
            get
            {
                return this.m_Length;
            }
            set
            {
                this.m_Length = value;
            }
        }

        public int Start
        {
            get
            {
                return this.m_Start;
            }
            set
            {
                this.m_Start = value;
            }
        }
    }
}

