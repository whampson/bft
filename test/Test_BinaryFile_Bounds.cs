using System;
using WHampson.Cascara;
using Xunit;

namespace CascaraTests
{
    public class Test_BinaryFile_Bounds
    {
        [Fact]
        public void BinaryFile_Bounds_ReadWrite_Byte_BeforeZero()
        {
            BinaryFile bf = new BinaryFile(16);

            Assert.Throws<ArgumentOutOfRangeException>(() => bf.Get<byte>(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => bf.Set<byte>(-1, 0xFF));
        }

        [Fact]
        public void BinaryFile_Bounds_ReadWrite_Word_BeforeZero()
        {
            BinaryFile bf = new BinaryFile(16);

            Assert.Throws<ArgumentOutOfRangeException>(() => bf.Get<uint>(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => bf.Set<uint>(-1, 0xDEADBEEF));
        }

        [Fact]
        public void BinaryFile_Bounds_ReadWrite_Byte_AtZero()
        {
            BinaryFile bf = new BinaryFile(16);

            bf.Get<byte>(0);
            bf.Set<byte>(0, 0xFF);
        }

        [Fact]
        public void BinaryFile_Bounds_ReadWrite_Word_AtZero()
        {
            BinaryFile bf = new BinaryFile(16);

            bf.Get<uint>(0);
            bf.Set<uint>(0, 0xDEADBEEF);
        }

        [Fact]
        public void BinaryFile_Bounds_ReadWrite_Byte_AtEnd()
        {
            BinaryFile bf = new BinaryFile(16);

            // Read/write last valid address in file
            bf.Get<byte>(bf.Length - sizeof(byte));
            bf.Set<byte>(bf.Length - sizeof(byte), 0xFF);
        }

        [Fact]
        public void BinaryFile_Bounds_ReadWrite_Word_AtEnd()
        {
            BinaryFile bf = new BinaryFile(16);

            // Read/write last valid address in file
            bf.Get<uint>(bf.Length - sizeof(uint));
            bf.Set<uint>(bf.Length - sizeof(uint), 0xDEADBEEF);
        }

        [Fact]
        public void BinaryFile_Bounds_ReadWrite_Byte_PastEnd()
        {
            BinaryFile bf = new BinaryFile(16);

            Assert.Throws<ArgumentOutOfRangeException>(() => bf.Get<byte>(bf.Length));
            Assert.Throws<ArgumentOutOfRangeException>(() => bf.Set<byte>(bf.Length, 0xFF));
        }

        [Fact]
        public void BinaryFile_Bounds_ReadWrite_Word_PastEnd()
        {
            BinaryFile bf = new BinaryFile(16);

            Assert.Throws<ArgumentOutOfRangeException>(() => bf.Get<uint>(bf.Length));
            Assert.Throws<ArgumentOutOfRangeException>(() => bf.Set<uint>(bf.Length, 0xDEADBEEF));
        }

        [Fact]
        public void BinaryFile_Bounds_ReadWrite_ByteArray_AcrossEnd()
        {
            BinaryFile bf = new BinaryFile(16);
            int count = 2;

            Assert.Throws<ArgumentOutOfRangeException>(() => bf.Get<byte>(bf.Length - sizeof(byte), count));
            Assert.Throws<ArgumentOutOfRangeException>(() => bf.Set<byte>(bf.Length, new byte[count]));
        }

        [Fact]
        public void BinaryFile_Bounds_ReadWrite_WordArray_AcrossEnd()
        {
            BinaryFile bf = new BinaryFile(16);
            int count = 2;

            Assert.Throws<ArgumentOutOfRangeException>(() => bf.Get<uint>(bf.Length - sizeof(uint), count));
            Assert.Throws<ArgumentOutOfRangeException>(() => bf.Set<uint>(bf.Length, new uint[count]));
        }
    }
}
