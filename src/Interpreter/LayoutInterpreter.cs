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
            Cascara.AssemblyVersion        // current version
        });

        private delegate void InterpretAction(Statement stmt);

        private LayoutScript layout;
        private BinaryData file;
        private Stack<CodeBlock> scopeStack;
        private HashSet<LayoutScript> includedLayouts;
        private TextWriter echoWriter;

        private Dictionary<string, TypeInfo> userDefinedTypes;

        private Dictionary<StatementType, InterpretAction> statementTypeActionMap;
        private Dictionary<string, InterpretAction> directiveActionMap;

        public LayoutInterpreter(LayoutScript layout, TextWriter echoWriter)
        {
            if (!SupportedVersions.Contains(layout.Version))
            {
                string msg = "This layout script is not supported by this version of Cascara.";
                throw new NotSupportedException(msg);
            }

            this.layout = layout;
            this.echoWriter = echoWriter;
            scopeStack = new Stack<CodeBlock>();
            includedLayouts = new HashSet<LayoutScript>();
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

        public void Execute(SymbolTable rootSymbol, BinaryData file)
        {
            Reset();

            this.file = file;
            scopeStack.Push(new CodeBlock(rootSymbol));
            includedLayouts.Add(layout);

            InterpretRootStatement(layout.RootStatement);
        }

        private void InterpretRootStatement(Statement stmt)
        {
            // Validate root element name
            if (stmt.Keyword != Keywords.XmlDocumentRoot)
            {
                string msg = Resources.SyntaxExceptionXmlInvalidRootElement;
                throw LayoutScriptException.Create<SyntaxException>(layout, stmt, msg, Keywords.XmlDocumentRoot);
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
                        throw LayoutScriptException.Create<LayoutScriptException>(layout, stmt, msg, stmt.Keyword);
                    }
                    break;

                case StatementType.None:
                    // Invalid state. Should never happen if the statement was parsed correctly...
                    msg = "Invalid statement.";
                    throw LayoutScriptException.Create<LayoutScriptException>(layout, stmt, msg);

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
            bool isAnonymousStructure = (typeName == Keywords.DataTypes.Struct || typeName == Keywords.DataTypes.Union);

            TypeInfo type = default(TypeInfo);
            if (!isAnonymousStructure && !TryLookupType(typeName, out type))
            {
                string msg = "Unknown type '{0}'.";
                throw LayoutScriptException.Create<LayoutScriptException>(layout, stmt, msg, typeName);
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
                    throw LayoutScriptException.Create<LayoutScriptException>(layout, e, stmt, msg, countStr);
                }

                if (count <= 0)
                {
                    string msg = "Parameter '{0}' must be a positive integer.";
                    throw LayoutScriptException.Create<LayoutScriptException>(layout, stmt, msg, Parameters.Count);
                }
            }

            // Validate name and create object symbol
            SymbolTable sym = SymbolTable.CreateRootSymbolTable();
            if (hasName)
            {
                if (!SymbolTable.IsIdentifierValid(objName))
                {
                    string msg = "Invalid identifier '{0}'. " +
                        "Identifiers must consist of only letters, numbers, and underscores. " +
                        "Identifiers cannot begin with a digit nor can they be identical to a reserved word.";
                    throw LayoutScriptException.Create<LayoutScriptException>(layout, stmt, msg, objName);
                }
                if (CurrentCodeBlock.Symbol.Lookup(objName) != null || TryLookupLocal(objName, out double dummyLocalValue))
                {
                    string msg = "A variable with identifier '{0}' already exists in the current scope.";
                    throw LayoutScriptException.Create<LayoutScriptException>(layout, stmt, msg, objName);
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

            // Set symbol properties
            sym.GlobalDataAddress = GlobalOffset;
            sym.LocalDataAddress = CurrentCodeBlock.Offset;
            if (isAnonymousStructure || type.IsStruct || type.IsUnion)
            {
                sym.DataType = typeof(Structure);
            }
            else
            {
                sym.DataType = type.NativeType;
            }

            int totalDataLength = 0;
            SymbolTable elemSym = sym;
            for (int i = 0; i < count; i++)
            {
                if (hasCount && hasName)
                {
                    elemSym = sym[i];
                }
                elemSym.GlobalDataAddress = GlobalOffset;

                if (isAnonymousStructure || type.IsStruct || type.IsUnion)
                {
                    if (hasName)
                    {
                        scopeStack.Push(new CodeBlock(elemSym));
                    }
                    else
                    {
                        scopeStack.Push(new CodeBlock(SymbolTable.CreateNamelessSymbolTable(CurrentCodeBlock.Symbol)));
                    }
                    CurrentCodeBlock.IsUnion = (stmt.Keyword == Keywords.DataTypes.Union || (type != null && type.IsUnion));

                    IEnumerable<Statement> members = (isAnonymousStructure)
                        ? stmt.NestedStatements
                        : type.Members;
                    if (!members.Any())
                    {
                        string msg = "Empty structures are not allowed.";
                        throw LayoutScriptException.Create<SyntaxException>(layout, stmt, msg);
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

                    elemSym.DataType = sym.DataType;
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
                        throw LayoutScriptException.Create<LayoutScriptException>(layout, stmt, msg);
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
                    throw LayoutScriptException.Create<LayoutScriptException>(layout, e, stmt, msg, countStr);
                }

                if (count <= 0)
                {
                    string msg = "Parameter '{0}' must be a positive integer.";
                    throw LayoutScriptException.Create<LayoutScriptException>(layout, stmt, msg, Parameters.Count);
                }
            }

            if (hasKind)
            {
                if (!TryLookupType(kindStr, out TypeInfo unit))
                {
                    string msg = "Unknown type '{0}'.";
                    throw LayoutScriptException.Create<LayoutScriptException>(layout, stmt, msg, kindStr);
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
            EnsureParameters(stmt, Parameters.Path);

            GetRequiredParameter(stmt, Parameters.Path, out string path);

            // Path is relative to the current layout script's path.
            // If the current script was not loaded from a file, the path is
            // relative to the current program's directory.
            string fullPath;
            if (layout.SourcePath == null)
            {
                fullPath = Directory.GetCurrentDirectory() + "/" + path;
            }
            else
            {
                fullPath = Path.GetDirectoryName(layout.SourcePath) + "/" + path;
            }
            fullPath = Path.GetFullPath(fullPath);

            if (!File.Exists(fullPath))
            {
                string fmt = "File not found - {0}";
                throw LayoutScriptException.Create<LayoutScriptException>(layout, stmt, fmt, path);
            }

            LayoutScript incl = LayoutScript.Load(fullPath);
            if (includedLayouts.Contains(incl))
            {
                string msg = "Include cycle detected!";
                throw LayoutScriptException.Create<LayoutScriptException>(layout, stmt, msg);
            }

            includedLayouts.Add(incl);
            InterpretRootStatement(incl.RootStatement);
        }

        private void InterpretLocal(Statement stmt)
        {
            EnsureParameters(stmt, Parameters.Comment, Parameters.Name, Parameters.Value);

            GetRequiredParameter(stmt, Parameters.Name, out string varName);
            GetRequiredParameter(stmt, Parameters.Value, out string valueStr);

            if (CurrentCodeBlock.Symbol.Lookup(varName) != null) {
                string msg = "A variable with identifier '{0}' already exists in the current scope.";
                throw LayoutScriptException.Create<LayoutScriptException>(layout, stmt, msg, varName);
            }

            try {
                double value = EvaluateExpression(valueStr);
                CurrentCodeBlock.Locals[varName] = value;
            } catch (SyntaxErrorException e) {
                string msg = "'{0}' is not a valid expression.";
                throw LayoutScriptException.Create<LayoutScriptException>(layout, e, stmt, msg, valueStr);
            }
        }

        private void InterpretTypedef(Statement stmt)
        {
            EnsureParameters(stmt, Parameters.Comment, Parameters.Kind, Parameters.Name);

            GetRequiredParameter(stmt, Parameters.Kind, out string baseTypeName);
            GetRequiredParameter(stmt, Parameters.Name, out string newTypeName);

            bool isStruct = baseTypeName == Keywords.DataTypes.Struct;
            bool isUnion = baseTypeName == Keywords.DataTypes.Union;
            TypeInfo baseType;

            if (isStruct)
            {
                // TODO: setting size to 0 here;, might want to reconsider this
                // if we want SizeOf to work on types in addition to objects
                baseType = TypeInfo.CreateStruct(0, stmt.NestedStatements.ToArray());
            }
            else if (isUnion)
            {
                // TODO: setting size to 0 here; might want to reconsider this
                // if we want SizeOf to work on types in addition to objects
                baseType = TypeInfo.CreateUnion(0, stmt.NestedStatements.ToArray());
            }
            else if (!TryLookupType(baseTypeName, out baseType)) {
                string msg = "Unknown type '{0}'.";
                throw LayoutScriptException.Create<LayoutScriptException>(layout, stmt, msg, baseTypeName);
            }

            if (!SymbolTable.IsIdentifierValid(newTypeName)) {
                string msg = "Invalid identifier '{0}'. " +
                    "Identifiers must consist of only letters, numbers, and underscores. " +
                    "Identifiers cannot begin with a digit nor can they be identical to a reserved word.";
                throw LayoutScriptException.Create<LayoutScriptException>(layout, stmt, msg, newTypeName);
            }

            if (TryLookupType(newTypeName, out TypeInfo dummyType)) {
                string msg = "Type '{0}' already exists.";
                throw LayoutScriptException.Create<LayoutScriptException>(layout, stmt, msg, newTypeName);
            }

            userDefinedTypes[newTypeName] = baseType;
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
            DataTable dt;
            object resultObj;
            double result;

            expr = ResolveLayoutVariables(expr);

            // Clever way of evaluating math expressions using .NET's DataTable class.
            dt = new DataTable();
            resultObj = dt.Compute(expr, string.Empty);

            if (!double.TryParse(resultObj.ToString(), out result))
            {
                throw new SyntaxException("Result cannot be cast to double.");
            }

            return result;
        }

        private void EnsureParameters(Statement stmt, params string[] paramNames)
        {
            List<string> unknownParams = stmt.Parameters
                .Select(x => x.Key)
                .Where(x => !paramNames.Contains(x))
                .ToList();

            if (unknownParams.Any())
            {
                string msg = "Unknown parameter '{0}'.";
                throw LayoutScriptException.Create<SyntaxException>(layout, stmt, msg, unknownParams[0]);
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
                throw LayoutScriptException.Create<SyntaxException>(layout, stmt, msg, paramName);
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
            if (!CurrentCodeBlock.Symbol.TryLookup(varName, out SymbolTable sym))
            {
                // TODO: get statement. Perhaps throw a different exception and have Caller of ResolveVariables catch and handle it
                string msg = "Unknown layout variable '{0}'";
                throw LayoutScriptException.Create<LayoutScriptException>(layout, null, msg, varName);
            }

            string val = "";
            switch (op)
            {
                case Operator.GlobalOffsetOf:
                    val = sym.GlobalDataAddress.ToString();
                    break;
                case Operator.OffsetOf:
                    val = sym.LocalDataAddress.ToString();
                    break;
                case Operator.SizeOf:
                    val = sym.DataLength.ToString();
                    break;
                case Operator.ValueOf:
                    val = StringValueOf(sym);
                    break;
            }

            return val;
        }

        private string StringValueOf(SymbolTable sym)
        {
            StringWriter sw = new StringWriter();

            using (JsonWriter jw = new JsonTextWriter(sw))
            {
                jw.Formatting = Formatting.None;
                StringValueOf(sym, jw);
            }

            return sw.ToString();
        }

        private void StringValueOf(SymbolTable sym, JsonWriter jw)
        {
            MethodInfo method;
            object val;

            if (jw == null)
            {
                return;
            }

            if (!sym.IsStruct && !sym.IsCollection)
            {
                method = typeof(BinaryData)
                    .GetMethod(nameof(BinaryData.Get), new Type[] { typeof(int) })
                    .MakeGenericMethod(sym.DataType);

                val = method.Invoke(file, new object[] { sym.GlobalDataAddress });
                jw.WriteRawValue(val.ToString());
            }
            else if (sym.IsCollection)
            {
                jw.WriteStartArray();
                foreach (SymbolTable elemSym in sym)
                {
                    StringValueOf(elemSym, jw);
                }
                jw.WriteEndArray();
            }
            else if (sym.GetAllMembers().Any())
            {
                jw.WriteStartObject();
                foreach (SymbolTable memberSym in sym.GetAllMembers())
                {
                    jw.WritePropertyName(memberSym.Name);
                    jw.WriteRawValue(StringValueOf(memberSym));
                }
                jw.WriteEndObject();
            }
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

            directiveActionMap[Keywords.Directives.Align] = InterpretAlign;
            //directiveActionMap[Keywords.Directives.Branch] = InterpretBranch;
            directiveActionMap[Keywords.Directives.Echo] = InterpretEcho;
            //directiveActionMap[Keywords.Directives.Goto] = InterpretGoto;
            directiveActionMap[Keywords.Directives.Include] = InterpretInclude;
            //directiveActionMap[Keywords.Directives.Label] = InterpretLabel;
            directiveActionMap[Keywords.Directives.Local] = InterpretLocal;
            directiveActionMap[Keywords.Directives.Typedef] = InterpretTypedef;
        }

        private static Dictionary<string, TypeInfo> BuiltInPrimitives = new Dictionary<string, TypeInfo>()
        {
            { Keywords.DataTypes.Bool, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<bool>(), typeof(bool)) },
            { Keywords.DataTypes.Bool8, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<bool>(), typeof(bool)) },
            { Keywords.DataTypes.Bool16, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<Bool16>(), typeof(Bool16)) },
            { Keywords.DataTypes.Bool32, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<Bool32>(), typeof(Bool32)) },
            { Keywords.DataTypes.Bool64, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<Bool64>(), typeof(Bool64)) },
            { Keywords.DataTypes.Byte, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<byte>(), typeof(byte)) },
            { Keywords.DataTypes.Char, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<Char8>(), typeof(Char8)) },
            { Keywords.DataTypes.Char8, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<Char8>(), typeof(Char8)) },
            { Keywords.DataTypes.Char16, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<char>(), typeof(char)) },
            { Keywords.DataTypes.Double, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<double>(), typeof(double)) },
            { Keywords.DataTypes.Float, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<float>(), typeof(float)) },
            { Keywords.DataTypes.Int, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<int>(), typeof(int)) },
            { Keywords.DataTypes.Int8, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<sbyte>(), typeof(sbyte)) },
            { Keywords.DataTypes.Int16, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<short>(), typeof(short)) },
            { Keywords.DataTypes.Int32, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<int>(), typeof(int)) },
            { Keywords.DataTypes.Int64, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<long>(),  typeof(long)) },
            { Keywords.DataTypes.Long, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<long>(), typeof(long)) },
            { Keywords.DataTypes.Short, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<short>(), typeof(short)) },
            { Keywords.DataTypes.Single, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<float>(), typeof(float)) },
            { Keywords.DataTypes.UInt, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<uint>(), typeof(uint)) },
            { Keywords.DataTypes.UInt8, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<byte>(), typeof(byte)) },
            { Keywords.DataTypes.UInt16, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<ushort>(), typeof(ushort)) },
            { Keywords.DataTypes.UInt32, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<uint>(), typeof(uint)) },
            { Keywords.DataTypes.UInt64, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<ulong>(), typeof(ulong)) },
            { Keywords.DataTypes.ULong, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<ulong>(), typeof(ulong)) },
            { Keywords.DataTypes.UShort, TypeInfo.CreatePrimitive(PrimitiveTypeUtils.SizeOf<ushort>(), typeof(ushort)) }
        };

        private class CodeBlock
        {
            private int _offset;
            private int _maxOffset;

            public CodeBlock(SymbolTable sym)
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

            public SymbolTable Symbol
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
