using System;
using System.Threading;
namespace GameBoyEmulator.Memory
{
    public class MemoryMap
    {
        private readonly RAM _ram;
        public MemoryMap(RAM ram)
        {
            _ram = ram;
        }
        public void LoadROM(byte[] romData)
        {
            if (romData.Length > 0x8000)
            {
                Console.WriteLine("WARNING! ROM too large. Basic ROM loading only supports up to 32KB ROMs.\n Continuing anyway...");
                Thread.Sleep(2500);
            }
            _ram.CopyToMemory(romData, 0x0000, romData.Length);
        }
    }
}
