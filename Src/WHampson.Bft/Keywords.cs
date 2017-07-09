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
        // boolN    -> uintN
        // byte     -> uint8
        // char     -> uint16
        // (u)int   -> (u)int32
        // (u)long  -> (u)int64
        // (u)short -> (u)int16



        // Root element
        public static readonly Keyword Bft = new Keyword(BftIdentifier);

        // Data types
        public static readonly Keyword Bool = new Keyword(BoolIdentifier);
        public static readonly Keyword Bool8 = new Keyword(Bool8Identifier);
        public static readonly Keyword Bool16 = new Keyword(Bool16Identifier);
        public static readonly Keyword Bool32 = new Keyword(Bool32Identifier);
        public static readonly Keyword Bool64 = new Keyword(Bool64Identifier);
        public static readonly Keyword Byte = new Keyword(ByteIdentifier);
        public static readonly Keyword Char = new Keyword(CharIdentifier);
        public static readonly Keyword Double = new Keyword(DoubleIdentifier);
        public static readonly Keyword Float = new Keyword(FloatIdentifier);
        public static readonly Keyword Half = new Keyword(HalfIdentifier);
        public static readonly Keyword Int = new Keyword(IntIdentifier);
        public static readonly Keyword Int8 = new Keyword(Int8Identifier);
        public static readonly Keyword Int16 = new Keyword(Int16Identifier);
        public static readonly Keyword Int32 = new Keyword(Int32Identifier);
        public static readonly Keyword Int64 = new Keyword(Int64Identifier);
        public static readonly Keyword Long = new Keyword(LongIdentifier);
        public static readonly Keyword Short = new Keyword(ShortIdentifier);
        public static readonly Keyword Struct = new Keyword(StructIdentifier);
        public static readonly Keyword UInt = new Keyword(UIntIdentifier);
        public static readonly Keyword UInt8 = new Keyword(UInt8Identifier);
        public static readonly Keyword UInt16 = new Keyword(UInt16Identifier);
        public static readonly Keyword UInt32 = new Keyword(UInt32Identifier);
        public static readonly Keyword UInt64 = new Keyword(UInt64Identifier);
        public static readonly Keyword ULong = new Keyword(ULong);
        public static readonly Keyword UShort = new Keyword(UShort);

        // Directives
        public static readonly Keyword Align = new Keyword(AlignIdentifier);
        public static readonly Keyword Echo = new Keyword(EchoIdentifier);
        public static readonly Keyword Typedef = new Keyword(TypedefIdentifier);

        // Modifiers
        public static readonly Keyword Comment = new Keyword(CommentIdentifier);
        public static readonly Keyword Count = new Keyword(CountIdentifier);
        public static readonly Keyword Kind = new Keyword(KindIdentifier);
        public static readonly Keyword Name = new Keyword(NameIdentifier);
        public static readonly Keyword Typename = new Keyword(TypenameIdentifier);

        public static readonly Dictionary<string, Keyword> KeywordMap = new Dictionary<string, Keyword>()
        {
            // Root element
            { BftIdentifier, Bft },

            // Data types
            { BoolIdentifier, Bool },
            { Bool8Identifier, Bool8 },
            { Bool16Identifier, Bool16 },
            { Bool32Identifier, Bool32 },
            { Bool64Identifier, Bool64 },
            { ByteIdentifier, Byte },
            { CharIdentifier, Char },
            { DoubleIdentifier, Double },
            { FloatIdentifier, Float },
            { HalfIdentifier, Half },
            { IntIdentifier, Int },
            { Int8Identifier, Int8 },
            { Int16Identifier, Int16 },
            { Int32Identifier, Int32 },
            { Int64Identifier, Int64 },
            { LongIdentifier, Long },
            { ShortIdentifier, Short },
            { StructIdentifier, Struct },
            { UIntIdentifier, UInt },
            { UInt8Identifier, UInt8 },
            { UInt16Identifier, UInt16 },
            { UInt32Identifier, UInt32 },
            { UInt64Identifier, UInt64 },
            { ULongIdentifier, ULong },
            { UShortIdentifier, UShort },

            // Directives
            { AlignIdentifier, Align },
            { EchoIdentifier, Echo },
            { TypedefIdentifier, Typedef },

            // Modifiers
            { CommentIdentifier, Comment },
            { CountIdentifier, Count },
            { KindIdentifier, Kind },
            { NameIdentifier, Name },
            { TypenameIdentifier, Typename }
        };

        // Root element
        private const string BftIdentifier = "bft";

        // Data types
        private const string BoolIdentifier = "bool";
        private const string Bool8Identifier = "bool8";
        private const string Bool16Identifier = "bool16";
        private const string Bool32Identifier = "bool32";
        private const string Bool64Identifier = "bool64";
        private const string ByteIdentifier = "byte";
        private const string CharIdentifier = "char";
        //private const string Char8Identifier = "char8";
        //private const string Char16dentifier = "char16";
        //private const string Char32Identifier = "char32";
        //private const string Char64Identifier = "char64";
        private const string DoubleIdentifier = "double";
        //private const string DWordIdentifier = "dword";
        private const string FloatIdentifier = "float";
        private const string HalfIdentifier = "half";
        private const string IntIdentifier = "int";
        private const string Int8Identifier = "int8";
        private const string Int16Identifier = "int16";
        private const string Int32Identifier = "int32";
        private const string Int64Identifier = "int64";
        private const string LongIdentifier = "long";
        //private const string QWordIdentifier = "qword";
        private const string ShortIdentifier = "short";
        private const string StructIdentifier = "struct";
        private const string UIntIdentifier = "uint";
        private const string UInt8Identifier = "uint8";
        private const string UInt16Identifier = "uint16";
        private const string UInt32Identifier = "uint32";
        private const string UInt64Identifier = "uint64";
        private const string ULongIdentifier = "ulong";
        private const string UShortIdentifier = "ushort";
        //private const string WordIdentifier = "word";

        // Directives
        private const string AlignIdentifier = "align";
        private const string EchoIdentifier = "echo";
        private const string TypedefIdentifier = "typedef";

        // Modifiers
        private const string CommentIdentifier = "comment";
        private const string CountIdentifier = "count";
        private const string KindIdentifier = "kind";
        private const string NameIdentifier = "name";
        private const string TypenameIdentifier = "typename";





        //// Builtin types
        //public const string Byte = "byte";
        //public const string Double = "double";
        //public const string Dword = "dword";
        //public const string Float = "float";
        //public const string Int8 = "int8";
        //public const string Int16 = "int16";
        //public const string Int32 = "int32";
        //public const string Int64 = "int64";
        //public const string Qword = "qword";
        //public const string Struct = "struct";
        //public const string UInt8 = "uint8";
        //public const string UInt16 = "uint16";
        //public const string UInt32 = "uint32";
        //public const string UInt64 = "uint64";
        //public const string Word = "word";

        //// Directives
        //public const string Align = "align";
        //public const string Echo = "echo";
        //public const string Typedef = "typedef";

        //// Modifiers
        //public const string Comment = "comment";
        //public const string Count = "count";
        //public const string Kind = "kind";
        //public const string Message = "message";
        //public const string Name = "name";
        ////public const string SentinelIdentifier = "sentinel";
        ////public const string ThreshIdentifier = "thresh";
        //public const string Typename = "typename";

        //public enum BuiltinTypeId
        //{
        //    Double,
        //    Float,
        //    Int8,
        //    Int16,
        //    Int32,
        //    Int64,
        //    Struct,
        //    UInt8,
        //    UInt16,
        //    UInt32,
        //    UInt64
        //}

        //public enum DirectiveId
        //{
        //    Align,
        //    Echo,
        //    Typedef
        //}

        //public enum ModifierId
        //{
        //    Comment,
        //    Count,
        //    Kind,
        //    Message,
        //    Name,
        //    //Sentinel,
        //    //Thresh,
        //    Typename
        //}

        //public static readonly Dictionary<string, BuiltinTypeId> BuiltinTypeIdMap = new Dictionary<string, BuiltinTypeId>()
        //{
        //    { Byte, BuiltinTypeId.UInt8 },
        //    { Double, BuiltinTypeId.Double },
        //    { Dword, BuiltinTypeId.UInt32 },
        //    { Float, BuiltinTypeId.Float },
        //    { Int8, BuiltinTypeId.Int8 },
        //    { Int16, BuiltinTypeId.Int16 },
        //    { Int32, BuiltinTypeId.Int32 },
        //    { Int64, BuiltinTypeId.Int64 },
        //    { Qword, BuiltinTypeId.UInt64 },
        //    { Struct, BuiltinTypeId.Struct },
        //    { UInt8, BuiltinTypeId.UInt8 },
        //    { UInt16, BuiltinTypeId.UInt16 },
        //    { UInt32, BuiltinTypeId.UInt32 },
        //    { UInt64, BuiltinTypeId.UInt64 },
        //    { Word, BuiltinTypeId.UInt16 }
        //};

        //public static readonly Dictionary<string, DirectiveId> DirectiveIdMap = new Dictionary<string, DirectiveId>()
        //{
        //    { Align, DirectiveId.Align },
        //    { Echo, DirectiveId.Echo },
        //    { Typedef, DirectiveId.Typedef },
        //};

        //public static readonly Dictionary<string, ModifierId> ModifierIdMap = new Dictionary<string, ModifierId>()
        //{
        //    { Comment, ModifierId.Comment },
        //    { Count, ModifierId.Count },
        //    { Kind, ModifierId.Kind },
        //    { Message, ModifierId.Message },
        //    { Name, ModifierId.Name },
        //    //{ SentinelIdentifier, Modifier.Sentinel },
        //    //{ ThreshIdentifier, Modifier.Thresh },
        //    { Typename, ModifierId.Typename },
        //};
    }
}