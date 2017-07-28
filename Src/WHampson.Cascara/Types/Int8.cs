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
    /// An 8-bit signed integer.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Int8 : ICascaraType,
        IComparable<Int8>, IEquatable<Int8>
    {
        private const int Size = 1;

        private sbyte m_value;

        private Int8(sbyte value)
        {
            m_value = value;
        }

        public int CompareTo(Int8 other)
        {
            return m_value.CompareTo(other);
        }

        public bool Equals(Int8 other)
        {
            return m_value == other.m_value;
        }

        int IComparable.CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            if (!(obj is Int8))
            {
                string fmt = "Object is not an instance of {0}.";
                string msg = string.Format(fmt, GetType().Name);

                throw new ArgumentException(msg, "obj");
            }

            return CompareTo((Int8) obj);
        }

        byte[] ICascaraType.GetBytes()
        {
            return new byte[] { (byte) m_value };
        }

        int ICascaraType.GetSize()
        {
            return Size;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Int8))
            {
                return false;
            }

            return Equals((Int8) obj);
        }

        public override int GetHashCode()
        {
            return m_value.GetHashCode();
        }

        public override string ToString()
        {
            return m_value.ToString();
        }

        public static implicit operator Int8(sbyte value)
        {
            return new Int8(value);
        }

        public static explicit operator sbyte(Int8 value)
        {
            return value.m_value;
        }

        #region IConvertible
        public TypeCode GetTypeCode()
        {
            return TypeCode.SByte;
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            return Convert.ToBoolean(m_value);
        }

        public char ToChar(IFormatProvider provider)
        {
            return Convert.ToChar(m_value);
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            return Convert.ToSByte(m_value);
        }

        public byte ToByte(IFormatProvider provider)
        {
            return Convert.ToByte(m_value);
        }

        public short ToInt16(IFormatProvider provider)
        {
            return Convert.ToInt16(m_value);
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            return Convert.ToUInt16(m_value);
        }

        public int ToInt32(IFormatProvider provider)
        {
            return Convert.ToInt32(m_value);
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            return Convert.ToUInt32(m_value);
        }

        public long ToInt64(IFormatProvider provider)
        {
            return Convert.ToInt64(m_value);
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            return Convert.ToUInt64(m_value);
        }

        public float ToSingle(IFormatProvider provider)
        {
            return Convert.ToSingle(m_value);
        }

        public double ToDouble(IFormatProvider provider)
        {
            return Convert.ToDouble(m_value);
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            return Convert.ToDecimal(m_value);
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            return Convert.ToDateTime(m_value);
        }

        public string ToString(IFormatProvider provider)
        {
            return Convert.ToString(m_value);
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            string fmt = "Cannot convert {0} to {1}.";
            string msg = string.Format(fmt, GetType().Name, conversionType.Name);

            throw new InvalidCastException(msg);
        }
        #endregion
    }
}
