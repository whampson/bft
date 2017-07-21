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
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WHampson.Bft
{
    internal static class NumberUtils
    {
        private const string Base2Pattern = @"^([01]{1,64})[bB]$";
        private const string Base8Pattern = @"^([0-7]{1,64})[oO]$";
        private const string Base16Pattern = @"^0?[xX]{1}([0-9a-fA-F]{1,16})$|^([0-9a-fA-F]{1,16})[hH]{1}$";
        private const string MathExprPattern = @"^[-+*/().\d ]+$";

        private static readonly Regex Base2Regex = new Regex(Base2Pattern);
        private static readonly Regex Base8Regex = new Regex(Base8Pattern);
        private static readonly Regex Base16Regex = new Regex(Base16Pattern);
        private static readonly Regex MathExprRegex = new Regex(MathExprPattern);

        public static double EvaluateExpression(string expr)
        {
            if (!MathExprRegex.IsMatch(expr))
            {
                string msg = string.Format("Invalid math expression '{0}'", expr);
                throw new FormatException(msg);
            }

            object valObj = new DataTable().Compute(expr, null);
            double val = Convert.ToDouble(valObj);
            if (double.IsInfinity(val))
            {
                string msg = string.Format("Expression '{0}' evaluates to infinity.", expr);
                throw new ArithmeticException(msg);
            }

            return val;
        }

        /// <summary>
        /// Attempts to convert a string to a 64-bit integer value.
        /// A return value indicates whether the conversion succeeded.
        /// </summary>
        /// <remarks>
        /// This method differs from <see cref="int.TryParse(string, out int)"/>
        /// in that it will handle base-2, base-8, bbase-10 and base-16 number representations.
        ///     Base-2  -- ends with 'b' or 'B' character
        ///     Base-8  -- ends with 'o' or 'O' character
        ///     Base-10 -- no prefix or suffix characters
        ///     Base-16 -- starts with 'x', 'X', 0x', or '0X'; or ends with 'h' or 'H'
        /// </remarks>
        /// <param name="valStr">
        /// The string representation of the number to convert.
        /// </param>
        /// <param name="val">
        /// The converted number.
        /// </param>
        /// <returns>
        /// <code>True</code> if the conversion was successful,
        /// <code>False</code> otherwise.
        /// </returns>
        public static bool TryParseInteger(string valStr, out long val)
        {
            int radix = 10;
            Match base2Match = Base2Regex.Match(valStr);
            Match base8Match = Base8Regex.Match(valStr);
            Match base16Match = Base16Regex.Match(valStr);

            if (base2Match.Success)
            {
                valStr = base2Match.Groups[1].Value;
                radix = 2;
            }
            else if (base8Match.Success)
            {
                valStr = base8Match.Groups[1].Value;
                radix = 8;
            }
            else if (base16Match.Success)
            {
                Group g1 = base16Match.Groups[1];   // 0x*
                Group g2 = base16Match.Groups[2];   // *h
                valStr = (!string.IsNullOrWhiteSpace(g1.Value)) ? g1.Value : g2.Value;
                radix = 16;
            }

            try
            {
                val = Convert.ToInt64(valStr, radix);
                return true;
            }
            catch (FormatException e)
            {
                val = 0;
                return false;
            }
        }
    }
}
