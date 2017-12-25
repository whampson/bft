using WHampson.Cascara;
using Xunit;

namespace Cascara.Tests
{
    public class BinaryLayoutTests
    {
        [Fact]
        public void EmptyLayout()
        {
            // Arrange
            string xmlData = "<cascaraLayout name='Foobar'></cascaraLayout>";

            // Act/assert
            var ex = Assert.Throws<LayoutException>(() => BinaryLayout.Create(xmlData));
            Assert.Equal(ex.Message, Resources.LayoutExceptionEmptyLayout);
        }
    }
}
