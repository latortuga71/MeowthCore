using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Agent.Models;
namespace Agent.Commands
{
    public class ListProcesses : AgentCommand
    {
        public override string Name => "ps";

        public override string Execute(AgentTask task)
        {
            var results = new SharpSploitResultList<ListProcessesResult>();
            var processes = Process.GetProcesses();
            foreach (var proc in processes)
            {
                var result = new ListProcessesResult
                {
                    ProcessName = proc.ProcessName,
                    ProcessId = proc.Id,
                    SessionId = proc.SessionId
                };
                result.ProcessPath = GetProcessPath(proc);
                result.Arch = GetProcessArch(proc);
                result.Owner = GetProcessOwner(proc);
                results.Add(result);
            }
            return results.ToString();
        }
        private string GetProcessPath(Process process)
        {
            try
            {
                return process.MainModule.FileName;
            }
            catch
            {
                return "-";
            }
        }
        private string GetProcessArch(Process process)
        {
            try
            {
                var is64BitOS = Environment.Is64BitOperatingSystem;
                if (!is64BitOS)
                    return "x86";
                if (!Native.Kernel32.IsWow64Process(process.Handle, out var isWow64))
                    return "-";
                if (is64BitOS && isWow64) return "x86";
                return "x64";
            }
            catch {
                return "-";
            }
        }
        private string GetProcessOwner(Process proc)
        {
            IntPtr hToken = IntPtr.Zero;
            try
            {
                var hProcess = proc.Handle;
                if (!Native.Advapi.OpenProcessToken(hProcess, Native.Advapi.TOKEN_ALL_ACCESS, out hToken))
                    return "-";
                var identity = new WindowsIdentity(hToken);
                return identity.Name;
            }
            catch
            {
                return "-";
            }
            finally
            {
                Native.Kernel32.CloseHandle(hToken);
            }
        }
    }
    public sealed class ListProcessesResult : SharpSploitResult
    {
        public string ProcessName { get; set; }
        public string ProcessPath { get; set; }
        public string Owner { get; set; }
        public string Arch { get; set; }
        public int ProcessId { get; set; }
        public int SessionId { get; set; }

        protected internal override IList<SharpSploitResultProperty> ResultProperties => new List<SharpSploitResultProperty>
        {
            new SharpSploitResultProperty{Name = nameof(ProcessName),Value = ProcessName},
            new SharpSploitResultProperty{Name = nameof(ProcessPath),Value = ProcessPath},
            new SharpSploitResultProperty{Name = nameof(Owner),Value = Owner},
            new SharpSploitResultProperty{Name = nameof(Arch),Value = Arch},
            new SharpSploitResultProperty{Name = "PID",Value = ProcessId},
            new SharpSploitResultProperty{Name = nameof(SessionId),Value = SessionId}
        };
    }
}
