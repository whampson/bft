using System;
using WHampson.Cascara;
using Xunit;

namespace CascaraTests
{
    public class Test_BinaryFile_DataAccess
    {
        [Fact]
        public void Read_AllData()
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
        public void ReadWrite_Data_LittleEndian()
        {
            // Arrange
            byte[] testData = new byte[8];
            Random rand = new Random();
            rand.NextBytes(testData);

            BinaryFile bf = new BinaryFile(testData, Endianness.Little);
            int rAddress = 0x00;
            int wAddress = 0x04;
            uint expRValue = GetUInt(rAddress, testData);
            uint expWValue = 0xDEADBEEF;

            // Act
            uint rValue = bf.Get<uint>(rAddress);
            uint wValue = expWValue;
            bf.Set(wAddress, wValue);

            // Assert
            byte[] fileData = bf.ToArray();
            Assert.Equal(expRValue, rValue);
            Assert.Equal(expWValue, GetUInt(wAddress, fileData));
        }

        [Fact]
        public void ReadWrite_Data_BigEndian()
        {
            // Arrange
            byte[] testData = new byte[8];
            Random rand = new Random();
            rand.NextBytes(testData);

            BinaryFile bf = new BinaryFile(testData, Endianness.Big);
            int rAddress = 0x00;
            int wAddress = 0x04;
            uint expRValue = GetUInt_BigEndian(rAddress, testData);
            uint expWValue = 0xDEADBEEF;

            // Act
            uint rValue = bf.Get<uint>(rAddress);
            uint wValue = expWValue;
            bf.Set(wAddress, wValue);

            // Assert
            byte[] fileData = bf.ToArray();
            Assert.Equal(expRValue, rValue);
            Assert.Equal(expWValue, GetUInt_BigEndian(wAddress, fileData));
        }

        [Fact]
        public void ReadWrite_DataArray()
        {
            // Arrange
            byte[] testData = new byte[64];
            Random rand = new Random();
            rand.NextBytes(testData);

            BinaryFile bf = new BinaryFile(testData);
            int rAddress = 0x00;
            int wAddress = 0x20;
            uint[] expRValue = new uint[8];
            for (int i = 0; i < 8; i++) {
                expRValue[i] = GetUInt(rAddress + i * sizeof(uint), testData);
            }
            uint[] expWValue = new uint[8];
            for (int i = 0; i < 8; i++) {
                expWValue[i] = (uint) rand.Next(0, int.MaxValue);
            }

            // Act
            uint[] rValue = bf.Get<uint>(rAddress, 8);
            uint[] wValue = new uint[8];
            Array.Copy(expWValue, wValue, expWValue.Length);
            bf.Set(wAddress, wValue);

            // Assert
            byte[] fileData = bf.ToArray();
            Assert.Equal(expRValue, rValue);
            for (int i = 0; i < 8; i++) {
                Assert.Equal(expWValue[i], GetUInt(wAddress + i * sizeof(uint), fileData));
            }
        }

        [Fact]
        public void ReadWrite_Bool8()
        {
            // Arrange
            Random rand = new Random();
            byte[] testData = new byte[16];
            for (int i = 0; i < testData.Length; i++) {
                testData[i] = (byte) 0x00;
            }

            BinaryFile bf = new BinaryFile(testData);
            int rAddress = 0x00;
            int wAddress = 0x08;
            bool expRValue = false;
            bool expWValue = rand.Next(0, 2) != 0;

            // Act
            bool rValue = bf.Get<bool>(rAddress);
            bool wValue = expWValue;
            bf.Set(wAddress, wValue);

            // Assert
            byte[] fileData = bf.ToArray();
            Assert.Equal(expRValue, rValue);
            Assert.Equal(expWValue, GetBool8(wAddress, fileData));
        }

        [Fact]
        public void ReadWrite_Bool16()
        {
            // Arrange
            Random rand = new Random();
            byte[] testData = new byte[16];
            for (int i = 0; i < testData.Length; i++) {
                testData[i] = (byte) 0x00;
            }

            BinaryFile bf = new BinaryFile(testData);
            int rAddress = 0x00;
            int wAddress = 0x08;
            Bool16 expRValue = false;
            Bool16 expWValue = rand.Next(0, 2) != 0;

            // Act
            Bool16 rValue = bf.Get<Bool16>(rAddress);
            Bool16 wValue = expWValue;
            bf.Set(wAddress, wValue);

            // Assert
            byte[] fileData = bf.ToArray();
            Assert.Equal(expRValue, rValue);
            Assert.Equal(expWValue, GetBool16(wAddress, fileData));
        }

        [Fact]
        public void ReadWrite_Bool32()
        {
            // Arrange
            Random rand = new Random();
            byte[] testData = new byte[16];
            for (int i = 0; i < testData.Length; i++) {
                testData[i] = (byte) 0x00;
            }

            BinaryFile bf = new BinaryFile(testData);
            int rAddress = 0x00;
            int wAddress = 0x08;
            Bool32 expRValue = false;
            Bool32 expWValue = rand.Next(0, 2) != 0;

            // Act
            Bool32 rValue = bf.Get<Bool32>(rAddress);
            Bool32 wValue = expWValue;
            bf.Set(wAddress, wValue);

            // Assert
            byte[] fileData = bf.ToArray();
            Assert.Equal(expRValue, rValue);
            Assert.Equal(expWValue, GetBool32(wAddress, fileData));
        }

        [Fact]
        public void ReadWrite_Bool64()
        {
            // Arrange
            Random rand = new Random();
            byte[] testData = new byte[16];
            for (int i = 0; i < testData.Length; i++) {
                testData[i] = (byte) 0x00;
            }

            BinaryFile bf = new BinaryFile(testData);
            int rAddress = 0x00;
            int wAddress = 0x08;
            Bool64 expRValue = false;
            Bool64 expWValue = rand.Next(0, 2) != 0;

            // Act
            Bool64 rValue = bf.Get<Bool64>(rAddress);
            Bool64 wValue = expWValue;
            bf.Set(wAddress, wValue);

            // Assert
            byte[] fileData = bf.ToArray();
            Assert.Equal(expRValue, rValue);
            Assert.Equal(expWValue, GetBool64(wAddress, fileData));
        }

        [Fact]
        public void ReadWrite_Byte()
        {
            // Arrange
            byte[] testData = new byte[16];
            Random rand = new Random();
            rand.NextBytes(testData);

            BinaryFile bf = new BinaryFile(testData);
            int rAddress = 0x00;
            int wAddress = 0x08;
            byte expRValue = GetByte(rAddress, testData);
            byte expWValue = (byte) rand.Next(0, byte.MaxValue);

            // Act
            byte rValue = bf.Get<byte>(rAddress);
            byte wValue = expWValue;
            bf.Set(wAddress, wValue);

            // Assert
            byte[] fileData = bf.ToArray();
            Assert.Equal(expRValue, rValue);
            Assert.Equal(expWValue, GetByte(wAddress, fileData));
        }

        [Fact]
        public void ReadWrite_SByte()
        {
            // Arrange
            byte[] testData = new byte[16];
            Random rand = new Random();
            rand.NextBytes(testData);

            BinaryFile bf = new BinaryFile(testData);
            int rAddress = 0x00;
            int wAddress = 0x08;
            sbyte expRValue = GetSByte(rAddress, testData);
            sbyte expWValue = (sbyte) rand.Next(0, byte.MaxValue);

            // Act
            sbyte rValue = bf.Get<sbyte>(rAddress);
            sbyte wValue = expWValue;
            bf.Set(wAddress, wValue);

            // Assert
            byte[] fileData = bf.ToArray();
            Assert.Equal(expRValue, rValue);
            Assert.Equal(expWValue, GetSByte(wAddress, fileData));
        }

        [Fact]
        public void ReadWrite_Char8()
        {
            // Arrange
            byte[] testData = new byte[16];
            Random rand = new Random();
            rand.NextBytes(testData);

            BinaryFile bf = new BinaryFile(testData);
            int rAddress = 0x00;
            int wAddress = 0x08;
            Char8 expRValue = GetChar8(rAddress, testData);
            Char8 expWValue = (Char8) rand.Next(0, byte.MaxValue);

            // Act
            Char8 rValue = bf.Get<Char8>(rAddress);
            Char8 wValue = expWValue;
            bf.Set(wAddress, wValue);

            // Assert
            byte[] fileData = bf.ToArray();
            Assert.Equal(expRValue, rValue);
            Assert.Equal(expWValue, GetChar8(wAddress, fileData));
        }

        [Fact]
        public void ReadWrite_Char16()
        {
            // Arrange
            byte[] testData = new byte[16];
            Random rand = new Random();
            rand.NextBytes(testData);

            BinaryFile bf = new BinaryFile(testData);
            int rAddress = 0x00;
            int wAddress = 0x08;
            char expRValue = GetChar16(rAddress, testData);
            char expWValue = (char) rand.Next(0, ushort.MaxValue);

            // Act
            char rValue = bf.Get<char>(rAddress);
            char wValue = expWValue;
            bf.Set(wAddress, wValue);

            // Assert
            byte[] fileData = bf.ToArray();
            Assert.Equal(expRValue, rValue);
            Assert.Equal(expWValue, GetChar16(wAddress, fileData));
        }

        [Fact]
        public void ReadWrite_Double()
        {
            // Arrange
            byte[] testData = new byte[16];
            Random rand = new Random();
            rand.NextBytes(testData);

            BinaryFile bf = new BinaryFile(testData);
            int rAddress = 0x00;
            int wAddress = 0x08;
            double expRValue = GetDouble(rAddress, testData);
            double expWValue = rand.NextDouble();

            // Act
            double rValue = bf.Get<double>(rAddress);
            double wValue = expWValue;
            bf.Set(wAddress, wValue);

            // Assert
            byte[] fileData = bf.ToArray();
            Assert.Equal(expRValue, rValue);
            Assert.Equal(expWValue, GetDouble(wAddress, fileData));
        }

        [Fact]
        public void ReadWrite_Float()
        {
            // Arrange
            byte[] testData = new byte[16];
            Random rand = new Random();
            rand.NextBytes(testData);

            BinaryFile bf = new BinaryFile(testData);
            int rAddress = 0x00;
            int wAddress = 0x08;
            float expRValue = GetFloat(rAddress, testData);
            float expWValue = (float) rand.NextDouble();

            // Act
            float rValue = bf.Get<float>(rAddress);
            float wValue = expWValue;
            bf.Set(wAddress, wValue);

            // Assert
            byte[] fileData = bf.ToArray();
            Assert.Equal(expRValue, rValue);
            Assert.Equal(expWValue, GetFloat(wAddress, fileData));
        }

        [Fact]
        public void ReadWrite_Int()
        {
            // Arrange
            byte[] testData = new byte[16];
            Random rand = new Random();
            rand.NextBytes(testData);

            BinaryFile bf = new BinaryFile(testData);
            int rAddress = 0x00;
            int wAddress = 0x08;
            int expRValue = GetInt(rAddress, testData);
            int expWValue = rand.Next(0, int.MaxValue);

            // Act
            int rValue = bf.Get<int>(rAddress);
            int wValue = expWValue;
            bf.Set(wAddress, wValue);

            // Assert
            byte[] fileData = bf.ToArray();
            Assert.Equal(expRValue, rValue);
            Assert.Equal(expWValue, GetInt(wAddress, fileData));
        }

        [Fact]
        public void ReadWrite_Long()
        {
            // Arrange
            byte[] testData = new byte[16];
            Random rand = new Random();
            rand.NextBytes(testData);

            BinaryFile bf = new BinaryFile(testData);
            int rAddress = 0x00;
            int wAddress = 0x08;
            long expRValue = GetLong(rAddress, testData);
            long expWValue = (long) (rand.Next(0, int.MaxValue) | rand.Next(0, int.MaxValue) << 32);

            // Act
            long rValue = bf.Get<long>(rAddress);
            long wValue = expWValue;
            bf.Set(wAddress, wValue);

            // Assert
            byte[] fileData = bf.ToArray();
            Assert.Equal(expRValue, rValue);
            Assert.Equal(expWValue, GetLong(wAddress, fileData));
        }

        [Fact]
        public void ReadWrite_Short()
        {
            // Arrange
            byte[] testData = new byte[16];
            Random rand = new Random();
            rand.NextBytes(testData);

            BinaryFile bf = new BinaryFile(testData);
            int rAddress = 0x00;
            int wAddress = 0x08;
            short expRValue = GetShort(rAddress, testData);
            short expWValue = (short) rand.Next(0, ushort.MaxValue);

            // Act
            short rValue = bf.Get<short>(rAddress);
            short wValue = expWValue;
            bf.Set(wAddress, wValue);

            // Assert
            byte[] fileData = bf.ToArray();
            Assert.Equal(expRValue, rValue);
            Assert.Equal(expWValue, GetShort(wAddress, fileData));
        }

        [Fact]
        public void ReadWrite_UInt()
        {
            // Arrange
            byte[] testData = new byte[16];
            Random rand = new Random();
            rand.NextBytes(testData);

            BinaryFile bf = new BinaryFile(testData);
            int rAddress = 0x00;
            int wAddress = 0x08;
            uint expRValue = GetUInt(rAddress, testData);
            uint expWValue = (uint) rand.Next(0, int.MaxValue);

            // Act
            uint rValue = bf.Get<uint>(rAddress);
            uint wValue = expWValue;
            bf.Set(wAddress, wValue);

            // Assert
            byte[] fileData = bf.ToArray();
            Assert.Equal(expRValue, rValue);
            Assert.Equal(expWValue, GetUInt(wAddress, fileData));
        }

        [Fact]
        public void ReadWrite_ULong()
        {
            // Arrange
            byte[] testData = new byte[16];
            Random rand = new Random();
            rand.NextBytes(testData);

            BinaryFile bf = new BinaryFile(testData);
            int rAddress = 0x00;
            int wAddress = 0x08;
            ulong expRValue = GetULong(rAddress, testData);
            ulong expWValue = (ulong) (rand.Next(0, int.MaxValue) | rand.Next(0, int.MaxValue) << 32);

            // Act
            ulong rValue = bf.Get<ulong>(rAddress);
            ulong wValue = expWValue;
            bf.Set(wAddress, wValue);

            // Assert
            byte[] fileData = bf.ToArray();
            Assert.Equal(expRValue, rValue);
            Assert.Equal(expWValue, GetULong(wAddress, fileData));
        }

        [Fact]
        public void ReadWrite_UShort()
        {
            // Arrange
            byte[] testData = new byte[16];
            Random rand = new Random();
            rand.NextBytes(testData);

            BinaryFile bf = new BinaryFile(testData);
            int rAddress = 0x00;
            int wAddress = 0x08;
            ushort expRValue = GetUShort(rAddress, testData);
            ushort expWValue = (ushort) rand.Next(0, ushort.MaxValue);

            // Act
            ushort rValue = bf.Get<ushort>(rAddress);
            ushort wValue = expWValue;
            bf.Set(wAddress, wValue);

            // Assert
            byte[] fileData = bf.ToArray();
            Assert.Equal(expRValue, rValue);
            Assert.Equal(expWValue, GetUShort(wAddress, fileData));
        }

        // Helper functions for reading back data.
        // All are little-endian unless otherwise noted

        private byte GetByte(int addr, byte[] data)
        {
            return data[addr];
        }

        private sbyte GetSByte(int addr, byte[] data)
        {
            return (sbyte) data[addr];
        }

        private short GetShort(int addr, byte[] data)
        {
            return (short) (data[addr] | data[addr + 1] << 8);
        }

        private ushort GetUShort(int addr, byte[] data)
        {
            return (ushort) (data[addr] | data[addr + 1] << 8);
        }

        private int GetInt(int addr, byte[] data)
        {
            return (int) (data[addr]
                        | data[addr + 1] << 8
                        | data[addr + 2] << 16
                        | data[addr + 3] << 24);
        }

        private uint GetUInt(int addr, byte[] data)
        {
            return (uint) (data[addr]
                         | data[addr + 1] << 8
                         | data[addr + 2] << 16
                         | data[addr + 3] << 24);
        }

        private uint GetUInt_BigEndian(int addr, byte[] data)
        {
            return (uint) (data[addr + 3]
                         | data[addr + 2] << 8
                         | data[addr + 1] << 16
                         | data[addr + 0] << 24);
        }

        private long GetLong(int addr, byte[] data)
        {
            return data[addr]
                | ((long) data[addr + 1] << 8)
                | ((long) data[addr + 2] << 16)
                | ((long) data[addr + 3] << 24)
                | ((long) data[addr + 4] << 32)
                | ((long) data[addr + 5] << 40)
                | ((long) data[addr + 6] << 48)
                | ((long) data[addr + 7] << 56);
        }

        private ulong GetULong(int addr, byte[] data)
        {
            return data[addr]
                | ((ulong) data[addr + 1] << 8)
                | ((ulong) data[addr + 2] << 16)
                | ((ulong) data[addr + 3] << 24)
                | ((ulong) data[addr + 4] << 32)
                | ((ulong) data[addr + 5] << 40)
                | ((ulong) data[addr + 6] << 48)
                | ((ulong) data[addr + 7] << 56);
        }

        private bool GetBool8(int addr, byte[] data)
        {
            return GetByte(addr, data) != 0;
        }

        private Bool16 GetBool16(int addr, byte[] data)
        {
            return GetUShort(addr, data) != 0;
        }

        private Bool32 GetBool32(int addr, byte[] data)
        {
            return GetUInt(addr, data) != 0;
        }

        private Bool64 GetBool64(int addr, byte[] data)
        {
            return GetULong(addr, data) != 0;
        }

        private Char8 GetChar8(int addr, byte[] data)
        {
            return (Char8) GetByte(addr, data);
        }

        private char GetChar16(int addr, byte[] data)
        {
            return (char) GetUShort(addr, data);
        }

        private float GetFloat(int addr, byte[] data)
        {
            return BitConverter.Int32BitsToSingle(GetInt(addr, data));
        }

        private double GetDouble(int addr, byte[] data)
        {
            return BitConverter.Int64BitsToDouble(GetLong(addr, data));
        }
    }
}
