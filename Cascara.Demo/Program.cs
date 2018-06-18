using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WHampson.Cascara;

namespace WHampson.CascaraDemo
{
    class Program
    {
        const string LayoutXml = @"
<cascaraLayout name='a layout' description='A Test Layout'>
    <int name='foo'/>
    <int name='bar'/>
    <byte name='baz'/>
    <byte name='bee'/>
    <char name='str' count='4'/>
</cascaraLayout>";

        static void Main(string[] args)
        {
            byte[] data = { 0xBE, 0xBA, 0xFE, 0xCA, 0xEF, 0xBE, 0xAD, 0xDE, 0xAA, 0x55, 0x61, 0x62 ,0x63, 0x00 };

            BinaryLayout layout = BinaryLayout.Parse(LayoutXml);
            BinaryFile file = new BinaryFile(data);
            file.ApplyLayout(layout);

            Console.WriteLine("{0:X}", file.Get<int>(0));
            Console.WriteLine("{0:X}", file.Get<int>(4));
            Console.WriteLine("{0:X}", file.Get<byte>(8));
            Console.WriteLine("{0:X}", file.Get<byte>(9));

            Console.WriteLine("{0:X}", file.GetPrimitive<int>("foo").Value);
            Console.WriteLine("{0:X}", file.GetPrimitive<int>("foo").Value);
            Console.WriteLine("{0:X}", file.GetPrimitive<byte>("baz").Value);
            Console.WriteLine("{0:X}", file.GetPrimitive<byte>("bee").Value);
            Console.WriteLine("{0:X}", file.GetPrimitive<bool>("str")[0].Value);

            Console.ReadKey();
        }
    }
}
