using CWE_MapPatcher.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CWE_MapPatcher
{
    public partial class MainWindow : Form
    {
        List<ComboBox> cbxList;

        public MainWindow()
        {
            InitializeComponent();
            FontFix();

            lblVersion.Text = string.Format("v{0}", System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString(2));

            cbxList = new List<ComboBox>() 
            {
                cbxPlayer1, cbxPlayer2, cbxPlayer3, cbxPlayer4, cbxPlayer5, cbxPlayer6, cbxPlayer7, cbxPlayer8
            };

            foreach (var cbx in cbxList)
                cbx.SelectedIndex = 0;
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            openMapFileDialog.ShowDialog();
            tbxMapPath.Text = openMapFileDialog.FileName;

            Properties.Settings.Default.MapsDir = new FileInfo(openMapFileDialog.FileName).DirectoryName;
            Properties.Settings.Default.Save();
        }

        private void btnPatch_Click(object sender, EventArgs e)
        {
            try
            {
                if (!File.Exists(tbxMapPath.Text))
                {
                    MessageBox.Show("File doesn't exist!");
                    return;
                }

                var patcher = new PatchManager();

                if (!patcher.Prepare(tbxMapPath.Text))
                {
                    MessageBox.Show("This map already CWE-patched. Use original map instead");
                    return;
                }

                if (cbNoClans.Checked)
                    patcher.Patch();
                else
                {
                    var clans = Clans.CreateDefaultPlayersClans();

                    for (int i = 0; i < cbxList.Count; i++)
                    {
                        int cbxIndex = cbxList[i].SelectedIndex;
                        if (cbxIndex == 0)
                            clans[i + 1] = Clans.NonSet;
                        else if (cbxIndex == 1)
                            clans[i + 1] = Clans.Clan_1;
                        else if (cbxIndex == 2)
                            clans[i + 1] = Clans.Clan_2;
                    }

                    patcher.Patch(clans);
                }

                MessageBox.Show("Map patched successfully!");
            }

            catch(Exception ex)
            {
                MessageBox.Show("There is an error. Please contact the developer, use Ctrl+C to copy this message\nERROR: " + ex.Message, 
                    "Error occured", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cbNoClans_CheckedChanged(object sender, EventArgs e)
        {
            foreach (var cbx in cbxList)
                cbx.Enabled = !cbNoClans.Checked;
        }

        private void FontFix()
        {
            string filename = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "heroes5-cwe-fonts.ttf");

            File.WriteAllBytes(filename, Resources.MTCORSVA);
            PrivateFontCollection pfc = new PrivateFontCollection();
            pfc.AddFontFile(filename);

            lblVersion.Font = new Font(pfc.Families[0], 10);
            lblPlayer1.Font = new Font(pfc.Families[0], 13);
            lblPlayer2.Font = new Font(pfc.Families[0], 13);
            lblPlayer3.Font = new Font(pfc.Families[0], 13);
            lblPlayer4.Font = new Font(pfc.Families[0], 13);
            lblPlayer5.Font = new Font(pfc.Families[0], 13);
            lblPlayer6.Font = new Font(pfc.Families[0], 13);
            lblPlayer7.Font = new Font(pfc.Families[0], 13);
            lblPlayer8.Font = new Font(pfc.Families[0], 13);
        }
    }
}
