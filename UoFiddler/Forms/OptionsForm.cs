/***************************************************************************
 *
 * $Author: Turley
 * 
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with 
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System.IO;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Plugin;

namespace UoFiddler.Forms
{
    public partial class OptionsForm : Form
    {
        public OptionsForm()
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();

            checkBoxAltDesign.Checked = Options.DesignAlternative;
            checkBoxCacheData.Checked = Files.CacheData;
            checkBoxNewMapSize.Checked = Map.Felucca.Width == 7168;
            checkBoxuseDiff.Checked = Map.UseDiff;
            numericUpDownItemSizeWidth.Value = Options.ArtItemSizeWidth;
            numericUpDownItemSizeHeight.Value = Options.ArtItemSizeHeight;
            checkBoxItemClip.Checked = Options.ArtItemClip;
            checkBoxUseHash.Checked = Files.UseHashFile;
            map0Nametext.Text = Options.MapNames[0];
            map1Nametext.Text = Options.MapNames[1];
            map2Nametext.Text = Options.MapNames[2];
            map3Nametext.Text = Options.MapNames[3];
            map4Nametext.Text = Options.MapNames[4];
            map5Nametext.Text = Options.MapNames[5];
            cmdtext.Text = Options.MapCmd;
            argstext.Text = Options.MapArgs;
            textBoxOutputPath.Text = Options.OutputPath;
        }

        private void OnClickApply(object sender, System.EventArgs e)
        {
            if (checkBoxAltDesign.Checked != Options.DesignAlternative)
            {
                Options.DesignAlternative = checkBoxAltDesign.Checked;
                MainForm.ChangeDesign();
                PluginEvents.FireDesignChangeEvent();
            }

            Files.CacheData = checkBoxCacheData.Checked;

            if (checkBoxNewMapSize.Checked != (Map.Felucca.Width == 7168))
            {
                if (checkBoxNewMapSize.Checked)
                {
                    Map.Felucca.Width = 7168;
                    Map.Trammel.Width = 7168;
                }
                else
                {
                    Map.Felucca.Width = 6144;
                    Map.Trammel.Width = 6144;
                }
                MainForm.ChangeMapSize();
            }

            if (checkBoxuseDiff.Checked != Map.UseDiff)
            {
                Map.UseDiff = checkBoxuseDiff.Checked;
                ControlEvents.FireMapDiffChangeEvent();
            }

            if (numericUpDownItemSizeWidth.Value != Options.ArtItemSizeWidth
                || numericUpDownItemSizeHeight.Value != Options.ArtItemSizeHeight)
            {
                Options.ArtItemSizeWidth = (int)numericUpDownItemSizeWidth.Value;
                Options.ArtItemSizeHeight = (int)numericUpDownItemSizeHeight.Value;
                MainForm.ReloadItemTab();
            }

            if (checkBoxItemClip.Checked != Options.ArtItemClip)
            {
                Options.ArtItemClip = checkBoxItemClip.Checked;
                MainForm.ReloadItemTab();
            }

            Files.UseHashFile = checkBoxUseHash.Checked;

            if (map0Nametext.Text != Options.MapNames[0]
                || map1Nametext.Text != Options.MapNames[1]
                || map2Nametext.Text != Options.MapNames[2]
                || map3Nametext.Text != Options.MapNames[3]
                || map4Nametext.Text != Options.MapNames[4]
                || map5Nametext.Text != Options.MapNames[5])
            {
                Options.MapNames[0] = map0Nametext.Text;
                Options.MapNames[1] = map1Nametext.Text;
                Options.MapNames[2] = map2Nametext.Text;
                Options.MapNames[3] = map3Nametext.Text;
                Options.MapNames[4] = map4Nametext.Text;
                Options.MapNames[5] = map5Nametext.Text;
                ControlEvents.FireMapNameChangeEvent();
            }

            Options.MapCmd = cmdtext.Text;
            Options.MapArgs = argstext.Text;

            if (Directory.Exists(textBoxOutputPath.Text))
            {
                Options.OutputPath = textBoxOutputPath.Text;
            }
        }

        private void OnClickBrowseOutputPath(object sender, System.EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory";
                dialog.ShowNewFolderButton = true;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    textBoxOutputPath.Text = dialog.SelectedPath;
                }
            }
        }
    }
}
