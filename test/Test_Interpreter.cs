using System;
using System.IO;
using WHampson.Cascara;
using WHampson.Cascara.Interpreter;
using Xunit;

namespace CascaraTests
{
    public class Test_Interpreter
    {
        [Fact]
        public void Interpreter_HelloWorld()
        {
            // Arrange
            string expOutput = "Hello, world!";
            string src = string.Format("<echo message='{0}'/>", expOutput);
            string script = BuildXmlLayoutScript(src);
            TestEnv test = new TestEnv(script);

            // Act
            test.Run();

            // Assert
            Assert.Equal(expOutput, test.Output);
        }

        [Fact]
        public void Interpreter_InvalidKeyword()
        {
            // Arrange
            string src = "<invalid/>";
            string script = BuildXmlLayoutScript(src);
            TestEnv test = new TestEnv(script);

            // Act, Assert
            Assert.Throws<LayoutScriptException>(() => test.Run()); // TODO: check message
        }

        [Fact]
        public void Interpreter_Struct_EmptyDeclaration()
        {
            // Arrange
            string src = @"
                <struct/>
            ";

            string script = BuildXmlLayoutScript(src);
            TestEnv test = new TestEnv(script);

            // Act, Assert
            Assert.ThrowsAny<SyntaxException>(() => test.Run());
        }

        [Fact]
        public void Interpreter_Union_EmptyDeclaration()
        {
            // Arrange
            string src = @"
                <union/>
            ";

            string script = BuildXmlLayoutScript(src);
            TestEnv test = new TestEnv(script);

            // Act, Assert
            Assert.ThrowsAny<SyntaxException>(() => test.Run());
        }

        [Fact]
        public void Interpreter_Echo_Primitive()
        {
            // Arrange
            uint val = 0xDEADBEEF;
            byte[] data = BitConverter.GetBytes(val);
            string expOutput = val.ToString();
            string src = @"
                <uint name='Foo'/>
                <echo message='${Foo}'/>
            ";

            string script = BuildXmlLayoutScript(src);
            TestEnv test = new TestEnv(script, data);

            // Act
            test.Run();

            // Assert
            Assert.Equal(expOutput, test.Output);
        }

        [Fact]
        public void Interpreter_Echo_StructureMembers()
        {
            // Arrange
            ulong val = 0xDEADBEEF00000000;
            byte[] data = BitConverter.GetBytes(val);
            string expOutput = string.Format(
                "Value1: {0}" + Environment.NewLine +
                "Value2: {1}" + Environment.NewLine +
                "Offset1: 0" + Environment.NewLine +
                "Offset2: 4",
                (val & 0xFFFFFFFF).ToString(), (val >> 32).ToString());

            string src = @"
                <struct name='MyStruct'>
                    <uint name='Zero'/>
                    <uint name='Beef'/>
                </struct>

                <echo message='Value1: ${MyStruct.Zero}'/>
                <echo message='Value2: ${MyStruct.Beef}'/>
                <echo message='Offset1: $OffsetOf(MyStruct.Zero)'/>
                <echo message='Offset2: $OffsetOf(MyStruct.Beef)'/>
            ";

            string script = BuildXmlLayoutScript(src);
            TestEnv test = new TestEnv(script, data);

            // Act
            test.Run();

            // Assert
            Assert.Equal(expOutput, test.Output);
        }

        [Fact]
        public void Interpreter_Echo_UnionMembers()
        {
            // Arrange
            ulong val = 0xDEADBEEF;
            byte[] data = BitConverter.GetBytes(val);
            string expOutput = string.Format(
                "Value1: {0}" + Environment.NewLine +
                "Value2: {1}" + Environment.NewLine +
                "Offset1: 0" + Environment.NewLine +
                "Offset2: 0",
                val.ToString(), val.ToString());

            string src = @"
                <union name='MyUnion'>
                    <uint name='Beef1'/>
                    <uint name='Beef2'/>
                </union>

                <echo message='Value1: ${MyUnion.Beef1}'/>
                <echo message='Value2: ${MyUnion.Beef2}'/>
                <echo message='Offset1: $OffsetOf(MyUnion.Beef1)'/>
                <echo message='Offset2: $OffsetOf(MyUnion.Beef2)'/>
            ";

            string script = BuildXmlLayoutScript(src);
            TestEnv test = new TestEnv(script, data);

            // Act
            test.Run();

            // Assert
            Assert.Equal(expOutput, test.Output);
        }

        [Fact]
        public void Interpreter_Typedef_Primitive()
        {
            // Arrange
            string expOutput = "Size: 4";
            string src = @"
                <typedef name='my_type' kind='int'/>
                <my_type name='Foo'/>
                <echo message='Size: $SizeOf(Foo)'/>
            ";

            string script = BuildXmlLayoutScript(src);
            TestEnv test = new TestEnv(script);

            // Act
            test.Run();

            // Assert
            Assert.Equal(expOutput, test.Output);
        }

        [Fact]
        public void Interpreter_Typedef_Primitive_Chain()
        {
            // Arrange
            string expOutput = "Size: 4";
            string src = @"
                <typedef name='my_type' kind='int'/>
                <typedef name='my_other_type' kind='my_type'/>

                <my_other_type name='Foo'/>

                <echo message='Size: $SizeOf(Foo)'/>
            ";

            string script = BuildXmlLayoutScript(src);
            TestEnv test = new TestEnv(script);

            // Act
            test.Run();

            // Assert
            Assert.Equal(expOutput, test.Output);
        }

        [Fact]
        public void Interpreter_Typedef_Structure()
        {
            // Arrange
            string expOutput =
                "Size: 8" + Environment.NewLine +
                "Offset1: 0" + Environment.NewLine +
                "Offset2: 4";
            string src = @"
                <typedef name='my_struct' kind='struct'>
                    <uint name='Foo'/>
                    <uint name='Bar'/>
                </typedef>

                <my_struct name='Foobar'/>

                <echo message='Size: $SizeOf(Foobar)'/>
                <echo message='Offset1: $OffsetOf(Foobar.Foo)'/>
                <echo message='Offset2: $OffsetOf(Foobar.Bar)'/>
            ";

            string script = BuildXmlLayoutScript(src);
            TestEnv test = new TestEnv(script);

            // Act
            test.Run();

            // Assert
            Assert.Equal(expOutput, test.Output);
        }

        [Fact]
        public void Interpreter_Typedef_Union()
        {
            // Arrange
            string expOutput =
                "Size: 4" + Environment.NewLine +
                "Offset1: 0" + Environment.NewLine +
                "Offset2: 0";
            string src = @"
                <typedef name='my_union' kind='union'>
                    <uint name='Foo'/>
                    <uint name='Bar'/>
                </typedef>

                <my_union name='Foobar'/>

                <echo message='Size: $SizeOf(Foobar)'/>
                <echo message='Offset1: $OffsetOf(Foobar.Foo)'/>
                <echo message='Offset2: $OffsetOf(Foobar.Bar)'/>
            ";

            string script = BuildXmlLayoutScript(src);
            TestEnv test = new TestEnv(script);

            // Act
            test.Run();

            // Assert
            Assert.Equal(expOutput, test.Output);
        }

        private string BuildXmlLayoutScript(string src, params Tuple<string, string>[] metadata)
        {
            const string DocRoot = ReservedWords.Keywords.XmlDocumentRoot;
            string xml = "<" + DocRoot;

            foreach (Tuple<string, string> p in metadata)
            {
                xml += string.Format(" {0}='{1}'", p.Item1, p.Item2);
            }

            if (src == null) {
                xml += "/>";
            }
            else {
                xml += string.Format(">{0}</{1}>", src, DocRoot);
            }

            return xml;
        }

        private class TestEnv
        {
            const int DefaultLength = 64;

            private StringWriter _outputWriter;
            private Random _rand;

            private TestEnv()
            {
                _outputWriter = new StringWriter();
                _rand = new Random();
            }

            public TestEnv(string scriptSrc) : this()
            {
                byte[] data = new byte[DefaultLength];
                _rand.NextBytes(data);

                Init(scriptSrc, data);
            }

            public TestEnv(string scriptSrc, int dataLen) : this()
            {
                byte[] data = new byte[dataLen];
                _rand.NextBytes(data);

                Init(scriptSrc, data);
            }

            public TestEnv(string scriptSrc, byte[] data) : this()
            {
                Init(scriptSrc, data);
            }

            private void Init(string scriptSrc, byte[] data)
            {
                Data = new BinaryData(data);
                SymbolTable = SymbolTable.CreateRootSymbolTable();
                Script = LayoutScript.Parse(scriptSrc);
                Interpreter = new LayoutInterpreter(Script, _outputWriter);
            }

            public void Run()
            {
                Interpreter.Execute(SymbolTable, Data);
            }

            public BinaryData Data { get; private set; }
            public SymbolTable SymbolTable { get; private set; }
            public string Output { get { return _outputWriter.ToString().Trim(); } }
            public LayoutScript Script { get; private set; }
            public LayoutInterpreter Interpreter { get; private set; }
        }
    }
}
