using GameBoyEmulator.Graphics;
using GameBoyEmulator.Interrupts;
using GameBoyEmulator.Memory;
using GameBoyEmulator.Processor;

namespace GameBoyEmulator.Tests
{
    [TestFixture]
    public class PPUTests
    {
        private CPU _cpu;
        private Registers _registry;
        private Opcode _opcode;
        private RAM _ram;
        private MMU _mmu;
        private PPU _ppu;
        private Renderer _renderer;
        private InterruptController _interruptController;
        private const int CyclesPerFrame = 70224;
        private bool DebugFrameStepThroughPerFrame = false;
        private bool DebugFrameStepThroughPerCycle = false;
        private bool GoFast = false; //Skips frames when renderering to speed up cpu cycles
        private const int ScreenWidth = 160;
        private const int ScreenHeight = 144;

        [SetUp]
        public void Setup()
        {
            _registry = new Registers();
            _ram = new RAM();
            _mmu = new MMU(_registry, _ram);
            _renderer = new Renderer(_mmu);
            _ppu = new PPU(_mmu, _renderer);
            _opcode = new Opcode(_registry, _mmu, _ram);
            _interruptController = new InterruptController(_registry, _mmu, _ram);
            _cpu = new CPU(_registry, _opcode, _ram, _interruptController);
        }

        [Test]
        public void Test_InitialScanline_ShouldBeZero()
        {
            Assert.AreEqual(0, _ppu.CurrentScanline);
        }

        [Test]
        public void Test_LCDDisabled_ShouldResetPPUState()
        {
            _mmu.WriteByte(0xFF40, 0x00); // Disable LCD
            _ppu.Update(456); // Update PPU by one scanline's worth of cycles
            Assert.That(_ppu.CurrentScanline, Is.EqualTo(0));
            Assert.That(_mmu.ReadByte(0xFF44), Is.EqualTo(0)); // LY register should be zero
        }

        //[Test]
        //public void Test_LYRegister_UpdatesCorrectly()
        //{
        //    _mmu.WriteByte(0xFF40, 0x80); // Enable LCD
        //    _ppu.Update(456); // One full scanline
        //    Assert.AreEqual(1, _ppu.CurrentScanline);
        //    Assert.AreEqual(1, _mmu.ReadByte(0xFF44));
        //}

        [Test]
        public void Test_VBlankInterrupt_TriggeredAtLine144()
        {
            _mmu.WriteByte(0xFF40, 0x80); // Enable LCD

            for (int i = 0; i <= 144; i++)
            {
                _ppu.Update(456); // Advance one scanline per iteration
            }

            byte stat = _mmu.ReadByte(0xFF41);
            Assert.AreEqual(1, stat & 0x03); // Should be in mode 1 (V-Blank)

            byte interruptFlags = _mmu.ReadByte(0xFF0F);
            Assert.IsTrue((interruptFlags & InterruptFlags.VBlank) != 0);
        }


        [Test]
        public void Test_LCDSTATInterrupt_TriggeredOnLYCMatch()
        {
            _mmu.WriteByte(0xFF40, 0x80); // Enable LCD
            _mmu.WriteByte(0xFF45, 10); // Set LYC to 10
            byte stat = _mmu.ReadByte(0xFF41);
            stat |= 0x44; // Enable LYC=LY interrupt (bit 6)
            _mmu.WriteByte(0xFF41, stat);

            for (int i = 0; i <= 10; i++)
            {
                _ppu.Update(456);
            }

            byte interruptFlags = _mmu.ReadByte(0xFF0F);
            Assert.IsTrue((interruptFlags & InterruptFlags.LCDSTAT) != 0);
        }

        //[Test]
        //public void Test_LCDMode_SetsCorrectly()
        //{
        //    _mmu.WriteByte(0xFF40, 0x80); // Enable LCD

        //    _ppu.Update(79); // Less than mode 2 duration
        //    byte stat = _mmu.ReadByte(0xFF41);
        //    Assert.AreEqual(2, stat & 0x03); // Should be in mode 2

        //    _ppu.Update(1); // Transition to mode 3 at 80 cycles
        //    stat = _mmu.ReadByte(0xFF41);
        //    Assert.AreEqual(3, stat & 0x03); // Should be in mode 3

        //    _ppu.Update(172); // Finish mode 3 (total cycles = 252)
        //    stat = _mmu.ReadByte(0xFF41);
        //    Assert.AreEqual(0, stat & 0x03); // Should be in mode 0

        //    _ppu.Update(204); // Finish mode 0 (total cycles = 456)
        //    stat = _mmu.ReadByte(0xFF41);
        //    Assert.AreEqual(2, stat & 0x03); // Should be back to mode 2 for next scanline
        //}



        [Test]
        public void Test_Renderer_CreatesFrameBuffer()
        {
            byte[,] frameBuffer = _renderer.GetScreenBuffer();
            Assert.IsNotNull(frameBuffer);
            Assert.AreEqual(ScreenHeight, frameBuffer.GetLength(0));
            Assert.AreEqual(ScreenWidth, frameBuffer.GetLength(1));
        }

        [Test]
        public void Test_BackgroundRendering_Disabled()
        {
            _mmu.WriteByte(0xFF40, 0x80); // LCDC with BG disabled
            _renderer.RenderScanline(0);
            byte[,] frameBuffer = _renderer.GetScreenBuffer();

            for (int x = 0; x < ScreenWidth; x++)
            {
                Assert.That(frameBuffer[0, x], Is.EqualTo(0)); // Background should not be drawn
            }
        }

        [Test]
        public void Test_BackgroundRendering_Enabled()
        {
            _mmu.WriteByte(0xFF40, 0x91); // LCDC with BG enabled and BG Tile Map at 0x9C00
            _mmu.WriteByte(0xFF42, 0x00); // SCY (Scroll Y)
            _mmu.WriteByte(0xFF43, 0x00); // SCX (Scroll X)

            // Initialize BGP (Background Palette Data) to default value
            _mmu.WriteByte(0xFF47, 0xFC); // Non-zero palette data

            // Set up tile data in VRAM at 0x8000
            ushort tileDataAddress = 0x8000;
            for (int i = 0; i < 16; i++)
            {
                _mmu.WriteByte((ushort)(tileDataAddress + i), 0xFF); // Fill tile with pixels
            }

            // Set up background tile map to point to our tile at 0x9C00
            ushort bgTileMapAddress = 0x9C00; // BG Tile Map selected via LCDC bit 3
            for (int i = 0; i < 32 * 32; i++)
            {
                _mmu.WriteByte((ushort)(bgTileMapAddress + i), 0x00); // Point to tile index 0
            }

            _renderer.RenderScanline(0);
            byte[,] frameBuffer = _renderer.GetScreenBuffer();
            bool nonZeroPixelFound = false;
            for (int x = 0; x < ScreenWidth; x++)
            {
                if (frameBuffer[0, x] != 0)
                {
                    nonZeroPixelFound = true;
                    break;
                }
            }
            Assert.IsTrue(nonZeroPixelFound);
        }


        [Test]
        public void Test_SpriteRendering_Disabled()
        {
            _mmu.WriteByte(0xFF40, 0x80); // LCDC with sprites disabled
            _renderer.RenderScanline(0);
            byte[,] frameBuffer = _renderer.GetScreenBuffer();
            // Further assertions can be added with proper setup
        }
        [Test]
        public void Test_SpriteRendering_Enabled()
        {
            _mmu.WriteByte(0xFF40, 0x82); // LCDC with sprites enabled

            // Write sprite attributes to OAM
            _mmu.WriteByte(0xFE00, 16); // Y position (16 - 16 = 0 on screen)
            _mmu.WriteByte(0xFE01, 8);  // X position (8 - 8 = 0 on screen)
            _mmu.WriteByte(0xFE02, 0);  // Tile index
            _mmu.WriteByte(0xFE03, 0);  // Attributes

            // Write sprite tile data to VRAM
            ushort tileAddress = 0x8000;
            for (int i = 0; i < 16; i++)
            {
                _mmu.WriteByte((ushort)(tileAddress + i), 0xFF); // Fill tile with pixels
            }

            // Initialize sprite palette to non-zero value
            _mmu.WriteByte(0xFF48, 0xE4); // OBP0 palette data

            _renderer.RenderScanline(0);
            byte[,] frameBuffer = _renderer.GetScreenBuffer();
            bool spritePixelFound = false;
            for (int x = 0; x < ScreenWidth; x++)
            {
                if (frameBuffer[0, x] != 0)
                {
                    spritePixelFound = true;
                    break;
                }
            }
            Assert.IsTrue(spritePixelFound);
        }



        [Test]
        public void Test_CoincidenceFlag_SetWhen_LY_Equals_LYC()
        {
            _mmu.WriteByte(0xFF45, 5); // Set LYC to 5
            _ppu.OverrideCurrentScanline(5);
            _mmu.WriteLY(5);
            _ppu.CheckLYCMatch();
            byte stat = _mmu.ReadByte(0xFF41);
            Assert.IsTrue((stat & 0x04) != 0); // Coincidence flag should be set
        }

        [Test]
        public void Test_CoincidenceFlag_ResetWhen_LY_NotEquals_LYC()
        {
            _mmu.WriteByte(0xFF45, 5); // Set LYC to 5
            _ppu.OverrideCurrentScanline(4);
            _mmu.WriteLY(4);
            _ppu.CheckLYCMatch();
            byte stat = _mmu.ReadByte(0xFF41);
            Assert.IsFalse((stat & 0x04) != 0); // Coincidence flag should be reset
        }

        [Test]
        public void Test_SpritePriority_Behavior()
        {
            // Implement test for sprite priority handling
        }



        [Test]
        public void Test_PPU_Modes_CycleAccurate()
        {
            _mmu.WriteByte(0xFF40, 0x80); // Enable LCD
            int totalCycles = 0;
            while (totalCycles < CyclesPerFrame)
            {
                _ppu.Update(4);
                totalCycles += 4;
                // Additional checks can be added here
            }
        }

        [Test]
        public void Test_WindowRendering_Enabled()
        {
            _mmu.WriteByte(0xFF40, 0xA1); // Enable LCD, BG, and Window
            _mmu.WriteByte(0xFF4B, 7); // WX
            _mmu.WriteByte(0xFF4A, 0); // WY
            _renderer.RenderScanline(0);
            // Additional assertions can be added with proper setup
        }

        [Test]
        public void Test_TileDataSelect_0x8000()
        {
            _mmu.WriteByte(0xFF40, 0x10); // Set tile data select to 0x8000
            // Implement test for tile data selection
        }

        [Test]
        public void Test_TileDataSelect_0x8800()
        {
            _mmu.WriteByte(0xFF40, 0x00); // Set tile data select to 0x8800
            // Implement test for tile data selection with signed indices
        }

        [Test]
        public void Test_SpriteSize_8x8()
        {
            _mmu.WriteByte(0xFF40, 0x00); // Set sprite size to 8x8
            // Implement test for sprite size
        }

        [Test]
        public void Test_SpriteSize_8x16()
        {
            _mmu.WriteByte(0xFF40, 0x04); // Set sprite size to 8x16
            // Implement test for sprite size
        }
    }
}
