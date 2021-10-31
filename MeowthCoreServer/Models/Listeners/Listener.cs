using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MeowthCoreServer.Services;

namespace MeowthCoreServer.Models
{
    public abstract class Listener
    {
        public abstract string Name { get; }
        protected IAgentService AgentService;
        public void Init(IAgentService agentService)
        {
            AgentService = agentService;
        }
        public abstract Task Start();
        public abstract void Stop();
    }
}
