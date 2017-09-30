namespace SimpleMDXParser
{
    using System;

    public class MDXObject
    {
        private string m_CanonicalName;
        private MDXFunctionOpt m_Opt;
        private MDXDataType m_ReturnType;
        private MDXSyntaxForm m_SyntaxForm;
        private string m_SyntaxTip;
        private MDXDataType m_ThisType;

        internal MDXObject(string name, MDXSyntaxForm form, MDXDataType type)
        {
            this.Init(name, form, type, MDXFunctionOpt.Normal, null, MDXDataType.Unknown);
        }

        internal MDXObject(string name, MDXSyntaxForm form, MDXDataType type, MDXDataType thisType)
        {
            this.Init(name, form, type, MDXFunctionOpt.Normal, null, thisType);
        }

        internal MDXObject(string name, MDXSyntaxForm form, MDXDataType type, MDXFunctionOpt opt)
        {
            this.Init(name, form, type, opt, null, MDXDataType.Unknown);
        }

        internal MDXObject(string name, MDXSyntaxForm form, MDXDataType type, string syntaxTip)
        {
            this.Init(name, form, type, MDXFunctionOpt.Normal, syntaxTip, MDXDataType.Unknown);
        }

        internal MDXObject(string name, MDXSyntaxForm form, MDXDataType type, MDXFunctionOpt opt, MDXDataType thisType)
        {
            this.Init(name, form, type, opt, null, thisType);
        }

        internal MDXObject(string name, MDXSyntaxForm form, MDXDataType type, MDXFunctionOpt opt, string syntaxTip)
        {
            this.Init(name, form, type, opt, syntaxTip, MDXDataType.Unknown);
        }

        internal MDXObject(string name, MDXSyntaxForm form, MDXDataType type, string syntaxTip, MDXDataType thisType)
        {
            this.Init(name, form, type, MDXFunctionOpt.Normal, syntaxTip, thisType);
        }

        internal MDXObject(string name, MDXSyntaxForm form, MDXDataType type, MDXFunctionOpt opt, string syntaxTip, MDXDataType thisType)
        {
            this.Init(name, form, type, opt, syntaxTip, thisType);
        }

        internal void Init(string name, MDXSyntaxForm form, MDXDataType type, MDXFunctionOpt opt, string syntaxTip, MDXDataType thisType)
        {
            this.m_CanonicalName = name;
            this.m_SyntaxForm = form;
            this.m_ReturnType = type;
            this.m_Opt = opt;
            this.m_SyntaxTip = syntaxTip;
            this.m_ThisType = thisType;
        }

        public string CanonicalName
        {
            get
            {
                return this.m_CanonicalName;
            }
        }

        public MDXFunctionOpt Optimization
        {
            get
            {
                return this.m_Opt;
            }
        }

        public MDXDataType ReturnType
        {
            get
            {
                return this.m_ReturnType;
            }
        }

        public MDXSyntaxForm SyntaxForm
        {
            get
            {
                return this.m_SyntaxForm;
            }
        }

        public string SyntaxTip
        {
            get
            {
                return this.m_SyntaxTip;
            }
        }

        public MDXDataType ThisType
        {
            get
            {
                return this.m_ThisType;
            }
        }
    }
}

