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
        private delegate void ModifierAnalysisAction(CascaraModifier m, XAttribute xa);

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
            ElementDefinitions = new Dictionary<string, CascaraElement>();
            //UserDefinedTypes = new Dictionary<string, CascaraType>();
            ElementAnalysisActions = new Dictionary<string, ElementAnalysisAction>();

            InitializeValidElementsMap();
            InitializeElementAnalysisActionMap();

            ValidateRootElement();
            Name = Document.Root.Attribute(Keywords.Name).Value;

            AnalyzeLayout(Document);
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

        ///// <summary>
        ///// Gets all user-defined types defined in this <see cref="BinaryLayout"/>.
        ///// </summary>
        //internal Dictionary<string, CascaraType> UserDefinedTypes
        //{
        //    get;
        //}

        internal Dictionary<string, CascaraElement> ElementDefinitions
        {
            get;
        }

        private Dictionary<string, ElementAnalysisAction> ElementAnalysisActions
        {
            get;
        }

        private Symbol CurrentSymbolTable
        {
            get { return symbolStack.Peek(); }
        }

        private void ValidateRootElement()
        {
            if (Document.Root.Name.LocalName != Keywords.DocumentRoot)
            {
                string fmt = Resources.LayoutExceptionInvalidRootElement;
                throw LayoutException.Create<LayoutException>(this, Document.Root, fmt, Keywords.DocumentRoot);
            }

            if (Document.Root.Attribute(Keywords.Name) == null)
            {
                string fmt = Resources.LayoutExceptionMissingRequiredAttribute;
                throw LayoutException.Create<LayoutException>(this, Document.Root, fmt, Keywords.Name);
            }

            if (Document.Root.Elements().Count() == 0)
            {
                throw LayoutException.Create<LayoutException>(this, Document.Root, Resources.LayoutExceptionEmptyLayout);
            }
        }

        private void AnalyzeLayout(XDocument doc)
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

        private void AnalyzeStructMembers(XElement elem)
        {
            foreach (XElement memberElem in elem.Elements())
            {
                string elemName = memberElem.Name.LocalName;

                // Check for valid element
                bool isValidElem = ElementDefinitions.TryGetValue(elemName, out CascaraElement e);
                if (!isValidElem)
                {
                    string fmt = Resources.LayoutExceptionUnknownType;
                    throw LayoutException.Create<LayoutException>(this, memberElem, fmt, elemName);
                }

                // Resolve aliases
                while (e.IsAlias)
                {
                    e = ElementDefinitions[e.AliasTarget];
                }

                // Analyze element
                ElementAnalysisAction elementAnalyzer = ElementAnalysisActions[e.Name];
                elementAnalyzer(e, memberElem);
            }
        }

        private void AnalyzeGenericElement(CascaraElement e, XElement xe)
        {
            Console.WriteLine("Analyzing element '{0}'", e.Name);
        }

        private void ValidateAttributes(IEnumerable<XAttribute> attrs, CascaraModifier[] validAttrs)
        {
            // TODO: Ensure required attributes are present

            foreach (XAttribute attr in attrs)
            {
                string attrName = attr.Name.LocalName;
                Console.WriteLine("Validating attribute '{0}'", attrName);

                bool isAttrValid = validAttrs.Any(m => m.Name == attrName);
                if (!isAttrValid)
                {
                    string fmt = Resources.LayoutExceptionUnknownAttribute;
                    throw LayoutException.Create<LayoutException>(this, attr, fmt, attrName);
                }
                else if (string.IsNullOrWhiteSpace(attr.Value))
                {
                    string fmt = Resources.LayoutExceptionEmptyAttribute;
                    throw LayoutException.Create<LayoutException>(this, attr, fmt, attrName);
                }
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
            ElementAnalysisActions[Keywords.Bool8] = AnalyzeGenericElement;
            ElementAnalysisActions[Keywords.Align] = AnalyzeGenericElement;
            ElementAnalysisActions[Keywords.Echo] = AnalyzeGenericElement;
            ElementAnalysisActions[Keywords.Include] = AnalyzeGenericElement;
            ElementAnalysisActions[Keywords.Local] = AnalyzeGenericElement;
            ElementAnalysisActions[Keywords.Typedef] = AnalyzeGenericElement;
        }

        private void InitializeValidElementsMap()
        {
            // Data types
            ElementDefinitions[Keywords.Bool8] = CascaraElement.CreateDataType(Keywords.Bool8,
                CascaraType.CreatePrimitive(typeof(bool), 1),
                CascaraModifier.Create(Keywords.Comment, true),
                CascaraModifier.Create(Keywords.Count, true),
                CascaraModifier.Create(Keywords.Name, false));

            // Data type aliases
            ElementDefinitions[Keywords.Bool] = CascaraElement.CreateDataTypeAlias(Keywords.Bool, Keywords.Bool8);

            // Directives
            ElementDefinitions[Keywords.Align] = CascaraElement.CreateDirective(Keywords.Align,
                CascaraModifier.Create(Keywords.Comment, true),
                CascaraModifier.Create(Keywords.Count, true),
                CascaraModifier.Create(Keywords.Kind, false));
            ElementDefinitions[Keywords.Echo] = CascaraElement.CreateDirective(Keywords.Echo,
                CascaraModifier.Create(Keywords.Comment, true),
                CascaraModifier.CreateRequired(Keywords.Message, true),
                CascaraModifier.Create(Keywords.Raw, false));
            ElementDefinitions[Keywords.Include] = CascaraElement.CreateDirective(Keywords.Include,
                CascaraModifier.Create(Keywords.Comment, true),
                CascaraModifier.CreateRequired(Keywords.Path, false));
            ElementDefinitions[Keywords.Local] = CascaraElement.CreateDirective(Keywords.Local,
                CascaraModifier.Create(Keywords.Comment, true),
                CascaraModifier.CreateRequired(Keywords.Name, false),
                CascaraModifier.CreateRequired(Keywords.Value, true));
            ElementDefinitions[Keywords.Typedef] = CascaraElement.CreateDirective(Keywords.Typedef,
                CascaraModifier.Create(Keywords.Comment, true),
                CascaraModifier.CreateRequired(Keywords.Kind, false),
                CascaraModifier.CreateRequired(Keywords.Name, false));

        }
    }
}
