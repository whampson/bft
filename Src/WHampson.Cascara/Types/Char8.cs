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
    public struct Char8 : ICascaraType,
        IComparable, IComparable<Char8>, IEquatable<Char8>
    {
        private const int Size = 1;

        private byte m_value;

        private Char8(char value)
        {
            m_value = (byte) value;
        }

        public int CompareTo(Char8 other)
        {
            return ((char) m_value).CompareTo((char) other.m_value);
        }

        public bool Equals(Char8 other)
        {
            return ((char) m_value) == ((char) other.m_value);
        }

        int IComparable.CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            if (!(obj is Char8))
            {
                string fmt = "Object is not an instance of {0}.";
                string msg = string.Format(fmt, GetType().Name);

                throw new ArgumentException(msg, "obj");
            }

            return CompareTo((Char8) obj);
        }

        byte[] ICascaraType.GetBytes()
        {
            return new byte[] { m_value };
        }

        int ICascaraType.GetSize()
        {
            return Size;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Char8))
            {
                return false;
            }

            return Equals((Char8) obj);
        }

        public override int GetHashCode()
        {
            return ((char) m_value).GetHashCode();
        }

        public override string ToString()
        {
            return ((char) m_value).ToString();
        }

        public static implicit operator Char8(char value)
        {
            return new Char8(value);
        }

        public static explicit operator char(Char8 value)
        {
            return (char) value.m_value;
        }
    }
}
