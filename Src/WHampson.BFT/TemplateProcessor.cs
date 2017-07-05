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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using WHampson.BFT.Types;
using static WHampson.BFT.Keyword;

using Int32 = WHampson.BFT.Types.Int32;

namespace WHampson.BFT
{
    internal class TemplateProcessor
    {
        private const string RootElementName = "bft";

        //private const string VariableRegex = "\\$\\{(.+)\\}";

        private delegate int ProcessAction(IntPtr pData, XElement intElem, int off);

        private XDocument doc;
        private Dictionary<BuiltinType, ProcessAction> builtinTypeParseActionMap;
        private Dictionary<Directive, ProcessAction> directiveParseActionMap;
        private Dictionary<string, CustomTypeInfo> customTypes;
        private int fileLen;

        public TemplateProcessor(XDocument doc)
        {
            this.doc = doc;
            customTypes = new Dictionary<string, CustomTypeInfo>();
            builtinTypeParseActionMap = new Dictionary<BuiltinType, ProcessAction>();
            directiveParseActionMap = new Dictionary<Directive, ProcessAction>();

            BuildActionMaps();
        }

        public T Process<T>(string filePath)
        {
            // Validate root element
            if (doc.Root.Name != RootElementName)
            {
                string fmt = "Template must have a root element named '{0}'.";
                string msg = XmlUtils.BuildXmlErrorMsg(doc.Root, fmt, RootElementName);
                throw new TemplateException(msg);
            }

            IEnumerable<XElement> elems = doc.Root.Elements();
            if (elems.Count() == 0)
            {
                throw new TemplateException("Empty binary file template.");
            }

            // Load binary file
            byte[] data = LoadFile(filePath);
            fileLen = data.Length;

            // Pin file data to unmanaged memory
            IntPtr pData = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, pData, data.Length);

            // Clear current list of custom types (if any)
            customTypes.Clear();

            // Process the template with respect to the file data
            T outObj;
            int bytesProcessed = ProcessStructure<T>(pData, doc.Root, out outObj);
            Console.WriteLine("Processed {0} bytes.", bytesProcessed);

            // Free pinned file data
            // (This will change in the future depending on how we want to manipulate the file data)
            Marshal.FreeHGlobal(pData);

            return outObj;
        }

        private int ProcessStructure<T>(IntPtr pData, XElement structureElem, out T outObj)
        {
            return ProcessStructure<T>(pData, structureElem, false, out outObj);
        }

        private int ProcessStructure<T>(IntPtr pData, XElement structureElem, bool ignoreAttributes, out T outObj)
        {
            Type objType = typeof(T);
            string dataTypeIdentifier = structureElem.Name.LocalName;

            // Ensure no text data is present
            if (!string.IsNullOrWhiteSpace(structureElem.Value))
            {
                string fmt = "Unexpected textual data.";
                throw new TemplateException(XmlUtils.BuildXmlErrorMsg(structureElem, fmt));
            }

            // Check whether this is user-defined type or a plain-old struct
            CustomTypeInfo typeInfo;
            bool isCustomType = customTypes.TryGetValue(dataTypeIdentifier, out typeInfo);

            // Get member elements
            IEnumerable<XElement> children = (isCustomType)
                ? typeInfo.Members
                : structureElem.Elements();

            // Ensure struct has at least one member field
            if (children.Count() == 0)
            {
                string fmt = "Empty structs are not allowed.";
                throw new TemplateException(XmlUtils.BuildXmlErrorMsg(structureElem, fmt));
            }

            // Extract modifiers
            Dictionary<Modifier, string> modifierMap = (!ignoreAttributes)
                ? BuildModifierMap(structureElem, Modifier.Comment, Modifier.Count, Modifier.Name)
                : new Dictionary<Modifier, string>();

            // Process members
            int offset = 0;
            bool hasDataFields = false;
            foreach (XElement memberElem in children)
            {
                string mDataTypeIdentifier = memberElem.Name.LocalName;
                bool doLookupDirective = false;

                // Look up member type
                BuiltinType type;
                bool validType = LookupType(mDataTypeIdentifier, out type);
                if (validType)
                {
                    hasDataFields = true;
                }
                else
                {
                    doLookupDirective = true;
                }

                // Look up directive if not a valid data type
                Directive dir = Directive.Align;    // dummy value
                if (doLookupDirective && !DirectiveIdentifierMap.TryGetValue(mDataTypeIdentifier, out dir))
                {
                    string fmt = "Unknown type or directive '{0}'.";
                    throw new TemplateException(XmlUtils.BuildXmlErrorMsg(memberElem, fmt, mDataTypeIdentifier));
                }

                if (dir == Directive.Align)
                {
                    hasDataFields = true;
                }

                // Parse type or directive
                ProcessAction parse = (validType)
                    ? builtinTypeParseActionMap[type]
                    : directiveParseActionMap[dir];

                offset += parse(pData, memberElem, offset);
            }

            outObj = Activator.CreateInstance<T>();

            return offset;
        }

        private int ProcessInt32Element(IntPtr pData, XElement intElem, int off, out int val)
        {
            if (off + 4 > fileLen)
            {
                throw new IndexOutOfRangeException("File length exceeded.");
            }

            IntPtr pVal = pData + off;
            unsafe
            {
                val = *(Int32*)pVal;
                Console.WriteLine("0x{0:X8}", val);
            }

            return 4;
        }

        private Dictionary<Modifier, string> BuildModifierMap(XElement e, params Modifier[] validAttrs)
        {
            Dictionary<Modifier, string> modifierMap = new Dictionary<Modifier, string>();
            IEnumerable<XAttribute> attrs = e.Attributes();

            foreach (XAttribute attr in attrs)
            {
                string mId = attr.Name.LocalName;
                Modifier m;

                // Get modifier
                if (!ModifierIdentifierMap.TryGetValue(mId, out m))
                {
                    string fmt = "Unknown modifier '{0}'.";
                    throw new TemplateException(XmlUtils.BuildXmlErrorMsg(attr, fmt, mId));
                }

                // Check if modifier is valid for current type
                if (!validAttrs.Contains(m))
                {
                    string fmt = "Invalid modifier '{0}'.";
                    throw new TemplateException(XmlUtils.BuildXmlErrorMsg(attr, fmt, mId));
                }

                // Validate modifier
                if (string.IsNullOrWhiteSpace(attr.Value))
                {
                    string fmt = "Value for modifier '{0}' cannot be empty.";
                    throw new TemplateException(XmlUtils.BuildXmlErrorMsg(attr, fmt, mId));
                }

                modifierMap[m] = attr.Value;
            }

            return modifierMap;
        }

        private int SizeOf(XElement structure)
        {
            return SizeOf(structure, true);
        }

        private int SizeOf(XElement structure, bool dereferenceVariables)
        {
            return 0;
        }

        private bool LookupType(string identifier, out BuiltinType type)
        {
            // Look up in custom type list
            CustomTypeInfo info;
            bool found = customTypes.TryGetValue(identifier, out info);
            if (found)
            {
                type = info.Kind;
                return true;
            }

            // Look up in builtin type list
            return BuiltinTypeIdentifierMap.TryGetValue(identifier, out type);
        }

        internal static Dictionary<BuiltinType, Type> TypeMap = new Dictionary<BuiltinType, Type>()
        {
            { BuiltinType.Float, typeof(Float) },
            { BuiltinType.Int8, typeof(Int8) },
            { BuiltinType.Int32, typeof(Int32) }
        };

        private byte[] LoadFile(string filePath)
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

        private void BuildActionMaps()
        {
            // Builtin types
            //builtinTypeParseActionMap[BuiltinType.Double] = ParseFloatElement;
            //builtinTypeParseActionMap[BuiltinType.Float] = ParseFloatElement;
            //builtinTypeParseActionMap[BuiltinType.Int8] = ParseIntegerElement;
            //builtinTypeParseActionMap[BuiltinType.Int16] = ParseIntegerElement;
            builtinTypeParseActionMap[BuiltinType.Int32] = ProcessIntegerElement;
            //builtinTypeParseActionMap[BuiltinType.Int64] = ParseIntegerElement;
            //builtinTypeParseActionMap[BuiltinType.Struct] = ParseStructElement;
            //builtinTypeParseActionMap[BuiltinType.UInt8] = ParseIntegerElement;
            //builtinTypeParseActionMap[BuiltinType.UInt16] = ParseIntegerElement;
            //builtinTypeParseActionMap[BuiltinType.UInt32] = ParseIntegerElement;
            //builtinTypeParseActionMap[BuiltinType.UInt64] = ParseIntegerElement;

            // Directives
            //directiveParseActionMap[Directive.Align] = ParseAlignElement;
            //directiveParseActionMap[Directive.Echo] = ParseEchoElement;
            //directiveParseActionMap[Directive.Typedef] = ParseTypedefElement;
        }
    }
}
