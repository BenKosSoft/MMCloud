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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MMServer
{
    public partial class Form_Server : Form
    {
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
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Cloud Path";

            if (fbd.ShowDialog() == DialogResult.OK)
                cloudPath.Text = fbd.SelectedPath;

            writeOnLog("Cloud path is selected");
        }

        private void startServer_Click(object sender, EventArgs e)
        {
            ushort port;
            if(!UInt16.TryParse(portText.Text, out port))
            {
                writeOnLog("Port number is invalid, please try again...");
                MessageBox.Show("Port number is invalid!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                portText.Clear();
                return;
            }
            if (!Directory.Exists(cloudPath.Text))
            {
                writeOnLog("Folder path is invalid, please try again...");
                MessageBox.Show("Folder path is invalid!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                portText.Clear();
                return;
            }
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
            writeOnLog("Setting up server...");
            serverSocket.Bind(new IPEndPoint(getMyIP(), port));
            if (serverSocket.IsBound)
            {
                serverSocket.Listen(0);
                serverSocket.BeginAccept(AcceptCallback, null);
                changeActivenessOfItems();
                writeOnLog("Server setup complete...");
            }
            else
            {
                writeOnLog("Server is not bound, please try again...");
            }
        }

        private static void AcceptCallback(IAsyncResult AR)
        {
            Socket socket;

            try
            {
                socket = serverSocket.EndAccept(AR);
                //socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, InitialCallback, socket);
            }
            catch (ObjectDisposedException) // I cannot seem to avoid this (on exit when properly closing sockets)
            {
                return;
            }
            serverSocket.BeginAccept(AcceptCallback, null);
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
                socket.Close();
            }

            serverSocket.Close();
            writeOnLog("Good bye Server");
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
                writeOnLog(ex.Message);
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

        private void writeOnLog(string text)
        {
            logText.AppendText(System.Environment.NewLine);
            StringBuilder sb = new StringBuilder().Append(">> ").Append(text);
            logText.AppendText(sb.ToString());

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
    }
}
