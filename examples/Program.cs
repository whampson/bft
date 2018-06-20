using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WHampson.Cascara;

namespace WHampson.CascaraExamples
{
    class Program
    {
        const string LayoutXml = @"
<cascaraLayout name='a layout' description='A Test Layout'>
    <struct name='test'>
        <int name='foo'/>
        <struct name='nest'>
            <int name='bar'/>
            <byte name='abc' count='2'/>
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

            // LayoutScript layout = LayoutScript.Parse(LayoutXml);
            LayoutScript layout = LayoutScript.Load("test.xml");
            BinaryFile file = new BinaryFile(data);
            file.ApplyLayout(layout);

            var w = file.GetPrimitive<int>("test.foo");
            var v = file.GetPrimitive<Char8>("test.nest.str");
            var a = v.ReinterpretCast<byte>();
            var b = file.GetStructure("test");
            Console.WriteLine(w);
            Console.WriteLine(v);
            Console.WriteLine(a);
            Console.WriteLine(b);
            Console.WriteLine(file);
            Console.WriteLine(layout);
        }
    }
}
