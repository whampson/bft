using System;
using WHampson.Cascara;
using WHampson.Cascara.Interpreter;
using Xunit;
using Xunit.Abstractions;

namespace Cascara.Tests
{
    public class LayoutExceptionTests : CascaraTestFramework
    {
        public LayoutExceptionTests(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        public void Create_AllNull()
        {
            // Arrange
            BinaryLayout layout = null;
            ISourceEntity srcElem = null;
            string msg = null;
            Exception innerException = null;

            string expectedMessage = string.Format("Exception of type '{0}' was thrown.", typeof(LayoutException).FullName);
            string expectedDetailedMessage = expectedMessage;
            int expectedLineNum = 0;
            int expectedLinePos = 0;
            Exception expectedInnerException = null;

            // Act
            LayoutException ex = LayoutException.Create<LayoutException>(layout, innerException, srcElem, msg);

            // Assert
            Assert.Equal(expectedMessage, ex.Message);
            Assert.Equal(expectedDetailedMessage, ex.DetailedMessage);
            Assert.Equal(expectedLineNum, ex.LineNumber);
            Assert.Equal(expectedLinePos, ex.LinePosition);
            Assert.Equal(expectedInnerException, ex.InnerException);
        }

        [Fact]
        public void Create_WithMessage()
        {
            // Arrange
            BinaryLayout layout = null;
            ISourceEntity srcElem = null;
            string msg = "Test.";
            Exception innerException = null;

            string expectedMessage = msg;
            string expectedDetailedMessage = expectedMessage;
            int expectedLineNum = 0;
            int expectedLinePos = 0;
            Exception expectedInnerException = null;

            // Act
            LayoutException ex = LayoutException.Create<LayoutException>(layout, innerException, srcElem, msg);

            // Assert
            Assert.Equal(expectedMessage, ex.Message);
            Assert.Equal(expectedDetailedMessage, ex.DetailedMessage);
            Assert.Equal(expectedLineNum, ex.LineNumber);
            Assert.Equal(expectedLinePos, ex.LinePosition);
            Assert.Equal(expectedInnerException, ex.InnerException);
        }

        [Fact]
        public void Create_NestedWithMessage()
        {
            // Inputs
            BinaryLayout layout = null;
            ISourceEntity srcElem = null;
            string msg = "Failed to process layout.";
            Exception innerException = new ArgumentNullException("foo", "The parameter was null."); ;

            // Expected results
            string expectedMessage = msg;
            string expectedDetailedMessage = msg + @"
Caused by:
  ArgumentNullException: The parameter was null.
    Parameter name: foo";
            int expectedLineNum = 0;
            int expectedLinePos = 0;
            Exception expectedInnerException = innerException;

            // Execution
            LayoutException ex = LayoutException.Create<LayoutException>(layout, innerException, srcElem, msg);

            // Assertion
            Assert.Equal(expectedMessage, ex.Message);
            Assert.Equal(expectedDetailedMessage, ex.DetailedMessage);
            Assert.Equal(expectedLineNum, ex.LineNumber);
            Assert.Equal(expectedLinePos, ex.LinePosition);
            Assert.Equal(expectedInnerException, ex.InnerException);
        }

//        [Fact]
//        public void Create_NestedLayoutExceptionWithMessage()
//        {
//            // Inputs
//            BinaryLayout layout = BinaryLayout.Create("<cascaraLayout><foo/></cascaraLayout>");
//            ISourceElement srcElem = null;
//            string msg = "Failed to process layout.";
//            Exception innerException = new ArgumentNullException("foo", "The parameter was null."); ;

//            // Expected results
//            string expectedMessage = msg;
//            string expectedDetailedMessage = msg + @"
//Caused by:
//  ArgumentNullException: The parameter was null.
//    Parameter name: foo";
//            int expectedLineNum = 0;
//            int expectedLinePos = 0;
//            Exception expectedInnerException = innerException;

//            // Execution
//            LayoutException ex = LayoutException.Create<LayoutException>(layout, innerException, srcElem, msg);

//            // Assertion
//            Assert.Equal(expectedMessage, ex.Message);
//            Assert.Equal(expectedDetailedMessage, ex.DetailedMessage);
//            Assert.Equal(expectedLineNum, ex.LineNumber);
//            Assert.Equal(expectedLinePos, ex.LinePosition);
//            Assert.Equal(expectedInnerException, ex.InnerException);
//        }
    }
}
