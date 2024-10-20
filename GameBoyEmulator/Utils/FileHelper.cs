using GameBoyEmulator.Debug;
using System;
using System.IO;
namespace GameBoyEmulator.Utils
{
    internal class FileHelper
    {
        private static string logFilePath = "GB_OpcodeLogs.txt"; 
        public static void LogToFile(string message)
        {
            if (!Debugger.dOutputLogToFile) return;
            try
            {
                using (StreamWriter writer = new StreamWriter(logFilePath, true)) 
                {
                    writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
                    writer.Flush();  
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to log file: {ex.Message}");
            }
        }
    }
}
