namespace SimpleMDXParser
{
    using SSVParseLib;
    using System;
    using System.Diagnostics;
    using System.Text;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;
    using System.Xml;

    public class MDXParser
    {
        private bool m_Cancel;
        private CubeInfo m_CubeInfo;
        private string m_MDX;
        private MDXParamParser m_ParamParser;
        private MDXEmptyNode m_PostComments;
        private Source m_Source;
        private MDXGrammarYaccClass m_SSParser;

        public MDXParser(string mdx)
        {
            this.Init(mdx, new Source(), new CubeInfo());
        }

        public MDXParser(string mdx, Source src, CubeInfo cb)
        {
            this.Init(mdx, src, cb);
        }

        public Analyzer Analyze()
        {
            Analyzer analyzer = new Analyzer();
            this.GetNode().Analyze(analyzer);
            DependencyGraph g = new DependencyGraph();
            this.GetNode().BuildDependencyGraph(g, new Vertex("root", g));
            g.FindLoops(analyzer);
            analyzer.Messages.Sort(new MessageLocationComparer());
            return analyzer;
        }

        public void Cancel()
        {
            this.m_Cancel = true;
            if (this.m_SSParser != null)
            {
                this.m_SSParser.m_Cancel = true;
            }
        }

        public bool ColorCode()
        {
            try
            {
                this.Parse(true);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public void FillTree(System.Windows.Forms.TreeNode parentnode)
        {
            WinFormsTreeNodeCollection nodes = new WinFormsTreeNodeCollection(parentnode.Nodes);
            this.m_SSParser.GetNode().FillParseTree(nodes);
            if (this.m_ParamParser.m_Params.Count > 0)
            {
                TreeNode node = nodes.Add("Parameters");
                node.ImageKey = node.SelectedImageKey = "FolderClosed.ico";
                foreach (MDXParameter parameter in this.m_ParamParser.m_Params)
                {
                    TreeNode node2 = node.Nodes.Add(parameter.m_Name);
                    node2.ImageKey = node2.SelectedImageKey = "FolderClosed.ico";
                    TreeNode node3 = node2.Nodes.Add(parameter.m_Value);
                    node3.ImageKey = node3.SelectedImageKey = "MeasureCalculated.ico";
                }
            }
        }

        public void FillWebTree(System.Web.UI.WebControls.TreeNode parentnode)
        {
            this.m_SSParser.GetNode().FillParseTree(new WebTreeNodeCollection(parentnode.ChildNodes));
        }

        public string FormatMDX()
        {
            FormatOptions fo = new FormatOptions();
            return this.FormatMDX(fo);
        }

        public string FormatMDX(FormatOptions fo)
        {
            StringBuilder mdx = new StringBuilder();
            Formatter f = new Formatter {
                Options = fo
            };
            mdx.Append(f.Prefix());
            if (this.m_SSParser.GetNode() != null) this.m_SSParser.GetNode().AppendMDX(mdx, f, 0);
            if (this.m_PostComments != null)
            {
                this.m_PostComments.AppendComment(mdx, f, 0);
            }
            if ('\n' == mdx[0])
            {
                mdx.Remove(0, 1);
            }
            if (this.m_ParamParser.m_Params.Count > 0)
            {
                mdx.Append("\n");
                XmlWriterSettings settings = new XmlWriterSettings {
                    Indent = true,
                    IndentChars = "  ",
                    OmitXmlDeclaration = true,
                    ConformanceLevel = ConformanceLevel.Fragment
                };
                XmlWriter writer = XmlWriter.Create(mdx, settings);
                writer.WriteStartElement("Parameters", "urn:schemas-microsoft-com:xml-analysis");
                writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
                writer.WriteAttributeString("xmlns", "xsd", null, "http://www.w3.org/2001/XMLSchema");
                foreach (MDXParameter parameter in this.m_ParamParser.m_Params)
                {
                    writer.WriteStartElement("Parameter");
                    writer.WriteElementString("Name", parameter.m_Name);
                    writer.WriteStartElement("Value");
                    writer.WriteAttributeString("xsi", "type", null, "xsd:string");
                    writer.WriteValue(parameter.m_Value);
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.Flush();
            }
            mdx.Append(f.Postfix());
            return mdx.ToString();
        }

        public ColorCoding GetColorCoding()
        {
            return this.m_SSParser.m_ColorCoding;
        }

        public string GetMdxText()
        {
            return this.m_SSParser.GetMdxText();
        }

        public MDXNode GetNode()
        {
            return this.m_SSParser.GetNode();
        }

        public MDXSelectNode GetSelect()
        {
            MDXNode node = this.GetNode();
            if (node == null)
            {
                return null;
            }
            MDXSelectNode node2 = null;
            if (typeof(MDXSelectNode) == node.GetType())
            {
                return (node as MDXSelectNode);
            }
            if (typeof(MDXScriptNode) == node.GetType())
            {
                MDXScriptNode node3 = node as MDXScriptNode;
                if ((1 == node3.Count) && (node3[0].GetType() == typeof(MDXSelectNode)))
                {
                    node2 = node3[0] as MDXSelectNode;
                }
            }
            return node2;
        }

        public bool HasColorCoding()
        {
            return ((this.m_SSParser != null) && (this.m_SSParser.m_ColorCoding != null));
        }

        private void Init(string mdx, Source src, CubeInfo cb)
        {
            this.m_MDX = mdx;
            this.m_Source = src;
            if (cb != null)
            {
                this.m_CubeInfo = cb;
            }
            else
            {
                this.m_CubeInfo = new CubeInfo();
            }
        }

        public void Parse()
        {
            this.Parse(false);
        }

        private void Parse(bool ColorCode)
        {
            string mDX = this.m_MDX;
            this.m_ParamParser = new MDXParamParser();
            mDX = this.m_ParamParser.ParseParameters(mDX);
            ColorCoding coding = new ColorCoding();
            MDXGrammarLexClass class2 = new MDXGrammarLexClass(new MDXGrammarLexTable(), new SSLexStringConsumer(mDX)) {
                m_Source = this.m_Source,
                m_ColorCoding = coding
            };
            this.m_SSParser = new MDXGrammarYaccClass(this.m_ParamParser, new MDXGrammarYaccTable(), class2);
            this.m_SSParser.m_ColorCodeOnly = ColorCode;
            this.m_SSParser.m_ColorCoding = coding;
            this.m_SSParser.SetMdxText(mDX);
            this.m_SSParser.SetMetadataInfo(this.m_CubeInfo);
            this.m_SSParser.m_Source = this.m_Source;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            if (!this.m_Cancel)
            {
                bool flag = this.m_SSParser.parse();
                if (!this.m_Cancel)
                {
                    stopwatch.Stop();
                    if (flag)
                    {
                        //throw new MDXParserException(this.m_SSParser.m_Errors);
                    }
                    if (class2.Comments.Count > 0)
                    {
                        this.m_PostComments = new MDXEmptyNode();
                        this.m_PostComments.Comments = class2.Comments;
                        class2.Comments = null;
                    }
                    if (this.m_ParamParser.m_Params.Count > 0)
                    {
                        this.GetNode();
                        MDXSelectNode select = this.GetSelect();
                        if (select != null)
                        {
                            select.m_Params = this.m_ParamParser.m_Params;
                        }
                    }
                }
            }
        }

        public void ParseExpression()
        {
            string mDX = this.m_MDX;
            try
            {
                this.m_MDX = string.Format("#{0}", this.m_MDX);
                if (this.m_Source != null)
                {
                    Locator startLocation = this.m_Source.StartLocation;
                    startLocation.Position--;
                }
                this.Parse();
            }
            finally
            {
                this.m_MDX = mDX;
            }
        }
    }
}

