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
using System.Runtime.InteropServices;

namespace WHampson.Cascara
{
    /// <summary>
    /// An 8-bit character value.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 1, Pack = 0)]
    public struct Char8 : IConvertible, IComparable,IComparable<Char8>, IEquatable<Char8>
    {
        private byte m_value;

        private Char8(char value)
        {
            m_value = (byte) value;
        }

        private char CharValue
        {
            get { return (char) m_value; }
        }

        /// <summary>
        /// Compares the current instance with another object of the same type
        /// and returns an integer that indicates whether the current instance
        /// precedes, follows, or occurs in the same position in the sort order
        /// as the other object.
        /// </summary>
        public int CompareTo(Char8 other)
        {
            return CharValue.CompareTo(other.CharValue);
        }

        int IComparable.CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            if (!(obj is Char8))
            {
                string msg = string.Format(Resources.ArgumentExceptionInvalidType, GetType().Name);
                throw new ArgumentException(msg, nameof(obj));
            }

            return CompareTo((Char8) obj);
        }

        /// <summary>
        /// Determines whether the specified object is equal to this object.
        /// </summary>
        public bool Equals(Char8 other)
        {
            return CharValue == other.CharValue;
        }

        /// <summary>
        /// Determines whether the specified object is equal to this object.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (!(obj is Char8))
            {
                return false;
            }

            return Equals((Char8) obj);
        }

        /// <summary>
        /// Serves as this object's default hash function.
        /// </summary>
        public override int GetHashCode()
        {
            return CharValue.GetHashCode();
        }

        /// <summary>
        /// Returns a string that represents this object.
        /// </summary>
        public override string ToString()
        {
            return CharValue.ToString();
        }

        /// <summary>
        /// Implicitly casts this value to a <see cref="Char8"/> value.
        /// </summary>
        public static implicit operator Char8(char value)
        {
            return new Char8(value);
        }

        /// <summary>
        /// Implicitly casts this value to a <see cref="char"/> value.
        /// </summary>
        public static implicit operator char(Char8 value)
        {
            return value.CharValue;
        }

        /// <summary>
        /// Returns the <see cref="TypeCode"/> for this object.
        /// </summary>
        public TypeCode GetTypeCode()
        {
            return TypeCode.Boolean;
        }

        /// <summary>
        /// Converts this value to the equivalent <see cref="bool"/> value.
        /// </summary>
        public bool ToBoolean(IFormatProvider provider)
        {
            return Convert.ToBoolean(CharValue);
        }

        /// <summary>
        /// Converts this value to the equivalent <see cref="char"/> value.
        /// </summary>
        public char ToChar(IFormatProvider provider)
        {
            return CharValue;
        }

        /// <summary>
        /// Converts this value to the equivalent <see cref="sbyte"/> value.
        /// </summary>
        public sbyte ToSByte(IFormatProvider provider)
        {
            return Convert.ToSByte(CharValue);
        }

        /// <summary>
        /// Converts this value to the equivalent <see cref="byte"/> value.
        /// </summary>
        public byte ToByte(IFormatProvider provider)
        {
            return Convert.ToByte(CharValue);
        }

        /// <summary>
        /// Converts this value to the equivalent <see cref="short"/> value.
        /// </summary>
        public short ToInt16(IFormatProvider provider)
        {
            return Convert.ToInt16(CharValue);
        }

        /// <summary>
        /// Converts this value to the equivalent <see cref="ushort"/> value.
        /// </summary>
        public ushort ToUInt16(IFormatProvider provider)
        {
            return Convert.ToUInt16(CharValue);
        }

        /// <summary>
        /// Converts this value to the equivalent <see cref="int"/> value.
        /// </summary>
        public int ToInt32(IFormatProvider provider)
        {
            return Convert.ToInt32(CharValue);
        }

        /// <summary>
        /// Converts this value to the equivalent <see cref="uint"/> value.
        /// </summary>
        public uint ToUInt32(IFormatProvider provider)
        {
            return Convert.ToUInt32(CharValue);
        }

        /// <summary>
        /// Converts this value to the equivalent <see cref="long"/> value.
        /// </summary>
        public long ToInt64(IFormatProvider provider)
        {
            return Convert.ToInt64(CharValue);
        }

        /// <summary>
        /// Converts this value to the equivalent <see cref="ulong"/> value.
        /// </summary>
        public ulong ToUInt64(IFormatProvider provider)
        {
            return Convert.ToUInt64(CharValue);
        }

        /// <summary>
        /// Converts this value to the equivalent <see cref="float"/> value.
        /// </summary>
        public float ToSingle(IFormatProvider provider)
        {
            return Convert.ToSingle(CharValue);
        }

        /// <summary>
        /// Converts this value to the equivalent <see cref="double"/> value.
        /// </summary>
        public double ToDouble(IFormatProvider provider)
        {
            return Convert.ToDouble(CharValue);
        }

        /// <summary>
        /// Converts this value to the equivalent <see cref="decimal"/> value.
        /// </summary>
        public decimal ToDecimal(IFormatProvider provider)
        {
            return Convert.ToDecimal(CharValue);
        }

        /// <summary>
        /// Converts this value to the equivalent <see cref="DateTime"/> value.
        /// </summary>
        public DateTime ToDateTime(IFormatProvider provider)
        {
            return Convert.ToDateTime(CharValue);
        }

        /// <summary>
        /// Converts this value to the equivalent <see cref="string"/> value.
        /// </summary>
        public string ToString(IFormatProvider provider)
        {
            return Convert.ToString(CharValue);
        }

        /// <summary>
        /// Converts this value to an equivalent value of the specified type.
        /// </summary>
        public object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == GetType())
            {
                return this;
            }

            return Convert.ChangeType(CharValue, conversionType);
        }
    }
}
