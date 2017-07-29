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
using System.Reflection;
using System.Runtime.InteropServices;
using WHampson.Cascara.Types;

using Pointer = WHampson.Cascara.Types.Pointer;

namespace WHampson.Cascara
{
    class Program
    {
        const string TestDir = "../../../../Test/Gta3Save";
        const string BinDir = TestDir + "/Binaries";

        static void Main(string[] args)
        {
            string xmlPath = TestDir + "/PCSave.xml";
            string binPath = BinDir + "/PC/GTA3sf1.b";

            using (BinaryFile bFile = BinaryFile.Open(binPath))
            {
                bFile.ApplyTemplate(xmlPath);
                Gta3PCSave gameSave = bFile.Extract<Gta3PCSave>();
                Console.WriteLine(gameSave.SimpleVars.SaveTitle.StringValue);

                //bFile.Write(TestDir + "/out.b");
            }

            // Pause
            Console.ReadKey();
        }
    }

    class SimpleVars
    {
        public Pointer<Char16> SaveTitle { get; set; }
    }

    class Gta3PCSave
    {
        public SimpleVars SimpleVars { get; set; }
    }
}
