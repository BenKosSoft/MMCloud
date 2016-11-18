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
    public partial class form_client : Form
    {
        public BackgroundTask backgroudTask { get; set; }

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
            lbl_user.Text = new StringBuilder().Append("Welcome ").Append(backgroudTask.Username).ToString();
            lbl_uploadStatus.Text = "No file chosen...";

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
            Application.Exit();
        }

        private void btn_logout_Click(object sender, EventArgs e)
        {
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

        }

        private void btn_upload_Click(object sender, EventArgs e)
        {

        }
    }
}
