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
using System.Xml.Linq;
using WHampson.Bft.Types;

using Int32 = WHampson.Bft.Types.Int32;

namespace WHampson.Bft
{
    internal sealed class TemplateProcessor
    {
        private delegate int DirectiveProcessAction(XElement elem);

        private XDocument templateDoc;
        private Dictionary<string, CustomTypeInfo> customTypes;
        private Dictionary<Keyword, DirectiveProcessAction> directiveActionMap;
        private IntPtr dataPtr;
        private int dataLen;
        private int dataOffset;
        private bool isEvalutingTypedef;

        public TemplateProcessor(XDocument doc)
        {
            templateDoc = doc;
            customTypes = new Dictionary<string, CustomTypeInfo>();
            directiveActionMap = new Dictionary<Keyword, DirectiveProcessAction>();
            dataPtr = IntPtr.Zero;
            dataLen = 0;
            dataOffset = 0;
            isEvalutingTypedef = false;

            BuildDirectiveActionMap();
        }

        public T Process<T>(string filePath)
        {
            T obj;
            Process(filePath, out obj);

            return obj;
        }

        public int Process<T>(string filePath, out T obj)
        {
            // Validate root element
            if (templateDoc.Root.Name.LocalName != Keywords.Bft)
            {
                string fmt = "Template must have a root element named '{0}'.";
                throw TemplateException.Create(templateDoc.Root, fmt, Keywords.Bft);
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

            // Free pinned file data
            // (This will change in the future depending on how we want to manipulate the file data)
            //Marshal.FreeHGlobal(dataPtr);

            obj = (T) o;
            return bytesProcessed;
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
                throw TemplateException.Create(elem, fmt);
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
                throw TemplateException.Create(elem, fmt);
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
                    throw TemplateException.Create(elem, fmt, name);
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
                throw TemplateException.Create(elem, fmt);
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
                    throw TemplateException.Create(elem, fmt);
                }
            }

            return localOffset;
        }

        private int ProcessStructureMember(XElement elem, Type parentType, PropertyInfo[] typeProperties, out object[] memberArray, out string name)
        {
            string keywordIdentifier = elem.Name.LocalName;

            Keyword kw;
            bool isValidKeyword = Keywords.KeywordMap.TryGetValue(keywordIdentifier, out kw);

            CustomTypeInfo userDefinedTypeInfo;
            bool isUserDefinedType = customTypes.TryGetValue(keywordIdentifier, out userDefinedTypeInfo);

            if (!isValidKeyword && !isUserDefinedType)
            {
                string fmt = "Unknown type or directive '{0}'.";
                throw TemplateException.Create(elem, fmt, keywordIdentifier);
            }

            if (isUserDefinedType)
            {
                if (userDefinedTypeInfo.IsStruct)
                {
                    return ProcessStructure(elem, typeProperties, out memberArray, out name);
                }

                return ProcessPrimitive(elem, userDefinedTypeInfo.BaseType, out memberArray, out name);
            }

            DirectiveProcessAction processDirective;
            bool isDirective = directiveActionMap.TryGetValue(kw, out processDirective);
            if (isDirective)
            {
                memberArray = new object[0];
                name = null;
                return processDirective(elem);
            }

            if (kw == Keywords.Struct)
            {
                return ProcessStructure(elem, typeProperties, out memberArray, out name);
            }

            Type type = TypeMap[kw];

            return ProcessPrimitive(elem, type, out memberArray, out name);
        }

        private void GetStructureModifiers(XElement elem, out int count, out string name)
        {
            Dictionary<Keyword, Modifier> modifierMap =
                BuildModifierMap(elem, Keywords.Comment, Keywords.Count, Keywords.Name);

            CountModifier countModifier = null;
            NameModifier nameModifier = null;

            Modifier tmpModifier;
            if (modifierMap.TryGetValue(Keywords.Count, out tmpModifier))
            {
                countModifier = (CountModifier) tmpModifier;
            }
            if (modifierMap.TryGetValue(Keywords.Name, out tmpModifier))
            {
                nameModifier = (NameModifier) tmpModifier;
            }

            name = (nameModifier != null) ? nameModifier.Value : null;
            count = (countModifier != null) ? countModifier.Value : 1;
        }

        private int ProcessAlign(XElement elem)
        {
            Dictionary<Keyword, Modifier> modifierMap =
                BuildModifierMap(elem, Keywords.Comment, Keywords.Count, Keywords.Kind);

            CountModifier countModifier = null;
            KindModifier kindModifier = null;

            Modifier tmpModifier;
            if (modifierMap.TryGetValue(Keywords.Count, out tmpModifier))
            {
                countModifier = (CountModifier) tmpModifier;
            }
            if (modifierMap.TryGetValue(Keywords.Kind, out tmpModifier))
            {
                kindModifier = (KindModifier) tmpModifier;
            }

            int count = (countModifier != null) ? countModifier.Value : 1;
            string kind = (kindModifier != null) ? kindModifier.Value : Keywords.Int8;

            int typeSize;

            CustomTypeInfo userDefinedTypeInfo;
            bool isKindCustomType = customTypes.TryGetValue(kind, out userDefinedTypeInfo);

            if (isKindCustomType)
            {
                typeSize = userDefinedTypeInfo.Size;
            }
            else
            {
                Keyword dummyKw;
                Type dummyType;
                if (!(Keywords.KeywordMap.TryGetValue(kind, out dummyKw)
                    && TypeMap.TryGetValue(dummyKw, out dummyType)))
                {
                    string fmt = "Unknown type '{0}'.";
                    throw TemplateException.Create(kindModifier.SourceAttribute, fmt, kind);
                }

                typeSize = Marshal.SizeOf(dummyType);
            }

            int off = count * typeSize;
            if (!isEvalutingTypedef)
            {
                dataOffset += off;
            }

            return off;
        }

        private int ProcessEcho(XElement elem)
        {
            Dictionary<Keyword, Modifier> modifierMap =
               BuildModifierMap(elem, Keywords.Comment, Keywords.Message);

            MessageModifier messageModifier = null;

            Modifier tmpModifier;
            if (modifierMap.TryGetValue(Keywords.Message, out tmpModifier))
            {
                messageModifier = (MessageModifier) tmpModifier;
            }

            if (messageModifier == null)
            {
                string msg = "Missing required modifier '{0}'.";
                throw TemplateException.Create(elem, msg, Keywords.Message);
            }

            string message = messageModifier.Value;

            // TODO: configurable output stream
            Console.WriteLine(message);

            return 0;
        }

        private int ProcessTypedef(XElement elem)
        {
            // Set this so we don't increment the offset when
            // evaluating the type structure
            isEvalutingTypedef = true;

            Dictionary<Keyword, Modifier> modifierMap =
                BuildModifierMap(elem, Keywords.Comment, Keywords.Kind, Keywords.Typename);

            KindModifier kindModifier = null;
            TypenameModifier typenameModifier = null;

            Modifier tmpModifier;
            if (modifierMap.TryGetValue(Keywords.Kind, out tmpModifier))
            {
                kindModifier = (KindModifier) tmpModifier;
            }
            if (modifierMap.TryGetValue(Keywords.Typename, out tmpModifier))
            {
                typenameModifier = (TypenameModifier) tmpModifier;
            }

            if (kindModifier == null || typenameModifier == null)
            {
                Keyword k = (kindModifier == null) ? Keywords.Kind : Keywords.Typename;
                string fmt = "Missing required modifier '{0}'.";
                throw TemplateException.Create(elem, fmt, k);
            }

            string kind = kindModifier.Value;
            string typename = typenameModifier.Value;

            if (Keywords.KeywordMap.ContainsKey(typename))
            {
                string fmt = "Reserved word '{0}' may not be used as a type name.";
                throw TemplateException.Create(typenameModifier.SourceAttribute, fmt, typename);
            }

            // Ensure type isn't already defined
            if (customTypes.ContainsKey(typename))
            {
                string fmt = "Type '{0}' has already been defined.";
                throw TemplateException.Create(typenameModifier.SourceAttribute, fmt, typename);
            }

            CustomTypeInfo existingCustomTypeInfo;
            Keyword dummyKeyword;
            bool isKindKeyword = Keywords.KeywordMap.TryGetValue(kind, out dummyKeyword);
            bool isAliasOfUserDefinedType = customTypes.TryGetValue(kind, out existingCustomTypeInfo);

            if (!isKindKeyword && !isAliasOfUserDefinedType)
            {
                string fmt = "Unknown type '{0}'.";
                throw TemplateException.Create(kindModifier.SourceAttribute, fmt, kind);
            }
            else if (isKindKeyword && kind != Keywords.Struct)
            {
                if (!TypeMap.ContainsKey(dummyKeyword))
                {
                    string fmt = "Unknown type '{0}'.";
                    throw TemplateException.Create(kindModifier.SourceAttribute, fmt, kind);
                }
            }

            if (kind != Keywords.Struct && elem.Elements().Count() != 0)
            {
                //string fmt = "Type definitions not descending directly from '{0}' cannot contain member fields.";
                string fmt = "Member fields are not allowed in types that are not '{0}'.";
                throw TemplateException.Create(elem.Elements().ElementAt(0), fmt, Keywords.Struct);
            }

            if (isAliasOfUserDefinedType && existingCustomTypeInfo.IsStruct)
            {
                kind = Keywords.Struct;
            }

            if (isAliasOfUserDefinedType)
            {
                customTypes[typename] = existingCustomTypeInfo;
                Console.WriteLine("{0} => {1}  ({2} bytes) (alias)", typename, kind, existingCustomTypeInfo.Size);
                isEvalutingTypedef = false;
                return 0;
            }

            CustomTypeInfo newTypeInfo;

            if (kind == Keywords.Struct)
            {
                // Ensure there are no nested typedefs
                IEnumerable<XElement> nestedTypedefs = elem.Descendants()
                    .Where(e => e.Name.LocalName == Keywords.Typedef);
                if (nestedTypedefs.Count() != 0)
                {
                    string fmt = "Nested type definitions are not allowed.";
                    throw TemplateException.Create(nestedTypedefs.ElementAt(0), fmt);
                }

                // Validate type definition
                object dummy;
                int size = ProcessStructure(elem, true, null, out dummy);
                newTypeInfo = CustomTypeInfo.CreateStruct(elem.Elements(), size);
            }
            else
            {
                Type baseType = TypeMap[dummyKeyword];
                newTypeInfo = CustomTypeInfo.CreatePrimitive(baseType);
            }

            // Store custom type
            customTypes[typename] = newTypeInfo;

            // DEBUG: show custom type mapping to its root builtin type
            Console.WriteLine("{0} => {1}  ({2} bytes)", typename, kind, newTypeInfo.Size);
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

            if (elem.Elements().Count() != 0)
            {
                string fmt = "Primtive types cannot have member fields.";
                throw TemplateException.Create(elem.Elements().ElementAt(0), fmt);
            }

            // Get modifiers
            Dictionary<Keyword, Modifier> modifierMap =
                BuildModifierMap(elem, Keywords.Comment, Keywords.Count, Keywords.Name);

            bool hasCount;
            bool hasName;
            CountModifier countModifier = null;
            NameModifier nameModifier = null;

            Modifier tmpModifier;
            if (hasCount = modifierMap.TryGetValue(Keywords.Count, out tmpModifier))
            {
                countModifier = (CountModifier) tmpModifier;
            }
            if (hasName = modifierMap.TryGetValue(Keywords.Name, out tmpModifier))
            {
                nameModifier = (NameModifier) tmpModifier;
            }

            int count = (hasCount) ? countModifier.Value : 1;
            name = (hasName) ? nameModifier.Value : null;

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
                Console.WriteLine("{0} at: {1} (abs: {2})", type.Name, dataOffset, dataPtr + dataOffset);
                dataOffset += localOffset;
                Console.WriteLine(dataOffset);
            }

            return localOffset;
        }

        private Dictionary<Keyword, Modifier> BuildModifierMap(XElement e, params Keyword[] validAttrs)
        {
            Dictionary<Keyword, Modifier> modifierMap = new Dictionary<Keyword, Modifier>();
            IEnumerable<XAttribute> attrs = e.Attributes();

            foreach (XAttribute attr in attrs)
            {
                string mId = attr.Name.LocalName;
                Keyword m;

                // Get modifier
                if (!Keywords.KeywordMap.TryGetValue(mId, out m))
                {
                    string fmt = "Invalid modifier '{0}'.";
                    throw TemplateException.Create(attr, fmt, mId);
                }

                // Check if modifier is valid for current type
                if (!validAttrs.Contains(m))
                {
                    string fmt = "Invalid modifier '{0}'.";
                    throw TemplateException.Create(attr, fmt, mId);
                }

                // Ensure value is not empty
                if (string.IsNullOrWhiteSpace(attr.Value))
                {
                    string fmt = "Value for modifier '{0}' cannot be empty.";
                    throw TemplateException.Create(attr, fmt, mId);
                }

                // Create new instance of Modifier subclass
                // First, get the modifier subclass
                Type modifierType = ModifierMap[m];

                // Create the Modifier instance and try to set it's value
                Modifier mod = (Modifier) Activator.CreateInstance(modifierType, attr);
                if (!mod.TrySetValue(attr.Value))
                {
                    string fmt = ModifierSetValueErrorMap[mod.GetType()];
                    throw TemplateException.Create(attr, fmt, attr.Value);
                }

                modifierMap[m] = mod;
            }

            return modifierMap;
        }

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

        private static readonly Dictionary<Type, string> ModifierSetValueErrorMap = new Dictionary<Type, string>()
        {
            { typeof(CommentModifier), null },
            { typeof(CountModifier),
                "'{0}' is not a valid integer. Value must be a non-negative number." },
            { typeof(KindModifier), null },
            { typeof(MessageModifier), null },
            { typeof(NameModifier),
                "'{0}' is not a valid variable name. Variable names may consist only of "
                + "alphanumeric characters and underscores, and may not begin with a number." },
            { typeof(TypenameModifier),
                "'{0}' is not a valid type name. Type names may consist only of "
                + "alphanumeric characters and underscores, and may not begin with a number." }
        };

        private static readonly Dictionary<Keyword, Type> ModifierMap = new Dictionary<Keyword, Type>()
        {
            { Keywords.Comment, typeof(CommentModifier) },
            { Keywords.Count, typeof(CountModifier) },
            { Keywords.Kind, typeof(KindModifier) },
            { Keywords.Message, typeof(MessageModifier) },
            { Keywords.Name, typeof(NameModifier) },
            { Keywords.Typename, typeof(TypenameModifier) },
        };

        private static readonly Dictionary<Keyword, Type> TypeMap = new Dictionary<Keyword, Type>()
        {
            { Keywords.Float, typeof(Float) },
            { Keywords.Int8, typeof(Int8) },
            { Keywords.Int32, typeof(Int32) },
        };

        private void BuildDirectiveActionMap()
        {
            directiveActionMap[Keywords.Align] = ProcessAlign;
            directiveActionMap[Keywords.Echo] = ProcessEcho;
            directiveActionMap[Keywords.Typedef] = ProcessTypedef;
        }
    }
}
