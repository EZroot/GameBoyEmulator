using GameBoyEmulator.Debug;
using GameBoyEmulator.Memory;
namespace GameBoyEmulator.Processor
{
    public class Opcode
    {
        private Registers _registers;
        private RAM _ram;
        private MMU _mmu;
        private bool DebugMode = false;
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
            if (Debugger.IsDebugEnabled)
            {
                Logger.Log($"Executing Opcode: 0x{opcode:X2} | PC: 0x{_registers.PC - 1:X4} | Registers: A=0x{_registers.A:X2} F=0x{_registers.F:X2} B=0x{_registers.B:X2} C=0x{_registers.C:X2} D=0x{_registers.D:X2} E=0x{_registers.E:X2} H=0x{_registers.H:X2} L=0x{_registers.L:X2} SP=0x{_registers.SP:X4}");
                if (Debugger.dStepThroughOpcode) Console.ReadKey();
            }
            switch (opcode)
            {
                case 0x00:
                    cycles = 4;
                    break;
                case 0x01:
                    {
                        byte low = _ram.ReadByte(_registers.PC++);
                        byte high = _ram.ReadByte(_registers.PC++);
                        _registers.C = low;
                        _registers.B = high;
                        cycles = 12;
                        break;
                    }
                case 0x02:
                    _ram.WriteByte(_registers.GetBC(), _registers.A);
                    cycles = 8;
                    break;
                case 0x03:
                    _registers.SetBC((ushort)(_registers.GetBC() + 1));
                    cycles = 8;
                    break;
                case 0x04:
                    _registers.B = _registers.Increment(_registers.B);
                    cycles = 4;
                    break;
                case 0x05:
                    _registers.B = _registers.Decrement(_registers.B);
                    cycles = 4;
                    break;
                case 0x06:
                    _registers.B = _ram.ReadByte(_registers.PC++);
                    cycles = 8;
                    break;
                case 0x07:
                    _registers.A = _registers.RotateLeft(_registers.A);
                    _registers.F = (byte)((_registers.F & 0xCF) | ((_registers.A & 0x10) != 0 ? 0x10 : 0x00));
                    cycles = 4;
                    break;
                case 0x08:
                    {
                        ushort addr = _ram.ReadWord(_registers.PC);
                        _registers.PC += 2;
                        _ram.WriteWord(addr, _registers.SP);
                        cycles = 20;
                        break;
                    }
                case 0x09:
                    {
                        ushort hl = _registers.GetHL();
                        ushort bc = _registers.GetBC();
                        int result = hl + bc;
                        _registers.SetFlag(6, false); 
                        _registers.SetHalfCarryFlag(((hl & 0x0FFF) + (bc & 0x0FFF)) > 0x0FFF);
                        _registers.SetCarryFlag(result > 0xFFFF);
                        _registers.SetHL((ushort)result);
                        cycles = 8;
                        break;
                    }
                case 0x0A:
                    _registers.A = _ram.ReadByte(_registers.GetBC());
                    cycles = 8;
                    break;
                case 0x0B:
                    _registers.SetBC((ushort)(_registers.GetBC() - 1));
                    cycles = 8;
                    break;
                case 0x0C:
                    _registers.C = _registers.Increment(_registers.C);
                    cycles = 4;
                    break;
                case 0x0D:
                    _registers.C = _registers.Decrement(_registers.C);
                    cycles = 4;
                    break;
                case 0x0E:
                    _registers.C = _ram.ReadByte(_registers.PC++);
                    cycles = 8;
                    break;
                case 0x0F:
                    _registers.A = _registers.RotateRight(_registers.A);
                    _registers.F = (byte)((_registers.F & 0xCF) | ((_registers.A & 0x10) != 0 ? 0x10 : 0x00));
                    cycles = 4;
                    break;
                case 0x10:
                    _registers.PC++;
                    _registers.Halted = true;
                    cycles = 4;
                    break;
                case 0x11:
                    {
                        byte low = _ram.ReadByte(_registers.PC++);
                        byte high = _ram.ReadByte(_registers.PC++);
                        _registers.E = low;
                        _registers.D = high;
                        cycles = 12;
                        break;
                    }
                case 0x12:
                    _ram.WriteByte(_registers.GetDE(), _registers.A);
                    cycles = 8;
                    break;
                case 0x13:
                    _registers.SetDE((ushort)(_registers.GetDE() + 1));
                    cycles = 8;
                    break;
                case 0x14:
                    _registers.D = _registers.Increment(_registers.D);
                    cycles = 4;
                    break;
                case 0x15:
                    _registers.D = _registers.Decrement(_registers.D);
                    cycles = 4;
                    break;
                case 0x16:
                    _registers.D = _ram.ReadByte(_registers.PC++);
                    cycles = 8;
                    break;
                case 0x17:
                    _registers.A = _registers.RotateLeftThroughCarry(_registers.A);
                    cycles = 4;
                    break;
                case 0x18:
                    {
                        sbyte offset = (sbyte)_ram.ReadByte(_registers.PC++);
                        _registers.PC = (ushort)(_registers.PC + offset);
                        cycles = 12;
                        break;
                    }
                case 0x19:
                    {
                        ushort hl = _registers.GetHL();
                        ushort de = _registers.GetDE();
                        int result = hl + de;
                        _registers.SetFlag(6, false); 
                        _registers.SetHalfCarryFlag(((hl & 0x0FFF) + (de & 0x0FFF)) > 0x0FFF);
                        _registers.SetCarryFlag(result > 0xFFFF);
                        _registers.SetHL((ushort)result);
                        cycles = 8;
                        break;
                    }
                case 0x1A:
                    _registers.A = _ram.ReadByte(_registers.GetDE());
                    cycles = 8;
                    break;
                case 0x1B:
                    _registers.SetDE((ushort)(_registers.GetDE() - 1));
                    cycles = 8;
                    break;
                case 0x1C:
                    _registers.E = _registers.Increment(_registers.E);
                    cycles = 4;
                    break;
                case 0x1D:
                    _registers.E = _registers.Decrement(_registers.E);
                    cycles = 4;
                    break;
                case 0x1E:
                    _registers.E = _ram.ReadByte(_registers.PC++);
                    cycles = 8;
                    break;
                case 0x1F:
                    _registers.A = _registers.RotateRightThroughCarry(_registers.A);
                    cycles = 4;
                    break;
                case 0x20:
                    {
                        sbyte offset = (sbyte)_ram.ReadByte(_registers.PC++);
                        if (!_registers.IsFlagSet(Registers.ZeroFlag))
                        {
                            _registers.PC = (ushort)(_registers.PC + offset);
                            cycles = 12;
                        }
                        else
                        {
                            cycles = 8;
                        }
                        break;
                    }
                case 0x21:
                    {
                        byte low = _ram.ReadByte(_registers.PC++);
                        byte high = _ram.ReadByte(_registers.PC++);
                        _registers.L = low;
                        _registers.H = high;
                        cycles = 12;
                        break;
                    }
                case 0x22:
                    _ram.WriteByte(_registers.GetHL(), _registers.A);
                    _registers.SetHL((ushort)(_registers.GetHL() + 1));
                    cycles = 8;
                    break;
                case 0x23:
                    _registers.SetHL((ushort)(_registers.GetHL() + 1));
                    cycles = 8;
                    break;
                case 0x24:
                    _registers.H = _registers.Increment(_registers.H);
                    cycles = 4;
                    break;
                case 0x25:
                    _registers.H = _registers.Decrement(_registers.H);
                    cycles = 4;
                    break;
                case 0x26:
                    _registers.H = _ram.ReadByte(_registers.PC++);
                    cycles = 8;
                    break;
                case 0x27:
                    _registers.DAA();
                    cycles = 4;
                    break;
                case 0x28:
                    {
                        sbyte offset = (sbyte)_ram.ReadByte(_registers.PC++);
                        if (_registers.IsFlagSet(Registers.ZeroFlag))
                        {
                            _registers.PC = (ushort)(_registers.PC + offset);
                            cycles = 12;
                        }
                        else
                        {
                            cycles = 8;
                        }
                        break;
                    }
                case 0x29:
                    {
                        ushort hl = _registers.GetHL();
                        int result = hl + hl;
                        _registers.SetFlag(6, false); 
                        _registers.SetHalfCarryFlag(((hl & 0x0FFF) + (hl & 0x0FFF)) > 0x0FFF);
                        _registers.SetCarryFlag(result > 0xFFFF);
                        _registers.SetHL((ushort)result);
                        cycles = 8;
                        break;
                    }
                case 0x2A:
                    _registers.A = _ram.ReadByte(_registers.GetHL());
                    _registers.SetHL((ushort)(_registers.GetHL() + 1));
                    cycles = 8;
                    break;
                case 0x2B:
                    _registers.SetHL((ushort)(_registers.GetHL() - 1));
                    cycles = 8;
                    break;
                case 0x2C:
                    _registers.L = _registers.Increment(_registers.L);
                    cycles = 4;
                    break;
                case 0x2D:
                    _registers.L = _registers.Decrement(_registers.L);
                    cycles = 4;
                    break;
                case 0x2E:
                    _registers.L = _ram.ReadByte(_registers.PC++);
                    cycles = 8;
                    break;
                case 0x2F:
                    _registers.A = (byte)~_registers.A;
                    _registers.SetFlag(6, true); 
                    _registers.SetFlag(5, true); 
                    cycles = 4;
                    break;
                case 0x30:
                    {
                        sbyte offset = (sbyte)_ram.ReadByte(_registers.PC++);
                        if (!_registers.IsFlagSet(Registers.CarryFlag))
                        {
                            _registers.PC = (ushort)(_registers.PC + offset);
                            cycles = 12;
                        }
                        else
                        {
                            cycles = 8;
                        }
                        break;
                    }
                case 0x31:
                    _registers.SP = _ram.ReadWord(_registers.PC);
                    _registers.PC += 2;
                    cycles = 12;
                    break;
                case 0x32:
                    _ram.WriteByte(_registers.GetHL(), _registers.A);
                    _registers.SetHL((ushort)(_registers.GetHL() - 1));
                    cycles = 8;
                    break;
                case 0x33:
                    _registers.SP++;
                    cycles = 8;
                    break;
                case 0x34:
                    {
                        ushort hl = _registers.GetHL();
                        byte val = _ram.ReadByte(hl);
                        val = _registers.Increment(val);
                        _ram.WriteByte(hl, val);
                        cycles = 12;
                        break;
                    }
                case 0x35:
                    {
                        ushort hl = _registers.GetHL();
                        byte val = _ram.ReadByte(hl);
                        val = _registers.Decrement(val);
                        _ram.WriteByte(hl, val);
                        cycles = 12;
                        break;
                    }
                case 0x36:
                    _ram.WriteByte(_registers.GetHL(), _ram.ReadByte(_registers.PC++));
                    cycles = 12;
                    break;
                case 0x37:
                    _registers.SetFlag(6, false); 
                    _registers.SetFlag(5, false); 
                    _registers.SetCarryFlag(true);
                    cycles = 4;
                    break;
                case 0x38:
                    {
                        sbyte offset = (sbyte)_ram.ReadByte(_registers.PC++);
                        if (_registers.IsFlagSet(Registers.CarryFlag))
                        {
                            _registers.PC = (ushort)(_registers.PC + offset);
                            cycles = 12;
                        }
                        else
                        {
                            cycles = 8;
                        }
                        break;
                    }
                case 0x39:
                    {
                        ushort hl = _registers.GetHL();
                        ushort sp = _registers.SP;
                        int result = hl + sp;
                        _registers.SetFlag(6, false); 
                        _registers.SetHalfCarryFlag(((hl & 0x0FFF) + (sp & 0x0FFF)) > 0x0FFF);
                        _registers.SetCarryFlag(result > 0xFFFF);
                        _registers.SetHL((ushort)result);
                        cycles = 8;
                        break;
                    }
                case 0x3A:
                    _registers.A = _ram.ReadByte(_registers.GetHL());
                    _registers.SetHL((ushort)(_registers.GetHL() - 1));
                    cycles = 8;
                    break;
                case 0x3B:
                    _registers.SP--;
                    cycles = 8;
                    break;
                case 0x3C:
                    _registers.A = _registers.Increment(_registers.A);
                    cycles = 4;
                    break;
                case 0x3D:
                    _registers.A = _registers.Decrement(_registers.A);
                    cycles = 4;
                    break;
                case 0x3E:
                    _registers.A = _ram.ReadByte(_registers.PC++);
                    cycles = 8;
                    break;
                case 0x3F:
                    _registers.SetCarryFlag(!_registers.IsFlagSet(Registers.CarryFlag));
                    _registers.SetFlag(6, false); 
                    _registers.SetFlag(5, false); 
                    cycles = 4;
                    break;
                case 0x40:
                    _registers.B = _registers.B;
                    cycles = 4;
                    break;
                case 0x41:
                    _registers.B = _registers.C;
                    cycles = 4;
                    break;
                case 0x42:
                    _registers.B = _registers.D;
                    cycles = 4;
                    break;
                case 0x43:
                    _registers.B = _registers.E;
                    cycles = 4;
                    break;
                case 0x44:
                    _registers.B = _registers.H;
                    cycles = 4;
                    break;
                case 0x45:
                    _registers.B = _registers.L;
                    cycles = 4;
                    break;
                case 0x46:
                    _registers.B = _ram.ReadByte(_registers.GetHL());
                    cycles = 8;
                    break;
                case 0x47:
                    _registers.B = _registers.A;
                    cycles = 4;
                    break;
                case 0x48:
                    _registers.C = _registers.B;
                    cycles = 4;
                    break;
                case 0x49:
                    _registers.C = _registers.C;
                    cycles = 4;
                    break;
                case 0x4A:
                    _registers.C = _registers.D;
                    cycles = 4;
                    break;
                case 0x4B:
                    _registers.C = _registers.E;
                    cycles = 4;
                    break;
                case 0x4C:
                    _registers.C = _registers.H;
                    cycles = 4;
                    break;
                case 0x4D:
                    _registers.C = _registers.L;
                    cycles = 4;
                    break;
                case 0x4E:
                    _registers.C = _ram.ReadByte(_registers.GetHL());
                    cycles = 8;
                    break;
                case 0x4F:
                    _registers.C = _registers.A;
                    cycles = 4;
                    break;
                case 0x50:
                    _registers.D = _registers.B;
                    cycles = 4;
                    break;
                case 0x51:
                    _registers.D = _registers.C;
                    cycles = 4;
                    break;
                case 0x52:
                    _registers.D = _registers.D;
                    cycles = 4;
                    break;
                case 0x53:
                    _registers.D = _registers.E;
                    cycles = 4;
                    break;
                case 0x54:
                    _registers.D = _registers.H;
                    cycles = 4;
                    break;
                case 0x55:
                    _registers.D = _registers.L;
                    cycles = 4;
                    break;
                case 0x56:
                    _registers.D = _ram.ReadByte(_registers.GetHL());
                    cycles = 8;
                    break;
                case 0x57:
                    _registers.D = _registers.A;
                    cycles = 4;
                    break;
                case 0x58:
                    _registers.E = _registers.B;
                    cycles = 4;
                    break;
                case 0x59:
                    _registers.E = _registers.C;
                    cycles = 4;
                    break;
                case 0x5A:
                    _registers.E = _registers.D;
                    cycles = 4;
                    break;
                case 0x5B:
                    _registers.E = _registers.E;
                    cycles = 4;
                    break;
                case 0x5C:
                    _registers.E = _registers.H;
                    cycles = 4;
                    break;
                case 0x5D:
                    _registers.E = _registers.L;
                    cycles = 4;
                    break;
                case 0x5E:
                    _registers.E = _ram.ReadByte(_registers.GetHL());
                    cycles = 8;
                    break;
                case 0x5F:
                    _registers.E = _registers.A;
                    cycles = 4;
                    break;
                case 0x60:
                    _registers.H = _registers.B;
                    cycles = 4;
                    break;
                case 0x61:
                    _registers.H = _registers.C;
                    cycles = 4;
                    break;
                case 0x62:
                    _registers.H = _registers.D;
                    cycles = 4;
                    break;
                case 0x63:
                    _registers.H = _registers.E;
                    cycles = 4;
                    break;
                case 0x64:
                    _registers.H = _registers.H;
                    cycles = 4;
                    break;
                case 0x65:
                    _registers.H = _registers.L;
                    cycles = 4;
                    break;
                case 0x66:
                    _registers.H = _ram.ReadByte(_registers.GetHL());
                    cycles = 8;
                    break;
                case 0x67:
                    _registers.H = _registers.A;
                    cycles = 4;
                    break;
                case 0x68:
                    _registers.L = _registers.B;
                    cycles = 4;
                    break;
                case 0x69:
                    _registers.L = _registers.C;
                    cycles = 4;
                    break;
                case 0x6A:
                    _registers.L = _registers.D;
                    cycles = 4;
                    break;
                case 0x6B:
                    _registers.L = _registers.E;
                    cycles = 4;
                    break;
                case 0x6C:
                    _registers.L = _registers.H;
                    cycles = 4;
                    break;
                case 0x6D:
                    _registers.L = _registers.L;
                    cycles = 4;
                    break;
                case 0x6E:
                    _registers.L = _ram.ReadByte(_registers.GetHL());
                    cycles = 8;
                    break;
                case 0x6F:
                    _registers.L = _registers.A;
                    cycles = 4;
                    break;
                case 0x70:
                    _ram.WriteByte(_registers.GetHL(), _registers.B);
                    cycles = 8;
                    break;
                case 0x71:
                    _ram.WriteByte(_registers.GetHL(), _registers.C);
                    cycles = 8;
                    break;
                case 0x72:
                    _ram.WriteByte(_registers.GetHL(), _registers.D);
                    cycles = 8;
                    break;
                case 0x73:
                    _ram.WriteByte(_registers.GetHL(), _registers.E);
                    cycles = 8;
                    break;
                case 0x74:
                    _ram.WriteByte(_registers.GetHL(), _registers.H);
                    cycles = 8;
                    break;
                case 0x75:
                    _ram.WriteByte(_registers.GetHL(), _registers.L);
                    cycles = 8;
                    break;
                case 0x76:
                    _registers.Halted = true;
                    cycles = 4;
                    break;
                case 0x77:
                    _ram.WriteByte(_registers.GetHL(), _registers.A);
                    cycles = 8;
                    break;
                case 0x78:
                    _registers.A = _registers.B;
                    cycles = 4;
                    break;
                case 0x79:
                    _registers.A = _registers.C;
                    cycles = 4;
                    break;
                case 0x7A:
                    _registers.A = _registers.D;
                    cycles = 4;
                    break;
                case 0x7B:
                    _registers.A = _registers.E;
                    cycles = 4;
                    break;
                case 0x7C:
                    _registers.A = _registers.H;
                    cycles = 4;
                    break;
                case 0x7D:
                    _registers.A = _registers.L;
                    cycles = 4;
                    break;
                case 0x7E:
                    _registers.A = _ram.ReadByte(_registers.GetHL());
                    cycles = 8;
                    break;
                case 0x7F:
                    _registers.A = _registers.A;
                    cycles = 4;
                    break;
                case 0x80:
                    _registers.AddToA(_registers.B);
                    cycles = 4;
                    break;
                case 0x81:
                    _registers.AddToA(_registers.C);
                    cycles = 4;
                    break;
                case 0x82:
                    _registers.AddToA(_registers.D);
                    cycles = 4;
                    break;
                case 0x83:
                    _registers.AddToA(_registers.E);
                    cycles = 4;
                    break;
                case 0x84:
                    _registers.AddToA(_registers.H);
                    cycles = 4;
                    break;
                case 0x85:
                    _registers.AddToA(_registers.L);
                    cycles = 4;
                    break;
                case 0x86:
                    _registers.AddToA(_ram.ReadByte(_registers.GetHL()));
                    cycles = 8;
                    break;
                case 0x87:
                    _registers.AddToA(_registers.A);
                    cycles = 4;
                    break;
                case 0x88:
                    _registers.AdcToA(_registers.B);
                    cycles = 4;
                    break;
                case 0x89:
                    _registers.AdcToA(_registers.C);
                    cycles = 4;
                    break;
                case 0x8A:
                    _registers.AdcToA(_registers.D);
                    cycles = 4;
                    break;
                case 0x8B:
                    _registers.AdcToA(_registers.E);
                    cycles = 4;
                    break;
                case 0x8C:
                    _registers.AdcToA(_registers.H);
                    cycles = 4;
                    break;
                case 0x8D:
                    _registers.AdcToA(_registers.L);
                    cycles = 4;
                    break;
                case 0x8E:
                    _registers.AdcToA(_ram.ReadByte(_registers.GetHL()));
                    cycles = 8;
                    break;
                case 0x8F:
                    _registers.AdcToA(_registers.A);
                    cycles = 4;
                    break;
                case 0x90:
                    _registers.SubtractFromA(_registers.B);
                    cycles = 4;
                    break;
                case 0x91:
                    _registers.SubtractFromA(_registers.C);
                    cycles = 4;
                    break;
                case 0x92:
                    _registers.SubtractFromA(_registers.D);
                    cycles = 4;
                    break;
                case 0x93:
                    _registers.SubtractFromA(_registers.E);
                    cycles = 4;
                    break;
                case 0x94:
                    _registers.SubtractFromA(_registers.H);
                    cycles = 4;
                    break;
                case 0x95:
                    _registers.SubtractFromA(_registers.L);
                    cycles = 4;
                    break;
                case 0x96:
                    _registers.SubtractFromA(_ram.ReadByte(_registers.GetHL()));
                    cycles = 8;
                    break;
                case 0x97:
                    _registers.SubtractFromA(_registers.A);
                    cycles = 4;
                    break;
                case 0x98:
                    _registers.SbcFromA(_registers.B);
                    cycles = 4;
                    break;
                case 0x99:
                    _registers.SbcFromA(_registers.C);
                    cycles = 4;
                    break;
                case 0x9A:
                    _registers.SbcFromA(_registers.D);
                    cycles = 4;
                    break;
                case 0x9B:
                    _registers.SbcFromA(_registers.E);
                    cycles = 4;
                    break;
                case 0x9C:
                    _registers.SbcFromA(_registers.H);
                    cycles = 4;
                    break;
                case 0x9D:
                    _registers.SbcFromA(_registers.L);
                    cycles = 4;
                    break;
                case 0x9E:
                    _registers.SbcFromA(_ram.ReadByte(_registers.GetHL()));
                    cycles = 8;
                    break;
                case 0x9F:
                    _registers.SbcFromA(_registers.A);
                    cycles = 4;
                    break;
                case 0xA0:
                    _registers.AndWithA(_registers.B);
                    cycles = 4;
                    break;
                case 0xA1:
                    _registers.AndWithA(_registers.C);
                    cycles = 4;
                    break;
                case 0xA2:
                    _registers.AndWithA(_registers.D);
                    cycles = 4;
                    break;
                case 0xA3:
                    _registers.AndWithA(_registers.E);
                    cycles = 4;
                    break;
                case 0xA4:
                    _registers.AndWithA(_registers.H);
                    cycles = 4;
                    break;
                case 0xA5:
                    _registers.AndWithA(_registers.L);
                    cycles = 4;
                    break;
                case 0xA6:
                    _registers.AndWithA(_ram.ReadByte(_registers.GetHL()));
                    cycles = 8;
                    break;
                case 0xA7:
                    _registers.AndWithA(_registers.A);
                    cycles = 4;
                    break;
                case 0xA8:
                    _registers.XorWithA(_registers.B);
                    cycles = 4;
                    break;
                case 0xA9:
                    _registers.XorWithA(_registers.C);
                    cycles = 4;
                    break;
                case 0xAA:
                    _registers.XorWithA(_registers.D);
                    cycles = 4;
                    break;
                case 0xAB:
                    _registers.XorWithA(_registers.E);
                    cycles = 4;
                    break;
                case 0xAC:
                    _registers.XorWithA(_registers.H);
                    cycles = 4;
                    break;
                case 0xAD:
                    _registers.XorWithA(_registers.L);
                    cycles = 4;
                    break;
                case 0xAE:
                    _registers.XorWithA(_ram.ReadByte(_registers.GetHL()));
                    cycles = 8;
                    break;
                case 0xAF:
                    _registers.XorWithA(_registers.A);
                    cycles = 4;
                    break;
                case 0xB0:
                    _registers.OrWithA(_registers.B);
                    cycles = 4;
                    break;
                case 0xB1:
                    _registers.OrWithA(_registers.C);
                    cycles = 4;
                    break;
                case 0xB2:
                    _registers.OrWithA(_registers.D);
                    cycles = 4;
                    break;
                case 0xB3:
                    _registers.OrWithA(_registers.E);
                    cycles = 4;
                    break;
                case 0xB4:
                    _registers.OrWithA(_registers.H);
                    cycles = 4;
                    break;
                case 0xB5:
                    _registers.OrWithA(_registers.L);
                    cycles = 4;
                    break;
                case 0xB6:
                    _registers.OrWithA(_ram.ReadByte(_registers.GetHL()));
                    cycles = 8;
                    break;
                case 0xB7:
                    _registers.OrWithA(_registers.A);
                    cycles = 4;
                    break;
                case 0xB8:
                    _registers.CompareA(_registers.B);
                    cycles = 4;
                    break;
                case 0xB9:
                    _registers.CompareA(_registers.C);
                    cycles = 4;
                    break;
                case 0xBA:
                    _registers.CompareA(_registers.D);
                    cycles = 4;
                    break;
                case 0xBB:
                    _registers.CompareA(_registers.E);
                    cycles = 4;
                    break;
                case 0xBC:
                    _registers.CompareA(_registers.H);
                    cycles = 4;
                    break;
                case 0xBD:
                    _registers.CompareA(_registers.L);
                    cycles = 4;
                    break;
                case 0xBE:
                    _registers.CompareA(_ram.ReadByte(_registers.GetHL()));
                    cycles = 8;
                    break;
                case 0xBF:
                    _registers.CompareA(_registers.A);
                    cycles = 4;
                    break;
                case 0xCB:
                    {
                        byte cbOpcode = _ram.ReadByte(_registers.PC++);
                        cycles = ExecuteCBInstruction(cbOpcode);
                        break;
                    }
                case 0xC6:
                    {
                        byte value = _ram.ReadByte(_registers.PC++);
                        _registers.AddToA(value);
                        cycles = 8;
                        break;
                    }
                case 0xCD:
                    {
                        ushort addr = _ram.ReadWord(_registers.PC);
                        _registers.PC += 2;
                        _mmu.PushStack(_registers.PC);
                        _registers.PC = addr;
                        cycles = 24;
                        break;
                    }
                case 0xC9:
                    _registers.PC = _mmu.PopStack();
                    cycles = 16;
                    break;
                case 0xC3:
                    {
                        ushort addr = _ram.ReadWord(_registers.PC);
                        _registers.PC = addr;
                        cycles = 16;
                        break;
                    }
                case 0xC2:
                    {
                        ushort addr = _ram.ReadWord(_registers.PC);
                        _registers.PC += 2;
                        if (!_registers.IsFlagSet(Registers.ZeroFlag))
                        {
                            _registers.PC = addr;
                            cycles = 16;
                        }
                        else
                        {
                            cycles = 12;
                        }
                        break;
                    }
                case 0xCA:
                    {
                        ushort addr = _ram.ReadWord(_registers.PC);
                        _registers.PC += 2;
                        if (_registers.IsFlagSet(Registers.ZeroFlag))
                        {
                            _registers.PC = addr;
                            cycles = 16;
                        }
                        else
                        {
                            cycles = 12;
                        }
                        break;
                    }
                case 0xD9:
                    _registers.PC = _mmu.PopStack();
                    _registers.IME = true;
                    cycles = 16;
                    if (DebugMode) Console.WriteLine($"RETI executed. PC set to 0x{_registers.PC:X4} and IME set to true.");
                    break;
                case 0xC0:
                    if (!_registers.IsFlagSet(Registers.ZeroFlag))
                    {
                        _registers.PC = _mmu.PopStack();
                        cycles = 20;
                    }
                    else
                    {
                        cycles = 8;
                    }
                    break;
                case 0xC1:
                    _registers.SetBC(_mmu.PopStack());
                    cycles = 12;
                    break;
                case 0xC4:
                    {
                        ushort addr = _ram.ReadWord(_registers.PC);
                        _registers.PC += 2;
                        if (!_registers.IsFlagSet(Registers.ZeroFlag))
                        {
                            _mmu.PushStack(_registers.PC);
                            _registers.PC = addr;
                            cycles = 24;
                        }
                        else
                        {
                            cycles = 12;
                        }
                        break;
                    }
                case 0xC5:
                    _mmu.PushStack(_registers.GetBC());
                    cycles = 16;
                    break;
                case 0xC7:
                    _mmu.PushStack(_registers.PC);
                    _registers.PC = 0x00;
                    cycles = 16;
                    break;
                case 0xC8:
                    if (_registers.IsFlagSet(Registers.ZeroFlag))
                    {
                        _registers.PC = _mmu.PopStack();
                        cycles = 20;
                    }
                    else
                    {
                        cycles = 8;
                    }
                    break;
                case 0xCC:
                    {
                        ushort addr = _ram.ReadWord(_registers.PC);
                        _registers.PC += 2;
                        if (_registers.IsFlagSet(Registers.ZeroFlag))
                        {
                            _mmu.PushStack(_registers.PC);
                            _registers.PC = addr;
                            cycles = 24;
                        }
                        else
                        {
                            cycles = 12;
                        }
                        break;
                    }
                case 0xCE:
                    {
                        byte value = _ram.ReadByte(_registers.PC++);
                        _registers.AdcToA(value);
                        cycles = 8;
                        break;
                    }
                case 0xCF:
                    _mmu.PushStack(_registers.PC);
                    _registers.PC = 0x08;
                    cycles = 16;
                    break;
                case 0xD0:
                    if (!_registers.IsFlagSet(Registers.CarryFlag))
                    {
                        _registers.PC = _mmu.PopStack();
                        cycles = 20;
                    }
                    else
                    {
                        cycles = 8;
                    }
                    break;
                case 0xD1:
                    _registers.SetDE(_mmu.PopStack());
                    cycles = 12;
                    break;
                case 0xD2:
                    {
                        ushort addr = _ram.ReadWord(_registers.PC);
                        _registers.PC += 2;
                        if (!_registers.IsFlagSet(Registers.CarryFlag))
                        {
                            _registers.PC = addr;
                            cycles = 16;
                        }
                        else
                        {
                            cycles = 12;
                        }
                        break;
                    }
                case 0xD3:
                    Console.WriteLine($"Undefined opcode: 0x{opcode:X2} at address 0x{_registers.PC - 1:X4}");
                    cycles = 4;
                    break;
                case 0xD4:
                    {
                        ushort addr = _ram.ReadWord(_registers.PC);
                        _registers.PC += 2;
                        if (!_registers.IsFlagSet(Registers.CarryFlag))
                        {
                            _mmu.PushStack(_registers.PC);
                            _registers.PC = addr;
                            cycles = 24;
                        }
                        else
                        {
                            cycles = 12;
                        }
                        break;
                    }
                case 0xD5:
                    _mmu.PushStack(_registers.GetDE());
                    cycles = 16;
                    break;
                case 0xD6:
                    {
                        byte value = _ram.ReadByte(_registers.PC++);
                        _registers.SubtractFromA(value);
                        cycles = 8;
                        break;
                    }
                case 0xD7:
                    _mmu.PushStack(_registers.PC);
                    _registers.PC = 0x10;
                    cycles = 16;
                    break;
                case 0xD8:
                    if (_registers.IsFlagSet(Registers.CarryFlag))
                    {
                        _registers.PC = _mmu.PopStack();
                        cycles = 20;
                    }
                    else
                    {
                        cycles = 8;
                    }
                    break;
                case 0xDA:
                    {
                        ushort addr = _ram.ReadWord(_registers.PC);
                        _registers.PC += 2;
                        if (_registers.IsFlagSet(Registers.CarryFlag))
                        {
                            _registers.PC = addr;
                            cycles = 16;
                        }
                        else
                        {
                            cycles = 12;
                        }
                        break;
                    }
                case 0xDB:
                    Console.WriteLine($"Undefined opcode: 0x{opcode:X2} at address 0x{_registers.PC - 1:X4}");
                    cycles = 4;
                    break;
                case 0xDC:
                    {
                        ushort addr = _ram.ReadWord(_registers.PC);
                        _registers.PC += 2;
                        if (_registers.IsFlagSet(Registers.CarryFlag))
                        {
                            _mmu.PushStack(_registers.PC);
                            _registers.PC = addr;
                            cycles = 24;
                        }
                        else
                        {
                            cycles = 12;
                        }
                        break;
                    }
                case 0xDD:
                    Console.WriteLine($"Undefined opcode: 0x{opcode:X2} at address 0x{_registers.PC - 1:X4}");
                    cycles = 4;
                    break;
                case 0xDE:
                    {
                        byte value = _ram.ReadByte(_registers.PC++);
                        _registers.SbcFromA(value);
                        cycles = 8;
                        break;
                    }
                case 0xDF:
                    _mmu.PushStack(_registers.PC);
                    _registers.PC = 0x18;
                    cycles = 16;
                    break;
                case 0xE0:
                    {
                        byte addr = _ram.ReadByte(_registers.PC++);
                        _ram.WriteByte((ushort)(0xFF00 + addr), _registers.A);
                        cycles = 12;
                        break;
                    }
                case 0xE1:
                    _registers.SetHL(_mmu.PopStack());
                    cycles = 12;
                    break;
                case 0xE2:
                    _ram.WriteByte((ushort)(0xFF00 + _registers.C), _registers.A);
                    cycles = 8;
                    break;
                case 0xE3:
                    Console.WriteLine($"Undefined opcode: 0x{opcode:X2} at address 0x{_registers.PC - 1:X4}");
                    cycles = 4;
                    break;
                case 0xE4:
                    Console.WriteLine($"Undefined opcode: 0x{opcode:X2} at address 0x{_registers.PC - 1:X4}");
                    cycles = 4;
                    break;
                case 0xE5:
                    _mmu.PushStack(_registers.GetHL());
                    cycles = 16;
                    break;
                case 0xE6:
                    {
                        byte value = _ram.ReadByte(_registers.PC++);
                        _registers.AndWithA(value);
                        cycles = 8;
                        break;
                    }
                case 0xE7:
                    _mmu.PushStack(_registers.PC);
                    _registers.PC = 0x20;
                    cycles = 16;
                    break;
                case 0xE8:
                    {
                        sbyte offset = (sbyte)_ram.ReadByte(_registers.PC++);
                        int result = _registers.SP + offset;
                        _registers.ClearZeroFlag();
                        _registers.ClearNegativeFlag();
                        _registers.SetHalfCarryFlag(((_registers.SP & 0x0F) + (offset & 0x0F)) > 0x0F);
                        _registers.SetCarryFlag(((_registers.SP & 0xFF) + (offset & 0xFF)) > 0xFF);
                        _registers.SP = (ushort)result;
                        cycles = 16;
                        break;
                    }
                case 0xE9:
                    _registers.PC = _registers.GetHL();
                    cycles = 4;
                    break;
                case 0xEA:
                    {
                        ushort addr = _ram.ReadWord(_registers.PC);
                        _registers.PC += 2;
                        _ram.WriteByte(addr, _registers.A);
                        cycles = 16;
                        break;
                    }
                case 0xEB:
                    Console.WriteLine($"Undefined opcode: 0x{opcode:X2} at address 0x{_registers.PC - 1:X4}");
                    cycles = 4;
                    break;
                case 0xEC:
                    Console.WriteLine($"Undefined opcode: 0x{opcode:X2} at address 0x{_registers.PC - 1:X4}");
                    cycles = 4;
                    break;
                case 0xED:
                    Console.WriteLine($"Undefined opcode: 0x{opcode:X2} at address 0x{_registers.PC - 1:X4}");
                    cycles = 4;
                    break;
                case 0xEE:
                    {
                        byte value = _ram.ReadByte(_registers.PC++);
                        _registers.XorWithA(value);
                        cycles = 8;
                        break;
                    }
                case 0xEF:
                    _mmu.PushStack(_registers.PC);
                    _registers.PC = 0x28;
                    cycles = 16;
                    break;
                case 0xF0:
                    {
                        byte addr = _ram.ReadByte(_registers.PC++);
                        _registers.A = _ram.ReadByte((ushort)(0xFF00 + addr));
                        cycles = 12;
                        break;
                    }
                case 0xF1:
                    {
                        ushort value = _mmu.PopStack();
                        _registers.A = (byte)(value >> 8);
                        _registers.F = (byte)(value & 0xF0);
                        cycles = 12;
                        break;
                    }
                case 0xF2:
                    _registers.A = _ram.ReadByte((ushort)(0xFF00 + _registers.C));
                    cycles = 8;
                    break;
                case 0xF3:
                    _registers.IME = false;
                    cycles = 4;
                    break;
                case 0xF4:
                    Console.WriteLine($"Undefined opcode: 0x{opcode:X2} at address 0x{_registers.PC - 1:X4}");
                    cycles = 4;
                    break;
                case 0xF5:
                    _mmu.PushStack((ushort)((_registers.A << 8) | (_registers.F & 0xF0)));
                    cycles = 16;
                    break;
                case 0xF6:
                    {
                        byte value = _ram.ReadByte(_registers.PC++);
                        _registers.OrWithA(value);
                        cycles = 8;
                        break;
                    }
                case 0xF7:
                    _mmu.PushStack(_registers.PC);
                    _registers.PC = 0x30;
                    cycles = 16;
                    break;
                case 0xF8:
                    {
                        sbyte offset = (sbyte)_ram.ReadByte(_registers.PC++);
                        int result = _registers.SP + offset;
                        _registers.ClearZeroFlag();
                        _registers.ClearNegativeFlag();
                        _registers.SetHalfCarryFlag(((_registers.SP & 0x0F) + (offset & 0x0F)) > 0x0F);
                        _registers.SetCarryFlag(((_registers.SP & 0xFF) + (offset & 0xFF)) > 0xFF);
                        _registers.SetHL((ushort)result);
                        cycles = 12;
                        break;
                    }
                case 0xF9:
                    _registers.SP = _registers.GetHL();
                    cycles = 8;
                    break;
                case 0xFA:
                    {
                        ushort addr = _ram.ReadWord(_registers.PC);
                        _registers.PC += 2;
                        _registers.A = _ram.ReadByte(addr);
                        cycles = 16;
                        break;
                    }
                case 0xFB:
                    _registers.EI_Scheduled = true;
                    cycles = 4;
                    break;
                case 0xFC:
                    Console.WriteLine($"Undefined opcode: 0x{opcode:X2} at address 0x{_registers.PC - 1:X4}");
                    cycles = 4;
                    break;
                case 0xFD:
                    Console.WriteLine($"Undefined opcode: 0x{opcode:X2} at address 0x{_registers.PC - 1:X4}");
                    cycles = 4;
                    break;
                case 0xFE:
                    {
                        byte value = _ram.ReadByte(_registers.PC++);
                        _registers.CompareA(value);
                        cycles = 8;
                        break;
                    }
                case 0xFF:
                    _mmu.PushStack(_registers.PC);
                    _registers.PC = 0x38;
                    cycles = 16;
                    break;
                default:
                    Console.WriteLine($"Unimplemented opcode: 0x{opcode:X2} at address 0x{_registers.PC - 1:X4}");
                    Console.WriteLine($"Press any key to continue...");
                    Console.ReadKey();
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
                case 0x00:
                    _registers.B = RotateLeftWithCarry(_registers.B);
                    cycles = 8;
                    break;
                case 0x01:
                    _registers.C = RotateLeftWithCarry(_registers.C);
                    cycles = 8;
                    break;
                case 0x02:
                    _registers.D = RotateLeftWithCarry(_registers.D);
                    cycles = 8;
                    break;
                case 0x03:
                    _registers.E = RotateLeftWithCarry(_registers.E);
                    cycles = 8;
                    break;
                case 0x04:
                    _registers.H = RotateLeftWithCarry(_registers.H);
                    cycles = 8;
                    break;
                case 0x05:
                    _registers.L = RotateLeftWithCarry(_registers.L);
                    cycles = 8;
                    break;
                case 0x06:
                    address = _registers.GetHL();
                    value = _ram.ReadByte(address);
                    result = RotateLeftWithCarry(value);
                    _ram.WriteByte(address, result);
                    cycles = 16;
                    break;
                case 0x07:
                    _registers.A = RotateLeftWithCarry(_registers.A);
                    cycles = 8;
                    break;
                case 0x08:
                    _registers.B = RotateRightWithCarry(_registers.B);
                    cycles = 8;
                    break;
                case 0x09:
                    _registers.C = RotateRightWithCarry(_registers.C);
                    cycles = 8;
                    break;
                case 0x0A:
                    _registers.D = RotateRightWithCarry(_registers.D);
                    cycles = 8;
                    break;
                case 0x0B:
                    _registers.E = RotateRightWithCarry(_registers.E);
                    cycles = 8;
                    break;
                case 0x10:
                    _registers.B = RotateLeft(_registers.B);
                    cycles = 8;
                    break;
                case 0x11:
                    _registers.C = RotateLeft(_registers.C);
                    cycles = 8;
                    break;
                case 0x12:
                    _registers.D = RotateLeft(_registers.D);
                    cycles = 8;
                    break;
                case 0x13:
                    _registers.E = RotateLeft(_registers.E);
                    cycles = 8;
                    break;
                case 0x14:
                    _registers.H = RotateLeft(_registers.H);
                    cycles = 8;
                    break;
                case 0x15:
                    _registers.L = RotateLeft(_registers.L);
                    cycles = 8;
                    break;
                case 0x16:
                    address = _registers.GetHL();
                    value = _ram.ReadByte(address);
                    result = RotateLeft(value);
                    _ram.WriteByte(address, result);
                    cycles = 16;
                    break;
                case 0x17:
                    _registers.A = RotateLeft(_registers.A);
                    cycles = 8;
                    break;
                case 0x18:
                    _registers.B = RotateRight(_registers.B);
                    cycles = 8;
                    break;
                case 0x19:
                    _registers.C = RotateRight(_registers.C);
                    cycles = 8;
                    break;
                case 0x1A:
                    _registers.D = RotateRight(_registers.D);
                    cycles = 8;
                    break;
                case 0x1B:
                    _registers.E = RotateRight(_registers.E);
                    cycles = 8;
                    break;
                case 0x1C:
                    _registers.H = RotateRight(_registers.H);
                    cycles = 8;
                    break;
                case 0x1D:
                    _registers.L = RotateRight(_registers.L);
                    cycles = 8;
                    break;
                case 0x1E:
                    address = _registers.GetHL();
                    value = _ram.ReadByte(address);
                    result = RotateRight(value);
                    _ram.WriteByte(address, result);
                    cycles = 16;
                    break;
                case 0x1F:
                    _registers.A = RotateRight(_registers.A);
                    cycles = 8;
                    break;
                case 0x20:
                    _registers.B = ShiftLeftArithmetic(_registers.B);
                    cycles = 8;
                    break;
                case 0x21:
                    _registers.C = ShiftLeftArithmetic(_registers.C);
                    cycles = 8;
                    break;
                case 0x22:
                    _registers.D = ShiftLeftArithmetic(_registers.D);
                    cycles = 8;
                    break;
                case 0x23:
                    _registers.E = ShiftLeftArithmetic(_registers.E);
                    cycles = 8;
                    break;
                case 0x24:
                    _registers.H = ShiftLeftArithmetic(_registers.H);
                    cycles = 8;
                    break;
                case 0x25:
                    _registers.L = ShiftLeftArithmetic(_registers.L);
                    cycles = 8;
                    break;
                case 0x26:
                    address = _registers.GetHL();
                    value = _ram.ReadByte(address);
                    result = ShiftLeftArithmetic(value);
                    _ram.WriteByte(address, result);
                    cycles = 16;
                    break;
                case 0x27:
                    _registers.A = ShiftLeftArithmetic(_registers.A);
                    cycles = 8;
                    break;
                case 0x28:
                    _registers.B = ShiftRightArithmetic(_registers.B);
                    cycles = 8;
                    break;
                case 0x29:
                    _registers.C = ShiftRightArithmetic(_registers.C);
                    cycles = 8;
                    break;
                case 0x2A:
                    _registers.D = ShiftRightArithmetic(_registers.D);
                    cycles = 8;
                    break;
                case 0x2B:
                    _registers.E = ShiftRightArithmetic(_registers.E);
                    cycles = 8;
                    break;
                case 0x2C:
                    _registers.H = ShiftRightArithmetic(_registers.H);
                    cycles = 8;
                    break;
                case 0x2D:
                    _registers.L = ShiftRightArithmetic(_registers.L);
                    cycles = 8;
                    break;
                case 0x2E:
                    address = _registers.GetHL();
                    value = _ram.ReadByte(address);
                    result = ShiftRightArithmetic(value);
                    _ram.WriteByte(address, result);
                    cycles = 16;
                    break;
                case 0x2F:
                    _registers.A = ShiftRightArithmetic(_registers.A);
                    cycles = 8;
                    break;
                case 0x30:
                    _registers.B = SwapNibbles(_registers.B);
                    cycles = 8;
                    break;
                case 0x31:
                    _registers.C = SwapNibbles(_registers.C);
                    cycles = 8;
                    break;
                case 0x32:
                    _registers.D = SwapNibbles(_registers.D);
                    cycles = 8;
                    break;
                case 0x33:
                    _registers.E = SwapNibbles(_registers.E);
                    cycles = 8;
                    break;
                case 0x34:
                    _registers.H = SwapNibbles(_registers.H);
                    cycles = 8;
                    break;
                case 0x35:
                    _registers.L = SwapNibbles(_registers.L);
                    cycles = 8;
                    break;
                case 0x36:
                    address = _registers.GetHL();
                    value = _ram.ReadByte(address);
                    result = SwapNibbles(value);
                    _ram.WriteByte(address, result);
                    cycles = 16;
                    break;
                case 0x37:
                    _registers.A = SwapNibbles(_registers.A);
                    cycles = 8;
                    break;
                case 0x38:
                    _registers.B = ShiftRightLogical(_registers.B);
                    cycles = 8;
                    break;
                case 0x39:
                    _registers.C = ShiftRightLogical(_registers.C);
                    cycles = 8;
                    break;
                case 0x3A:
                    _registers.D = ShiftRightLogical(_registers.D);
                    cycles = 8;
                    break;
                case 0x3B:
                    _registers.E = ShiftRightLogical(_registers.E);
                    cycles = 8;
                    break;
                case 0x3C:
                    _registers.H = ShiftRightLogical(_registers.H);
                    cycles = 8;
                    break;
                case 0x3D:
                    _registers.L = ShiftRightLogical(_registers.L);
                    cycles = 8;
                    break;
                case 0x3E:
                    address = _registers.GetHL();
                    value = _ram.ReadByte(address);
                    result = ShiftRightLogical(value);
                    _ram.WriteByte(address, result);
                    cycles = 16;
                    break;
                case 0x3F:
                    _registers.A = ShiftRightLogical(_registers.A);
                    cycles = 8;
                    break;
                case 0x40:
                    TestBit(_registers.B, 0);
                    cycles = 8;
                    break;
                case 0x41:
                    TestBit(_registers.C, 0);
                    cycles = 8;
                    break;
                case 0x42:
                    TestBit(_registers.D, 0);
                    cycles = 8;
                    break;
                case 0x43:
                    TestBit(_registers.E, 0);
                    cycles = 8;
                    break;
                case 0x44:
                    TestBit(_registers.H, 0);
                    cycles = 8;
                    break;
                case 0x45:
                    TestBit(_registers.L, 0);
                    cycles = 8;
                    break;
                case 0x46:
                    value = _ram.ReadByte(_registers.GetHL());
                    TestBit(value, 0);
                    cycles = 12;
                    break;
                case 0x47:
                    TestBit(_registers.A, 0);
                    cycles = 8;
                    break;
                case 0x7F:
                    _registers.A = _registers.A;
                    cycles = 4;
                    break;
                case 0x80:
                    _registers.AddToA(_registers.B);
                    cycles = 4;
                    break;
                case 0x81:
                    _registers.AddToA(_registers.C);
                    cycles = 4;
                    break;
                case 0x82:
                    _registers.AddToA(_registers.D);
                    cycles = 4;
                    break;
                case 0x83:
                    _registers.AddToA(_registers.E);
                    cycles = 4;
                    break;
                case 0x84:
                    _registers.AddToA(_registers.H);
                    cycles = 4;
                    break;
                case 0x85:
                    _registers.AddToA(_registers.L);
                    cycles = 4;
                    break;
                case 0x86:
                    _registers.AddToA(_ram.ReadByte(_registers.GetHL()));
                    cycles = 8;
                    break;
                case 0x87:
                    _registers.AddToA(_registers.A);
                    cycles = 4;
                    break;
                case 0x88:
                    _registers.AdcToA(_registers.B);
                    cycles = 4;
                    break;
                case 0x89:
                    _registers.AdcToA(_registers.C);
                    cycles = 4;
                    break;
                case 0x8A:
                    _registers.AdcToA(_registers.D);
                    cycles = 4;
                    break;
                case 0x8B:
                    _registers.AdcToA(_registers.E);
                    cycles = 4;
                    break;
                case 0x8C:
                    _registers.AdcToA(_registers.H);
                    cycles = 4;
                    break;
                case 0x8D:
                    _registers.AdcToA(_registers.L);
                    cycles = 4;
                    break;
                case 0x8E:
                    _registers.AdcToA(_ram.ReadByte(_registers.GetHL()));
                    cycles = 8;
                    break;
                case 0x8F:
                    _registers.AdcToA(_registers.A);
                    cycles = 4;
                    break;
                case 0x90:
                    _registers.SubtractFromA(_registers.B);
                    cycles = 4;
                    break;
                case 0x91:
                    _registers.SubtractFromA(_registers.C);
                    cycles = 4;
                    break;
                case 0x92:
                    _registers.SubtractFromA(_registers.D);
                    cycles = 4;
                    break;
                case 0x93:
                    _registers.SubtractFromA(_registers.E);
                    cycles = 4;
                    break;
                case 0x94:
                    _registers.SubtractFromA(_registers.H);
                    cycles = 4;
                    break;
                case 0x95:
                    _registers.SubtractFromA(_registers.L);
                    cycles = 4;
                    break;
                case 0x96:
                    _registers.SubtractFromA(_ram.ReadByte(_registers.GetHL()));
                    cycles = 8;
                    break;
                case 0x97:
                    _registers.SubtractFromA(_registers.A);
                    cycles = 4;
                    break;
                case 0x98:
                    _registers.SbcFromA(_registers.B);
                    cycles = 4;
                    break;
                case 0x99:
                    _registers.SbcFromA(_registers.C);
                    cycles = 4;
                    break;
                case 0x9A:
                    _registers.SbcFromA(_registers.D);
                    cycles = 4;
                    break;
                case 0x9B:
                    _registers.SbcFromA(_registers.E);
                    cycles = 4;
                    break;
                case 0x9C:
                    _registers.SbcFromA(_registers.H);
                    cycles = 4;
                    break;
                case 0x9D:
                    _registers.SbcFromA(_registers.L);
                    cycles = 4;
                    break;
                case 0x9E:
                    _registers.SbcFromA(_ram.ReadByte(_registers.GetHL()));
                    cycles = 8;
                    break;
                case 0x9F:
                    _registers.SbcFromA(_registers.A);
                    cycles = 4;
                    break;
                case 0xA0:
                    _registers.AndWithA(_registers.B);
                    cycles = 4;
                    break;
                case 0xA1:
                    _registers.AndWithA(_registers.C);
                    cycles = 4;
                    break;
                case 0xA2:
                    _registers.AndWithA(_registers.D);
                    cycles = 4;
                    break;
                case 0xA3:
                    _registers.AndWithA(_registers.E);
                    cycles = 4;
                    break;
                case 0xA4:
                    _registers.AndWithA(_registers.H);
                    cycles = 4;
                    break;
                case 0xA5:
                    _registers.AndWithA(_registers.L);
                    cycles = 4;
                    break;
                case 0xA6:
                    _registers.AndWithA(_ram.ReadByte(_registers.GetHL()));
                    cycles = 8;
                    break;
                case 0xA7:
                    _registers.AndWithA(_registers.A);
                    cycles = 4;
                    break;
                case 0xA8:
                    _registers.XorWithA(_registers.B);
                    cycles = 4;
                    break;
                case 0xA9:
                    _registers.XorWithA(_registers.C);
                    cycles = 4;
                    break;
                case 0xAA:
                    _registers.XorWithA(_registers.D);
                    cycles = 4;
                    break;
                case 0xAB:
                    _registers.XorWithA(_registers.E);
                    cycles = 4;
                    break;
                case 0xAC:
                    _registers.XorWithA(_registers.H);
                    cycles = 4;
                    break;
                case 0xAD:
                    _registers.XorWithA(_registers.L);
                    cycles = 4;
                    break;
                case 0xAE:
                    _registers.XorWithA(_ram.ReadByte(_registers.GetHL()));
                    cycles = 8;
                    break;
                case 0xAF:
                    _registers.XorWithA(_registers.A);
                    cycles = 4;
                    break;
                case 0xB0:
                    _registers.OrWithA(_registers.B);
                    cycles = 4;
                    break;
                case 0xB1:
                    _registers.OrWithA(_registers.C);
                    cycles = 4;
                    break;
                case 0xB2:
                    _registers.OrWithA(_registers.D);
                    cycles = 4;
                    break;
                case 0xB3:
                    _registers.OrWithA(_registers.E);
                    cycles = 4;
                    break;
                case 0xB4:
                    _registers.OrWithA(_registers.H);
                    cycles = 4;
                    break;
                case 0xB5:
                    _registers.OrWithA(_registers.L);
                    cycles = 4;
                    break;
                case 0xB6:
                    _registers.OrWithA(_ram.ReadByte(_registers.GetHL()));
                    cycles = 8;
                    break;
                case 0xB7:
                    _registers.OrWithA(_registers.A);
                    cycles = 4;
                    break;
                case 0xB8:
                    _registers.CompareA(_registers.B);
                    cycles = 4;
                    break;
                case 0xB9:
                    _registers.CompareA(_registers.C);
                    cycles = 4;
                    break;
                case 0xBA:
                    _registers.CompareA(_registers.D);
                    cycles = 4;
                    break;
                case 0xBB:
                    _registers.CompareA(_registers.E);
                    cycles = 4;
                    break;
                case 0xBC:
                    _registers.CompareA(_registers.H);
                    cycles = 4;
                    break;
                case 0xBD:
                    _registers.CompareA(_registers.L);
                    cycles = 4;
                    break;
                case 0xBE:
                    _registers.CompareA(_ram.ReadByte(_registers.GetHL()));
                    cycles = 8;
                    break;
                case 0xBF:
                    _registers.CompareA(_registers.A);
                    cycles = 4;
                    break;
                case 0x50:
                    TestBit(_registers.B, 2);
                    cycles = 8;
                    break;
                case 0x58:
                    TestBit(_registers.B, 3);
                    cycles = 8;
                    break;
                case 0x70:
                    TestBit(_registers.B, 6);
                    cycles = 8;
                    break;
                case 0x78:
                    TestBit(_registers.B, 7);
                    cycles = 8;
                    break;
                case 0xFE:
                    address = _registers.GetHL();
                    value = _ram.ReadByte(address);
                    result = SetBit(value, 7);
                    _ram.WriteByte(address, result);
                    cycles = 16;
                    break;
                case 0x0C:
                    _registers.H = RotateRightWithCarry(_registers.H);
                    cycles = 8;
                    break;
                case 0x0D:
                    _registers.L = RotateRightWithCarry(_registers.L);
                    cycles = 8;
                    break;
                case 0x0E:
                    address = _registers.GetHL();
                    value = _ram.ReadByte(address);
                    result = RotateRightWithCarry(value);
                    _ram.WriteByte(address, result);
                    cycles = 16;
                    break;
                case 0x0F:
                    _registers.A = RotateRightWithCarry(_registers.A);
                    cycles = 8;
                    break;
                default:
                    Console.WriteLine($"Unimplemented CB opcode: 0x{opcode:X2} at address 0x{_registers.PC - 2:X4}");
                    Console.ReadKey();
                    cycles = 4;
                    break;
            }
            return cycles;
        }
        private byte RotateLeft(byte value)
        {
            byte carryIn = _registers.IsFlagSet(Registers.CarryFlag) ? (byte)1 : (byte)0;
            byte newCarry = (byte)((value & 0x80) >> 7);
            value = (byte)((value << 1) | carryIn);
            _registers.SetZeroFlag(value == 0);
            _registers.SetNegativeFlag(false);
            _registers.SetHalfCarryFlag(false);
            _registers.SetCarryFlag(newCarry != 0);
            return value;
        }
        private byte RotateRight(byte value)
        {
            byte carryIn = _registers.IsFlagSet(Registers.CarryFlag) ? (byte)0x80 : (byte)0;
            byte newCarry = (byte)(value & 0x01);
            value = (byte)((value >> 1) | carryIn);
            _registers.SetZeroFlag(value == 0);
            _registers.SetNegativeFlag(false);
            _registers.SetHalfCarryFlag(false);
            _registers.SetCarryFlag(newCarry != 0);
            return value;
        }
        private byte ShiftLeftArithmetic(byte value)
        {
            byte newCarry = (byte)((value & 0x80) >> 7);
            value <<= 1;
            _registers.SetZeroFlag(value == 0);
            _registers.SetNegativeFlag(false);
            _registers.SetHalfCarryFlag(false);
            _registers.SetCarryFlag(newCarry != 0);
            return value;
        }
        private byte ShiftRightArithmetic(byte value)
        {
            byte newCarry = (byte)(value & 0x01);
            byte msb = (byte)(value & 0x80); 
            value = (byte)((value >> 1) | msb);
            _registers.SetZeroFlag(value == 0);
            _registers.SetNegativeFlag(false);
            _registers.SetHalfCarryFlag(false);
            _registers.SetCarryFlag(newCarry != 0);
            return value;
        }
        private byte ShiftRightLogical(byte value)
        {
            byte newCarry = (byte)(value & 0x01);
            value >>= 1;
            _registers.SetZeroFlag(value == 0);
            _registers.SetNegativeFlag(false);
            _registers.SetHalfCarryFlag(false);
            _registers.SetCarryFlag(newCarry != 0);
            return value;
        }
        private byte SwapNibbles(byte value)
        {
            value = (byte)((value << 4) | (value >> 4));
            _registers.SetZeroFlag(value == 0);
            _registers.SetNegativeFlag(false);
            _registers.SetHalfCarryFlag(false);
            _registers.SetCarryFlag(false);
            return value;
        }
        private void TestBit(byte value, int bit)
        {
            bool bitSet = (value & (1 << bit)) != 0;
            _registers.SetZeroFlag(!bitSet);
            _registers.SetNegativeFlag(false);
            _registers.SetHalfCarryFlag(true);
        }
        private byte ResetBit(byte value, int bit)
        {
            value &= (byte)~(1 << bit);
            return value;
        }
        private byte SetBit(byte value, int bit)
        {
            value |= (byte)(1 << bit);
            return value;
        }
        private byte RotateLeftWithCarry(byte value)
        {
            byte carry = (byte)((value & 0x80) >> 7);
            value = (byte)((value << 1) | carry);
            _registers.SetZeroFlag(value == 0);
            _registers.SetNegativeFlag(false);
            _registers.SetHalfCarryFlag(false);
            _registers.SetCarryFlag(carry != 0);
            return value;
        }
        private byte RotateRightWithCarry(byte value)
        {
            byte carry = (byte)(value & 0x01);
            value = (byte)((value >> 1) | (carry << 7));
            _registers.SetZeroFlag(value == 0);
            _registers.SetNegativeFlag(false);
            _registers.SetHalfCarryFlag(false);
            _registers.SetCarryFlag(carry != 0);
            return value;
        }
    }
}
