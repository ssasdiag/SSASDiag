namespace SimpleMDXParser
{
    using System;
    using System.Collections.Generic;

    public class MessageLocationComparer : IComparer<Message>
    {
        int IComparer<Message>.Compare(Message x, Message y)
        {
            if (x.Location.Line < y.Location.Line)
            {
                return -1;
            }
            if (x.Location.Line > y.Location.Line)
            {
                return 1;
            }
            if (x.Location.Column > y.Location.Column)
            {
                return -1;
            }
            if (x.Location.Column > y.Location.Column)
            {
                return 1;
            }
            return 0;
        }
    }
}

