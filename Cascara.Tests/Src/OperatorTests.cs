using System.Linq;
using WHampson.Cascara;
using Xunit;

namespace Cascara.Tests
{
    public class OperatorTests
    {
        [Fact]
        public void ValueOf_GetVariableNames()
        {
            // Arrange
            string data = "4 * (${foo.bar} + ${baz} * 6) + 7";

            string[] expectedVarNames = { "foo.bar", "baz" };

            // Act
            foreach (string var in CascaraOperator.ValueOf.GetVariableNames(data))
            {
                Assert.True(expectedVarNames.Contains(var));
            }

            // Assert
        }
    }
}
