using System;
using Xunit;

namespace Cascara.Tests
{
    public class AssertExtension : Assert
    {
        public static T ThrowsWithMessage<T>(Func<object> testCode, string message)
            where T : Exception
        {
            var ex = Assert.Throws<T>(testCode);
            Assert.Equal(ex.Message, message);

            return ex;
        }
    }
}
