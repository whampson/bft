using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace WHampson.BFT
{
    class CustomTypeInfo
    {
        public string Kind { get; set; }
        public XElement Element { get; set; }
        public bool HasMembers { get { return Element.Descendants().Count() != 0; } }
    }

    internal class SyntaxParser
    {
        private const string RootElementName = "bft";

        //private const string VariableRegex = "\\$\\{(.+)\\}";

        private delegate void ValidationAction(XElement e);

        private XDocument doc;
        private Dictionary<Keyword, ValidationAction> validationActionMap;
        private Dictionary<string, CustomTypeInfo> typedefs;

        public SyntaxParser(ref XDocument doc)
        {
            this.doc = doc;
            validationActionMap = new Dictionary<Keyword, ValidationAction>();
            typedefs = new Dictionary<string, CustomTypeInfo>();

            BuildActionMap();
        }

        public void ParseTemplateStructure()
        {
            // Validate root element
            if (doc.Root.Name != RootElementName)
            {
                string fmt = "Template must have a root element named '{0}'.";
                string msg = XmlUtils.BuildXmlErrorMsg(doc.Root, fmt, RootElementName);
                throw new TemplateException(msg);
            }

            IEnumerable<XElement> elems = doc.Root.Elements();
            if (elems.Count() == 0)
            {
                throw new TemplateException("Empty template.");
            }

            // Validate rest of document
            ParseStructElement(doc.Root, true);

            ResolveTypedefs();
        }

        private void ResolveTypedefs()
        {
            // Remove 'typedef' elements from XML document
            doc.Descendants().Where(e => e.Name.LocalName == "typedef").Remove();

            // Substitute typedef'd types with their definitions
            IEnumerable<XElement> desc = doc.Descendants();
            foreach (XElement elem in desc)
            {
                if (!typedefs.ContainsKey(elem.Name.LocalName))
                {
                    continue;
                }

                CustomTypeInfo info = typedefs[elem.Name.LocalName];
                elem.Name = info.Kind;
                if (info.Kind == "struct" && info.HasMembers)
                {
                    elem.Add(info.Element.Elements());
                }
            }
        }

        private void ParseElement(XElement e, bool childrenAllowed, params Keyword[] modifiers)
        {
            string name = e.Name.LocalName;
            if (!childrenAllowed && !e.IsEmpty)
            {
                string fmt = "'{0}' cannot contain member fields.";
                throw new TemplateException(XmlUtils.BuildXmlErrorMsg(e, fmt, name));
            }

            CheckAttributePresence(e, modifiers);
        }

        private void CheckAttributePresence(XElement e, params Keyword[] modifiers)
        {
            IEnumerable<XAttribute> attrs = e.Attributes();
            foreach (XAttribute m in attrs)
            {
                string mName = m.Name.LocalName;
                Keyword kw;

                // Get modifier keyword
                bool found = Keyword.IdentifierMap.TryGetValue(mName, out kw);
                if (!found || kw.Type != Keyword.KeywordType.Modifier)
                {
                    string fmt = "Unknown modifier '{0}'.";
                    throw new TemplateException(XmlUtils.BuildXmlErrorMsg(m, fmt, mName));
                }

                // Check if modifier valid for current type
                if (!modifiers.Contains(kw))
                {
                    string fmt = "Invalid modifier '{0}'.";
                    throw new TemplateException(XmlUtils.BuildXmlErrorMsg(m, fmt, mName));
                }

                // Validate modifier
                if (string.IsNullOrWhiteSpace(m.Value))
                {
                    string fmt = "Value for modifier '{0}' cannot be empty.";
                    throw new TemplateException(XmlUtils.BuildXmlErrorMsg(m, fmt, mName));
                }
            }
        }

        private void ParseDirectiveElement(XElement e, bool childrenAllowed, params Keyword[] modifiers)
        {
            ParseElement(e, childrenAllowed, modifiers);
        }

        private void ParsePrimitiveElement(XElement e, params Keyword[] modifiers)
        {
            ParseElement(e, false, modifiers);
        }

        private void ParseStructElement(XElement e)
        {
            ParseStructElement(e, false);
        }

        private void ParseStructElement(XElement e, bool ignoreModifiers)
        {
            string name = e.Name.LocalName;

            // Ensure no text data is present
            if (!string.IsNullOrWhiteSpace(e.Value))
            {
                string fmt = "Unexpected textual data.";
                throw new TemplateException(XmlUtils.BuildXmlErrorMsg(e, fmt));
            }

            // Ensure struct has at least one member
            CustomTypeInfo info;
            bool typedefd = typedefs.TryGetValue(name, out info);
            IEnumerable<XElement> children = e.Elements();
            if (!typedefd && children.Count() == 0)
            {
                string fmt = "Empty structs are not allowed.";
                throw new TemplateException(XmlUtils.BuildXmlErrorMsg(e, fmt));
            }

            // Validate modifiers
            if (!ignoreModifiers)
            {
                CheckAttributePresence(e, Keyword.Comment, Keyword.Count, Keyword.Name);
            }

            // Validate members
            foreach (XElement child in children)
            {
                string childName = child.Name.LocalName;
                Keyword kw;

                // Get member keyword
                bool found = LookupType(childName, out kw);
                if (!found || kw.Type == Keyword.KeywordType.Modifier)
                {
                    string fmt = "Unknown type or directive '{0}'.";
                    throw new TemplateException(XmlUtils.BuildXmlErrorMsg(child, fmt, childName));
                }

                // Validate!
                ValidationAction validate = validationActionMap[kw];
                validate(child);
            }
        }

        private void ParseFloatElement(XElement e)
        {
            ParsePrimitiveElement(e, Keyword.Comment, Keyword.Count, Keyword.Name, Keyword.Sentinel, Keyword.Thresh);

            XAttribute sentinel = e.Attribute("sentinel");      // I don't like hardcoding these but it'll have to do
            XAttribute thresh = e.Attribute("thresh");

            // Ensure 'thresh' is only present when 'sentinel' is also present
            if (thresh != null && sentinel == null)
            {
                string fmt = "Modifier 'thresh' requires modifier 'sentinel' to be present.";
                throw new TemplateException(XmlUtils.BuildXmlErrorMsg(thresh, fmt));
            }
        }

        private void ParseIntegerElement(XElement e)
        {
            ParsePrimitiveElement(e, Keyword.Comment, Keyword.Count, Keyword.Name, Keyword.Sentinel);
        }

        private void ParseAlignElement(XElement e)
        {
            ParseDirectiveElement(e, false, Keyword.Count, Keyword.Kind);
        }

        private void ParseEchoElement(XElement e)
        {
            ParseDirectiveElement(e, false, Keyword.Message);

            XAttribute message = e.Attribute("message");
            string textData = e.Value;
            if (message == null)
            {
                string fmt = "Missing required message.";
                throw new TemplateException(XmlUtils.BuildXmlErrorMsg(e, fmt));
            }
        }

        private void ParseTypedefElement(XElement e)
        {
            ParseDirectiveElement(e, true, Keyword.Kind, Keyword.Typename);

            XAttribute kind = e.Attribute("kind");
            XAttribute typename = e.Attribute("typename");

            if (kind == null)
            {
                string fmt = "Missing required modifier 'kind'.";
                throw new TemplateException(XmlUtils.BuildXmlErrorMsg(e, fmt));
            }
            else if (typename == null)
            {
                string fmt = "Missing required modifier 'typename'.";
                throw new TemplateException(XmlUtils.BuildXmlErrorMsg(e, fmt));
            }

            if (kind.Value != "struct" && !e.IsEmpty)
            {
                string fmt = "Type definitions not descending from 'struct' cannot contain member fields.";
                throw new TemplateException(XmlUtils.BuildXmlErrorMsg(e, fmt));
            }
            else if (kind.Value == "struct")
            {
                IEnumerable<XElement> nestedTypedefs = e.Descendants().Where(t => t.Name.LocalName == "typedef");
                if (nestedTypedefs.Count() != 0)
                {
                    string fmt = "Nested type definitions are not allowed.";
                    string msg = XmlUtils.BuildXmlErrorMsg(nestedTypedefs.ElementAt(0), fmt);
                    throw new TemplateException(msg);
                }

                ParseStructElement(e, true);
            }

            CustomTypeInfo typeInfo = new CustomTypeInfo();
            typeInfo.Kind = kind.Value;
            typeInfo.Element = e;

            typedefs[typename.Value] = typeInfo;
        }

        private bool LookupType(string name, out Keyword kw)
        {
            string kind = name;
            CustomTypeInfo info = null;
            while (typedefs.TryGetValue(kind, out info))
            {
                kind = info.Kind;
            }
            
            return Keyword.IdentifierMap.TryGetValue(kind, out kw);
        }

        private void BuildActionMap()
        {
            // Data types
            validationActionMap[Keyword.Double] = ParseFloatElement;
            validationActionMap.Add(Keyword.Float, ParseFloatElement);
            validationActionMap.Add(Keyword.Int8, ParseIntegerElement);
            validationActionMap.Add(Keyword.Int16, ParseIntegerElement);
            validationActionMap.Add(Keyword.Int32, ParseIntegerElement);
            validationActionMap.Add(Keyword.Int64, ParseIntegerElement);
            validationActionMap.Add(Keyword.Struct, ParseStructElement);
            validationActionMap.Add(Keyword.UInt8, ParseIntegerElement);
            validationActionMap.Add(Keyword.UInt16, ParseIntegerElement);
            validationActionMap.Add(Keyword.UInt32, ParseIntegerElement);
            validationActionMap.Add(Keyword.UInt64, ParseIntegerElement);

            // Directives
            validationActionMap.Add(Keyword.Align, ParseAlignElement);
            validationActionMap.Add(Keyword.Echo, ParseEchoElement);
            validationActionMap.Add(Keyword.Typedef, ParseTypedefElement);
        }
    }
}
