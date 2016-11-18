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
        private static Socket ClientSocket = new Socket
           (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static ushort Port;
        private IPAddress ServerIp;
        private static string Username;
        private BackgroundTask backgroudTask;

        public form_login()
        {
            // Define the border style of the form to a dialog box.
            FormBorderStyle = FormBorderStyle.FixedDialog;
            // Set the MaximizeBox to false to remove the maximize box.
            MaximizeBox = false;
            // Set the start position of the form to the center of the screen.
            StartPosition = FormStartPosition.CenterScreen;

            backgroudTask = new BackgroundTask();

            InitializeComponent();
        }

        private void form_login_Load(object sender, EventArgs e)
        {
            txt_password.Enabled = false;
            this.FormClosing += Form_login_FormClosing;
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
            backgroudTask.ServerIp = ServerIp;

            if (!UInt16.TryParse(txt_port.Text, out Port))
            {
                MessageBox.Show("Wrong Port number!", "Wrong Port!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txt_port.Clear();
                return;
            }
            backgroudTask.Port = Port;

            if (!Regex.IsMatch(txt_username.Text, @"^[\w\- ]+$"))
            {
                MessageBox.Show("Username cannot contain \" . + \\ / : * ? \" < > |\" and cannot have white spaces", "Wrong username format!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txt_username.Clear();
                return;
            }
            Username = txt_username.Text;
            backgroudTask.Username = Username;

            backgroudTask.ConnectToServer();
            btn_connect.Enabled = false;
            this.Hide();
            form_client fc = new form_client();
            fc.backgroudTask = backgroudTask;
            fc.Show();

        }
    }
}