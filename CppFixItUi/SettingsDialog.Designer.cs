namespace CppFixItAddIn
{
    partial class SettingsDialog
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
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.button2 = new System.Windows.Forms.Button();
            this.buttonSave = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonBrowserClangExe = new System.Windows.Forms.Button();
            this.textBoxClangExecutable = new System.Windows.Forms.TextBox();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // button2
            // 
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(777, 136);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // buttonSave
            // 
            this.buttonSave.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonSave.Location = new System.Drawing.Point(696, 136);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(75, 23);
            this.buttonSave.TabIndex = 2;
            this.buttonSave.Text = "Save";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.buttonBrowserClangExe);
            this.groupBox2.Controls.Add(this.textBoxClangExecutable);
            this.groupBox2.Location = new System.Drawing.Point(12, 21);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(834, 97);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Clang";
            // 
            // label2
            // 
            this.label2.AccessibleRole = System.Windows.Forms.AccessibleRole.IpAddress;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 26);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Clang executable";
            // 
            // buttonBrowserClangExe
            // 
            this.buttonBrowserClangExe.Location = new System.Drawing.Point(805, 39);
            this.buttonBrowserClangExe.Name = "buttonBrowserClangExe";
            this.buttonBrowserClangExe.Size = new System.Drawing.Size(23, 23);
            this.buttonBrowserClangExe.TabIndex = 3;
            this.buttonBrowserClangExe.Text = "...";
            this.buttonBrowserClangExe.UseVisualStyleBackColor = true;
            this.buttonBrowserClangExe.Click += new System.EventHandler(this.buttonBrowserClangExe_Click);
            // 
            // textBoxClangExecutable
            // 
            this.textBoxClangExecutable.Location = new System.Drawing.Point(9, 42);
            this.textBoxClangExecutable.Name = "textBoxClangExecutable";
            this.textBoxClangExecutable.Size = new System.Drawing.Size(784, 20);
            this.textBoxClangExecutable.TabIndex = 1;
            // 
            // SettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(864, 171);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.button2);
            this.Name = "SettingsDialog";
            this.Text = "SettingsDialog";
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonBrowserClangExe;
        private System.Windows.Forms.TextBox textBoxClangExecutable;
    }
}