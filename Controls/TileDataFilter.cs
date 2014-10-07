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

namespace FiddlerControls
{
    public partial class TileDataFilter : Form
    {
        public TileDataFilter()
        {
            InitializeComponent();
            this.Icon = FiddlerControls.Options.GetFiddlerIcon();
            checkedListBox1.BeginUpdate();
            checkedListBox1.Items.Clear();
            string[] EnumNames = System.Enum.GetNames(typeof(TileFlag));
            for (int i = 1; i < EnumNames.Length; ++i)
            {
                checkedListBox1.Items.Add(EnumNames[i], false);
            }
            checkedListBox1.EndUpdate();
            checkedListBox2.BeginUpdate();
            checkedListBox2.Items.Clear();
            checkedListBox2.Items.Add(System.Enum.GetName(typeof(TileFlag), TileFlag.Damaging), false);
            checkedListBox2.Items.Add(System.Enum.GetName(typeof(TileFlag), TileFlag.Wet), false);
            checkedListBox2.Items.Add(System.Enum.GetName(typeof(TileFlag), TileFlag.Impassable), false);
            checkedListBox2.Items.Add(System.Enum.GetName(typeof(TileFlag), TileFlag.Wall), false);
            checkedListBox2.Items.Add(System.Enum.GetName(typeof(TileFlag), TileFlag.Unknown3), false);
            checkedListBox2.EndUpdate();
        }

        private void OnClickApplyFilterItem(object sender, EventArgs e)
        {
            ItemData item = new ItemData();
            string name = textBoxName.Text;
            if (name.Length > 20)
                name = name.Substring(0, 20);
            item.Name = name;
            byte byteres;
            short shortres;
            if (short.TryParse(textBoxAnim.Text, out shortres))
                item.Animation = shortres;
            if (byte.TryParse(textBoxWeight.Text, out byteres))
                item.Weight = byteres;
            if (byte.TryParse(textBoxQuality.Text, out byteres))
                item.Quality = byteres;
            if (byte.TryParse(textBoxQuantity.Text, out byteres))
                item.Quantity = byteres;
            if (byte.TryParse(textBoxHue.Text, out byteres))
                item.Hue = byteres;
            if (byte.TryParse(textBoxStackOff.Text, out byteres))
                item.StackingOffset = byteres;
            if (byte.TryParse(textBoxValue.Text, out byteres))
                item.Value = byteres;
            if (byte.TryParse(textBoxHeigth.Text, out byteres))
                item.Height = byteres;
            if (short.TryParse(textBoxUnk1.Text, out shortres))
                item.MiscData = shortres;
            if (byte.TryParse(textBoxUnk2.Text, out byteres))
                item.Unk2 = byteres;
            if (byte.TryParse(textBoxUnk3.Text, out byteres))
                item.Unk3 = byteres;
            item.Flags = TileFlag.None;
            Array EnumValues = System.Enum.GetValues(typeof(TileFlag));
            for (int i = 0; i < checkedListBox1.Items.Count; ++i)
            {
                if (checkedListBox1.GetItemChecked(i))
                    item.Flags |= (TileFlag)EnumValues.GetValue(i + 1);
            }
            TileDatas.ApplyFilterItem(item);
        }

        private void OnClickApplyFilterLand(object sender, EventArgs e)
        {
            LandData land = new LandData();
            string name = textBoxNameLand.Text;
            if (name.Length > 20)
                name = name.Substring(0, 20);
            land.Name = name;
            short shortres;
            if (short.TryParse(textBoxTexID.Text, out shortres))
                land.TextureID = shortres;
            land.Flags = TileFlag.None;
            if (checkedListBox2.GetItemChecked(0))
                land.Flags |= TileFlag.Damaging;
            if (checkedListBox2.GetItemChecked(1))
                land.Flags |= TileFlag.Wet;
            if (checkedListBox2.GetItemChecked(2))
                land.Flags |= TileFlag.Impassable;
            if (checkedListBox2.GetItemChecked(3))
                land.Flags |= TileFlag.Wall;
            if (checkedListBox2.GetItemChecked(4))
                land.Flags |= TileFlag.Unknown3;

            TileDatas.ApplyFilterLand(land);
        }

        private void OnClickResetFilterItem(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox1.Items.Count; ++i)
            {
                checkedListBox1.SetItemChecked(i, false);
            }
            textBoxAnim.Text = String.Empty;
            textBoxHeigth.Text = String.Empty;
            textBoxHue.Text = String.Empty;
            textBoxName.Text = String.Empty;
            textBoxQuality.Text = String.Empty;
            textBoxQuantity.Text = String.Empty;
            textBoxStackOff.Text = String.Empty;
            textBoxUnk1.Text = String.Empty;
            textBoxUnk2.Text = String.Empty;
            textBoxUnk3.Text = String.Empty;
            textBoxValue.Text = String.Empty;
            textBoxWeight.Text = String.Empty;
        }

        private void OnClickResetFilterLand(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox2.Items.Count; ++i)
            {
                checkedListBox2.SetItemChecked(i, false);
            }
            textBoxNameLand.Text = String.Empty;
            textBoxTexID.Text = String.Empty;
        }
    }
}
