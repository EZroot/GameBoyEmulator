using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace GameBoyEmulator
{
    internal class Tests
    {
        private CPU _cpu;
        private Memory _memory;
        private PPU _ppu;
        private Timer _timer; 
        private const int CyclesPerFrame = 70224; 
        private bool DebugFrame = false;
        private bool DebugStep = false;
        private const int ScreenWidth = 160;
        private const int ScreenHeight = 144;
    }
}
