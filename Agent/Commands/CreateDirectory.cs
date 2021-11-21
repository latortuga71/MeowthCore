using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agent.Models;

namespace Agent.Commands
{
    public class CreateDirectory : AgentCommand
    {
        public override string Name => "mkdir";

        public override string Execute(AgentTask task)
        {
            string path;
            if (task.Args is null || task.Args.Length == 0)
            {
                return "Error Provide Args <fullPathToDir>";
            }
            else
            {
                path = task.Args[0];
            }
            var dirInfo = Directory.CreateDirectory(path);
            return $"{dirInfo.FullName} Created";
        }
    }
}
