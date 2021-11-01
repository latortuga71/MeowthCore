using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agent.Models;
using System.Diagnostics;
using System.Threading;
using System.Net.NetworkInformation;

namespace Agent.Commands
{
    public class PingSweep : AgentCommand
    {
        public override string Name => "ping-sweep";

        static CountdownEvent countdown;
        static int upCount;
        public static object lockObj = new object();
        const bool resolveNames = true;
        private static List<string> upHosts;

        public override string Execute(AgentTask task)
        {
            upHosts = new List<string>();
            var subnet = task.Args[0];
            ExecutePingSweep(subnet);
            return string.Join("\n", upHosts);
        }
        public static void ExecutePingSweep(string net)
        {
            countdown = new CountdownEvent(1);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var ipSplit = net.Split('.');
            Array.Resize(ref ipSplit, ipSplit.Length - 1);
            string ipBase = String.Join(".", ipSplit) + ".";
            for (int i = 1; i < 255; i++)
            {
                string ip = ipBase + i.ToString();

                Ping p = new Ping();
                p.PingCompleted += new PingCompletedEventHandler(p_PingCompleted);
                countdown.AddCount();
                p.SendAsync(ip, 100, ip);
            }
            countdown.Signal();
            countdown.Wait();
            sw.Stop();
            TimeSpan span = new TimeSpan(sw.ElapsedTicks);
            //Console.WriteLine("Took {0} milliseconds. {1} hosts active.", sw.ElapsedMilliseconds, upCount);
        }

        private static void p_PingCompleted(object sender, PingCompletedEventArgs e)
        {
            string ip = (string)e.UserState;
            if (e.Reply != null && e.Reply.Status == IPStatus.Success)
            {
                //Console.WriteLine("{0} is up: ({1} ms)", ip, e.Reply.RoundtripTime);
                lock (lockObj)
                {
                    upHosts.Add(ip);
                    upCount++;
                }
            }
            else if (e.Reply == null)
            {
                //Console.WriteLine("Pinging {0} failed. (Null Reply object?)", ip);
            }
            countdown.Signal();
        }
    }
}
