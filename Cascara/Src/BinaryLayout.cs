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
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using WHampson.Cascara.Interpreter;
using WHampson.Cascara.Interpreter.Xml;
using static WHampson.Cascara.Interpreter.ReservedWords;

namespace WHampson.Cascara
{
    /// <summary>
    /// Contains information about the organization of a binary file.
    /// </summary>
    /// <remarks>
    /// A <see cref="BinaryLayout"/> is represented on disk with an XML file.
    /// </remarks>
    public abstract partial class BinaryLayout : IEquatable<BinaryLayout>
    {
        /// <summary>
        /// Creates a new <see cref="BinaryLayout"/> object from a file.
        /// </summary>
        /// <param name="path">The path to the file to load.</param>
        /// <returns>The newly-created <see cref="BinaryLayout"/> object.</returns>
        /// <exception cref="LayoutException">
        /// Thrown if the <see cref="BinaryLayout"/> is empty, does not have a name,
        /// contains a malformatted version, or contains a syntax error.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if the path is empty or null.
        /// </exception>
        public static BinaryLayout Load(string path)
        {
            return Load(path, LayoutFormat.Xml);
        }

        /// <summary>
        /// Creates a new <see cref="BinaryLayout"/> object from a file.
        /// </summary>
        /// <param name="path">The path to the file to load.</param>
        /// <param name="format">The source code format.</param>
        /// <returns>The newly-created <see cref="BinaryLayout"/> object.</returns>
        /// <exception cref="LayoutException">
        /// Thrown if the <see cref="BinaryLayout"/> is empty, does not have a name,
        /// contains a malformatted version, or contains a syntax error.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if the path is empty or null.
        /// </exception>
        internal static BinaryLayout Load(string path, LayoutFormat type)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException(Resources.ArgumentExceptionEmptyString, nameof(path));
            }

            switch (type)
            {
                case LayoutFormat.Xml:
                    return XmlBinaryLayout.LoadSource(path);
                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Creates a new <see cref="BinaryLayout"/> object from a string.
        /// </summary>
        /// <param name="source">The <see cref="BinaryLayout"/> source code string.</param>
        /// <returns>The newly-created <see cref="BinaryLayout"/> object.</returns>
        /// <exception cref="LayoutException">
        /// Thrown if the <see cref="BinaryLayout"/> is empty, does not have a name,
        /// contains a malformatted version, or contains a syntax error.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if the source string is empty or null.
        /// </exception>
        public static BinaryLayout Parse(string source)
        {
            return Parse(source, LayoutFormat.Xml);
        }

        /// <summary>
        /// Creates a new <see cref="BinaryLayout"/> object from a string.
        /// </summary>
        /// <param name="source">The <see cref="BinaryLayout"/> source code string.</param>
        /// <param name="format">The source code format.</param>
        /// <returns>The newly-created <see cref="BinaryLayout"/> object.</returns>
        /// <exception cref="LayoutException">
        /// Thrown if the <see cref="BinaryLayout"/> is empty, does not have a name,
        /// contains a malformatted version, or contains a syntax error.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if the source string is empty or null.
        /// </exception>
        internal static BinaryLayout Parse(string source, LayoutFormat format)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                throw new ArgumentException(Resources.ArgumentExceptionEmptyString, nameof(source));
            }

            switch (format)
            {
                case LayoutFormat.Xml:
                    return XmlBinaryLayout.ParseSource(source);
                default:
                    throw new NotSupportedException();
            }
        }

        protected Dictionary<string, string> _metadata;

        /// <summary>
        /// Creates a new <see cref="BinaryLayout"/> object.
        /// </summary>
        /// <param name="name">The name of the layout.</param>
        /// <param name="version">The Cascara version that the layout is designed for.</param>
        /// <param name="sourcePath">The path to the source file (if applicable).</param>
        private BinaryLayout(string name, Version version, string sourcePath)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(Resources.ArgumentExceptionEmptyString, nameof(name));
            }

            if (version == null)
            {
                throw new ArgumentNullException(nameof(version));
            }

            Name = name;
            Version = version;
            SourcePath = sourcePath;
            _metadata = new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets metadata associated with this <see cref="BinaryLayout"/>.
        /// </summary>
        /// <remarks>
        /// For XML-formatted layouts, metadata is encoded as attributes in the root element.
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
                if (_metadata.TryGetValue(key, out string value))
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
        /// Gets the version of Cascara that this <see cref="BinaryLayout"/> is designed for.
        /// </summary>
        public Version Version
        {
            get;
        }

        /// <summary>
        /// Gets the path to the file that created this <see cref="BinaryLayout"/> object.
        /// If the layout was not loaded from a file, this value is <c>null</c>.
        /// </summary>
        public string SourcePath
        {
            get;
        }

        /// <summary>
        /// Gets metadata associated with this <see cref="BinaryLayout"/>.
        /// </summary>
        public IReadOnlyDictionary<string, string> Metadata
        {
            get { return _metadata; }
        }

        /// <summary>
        /// Gets the top-level <see cref="Statement"/> that defines this <see cref="BinaryLayout"/>.
        /// </summary>
        internal Statement RootStatement
        {
            get;
            private set;
        }

        /// <summary>
        /// Validates the layout data, sets the root statement, and extracts metadata.
        /// </summary>
        protected abstract void Initialize();

        #region Equality
        public bool Equals(BinaryLayout other)
        {
            if (other == null)
            {
                return false;
            }

            return Name == other.Name
                && RootStatement.Equals(other.RootStatement);
        }

        public sealed override bool Equals(object obj)
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

        public sealed override int GetHashCode()
        {
            unchecked
            {
                int hash = 13;
                hash = (hash * 37) ^ Name.GetHashCode();
                hash = (hash * 37) ^ RootStatement.GetHashCode();

                return hash;
            }
        }
        #endregion

        public sealed override string ToString()
        {
            Dictionary<string, string> meta = _metadata
                .Where(p => (p.Key != Parameters.Name && p.Key != Parameters.Version))
                .ToDictionary(p => p.Key, q => q.Value);
            string metaStr = "{";
            foreach (var kvp in meta)
            {
                metaStr += " " + kvp.Key + " => " + kvp.Value + ",";
            }
            metaStr = metaStr.Substring(0, Math.Max(1, metaStr.Length - 1)) + " }";

            return string.Format("{0}: [ {1} = {2}, {3} = {4}, {5} = {6}, {7} = {8} ]",
                GetType().Name,
                nameof(Name), Name,
                nameof(Version), Version,
                nameof(SourcePath), (SourcePath == null) ? "(null)" : SourcePath,
                nameof(Metadata), metaStr);
        }
    }

    /// <summary>
    /// Defines all possible source code formats for a <see cref="BinaryLayout"/>.
    /// </summary>
    internal enum LayoutFormat
    {
        /// <summary>
        /// Indicates a <see cref="BinaryLayout"/> that is formatted as an XML document.
        /// </summary>
        Xml
    }
}
