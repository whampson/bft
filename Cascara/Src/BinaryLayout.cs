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
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using System.Xml.Linq;

[assembly: InternalsVisibleTo("Cascara.Tests")]

namespace WHampson.Cascara
{
    /// <summary>
    /// Contains information about the organization of a binary file.
    /// </summary>
    /// <remarks>
    /// A <see cref="BinaryLayout"/> is represented on disk with an XML file.
    /// </remarks>
    public sealed class BinaryLayout
    {
        /// <summary>
        /// Creates a new <see cref="BinaryLayout"/> object using data from
        /// an existing XML file.
        /// </summary>
        /// <param name="xmlPath">
        /// The path to the XML file to load.
        /// </param>
        /// <returns>
        /// The newly-created <see cref="BinaryLayout"/> object.
        /// </returns>
        /// <exception cref="XmlException">
        /// Thrown if the XML data is malformatted.
        /// </exception>
        /// <exception cref="LayoutException">
        /// Thrown if the XML data does not contain valid <see cref="BinaryLayout"/> information.
        /// </exception>
        public static BinaryLayout Load(string xmlPath)
        {
            if (string.IsNullOrWhiteSpace(xmlPath))
            {
                throw new ArgumentException(Resources.ArgumentExceptionEmptyPath, nameof(xmlPath));
            }

            XDocument doc;
            try
            {
                doc = XDocument.Load(xmlPath, LoadOptions.SetLineInfo);
            }
            catch (XmlException e)
            {
                throw LayoutException.Create<LayoutException>(null, e, null, Resources.LayoutExceptionXmlLoadFailure);
            }

            return new BinaryLayout(doc, null);
        }

        /// <summary>
        /// Creates a new <see cref="BinaryLayout"/> from the given XML string.
        /// </summary>
        /// <param name="xmlData">
        /// The XML string to parse.
        /// </param>
        /// <returns>
        /// The newly-created <see cref="BinaryLayout"/> object.
        /// </returns>
        /// <exception cref="XmlException">
        /// Thrown if the XML data is malformatted.
        /// </exception>
        /// <exception cref="LayoutException">
        /// Thrown if the XML data does not contain valid <see cref="BinaryLayout"/> information.
        /// </exception>
        public static BinaryLayout Create(string xmlData)
        {
            if (string.IsNullOrWhiteSpace(xmlData))
            {
                throw new ArgumentException(Resources.ArgumentExceptionEmptyXmlData, nameof(xmlData));
            }

            XDocument doc;
            try
            {
                doc = XDocument.Parse(xmlData, LoadOptions.SetLineInfo);
            }
            catch (XmlException e)
            {
                throw LayoutException.Create<LayoutException>(null, e, null, Resources.LayoutExceptionXmlLoadFailure);
            }

            return new BinaryLayout(doc, null);
        }

        private delegate void ElementAnalysisAction(CascaraElement e, XElement xe);
        private delegate void AttributeAnalysisAction(CascaraModifier m, XAttribute xa);

        private Dictionary<string, BinaryLayout> includedLayouts;
        private Dictionary<string, double> locals;
        private Stack<Symbol> symbolStack;

        private BinaryLayout(XDocument xDoc, string sourcePath)
        {
            Document = xDoc ?? throw new ArgumentNullException(nameof(xDoc));

            includedLayouts = new Dictionary<string, BinaryLayout>();
            locals = new Dictionary<string, double>();
            symbolStack = new Stack<Symbol>();

            SourcePath = sourcePath;
            ValidElements = new Dictionary<string, CascaraElement>();
            ElementAnalysisActions = new Dictionary<string, ElementAnalysisAction>();
            AttributeAnalysisActions = new Dictionary<string, AttributeAnalysisAction>();

            InitializeValidElementsMap();
            InitializeElementAnalysisActionMap();
            InitializeAttributeAnalysisActionMap();

            ValidateRootElement();
            Name = Document.Root.Attribute(Keywords.Name).Value;

            Preprocess(Document);
        }

        /// <summary>
        /// Gets metadata associated with this <see cref="BinaryLayout"/>.
        /// </summary>
        /// <remarks>
        /// Metadata is encoded as XML attributes in the root element.
        /// </remarks>
        /// <param name="key">
        /// The name of the metadata element to retrieve.
        /// </param>
        /// <returns>
        /// The data associated with the specified <paramref name="key"/>.
        /// An empty string if such metadata does not exist.
        /// </returns>
        public string this[string key]
        {
            get
            {
                XAttribute attr;
                if ((attr = Document.Root.Attribute(key)) == null)
                {
                    return "";
                }
                else
                {
                    return attr.Value;
                }
            }
        }

        /// <summary>
        /// Gets the name of this <see cref="BinaryLayout"/>.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the path to the file from which this <see cref="BinaryLayout"/> was loaded.
        /// Will return <c>null</c> if the <see cref="BinaryLayout"/> was not loaded from
        /// a file on disk.
        /// </summary>
        public string SourcePath
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the XML document containing the layout information.
        /// </summary>
        internal XDocument Document
        {
            get;
        }

        internal Dictionary<string, CascaraElement> ValidElements
        {
            get;
        }

        internal Dictionary<string, CascaraType> DataTypes
        {
            get
            {
                return ValidElements
                    .Where(p => p.Value is DataTypeElement && ((DataTypeElement) p.Value).Type != null)
                    .ToDictionary(p => p.Key, q => ((DataTypeElement) q.Value).Type);
            }
        }

        private Dictionary<string, ElementAnalysisAction> ElementAnalysisActions
        {
            get;
        }

        private Dictionary<string, AttributeAnalysisAction> AttributeAnalysisActions
        {
            get;
        }

        private Symbol CurrentSymbolTable
        {
            get
            {
                return (symbolStack.Count == 0) ? null : symbolStack.Peek();
            }
        }

        private void ValidateRootElement()
        {
            // Ensure correct element name
            if (Document.Root.Name.LocalName != Keywords.DocumentRoot)
            {
                string fmt = Resources.LayoutExceptionInvalidRootElement;
                throw LayoutException.Create<LayoutException>(this, Document.Root, fmt, Keywords.DocumentRoot);
            }

            // Ensure 'name' attribute exists
            if (Document.Root.Attribute(Keywords.Name) == null)
            {
                string fmt = Resources.LayoutExceptionMissingRequiredAttribute;
                throw LayoutException.Create<LayoutException>(this, Document.Root, fmt, Keywords.Name);
            }

            // Ensure layout is not empty
            if (!HasChildren(Document.Root))
            {
                throw LayoutException.Create<LayoutException>(this, Document.Root, Resources.LayoutExceptionEmptyLayout);
            }
        }

        private void Preprocess(XDocument doc)
        {
            if (includedLayouts.ContainsKey(Name))
            {
                XAttribute nameAttr = doc.Root.Attribute(Keywords.Name);
                string fmt = Resources.LayoutExceptionLayoutExists;
                throw LayoutException.Create<LayoutException>(this, nameAttr, fmt, Name);
            }

            includedLayouts[Name] = this;

            symbolStack.Push(Symbol.CreateRootSymbol());
            AnalyzeStructMembers(doc.Root);
        }

        private void AnalyzeStructElement(CascaraElement e, XElement xe)
        {
            if (!HasChildren(xe))
            {
                throw LayoutException.Create<LayoutException>(this, xe, Resources.LayoutExceptionEmptyStruct);
            }

            AnalyzeAttributes(e, xe);
            AnalyzeStructMembers(xe);
        }

        private void AnalyzeStructMembers(XElement elem)
        {
            foreach (XElement memberElem in elem.Elements())
            {
                string elemName = memberElem.Name.LocalName;

                // Check for valid element
                bool isValidElem = ValidElements.TryGetValue(elemName, out CascaraElement e);
                if (!isValidElem)
                {
                    string fmt = Resources.LayoutExceptionUnknownType;
                    throw LayoutException.Create<LayoutException>(this, memberElem, fmt, elemName);
                }

                // Analyze element
                ElementAnalysisAction elementAnalyzer = ElementAnalysisActions[e.Name];
                elementAnalyzer(e, memberElem);
            }
        }

        private void AnalyzeGenericElement(CascaraElement e, XElement xe)
        {
            if (HasChildren(xe))
            {
                //string fmt
            }
            AnalyzeAttributes(e, xe);
        }

        private void AnalyzeAttributes(CascaraElement e, XElement xe)
        {
            List<string> attrsSeen = new List<string>();

            foreach (CascaraModifier m in e.Modifiers)
            {
                XAttribute attr = xe.Attribute(m.Name);

                // Ensure required attributes are present
                if (attr == null)
                {
                    if (m.IsRequired)
                    {
                        string fmt = Resources.LayoutExceptionMissingRequiredAttribute;
                        throw LayoutException.Create<LayoutException>(this, xe, fmt, m.Name);
                    }
                    continue;
                }

                // Ensure attribute values are not whitespace
                if (string.IsNullOrWhiteSpace(attr.Value))
                {
                    string fmt = Resources.LayoutExceptionEmptyAttribute;
                    throw LayoutException.Create<LayoutException>(this, attr, fmt, m.Name);
                }

                // Check if attribute value contains variables.
                // Make sure they're allowed for this attribute and exist in the namespace
                if (m.CanContainVariables)
                {

                }

                attrsSeen.Add(m.Name);

                // Analyze attribute
                AttributeAnalysisAction attributeAnalyzer = AttributeAnalysisActions[m.Name];
                if (attributeAnalyzer == null)
                {
                    continue;
                }

                attributeAnalyzer(m, attr);
            }

            // Look for unknwon attributes
            IEnumerable<XAttribute> unknownAttrs = xe.Attributes()
                .Where(x => !attrsSeen.Any(y => x.Name.LocalName == y));

            if (unknownAttrs.Count() != 0)
            {
                XAttribute attr = unknownAttrs.ElementAt(0);
                string fmt = Resources.LayoutExceptionUnknownAttribute;
                throw LayoutException.Create<LayoutException>(this, attr, fmt, attr.Name.LocalName);
            }
        }

        private bool IsDefined(string variableName)
        {
            return locals.ContainsKey(variableName) || CurrentSymbolTable.Contains(variableName);
        }

        private bool HasChildren(XElement elem)
        {
            return elem != null && elem.Elements().Count() != 0;
        }

        /// <summary>
        /// Returns a unique hash code of this object based on its contents.
        /// </summary>
        /// <returns>
        /// A unique hash code of this object based on its contents.
        /// </returns>
        public override int GetHashCode()
        {
            return Name.GetHashCode() * 17
                + XNode.EqualityComparer.GetHashCode(Document);
        }

        /// <summary>
        /// Compares an <see cref="object"/> against this <see cref="BinaryLayout"/>
        /// for equality.
        /// </summary>
        /// <param name="obj">
        /// The <see cref="object"/> to compare against.
        /// </param>
        /// <returns>
        /// <code>True</code> if the objects are equal,
        /// <code>False</code> otherwise
        /// </returns>
        public override bool Equals(object obj)
        {
            // TODO: LONG TERM: define equality as describing
            // the same file structure, not as having the same XML contents
            // To do this, verify equality of:
            //   - symbols
            //   - user-defined types
            //   - min. size of data being described

            if (!(obj is BinaryLayout))
            {
                return false;
            }
            BinaryLayout other = (BinaryLayout) obj;

            return Name == other.Name
                && XNode.DeepEquals(Document, other.Document);
        }

        private void InitializeElementAnalysisActionMap()
        {
            // Primitive types
            foreach (string typeName in Keywords.DataTypes.Select(x => x.Key))
            {
                ElementAnalysisActions[typeName] = AnalyzeGenericElement;
            }

            // Structure types
            ElementAnalysisActions[Keywords.Struct] = AnalyzeStructElement;
            ElementAnalysisActions[Keywords.Union] = AnalyzeStructElement;

            // Directives
            ElementAnalysisActions[Keywords.Align] = AnalyzeGenericElement;
            ElementAnalysisActions[Keywords.Echo] = AnalyzeGenericElement;
            ElementAnalysisActions[Keywords.Include] = AnalyzeGenericElement;
            ElementAnalysisActions[Keywords.Local] = AnalyzeGenericElement;
            ElementAnalysisActions[Keywords.Typedef] = AnalyzeGenericElement;
        }

        private void InitializeAttributeAnalysisActionMap()
        {
            AttributeAnalysisActions[Keywords.Comment] = null;
            AttributeAnalysisActions[Keywords.Count] = null;    // ensure it is a non-negative integer
            AttributeAnalysisActions[Keywords.Kind] = null;     // ensure it is a valid type
            AttributeAnalysisActions[Keywords.Message] = null;
            AttributeAnalysisActions[Keywords.Name] = null;     // ensrue adheres follows naming constraints
            AttributeAnalysisActions[Keywords.Path] = null;
            AttributeAnalysisActions[Keywords.Raw] = null;      // "true" or "false"
            AttributeAnalysisActions[Keywords.Value] = null;
        }

        private void InitializeValidElementsMap()
        {
            /* ===== Data types ===== */

            // struct
            ValidElements[Keywords.Struct] = CascaraElement.CreateDataType(Keywords.Struct,
                null,
                CascaraModifier.Create(Keywords.Comment, true),
                CascaraModifier.Create(Keywords.Count, true),
                CascaraModifier.Create(Keywords.Name, false));

            // union
            ValidElements[Keywords.Union] = CascaraElement.CreateDataType(Keywords.Union,
                null,
                CascaraModifier.Create(Keywords.Comment, true),
                CascaraModifier.Create(Keywords.Count, true),
                CascaraModifier.Create(Keywords.Name, false));

            // bool8
            ValidElements[Keywords.Bool8] = CascaraElement.CreateDataType(Keywords.Bool8,
                CascaraType.CreatePrimitive(typeof(System.Boolean), 1),
                CascaraModifier.Create(Keywords.Comment, true),
                CascaraModifier.Create(Keywords.Count, true),
                CascaraModifier.Create(Keywords.Name, false));

            // bool16
            ValidElements[Keywords.Bool16] = CascaraElement.CreateDataType(Keywords.Bool16,
                CascaraType.CreatePrimitive(typeof(System.Boolean), 2),
                CascaraModifier.Create(Keywords.Comment, true),
                CascaraModifier.Create(Keywords.Count, true),
                CascaraModifier.Create(Keywords.Name, false));

            // bool32
            ValidElements[Keywords.Bool32] = CascaraElement.CreateDataType(Keywords.Bool32,
                CascaraType.CreatePrimitive(typeof(System.Boolean), 4),
                CascaraModifier.Create(Keywords.Comment, true),
                CascaraModifier.Create(Keywords.Count, true),
                CascaraModifier.Create(Keywords.Name, false));

            // bool64
            ValidElements[Keywords.Bool64] = CascaraElement.CreateDataType(Keywords.Bool64,
                CascaraType.CreatePrimitive(typeof(System.Boolean), 8),
                CascaraModifier.Create(Keywords.Comment, true),
                CascaraModifier.Create(Keywords.Count, true),
                CascaraModifier.Create(Keywords.Name, false));

            // char8
            ValidElements[Keywords.Char8] = CascaraElement.CreateDataType(Keywords.Char8,
                CascaraType.CreatePrimitive(typeof(System.Char), 1),
                CascaraModifier.Create(Keywords.Comment, true),
                CascaraModifier.Create(Keywords.Count, true),
                CascaraModifier.Create(Keywords.Name, false));

            // char16
            ValidElements[Keywords.Char16] = CascaraElement.CreateDataType(Keywords.Char16,
                CascaraType.CreatePrimitive(typeof(System.Char), 2),
                CascaraModifier.Create(Keywords.Comment, true),
                CascaraModifier.Create(Keywords.Count, true),
                CascaraModifier.Create(Keywords.Name, false));

            // double
            ValidElements[Keywords.Double] = CascaraElement.CreateDataType(Keywords.Double,
                CascaraType.CreatePrimitive(typeof(double), 8),
                CascaraModifier.Create(Keywords.Comment, true),
                CascaraModifier.Create(Keywords.Count, true),
                CascaraModifier.Create(Keywords.Name, false));

            // int8
            ValidElements[Keywords.Int8] = CascaraElement.CreateDataType(Keywords.Int8,
                CascaraType.CreatePrimitive(typeof(System.SByte), 1),
                CascaraModifier.Create(Keywords.Comment, true),
                CascaraModifier.Create(Keywords.Count, true),
                CascaraModifier.Create(Keywords.Name, false));

            // int16
            ValidElements[Keywords.Int16] = CascaraElement.CreateDataType(Keywords.Int16,
                CascaraType.CreatePrimitive(typeof(System.Int16), 2),
                CascaraModifier.Create(Keywords.Comment, true),
                CascaraModifier.Create(Keywords.Count, true),
                CascaraModifier.Create(Keywords.Name, false));

            // int32
            ValidElements[Keywords.Int32] = CascaraElement.CreateDataType(Keywords.Int32,
                CascaraType.CreatePrimitive(typeof(System.Int32), 4),
                CascaraModifier.Create(Keywords.Comment, true),
                CascaraModifier.Create(Keywords.Count, true),
                CascaraModifier.Create(Keywords.Name, false));

            // int64
            ValidElements[Keywords.Int64] = CascaraElement.CreateDataType(Keywords.Int64,
                CascaraType.CreatePrimitive(typeof(System.Int64), 8),
                CascaraModifier.Create(Keywords.Comment, true),
                CascaraModifier.Create(Keywords.Count, true),
                CascaraModifier.Create(Keywords.Name, false));

            // single
            ValidElements[Keywords.Single] = CascaraElement.CreateDataType(Keywords.Single,
                CascaraType.CreatePrimitive(typeof(System.Single), 4),
                CascaraModifier.Create(Keywords.Comment, true),
                CascaraModifier.Create(Keywords.Count, true),
                CascaraModifier.Create(Keywords.Name, false));

            // uint8
            ValidElements[Keywords.UInt8] = CascaraElement.CreateDataType(Keywords.UInt8,
                CascaraType.CreatePrimitive(typeof(System.Byte), 1),
                CascaraModifier.Create(Keywords.Comment, true),
                CascaraModifier.Create(Keywords.Count, true),
                CascaraModifier.Create(Keywords.Name, false));

            // uint16
            ValidElements[Keywords.UInt16] = CascaraElement.CreateDataType(Keywords.UInt16,
                CascaraType.CreatePrimitive(typeof(System.UInt16), 2),
                CascaraModifier.Create(Keywords.Comment, true),
                CascaraModifier.Create(Keywords.Count, true),
                CascaraModifier.Create(Keywords.Name, false));

            // uint32
            ValidElements[Keywords.UInt32] = CascaraElement.CreateDataType(Keywords.UInt32,
                CascaraType.CreatePrimitive(typeof(System.UInt32), 4),
                CascaraModifier.Create(Keywords.Comment, true),
                CascaraModifier.Create(Keywords.Count, true),
                CascaraModifier.Create(Keywords.Name, false));

            // uint64
            ValidElements[Keywords.UInt64] = CascaraElement.CreateDataType(Keywords.UInt64,
                CascaraType.CreatePrimitive(typeof(System.UInt64), 8),
                CascaraModifier.Create(Keywords.Comment, true),
                CascaraModifier.Create(Keywords.Count, true),
                CascaraModifier.Create(Keywords.Name, false));

            /* ===== Data type aliases ===== */
            ValidElements[Keywords.Bool] = ValidElements[Keywords.Bool8];
            ValidElements[Keywords.Byte] = ValidElements[Keywords.UInt8];
            ValidElements[Keywords.Char] = ValidElements[Keywords.Char8];
            ValidElements[Keywords.Float] = ValidElements[Keywords.Single];
            ValidElements[Keywords.Int] = ValidElements[Keywords.Int32];
            ValidElements[Keywords.Long] = ValidElements[Keywords.Int64];
            ValidElements[Keywords.Short] = ValidElements[Keywords.Int16];
            ValidElements[Keywords.UInt] = ValidElements[Keywords.UInt32];
            ValidElements[Keywords.ULong] = ValidElements[Keywords.UInt64];
            ValidElements[Keywords.UShort] = ValidElements[Keywords.UInt16];

            /* ===== Directives ===== */

            // align
            ValidElements[Keywords.Align] = CascaraElement.CreateDirective(Keywords.Align,
                CascaraModifier.Create(Keywords.Comment, true),
                CascaraModifier.Create(Keywords.Count, true),
                CascaraModifier.Create(Keywords.Kind, false));

            // echo
            ValidElements[Keywords.Echo] = CascaraElement.CreateDirective(Keywords.Echo,
                CascaraModifier.Create(Keywords.Comment, true),
                CascaraModifier.CreateRequired(Keywords.Message, true),
                CascaraModifier.Create(Keywords.Raw, false));

            // include
            ValidElements[Keywords.Include] = CascaraElement.CreateDirective(Keywords.Include,
                CascaraModifier.Create(Keywords.Comment, true),
                CascaraModifier.CreateRequired(Keywords.Path, false));

            // local
            ValidElements[Keywords.Local] = CascaraElement.CreateDirective(Keywords.Local,
                CascaraModifier.Create(Keywords.Comment, true),
                CascaraModifier.CreateRequired(Keywords.Name, false),
                CascaraModifier.CreateRequired(Keywords.Value, true));

            // typedef
            ValidElements[Keywords.Typedef] = CascaraElement.CreateDirective(Keywords.Typedef,
                CascaraModifier.Create(Keywords.Comment, true),
                CascaraModifier.CreateRequired(Keywords.Kind, false),
                CascaraModifier.CreateRequired(Keywords.Name, false));

        }
    }
}
