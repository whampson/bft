using Cascara.Tests.Extensions;
using System;
using System.Linq;
using System.Xml.Linq;
using WHampson.Cascara;
using WHampson.Cascara.Interpreter;
using WHampson.Cascara.Interpreter.Xml;
using Xunit;
using Xunit.Abstractions;
using static WHampson.Cascara.Interpreter.ReservedWords;

namespace Cascara.Tests
{
    public class XmlStatementTests : CascaraTestFramework
    {
        public XmlStatementTests(ITestOutputHelper output)
            : base(output)
        {
        }

        private string BuildXmlElement(string name, params Tuple<string, string>[] attrs)
        {
            return BuildXmlElement(name, "", attrs);
        }

        private string BuildXmlElement(string name, string innerData, params Tuple<string, string>[] attrs)
        {
            string attrString = "";
            foreach (Tuple<string, string> t in attrs)
            {
                attrString += string.Format(" {0}='{1}'", t.Item1, t.Item2);
            }

            return string.Format("<{0}{1}>{2}</{0}>", name, attrString, innerData);
        }

        private XElement Parse(string data)
        {
            XDocument doc = XDocument.Parse(data, LoadOptions.SetLineInfo);

            return doc.Root;
        }

        [Fact]
        public void Parse_Simple()
        {
            // Arrange
            string expectedKeyword = Keywords.Int;
            bool expectedHasParameters = false;
            int expectedParameterCount = 0;
            bool expectedHasNestedStatements = false;
            int expectedNestedStatementsCount = 0;
            StatementType expectedType = StatementType.FileObjectDefinition;
            string data = BuildXmlElement(expectedKeyword);
            XElement elem = Parse(data);

            // Act
            Statement stmt = XmlStatement.Parse(elem);
            Output.WriteLine(stmt.ToString());

            // Assert
            Assert.Equal(stmt.Keyword, expectedKeyword);
            Assert.Equal(stmt.HasParameters, expectedHasParameters);
            Assert.Equal(stmt.Parameters.Count, expectedParameterCount);
            Assert.Equal(stmt.HasNestedStatements, expectedHasNestedStatements);
            Assert.Equal(stmt.NestedStatements.Count(), expectedNestedStatementsCount);
            Assert.Equal(stmt.StatementType, expectedType);
        }

        [Fact]
        public void Parse_Complex()
        {
            // Arrange
            string expectedKeywordParent = Keywords.Struct;
            string[] expectedKeywordChild = { Keywords.Int, Keywords.Float };
            bool expectedHasParameters = false;
            int expectedParameterCount = 0;
            bool expectedHasNestedStatements = true;
            int expectedNestedStatementsCount = 2;
            StatementType expectedType = StatementType.FileObjectDefinition;
            string data = BuildXmlElement(expectedKeywordParent,
                BuildXmlElement(expectedKeywordChild[0]) + BuildXmlElement(expectedKeywordChild[1]));
            XElement elem = Parse(data);

            // Act
            Statement stmt = XmlStatement.Parse(elem);
            Output.WriteLine(stmt.ToString());

            // Assert
            Assert.Equal(stmt.Keyword, expectedKeywordParent);
            Assert.Equal(stmt.HasParameters, expectedHasParameters);
            Assert.Equal(stmt.Parameters.Count, expectedParameterCount);
            Assert.Equal(stmt.HasNestedStatements, expectedHasNestedStatements);
            Assert.Equal(stmt.NestedStatements.Count(), expectedNestedStatementsCount);
            Assert.Equal(stmt.StatementType, expectedType);

            for (int i = 0; i < stmt.NestedStatements.Count(); i++)
            {
                Statement childStmt = stmt.NestedStatements.ElementAt(i);
                Assert.Equal(childStmt.Keyword, expectedKeywordChild[i]);
            }
        }

        [Fact]
        public void Parse_Invalid_BadParams()
        {
            // Arrange
            string data1 = BuildXmlElement("local", Tuple.Create("name", "foo"));
            string data2 = BuildXmlElement("align", Tuple.Create("bogus", "true"));
            XElement elem1 = Parse(data1);
            XElement elem2 = Parse(data2);
            string expectedMissing = Parameters.Value;
            string expectedUnknown = "bogus";

            // Act, assert
            AssertExtensions.ThrowsWithMessage<SyntaxException>(
                () => XmlStatement.Parse(elem1),
                Resources.SyntaxExceptionMissingRequiredParameter, expectedMissing);

            AssertExtensions.ThrowsWithMessage<SyntaxException>(
                () => XmlStatement.Parse(elem2),
                Resources.SyntaxExceptionUnknownIdentifier, expectedUnknown);
        }

        [Fact]
        public void Parse_Invalid_UnknownKeyword()
        {
            // Arrange
            string identifier = "foo";
            string data = BuildXmlElement(identifier);
            XElement elem = Parse(data);

            // Act, assert
            AssertExtensions.ThrowsWithMessage<SyntaxException>(
                () => XmlStatement.Parse(elem),
                Resources.SyntaxExceptionUnknownIdentifier, identifier);
        }

        [Fact]
        public void Parse_Invalid_RootElementUsage()
        {
            // Arrange
            string identifier = Keywords.XmlDocumentRoot;
            string data = BuildXmlElement(identifier);
            XElement elem = Parse(data);

            // Act, assert
            AssertExtensions.ThrowsWithMessage<SyntaxException>(
                () => XmlStatement.Parse(elem),
                Resources.SyntaxExceptionXmlInvalidUsageOfRootElement, identifier);
        }

        [Fact]
        public void Parse_Invalid_EmptyStructure()
        {
            string data = BuildXmlElement("struct");
            XElement elem = Parse(data);

            // Act, assert
            AssertExtensions.ThrowsWithMessage<SyntaxException>(
                () => XmlStatement.Parse(elem),
                Resources.SyntaxExceptionEmptyStructure);
        }

        [Fact]
        public void Parse_Invalid_UnexpectedText()
        {
            string text = "Hello, world!";
            string elemName = Keywords.Int;
            string data = BuildXmlElement(elemName, text);
            XElement elem = Parse(data);

            // Act, assert
            AssertExtensions.ThrowsWithMessage<SyntaxException>(
                () => XmlStatement.Parse(elem),
                Resources.SyntaxExceptionXmlUnexpectedText, text, elemName);
        }
    }
}
