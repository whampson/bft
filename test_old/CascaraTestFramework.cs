using System;
using System.Xml.Linq;
using Xunit.Abstractions;

namespace Cascara.Tests
{
    public abstract class CascaraTestFramework
    {
        protected static string BuildXmlElement(string name, params Tuple<string, string>[] attrs)
        {
            return BuildXmlElement(name, "", attrs);
        }

        protected static string BuildXmlElement(string name, string innerData, params Tuple<string, string>[] attrs)
        {
            string attrString = "";
            foreach (Tuple<string, string> t in attrs)
            {
                attrString += string.Format(" {0}='{1}'", t.Item1, t.Item2);
            }

            return string.Format("<{0}{1}>{2}</{0}>", name, attrString, innerData);
        }

        protected static XElement ParseXmlElement(string data)
        {
            XDocument doc = XDocument.Parse(data, LoadOptions.SetLineInfo);

            return doc.Root;
        }

        public CascaraTestFramework(ITestOutputHelper output)
        {
            Output = output;
        }

        protected ITestOutputHelper Output
        {
            get;
        }
    }
}
