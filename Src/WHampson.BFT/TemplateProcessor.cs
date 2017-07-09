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

        //private delegate int ProcessAction<U>(IntPtr pData, XElement intElem, int off);

        private XDocument templateDoc;
        private Dictionary<string, CustomTypeInfo> customTypes;
        private IntPtr dataPtr;
        private int dataLen;
        private int dataOffset;

        private bool isEvalutingTypedef;

        public TemplateProcessor(XDocument doc)
        {
            templateDoc = doc;
            customTypes = new Dictionary<string, CustomTypeInfo>();
            dataPtr = IntPtr.Zero;
            dataLen = 0;
            dataOffset = 0;
            isEvalutingTypedef = false;

            //BuildActionMaps();
        }

        public T Process<T>(string filePath)
        {
            // Validate root element
            if (templateDoc.Root.Name != RootElementName)
            {
                string fmt = "Template must have a root element named '{0}'.";
                string msg = XmlUtils.BuildXmlErrorMsg(templateDoc.Root, fmt, RootElementName);
                throw new TemplateException(msg);
            }

            IEnumerable<XElement> elems = templateDoc.Root.Elements();
            if (elems.Count() == 0)
            {
                throw new TemplateException("Empty binary file template.");
            }

            // Load binary file
            byte[] data = LoadFile(filePath);
            dataLen = data.Length;

            // Pin file data to unmanaged memory
            dataPtr = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, dataPtr, data.Length);

            // Clear current list of custom types (if any)
            customTypes.Clear();

            Console.WriteLine("File located at: " + dataPtr);

            // Process the template with respect to the file data
            dataOffset = 0;
            object o;
            int bytesProcessed = ProcessStructure(templateDoc.Root, true, typeof(T), out o);
            Console.WriteLine("Processed {0} bytes.", bytesProcessed);

            // Free pinned file data
            // (This will change in the future depending on how we want to manipulate the file data)
            //Marshal.FreeHGlobal(dataPtr);

            return (T) o;
        }

        private int ProcessStructure(XElement elem, PropertyInfo[] parentPropertyInfo, out object[] structureArr, out string name)
        {
            int count = 1;
            GetStructureModifiers(elem, out count, out name);

            structureArr = new object[count];
            string nam = name;
            PropertyInfo prop = parentPropertyInfo.Where(p => p.Name.ToLower() == nam.ToLower()).Single();
            Type structureType = (prop == null) ? null : prop.PropertyType;
            if (structureType != null && structureType.IsArray)
            {
                structureType = structureType.GetElementType();
            }

            int localOffset = 0;
            for (int i = 0; i < count; i++)
            {
                object structure;
                localOffset += ProcessStructure(elem, true, structureType, out structure);
                structureArr[i] = structure;
            }

            return localOffset;
        }

        private int ProcessStructure(XElement elem, Type oType, out object o)
        {
            return ProcessStructure(elem, false, oType, out o);
        }

        private int ProcessStructure(XElement elem, bool ignoreModifiers, Type oType, out object o)
        {
            string dataTypeIdentifier = elem.Name.LocalName;

            // Ensure no text data is present
            if (!string.IsNullOrWhiteSpace(elem.Value))
            {
                string fmt = "Unexpected textual data.";
                throw new TemplateException(XmlUtils.BuildXmlErrorMsg(elem, fmt));
            }

            // Check whether this is user-defined type or a plain-old struct
            CustomTypeInfo typeInfo;
            bool isCustomType = customTypes.TryGetValue(dataTypeIdentifier, out typeInfo);

            // Get member elements
            IEnumerable<XElement> children = (isCustomType)
                ? typeInfo.Members
                : elem.Elements();

            // Ensure struct has at least one member field
            if (children.Count() == 0)
            {
                string fmt = "Empty structs are not allowed.";
                throw new TemplateException(XmlUtils.BuildXmlErrorMsg(elem, fmt));
            }

            // Extract modifiers
            int count = 1;
            string name = null;
            if (!ignoreModifiers)
            {
                GetStructureModifiers(elem, out count, out name);
            }

            // Get type info
            PropertyInfo[] oProperties = (oType != null)
                ? oType.GetProperties()
                : new PropertyInfo[0];

            // Process members
            Dictionary<string, object[]> members = new Dictionary<string, object[]>();
            int localOffset = 0;

            foreach (XElement memberElem in children)
            {
                object[] memberArr;
                localOffset += ProcessStructureMember(memberElem, oType, oProperties, out memberArr, out name);

                if (name != null && members.ContainsKey(name.ToLower()))
                {
                    string fmt = "Variable '{0}' already defined.";
                    throw new TemplateException(XmlUtils.BuildXmlErrorMsg(memberElem, fmt, name));
                }

                // Add to member list
                if (name != null)
                {
                    members[name.ToLower()] = memberArr;
                }
            }
            
            if (localOffset == 0)
            {
                string fmt = "Empty structs are not allowed.";
                throw new TemplateException(XmlUtils.BuildXmlErrorMsg(elem, fmt));
            }

            if (oType == null || name == null)
            {
                o = null;
                return localOffset;
            }

            // Create object with members (move to func)
            o = Activator.CreateInstance(oType);
            foreach (PropertyInfo prop in oProperties)
            {
                string pName = prop.Name.ToLower();
                if (/*!prop.CanWrite || */!members.ContainsKey(pName))
                {
                    continue;
                }

                object[] propVal = members[pName];
                Type propValType = prop.PropertyType.GetElementType();
                bool propIsArray = prop.PropertyType.IsArray;
                if (propVal.Length == 1 && !propIsArray)
                {
                    prop.SetValue(o, propVal[0]);
                }
                else if (propIsArray)
                {
                    Array a = Array.CreateInstance(propValType, propVal.Length);
                    for (int i = 0; i < propVal.Length; i++)
                    {
                        a.SetValue(propVal[i], i);
                    }
                    prop.SetValue(o, a);
                }
                else
                {
                    // tried to set array to non-array property, throw excep
                    string fmt = "Attempt to set an array to a non-array value.";
                    throw new TemplateException(XmlUtils.BuildXmlErrorMsg(elem, fmt));
                }
            }

            return localOffset;
        }

        private int ProcessStructureMember(XElement elem, Type parentType, PropertyInfo[] typeProperties, out object[] memberArray, out string name)
        {
            string dataTypeIdentifier = elem.Name.LocalName;

            // Look up member type
            BuiltinType type;
            bool validType = LookupType(dataTypeIdentifier, out type);

            // Look up directive if not a valid data type
            Directive dir = default(Directive);
            bool validDirective = false;
            if (!validType)
            {
                validDirective = DirectiveIdentifierMap.TryGetValue(dataTypeIdentifier, out dir);
                if (!validDirective)
                {
                    string fmt = "Unknown type or directive '{0}'.";
                    throw new TemplateException(XmlUtils.BuildXmlErrorMsg(elem, fmt, dataTypeIdentifier));
                }
            }

            if (validDirective)
            {
                memberArray = new object[0];
                name = null;
                return ProcessDirective(elem, dir);
            }

            if (type == BuiltinType.Struct)
            {
                return ProcessStructure(elem, typeProperties, out memberArray, out name);
            }
            else
            {
                return ProcessPrimitive(elem, TypeMap[type], out memberArray, out name);
                //Type valueType = TypeMap[type];
                //MethodInfo processPrimGeneric = GetType().GetMethod("ProcessPrimitive", BindingFlags.NonPublic | BindingFlags.Instance);
                //MethodInfo processPromSpecific = processPrimGeneric.MakeGenericMethod(new Type[] { valueType });
                //object[] parameters = { dataPtr, memberElem, offset };
                //offset += (int) processPromSpecific.Invoke(this, parameters);
            }
        }

        private void GetStructureModifiers(XElement elem, out int count, out string name)
        {
            Dictionary<Modifier, Modifier2> modifierMap =
                BuildModifierMap(elem, Modifier.Comment, Modifier.Count, Modifier.Name);

            CountModifier countModifier = null;
            NameModifier nameModifier = null;

            Modifier2 tmpModifier;
            if (modifierMap.TryGetValue(Modifier.Count, out tmpModifier))
            {
                countModifier = (CountModifier) tmpModifier;
            }
            if (modifierMap.TryGetValue(Modifier.Name, out tmpModifier))
            {
                nameModifier = (NameModifier) tmpModifier;
            }

            name = (nameModifier != null) ? nameModifier.Value : null;
            count = (countModifier != null) ? countModifier.Value : 1;
        }

        private int ProcessDirective(XElement elem, Directive dir)
        {
            int localOffset = 0;
            switch (dir)
            {
                case Directive.Align:
                    localOffset += ProcessAlign(elem);
                    break;

                case Directive.Echo:
                    localOffset += ProcessEcho(elem);
                    break;

                case Directive.Typedef:
                    localOffset += ProcessTypedef(elem);
                    break;
            }

            dataOffset += localOffset;

            return localOffset;
        }

        private int ProcessAlign(XElement elem)
        {
            Dictionary<Modifier, Modifier2> modifierMap =
                BuildModifierMap(elem, Modifier.Comment, Modifier.Count/*, Modifier.Kind*/);

            CountModifier countModifier = null;

            Modifier2 tmpModifier;
            if (modifierMap.TryGetValue(Modifier.Count, out tmpModifier))
            {
                countModifier = (CountModifier) tmpModifier;
            }

            int count = (countModifier != null) ? countModifier.Value : 1;
            int typeSize = 1;

            return count * typeSize;
        }

        private int ProcessEcho(XElement elem)
        {
            return 0;
        }

        private int ProcessTypedef(XElement elem)
        {
            isEvalutingTypedef = true;
            Dictionary<Modifier, Modifier2> modifierMap =
                BuildModifierMap(elem, Modifier.Comment, Modifier.Kind, Modifier.Name);

            KindModifier kindModifier = null;
            NameModifier nameModifier = null;

            Modifier2 tmpModifier;
            if (modifierMap.TryGetValue(Modifier.Kind, out tmpModifier))
            {
                kindModifier = (KindModifier) tmpModifier;
            }
            if (modifierMap.TryGetValue(Modifier.Name, out tmpModifier))
            {
                nameModifier = (NameModifier) tmpModifier;
            }

            string baseType = (kindModifier != null) ? kindModifier.Value : null;
            string typeName = (nameModifier != null) ? nameModifier.Value : null;

            if (baseType == null || typeName == null)
            {
                string fmt = "Missing type name or kind.";
                throw new TemplateException(XmlUtils.BuildXmlErrorMsg(elem, fmt));
            }

            if (baseType != StructIdentifier && !elem.IsEmpty)
            {
                string fmt = "Type definitions not descending directly from '{0}' cannot contain member fields.";
                throw new TemplateException(XmlUtils.BuildXmlErrorMsg(elem, fmt, StructIdentifier));
            }

            if (baseType == StructIdentifier)
            {
                // Ensure there are no nested typedefs
                IEnumerable<XElement> nestedTypedefs = elem.Descendants()
                    .Where(t => t.Name.LocalName == TypedefIdentifier);
                if (nestedTypedefs.Count() != 0)
                {
                    string fmt = "Nested type definitions are not allowed.";
                    string msg = XmlUtils.BuildXmlErrorMsg(nestedTypedefs.ElementAt(0), fmt);
                    throw new TemplateException(msg);
                }

                // Parse type definition
                object dummy;
                ProcessStructure(elem, true, null, out dummy);
            }

            XAttribute kindAttr = elem.Attribute(KindIdentifier);
            XAttribute nameAttr = elem.Attribute(NameIdentifier);
            BuiltinType kind;

            // Ensure type isn't already defined
            if (LookupType(typeName, out kind))
            {
                string fmt = "Type '{0}' has already been defined.";
                throw new TemplateException(XmlUtils.BuildXmlErrorMsg(nameAttr, fmt, typeName));
            }

            // Ensure new type is built from some pre-existing type
            if (!LookupType(baseType, out kind))
            {
                string fmt = "Unknown type '{0}'.";
                throw new TemplateException(XmlUtils.BuildXmlErrorMsg(kindAttr, fmt, baseType));
            }

            // If new type descends from another user-defined type,
            // get member elements from that type
            IEnumerable<XElement> members;
            CustomTypeInfo existingTypeInfo;
            if (customTypes.TryGetValue(baseType, out existingTypeInfo))
            {
                members = existingTypeInfo.Members;
            }
            else
            {
                members = elem.Elements();
            }

            // Store custom type
            int size = SizeOf(elem);
            CustomTypeInfo newTypeInfo = new CustomTypeInfo(kind, members, size);
            customTypes[typeName] = newTypeInfo;

            // DEBUG: show custom type mapping to its root builtin type
            Console.WriteLine("{0} => {1}  ({2} bytes)", typeName, kind.ToString().ToLower(), size);

            isEvalutingTypedef = false;
            return 0;
        }

        private int ProcessPrimitive(XElement elem, Type type, out object[] memberArray, out string name)
        {
            //int typeSize = Marshal.SizeOf(typeof(T));
            //if (offset + typeSize > dataLen)
            //{
            //    throw new IndexOutOfRangeException("File length exceeded.");
            //}

            // Get modifiers
            Dictionary<Modifier, Modifier2> modifierMap =
                BuildModifierMap(elem, Modifier.Comment, Modifier.Count, Modifier.Name/*, Modifier.Sentinel*/);

            bool hasCount;
            bool hasName;
            //bool hasSentinel;
            CountModifier countModifier = null;
            NameModifier nameModifier = null;
            //SentinelModifier<U> sentinelModifier = null;

            Modifier2 tmpModifier;
            if (hasCount = modifierMap.TryGetValue(Modifier.Count, out tmpModifier))
            {
                countModifier = (CountModifier) tmpModifier;
            }
            if (hasName = modifierMap.TryGetValue(Modifier.Name, out tmpModifier))
            {
                nameModifier = (NameModifier) tmpModifier;
            }
            //if (hasSentinel = modifierMap.TryGetValue(Modifier.Sentinel, out tmpModifier))
            //{
            //    sentinelModifier = (SentinelModifier<U>) tmpModifier;
            //}

            int count = (hasCount) ? countModifier.Value : 1;
            name = (hasName) ? nameModifier.Value : null;
            //U sentinel = (hasSentinel) ? sentinelModifier.Value : default(U);

            memberArray = new object[1];

            int localOffset = 0;
            Type pointerType = typeof(Pointer<>);
            Type[] typeArgs = new Type[] { type };
            int typeSize = Marshal.SizeOf(type);

            Type t = pointerType.MakeGenericType(typeArgs);
            memberArray[0] = Activator.CreateInstance(t, dataPtr + dataOffset, count);
            localOffset = (typeSize * count);
            if (!isEvalutingTypedef)
            {
                Console.WriteLine("Primitive at: " + (dataPtr + dataOffset));
                dataOffset += localOffset;
                Console.WriteLine(dataOffset);
            }

            //for (int i = 0; i < count; i++)
            //{
            //    Type t = pointerType.MakeGenericType(typeArgs);
            //    memberArray[i] = Activator.CreateInstance(t, dataPtr + dataOffset);
            //    dataOffset += typeSize;
            //    localOffset += typeSize;
            //}

            return localOffset;
        }

        private Dictionary<Modifier, Modifier2> BuildModifierMap(XElement e, params Modifier[] validAttrs)
        {
            Dictionary<Modifier, Modifier2> modifierMap = new Dictionary<Modifier, Modifier2>();
            IEnumerable<XAttribute> attrs = e.Attributes();

            bool hasCount = false;
            bool hasSentinel = false;
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

                // Ensure value is not empty
                if (string.IsNullOrWhiteSpace(attr.Value))
                {
                    string fmt = "Value for modifier '{0}' cannot be empty.";
                    throw new TemplateException(XmlUtils.BuildXmlErrorMsg(attr, fmt, mId));
                }

                // Ensure that both 'count' and 'sentinel' aren't present at the same time
                // (as this causes confusion about where to end the array)
                if (m == Modifier.Count)
                {
                    if (hasSentinel)
                    {
                        string fmt = "Modifier '{0}' conflicts with modifier '{1}'.";
                        string msg = XmlUtils.BuildXmlErrorMsg(attr, fmt, CountIdentifier, SentinelIdentifier);
                        throw new TemplateException(msg);
                    }
                    hasCount = true;
                }
                else if (m == Modifier.Sentinel)
                {
                    if (hasCount)
                    {
                        string fmt = "Modifier '{0}' conflicts with modifier '{1}'.";
                        string msg = XmlUtils.BuildXmlErrorMsg(attr, fmt, SentinelIdentifier, CountIdentifier);
                        throw new TemplateException(msg);
                    }
                    hasSentinel = true;
                }

                // Create new instance of Modifier subclass
                // First, get the modifier subclass
                Type modifierType = ModifierMap[m];

                // The Sentinel modifier takes a type parameter, so we must supply that
                // The type parameter matches the value type on which this modifier is used
                //if (m == Modifier.Sentinel)
                //{
                //    Type valueType = typeof(t);
                //    modifierType = modifierType.MakeGenericType(new Type[] { valueType });
                //}

                // Create the Modifier instance and try to set it's value
                Modifier2 mod = (Modifier2) Activator.CreateInstance(modifierType);
                if (!mod.TrySetValue(attr.Value))
                {
                    string fmt = mod.GetTryParseErrorMessage();
                    throw new TemplateException(XmlUtils.BuildXmlErrorMsg(attr, fmt, attr.Value));
                }

                modifierMap[m] = mod;
            }

            return modifierMap;
        }

        //private int SizeOf(XElement structure)
        //{
        //    return SizeOf(structure, true);
        //}

        //private int SizeOf(XElement structure, bool dereferenceVariables)
        //{
        //    return 0;
        //}

        // TODO: rewrite
        private int SizeOf(XElement e)
        {
            string typeIdentifier = e.Name.LocalName;

            CustomTypeInfo customTypeInfo;
            bool isUserDefinedType = customTypes.TryGetValue(typeIdentifier, out customTypeInfo);
            if (isUserDefinedType)
            {
                return customTypeInfo.Size;
            }

            BuiltinType type;
            Directive dir;
            bool isBuiltinType = BuiltinTypeIdentifierMap.TryGetValue(typeIdentifier, out type);
            bool isDirective = DirectiveIdentifierMap.TryGetValue(typeIdentifier, out dir);

            XAttribute countAttr = e.Attribute(CountIdentifier);
            int count = 1;

            // TODO: make func bool GetCountValue(XElement, bool allowVariables, out value);
            if (countAttr != null)
            {
                bool countValid = int.TryParse(countAttr.Value, out count);
                if (!countValid || count < 0)
                {
                    string fmt = "'{0}' must be a non-negative integer.";
                    throw new TemplateException(XmlUtils.BuildXmlErrorMsg(countAttr, fmt, CountIdentifier));
                }
            }

            if (isDirective)
            {
                if (dir != Directive.Align && dir != Directive.Typedef)
                {
                    return 0;   // Directives other than 'align' and 'typedef' do not add to the size
                }

                XAttribute kindAttr = e.Attribute(KindIdentifier);
                type = BuiltinType.Int8;    // Default
                if (kindAttr != null)
                {
                    bool kindValid = LookupType(kindAttr.Value, out type);
                    if (!kindValid)
                    {
                        string fmt = "Unknown type '{0}'.";
                        throw new TemplateException(XmlUtils.BuildXmlErrorMsg(kindAttr, fmt, kindAttr.Value));
                    }
                }
            }

            int size = 0;
            if (type == BuiltinType.Struct)
            {
                foreach (XElement memb in e.Elements())
                {
                    size += SizeOf(memb);
                }
            }
            else
            {
                Type t = TemplateProcessor.TypeMap[type];
                size = Marshal.SizeOf(t);
            }

            return size * count;
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

        private static readonly Dictionary<Modifier, Type> ModifierMap = new Dictionary<Modifier, Type>()
        {
            { Modifier.Count, typeof(CountModifier) },
            { Modifier.Kind, typeof(KindModifier) },
            { Modifier.Name, typeof(NameModifier) },
            //{ Modifier.Sentinel, typeof(SentinelModifier<>) },
        };
    }
}
