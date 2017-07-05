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

namespace WHampson.BFT.Types
{
    /// <summary>
    /// Represents an 8-bit signed integer.
    /// The .NET analog for this type is <see cref="sbyte"/>.
    /// </summary>
    public unsafe struct Int8 : IPrimitiveType
    {
        /// <summary>
        /// Represents the largest (most positive) possible value of
        /// <see cref="Int8"/>.
        /// </summary>
        public static readonly Int8 MaxValue = sbyte.MaxValue;

        /// <summary>
        /// Represents the smallest (most negative) possible value of
        /// <see cref="Int8"/>.
        /// </summary>
        public static readonly Int8 MinValue = sbyte.MinValue;

        /// <summary>
        /// Handles implicit conversion from <see cref="Int8"/>
        /// to <see cref="sbyte"/>.
        /// </summary>
        /// <param name="i8">
        /// The <see cref="Int8"/> to convert to <see cref="sbyte"/>.
        /// </param>
        public static implicit operator sbyte(Int8 i8)
        {
            return (sbyte) i8.data[0];
        }

        /// <summary>
        /// Handles implicit conversion from <see cref="sbyte"/>
        /// to <see cref="Int8"/>.
        /// </summary>
        /// <param name="sb">
        /// The <see cref="sbyte"/> to convert to <see cref="Int8"/>.
        /// </param>
        public static implicit operator Int8(sbyte sb)
        {
            Int8 i8 = new Int8();
            i8.data[0] = (byte) sb;

            return i8;
        }

        // Backing data for Int8.
        private fixed byte data[1];

        byte[] IPrimitiveType.GetBytes()
        {
            byte[] b = new byte[1];
            fixed (byte* pData = data)
            {
                b[0] = *pData;
            }

            return b;
        }

        public override string ToString()
        {
            // Use the native type's ToString().
            return ((sbyte) this).ToString();
        }
    }
}
