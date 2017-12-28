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
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;

namespace WHampson.Cascara
{
    /// <summary>
    /// Represents a data type used in a <see cref="BinaryLayout"/>.
    /// </summary>
    internal sealed class CascaraType
    {
        /// <summary>
        /// A dictionary of all built-in primitive type definitions.
        /// </summary>
        public static Dictionary<string, CascaraType> BuiltInPrimitives = new Dictionary<string, CascaraType>()
        {
            //{ Keywords.Bool,    CreatePrimitive(typeof(bool),   1) },
            //{ Keywords.Bool8,   CreatePrimitive(typeof(bool),   1) },
            //{ Keywords.Bool16,  CreatePrimitive(typeof(bool),   2) },
            //{ Keywords.Bool32,  CreatePrimitive(typeof(bool),   4) },
            //{ Keywords.Bool64,  CreatePrimitive(typeof(bool),   8) },
            //{ Keywords.Byte,    CreatePrimitive(typeof(byte),   1) },
            //{ Keywords.Char,    CreatePrimitive(typeof(char),   1) },
            //{ Keywords.Char8,   CreatePrimitive(typeof(char),   1) },
            //{ Keywords.Char16,  CreatePrimitive(typeof(char),   2) },
            //{ Keywords.Double,  CreatePrimitive(typeof(double), 8) },
            //{ Keywords.Float,   CreatePrimitive(typeof(float),  4) },
            //{ Keywords.Int,     CreatePrimitive(typeof(int),    4) },
            //{ Keywords.Int8,    CreatePrimitive(typeof(sbyte),  1) },
            //{ Keywords.Int16,   CreatePrimitive(typeof(short),  2) },
            //{ Keywords.Int32,   CreatePrimitive(typeof(int),    4) },
            //{ Keywords.Int64,   CreatePrimitive(typeof(long),   8) },
            //{ Keywords.Long,    CreatePrimitive(typeof(long),   8) },
            //{ Keywords.Short,   CreatePrimitive(typeof(short),  2) },
            //{ Keywords.Single,  CreatePrimitive(typeof(float),  4) },
            //{ Keywords.UInt,    CreatePrimitive(typeof(uint),   4) },
            //{ Keywords.UInt8,   CreatePrimitive(typeof(byte),   1) },
            //{ Keywords.UInt16,  CreatePrimitive(typeof(ushort), 2) },
            //{ Keywords.UInt32,  CreatePrimitive(typeof(uint),   4) },
            //{ Keywords.UInt64,  CreatePrimitive(typeof(ulong),  8) },
            //{ Keywords.ULong,   CreatePrimitive(typeof(ulong),  8) },
            //{ Keywords.UShort,  CreatePrimitive(typeof(ushort), 2) },
        };

        /// <summary>
        /// Creates a definition for a fixed-size data type. Primitive types contain no members
        /// and all bytes collectively represent a single piece of data.
        /// </summary>
        /// <param name="systemType">The .NET type analogous to this <see cref="CascaraType"/>.</param>
        /// <param name="size">The size in bytes of this type.</param>
        /// <returns>The newly-created type definition.</returns>
        public static CascaraType CreatePrimitive(Type systemType, int size)
        {
            if (systemType == null)
            {
                throw new ArgumentNullException(nameof(systemType));
            }

            if (!systemType.IsValueType)
            {
                throw new ArgumentException(Resources.ArgumentExceptionValueType, nameof(systemType));
            }

            if (size < 0)
            {
                throw new ArgumentException(Resources.ArgumentExceptionNonNegativeInteger, nameof(size));
            }

            return new CascaraType(systemType, new List<XElement>(), size);
        }

        /// <summary>
        /// Creates a definition for a composite data type.
        /// The bytes of a composite data type may collectively represent many pieces of data.
        /// </summary>
        /// <param name="members">
        /// A list of <see cref="XElement"/>s that describe the structure of the type.
        /// These must be valid <see cref="BinaryLayout"/> elements.
        /// </param>
        /// <param name="size">The size in bytes of this type.</param>
        /// <returns>The newly-created type definition.</returns>
        public static CascaraType CreateStruct(IEnumerable<XElement> members, int size)
        {
            if (members == null)
            {
                throw new ArgumentNullException(nameof(members));
            }

            if (members.Count() == 0)
            {
                throw new ArgumentException(Resources.ArgumentExceptionEmptyList, nameof(members));
            }

            if (size < 0)
            {
                throw new ArgumentException(Resources.ArgumentExceptionNonNegativeInteger, nameof(size));
            }

            return new CascaraType(null, members, size);
        }

        private CascaraType(Type systemType, IEnumerable<XElement> members, int size)
        {
            SystemType = systemType;
            Members = new List<XElement>(members);
            Size = size;
        }

        /// <summary>
        /// Gets the .NET <see cref="Type"/> that is analogous to this <see cref="CascaraType"/>.
        /// </summary>
        public Type SystemType
        {
            get;
        }

        /// <summary>
        /// Gets the list of <see cref="XElement"/>s that describe this type's structure.
        /// </summary>
        /// <remarks>
        /// This list is empty unless the type is a struct.
        /// </remarks>
        public IEnumerable<XElement> Members
        {
            get;
        }

        /// <summary>
        /// Gets the size in bytes of this <see cref="CascaraType"/>.
        /// </summary>
        public int Size
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="CascaraType"/> is a composite data type.
        /// </summary>
        public bool IsStruct
        {
            get { return SystemType == null && Members.Count() != 0; }
        }

        /// <summary>
        /// Gets the string value representation of this object.
        /// </summary>
        /// <returns>The string value representation of this object.</returns>
        public override string ToString()
        {
            return string.Format("CascaraType: [ SystemType: {0}, Size: {1}, IsStruct: {2} ]",
                SystemType, Size, IsStruct);
        }
    }
}
