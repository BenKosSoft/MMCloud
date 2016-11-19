﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MMClient
{
    public partial class form_login : Form
    {
        //Utiliy'nin icinde socket static oldugu icin ve initilizae edildigi icin burada gerek yok diye dusundum.
        //private static Socket ClientSocket = new Socket
        //   (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static ushort Port;
        private IPAddress ServerIp;
        private static string Username;
        public Utility utility { get; set; }

        public form_login()
        {
            // Define the border style of the form to a dialog box.
            FormBorderStyle = FormBorderStyle.FixedDialog;
            // Set the MaximizeBox to false to remove the maximize box.
            MaximizeBox = false;
            // Set the start position of the form to the center of the screen.
            StartPosition = FormStartPosition.CenterScreen;

            utility = new Utility();

            InitializeComponent();
        }

        private void form_login_Load(object sender, EventArgs e)
        {
            txt_password.Enabled = false;
            this.FormClosing += Form_login_FormClosing;

            this.KeyPress += Form_login_KeyPress;
            txt_username.KeyPress += Form_login_KeyPress;
            txt_ip.KeyPress += Form_login_KeyPress;
            txt_port.KeyPress += Form_login_KeyPress;
        }

        private void Form_login_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                e.Handled = true;
                btn_connect_Click(sender, e);
            }
        }

        private void Form_login_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void btn_connect_Click(object sender, EventArgs e)
        {
            if (txt_ip.Text==""||txt_port.Text==""||txt_username.Text=="")
            {
                MessageBox.Show("Fields cannot be left empty","Empty fields!",MessageBoxButtons.OK,MessageBoxIcon.Error);
                return;
            }

            if (!IPAddress.TryParse(txt_ip.Text, out ServerIp))
            {
                MessageBox.Show("Wrong IP address format!", "Wrong IP!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txt_ip.Clear();
                return;
            }
            utility.ServerIp = ServerIp;

            if (!UInt16.TryParse(txt_port.Text, out Port))
            {
                MessageBox.Show("Wrong Port number!", "Wrong Port!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txt_port.Clear();
                return;
            }
            utility.Port = Port;

            if (!Regex.IsMatch(txt_username.Text, @"^[\w\- ]+$"))
            {
                MessageBox.Show("Username cannot contain \" . + \\ / : * ? \" < > |\" and cannot have white spaces", "Wrong username format!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txt_username.Clear();
                return;
            }
            Username = txt_username.Text;
            utility.Username = Username;

            utility.ConnectToServer();

            //Mert: burada check ediyor, baglanabilmis mi diye, message box'a ne yazacagimiz degisebilir...
            if (!Utility.IsSocketConnected(Utility.ClientSocket))
            {
                MessageBox.Show("username is already exist in the system!", "Error!");
            }else
            {
                btn_connect.Enabled = false;
                this.Hide();
                form_client fc = new form_client();
                fc.utility = utility;
                fc.Show();
            }
        }
    }
}
