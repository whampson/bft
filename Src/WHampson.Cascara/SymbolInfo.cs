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

namespace WHampson.Cascara
{
    /// <summary>
    /// Contains type, location, and child information about a variable
    /// identifier.
    /// </summary>
    internal sealed class SymbolInfo
    {
        private TypeInfo type;
        private bool isTypeSet;

        /// <summary>
        /// Creates a new <see cref="SymbolInfo"/> object with the given type
        /// information, location in the binary data, and child symbol
        /// information.
        /// </summary>
        /// <param name="typeInfo">
        /// Information about the data the symbol represents.
        /// </param>
        /// <param name="offset">
        /// The position in the binary data of the first byte of this type's value.
        /// </param>
        /// <param name="child">
        /// </param>
        public SymbolInfo(TypeInfo typeInfo, int offset, SymbolTable child)
        {
            TypeInfo = typeInfo;
            Offset = offset;
            Child = child;
        }

        /// <summary>
        /// Gets or sets the type information.
        /// </summary>
        /// <remarks>
        /// NOTE: the setter can only be used ONCE!
        /// This is to allow for entries to be added to symbol tables
        /// before their types are analyzed. This allows the symbol's offset
        /// to be referenced while it's children are being populated.
        /// This type should otherwise be immutable.
        /// </remarks>
        public TypeInfo TypeInfo
        {
            get
            {
                return type;
            }

            set
            {
                // Ensure type is set exactly once
                if (!isTypeSet)
                {
                    type = value;
                    if (value != null)
                    {
                        isTypeSet = true;
                    }
                }
                else
                {
                    string msg = "Cannot set Type as it has already been set.";
                    throw new InvalidOperationException(msg);
                }
            }
        }

        /// <summary>
        /// Gets the position in the binary data of the first byte of this type's value.
        /// </summary>
        public int Offset
        {
            get;
        }

        /// <summary>
        /// Gets the <see cref="SymbolTable"/> containing all child symbols.
        /// </summary>
        public SymbolTable Child
        {
            get;
        }

        public override string ToString()
        {
            string s = "{";
            s += string.Format("{0}: {1:X8}", TypeInfo.Type.Name, Offset);
            s += "}";

            return s;
        }
    }
}