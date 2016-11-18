using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MMClient
{
    public partial class form_client : Form
    {
        public BackgroundTask backgroundTask { get; set; }

        private string[] filesToUpload;

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
            lbl_user.Text = new StringBuilder().Append("Welcome ").Append(backgroundTask.Username).ToString();
            lbl_uploadStatus.Text = "No file chosen...";

            //Update activity
            writeOnConsole("Server connection successful");
            writeOnConsole("User logged in username: " + backgroundTask.Username);

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
            btn_logout.Enabled = false;
            this.Hide();
            form_login fl = new form_login();
            fl.backgroudTask = new BackgroundTask();
            fl.Show();
        }

        private void lbl_refresh_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //TODO: call request file list function here
        }

        private void btn_browse_Click(object sender, EventArgs e)
        {
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
            }
        }

        private void btn_upload_Click(object sender, EventArgs e)
        {
            btn_upload.Enabled = false;
            lbl_uploadStatus.Text = "Upload starting...";
            writeOnConsole("User started upload request");
            filesToUpload = Regex.Split(txt_filepath.Text, "\" \"");

            filesToUpload[0] = filesToUpload[0].Substring(1);
            filesToUpload[filesToUpload.Length - 1] = filesToUpload[filesToUpload.Length - 1].Substring(0, filesToUpload[filesToUpload.Length - 1].Length - 2);

            //TODO: upload request
            foreach (string s in filesToUpload)
            {
                bool retry = false;

                do
                {
                    if (File.Exists(s))
                    {
                        //TODO: IMPLEMENT HERE
                    }
                    else
                    {
                        retry = false;
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
