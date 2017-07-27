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
    /// A 32-bit unsigned integer.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct UInt32 : ICascaraType,
        IComparable, IComparable<UInt32>, IEquatable<UInt32>
    {
        private const int Size = 4;

        private uint m_value;

        private UInt32(uint value)
        {
            m_value = value;
        }

        public int CompareTo(UInt32 other)
        {
            return m_value.CompareTo(other.m_value);
        }

        public bool Equals(UInt32 other)
        {
            return m_value == other.m_value;
        }

        int IComparable.CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            if (!(obj is UInt32))
            {
                string fmt = "Object is not an instance of {0}.";
                string msg = string.Format(fmt, GetType().Name);

                throw new ArgumentException(msg, "obj");
            }

            return CompareTo((UInt32) obj);
        }

        byte[] ICascaraType.GetBytes()
        {
            return BitConverter.GetBytes(m_value);
        }

        int ICascaraType.GetSize()
        {
            return Size;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is UInt32))
            {
                return false;
            }

            return Equals((UInt32) obj);
        }

        public override int GetHashCode()
        {
            return m_value.GetHashCode();
        }

        public override string ToString()
        {
            return m_value.ToString();
        }

        public static implicit operator UInt32(uint value)
        {
            return new UInt32(value);
        }

        public static explicit operator uint(UInt32 value)
        {
            return value.m_value;
        }
    }
}
