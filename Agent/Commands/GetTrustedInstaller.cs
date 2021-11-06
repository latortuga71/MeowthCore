using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agent.Models;

namespace Agent.Commands
{
    public class GetTrustedInstaller : AgentCommand
    {
        public override string Name => "get-trustedinstaller";

        public override string Execute(AgentTask task)
        {
            if (!Internal.Impersonator.ElevateToTrustedInstaller())
                return "Failed to elevate to trusted installer, need SYSTEM first";
            return "Successfully impersonated trustedinstaller";
        }
    }
}
