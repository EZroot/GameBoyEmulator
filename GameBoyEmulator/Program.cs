using System;

namespace GameBoyEmulator
{
    class Program
    {
        static void Main(string[] args)
        {
            Emulator emulator = new Emulator();

            Console.WriteLine("Enter the path to the Game Boy ROM file:");
            string romPath = Console.ReadLine();

            if (System.IO.File.Exists(romPath))
            {
                emulator.LoadROM(romPath);
                emulator.Run();
            }
            else
            {
                Console.WriteLine("ROM file not found.");
            }
        }
    }
}
