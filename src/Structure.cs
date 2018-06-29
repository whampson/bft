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
using System.Reflection;
using WHampson.Cascara.Interpreter;

namespace WHampson.Cascara
{
    /// <summary>
    /// Represents a composite data type found in a <see cref="BinaryData"/>.
    /// </summary>
    public class Structure : IFileObject
    {
        internal Structure(BinaryData sourceFile, SymbolTable symbol)
        {
            this.DataSource = sourceFile;
            Symbol = symbol;
        }

        /// <summary>
        /// Gets the element of the collection at the specified index.
        /// Throws <see cref="InvalidOperationException"/> if this <see cref="Structure"/>
        /// does not represent a collection.
        /// </summary>
        /// <param name="index">The index of the element to get.</param>
        /// <returns>The element at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the specified index is negative or greater than or equal to the element count.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this <see cref="Structure"/> does not represent a collection.
        /// </exception>
        /// <seealso cref="IsCollection"/>
        public Structure this[int index]
        {
            get { return (Structure) ((IFileObject) this).ElementAt(index); }
        }

        /// <summary>
        /// Gets the position of this <see cref="IFileObject"/> relative to the start
        /// of the <see cref="BinaryData"/>.
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
        /// Gets the <see cref="BinaryData"/> that this <see cref="IFileObject"/>
        /// belongs to.
        /// </summary>
        public BinaryData DataSource
        {
            get;
        }

        internal SymbolTable Symbol
        {
            get;
        }

        /// <summary>
        /// Converts the data in this <see cref="Structure"/> into an object
        /// by setting properties or fields using the names specified in a
        /// <see cref="LayoutScript"/>.
        /// <see cref="DeserializationFlags"/> for this method are set to
        /// <see cref="DeserializationFlags.Public"/> and
        /// <see cref="DeserializationFlags.Properties"/>.
        /// </summary>
        public T Deserialize<T>() where T : new()
        {
            DeserializationFlags flags =
                DeserializationFlags.Public | DeserializationFlags.Properties;
            return Deserialize<T>(flags);
        }

        /// <summary>
        /// Converts the data in this <see cref="BinaryData"/> into an object
        /// by setting properties or fields using the names specified in a
        /// <see cref="LayoutScript"/>.
        /// </summary>
        /// <param name="flags">
        /// A <see cref="DeserializationFlags"/> bitfield that controls the
        /// behavior of deserialization.
        /// </param>
        public T Deserialize<T>(DeserializationFlags flags) where T : new()
        {
            return (T) Deserialize(typeof(T), flags);
        }

        private object Deserialize(Type t, DeserializationFlags flags)
        {
            object o;
            BindingFlags bindFlags;

            o = Activator.CreateInstance(t);

            bindFlags = BindingFlags.Instance;
            if (flags.HasFlag(DeserializationFlags.NonPublic))
            {
                bindFlags |= BindingFlags.NonPublic;
            }
            if (flags.HasFlag(DeserializationFlags.Public))
            {
                bindFlags |= BindingFlags.Public;
            }

            o = DeserializeToProperties(t, o, flags, bindFlags);
            o = DeserializeToFields(t, o, flags, bindFlags);

            return o;
        }

        private object DeserializeToProperties(Type t, object o, DeserializationFlags flags, BindingFlags bindFlags)
        {
            PropertyInfo[] allProperties;
            Type propType;
            bool isArray;
            Type elemType;
            bool isGeneric;
            Type genType;
            object val;

            if (!flags.HasFlag(DeserializationFlags.Properties))
            {
                return o;
            }

            allProperties = t.GetProperties(bindFlags);

            foreach (PropertyInfo p in allProperties)
            {
                propType = p.PropertyType;
                isArray = propType.IsArray;
                elemType = propType.GetElementType();
                isGeneric = propType.IsGenericType;

                if (p.GetSetMethod() == null)
                {
                    continue;
                }

                if (isGeneric && propType.GetGenericTypeDefinition() == typeof(Primitive<>))
                {
                    genType = propType.GetGenericArguments()[0];
                    val = DeserializePrimitive(p.Name, genType, o, flags);
                }
                else if (isArray)
                {
                    if (PrimitiveTypeUtils.IsPrimitiveType(elemType))
                    {
                        val = DeserializeValueArray(p.Name, elemType, o, flags);
                    }
                    else
                    {
                        val = DeserializeStructureArray(p.Name, elemType, o, flags);
                    }
                }
                else if (PrimitiveTypeUtils.IsPrimitiveType(propType))
                {
                    val = DeserializeValue(p.Name, propType, o, flags);
                }
                else
                {
                    val = DeserializeStructure(p.Name, propType, o, flags);
                }

                if (val != null)
                {
                    p.SetValue(o, val);
                }
            }

            return o;
        }

        private object DeserializeToFields(Type t, object o, DeserializationFlags flags, BindingFlags bindFlags)
        {
            // TODO: there /has/ to be some way to generalize this process
            //       for both fields and properties...

            FieldInfo[] allFields;
            Type fieldType;
            bool isArray;
            Type elemType;
            bool isGeneric;
            Type genType;
            object val;

            if (!flags.HasFlag(DeserializationFlags.Fields))
            {
                return o;
            }

            allFields = t.GetFields(bindFlags);

            foreach (FieldInfo f in allFields)
            {
                fieldType = f.FieldType;
                isArray = fieldType.IsArray;
                elemType = fieldType.GetElementType();
                isGeneric = fieldType.IsGenericType;

                if (isGeneric && fieldType.GetGenericTypeDefinition() == typeof(Primitive<>))
                {
                    genType = fieldType.GetGenericArguments()[0];
                    val = DeserializePrimitive(f.Name, genType, o, flags);
                }
                else if (isArray)
                {
                    if (PrimitiveTypeUtils.IsPrimitiveType(elemType))
                    {
                        val = DeserializeValueArray(f.Name, elemType, o, flags);
                    }
                    else
                    {
                        val = DeserializeStructureArray(f.Name, elemType, o, flags);
                    }
                }
                else if (PrimitiveTypeUtils.IsPrimitiveType(fieldType))
                {
                    val = DeserializeValue(f.Name, fieldType, o, flags);
                }
                else
                {
                    val = DeserializeStructure(f.Name, fieldType, o, flags);
                }

                if (val != null)
                {
                    f.SetValue(o, val);
                }
            }

            return o;
        }

        private object DeserializePrimitive(string name, Type t, object o, DeserializationFlags flags)
        {
            bool ignoreCase = flags.HasFlag(DeserializationFlags.IgnoreCase);
            object prim = GetPrimitive(t, name, ignoreCase);

            return prim;
        }

        private object DeserializeValue(string name, Type t, object o, DeserializationFlags flags)
        {
            bool ignoreCase = flags.HasFlag(DeserializationFlags.IgnoreCase);
            object prim = GetPrimitive(t, name, ignoreCase);
            if (prim == null)
            {
                return null;
            }

            PropertyInfo valueProp = prim.GetType().GetProperty(nameof(Primitive<byte>.Value));
            return valueProp.GetValue(prim, null);
        }

        private object DeserializeValueArray(string name, Type t, object o, DeserializationFlags flags)
        {
            bool ignoreCase = flags.HasFlag(DeserializationFlags.IgnoreCase);
            object prim = GetPrimitive(t, name, ignoreCase);
            if (prim == null)
            {
                return null;
            }

            PropertyInfo elemCountProp = prim.GetType().GetProperty(nameof(Primitive<byte>.ElementCount));
            PropertyInfo valueProp = prim.GetType().GetProperty(nameof(Primitive<byte>.Value));
            MethodInfo indexerMeth = prim.GetType().GetMethod("get_Item");   // 'this[int i]' property

            int elemCount = (int) elemCountProp.GetValue(prim, null);
            Array arr = Array.CreateInstance(t, elemCount);
            for (int i = 0; i < elemCount; i++)
            {
                object elem = indexerMeth.Invoke(prim, new object[] { i });
                object val = valueProp.GetValue(elem);
                arr.SetValue(val, i);
            }

            return arr;
        }

        private object DeserializeStructure(string name, Type t, object o, DeserializationFlags flags)
        {
            bool ignoreCase = flags.HasFlag(DeserializationFlags.IgnoreCase);
            Structure s = GetStructure(name, ignoreCase);
            if (s == null)
            {
                return null;
            }

            return s.Deserialize(t, flags);
        }

        private object DeserializeStructureArray(string name, Type t, object o, DeserializationFlags flags)
        {
            bool ignoreCase = flags.HasFlag(DeserializationFlags.IgnoreCase);
            Structure s = GetStructure(name, ignoreCase);
            if (s == null)
            {
                return null;
            }

            Array arr = Array.CreateInstance(t, s.ElementCount);
            for (int i = 0; i < s.ElementCount; i++)
            {
                object elem = s[i].Deserialize(t, flags);
                arr.SetValue(elem, i);
            }

            return arr;
        }

        /// <summary>
        /// Gets a <see cref="Structure"/> by searching this structure's
        /// symbol table for the specified name. If no match is found,
        /// <c>null</c> is returned.
        /// </summary>
        /// <param name="name">The name of the <see cref="Structure"/> to search for.</param>
        /// <returns>The <see cref="Structure"/> object, if found. <c>null</c> otherwise.</returns>
        public Structure GetStructure(string name)
        {
            return GetStructure(name, false);
        }

        private Structure GetStructure(string name, bool ignoreCase)
        {
            bool exists = Symbol.TryLookup(name, ignoreCase, out SymbolTable sym);
            if (!exists || !sym.IsStruct)
            {
                return null;
            }

            return new Structure(DataSource, sym);
        }

        /// <summary>
        /// Gets a <see cref="Primitive{T}"/> by searching this structure's
        /// symbol table for the specified name. If no match is found,
        /// <c>null</c> is returned.
        /// </summary>
        /// <typeparam name="T">The type of primitive to get.</typeparam>
        /// <param name="name">The name of the <see cref="Primitive{T}"/> to search for.</param>
        /// <returns>The <see cref="Primitive{T}"/> object, if found. <c>null</c> otherwise.</returns>
        public Primitive<T> GetPrimitive<T>(string name)
            where T : struct
        {
            return GetPrimitive<T>(name, false);
        }

        private Primitive<T> GetPrimitive<T>(string name, bool ignoreCase)
            where T : struct
        {
            bool exists = Symbol.TryLookup(name, ignoreCase, out SymbolTable sym);
            if (!exists || sym.IsStruct)
            {
                return null;
            }

            if (sym.DataType != typeof(T))
            {
                return null;
            }

            return new Primitive<T>(DataSource, sym);
        }

        private object GetPrimitive(Type t, string name, bool ignoreCase)
        {
            string mName = nameof(GetPrimitive);
            Type[] sig = new Type[] { typeof(string), typeof(bool) };
            BindingFlags bFlags = BindingFlags.NonPublic | BindingFlags.Instance;

            MethodInfo m = this.GetType()
                .GetMethod(mName, bFlags, null, sig, null);
            m = m.MakeGenericMethod(t);

            return m.Invoke(this, new object[] { name, ignoreCase });
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

            return new Structure(DataSource, Symbol[index]);
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
            o.Add(nameof(Symbol.DataType), nameof(Structure));
            o.Add(nameof(GlobalOffset), GlobalOffset);
            o.Add(nameof(LocalOffset), LocalOffset);
            o.Add(nameof(Length), Length);

            if (IsCollection)
            {
                o.Add(nameof(ElementCount), ElementCount);
            }

            JArray a = new JArray();
            object obj;
            foreach (SymbolTable member in Symbol.GetAllMembers())
            {
                if (member.IsStruct)
                {
                    obj = GetStructure(member.Name);
                }
                else
                {
                    MethodInfo getPrimitive = GetType()
                        .GetMethod(nameof(GetPrimitive))
                        .MakeGenericMethod(member.DataType);
                    obj = getPrimitive.Invoke(this, new object[] { member.Name });
                }
                a.Add(JToken.Parse(obj.ToString()));
            }
            o.Add("Members", a);
            return o.ToString(Formatting.None);

        }
    }
}
