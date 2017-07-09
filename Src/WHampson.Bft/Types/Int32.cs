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

namespace WHampson.Bft.Types
{
    /// <summary>
    /// Represents a 32-bit signed integer.
    /// The .NET analog for ths type is <see cref="int"/>. 
    /// </summary>
    public unsafe struct Int32 : IPrimitiveType
    {
        /// <summary>
        /// Represents the largest (most positive) possible value of
        /// <see cref="Int32"/>.
        /// </summary>
        public static readonly Int32 MaxValue = int.MaxValue;

        /// <summary>
        /// Represents the smallest (most negative) possible value of
        /// <see cref="Int32"/>.
        /// </summary>
        public static readonly Int32 MinValue = int.MinValue;

        /// <summary>
        /// Handles implicit conversion from <see cref="Int32"/>
        /// to <see cref="int"/>.
        /// </summary>
        /// <param name="i32">
        /// The <see cref="Int32"/> to convert to <see cref="int"/>.
        /// </param>
        public static implicit operator int(Int32 i32)
        {
            int val = 0;
            int len = sizeof(Int32);
            for (int i = 0; i < len; i++)
            {
                // Decode data from little-endian format
                val |= i32.data[i] << (i * 8);
            }
            
            return val;
        }

        /// <summary>
        /// Handles implicit conversion from <see cref="int"/>
        /// to <see cref="Int32"/>.
        /// </summary>
        /// <param name="i">
        /// The <see cref="int"/> to convert to <see cref="Int32"/>.
        /// </param>
        public static implicit operator Int32(int i)
        {
            Int32 i32 = new Int32();
            int len = sizeof(Int32);
            for (int k = 0; k < len; k++)
            {
                // Encode data in little-endian format
                i32.data[k] = (byte) ((i >> (k * 8)) & 0xFF);
            }

            return i32;
        }

        public static bool TryParse(string valStr, out Int32 val)
        {
            long v;
            bool success;
            if (success = NumberUtils.TryParseInteger(valStr, out v))
            {
                val = (Int32)v;
            }

            return success;
        }

        // Backing data for Int32.
        private fixed byte data[4];

        bool IPrimitiveType.Equals(IPrimitiveType o)
        {
            if (!(o is Int32))
            {
                return false;
            }

            Int32 other = (Int32) o;
            return this == other;
        }

        byte[] IPrimitiveType.GetBytes()
        {
            int siz = sizeof(Int32);
            byte[] b = new byte[siz];

            fixed (byte* pData = data)
            {
                // Copy bytes from 'data' into 'b'
                for (int i = 0; i < siz; i++)
                {
                    b[i] = *(pData + (i * sizeof(byte)));
                }
            }

            return b;
        }

        public override string ToString()
        {
            // Use the native type's ToString().
            return ((int) this).ToString();
        }
    }

    public interface IBftPrimitive
    {

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BftInt32 : IBftPrimitive, IComparable,
        IComparable<BftInt32>, IEquatable<BftInt32>
    {
        public static readonly BftInt32 MaxValue = 0x7fffffff;
        public static readonly BftInt32 MinValue = unchecked((int) 0x80000000);

        public static implicit operator BftInt32(int value)
        {
            return new BftInt32(value);
        }

        //public static implicit operator int(BftInt32 value)
        //{
        //    return value.m_value;
        //}

        public static explicit operator int(BftInt32 value)
        {
            return value.m_value;
        }

        public static bool operator ==(BftInt32 a, BftInt32 b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(BftInt32 a, BftInt32 b)
        {
            return !a.Equals(b);
        }

        public static bool operator >(BftInt32 a, BftInt32 b)
        {
            return a.CompareTo(b) > 0;
        }

        public static bool operator <(BftInt32 a, BftInt32 b)
        {
            return a.CompareTo(b) < 0;
        }

        public static bool operator >=(BftInt32 a, BftInt32 b)
        {
            return a.CompareTo(b) >= 0;
        }

        public static bool operator <=(BftInt32 a, BftInt32 b)
        {
            return a.CompareTo(b) <= 0;
        }

        private int m_value;

        private BftInt32(int value)
        {
            m_value = value;
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            if (!(obj is BftInt32))
            {
                throw new ArgumentException("Must be BftInt32.");
            }

            return CompareTo((BftInt32) obj);
        }

        public int CompareTo(BftInt32 obj)
        {
            if (m_value < obj.m_value)
            {
                return -1;
            }
            else if (m_value > obj.m_value)
            {
                return 1;
            }

            return 0;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is BftInt32))
            {
                return false;
            }

            return Equals((BftInt32) obj);
        }

        public bool Equals(BftInt32 obj)
        {
            return m_value == obj.m_value;
        }

        public override int GetHashCode()
        {
            return m_value;
        }

        public override string ToString()
        {
            return m_value.ToString();
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct BftBool64 : IBftPrimitive, IComparable,
        IComparable<BftBool64>, IEquatable<BftBool64>
    {
        private const int True = 1;
        private const int False = 0;

        private const ulong TrueUL = 1ul;
        private const ulong FalseUL = 0ul;

        public static implicit operator BftBool64(bool value)
        {
            return new BftBool64(value);
        }

        //public static implicit operator int(BftInt32 value)
        //{
        //    return value.m_value;
        //}

        public static explicit operator bool(BftBool64 value)
        {
            return value.BoolValue();
        }

        public static bool operator ==(BftBool64 a, BftBool64 b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(BftBool64 a, BftBool64 b)
        {
            return !a.Equals(b);
        }

        private ulong m_value;

        private BftBool64(bool value)
        {
            m_value = (value) ? TrueUL : FalseUL;
        }

        private bool BoolValue()
        {
            return m_value != 0;
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            if (!(obj is BftBool64))
            {
                throw new ArgumentException("Must be BftBool64.");
            }

            return CompareTo((BftBool64) obj);
        }

        public int CompareTo(BftBool64 obj)
        {
            if (BoolValue() == obj.BoolValue())
            {
                return 0;
            }
            else if (BoolValue() == false)
            {
                return -1;
            }

            return 1;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is BftBool64))
            {
                return false;
            }

            return Equals((BftBool64) obj);
        }

        public bool Equals(BftBool64 obj)
        {
            return BoolValue() == obj.BoolValue();
        }

        public override int GetHashCode()
        {
            return (m_value != 0) ? True : False;
        }

        public override string ToString()
        {
            return BoolValue().ToString();
        }
    }
}
