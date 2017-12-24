using System;
using WHampson.Cascara;
using Xunit;

namespace Cascara.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            // Arrange
            string xmlData = "<cascaraLayout name='Foobar'></cascaraLayout>";

            // Act/assert
            var ex = Assert.Throws<LayoutException>(() => BinaryLayout.Create(xmlData));
            Assert.Equal(ex.Message, "Empty layout.");
        }
    }
}
