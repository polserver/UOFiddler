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

using System.Windows.Forms;
using System.IO;
using Ultima;

namespace UoFiddler
{
    public partial class OptionsForm : Form
    {
        public OptionsForm()
        {
            InitializeComponent();
            this.Icon = FiddlerControls.Options.GetFiddlerIcon();

            checkBoxAltDesign.Checked = FiddlerControls.Options.DesignAlternative;
            checkBoxCacheData.Checked = Files.CacheData;
            checkBoxNewMapSize.Checked = (Map.Felucca.Width == 7168);
            checkBoxuseDiff.Checked = Map.UseDiff;
            numericUpDownItemSizeWidth.Value = FiddlerControls.Options.ArtItemSizeWidth;
            numericUpDownItemSizeHeight.Value = FiddlerControls.Options.ArtItemSizeHeight;
            checkBoxItemClip.Checked = FiddlerControls.Options.ArtItemClip;
            checkBoxUseHash.Checked = Files.UseHashFile;
            map0Nametext.Text = FiddlerControls.Options.MapNames[0];
            map1Nametext.Text = FiddlerControls.Options.MapNames[1];
            map2Nametext.Text = FiddlerControls.Options.MapNames[2];
            map3Nametext.Text = FiddlerControls.Options.MapNames[3];
            map4Nametext.Text = FiddlerControls.Options.MapNames[4];
            map5Nametext.Text = FiddlerControls.Options.MapNames[5];
            cmdtext.Text = FiddlerControls.Options.MapCmd;
            argstext.Text = FiddlerControls.Options.MapArgs;
            textBoxOutputPath.Text = FiddlerControls.Options.OutputPath;
        }

        private void OnClickApply(object sender, System.EventArgs e)
        {
            if (checkBoxAltDesign.Checked != FiddlerControls.Options.DesignAlternative)
            {
                FiddlerControls.Options.DesignAlternative = checkBoxAltDesign.Checked;
                UoFiddler.ChangeDesign();
                PluginInterface.Events.FireDesignChangeEvent();
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
                UoFiddler.ChangeMapSize();
            }
            if (checkBoxuseDiff.Checked != Map.UseDiff)
            {
                Map.UseDiff = checkBoxuseDiff.Checked;
                FiddlerControls.Events.FireMapDiffChangeEvent();
            }
            if ((numericUpDownItemSizeWidth.Value != FiddlerControls.Options.ArtItemSizeWidth)
                || (numericUpDownItemSizeHeight.Value != FiddlerControls.Options.ArtItemSizeHeight))
            {
                FiddlerControls.Options.ArtItemSizeWidth = (int)numericUpDownItemSizeWidth.Value;
                FiddlerControls.Options.ArtItemSizeHeight = (int)numericUpDownItemSizeHeight.Value;
                UoFiddler.ReloadItemTab();
            }
            if (checkBoxItemClip.Checked != FiddlerControls.Options.ArtItemClip)
            {
                FiddlerControls.Options.ArtItemClip = checkBoxItemClip.Checked;
                UoFiddler.ReloadItemTab();
            }
            Files.UseHashFile = checkBoxUseHash.Checked;

            if ((map0Nametext.Text != FiddlerControls.Options.MapNames[0])
                || (map1Nametext.Text != FiddlerControls.Options.MapNames[1])
                || (map2Nametext.Text != FiddlerControls.Options.MapNames[2])
                || (map3Nametext.Text != FiddlerControls.Options.MapNames[3])
                || (map4Nametext.Text != FiddlerControls.Options.MapNames[4])
                || (map5Nametext.Text != FiddlerControls.Options.MapNames[5]))
            {
                FiddlerControls.Options.MapNames[0] = map0Nametext.Text;
                FiddlerControls.Options.MapNames[1] = map1Nametext.Text;
                FiddlerControls.Options.MapNames[2] = map2Nametext.Text;
                FiddlerControls.Options.MapNames[3] = map3Nametext.Text;
                FiddlerControls.Options.MapNames[4] = map4Nametext.Text;
                FiddlerControls.Options.MapNames[5] = map5Nametext.Text;
                FiddlerControls.Events.FireMapNameChangeEvent();
            }
            FiddlerControls.Options.MapCmd = cmdtext.Text;
            FiddlerControls.Options.MapArgs = argstext.Text;
            if (Directory.Exists(textBoxOutputPath.Text))
                FiddlerControls.Options.OutputPath = textBoxOutputPath.Text;
        }

        private void onClickBrowseOutputPath(object sender, System.EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() == DialogResult.OK)
                    textBoxOutputPath.Text = dialog.SelectedPath;
            }
        }
    }
}
