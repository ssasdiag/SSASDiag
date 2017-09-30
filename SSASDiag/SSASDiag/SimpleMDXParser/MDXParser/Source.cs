namespace SimpleMDXParser
{
    using System;
    using System.Drawing;

    public class Source
    {
        private Locator m_StartLocation = new Locator();

        public virtual Source Clone()
        {
            return new Source { m_StartLocation = this.m_StartLocation };
        }

        public virtual void DrawWigglyLine(int pos, int len)
        {
        }

        public virtual void SetColor(int pos, int len, Color color)
        {
        }

        public Locator StartLocation
        {
            get
            {
                return this.m_StartLocation;
            }
            set
            {
                this.m_StartLocation = value;
            }
        }
    }
}

