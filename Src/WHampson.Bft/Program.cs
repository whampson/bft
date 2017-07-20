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
using WHampson.Bft.Types;

using Int32 = WHampson.Bft.Types.Int32;

namespace WHampson.Bft
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

            //SymbolTableTest();


            // TODO: handle arrays in symbol table

            // Pause
            Console.ReadKey();
        }

        static void SymbolTableTest()
        {
            /* {
             *     struct {
             *         int32 baz;
             *     } foo;
             *     
             *     struct {
             *         int32 biz;
             *         struct {
             *             int32 b;
             *             int32 c;
             *         } a;
             *     } bar;
             * } tabl
             * 
             */
        //    SymbolTableEntry foo = new SymbolTableEntry(TypeInfo.CreateStruct(new List<XElement>(), 4), 0);
        //    SymbolTableEntry baz = new SymbolTableEntry(TypeInfo.CreatePrimitive(typeof(Int32)), 0);
        //    SymbolTableEntry bar = new SymbolTableEntry(TypeInfo.CreateStruct(new List<XElement>(), 12), 4);
        //    SymbolTableEntry biz = new SymbolTableEntry(TypeInfo.CreatePrimitive(typeof(Int32)), 4);
        //    SymbolTableEntry a = new SymbolTableEntry(TypeInfo.CreateStruct(new List<XElement>(), 8), 8);
        //    SymbolTableEntry b = new SymbolTableEntry(TypeInfo.CreatePrimitive(typeof(Int32)), 8);
        //    SymbolTableEntry c = new SymbolTableEntry(TypeInfo.CreatePrimitive(typeof(Int32)), 12);

        //    SymbolTable tabl = new SymbolTable();
        //    tabl.Entries.Add("foo", foo);
        //    tabl.Entries.Add("bar", bar);
        //    SymbolTable fooTabl = new SymbolTable("foo", tabl);
        //    foo.Child = fooTabl;
        //    fooTabl.Entries.Add("baz", baz);
        //    SymbolTable barTabl = new SymbolTable("bar", tabl);
        //    bar.Child = barTabl;
        //    barTabl.Entries.Add("biz", biz);
        //    barTabl.Entries.Add("a", a);
        //    SymbolTable aTabl = new SymbolTable("a", barTabl);
        //    a.Child = aTabl;
        //    aTabl.Entries.Add("b", b);
        //    aTabl.Entries.Add("c", c);

        //    Console.WriteLine(aTabl);

        //    SymbolTableEntry e = tabl.GetEntry("bar");
        //    if (e == null)
        //    {
        //        Console.WriteLine("null");
        //    }
        //    else
        //    {
        //        Console.WriteLine(e);
        //    }
        }
    }
}
