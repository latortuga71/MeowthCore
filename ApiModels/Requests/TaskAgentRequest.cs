using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiModels.Requests
{
    public class TaskAgentRequest
    {
        public string Command { get; set; }
        public string[] Args { get; set; }
        public byte[] File { get; set; }
    }
}
