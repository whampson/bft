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
using static WHampson.Cascara.ReservedWords;

namespace WHampson.Cascara
{
    /// <summary>
    /// Contains information about the organization of a binary file.
    /// </summary>
    /// <remarks>
    /// A <see cref="BinaryLayout"/> is represented on disk with an XML file.
    /// </remarks>
    public sealed class BinaryLayout : IEquatable<BinaryLayout>
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

        private BinaryLayout(XDocument xDoc, string sourcePath)
        {
            SourcePath = sourcePath;
            LayoutData = new XmlStatement(xDoc.Root);

            if (!LayoutData.Parameters.TryGetValue(Parameters.Name, out string name))
            {
                throw LayoutException.Create<LayoutException>(this, xDoc, Resources.LayoutExceptionMissingRequiredAttribute, Parameters.Name);
            }
            Name = name;
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
                if (LayoutData.Parameters.TryGetValue(key, out string value))
                {
                    return value;
                }
                else
                {
                    return "";
                }
            }
        }

        /// <summary>
        /// Gets the name of this <see cref="BinaryLayout"/>.
        /// </summary>
        public string Name
        {
            get;
        }

        /// <summary>
        /// Gets the path to the file from which this <see cref="BinaryLayout"/> was loaded.
        /// Will return <c>null</c> if the <see cref="BinaryLayout"/> was not loaded from
        /// a file on disk.
        /// </summary>
        public string SourcePath
        {
            get;
        }

        internal LayoutVersion Version
        {
            get;
        }

        internal Statement LayoutData
        {
            get;
        }

        private void Initialize()

        { }

        public bool Equals(BinaryLayout other)
        {
            if (other == null)
            {
                return false;
            }

            return LayoutData.Equals(other.LayoutData);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is BinaryLayout))
            {
                return false;
            }
            else if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return Equals(obj as BinaryLayout);
        }
    }

    internal struct LayoutVersion : IComparable<LayoutVersion>
    {
        public LayoutVersion(int major, int minor)
        {
            Major = major;
            Minor = minor;
        }

        public int Major { get; }
        public int Minor { get; }

        public override bool Equals(object obj)
        {
            if (!(obj is LayoutVersion))
            {
                return false;
            }

            LayoutVersion other = (LayoutVersion) obj;

            return Major == other.Major && Minor == other.Minor;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 37) ^ Major;
                hash = (hash * 37) ^ Minor;

                return hash;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}: [ {1} = {2}, {3} = {4} ]",
                GetType().Name,
                nameof(Major), Major,
                nameof(Minor), Minor);
        }

        public int CompareTo(LayoutVersion other)
        {
            if (this > other)
            {
                return -1;
            }

            if (this == other)
            {
                return 0;
            }

            return 1;
        }

        public static bool operator ==(LayoutVersion a, LayoutVersion b)
        {
            return a.Major == b.Major && a.Minor == b.Minor;
        }

        public static bool operator !=(LayoutVersion a, LayoutVersion b)
        {
            return !(a == b);
        }

        public static bool operator >(LayoutVersion a, LayoutVersion b)
        {
            if (a.Major > b.Major)
            {
                return true;
            }
            else if (a.Major < b.Major)
            {
                return false;
            }

            return a.Minor > b.Minor;
        }

        public static bool operator <(LayoutVersion a, LayoutVersion b)
        {
            if (a.Major < b.Major)
            {
                return true;
            }
            else if (a.Major > b.Major)
            {
                return false;
            }

            return a.Minor < b.Minor;
        }

        public static bool operator >=(LayoutVersion a, LayoutVersion b)
        {
            return a > b || a == b;
        }

        public static bool operator <=(LayoutVersion a, LayoutVersion b)
        {
            return a < b || a == b;
        }
    }
}
