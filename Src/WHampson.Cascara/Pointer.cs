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
using System.Runtime.InteropServices;
using WHampson.Cascara.Types;

namespace WHampson.Cascara
{
    /// <summary>
    /// Represents a pointer to a primitive data type.
    /// </summary>
    /// <typeparam name="T">
    /// The type describing the data being pointed to.
    /// </typeparam>
    public class Pointer<T>
        where T : struct, IPrimitiveType
    {
        /// <summary>
        /// Handles implicit conversion from <see cref="Pointer{T}"/>
        /// to <see cref="IntPtr"/>.
        /// </summary>
        /// <param name="p">
        /// The <see cref="Pointer{T}"/> to convert to <see cref="IntPtr"/>.
        /// </param>
        public static implicit operator IntPtr(Pointer<T> p)
        {
            return p.Address;
        }

        /// <summary>
        /// Handles implicit conversion from <see cref="IntPtr"/>
        /// to <see cref="Pointer{T}"/>.
        /// </summary>
        /// <param name="p">
        /// The <see cref="IntPtr"/> to convert to <see cref="Pointer{T}"/>.
        /// </param>
        public static implicit operator Pointer<T>(IntPtr p)
        {
            return new Pointer<T>(p);
        }

        /// <summary>
        /// Creates an instance of the primitive type <see cref="T"/> using
        /// the data located at the specified memory address.
        /// </summary>
        /// <param name="addr">
        /// The memory location to be dereferenced.
        /// </param>
        /// <returns>
        /// A new instance of <see cref="T"/> containing the data at the
        /// specified memory location.
        /// </returns>
        private static unsafe T Dereference(IntPtr addr)
        {
            if (addr == IntPtr.Zero)
            {
                throw new ArgumentException("Cannot dereference null pointer.");
            }

            Type t = typeof(T);
            IPrimitiveType inst = null;

            // This likely isn't the safest way to do this,
            // but it'll do for what we need it to.
            switch (t.Name)
            {
                // TODO: DON'T FOEGET to finish for all defined types
                case "Float":
                    Float* pF = (Float*) addr;
                    inst = *pF;
                    break;

                case "Int8":
                    Int8* pI8 = (Int8*) addr;
                    inst = *pI8;
                    break;

                case "Int32":
                    Types.Int32* pI32 = (Types.Int32*) addr;
                    inst = *pI32;
                    break;

                default:
                    // Should never happen as long as I remember
                    // to list all types ;)
                    throw new InvalidOperationException();
            }

            return (T) inst;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Pointer{T}"/> class using
        /// the provided memory address.
        /// </summary>
        /// <param name="addr">
        /// The memory location of the data being pointed to.
        /// </param>
        public Pointer(IntPtr addr) : this(addr, 1)
        {
        }

        /// <summary>
        /// Creates a new array instance of the <see cref="Pointer{T}"/> class using
        /// the provided memory address and number of array elements.
        /// </summary>
        /// <param name="addr">
        /// The memory location of the data being pointed to.
        /// </param>
        /// <param name="count">
        /// The number of items present in the data set that the data represent.
        /// </param>
        public Pointer(IntPtr addr, int count)
        {
            if (addr == IntPtr.Zero)
            {
                throw new ArgumentException("Null pointer not allowed.", "addr");
            }

            if (count < 1)
            {
                throw new ArgumentException("Count must be a positive integer.", "count");
            }

            Address = addr;
            Count = count;
        }

        /// <summary>
        /// Gets the ith element in the array being pointed to.
        /// </summary>
        /// <param name="i">
        /// The index of the element to get.
        /// </param>
        /// <returns>
        /// The value of the ith element.
        /// </returns>
        /// <exception cref="IndexOutOfRangeException"/>
        /// Thrown if the specified element index is out of the bounds of the array.
        /// </exception>
        public T this[int i]
        {
            get
            {
                if (i < 0 || i > Count - 1)
                {
                    string msg = "Index was outside the bounds of the array.";
                    throw new IndexOutOfRangeException(msg);
                }

                int siz = Marshal.SizeOf(typeof(T));
                IntPtr valAddr = Address + (i * siz);

                return Dereference(valAddr);
            }

            set
            {
                if (i < 0 || i > Count - 1)
                {
                    string msg = "Index was outside the bounds of the array.";
                    throw new IndexOutOfRangeException(msg);
                }

                int siz = Marshal.SizeOf(typeof(T));
                IntPtr valAddr = Address + (i * siz);

                byte[] data = value.GetBytes();
                Marshal.Copy(data, 0, valAddr, data.Length);
            }
        }

        /// <summary>
        /// Gets the memory location of the data being pointed to.
        /// </summary>
        public IntPtr Address
        {
            get;
        }

        /// <summary>
        /// The number of elements in the data set.
        /// </summary>
        public int Count
        {
            get;
        }

        /// <summary>
        /// Gets or sets the data that this pointer points to.
        /// If the pointer is an array, the 0th value is returned.
        /// </summary>
        public T Value
        {
            get
            {
                return this[0];
            }

            set
            {
                this[0] = value;
            }
        }

        public override string ToString()
        {
            return Address.ToString();
        }
    }
}
