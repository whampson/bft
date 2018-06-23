#region License
/* Copyright (c) 2017-2018 Wes Hampson
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
using System.Text;
using System.Xml;
using System.Xml.Linq;
using WHampson.Cascara.Interpreter;
using WHampson.Cascara.Interpreter.Xml;
using static WHampson.Cascara.Interpreter.ReservedWords;

namespace WHampson.Cascara
{
    public abstract partial class LayoutScript
    {
        internal sealed class XmlLayoutScript : LayoutScript
        {
            public static XmlLayoutScript LoadSource(string path)
            {
                // Load XML document
                XDocument doc;
                try
                {
                    doc = XDocument.Load(path, LoadOptions.SetLineInfo);
                }
                catch (Exception e)
                {
                    if (!(e is IOException) && !(e is XmlException))
                    {
                        throw;
                    }

                    string msg = e.Message;
                    throw LayoutScriptException.Create<LayoutScriptException>(null, e, null, msg);
                }

                // Create and return layout
                return CreateXmlBinaryLayout(doc, path);
            }

            public static XmlLayoutScript ParseSource(string source)
            {
                // Parse XML document string
                XDocument doc;
                try
                {
                    doc = XDocument.Parse(source, LoadOptions.SetLineInfo);
                }
                catch (XmlException e)
                {
                    string msg = e.Message;
                    throw LayoutScriptException.Create<LayoutScriptException>(null, e, null, msg);
                }

                // Create and return layout
                return CreateXmlBinaryLayout(doc, null);
            }

            private static XmlLayoutScript CreateXmlBinaryLayout(XDocument doc, string sourcePath)
            {
                // Ensure root element is named correctly
                if (doc.Root.Name.LocalName != Keywords.XmlDocumentRoot)
                {
                    string msg = Resources.SyntaxExceptionXmlInvalidRootElement;
                    throw LayoutScriptException.Create<SyntaxException>(null, new XmlSourceEntity(doc.Root), msg, Keywords.XmlDocumentRoot);
                }

                // Read version
                Version ver = GetLayoutVersion(doc);

                // Ensure root element is not empty
                if (!doc.Root.HasElements)
                {
                    string msg = Resources.SyntaxExceptionEmptyLayout;
                    throw LayoutScriptException.Create<SyntaxException>(null, new XmlSourceEntity(doc.Root), msg);
                }

                // Create and return layout object
                return new XmlLayoutScript(doc, ver, sourcePath);
            }

            private static Version GetLayoutVersion(XDocument doc)
            {
                XAttribute versionAttr = doc.Root.Attribute(Parameters.Version);
                if (versionAttr == null)
                {
                    return Cascara.AssemblyVersion;
                }

                if (!Version.TryParse(versionAttr.Value, out Version ver))
                {
                    string msg = Resources.LayoutExceptionMalformattedLayoutVersion;
                    throw LayoutScriptException.Create<LayoutScriptException>(null, new XmlSourceEntity(versionAttr), msg, versionAttr.Value);
                }

                return ver;
            }

            private XmlLayoutScript(XDocument document, Version version, string sourcePath)
                : base(version, sourcePath)
            {
                if (document == null)
                {
                    throw new ArgumentNullException(nameof(document));
                }

                Document = document;
                Initialize();
            }

            internal XDocument Document
            {
                get;
            }

            protected override void Initialize()
            {
                // Read metadata from root element attributes
                foreach (XAttribute attr in Document.Root.Attributes())
                {
                    _metadata[attr.Name.LocalName] = attr.Value;
                }

                // Parse XML data
                RootStatement = XmlStatement.Parse(Document.Root);
            }
        }
    }
}
