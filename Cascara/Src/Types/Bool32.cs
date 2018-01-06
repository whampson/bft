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

namespace WHampson.Cascara
{
    /// <summary>
    /// A 32-bit true/false value.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 4, Pack = 0)]
    public struct Bool32 : IConvertible, IComparable, IComparable<Bool32>, IEquatable<Bool32>
    {
        private uint m_value;

        private Bool32(bool value)
        {
            m_value = (value) ? 1U : 0U;
        }

        private bool BoolValue
        {
            get { return m_value != 0; }
        }

        public int CompareTo(Bool32 other)
        {
            return BoolValue.CompareTo(other.BoolValue);
        }

        public bool Equals(Bool32 other)
        {
            return BoolValue == other.BoolValue;
        }

        int IComparable.CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            if (!(obj is Bool32))
            {
                string msg = string.Format(Resources.ArgumentExceptionInvalidType, GetType().Name);
                throw new ArgumentException(msg, nameof(obj));
            }

            return CompareTo((Bool32) obj);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Bool32))
            {
                return false;
            }

            return Equals((Bool32) obj);
        }

        public override int GetHashCode()
        {
            return BoolValue.GetHashCode();
        }

        public override string ToString()
        {
            return BoolValue.ToString();
        }

        public static implicit operator Bool32(bool value)
        {
            return new Bool32(value);
        }

        public static implicit operator Bool32(Bool16 value)
        {
            return new Bool32(value);
        }

        public static implicit operator Bool32(Bool64 value)
        {
            return new Bool32(value);
        }

        public static implicit operator bool(Bool32 value)
        {
            return value.BoolValue;
        }

        #region Type Conversion
        public TypeCode GetTypeCode()
        {
            return TypeCode.Boolean;
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            return BoolValue;
        }

        public char ToChar(IFormatProvider provider)
        {
            return Convert.ToChar(BoolValue);
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            return Convert.ToSByte(BoolValue);
        }

        public byte ToByte(IFormatProvider provider)
        {
            return Convert.ToByte(BoolValue);
        }

        public short ToInt16(IFormatProvider provider)
        {
            return Convert.ToInt16(BoolValue);
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            return Convert.ToUInt16(BoolValue);
        }

        public int ToInt32(IFormatProvider provider)
        {
            return Convert.ToInt32(BoolValue);
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            return Convert.ToUInt32(BoolValue);
        }

        public long ToInt64(IFormatProvider provider)
        {
            return Convert.ToInt64(BoolValue);
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            return Convert.ToUInt64(BoolValue);
        }

        public float ToSingle(IFormatProvider provider)
        {
            return Convert.ToSingle(BoolValue);
        }

        public double ToDouble(IFormatProvider provider)
        {
            return Convert.ToDouble(BoolValue);
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            return Convert.ToDecimal(BoolValue);
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            return Convert.ToDateTime(BoolValue);
        }

        public string ToString(IFormatProvider provider)
        {
            return Convert.ToString(BoolValue);
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == GetType())
            {
                return this;
            }

            return Convert.ChangeType(BoolValue, conversionType);
        }
        #endregion
    }
}
