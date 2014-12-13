using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;

namespace CppFixItAddIn
{
    /// <summary>
    /// Settings dialog
    /// </summary>
    public partial class SettingsDialog : Form
    {
        public SettingsDialog()
        {
            InitializeComponent();
            textBoxClangExecutable.Text = Settings.Default.ClangExeFullPath;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (File.Exists(textBoxClangExecutable.Text) || (textBoxClangExecutable.Text.Length == 0))
            {
                Settings.Default.ClangExeFullPath = textBoxClangExecutable.Text;
                Settings.Default.Save();
            }
            else {
                MessageBox.Show("clang executable does not exist", "CppFixIt Error", MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            }
        }

        private void buttonBrowserClangExe_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK) // Test result.
            {
                string file = openFileDialog1.FileName;
                textBoxClangExecutable.Text = file;
            }
        }
    }
}
