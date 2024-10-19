namespace GameBoyEmulator.Tests
{
    public class EmulatorTests
    {
        private Emulator _emulator;
        private CPU _cpu;
        private Memory _memory;

        [SetUp]
        public void Setup()
        {
            _emulator = new Emulator();
            _memory = new Memory();
            _cpu = new CPU(_memory);
        }

        [Test]
        public void Test_JR_NZ_When_ZeroFlag_Not_Set_Should_Jump()
        {

            _cpu.PC = 0x100;
            _cpu.SetZeroFlag(false); 
            _memory.WriteByte(0x100, 0x20); 
            _memory.WriteByte(0x101, 0x05); 

            _cpu.Step(); 

            Assert.AreEqual(0x107, _cpu.PC); 
        }

        [Test]
        public void Test_JR_NZ_When_ZeroFlag_Set_Should_Not_Jump()
        {

            _cpu.PC = 0x200;
            _cpu.SetZeroFlag(true); 
            _memory.WriteByte(0x200, 0x20); 
            _memory.WriteByte(0x201, 0x05); 

            _cpu.Step(); 

            Assert.AreEqual(0x202, _cpu.PC); 
        }

        [Test]
        public void Test_JP_Z_When_ZeroFlag_Set_Should_Jump()
        {

            _cpu.PC = 0x100;
            _cpu.SetZeroFlag(true); 
            _memory.WriteByte(0x100, 0xCA); 
            _memory.WriteWord(0x101, 0x200); 

            _cpu.Step(); 

            Assert.AreEqual(0x200, _cpu.PC); 
        }

        [Test]
        public void Test_JP_Z_When_ZeroFlag_Not_Set_Should_Not_Jump()
        {

            _cpu.PC = 0x300;
            _cpu.SetZeroFlag(false); 
            _memory.WriteByte(0x300, 0xCA); 
            _memory.WriteWord(0x301, 0x400); 

            _cpu.Step(); 

            Assert.AreEqual(0x303, _cpu.PC); 
        }

        [Test]
        public void Test_CP_d8_With_A_Equal_To_d8_Should_Set_Zero_Flag()
        {

            _cpu.A = 0x50;
            _memory.WriteByte(0x100, 0xFE); 
            _memory.WriteByte(0x101, 0x50); 
            _cpu.PC = 0x100;

            _cpu.Step(); 

            Assert.IsTrue(_cpu.GetZeroFlag()); 
            Assert.IsTrue(_cpu.GetNegativeFlag()); 
            Assert.IsFalse(_cpu.GetCarryFlag()); 
            Assert.IsFalse(_cpu.GetHalfCarryFlag()); 
        }

        [Test]
        public void Test_CP_d8_With_A_Less_Than_d8_Should_Set_Carry_Flag()
        {

            _cpu.A = 0x30;
            _memory.WriteByte(0x200, 0xFE); 
            _memory.WriteByte(0x201, 0x50); 
            _cpu.PC = 0x200;

            _cpu.Step(); 

            Assert.IsFalse(_cpu.GetZeroFlag()); 
            Assert.IsTrue(_cpu.GetCarryFlag()); 
            Assert.IsTrue(_cpu.GetNegativeFlag()); 
            Assert.IsFalse(_cpu.GetHalfCarryFlag()); 
        }

        [Test]
        public void Test_RET_Should_Return_To_Correct_Address()
        {

            _cpu.SP = 0xFFFE;
            _memory.WriteWord(0xFFFE, 0x300); 
            _cpu.PC = 0x100;
            _memory.WriteByte(0x100, 0xC9); 

            _cpu.Step(); 

            Assert.AreEqual(0x300, _cpu.PC); 
        }

        [Test]
        public void Test_RET_Z_When_ZeroFlag_Set_Should_Return()
        {

            _cpu.SP = 0xFFFE;
            _memory.WriteWord(0xFFFE, 0x400); 
            _cpu.PC = 0x200;
            _cpu.SetZeroFlag(true); 
            _memory.WriteByte(0x200, 0xC8); 

            _cpu.Step(); 

            Assert.AreEqual(0x400, _cpu.PC); 
        }

        [Test]
        public void Test_ADD_A_ImmediateValue()
        {

            _cpu.A = 0x12;
            _memory.WriteByte(0x100, 0xC6); 
            _memory.WriteByte(0x101, 0x22); 
            _cpu.PC = 0x100;

            _cpu.Step();

            Assert.AreEqual(0x34, _cpu.A); 
            Assert.IsFalse(_cpu.GetZeroFlag());
            Assert.IsFalse(_cpu.GetNegativeFlag());
            Assert.IsFalse(_cpu.GetCarryFlag());
            Assert.IsFalse(_cpu.GetHalfCarryFlag());
        }

        [Test]
        public void Test_XOR_A_WithImmediateValue()
        {

            _cpu.A = 0xFF;
            _memory.WriteByte(0x100, 0xEE); 
            _memory.WriteByte(0x101, 0x0F); 
            _cpu.PC = 0x100;

            _cpu.Step();

            Assert.AreEqual(0xF0, _cpu.A); 
            Assert.IsFalse(_cpu.GetZeroFlag());
            Assert.IsFalse(_cpu.GetNegativeFlag());
            Assert.IsFalse(_cpu.GetCarryFlag());
            Assert.IsFalse(_cpu.GetHalfCarryFlag());
        }

        [Test]
        public void Test_SUB_A_ValueAt_HL()
        {

            _cpu.A = 0x50;
            _cpu.SetHL(0x2000);
            _memory.WriteByte(0x2000, 0x20); 
            _memory.WriteByte(0x100, 0x96);  
            _cpu.PC = 0x100;

            _cpu.Step();

            Assert.AreEqual(0x30, _cpu.A); 
            Assert.IsFalse(_cpu.GetZeroFlag());
            Assert.IsTrue(_cpu.GetNegativeFlag());
            Assert.IsFalse(_cpu.GetCarryFlag());
            Assert.IsFalse(_cpu.GetHalfCarryFlag());
        }

        [Test]
        public void Test_PUSH_POP_RegisterPair()
        {

            _cpu.B = 0x12;
            _cpu.C = 0x34;
            _cpu.SP = 0xFFFE;
            _memory.WriteByte(0x100, 0xC5); 
            _memory.WriteByte(0x101, 0xC1); 
            _cpu.PC = 0x100;

            _cpu.Step(); 
            _cpu.B = 0x00; 
            _cpu.C = 0x00;
            _cpu.Step(); 

            Assert.AreEqual(0x12, _cpu.B); 
            Assert.AreEqual(0x34, _cpu.C);
            Assert.AreEqual(0xFFFE, _cpu.SP); 
        }

        [Test]
        public void Test_OR_A_With_D()
        {

            _cpu.A = 0x33;
            _cpu.D = 0x55;
            _memory.WriteByte(0x100, 0xB2); 
            _cpu.PC = 0x100;

            _cpu.Step();

            Assert.AreEqual(0x77, _cpu.A); 
            Assert.IsFalse(_cpu.GetZeroFlag());
            Assert.IsFalse(_cpu.GetNegativeFlag());
            Assert.IsFalse(_cpu.GetCarryFlag());
            Assert.IsFalse(_cpu.GetHalfCarryFlag());
        }

        [Test]
        public void Test_RET_WhenCalled_ShouldReturnToCorrectAddress()
        {

            _cpu.SP = 0xFFFE;
            _memory.WriteWord(0xFFFE, 0x1234); 
            _memory.WriteByte(0x100, 0xC9);    
            _cpu.PC = 0x100;

            _cpu.Step(); 

            Assert.AreEqual(0x1234, _cpu.PC); 
            Assert.AreEqual(0x0000, _cpu.SP); 
        }

        [Test]
        public void Test_INC_B_ShouldIncrementBAndSetFlags()
        {

            _cpu.B = 0xFE;
            _memory.WriteByte(0x100, 0x04); 
            _cpu.PC = 0x100;

            _cpu.Step();

            Assert.AreEqual(0xFF, _cpu.B); 
            Assert.IsFalse(_cpu.GetZeroFlag()); 
            Assert.IsFalse(_cpu.GetNegativeFlag()); 
            Assert.IsTrue(_cpu.GetHalfCarryFlag()); 
        }

        [Test]
        public void Test_DEC_C_ShouldDecrementCAndSetFlags()
        {

            _cpu.C = 0x01;
            _memory.WriteByte(0x100, 0x0D); 
            _cpu.PC = 0x100;

            _cpu.Step();

            Assert.AreEqual(0x00, _cpu.C); 
            Assert.IsTrue(_cpu.GetZeroFlag()); 
            Assert.IsTrue(_cpu.GetNegativeFlag()); 
            Assert.IsFalse(_cpu.GetHalfCarryFlag()); 
        }

        [Test]
        public void Test_HALT_ShouldHaltCPU()
        {

            _memory.WriteByte(0x100, 0x76); 
            _cpu.PC = 0x100;

            _cpu.Step();

            Assert.IsTrue(_cpu.Halted); 
        }

        [Test]
        public void Test_AND_A_WithImmediateValue()
        {

            _cpu.A = 0xFF;
            _memory.WriteByte(0x100, 0xE6); 
            _memory.WriteByte(0x101, 0x0F); 
            _cpu.PC = 0x100;

            _cpu.Step();

            Assert.AreEqual(0x0F, _cpu.A); 
            Assert.IsFalse(_cpu.GetZeroFlag());
            Assert.IsFalse(_cpu.GetNegativeFlag());
            Assert.IsTrue(_cpu.GetHalfCarryFlag());
            Assert.IsFalse(_cpu.GetCarryFlag());
        }

        [Test]
        public void Test_RLCA_ShouldRotateLeftA()
        {

            _cpu.A = 0x85; 
            _memory.WriteByte(0x100, 0x07); 
            _cpu.PC = 0x100;

            _cpu.Step();

            Assert.AreEqual(0x0B, _cpu.A); 
            Assert.IsFalse(_cpu.GetZeroFlag());
            Assert.IsFalse(_cpu.GetNegativeFlag());
            Assert.IsFalse(_cpu.GetHalfCarryFlag());
            Assert.IsTrue(_cpu.GetCarryFlag()); 
        }

        [Test]
        public void Test_DEC_H_ShouldDecrementH()
        {

            _cpu.H = 0x01;
            _memory.WriteByte(0x100, 0x25); 
            _cpu.PC = 0x100;

            _cpu.Step();

            Assert.AreEqual(0x00, _cpu.H); 
            Assert.IsTrue(_cpu.GetZeroFlag()); 
            Assert.IsTrue(_cpu.GetNegativeFlag()); 
            Assert.IsFalse(_cpu.GetHalfCarryFlag()); 
        }

        [Test]
        public void Test_INC_L_ShouldIncrementL()
        {

            _cpu.L = 0x0F;
            _memory.WriteByte(0x100, 0x2C); 
            _cpu.PC = 0x100;

            _cpu.Step();

            Assert.AreEqual(0x10, _cpu.L); 
            Assert.IsFalse(_cpu.GetZeroFlag()); 
            Assert.IsFalse(_cpu.GetNegativeFlag()); 
            Assert.IsTrue(_cpu.GetHalfCarryFlag()); 
        }

        [Test]
        public void Test_ADD_HL_BC()
        {

            _cpu.SetHL(0x1234);
            _cpu.B = 0x12;
            _cpu.C = 0x34;
            _memory.WriteByte(0x100, 0x09); 
            _cpu.PC = 0x100;

            _cpu.Step();

            Assert.AreEqual(0x2468, _cpu.GetHL()); 
            Assert.IsFalse(_cpu.GetZeroFlag());
            Assert.IsFalse(_cpu.GetNegativeFlag());
            Assert.IsTrue(_cpu.GetHalfCarryFlag()); 
            Assert.IsFalse(_cpu.GetCarryFlag());
        }
        [Test]
        public void Test_ADC_A_ImmediateValue_WithCarry()
        {

            _cpu.A = 0x15;
            _cpu.SetCarryFlag(true); 
            _memory.WriteByte(0x100, 0xCE); 
            _memory.WriteByte(0x101, 0x05); 
            _cpu.PC = 0x100;

            _cpu.Step();

            Assert.AreEqual(0x1B, _cpu.A); 
            Assert.IsFalse(_cpu.GetZeroFlag());
            Assert.IsFalse(_cpu.GetNegativeFlag());
            Assert.IsFalse(_cpu.GetCarryFlag());
            Assert.IsFalse(_cpu.GetHalfCarryFlag());
        }

        [Test]
        public void Test_SBC_A_ImmediateValue_WithCarry()
        {

            _cpu.A = 0x20;
            _cpu.SetCarryFlag(true); 
            _memory.WriteByte(0x100, 0xDE); 
            _memory.WriteByte(0x101, 0x05); 
            _cpu.PC = 0x100;

            _cpu.Step();

            Assert.AreEqual(0x1A, _cpu.A); 
            Assert.IsFalse(_cpu.GetZeroFlag());
            Assert.IsTrue(_cpu.GetNegativeFlag());
            Assert.IsFalse(_cpu.GetCarryFlag());
            Assert.IsFalse(_cpu.GetHalfCarryFlag());
        }

        [Test]
        public void Test_LD_HL_PointerWithValueFromA()
        {

            _cpu.A = 0x77;
            _cpu.SetHL(0x2000);
            _memory.WriteByte(0x100, 0x77); 
            _cpu.PC = 0x100;

            _cpu.Step();

            Assert.AreEqual(0x77, _memory.ReadByte(0x2000)); 
        }

        [Test]
        public void Test_LD_A_AddressFromHL()
        {

            _cpu.SetHL(0x2000);
            _memory.WriteByte(0x2000, 0x99); 
            _memory.WriteByte(0x100, 0x7E);  
            _cpu.PC = 0x100;

            _cpu.Step();

            Assert.AreEqual(0x99, _cpu.A); 
        }

        [Test]
        public void Test_LD_A_BC()
        {

            _cpu.B = 0x12;
            _cpu.C = 0x34;
            _memory.WriteByte(0x1234, 0x56); 
            _memory.WriteByte(0x100, 0x0A);  
            _cpu.PC = 0x100;

            _cpu.Step();

            Assert.AreEqual(0x56, _cpu.A); 
        }

        [Test]
        public void Test_LD_A_DE()
        {

            _cpu.D = 0x22;
            _cpu.E = 0x33;
            _memory.WriteByte(0x2233, 0x77); 
            _memory.WriteByte(0x100, 0x1A);  
            _cpu.PC = 0x100;

            _cpu.Step();

            Assert.AreEqual(0x77, _cpu.A); 
        }

        [Test]
        public void Test_LD_BC_A()
        {

            _cpu.A = 0x88;
            _cpu.B = 0x11;
            _cpu.C = 0x22;
            _memory.WriteByte(0x100, 0x02);  
            _cpu.PC = 0x100;

            _cpu.Step();

            Assert.AreEqual(0x88, _memory.ReadByte(0x1122)); 
        }

        [Test]
        public void Test_LD_DE_A()
        {

            _cpu.A = 0x99;
            _cpu.D = 0x22;
            _cpu.E = 0x33;
            _memory.WriteByte(0x100, 0x12);  
            _cpu.PC = 0x100;

            _cpu.Step();

            Assert.AreEqual(0x99, _memory.ReadByte(0x2233)); 
        }

        [Test]
        public void Test_RRCA_ShouldRotateRightA()
        {

            _cpu.A = 0x81; 
            _memory.WriteByte(0x100, 0x0F); 
            _cpu.PC = 0x100;

            _cpu.Step();

            Assert.AreEqual(0xC0, _cpu.A); 
            Assert.IsFalse(_cpu.GetZeroFlag());
            Assert.IsFalse(_cpu.GetNegativeFlag());
            Assert.IsFalse(_cpu.GetHalfCarryFlag());
            Assert.IsTrue(_cpu.GetCarryFlag()); 
        }

        [Test]
        public void Test_RRA_ShouldRotateRightThroughCarry()
        {

            _cpu.A = 0x02; 
            _cpu.SetCarryFlag(true); 
            _memory.WriteByte(0x100, 0x1F); 
            _cpu.PC = 0x100;

            _cpu.Step();

            Assert.AreEqual(0x81, _cpu.A); 
            Assert.IsFalse(_cpu.GetZeroFlag());
            Assert.IsFalse(_cpu.GetNegativeFlag());
            Assert.IsFalse(_cpu.GetHalfCarryFlag());
            Assert.IsFalse(_cpu.GetCarryFlag()); 
        }

        [Test]
        public void Test_LD_SP_ImmediateAddress()
        {

            _cpu.SP = 0xFFFE;
            _memory.WriteWord(0x100, 0x1234); 
            _memory.WriteByte(0x100, 0x08);   
            _cpu.PC = 0x100;

            _cpu.Step();

            Assert.AreEqual(0xFFFE, _memory.ReadWord(0x1234)); 
        }

        [Test]
        public void Test_SCF_ShouldSetCarryFlag()
        {

            _cpu.ClearCarryFlag();
            _memory.WriteByte(0x100, 0x37); 
            _cpu.PC = 0x100;

            _cpu.Step();

            Assert.IsTrue(_cpu.GetCarryFlag()); 
            Assert.IsFalse(_cpu.GetNegativeFlag()); 
            Assert.IsFalse(_cpu.GetHalfCarryFlag()); 
        }

        [Test]
        public void Test_CCF_ShouldToggleCarryFlag()
        {

            _cpu.SetCarryFlag(true); 
            _memory.WriteByte(0x100, 0x3F); 
            _cpu.PC = 0x100;

            _cpu.Step();

            Assert.IsFalse(_cpu.GetCarryFlag()); 
            Assert.IsFalse(_cpu.GetNegativeFlag()); 
            Assert.IsFalse(_cpu.GetHalfCarryFlag()); 
        }

        [Test]
        public void Test_LD_A_DirectAddress()
        {

            _memory.WriteWord(0x100, 0x1234); 
            _memory.WriteByte(0x1234, 0x56); 
            _cpu.PC = 0x100;

            _cpu.Step();

            Assert.AreEqual(0x56, _cpu.A); 
        }

        [Test]
        public void Test_ADD_SP_Immediate()
        {

            _cpu.SP = 0xFFF8;
            _memory.WriteByte(0x100, 0xE8); 
            _memory.WriteByte(0x101, 0x08); 
            _cpu.PC = 0x100;

            _cpu.Step();

            Assert.AreEqual(0x10000, _cpu.SP); 
            Assert.IsFalse(_cpu.GetZeroFlag());
            Assert.IsFalse(_cpu.GetNegativeFlag());
        }


        [Test]
        public void Test_RET_Z_When_ZeroFlag_Not_Set_Should_Not_Return()
        {

            _cpu.SP = 0xFFFE;
            _memory.WriteWord(0xFFFE, 0x400); 
            _cpu.PC = 0x200;
            _cpu.SetZeroFlag(false); 
            _memory.WriteByte(0x200, 0xC8); 

            _cpu.Step(); 

            Assert.AreEqual(0x201, _cpu.PC); 
        }
    }
}
