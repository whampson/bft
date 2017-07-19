﻿#region License
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
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using WHampson.Bft.Types;

namespace WHampson.Bft
{
    internal sealed class BftStruct
    {
        // Dummy type to classify structs
    }

    internal sealed class TemplateProcessor2
    {
        private static readonly Regex IdentifierRegex = new Regex(@"^[a-zA-Z_][\da-zA-Z_]*$");

        private delegate int DirectiveProcessAction(XElement elem);

        private XDocument templateDoc;

        private IntPtr dataPtr;
        private int dataLen;
        private int dataOffset;

        private Dictionary<string, TypeInfo> typeMap;
        private Dictionary<string, DirectiveProcessAction> directiveActionMap;

        private SymbolTable symbolTable;
        private Stack<SymbolTable> symTablStack;

        private bool isEvalutingTypedef;
        private bool isConductingDryRun;
        private int dryRunRecursionDepth;

        public TemplateProcessor2(XDocument doc)
        {
            templateDoc = doc ?? throw new ArgumentNullException("doc");
            dataPtr = IntPtr.Zero;
            dataLen = 0;
            dataOffset = 0;

            typeMap = new Dictionary<string, TypeInfo>();
            directiveActionMap = new Dictionary<string, DirectiveProcessAction>();

            isEvalutingTypedef = false;
            isConductingDryRun = false;
            dryRunRecursionDepth = 0;

            symbolTable = new SymbolTable();
            symTablStack = new Stack<SymbolTable>();
            symTablStack.Push(symbolTable);

            BuildTypeMap();
            BuildDirectiveActionMap();
        }

        public T Process<T>(string filePath) where T : new()
        {
            T obj;
            Process(filePath, out obj);

            return obj;
        }

        public int Process<T>(string filePath, out T obj) where T : new()
        {
            // Validate root element
            if (templateDoc.Root.Name.LocalName != Keywords.Bft)
            {
                string fmt = "Template must have a root element named '{0}'.";
                throw TemplateException.Create(templateDoc.Root, fmt, Keywords.Bft);
            }
            if (!HasChildren(templateDoc.Root))
            {
                throw new TemplateException("Empty binary file template.");
            }

            isEvalutingTypedef = false;
            isConductingDryRun = false;
            dryRunRecursionDepth = 0;

            // Load binary file
            byte[] data = LoadFile(filePath);
            dataLen = data.Length;

            // Copy file data to unmanaged memory
            dataPtr = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, dataPtr, data.Length);

            // Process the template with respect to the file data
            dataOffset = 0;
            int bytesProcessed = ProcessStructMembers(templateDoc.Root);

            // Free unmanaged memory
            // (This will change in the future depending on how we want to manipulate the file data)
            //Marshal.FreeHGlobal(dataPtr);

            Console.WriteLine("Processed {0} out of {1} bytes.", bytesProcessed, dataLen);
            //Console.WriteLine(symbolTable);

            obj = new T();
            return bytesProcessed;
        }

        private int ProcessStruct(XElement elem)
        {
            EnsureAttributes(elem, Keywords.Comment, Keywords.Count, Keywords.Name);

            // Get attribute values
            int count = GetCountAttribute(elem, false, 1);
            string name = GetNameAttribute(elem, false, null);

            // Process the struct 'count' times
            int localOffset = 0;
            string varName;
            for (int i = 0; i < count; i++)
            {
                varName = name + "[" + i + "]";

                SymbolTableEntry entry = null;
                if (!isConductingDryRun && name != null)
                {
                    // Create new symbol table and make current table its parent
                    SymbolTable curSymTabl = symTablStack.Peek();
                    SymbolTable newSymTabl = new SymbolTable(varName, curSymTabl);

                    // Create symbol table entry for this struct in the current table
                    // Type reamins 'null' because we haven't processed teh struct yet
                    entry = new SymbolTableEntry(null, dataOffset, newSymTabl);
                    if (!curSymTabl.AddEntry(varName, entry))
                    {
                        string fmt = "Variable '{0}' already defined.";
                        throw TemplateException.Create(elem, fmt, name);
                    }

                    // Push new symbol table onto the stack to make it "active"
                    symTablStack.Push(newSymTabl);
                }

                localOffset += ProcessStructMembers(elem);

                if (!isConductingDryRun && name != null)
                {
                    // We've finished processing the struct, so now we can set the type
                    entry.Type = TypeInfo.CreateStruct(elem.Elements(), localOffset);

                    // Make the previous table "active"
                    symTablStack.Pop();
                }
            }

            return localOffset;
        }

        private int ProcessStructMembers(XElement elem)
        {
            // Ensure struct element has child elements
            if (!HasChildren(elem))
            {
                string fmt = "Empty struct.";
                throw TemplateException.Create(elem, fmt);
            }

            // Process child elements
            int localOffset = 0;
            foreach (XElement memberElem in elem.Elements())
            {
                localOffset += ProcessElement(memberElem);
            }

            return localOffset;
        }

        private int ProcessElement(XElement elem)
        {
            string elemName = elem.Name.LocalName;
            int localOffset = 0;

            // Process 'struct'
            if (elemName == Keywords.Struct)
            {
                localOffset += ProcessStruct(elem);
                return localOffset;
            }

            // Ensure element name corresponds to either a primitive type,
            // user-defined type, or directive
            TypeInfo tInfo;
            bool isType = typeMap.TryGetValue(elemName, out tInfo);
            bool isDirective = directiveActionMap.ContainsKey(elemName);
            if (!isDirective && !isType)
            {
                string fmt = "Unknown type or directive '{0}'.";
                throw TemplateException.Create(elem, fmt, elemName);
            }

            // Process directive
            if (isDirective)
            {
                DirectiveProcessAction action = directiveActionMap[elemName];
                return action(elem);
            }

            // Process primitive or user-defined type
            // Ensure element has no child elements (only allowed on 'struct's)
            if (HasChildren(elem))
            {
                string fmt = "Type '{0}' cannot contain child elements.";
                throw TemplateException.Create(elem, fmt, elemName);
            }

            if (tInfo.Type == typeof(BftStruct))
            {
                // Process user-defined type
                XElement structElem = new XElement(elem);
                structElem.Add(typeMap[elemName].Members);
                localOffset += ProcessStruct(structElem);
            }
            else
            {
                // Process primitive
                localOffset += ProcessPrimitive(elem);
            }

            return localOffset;
        }

        private int ProcessPrimitive(XElement elem)
        {
            EnsureAttributes(elem, Keywords.Comment, Keywords.Count, Keywords.Name);

            // Get attribute values
            int count = GetCountAttribute(elem, false, 1);
            string name = GetNameAttribute(elem, false, null);

            string elemName = elem.Name.LocalName;
            TypeInfo t = typeMap[elemName];

            // Process primitive 'count' times
            int localOffset = 0;
            string varName;
            for (int i = 0; i < count; i++)
            {
                EnsureCapacity(elem, t.Size);

                varName = name + "[" + i + "]";
                if (!isConductingDryRun)
                {
                    if (name != null)
                    {
                        // Create symbol table entry for this type
                        // It's not a struct so it doesn't have a child table
                        SymbolTableEntry e = new SymbolTableEntry(t, dataOffset, null);
                        if (!symTablStack.Peek().AddEntry(varName, e))
                        {
                            string fmt = "Variable '{0}' already defined.";
                            throw TemplateException.Create(elem, fmt, name);
                        }
                    }

                    // Increment data pointer
                    dataOffset += t.Size;
                }
                localOffset += t.Size;
            }

            return localOffset;
        }

        private int ProcessAlign(XElement elem)
        {
            EnsureAttributes(elem, Keywords.Comment, Keywords.Count, Keywords.Kind);

            // Get attribute values
            int count = GetCountAttribute(elem, false, 1);
            TypeInfo kind = GetKindAttribute(elem, false, typeMap[Keywords.Int8]);

            // Skip ahead correct number of bytes as defined by 'kind' and 'count'
            int off = kind.Size * count;
            EnsureCapacity(elem, off);

            if (!isConductingDryRun)
            {
                dataOffset += off;
            }
            
            return off;
        }

        private int ProcessEcho(XElement elem)
        {
            if(isEvalutingTypedef)
            {
                return 0;
            }

            if (HasChildren(elem))
            {
                string fmt = "Directive '{0}' cannot contain child elements.";
                throw TemplateException.Create(elem, fmt, Keywords.Echo);
            }

            EnsureAttributes(elem, Keywords.Comment, Keywords.Message);

            string message = GetMessageAttribute(elem, true, null);
            Console.WriteLine(message);         // TODO: allow for custom output streams

            return 0;
        }

        private int ProcessTypedef(XElement elem)
        {
            if (isEvalutingTypedef)
            {
                string fmt = "Nested type definitions are not allowed.";
                throw TemplateException.Create(elem, fmt);
            }
            isEvalutingTypedef = true;

            EnsureAttributes(elem, Keywords.Comment, Keywords.Kind, Keywords.Name);

            TypeInfo kind = GetKindAttribute(elem, true, null);
            string typename = GetNameAttribute(elem, true, null);

            if (typeMap.ContainsKey(typename))
            {
                string fmt = "Type '{0}' has already been defined.";
                throw TemplateException.Create(elem, fmt, typename);
            }
            else if (directiveActionMap.ContainsKey(typename))
            {
                string fmt = "Cannot use reserved word '{0}' as a type name.";
                throw TemplateException.Create(elem, fmt, typename);
            }

            typeMap[typename] = kind;
            isEvalutingTypedef = false;

            return 0;
        }

        private void EnsureAttributes(XElement elem, params string[] validAttributes)
        {
            foreach (XAttribute attr in elem.Attributes())
            {
                string name = attr.Name.LocalName;
                if (!(Keywords.KeywordMap.ContainsKey(name) && validAttributes.Contains(name)))
                {
                    string fmt = "Unknown attribute '{0}'.";
                    throw TemplateException.Create(attr, fmt, name);
                }
            }
        }

        private bool GetAttribute(XElement elem, string name, bool isRequired, out XAttribute attr)
        {
            attr = elem.Attribute(name);
            if (attr == null)
            {
                if (isRequired)
                {
                    string fmt = "Missing required attribute '{0}'.";
                    throw TemplateException.Create(elem, fmt, name);
                }

                return false;
            }

            return true;
        }

        private int GetCountAttribute(XElement elem, bool isRequired, int defaultValue)
        {
            XAttribute countAttr;
            if (!GetAttribute(elem, Keywords.Count, isRequired, out countAttr))
            {
                return defaultValue;
            }

            return ProcessCountAttribute(countAttr);
        }

        private int ProcessCountAttribute(XAttribute attr)
        {
            //long val;
            //bool isInt = NumberUtils.TryParseInteger(attr.Value, out val);
            //if (!isInt || (int) val < 0)
            //{
            //    string fmt = "'{0}' is not a valid integer. Value must be a non-negative number.";
            //    throw TemplateException.Create(attr, fmt, attr.Value);
            //}

            string valStr = ResolveVariables(attr.Value);
            Regex mathExprPattern = new Regex(@"^[-+*/().\d ]+$");

            if (!mathExprPattern.IsMatch(valStr))
            {
                string fmt = "Invalid math expression '{0}'";
                throw TemplateException.Create(attr, fmt, valStr);
            }

            DataTable dt = new DataTable();
            object val = dt.Compute(valStr, "");

            return Convert.ToInt32(val);
        }

        private TypeInfo GetKindAttribute(XElement elem, bool isRequired, TypeInfo defaultValue)
        {
            XAttribute countAttr;
            if (!GetAttribute(elem, Keywords.Kind, isRequired, out countAttr))
            {
                return defaultValue;
            }

            return ProcessKindAttribute(countAttr);
        }

        private TypeInfo ProcessKindAttribute(XAttribute attr)
        {
            string typeName = attr.Value;
            XElement srcElem = attr.Parent;

            // Process 'struct'
            if (typeName == Keywords.Struct)
            {
                if (dryRunRecursionDepth == 0)
                {
                    isConductingDryRun = true;
                }
                dryRunRecursionDepth++;

                int size = ProcessStructMembers(attr.Parent);

                dryRunRecursionDepth--;
                if (dryRunRecursionDepth == 0)
                {
                    isConductingDryRun = false;
                }

                return TypeInfo.CreateStruct(srcElem.Elements(), size);
            }

            // Process primitive or user-defined type
            TypeInfo tInfo;
            if (typeMap.TryGetValue(typeName, out tInfo))
            {
                if (HasChildren(srcElem))
                {
                    string fmt = "Type '{0}' cannot contain child elements.";
                    throw TemplateException.Create(attr, fmt, typeName);
                }

                return typeMap[typeName];
            }
            else
            {
                string fmt = "Unknown type '{0}'.";
                throw TemplateException.Create(attr, fmt, typeName);
            }
        }

        private string GetMessageAttribute(XElement elem, bool isRequired, string defaultValue)
        {
            XAttribute messageAttr;
            if (!GetAttribute(elem, Keywords.Message, isRequired, out messageAttr))
            {
                return defaultValue;
            }

            return ProcessMessageAttribute(messageAttr);
        }

        private string ProcessMessageAttribute(XAttribute attr)
        {
            string msg = ResolveVariables(attr.Value);

            // Handle control chars
            msg = Regex.Replace(msg, @"\\([bnr])", m =>
            {
                string esc = "";
                switch (m.Groups[1].Value[0])
                {
                    case 'b':
                        esc += '\b';
                        break;

                    case 'n':
                        esc += '\n';
                        break;

                    case 'r':
                        esc += '\r';
                        break;
                }

                return esc;
            });

            return msg;
        }

        private string GetNameAttribute(XElement elem, bool isRequired, string defaultValue)
        {
            XAttribute nameAttr;
            if (!GetAttribute(elem, Keywords.Name, isRequired, out nameAttr))
            {
                return defaultValue;
            }

            return ProcessNameAttribute(nameAttr);
        }

        private string ProcessNameAttribute(XAttribute attr)
        {
            if (!IdentifierRegex.IsMatch(attr.Value))
            {
                string fmt = "'{0}' is not a valid identifier. Identifiers may consist only of "
                    + "alphanumeric characters and underscores, and may not begin with a number.";
                throw TemplateException.Create(attr, fmt, attr.Value);
            }

            return attr.Value;
        }

        private string ResolveVariables(string s)
        {
            // Resolve variable values
            s = Regex.Replace(s, @"\${([\[\]\._\da-zA-Z]+)}", m =>
            {
                string varName = m.Groups[1].Value;
                switch (varName)
                {
                    case "__OFFSET__":
                        return dataOffset + "";
                    case "__FILESIZE__":
                        return dataLen + "";
                }

                SymbolTableEntry e = symTablStack.Peek().GetEntry(varName);
                Type ptrTypeGeneric = typeof(Pointer<>);
                Type ptrType = ptrTypeGeneric.MakeGenericType(new Type[] { e.Type.Type });
                object ptr = Activator.CreateInstance(ptrType, dataPtr + e.Offset);
                object val = ptr.GetType().GetProperty("Value").GetValue(ptr);
                return val.ToString();
            });

            // Resolve variable offsets
            s = Regex.Replace(s, @"\$\[([\[\]\._\da-zA-Z]+)\]", m =>
            {
                string varName = m.Groups[1].Value;
                SymbolTableEntry e = symTablStack.Peek().GetEntry(varName);
                return e.Offset + "";
            });

            // Resolve variable sizes
            s = Regex.Replace(s, @"\$\(([\[\]\._\da-zA-Z]+)\)", m =>
            {
                string varName = m.Groups[1].Value;
                SymbolTableEntry e = symTablStack.Peek().GetEntry(varName);
                if (e.Type == null)
                {
                    // TODO: throw exception
                    return "";
                }

                return e.Type.Size + "";
            });

            return s;
        }

        private void EnsureCapacity(XElement elem, int localOffset)
        {
            int absOffset = dataOffset + localOffset;
            if (/*absOffset < 0 || */absOffset > dataLen)
            {
                string fmt = "Reached end of file. Offset: {0}, length: {1}.";
                throw TemplateException.Create(elem, fmt, absOffset, dataLen);
            }
        }

        private bool HasChildren(XElement elem)
        {
            return elem.Elements().Count() != 0;
        }

        private void BuildTypeMap()
        {
            typeMap[Keywords.Float] = TypeInfo.CreatePrimitive(typeof(Float));
            typeMap[Keywords.Int8] = TypeInfo.CreatePrimitive(typeof(Int8));
            typeMap[Keywords.Int32] = TypeInfo.CreatePrimitive(typeof(Types.Int32));
        }

        private void BuildDirectiveActionMap()
        {
            directiveActionMap[Keywords.Align] = ProcessAlign;
            directiveActionMap[Keywords.Echo] = ProcessEcho;
            directiveActionMap[Keywords.Typedef] = ProcessTypedef;
        }

        private static byte[] LoadFile(string filePath)
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

            return data;
        }
    }
}
