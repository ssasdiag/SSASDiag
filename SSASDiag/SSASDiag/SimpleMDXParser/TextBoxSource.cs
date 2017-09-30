using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using SimpleMDXParser;


using System.Reflection;
using System.Collections;

namespace SimpleMDXParser
{
    public class WigglyLine
    {
        // Fields
        internal int EndChar;
        internal int StartChar;

        // Methods
        internal WigglyLine(int pos, int len)
        {
            this.StartChar = pos;
            this.EndChar = this.StartChar + len;
        }
    }

    public class TextBoxAPIHelper
    {
        // Fields
        private const double anInch = 14.4;

        // Methods
        private TextBoxAPIHelper()
        {
        }

        internal static int CharFromPos(TextBoxBase txt, Point pt)
        {
            int num = (pt.X & 0xffff) + ((pt.Y & 0xffff) << 0x10);
            int num2 = NativeMethods.SendMessageInt(txt.Handle, 0xd7, IntPtr.Zero, new IntPtr(num)).ToInt32();
            int num3 = (num2 & 0xffff) >> 0x10;
            int num4 = num2 & 0xffff;
            return (NativeMethods.SendMessageInt(txt.Handle, 0xbb, new IntPtr(num3), IntPtr.Zero).ToInt32() + num4);
        }

        internal static int GetBaselineOffsetAtCharIndex(TextBoxBase tb, int index)
        {
            NativeMethods.CHARRANGE charrange;
            NativeMethods.RECT rect;
            NativeMethods.RECT rect2;
            NativeMethods.FORMATRANGE formatrange;
            RichTextBox box = tb as RichTextBox;
            if (box == null)
            {
                return tb.Font.Height;
            }
            int lineFromCharIndex = box.GetLineFromCharIndex(index);
            int num2 = NativeMethods.SendMessageInt(box.Handle, 0xbb, new IntPtr(lineFromCharIndex), IntPtr.Zero).ToInt32();
            int num3 = NativeMethods.SendMessageInt(box.Handle, 0xc1, new IntPtr(lineFromCharIndex), IntPtr.Zero).ToInt32();
            charrange.cpMin = num2;
            charrange.cpMax = num2 + num3;
            rect.Top = 0;
            rect.Bottom = 14;
            rect.Left = 0;
            rect.Right = 0x989680;
            rect2.Top = 0;
            rect2.Bottom = 14;
            rect2.Left = 0;
            rect2.Right = 0x989680;
            Graphics graphics = Graphics.FromHwnd(box.Handle);
            IntPtr hdc = graphics.GetHdc();
            formatrange.chrg = charrange;
            formatrange.hdc = hdc;
            formatrange.hdcTarget = hdc;
            formatrange.rc = rect;
            formatrange.rcPage = rect2;
            NativeMethods.SendMessage(box.Handle, 0x439, IntPtr.Zero, ref formatrange).ToInt32();
            graphics.ReleaseHdc(hdc);
            graphics.Dispose();
            return (int)(((double)(formatrange.rc.Bottom - formatrange.rc.Top)) / 14.4);
        }

        internal static int GetFirstVisibleLine(TextBoxBase txt)
        {
            return NativeMethods.SendMessageInt(txt.Handle, 0xce, IntPtr.Zero, IntPtr.Zero).ToInt32();
        }

        internal static int GetLineIndex(TextBoxBase txt, int line)
        {
            return NativeMethods.SendMessageInt(txt.Handle, 0xbb, new IntPtr(line), IntPtr.Zero).ToInt32();
        }

        internal static int GetTextWidthAtCharIndex(TextBoxBase tb, int index, int length)
        {
            NativeMethods.CHARRANGE charrange;
            NativeMethods.RECT rect;
            NativeMethods.RECT rect2;
            NativeMethods.FORMATRANGE formatrange;
            RichTextBox box = tb as RichTextBox;
            if (box == null)
            {
                return tb.Font.Height;
            }
            charrange.cpMin = index;
            charrange.cpMax = index + length;
            rect.Top = 0;
            rect.Bottom = 14;
            rect.Left = 0;
            rect.Right = (int)(box.ClientSize.Width * 14.4);
            rect2.Top = 0;
            rect2.Bottom = 14;
            rect2.Left = 0;
            rect2.Right = (int)(box.ClientSize.Width * 14.4);
            Graphics graphics = Graphics.FromHwnd(box.Handle);
            IntPtr hdc = graphics.GetHdc();
            formatrange.chrg = charrange;
            formatrange.hdc = hdc;
            formatrange.hdcTarget = hdc;
            formatrange.rc = rect;
            formatrange.rcPage = rect2;
            NativeMethods.SendMessage(box.Handle, 0x439, IntPtr.Zero, ref formatrange);
            graphics.ReleaseHdc(hdc);
            graphics.Dispose();
            return (int)(((double)(formatrange.rc.Right - formatrange.rc.Left)) / 14.4);
        }

        internal static Point PosFromChar(TextBoxBase txt, int charIndex)
        {
            return new Point(NativeMethods.SendMessageInt(txt.Handle, 0xd6, new IntPtr(charIndex), IntPtr.Zero).ToInt32());
        }

        internal static void ScrollLineDown(TextBoxBase txt)
        {
            NativeMethods.SendMessageInt(txt.Handle, 0xb5, (IntPtr)1, IntPtr.Zero).ToInt32();
        }

        internal static void ScrollLineUp(TextBoxBase txt)
        {
            NativeMethods.SendMessageInt(txt.Handle, 0xb5, IntPtr.Zero, IntPtr.Zero).ToInt32();
        }

        internal static void ShowCaret(TextBox txt)
        {
            bool flag = false;
            for (int i = 0; !flag && (i < 10); i++)
            {
                flag = NativeMethods.ShowCaretAPI(txt.Handle);
            }
        }
    }


    public class WigglyLines : List<WigglyLine>
    {
    }


    public class WigglyLinesPainter : NativeWindow
    {
        // Fields
        private Bitmap bitmap;
        private Graphics bufferGraphics;
        public WigglyLines m_WigglyLines = new WigglyLines();
        private RichTextBox parentTextBox;
        private Graphics textBoxGraphics;

        // Methods
        internal WigglyLinesPainter(RichTextBox textBox)
        {
            this.parentTextBox = textBox;
            this.bitmap = new Bitmap(textBox.Width, textBox.Height);
            this.bufferGraphics = Graphics.FromImage(this.bitmap);
            this.bufferGraphics.Clip = new Region(textBox.ClientRectangle);
            this.textBoxGraphics = Graphics.FromHwnd(textBox.Handle);
            base.AssignHandle(this.parentTextBox.Handle);
        }

        internal void AddWigglyLine(WigglyLine wl)
        {
            if (this.m_WigglyLines != null)
            {
                this.m_WigglyLines.Add(wl);
            }
        }

        internal void ClearWigglyLines()
        {
            if (this.m_WigglyLines != null)
            {
                this.m_WigglyLines.Clear();
            }
        }

        private void CustomPaint()
        {
            int firstVisibleLine = TextBoxAPIHelper.GetFirstVisibleLine(this.parentTextBox);
            firstVisibleLine = TextBoxAPIHelper.GetLineIndex(this.parentTextBox, firstVisibleLine);
            this.bufferGraphics.Clear(Color.Transparent);
            foreach (WigglyLine line in this.m_WigglyLines)
            {
                Point start = TextBoxAPIHelper.PosFromChar(this.parentTextBox, line.StartChar);
                Point end = TextBoxAPIHelper.PosFromChar(this.parentTextBox, line.EndChar);
                end.X++;
                start.Y += TextBoxAPIHelper.GetBaselineOffsetAtCharIndex(this.parentTextBox, line.StartChar);
                end.Y += TextBoxAPIHelper.GetBaselineOffsetAtCharIndex(this.parentTextBox, line.EndChar);
                this.DrawWave(start, end);
            }
            this.textBoxGraphics.DrawImageUnscaled(this.bitmap, 0, 0);
        }

        private void DrawWave(Point start, Point end)
        {
            Pen red = Pens.Red;
            if ((end.X - start.X) > 4)
            {
                ArrayList list = new ArrayList();
                for (int i = start.X; i <= (end.X - 2); i += 4)
                {
                    list.Add(new Point(i, start.Y));
                    list.Add(new Point(i + 2, start.Y + 2));
                }
                Point[] points = (Point[])list.ToArray(typeof(Point));
                this.bufferGraphics.DrawLines(red, points);
            }
            else
            {
                this.bufferGraphics.DrawLine(red, start, end);
            }
        }

        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            if ((this.m_WigglyLines == null) || (this.m_WigglyLines.Count == 0))
            {
                base.WndProc(ref m);
            }
            else if (m.Msg == 15)
            {
                this.parentTextBox.Invalidate();
                base.WndProc(ref m);
                this.CustomPaint();
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        // Properties
        internal WigglyLines WigglyLines
        {
            get
            {
                return this.m_WigglyLines;
            }
            set
            {
                this.m_WigglyLines = value;
            }
        }
    }


    public class TextBoxSource : Source
    {
        // Fields
        protected RichTextBox m_TextBox;

        // Methods
        public TextBoxSource(RichTextBox tb, Locator l)
        {
            this.m_TextBox = tb;
            WigglyLinesPainter wp = new WigglyLinesPainter(m_TextBox);
            wp.ClearWigglyLines();
            m_TextBox.Tag = wp;


            base.StartLocation = l;
        }

        public override Source Clone()
        {
            return new TextBoxSource(this.m_TextBox, base.StartLocation);
        }

        public void ClearWigglyLines()
        {
            (m_TextBox.Tag as WigglyLinesPainter).ClearWigglyLines();
        }

        public override void DrawWigglyLine(int pos, int len)
        {
            (this.m_TextBox.Tag as WigglyLinesPainter).AddWigglyLine(new WigglyLine(pos, len));
        }

        public override void SetColor(int pos, int len, Color color)
        {
            this.m_TextBox.SelectionStart = base.StartLocation.Position + pos;
            this.m_TextBox.SelectionLength = len;
            this.m_TextBox.SelectionColor = color;
        }

        // Properties
        internal RichTextBox TextBox
        {
            get
            {
                return this.m_TextBox;
            }
        }
    }
}
