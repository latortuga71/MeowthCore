using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Models
{
    public abstract class AgentCommand
    {
        public abstract string Name { get; }
        public abstract string Execute(AgentTask task);

    }
}
