using GameBoyEmulator.Debug;
using GameBoyEmulator.Memory;
using GameBoyEmulator.Utils;
using System;
namespace GameBoyEmulator.Processor
{
    public class Opcode
    {
        private Registers _registers;
        private RAM _ram;
        private MMU _mmu;
        private int stepThroughOpcodeCount;
        private int subroutineDepth = 0;  
        public Opcode(Registers registers, MMU mmu, RAM ram)
        {
            this._registers = registers;
            this._ram = ram;
            this._mmu = mmu;
            Logger.Log("Opcode initialized.");
        }
        public int ExecuteInstruction(byte opcode)
        {
            int cycles = 0;
            var message = $"Executing Opcode: 0x{opcode:X2} | PC: 0x{_registers.PC - 1:X4} | Registers: A=0x{_registers.A:X2} F=0x{_registers.F:X2} B=0x{_registers.B:X2} C=0x{_registers.C:X2} D=0x{_registers.D:X2} E=0x{_registers.E:X2} H=0x{_registers.H:X2} L=0x{_registers.L:X2} SP=0x{_registers.SP:X4}";
            FileHelper.LogToFile(message);
            if (Debugger.IsDebugEnabled && Debugger.dWriteOutOpcode)
            {
                Logger.Log($"{message}");
                if (Debugger.dStepThroughOpcode)
                {
                    stepThroughOpcodeCount--;
                    if (stepThroughOpcodeCount <= 0)
                    {
                        Console.ReadKey();
                        stepThroughOpcodeCount = Debugger.dStepThroughOpcodeStepCount;
                    }
                }
            }
            switch (opcode)
            {
                case 0x00: 
                    cycles = Opcode_0x00();
                    break;
                case 0x7D: 
                    cycles = Opcode_0x7D();
                    break;
                case 0x24: 
                    cycles = Opcode_0x24();
                    break;
                case 0xB7: 
                    cycles = Opcode_0xB7();
                    break;
                case 0x75: 
                    cycles = Opcode_0x75();
                    break;
                case 0x6E: 
                    cycles = Opcode_0x6E();
                    break;
                case 0x74: 
                    cycles = Opcode_0x74();
                    break;
                case 0x72: 
                    cycles = Opcode_0x72();
                    break;
                case 0x30: 
                    cycles = Opcode_0x30();
                    break;
                case 0x02: 
                    cycles = Opcode_0x02();
                    break;
                case 0x03: 
                    cycles = Opcode_0x03();
                    break;
                case 0x0B: 
                    cycles = Opcode_0x0B();
                    break;
                case 0x0D: 
                    cycles = Opcode_0x0D();
                    break;
                case 0x08: 
                    cycles = Opcode_0x08();
                    break;
                case 0x11: 
                    cycles = Opcode_0x11();
                    break;
                case 0x1F: 
                    cycles = Opcode_0x1F();
                    break;
                case 0x88: 
                    cycles = Opcode_0x88();
                    break;
                case 0xBA: 
                    cycles = Opcode_0xBA();
                    break;
                case 0xD8: 
                    cycles = Opcode_0xD8();
                    break;
                case 0x38: 
                    cycles = Opcode_0x38();
                    break;
                case 0x89: 
                    cycles = Opcode_0x89();
                    break;
                case 0x5C: 
                    cycles = Opcode_0x5C();
                    break;
                case 0xCE: 
                    cycles = Opcode_0xCE();
                    break;
                case 0xC3: 
                    cycles = Opcode_0xC3();
                    break;
                case 0xCC: 
                    cycles = Opcode_0xCC();
                    break;
                case 0xF3: 
                    cycles = Opcode_0xF3();
                    break;
                case 0xAF: 
                    cycles = Opcode_0xAF();
                    break;
                case 0xE0: 
                    cycles = Opcode_0xE0();
                    break;
                case 0x0F: 
                    cycles = Opcode_0x0F();
                    break;
                case 0x26: 
                    cycles = Opcode_0x26();
                    break;
                case 0x41: 
                    cycles = Opcode_0x41();
                    break;
                case 0x42: 
                    cycles = Opcode_0x42();
                    break;
                case 0x43: 
                    cycles = Opcode_0x43();
                    break;
                case 0x45: 
                    cycles = Opcode_0x45();
                    break;
                case 0x3E: 
                    cycles = Opcode_0x3E();
                    break;
                case 0xFC: 
                    cycles = Opcode_0xFC();
                    break;
                case 0x47: 
                    cycles = Opcode_0x47();
                    break;
                case 0x10: 
                    cycles = Opcode_0x10();
                    break;
                case 0x80: 
                    cycles = Opcode_0x80();
                    break;
                case 0xFF: 
                    cycles = Opcode_0xFF();
                    break;
                case 0x81: 
                    cycles = Opcode_0x81();
                    break;
                case 0xF0: 
                    cycles = Opcode_0xF0();
                    break;
                case 0x44: 
                    cycles = Opcode_0x44();
                    break;
                case 0xFE: 
                    cycles = Opcode_0xFE();
                    break;
                case 0x90: 
                    cycles = Opcode_0x90();
                    break;
                case 0xCB: 
                    {
                        byte cbOpcode = _ram.ReadByte(_registers.PC++);
                        cycles = ExecuteCBInstruction(cbOpcode);
                        break;
                    }
                case 0x20: 
                    cycles = Opcode_0x20();
                    break;
                case 0xFA: 
                    cycles = Opcode_0xFA();
                    break;
                case 0x21: 
                    cycles = Opcode_0x21();
                    break;
                case 0x73: 
                    cycles = Opcode_0x73();
                    break;
                case 0x01: 
                    cycles = Opcode_0x01();
                    break;
                case 0xB0: 
                    cycles = Opcode_0xB0();
                    break;
                case 0x05: 
                    cycles = Opcode_0x05();
                    break;
                case 0x1A: 
                    cycles = Opcode_0x1A();
                    break;
                case 0x22: 
                    cycles = Opcode_0x22();
                    break;
                case 0x13: 
                    cycles = Opcode_0x13();
                    break;
                case 0x78: 
                    cycles = Opcode_0x78();
                    break;
                case 0xB1: 
                    cycles = Opcode_0xB1();
                    break;
                case 0xF8: 
                    cycles = Opcode_0xF8();
                    break;
                case 0xA7: 
                    cycles = Opcode_0xA7();
                    break;
                case 0x18: 
                    cycles = Opcode_0x18();
                    break;
                case 0xF5: 
                    cycles = Opcode_0xF5();
                    break;
                case 0xE5: 
                    cycles = Opcode_0xE5();
                    break;
                case 0xCD: 
                    cycles = Opcode_0xCD();
                    break;
                case 0x4E: 
                    cycles = Opcode_0x4E();
                    break;
                case 0xE1: 
                    cycles = Opcode_0xE1();
                    break;
                case 0xF1: 
                    cycles = Opcode_0xF1();
                    break;
                case 0xFB: 
                    cycles = Opcode_0xFB();
                    break;
                case 0xD9: 
                    cycles = Opcode_0xD9();
                    break;
                case 0x35: 
                    cycles = Opcode_0x35();
                    break;
                case 0x28: 
                    cycles = Opcode_0x28();
                    break;
                case 0x3C: 
                    cycles = Opcode_0x3C();
                    break;
                case 0x2E: 
                    cycles = Opcode_0x2E();
                    break;
                case 0x5D: 
                    cycles = Opcode_0x5D();
                    break;
                case 0x0A: 
                    cycles = Opcode_0x0A();
                    break;
                case 0x85: 
                    cycles = Opcode_0x85();
                    break;
                case 0xC9: 
                    cycles = Opcode_0xC9();
                    break;
                case 0x36: 
                    cycles = Opcode_0x36();
                    break;
                case 0x6F: 
                    cycles = Opcode_0x6F();
                    break;
                case 0x7E: 
                    cycles = Opcode_0x7E();
                    break;
                case 0x16: 
                    cycles = Opcode_0x16();
                    break;
                case 0x06: 
                    cycles = Opcode_0x06();
                    break;
                case 0x15: 
                    cycles = Opcode_0x15();
                    break;
                case 0x0E: 
                    cycles = Opcode_0x0E();
                    break;
                case 0x32: 
                    cycles = Opcode_0x32();
                    break;
                case 0xEA: 
                    cycles = Opcode_0xEA();
                    break;
                case 0xD0: 
                    cycles = Opcode_0xD0();
                    break;
                case 0x77: 
                    cycles = Opcode_0x77();
                    break;
                case 0x31: 
                    cycles = Opcode_0x31();
                    break;
                case 0x12: 
                    cycles = Opcode_0x12();
                    break;
                case 0xBF: 
                    cycles = Opcode_0xBF();
                    break;
                case 0x2A: 
                    cycles = Opcode_0x2A();
                    break;
                case 0xE2: 
                    cycles = Opcode_0xE2();
                    break;
                case 0x0C: 
                    cycles = Opcode_0x0C();
                    break;
                case 0xC0: 
                    cycles = Opcode_0xC0();
                    break;
                case 0xCF: 
                    cycles = Opcode_0xCF();
                    break;
                case 0x2F: 
                    cycles = Opcode_0x2F();
                    break;
                case 0xE6: 
                    cycles = Opcode_0xE6();
                    break;
                case 0x79: 
                    cycles = Opcode_0x79();
                    break;
                case 0xD5: 
                    cycles = Opcode_0xD5();
                    break;
                case 0xCA: 
                    cycles = Opcode_0xCA();
                    break;
                case 0x7A: 
                    cycles = Opcode_0x7A();
                    break;
                case 0x66: 
                    cycles = Opcode_0x66();
                    break;
                case 0xC2: 
                    cycles = Opcode_0xC2();
                    break;
                case 0x84: 
                    cycles = Opcode_0x84();
                    break;
                case 0x1C: 
                    cycles = Opcode_0x1C();
                    break;
                case 0xC8: 
                    cycles = Opcode_0xC8();
                    break;
                case 0x34: 
                    cycles = Opcode_0x34();
                    break;
                case 0xBE: 
                    cycles = Opcode_0xBE();
                    break;
                case 0x2C: 
                    cycles = Opcode_0x2C();
                    break;
                case 0x67: 
                    cycles = Opcode_0x67();
                    break;
                case 0xC5: 
                    cycles = Opcode_0xC5();
                    break;
                case 0x40: 
                    cycles = Opcode_0x40();
                    break;
                case 0x56: 
                    cycles = Opcode_0x56();
                    break;
                case 0xE9: 
                    cycles = Opcode_0xE9();
                    break;
                case 0xBB: 
                    cycles = Opcode_0xBB();
                    break;
                case 0xD1: 
                    cycles = Opcode_0xD1();
                    break;
                case 0x76: 
                    cycles = Opcode_0x76();
                    break;
                case 0xC1: 
                    cycles = Opcode_0xC1();
                    break;
                case 0x3D: 
                    cycles = Opcode_0x3D();
                    break;
                case 0xA1: 
                    cycles = Opcode_0xA1();
                    break;
                case 0x87: 
                    cycles = Opcode_0x87();
                    break;
                case 0x5F: 
                    cycles = Opcode_0x5F();
                    break;
                case 0x19: 
                    cycles = Opcode_0x19();
                    break;
                case 0x5E: 
                    cycles = Opcode_0x5E();
                    break;
                case 0x23: 
                    cycles = Opcode_0x23();
                    break;
                case 0xA9: 
                    cycles = Opcode_0xA9();
                    break;
                case 0xEF: 
                    cycles = Opcode_0xEF();
                    break;
                case 0x92: 
                    cycles = Opcode_0x92();
                    break;
                case 0x04: 
                    cycles = Opcode_0x04();
                    break;
                case 0xDA: 
                    cycles = Opcode_0xDA();
                    break;
                case 0x8F: 
                    cycles = Opcode_0x8F();
                    break;
                case 0xC6: 
                    cycles = Opcode_0xC6();
                    break;
                case 0xB4: 
                    cycles = Opcode_0xB4();
                    break;
                case 0x39: 
                    cycles = Opcode_0x39();
                    break;
                case 0xF4: 
                    cycles = Opcode_0xF4();
                    break;
                case 0x14: 
                    cycles = Opcode_0x14();
                    break;
                case 0xE8: 
                    cycles = Opcode_0xE8();
                    break;
                case 0x33: 
                    cycles = Opcode_0x33();
                    break;
                case 0xD6: 
                    cycles = Opcode_0xD6();
                    break;
                case 0xC4: 
                    cycles = Opcode_0xC4();
                    break;
                case 0x2D: 
                    cycles = Opcode_0x2D();
                    break;
                case 0xA3: 
                    cycles = Opcode_0xA3();
                    break;
                case 0x4F: 
                    cycles = Opcode_0x4F();
                    break;
                case 0x09: 
                    cycles = Opcode_0x09();
                    break;
                case 0x46: 
                    cycles = Opcode_0x46();
                    break;
                case 0x69: 
                    cycles = Opcode_0x69();
                    break;
                case 0xAE: 
                    cycles = Opcode_0xAE();
                    break;
                case 0x71: 
                    cycles = Opcode_0x71();
                    break;
                case 0x25: 
                    cycles = Opcode_0x25();
                    break;
                case 0xB6: 
                    cycles = Opcode_0xB6();
                    break;
                case 0x83: 
                    cycles = Opcode_0x83();
                    break;
                case 0x17: 
                    cycles = Opcode_0x17();
                    break;
                case 0x27: 
                    cycles = Opcode_0x27();
                    break;
                case 0x57: 
                    cycles = Opcode_0x57();
                    break;
                case 0xEE: 
                    cycles = Opcode_0xEE();
                    break;
                case 0xAD: 
                    cycles = Opcode_0xAD();
                    break;
                case 0xED: 
                    cycles = Opcode_0xED();
                    break;
                case 0x60: 
                    cycles = Opcode_0x60();
                    break;
                case 0x7C: 
                    cycles = Opcode_0x7C();
                    break;
                case 0x61: 
                    cycles = Opcode_0x61();
                    break;
                case 0x29: 
                    cycles = Opcode_0x29();
                    break;
                case 0x1B: 
                    cycles = Opcode_0x1B();
                    break;
                case 0xF6: 
                    cycles = Opcode_0xF6();
                    break;
                case 0xB8: 
                    cycles = Opcode_0xB8();
                    break;
                case 0x63: 
                    cycles = Opcode_0x63();
                    break;
                case 0xF9: 
                    cycles = Opcode_0xF9();
                    break;
                case 0x62: 
                    cycles = Opcode_0x62();
                    break;
                case 0x07: 
                    cycles = Opcode_0x07();
                    break;
                case 0x4A: 
                    cycles = Opcode_0x4A();
                    break;
                case 0x50: 
                    cycles = Opcode_0x50();
                    break;
                case 0x64: 
                    cycles = Opcode_0x64();
                    break;
                case 0x1D: 
                    cycles = Opcode_0x1D();
                    break;
                case 0x7B: 
                    cycles = Opcode_0x7B();
                    break;
                case 0x70: 
                    cycles = Opcode_0x70();
                    break;
                case 0xC7: 
                    cycles = Opcode_0xC7();
                    break;
                case 0xAC: 
                    cycles = Opcode_0xAC();
                    break;
                case 0x91: 
                    cycles = Opcode_0x91();
                    break;
                case 0x1E: 
                    cycles = Opcode_0x1E();
                    break;
                case 0x9D: 
                    cycles = Opcode_0x9D();
                    break;
                case 0x6B: 
                    cycles = Opcode_0x6B();
                    break;
                case 0x3A:
                    cycles = Opcode_0x3A();
                    break;
                default:
                    message = $"Unimplemented opcode: 0x{opcode:X2} at address 0x{_registers.PC - 1:X4}";
                    Logger.Log($"<color=red>{message}</color>");
                    FileHelper.LogToFile(message);
                    if (Debugger.IsDebugEnabled) Console.ReadKey();
                    cycles = 4; 
                    break;
            }
            return cycles;
        }
        private int ExecuteCBInstruction(byte opcode)
        {
            int cycles = 0;
            byte value;
            byte result;
            ushort address;
            switch (opcode)
            {
                case 0x38:
                    cycles = Opcode_CB_38();
                    break;
                case 0x1A:
                    cycles = Opcode_CB_1A();
                    break;
                case 0x19:
                    cycles = Opcode_CB_19();
                    break;
                case 0x3F: 
                    cycles = Opcode_CB_3F();
                    break;
                case 0x37: 
                    cycles = Opcode_CB_37();
                    break;
                case 0x87: 
                    cycles = Opcode_CB_87();
                    break;
                case 0x27: 
                    cycles = Opcode_CB_27();
                    break;
                case 0xBE: 
                    cycles = Opcode_CB_BE();
                    break;
                case 0xFE: 
                    cycles = Opcode_CB_FE();
                    break;
                case 0x1B: 
                    cycles = Opcode_CB_1B();
                    break;
                case 0x72:
                    cycles = Opcode_CB_72();
                    break;
                case 0x73:
                    cycles = Opcode_CB_73();
                    break;
                case 0x74:
                    cycles = Opcode_CB_74();
                    break;
                case 0x75:
                    cycles = Opcode_CB_75();
                    break;
                case 0x77:
                    cycles = Opcode_CB_77();
                    break;
                case 0x40: 
                    cycles = Opcode_CB_40();
                    break;
                case 0x79:
                    cycles = Opcode_CB_79();
                    break;
                case 0x7A:
                    cycles = Opcode_CB_7A();
                    break;
                case 0x7B:
                    cycles = Opcode_CB_7B();
                    break;
                case 0x7C:
                    cycles = Opcode_CB_7C();
                    break;
                case 0x7D:
                    cycles = Opcode_CB_7D();
                    break;
                case 0x7F:
                    cycles = Opcode_CB_7F();
                    break;
                case 0x70: 
                    cycles = Opcode_CB_70();
                    break;
                case 0x78: 
                    cycles = Opcode_CB_78();
                    break;
                case 0x50: 
                    cycles = Opcode_CB_50();
                    break;
                case 0x58: 
                    cycles = Opcode_CB_58();
                    break;
                case 0x80:
                    cycles = Opcode_CB_80();
                    break;
                case 0x81:
                    cycles = Opcode_CB_81();
                    break;
                case 0x82:
                    cycles = Opcode_CB_82();
                    break;
                case 0x83:
                    cycles = Opcode_CB_83();
                    break;
                case 0x84:
                    cycles = Opcode_CB_84();
                    break;
                case 0x85:
                    cycles = Opcode_CB_85();
                    break;
                case 0x88:
                    cycles = Opcode_CB_88();
                    break;
                case 0x89:
                    cycles = Opcode_CB_89();
                    break;
                case 0x8A:
                    cycles = Opcode_CB_8A();
                    break;
                case 0x8B:
                    cycles = Opcode_CB_8B();
                    break;
                case 0x8C:
                    cycles = Opcode_CB_8C();
                    break;
                case 0x8D:
                    cycles = Opcode_CB_8D();
                    break;
                case 0x8F:
                    cycles = Opcode_CB_8F();
                    break;
                case 0x90:
                    cycles = Opcode_CB_90();
                    break;
                case 0x91:
                    cycles = Opcode_CB_91();
                    break;
                case 0x92:
                    cycles = Opcode_CB_92();
                    break;
                case 0x93:
                    cycles = Opcode_CB_93();
                    break;
                case 0x94:
                    cycles = Opcode_CB_94();
                    break;
                case 0x95:
                    cycles = Opcode_CB_95();
                    break;
                case 0x97:
                    cycles = Opcode_CB_97();
                    break;
                case 0x67: 
                    cycles = Opcode_CB_67();
                    break;
                case 0x68: 
                    cycles = Opcode_CB_68();
                    break;
                case 0x69: 
                    cycles = Opcode_CB_69();
                    break;
                case 0x6A: 
                    cycles = Opcode_CB_6A();
                    break;
                case 0x6B: 
                    cycles = Opcode_CB_6B();
                    break;
                case 0x6C: 
                    cycles = Opcode_CB_6C();
                    break;
                case 0x6D: 
                    cycles = Opcode_CB_6D();
                    break;
                case 0x6F: 
                    cycles = Opcode_CB_6F();
                    break;
                case 0x71: 
                    cycles = Opcode_CB_71();
                    break;
                case 0x12:
                    cycles = Opcode_CB_12();
                    break;
                default:
                    var message = $"Unimplemented CB opcode:</color> 0x{opcode:X2} at address 0x{_registers.PC - 2:X4}";
                    Logger.Log($"<color=cyan>{message}");
                    FileHelper.LogToFile(message);
                    if (Debugger.IsDebugEnabled) Console.ReadKey();
                    cycles = 4;
                    break;
            }
            return cycles;
        }
        public int Opcode_0x3A() 
        {
            ushort hl = _registers.GetHL(); 
            _registers.A = _ram.ReadByte(hl); 
            _registers.SetHL((ushort)(hl - 1)); 
            return 8; 
        }
        public int Opcode_CB_12() 
        {
            _registers.D = (byte)(_registers.D & ~(1 << 0)); 
            _registers.SetNegativeFlag(false); 
            _registers.SetHalfCarryFlag(false); 
            return 8; 
        }
        public int Opcode_0xAE()
        {
            byte value = _ram.ReadByte(_registers.GetHL());
            _registers.XorWithA(value);
            return 8; 
        }
        public int Opcode_0x71()
        {
            _ram.WriteByte(_registers.GetHL(), _registers.C);
            return 8; 
        }
        public int Opcode_0x25()
        {
            _registers.H = _registers.Decrement(_registers.H);
            return 4; 
        }
        public int Opcode_0xB6()
        {
            byte value = _ram.ReadByte(_registers.GetHL());
            _registers.OrWithA(value);
            return 8; 
        }
        public int Opcode_0x83()
        {
            _registers.AddToA(_registers.E);
            return 4; 
        }
        public int Opcode_0x57()
        {
            _registers.D = _registers.A;
            return 4; 
        }
        public int Opcode_0x17() 
        {
            byte carryIn = _registers.IsFlagSet(Registers.CarryFlag) ? (byte)1 : (byte)0; 
            byte newCarry = (byte)((_registers.A & 0x80) >> 7); 
            _registers.A = (byte)((_registers.A << 1) | carryIn); 
            _registers.SetCarryFlag(newCarry != 0); 
            _registers.SetZeroFlag(false); 
            _registers.SetNegativeFlag(false); 
            _registers.SetHalfCarryFlag(false); 
            return 4; 
        }
        public int Opcode_0xAD() 
        {
            _registers.XorWithA(_registers.L); 
            return 4; 
        }
        public int Opcode_0xEE()
        {
            byte immediate = _ram.ReadByte(_registers.PC++);
            _registers.XorWithA(immediate);
            return 8; 
        }
        public int Opcode_0x29()
        {
            ushort hl = _registers.GetHL();
            int result = hl + hl;
            _registers.SetCarryFlag(result > 0xFFFF);
            _registers.SetHalfCarryFlag(((hl & 0x0FFF) + (hl & 0x0FFF)) > 0x0FFF);
            _registers.SetNegativeFlag(false);
            _registers.SetHL((ushort)(result & 0xFFFF));
            return 8; 
        }
        public int Opcode_0x1B()
        {
            ushort de = _registers.GetDE();
            _registers.SetDE((ushort)(de - 1));
            return 8; 
        }
        public int Opcode_0xF6()
        {
            byte immediate = _ram.ReadByte(_registers.PC++);
            _registers.OrWithA(immediate);
            return 8; 
        }
        public int Opcode_0xED()
        {
            _registers.PC = _mmu.PopStack();
            _registers.IME = true; 
            return 16; 
        }
        public int Opcode_0x6B() 
        {
            _registers.L = _registers.E; 
            return 4; 
        }
        public int Opcode_0x24() 
        {
            _registers.H = _registers.Increment(_registers.H); 
            return 4; 
        }
        public int Opcode_0xB7() 
        {
            _registers.A |= _registers.A; 
            _registers.SetZeroFlag(_registers.A == 0); 
            _registers.SetNegativeFlag(false); 
            _registers.SetHalfCarryFlag(false); 
            _registers.SetCarryFlag(false); 
            return 4; 
        }
        public int Opcode_0x75() 
        {
            _ram.WriteByte(_registers.GetHL(), 0); 
            return 8; 
        }
        public int Opcode_0x6E() 
        {
            _registers.L = _ram.ReadByte(_registers.GetHL()); 
            return 8; 
        }
        public int Opcode_0x74() 
        {
            _ram.WriteByte(_registers.GetHL(), _registers.H); 
            return 8; 
        }
        public int Opcode_0x72() 
        {
            _ram.WriteByte(_registers.GetHL(), _registers.D); 
            return 8; 
        }
        public int Opcode_0x30() 
        {
            sbyte offset = (sbyte)_ram.ReadByte(_registers.PC++); 
            if (!_registers.GetCarryFlag()) 
            {
                _registers.PC = (ushort)(_registers.PC + offset); 
                return 12; 
            }
            return 8; 
        }
        public int Opcode_0x7D() 
        {
            _registers.A = _registers.L; 
            return 4; 
        }
        public int Opcode_0x27() 
        {
            _registers.DAA(); 
            return 4; 
        }
        public int Opcode_CB_50() 
        {
            bool isBitSet = (_registers.B & (1 << 2)) != 0; 
            _registers.SetZeroFlag(!isBitSet); 
            _registers.SetNegativeFlag(false); 
            _registers.SetHalfCarryFlag(true); 
            return 8; 
        }
        public int Opcode_CB_58() 
        {
            bool isBitSet = (_registers.B & (1 << 3)) != 0; 
            _registers.SetZeroFlag(!isBitSet); 
            _registers.SetNegativeFlag(false); 
            _registers.SetHalfCarryFlag(true); 
            return 8; 
        }
        public int Opcode_CB_78() 
        {
            bool isBitSet = (_registers.B & (1 << 7)) != 0; 
            _registers.SetZeroFlag(!isBitSet); 
            _registers.SetNegativeFlag(false); 
            _registers.SetHalfCarryFlag(true); 
            return 8; 
        }
        public int Opcode_CB_70() 
        {
            bool isBitSet = (_registers.B & (1 << 6)) != 0; 
            _registers.SetZeroFlag(!isBitSet); 
            _registers.SetNegativeFlag(false); 
            _registers.SetHalfCarryFlag(true); 
            return 8; 
        }
        public int Opcode_CB_72() 
        {
            bool isBitSet = (_registers.D & (1 << 6)) != 0;
            _registers.SetZeroFlag(!isBitSet);
            _registers.SetNegativeFlag(false);
            _registers.SetHalfCarryFlag(true);
            return 8; 
        }
        public int Opcode_CB_73() 
        {
            bool isBitSet = (_registers.E & (1 << 6)) != 0;
            _registers.SetZeroFlag(!isBitSet);
            _registers.SetNegativeFlag(false);
            _registers.SetHalfCarryFlag(true);
            return 8; 
        }
        public int Opcode_CB_74() 
        {
            bool isBitSet = (_registers.H & (1 << 6)) != 0;
            _registers.SetZeroFlag(!isBitSet);
            _registers.SetNegativeFlag(false);
            _registers.SetHalfCarryFlag(true);
            return 8; 
        }
        public int Opcode_CB_75() 
        {
            bool isBitSet = (_registers.L & (1 << 6)) != 0;
            _registers.SetZeroFlag(!isBitSet);
            _registers.SetNegativeFlag(false);
            _registers.SetHalfCarryFlag(true);
            return 8; 
        }
        public int Opcode_CB_79() 
        {
            bool isBitSet = (_registers.C & (1 << 7)) != 0;
            _registers.SetZeroFlag(!isBitSet);
            _registers.SetNegativeFlag(false);
            _registers.SetHalfCarryFlag(true);
            return 8; 
        }
        public int Opcode_CB_7A() 
        {
            bool isBitSet = (_registers.D & (1 << 7)) != 0;
            _registers.SetZeroFlag(!isBitSet);
            _registers.SetNegativeFlag(false);
            _registers.SetHalfCarryFlag(true);
            return 8; 
        }
        public int Opcode_CB_7B() 
        {
            bool isBitSet = (_registers.E & (1 << 7)) != 0;
            _registers.SetZeroFlag(!isBitSet);
            _registers.SetNegativeFlag(false);
            _registers.SetHalfCarryFlag(true);
            return 8; 
        }
        public int Opcode_CB_7C() 
        {
            bool isBitSet = (_registers.H & (1 << 7)) != 0;
            _registers.SetZeroFlag(!isBitSet);
            _registers.SetNegativeFlag(false);
            _registers.SetHalfCarryFlag(true);
            return 8; 
        }
        public int Opcode_CB_7D() 
        {
            bool isBitSet = (_registers.L & (1 << 7)) != 0;
            _registers.SetZeroFlag(!isBitSet);
            _registers.SetNegativeFlag(false);
            _registers.SetHalfCarryFlag(true);
            return 8; 
        }
        public int Opcode_CB_7F() 
        {
            bool isBitSet = (_registers.A & (1 << 7)) != 0;
            _registers.SetZeroFlag(!isBitSet);
            _registers.SetNegativeFlag(false);
            _registers.SetHalfCarryFlag(true);
            return 8; 
        }
        public int Opcode_CB_77() 
        {
            ushort hlAddress = _registers.GetHL();
            byte value = _ram.ReadByte(hlAddress);
            bool isBitSet = (value & (1 << 6)) != 0;
            _registers.SetZeroFlag(!isBitSet);
            _registers.SetNegativeFlag(false);
            _registers.SetHalfCarryFlag(true);
            return 12; 
        }
        public int Opcode_CB_40() 
        {
            bool isBitSet = (_registers.B & (1 << 0)) != 0; 
            _registers.SetZeroFlag(!isBitSet); 
            _registers.SetNegativeFlag(false); 
            _registers.SetHalfCarryFlag(true); 
            return 8; 
        }
        public int Opcode_CB_FE() 
        {
            ushort hlAddress = _registers.GetHL(); 
            byte value = _ram.ReadByte(hlAddress); 
            value = (byte)(value | (1 << 7)); 
            _ram.WriteByte(hlAddress, value); 
            return 16; 
        }
        public int Opcode_0x1D() 
        {
            _registers.E = _registers.Decrement(_registers.E);
            return 4; 
        }
        public int Opcode_0x9D() 
        {
            _registers.SbcFromA(_registers.L);
            return 4; 
        }
        public int Opcode_0x91() 
        {
            _registers.SubtractFromA(_registers.C);
            return 4; 
        }
        public int Opcode_0xAC() 
        {
            _registers.XorWithA(_registers.H);
            return 4; 
        }
        public int Opcode_0xC7() 
        {
            _ram.WriteByte(--_registers.SP, (byte)((_registers.PC >> 8) & 0xFF)); 
            _ram.WriteByte(--_registers.SP, (byte)(_registers.PC & 0xFF));        
            _registers.PC = 0x0000; 
            return 16; 
        }
        public int Opcode_0x70() 
        {
            _ram.WriteByte(_registers.GetHL(), _registers.B);
            return 8; 
        }
        public int Opcode_0x7B() 
        {
            _registers.A = _registers.E;
            return 4; 
        }
        public int Opcode_CB_BE() 
        {
            ushort hlAddress = _registers.GetHL(); 
            byte value = _ram.ReadByte(hlAddress); 
            value = (byte)(value & ~(1 << 7)); 
            _ram.WriteByte(hlAddress, value); 
            return 16; 
        }
        public int Opcode_0x4A() 
        {
            _registers.C = _registers.D;
            return 4; 
        }
        public int Opcode_0x50() 
        {
            _registers.D = _registers.B;
            return 4; 
        }
        public int Opcode_0x64() 
        {
            return 4; 
        }
        public int Opcode_0x07() 
        {
            byte carry = (byte)((_registers.A & 0x80) >> 7); 
            _registers.A = (byte)((_registers.A << 1) | carry); 
            _registers.SetZeroFlag(false); 
            _registers.SetNegativeFlag(false); 
            _registers.SetHalfCarryFlag(false); 
            _registers.SetCarryFlag(carry == 1); 
            return 4; 
        }
        public int Opcode_0x63() 
        {
            _registers.H = _registers.E;
            return 4; 
        }
        public int Opcode_0xF9() 
        {
            _registers.SP = _registers.GetHL();
            return 8; 
        }
        public int Opcode_0x62() 
        {
            _registers.H = _registers.D;
            return 4; 
        }
        public int Opcode_0xB8() 
        {
            _registers.CompareA(_registers.B);
            return 4; 
        }
        public int Opcode_0x61() 
        {
            _registers.H = _registers.C;
            return 4; 
        }
        public int Opcode_0x7C() 
        {
            _registers.A = _registers.H;
            return 4; 
        }
        public int Opcode_0x5C() 
        {
            _registers.E = _registers.H; 
            return 4; 
        }
        public int Opcode_0x60() 
        {
            _registers.H = _registers.B;
            return 4; 
        }
        public int Opcode_0x1E() 
        {
            byte immediateValue = _ram.ReadByte(_registers.PC++);
            _registers.E = immediateValue;
            return 8; 
        }
        public int Opcode_CB_3F() 
        {
            byte a = _registers.A;
            _registers.SetCarryFlag((a & 0x01) != 0);
            _registers.A = (byte)(a >> 1);
            _registers.SetZeroFlag(_registers.A == 0);
            _registers.SetNegativeFlag(false);
            _registers.SetHalfCarryFlag(false);
            return 8; 
        }
        public int Opcode_0x69() 
        {
            _registers.L = _registers.C;
            return 4; 
        }
        public int Opcode_0x46() 
        {
            _registers.B = _ram.ReadByte(_registers.GetHL());
            return 8; 
        }
        public int Opcode_0x09() 
        {
            int result = _registers.GetHL() + _registers.GetBC();
            _registers.SetHalfCarryFlag((_registers.GetHL() & 0x0FFF) + (_registers.GetBC() & 0x0FFF) > 0x0FFF); 
            _registers.SetCarryFlag(result > 0xFFFF); 
            _registers.SetHL((ushort)result);
            _registers.SetNegativeFlag(false); 
            return 8; 
        }
        public int Opcode_CB_27() 
        {
            byte carry = (byte)((_registers.A & 0x80) >> 7); 
            _registers.A = (byte)(_registers.A << 1); 
            _registers.A &= 0xFE; 
            _registers.SetZeroFlag(_registers.A == 0); 
            _registers.SetNegativeFlag(false); 
            _registers.SetHalfCarryFlag(false); 
            _registers.SetCarryFlag(carry == 1); 
            return 8; 
        }
        public int Opcode_CB_37() 
        {
            byte value = _registers.A;
            _registers.A = (byte)((value >> 4) | (value << 4)); 
            _registers.SetZeroFlag(_registers.A == 0);
            _registers.SetNegativeFlag(false);
            _registers.SetHalfCarryFlag(false);
            _registers.SetCarryFlag(false);
            return 8; 
        }
        public int Opcode_CB_87() 
        {
            _registers.A = (byte)(_registers.A & ~(1 << 0)); 
            return 8; 
        }
        public int Opcode_0x76() 
        {
            _registers.Halted = true; 
            return 4; 
        }
        public int Opcode_0xC4()
        {
            ushort address = _ram.ReadWord(_registers.PC);
            _registers.PC += 2;
            if (!_registers.GetZeroFlag())
            {
                _ram.WriteByte(--_registers.SP, (byte)((_registers.PC >> 8) & 0xFF)); 
                _ram.WriteByte(--_registers.SP, (byte)(_registers.PC & 0xFF));        
                _registers.PC = address;
                return 24; 
            }
            return 12; 
        }
        public int Opcode_0x2D()
        {
            _registers.L = _registers.Decrement(_registers.L);
            return 4; 
        }
        public int Opcode_0xA3()
        {
            _registers.AndWithA(_registers.E);
            return 4; 
        }
        public int Opcode_0x56()
        {
            _registers.D = _ram.ReadByte(_registers.GetHL());
            return 8; 
        }
        public int Opcode_0xE9()
        {
            _registers.PC = _registers.GetHL();
            return 4; 
        }
        public int Opcode_0xBB()
        {
            _registers.CompareA(_registers.E);
            return 4; 
        }
        public int Opcode_0xD1()
        {
            _registers.E = _ram.ReadByte(_registers.SP++);
            _registers.D = _ram.ReadByte(_registers.SP++);
            return 12; 
        }
        public int Opcode_0xC1()
        {
            _registers.C = _ram.ReadByte(_registers.SP++);
            _registers.B = _ram.ReadByte(_registers.SP++);
            return 12; 
        }
        public int Opcode_0x3D()
        {
            _registers.A = _registers.Decrement(_registers.A);
            return 4; 
        }
        public int Opcode_0xA1()
        {
            _registers.AndWithA(_registers.C);
            return 4; 
        }
        public int Opcode_0x87()
        {
            _registers.AddToA(_registers.A);
            return 4; 
        }
        public int Opcode_0x5F()
        {
            _registers.E = _registers.A;
            return 4; 
        }
        public int Opcode_0x19()
        {
            int result = _registers.GetHL() + _registers.GetDE();
            _registers.SetHalfCarryFlag((_registers.GetHL() & 0x0FFF) + (_registers.GetDE() & 0x0FFF) > 0x0FFF);
            _registers.SetCarryFlag(result > 0xFFFF);
            _registers.SetHL((ushort)result);
            _registers.SetNegativeFlag(false);
            return 8; 
        }
        public int Opcode_0x5E()
        {
            _registers.E = _ram.ReadByte(_registers.GetHL());
            return 8; 
        }
        public int Opcode_0x23()
        {
            _registers.SetHL((ushort)(_registers.GetHL() + 1));
            return 8; 
        }
        public int Opcode_0xC0()
        {
            if (!_registers.GetZeroFlag())
            {
                _registers.PC = (ushort)(_ram.ReadByte(_registers.SP++) | (_ram.ReadByte(_registers.SP++) << 8));
                return 20; 
            }
            return 8; 
        }
        public int Opcode_0xCF()
        {
            _ram.WriteByte(--_registers.SP, (byte)((_registers.PC >> 8) & 0xFF)); 
            _ram.WriteByte(--_registers.SP, (byte)(_registers.PC & 0xFF));        
            _registers.PC = 0x0008;
            return 16; 
        }
        public int Opcode_0x2F()
        {
            _registers.A = (byte)~_registers.A;
            _registers.SetNegativeFlag(true);
            _registers.SetHalfCarryFlag(true);
            return 4; 
        }
        public int Opcode_0xE6()
        {
            byte value = _ram.ReadByte(_registers.PC++);
            _registers.A &= value;
            _registers.SetZeroFlag(_registers.A == 0);
            _registers.SetNegativeFlag(false);
            _registers.SetHalfCarryFlag(true);
            _registers.SetCarryFlag(false);
            return 8; 
        }
        public int Opcode_0x79()
        {
            _registers.A = _registers.C;
            return 4; 
        }
        public int Opcode_0xEF()
        {
            _ram.WriteByte(--_registers.SP, (byte)((_registers.PC >> 8) & 0xFF)); 
            _ram.WriteByte(--_registers.SP, (byte)(_registers.PC & 0xFF));        
            _registers.PC = 0x0028;
            return 16; 
        }
        public int Opcode_0x92()
        {
            _registers.SubtractFromA(_registers.D);
            return 4; 
        }
        public int Opcode_0x04()
        {
            _registers.B = _registers.Increment(_registers.B);
            return 4; 
        }
        public int Opcode_0xDA()
        {
            ushort address = _ram.ReadWord(_registers.PC);
            _registers.PC += 2;
            if (_registers.GetCarryFlag())
            {
                _registers.PC = address;
                return 16; 
            }
            return 12; 
        }
        public int Opcode_0x8F()
        {
            _registers.AdcToA(_registers.A);
            return 4; 
        }
        public int Opcode_0xC6()
        {
            byte value = _ram.ReadByte(_registers.PC++);
            _registers.AddToA(value);
            return 8; 
        }
        public int Opcode_0xB4()
        {
            _registers.A |= _registers.H;
            _registers.SetZeroFlag(_registers.A == 0);
            _registers.SetNegativeFlag(false);
            _registers.SetHalfCarryFlag(false);
            _registers.SetCarryFlag(false);
            return 4; 
        }
        public int Opcode_0x39()
        {
            int result = _registers.GetHL() + _registers.SP;
            _registers.SetHalfCarryFlag((_registers.GetHL() & 0x0FFF) + (_registers.SP & 0x0FFF) > 0x0FFF);
            _registers.SetCarryFlag(result > 0xFFFF);
            _registers.SetHL((ushort)result);
            _registers.SetNegativeFlag(false);
            return 8; 
        }
        public int Opcode_CB_67() 
        {
            bool isBitSet = (_registers.A & (1 << 4)) != 0;
            _registers.SetZeroFlag(!isBitSet);
            _registers.SetNegativeFlag(false);
            _registers.SetHalfCarryFlag(true);
            return 8; 
        }
        public int Opcode_CB_68() 
        {
            bool isBitSet = (_registers.B & (1 << 5)) != 0;
            _registers.SetZeroFlag(!isBitSet);
            _registers.SetNegativeFlag(false);
            _registers.SetHalfCarryFlag(true);
            return 8; 
        }
        public int Opcode_CB_80() 
        {
            _registers.B = (byte)(_registers.B & ~(1 << 0)); 
            _registers.SetNegativeFlag(false); 
            _registers.SetHalfCarryFlag(false); 
            return 8; 
        }
        public int Opcode_CB_81() 
        {
            _registers.C = (byte)(_registers.C & ~(1 << 0)); 
            _registers.SetNegativeFlag(false); 
            _registers.SetHalfCarryFlag(false); 
            return 8; 
        }
        public int Opcode_CB_82() 
        {
            _registers.D = (byte)(_registers.D & ~(1 << 0)); 
            _registers.SetNegativeFlag(false); 
            _registers.SetHalfCarryFlag(false); 
            return 8; 
        }
        public int Opcode_CB_83() 
        {
            _registers.E = (byte)(_registers.E & ~(1 << 0)); 
            _registers.SetNegativeFlag(false); 
            _registers.SetHalfCarryFlag(false); 
            return 8; 
        }
        public int Opcode_CB_84() 
        {
            _registers.H = (byte)(_registers.H & ~(1 << 0)); 
            _registers.SetNegativeFlag(false); 
            _registers.SetHalfCarryFlag(false); 
            return 8; 
        }
        public int Opcode_CB_85() 
        {
            _registers.L = (byte)(_registers.L & ~(1 << 0)); 
            _registers.SetNegativeFlag(false); 
            _registers.SetHalfCarryFlag(false); 
            return 8; 
        }
        public int Opcode_CB_88() 
        {
            _registers.B = (byte)(_registers.B & ~(1 << 1)); 
            _registers.SetNegativeFlag(false); 
            _registers.SetHalfCarryFlag(false); 
            return 8; 
        }
        public int Opcode_CB_89() 
        {
            _registers.C = (byte)(_registers.C & ~(1 << 1)); 
            _registers.SetNegativeFlag(false); 
            _registers.SetHalfCarryFlag(false); 
            return 8; 
        }
        public int Opcode_CB_8A() 
        {
            _registers.D = (byte)(_registers.D & ~(1 << 1)); 
            _registers.SetNegativeFlag(false); 
            _registers.SetHalfCarryFlag(false); 
            return 8; 
        }
        public int Opcode_CB_8B() 
        {
            _registers.E = (byte)(_registers.E & ~(1 << 1)); 
            _registers.SetNegativeFlag(false); 
            _registers.SetHalfCarryFlag(false); 
            return 8; 
        }
        public int Opcode_CB_8C() 
        {
            _registers.H = (byte)(_registers.H & ~(1 << 1)); 
            _registers.SetNegativeFlag(false); 
            _registers.SetHalfCarryFlag(false); 
            return 8; 
        }
        public int Opcode_CB_8D() 
        {
            _registers.L = (byte)(_registers.L & ~(1 << 1)); 
            _registers.SetNegativeFlag(false); 
            _registers.SetHalfCarryFlag(false); 
            return 8; 
        }
        public int Opcode_CB_8F() 
        {
            _registers.A = (byte)(_registers.A & ~(1 << 1)); 
            _registers.SetNegativeFlag(false); 
            _registers.SetHalfCarryFlag(false); 
            return 8; 
        }
        public int Opcode_CB_90() 
        {
            _registers.B = (byte)(_registers.B & ~(1 << 2)); 
            _registers.SetNegativeFlag(false); 
            _registers.SetHalfCarryFlag(false); 
            return 8; 
        }
        public int Opcode_CB_91() 
        {
            _registers.C = (byte)(_registers.C & ~(1 << 2)); 
            _registers.SetNegativeFlag(false); 
            _registers.SetHalfCarryFlag(false); 
            return 8; 
        }
        public int Opcode_CB_92() 
        {
            _registers.D = (byte)(_registers.D & ~(1 << 2)); 
            _registers.SetNegativeFlag(false); 
            _registers.SetHalfCarryFlag(false); 
            return 8; 
        }
        public int Opcode_CB_93() 
        {
            _registers.E = (byte)(_registers.E & ~(1 << 2)); 
            _registers.SetNegativeFlag(false); 
            _registers.SetHalfCarryFlag(false); 
            return 8; 
        }
        public int Opcode_CB_94() 
        {
            _registers.H = (byte)(_registers.H & ~(1 << 2)); 
            _registers.SetNegativeFlag(false); 
            _registers.SetHalfCarryFlag(false); 
            return 8; 
        }
        public int Opcode_CB_95() 
        {
            _registers.L = (byte)(_registers.L & ~(1 << 2)); 
            _registers.SetNegativeFlag(false); 
            _registers.SetHalfCarryFlag(false); 
            return 8; 
        }
        public int Opcode_CB_97() 
        {
            _registers.A = (byte)(_registers.A & ~(1 << 2)); 
            _registers.SetNegativeFlag(false); 
            _registers.SetHalfCarryFlag(false); 
            return 8; 
        }
        public int Opcode_CB_69() 
        {
            bool isBitSet = (_registers.C & (1 << 5)) != 0;
            _registers.SetZeroFlag(!isBitSet);
            _registers.SetNegativeFlag(false);
            _registers.SetHalfCarryFlag(true);
            return 8; 
        }
        public int Opcode_CB_6A() 
        {
            bool isBitSet = (_registers.D & (1 << 5)) != 0;
            _registers.SetZeroFlag(!isBitSet);
            _registers.SetNegativeFlag(false);
            _registers.SetHalfCarryFlag(true);
            return 8; 
        }
        public int Opcode_CB_6B() 
        {
            bool isBitSet = (_registers.E & (1 << 5)) != 0;
            _registers.SetZeroFlag(!isBitSet);
            _registers.SetNegativeFlag(false);
            _registers.SetHalfCarryFlag(true);
            return 8; 
        }
        public int Opcode_CB_6C() 
        {
            bool isBitSet = (_registers.H & (1 << 5)) != 0;
            _registers.SetZeroFlag(!isBitSet);
            _registers.SetNegativeFlag(false);
            _registers.SetHalfCarryFlag(true);
            return 8; 
        }
        public int Opcode_CB_6D() 
        {
            bool isBitSet = (_registers.L & (1 << 5)) != 0;
            _registers.SetZeroFlag(!isBitSet);
            _registers.SetNegativeFlag(false);
            _registers.SetHalfCarryFlag(true);
            return 8; 
        }
        public int Opcode_CB_6F() 
        {
            bool isBitSet = (_registers.A & (1 << 5)) != 0;
            _registers.SetZeroFlag(!isBitSet);
            _registers.SetNegativeFlag(false);
            _registers.SetHalfCarryFlag(true);
            return 8; 
        }
        public int Opcode_CB_71() 
        {
            bool isBitSet = (_registers.C & (1 << 6)) != 0;
            _registers.SetZeroFlag(!isBitSet);
            _registers.SetNegativeFlag(false);
            _registers.SetHalfCarryFlag(true);
            return 8; 
        }
        public int Opcode_0xF4()
        {
            _registers.PC += 2; 
            return 12; 
        }
        public int Opcode_0x14()
        {
            _registers.D = _registers.Increment(_registers.D);
            return 4; 
        }
        public int Opcode_0xD5()
        {
            _ram.WriteByte(--_registers.SP, _registers.D);
            _ram.WriteByte(--_registers.SP, _registers.E);
            return 16; 
        }
        public int Opcode_0xCA()
        {
            ushort address = _ram.ReadWord(_registers.PC);
            _registers.PC += 2;
            if (_registers.GetZeroFlag())
            {
                _registers.PC = address;
                return 16; 
            }
            return 12; 
        }
        public int Opcode_0x7A()
        {
            _registers.A = _registers.D;
            return 4; 
        }
        public int Opcode_0x66()
        {
            _registers.H = _ram.ReadByte(_registers.GetHL());
            return 8; 
        }
        public int Opcode_0xC2()
        {
            ushort address = _ram.ReadWord(_registers.PC);
            _registers.PC += 2;
            if (!_registers.GetZeroFlag())
            {
                _registers.PC = address;
                return 16; 
            }
            return 12; 
        }
        public int Opcode_0x84()
        {
            _registers.AddToA(_registers.H);
            return 4; 
        }
        public int Opcode_0x1C() 
        {
            _registers.E = _registers.Increment(_registers.E); 
            _registers.SetZeroFlag(_registers.E == 0); 
            _registers.SetNegativeFlag(false); 
            _registers.SetHalfCarryFlag((_registers.E & 0x0F) == 0); 
            return 4; 
        }
        public int Opcode_0xC8()
        {
            if (_registers.GetZeroFlag())
            {
                _registers.PC = (ushort)(_ram.ReadByte(_registers.SP++) | (_ram.ReadByte(_registers.SP++) << 8));
                return 20; 
            }
            return 8; 
        }
        public int Opcode_0x34()
        {
            byte value = _ram.ReadByte(_registers.GetHL());
            value = _registers.Increment(value);
            _ram.WriteByte(_registers.GetHL(), value);
            return 12; 
        }
        public int Opcode_0xBE()
        {
            byte value = _ram.ReadByte(_registers.GetHL());
            _registers.CompareA(value);
            return 8; 
        }
        public int Opcode_0x2C()
        {
            _registers.L = _registers.Increment(_registers.L);
            return 4; 
        }
        public int Opcode_0x67()
        {
            _registers.H = _registers.A;
            return 4; 
        }
        public int Opcode_0xC5()
        {
            _ram.WriteByte(--_registers.SP, _registers.B);
            _ram.WriteByte(--_registers.SP, _registers.C);
            return 16; 
        }
        public int Opcode_0x40()
        {
            return 4; 
        }
        public int Opcode_0xA9()
        {
            _registers.A ^= _registers.C;
            _registers.SetZeroFlag(_registers.A == 0);
            _registers.SetNegativeFlag(false);
            _registers.SetHalfCarryFlag(false);
            _registers.SetCarryFlag(false);
            return 4; 
        }
        public int Opcode_0xE8()
        {
            sbyte value = (sbyte)_ram.ReadByte(_registers.PC++);
            ushort result = (ushort)(_registers.SP + value);
            _registers.SetZeroFlag(false);
            _registers.SetNegativeFlag(false);
            _registers.SetHalfCarryFlag((_registers.SP & 0x0F) + (value & 0x0F) > 0x0F);
            _registers.SetCarryFlag((_registers.SP & 0xFF) + (value & 0xFF) > 0xFF);
            _registers.SP = result;
            return 16; 
        }
        public int Opcode_0x33()
        {
            _registers.SP++;
            return 8; 
        }
        public int Opcode_0xD6()
        {
            byte value = _ram.ReadByte(_registers.PC++);
            _registers.SubtractFromA(value);
            return 8; 
        }
        public int Opcode_0x4F()
        {
            _registers.C = _registers.A;
            return 4; 
        }
        public int Opcode_0x12()
        {
            _ram.WriteByte(_registers.GetDE(), _registers.A);
            return 8; 
        }
        public int Opcode_0x2A()
        {
            _registers.A = _ram.ReadByte(_registers.GetHL());
            _registers.SetHL((ushort)(_registers.GetHL() + 1));
            return 8; 
        }
        public int Opcode_0xE2()
        {
            _ram.WriteByte((ushort)(0xFF00 + _registers.C), _registers.A);
            return 8; 
        }
        public int Opcode_0x0C()
        {
            _registers.C = _registers.Increment(_registers.C);
            return 4; 
        }
        public int Opcode_0x77() 
        {
            _ram.WriteByte(_registers.GetHL(), _registers.A);
            return 8; 
        }
        public int Opcode_0x31() 
        {
            _registers.SP = _ram.ReadWord(_registers.PC);
            _registers.PC += 2;
            return 12; 
        }
        public int Opcode_0xBF() 
        {
            _registers.CompareA(_registers.A); 
            return 4; 
        }
        public int Opcode_0x32() 
        {
            _ram.WriteByte(_registers.GetHL(), _registers.A);
            _registers.SetHL((ushort)(_registers.GetHL() - 1));
            return 8; 
        }
        public int Opcode_0x0E() 
        {
            _registers.C = _ram.ReadByte(_registers.PC++);
            return 8; 
        }
        public int Opcode_0xEA() 
        {
            ushort address = _ram.ReadWord(_registers.PC);
            _registers.PC += 2;
            _ram.WriteByte(address, _registers.A);
            return 16; 
        }
        public int Opcode_0xD0() 
        {
            if (!_registers.GetCarryFlag())
            {
                _registers.PC = (ushort)(_ram.ReadByte(_registers.SP++) | (_ram.ReadByte(_registers.SP++) << 8));
                return 20; 
            }
            return 8; 
        }
        public int Opcode_0x15() 
        {
            _registers.D = _registers.Decrement(_registers.D);
            return 4; 
        }
        public int Opcode_0x06() 
        {
            _registers.B = _ram.ReadByte(_registers.PC++);
            return 8; 
        }
        public int Opcode_0x6F() 
        {
            _registers.L = _registers.A;
            return 4; 
        }
        public int Opcode_0xC9() 
        {
            ushort low = _ram.ReadByte(_registers.SP++);  
            ushort high = _ram.ReadByte(_registers.SP++);  
            _registers.PC = (ushort)((high << 8) | low);  
            if (Debugger.dStepOutModeFlag && subroutineDepth == 0)  
            {
                Debugger.dStepOutModeFlag = false;  
                Logger.Log("Step out completed.");
                Console.ReadKey();  
            }
            subroutineDepth--;  
            return 16;  
        }
        public int Opcode_0x36() 
        {
            byte value = _ram.ReadByte(_registers.PC++);
            _ram.WriteByte(_registers.GetHL(), value);
            return 12; 
        }
        public int Opcode_0x7E() 
        {
            _registers.A = _ram.ReadByte(_registers.GetHL());
            return 8; 
        }
        public int Opcode_0x16() 
        {
            _registers.D = _ram.ReadByte(_registers.PC++);
            return 8; 
        }
        public int Opcode_0x3C() 
        {
            _registers.A = _registers.Increment(_registers.A);
            return 4; 
        }
        public int Opcode_0x35() 
        {
            byte value = _ram.ReadByte(_registers.GetHL());
            value = _registers.Decrement(value);
            _ram.WriteByte(_registers.GetHL(), value);
            return 12; 
        }
        public int Opcode_0x28() 
        {
            sbyte offset = (sbyte)_ram.ReadByte(_registers.PC++);
            if (_registers.GetZeroFlag())
            {
                _registers.PC = (ushort)(_registers.PC + offset);
                return 12; 
            }
            return 8; 
        }
        public int Opcode_0x85() 
        {
            _registers.AddToA(_registers.L);
            return 4; 
        }
        public int Opcode_0x0A() 
        {
            _registers.A = _ram.ReadByte(_registers.GetBC());
            return 8; 
        }
        public int Opcode_0xFB() 
        {
            _registers.EI_Scheduled = true; 
            return 4; 
        }
        public int Opcode_0x20() 
        {
            sbyte offset = (sbyte)_ram.ReadByte(_registers.PC++);
            if (!_registers.GetZeroFlag())
            {
                if (Debugger.IsDebugEnabled && Debugger.dWriteOutOpcode) Console.WriteLine($"[DEBUG] Jumping to PC: {_registers.PC + offset}");
                _registers.PC = (ushort)(_registers.PC + offset);
                return 12; 
            }
            if(Debugger.IsDebugEnabled && Debugger.dWriteOutOpcode) Console.WriteLine("[DEBUG] Zero flag is set, not jumping.");
            return 8; 
        }
        public int Opcode_CB_1B() 
        {
            _registers.E = _registers.RotateRightThroughCarry(_registers.E); 
            return 8; 
        }
        public int Opcode_0x5D() 
        {
            _registers.E = _registers.L; 
            return 4; 
        }
        public int Opcode_0x2E() 
        {
            _registers.L = _ram.ReadByte(_registers.PC++); 
            return 8; 
        }
        public int Opcode_0xFA() 
        {
            ushort address = _ram.ReadWord(_registers.PC);
            _registers.PC += 2;
            _registers.A = _ram.ReadByte(address);
            return 16; 
        }
        public int Opcode_0x21() 
        {
            _registers.SetHL(_ram.ReadWord(_registers.PC));
            _registers.PC += 2;
            return 12; 
        }
        public int Opcode_0x73() 
        {
            _ram.WriteByte(_registers.GetHL(), _registers.E);
            return 8; 
        }
        public int Opcode_0x01() 
        {
            _registers.SetBC(_ram.ReadWord(_registers.PC));
            _registers.PC += 2;
            return 12; 
        }
        public int Opcode_0xB0() 
        {
            _registers.A |= _registers.B;
            _registers.SetZeroFlag(_registers.A == 0);
            _registers.SetNegativeFlag(false);
            _registers.SetHalfCarryFlag(false);
            _registers.SetCarryFlag(false);
            return 4; 
        }
        public int Opcode_0x05() 
        {
            _registers.B = _registers.Decrement(_registers.B);
            return 4; 
        }
        public int Opcode_0x1A() 
        {
            _registers.A = _ram.ReadByte(_registers.GetDE());
            return 8; 
        }
        public int Opcode_0x22() 
        {
            _ram.WriteByte(_registers.GetHL(), _registers.A);
            _registers.SetHL((ushort)(_registers.GetHL() + 1));
            return 8; 
        }
        public int Opcode_0x13() 
        {
            _registers.SetDE((ushort)(_registers.GetDE() + 1));
            return 8; 
        }
        public int Opcode_0x78() 
        {
            _registers.A = _registers.B;
            return 4; 
        }
        public int Opcode_0xB1() 
        {
            _registers.A |= _registers.C;
            _registers.SetZeroFlag(_registers.A == 0);
            _registers.SetNegativeFlag(false);
            _registers.SetHalfCarryFlag(false);
            _registers.SetCarryFlag(false);
            return 4; 
        }
        public int Opcode_0xF8() 
        {
            sbyte offset = (sbyte)_ram.ReadByte(_registers.PC++);
            ushort result = (ushort)(_registers.SP + offset);
            _registers.SetHL(result);
            _registers.SetZeroFlag(false);
            _registers.SetNegativeFlag(false);
            _registers.SetHalfCarryFlag((_registers.SP & 0x0F) + (offset & 0x0F) > 0x0F);
            _registers.SetCarryFlag((_registers.SP & 0xFF) + (offset & 0xFF) > 0xFF);
            return 12; 
        }
        public int Opcode_0xA7() 
        {
            _registers.A &= _registers.A;
            _registers.SetZeroFlag(_registers.A == 0);
            _registers.SetNegativeFlag(false);
            _registers.SetHalfCarryFlag(true);
            _registers.SetCarryFlag(false);
            return 4; 
        }
        public int Opcode_0x18() 
        {
            sbyte offset = (sbyte)_ram.ReadByte(_registers.PC++);
            _registers.PC = (ushort)(_registers.PC + offset);
            return 12; 
        }
        public int Opcode_0xF5() 
        {
            _ram.WriteByte(--_registers.SP, _registers.A);
            _ram.WriteByte(--_registers.SP, _registers.F);
            return 16; 
        }
        public int Opcode_0xE5() 
        {
            _ram.WriteByte(--_registers.SP, _registers.H);
            _ram.WriteByte(--_registers.SP, _registers.L);
            return 16; 
        }
        public int Opcode_0xCD() 
        {
            ushort addr = _ram.ReadWord(_registers.PC);  
            _registers.PC += 2;  
            _ram.WriteByte(--_registers.SP, (byte)((_registers.PC >> 8) & 0xFF));  
            _ram.WriteByte(--_registers.SP, (byte)(_registers.PC & 0xFF));         
            _registers.PC = addr;  
            subroutineDepth++;  
            return 24;  
        }
        public int Opcode_0x4E() 
        {
            _registers.C = _ram.ReadByte(_registers.GetHL());
            return 8; 
        }
        public int Opcode_0xE1() 
        {
            _registers.L = _ram.ReadByte(_registers.SP++);
            _registers.H = _ram.ReadByte(_registers.SP++);
            return 12; 
        }
        public int Opcode_0xF1() 
        {
            _registers.F = _ram.ReadByte(_registers.SP++);
            _registers.A = _ram.ReadByte(_registers.SP++);
            return 12; 
        }
        public int Opcode_0xD9() 
        {
            _registers.PC = (ushort)(_ram.ReadByte(_registers.SP++) | (_ram.ReadByte(_registers.SP++) << 8));
            _registers.IME = true; 
            return 16; 
        }
        public int Opcode_0xAF() 
        {
            _registers.A ^= _registers.A;
            _registers.SetZeroFlag(true);
            _registers.SetNegativeFlag(false);
            _registers.SetHalfCarryFlag(false);
            _registers.SetCarryFlag(false);
            return 4; 
        }
        public int Opcode_0xE0() 
        {
            byte address = _ram.ReadByte(_registers.PC++);
            _ram.WriteByte((ushort)(0xFF00 + address), _registers.A);
            return 12; 
        }
        public int Opcode_0x0F() 
        {
            byte carry = (byte)(_registers.A & 0x01);
            _registers.A = (byte)((_registers.A >> 1) | (carry << 7));
            _registers.SetZeroFlag(false);
            _registers.SetNegativeFlag(false);
            _registers.SetHalfCarryFlag(false);
            _registers.SetCarryFlag(carry == 1);
            return 4; 
        }
        public int Opcode_0x26() 
        {
            _registers.H = _ram.ReadByte(_registers.PC++);
            return 8; 
        }
        public int Opcode_0x41() 
        {
            _registers.B = _registers.C;
            return 4; 
        }
        public int Opcode_0x42() 
        {
            _registers.B = _registers.D;
            return 4; 
        }
        public int Opcode_0x43() 
        {
            _registers.B = _registers.E;
            return 4; 
        }
        public int Opcode_0x45() 
        {
            _registers.B = _registers.L;
            return 4; 
        }
        public int Opcode_0x3E() 
        {
            _registers.A = _ram.ReadByte(_registers.PC++);
            return 8; 
        }
        public int Opcode_0xFC() 
        {
            return 4;
        }
        public int Opcode_0x47() 
        {
            _registers.B = _registers.A;
            return 4; 
        }
        public int Opcode_0x10() 
        {
            return 4; 
        }
        public int Opcode_0x80() 
        {
            _registers.AddToA(_registers.B);
            return 4; 
        }
        public int Opcode_0xFF() 
        {
            _ram.WriteByte(--_registers.SP, (byte)((_registers.PC >> 8) & 0xFF)); 
            _ram.WriteByte(--_registers.SP, (byte)(_registers.PC & 0xFF));        
            _registers.PC = 0x0038;
            return 16; 
        }
        public int Opcode_0x81() 
        {
            _registers.AddToA(_registers.C);
            return 4; 
        }
        public int Opcode_0xF0() 
        {
            byte address = _ram.ReadByte(_registers.PC++);
            _registers.A = _ram.ReadByte((ushort)(0xFF00 + address));
            return 12; 
        }
        public int Opcode_0x44() 
        {
            _registers.B = _registers.H;
            return 4; 
        }
        public int Opcode_0xFE() 
        {
            byte value = _ram.ReadByte(_registers.PC++);
            _registers.CompareA(value);
            return 8; 
        }
        public int Opcode_0x90() 
        {
            _registers.SubtractFromA(_registers.B);
            return 4; 
        }
        public int Opcode_CB_38() 
        {
            byte value = _registers.B;
            bool isBitSet = (value & (1 << 7)) != 0;
            _registers.SetZeroFlag(!isBitSet);
            _registers.SetNegativeFlag(false);
            _registers.SetHalfCarryFlag(true);
            return 8; 
        }
        public int Opcode_CB_19() 
        {
            byte value = _registers.C;
            bool isBitSet = (value & (1 << 3)) != 0;
            _registers.SetZeroFlag(!isBitSet);
            _registers.SetNegativeFlag(false);
            _registers.SetHalfCarryFlag(true);
            return 8; 
        }
        public int Opcode_CB_1A() 
        {
            byte value = _registers.D;
            bool isBitSet = (value & (1 << 3)) != 0;
            _registers.SetZeroFlag(!isBitSet);
            _registers.SetNegativeFlag(false);
            _registers.SetHalfCarryFlag(true);
            return 8; 
        }
        public int Opcode_0xF3() 
        {
            _registers.IME = false; 
            return 4; 
        }
        public int Opcode_0xC3() 
        {
            ushort address = _ram.ReadWord(_registers.PC);
            _registers.PC = address; 
            return 16; 
        }
        public int Opcode_0x00() 
        {
            return 4; 
        }
        public int Opcode_0x02() 
        {
            _ram.WriteByte(_registers.GetBC(), _registers.A);
            return 8; 
        }
        public int Opcode_0xCE() 
        {
            byte value = _ram.ReadByte(_registers.PC++);
            _registers.AdcToA(value); 
            return 8; 
        }
        public int Opcode_0xCC() 
        {
            if (_registers.GetZeroFlag())
            {
                ushort address = _ram.ReadWord(_registers.PC);
                _registers.PC = address; 
                return 24; 
            }
            else
            {
                _registers.PC += 2; 
                return 12; 
            }
        }
        public int Opcode_0x0D() 
        {
            _registers.C = _registers.Decrement(_registers.C); 
            _registers.SetZeroFlag(_registers.C == 0); 
            _registers.SetNegativeFlag(true); 
            _registers.SetHalfCarryFlag((_registers.C & 0x0F) == 0x0F); 
            return 4; 
        }
        public int Opcode_0x0B() 
        {
            ushort bc = _registers.GetBC();
            _registers.SetBC((ushort)(bc - 1)); 
            return 8; 
        }
        public int Opcode_0x03() 
        {
            _registers.SetBC((ushort)(_registers.GetBC() + 1)); 
            return 8; 
        }
        public int Opcode_0x08() 
        {
            ushort address = _ram.ReadWord(_registers.PC);
            _ram.WriteWord(address, _registers.SP); 
            return 20; 
        }
        public int Opcode_0x11() 
        {
            _registers.SetDE(_ram.ReadWord(_registers.PC)); 
            return 12; 
        }
        public int Opcode_0x1F() 
        {
            _registers.A = _registers.RotateRightThroughCarry(_registers.A); 
            return 4; 
        }
        public int Opcode_0x88() 
        {
            _registers.AdcToA(_registers.B); 
            return 4; 
        }
        public int Opcode_0xBA() 
        {
            _registers.CompareA(_registers.D); 
            return 4; 
        }
        public int Opcode_0xD8() 
        {
            if (_registers.GetCarryFlag())
            {
                _registers.PC = _ram.ReadWord(_registers.SP); 
                _registers.SP += 2; 
                return 20; 
            }
            return 8; 
        }
        public int Opcode_0x38() 
        {
            sbyte offset = (sbyte)_ram.ReadByte(_registers.PC++);
            if (_registers.GetCarryFlag())
            {
                _registers.PC = (ushort)(_registers.PC + offset); 
                return 12; 
            }
            return 8; 
        }
        public int Opcode_0x89() 
        {
            _registers.AdcToA(_registers.C); 
            return 4; 
        }
    }
}
