using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agent.Models;

namespace Agent.Commands
{
    public class AdminCheck : AgentCommand
    {
        public override string Name => "admin-check";

        public override string Execute(AgentTask task)
        {
            if (task.Args.Length == 1)
            {
                var targetHost = task.Args[0];
                if (!Internal.Services.CheckIfAdminAccess(targetHost))
                    return $"Not Admin on {targetHost}";
                return $"Admin on {targetHost}";
            }
            var results = "";
            foreach (string target in task.Args)
            {
                if (!Internal.Services.CheckIfAdminAccess(target))
                {
                    results += $"Not Admin on {target}\n";
                    continue;
                }
                results += $"Admin on {target}\n";
            }
            return results;
        }
    }
}
