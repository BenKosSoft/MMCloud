using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MMClient
{
    public class Utility
    {
        public static readonly string BEGIN_UPLOAD = "_MMCloud_begin$upload";
        public static readonly string END_UPLOAD = "_MMCloud_end$upload";
        public static readonly string BEGIN_DOWNLOAD = "_MMCloud_begin$download";
        public static readonly string END_DOWNLOAD = "_MMCloud_end$download";
        public static readonly string REQUEST_FILE_LIST = "_MMCloud_request$file$list";
        public static readonly string REQUEST_FILE = "_MMCloud_request$file";
        public static readonly string RENAME_FILE = "_MMCloud_rename$file";
        public static readonly string DELETE_FILE = "_MMCloud_delete$file";
        public static readonly string SHARE_FILE = "_MMCloud_share$file";

        public Socket ClientSocket { get; set; } 
        public ushort Port { get; set; }
        public IPAddress ServerIp { get; set; }
        public string Username { get; set; }

        public void ConnectToServer()
        {
            ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ClientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
            ClientSocket.Connect(ServerIp, Port);
            SendString(Username);
            Thread.Sleep(1000);
        }


        public void DisconnectFromServer()
        {
            ClientSocket.Shutdown(SocketShutdown.Both);
            ClientSocket.Close();
        }

        public void SendString(string text)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(text);
            ClientSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
        }

        public static IPAddress getMyIp()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }
            return IPAddress.Loopback;
        }

        public static bool IsSocketConnected(Socket s)
        {
            return !(!s.Connected || (s.Poll(1000, SelectMode.SelectRead) && (s.Available == 0)));
        }
    }
}
