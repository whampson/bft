using Microsoft.VisualStudio.TestTools.UnitTesting;
using WHampson.Cascara;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace WHampson.Cascara.Tests
{
    [TestClass]
    public class BinaryLayoutTests
    {
        private const string TestLayoutName = "TestLayout";

        private static string GetRootElementOpenTag()
        {
            return "<" + Keywords.DocumentRoot + ">";
        }

        private static string GetRootElementCloseTag()
        {
            return "</" + Keywords.DocumentRoot + ">";
        }

        private static string CreateLayoutFromString(string xmlData, params Tuple<string, string>[] metadata)
        {
            string metadataStr = "";

            foreach (Tuple<string, string> t in metadata)
            {
                metadataStr += (" " + t.Item1 + "='" + t.Item2 + "'");
            }
            metadataStr.Trim();

            return string.Format("<{0} name='{1}'{2}>{3}</{0}>",
                Keywords.DocumentRoot, TestLayoutName, metadataStr, xmlData);
        }

        [TestMethod]
        public void InvalidXml()
        {
            // Arrange
            string data = "This is not XML";

            // Act, Assert
            Assert.ThrowsException<XmlException>(() => new BinaryLayout(data));
        }

        [TestMethod]
        public void InvalidRootElement()
        {
            // Arrange
            string data = "<wrongRoot>Some Data</wrongRoot>";

            // Act, Assert
            Assert.ThrowsException<LayoutException>(() => new BinaryLayout(data));
        }

        [TestMethod]
        public void EmptyLayout()
        {
            // Arrange
            string data = CreateLayoutFromString("");

            // Act, Assert
            Assert.ThrowsException<LayoutException>(() => new BinaryLayout(data));
        }

        [TestMethod]
        public void InvalidElement()
        {
            // Arrange
            string data = CreateLayoutFromString("<badElem/>");

            // Act, Assert
            Assert.ThrowsException<LayoutException>(() => new BinaryLayout(data));
        }

        [TestMethod]
        public void ReadMetadata()
        {
            // Arrange
            Tuple<string, string> m1 = Tuple.Create("Description", "Test Layout");
            string data = CreateLayoutFromString("<align/>", m1);

            // Act
            BinaryLayout bl = new BinaryLayout(data);

            // Assert
            Assert.AreEqual(bl.Name, TestLayoutName);
            Assert.AreEqual(bl["name"], TestLayoutName);
            Assert.AreEqual(bl[m1.Item1], m1.Item2);
        }

        [TestMethod]
        public void Equals()
        {
            // TODO: Define equality as describing the same data rather than XML data matching

            // Arrange
            string data1 = CreateLayoutFromString("<int/><float/><char/><bool/>", Tuple.Create("Description", "Test Layout"));
            string data2 = CreateLayoutFromString("<int/><float/><char/><bool/>", Tuple.Create("Description", "Test Layout"));
            BinaryLayout l1 = new BinaryLayout(data1);
            BinaryLayout l2 = new BinaryLayout(data2);

            // Act
            bool equality1 = l1.Equals(l2);
            bool equality2 = l2.Equals(l1);

            // Assert
            Assert.IsTrue(equality1);
            Assert.IsTrue(equality2);
        }
    }
}
