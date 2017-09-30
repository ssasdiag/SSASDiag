using System;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Drawing;
using System.Text.RegularExpressions;

namespace XMLParser
{
    public class XMLViewer : System.Windows.Forms.RichTextBox
    {
        private XMLViewerSettings settings;
        /// <summary>
        /// The format settings.
        /// </summary>
        public XMLViewerSettings Settings
        {
            get
            {
                if (settings == null)
                {
                    settings = new XMLViewerSettings
                    {
                        AttributeKey = Color.Red,
                        AttributeValue = Color.Blue,
                        Tag = Color.Blue,
                        Element = Color.DarkRed,
                        Value = Color.Black,
                    };
                }
                return settings;
            }
            set
            {
                settings = value;
            }
        }

        bool ModifiedByPaste = true;
        protected override void OnTextChanged(EventArgs e)
        {
            if (ModifiedByPaste)
            {
                this.SuspendLayout();

                ModifiedByPaste = false;
                try
                {
                    Process(false);
                }
                catch (Exception ex)
                {
                    //throw;
                }
            }
            else
            {
                ResumeLayout();
                ModifiedByPaste = true;
            }
            base.OnTextChanged(e);
        }

        XmlNameTable names;
        XmlNamespaceManager nsm;
        string DefaultNamespace = "";
        public void Process(bool includeDeclaration)
        {
            try
            {
                int pos = SelectionStart;

                // The Rtf contains 2 parts, header and content. The colortbl is a part of
                // the header, and the {1} will be replaced with the content.
                string rtfFormat = @"{{\rtf1\ansi\ansicpg1252\deff0\deflang1033\deflangfe2052
{{\fonttbl{{\f0\fnil Courier New;}}}}
{{\colortbl ;{0}}}
\viewkind4\uc1\pard\lang1033\fs18\f0 
{1}}}";

                // Get the XDocument from the Text property.
                
                XmlReader reader = new XmlTextReader(new System.IO.StringReader(this.Text));
                reader.Read();
                names = reader.NameTable;
                nsm = new XmlNamespaceManager(names);
                var xmlDoc = XDocument.Load(reader);

                StringBuilder xmlRtfContent = new StringBuilder();

                // If includeDeclaration is true and the XDocument has declaration,
                // then add the declaration to the content.
                if (includeDeclaration && xmlDoc.Declaration != null)
                {

                    // The constants in XMLViewerSettings are used to specify the order 
                    // in colortbl of the Rtf.
                    xmlRtfContent.AppendFormat(@"
\cf{0} <?\cf{1} xml \cf{2} version\cf{0} =\cf0 ""\cf{3} {4}\cf0 "" 
\cf{2} encoding\cf{0} =\cf0 ""\cf{3} {5}\cf0 ""\cf{0} ?>\par",
                        XMLViewerSettings.TagID,
                        XMLViewerSettings.ElementID,
                        XMLViewerSettings.AttributeKeyID,
                        XMLViewerSettings.AttributeValueID,
                        xmlDoc.Declaration.Version,
                        xmlDoc.Declaration.Encoding);
                }

                DefaultNamespace = xmlDoc.Root.Name.Namespace.NamespaceName;
                // Get the Rtf of the root element.
                string rootRtfContent = ProcessElement(xmlDoc.Root, 0);

                xmlRtfContent.Append(rootRtfContent);

                // Construct the completed Rtf, and set the Rtf property to this value.
                this.Rtf = string.Format(rtfFormat, Settings.ToRtfFormatString(),
                    xmlRtfContent.ToString());

                Select(pos, 0);
            }
            catch (System.Xml.XmlException xmlException)
            {
                throw new ApplicationException(
                    "Please check the input Xml. Error:" + xmlException.Message,
                    xmlException);
            }
            catch
            {
                throw;
            }
        }

        // Get the Rtf of the xml element.
        private string ProcessElement(XElement element, int level)
        {

            // This viewer does not support the Xml file that has Namespace.
            //if (!string.IsNullOrEmpty(element.Name.Namespace.NamespaceName))
            //{
                //throw new ApplicationException(
                //    "This viewer does not support the Xml file that has Namespace.");
            //}

            string elementRtfFormat = string.Empty;
            StringBuilder childElementsRtfContent = new StringBuilder();
            StringBuilder attributesRtfContent = new StringBuilder();

            // Construct the indent.
            string indent = new string(' ', level).Replace(" ", @"\tab");

            // If the element has child elements or value, then add the element to the 
            // Rtf. {{0}} will be replaced with the attributes and {{1}} will be replaced
            // with the child elements or value.
            if (element.HasElements || !string.IsNullOrWhiteSpace(element.Value))
            {
                
                elementRtfFormat = string.Format(
                    @"{0}\cf{1} <\cf{2} {3}{{0}}\cf{1} >" + 
                    (element.HasElements ? @"\par" : "") + 
                    @"{{1}}\cf{1} " + 
                    (element.HasElements ? "{0}" : "") + 
                    @"</\cf{2} {3}\cf{1} >\par",
                    indent,
                    XMLViewerSettings.TagID,
                    XMLViewerSettings.ElementID,
                    element.Name.Namespace.NamespaceName == DefaultNamespace ? element.Name.LocalName : element.GetPrefixOfNamespace(element.Name.NamespaceName) + ":" + element.Name.LocalName);

                // Construct the Rtf of child elements.
                if (element.HasElements)
                {
                    foreach (var childElement in element.Elements())
                    {
                        string childElementRtfContent =
                            ProcessElement(childElement, level + 1);
                        childElementsRtfContent.Append(childElementRtfContent);
                    }
                }

                // If !string.IsNullOrWhiteSpace(element.Value), then construct the Rtf 
                // of the value.
                else
                {
                    childElementsRtfContent.AppendFormat(@"\cf{0} {1}",
                        XMLViewerSettings.ValueID,
                        CharacterEncoder.Encode(element.Value.Trim()));
                }
            }

            // This element only has attributes. {{0}} will be replaced with the attributes.
            else
            {
                elementRtfFormat =
                    string.Format(@"\cf{1} <\cf{2} {3}\{0\}}} \cf{1} />\par",
                    indent,
                    XMLViewerSettings.TagID,
                    XMLViewerSettings.ElementID,
                    element.Name.LocalName);
            }

            // Construct the Rtf of the attributes.
            if (element.HasAttributes)
            {
                foreach (XAttribute attribute in element.Attributes())
                {
                    string attributeRtfContent = string.Format(
                        @" \cf{0} {3}\cf{1} =\cf0 ""\cf{2} {4}\cf0 """,
                        XMLViewerSettings.AttributeKeyID,
                        XMLViewerSettings.TagID,
                        XMLViewerSettings.AttributeValueID,
                        attribute.Name.NamespaceName == "" ? "xmlns" : element.GetPrefixOfNamespace(attribute.Name.NamespaceName) + ":" + attribute.Name.LocalName,
                       CharacterEncoder.Encode(attribute.Value));
                    attributesRtfContent.Append(attributeRtfContent);
                }
                attributesRtfContent.Append(" ");
            }


            return string.Format(elementRtfFormat, attributesRtfContent,
                childElementsRtfContent);
        }

    }
}
