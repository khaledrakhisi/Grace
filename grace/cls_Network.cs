using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace grace
{
    class cls_Network
    {
        private static Thread thread;
        public const int PORT = 2019;        
        private static Socket ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static readonly Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private const int BUFFER_SIZE = 2048;
        private static readonly byte[] buffer = new byte[BUFFER_SIZE];
        public const string pingableIPAddress = "172.27.20.1";
        private static HttpClient httpClient;

        private static UdpClient udpServer = null;//new UdpClient(PORT);    
        private static NamedPipeServerStream pipeServer = null;

        static cls_Network()
        {
            #region Initializing HttpsClient object
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("http://localhost:7340");
            // Add an Accept header for JSON format.
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            #endregion
        }

        public static string http_GET(string s_subURL)
        {
            HttpResponseMessage response = httpClient.GetAsync(s_subURL).Result;  // Blocking call!
            if (response.IsSuccessStatusCode)
            {
                var data = response.Content.ReadAsStringAsync().Result;
                return data;
            }

            return null;
        }
        public static string http_DELETE(string s_subURL)
        {
            HttpResponseMessage response = httpClient.DeleteAsync(s_subURL).Result;
            if (response.IsSuccessStatusCode)
            {
                var data = response.Content.ReadAsStringAsync().Result;
                return data;
            }

            return null;
        }

        public static bool ValidateIPv4(string ipString)
        {
            if (String.IsNullOrWhiteSpace(ipString))
            {
                return false;
            }

            string[] splitValues = ipString.Split('.');
            if (splitValues.Length != 4)
            {
                return false;
            }

            byte tempForParsing;

            return splitValues.All(r => byte.TryParse(r, out tempForParsing));
        }

        public static void SetupUDPServer()
        {
            try
            {
                udpServer = new UdpClient(PORT);

                cls_Utility.Log("** UDP server is about to start...");

                thread = new Thread(UDPServerListening);
                thread.IsBackground = true;
                thread.Start();
                //Start(new Action(UDPServerListening));
            }
            catch (Exception)
            {
                //if the ip is in wrong format
            }      
        }

        private static void UDPServerListening()
        {
            cls_Utility.Log("** UDP server is listening now...");
            frm_Terminal.UpdateStaticConsole("** UDP server is listening now...");            
            
            while (true)
            {
                string sMessage = string.Empty;
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, PORT);
                byte[] buffer = udpServer.Receive(ref remoteEP);
                sMessage = Encoding.Unicode.GetString(buffer);
                string sResult = "** A reply from \'" + remoteEP.Address + "\' : \r\n" + sMessage;
                cls_Utility.Log(sResult);

                if (sMessage == "{@ack@}")
                {
                    //frm_Terminal.UpdateStaticConsole("** The message has been delivered to \'" + remoteEP.Address + "\'.");
                }
                else if (!sMessage.Contains("{@command@}"))
                {
                    frm_Terminal.UpdateStaticConsole(sResult);
                }
                else if (sMessage.Contains("{@command@}"))
                {
                    //cls_Utility.Log("-----attempting split secure-string");
                    // In this section user can run a bunch of commands seperated by ';'
                    string[] commandStatements = sMessage.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string commandStatement in commandStatements)
                    {
                        sResult = (string)cls_Interpreter.RunACommand(commandStatement.Replace("{@command@}", string.Empty), remoteEP.Address);
                        cls_Utility.Log(sResult);
                        frm_Terminal.UpdateStaticConsole(sResult);
                        UnicastUDPPacket(remoteEP.Address, sResult);

                        Application.DoEvents();
                    }
                    
                }

                //buffer = Encoding.Unicode.GetBytes("{@ack@}");
                //udpServer.Send(buffer, buffer.Length, remoteEP);
                if (sMessage != "{@ack@}")
                {
                    UnicastUDPPacket(remoteEP.Address, "{@ack@}");
                }                
                //return sMessage;
            }
            //udpServer.Close();
            //SetupUDPServer();
            //listBox_status.Items.Add("UDP Server setup completed.");
        }

        public static void EndUDPServer()
        {
            if (thread != null)
            {
                thread.Abort();
                udpServer.Close();
            }
        }

        public static void UnicastUDPPacket(IPAddress ip, string sMessage)
        {
            try
            {
                if (string.IsNullOrEmpty(sMessage)) sMessage = string.Empty;

                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                IPEndPoint ipep = new IPEndPoint(ip, PORT);
                byte[] buffer = Encoding.Unicode.GetBytes(sMessage);
                socket.SendTo(buffer, ipep);
                //socket.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void MulticastUDPPacket(IPAddress ip_begin, IPAddress ip_end, string sMessage)
        {
            Thread t = new Thread(() => MulticastUDP(ip_begin, ip_end, sMessage));
            t.IsBackground = true;
            t.Start();
        }
        private static void MulticastUDP(IPAddress ip_begin, IPAddress ip_end, string sMessage)
        {
            //IPAddress ip_begin = ip_begin, ip_end = IPAddress.Parse(txt_ip_end.Text);
            cls_IPTools it = new cls_IPTools();
            IEnumerable<string> addresses = it.GetIPRange(ip_begin, ip_end);
            foreach (string ad in addresses)
            {
                string localIP = cls_IPTools.GetLocalActiveIP(pingableIPAddress, PORT).ToString();
                if (ad == localIP) continue;
                UnicastUDPPacket(IPAddress.Parse(ad), sMessage);
                //frm_Terminal.UpdateStaticConsole("** mullticasted to : " + ad);
                //AddItemToListBox("message sent to :" + ad);
            }
        }

        public static void BroadcastUDPPacket(string sMessage)
        {
            var Client = new UdpClient();
            var RequestData = Encoding.Unicode.GetBytes(sMessage);
            //var ServerEp = new IPEndPoint(IPAddress.Any, PORT);

            Client.EnableBroadcast = true;
            Client.Send(RequestData, RequestData.Length, new IPEndPoint(IPAddress.Broadcast, PORT));

            //var ServerResponseData = Client.Receive(ref ServerEp);
            //var ServerResponse = Encoding.ASCII.GetString(ServerResponseData);
            //Console.WriteLine("Recived {0} from {1}", ServerResponse, ServerEp.Address.ToString());

            Client.Close();
        }


        //   TCP   //

        private static void ConnectToServer(IPAddress serverIP)
        {
            int attempts = 0;
            ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            while (!ClientSocket.Connected)
            {
                if (attempts >= 3) break;
                try
                {
                    attempts++;
                    frm_Terminal.UpdateStaticConsole("** Connection attempt " + attempts);
                    // Change IPAddress.Loopback to a remote IP to connect to a remote host.
                    ClientSocket.Connect(serverIP, PORT);
                }
                catch (SocketException ex)
                {
                    //Console.Clear();                    
                    frm_Terminal.UpdateStaticConsole("! Error in connecting : " + ex.Message);
                }
            }

            if (attempts < 3)
                frm_Terminal.UpdateStaticConsole("** Connected to server.");
        }
        private static void DisconnectFromServer(IPAddress ip)
        {
            UnicastTCPPacket(ip, "{@exit@}"); // Tell the server we are exiting
            ClientSocket.Shutdown(SocketShutdown.Both);
            ClientSocket.Close();
        }
        public static void UnicastTCPPacket(IPAddress ip, string text)
        {
            if(!text.Contains("{@exit@}"))
                ConnectToServer(ip);

            byte[] buffer = Encoding.Unicode.GetBytes(text);
            //ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ClientSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);

            if (!text.Contains("{@exit@}"))
                frm_Terminal.UpdateStaticConsole("** TCP Unicast message \'" + text.Replace("{@command@}", string.Empty) + "\' sent.");

            if (!text.Contains("{@exit@}"))
                DisconnectFromServer(ip);
        }
        public static void SetupTCPServer()
        {
            try
            {
                serverSocket.Bind(new IPEndPoint(cls_IPTools.GetLocalActiveIP(pingableIPAddress, PORT), PORT));
                serverSocket.Listen(0);
                serverSocket.BeginAccept(AcceptCallback, null);
                frm_Terminal.UpdateStaticConsole("** TCP server is listening now...");
            }
            catch (Exception ex)
            {
                cls_Utility.Log("! Error in setting up TCP server. " + ex.Message);
                frm_Terminal.UpdateStaticConsole("! Error in setting up TCP server. " + ex.Message);
            }
        }
        private static void AcceptCallback(IAsyncResult AR)
        {
            Socket socket;

            try
            {
                socket = serverSocket.EndAccept(AR);
            }
            catch (ObjectDisposedException) // I cannot seem to avoid this (on exit when properly closing sockets)
            {
                return;
            }
            //clientSockets.Add(socket);
            socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, socket);
            //listBox_status.Items.Add("Client connected, waiting for request...");            
            serverSocket.BeginAccept(AcceptCallback, null);
            //frm_Terminal.UpdateStaticConsole("** A Client connected...");
        }

        private static void ReceiveCallback(IAsyncResult AR)
        {
            Socket current = (Socket)AR.AsyncState;
            int received;

            try
            {
                received = current.EndReceive(AR);
            }
            catch (SocketException)
            {
                frm_Terminal.UpdateStaticConsole("** Client forcefully disconnected");
                // Don't shutdown because the socket may be disposed and its disconnected anyway.
                current.Close();
                //clientSockets.Remove(current);
                return;
            }

            byte[] recBuf = new byte[received];
            Array.Copy(buffer, recBuf, received);
            string text = Encoding.Unicode.GetString(recBuf);
            //Console.WriteLine("Received Text: " + text);
            //listBox_status.Items.Add(text);            
            if (!text.Contains("{@command@}")) // Client wants to exit gracefully
                frm_Terminal.UpdateStaticConsole("** A TCP message just received from \'" + IPAddress.Parse(((IPEndPoint)current.RemoteEndPoint).Address.ToString()) + "\' : " + (text.Replace("{@exit@}", string.Empty)).Replace("{@command@}", string.Empty));

            if (text.Contains("{@exit@}")) // Client wants to exit gracefully
            {
                // Always Shutdown before closing
                current.Shutdown(SocketShutdown.Both);
                current.Close();
                text = text.Replace("{@exit@}", string.Empty);
                //clientSockets.Remove(current);
                //frm_Terminal.UpdateStaticConsole("** The Client disconnected.");
            }
            if (text.Contains("{@command@}"))
            {
                string s = (string)cls_Interpreter.RunACommand(text.Replace("{@command@}", string.Empty), null);
                frm_Terminal.UpdateStaticConsole(s);
            }
            //else
            //{
            //    //Console.WriteLine("Text is an invalid request");
            //    listBox_status.Items.Add("Text is an invalid request");
            //    byte[] data = Encoding.Unicode.GetBytes("Invalid request");
            //    current.Send(data);
            //    //Console.WriteLine("Warning Sent");
            //    listBox_status.Items.Add("Warning Sent");
            //}

            //current.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, current);
        }
        public static string PipeClientReceiveText(string pipeServerName)
        {
            using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", pipeServerName, PipeDirection.In))
            {

                // Connect to the pipe or wait until the pipe is available.                
                cls_Utility.Log("Attempting to connect to pipe...");
                pipeClient.Connect();

                
                cls_Utility.Log("Connected to pipe.");

                cls_Utility.Log("There are currently " + pipeClient.NumberOfServerInstances + " pipe server instances open.");
                using (StreamReader sr = new StreamReader(pipeClient))
                {
                    // Display the read text to the console
                    string temp;
                    temp = sr.ReadLine();
                    return temp;
                    //while ((temp = sr.ReadLine()) != null)
                    //{
                    //    Console.WriteLine("Received from server: {0}", temp);
                    //}
                }
            }
            //Console.Write("Press Enter to continue...");
            //Console.ReadLine();
        }
        private static void PipeServerListening()
        {
            using (pipeServer = new NamedPipeServerStream("grace_pipe", PipeDirection.Out))
            {
                //UpdateStaticConsole("NamedPipeServerStream object created.\r\n");
                cls_Utility.Log("NamedPipeServerStream object created.");

                // Wait for a client to connect                                
                //UpdateStaticConsole("Waiting for client connection...\r\n");
                cls_Utility.Log("Waiting for client connection...");

                pipeServer.WaitForConnection();

                //Console.WriteLine("Client connected.");
                //UpdateStaticConsole("Client connected.");
                cls_Utility.Log("Client connected.");
                
            }
        }
        public static void PipeServerSendText(string text)
        {
            try
            {
                if (pipeServer != null && pipeServer.IsConnected)
                {
                    // Read user input and send that to the client process.
                    using (StreamWriter sw = new StreamWriter(pipeServer))
                    {
                        sw.AutoFlush = true;
                        //UpdateStaticConsole("Enter text: ");
                        sw.WriteLine(text);
                    }
                }
            }
            // Catch the IOException that is raised if the pipe is broken
            // or disconnected.
            catch (IOException ex)
            {
                //Console.WriteLine("ERROR: {0}", e.Message);
                //MessageBox.Show("ERROR: " + ex.Message);
                cls_Utility.Log("ERROR: " + ex.Message);
            }
        }

        public static void PipeServerSetup()
        {
            cls_Utility.StartThread(new Action(PipeServerListening));

            //thread = new Thread(PipeServerListening);            
            ////thread = new Thread(() => PipeServerListening(filename));
            //thread.IsBackground = true;
            //thread.Start();
        }

        public static List<IPAddress> GetAllIPAddressesByMachineName(string machineName)
        {
            //string ipAdress = string.Empty;
            List<IPAddress> ipAddresses = null;
            try
            {
                ipAddresses = Dns.GetHostAddresses(machineName).ToList();

                //IPAddress ip = ipAddresses[0];

                //ipAdress = ip.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
                // Machine not found...
            }
            return ipAddresses;
        }
 /* ====================================================================================
                    C# IP address range finder helper class (C) Nahum Bazes
 * Free for private & commercial use - no restriction applied, please leave credits.
 *                              DO NOT REMOVE THIS COMMENT
 * ==================================================================================== */
        public class cls_IPTools
        {
            public IEnumerable<string> GetIPRange(IPAddress startIP,
                IPAddress endIP)
            {
                uint sIP = ipToUint(startIP.GetAddressBytes());
                uint eIP = ipToUint(endIP.GetAddressBytes());
                while (sIP <= eIP)
                {
                    yield return new IPAddress(reverseBytesArray(sIP)).ToString();
                    sIP++;
                }
            }


            /* reverse byte order in array */
            protected uint reverseBytesArray(uint ip)
            {
                byte[] bytes = BitConverter.GetBytes(ip);
                bytes = bytes.Reverse().ToArray();
                return (uint)BitConverter.ToInt32(bytes, 0);
            }


            /* Convert bytes array to 32 bit long value */
            protected uint ipToUint(byte[] ipBytes)
            {
                ByteConverter bConvert = new ByteConverter();
                uint ipUint = 0;

                int shift = 24; // indicates number of bits left for shifting
                foreach (byte b in ipBytes)
                {
                    if (ipUint == 0)
                    {
                        ipUint = (uint)bConvert.ConvertTo(b, typeof(uint)) << shift;
                        shift -= 8;
                        continue;
                    }

                    if (shift >= 8)
                        ipUint += (uint)bConvert.ConvertTo(b, typeof(uint)) << shift;
                    else
                        ipUint += (uint)bConvert.ConvertTo(b, typeof(uint));

                    shift -= 8;
                }

                return ipUint;
            }

            public static IPAddress GetLocalActiveIP(string relatedNetworkIP, int portNumber)
            {
                IPAddress localIP = null;
                try
                {                    
                    using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                    {
                        socket.Connect(relatedNetworkIP, portNumber);
                        IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                        localIP = endPoint.Address;
                    }
                }catch(SocketException)
                {
                    using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                    {
                        socket.Connect("localhost", portNumber);
                        IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                        localIP = endPoint.Address;
                    }
                }
                return localIP;
            }
        }


        public class SSHConnection
        {
            SshClient sshc = null;
            public SSHConnection(string sHostName, string sUsername, string sPassword, int nPort)
            {
                sshc = new SshClient(sHostName, nPort, sUsername, sPassword);
            }

            public bool Connect()
            {
                try
                {
                    sshc.Connect();
                    return sshc.IsConnected;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            public string RunACommand(string sCommand)
            {                
                SshCommand sshcom = null;
                if (sshc != null && sshc.IsConnected)
                    //sshcom = sshc.RunCommand("etc/init.d/networking restart");
                    sshcom = sshc.RunCommand(sCommand);
                string sResult = sshcom.Result;
                if (string.IsNullOrEmpty(sResult))
                    sResult = sshcom.Error;
                return sResult;
            }

            public void Dissconnect()
            {
                if (sshc.IsConnected)
                {
                    sshc.Disconnect();
                }
                
            }
        }


        /// <summary>Interoperability Helper
        ///     <see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/bb309069(v=vs.85).aspx" />
        /// </summary>
        private static class Interop
        {
            private static IntPtr? icmpHandle;
            private static int? _replyStructLength;

            /// <summary>Returns the application legal icmp handle. Should be close by IcmpCloseHandle
            ///     <see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/aa366045(v=vs.85).aspx" />
            /// </summary>
            public static IntPtr IcmpHandle
            {
                get
                {
                    if (icmpHandle == null)
                    {
                        icmpHandle = IcmpCreateFile();
                        //TODO Close Icmp Handle appropiate
                    }

                    return icmpHandle.GetValueOrDefault();
                }
            }
            /// <summary>Returns the the marshaled size of the reply struct.</summary>
            public static int ReplyMarshalLength
            {
                get
                {
                    if (_replyStructLength == null)
                    {
                        _replyStructLength = Marshal.SizeOf(typeof(Reply));
                    }
                    return _replyStructLength.GetValueOrDefault();
                }
            }


            [DllImport("Iphlpapi.dll", SetLastError = true)]
            private static extern IntPtr IcmpCreateFile();
            [DllImport("Iphlpapi.dll", SetLastError = true)]
            private static extern bool IcmpCloseHandle(IntPtr handle);
            [DllImport("Iphlpapi.dll", SetLastError = true)]
            public static extern uint IcmpSendEcho2Ex(IntPtr icmpHandle, IntPtr Event, IntPtr apcroutine, IntPtr apccontext, UInt32 sourceAddress, UInt32 destinationAddress, byte[] requestData, short requestSize, ref Option requestOptions, IntPtr replyBuffer, int replySize, int timeout);
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
            public struct Option
            {
                public byte Ttl;
                public readonly byte Tos;
                public byte Flags;
                public readonly byte OptionsSize;
                public readonly IntPtr OptionsData;
            }
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
            public struct Reply
            {
                public readonly UInt32 Address;
                public readonly int Status;
                public readonly int RoundTripTime;
                public readonly short DataSize;
                public readonly short Reserved;
                public readonly IntPtr DataPtr;
                public readonly Option Options;
            }
        }
        public static PingReply Ping(IPAddress srcAddress, IPAddress destAddress, int timeout = 5000, byte[] buffer = null, PingOptions po = null)
        {            
            if (destAddress == null || destAddress.AddressFamily != AddressFamily.InterNetwork || destAddress.Equals(IPAddress.Any))
                throw new ArgumentException();

            //Defining pinvoke args
            var source = srcAddress == null ? 0 : BitConverter.ToUInt32(srcAddress.GetAddressBytes(), 0);
            var destination = BitConverter.ToUInt32(destAddress.GetAddressBytes(), 0);
            var sendbuffer = buffer ?? new byte[] { };
            var options = new Interop.Option
            {
                Ttl = (po == null ? (byte)255 : (byte)po.Ttl),
                Flags = (po == null ? (byte)0 : po.DontFragment ? (byte)0x02 : (byte)0) //0x02
            };
            var fullReplyBufferSize = Interop.ReplyMarshalLength + sendbuffer.Length; //Size of Reply struct and the transmitted buffer length.



            var allocSpace = Marshal.AllocHGlobal(fullReplyBufferSize); // unmanaged allocation of reply size. TODO Maybe should be allocated on stack
            try
            {
                DateTime start = DateTime.Now;
                var nativeCode = Interop.IcmpSendEcho2Ex(
                    Interop.IcmpHandle, //_In_      HANDLE IcmpHandle,
                    default(IntPtr), //_In_opt_  HANDLE Event,
                    default(IntPtr), //_In_opt_  PIO_APC_ROUTINE ApcRoutine,
                    default(IntPtr), //_In_opt_  PVOID ApcContext
                    source, //_In_      IPAddr SourceAddress,
                    destination, //_In_      IPAddr DestinationAddress,
                    sendbuffer, //_In_      LPVOID RequestData,
                    (short)sendbuffer.Length, //_In_      WORD RequestSize,
                    ref options, //_In_opt_  PIP_OPTION_INFORMATION RequestOptions,
                    allocSpace, //_Out_     LPVOID ReplyBuffer,
                    fullReplyBufferSize, //_In_      DWORD ReplySize,
                    timeout //_In_      DWORD Timeout
                    );
                TimeSpan duration = DateTime.Now - start;
                var reply = (Interop.Reply)Marshal.PtrToStructure(allocSpace, typeof(Interop.Reply)); // Parse the beginning of reply memory to reply struct

                byte[] replyBuffer = null;
                if (sendbuffer.Length != 0)
                {
                    replyBuffer = new byte[sendbuffer.Length];
                    Marshal.Copy(allocSpace + Interop.ReplyMarshalLength, replyBuffer, 0, sendbuffer.Length); //copy the rest of the reply memory to managed byte[]
                }

                if (nativeCode == 0) //Means that native method is faulted.
                    return new PingReply(nativeCode, reply.Status, new IPAddress(reply.Address), duration);
                else
                    return new PingReply(nativeCode, reply.Status, new IPAddress(reply.Address), reply.RoundTripTime, replyBuffer);
            }
            finally
            {
                Marshal.FreeHGlobal(allocSpace); //free allocated space
            }
        }

        public class cls_Firewall
        {
            [DllImport("grace_pf.dll", CallingConvention = CallingConvention.Cdecl)]
            static private extern IntPtr CreatePacketFilter();

            [DllImport("grace_pf.dll", CallingConvention = CallingConvention.Cdecl)]
            static private extern void DisposePacketFilter(IntPtr pPacketFilterObject);

            [DllImport("grace_pf.dll", CallingConvention = CallingConvention.Cdecl)]
            static private extern void StartTheFirewall(IntPtr pPacketFilterObject);

            [DllImport("grace_pf.dll", CallingConvention = CallingConvention.Cdecl)]
            static private extern void StopTheFirewall(IntPtr pPacketFilterObject);

            [DllImport("grace_pf.dll", CallingConvention = CallingConvention.Cdecl)]
            static private extern void AddToBlockList(IntPtr pPacketFilterObject, string ip);

            private IntPtr pPacketFiltering = IntPtr.Zero;
            private List<string> blockList = null;

            public cls_Firewall()
            {
                pPacketFiltering = CreatePacketFilter();
                blockList = new List<string>();
            }

            public void FirewallStart()
            {
                if (pPacketFiltering != IntPtr.Zero)
                {
                    // Stop all previous ip filters
                    StopTheFirewall(pPacketFiltering);                    

                    foreach (string ip in blockList)
                    {
                        AddToBlockList(pPacketFiltering, ip);
                    }
                    
                    StartTheFirewall(pPacketFiltering);
                }                
            }
            public void FirewallStop()
            {
                if (pPacketFiltering != IntPtr.Zero)
                {
                    StopTheFirewall(pPacketFiltering);
                }
            }

            public void FirewallClear()
            {
                if (pPacketFiltering != IntPtr.Zero)
                {
                    blockList.Clear();
                    StopTheFirewall(pPacketFiltering);
                    DisposePacketFilter(pPacketFiltering);
                    pPacketFiltering = IntPtr.Zero;
                }
            }

            public bool FirewallAddToBlockList(string ip)
            {
                if (pPacketFiltering != IntPtr.Zero)
                {
                    //AddToBlockList(pPacketFiltering, ip);
                    
                    if (blockList.Find(x => x.Equals(ip)) == null) // If ip is not exist on the block list
                    {
                        blockList.Add(ip);
                        return true;
                    }
                    else // If ip is already exist on the list
                    {
                        return false;
                    }
                }
                return false;
            }

            public bool FirewallRemoveFromBlockList(string ip)
            {
                if (pPacketFiltering != IntPtr.Zero)
                {
                    //AddToBlockList(pPacketFiltering, ip);
                    if (blockList.Find(x => x.Equals(ip)) == null) // If ip is not exist on the block list
                    {
                        return false;
                    }
                    else // If ip is already exist on the list
                    {
                        blockList.Remove(ip);
                        return true;
                    }
                }
                return false;
            }
        }
    }

    [Serializable]
    public class PingReply
    {
        private Win32Exception _exception;


        internal PingReply(uint nativeCode, int replystatus, IPAddress ipAddress, TimeSpan duration)
        {
            NativeCode = nativeCode;
            IpAddress = ipAddress;
            if (Enum.IsDefined(typeof(IPStatus), replystatus))
                Status = (IPStatus)replystatus;
        }
        internal PingReply(uint nativeCode, int replystatus, IPAddress ipAddress, int roundTripTime, byte[] buffer)
        {
            NativeCode = nativeCode;
            IpAddress = ipAddress;
            RoundTripTime = TimeSpan.FromMilliseconds(roundTripTime);
            Buffer1 = buffer;
            if (Enum.IsDefined(typeof(IPStatus), replystatus))
                Status = (IPStatus)replystatus;
        }


        /// <summary>Native result from <code>IcmpSendEcho2Ex</code>.</summary>
        public uint NativeCode { get; private set; } = 0;
        public IPStatus Status { get; private set; } = IPStatus.Unknown;
        /// <summary>The source address of the reply.</summary>
        public IPAddress IpAddress { get; private set; } = null;
        public byte[] Buffer
        {
            get { return Buffer1; }
        }
        public TimeSpan RoundTripTime { get; private set; } = TimeSpan.Zero;
        /// <summary>Resolves the <code>Win32Exception</code> from native code</summary>
        public Win32Exception Exception
        {
            get
            {
                if (Status != IPStatus.Success)
                    return _exception ?? (_exception = new Win32Exception((int)NativeCode, Status.ToString()));
                else
                    return null;
            }
        }

        public byte[] Buffer1 { get; private set; } = null;

        public override string ToString()
        {
            if (Status == IPStatus.Success)
                return Status + " from " + IpAddress + " in " + RoundTripTime + " ms with " + Buffer.Length + " bytes";
            else if (Status != IPStatus.Unknown)
                return Status + " from " + IpAddress;
            else
                return Exception.Message + " from " + IpAddress;
        }
    }
}
