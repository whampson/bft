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
using System.Collections;

namespace WHampson.Cascara.Types
{
    public class ArrayPointer<T> : Pointer<T>, IEnumerable
        where T : struct, ICascaraType
    {
        public ArrayPointer(IntPtr addr, int count)
            : base(addr)
        {
            Count = count;
        }

        private int Count
        {
            get;
        }

        public IEnumerator GetEnumerator()
        {
            return new ArrayPointerEnumerator<T>(this);
        }

        private class ArrayPointerEnumerator<U> : IEnumerator
            where U : struct, ICascaraType
        {
            private ArrayPointer<U> arr;
            private int position;

            public ArrayPointerEnumerator(ArrayPointer<U> arr)
            {
                this.arr = arr;
                Reset();
            }

            public object Current
            {
                get { return arr[position]; }
            }

            public bool MoveNext()
            {
                position++;

                return position < arr.Count;
            }

            public void Reset()
            {
                position = -1;
            }
        }
    }
}
