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
using System.Text.RegularExpressions;

namespace WHampson.Cascara
{
    /// <summary>
    /// Associates identifiers with a type, offset and child
    /// <see cref="SymbolTable"/> for nested identifiers.
    /// </summary>
    internal sealed class SymbolTable
    {
        private Dictionary<string, SymbolInfo> entries;

        /// <summary>
        /// Creates a new nameless, parentless <see cref="SymbolTable"/>.
        /// Use this for creating the root table.
        /// </summary>
        public SymbolTable()
            : this(null, null)
        {
        }

        /// <summary>
        /// Creates a new named <see cref="SymbolTable"/> that is the
        /// child of an existing table.
        /// </summary>
        /// <param name="name">
        /// The name associated with the symbol table.
        /// </param>
        /// <param name="parent">
        /// The <see cref="SymbolTable"/> from which this table stems.
        /// </param>
        public SymbolTable(string name, SymbolTable parent)
        {
            Name = name;
            Parent = parent;
            entries = new Dictionary<string, SymbolInfo>();
        }

        /// <summary>
        /// Gets locally scoped name of this table.
        /// </summary>
        public string Name
        {
            get;
        }

        /// <summary>
        /// Gets the globally scoped name of this variable.
        /// </summary>
        public string FullyQualifiedName
        {
            get
            {
                string baseName = "";
                if (!(Parent == null || Parent.Name == null))
                {
                    baseName += CreateSymbol(Parent.Name) + ".";
                }

                if (Name != null)
                {
                    baseName += CreateSymbol(Name);
                }

                return baseName;
            }
        }

        /// <summary>
        /// Gets the parent table.
        /// Returns <code>null</code> if this is the root table.
        /// </summary>
        public SymbolTable Parent
        {
            get;
        }

        /// <summary>
        /// Adds the provided <see cref="SymbolInfo"/> to the table and
        /// associates it with the provided name.
        /// </summary>
        /// <param name="name">
        /// The name to be given to the table entry.
        /// </param>
        /// <param name="e">
        /// The <see cref="SymbolInfo"/> to add to the table.
        /// </param>
        /// <returns>
        /// <code>False</code> if the given name already exists in the table.
        /// </returns>
        public bool AddEntry(string name, SymbolInfo e)
        {
            string symbol = CreateSymbol(name);
            if (entries.ContainsKey(symbol))
            {
                return false;
            }

            entries.Add(symbol, e);

            return true;
        }

        /// <summary>
        /// Gets an entry from the table 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public SymbolInfo GetEntry(string name)
        {
            string[] splitname = name.Split('.');

            SymbolInfo top = SearchUp(this, CreateSymbol(splitname[0]));
            if (top == null)
            {
                return null;
            }

            SymbolInfo result = top;
            SymbolTable tabl = result.Child;
            string sym;

            for (int i = 1; i < splitname.Length; i++)
            {
                sym = CreateSymbol(splitname[i]);
                if (tabl == null || !tabl.entries.TryGetValue(sym, out result))
                {
                    return null;
                }
                tabl = result.Child;
            }

            return result;
        }

        public override string ToString()
        {
            string baseName = FullyQualifiedName;
            string s = "";
            foreach (KeyValuePair<string, SymbolInfo> kvp in entries)
            {
                s += baseName;
                s += (baseName == "") ? "" : ".";
                s += string.Format("{0}: {1}\n", kvp.Key, kvp.Value);

                if (kvp.Value.Child != null)
                {
                    s += kvp.Value.Child;
                }

            }
            return s;
        }

        /// <summary>
        /// Creates a unique symbol for the given identifier by appending
        /// an array index to the end (if not already present).
        /// </summary>
        /// <param name="name">
        /// The identifier to turn into a symbol.
        /// </param>
        /// <returns>
        /// The newly-created symbol.
        /// </returns>
        private static string CreateSymbol(string name)
        {
            const string ArrayNotationPattern = @"^.*\[\d+\]$";

            if (!Regex.IsMatch(name, ArrayNotationPattern))
            {
                name = name + "[0]";
            }

            return name;
        }

        /// <summary>
        /// Searches the parent tables for the given symbol.
        /// </summary>
        /// <param name="tabl">
        /// The table to begin the search from.
        /// </param>
        /// <param name="symbolName">
        /// The symbol to search for.
        /// </param>
        /// <returns>
        /// The <see cref="SymbolInfo"/> corresponding to the symbol if found,
        /// <code>null</code> if not found.
        /// </returns>
        private static SymbolInfo SearchUp(SymbolTable tabl, string symbolName)
        {
            if (tabl == null)
            {
                return null;
            }

            if (tabl.entries.TryGetValue(symbolName, out SymbolInfo entry))
            {
                return entry;
            }

            return SearchUp(tabl.Parent, symbolName);
        }
    }
}
