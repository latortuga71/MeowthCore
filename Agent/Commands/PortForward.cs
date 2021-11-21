using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agent.Models;
namespace Agent.Commands
{
    public class PortForward : AgentCommand
    {
        public override string Name => "port-forward";

        public override string Execute(AgentTask task)
        {
            // netsh interface portproxy add v4tov4 listenport {} listenaddress={} connectport={} connectaddress={}
            if (task.Args is null || task.Args.Length == 0)
            {
                return "Error Provide Args <listenport> <listenAddress> <connectPort> <connectAddress>";
            }
            var listenPort = task.Args[0];
            var listenAddress = task.Args[1];
            var connectPort = task.Args[2];
            var connectAddress = task.Args[3];
            var args = $"interface portproxy add v4tov4 listenport={listenPort} listenaddress={listenAddress} connectport={connectPort} connectaddress={connectAddress}";
            try
            {
                var res = Internal.Execute.ExecuteCommand("netsh", args);
                return $"Forwarding {listenAddress}:{listenPort} to {connectAddress}:{connectPort} -> {res}";
            }
            catch
            {
                return "Failed to execute port forward";
            }
        }
    }
}
