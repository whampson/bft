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
using System.Text;

namespace WHampson.Cascara
{
    public enum Endianness
    {
        BigEndian,
        LittleEndian
    }

    public class BinaryFile : IDisposable
    {
        private bool hasBeenDisposed;
        private IntPtr dataPtr;

        private BinaryFile()
        {
            hasBeenDisposed = false;
            dataPtr = IntPtr.Zero;
            Endianness = Endianness.LittleEndian;
        }

        public BinaryFile(int length)
            : this()
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            dataPtr = Marshal.AllocHGlobal(length);
            Length = length;
        }

        public Endianness Endianness
        {
            get;
            set;
        }

        public int Length
        {
            get;
        }

        public byte this[int offset]
        {
            get { return GetByte(offset); }
            set { SetByte(offset, value); }
        }

        public byte GetByte(int offset)
        {
            return GetBytes(offset, 1)[0];
        }

        public byte[] GetBytes(int offset, int length)
        {
            if (offset < 0 || offset >= Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
            if (offset + length > Length)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            byte[] b = new byte[length];
            Marshal.Copy(dataPtr + offset, b, 0, length);

            return b;
        }

        public T GetValue<T>(int offset)
            where T : struct
        {
            Type t = typeof(T);
            if (!t.IsPrimitive)
            {
                throw new ArgumentException(Resources.ArgumentExceptionPrimitiveType, nameof(T));
            }

            int size = SizeOf<T>();
            if (offset + size > Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), Resources.ArgumentExceptionOutOfRangeForType);
            }

            if (t == typeof(byte))
            {
                return (T) Convert.ChangeType(GetByte(offset), t);
            }
            else if (t == typeof(sbyte))
            {
                return (T) Convert.ChangeType((sbyte) GetByte(offset), t);
            }

            byte[] b = GetBytes(offset, size);
            if (Endianness == Endianness.BigEndian)
            {
                b = b.Reverse().ToArray();
            }

            object value = default(object);
            if (t == typeof(bool))
            {
                value = BitConverter.ToBoolean(b, 0);
            }
            else if (t == typeof(char))
            {
                value = BitConverter.ToChar(b, 0);
            }
            else if (t == typeof(int))
            {
                value = BitConverter.ToInt32(b, 0);
            }

            if (value == null)
            {
                throw new InvalidOperationException("Bug!!");
            }

            return (T) Convert.ChangeType(value, t);
        }

        public void SetByte(int offset, byte value)
        {
            SetBytes(offset, new byte[] { value });
        }

        public void SetBytes(int offset, byte[] b)
        {
            if (offset < 0 || offset >= Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
            if (offset + b.Length > Length)
            {
                throw new ArgumentException("blah", nameof(b));
            }

            Marshal.Copy(b, 0, dataPtr + offset, b.Length);
        }

        //public void SetValue<T>(int offset, T value) where T : struct
        //{
        //    if (typeof(T).IsPrimitive)
        //    {
        //        // TODO: message
        //        throw new ArgumentException("blah", nameof(T));
        //    }
        //    if (Endianness == Endianness.BigEndian)
        //    {
        //        b = b.Reverse().ToArray();
        //    }
        //}

        #region Disposal
        protected virtual void Dispose(bool disposing)
        {
            if (!hasBeenDisposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                if (dataPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(dataPtr);
                    dataPtr = IntPtr.Zero;
                }

                hasBeenDisposed = true;
            }
        }

        ~BinaryFile()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        private static int SizeOf<T>()
            where T : struct
        {
            Type t = typeof(T);
            if (t.IsPrimitive)
            {
                return PrimitiveTypeSizeDictionary[t];
            }

            return Marshal.SizeOf<T>();
        }

        private static readonly IReadOnlyDictionary<Type, int> PrimitiveTypeSizeDictionary = new Dictionary<Type, int>()
        {
            { typeof(bool), sizeof(bool) },
            { typeof(char), sizeof(char) },
            { typeof(int), sizeof(int) },
        };
    }
}
