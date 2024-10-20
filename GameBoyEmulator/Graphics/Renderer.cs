using GameBoyEmulator.Memory;
using GameBoyEmulator.Debug;
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
        private bool[,] _spriteBuffer = new bool[PPU.ScreenHeight, PPU.ScreenWidth];
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
            for (int x = 0; x < PPU.ScreenWidth; x++)
            {
                _frameBuffer[currentScanline, x] = 0;
                _spriteBuffer[currentScanline, x] = false;
            }
            if ((lcdc & 0x01) != 0) 
            {
                RenderBackground(currentScanline);
            }
            if ((lcdc & 0x20) != 0) 
            {
                RenderWindow(currentScanline);
            }
            if ((lcdc & 0x02) != 0) 
            {
                _spriteRenderer.RenderSprites(currentScanline, _frameBuffer, _spriteBuffer);
            }
        }
        public void RenderBackground(int currentScanline)
        {
            byte scx = _mmu.ReadByte(0xFF43);
            byte scy = _mmu.ReadByte(0xFF42);
            byte bgp = _mmu.ReadByte(0xFF47);
            for (int screenX = 0; screenX < PPU.ScreenWidth; screenX++)
            {
                int mapX = (screenX + scx) % 256;
                int mapY = (currentScanline + scy) % 256;
                int tileIndex = _tileRenderer.GetTileIndex(mapX, mapY);
                int tilePixelX = mapX % 8;
                int tilePixelY = mapY % 8;
                byte pixelValue = _tileRenderer.GetTilePixel(tileIndex, tilePixelX, tilePixelY);
                byte colorNumber = (byte)(pixelValue & 0x03);
                byte palette = bgp;
                int color = (palette >> (colorNumber * 2)) & 0x03;
                _frameBuffer[currentScanline, screenX] = (byte)color;
            }
        }
        public void RenderWindow(int currentScanline)
        {
            byte wy = _mmu.ReadByte(0xFF4A);
            byte wx = _mmu.ReadByte(0xFF4B);
            if (Debugger.IsDebugEnabled && Debugger.dDebugPPU)
            {
                Logger.Log($"Rendering Window: WY={wy}, WX={wx}, CurrentScanline={currentScanline}");
            }
            if (currentScanline < (wy - 7))
                return;
            int windowY = currentScanline - (wy - 7);
            byte lcdc = _mmu.ReadByte(0xFF40);
            ushort windowTileMapAddress = (lcdc & 0x40) != 0 ? (ushort)0x9C00 : (ushort)0x9800;
            for (int screenX = 0; screenX < PPU.ScreenWidth; screenX++)
            {
                int windowX = screenX - (wx - 7);
                if (windowX < 0)
                    continue; 
                int tileX = windowX / 8;
                int tileY = windowY / 8;
                int tileIndexAddress = windowTileMapAddress + (tileY * 32) + tileX;
                byte tileIndex = _mmu.ReadByte((ushort)tileIndexAddress);
                int tilePixelX = windowX % 8;
                int tilePixelY = windowY % 8;
                byte pixelValue = _tileRenderer.GetTilePixel(tileIndex, tilePixelX, tilePixelY);
                byte colorNumber = (byte)(pixelValue & 0x03);
                byte palette = _mmu.ReadByte(0xFF47); 
                int color = (palette >> (colorNumber * 2)) & 0x03;
                _frameBuffer[currentScanline, screenX] = (byte)color;
                if (Debugger.IsDebugEnabled && Debugger.dDebugPPU)
                {
                    Logger.Log($"Window Pixel - Scanline: {currentScanline}, X: {screenX}, Tile: {tileIndex}, Pixel: {pixelValue}, Color: {color}");
                }
            }
        }
    }
}
