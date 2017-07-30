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

namespace WHampson.Cascara.Types
{
    /// <summary>
    /// Provides a framework for creating a pointer to an <see cref="ICascaraType"/>.
    /// </summary>
    public interface ICascaraPointer : IConvertible
    {
        /// <summary>
        /// Gets the absolute memory address pointed to.
        /// </summary>
        IntPtr Address
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether this pointer is zero.
        /// </summary>
        /// <remarks>
        /// A pointer that is not <code>null</code> does not necessarily
        /// mean that is usable.
        /// </remarks>
        /// <returns>
        /// <code>True</code> if the pointer points to zero,
        /// <code>False</code> othwewise.
        /// </returns>
        bool IsNull();

        T GetValue<T>() where T : struct;

        void SetValue<T>(T value) where T : struct;
    }
}
