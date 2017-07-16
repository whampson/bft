﻿#region License
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
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace WHampson.Bft
{
    internal sealed class TypeInfo
    {
        public static TypeInfo CreatePrimitive(Type t)
        {
            return new TypeInfo(t, new List<XElement>(), Marshal.SizeOf(t));
        }

        public static TypeInfo CreateStruct(IEnumerable<XElement> members, int size) 
        {
            if (members == null)
            {
                throw new ArgumentNullException("members");
            }

            if (size < 0)
            {
                throw new ArgumentException("Size must be a non-negative integer.");
            }

            return new TypeInfo(typeof(BftStruct), members, size);
        }

        private TypeInfo(Type t, IEnumerable<XElement> members, int size)
        {
            Type = t;
            Members = new List<XElement>(members);
            Size = size;
        }

        public Type Type
        {
            get;
        }

        public IEnumerable<XElement> Members
        {
            get;
        }

        public int Size
        {
            get;
        }
    }
}
