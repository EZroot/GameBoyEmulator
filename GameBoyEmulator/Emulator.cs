using System.Drawing;
using System.Text;

namespace GameBoyEmulator
{
    public class Emulator
    {
        private CPU _cpu;
        private Memory _memory;
        private PPU _ppu;
        private Timer _timer;
        private const int CyclesPerFrame = 70224;
        private bool DebugFrameStepThroughPerFrame = false;
        private bool DebugFrameStepThroughPerCycle = false;
        private bool GoFast = false; //Skips frames when renderering to speed up cpu cycles
        private const int ScreenWidth = 160;
        private const int ScreenHeight = 144;
        public Emulator()
        {
            _memory = new Memory();
            _cpu = new CPU(_memory);
            _ppu = new PPU(_memory);
            _timer = new Timer(_memory);
            InitializeHardwareRegisters();
        }
        public void Run()
        {
            Console.WriteLine("Emulator started. Loading ROM...");
            Console.WriteLine("Starting execution...");
            if (GoFast) Console.WriteLine("Fast mode activated!");
            Dictionary<ConsoleKey, (int buttonIndex, bool isPressed, int frameCounter)> keyMappings = new Dictionary<ConsoleKey, (int, bool, int)>
    {
        { ConsoleKey.Z, (0, false, 0) },
        { ConsoleKey.X, (1, false, 0) },
        { ConsoleKey.LeftArrow, (2, false, 0) },
        { ConsoleKey.RightArrow, (3, false, 0) },
        { ConsoleKey.UpArrow, (4, false, 0) },
        { ConsoleKey.DownArrow, (5, false, 0) },
        { ConsoleKey.A, (6, false, 0) },
        { ConsoleKey.Spacebar, (7, false, 0) }
    };
            Task.Run(() => DetectKeyPressesAsync(keyMappings));

            var skipFrame = 60;
            while (true)
            {
                ExecuteFrame();
                if (DebugFrameStepThroughPerFrame) Console.ReadKey();
                if (GoFast)
                {
                    skipFrame--;
                    if (skipFrame <= 0)
                    {
                        if (!DebugFrameStepThroughPerFrame) RenderScreen();
                        skipFrame = 1000;
                    }
                }
                else
                {
                    if (!DebugFrameStepThroughPerFrame) RenderScreen();
                }
                if (GoFast) continue;

                foreach (var key in keyMappings.Keys.ToList())
                {
                    if (keyMappings[key].isPressed)
                    {
                        _memory.SetJoypadButtonState(keyMappings[key].buttonIndex, true);
                        if (keyMappings[key].frameCounter > 0)
                        {
                            keyMappings[key] = (keyMappings[key].buttonIndex, true, keyMappings[key].frameCounter - 1);
                        }
                        else
                        {
                            keyMappings[key] = (keyMappings[key].buttonIndex, false, 0);
                            _memory.SetJoypadButtonState(keyMappings[key].buttonIndex, false);
                        }
                    }
                    else
                    {
                        _memory.SetJoypadButtonState(keyMappings[key].buttonIndex, false);
                    }
                }
                Thread.Sleep(16);
            }
        }
        private async Task DetectKeyPressesAsync(Dictionary<ConsoleKey, (int buttonIndex, bool isPressed, int frameCounter)> keyMappings)
        {
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(intercept: true).Key;
                    if (keyMappings.ContainsKey(key))
                    {
                        keyMappings[key] = (keyMappings[key].buttonIndex, true, 10);
                    }
                }
                await Task.Delay(1);
            }
        }
        public void RenderScreen()
        {
            byte[,] screenBuffer = _ppu.GetScreenBuffer();
            StringBuilder outputBuilder = new StringBuilder(ScreenHeight * (ScreenWidth + 1));
            for (int y = 0; y < ScreenHeight; y++)
            {
                for (int x = 0; x < ScreenWidth; x++)
                {
                    byte pixel = screenBuffer[y, x];
                    char displayChar = GetCharForPixel(pixel);
                    outputBuilder.Append(displayChar);
                }
                outputBuilder.AppendLine();
            }
            Console.Clear();
            Console.Write(outputBuilder.ToString());
        }
        private char GetCharForPixel(byte pixelValue)
        {
            char[] gradient = { ' ', '░', '▒', '▓', '█' };
            return gradient[pixelValue];
        }

        //    public void RenderScreenAsImage(string filename = "output.png")
        //{
        //    byte[,] screenBuffer = _ppu.GetScreenBuffer();
        //    using (Bitmap bitmap = new Bitmap(ScreenWidth, ScreenHeight))
        //    {
        //        for (int y = 0; y < ScreenHeight; y++)
        //        {
        //            for (int x = 0; x < ScreenWidth; x++)
        //            {
        //                byte pixel = screenBuffer[y, x];
        //                Color color = GetColorForPixel(pixel);  
        //                bitmap.SetPixel(x, y, color);
        //            }
        //        }
        //        bitmap.Save(filename);
        //        Console.WriteLine($"Saved Render.");
        //    }
        //}

        private Color GetColorForPixel(byte pixelValue)
        {
            return pixelValue switch
            {
                0 => Color.White,
                1 => Color.LightGray,
                2 => Color.Gray,
                3 => Color.Black,
                _ => Color.White,
            };
        }
        private void ExecuteFrame()
        {
            int cycles = 0;
            int debugSteps = 10;
            while (cycles < CyclesPerFrame)
            {
                int stepCycles = _cpu.Step();
                _ppu.Update(stepCycles);
                _timer.Update(stepCycles);
                cycles += stepCycles;
                if (DebugFrameStepThroughPerCycle)
                {
                    debugSteps--;
                    if (debugSteps == 0)
                    {
                        Console.ReadKey();
                        debugSteps = 10;
                    }
                }
            }
        }
        private void InitializeHardwareRegisters()
        {
            _memory.WriteByte(0xFF40, 0x91);
            _memory.WriteByte(0xFF41, 0x85);
            _memory.WriteByte(0xFF42, 0x00);
            _memory.WriteByte(0xFF43, 0x00);
            _memory.WriteByte(0xFF44, 0x00);
            _memory.WriteByte(0xFF45, 0x00);
            _memory.WriteByte(0xFF4A, 0x00);
            _memory.WriteByte(0xFF4B, 0x00);
            _memory.WriteByte(0xFF46, 0xFF);
            _memory.WriteByte(0xFFFF, 0x00);
            _memory.WriteByte(0xFF0F, 0xE1);
            _memory.WriteByte(0xFF04, 0x00);
            _memory.WriteByte(0xFF05, 0x00);
            _memory.WriteByte(0xFF06, 0x00);
            _memory.WriteByte(0xFF07, 0x00);
            _memory.WriteByte(0xFF26, 0xF1);
        }
        public void LoadROM(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"ROM file not found: {filePath}");
                Environment.Exit(1);
            }
            byte[] romData = File.ReadAllBytes(filePath);
            if (!VerifyNintendoLogo(romData))
            {
                Console.WriteLine("Invalid ROM: Nintendo logo verification failed.");
                Environment.Exit(1);
            }
            int romSizeCode = romData[0x148];
            int romSize = GetROMSize(romSizeCode, romData);
            Console.WriteLine($"ROM loaded: {romData.Length} bytes into memory.");
            Console.WriteLine($"ROM size from header: {romSize / 1024} KB");
            Thread.Sleep(5000);
            _memory.LoadROM(romData);
        }
        private int GetROMSize(int romSizeCode, byte[] romData)
        {
            switch (romSizeCode)
            {
                case 0x00: return 32 * 1024;
                case 0x01: return 64 * 1024;
                case 0x02: return 128 * 1024;
                case 0x03: return 256 * 1024;
                case 0x04: return 512 * 1024;
                case 0x05: return 1024 * 1024;
                case 0x06: return 2 * 1024 * 1024;
                case 0x07: return 4 * 1024 * 1024;
                case 0x08: return 8 * 1024 * 1024;
                default:
                    Console.WriteLine("Unknown ROM size code.");
                    return romData.Length;
            }
        }
        public void Test_JR_NZ()
        {
            Console.WriteLine("\n--- JR NZ Test ---");
            _cpu.PC = 0x100;
            _cpu.SetZeroFlag(false);
            _memory.WriteByte(0x100, 0x20);
            _memory.WriteByte(0x101, 0x05);
            Console.WriteLine($"Memory[0x101] = {_memory.ReadByte(0x101)}");
            _cpu.Step();
            Console.WriteLine($"PC after JR NZ (Zero Flag not set): 0x{_cpu.PC:X4}");
            if (_cpu.PC == 0x107)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Test Passed: JR NZ jumped when ZeroFlag is not set.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Test Failed: Expected PC=0x107, got PC=0x{_cpu.PC:X4}");
            }
            Console.ResetColor();
            _cpu.PC = 0x200;
            _cpu.SetZeroFlag(true);
            _memory.WriteByte(0x200, 0x20);
            _memory.WriteByte(0x201, 0x05);
            _cpu.Step();
            Console.WriteLine($"PC after JR NZ (Zero Flag set): 0x{_cpu.PC:X4}");
            if (_cpu.PC == 0x202)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Test Passed: JR NZ did not jump when ZeroFlag is set.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Test Failed: Expected PC=0x202, got PC=0x{_cpu.PC:X4}");
            }
            Console.ResetColor();
        }
        public void Test_JP_Z()
        {
            Console.WriteLine("\n--- JP Z Test ---");
            _cpu.PC = 0x100;
            _cpu.SetZeroFlag(true);
            _memory.WriteByte(0x100, 0xCA);
            _memory.WriteWord(0x101, 0x200);
            _cpu.Step();
            if (_cpu.PC == 0x200)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Test Passed: JP Z jumped when ZeroFlag is set.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Test Failed: Expected PC=0x200, got PC=0x{_cpu.PC:X4}");
            }
            Console.ResetColor();
            _cpu.PC = 0x300;
            _cpu.SetZeroFlag(false);
            _memory.WriteByte(0x300, 0xCA);
            _memory.WriteWord(0x301, 0x400);
            _cpu.Step();
            if (_cpu.PC == 0x303)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Test Passed: JP Z did not jump when ZeroFlag is cleared.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Test Failed: Expected PC=0x303, got PC=0x{_cpu.PC:X4}");
            }
            Console.ResetColor();
        }
        public void Test_CP_d8()
        {
            Console.WriteLine("\n--- CP d8 Test ---");
            _cpu.A = 0x50;
            _memory.WriteByte(0x100, 0xFE);
            _memory.WriteByte(0x101, 0x50);
            _cpu.PC = 0x100;
            _cpu.Step();
            if (_cpu.GetZeroFlag() && _cpu.GetNegativeFlag() && !_cpu.GetCarryFlag() && !_cpu.GetHalfCarryFlag())
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Test Passed: CP d8 with A == d8, flags are set correctly.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Test Failed: Expected Zero Flag set and Carry Flag cleared.");
            }
            Console.ResetColor();
            _cpu.A = 0x30;
            _memory.WriteByte(0x200, 0xFE);
            _memory.WriteByte(0x201, 0x50);
            _cpu.PC = 0x200;
            _cpu.Step();
            if (!_cpu.GetZeroFlag() && _cpu.GetCarryFlag() && _cpu.GetNegativeFlag() && !_cpu.GetHalfCarryFlag())
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Test Passed: CP d8 with A < d8, Carry Flag set.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Test Failed: Expected Carry Flag set and Zero Flag cleared.");
            }
            Console.ResetColor();
        }
        public void Test_RET_and_RET_Z()
        {
            Console.WriteLine("\n--- RET and RET Z Test ---");
            Console.WriteLine("\nWARNING - THIS TEST NEEDS INTERUPTS DISABLED IN CPU STEP!");
            _cpu.SP = 0xFFFE;
            _memory.WriteWord(0xFFFE, 0x300);
            _cpu.PC = 0x100;
            _memory.WriteByte(0x100, 0xC9);
            _cpu.Step();
            if (_cpu.PC == 0x300)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Test Passed: RET returned to correct address.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Test Failed: Expected PC=0x300, got PC=0x{_cpu.PC:X4}");
            }
            Console.ResetColor();
            _cpu.SP = 0xFFFE;
            _memory.WriteWord(0xFFFE, 0x400);
            _cpu.PC = 0x200;
            Console.WriteLine("Setting Zero Flag to true before RET Z.");
            _cpu.SetZeroFlag(true);
            Console.WriteLine($"F register after setting ZeroFlag: 0x{_cpu.F:X2}");
            _memory.WriteByte(0x200, 0xC8);
            _cpu.Step();
            if (_cpu.PC == 0x400)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Test Passed: RET Z returned when ZeroFlag is set.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Test Failed: Expected PC=0x400, got PC=0x{_cpu.PC:X4}");
            }
            Console.ResetColor();
        }
        private bool VerifyNintendoLogo(byte[] romData)
        {
            const int logoStartAddress = 0x0104;
            byte[] expectedLogo = new byte[]
            {
                0xCE, 0xED, 0x66, 0x66, 0xCC, 0x0D, 0x00, 0x0B,
                0x03, 0x73, 0x00, 0x83, 0x00, 0x0C, 0x00, 0x0D,
                0x00, 0x08, 0x11, 0x1F, 0x88, 0x89, 0x00, 0x0E,
                0xDC, 0xCC, 0x6E, 0xE6, 0xDD, 0xDD, 0xD9, 0x99,
                0xBB, 0xBB, 0x67, 0x63, 0x6E, 0x0E, 0xEC, 0xCC,
                0xDD, 0xDC, 0x99, 0x9F, 0xBB, 0xB9, 0x33, 0x3E
            };
            for (int i = 0; i < expectedLogo.Length; i++)
            {
                if (romData[logoStartAddress + i] != expectedLogo[i])
                {
                    Console.WriteLine($"Logo mismatch at byte {i + logoStartAddress:X4}: ROM = 0x{romData[logoStartAddress + i]:X2}, Expected = 0x{expectedLogo[i]:X2}");
                    return false;
                }
            }
            Console.WriteLine("Nintendo logo verification passed.");
            return true;
        }
        private void LoadTestTileData()
        {
            ushort tileDataStart = 0x8000;
            byte[] testTile = new byte[]
            {
        0b11111111, 0b00000000,
        0b11111111, 0b00000000,
        0b00000000, 0b11111111,
        0b00000000, 0b11111111,
        0b11111111, 0b00000000,
        0b11111111, 0b00000000,
        0b00000000, 0b11111111,
        0b00000000, 0b11111111,
            };
            for (int i = 0; i < testTile.Length; i++)
            {
                _memory.WriteByte((ushort)(tileDataStart + i), testTile[i]);
            }
        }
    }
}
