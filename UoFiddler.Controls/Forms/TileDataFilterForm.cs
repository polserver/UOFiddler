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
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.UserControls;

namespace UoFiddler.Controls.Forms
{
    public partial class TileDataFilterForm : Form
    {
        public TileDataFilterForm()
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();

            InitFlagCheckBoxes();
        }

        private void InitFlagCheckBoxes()
        {
            string[] enumNames = Enum.GetNames(typeof(TileFlag));
            int maxLength = Art.IsUOAHS() ? enumNames.Length : (enumNames.Length / 2) + 1;

            // items
            checkedListBox1.BeginUpdate();
            checkedListBox1.Items.Clear();
            for (int i = 1; i < maxLength; ++i)
            {
                checkedListBox1.Items.Add(enumNames[i], false);
            }
            checkedListBox1.EndUpdate();

            // land
            checkedListBox2.BeginUpdate();
            checkedListBox2.Items.Clear();
            for (int i = 1; i < maxLength; ++i)
            {
                checkedListBox2.Items.Add(enumNames[i], false);
            }
            checkedListBox2.EndUpdate();
        }

        private void OnClickApplyFilterItem(object sender, EventArgs e)
        {
            ItemData item = new ItemData();
            string name = textBoxName.Text;
            if (name.Length > 20)
            {
                name = name.Substring(0, 20);
            }

            item.Name = name;
            if (short.TryParse(textBoxAnim.Text, out short shortres))
            {
                item.Animation = shortres;
            }

            if (byte.TryParse(textBoxWeight.Text, out byte byteres))
            {
                item.Weight = byteres;
            }

            if (byte.TryParse(textBoxQuality.Text, out byteres))
            {
                item.Quality = byteres;
            }

            if (byte.TryParse(textBoxQuantity.Text, out byteres))
            {
                item.Quantity = byteres;
            }

            if (byte.TryParse(textBoxHue.Text, out byteres))
            {
                item.Hue = byteres;
            }

            if (byte.TryParse(textBoxStackOff.Text, out byteres))
            {
                item.StackingOffset = byteres;
            }

            if (byte.TryParse(textBoxValue.Text, out byteres))
            {
                item.Value = byteres;
            }

            if (byte.TryParse(textBoxHeigth.Text, out byteres))
            {
                item.Height = byteres;
            }

            if (short.TryParse(textBoxUnk1.Text, out shortres))
            {
                item.MiscData = shortres;
            }

            if (byte.TryParse(textBoxUnk2.Text, out byteres))
            {
                item.Unk2 = byteres;
            }

            if (byte.TryParse(textBoxUnk3.Text, out byteres))
            {
                item.Unk3 = byteres;
            }

            item.Flags = TileFlag.None;
            Array enumValues = Enum.GetValues(typeof(TileFlag));
            for (int i = 0; i < checkedListBox1.Items.Count; ++i)
            {
                if (checkedListBox1.GetItemChecked(i))
                {
                    item.Flags |= (TileFlag)enumValues.GetValue(i + 1);
                }
            }
            TileDataControl.ApplyFilterItem(item);
        }

        private void OnClickApplyFilterLand(object sender, EventArgs e)
        {
            LandData land = new LandData();
            string name = textBoxNameLand.Text;
            if (name.Length > 20)
            {
                name = name.Substring(0, 20);
            }

            land.Name = name;
            if (ushort.TryParse(textBoxTexID.Text, out ushort shortres))
            {
                land.TextureID = shortres;
            }

            land.Flags = TileFlag.None;
            Array enumValues = Enum.GetValues(typeof(TileFlag));
            for (int i = 0; i < checkedListBox2.Items.Count; ++i)
            {
                if (checkedListBox2.GetItemChecked(i))
                {
                    land.Flags |= (TileFlag)enumValues.GetValue(i + 1);
                }
            }
            TileDataControl.ApplyFilterLand(land);
        }

        private void OnClickResetFilterItem(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox1.Items.Count; ++i)
            {
                checkedListBox1.SetItemChecked(i, false);
            }
            textBoxAnim.Text = string.Empty;
            textBoxHeigth.Text = string.Empty;
            textBoxHue.Text = string.Empty;
            textBoxName.Text = string.Empty;
            textBoxQuality.Text = string.Empty;
            textBoxQuantity.Text = string.Empty;
            textBoxStackOff.Text = string.Empty;
            textBoxUnk1.Text = string.Empty;
            textBoxUnk2.Text = string.Empty;
            textBoxUnk3.Text = string.Empty;
            textBoxValue.Text = string.Empty;
            textBoxWeight.Text = string.Empty;
        }

        private void OnClickResetFilterLand(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox2.Items.Count; ++i)
            {
                checkedListBox2.SetItemChecked(i, false);
            }
            textBoxNameLand.Text = string.Empty;
            textBoxTexID.Text = string.Empty;
        }
    }
}
