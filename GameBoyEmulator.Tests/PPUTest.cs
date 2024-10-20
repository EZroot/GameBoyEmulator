using GameBoyEmulator.Graphics;
using GameBoyEmulator.Interrupts;
using GameBoyEmulator.Memory;
using GameBoyEmulator.Processor;
namespace GameBoyEmulator.Tests
{
    [TestFixture]
    public class PPUTests
    {
        private GameBoyEmulator.Timer _timer;
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
        private bool GoFast = false; 
        private const int ScreenWidth = 160;
        private const int ScreenHeight = 144;
        [SetUp]
        public void Setup()
        {
            _registry = new Registers();
            _ram = new RAM();
            _timer = new Timer(_ram);
            _mmu = new MMU(_registry, _ram);
            _renderer = new Renderer(_mmu);
            _ppu = new PPU(_mmu, _renderer);
            _opcode = new Opcode(_registry, _mmu, _ram);
            _interruptController = new InterruptController(_registry, _mmu, _ram);
            _cpu = new CPU(_registry, _opcode, _ram, _interruptController);
            GameBoyEmulator.Debug.Debugger.EnableDebugModeForTests();
            Debug.Debugger.dDebugPPU = true;
        }
        [Test]
        public void Test_InitialScanline_ShouldBeZero()
        {
            Assert.AreEqual(0, _ppu.CurrentScanline, "Initial scanline should be 0.");
        }
        [Test]
        public void Test_LCDDisabled_ShouldResetScanlineAndCycles()
        {
            _mmu.WriteByte(0xFF40, 0x00); 
            _ppu.Update(456);
            Assert.AreEqual(0, _ppu.CurrentScanline, "Scanline should reset to 0 when LCD is disabled.");
            Assert.AreEqual(0, _mmu.ReadByte(0xFF44), "LY register should reset to 0 when LCD is disabled.");
        }
        [Test]
        public void Test_LYCMatchInterrupt_ShouldTriggerWhenLYEqualsLYC()
        {
            _mmu.WriteByte(0xFF40, 0x80); 
            _mmu.WriteByte(0xFF45, 10);   
            for (int i = 0; i < 4560; i += 456)
            {
                _ppu.Update(456);
                _cpu.Step(); 
            }
            byte stat = _mmu.ReadByte(0xFF41);
            Assert.IsTrue((stat & 0x04) != 0, "LYC match should set the corresponding STAT flag.");
        }
        [Test]
        public void Test_VBlankInterrupt_ShouldTriggerAtScanline144()
        {
            _mmu.WriteByte(0xFF40, 0x80); 
            _registry.IME = false;
            ExecuteFrame();
            Assert.AreEqual(144, _ppu.CurrentScanline, "VBlank should start at scanline 144.");
            byte interruptFlags = _mmu.ReadByte(0xFF0F);
            Assert.IsTrue((interruptFlags & InterruptFlags.VBlank) != 0, "VBlank interrupt should be triggered.");
        }
        private void ExecuteFrame()
        {
            int cycles = 0;
            while (cycles < CyclesPerFrame)
            {
                int stepCycles = _cpu.Step();
                _ppu.Update(stepCycles);
                _timer.Update(stepCycles);  
                cycles += stepCycles;
            }
        }
        [Test]
        public void Test_PPUUpdatesScreenBuffer_AfterFullFrame()
        {
            for (int i = 0; i < CyclesPerFrame; i += 456)
            {
                _ppu.Update(456);
            }
            byte[,] screenBuffer = _renderer.GetScreenBuffer();
            bool screenUpdated = false;
            for (int y = 0; y < ScreenHeight; y++)
            {
                for (int x = 0; x < ScreenWidth; x++)
                {
                    if (screenBuffer[y, x] != 0)
                    {
                        screenUpdated = true;
                        break;
                    }
                }
                if (screenUpdated) break;
            }
            Assert.IsTrue(screenUpdated, "Screen buffer should be updated after rendering a full frame.");
        }
        [Test]
        public void Test_PPUScanlineRendering_ProperModeTransitions()
        {
            _ppu.Update(80);  
            Assert.AreEqual(2, _mmu.ReadByte(0xFF41) & 0x03, "PPU should be in OAM mode after 80 cycles.");
            _ppu.Update(172); 
            Assert.AreEqual(3, _mmu.ReadByte(0xFF41) & 0x03, "PPU should be in VRAM mode after OAM cycles.");
            _ppu.Update(204); 
            Assert.AreEqual(0, _mmu.ReadByte(0xFF41) & 0x03, "PPU should be in HBlank mode after VRAM cycles.");
        }
        private int CountRenderedSpritesOnScanline(int scanline)
        {
            byte[,] screenBuffer = _renderer.GetScreenBuffer();
            int spriteCount = 0;
            for (int x = 0; x < ScreenWidth; x++)
            {
                if (screenBuffer[scanline, x] != 0)
                {
                    spriteCount++;
                }
            }
            return spriteCount;
        }
        [Test]
        public void Test_LCDDisabled_ShouldResetPPUState()
        {
            _mmu.WriteByte(0xFF40, 0x00); 
            _ppu.Update(456); 
            Assert.That(_ppu.CurrentScanline, Is.EqualTo(0));
            Assert.That(_mmu.ReadByte(0xFF44), Is.EqualTo(0)); 
        }
        [Test]
        public void Test_VBlankInterrupt_TriggeredAtLine144()
        {
            _mmu.WriteByte(0xFF40, 0x80); 
            for (int i = 0; i <= 144; i++)
            {
                _ppu.Update(456); 
            }
            byte stat = _mmu.ReadByte(0xFF41);
            Assert.AreEqual(1, stat & 0x03); 
            byte interruptFlags = _mmu.ReadByte(0xFF0F);
            Assert.IsTrue((interruptFlags & InterruptFlags.VBlank) != 0);
        }
        [Test]
        public void Test_LCDSTATInterrupt_TriggeredOnLYCMatch()
        {
            _mmu.WriteByte(0xFF40, 0x80); 
            _mmu.WriteByte(0xFF45, 10); 
            byte stat = _mmu.ReadByte(0xFF41);
            stat |= 0x44; 
            _mmu.WriteByte(0xFF41, stat);
            for (int i = 0; i <= 10; i++)
            {
                _ppu.Update(456);
            }
            byte interruptFlags = _mmu.ReadByte(0xFF0F);
            Assert.IsTrue((interruptFlags & InterruptFlags.LCDSTAT) != 0);
        }
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
            _mmu.WriteByte(0xFF40, 0x80); 
            _renderer.RenderScanline(0);
            byte[,] frameBuffer = _renderer.GetScreenBuffer();
            for (int x = 0; x < ScreenWidth; x++)
            {
                Assert.That(frameBuffer[0, x], Is.EqualTo(0)); 
            }
        }
        [Test]
        public void Test_BackgroundRendering_Enabled()
        {
            _mmu.WriteByte(0xFF40, 0x91); 
            _mmu.WriteByte(0xFF42, 0x00); 
            _mmu.WriteByte(0xFF43, 0x00); 
            _mmu.WriteByte(0xFF47, 0xFC); 
            ushort tileDataAddress = 0x8000;
            for (int i = 0; i < 16; i++)
            {
                _mmu.WriteByte((ushort)(tileDataAddress + i), 0xFF); 
            }
            ushort bgTileMapAddress = 0x9C00; 
            for (int i = 0; i < 32 * 32; i++)
            {
                _mmu.WriteByte((ushort)(bgTileMapAddress + i), 0x00); 
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
            _mmu.WriteByte(0xFF40, 0x80); 
            _renderer.RenderScanline(0);
            byte[,] frameBuffer = _renderer.GetScreenBuffer();
        }
        [Test]
        public void Test_SpriteRendering_Enabled()
        {
            _mmu.WriteByte(0xFF40, 0x82); 
            _mmu.WriteByte(0xFE00, 16); 
            _mmu.WriteByte(0xFE01, 8);  
            _mmu.WriteByte(0xFE02, 0);  
            _mmu.WriteByte(0xFE03, 0); 
            ushort tileAddress = 0x8000;
            for (int i = 0; i < 16; i++)
            {
                _mmu.WriteByte((ushort)(tileAddress + i), 0xFF);
            }
            _mmu.WriteByte(0xFF48, 0xE4); 
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
        public void Test_SpritePriority_Behavior()
        {
        }
        [Test]
        public void Test_PPU_Modes_CycleAccurate()
        {
            _mmu.WriteByte(0xFF40, 0x80); 
            int totalCycles = 0;
            while (totalCycles < CyclesPerFrame)
            {
                _ppu.Update(4);
                totalCycles += 4;
            }
        }
        [Test]
        public void Test_WindowRendering_Enabled()
        {
            _mmu.WriteByte(0xFF40, 0xA1); 
            _mmu.WriteByte(0xFF4B, 7); 
            _mmu.WriteByte(0xFF4A, 0); 
            _renderer.RenderScanline(0);
        }
        [Test]
        public void Test_TileDataSelect_0x8000()
        {
            _mmu.WriteByte(0xFF40, 0x10); 
        }
        [Test]
        public void Test_TileDataSelect_0x8800()
        {
            _mmu.WriteByte(0xFF40, 0x00); 
        }
        [Test]
        public void Test_SpriteSize_8x8()
        {
            _mmu.WriteByte(0xFF40, 0x00); 
        }
        [Test]
        public void Test_SpriteSize_8x16()
        {
            _mmu.WriteByte(0xFF40, 0x04); 
        }
        [Test]
        public void Test_Sprite_Sprite_Priority()
        {
            _mmu.WriteByte(0xFF40, 0x82); 
            _mmu.WriteByte(0xFE00, 32); 
            _mmu.WriteByte(0xFE01, 16); 
            _mmu.WriteByte(0xFE02, 0);  
            _mmu.WriteByte(0xFE03, 0x00);  
            _mmu.WriteByte(0xFE04, 32); 
            _mmu.WriteByte(0xFE05, 16); 
            _mmu.WriteByte(0xFE06, 1);  
            _mmu.WriteByte(0xFE07, 0x00);  
            ushort tileAddressSprite1 = 0x8000;
            for (int i = 0; i < 16; i += 2)
            {
                _mmu.WriteByte((ushort)(tileAddressSprite1 + i), 0xFF);     
                _mmu.WriteByte((ushort)(tileAddressSprite1 + i + 1), 0x00); 
            }
            ushort tileAddressSprite2 = 0x8000 + 16;
            for (int i = 0; i < 16; i += 2)
            {
                _mmu.WriteByte((ushort)(tileAddressSprite2 + i), 0x00);
                _mmu.WriteByte((ushort)(tileAddressSprite2 + i + 1), 0xFF);
            }
            _mmu.WriteByte(0xFF48, 0xE4);
            _renderer.RenderScanline(16);
            byte[,] frameBuffer = _renderer.GetScreenBuffer();
            int xPosition = 8;
            byte expectedSpriteColor = 1; 
            Assert.AreEqual(expectedSpriteColor, frameBuffer[16, xPosition], "First sprite should be displayed over second sprite due to higher priority.");
        }
        [Test]
        public void Test_Sprite_Background_Priority_Behind()
        {
            _mmu.WriteByte(0xFF40, 0x93);
            _mmu.WriteByte(0xFF42, 0x00); 
            _mmu.WriteByte(0xFF43, 0x00); 
            _mmu.WriteByte(0xFF47, 0xE4); 
            ushort tileDataAddressBackground = 0x8000; 
            for (int i = 0; i < 16; i += 2)
            {
                _mmu.WriteByte((ushort)(tileDataAddressBackground + i), 0xFF);     
                _mmu.WriteByte((ushort)(tileDataAddressBackground + i + 1), 0x00); 
            }
            ushort tileDataAddressSprite = 0x8000 + 16; 
            for (int i = 0; i < 16; i += 2)
            {
                _mmu.WriteByte((ushort)(tileDataAddressSprite + i), 0x00);
                _mmu.WriteByte((ushort)(tileDataAddressSprite + i + 1), 0xFF);
            }
            ushort bgTileMapAddress = 0x9C00;
            _mmu.WriteByte((ushort)(bgTileMapAddress + 1 + 2 * 32), 0x00); 
            _mmu.WriteByte(0xFF48, 0xE4);
            _mmu.WriteByte(0xFE00, 32);   
            _mmu.WriteByte(0xFE01, 16);   
            _mmu.WriteByte(0xFE02, 1);    
            _mmu.WriteByte(0xFE03, 0x80); 
            _renderer.RenderScanline(16);
            byte[,] frameBuffer = _renderer.GetScreenBuffer();
            int xPosition = 8; 
            byte expectedBackgroundColor = 1; 
            byte expectedSpriteColor = 2;     
            Assert.AreEqual(expectedBackgroundColor, frameBuffer[16, xPosition], "Background pixel should be displayed over sprite pixel.");
        }
        [Test]
        public void Test_Sprite_Background_Priority()
        {
            _mmu.WriteByte(0xFF40, 0x93); 
            _mmu.WriteByte(0xFF42, 0x00); 
            _mmu.WriteByte(0xFF43, 0x00); 
            _mmu.WriteByte(0xFF47, 0xE4); 
            ushort tileDataAddress = 0x8000;
            for (int i = 0; i < 16; i += 2)
            {
                _mmu.WriteByte((ushort)(tileDataAddress + i), 0xFF); 
                _mmu.WriteByte((ushort)(tileDataAddress + i + 1), 0x00); 
            }
            ushort bgTileMapAddress = 0x9C00;
            _mmu.WriteByte((ushort)(bgTileMapAddress + 1 + 2 * 32), 0x00); 
            _mmu.WriteByte(0xFE00, 16); 
            _mmu.WriteByte(0xFE01, 8);  
            _mmu.WriteByte(0xFE02, 0);  
            _mmu.WriteByte(0xFE03, 0x00);  
            for (int i = 0; i < 16; i += 2)
            {
                _mmu.WriteByte((ushort)(tileDataAddress + i), 0x00); 
                _mmu.WriteByte((ushort)(tileDataAddress + i + 1), 0xFF); 
            }
            _mmu.WriteByte(0xFF48, 0xE4); 
            _renderer.RenderScanline(16); 
            byte[,] frameBuffer = _renderer.GetScreenBuffer();
            int xPosition = 8; 
            byte backgroundPixel = frameBuffer[16, xPosition];
            byte expectedSpriteColor = 2; 
            byte expectedBackgroundColor = 1; 
            Assert.AreEqual(expectedSpriteColor, frameBuffer[16, xPosition], "Sprite pixel should be displayed over background pixel.");
        }
    }
}
