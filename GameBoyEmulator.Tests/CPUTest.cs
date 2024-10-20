using GameBoyEmulator.Graphics;
using GameBoyEmulator.Interrupts;
using GameBoyEmulator.Memory;
using GameBoyEmulator.Processor;
using NUnit.Framework;
using System.Diagnostics;
namespace GameBoyEmulator.Tests
{
    [TestFixture]
    public class CPUTests
    {
        private CPU _cpu;
        private Registers _registers;
        private Opcode _opcode;
        private RAM _ram;
        private MMU _mmu;
        private PPU _ppu;
        private Renderer _renderer;
        private InterruptController _interruptController;
        [SetUp]
        public void Setup()
        {
            _registers = new Registers();
            _ram = new RAM();
            _mmu = new MMU(_registers, _ram);
            _renderer = new Renderer(_mmu);
            _ppu = new PPU(_mmu, _renderer);
            _opcode = new Opcode(_registers, _mmu, _ram);
            _interruptController = new InterruptController(_registers, _mmu, _ram);
            _cpu = new CPU(_registers, _opcode, _ram, _interruptController);
            GameBoyEmulator.Debug.Debugger.EnableDebugModeForTests();
        }
        [Test]
        public void Test_DEC_B()
        {
            _registers.B = 0x01; 
            _registers.PC = 0x027B; 
            _mmu.WriteByte(0x027B, 0x05); 
            int cycles = _cpu.Step();
            Assert.AreEqual(4, cycles);
            Assert.AreEqual(0x00, _registers.B); 
            Assert.IsTrue(_registers.GetZeroFlag()); 
            Assert.IsTrue(_registers.GetNegativeFlag()); 
            Assert.IsFalse(_registers.GetHalfCarryFlag()); 
            Assert.AreEqual(0x027C, _registers.PC); 
        }
        [Test]
        public void Test_JR_NZ_r8_Taken()
        {
            _registers.SetZeroFlag(false); 
            _registers.PC = 0x027C; 
            _mmu.WriteByte(0x027C, 0x20); 
            _mmu.WriteByte(0x027D, 0xFE); 
            int cycles = _cpu.Step();
            Assert.AreEqual(12, cycles);
            sbyte v = unchecked((sbyte)0xFE);
            Assert.AreEqual(0x027C + v + 2, _registers.PC); 
        }
        [Test]
        public void Test_JR_NZ_r8_NotTaken()
        {
            _registers.SetZeroFlag(true); 
            _registers.PC = 0x027C; 
            _mmu.WriteByte(0x027C, 0x20); 
            _mmu.WriteByte(0x027D, 0x02); 
            int cycles = _cpu.Step();
            Assert.AreEqual(8, cycles);
            Assert.AreEqual(0x027E, _registers.PC); 
        }
        [Test]
        public void Test_LD_HLm_A()
        {
            _registers.A = 0x42; 
            _registers.SetHL(0x1234); 
            _registers.PC = 0x027A; 
            _mmu.WriteByte(0x027A, 0x32); 
            int cycles = _cpu.Step();
            Assert.AreEqual(8, cycles);
            Assert.AreEqual(0x42, _ram.ReadByte(0x1234)); 
            Assert.AreEqual(0x1233, _registers.GetHL()); 
            Assert.AreEqual(0x027B, _registers.PC); 
        }
        [Test]
        public void Test_CPU_InitialState()
        {
            Assert.AreEqual(0x0100, _registers.PC);
            Assert.AreEqual(0xFFFE, _registers.SP);
            Assert.AreEqual(0x01, _registers.A);
            Assert.AreEqual(0xB0, _registers.F);
            Assert.AreEqual(0x00, _registers.B);
            Assert.AreEqual(0x13, _registers.C);
            Assert.AreEqual(0x00, _registers.D);
            Assert.AreEqual(0xD8, _registers.E);
            Assert.AreEqual(0x01, _registers.H);
            Assert.AreEqual(0x4D, _registers.L);
            Assert.IsTrue(_registers.IME);
        }
        [Test]
        public void Test_CPU_NOP()
        {
            _mmu.WriteByte(0x0100, 0x00); 
            int cycles = _cpu.Step();
            Assert.AreEqual(4, cycles);
            Assert.AreEqual(0x0101, _registers.PC);
        }
        [Test]
        public void Test_CPU_LD_BC_d16()
        {
            _mmu.WriteByte(0x0100, 0x01); 
            _mmu.WriteByte(0x0101, 0x34);
            _mmu.WriteByte(0x0102, 0x12);
            int cycles = _cpu.Step();
            Assert.AreEqual(12, cycles);
            Assert.AreEqual(0x1234, _registers.GetBC());
            Assert.AreEqual(0x0103, _registers.PC);
        }
        [Test]
        public void Test_CPU_INC_BC()
        {
            _registers.SetBC(0x1234);
            _mmu.WriteByte(0x0100, 0x03); 
            int cycles = _cpu.Step();
            Assert.AreEqual(8, cycles);
            Assert.AreEqual(0x1235, _registers.GetBC());
            Assert.AreEqual(0x0101, _registers.PC);
        }
        [Test]
        public void Test_CPU_LD_A_mBC()
        {
            _registers.SetBC(0x1234);
            _mmu.WriteByte(0x1234, 0x56);
            _mmu.WriteByte(0x0100, 0x0A); 
            int cycles = _cpu.Step();
            Assert.AreEqual(8, cycles);
            Assert.AreEqual(0x56, _registers.A);
            Assert.AreEqual(0x0101, _registers.PC);
        }
        [Test]
        public void Test_CPU_JP_NZ_a16()
        {
            _registers.SetZeroFlag(false);
            _mmu.WriteByte(0x0100, 0xC2); 
            _mmu.WriteByte(0x0101, 0x00);
            _mmu.WriteByte(0x0102, 0xC0);
            int cycles = _cpu.Step();
            Assert.AreEqual(16, cycles);
            Assert.AreEqual(0xC000, _registers.PC);
        }
        [Test]
        public void Test_CPU_JP_Z_a16()
        {
            _registers.SetZeroFlag(true);
            _mmu.WriteByte(0x0100, 0xCA); 
            _mmu.WriteByte(0x0101, 0x00);
            _mmu.WriteByte(0x0102, 0xC0);
            int cycles = _cpu.Step();
            Assert.AreEqual(16, cycles);
            Assert.AreEqual(0xC000, _registers.PC);
        }
        [Test]
        public void Test_CPU_JP_Z_a16_NotTaken()
        {
            _registers.SetZeroFlag(false);
            _mmu.WriteByte(0x0100, 0xCA); 
            _mmu.WriteByte(0x0101, 0x00);
            _mmu.WriteByte(0x0102, 0xC0);
            int cycles = _cpu.Step();
            Assert.AreEqual(12, cycles);
            Assert.AreEqual(0x0103, _registers.PC);
        }
        [Test]
        public void Test_CPU_Call()
        {
            _registers.SP = 0xFFFE;
            _mmu.WriteByte(0x0100, 0xCD); 
            _mmu.WriteByte(0x0101, 0x34);
            _mmu.WriteByte(0x0102, 0x12);
            int cycles = _cpu.Step();
            Assert.AreEqual(24, cycles);
            Assert.AreEqual(0x1234, _registers.PC);
            Assert.AreEqual(0xFFFC, _registers.SP);
            ushort returnAddress = (ushort)(_mmu.ReadByte(0xFFFC) | (_mmu.ReadByte(0xFFFD) << 8));
            Assert.AreEqual(0x0103, returnAddress);
        }
        [Test]
        public void Test_CPU_RET()
        {
            _registers.SP = 0xFFFC;
            _mmu.WriteByte(0xFFFC, 0x03);
            _mmu.WriteByte(0xFFFD, 0x01);
            _mmu.WriteByte(0x0100, 0xC9); 
            int cycles = _cpu.Step();
            Assert.AreEqual(16, cycles);
            Assert.AreEqual(0x0103, _registers.PC);
            Assert.AreEqual(0xFFFE, _registers.SP);
        }
        [Test]
        public void Test_CPU_LD_HL_SP_plus_n()
        {
            _registers.SP = 0xFFF8;
            _mmu.WriteByte(0x0100, 0xF8); 
            _mmu.WriteByte(0x0101, 0x08); 
            int cycles = _cpu.Step();
            Assert.AreEqual(12, cycles);
            Assert.AreEqual(0x0000, _registers.GetHL());
            Assert.IsFalse(_registers.GetZeroFlag());
            Assert.IsFalse(_registers.GetNegativeFlag());
            Assert.IsTrue(_registers.GetHalfCarryFlag());
            Assert.IsTrue(_registers.GetCarryFlag());
        }
        [Test]
        public void Test_CPU_RST_38H()
        {
            _registers.SP = 0xFFFE;
            _mmu.WriteByte(0x0100, 0xFF); 
            int cycles = _cpu.Step();
            Assert.AreEqual(16, cycles);
            Assert.AreEqual(0x0038, _registers.PC);
            Assert.AreEqual(0xFFFC, _registers.SP);
            ushort returnAddress = (ushort)(_mmu.ReadByte(0xFFFC) | (_mmu.ReadByte(0xFFFD) << 8));
            Assert.AreEqual(0x0101, returnAddress);
        }
        [Test]
        public void Test_CPU_ADD_A_n()
        {
            _registers.A = 0x14;
            _mmu.WriteByte(0x0100, 0xC6); 
            _mmu.WriteByte(0x0101, 0x42);
            int cycles = _cpu.Step();
            Assert.AreEqual(8, cycles);
            Assert.AreEqual(0x56, _registers.A);
            Assert.IsFalse(_registers.GetZeroFlag());
            Assert.IsFalse(_registers.GetNegativeFlag());
            Assert.IsFalse(_registers.GetHalfCarryFlag());
            Assert.IsFalse(_registers.GetCarryFlag());
        }
        [Test]
        public void Test_CPU_ADD_A_n_WithCarry()
        {
            _registers.A = 0xFF;
            _mmu.WriteByte(0x0100, 0xC6); 
            _mmu.WriteByte(0x0101, 0x01);
            int cycles = _cpu.Step();
            Assert.AreEqual(8, cycles);
            Assert.AreEqual(0x00, _registers.A);
            Assert.IsTrue(_registers.GetZeroFlag());
            Assert.IsFalse(_registers.GetNegativeFlag());
            Assert.IsTrue(_registers.GetHalfCarryFlag());
            Assert.IsTrue(_registers.GetCarryFlag());
        }
        [Test]
        public void Test_CPU_CP_n()
        {
            _registers.A = 0x3C;
            _mmu.WriteByte(0x0100, 0xFE); 
            _mmu.WriteByte(0x0101, 0x2F);
            int cycles = _cpu.Step();
            Assert.AreEqual(8, cycles);
            Assert.IsFalse(_registers.GetZeroFlag());
            Assert.IsTrue(_registers.GetNegativeFlag());
            Assert.IsTrue(_registers.GetHalfCarryFlag());
            Assert.IsFalse(_registers.GetCarryFlag());
        }
        [Test]
        public void Test_CPU_CP_n_Equal()
        {
            _registers.A = 0x3C;
            _mmu.WriteByte(0x0100, 0xFE); 
            _mmu.WriteByte(0x0101, 0x3C);
            int cycles = _cpu.Step();
            Assert.AreEqual(8, cycles);
            Assert.IsTrue(_registers.GetZeroFlag());
            Assert.IsTrue(_registers.GetNegativeFlag());
            Assert.IsFalse(_registers.GetHalfCarryFlag());
            Assert.IsFalse(_registers.GetCarryFlag());
        }
        [Test]
        public void Test_CPU_Halt()
        {
            _mmu.WriteByte(0x0100, 0x76); 
            int cycles = _cpu.Step();
            Assert.AreEqual(4, cycles); 
            Assert.IsTrue(_registers.Halted);
        }
        [Test]
        public void Test_CPU_LD_A_A()
        {
            _registers.A = 0x42;
            _mmu.WriteByte(0x0100, 0x7F); 
            int cycles = _cpu.Step();
            Assert.AreEqual(4, cycles);
            Assert.AreEqual(0x42, _registers.A);
        }
        [Test]
        public void Test_CPU_InterruptHandling()
        {
            _registers.IME = true;
            _mmu.WriteByte(0xFFFF, 0x01); 
            _mmu.WriteByte(0xFF0F, 0x01); 
            _mmu.WriteByte(0x0040, 0xC9); 
            int cycles = _cpu.Step();
            Assert.AreEqual(20, cycles);
            Assert.AreEqual(0x0040, _registers.PC);
            Assert.IsFalse(_registers.IME);
        }
        [Test]
        public void Test_CPU_DEC_B()
        {
            _registers.B = 0x01;
            _mmu.WriteByte(0x0100, 0x05); 
            int cycles = _cpu.Step();
            Assert.AreEqual(4, cycles);
            Assert.AreEqual(0x00, _registers.B);
            Assert.IsTrue(_registers.GetZeroFlag());
            Assert.IsTrue(_registers.GetNegativeFlag());
            Assert.IsFalse(_registers.GetHalfCarryFlag());
            Assert.AreEqual(0x0101, _registers.PC);
        }
        [Test]
        public void Test_CPU_LD_HL_d16()
        {
            _mmu.WriteByte(0x0100, 0x21); 
            _mmu.WriteByte(0x0101, 0xCD);
            _mmu.WriteByte(0x0102, 0xAB);
            int cycles = _cpu.Step();
            Assert.AreEqual(12, cycles);
            Assert.AreEqual(0xABCD, _registers.GetHL());
            Assert.AreEqual(0x0103, _registers.PC);
        }
        [Test]
        public void Test_CPU_INC_HL()
        {
            _registers.SetHL(0x1234);
            _mmu.WriteByte(0x0100, 0x23); 
            int cycles = _cpu.Step();
            Assert.AreEqual(8, cycles);
            Assert.AreEqual(0x1235, _registers.GetHL());
            Assert.AreEqual(0x0101, _registers.PC);
        }
        [Test]
        public void Test_CPU_JR_NZ()
        {
            _registers.SetZeroFlag(false);
            _mmu.WriteByte(0x0100, 0x20); 
            _mmu.WriteByte(0x0101, 0x02); 
            int cycles = _cpu.Step();
            Assert.AreEqual(12, cycles);
            Assert.AreEqual(0x0104, _registers.PC);
        }
        [Test]
        public void Test_CPU_JR_Z_NotTaken()
        {
            _registers.SetZeroFlag(false);
            _mmu.WriteByte(0x0100, 0x28); 
            _mmu.WriteByte(0x0101, 0x02); 
            int cycles = _cpu.Step();
            Assert.AreEqual(8, cycles);
            Assert.AreEqual(0x0102, _registers.PC);
        }
        [Test]
        public void Test_CPU_LD_HL_A_Increment()
        {
            _registers.SetHL(0xC000);
            _registers.A = 0x42;
            _mmu.WriteByte(0x0100, 0x22); 
            int cycles = _cpu.Step();
            Assert.AreEqual(8, cycles);
            Assert.AreEqual(0x42, _ram.ReadByte(0xC000));
            Assert.AreEqual(0xC001, _registers.GetHL());
            Assert.AreEqual(0x0101, _registers.PC);
        }
        [Test]
        public void Test_CPU_SBC_A_A()
        {
            _registers.A = 0x01;
            _registers.SetCarryFlag(true);
            _mmu.WriteByte(0x0100, 0x9F); 
            int cycles = _cpu.Step();
            Assert.AreEqual(4, cycles);
            Assert.AreEqual(0xFF, _registers.A);
            Assert.IsFalse(_registers.GetZeroFlag());
            Assert.IsTrue(_registers.GetNegativeFlag());
            Assert.IsTrue(_registers.GetHalfCarryFlag());
            Assert.IsTrue(_registers.GetCarryFlag());
            Assert.AreEqual(0x0101, _registers.PC);
        }
        [Test]
        public void Test_CPU_AND_d8()
        {
            _registers.A = 0xF0;
            _mmu.WriteByte(0x0100, 0xE6); 
            _mmu.WriteByte(0x0101, 0x0F); 
            int cycles = _cpu.Step();
            Assert.AreEqual(8, cycles);
            Assert.AreEqual(0x00, _registers.A);
            Assert.IsTrue(_registers.GetZeroFlag());
            Assert.IsFalse(_registers.GetNegativeFlag());
            Assert.IsTrue(_registers.GetHalfCarryFlag());
            Assert.IsFalse(_registers.GetCarryFlag());
            Assert.AreEqual(0x0102, _registers.PC);
        }
        [Test]
        public void Test_CPU_OR_d8()
        {
            _registers.A = 0xF0;
            _mmu.WriteByte(0x0100, 0xF6); 
            _mmu.WriteByte(0x0101, 0x0F); 
            int cycles = _cpu.Step();
            Assert.AreEqual(8, cycles);
            Assert.AreEqual(0xFF, _registers.A);
            Assert.IsFalse(_registers.GetZeroFlag());
            Assert.IsFalse(_registers.GetNegativeFlag());
            Assert.IsFalse(_registers.GetHalfCarryFlag());
            Assert.IsFalse(_registers.GetCarryFlag());
            Assert.AreEqual(0x0102, _registers.PC);
        }
        [Test]
        public void Test_CPU_XOR_d8()
        {
            _registers.A = 0xFF;
            _mmu.WriteByte(0x0100, 0xEE); 
            _mmu.WriteByte(0x0101, 0xFF); 
            int cycles = _cpu.Step();
            Assert.AreEqual(8, cycles);
            Assert.AreEqual(0x00, _registers.A);
            Assert.IsTrue(_registers.GetZeroFlag());
            Assert.IsFalse(_registers.GetNegativeFlag());
            Assert.IsFalse(_registers.GetHalfCarryFlag());
            Assert.IsFalse(_registers.GetCarryFlag());
            Assert.AreEqual(0x0102, _registers.PC);
        }
        [Test]
        public void Test_CPU_CP_d8()
        {
            _registers.A = 0x42;
            _mmu.WriteByte(0x0100, 0xFE); 
            _mmu.WriteByte(0x0101, 0x42); 
            int cycles = _cpu.Step();
            Assert.AreEqual(8, cycles);
            Assert.IsTrue(_registers.GetZeroFlag());
            Assert.IsTrue(_registers.GetNegativeFlag());
            Assert.IsFalse(_registers.GetHalfCarryFlag());
            Assert.IsFalse(_registers.GetCarryFlag());
            Assert.AreEqual(0x0102, _registers.PC);
        }
        [Test]
        public void Test_CPU_INC_A()
        {
            _registers.A = 0xFF;
            _mmu.WriteByte(0x0100, 0x3C); 
            int cycles = _cpu.Step();
            Assert.AreEqual(4, cycles);
            Assert.AreEqual(0x00, _registers.A);
            Assert.IsTrue(_registers.GetZeroFlag());
            Assert.IsFalse(_registers.GetNegativeFlag());
            Assert.IsTrue(_registers.GetHalfCarryFlag());
            Assert.AreEqual(0x0101, _registers.PC);
        }
        [Test]
        public void Test_CPU_DEC_A()
        {
            _registers.A = 0x01;
            _mmu.WriteByte(0x0100, 0x3D); 
            int cycles = _cpu.Step();
            Assert.AreEqual(4, cycles);
            Assert.AreEqual(0x00, _registers.A);
            Assert.IsTrue(_registers.GetZeroFlag());
            Assert.IsTrue(_registers.GetNegativeFlag());
            Assert.IsFalse(_registers.GetHalfCarryFlag());
            Assert.AreEqual(0x0101, _registers.PC);
        }
        [Test]
        public void Test_CPU_JP_a16()
        {
            _mmu.WriteByte(0x0100, 0xC3); 
            _ram.WriteWord(0x0101, 0x1234); 
            int cycles = _cpu.Step();
            Assert.AreEqual(16, cycles);
            Assert.AreEqual(0x1234, _registers.PC);
        }
        [Test]
        public void Test_CPU_CALL_a16()
        {
            _registers.SP = 0xFFFE;
            _mmu.WriteByte(0x0100, 0xCD); 
            _ram.WriteWord(0x0101, 0x1234); 
            int cycles = _cpu.Step();
            Assert.AreEqual(24, cycles);
            Assert.AreEqual(0x1234, _registers.PC);
            Assert.AreEqual(0xFFFC, _registers.SP);
            ushort returnAddress = _mmu.PopStack();
            Assert.AreEqual(0x0103, returnAddress);
        }
        [Test]
        public void Test_CPU_RST_00H()
        {
            _registers.SP = 0xFFFE;
            _mmu.WriteByte(0x0100, 0xC7); 
            int cycles = _cpu.Step();
            Assert.AreEqual(16, cycles);
            Assert.AreEqual(0x0000, _registers.PC);
            Assert.AreEqual(0xFFFC, _registers.SP);
            ushort returnAddress = _mmu.PopStack();
            Assert.AreEqual(0x0101, returnAddress);
        }
        [Test]
        public void Test_CPU_PUSH_AF()
        {
            _registers.A = 0x12;
            _registers.F = 0x34;
            _registers.SP = 0xFFFE;
            _mmu.WriteByte(0x0100, 0xF5); 
            int cycles = _cpu.Step();
            Assert.AreEqual(16, cycles);
            Assert.AreEqual(0xFFFC, _registers.SP);
            ushort value = _mmu.PopStack();
            Assert.AreEqual(0x1230, value); 
            Assert.AreEqual(0x0101, _registers.PC);
        }
        [Test]
        public void Test_CPU_LD_A_n()
        {
            _mmu.WriteByte(0x0100, 0x3E); 
            _mmu.WriteByte(0x0101, 0x42); 
            int cycles = _cpu.Step();
            Assert.AreEqual(8, cycles);
            Assert.AreEqual(0x42, _registers.A);
            Assert.AreEqual(0x0102, _registers.PC);
        }
        [Test]
        public void Test_CPU_LD_B_n()
        {
            _mmu.WriteByte(0x0100, 0x06); 
            _mmu.WriteByte(0x0101, 0x99); 
            int cycles = _cpu.Step();
            Assert.AreEqual(8, cycles);
            Assert.AreEqual(0x99, _registers.B);
            Assert.AreEqual(0x0102, _registers.PC);
        }
        [Test]
        public void Test_CPU_LD_C_n()
        {
            _mmu.WriteByte(0x0100, 0x0E); 
            _mmu.WriteByte(0x0101, 0x88); 
            int cycles = _cpu.Step();
            Assert.AreEqual(8, cycles);
            Assert.AreEqual(0x88, _registers.C);
            Assert.AreEqual(0x0102, _registers.PC);
        }
        [Test]
        public void Test_CPU_LD_D_n()
        {
            _mmu.WriteByte(0x0100, 0x16); 
            _mmu.WriteByte(0x0101, 0x77); 
            int cycles = _cpu.Step();
            Assert.AreEqual(8, cycles);
            Assert.AreEqual(0x77, _registers.D);
            Assert.AreEqual(0x0102, _registers.PC);
        }
        [Test]
        public void Test_CPU_LD_E_n()
        {
            _mmu.WriteByte(0x0100, 0x1E); 
            _mmu.WriteByte(0x0101, 0x66); 
            int cycles = _cpu.Step();
            Assert.AreEqual(8, cycles);
            Assert.AreEqual(0x66, _registers.E);
            Assert.AreEqual(0x0102, _registers.PC);
        }
        [Test]
        public void Test_CPU_LD_H_n()
        {
            _mmu.WriteByte(0x0100, 0x26); 
            _mmu.WriteByte(0x0101, 0x55); 
            int cycles = _cpu.Step();
            Assert.AreEqual(8, cycles);
            Assert.AreEqual(0x55, _registers.H);
            Assert.AreEqual(0x0102, _registers.PC);
        }
        [Test]
        public void Test_CPU_LD_L_n()
        {
            _mmu.WriteByte(0x0100, 0x2E); 
            _mmu.WriteByte(0x0101, 0x44); 
            int cycles = _cpu.Step();
            Assert.AreEqual(8, cycles);
            Assert.AreEqual(0x44, _registers.L);
            Assert.AreEqual(0x0102, _registers.PC);
        }
        [Test]
        public void Test_CPU_LD_mHL_n()
        {
            _registers.SetHL(0xC000);
            _mmu.WriteByte(0x0100, 0x36); 
            _mmu.WriteByte(0x0101, 0x33); 
            int cycles = _cpu.Step();
            Assert.AreEqual(12, cycles);
            Assert.AreEqual(0x33, _ram.ReadByte(0xC000));
            Assert.AreEqual(0x0102, _registers.PC);
        }
        [Test]
        public void Test_CPU_LD_mHL_A()
        {
            _registers.SetHL(0xC000);
            _registers.A = 0x22;
            _mmu.WriteByte(0x0100, 0x77); 
            int cycles = _cpu.Step();
            Assert.AreEqual(8, cycles);
            Assert.AreEqual(0x22, _ram.ReadByte(0xC000));
            Assert.AreEqual(0x0101, _registers.PC);
        }
        [Test]
        public void Test_CPU_LD_A_mHL()
        {
            _registers.SetHL(0xC000);
            _ram.WriteByte(0xC000, 0x11);
            _mmu.WriteByte(0x0100, 0x7E); 
            int cycles = _cpu.Step();
            Assert.AreEqual(8, cycles);
            Assert.AreEqual(0x11, _registers.A);
            Assert.AreEqual(0x0101, _registers.PC);
        }
        [Test]
        public void Test_CPU_INC_B()
        {
            _registers.B = 0x0F;
            _mmu.WriteByte(0x0100, 0x04); 
            int cycles = _cpu.Step();
            Assert.AreEqual(4, cycles);
            Assert.AreEqual(0x10, _registers.B);
            Assert.IsFalse(_registers.GetZeroFlag());
            Assert.IsFalse(_registers.GetNegativeFlag());
            Assert.IsTrue(_registers.GetHalfCarryFlag());
            Assert.AreEqual(0x0101, _registers.PC);
        }
        [Test]
        public void Test_CPU_DEC_C()
        {
            _registers.C = 0x10;
            _mmu.WriteByte(0x0100, 0x0D); 
            int cycles = _cpu.Step();
            Assert.AreEqual(4, cycles);
            Assert.AreEqual(0x0F, _registers.C);
            Assert.IsFalse(_registers.GetZeroFlag());
            Assert.IsTrue(_registers.GetNegativeFlag());
            Assert.IsTrue(_registers.GetHalfCarryFlag()); 
            Assert.AreEqual(0x0101, _registers.PC);
        }
        [Test]
        public void Test_CPU_LD_mnn_SP()
        {
            _registers.SP = 0xFFF8;
            _mmu.WriteByte(0x0100, 0x08); 
            _mmu.WriteByte(0x0101, 0x00); 
            _mmu.WriteByte(0x0102, 0xC0); 
            int cycles = _cpu.Step();
            Assert.AreEqual(20, cycles);
            Assert.AreEqual(0xF8, _ram.ReadByte(0xC000));
            Assert.AreEqual(0xFF, _ram.ReadByte(0xC001));
            Assert.AreEqual(0x0103, _registers.PC);
        }
        [Test]
        public void Test_CPU_LD_SP_HL()
        {
            _registers.SetHL(0x1234);
            _mmu.WriteByte(0x0100, 0xF9); 
            int cycles = _cpu.Step();
            Assert.AreEqual(8, cycles);
            Assert.AreEqual(0x1234, _registers.SP);
            Assert.AreEqual(0x0101, _registers.PC);
        }
        [Test]
        public void Test_CPU_LD_A_mFF00_plus_n()
        {
            _ram.WriteByte(0xFF80, 0xAB);
            _mmu.WriteByte(0x0100, 0xF0); 
            _mmu.WriteByte(0x0101, 0x80); 
            int cycles = _cpu.Step();
            Assert.AreEqual(12, cycles);
            Assert.AreEqual(0xAB, _registers.A);
            Assert.AreEqual(0x0102, _registers.PC);
        }
        [Test]
        public void Test_CPU_LD_mFF00_plus_n_A()
        {
            _registers.A = 0xCD;
            _mmu.WriteByte(0x0100, 0xE0); 
            _mmu.WriteByte(0x0101, 0x80); 
            int cycles = _cpu.Step();
            Assert.AreEqual(12, cycles);
            Assert.AreEqual(0xCD, _ram.ReadByte(0xFF80));
            Assert.AreEqual(0x0102, _registers.PC);
        }
        [Test]
        public void Test_CPU_JR_C_r8()
        {
            _registers.SetCarryFlag(true);
            _mmu.WriteByte(0x0100, 0x38); 
            _mmu.WriteByte(0x0101, 0x05); 
            int cycles = _cpu.Step();
            Assert.AreEqual(12, cycles);
            Assert.AreEqual(0x0107, _registers.PC);
        }
        [Test]
        public void Test_CPU_INC_mHL()
        {
            _registers.SetHL(0xC000);
            _ram.WriteByte(0xC000, 0xFF);
            _mmu.WriteByte(0x0100, 0x34); 
            int cycles = _cpu.Step();
            Assert.AreEqual(12, cycles);
            Assert.AreEqual(0x00, _ram.ReadByte(0xC000));
            Assert.IsTrue(_registers.GetZeroFlag());
            Assert.IsFalse(_registers.GetNegativeFlag());
            Assert.IsTrue(_registers.GetHalfCarryFlag());
            Assert.AreEqual(0x0101, _registers.PC);
        }
        [Test]
        public void Test_CPU_DEC_mHL()
        {
            _registers.SetHL(0xC000);
            _ram.WriteByte(0xC000, 0x01);
            _mmu.WriteByte(0x0100, 0x35); 
            int cycles = _cpu.Step();
            Assert.AreEqual(12, cycles);
            Assert.AreEqual(0x00, _ram.ReadByte(0xC000));
            Assert.IsTrue(_registers.GetZeroFlag());
            Assert.IsTrue(_registers.GetNegativeFlag());
            Assert.IsFalse(_registers.GetHalfCarryFlag());
            Assert.AreEqual(0x0101, _registers.PC);
        }
        [Test]
        public void Test_CPU_SCF()
        {
            _registers.SetCarryFlag(false);
            _mmu.WriteByte(0x0100, 0x37); 
            int cycles = _cpu.Step();
            Assert.AreEqual(4, cycles);
            Assert.IsFalse(_registers.GetNegativeFlag());
            Assert.IsFalse(_registers.GetHalfCarryFlag());
            Assert.IsTrue(_registers.GetCarryFlag());
            Assert.AreEqual(0x0101, _registers.PC);
        }
        [Test]
        public void Test_CPU_CCF()
        {
            _registers.SetCarryFlag(true);
            _mmu.WriteByte(0x0100, 0x3F); 
            int cycles = _cpu.Step();
            Assert.AreEqual(4, cycles);
            Assert.IsFalse(_registers.GetNegativeFlag());
            Assert.IsFalse(_registers.GetHalfCarryFlag());
            Assert.IsFalse(_registers.GetCarryFlag());
            Assert.AreEqual(0x0101, _registers.PC);
        }
        [Test]
        public void Test_CPU_RLA()
        {
            _registers.A = 0x80;
            _registers.SetCarryFlag(true);
            _mmu.WriteByte(0x0100, 0x17); 
            int cycles = _cpu.Step();
            Assert.AreEqual(4, cycles);
            Assert.AreEqual(0x01, _registers.A);
            Assert.IsFalse(_registers.GetZeroFlag());
            Assert.IsFalse(_registers.GetNegativeFlag());
            Assert.IsFalse(_registers.GetHalfCarryFlag());
            Assert.IsTrue(_registers.GetCarryFlag());
            Assert.AreEqual(0x0101, _registers.PC);
        }
        [Test]
        public void Test_CPU_RRA()
        {
            _registers.A = 0x01;
            _registers.SetCarryFlag(true);
            _mmu.WriteByte(0x0100, 0x1F); 
            int cycles = _cpu.Step();
            Assert.AreEqual(4, cycles);
            Assert.AreEqual(0x80, _registers.A);
            Assert.IsFalse(_registers.GetZeroFlag());
            Assert.IsFalse(_registers.GetNegativeFlag());
            Assert.IsFalse(_registers.GetHalfCarryFlag());
            Assert.IsTrue(_registers.GetCarryFlag());
            Assert.AreEqual(0x0101, _registers.PC);
        }
        [Test]
        public void Test_CPU_PUSH_BC()
        {
            _registers.SetBC(0x1234);
            _registers.SP = 0xFFFE;
            _mmu.WriteByte(0x0100, 0xC5); 
            int cycles = _cpu.Step();
            Assert.AreEqual(16, cycles);
            Assert.AreEqual(0xFFFC, _registers.SP);
            ushort value = _mmu.PopStack();
            Assert.AreEqual(0x1234, value);
            Assert.AreEqual(0x0101, _registers.PC);
        }
        [Test]
        public void Test_CPU_DI()
        {
            _registers.IME = true;
            _mmu.WriteByte(0x0100, 0xF3); 
            int cycles = _cpu.Step();
            Assert.AreEqual(4, cycles);
            Assert.IsFalse(_registers.IME);
            Assert.AreEqual(0x0101, _registers.PC);
        }
        [Test]
        public void Test_CPU_CPL()
        {
            _registers.A = 0x0F;
            _mmu.WriteByte(0x0100, 0x2F); 
            int cycles = _cpu.Step();
            Assert.AreEqual(4, cycles);
            Assert.AreEqual(0xF0, _registers.A);
            Assert.IsTrue(_registers.GetNegativeFlag());
            Assert.IsTrue(_registers.GetHalfCarryFlag());
            Assert.AreEqual(0x0101, _registers.PC);
        }
    }
}
