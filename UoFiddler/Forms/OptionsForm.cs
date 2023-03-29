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
using UoFiddler.Controls.Classes;

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

            checkBoxOverrideBackgroundColorFromTile.Checked = Options.OverrideBackgroundColorFromTile;
            checkboxRemoveTileBorder.Checked = Options.RemoveTileBorder;

            TileSelectionColorComboBox.SelectedItem = Options.TileSelectionColor;

            checkBoxCacheData.Checked = Files.CacheData;
            checkBoxNewMapSize.Checked = Map.Felucca.Width == 7168;
            checkBoxuseDiff.Checked = Map.UseDiff;
            checkBoxPanelSoundsDesign.Checked = Options.RightPanelInSoundsTab;
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
        }

        private void OnClickApply(object sender, EventArgs e)
        {
            Options.RightPanelInSoundsTab = checkBoxPanelSoundsDesign.Checked;

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

            if (checkBoxOverrideBackgroundColorFromTile.Checked != Options.OverrideBackgroundColorFromTile)
            {
                Options.OverrideBackgroundColorFromTile = checkBoxOverrideBackgroundColorFromTile.Checked;

                _updateAllTileViewsAction();
            }

            if (checkboxRemoveTileBorder.Checked != Options.RemoveTileBorder)
            {
                Options.RemoveTileBorder = checkboxRemoveTileBorder.Checked;

                _updateAllTileViewsAction();
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

            checkBoxOverrideBackgroundColorFromTile.Checked = false;
            checkboxRemoveTileBorder.Checked = false;

            TileFocusColorComboBox.SelectedItem = Color.DarkRed;
            TileSelectionColorComboBox.SelectedItem = Color.DodgerBlue;
        }

        private void OnClickClose(object sender, EventArgs e)
        {
            Close();
        }
    }
}
