using System;
namespace GameBoyEmulator
{
    public class Memory
    {
        private const int MemorySize = 0x10000;
        private readonly byte[] _memory = new byte[MemorySize];
        private readonly bool[] _joypadButtons = new bool[8];
        public bool debugMode = false;
        public byte ReadByte(ushort address)
        {
            if (debugMode)
            {
                Console.WriteLine($"[DEBUG] ReadByte called at address 0x{address:X4}");
            }
            if (address == 0xFF00)
            {
                byte joypadState = GetJoypadState();
                if (debugMode)
                {
                    Console.WriteLine($"[DEBUG] Read from Joypad register (0xFF00): 0x{joypadState:X2}");
                }
                return joypadState;
            }
            byte value = _memory[address];
            if (debugMode)
            {
                Console.WriteLine($"[DEBUG] Read from memory address 0x{address:X4}: 0x{value:X2}");
            }
            return value;
        }
        public void WriteByte(ushort address, byte value)
        {
            if (debugMode)
            {
                Console.WriteLine($"[DEBUG] WriteByte called at address 0x{address:X4} with value 0x{value:X2}");
            }
            if (address == 0xFF44)
            {
                if (debugMode)
                {
                    Console.WriteLine("[DEBUG] Attempted write to LY (0xFF44). Ignored.");
                }
                return;
            }
            if (address >= 0x8000 && address <= 0x9FFF)
            {
                if (debugMode)
                {
                    Console.WriteLine($"[DEBUG] TILE DATA! VRAM Write: 0x{address:X4} <= 0x{value:X2}");
                }
            }
            _memory[address] = value;
            if (debugMode)
            {
                Console.WriteLine($"[DEBUG] Memory address 0x{address:X4} written with value 0x{value:X2}");
            }
            if (address == 0xFF46)
            {
                HandleDMA(value);
            }
        }
        public void WriteLY(byte value)
        {
            _memory[0xFF44] = value;
            if (debugMode)
            {
                Console.WriteLine($"[DEBUG] PPU wrote LY=0x{value:X2}");
            }
        }
        public void HandleDMA(byte value)
        {
            ushort sourceAddress = (ushort)(value << 8);
            for (int i = 0; i < 0xA0; i++)
            {
                _memory[0xFE00 + i] = ReadByte((ushort)(sourceAddress + i));
            }
            if (debugMode)
            {
                Console.WriteLine($"[DEBUG] DMA Transfer initiated from 0x{sourceAddress:X4}.");
            }
        }
        public ushort ReadWord(ushort address)
        {
            ushort value = (ushort)(_memory[address] | (_memory[address + 1] << 8));
            if (debugMode)
            {
                Console.WriteLine($"[DEBUG] ReadWord from memory address 0x{address:X4}: 0x{value:X4}");
            }
            return value;
        }
        public void WriteWord(ushort address, ushort value)
        {
            if (address == 0xFF44 || address == 0xFF45)
            {
                if (debugMode)
                {
                    Console.WriteLine($"[DEBUG] Attempted write to read-only register at 0x{address:X4}. Ignored.");
                }
                return;
            }
            _memory[address] = (byte)(value & 0xFF);
            _memory[address + 1] = (byte)(value >> 8);
            if (debugMode)
            {
                Console.WriteLine($"[DEBUG] WriteWord to memory address 0x{address:X4}: 0x{value:X4}");
            }
        }
        public void SetJoypadButtonState(int buttonIndex, bool pressed)
        {
            ValidateButtonIndex(buttonIndex);
            _joypadButtons[buttonIndex] = pressed;
            if (debugMode)
            {
                Console.WriteLine($"[DEBUG] Joypad button {(pressed ? "pressed" : "released")}: {buttonIndex}");
            }
        }
        public void LoadROM(byte[] romData)
        {
            if (romData.Length > 0x8000)
            {
                throw new InvalidOperationException("ROM too large. Basic ROM loading only supports up to 32KB ROMs.");
            }
            Array.Copy(romData, 0, _memory, 0x0000, romData.Length);
            if (debugMode)
            {
                Console.WriteLine($"[DEBUG] ROM loaded: {romData.Length} bytes into memory at 0x0000.");
            }
        }
        private byte GetJoypadState()
        {
            byte selector = _memory[0xFF00];
            byte state = 0xFF;
            bool selectButtons = (selector & 0x20) == 0;
            bool selectDirections = (selector & 0x10) == 0;
            if (selectDirections)
            {
                state &= _joypadButtons[0] ? (byte)0xFE : (byte)0xFF;
                state &= _joypadButtons[1] ? (byte)0xFD : (byte)0xFF;
                state &= _joypadButtons[2] ? (byte)0xFB : (byte)0xFF;
                state &= _joypadButtons[3] ? (byte)0xF7 : (byte)0xFF;
            }
            if (selectButtons)
            {
                state &= _joypadButtons[4] ? (byte)0xFE : (byte)0xFF;
                state &= _joypadButtons[5] ? (byte)0xFD : (byte)0xFF;
                state &= _joypadButtons[6] ? (byte)0xFB : (byte)0xFF;
                state &= _joypadButtons[7] ? (byte)0xF7 : (byte)0xFF;
            }
            return state;
        }
        public byte this[ushort address]
        {
            get => ReadByte(address);
            set => WriteByte(address, value);
        }
        private void ValidateButtonIndex(int button)
        {
            if (button < 0 || button > 7)
            {
                throw new ArgumentOutOfRangeException(nameof(button), "Button index must be between 0 and 7.");
            }
        }
    }
}
