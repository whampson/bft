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

using Int32 = WHampson.Cascara.Types.Int32;
using Pointer = WHampson.Cascara.Types.Pointer;
using UInt32 = WHampson.Cascara.Types.UInt32;

namespace WHampson.Cascara
{
    class Vect3D
    {
        public Pointer<Float> X { get; set; }
        public Pointer<Float> Y { get; set; }
        public Pointer<Float> Z { get; set; }

        public override string ToString()
        {
            return string.Format("<{0}, {1}, {2}>", X.Value, Y.Value, Z.Value);
        }
    }

    class TestClass
    {
        public Vect3D Location { get; set; }
        public Pointer<Int32> Money { get; set; }
        public Pointer<Int32> NumKills { get; set; }
        public Pointer<UInt8> Health { get; set; }
        public Pointer<UInt8> Armor { get; set; }
        public ArrayPointer<Float> Misc { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            using (BinaryFile bFile = BinaryFile.Open("../../../../Test/Test.bin"))
            {
                bFile.ApplyTemplate("../../../../Test/Test.xml");

                TestClass tc = bFile.Extract<TestClass>();
                foreach (Float f in tc.Misc)
                {
                    Console.WriteLine(f);
                }
            }

            // Pause
            Console.ReadKey();
        }
    }
}
