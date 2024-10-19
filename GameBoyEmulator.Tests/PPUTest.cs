namespace GameBoyEmulator.Tests
{
    [TestFixture]
    public class PPUTests
    {
        private PPU _ppu;
        private Memory _memory;

        [SetUp]
        public void Setup()
        {
            _memory = new Memory();
            _ppu = new PPU(_memory);
        }

        [Test]
        public void Test_InitialScanline_ShouldBeZero()
        {

            Assert.AreEqual(0, _ppu.CurrentScanline);
        }

        [Test]
        public void Test_RenderScanline_ShouldRenderCorrectly()
        {

            _memory.WriteByte(0xFF43, 0x00); 
            _memory.WriteByte(0xFF42, 0x00); 
            _memory.WriteByte(0xFF40, 0x91); 

            for (int i = 0; i < 16; i++)
            {
                _memory.WriteByte((ushort)(0x8000 + i), (byte)(i % 2 == 0 ? 0xFF : 0x00));
            }

            _ppu.RenderScanline(0);

            byte[,] screenBuffer = _ppu.GetScreenBuffer();
            Assert.AreEqual(0x03, screenBuffer[0, 0]); 
        }

        [Test]
        public void Test_RenderScanline_ShouldHandleOutOfBoundsScanlines()
        {

            _ppu.RenderScanline(-1); 
            _ppu.RenderScanline(PPU.ScreenHeight + 1); 

            byte[,] screenBuffer = _ppu.GetScreenBuffer();
            for (int x = 0; x < PPU.ScreenWidth; x++)
            {
                Assert.AreEqual(0x00, screenBuffer[0, x]); 
            }
        }

        [Test]
        public void Test_Update_ShouldAdvanceScanlineAndTriggerVBlank()
        {

            int cyclesToVBlank = PPU.CyclesPerScanline * PPU.VBlankScanline;

            _ppu.Update(cyclesToVBlank);

            Assert.AreEqual(PPU.VBlankScanline, _ppu.CurrentScanline); 
            Assert.AreEqual(1, _memory.ReadByte(0xFF41) & 0x01); 
        }

        [Test]
        public void Test_Update_ShouldWrapScanlinesAfterTotalScanlines()
        {

            int cyclesToEnd = PPU.CyclesPerScanline * PPU.TotalScanlines;

            _ppu.Update(cyclesToEnd);

            Assert.AreEqual(0, _ppu.CurrentScanline); 
        }

        [Test]
        public void Test_SetLCDMode_ShouldChangeModeCorrectly()
        {

            _ppu.Update(PPU.CyclesPerScanline);

            byte stat = _memory.ReadByte(0xFF41);
            Assert.AreEqual(2, stat & 0x03); 
        }

        [Test]
        public void Test_CheckLYCMatch_ShouldSetFlagWhenLYMatchesLYC()
        {

            _memory.WriteByte(0xFF44, 0x10); 
            _memory.WriteByte(0xFF45, 0x10); 

            _ppu.Update(PPU.CyclesPerScanline);

            byte stat = _memory.ReadByte(0xFF41);
            Assert.IsTrue((stat & 0x04) != 0); 
        }

        [Test]
        public void Test_CheckLYCMatch_ShouldClearFlagWhenLYDoesNotMatchLYC()
        {

            _memory.WriteByte(0xFF44, 0x10); 
            _memory.WriteByte(0xFF45, 0x20); 

            _ppu.Update(PPU.CyclesPerScanline);

            byte stat = _memory.ReadByte(0xFF41);
            Assert.IsFalse((stat & 0x04) != 0); 
        }

        [Test]
        public void Test_RenderSprites_ShouldRenderSpriteCorrectly()
        {

            _memory.WriteByte(0xFE00, 10); 
            _memory.WriteByte(0xFE01, 5);  
            _memory.WriteByte(0xFE02, 0);  
            _memory.WriteByte(0xFE03, 0);  

            for (int i = 0; i < 16; i++)
            {
                _memory.WriteByte((ushort)(0x8000 + i), (byte)(i % 2 == 0 ? 0xFF : 0x00));
            }

            _ppu.RenderSprites(10); 

            byte[,] screenBuffer = _ppu.GetScreenBuffer();
            Assert.AreEqual(0x03, screenBuffer[10, 5]); 
        }

        [Test]
        public void Test_RenderSprites_ShouldHandleSpritePriority()
        {

            _memory.WriteByte(0xFE00, 10); 
            _memory.WriteByte(0xFE01, 5);  
            _memory.WriteByte(0xFE02, 0);  
            _memory.WriteByte(0xFE03, 0x80);  

            for (int x = 0; x < PPU.ScreenWidth; x++)
            {
                _memory.WriteByte((ushort)(0x9800 + x), 0x01); 
            }

            _ppu.RenderSprites(10); 

            byte[,] screenBuffer = _ppu.GetScreenBuffer();
            Assert.AreEqual(0x01, screenBuffer[10, 5]); 
        }

        [Test]
        public void Test_RenderBackground_ShouldFillFrameBuffer()
        {

            _memory.WriteByte(0xFF43, 0x00); 
            _memory.WriteByte(0xFF42, 0x00); 
            _memory.WriteByte(0xFF40, 0x91); 

            for (int i = 0; i < 16; i++)
            {
                _memory.WriteByte((ushort)(0x8000 + i), (byte)(i % 2 == 0 ? 0xFF : 0x00));
            }

            _ppu.RenderBackground();

            byte[,] screenBuffer = _ppu.GetScreenBuffer();
            Assert.AreEqual(0x03, screenBuffer[0, 0]); 
        }

        [Test]
        public void Test_AdvanceScanline_ShouldTriggerVBlankAt144()
        {

            _ppu.Update(PPU.CyclesPerScanline * PPU.VBlankScanline); 

            _ppu.AdvanceScanline();

            Assert.AreEqual(1, _memory.ReadByte(0xFF0F) & 0x01); 
        }
    }

}
