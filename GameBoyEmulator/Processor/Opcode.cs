using GameBoyEmulator.Debugger;
using GameBoyEmulator.Memory;

namespace GameBoyEmulator.Processor
{
    internal class Opcode
    {
        Registers _registers;
        RAM _ram;

        private bool DebugMode = false;
        public Opcode(Registers registers, RAM ram) 
        {
            this._registers = registers;
            this._ram = ram;
            Logger.Log("Opcode initialized.");
        }

        public int ExecuteInstruction(byte opcode)
        {
            int cycles = 0;
            //if (DebugMode)
            //    Console.WriteLine($"Executing Opcode: 0x{opcode:X2} | _registers.PC: 0x{_registers.PC - 1:X4} | Registers: _registers.A=0x{_registers.A:X2} _registers.F=0x{_registers.F:X2} _registers.B=0x{_registers.B:X2} _registers.C=0x{_registers.C:X2} _registers.D=0x{_registers.D:X2} _registers.E=0x{_registers.E:X2} _registers.H=0x{_registers.H:X2} _registers.L=0x{_registers.L:X2} _registers.SP=0x{_registers.SP:X4}");
            switch (opcode)
            {
                case 0xC6:
                    {
                        byte zxc = _ram.ReadByte(_registers.PC++);
                        ushort rrz = (ushort)(_registers.A + zxc);
                        _registers.SetZeroFlag((rrz & 0xFF) == 0);
                        _registers.SetNegativeFlag(false);
                        _registers.SetHalfCarryFlag((_registers.A & 0xF) + (zxc & 0xF) > 0xF);
                        _registers.SetCarryFlag(rrz > 0xFF);
                        _registers.A = (byte)(rrz & 0xFF);
                        cycles = 8;
                        break;
                    }
                case 0x4B:
                    _registers.C = _registers.E;
                    cycles = 4;
                    break;
                case 0xDF:
                    _registers.SP -= 2;
                    _ram.WriteWord(_registers.SP, _registers.PC);
                    _registers.PC = 0x18;
                    cycles = 16;
                    break;
                case 0x4A:
                    _registers.C = _registers.D;
                    cycles = 4;
                    break;
                case 0x43:
                    _registers.B = _registers.E;
                    cycles = 4;
                    break;
                case 0xEE:
                    {
                        byte immediate = _ram.ReadByte(_registers.PC++);
                        _registers.A ^= immediate;
                        _registers.SetZeroFlag(_registers.A == 0);
                        _registers.SetNegativeFlag(false);
                        _registers.SetHalfCarryFlag(false);
                        _registers.SetCarryFlag(false);
                        cycles = 8;
                        break;
                    }
                case 0x96:
                    {
                        byte valueAtHL = _ram.ReadByte(_registers.GetHL());
                        int zzczczzx = _registers.A - valueAtHL;
                        _registers.SetZeroFlag((zzczczzx & 0xFF) == 0);
                        _registers.SetNegativeFlag(true);
                        _registers.SetHalfCarryFlag((_registers.A & 0xF) < (valueAtHL & 0xF));
                        _registers.SetCarryFlag(_registers.A < valueAtHL);
                        _registers.A = (byte)(zzczczzx & 0xFF);
                        cycles = 8;
                        break;
                    }
                case 0xD6:
                    {
                        byte zx = _ram.ReadByte(_registers.PC++);
                        ushort zczc = (ushort)(_registers.A - zx);
                        _registers.SetZeroFlag((zczc & 0xFF) == 0);
                        _registers.SetNegativeFlag(true);
                        _registers.SetHalfCarryFlag((_registers.A & 0xF) < (zx & 0xF));
                        _registers.SetCarryFlag(_registers.A < zx);
                        _registers.A = (byte)(zczc & 0xFF);
                        cycles = 8;
                        break;
                    }
                case 0x50:
                    _registers.D = _registers.B;
                    cycles = 4;
                    break;
                case 0x63:
                    _registers.H = _registers.E;
                    cycles = 4;
                    break;
                case 0x75:
                    ushort HL = (ushort)((_registers.H << 8) | _registers.L);
                    _ram.WriteByte(HL, _registers.L);
                    cycles = 8;
                    break;
                case 0x61:
                    _registers.H = _registers.C;
                    cycles = 4;
                    break;
                case 0x8C:
                    int temp = _registers.A + _registers.H + (_registers.IsFlagSet(_registers.CarryFlag) ? 1 : 0);
                    _registers.SetZeroFlag((temp & 0xFF) == 0);
                    _registers.SetNegativeFlag(false);
                    _registers.SetHalfCarryFlag((_registers.A & 0xF) + (_registers.H & 0xF) + (_registers.IsFlagSet(_registers.CarryFlag) ? 1 : 0) > 0xF);
                    _registers.SetCarryFlag(temp > 0xFF);
                    _registers.A = (byte)temp;
                    cycles = 4;
                    break;
                case 0x00:
                    cycles = 4;
                    break;
                case 0x01:
                    _registers.C = _ram.ReadByte(_registers.PC++);
                    _registers.B = _ram.ReadByte(_registers.PC++);
                    cycles = 12;
                    break;
                case 0x02:
                    _ram.WriteByte(_registers.GetBC(), _registers.A);
                    cycles = 8;
                    break;
                case 0xC1:
                    _registers.C = _ram.ReadByte(_registers.SP++);
                    _registers.B = _ram.ReadByte(_registers.SP++);
                    cycles = 12;
                    break;
                case 0xE5:
                    _registers.SP--;
                    _ram.WriteByte(_registers.SP, _registers.H);
                    _registers.SP--;
                    _ram.WriteByte(_registers.SP, _registers.L);
                    cycles = 16;
                    break;
                case 0xE1:
                    _registers.L = _ram.ReadByte(_registers.SP++);
                    _registers.H = _ram.ReadByte(_registers.SP++);
                    cycles = 12;
                    break;
                case 0xC5:
                    _registers.SP--;
                    _ram.WriteByte(_registers.SP, _registers.B);
                    _registers.SP--;
                    _ram.WriteByte(_registers.SP, _registers.C);
                    cycles = 16;
                    break;
                case 0xB1:
                    _registers.A |= _registers.C;
                    _registers.SetZeroFlag(_registers.A == 0);
                    _registers.ClearNegativeFlag();
                    _registers.ClearHalfCarryFlag();
                    _registers.ClearCarryFlag();
                    cycles = 4;
                    break;
                case 0x4F:
                    _registers.C = _registers.A;
                    cycles = 4;
                    break;
                case 0x57:
                    _registers.D = _registers.A;
                    cycles = 4;
                    break;
                case 0x5D:
                    _registers.E = _registers.L;
                    cycles = 4;
                    break;
                case 0x72:
                    _ram.WriteByte(_registers.GetHL(), _registers.D);
                    cycles = 8;
                    break;
                case 0x71:
                    _ram.WriteByte(_registers.GetHL(), _registers.C);
                    cycles = 8;
                    break;
                case 0xB2:
                    {
                        _registers.A |= _registers.D;
                        _registers.SetZeroFlag(_registers.A == 0);
                        _registers.ClearNegativeFlag();
                        _registers.ClearHalfCarryFlag();
                        _registers.ClearCarryFlag();
                        cycles = 4;
                        break;
                    }
                case 0xAB:
                    {
                        _registers.A ^= _registers.E;
                        _registers.SetZeroFlag(_registers.A == 0);
                        _registers.ClearNegativeFlag();
                        _registers.ClearHalfCarryFlag();
                        _registers.ClearCarryFlag();
                        cycles = 4;
                        break;
                    }
                case 0xBD:
                    {
                        int zzxczxczx = _registers.A - _registers.L;
                        _registers.SetZeroFlag((zzxczxczx & 0xFF) == 0);
                        _registers.SetNegativeFlag(true);
                        _registers.SetHalfCarryFlag((_registers.A & 0xF) < (_registers.L & 0xF));
                        _registers.SetCarryFlag(_registers.A < _registers.L);
                        cycles = 4;
                        break;
                    }
                case 0xBA:
                    {
                        int vvzvzv = _registers.A - _registers.D;
                        _registers.SetZeroFlag((vvzvzv & 0xFF) == 0);
                        _registers.SetNegativeFlag(true);
                        _registers.SetHalfCarryFlag((_registers.A & 0xF) < (_registers.D & 0xF));
                        _registers.SetCarryFlag(_registers.A < _registers.D);
                        cycles = 4;
                        break;
                    }
                case 0x93:
                    {
                        int vvzvvz = _registers.A - _registers.E;
                        _registers.SetZeroFlag((vvzvvz & 0xFF) == 0);
                        _registers.SetNegativeFlag(true);
                        _registers.SetHalfCarryFlag((_registers.A & 0xF) < (_registers.E & 0xF));
                        _registers.SetCarryFlag(_registers.A < _registers.E);
                        _registers.A = (byte)(vvzvvz & 0xFF);
                        cycles = 4;
                        break;
                    }
                case 0xC7:
                    {
                        _registers.SP -= 2;
                        _ram.WriteWord(_registers.SP, _registers.PC);
                        _registers.PC = 0x00;
                        cycles = 16;
                        break;
                    }
                case 0xA2:
                    {
                        _registers.A &= _registers.D;
                        _registers.SetZeroFlag(_registers.A == 0);
                        _registers.ClearNegativeFlag();
                        _registers.SetHalfCarryFlag(true);
                        _registers.ClearCarryFlag();
                        cycles = 4;
                        break;
                    }
                case 0x9B:
                    {
                        int zvvzx = _registers.IsFlagSet(_registers.CarryFlag) ? 1 : 0;
                        int cczxc = _registers.A - _registers.E - zvvzx;
                        _registers.SetZeroFlag((cczxc & 0xFF) == 0);
                        _registers.SetNegativeFlag(true);
                        _registers.SetHalfCarryFlag((_registers.A & 0xF) < ((_registers.E & 0xF) + zvvzx));
                        _registers.SetCarryFlag(_registers.A < _registers.E + zvvzx);
                        _registers.A = (byte)(cczxc & 0xFF);
                        cycles = 4;
                        break;
                    }
                case 0x5B:
                    {
                        cycles = 4;
                        break;
                    }
                case 0xB4:
                    {
                        _registers.A |= _registers.H;
                        _registers.SetZeroFlag(_registers.A == 0);
                        _registers.ClearNegativeFlag();
                        _registers.ClearHalfCarryFlag();
                        _registers.ClearCarryFlag();
                        cycles = 4;
                        break;
                    }
                case 0x81:
                    {
                        int zxczxzz = _registers.A + _registers.C;
                        _registers.SetZeroFlag((zxczxzz & 0xFF) == 0);
                        _registers.ClearNegativeFlag();
                        _registers.SetHalfCarryFlag((_registers.A & 0xF) + (_registers.C & 0xF) > 0xF);
                        _registers.SetCarryFlag(zxczxzz > 0xFF);
                        _registers.A = (byte)(zxczxzz & 0xFF);
                        cycles = 4;
                        break;
                    }
                case 0x98:
                    {
                        int zxzvxz = _registers.IsFlagSet(_registers.CarryFlag) ? 1 : 0;
                        int vzvzvz = _registers.A - _registers.B - zxzvxz;
                        _registers.SetZeroFlag((vzvzvz & 0xFF) == 0);
                        _registers.SetNegativeFlag(true);
                        _registers.SetHalfCarryFlag((_registers.A & 0xF) < ((_registers.B & 0xF) + zxzvxz));
                        _registers.SetCarryFlag(vzvzvz < 0);
                        _registers.A = (byte)(vzvzvz & 0xFF);
                        cycles = 4;
                        break;
                    }
                case 0xD2:
                    {
                        ushort nnnnn = _ram.ReadWord(_registers.PC);
                        _registers.PC += 2;
                        if (!_registers.IsFlagSet(_registers.CarryFlag))
                        {
                            _registers.PC = nnnnn;
                            cycles = 16;
                        }
                        else
                        {
                            cycles = 12;
                        }
                        break;
                    }
                case 0x8A:
                    {
                        int caaa = _registers.IsFlagSet(_registers.CarryFlag) ? 1 : 0;
                        int vvvv = _registers.A + _registers.D + caaa;
                        _registers.SetZeroFlag((vvvv & 0xFF) == 0);
                        _registers.ClearNegativeFlag();
                        _registers.SetHalfCarryFlag((_registers.A & 0xF) + (_registers.D & 0xF) + caaa > 0xF);
                        _registers.SetCarryFlag(vvvv > 0xFF);
                        _registers.A = (byte)(vvvv & 0xFF);
                        cycles = 4;
                        break;
                    }
                case 0x8B:
                    {
                        int bbg = _registers.IsFlagSet(_registers.CarryFlag) ? 1 : 0;
                        int bhb = _registers.A + _registers.E + bbg;
                        _registers.SetZeroFlag((bhb & 0xFF) == 0);
                        _registers.ClearNegativeFlag();
                        _registers.SetHalfCarryFlag((_registers.A & 0xF) + (_registers.E & 0xF) + bbg > 0xF);
                        _registers.SetCarryFlag(bhb > 0xFF);
                        _registers.A = (byte)(bhb & 0xFF);
                        cycles = 4;
                        break;
                    }
                case 0x7F:
                    {
                        cycles = 4;
                        break;
                    }
                case 0xFB:
                    _registers.EI_Scheduled = true;
                    cycles = 4;
                    if (DebugMode) Console.WriteLine("EI (Enable Interrupts) scheduled.");
                    break;
                case 0xD9:
                    _registers.PC = _ram.PopStack();
                    _registers.IME = true;
                    cycles = 16;
                    if (DebugMode) Console.WriteLine($"RETI executed. _registers.PC set to 0x{_registers.PC:X4} and _registers.IME set to true.");
                    break;
                case 0xF8:
                    {
                        sbyte zxczxzz = (sbyte)_ram.ReadByte(_registers.PC++);
                        ushort cczcczzzx = (ushort)(_registers.SP + zxczxzz);

                        // Set flags
                        _registers.SetZeroFlag(false); // The Z flag is always cleared
                        _registers.ClearNegativeFlag(); // The N flag is always cleared
                        _registers.SetHalfCarryFlag(((_registers.SP & 0xF) + (zxczxzz & 0xF)) > 0xF); // Check for half carry
                        _registers.SetCarryFlag(((_registers.SP & 0xFF) + (zxczxzz & 0xFF)) > 0xFF); // Check for full carry

                        // Store the result in HL
                        _registers.H = (byte)((cczcczzzx >> 8) & 0xFF);
                        _registers.L = (byte)(cczcczzzx & 0xFF);

                        cycles = 12;
                        break;
                    }

                case 0x10:
                    {
                        // STOP instruction, halts CPU until a button press occurs
                        _registers.PC++;
                        _registers.Halted = true;
                        cycles = 4;
                        break;
                    }
                case 0xE8:
                    {
                        int signedValue = (sbyte)_ram.ReadByte(_registers.PC++);
                        _registers.SetZeroFlag(false);
                        _registers.ClearNegativeFlag();
                        _registers.SetHalfCarryFlag((_registers.SP & 0xF) + (signedValue & 0xF) > 0xF);
                        _registers.SetCarryFlag((_registers.SP & 0xFF) + (signedValue & 0xFF) > 0xFF);
                        _registers.SP = (ushort)(_registers.SP + signedValue);
                        cycles = 16;
                        break;
                    }
                case 0x99:
                    {
                        int vvvavvvvv = _registers.A - _registers.C - (_registers.IsFlagSet(_registers.CarryFlag) ? 1 : 0);
                        _registers.SetZeroFlag((vvvavvvvv & 0xFF) == 0);
                        _registers.SetNegativeFlag(true);
                        _registers.SetHalfCarryFlag((_registers.A & 0xF) < (_registers.C & 0xF) + (_registers.IsFlagSet(_registers.CarryFlag) ? 1 : 0));
                        _registers.SetCarryFlag(vvvavvvvv < 0);
                        _registers.A = (byte)(vvvavvvvv & 0xFF);
                        cycles = 4;
                        break;
                    }

                case 0xC3:
                    {
                        ushort xxx = _ram.ReadWord(_registers.PC);
                        _registers.PC = xxx;
                        cycles = 16;
                        break;
                    }
                case 0x40:
                    {
                        cycles = 4;
                        break;
                    }
                case 0xEF:
                    {
                        _ram.PushStack(_registers.PC);
                        _registers.PC = 0x28;
                        cycles = 16;
                        break;
                    }
                case 0xFC:
                    {
                        cycles = 4;
                        break;
                    }
                case 0x4D:
                    {
                        _registers.PC = _ram.PopStack();
                        _registers.IME = true;
                        cycles = 16;
                        break;
                    }
                case 0x42:
                    {
                        _registers.B = _registers.D;
                        cycles = 4;
                        break;
                    }
                case 0xA4:
                    {
                        _registers.A &= _registers.H;
                        _registers.SetZeroFlag(_registers.A == 0);
                        _registers.SetNegativeFlag(false);
                        _registers.SetHalfCarryFlag(true);
                        _registers.SetCarryFlag(false);
                        cycles = 4;
                        break;
                    }
                case 0x54:
                    {
                        _registers.D = _registers.H;
                        cycles = 4;
                        break;
                    }
                case 0xF6:
                    {
                        byte d8 = _ram.ReadByte(_registers.PC++);
                        _registers.A |= d8;
                        _registers.SetZeroFlag(_registers.A == 0);
                        _registers.SetNegativeFlag(false);
                        _registers.SetHalfCarryFlag(false);
                        _registers.SetCarryFlag(false);
                        cycles = 8;
                        break;
                    }
                case 0xCC:
                    {
                        ushort xxxx = _ram.ReadWord(_registers.PC);
                        _registers.PC += 2;
                        if (_registers.IsFlagSet(_registers.ZeroFlag))
                        {
                            _ram.PushStack(_registers.PC);
                            _registers.PC = xxxx;
                            cycles = 24;
                        }
                        else
                        {
                            cycles = 12;
                        }
                        break;
                    }
                case 0x5E:
                    {
                        ushort x = (ushort)((_registers.H << 8) | _registers.L);
                        _registers.E = _ram.ReadByte(x);
                        cycles = 8;
                        break;
                    }
                case 0xE2:
                    {
                        _ram.WriteByte((ushort)(0xFF00 + _registers.C), _registers.A);
                        cycles = 8;
                        break;
                    }
                case 0xCB:
                    {
                        byte cbOpcode = _ram.ReadByte(_registers.PC++);
                        cycles = ExecuteCBInstruction(cbOpcode);
                        break;
                    }
                case 0xB0:
                    {
                        _registers.A |= _registers.B;
                        _registers.SetZeroFlag(_registers.A == 0);
                        _registers.SetNegativeFlag(false);
                        _registers.SetHalfCarryFlag(false);
                        _registers.SetCarryFlag(false);
                        cycles = 4;
                        break;
                    }
                case 0xA1:
                    {
                        _registers.A &= _registers.C;
                        _registers.SetZeroFlag(_registers.A == 0);
                        _registers.SetNegativeFlag(false);
                        _registers.SetHalfCarryFlag(true);
                        _registers.SetCarryFlag(false);
                        cycles = 4;
                        break;
                    }
                case 0x79:
                    {
                        _registers.A = _registers.C;
                        cycles = 4;
                        break;
                    }
                case 0x82:
                    {
                        int z = _registers.A + _registers.D;
                        _registers.SetZeroFlag((z & 0xFF) == 0);
                        _registers.SetNegativeFlag(false);
                        _registers.SetHalfCarryFlag(((_registers.A & 0xF) + (_registers.D & 0xF)) > 0xF);
                        _registers.SetCarryFlag(z > 0xFF);
                        _registers.A = (byte)(z & 0xFF);
                        cycles = 4;
                        break;
                    }
                case 0xDE:
                    {
                        byte d8 = _ram.ReadByte(_registers.PC++);
                        int cz = _registers.IsFlagSet(_registers.CarryFlag) ? 1 : 0;
                        int zz = _registers.A - d8 - cz;
                        _registers.SetZeroFlag((zz & 0xFF) == 0);
                        _registers.SetNegativeFlag(true);
                        _registers.SetHalfCarryFlag((_registers.A & 0xF) < ((d8 & 0xF) + cz));
                        _registers.SetCarryFlag(zz < 0);
                        _registers.A = (byte)(zz & 0xFF);
                        cycles = 8;
                        break;
                    }
                case 0xBF:
                    {
                        _registers.SetZeroFlag(true);
                        _registers.SetNegativeFlag(true);
                        _registers.SetHalfCarryFlag(false);
                        _registers.SetCarryFlag(false);
                        cycles = 4;
                        break;
                    }
                case 0x76:
                    {
                        _registers.Halted = true;
                        cycles = 4;
                        break;
                    }
                case 0xC2:
                    {
                        ushort zxczxczxc = _ram.ReadWord(_registers.PC);
                        _registers.PC += 2;
                        if (!_registers.IsFlagSet(_registers.ZeroFlag))
                        {
                            _registers.PC = zxczxczxc;
                            cycles = 16;
                        }
                        else
                        {
                            cycles = 12;
                        }
                        break;
                    }
                case 0x84:
                    {
                        int czxczxc = _registers.A + _registers.H;
                        _registers.SetZeroFlag((czxczxc & 0xFF) == 0);
                        _registers.SetNegativeFlag(false);
                        _registers.SetHalfCarryFlag((_registers.A & 0xF) + (_registers.H & 0xF) > 0xF);
                        _registers.SetCarryFlag(czxczxc > 0xFF);
                        _registers.A = (byte)(czxczxc & 0xFF);
                        cycles = 4;
                        break;
                    }
                case 0x69:
                    {
                        _registers.L = _registers.C;
                        cycles = 4;
                        break;
                    }
                case 0x60:
                    {
                        _registers.H = _registers.B;
                        cycles = 4;
                        break;
                    }
                case 0xB8:
                    {
                        int cccc = _registers.A - _registers.B;
                        _registers.SetZeroFlag((cccc & 0xFF) == 0);
                        _registers.SetNegativeFlag(true);
                        _registers.SetHalfCarryFlag((_registers.A & 0xF) < (_registers.B & 0xF));
                        _registers.SetCarryFlag(_registers.A < _registers.B);
                        cycles = 4;
                        break;
                    }
                case 0x44:
                    {
                        _registers.B = _registers.H;
                        cycles = 4;
                        break;
                    }
                case 0x62:
                    {
                        _registers.H = _registers.D;
                        cycles = 4;
                        break;
                    }
                case 0x9A:
                    {
                        int aaaaa = _registers.IsFlagSet(_registers.CarryFlag) ? 1 : 0;
                        int zxczxc = _registers.A - _registers.D - aaaaa;
                        _registers.SetZeroFlag((zxczxc & 0xFF) == 0);
                        _registers.SetNegativeFlag(true);
                        _registers.SetHalfCarryFlag((_registers.A & 0xF) < ((_registers.D & 0xF) + aaaaa));
                        _registers.SetCarryFlag(zxczxc < 0);
                        _registers.A = (byte)(zxczxc & 0xFF);
                        cycles = 4;
                        break;
                    }
                case 0x86:
                    {
                        byte zzczz = _ram.ReadByte(_registers.GetHL());
                        int vvvvv = _registers.A + zzczz;
                        _registers.SetZeroFlag((vvvvv & 0xFF) == 0);
                        _registers.SetNegativeFlag(false);
                        _registers.SetHalfCarryFlag((_registers.A & 0xF) + (zzczz & 0xF) > 0xF);
                        _registers.SetCarryFlag(vvvvv > 0xFF);
                        _registers.A = (byte)(vvvvv & 0xFF);
                        cycles = 8;
                        break;
                    }
                case 0x80:
                    {
                        int zxczzz = _registers.A + _registers.B;
                        _registers.SetZeroFlag((zxczzz & 0xFF) == 0);
                        _registers.SetNegativeFlag(false);
                        _registers.SetHalfCarryFlag((_registers.A & 0xF) + (_registers.B & 0xF) > 0xF);
                        _registers.SetCarryFlag(zxczzz > 0xFF);
                        _registers.A = (byte)(zxczzz & 0xFF);
                        cycles = 4;
                        break;
                    }
                case 0x92:
                    {
                        int zzzzcccccz = _registers.A - _registers.D;
                        _registers.SetZeroFlag((zzzzcccccz & 0xFF) == 0);
                        _registers.SetNegativeFlag(true);
                        _registers.SetHalfCarryFlag((_registers.A & 0xF) < (_registers.D & 0xF));
                        _registers.SetCarryFlag(_registers.A < _registers.D);
                        _registers.A = (byte)(zzzzcccccz & 0xFF);
                        cycles = 4;
                        break;
                    }
                case 0xE3:
                    {
                        cycles = 4;
                        break;
                    }
                case 0xD7:
                    {
                        _ram.PushStack(_registers.PC);
                        _registers.PC = 0x10;
                        cycles = 16;
                        break;
                    }
                case 0xA8:
                    {
                        _registers.A ^= _registers.B;
                        _registers.SetZeroFlag(_registers.A == 0);
                        _registers.SetNegativeFlag(false);
                        _registers.SetHalfCarryFlag(false);
                        _registers.SetCarryFlag(false);
                        cycles = 4;
                        break;
                    }
                case 0x6C:
                    {
                        _registers.L = _registers.H;
                        cycles = 4;
                        break;
                    }
                case 0x67:
                    {
                        _registers.H = _registers.A;
                        cycles = 4;
                        break;
                    }
                case 0xB3:
                    {
                        _registers.A |= _registers.E;
                        _registers.SetZeroFlag(_registers.A == 0);
                        _registers.SetNegativeFlag(false);
                        _registers.SetHalfCarryFlag(false);
                        _registers.SetCarryFlag(false);
                        cycles = 4;
                        break;
                    }
                case 0xBE:
                    {
                        byte zczzvzc = _ram.ReadByte(_registers.GetHL());
                        int zzzxzz = _registers.A - zczzvzc;
                        _registers.SetZeroFlag((zzzxzz & 0xFF) == 0);
                        _registers.SetNegativeFlag(true);
                        _registers.SetHalfCarryFlag((_registers.A & 0xF) < (zczzvzc & 0xF));
                        _registers.SetCarryFlag(_registers.A < zczzvzc);
                        cycles = 8;
                        break;
                    }
                case 0xCF:
                    {
                        _ram.PushStack(_registers.PC);
                        _registers.PC = 0x08;
                        cycles = 16;
                        break;
                    }
                case 0x6B:
                    {
                        _registers.L = _registers.E;
                        cycles = 4;
                        break;
                    }
                case 0xBB:
                    {
                        int vzvv = _registers.A - _registers.E;
                        _registers.SetZeroFlag((vzvv & 0xFF) == 0);
                        _registers.SetNegativeFlag(true);
                        _registers.SetHalfCarryFlag((_registers.A & 0xF) < (_registers.E & 0xF));
                        _registers.SetCarryFlag(_registers.A < _registers.E);
                        cycles = 4;
                        break;
                    }
                case 0x87:
                    {
                        int czzc = _registers.A + _registers.A;
                        _registers.SetZeroFlag((czzc & 0xFF) == 0);
                        _registers.SetNegativeFlag(false);
                        _registers.SetHalfCarryFlag((_registers.A & 0xF) + (_registers.A & 0xF) > 0xF);
                        _registers.SetCarryFlag(czzc > 0xFF);
                        _registers.A = (byte)(czzc & 0xFF);
                        cycles = 4;
                        break;
                    }
                case 0xE9:
                    {
                        _registers.PC = _registers.GetHL();
                        cycles = 4;
                        break;
                    }
                case 0x85:
                    {
                        int c = _registers.A + _registers.L;
                        _registers.SetZeroFlag((c & 0xFF) == 0);
                        _registers.SetNegativeFlag(false);
                        _registers.SetHalfCarryFlag(((_registers.A & 0xF) + (_registers.L & 0xF)) > 0xF);
                        _registers.SetCarryFlag(c > 0xFF);
                        _registers.A = (byte)(c & 0xFF);
                        cycles = 4;
                        break;
                    }
                case 0x91:
                    {
                        int z = _registers.A - _registers.C;
                        _registers.SetZeroFlag((z & 0xFF) == 0);
                        _registers.SetNegativeFlag(true);
                        _registers.SetHalfCarryFlag((_registers.A & 0xF) < (_registers.C & 0xF));
                        _registers.SetCarryFlag(z < 0);
                        _registers.A = (byte)(z & 0xFF);
                        cycles = 4;
                        break;
                    }
                case 0xC0:
                    {
                        if (!_registers.IsFlagSet(_registers.ZeroFlag))
                        {
                            _registers.PC = _ram.PopStack();
                            cycles = 20;
                        }
                        else
                        {
                            cycles = 8;
                        }
                        break;
                    }
                case 0x73:
                    {
                        _ram.WriteByte(_registers.GetHL(), _registers.E);
                        cycles = 8;
                        break;
                    }
                case 0x83:
                    {
                        _registers.AddToA(_registers.E);
                        cycles = 4;
                        break;
                    }
                case 0x88:
                    {
                        _registers.AdcToA(_registers.B);
                        cycles = 4;
                        break;
                    }
                case 0x89:
                    {
                        _registers.AdcToA(_registers.C);
                        cycles = 4;
                        break;
                    }
                case 0x6E:
                    {
                        _registers.L = _ram.ReadByte(_registers.GetHL());
                        cycles = 8;
                        break;
                    }
                case 0xCE:
                    byte value = _ram.ReadByte(_registers.PC++);
                    int carry = _registers.IsFlagSet(_registers.CarryFlag) ? 1 : 0;
                    int result = _registers.A + value + carry;
                    _registers.SetZeroFlag((result & 0xFF) == 0);
                    _registers.SetHalfCarryFlag(((_registers.A & 0xF) + (value & 0xF) + carry) > 0xF);
                    _registers.SetCarryFlag(result > 0xFF);
                    _registers.A = (byte)result;
                    cycles = 8;
                    break;
                case 0xC8:
                    if (DebugMode)
                    {
                        Console.WriteLine($"Executing RET Z | _registers.ZeroFlag: {_registers.IsFlagSet(_registers.ZeroFlag)} | _registers.SP=0x{_registers.SP:X4}");
                    }
                    if (_registers.IsFlagSet(_registers.ZeroFlag))
                    {
                        ushort returnAddress = _ram.PopStack();
                        if (DebugMode)
                        {
                            Console.WriteLine($"RET Z: Popped return address 0x{returnAddress:X4} from stack.");
                        }
                        _registers.PC = returnAddress;
                        cycles = 20;
                    }
                    else
                    {
                        if (DebugMode)
                        {
                            Console.WriteLine("RET Z: _registers.ZeroFlag not set, no return executed.");
                        }
                        cycles = 8;
                    }
                    break;
                case 0xD8:
                    if (_registers.IsFlagSet(_registers.CarryFlag))
                    {
                        _registers.PC = _ram.PopStack();
                        cycles = 20;
                    }
                    else
                    {
                        cycles = 8;
                    }
                    break;
                case 0xD5:
                    _registers.SP--;
                    _ram.WriteByte(_registers.SP, _registers.D);
                    _registers.SP--;
                    _ram.WriteByte(_registers.SP, _registers.E);
                    cycles = 16;
                    break;
                case 0x6F:
                    _registers.L = _registers.A;
                    cycles = 4;
                    break;
                case 0xAD:
                    _registers.A ^= _registers.L;
                    _registers.SetZeroFlag(_registers.A == 0);
                    _registers.ClearNegativeFlag();
                    _registers.ClearHalfCarryFlag();
                    _registers.ClearCarryFlag();
                    cycles = 4;
                    break;
                case 0xAE:
                    _registers.A ^= _ram.ReadByte(_registers.GetHL());
                    _registers.SetZeroFlag(_registers.A == 0);
                    _registers.ClearNegativeFlag();
                    _registers.ClearHalfCarryFlag();
                    _registers.ClearCarryFlag();
                    cycles = 8;
                    break;
                case 0xFA:
                    ushort address = (ushort)(_ram.ReadByte(_registers.PC++) | (_ram.ReadByte(_registers.PC++) << 8));
                    _registers.A = _ram.ReadByte(address);
                    cycles = 16;
                    break;
                case 0x7B:
                    _registers.A = _registers.E;
                    cycles = 4;
                    break;
                case 0xC4:
                    if (!_registers.IsFlagSet(_registers.ZeroFlag))
                    {
                        address = (ushort)(_ram.ReadByte(_registers.PC++) | (_ram.ReadByte(_registers.PC++) << 8));
                        _ram.PushStack(_registers.PC);
                        _registers.PC = address;
                        cycles = 24;
                    }
                    else
                    {
                        _registers.PC += 2;
                        cycles = 12;
                    }
                    break;
                case 0x8D:
                    carry = _registers.IsFlagSet(_registers.CarryFlag) ? 1 : 0;
                    result = _registers.A + _registers.L + carry;
                    _registers.SetZeroFlag((result & 0xFF) == 0);
                    _registers.SetHalfCarryFlag(((_registers.A & 0xF) + (_registers.L & 0xF) + carry) > 0xF);
                    _registers.SetCarryFlag(result > 0xFF);
                    _registers.A = (byte)result;
                    cycles = 8;
                    break;
                case 0x77:
                    _ram.WriteByte(_registers.GetHL(), _registers.A);
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
                    ushort addr = _ram.ReadWord(_registers.PC);
                    _registers.PC += 2;
                    _ram.WriteWord(addr, _registers.SP);
                    cycles = 20;
                    break;
                case 0x09:
                    ushort hl = _registers.GetHL();
                    ushort bc = _registers.GetBC();
                    result = hl + bc;
                    _registers.SetFlag(6, false);
                    _registers.SetHalfCarryFlag(((hl & 0x0FFF) + (bc & 0x0FFF)) > 0x0FFF);
                    _registers.SetCarryFlag(result > 0xFFFF);
                    _registers.SetHL((ushort)result);
                    cycles = 8;
                    break;
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
                case 0x11:
                    _registers.E = _ram.ReadByte(_registers.PC++);
                    _registers.D = _ram.ReadByte(_registers.PC++);
                    cycles = 12;
                    break;
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
                    sbyte offset = (sbyte)_ram.ReadByte(_registers.PC++);
                    _registers.PC = (ushort)(_registers.PC + offset);
                    cycles = 12;
                    break;
                case 0x19:
                    hl = _registers.GetHL();
                    ushort de = _registers.GetDE();
                    result = hl + de;
                    _registers.SetFlag(6, false);
                    _registers.SetHalfCarryFlag(((hl & 0x0FFF) + (de & 0x0FFF)) > 0x0FFF);
                    _registers.SetCarryFlag(result > 0xFFFF);
                    _registers.SetHL((ushort)result);
                    cycles = 8;
                    break;
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
                    sbyte vzv = (sbyte)_ram.ReadByte(_registers.PC++);
                    if (DebugMode)
                        Console.WriteLine($"JR NZ: _registers.PC=0x{_registers.PC:X4}, Offset={vzv}, _registers.ZeroFlag={_registers.IsFlagSet(_registers.ZeroFlag)}");
                    if (!_registers.IsFlagSet(_registers.ZeroFlag))
                    {
                        _registers.PC = (ushort)(_registers.PC + vzv);
                        if (DebugMode)
                            Console.WriteLine($"JR NZ: Jumping to _registers.PC=0x{_registers.PC:X4}");
                        cycles = 12;
                    }
                    else
                    {
                        if (DebugMode)
                            Console.WriteLine("JR NZ: No jump, _registers.ZeroFlag is set.");
                        cycles = 8;
                    }
                    break;
                case 0x21:
                    _registers.L = _ram.ReadByte(_registers.PC++);
                    _registers.H = _ram.ReadByte(_registers.PC++);
                    cycles = 12;
                    break;
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
                    offset = (sbyte)_ram.ReadByte(_registers.PC++);
                    if (_registers.IsFlagSet(_registers.ZeroFlag))
                    {
                        _registers.PC = (ushort)(_registers.PC + offset);
                        cycles = 12;
                    }
                    else
                    {
                        cycles = 8;
                    }
                    break;
                case 0x29:
                    hl = _registers.GetHL();
                    result = hl + hl;
                    _registers.SetFlag(6, false);
                    _registers.SetHalfCarryFlag(((hl & 0x0FFF) + (hl & 0x0FFF)) > 0x0FFF);
                    _registers.SetCarryFlag(result > 0xFFFF);
                    _registers.SetHL((ushort)result);
                    cycles = 8;
                    break;
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
                    offset = (sbyte)_ram.ReadByte(_registers.PC++);
                    if (!_registers.IsFlagSet(_registers.CarryFlag))
                    {
                        _registers.PC = (ushort)(_registers.PC + offset);
                        cycles = 12;
                    }
                    else
                    {
                        cycles = 8;
                    }
                    break;
                case 0x31:
                    _registers.SP = _ram.ReadWord(_registers.PC);
                    _registers.PC += 2;
                    cycles = 12;
                    break;
                case 0x32:
                    hl = _registers.GetHL();
                    _ram.WriteByte(hl, _registers.A);
                    _registers.SetHL((ushort)(hl - 1));
                    cycles = 8;
                    break;
                case 0x33:
                    _registers.SP++;
                    cycles = 8;
                    break;
                case 0x34:
                    byte val = _ram.ReadByte(_registers.GetHL());
                    val = _registers.Increment(val);
                    _ram.WriteByte(_registers.GetHL(), val);
                    cycles = 12;
                    break;
                case 0x35:
                    val = _ram.ReadByte(_registers.GetHL());
                    val = _registers.Decrement(val);
                    _ram.WriteByte(_registers.GetHL(), val);
                    cycles = 12;
                    break;
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
                    offset = (sbyte)_ram.ReadByte(_registers.PC++);
                    if (_registers.IsFlagSet(_registers.CarryFlag))
                    {
                        _registers.PC = (ushort)(_registers.PC + offset);
                        cycles = 12;
                    }
                    else
                    {
                        cycles = 8;
                    }
                    break;
                case 0x39:
                    hl = _registers.GetHL();
                    result = hl + _registers.SP;
                    _registers.SetFlag(6, false);
                    _registers.SetHalfCarryFlag(((hl & 0x0FFF) + (_registers.SP & 0x0FFF)) > 0x0FFF);
                    _registers.SetCarryFlag(result > 0xFFFF);
                    _registers.SetHL((ushort)result);
                    cycles = 8;
                    break;
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
                    _registers.SetCarryFlag(!_registers.IsFlagSet(_registers.CarryFlag));
                    _registers.SetFlag(6, false);
                    _registers.SetFlag(5, false);
                    cycles = 4;
                    break;
                case 0xAF:
                    _registers.A ^= _registers.A;
                    _registers.SetZeroFlag(_registers.A == 0);
                    _registers.ClearNegativeFlag();
                    _registers.ClearHalfCarryFlag();
                    _registers.ClearCarryFlag();
                    cycles = 4;
                    break;
                case 0xD0:  // RET NC
                    if (!_registers.IsFlagSet(_registers.CarryFlag))
                    {
                        _registers.PC = _ram.PopStack();  // Return to the address on top of the stack
                        cycles = 20;
                    }
                    else
                    {
                        cycles = 8;
                    }
                    break;

                case 0x41:
                    _registers.B = _registers.C;
                    cycles = 4;
                    break;
                case 0xB7:
                    _registers.A |= _registers.A;
                    _registers.SetZeroFlag(_registers.A == 0);
                    _registers.ClearNegativeFlag();
                    _registers.ClearHalfCarryFlag();
                    _registers.ClearCarryFlag();
                    cycles = 4;
                    break;
                case 0x4E:
                    _registers.C = _ram.ReadByte(_registers.GetHL());
                    cycles = 8;
                    break;
                case 0x70:
                    _ram.WriteByte(_registers.GetHL(), _registers.B);
                    cycles = 8;
                    break;
                case 0xD1:
                    _registers.E = _ram.ReadByte(_registers.SP++);
                    _registers.D = _ram.ReadByte(_registers.SP++);
                    cycles = 12;
                    break;
                case 0xF9:
                    _registers.SP = _registers.GetHL();
                    cycles = 8;
                    break;
                case 0x7A:
                    _registers.A = _registers.D;
                    cycles = 4;
                    break;
                case 0xE6:
                    value = _ram.ReadByte(_registers.PC++);
                    _registers.A &= value;
                    _registers.F = 0x20;
                    if (_registers.A == 0)
                        _registers.F |= 0x80;
                    cycles = 8;
                    break;
                case 0xF0:
                    byte a8 = _ram.ReadByte(_registers.PC++);
                    _registers.A = _ram.ReadByte((ushort)(0xFF00 + a8));
                    cycles = 12;
                    break;
                case 0xB6:
                    _registers.A |= _ram.ReadByte(_registers.GetHL());
                    _registers.SetZeroFlag(_registers.A == 0);
                    _registers.ClearNegativeFlag();
                    _registers.ClearHalfCarryFlag();
                    _registers.ClearCarryFlag();
                    cycles = 8;
                    break;
                case 0xFF:
                    _ram.PushStack(_registers.PC);
                    _registers.PC = 0x0038;
                    cycles = 16;
                    break;
                case 0xCD:
                    ushort ca = _ram.ReadWord(_registers.PC);
                    _registers.PC += 2;
                    _ram.PushStack(_registers.PC);
                    _registers.PC = ca;
                    cycles = 24;
                    break;
                case 0xC9:
                    _registers.PC = _ram.PopStack();
                    cycles = 16;
                    break;
                case 0xCA:
                    addr = _ram.ReadWord(_registers.PC);
                    _registers.PC += 2;
                    if (_registers.IsFlagSet(_registers.ZeroFlag))
                    {
                        _registers.PC = addr;
                        cycles = 16;
                    }
                    else
                    {
                        cycles = 12;
                    }
                    break;
                case 0x78:
                    _registers.A = _registers.B;
                    cycles = 4;
                    break;
                case 0xF5:
                    _registers.SP -= 2;
                    _ram.WriteWord(_registers.SP, (ushort)((_registers.A << 8) | (_registers.F & 0xF0)));
                    cycles = 16;
                    break;
                case 0xE0:
                    byte n = _ram.ReadByte(_registers.PC++);
                    _ram.WriteByte((ushort)(0xFF00 + n), _registers.A);
                    cycles = 12;
                    break;
                case 0xF1:
                    ushort af = _ram.ReadWord(_registers.SP);
                    _registers.SP += 2;
                    _registers.A = (byte)(af >> 8);
                    _registers.F = (byte)(af & 0xF0);
                    cycles = 12;
                    break;
                case 0x47:
                    _registers.B = _registers.A;
                    cycles = 4;
                    break;
                case 0xA7:
                    _registers.A &= _registers.A;
                    _registers.F = 0x20;
                    if (_registers.A == 0)
                        _registers.F |= 0x80;
                    cycles = 4;
                    break;
                case 0xA9:
                    _registers.A ^= _registers.C;
                    _registers.SetZeroFlag(_registers.A == 0);
                    _registers.ClearNegativeFlag();
                    _registers.ClearHalfCarryFlag();
                    _registers.ClearCarryFlag();
                    cycles = 4;
                    break;
                case 0x5F:
                    _registers.E = _registers.A;
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
                case 0x46:
                    _registers.B = _ram.ReadByte(_registers.GetHL());
                    cycles = 8;
                    break;
                case 0x7E:
                    _registers.A = _ram.ReadByte(_registers.GetHL());
                    cycles = 8;
                    break;
                case 0xA0:
                    _registers.A &= _registers.B;
                    _registers.F = 0x20;
                    if (_registers.A == 0)
                        _registers.F |= 0x80;
                    cycles = 4;
                    break;
                case 0x56:
                    _registers.D = _ram.ReadByte(_registers.GetHL());
                    cycles = 8;
                    break;
                case 0xF3:
                    _registers.IME = false;
                    cycles = 4;
                    break;
                case 0xFE:
                    {
                        byte d8 = _ram.ReadByte(_registers.PC++);
                        _registers.SetZeroFlag(_registers.A == d8);
                        _registers.SetNegativeFlag(true);
                        _registers.SetHalfCarryFlag((_registers.A & 0xF) < (d8 & 0xF));
                        _registers.SetCarryFlag(_registers.A < d8);
                        cycles = 8;
                        break;
                    }
                case 0x45:
                    _registers.B = _registers.L;
                    cycles = 4;
                    break;
                case 0x4C:
                    _registers.C = _registers.H;
                    cycles = 4;
                    break;
                case 0x48:
                    _registers.C = _registers.B;
                    cycles = 4;
                    break;
                case 0x90:
                    _registers.SetCarryFlag(_registers.A < _registers.B);
                    _registers.SetHalfCarryFlag((_registers.A & 0xF) < (_registers.B & 0xF));
                    _registers.A -= _registers.B;
                    _registers.SetZeroFlag(_registers.A == 0);
                    _registers.SetFlag(6, true);
                    cycles = 4;
                    break;
                case 0x66:
                    ushort cc = (ushort)((_registers.H << 8) | _registers.L);
                    _registers.H = _ram.ReadByte(cc);
                    cycles = 8;
                    break;
                case 0x94:
                    _registers.Subtract(_registers.H);
                    cycles = 4;
                    break;
                case 0xEA:
                    ushort k = _ram.ReadWord(_registers.PC);
                    _registers.PC += 2;
                    _ram.WriteByte(k, _registers.A);
                    cycles = 16;
                    break;
                case 0x59:
                    _registers.E = _registers.C;
                    cycles = 4;
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
            byte value = 0;
            byte result = 0;
            ushort address = 0;
            byte GetRegisterValue(int index)
            {
                switch (index)
                {
                    case 0: return _registers.B;
                    case 1: return _registers.C;
                    case 2: return _registers.D;
                    case 3: return _registers.E;
                    case 4: return _registers.H;
                    case 5: return _registers.L;
                    case 6: return _ram.ReadByte(_registers.GetHL());
                    case 7: return _registers.A;
                    default: return 0;
                }
            }
            void SetRegisterValue(int index, byte val)
            {
                switch (index)
                {
                    case 0: _registers.B = val; break;
                    case 1: _registers.C = val; break;
                    case 2: _registers.D = val; break;
                    case 3: _registers.E = val; break;
                    case 4: _registers.H = val; break;
                    case 5: _registers.L = val; break;
                    case 6: _ram.WriteByte(_registers.GetHL(), val); break;
                    case 7: _registers.A = val; break;
                }
            }
            int RegisterCycles(int index, int regCycles, int memCycles)
            {
                return index == 6 ? memCycles : regCycles;
            }
            int regIndex = opcode % 8;
            int opCodeGroup = opcode / 8;
            switch (opCodeGroup)
            {
                case 0x00:
                    value = GetRegisterValue(regIndex);
                    result = (byte)((value << 1) | (value >> 7));
                    _registers.SetZeroFlag(result == 0);
                    _registers.ClearNegativeFlag();
                    _registers.ClearHalfCarryFlag();
                    _registers.SetCarryFlag((value & 0x80) != 0);
                    SetRegisterValue(regIndex, result);
                    cycles = RegisterCycles(regIndex, 8, 16);
                    break;
                case 0x01:
                    value = GetRegisterValue(regIndex);
                    result = (byte)((value >> 1) | (value << 7));
                    _registers.SetZeroFlag(result == 0);
                    _registers.ClearNegativeFlag();
                    _registers.ClearHalfCarryFlag();
                    _registers.SetCarryFlag((value & 0x01) != 0);
                    SetRegisterValue(regIndex, result);
                    cycles = RegisterCycles(regIndex, 8, 16);
                    break;
                case 0x02:
                    value = GetRegisterValue(regIndex);
                    byte carryIn = _registers.IsFlagSet(_registers.CarryFlag) ? (byte)1 : (byte)0;
                    result = (byte)((value << 1) | carryIn);
                    _registers.SetZeroFlag(result == 0);
                    _registers.ClearNegativeFlag();
                    _registers.ClearHalfCarryFlag();
                    _registers.SetCarryFlag((value & 0x80) != 0);
                    SetRegisterValue(regIndex, result);
                    cycles = RegisterCycles(regIndex, 8, 16);
                    break;
                case 0x03:
                    value = GetRegisterValue(regIndex);
                    byte carryOut = (byte)(value & 0x01);
                    byte carryInRR = _registers.IsFlagSet(_registers.CarryFlag) ? (byte)0x80 : (byte)0;
                    result = (byte)((value >> 1) | carryInRR);
                    _registers.SetZeroFlag(result == 0);
                    _registers.ClearNegativeFlag();
                    _registers.ClearHalfCarryFlag();
                    _registers.SetCarryFlag(carryOut != 0);
                    SetRegisterValue(regIndex, result);
                    cycles = RegisterCycles(regIndex, 8, 16);
                    break;
                case 0x04:
                    value = GetRegisterValue(regIndex);
                    result = (byte)(value << 1);
                    _registers.SetZeroFlag(result == 0);
                    _registers.ClearNegativeFlag();
                    _registers.ClearHalfCarryFlag();
                    _registers.SetCarryFlag((value & 0x80) != 0);
                    SetRegisterValue(regIndex, result);
                    cycles = RegisterCycles(regIndex, 8, 16);
                    break;
                case 0x05:
                    value = GetRegisterValue(regIndex);
                    result = (byte)((value >> 1) | (value & 0x80));
                    _registers.SetZeroFlag(result == 0);
                    _registers.ClearNegativeFlag();
                    _registers.ClearHalfCarryFlag();
                    _registers.SetCarryFlag((value & 0x01) != 0);
                    SetRegisterValue(regIndex, result);
                    cycles = RegisterCycles(regIndex, 8, 16);
                    break;
                case 0x06:
                    value = GetRegisterValue(regIndex);
                    result = (byte)(((value & 0x0F) << 4) | ((value & 0xF0) >> 4));
                    _registers.SetZeroFlag(result == 0);
                    _registers.ClearNegativeFlag();
                    _registers.ClearHalfCarryFlag();
                    _registers.ClearCarryFlag();
                    SetRegisterValue(regIndex, result);
                    cycles = RegisterCycles(regIndex, 8, 16);
                    break;
                case 0x07:
                    value = GetRegisterValue(regIndex);
                    carryOut = (byte)(value & 0x01);
                    result = (byte)(value >> 1);
                    _registers.SetZeroFlag(result == 0);
                    _registers.ClearNegativeFlag();
                    _registers.ClearHalfCarryFlag();
                    _registers.SetCarryFlag(carryOut != 0);
                    SetRegisterValue(regIndex, result);
                    cycles = RegisterCycles(regIndex, 8, 16);
                    break;
                case int n when (n >= 0x08 && n <= 0x0F):
                    int bit = (opCodeGroup - 0x08);
                    value = GetRegisterValue(regIndex);
                    _registers.SetZeroFlag((value & (1 << bit)) == 0);
                    _registers.ClearNegativeFlag();
                    _registers.SetHalfCarryFlag(true);
                    cycles = RegisterCycles(regIndex, 8, 12);
                    break;
                case int n when (n >= 0x10 && n <= 0x17):
                    bit = (opCodeGroup - 0x10);
                    value = GetRegisterValue(regIndex);
                    result = (byte)(value & ~(1 << bit));
                    SetRegisterValue(regIndex, result);
                    cycles = RegisterCycles(regIndex, 8, 16);
                    break;
                case int n when (n >= 0x18 && n <= 0x1F):
                    bit = (opCodeGroup - 0x18);
                    value = GetRegisterValue(regIndex);
                    result = (byte)(value | (1 << bit));
                    SetRegisterValue(regIndex, result);
                    cycles = RegisterCycles(regIndex, 8, 16);
                    break;
                default:
                    Console.WriteLine($"Unimplemented CB opcode: 0x{opcode:X2} at address 0x{_registers.PC - 2:X4}");
                    cycles = 4;
                    break;
            }
            return cycles;
        }

    }
}
