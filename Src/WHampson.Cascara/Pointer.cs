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

using Int32 = WHampson.Cascara.Types.Int32;

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
        /// <exception cref="Exception">
        /// If <see cref="T"/> is unable to be dereferenced.
        /// </exception>
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
                // TODO: finish for all defined types
                case "Float":
                    Float* pF = (Float*) addr;
                    inst = *pF;
                    break;

                case "Int8":
                    Int8* pI8 = (Int8*) addr;
                    inst = *pI8;
                    break;

                case "Int32":
                    Int32* pI32 = (Int32*) addr;
                    inst = *pI32;
                    break;
                
                default:
                    throw new Exception();  // TODO: create specialized exception
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

        public Pointer(IntPtr addr, int count)
        {
            if (addr == IntPtr.Zero)
            {
                throw new ArgumentException("Null pointer not allowed.");
            }

            if (count < 1)
            {
                throw new ArgumentException("Count must be a positive integer.");
            }

            Address = addr;
            Count = count;
        }

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

        public int Count
        {
            get;
        }

        /// <summary>
        /// Gets or sets the data that this pointer points to.
        /// </summary>
        public T Value
        {
            get
            {
                //// Create instance of T using data at address
                //return Dereference(Address);
                return this[0];
            }

            set
            {
                //// Get bytes of new value, copy bytes to address
                //byte[] data = value.GetBytes();
                //Marshal.Copy(data, 0, Address, data.Length);
                this[0] = value;
            }
        }

        public override string ToString()
        {
            return Address.ToString();
        }
    }
}
