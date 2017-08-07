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
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using WHampson.Cascara.Types;

using Pointer = WHampson.Cascara.Types.Pointer;

namespace WHampson.Cascara
{
    class Program
    {
        const string TestDir = "../../TestData/Gta3Save";
        const string BinDir = TestDir + "/Binaries";

        static void Main(string[] args)
        {
            string xmlPath = TestDir + "/PCSave.xml";
            string binPath = BinDir + "/PC/GTA3sf1.b";

            using (BinaryFile bFile = BinaryFile.Open(binPath))
            {
                bFile.ApplyTemplate(xmlPath);
                Gta3PCSave gameSave = bFile.ExtractData<Gta3PCSave>();
                ArrayPointer<int> GlobalVariables = bFile.GetArrayPointer<int>("SimpleVars.Scripts.Data.GlobalVariables");
                for (int i = 0; i < GlobalVariables.Count; i++)
                {
                    GlobalVariables[i] = 0;
                }

                //bFile.Write(BinDir + "/PC/GTA3sf2.b");
            }

            // Pause
            Console.ReadKey();
        }

        private static void PrintString(BinaryFile bFile, string name)
        {
            int len = bFile.GetElemCount(name);
            name = Regex.Replace(name, @"\[\d+\]$", "");

            string s = "";
            for (int i = 0; i < len; i++)
            {
                char ch = bFile.GetValue<char>(name + "[" + i + "]");
                if (ch == 0)
                {
                    break;
                }
                s += ch;
            }

            Console.WriteLine(s);
        }

        private static string GetStringValue(Pointer<Char16> ptr)
        {
            string s = "";
            int i = 0;
            char c = (char) ptr[0];
            while (c != 0)
            {
                s += c;
                c = (char) ptr[++i];
            }

            return s;
        }

        private static bool IsCharType(object o)
        {
            if (!(o is IConvertible))
            {
                return false;
            }

            IConvertible conv = (IConvertible) o;
            return conv.GetTypeCode() == TypeCode.Char;
        }

        public static bool IsAssignableToGenericType(Type givenType, Type genericType)
        {
            Type[] interfaceTypes = givenType.GetInterfaces();

            foreach (Type type in interfaceTypes)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == genericType)
                {
                    return true;
                }
            }

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
            {
                return true;
            }

            Type baseType = givenType.BaseType;
            if (baseType == null)
            {
                return false;
            }

            return IsAssignableToGenericType(baseType, genericType);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 0, Size = 12)]
    struct RwV3d
    {
        public RwV3d(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public float X { get; }
        public float Y { get; }
        public float Z { get; }

        public override string ToString()
        {
            return string.Format("<{0}, {1}, {2}>", X, Y, Z);
        }
    }

    class SimpleVars
    {
        public ArrayPointer<Char16> SaveTitle { get; set; }
        public Pointer<RwV3d> CameraCoords { get; set; }
    }

    class Gta3PCSave
    {
        public SimpleVars SimpleVars { get; set; }
        public Pointer<uint> Checksum { get; set; }
    }
}
