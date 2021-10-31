using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Internal
{
    public static class Execute
    {
        public static string ExecuteCommand(string fileName,string args)
        {
            var startInfo = new ProcessStartInfo
            {
                
                FileName = fileName,
                Arguments = args,
                WorkingDirectory = Directory.GetCurrentDirectory(),
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            var process = Process.Start(startInfo);
            string output = "";
            using (process.StandardOutput)
            {
                output += process.StandardOutput.ReadToEnd();
            }
            using (process.StandardError)
            {
                output += process.StandardError.ReadToEnd();
            }
            return output;
        }

        public static string ExecuteAssembly(byte[] asm,string[] args = null)
        {
            if (args is null)
            {
                args = new string[] { };
            }
            var currentOut = Console.Out;
            var currentError = Console.Error;
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);
            sw.AutoFlush = true;
            Console.SetOut(sw);
            Console.SetError(sw);
            var assembly = Assembly.Load(asm);
            assembly.EntryPoint.Invoke(null, new object[] { args });
            Console.Out.Flush();
            Console.Error.Flush();
            var output = Encoding.UTF8.GetString(ms.ToArray());
            Console.SetOut(currentOut);
            Console.SetError(currentError);
            sw.Dispose();
            ms.Dispose();
            return output;

        }
    }
}
