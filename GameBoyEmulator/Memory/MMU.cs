using GameBoyEmulator.Processor;
namespace GameBoyEmulator.Memory
{
    public class MMU
    {
        private readonly RAM _ram;
        private readonly Registers _registers;
        private readonly bool[] _joypadButtons = new bool[8];
        private bool debugMode;
        public MMU(Registers registers, RAM ram)
        {
            _ram = ram;
            _registers = registers;
        }
        public byte ReadByte(ushort address)
        {
            if (debugMode)
            {
                Console.WriteLine($"[DEBUG] ReadByte called at address 0x{address:X4}");
            }
            if (address == 0xFF00) 
            {
                return GetJoypadState();
            }
            byte value = _ram.ReadByte(address);
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
            _ram.WriteByte(address, value);
            if (address == 0xFF46) 
            {
                HandleDMA(value);
            }
        }
        public void WriteLY(byte value)
        {
            _ram.WriteByte(0xFF44, value); 
            if (debugMode)
            {
                Console.WriteLine($"[DEBUG] PPU wrote LY=0x{value:X2}");
            }
        }
        public void PushStack(ushort value)
        {
            _registers.SP--;
            WriteByte(_registers.SP, (byte)(value >> 8));
            _registers.SP--;
            WriteByte(_registers.SP, (byte)(value & 0xFF));
        }
        public ushort PopStack()
        {
            ushort lowByte = ReadByte(_registers.SP++);
            ushort highByte = ReadByte(_registers.SP++);
            return (ushort)((highByte << 8) | lowByte);
        }
        public void HandleDMA(byte value)
        {
            ushort sourceAddress = (ushort)(value << 8);
            for (int i = 0; i < 0xA0; i++)
            {
                _ram.WriteByte((ushort)(0xFE00 + i), ReadByte((ushort)(sourceAddress + i)));
            }
            if (debugMode)
            {
                Console.WriteLine($"[DEBUG] DMA Transfer initiated from 0x{sourceAddress:X4}.");
            }
        }
        private byte GetJoypadState()
        {
            byte selector = _ram.ReadByte(0xFF00);
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
        public void SetJoypadButtonState(int buttonIndex, bool pressed)
        {
            ValidateButtonIndex(buttonIndex);
            _joypadButtons[buttonIndex] = pressed;
            if (debugMode)
            {
                Console.WriteLine($"[DEBUG] Joypad button {(pressed ? "pressed" : "released")}: {buttonIndex}");
            }
        }
        private void ValidateButtonIndex(int button)
        {
            if (button < 0 || button > 7)
            {
                throw new ArgumentOutOfRangeException(nameof(button), "Button index must be between 0 and 7.");
            }
        }
        public byte this[ushort address]
        {
            get => ReadByte(address);
            set => WriteByte(address, value);
        }
    }
}
