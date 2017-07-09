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

namespace WHampson.BFT.Types
{
    /// <summary>
    /// Represents a 32-bit (single-precision) floating-point number.
    /// The .NET analog for this type is <see cref="float"/>. 
    /// </summary>
    public unsafe struct Float : IPrimitiveType
    {
        /// <summary>
        /// Represents the largest (most positive) possible value of
        /// <see cref="Float"/>.
        /// </summary>
        public static readonly Float MaxValue = float.MaxValue;

        /// <summary>
        /// Represents the smallest (most negative) possible value of
        /// <see cref="Float"/>.
        /// </summary>
        public static readonly Float MinValue = float.MinValue;

        /// <summary>
        /// Represents positive infinity.
        /// </summary>
        public static readonly Float PositiveInfinity = float.PositiveInfinity;

        /// <summary>
        /// Represents negative infinity.
        /// </summary>
        public static readonly Float NegativeInfinity = float.NegativeInfinity;

        /// <summary>
        /// Represents not a number (NaN).
        /// </summary>
        public static readonly Float NaN = float.NaN;

        /// <summary>
        /// Represents the smallest positive <see cref="Float"/> value.
        /// </summary>
        public static readonly Float Epsilon = float.Epsilon;

        /// <summary>
        /// Handles implicit conversion from <see cref="Float"/>
        /// to <see cref="float"/>.
        /// </summary>
        /// <param name="f">
        /// The <see cref="Float"/> to convert to <see cref="float"/>.
        /// </param>
        public static implicit operator float(Float f)
        {
            int len = sizeof(Float);
            byte[] b = new byte[len];

            // Copy backing data to temp array
            for (int i = 0; i < len; i++)
            {
                b[i] = f.data[i];
            }

            // Convert bytes to single-precision float
            return BitConverter.ToSingle(b, 0);
        }

        /// <summary>
        /// Handles implicit conversion from <see cref="float"/>
        /// to <see cref="Float"/>.
        /// </summary>
        /// <param name="f">
        /// The <see cref="float"/> to convert to <see cref="Float"/>.
        /// </param>
        public static implicit operator Float(float f)
        {
            // Get single-precision float bytes
            byte[] b = BitConverter.GetBytes(f);

            // Copy float bytes to backing data array
            Float fl = new Float();
            for (int i = 0; i < b.Length; i++)
            {
                fl.data[i] = b[i];
            }

            return fl;
        }

        public static bool TryParse(string valStr, out Float val)
        {
            float v;
            bool success;
            if (success = float.TryParse(valStr, out v))
            {
                val = v;
            }

            return success;
        }

        // Backing data for Float.
        private fixed byte data[4];

        bool IPrimitiveType.Equals(IPrimitiveType o)
        {
            if (!(o is Float))
            {
                return false;
            }

            Float other = (Float) o;
            return this == other;
        }

        public byte[] GetBytes()
        {
            int siz = sizeof(Float);
            byte[] b = new byte[siz];

            fixed (byte *pData = data)
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
            return ((float) this).ToString();
        }
    }
}
