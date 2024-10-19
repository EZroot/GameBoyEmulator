using GameBoyEmulator.Debugger;
using GameBoyEmulator.Memory;
using GameBoyEmulator.Interrupts;

namespace GameBoyEmulator.Processor
{
    internal class CPU
    {
        Opcode _opcode;
        Registers _registers;
        InterruptController _interruptController;
        RAM _memory;

        public CPU(Registers registers, Opcode opcode, RAM ram)
        {
            this._registers = registers;
            this._memory = ram;
            this._opcode = opcode;
            this._interruptController = new InterruptController(ram, registers);

            Logger.Log("CPU Initialized.");
        }

        public int Step()
        {
            _interruptController.HandleInterrupts();
            byte opcode = _memory.ReadByte(_registers.PC);
            _registers.PC++;
            int cycles = _opcode.ExecuteInstruction(opcode);
            if (_registers.EI_Scheduled)
            {
                _registers.IME = true;
                _registers.EI_Scheduled = false;
                //if (DebugMode) Console.WriteLine("Interrupt Master Enable (IME) set to true.");
            }
            return cycles;
        }
    }
}
