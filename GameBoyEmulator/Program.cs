using System;
namespace GameBoyEmulator
{
    class Program
    {
        static void Main(string[] args)
        {
            Emulator emulator = new Emulator();
            string romPath;
            if (args.Length > 0 && args[0].EndsWith(".gb", StringComparison.OrdinalIgnoreCase))
            {
                romPath = args[0];
                Console.WriteLine($"Loading ROM from command-line argument: {romPath}.\n Try /fullpath/romname.gb");
            }
            else
            {
                Console.WriteLine("Enter the path to the Game Boy ROM file:");
                romPath = Console.ReadLine();
            }
            emulator.LoadROM(romPath);
            emulator.Run();
        }
    }
}
