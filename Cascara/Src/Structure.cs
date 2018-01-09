
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using WHampson.Cascara.Interpreter;

namespace WHampson.Cascara
{
    /// <summary>
    /// Represents a composite data type found in a <see cref="BinaryFile"/>.
    /// </summary>
    public class Structure : IFileObject
    {
        private BinaryFile sourceFile;
        private Symbol symbol;

        internal Structure(BinaryFile sourceFile, Symbol symbol)
        {
            this.sourceFile = sourceFile;
            this.symbol = symbol;
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
        /// of the <see cref="BinaryFile"/>.
        /// </summary>
        public int FilePosition
        {
            get { return symbol.DataOffset; }
        }

        /// <summary>
        /// Gets the position of this <see cref="IFileObject"/> relative to the start
        /// of the parent object.
        /// </summary>
        public int Offset
        {
            get
            {
                if (symbol.Parent != null)
                {
                    return symbol.DataOffset - symbol.Parent.DataOffset;
                }

                return symbol.DataOffset;
            }
        }

        /// <summary>
        /// Gets the number of bytes that make up this <see cref="IFileObject"/>.
        /// </summary>
        public int Length
        {
            get { return symbol.DataLength; }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IFileObject"/> represents a collection.
        /// </summary>
        public bool IsCollection
        {
            get { return symbol.IsCollection; }
        }

        /// <summary>
        /// Gets the number of elements in the collection represented by this <see cref="IFileObject"/>.
        /// If this <see cref="IFileObject"/> does not represent a collection, this value is -1.
        /// </summary>
        /// <seealso cref="IsCollection"/>
        public int ElementCount
        {
            get { return symbol.ElementCount; }
        }

        //public int SizeOf(string name)
        //{
        //    bool exists = symbol.TryLookup(name, out Symbol sym);
        //    if (!exists)
        //    {
        //        return 0;
        //    }

        //    return sym.DataLength;
        //}

        //public Type TypeOf(string name)
        //{
        //    bool exists = symbol.TryLookup(name, out Symbol sym);
        //    if (!exists)
        //    {
        //        return null;
        //    }

        //    return sym.DataType;
        //}

        //public int OffsetOf(string name)
        //{
        //    bool exists = symbol.TryLookup(name, out Symbol sym);
        //    if (!exists)
        //    {
        //        return -1;
        //    }

        //    return sym.DataOffset;
        //}

        /// <summary>
        /// Gets a <see cref="Structure"/> by searching this structure's
        /// symbol table for the specified name. If no match is found,
        /// <c>null</c> is returned.
        /// </summary>
        /// <param name="name">The name of the <see cref="Structure"/> to search for.</param>
        /// <returns>The <see cref="Structure"/> object, if found. <c>null</c> otherwise</returns>
        public Structure GetStructure(string name)
        {
            bool exists = symbol.TryLookup(name, out Symbol sym);
            if (!exists)
            {
                return null;
            }

            // TODO: ensure symbol referrs to a structure (not a primitive)

            return new Structure(sourceFile, sym);
        }

        /// <summary>
        /// Gets a <see cref="Primitive{T}"/> by searching this structure's
        /// symbol table for the specified name. If no match is found,
        /// <c>null</c> is returned.
        /// </summary>
        /// <typeparam name="T">The type of primitive to get.</typeparam>
        /// <param name="name">The name of the <see cref="Primitive{T}"/> to search for.</param>
        /// <returns>The <see cref="Primitive{T}"/> object, if found. <c>null</c> otherwise</returns>
        public Primitive<T> GetPrimitive<T>(string name)
            where T : struct
        {
            bool exists = symbol.TryLookup(name, out Symbol sym);
            if (!exists)
            {
                return null;
            }

            // TODO: ensure symbol referrs to a primitive (not a structure)

            return new Primitive<T>(sourceFile, sym);
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

            return new Structure(sourceFile, symbol[index]);
        }

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

        public T Deserialize<T>()
        {
            throw new NotImplementedException();
        }

        //public Structure AddStructure(string name, int offset, int length)
        //{
        //    Symbol sym = symbol.Insert(name);
        //    sym.DataOffset = offset;
        //    sym.DataLength = length;

        //    return (Structure) GetStructure(name);
        //}

        //public Primitive<T> AddPrimitive<T>(string name, int offset)
        //    where T : struct
        //{
        //    Symbol sym = symbol.Insert(name);
        //    sym.DataOffset = offset;
        //    sym.DataLength = PrimitiveTypeUtils.SizeOf<T>();
        //    sym.DataType = typeof(T);

        //    return GetPrimitive<T>(name);
        //}

        //public Primitive<T> AddPrimitive<T>(string name, int offset, int count)
        //    where T : struct
        //{
        //    int typeSize = PrimitiveTypeUtils.SizeOf<T>();

        //    Symbol sym = symbol.Insert(name, count);
        //    sym.DataOffset = offset;
        //    sym.DataLength = typeSize * count;
        //    sym.DataType = typeof(T[]);

        //    int i = 0;
        //    foreach (Symbol elem in sym)
        //    {
        //        elem.DataOffset = sym.DataOffset + (i * typeSize);
        //        elem.DataLength = typeSize;
        //        i++;
        //    }

        //    return GetPrimitive<T>(name);
        //}

        /// <summary>
        /// Converts this <see cref="Structure"/> object to an <see cref="int"/>
        /// whose value equals <see cref="FilePosition"/>.
        /// </summary>
        /// <param name="p">The <see cref="Structure"/> object to convert to an <see cref="int"/>.</param>
        public static implicit operator int(Structure s)
        {
            return s.FilePosition;
        }
    }
}
