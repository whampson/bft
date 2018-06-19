using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WHampson.Cascara;

namespace WHampson.CascaraDemo
{
    // TODO: rename to CascaraExamples

    class Program
    {
        const string LayoutXml = @"
<cascaraLayout name='a layout' description='A Test Layout'>
    <struct name='test'>
        <int name='foo'/>
        <struct name='nest'>
            <int name='bar'/>
            <align count='2'/>
            <char name='str' count='4'/>
            <echo message='${__OFFSET__}'/>
            <echo message='${__GLOBALOFFSET__}'/>
        </struct>
    </struct>
    <echo message='$OffsetOf(test.nest.str)'/>
    <echo message='$GlobalOffsetOf(test.nest.str)'/>
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

            Console.WriteLine(file.GetStructure("test"));
            Console.WriteLine(file.GetPrimitive<int>("test.foo"));
            Console.WriteLine("{0:X}", file.GetPrimitive<int>("test.nest.bar").Value);
            Console.WriteLine(file.GetPrimitive<Char8>("test.nest.str"));

            Console.ReadKey();
        }
    }
}
