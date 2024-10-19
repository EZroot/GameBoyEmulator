namespace GameBoyEmulator.Tests
{
    [TestFixture]
    public class MemoryTests
    {
        private Memory _memory;

        [SetUp]
        public void Setup()
        {
            _memory = new Memory();
        }

        [Test]
        public void Test_ReadByte_ShouldReturnCorrectValue()
        {

            _memory.WriteByte(0x1234, 0xAB);

            byte value = _memory.ReadByte(0x1234);

            Assert.AreEqual(0xAB, value);
        }

        [Test]
        public void Test_WriteByte_ShouldStoreCorrectValue()
        {

            _memory.WriteByte(0x1234, 0xCD);

            Assert.AreEqual(0xCD, _memory.ReadByte(0x1234));
        }

        [Test]
        public void Test_WriteByte_IgnoreWriteToLY()
        {

            _memory.WriteByte(0xFF44, 0x12); 

            Assert.AreNotEqual(0x12, _memory.ReadByte(0xFF44)); 
        }

        [Test]
        public void Test_ReadWord_ShouldReturnCorrectValue()
        {

            _memory.WriteByte(0x1000, 0x34);
            _memory.WriteByte(0x1001, 0x12);

            ushort value = _memory.ReadWord(0x1000);

            Assert.AreEqual(0x1234, value);
        }

        [Test]
        public void Test_WriteWord_ShouldStoreCorrectValue()
        {

            _memory.WriteWord(0x2000, 0x5678);

            Assert.AreEqual(0x78, _memory.ReadByte(0x2000));
            Assert.AreEqual(0x56, _memory.ReadByte(0x2001));
        }

        [Test]
        public void Test_SetJoypadButtonState_ShouldUpdateState()
        {

            _memory.SetJoypadButtonState(0, true); 

            byte joypadState = _memory.ReadByte(0xFF00); 
            Assert.IsTrue((joypadState & 0x01) == 0); 
        }

        [Test]
        public void Test_SetJoypadButtonState_ShouldThrowIfInvalidButton()
        {

            Assert.Throws<ArgumentOutOfRangeException>(() => _memory.SetJoypadButtonState(8, true)); 
        }

        [Test]
        public void Test_DMA_Transfer()
        {

            _memory.WriteByte(0x1000, 0xAA); 
            _memory.WriteByte(0xFF46, 0x10); 

            _memory.HandleDMA(0x10); 

            Assert.AreEqual(0xAA, _memory.ReadByte(0xFE00)); 
        }

        [Test]
        public void Test_JoypadState_DirectionsSelected()
        {

            _memory.WriteByte(0xFF00, 0x10); 
            _memory.SetJoypadButtonState(0, true); 
            _memory.SetJoypadButtonState(1, true); 

            byte joypadState = _memory.ReadByte(0xFF00);

            Assert.AreEqual(0xFC, joypadState); 
        }

        [Test]
        public void Test_JoypadState_ButtonsSelected()
        {

            _memory.WriteByte(0xFF00, 0x20); 
            _memory.SetJoypadButtonState(6, true); 
            _memory.SetJoypadButtonState(7, true); 

            byte joypadState = _memory.ReadByte(0xFF00);

            Assert.AreEqual(0xF3, joypadState); 
        }

        [Test]
        public void Test_LoadROM_ShouldCopyROMDataToMemory()
        {

            byte[] romData = new byte[0x8000]; 
            romData[0] = 0x01; 

            _memory.LoadROM(romData);

            Assert.AreEqual(0x01, _memory.ReadByte(0x0000)); 
        }
    }

}
