using System.Xml.Linq;
using WHampson.Cascara.Interpreter;
using WHampson.Cascara.Interpreter.Xml;
using Xunit;
using Xunit.Abstractions;

namespace Cascara.Tests
{
    public class StatementTests : CascaraTestFramework
    {
        public StatementTests(ITestOutputHelper output)
            : base(output)
        {
        }

        private Statement CreateXmlStatement(string xml)
        {
            XDocument doc = XDocument.Parse(xml, LoadOptions.SetLineInfo);

            return XmlStatement.Parse(doc.Root);
        }

        // TODO: make this cleaner and more readable! 
        [Theory]
        [InlineData("<int/>", "<int/>", true)]
        [InlineData("<int/>", "<int name='foo'/>", false)]
        [InlineData("<int count='4' name='foo'/>", "<int name='foo' count='4'/>", true)]
        [InlineData("<int/>", "<char/>", false)]
        [InlineData("<struct><float/></struct>", "<struct><float/></struct>", true)]
        [InlineData("<struct><float/></struct>", "<struct><int/></struct>", false)]
        [InlineData("<typedef name='my_t' kind='struct'><float name='foo' comment='bar'/></typedef>",
            "<typedef kind='struct' name='my_t'><float comment='bar' name='foo'/></typedef>", true)]
        [InlineData("<struct><int/><float/></struct>", "<struct><int/><float/></struct>", true)]
        [InlineData("<struct><float/><float/></struct>", "<struct><int/><float/></struct>", false)]
        [InlineData("<struct><int/><float/></struct>", "<struct><int name='foo'/><float/></struct>", false)]
        public void Equality(string leftXml, string rightXml, bool expectedResult)
        {
            // Arrange
            Statement stmtA = CreateXmlStatement(leftXml);
            Statement stmtB = CreateXmlStatement(rightXml);

            // Act
            bool result1 = stmtA.Equals(stmtB);
            bool result2 = stmtB.Equals(stmtA);

            // Assert
            Assert.Equal(result1, expectedResult);
            Assert.Equal(result2, expectedResult);
            if (expectedResult == true)
            {
                Assert.Equal(stmtA.GetHashCode(), stmtB.GetHashCode());
            }

        }
    }
}
