using System;
using System.Linq;
using System.Xml.Linq;
using WHampson.Cascara;
using Xunit;
using Xunit.Abstractions;

namespace Cascara.Tests
{
    public class XmlStatementTests : CascaraTestFramework
    {
        public XmlStatementTests(ITestOutputHelper output)
            : base(output)
        {
        }

        private XElement CreateXmlElement(string data)
        {
            XDocument doc = XDocument.Parse(data, LoadOptions.SetLineInfo);

            return doc.Root;
        }

        [Fact]
        public void TestCreate()
        {
            // Arrange
            string expectedKeyword = "foo";
            bool expectedHasParameters = false;
            int expectedParameterCount = 0;
            bool expectedHasNestedStatements = false;
            int expectedNestedStatementsCount = 0;
            XElement elem = CreateXmlElement("<foo/>");

            // Act
            Statement stmt = new XmlStatement(elem);
            Output.WriteLine(stmt.ToString());

            // Assert
            Assert.Equal(stmt.Keyword, expectedKeyword);
            Assert.Equal(stmt.HasParameters, expectedHasParameters);
            Assert.Equal(stmt.Parameters.Count, expectedParameterCount);
            Assert.Equal(stmt.HasNestedStatements, expectedHasNestedStatements);
            Assert.Equal(stmt.NestedStatements.Count(), expectedNestedStatementsCount);
        }

        [Fact]
        public void TestCreateParametered()
        {
            // Arrange
            string expectedKeyword = "int";
            bool expectedHasParameters = true;
            int expectedParameterCount = 2;
            bool expectedHasNestedStatements = false;
            int expectedNestedStatementsCount = 0;
            Tuple<string, string> expectedParam1 = Tuple.Create("name", "foo");
            Tuple<string, string> expectedParam2 = Tuple.Create("count", "5");
            XElement elem = CreateXmlElement("<int name='foo' count='5'/>");

            // Act
            Statement stmt = new XmlStatement(elem);
            Output.WriteLine(stmt.ToString());

            // Assert
            Assert.Equal(stmt.Keyword, expectedKeyword);
            Assert.Equal(stmt.HasParameters, expectedHasParameters);
            Assert.Equal(stmt.Parameters.Count, expectedParameterCount);
            Assert.Equal(stmt.HasNestedStatements, expectedHasNestedStatements);
            Assert.Equal(stmt.NestedStatements.Count(), expectedNestedStatementsCount);
            Assert.Equal(stmt.Parameters[expectedParam1.Item1], expectedParam1.Item2);
            Assert.Equal(stmt.Parameters[expectedParam2.Item1], expectedParam2.Item2);
        }

        [Fact]
        public void TestCreateNested()
        {
            // Arrange
            string expectedKeywordParent = "struct";
            string[] expectedKeywordChild = { "int", "float" };
            bool expectedHasParameters = false;
            int expectedParameterCount = 0;
            bool expectedHasNestedStatements = true;
            int expectedNestedStatementsCount = 2;
            XElement elem = CreateXmlElement("<struct><int/><float/></struct>");

            // Act
            Statement stmt = new XmlStatement(elem);
            Output.WriteLine(stmt.ToString());

            // Assert
            Assert.Equal(stmt.Keyword, expectedKeywordParent);
            Assert.Equal(stmt.HasParameters, expectedHasParameters);
            Assert.Equal(stmt.Parameters.Count, expectedParameterCount);
            Assert.Equal(stmt.HasNestedStatements, expectedHasNestedStatements);
            Assert.Equal(stmt.NestedStatements.Count(), expectedNestedStatementsCount);

            for (int i = 0; i < stmt.NestedStatements.Count(); i++)
            {
                Statement childStmt = stmt.NestedStatements.ElementAt(i);
                Assert.Equal(childStmt.Keyword, expectedKeywordChild[i]);
            }
        }

        [Fact]
        public void TestCreateWithText()
        {
            // Arrange
            XElement elem = CreateXmlElement("<string>Hello, world!</string>");

            // Act, assert
            var ex = Assert.Throws<SyntaxException>(() => new XmlStatement(elem));
        }
    }
}
