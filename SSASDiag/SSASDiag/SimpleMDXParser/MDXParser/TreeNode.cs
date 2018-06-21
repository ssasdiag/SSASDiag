namespace SimpleMDXParser
{
    using System;

    public abstract class MDXTreeNode
    {
        protected MDXTreeNode()
        {
        }

        public abstract string ImageKey { get; set; }

        public abstract MDXTreeNodeCollection Nodes { get; }

        public abstract MDXTreeNode Parent { get; }

        public abstract string SelectedImageKey { get; set; }

        public abstract object Tag { get; set; }
    }
}

