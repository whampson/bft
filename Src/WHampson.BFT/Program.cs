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
using System.Xml.Linq;
using WHampson.BFT.Types;

using Int32 = WHampson.BFT.Types.Int32;

namespace WHampson.BFT
{
    public class Vect3D
    {
        public Pointer<Float> X { get; set; }
        public Pointer<Float> Y { get; set; }
        public Pointer<Float> Z { get; set; }
    }

    public class PlayerInfo
    {
        public Pointer<Int8> Health { get; set; }
        public Pointer<Int8> Armor { get; set; }
        public Pointer<Int32> Money { get; set; }
        public Vect3D Location { get; set; }        // Nested structure
        public Pointer<Int32>[] WeaponAmmo { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Allocate fixed memory on heap
            IntPtr ptr = Marshal.AllocHGlobal(128);

            // Fill buffer with test data
            CreateTestData(ptr);

            // Read template and map to PlayerInfo instance
            TemplateFile template = new TemplateFile("../../Test.xml");
            string a = template["platform"];
            //Console.WriteLine(a ?? "Undefined");
            

            PlayerInfo info;
            //int bytesRead = bft.Apply(ptr, 128, out info);

            //PlayerInfo info
            //info = BinaryFileTemplate.ProcessTemplate<PlayerInfo>("../../Test.xml", ptr, 128);

            //// Print all data
            //Console.WriteLine("Health:\t\t" + info.Health.Value);
            //Console.WriteLine("Armor:\t\t" + info.Armor.Value);
            //Console.WriteLine("Money:\t\t" + info.Money.Value);
            //Console.WriteLine("Location.X:\t" + info.Location.X.Value);
            //Console.WriteLine("Location.Y:\t" + info.Location.Y.Value);
            //Console.WriteLine("Location.Z:\t" + info.Location.Z.Value);
            //for (int i = 0; i < info.WeaponAmmo.Length; i++)
            //{
            //    Console.WriteLine("WeaponAmmo[" + i + "]:\t" + info.WeaponAmmo[i].Value);
            //}

            // Free heap-allocated memory
            Marshal.FreeHGlobal(ptr);

            // Pause
            Console.ReadKey();
        }

        static void CreateTestData(IntPtr ptr)
        {
            byte[] data = { 0x61, 0x45, 0x00, 0x00, // Health (97), Armor (69), (align 2 bytes)
                            0xF2, 0xBE, 0x65, 0x01, // Money (23445234)
                            0x48, 0x61, 0x28, 0x44, // Location.X (673.52)
                            0x3D, 0xC2, 0xA7, 0xC4, // Location.Y (-1342.07)
                            0x71, 0x3D, 0xC0, 0x41, // Location.Z (24.03)
                            0x01, 0x00, 0x00, 0x00, // WeaponAmmo[0] (Fist) (1)
                            0x01, 0x00, 0x00, 0x00, // WeaponAmmo[1] (Bat) (1)
                            0x15, 0x16, 0x00, 0x00, // WeaponAmmo[2] (Pistol) (5653)
                            0xE3, 0x24, 0x00, 0x00, // WeaponAmmo[3] (Uzi) (9443)
                            0x1B, 0x01, 0x00, 0x00, // WeaponAmmo[4] (Shotgun) (283)
                            0x38, 0x15, 0x00, 0x00, // WeaponAmmo[5] (AK) (5432)
                            0x2A, 0x19, 0x00, 0x00, // WeaponAmmo[6] (M16) (6442)
                            0x14, 0x02, 0x00, 0x00, // WeaponAmmo[7] (Sniper) (532)
                            0x53, 0x00, 0x00, 0x00, // WeaponAmmo[8] (Rocket Launcher) (83)
                            0x8A, 0x10, 0x00, 0x00, // WeaponAmmo[9] (Flamethrower) (4234)
                            0x0A, 0x00, 0x00, 0x00, // WeaponAmmo[10] (Molotov) (10)
                            0x2B, 0x00, 0x00, 0x00, // WeaponAmmo[11] (Grenades) (43)
                            0x00, 0x00, 0x00, 0x00  // WeaponAmmo[12] (Detonator) (0)
            };
            Marshal.Copy(data, 0, ptr, data.Length);
        }
    }
}
