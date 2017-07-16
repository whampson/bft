//#region License
///* Copyright (c) 2017 Wes Hampson
// * 
// * Permission is hereby granted, free of charge, to any person obtaining a copy
// * of this software and associated documentation files (the "Software"), to deal
// * in the Software without restriction, including without limitation the rights
// * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// * copies of the Software, and to permit persons to whom the Software is
// * furnished to do so, subject to the following conditions:
// * 
// * The above copyright notice and this permission notice shall be included in all
// * copies or substantial portions of the Software.
// * 
// * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// * SOFTWARE.
// */
//#endregion

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Runtime.InteropServices;
//using System.Xml.Linq;

//namespace WHampson.Bft
//{
//    internal class CustomTypeInfo
//    {
//        /// <summary>
//        /// Creates a new <see cref="CustomTypeInfo"/> object whose type
//        /// declaration defines a grouped list of variables to be collectivey
//        /// treated as one block of memory.
//        /// </summary>
//        /// <param name="members">
//        /// The XML elements that define the variables contained within the
//        /// struct.
//        /// </param>
//        /// <param name="size">
//        /// The size in bytes of the struct. Must be constant.
//        /// </param>
//        /// <returns>
//        /// A new <see cref="CustomTypeInfo"/> object.
//        /// </returns>
//        public static CustomTypeInfo CreateStruct(IEnumerable<XElement> members, int size)
//        {
//            return new CustomTypeInfo(null, members, size);
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="baseType"></param>
//        /// <returns></returns>
//        public static CustomTypeInfo CreatePrimitive(Type baseType)
//        {
//            int siz = Marshal.SizeOf(baseType);
//            return new CustomTypeInfo(baseType, new List<XElement>(), siz);
//        }

//        private CustomTypeInfo(Type baseType, IEnumerable<XElement> members, int size)
//        {
//            if (members == null)
//            {
//                throw new ArgumentNullException("members");
//            }
//            if (size < 0)
//            {
//                throw new ArgumentException("Size must be a non-negative integer.");
//            }

//            BaseType = baseType;
//            Members = new List<XElement>(members);
//            Size = size;
//        }

//        /// <summary>
//        /// Gets a value indicating whether this custom type is a struct.
//        /// </summary>
//        public bool IsStruct
//        {
//            get { return BaseType == null && Members.Count() != 0; }
//        }

//        /// <summary>
//        /// Gets the type from which this custom type is derived.
//        /// </summary>
//        /// <remarks>
//        /// If <see cref="IsStruct"/> is <code>True</code>, this value will be
//        /// <code>null</code>. This is because structs are a mixture of many
//        /// different types and do not necessarily stem from a specific type.
//        /// </remarks>
//        public Type BaseType
//        {
//            get;
//        }

//        /// <summary>
//        /// Gets the list of XML elements that define the structure of this
//        /// custom type.
//        /// </summary>
//        /// <remarks>
//        /// If <see cref="IsStruct"/> is <code>False</code>, this list will be
//        /// empty.
//        /// </remarks>
//        public IEnumerable<XElement> Members
//        {
//            get;
//        }

//        /// <summary>
//        /// Gets the size in bytes of this custom type.
//        /// </summary>
//        public int Size
//        {
//            get;
//        }
//    }
//}
