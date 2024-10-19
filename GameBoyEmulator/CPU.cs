namespace GameBoyEmulator
{
    public class CPU
    {
        public byte A, F, B, C, D, E, H, L;
        public ushort PC, SP;
        public bool IME;
        public bool Halted;
        private bool DebugMode = false;
        private bool DebugModeFlag = false;
        private Memory memory;
        private bool EI_Scheduled = false;
        public CPU(Memory memory)
        {
            this.memory = memory;
            Reset();
        }
        public void Reset()
        {
            PC = 0x0100;
            SP = 0xFFFE;
            A = 0x01;
            F = 0xB0;
            B = 0x00;
            C = 0x13;
            D = 0x00;
            E = 0xD8;
            H = 0x01;
            L = 0x4D;
            IME = true;
        }
        public void HandleInterrupts()
        {
            if (!IME)
                return;
            byte interruptEnable = memory.ReadByte(0xFFFF);
            byte interruptFlags = memory.ReadByte(0xFF0F);
            byte enabledInterrupts = (byte)(interruptEnable & interruptFlags);
            if (enabledInterrupts == 0)
                return;
            IME = false;
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
            memory.WriteByte(0xFF0F, interruptFlags);
            PushStack(PC);
            PC = interruptAddress;
            if (DebugMode) Console.WriteLine($"Handling Interrupt: 0x{interruptBit:X2} at address 0x{interruptAddress:X4}");
        }
        public int Step()
        {
            HandleInterrupts();
            byte opcode = memory.ReadByte(PC);
            PC++;
            int cycles = ExecuteInstruction(opcode);
            if (EI_Scheduled)
            {
                IME = true;
                EI_Scheduled = false;
                if (DebugMode) Console.WriteLine("Interrupt Master Enable (IME) set to true.");
            }
            return cycles;
        }
        private int ExecuteInstruction(byte opcode)
        {
            int cycles = 0;
            if (DebugMode)
                Console.WriteLine($"Executing Opcode: 0x{opcode:X2} | PC: 0x{PC - 1:X4} | Registers: A=0x{A:X2} F=0x{F:X2} B=0x{B:X2} C=0x{C:X2} D=0x{D:X2} E=0x{E:X2} H=0x{H:X2} L=0x{L:X2} SP=0x{SP:X4}");
            switch (opcode)
            {
                case 0xC6:
                    {
                        byte zxc = memory.ReadByte(PC++);
                        ushort rrz = (ushort)(A + zxc);
                        SetZeroFlag((rrz & 0xFF) == 0);
                        SetNegativeFlag(false);
                        SetHalfCarryFlag((A & 0xF) + (zxc & 0xF) > 0xF);
                        SetCarryFlag(rrz > 0xFF);
                        A = (byte)(rrz & 0xFF);
                        cycles = 8;
                        break;
                    }
                case 0x4B:
                    C = E;
                    cycles = 4;
                    break;
                case 0xDF:
                    SP -= 2;
                    memory.WriteWord(SP, PC);
                    PC = 0x18;
                    cycles = 16;
                    break;
                case 0x4A:
                    C = D;
                    cycles = 4;
                    break;
                case 0x43:
                    B = E;
                    cycles = 4;
                    break;
                case 0xEE:
                    {
                        byte immediate = memory.ReadByte(PC++);
                        A ^= immediate;
                        SetZeroFlag(A == 0);
                        SetNegativeFlag(false);
                        SetHalfCarryFlag(false);
                        SetCarryFlag(false);
                        cycles = 8;
                        break;
                    }
                case 0x96:
                    {
                        byte valueAtHL = memory.ReadByte(GetHL());
                        int zzczczzx = A - valueAtHL;
                        SetZeroFlag((zzczczzx & 0xFF) == 0);
                        SetNegativeFlag(true);
                        SetHalfCarryFlag((A & 0xF) < (valueAtHL & 0xF));
                        SetCarryFlag(A < valueAtHL);
                        A = (byte)(zzczczzx & 0xFF);
                        cycles = 8;
                        break;
                    }
                case 0xD6:
                    {
                        byte zx = memory.ReadByte(PC++);
                        ushort zczc = (ushort)(A - zx);
                        SetZeroFlag((zczc & 0xFF) == 0);
                        SetNegativeFlag(true);
                        SetHalfCarryFlag((A & 0xF) < (zx & 0xF));
                        SetCarryFlag(A < zx);
                        A = (byte)(zczc & 0xFF);
                        cycles = 8;
                        break;
                    }
                case 0x50:
                    D = B;
                    cycles = 4;
                    break;
                case 0x63:
                    H = E;
                    cycles = 4;
                    break;
                case 0x75:
                    ushort HL = (ushort)((H << 8) | L);
                    memory.WriteByte(HL, L);
                    cycles = 8;
                    break;
                case 0x61:
                    H = C;
                    cycles = 4;
                    break;
                case 0x8C:
                    int temp = A + H + (IsFlagSet(CarryFlag) ? 1 : 0);
                    SetZeroFlag((temp & 0xFF) == 0);
                    SetNegativeFlag(false);
                    SetHalfCarryFlag((A & 0xF) + (H & 0xF) + (IsFlagSet(CarryFlag) ? 1 : 0) > 0xF);
                    SetCarryFlag(temp > 0xFF);
                    A = (byte)temp;
                    cycles = 4;
                    break;
                case 0x00:
                    cycles = 4;
                    break;
                case 0x01:
                    C = memory.ReadByte(PC++);
                    B = memory.ReadByte(PC++);
                    cycles = 12;
                    break;
                case 0x02:
                    memory.WriteByte(GetBC(), A);
                    cycles = 8;
                    break;
                case 0xC1:
                    C = memory.ReadByte(SP++);
                    B = memory.ReadByte(SP++);
                    cycles = 12;
                    break;
                case 0xE5:
                    SP--;
                    memory.WriteByte(SP, H);
                    SP--;
                    memory.WriteByte(SP, L);
                    cycles = 16;
                    break;
                case 0xE1:
                    L = memory.ReadByte(SP++);
                    H = memory.ReadByte(SP++);
                    cycles = 12;
                    break;
                case 0xC5:
                    SP--;
                    memory.WriteByte(SP, B);
                    SP--;
                    memory.WriteByte(SP, C);
                    cycles = 16;
                    break;
                case 0xB1:
                    A |= C;
                    SetZeroFlag(A == 0);
                    ClearNegativeFlag();
                    ClearHalfCarryFlag();
                    ClearCarryFlag();
                    cycles = 4;
                    break;
                case 0x4F:
                    C = A;
                    cycles = 4;
                    break;
                case 0x57:
                    D = A;
                    cycles = 4;
                    break;
                case 0x5D:
                    E = L;
                    cycles = 4;
                    break;
                case 0x72:
                    memory.WriteByte(GetHL(), D);
                    cycles = 8;
                    break;
                case 0x71:
                    memory.WriteByte(GetHL(), C);
                    cycles = 8;
                    break;
                case 0xB2:
                    {
                        A |= D;
                        SetZeroFlag(A == 0);
                        ClearNegativeFlag();
                        ClearHalfCarryFlag();
                        ClearCarryFlag();
                        cycles = 4;
                        break;
                    }
                case 0xAB:
                    {
                        A ^= E;
                        SetZeroFlag(A == 0);
                        ClearNegativeFlag();
                        ClearHalfCarryFlag();
                        ClearCarryFlag();
                        cycles = 4;
                        break;
                    }
                case 0xBD:
                    {
                        int zzxczxczx = A - L;
                        SetZeroFlag((zzxczxczx & 0xFF) == 0);
                        SetNegativeFlag(true);
                        SetHalfCarryFlag((A & 0xF) < (L & 0xF));
                        SetCarryFlag(A < L);
                        cycles = 4;
                        break;
                    }
                case 0xBA:
                    {
                        int vvzvzv = A - D;
                        SetZeroFlag((vvzvzv & 0xFF) == 0);
                        SetNegativeFlag(true);
                        SetHalfCarryFlag((A & 0xF) < (D & 0xF));
                        SetCarryFlag(A < D);
                        cycles = 4;
                        break;
                    }
                case 0x93:
                    {
                        int vvzvvz = A - E;
                        SetZeroFlag((vvzvvz & 0xFF) == 0);
                        SetNegativeFlag(true);
                        SetHalfCarryFlag((A & 0xF) < (E & 0xF));
                        SetCarryFlag(A < E);
                        A = (byte)(vvzvvz & 0xFF);
                        cycles = 4;
                        break;
                    }
                case 0xC7:
                    {
                        SP -= 2;
                        memory.WriteWord(SP, PC);
                        PC = 0x00;
                        cycles = 16;
                        break;
                    }
                case 0xA2:
                    {
                        A &= D;
                        SetZeroFlag(A == 0);
                        ClearNegativeFlag();
                        SetHalfCarryFlag(true);
                        ClearCarryFlag();
                        cycles = 4;
                        break;
                    }
                case 0x9B:
                    {
                        int zvvzx = IsFlagSet(CarryFlag) ? 1 : 0;
                        int cczxc = A - E - zvvzx;
                        SetZeroFlag((cczxc & 0xFF) == 0);
                        SetNegativeFlag(true);
                        SetHalfCarryFlag((A & 0xF) < ((E & 0xF) + zvvzx));
                        SetCarryFlag(A < E + zvvzx);
                        A = (byte)(cczxc & 0xFF);
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
                        A |= H;
                        SetZeroFlag(A == 0);
                        ClearNegativeFlag();
                        ClearHalfCarryFlag();
                        ClearCarryFlag();
                        cycles = 4;
                        break;
                    }
                case 0x81:
                    {
                        int zxczxzz = A + C;
                        SetZeroFlag((zxczxzz & 0xFF) == 0);
                        ClearNegativeFlag();
                        SetHalfCarryFlag((A & 0xF) + (C & 0xF) > 0xF);
                        SetCarryFlag(zxczxzz > 0xFF);
                        A = (byte)(zxczxzz & 0xFF);
                        cycles = 4;
                        break;
                    }
                case 0x98:
                    {
                        int zxzvxz = IsFlagSet(CarryFlag) ? 1 : 0;
                        int vzvzvz = A - B - zxzvxz;
                        SetZeroFlag((vzvzvz & 0xFF) == 0);
                        SetNegativeFlag(true);
                        SetHalfCarryFlag((A & 0xF) < ((B & 0xF) + zxzvxz));
                        SetCarryFlag(vzvzvz < 0);
                        A = (byte)(vzvzvz & 0xFF);
                        cycles = 4;
                        break;
                    }
                case 0xD2:
                    {
                        ushort nnnnn = memory.ReadWord(PC);
                        PC += 2;
                        if (!IsFlagSet(CarryFlag))
                        {
                            PC = nnnnn;
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
                        int caaa = IsFlagSet(CarryFlag) ? 1 : 0;
                        int vvvv = A + D + caaa;
                        SetZeroFlag((vvvv & 0xFF) == 0);
                        ClearNegativeFlag();
                        SetHalfCarryFlag((A & 0xF) + (D & 0xF) + caaa > 0xF);
                        SetCarryFlag(vvvv > 0xFF);
                        A = (byte)(vvvv & 0xFF);
                        cycles = 4;
                        break;
                    }
                case 0x8B:
                    {
                        int bbg = IsFlagSet(CarryFlag) ? 1 : 0;
                        int bhb = A + E + bbg;
                        SetZeroFlag((bhb & 0xFF) == 0);
                        ClearNegativeFlag();
                        SetHalfCarryFlag((A & 0xF) + (E & 0xF) + bbg > 0xF);
                        SetCarryFlag(bhb > 0xFF);
                        A = (byte)(bhb & 0xFF);
                        cycles = 4;
                        break;
                    }
                case 0x7F:
                    {
                        cycles = 4;
                        break;
                    }
                case 0xFB:
                    EI_Scheduled = true;
                    cycles = 4;
                    if (DebugMode) Console.WriteLine("EI (Enable Interrupts) scheduled.");
                    break;
                case 0xD9:
                    PC = PopStack();
                    IME = true;
                    cycles = 16;
                    if (DebugMode) Console.WriteLine($"RETI executed. PC set to 0x{PC:X4} and IME set to true.");
                    break;
                case 0xC3:
                    {
                        ushort xxx = memory.ReadWord(PC);
                        PC = xxx;
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
                        PushStack(PC);
                        PC = 0x28;
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
                        PC = PopStack();
                        IME = true;
                        cycles = 16;
                        break;
                    }
                case 0x42:
                    {
                        B = D;
                        cycles = 4;
                        break;
                    }
                case 0xA4:
                    {
                        A &= H;
                        SetZeroFlag(A == 0);
                        SetNegativeFlag(false);
                        SetHalfCarryFlag(true);
                        SetCarryFlag(false);
                        cycles = 4;
                        break;
                    }
                case 0x54:
                    {
                        D = H;
                        cycles = 4;
                        break;
                    }
                case 0xF6:
                    {
                        byte d8 = memory.ReadByte(PC++);
                        A |= d8;
                        SetZeroFlag(A == 0);
                        SetNegativeFlag(false);
                        SetHalfCarryFlag(false);
                        SetCarryFlag(false);
                        cycles = 8;
                        break;
                    }
                case 0xCC:
                    {
                        ushort xxxx = memory.ReadWord(PC);
                        PC += 2;
                        if (IsFlagSet(ZeroFlag))
                        {
                            PushStack(PC);
                            PC = xxxx;
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
                        ushort x = (ushort)((H << 8) | L);
                        E = memory.ReadByte(x);
                        cycles = 8;
                        break;
                    }
                case 0xE2:
                    {
                        memory.WriteByte((ushort)(0xFF00 + C), A);
                        cycles = 8;
                        break;
                    }
                case 0xCB:
                    {
                        byte cbOpcode = memory.ReadByte(PC++);
                        cycles = ExecuteCBInstruction(cbOpcode);
                        break;
                    }
                case 0xB0:
                    {
                        A |= B;
                        SetZeroFlag(A == 0);
                        SetNegativeFlag(false);
                        SetHalfCarryFlag(false);
                        SetCarryFlag(false);
                        cycles = 4;
                        break;
                    }
                case 0xA1:
                    {
                        A &= C;
                        SetZeroFlag(A == 0);
                        SetNegativeFlag(false);
                        SetHalfCarryFlag(true);
                        SetCarryFlag(false);
                        cycles = 4;
                        break;
                    }
                case 0x79:
                    {
                        A = C;
                        cycles = 4;
                        break;
                    }
                case 0x82:
                    {
                        int z = A + D;
                        SetZeroFlag((z & 0xFF) == 0);
                        SetNegativeFlag(false);
                        SetHalfCarryFlag(((A & 0xF) + (D & 0xF)) > 0xF);
                        SetCarryFlag(z > 0xFF);
                        A = (byte)(z & 0xFF);
                        cycles = 4;
                        break;
                    }
                case 0xDE:
                    {
                        byte d8 = memory.ReadByte(PC++);
                        int cz = IsFlagSet(CarryFlag) ? 1 : 0;
                        int zz = A - d8 - cz;
                        SetZeroFlag((zz & 0xFF) == 0);
                        SetNegativeFlag(true);
                        SetHalfCarryFlag((A & 0xF) < ((d8 & 0xF) + cz));
                        SetCarryFlag(zz < 0);
                        A = (byte)(zz & 0xFF);
                        cycles = 8;
                        break;
                    }
                case 0xBF:
                    {
                        SetZeroFlag(true);
                        SetNegativeFlag(true);
                        SetHalfCarryFlag(false);
                        SetCarryFlag(false);
                        cycles = 4;
                        break;
                    }
                case 0x76:
                    {
                        Halted = true;
                        cycles = 4;
                        break;
                    }
                case 0xC2:
                    {
                        ushort zxczxczxc = memory.ReadWord(PC);
                        PC += 2;
                        if (!IsFlagSet(ZeroFlag))
                        {
                            PC = zxczxczxc;
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
                        int czxczxc = A + H;
                        SetZeroFlag((czxczxc & 0xFF) == 0);
                        SetNegativeFlag(false);
                        SetHalfCarryFlag((A & 0xF) + (H & 0xF) > 0xF);
                        SetCarryFlag(czxczxc > 0xFF);
                        A = (byte)(czxczxc & 0xFF);
                        cycles = 4;
                        break;
                    }
                case 0x69:
                    {
                        L = C;
                        cycles = 4;
                        break;
                    }
                case 0x60:
                    {
                        H = B;
                        cycles = 4;
                        break;
                    }
                case 0xB8:
                    {
                        int cccc = A - B;
                        SetZeroFlag((cccc & 0xFF) == 0);
                        SetNegativeFlag(true);
                        SetHalfCarryFlag((A & 0xF) < (B & 0xF));
                        SetCarryFlag(A < B);
                        cycles = 4;
                        break;
                    }
                case 0x44:
                    {
                        B = H;
                        cycles = 4;
                        break;
                    }
                case 0x62:
                    {
                        H = D;
                        cycles = 4;
                        break;
                    }
                case 0x9A:
                    {
                        int aaaaa = IsFlagSet(CarryFlag) ? 1 : 0;
                        int zxczxc = A - D - aaaaa;
                        SetZeroFlag((zxczxc & 0xFF) == 0);
                        SetNegativeFlag(true);
                        SetHalfCarryFlag((A & 0xF) < ((D & 0xF) + aaaaa));
                        SetCarryFlag(zxczxc < 0);
                        A = (byte)(zxczxc & 0xFF);
                        cycles = 4;
                        break;
                    }
                case 0x86:
                    {
                        byte zzczz = memory.ReadByte(GetHL());
                        int vvvvv = A + zzczz;
                        SetZeroFlag((vvvvv & 0xFF) == 0);
                        SetNegativeFlag(false);
                        SetHalfCarryFlag((A & 0xF) + (zzczz & 0xF) > 0xF);
                        SetCarryFlag(vvvvv > 0xFF);
                        A = (byte)(vvvvv & 0xFF);
                        cycles = 8;
                        break;
                    }
                case 0x80:
                    {
                        int zxczzz = A + B;
                        SetZeroFlag((zxczzz & 0xFF) == 0);
                        SetNegativeFlag(false);
                        SetHalfCarryFlag((A & 0xF) + (B & 0xF) > 0xF);
                        SetCarryFlag(zxczzz > 0xFF);
                        A = (byte)(zxczzz & 0xFF);
                        cycles = 4;
                        break;
                    }
                case 0x92:
                    {
                        int zzzzcccccz = A - D;
                        SetZeroFlag((zzzzcccccz & 0xFF) == 0);
                        SetNegativeFlag(true);
                        SetHalfCarryFlag((A & 0xF) < (D & 0xF));
                        SetCarryFlag(A < D);
                        A = (byte)(zzzzcccccz & 0xFF);
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
                        PushStack(PC);
                        PC = 0x10;
                        cycles = 16;
                        break;
                    }
                case 0xA8:
                    {
                        A ^= B;
                        SetZeroFlag(A == 0);
                        SetNegativeFlag(false);
                        SetHalfCarryFlag(false);
                        SetCarryFlag(false);
                        cycles = 4;
                        break;
                    }
                case 0x6C:
                    {
                        L = H;
                        cycles = 4;
                        break;
                    }
                case 0x67:
                    {
                        H = A;
                        cycles = 4;
                        break;
                    }
                case 0xB3:
                    {
                        A |= E;
                        SetZeroFlag(A == 0);
                        SetNegativeFlag(false);
                        SetHalfCarryFlag(false);
                        SetCarryFlag(false);
                        cycles = 4;
                        break;
                    }
                case 0xBE:
                    {
                        byte zczzvzc = memory.ReadByte(GetHL());
                        int zzzxzz = A - zczzvzc;
                        SetZeroFlag((zzzxzz & 0xFF) == 0);
                        SetNegativeFlag(true);
                        SetHalfCarryFlag((A & 0xF) < (zczzvzc & 0xF));
                        SetCarryFlag(A < zczzvzc);
                        cycles = 8;
                        break;
                    }
                case 0xCF:
                    {
                        PushStack(PC);
                        PC = 0x08;
                        cycles = 16;
                        break;
                    }
                case 0x6B:
                    {
                        L = E;
                        cycles = 4;
                        break;
                    }
                case 0xBB:
                    {
                        int vzvv = A - E;
                        SetZeroFlag((vzvv & 0xFF) == 0);
                        SetNegativeFlag(true);
                        SetHalfCarryFlag((A & 0xF) < (E & 0xF));
                        SetCarryFlag(A < E);
                        cycles = 4;
                        break;
                    }
                case 0x87:
                    {
                        int czzc = A + A;
                        SetZeroFlag((czzc & 0xFF) == 0);
                        SetNegativeFlag(false);
                        SetHalfCarryFlag((A & 0xF) + (A & 0xF) > 0xF);
                        SetCarryFlag(czzc > 0xFF);
                        A = (byte)(czzc & 0xFF);
                        cycles = 4;
                        break;
                    }
                case 0xE9:
                    {
                        PC = GetHL();
                        cycles = 4;
                        break;
                    }
                case 0x85:
                    {
                        int c = A + L;
                        SetZeroFlag((c & 0xFF) == 0);
                        SetNegativeFlag(false);
                        SetHalfCarryFlag(((A & 0xF) + (L & 0xF)) > 0xF);
                        SetCarryFlag(c > 0xFF);
                        A = (byte)(c & 0xFF);
                        cycles = 4;
                        break;
                    }
                case 0x91:
                    {
                        int z = A - C;
                        SetZeroFlag((z & 0xFF) == 0);
                        SetNegativeFlag(true);
                        SetHalfCarryFlag((A & 0xF) < (C & 0xF));
                        SetCarryFlag(z < 0);
                        A = (byte)(z & 0xFF);
                        cycles = 4;
                        break;
                    }
                case 0xC0:
                    {
                        if (!IsFlagSet(ZeroFlag))
                        {
                            PC = PopStack();
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
                        memory.WriteByte(GetHL(), E);
                        cycles = 8;
                        break;
                    }
                case 0x83:
                    {
                        AddToA(E);
                        cycles = 4;
                        break;
                    }
                case 0x88:
                    {
                        AdcToA(B);
                        cycles = 4;
                        break;
                    }
                case 0x89:
                    {
                        AdcToA(C);
                        cycles = 4;
                        break;
                    }
                case 0x6E:
                    {
                        L = memory.ReadByte(GetHL());
                        cycles = 8;
                        break;
                    }
                case 0xCE:
                    byte value = memory.ReadByte(PC++);
                    int carry = IsFlagSet(CarryFlag) ? 1 : 0;
                    int result = A + value + carry;
                    SetZeroFlag((result & 0xFF) == 0);
                    SetHalfCarryFlag(((A & 0xF) + (value & 0xF) + carry) > 0xF);
                    SetCarryFlag(result > 0xFF);
                    A = (byte)result;
                    cycles = 8;
                    break;
                case 0xC8:
                    if (DebugMode)
                    {
                        Console.WriteLine($"Executing RET Z | ZeroFlag: {IsFlagSet(ZeroFlag)} | SP=0x{SP:X4}");
                    }
                    if (IsFlagSet(ZeroFlag))
                    {
                        ushort returnAddress = PopStack();
                        if (DebugMode)
                        {
                            Console.WriteLine($"RET Z: Popped return address 0x{returnAddress:X4} from stack.");
                        }
                        PC = returnAddress;
                        cycles = 20;
                    }
                    else
                    {
                        if (DebugMode)
                        {
                            Console.WriteLine("RET Z: ZeroFlag not set, no return executed.");
                        }
                        cycles = 8;
                    }
                    break;
                case 0xD8:
                    if (IsFlagSet(CarryFlag))
                    {
                        PC = PopStack();
                        cycles = 20;
                    }
                    else
                    {
                        cycles = 8;
                    }
                    break;
                case 0xD5:
                    SP--;
                    memory.WriteByte(SP, D);
                    SP--;
                    memory.WriteByte(SP, E);
                    cycles = 16;
                    break;
                case 0x6F:
                    L = A;
                    cycles = 4;
                    break;
                case 0xAD:
                    A ^= L;
                    SetZeroFlag(A == 0);
                    ClearNegativeFlag();
                    ClearHalfCarryFlag();
                    ClearCarryFlag();
                    cycles = 4;
                    break;
                case 0xAE:
                    A ^= memory.ReadByte(GetHL());
                    SetZeroFlag(A == 0);
                    ClearNegativeFlag();
                    ClearHalfCarryFlag();
                    ClearCarryFlag();
                    cycles = 8;
                    break;
                case 0xFA:
                    ushort address = (ushort)(memory.ReadByte(PC++) | (memory.ReadByte(PC++) << 8));
                    A = memory.ReadByte(address);
                    cycles = 16;
                    break;
                case 0x7B:
                    A = E;
                    cycles = 4;
                    break;
                case 0xC4:
                    if (!IsFlagSet(ZeroFlag))
                    {
                        address = (ushort)(memory.ReadByte(PC++) | (memory.ReadByte(PC++) << 8));
                        PushStack(PC);
                        PC = address;
                        cycles = 24;
                    }
                    else
                    {
                        PC += 2;
                        cycles = 12;
                    }
                    break;
                case 0x8D:
                    carry = IsFlagSet(CarryFlag) ? 1 : 0;
                    result = A + L + carry;
                    SetZeroFlag((result & 0xFF) == 0);
                    SetHalfCarryFlag(((A & 0xF) + (L & 0xF) + carry) > 0xF);
                    SetCarryFlag(result > 0xFF);
                    A = (byte)result;
                    cycles = 8;
                    break;
                case 0x77:
                    memory.WriteByte(GetHL(), A);
                    cycles = 8;
                    break;
                case 0x03:
                    SetBC((ushort)(GetBC() + 1));
                    cycles = 8;
                    break;
                case 0x04:
                    B = Increment(B);
                    cycles = 4;
                    break;
                case 0x05:
                    B = Decrement(B);
                    cycles = 4;
                    break;
                case 0x06:
                    B = memory.ReadByte(PC++);
                    cycles = 8;
                    break;
                case 0x07:
                    A = RotateLeft(A);
                    F = (byte)((F & 0xCF) | ((A & 0x10) != 0 ? 0x10 : 0x00));
                    cycles = 4;
                    break;
                case 0x08:
                    ushort addr = memory.ReadWord(PC);
                    PC += 2;
                    memory.WriteWord(addr, SP);
                    cycles = 20;
                    break;
                case 0x09:
                    ushort hl = GetHL();
                    ushort bc = GetBC();
                    result = hl + bc;
                    SetFlag(6, false);
                    SetHalfCarryFlag(((hl & 0x0FFF) + (bc & 0x0FFF)) > 0x0FFF);
                    SetCarryFlag(result > 0xFFFF);
                    SetHL((ushort)result);
                    cycles = 8;
                    break;
                case 0x0A:
                    A = memory.ReadByte(GetBC());
                    cycles = 8;
                    break;
                case 0x0B:
                    SetBC((ushort)(GetBC() - 1));
                    cycles = 8;
                    break;
                case 0x0C:
                    C = Increment(C);
                    cycles = 4;
                    break;
                case 0x0D:
                    C = Decrement(C);
                    cycles = 4;
                    break;
                case 0x0E:
                    C = memory.ReadByte(PC++);
                    cycles = 8;
                    break;
                case 0x0F:
                    A = RotateRight(A);
                    F = (byte)((F & 0xCF) | ((A & 0x10) != 0 ? 0x10 : 0x00));
                    cycles = 4;
                    break;
                case 0x11:
                    E = memory.ReadByte(PC++);
                    D = memory.ReadByte(PC++);
                    cycles = 12;
                    break;
                case 0x12:
                    memory.WriteByte(GetDE(), A);
                    cycles = 8;
                    break;
                case 0x13:
                    SetDE((ushort)(GetDE() + 1));
                    cycles = 8;
                    break;
                case 0x14:
                    D = Increment(D);
                    cycles = 4;
                    break;
                case 0x15:
                    D = Decrement(D);
                    cycles = 4;
                    break;
                case 0x16:
                    D = memory.ReadByte(PC++);
                    cycles = 8;
                    break;
                case 0x17:
                    A = RotateLeftThroughCarry(A);
                    cycles = 4;
                    break;
                case 0x18:
                    sbyte offset = (sbyte)memory.ReadByte(PC++);
                    PC = (ushort)(PC + offset);
                    cycles = 12;
                    break;
                case 0x19:
                    hl = GetHL();
                    ushort de = GetDE();
                    result = hl + de;
                    SetFlag(6, false);
                    SetHalfCarryFlag(((hl & 0x0FFF) + (de & 0x0FFF)) > 0x0FFF);
                    SetCarryFlag(result > 0xFFFF);
                    SetHL((ushort)result);
                    cycles = 8;
                    break;
                case 0x1A:
                    A = memory.ReadByte(GetDE());
                    cycles = 8;
                    break;
                case 0x1B:
                    SetDE((ushort)(GetDE() - 1));
                    cycles = 8;
                    break;
                case 0x1C:
                    E = Increment(E);
                    cycles = 4;
                    break;
                case 0x1D:
                    E = Decrement(E);
                    cycles = 4;
                    break;
                case 0x1E:
                    E = memory.ReadByte(PC++);
                    cycles = 8;
                    break;
                case 0x1F:
                    A = RotateRightThroughCarry(A);
                    cycles = 4;
                    break;
                case 0x20:
                    sbyte vzv = (sbyte)memory.ReadByte(PC++);
                    if (DebugMode)
                        Console.WriteLine($"JR NZ: PC=0x{PC:X4}, Offset={vzv}, ZeroFlag={IsFlagSet(ZeroFlag)}");
                    if (!IsFlagSet(ZeroFlag))
                    {
                        PC = (ushort)(PC + vzv);
                        if (DebugMode)
                            Console.WriteLine($"JR NZ: Jumping to PC=0x{PC:X4}");
                        cycles = 12;
                    }
                    else
                    {
                        if (DebugMode)
                            Console.WriteLine("JR NZ: No jump, ZeroFlag is set.");
                        cycles = 8;
                    }
                    break;
                case 0x21:
                    L = memory.ReadByte(PC++);
                    H = memory.ReadByte(PC++);
                    cycles = 12;
                    break;
                case 0x22:
                    memory.WriteByte(GetHL(), A);
                    SetHL((ushort)(GetHL() + 1));
                    cycles = 8;
                    break;
                case 0x23:
                    SetHL((ushort)(GetHL() + 1));
                    cycles = 8;
                    break;
                case 0x24:
                    H = Increment(H);
                    cycles = 4;
                    break;
                case 0x25:
                    H = Decrement(H);
                    cycles = 4;
                    break;
                case 0x26:
                    H = memory.ReadByte(PC++);
                    cycles = 8;
                    break;
                case 0x27:
                    DAA();
                    cycles = 4;
                    break;
                case 0x28:
                    offset = (sbyte)memory.ReadByte(PC++);
                    if (IsFlagSet(ZeroFlag))
                    {
                        PC = (ushort)(PC + offset);
                        cycles = 12;
                    }
                    else
                    {
                        cycles = 8;
                    }
                    break;
                case 0x29:
                    hl = GetHL();
                    result = hl + hl;
                    SetFlag(6, false);
                    SetHalfCarryFlag(((hl & 0x0FFF) + (hl & 0x0FFF)) > 0x0FFF);
                    SetCarryFlag(result > 0xFFFF);
                    SetHL((ushort)result);
                    cycles = 8;
                    break;
                case 0x2A:
                    A = memory.ReadByte(GetHL());
                    SetHL((ushort)(GetHL() + 1));
                    cycles = 8;
                    break;
                case 0x2B:
                    SetHL((ushort)(GetHL() - 1));
                    cycles = 8;
                    break;
                case 0x2C:
                    L = Increment(L);
                    cycles = 4;
                    break;
                case 0x2D:
                    L = Decrement(L);
                    cycles = 4;
                    break;
                case 0x2E:
                    L = memory.ReadByte(PC++);
                    cycles = 8;
                    break;
                case 0x2F:
                    A = (byte)~A;
                    SetFlag(6, true);
                    SetFlag(5, true);
                    cycles = 4;
                    break;
                case 0x30:
                    offset = (sbyte)memory.ReadByte(PC++);
                    if (!IsFlagSet(CarryFlag))
                    {
                        PC = (ushort)(PC + offset);
                        cycles = 12;
                    }
                    else
                    {
                        cycles = 8;
                    }
                    break;
                case 0x31:
                    SP = memory.ReadWord(PC);
                    PC += 2;
                    cycles = 12;
                    break;
                case 0x32:
                    hl = GetHL();
                    memory.WriteByte(hl, A);
                    SetHL((ushort)(hl - 1));
                    cycles = 8;
                    break;
                case 0x33:
                    SP++;
                    cycles = 8;
                    break;
                case 0x34:
                    byte val = memory.ReadByte(GetHL());
                    val = Increment(val);
                    memory.WriteByte(GetHL(), val);
                    cycles = 12;
                    break;
                case 0x35:
                    val = memory.ReadByte(GetHL());
                    val = Decrement(val);
                    memory.WriteByte(GetHL(), val);
                    cycles = 12;
                    break;
                case 0x36:
                    memory.WriteByte(GetHL(), memory.ReadByte(PC++));
                    cycles = 12;
                    break;
                case 0x37:
                    SetFlag(6, false);
                    SetFlag(5, false);
                    SetCarryFlag(true);
                    cycles = 4;
                    break;
                case 0x38:
                    offset = (sbyte)memory.ReadByte(PC++);
                    if (IsFlagSet(CarryFlag))
                    {
                        PC = (ushort)(PC + offset);
                        cycles = 12;
                    }
                    else
                    {
                        cycles = 8;
                    }
                    break;
                case 0x39:
                    hl = GetHL();
                    result = hl + SP;
                    SetFlag(6, false);
                    SetHalfCarryFlag(((hl & 0x0FFF) + (SP & 0x0FFF)) > 0x0FFF);
                    SetCarryFlag(result > 0xFFFF);
                    SetHL((ushort)result);
                    cycles = 8;
                    break;
                case 0x3A:
                    A = memory.ReadByte(GetHL());
                    SetHL((ushort)(GetHL() - 1));
                    cycles = 8;
                    break;
                case 0x3B:
                    SP--;
                    cycles = 8;
                    break;
                case 0x3C:
                    A = Increment(A);
                    cycles = 4;
                    break;
                case 0x3D:
                    A = Decrement(A);
                    cycles = 4;
                    break;
                case 0x3E:
                    A = memory.ReadByte(PC++);
                    cycles = 8;
                    break;
                case 0x3F:
                    SetCarryFlag(!IsFlagSet(CarryFlag));
                    SetFlag(6, false);
                    SetFlag(5, false);
                    cycles = 4;
                    break;
                case 0xAF:
                    A ^= A;
                    SetZeroFlag(A == 0);
                    ClearNegativeFlag();
                    ClearHalfCarryFlag();
                    ClearCarryFlag();
                    cycles = 4;
                    break;
                case 0xD0:  // RET NC
                    if (!IsFlagSet(CarryFlag))
                    {
                        PC = PopStack();  // Return to the address on top of the stack
                        cycles = 20;
                    }
                    else
                    {
                        cycles = 8;
                    }
                    break;

                case 0x41:
                    B = C;
                    cycles = 4;
                    break;
                case 0xB7:
                    A |= A;
                    SetZeroFlag(A == 0);
                    ClearNegativeFlag();
                    ClearHalfCarryFlag();
                    ClearCarryFlag();
                    cycles = 4;
                    break;
                case 0x4E:
                    C = memory.ReadByte(GetHL());
                    cycles = 8;
                    break;
                case 0x70:
                    memory.WriteByte(GetHL(), B);
                    cycles = 8;
                    break;
                case 0xD1:
                    E = memory.ReadByte(SP++);
                    D = memory.ReadByte(SP++);
                    cycles = 12;
                    break;
                case 0xF9:
                    SP = GetHL();
                    cycles = 8;
                    break;
                case 0x7A:
                    A = D;
                    cycles = 4;
                    break;
                case 0xE6:
                    value = memory.ReadByte(PC++);
                    A &= value;
                    F = 0x20;
                    if (A == 0)
                        F |= 0x80;
                    cycles = 8;
                    break;
                case 0xF0:
                    byte a8 = memory.ReadByte(PC++);
                    A = memory.ReadByte((ushort)(0xFF00 + a8));
                    cycles = 12;
                    break;
                case 0xB6:
                    A |= memory.ReadByte(GetHL());
                    SetZeroFlag(A == 0);
                    ClearNegativeFlag();
                    ClearHalfCarryFlag();
                    ClearCarryFlag();
                    cycles = 8;
                    break;
                case 0xFF:
                    PushStack(PC);
                    PC = 0x0038;
                    cycles = 16;
                    break;
                case 0xCD:
                    ushort ca = memory.ReadWord(PC);
                    PC += 2;
                    PushStack(PC);
                    PC = ca;
                    cycles = 24;
                    break;
                case 0xC9:
                    PC = PopStack();
                    cycles = 16;
                    break;
                case 0xCA:
                    addr = memory.ReadWord(PC);
                    PC += 2;
                    if (IsFlagSet(ZeroFlag))
                    {
                        PC = addr;
                        cycles = 16;
                    }
                    else
                    {
                        cycles = 12;
                    }
                    break;
                case 0x78:
                    A = B;
                    cycles = 4;
                    break;
                case 0xF5:
                    SP -= 2;
                    memory.WriteWord(SP, (ushort)((A << 8) | (F & 0xF0)));
                    cycles = 16;
                    break;
                case 0xE0:
                    byte n = memory.ReadByte(PC++);
                    memory.WriteByte((ushort)(0xFF00 + n), A);
                    cycles = 12;
                    break;
                case 0xF1:
                    ushort af = memory.ReadWord(SP);
                    SP += 2;
                    A = (byte)(af >> 8);
                    F = (byte)(af & 0xF0);
                    cycles = 12;
                    break;
                case 0x47:
                    B = A;
                    cycles = 4;
                    break;
                case 0xA7:
                    A &= A;
                    F = 0x20;
                    if (A == 0)
                        F |= 0x80;
                    cycles = 4;
                    break;
                case 0xA9:
                    A ^= C;
                    SetZeroFlag(A == 0);
                    ClearNegativeFlag();
                    ClearHalfCarryFlag();
                    ClearCarryFlag();
                    cycles = 4;
                    break;
                case 0x5F:
                    E = A;
                    cycles = 4;
                    break;
                case 0x7C:
                    A = H;
                    cycles = 4;
                    break;
                case 0x7D:
                    A = L;
                    cycles = 4;
                    break;
                case 0x46:
                    B = memory.ReadByte(GetHL());
                    cycles = 8;
                    break;
                case 0x7E:
                    A = memory.ReadByte(GetHL());
                    cycles = 8;
                    break;
                case 0xA0:
                    A &= B;
                    F = 0x20;
                    if (A == 0)
                        F |= 0x80;
                    cycles = 4;
                    break;
                case 0x56:
                    D = memory.ReadByte(GetHL());
                    cycles = 8;
                    break;
                case 0xF3:
                    IME = false;
                    cycles = 4;
                    break;
                case 0xFE:
                    {
                        byte d8 = memory.ReadByte(PC++);
                        if (DebugMode)
                        {
                            Console.WriteLine($"Comparing A: 0x{A:X2} with d8: 0x{d8:X2} at PC: 0x{PC:X4}");
                        }
                        SetZeroFlag(A == d8);
                        SetNegativeFlag(true);
                        SetHalfCarryFlag((A & 0xF) < (d8 & 0xF));
                        SetCarryFlag(A < d8);
                        if (DebugMode)
                        {
                            Console.WriteLine($"ZeroFlag: {IsFlagSet(ZeroFlag)}, NegativeFlag: {IsFlagSet(NegativeFlag)}, HalfCarryFlag: {IsFlagSet(HalfCarryFlag)}, CarryFlag: {IsFlagSet(CarryFlag)}");
                        }
                        cycles = 8;
                        break;
                    }
                case 0x45:
                    B = L;
                    cycles = 4;
                    break;
                case 0x4C:
                    C = H;
                    cycles = 4;
                    break;
                case 0x48:
                    C = B;
                    cycles = 4;
                    break;
                case 0x90:
                    SetCarryFlag(A < B);
                    SetHalfCarryFlag((A & 0xF) < (B & 0xF));
                    A -= B;
                    SetZeroFlag(A == 0);
                    SetFlag(6, true);
                    cycles = 4;
                    break;
                case 0x66:
                    ushort cc = (ushort)((H << 8) | L);
                    H = memory.ReadByte(cc);
                    cycles = 8;
                    break;
                case 0x94:
                    Subtract(H);
                    cycles = 4;
                    break;
                case 0xEA:
                    ushort k = memory.ReadWord(PC);
                    PC += 2;
                    memory.WriteByte(k, A);
                    cycles = 16;
                    break;
                case 0x59:
                    E = C;
                    cycles = 4;
                    break;
                default:
                    Console.WriteLine($"Unimplemented opcode: 0x{opcode:X2} at address 0x{PC - 1:X4}");
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
                    case 0: return B;
                    case 1: return C;
                    case 2: return D;
                    case 3: return E;
                    case 4: return H;
                    case 5: return L;
                    case 6: return memory.ReadByte(GetHL());
                    case 7: return A;
                    default: return 0;
                }
            }
            void SetRegisterValue(int index, byte val)
            {
                switch (index)
                {
                    case 0: B = val; break;
                    case 1: C = val; break;
                    case 2: D = val; break;
                    case 3: E = val; break;
                    case 4: H = val; break;
                    case 5: L = val; break;
                    case 6: memory.WriteByte(GetHL(), val); break;
                    case 7: A = val; break;
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
                    SetZeroFlag(result == 0);
                    ClearNegativeFlag();
                    ClearHalfCarryFlag();
                    SetCarryFlag((value & 0x80) != 0);
                    SetRegisterValue(regIndex, result);
                    cycles = RegisterCycles(regIndex, 8, 16);
                    break;
                case 0x01:
                    value = GetRegisterValue(regIndex);
                    result = (byte)((value >> 1) | (value << 7));
                    SetZeroFlag(result == 0);
                    ClearNegativeFlag();
                    ClearHalfCarryFlag();
                    SetCarryFlag((value & 0x01) != 0);
                    SetRegisterValue(regIndex, result);
                    cycles = RegisterCycles(regIndex, 8, 16);
                    break;
                case 0x02:
                    value = GetRegisterValue(regIndex);
                    byte carryIn = IsFlagSet(CarryFlag) ? (byte)1 : (byte)0;
                    result = (byte)((value << 1) | carryIn);
                    SetZeroFlag(result == 0);
                    ClearNegativeFlag();
                    ClearHalfCarryFlag();
                    SetCarryFlag((value & 0x80) != 0);
                    SetRegisterValue(regIndex, result);
                    cycles = RegisterCycles(regIndex, 8, 16);
                    break;
                case 0x03:
                    value = GetRegisterValue(regIndex);
                    byte carryOut = (byte)(value & 0x01);
                    byte carryInRR = IsFlagSet(CarryFlag) ? (byte)0x80 : (byte)0;
                    result = (byte)((value >> 1) | carryInRR);
                    SetZeroFlag(result == 0);
                    ClearNegativeFlag();
                    ClearHalfCarryFlag();
                    SetCarryFlag(carryOut != 0);
                    SetRegisterValue(regIndex, result);
                    cycles = RegisterCycles(regIndex, 8, 16);
                    break;
                case 0x04:
                    value = GetRegisterValue(regIndex);
                    result = (byte)(value << 1);
                    SetZeroFlag(result == 0);
                    ClearNegativeFlag();
                    ClearHalfCarryFlag();
                    SetCarryFlag((value & 0x80) != 0);
                    SetRegisterValue(regIndex, result);
                    cycles = RegisterCycles(regIndex, 8, 16);
                    break;
                case 0x05:
                    value = GetRegisterValue(regIndex);
                    result = (byte)((value >> 1) | (value & 0x80));
                    SetZeroFlag(result == 0);
                    ClearNegativeFlag();
                    ClearHalfCarryFlag();
                    SetCarryFlag((value & 0x01) != 0);
                    SetRegisterValue(regIndex, result);
                    cycles = RegisterCycles(regIndex, 8, 16);
                    break;
                case 0x06:
                    value = GetRegisterValue(regIndex);
                    result = (byte)(((value & 0x0F) << 4) | ((value & 0xF0) >> 4));
                    SetZeroFlag(result == 0);
                    ClearNegativeFlag();
                    ClearHalfCarryFlag();
                    ClearCarryFlag();
                    SetRegisterValue(regIndex, result);
                    cycles = RegisterCycles(regIndex, 8, 16);
                    break;
                case 0x07:
                    value = GetRegisterValue(regIndex);
                    carryOut = (byte)(value & 0x01);
                    result = (byte)(value >> 1);
                    SetZeroFlag(result == 0);
                    ClearNegativeFlag();
                    ClearHalfCarryFlag();
                    SetCarryFlag(carryOut != 0);
                    SetRegisterValue(regIndex, result);
                    cycles = RegisterCycles(regIndex, 8, 16);
                    break;
                case int n when (n >= 0x08 && n <= 0x0F):
                    int bit = (opCodeGroup - 0x08);
                    value = GetRegisterValue(regIndex);
                    SetZeroFlag((value & (1 << bit)) == 0);
                    ClearNegativeFlag();
                    SetHalfCarryFlag(true);
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
                    Console.WriteLine($"Unimplemented CB opcode: 0x{opcode:X2} at address 0x{PC - 2:X4}");
                    cycles = 4;
                    break;
            }
            return cycles;
        }
        private void Subtract(byte value)
        {
            int result = A - value;
            SetZeroFlag((result & 0xFF) == 0);
            SetFlag(6, true);
            SetHalfCarryFlag((A & 0x0F) < (value & 0x0F));
            SetCarryFlag(A < value);
            A = (byte)result;
        }
        private void AddToA(byte value)
        {
            int result = A + value;
            SetZeroFlag((result & 0xFF) == 0);
            SetFlag(6, false);
            SetHalfCarryFlag(((A & 0x0F) + (value & 0x0F)) > 0x0F);
            SetCarryFlag(result > 0xFF);
            A = (byte)result;
        }
        private void AdcToA(byte value)
        {
            int carry = IsFlagSet(CarryFlag) ? 1 : 0;
            int result = A + value + carry;
            SetZeroFlag((result & 0xFF) == 0);
            SetFlag(6, false);
            SetHalfCarryFlag(((A & 0x0F) + (value & 0x0F) + carry) > 0x0F);
            SetCarryFlag(result > 0xFF);
            A = (byte)result;
        }
        private void SubtractWithCarry(byte value)
        {
            int carry = IsFlagSet(CarryFlag) ? 1 : 0;
            int result = A - value - carry;
            SetZeroFlag((result & 0xFF) == 0);
            SetFlag(6, true);
            SetHalfCarryFlag(((A & 0x0F) - (value & 0x0F) - carry) < 0);
            SetCarryFlag((A - carry) < value);
            A = (byte)result;
        }
        private void CP(byte value)
        {
            int result = A - value;
            SetZeroFlag(A == value);
            SetFlag(6, true);
            SetHalfCarryFlag((A & 0x0F) < (value & 0x0F));
            SetCarryFlag(A < value);
        }
        private byte Increment(byte val)
        {
            val++;
            SetZeroFlag(val == 0);
            ClearNegativeFlag();
            SetHalfCarryFlag((val & 0x0F) == 0);
            return val;
        }
        private byte Decrement(byte val)
        {
            val--;
            SetZeroFlag(val == 0);
            SetFlag(6, true);
            SetHalfCarryFlag((val & 0x0F) == 0x0F);
            return val;
        }
        private byte RotateLeft(byte val)
        {
            byte carry = (byte)((val & 0x80) >> 7);
            val = (byte)((val << 1) | carry);
            SetZeroFlag(val == 0);
            ClearNegativeFlag();
            ClearHalfCarryFlag();
            SetCarryFlag(carry != 0);
            return val;
        }
        private byte RotateRight(byte val)
        {
            byte carry = (byte)(val & 0x01);
            val = (byte)((val >> 1) | (carry << 7));
            SetZeroFlag(val == 0);
            ClearNegativeFlag();
            ClearHalfCarryFlag();
            SetCarryFlag(carry != 0);
            return val;
        }
        private byte RotateLeftThroughCarry(byte val)
        {
            byte carryIn = IsFlagSet(CarryFlag) ? (byte)1 : (byte)0;
            byte newCarry = (byte)((val & 0x80) >> 7);
            val = (byte)((val << 1) | carryIn);
            SetZeroFlag(val == 0);
            ClearNegativeFlag();
            ClearHalfCarryFlag();
            SetCarryFlag(newCarry != 0);
            return val;
        }
        private byte RotateRightThroughCarry(byte val)
        {
            byte carryIn = IsFlagSet(CarryFlag) ? (byte)0x80 : (byte)0;
            byte newCarry = (byte)(val & 0x01);
            val = (byte)((val >> 1) | carryIn);
            SetZeroFlag(val == 0);
            ClearNegativeFlag();
            ClearHalfCarryFlag();
            SetCarryFlag(newCarry != 0);
            return val;
        }
        private void DAA()
        {
            int adjust = 0;
            if ((F & HalfCarryFlag) != 0 || ((A & 0x0F) > 9))
                adjust += 0x06;
            if ((F & CarryFlag) != 0 || A > 0x99)
            {
                adjust += 0x60;
                SetCarryFlag(true);
            }
            else
            {
                SetCarryFlag(false);
            }
            if ((F & NegativeFlag) != 0)
                A = (byte)(A - adjust);
            else
                A = (byte)(A + adjust);
            SetZeroFlag(A == 0);
            ClearHalfCarryFlag();
        }
        private void PushStack(ushort value)
        {
            SP--;
            memory.WriteByte(SP, (byte)(value >> 8));
            SP--;
            memory.WriteByte(SP, (byte)(value & 0xFF));
        }
        private ushort PopStack()
        {
            ushort lowByte = memory.ReadByte(SP++);
            ushort highByte = memory.ReadByte(SP++);
            return (ushort)((highByte << 8) | lowByte);
        }
        private ushort GetBC()
        {
            return (ushort)((B << 8) | C);
        }
        private void SetBC(ushort val)
        {
            B = (byte)(val >> 8);
            C = (byte)(val & 0xFF);
        }
        private ushort GetDE()
        {
            return (ushort)((D << 8) | E);
        }
        private void SetDE(ushort val)
        {
            D = (byte)(val >> 8);
            E = (byte)(val & 0xFF);
        }
        private ushort GetHL()
        {
            return (ushort)((H << 8) | L);
        }
        private void SetHL(ushort val)
        {
            H = (byte)(val >> 8);
            L = (byte)(val & 0xFF);
        }
        const byte ZeroFlag = 0x80;
        const byte NegativeFlag = 0x40;
        const byte HalfCarryFlag = 0x20;
        const byte CarryFlag = 0x10;
        public void SetZeroFlag(bool condition)
        {
            if (DebugMode) Console.WriteLine("Condition: " + condition);
            if (DebugMode) Console.WriteLine($"F before setting ZeroFlag: 0x{F:X2}");
            if (condition)
                F |= ZeroFlag;
            else
                F &= (byte)(~ZeroFlag & 0xFF);
            if (DebugMode) Console.WriteLine($"F after setting ZeroFlag: 0x{F:X2}");
        }
        private void ClearNegativeFlag()
        {
            F &= (byte)(~NegativeFlag & 0xFF);
        }
        private void ClearHalfCarryFlag()
        {
            F &= (byte)(~HalfCarryFlag & 0xFF);
        }
        private void ClearCarryFlag()
        {
            F &= (byte)(~CarryFlag & 0xFF);
        }
        private void SetCarryFlag(bool condition)
        {
            if (condition)
                F |= CarryFlag;
            else
                F &= (byte)(~CarryFlag & 0xFF);
        }
        private void SetHalfCarryFlag(bool condition)
        {
            if (condition)
                F |= HalfCarryFlag;
            else
                F &= (byte)(~HalfCarryFlag & 0xFF);
        }
        private void SetFlag(int bit, bool value)
        {
            if (value)
            {
                F |= (byte)(1 << bit);
            }
            else
            {
                F &= (byte)~(1 << bit);
            }
            if (DebugModeFlag)
            {
                Console.WriteLine($"SetFlag({bit} set to {value}). F = 0x{F:X2}");
            }
        }
        public void SetNegativeFlag(bool condition)
        {
            if (DebugMode) Console.WriteLine($"Setting NegativeFlag to {condition}");
            if (condition)
                F |= NegativeFlag;
            else
                F &= (byte)(~NegativeFlag & 0xFF);
            if (DebugMode) Console.WriteLine($"F after setting NegativeFlag: 0x{F:X2}");
        }
        public bool GetZeroFlag()
        {
            return IsFlagSet(0x80);
        }
        public bool GetNegativeFlag()
        {
            return IsFlagSet(0x40);
        }
        public bool GetHalfCarryFlag()
        {
            return IsFlagSet(0x20);
        }
        public bool GetCarryFlag()
        {
            return IsFlagSet(0x10);
        }
        public bool IsFlagSet(byte flagMask)
        {
            bool isSet = (F & flagMask) != 0;
            if (DebugMode) Console.WriteLine($"IsFlagSet: Checking flag 0x{flagMask:X2}, Result: {isSet}");
            return isSet;
        }
    }
}
