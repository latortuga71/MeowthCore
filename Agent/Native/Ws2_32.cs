using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System;
using System.Net;
using System.Net.Sockets;
namespace Agent.Native
{
    public static class Ws2_32
    {
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool FreeConsole();

        [DllImport("ws2_32.dll", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern int WSAStartup(
            [In] short wVersionRequested,
            [Out] out WSAData lpWSAData
            );

        [DllImport("ws2_32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr WSASocket(
            [In] AddressFamily addressFamily,
            [In] SocketType socketType,
            [In] ProtocolType protocolType,
            [In] IntPtr protocolInfo,
            [In] uint group,
            [In] int flags
            );

        [DllImport("ws2_32.dll", SetLastError = true)]
        public static extern int WSAConnect(
            [In] IntPtr socketHandle,
            [In] byte[] socketAddress,
            [In] int socketAddressSize,
            [In] IntPtr inBuffer,
            [In] IntPtr outBuffer,
            [In] IntPtr sQOS,
            [In] IntPtr gQOS
            );

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool CreateProcessWithLogonW(
            String userName,
            String domain,
            String password,
            int logonFlags,
            String applicationName,
            String commandLine,
            int creationFlags,
            int environment,
            String currentDirectory,
            ref STARTUPINFO startupInfo,
            out PROCESS_INFORMATION processInformation);

        [StructLayout(LayoutKind.Sequential)]
        public struct WSAData
        {
            public Int16 wVestion;
            public Int16 wHighVersion;
            public Byte szDescription;
            public Byte szSystemStatus;
            public Int16 iMaxSockets;
            public Int16 iMaxUdpDg;
            public IntPtr lpVendorInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct STARTUPINFO
        {
            public Int32 cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public Int32 dwX;
            public Int32 dwY;
            public Int32 dwXSize;
            public Int32 dwYSize;
            public Int32 dwXCountChars;
            public Int32 dwYCountChars;
            public Int32 dwFillAttribute;
            public Int32 dwFlags;
            public Int16 wShowWindow;
            public Int16 cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }
        public enum SOCKET_TYPE : short
        {
            /// <summary>
            /// stream socket
            /// </summary>
            SOCK_STREAM = 1,

            /// <summary>
            /// datagram socket
            /// </summary>
            SOCK_DGRAM = 2,

            /// <summary>
            /// raw-protocol interface
            /// </summary>
            SOCK_RAW = 3,

            /// <summary>
            /// reliably-delivered message
            /// </summary>
            SOCK_RDM = 4,

            /// <summary>
            /// sequenced packet stream
            /// </summary>
            SOCK_SEQPACKET = 5
        }

        public enum PROTOCOL : short
        {//dummy for IP  
            IPPROTO_IP = 0,
            //control message protocol  
            IPPROTO_ICMP = 1,
            //internet group management protocol  
            IPPROTO_IGMP = 2,
            //gateway^2 (deprecated)  
            IPPROTO_GGP = 3,
            //tcp  
            IPPROTO_TCP = 6,
            //pup  
            IPPROTO_PUP = 12,
            //user datagram protocol  
            IPPROTO_UDP = 17,
            //xns idp  
            IPPROTO_IDP = 22,
            //IPv6  
            IPPROTO_IPV6 = 41,
            //UNOFFICIAL net disk proto  
            IPPROTO_ND = 77,

            IPPROTO_ICLFXBM = 78,
            //raw IP packet  
            IPPROTO_RAW = 255,

            IPPROTO_MAX = 256
        }

        public enum ADDRESS_FAMILIES : int
        {
            /// <summary>
            /// Unspecified [value = 0].
            /// </summary>
            AF_UNSPEC = 0,
            /// <summary>
            /// Local to host (pipes, portals) [value = 1].
            /// </summary>
            AF_UNIX = 1,
            /// <summary>
            /// Internetwork: UDP, TCP, etc [value = 2].
            /// </summary>
            AF_INET = 2,
            /// <summary>
            /// Arpanet imp addresses [value = 3].
            /// </summary>
            AF_IMPLINK = 3,
            /// <summary>
            /// Pup protocols: e.g. BSP [value = 4].
            /// </summary>
            AF_PUP = 4,
            /// <summary>
            /// Mit CHAOS protocols [value = 5].
            /// </summary>
            AF_CHAOS = 5,
            /// <summary>
            /// XEROX NS protocols [value = 6].
            /// </summary>
            AF_NS = 6,
            /// <summary>
            /// IPX protocols: IPX, SPX, etc [value = 6].
            /// </summary>
            AF_IPX = 6,
            /// <summary>
            /// ISO protocols [value = 7].
            /// </summary>
            AF_ISO = 7,
            /// <summary>
            /// OSI is ISO [value = 7].
            /// </summary>
            AF_OSI = 7,
            /// <summary>
            /// european computer manufacturers [value = 8].
            /// </summary>
            AF_ECMA = 8,
            /// <summary>
            /// datakit protocols [value = 9].
            /// </summary>
            AF_DATAKIT = 9,
            /// <summary>
            /// CCITT protocols, X.25 etc [value = 10].
            /// </summary>
            AF_CCITT = 10,
            /// <summary>
            /// IBM SNA [value = 11].
            /// </summary>
            AF_SNA = 11,
            /// <summary>
            /// DECnet [value = 12].
            /// </summary>
            AF_DECnet = 12,
            /// <summary>
            /// Direct data link interface [value = 13].
            /// </summary>
            AF_DLI = 13,
            /// <summary>
            /// LAT [value = 14].
            /// </summary>
            AF_LAT = 14,
            /// <summary>
            /// NSC Hyperchannel [value = 15].
            /// </summary>
            AF_HYLINK = 15,
            /// <summary>
            /// AppleTalk [value = 16].
            /// </summary>
            AF_APPLETALK = 16,
            /// <summary>
            /// NetBios-style addresses [value = 17].
            /// </summary>
            AF_NETBIOS = 17,
            /// <summary>
            /// VoiceView [value = 18].
            /// </summary>
            AF_VOICEVIEW = 18,
            /// <summary>
            /// Protocols from Firefox [value = 19].
            /// </summary>
            AF_FIREFOX = 19,
            /// <summary>
            /// Somebody is using this! [value = 20].
            /// </summary>
            AF_UNKNOWN1 = 20,
            /// <summary>
            /// Banyan [value = 21].
            /// </summary>
            AF_BAN = 21,
            /// <summary>
            /// Native ATM Services [value = 22].
            /// </summary>
            AF_ATM = 22,
            /// <summary>
            /// Internetwork Version 6 [value = 23].
            /// </summary>
            AF_INET6 = 23,
            /// <summary>
            /// Microsoft Wolfpack [value = 24].
            /// </summary>
            AF_CLUSTER = 24,
            /// <summary>
            /// IEEE 1284.4 WG AF [value = 25].
            /// </summary>
            AF_12844 = 25,
            /// <summary>
            /// IrDA [value = 26].
            /// </summary>
            AF_IRDA = 26,
            /// <summary>
            /// Network Designers OSI &amp; gateway enabled protocols [value = 28].
            /// </summary>
            AF_NETDES = 28,
            /// <summary>
            /// [value = 29].
            /// </summary>
            AF_TCNPROCESS = 29,
            /// <summary>
            /// [value = 30].
            /// </summary>
            AF_TCNMESSAGE = 30,
            /// <summary>
            /// [value = 31].
            /// </summary>
            AF_ICLFXBM = 31
        }
    }
}
