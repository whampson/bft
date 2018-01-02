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

using System.Collections.Generic;
using WHampson.Cascara.Extensions;

namespace WHampson.Cascara.Interpreter
{
    internal static partial class ReservedWords
    {
        /// <summary>
        /// Defines all valid XML element attributes.
        /// </summary>
        /// <remarks>
        /// Not all parameters are valid on all elements.
        /// </remarks>
        public static class Parameters
        {
            public const string Comment = "comment";
            public const string Count = "count";
            public const string Kind = "kind";
            public const string Message = "message";
            public const string Name = "name";
            public const string Path = "path";
            public const string Value = "value";
            public const string Version = "version";

            public static readonly HashSet<string> AllParameters = new HashSet<string>(
                typeof(Parameters).GetPublicConstants<string>());
        }
    }
}
