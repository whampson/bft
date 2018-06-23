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
                Data = new BinaryFile(data);
                SymbolTable = SymbolTable.CreateRootSymbolTable();
                Script = LayoutScript.Parse(scriptSrc);
                Interpreter = new LayoutInterpreter(Script, _outputWriter);
            }

            public void Run()
            {
                Interpreter.Execute(SymbolTable, Data);
            }

            public BinaryFile Data { get; private set; }
            public SymbolTable SymbolTable { get; private set; }
            public string Output { get { return _outputWriter.ToString().Trim(); } }
            public LayoutScript Script { get; private set; }
            public LayoutInterpreter Interpreter { get; private set; }
        }
    }
}
