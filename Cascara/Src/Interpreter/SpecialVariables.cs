using System;
using System.Collections.Generic;

namespace WHampson.Cascara
{
    internal static partial class ReservedWords
    {
        public static class SpecialVariables
        {
            public const string Filesize = "__FILESIZE__";
            public const string Offset = "__OFFSET__";

            public static readonly HashSet<string> AllSpecialVariables = new HashSet<string>(
                typeof(SpecialVariables).GetPublicConstants<string>());
        }
    }
}
