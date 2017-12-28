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
using static WHampson.Cascara.ReservedWords;

namespace WHampson.Cascara
{
    /// <summary>
    /// Represents an instruction to be carried out by the interpreter.
    /// </summary>
    internal abstract class Statement : IEquatable<Statement>
    {
        /// <summary>
        /// Defines all possible kinds of <see cref="Statement"/>s.
        /// </summary>
        public enum StatementType
        {
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

        private List<Statement> _nestedStatements;
        private Dictionary<string, string> _parameters;

        protected Statement(object sourceObject)
        {
            if (sourceObject == null)
            {
                throw new ArgumentNullException(nameof(sourceObject));
            }

            SourceObject = sourceObject;
            _parameters = new Dictionary<string, string>();
            _nestedStatements = new List<Statement>();

            Parse();
        }

        /// <summary>
        /// Gets the object from the source code that created this <see cref="Statement"/>.
        /// </summary>
        public object SourceObject
        {
            get;
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
            get { return !_nestedStatements.Any(); }
        }

        /// <summary>
        /// Gets an enumeration of all child <see cref="Statement"/>s.
        /// </summary>
        public IEnumerable<Statement> NestedStatements
        {
            get { return _nestedStatements; }
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
            _parameters[key.Trim()] = value.Trim();
        }

        protected abstract void Parse();

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

                foreach (var kvp in _parameters)
                {
                    hash = (hash * 37) ^ (kvp.Key.GetHashCode() ^ kvp.Value.GetHashCode());
                }

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
                nameof(Keyword), Keyword,
                nameof(Parameters), paramStr,
                nameof(HasNestedStatements), HasNestedStatements,
                nameof(SourceObject), SourceObject);
        }
    }
}
