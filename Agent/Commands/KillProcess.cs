using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agent.Models;
using System.Diagnostics;
namespace Agent.Commands
{
    public class KillProcess : AgentCommand
    {
        public override string Name => "kill-process";

        public override string Execute(AgentTask task)
        {
            if (!int.TryParse(task.Args[0], out var pid))
                return "Error Provide Args <pid>";
            var proc = Process.GetProcessById(pid);
            try
            {
                proc.Kill();
                return $"Successfully Terminated PID {pid}";
            }
            catch (Exception e)
            {
                return $"Failed to terminate PID {pid} -> {e}";
            }

        }
    }
}
