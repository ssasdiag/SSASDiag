namespace SimpleMDXParser
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class MDXSelectNode : MDXStatementNode
    {
        private MDXAxesListNode m_Axes;
        private MDXExpListNode m_CellProps;
        private string m_Cube;
        internal List<MDXParameter> m_Params;
        private MDXSelectNode m_Subselect;
        internal MDXWhereNode m_Where;
        internal MDXWithListNode m_Withs;

        internal MDXSelectNode(MDXWithListNode withs, MDXAxesListNode axes, MDXWhereNode where, MDXSelectNode subselect, MDXStringConstNode cube, MDXExpListNode cellprops)
        {
            this.m_Withs = withs;
            this.m_Axes = axes;
            this.m_Where = where;
            this.m_Subselect = subselect;
            if (cube != null)
            {
                this.m_Cube = cube.GetString();
            }
            this.m_CellProps = cellprops;
            foreach (MDXAxisNode node in this.m_Axes)
            {
                node.ApplyNonEmpty(this.m_Axes);
            }
        }

        internal override void Analyze(Analyzer analyzer)
        {
            base.Analyze(analyzer);
            if (this.m_Withs != null)
            {
                this.m_Withs.Analyze(analyzer);
            }
            this.m_Axes.Analyze(analyzer);
            if (this.m_Subselect != null)
            {
                this.m_Subselect.Analyze(analyzer);
            }
            if (this.m_Where != null)
            {
                this.m_Where.Analyze(analyzer);
            }
        }

        internal override void AppendMDX(StringBuilder mdx, Formatter f, int indent, bool comma)
        {
            base.AppendComment(mdx, f, indent);
            if (this.m_Withs.Count > 0)
            {
                this.m_Withs.AppendComment(mdx, f, indent);
                f.AppendIndent(mdx, indent, comma);
                mdx.Append(f.Keyword("WITH "));
                this.m_Withs.AppendMDX(mdx, f, f.Indent(indent));
            }
            f.AppendIndent(mdx, indent, comma);
            mdx.Append(f.Keyword("SELECT "));
            this.m_Axes.AppendMDX(mdx, f, f.Indent(indent));
            f.AppendIndent(mdx, indent, comma);
            mdx.Append(f.Keyword("FROM "));
            if (this.m_Subselect != null)
            {
                f.AppendIndent(mdx, indent, comma);
                mdx.Append("(");
                this.m_Subselect.AppendMDX(mdx, f, f.Indent(indent));
                f.AppendIndent(mdx, indent, comma);
                mdx.Append(")");
            }
            else
            {
                mdx.Append(this.m_Cube);
            }
            if (this.m_Where != null)
            {
                this.m_Where.AppendComment(mdx, f, indent);
                f.AppendIndent(mdx, indent, comma);
                mdx.AppendFormat("{0} {1}", f.Keyword("WHERE"), this.m_Where.GetMDX(f, f.Indent(indent)));
            }
            if (this.m_CellProps != null)
            {
                f.AppendIndent(mdx, indent, comma);
                mdx.Append(f.Keyword("CELL PROPERTIES "));
                if (this.m_CellProps.Count == 1)
                {
                    this.m_CellProps.AppendMDX(mdx, f, -1);
                }
                else
                {
                    this.m_CellProps.AppendMDX(mdx, f, f.Indent(indent));
                }
            }
        }

        internal override void FillParseTree(TreeNodeCollection Nodes)
        {
            TreeNode node = Nodes.Add("SELECT");
            node.Tag = this;
            node.ImageKey = node.SelectedImageKey = "MDXQuery.ico";
            if (this.m_Withs.Count > 0)
            {
                TreeNode node2 = node.Nodes.Add("WITH");
                node2.ImageKey = node2.SelectedImageKey = "FolderClosed.ico";
                this.m_Withs.FillParseTree(node2.Nodes);
            }
            TreeNode node3 = node.Nodes.Add("Axes");
            node3.ImageKey = node3.SelectedImageKey = "FolderClosed.ico";
            this.m_Axes.FillParseTree(node3.Nodes);
            if (this.m_Subselect != null)
            {
                TreeNode node4 = node.Nodes.Add("FROM");
                node4.ImageKey = node4.SelectedImageKey = "CellCalculation.ico";
                this.m_Subselect.FillParseTree(node4.Nodes);
            }
            if (this.m_Cube != null)
            {
                TreeNode node5 = node.Nodes.Add(string.Format("FROM {0}", this.m_Cube));
                node5.ImageKey = node5.SelectedImageKey = "Cube.ico";
            }
            if (this.m_Where != null)
            {
                this.m_Where.FillParseTree(node.Nodes);
            }
            if (this.m_CellProps != null)
            {
                TreeNode node6 = node.Nodes.Add("CELL PROPERTIES");
                node6.ImageKey = node6.SelectedImageKey = "FolderClosed.ico";
                this.m_CellProps.FillParseTree(node6.Nodes);
            }
        }

        public List<MDXParameter> Params
        {
            get
            {
                return this.m_Params;
            }
        }

        public MDXNode Where
        {
            get
            {
                return this.m_Where;
            }
        }

        public MDXWithListNode Withs
        {
            get
            {
                return this.m_Withs;
            }
        }
    }
}

