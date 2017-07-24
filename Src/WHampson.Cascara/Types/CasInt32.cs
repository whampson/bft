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
    /// 
    /// </summary>
    /// <remarks>
    /// 'Cas' perfix added to avoid collision with <see cref="System.Int32"/>.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct CasInt32 : ICascaraType,
        IComparable, IComparable<CasInt32>, IEquatable<CasInt32>
    {
        private const int Size = 4;

        private int m_value;

        private CasInt32(int value)
        {
            m_value = value;
        }

        public int CompareTo(CasInt32 other)
        {
            if (m_value < other.m_value)
            {
                return -1;
            }
            else if (m_value > other.m_value)
            {
                return 1;
            }

            return 0;
        }

        public bool Equals(CasInt32 other)
        {
            return m_value == other.m_value;
        }

        int IComparable.CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            if (!(obj is CasInt32))
            {
                string fmt = "Object is not an instance of {0}.";
                string msg = string.Format(fmt, GetType().Name);

                throw new ArgumentException(msg, "obj");
            }

            return CompareTo((CasInt32) obj);
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
            if (!(obj is CasInt32))
            {
                return false;
            }

            return Equals((CasInt32) obj);
        }

        public override int GetHashCode()
        {
            return m_value;
        }

        public override string ToString()
        {
            return m_value.ToString();
        }

        public static implicit operator CasInt32(int value)
        {
            return new CasInt32(value);
        }

        public static explicit operator int(CasInt32 value)
        {
            return value.m_value;
        }
    }
}
