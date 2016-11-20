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

        private List<string> filesToUpload;

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
            
            InitializeComponent();
        }

        private void form_client_Load(object sender, EventArgs e)
        {
            this.FormClosing += this.form_client_FormClosing;
            lbl_user.Text = new StringBuilder().Append("Welcome ").Append(utility.Username).ToString();
            lbl_uploadStatus.Text = "No file chosen...";

            //TODO: CALL request file list here

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
            tt_fileListTip.SetToolTip(this.lb_fileList, "Click to see more options...");
        }

        private void form_client_FormClosing(object sender, FormClosingEventArgs e)
        {
            //TODO:check ongoing upload downloads
            Application.Exit();
        }

        private void btn_logout_Click(object sender, EventArgs e)
        {
            //TODO:check ongoing upload downloads
            utility.DisconnectFromServer();

            btn_logout.Enabled = false;
            this.Hide();
            form_login fl = new form_login();
            fl.utility = new Utility();
            fl.Show();
        }

        private void lbl_refresh_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //TODO: call request file list function here
            writeOnConsole("User requested refreshing file list");
            utility.SendString(Utility.REQUEST_FILE_LIST);
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
            btn_upload.Enabled = false;
            lbl_uploadStatus.Text = "Upload starting...";
            writeOnConsole("User started upload request");
            filesToUpload = Regex.Split(txt_filepath.Text, "\" \"").OfType<string>().ToList();

            filesToUpload[0] = filesToUpload[0].Substring(1);
            filesToUpload[filesToUpload.Count - 1] = filesToUpload[filesToUpload.Count - 1].Substring(0, filesToUpload[filesToUpload.Count - 1].Length - 2);
            
            foreach (string s in filesToUpload)
            {
                bool retry = false;

                do
                {
                    if (File.Exists(s))
                    {
                        //TODO: IMPLEMENT HERE
                        lbl_uploadStatus.Text = "Uploading " + s + "...";
                        writeOnConsole("Uploading " + s);

                        // Send a file fileName to the remote device with preBuffer and postBuffer data.

                        // Create the preBuffer data.
                        string string1 = String.Format(Utility.BEGIN_UPLOAD + " file:{0}{1}", s, Environment.NewLine);
                        byte[] preBuf = Encoding.UTF8.GetBytes(string1);

                        // Create the postBuffer data.
                        string string2 = String.Format(Utility.END_UPLOAD + " file: {0}{1}", s, Environment.NewLine);
                        byte[] postBuf = Encoding.UTF8.GetBytes(string2);

                        //Send file s with buffers and default flags to the remote device.
                        //utility.ClientSocket.BeginSendFile(s, preBuf, postBuf, 0, new AsyncCallback(AsyncFileSendCallback), utility.ClientSocket);
                    }
                    else
                    {
                        retry = false;
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
                    Thread.Sleep(1000);
                } while (retry);
            }
            filesToUpload.Clear();
            lbl_uploadStatus.Text = "Upload done";
            writeOnConsole("Upload is finished");
            btn_upload.Enabled = true;
        }

        private void AsyncFileSendCallback(IAsyncResult ar)
        {
            //TODO: writeOnConsole("File uploaded:" + );
            // Retrieve the socket from the state object.
            Socket client = (Socket)ar.AsyncState;

            // Complete sending the data to the remote device.
            client.EndSendFile(ar);
            //TODO: sendDone.Set();
        }

        private void writeOnConsole(string text)
        {
            StringBuilder sb = new StringBuilder().Append("\n>> ").Append(text);
            Invoke((MethodInvoker)delegate
            {
                rtb_activity.AppendText(sb.ToString());
            });
        }
    }
}
