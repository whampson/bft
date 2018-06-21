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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace WHampson.Cascara.Interpreter
{
    /// <summary>
    /// Represents a node in a symbol tree.
    /// Symbol trees are used to keep track of and maintain the hierarchy
    /// of identifiers during parsing.
    /// </summary>
    internal class SymbolTable : IEnumerable<SymbolTable>
    {
        /// <summary>
        /// The character used to denote the member of a structure.
        /// Example: 'x.y' means "member y of structure x".
        /// </summary>
        private const char StructureReferenceOperatorChar = '.';

        /// <summary>
        /// Regex pattern for matching identifiers that refer to collection elements.
        /// </summary>
        private const string CollectionNotationPattern = @"\[(\d+)\]$";

        /// <summary>
        /// Creates a nameless, parentless <see cref="SymbolTable"/> for use
        /// as the root in a symbol tree.
        /// </summary>
        /// <returns>
        /// The newly-created <see cref="SymbolTable"/> object.
        /// </returns>
        internal static SymbolTable CreateRootSymbolTable()
        {
            return new SymbolTable(null, null);
        }

        /// <summary>
        /// Creates a nameless <see cref="SymbolTable"/> with the specified parent <see cref="SymbolTable"/>.
        /// The parent does not contain a reference to this <see cref="SymbolTable"/>.
        /// </summary>
        /// <param name="parent">
        /// The <see cref="SymbolTable"/> that this <see cref="SymbolTable"/> stems from.
        /// </param>
        /// <returns>
        /// The newly-created <see cref="SymbolTable"/> object.
        /// </returns>
        internal static SymbolTable CreateNamelessSymbolTable(SymbolTable parent)
        {
            return new SymbolTable(null, parent);
        }

        /// <summary>
        /// Checks whether a given string satisfies the requirements
        /// for a symbol table entry.
        /// </summary>
        /// <remarks>
        /// Identifiers must consist of only alphabetic characters (any case),
        /// decimal digits, or underscores. Identifiers cannot begin with a decimal digit.
        /// </remarks>
        /// <param name="identifier">
        /// The string to validate.
        /// </param>
        /// <returns>
        /// <code>True</code> if the specified string meets the criteria for a
        /// valid <see cref="SymbolTable"/> name, <code>False</code> otherwise.
        /// </returns>
        internal static bool IsIdentifierValid(string identifier)
        {
            // Don't allow identifiers to start with a digit.
            // This is because we use numbers as the names of collection element
            // symbol tables internally.
            if (string.IsNullOrWhiteSpace(identifier) || char.IsDigit(identifier[0]))
            {
                return false;
            }

            // Don't allow identifiers that match reserved words
            if (ReservedWords.AllReservedWords.Contains(identifier))
            {
                return false;
            }

            foreach (char c in identifier)
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
        /// Searches up the symbol tree from the given <see cref="SymbolTable"/>
        /// to the root for all occurrences of the specified identifier.
        /// </summary>
        /// <remarks>
        /// This is a wrapper function for <see cref="SearchUp(string, SymbolTable, ref List{SymbolTable})"/>.
        /// </remarks>
        /// <param name="identifier">
        /// The identifier to search for.
        /// </param>
        /// <param name="sym">
        /// The node in the symbol tree from which to begin the search.
        /// </param>
        /// <returns>
        /// A <see cref="List{Symbol}"/> of all <see cref="SymbolTable"/>s with a matching identifier.
        /// </returns>
        private static List<SymbolTable> SearchUp(string identifier, SymbolTable sym)
        {
            List<SymbolTable> results = new List<SymbolTable>();
            SearchUp(identifier, sym, ref results);

            return results;
        }

        /// <summary>
        /// Searches up the symbol tree from the given <see cref="SymbolTable"/>
        /// to the root for all occurrences of the specified identifier.
        /// </summary>
        /// <remarks>
        /// If matches are found, they are said to be "within the scope of" <paramref name="sym"/>.
        /// </remarks>
        /// <param name="identifier">
        /// The identifier to search for.
        /// </param>
        /// <param name="sym">
        /// The node in the symbol tree from which to begin the search.
        /// </param>
        /// <param name="results">
        /// A <see cref="List{Symbol}"/> reference which is added to in each recursive
        /// call of this function.
        /// </param>
        private static void SearchUp(string identifier, SymbolTable sym, ref List<SymbolTable> results)
        {
            if (sym == null)
            {
                return;
            }

            string tempName = identifier;

            // Read collection index and remove from symbol name (if applicable)
            bool isSearchingForCollection = TryGetElementIndex(tempName, out int index);
            if (isSearchingForCollection)
            {
                tempName = StripCollectionNotation(tempName);
            }

            // Add entry to list if it exists in the current table
            if (sym.Members.TryGetValue(tempName, out SymbolTable entry))
            {
                if (isSearchingForCollection)
                {
                    if (entry.IsCollection && index < entry.ElementCount)
                    {
                        results.Add(entry.CollectionElements[index]);
                    }
                }
                else
                {
                    results.Add(entry);
                }
            }

            // Search the parent table
            SearchUp(identifier, sym.Parent, ref results);
        }

        /// <summary>
        /// Searches down the symbol tree from the given node
        /// for the given filly-qualified symbol name.
        /// </summary>
        /// <param name="identifier">
        /// The fully-qualified name of the <see cref="SymbolTable"/> search for.
        /// </param>
        /// <param name="sym">
        /// The node of the symbol tree to begin the search.
        /// </param>
        /// <returns>
        /// The <see cref="SymbolTable"/> with the matching name, if found.
        /// <code>Null</code> otherwise.
        /// </returns>
        private static SymbolTable SearchDown(string identifier, SymbolTable sym)
        {
            if (sym == null)
            {
                return null;
            }

            string[] splitIdent = identifier.Split(new char[] { StructureReferenceOperatorChar }, 2);

            // Read collection index and remove from symbol name (if applicable)
            bool isSearchingForCollection = TryGetElementIndex(splitIdent[0], out int index);
            if (isSearchingForCollection)
            {
                splitIdent[0] = StripCollectionNotation(splitIdent[0]);
            }

            // Look for top-level name in current symbol table
            if (!sym.Members.TryGetValue(splitIdent[0], out SymbolTable entry))
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
                entry = entry.CollectionElements[index];
            }

            // If only a top-level name was specified (no dot), return the entry (base case)
            if (splitIdent.Length == 1)
            {
                return entry;
            }

            // Continue searching down the tree for the rest of the name
            return SearchDown(splitIdent[1], entry);
        }

        /// <summary>
        /// Gets the collection element index from the specified symbol name.
        /// A return value indicates whether the extraction succeeded.
        /// </summary>
        /// <remarks>
        /// If the name passed in does not refer to a collection element,
        /// <code>False</code> will be returned.
        /// </remarks>
        /// <param name="identifier">
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
        private static bool TryGetElementIndex(string identifier, out int index)
        {
            Match m = Regex.Match(identifier, CollectionNotationPattern);

            return int.TryParse(m.Groups[1].Value, out index);
        }

        /// <summary>
        /// Removes the "element brackets" from the given symbol identifier.
        /// </summary>
        /// <remarks>
        /// If the name passed in does not refer to a collection element,
        /// <paramref name="identifier"/> will be returned.
        /// </remarks>
        /// <param name="identifier">
        /// The symbol to strip the collection notation from.
        /// </param>
        /// <returns>
        /// The modified <see cref="SymbolTable"/> name.
        /// </returns>
        private static string StripCollectionNotation(string identifier)
        {
            return Regex.Replace(identifier, CollectionNotationPattern, "");
        }

        /// <summary>
        /// Creates a new <see cref="SymbolTable"/> object.
        /// </summary>
        /// <remarks>
        /// The new <see cref="SymbolTable"/> is guaranteed to NOT be a collection.
        /// </remarks>
        /// <param name="name">
        /// The name of the <see cref="SymbolTable"/>.
        /// </param>
        /// <param name="parent">
        /// The <see cref="SymbolTable"/> that this <see cref="SymbolTable"/> is a child of.
        /// </param>
        private SymbolTable(string name, SymbolTable parent)
            : this(name, parent, 0)
        {
        }

        /// <summary>
        /// Creates a new <see cref="SymbolTable"/> object.
        /// </summary>
        /// <param name="identifier">
        /// The name of the <see cref="SymbolTable"/>.
        /// </param>
        /// <param name="parent">
        /// The <see cref="SymbolTable"/> that this <see cref="SymbolTable"/> is a child of.
        /// </param>
        /// <param name="elemCount">
        /// The number of elements in the collection.
        /// 0 indicates that the symbol is not a collection.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the <paramref name="elemCount"/> is out of range.
        /// </exception>
        private SymbolTable(string identifier, SymbolTable parent, int elemCount)
        {
            if (elemCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(elemCount));
            }

            Name = identifier;
            Parent = parent;
            Members = new Dictionary<string, SymbolTable>();
            CollectionElements = new List<SymbolTable>(elemCount);
        }

        /// <summary>
        /// Indexer for selecting the <see cref="SymbolTable"/> of an element in a collection.
        /// </summary>
        /// <param name="index">The element index.</param>
        /// <returns>
        /// The <see cref="SymbolTable"/> object at the specified collection index.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// Thrown if this indexer is used when the <see cref="SymbolTable"/> it is being used on
        /// does not represent a collection.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the <paramref name="index"/> is negative or greater than the number of
        /// elements in the collection.
        /// </exception>
        /// <see cref="IsCollection"/>
        public SymbolTable this[int index]
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

                return CollectionElements[index];
            }
        }

        /// <summary>
        /// Gets the parent <see cref="SymbolTable"/>.
        /// If this <see cref="SymbolTable"/> is the root, this property
        /// returns <c>null</c>.
        /// </summary>
        public SymbolTable Parent
        {
            get;
        }

        /// <summary>
        /// Gets the name of this <see cref="SymbolTable"/>.
        /// </summary>
        public string Name
        {
            get;
        }

        /// <summary>
        /// Gets the fully-qualified name of this <see cref="SymbolTable"/>.
        /// </summary>
        public string FullName
        {
            get { return GetFullName(); }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="SymbolTable"/> represents a collection.
        /// </summary>
        public bool IsCollection
        {
            get { return CollectionElements.Count != 0; }
        }

        /// <summary>
        /// Gets the number of elements in the collection.
        /// Returns 0 if the <see cref="SymbolTable"/> does not represent a collection.
        /// </summary>
        public int ElementCount
        {
            get { return CollectionElements.Count; }
        }

        /// <summary>
        /// Gets value indicating whether this <see cref="SymbolTable"/> represents a composite data structure.
        /// Composite data structure symbols have child tables, one for each member of the structure.
        /// </summary>
        public bool IsStruct
        {
            get { return Members.Any(); }
        }

        /// <summary>
        /// Gets or sets the absolute address of the data represented by the symbol.
        /// </summary>
        public int GlobalDataAddress
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the address of the data represented by the symbol relative to its parent's address.
        /// </summary>
        public int LocalDataAddress
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the size (in bytes) of the data represented by the symbol.
        /// </summary>
        public int DataLength
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the <see cref="Type"/> of data represented by the symbol,
        /// if the symbol represents a primitive type.
        /// </summary>
        public Type DataType
        {
            get;
            set;
        }

        /// <summary>
        /// A dictionary of member identifiers to child <see cref="SymbolTable"/>s.
        /// </summary>
        private Dictionary<string, SymbolTable> Members
        {
            get;
            set;
        }

        /// <summary>
        /// A list of child <see cref="SymbolTable"/>s corresponding to each element of the collection.
        /// </summary>
        private List<SymbolTable> CollectionElements
        {
            get;
            set;
        }

        /// <summary>
        /// Adds an entry to the symbol table.
        /// </summary>
        /// <param name="identifier">
        /// The name of the entry to add.
        /// </param>
        /// <returns>
        /// The new <see cref="SymbolTable"/> if the entry was successfully added,
        /// <code>Null</code> if the entry's name contains an invalid character
        /// or if the entry already exists in the symbol table.
        /// </returns>
        public SymbolTable Insert(string identifier)
        {
            return Insert(identifier, 0);
        }

        /// <summary>
        /// Adds an entry to the symbol table.
        /// </summary>
        /// <param name="identifier">
        /// The name of the entry to add.
        /// </param>
        /// <param name="elemCount">
        /// The number of elements in the collection, if adding a symbol that
        /// refers to a collection. If the symbol should not represent a collection,
        /// set this value to 0.
        /// </param>
        /// <returns>
        /// The new <see cref="SymbolTable"/> if the entry was successfully added,
        /// <code>Null</code> if the entry's name contains an invalid character
        /// or if the entry already exists in the symbol table.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the <paramref name="elemCount"/> is a negative number.
        /// </exception>
        public SymbolTable Insert(string identifier, int elemCount)
        {
            if (!IsIdentifierValid(identifier) || Contains(identifier))
            {
                return null;
            }

            if (elemCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(elemCount),
                    Resources.ArgumentExceptionNonNegativeInteger);
            }

            if (elemCount > 0)
            {
                return InsertCollection(identifier, elemCount);
            }

            if (IsCollection)
            {
                return InsertIntoCollection(identifier);
            }

            return (Members[identifier] = new SymbolTable(identifier, this));
        }

        /// <summary>
        /// Attempts to add an entry to the symbol table and returns a value
        /// indicating whether or not the operation was successful.
        /// </summary>
        /// <param name="identifier">
        /// The name of the entry to add.
        /// </param>
        /// <param name="symbol">
        /// The newly created <see cref="SymbolTable"/>.
        /// If the insertion fails, this will be set to <code>Null</code>.
        /// </param>
        /// <returns>
        /// <code>True</code> if the insertion succeeded,
        /// <code>False</code> otherwise.
        /// </returns>
        public bool TryInsert(string identifier, out SymbolTable symbol)
        {
            return (symbol = Insert(identifier)) != null;
        }

        /// <summary>
        /// Attempts to add an entry to the symbol table and returns a value
        /// indicating whether or not the operation was successful.
        /// </summary>
        /// <param name="identifier">
        /// The name of the entry to add.
        /// </param>
        /// <param name="elemCount">
        /// The number of elements in the collection, if adding a symbol that
        /// refers to a collection. If the symbol should not represent a collection,
        /// set this value to 0.
        /// </param>
        /// <param name="symbol">
        /// The newly created <see cref="SymbolTable"/>.
        /// If the insertion fails, this will be set to <code>Null</code>.
        /// </param>
        /// <returns>
        /// <code>True</code> if the insertion succeeded,
        /// <code>False</code> otherwise.
        /// </returns>
        public bool TryInsert(string identifier, int elemCount, out SymbolTable symbol)
        {
            return (symbol = Insert(identifier, elemCount)) != null;
        }

        /// <summary>
        /// Gets a value indicating whether a symbol with
        /// the specified name exists as a first-level child.
        /// </summary>
        /// <param name="identifier">
        /// The name of the symbol to check for.
        /// </param>
        /// <returns>
        /// <code>True</code> if the entry exists,
        /// <code>False</code> otherwise.
        /// </returns>
        public bool Contains(string identifier)
        {
            if (IsCollection)
            {
                // All elements have the same set of symbols, albeit with different addresses,
                // so checking element 0 is sufficient
                return CollectionElements[0].Contains(identifier);
            }

            return Members.ContainsKey(identifier);
        }

        /// <summary>
        /// Searches the symbol tree for specified symbol.
        /// The search will be conducted relative to this <see cref="SymbolTable"/>.
        /// </summary>
        /// <param name="identifier">
        /// The symbol to search for.
        /// </param>
        /// <returns>
        /// The symbol's <see cref="SymbolTable"/> if found,
        /// <code>Null</code> otherwise.
        /// </returns>
        public SymbolTable Lookup(string identifier)
        {
            string[] splitIdent = identifier.Split(new char[] { StructureReferenceOperatorChar }, 2);

            // Get a list of all symbols in scope matching top-level name
            List<SymbolTable> symbols = SearchUp(splitIdent[0], this);
            if (symbols.Count == 0)
            {
                return null;
            }

            // Return the first match if there are no more tables to search
            if (splitIdent.Length == 1)
            {
                return symbols[0];
            }

            // Iterate through all matches and search down each one for the rest of the name.
            SymbolTable retval = null;
            foreach (SymbolTable s in symbols)
            {
                if ((retval = SearchDown(splitIdent[1], s)) != null)
                {
                    break;
                }
            }

            return retval;
        }

        /// <summary>
        /// Attempts to search the symbol tree for specified symbol and returns
        /// a value indicating whether the search was successful.
        /// The search will be conducted relative to this <see cref="SymbolTable"/>.
        /// </summary>
        /// <param name="identifier">
        /// The symbol to search for.
        /// </param>
        /// <param name="table">
        /// The variable to store the result of the search in.
        /// If the search fails, this will be set to <code>Null</code>.
        /// </param>
        /// <returns>
        /// <code>True</code> if a <see cref="SymbolTable"/> matching the provided
        /// name was found, <code>False</code> otherwise.
        /// </returns>
        public bool TryLookup(string identifier, out SymbolTable table)
        {
            return (table = Lookup(identifier)) != null;
        }

        /// <summary>
        /// Gets the fully-qualified name of this symbol.
        /// </summary>
        /// <returns>
        /// The fully-qualified name of this symbol.
        /// </returns>
        public string GetFullName()
        {
            string fqName = "";
            string currName;
            string collectionElemSymbol = null;
            SymbolTable curr = this;

            if (curr.Name == null)
            {
                return fqName;
            }

            // Iterate to top of tree
            while (curr != null && curr.Name != null)
            {
                currName = curr.Name;

                // If symbol name begins with a digit, it is an internal name used for collection elements
                if (char.IsDigit(currName[0]))
                {
                    // Append the "array brackets" to the name
                    collectionElemSymbol = "[" + currName + "]";
                    curr = curr.Parent;
                    continue;
                }

                if (collectionElemSymbol != null)
                {
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
        /// this symbol.
        /// </summary>
        public List<string> GetAllFullNames()
        {
            // TODO: this is SLOW!
            // CONSIDER: Rather than getting the FQ name for every element
            // using the above function, build the FQ name incrementally.

            List<string> names = new List<string>();

            if (IsCollection)
            {
                foreach (SymbolTable sym in CollectionElements)
                {
                    names.Add(sym.FullName);
                    names.AddRange(sym.GetAllFullNames());
                }

                return names;
            }

            foreach (KeyValuePair<string, SymbolTable> entry in Members)
            {
                SymbolTable sym = entry.Value;
                names.Add(sym.FullName);
                names.AddRange(sym.GetAllFullNames());
            }

            return names;
        }

        /// <summary>
        /// Gets a list of all child <see cref="SymbolTable"/>s.
        /// </summary>
        /// <returns>A list of child <see cref="SymbolTable"/>s.</returns>
        public List<SymbolTable> GetAllMembers()
        {
            return Members.Select(x => x.Value).ToList();
        }

        /// <summary>
        /// Inserts a <see cref="SymbolTable"/> into all elements of a collection.
        /// </summary>
        /// <param name="name">
        /// The symbol to insert.
        /// </param>
        /// <returns>
        /// The new <see cref="SymbolTable"/> from the first element of the collection
        /// if the entry was successfully added, <code>Null</code> if the entry's
        /// name contains an invalid character, if the entry already exists in the
        /// symbol table, or if the current symbol does not represent a collection.
        /// </returns>
        private SymbolTable InsertIntoCollection(string name)
        {
            SymbolTable sym = null;
            foreach (SymbolTable elem in CollectionElements)
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
        /// Adds a collection entry to this <see cref="SymbolTable"/>.
        /// </summary>
        /// <param name="identifier">
        /// The name of the entry to add.
        /// </param>
        /// <param name="elemCount">
        /// The number of elements in the collection.
        /// </param>
        /// <returns>
        /// The new <see cref="SymbolTable"/> if the entry was successfully added,
        /// <code>Null</code> if the entry's name contains an invalid character
        /// or if the entry already exists in the symbol table.
        /// </returns>
        private SymbolTable InsertCollection(string identifier, int elemCount)
        {
            if (IsCollection)
            {
                return InsertCollectionIntoCollection(identifier, elemCount);
            }

            SymbolTable sym = new SymbolTable(identifier, this, elemCount);
            for (int i = 0; i < elemCount; i++)
            {
                sym.CollectionElements.Add(new SymbolTable(i.ToString(), sym));
            }

            return (Members[identifier] = sym);
        }

        /// <summary>
        /// Inserts a <see cref="SymbolTable"/> representing a collection into all
        /// elements of another collection.
        /// </summary>
        /// <param name="name">
        /// The name of the <see cref="SymbolTable"/> to insert.
        /// </param>
        /// <param name="elemCount">
        /// The number of elements in the collection to add.
        /// </param>
        /// <returns>
        /// The new <see cref="SymbolTable"/> from the first element of the collection
        /// if the entry was successfully added, <code>Null</code> if the entry's
        /// name contains an invalid character, if the entry already exists in the
        /// symbol table, if the current symbol does not represent a collection.
        /// </returns>
        /// /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the <paramref name="elemCount"/> is negative.
        /// </exception>
        private SymbolTable InsertCollectionIntoCollection(string name, int elemCount)
        {
            if (elemCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(elemCount), Resources.ArgumentExceptionNonNegativeInteger);
            }

            SymbolTable sym = null;
            foreach (SymbolTable elem in CollectionElements)
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
        /// Returns an enumerator that iterates through all <see cref="SymbolTable"/>s in the collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator{Symbol}"/> that iterates through all
        /// <see cref="SymbolTable"/>s in the collection.
        /// </returns>
        public IEnumerator<SymbolTable> GetEnumerator()
        {
            return CollectionElements.GetEnumerator();
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

        /// <summary>
        /// Gets the string representation of this <see cref="SymbolTable"/>.
        /// </summary>
        /// <returns>
        /// The string representation of this object.
        /// </returns>
        public override string ToString()
        {
            JObject o = new JObject();
            o.Add(nameof(Name), Name);
            o.Add(nameof(IsCollection), IsCollection);
            o.Add(nameof(ElementCount), ElementCount);
            o.Add(nameof(IsStruct), IsStruct);
            o.Add(nameof(GlobalDataAddress), GlobalDataAddress);
            o.Add(nameof(LocalDataAddress), LocalDataAddress);
            o.Add(nameof(DataLength), DataLength);
            o.Add(nameof(DataType), DataType.FullName);
            o.Add(nameof(Members), JToken.FromObject(Members));
            return o.ToString(Formatting.None);
        }
    }
}
