using System;
using WHampson.Cascara;
using Xunit;

namespace CascaraTests
{
    public class Test_BinaryFile_DataAccess
    {
        [Fact]
        public void ReadBackData()
        {
            // Arrange
            byte[] testData = new byte[256];
            Random rand = new Random();
            rand.NextBytes(testData);

            // Act
            BinaryFile bf = new BinaryFile(testData);
            byte[] outData = bf.ToArray();

            // Assert
            Assert.Equal(testData, outData);
        }

        [Fact]
        public void GetSet_ByAddress()
        {
            // Arrange
            byte[] testData = new byte[8];
            Random rand = new Random();
            rand.NextBytes(testData);

            BinaryFile bf = new BinaryFile(testData, Endianness.Little);
            int rAddress = 0x00;
            int wAddress = 0x04;
            uint expRValue = GetUIntLittleEndian(rAddress, testData);
            uint expWValue = 0xDEADBEEF;

            // Act
            uint rValue = bf.Get<uint>(rAddress);
            uint wValue = expWValue;
            bf.Set(wAddress, wValue);

            // Assert
            byte[] fileData = bf.ToArray();
            Assert.Equal(expRValue, rValue);
            Assert.Equal(expWValue, GetUIntLittleEndian(wAddress, fileData));
        }

        private uint GetUIntLittleEndian(int addr, byte[] data)
        {
            return (uint) (data[addr] | data[addr + 1] << 8 | data[addr + 2] << 16 | data[addr + 3] << 24);
        }
    }
}
