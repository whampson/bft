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
using System.Xml;
using System.Xml.Linq;

namespace WHampson.Bft
{
    public sealed class TemplateFile
    {
        private static XDocument OpenXmlFile(string path)
        {
            try
            {
                return XDocument.Load(path, LoadOptions.SetLineInfo);
            }
            catch (XmlException e)
            {
                throw new TemplateException(e.Message, e);
            }
        }

        private XDocument doc;
        private TemplateProcessor processor;

        public TemplateFile(string path)
        {
            doc = OpenXmlFile(path);
        }

        public T Process<T>(string filePath)
        {
            TemplateProcessor processor = new TemplateProcessor(doc);

            return processor.Process<T>(filePath);
        }

        public string this[string key]
        {
            // Get template metadata (Root element attribute values)
            get
            {
                XAttribute attr = doc.Root.Attribute(key);
                return (attr != null) ? attr.Value : null;
            }
        }
    }
}