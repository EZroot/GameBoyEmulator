using GameBoyEmulator.Debug;
using System;
namespace GameBoyEmulator.Processor
{
    public class Registers
    {
        public const byte ZeroFlag = 0x80;
        public const byte NegativeFlag = 0x40;
        public const byte HalfCarryFlag = 0x20;
        public const byte CarryFlag = 0x10;
        public byte A, F, B, C, D, E, H, L;
        public ushort PC, SP;
        public bool IME;
        public bool Halted;
        public bool EI_Scheduled = false;
        public Registers()
        {
            Reset();
            Logger.Log("Registers initialized.");
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
        public void Subtract(byte value)
        {
            int result = A - value;
            SetZeroFlag((result & 0xFF) == 0);
            SetFlag(6, true);
            SetHalfCarryFlag((A & 0x0F) < (value & 0x0F));
            SetCarryFlag(A < value);
            A = (byte)result;
        }
        public void AddToA(byte value)
        {
            int result = A + value;
            SetZeroFlag((result & 0xFF) == 0);
            SetFlag(6, false);
            SetHalfCarryFlag(((A & 0x0F) + (value & 0x0F)) > 0x0F);
            SetCarryFlag(result > 0xFF);
            A = (byte)result;
        }
        public void AdcToA(byte value)
        {
            int carry = IsFlagSet(CarryFlag) ? 1 : 0;
            int result = A + value + carry;
            SetZeroFlag((result & 0xFF) == 0);
            SetFlag(6, false);
            SetHalfCarryFlag(((A & 0x0F) + (value & 0x0F) + carry) > 0x0F);
            SetCarryFlag(result > 0xFF);
            A = (byte)result;
        }
        public void SubtractWithCarry(byte value)
        {
            int carry = IsFlagSet(CarryFlag) ? 1 : 0;
            int result = A - value - carry;
            SetZeroFlag((result & 0xFF) == 0);
            SetFlag(6, true);
            SetHalfCarryFlag(((A & 0x0F) - (value & 0x0F) - carry) < 0);
            SetCarryFlag((A - carry) < value);
            A = (byte)result;
        }
        public void CP(byte value)
        {
            int result = A - value;
            SetZeroFlag(A == value);
            SetFlag(6, true);
            SetHalfCarryFlag((A & 0x0F) < (value & 0x0F));
            SetCarryFlag(A < value);
        }
        public byte Increment(byte val)
        {
            val++;
            SetZeroFlag(val == 0);
            ClearNegativeFlag();
            SetHalfCarryFlag((val & 0x0F) == 0);
            return val;
        }
        public byte Decrement(byte val)
        {
            val--;
            SetZeroFlag(val == 0);
            SetFlag(6, true);
            SetHalfCarryFlag((val & 0x0F) == 0x0F);
            return val;
        }
        public byte RotateLeft(byte val)
        {
            byte carry = (byte)((val & 0x80) >> 7);
            val = (byte)((val << 1) | carry);
            SetZeroFlag(val == 0);
            ClearNegativeFlag();
            ClearHalfCarryFlag();
            SetCarryFlag(carry != 0);
            return val;
        }
        public byte RotateRight(byte val)
        {
            byte carry = (byte)(val & 0x01);
            val = (byte)((val >> 1) | (carry << 7));
            SetZeroFlag(val == 0);
            ClearNegativeFlag();
            ClearHalfCarryFlag();
            SetCarryFlag(carry != 0);
            return val;
        }
        public byte RotateLeftThroughCarry(byte val)
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
        public byte RotateRightThroughCarry(byte val)
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
        public void DAA()
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
        public ushort GetBC()
        {
            return (ushort)((B << 8) | C);
        }
        public void SetBC(ushort val)
        {
            B = (byte)(val >> 8);
            C = (byte)(val & 0xFF);
        }
        public ushort GetDE()
        {
            return (ushort)((D << 8) | E);
        }
        public void SetDE(ushort val)
        {
            D = (byte)(val >> 8);
            E = (byte)(val & 0xFF);
        }
        public ushort GetHL()
        {
            return (ushort)((H << 8) | L);
        }
        public void SetHL(ushort val)
        {
            H = (byte)(val >> 8);
            L = (byte)(val & 0xFF);
        }
        public void SubtractFromA(byte value)
        {
            int result = (A - value) & 0xFF;
            SetZeroFlag(result == 0);
            SetNegativeFlag(true);
            bool halfCarry = ((A & 0x0F) < (value & 0x0F));
            SetHalfCarryFlag(halfCarry);
            SetCarryFlag(A < value);
            A = (byte)result;
        }
        public void SbcFromA(byte value)
        {
            int carry = IsFlagSet(CarryFlag) ? 1 : 0;
            int result = A - value - carry;
            SetZeroFlag((result & 0xFF) == 0);
            SetNegativeFlag(true);
            SetHalfCarryFlag(((A ^ value ^ result) & 0x10) != 0);
            SetCarryFlag(A < (value + carry));
            A = (byte)result;
        }
        public void AndWithA(byte value)
        {
            A &= value;
            SetZeroFlag(A == 0);
            SetNegativeFlag(false);
            SetHalfCarryFlag(true);
            SetCarryFlag(false);
        }
        public void XorWithA(byte value)
        {
            A ^= value;
            SetZeroFlag(A == 0);
            SetNegativeFlag(false);
            SetHalfCarryFlag(false);
            SetCarryFlag(false);
        }
        public void OrWithA(byte value)
        {
            A |= value;
            SetZeroFlag(A == 0);
            SetNegativeFlag(false);
            SetHalfCarryFlag(false);
            SetCarryFlag(false);
        }
        public void CompareA(byte value)
        {
            int result = A - value;
            SetZeroFlag((result & 0xFF) == 0);
            SetNegativeFlag(true);
            SetHalfCarryFlag((A & 0x0F) < (value & 0x0F));
            SetCarryFlag(A < value);
        }
        public void SetZeroFlag(bool condition)
        {
            if (Debugger.IsDebugEnabled && Debugger.dWriteOutOpcode) Console.WriteLine("Condition: " + condition);
            if (Debugger.IsDebugEnabled && Debugger.dWriteOutOpcode) Console.WriteLine($"F before setting ZeroFlag: 0x{F:X2}");
            if (condition)
                F |= ZeroFlag;
            else
                F &= (byte)(~ZeroFlag & 0xFF);
            if (Debugger.IsDebugEnabled && Debugger.dWriteOutOpcode) Console.WriteLine($"F after setting ZeroFlag: 0x{F:X2}");
        }
        public void ClearNegativeFlag()
        {
            F &= (byte)(~NegativeFlag & 0xFF);
        }
        public void ClearHalfCarryFlag()
        {
            F &= (byte)(~HalfCarryFlag & 0xFF);
        }
        public void ClearCarryFlag()
        {
            F &= (byte)(~CarryFlag & 0xFF);
        }
        public void SetCarryFlag(bool condition)
        {
            if (condition)
                F |= CarryFlag;
            else
                F &= (byte)(~CarryFlag & 0xFF);
        }
        public void SetHalfCarryFlag(bool condition)
        {
            if(Debugger.IsDebugEnabled && Debugger.dWriteOutMemoryReadWrite) Console.WriteLine($"Setting Half Carry Flag to {condition}");
            if (condition)
                F |= HalfCarryFlag;
            else
                F &= (byte)(~HalfCarryFlag & 0xFF);
            if (Debugger.IsDebugEnabled && Debugger.dWriteOutMemoryReadWrite) Console.WriteLine($"F after setting Half Carry Flag: 0x{F:X2}");
        }
        public void SetFlag(int bit, bool value)
        {
            if (value)
            {
                F |= (byte)(1 << bit);
            }
            else
            {
                F &= (byte)~(1 << bit);
            }
            if (Debugger.IsDebugEnabled && Debugger.dWriteOutOpcode)
            {
                Console.WriteLine($"SetFlag({bit} set to {value}). F = 0x{F:X2}");
            }
        }
        public void ClearZeroFlag()
        {
            F &= (byte)(~ZeroFlag & 0xFF);
            if (Debugger.IsDebugEnabled && Debugger.dWriteOutOpcode) Console.WriteLine($"F after clearing ZeroFlag: 0x{F:X2}");
        }
        public void SetNegativeFlag(bool condition)
        {
            if (Debugger.IsDebugEnabled && Debugger.dWriteOutOpcode) Console.WriteLine($"Setting NegativeFlag to {condition}");
            if (condition)
                F |= NegativeFlag;
            else
                F &= (byte)(~NegativeFlag & 0xFF);
            if (Debugger.IsDebugEnabled && Debugger.dWriteOutOpcode) Console.WriteLine($"F after setting NegativeFlag: 0x{F:X2}");
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
            if (Debugger.IsDebugEnabled && Debugger.dWriteOutOpcode) Console.WriteLine($"IsFlagSet: Checking flag 0x{flagMask:X2}, Result: {isSet}");
            return isSet;
        }
    }
}
