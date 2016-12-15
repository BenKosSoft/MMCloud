﻿using MMClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MMServer
{
    public partial class Form_Server : Form
    {
        // Thread signal.
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        private static Socket serverSocket = null;
        private static Dictionary<Socket, UserState> clientInfo = new Dictionary<Socket, UserState>();

        private const int BUFFER_SIZE = 2097152;
        private static readonly byte[] bufferGlobal = new byte[BUFFER_SIZE];

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
            logText.Text = ">> Hello Server... Please enter Start Server button to start server...";

            //MERT: read only log text
            logText.ReadOnly = true;

            StringBuilder sb = new StringBuilder();
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            path = sb.Append(path).Append(@"\MMCloud\.path").ToString();
            if (File.Exists(path))
            {
                string[] paths = File.ReadAllLines(path);
                cloudPath.Text = paths[0];
            }
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Cloud Path";

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                cloudPath.Text = fbd.SelectedPath;
                writeOnConsole("Cloud path is selected");
            }
        }

        private void startServer_Click(object sender, EventArgs e)
        {
            //create .path
            StringBuilder sb = new StringBuilder();
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            path = sb.Append(path).Append(@"\MMCloud\.path").ToString();

            //cloud path is created
            if (!Directory.Exists(Directory.GetParent(path).ToString()))
            {
                Directory.CreateDirectory(Directory.GetParent(path).ToString());
            }

            StreamWriter sw = File.CreateText(path);
            sw.WriteLine(cloudPath.Text.ToString());
            sw.Flush();
            sw.Close();

            //controls validity of inputs
            ushort port;
            if (!UInt16.TryParse(portText.Text, out port))
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

            //bind the socket
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
            serverSocket.NoDelay = true;
            writeOnConsole("Setting up server...");
            serverSocket.Bind(new IPEndPoint(Utility.getMyIp(), port));
            if (serverSocket.IsBound)
            {
                serverSocket.Listen(100);
                serverSocket.BeginAccept(AcceptCallback, null);
                changeActivenessOfItems();
                writeOnConsole("Server setup complete... To stop, please enter Stop Server button...");

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
                socket.NoDelay = true; //Disable the Nagle Algorithm for this tcp socket.
                socket.BeginReceive(bufferGlobal, 0, BUFFER_SIZE, SocketFlags.None, InitialCallback, socket);
            }
            catch (Exception e) // I cannot seem to avoid this (on exit when properly closing sockets)
            {
                //writeOnConsole(e.Message);
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
            Array.Copy(bufferGlobal, recBuf, received);
            string username = Encoding.UTF8.GetString(recBuf);
            
            bool availableUser = !clientInfo.ContainsValue(new UserState(username, null));

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
                UserState us = new UserState(username, new byte[BUFFER_SIZE]);

                clientInfo.Add(current, us);
                if (Directory.Exists(Path.Combine(cloudPath.Text, username)))
                { //if user exists return her files.
                    //SendFileList(current, username);
                }
                else
                { //create user directory
                    string newPath = Path.Combine(cloudPath.Text, username);
                    Directory.CreateDirectory(newPath);
                    writeOnConsole(username + " directory is created...");
                    string newPathPath = Path.Combine(newPath, ".shared.");
                    File.CreateText(newPathPath).Close();
                }

                try
                {
                    //For each users, buffer is different... thats why we send userBuffer to callback...
                    current.BeginReceive(clientInfo[current].buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, current);
                }
                catch (Exception e)
                {
                    writeOnConsole(e.Message);
                    writeOnConsole(username + " is disconnected from Server...");
                    current.Close();
                    clientInfo.Remove(current);
                    return;
                }
            }
        }

        private void ReceiveCallback(IAsyncResult AR)
        {
            Socket current = (Socket)AR.AsyncState;
            UserState us;
            clientInfo.TryGetValue(current, out us);
            if (!Utility.IsSocketConnected(current))
            {
                writeOnConsole(us.username + " is disconnected from Server...");
                current.Close();
                clientInfo.Remove(current);
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
                writeOnConsole(us.username + " is disconnected from Server...");
                // Don't shutdown because the socket may be disposed and its disconnected anyway.
                current.Close();
                clientInfo.Remove(current);
                return;
            }

            if (received > 0)
            {
                byte[] recBuf = new byte[received];
                lock (us.buffer)
                {
                    Array.Copy(us.buffer, recBuf, received);
                }
                string text = Encoding.UTF8.GetString(recBuf);

                if (text.IndexOf(Utility.BEGIN_UPLOAD) > -1)  //file transfer is started...
                {
                    string filename = text.Split(':')[1];
                    string sizeStr = text.Split(':')[2];
                    long size = long.Parse(sizeStr);
                    us.totalFileSize = size;

                    if (!filename.Contains('.'))
                    {
                        us.currentFileName = filename;
                        us.fileExtension = "";
                    } else
                    {
                        us.currentFileName = filename.Substring(0, filename.LastIndexOf('.'));
                        us.fileExtension = filename.Substring(filename.LastIndexOf('.'));
                    }
                    us.currentFileSize = 0;

                    StringBuilder sb = new StringBuilder().Append("from ").Append(us.username)
                        .Append(": File (filename=").Append(us.currentFileName).Append(us.fileExtension)
                        .Append(", size=").Append(us.totalFileSize).Append(" bytes) is uploading...");
                    writeOnConsole(sb.ToString());
                    string pathstr = Path.Combine(cloudPath.Text, us.username, us.currentFileName);
                    pathstr = pathstr + ".MMCloud";
                    File.Create(pathstr).Close(); //close it...
                }
                else if (text.IndexOf(Utility.REQUEST_FILE_LIST) > -1)
                {
                    //request refresh of list of files
                    SendFileList(current, us.username);
                }
                else if (text.IndexOf(Utility.DELETE_FILE) > -1)
                {
                    StringBuilder sb = new StringBuilder();
                    string filename = text.Split(':')[1];
                    string toBeDeleted = Path.Combine(cloudPath.Text, us.username, filename);
                    if (File.Exists(toBeDeleted))
                    {
                        File.Delete(toBeDeleted);
                        string deletedFileInfo = DeleteFromDisk(filename, us.username);
                        sb.Append(Utility.INFO).Append(":").Append(filename).Append(" is deleted from system.");
                        //revoke(deletedFileInfo, reason:deleted); //UNDONE:
                    }
                    else
                    {
                        sb.Append(Utility.INFO).Append(":ERROR:").Append(filename).Append(" is not available in system.");
                    }
                    byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString().Trim());
                    current.Send(buffer);
                    //TODO: Should i send file list again?
                }
                else if (text.IndexOf(Utility.RENAME_FILE) > -1)
                {
                    StringBuilder sb = new StringBuilder();
                    string[] elements = text.Split(':');
                    string oldFileName = elements[1];
                    string newFileName = elements[2];
                    string directoryPath = Path.Combine(cloudPath.Text, us.username);
                    string renamedFileInfo = RenameFile(oldFileName, newFileName, us.username);
                    if (!renamedFileInfo.Equals(""))
                    {
                        sb.Append(Utility.INFO).Append(":").Append(oldFileName).Append(" is changed to ").Append(newFileName);
                        //revoke(renamedFileInfo, "reason:renamed"); //UNDONE:
                    }
                    else
                    {
                        sb.Append(Utility.INFO).Append(":ERROR:").Append(" File (").Append(oldFileName).Append(") you want to rename is not available in the system.");
                    }
                    byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString().Trim());
                    current.Send(buffer);
                    //TODO: Should i send file list again?
                }
                //TODO: Download request from client
                else if (text.IndexOf(Utility.BEGIN_DOWNLOAD) > -1)
                {
                    StringBuilder sb = new StringBuilder();
                    string[] elements = text.Split(':');
                    string fileName = elements[1];
                    string owner = elements[2];

                    //UNDONE: Think about when someone downloads shared file, while owner is deleting that file
                    string filePath = Path.Combine(cloudPath.Text, owner, fileName);
                    if (File.Exists(filePath))
                    {
                        //UNDONE: Should server send to pre information?
                        sb.Append(Utility.BEGIN_DOWNLOAD).Append(":true");
                        byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString().Trim());
                        current.Send(buffer);
                        //Thread.Sleep(50);
                        current.SendFile(filePath);
                    }else //requested file is not available...
                    {
                        //TODO: change error message accordingly...
                        sb.Append(Utility.INFO).Append(":ERROR:").Append("File that you want to download is not available in the server...");
                        byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString().Trim());
                        current.Send(buffer);
                    }
                }
                //shared
                else if(text.IndexOf(Utility.SHARE_FILE) > -1)
                {
                    StringBuilder sb = new StringBuilder();
                    string[] elements = text.Split(':');
                    string fileName = elements[1];
                    string friend = elements[2];

                    string userPath = Path.Combine(cloudPath.Text, friend);
                    string filePath = Path.Combine(userPath, fileName);
                    if (!Directory.Exists(userPath))
                    {
                        sb.Append(Utility.INFO).Append(":ERROR:").Append("Username is not defined in the system...");
                        byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString().Trim());
                        current.Send(buffer);
                    }
                    else if (!File.Exists(filePath))
                    {
                        sb.Append(Utility.INFO).Append(":ERROR:").Append("File that you want to share is corrupted...");
                        byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString().Trim());
                        current.Send(buffer);
                    }
                    else //no problem with sharing
                    {
                        sb.Append(Utility.INFO).Append(":File->").Append(fileName).Append(" is shared with ").Append(friend);
                        byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString().Trim());

                        //save friend .shared file
                        SaveOnDisk(fileName, us.username, friend);
                        current.Send(buffer);
                    }
                }
                else //upload
                {
                    //get data
                    string userCloudName = Path.Combine(cloudPath.Text, us.username);
                    string filePath = Path.Combine(userCloudName, us.currentFileName + ".MMCloud");
                    lock (us.buffer)
                    {
                        AppendAllBytes(filePath, us.buffer, received);
                    }
                    us.currentFileSize += received;

                    if (us.currentFileSize >= us.totalFileSize) //done
                    {
                        string filename = us.currentFileName + us.fileExtension;
                        string newPath = Path.Combine(userCloudName, filename);
                        if (File.Exists(newPath)) //overriding...
                        {
                            File.Delete(newPath);
                            string deletedFileInfo = DeleteFromDisk(filename, us.username);
                            //revoke(deletedFileInfo, "reason:override"); //UNDONE:
                        }
                        File.Move(filePath, newPath);
                        StringBuilder sb = new StringBuilder().Append("from ").Append(us.username)
                        .Append(": File (filename=").Append(filename)
                        .Append(", size=").Append(us.totalFileSize).Append(" bytes) is uploaded...");
                        writeOnConsole(sb.ToString());
                        SaveOnDisk(filename, us.username, us.username);
                    }

                    //TODO: not needed maybe...
                    lock (us.buffer)
                    {
                        Array.Clear(us.buffer, 0, us.buffer.Length);
                    }
                }
                //TODO: new else ifs will come in the next steps.
            }

            try
            {
                current.BeginReceive(us.buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, current);
            }
            catch (Exception e)
            {
                writeOnConsole(e.Message);
                writeOnConsole(us.username + " is disconnected from Server...");
                current.Close();
                clientInfo.Remove(current);
                return;
            }

        }

        //TODO: test it
        /*
         * returns renamed fileinfo
         */ 
        private string RenameFile(string oldFileName, string newFileName, string username)
        {
            string oldFilePath = Path.Combine(cloudPath.Text, username, oldFileName);
            string newFilePath = Path.Combine(cloudPath.Text, username, newFileName);
            if (File.Exists(oldFilePath))
            {
                if (File.Exists(newFilePath))
                    File.Delete(newFilePath);
                File.Move(oldFilePath, newFilePath);
                StringBuilder sb = new StringBuilder()
                    .Append("from ").Append(username)
                    .Append(": File ").Append(oldFileName)
                    .Append(" is changed to ").Append(newFileName);
                writeOnConsole(sb.ToString());
            }
            else
            {
                writeOnConsole("Failure on renaming file... File does not exist!");
                return "";
            }
            string deletedfileInfo = DeleteFromDisk(oldFileName, username);
            SaveOnDisk(newFileName, username, username);
            return deletedfileInfo;
        }

        /// <summary>
        /// Close all connected client (we do not need to shutdown the server socket as its connections
        /// are already closed with the clients).
        /// </summary>
        private void CloseAllSockets()
        {
            foreach (Socket socket in clientInfo.Keys)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Disconnect(false);
                socket.Close();
            }

            serverSocket.Close();
            writeOnConsole("Server is closing...");
            writeOnConsole("Good bye Server... :(");
        }

        private void stopServer_Click(object sender, EventArgs e)
        {
            try
            {
                CloseAllSockets();
                changeActivenessOfItems();
            }
            catch (Exception ex)
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

        public void AppendAllBytes(string path, byte[] bytes, int size)
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

        //TODO: fix this function...
        private void SendFileList(Socket current, string username)
        {
            string newPath = Path.Combine(cloudPath.Text, username, ".shared.");
            string message = Utility.BEGIN_DOWNLOAD + ":" + "false";
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            current.Send(buffer);
            //Thread.Sleep(50);
            current.SendFile(newPath);
            writeOnConsole(username + "'s files are sent to client...");
        }

        /*
         * filename is name of file
         * owner is owner of file
         * friend is friend of owner (owner of .shared file)
         * if owner == friend then it is not sharing operation
         */
        private void SaveOnDisk(string filename, string owner, string friend)
        {
            string userFileName = Path.Combine(owner, filename);
            string filePath = Path.Combine(cloudPath.Text, userFileName);
            if (File.Exists(filePath))
            {
                FileInfo fileInfo = new FileInfo(filePath);
                long sizeInByte = fileInfo.Length;
                string date = fileInfo.LastWriteTime.ToShortDateString();
                string diskPath = Path.Combine(cloudPath.Text, friend, ".shared.");
                if (File.Exists(diskPath))
                {
                    StringBuilder sb = new StringBuilder().Append(userFileName).Append(':')
                        .Append(date).Append(':').Append(sizeInByte).Append(':').Append(owner).Append(':');
                    StreamWriter writer = File.AppendText(diskPath);
                    writer.WriteLine(sb.ToString());
                    writer.Flush();
                    writer.Close();
                }
                else
                {
                    writeOnConsole("Something wrong with .shared file...");
                }
            }
            else
            {
                writeOnConsole("Something wrong with uploaded file...");
            }
        }


        //TODO: test it
        //Delete one line function is creating new txt, then write lines, on that txt, which will not be deleted, 
        //complexity is high. Think about it
        //[MethodImpl(MethodImplOptions.Synchronized)] 
        /*
         * returns deleted file info
         */ 
        private string DeleteFromDisk(string toBeDeleted, string username)
        {
            string deletedFileinfo = "";
            string usernameFileToBeDeleted = Path.Combine(username, toBeDeleted);
            string usernamePath = Path.Combine(cloudPath.Text, username);
            string diskPath = Path.Combine(usernamePath, ".shared.");
            string tempPath = Path.Combine(usernamePath, "temp.txt");
            string[] allPaths = File.ReadAllLines(diskPath);

            StreamWriter writer = File.AppendText(tempPath);

            foreach (string s in allPaths)
            {
                if (!s.Equals(""))
                {
                    string filePath = s.Split(':')[0];
                    if (!filePath.Trim().Equals(usernameFileToBeDeleted))
                    {
                        writer.WriteLine(s);
                        writer.Flush();
                    }else
                    {
                        deletedFileinfo = s;
                    }
                }
            }
            writer.Close();

            File.Delete(diskPath);
            File.Move(tempPath, diskPath);
            return deletedFileinfo;
        }

        /*
         * Revoke file from shared
         */
        //UNDONE: :(
        /*private void revokeFile(string fileinfo)
        {
            string[] fileinfoelements = fileinfo.Split(':');
            string[] sharedUser = fileinfoelements[4].Split('|');
            UserState [] users = clientInfo.Values;
            foreach (string friend in sharedUser)
            {
                if (!sharedUser.Equals(""))
                {
                    string sharedFilePath = Path.Combine(cloudPath.Text, friend, ".shared.");
                    
                }
            }
        }*/ 

        public class UserState
        {
            public string username { get; set; }
            public byte[] buffer { get; set; }
            public string currentFileName { get; set; }
            public string fileExtension { get; set; }
            public long totalFileSize { get; set; }
            public long currentFileSize { get; set; }

            public override bool Equals(object obj)
            {
                var item = obj as UserState;
                if (item == null) return false;
                return username.Equals(item.username);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public UserState(string username, byte[] buffer)
            {
                this.username = username;
                this.buffer = buffer;
            }
        }
    }
}
