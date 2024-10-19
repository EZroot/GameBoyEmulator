using GameBoyEmulator.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBoyEmulator.Graphics
{
    public class Renderer
    {
        private readonly MMU _mmu;
        private readonly Tile _tileRenderer;
        private readonly Sprite _spriteRenderer;
        private byte[,] _frameBuffer = new byte[PPU.ScreenHeight, PPU.ScreenWidth];

        public Renderer(MMU mmu)
        {
            _mmu = mmu;
            _tileRenderer = new Tile(mmu);
            _spriteRenderer = new Sprite(mmu);
        }

        public byte[,] GetScreenBuffer()
        {
            return _frameBuffer;
        }

        public void RenderScanline(int currentScanline)
        {
            if (currentScanline < 0 || currentScanline >= PPU.ScreenHeight)
                return;

            byte lcdc = _mmu.ReadByte(0xFF40);

            // Clear the scanline
            for (int x = 0; x < PPU.ScreenWidth; x++)
            {
                _frameBuffer[currentScanline, x] = 0; // Assuming 0 is the background color
            }

            if ((lcdc & 0x01) != 0) // BG Display Enable
            {
                RenderBackground(currentScanline);
            }

            if ((lcdc & 0x02) != 0) // OBJ (Sprite) Display Enable
            {
                _spriteRenderer.RenderSprites(currentScanline, _frameBuffer);
            }
        }

        public void RenderBackground(int currentScanline)
        {
            byte scx = _mmu.ReadByte(0xFF43);
            byte scy = _mmu.ReadByte(0xFF42);
            byte bgp = _mmu.ReadByte(0xFF47); // Background Palette Data

            for (int screenX = 0; screenX < PPU.ScreenWidth; screenX++)
            {
                int mapX = (screenX + scx) % 256;
                int mapY = (currentScanline + scy) % 256;
                int tileIndex = _tileRenderer.GetTileIndex(mapX, mapY);
                int tilePixelX = mapX % 8;
                int tilePixelY = mapY % 8;
                byte pixelValue = _tileRenderer.GetTilePixel(tileIndex, tilePixelX, tilePixelY);

                // Apply the palette
                byte colorNumber = (byte)(pixelValue & 0x03);
                byte palette = bgp;
                int color = (palette >> (colorNumber * 2)) & 0x03;
                _frameBuffer[currentScanline, screenX] = (byte)color;
            }
        }

    }
}
