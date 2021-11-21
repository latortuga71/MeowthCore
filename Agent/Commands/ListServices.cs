using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agent.Models;
using Agent.Internal;
using System.Management;

namespace Agent.Commands 
{
    public class ListServices : AgentCommand
    {
        public override string Name => "list-services";

        public override string Execute(AgentTask task)
        {
            var results = new SharpSploitResultList<ListServicesResult>();
            var services = Services.GetServices();
            bool canAccess;
            foreach (var s in services)
            {
                try
                {
                    canAccess = GetServiceAccess(s);
                }
                catch
                {
                    canAccess = false;
                }
                var result = new ListServicesResult
                {
                    ServiceName = s.ServiceName,
                    ServiceStatus = GetServiceStatus(s),
                    ServiceStartType = GetServiceStartType(s),
                    BinPath = GetPathOfService(s.ServiceName),
                    CanAccess = canAccess,
                };
                results.Add(result);
            }
            return results.ToString();
        }
        private bool GetServiceAccess(System.ServiceProcess.ServiceController s)
        {
            var handle = s.ServiceHandle;
            if (handle.IsInvalid) { return false; }
            handle.Close();
            return true;
        }
        private string GetServiceStatus(System.ServiceProcess.ServiceController s)
        {
            var status = s.Status;
            return status.ToString();
        }
        private string GetServiceStartType(System.ServiceProcess.ServiceController s)
        {
            var type = s.StartType;
            return type.ToString();
        }
        private string GetServiceType(System.ServiceProcess.ServiceController s)
        {
            var type = s.ServiceType;
            return type.ToString();
        }
        private static string GetPathOfService(string serviceName)
        {
            WqlObjectQuery wqlObjectQuery = new WqlObjectQuery(string.Format("SELECT * FROM Win32_Service WHERE Name = '{0}'", serviceName));
            ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(wqlObjectQuery);
            ManagementObjectCollection managementObjectCollection = managementObjectSearcher.Get();

            foreach (ManagementObject managementObject in managementObjectCollection)
            {
                return managementObject.GetPropertyValue("PathName").ToString();
            }

            return "";
        }
    }
    public sealed class ListServicesResult : SharpSploitResult
    {
        public string ServiceName { get; set; }
        public string ServiceStatus { get; set; }
        public string ServiceStartType { get; set; }
        public string BinPath { get; set; }
        public bool CanAccess { get; set; }


        protected internal override IList<SharpSploitResultProperty> ResultProperties => new List<SharpSploitResultProperty>
        {
            new SharpSploitResultProperty{Name = nameof(ServiceName),Value = ServiceName},
            new SharpSploitResultProperty{Name = nameof(ServiceStatus),Value = ServiceStatus},
            new SharpSploitResultProperty{Name = nameof(ServiceStartType),Value = ServiceStartType},
            new SharpSploitResultProperty{Name = nameof(CanAccess),Value = CanAccess},
            new SharpSploitResultProperty{Name = nameof(BinPath),Value = BinPath},
        };
    }
}
