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
    /// <summary>
    /// Represents a non-text file.
    /// </summary>
    public class BinaryFile : IDisposable
    {
        private bool hasBeenDisposed;
        private IntPtr dataPtr;

        private BinaryFile(Endianness endianness)
        {
            if (endianness == null)
            {
                throw new ArgumentNullException(nameof(endianness));
            }

            hasBeenDisposed = false;
            dataPtr = IntPtr.Zero;
            Endianness = endianness;
        }

        /// <summary>
        /// Initializes a new <see cref="BinaryFile"/> object of the specified length
        /// with little-endian byte order.
        /// </summary>
        /// <param name="length">The number of bytes to allocate.</param>
        /// <exception cref="OutOfMemoryException">
        /// Thrown if there is not enough memory to create the <see cref="BinaryFile"/>
        /// with the length provided.
        /// </exception>
        public BinaryFile(int length)
            : this(length, Endianness.Little)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="BinaryFile"/> object of the specified length.
        /// </summary>
        /// <param name="length">The number of bytes to allocate.</param>
        /// <param name="endianness">The byte order for primitive types.</param>
        /// <exception cref="OutOfMemoryException">
        /// Thrown if there is not enough memory to create the <see cref="BinaryFile"/>
        /// with the length provided.
        /// </exception>
        public BinaryFile(int length, Endianness endianness)
            : this(endianness)
        {
            if (length < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            dataPtr = Marshal.AllocHGlobal(length);
            Length = length;
        }

        /// <summary>
        /// Initializes a new <see cref="BinaryFile"/> object with data
        /// from the specified byte array with little-endian byte order.
        /// </summary>
        /// <param name="data">The array to initialize the file with.</param>
        /// <exception cref="OutOfMemoryException">
        /// Thrown if there is not enough memory to create the <see cref="BinaryFile"/>
        /// with the length provided.
        /// </exception>
        public BinaryFile(byte[] data)
            : this(data, Endianness.Little)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="BinaryFile"/> object with data
        /// from the specified byte array.
        /// </summary>
        /// <param name="data">The array to initialize the file with.</param>
        /// <param name="endianness">The byte order for primitive types.</param>
        /// <exception cref="OutOfMemoryException">
        /// Thrown if there is not enough memory to create the <see cref="BinaryFile"/>
        /// with the length provided.
        /// </exception>
        public BinaryFile(byte[] data, Endianness endianness)
            : this(endianness)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            if (data.Length < 1)
            {
                throw new ArgumentException(Resources.ArgumentExceptionEmptyCollection, nameof(data));
            }

            dataPtr = Marshal.AllocHGlobal(data.Length);
            Length = data.Length;

            SetBytes(0, data);
        }

        /// <summary>
        /// Gets or sets the byte order.
        /// The value of this property affects the values of primitive types
        /// returned by <see cref="GetValue{T}(int)"/> and set by <see cref="SetValue{T}(int, T)"/>.
        /// </summary>
        public Endianness Endianness
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the number of bytes in the <see cref="BinaryFile"/>.
        /// </summary>
        public int Length
        {
            get;
        }

        /// <summary>
        /// Gets or sets a byte in the file data.
        /// </summary>
        /// <param name="offset">The position in the file data of the byte to get or set.</param>
        /// <returns>The value of the byte at the offset.</returns>
        public byte this[int offset]
        {
            get { return GetByte(offset); }
            set { SetByte(offset, value); }
        }

        /// <summary>
        /// Gets the value of a byte in the file data.
        /// </summary>
        /// <param name="offset">The position in the file data of the byte to get.</param>
        /// <returns>The value of the byte at the offset.</returns>
        public byte GetByte(int offset)
        {
            return GetBytes(offset, 1)[0];
        }

        /// <summary>
        /// Gets a contiguous array of bytes from the file data.
        /// </summary>
        /// <param name="offset">The position in the file data of the first byte to get.</param>
        /// <param name="length">The number of bytes to get.</param>
        /// <returns>
        /// An array of the specified length containing the values of all bytes
        /// beginning at the specified offset.
        /// </returns>
        public byte[] GetBytes(int offset, int length)
        {
            if (offset < 0 || offset >= Length)
            {
                throw new ArgumentException(Resources.ArgumentExceptionOffsetTooLarge, nameof(offset));
            }
            if (offset + length > Length)
            {
                throw new ArgumentException(Resources.ArgumentExceptionLengthTooLarge, nameof(length));
            }

            byte[] b = new byte[length];
            Marshal.Copy(dataPtr + offset, b, 0, length);

            return b;
        }

        /// <summary>
        /// Gets the value of a primitive type in the file data.
        /// </summary>
        /// <typeparam name="T">The type of value to get.</typeparam>
        /// <param name="offset">The position in the file data of the value to get.</param>
        /// <returns>The value of the primitive type at the specified offset.</returns>
        public T GetValue<T>(int offset)
            where T : struct
        {
            Type t = typeof(T);
            if (!t.IsPrimitive)
            {
                throw new ArgumentException(Resources.ArgumentExceptionPrimitiveType, nameof(T));
            }

            byte[] b = GetBytes(offset, PrimitiveTypeUtils.SizeOf<T>());
            if (Endianness == Endianness.Big)
            {
                b = b.Reverse().ToArray();
            }

            return PrimitiveTypeUtils.GetValue<T>(b);
        }

        /// <summary>
        /// Sets the value of a byte in the file data.
        /// </summary>
        /// <param name="offset">The position in the file data of the byte to set.</param>
        /// <param name="value">The value to write at the offset.</param>
        public void SetByte(int offset, byte value)
        {
            SetBytes(offset, new byte[] { value });
        }

        /// <summary>
        /// Sets the values of a contiguous array of bytes in the file data.
        /// </summary>
        /// <param name="offset">The position in the file data of the first byte to set.</param>
        /// <param name="b">The bytes to write at the offset.</param>
        public void SetBytes(int offset, byte[] b)
        {
            if (offset < 0 || offset >= Length)
            {
                throw new ArgumentException(Resources.ArgumentExceptionOffsetTooLarge, nameof(offset));
            }
            if (offset + b.Length > Length)
            {
                throw new ArgumentException(Resources.ArgumentExceptionLengthTooLarge, nameof(b));
            }

            Marshal.Copy(b, 0, dataPtr + offset, b.Length);
        }

        /// <summary>
        /// Sets the value of a primitive type in the file data.
        /// </summary>
        /// <typeparam name="T">The type of value to set.</typeparam>
        /// <param name="offset">The position in the file data of the value to set.</param>
        /// <param name="value">The value to write at the offset.</param>
        public void SetValue<T>(int offset, T value)
            where T : struct
        {
            Type t = typeof(T);
            if (!t.IsPrimitive)
            {
                throw new ArgumentException(Resources.ArgumentExceptionPrimitiveType, nameof(T));
            }

            byte[] b = PrimitiveTypeUtils.GetBytes(value);
            if (Endianness == Endianness.Big)
            {
                b = b.Reverse().ToArray();
            }

            SetBytes(offset, b);
        }

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

        public override string ToString()
        {
            const int MaxBytes = 16;

            byte[] data = GetBytes(0, Math.Min(Length, MaxBytes));
            string dataStr = "";
            foreach (byte b in data)
            {
                dataStr += string.Format(" {0:X2}", b);
            }
            dataStr = (Length > MaxBytes) ? dataStr.Trim() + "..." : dataStr.Trim();

            return string.Format("{0}: [ {1} = {2}, {3} = {4}, {5} = {6} ]",
                GetType().Name,
                nameof(Length), Length,
                nameof(Endianness), Endianness,
                "Data", dataStr);
        }
    }

    /// <summary>
    /// Specifies the byte order for multi-byte primitive data types.
    /// </summary>
    public enum Endianness
    {
        /// <summary>
        /// The most significant byte goes first.
        /// </summary>
        Big,

        /// <summary>
        /// The least significant byte goes first.
        /// </summary>
        Little
    }
}
