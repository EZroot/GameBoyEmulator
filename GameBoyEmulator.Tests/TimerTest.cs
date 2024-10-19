namespace GameBoyEmulator.Tests
{
    [TestFixture]
    public class TimerTests
    {
        private Timer _timer;
        private Memory _memory;

        [SetUp]
        public void Setup()
        {
            _memory = new Memory();
            _timer = new Timer(_memory);
        }

        [Test]
        public void Test_DIV_Register_Increment()
        {

            _memory.WriteByte(0xFF04, 0x00); 
            int cycles = 256; 

            _timer.Update(cycles);

            Assert.AreEqual(1, _memory.ReadByte(0xFF04)); 
        }

        [Test]
        public void Test_Timer_Enabled_ShouldIncrementTIMA()
        {

            _memory.WriteByte(0xFF07, 0x05); 
            _memory.WriteByte(0xFF05, 0x00); 
            int cycles = 1024; 

            _timer.Update(cycles);

            Assert.AreEqual(1, _memory.ReadByte(0xFF05)); 
        }

        [Test]
        public void Test_Timer_Disabled_ShouldNotIncrementTIMA()
        {

            _memory.WriteByte(0xFF07, 0x00); 
            _memory.WriteByte(0xFF05, 0x00); 
            int cycles = 1024; 

            _timer.Update(cycles);

            Assert.AreEqual(0, _memory.ReadByte(0xFF05)); 
        }

        [Test]
        public void Test_Timer_Overflow_ShouldResetTIMA_AndTriggerInterrupt()
        {

            _memory.WriteByte(0xFF07, 0x05); 
            _memory.WriteByte(0xFF05, 0xFF); 
            _memory.WriteByte(0xFF06, 0x42); 
            _memory.WriteByte(0xFF0F, 0x00); 
            int cycles = 1024; 

            _timer.Update(cycles);

            Assert.AreEqual(0x42, _memory.ReadByte(0xFF05)); 
            Assert.AreEqual(0x04, _memory.ReadByte(0xFF0F)); 
        }

        [Test]
        public void Test_Timer_DifferentFrequencies_ShouldIncrementAtCorrectRates()
        {

            var testCases = new[]
            {
            new { Tac = 0x04, Frequency = 4096, CyclesForIncrement = 1024 },   
            new { Tac = 0x05, Frequency = 262144, CyclesForIncrement = 16 },   
            new { Tac = 0x06, Frequency = 65536, CyclesForIncrement = 64 },    
            new { Tac = 0x07, Frequency = 16384, CyclesForIncrement = 256 }    
        };

            foreach (var testCase in testCases)
            {

                _memory.WriteByte(0xFF07, (byte)testCase.Tac); 
                _memory.WriteByte(0xFF05, 0x00); 
                int cycles = testCase.CyclesForIncrement; 

                _timer.Update(cycles);

                Assert.AreEqual(1, _memory.ReadByte(0xFF05)); 
            }
        }

        [Test]
        public void Test_DIV_Counter_ShouldNotAffectTIMA()
        {

            _memory.WriteByte(0xFF07, 0x04); 
            _memory.WriteByte(0xFF05, 0x00); 
            int divCycles = 256; 
            int timaCycles = 1024; 

            _timer.Update(divCycles); 
            Assert.AreEqual(0, _memory.ReadByte(0xFF05)); 

            _timer.Update(timaCycles - divCycles); 
            Assert.AreEqual(1, _memory.ReadByte(0xFF05)); 
        }
    }

}
