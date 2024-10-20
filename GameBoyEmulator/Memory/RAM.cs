namespace GameBoyEmulator.Memory
{
    public class RAM
    {
        private readonly byte[] _memory;
        public RAM(int memorySize = 0x10000)
        {
            _memory = new byte[memorySize];
        }
        public byte ReadByte(ushort address)
        {
            return _memory[address];
        }
        public void WriteByte(ushort address, byte value)
        {
            _memory[address] = value;
        }
        public ushort ReadWord(ushort address)
        {
            return (ushort)(_memory[address] | (_memory[address + 1] << 8));
        }
        public void WriteWord(ushort address, ushort value)
        {
            _memory[address] = (byte)(value & 0xFF);
            _memory[address + 1] = (byte)(value >> 8);
        }
        public void CopyToMemory(byte[] source, int destinationOffset, int length)
        {
            Array.Copy(source, 0, _memory, destinationOffset, length);
        }
    }
}
