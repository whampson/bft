using System;
using System.Collections.Generic;

namespace WHampson.Cascara
{
    internal static partial class ReservedWords
    {
        public static class Parameters
        {
            public const string Comment = "comment";
            public const string Count = "count";
            public const string Kind = "kind";
            public const string Message = "message";
            public const string Name = "name";
            public const string Path = "path";
            public const string Raw = "raw";
            public const string Value = "value";

            public static readonly HashSet<string> AllParameters = new HashSet<string>(
                typeof(Parameters).GetPublicConstants<string>());
        }
    }
}
