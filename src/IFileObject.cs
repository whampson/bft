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

namespace WHampson.Cascara
{
    /// <summary>
    /// Defines a file object, which is a meaningful piece of data
    /// made up of bytes in a <see cref="BinaryData"/>.
    /// </summary>
    public interface IFileObject : IEnumerable<IFileObject>
    {
        /// <summary>
        /// Gets the position of this <see cref="IFileObject"/> relative to the start
        /// of the <see cref="BinaryData"/>.
        /// </summary>
        int GlobalOffset
        {
            get;
        }

        /// <summary>
        /// Gets the position of this <see cref="IFileObject"/> relative to the start
        /// of its parent object.
        /// </summary>
        int LocalOffset
        {
            get;
        }

        /// <summary>
        /// Gets the number of bytes that make up this <see cref="IFileObject"/>.
        /// </summary>
        int Length
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IFileObject"/> represents a collection.
        /// </summary>
        bool IsCollection
        {
            get;
        }

        /// <summary>
        /// Gets the number of elements in the collection represented by this <see cref="IFileObject"/>.
        /// If this <see cref="IFileObject"/> does not represent a collection, this value is -1.
        /// </summary>
        /// <seealso cref="IsCollection"/>
        int ElementCount
        {
            get;
        }

        /// <summary>
        /// Gets the <see cref="BinaryData"/> that this <see cref="IFileObject"/>
        /// belongs to.
        /// </summary>
        BinaryData DataSource
        {
            get;
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
        IFileObject ElementAt(int index);
    }
}
