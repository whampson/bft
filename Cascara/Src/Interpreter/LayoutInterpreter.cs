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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using static WHampson.Cascara.Interpreter.ReservedWords;

namespace WHampson.Cascara.Interpreter
{
    internal sealed class LayoutInterpreter
    {
        private const string GlobalOffsetOfPattern = @"\$GlobalOffsetOf\((.+?)\)";
        private const string OffsetOfPattern = @"\$OffsetOf\((.+?)\)";
        private const string SizeOfPattern = @"\$SizeOf\((.+?)\)";
        private const string ValueOfPattern = @"\$ValueOf\((.+?)\)";
        private const string ValueOfShorthandPattern = @"\${(.+?)}";

        private static readonly HashSet<Version> SupportedVersions = new HashSet<Version>(new Version[]
        {
            AssemblyInfo.AssemblyVersion        // current version
        });

        private delegate void InterpretAction(Statement stmt);

        private BinaryLayout layout;
        private BinaryFile file;
        private Stack<CodeBlock> scopeStack;
        private HashSet<BinaryLayout> includedLayouts;
        private TextWriter echoWriter;

        private Dictionary<string, TypeInfo> userDefinedTypes;

        private Dictionary<StatementType, InterpretAction> statementTypeActionMap;
        private Dictionary<string, InterpretAction> directiveActionMap;

        public LayoutInterpreter(BinaryLayout layout, TextWriter echoWriter)
        {
            if (!SupportedVersions.Contains(layout.Version))
            {
                string msg = "This layout version is not supported by this version of Cascara.";
                throw new NotSupportedException(msg);
            }

            this.layout = layout;
            this.echoWriter = echoWriter;
            scopeStack = new Stack<CodeBlock>();
            includedLayouts = new HashSet<BinaryLayout>();
            userDefinedTypes = new Dictionary<string, TypeInfo>();

            statementTypeActionMap = new Dictionary<StatementType, InterpretAction>();
            directiveActionMap = new Dictionary<string, InterpretAction>();
            InitializeInterpreterActionMaps();
        }

        private CodeBlock CurrentCodeBlock
        {
            get { return scopeStack.Peek(); }
        }

        private int GlobalOffset
        {
            get { return scopeStack.Sum(s => s.Offset); }
        }

        public void Reset()
        {
            scopeStack.Clear();
            includedLayouts.Clear();
            userDefinedTypes.Clear();
            file = null;
        }

        public void Execute(Symbol rootSymbol, BinaryFile file)
        {
            Reset();

            this.file = file;
            scopeStack.Push(new CodeBlock(rootSymbol));
            includedLayouts.Add(layout);

            InterpretRootStatement(layout.RootStatement);
            echoWriter.WriteLine("Final offset: {0}", GlobalOffset);
        }

        private void InterpretRootStatement(Statement stmt)
        {
            // Validate root element name
            if (stmt.Keyword != Keywords.XmlDocumentRoot)
            {
                string msg = Resources.SyntaxExceptionXmlInvalidRootElement;
                throw LayoutException.Create<SyntaxException>(layout, stmt, msg, Keywords.XmlDocumentRoot);
            }

            // Interpret nested statements
            foreach (Statement childStmt in stmt.NestedStatements)
            {
                InterpretStatement(childStmt);
            }
        }

        private void InterpretStatement(Statement stmt)
        {
            StatementType type = stmt.StatementType;
            InterpretAction interpret;
            string msg;

            switch (type)
            {
                case StatementType.Directive:
                    // Get action based on directive name.
                    if (!directiveActionMap.TryGetValue(stmt.Keyword, out interpret))
                    {
                        msg = "Unknown directive '{0}'.";
                        throw LayoutException.Create<LayoutException>(layout, stmt, msg, stmt.Keyword);
                    }
                    break;

                case StatementType.None:
                    // Invalid state. Should never happen if the statement was parsed correctly...
                    msg = "Invalid statement.";
                    throw LayoutException.Create<LayoutException>(layout, stmt, msg);

                default:
                    // Get action based on statement type
                    if (!statementTypeActionMap.TryGetValue(type, out interpret))
                    {
                        // Should never happen...
                        msg = string.Format("Bug! No action exists for statement type '{0}'!", type);
                        throw new InvalidOperationException(msg);
                    }
                    break;
            }

            interpret(stmt);
        }

        private void InterpretFileObjectDefinition(Statement stmt)
        {
            EnsureParameters(stmt, Parameters.Comment, Parameters.Count, Parameters.Name);

            bool hasCount = GetParameter(stmt, Parameters.Count, out string countStr);
            bool hasName = GetParameter(stmt, Parameters.Name, out string objName);

            string typeName = stmt.Keyword;
            bool isStruct = (typeName == Keywords.Struct || typeName == Keywords.Union);

            TypeInfo type = default(TypeInfo);
            if (!isStruct && !TryLookupType(typeName, out type))
            {
                string msg = "Unknown type '{0}'.";
                throw LayoutException.Create<LayoutException>(layout, stmt, msg, typeName);
            }

            // Evaluate 'count' expression
            int count = 1;
            if (hasCount)
            {
                try
                {
                    count = (int) EvaluateExpression(countStr);
                }
                catch (SyntaxErrorException e)
                {
                    string msg = "'{0}' is not a valid expression.";
                    throw LayoutException.Create<LayoutException>(layout, e, stmt, msg, countStr);
                }

                if (count <= 0)
                {
                    string msg = "Parameter '{0}' must be a positive integer.";
                    throw LayoutException.Create<LayoutException>(layout, stmt, msg, Parameters.Count);
                }
            }

            // Validate name and create object symbol
            Symbol sym = Symbol.CreateRootSymbol();
            if (hasName)
            {
                if (!Symbol.IsNameValid(objName))
                {
                    string msg = "Invalid identifier '{0}'. " +
                        "Identifiers must consist of only letters, numbers, and underscores. " +
                        "Identifiers cannot begin with a digit nor can they be identical to a reserved word.";
                    throw LayoutException.Create<LayoutException>(layout, stmt, msg, objName);
                }
                if (CurrentCodeBlock.Symbol.Lookup(objName) != null || TryLookupLocal(objName, out double dummyLocalValue))
                {
                    string msg = "A variable with identifier '{0}' already exists in the current scope.";
                    throw LayoutException.Create<LayoutException>(layout, stmt, msg, objName);
                }

                if (hasCount)
                {
                    sym = CurrentCodeBlock.Symbol.Insert(objName, count);
                }
                else
                {
                    sym = CurrentCodeBlock.Symbol.Insert(objName);
                }
            }

            // Set symbol proprties
            sym.DataOffset = GlobalOffset;
            sym.DataType = (hasCount && !(isStruct || type.IsStruct))
                ? type.NativeType.MakeArrayType()
                : type.NativeType;
            
            int totalDataLength = 0;
            Symbol elemSym = sym;
            for (int i = 0; i < count; i++)
            {
                if (hasCount)
                {
                    elemSym = sym[i];
                }
                elemSym.DataOffset = GlobalOffset;

                if (isStruct || type.IsStruct)
                {
                    if (hasName)
                    {
                        scopeStack.Push(new CodeBlock(elemSym));
                    }
                    else
                    {
                        scopeStack.Push(new CodeBlock(Symbol.CreateNamelessSymbol(CurrentCodeBlock.Symbol)));
                    }
                    CurrentCodeBlock.IsUnion = (stmt.Keyword == Keywords.Union);

                    IEnumerable<Statement> members = (isStruct)
                        ? stmt.NestedStatements
                        : type.Members;
                    if (!members.Any())
                    {
                        string msg = "Empty structures are not allowed.";
                        throw LayoutException.Create<SyntaxException>(layout, stmt, msg);
                    }

                    foreach (Statement member in members)
                    {
                        InterpretStatement(member);
                    }

                    CodeBlock oldScope = scopeStack.Pop();
                    int numBytes = oldScope.MaxOffset;
                    if (!CurrentCodeBlock.IsUnion)
                    {
                        CurrentCodeBlock.Offset += numBytes;
                    }
                    else
                    {
                        CurrentCodeBlock.MaxOffset = numBytes;
                    }

                    elemSym.DataType = null;
                    elemSym.DataLength = numBytes;
                    totalDataLength += numBytes;
                }
                else
                {
                    int numBytes = type.Size;
                    int newOffset = CurrentCodeBlock.Offset + numBytes;
                    if (newOffset > file.Length)
                    {
                        string msg = "Object definition runs past the end of the file.";
                        throw LayoutException.Create<LayoutException>(layout, stmt, msg);
                    }

                    if (!CurrentCodeBlock.IsUnion)
                    {
                        CurrentCodeBlock.Offset = newOffset;
                    }
                    else
                    {
                        CurrentCodeBlock.MaxOffset = numBytes;
                    }

                    elemSym.DataType = type.NativeType;
                    elemSym.DataLength = numBytes;
                    totalDataLength += numBytes;
                }
            }
            sym.DataLength = totalDataLength;
        }

        private void InterpretTypeDefinition(Statement stmt)
        {
            EnsureParameters(stmt, Parameters.Comment, Parameters.Kind, Parameters.Name);

            GetRequiredParameter(stmt, Parameters.Kind, out string baseTypeName);
            GetRequiredParameter(stmt, Parameters.Name, out string newTypeName);

            // TODO: handle struct/union

            if (!TryLookupType(baseTypeName, out TypeInfo baseType))
            {
                string msg = "Unknown type '{0}'.";
                throw LayoutException.Create<LayoutException>(layout, stmt, msg, baseTypeName);
            }

            if (!Symbol.IsNameValid(newTypeName))
            {
                string msg = "Invalid identifier '{0}'. " +
                    "Identifiers must consist of only letters, numbers, and underscores. " +
                    "Identifiers cannot begin with a digit nor can they be identical to a reserved word.";
                throw LayoutException.Create<LayoutException>(layout, stmt, msg, newTypeName);
            }

            if (TryLookupType(newTypeName, out TypeInfo dummyType))
            {
                string msg = "Type '{0}' already exists.";
                throw LayoutException.Create<LayoutException>(layout, stmt, msg, newTypeName);
            }

            userDefinedTypes[newTypeName] = baseType;
        }

        private void InterpretLocalVariableDefinition(Statement stmt)
        {
            EnsureParameters(stmt, Parameters.Comment, Parameters.Name, Parameters.Value);

            GetRequiredParameter(stmt, Parameters.Name, out string varName);
            GetRequiredParameter(stmt, Parameters.Value, out string valueStr);

            if (CurrentCodeBlock.Symbol.Lookup(varName) != null)
            {
                string msg = "A variable with identifier '{0}' already exists in the current scope.";
                throw LayoutException.Create<LayoutException>(layout, stmt, msg, varName);
            }

            try
            {
                double value = EvaluateExpression(valueStr);
                CurrentCodeBlock.Locals[varName] = value;
            }
            catch (SyntaxErrorException e)
            {
                string msg = "'{0}' is not a valid expression.";
                throw LayoutException.Create<LayoutException>(layout, e, stmt, msg, valueStr);
            }
        }

        private void InterpretAlign(Statement stmt)
        {
            EnsureParameters(stmt, Parameters.Comment, Parameters.Count, Parameters.Kind);

            bool hasCount = GetParameter(stmt, Parameters.Count, out string countStr);
            bool hasKind = GetParameter(stmt, Parameters.Kind, out string kindStr);

            int count = 1;
            int unitSize = 1;

            if (hasCount)
            {
                try
                {
                    count = (int) EvaluateExpression(countStr);
                }
                catch (SyntaxErrorException e)
                {
                    string msg = "'{0}' is not a valid expression.";
                    throw LayoutException.Create<LayoutException>(layout, e, stmt, msg, countStr);
                }

                if (count <= 0)
                {
                    string msg = "Parameter '{0}' must be a positive integer.";
                    throw LayoutException.Create<LayoutException>(layout, stmt, msg, Parameters.Count);
                }
            }

            if (hasKind)
            {
                if (!TryLookupType(kindStr, out TypeInfo unit))
                {
                    string msg = "Unknown type '{0}'.";
                    throw LayoutException.Create<LayoutException>(layout, stmt, msg, kindStr);
                }
                unitSize = unit.Size;
            }

            CurrentCodeBlock.Offset += (unitSize * count);
        }

        private void InterpretEcho(Statement stmt)
        {
            EnsureParameters(stmt, Parameters.Comment, Parameters.Message);

            GetRequiredParameter(stmt, Parameters.Message, out string echoMsg);

            echoWriter.WriteLine(ResolveLayoutVariables(echoMsg));
        }

        private void InterpretInclude(Statement stmt)
        {

        }

        private bool TryLookupType(string typeName, out TypeInfo typeInfo)
        {
            if (BuiltInPrimitives.TryGetValue(typeName, out typeInfo))
            {
                return true;
            }

            return userDefinedTypes.TryGetValue(typeName, out typeInfo);
        }

        private bool TryLookupLocal(string localVarName, out double value)
        {
            Dictionary<string, double> allLocals = scopeStack
                .SelectMany(m => m.Locals)
                .ToDictionary(k => k.Key, v => v.Value);

            return allLocals.TryGetValue(localVarName, out value);
        }

        private bool IsVariableIdentifierAvailable(string identName)
        {
            return (CurrentCodeBlock.Symbol.Lookup(identName)) == null
                //&& !TryLookupType(identName, out CascaraTypeInfo dummyType)
                && !TryLookupLocal(identName, out double dummyValue);
        }

        private double EvaluateExpression(string expr)
        {
            expr = ResolveLayoutVariables(expr);

            // Clever way of evaluating math expressions using .NET's DataTable class.
            DataTable dt = new DataTable();
            DataColumn dc = new DataColumn(null, typeof(bool), expr);
            dt.Columns.Add(dc);
            dt.Rows.Add(0);

            return (double) dt.Rows[0][0];
        }

        //private T EvaluateExpression2<T>(string expr)
        //    where T : struct
        //{
        //    expr = ResolveLayoutVariables(expr);

        //    // Clever way of evaluating math expressions using .NET's DataTable class.
        //    DataTable dt = new DataTable();
        //    DataColumn dc = new DataColumn(null, typeof(T), expr);
        //    dt.Columns.Add(dc);
        //    dt.Rows.Add(0);

        //    return (T) dt.Rows[0][0];
        //}

        /* Put these in 'Parameters.cs' */
        /* Or maybe an internal class within the interpreter (make this a partial class) */
        // bool GetCondParam(Statement stmt) { }
        // int GetNameParam(Statement stmt) { }
        // double GetValueParam(Statement stmt) { }

        private void EnsureParameters(Statement stmt, params string[] paramNames)
        {
            List<string> unknownParams = stmt.Parameters
                .Select(x => x.Key)
                .Where(x => !paramNames.Contains(x))
                .ToList();

            if (unknownParams.Any())
            {
                string msg = "Unknown parameter '{0}'.";
                throw LayoutException.Create<SyntaxException>(layout, stmt, msg, unknownParams[0]);
            }
        }

        private bool GetParameter(Statement stmt, string paramName, out string value)
        {
            return GetParameter(stmt, paramName, false, out value);
        }

        private bool GetRequiredParameter(Statement stmt, string paramName, out string value)
        {
            return GetParameter(stmt, paramName, true, out value);
        }

        private bool GetParameter(Statement stmt, string paramName, bool isRequired, out string value)
        {
            bool hasParam = stmt.Parameters.TryGetValue(paramName, out value);
            if (!hasParam && isRequired)
            {
                string msg = "Missing required parameter '{0}'";
                throw LayoutException.Create<SyntaxException>(layout, stmt, msg, paramName);
            }

            return hasParam;
        }

        private string ResolveLayoutVariables(string input)
        {
            input = Regex.Replace(input, GlobalOffsetOfPattern, ResolveGlobalOffsetOf);
            input = Regex.Replace(input, OffsetOfPattern, ResolveOffsetOf);
            input = Regex.Replace(input, SizeOfPattern, ResolveSizeOf);
            input = Regex.Replace(input, ValueOfPattern, ResolveValueOf);
            input = Regex.Replace(input, ValueOfShorthandPattern, ResolveValueOf);

            return input;
        }

        private string ResolveGlobalOffsetOf(Match m)
        {
            return EvaluateOperator(Operator.GlobalOffsetOf, m.Groups[1].Value);
        }

        private string ResolveOffsetOf(Match m)
        {
            return EvaluateOperator(Operator.OffsetOf, m.Groups[1].Value);
        }

        private string ResolveSizeOf(Match m)
        {
            return EvaluateOperator(Operator.SizeOf, m.Groups[1].Value);
        }

        private string ResolveValueOf(Match m)
        {
            return EvaluateOperator(Operator.ValueOf, m.Groups[1].Value);
        }

        private enum Operator
        {
            GlobalOffsetOf,
            OffsetOf,
            SizeOf,
            ValueOf
        }

        private string EvaluateOperator(Operator op, string varName)
        {
            bool isSpecialVar = SpecialVariables.AllSpecialVariables.Contains(varName);
            bool isLocalVar = TryLookupLocal(varName, out double localVarVal);

            // Handle special variables
            if (isSpecialVar)
            {
                if (op != Operator.ValueOf)
                {
                    Log(LogLevel.Warning, "Operator {0} cannot be used on special variable '{1}'.", op, varName);
                    return "";
                }

                switch (varName)
                {
                    case SpecialVariables.Filesize:
                        return file.Length + "";
                    case SpecialVariables.GlobalOffset:
                        return GlobalOffset + "";
                    case SpecialVariables.Offset:
                        return CurrentCodeBlock.Offset + "";
                }
            }

            // Handle local variables
            if (isLocalVar)
            {
                if (op != Operator.ValueOf)
                {
                    Log(LogLevel.Warning, "Operator {0} cannot be used on local variable '{1}'.", op, varName);
                    return "";
                }

                return localVarVal + "";
            }

            // Handle file-mapped variables
            if (!CurrentCodeBlock.Symbol.TryLookup(varName, out Symbol sym))
            {
                // TODO: get statement. Perhaps throw a different exception and have Caller of ResolveVariables catch and handle it
                string msg = "Unknown layout variable '{0}'";
                throw LayoutException.Create<LayoutException>(layout, null, msg, varName);
            }

            return StringValueOf(sym);
        }

        private string StringValueOf(Symbol sym)
        {
            if (sym.IsLeaf && !sym.IsCollection)
            {
                // file.Get<sym.DataType>(sym.DataOffset);
                MethodInfo method = typeof(BinaryFile)
                    .GetMethod(nameof(BinaryFile.Get), new Type[] { typeof(int) })
                    .MakeGenericMethod(sym.DataType);
                object val = method.Invoke(file, new object[] { sym.DataOffset });

                return val.ToString();
            }

            StringWriter sw = new StringWriter();

            // Treat character arrays as strings
            if (sym.IsCollection && (sym.DataType == typeof(char[]) || sym.DataType == typeof(Char8[])))
            {
                foreach (Symbol childSym in sym)
                {
                    string charValue = StringValueOf(childSym);
                    if (charValue == "\0")
                    {
                        break;
                    }
                    sw.Write(charValue);
                }

                return sw.ToString();
            }

            // Write file object in JSON form
            using (JsonWriter jw = new JsonTextWriter(sw))
            {
                if (sym.IsCollection)
                {
                    jw.Formatting = Formatting.None;
                    jw.WriteStartArray();
                    foreach (Symbol elemSym in sym)
                    {
                        jw.WriteRawValue(StringValueOf(elemSym));
                    }
                    jw.WriteEndArray();
                }
                
                if (sym.GetAllMembers().Any())
                {
                    jw.Formatting = Formatting.Indented;
                    jw.WriteStartObject();
                    foreach (Symbol memberSym in sym.GetAllMembers())
                    {
                        jw.WritePropertyName(memberSym.Name);
                        jw.WriteRawValue(StringValueOf(memberSym));
                    }
                    jw.WriteEndObject();
                }
            }

            return sw.ToString();
        }

        private enum LogLevel
        {
            Info,
            Warning,
            Error
        }

        private void Log(LogLevel level, string fmt, params object[] fmtArgs)
        {
            string prefix = "";
            switch (level)
            {
                case LogLevel.Info:
                    prefix = "<Layout Interpreter> [INFO]: ";
                    break;
                case LogLevel.Warning:
                    prefix = "<Layout Interpreter> [WARNING]: ";
                    break;
                case LogLevel.Error:
                    prefix = "<Layout Interpreter> [ERROR]: ";
                    break;
            }

            echoWriter.WriteLine("{0}{1}", prefix, string.Format(fmt, fmtArgs));
        }

        private void InitializeInterpreterActionMaps()
        {
            statementTypeActionMap[StatementType.FileObjectDefinition] = InterpretFileObjectDefinition;
            statementTypeActionMap[StatementType.LocalVariableDefinition] = InterpretLocalVariableDefinition;
            statementTypeActionMap[StatementType.TypeDefinition] = InterpretTypeDefinition;

            directiveActionMap[Keywords.Align] = InterpretAlign;
            directiveActionMap[Keywords.Echo] = InterpretEcho;
            directiveActionMap[Keywords.Include] = InterpretInclude;
        }

        private static Dictionary<string, TypeInfo> BuiltInPrimitives = new Dictionary<string, TypeInfo>()
        {
            { Keywords.Bool, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<bool>(), typeof(bool)) },
            { Keywords.Bool8, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<bool>(), typeof(bool)) },
            { Keywords.Bool16, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<Bool16>(), typeof(Bool16)) },
            { Keywords.Bool32, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<Bool32>(), typeof(Bool32)) },
            { Keywords.Bool64, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<Bool64>(), typeof(Bool64)) },
            { Keywords.Byte, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<byte>(), typeof(byte)) },
            { Keywords.Char, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<Char8>(), typeof(Char8)) },
            { Keywords.Char8, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<Char8>(), typeof(Char8)) },
            { Keywords.Char16, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<char>(), typeof(char)) },
            { Keywords.Double, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<double>(), typeof(double)) },
            { Keywords.Float, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<float>(), typeof(float)) },
            { Keywords.Int, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<int>(), typeof(int)) },
            { Keywords.Int8, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<sbyte>(), typeof(sbyte)) },
            { Keywords.Int16, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<short>(), typeof(short)) },
            { Keywords.Int32, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<int>(), typeof(int)) },
            { Keywords.Int64, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<long>(),  typeof(long)) },
            { Keywords.Long, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<long>(), typeof(long)) },
            { Keywords.Short, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<short>(), typeof(short)) },
            { Keywords.Single, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<float>(), typeof(float)) },
            { Keywords.UInt, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<uint>(), typeof(uint)) },
            { Keywords.UInt8, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<byte>(), typeof(byte)) },
            { Keywords.UInt16, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<ushort>(), typeof(ushort)) },
            { Keywords.UInt32, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<uint>(), typeof(uint)) },
            { Keywords.UInt64, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<ulong>(), typeof(ulong)) },
            { Keywords.ULong, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<ulong>(), typeof(ulong)) },
            { Keywords.UShort, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<ushort>(), typeof(ushort)) }
        };

        private class CodeBlock
        {
            private int _offset;
            private int _maxOffset;

            public CodeBlock(Symbol sym)
            {
                _offset = 0;
                _maxOffset = 0;
                IsUnion = false;
                Symbol = sym;
                Locals = new Dictionary<string, double>();
            }

            public int Offset
            {
                get { return _offset; }
                set
                {
                    _offset = value;
                    _maxOffset = Math.Max(_offset, _maxOffset);
                }
            }

            public int MaxOffset
            {
                get { return _maxOffset; }
                set { _maxOffset = value; }
            }

            public bool IsUnion
            {
                get;
                set;
            }

            public Symbol Symbol
            {
                get;
            }

            public Dictionary<string, double> Locals
            {
                get;
            }
        }
    }
}
