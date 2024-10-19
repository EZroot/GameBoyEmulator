using GameBoyEmulator.Processor;
using GameBoyEmulator.Memory;
using System.Drawing;
using System.Text;
using GameBoyEmulator.Interrupts;
using GameBoyEmulator.Graphics;

namespace GameBoyEmulator
{
    public class Emulator
    {
        private CPU _cpu;
        private Registers _registry;
        private Opcode _opcode;
        private MemoryMap _memoryMap;
        private RAM _ram;
        private MMU _mmu;
        private PPU _ppu;
        private Renderer _renderer;
        private InterruptController _interruptController;
        private Timer _timer;
        private const int CyclesPerFrame = 70224;
        private bool DebugFrameStepThroughPerFrame = false;
        private bool DebugFrameStepThroughPerCycle = false;
        private bool GoFast = true; //Skips frames when renderering to speed up cpu cycles
        private const int ScreenWidth = 160;
        private const int ScreenHeight = 144;
        public Emulator()
        {
            _registry = new Registers();
            _ram = new RAM();
            _timer = new Timer(_ram);
            _memoryMap = new MemoryMap(_ram);
            _mmu = new MMU(_registry, _ram);
            _renderer = new Renderer(_mmu);
            _ppu = new PPU(_mmu, _renderer);
            _opcode = new Opcode(_registry, _mmu, _ram);
            _interruptController = new InterruptController(_registry, _mmu, _ram);
            _cpu = new CPU(_registry, _opcode, _ram, _interruptController);
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
                        _mmu.SetJoypadButtonState(keyMappings[key].buttonIndex, true);
                        if (keyMappings[key].frameCounter > 0)
                        {
                            keyMappings[key] = (keyMappings[key].buttonIndex, true, keyMappings[key].frameCounter - 1);
                        }
                        else
                        {
                            keyMappings[key] = (keyMappings[key].buttonIndex, false, 0);
                            _mmu.SetJoypadButtonState(keyMappings[key].buttonIndex, false);
                        }
                    }
                    else
                    {
                        _mmu.SetJoypadButtonState(keyMappings[key].buttonIndex, false);
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
            byte[,] screenBuffer = _renderer.GetScreenBuffer();
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
            _mmu.WriteByte(0xFF40, 0x91);
            _mmu.WriteByte(0xFF41, 0x85);
            _mmu.WriteByte(0xFF42, 0x00);
            _mmu.WriteByte(0xFF43, 0x00);
            _mmu.WriteByte(0xFF44, 0x00);
            _mmu.WriteByte(0xFF45, 0x00);
            _mmu.WriteByte(0xFF4A, 0x00);
            _mmu.WriteByte(0xFF4B, 0x00);
            _mmu.WriteByte(0xFF46, 0xFF);
            _mmu.WriteByte(0xFFFF, 0x00);
            _mmu.WriteByte(0xFF0F, 0xE1);
            _mmu.WriteByte(0xFF04, 0x00);
            _mmu.WriteByte(0xFF05, 0x00);
            _mmu.WriteByte(0xFF06, 0x00);
            _mmu.WriteByte(0xFF07, 0x00);
            _mmu.WriteByte(0xFF26, 0xF1);
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
            _memoryMap.LoadROM(romData);
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
                _mmu.WriteByte((ushort)(tileDataStart + i), testTile[i]);
            }
        }
    }
}
