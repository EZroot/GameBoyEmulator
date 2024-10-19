using GameBoyEmulator.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBoyEmulator.Graphics
{
    internal class Tile
    {
        private readonly MMU _mmu;

        public Tile(MMU mmu)
        {
            _mmu = mmu;
        }

        public int GetTileIndex(int mapX, int mapY)
        {
            byte lcdc = _mmu.ReadByte(0xFF40);
            ushort bgTileMapAddress = (lcdc & 0x08) != 0 ? (ushort)0x9C00 : (ushort)0x9800;
            int tileRow = (mapY / 8) % 32;
            int tileCol = (mapX / 8) % 32;
            int tileIndexAddress = bgTileMapAddress + tileRow * 32 + tileCol;
            return _mmu.ReadByte((ushort)tileIndexAddress);
        }

        public byte GetTilePixel(int tileIndex, int tilePixelX, int tilePixelY)
        {
            byte lcdc = _mmu.ReadByte(0xFF40);
            ushort tileAddress;
            if ((lcdc & 0x10) != 0) // BG & Window Tile Data Select
            {
                tileAddress = (ushort)(0x8000 + tileIndex * 16);
            }
            else
            {
                sbyte signedTileIndex = (sbyte)tileIndex;
                tileAddress = (ushort)(0x9000 + signedTileIndex * 16);
            }

            byte byte1 = _mmu.ReadByte((ushort)(tileAddress + tilePixelY * 2));
            byte byte2 = _mmu.ReadByte((ushort)(tileAddress + tilePixelY * 2 + 1));

            // Extract pixel bits
            byte lowBit = (byte)((byte1 >> (7 - tilePixelX)) & 1);
            byte highBit = (byte)((byte2 >> (7 - tilePixelX)) & 1);
            return (byte)((highBit << 1) | lowBit);
        }

    }
}
