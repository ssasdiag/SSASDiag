namespace SimpleMDXParser
{
    using System;
    using System.Collections.Generic;

    internal class ColorSegmentDescComparer : IComparer<ColorSegment>
    {
        int IComparer<ColorSegment>.Compare(ColorSegment x, ColorSegment y)
        {
            return (y.Start - x.Start);
        }
    }
}

