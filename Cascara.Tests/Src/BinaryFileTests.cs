using System;
using WHampson.Cascara;
using Xunit;
using Xunit.Abstractions;

namespace Cascara.Tests
{
    public class BinaryFileTests : CascaraTestFramework
    {
        public BinaryFileTests(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        public void ByteArray_ctor()
        {
            // Arrange
            byte[] arr = new byte[] { 0xCA, 0xFE, 0xBA, 0xBE };
            uint expectedValue = 0xCAFEBABE;

            // Act
            BinaryFile bf = new BinaryFile(arr, Endianness.Big);

            // Assert
            uint val = bf.GetValue<uint>(0);
            byte[] data = bf.GetBytes(0, arr.Length);
            Assert.Equal(expectedValue, val);
            Assert.Equal(arr, data);
        }

        [Fact]
        public void GetSetValue()
        {
            // Arrange
            BinaryFile bf = new BinaryFile(16);
            int expectedValue = 0xBADF00D;

            // Act
            bf.SetValue(0, expectedValue);
            int val1 = bf.GetValue<int>(0);
            byte val2 = bf.GetValue<byte>(0);
            ushort val3 = bf.GetValue<ushort>(0);

            // Assert
            Assert.Equal(expectedValue, val1);
            Assert.Equal(expectedValue & 0xFF, val2);
            Assert.Equal(expectedValue & 0xFFFF, val3);
        }

        [Fact]
        public void BoundsCheck()
        {
            // Arrange
            BinaryFile bf = new BinaryFile(16);

            // Act, assert
            Assert.ThrowsAny<ArgumentException>(() => bf.GetByte(-1));
            Assert.ThrowsAny<ArgumentException>(() => bf.GetByte(bf.Length));
            Assert.ThrowsAny<ArgumentException>(() => bf.GetBytes(-1, 0));
            Assert.ThrowsAny<ArgumentException>(() => bf.GetBytes(bf.Length, 0));
            Assert.ThrowsAny<ArgumentException>(() => bf.GetBytes(0, bf.Length + 1));
            Assert.ThrowsAny<ArgumentException>(() => bf.GetBytes(bf.Length - 1, 2));
            Assert.ThrowsAny<ArgumentException>(() => bf.GetValue<int>(-1));
            Assert.ThrowsAny<ArgumentException>(() => bf.GetValue<int>(bf.Length));
            Assert.ThrowsAny<ArgumentException>(() => bf.GetValue<int>(bf.Length - 3));
            Assert.ThrowsAny<ArgumentException>(() => bf.SetByte(-1, 0xFF));
            Assert.ThrowsAny<ArgumentException>(() => bf.SetByte(bf.Length, 0xFF));
            Assert.ThrowsAny<ArgumentException>(() => bf.SetBytes(-1, new byte[1]));
            Assert.ThrowsAny<ArgumentException>(() => bf.SetBytes(bf.Length, new byte[1]));
            Assert.ThrowsAny<ArgumentException>(() => bf.SetBytes(0, new byte[bf.Length + 1]));
            Assert.ThrowsAny<ArgumentException>(() => bf.SetBytes(bf.Length - 1, new byte[2]));
            Assert.ThrowsAny<ArgumentException>(() => bf.SetValue<int>(-1, 0xBADBEEF));
            Assert.ThrowsAny<ArgumentException>(() => bf.SetValue<int>(bf.Length, 0xBADBEEF));
            Assert.ThrowsAny<ArgumentException>(() => bf.SetValue<int>(bf.Length - 3, 0xBADBEEF));
        }
    }
}
