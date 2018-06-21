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
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using WHampson.Cascara.Interpreter;

namespace WHampson.Cascara
{
    /// <summary>
    /// Represents a primitive data type found in a <see cref="BinaryFile"/>.
    /// </summary>
    /// <typeparam name="T">The .NET or Cascara type that this object encapsulates.</typeparam>
    public class Primitive<T> : IFileObject
        where T : struct
    {
        internal Primitive(BinaryFile sourceFile, SymbolTable symbol)
        {
            if (!PrimitiveTypeUtils.IsPrimitiveType<T>())
            {
                string msg = Resources.ArgumentExceptionPrimitiveType;
                throw new ArgumentException(msg, nameof(T));
            }
            if (PrimitiveTypeUtils.SizeOf<T>() > symbol.DataLength)
            {
                string msg = "The type provided cannot be larger than the size of the data field.";
                throw new ArgumentException(msg, nameof(T));
            }

            this.SourceFile = sourceFile;
            this.Symbol = symbol;
        }

        /// <summary>
        /// Gets the element of the collection at the specified index.
        /// Throws <see cref="InvalidOperationException"/> if this <see cref="Primitive{T}"/>
        /// does not represent a collection.
        /// </summary>
        /// <param name="index">The index of the element to get.</param>
        /// <returns>The element at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the specified index is negative or greater than or equal to the element count.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this <see cref="Primitive{T}"/> does not represent a collection.
        /// </exception>
        /// <seealso cref="IsCollection"/>
        public Primitive<T> this[int index]
        {
            get { return (Primitive<T>) ((IFileObject) this).ElementAt(index); }
        }

        /// <summary>
        /// Gets or sets the value of this primitive type.
        /// Setting this property will change the bytes in the <see cref="BinaryFile"/>
        /// at the location specified by <see cref="GlobalOffset"/>.
        /// </summary>
        public T Value
        {
            get
            {
                if (IsCollection)
                {
                    string fmt = "The '{0}' property can only be used on elements of a collection, not the collection itself.";
                    string msg = string.Format(fmt, nameof(Value));
                    throw new InvalidOperationException(msg);
                }
                return SourceFile.Get<T>(GlobalOffset);
            }

            set
            {
                if (IsCollection)
                {
                    string fmt = "The '{0}' property can only be used on elements of a collection, not the collection itself.";
                    string msg = string.Format(fmt, nameof(Value));
                    throw new InvalidOperationException(msg);
                }
                SourceFile.Set<T>(GlobalOffset, value);
            }
        }

        /// <summary>
        /// Gets the value of this primitive type as a string.
        /// </summary>
        public string StringValue
        {
            get
            {
                if (!IsCollection)
                {
                    return Value.ToString();
                }

                string val = "";

                if (PrimitiveTypeUtils.IsCharacterType<T>())
                {
                    for (int i = 0; i < ElementCount; i++)
                    {
                        if (Convert.ToChar(this[i].Value) == '\0')
                        {
                            break;
                        }
                        val += this[i].StringValue;
                    }
                }
                else
                {
                    JArray a = new JArray();
                    for (int i = 0; i < ElementCount; i++)
                    {
                        a.Add(JToken.FromObject(this[i].Value));
                    }
                    val = a.ToString(Formatting.None);
                }

                return val;
            }
        }

        /// <summary>
        /// Gets the position of this <see cref="IFileObject"/> relative to the start
        /// of the <see cref="BinaryFile"/>.
        /// </summary>
        public int GlobalOffset
        {
            get { return Symbol.GlobalDataAddress; }
        }

        /// <summary>
        /// Gets the position of this <see cref="IFileObject"/> relative to the start
        /// of the parent object.
        /// </summary>
        public int LocalOffset
        {
            get { return Symbol.LocalDataAddress; }
        }

        /// <summary>
        /// Gets the number of bytes that make up this <see cref="IFileObject"/>.
        /// </summary>
        public int Length
        {
            get { return Symbol.DataLength; }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IFileObject"/> represents a collection.
        /// </summary>
        public bool IsCollection
        {
            get { return Symbol.IsCollection; }
        }

        /// <summary>
        /// Gets the number of elements in the collection represented by this <see cref="IFileObject"/>.
        /// Returns 0 if this <see cref="IFileObject"/> does not represent a collection.
        /// </summary>
        /// <seealso cref="IsCollection"/>
        public int ElementCount
        {
            get { return Symbol.ElementCount; }
        }

        /// <summary>
        /// Gets the <see cref="BinaryFile"/> that this <see cref="IFileObject"/>
        /// belongs to.
        /// </summary>
        public BinaryFile SourceFile
        {
            get;
        }

        internal SymbolTable Symbol
        {
            get;
        }

        /// <summary>
        /// Treats the sequence of bytes that make up this type as if they were
        /// a different type. It is important to note that the properties of the
        /// type (length, position, element count), do not change.
        /// </summary>
        /// <typeparam name="U">The value type to cast to.</typeparam>
        /// <exception cref="ArgumentException">
        /// Thrown if U is larger than <see cref="Length"/>
        /// </exception>
        public Primitive<U> ReinterpretCast<U>()
            where U : struct
        {
            return new Primitive<U>(SourceFile, Symbol);
        }

        /// <summary>
        /// Gets the element at the specified index in the collection as an <see cref="IFileObject"/>.
        /// If this <see cref="IFileObject"/> does not represent a collection, an
        /// <see cref="InvalidOperationException"/> will be thrown.
        /// </summary>
        /// <param name="index">The index of the element to get.</param>
        /// <returns>The element at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the specified index is negative or greater than or equal to the element count.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this <see cref="IFileObject"/> does not represent a collection.
        /// </exception>
        /// <seealso cref="IsCollection"/>
        IFileObject IFileObject.ElementAt(int index)
        {
            if (!IsCollection)
            {
                string msg = "Object instance is not a collection.";
                throw new InvalidOperationException(msg);
            }

            if (index < 0 || index >= ElementCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return new Primitive<T>(SourceFile, Symbol[index]);
        }

        /// <summary>
        /// Returns an enumerator that iterates through a all elements of this
        /// collection.
        /// </summary>
        public IEnumerator<IFileObject> GetEnumerator()
        {
            if (!IsCollection)
            {
                yield break;
            }

            for (int i = 0; i < ElementCount; i++)
            {
                yield return ((IFileObject) this).ElementAt(i);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Returns a string that represents this object.
        /// </summary>
        public override string ToString()
        {
            JObject o = new JObject();
            o.Add(nameof(Symbol.FullName), Symbol.FullName);
            o.Add(nameof(Symbol.DataType), Symbol.DataType.Name);
            o.Add(nameof(GlobalOffset), GlobalOffset);
            o.Add(nameof(LocalOffset), LocalOffset);
            o.Add(nameof(Length), Length);

            if (IsCollection)
            {
                o.Add(nameof(ElementCount), ElementCount);
            }

            if (PrimitiveTypeUtils.IsCharacterType<T>())
            {
                o.Add(nameof(Value), StringValue);
            }
            else
            {
                o.Add(nameof(Value), JToken.Parse(StringValue));
            }

            return o.ToString(Newtonsoft.Json.Formatting.None);
        }
    }
}
