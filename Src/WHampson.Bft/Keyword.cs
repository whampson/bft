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

namespace WHampson.Bft
{
    internal static class Keyword
    {
        // Builtin types and aliases
        public const string ByteIdentifier = "byte";
        public const string DoubleIdentifier = "double";
        public const string DwordIdentifier = "dword";
        public const string FloatIdentifier = "float";
        public const string Int8Identifier = "int8";
        public const string Int16Identifier = "int16";
        public const string Int32Identifier = "int32";
        public const string Int64Identifier = "int64";
        public const string QwordIdentifier = "qword";
        public const string StructIdentifier = "struct";
        public const string UInt8Identifier = "uint8";
        public const string UInt16Identifier = "uint16";
        public const string UInt32Identifier = "uint32";
        public const string UInt64Identifier = "uint64";
        public const string WordIdentifier = "word";

        // Directives
        public const string AlignIdentifier = "align";
        public const string EchoIdentifier = "echo";
        public const string TypedefIdentifier = "typedef";

        // Modifiers
        public const string CommentIdentifier = "comment";
        public const string CountIdentifier = "count";
        public const string KindIdentifier = "kind";
        public const string MessageIdentifier = "message";
        public const string NameIdentifier = "name";
        public const string SentinelIdentifier = "sentinel";
        public const string ThreshIdentifier = "thresh";
        //public const string TypenameIdentifier = "typename";

        public enum BuiltinType
        {
            Double,
            Float,
            Int8,
            Int16,
            Int32,
            Int64,
            Struct,
            UInt8,
            UInt16,
            UInt32,
            UInt64
        }

        public enum Directive
        {
            Align,
            Echo,
            Typedef
        }

        public enum Modifier
        {
            Comment,
            Count,
            Kind,
            Message,
            Name,
            Sentinel,
            Thresh,
            //Typename
        }

        public static readonly Dictionary<string, BuiltinType> BuiltinTypeIdentifierMap = new Dictionary<string, BuiltinType>()
        {
            { ByteIdentifier, BuiltinType.UInt8 },
            { DoubleIdentifier, BuiltinType.Double },
            { DwordIdentifier, BuiltinType.UInt32 },
            { FloatIdentifier, BuiltinType.Float },
            { Int8Identifier, BuiltinType.Int8 },
            { Int16Identifier, BuiltinType.Int16 },
            { Int32Identifier, BuiltinType.Int32 },
            { Int64Identifier, BuiltinType.Int64 },
            { QwordIdentifier, BuiltinType.UInt64 },
            { StructIdentifier, BuiltinType.Struct },
            { UInt8Identifier, BuiltinType.UInt8 },
            { UInt16Identifier, BuiltinType.UInt16 },
            { UInt32Identifier, BuiltinType.UInt32 },
            { UInt64Identifier, BuiltinType.UInt64 },
            { WordIdentifier, BuiltinType.UInt16 }
        };

        public static readonly Dictionary<string, Directive> DirectiveIdentifierMap = new Dictionary<string, Directive>()
        {
            { AlignIdentifier, Directive.Align },
            { EchoIdentifier, Directive.Echo },
            { TypedefIdentifier, Directive.Typedef },
        };

        public static readonly Dictionary<string, Modifier> ModifierIdentifierMap = new Dictionary<string, Modifier>()
        {
            { CommentIdentifier, Modifier.Comment },
            { CountIdentifier, Modifier.Count },
            { KindIdentifier, Modifier.Kind },
            { MessageIdentifier, Modifier.Message },
            { NameIdentifier, Modifier.Name },
            { SentinelIdentifier, Modifier.Sentinel },
            { ThreshIdentifier, Modifier.Thresh },
            //{ TypenameIdentifier, Modifier.Typename },
        };
    }
}