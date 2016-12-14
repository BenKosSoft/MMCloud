namespace MMClient
{
    partial class FormFileManagement
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
            this.lbl_fileName = new System.Windows.Forms.Label();
            this.txt_fileName = new System.Windows.Forms.TextBox();
            this.btn_download = new System.Windows.Forms.Button();
            this.btn_rename = new System.Windows.Forms.Button();
            this.btn_delete = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.lbl_uploadDate = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lbl_owner = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txt_downloadLoc = new System.Windows.Forms.TextBox();
            this.btn_browse = new System.Windows.Forms.Button();
            this.txt_share = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btn_share = new System.Windows.Forms.Button();
            this.lbl_status = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lbl_fileSize = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lbl_fileName
            // 
            this.lbl_fileName.AutoSize = true;
            this.lbl_fileName.Location = new System.Drawing.Point(12, 9);
            this.lbl_fileName.Name = "lbl_fileName";
            this.lbl_fileName.Size = new System.Drawing.Size(57, 13);
            this.lbl_fileName.TabIndex = 0;
            this.lbl_fileName.Text = "File Name:";
            // 
            // txt_fileName
            // 
            this.txt_fileName.Location = new System.Drawing.Point(12, 25);
            this.txt_fileName.Name = "txt_fileName";
            this.txt_fileName.Size = new System.Drawing.Size(182, 20);
            this.txt_fileName.TabIndex = 1;
            // 
            // btn_download
            // 
            this.btn_download.Location = new System.Drawing.Point(214, 173);
            this.btn_download.Name = "btn_download";
            this.btn_download.Size = new System.Drawing.Size(75, 23);
            this.btn_download.TabIndex = 2;
            this.btn_download.Text = "Download";
            this.btn_download.UseVisualStyleBackColor = true;
            this.btn_download.Click += new System.EventHandler(this.btn_download_Click);
            // 
            // btn_rename
            // 
            this.btn_rename.Location = new System.Drawing.Point(119, 51);
            this.btn_rename.Name = "btn_rename";
            this.btn_rename.Size = new System.Drawing.Size(75, 23);
            this.btn_rename.TabIndex = 3;
            this.btn_rename.Text = "Rename";
            this.btn_rename.UseVisualStyleBackColor = true;
            this.btn_rename.Click += new System.EventHandler(this.btn_rename_Click);
            // 
            // btn_delete
            // 
            this.btn_delete.Location = new System.Drawing.Point(295, 173);
            this.btn_delete.Name = "btn_delete";
            this.btn_delete.Size = new System.Drawing.Size(75, 23);
            this.btn_delete.TabIndex = 4;
            this.btn_delete.Text = "Delete";
            this.btn_delete.UseVisualStyleBackColor = true;
            this.btn_delete.Click += new System.EventHandler(this.btn_delete_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(196, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(73, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Uploaded On:";
            // 
            // lbl_uploadDate
            // 
            this.lbl_uploadDate.AutoSize = true;
            this.lbl_uploadDate.Location = new System.Drawing.Point(279, 9);
            this.lbl_uploadDate.Name = "lbl_uploadDate";
            this.lbl_uploadDate.Size = new System.Drawing.Size(35, 13);
            this.lbl_uploadDate.TabIndex = 6;
            this.lbl_uploadDate.Text = "label3";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(232, 32);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Owner:";
            // 
            // lbl_owner
            // 
            this.lbl_owner.AutoSize = true;
            this.lbl_owner.Location = new System.Drawing.Point(279, 32);
            this.lbl_owner.Name = "lbl_owner";
            this.lbl_owner.Size = new System.Drawing.Size(35, 13);
            this.lbl_owner.TabIndex = 8;
            this.lbl_owner.Text = "label5";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 114);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(135, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Chose Download Location:";
            // 
            // txt_downloadLoc
            // 
            this.txt_downloadLoc.Location = new System.Drawing.Point(12, 130);
            this.txt_downloadLoc.Name = "txt_downloadLoc";
            this.txt_downloadLoc.Size = new System.Drawing.Size(182, 20);
            this.txt_downloadLoc.TabIndex = 10;
            // 
            // btn_browse
            // 
            this.btn_browse.Location = new System.Drawing.Point(200, 128);
            this.btn_browse.Name = "btn_browse";
            this.btn_browse.Size = new System.Drawing.Size(75, 23);
            this.btn_browse.TabIndex = 11;
            this.btn_browse.Text = "Browse";
            this.btn_browse.UseVisualStyleBackColor = true;
            this.btn_browse.Click += new System.EventHandler(this.btn_browse_Click);
            // 
            // txt_share
            // 
            this.txt_share.Location = new System.Drawing.Point(12, 86);
            this.txt_share.Name = "txt_share";
            this.txt_share.Size = new System.Drawing.Size(182, 20);
            this.txt_share.TabIndex = 12;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 70);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(63, 13);
            this.label3.TabIndex = 13;
            this.label3.Text = "Share With:";
            // 
            // btn_share
            // 
            this.btn_share.Location = new System.Drawing.Point(200, 84);
            this.btn_share.Name = "btn_share";
            this.btn_share.Size = new System.Drawing.Size(75, 23);
            this.btn_share.TabIndex = 14;
            this.btn_share.Text = "Share";
            this.btn_share.UseVisualStyleBackColor = true;
            this.btn_share.Click += new System.EventHandler(this.btn_share_Click);
            // 
            // lbl_status
            // 
            this.lbl_status.AutoSize = true;
            this.lbl_status.Location = new System.Drawing.Point(12, 173);
            this.lbl_status.Name = "lbl_status";
            this.lbl_status.Size = new System.Drawing.Size(110, 13);
            this.lbl_status.TabIndex = 15;
            this.lbl_status.Text = "Chose an Operation...";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(224, 56);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(49, 13);
            this.label5.TabIndex = 16;
            this.label5.Text = "File Size:";
            // 
            // lbl_fileSize
            // 
            this.lbl_fileSize.AutoSize = true;
            this.lbl_fileSize.Location = new System.Drawing.Point(279, 56);
            this.lbl_fileSize.Name = "lbl_fileSize";
            this.lbl_fileSize.Size = new System.Drawing.Size(35, 13);
            this.lbl_fileSize.TabIndex = 17;
            this.lbl_fileSize.Text = "label6";
            // 
            // FormFileManagement
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(382, 208);
            this.Controls.Add(this.lbl_fileSize);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.lbl_status);
            this.Controls.Add(this.btn_share);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txt_share);
            this.Controls.Add(this.btn_browse);
            this.Controls.Add(this.txt_downloadLoc);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lbl_owner);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lbl_uploadDate);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btn_delete);
            this.Controls.Add(this.btn_rename);
            this.Controls.Add(this.btn_download);
            this.Controls.Add(this.txt_fileName);
            this.Controls.Add(this.lbl_fileName);
            this.Name = "FormFileManagement";
            this.Text = "File Details";
            this.Load += new System.EventHandler(this.FormFileManagement_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbl_fileName;
        private System.Windows.Forms.TextBox txt_fileName;
        private System.Windows.Forms.Button btn_download;
        private System.Windows.Forms.Button btn_rename;
        private System.Windows.Forms.Button btn_delete;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lbl_uploadDate;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lbl_owner;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txt_downloadLoc;
        private System.Windows.Forms.Button btn_browse;
        private System.Windows.Forms.TextBox txt_share;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btn_share;
        private System.Windows.Forms.Label lbl_status;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lbl_fileSize;
    }
}