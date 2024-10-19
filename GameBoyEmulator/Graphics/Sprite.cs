﻿using GameBoyEmulator.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBoyEmulator.Graphics
{
    internal class Sprite
    {
        private readonly MMU _mmu;

        public Sprite(MMU mmu)
        {
            _mmu = mmu;
        }

        public void RenderSprites(int currentScanline, byte[,] frameBuffer)
        {
            byte lcdc = _mmu.ReadByte(0xFF40);
            if ((lcdc & 0x02) == 0)
                return; // Sprites are disabled

            int spriteHeight = ((lcdc & 0x04) != 0) ? 16 : 8; // 8x16 or 8x8 sprites

            int spritesDrawn = 0;

            for (int i = 0; i < 40 && spritesDrawn < 10; i++) // Max 10 sprites per scanline
            {
                int spriteBaseAddress = 0xFE00 + i * 4;
                int spriteY = _mmu.ReadByte((ushort)(spriteBaseAddress)) - 16;
                int spriteX = _mmu.ReadByte((ushort)(spriteBaseAddress + 1)) - 8;
                byte tileIndex = _mmu.ReadByte((ushort)(spriteBaseAddress + 2));
                byte attributes = _mmu.ReadByte((ushort)(spriteBaseAddress + 3));

                if (currentScanline >= spriteY && currentScanline < spriteY + spriteHeight)
                {
                    RenderSpriteLine(spriteX, spriteY, tileIndex, attributes, currentScanline, frameBuffer, spriteHeight);
                    spritesDrawn++;
                }
            }
        }

        private void RenderSpriteLine(int spriteX, int spriteY, byte tileIndex, byte attributes, int currentScanline, byte[,] frameBuffer, int spriteHeight)
        {
            int tileRow = currentScanline - spriteY;
            if ((attributes & 0x40) != 0) // Flip vertically
            {
                tileRow = spriteHeight - 1 - tileRow;
            }

            // For 8x16 sprites, the tile index ignores bit 0
            if (spriteHeight == 16)
            {
                tileIndex &= 0xFE;
            }

            ushort tileAddress = (ushort)(0x8000 + tileIndex * 16);
            byte byte1 = _mmu.ReadByte((ushort)(tileAddress + tileRow * 2));
            byte byte2 = _mmu.ReadByte((ushort)(tileAddress + tileRow * 2 + 1));

            for (int x = 0; x < 8; x++)
            {
                int tilePixelX = x;
                if ((attributes & 0x20) != 0) // Flip horizontally
                {
                    tilePixelX = 7 - tilePixelX;
                }

                int screenX = spriteX + x;

                if (screenX < 0 || screenX >= PPU.ScreenWidth)
                    continue;

                byte lowBit = (byte)((byte1 >> (7 - tilePixelX)) & 1);
                byte highBit = (byte)((byte2 >> (7 - tilePixelX)) & 1);
                byte pixelValue = (byte)((highBit << 1) | lowBit);

                if (pixelValue == 0) // Transparent pixel
                    continue;

                // Handle sprite priority
                if ((attributes & 0x80) != 0 && frameBuffer[currentScanline, screenX] != 0)
                    continue; // Sprite is behind background pixel that is not color 0

                // Apply sprite palette
                byte obp = (attributes & 0x10) != 0 ? _mmu.ReadByte(0xFF49) : _mmu.ReadByte(0xFF48);
                byte colorNumber = pixelValue;
                int color = (obp >> (colorNumber * 2)) & 0x03;

                frameBuffer[currentScanline, screenX] = (byte)color;
            }
        }
    }
}
