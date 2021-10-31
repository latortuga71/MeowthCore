using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Agent.Models;

namespace Agent.Commands
{
    public class WhoAmI : AgentCommand
    {
        public override string Name => "whoami";

        public override string Execute(AgentTask task)
        {
            string output = "";
            WindowsIdentity currentIdentity = WindowsIdentity.GetCurrent();
            output += $"The current user is : {currentIdentity.Name}\n";
            TokenImpersonationLevel token = currentIdentity.ImpersonationLevel;
            output += $"The impersonation level for the current user is : {token.ToString()} \n";
            var groups = currentIdentity.Groups.Translate(typeof(NTAccount));
            output += "The group memberships: \n";
            output += "####################\n";
            foreach (var grp in groups)
            {
                output += $" ::: {grp.Value} :::";
            }
            return output;
        }
    }
}
