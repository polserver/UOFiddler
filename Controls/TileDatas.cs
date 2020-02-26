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
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Ultima;

namespace FiddlerControls
{
    public partial class TileDatas : UserControl
    {
        public TileDatas()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            InitializeComponent();
            checkedListBox1.BeginUpdate();
            checkedListBox1.Items.Clear();
            string[] enumNames = Enum.GetNames(typeof(TileFlag));
            for (int i = 1; i < enumNames.Length; ++i)
            {
                checkedListBox1.Items.Add(enumNames[i], false);
            }
            checkedListBox1.EndUpdate();
            checkedListBox2.BeginUpdate();
            checkedListBox2.Items.Clear();
            checkedListBox2.Items.Add(Enum.GetName(typeof(TileFlag), TileFlag.Damaging), false);
            checkedListBox2.Items.Add(Enum.GetName(typeof(TileFlag), TileFlag.Wet), false);
            checkedListBox2.Items.Add(Enum.GetName(typeof(TileFlag), TileFlag.Impassable), false);
            checkedListBox2.Items.Add(Enum.GetName(typeof(TileFlag), TileFlag.Wall), false);
            checkedListBox2.Items.Add(Enum.GetName(typeof(TileFlag), TileFlag.Unknown3), false);
            checkedListBox2.EndUpdate();
            _refMarker = this;

            treeViewItem.BeforeSelect += TreeViewItemOnBeforeSelect;
            saveDirectlyOnChangesToolStripMenuItem.Checked = Options.TileDataDirectlySaveOnChange;
            saveDirectlyOnChangesToolStripMenuItem.CheckedChanged += SaveDirectlyOnChangesToolStripMenuItemOnCheckedChanged;
        }

        private static TileDatas _refMarker;
        private bool _changingIndex;

        public bool IsLoaded { get; private set; }

        private int? _reselectGraphic;
        private bool? _reselectGraphicLand;

        public static void Select(int graphic, bool land)
        {
            if (!_refMarker.IsLoaded)
            {
                _refMarker.OnLoad(_refMarker, EventArgs.Empty);
                _refMarker._reselectGraphic = graphic;
                _refMarker._reselectGraphicLand = land;
            }

            SearchGraphic(graphic, land);
        }

        public static bool SearchGraphic(int graphic, bool land)
        {
            const int index = 0;
            if (land)
            {
                for (int i = index; i < _refMarker.treeViewLand.Nodes.Count; ++i)
                {
                    TreeNode node = _refMarker.treeViewLand.Nodes[i];
                    if (node.Tag == null || (int)node.Tag != graphic)
                    {
                        continue;
                    }

                    _refMarker.tabcontrol.SelectTab(1);
                    _refMarker.treeViewLand.SelectedNode = node;
                    node.EnsureVisible();
                    return true;
                }
            }
            else
            {
                for (int i = index; i < _refMarker.treeViewItem.Nodes.Count; ++i)
                {
                    for (int j = 0; j < _refMarker.treeViewItem.Nodes[i].Nodes.Count; ++j)
                    {
                        TreeNode node = _refMarker.treeViewItem.Nodes[i].Nodes[j];
                        if (node.Tag == null || (int)node.Tag != graphic)
                        {
                            continue;
                        }

                        _refMarker.tabcontrol.SelectTab(0);
                        _refMarker.treeViewItem.SelectedNode = node;
                        node.EnsureVisible();
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool SearchName(string name, bool next, bool land)
        {
            int index = 0;
            Regex regex = new Regex(name, RegexOptions.IgnoreCase);
            if (land)
            {
                if (next)
                {
                    if (_refMarker.treeViewLand.SelectedNode.Index >= 0)
                    {
                        index = _refMarker.treeViewLand.SelectedNode.Index + 1;
                    }

                    if (index >= _refMarker.treeViewLand.Nodes.Count)
                    {
                        index = 0;
                    }
                }
                for (int i = index; i < _refMarker.treeViewLand.Nodes.Count; ++i)
                {
                    TreeNode node = _refMarker.treeViewLand.Nodes[i];
                    if (node.Tag == null || !regex.IsMatch(TileData.LandTable[(int)node.Tag].Name))
                    {
                        continue;
                    }

                    _refMarker.tabcontrol.SelectTab(1);
                    _refMarker.treeViewLand.SelectedNode = node;
                    node.EnsureVisible();
                    return true;
                }
            }
            else
            {
                int sIndex = 0;
                if (next && _refMarker.treeViewItem.SelectedNode != null)
                {
                    if (_refMarker.treeViewItem.SelectedNode.Parent != null)
                    {
                        index = _refMarker.treeViewItem.SelectedNode.Parent.Index;
                        sIndex = _refMarker.treeViewItem.SelectedNode.Index + 1;
                    }
                    else
                    {
                        index = _refMarker.treeViewItem.SelectedNode.Index;
                        sIndex = 0;
                    }
                }
                for (int i = index; i < _refMarker.treeViewItem.Nodes.Count; ++i)
                {
                    for (int j = sIndex; j < _refMarker.treeViewItem.Nodes[i].Nodes.Count; ++j)
                    {
                        TreeNode node = _refMarker.treeViewItem.Nodes[i].Nodes[j];
                        if (node.Tag == null || !regex.IsMatch(TileData.ItemTable[(int)node.Tag].Name))
                        {
                            continue;
                        }

                        _refMarker.tabcontrol.SelectTab(0);
                        _refMarker.treeViewItem.SelectedNode = node;
                        node.EnsureVisible();
                        return true;
                    }
                    sIndex = 0;
                }
            }
            return false;
        }

        public static void ApplyFilterItem(ItemData item)
        {
            _refMarker.treeViewItem.BeginUpdate();
            _refMarker.treeViewItem.Nodes.Clear();
            var nodes = new List<TreeNode>();
            var nodesSa = new List<TreeNode>();
            var nodesHsa = new List<TreeNode>();
            for (int i = 0; i < TileData.ItemTable.Length; ++i)
            {
                if (!string.IsNullOrEmpty(item.Name) && TileData.ItemTable[i].Name.IndexOf(item.Name, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                if (item.Animation != 0 && TileData.ItemTable[i].Animation != item.Animation)
                {
                    continue;
                }

                if (item.Weight != 0 && TileData.ItemTable[i].Weight != item.Weight)
                {
                    continue;
                }

                if (item.Quality != 0 && TileData.ItemTable[i].Quality != item.Quality)
                {
                    continue;
                }

                if (item.Quantity != 0 && TileData.ItemTable[i].Quantity != item.Quantity)
                {
                    continue;
                }

                if (item.Hue != 0 && TileData.ItemTable[i].Hue != item.Hue)
                {
                    continue;
                }

                if (item.StackingOffset != 0 && TileData.ItemTable[i].StackingOffset != item.StackingOffset)
                {
                    continue;
                }

                if (item.Value != 0 && TileData.ItemTable[i].Value != item.Value)
                {
                    continue;
                }

                if (item.Height != 0 && TileData.ItemTable[i].Height != item.Height)
                {
                    continue;
                }

                if (item.MiscData != 0 && TileData.ItemTable[i].MiscData != item.MiscData)
                {
                    continue;
                }

                if (item.Unk2 != 0 && TileData.ItemTable[i].Unk2 != item.Unk2)
                {
                    continue;
                }

                if (item.Unk3 != 0 && TileData.ItemTable[i].Unk3 != item.Unk3)
                {
                    continue;
                }

                if (item.Flags != 0 && (TileData.ItemTable[i].Flags & item.Flags) == 0)
                {
                    continue;
                }

                TreeNode node = new TreeNode(string.Format("0x{0:X4} ({0}) {1}", i, TileData.ItemTable[i].Name))
                {
                    Tag = i
                };

                if (i < 0x4000)
                {
                    nodes.Add(node);
                }
                else if (i < 0x8000)
                {
                    nodesSa.Add(node);
                }
                else
                {
                    nodesHsa.Add(node);
                }
            }

            if (nodes.Count > 0)
            {
                _refMarker.treeViewItem.Nodes.Add(new TreeNode("AOS - ML", nodes.ToArray()));
            }

            if (nodesSa.Count > 0)
            {
                _refMarker.treeViewItem.Nodes.Add(new TreeNode("Stygian Abyss", nodesSa.ToArray()));
            }

            if (nodesHsa.Count > 0)
            {
                _refMarker.treeViewItem.Nodes.Add(new TreeNode("Adventures High Seas", nodesHsa.ToArray()));
            }

            _refMarker.treeViewItem.EndUpdate();
            if (_refMarker.treeViewItem.Nodes.Count > 0 && _refMarker.treeViewItem.Nodes[0].Nodes.Count > 0)
            {
                _refMarker.treeViewItem.SelectedNode = _refMarker.treeViewItem.Nodes[0].Nodes[0];
            }
        }

        public static void ApplyFilterLand(LandData land)
        {
            _refMarker.treeViewLand.BeginUpdate();
            _refMarker.treeViewLand.Nodes.Clear();
            var nodes = new List<TreeNode>();
            for (int i = 0; i < TileData.LandTable.Length; ++i)
            {
                if (!string.IsNullOrEmpty(land.Name) && TileData.ItemTable[i].Name.IndexOf(land.Name, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                if (land.TextureId != 0 && TileData.LandTable[i].TextureId != land.TextureId)
                {
                    continue;
                }

                if (land.Flags != 0 && (TileData.LandTable[i].Flags & land.Flags) == 0)
                {
                    continue;
                }

                TreeNode node = new TreeNode(string.Format("0x{0:X4} ({0}) {1}", i, TileData.LandTable[i].Name))
                {
                    Tag = i
                };
                nodes.Add(node);
            }

            _refMarker.treeViewLand.Nodes.AddRange(nodes.ToArray());
            _refMarker.treeViewLand.EndUpdate();

            if (_refMarker.treeViewLand.Nodes.Count > 0)
            {
                _refMarker.treeViewLand.SelectedNode = _refMarker.treeViewLand.Nodes[0];
            }
        }

        private void Reload()
        {
            if (IsLoaded)
            {
                OnLoad(this, new MyEventArgs(MyEventArgs.Types.ForceReload));
            }
        }

        public void OnLoad(object sender, EventArgs e)
        {
            if (_reselectGraphic != null && _reselectGraphicLand != null)
            {
                SearchGraphic(_reselectGraphic.Value, _reselectGraphicLand.Value);
                _reselectGraphic = null;
                _reselectGraphicLand = null;
            }

            if (IsLoaded && (!(e is MyEventArgs args) || args.Type != MyEventArgs.Types.ForceReload))
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            Options.LoadedUltimaClass["TileData"] = true;
            Options.LoadedUltimaClass["Art"] = true;
            treeViewItem.BeginUpdate();
            treeViewItem.Nodes.Clear();
            if (TileData.ItemTable != null)
            {
                var nodes = new TreeNode[0x4000];
                for (int i = 0; i < 0x4000; ++i)
                {
                    nodes[i] = new TreeNode(string.Format("0x{0:X4} ({0}) {1}", i, TileData.ItemTable[i].Name))
                    {
                        Tag = i
                    };
                }
                treeViewItem.Nodes.Add(new TreeNode("AOS - ML", nodes));

                if (TileData.ItemTable.Length > 0x4000) // SA
                {
                    nodes = new TreeNode[0x4000];
                    for (int i = 0; i < 0x4000; ++i)
                    {
                        int j = i + 0x4000;
                        nodes[i] = new TreeNode(string.Format("0x{0:X4} ({0}) {1}", j, TileData.ItemTable[j].Name))
                        {
                            Tag = j
                        };
                    }
                    treeViewItem.Nodes.Add(new TreeNode("Stygian Abyss", nodes));
                }

                if (TileData.ItemTable.Length > 0x8000) // AHS
                {
                    nodes = new TreeNode[0x8000];
                    for (int i = 0; i < 0x8000; ++i)
                    {
                        int j = i + 0x8000;
                        nodes[i] = new TreeNode(string.Format("0x{0:X4} ({0}) {1}", j, TileData.ItemTable[j].Name))
                        {
                            Tag = j
                        };
                    }
                    treeViewItem.Nodes.Add(new TreeNode("Adventures High Seas", nodes));
                }
                else
                {
                    treeViewItem.ExpandAll();
                }
            }
            treeViewItem.EndUpdate();

            treeViewLand.BeginUpdate();
            treeViewLand.Nodes.Clear();
            if (TileData.LandTable != null)
            {
                var nodes = new TreeNode[TileData.LandTable.Length];
                for (int i = 0; i < TileData.LandTable.Length; ++i)
                {
                    nodes[i] = new TreeNode(string.Format("0x{0:X4} ({0}) {1}", i, TileData.LandTable[i].Name))
                    {
                        Tag = i
                    };
                }
                treeViewLand.Nodes.AddRange(nodes);
            }
            treeViewLand.EndUpdate();

            if (!IsLoaded)
            {
                FiddlerControls.Events.FilePathChangeEvent += OnFilePathChangeEvent;
                FiddlerControls.Events.TileDataChangeEvent += OnTileDataChangeEvent;
            }
            IsLoaded = true;
            Cursor.Current = Cursors.Default;
        }

        private void OnFilePathChangeEvent()
        {
            Reload();
        }

        private void OnTileDataChangeEvent(object sender, int index)
        {
            if (!IsLoaded)
            {
                return;
            }

            if (sender.Equals(this))
            {
                return;
            }

            if (index > 0x3FFF) // items
            {
                if (treeViewItem.SelectedNode == null)
                {
                    return;
                }

                if ((int)treeViewItem.SelectedNode.Tag == index)
                {
                    treeViewItem.SelectedNode.ForeColor = Color.Red;
                    AfterSelectTreeViewItem(this, new TreeViewEventArgs(treeViewItem.SelectedNode));
                }
                else
                {
                    foreach (TreeNode parentNode in treeViewItem.Nodes)
                    {
                        foreach (TreeNode node in parentNode.Nodes)
                        {
                            if ((int)node.Tag != index)
                            {
                                continue;
                            }

                            node.ForeColor = Color.Red;
                            break;
                        }
                    }
                }
            }
            else
            {
                if (treeViewLand.SelectedNode == null)
                {
                    return;
                }

                if ((int)treeViewLand.SelectedNode.Tag == index)
                {
                    treeViewLand.SelectedNode.ForeColor = Color.Red;
                    AfterSelectTreeViewLand(this, new TreeViewEventArgs(treeViewLand.SelectedNode));
                }
                else
                {
                    foreach (TreeNode node in treeViewLand.Nodes)
                    {
                        if ((int)node.Tag != index)
                        {
                            continue;
                        }

                        node.ForeColor = Color.Red;
                        break;
                    }
                }
            }
        }

        private void AfterSelectTreeViewItem(object sender, TreeViewEventArgs e)
        {
            if (e.Node?.Tag == null)
            {
                return;
            }

            int index = (int)e.Node.Tag;
            try
            {
                Bitmap bit = Art.GetStatic(index);
                Bitmap newBit = new Bitmap(pictureBoxItem.Size.Width, pictureBoxItem.Size.Height);
                Graphics newGraph = Graphics.FromImage(newBit);
                newGraph.Clear(Color.FromArgb(-1));
                newGraph.DrawImage(bit, (pictureBoxItem.Size.Width - bit.Width) / 2, 1);
                pictureBoxItem.Image = newBit;
            }
            catch
            {
                pictureBoxItem.Image = new Bitmap(pictureBoxItem.Width, pictureBoxItem.Height);
            }
            ItemData data = TileData.ItemTable[index];
            _changingIndex = true;
            textBoxName.Text = data.Name;
            textBoxAnim.Text = data.Animation.ToString();
            textBoxWeight.Text = data.Weight.ToString();
            textBoxQuality.Text = data.Quality.ToString();
            textBoxQuantity.Text = data.Quantity.ToString();
            textBoxHue.Text = data.Hue.ToString();
            textBoxStackOff.Text = data.StackingOffset.ToString();
            textBoxValue.Text = data.Value.ToString();
            textBoxHeigth.Text = data.Height.ToString();
            textBoxUnk1.Text = data.MiscData.ToString();
            textBoxUnk2.Text = data.Unk2.ToString();
            textBoxUnk3.Text = data.Unk3.ToString();
            textBoxUnk1HSA.Text = data.Unk1.ToString();
            Array enumValues = Enum.GetValues(typeof(TileFlag));
            for (int i = 1; i < enumValues.Length; ++i)
            {
                checkedListBox1.SetItemChecked(i - 1, (data.Flags & (TileFlag)enumValues.GetValue(i)) != 0);
            }
            _changingIndex = false;
        }

        private void AfterSelectTreeViewLand(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null)
            {
                return;
            }

            int index = (int)e.Node.Tag;
            try
            {
                Bitmap bit = Art.GetLand(index);
                Bitmap newBit = new Bitmap(pictureBoxLand.Size.Width, pictureBoxLand.Size.Height);
                Graphics newGraph = Graphics.FromImage(newBit);
                newGraph.Clear(Color.FromArgb(-1));
                newGraph.DrawImage(bit, (pictureBoxLand.Size.Width - bit.Width) / 2, 1);
                pictureBoxLand.Image = newBit;
            }
            catch
            {
                pictureBoxLand.Image = new Bitmap(pictureBoxLand.Width, pictureBoxLand.Height);
            }
            LandData data = TileData.LandTable[index];
            _changingIndex = true;
            textBoxNameLand.Text = data.Name;
            textBoxTexID.Text = data.TextureId.ToString();
            textBoxUnkLandHSA.Text = data.Unk1.ToString();

            checkedListBox2.SetItemChecked(0, (data.Flags & TileFlag.Damaging) != 0);
            checkedListBox2.SetItemChecked(1, (data.Flags & TileFlag.Wet) != 0);
            checkedListBox2.SetItemChecked(2, (data.Flags & TileFlag.Impassable) != 0);
            checkedListBox2.SetItemChecked(3, (data.Flags & TileFlag.Wall) != 0);
            checkedListBox2.SetItemChecked(4, (data.Flags & TileFlag.Unknown3) != 0);

            _changingIndex = false;
        }

        private void OnClickSaveTiledata(object sender, EventArgs e)
        {
            string path = Options.OutputPath;
            string fileName = Path.Combine(path, "tiledata.mul");
            TileData.SaveTileData(fileName);
            MessageBox.Show(
                $"TileData saved to {fileName}",
                "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
            Options.ChangedUltimaClass["TileData"] = false;
        }

        private void OnClickSaveChanges(object sender, EventArgs e)
        {
            if (tabcontrol.SelectedIndex == 0) //items
            {
                if (treeViewItem.SelectedNode?.Tag == null)
                {
                    return;
                }

                int index = (int)treeViewItem.SelectedNode.Tag;
                ItemData item = TileData.ItemTable[index];
                string name = textBoxName.Text;
                if (name.Length > 20)
                {
                    name = name.Substring(0, 20);
                }

                item.Name = name;
                treeViewItem.SelectedNode.Text = string.Format("0x{0:X4} ({0}) {1}", index, name);
                if (short.TryParse(textBoxAnim.Text, out short shortRes))
                {
                    item.Animation = shortRes;
                }

                if (byte.TryParse(textBoxWeight.Text, out byte byteRes))
                {
                    item.Weight = byteRes;
                }

                if (byte.TryParse(textBoxQuality.Text, out byteRes))
                {
                    item.Quality = byteRes;
                }

                if (byte.TryParse(textBoxQuantity.Text, out byteRes))
                {
                    item.Quantity = byteRes;
                }

                if (byte.TryParse(textBoxHue.Text, out byteRes))
                {
                    item.Hue = byteRes;
                }

                if (byte.TryParse(textBoxStackOff.Text, out byteRes))
                {
                    item.StackingOffset = byteRes;
                }

                if (byte.TryParse(textBoxValue.Text, out byteRes))
                {
                    item.Value = byteRes;
                }

                if (byte.TryParse(textBoxHeigth.Text, out byteRes))
                {
                    item.Height = byteRes;
                }

                if (short.TryParse(textBoxUnk1.Text, out shortRes))
                {
                    item.MiscData = shortRes;
                }

                if (byte.TryParse(textBoxUnk2.Text, out byteRes))
                {
                    item.Unk2 = byteRes;
                }

                if (byte.TryParse(textBoxUnk3.Text, out byteRes))
                {
                    item.Unk3 = byteRes;
                }

                if (int.TryParse(textBoxUnk1HSA.Text, out int intRes))
                {
                    item.Unk1 = intRes;
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

                TileData.ItemTable[index] = item;
                treeViewItem.SelectedNode.ForeColor = Color.Red;
                Options.ChangedUltimaClass["TileData"] = true;
                FiddlerControls.Events.FireTileDataChangeEvent(this, index + 0x4000);
                if (memorySaveWarningToolStripMenuItem.Checked)
                {
                    MessageBox.Show(
                        string.Format("Edits of 0x{0:X4} ({0}) saved to memory. Click 'Save Tiledata' to write to file.", index), "Saved",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information,
                        MessageBoxDefaultButton.Button1);
                }
            }
            else // land
            {
                if (treeViewLand.SelectedNode == null)
                {
                    return;
                }

                int index = (int)treeViewLand.SelectedNode.Tag;
                LandData land = TileData.LandTable[index];
                string name = textBoxNameLand.Text;
                if (name.Length > 20)
                {
                    name = name.Substring(0, 20);
                }

                land.Name = name;
                treeViewLand.SelectedNode.Text = $"0x{index:X4} {name}";
                if (short.TryParse(textBoxTexID.Text, out short shortRes))
                {
                    land.TextureId = shortRes;
                }

                if (int.TryParse(textBoxUnkLandHSA.Text, out int intRes))
                {
                    land.Unk1 = intRes;
                }

                land.Flags = TileFlag.None;
                if (checkedListBox2.GetItemChecked(0))
                {
                    land.Flags |= TileFlag.Damaging;
                }

                if (checkedListBox2.GetItemChecked(1))
                {
                    land.Flags |= TileFlag.Wet;
                }

                if (checkedListBox2.GetItemChecked(2))
                {
                    land.Flags |= TileFlag.Impassable;
                }

                if (checkedListBox2.GetItemChecked(3))
                {
                    land.Flags |= TileFlag.Wall;
                }

                if (checkedListBox2.GetItemChecked(4))
                {
                    land.Flags |= TileFlag.Unknown3;
                }

                TileData.LandTable[index] = land;
                Options.ChangedUltimaClass["TileData"] = true;
                FiddlerControls.Events.FireTileDataChangeEvent(this, index);
                treeViewLand.SelectedNode.ForeColor = Color.Red;
                if (memorySaveWarningToolStripMenuItem.Checked)
                {
                    MessageBox.Show(
                        string.Format("Edits of 0x{0:X4} ({0}) saved to memory. Click 'Save Tiledata' to write to file.", index), "Saved",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information,
                        MessageBoxDefaultButton.Button1);
                }
            }
        }

        private void SaveDirectlyOnChangesToolStripMenuItemOnCheckedChanged(object sender, EventArgs eventArgs)
        {
            Options.TileDataDirectlySaveOnChange = saveDirectlyOnChangesToolStripMenuItem.Checked;
        }

        private void OnFlagItemCheckItems(object sender, ItemCheckEventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            if (e.CurrentValue == e.NewValue)
            {
                return;
            }

            if (treeViewItem.SelectedNode?.Tag == null)
            {
                return;
            }

            int index = (int)treeViewItem.SelectedNode.Tag;
            ItemData item = TileData.ItemTable[index];
            Array enumValues = Enum.GetValues(typeof(TileFlag));
            TileFlag changeFlag = (TileFlag)enumValues.GetValue(e.Index + 1);
            if ((item.Flags & changeFlag) != 0) // better double check
            {
                if (e.NewValue != CheckState.Unchecked)
                {
                    return;
                }

                item.Flags ^= changeFlag;
                TileData.ItemTable[index] = item;
                treeViewItem.SelectedNode.ForeColor = Color.Red;
                Options.ChangedUltimaClass["TileData"] = true;
                FiddlerControls.Events.FireTileDataChangeEvent(this, index + 0x4000);
            }
            else if ((item.Flags & changeFlag) == 0)
            {
                if (e.NewValue != CheckState.Checked)
                {
                    return;
                }

                item.Flags |= changeFlag;
                TileData.ItemTable[index] = item;
                treeViewItem.SelectedNode.ForeColor = Color.Red;
                Options.ChangedUltimaClass["TileData"] = true;
                FiddlerControls.Events.FireTileDataChangeEvent(this, index + 0x4000);
            }
        }

        private void OnTextChangedItemAnim(object sender, EventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            if (treeViewItem.SelectedNode?.Tag == null)
            {
                return;
            }

            int index = (int)treeViewItem.SelectedNode.Tag;
            ItemData item = TileData.ItemTable[index];
            if (!short.TryParse(textBoxAnim.Text, out short shortRes))
            {
                return;
            }

            item.Animation = shortRes;
            TileData.ItemTable[index] = item;
            treeViewItem.SelectedNode.ForeColor = Color.Red;
            Options.ChangedUltimaClass["TileData"] = true;
            FiddlerControls.Events.FireTileDataChangeEvent(this, index + 0x4000);
        }

        private void OnTextChangedItemName(object sender, EventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            if (treeViewItem.SelectedNode?.Tag == null)
            {
                return;
            }

            int index = (int)treeViewItem.SelectedNode.Tag;
            ItemData item = TileData.ItemTable[index];
            string name = textBoxName.Text;
            if (name.Length == 0)
            {
                return;
            }

            if (name.Length > 20)
            {
                name = name.Substring(0, 20);
            }

            item.Name = name;
            //treeViewItem.SelectedNode.Text = String.Format("0x{0:X4} ({0}) {1}", index, name);
            TileData.ItemTable[index] = item;
            treeViewItem.SelectedNode.ForeColor = Color.Red;
            Options.ChangedUltimaClass["TileData"] = true;
            FiddlerControls.Events.FireTileDataChangeEvent(this, index + 0x4000);
        }

        private void TreeViewItemOnBeforeSelect(object sender, TreeViewCancelEventArgs treeViewCancelEventArgs)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (treeViewItem.SelectedNode?.Tag == null)
            {
                return;
            }

            int index = (int)treeViewItem.SelectedNode.Tag;
            ItemData item = TileData.ItemTable[index];

            string itemText = string.Format("0x{0:X4} ({0}) {1}", index, item.Name);
            if (treeViewItem.SelectedNode.Text != itemText)
            {
                treeViewItem.SelectedNode.Text = string.Format("0x{0:X4} ({0}) {1}", index, item.Name);
            }
        }

        private void OnTextChangedItemWeight(object sender, EventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            if (treeViewItem.SelectedNode?.Tag == null)
            {
                return;
            }

            int index = (int)treeViewItem.SelectedNode.Tag;
            ItemData item = TileData.ItemTable[index];
            if (!byte.TryParse(textBoxWeight.Text, out byte byteRes))
            {
                return;
            }

            item.Weight = byteRes;
            TileData.ItemTable[index] = item;
            treeViewItem.SelectedNode.ForeColor = Color.Red;
            Options.ChangedUltimaClass["TileData"] = true;
            FiddlerControls.Events.FireTileDataChangeEvent(this, index + 0x4000);
        }

        private void OnTextChangedItemQuality(object sender, EventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            if (treeViewItem.SelectedNode?.Tag == null)
            {
                return;
            }

            int index = (int)treeViewItem.SelectedNode.Tag;
            ItemData item = TileData.ItemTable[index];
            if (!byte.TryParse(textBoxQuality.Text, out byte byteRes))
            {
                return;
            }

            item.Quality = byteRes;
            TileData.ItemTable[index] = item;
            treeViewItem.SelectedNode.ForeColor = Color.Red;
            Options.ChangedUltimaClass["TileData"] = true;
            FiddlerControls.Events.FireTileDataChangeEvent(this, index + 0x4000);
        }

        private void OnTextChangedItemQuantity(object sender, EventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            if (treeViewItem.SelectedNode?.Tag == null)
            {
                return;
            }

            int index = (int)treeViewItem.SelectedNode.Tag;
            ItemData item = TileData.ItemTable[index];
            if (!byte.TryParse(textBoxQuantity.Text, out byte byteRes))
            {
                return;
            }

            item.Quantity = byteRes;
            TileData.ItemTable[index] = item;
            treeViewItem.SelectedNode.ForeColor = Color.Red;
            Options.ChangedUltimaClass["TileData"] = true;
            FiddlerControls.Events.FireTileDataChangeEvent(this, index + 0x4000);
        }

        private void OnTextChangedItemHue(object sender, EventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            if (treeViewItem.SelectedNode?.Tag == null)
            {
                return;
            }

            int index = (int)treeViewItem.SelectedNode.Tag;
            ItemData item = TileData.ItemTable[index];
            if (byte.TryParse(textBoxHue.Text, out byte byteRes))
            {
                item.Hue = byteRes;
                TileData.ItemTable[index] = item;
                treeViewItem.SelectedNode.ForeColor = Color.Red;
                Options.ChangedUltimaClass["TileData"] = true;
                FiddlerControls.Events.FireTileDataChangeEvent(this, index + 0x4000);
            }
        }

        private void OnTextChangedItemStackOff(object sender, EventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            if (treeViewItem.SelectedNode?.Tag == null)
            {
                return;
            }

            int index = (int)treeViewItem.SelectedNode.Tag;
            ItemData item = TileData.ItemTable[index];
            if (byte.TryParse(textBoxStackOff.Text, out byte byteRes))
            {
                item.StackingOffset = byteRes;
                TileData.ItemTable[index] = item;
                treeViewItem.SelectedNode.ForeColor = Color.Red;
                Options.ChangedUltimaClass["TileData"] = true;
                FiddlerControls.Events.FireTileDataChangeEvent(this, index + 0x4000);
            }
        }

        private void OnTextChangedItemValue(object sender, EventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            if (treeViewItem.SelectedNode?.Tag == null)
            {
                return;
            }

            int index = (int)treeViewItem.SelectedNode.Tag;
            ItemData item = TileData.ItemTable[index];
            if (byte.TryParse(textBoxValue.Text, out byte byteRes))
            {
                item.Value = byteRes;
                TileData.ItemTable[index] = item;
                treeViewItem.SelectedNode.ForeColor = Color.Red;
                Options.ChangedUltimaClass["TileData"] = true;
                FiddlerControls.Events.FireTileDataChangeEvent(this, index + 0x4000);
            }
        }

        private void OnTextChangedItemHeight(object sender, EventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            if (treeViewItem.SelectedNode?.Tag == null)
            {
                return;
            }

            int index = (int)treeViewItem.SelectedNode.Tag;
            ItemData item = TileData.ItemTable[index];
            if (byte.TryParse(textBoxHeigth.Text, out byte byteRes))
            {
                item.Height = byteRes;
                TileData.ItemTable[index] = item;
                treeViewItem.SelectedNode.ForeColor = Color.Red;
                Options.ChangedUltimaClass["TileData"] = true;
                FiddlerControls.Events.FireTileDataChangeEvent(this, index + 0x4000);
            }
        }

        private void OnTextChangedItemMiscData(object sender, EventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            if (treeViewItem.SelectedNode?.Tag == null)
            {
                return;
            }

            int index = (int)treeViewItem.SelectedNode.Tag;
            ItemData item = TileData.ItemTable[index];
            if (short.TryParse(textBoxUnk1.Text, out short shortRes))
            {
                item.MiscData = shortRes;
                TileData.ItemTable[index] = item;
                treeViewItem.SelectedNode.ForeColor = Color.Red;
                Options.ChangedUltimaClass["TileData"] = true;
                FiddlerControls.Events.FireTileDataChangeEvent(this, index + 0x4000);
            }
        }

        private void OnTextChangedItemUnk2(object sender, EventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            if (treeViewItem.SelectedNode?.Tag == null)
            {
                return;
            }

            int index = (int)treeViewItem.SelectedNode.Tag;
            ItemData item = TileData.ItemTable[index];
            if (byte.TryParse(textBoxUnk2.Text, out byte byteRes))
            {
                item.Unk2 = byteRes;
                TileData.ItemTable[index] = item;
                treeViewItem.SelectedNode.ForeColor = Color.Red;
                Options.ChangedUltimaClass["TileData"] = true;
                FiddlerControls.Events.FireTileDataChangeEvent(this, index + 0x4000);
            }
        }

        private void OnTextChangedItemUnk3(object sender, EventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            if (treeViewItem.SelectedNode?.Tag == null)
            {
                return;
            }

            int index = (int)treeViewItem.SelectedNode.Tag;
            ItemData item = TileData.ItemTable[index];
            if (byte.TryParse(textBoxUnk3.Text, out byte byteRes))
            {
                item.Unk3 = byteRes;
                TileData.ItemTable[index] = item;
                treeViewItem.SelectedNode.ForeColor = Color.Red;
                Options.ChangedUltimaClass["TileData"] = true;
                FiddlerControls.Events.FireTileDataChangeEvent(this, index + 0x4000);
            }
        }

        private void OnTextChangedItemUnk1HSA(object sender, EventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            if (treeViewItem.SelectedNode?.Tag == null)
            {
                return;
            }

            int index = (int)treeViewItem.SelectedNode.Tag;
            ItemData item = TileData.ItemTable[index];
            if (int.TryParse(textBoxUnk1HSA.Text, out int intRes))
            {
                item.Unk1 = intRes;
                TileData.ItemTable[index] = item;
                treeViewItem.SelectedNode.ForeColor = Color.Red;
                Options.ChangedUltimaClass["TileData"] = true;
                FiddlerControls.Events.FireTileDataChangeEvent(this, index + 0x4000);
            }
        }

        private void OnTextChangedLandName(object sender, EventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            if (treeViewLand.SelectedNode?.Tag == null)
            {
                return;
            }

            int index = (int)treeViewLand.SelectedNode.Tag;
            LandData land = TileData.LandTable[index];
            string name = textBoxNameLand.Text;
            if (name.Length == 0)
            {
                return;
            }

            if (name.Length > 20)
            {
                name = name.Substring(0, 20);
            }

            land.Name = name;
            treeViewLand.SelectedNode.Text = string.Format("0x{0:X4} ({0}) {1}", index, name);
            TileData.LandTable[index] = land;
            treeViewLand.SelectedNode.ForeColor = Color.Red;
            Options.ChangedUltimaClass["TileData"] = true;
            FiddlerControls.Events.FireTileDataChangeEvent(this, index);
        }

        private void OnTextChangedLandTexID(object sender, EventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            if (treeViewLand.SelectedNode == null)
            {
                return;
            }

            int index = (int)treeViewLand.SelectedNode.Tag;
            LandData land = TileData.LandTable[index];
            if (short.TryParse(textBoxTexID.Text, out short shortRes))
            {
                land.TextureId = shortRes;
                TileData.LandTable[index] = land;
                treeViewLand.SelectedNode.ForeColor = Color.Red;
                Options.ChangedUltimaClass["TileData"] = true;
                FiddlerControls.Events.FireTileDataChangeEvent(this, index);
            }
        }

        private void OnTextChangedLandUnkHSA(object sender, EventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            if (treeViewLand.SelectedNode == null)
            {
                return;
            }

            int index = (int)treeViewLand.SelectedNode.Tag;
            LandData land = TileData.LandTable[index];
            if (int.TryParse(textBoxUnkLandHSA.Text, out int intRes))
            {
                land.Unk1 = intRes;
                TileData.LandTable[index] = land;
                treeViewLand.SelectedNode.ForeColor = Color.Red;
                Options.ChangedUltimaClass["TileData"] = true;
                FiddlerControls.Events.FireTileDataChangeEvent(this, index);
            }
        }

        private void OnFlagItemCheckLandTiles(object sender, ItemCheckEventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            if (e.CurrentValue == e.NewValue)
            {
                return;
            }

            if (treeViewLand.SelectedNode == null)
            {
                return;
            }

            int index = (int)treeViewLand.SelectedNode.Tag;
            LandData land = TileData.LandTable[index];
            TileFlag changeFlag;
            switch (e.Index)
            {
                case 0:
                    changeFlag = TileFlag.Damaging;
                    break;
                case 1:
                    changeFlag = TileFlag.Wet;
                    break;
                case 2:
                    changeFlag = TileFlag.Impassable;
                    break;
                case 3:
                    changeFlag = TileFlag.Wall;
                    break;
                case 4:
                    changeFlag = TileFlag.Unknown3;
                    break;
                default:
                    changeFlag = TileFlag.None;
                    break;
            }

            if ((land.Flags & changeFlag) != 0)
            {
                if (e.NewValue != CheckState.Unchecked)
                {
                    return;
                }

                land.Flags ^= changeFlag;
                TileData.LandTable[index] = land;
                treeViewLand.SelectedNode.ForeColor = Color.Red;
                Options.ChangedUltimaClass["TileData"] = true;
                FiddlerControls.Events.FireTileDataChangeEvent(this, index);
            }
            else if ((land.Flags & changeFlag) == 0)
            {
                if (e.NewValue != CheckState.Checked)
                {
                    return;
                }

                land.Flags |= changeFlag;
                TileData.LandTable[index] = land;
                treeViewLand.SelectedNode.ForeColor = Color.Red;
                Options.ChangedUltimaClass["TileData"] = true;
                FiddlerControls.Events.FireTileDataChangeEvent(this, index);
            }
        }

        private void OnClickExport(object sender, EventArgs e)
        {
            string path = Options.OutputPath;
            if (tabcontrol.SelectedIndex == 0) // items
            {
                string fileName = Path.Combine(path, "ItemData.csv");
                TileData.ExportItemDataToCsv(fileName);
                MessageBox.Show($"ItemData saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
            else
            {
                string fileName = Path.Combine(path, "LandData.csv");
                TileData.ExportLandDataToCsv(fileName);
                MessageBox.Show($"LandData saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
        }

        private TileDatasSearch _showForm1;
        private TileDatasSearch _showForm2;

        private void OnClickSearch(object sender, EventArgs e)
        {
            if (tabcontrol.SelectedIndex == 0) // items
            {
                if (_showForm1?.IsDisposed == false)
                {
                    return;
                }

                _showForm1 = new TileDatasSearch(false)
                {
                    TopMost = true
                };
                _showForm1.Show();
            }
            else // land tiles
            {
                if (_showForm2?.IsDisposed == false)
                {
                    return;
                }

                _showForm2 = new TileDatasSearch(true)
                {
                    TopMost = true
                };
                _showForm2.Show();
            }
        }

        private void OnClickSelectItem(object sender, EventArgs e)
        {
            if (treeViewItem.SelectedNode == null)
            {
                return;
            }

            int index = (int)treeViewItem.SelectedNode.Tag;
            if (Options.DesignAlternative)
            {
                ItemShowAlternative.SearchGraphic(index);
            }
            else
            {
                ItemShow.SearchGraphic(index);
            }
        }

        private void OnClickSelectInLandTiles(object sender, EventArgs e)
        {
            if (treeViewLand.SelectedNode == null)
            {
                return;
            }

            int index = (int)treeViewLand.SelectedNode.Tag;
            if (Options.DesignAlternative)
            {
                LandTilesAlternative.SearchGraphic(index);
            }
            else
            {
                LandTiles.SearchGraphic(index);
            }
        }

        private void OnClickSelectRadarItem(object sender, EventArgs e)
        {
            if (treeViewItem.SelectedNode == null)
            {
                return;
            }

            int index = (int)treeViewItem.SelectedNode.Tag;
            RadarColor.Select(index, false);
        }

        private void OnClickSelectRadarLand(object sender, EventArgs e)
        {
            if (treeViewLand.SelectedNode == null)
            {
                return;
            }

            int index = (int)treeViewLand.SelectedNode.Tag;
            RadarColor.Select(index, true);
        }

        private void OnClickImport(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Multiselect = false,
                Title = "Choose csv file to import",
                CheckFileExists = true,
                Filter = "csv files (*.csv)|*.csv"
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Options.ChangedUltimaClass["TileData"] = true;
                if (tabcontrol.SelectedIndex == 0) // items
                {
                    TileData.ImportItemDataFromCsv(dialog.FileName);
                    AfterSelectTreeViewItem(this, new TreeViewEventArgs(treeViewItem.SelectedNode));
                }
                else
                {
                    TileData.ImportLandDataFromCsv(dialog.FileName);
                    AfterSelectTreeViewLand(this, new TreeViewEventArgs(treeViewLand.SelectedNode));
                }
            }
            dialog.Dispose();
        }

        private TileDataFilter _filterForm;

        private void OnClickSetFilter(object sender, EventArgs e)
        {
            if (_filterForm?.IsDisposed == false)
            {
                return;
            }

            _filterForm = new TileDataFilter
            {
                TopMost = true
            };
            _filterForm.Show();
        }

        private void OnItemDataNodeExpanded(object sender, TreeViewCancelEventArgs e)
        {
            // TODO: is it still viable maybe they've fixed it? 
            if (treeViewItem.Nodes.Count == 3) // workaround for 65536 items microsoft bug
            {
                treeViewItem.CollapseAll();
            }
        }

        private void TileData_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.F || !e.Control)
            {
                return;
            }

            OnClickSearch(sender, e);
            e.SuppressKeyPress = true;
            e.Handled = true;
        }
    }
}
