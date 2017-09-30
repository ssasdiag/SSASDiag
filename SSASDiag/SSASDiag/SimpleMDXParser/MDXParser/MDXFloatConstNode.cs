namespace SimpleMDXParser
{
    using System;
    using System.Globalization;

    internal class MDXFloatConstNode : MDXConstNode
    {
        private double m_Double;

        internal MDXFloatConstNode(string LexString)
        {
            this.m_Double = double.Parse(LexString, NumberFormatInfo.InvariantInfo);
        }

        public override string GetLabel()
        {
            NumberFormatInfo numberFormat = CultureInfo.InvariantCulture.NumberFormat;
            return this.m_Double.ToString(numberFormat);
        }
    }
}

