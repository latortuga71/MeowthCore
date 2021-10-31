using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agent.Models;
namespace Agent.Commands
{
    public class TestCommand : AgentCommand
    {
        public override string Name => "TestCommand";

        public override string Execute(AgentTask task)
        {
            return "hello";
        }
    }
}
