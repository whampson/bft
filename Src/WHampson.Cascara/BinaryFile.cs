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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using WHampson.Cascara.Types;

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
        /// <code>True</code> if <paramref name="name"/> is a valid identifier,
        /// <code>False</code> otherwise.
        /// </returns>
        public bool IdentifierExists(string name)
        {
            return symTabl.GetEntry(name) != null;
        }

        public bool IsArray(string name)
        {
            return symTabl.IsArray(name);
        }

        public bool IsPrimitive(string name)
        {
            return symTabl.IsPrimitive(name);
        }

        public bool IsStruct(string name)
        {
            return symTabl.IsStruct(name);
        }

        public int GetElemCount(string name)
        {
            return symTabl.GetElemCount(name);
        }

        public void ApplyTemplate(string templateFilePath)
        {
            TemplateProcessor proc = new TemplateProcessor(templateFilePath);
            symTabl = proc.Process(dataPtr, dataLen);
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
            where T : struct
        {
            if (!RangeCheck(offset))
            {
                throw new ArgumentOutOfRangeException("offset");
            }

            return new Pointer<T>(dataPtr + offset);
        }

        public Pointer GetPointer(string name)
        {
            SymbolTableEntry entry = symTabl.GetEntry(name);
            if (entry == null)
            {
                string fmt = "Symbol not found '{0}'";
                throw new ArgumentException(string.Format(fmt, name));
            }

            return new Pointer(dataPtr + entry.TypeInfo.Offset);
        }

        public Pointer<T> GetPointer<T>(string name)
            where T : struct
        {
            SymbolTableEntry entry = symTabl.GetEntry(name);
            if (entry == null)
            {
                string fmt = "Symbol not found '{0}'";
                throw new ArgumentException(string.Format(fmt, name));
            }

            if (entry.TypeInfo.IsStruct)
            {
                string fmt = "Cannot get a pointer to struct '{0}' using type '{1}'.";
                throw new NotSupportedException(string.Format(fmt, name, typeof(T).Name));
            }

            return new Pointer<T>(dataPtr + entry.TypeInfo.Offset);
        }

        public ArrayPointer<T> GetArrayPointer<T>(int offset, int count)
            where T : struct
        {
            if (!RangeCheck(offset))
            {
                throw new ArgumentOutOfRangeException("offset");
            }

            return new ArrayPointer<T>(dataPtr + offset, count);
        }

        public T GetValue<T>(int offset)
            where T : struct
        {
            Pointer<T> pValue = GetPointer<T>(offset);

            return pValue.Value;
        }

        public T GetValue<T>(string name)
            where T : struct
        {
            Pointer<T> pValue = GetPointer<T>(name);

            return pValue.Value;
        }

        public void SetValue<T>(int offset, T value)
            where T : struct
        {
            Pointer<T> pValue = GetPointer<T>(offset);
            pValue.Value = value;
        }

        public void SetValue<T>(string name, T value)
            where T : struct
        {
            Pointer<T> pValue = GetPointer<T>(name);
            pValue.Value = value;
        }

        public Dictionary<string, TypeInfo> GetAll()
        {
            Dictionary<string, TypeInfo> allEntries = symTabl.GetAllEntries()
                .ToDictionary(k => k.Key, v => v.Value.TypeInfo);

            return allEntries;
        }

        public Dictionary<string, TypeInfo> GetAllPrimitives()
        {
            Dictionary<string, TypeInfo> primitives = symTabl.GetAllEntries()
                .Where(x => !x.Value.TypeInfo.IsStruct)
                .ToDictionary(k => k.Key, v => v.Value.TypeInfo);

            return primitives;
        }

        public Dictionary<string, TypeInfo> GetAllStructs()
        {
            Dictionary<string, TypeInfo> structs = symTabl.GetAllEntries()
                .Where(x => x.Value.TypeInfo.IsStruct)
                .ToDictionary(k => k.Key, v => v.Value.TypeInfo);

            return structs;
        }

        //public Dictionary<string, TypeInfo> GetAllMembers(string name)
        //{
        //    Dictionary<string, TypeInfo> allEntries = GetAll();
        //}

        public T Extract<T>() where T : new()
        {
            return (T) Extract(symTabl, typeof(T));
        }

        private object Extract(SymbolTable tabl, Type t)
        {
            object o = Activator.CreateInstance(t);
            PropertyInfo[] props = t.GetProperties();

            foreach (PropertyInfo p in props)
            {
                SymbolTableEntry sInfo = tabl.GetEntry(p.Name);
                if (sInfo == null)
                {
                    continue;
                }

                //bool isCascaraPrimitive = typeof(ICascaraType).IsAssignableFrom(p.PropertyType);
                bool isCascaraPointer = typeof(ICascaraPointer).IsAssignableFrom(p.PropertyType);
                bool isStruct = sInfo.TypeInfo.IsStruct;
                bool isArrayPointer = IsPropertyArrayPointer(p);

                if (p.PropertyType.IsPrimitive)
                {
                    SetPrimitiveValue(p, o, sInfo);
                }
                else if (isCascaraPointer)
                {
                    if (isArrayPointer)
                    {
                        int elemCount = tabl.GetElemCount(p.Name);
                        SetArrayPointerValue(p, o, sInfo, elemCount);
                    }
                    else
                    {
                        SetPointerValue(p, o, sInfo);
                    }
                }
                else if (isStruct)
                {
                    if (p.PropertyType.IsArray)
                    {
                        SetStructArrayElements(p, o, tabl);
                    }
                    else
                    {
                        object memb = Extract(sInfo.Child, p.PropertyType);
                        p.SetValue(o, memb);
                    }
                }
                else
                {
                    // temp
                    throw new InvalidOperationException("Operation not allowed.");
                }
            }

            return o;
        }

        private void SetStructArrayElements(PropertyInfo p, object o, SymbolTable tabl)
        {
            Type propType = p.PropertyType;
            Type elemType = propType.GetElementType();

            //// Check if property is a Pointer<T>[], which is not allowed
            //if (propType.IsGenericType)
            //{
            //    Type propGenType = elemType.GetGenericTypeDefinition();
            //    if (propGenType == typeof(Pointer<>))
            //    {
            //        // TODO: better exception message
            //        string msg = "Arrays of the Pointer<> type are not allowed. "
            //            + "Use the ArrayPointer<> class to create a pointer to an array.";
            //        throw new InvalidCastException(msg);
            //    }
            //}

            int elemCount = tabl.GetElemCount(p.Name);
            Array a = Array.CreateInstance(elemType, elemCount);
            for (int i = 0; i < elemCount; i++)
            {
                string elemName = string.Format("{0}[{1}]", p.Name, i);
                SymbolTableEntry sInfo = tabl.GetEntry(elemName);
                // TODO: watch out for sInfo == null
                object memb = Extract(sInfo.Child, elemType);
                a.SetValue(memb, i);
            }

            p.SetValue(o, a);
        }

        private void SetPrimitiveValue(PropertyInfo p, object o, SymbolTableEntry sInfo)
        {
            object val = GetValue(p.PropertyType, sInfo.TypeInfo.Offset);
            p.SetValue(o, Convert.ChangeType(val, p.PropertyType));
        }

        private void SetPointerValue(PropertyInfo p, object o, SymbolTableEntry sInfo)
        {
            Pointer ptr = GetPointer(sInfo.TypeInfo.Offset);

            p.SetValue(o, Convert.ChangeType(ptr, p.PropertyType));
        }

        private void SetArrayPointerValue(PropertyInfo p, object o, SymbolTableEntry sInfo, int elemCount)
        {
            object ptrVal = Activator.CreateInstance(p.PropertyType, dataPtr + sInfo.TypeInfo.Offset, elemCount);
            p.SetValue(o, ptrVal);
        }

        public object GetValue(Type valType, int offset)
        {
            //Type ptrGeneric = typeof(Pointer<>).MakeGenericType(new Type[] { valType });
            //object ptr = Activator.CreateInstance(ptrGeneric, dataPtr + offset);
            //PropertyInfo valProp = ptr.GetType().GetProperty("Value");

            //return valProp.GetValue(ptr);

            Pointer ptr = dataPtr + offset;
            return ptr.GetValue(valType);
        }

        public object GetValue(string name, Type valType)
        {
            Pointer ptr = GetPointer(name);
            return ptr.GetValue(valType);
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
        /// Closes out the file by releasing all unmanaged resources.
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

        public static bool IsPropertyArrayPointer(PropertyInfo p)
        {
            Type propType = p.PropertyType;
            if (propType.IsGenericType)
            {
                Type propGenType = propType.GetGenericTypeDefinition();
                if (propGenType == typeof(ArrayPointer<>))
                {
                    return true;
                }
            }

            return false;
        }

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
