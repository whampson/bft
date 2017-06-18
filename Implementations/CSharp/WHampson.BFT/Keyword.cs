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

using System.Collections.Generic;

namespace WHampson.BFT
{
    internal class Keyword
    {
        public static readonly Keyword Double   = new Keyword(KeywordType.PrimitiveType);
        public static readonly Keyword Float    = new Keyword(KeywordType.PrimitiveType);
        public static readonly Keyword Int8     = new Keyword(KeywordType.PrimitiveType);
        public static readonly Keyword Int16    = new Keyword(KeywordType.PrimitiveType);
        public static readonly Keyword Int32    = new Keyword(KeywordType.PrimitiveType);
        public static readonly Keyword Int64    = new Keyword(KeywordType.PrimitiveType);
        public static readonly Keyword Struct   = new Keyword(KeywordType.StructType);
        public static readonly Keyword UInt8    = new Keyword(KeywordType.PrimitiveType);
        public static readonly Keyword UInt16   = new Keyword(KeywordType.PrimitiveType);
        public static readonly Keyword UInt32   = new Keyword(KeywordType.PrimitiveType);
        public static readonly Keyword UInt64   = new Keyword(KeywordType.PrimitiveType);

        public static readonly Keyword Align    = new Keyword(KeywordType.Directive);
        public static readonly Keyword Echo     = new Keyword(KeywordType.Directive);
        public static readonly Keyword Typedef  = new Keyword(KeywordType.Directive);

        public static readonly Keyword Comment  = new Keyword(KeywordType.Modifier);
        public static readonly Keyword Count    = new Keyword(KeywordType.Modifier);
        public static readonly Keyword Kind     = new Keyword(KeywordType.Modifier);
        public static readonly Keyword Message  = new Keyword(KeywordType.Modifier);
        public static readonly Keyword Name     = new Keyword(KeywordType.Modifier);
        public static readonly Keyword Sentinel = new Keyword(KeywordType.Modifier);
        public static readonly Keyword Thresh   = new Keyword(KeywordType.Modifier);
        public static readonly Keyword Typename = new Keyword(KeywordType.Modifier);

        public static readonly Dictionary<string, Keyword> IdentifierMap = new Dictionary<string, Keyword>()
        {
            // Data types
            { "byte", Keyword.UInt8 },
            { "double", Keyword.Double },
            { "dword", Keyword.UInt32 },
            { "float", Keyword.Float },
            { "int8", Keyword.Int8 },
            { "int16", Keyword.Int16 },
            { "int32", Keyword.Int32 },
            { "int64", Keyword.Int64 },
            { "qword", Keyword.UInt64 },
            { "struct", Keyword.Struct },
            { "uint8", Keyword.UInt8 },
            { "uint16", Keyword.UInt16 },
            { "uint32", Keyword.UInt32 },
            { "uint64", Keyword.UInt64 },
            { "word", Keyword.UInt16 },

            // Directives
            { "align", Keyword.Align },
            { "echo", Keyword.Echo },
            { "typedef", Keyword.Typedef },

            // Modifiers
            { "comment", Keyword.Comment },
            { "count", Keyword.Count },
            { "kind", Keyword.Kind },
            { "message", Keyword.Message },
            { "name", Keyword.Name },
            { "sentinel", Keyword.Sentinel },
            { "thresh", Keyword.Thresh },
            { "typename", Keyword.Typename },
        };

        public enum KeywordType
        {
            Directive,
            Modifier,
            PrimitiveType,
            StructType
        }

        private Keyword(KeywordType type)
        {
            Type = type;
        }

        public KeywordType Type { get; }
    }
}