namespace MMServer
{
    partial class Form_Server
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.logText = new System.Windows.Forms.RichTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.startServer = new System.Windows.Forms.Button();
            this.stopServer = new System.Windows.Forms.Button();
            this.browseButton = new System.Windows.Forms.Button();
            this.cloudPath = new System.Windows.Forms.TextBox();
            this.ipLabel = new System.Windows.Forms.Label();
            this.portText = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(392, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "SERVER";
            // 
            // logText
            // 
            this.logText.Location = new System.Drawing.Point(77, 150);
            this.logText.Name = "logText";
            this.logText.Size = new System.Drawing.Size(694, 279);
            this.logText.TabIndex = 3;
            this.logText.Text = "";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(222, 77);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 17);
            this.label2.TabIndex = 4;
            this.label2.Text = "PORT:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(74, 77);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(24, 17);
            this.label3.TabIndex = 5;
            this.label3.Text = "IP:";
            // 
            // startServer
            // 
            this.startServer.Location = new System.Drawing.Point(395, 77);
            this.startServer.Name = "startServer";
            this.startServer.Size = new System.Drawing.Size(173, 23);
            this.startServer.TabIndex = 6;
            this.startServer.Text = "START SERVER";
            this.startServer.UseVisualStyleBackColor = true;
            this.startServer.Click += new System.EventHandler(this.startServer_Click);
            // 
            // stopServer
            // 
            this.stopServer.Enabled = false;
            this.stopServer.Location = new System.Drawing.Point(588, 77);
            this.stopServer.Name = "stopServer";
            this.stopServer.Size = new System.Drawing.Size(183, 23);
            this.stopServer.TabIndex = 7;
            this.stopServer.Text = "STOP SERVER";
            this.stopServer.UseVisualStyleBackColor = true;
            this.stopServer.Click += new System.EventHandler(this.stopServer_Click);
            // 
            // browseButton
            // 
            this.browseButton.Location = new System.Drawing.Point(77, 105);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(142, 23);
            this.browseButton.TabIndex = 8;
            this.browseButton.Text = "BROWSE";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
            // 
            // cloudPath
            // 
            this.cloudPath.Location = new System.Drawing.Point(225, 106);
            this.cloudPath.Name = "cloudPath";
            this.cloudPath.Size = new System.Drawing.Size(546, 22);
            this.cloudPath.TabIndex = 9;
            // 
            // ipLabel
            // 
            this.ipLabel.AutoSize = true;
            this.ipLabel.Location = new System.Drawing.Point(100, 77);
            this.ipLabel.Name = "ipLabel";
            this.ipLabel.Size = new System.Drawing.Size(70, 17);
            this.ipLabel.TabIndex = 10;
            this.ipLabel.Text = "ipaddress";
            // 
            // portText
            // 
            this.portText.Location = new System.Drawing.Point(280, 77);
            this.portText.Name = "portText";
            this.portText.Size = new System.Drawing.Size(100, 22);
            this.portText.TabIndex = 11;
            // 
            // Form_Server
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(852, 453);
            this.Controls.Add(this.portText);
            this.Controls.Add(this.ipLabel);
            this.Controls.Add(this.cloudPath);
            this.Controls.Add(this.browseButton);
            this.Controls.Add(this.stopServer);
            this.Controls.Add(this.startServer);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.logText);
            this.Controls.Add(this.label1);
            this.Name = "Form_Server";
            this.Text = "Server";
            this.Load += new System.EventHandler(this.Form_Server_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RichTextBox logText;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button startServer;
        private System.Windows.Forms.Button stopServer;
        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.TextBox cloudPath;
        private System.Windows.Forms.Label ipLabel;
        private System.Windows.Forms.TextBox portText;
    }
}

