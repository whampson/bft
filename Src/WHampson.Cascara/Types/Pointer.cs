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
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WHampson.Cascara.Types
{
    public class Pointer : ICascaraPointer
    {
        public Pointer(IntPtr addr)
        {
            Address = addr;
        }

        public IntPtr Address
        {
            get;
        }

        public bool IsNull()
        {
            return Address == IntPtr.Zero;
        }

        public static implicit operator Pointer(IntPtr ptr)
        {
            return new Pointer(ptr);
        }

        public static implicit operator IntPtr(Pointer ptr)
        {
            return ptr.Address;
        }

        public static Pointer operator +(Pointer ptr, int off)
        {
            return new Pointer(ptr.Address + off);
        }

        public static Pointer operator -(Pointer ptr, int off)
        {
            return new Pointer(ptr.Address - off);
        }
    }

    public class Pointer<T> : ICascaraPointer
        where T : struct, ICascaraType
    {
        public Pointer(IntPtr addr)
        {
            Address = addr;
        }

        public T Value
        {
            get
            {
                return Dereference(Address);
            }

            set
            {
                int siz = Marshal.SizeOf(typeof(T));
                byte[] data = value.GetBytes();
                Debug.Assert(siz == data.Length);

                Marshal.Copy(data, 0, Address, data.Length);
            }
        }

        public IntPtr Address
        {
            get;
        }

        public bool IsNull()
        {
            return Address == IntPtr.Zero;
        }

        public static implicit operator Pointer<T>(IntPtr ptr)
        {
            return new Pointer<T>(ptr);
        }

        public static implicit operator IntPtr(Pointer<T> ptr)
        {
            return ptr.Address;
        }

        public static explicit operator Pointer(Pointer<T> ptr)
        {
            return new Pointer(ptr.Address);
        }

        public static explicit operator Pointer<T>(Pointer ptr)
        {
            return new Pointer<T>(ptr.Address);
        }

        public static Pointer<T> operator +(Pointer<T> ptr, int off)
        {
            int siz = Marshal.SizeOf(typeof(T));

            return new Pointer<T>(ptr.Address + (siz * off));
        }

        public static Pointer<T> operator -(Pointer<T> ptr, int off)
        {
            int siz = Marshal.SizeOf(typeof(T));

            return new Pointer<T>(ptr.Address - (off * siz));
        }

        private static unsafe T Dereference(IntPtr addr)
        {
            if (addr == IntPtr.Zero)
            {
                throw new ArgumentException("Cannot dereference null pointer.");
            }

            Type t = typeof(T);
            object o;

            if (t == typeof(Bool8))
            {
                o = *(Bool8*) addr;
            }
            else if (t == typeof(Bool16))
            {
                o = *(Bool16*) addr;
            }
            else if (t == typeof(Bool32))
            {
                o = *(Bool32*) addr;
            }
            else if (t == typeof(Bool64))
            {
                o = *(Bool64*) addr;
            }
            else if (t == typeof(Char8))
            {
                o = *(Char8*) addr;
            }
            else if (t == typeof(Char16))
            {
                o = *(Char16*) addr;
            }
            else if (t == typeof(Double))
            {
                o = *(Double*) addr;
            }
            else if (t == typeof(Float))
            {
                o = *(Float*) addr;
            }
            else if (t == typeof(Int8))
            {
                o = *(Int8*) addr;
            }
            else if (t == typeof(Int16))
            {
                o = *(Int16*) addr;
            }
            else if (t == typeof(Int32))
            {
                o = *(Int32*) addr;
            }
            else if (t == typeof(Int64))
            {
                o = *(Int64*) addr;
            }
            else if (t == typeof(UInt8))
            {
                o = *(UInt8*) addr;
            }
            else if (t == typeof(UInt16))
            {
                o = *(UInt16*) addr;
            }
            else if (t == typeof(UInt32))
            {
                o = *(UInt32*) addr;
            }
            else if (t == typeof(UInt64))
            {
                o = *(UInt64*) addr;
            }
            else
            {
                // Should never happen as long as I remember
                // to list all types ;)
                string fmt = "{0} cannot be dereferenced.";
                throw new InvalidOperationException(string.Format(fmt, t.Name));
            }

            return (T) o;
        }
    }
}
