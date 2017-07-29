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
using System.Runtime.InteropServices;
using System.IO;
using WHampson.Cascara.Types;
using System.Text.RegularExpressions;
using System.Reflection;

using Double = WHampson.Cascara.Types.Double;
using Int16 = WHampson.Cascara.Types.Int16;
using Int32 = WHampson.Cascara.Types.Int32;
using Int64 = WHampson.Cascara.Types.Int64;
using UInt16 = WHampson.Cascara.Types.UInt16;
using UInt32 = WHampson.Cascara.Types.UInt32;
using UInt64 = WHampson.Cascara.Types.UInt64;
using Pointer = WHampson.Cascara.Types.Pointer;

namespace WHampson.Cascara
{
    /// <summary>
    /// Represents a non-text file.
    /// </summary>
    public class BinaryFile : IDisposable
    {
        private IntPtr dataPtr;
        private int dataLen;

        private SymbolTable symTabl;

        private bool hasBeenDisposed;

        private BinaryFile(IntPtr addr, int len)
        {
            dataPtr = addr;
            dataLen = len;
            symTabl = new SymbolTable();

            hasBeenDisposed = false;
        }

        /// <summary>
        /// Gets the number of bytes in the binary file.
        /// </summary>
        public int Length
        {
            get
            {
                if (hasBeenDisposed)
                {
                    throw new ObjectDisposedException(GetType().FullName);
                }

                return dataLen;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this file is no longer available for use.
        /// </summary>
        public bool IsClosed
        {
            get { return hasBeenDisposed; }
        }

        /// <summary>
        /// Gets a value indicating whether the given variable identifier
        /// exists in the file's symbol table.
        /// </summary>
        /// <param name="name">
        /// The identifier to check for.
        /// </param>
        /// <returns>
        /// <code>True</code> if <paramref name="name"/> is a valid identitifer,
        /// <code>False</code> otherwise.
        /// </returns>
        public bool IdentifierExists(string name)
        {
            return symTabl.GetEntry(name) != null;
        }

        /// <summary>
        /// Gets a value indicating whether the given variable identifier
        /// represents a primitive type (i.e. non-struct).
        /// </summary>
        /// <param name="name">
        /// The identifier to check for.
        /// </param>
        /// <returns>
        /// <code>True</code> if <paramref name="name"/> represents a primitive type
        /// <code>False</code> otherwise.
        /// </returns>
        public bool IsPrimitive(string name)
        {
            SymbolInfo info = symTabl.GetEntry(name);

            return info != null && info.TypeInfo.Type != typeof(BftStruct);
        }

        /// <summary>
        /// Gets a typeless pointer that points to a position in the binary file.
        /// </summary>
        /// <param name="offset">
        /// The position in the binary file data to point to,
        /// measured as the number of bytes from the start of
        /// the file.
        /// </param>
        /// <returns>
        /// A typeless pointer to the desired offset.
        /// </returns>
        public Pointer GetPointer(int offset)
        {
            if (!RangeCheck(offset))
            {
                throw new ArgumentOutOfRangeException("offset");
            }

            return new Pointer(dataPtr + offset);
        }

        /// <summary>
        /// Gets a pointer to an <see cref="ICascaraType"/> at the given position
        /// in the binary file.
        /// </summary>
        /// <typeparam name="T">
        /// The type to get a pointer to.
        /// </typeparam>
        /// <param name="offset">
        /// The position in the binary file data to point to,
        /// measured as the number of bytes from the start of
        /// the file.
        /// </param>
        /// <returns>
        /// A pointer to a type <see cref="T"/> at the desired offset.
        /// </returns>
        public Pointer<T> GetPointer<T>(int offset)
            where T : struct, ICascaraType
        {
            if (!RangeCheck(offset))
            {
                throw new ArgumentOutOfRangeException("offset");
            }

            return new Pointer<T>(dataPtr + offset);
        }

        public Pointer GetPointer(string name)
        {
            SymbolInfo info = symTabl.GetEntry(name);
            if (info == null)
            {
                string fmt = "Symbol not found '{0}'";
                throw new ArgumentException(string.Format(fmt, name));
            }

            return new Pointer(dataPtr + info.Offset);
        }

        public Pointer<T> GetPointer<T>(string name)
            where T : struct, ICascaraType
        {
            SymbolInfo info = symTabl.GetEntry(name);
            if (info == null)
            {
                string fmt = "Symbol not found '{0}'";
                throw new ArgumentException(string.Format(fmt, name));
            }

            if (info.TypeInfo.Type == typeof(BftStruct))
            {
                string fmt = "Cannot get a pointer to struct '{0}' using type '{1}'.";
                throw new NotSupportedException(string.Format(fmt, name, typeof(T).Name));
            }

            return new Pointer<T>(dataPtr + info.Offset);
        }

        public T GetValue<T>(int offset)
            where T : struct, ICascaraType
        {
            Pointer<T> pValue = GetPointer<T>(offset);

            return pValue.Value;
        }

        public T GetValue<T>(string name)
            where T : struct, ICascaraType
        {
            Pointer<T> pValue = GetPointer<T>(name);

            return pValue.Value;
        }

        public void SetValue<T>(int offset, T value)
            where T : struct, ICascaraType
        {
            Pointer<T> pValue = GetPointer<T>(offset);
            pValue.Value = value;
        }

        public void SetValue<T>(string name, T value)
            where T : struct, ICascaraType
        {
            Pointer<T> pValue = GetPointer<T>(name);
            pValue.Value = value;
        }

        public T Extract<T>() where T : new()
        {
            //PrintSymbols(symTabl);

            return (T) Extract(symTabl, typeof(T));
            //return (T) new object();
        }

        private object Extract(SymbolTable tabl, Type t)
        {
            object o = Activator.CreateInstance(t);
            PropertyInfo[] props = t.GetProperties();
            foreach (PropertyInfo p in props)
            {
                SymbolInfo sInfo = tabl.GetEntry(p.Name);
                if (sInfo == null)
                {
                    continue;
                }

                bool isCascaraPrimitive = p.GetValue(o) is ICascaraType;
                bool isCascaraPointer = typeof(ICascaraPointer).IsAssignableFrom(p.PropertyType);
                if (isCascaraPrimitive || p.PropertyType.IsPrimitive)
                {
                    SetPrimitiveValue(p, o, sInfo);
                }
                else if (isCascaraPointer)
                {
                    SetPointerValue(p, o, sInfo);
                }
                else if (sInfo.TypeInfo.Type == typeof(BftStruct) && sInfo.Child != null)
                {
                    if (p.PropertyType.IsArray)
                    {
                        SetArrayValues(p, o, tabl);
                        continue;
                    }

                    object memb = Extract(sInfo.Child, p.PropertyType);
                    p.SetValue(o, memb);
                }
                
                // TODO: handle arrays
            }

            return o;
        }

        private void SetArrayValues(PropertyInfo p, object o, SymbolTable tabl)
        {
            Type propType = p.PropertyType;
            if (propType.IsGenericType)
            {
                Type propGenType = propType.GetElementType().GetGenericTypeDefinition();
                Type ptrType = typeof(Pointer<>);
                if (propGenType == ptrType)
                {
                    // TODO: better exception message
                    throw new InvalidCastException(p.Name + " is Pointer<> array!");
                }
            }

            Type elemType = propType.GetElementType();
            int elemCount = CountElems(p.Name, tabl);
            Array a = Array.CreateInstance(elemType, elemCount);
            for (int i = 0; i < elemCount; i++)
            {
                string elemName = string.Format("{0}[{1}]", p.Name, i);
                SymbolInfo sInfo = tabl.GetEntry(elemName);
                // TODO: watch out for sInfo == null
                object memb = Extract(sInfo.Child, elemType);
                a.SetValue(memb, i);
            }

            p.SetValue(o, a);
        }

        public int CountElems(string name)
        {
            return CountElems(name, symTabl);
        }

        private int CountElems(string name, SymbolTable tabl)
        {
            name = Regex.Replace(name, @"\[\d+\]$", "");
            int count = 0;
            do
            {
                string elemName = name + string.Format("[{0}]", count);
                SymbolInfo sInfo = tabl.GetEntry(elemName);
                if (sInfo == null) break;
                count++;
            } while (true);

            return count;
        }

        private void SetPrimitiveValue(PropertyInfo p, object o, SymbolInfo sInfo)
        {
            object val = GetValue(sInfo.TypeInfo.Type, sInfo.Offset);
            p.SetValue(o, Convert.ChangeType(val, p.PropertyType));
        }

        private void SetPointerValue(PropertyInfo p, object o, SymbolInfo sInfo)
        {
            Type propType = p.PropertyType;
            if (propType.IsGenericType)
            {
                Type propGenType = propType.GetGenericTypeDefinition();
                Type arrayPtrType = typeof(ArrayPointer<>);
                if (propGenType == arrayPtrType)
                {
                    int elemCount = CountElems(p.Name);
                    object ptrVal = Activator.CreateInstance(propType, dataPtr + sInfo.Offset, elemCount);
                    p.SetValue(o, ptrVal);
                    return;
                }
            }

            Pointer ptr = GetPointer(sInfo.Offset);
            Type ptrGeneric = typeof(Pointer<>).MakeGenericType(new Type[] { sInfo.TypeInfo.Type });

            p.SetValue(o, Convert.ChangeType(ptr, ptrGeneric));
        }

        public object GetValue(Type valType, int offset)
        {
            Type ptrGeneric = typeof(Pointer<>).MakeGenericType(new Type[] { valType });
            object ptr = Activator.CreateInstance(ptrGeneric, dataPtr + offset);
            PropertyInfo valProp = ptr.GetType().GetProperty("Value");

            return valProp.GetValue(ptr);
        }

        private void PrintSymbols(SymbolTable tabl)
        {
            string baseName = (string.IsNullOrWhiteSpace(tabl.FullyQualifiedName))
                ? ""
                : tabl.FullyQualifiedName + ".";
            foreach (var entry in tabl.entries)
            {
                Console.WriteLine(baseName + entry.Key);
                SymbolInfo sInfo = entry.Value;
                if (sInfo.TypeInfo.Type == typeof(BftStruct))
                {
                    PrintSymbols(sInfo.Child);
                }
            }
        }

        public void ApplyTemplate(string templateFilePath)
        {
            TemplateProcessor proc = new TemplateProcessor(templateFilePath);
            symTabl = proc.Process(dataPtr, dataLen);

            //Console.WriteLine(symTabl);
        }

        /// <summary>
        /// Store the current state of the file data at the specified location.
        /// </summary>
        /// <param name="filePath">
        /// The path to write the file data.
        /// </param>
        public void Write(string filePath)
        {
            if (hasBeenDisposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            byte[] data = new byte[dataLen];
            Marshal.Copy(dataPtr, data, 0, dataLen);

            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                fs.Write(data, 0, dataLen);
            }
        }

        /// <summary>
        /// Closes out the file by releasing all umanaged resources.
        /// </summary>
        public void Close()
        {
            if (hasBeenDisposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            Dispose();
        }

        private bool RangeCheck(int offset)
        {
            return offset > -1 && offset < dataLen;
        }

        //public Pointer GetPointer(Type t, string name)
        //{
        //    if (t == typeof(Bool8))
        //    {
        //        return (Pointer) GetPointer<Bool8>(name);
        //    }
        //    else if(t == typeof(Bool16))
        //    {
        //        return (Pointer) GetPointer<Bool8>(name);
        //    }
        //    else if (t == typeof(Bool32))
        //    {
        //        return (Pointer) GetPointer<Bool8>(name);
        //    }
        //    else if (t == typeof(Bool64))
        //    {
        //        return (Pointer) GetPointer<Bool8>(name);
        //    }
        //    else if (t == typeof(Char8))
        //    {
        //        return (Pointer) GetPointer<Bool8>(name);
        //    }
        //    else if (t == typeof(Char16))
        //    {
        //        return (Pointer) GetPointer<Bool8>(name);
        //    }
        //    else if (t == typeof(Double))
        //    {
        //        return (Pointer) GetPointer<Bool8>(name);
        //    }
        //    else if (t == typeof(Float))
        //    {
        //        return (Pointer) GetPointer<Bool8>(name);
        //    }
        //    else if (t == typeof(Int8))
        //    {
        //        return (Pointer) GetPointer<Bool8>(name);
        //    }
        //    else if (t == typeof(Int16))
        //    {
        //        return (Pointer) GetPointer<Bool8>(name);
        //    }
        //    else if (t == typeof(Int32))
        //    {
        //        return (Pointer) GetPointer<Bool8>(name);
        //    }
        //    else if (t == typeof(Int64))
        //    {
        //        return (Pointer) GetPointer<Bool8>(name);
        //    }
        //    else if (t == typeof(UInt8))
        //    {
        //        return (Pointer) GetPointer<Bool8>(name);
        //    }
        //    else if (t == typeof(UInt16))
        //    {
        //        return (Pointer) GetPointer<Bool8>(name);
        //    }
        //    else if (t == typeof(UInt32))
        //    {
        //        return (Pointer) GetPointer<Bool8>(name);
        //    }
        //    else if (t == typeof(UInt64))
        //    {
        //        return (Pointer) GetPointer<Bool8>(name);
        //    }
        //    else
        //    {
        //        throw new InvalidOperationException();
        //    }
        //}

        #region Disposal
        protected virtual void Dispose(bool disposing)
        {
            if (!hasBeenDisposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // Free unmanaged resources (unmanaged objects)
                if (dataPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(dataPtr);
                    dataPtr = IntPtr.Zero;
                }

                hasBeenDisposed = true;
            }
        }

        ~BinaryFile()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        public static BinaryFile Open(string filePath)
        {
            const int OneGibiByte = 1 << 30;

            FileInfo fInfo = new FileInfo(filePath);
            if (fInfo.Length > OneGibiByte)
            {
                throw new IOException("File size must be less than 1 GiB.");
            }

            int len = (int) fInfo.Length;
            byte[] data = new byte[len];

            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                fs.Read(data, 0, len);
            }

            IntPtr dataPtr = Marshal.AllocHGlobal(len);
            Marshal.Copy(data, 0, dataPtr, len);

            return new BinaryFile(dataPtr, len);
        }
    }
}
