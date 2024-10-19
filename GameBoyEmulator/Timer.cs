namespace GameBoyEmulator
{
    public class Timer
    {
        private int _divCounter;
        private int _timaCounter;
        private const int ClockSpeed = 4194304; 
        private Memory _memory;
        public Timer(Memory memory)
        {
            _memory = memory;
        }
        public void Update(int cycles)
        {
            _divCounter += cycles;
            if (_divCounter >= ClockSpeed / 16384)
            {
                _memory.WriteByte(0xFF04, (byte)(_memory.ReadByte(0xFF04) + 1)); 
                _divCounter -= ClockSpeed / 16384;
            }
            byte tac = _memory.ReadByte(0xFF07);
            if ((tac & 0x04) != 0) 
            {
                int frequency = GetTimerFrequency(tac & 0x03);
                _timaCounter += cycles;
                if (_timaCounter >= frequency)
                {
                    _timaCounter -= frequency;
                    byte tima = _memory.ReadByte(0xFF05);
                    if (tima == 0xFF)
                    {
                        _memory.WriteByte(0xFF05, _memory.ReadByte(0xFF06));
                        byte ifFlag = _memory.ReadByte(0xFF0F);
                        ifFlag |= 0x04; 
                        _memory.WriteByte(0xFF0F, ifFlag);
                    }
                    else
                    {
                        _memory.WriteByte(0xFF05, (byte)(tima + 1));
                    }
                }
            }
        }
        private int GetTimerFrequency(int tacSetting)
        {
            switch (tacSetting)
            {
                case 0: return ClockSpeed / 4096;
                case 1: return ClockSpeed / 262144;
                case 2: return ClockSpeed / 65536;
                case 3: return ClockSpeed / 16384;
                default: throw new ArgumentOutOfRangeException();
            }
        }
    }
}