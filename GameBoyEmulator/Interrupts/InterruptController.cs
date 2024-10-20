using GameBoyEmulator.Processor;
using GameBoyEmulator.Debug;
using GameBoyEmulator.Memory;
using System;
namespace GameBoyEmulator.Interrupts
{
    public class InterruptController
    {
        private readonly RAM _memory;
        private readonly MMU _mmu;
        private readonly Registers _register;
        private const int InterruptHandlingCycles = 20;
        public InterruptController(Registers registry, MMU mmu, RAM memory)
        {
            _memory = memory;
            _register = registry;
            _mmu = mmu;
            Logger.Log("Interrupt Controller Initialized.");
        }
        public int HandleInterrupts()
        {
            byte interruptEnable = _memory.ReadByte(0xFFFF);
            byte interruptFlags = _memory.ReadByte(0xFF0F);
            byte requestedInterrupts = (byte)(interruptEnable & interruptFlags); 
            if (requestedInterrupts == 0)
            {
                return 0; 
            }
            if (_register.Halted)
            {
                _register.Halted = false;
            }
            if (_register.IME)
            {
                _register.IME = false; 
                _register.Halted = false; 
                if ((requestedInterrupts & InterruptFlags.VBlank) != 0)
                {
                    HandleInterrupt(InterruptFlags.VBlank, 0x0040);
                    return InterruptHandlingCycles;
                }
                if ((requestedInterrupts & InterruptFlags.LCDSTAT) != 0)
                {
                    HandleInterrupt(InterruptFlags.LCDSTAT, 0x0048);
                    return InterruptHandlingCycles;
                }
                if ((requestedInterrupts & InterruptFlags.Timer) != 0)
                {
                    HandleInterrupt(InterruptFlags.Timer, 0x0050);
                    return InterruptHandlingCycles;
                }
                if ((requestedInterrupts & InterruptFlags.Serial) != 0)
                {
                    HandleInterrupt(InterruptFlags.Serial, 0x0058);
                    return InterruptHandlingCycles;
                }
                if ((requestedInterrupts & InterruptFlags.Joypad) != 0)
                {
                    HandleInterrupt(InterruptFlags.Joypad, 0x0060);
                    return InterruptHandlingCycles;
                }
            }
            return 0; 
        }
        private void HandleInterrupt(byte interruptBit, ushort interruptAddress)
        {
            byte interruptFlagsReg = _memory.ReadByte(0xFF0F);
            interruptFlagsReg &= (byte)~interruptBit; 
            _memory.WriteByte(0xFF0F, interruptFlagsReg);
            _mmu.PushStack(_register.PC);
            _register.PC = interruptAddress;
            if (Debugger.IsDebugEnabled && Debugger.dWriteOutMemoryReadWrite)
            {
                Console.WriteLine($"[DEBUG] Interrupt handled: 0x{interruptAddress:X4}");
            }
        }
    }
}
