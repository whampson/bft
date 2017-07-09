//#region License
///* Copyright (c) 2017 Wes Hampson
// * 
// * Permission is hereby granted, free of charge, to any person obtaining a copy
// * of this software and associated documentation files (the "Software"), to deal
// * in the Software without restriction, including without limitation the rights
// * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// * copies of the Software, and to permit persons to whom the Software is
// * furnished to do so, subject to the following conditions:
// * 
// * The above copyright notice and this permission notice shall be included in all
// * copies or substantial portions of the Software.
// * 
// * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// * SOFTWARE.
// */
//#endregion

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Runtime.InteropServices;
//using System.Xml.Linq;
//using static WHampson.Bft.Keyword;

//namespace WHampson.Bft
//{
//    /// <summary>
//    /// Handles syntax validation and name resolution.
//    /// </summary>
//    internal class SyntaxParser
//    {
//        private const string RootElementName = "bft";

//        private delegate void ParseAction(XElement e);

//        private XDocument doc;
//        private Dictionary<BuiltinType, ParseAction> builtinTypeParseActionMap;
//        private Dictionary<Directive, ParseAction> directiveParseActionMap;
//        private Dictionary<string, CustomTypeInfo> customTypeMap;

//        /// <summary>
//        /// Creates a new <see cref="SyntaxParser"/> object for the specified
//        /// template document.
//        /// </summary>
//        /// <param name="doc">
//        /// The XML document pertaining to the binary file template.
//        /// </param>
//        public SyntaxParser(XDocument doc)
//        {
//            this.doc = doc;
//            builtinTypeParseActionMap = new Dictionary<BuiltinType, ParseAction>();
//            directiveParseActionMap = new Dictionary<Directive, ParseAction>();
//            customTypeMap = new Dictionary<string, CustomTypeInfo>();

//            BuildActionMaps();
//        }

//        public Dictionary<string, CustomTypeInfo> CustomTypeMap
//        {
//            get { return new Dictionary<string, CustomTypeInfo>(customTypeMap); }
//        }

//        /// <summary>
//        /// Validates the element-wise structure of the template and builds
//        /// a map of user-defined types.
//        /// </summary>
//        public void ParseTemplateStructure()
//        {
//            // Validate root element
//            if (doc.Root.Name != RootElementName)
//            {
//                string fmt = "Template must have a root element named '{0}'.";
//                string msg = XmlUtils.BuildXmlErrorMsg(doc.Root, fmt, RootElementName);
//                throw new TemplateException(msg);
//            }

//            IEnumerable<XElement> elems = doc.Root.Elements();
//            if (elems.Count() == 0)
//            {
//                throw new TemplateException("Empty template.");
//            }

//            // Parse rest of document
//            customTypeMap.Clear();
//            ParseStructElement(doc.Root, true);
//        }

//        private Dictionary<Modifier, string> ParseElement(XElement e, bool childrenAllowed, params Modifier[] modifiers)
//        {
//            string name = e.Name.LocalName;
//            if (!childrenAllowed && !e.IsEmpty)
//            {
//                string fmt = "'{0}' cannot contain member fields.";
//                throw new TemplateException(XmlUtils.BuildXmlErrorMsg(e, fmt, name));
//            }

//            return BuildModifierMap(e, modifiers);
//        }

//        /// <summary>
//        /// Build a dictionary of <see cref="Modifier"/>s to string values and
//        /// ensures that all are valid for the provided XML element.
//        /// </summary>
//        /// <param name="e">
//        /// The XML element from which modifiers will be extracted.
//        /// </param>
//        /// <param name="validAttrs">
//        /// Modifiers (XML Attributes) which are allowed for thie provided element.
//        /// </param>
//        /// <returns>
//        /// A dictionary of <see cref="Modifier"/>s to their corresponding
//        /// string values.
//        /// </returns>
//        private Dictionary<Modifier, string> BuildModifierMap(XElement e, params Modifier[] validAttrs)
//        {
//            Dictionary<Modifier, string> modifierMap = new Dictionary<Modifier, string>();
//            IEnumerable<XAttribute> attrs = e.Attributes();

//            foreach (XAttribute attr in attrs)
//            {
//                string mId = attr.Name.LocalName;
//                Modifier m;

//                // Get modifier
//                if (!ModifierIdentifierMap.TryGetValue(mId, out m))
//                {
//                    string fmt = "Unknown modifier '{0}'.";
//                    throw new TemplateException(XmlUtils.BuildXmlErrorMsg(attr, fmt, mId));
//                }

//                // Check if modifier is valid for current type
//                if (!validAttrs.Contains(m))
//                {
//                    string fmt = "Invalid modifier '{0}'.";
//                    throw new TemplateException(XmlUtils.BuildXmlErrorMsg(attr, fmt, mId));
//                }

//                // Validate modifier
//                if (string.IsNullOrWhiteSpace(attr.Value))
//                {
//                    string fmt = "Value for modifier '{0}' cannot be empty.";
//                    throw new TemplateException(XmlUtils.BuildXmlErrorMsg(attr, fmt, mId));
//                }

//                modifierMap[m] = attr.Value;
//            }

//            return modifierMap;
//        }

//        private Dictionary<Modifier, string> ParseBuiltinTypeElement(XElement e, params Modifier[] modifiers)
//        {
//            string identifier = e.Name.LocalName;
//            BuiltinType type;
//            LookupType(identifier, out type);

//            return ParseElement(e, false, modifiers);
//        }

//        private Dictionary<Modifier, string> ParseDirectiveElement(XElement e, bool childrenAllowed, params Modifier[] modifiers)
//        {
//            string identifier = e.Name.LocalName;
//            Directive directive = DirectiveIdentifierMap[identifier];

//            return ParseElement(e, childrenAllowed, modifiers);
//        }

//        private void ParseStructElement(XElement e)
//        {
//            ParseStructElement(e, false);
//        }

//        private void ParseStructElement(XElement e, bool ignoreModifiers)
//        {
//            string identifier = e.Name.LocalName;

//            // Ensure no text data is present
//            if (!string.IsNullOrWhiteSpace(e.Value))
//            {
//                string fmt = "Unexpected textual data.";
//                throw new TemplateException(XmlUtils.BuildXmlErrorMsg(e, fmt));
//            }

//            // Check whether this is a "raw" struct or if it is a user-defied type.
//            CustomTypeInfo typeInfo;
//            bool isTypedefd = customTypeMap.TryGetValue(identifier, out typeInfo);

//            // Get member elements
//            IEnumerable<XElement> children = (isTypedefd) ? typeInfo.Members : e.Elements();

//            // Ensure struct has at least one member field
//            if (children.Count() == 0)
//            {
//                string fmt = "Empty structs are not allowed.";
//                throw new TemplateException(XmlUtils.BuildXmlErrorMsg(e, fmt));
//            }

//            // Validate modifiers
//            Dictionary<Modifier, string> modifierMap = (!ignoreModifiers)
//                ? BuildModifierMap(e, Modifier.Comment, Modifier.Count, Modifier.Name)
//                : new Dictionary<Modifier, string>();

//            // Parse members
//            bool hasDataFields = false;
//            foreach (XElement memberElem in children)
//            {
//                string mIdentifier = memberElem.Name.LocalName;
//                bool doLookupDirective = false;

//                // Look up member type
//                BuiltinType type;
//                bool validType = LookupType(mIdentifier, out type);
//                if (validType)
//                {
//                    hasDataFields = true;
//                }
//                else
//                {
//                    doLookupDirective = true;
//                }

//                // Look up directive if not a valid data type
//                Directive dir = Directive.Align;    // dummy value
//                if (doLookupDirective && !DirectiveIdentifierMap.TryGetValue(mIdentifier, out dir))
//                {
//                    string fmt = "Unknown type or directive '{0}'.";
//                    throw new TemplateException(XmlUtils.BuildXmlErrorMsg(memberElem, fmt, mIdentifier));
//                }

//                if (dir == Directive.Align)
//                {
//                    hasDataFields = true;
//                }

//                // Parse type or directive
//                ParseAction parse = (validType)
//                    ? builtinTypeParseActionMap[type]
//                    : directiveParseActionMap[dir];

//                parse(memberElem);
//            }

//            // Ensure that member elements consisted of at least one field representing data
//            if (!hasDataFields)
//            {
//                string fmt = "Empty structs are not allowed.";
//                throw new TemplateException(XmlUtils.BuildXmlErrorMsg(e, fmt));
//            }
//        }

//        private void ParseFloatElement(XElement e)
//        {
//            Dictionary<Modifier, string> modifierMap = ParseBuiltinTypeElement(e,
//                Modifier.Comment, Modifier.Count, Modifier.Name, Modifier.Sentinel, Modifier.Thresh);

//            string sentinelValue;
//            string threshValue;
//            bool hasSentinel = modifierMap.TryGetValue(Modifier.Sentinel, out sentinelValue);
//            bool hasThresh = modifierMap.TryGetValue(Modifier.Thresh, out threshValue);

//            // Ensure 'thresh' modifier is only present when 'sentinel' is also present
//            if (hasThresh && !hasSentinel)
//            {
//                XAttribute threshAttr = e.Attribute(ThreshIdentifier);
//                string fmt = "Modifier '{0}' requires modifier '{1}' to be present.";
//                string msg = XmlUtils.BuildXmlErrorMsg(threshAttr, fmt, ThreshIdentifier, SentinelIdentifier);
//                throw new TemplateException(msg);
//            }
//        }

//        private void ParseIntegerElement(XElement e)
//        {
//            ParseBuiltinTypeElement(e, Modifier.Comment, Modifier.Count, Modifier.Name, Modifier.Sentinel);
//        }

//        private void ParseAlignElement(XElement e)
//        {
//            ParseDirectiveElement(e, false, Modifier.Count, Modifier.Kind);
//        }

//        private void ParseEchoElement(XElement e)
//        {
//            Dictionary<Modifier, string> modifierMap = ParseDirectiveElement(e, false, Modifier.Message);

//            string msg;
//            if (!modifierMap.TryGetValue(Modifier.Message, out msg))
//            {
//                string fmt = "Missing required message.";
//                throw new TemplateException(XmlUtils.BuildXmlErrorMsg(e, fmt));
//            }
//        }

//        private void ParseTypedefElement(XElement e)
//        {
//            Dictionary<Modifier, string> modifierMap = ParseDirectiveElement(e, true, Modifier.Kind, Modifier.Name);

//            string kindValue;
//            string nameValue;

//            if (!modifierMap.TryGetValue(Modifier.Kind, out kindValue))
//            {
//                string fmt = "Missing required modifier '{0}'.";
//                throw new TemplateException(XmlUtils.BuildXmlErrorMsg(e, fmt, KindIdentifier));
//            }
//            else if (!modifierMap.TryGetValue(Modifier.Name, out nameValue))
//            {
//                string fmt = "Missing required modifier '{0}'.";
//                throw new TemplateException(XmlUtils.BuildXmlErrorMsg(e, fmt, NameIdentifier));
//            }

//            // If the user-defined type's kind is not explicitly "struct", ensure that the definition instance
//            // does not have member fields, since only structs can contain member fields.
//            if (kindValue != StructIdentifier && !e.IsEmpty)
//            {
//                string fmt = "Type definitions not descending directly from '{0}' cannot contain member fields.";
//                throw new TemplateException(XmlUtils.BuildXmlErrorMsg(e, fmt, StructIdentifier));
//            }

//            if (kindValue == StructIdentifier)
//            {
//                // Ensure there are no nested typedefs
//                IEnumerable<XElement> nestedTypedefs = e.Descendants()
//                    .Where(t => t.Name.LocalName == TypedefIdentifier);
//                if (nestedTypedefs.Count() != 0)
//                {
//                    string fmt = "Nested type definitions are not allowed.";
//                    string msg = XmlUtils.BuildXmlErrorMsg(nestedTypedefs.ElementAt(0), fmt);
//                    throw new TemplateException(msg);
//                }

//                // Parse type definition
//                ParseStructElement(e, true);
//            }

//            XAttribute kindAttr = e.Attribute(KindIdentifier);
//            XAttribute nameAttr = e.Attribute(NameIdentifier);
//            BuiltinType kind;

//            // Ensure type isn't already defined
//            if (LookupType(nameValue, out kind))
//            {
//                string fmt = "Type '{0}' has already been defined.";
//                throw new TemplateException(XmlUtils.BuildXmlErrorMsg(nameAttr, fmt, nameValue));
//            }

//            // Ensure new type is built from some pre-existing type
//            if (!LookupType(kindValue, out kind))
//            {
//                string fmt = "Unknown type '{0}'.";
//                throw new TemplateException(XmlUtils.BuildXmlErrorMsg(kindAttr, fmt, kindValue));
//            }

//            // If new type descends from another user-defined type,
//            // get member elements from that type
//            IEnumerable<XElement> members;
//            CustomTypeInfo existingTypeInfo;
//            if (customTypeMap.TryGetValue(kindValue, out existingTypeInfo))
//            {
//                members = existingTypeInfo.Members;
//            }
//            else
//            {
//                members = e.Elements();
//            }

//            // Store custom type
//            int size = SizeOf(e);
//            CustomTypeInfo newTypeInfo = new CustomTypeInfo(kind, members, size);
//            customTypeMap[nameValue] = newTypeInfo;

//            // DEBUG: show custom type mapping to its root builtin type
//            Console.WriteLine("{0} => {1}  ({2} bytes)", nameValue, kind.ToString().ToLower(), size);
//        }

//        private bool LookupType(string identifier, out BuiltinType type)
//        {
//            // Look up in custom type list
//            CustomTypeInfo info;
//            bool found = customTypeMap.TryGetValue(identifier, out info);
//            if (found)
//            {
//                type = info.Kind;
//                return true;
//            }

//            // Look up in builtin type list
//            return BuiltinTypeIdentifierMap.TryGetValue(identifier, out type);
//        }

//        private int SizeOf(XElement e)
//        {
//            string typeIdentifier = e.Name.LocalName;

//            CustomTypeInfo customTypeInfo;
//            bool isUserDefinedType = customTypeMap.TryGetValue(typeIdentifier, out customTypeInfo);
//            if (isUserDefinedType)
//            {
//                return customTypeInfo.Size;
//            }

//            BuiltinType type;
//            Directive dir;
//            bool isBuiltinType = BuiltinTypeIdentifierMap.TryGetValue(typeIdentifier, out type);
//            bool isDirective = DirectiveIdentifierMap.TryGetValue(typeIdentifier, out dir);

//            XAttribute countAttr = e.Attribute(CountIdentifier);
//            int count = 1;

//            // TODO: make func bool GetCountValue(XElement, bool allowVariables, out value);
//            if (countAttr != null)
//            {
//                bool countValid = int.TryParse(countAttr.Value, out count);
//                if (!countValid || count < 0)
//                {
//                    string fmt = "'{0}' must be a non-negative integer.";
//                    throw new TemplateException(XmlUtils.BuildXmlErrorMsg(countAttr, fmt, CountIdentifier));
//                }
//            }

//            if (isDirective)
//            {
//                if (dir != Directive.Align && dir != Directive.Typedef)
//                {
//                    return 0;   // Directives other than 'align' and 'typedef' do not add to the size
//                }

//                XAttribute kindAttr = e.Attribute(KindIdentifier);
//                type = BuiltinType.Int8;    // Default
//                if (kindAttr != null)
//                {
//                    bool kindValid = LookupType(kindAttr.Value, out type);
//                    if (!kindValid)
//                    {
//                        string fmt = "Unknown type '{0}'.";
//                        throw new TemplateException(XmlUtils.BuildXmlErrorMsg(kindAttr, fmt, kindAttr.Value));
//                    }
//                }
//            }

//            int size = 0;
//            if (type == BuiltinType.Struct)
//            {
//                foreach (XElement memb in e.Elements())
//                {
//                    size += SizeOf(memb);
//                }
//            }
//            else
//            {
//                Type t = TemplateProcessor.TypeMap[type];
//                size = Marshal.SizeOf(t);
//            }

//            return size * count;
//        }

//        private void BuildActionMaps()
//        {
//            // Builtin types
//            builtinTypeParseActionMap[BuiltinType.Double] = ParseFloatElement;
//            builtinTypeParseActionMap[BuiltinType.Float] = ParseFloatElement;
//            builtinTypeParseActionMap[BuiltinType.Int8] = ParseIntegerElement;
//            builtinTypeParseActionMap[BuiltinType.Int16] = ParseIntegerElement;
//            builtinTypeParseActionMap[BuiltinType.Int32] = ParseIntegerElement;
//            builtinTypeParseActionMap[BuiltinType.Int64] = ParseIntegerElement;
//            builtinTypeParseActionMap[BuiltinType.Struct] = ParseStructElement;
//            builtinTypeParseActionMap[BuiltinType.UInt8] = ParseIntegerElement;
//            builtinTypeParseActionMap[BuiltinType.UInt16] = ParseIntegerElement;
//            builtinTypeParseActionMap[BuiltinType.UInt32] = ParseIntegerElement;
//            builtinTypeParseActionMap[BuiltinType.UInt64] = ParseIntegerElement;

//            // Directives
//            directiveParseActionMap[Directive.Align] = ParseAlignElement;
//            directiveParseActionMap[Directive.Echo] = ParseEchoElement;
//            directiveParseActionMap[Directive.Typedef] = ParseTypedefElement;
//        }
//    }

//    class SymbolTable
//    {
//        private Dictionary<string, XElement> symbols;
//        private IEnumerable<SymbolTable> childTables;

//        public SymbolTable()
//        {
//            symbols = new Dictionary<string, XElement>();
//            childTables = new List<SymbolTable>();
//        }

//        public XElement this[string name]
//        {
//            get
//            {
//                XElement val;
//                if (symbols.TryGetValue(name, out val))
//                {
//                    return val;
//                }

//                foreach (SymbolTable childTable in childTables)
//                {
//                    val = childTable[name];
//                    if (val != null)
//                    {
//                        break;
//                    }
//                }

//                return val;
//            }
//            set
//            {

//            }
//        }

//    }
//}