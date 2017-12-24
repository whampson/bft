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
using System.Text;

namespace WHampson.Cascara
{
    internal sealed class CascaraModifier
    {
        public static CascaraModifier Create(string name, bool canContainVariables)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            return new CascaraModifier(name, false, canContainVariables);
        }

        public static CascaraModifier CreateRequired(string name, bool canContainVariables)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            return new CascaraModifier(name, true, canContainVariables);
        }

        private CascaraModifier(string name, bool isRequired, bool canContainVariables)
        {
            Name = name;
            IsRequired = isRequired;
            CanContainVariables = canContainVariables;
        }

        public string Name
        {
            get;
        }

        public bool IsRequired
        {
            get;
        }

        public bool CanContainVariables
        {
            get;
        }

        public override string ToString()
        {
            return string.Format("CascaraModifier: [ Name: {0}, IsRequired: {1}, CanContainVariables: {2} ]",
                Name, IsRequired, CanContainVariables);
        }
    }
}
