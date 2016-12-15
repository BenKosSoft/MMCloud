using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MMClient
{
    /*
     * HACK: This implementation doesn't take possible collisions with uploads, and
     *          possible errors happening on the server into consideration!..
     */     
    public partial class FormFileManagement : Form
    {
        private static new form_client ParentForm;
        public Utility utility { get; set; }
        public string CurrentUser { get; set; }
        public ListViewItem SelectedItem { get; set; }

        public FormFileManagement(form_client parent)
        {
            ParentForm = parent;

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

        private void FormFileManagement_Load(object sender, EventArgs e)
        {
            //TODO: Handle oversized text overflow

            //set labels, filename textbox and button availabilities
            txt_fileName.Text = SelectedItem.SubItems[0].Text;
            lbl_uploadDate.Text = SelectedItem.SubItems[1].Text;
            lbl_fileSize.Text = SelectedItem.SubItems[2].Text + " bytes";
            lbl_owner.Text = SelectedItem.SubItems[3].Text;

            btn_rename.Enabled = false;
            btn_delete.Enabled = false;
            btn_share.Enabled = false;
            txt_share.Enabled = false;
            txt_fileName.Enabled = false;

            //rename, delete, share options available only if the user is the owner.
            if (SelectedItem.SubItems[3].Text == utility.Username)
            {
                this.txt_fileName.TextChanged += new System.EventHandler(this.txt_fileName_TextChanged);
                btn_delete.Enabled = true;
                btn_share.Enabled = true;
                txt_share.Enabled = true;
                txt_fileName.Enabled = true;

                lbl_owner.Text += " (you)";
            }
        }

        private void txt_fileName_TextChanged (object sender, EventArgs e)
        {
            if (txt_fileName.Text == SelectedItem.SubItems[0].Text && btn_rename.Enabled)
            {
                btn_rename.Enabled = false;
            }
            else if (txt_fileName.Text != SelectedItem.SubItems[0].Text && !btn_rename.Enabled)
            {
                btn_rename.Enabled = true;
            }
        }

        private void btn_rename_Click(object sender, EventArgs e)
        {
            txt_fileName.Enabled = false;

            //format = renameKey:OldFileName:NewFileName
            string renameStr = string.Format(Utility.RENAME_FILE + ":{0}:{1}", SelectedItem.SubItems[0].Text, txt_fileName.Text);
            try
            {
                utility.SendString(renameStr);
            }
            catch (SocketException)
            {
                this.DialogResult = DialogResult.Abort;
                this.Close();
            }
            SelectedItem.SubItems[0].Text = txt_fileName.Text;

            txt_fileName.Enabled = true;
        }

        private void btn_share_Click(object sender, EventArgs e)
        {
            txt_share.Enabled = false;

            //format = shareKey:FileName:SharedUser
            string shareStr = string.Format(Utility.SHARE_FILE + ":{0}:{1}", SelectedItem.SubItems[0].Text, txt_share.Text);
            try
            {
                utility.SendString(shareStr);
            }
            catch (SocketException)
            {
                this.DialogResult = DialogResult.Abort;
                this.Close();
            }
        }

        private void btn_browse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Download Path";

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                txt_downloadLoc.Text = fbd.SelectedPath;
                ParentForm.Invoke((MethodInvoker) delegate ()
                {
                    ParentForm.writeOnConsole("Download path changed: " + txt_downloadLoc.Text);
                });
            }
        }

        private void btn_download_Click(object sender, EventArgs e)
        {
            txt_downloadLoc.Enabled = false;

            if (txt_downloadLoc.Text == "")
            {
                MessageBox.Show("Download location cannot be empty!", "Empty Download location!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txt_downloadLoc.Enabled = true;
                return;
            }
            if (!Directory.Exists(txt_downloadLoc.Text))
            {
                MessageBox.Show("Specified folder cannot be found!", "Directory Does Not Exist!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txt_downloadLoc.Enabled = true;
                return;
            }

            //set download path
            ParentForm.DownloadPath = txt_downloadLoc.Text;

            //format = downloadKey:filename:owner
            string downloadStr = string.Format(Utility.BEGIN_DOWNLOAD + ":{0}:{1}", SelectedItem.SubItems[0].Text,
                SelectedItem.SubItems[3].Text);
            try
            {
                //create file templete
                string filename = SelectedItem.SubItems[0].Text;
                File.Create(Path.Combine(txt_downloadLoc.Text,
                   filename.Contains(".") ? filename.Substring(0, filename.LastIndexOf('.')) 
                   : filename + ".MMCloud")).Close();

                utility.SendString(downloadStr);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (SocketException)
            {
                this.DialogResult = DialogResult.Abort;
                this.Close();
            }
        }

        private void btn_delete_Click(object sender, EventArgs e)
        {
            string body = "Do you really want to permenently delete this file from the cloud?";
            string title = "Confirm Deletion";
            MessageBoxButtons button = MessageBoxButtons.YesNo;
            MessageBoxIcon icon = MessageBoxIcon.Exclamation;
            //Show message box
            DialogResult result = MessageBox.Show(body, title, button, icon);
            if (result == DialogResult.Yes)
            {
                //format = deleteKey:filename
                string deleteStr = string.Format(Utility.DELETE_FILE + ":{0}", SelectedItem.SubItems[0].Text);
                try
                {
                    utility.SendString(deleteStr);
                    this.DialogResult = DialogResult.Cancel;
                    this.Close();
                }
                catch (SocketException)
                {
                    this.DialogResult = DialogResult.Abort;
                    this.Close();
                }
            }
            else if (result == DialogResult.No)
                return;
        }
    }
}
