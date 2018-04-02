using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace qemu_img_gui
{
    public partial class MainForm : Form
    {
        string qemuimgpath = @"qemu-img\qemu-img.exe";

        public MainForm()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.ShowDialog();
            txtSourceFile.Text = dialog.FileName;
        }

        private void btnBrowseOutput_Click(object sender, EventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.FileName = BuildOutputName();
            dialog.ShowDialog();
            txtOutput.Text = dialog.FileName;
            txtOutput.Text = BuildOutputName();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(txtSourceFile.Text.Trim()))
            {
                MessageBox.Show("Please browse input file.");
                txtSourceFile.Focus();
                return;
            }

            if (String.IsNullOrEmpty(txtOutput.Text.Trim()))
            {
                MessageBox.Show("Please browse output folder.");
                txtOutput.Focus();
                return;
            }

            if (comboBox1.SelectedIndex == -1)
            {
                MessageBox.Show("Please select output format.");
                comboBox1.Focus();
                return;
            }

            if (File.Exists(qemuimgpath))
            {
                if (comboBox1.Text != "vpc" && comboBox1.Text != "vhdx")
                {
                    cbDynamic.Checked = false;
                }

                var dynamic = "";
                if (cbDynamic.Checked)
                    dynamic += "-o subformat=dynamic";

                var command = String.Format("convert {0} -O {1} {2} {3}", 
                    txtSourceFile.Text.Trim(), comboBox1.Text, dynamic, txtOutput.Text.Trim());

                var process = new System.Diagnostics.Process();
                process.StartInfo.FileName = qemuimgpath;
                process.StartInfo.Arguments = command;
                process.Start();
            }
            else
            {
                MessageBox.Show("qemu-img.exe doesn't exist.");
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtOutput.Text = BuildOutputName();
        }

        private string BuildOutputName()
        {
            string tmp = "";
            string tmpOutput = "";
            if (!String.IsNullOrEmpty(txtOutput.Text))
                tmpOutput = txtOutput.Text;
            else
                tmpOutput = txtSourceFile.Text;

            tmp = Path.ChangeExtension(tmpOutput.Trim(), comboBox1.Text);
            tmp = tmp.Replace("vpc", "vhd");
            return tmp;
        }
    }
}
