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
using System.Xml.Linq;
using WHampson.Cascara.Types;

namespace WHampson.Cascara
{
    /// <summary>
    /// Contains information about a data type used during the processing of a template.
    /// </summary>
    internal sealed class TypeInfo
    {
        /// <summary>
        /// Creates a <see cref="TypeInfo"/> object representing a primitive
        /// data type that is equivalent to the provided .NET <see cref="Kind"/>.
        /// </summary>
        /// <param name="t">
        /// The .NET type that this primitive represents.
        /// </param>
        /// <returns>
        /// The newly-created <see cref="TypeInfo"/> object.
        /// </returns>
        public static TypeInfo CreatePrimitive(Type t, int size)
        {
            return new TypeInfo(t, new List<XElement>(), size);
        }

        /// <summary>
        /// Creates a new <see cref="TypeInfo"/> object that represents a
        /// composite data type.
        /// </summary>
        /// <param name="members">
        /// The <see cref="XElement"/>s that describe the composite type.
        /// </param>
        /// <param name="size">
        /// The size in bytes of the data type.
        /// </param>
        /// <returns>
        /// The newly-created <see cref="TypeInfo"/> object.
        /// </returns>
        public static TypeInfo CreateStruct(IEnumerable<XElement> members, int size)
        {
            if (members == null)
            {
                throw new ArgumentNullException("members");
            }

            if (size < 0)
            {
                throw new ArgumentException("Size must be a non-negative integer.");
            }

            return new TypeInfo(typeof(ICascaraStruct), members, size);
        }

        private TypeInfo(Type t, IEnumerable<XElement> members, int size)
        {
            Kind = t;
            Members = new List<XElement>(members);
            Size = size;
        }

        /// <summary>
        /// Gets the .NET <see cref="Kind"/> represented.
        /// </summary>
        public Type Kind
        {
            get;
        }

        /// <summary>
        /// Gets the collection of <see cref="XElement"/>s that define this type.
        /// </summary>
        public IEnumerable<XElement> Members
        {
            get;
        }

        /// <summary>
        /// Gets the size in bytes of the data represented by this type.
        /// </summary>
        public int Size
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the type is a struct.
        /// </summary>
        public bool IsStruct
        {
            get { return Kind == typeof(ICascaraStruct); }
        }

        public override string ToString()
        {
            return string.Format("[Kind: {0}, Size: {1}]", Kind, Size);
        }
    }
}
