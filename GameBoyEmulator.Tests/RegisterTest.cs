using GameBoyEmulator.Graphics;
using GameBoyEmulator.Interrupts;
using GameBoyEmulator.Memory;
using GameBoyEmulator.Processor;
using NUnit.Framework;
namespace GameBoyEmulator.Tests
{
    [TestFixture]
    public class RegisterTest
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
        public void Test_SubtractFromA()
        {
            _registers.A = 0x10;
            _registers.SubtractFromA(0x01);
            Assert.AreEqual(0x0F, _registers.A); 
            Assert.IsFalse(_registers.GetZeroFlag()); 
            Assert.IsTrue(_registers.GetNegativeFlag()); 
            Assert.IsTrue(_registers.GetHalfCarryFlag()); 
            Assert.IsFalse(_registers.GetCarryFlag()); 
        }
        [Test]
        public void Test_AddToA()
        {
            _registers.A = 0x0F;
            _registers.AddToA(0x01);
            Assert.AreEqual(0x10, _registers.A); 
            Assert.IsFalse(_registers.GetZeroFlag()); 
            Assert.IsFalse(_registers.GetNegativeFlag()); 
            Assert.IsTrue(_registers.GetHalfCarryFlag()); 
            Assert.IsFalse(_registers.GetCarryFlag()); 
        }
        [Test]
        public void Test_SbcFromA()
        {
            _registers.A = 0x10;
            _registers.SetCarryFlag(true); 
            _registers.SbcFromA(0x01);
            Assert.AreEqual(0x0E, _registers.A); 
            Assert.IsFalse(_registers.GetZeroFlag()); 
            Assert.IsTrue(_registers.GetNegativeFlag()); 
            Assert.IsTrue(_registers.GetHalfCarryFlag()); 
            Assert.IsFalse(_registers.GetCarryFlag()); 
        }
        [Test]
        public void Test_AndWithA()
        {
            _registers.A = 0xF0;
            _registers.AndWithA(0x0F);
            Assert.AreEqual(0x00, _registers.A); 
            Assert.IsTrue(_registers.GetZeroFlag()); 
            Assert.IsFalse(_registers.GetNegativeFlag()); 
            Assert.IsTrue(_registers.GetHalfCarryFlag()); 
            Assert.IsFalse(_registers.GetCarryFlag()); 
        }
        [Test]
        public void Test_XorWithA()
        {
            _registers.A = 0xFF;
            _registers.XorWithA(0xFF);
            Assert.AreEqual(0x00, _registers.A); 
            Assert.IsTrue(_registers.GetZeroFlag()); 
            Assert.IsFalse(_registers.GetNegativeFlag()); 
            Assert.IsFalse(_registers.GetHalfCarryFlag()); 
            Assert.IsFalse(_registers.GetCarryFlag()); 
        }
        [Test]
        public void Test_OrWithA()
        {
            _registers.A = 0x0F;
            _registers.OrWithA(0xF0);
            Assert.AreEqual(0xFF, _registers.A); 
            Assert.IsFalse(_registers.GetZeroFlag()); 
            Assert.IsFalse(_registers.GetNegativeFlag()); 
            Assert.IsFalse(_registers.GetHalfCarryFlag()); 
            Assert.IsFalse(_registers.GetCarryFlag()); 
        }
        [Test]
        public void Test_CompareA()
        {
            _registers.A = 0x10;
            _registers.CompareA(0x10);
            Assert.IsTrue(_registers.GetZeroFlag()); 
            Assert.IsTrue(_registers.GetNegativeFlag()); 
            Assert.IsFalse(_registers.GetHalfCarryFlag()); 
            Assert.IsFalse(_registers.GetCarryFlag()); 
        }
        [Test]
        public void Test_Increment()
        {
            byte result = _registers.Increment(0x0F);
            Assert.AreEqual(0x10, result); 
            Assert.IsFalse(_registers.GetZeroFlag()); 
            Assert.IsFalse(_registers.GetNegativeFlag()); 
            Assert.IsTrue(_registers.GetHalfCarryFlag()); 
        }
        [Test]
        public void Test_Decrement()
        {
            byte result = _registers.Decrement(0x10);
            Assert.AreEqual(0x0F, result); 
            Assert.IsFalse(_registers.GetZeroFlag()); 
            Assert.IsTrue(_registers.GetNegativeFlag()); 
            Assert.IsTrue(_registers.GetHalfCarryFlag()); 
        }
        [Test]
        public void Test_RotateLeft()
        {
            byte result = _registers.RotateLeft(0x80); 
            Assert.AreEqual(0x01, result); 
            Assert.IsFalse(_registers.GetZeroFlag()); 
            Assert.IsFalse(_registers.GetNegativeFlag()); 
            Assert.IsFalse(_registers.GetHalfCarryFlag()); 
            Assert.IsTrue(_registers.GetCarryFlag()); 
        }
        [Test]
        public void Test_RotateRight()
        {
            byte result = _registers.RotateRight(0x01); 
            Assert.AreEqual(0x80, result); 
            Assert.IsFalse(_registers.GetZeroFlag()); 
            Assert.IsFalse(_registers.GetNegativeFlag()); 
            Assert.IsFalse(_registers.GetHalfCarryFlag()); 
            Assert.IsTrue(_registers.GetCarryFlag()); 
        }
    }
}
