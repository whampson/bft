using System;
using WHampson.Cascara;
using Xunit;
using Xunit.Abstractions;

namespace Cascara.Tests
{
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

        private static string CreateLayout(string xmlBody, params Tuple<string, string>[] metadata)
        {
            return CreateLayout(TestLayoutName, xmlBody, metadata);
        }

        private static string CreateLayout(string name, string xmlBody, params Tuple<string, string>[] metadata)
        {
            string meta = "";
            foreach (Tuple<string, string> t in metadata)
            {
                meta += string.Format(" {0}='{1}'", t.Item1, t.Item2);
            }

            return string.Format("<{0} name='{1}'{2}>{3}</{0}>",
                Keywords.DocumentRoot, name, meta, xmlBody);
        }

        public BinaryLayoutTests(ITestOutputHelper output)
        {
            Output = output;
        }

        private ITestOutputHelper Output
        {
            get;
        }

        [Fact]
        public void EmptyLayout()
        {
            // Arrange
            string xmlData = CreateLayout("");

            // Act, assert
            AssertExtension.ThrowsWithMessage<LayoutException>(
                () => BinaryLayout.Create(xmlData),
                Resources.LayoutExceptionEmptyLayout);
        }

        [Fact]
        public void InvalidElement()
        {
            // Arrange
            string elemName = "badElem";
            string xmlData = CreateLayout("<" + elemName + "/>");

            // Act, assert
            AssertExtension.ThrowsWithMessage<LayoutException>(
                () => BinaryLayout.Create(xmlData),
                string.Format(Resources.LayoutExceptionUnknownType, elemName));
        }

        [Fact]
        public void ReadMetadata()
        {
            // Arrange
            Tuple<string, string> m1 = Tuple.Create("description", "Test Layout");
            string data = CreateLayout("<align/>", m1);

            // Act
            BinaryLayout bl = BinaryLayout.Create(data);

            // Assert
            Assert.Equal(bl.Name, TestLayoutName);
            Assert.Equal(bl["name"], TestLayoutName);
            Assert.Equal(bl[m1.Item1], m1.Item2);
        }

        //[TestMethod]
        //public void Equals()
        //{
        //    // TODO: Define equality as describing the same data rather than XML data matching

        //    // Arrange
        //    string data1 = CreateLayoutFromString("<int/><float/><char/><bool/>", Tuple.Create("Description", "Test Layout"));
        //    string data2 = CreateLayoutFromString("<int/><float/><char/><bool/>", Tuple.Create("Description", "Test Layout"));
        //    BinaryLayout l1 = new BinaryLayout(data1);
        //    BinaryLayout l2 = new BinaryLayout(data2);

        //    // Act
        //    bool equality1 = l1.Equals(l2);
        //    bool equality2 = l2.Equals(l1);

        //    // Assert
        //    Assert.IsTrue(equality1);
        //    Assert.IsTrue(equality2);
        //}
    }
}
