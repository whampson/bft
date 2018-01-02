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

namespace WHampson.Cascara.Extensions
{
    /// <summary>
    /// Extension methods for the <see cref="string"/> class.
    /// </summary>
    internal static class StringExtensions
    {
        private const string EllipesString = "...";

        /// <summary>
        /// Truncates a string with ellipses (...) if the string is
        /// longer than the specified length. The resulting string
        /// will have a length less than or equal to the specified
        /// length.
        /// </summary>
        /// <param name="s">The string to truncate.</param>
        /// <param name="length">The maximum desired length of the string.</param>
        /// <returns></returns>
        public static string Ellipses(this string s, int length)
        {
            if (s.Length > length)
            {
                s = s.Substring(0, length - EllipesString.Length) + EllipesString;
            }

            return s;
        }
    }
}
