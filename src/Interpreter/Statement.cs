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
using System.Collections.Generic;
using System.Linq;
using static WHampson.Cascara.Interpreter.ReservedWords;

namespace WHampson.Cascara.Interpreter
{
    /// <summary>
    /// A complete clause in the layout script language. A
    /// <see cref="Statement"/> is the smallest unit of execution in the layout
    /// script interpreter.
    /// </summary>
    internal abstract class Statement : ISourceEntity, IEquatable<Statement>
    {
        private int _lineNumber;
        private int _linePosition;
        private List<Statement> _nestedStatements;
        private Dictionary<string, string> _parameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="Statement"/> class.
        /// </summary>
        /// <param name="lineNum">
        /// The line number in the source code where the statement begins.
        /// </param>
        /// <param name="linePos">
        /// The column number in the source code where the statement begins.
        /// </param>
        protected Statement(int lineNum, int linePos)
        {
            _lineNumber = (lineNum < 1) ? 0 : lineNum;
            _linePosition = (linePos < 1) ? 0 : linePos;
            _parameters = new Dictionary<string, string>();
            _nestedStatements = new List<Statement>();
            StatementType = StatementType.None;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Statement"/> class.
        /// </summary>
        /// <param name="lineInfo">
        /// An integer pair representing the text coordinates of the statement
        /// in the source code. 'Item1' is the line number; 'Item2' is the
        /// column number.
        /// </param>
        protected Statement(Tuple<int, int> lineInfo)
            : this(lineInfo.Item1, lineInfo.Item2)
        {
        }

        /// <summary>
        /// Gets the keyword that begins the <see cref="Statement"/>.
        /// </summary>
        public string Keyword
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the <see cref="Statement"/> type.
        /// </summary>
        public StatementType StatementType
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Statement"/> has
        /// parameters.
        /// </summary>
        public bool HasParameters
        {
            get { return _parameters.Any(); }
        }

        /// <summary>
        /// Gets a map of all <see cref="Statement"/> parameters.
        /// </summary>
        public IReadOnlyDictionary<string, string> Parameters
        {
            get { return _parameters; }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Statement"/>
        /// is the parent of one or more <see cref="Statement"/>s.
        /// </summary>
        public bool HasNestedStatements
        {
            get { return _nestedStatements.Any(); }
        }

        /// <summary>
        /// Gets an enumeration of all child <see cref="Statement"/>s.
        /// </summary>
        public IEnumerable<Statement> NestedStatements
        {
            get { return _nestedStatements; }
        }

        /// <summary>
        /// Reads parameters from the statement source code and determines the
        /// statement type.
        /// </summary>
        protected void Parse()
        {
            ExtractInfo();
            DetermineType();
        }

        /// <summary>
        /// Adds a <see cref="Statement"/> to the list of nested statements for
        /// this <see cref="Statement"/>.
        /// </summary>
        /// <param name="stmt">
        /// The nested <see cref="Statement"/> to add.
        /// </param>
        protected void AddNestedStatement(Statement stmt)
        {
            if (stmt == null)
            {
                throw new ArgumentNullException(nameof(stmt));
            }
            if (stmt == this)
            {
                throw new ArgumentException(Resources.ArgumentExceptionNestedStatementReference, nameof(stmt));
            }

            _nestedStatements.Add(stmt);
        }

        /// <summary>
        /// Sets a parameter value.
        /// </summary>
        /// <param name="key">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        protected void SetParameter(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                string msg = Resources.ArgumentExceptionEmptyString;
                throw new ArgumentException(msg, nameof(key));
            }
            if (string.IsNullOrWhiteSpace(value))
            {
                string msg = Resources.ArgumentExceptionEmptyString;
                throw new ArgumentException(msg, nameof(value));
            }

            _parameters[key.Trim()] = value.Trim();
        }

        /// <summary>
        /// Calculates the <see cref="StatementType"/> using the keyword
        /// extracted from the source code.
        /// </summary>
        protected void DetermineType()
        {
            if (Keywords.Directives.AllDirectives.Contains(Keyword))
            {
                StatementType = StatementType.Directive;
            }
            else
            {
                // Assume file object definition. We can't know for sure until
                // the statement is parsed by the interpreter because the
                // keyword may be a user-defined type name.
                StatementType = StatementType.FileObjectDefinition;
            }
        }

        /// <summary>
        /// Retrieves the <see cref="Keyword"/>, <see cref="Parameters"/>,
        /// and any nested <see cref="Statement"/>s from the source element.
        /// </summary>
        /// <remarks>
        /// Hint: use the <see cref="SetParameter(string, string)"/>
        /// and <see cref="AddNestedStatement(Statement)"/> helper functions!
        /// </remarks>
        protected abstract void ExtractInfo();

        int ISourceEntity.LineNumber
        {
            get { return _lineNumber; }
        }

        int ISourceEntity.LinePosition
        {
            get { return _linePosition; }
        }

        #region Equality
        public bool Equals(Statement other)
        {
            if (other == null)
            {
                return false;
            }

            if (Keyword != other.Keyword
                || _parameters.Count != other._parameters.Count
                || _nestedStatements.Count != other._nestedStatements.Count)
            {
                return false;
            }

            return _nestedStatements.SequenceEqual(other._nestedStatements);
        }

        public sealed override bool Equals(object obj)
        {
            if (!(obj is Statement))
            {
                return false;
            }
            else if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return Equals(obj as Statement);
        }

        public sealed override int GetHashCode()
        {
            unchecked
            {
                int hash = 13;
                hash = (hash * 37) ^ Keyword.GetHashCode();

                int paramHash = 17;
                foreach (var kvp in _parameters)
                {
                    // Order doesn't matter here, so we add onto a separate hash
                     paramHash += (kvp.Key.GetHashCode() ^ kvp.Value.GetHashCode());
                }
                hash = (hash * 37) ^ paramHash;

                foreach (Statement stmt in _nestedStatements)
                {
                    hash = (hash * 37) ^ stmt.GetHashCode();
                }

                return hash;
            }
        }
        #endregion

        public sealed override string ToString()
        {
            JObject o = new JObject();
            o.Add(nameof(Keyword), Keyword);
            o.Add(nameof(StatementType), StatementType.ToString());
            if (_lineNumber > 0 && _linePosition > 0)
            {
                o.Add("LineNumber", _lineNumber);
                o.Add("LinePosition", _linePosition);
            }
            o.Add(nameof(Parameters), JToken.FromObject(Parameters));

            JArray a = new JArray();
            foreach (Statement s in NestedStatements)
            {
                a.Add(JToken.Parse(s.ToString()));
            }
            o.Add(nameof(NestedStatements), a);
            return o.ToString(Formatting.None);
        }
    }

    /// <summary>
    /// Defines all possible kinds of <see cref="Statement"/>s.
    /// </summary>
    internal enum StatementType
    {
        /// <summary>
        /// The type assigned before the <see cref="Statement"/> has been parsed.
        /// </summary>
        None,

        /// <summary>
        /// Directs the interpreter to carry out some action.
        /// </summary>
        Directive,

        /// <summary>
        /// Defines an object at the current address in the file.
        /// </summary>
        FileObjectDefinition,
    }
}
