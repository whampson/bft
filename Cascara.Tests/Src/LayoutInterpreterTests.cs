using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WHampson.Cascara;
using Xunit;
using Xunit.Abstractions;

namespace Cascara.Tests
{
    public class LayoutInterpreterTests : CascaraTestFramework
    {
        public LayoutInterpreterTests(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        public void ScratchArea()
        {
            StringWriter sw = new StringWriter();
            BinaryFile file = BinaryFile.Load(@"C:\Users\Wes\Desktop\GTA3LCSsf1.b", Endianness.Little);
            BinaryLayout layout = BinaryLayout.Load(@"C:\Users\Wes\Desktop\testLayout.xml");
            file.ApplyLayout(layout, sw);
            Output.WriteLine(sw.ToString());
        }
    }
}
