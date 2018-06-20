using System;
using Xunit;

namespace Cascara.Tests.Extensions
{
    public class AssertExtensions : Assert
    {
        public static T ThrowsWithMessage<T>(Func<object> testCode, string message)
            where T : Exception
        {
            var ex = Assert.Throws<T>(testCode);
            Assert.Equal(message, ex.Message);

            return ex;
        }

        public static T ThrowsWithMessage<T>(Func<object> testCode, string fmt, params object[] args)
            where T : Exception
        {
            return ThrowsWithMessage<T>(testCode, string.Format(fmt, args));
        }

        public static T ThrowsWithMessageContaining<T>(Func<object> testCode, string message)
            where T : Exception
        {
            var ex = Assert.Throws<T>(testCode);
            Assert.Contains(message, ex.Message);

            return ex;
        }

        public static T ThrowsWithMessageContaining<T>(Func<object> testCode, string fmt, params object[] args)
            where T : Exception
        {
            return ThrowsWithMessageContaining<T>(testCode, string.Format(fmt, args));
        }

        public static T ThrowsAnyWithMessage<T>(Func<object> testCode, string message)
            where T : Exception
        {
            var ex = Assert.ThrowsAny<T>(testCode);
            Assert.Equal(message, ex.Message);

            return ex;
        }

        public static T ThrowsAnyWithMessage<T>(Func<object> testCode, string fmt, params object[] args)
            where T : Exception
        {
            return ThrowsAnyWithMessage<T>(testCode, string.Format(fmt, args));
        }

        public static T ThrowsAnyWithMessageContaining<T>(Func<object> testCode, string message)
            where T : Exception
        {
            var ex = Assert.ThrowsAny<T>(testCode);
            Assert.Contains(message, ex.Message);

            return ex;
        }

        public static T ThrowsAnyWithMessageContaining<T>(Func<object> testCode, string fmt, params object[] args)
            where T : Exception
        {
            return ThrowsAnyWithMessageContaining<T>(testCode, string.Format(fmt, args));
        }
    }
}
