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
using System.Collections.Generic;
using System.Linq;
using static WHampson.Cascara.Interpreter.ReservedWords;

namespace WHampson.Cascara.Interpreter
{
    /// <summary>
    /// Represents an instruction to be carried out by the interpreter.
    /// </summary>
    internal abstract class Statement : ISourceElement, IEquatable<Statement>
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
        /// An integer pair representing the text coordinates of the statement in the source code.
        /// 'Item1' is the line number; 'Item2' is the column number.
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
        /// Gets a value indicating whether this <see cref="Statement"/> has parameters.
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

        protected void Parse()
        {
            ExtractInfo();
            DetermineType();
        }

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

        protected void SetParameter(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException(nameof(key));
            }
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(nameof(value));
            }

            _parameters[key.Trim()] = value.Trim();
        }

        /// <summary>
        /// Calculates the <see cref="StatementType"/> using the keyword
        /// extracted from the source code.
        /// </summary>
        /// <exception cref="SyntaxException">
        /// Thrown if the keyword is not valid.
        /// </exception>
        protected void DetermineType()
        {
            string msg;

            switch (Keyword)
            {
                case Keywords.Align:
                case Keywords.Echo:
                case Keywords.Include:
                    StatementType = StatementType.Directive;
                    break;

                case Keywords.Local:
                    StatementType = StatementType.LocalVariableDefinition;
                    break;

                case Keywords.Typedef:
                    StatementType = StatementType.TypeDefinition;
                    break;

                case Keywords.XmlDocumentRoot:
                    msg = Resources.SyntaxExceptionXmlInvalidUsageOfRootElement;
                    throw LayoutException.Create<SyntaxException>(null, this, msg, Keyword);

                default:
                    if (!Keywords.AllKeywords.Contains(Keyword))
                    {
                        msg = Resources.SyntaxExceptionUnknownIdentifier;
                        throw LayoutException.Create<SyntaxException>(null, this, msg, Keyword);
                    }

                    StatementType = StatementType.FileObjectDefinition;
                    break;
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

        int ISourceElement.LineNumber
        {
            get { return _lineNumber; }
        }

        int ISourceElement.LinePosition
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
            string paramStr = "{";
            foreach (var kvp in Parameters)
            {
                paramStr += " " + kvp.Key + " => " + kvp.Value + ",";
            }
            paramStr = paramStr.Substring(0, Math.Max(1, paramStr.Length - 1)) + " }";

            return string.Format("{0}: [ {1} = {2}, {3} = {4}, {5} = {6}, {7} = {8} ]",
                GetType().Name,
                nameof(StatementType), StatementType,
                nameof(Keyword), Keyword,
                nameof(Parameters), paramStr,
                nameof(HasNestedStatements), HasNestedStatements);
        }
    }

    /// <summary>
    /// Defines all possible kinds of <see cref="Statement"/>s.
    /// </summary>
    public enum StatementType
    {
        /// <summary>
        /// The type assigned before the <see cref="Statement"/> has been parsed.
        /// </summary>
        None,

        /// <summary>
        /// Directs the interpreter to carry out some command.
        /// </summary>
        Directive,                  // <align count="2"/>, <include path="foo.xml"/>, <echo message="Fee Fie Foe Fum!"/>

        /// <summary>
        /// Defines an object at the current address in the file.
        /// </summary>
        FileObjectDefinition,       // <float/>, <int name="foo"/>, <struct name="my_struct"><float/></struct>

        /// <summary>
        /// Defines a variable with some value in the current scope.
        /// </summary>
        /// <remarks>
        /// Local variables are not mapped to the file data.
        /// </remarks>
        LocalVariableDefinition,    // <local name="a" value="4"/>, 

        /// <summary>
        /// Defines a new data type and a globally-accessible identifier for that type.
        /// </summary>
        TypeDefinition,             // <typedef name="my_int" kind="int"/>
    }
}
