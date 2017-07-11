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
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace WHampson.Bft
{
    internal class CustomTypeInfo
    {
        public static CustomTypeInfo CreateStruct(/*string name, */IEnumerable<XElement> members, int size)
        {
            return new CustomTypeInfo(/*name, */null, members, size);
        }

        public static CustomTypeInfo CreatePrimitive(/*string name, */Type baseType)
        {
            int siz = Marshal.SizeOf(baseType);
            return new CustomTypeInfo(/*name, */baseType, new List<XElement>(), siz);
        }

        /// <summary>
        /// Creates a new <see cref="CustomTypeInfo"/> object.
        /// </summary>
        /// <param name="kind">
        /// The parent type.
        /// </param>
        /// <param name="members">
        /// A list of member elements.
        /// </param>
        private CustomTypeInfo(/*string name, */Type baseType, IEnumerable<XElement> members, int size)
        {
            //if (name == null)
            //{
            //    throw new ArgumentNullException("name");
            //}
            if (members == null)
            {
                throw new ArgumentNullException("members");
            }
            if (size < 0)
            {
                throw new ArgumentException("Size must be a non-negative integer.");
            }

            //TypeName = name;
            BaseType = baseType;
            Members = new List<XElement>(members);
            Size = size;
        }

        public bool IsStruct
        {
            get { return BaseType == null && Members.Count() != 0; }
        }

        public Type BaseType
        {
            get;
        }

        /// <summary>
        /// Gets the list of XML elements that descend from this custom type.
        /// </summary>
        public IEnumerable<XElement> Members
        {
            get;
        }

        /// <summary>
        /// Gets the size in bytes that this custom type occupies.
        /// </summary>
        public int Size
        {
            get;
        }
    }
}
