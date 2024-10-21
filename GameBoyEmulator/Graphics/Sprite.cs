using GameBoyEmulator.Memory;
using GameBoyEmulator.Debug;
using System;

namespace GameBoyEmulator.Graphics
{
    internal class Sprite
    {
        private readonly MMU _mmu;
        public Sprite(MMU mmu)
        {
            _mmu = mmu;
        }
        public void RenderSprites(int currentScanline, byte[,] frameBuffer, bool[,] spriteBuffer)
        {
            byte lcdc = _mmu.ReadByte(0xFF40);
            if ((lcdc & 0x02) == 0)
                return; 
            int spriteHeight = ((lcdc & 0x04) != 0) ? 16 : 8; 
            int spritesDrawn = 0;
            for (int i = 0; i < 40 && spritesDrawn < 10; i++) 
            {
                int spriteBaseAddress = 0xFE00 + i * 4;
                int spriteY = _mmu.ReadByte((ushort)(spriteBaseAddress)) - 16;
                int spriteX = _mmu.ReadByte((ushort)(spriteBaseAddress + 1)) - 8;
                byte tileIndex = _mmu.ReadByte((ushort)(spriteBaseAddress + 2));
                byte attributes = _mmu.ReadByte((ushort)(spriteBaseAddress + 3));
                if (currentScanline >= spriteY && currentScanline < spriteY + spriteHeight)
                {
                    RenderSpriteLine(spriteX, spriteY, tileIndex, attributes, currentScanline, frameBuffer, spriteBuffer, spriteHeight);
                    spritesDrawn++;
                }
            }
        }
        private void RenderSpriteLine(int spriteX, int spriteY, byte tileIndex, byte attributes, int currentScanline, byte[,] frameBuffer, bool[,] spriteBuffer, int spriteHeight)
        {
            int tileRow = currentScanline - spriteY;
            if ((attributes & 0x40) != 0) 
            {
                tileRow = spriteHeight - 1 - tileRow;
            }
            if (spriteHeight == 16)
            {
                tileIndex &= 0xFE; 
                if (tileRow >= 8)
                {
                    tileIndex += 1;
                    tileRow -= 8;
                }
            }
            ushort tileAddress = (ushort)(0x8000 + tileIndex * 16 + tileRow * 2);
            byte byte1 = _mmu.ReadByte(tileAddress);
            byte byte2 = _mmu.ReadByte((ushort)(tileAddress + 1));
            for (int x = 0; x < 8; x++)
            {
                int tilePixelX = x;
                if ((attributes & 0x20) != 0) 
                {
                    tilePixelX = 7 - tilePixelX;
                }
                int screenX = spriteX + x;
                if (screenX < 0 || screenX >= PPU.ScreenWidth)
                    continue;
                byte lowBit = (byte)((byte1 >> (7 - tilePixelX)) & 1);
                byte highBit = (byte)((byte2 >> (7 - tilePixelX)) & 1);
                byte pixelValue = (byte)((highBit << 1) | lowBit);
                if (pixelValue == 0)
                    continue; 
                if (spriteBuffer[currentScanline, screenX])
                    continue; 
                if ((attributes & 0x80) != 0 && frameBuffer[currentScanline, screenX] != 0)
                    continue; 
                byte obp = (attributes & 0x10) != 0 ? _mmu.ReadByte(0xFF49) : _mmu.ReadByte(0xFF48);
                byte colorNumber = pixelValue;
                int color = (obp >> (colorNumber * 2)) & 0x03;
                frameBuffer[currentScanline, screenX] = (byte)color;
                spriteBuffer[currentScanline, screenX] = true;
                if (Debugger.IsDebugEnabled && Debugger.dDebugPPU)
                {
                    Console.WriteLine($"Rendering pixel at X={screenX}, Y={currentScanline}");
                    Console.WriteLine($"Sprite Pixel: {pixelValue}");
                    Console.WriteLine($"Sprite Attributes: {attributes:X2}");
                }
            }
        }
    }
}
