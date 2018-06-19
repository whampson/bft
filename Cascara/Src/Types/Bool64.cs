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
    /// A 64-bit true/false value.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 8, Pack = 0)]
    public struct Bool64 : IConvertible, IComparable, IComparable<Bool64>, IEquatable<Bool64>
    {
        private ulong m_value;

        private Bool64(bool value)
        {
            m_value = (value) ? 1UL : 0UL;
        }

        private bool BoolValue
        {
            get { return m_value != 0; }
        }

        /// <summary>
        /// Compares the current instance with another object of the same type
        /// and returns an integer that indicates whether the current instance
        /// precedes, follows, or occurs in the same position in the sort order
        /// as the other object.
        /// </summary>
        public int CompareTo(Bool64 other)
        {
            return BoolValue.CompareTo(other.BoolValue);
        }

        int IComparable.CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            if (!(obj is Bool64))
            {
                string msg = string.Format(Resources.ArgumentExceptionInvalidType, GetType().Name);
                throw new ArgumentException(msg, nameof(obj));
            }

            return CompareTo((Bool64) obj);
        }

        /// <summary>
        /// Determines whether the specified object is equal to this object.
        /// </summary>
        public bool Equals(Bool64 other)
        {
            return BoolValue == other.BoolValue;
        }

        /// <summary>
        /// Determines whether the specified object is equal to this object.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (!(obj is Bool64))
            {
                return false;
            }

            return Equals((Bool64) obj);
        }

        /// <summary>
        /// Serves as this object's default hash function.
        /// </summary>
        public override int GetHashCode()
        {
            return BoolValue.GetHashCode();
        }

        /// <summary>
        /// Returns a string that represents this object.
        /// </summary>
        public override string ToString()
        {
            return BoolValue.ToString();
        }

        /// <summary>
        /// Implicitly casts this value to a <see cref="Bool64"/> value.
        /// </summary>
        public static implicit operator Bool64(bool value)
        {
            return new Bool64(value);
        }

        /// <summary>
        /// Implicitly casts this value to a <see cref="Bool64"/> value.
        /// </summary>
        public static implicit operator Bool64(Bool16 value)
        {
            return new Bool64(value);
        }

        /// <summary>
        /// Implicitly casts this value to a <see cref="Bool64"/> value.
        /// </summary>
        public static implicit operator Bool64(Bool32 value)
        {
            return new Bool64(value);
        }

        /// <summary>
        /// Implicitly casts this value to a <see cref="bool"/> value.
        /// </summary>
        public static implicit operator bool(Bool64 value)
        {
            return value.BoolValue;
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
            return BoolValue;
        }

        /// <summary>
        /// Converts this value to the equivalent <see cref="char"/> value.
        /// </summary>
        public char ToChar(IFormatProvider provider)
        {
            return Convert.ToChar(BoolValue);
        }

        /// <summary>
        /// Converts this value to the equivalent <see cref="sbyte"/> value.
        /// </summary>
        public sbyte ToSByte(IFormatProvider provider)
        {
            return Convert.ToSByte(BoolValue);
        }

        /// <summary>
        /// Converts this value to the equivalent <see cref="byte"/> value.
        /// </summary>
        public byte ToByte(IFormatProvider provider)
        {
            return Convert.ToByte(BoolValue);
        }

        /// <summary>
        /// Converts this value to the equivalent <see cref="short"/> value.
        /// </summary>
        public short ToInt16(IFormatProvider provider)
        {
            return Convert.ToInt16(BoolValue);
        }

        /// <summary>
        /// Converts this value to the equivalent <see cref="ushort"/> value.
        /// </summary>
        public ushort ToUInt16(IFormatProvider provider)
        {
            return Convert.ToUInt16(BoolValue);
        }

        /// <summary>
        /// Converts this value to the equivalent <see cref="int"/> value.
        /// </summary>
        public int ToInt32(IFormatProvider provider)
        {
            return Convert.ToInt32(BoolValue);
        }

        /// <summary>
        /// Converts this value to the equivalent <see cref="uint"/> value.
        /// </summary>
        public uint ToUInt32(IFormatProvider provider)
        {
            return Convert.ToUInt32(BoolValue);
        }

        /// <summary>
        /// Converts this value to the equivalent <see cref="long"/> value.
        /// </summary>
        public long ToInt64(IFormatProvider provider)
        {
            return Convert.ToInt64(BoolValue);
        }

        /// <summary>
        /// Converts this value to the equivalent <see cref="ulong"/> value.
        /// </summary>
        public ulong ToUInt64(IFormatProvider provider)
        {
            return Convert.ToUInt64(BoolValue);
        }

        /// <summary>
        /// Converts this value to the equivalent <see cref="float"/> value.
        /// </summary>
        public float ToSingle(IFormatProvider provider)
        {
            return Convert.ToSingle(BoolValue);
        }

        /// <summary>
        /// Converts this value to the equivalent <see cref="double"/> value.
        /// </summary>
        public double ToDouble(IFormatProvider provider)
        {
            return Convert.ToDouble(BoolValue);
        }

        /// <summary>
        /// Converts this value to the equivalent <see cref="decimal"/> value.
        /// </summary>
        public decimal ToDecimal(IFormatProvider provider)
        {
            return Convert.ToDecimal(BoolValue);
        }

        /// <summary>
        /// Converts this value to the equivalent <see cref="DateTime"/> value.
        /// </summary>
        public DateTime ToDateTime(IFormatProvider provider)
        {
            return Convert.ToDateTime(BoolValue);
        }

        /// <summary>
        /// Converts this value to the equivalent <see cref="string"/> value.
        /// </summary>
        public string ToString(IFormatProvider provider)
        {
            return Convert.ToString(BoolValue);
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

            return Convert.ChangeType(BoolValue, conversionType);
        }
    }
}
