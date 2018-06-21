namespace SimpleMDXParser
{
    using System;
    using System.Runtime.CompilerServices;

    public abstract class MDXExpNode : MDXNode
    {
        protected bool m_IsCommonSubExpr;
        public MDXNode m_OuterIterator;

        protected MDXExpNode()
        {
        }

        internal virtual MDXTreeNode AddParseNode(MDXTreeNodeCollection Nodes)
        {
            MDXTreeNode node = Nodes.Add(this.GetLabel());
            node.Tag = this;
            switch (this.GetMDXType())
            {
                case MDXDataType.Number:
                case MDXDataType.String:
                    node.ImageKey = node.SelectedImageKey = "Measure.ico";
                    return node;

                case MDXDataType.Member:
                    node.ImageKey = node.SelectedImageKey = "Member.ico";
                    return node;

                case MDXDataType.Tuple:
                    node.ImageKey = node.SelectedImageKey = "Level2.ico";
                    return node;

                case MDXDataType.Set:
                    node.ImageKey = node.SelectedImageKey = "NamedSet.ico";
                    return node;

                case MDXDataType.Level:
                    node.ImageKey = node.SelectedImageKey = "Level6.ico";
                    return node;

                case MDXDataType.Hierarchy:
                    node.ImageKey = node.SelectedImageKey = "Hierarchy.ico";
                    return node;
            }
            return node;
        }

        protected void EndAnalyze(Analyzer analyzer)
        {
            if (this.m_IsCommonSubExpr)
            {
                analyzer.InCommonSubExpr = false;
            }
        }

        internal override void FillParseTree(MDXTreeNodeCollection Nodes)
        {
            this.AddParseNode(Nodes);
        }

        internal override MDXConstruct GetConstruct()
        {
            return MDXConstruct.Expression;
        }

        public override MDXExpNode GetExpNode()
        {
            return this;
        }

        public abstract string GetLabel();
        public abstract MDXDataType GetMDXType();
        internal static bool IsCachePrimitive(MDXExpNode e)
        {
            if (((e.GetType() != typeof(MDXIntegerConstNode)) && (e.GetType() != typeof(MDXFloatConstNode))) && ((e.GetType() != typeof(MDXStringConstNode)) && (e.GetType() != typeof(MDXIDNode))))
            {
                return (e.GetType() == typeof(MDXParamNode));
            }
            return true;
        }

        internal virtual bool IsCacheTrivial()
        {
            return false;
        }

        internal bool IsScalar()
        {
            MDXDataType mDXType = this.GetMDXType();
            if (mDXType != MDXDataType.String)
            {
                return (mDXType == MDXDataType.Number);
            }
            return true;
        }

        internal virtual void SetOuterIterator(MDXNode iter)
        {
            this.m_OuterIterator = iter;
        }

        protected void StartAnalyze(Analyzer analyzer)
        {
            if ((!this.IsCacheTrivial() && !analyzer.InCommonSubExpr) && ((base.GetType() == typeof(MDXUnaryOpNode)) || (this.GetMDXType() != MDXDataType.Set)))
            {
                MDXExpNode node;
                if (analyzer.SeenExprs.TryGetValue(this.GetNormalizedMDX(), out node))
                {
                    Message m = new Message(this) {
                        Id = 10,
                        Text = string.Format("Same expression was used before at Line {0} Column {1}. Consider eliminating common subexpressions for better performance and to take advantage of cache", node.Locator.Line, node.Locator.Column),
                        Severity = 2
                    };
                    analyzer.Add(m);
                    analyzer.InCommonSubExpr = true;
                    this.m_IsCommonSubExpr = true;
                }
                else
                {
                    analyzer.SeenExprs.Add(this.GetDebugMDX(), this);
                }
            }
        }

        internal virtual void Traverse(TraverseExpNode func, object traverseState)
        {
            func(this, traverseState);
        }

        public delegate void TraverseExpNode(MDXExpNode exp, object traverseState);
    }
}

