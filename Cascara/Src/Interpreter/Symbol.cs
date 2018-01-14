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
using System.Linq;
using System.Text.RegularExpressions;

namespace WHampson.Cascara.Interpreter
{
    /// <summary>
    /// Represents an identifier given to some type of data.
    /// </summary>
    /// <remarks>
    /// This class also encapsulates the functionality of a symbol table.
    /// </remarks>
    internal class Symbol : IEnumerable<Symbol>
    {
        /// <summary>
        /// Indicates the member of a structure.
        /// Example: 'x.y' means "member y of structure x".
        /// </summary>
        private const char StructureReferenceOperatorChar = '.';

        /// <summary>
        /// Regex pattern for matching symbol names that refer to collection elements.
        /// </summary>
        private const string CollectionNotationPattern = @"\[(\d+)\]$";

        /// <summary>
        /// The child <see cref="Symbol"/> table.
        /// </summary>
        private Dictionary<string, Symbol> symbolTable;

        /// <summary>
        /// Child <see cref="Symbol"/>s for referencing elements of a collection.
        /// </summary>
        private List<Symbol> collectionSymbols;

        /// <summary>
        /// Creates a nameless, parentless <see cref="Symbol"/> for use
        /// as the root in a <see cref="Symbol"/> tree.
        /// </summary>
        /// <returns>
        /// The newly-created <see cref="Symbol"/> object.
        /// </returns>
        internal static Symbol CreateRootSymbol()
        {
            return new Symbol(null, null);
        }

        /// <summary>
        /// Creates a nameless <see cref="Symbol"/> with the specified parent <see cref="Symbol"/>.
        /// An entry for this <see cref="Symbol"/> does not exist in the parent.
        /// </summary>
        /// <param name="parent">
        /// The <see cref="Symbol"/> that this <see cref="Symbol"/> stems from.
        /// </param>
        /// <returns>
        /// The newly-created <see cref="Symbol"/> object.
        /// </returns>
        internal static Symbol CreateNamelessSymbol(Symbol parent)
        {
            return new Symbol(null, parent);
        }

        /// <summary>
        /// Checks whether a given string satisfies the requirements
        /// for a <see cref="Symbol"/> name.
        /// </summary>
        /// <remarks>
        /// <see cref="Symbol"/> names must consist of only alphabetic characters
        /// (any case), decimal digits, or underscores. <see cref="Symbol"/> names
        /// cannot begin with a decimal digit.
        /// </remarks>
        /// <param name="name">
        /// The string to validate.
        /// </param>
        /// <returns>
        /// <code>True</code> if the specified string meets the criteria for a
        /// valid <see cref="Symbol"/> name, <code>False</code> otherwise.
        /// </returns>
        internal static bool IsNameValid(string name)
        {
            // Don't allow names to start with a digit.
            // This is because we use numbers as the names of collection element
            // symbol tables.
            if (string.IsNullOrWhiteSpace(name) || char.IsDigit(name[0]))
            {
                return false;
            }

            // Don't allow identifiers that match reserved words
            if (ReservedWords.AllReservedWords.Contains(name))
            {
                return false;
            }

            foreach (char c in name)
            {
                // Only allow alphanumeric characters and underscores.
                if (!char.IsLetterOrDigit(c) && c != '_')
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Searches up the <see cref="Symbol"/> tree from the given <see cref="Symbol"/>
        /// to the root for all occurrences of the specified <see cref="Symbol"/> name.
        /// </summary>
        /// <remarks>
        /// This is a wrapper function for <see cref="SearchUp(string, Symbol, ref List{Symbol})"/>.
        /// </remarks>
        /// <param name="name">
        /// The name of the <see cref="Symbol"/> to search for.
        /// </param>
        /// <param name="sym">
        /// The node in the <see cref="Symbol"/> tree from which to begin the search.
        /// </param>
        /// <returns>
        /// A <see cref="List{Symbol}"/> of all <see cref="Symbol"/>s with a matching name.
        /// </returns>
        private static List<Symbol> SearchUp(string name, Symbol sym)
        {
            List<Symbol> results = new List<Symbol>();
            SearchUp(name, sym, ref results);

            return results;
        }

        /// <summary>
        /// Searches up the <see cref="Symbol"/> tree from the given <see cref="Symbol"/>
        /// to the root for all occurrences of the specified <see cref="Symbol"/> name.
        /// </summary>
        /// <remarks>
        /// If <see cref="Symbol"/>s are found, they are said to be within the scope of
        /// <paramref name="sym"/>.
        /// </remarks>
        /// <param name="name">
        /// The name of the <see cref="Symbol"/> to search for.
        /// </param>
        /// <param name="sym">
        /// The node in the <see cref="Symbol"/> tree from which to begin the search.
        /// </param>
        /// <param name="results">
        /// A <see cref="List{Symbol}"/> reference which is added to in each recursive
        /// call of this function.
        /// </param>
        private static void SearchUp(string name, Symbol sym, ref List<Symbol> results)
        {
            if (sym == null)
            {
                return;
            }

            string tempName = name;

            // Read collection index and remove from symbol name (if applicable)
            bool isSearchingForCollection = TryGetElementIndex(tempName, out int index);
            if (isSearchingForCollection)
            {
                tempName = StripCollectionNotation(tempName);
            }

            // Add entry to list if it exists in the current table
            if (sym.symbolTable.TryGetValue(tempName, out Symbol entry))
            {
                if (isSearchingForCollection)
                {
                    if (entry.IsCollection && index < entry.ElementCount)
                    {
                        results.Add(entry.collectionSymbols[index]);
                    }
                }
                else
                {
                    results.Add(entry);
                }
            }

            // Search the parent table
            SearchUp(name, sym.Parent, ref results);
        }

        /// <summary>
        /// Searches down the <see cref="Symbol"/> tree from the given root
        /// for the given filly qualified symbol name.
        /// </summary>
        /// <param name="name">
        /// The fully-qualified name of the <see cref="Symbol"/> search for.
        /// </param>
        /// <param name="sym">
        /// The root of the <see cref="Symbol"/> tree to begin the search.
        /// </param>
        /// <returns>
        /// The <see cref="Symbol"/> with the matching name, if found.
        /// <code>Null</code> otherwise.
        /// </returns>
        private static Symbol SearchDown(string name, Symbol sym)
        {
            if (sym == null)
            {
                return null;
            }

            string[] splitName = name.Split(new char[] { StructureReferenceOperatorChar }, 2);

            // Read collection index and remove from symbol name (if applicable)
            bool isSearchingForCollection = TryGetElementIndex(splitName[0], out int index);
            if (isSearchingForCollection)
            {
                splitName[0] = StripCollectionNotation(splitName[0]);
            }

            // Look for top-level name in current symbol table
            if (!sym.symbolTable.TryGetValue(splitName[0], out Symbol entry))
            {
                return null;
            }

            if (isSearchingForCollection)
            {
                if (!entry.IsCollection || index >= entry.ElementCount)
                {
                    return null;
                }

                // Get collection element symbol table
                entry = entry.collectionSymbols[index];
            }

            // If only a top-level name was specified (no dot), return the entry (base case)
            if (splitName.Length == 1)
            {
                return entry;
            }

            // Continue searching down the tree for the rest of the name
            return SearchDown(splitName[1], entry);
        }

        /// <summary>
        /// Gets the collection element index from the specified symbol name.
        /// A return value indicates whether the extraction succeeded.
        /// </summary>
        /// <remarks>
        /// If the name passed in does not refer to a collection element,
        /// <code>False</code> will be returned.
        /// </remarks>
        /// <param name="name">
        /// The name of the symbol to extract the element index from.
        /// </param>
        /// <param name="index">
        /// The variable to use to store the index, if extracted.
        /// If the extraction did not succeed, this value will be set to
        /// <code>default(int)</code>.
        /// </param>
        /// <returns>
        /// <code>True</code> if the element index extraction succeeded,
        /// <code>False</code> otherwise.
        /// </returns>
        private static bool TryGetElementIndex(string name, out int index)
        {
            Match m = Regex.Match(name, CollectionNotationPattern);

            return int.TryParse(m.Groups[1].Value, out index);
        }

        /// <summary>
        /// Removes the "element brackets" from the given <see cref="Symbol"/> name.
        /// </summary>
        /// <remarks>
        /// If the name passed in does not refer to a collection element,
        /// <paramref name="name"/> will be returned.
        /// </remarks>
        /// <param name="name">
        /// The symbol to strip the collection notation from.
        /// </param>
        /// <returns>
        /// The modified <see cref="Symbol"/> name.
        /// </returns>
        private static string StripCollectionNotation(string name)
        {
            return Regex.Replace(name, CollectionNotationPattern, "");
        }

        /// <summary>
        /// Creates a new <see cref="Symbol"/> object.
        /// </summary>
        /// <remarks>
        /// The new <see cref="Symbol"/> is guaranteed to NOT be a collection.
        /// </remarks>
        /// <param name="name">
        /// The name of the <see cref="Symbol"/>.
        /// </param>
        /// <param name="parent">
        /// The <see cref="Symbol"/> that this <see cref="Symbol"/> is a child of.
        /// </param>
        private Symbol(string name, Symbol parent)
            : this(name, parent, false, 1)
        {
        }

        /// <summary>
        /// Creates a new <see cref="Symbol"/> object.
        /// </summary>
        /// <param name="name">
        /// The name of the <see cref="Symbol"/>.
        /// </param>
        /// <param name="parent">
        /// The <see cref="Symbol"/> that this <see cref="Symbol"/> is a child of.
        /// </param>
        /// <param name="isCollection">
        /// A value indicating whether this symbol represents a collection.
        /// </param>
        /// <param name="elemCount">
        /// The number of elements in the collection.
        /// MUST be -1 if the symbol does not represent a collection.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the <paramref name="elemCount"/> is out of range.
        /// </exception>
        private Symbol(string name, Symbol parent, bool isCollection, int elemCount)
        {
            if (elemCount < 0 || (!isCollection && elemCount != 1))
            {
                throw new ArgumentOutOfRangeException(nameof(elemCount));
            }

            Name = name;
            Parent = parent;
            IsCollection = isCollection;
            symbolTable = new Dictionary<string, Symbol>();
            collectionSymbols = new List<Symbol>(elemCount);
        }

        /// <summary>
        /// Indexer for selecting the <see cref="Symbol"/> of an element of a collection.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>
        /// The <see cref="Symbol"/> object at the specified collection index.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// Thrown if this indexer is used when the <see cref="Symbol"/> it is being used on
        /// does not represent a collection.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the <paramref name="index"/> is negative or greater than the number of
        /// elements in the collection.
        /// </exception>
        public Symbol this[int index]
        {
            get
            {
                if (!IsCollection)
                {
                    throw new NotSupportedException(Resources.NotSupportedExceptionElementAccess);
                }

                if (index < 0 || index >= ElementCount)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return collectionSymbols[index];
            }
        }

        /// <summary>
        /// Gets the parent <see cref="Symbol"/> of this <see cref="Symbol"/>.
        /// </summary>
        public Symbol Parent
        {
            get;
        }

        /// <summary>
        /// Gets the name of this <see cref="Symbol"/>.
        /// </summary>
        public string Name
        {
            get;
        }

        /// <summary>
        /// Gets the fully-qualified name of this <see cref="Symbol"/>.
        /// </summary>
        public string FullName
        {
            get { return GetFullName(); }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Symbol"/> represents a collection.
        /// </summary>
        public bool IsCollection
        {
            get;
        }

        /// <summary>
        /// Gets value indicating whether this <see cref="Symbol"/> contains child symbols.
        /// </summary>
        public bool IsLeaf
        {
            get { return symbolTable.Count == 0 && collectionSymbols.Count == 0; }
        }

        /// <summary>
        /// Gets the number of elements in the collection.
        /// Returns -1 if the <see cref="Symbol"/> does not represent a collection.
        /// </summary>
        public int ElementCount
        {
            get { return (IsCollection) ? collectionSymbols.Count : -1; }
        }

        public int DataOffset
        {
            get;
            set;
        }

        public int DataLength
        {
            get;
            set;
        }

        public Type DataType
        {
            get;
            set;
        }

        /// <summary>
        /// Adds an entry to this <see cref="Symbol"/>'s symbol table.
        /// </summary>
        /// <param name="name">
        /// The name of the entry to add.
        /// </param>
        /// <returns>
        /// The new <see cref="Symbol"/> if the entry was successfully added,
        /// <code>Null</code> if the entry's name contains an invalid character
        /// or if the entry already exists in the symbol table.
        /// </returns>
        public Symbol Insert(string name)
        {
            return Insert(name, 0);
        }

        /// <summary>
        /// Adds an entry to this <see cref="Symbol"/>'s symbol table.
        /// </summary>
        /// <param name="name">
        /// The name of the entry to add.
        /// </param>
        /// <param name="elemCount">
        /// The number of elements in the collection, if adding a symbol that
        /// refers to a collection. Specify 0 to insert a single entry.
        /// </param>
        /// <returns>
        /// The new <see cref="Symbol"/> if the entry was successfully added,
        /// <code>Null</code> if the entry's name contains an invalid character
        /// or if the entry already exists in the symbol table.
        /// </returns>
        public Symbol Insert(string name, int elemCount)
        {
            if (!IsNameValid(name) || Contains(name))
            {
                return null;
            }

            if (elemCount > 0)
            {
                return InsertCollection(name, elemCount);
            }

            if (IsCollection)
            {
                return InsertIntoCollection(name);
            }

            return (symbolTable[name] = new Symbol(name, this));
        }

        /// <summary>
        /// Adds an entry to this <see cref="Symbol"/>'s symbol table.
        /// </summary>
        /// <param name="name">
        /// The name of the entry to add.
        /// </param>
        /// <param name="symbol">
        /// The newly created <see cref="Symbol"/>.
        /// If the insertion fails, this will be set to <code>Null</code>.
        /// </param>
        /// <returns>
        /// <code>True</code> if the insertion succeeded,
        /// <code>False</code> otherwise.
        /// </returns>
        public bool TryInsert(string name, out Symbol symbol)
        {
            if ((symbol = Insert(name)) == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets a value indicating whether a <see cref="Symbol"/> with
        /// the specified name exists as a first-level child.
        /// </summary>
        /// <param name="name">
        /// The name of the <see cref="Symbol"/> to check for.
        /// </param>
        /// <returns>
        /// <code>True</code> if the entry exists,
        /// <code>False</code> otherwise.
        /// </returns>
        public bool Contains(string name)
        {
            if (IsCollection)
            {
                return collectionSymbols[0].Contains(name);
            }

            return symbolTable.ContainsKey(name);
        }

        /// <summary>
        /// Searches the <see cref="Symbol"/> tree for specified name.
        /// The search will be conducted relative to this <see cref="Symbol"/>.
        /// </summary>
        /// <param name="name">
        /// The name to search for.
        /// </param>
        /// <returns>
        /// The <see cref="Symbol"/> if found,
        /// <code>Null</code> otherwise.
        /// </returns>
        public Symbol Lookup(string name)
        {
            string[] splitName = name.Split(new char[] { StructureReferenceOperatorChar }, 2);

            // Get a list of all symbols in scope matching top-level name
            List<Symbol> syms = SearchUp(splitName[0], this);
            if (syms.Count == 0)
            {
                return null;
            }

            if (splitName.Length == 1)
            {
                return syms[0];
            }

            // Iterate through all matches and search down each one for the rest of the name.
            Symbol retval = null;
            foreach (Symbol s in syms)
            {
                if ((retval = SearchDown(splitName[1], s)) != null)
                {
                    break;
                }
            }

            return retval;
        }

        /// <summary>
        /// Searches the <see cref="Symbol"/> tree for specified name.
        /// The search will be conducted relative to this <see cref="Symbol"/>.
        /// </summary>
        /// <param name="name">
        /// The name to search for.
        /// </param>
        /// <param name="symbol">
        /// The variable to store the result of the search in.
        /// If the search fails, this will be set to <code>Null</code>.
        /// </param>
        /// <returns>
        /// <code>True</code> if a <see cref="Symbol"/> matching the provided
        /// name was found, <code>False</code> otherwise.
        /// </returns>
        public bool TryLookup(string name, out Symbol symbol)
        {
            if ((symbol = Lookup(name)) == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the fully-qualified name of this <see cref="Symbol"/>.
        /// </summary>
        /// <returns>
        /// The fully-qualified name of this <see cref="Symbol"/>.
        /// </returns>
        public string GetFullName()
        {
            string fqName = "";
            string currName;
            string collectionElemSymbol = null;
            Symbol curr = this;

            if (curr.Name == null)
            {
                return fqName;
            }

            // Iterate to top of tree
            while (curr != null && curr.Name != null)
            {
                currName = curr.Name;
                if (char.IsDigit(currName[0]))
                {
                    // A symbol whose name starts with a number indicates a collection element
                    collectionElemSymbol = "[" + currName + "]";
                    curr = curr.Parent;
                    continue;
                }

                if (collectionElemSymbol != null)
                {
                    if (!curr.IsCollection)
                    {
                        // Bug! Should never be thrown...
                        throw new InvalidOperationException(Resources.InvalidOperationExceptionReservedSymbolName);
                    }

                    // Append collection element index to collection name
                    currName += collectionElemSymbol;
                    collectionElemSymbol = null;
                }

                // Prepend current name to fully-qualified name and move up a level
                fqName = currName + StructureReferenceOperatorChar + fqName;
                curr = curr.Parent;
            }

            // Remove trailing '.' and return!
            return fqName.Substring(0, fqName.Length - 1);
        }

        /// <summary>
        /// Gets a list of all fully-qualified names descending from and including
        /// this <see cref="Symbol"/>.
        /// </summary>
        public List<string> GetAllFullNames()
        {
            // TODO: this is SLOW!
            // Rather than getting the FQ name for every element
            // using the above function, build the FQ name incrementally.

            List<string> names = new List<string>();

            if (IsCollection)
            {
                foreach (Symbol sym in collectionSymbols)
                {
                    names.Add(sym.FullName);
                    names.AddRange(sym.GetAllFullNames());
                }

                return names;
            }

            foreach (KeyValuePair<string, Symbol> entry in symbolTable)
            {
                Symbol sym = entry.Value;
                names.Add(sym.FullName);
                names.AddRange(sym.GetAllFullNames());
            }

            return names;
        }

        /// <summary>
        /// Gets a list of all <see cref="Symbol"/>s that descend from this symbol.
        /// </summary>
        /// <returns>A list of member <see cref="Symbol"/>s.</returns>
        public List<Symbol> GetAllMembers()
        {
            return symbolTable.Select(x => x.Value).ToList();
        }

        /// <summary>
        /// Inserts a <see cref="Symbol"/> into all elements of a collection.
        /// </summary>
        /// <param name="name">
        /// The name of the <see cref="Symbol"/> to insert.
        /// </param>
        /// <returns>
        /// The new <see cref="Symbol"/> from the first element of the collection
        /// if the entry was successfully added, <code>Null</code> if the entry's
        /// name contains an invalid character, if the entry already exists in the
        /// symbol table, or if the current symbol does not represent a collection.
        /// </returns>
        private Symbol InsertIntoCollection(string name)
        {
            Symbol sym = null;
            foreach (Symbol elem in collectionSymbols)
            {
                if (sym == null)
                {
                    sym = elem.Insert(name);
                }
                else
                {
                    elem.Insert(name);
                }
            }

            return sym;
        }

        /// <summary>
        /// Adds a collection entry to this <see cref="Symbol"/>'s symbol table.
        /// </summary>
        /// <param name="name">
        /// The name of the entry to add.
        /// </param>
        /// <param name="elemCount">
        /// The number of elements in the collection.
        /// </param>
        /// <returns>
        /// The new <see cref="Symbol"/> if the entry was successfully added,
        /// <code>Null</code> if the entry's name contains an invalid character
        /// or if the entry already exists in the symbol table.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the <paramref name="elemCount"/> is negative.
        /// </exception>
        private Symbol InsertCollection(string name, int elemCount)
        {
            if (IsCollection)
            {
                return InsertCollectionIntoCollection(name, elemCount);
            }

            Symbol sym = new Symbol(name, this, true, elemCount);
            for (int i = 0; i < elemCount; i++)
            {
                sym.collectionSymbols.Add(new Symbol(i.ToString(), sym));
            }

            return (symbolTable[name] = sym);
        }

        /// <summary>
        /// Inserts a <see cref="Symbol"/> representing a collection into all
        /// elements of another collection.
        /// </summary>
        /// <param name="name">
        /// The name of the <see cref="Symbol"/> to insert.
        /// </param>
        /// <param name="elemCount">
        /// The number of elements in the collection to add.
        /// </param>
        /// <returns>
        /// The new <see cref="Symbol"/> from the first element of the collection
        /// if the entry was successfully added, <code>Null</code> if the entry's
        /// name contains an invalid character, if the entry already exists in the
        /// symbol table, if the current symbol does not represent a collection.
        /// </returns>
        /// /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the <paramref name="elemCount"/> is negative.
        /// </exception>
        private Symbol InsertCollectionIntoCollection(string name, int elemCount)
        {
            if (elemCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(elemCount));
            }

            Symbol sym = null;
            foreach (Symbol elem in collectionSymbols)
            {
                if (sym == null)
                {
                    sym = elem.InsertCollection(name, elemCount);
                }
                else
                {
                    elem.InsertCollection(name, elemCount);
                }
            }

            return sym;
        }

        /// <summary>
        /// Returns an enumerator that iterates through all <see cref="Symbol"/>s in the collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator{Symbol}"/> that iterates through all
        /// <see cref="Symbol"/>s in the collection.
        /// </returns>
        public IEnumerator<Symbol> GetEnumerator()
        {
            return collectionSymbols.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through all elements in the collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator{Symbol}"/> that iterates through all
        /// elements in the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        //public override int GetHashCode()
        //{
        //    return base.GetHashCode();
        //}

        //public override bool Equals(object obj)
        //{
        //    if (!(obj is Symbol))
        //    {
        //        return false;
        //    }

        //    Symbol sym = (Symbol) obj;

        //    if (IsLeaf)
        //    {
        //        if (!sym.IsLeaf)
        //        {
        //            return false;
        //        }

        //        return Name == sym.Name;
        //    }

        //    return false;
        //}

        /// <summary>
        /// Gets the string representation of this <see cref="Symbol"/>.
        /// </summary>
        /// <returns>
        /// The string representation of this object.
        /// </returns>
        public override string ToString()
        {
            string elemCountStr = string.Format(", ElementCount = {0}", ElementCount);
            return string.Format("Symbol: [ FullName = {0}, IsCollection = {1}{2} ]",
                FullName, IsCollection, (IsCollection) ? elemCountStr : "");
        }
    }
}
