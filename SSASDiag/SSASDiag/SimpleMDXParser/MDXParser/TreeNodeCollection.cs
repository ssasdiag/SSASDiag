namespace SimpleMDXParser
{
    using System;

    public abstract class MDXTreeNodeCollection
    {
        protected MDXTreeNodeCollection()
        {
        }

        public abstract MDXTreeNode Add(string label);
    }
}

