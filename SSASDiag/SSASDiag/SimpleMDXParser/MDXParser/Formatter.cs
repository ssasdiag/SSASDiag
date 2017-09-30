namespace SimpleMDXParser
{
    using System;
    using System.Text;
    using System.Linq;

    internal class Formatter
    {
        private FormatOptions m_Options = new FormatOptions();

        internal Formatter()
        {
        }

        internal void AppendCommaAtTheEndOfLine(StringBuilder sb, bool comma)
        {
            if (comma && this.m_Options.CommaBeforeNewLine)
            {
                sb.Append(",");
            }
        }

        internal void AppendIndent(StringBuilder sb, int indent)
        {
            this.AppendIndent(sb, indent, false);
        }

        internal void AppendIndent(StringBuilder sb, int indent, bool comma)
        {
            if (indent >= 0)
            {
                sb.Append(this.Newline());
                if (comma && !this.m_Options.CommaBeforeNewLine)
                {
                    indent--;
                }
                for (int i = 0; i < indent; i++)
                {
                    sb.Append(this.Space());
                }
            }
            if (comma && !this.m_Options.CommaBeforeNewLine)
            {
                sb.Append(",");
            }
        }

        internal string CloseBrace()
        {
            if (this.m_Options.Output == OutputFormat.RTF)
            {
                return @"\}";
            }
            return "}";
        }

        internal string Comment(string comment)
        {
            comment = comment.Replace("\n", this.Newline());
            if (this.m_Options.Output == OutputFormat.HTML)
            {
                return string.Format("<span style=\"color:green\">{0}</span>", comment);
            }
            if (this.m_Options.Output == OutputFormat.RTF)
            {
                return string.Format(@"\cf2 {0}\cf0 ", comment);
            }
            return comment;
        }

        internal int Indent(int indent)
        {
            if (indent < 0)
            {
                return indent;
            }
            return (indent + this.m_Options.Indent);
        }

        internal string Keyword(string kwd)
        {
            if (this.m_Options.Output == OutputFormat.HTML)
            {
                return string.Format("<span style=\"color:blue\">{0}</span>", kwd);
            }
            if (this.m_Options.Output == OutputFormat.RTF)
            {

                if (MDXParserObjects.s_Objects.Where(ob => ob.CanonicalName.ToUpper() == kwd.ToUpper()).Count() > 0)
                {
                    MDXObject o = MDXParserObjects.s_Objects.Where(ob => ob.CanonicalName.ToUpper() == kwd.ToUpper()).First();
                    if (o.SyntaxForm == MDXSyntaxForm.Function || o.ThisType == MDXDataType.Unknown)
                        return string.Format(@"\cf4 {0}\cf0 ", kwd);
                    else
                        return string.Format(@"\cf1 {0}\cf0 ", kwd);
                }
                else
                    return string.Format(@"\cf1 {0}\cf0 ", kwd);
            }
            return kwd;
        }

        internal string Highlight(string kwd)
        {
            if (this.m_Options.Output == OutputFormat.HTML)
            {
                return string.Format("<span style=\"bgcolor:yellow\">{0}</span>", kwd);
            }
            if (this.m_Options.Output == OutputFormat.RTF)
            {

                return string.Format(@"\highlight5 {0}\highlight0 ", kwd);
            }
            return kwd;
        }

        internal string Newline()
        {
            if (this.m_Options.Output == OutputFormat.HTML)
            {
                return "<br/>";
            }
            if (this.m_Options.Output == OutputFormat.RTF)
            {
                return "\\par\n";
            }
            return "\n";
        }

        internal string OpenBrace()
        {
            if (this.m_Options.Output == OutputFormat.RTF)
            {
                return @"\{";
            }
            return "{";
        }

        internal string Postfix()
        {
            if (this.m_Options.Output == OutputFormat.HTML)
            {
                return "</span>";
            }
            if (this.m_Options.Output == OutputFormat.RTF)
            {
                return "}";
            }
            return "";
        }

        internal string Prefix()
        {
            if (this.m_Options.Output == OutputFormat.HTML)
            {
                return "<span style=\"font:10pt Courier New\">";
            }
            if (this.m_Options.Output == OutputFormat.RTF)
            {
                return "{\\rtf1\\ansi\\ansicpg1252\\deff0\\deflang1033{\\fonttbl{\\f0\\fmodern\\fprq1\\fcharset0 Consolas;}}\r\n{\\colortbl ;\\red0\\green0\\blue255;\\red0\\green128\\blue0;\\red192\\green0\\blue0;\\red128\\green0\\blue0;\\red255\\green255\\blue0;}{\\f1\\fs20 ";
            }
            return "";
        }

        internal string Space()
        {
            if (this.m_Options.Output == OutputFormat.HTML)
            {
                return "&#160;";
            }
            return " ";
        }

        internal string StringConst(string str)
        {
            if (this.m_Options.Output == OutputFormat.HTML)
            {
                return string.Format("<span style=\"color:red\">{0}</span>", str);
            }
            if (this.m_Options.Output == OutputFormat.RTF)
            {
                return string.Format(@"\cf3 {0}\cf0 ", str);
            }
            return str;
        }

        internal FormatOptions Options
        {
            get
            {
                return this.m_Options;
            }
            set
            {
                this.m_Options = value;
            }
        }
    }
}

