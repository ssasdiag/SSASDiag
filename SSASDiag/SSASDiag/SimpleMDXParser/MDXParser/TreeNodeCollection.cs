namespace SimpleMDXParser
{
    using System;

    public abstract class TreeNodeCollection
    {
        protected TreeNodeCollection()
        {
        }

        public abstract TreeNode Add(string label);
    }
}

