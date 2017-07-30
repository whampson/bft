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

namespace WHampson.Cascara.Types
{
    /// <summary>
    /// A pointer to some <see cref="ICascaraType"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The type represented by the data pointed to.
    /// </typeparam>
    public class Pointer<T> : Pointer
        where T : struct
    {
        /// <summary>
        /// Creates a new pointer to the specified address.
        /// </summary>
        /// <param name="addr">
        /// The address to point to.
        /// </param>
        public Pointer(IntPtr addr)
            : base(addr)
        {
        }

        /// <summary>
        /// Dereferences the pointer and gets or sets the value pointed to.
        /// </summary>
        public T Value
        {
            get { return this[0]; }
            set { this[0] = value; }
        }

        //public virtual string StringValue
        //{
        //    get
        //    {
        //        string s = "";
        //        int i = 0;
        //        char c;
        //        do
        //        {
        //            c = Convert.ToChar(this[i]);
        //            s += c;
        //            i++;
        //        } while (c != '\0');

        //        return s;
        //    }
        //}

        /// <summary>
        /// Increments the address pointed to by <paramref name="i"/> units,
        /// then dereferences the value at that address so it can be get or set.
        /// </summary>
        /// <param name="i">
        /// The offset in units of the type <see cref="T"/>.
        /// </param>
        public T this[int i]
        {
            get
            {
                int siz = Marshal.SizeOf(typeof(T));
                return Marshal.PtrToStructure<T>(Address + (i * siz));
            }

            set
            {
                int siz = Marshal.SizeOf(typeof(T));
                byte[] data = BitConverter.GetBytes((dynamic) value);
                Marshal.Copy(data, 0, Address + (i * siz), data.Length);
            }
        }

        /// <summary>
        /// Converts a .NET <see cref="IntPtr"/> type into a <see cref="Pointer{T}"/>.
        /// </summary>
        /// <param name="ptr">
        /// The <see cref="IntPtr"/> to convert.
        /// </param>
        public static implicit operator Pointer<T>(IntPtr ptr)
        {
            return new Pointer<T>(ptr);
        }

        /// <summary>
        /// Converts a <see cref="Pointer{T}"/> into a .NET <see cref="IntPtr"/> type.
        /// </summary>
        /// <param name="ptr">
        /// The <see cref="Pointer{T}"/> to convert.
        /// </param>
        public static implicit operator IntPtr(Pointer<T> ptr)
        {
            return ptr.Address;
        }

        /// <summary>
        /// Adds an offset to the value of a pointer.
        /// The offset is in units of the type <see cref="T"/>. This means that if
        /// <see cref="T"/> is 4 bytes, an offset of "1" will add 4 bytes to the address.
        /// </summary>
        /// <param name="ptr">
        /// The base address.
        /// </param>
        /// <param name="off">
        /// The offset to add to the base.
        /// </param>
        /// <returns>
        /// A <see cref="Pointer{T}"/> object that points to the new address.
        /// </returns>
        public static Pointer<T> operator +(Pointer<T> ptr, int off)
        {
            int siz = Marshal.SizeOf(typeof(T));

            return new Pointer<T>(ptr.Address + (siz * off));
        }

        /// <summary>
        /// Subtracts an offset from the value of a pointer.
        /// The offset is in units of the type <see cref="T"/>. This means that if
        /// <see cref="T"/> is 4 bytes, an offset of "1" will add 4 bytes to the address.
        /// </summary>
        /// <param name="ptr">
        /// The base address.
        /// </param>
        /// <param name="off">
        /// The offset to subtract from the base.
        /// </param>
        /// <returns>
        /// A <see cref="Pointer{T}"/> object that points to the new address.
        /// </returns>
        public static Pointer<T> operator -(Pointer<T> ptr, int off)
        {
            int siz = Marshal.SizeOf(typeof(T));

            return new Pointer<T>(ptr.Address - (siz * off));
        }
    }
}
