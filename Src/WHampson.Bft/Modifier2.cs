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
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WHampson.Bft.Types;


// TODO: consider modifiers 'maxCount' and 'minCount' to be used with 'sentinel'
// to serve as upper- and lower-bounds
namespace WHampson.Bft
{
    internal abstract class Modifier2
    {
        public Modifier2(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public abstract object GetValue();

        public abstract string GetTryParseErrorMessage();

        public abstract bool TrySetValue(string valStr);

        //public abstract bool TrySetValue(string valStr, SymbolTable sTabl);
    }

    internal abstract class Modifier2<T> : Modifier2
    {
        public Modifier2(string name) : base(name)
        {
        }

        public T Value { get; protected set; }

        public override object GetValue()
        {
            return Value;
        }
    }

    internal sealed class NameModifier : Modifier2<string>
    {
        private static readonly Regex NameFormatRegex = new Regex(@"^[a-zA-Z_][\da-zA-Z_]*$");

        public NameModifier()
            : base("name")
        {
        }

        public override string GetTryParseErrorMessage()
        {
            return "'{0}' is not a valid variable name. Variable names can only consist of "
                + "alphanumeric characters and underscores, and cannot begin with a number.";
        }

        public override bool TrySetValue(string valStr)
        {
            if (!NameFormatRegex.IsMatch(valStr))
            {
                return false;
            }

            Value = valStr;
            return true;
        }
    }

    internal sealed class KindModifier : Modifier2<string>
    {
        public KindModifier()
            : base("kind")
        {
        }

        public override string GetTryParseErrorMessage()
        {
            return "";
        }

        public override bool TrySetValue(string valStr)
        {
            Value = valStr;
            return true;
        }
    }

    internal sealed class CommentModifier : Modifier2<string>
    {
        public CommentModifier()
            : base("comment")
        {
        }

        public override string GetTryParseErrorMessage()
        {
            return "";
        }

        public override bool TrySetValue(string valStr)
        {
            Value = valStr;
            return true;
        }
    }

    internal sealed class CountModifier : Modifier2<int>
    {
        public CountModifier()
            : base("count")
        {
        }

        public override string GetTryParseErrorMessage()
        {
            return "'{0}' is not a valid count value. Value must be a non-negative binary, octal, decimal, or hexadecimal number.";
        }

        public override bool TrySetValue(string valStr)
        {
            long val;
            bool isInt = NumberUtils.TryParseInteger(valStr, out val);
            if (!isInt || (int) val < 0)
            {
                return false;
            }

            //TODO: process as math expr

            Value = (int) val;
            return true;
        }
    }

    internal sealed class SentinelModifier : Modifier2<object>
    {
        public SentinelModifier(string name)
            : base("sentinel")
        {
        }

        public override string GetTryParseErrorMessage()
        {
            return "";
        }

        public override bool TrySetValue(string valStr)
        {
            return false;
        }
    }


    internal sealed class SentinelModifier<T> : Modifier2<T> where T : IPrimitiveType
    {
        public SentinelModifier()
            : base("sentinel")
        {
        }

        public override string GetTryParseErrorMessage()
        {
            Type t = typeof(T);
            string typeName = t.Name.ToLower();

            return "'{0}' is not a valid " + typeName + " value.";
        }

        public override bool TrySetValue(string valStr)
        {
            // Get 'TryParse' method for type T
            T val = default(T);
            Type t = typeof(T);
            MethodInfo tryParseMethod = t.GetMethod("TryParse", BindingFlags.Public | BindingFlags.Static);
            object[] args = new object[] { valStr, val };

            // Invoke 'TryParse' and set value if successful
            bool success;
            if (success = (bool) tryParseMethod.Invoke(null, args))
            {
                Value = (T) args[1];
            }

            return success;
        }
    }
}
