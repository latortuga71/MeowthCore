using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agent.Models;

namespace Agent.Commands
{
    public class PrintWorkingDirectory : AgentCommand
    {
        public override string Name => "pwd";
        public override string Execute(AgentTask task)
        {
            return Directory.GetCurrentDirectory();
        }

    }
}
