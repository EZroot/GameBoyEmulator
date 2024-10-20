using GameBoyEmulator.Debug;
using GameBoyEmulator.Memory;
using GameBoyEmulator.Interrupts;
namespace GameBoyEmulator.Processor
{
    public class CPU
    {
        private Opcode _opcode;
        private Registers _registers;
        private InterruptController _interruptController;
        private RAM _memory;
        public CPU(Registers registers, Opcode opcode, RAM ram, InterruptController interruptController)
        {
            this._registers = registers;
            this._memory = ram;
            this._opcode = opcode;
            this._interruptController = interruptController;
            Logger.Log("CPU Initialized.");
        }
        public int Step()
        {
            int cycles = 0;
            int interruptCycles = _interruptController.HandleInterrupts();
            if (interruptCycles > 0)
            {
                cycles += interruptCycles;
                return cycles; 
            }
            if (_registers.Halted)
            {
                cycles += 4;
                return cycles;
            }
            byte opcode = _memory.ReadByte(_registers.PC);
            _registers.PC++;
            cycles += _opcode.ExecuteInstruction(opcode);
            if (_registers.EI_Scheduled)
            {
                _registers.IME = true;
                _registers.EI_Scheduled = false;
                if (_registers.DebugMode)
                {
                    Console.WriteLine("Interrupt Master Enable (IME) set to true.");
                }
            }
            return cycles;
        }
    }
}
