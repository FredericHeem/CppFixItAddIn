namespace CppFixItAddIn
{
    partial class AboutDialog
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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.licenseInfo = new System.Windows.Forms.Label();
            this.version = new System.Windows.Forms.Label();
            this.copyright = new System.Windows.Forms.Label();
            this.description = new System.Windows.Forms.Label();
            this.title = new System.Windows.Forms.Label();
            this.linkLabelEmailAddress = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::CppFixItAddIn.Resource.LogoCpp_320x320;
            this.pictureBox1.Location = new System.Drawing.Point(12, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(320, 320);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.linkLabelEmailAddress);
            this.panel1.Controls.Add(this.licenseInfo);
            this.panel1.Controls.Add(this.version);
            this.panel1.Controls.Add(this.copyright);
            this.panel1.Controls.Add(this.description);
            this.panel1.Controls.Add(this.title);
            this.panel1.Location = new System.Drawing.Point(303, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(474, 320);
            this.panel1.TabIndex = 1;
            // 
            // licenseInfo
            // 
            this.licenseInfo.AutoSize = true;
            this.licenseInfo.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.licenseInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.licenseInfo.Location = new System.Drawing.Point(4, 206);
            this.licenseInfo.Name = "licenseInfo";
            this.licenseInfo.Size = new System.Drawing.Size(76, 24);
            this.licenseInfo.TabIndex = 4;
            this.licenseInfo.Text = "License";
            // 
            // version
            // 
            this.version.AutoSize = true;
            this.version.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.version.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.version.Location = new System.Drawing.Point(4, 167);
            this.version.Name = "version";
            this.version.Size = new System.Drawing.Size(75, 24);
            this.version.TabIndex = 3;
            this.version.Text = "Version";
            // 
            // copyright
            // 
            this.copyright.AutoSize = true;
            this.copyright.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.copyright.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.copyright.Location = new System.Drawing.Point(3, 126);
            this.copyright.Name = "copyright";
            this.copyright.Size = new System.Drawing.Size(90, 24);
            this.copyright.TabIndex = 2;
            this.copyright.Text = "Copyright";
            // 
            // description
            // 
            this.description.AutoSize = true;
            this.description.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.description.Location = new System.Drawing.Point(4, 87);
            this.description.Name = "description";
            this.description.Size = new System.Drawing.Size(104, 24);
            this.description.TabIndex = 1;
            this.description.Text = "Description";
            this.description.Click += new System.EventHandler(this.label1_Click);
            // 
            // title
            // 
            this.title.AutoSize = true;
            this.title.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.title.Location = new System.Drawing.Point(3, 45);
            this.title.Name = "title";
            this.title.Size = new System.Drawing.Size(99, 25);
            this.title.TabIndex = 0;
            this.title.Text = "CppFixIt";
            // 
            // linkLabelEmailAddress
            // 
            this.linkLabelEmailAddress.AutoSize = true;
            this.linkLabelEmailAddress.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkLabelEmailAddress.Location = new System.Drawing.Point(5, 254);
            this.linkLabelEmailAddress.Name = "linkLabelEmailAddress";
            this.linkLabelEmailAddress.Size = new System.Drawing.Size(229, 20);
            this.linkLabelEmailAddress.TabIndex = 5;
            this.linkLabelEmailAddress.TabStop = true;
            this.linkLabelEmailAddress.Text = "frederic.heem@gmail.com";
            this.linkLabelEmailAddress.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelEmailAddress_LinkClicked);
            // 
            // AboutDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(789, 344);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.pictureBox1);
            this.Name = "AboutDialog";
            this.Text = "About";
            this.Load += new System.EventHandler(this.About_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label title;
        private System.Windows.Forms.Label description;
        private System.Windows.Forms.Label version;
        private System.Windows.Forms.Label copyright;
        private System.Windows.Forms.Label licenseInfo;
        private System.Windows.Forms.LinkLabel linkLabelEmailAddress;
    }
}