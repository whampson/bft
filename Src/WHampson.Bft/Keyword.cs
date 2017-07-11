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

namespace WHampson.Bft
{
    internal class Keyword : IEquatable<string>
    {
        public Keyword(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public bool Equals(string value)
        {
            return string.Equals(Value, value);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Keyword))
            {
                return false;
            }

            Keyword other = obj as Keyword;
            return Equals(other.Value);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value;
        }

        public static bool operator ==(Keyword a, Keyword b)
        {
            if ((object) a == null || (object) b == null)
            {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(Keyword a, Keyword b)
        {
            return !a.Equals(b);
        }

        public static implicit operator string(Keyword keyword)
        {
            return keyword.Value;
        }
    }
}
