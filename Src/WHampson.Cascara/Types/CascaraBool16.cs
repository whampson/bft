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
    /// A 16-bit true/false value.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CascaraBool16 : ICascaraType,
        IComparable, IComparable<CascaraBool16>, IEquatable<CascaraBool16>
    {
        private const int Size = 2;

        private CascaraInt16 m_value;

        private CascaraBool16(bool value)
        {
            m_value = (short) ((value) ? 1 : 0);
        }

        private bool BoolValue
        {
            get { return (int) m_value != 0; }
        }

        public int CompareTo(CascaraBool16 other)
        {
            return BoolValue.CompareTo(other.BoolValue);
        }

        public bool Equals(CascaraBool16 other)
        {
            return BoolValue == other.BoolValue;
        }

        int IComparable.CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            if (!(obj is CascaraBool16))
            {
                string fmt = "Object is not an instance of {0}.";
                string msg = string.Format(fmt, GetType().Name);

                throw new ArgumentException(msg, "obj");
            }

            return CompareTo((CascaraBool16) obj);
        }

        byte[] ICascaraType.GetBytes()
        {
            return ((ICascaraType) m_value).GetBytes();
        }

        int ICascaraType.GetSize()
        {
            return Size;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is CascaraBool16))
            {
                return false;
            }

            return Equals((CascaraBool16) obj);
        }

        public override int GetHashCode()
        {
            return BoolValue.GetHashCode();
        }

        public override string ToString()
        {
            return BoolValue.ToString();
        }

        public static implicit operator CascaraBool16(bool value)
        {
            return new CascaraBool16(value);
        }

        public static explicit operator bool(CascaraBool16 value)
        {
            return value.BoolValue;
        }
    }
}
