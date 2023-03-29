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

namespace UoFiddler.Controls.Forms
{
    public partial class TileDataFilterForm : Form
    {
        private readonly Action<ItemData> _applyItemFilterAction;
        private readonly Action<LandData> _applyLandFilterAction;

        public TileDataFilterForm(Action<ItemData> applyItemFilterAction, Action<LandData> applyLandFilterAction)
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();

            _applyItemFilterAction = applyItemFilterAction;
            _applyLandFilterAction = applyLandFilterAction;

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
            if (short.TryParse(textBoxAnim.Text, out short animation))
            {
                item.Animation = animation;
            }

            if (byte.TryParse(textBoxWeight.Text, out byte weight))
            {
                item.Weight = weight;
            }

            if (byte.TryParse(textBoxQuality.Text, out byte quality))
            {
                item.Quality = quality;
            }

            if (byte.TryParse(textBoxQuantity.Text, out byte quantity))
            {
                item.Quantity = quantity;
            }

            if (byte.TryParse(textBoxHue.Text, out byte hue))
            {
                item.Hue = hue;
            }

            if (byte.TryParse(textBoxStackOff.Text, out byte stackingOffset))
            {
                item.StackingOffset = stackingOffset;
            }

            if (byte.TryParse(textBoxValue.Text, out byte value))
            {
                item.Value = value;
            }

            if (byte.TryParse(textBoxHeigth.Text, out byte height))
            {
                item.Height = height;
            }

            if (short.TryParse(textBoxUnk1.Text, out short miscData))
            {
                item.MiscData = miscData;
            }

            if (byte.TryParse(textBoxUnk2.Text, out byte unk2))
            {
                item.Unk2 = unk2;
            }

            if (byte.TryParse(textBoxUnk3.Text, out byte unk3))
            {
                item.Unk3 = unk3;
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

            _applyItemFilterAction(item);
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
            if (ushort.TryParse(textBoxTexID.Text, out ushort value))
            {
                land.TextureId = value;
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

            _applyLandFilterAction(land);
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
