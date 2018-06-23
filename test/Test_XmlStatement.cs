using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using WHampson.Cascara.Interpreter;
using WHampson.Cascara.Interpreter.Xml;
using Xunit;

namespace CascaraTests
{
    public class Test_XmlStatement
    {
        class ExpectedStatement
        {
            public ExpectedStatement(params Tuple<string, string>[] parameters)
            {
                Parameters = new Dictionary<string, string>();
                Nested = new List<ExpectedStatement>();

                foreach (var p in parameters) {
                    Parameters.Add(p.Item1, p.Item2);
                }
            }

            public string Keyword { get; set; }
            public StatementType Type { get; set; }
            public bool HasParameters { get; set; }
            public bool HasNested { get; set; }
            public Dictionary<string, string> Parameters { get; }
            public List<ExpectedStatement> Nested { get; }
        }

        [Fact]
        public void XmlStatement_FileObject()
        {
            // Arrange
            ExpectedStatement expStmt = new ExpectedStatement();
            expStmt.Keyword = ReservedWords.Keywords.DataTypes.Int;
            expStmt.Type = StatementType.FileObjectDefinition;
            expStmt.HasParameters = false;
            expStmt.HasNested = false;

            string testXml = BuildXmlElement(expStmt.Keyword);

            // Act
            XmlStatement testStmt = XmlStatement.Parse(XElement.Parse(testXml));

            // Assert
            StatementAssertEqual(expStmt, testStmt);
        }

        [Fact]
        public void XmlStatement_Directive()
        {
            // Arrange
            ExpectedStatement expStmt = new ExpectedStatement();
            expStmt.Keyword = ReservedWords.Keywords.Directives.Typedef;
            expStmt.Type = StatementType.Directive;
            expStmt.HasParameters = false;
            expStmt.HasNested = false;

            string testXml = BuildXmlElement(expStmt.Keyword);

            // Act
            XmlStatement testStmt = XmlStatement.Parse(XElement.Parse(testXml));

            // Assert
            StatementAssertEqual(expStmt, testStmt);
        }

        [Fact]
        public void XmlStatement_WithParameters()
        {
            // Arrange
            Tuple<string, string>[] expParams = new Tuple<string, string>[]
            {
                new Tuple<string, string>("name", "Foo"),
                new Tuple<string, string>("count", "4")
            };
            ExpectedStatement expStmt = new ExpectedStatement(expParams);
            expStmt.Keyword = ReservedWords.Keywords.DataTypes.Int;
            expStmt.Type = StatementType.FileObjectDefinition;
            expStmt.HasParameters = true;
            expStmt.HasNested = false;

            string testXml = BuildXmlElement(expStmt.Keyword, expParams);

            // Act
            XmlStatement testStmt = XmlStatement.Parse(XElement.Parse(testXml));

            // Assert
            StatementAssertEqual(expStmt, testStmt);
        }

        [Fact]
        public void XmlStatement_Nested()
        {
            // Arrange
            ExpectedStatement expInnerStmt = new ExpectedStatement();
            expInnerStmt.Keyword = ReservedWords.Keywords.DataTypes.Int;
            expInnerStmt.Type = StatementType.FileObjectDefinition;
            expInnerStmt.HasParameters = false;
            expInnerStmt.HasNested = false;

            ExpectedStatement expOuterStmt = new ExpectedStatement();
            expOuterStmt.Keyword = ReservedWords.Keywords.DataTypes.Struct;
            expOuterStmt.Type = StatementType.FileObjectDefinition;
            expOuterStmt.HasParameters = false;
            expOuterStmt.HasNested = true;
            expOuterStmt.Nested.Add(expInnerStmt);

            // <struct><int/></struct>
            string testInnerXml = BuildXmlElement(expInnerStmt.Keyword);
            string testOuterXml = BuildXmlElement(expOuterStmt.Keyword, testInnerXml);

            // Act
            XmlStatement testStmt = XmlStatement.Parse(XElement.Parse(testOuterXml));

            // Assert
            StatementAssertEqual(expOuterStmt, testStmt);
        }

        [Theory]
        [InlineData("<int/>", "<int/>", true)]
        [InlineData("<int name='Foo'/>", "<int/>", false)]
        [InlineData("<int name='Foo'/>", "<int name='bar'/>", false)]
        [InlineData("<int name='Foo'/>", "<int name='foo'/>", false)]
        [InlineData("<int name='Foo'/>", "<int name='Foo'/>", true)]
        [InlineData("<int name='Foo' count='4'/>", "<int name='Foo'/>", false)]
        [InlineData("<int name='Foo' count='4'/>", "<int count='4' name='Foo'/>", true)]
        [InlineData("<struct><int/></struct>", "<struct><int/></struct>", true)]
        [InlineData("<struct><int/></struct>", "<struct><float/></struct>", false)]
        [InlineData("<struct><int/><float/></struct>", "<struct><float/><int/></struct>", false)]
        [InlineData("<struct><int/><float/></struct>", "<struct><int/><float/></struct>", true)]
        public void XmlStatement_Equality(string leftXml, string rightXml, bool expResult)
        {
            // Arrange
            XmlStatement leftStmt = XmlStatement.Parse(XElement.Parse(leftXml));
            XmlStatement rightStmt = XmlStatement.Parse(XElement.Parse(rightXml));

            // Act
            bool result1 = leftStmt.Equals(rightStmt);
            bool result2 = rightStmt.Equals(leftStmt);
            int hash1 = leftStmt.GetHashCode();
            int hash2 = rightStmt.GetHashCode();

            // Assert
            Assert.Equal(expResult, result1);
            Assert.Equal(expResult, result2);
            if (expResult == true) Assert.True(hash1 == hash2);
            else Assert.False(hash1 == hash2);
        }

        [Fact]
        public void XmlStatement_InnerText()
        {
            // Arrange
            string testXml = BuildXmlElement("int", "This is invalid.");

            // Act, Assert
            Assert.Throws<SyntaxException>(() => XmlStatement.Parse(XElement.Parse(testXml)));
        }

        private string BuildXmlElement(string name, params Tuple<string, string>[] parameters)
        {
            return BuildXmlElement(name, null, parameters);
        }

        private string BuildXmlElement(string name, string innerData, params Tuple<string, string>[] parameters)
        {
            string xml = "<" + name;

            foreach (Tuple<string, string> p in parameters)
            {
                xml += string.Format(" {0}='{1}'", p.Item1, p.Item2);
            }

            if (innerData == null) {
                xml += "/>";
            }
            else {
                xml += string.Format(">{0}</{1}>", innerData, name);
            }

            return xml;
        }

        private void StatementAssertEqual(ExpectedStatement expected, Statement actual)
        {
            Assert.Equal(expected.Keyword, actual.Keyword);
            Assert.Equal(expected.Type, actual.StatementType);
            Assert.Equal(expected.HasParameters, actual.HasParameters);
            Assert.Equal(expected.HasNested, actual.HasNestedStatements);
            Assert.Equal(expected.Parameters, actual.Parameters);
            Assert.Equal(expected.Nested.Count(), actual.NestedStatements.Count());
            for (int i = 0; i < expected.Nested.Count(); i++) {
                StatementAssertEqual(expected.Nested.ElementAt(i), actual.NestedStatements.ElementAt(i));
            }
        }
    }
}
