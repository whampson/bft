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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using WHampson.Cascara.Extensions;

namespace WHampson.Cascara.Interpreter.Xml
{
    /// <summary>
    /// Encapsulates an <see cref="XObject"/> as an <see cref="ISourceEntity"/>.
    /// </summary>
    internal sealed class XmlSourceEntity : ISourceEntity
    {
        public XmlSourceEntity(XObject sourceObject)
        {
            if (sourceObject == null)
            {
                throw new ArgumentNullException(nameof(sourceObject));
            }

            SourceObject = sourceObject;
        }

        public XObject SourceObject
        {
            get;
        }

        int ISourceEntity.LineNumber
        {
            get { return SourceObject.GetLineInfo().Item1; }
        }

        int ISourceEntity.LinePosition
        {
            get { return SourceObject.GetLineInfo().Item2; }
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
