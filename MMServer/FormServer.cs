using MMClient;
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

        // File send signal.
        public static ManualResetEvent sendDone = new ManualResetEvent(false);

        private static Socket serverSocket = null;

        //to get userinfo with socket key
        private static Dictionary<Socket, UserState> clientInfo = new Dictionary<Socket, UserState>();

        //to get socket with username key...
        private static Dictionary<string, Socket> usernameSocketMatch = new Dictionary<string, Socket>();

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
            string username = Encoding.UTF8.GetString(recBuf).ToLower();
            
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
                usernameSocketMatch.Add(username, current);
                if (Directory.Exists(Path.Combine(cloudPath.Text, username)))
                { //if user exists return her files.
                    //SendFileList(current, username);
                    //This section is not implemented, because client sends a request when she is online.
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
                    usernameSocketMatch.Remove(username);
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
                usernameSocketMatch.Remove(us.username);
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
                usernameSocketMatch.Remove(us.username);
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
                    string[] elements = text.Split(':');
                    string filename = elements[1];
                    string sizeStr = elements[2];
                    long size = long.Parse(sizeStr);
                    us.totalFileSize = size;

                    if (!filename.Contains('.'))
                    {
                        us.currentFileName = filename;
                        us.fileExtension = "";
                    }
                    else
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

                    //HACK: Fuck the Nagle Algorithm
                    if (elements[3].Length > 0 || elements.Length > 4)
                    {
                        int index = elements[0].Length + elements[1].Length + elements[2].Length + 3;
                        int header = Encoding.UTF8.GetByteCount(text.Substring(0, index));
                        int receivedRawData = received - header;
                        byte[] bytes = new byte[receivedRawData];
                        Array.Copy(recBuf, header, bytes, 0, receivedRawData);
                        AppendAllBytes(pathstr, bytes, receivedRawData);
                        us.currentFileSize += receivedRawData;
                        isUploadDone(us);
                    }
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
                    writeOnConsole(us.username + " requests " + filename + " file to be deleted!");
                    if (File.Exists(toBeDeleted))
                    {
                        try
                        {
                            File.Delete(toBeDeleted);
                            string deletedFileInfo = DeleteFromDisk(filename, us.username, us.username);
                            sb.Append(Utility.INFO).Append(":").Append(filename).Append(" is deleted from system.");
                            writeOnConsole(us.username + " deleted " + filename + " file");
                            revokeFile(deletedFileInfo, "deleted");
                            byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString().Trim());
                            current.Send(buffer);
                        }
                        catch (Exception e)
                        {
                            writeOnConsole("from: " + us.username + "-> You cannot delete at the momment...!");
                        }
                    }
                    else
                    {
                        sb.Append(Utility.INFO).Append(":ERROR:").Append(filename).Append(" is not available in system.");
                        DeleteFromDisk(filename, us.username, us.username);
                        byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString().Trim());
                        current.Send(buffer);
                    }
                }
                else if (text.IndexOf(Utility.RENAME_FILE) > -1)
                {
                    StringBuilder sb = new StringBuilder();
                    string[] elements = text.Split(':');
                    string oldFileName = elements[1].Trim();
                    string newFileName = elements[2].Trim();
                    string directoryPath = Path.Combine(cloudPath.Text, us.username);
                    string renamedFileInfo = RenameFile(oldFileName, newFileName, us.username);

                    if (renamedFileInfo.Equals(""))
                    {
                        sb.Append(Utility.INFO).Append(":ERROR:").Append(" File (").Append(oldFileName).Append(") that want to rename is not available in the system.");
                        DeleteFromDisk(oldFileName, us.username, us.username);
                    }
                    else if (renamedFileInfo.Equals(" "))
                    {
                        sb.Append(Utility.INFO).Append(":ERROR:").Append(" File (").Append(oldFileName).Append(") that want to rename is have same name your new name, bro...");
                    }
                    else if (renamedFileInfo.Equals("  "))
                    {
                        sb.Append(Utility.INFO).Append(":ERROR:").Append(" File (").Append(newFileName).Append(") that want to rename is already exist in the system, bro...");
                    }
                    else if (renamedFileInfo.Equals("   "))
                    {
                        sb.Append(Utility.INFO).Append(":ERROR:").Append(" File (").Append(newFileName).Append(") that want to rename is too long, bro...");
                    }
                    else
                    {
                        sb.Append(Utility.INFO).Append(":").Append(oldFileName).Append(" is changed to ").Append(newFileName);
                        revokeFile(renamedFileInfo, "renamed");
                    }
                    string msg = sb.ToString().Trim();
                    writeOnConsole(msg.Substring(msg.IndexOf(":") + 1));
                    byte[] buffer = Encoding.UTF8.GetBytes(msg);
                    current.Send(buffer);
                }
                else if (text.IndexOf(Utility.BEGIN_DOWNLOAD) > -1)
                {
                    StringBuilder sb = new StringBuilder();
                    string[] elements = text.Split(':');
                    string filename = elements[1];
                    string owner = elements[2];

                    //UNDONE: Think about when someone downloads shared file, while owner is deleting that file, it is implemented, but it is not tested...
                    string filePath = Path.Combine(cloudPath.Text, owner, filename);
                    if (File.Exists(filePath))
                    {
                        writeOnConsole(us.username + " requests " + filename + " file which belong to " + owner);
                        FileInfo fi = new FileInfo(filePath);
                        sb.Append(Utility.BEGIN_DOWNLOAD).Append(":true").Append(":").Append(fi.Length).Append(":");
                        byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString().Trim());
                        try
                        {
                            SendString(current, sb.ToString());
                            current.SendFile(filePath);
                            writeOnConsole(us.username + " has downloaded " + filename + " file which belong to " + owner);
                        }
                        catch (Exception e)
                        {
                            writeOnConsole("from: " + us.username + "-> File download is terminated...!");
                        }
                    }
                    else //requested file is not available...
                    {
                        DeleteFromDisk(filename, us.username, owner);
                        string msg = sb.Append(Utility.INFO).Append(":ERROR:").Append("File that want to download is not available in the server...").ToString().Trim();
                        writeOnConsole(msg.Substring(msg.IndexOf(":") + 1));
                        byte[] buffer = Encoding.UTF8.GetBytes(msg);
                        current.Send(buffer);
                    }
                }
                //shared
                else if (text.IndexOf(Utility.SHARE_FILE) > -1)
                {
                    StringBuilder sb = new StringBuilder();
                    string[] elements = text.Split(':');
                    string filename = elements[1];
                    string friend = elements[2];

                    if (us.username.Equals(friend))
                    {
                        sb.Append(Utility.INFO).Append(":ERROR:").Append("You cannot share file with yourself...");
                        byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString().Trim());
                        current.Send(buffer);
                    }
                    else
                    {
                        string userPath = Path.Combine(cloudPath.Text, us.username);
                        string friendPath = Path.Combine(cloudPath.Text, friend);
                        string filePath = Path.Combine(userPath, filename);
                        if (!Directory.Exists(friendPath))
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
                        else //next step for sharing
                        {
                            //check shared condition
                            string[] friends = returnSharedUsers(filename, us.username);

                            if (friends.Contains(friend)) //file is already shared...
                            {
                                //send error
                                sb.Append(Utility.INFO).Append(":ERROR:").Append("File->").Append(filename).Append(" is already shared with ").Append(friend);
                                byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString().Trim());
                                current.Send(buffer);
                            }
                            else
                            {   //share file
                                sb.Append(Utility.INFO).Append(":File->").Append(filename).Append(" is shared with ").Append(friend);
                                byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString().Trim());

                                //create friends
                                string[] newfriends = new string[friends.Length + 1];
                                friends.CopyTo(newfriends, 0);
                                newfriends[newfriends.Length - 1] = friend;

                                //rewrite own .shared file
                                DeleteFromDisk(filename, us.username, us.username);
                                SaveOnDisk(filename, us.username, us.username, newfriends);

                                //save friend .shared file
                                SaveOnDisk(filename, us.username, friend, new string[] { });
                                current.Send(buffer);
                            }
                        }
                    }
                }
                else if (text.IndexOf(Utility.REVOKE_FILE) > -1)
                {
                    StringBuilder sb = new StringBuilder();
                    string[] elements = text.Split(':');
                    string filename = elements[1];
                    string friend = elements[2];

                    if (us.username.Equals(friend))
                    {
                        sb.Append(Utility.INFO).Append(":ERROR:").Append("You cannot share file with yourself...");
                        byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString().Trim());
                        current.Send(buffer);
                    }else
                    {
                        writeOnConsole("Revoke file request from client: " + us.username + ", shared user: " + friend + ", filename: " + filename);

                        string userPath = Path.Combine(cloudPath.Text, us.username);
                        string friendPath = Path.Combine(cloudPath.Text, friend);
                        string filePath = Path.Combine(userPath, filename);
                        if (!Directory.Exists(friendPath))
                        {
                            sb.Append(Utility.INFO).Append(":ERROR:").Append("user is not defined in the system...");
                            byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString().Trim());
                            current.Send(buffer);
                        }
                        else if (!File.Exists(filePath))
                        {
                            sb.Append(Utility.INFO).Append(":ERROR:").Append("File that you want to revoke is corrupted...");
                            byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString().Trim());
                            current.Send(buffer);
                        }
                        else
                        {// about to revoke

                            //check shared condition
                            string[] friends = returnSharedUsers(filename, us.username);

                            if (!friends.Contains(friend)) //file is not shared...
                            {
                                //send error
                                sb.Append(Utility.INFO).Append(":ERROR:").Append("File->").Append(filename).Append(" is not shared with ").Append(friend);
                                byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString().Trim());
                                current.Send(buffer);
                            }
                            else
                            { //revoke file
                                sb.Append(Utility.INFO).Append(Utility.INFO).Append(":REVOKE:").Append("File-> ").Append(filename)
                                    .Append(" is revoked by ").Append(us.username).Append(" (reason-> ")
                                    .Append("revoke").Append(")");

                                //delete from friends array
                                friends = friends.Where(val => val != friend).ToArray();

                                //rewrite own .shared file
                                DeleteFromDisk(filename, us.username, us.username);
                                SaveOnDisk(filename, us.username, us.username, friends);

                                //delete friend .shared file
                                DeleteFromDisk(filename, friend, us.username);
                                Socket friendSocket = usernameSocketMatch[friend];
                                if (friendSocket != null)
                                {
                                    SendString(friendSocket, sb.ToString().Trim());
                                }
                            }
                        }
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
                    isUploadDone(us);
                }
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
                usernameSocketMatch.Remove(us.username);
                return;
            }

        }

        private string[] returnSharedUsers(string filename, string owner)
        {
            string usernameFile = Path.Combine(owner, filename);
            string usernamePath = Path.Combine(cloudPath.Text, owner);
            string diskPath = Path.Combine(usernamePath, ".shared.");
            string[] allPaths = File.ReadAllLines(diskPath);
            

            foreach (string s in allPaths)
            {
                if (!s.Equals(""))
                {
                    string filePath = s.Split(':')[0];
                    string ownerInFile = s.Split(':')[3];
                    if (filePath.Trim().Equals(usernameFile) &&
                        ownerInFile.Trim().Equals(owner))
                    {
                        string[] friends = s.Split(':')[4].Split('|');
                        friends = friends.Take(friends.Count() - 1).ToArray();
                        return friends;
                    }
                }
            }
            return new string[] { };
        }

        private void isUploadDone(UserState us)
        {
            string userCloudName = Path.Combine(cloudPath.Text, us.username);
            string filePath = Path.Combine(userCloudName, us.currentFileName + ".MMCloud");
            if (us.currentFileSize >= us.totalFileSize) //done
            {
                string filename = us.currentFileName + us.fileExtension;
                string newPath = Path.Combine(userCloudName, filename);
                if (File.Exists(newPath)) //overriding...
                {
                    File.Delete(newPath);
                    string deletedFileInfo = DeleteFromDisk(filename, us.username, us.username);
                    revokeFile(deletedFileInfo, "override");
                }
                File.Move(filePath, newPath);
                StringBuilder sb = new StringBuilder().Append("from ").Append(us.username)
                .Append(": File (filename=").Append(filename)
                .Append(", size=").Append(us.totalFileSize).Append(" bytes) is uploaded...");
                writeOnConsole(sb.ToString());
                SaveOnDisk(filename, us.username, us.username, new string[] { });
            }

            lock (us.buffer)
            {
                Array.Clear(us.buffer, 0, us.buffer.Length);
            }
        }

        /*
         * returns renamed fileinfo
         */
        private string RenameFile(string oldFileName, string newFileName, string username)
        {
            if (oldFileName.Equals(newFileName)) return " "; //it means old file name is equal to new file name...
            string oldFilePath = Path.Combine(cloudPath.Text, username, oldFileName);
            string newFilePath = Path.Combine(cloudPath.Text, username, newFileName);
            if (File.Exists(oldFilePath))
            {
                try
                {
                    File.Move(oldFilePath, newFilePath);
                }
                catch (PathTooLongException)
                {
                    return "   "; // it means io exception from PathTooLongException
                }
                catch(IOException)
                {
                    return "  "; // it means io exception from MOVE
                }
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
            string deletedfileInfo = DeleteFromDisk(oldFileName, username, username);
            SaveOnDisk(newFileName, username, username, new string[] { });
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

        private void SendFileList(Socket current, string username)
        {
            string newPath = Path.Combine(cloudPath.Text, username, ".shared.");
            FileInfo fi = new FileInfo(newPath);
            StringBuilder sb = new StringBuilder();
            sb.Append(Utility.BEGIN_DOWNLOAD).Append(":false").Append(":").Append(fi.Length).Append(":");

            byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString());
            try{
                SendString(current, sb.ToString());
                sendDone.Reset();
                current.BeginSendFile(newPath, null, null, 0, new AsyncCallback(FileSendCallback), current);
                sendDone.WaitOne();
            }catch(Exception e)
            {
               writeOnConsole("File send is terminated...!");
               return;
            }
            writeOnConsole(username + "'s files are sent to client...");
        }

        /*
         * filename is name of file
         * owner is owner of file
         * friend is friend of owner (owner of .shared file)
         * if owner == friend then it is not sharing operation
         */
        private void SaveOnDisk(string filename, string owner, string friend, string [] friends)
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
                    foreach(string f in friends)
                    {
                        sb.Append(f).Append("|");
                    }
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

        /*
         * delete the file information from .shared and returns deleted file info
         */ 
        private string DeleteFromDisk(string toBeDeleted, string username, string owner)
        {
            string deletedFileinfo = "";
            string usernamePath = Path.Combine(cloudPath.Text, username);
            string diskPath = Path.Combine(usernamePath, ".shared.");
            string[] allPaths = File.ReadAllLines(diskPath);
            List<string> newPaths = new List<string>();

            foreach (string s in allPaths)
            {
                if (!s.Equals(""))
                {
                    string filename = s.Split(':')[0].Substring(s.Split(':')[0].IndexOf('\\')+1);
                    string ownerInFile = s.Split(':')[3];

                    bool filenameMatch = filename.Trim().Equals(toBeDeleted);
                    bool ownerMatch = ownerInFile.Trim().Equals(owner);
                    if (!(filenameMatch && ownerMatch))
                    {
                        //add to new string array
                        newPaths.Add(s);
                    }else
                    {
                        deletedFileinfo = s;
                    }
                }
            }

            File.WriteAllLines(diskPath, newPaths.ToArray());
            return deletedFileinfo;
        }

        /*
         * Revoke file from shared
         */
        private void revokeFile(string fileinfo, string reason)
        {
            string[] fileinfoelements = fileinfo.Split(':');
            string filename = fileinfoelements[0].Substring(fileinfoelements[0].IndexOf('\\')+1);
            string owner = fileinfoelements[3];
            string[] sharedUser = fileinfoelements[4].Split('|');
            foreach (string friend in sharedUser)
            {
                if (!friend.Equals(""))
                {
                    string sharedFilePath = Path.Combine(cloudPath.Text, friend, ".shared.");
                    //delete file from .shared of friend.
                    DeleteFromDisk(filename, friend, owner);

                    //send message to client if he is available
                    try
                    {
                        Socket friendSocket = usernameSocketMatch[friend];
                        if(friendSocket != null)
                        {
                            StringBuilder sb = new StringBuilder().Append(Utility.INFO).Append(":REVOKE:").Append("File-> ").Append(filename)
                                .Append(" is revoked by ").Append(owner).Append(" (reason-> ")
                                .Append(reason).Append(")");
                            SendString(friendSocket, sb.ToString().Trim());
                        }
                    }
                    catch (Exception)
                    {
                        writeOnConsole("Something is crashed, during revoke the shared file");
                    }
                }
            }
        }

        public void SendString(Socket current, string text)
        {
            sendDone.Reset();
            byte[] buffer = Encoding.UTF8.GetBytes(text);

            try
            {
                current.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(SendCallback), current);
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
            catch (Exception)
            { }

            // Signal that all bytes have been sent.
            sendDone.Set();
        }

        private void FileSendCallback(IAsyncResult ar)
        {
            // Retrieve the socket from the state object.
            Socket client = (Socket)ar.AsyncState;

            // Complete sending the data to the remote device.
            //writeOnConsole("filesendcallback eneterd");
            try
            {
                client.EndSendFile(ar);
                //Thread.Sleep(2000);
            }
            catch (Exception)
            {
            }
            sendDone.Set();
        }

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
