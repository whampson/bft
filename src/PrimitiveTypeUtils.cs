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
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace WHampson.Cascara
{
    /// <summary>
    /// Utility class for functionality related to .NET and Cascara primitive data types.
    /// </summary>
    /// <remarks>
    /// Note that for the context this class library, <see cref="System.IntPtr"/> and
    /// <see cref="System.UIntPtr"/> are not considered primitive types.
    /// </remarks>
    internal static class PrimitiveTypeUtils
    {
        public static bool IsPrimitiveType<T>()
        {
            return IsPrimitiveType(typeof(T));
        }

        public static bool IsPrimitiveType(Type t)
        {
            return PrimitiveTypes.Contains(t);
        }

        public static bool IsCharacterType<T>()
        {
            return IsCharacterType(typeof(T));
        }

        public static bool IsCharacterType(Type t)
        {
            return t == typeof(char) || t == typeof(Char8);
        }

        /// <summary>
        /// Gets the size of a primitive type.
        /// </summary>
        /// <remarks>
        /// This method is equivalent to using sizeof() on a primitive type.
        /// However, this method is necessary because sizeof() cannot be used
        /// on a 'Type' object. Additionally, the value returned by Marshal.SizeOf()
        /// is not always equivalent to the value returned by the sizeof() operator.
        /// </remarks>
        /// <typeparam name="T">The type to get the size of. Must be a primitive type.</typeparam>
        /// <returns>The size of the type in bytes.</returns>
        public static int SizeOf<T>()
            where T : struct
        {
            return SizeOf(typeof(T));
        }

        /// <summary>
        /// Gets the size of a primitive type.
        /// </summary>
        /// <remarks>
        /// This method is equivalent to using sizeof() on a primitive type.
        /// However, this method is necessary because sizeof() cannot be used
        /// on a 'Type' object. Additionally, the value returned by Marshal.SizeOf()
        /// is not always equivalent to the value returned by the sizeof() operator.
        /// </remarks>
        /// <param name="t">The type to get the size of. Must be a primitive type.</param>
        /// <returns>The size of the type in bytes.</returns>
        public static int SizeOf(Type t)
        {
            if (!IsPrimitiveType(t))
            {
                throw new ArgumentException(Resources.ArgumentExceptionPrimitiveType, nameof(t));
            }

            return SizeOfMap[t];
        }

        /// <summary>
        /// Gets the bytes that compose the specified value in little-endian byte order.
        /// </summary>
        /// <typeparam name="T">The type of the value to get the bytes of.</typeparam>
        /// <param name="value">The value to get the bytes of.</param>
        /// <returns>An array containing the bytes represented by the specified value.</returns>
        public static byte[] GetBytes<T>(T value)
            where T : struct
        {
            return GetBytes(typeof(T), value);
        }

        /// <summary>
        /// Gets the bytes that compose the specified value in little-endian byte order.
        /// </summary>
        /// <param name="t">The type of the value to get the bytes of.</param>
        /// <param name="value">The value to get the bytes of.</param>
        /// <returns>An array containing the bytes represented by the specified value.</returns>
        public static byte[] GetBytes(Type t, object value)
        {
            if (!IsPrimitiveType(t))
            {
                throw new ArgumentException(Resources.ArgumentExceptionPrimitiveType, nameof(t));
            }
            if (value.GetType() != t)
            {
                throw new ArgumentException(Resources.ArgumentExceptionTypeMismatch, nameof(value));
            }

            // .NET types
            if (t == typeof(bool)) return BitConverter.GetBytes((bool) value);
            if (t == typeof(byte)) return new byte[] { (byte) value };
            if (t == typeof(sbyte)) return new byte[] { (byte) ((sbyte) value) };
            if (t == typeof(char)) return BitConverter.GetBytes((char) value);
            if (t == typeof(double)) return BitConverter.GetBytes((double) value);
            if (t == typeof(float)) return BitConverter.GetBytes((float) value);
            if (t == typeof(int)) return BitConverter.GetBytes((int) value);
            if (t == typeof(uint)) return BitConverter.GetBytes((uint) value);
            if (t == typeof(long)) return BitConverter.GetBytes((long) value);
            if (t == typeof(ulong)) return BitConverter.GetBytes((ulong) value);
            if (t == typeof(short)) return BitConverter.GetBytes((short) value);
            if (t == typeof(ushort)) return BitConverter.GetBytes((ushort) value);

            // Cascara types
            if (t == typeof(Bool16)) return BitConverter.GetBytes((ushort) Convert.ChangeType(value, typeof(ushort)));
            if (t == typeof(Bool32)) return BitConverter.GetBytes((uint) Convert.ChangeType(value, typeof(uint)));
            if (t == typeof(Bool64)) return BitConverter.GetBytes((ulong) Convert.ChangeType(value, typeof(ulong)));
            if (t == typeof(Char8)) return new byte[] { (byte) Convert.ChangeType(value, typeof(byte)) };

            string msg = string.Format("Oops, you found a bug! (Reached the end of {0}.{1} and bytes were not gotten!).",
                nameof(PrimitiveTypeUtils), nameof(GetBytes));
            throw new InvalidOperationException(msg);
        }

        /// <summary>
        /// Gets a primitive type value given a set of bytes.
        /// </summary>
        /// <typeparam name="T">The primitive type to get.</typeparam>
        /// <param name="b">The bytes to convert.</param>
        /// <returns>The primitive type value represented by the bytes provided.</returns>
        public static T GetValue<T>(byte[] b)
            where T : struct
        {
            return (T) GetValue(typeof(T), b);
        }

        /// <summary>
        /// Gets a primitive type value given a set of bytes.
        /// </summary>
        /// <param name="t">The primitive type to get.</param>
        /// <param name="b">The bytes to convert.</param>
        /// <returns>The primitive type value represented by the bytes provided.</returns>
        public static object GetValue(Type t, byte[] b)
        {
            if (!IsPrimitiveType(t))
            {
                throw new ArgumentException(Resources.ArgumentExceptionPrimitiveType, nameof(t));
            }
            if (b.Length == 0)
            {
                throw new ArgumentException(Resources.ArgumentExceptionEmptyCollection, nameof(b));
            }

            // .NET types
            if (t == typeof(bool)) return BitConverter.ToBoolean(b, 0);
            if (t == typeof(byte)) return b[0];
            if (t == typeof(sbyte)) return (sbyte) b[0];
            if (t == typeof(char)) return BitConverter.ToChar(b, 0);
            if (t == typeof(double)) return BitConverter.ToDouble(b, 0);
            if (t == typeof(float)) return BitConverter.ToSingle(b, 0);
            if (t == typeof(int)) return BitConverter.ToInt32(b, 0);
            if (t == typeof(uint)) return BitConverter.ToUInt32(b, 0);
            if (t == typeof(long)) return BitConverter.ToInt64(b, 0);
            if (t == typeof(ulong)) return BitConverter.ToUInt64(b, 0);
            if (t == typeof(short)) return BitConverter.ToInt16(b, 0);
            if (t == typeof(ushort)) return BitConverter.ToUInt16(b, 0);

            // Cascara types
            if (t == typeof(Bool16)) return (Bool16) (BitConverter.ToUInt16(b, 0) != 0);
            if (t == typeof(Bool32)) return (Bool32) (BitConverter.ToUInt32(b, 0) != 0);
            if (t == typeof(Bool64)) return (Bool64) (BitConverter.ToUInt64(b, 0) != 0);
            if (t == typeof(Char8)) return (Char8) ((char) Convert.ChangeType(b[0], typeof(char)));

            string msg = string.Format("Oops, you found a bug! (Reached the end of {0}.{1} and a value was not gotten!).",
            nameof(PrimitiveTypeUtils), nameof(GetValue));
            throw new InvalidOperationException(msg);
        }

        /// <summary>
        /// A lookup table of all primitive types supported by Cascara.
        /// </summary>
        private static readonly HashSet<Type> PrimitiveTypes = new HashSet<Type>()
        {
            // .NET types
            typeof(bool),
            typeof(byte),
            typeof(sbyte),
            typeof(char),
            typeof(double),
            typeof(float),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(short),
            typeof(ushort),

            // Cascara types
            typeof(Bool16),
            typeof(Bool32),
            typeof(Bool64),
            typeof(Char8)
        };

        /// <summary>
        /// A lookup table to find the size of any .NET primitive type.
        /// </summary>
        private static readonly IReadOnlyDictionary<Type, int> SizeOfMap = new Dictionary<Type, int>()
        {
            // .NET types
            { typeof(bool), sizeof(bool) },
            { typeof(byte), sizeof(byte) },
            { typeof(sbyte), sizeof(sbyte) },
            { typeof(char), sizeof(char) },
            { typeof(double), sizeof(double) },
            { typeof(float), sizeof(float) },
            { typeof(int), sizeof(int) },
            { typeof(uint), sizeof(uint) },
            { typeof(long), sizeof(long) },
            { typeof(ulong), sizeof(ulong) },
            { typeof(short), sizeof(short) },
            { typeof(ushort), sizeof(ushort) },

            // Cascara types
            { typeof(Bool16), sizeof(ushort) },
            { typeof(Bool32), sizeof(uint) },
            { typeof(Bool64), sizeof(ulong) },
            { typeof(Char8), sizeof(byte) },
        };
    }
}
