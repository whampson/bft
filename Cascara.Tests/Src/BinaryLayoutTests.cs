using Cascara.Tests.Extensions;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using WHampson.Cascara;
using WHampson.Cascara.Interpreter;
using WHampson.Cascara.Interpreter.Xml;
using Xunit;
using Xunit.Abstractions;
using static WHampson.Cascara.Interpreter.ReservedWords;

namespace Cascara.Tests
{
    public class BinaryLayoutTests : CascaraTestFramework
    {
        private const string DefaultLayoutName = "TestLayout";

        public BinaryLayoutTests(ITestOutputHelper output)
            : base(output)
        {
        }

        private string CreateLayout(string body, params Tuple<string, string>[] attrs)
        {
            return CreateLayout(DefaultLayoutName, null, body, attrs);
        }

        private string CreateLayout(string name, string body, params Tuple<string, string>[] attrs)
        {
            return CreateLayout(name, null, body, attrs);
        }

        private string CreateLayout(string name, string version, string body, params Tuple<string, string>[] attrs)
        {
            List<Tuple<string, string>> allAttrs = new List<Tuple<string, string>>(attrs)
            {
                Tuple.Create(Parameters.Name, name)
            };

            if (version != null)
            {
                allAttrs.Add(Tuple.Create(Parameters.Version, version));
            }
            if (body == null)
            {
                body = "";
            }

            return BuildXmlElement(Keywords.XmlDocumentRoot, body, allAttrs.ToArray());
        }

        [Fact]
        public void Parse_Xml_Null()
        {
            // Arrange
            string src = null;

            // Act, assert
            Assert.Throws<ArgumentException>(() => BinaryLayout.Parse(src));
        }

        [Fact]
        public void Parse_Xml_Empty()
        {
            // Arrange
            string src = "";

            // Act, assert
            Assert.Throws<ArgumentException>(() => BinaryLayout.Parse(src));
        }

        [Fact]
        public void Parse_Xml_InvalidXml()
        {
            // Arrange
            string src = "not xml";

            // Act, assert
            AssertExtensions.ThrowsAnyWithMessage<LayoutException>(
                () => BinaryLayout.Parse(src),
                Resources.LayoutExceptionLayoutLoadFailure);
        }

        [Fact]
        public void Parse_Xml_BadRootName()
        {
            // Arrange
            string src = BuildXmlElement("badRoot");

            // Act, assert
            AssertExtensions.ThrowsAnyWithMessage<LayoutException>(
                () => BinaryLayout.Parse(src),
                Resources.SyntaxExceptionXmlInvalidRootElement, Keywords.XmlDocumentRoot);
        }

        [Fact]
        public void Parse_Xml_EmptyLayout()
        {
            // Arrange
            string src = BuildXmlElement(Keywords.XmlDocumentRoot);

            // Act, assert
            AssertExtensions.ThrowsAnyWithMessage<LayoutException>(
                () => BinaryLayout.Parse(src),
                Resources.SyntaxExceptionEmptyStructure);
        }

        [Fact]
        public void Parse_Xml_MissingName()
        {
            // Arrange
            string src = BuildXmlElement(Keywords.XmlDocumentRoot, BuildXmlElement(Keywords.Int));

            // Act, assert
            AssertExtensions.ThrowsAnyWithMessage<LayoutException>(
                () => BinaryLayout.Parse(src),
                Resources.SyntaxExceptionMissingLayoutName);
        }

        [Fact]
        public void Parse_Xml_MalformattedVersion()
        {
            // Arrange
            string badVersion = "not a version number";
            string src = CreateLayout("foo", badVersion, BuildXmlElement(Keywords.Int));

            // Act, assert
            AssertExtensions.ThrowsAnyWithMessage<LayoutException>(
                () => BinaryLayout.Parse(src),
                Resources.LayoutExceptionMalformattedLayoutVersion, badVersion);
        }
    }
}
