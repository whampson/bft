#region License
/* Copyright (c) 2017 Wes Hampson
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using static WHampson.BFT.Keyword;

namespace WHampson.BFT
{
    /// <summary>
    /// Handles syntax validation and name resolution.
    /// </summary>
    internal class SyntaxParser
    {
        private const string RootElementName = "bft";

        private delegate ISyntaxTreeNode ParseAction(XElement e);

        private XDocument doc;
        private Dictionary<BuiltinType, ParseAction> builtinTypeParseActionMap;
        private Dictionary<Directive, ParseAction> directiveParseActionMap;
        private Dictionary<string, CustomTypeInfo> customTypeMap;

        /// <summary>
        /// Creates a new <see cref="SyntaxParser"/> object for the specified
        /// template document.
        /// </summary>
        /// <param name="doc">
        /// The XML document pertaining to the binary file template.
        /// </param>
        public SyntaxParser(XDocument doc)
        {
            this.doc = doc;
            builtinTypeParseActionMap = new Dictionary<BuiltinType, ParseAction>();
            directiveParseActionMap = new Dictionary<Directive, ParseAction>();
            customTypeMap = new Dictionary<string, CustomTypeInfo>();

            BuildActionMaps();
        }


        /// <summary>
        /// Validates the element-wise structure of the template and builds
        /// a map of user-defined types.
        /// </summary>
        public SyntaxTree ParseTemplateStructure()
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


            // Parse rest of document
            ISyntaxTreeNode root = ParseStructElement(doc.Root, true);
            SyntaxTree syntaxTree = new SyntaxTree(root);

            //ResolveTypedefs();

            return syntaxTree;
        }

        //private void ResolveTypedefs()
        //{
        //    // Remove 'typedef' elements from XML document
        //    doc.Descendants().Where(e => e.Name.LocalName == Keyword.TypedefIdentifier).Remove();

        //    // Substitute typedef'd types with their definitions
        //    IEnumerable<XElement> desc = doc.Descendants();
        //    foreach (XElement elem in desc)
        //    {
        //        string identifier = elem.Name.LocalName;

        //        // Skip builtins and directives
        //        if (!customTypeMap.ContainsKey(identifier))
        //        {
        //            continue;
        //        }

        //        CustomTypeInfo info = customTypeMap[identifier];
        //        elem.Name = info.Kind.ToString().ToLower();
        //        if (info.Kind == BuiltinType.Struct)
        //        {
        //            elem.Add(info.Members);
        //        }
        //    }
        //}

        private Dictionary<Modifier, string> ParseElement(XElement e, bool childrenAllowed, params Modifier[] modifiers)
        {
            string name = e.Name.LocalName;
            if (!childrenAllowed && !e.IsEmpty)
            {
                string fmt = "'{0}' cannot contain member fields.";
                throw new TemplateException(XmlUtils.BuildXmlErrorMsg(e, fmt, name));
            }

            return CheckAttributePresence(e, modifiers);
        }

        private Dictionary<Modifier, string> CheckAttributePresence(XElement e, params Modifier[] validAttrs)
        {
            Dictionary<Modifier, string> modifierMap = new Dictionary<Modifier, string>();
            IEnumerable<XAttribute> attrs = e.Attributes();

            foreach (XAttribute attr in attrs)
            {
                string mId = attr.Name.LocalName;
                Modifier m;

                // Get modifier
                if (!ModifierIdentifierMap.TryGetValue(mId, out m))
                {
                    string fmt = "Unknown modifier '{0}'.";
                    throw new TemplateException(XmlUtils.BuildXmlErrorMsg(attr, fmt, mId));
                }

                // Check if modifier is valid for current type
                if (!validAttrs.Contains(m))
                {
                    string fmt = "Invalid modifier '{0}'.";
                    throw new TemplateException(XmlUtils.BuildXmlErrorMsg(attr, fmt, mId));
                }

                // Validate modifier
                if (string.IsNullOrWhiteSpace(attr.Value))
                {
                    string fmt = "Value for modifier '{0}' cannot be empty.";
                    throw new TemplateException(XmlUtils.BuildXmlErrorMsg(attr, fmt, mId));
                }

                modifierMap[m] = attr.Value;
            }

            return modifierMap;
        }

        private ISyntaxTreeNode ParseBuiltinTypeElement(XElement e, params Modifier[] modifiers)
        {
            string identifier = e.Name.LocalName;
            BuiltinType type;
            LookupType(identifier, out type);

            Dictionary<Modifier, string> modifierMap = ParseElement(e, false, modifiers);

            return new DataTypeTreeNode(type, modifierMap);
        }

        private ISyntaxTreeNode ParseDirectiveElement(XElement e, bool childrenAllowed, params Modifier[] modifiers)
        {
            string identifier = e.Name.LocalName;
            Directive directive = DirectiveIdentifierMap[identifier];

            Dictionary<Modifier, string> modifierMap = ParseElement(e, childrenAllowed, modifiers);

            return new DirectiveTreeNode(directive, modifierMap);
        }

        private ISyntaxTreeNode ParseStructElement(XElement e)
        {
            return ParseStructElement(e, false);
        }

        private ISyntaxTreeNode ParseStructElement(XElement e, bool ignoreModifiers)
        {
            string name = e.Name.LocalName;

            // Ensure no text data is present
            if (!string.IsNullOrWhiteSpace(e.Value))
            {
                string fmt = "Unexpected textual data.";
                throw new TemplateException(XmlUtils.BuildXmlErrorMsg(e, fmt));
            }

            CustomTypeInfo typeInfo;
            bool isTypedefd = customTypeMap.TryGetValue(name, out typeInfo);

            // Ensure struct has at least one member
            IEnumerable<XElement> children = e.Elements();
            if (!isTypedefd && children.Count() == 0)
            {
                string fmt = "Empty structs are not allowed.";
                throw new TemplateException(XmlUtils.BuildXmlErrorMsg(e, fmt));
            }

            // Validate modifiers
            Dictionary<Modifier, string> modifierMap;
            if (!ignoreModifiers)
            {
                modifierMap = CheckAttributePresence(e, Modifier.Comment, Modifier.Count, Modifier.Name);
            }
            else
            {
                modifierMap = new Dictionary<Modifier, string>();
            }

            if (isTypedefd)
            {
                children = typeInfo.Members;
            }

            DataTypeTreeNode node = new DataTypeTreeNode(BuiltinType.Struct, modifierMap);

            // Parse members
            foreach (XElement memberElem in children)
            {
                string identifier = memberElem.Name.LocalName;
                bool doLookupDirective = false;

                // Look up member type
                BuiltinType type;
                bool typeFound = LookupType(identifier, out type);
                if (!typeFound)
                {
                    doLookupDirective = true;
                }

                // Look up directive if necessary
                Directive dir = Directive.Align;
                if (doLookupDirective && !DirectiveIdentifierMap.TryGetValue(identifier, out dir))
                {
                    string fmt = "Unknown type or directive '{0}'.";
                    throw new TemplateException(XmlUtils.BuildXmlErrorMsg(memberElem, fmt, identifier));
                }

                // Parse type or directive
                ParseAction parse;
                if (typeFound)
                {
                    parse = builtinTypeParseActionMap[type];
                }
                else
                {
                    parse = directiveParseActionMap[dir];
                }

                ISyntaxTreeNode n = parse(memberElem);
                if (!(n is DirectiveTreeNode
                    && ((DirectiveTreeNode)n).Directive == Directive.Typedef))
                {
                    node.Children.Add(n);
                }
            }

            return node;
        }

        private ISyntaxTreeNode ParseFloatElement(XElement e)
        {
            ISyntaxTreeNode n = ParseBuiltinTypeElement(e,
                Modifier.Comment, Modifier.Count, Modifier.Name, Modifier.Sentinel, Modifier.Thresh);

            XAttribute sentinelAttr = e.Attribute(SentinelIdentifier);
            XAttribute threshAttr = e.Attribute(ThreshIdentifier);

            // Ensure 'thresh' modifier is only present when 'sentinel' is also present
            if (threshAttr != null && sentinelAttr == null)
            {
                string fmt = "Modifier '{0}' requires modifier '{1}' to be present.";
                string msg = XmlUtils.BuildXmlErrorMsg(threshAttr, fmt, ThreshIdentifier, SentinelIdentifier);
                throw new TemplateException(msg);
            }

            return n;
        }

        private ISyntaxTreeNode ParseIntegerElement(XElement e)
        {
            return ParseBuiltinTypeElement(e, Modifier.Comment, Modifier.Count, Modifier.Name, Modifier.Sentinel);
        }

        private ISyntaxTreeNode ParseAlignElement(XElement e)
        {
            return ParseDirectiveElement(e, false, Modifier.Count, Modifier.Kind);
        }

        private ISyntaxTreeNode ParseEchoElement(XElement e)
        {
            ISyntaxTreeNode n = ParseDirectiveElement(e, false, Modifier.Message);

            //XAttribute messageAttr = e.Attribute(MessageIdentifier);
            //string textData = e.Value;
            //if (messageAttr == null)
            string msg;
            if (!n.Modifiers.TryGetValue(Modifier.Message, out msg))
            {
                string fmt = "Missing required message.";
                throw new TemplateException(XmlUtils.BuildXmlErrorMsg(e, fmt));
            }

            return n;
        }

        private ISyntaxTreeNode ParseTypedefElement(XElement e)
        {
            ParseDirectiveElement(e, true, Modifier.Kind, Modifier.Name);

            XAttribute kindAttr = e.Attribute(KindIdentifier);
            XAttribute nameAttr = e.Attribute(NameIdentifier);

            if (kindAttr == null)
            {
                string fmt = "Missing required modifier '{0}'.";
                throw new TemplateException(XmlUtils.BuildXmlErrorMsg(e, fmt, KindIdentifier));
            }
            else if (nameAttr == null)
            {
                string fmt = "Missing required modifier '{0}'.";
                throw new TemplateException(XmlUtils.BuildXmlErrorMsg(e, fmt, NameIdentifier));
            }

            if (kindAttr.Value != StructIdentifier && !e.IsEmpty)
            {
                string fmt = "Type definitions not descending directly from '{0}' cannot contain member fields.";
                throw new TemplateException(XmlUtils.BuildXmlErrorMsg(e, fmt, StructIdentifier));
            }
            else if (kindAttr.Value == StructIdentifier)
            {
                // Ensure there are no nested typedefs
                IEnumerable<XElement> nestedTypedefs = e.Descendants()
                    .Where(t => t.Name.LocalName == TypedefIdentifier);
                if (nestedTypedefs.Count() != 0)
                {
                    string fmt = "Nested type definitions are not allowed.";
                    string msg = XmlUtils.BuildXmlErrorMsg(nestedTypedefs.ElementAt(0), fmt);
                    throw new TemplateException(msg);
                }

                // Parse type definition
                ParseStructElement(e, true);
            }

            string newTypeName = nameAttr.Value;
            string kindIdentifier = kindAttr.Value;
            BuiltinType kind;

            // Ensure type isn't already defined
            if (LookupType(newTypeName, out kind))
            {
                string fmt = "Type '{0}' has already been defined.";
                throw new TemplateException(XmlUtils.BuildXmlErrorMsg(nameAttr, fmt, newTypeName));
            }

            // Ensure new type is built from a pre-existing type
            if (!LookupType(kindIdentifier, out kind))
            {
                string fmt = "Unknown type '{0}'."; ;
                throw new TemplateException(XmlUtils.BuildXmlErrorMsg(kindAttr, fmt, kindIdentifier));
            }

            // If new type descends from another custom type,
            // get member elements from that type
            IEnumerable<XElement> members;
            CustomTypeInfo existingTypeInfo;
            if (customTypeMap.TryGetValue(kindIdentifier, out existingTypeInfo))
            {
                members = existingTypeInfo.Members;
            }
            else
            {
                members = e.Descendants();
            }

            // Store custom type
            CustomTypeInfo newTypeInfo = new CustomTypeInfo(kind, members);
            customTypeMap[newTypeName] = newTypeInfo;

            Console.WriteLine("{0} => {1}", newTypeName, kind.ToString().ToLower());

            return new DirectiveTreeNode(Directive.Typedef, null);
        }

        private bool LookupType(string identifier, out BuiltinType type)
        {
            // Look up in custom type list
            CustomTypeInfo info;
            bool found = customTypeMap.TryGetValue(identifier, out info);
            if (found)
            {
                type = info.Kind;
                return true;
            }

            // Look up in builtin type list
            return BuiltinTypeIdentifierMap.TryGetValue(identifier, out type);
        }

        private void BuildActionMaps()
        {
            // Builtin types
            builtinTypeParseActionMap[BuiltinType.Double] = ParseFloatElement;
            builtinTypeParseActionMap[BuiltinType.Float] = ParseFloatElement;
            builtinTypeParseActionMap[BuiltinType.Int8] = ParseIntegerElement;
            builtinTypeParseActionMap[BuiltinType.Int16] = ParseIntegerElement;
            builtinTypeParseActionMap[BuiltinType.Int32] = ParseIntegerElement;
            builtinTypeParseActionMap[BuiltinType.Int64] = ParseIntegerElement;
            builtinTypeParseActionMap[BuiltinType.Struct] = ParseStructElement;
            builtinTypeParseActionMap[BuiltinType.UInt8] = ParseIntegerElement;
            builtinTypeParseActionMap[BuiltinType.UInt16] = ParseIntegerElement;
            builtinTypeParseActionMap[BuiltinType.UInt32] = ParseIntegerElement;
            builtinTypeParseActionMap[BuiltinType.UInt64] = ParseIntegerElement;

            // Directives
            directiveParseActionMap[Directive.Align] = ParseAlignElement;
            directiveParseActionMap[Directive.Echo] = ParseEchoElement;
            directiveParseActionMap[Directive.Typedef] = ParseTypedefElement;
        }
    }
}