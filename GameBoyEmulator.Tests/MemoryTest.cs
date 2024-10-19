using GameBoyEmulator.Graphics;
using GameBoyEmulator.Interrupts;
using GameBoyEmulator.Memory;
using GameBoyEmulator.Processor;

namespace GameBoyEmulator.Tests
{
    [TestFixture]
    public class MemoryTests
    {
        private CPU _cpu;
        private Registers _registers;
        private Opcode _opcode;
        private RAM _ram;
        private MMU _mmu;
        private PPU _ppu;
        private Renderer _renderer;
        private InterruptController _interruptController;
        private const int CyclesPerFrame = 70224;
        private bool DebugFrameStepThroughPerFrame = false;
        private bool DebugFrameStepThroughPerCycle = false;
        private bool GoFast = false; // Skips frames when rendering to speed up CPU cycles
        private const int ScreenWidth = 160;
        private const int ScreenHeight = 144;

        [SetUp]
        public void Setup()
        {
            _registers = new Registers();
            _ram = new RAM();
            _mmu = new MMU(_registers, _ram);
            _renderer = new Renderer(_mmu);
            _ppu = new PPU(_mmu, _renderer);
            _opcode = new Opcode(_registers, _mmu, _ram);
            _interruptController = new InterruptController(_registers, _mmu, _ram);
            _cpu = new CPU(_registers, _opcode, _ram, _interruptController);
        }

        [Test]
        public void Test_RAM_ReadWriteByte()
        {
            ushort address = 0xC000;
            byte value = 0x42;
            _ram.WriteByte(address, value);
            byte readValue = _ram.ReadByte(address);
            Assert.AreEqual(value, readValue);
        }

        [Test]
        public void Test_RAM_ReadWriteWord()
        {
            ushort address = 0xC000;
            ushort value = 0x1234;
            _ram.WriteWord(address, value);
            ushort readValue = _ram.ReadWord(address);
            Assert.AreEqual(value, readValue);
        }

        [Test]
        public void Test_MMU_ReadWriteByte_VRAM()
        {
            ushort address = 0x8000;
            byte value = 0x55;
            _mmu.WriteByte(address, value);
            byte readValue = _mmu.ReadByte(address);
            Assert.AreEqual(value, readValue);
        }

        [Test]
        public void Test_MMU_ReadWriteByte_OAM()
        {
            ushort address = 0xFE00;
            byte value = 0xAA;
            _mmu.WriteByte(address, value);
            byte readValue = _mmu.ReadByte(address);
            Assert.AreEqual(value, readValue);
        }

        [Test]
        public void Test_MMU_WriteLY_Ignored()
        {
            _mmu.WriteByte(0xFF44, 0x99);
            byte value = _mmu.ReadByte(0xFF44);
            Assert.AreEqual(0x00, value); // LY register should remain 0
        }

        [Test]
        public void Test_MMU_WriteLY_UsingWriteLYMethod()
        {
            _mmu.WriteLY(0x45);
            byte value = _mmu.ReadByte(0xFF44);
            Assert.AreEqual(0x45, value);
        }

        [Test]
        public void Test_MMU_DMA_Transfer()
        {
            // Prepare source data
            ushort sourceAddress = 0xC000;
            byte[] sourceData = new byte[0xA0];
            for (int i = 0; i < 0xA0; i++)
            {
                sourceData[i] = (byte)(i & 0xFF);
                _mmu.WriteByte((ushort)(sourceAddress + i), sourceData[i]);
            }

            // Initiate DMA transfer
            _mmu.WriteByte(0xFF46, 0xC0); // Write source high byte to DMA register

            // Verify OAM data
            for (int i = 0; i < 0xA0; i++)
            {
                byte oamData = _mmu.ReadByte((ushort)(0xFE00 + i));
                Assert.AreEqual(sourceData[i], oamData);
            }
        }

        [Test]
        public void Test_MMU_JoypadState_NoButtonsPressed()
        {
            _mmu.WriteByte(0xFF00, 0xF0); // Select button keys
            byte joypadState = _mmu.ReadByte(0xFF00);
            Assert.AreEqual(0xFF, joypadState);
        }

        //[Test]
        //public void Test_MMU_JoypadState_ButtonPressed()
        //{
        //    _mmu.WriteByte(0xFF00, 0xF0); // Select button keys
        //    _mmu.SetJoypadButtonState(4, true); // Press A button
        //    byte joypadState = _mmu.ReadByte(0xFF00);
        //    Assert.IsTrue((joypadState & 0x01) == 0x00); // A button should be pressed
        //}

        [Test]
        public void Test_MMU_JoypadState_DirectionPressed()
        {
            _mmu.WriteByte(0xFF00, 0xE0); // Select direction keys
            _mmu.SetJoypadButtonState(0, true); // Press Right button
            byte joypadState = _mmu.ReadByte(0xFF00);
            Assert.IsTrue((joypadState & 0x01) == 0x00); // Right button should be pressed
        }

        [Test]
        public void Test_MMU_JoypadState_MultipleButtonsPressed()
        {
            _mmu.WriteByte(0xFF00, 0xE0); // Select direction keys
            _mmu.SetJoypadButtonState(0, true); // Right
            _mmu.SetJoypadButtonState(1, true); // Left
            byte joypadState = _mmu.ReadByte(0xFF00);
            Assert.IsTrue((joypadState & 0x03) == 0x00); // Both Right and Left pressed
        }

        [Test]
        public void Test_MMU_JoypadState_InvalidButtonIndex()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _mmu.SetJoypadButtonState(8, true));
        }

        [Test]
        public void Test_MMU_PushPopStack()
        {
            ushort value = 0xDEAD;
            _registers.SP = 0xFFFE;
            _mmu.PushStack(value);
            Assert.AreEqual(0xFFFC, _registers.SP);
            ushort poppedValue = _mmu.PopStack();
            Assert.AreEqual(0xFFFE, _registers.SP);
            Assert.AreEqual(value, poppedValue);
        }

        [Test]
        public void Test_MMU_Indexer_ReadWrite()
        {
            ushort address = 0xC000;
            byte value = 0x77;
            _mmu[address] = value;
            byte readValue = _mmu[address];
            Assert.AreEqual(value, readValue);
        }

        [Test]
        public void Test_MMU_ReadByte_AfterWriteByte()
        {
            ushort address = 0xD000;
            byte value = 0x33;
            _mmu.WriteByte(address, value);
            byte readValue = _mmu.ReadByte(address);
            Assert.AreEqual(value, readValue);
        }

        [Test]
        public void Test_RAM_CopyToMemory()
        {
            byte[] source = new byte[] { 0x11, 0x22, 0x33, 0x44 };
            int destinationOffset = 0x8000;
            int length = source.Length;
            _ram.CopyToMemory(source, destinationOffset, length);

            for (int i = 0; i < length; i++)
            {
                byte value = _ram.ReadByte((ushort)(destinationOffset + i));
                Assert.AreEqual(source[i], value);
            }
        }

        [Test]
        public void Test_MemoryMap_LoadROM()
        {
            byte[] romData = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            MemoryMap memoryMap = new MemoryMap(_ram);
            memoryMap.LoadROM(romData);

            for (int i = 0; i < romData.Length; i++)
            {
                byte value = _ram.ReadByte((ushort)i);
                Assert.AreEqual(romData[i], value);
            }
        }

        [Test]
        public void Test_MMU_HandleDMA_CorrectlyTransfersData()
        {
            // Set up source data
            for (int i = 0; i < 0x100; i++)
            {
                _mmu.WriteByte((ushort)(0x8000 + i), (byte)i);
            }

            // Initiate DMA transfer from 0x8000
            _mmu.WriteByte(0xFF46, 0x80);

            // Verify OAM data
            for (int i = 0; i < 0xA0; i++)
            {
                byte oamData = _mmu.ReadByte((ushort)(0xFE00 + i));
                Assert.AreEqual((byte)i, oamData);
            }
        }

        //[Test]
        //public void Test_MMU_ReadByte_JoypadRegister()
        //{
        //    // No buttons pressed, select buttons
        //    _mmu.WriteByte(0xFF00, 0xF0);
        //    byte joypadState = _mmu.ReadByte(0xFF00);
        //    Assert.AreEqual(0xFF, joypadState);

        //    // Press 'Start' button
        //    _mmu.SetJoypadButtonState(7, true);
        //    joypadState = _mmu.ReadByte(0xFF00);
        //    Assert.AreEqual(0xF7, joypadState);
        //}

        [Test]
        public void Test_MMU_WriteByte_LYRegisterIgnored()
        {
            _mmu.WriteByte(0xFF44, 0xAB);
            byte lyValue = _mmu.ReadByte(0xFF44);
            Assert.AreEqual(0x00, lyValue);
        }

        [Test]
        public void Test_MMU_WriteByte_DMA_Transfer()
        {
            // Prepare source data
            for (int i = 0; i < 0xA0; i++)
            {
                _mmu.WriteByte((ushort)(0xC000 + i), (byte)(0xFF - i));
            }

            // Start DMA transfer
            _mmu.WriteByte(0xFF46, 0xC0);

            // Verify OAM data
            for (int i = 0; i < 0xA0; i++)
            {
                byte oamData = _mmu.ReadByte((ushort)(0xFE00 + i));
                Assert.AreEqual((byte)(0xFF - i), oamData);
            }
        }
    }
}
