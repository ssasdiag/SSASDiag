namespace SimpleMDXParser
{
    using SSVParseLib;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Text;

    public class ColorCoding : List<ColorSegment>
    {
        public void AdjustOffsets(int start, int offset)
        {
            foreach (ColorSegment segment in this)
            {
                if (segment.Start <= start)
                {
                    if (segment.End > start)
                    {
                        segment.End += offset;
                    }
                }
                else
                {
                    segment.Start += offset;
                }
            }
        }

        internal void ApplyColor(SSLexLexeme lexeme, Color color)
        {
            ColorSegment item = new ColorSegment {
                Start = (lexeme.index() - lexeme.length()) - 1,
                Length = lexeme.length(),
                Color = color
            };
            base.Add(item);
        }

        public string ConvertToRTF(string text)
        {
            foreach (char ch in text.ToCharArray())
            {
                if (ch > '\x007f')
                {
                    return null;
                }
            }
            StringBuilder builder = new StringBuilder();
            builder.Append("{\\rtf1\\ansi\\ansicpg1252\\deff0\\deftab720{\\fonttbl{\\f0\\fmodern\\fprq2\\fcharset204 Courier New;}}\r\n{\\colortbl ;\\red0\\green0\\blue255;\\red0\\green128\\blue0;\\red255\\green0\\blue0;\\red192\\green192\\blue192;\\red0\\green0\\blue128;}{\\f1\\fs20 ");
            base.Sort(new ColorSegmentAscComparer());
            int startIndex = 0;
            foreach (ColorSegment segment in this)
            {
                if (startIndex < segment.Start)
                {
                    builder.Append(ToRTF(text.Substring(startIndex, segment.Start - startIndex)));
                }
                if (Color.Blue == segment.Color)
                {
                    builder.AppendFormat(@"\cf1 {0}\cf0 ", ToRTF(text.Substring(segment.Start, segment.Length)));
                }
                else if (Color.Green == segment.Color)
                {
                    builder.AppendFormat(@"\cf2 {0}\cf0 ", ToRTF(text.Substring(segment.Start, segment.Length)));
                }
                else if (Color.Red == segment.Color)
                {
                    builder.AppendFormat(@"\cf3 {0}\cf0 ", ToRTF(text.Substring(segment.Start, segment.Length)));
                }
                else if (Color.Silver == segment.Color)
                {
                    builder.AppendFormat(@"\cf4 {0}\cf0 ", ToRTF(text.Substring(segment.Start, segment.Length)));
                }
                else if (Color.Maroon == segment.Color)
                {
                    builder.AppendFormat(@"\cf5 {0}\cf0 ", ToRTF(text.Substring(segment.Start, segment.Length)));
                }
                else
                {
                    builder.Append(ToRTF(text.Substring(segment.Start, segment.Length)));
                }
                startIndex = segment.End + 1;
            }
            int length = text.Length;
            if (startIndex < length)
            {
                builder.Append(ToRTF(text.Substring(startIndex, length - startIndex)));
            }
            builder.Append("}");
            return builder.ToString();
        }

        public ColorSegment FindSegment(int pos, int end)
        {
            bool flag = pos == end;
            foreach (ColorSegment segment in this)
            {
                if ((segment.Start <= pos) && (segment.End >= pos))
                {
                    return segment;
                }
                if (flag && ((segment.End + 1) == pos))
                {
                    return segment;
                }
            }
            return null;
        }

        public ColorCoding GetDelta(ColorCoding cc)
        {
            if (cc == null)
            {
                return this;
            }
            ColorCoding coding = new ColorCoding();
            Stack<ColorSegment> segmentsStack = this.GetSegmentsStack();
            Stack<ColorSegment> stack2 = cc.GetSegmentsStack();
            while ((segmentsStack.Count > 0) || (stack2.Count > 0))
            {
                ColorSegment item = null;
                ColorSegment segment2 = null;
                if (segmentsStack.Count == 0)
                {
                    ColorSegment segment3 = new ColorSegment {
                        Color = Color.Black
                    };
                    segment2 = stack2.Pop();
                    segment3.Start = segment2.Start;
                    while (stack2.Count > 0)
                    {
                        segment2 = stack2.Pop();
                    }
                    segment3.End = segment2.End;
                    coding.Add(segment3);
                    return coding;
                }
                if (stack2.Count == 0)
                {
                    while (segmentsStack.Count > 0)
                    {
                        item = segmentsStack.Pop();
                        coding.Add(item);
                    }
                    return coding;
                }
                item = segmentsStack.Peek();
                segment2 = stack2.Peek();
                if ((item.Start == segment2.Start) && (item.End == segment2.End))
                {
                    segmentsStack.Pop();
                    stack2.Pop();
                }
                else
                {
                    if (item.Start <= segment2.Start)
                    {
                        segmentsStack.Pop();
                        coding.Add(item);
                        while ((segment2.End <= item.End) && (stack2.Count > 0))
                        {
                            segment2 = stack2.Pop();
                        }
                        if (segment2.Start > item.End)
                        {
                            stack2.Push(segment2);
                        }
                        else
                        {
                            ColorSegment segment4 = new ColorSegment {
                                Start = item.End + 1,
                                End = segment2.End
                            };
                            stack2.Push(segment4);
                        }
                        continue;
                    }
                    if (item.Start > segment2.Start)
                    {
                        stack2.Pop();
                        ColorSegment segment5 = new ColorSegment {
                            Start = segment2.Start,
                            Color = Color.Black
                        };
                        if (segment2.End < item.End)
                        {
                            segment5.End = segment2.End;
                        }
                        else
                        {
                            segment5.End = item.Start - 1;
                            ColorSegment segment6 = new ColorSegment {
                                Start = item.Start,
                                End = segment2.End
                            };
                            stack2.Push(segment6);
                        }
                        coding.Add(segment5);
                    }
                }
            }
            return coding;
        }

        private Stack<ColorSegment> GetSegmentsStack()
        {
            List<ColorSegment> list = new List<ColorSegment>(base.Count);
            list = this;
            list.Sort(new ColorSegmentDescComparer());
            Stack<ColorSegment> stack = new Stack<ColorSegment>(base.Count);
            foreach (ColorSegment segment in list)
            {
                stack.Push(segment);
            }
            return stack;
        }

        private static string ToRTF(string str)
        {
            return str.Replace(@"\", @"\\").Replace("\n", "\\par\n").Replace("{", @"\{").Replace("}", @"\}");
        }
    }
}

