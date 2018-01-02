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
        public static BinaryLayout Load(string path)
        {
            return Load(path, LayoutType.Xml);
        }

        public static BinaryLayout Parse(string source)
        {
            return Parse(source, LayoutType.Xml);
        }

        internal static BinaryLayout Load(string path, LayoutType type)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException(Resources.ArgumentExceptionEmptyString, nameof(path));
            }

            switch (type)
            {
                case LayoutType.Xml:
                    return XmlBinaryLayout.LoadSource(path);
                default:
                    throw new NotSupportedException();
            }
        }

        internal static BinaryLayout Parse(string source, LayoutType type)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                throw new ArgumentException(Resources.ArgumentExceptionEmptyString, nameof(source));
            }

            switch (type)
            {
                case LayoutType.Xml:
                    return XmlBinaryLayout.ParseSource(source);
                default:
                    throw new NotSupportedException();
            }
        }

        protected Dictionary<string, string> _metadata;

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

        public string Name
        {
            get;
        }

        public Version Version
        {
            get;
        }

        public string SourcePath
        {
            get;
        }

        public IReadOnlyDictionary<string, string> Metadata
        {
            get { return _metadata; }
        }

        internal Statement RootStatement
        {
            get;
            private set;
        }

        protected abstract void Initialize();

        public bool Equals(BinaryLayout other)
        {
            if (other == null)
            {
                return false;
            }

            return Name == other.Name
                && RootStatement.Equals(other.RootStatement);
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

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 13;
                hash = (hash * 37) ^ Name.GetHashCode();
                hash = (hash * 37) ^ RootStatement.GetHashCode();

                return hash;
            }
        }
    }

    internal enum LayoutType
    {
        Xml
    }
}
