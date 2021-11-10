using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agent.Models;
using NetFwTypeLib;

namespace Agent.Commands
{
    public class EnableDefenderForEndpoint : AgentCommand
    {
        public override string Name => "enable-defender";

        public override string Execute(AgentTask task)
        {
            try
            {
                ExecuteDeleteFirewallBlock(); // delete firewall rules
                Internal.Services.StartWinDefend(); //start windefend
                ExecuteDeleteFirewallBlock(); // delete firewall rules
                return "Successfully enabled defender for endpoint";
            }
            catch (Exception e)
            {
                return $"Failed to enable defender for endpoint {e.Message}";
            }
        }
        public static void ExecuteDeleteFirewallBlock()
        {
            DeleteBlockRule("windefend", "windefendBlocker", "windefend");
            DeleteBlockRule("senseCncProxy", "senseCncProxyBlocker", "", "%ProgramFiles%\\Windows Defender Advanced Threat Protection\\SenseCncProxy.exe");
            DeleteBlockRule("sense", "senseblocker", "sense");
        }
        public static bool DeleteBlockRule(string description, string ruleName, string serviceName = "", string fullPath = "")
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
                firewallPolicy.Rules.Remove(ruleName);
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
                firewallPolicy.Rules.Remove(ruleName);
                return true;
            }
        }
    }
}
