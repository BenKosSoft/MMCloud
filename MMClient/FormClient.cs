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
        private StringBuilder Response;
        private byte[] responseBuffer = new byte[1024];

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

            Response = new StringBuilder();

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

            //Prepare Headers for file list
            lv_fileList.Columns.Add("File Name", -2, HorizontalAlignment.Left);
            lv_fileList.Columns.Add("Date Modified", -2, HorizontalAlignment.Left);
            lv_fileList.Columns.Add("Size", -2, HorizontalAlignment.Left);
            lv_fileList.Columns.Add("Owner", -2, HorizontalAlignment.Left);

            //CALL request file list here
            //lbl_refresh_LinkClicked(sender, (LinkLabelLinkClickedEventArgs)e);

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
            tt_fileListTip.SetToolTip(this.lv_fileList, "Click to see more options...");
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
                        string string1 = String.Format(Utility.BEGIN_UPLOAD + ":{0}:{1}", filename, new FileInfo(s).Length);

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
                //UNDONE: CANCEL CODE
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
            try
            {
                utility.SendString(Utility.REQUEST_FILE_LIST);
            }
            catch (SocketException)
            {
                MessageBox.Show("Server connection cannot be established! Logging out....", "Server Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btn_logout_Click(sender, e);
            }

            try
            {
                ReceiveResponse();
                receiveDone.WaitOne();
            }
            catch (SocketException)
            {
                MessageBox.Show("Server connection cannot be established! Logging out....", "Server Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btn_logout_Click(sender, e);
            }

            // File list has arrived; process it.
            if (Response.Length > 1)
            {
                string[] files = Regex.Split(Response.ToString(), "\n");
                foreach (string s in files)
                {
                    string[] data = Regex.Split(s, ":");
                    ListViewItem item = new ListViewItem(data);
                    lv_fileList.Items.Add(item);
                }
            }
            Response.Clear();
            writeOnConsole("Done refreshing file list.");
            lbl_fileListStatus.Text = "Click an item for more options";
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
            //writeOnConsole("filesendcallback eneterd");
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

        public void ReceiveResponse()
        {
            // Begin receiving the data from the remote device.
            utility.ClientSocket.BeginReceive(responseBuffer, 0, responseBuffer.Length, SocketFlags.None,
                    new AsyncCallback(ReceiveCallback), utility.ClientSocket);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            //TODO: If checks to distinguish between response types
            // Retrieve the client socket from the asynchronous state object.
            Socket client = (Socket)ar.AsyncState;

            // Read data from the remote device.
            int bytesRead = client.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There might be more data, so store the data received so far.
                Response.Append(Encoding.UTF8.GetString(responseBuffer, 0, bytesRead));

                // Get the rest of the data.
                client.BeginReceive(responseBuffer, 0, responseBuffer.Length, SocketFlags.None,
                    new AsyncCallback(ReceiveCallback), client);
            }
            else
            {
                //TODO: End of file recieve, any processing??
                // Signal that all bytes have been received.
                receiveDone.Set();
            }
        }

        private void writeOnConsole(string text)
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
