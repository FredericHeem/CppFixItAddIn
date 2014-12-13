using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CppFixItAddIn
{
    public partial class AboutDialog : Form
    {
     

        
        public AboutDialog()
        {
            InitializeComponent();
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void About_Load(object sender, EventArgs e)
        {
            var assemblyInfo = new AssemblyInfo<AboutDialog>();
            title.Text = assemblyInfo.Product;
            description.Text = assemblyInfo.Description;
            copyright.Text = assemblyInfo.Copyright;
            version.Text = assemblyInfo.Version;
            licenseInfo.Text = "Open source";
            
            
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void linkLabelEmailAddress_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string emailCommand = "mailto:" + linkLabelEmailAddress.Text + "?subject=CppFixItAddIn";
            System.Diagnostics.Process.Start(emailCommand);
        }
    }
}
