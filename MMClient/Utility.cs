using System;
using System.Collections.Generic;
using System.IO;
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
        public static readonly string UPLOAD_CANCELED = "_MMCloud_upload$canceled";
        public static readonly string BEGIN_DOWNLOAD = "_MMCloud_begin$download";
        public static readonly string END_DOWNLOAD = "_MMCloud_end$download";
        public static readonly string DOWNLOAD_CANCELED = "_MMCloud_download$canceled";
        public static readonly string REQUEST_FILE_LIST = "_MMCloud_request$file$list";
        public static readonly string REQUEST_FILE = "_MMCloud_request$file";
        public static readonly string RENAME_FILE = "_MMCloud_rename$file";
        public static readonly string DELETE_FILE = "_MMCloud_delete$file";
        public static readonly string SHARE_FILE = "_MMCloud_share$file";
        public static readonly string REVOKE_FILE = "_MMCloud_revoke$file";
        public static readonly string INFO = "_MMCloud_info$";
        public static readonly string UNNAMED_FILE = "_MMCloud_unnamed:file";
        public static readonly string UNNAMED_DIR = "_MMCloud_unnamed:directory";

        public Socket ClientSocket { get; set; }
        public ushort Port { get; set; }
        public IPAddress ServerIp { get; set; }
        public string Username { get; set; }

        // ManualResetEvent instance signal completion.
        private static ManualResetEvent sendDone =
            new ManualResetEvent(false);

        //=================================================================================
        //public static functions
        //=================================================================================
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

        public static void AppendAllBytes(string path, byte[] bytes, int size)
        {
            bool isFileExists = File.Exists(path);

            using (var stream = new FileStream(path, FileMode.Append))
            {
                if (isFileExists)
                    stream.Seek(stream.Length, SeekOrigin.Begin);
                stream.Write(bytes, 0, size);
                stream.Flush();
                stream.Close();
            }
        }
        //=================================================================================
        //=================================================================================

        public void ConnectToServer()
        {
            ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ClientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
            ClientSocket.NoDelay = true; //Disable Nagle's Algorithm.

            //Connect to remote server
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
            sendDone.Reset();
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            
            try
            {
                ClientSocket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(SendCallback), ClientSocket);
            }
            catch (Exception)
            {
                throw;
            }
            sendDone.WaitOne();
        }

        private void SendCallback(IAsyncResult ar)
        {
            // Retrieve the socket from the state object.
            Socket client = (Socket)ar.AsyncState;

            // Complete sending the data to the remote device.
            try
            {
                int bytesSent = client.EndSend(ar);
            }
            catch(Exception)
            {}

            // Signal that all bytes have been sent.
            sendDone.Set();
        }
    }
}
