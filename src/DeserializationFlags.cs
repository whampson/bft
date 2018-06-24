#region License
/* Copyright (c) 2017-2018 Wes Hampson
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
    /// Flags that control the behavior of binary data deserialization.
    /// </summary>
    [Flags]
    public enum DeserializationFlags
    {
        /// <summary>
        /// Ignore the case of identifier names when deserializing an object.
        /// </summary>
        IgnoreCase = 1,

        /// <summary>
        /// Deserialize binary data into fields of an object with matching
        /// names.
        /// </summary>
        Fields = 2,

        /// <summary>
        /// Deserialize binary data into properties of an object with matching
        /// names.
        /// </summary>
        Properties = 4,

        /// <summary>
        /// Deserialize binary data into public members of an object.
        /// </summary>
        Public = 8,

        /// <summary>
        /// Deserialize binary data into private, protected, or internal
        /// members of an object.
        /// </summary>
        NonPublic = 16
    }
}
