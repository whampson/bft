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
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace WHampson.BFT
{
    public static class TemplateReader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="templatePath"></param>
        /// <param name="baseAddr"></param>
        /// <param name="bufLen"></param>
        /// <returns></returns>
        /// <exception cref="BinaryFileTemplateException"></exception>
        public static T ProcessTemplate<T>(string templatePath, IntPtr baseAddr, int bufLen)
        {
            XDocument xDoc;

            // Open template XML file
            try
            {
                xDoc = XDocument.Load(templatePath, LoadOptions.SetLineInfo);
            }
            catch (XmlException e)
            {
                throw new BinaryFileTemplateException(e.Message);
            }

            IEnumerable<XElement> children = xDoc.Root.Elements();
            if (children.Count() == 0)
            {
                throw new BinaryFileTemplateException("empty binary file template");
            }

            object o;
            int size = ProcessStructure<T>(xDoc.Root, baseAddr, out o);
            Console.WriteLine("Processed {0} bytes, ({1} bytes remain)", size, bufLen - size);

            return (T) o;
        }

        private static int ProcessStructure<T>(XElement structRoot, IntPtr baseAddr, out object o)
        {
            return ProcessStructure(typeof(T), structRoot, baseAddr, out o);
        }

        private static int ProcessStructure(Type targetType, XElement structRoot, IntPtr baseAddr, out object o)
        {
            PropertyInfo[] typeProperties = targetType.GetProperties();

            int offset = 0;
            Dictionary<string, object[]> members = new Dictionary<string, object[]>();

            foreach (XElement elem in structRoot.Elements())
            {
                int count = 1;
                XAttribute countAttr = elem.Attribute("count");
                if (countAttr != null)
                {
                    bool success = int.TryParse(countAttr.Value, out count);
                    if (!success || count < 1)
                    {
                        throw new BinaryFileTemplateException("count must be a positive integer");
                    }
                }
                object[] membArr = new object[count];

                XAttribute nameAttr = elem.Attribute("name");

                if (elem.Name.LocalName == "struct")
                {
                    // TODO: handle case where struct name is undefined
                    PropertyInfo prop = null;
                    foreach (PropertyInfo p in typeProperties)
                    {
                        if (p.Name.ToLower() == nameAttr.Value.ToLower())
                        {
                            prop = p;
                            break;
                        }
                    }
                    if (prop == null)
                    {
                        throw new BinaryFileTemplateException("struct not found: " + nameAttr.Value);
                    }

                    Type propType = prop.PropertyType;
                    if (propType.IsArray)
                    {
                        propType = propType.GetElementType();
                    }

                    for (int i = 0; i < count; i++)
                    {
                        object structure;
                        offset += ProcessStructure(propType, elem, baseAddr + offset, out structure);
                        membArr[i] = structure;
                    }
                }
                else
                {
                    Type dataType = TemplatePrimitives[elem.Name.LocalName];
                    for (int i = 0; i < count; i++)
                    {
                        // TODO: allow option to read template member as a primitive type or pointer to primitive type
                        Type membType = typeof(Pointer<>).MakeGenericType(dataType);
                        membArr[i] = Activator.CreateInstance(membType, new object[] { baseAddr + offset });
                        offset += Marshal.SizeOf(dataType);
                    }
                }

                if (nameAttr == null)
                {
                    continue;
                }

                if (members.ContainsKey(nameAttr.Value))
                {
                    throw new BinaryFileTemplateException("member already defined: " + nameAttr.Value);
                }
                members[nameAttr.Value.ToLower()] = membArr;
            }

            // Create "outer" type
            object inst = Activator.CreateInstance(targetType);
            foreach (PropertyInfo prop in typeProperties)
            {
                string name = prop.Name.ToLower();
                if (!prop.CanWrite || !members.ContainsKey(name))
                {
                    continue;
                }

                object[] val = members[name];
                bool membIsArray = prop.PropertyType.IsArray;
                if (val.Length == 1 && !membIsArray)
                {
                    // Single value
                    prop.SetValue(inst, val[0]);
                }
                else if (membIsArray)
                {
                    // Array of values
                    Array a = Array.CreateInstance(prop.PropertyType.GetElementType(), val.Length);
                    for (int i = 0; i < val.Length; i++)
                    {
                        a.SetValue(val[i], i);
                    }
                    prop.SetValue(inst, a);
                }
                else
                {
                    throw new BinaryFileTemplateException("attempt to set an array to an non-array member");
                }
            }

            o = inst;
            return offset;
        }

        private static readonly Dictionary<string, Type> TemplatePrimitives = new Dictionary<string, Type>
        {
            { "float", typeof(Float) },
            { "int8", typeof(Int8) },
            { "int32", typeof(Int32) }
        };
    }
}
