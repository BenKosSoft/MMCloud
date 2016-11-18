using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MMServer
{
    public partial class Form_Server : Form
    {
        // Thread signal.
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        private static Socket serverSocket = null;
        private static Dictionary<Socket,string> clientSockets = new Dictionary<Socket,string>();

        private const int BUFFER_SIZE = 2048;
        private static readonly byte[] buffer = new byte[BUFFER_SIZE];

        public Form_Server()
        {
            // Define the border style of the form to a dialog box.
            FormBorderStyle = FormBorderStyle.FixedDialog;
            // Set the MaximizeBox to false to remove the maximize box.
            MaximizeBox = false;
            // Set the start position of the form to the center of the screen.
            StartPosition = FormStartPosition.CenterScreen;
            InitializeComponent();
        }

        private void Form_Server_Load(object sender, EventArgs e)
        {
            ipLabel.Text = getMyIP().ToString();
            logText.Text = ">> Hello Server";

            string path = "C:\\Users\\Mert\\Documents\\cloud\\.path";
            if (File.Exists(path))
            {
                string [] paths = File.ReadAllLines(path);
                cloudPath.Text = paths[0];
            }
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Cloud Path";

            if (fbd.ShowDialog() == DialogResult.OK)
                cloudPath.Text = fbd.SelectedPath;

            string path = "C:\\Users\\Mert\\Documents\\cloud\\.path";
            StreamWriter sw = File.CreateText(path);
            sw.WriteLine(cloudPath.Text.ToString());
            sw.Flush();
            sw.Close();

            writeOnConsol("Cloud path is selected");
        }

        private void startServer_Click(object sender, EventArgs e)
        {
            ushort port;
            if(!UInt16.TryParse(portText.Text, out port))
            {
                writeOnConsol("Port number is invalid, please try again...");
                MessageBox.Show("Port number is invalid!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                portText.Clear();
                return;
            }
            if (!Directory.Exists(cloudPath.Text))
            {
                writeOnConsol("Folder path is invalid, please try again...");
                MessageBox.Show("Folder path is invalid!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                portText.Clear();
                return;
            }
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
            writeOnConsol("Setting up server...");
            serverSocket.Bind(new IPEndPoint(getMyIP(), port));
            if (serverSocket.IsBound)
            {
                serverSocket.Listen(100);
                serverSocket.BeginAccept(AcceptCallback, null);
                changeActivenessOfItems();
                writeOnConsol("Server setup complete...");

                //create .log file
                string logFile = Path.Combine(cloudPath.Text, ".log.");
                if (!File.Exists(logFile))
                {
                    File.Create(logFile);
                    writeOnConsol("Log file is created...");
                }
            }
            else
            {
                writeOnConsol("Server is not bound, please try again...");
            }
        }

        private void AcceptCallback(IAsyncResult AR)
        {

            // Signal the main thread to continue.
            allDone.Set(); //?? burada kod patliyor olabilir...

            Socket socket;

            try
            {
                socket = serverSocket.EndAccept(AR);
                socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, InitialCallback, socket);
            }
            catch (ObjectDisposedException) // I cannot seem to avoid this (on exit when properly closing sockets)
            {
                return;
            }
            serverSocket.BeginAccept(AcceptCallback, null);
        }

        private void InitialCallback(IAsyncResult AR)
        {
            Socket current = (Socket)AR.AsyncState;

            int received;

            try
            {
                received = current.EndReceive(AR);
            }
            catch (SocketException)
            {
                Console.WriteLine("Client forcefully disconnected");
                current.Close();
                return;
            }

            byte[] recBuf = new byte[received];
            Array.Copy(buffer, recBuf, received);
            string username = Encoding.ASCII.GetString(recBuf);

            bool availableUser = !clientSockets.ContainsValue(username);

            if (!availableUser)
            {
                writeOnConsol(username + " is already in the system...");
                current.Shutdown(SocketShutdown.Both);
                current.Disconnect(false);
                current.Close();
            }
            else
            {
                writeOnConsol(username + " is connected, welcome to the cloud...");
                clientSockets.Add(current, username);
                if (Directory.Exists(Path.Combine(cloudPath.Text, username))){ //if user exists return her files.
                    string newPath = Path.Combine(cloudPath.Text, username, ".shared.");
                    string [] files = File.ReadAllLines(newPath);
                    foreach(string s in files)
                    {
                        string[] info = s.Split(':');
                        string fileName = info[0];
                        uint sizeInKB = UInt32.Parse(info[1]);
                        string date = info[2];
                        string owner = info[3];
                        string[] users = info[4].Split('|');
                    }
                }else{ //create user directory
                    string newPath = Path.Combine(cloudPath.Text, username);
                    Directory.CreateDirectory(newPath);
                    writeOnConsol(username + " directory is created...");
                    string newPathPath = Path.Combine(newPath, ".shared.");
                    File.CreateText(newPathPath);
                }
                current.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, current);
            }
        }

        private void ReceiveCallback(IAsyncResult AR)
        {
            Socket current = (Socket)AR.AsyncState;
            string username;
            clientSockets.TryGetValue(current, out username);
            if (!IsSocketConnected(current))
            {
                writeOnConsol(username + " is disconnected from Server...");
                return;
            }

            int received;
            try
            {
                received = current.EndReceive(AR);
            }
            catch (SocketException)
            {
                Console.WriteLine("Client forcefully disconnected");
                // Don't shutdown because the socket may be disposed and its disconnected anyway.
                current.Close();
                clientSockets.Remove(current);
                return;
            }

            byte[] recBuf = new byte[received];
            Array.Copy(buffer, recBuf, received);
            string text = Encoding.ASCII.GetString(recBuf);

            if (text.Equals("pre"))
            {
                writeOnConsol("File is coming...");
                string msg = "File is coming... Please wait...";
                byte[] data = Encoding.ASCII.GetBytes(msg);
                current.Send(data);
            }
            else if (text.Equals("post"))
            {
                writeOnConsol("File upload is done...");
                string msg = "File upload is done...";
                byte[] data = Encoding.ASCII.GetBytes(msg);
                current.Send(data);
            }
            else
            {
                //string newPath = Path.Combine(cloudPath, username, filename????);
                //AppendAllBytes(newPath, buffer);
            }
            try
            {
                current.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, current);
            }catch(Exception e)
            {
                writeOnConsol(username + " is disconnected from Server...");
                return;
            }
        }

        /// <summary>
        /// Close all connected client (we do not need to shutdown the server socket as its connections
        /// are already closed with the clients).
        /// </summary>
        private void CloseAllSockets()
        {
            foreach (Socket socket in clientSockets.Keys)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Disconnect(false);
                socket.Close();
            }

            serverSocket.Close();
            writeOnConsol("Good bye Server");
        }

        private void stopServer_Click(object sender, EventArgs e)
        {
            try
            {
                CloseAllSockets();
                changeActivenessOfItems();
            }
            catch(Exception ex)
            {
                writeOnConsol(ex.Message);
            }
        }

        private void changeActivenessOfItems()
        {
            startServer.Enabled = !startServer.Enabled;
            stopServer.Enabled = !stopServer.Enabled;
            cloudPath.Enabled = !cloudPath.Enabled;
            portText.Enabled = !portText.Enabled;
            browseButton.Enabled = !browseButton.Enabled;
        }

        private void writeOnConsol(string text)
        {
            StringBuilder sb = new StringBuilder().Append("\n>> ").Append(text);
            Invoke((MethodInvoker)delegate
            {
                logText.AppendText(sb.ToString());
            });
        }

        public static void AppendAllBytes(string path, byte[] bytes)
        {
            bool isFileExists = File.Exists(path);

            using (var stream = new FileStream(path, FileMode.Append))
            {
                if(isFileExists)
                    stream.Seek(stream.Length, SeekOrigin.Begin);
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        private static IPAddress getMyIP()
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

        private static bool IsSocketConnected(Socket s)
        {
            if (!s.Connected)
            {
                return false;
            }
            return !((s.Poll(1000, SelectMode.SelectRead) && (s.Available == 0)) || !s.Connected);
        }
    }
}
