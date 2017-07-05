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

        // Backing data for Int32.
        private fixed byte data[4];

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
}
