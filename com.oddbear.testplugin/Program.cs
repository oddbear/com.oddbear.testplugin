using BarRaider.SdTools;
using System.Diagnostics;

namespace com.oddbear.testplugin
{
    internal class Program
    {
        static void Main(string[] args)
        {
#if DEBUG

            while (!Debugger.IsAttached)
            {
                Thread.Sleep(100);
            }

            // If not using Endrian:
            //System.Diagnostics.Debugger.Launch();

            //I am using Entrian Attach to auto attach... but we do want to wait for the Debugger:
            // Settings for Entrian: 'com.oddbear.testplugin.exe', 'CoreCLR', 'Always', 'Continue', 'None'
            // launchSettings.json: 
            //{
            //   "profiles": {
            //     "DebugWin": {
            //       "commandName": "Executable",
            //       "executablePath": "c:\\windows\\system32\\cmd.exe",
            //       "commandLineArgs": "/S /C \"start \"title\" /B \"%ProgramW6432%\\Elgato\\StreamDeck\\StreamDeck.exe\" \""
            //     }
            //   }
            // }

            // At least if stopped by Debug -> Detach All (can be added to toolbar menu):
            new Thread(() =>
            {
                while (Debugger.IsAttached)
                {
                    Thread.Sleep(100);
                }
                KillTargetProcess("StreamDeck");
            }).Start();
#endif
            SDWrapper.Run(args);
        }

        static void KillTargetProcess(string processName)
        {
            foreach (var process in Process.GetProcessesByName(processName))
            {
                process.Kill();
            }
        }
    }
}
