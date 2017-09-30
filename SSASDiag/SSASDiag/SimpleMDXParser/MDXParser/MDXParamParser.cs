namespace SimpleMDXParser
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Xml;

    public class MDXParamParser
    {
        public List<MDXParameter> m_Params = new List<MDXParameter>();

        public string ParseParameters(string mdx)
        {
            string str = mdx;
            StringBuilder builder = new StringBuilder(mdx);
            if (str.Contains("<Parameters"))
            {
                int index = str.IndexOf("<Parameters");
                int num2 = str.IndexOf("</Parameters>", index);
                builder.Remove(index, builder.Length - index);
                XmlDocument document = new XmlDocument();
                try
                {
                    document.LoadXml(str.Substring(index, (num2 - index) + "</Parameters>".Length));
                }
                catch (Exception exception)
                {
                    throw new Exception("Unable to parse parameters: " + exception.Message);
                }
                foreach (XmlNode node in document.DocumentElement.ChildNodes)
                {
                    string name = "";
                    foreach (XmlNode node2 in node.ChildNodes)
                    {
                        if (node2.Name == "Name")
                        {
                            name = node2.InnerText;
                        }
                        else if (node2.Name == "Value")
                        {
                            if (node2.Attributes["xsi:type"].InnerText == "xsd:string")
                            {
                                this.m_Params.Add(new MDXParameter(name, node2.InnerText));
                            }
                            else
                            {
                                this.m_Params.Add(new MDXParameter(name, node2.InnerText));
                            }
                        }
                    }
                }
            }
            return builder.ToString();
        }
    }
}

