using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace WHampson.Cascara
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
