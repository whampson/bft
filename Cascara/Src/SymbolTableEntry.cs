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

namespace WHampson.Cascara
{
    /// <summary>
    /// Represents an entry in a <see cref="SymbolTable"/>. It contains
    /// a copy of the symbol name, the type information, location of the
    /// data that the symbol refers to, and a child table.
    /// </summary>
    internal sealed class SymbolTableEntry
    {
        /// <summary>
        /// Creates a new <see cref="SymbolTableEntry"/> object containing the
        /// given symbol, type, offset, and child table.
        /// </summary>
        /// <param name="tInfo">
        /// The type of data that the symbol refers to.
        /// </param>
        /// <param name="child">
        /// A <see cref="SymbolTable"/> that stems from this entry.
        /// </param>
        public SymbolTableEntry(TypeInfo tInfo, SymbolTable child)
        {
            TypeInfo = tInfo;
            Count = (child != null) ? 1 : 0;
            Children = new SymbolTable[Count];
            if (Count != 0)
            {
                Children[0] = child;
            }
        }

        /// <summary>
        /// Creates a new <see cref="SymbolTableEntry"/> object containing the
        /// given symbol, type, offset, and child table.
        /// </summary>
        /// <param name="tInfo">
        /// The type of data that the symbol refers to.
        /// </param>
        /// <param name="count">
        /// The number of consecutive elements that this symbol refers to.
        /// </param>
        /// <param name="children">
        /// A <see cref="SymbolTable"/> array of symbol tables that stem from this entry.
        /// </param>
        public SymbolTableEntry(TypeInfo tInfo, int count, SymbolTable[] children)
        {
            if (children.Length != count)
            {
                throw new ArgumentException("Child symbol table array must have a length equal to 'count'.", "children");
            }

            TypeInfo = tInfo;
            Count = count;
            Children = new SymbolTable[count];
            Array.Copy(children, Children, count);
        }

        /// <summary>
        /// Gets or sets the type information associated with the symbol.
        /// </summary>
        public TypeInfo TypeInfo
        {
            get;
        }

        /// <summary>
        /// Gets the number of consecutive elements associated with the symbol.
        /// </summary>
        public int Count
        {
            get;
        }

        /// <summary>
        /// Gets the <see cref="SymbolTable"/> object containing all child symbols.
        /// </summary>
        public SymbolTable[] Children
        {
            get;
        }

        public SymbolTable FirstChild
        {
            get { return (HasChildren) ? Children[0] : null; }
        }

        /// <summary>
        /// Gets a value indicating whether this symbol has a child table.
        /// </summary>
        /// <remarks>
        /// This indicates whether the symbol refers to a struct.
        /// </remarks>
        public bool HasChildren
        {
            get { return Children.Length != 0; }
        }

        public override string ToString()
        {
            return string.Format("[Type: {0}, Count{1}, HasChildren: {2}]", TypeInfo, Count, HasChildren);
        }
    }
}
