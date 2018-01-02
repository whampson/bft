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

        [Fact(Skip = "Not yet implemented.")]
        public void Parse_Xml_UnsupportedVersion()
        {
            // TODO
        }

        [Fact]
        public void Parse_Xml_BadElementName()
        {
            // Arrange
            string badElem = "foo";
            string expectedInnerMessage = string.Format(Resources.SyntaxExceptionUnknownIdentifier, badElem);
            string src = CreateLayout(BuildXmlElement(badElem));

            // Act, assert
            var ex = Assert.ThrowsAny<LayoutException>(() => BinaryLayout.Parse(src));
            Assert.True(ex.InnerException is SyntaxException);
            var inner = (SyntaxException) ex.InnerException;
            Assert.Equal(expectedInnerMessage, inner.Message);

        }

        [Fact(Skip = "Ignore for now.")]
        public void ScratchArea()
        {
            const string XmlData = @"
        <cascaraLayout name='LCS Save Format' platform='PS2'>
            <typedef name='weather_t' kind='int16'/>
            <typedef name='blockheader_t' kind='struct'>
                <uint32 name='Size'/>
                <char name='Tag' count='4'/>
            </typedef>

            <struct name='SIMP'>
                <blockheader_t name='Header'/>
                <align kind='int32' count='32'/>
                <short/>
                <short/>
                <char name='SaveTitle' count='24'/>
                <weather_t name='OldWeather'/>
                <weather_t name='CurrentWeather'/>
                <struct name='GameSettings'>
                    <byte name='Brightness'/>
                    <byte name='Contrast'/>
                    <align count='2'/>
                    <bool32 name='HudStatus'/>
                </struct>
                <align count='${Header.Size} - ${__OFFSET__}'/>
            </struct>
            <struct name='SRPT'>
                <blockheader_t name='Header'/>
                <align count='${Header.Size} - ${__OFFSET__}'/>
            </struct>
            <struct name='PLYR'>
                <blockheader_t name='Header'/>
                <align count='${Header.Size} - ${__OFFSET__}'/>
            </struct>
            <struct name='GRGE'>
                <blockheader_t name='Header'/>
                <align count='${Header.Size} - ${__OFFSET__}'/>
            </struct>
            <struct name='STAT'>
                <blockheader_t name='Header'/>
                <align count='${Header.Size} - ${__OFFSET__}'/>
            </struct>
            <uint32 name='Checksum'/>
        </cascaraLayout>";

            BinaryLayout layout = BinaryLayout.Parse(XmlData);
            Output.WriteLine(layout.Name);
            Output.WriteLine(layout["platform"]);

            ProcessStatement(layout.RootStatement, 0);
        }

        private Dictionary<string, Statement> Typedefs = new Dictionary<string, Statement>();

        private void ProcessStatement(Statement stmt, int indent)
        {
            Output.WriteLine(stmt.ToString());
            bool hasName = stmt.Parameters.TryGetValue(Parameters.Name, out string name);
            bool hasKind = stmt.Parameters.TryGetValue(Parameters.Kind, out string kind);
            bool hasCount = stmt.Parameters.TryGetValue(Parameters.Count, out string countStr);

            if (hasName && stmt.Keyword == Keywords.Typedef)
            {
                XElement e = ((XmlStatement) stmt).Element;
                e.Name = kind;
                e.Attributes().Remove();
                Typedefs[name] = XmlStatement.Parse(e);
                return;
            }

            if (Typedefs.TryGetValue(stmt.Keyword, out Statement userType))
            {
                stmt = userType;
            }

            bool isStruct = stmt.Keyword == Keywords.Struct;

            //Output.WriteLine(string.Format("{0}{1}{2}{3}{4}",
            //    new string(' ', indent),
            //    stmt.Keyword,
            //    (hasCount) ? "[" + countStr + "]" : "",
            //    (hasName) ? " " + name : "",
            //    (isStruct) ? " {" : ";"));
            foreach (Statement s in stmt.NestedStatements)
            {
                ProcessStatement(s, indent + 4);
            }

            if (isStruct)
            {
                //Output.WriteLine(string.Format("{0}{1}",
                //    new string(' ', indent),
                //    "}"));
            }
        }
    }
}
