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

        public void ApplyTemplate(string templateFilePath)
        {
            TemplateProcessor proc = new TemplateProcessor(templateFilePath);
            symTabl = proc.Process(dataPtr, dataLen);

            Console.WriteLine(symTabl);
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
