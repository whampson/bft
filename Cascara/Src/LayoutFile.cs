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
using System.Xml;
using System.Xml.Linq;

namespace WHampson.Cascara
{
    /// <summary>
    /// Represents a Layout File, used for defining the structure of a binary file.
    /// </summary>
    public sealed class LayoutFile
    {
        /// <summary>
        /// Loads a Layout File from the filesystem.
        /// </summary>
        /// <param name="layoutFilePath">
        /// The path to the layout file.
        /// </param>
        /// <returns>
        /// A <see cref="LayoutFile"/> object containing the layout information
        /// from the provided file.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if the XML path is invalid.
        /// </exception>
        /// <exception cref="XmlException">
        /// If an error occurs while loading the Layout XML file.
        /// </exception>
        public static LayoutFile Load(string layoutFilePath)
        {
            return new LayoutFile(OpenXmlFile(layoutFilePath));
        }

        /// <summary>
        /// Creates a new <see cref="LayoutFile"/> object
        /// from an XML data string.
        /// </summary>
        /// <param name="xmlData">
        /// The contents of an XML file containing the layout information.
        /// </param>
        public LayoutFile(string xmlData)
            : this(XDocument.Parse(xmlData))
        { }

        /// <summary>
        /// Creates a new <see cref="LayoutFile"/> object
        /// from an existing <see cref="XDocument"/> object.
        /// </summary>
        /// <param name="xDoc">
        /// An <see cref="XDocument"/> object containing the
        /// Layout File XML data.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If the provided <see cref="XDocument"/> object is <code>null</code>.
        /// </exception>
        public LayoutFile(XDocument xDoc)
        {
            Document = xDoc ?? throw new ArgumentNullException("xDoc");
        }

        /// <summary>
        /// Gets the <see cref="XDocument"/> associated with
        /// this <see cref="LayoutFile"/> file.
        /// </summary>
        internal XDocument Document
        {
            get;
        }

        public override int GetHashCode()
        {
            return Document.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is LayoutFile))
            {
                return false;
            }

            LayoutFile lf = (LayoutFile) obj;
            return lf.Document.Equals(Document);
        }

        /// <summary>
        /// Loads data from an XML file located at the specified path.
        /// </summary>
        /// <param name="path">
        /// The path to the XML file to load.
        /// </param>
        /// <returns>
        /// The loaded XML data as an <see cref="XDocument"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if the XML path is invalid.
        /// </exception>
        /// <exception cref="XmlException">
        /// Thrown if there is an error while loading the XML document.
        /// </exception>
        private static XDocument OpenXmlFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Path cannot be empty or null");
            }


            return XDocument.Load(path, LoadOptions.SetLineInfo);
        }
    }
}
