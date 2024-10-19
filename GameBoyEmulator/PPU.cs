namespace GameBoyEmulator
{
    public class PPU
    {
        public const int CyclesPerScanline = 456;
        public const int TotalScanlines = 154;
        public const int VBlankScanline = 144;
        public int CurrentScanline { get; private set; } = 0;
        private int _cycles = 0;
        private bool DebugMode = false;
        private readonly Memory _memory;
        public const int ScreenWidth = 160;
        public const int ScreenHeight = 144;
        private byte[,] _frameBuffer = new byte[ScreenHeight, ScreenWidth];
        public PPU(Memory memory)
        {
            _memory = memory;
        }
        public byte[,] GetScreenBuffer()
        {
            return _frameBuffer;
        }
        public void RenderScanline(int currentScanline)
        {
            if (currentScanline < 0 || currentScanline >= ScreenHeight)
                return;
            byte scx = _memory.ReadByte(0xFF43);
            byte scy = _memory.ReadByte(0xFF42);
            int mapY = (currentScanline + scy) & 0xFF;
            int tileRow = mapY / 8;
            int tilePixelY = mapY % 8;
            ushort bgTileMapAddress = (_memory.ReadByte(0xFF40) & 0x08) != 0 ? (ushort)0x9C00 : (ushort)0x9800;
            ushort tileDataStart = (_memory.ReadByte(0xFF40) & 0x10) != 0 ? (ushort)0x8000 : (ushort)0x8800;
            for (int screenX = 0; screenX < ScreenWidth; screenX++)
            {
                int mapX = (screenX + scx) & 0xFF;
                int tileCol = mapX / 8;
                int tilePixelX = mapX % 8;
                int tileIndexAddress = bgTileMapAddress + tileRow * 32 + tileCol;
                byte tileIndex = _memory.ReadByte((ushort)tileIndexAddress);
                ushort tileAddress;
                if (tileDataStart == 0x8800)
                {
                    sbyte signedTileIndex = (sbyte)tileIndex;
                    tileAddress = (ushort)(tileDataStart + (signedTileIndex + 128) * 16);
                }
                else
                {
                    tileAddress = (ushort)(tileDataStart + tileIndex * 16);
                }
                byte byte1 = _memory.ReadByte((ushort)(tileAddress + tilePixelY * 2));
                byte byte2 = _memory.ReadByte((ushort)(tileAddress + tilePixelY * 2 + 1));
                byte lowBit = (byte)((byte1 >> (7 - tilePixelX)) & 1);
                byte highBit = (byte)((byte2 >> (7 - tilePixelX)) & 1);
                byte pixelValue = (byte)((highBit << 1) | lowBit);
                _frameBuffer[currentScanline, screenX] = pixelValue;
            }
        }
        private byte RenderPixel(int screenY, int screenX)
        {
            byte scx = _memory.ReadByte(0xFF43);
            byte scy = _memory.ReadByte(0xFF42);
            int mapY = (screenY + scy) % 256;
            int mapX = (screenX + scx) % 256;
            int tileRow = mapY / 8;
            int tileCol = mapX / 8;
            int tilePixelY = mapY % 8;
            int tilePixelX = mapX % 8;
            if (tileRow < 0 || tileRow >= 32 || tileCol < 0 || tileCol >= 32)
                return 0;
            ushort bgTileMapAddress = 0x9800;
            int tileIndexAddress = bgTileMapAddress + tileRow * 32 + tileCol;
            byte tileIndex = _memory.ReadByte((ushort)tileIndexAddress);
            byte[,] tileData = GetTileData(tileIndex);
            if (tileData.GetLength(0) != 8 || tileData.GetLength(1) != 8)
                return 0;
            return tileData[tilePixelY, tilePixelX];
        }
        public void Update(int cpuCycles)
        {
            _cycles += cpuCycles;
            while (_cycles >= CyclesPerScanline)
            {
                _cycles -= CyclesPerScanline;
                AdvanceScanline();
            }
        }
        public void AdvanceScanline()
        {
            RenderScanline(CurrentScanline);
            RenderSprites(CurrentScanline);
            CurrentScanline++;
            _memory.WriteLY((byte)CurrentScanline);
            CheckLYCMatch();
            if (CurrentScanline == VBlankScanline)
            {
                SetLCDMode(1);
                TriggerVBlank();
            }
            if (CurrentScanline >= TotalScanlines)
            {
                CurrentScanline = 0;
                SetLCDMode(2);
            }
            else if (CurrentScanline < VBlankScanline)
            {
                SetLCDMode(2);
            }
        }
        private void SetLCDMode(int mode)
        {
            byte stat = _memory.ReadByte(0xFF41);
            stat &= 0xFC;
            stat |= (byte)(mode & 0x03);
            _memory.WriteByte(0xFF41, stat);
            byte ifFlag = _memory.ReadByte(0xFF0F);
            if ((stat & (1 << (3 + mode))) != 0)
            {
                ifFlag |= 0x02;
                _memory.WriteByte(0xFF0F, ifFlag);
            }
        }
        private void CheckLYCMatch()
        {
            byte ly = _memory.ReadByte(0xFF44);
            byte lyc = _memory.ReadByte(0xFF45);
            if (ly == lyc)
            {
                byte stat = _memory.ReadByte(0xFF41);
                stat |= 0x04;
                _memory.WriteByte(0xFF41, stat);
                byte ie = _memory.ReadByte(0xFFFF);
                byte ifFlag = _memory.ReadByte(0xFF0F);
                if ((ie & 0x02) != 0)
                {
                    ifFlag |= 0x02;
                    _memory.WriteByte(0xFF0F, ifFlag);
                }
            }
            else
            {
                byte stat = _memory.ReadByte(0xFF41);
                stat &= 0xFB;
                _memory.WriteByte(0xFF41, stat);
            }
        }
        private byte[,] GetTileData(int tileIndex)
        {
            ushort tileDataStart = 0x8000;
            ushort tileAddress = (ushort)(tileDataStart + tileIndex * 16);
            byte[,] tilePixels = new byte[8, 8];
            for (int y = 0; y < 8; y++)
            {
                byte byte1 = _memory.ReadByte((ushort)(tileAddress + y * 2));
                byte byte2 = _memory.ReadByte((ushort)(tileAddress + y * 2 + 1));
                for (int x = 0; x < 8; x++)
                {
                    byte lowBit = (byte)((byte1 >> (7 - x)) & 1);
                    byte highBit = (byte)((byte2 >> (7 - x)) & 1);
                    tilePixels[y, x] = (byte)((highBit << 1) | lowBit);
                }
            }
            return tilePixels;
        }
        public void RenderBackground()
        {
            ushort bgTileMapAddress = 0x9800;
            byte scx = _memory.ReadByte(0xFF43);
            byte scy = _memory.ReadByte(0xFF42);
            for (int screenY = 0; screenY < ScreenHeight; screenY++)
            {
                int mapY = (screenY + scy) % 256;
                int tileRow = mapY / 8;
                int tilePixelY = mapY % 8;
                for (int screenX = 0; screenX < ScreenWidth; screenX++)
                {
                    int mapX = (screenX + scx) % 256;
                    int tileCol = mapX / 8;
                    int tilePixelX = mapX % 8;
                    int tileIndexAddress = bgTileMapAddress + tileRow * 32 + tileCol;
                    byte tileIndex = _memory.ReadByte((ushort)tileIndexAddress);
                    byte[,] tileData = GetTileData(tileIndex);
                    byte pixelValue = tileData[tilePixelY, tilePixelX];
                    _frameBuffer[screenY, screenX] = pixelValue;
                }
            }
        }
        public void RenderSprites(int currentScanline)
        {
            for (int i = 0; i < 40; i++)
            {
                int spriteBaseAddress = 0xFE00 + i * 4;
                int spriteY = _memory.ReadByte((ushort)(spriteBaseAddress)) - 16;
                int spriteX = _memory.ReadByte((ushort)(spriteBaseAddress + 1)) - 8;
                byte tileIndex = _memory.ReadByte((ushort)(spriteBaseAddress + 2));
                byte attributes = _memory.ReadByte((ushort)(spriteBaseAddress + 3));
                if (spriteX < -7 || spriteX >= ScreenWidth || spriteY < -7 || spriteY >= ScreenHeight)
                {
                    return;
                }
                if (currentScanline >= spriteY && currentScanline < spriteY + 8)
                {
                    int tileRow = currentScanline - spriteY;
                    if ((attributes & 0x40) != 0)
                    {
                        tileRow = 7 - tileRow;
                    }
                    ushort tileAddress = (ushort)(0x8000 + tileIndex * 16);
                    byte byte1 = _memory.ReadByte((ushort)(tileAddress + tileRow * 2));
                    byte byte2 = _memory.ReadByte((ushort)(tileAddress + tileRow * 2 + 1));
                    for (int tilePixelX = 0; tilePixelX < 8; tilePixelX++)
                    {
                        int screenX = spriteX + tilePixelX;
                        int pixelBit = (attributes & 0x20) != 0 ? tilePixelX : 7 - tilePixelX;
                        byte lowBit = (byte)((byte1 >> pixelBit) & 1);
                        byte highBit = (byte)((byte2 >> pixelBit) & 1);
                        byte pixelValue = (byte)((highBit << 1) | lowBit);
                        if (pixelValue == 0)
                        {
                            continue;
                        }
                        bool bgPriority = (attributes & 0x80) != 0;
                        if (bgPriority && _frameBuffer[currentScanline, screenX] != 0)
                        {
                            continue;
                        }
                        _frameBuffer[currentScanline, screenX] = pixelValue;
                    }
                }
            }
        }
        private void TriggerVBlank()
        {
            byte stat = _memory.ReadByte(0xFF41);
            stat |= 0x01;
            _memory.WriteByte(0xFF41, stat);
            byte ifFlag = _memory.ReadByte(0xFF0F);
            ifFlag |= 0x01;
            _memory.WriteByte(0xFF0F, ifFlag);
            if (DebugMode) Console.WriteLine("V-Blank interrupt triggered.");
        }
    }
}