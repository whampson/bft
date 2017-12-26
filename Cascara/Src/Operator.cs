using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace WHampson.Cascara
{
    internal abstract class CascaraOperator
    {
        private const string OffsetOfOperatorPattern = @"\$\[([\[\]\S]+)\]";
        private const string ValueOfOperatorPattern = @"\${(.+?)}";
        private const string SizeOfOperatorPattern = @"\$\((.+?)\)";
        private const string TypeOperatorPattern = @"type[ ]+(.+)";

        public static readonly CascaraOperator OffsetOf = new OffsetOfOperator();
        public static readonly CascaraOperator ValueOf = new ValueOfOperator();
        public static readonly CascaraOperator SizeOf = new SizeOfOperator();

        public static bool HasOperators(string input)
        {
            Match m1 = Regex.Match(input, OffsetOfOperatorPattern);
            Match m2 = Regex.Match(input, ValueOfOperatorPattern);
            Match m3 = Regex.Match(input, SizeOfOperatorPattern);

            return m1.Success || m2.Success || m3.Success;
        }

        private CascaraOperator() { }
        public abstract IEnumerable<string> GetVariableNames(string input);
        public abstract string ResolveVariables(string input, Symbol symbolTable, Dictionary<string, CascaraType> dataTypes);

        private sealed class ValueOfOperator : CascaraOperator
        {
            public override IEnumerable<string> GetVariableNames(string input)
            {
                MatchCollection matches = Regex.Matches(input, ValueOfOperatorPattern);
                foreach (Match m in matches)
                {
                    yield return m.Groups[1].Value;
                }
            }

            public override string ResolveVariables(string input, Symbol symbolTable, Dictionary<string, CascaraType> dataTypes)
            {
                throw new NotImplementedException();
            }
        }

        private sealed class OffsetOfOperator : CascaraOperator
        {
            public override IEnumerable<string> GetVariableNames(string input)
            {
                MatchCollection matches = Regex.Matches(input, ValueOfOperatorPattern);
                foreach (Match m in matches)
                {
                    yield return m.Groups[1].Value;
                }
            }

            public override string ResolveVariables(string input, Symbol symbolTable, Dictionary<string, CascaraType> dataTypes)
            {
                throw new NotImplementedException();
            }
        }

        private sealed class SizeOfOperator : CascaraOperator
        {
            public override IEnumerable<string> GetVariableNames(string input)
            {
                MatchCollection matches = Regex.Matches(input, ValueOfOperatorPattern);
                foreach (Match m in matches)
                {
                    string varName = m.Groups[1].Value;
                    Match m2 = Regex.Match(varName, TypeOperatorPattern);
                    if (m2.Success)
                    {
                        // Don't return type name
                        continue;
                    }

                    yield return varName;
                }
            }

            public override string ResolveVariables(string input, Symbol symbolTable, Dictionary<string, CascaraType> dataTypes)
            {
                throw new NotImplementedException();
            }
        }
    }
}
