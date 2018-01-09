#region License
/* Copyright (c) 2017-2018 Wes Hampson
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using static WHampson.Cascara.Interpreter.ReservedWords;

namespace WHampson.Cascara.Interpreter
{
    internal sealed class LayoutInterpreter
    {
        private static readonly HashSet<Version> SupportedVersions = new HashSet<Version>(new Version[]
        {
            //new Version("1.0.0"),
            AssemblyInfo.AssemblyVersion        // current version
        });

        public enum Directive
        {
            Align,
            Echo,
            Include
        }

        private delegate void StatementTypeAction(Statement stmt);
        private delegate void DirectiveAction(Statement stmt);

        private int offset;
        private int dataLength;

        private BinaryLayout layout;
        private Stack<Symbol> symbolStack;
        private HashSet<BinaryLayout> includedLayouts;
        private TextWriter echoWriter;

        private Dictionary<StatementType, StatementTypeAction> statementTypeActionMap;
        private Dictionary<string, DirectiveAction> directiveActionMap;

        private Dictionary<string, CascaraTypeInfo> userDefinedTypes;

        public LayoutInterpreter(BinaryLayout layout)
            : this(layout, Console.Out)
        {
        }

        public LayoutInterpreter(BinaryLayout layout, TextWriter echoWriter)
        {
            if (!SupportedVersions.Contains(layout.Version))
            {
                string msg = "This layout version is not supported by this version of Cascara.";
                throw new NotSupportedException(msg);
            }

            offset = 0;
            dataLength = 0;

            this.layout = layout;
            this.echoWriter = echoWriter;
            symbolStack = new Stack<Symbol>();
            includedLayouts = new HashSet<BinaryLayout>();

            statementTypeActionMap = new Dictionary<StatementType, StatementTypeAction>();
            directiveActionMap = new Dictionary<string, DirectiveAction>();

            userDefinedTypes = new Dictionary<string, CascaraTypeInfo>();

            InitializeInterpreterActionMaps();
        }

        public void Reset()
        {
            symbolStack.Clear();
            includedLayouts.Clear();
            offset = 0;
            dataLength = 0;
            userDefinedTypes.Clear();
        }

        public void Execute(Symbol rootSymbol, int dataLength)
        {
            this.dataLength = dataLength;
            symbolStack.Push(rootSymbol);
            includedLayouts.Add(layout);

            InterpretRootStatement(layout.RootStatement);
        }

        private void InterpretRootStatement(Statement stmt)
        {
            if (stmt.Keyword != Keywords.XmlDocumentRoot)
            {
                // TODO: exception
                return;
            }

            foreach (Statement childStmt in stmt.NestedStatements)
            {
                InterpretStatement(childStmt);
            }

        }

        private void InterpretStatement(Statement stmt)
        {
            // TODO: error checking
            StatementTypeAction interpret = statementTypeActionMap[stmt.StatementType];
            interpret(stmt);
        }

        private void InterpretFileObjectDefinition(Statement stmt)
        {
            Console.WriteLine("{0} at 0x{1:X2}", stmt.Keyword, offset);

            bool hasCount = stmt.Parameters.TryGetValue(Parameters.Count, out string countStr);
            bool hasName = stmt.Parameters.TryGetValue(Parameters.Name, out string nameStr);

            int count = 1;

            if (hasCount)
            {
                // TODO: resolve variables
                try
                {
                    count = (int) EvaluateExpression(countStr);
                }
                catch (SyntaxErrorException e)
                {
                    string msg = "Syntax error in expression.";
                    throw LayoutException.Create<SyntaxException>(layout, e, stmt, msg);
                }
            }

            if (stmt.Keyword == Keywords.Struct || stmt.Keyword == Keywords.Union)
            {
                int startOffset = offset;
                if (hasName)
                {
                    // TODO: handle collections
                    Symbol sym = symbolStack.Peek().Insert(nameStr);
                    sym.DataOffset = offset;
                    symbolStack.Push(sym);
                }
                else
                {
                    symbolStack.Push(Symbol.CreateNamelessSymbol(symbolStack.Peek()));
                }

                foreach (Statement childStmt in stmt.NestedStatements)
                {
                    InterpretStatement(childStmt);
                }

                if (hasName)
                {
                    Symbol sym = symbolStack.Pop();
                    sym.DataLength = offset - startOffset;
                }
            }
            else
            {
                if (!TryLookupType(stmt.Keyword, out CascaraTypeInfo typeInfo))
                {
                    string msg = string.Format("Unknown type '{0}'", stmt.Keyword);
                    throw LayoutException.Create<SyntaxException>(layout, stmt, msg);
                }

                if (hasName)
                {
                    Symbol sym = symbolStack.Peek().Insert(nameStr, (hasCount) ? count : 0);
                    sym.DataOffset = offset;
                    sym.DataLength = typeInfo.Size;
                    sym.DataType = typeInfo.NativeType;
                }

                for (int i = 0; i < count; i++)
                {
                    offset += typeInfo.Size;
                }
            }
        }

        private void InterpretTypeDefinition(Statement stmt) { }
        private void InterpretLocalVariableDefinition(Statement stmt) { }
        private void InterpretDirective(Statement stmt)
        {
            // TODO: error checking
            DirectiveAction interpret = directiveActionMap[stmt.Keyword];
            interpret(stmt);
        }

        private void InterpretAlign(Statement stmt)
        {
            int count = 1;
            int unitSize = 1;

            bool hasCount = stmt.Parameters.TryGetValue(Parameters.Count, out string countStr);
            bool hasKind = stmt.Parameters.TryGetValue(Parameters.Kind, out string kindStr);

            if (hasCount)
            {
                // TODO: resolve variables
                try
                {
                    count = (int) EvaluateExpression(countStr);
                }
                catch (SyntaxErrorException e)
                {
                    string msg = "Syntax error in expression.";
                    throw LayoutException.Create<SyntaxException>(layout, e, stmt, msg);
                }
            }

            if (hasKind)
            {
                if (!TryLookupType(kindStr, out CascaraTypeInfo typeInfo))
                {
                    string msg = string.Format("Unknown type '{0}'", kindStr);
                    throw LayoutException.Create<SyntaxException>(layout, stmt, msg);
                }
                unitSize = typeInfo.Size;
            }

            offset += (count * unitSize);
            //Console.WriteLine("Aligned {0} units of size {1}", count, unitSize);
        }

        private void InterpretEcho(Statement stmt)
        {
            if (!stmt.Parameters.TryGetValue(Parameters.Message, out string echoMsg))
            {
                string msg = string.Format("Missing required parameter '{0}'", Parameters.Message);
                throw LayoutException.Create<SyntaxException>(layout, stmt, msg);
            }

            // TODO: resolve variables

            echoWriter.WriteLine(echoMsg);
        }

        private void InterpretInclude(Statement stmt)
        {
            // TODO
        }

        private bool TryLookupType(string typeName, out CascaraTypeInfo typeInfo)
        {
            if (BuiltInPrimitives.TryGetValue(typeName, out typeInfo))
            {
                return true;
            }

            return userDefinedTypes.TryGetValue(typeName, out typeInfo);
        }

        private double EvaluateExpression(string expr)
        {
            // Clever way of evaluating math expressions using .NET's DataTable class.

            DataTable dt = new DataTable();
            DataColumn dc = new DataColumn(null, typeof(double), expr);
            dt.Columns.Add(dc);
            dt.Rows.Add(0);

            return (double) dt.Rows[0][0];
        }

        private void InitializeInterpreterActionMaps()
        {
            statementTypeActionMap[StatementType.Directive] = InterpretDirective;
            statementTypeActionMap[StatementType.FileObjectDefinition] = InterpretFileObjectDefinition;
            statementTypeActionMap[StatementType.LocalVariableDefinition] = InterpretLocalVariableDefinition;
            statementTypeActionMap[StatementType.TypeDefinition] = InterpretTypeDefinition;

            directiveActionMap[Keywords.Align] = InterpretAlign;
            directiveActionMap[Keywords.Echo] = InterpretEcho;
            directiveActionMap[Keywords.Include] = InterpretInclude;
        }

        private static Dictionary<string, CascaraTypeInfo> BuiltInPrimitives = new Dictionary<string, CascaraTypeInfo>()
        {
            { Keywords.Bool, CascaraTypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<bool>(), typeof(bool)) },
            { Keywords.Bool8, CascaraTypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<bool>(), typeof(bool)) },
            { Keywords.Bool16, CascaraTypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<Bool16>(), typeof(Bool16)) },
            { Keywords.Bool32, CascaraTypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<Bool32>(), typeof(Bool32)) },
            { Keywords.Bool64, CascaraTypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<Bool64>(), typeof(Bool64)) },
            { Keywords.Byte, CascaraTypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<byte>(), typeof(byte)) },
            { Keywords.Char, CascaraTypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<Char8>(), typeof(Char8)) },
            { Keywords.Char8, CascaraTypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<Char8>(), typeof(Char8)) },
            { Keywords.Char16, CascaraTypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<char>(), typeof(char)) },
            { Keywords.Double, CascaraTypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<double>(), typeof(double)) },
            { Keywords.Float, CascaraTypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<float>(), typeof(float)) },
            { Keywords.Int, CascaraTypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<int>(), typeof(int)) },
            { Keywords.Int8, CascaraTypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<sbyte>(), typeof(sbyte)) },
            { Keywords.Int16, CascaraTypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<short>(), typeof(short)) },
            { Keywords.Int32, CascaraTypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<int>(), typeof(int)) },
            { Keywords.Int64, CascaraTypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<long>(),  typeof(long)) },
            { Keywords.Long, CascaraTypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<long>(), typeof(long)) },
            { Keywords.Short, CascaraTypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<short>(), typeof(short)) },
            { Keywords.Single, CascaraTypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<float>(), typeof(float)) },
            { Keywords.UInt, CascaraTypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<uint>(), typeof(uint)) },
            { Keywords.UInt8, CascaraTypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<byte>(), typeof(byte)) },
            { Keywords.UInt16, CascaraTypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<ushort>(), typeof(ushort)) },
            { Keywords.UInt32, CascaraTypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<uint>(), typeof(uint)) },
            { Keywords.UInt64, CascaraTypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<ulong>(), typeof(ulong)) },
            { Keywords.ULong, CascaraTypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<ulong>(), typeof(ulong)) },
            { Keywords.UShort, CascaraTypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<ushort>(), typeof(ushort)) }
        };
    }

    internal struct CascaraTypeInfo
    {
        public static CascaraTypeInfo CreatePrimitive(int size, Type nativeType)
        {
            return new CascaraTypeInfo(size, nativeType);
        }

        public static CascaraTypeInfo CreateStruct(int size, params Tuple<int, CascaraTypeInfo>[] members)
        {
            return new CascaraTypeInfo(size, members);
        }

        private CascaraTypeInfo(int size, Type nativeType)
        {
            Size = size;
            NativeType = nativeType;
            Members = new List<Tuple<int, CascaraTypeInfo>>();
        }

        private CascaraTypeInfo(int size, params Tuple<int, CascaraTypeInfo>[] members)
        {
            Size = size;
            NativeType = null;
            Members = new List<Tuple<int, CascaraTypeInfo>>(members);
        }

        public int Size { get; }
        public Type NativeType { get; }
        public IEnumerable<Tuple<int, CascaraTypeInfo>> Members { get; }
        public bool IsStruct { get { return NativeType == null && Members.Any(); } }
    }
}
