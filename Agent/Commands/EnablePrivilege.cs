using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agent.Models;
namespace Agent.Commands
{
    public class EnablePrivilege : AgentCommand
    {
        public override string Name => "enable-priv";

        public override string Execute(AgentTask task)
        {
            var priv = task.Args[0];
            if (Internal.Impersonator.IsPrivilegeEnabled(priv))
                return "Already Enabled";
            if (!Internal.Impersonator.EnablePrivilege(priv))
                return $"Failed to enable {priv}";
            return $"Successfully enabled {priv}";
        }
    }
}
