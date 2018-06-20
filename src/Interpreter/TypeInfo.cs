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
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WHampson.Cascara.Interpreter
{
    /// <summary>
    /// Maintains all of the necessary information needed to instantiate
    /// a user-defined type or structure.
    /// </summary>
    internal class TypeInfo
    {
        public static TypeInfo CreatePrimitive(int size, Type nativeType)
        {
            return new TypeInfo(size, nativeType);
        }

        public static TypeInfo CreateStruct(int size, params Statement[] members)
        {
            return new TypeInfo(size, members);
        }

        private TypeInfo(int size, Type nativeType)
        {
            Size = size;
            NativeType = nativeType;
            Members = new List<Statement>();
        }

        private TypeInfo(int size, params Statement[] members)
        {
            Size = size;
            NativeType = null;
            Members = new List<Statement>(members);
        }

        public int Size
        {
            get;
        }

        public Type NativeType
        {
            get;
        }

        public IEnumerable<Statement> Members
        {
            get;
        }

        public bool IsStruct
        {
            get { return NativeType == null && Members.Any(); }
        }

        public override string ToString()
        {
            JObject o = new JObject();
            o.Add(nameof(Size), Size);
            o.Add(nameof(NativeType), NativeType.FullName);
            o.Add(nameof(IsStruct), IsStruct);
            o.Add(nameof(Members), JToken.FromObject(Members));
            return o.ToString(Formatting.None);
        }
    }
}
