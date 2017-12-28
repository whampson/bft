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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WHampson.Cascara
{
    internal abstract class CascaraElement
    {
        public static CascaraElement CreateDirective(string name, params CascaraModifier[] modifiers)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            return new DirectiveElement(name, modifiers);
        }

        public static CascaraElement CreateDataType(string name, CascaraType type, params CascaraModifier[] modifiers)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            return new DataTypeElement(name, type, modifiers);
        }

        protected CascaraElement(string name, params CascaraModifier[] modifiers)
        {
            Name = name;
            Modifiers = new List<CascaraModifier>(modifiers);
        }

        public string Name
        {
            get;
        }

        public IEnumerable<CascaraModifier> Modifiers
        {
            get;
        }
    }

    internal sealed class DataTypeElement : CascaraElement
    {
        public DataTypeElement(string name, CascaraType type, params CascaraModifier[] modifiers)
            : base(name, modifiers)
        {
            Type = type;
        }

        public CascaraType Type
        {
            get;
        }

        public override string ToString()
        {
            return string.Format("DataTypeElement: [ Name: {0}, ModifierCount: {1}, Type: {2} ]",
                Name, Modifiers.Count(), Type);
        }
    }

    internal sealed class DirectiveElement : CascaraElement
    {
        public DirectiveElement(string name, params CascaraModifier[] modifiers)
            : base(name, modifiers)
        {
        }

        public override string ToString()
        {
            return string.Format("DirectiveElement: [ Name: {0}, ModifierCount: {1} ]",
                Name, Modifiers.Count());
        }
    }
}
