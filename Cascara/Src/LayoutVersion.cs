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

namespace WHampson.Cascara
{
    /// <summary>
    /// 
    /// </summary>
    internal struct LayoutVersion : IComparable<LayoutVersion>
    {
        /// <summary>
        /// Creates a new <see cref="LayoutVersion"/> struct using
        /// the specified major and minor numbers.
        /// </summary>
        /// <param name="major"></param>
        /// <param name="minor"></param>
        public LayoutVersion(int major, int minor, int patch)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
        }

        public int Major { get; }
        public int Minor { get; }
        public int Patch { get; }

        public override bool Equals(object obj)
        {
            if (!(obj is LayoutVersion))
            {
                return false;
            }

            LayoutVersion other = (LayoutVersion) obj;

            return this == other;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 37) ^ Major;
                hash = (hash * 37) ^ Minor;

                return hash;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}.{1}.{2}", Major, Minor, Patch);
        }

        public int CompareTo(LayoutVersion other)
        {
            if (this > other)
            {
                return -1;
            }

            if (this == other)
            {
                return 0;
            }

            return 1;
        }

        public static bool operator ==(LayoutVersion a, LayoutVersion b)
        {
            return a.Major == b.Major
                && a.Minor == b.Minor
                && a.Patch == b.Patch;
        }

        public static bool operator !=(LayoutVersion a, LayoutVersion b)
        {
            return !(a == b);
        }

        public static bool operator >(LayoutVersion a, LayoutVersion b)
        {
            if (a.Major > b.Major)
            {
                return true;
            }
            else if (a.Major < b.Major)
            {
                return false;
            }

            if (a.Minor > b.Minor)
            {
                return true;
            }
            else if (a.Minor < b.Minor)
            {
                return false;
            }

            return a.Patch > b.Patch;
        }

        public static bool operator <(LayoutVersion a, LayoutVersion b)
        {
            return !(a > b) && a != b;
        }

        public static bool operator >=(LayoutVersion a, LayoutVersion b)
        {
            return a > b || a == b;
        }

        public static bool operator <=(LayoutVersion a, LayoutVersion b)
        {
            return a < b || a == b;
        }
    }
}
