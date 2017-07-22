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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using WHampson.Cascara.Types;

using Int32 = WHampson.Cascara.Types.Int32;

namespace WHampson.Cascara
{
    public class Vect3D
    {
        public Pointer<Float> X { get; set; }
        public Pointer<Float> Y { get; set; }
        public Pointer<Float> Z { get; set; }

        public override string ToString()
        {
            return "{ " + string.Format("X: {0}, Y: {1}, Z: {2}", X[0], Y[0], Z[0]) + " }";
        }
    }

    public class PlayerInfo
    {
        public Vect3D Location { get; set; }
        public Pointer<Int32> Money { get; set; }
        public Pointer<Int32> NumKills { get; set; }
        public Pointer<Int8> Health { get; set; }
        public Pointer<Int8> Armor { get; set; }
        public Vect3D Location2 { get; set; }
    }

    public class TestClass
    {
        public PlayerInfo PLYR { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Read template and map to object instance
            TemplateFile template = new TemplateFile("../../../../Test/DynamicArray.xml");
            TestClass test = template.Process<TestClass>("../../../../Test/DynamicArray.bin");
            //Console.WriteLine(test.PLYR.Location);
            //Console.WriteLine(test.PLYR.Money.Value);
            //Console.WriteLine(test.PLYR.NumKills.Value);
            //Console.WriteLine(test.PLYR.Health.Value);
            //Console.WriteLine(test.PLYR.Armor.Value);
            //Console.WriteLine(test.PLYR.Location2);

            // Pause
            Console.ReadKey();
        }
    }
}
