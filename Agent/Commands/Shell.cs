using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agent.Models;

namespace Agent.Commands
{
    public class Shell : AgentCommand
    {
        public override string Name => "shell";

        public override string Execute(AgentTask task)
        {
            var args = string.Join(" ", task.Args);
            return Internal.Execute.ExecuteCommand(@"C:\Windows\System32\cmd.exe", $"/c {args}");
        }
    }
}
