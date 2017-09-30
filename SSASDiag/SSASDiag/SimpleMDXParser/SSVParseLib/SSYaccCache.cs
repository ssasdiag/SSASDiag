namespace SSVParseLib
{
    using System;
    using System.Collections;

    public class SSYaccCache : Queue
    {
        public bool hasElements()
        {
            return (this.Count != 0);
        }

        public SSLexLexeme remove()
        {
            SSLexLexeme lexeme = null;
            if (this.Count != 0)
            {
                lexeme = (SSLexLexeme) this.Dequeue();
            }
            return lexeme;
        }
    }
}

