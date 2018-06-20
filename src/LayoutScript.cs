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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    /// A script that defines the structure of a <see cref="BinaryFile"/>.
    /// </summary>
    public abstract partial class LayoutScript : IEquatable<LayoutScript>
    {
        /// <summary>
        /// Creates a new <see cref="LayoutScript"/> object from a file.
        /// </summary>
        /// <param name="path">The path to the file to load.</param>
        /// <returns>The newly-created <see cref="LayoutScript"/> object.</returns>
        /// <exception cref="LayoutScriptException">
        /// Thrown if the <see cref="LayoutScript"/> is empty or contains a
        /// malformatted version string.
        /// </exception>
        /// <exception cref="SyntaxException">
        /// Thrown if the <see cref="LayoutScript"/> contains a syntax error.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if the path is empty or null.
        /// </exception>
        public static LayoutScript Load(string path)
        {
            return Load(path, LayoutFormat.Xml);
        }

        /// <summary>
        /// Creates a new <see cref="LayoutScript"/> object from a file.
        /// </summary>
        /// <param name="path">The path to the file to load.</param>
        /// <param name="format">The source code format.</param>
        /// <returns>The newly-created <see cref="LayoutScript"/> object.</returns>
        /// <exception cref="LayoutScriptException">
        /// Thrown if the <see cref="LayoutScript"/> is empty or contains a
        /// malformatted version string.
        /// </exception>
        /// <exception cref="SyntaxException">
        /// Thrown if the <see cref="LayoutScript"/> contains a syntax error.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if the path is empty or null.
        /// </exception>
        internal static LayoutScript Load(string path, LayoutFormat format)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException(Resources.ArgumentExceptionEmptyString, nameof(path));
            }

            switch (format)
            {
                case LayoutFormat.Xml:
                    return XmlLayoutScript.LoadSource(path);
                default:
                    // TODO: message
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Creates a new <see cref="LayoutScript"/> object from a string.
        /// </summary>
        /// <param name="xmlSource">The <see cref="LayoutScript"/> XML source code.</param>
        /// <returns>The newly-created <see cref="LayoutScript"/> object.</returns>
        /// <exception cref="LayoutScriptException">
        /// Thrown if the <see cref="LayoutScript"/> is empty or contains a
        /// malformatted version string.
        /// </exception>
        /// <exception cref="SyntaxException">
        /// Thrown if the <see cref="LayoutScript"/> contains a syntax error.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if the source string is empty or null.
        /// </exception>
        public static LayoutScript Parse(string xmlSource)
        {
            return Parse(xmlSource, LayoutFormat.Xml);
        }

        /// <summary>
        /// Creates a new <see cref="LayoutScript"/> object from a string.
        /// </summary>
        /// <param name="source">The <see cref="LayoutScript"/> source code string.</param>
        /// <param name="format">The source code format.</param>
        /// <returns>The newly-created <see cref="LayoutScript"/> object.</returns>
        /// <exception cref="LayoutScriptException">
        /// Thrown if the <see cref="LayoutScript"/> is empty or contains a
        /// malformatted version string.
        /// </exception>
        /// <exception cref="SyntaxException">
        /// Thrown if the <see cref="LayoutScript"/> contains a syntax error.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if the source string is empty or null.
        /// </exception>
        internal static LayoutScript Parse(string source, LayoutFormat format)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                throw new ArgumentException(Resources.ArgumentExceptionEmptyString, nameof(source));
            }

            switch (format)
            {
                case LayoutFormat.Xml:
                    return XmlLayoutScript.ParseSource(source);
                default:
                    throw new NotSupportedException();
            }
        }

        internal Dictionary<string, string> _metadata;

        /// <summary>
        /// Creates a new <see cref="LayoutScript"/> object.
        /// </summary>
        /// <param name="version">The Cascara version that the layout is designed for.</param>
        /// <param name="sourcePath">The path to the source file (if applicable).</param>
        private LayoutScript( Version version, string sourcePath)
        {
            if (version == null)
            {
                throw new ArgumentNullException(nameof(version));
            }

            Version = version;
            SourcePath = sourcePath;
            _metadata = new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets metadata associated with this <see cref="LayoutScript"/>.
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
        /// Gets the version string encoded within this <see cref="LayoutScript"/>.
        /// </summary>
        /// <remarks>
        /// The version specifies the minimum version of Cascara that this script
        /// can be run with.
        /// </remarks>
        public Version Version
        {
            get;
        }

        /// <summary>
        /// Gets the path to the file that created this <see cref="LayoutScript"/> object.
        /// If the layout was not loaded from a file, this value is <c>null</c>.
        /// </summary>
        public string SourcePath
        {
            get;
        }

        /// <summary>
        /// Gets metadata associated with this <see cref="LayoutScript"/>.
        /// </summary>
        public IReadOnlyDictionary<string, string> Metadata
        {
            get { return _metadata; }
        }

        /// <summary>
        /// Gets the top-level <see cref="Statement"/> that defines this <see cref="LayoutScript"/>.
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

        /// <summary>
        /// Determines whether the specified object is equal to this object.
        /// </summary>
        public bool Equals(LayoutScript other)
        {
            if (other == null)
            {
                return false;
            }

            return RootStatement.Equals(other.RootStatement);
        }

        /// <summary>
        /// Determines whether the specified object is equal to this object.
        /// </summary>
        public sealed override bool Equals(object obj)
        {
            if (!(obj is LayoutScript))
            {
                return false;
            }
            else if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return Equals(obj as LayoutScript);
        }

        /// <summary>
        /// Serves as this object's default hash function.
        /// </summary>
        public sealed override int GetHashCode()
        {
            unchecked
            {
                int hash = 13;
                hash = (hash * 37) ^ RootStatement.GetHashCode();

                return hash;
            }
        }

        /// <summary>
        /// Returns a string that represents this object.
        /// </summary>
        public sealed override string ToString()
        {
            JObject o = new JObject();
            o.Add(nameof(Version), Version.ToString());
            o.Add(nameof(SourcePath), SourcePath);
            o.Add(nameof(Metadata), JToken.FromObject(Metadata));
            return o.ToString(Newtonsoft.Json.Formatting.None);
        }
    }

    /// <summary>
    /// Defines all possible source code formats for a <see cref="LayoutScript"/>.
    /// </summary>
    internal enum LayoutFormat
    {
        /// <summary>
        /// Indicates a <see cref="LayoutScript"/> that is formatted as an XML document.
        /// </summary>
        Xml
    }
}
