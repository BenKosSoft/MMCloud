using MMClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
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

        private readonly object syncLock = new object();

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
            ipLabel.Text = Utility.getMyIp().ToString();
            logText.Text = ">> Hello Server";

            //MERT: read only log text
            logText.ReadOnly = true;

            StringBuilder sb = new StringBuilder();
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            path = sb.Append(path).Append(@"\MMCloud\.path").ToString();
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

            writeOnConsole("Cloud path is selected");
        }

        private void startServer_Click(object sender, EventArgs e)
        {
            //create .path
            StringBuilder sb = new StringBuilder();
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            path = sb.Append(path).Append(@"\MMCloud\.path").ToString();
            StreamWriter sw = File.CreateText(path);
            sw.WriteLine(cloudPath.Text.ToString());
            sw.Flush();
            sw.Close();

            ushort port;
            if(!UInt16.TryParse(portText.Text, out port))
            {
                writeOnConsole("Port number is invalid, please try again...");
                MessageBox.Show("Port number is invalid!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                portText.Clear();
                return;
            }
            if (!Directory.Exists(cloudPath.Text))
            {
                writeOnConsole("Folder path is invalid, please try again...");
                MessageBox.Show("Folder path is invalid!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                portText.Clear();
                return;
            }
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
            writeOnConsole("Setting up server...");
            serverSocket.Bind(new IPEndPoint(Utility.getMyIp(), port));
            if (serverSocket.IsBound)
            {
                serverSocket.Listen(100);
                serverSocket.BeginAccept(AcceptCallback, null);
                changeActivenessOfItems();
                writeOnConsole("Server setup complete...");

                //create .log file
                string logFile = Path.Combine(cloudPath.Text, ".log.");
                if (!File.Exists(logFile))
                {
                    File.Create(logFile);
                    writeOnConsole("Log file is created...");
                }
            }
            else
            {
                writeOnConsole("Server is not bound, please try again...");
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
                writeOnConsole("client is disconnected from Server...");
                current.Close();
                return;
            }

            byte[] recBuf = new byte[received];
            Array.Copy(buffer, recBuf, received);
            string username = Encoding.ASCII.GetString(recBuf);

            bool availableUser = !clientSockets.ContainsValue(username);

            if (!availableUser)
            {
                writeOnConsole(username + " is already in the system...");
                current.Shutdown(SocketShutdown.Both);
                current.Disconnect(false);
                current.Close();
            }
            else
            {
                writeOnConsole(username + " is connected, welcome to the cloud...");
                clientSockets.Add(current, username);
                if (Directory.Exists(Path.Combine(cloudPath.Text, username))){ //if user exists return her files.
                    SendFileList(current, username);
                }
                else{ //create user directory
                    string newPath = Path.Combine(cloudPath.Text, username);
                    Directory.CreateDirectory(newPath);
                    writeOnConsole(username + " directory is created...");
                    string newPathPath = Path.Combine(newPath, ".shared.");
                    File.CreateText(newPathPath).Close();
                }

                try
                {
                    current.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, current);
                }
                catch (Exception e)
                {
                    writeOnConsole(e.Message);
                    writeOnConsole(username + " is disconnected from Server...");
                    current.Close();
                    clientSockets.Remove(current);
                    return;
                }
            }
        }

        private void ReceiveCallback(IAsyncResult AR)
        {
            Socket current = (Socket)AR.AsyncState;
            string username;
            clientSockets.TryGetValue(current, out username);
            if (!Utility.IsSocketConnected(current))
            {
                writeOnConsole(username + " is disconnected from Server...");
                current.Close();
                clientSockets.Remove(current);
                return;
            }

            int received;
            try
            {
                received = current.EndReceive(AR);
            }
            catch (Exception e)
            {
                writeOnConsole(e.Message);
                writeOnConsole(username + " is disconnected from Server...");
                // Don't shutdown because the socket may be disposed and its disconnected anyway.
                current.Close();
                clientSockets.Remove(current);
                return;
            }

            if(received > 0)
            {
                byte[] recBuf = new byte[received];
                Array.Copy(buffer, recBuf, received);
                string text = Encoding.ASCII.GetString(recBuf);

                if (text.IndexOf(Utility.BEGIN_UPLOAD) > -1)  //file transfer is started...
                {
                    //create filename on .path file
                    string filename = text.Split(':')[1];
                    StreamWriter sw = File.CreateText(Path.Combine(cloudPath.Text, username, ".path"));
                    sw.WriteLine(filename);
                    sw.Flush();
                    sw.Close();

                    writeOnConsole("File is coming...");
                    string msg = "File is uploading... Please wait...";
                    byte[] data = Encoding.ASCII.GetBytes(msg);
                    try { current.Send(data); }
                    catch (Exception e)
                    {
                        writeOnConsole(e.Message);
                        writeOnConsole(username + " is disconnected from Server...");
                        current.Close();
                        clientSockets.Remove(current);
                        return;
                    }
                    string pathstr = Path.Combine(cloudPath.Text, username, filename);
                    File.Create(pathstr).Close(); //close it...
                    //current.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, FileCallBack, current);
                }
                else if (text.IndexOf(Utility.REQUEST_FILE_LIST) > -1)
                {
                    //request refresh of list of files
                    SendFileList(current, username);
                }
                else if (text.IndexOf(Utility.DELETE_FILE) > -1)
                    //TODO: Delete From Disk function will be covered again...
                {
                    string toBeDeleted = text.Split(':')[1];
                    File.Delete(toBeDeleted);
                    DeleteFromDisk(toBeDeleted, username);
                }
                else
                {
                    //get data
                    string filePath = File.ReadAllLines(Path.Combine(cloudPath.Text, username, ".path"))[0];
                    filePath = Path.Combine(cloudPath.Text, username, filePath);
                    AppendAllBytes(filePath, buffer);
                }
                //TODO: new else ifs will come in the next steps.
            }
            
            try
            {
                current.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, current);
            }catch(Exception e)
            {
                writeOnConsole(e.Message);
                writeOnConsole(username + " is disconnected from Server...");
                current.Close();
                clientSockets.Remove(current);
                return;
            }
        }

        //private void FileCallBack(IAsyncResult AR)
        //{
        //    Socket current = (Socket)AR.AsyncState;
        //    string username;
        //    clientSockets.TryGetValue(current, out username);
        //    if (!Utility.IsSocketConnected(current))
        //    {
        //        writeOnConsole(username + " is disconnected from Server...");
        //        current.Close();
        //        clientSockets.Remove(current);
        //        return;
        //    }

        //    int received;
        //    try
        //    {
        //        received = current.EndReceive(AR);
        //    }
        //    catch (Exception e)
        //    {
        //        writeOnConsole(e.Message);
        //        writeOnConsole(username + " is disconnected from Server...");
        //        // Don't shutdown because the socket may be disposed and its disconnected anyway.
        //        current.Close();
        //        clientSockets.Remove(current);
        //        return;
        //    }

        //    writeOnConsole(string.Format("received size: {0}", received));
        //    if(received > 0)
        //    {
        //       byte[] recBuf = new byte[received];
        //       Array.Copy(buffer, recBuf, received);
        //       string text = Encoding.ASCII.GetString(recBuf);

        //       //get data
        //       string filePath = File.ReadAllLines(Path.Combine(cloudPath.Text, username, ".path"))[0];
        //       filePath = Path.Combine(cloudPath.Text, username, filePath);
        //       AppendAllBytes(filePath, buffer);
        //       current.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, FileCallBack, current);
        //    }
        //    else
        //    {
        //        writeOnConsole("File upload is done...");
        //        string filePath = File.ReadAllLines(Path.Combine(cloudPath.Text, username, ".path"))[0];
        //        SaveOnDisk(filePath, username);
        //        //string msg = "File upload is done...";
        //        //byte[] data = Encoding.ASCII.GetBytes(msg);
        //        /*try { current.Send(data); }
        //        catch (Exception e)
        //        {
        //            writeOnConsole(e.Message);
        //            writeOnConsole(username + " is disconnected from Server...");
        //            current.Close();
        //            clientSockets.Remove(current);
        //            return;
        //        }*/
        //    }
        //    current.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, current);
        //}

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
            writeOnConsole("Good bye Server");
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
                writeOnConsole(ex.Message);
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

        private void writeOnConsole(string text)
        {
            StringBuilder sb = new StringBuilder().Append("\n>> ").Append(text);
            Invoke((MethodInvoker)delegate
            {
                logText.AppendText(sb.ToString());
            });
        }

        public void AppendAllBytes(string path, byte[] bytes)
        {
            bool isFileExists = File.Exists(path);

            using (
            var stream = new FileStream(path, FileMode.Append))
            {
                if (isFileExists)
                    stream.Seek(stream.Length, SeekOrigin.Begin);
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush();
                stream.Close();
            }
        }

        private void SendFileList(Socket current, string username)
        {
            string newPath = Path.Combine(cloudPath.Text, username, ".shared.");
            string pre = "Pre Message";
            string post = "Post Message";
            current.SendFile(newPath, Encoding.ASCII.GetBytes(pre), Encoding.ASCII.GetBytes(post), TransmitFileOptions.UseDefaultWorkerThread);
            /*string[] files = File.ReadAllLines(newPath);
            foreach (string s in files)
            {
                //send filename path to the client
                byte[] data = Encoding.ASCII.GetBytes(s);
                try
                {
                    current.Send(data);
                }
                catch (Exception e)
                {

                }
            }*/
            writeOnConsole(username + "'s files are sent to client...");
        }

        private void SaveOnDisk(string filePath, string username)
        {
            filePath = Path.Combine(cloudPath.Text, username, filePath);
            if (File.Exists(filePath))
            {
                FileInfo fileInfo = new FileInfo(filePath);
                long sizeInKb = fileInfo.Length / 1024;
                string date = fileInfo.LastWriteTime.ToShortDateString();
                string owner = username;
                string diskPath = Path.Combine(cloudPath.Text, username, ".shared.");
                string fileName = fileInfo.Name;
                if (File.Exists(diskPath))
                {
                    StringBuilder sb = new StringBuilder().Append(fileName).Append(':')
                        .Append(sizeInKb).Append(" KB").Append(':').Append(date).Append(':').Append(owner).Append(':');
                    StreamWriter writer = File.AppendText(diskPath);
                    writer.WriteLine(sb.ToString());
                    writer.WriteLine();
                    writer.Flush();
                    writer.Close();
                }else
                {
                    writeOnConsole("Something wrong with .shared file...");
                }
            }
            else
            {
                writeOnConsole("Something wrong with uploaded file...");
            }
        }

        //Delete one line function is creating new txt, then write lines, on that txt, which will not be deleted, 
        //complexity is high. Think about it
        //[MethodImpl(MethodImplOptions.Synchronized)] 
        private void DeleteFromDisk(string toBeDeleted, string username)
        {
            lock (syncLock) //mutex
            {
                string usernamePath = Path.Combine(cloudPath.Text, username);
                string diskPath = Path.Combine(usernamePath, ".shared.");
                string tempPath = Path.Combine(usernamePath, "temp.txt");
                string[] allPaths = File.ReadAllLines(diskPath);
                StreamWriter writer = File.AppendText(tempPath);
                string[] users = { };

                foreach (string s in allPaths)
                {
                    if (!s.Equals(""))
                    {
                        string filePath = s.Split(':')[0];
                        if (!filePath.Trim().Equals(toBeDeleted))
                        {
                            writer.WriteLine(s);
                            writer.WriteLine();
                            writer.Flush();
                        }else
                        {
                            users = s.Split(':')[4].Split('|');
                        }
                    }
                }
                writer.Close();

                //this is wrong, it cannot be recursive...
                foreach(string user in users)
                {
                    DeleteFromDisk(toBeDeleted, user);
                }

                File.Delete(diskPath);
                File.Move(tempPath, diskPath);
            }
        }
    }
}
