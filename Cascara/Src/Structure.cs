
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using WHampson.Cascara.Interpreter;

namespace WHampson.Cascara
{
    public class Structure : IFileObject
    {
        private BinaryFile sourceFile;
        private Symbol symbol;

        internal Structure(BinaryFile sourceFile, Symbol symbol)
        {
            this.sourceFile = sourceFile;
            this.symbol = symbol;
        }

        public Structure this[int index]
        {
            get { return (Structure) ((IFileObject) this).ElementAt(index); }
        }

        public int FilePosition
        {
            get { return symbol.DataOffset; }
        }

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

        public int Length
        {
            get { return symbol.DataLength; }
        }

        public bool IsCollection
        {
            get { return symbol.IsCollection; }
        }

        public int ElementCount
        {
            get { return symbol.ElementCount; }
        }

        public int SizeOf(string name)
        {
            bool exists = symbol.TryLookup(name, out Symbol sym);
            if (!exists)
            {
                return 0;
            }

            return sym.DataLength;
        }

        public Type TypeOf(string name)
        {
            bool exists = symbol.TryLookup(name, out Symbol sym);
            if (!exists)
            {
                return null;
            }

            return sym.DataType;
        }

        public int OffsetOf(string name)
        {
            bool exists = symbol.TryLookup(name, out Symbol sym);
            if (!exists)
            {
                return -1;
            }

            return sym.DataOffset;
        }

        public Structure GetStructure(string name)
        {
            bool exists = symbol.TryLookup(name, out Symbol sym);
            if (!exists)
            {
                return null;
            }

            return new Structure(sourceFile, sym);
        }

        public Primitive<T> GetPrimitive<T>(string name)
            where T : struct
        {
            bool exists = symbol.TryLookup(name, out Symbol sym);
            if (!exists)
            {
                return null;
            }

            return new Primitive<T>(sourceFile, sym);
        }

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

        public Structure AddStructure(string name, int offset, int length)
        {
            Symbol sym = symbol.Insert(name);
            sym.DataOffset = offset;
            sym.DataLength = length;

            return (Structure) GetStructure(name);
        }

        public Primitive<T> AddPrimitive<T>(string name, int offset)
            where T : struct
        {
            Symbol sym = symbol.Insert(name);
            sym.DataOffset = offset;
            sym.DataLength = PrimitiveTypeUtils.SizeOf<T>();
            sym.DataType = typeof(T);

            return GetPrimitive<T>(name);
        }

        public Primitive<T> AddPrimitive<T>(string name, int offset, int count)
            where T : struct
        {
            int typeSize = PrimitiveTypeUtils.SizeOf<T>();

            Symbol sym = symbol.Insert(name, count);
            sym.DataOffset = offset;
            sym.DataLength = typeSize * count;
            sym.DataType = typeof(T[]);

            int i = 0;
            foreach (Symbol elem in sym)
            {
                elem.DataOffset = sym.DataOffset + (i * typeSize);
                elem.DataLength = typeSize;
                i++;
            }

            return GetPrimitive<T>(name);
        }

        public static implicit operator int(Structure s)
        {
            return s.FilePosition;
        }
    }
}
