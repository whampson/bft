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
        public SymbolTableEntry(TypeInstance dataType)
            : this(dataType, (SymbolTable[]) null)
        {
        }

        public SymbolTableEntry(TypeInstance dataType, SymbolTable child)
        {
            if (dataType == null)
            {
                throw new ArgumentNullException(nameof(dataType));
            }

            if (dataType.IsArray)
            {
                string msg = "Cannot use non-array SymbolTableEntry constructor for " +
                    "an array type.";
                throw new ArgumentException(msg, nameof(dataType));
            }

            DataType = dataType;
            ChildSymbols = new SymbolTable[] { child };
            ChildTypes = new TypeInstance[1];
        }

        /// <summary>
        /// Creates a new <see cref="SymbolTableEntry"/> object containing the
        /// given symbol, type, offset, and child table.
        /// </summary>
        /// <param name="dataType">
        /// The type of data that the symbol refers to.
        /// </param>
        /// <param name="children">
        /// A <see cref="SymbolTable"/> array of symbol tables that stem from this entry.
        /// </param>
        public SymbolTableEntry(TypeInstance dataType, SymbolTable[] children)
        {
            if (dataType == null)
            {
                throw new ArgumentNullException(nameof(dataType));
            }

            if (children == null)
            {
                children = new SymbolTable[dataType.Count];
            }

            if (children.Length != dataType.Count)
            {
                string msg = "Child symbol table array must have a length equal to " +
                    "the number of elements in the type instance.";
                throw new ArgumentException(msg, nameof(children));
            }

            DataType = dataType;
            ChildSymbols = new SymbolTable[dataType.Count];
            ChildTypes = new TypeInstance[dataType.Count];
            Array.Copy(children, ChildSymbols, dataType.Count);
        }

        /// <summary>
        /// Gets the type information associated with the symbol.
        /// </summary>
        public TypeInstance DataType
        {
            get;
        }

        public TypeInstance[] ChildTypes
        {
            get;
        }

        /// <summary>
        /// Gets the <see cref="SymbolTable"/> object containing all child symbols.
        /// </summary>
        public SymbolTable[] ChildSymbols
        {
            get;
        }

        public bool HasChildTypes
        {
            get
            {
                foreach (TypeInstance child in ChildTypes)
                {
                    if (child != null)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool HasChildSymbols
        {
            get
            {
                foreach (SymbolTable child in ChildSymbols)
                {
                    if (child != null)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public override string ToString()
        {
            return string.Format("[DataType: {0}, HasChildSymbols: {1}, HasChildTypes: {2}]",
                DataType, HasChildSymbols, HasChildTypes);
        }
    }
}
