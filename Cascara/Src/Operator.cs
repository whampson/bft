#region License
/* Copyright (c) 2017 Wes Hampson
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
