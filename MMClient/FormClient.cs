using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MMClient
{
    public partial class form_client : Form
    {
        public Utility utility { get; set; }

        //File list to upload server
        private List<string> filesToUpload;

        // ManualResetEvent instances signal completion.
        private static ManualResetEvent sendDone =
            new ManualResetEvent(false);
        private static ManualResetEvent receiveDone =
            new ManualResetEvent(false);

        //for server response
        private static bool isFile; //indicates whether we are downloading the fileList or actual file
        private static string currentFileName = Utility.UNNAMED_FILE;
        private StringBuilder FileList;
        private const int BUFFER_SIZE = 2097152; //2MB
        private static readonly byte[] responseBuffer = new byte[BUFFER_SIZE];

        //download path determined by the user
        public string DownloadPath { get; set; }
        private long CurrentFileListSize = 0;
        private long TotalFileListSize = 0;
        private long CurrentFileSize = 0;
        private long TotalFileSize = 0;
        private ListViewItem CurrentFile = null;

        //background worker
        BackgroundWorker backgroundWorker = new BackgroundWorker();

        public form_client()
        {
            // Define the border style of the form to a dialog box.
            FormBorderStyle = FormBorderStyle.FixedDialog;
            // Set the MaximizeBox to false to remove the maximize box.
            MaximizeBox = false;
            // Set the MinimizeBox to false to remove the minimize box.
            MinimizeBox = false;
            // Set the start position of the form to the center of the screen.
            StartPosition = FormStartPosition.CenterScreen;

            FileList = new StringBuilder();

            InitializeComponent();
            InitializeBackgroundWorker();
        }

        private void form_client_Load(object sender, EventArgs e)
        {
            this.FormClosing += this.form_client_FormClosing;
            lbl_user.Text = new StringBuilder().Append("Welcome ").Append(utility.Username).ToString();
            lbl_uploadStatus.Text = "No file chosen...";
            lbl_uploadStatus.AutoEllipsis = true;

            //initilize backgroundworker
            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.WorkerReportsProgress = false;

            //Configure list view
            lv_fileList.View = View.Details;
            lv_fileList.LabelEdit = false;
            lv_fileList.AllowColumnReorder = false;
            lv_fileList.FullRowSelect = true;
            lv_fileList.GridLines = false;
            lv_fileList.Sorting = SortOrder.Ascending;
            lv_fileList.MultiSelect = false;

            //Prepare Headers for file list
            lv_fileList.Columns.Add("File Name", -2, HorizontalAlignment.Left);
            lv_fileList.Columns.Add("Date Modified", -2, HorizontalAlignment.Left);
            lv_fileList.Columns.Add("Size", -2, HorizontalAlignment.Left);
            lv_fileList.Columns.Add("Owner", -2, HorizontalAlignment.Left);

            //CALL request file list here
            lbl_refresh_LinkClicked(sender, null);

            //Update activity
            writeOnConsole("Server connection successful ip:port = " + utility.ServerIp + ":" + utility.Port);
            writeOnConsole("User logged in username: " + utility.Username);

            //Set up the delays for tool tip
            tt_fileListTip.AutoPopDelay = 5000;
            tt_fileListTip.InitialDelay = 1000;
            tt_fileListTip.ReshowDelay = 500;
            // Force the ToolTip text to be displayed whether or not the form is active.
            tt_fileListTip.ShowAlways = true;

            // Set up the ToolTip text for the File list
            tt_fileListTip.SetToolTip(this.lv_fileList, "Double click to see more options...");

            //Start accepting connections from server
            receiveDone.Reset();
            utility.ClientSocket.BeginReceive(responseBuffer, 0, BUFFER_SIZE, SocketFlags.None,
                    new AsyncCallback(ReceiveCallback), utility.ClientSocket);
            receiveDone.WaitOne();
        }

        private void InitializeBackgroundWorker()
        {
            backgroundWorker.DoWork +=
                new DoWorkEventHandler(BackgroundWorker_DoWork);
            backgroundWorker.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(BackgroundWorker_RunWorkerCompleted);
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            foreach (string s in filesToUpload)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }

                bool retry;

                do
                {
                    retry = false;
                    if (File.Exists(s))
                    {
                        string filename = Path.GetFileName(s);
                        this.Invoke((MethodInvoker)delegate ()
                        {
                            lbl_uploadStatus.Text = "Uploading " + filename + "...";
                        });
                        writeOnConsole("Uploading " + s);

                        // Send a file to the remote device with preBuffer data.

                        // Create the preBuffer data.
                        string string1 = string.Format(Utility.BEGIN_UPLOAD + ":{0}:{1}:", filename, new FileInfo(s).Length);

                        //Send file s with buffers and default flags to the remote device.
                        try
                        {
                            utility.SendString(string1);
                            sendDone.Reset();
                            utility.ClientSocket.BeginSendFile(s, null, null, 0, new AsyncCallback(FileSendCallback), utility.ClientSocket);
                            sendDone.WaitOne();
                            if (!Utility.IsSocketConnected(utility.ClientSocket) && !backgroundWorker.CancellationPending)
                                throw new SocketException();
                        }
                        catch (SocketException)
                        {
                            MessageBox.Show("Server connection cannot be established! Logging out....", "Server Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);

                            btn_logout_Click(sender, e);
                        }
                        catch (ObjectDisposedException)
                        {
                            break;
                        }
                    }
                    else
                    {
                        writeOnConsole("Requested file could not be found: " + s);
                        //Configure warning message.
                        string body = new StringBuilder().Append("Specfied file ").Append(s).Append(" cannot be found!").ToString();
                        string title = "File cannot be found";
                        MessageBoxButtons button = MessageBoxButtons.AbortRetryIgnore;
                        MessageBoxIcon icon = MessageBoxIcon.Exclamation;
                        //Show message box
                        DialogResult result = MessageBox.Show(body, title, button, icon);
                        if (result == DialogResult.Abort)
                        {
                            writeOnConsole("User canceled upload.");
                            break;
                        }
                        else if (result == DialogResult.Retry)
                        {
                            writeOnConsole("Retrying current file.");
                            retry = true;
                        }
                        else if (result == DialogResult.Ignore)
                        {
                            writeOnConsole("User skipped current file");
                            continue;
                        }
                    }
                } while (retry);
            }
            this.Invoke((MethodInvoker)delegate ()
           {
               lbl_refresh_LinkClicked(sender, null);
           });
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //Detect if there was an exception
            if (e.Error != null)
            {
                writeOnConsole(e.Error.Message);
            }
            else if (e.Cancelled)
            {
                //UNDONE: CANCEL CODE? when does this happen? think & test
                filesToUpload.Clear();
                writeOnConsole("Upload is canceled");
                txt_filepath.Clear();
                this.Invoke((MethodInvoker)delegate ()
                {
                    lbl_uploadStatus.Text = "Upload canceled";
                    btn_upload.Enabled = true;
                    lv_fileList.Enabled = true;
                    lbl_refresh.Enabled = true;
                });
            }
            else
            {
                filesToUpload.Clear();
                writeOnConsole("Upload is finished");
                txt_filepath.Clear();
                this.Invoke((MethodInvoker)delegate ()
                {
                    lbl_uploadStatus.Text = "Upload done";
                    btn_upload.Enabled = true;
                    lv_fileList.Enabled = true;
                    lbl_refresh.Enabled = true;
                });
            }
        }


        private void form_client_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (backgroundWorker.IsBusy && !backgroundWorker.CancellationPending && Utility.IsSocketConnected(utility.ClientSocket))
            {
                string body = "You have ongoing uploads. Do you really want to cancel them and exit the program?";
                string title = "Cancel ongoing uploads";
                MessageBoxButtons button = MessageBoxButtons.YesNo;
                MessageBoxIcon icon = MessageBoxIcon.Exclamation;
                //Show message box
                DialogResult result = MessageBox.Show(body, title, button, icon);
                if (result == DialogResult.Yes)
                {
                    writeOnConsole("Exiting...");
                    backgroundWorker.CancelAsync();
                }
                else if (result == DialogResult.No)
                {
                    writeOnConsole("Canceled Application exit");
                    e.Cancel = true;
                    return;
                }
            }

            Application.Exit();
        }

        private void btn_logout_Click(object sender, EventArgs e)
        {
            if (backgroundWorker.IsBusy && Utility.IsSocketConnected(utility.ClientSocket))
            {
                string body = "You have ongoing uploads. Do you really want to cancel them and logout?";
                string title = "Cancel ongoing uploads";
                MessageBoxButtons button = MessageBoxButtons.YesNo;
                MessageBoxIcon icon = MessageBoxIcon.Exclamation;
                //Show message box
                DialogResult result = MessageBox.Show(body, title, button, icon);
                if (result == DialogResult.Yes)
                {
                    writeOnConsole("Logging out...");
                    backgroundWorker.CancelAsync();
                }
                else if (result == DialogResult.No)
                {
                    writeOnConsole("Canceled logout");
                    return;
                }
            }

            utility.DisconnectFromServer();

            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate ()
                {
                    this.btn_logout.Enabled = false;
                    this.Hide();
                    form_login fl = new form_login();
                    fl.utility = new Utility();
                    fl.Show();
                });
            }
            else
            {
                btn_logout.Enabled = false;
                this.Hide();
                form_login fl = new form_login();
                fl.utility = new Utility();
                fl.Show();
            }
        }

        private void lbl_refresh_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            writeOnConsole("User requested refreshing file list");
            lbl_fileListStatus.Text = "Refreshing file list....";
            lv_fileList.Items.Clear();
            try
            {
                utility.SendString(Utility.REQUEST_FILE_LIST);
            }
            catch (SocketException)
            {
                MessageBox.Show("Server connection cannot be established! Logging out....", "Server Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btn_logout_Click(sender, e);
            }

            writeOnConsole("Refreshing filelist....");
            lbl_fileListStatus.Text = "Click an item for more options";
        }

        private void lv_fileList_MouseDoubleClicked(object sender, EventArgs e)
        {
            FormFileManagement ffm = new FormFileManagement(this);
            ffm.utility = utility;
            ffm.CurrentUser = utility.Username;
            ffm.SelectedItem = lv_fileList.SelectedItems[0];
            DialogResult result = ffm.ShowDialog(this);

            switch (result)
            {
                case DialogResult.OK:
                    //download button is pressed...
                    writeOnConsole("Downloading file: " + lv_fileList.SelectedItems[0].SubItems[0].Text + "...");
                    CurrentFile = lv_fileList.SelectedItems[0];
                    currentFileName = CurrentFile.SubItems[0].Text;
                    lv_fileList.Enabled = false;
                    lbl_refresh.Enabled = false;
                    btn_upload.Enabled = false;
                    break;
                case DialogResult.No:
                    //delete button is pressed
                    writeOnConsole("Deleting file from cloud: " + lv_fileList.SelectedItems[0].SubItems[0].Text + "...");
                    lbl_refresh_LinkClicked(sender, null);
                    break;
                case DialogResult.Abort:
                    //Server connection cannot be established.
                    MessageBox.Show("Server connection cannot be established! Logging out....", "Server Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btn_logout_Click(sender, e);
                    break;
                default:
                    lbl_refresh_LinkClicked(sender, null);
                    break;
            }
        }

        private void btn_browse_Click(object sender, EventArgs e)
        {
            writeOnConsole("User browsing file system to chose a file.");

            OpenFileDialog openFileDialog = new OpenFileDialog();

            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            openFileDialog.InitialDirectory = path;
            openFileDialog.Filter = "All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Multiselect = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                StringBuilder sb = new StringBuilder();
                foreach (string s in openFileDialog.FileNames)
                {
                    sb.Append("\"").Append(s).Append("\"").Append(" ");
                }
                txt_filepath.Text = sb.ToString();
                lbl_uploadStatus.Text = "Files Selected...";
                writeOnConsole("User chose files.");
            }
        }

        private void btn_upload_Click(object sender, EventArgs e)
        {
            if (txt_filepath.Text == "")
            {
                MessageBox.Show("File path cannot be left empty", "Empty fields!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            btn_upload.Enabled = false;
            lv_fileList.Enabled = false;
            lbl_refresh.Enabled = false;
            lbl_uploadStatus.Text = "Upload starting...";
            writeOnConsole("User started upload request");
            filesToUpload = Regex.Split(txt_filepath.Text, "\" \"").OfType<string>().ToList();

            filesToUpload[0] = filesToUpload[0].Substring(1);
            filesToUpload[filesToUpload.Count - 1] = filesToUpload[filesToUpload.Count - 1].Substring(0, filesToUpload[filesToUpload.Count - 1].Length - 2);

            //Start Sending file asynchronusly
            backgroundWorker.RunWorkerAsync();
        }

        private void FileSendCallback(IAsyncResult ar)
        {
            // Retrieve the socket from the state object.
            Socket client = (Socket)ar.AsyncState;

            // Complete sending the data to the remote device.
            try
            {
                client.EndSendFile(ar);
                Thread.Sleep(2000);
            }
            catch (Exception)
            {
            }
            sendDone.Set();
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            receiveDone.Set();

            // Retrieve the client socket from the asynchronous state object.
            Socket current = (Socket)ar.AsyncState;

            if (!Utility.IsSocketConnected(current))
            {
                //HACK: dude... seriously?
                try
                {
                    int av = current.Available;
                }
                catch (ObjectDisposedException)
                {
                    return;
                }

                writeOnConsole("Server is disconnected!");
                MessageBox.Show("Server connection cannot be established! Logging out....", "Server Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btn_logout_Click(null, null);
                return;
            }

            // Read data from the remote device.
            int bytesRead;

            try
            {
                bytesRead = current.EndReceive(ar);
            }
            catch (Exception e)
            {
                writeOnConsole(e.Message);
                writeOnConsole("Server is disconnected!");
                MessageBox.Show("Server connection cannot be established! Logging out....", "Server Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btn_logout_Click(null, null);
                return;
            }

            if (bytesRead > 0)
            {
                byte[] recBuf = new byte[bytesRead];
                Array.Copy(responseBuffer, recBuf, bytesRead);
                string msg = Encoding.UTF8.GetString(recBuf);

                if (msg.IndexOf(Utility.BEGIN_DOWNLOAD) != -1)
                {
                    string[] elements = msg.Split(':');
                    isFile = bool.Parse(elements[1]);

                    if (isFile)
                        TotalFileSize = long.Parse(elements[2]);
                    else
                        TotalFileListSize = long.Parse(elements[2]);

                    if (!isFile && (elements[3].Length > 0 || elements.Length > 4))
                    {
                        string msgAfterSecond = msg.Substring(msg.IndexOf(elements[2]));
                        string s = msgAfterSecond.Substring(msgAfterSecond.IndexOf(":") + 1);
                        FileList.Append(s);
                        CurrentFileListSize += Encoding.UTF8.GetByteCount(s);
                        showFileList();
                    }
                    else if (isFile && (elements[3].Length > 0 || elements.Length > 4))
                    {
                        string msgAfterSecond = msg.Substring(msg.IndexOf(elements[2]));
                        string s = msgAfterSecond.Substring(msgAfterSecond.IndexOf(":"));

                        int paddingLength = elements[0].Length + elements[1].Length + elements[2].Length + 3;
                        int padding = Encoding.UTF8.GetByteCount(msg.Substring(0, paddingLength));

                        byte[] bytes = new byte[bytesRead - padding];
                        Array.Copy(recBuf, padding, bytes, 0, bytesRead - padding);
                        writeToFile(bytes, bytes.Length);
                    }
                }
                else if (msg.IndexOf(Utility.INFO) != -1)
                {
                    string[] elements = msg.Split(':');

                    StringBuilder consoleMsg = new StringBuilder();

                    switch (elements[1])
                    {
                        case "ERROR":
                            writeOnConsole(consoleMsg.Append("Server Error: ")
                                .Append(elements[2]).ToString());
                            MessageBox.Show(elements[2], "Server Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                        case "REVOKE":
                            //UNDONE
                            break;
                        default:
                            writeOnConsole(consoleMsg.Append("Server Message: ")
                                .Append(elements[1]).ToString());
                            MessageBox.Show(elements[1], "Server Message", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                            break;
                    }
                }
                else if (isFile) //received data belongs to downloaded file
                {
                    writeToFile(recBuf, bytesRead);
                }
                else //received data belongs to filelist
                {
                    FileList.Append(msg);
                    CurrentFileListSize += bytesRead;

                    showFileList();
                }

            }

            lock (responseBuffer)
            {
                Array.Clear(responseBuffer, 0, BUFFER_SIZE);
            }
            try
            {
                // Continue listening to get the rest of the data.
                receiveDone.Reset(); //HACK: not sure about these
                current.BeginReceive(responseBuffer, 0, BUFFER_SIZE, SocketFlags.None,
                    new AsyncCallback(ReceiveCallback), current);
                receiveDone.WaitOne(); //HACK: not sure about these
            }
            catch (Exception e)
            {
                writeOnConsole(e.Message);
                writeOnConsole("Server is disconnected!");
                MessageBox.Show("Server connection cannot be established! Logging out....", "Server Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btn_logout_Click(null, null);
                return;
            }
        }

        private void showFileList()
        {
            if (CurrentFileListSize >= TotalFileListSize)
            {
                this.Invoke((MethodInvoker)delegate ()
                {
                    string[] files = Regex.Split(FileList.ToString(), Environment.NewLine);

                    ListViewItem item;
                    foreach (string s in files)
                    {
                        if (!string.IsNullOrWhiteSpace(s))
                        {
                            string[] data = s.Split(':');

                            if (data.Length < 2) continue;

                            data[0] = data[0].Substring(data[0].IndexOf('\\') + 1);

                            item = new ListViewItem(data);

                            lv_fileList.Items.Add(item);
                        }
                    }
                });
                writeOnConsole("Done refreshing filelist.");
                FileList.Clear();
                CurrentFileListSize = 0;
            }
        }

        private void writeToFile (byte[] recBuf, int bytesRead)
        {
            string pathStr = Path.Combine(DownloadPath,
                currentFileName.Contains(".") ? currentFileName.Substring(0, currentFileName.LastIndexOf('.'))
                : currentFileName);
            pathStr += ".MMCloud";

            Utility.AppendAllBytes(pathStr, recBuf, bytesRead);
            CurrentFileSize += bytesRead;
            
            if (CurrentFileSize >= TotalFileSize)
            {
                string newPath = Path.Combine(DownloadPath, currentFileName);
                if (File.Exists(newPath))
                    File.Delete(newPath);

                File.Move(pathStr, newPath);
                writeOnConsole(new StringBuilder().Append("File Download Finished: Filename = ")
                    .Append(CurrentFile.SubItems[0].Text).Append(" Size= ")
                    .Append(CurrentFileSize).ToString());

                currentFileName = Utility.UNNAMED_FILE;
                DownloadPath = Utility.UNNAMED_DIR;
                CurrentFile = null;
                CurrentFileSize = 0;

                this.Invoke((MethodInvoker)delegate ()
               {
                   lv_fileList.Enabled = true;
                   lbl_refresh.Enabled = true;
                   btn_upload.Enabled = true;
               });
            }
        }

        public void writeOnConsole(string text)
        {
            StringBuilder sb = new StringBuilder().Append("\n>> ").Append(text);
            if (this.rtb_activity.InvokeRequired)
            {
                rtb_activity.Invoke((MethodInvoker)delegate ()
                {
                    this.rtb_activity.AppendText(sb.ToString());
                });
            }
            else
            {
                this.rtb_activity.AppendText(sb.ToString());
            }
        }
    }
}
