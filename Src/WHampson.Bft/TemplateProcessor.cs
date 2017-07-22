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

    internal sealed class TemplateProcessor
    {
        private const string IdentifierPattern = @"^[a-zA-Z_][\da-zA-Z_]*$";
        private const string ValueofOpPattern = @"\${(.+?)}";
        private const string OffsetofOpPattern = @"\$\[([\[\]\S]+)\]";
        private const string SizeofOpPattern = @"\$\((.+?)\)";
        private const string TypeOpPattern = @"type[ ]+(.+)";

        private static readonly Type GenericPointerType = typeof(Pointer<>);

        private delegate int DirectiveProcessAction(XElement elem);
        private delegate T AttributeProcessAction<T>(XAttribute attr);

        private XDocument templateDoc;

        private IntPtr dataPtr;
        private int dataLen;
        private int dataOffset;

        private Dictionary<string, TypeInfo> typeMap;
        private Dictionary<string, DirectiveProcessAction> directiveActionMap;
        private Dictionary<string, Delegate> attributeActionMap;
        private Dictionary<string, double> localsMap;

        private SymbolTable symbolTable;
        private Stack<SymbolTable> symTablStack;

        private bool isEvalutingTypedef;
        private bool isConductingDryRun;    // Analyzing a struct and computing size, but not applying to binary data
        private int dryRunRecursionDepth;

        private TextWriter echoWriter;

        public TemplateProcessor(XDocument doc)
        {
            templateDoc = doc ?? throw new ArgumentNullException("doc");

            dataPtr = IntPtr.Zero;
            dataLen = 0;
            dataOffset = 0;

            typeMap = new Dictionary<string, TypeInfo>();
            directiveActionMap = new Dictionary<string, DirectiveProcessAction>();
            attributeActionMap = new Dictionary<string, Delegate>();
            localsMap = new Dictionary<string, double>();

            symbolTable = new SymbolTable();
            symTablStack = new Stack<SymbolTable>();
            symTablStack.Push(symbolTable);

            isEvalutingTypedef = false;
            isConductingDryRun = false;
            dryRunRecursionDepth = 0;

            echoWriter = Console.Out;

            BuildTypeMap();
            BuildDirectiveActionMap();
            BuildAttributeActionMap();
        }

        public void SetEchoWriter(TextWriter w)
        {
            echoWriter = w ?? throw new ArgumentNullException("w");
        }

        public T Process<T>(string filePath) where T : new()
        {
            Process(filePath, out T obj);

            return obj;
        }

        public int Process<T>(string filePath, out T obj) where T : new()
        {
            isEvalutingTypedef = false;
            isConductingDryRun = false;
            dryRunRecursionDepth = 0;

            // Validate root element
            if (templateDoc.Root.Name.LocalName != Keywords.BftRoot)
            {
                string fmt = "Template must have a root element named '{0}'.";
                throw TemplateException.Create(templateDoc.Root, fmt, Keywords.BftRoot);
            }
            if (!HasChildren(templateDoc.Root))
            {
                throw new TemplateException("Empty binary file template.");
            }

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
            bool isType = typeMap.TryGetValue(elemName, out TypeInfo tInfo);
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
                // Process user-defined struct type
                // "Copy and paste" members from type definition into current element
                elem.Add(typeMap[elemName].Members);
                localOffset += ProcessStruct(elem);
            }
            else
            {
                // Process primitive
                localOffset += ProcessPrimitiveType(elem);
            }

            return localOffset;
        }

        private int ProcessStruct(XElement elem)
        {
            EnsureAttributes(elem, Keywords.Comment, Keywords.Count, Keywords.Name);

            // Get attribute values
            int count = GetAttributeValue<int>(elem, Keywords.Count, false, 1);
            string name = GetAttributeValue<string>(elem, Keywords.Name, false, null);

            // Process the struct 'count' times
            int localOffset = 0;
            string varName;
            for (int i = 0; i < count; i++)
            {
                // Tack on array index to var name so it's unique
                varName = name + "[" + i + "]";

                SymbolTableEntry entry = null;
                if (!isConductingDryRun && name != null)
                {
                    if (localsMap.ContainsKey(name))
                    {
                        string fmt = "Variable '{0}' already defined as a local.";
                        throw TemplateException.Create(elem, fmt, name);
                    }

                    // Create new symbol table and make current table its parent
                    SymbolTable curSymTabl = symTablStack.Peek();
                    SymbolTable newSymTabl = new SymbolTable(varName, curSymTabl);

                    // Create symbol table entry for this struct in the current table
                    // Type reamins 'null' because we haven't processed the struct yet
                    entry = new SymbolTableEntry(null, dataOffset, newSymTabl);
                    if (!curSymTabl.AddEntry(varName, entry))
                    {
                        string fmt = "Variable '{0}' already defined.";
                        throw TemplateException.Create(elem, fmt, name);
                    }

                    // Push new symbol table onto the stack to make it "active"
                    symTablStack.Push(newSymTabl);
                }

                // Process the struct
                localOffset += ProcessStructMembers(elem);

                if (!isConductingDryRun && name != null)
                {
                    // We've finished processing the struct, so now we can set the type
                    entry.TypeInfo = TypeInfo.CreateStruct(elem.Elements(), localOffset);

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

        private int ProcessPrimitiveType(XElement elem)
        {
            EnsureAttributes(elem, Keywords.Comment, Keywords.Count, Keywords.Name);

            // Get attribute values
            int count = GetAttributeValue<int>(elem, Keywords.Count, false, 1);
            string name = GetAttributeValue<string>(elem, Keywords.Name, false, null);

            string elemName = elem.Name.LocalName;
            TypeInfo t = typeMap[elemName];

            // Process primitive 'count' times
            int localOffset = 0;
            string varName;
            for (int i = 0; i < count; i++)
            {
                // Make sure we have enough bytes left in the buffer
                EnsureCapacity(elem, t.Size);

                // Tack on array index to var name so it's unique
                varName = name + "[" + i + "]";

                if (!isConductingDryRun)
                {
                    if (name != null)
                    {
                        if (localsMap.ContainsKey(name))
                        {
                            string fmt = "Variable '{0}' already defined as a local.";
                            throw TemplateException.Create(elem, fmt, name);
                        }

                        // Create symbol table entry for this type
                        // It's not a struct so the child symbol table is 'null'
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

        private int ProcessAlignDirective(XElement elem)
        {
            EnsureAttributes(elem, Keywords.Comment, Keywords.Count, Keywords.Kind);

            // Get attribute values
            int count = GetAttributeValue<int>(elem, Keywords.Count, false, 1);
            TypeInfo kind = GetAttributeValue<TypeInfo>(elem, Keywords.Kind, false, typeMap[Keywords.Int8]);

            // Skip ahead correct number of bytes as defined by 'kind' and 'count'
            int off = kind.Size * count;
            EnsureCapacity(elem, off);

            if (!isConductingDryRun)
            {
                dataOffset += off;
            }
            
            return off;
        }

        private int ProcessEchoDirective(XElement elem)
        {
            if (isEvalutingTypedef)
            {
                return 0;
            }
            if (HasChildren(elem))
            {
                string fmt = "Directive '{0}' cannot contain child elements.";
                throw TemplateException.Create(elem, fmt, Keywords.Echo);
            }

            EnsureAttributes(elem, Keywords.Comment, Keywords.Message, Keywords.Newline, Keywords.Raw);

            string message = GetAttributeValue<string>(elem, Keywords.Message, true, null);
            bool hasNewline = GetAttributeValue<bool>(elem, Keywords.Newline, false, true);
            bool isRaw = GetAttributeValue<bool>(elem, Keywords.Raw, false, false);

            XAttribute messageAttr = elem.Attribute(Keywords.Message);

            if (!isRaw)
            {
                try
                {
                    message = ResolveVariables(message);
                    message = ResolveEscapeSequences(message);
                }
                catch (Exception e)
                {
                    if (e is ArithmeticException || e is FormatException || e is OverflowException || e is TemplateException)
                    {
                        throw TemplateException.Create(e, messageAttr, e.Message);
                    }

                    throw;
                }
            }

            if (hasNewline)
            {
                echoWriter.WriteLine(message);
            }
            else
            {
                echoWriter.Write(message);
            }

            return 0;
        }

        private int ProcessLocalDirective(XElement elem)
        {
            if (isEvalutingTypedef)
            {
                return 0;
            }
            if (HasChildren(elem))
            {
                string fmt = "Directive '{0}' cannot contain child elements.";
                throw TemplateException.Create(elem, fmt, Keywords.Echo);
            }

            EnsureAttributes(elem, Keywords.Comment, Keywords.Name, Keywords.Value);

            string name = GetAttributeValue<string>(elem, Keywords.Name, true, null);
            double value = GetAttributeValue<double>(elem, Keywords.Value, true, 0);

            if (symbolTable.GetEntry(name) != null)
            {
                string fmt = "Variable '{0}' already exists as a non-local variable.";
                throw TemplateException.Create(elem, fmt, name);
            }

            localsMap[name] = value;

            return 0;
        }

        private int ProcessTypedefDirective(XElement elem)
        {
            if (isEvalutingTypedef)
            {
                string fmt = "Nested type definitions are not allowed.";
                throw TemplateException.Create(elem, fmt);
            }
            isEvalutingTypedef = true;

            EnsureAttributes(elem, Keywords.Comment, Keywords.Kind, Keywords.Name);

            string typename = GetAttributeValue<string>(elem, Keywords.Name, true, null);
            if (typeMap.ContainsKey(typename))
            {
                string fmt = "Type '{0}' has already been defined.";
                throw TemplateException.Create(elem, fmt, typename);
            }
            else if (Keywords.ReservedWords.Contains(typename))
            {
                string fmt = "Cannot use reserved word '{0}' as a type name.";
                throw TemplateException.Create(elem, fmt, typename);
            }

            TypeInfo kind = GetAttributeValue<TypeInfo>(elem, Keywords.Kind, true, null);   // Type analysis happens here

            typeMap[typename] = kind;
            isEvalutingTypedef = false;

            return 0;
        }

        /// <summary>
        /// Makes sure the only attributes present on a given <see cref="XElement"/>
        /// are ones with names matching a list of valid attributes. Also makes sure
        /// that each attribute has a non-whitespace value.
        /// </summary>
        /// <param name="elem">
        /// The <see cref="XElement"/> to check for valid attributes.
        /// </param>
        /// <param name="validAttributes">
        /// An array of valid attribute names.
        /// </param>
        /// <exception cref="TemplateException">
        /// If an attribute whose name does not appear in the list of valid attributes
        /// is present or if the attribute value is empty.
        /// </exception>
        private void EnsureAttributes(XElement elem, params string[] validAttributes)
        {
            foreach (XAttribute attr in elem.Attributes())
            {
                string name = attr.Name.LocalName;
                if (!validAttributes.Contains(name))
                {
                    string fmt = "Unknown attribute '{0}'.";
                    throw TemplateException.Create(attr, fmt, name);
                }
                else if (string.IsNullOrWhiteSpace(attr.Value))
                {
                    string fmt = "Attribute '{0}' cannot have an empty value.";
                    throw TemplateException.Create(attr, fmt, name);
                }
            }
        }

        private T GetAttributeValue<T>(XElement elem, string name, bool isRequired, T defaultValue)
        {
            if (!attributeActionMap.ContainsKey(name))
            {
                // Should never happen;
                // name SHOULD HAVE been validated before this method is called
                string msg = string.Format("Unknown attribute '{0}'", name);
                throw new TemplateException(msg);
            }

            XAttribute attr = elem.Attribute(name);
            if (attr == null)
            {
                if (isRequired)
                {
                    string fmt = "Missing required attribute '{0}'.";
                    throw TemplateException.Create(elem, fmt, name);
                }

                return defaultValue;
            }

            // Process the attribute value
            AttributeProcessAction<T> process = (AttributeProcessAction<T>) attributeActionMap[name];
            return process(attr);
        }

        private int ProcessCountAttribute(XAttribute attr)
        {
            try
            {
                string valStr = ResolveVariables(attr.Value);
                double val = NumberUtils.EvaluateExpression(valStr);
                if (val < 0 || !NumberUtils.IsInteger(val))
                {
                    string msg = "Value '{0}' is not a non-negative integer.";
                    throw TemplateException.Create(attr, msg, val);
                }

                return Convert.ToInt32(val);
            }
            catch (Exception e)
            {
                if (e is ArithmeticException || e is FormatException || e is OverflowException || e is TemplateException)
                {
                    throw TemplateException.Create(e, attr, e.Message);
                }

                throw;
            }
        }

        private TypeInfo ProcessKindAttribute(XAttribute attr)
        {
            string typeName = attr.Value;
            XElement srcElem = attr.Parent;

            // Process 'struct'
            if (typeName == Keywords.Struct)
            {
                // Enable 'dry run'
                // We only want to gather the type information,
                // but not apply it to underlying data
                if (dryRunRecursionDepth == 0)
                {
                    isConductingDryRun = true;
                }

                dryRunRecursionDepth++;
                int size = ProcessStructMembers(srcElem);
                dryRunRecursionDepth--;

                // Disable 'dry run'
                if (dryRunRecursionDepth == 0)
                {
                    isConductingDryRun = false;
                }

                return TypeInfo.CreateStruct(srcElem.Elements(), size);
            }

            // Process primitive or user-defined type
            if (typeMap.ContainsKey(typeName))
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

        private string ProcessMessageAttribute(XAttribute attr)
        {
            return attr.Value;
        }

        private string ProcessNameAttribute(XAttribute attr)
        {
            if (!Regex.IsMatch(attr.Value, IdentifierPattern))
            {
                string fmt = "'{0}' is not a valid identifier. Identifiers may consist only of "
                    + "alphanumeric characters and underscores, and may not begin with a number.";
                throw TemplateException.Create(attr, fmt, attr.Value);
            }

            return attr.Value;
        }

        private bool ProcessNewlineAttribute(XAttribute attr)
        {
            return ProcessBooleanAttribute(attr);
        }

        private bool ProcessRawAttribute(XAttribute attr)
        {
            return ProcessBooleanAttribute(attr);
        }

        private double ProcessValueAttribute(XAttribute attr)
        {
            try
            {
                string valStr = ResolveVariables(attr.Value);
                return NumberUtils.EvaluateExpression(valStr);
            }
            catch (Exception e)
            {
                if (e is ArithmeticException || e is FormatException || e is OverflowException || e is TemplateException)
                {
                    throw TemplateException.Create(e, attr, e.Message);
                }

                throw;
            }
        }

        private bool ProcessBooleanAttribute(XAttribute attr)
        {
            if (!bool.TryParse(attr.Value, out bool val))
            {
                string fmt = "'{0}' is not a valid boolean value.";
                throw TemplateException.Create(attr, fmt, attr.Value);
            }

            return val;
        }

        /// <summary>
        /// Replaces all variable references in the given string with their values.
        /// </summary>
        /// <param name="s">
        /// The string on which to resolve variables.
        /// </param>
        /// <returns>
        /// The input string with all variables replaced with their corresponding values.
        /// </returns>
        /// <exception cref="TemplateException">
        /// If an undefined variable is present in the string.
        /// </exception>
        private string ResolveVariables(string s)
        {
            // Resolve values
            s = Regex.Replace(s, ValueofOpPattern, ResolveValueof);
            s = Regex.Replace(s, OffsetofOpPattern, ResolveOffsetof);
            s = Regex.Replace(s, SizeofOpPattern, ResolveSizeof);

            return s;
        }

        private string ResolveValueof(Match m)
        {
            if (isEvalutingTypedef)
            {
                throw new TemplateException("Variables cannot be used when defining a type.");
            }

            string varName = m.Groups[1].Value;

            //// Evaluate expression
            //if (!string.IsNullOrWhiteSpace(expr))
            //{
            //    expr = ResolveVariables(expr);
            //    try
            //    {
            //        return NumberUtils.EvaluateExpression(expr) + "";
            //    }
            //    catch (FormatException ex)
            //    {
            //        throw new TemplateException(ex.Message, ex);
            //    }
            //}

            // Handle special variables
            switch (varName)
            {
                case Keywords.Filesize:
                    return dataLen + "";

                case Keywords.Offset:
                    return dataOffset + "";
            }

            if (localsMap.TryGetValue(varName, out double localVal))
            {
                return localVal + "";
            }

            SymbolTableEntry e = GetVariableInfo(varName);
            if (e.TypeInfo == null)
            {
                string msg = string.Format("Variable '{0}' is not yet fully defined.", varName);
                throw new TemplateException(msg);
            }
            else if (e.TypeInfo.Type == typeof(BftStruct))
            {
                throw new TemplateException("Cannot take the value of a struct.");
            }

            // Create pointer to value and dereference
            Type ptrType = GenericPointerType.MakeGenericType(new Type[] { e.TypeInfo.Type });
            object ptrObj = Activator.CreateInstance(ptrType, dataPtr + e.Offset);
            object val = ptrObj.GetType().GetProperty("Value").GetValue(ptrObj);

            return val.ToString();
        }

        private string ResolveOffsetof(Match m)
        {
            if (isEvalutingTypedef)
            {
                throw new TemplateException("Variables cannot be used when defining a type.");
            }

            string varName = m.Groups[1].Value;

            if (localsMap.ContainsKey(varName))
            {
                string msg = "Offset not defined for local variables.";
                throw new TemplateException(msg);
            }

            SymbolTableEntry e = GetVariableInfo(varName);

            return e.Offset + "";
        }

        private string ResolveSizeof(Match m)
        {
            if (isEvalutingTypedef)
            {
                throw new TemplateException("Variables cannot be used when defining a type.");
            }

            string varName = m.Groups[1].Value;

            if (localsMap.ContainsKey(varName))
            {
                string msg = "Size not defined for local variables.";
                throw new TemplateException(msg);
            }

            string typeName = null;
            Match m2 = Regex.Match(varName, TypeOpPattern);
            if (m2.Success)
            {
                typeName = m2.Groups[1].Value;
            }

            if (!string.IsNullOrWhiteSpace(typeName))
            {
                // Get size of type
                if (!typeMap.TryGetValue(typeName, out TypeInfo info))
                {
                    string msg = string.Format("Invalid type '{0}'", typeName);
                    throw new TemplateException(msg);
                }

                return info.Size + "";
            }

            // Get size of variable value
            SymbolTableEntry e = GetVariableInfo(varName);
            if (e.TypeInfo == null)
            {
                string msg = string.Format("Variable '{0}' is not yet fully defined.", varName);
                throw new TemplateException(msg);
            }

            return e.TypeInfo.Size + "";
        }

        private SymbolTableEntry GetVariableInfo(string varName)
        {
            SymbolTableEntry e = symTablStack.Peek().GetEntry(varName);
            if (e == null)
            {
                string msg = string.Format("Variable '{0}' not defined.", varName);
                throw new TemplateException(msg);
            }

            return e;
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
            directiveActionMap[Keywords.Align] = ProcessAlignDirective;
            directiveActionMap[Keywords.Echo] = ProcessEchoDirective;
            directiveActionMap[Keywords.Local] = ProcessLocalDirective;
            directiveActionMap[Keywords.Typedef] = ProcessTypedefDirective;
        }

        private void BuildAttributeActionMap()
        {
            attributeActionMap[Keywords.Count] = (AttributeProcessAction<int>) ProcessCountAttribute;
            attributeActionMap[Keywords.Kind] = (AttributeProcessAction<TypeInfo>) ProcessKindAttribute;
            attributeActionMap[Keywords.Message] = (AttributeProcessAction<string>) ProcessMessageAttribute;
            attributeActionMap[Keywords.Name] = (AttributeProcessAction<string>) ProcessNameAttribute;
            attributeActionMap[Keywords.Newline] = (AttributeProcessAction<bool>) ProcessNewlineAttribute;
            attributeActionMap[Keywords.Raw] = (AttributeProcessAction<bool>) ProcessRawAttribute;
            attributeActionMap[Keywords.Value] = (AttributeProcessAction<double>) ProcessValueAttribute;
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

        /// <summary>
        /// Replaces all C-like escape sequences with the character they
        /// represent.
        /// </summary>
        /// <remarks>
        /// Not all C escape sequences are supported.
        /// </remarks>
        /// <param name="s">
        /// The string on which to resolve escape sequences.
        /// </param>
        /// <returns>
        /// The input string with all valid C-like escape sequences
        /// resolved.
        /// </returns>
        /// <exception cref="FormatException">
        /// If an invalid escape sequence exists in the string.
        /// </exception>
        private static string ResolveEscapeSequences(string s)
        {
            return Regex.Replace(s, @"\\([\S\s])", m =>
            {
                char c = m.Groups[1].Value[0];
                string esc = "";
                switch (c)
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

                    case 't':
                        esc += '\t';
                        break;

                    case '\\':
                        esc += '\\';
                        break;

                    default:
                        string exMsg = string.Format(@"Invalid escape sequence '\{0}'", c);
                        throw new FormatException(exMsg);
                }

                return esc;
            });
        }
    }
}
