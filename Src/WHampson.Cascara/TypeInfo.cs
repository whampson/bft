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
using WHampson.Cascara.Types;

namespace WHampson.Cascara
{
    /// <summary>
    /// Retains information regarding the data type represented
    /// by a contiguous set of bytes.
    /// </summary>
    public sealed class TypeInfo
    {
        /// <summary>
        /// Creates a new <see cref="TypeInfo"/> object using the specified
        /// type, offset, and size.
        /// </summary>
        /// <param name="type">
        /// The .NET <see cref="System.Type"/> that the data represents.
        /// </param>
        /// <param name="offset">
        /// The location in the overall dataset of the first byte
        /// of data represented by this type instance.
        /// </param>
        /// <param name="size">
        /// The number of bytes that an instance of this type occupies.
        /// </param>
        internal TypeInfo(Type type, int offset, int size)
        {
            if (offset < 0)
            {
                throw new ArgumentException("Offset must be a non-negative integer.", nameof(offset));
            }

            if (size < 0)
            {
                throw new ArgumentException("Size must be a non-negative integer.", nameof(size));
            }

            Type = type;
            Offset = offset;
            Size = size;
        }

        /// <summary>
        /// Gets the .NET <see cref="System.Type"/> that the data represents.
        /// The classifier <see cref="ICascaraStruct"/> is used if the data
        /// represents a struct.
        /// </summary>
        public Type Type
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets location in the binary file data of the first byte
        /// of data for this type instance.
        /// </summary>
        public int Offset
        {
            get;
        }

        /// <summary>
        /// Gets the number of bytes that an instance of this type occupies.
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
            get { return Type == typeof(ICascaraStruct); }
        }

        public override string ToString()
        {
            return string.Format("[Type: {0}, Offset: {1}, Size: {2}]",
                Type, Offset, Size);
        }
    }
}
