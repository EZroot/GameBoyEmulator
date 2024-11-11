using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace GameBoyEmulator.Debug
{
    public static class Debugger
    {
        public static bool IsDebugEnabled { get; set; }
        public static bool dStepThroughFrame { get; set; }
        public static bool dStepThroughOpcode { get; set; }
        public static int dStepThroughOpcodeStepCount { get; private set; }
        public static bool dStepThroughCpuCycle { get; private set; }
        public static bool dFastForward { get; private set; }
        public static int dFastForwardStepCount { get; private set; }
        public static bool dWriteOutOpcode { get;  set; }
        public static bool dWriteOutMemoryReadWrite { get; private set; }
        public static bool dWriteOutJoypadReadWrite { get; private set; }
        public static bool dDisableRenderFrame { get;  set; }
        public static bool dDisableRenderFrameClearScreen { get; private set; }
        public static bool dBlarrgsTestPrintCpuInstrs { get; private set; }
        public static bool dDebugPPU { get; set; }
        public static bool dStepOutModeFlag = false;  
        public static bool dOutputLogToFile { get; set; }  
        public static void EnableDebugMode()
        {
            IsDebugEnabled = true;
            dWriteOutJoypadReadWrite = true;
            dStepThroughOpcode = true;
            dWriteOutOpcode = true;
            dStepThroughOpcodeStepCount = 100; 
        }
        public static void EnableDebugModeForTests()
        {
            IsDebugEnabled = true;
            dDebugPPU = true;
        }
    }
}
