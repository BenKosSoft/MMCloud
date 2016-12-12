using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MMClient
{
    public partial class FormFileManagement : Form
    {
        private static new form_client ParentForm;
        public string FileName { get; set; }
        public string OwnerName { get; set; }

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
            //set DialogResult values returned from the buttons
            btn_download.DialogResult = DialogResult.OK;
            btn_delete.DialogResult = DialogResult.Abort;
        }

        private void btn_rename_Click(object sender, EventArgs e)
        {

        }

        private void btn_share_Click(object sender, EventArgs e)
        {

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

        }

        private void btn_delete_Click(object sender, EventArgs e)
        {

        }
    }
}
