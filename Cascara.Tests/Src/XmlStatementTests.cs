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
            string expectedKeyword = Keywords.DataTypes.Int;
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
            string expectedKeywordParent = Keywords.DataTypes.Struct;
            string[] expectedKeywordChild = { Keywords.DataTypes.Int, Keywords.DataTypes.Float };
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
        public void Parse_Invalid_UnexpectedText()
        {
            string text = "Hello, world!";
            string elemName = Keywords.DataTypes.Int;
            string data = BuildXmlElement("outer", BuildXmlElement(elemName, text));
            XElement elem = ParseXmlElement(data);

            // Act, assert
            AssertExtensions.ThrowsWithMessageContaining<SyntaxException>(
                () => XmlStatement.Parse(elem),
                Resources.SyntaxExceptionXmlUnexpectedText, text, elemName);
        }
    }
}
