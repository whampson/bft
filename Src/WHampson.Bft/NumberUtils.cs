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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WHampson.Bft
{
    internal static class NumberUtils
    {
        private const string Base2RegexString = @"^([01]{1,64})[bB]$";
        private const string Base8RegexString = @"^([0-7]{1,64})[oO]$";
        private const string Base16RegexString = @"^0?[xX]{1}([0-9a-fA-F]{1,16})$|^([0-9a-fA-F]{1,16})[hH]{1}$";

        private static readonly Regex Base2Regex = new Regex(Base2RegexString);
        private static readonly Regex Base8Regex = new Regex(Base8RegexString);
        private static readonly Regex Base16Regex = new Regex(Base16RegexString);

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
