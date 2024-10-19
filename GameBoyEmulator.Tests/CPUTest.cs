using GameBoyEmulator.Graphics;
using GameBoyEmulator.Interrupts;
using GameBoyEmulator.Memory;
using GameBoyEmulator.Processor;
using NUnit.Framework;

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
            _mmu.WriteByte(0x0100, 0x00); // NOP
            int cycles = _cpu.Step();
            Assert.AreEqual(4, cycles);
            Assert.AreEqual(0x0101, _registers.PC);
        }

        [Test]
        public void Test_CPU_LD_BC_d16()
        {
            _mmu.WriteByte(0x0100, 0x01); // LD BC,d16
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
            _mmu.WriteByte(0x0100, 0x03); // INC BC
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
            _mmu.WriteByte(0x0100, 0x0A); // LD A,(BC)
            int cycles = _cpu.Step();
            Assert.AreEqual(8, cycles);
            Assert.AreEqual(0x56, _registers.A);
            Assert.AreEqual(0x0101, _registers.PC);
        }

        [Test]
        public void Test_CPU_JP_NZ_a16()
        {
            _registers.SetZeroFlag(false);
            _mmu.WriteByte(0x0100, 0xC2); // JP NZ,a16
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
            _mmu.WriteByte(0x0100, 0xCA); // JP Z,a16
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
            _mmu.WriteByte(0x0100, 0xCA); // JP Z,a16
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
            _mmu.WriteByte(0x0100, 0xCD); // CALL a16
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
            _mmu.WriteByte(0x0100, 0xC9); // RET
            int cycles = _cpu.Step();
            Assert.AreEqual(16, cycles);
            Assert.AreEqual(0x0103, _registers.PC);
            Assert.AreEqual(0xFFFE, _registers.SP);
        }

        [Test]
        public void Test_CPU_LD_HL_SP_plus_n()
        {
            _registers.SP = 0xFFF8;
            _mmu.WriteByte(0x0100, 0xF8); // LD HL,SP+n
            _mmu.WriteByte(0x0101, 0x08); // n = +8
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
            _mmu.WriteByte(0x0100, 0xFF); // RST 38H
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
            _mmu.WriteByte(0x0100, 0xC6); // ADD A,n
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
            _mmu.WriteByte(0x0100, 0xC6); // ADD A,n
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
            _mmu.WriteByte(0x0100, 0xFE); // CP n
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
            _mmu.WriteByte(0x0100, 0xFE); // CP n
            _mmu.WriteByte(0x0101, 0x3C);
            int cycles = _cpu.Step();
            Assert.AreEqual(8, cycles);
            Assert.IsTrue(_registers.GetZeroFlag());
            Assert.IsTrue(_registers.GetNegativeFlag());
            Assert.IsFalse(_registers.GetHalfCarryFlag());
            Assert.IsFalse(_registers.GetCarryFlag());
        }

        [Test]
        public void Test_CPU_Interrupts_EnableDisable()
        {
            _registers.IME = false;
            _mmu.WriteByte(0x0100, 0xFB); // EI
            int cycles = _cpu.Step();
            Assert.AreEqual(4, cycles);
            Assert.IsFalse(_registers.IME); // IME is enabled after the next instruction

            _mmu.WriteByte(0x0101, 0x00); // NOP
            cycles = _cpu.Step();
            Assert.AreEqual(4, cycles);
            Assert.IsTrue(_registers.IME);
        }


        [Test]
        public void Test_CPU_Halt()
        {
            _mmu.WriteByte(0x0100, 0x76); // HALT
            int cycles = _cpu.Step();
            Assert.AreEqual(4, cycles); // HALT should consume 4 cycles
            Assert.IsTrue(_registers.Halted);
        }

        [Test]
        public void Test_CPU_LD_A_A()
        {
            _registers.A = 0x42;
            _mmu.WriteByte(0x0100, 0x7F); // LD A,A
            int cycles = _cpu.Step();
            Assert.AreEqual(4, cycles);
            Assert.AreEqual(0x42, _registers.A);
        }

        [Test]
        public void Test_CPU_InterruptHandling()
        {
            _registers.IME = true;
            _mmu.WriteByte(0xFFFF, 0x01); // Enable V-Blank interrupt
            _mmu.WriteByte(0xFF0F, 0x01); // Request V-Blank interrupt
            _mmu.WriteByte(0x0040, 0xC9); // RET at interrupt vector 0x0040
            int cycles = _cpu.Step();
            Assert.AreEqual(20, cycles);
            Assert.AreEqual(0x0040, _registers.PC);
            Assert.IsFalse(_registers.IME);
        }
    }
}
