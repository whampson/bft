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

namespace WHampson.Cascara
{
    class Program
    {
        static void Main(string[] args)
        {
            IntPtr ptr = Marshal.AllocHGlobal(64);
            Marshal.Copy(new byte[] { 0x01, 0x00, 0xFF, 0xFF }, 0, ptr, 4);

            Types.Pointer p = (ptr + 2);
            Types.Pointer<CascaraUInt16> a = (Types.Pointer<CascaraUInt16>) p;
            Types.Pointer<CascaraInt16> b = (Types.Pointer<CascaraInt16>) p;
            Types.Pointer<CascaraBool16> c = (Types.Pointer<CascaraBool16>) (Types.Pointer) b;
            Types.Pointer<CascaraInt16> d = ptr;

            Console.WriteLine(p.Address);
            Console.WriteLine(a.Address);
            Console.WriteLine(b.Address);
            Console.WriteLine(c.Address);
            Console.WriteLine((p + 1).Address);
            Console.WriteLine((a + 1).Address);
            Console.WriteLine((b + 1).Address);
            Console.WriteLine((c + 1).Address);

            Marshal.FreeHGlobal(ptr);

            //TemplateFile tf = new TemplateFile("../../../../Test/DynamicArray.xml");
            //tf.Process<object>("../../../../Test/DynamicArray.bin");

            using (BinaryFile bFile = BinaryFile.Open("../../../../Test/DynamicArray.bin"))
            {
                Console.WriteLine(bFile.Length);
                bFile.ApplyTemplate("../../../../Test/DynamicArray.xml");
            }

            // Pause
            Console.ReadKey();
        }
    }
}
