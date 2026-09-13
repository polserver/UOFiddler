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
    /// <summary>
    /// Picks which parts of a copied tiledata entry get pasted onto the selected
    /// entries. Produces an <see cref="ItemDataEdit"/> / <see cref="LandDataEdit"/>,
    /// so the paste lands through the same apply path as a multi-selection
    /// "Save Changes".
    /// </summary>
    public partial class TileDataPasteSpecialForm : Form
    {
        private enum FlagMode
        {
            Leave,
            Replace,
            SetChecked,
            ClearChecked
        }

        private enum ItemField
        {
            Name,
            Animation,
            Weight,
            Quality,
            Quantity,
            Hue,
            StackingOffset,
            Value,
            Height,
            MiscData,
            Unk2,
            Unk3
        }

        private enum LandField
        {
            Name,
            TextureId
        }

        // Remembered for the session so pasting the same subset onto batch after batch
        // does not mean re-ticking the same boxes every time.
        private static readonly HashSet<int> _rememberedItemFields = new HashSet<int>();
        private static readonly HashSet<int> _rememberedLandFields = new HashSet<int>();
        private static bool _hasRememberedItemFields;
        private static bool _hasRememberedLandFields;
        private static FlagMode _rememberedFlagMode = FlagMode.Replace;

        private readonly bool _land;
        private readonly ItemData _sourceItem;
        private readonly LandData _sourceLand;
        private readonly List<int> _fieldKeys = new List<int>();
        private readonly List<TileFlag> _flagValues = new List<TileFlag>();

        public TileDataPasteSpecialForm(ItemData source, int sourceGraphic, int targetCount)
            : this(false, sourceGraphic, targetCount)
        {
            _sourceItem = source;
            BuildItemFields();
            BuildFlagList(source.Flags);
            RestoreRemembered(_rememberedItemFields, _hasRememberedItemFields);
        }

        public TileDataPasteSpecialForm(LandData source, int sourceGraphic, int targetCount)
            : this(true, sourceGraphic, targetCount)
        {
            _sourceLand = source;
            BuildLandFields();
            BuildFlagList(source.Flags);
            RestoreRemembered(_rememberedLandFields, _hasRememberedLandFields);
        }

        private TileDataPasteSpecialForm(bool land, int sourceGraphic, int targetCount)
        {
            InitializeComponent();

            _land = land;

            string sourceName = land
                ? TileData.LandTable[sourceGraphic].Name
                : TileData.ItemTable[sourceGraphic].Name;

            headerLabel.Text = targetCount == 1
                ? $"Paste from 0x{sourceGraphic:X4} ({sourceGraphic}) \"{sourceName}\" onto 1 selected entry."
                : $"Paste from 0x{sourceGraphic:X4} ({sourceGraphic}) \"{sourceName}\" onto {targetCount} selected entries.";

            switch (_rememberedFlagMode)
            {
                case FlagMode.Leave:
                    flagsLeaveRadioButton.Checked = true;
                    break;

                case FlagMode.SetChecked:
                    flagsSetCheckedRadioButton.Checked = true;
                    break;

                case FlagMode.ClearChecked:
                    flagsClearCheckedRadioButton.Checked = true;
                    break;

                default:
                    flagsReplaceRadioButton.Checked = true;
                    break;
            }

            UpdateFlagListEnabled();
        }

        private void BuildItemFields()
        {
            AddField((int)ItemField.Name, "Name", _sourceItem.Name);
            AddField((int)ItemField.Animation, "Anim", _sourceItem.Animation.ToString());
            AddField((int)ItemField.Weight, "Weight", _sourceItem.Weight.ToString());
            AddField((int)ItemField.Quality, "Layer", _sourceItem.Quality.ToString());
            AddField((int)ItemField.Quantity, "Quantity", _sourceItem.Quantity.ToString());
            AddField((int)ItemField.Hue, "Hue", _sourceItem.Hue.ToString());
            AddField((int)ItemField.StackingOffset, "StackOff", _sourceItem.StackingOffset.ToString());
            AddField((int)ItemField.Value, "Value", _sourceItem.Value.ToString());
            AddField((int)ItemField.Height, "Height", _sourceItem.Height.ToString());
            AddField((int)ItemField.MiscData, "MiscData", _sourceItem.MiscData.ToString());
            AddField((int)ItemField.Unk2, "Unk2", _sourceItem.Unk2.ToString());
            AddField((int)ItemField.Unk3, "Unk3", _sourceItem.Unk3.ToString());
        }

        private void BuildLandFields()
        {
            AddField((int)LandField.Name, "Name", _sourceLand.Name);
            AddField((int)LandField.TextureId, "TexID", _sourceLand.TextureId.ToString());
        }

        private void AddField(int key, string label, string value)
        {
            _fieldKeys.Add(key);
            fieldsCheckedListBox.Items.Add($"{label} = {value}", true);
        }

        private void BuildFlagList(TileFlag sourceFlags)
        {
            string[] enumNames = Enum.GetNames(typeof(TileFlag));
            Array enumValues = Enum.GetValues(typeof(TileFlag));

            // Same "which half of the enum is valid for this client" rule the tiledata
            // editor and the CSV export use.
            int maxLength = Art.IsUOAHS() ? enumNames.Length : (enumNames.Length / 2) + 1;

            flagsCheckedListBox.BeginUpdate();
            try
            {
                for (int i = 1; i < maxLength; ++i)
                {
                    var flag = (TileFlag)enumValues.GetValue(i);
                    _flagValues.Add(flag);
                    flagsCheckedListBox.Items.Add(enumNames[i], (sourceFlags & flag) != 0);
                }
            }
            finally
            {
                flagsCheckedListBox.EndUpdate();
            }
        }

        private void RestoreRemembered(HashSet<int> remembered, bool hasRemembered)
        {
            if (!hasRemembered)
            {
                return;
            }

            for (int i = 0; i < _fieldKeys.Count; ++i)
            {
                fieldsCheckedListBox.SetItemChecked(i, remembered.Contains(_fieldKeys[i]));
            }
        }

        private FlagMode SelectedFlagMode
        {
            get
            {
                if (flagsLeaveRadioButton.Checked)
                {
                    return FlagMode.Leave;
                }

                if (flagsSetCheckedRadioButton.Checked)
                {
                    return FlagMode.SetChecked;
                }

                if (flagsClearCheckedRadioButton.Checked)
                {
                    return FlagMode.ClearChecked;
                }

                return FlagMode.Replace;
            }
        }

        /// <summary>The item edit the chosen boxes describe. Only valid for an item paste.</summary>
        public ItemDataEdit BuildItemEdit()
        {
            var edit = new ItemDataEdit();

            for (int i = 0; i < _fieldKeys.Count; ++i)
            {
                if (!fieldsCheckedListBox.GetItemChecked(i))
                {
                    continue;
                }

                switch ((ItemField)_fieldKeys[i])
                {
                    case ItemField.Name:
                        edit.Name = _sourceItem.Name ?? string.Empty;
                        break;

                    case ItemField.Animation:
                        edit.Animation = _sourceItem.Animation;
                        break;

                    case ItemField.Weight:
                        edit.Weight = _sourceItem.Weight;
                        break;

                    case ItemField.Quality:
                        edit.Quality = _sourceItem.Quality;
                        break;

                    case ItemField.Quantity:
                        edit.Quantity = _sourceItem.Quantity;
                        break;

                    case ItemField.Hue:
                        edit.Hue = _sourceItem.Hue;
                        break;

                    case ItemField.StackingOffset:
                        edit.StackingOffset = _sourceItem.StackingOffset;
                        break;

                    case ItemField.Value:
                        edit.Value = _sourceItem.Value;
                        break;

                    case ItemField.Height:
                        edit.Height = _sourceItem.Height;
                        break;

                    case ItemField.MiscData:
                        edit.MiscData = _sourceItem.MiscData;
                        break;

                    case ItemField.Unk2:
                        edit.Unk2 = _sourceItem.Unk2;
                        break;

                    case ItemField.Unk3:
                        edit.Unk3 = _sourceItem.Unk3;
                        break;
                }
            }

            ApplyFlagMode(_sourceItem.Flags, out TileFlag setFlags, out TileFlag clearFlags);
            edit.SetFlags = setFlags;
            edit.ClearFlags = clearFlags;

            return edit;
        }

        /// <summary>The land edit the chosen boxes describe. Only valid for a land paste.</summary>
        public LandDataEdit BuildLandEdit()
        {
            var edit = new LandDataEdit();

            for (int i = 0; i < _fieldKeys.Count; ++i)
            {
                if (!fieldsCheckedListBox.GetItemChecked(i))
                {
                    continue;
                }

                switch ((LandField)_fieldKeys[i])
                {
                    case LandField.Name:
                        edit.Name = _sourceLand.Name ?? string.Empty;
                        break;

                    case LandField.TextureId:
                        edit.TextureId = _sourceLand.TextureId;
                        break;
                }
            }

            ApplyFlagMode(_sourceLand.Flags, out TileFlag setFlags, out TileFlag clearFlags);
            edit.SetFlags = setFlags;
            edit.ClearFlags = clearFlags;

            return edit;
        }

        private void ApplyFlagMode(TileFlag sourceFlags, out TileFlag setFlags, out TileFlag clearFlags)
        {
            setFlags = TileFlag.None;
            clearFlags = TileFlag.None;

            switch (SelectedFlagMode)
            {
                case FlagMode.Leave:
                    return;

                case FlagMode.Replace:
                    // Set what the source has and clear every other flag this client
                    // knows about, so the target ends up with exactly the source flags.
                    foreach (TileFlag flag in _flagValues)
                    {
                        if ((sourceFlags & flag) != 0)
                        {
                            setFlags |= flag;
                        }
                        else
                        {
                            clearFlags |= flag;
                        }
                    }

                    return;

                case FlagMode.SetChecked:
                    setFlags = GetCheckedFlags();
                    return;

                case FlagMode.ClearChecked:
                    clearFlags = GetCheckedFlags();
                    return;
            }
        }

        private TileFlag GetCheckedFlags()
        {
            TileFlag flags = TileFlag.None;
            for (int i = 0; i < _flagValues.Count; ++i)
            {
                if (flagsCheckedListBox.GetItemChecked(i))
                {
                    flags |= _flagValues[i];
                }
            }

            return flags;
        }

        private void UpdateFlagListEnabled()
        {
            FlagMode mode = SelectedFlagMode;
            flagsCheckedListBox.Enabled = mode == FlagMode.SetChecked || mode == FlagMode.ClearChecked;
        }

        private void OnFlagModeChanged(object sender, EventArgs e)
        {
            UpdateFlagListEnabled();
        }

        private void OnClickCheckAll(object sender, EventArgs e)
        {
            SetAllFieldsChecked(true);
        }

        private void OnClickUncheckAll(object sender, EventArgs e)
        {
            SetAllFieldsChecked(false);
        }

        private void SetAllFieldsChecked(bool value)
        {
            for (int i = 0; i < fieldsCheckedListBox.Items.Count; ++i)
            {
                fieldsCheckedListBox.SetItemChecked(i, value);
            }
        }

        private void OnClickApply(object sender, EventArgs e)
        {
            Remember();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void Remember()
        {
            _rememberedFlagMode = SelectedFlagMode;

            HashSet<int> remembered = _land ? _rememberedLandFields : _rememberedItemFields;
            remembered.Clear();
            for (int i = 0; i < _fieldKeys.Count; ++i)
            {
                if (fieldsCheckedListBox.GetItemChecked(i))
                {
                    remembered.Add(_fieldKeys[i]);
                }
            }

            if (_land)
            {
                _hasRememberedLandFields = true;
            }
            else
            {
                _hasRememberedItemFields = true;
            }
        }
    }
}
