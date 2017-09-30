namespace SimpleMDXParser
{
    using System;

    public class FormatOptions
    {
        private bool m_ColorFunctionNames;
        private bool m_CommaAfterNewLine;
        private int m_Indent;
        private int m_LineWidth;
        private OutputFormat m_Output;

        public FormatOptions()
        {
            this.m_Indent = 2;
            this.m_LineWidth = 80;
            this.m_CommaAfterNewLine = true;
            this.m_Output = OutputFormat.Text;
            this.m_ColorFunctionNames = true;
        }

        public FormatOptions(FormatOptions fo)
        {
            this.m_Indent = fo.m_Indent;
            this.m_LineWidth = fo.m_LineWidth;
            this.m_CommaAfterNewLine = fo.m_CommaAfterNewLine;
            this.m_Output = OutputFormat.Text;
            this.m_ColorFunctionNames = fo.m_ColorFunctionNames;
        }

        public bool ColorFunctionNames
        {
            get
            {
                return this.m_ColorFunctionNames;
            }
            set
            {
                this.m_ColorFunctionNames = value;
            }
        }

        public bool CommaBeforeNewLine
        {
            get
            {
                return !this.m_CommaAfterNewLine;
            }
            set
            {
                this.m_CommaAfterNewLine = !value;
            }
        }

        public int Indent
        {
            get
            {
                return this.m_Indent;
            }
            set
            {
                this.m_Indent = value;
            }
        }

        public int LineWidth
        {
            get
            {
                return this.m_LineWidth;
            }
            set
            {
                this.m_LineWidth = value;
            }
        }

        public OutputFormat Output
        {
            get
            {
                return this.m_Output;
            }
            set
            {
                this.m_Output = value;
            }
        }
    }
}

