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
        public BinaryLayoutTests(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        public void Parse_Xml_Null()
        {

        }

        [Fact]
        public void Parse_Xml_Empty()
        {

        }

        [Fact]
        public void Parse_Xml_InvalidXml()
        {

        }

        [Fact]
        public void Parse_Xml_MissingName()
        {

        }

        [Fact]
        public void Parse_Xml_MalformattedVersion()
        {

        }

        [Fact]
        public void Parse_Xml_EmptyLayout()
        {

        }

        [Fact]
        public void Parse_Xml_BadElementName()
        {

        }

        //        [Fact]
        //        public void ScratchArea()
        //        {
        //            const string XmlData = @"
        //<cascaraLayout name='LCS Save Format' platform='PS2'>
        //    <typedef name='weather_t' kind='int16'/>
        //    <typedef name='blockheader_t' kind='struct'>
        //        <uint32 name='Size'/>
        //        <char name='Tag' count='4'/>
        //    </typedef>

        //    <struct name='SIMP'>
        //        <blockheader_t name='Header'/>
        //        <align kind='int32' count='32'/>
        //        <short/>
        //        <short/>
        //        <char name='SaveTitle' count='24'/>
        //        <weather_t name='OldWeather'/>
        //        <weather_t name='CurrentWeather'/>
        //        <struct name='GameSettings'>
        //            <byte name='Brightness'/>
        //            <byte name='Contrast'/>
        //            <align count='2'/>
        //            <bool32 name='HudStatus'/>
        //        </struct>
        //        <align count='${Header.Size} - ${__OFFSET__}'/>
        //    </struct>
        //    <struct name='SRPT'>
        //        <blockheader_t name='Header'/>
        //        <align count='${Header.Size} - ${__OFFSET__}'/>
        //    </struct>
        //    <struct name='PLYR'>
        //        <blockheader_t name='Header'/>
        //        <align count='${Header.Size} - ${__OFFSET__}'/>
        //    </struct>
        //    <struct name='GRGE'>
        //        <blockheader_t name='Header'/>
        //        <align count='${Header.Size} - ${__OFFSET__}'/>
        //    </struct>
        //    <struct name='STAT'>
        //        <blockheader_t name='Header'/>
        //        <align count='${Header.Size} - ${__OFFSET__}'/>
        //    </struct>
        //    <uint32 name='Checksum'/>
        //</cascaraLayout>";

        //            BinaryLayout layout = BinaryLayout.Parse(XmlData);
        //            Output.WriteLine(layout.Name);
        //            Output.WriteLine(layout["platform"]);

        //            ProcessStatement(layout.RootStatement, 0);
        //        }

        //        private Dictionary<string, Statement> Typedefs = new Dictionary<string, Statement>();

        //        private void ProcessStatement(Statement stmt, int indent)
        //        {
        //            Output.WriteLine(stmt.ToString());
        //            bool hasName = stmt.Parameters.TryGetValue(Parameters.Name, out string name);
        //            bool hasKind = stmt.Parameters.TryGetValue(Parameters.Kind, out string kind);
        //            bool hasCount = stmt.Parameters.TryGetValue(Parameters.Count, out string countStr);

        //            if (hasName && stmt.Keyword == Keywords.Typedef)
        //            {
        //                XElement e = ((XmlStatement) stmt).Element;
        //                e.Name = kind;
        //                e.Attributes().Remove();
        //                Typedefs[name] = XmlStatement.Create(e);
        //                return;
        //            }

        //            if (Typedefs.TryGetValue(stmt.Keyword, out Statement userType))
        //            {
        //                stmt = userType;
        //            }

        //            bool isStruct = stmt.Keyword == Keywords.Struct;

        //            //Output.WriteLine(string.Format("{0}{1}{2}{3}{4}",
        //            //    new string(' ', indent),
        //            //    stmt.Keyword,
        //            //    (hasCount) ? "[" + countStr + "]" : "",
        //            //    (hasName) ? " " + name : "",
        //            //    (isStruct) ? " {" : ";"));
        //            foreach (Statement s in stmt.NestedStatements)
        //            {
        //                ProcessStatement(s, indent + 4);
        //            }

        //            if (isStruct)
        //            {
        //                //Output.WriteLine(string.Format("{0}{1}",
        //                //    new string(' ', indent),
        //                //    "}"));
        //            }
        //        }
    }
}
