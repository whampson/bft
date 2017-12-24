using System;
using NUnit.Framework;
using WHampson.Cascara;

namespace WHampson.Cascara.Tests
{
    [TestFixture]
    public class BinaryLayoutTests
    {
        [Test]
        public void TestTest()
        {
            // Arrange
            string xmlData = "<cascaraLayout name='Foobar'></cascaraLayout>";

            // Act/assert
            Assert.That(() => BinaryLayout.Create(xmlData),
                Throws.TypeOf<LayoutException>()
                .With.Message.EqualTo("Empty layout."));
        }
    }
}
