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
using System.Management;

namespace qemu_img_gui
{
    public partial class MainForm : Form
    {
        string qemuimgpath = @"qemu-img\qemu-img.exe";
        List<ManagementObject> physicalDiskList = null;

        public MainForm()
        {
            InitializeComponent();
            GetPhysicalDriveList();
        }

        private void GetPhysicalDriveList()
        {
            var query = new WqlObjectQuery("SELECT * FROM Win32_DiskDrive");
            using (var searcher = new ManagementObjectSearcher(query))
            {
                physicalDiskList = searcher.Get()
                                 .OfType<ManagementObject>()
                                 .ToList();

                var ds = physicalDiskList.Select(o => $"{o.Properties["DeviceID"].Value} | {o.Properties["Model"].Value} | {(Double.Parse(o.Properties["Size"].Value.ToString())/1073741824).ToString("N2")} GiB")
                    .ToList();

                cmbSourcePhysical.DataSource = ds;
            }
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
            dialog.FileName = BuildOutputName(dialog.FileName);
            var filter = cmbOutput.Text.Replace("vpc", "vhd");
            dialog.Filter = "Virtual Disk | *." + filter;
            dialog.ShowDialog();
            
            if((sender as Button).Name == "btnBrowseOutput")
                txtOutput.Text = BuildOutputName(dialog.FileName);
            else if ((sender as Button).Name == "btnBrowseOutputPhysical")
                txtOutputPhysical.Text = BuildOutputName(dialog.FileName);
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

            if (cmbOutput.SelectedIndex == -1)
            {
                MessageBox.Show("Please select output format.");
                cmbOutput.Focus();
                return;
            }

            if (File.Exists(qemuimgpath))
            {
                if (cmbOutput.Text != "vpc" && cmbOutput.Text != "vhdx")
                {
                    cbDynamic.Checked = false;
                }

                var dynamic = "";
                if (cbDynamic.Checked)
                    dynamic += "-o subformat=dynamic";

                var command = String.Format("convert {0} -O {1} {2} {3}", 
                    "\"" + txtSourceFile.Text.Trim()+ "\"", cmbOutput.Text, dynamic, "\""+txtOutput.Text.Trim()+ "\"");

                var process = new System.Diagnostics.Process();
                process.StartInfo.FileName = qemuimgpath;
                process.StartInfo.Arguments = command;
                process.EnableRaisingEvents = true;
                process.Exited += new EventHandler(myProcess_HasExited);
                process.Start();
            }
            else
            {
                MessageBox.Show("qemu-img.exe doesn't exist.");
            }
        }

        private void cmbOutput_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtOutput.Text = BuildOutputName(txtOutput.Text);
        }

        private string BuildOutputName(string file)
        {
            string tmp = "";
            string tmpOutput = "";
            if (!String.IsNullOrEmpty(file))
                tmpOutput = file;
            else
                tmpOutput = txtSourceFile.Text;

            tmp = Path.ChangeExtension(tmpOutput.Trim(), cmbOutput.Text);
            tmp = tmp.Replace("vpc", "vhd");
            return tmp;
        }

        private static void myProcess_HasExited(object sender, System.EventArgs e)
        {
            MessageBox.Show("Done.");
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            GetPhysicalDriveList();
        }

        private void btnStartPhysical_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(txtOutputPhysical.Text.Trim()))
            {
                MessageBox.Show("Please browse output folder.");
                txtOutputPhysical.Focus();
                return;
            }

            if (File.Exists(qemuimgpath))
            {
                if (cmbOutput.Text != "vpc" && cmbOutput.Text != "vhdx")
                {
                    cbDynamic.Checked = false;
                }

                var dynamic = "";
                if (cbDynamic.Checked)
                    dynamic += "-o subformat=dynamic";

                var command = String.Format("convert -f raw {0} -O {1} {2} {3}",
                    cmbSourcePhysical.Text.Split('|')[0].Trim(), cmbOutput.Text, dynamic, "\"" + txtOutputPhysical.Text.Trim() + "\"");

                var process = new System.Diagnostics.Process();
                process.StartInfo.FileName = qemuimgpath;
                process.StartInfo.Arguments = command;
                process.EnableRaisingEvents = true;
                process.Exited += new EventHandler(myProcess_HasExited);
                process.Start();
            }
            else
            {
                MessageBox.Show("qemu-img.exe doesn't exist.");
            }
        }
    }
}
