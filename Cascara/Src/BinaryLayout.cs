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
        /// <summary>
        /// Creates a <see cref="BinaryLayout"/> object from the data stored in
        /// the XML file located at the specified path.
        /// </summary>
        /// <param name="xmlPath">
        /// The path to the XML file containing the binary layout information.
        /// </param>
        /// <returns>
        /// A <see cref="BinaryLayout"/> object containing the layout information
        /// for a specific binary file format.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if the XML path is empty or null.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// Thrown if the path provided does not exist.
        /// </exception>
        /// <exception cref="XmlException">
        /// Thrown if there is an error while loading the XML document.
        /// </exception>
        /// <exception cref="LayoutException">
        /// Thrown if the binary layout XML data contains an error.
        /// </exception>
        public static BinaryLayout Load(string xmlPath)
        {
            if (string.IsNullOrWhiteSpace(xmlPath))
            {
                throw new ArgumentException("Path cannot be empty or null.", nameof(xmlPath));
            }
            XDocument doc = XDocument.Load(xmlPath, LoadOptions.SetLineInfo);
            
            return new BinaryLayout(doc) { SourcePath = xmlPath };
        }

        /// <summary>
        /// Creates a new <see cref="BinaryLayout"/> object from an XML data string.
        /// </summary>
        /// <param name="xmlData">
        /// An XML string containing the binary layout information.
        /// </param>
        /// <exception cref="XmlException">
        /// Thrown if there is an error while loading the XML document.
        /// </exception>
        /// <exception cref="LayoutException">
        /// Thrown if the binary layout XML data contains an error.
        /// </exception>
        public BinaryLayout(string xmlData)
            : this(XDocument.Parse(xmlData))
        {
        }

        /// <summary>
        /// Creates a new <see cref="BinaryLayout"/> object from the XML data
        /// contained within an <see cref="XDocument"/>.
        /// </summary>
        /// <param name="xDoc">
        /// An <see cref="XDocument"/> containing the binary layout XML data.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the provided <see cref="XDocument"/> object is <code>null</code>.
        /// </exception>
        /// <exception cref="LayoutException">
        /// Thrown if the binary layout XML data contains an error.
        /// </exception>
        public BinaryLayout(XDocument xDoc)
        {
            Document = xDoc ?? throw new ArgumentNullException(nameof(xDoc));
            ValidateRootElement();

            SourcePath = null;
            Name = Document.Root.Attribute(Keywords.Name).Value;

            // TODO: Check entire layout for invalid names and syntax errors
        }

        /// <summary>
        /// Gets the name of this <see cref="BinaryLayout"/>.
        /// </summary>
        public string Name
        {
            get;
        }

        /// <summary>
        /// Gets the file path used to load this <see cref="BinaryLayout"/>.
        /// Returns <code>null</code> if the Layout File was not loaded 
        /// </summary>
        public string SourcePath
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the <see cref="XDocument"/> associated with this <see cref="BinaryLayout"/>.
        /// </summary>
        internal XDocument Document
        {
            get;
        }

        /// <summary>
        /// Ensures the root element of <see cref="Document"/> is correct.
        /// </summary>
        /// <exception cref="LayoutException">
        /// If the root element is empty, has the wrong name, or is missing
        /// the 'name' attribute.
        /// </exception>
        private void ValidateRootElement()
        {
            if (Document.Root.Name.LocalName != Keywords.DocumentRoot)
            {
                string fmt = "Layout Files must have a root element named '{0}'.";
                throw LayoutException.Create(this, Document.Root, fmt, Keywords.DocumentRoot);
            }

            if (Document.Root.Elements().Count() == 0)
            {
                throw new LayoutException(this, "Empty Layout File.");
            }

            if (Document.Root.Attribute(Keywords.Name) == null)
            {
                string fmt = "Missing required attribute '{0}'.";
                throw LayoutException.Create(this, Document.Root, fmt, Keywords.Name);
            }
        }

        /// <summary>
        /// Returns a unique hash code of this object based on its contents.
        /// </summary>
        /// <returns>
        /// A unique hash code of this object based on its contents.
        /// </returns>
        public override int GetHashCode()
        {
            return XNode.EqualityComparer.GetHashCode(Document);
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
            if (!(obj is BinaryLayout))
            {
                return false;
            }

            return XNode.DeepEquals(Document, ((BinaryLayout) obj).Document);
        }
    }
}
