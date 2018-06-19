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
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace WHampson.Cascara.Extensions
{
    /// <summary>
    /// Extension methods for the <see cref="System.Xml.Linq.XObject"/> class.
    /// </summary>
    internal static class XObjectExtensions
    {
        /// <summary>
        /// Extracts source code line information from the specified <see cref="XElement"/>.
        /// </summary>
        /// <param name="node">The node to extract line info from.</param>
        /// <returns>
        /// An integer pair representing the text coordinates of the statement in the source code.
        /// 'Item1' is the line number; 'Item2' is the column number.
        /// </returns>
        public static Tuple<int, int> GetLineInfo(this XObject node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            IXmlLineInfo lineInfo = (IXmlLineInfo) node;
            return new Tuple<int, int>(lineInfo.LineNumber, lineInfo.LinePosition);
        }
    }
}
