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
using System.Linq;
using System.Reflection;

namespace WHampson.Cascara.Extensions
{
    /// <summary>
    /// Extension methods for the <see cref="System.Type"/> class.
    /// </summary>
    internal static class TypeExtensions
    {
        /// <summary>
        /// Gets the values of all public constants of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the constants to get.</typeparam>
        /// <param name="clazz">The class to get constants from.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of the values of all constants.</returns>
        public static IEnumerable<T> GetPublicConstants<T>(this Type clazz)
            where T : IConvertible
        {
            if (clazz == null)
            {
                throw new ArgumentNullException(nameof(clazz));
            }

            return clazz.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(T))
                .Select(f => (T) f.GetRawConstantValue())
                .Concat(clazz.GetNestedTypes(BindingFlags.Public).SelectMany(GetPublicConstants<T>));
        }
    }
}
