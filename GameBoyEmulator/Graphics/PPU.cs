﻿using GameBoyEmulator.Interrupts;
using GameBoyEmulator.Memory;

namespace GameBoyEmulator.Graphics
{
    public class PPU
    {
        public const int ScreenWidth = 160;
        public const int ScreenHeight = 144;
        public const int CyclesPerScanline = 456;
        public const int TotalScanlines = 154;
        public const int VBlankScanline = 144;
        public int CurrentScanline { get; private set; } = 0;
        private int _cycles = 0;
        private readonly MMU _mmu;
        private readonly Renderer _renderer;
        private bool _lcdEnabledLastUpdate = false;

        public PPU(MMU mmu, Renderer renderer)
        {
            _mmu = mmu;
            _renderer = renderer;
        }
        // Inside the PPU class

        public void Update(int cpuCycles)
        {
            _cycles += cpuCycles;
            byte lcdc = _mmu.ReadByte(0xFF40);
            bool lcdEnabled = (lcdc & 0x80) != 0; // LCD Display Enable

            if (!lcdEnabled)
            {
                _cycles = 0;
                CurrentScanline = 0;
                SetLCDMode(0);
                _lcdEnabledLastUpdate = false;
                return;
            }
            else if (!_lcdEnabledLastUpdate && lcdEnabled)
            {
                // LCD just turned on
                _cycles = 0;
                CurrentScanline = 0;
                SetLCDMode(2);
            }

            _lcdEnabledLastUpdate = lcdEnabled;

            while (_cycles > 0) // Changed from _cycles >= 0 to _cycles > 0
            {
                int mode = _mmu.ReadByte(0xFF41) & 0x03;

                if (CurrentScanline < VBlankScanline)
                {
                    if (mode == 2)
                    {
                        if (_cycles >= 80)
                        {
                            _cycles -= 80;
                            SetLCDMode(3);
                        }
                        else
                        {
                            break;
                        }
                    }
                    else if (mode == 3)
                    {
                        if (_cycles >= 172)
                        {
                            _cycles -= 172;
                            _renderer.RenderScanline(CurrentScanline);
                            SetLCDMode(0);
                        }
                        else
                        {
                            break;
                        }
                    }
                    else if (mode == 0)
                    {
                        if (_cycles >= 204)
                        {
                            _cycles -= 204;
                            CurrentScanline++;
                            _mmu.WriteLY((byte)CurrentScanline);
                            CheckLYCMatch();
                            if (CurrentScanline == VBlankScanline)
                            {
                                SetLCDMode(1);
                                TriggerVBlank();
                            }
                            else
                            {
                                SetLCDMode(2);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    else if (mode == 1)
                    {
                        SetLCDMode(2);
                    }
                }
                else
                {
                    if (mode != 1)
                    {
                        SetLCDMode(1);
                    }
                    if (_cycles >= 456)
                    {
                        _cycles -= 456;
                        CurrentScanline++;
                        _mmu.WriteLY((byte)CurrentScanline);
                        CheckLYCMatch();
                        if (CurrentScanline >= TotalScanlines)
                        {
                            CurrentScanline = 0;
                            SetLCDMode(2);
                            _mmu.WriteLY((byte)CurrentScanline);
                            CheckLYCMatch();
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private void SetLCDMode(int mode)
        {
            byte stat = _mmu.ReadByte(0xFF41);
            stat = (byte)((stat & 0xFC) | (mode & 0x03));
            _mmu.WriteByte(0xFF41, stat);

            bool requestInterrupt = false;
            switch (mode)
            {
                case 0:
                    if ((stat & 0x08) != 0) requestInterrupt = true; // H-Blank Interrupt
                    break;
                case 1:
                    if ((stat & 0x10) != 0) requestInterrupt = true; // V-Blank Interrupt
                    break;
                case 2:
                    if ((stat & 0x20) != 0) requestInterrupt = true; // OAM Interrupt
                    break;
            }

            if (requestInterrupt)
            {
                // Set the LCD STAT interrupt flag in 0xFF0F
                byte interruptFlags = _mmu.ReadByte(0xFF0F);
                interruptFlags |= InterruptFlags.LCDSTAT;
                _mmu.WriteByte(0xFF0F, interruptFlags);
            }
        }

        public void CheckLYCMatch()
        {
            byte ly = _mmu.ReadByte(0xFF44);
            byte lyc = _mmu.ReadByte(0xFF45);
            byte stat = _mmu.ReadByte(0xFF41);

            if (ly == lyc)
            {
                stat |= 0x04; // Set coincidence flag
                _mmu.WriteByte(0xFF41, stat);

                if ((stat & 0x40) != 0) // LYC=LY Interrupt enabled
                {
                    // Set the LCD STAT interrupt flag
                    byte interruptFlags = _mmu.ReadByte(0xFF0F);
                    interruptFlags |= InterruptFlags.LCDSTAT;
                    _mmu.WriteByte(0xFF0F, interruptFlags);
                }
            }
            else
            {
                stat &= 0xFB; // Reset coincidence flag
                _mmu.WriteByte(0xFF41, stat);
            }
        }

        public void OverrideCurrentScanline(int scanLine)
        {
            CurrentScanline = scanLine;
        }

        private void TriggerVBlank()
        {
            // Set the V-Blank interrupt flag
            byte interruptFlags = _mmu.ReadByte(0xFF0F);
            interruptFlags |= InterruptFlags.VBlank;
            _mmu.WriteByte(0xFF0F, interruptFlags);
        }
    }
}
