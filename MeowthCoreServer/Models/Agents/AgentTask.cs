using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeowthCoreServer.Models
{
    public class AgentTask
    {
        public string Id { get; set; }
        public string Command { get; set; }
        public string[] Args { get; set; }
        public byte[] File { get; set; }

    }
}
