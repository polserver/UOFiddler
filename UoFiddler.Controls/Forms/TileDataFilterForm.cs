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
using System.Collections.Generic;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.Forms
{
    public partial class TileDataFilterForm : Form
    {
        private readonly Action<ItemData> _applyItemFilterAction;
        private readonly Action<LandData> _applyLandFilterAction;
        private Func<string, string?>? _localizationGetter;

        public TileDataFilterForm(Action<ItemData> applyItemFilterAction, Action<LandData> applyLandFilterAction)
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();

            _applyItemFilterAction = applyItemFilterAction;
            _applyLandFilterAction = applyLandFilterAction;

            InitFlagCheckBoxes();
        }

        /// <summary>
        /// Sets the localization getter and applies localized text.
        /// </summary>
        public void SetLocalization(Func<string, string?>? getLocalized)
        {
            _localizationGetter = getLocalized;
            ApplyLocalization();
        }

        /// <summary>
        /// Applies localized text to UI elements.
        /// </summary>
        private void ApplyLocalization()
        {
            if (_localizationGetter == null) return;

            // Localize title
            var title = _localizationGetter("Forms.TileDataFilterForm.Title");
            if (title != null)
                Text = title;

            // Localize tab pages - look for TabPage controls named tabPageItems and tabPageLand
            foreach (TabPage page in tabcontrol.TabPages)
            {
                if (page.Name == "tabPageItems")
                {
                    var itemsTab = _localizationGetter("Forms.TileDataFilterForm.ItemsTab");
                    if (itemsTab != null)
                        page.Text = itemsTab;
                }
                else if (page.Name == "tabPageLand")
                {
                    var landTab = _localizationGetter("Forms.TileDataFilterForm.LandTab");
                    if (landTab != null)
                        page.Text = landTab;
                }
            }

            // Map textbox names to their corresponding label names and JSON keys
            // The label names are defined in Designer.cs (e.g., label1, label2, etc.)
            var textboxToLabelName = new Dictionary<string, (string labelName, string jsonKey)>
            {
                { "textBoxName", ("label1", "Forms.TileDataFilterForm.Labels.Name") },
                { "textBoxAnim", ("label2", "Forms.TileDataFilterForm.Labels.Anim") },
                { "textBoxWeight", ("label3", "Forms.TileDataFilterForm.Labels.Weight") },
                { "textBoxQuality", ("label4", "Forms.TileDataFilterForm.Labels.Quality") },
                { "textBoxQuantity", ("label5", "Forms.TileDataFilterForm.Labels.Quantity") },
                { "textBoxHue", ("label8", "Forms.TileDataFilterForm.Labels.Hue") },
                { "textBoxStackOff", ("label7", "Forms.TileDataFilterForm.Labels.StackOff") },
                { "textBoxValue", ("label6", "Forms.TileDataFilterForm.Labels.Value") },
                { "textBoxHeigth", ("label11", "Forms.TileDataFilterForm.Labels.Height") },  // label11.Text = "Heigth" (typo in Designer)
                { "textBoxUnk1", ("label10", "Forms.TileDataFilterForm.Labels.MiscData") },
                { "textBoxUnk2", ("label9", "Forms.TileDataFilterForm.Labels.Unk2") },
                { "textBoxUnk3", ("label12", "Forms.TileDataFilterForm.Labels.Unk3") },
                { "textBoxNameLand", ("label23", "Forms.TileDataFilterForm.Labels.NameLand") },
                { "textBoxTexID", ("label24", "Forms.TileDataFilterForm.Labels.TexID") }
            };

            // Create a map of label names to controls for quick lookup
            var labelMap = new Dictionary<string, Control>();
            foreach (Control control in GetAllControls(this))
            {
                if (control is Label && !string.IsNullOrEmpty(control.Name))
                {
                    labelMap[control.Name] = control;
                }
            }

            // Now apply localizations to each label based on its mapped textbox
            foreach (var textbox in textboxToLabelName)
            {
                if (labelMap.TryGetValue(textbox.Value.labelName, out Control? labelControl))
                {
                    if (labelControl is Label label)
                    {
                        var localized = _localizationGetter(textbox.Value.jsonKey);
                        if (localized != null)
                            label.Text = localized;
                    }
                }
            }

            // Localize buttons
            foreach (Control control in GetAllControls(this))
            {
                if (control is Button button)
                {
                    if (button.Text == "Apply Filter" || button.Text == "应用过滤器")
                    {
                        var applyButton = _localizationGetter("Forms.TileDataFilterForm.ApplyButton");
                        if (applyButton != null)
                            button.Text = applyButton;
                    }
                    else if (button.Text == "Reset Filter" || button.Text == "重置过滤器")
                    {
                        var resetButton = _localizationGetter("Forms.TileDataFilterForm.ResetButton");
                        if (resetButton != null)
                            button.Text = resetButton;
                    }
                }
            }

            // Re-initialize flag checkboxes with localized names
            InitFlagCheckBoxes();
        }

        /// <summary>
        /// Helper method to recursively get all controls.
        /// </summary>
        private IEnumerable<Control> GetAllControls(Control container)
        {
            foreach (Control control in container.Controls)
            {
                yield return control;
                foreach (Control child in GetAllControls(control))
                    yield return child;
            }
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
                string flagName = enumNames[i];
                // 获取汉化文本，如果没有就用原始英文
                string displayName = _localizationGetter?.Invoke($"Forms.TileDataControl.TileFlags.{flagName}") ?? flagName;
                checkedListBox1.Items.Add(displayName, false);
            }
            checkedListBox1.EndUpdate();

            // land
            checkedListBox2.BeginUpdate();
            checkedListBox2.Items.Clear();
            for (int i = 1; i < maxLength; ++i)
            {
                string flagName = enumNames[i];
                // 获取汉化文本，如果没有就用原始英文
                string displayName = _localizationGetter?.Invoke($"Forms.TileDataControl.TileFlags.{flagName}") ?? flagName;
                checkedListBox2.Items.Add(displayName, false);
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
