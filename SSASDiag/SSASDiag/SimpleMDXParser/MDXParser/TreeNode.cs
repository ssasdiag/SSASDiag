namespace SimpleMDXParser
{
    using System;

    public abstract class TreeNode
    {
        protected TreeNode()
        {
        }

        public abstract string ImageKey { get; set; }

        public abstract TreeNodeCollection Nodes { get; }

        public abstract TreeNode Parent { get; }

        public abstract string SelectedImageKey { get; set; }

        public abstract object Tag { get; set; }
    }
}

