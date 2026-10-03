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

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Classes;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Plugin;
using UoFiddler.Localization;

namespace UoFiddler.Forms
{
    public partial class OptionsForm : Form
    {
        private readonly Action _updateAllTileViewsAction;
        private readonly Action _updateMapTabAction;
        private readonly Action _updateItemsTabAction;
        private readonly Action _updateSoundTabAction;

        public OptionsForm(Action updateAllTileViewsAction,
            Action updateItemsTabAction,
            Action updateSoundTabAction,
            Action updateMapTabAction)
        {
            InitializeComponent();

            radioExportFilenameHex.Checked = AppSettings.ExportFilenameInHex;
            radioExportFilenameDec.Checked = !AppSettings.ExportFilenameInHex;
            checkBoxExportFilenameDecPad.Checked = AppSettings.ExportFilenameDecimalPadded;
            checkBoxExportFilenameDecPad.Enabled = !AppSettings.ExportFilenameInHex;

            Icon = Options.GetFiddlerIcon();

            _updateAllTileViewsAction = updateAllTileViewsAction;
            _updateItemsTabAction = updateItemsTabAction;
            _updateSoundTabAction = updateSoundTabAction;
            _updateMapTabAction = updateMapTabAction;

            TileFocusColorComboBox.MaxDropDownItems = 14;
            TileFocusColorComboBox.IntegralHeight = false;
            TileFocusColorComboBox.DrawMode = DrawMode.OwnerDrawFixed;
            TileFocusColorComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            TileFocusColorComboBox.DrawItem += TileFocusColorComboBoxDrawItem;

            TileFocusColorComboBox.DataSource = typeof(Color).GetProperties()
                .Where(x => x.PropertyType == typeof(Color))
                .Select(x => x.GetValue(null)).ToList();

            TileFocusColorComboBox.SelectedItem = Options.TileFocusColor;

            TileSelectionColorComboBox.MaxDropDownItems = 14;
            TileSelectionColorComboBox.IntegralHeight = false;
            TileSelectionColorComboBox.DrawMode = DrawMode.OwnerDrawFixed;
            TileSelectionColorComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            TileSelectionColorComboBox.DrawItem += TileSelectionColorComboBoxDrawItem;

            TileSelectionColorComboBox.DataSource = typeof(Color).GetProperties()
                .Where(x => x.PropertyType == typeof(Color))
                .Select(x => x.GetValue(null)).ToList();

            checkboxRemoveTileBorder.Checked = Options.RemoveTileBorder;

            TileSelectionColorComboBox.SelectedItem = Options.TileSelectionColor;

            PreviewBackgroundColorButton.BackColor = Options.PreviewBackgroundColor;

            checkBoxCacheData.Checked = Files.CacheData;
            checkBoxNewMapSize.Checked = Map.Felucca.Width == 7168;
            checkBoxuseDiff.Checked = Map.UseDiff;
            checkBoxPolSoundIdOffset.Checked = Options.PolSoundIdOffset;
            numericUpDownItemSizeWidth.Value = Options.ArtItemSizeWidth;
            numericUpDownItemSizeHeight.Value = Options.ArtItemSizeHeight;
            checkBoxItemClip.Checked = Options.ArtItemClip;
            map0Nametext.Text = Options.MapNames[0];
            map1Nametext.Text = Options.MapNames[1];
            map2Nametext.Text = Options.MapNames[2];
            map3Nametext.Text = Options.MapNames[3];
            map4Nametext.Text = Options.MapNames[4];
            map5Nametext.Text = Options.MapNames[5];
            cmdtext.Text = Options.MapCmd;
            argstext.Text = Options.MapArgs;
            textBoxOutputPath.Text = Options.OutputPath;

            foreach (ClientFileSaveFormat format in ClientFileSaveFormats.All)
            {
                comboBoxSaveFormat.Items.Add(ClientFileSaveFormats.DisplayName(format));
            }

            comboBoxSaveFormat.SelectedIndex = ClientFileSaveFormats.IndexOf(Options.SaveFormat);

            // 应用中文汉化
            LocalizationService.LocalizeForm(this);
            
            // 在 Load 事件中应用 ToolTip 汉化
            this.Load += (s, e) => ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            var getter = PluginBase.LocalizationGetter;
            if (getter == null) return;

            // ToolTips
            toolTip1.SetToolTip(numericUpDownItemSizeHeight, getter("Forms.OptionsForm.tooltip_numericUpDownItemSizeHeight") ?? "Height");
            toolTip1.SetToolTip(checkBoxItemClip, getter("Forms.OptionsForm.tooltip_checkBoxItemClip") ?? "ItemClip images in items tab shrinked or clipped");
            toolTip1.SetToolTip(label1, getter("Forms.OptionsForm.tooltip_label1") ?? "ItemSize controls the size of images in items tab");
            toolTip1.SetToolTip(numericUpDownItemSizeWidth, getter("Forms.OptionsForm.tooltip_numericUpDownItemSizeWidth") ?? "Width");
            toolTip1.SetToolTip(checkBoxCacheData, getter("Forms.OptionsForm.tooltip_checkBoxCacheData") ?? "CacheData should mul entries be cached for faster load");
            toolTip1.SetToolTip(checkBoxNewMapSize, getter("Forms.OptionsForm.tooltip_checkBoxNewMapSize") ?? "NewMapSize Felucca/Trammel width 7168?");
            toolTip1.SetToolTip(checkBoxuseDiff, getter("Forms.OptionsForm.tooltip_checkBoxuseDiff") ?? "Should map diff files be used");
            toolTip1.SetToolTip(checkBoxPolSoundIdOffset, getter("Forms.OptionsForm.tooltip_checkBoxPolSoundIdOffset") ?? "UO Sounds are indexed from 0 but POL uses +1 offset.\r\nWhen this option is checked Sounds tab will display sound indexes starting from 1 instead of 0.\r\nThis option also affects the export sound list.");
            toolTip1.SetToolTip(label2, getter("Forms.OptionsForm.tooltip_label2") ?? "Defines the map name");
            toolTip1.SetToolTip(label3, getter("Forms.OptionsForm.tooltip_label3") ?? "Defines the map name");
            toolTip1.SetToolTip(label4, getter("Forms.OptionsForm.tooltip_label4") ?? "Defines the map name");
            toolTip1.SetToolTip(label5, getter("Forms.OptionsForm.tooltip_label5") ?? "Defines the map name");
            toolTip1.SetToolTip(label6, getter("Forms.OptionsForm.tooltip_label6") ?? "Defines the map name");
            toolTip1.SetToolTip(label7, getter("Forms.OptionsForm.tooltip_label7") ?? "Defines the cmd to send Client to loc");
            toolTip1.SetToolTip(label8, getter("Forms.OptionsForm.tooltip_label8") ?? "{1} = x, {2} = y, {3} = z, {4} = mapid, {5} = mapname");
            toolTip1.SetToolTip(label9, getter("Forms.OptionsForm.tooltip_label9") ?? "Defines the map name");
            toolTip1.SetToolTip(FocusColorLabel, getter("Forms.OptionsForm.tooltip_FocusColorLabel") ?? "ItemSize controls the size of images in items tab");
            toolTip1.SetToolTip(SelectedColorLabel, getter("Forms.OptionsForm.tooltip_SelectedColorLabel") ?? "ItemSize controls the size of images in items tab");
            toolTip1.SetToolTip(radioExportFilenameHex, getter("Forms.OptionsForm.tooltip_radioExportFilenameHex") ?? "Exported filenames embed the ID in hexadecimal form.");
            toolTip1.SetToolTip(radioExportFilenameDec, getter("Forms.OptionsForm.tooltip_radioExportFilenameDec") ?? "Exported filenames embed the ID in decimal form.");
            toolTip1.SetToolTip(checkBoxExportFilenameDecPad, getter("Forms.OptionsForm.tooltip_checkBoxExportFilenameDecPad") ?? "When using decimal format, pad the ID with leading zeros so files sort correctly.");
            toolTip1.SetToolTip(comboBoxSaveFormat, getter("Forms.OptionsForm.tooltip_comboBoxSaveFormat") ?? "Which container a save writes for the files the client ships as either. Affects art, gumpart, sound, multis and maps; everything else has only one format.");
        }

        private void OnClickApply(object sender, EventArgs e)
        {
            if (checkBoxPolSoundIdOffset.Checked != Options.PolSoundIdOffset)
            {
                Options.PolSoundIdOffset = checkBoxPolSoundIdOffset.Checked;

                _updateSoundTabAction();
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

                _updateMapTabAction();
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

                _updateItemsTabAction();
            }

            if (checkBoxItemClip.Checked != Options.ArtItemClip)
            {
                Options.ArtItemClip = checkBoxItemClip.Checked;

                _updateItemsTabAction();
            }

            if ((Color)TileFocusColorComboBox.SelectedItem != Options.TileFocusColor)
            {
                Options.TileFocusColor = (Color)TileFocusColorComboBox.SelectedItem;

                _updateAllTileViewsAction();
            }

            if ((Color)TileSelectionColorComboBox.SelectedItem != Options.TileSelectionColor)
            {
                Options.TileSelectionColor = (Color)TileSelectionColorComboBox.SelectedItem;

                _updateAllTileViewsAction();
            }

            if (checkboxRemoveTileBorder.Checked != Options.RemoveTileBorder)
            {
                Options.RemoveTileBorder = checkboxRemoveTileBorder.Checked;

                _updateAllTileViewsAction();
            }

            if (PreviewBackgroundColorButton.BackColor != Options.PreviewBackgroundColor)
            {
                Options.PreviewBackgroundColor = PreviewBackgroundColorButton.BackColor;
                ControlEvents.FirePreviewBackgroundColorChangeEvent();
            }

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

            if (comboBoxSaveFormat.SelectedIndex >= 0)
            {
                Options.SaveFormat = ClientFileSaveFormats.All[comboBoxSaveFormat.SelectedIndex];
            }

            bool newHex = radioExportFilenameHex.Checked;
            bool newPad = checkBoxExportFilenameDecPad.Checked;
            if (newHex != AppSettings.ExportFilenameInHex || newPad != AppSettings.ExportFilenameDecimalPadded)
            {
                AppSettings.ExportFilenameInHex = newHex;
                AppSettings.ExportFilenameDecimalPadded = newPad;
                Options.ExportFilenameInHex = newHex;
                Options.ExportFilenameDecimalPadded = newPad;
                AppSettings.Save();
            }
        }

        private void OnExportFilenameFormatChanged(object sender, EventArgs e)
        {
            checkBoxExportFilenameDecPad.Enabled = !radioExportFilenameHex.Checked;
        }

        private void OnClickBrowseOutputPath(object sender, EventArgs e)
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

        private void TileFocusColorComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();

            if (e.Index < 0)
            {
                return;
            }

            var itemText = TileFocusColorComboBox.GetItemText(TileFocusColorComboBox.Items[e.Index]);
            var color = (Color)TileFocusColorComboBox.Items[e.Index];

            var rectangle = new Rectangle(e.Bounds.Left + 1, e.Bounds.Top + 1, 2 * (e.Bounds.Height - 2), e.Bounds.Height - 2);
            var textRectangle = Rectangle.FromLTRB(rectangle.Right + 2, e.Bounds.Top, e.Bounds.Right, e.Bounds.Bottom);

            using (var b = new SolidBrush(color))
            {
                e.Graphics.FillRectangle(b, rectangle);
            }

            e.Graphics.DrawRectangle(Pens.Black, rectangle);

            TextRenderer.DrawText(e.Graphics, itemText, TileFocusColorComboBox.Font, textRectangle, TileFocusColorComboBox.ForeColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        }

        private void TileSelectionColorComboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();

            if (e.Index < 0)
            {
                return;
            }

            var itemText = TileSelectionColorComboBox.GetItemText(TileSelectionColorComboBox.Items[e.Index]);
            var color = (Color)TileSelectionColorComboBox.Items[e.Index];

            var rectangle = new Rectangle(e.Bounds.Left + 1, e.Bounds.Top + 1, 2 * (e.Bounds.Height - 2), e.Bounds.Height - 2);
            var textRectangle = Rectangle.FromLTRB(rectangle.Right + 2, e.Bounds.Top, e.Bounds.Right, e.Bounds.Bottom);

            using (var b = new SolidBrush(color))
            {
                e.Graphics.FillRectangle(b, rectangle);
            }

            e.Graphics.DrawRectangle(Pens.Black, rectangle);

            TextRenderer.DrawText(e.Graphics, itemText, TileSelectionColorComboBox.Font, textRectangle, TileSelectionColorComboBox.ForeColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        }

        private void RestoreDefaultsButton_Click(object sender, EventArgs e)
        {
            const string title = "Restore defaults";
            const string message = "Do you want to reset tile views settings to default?";

            if (MessageBox.Show(message, title, MessageBoxButtons.YesNo) == DialogResult.No)
            {
                return;
            }

            checkboxRemoveTileBorder.Checked = false;

            if (AppSettings.DarkMode)
            {
                TileFocusColorComboBox.SelectedItem = Color.Red;
                TileSelectionColorComboBox.SelectedItem = Color.MediumTurquoise;
                PreviewBackgroundColorButton.BackColor = Color.FromArgb(32, 32, 32);
            }
            else
            {
                TileFocusColorComboBox.SelectedItem = Color.DarkRed;
                TileSelectionColorComboBox.SelectedItem = Color.DodgerBlue;
                PreviewBackgroundColorButton.BackColor = Color.White;
            }
        }

        private void PreviewBackgroundColorButton_Click(object sender, EventArgs e)
        {
            using var dlg = new ColorDialog { Color = PreviewBackgroundColorButton.BackColor };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                PreviewBackgroundColorButton.BackColor = dlg.Color;
            }
        }

        private void OnClickClose(object sender, EventArgs e)
        {
            Close();
        }
    }
}
