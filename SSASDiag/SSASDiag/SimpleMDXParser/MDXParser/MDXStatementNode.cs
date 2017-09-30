namespace SimpleMDXParser
{
    using System;

    public abstract class MDXStatementNode : MDXNode
    {
        protected MDXStatementNode()
        {
        }

        internal override MDXConstruct GetConstruct()
        {
            return MDXConstruct.Statement;
        }
    }
}

