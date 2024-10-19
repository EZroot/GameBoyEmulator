using GameBoyEmulator.Processor;
using GameBoyEmulator.Debugger;
using GameBoyEmulator.Memory;

namespace GameBoyEmulator.Interrupts
{
    internal class InterruptController
    {
        RAM _memory;
        Registers _register;

        public InterruptController(RAM memory, Registers registry)
        {
            _memory = memory;
            _register = registry;
            Logger.Log("Interrupt Controller Initialized.");
        }

        public void HandleInterrupts()
        {
            if (!_register.IME)
                return;
            byte interruptEnable = _memory.ReadByte(0xFFFF);
            byte interruptFlags = _memory.ReadByte(0xFF0F);
            byte enabledInterrupts = (byte)(interruptEnable & interruptFlags);
            if (enabledInterrupts == 0)
                return;
            _register.IME = false;
            if ((enabledInterrupts & 0x01) != 0)
            {
                HandleInterrupt(0x01, 0x0040, ref interruptFlags);
                return;
            }
            if ((enabledInterrupts & 0x02) != 0)
            {
                HandleInterrupt(0x02, 0x0048, ref interruptFlags);
                return;
            }
            if ((enabledInterrupts & 0x04) != 0)
            {
                HandleInterrupt(0x04, 0x0050, ref interruptFlags);
                return;
            }
            if ((enabledInterrupts & 0x08) != 0)
            {
                HandleInterrupt(0x08, 0x0058, ref interruptFlags);
                return;
            }
            if ((enabledInterrupts & 0x10) != 0)
            {
                HandleInterrupt(0x10, 0x0060, ref interruptFlags);
                return;
            }
        }
        private void HandleInterrupt(byte interruptBit, ushort interruptAddress, ref byte interruptFlags)
        {
            interruptFlags &= (byte)~interruptBit;
            _memory.WriteByte(0xFF0F, interruptFlags);
            _memory.PushStack(_register.PC);
            _register.PC = interruptAddress;
            //if (DebugMode) Console.WriteLine($"Handling Interrupt: 0x{interruptBit:X2} at address 0x{interruptAddress:X4}");
        }
    }
}
