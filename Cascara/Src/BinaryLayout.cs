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
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

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
        public static BinaryLayout Load(string xmlPath)
        {
            if (string.IsNullOrWhiteSpace(xmlPath))
            {
                throw new ArgumentException("Path cannot be empty or null.", nameof(xmlPath));
            }

            try
            {
                XDocument doc = XDocument.Load(xmlPath, LoadOptions.SetLineInfo);
                return new BinaryLayout(doc, xmlPath);
            }
            catch (Exception e)
            {
                throw LayoutException.Create(null, e, null, "Failed to load layout.");
            }
        }

        private Dictionary<string, BinaryLayout> includedLayouts;
        private Dictionary<string, double> locals;
        private Stack<Symbol> symbolStack;

        public BinaryLayout(string xmlData)
            : this(XDocument.Parse(xmlData))
        {
        }

        public BinaryLayout(XDocument xDoc)
            : this(xDoc, null)
        {
        }

        private BinaryLayout(XDocument xDoc, string sourcePath)
        {
            Document = xDoc ?? throw new ArgumentNullException(nameof(xDoc));
            ValidateRootElement();

            includedLayouts = new Dictionary<string, BinaryLayout>();
            locals = new Dictionary<string, double>();
            symbolStack = new Stack<Symbol>();

            SourcePath = sourcePath;
            Name = Document.Root.Attribute(Keywords.Name).Value;
            UserDefinedTypes = new Dictionary<string, TypeDefinition>();

            // TODO: Check entire layout for invalid names and syntax errors

            try
            {
                AnalyzeLayout(Document);
            }
            catch (Exception e)
            {
                throw LayoutException.Create(null, e, null, "Layout validation failure.");
            }
        }

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

        public string Name
        {
            get;
        }

        public string SourcePath
        {
            get;
            private set;
        }

        internal XDocument Document
        {
            get;
        }

        internal Dictionary<string, TypeDefinition> UserDefinedTypes
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
                string fmt = "Must have a root element named '{0}'.";
                throw LayoutException.Create(this, Document.Root, fmt, Keywords.DocumentRoot);
            }

            if (Document.Root.Attribute(Keywords.Name) == null)
            {
                string fmt = "Missing required attribute '{0}'.";
                throw LayoutException.Create(this, Document.Root, fmt, Keywords.Name);
            }

            if (Document.Root.Elements().Count() == 0)
            {
                throw LayoutException.Create(this, Document.Root, "Empty layout.");
            }
        }

        private void AnalyzeLayout(XDocument doc)
        {
            //ValidateRootElement();

            // TODO: handle 'include' cycles

            symbolStack.Push(Symbol.CreateRootSymbol());
            AnalyzeStructMembers(doc.Root);
        }

        private void AnalyzeElement(XElement elem)
        {
            string elemName = elem.Name.LocalName;

            // Analyze 'struct' and 'union'
            if (elemName == Keywords.Struct || elemName == Keywords.Union)
            {
                AnalyzeStruct(elem);
                return;
            }

            // Ensure element name corresponds to either a primitive type,
            // user-defined type, or directive
            bool isUserDefinedType = UserDefinedTypes.TryGetValue(elemName, out TypeDefinition typeDef);
            bool isDirective = Keywords.Directives.ContainsKey(elemName);
            if (!isUserDefinedType && !isDirective)
            {
                string fmt = "Unknown type or directive '{0}'.";
                throw LayoutException.Create(this, elem, fmt, elemName);
            }

            if (isDirective)
            {
                // AnalyzeDirective(elem);
                return;
            }

            // Ensure element has no child elements (only allowed on structures)
            if (HasChildren(elem))
            {
                string fmt = "Type '{0}' cannot contain child elements.";
                throw LayoutException.Create(this, elem, fmt, elemName);
            }

            if (isUserDefinedType && typeDef.IsStruct)
            {
                // Analuze user-defined structure type
                // "Copy and paste" members from type definition into copy of current element
                XElement copy = new XElement(elem);
                copy.Add(typeDef.Members);
                AnalyzeStruct(copy);
            }
            else
            {
                AnalyzePrimitiveType(elem);
            }
        }

        private void AnalyzeStruct(XElement elem)
        {
            AnalyzeAttributes(elem, Keywords.Comment, Keywords.Count, Keywords.Name);

            string name = null;
            if (elem.Attribute(Keywords.Name) != null)
            {
                name = elem.Attribute(Keywords.Name).Value;
            }

            Symbol newTable;
            if (name != null)
            {
                if (IsDefined(name))
                {
                    string fmt = "Variable '{0}' previously defined.";
                    throw LayoutException.Create(this, elem, fmt, name);
                }

                newTable = CurrentSymbolTable.Insert(name);
            }
            else
            {
                newTable = Symbol.CreateNamelessSymbol(CurrentSymbolTable);
            }

            symbolStack.Push(newTable);
            AnalyzeStructMembers(elem);
            symbolStack.Pop();
        }

        private void AnalyzeStructMembers(XElement elem)
        {
            foreach (XElement memberElem in elem.Elements())
            {
                AnalyzeElement(memberElem);
            }
        }

        private void AnalyzePrimitiveType(XElement elem)
        {
            AnalyzeAttributes(elem, Keywords.Comment, Keywords.Count, Keywords.Name);

            string name = null;
            if (elem.Attribute(Keywords.Name) != null)
            {
                name = elem.Attribute(Keywords.Name).Value;
            }

            if (name != null)
            {
                if (IsDefined(name))
                {
                    string fmt = "Variable '{0}' previously defined.";
                    throw LayoutException.Create(this, elem, fmt, name);
                }

                CurrentSymbolTable.Insert(name);
            }
        }

        private void AnalyzeAttributes(XElement elem, params string[] validAttributes)
        {
            foreach (XAttribute attr in elem.Attributes())
            {
                string name = attr.Name.LocalName;
                if (!validAttributes.Contains(name))
                {
                    string fmt = "Unknown attribute '{0}'.";
                    throw LayoutException.Create(this, attr, fmt, name);
                }
                else if (string.IsNullOrWhiteSpace(attr.Value))
                {
                    string fmt = "Attribute '{0}' cannot have an empty value.";
                    throw LayoutException.Create(this, attr, fmt, name);
                }

                // TODO: ensure attribute values have the correct type snd varnames are correct
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
    }
}
