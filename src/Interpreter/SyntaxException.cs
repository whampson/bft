﻿#region License
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

namespace WHampson.Cascara.Interpreter
{
    /// <summary>
    /// The exception that is thrown when a syntactical error is found while
    /// interpreting the source code of a <see cref="LayoutScript"/>.
    /// </summary>
    public class SyntaxException : LayoutScriptException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxException"/> class.
        /// </summary>
        internal SyntaxException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxException"/> class.
        /// </summary>
        /// <param name="message">A message that describes the error.</param>
        internal SyntaxException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyntaxException"/> class.
        /// </summary>
        /// <param name="message">
        /// A message that describes the error.
        /// </param>
        /// <param name="innerException">
        /// The <see cref="Exception"/> that caused this exception.
        /// </param>
        internal SyntaxException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
