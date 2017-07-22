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
    internal sealed class SymbolTable
    {
        private Dictionary<string, SymbolTableEntry> entries;

        public SymbolTable()
            : this(null, null)
        {
        }

        public SymbolTable(string name, SymbolTable parent)
        {
            Name = name;
            Parent = parent;
            entries = new Dictionary<string, SymbolTableEntry>();
        }

        public string Name
        {
            get;
        }

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

        public SymbolTable Parent
        {
            get;
        }

        public bool AddEntry(string name, SymbolTableEntry e)
        {
            string symbol = CreateSymbol(name);
            if (entries.ContainsKey(symbol))
            {
                return false;
            }

            entries.Add(symbol, e);
            return true;
        }

        public SymbolTableEntry GetEntry(string name)
        {
            string[] splitname = name.Split('.');

            SymbolTableEntry top = SearchUp(this, CreateSymbol(splitname[0]));
            if (top == null)
            {
                return null;
            }

            SymbolTableEntry result = top;
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
            foreach (KeyValuePair<string, SymbolTableEntry> kvp in entries)
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

        public static string CreateSymbol(string name)
        {
            Regex arrayNotation = new Regex(@"^.*\[\d+\]$");

            if (!arrayNotation.IsMatch(name))
            {
                name = name + "[0]";
            }

            return name;
        }

        private static SymbolTableEntry SearchUp(SymbolTable tabl, string symbolName)
        {
            if (tabl == null)
            {
                return null;
            }

            if (tabl.entries.TryGetValue(symbolName, out SymbolTableEntry entry))
            {
                return entry;
            }

            return SearchUp(tabl.Parent, symbolName);
        }
    }
}
