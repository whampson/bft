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
    internal static class Keywords
    {
        // Builtin types
        public const string Byte = "byte";
        public const string Double = "double";
        public const string Dword = "dword";
        public const string Float = "float";
        public const string Int8 = "int8";
        public const string Int16 = "int16";
        public const string Int32 = "int32";
        public const string Int64 = "int64";
        public const string Qword = "qword";
        public const string Struct = "struct";
        public const string UInt8 = "uint8";
        public const string UInt16 = "uint16";
        public const string UInt32 = "uint32";
        public const string UInt64 = "uint64";
        public const string Word = "word";

        // Directives
        public const string Align = "align";
        public const string Echo = "echo";
        public const string Typedef = "typedef";

        // Modifiers
        public const string Comment = "comment";
        public const string Count = "count";
        public const string Kind = "kind";
        public const string Message = "message";
        public const string Name = "name";
        //public const string SentinelIdentifier = "sentinel";
        //public const string ThreshIdentifier = "thresh";
        public const string Typename = "typename";

        public enum BuiltinTypeId
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

        public enum DirectiveId
        {
            Align,
            Echo,
            Typedef
        }

        public enum ModifierId
        {
            Comment,
            Count,
            Kind,
            Message,
            Name,
            //Sentinel,
            //Thresh,
            Typename
        }

        public static readonly Dictionary<string, BuiltinTypeId> BuiltinTypeIdMap = new Dictionary<string, BuiltinTypeId>()
        {
            { Byte, BuiltinTypeId.UInt8 },
            { Double, BuiltinTypeId.Double },
            { Dword, BuiltinTypeId.UInt32 },
            { Float, BuiltinTypeId.Float },
            { Int8, BuiltinTypeId.Int8 },
            { Int16, BuiltinTypeId.Int16 },
            { Int32, BuiltinTypeId.Int32 },
            { Int64, BuiltinTypeId.Int64 },
            { Qword, BuiltinTypeId.UInt64 },
            { Struct, BuiltinTypeId.Struct },
            { UInt8, BuiltinTypeId.UInt8 },
            { UInt16, BuiltinTypeId.UInt16 },
            { UInt32, BuiltinTypeId.UInt32 },
            { UInt64, BuiltinTypeId.UInt64 },
            { Word, BuiltinTypeId.UInt16 }
        };

        public static readonly Dictionary<string, DirectiveId> DirectiveIdMap = new Dictionary<string, DirectiveId>()
        {
            { Align, DirectiveId.Align },
            { Echo, DirectiveId.Echo },
            { Typedef, DirectiveId.Typedef },
        };

        public static readonly Dictionary<string, ModifierId> ModifierIdMap = new Dictionary<string, ModifierId>()
        {
            { Comment, ModifierId.Comment },
            { Count, ModifierId.Count },
            { Kind, ModifierId.Kind },
            { Message, ModifierId.Message },
            { Name, ModifierId.Name },
            //{ SentinelIdentifier, Modifier.Sentinel },
            //{ ThreshIdentifier, Modifier.Thresh },
            { Typename, ModifierId.Typename },
        };
    }
}