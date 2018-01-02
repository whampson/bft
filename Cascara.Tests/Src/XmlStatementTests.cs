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
            XElement elem = ParseXmlElement(data);

            // Act
            Statement stmt = XmlStatement.Parse(elem);
            Output.WriteLine(stmt.ToString());

            // Assert
            Assert.Equal(expectedKeyword, stmt.Keyword);
            Assert.Equal(expectedHasParameters, stmt.HasParameters);
            Assert.Equal(expectedParameterCount, stmt.Parameters.Count);
            Assert.Equal(expectedHasNestedStatements, stmt.HasNestedStatements);
            Assert.Equal(expectedNestedStatementsCount, stmt.NestedStatements.Count());
            Assert.Equal(expectedType, stmt.StatementType);
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
            XElement elem = ParseXmlElement(data);

            // Act
            Statement stmt = XmlStatement.Parse(elem);
            Output.WriteLine(stmt.ToString());

            // Assert
            Assert.Equal(expectedKeywordParent, stmt.Keyword);
            Assert.Equal(expectedHasParameters, stmt.HasParameters);
            Assert.Equal(expectedParameterCount, stmt.Parameters.Count);
            Assert.Equal(expectedHasNestedStatements, stmt.HasNestedStatements);
            Assert.Equal(expectedNestedStatementsCount, stmt.NestedStatements.Count());
            Assert.Equal(expectedType, stmt.StatementType);

            for (int i = 0; i < stmt.NestedStatements.Count(); i++)
            {
                Statement childStmt = stmt.NestedStatements.ElementAt(i);
                Assert.Equal(expectedKeywordChild[i], childStmt.Keyword);
            }
        }

        [Fact]
        public void Parse_Invalid_BadParams()
        {
            // Arrange
            string data1 = BuildXmlElement(Keywords.Local, Tuple.Create("name", "foo"));
            string data2 = BuildXmlElement(Keywords.Align, Tuple.Create("bogus", "true"));
            XElement elem1 = ParseXmlElement(data1);
            XElement elem2 = ParseXmlElement(data2);
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
            XElement elem = ParseXmlElement(data);

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
            XElement elem = ParseXmlElement(data);

            // Act, assert
            AssertExtensions.ThrowsWithMessage<SyntaxException>(
                () => XmlStatement.Parse(elem),
                Resources.SyntaxExceptionXmlInvalidUsageOfRootElement, identifier);
        }

        [Fact]
        public void Parse_Invalid_EmptyStructure()
        {
            string data = BuildXmlElement(Keywords.Struct);
            XElement elem = ParseXmlElement(data);

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
            XElement elem = ParseXmlElement(data);

            // Act, assert
            AssertExtensions.ThrowsWithMessage<SyntaxException>(
                () => XmlStatement.Parse(elem),
                Resources.SyntaxExceptionXmlUnexpectedText, text, elemName);
        }
    }
}
