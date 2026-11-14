/***************************************************************************
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

namespace UoFiddler.Plugin.MultiEditor.UI
{
    /// <summary>
    /// Asks what kind of multi to start. The client has two kinds of houses: custom houses, always
    /// built on one of its ready-made foundations (floors from Z 7), and multis built whole, which
    /// may stand on a foundation of their own or start from the ground.
    /// </summary>
    internal sealed partial class NewMultiDialog : Form
    {
        private sealed record FoundationEntry(int Id, MultiComponentList Components)
        {
            public override string ToString() => $"{Components.Width} x {Components.Height}   (multi 0x{Id:X4})";
        }

        /// <summary>The floor of a custom house foundation.</summary>
        private const ushort _foundationDirt = 0x31F4;

        private static List<FoundationEntry> _foundations;

        public NewMultiDialog()
        {
            InitializeComponent();

            foreach (FoundationEntry entry in Foundations())
            {
                _foundationCombo.Items.Add(entry);
            }

            if (_foundationCombo.Items.Count > 0)
            {
                _foundationCombo.SelectedIndex = 0;
                _custom.Checked = true;
            }
            else
            {
                // An old client without custom housing.
                _custom.Enabled = false;
                _foundationCombo.Enabled = false;
                _blankFoundation.Checked = true;
            }

            SyncEnabled();
        }

        /// <summary>The chosen client foundation, or null for a blank multi.</summary>
        public MultiComponentList Foundation => _custom.Checked ? (_foundationCombo.SelectedItem as FoundationEntry)?.Components : null;

        public int MultiWidth => (int)_width.Value;

        public int MultiHeight => (int)_height.Value;

        public int FirstFloorZ => _blankGround.Checked ? 0 : 7;

        private void Kind_CheckedChanged(object sender, EventArgs e)
        {
            SyncEnabled();
        }

        private void SyncEnabled()
        {
            _foundationCombo.Enabled = _custom.Checked;
            _width.Enabled = _height.Enabled = !_custom.Checked;
        }

        /// <summary>Forgets the foundation list, after the client files changed.</summary>
        public static void Reset()
        {
            _foundations = null;
        }

        /// <summary>
        /// The client's custom house foundations: multis that are nothing but a low wall around a
        /// dirt floor at Z 7. They are recognised by that shape, not by an id range.
        /// </summary>
        private static List<FoundationEntry> Foundations()
        {
            if (_foundations != null)
            {
                return _foundations;
            }

            var found = new List<FoundationEntry>();
            for (int id = 0; id < Multis.MaximumMultiIndex; id++)
            {
                MultiComponentList list = Multis.GetComponents(id);
                if (list != null && list != MultiComponentList.Empty && IsFoundation(list))
                {
                    found.Add(new FoundationEntry(id, list));
                }
            }

            found.Sort((a, b) => a.Components.Width != b.Components.Width
                ? a.Components.Width.CompareTo(b.Components.Width)
                : a.Components.Height.CompareTo(b.Components.Height));
            _foundations = found;
            return found;
        }

        private static bool IsFoundation(MultiComponentList list)
        {
            int dirt = 0;
            for (int x = 0; x < list.Width; x++)
            {
                for (int y = 0; y < list.Height; y++)
                {
                    foreach (MTile tile in list.Tiles[x][y])
                    {
                        if (tile.Z > 7)
                        {
                            return false;
                        }

                        if (tile.Id == _foundationDirt && tile.Z == 7)
                        {
                            ++dirt;
                        }
                    }
                }
            }

            return dirt > 0;
        }
    }
}
