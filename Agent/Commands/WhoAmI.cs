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
            var results = new SharpSploitResultList<WhoamiResult>();
            string grps = "";
            WindowsIdentity currentIdentity = WindowsIdentity.GetCurrent();
            TokenImpersonationLevel token = currentIdentity.ImpersonationLevel;
            var groups = currentIdentity.Groups.Translate(typeof(NTAccount));
            foreach (var grp in groups)
            {
                grps += $"{grp.Value}\n\t\t\t\t\t\t";
            }
            var result = new WhoamiResult
            {
                IdentityName = currentIdentity.Name,
                ImpersonationLevel = token.ToString(),
                Groups = grps
            };
            results.Add(result);
            return results.ToString();
        }
    }
    public sealed class WhoamiResult : SharpSploitResult
    {
        public string IdentityName{ get; set; }
        public string ImpersonationLevel { get; set; }
        public string Groups { get; set; }

        protected internal override IList<SharpSploitResultProperty> ResultProperties => new List<SharpSploitResultProperty>
        {
            new SharpSploitResultProperty{Name = nameof(IdentityName),Value = IdentityName},
            new SharpSploitResultProperty{Name = nameof(ImpersonationLevel),Value = ImpersonationLevel},
            new SharpSploitResultProperty{Name = nameof(Groups),Value = Groups},

        };
    }
}
