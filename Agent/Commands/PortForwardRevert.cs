using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agent.Models;
namespace Agent.Commands
{
    public class PortForwardRevert : AgentCommand
    {
        public override string Name => "port-forward-revert";

        public override string Execute(AgentTask task)
        {
            try
            {
                var res = Internal.Execute.ExecuteCommand("netsh", "interface portproxy reset");
                return $"Successfully reverted port forward";
            }
            catch
            {
                return "Failed to execute revert port forward";
            }
        }
    }
}
