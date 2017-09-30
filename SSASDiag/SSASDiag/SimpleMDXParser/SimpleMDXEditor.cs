using System;
using System.Windows.Forms;

namespace SimpleMDXParser
{
    public class SimpleMDXEditor : RichTextBox
    {

        private bool ModifiedByPaste = true;
        private TextBoxSource tbs;

        protected override void OnCreateControl()
        {
            tbs = new TextBoxSource(this, new Locator());
 	         base.OnCreateControl();
        }

        protected override void OnTextChanged(EventArgs e)
        {

            if (ModifiedByPaste)
            {
                SuspendLayout();
                ModifiedByPaste = false;
                ParseAndFormat();
            }
            else
            {
                ResumeLayout();
                ModifiedByPaste = true;
            }
            base.OnTextChanged(e);
        }

        public void ParseAndFormat()
        {
            int pos = SelectionStart;
                
            tbs.ClearWigglyLines();

            MDXParser p = new MDXParser(Text, tbs, new CubeInfo());
            FormatOptions fo = new FormatOptions();
            fo.Indent = 3;
            fo.Output = OutputFormat.RTF;
            try
            {
                p.Parse();
                string s = p.FormatMDX(fo);
                s = s.Substring(0, s.IndexOf("\\par")) + s.Substring(s.IndexOf("\\par") + 4);
                Rtf = s;

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                try
                {
                    p.Parse();
                    string s = p.FormatMDX(fo);
                    s = s.Substring(0, s.IndexOf("\\par")) + s.Substring(s.IndexOf("\\par") + 4);
                    Rtf = s;
                }
                catch (MDXParserException mdxex)
                {
                    System.Diagnostics.Debug.WriteLine(mdxex.Message);
                    try
                    {
                        p.Parse();
                        string s = p.FormatMDX(fo);
                        s = s.Substring(0, s.IndexOf("\\par")) + s.Substring(s.IndexOf("\\par") + 4);
                        Rtf = s;
                    }
                    catch (MDXParserException mdxex2)
                    {
                        Select(pos, 0);
                        System.Diagnostics.Debug.WriteLine(mdxex2.Message);
                    }
                    catch (Exception ex3)
                    {
                        Select(pos, 0);
                        System.Diagnostics.Debug.WriteLine(ex3.Message);
                    }
                }
                catch (Exception ex2)
                {
                    System.Diagnostics.Debug.WriteLine(ex2.Message);
                    try
                    {
                        p.Parse();
                        string s = p.FormatMDX(fo);
                        s = s.Substring(0, s.IndexOf("\\par")) + s.Substring(s.IndexOf("\\par") + 4);
                        Rtf = s;
                    }
                    catch (MDXParserException mdxex2)
                    {
                        Select(pos, 0);
                        System.Diagnostics.Debug.WriteLine(mdxex2.Message);
                    }
                    catch (Exception ex3)
                    {
                        Select(pos, 0);
                        System.Diagnostics.Debug.WriteLine(ex3.Message);
                    }
                }

            }

            Select(pos, 0);
        }
    }
}
