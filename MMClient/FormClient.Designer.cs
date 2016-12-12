namespace MMClient
{
    partial class form_client
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
            this.components = new System.ComponentModel.Container();
            this.btn_logout = new System.Windows.Forms.Button();
            this.lbl_user = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txt_filepath = new System.Windows.Forms.TextBox();
            this.btn_browse = new System.Windows.Forms.Button();
            this.lbl_fileList = new System.Windows.Forms.Label();
            this.lbl_fileListStatus = new System.Windows.Forms.Label();
            this.btn_upload = new System.Windows.Forms.Button();
            this.rtb_activity = new System.Windows.Forms.RichTextBox();
            this.lbl_activity = new System.Windows.Forms.Label();
            this.lbl_uploadStatus = new System.Windows.Forms.Label();
            this.tt_fileListTip = new System.Windows.Forms.ToolTip(this.components);
            this.lbl_refresh = new System.Windows.Forms.LinkLabel();
            this.lv_fileList = new System.Windows.Forms.ListView();
            this.SuspendLayout();
            // 
            // btn_logout
            // 
            this.btn_logout.Location = new System.Drawing.Point(13, 13);
            this.btn_logout.Name = "btn_logout";
            this.btn_logout.Size = new System.Drawing.Size(54, 23);
            this.btn_logout.TabIndex = 0;
            this.btn_logout.Text = "Logout";
            this.btn_logout.UseVisualStyleBackColor = true;
            this.btn_logout.Click += new System.EventHandler(this.btn_logout_Click);
            // 
            // lbl_user
            // 
            this.lbl_user.AutoSize = true;
            this.lbl_user.Location = new System.Drawing.Point(73, 13);
            this.lbl_user.Name = "lbl_user";
            this.lbl_user.Size = new System.Drawing.Size(35, 13);
            this.lbl_user.TabIndex = 1;
            this.lbl_user.Text = "label1";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 256);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(114, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Choose File to Upload:";
            // 
            // txt_filepath
            // 
            this.txt_filepath.Location = new System.Drawing.Point(8, 272);
            this.txt_filepath.Name = "txt_filepath";
            this.txt_filepath.Size = new System.Drawing.Size(184, 20);
            this.txt_filepath.TabIndex = 3;
            // 
            // btn_browse
            // 
            this.btn_browse.Location = new System.Drawing.Point(197, 270);
            this.btn_browse.Name = "btn_browse";
            this.btn_browse.Size = new System.Drawing.Size(75, 23);
            this.btn_browse.TabIndex = 4;
            this.btn_browse.Text = "Browse";
            this.btn_browse.UseVisualStyleBackColor = true;
            this.btn_browse.Click += new System.EventHandler(this.btn_browse_Click);
            // 
            // lbl_fileList
            // 
            this.lbl_fileList.AutoSize = true;
            this.lbl_fileList.Location = new System.Drawing.Point(12, 51);
            this.lbl_fileList.Name = "lbl_fileList";
            this.lbl_fileList.Size = new System.Drawing.Size(45, 13);
            this.lbl_fileList.TabIndex = 6;
            this.lbl_fileList.Text = "File List:";
            // 
            // lbl_fileListStatus
            // 
            this.lbl_fileListStatus.AutoSize = true;
            this.lbl_fileListStatus.Location = new System.Drawing.Point(9, 230);
            this.lbl_fileListStatus.Name = "lbl_fileListStatus";
            this.lbl_fileListStatus.Size = new System.Drawing.Size(145, 13);
            this.lbl_fileListStatus.TabIndex = 7;
            this.lbl_fileListStatus.Text = "Click an item for more options";
            // 
            // btn_upload
            // 
            this.btn_upload.Location = new System.Drawing.Point(197, 299);
            this.btn_upload.Name = "btn_upload";
            this.btn_upload.Size = new System.Drawing.Size(75, 23);
            this.btn_upload.TabIndex = 8;
            this.btn_upload.Text = "Upload";
            this.btn_upload.UseVisualStyleBackColor = true;
            this.btn_upload.Click += new System.EventHandler(this.btn_upload_Click);
            // 
            // rtb_activity
            // 
            this.rtb_activity.Location = new System.Drawing.Point(8, 350);
            this.rtb_activity.Name = "rtb_activity";
            this.rtb_activity.Size = new System.Drawing.Size(268, 164);
            this.rtb_activity.TabIndex = 9;
            this.rtb_activity.Text = "";
            // 
            // lbl_activity
            // 
            this.lbl_activity.AutoSize = true;
            this.lbl_activity.Location = new System.Drawing.Point(9, 334);
            this.lbl_activity.Name = "lbl_activity";
            this.lbl_activity.Size = new System.Drawing.Size(44, 13);
            this.lbl_activity.TabIndex = 10;
            this.lbl_activity.Text = "Activity:";
            // 
            // lbl_uploadStatus
            // 
            this.lbl_uploadStatus.AutoSize = true;
            this.lbl_uploadStatus.Location = new System.Drawing.Point(5, 299);
            this.lbl_uploadStatus.MaximumSize = new System.Drawing.Size(190, 26);
            this.lbl_uploadStatus.Name = "lbl_uploadStatus";
            this.lbl_uploadStatus.Size = new System.Drawing.Size(74, 13);
            this.lbl_uploadStatus.TabIndex = 11;
            this.lbl_uploadStatus.Text = "Upload Status";
            // 
            // lbl_refresh
            // 
            this.lbl_refresh.AutoSize = true;
            this.lbl_refresh.Location = new System.Drawing.Point(228, 51);
            this.lbl_refresh.Name = "lbl_refresh";
            this.lbl_refresh.Size = new System.Drawing.Size(44, 13);
            this.lbl_refresh.TabIndex = 12;
            this.lbl_refresh.TabStop = true;
            this.lbl_refresh.Text = "Refresh";
            this.lbl_refresh.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lbl_refresh_LinkClicked);
            // 
            // lv_fileList
            // 
            this.lv_fileList.Location = new System.Drawing.Point(12, 68);
            this.lv_fileList.Name = "lv_fileList";
            this.lv_fileList.Size = new System.Drawing.Size(264, 159);
            this.lv_fileList.TabIndex = 13;
            this.lv_fileList.UseCompatibleStateImageBehavior = false;
            this.lv_fileList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lv_fileList_MouseDoubleClicked);
            // 
            // form_client
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 526);
            this.Controls.Add(this.lv_fileList);
            this.Controls.Add(this.lbl_refresh);
            this.Controls.Add(this.lbl_uploadStatus);
            this.Controls.Add(this.lbl_activity);
            this.Controls.Add(this.rtb_activity);
            this.Controls.Add(this.btn_upload);
            this.Controls.Add(this.lbl_fileListStatus);
            this.Controls.Add(this.lbl_fileList);
            this.Controls.Add(this.btn_browse);
            this.Controls.Add(this.txt_filepath);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lbl_user);
            this.Controls.Add(this.btn_logout);
            this.Name = "form_client";
            this.Text = "MMCloud - Client";
            this.Load += new System.EventHandler(this.form_client_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_logout;
        private System.Windows.Forms.Label lbl_user;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txt_filepath;
        private System.Windows.Forms.Button btn_browse;
        private System.Windows.Forms.Label lbl_fileList;
        private System.Windows.Forms.Label lbl_fileListStatus;
        private System.Windows.Forms.Button btn_upload;
        private System.Windows.Forms.RichTextBox rtb_activity;
        private System.Windows.Forms.Label lbl_activity;
        private System.Windows.Forms.Label lbl_uploadStatus;
        private System.Windows.Forms.ToolTip tt_fileListTip;
        private System.Windows.Forms.LinkLabel lbl_refresh;
        private System.Windows.Forms.ListView lv_fileList;
    }
}

