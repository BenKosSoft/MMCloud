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
        public static Socket ClientSocket { get; set; } //Mert: socket'i static yaptim, her yerde ayni olsun diye...
        public ushort Port { get; set; }
        public IPAddress ServerIp { get; set; }
        public string Username { get; set; }

        public void ConnectToServer()
        {
            //Mert: while loop'una gerek yok diye dusundum, cunku baglanamiyorsa, surekli baglanmaya calismayacagiz, sadece Message Box verebiliriz
            // onu login formunda yaptim, connect To Server cagirilinca...
            ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ClientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
            ClientSocket.Connect(ServerIp, Port);
            SendString(Username);
            Thread.Sleep(1000);
        }

        //Mert: bunu ekledim, server anlaasin diye client'in ciktigini socket'in kapanmasi gerekiyor.
        public void DisconnectFromServer()
        {
            ClientSocket.Shutdown(SocketShutdown.Both);
            ClientSocket.Close();
        }

        private void SendString(string text)
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
            return !((s.Poll(1000, SelectMode.SelectRead) && (s.Available == 0)) || !s.Connected);
        }
    }
}
