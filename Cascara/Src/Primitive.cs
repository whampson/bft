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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using WHampson.Cascara.Interpreter;

namespace WHampson.Cascara
{
    public class Primitive<T> : IFileObject
        where T : struct
    {
        private BinaryFile sourceFile;
        private Symbol symbol;

        internal Primitive(BinaryFile sourceFile, Symbol symbol)
        {
            if (!PrimitiveTypeUtils.IsPrimitiveType<T>())
            {
                string msg = Resources.ArgumentExceptionPrimitiveType;
                throw new ArgumentException(msg, nameof(T));
            }
            if (PrimitiveTypeUtils.SizeOf<T>() > symbol.DataLength)
            {
                string msg = "The type provided cannot be larger than the size of the data field.";
                throw new ArgumentException(msg, nameof(T));
            }

            this.sourceFile = sourceFile;
            this.symbol = symbol;
        }

        public Primitive<T> this[int index]
        {
            get { return (Primitive<T>) ((IFileObject) this).ElementAt(index); }
        }

        public T Value
        {
            get { return sourceFile.Get<T>(FilePosition); }
            set { sourceFile.Set<T>(FilePosition, value); }
        }

        public int FilePosition
        {
            get { return symbol.DataOffset; }
        }

        public int Offset
        {
            get
            {
                if (symbol.Parent != null)
                {
                    return symbol.DataOffset - symbol.Parent.DataOffset;
                }

                return symbol.DataOffset;
            }
        }

        public int Length
        {
            get { return symbol.DataLength; }
        }

        public bool IsCollection
        {
            get { return symbol.IsCollection; }
        }

        public int ElementCount
        {
            get { return symbol.ElementCount; }
        }

        IFileObject IFileObject.ElementAt(int index)
        {
            if (!IsCollection)
            {
                string msg = "Object instance is not a collection.";
                throw new InvalidOperationException(msg);
            }

            if (index < 0 || index >= ElementCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return new Primitive<T>(sourceFile, symbol[index]);
        }

        public IEnumerator<IFileObject> GetEnumerator()
        {
            if (!IsCollection)
            {
                yield break;
            }

            for (int i = 0; i < ElementCount; i++)
            {
                yield return ((IFileObject) this).ElementAt(i);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static implicit operator int(Primitive<T> p)
        {
            return p.FilePosition;
        }
    }
}
