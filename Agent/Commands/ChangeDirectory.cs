using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agent.Models;
using System.IO;

namespace Agent.Commands
{
    class ChangeDirectory : AgentCommand
    {
        public override string Name => "cd";
        public override string Execute(AgentTask task)
        {
            string path;
            if (task.Args is null || task.Args.Length == 0)
            {
                path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            }
            else
            {
                path = task.Args[0];
            }
            Directory.SetCurrentDirectory(path);
            return Directory.GetCurrentDirectory();
        }

    }
}
