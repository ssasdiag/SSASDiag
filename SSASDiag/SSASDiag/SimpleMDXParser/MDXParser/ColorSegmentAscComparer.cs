namespace SimpleMDXParser
{
    using System;
    using System.Collections.Generic;

    internal class ColorSegmentAscComparer : IComparer<ColorSegment>
    {
        int IComparer<ColorSegment>.Compare(ColorSegment x, ColorSegment y)
        {
            return (x.Start - y.Start);
        }
    }
}

