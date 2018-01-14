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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using WHampson.Cascara.Interpreter;

namespace WHampson.Cascara
{
    /// <summary>
    /// Represents a primitive data type found in a <see cref="BinaryFile"/>.
    /// </summary>
    /// <typeparam name="T">The .NET or Cascara type that this object encapsulates.</typeparam>
    public class Primitive<T> : IFileObject
        where T : struct
    {
        private BinaryFile sourceFile;
        private Symbol symbol;

        internal Primitive(BinaryFile sourceFile, Symbol symbol)
        {
            if (!PrimitiveTypeUtils.IsPrimitiveType<T>())
            {
                string msg = Resources.ArgumentExceptionPrimitiveType;
                throw new ArgumentException(msg, nameof(T));
            }
            if (PrimitiveTypeUtils.SizeOf<T>() > symbol.DataLength)
            {
                string msg = "The type provided cannot be larger than the size of the data field.";
                throw new ArgumentException(msg, nameof(T));
            }

            this.sourceFile = sourceFile;
            this.symbol = symbol;
        }

        /// <summary>
        /// Gets the element of the collection at the specified index.
        /// Throws <see cref="InvalidOperationException"/> if this <see cref="Primitive{T}"/>
        /// does not represent a collection.
        /// </summary>
        /// <param name="index">The index of the element to get.</param>
        /// <returns>The element at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the specified index is negative or greater than or equal to the element count.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this <see cref="Primitive{T}"/> does not represent a collection.
        /// </exception>
        /// <seealso cref="IsCollection"/>
        public Primitive<T> this[int index]
        {
            get { return (Primitive<T>) ((IFileObject) this).ElementAt(index); }
        }

        /// <summary>
        /// Gets or sets the value of this primitive type.
        /// Setting this property will change the bytes in the <see cref="BinaryFile"/>
        /// at the location specified by <see cref="FilePosition"/>.
        /// </summary>
        public T Value
        {
            get
            {
                if (IsCollection)
                {
                    string msg = "This property can only be used on elements of a collection, not the collection itself.";
                    throw new InvalidOperationException(msg);
                }
                return sourceFile.Get<T>(FilePosition);
            }

            set
            {
                if (IsCollection)
                {
                    string msg = "This property can only be used on elements of a collection, not the collection itself.";
                    throw new InvalidOperationException(msg);
                }
                sourceFile.Set<T>(FilePosition, value);
            }
        }

        /// <summary>
        /// Gets the position of this <see cref="IFileObject"/> relative to the start
        /// of the <see cref="BinaryFile"/>.
        /// </summary>
        public int FilePosition
        {
            get { return symbol.DataOffset; }
        }

        /// <summary>
        /// Gets the position of this <see cref="IFileObject"/> relative to the start
        /// of the parent object.
        /// </summary>
        public int Offset
        {
            get
            {
                if (symbol.Parent != null)
                {
                    return symbol.DataOffset - symbol.Parent.DataOffset;
                }

                return symbol.DataOffset;
            }
        }

        /// <summary>
        /// Gets the number of bytes that make up this <see cref="IFileObject"/>.
        /// </summary>
        public int Length
        {
            get { return symbol.DataLength; }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IFileObject"/> represents a collection.
        /// </summary>
        public bool IsCollection
        {
            get { return symbol.IsCollection; }
        }

        /// <summary>
        /// Gets the number of elements in the collection represented by this <see cref="IFileObject"/>.
        /// If this <see cref="IFileObject"/> does not represent a collection, this value is -1.
        /// </summary>
        /// <seealso cref="IsCollection"/>
        public int ElementCount
        {
            get { return symbol.ElementCount; }
        }

        /// <summary>
        /// Gets the element at the specified index in the collection as an <see cref="IFileObject"/>.
        /// If this <see cref="IFileObject"/> does not represent a collection, an
        /// <see cref="InvalidOperationException"/> will be thrown.
        /// </summary>
        /// <param name="index">The index of the element to get.</param>
        /// <returns>The element at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the specified index is negative or greater than or equal to the element count.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this <see cref="IFileObject"/> does not represent a collection.
        /// </exception>
        /// <seealso cref="IsCollection"/>
        IFileObject IFileObject.ElementAt(int index)
        {
            if (!IsCollection)
            {
                string msg = "Object instance is not a collection.";
                throw new InvalidOperationException(msg);
            }

            if (index < 0 || index >= ElementCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return new Primitive<T>(sourceFile, symbol[index]);
        }

        public IEnumerator<IFileObject> GetEnumerator()
        {
            if (!IsCollection)
            {
                yield break;
            }

            for (int i = 0; i < ElementCount; i++)
            {
                yield return ((IFileObject) this).ElementAt(i);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Converts this <see cref="Primitive{T}"/> object to an <see cref="int"/>
        /// whose value equals <see cref="FilePosition"/>.
        /// </summary>
        /// <param name="p">The <see cref="Primitive{T}"/> object to convert to an <see cref="int"/>.</param>
        public static implicit operator int(Primitive<T> p)
        {
            return p.FilePosition;
        }
    }
}
