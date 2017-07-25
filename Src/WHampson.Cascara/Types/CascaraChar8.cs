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
    /// An 8-bit character value.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CascaraChar8 : ICascaraType,
        IComparable, IComparable<CascaraChar8>, IEquatable<CascaraChar8>
    {
        private const int Size = 1;

        private byte m_value;

        private CascaraChar8(char value)
        {
            m_value = (byte) value;
        }

        public int CompareTo(CascaraChar8 other)
        {
            return m_value - other.m_value;
        }

        public bool Equals(CascaraChar8 other)
        {
            return m_value == other.m_value;
        }

        int IComparable.CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            if (!(obj is CascaraChar8))
            {
                string fmt = "Object is not an instance of {0}.";
                string msg = string.Format(fmt, GetType().Name);

                throw new ArgumentException(msg, "obj");
            }

            return CompareTo((CascaraChar8) obj);
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
            if (!(obj is CascaraChar8))
            {
                return false;
            }

            return Equals((CascaraChar8) obj);
        }

        public override int GetHashCode()
        {
            return m_value | (m_value << 8);
        }

        public override string ToString()
        {
            return ((char) m_value).ToString();
        }

        public static implicit operator CascaraChar8(char value)
        {
            return new CascaraChar8(value);
        }

        public static explicit operator char(CascaraChar8 value)
        {
            return (char) value.m_value;
        }
    }
}
