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
            refMarker = this;
        }

        private static TileDatas refMarker = null;
        private bool ChangingIndex = false;
        private bool Loaded = false;

        public bool isLoaded { get { return Loaded; } }


        public static void Select(int graphic, bool land)
        {
            if (!refMarker.isLoaded)
                refMarker.OnLoad(refMarker, EventArgs.Empty);
            SearchGraphic(graphic, land);
        }
        public static bool SearchGraphic(int graphic, bool land)
        {
            int index = 0;
            if (land)
            {
                for (int i = index; i < refMarker.treeViewLand.Nodes.Count; ++i)
                {
                    TreeNode node = refMarker.treeViewLand.Nodes[i];
                    if (node.Tag != null)
                    {
                        if ((int)node.Tag == graphic)
                        {
                            refMarker.tabcontrol.SelectTab(1);
                            refMarker.treeViewLand.SelectedNode = node;
                            node.EnsureVisible();
                            return true;
                        }
                    }
                }
            }
            else
            {
                for (int i = index; i < refMarker.treeViewItem.Nodes.Count; ++i)
                {
                    for (int j = 0; j < refMarker.treeViewItem.Nodes[i].Nodes.Count; ++j)
                    {
                        TreeNode node = refMarker.treeViewItem.Nodes[i].Nodes[j];
                        if (node.Tag != null)
                        {
                            if ((int)node.Tag == graphic)
                            {
                                refMarker.tabcontrol.SelectTab(0);
                                refMarker.treeViewItem.SelectedNode = node;
                                node.EnsureVisible();
                                return true;
                            }
                        }
                    }

                }
            }
            return false;
        }

        public static bool SearchName(string name, bool next, bool land)
        {
            int index = 0;
            Regex regex = new Regex(@name, RegexOptions.IgnoreCase);
            if (land)
            {
                if (next)
                {
                    if (refMarker.treeViewLand.SelectedNode.Index >= 0)
                        index = refMarker.treeViewLand.SelectedNode.Index + 1;
                    if (index >= refMarker.treeViewLand.Nodes.Count)
                        index = 0;
                }
                for (int i = index; i < refMarker.treeViewLand.Nodes.Count; ++i)
                {
                    TreeNode node = refMarker.treeViewLand.Nodes[i];
                    if (node.Tag != null)
                    {
                        if (regex.IsMatch(TileData.LandTable[(int)node.Tag].Name))
                        {
                            refMarker.tabcontrol.SelectTab(1);
                            refMarker.treeViewLand.SelectedNode = node;
                            node.EnsureVisible();
                            return true;
                        }
                    }
                }
            }
            else
            {
                int s_index=0;
                if (next)
                {
                    if (refMarker.treeViewItem.SelectedNode!=null)
                    {
                        if (refMarker.treeViewItem.SelectedNode.Parent!=null)
                        {
                            index=refMarker.treeViewItem.SelectedNode.Parent.Index;
                            s_index=refMarker.treeViewItem.SelectedNode.Index+1;
                        }
                        else
                        {
                            index = refMarker.treeViewItem.SelectedNode.Index;
                            s_index = 0;
                        }
                    }
                }
                for (int i = index; i < refMarker.treeViewItem.Nodes.Count; ++i)
                {
                    for (int j = s_index; j < refMarker.treeViewItem.Nodes[i].Nodes.Count; ++j)
                    {
                        TreeNode node = refMarker.treeViewItem.Nodes[i].Nodes[j];
                        if (node.Tag != null)
                        {
                            if (regex.IsMatch(TileData.ItemTable[(int)node.Tag].Name))
                            {
                                refMarker.tabcontrol.SelectTab(0);
                                refMarker.treeViewItem.SelectedNode = node;
                                node.EnsureVisible();
                                return true;
                            }
                        }
                    }
                    s_index = 0;
                }
            }
            return false;
        }

        public static void ApplyFilterItem(ItemData item)
        {
            refMarker.treeViewItem.BeginUpdate();
            refMarker.treeViewItem.Nodes.Clear();
            List<TreeNode> nodes = new List<TreeNode>();
            List<TreeNode> nodesSA = new List<TreeNode>();
            List<TreeNode> nodesHSA = new List<TreeNode>();
            for (int i = 0; i < TileData.ItemTable.Length; ++i)
            {
                if (!String.IsNullOrEmpty(item.Name))
                {
                    if (!TileData.ItemTable[i].Name.ToLower().Contains(item.Name.ToLower()))
                        continue;
                }
                if (item.Animation != 0)
                {
                    if (TileData.ItemTable[i].Animation != item.Animation)
                        continue;
                }
                if (item.Weight != 0)
                {
                    if (TileData.ItemTable[i].Weight != item.Weight)
                        continue;
                }
                if (item.Quality != 0)
                {
                    if (TileData.ItemTable[i].Quality != item.Quality)
                        continue;
                }
                if (item.Quantity != 0)
                {
                    if (TileData.ItemTable[i].Quantity != item.Quantity)
                        continue;
                }
                if (item.Hue != 0)
                {
                    if (TileData.ItemTable[i].Hue != item.Hue)
                        continue;
                }
                if (item.StackingOffset != 0)
                {
                    if (TileData.ItemTable[i].StackingOffset != item.StackingOffset)
                        continue;
                }
                if (item.Value != 0)
                {
                    if (TileData.ItemTable[i].Value != item.Value)
                        continue;
                }
                if (item.Height != 0)
                {
                    if (TileData.ItemTable[i].Height != item.Height)
                        continue;
                }
                if (item.MiscData != 0)
                {
                    if (TileData.ItemTable[i].MiscData != item.MiscData)
                        continue;
                }
                if (item.Unk2 != 0)
                {
                    if (TileData.ItemTable[i].Unk2 != item.Unk2)
                        continue;
                }
                if (item.Unk3 != 0)
                {
                    if (TileData.ItemTable[i].Unk3 != item.Unk3)
                        continue;
                }
                if (item.Flags != 0)
                {
                    if ((TileData.ItemTable[i].Flags & item.Flags) == 0)
                        continue;
                }
                TreeNode node = new TreeNode(String.Format("0x{0:X4} ({0}) {1}", i, TileData.ItemTable[i].Name));
                node.Tag = i;
                if (i < 0x4000)
                    nodes.Add(node);
                else if (i < 0x8000)
                    nodesSA.Add(node);
                else
                    nodesHSA.Add(node);
            }
            if (nodes.Count>0)
                refMarker.treeViewItem.Nodes.Add(new TreeNode("AOS - ML", nodes.ToArray()));
            if (nodesSA.Count>0)
                refMarker.treeViewItem.Nodes.Add(new TreeNode("Stygian Abyss", nodesSA.ToArray()));
            if (nodesHSA.Count>0)
                refMarker.treeViewItem.Nodes.Add(new TreeNode("Adventures High Seas", nodesHSA.ToArray()));
            refMarker.treeViewItem.EndUpdate();
            if (refMarker.treeViewItem.Nodes.Count > 0)
                if (refMarker.treeViewItem.Nodes[0].Nodes.Count > 0)
                    refMarker.treeViewItem.SelectedNode = refMarker.treeViewItem.Nodes[0].Nodes[0];
        }

        public static void ApplyFilterLand(LandData land)
        {
            refMarker.treeViewLand.BeginUpdate();
            refMarker.treeViewLand.Nodes.Clear();
            List<TreeNode> nodes = new List<TreeNode>();
            for (int i = 0; i < TileData.LandTable.Length; ++i)
            {
                if (!String.IsNullOrEmpty(land.Name))
                {
                    if (!TileData.ItemTable[i].Name.ToLower().Contains(land.Name.ToLower()))
                        continue;
                }
                if (land.TextureID != 0)
                {
                    if (TileData.LandTable[i].TextureID != land.TextureID)
                        continue;
                }
                if (land.Flags != 0)
                {
                    if ((TileData.LandTable[i].Flags & land.Flags) == 0)
                        continue;
                }
                TreeNode node = new TreeNode(String.Format("0x{0:X4} ({0}) {1}", i, TileData.LandTable[i].Name));
                node.Tag = i;
                nodes.Add(node);
            }
            refMarker.treeViewLand.Nodes.AddRange(nodes.ToArray());
            refMarker.treeViewLand.EndUpdate();
            if (refMarker.treeViewLand.Nodes.Count > 0)
                refMarker.treeViewLand.SelectedNode = refMarker.treeViewLand.Nodes[0];
        }

        private void Reload()
        {
            if (Loaded)
                OnLoad(this, new MyEventArgs(MyEventArgs.TYPES.FORCERELOAD));
        }
        public void OnLoad(object sender, EventArgs e)
        {
            MyEventArgs _args = e as MyEventArgs;
            if (Loaded && (_args == null || _args.Type != MyEventArgs.TYPES.FORCERELOAD))
                return;
            Cursor.Current = Cursors.WaitCursor;
            Options.LoadedUltimaClass["TileData"] = true;
            Options.LoadedUltimaClass["Art"] = true;
            treeViewItem.BeginUpdate();
            treeViewItem.Nodes.Clear();
            if (TileData.ItemTable != null)
            {
                TreeNode[] nodes = new TreeNode[0x4000];
                for (int i = 0; i < 0x4000; ++i)
                {
                    TreeNode node = new TreeNode(String.Format("0x{0:X4} ({0}) {1}", i, TileData.ItemTable[i].Name));
                    node.Tag = i;
                    nodes[i] = node;
                }
                treeViewItem.Nodes.Add(new TreeNode("AOS - ML", nodes));

                if (TileData.ItemTable.Length > 0x4000) // SA
                {
                    nodes = new TreeNode[0x4000];
                    for (int i = 0; i < 0x4000; ++i)
                    {
                        int j = i + 0x4000;
                        TreeNode node = new TreeNode(String.Format("0x{0:X4} ({0}) {1}", j, TileData.ItemTable[j].Name));
                        node.Tag = j;
                        nodes[i] = node;
                    }
                    treeViewItem.Nodes.Add(new TreeNode("Stygian Abyss", nodes));
                }

                if (TileData.ItemTable.Length > 0x8000) // AHS
                {
                    nodes = new TreeNode[0x8000];
                    for (int i = 0; i < 0x8000; ++i)
                    {
                        int j = i + 0x8000;
                        TreeNode node = new TreeNode(String.Format("0x{0:X4} ({0}) {1}", j, TileData.ItemTable[j].Name));
                        node.Tag = j;
                        nodes[i] = node;
                    }
                    treeViewItem.Nodes.Add(new TreeNode("Adventures High Seas", nodes));
                }
                else
                    treeViewItem.ExpandAll();
            }
            treeViewItem.EndUpdate();
            treeViewLand.BeginUpdate();
            treeViewLand.Nodes.Clear();
            if (TileData.LandTable != null)
            {
                TreeNode[] nodes = new TreeNode[TileData.LandTable.Length];
                for (int i = 0; i < TileData.LandTable.Length; ++i)
                {
                    TreeNode node = new TreeNode(String.Format("0x{0:X4} ({0}) {1}", i, TileData.LandTable[i].Name));
                    node.Tag = i;
                    nodes[i] = node;
                }
                treeViewLand.Nodes.AddRange(nodes);
            }
            treeViewLand.EndUpdate();
            if (!Loaded)
            {
                FiddlerControls.Events.FilePathChangeEvent += new FiddlerControls.Events.FilePathChangeHandler(OnFilePathChangeEvent);
                FiddlerControls.Events.TileDataChangeEvent += new FiddlerControls.Events.TileDataChangeHandler(OnTileDataChangeEvent);
            }
            Loaded = true;
            Cursor.Current = Cursors.Default;
        }

        private void OnFilePathChangeEvent()
        {
            Reload();
        }

        private void OnTileDataChangeEvent(object sender, int index)
        {
            if (!Loaded)
                return;
            if (sender.Equals(this))
                return;
            if (index > 0x3FFF) //items
            {
                if (treeViewItem.SelectedNode == null)
                    return;
                if ((int)treeViewItem.SelectedNode.Tag == index)
                {
                    treeViewItem.SelectedNode.ForeColor = Color.Red;
                    AfterSelectTreeViewItem(this, new TreeViewEventArgs(treeViewItem.SelectedNode));
                }
                else
                {
                    foreach (TreeNode parentnode in treeViewItem.Nodes)
                    {
                        foreach (TreeNode node in parentnode.Nodes)
                        {
                            if ((int)node.Tag == index)
                            {
                                node.ForeColor = Color.Red;
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                if (treeViewLand.SelectedNode == null)
                    return;
                if ((int)treeViewLand.SelectedNode.Tag == index)
                {
                    treeViewLand.SelectedNode.ForeColor = Color.Red;
                    AfterSelectTreeViewLand(this, new TreeViewEventArgs(treeViewLand.SelectedNode));
                }
                else
                {
                    foreach (TreeNode node in treeViewLand.Nodes)
                    {
                        if ((int)node.Tag == index)
                        {
                            node.ForeColor = Color.Red;
                            break;
                        }
                    }
                }
            }
        }

        private void AfterSelectTreeViewItem(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null || e.Node.Tag == null)
                return;
            int index = (int)e.Node.Tag;
            try
            {
                Bitmap bit = Ultima.Art.GetStatic(index);
                Bitmap newbit = new Bitmap(pictureBoxItem.Size.Width, pictureBoxItem.Size.Height);
                Graphics newgraph = Graphics.FromImage(newbit);
                newgraph.Clear(Color.FromArgb(-1));
                newgraph.DrawImage(bit, (pictureBoxItem.Size.Width - bit.Width) / 2, 1);
                pictureBoxItem.Image = newbit;
            }
            catch
            {
                pictureBoxItem.Image = new Bitmap(pictureBoxItem.Width, pictureBoxItem.Height);
            }
            ItemData data = TileData.ItemTable[index];
            ChangingIndex = true;
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
            Array EnumValues = System.Enum.GetValues(typeof(TileFlag));
            for (int i = 1; i < EnumValues.Length; ++i)
            {
                if ((data.Flags & (TileFlag)EnumValues.GetValue(i)) != 0)
                    checkedListBox1.SetItemChecked(i - 1, true);
                else
                    checkedListBox1.SetItemChecked(i - 1, false);
            }
            ChangingIndex = false;
        }

        private void AfterSelectTreeViewLand(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null)
                return;
            int index = (int)e.Node.Tag;
            try
            {
                Bitmap bit = Ultima.Art.GetLand(index);
                Bitmap newbit = new Bitmap(pictureBoxLand.Size.Width, pictureBoxLand.Size.Height);
                Graphics newgraph = Graphics.FromImage(newbit);
                newgraph.Clear(Color.FromArgb(-1));
                newgraph.DrawImage(bit, (pictureBoxLand.Size.Width - bit.Width) / 2, 1);
                pictureBoxLand.Image = newbit;
            }
            catch
            {
                pictureBoxLand.Image = new Bitmap(pictureBoxLand.Width, pictureBoxLand.Height);
            }
            LandData data = TileData.LandTable[index];
            ChangingIndex = true;
            textBoxNameLand.Text = data.Name;
            textBoxTexID.Text = data.TextureID.ToString();
            textBoxUnkLandHSA.Text = data.Unk1.ToString();
            if ((data.Flags & TileFlag.Damaging) != 0)
                checkedListBox2.SetItemChecked(0, true);
            else
                checkedListBox2.SetItemChecked(0, false);
            if ((data.Flags & TileFlag.Wet) != 0)
                checkedListBox2.SetItemChecked(1, true);
            else
                checkedListBox2.SetItemChecked(1, false);
            if ((data.Flags & TileFlag.Impassable) != 0)
                checkedListBox2.SetItemChecked(2, true);
            else
                checkedListBox2.SetItemChecked(2, false);
            if ((data.Flags & TileFlag.Wall) != 0)
                checkedListBox2.SetItemChecked(3, true);
            else
                checkedListBox2.SetItemChecked(3, false);
            if ((data.Flags & TileFlag.Unknown3) != 0)
                checkedListBox2.SetItemChecked(4, true);
            else
                checkedListBox2.SetItemChecked(4, false);
            ChangingIndex = false;
        }

        private void OnClickSaveTiledata(object sender, EventArgs e)
        {
            string path = FiddlerControls.Options.OutputPath;
            string FileName = Path.Combine(path, "tiledata.mul");
            Ultima.TileData.SaveTileData(FileName);
            MessageBox.Show(
                String.Format("TileData saved to {0}", FileName),
                "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
            Options.ChangedUltimaClass["TileData"] = false;
        }

        private void OnClickSaveChanges(object sender, EventArgs e)
        {
            if (tabcontrol.SelectedIndex == 0) //items
            {
                if (treeViewItem.SelectedNode == null || treeViewItem.SelectedNode.Tag == null)
                    return;
                int index = (int)treeViewItem.SelectedNode.Tag;
                ItemData item = TileData.ItemTable[index];
                string name = textBoxName.Text;
                if (name.Length > 20)
                    name = name.Substring(0, 20);
                item.Name = name;
                treeViewItem.SelectedNode.Text = String.Format("0x{0:X4} ({0}) {1}", index, name);
                byte byteres;
                short shortres;
                int intres;
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
                if (int.TryParse(textBoxUnk1HSA.Text, out intres))
                    item.Unk1 = intres;
                item.Flags = TileFlag.None;
                Array EnumValues = System.Enum.GetValues(typeof(TileFlag));
                for (int i = 0; i < checkedListBox1.Items.Count; ++i)
                {
                    if (checkedListBox1.GetItemChecked(i))
                        item.Flags |= (TileFlag)EnumValues.GetValue(i + 1);
                }
                TileData.ItemTable[index] = item;
                treeViewItem.SelectedNode.ForeColor = Color.Red;
                Options.ChangedUltimaClass["TileData"] = true;
                FiddlerControls.Events.FireTileDataChangeEvent(this, index + 0x4000);
                if (memorySaveWarningToolStripMenuItem.Checked)
                    MessageBox.Show(
                        String.Format("Edits of 0x{0:X4} ({0}) saved to memory. Click 'Save Tiledata' to write to file.", index), "Saved",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information,
                        MessageBoxDefaultButton.Button1);
            }
            else //land
            {
                if (treeViewLand.SelectedNode == null)
                    return;
                int index = (int)treeViewLand.SelectedNode.Tag;
                LandData land = TileData.LandTable[index];
                string name = textBoxNameLand.Text;
                if (name.Length > 20)
                    name = name.Substring(0, 20);
                land.Name = name;
                treeViewLand.SelectedNode.Text = String.Format("0x{0:X4} {1}", index, name);
                short shortres;
                if (short.TryParse(textBoxTexID.Text, out shortres))
                    land.TextureID = shortres;
                int intres;
                if (int.TryParse(textBoxUnkLandHSA.Text, out intres))
                    land.Unk1 = intres;
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

                TileData.LandTable[index] = land;
                Options.ChangedUltimaClass["TileData"] = true;
                FiddlerControls.Events.FireTileDataChangeEvent(this, index);
                treeViewLand.SelectedNode.ForeColor = Color.Red;
                if (memorySaveWarningToolStripMenuItem.Checked)
                    MessageBox.Show(
                        String.Format("Edits of 0x{0:X4} ({0}) saved to memory. Click 'Save Tiledata' to write to file.", index), "Saved",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information,
                        MessageBoxDefaultButton.Button1);
            }
        }

        #region SaveDirectEvents
        private void OnFlagItemCheckItems(object sender, ItemCheckEventArgs e)
        {
            if (saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                if (ChangingIndex)
                    return;
                if (e.CurrentValue != e.NewValue)
                {
                    if (treeViewItem.SelectedNode == null || treeViewItem.SelectedNode.Tag == null)
                        return;
                    int index = (int)treeViewItem.SelectedNode.Tag;
                    ItemData item = TileData.ItemTable[index];
                    Array EnumValues = System.Enum.GetValues(typeof(TileFlag));
                    TileFlag changeflag = (TileFlag)EnumValues.GetValue(e.Index + 1);
                    if ((item.Flags & changeflag) != 0) //better doublecheck
                    {
                        if (e.NewValue == CheckState.Unchecked)
                        {
                            item.Flags ^= changeflag;
                            TileData.ItemTable[index] = item;
                            treeViewItem.SelectedNode.ForeColor = Color.Red;
                            Options.ChangedUltimaClass["TileData"] = true;
                            FiddlerControls.Events.FireTileDataChangeEvent(this, index + 0x4000);
                        }
                    }
                    else if ((item.Flags & changeflag) == 0)
                    {
                        if (e.NewValue == CheckState.Checked)
                        {
                            item.Flags |= changeflag;
                            TileData.ItemTable[index] = item;
                            treeViewItem.SelectedNode.ForeColor = Color.Red;
                            Options.ChangedUltimaClass["TileData"] = true;
                            FiddlerControls.Events.FireTileDataChangeEvent(this, index + 0x4000);
                        }
                    }

                }
            }
        }

        private void OnTextChangedItemAnim(object sender, EventArgs e)
        {
            if (saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                if (ChangingIndex)
                    return;
                if (treeViewItem.SelectedNode == null || treeViewItem.SelectedNode.Tag == null)
                    return;
                int index = (int)treeViewItem.SelectedNode.Tag;
                ItemData item = TileData.ItemTable[index];
                short shortres;
                if (short.TryParse(textBoxAnim.Text, out shortres))
                {
                    item.Animation = shortres;
                    TileData.ItemTable[index] = item;
                    treeViewItem.SelectedNode.ForeColor = Color.Red;
                    Options.ChangedUltimaClass["TileData"] = true;
                    FiddlerControls.Events.FireTileDataChangeEvent(this, index + 0x4000);
                }
            }
        }

        private void OnTextChangedItemName(object sender, EventArgs e)
        {
            if (saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                if (ChangingIndex)
                    return;
                if (treeViewItem.SelectedNode == null || treeViewItem.SelectedNode.Tag == null)
                    return;
                int index = (int)treeViewItem.SelectedNode.Tag;
                ItemData item = TileData.ItemTable[index];
                string name = textBoxName.Text;
                if (name.Length == 0)
                    return;
                if (name.Length > 20)
                    name = name.Substring(0, 20);
                item.Name = name;
                treeViewItem.SelectedNode.Text = String.Format("0x{0:X4} ({0}) {1}", index, name);
                TileData.ItemTable[index] = item;
                treeViewItem.SelectedNode.ForeColor = Color.Red;
                Options.ChangedUltimaClass["TileData"] = true;
                FiddlerControls.Events.FireTileDataChangeEvent(this, index + 0x4000);
            }
        }

        private void OnTextChangedItemWeight(object sender, EventArgs e)
        {
            if (saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                if (ChangingIndex)
                    return;
                if (treeViewItem.SelectedNode == null || treeViewItem.SelectedNode.Tag == null)
                    return;
                int index = (int)treeViewItem.SelectedNode.Tag;
                ItemData item = TileData.ItemTable[index];
                byte byteres;
                if (byte.TryParse(textBoxWeight.Text, out byteres))
                {
                    item.Weight = byteres;
                    TileData.ItemTable[index] = item;
                    treeViewItem.SelectedNode.ForeColor = Color.Red;
                    Options.ChangedUltimaClass["TileData"] = true;
                    FiddlerControls.Events.FireTileDataChangeEvent(this, index + 0x4000);
                }
            }
        }

        private void OnTextChangedItemQuality(object sender, EventArgs e)
        {
            if (saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                if (ChangingIndex)
                    return;
                if (treeViewItem.SelectedNode == null || treeViewItem.SelectedNode.Tag == null)
                    return;
                int index = (int)treeViewItem.SelectedNode.Tag;
                ItemData item = TileData.ItemTable[index];
                byte byteres;
                if (byte.TryParse(textBoxQuality.Text, out byteres))
                {
                    item.Quality = byteres;
                    TileData.ItemTable[index] = item;
                    treeViewItem.SelectedNode.ForeColor = Color.Red;
                    Options.ChangedUltimaClass["TileData"] = true;
                    FiddlerControls.Events.FireTileDataChangeEvent(this, index + 0x4000);
                }
            }
        }

        private void OnTextChangedItemQuantity(object sender, EventArgs e)
        {
            if (saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                if (ChangingIndex)
                    return;
                if (treeViewItem.SelectedNode == null || treeViewItem.SelectedNode.Tag == null)
                    return;
                int index = (int)treeViewItem.SelectedNode.Tag;
                ItemData item = TileData.ItemTable[index];
                byte byteres;
                if (byte.TryParse(textBoxQuantity.Text, out byteres))
                {
                    item.Quantity = byteres;
                    TileData.ItemTable[index] = item;
                    treeViewItem.SelectedNode.ForeColor = Color.Red;
                    Options.ChangedUltimaClass["TileData"] = true;
                    FiddlerControls.Events.FireTileDataChangeEvent(this, index + 0x4000);
                }
            }
        }

        private void OnTextChangedItemHue(object sender, EventArgs e)
        {
            if (saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                if (ChangingIndex)
                    return;
                if (treeViewItem.SelectedNode == null || treeViewItem.SelectedNode.Tag == null)
                    return;
                int index = (int)treeViewItem.SelectedNode.Tag;
                ItemData item = TileData.ItemTable[index];
                byte byteres;
                if (byte.TryParse(textBoxHue.Text, out byteres))
                {
                    item.Hue = byteres;
                    TileData.ItemTable[index] = item;
                    treeViewItem.SelectedNode.ForeColor = Color.Red;
                    Options.ChangedUltimaClass["TileData"] = true;
                    FiddlerControls.Events.FireTileDataChangeEvent(this, index + 0x4000);
                }
            }
        }

        private void OnTextChangedItemStackOff(object sender, EventArgs e)
        {
            if (saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                if (ChangingIndex)
                    return;
                if (treeViewItem.SelectedNode == null || treeViewItem.SelectedNode.Tag == null)
                    return;
                int index = (int)treeViewItem.SelectedNode.Tag;
                ItemData item = TileData.ItemTable[index];
                byte byteres;
                if (byte.TryParse(textBoxStackOff.Text, out byteres))
                {
                    item.StackingOffset = byteres;
                    TileData.ItemTable[index] = item;
                    treeViewItem.SelectedNode.ForeColor = Color.Red;
                    Options.ChangedUltimaClass["TileData"] = true;
                    FiddlerControls.Events.FireTileDataChangeEvent(this, index + 0x4000);
                }
            }
        }

        private void OnTextChangedItemValue(object sender, EventArgs e)
        {
            if (saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                if (ChangingIndex)
                    return;
                if (treeViewItem.SelectedNode == null || treeViewItem.SelectedNode.Tag == null)
                    return;
                int index = (int)treeViewItem.SelectedNode.Tag;
                ItemData item = TileData.ItemTable[index];
                byte byteres;
                if (byte.TryParse(textBoxValue.Text, out byteres))
                {
                    item.Value = byteres;
                    TileData.ItemTable[index] = item;
                    treeViewItem.SelectedNode.ForeColor = Color.Red;
                    Options.ChangedUltimaClass["TileData"] = true;
                    FiddlerControls.Events.FireTileDataChangeEvent(this, index + 0x4000);
                }
            }
        }

        private void OnTextChangedItemHeight(object sender, EventArgs e)
        {
            if (saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                if (ChangingIndex)
                    return;
                if (treeViewItem.SelectedNode == null || treeViewItem.SelectedNode.Tag == null)
                    return;
                int index = (int)treeViewItem.SelectedNode.Tag;
                ItemData item = TileData.ItemTable[index];
                byte byteres;
                if (byte.TryParse(textBoxHeigth.Text, out byteres))
                {
                    item.Height = byteres;
                    TileData.ItemTable[index] = item;
                    treeViewItem.SelectedNode.ForeColor = Color.Red;
                    Options.ChangedUltimaClass["TileData"] = true;
                    FiddlerControls.Events.FireTileDataChangeEvent(this, index + 0x4000);
                }
            }
        }

        private void OnTextChangedItemMiscData(object sender, EventArgs e)
        {
            if (saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                if (ChangingIndex)
                    return;
                if (treeViewItem.SelectedNode == null || treeViewItem.SelectedNode.Tag == null)
                    return;
                int index = (int)treeViewItem.SelectedNode.Tag;
                ItemData item = TileData.ItemTable[index];
                short shortres;
                if (short.TryParse(textBoxUnk1.Text, out shortres))
                {
                    item.MiscData = shortres;
                    TileData.ItemTable[index] = item;
                    treeViewItem.SelectedNode.ForeColor = Color.Red;
                    Options.ChangedUltimaClass["TileData"] = true;
                    FiddlerControls.Events.FireTileDataChangeEvent(this, index + 0x4000);
                }
            }
        }

        private void OnTextChangedItemUnk2(object sender, EventArgs e)
        {
            if (saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                if (ChangingIndex)
                    return;
                if (treeViewItem.SelectedNode == null || treeViewItem.SelectedNode.Tag == null)
                    return;
                int index = (int)treeViewItem.SelectedNode.Tag;
                ItemData item = TileData.ItemTable[index];
                byte byteres;
                if (byte.TryParse(textBoxUnk2.Text, out byteres))
                {
                    item.Unk2 = byteres;
                    TileData.ItemTable[index] = item;
                    treeViewItem.SelectedNode.ForeColor = Color.Red;
                    Options.ChangedUltimaClass["TileData"] = true;
                    FiddlerControls.Events.FireTileDataChangeEvent(this, index + 0x4000);
                }
            }
        }

        private void OnTextChangedItemUnk3(object sender, EventArgs e)
        {
            if (saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                if (ChangingIndex)
                    return;
                if (treeViewItem.SelectedNode == null || treeViewItem.SelectedNode.Tag == null)
                    return;
                int index = (int)treeViewItem.SelectedNode.Tag;
                ItemData item = TileData.ItemTable[index];
                byte byteres;
                if (byte.TryParse(textBoxUnk3.Text, out byteres))
                {
                    item.Unk3 = byteres;
                    TileData.ItemTable[index] = item;
                    treeViewItem.SelectedNode.ForeColor = Color.Red;
                    Options.ChangedUltimaClass["TileData"] = true;
                    FiddlerControls.Events.FireTileDataChangeEvent(this, index + 0x4000);
                }
            }
        }

        private void OnTextChangedItemUnk1HSA(object sender, EventArgs e)
        {
            if (saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                if (ChangingIndex)
                    return;
                if (treeViewItem.SelectedNode == null || treeViewItem.SelectedNode.Tag == null)
                    return;
                int index = (int)treeViewItem.SelectedNode.Tag;
                ItemData item = TileData.ItemTable[index];
                int intres;
                if (int.TryParse(textBoxUnk1HSA.Text, out intres))
                {
                    item.Unk1 = intres;
                    TileData.ItemTable[index] = item;
                    treeViewItem.SelectedNode.ForeColor = Color.Red;
                    Options.ChangedUltimaClass["TileData"] = true;
                    FiddlerControls.Events.FireTileDataChangeEvent(this, index + 0x4000);
                }
            }
        }

        private void OnTextChangedLandName(object sender, EventArgs e)
        {
            if (saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                if (ChangingIndex)
                    return;
                if (treeViewLand.SelectedNode == null || treeViewLand.SelectedNode.Tag == null)
                    return;
                int index = (int)treeViewLand.SelectedNode.Tag;
                LandData land = TileData.LandTable[index];
                string name = textBoxNameLand.Text;
                if (name.Length == 0)
                    return;
                if (name.Length > 20)
                    name = name.Substring(0, 20);
                land.Name = name;
                treeViewLand.SelectedNode.Text = String.Format("0x{0:X4} ({0}) {1}", index, name);
                TileData.LandTable[index] = land;
                treeViewLand.SelectedNode.ForeColor = Color.Red;
                Options.ChangedUltimaClass["TileData"] = true;
                FiddlerControls.Events.FireTileDataChangeEvent(this, index);
            }
        }

        private void OnTextChangedLandTexID(object sender, EventArgs e)
        {
            if (saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                if (ChangingIndex)
                    return;
                if (treeViewLand.SelectedNode == null)
                    return;
                int index = (int)treeViewLand.SelectedNode.Tag;
                LandData land = TileData.LandTable[index];
                short shortres;
                if (short.TryParse(textBoxTexID.Text, out shortres))
                {
                    land.TextureID = shortres;
                    TileData.LandTable[index] = land;
                    treeViewLand.SelectedNode.ForeColor = Color.Red;
                    Options.ChangedUltimaClass["TileData"] = true;
                    FiddlerControls.Events.FireTileDataChangeEvent(this, index);
                }
            }
        }

        private void OnTextChangedLandUnkHSA(object sender, EventArgs e)
        {
            if (saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                if (ChangingIndex)
                    return;
                if (treeViewLand.SelectedNode == null)
                    return;
                int index = (int)treeViewLand.SelectedNode.Tag;
                LandData land = TileData.LandTable[index];
                int intres;
                if (int.TryParse(textBoxUnkLandHSA.Text, out intres))
                {
                    land.Unk1 = intres;
                    TileData.LandTable[index] = land;
                    treeViewLand.SelectedNode.ForeColor = Color.Red;
                    Options.ChangedUltimaClass["TileData"] = true;
                    FiddlerControls.Events.FireTileDataChangeEvent(this, index);
                }
            }
        }

        private void OnFlagItemCheckLandtiles(object sender, ItemCheckEventArgs e)
        {
            if (saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                if (ChangingIndex)
                    return;
                if (e.CurrentValue != e.NewValue)
                {
                    if (treeViewLand.SelectedNode == null)
                        return;
                    int index = (int)treeViewLand.SelectedNode.Tag;
                    LandData land = TileData.LandTable[index];
                    TileFlag changeflag;
                    switch (e.Index)
                    {
                        case 0: changeflag = TileFlag.Damaging;
                            break;
                        case 1: changeflag = TileFlag.Wet;
                            break;
                        case 2: changeflag = TileFlag.Impassable;
                            break;
                        case 3: changeflag = TileFlag.Wall;
                            break;
                        case 4: changeflag = TileFlag.Unknown3;
                            break;
                        default: changeflag = TileFlag.None;
                            break;
                    }

                    if ((land.Flags & changeflag) != 0)
                    {
                        if (e.NewValue == CheckState.Unchecked)
                        {
                            land.Flags ^= changeflag;
                            TileData.LandTable[index] = land;
                            treeViewLand.SelectedNode.ForeColor = Color.Red;
                            Options.ChangedUltimaClass["TileData"] = true;
                            FiddlerControls.Events.FireTileDataChangeEvent(this, index);
                        }
                    }
                    else if ((land.Flags & changeflag) == 0)
                    {
                        if (e.NewValue == CheckState.Checked)
                        {
                            land.Flags |= changeflag;
                            TileData.LandTable[index] = land;
                            treeViewLand.SelectedNode.ForeColor = Color.Red;
                            Options.ChangedUltimaClass["TileData"] = true;
                            FiddlerControls.Events.FireTileDataChangeEvent(this, index);
                        }
                    }
                }
            }
        }
        #endregion


        private void OnClickExport(object sender, EventArgs e)
        {
            string path = FiddlerControls.Options.OutputPath;
            if (tabcontrol.SelectedIndex == 0) //items
            {
                string FileName = Path.Combine(path, "ItemData.csv");
                Ultima.TileData.ExportItemDataToCSV(FileName);
                MessageBox.Show(String.Format("ItemData saved to {0}", FileName), "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
            else
            {
                string FileName = Path.Combine(path, "LandData.csv");
                Ultima.TileData.ExportLandDataToCSV(FileName);
                MessageBox.Show(String.Format("LandData saved to {0}", FileName), "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
        }

        private TileDatasSearch showform1 = null;
        private TileDatasSearch showform2 = null;
        private void OnClickSearch(object sender, EventArgs e)
        {
            if (tabcontrol.SelectedIndex == 0) //items
            {
                if ((showform1 == null) || (showform1.IsDisposed))
                {
                    showform1 = new TileDatasSearch(false);
                    showform1.TopMost = true;
                    showform1.Show();
                }
            }
            else //landtiles
            {
                if ((showform2 == null) || (showform2.IsDisposed))
                {
                    showform2 = new TileDatasSearch(true);
                    showform2.TopMost = true;
                    showform2.Show();
                }
            }
        }

        private void OnClickSelectItem(object sender, EventArgs e)
        {
            if (treeViewItem.SelectedNode == null)
                return;
            int index = (int)treeViewItem.SelectedNode.Tag;
            if (Options.DesignAlternative)
                FiddlerControls.ItemShowAlternative.SearchGraphic(index);
            else
                FiddlerControls.ItemShow.SearchGraphic(index);
        }

        private void OnClickSelectInLandtiles(object sender, EventArgs e)
        {
            if (treeViewLand.SelectedNode == null)
                return;
            int index = (int)treeViewLand.SelectedNode.Tag;
            if (Options.DesignAlternative)
                FiddlerControls.LandTilesAlternative.SearchGraphic(index);
            else
                FiddlerControls.LandTiles.SearchGraphic(index);
        }

        private void OnClickSelectRadarItem(object sender, EventArgs e)
        {
            if (treeViewItem.SelectedNode == null)
                return;
            int index = (int)treeViewItem.SelectedNode.Tag;
            FiddlerControls.RadarColor.Select(index, false);
        }

        private void OnClickSelectRadarLand(object sender, EventArgs e)
        {
            if (treeViewLand.SelectedNode == null)
                return;
            int index = (int)treeViewLand.SelectedNode.Tag;
            FiddlerControls.RadarColor.Select(index, true);
        }

        private void OnClickImport(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Title = "Choose csv file to import";
            dialog.CheckFileExists = true;
            dialog.Filter = "csv files (*.csv)|*.csv";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Options.ChangedUltimaClass["TileData"] = true;
                if (tabcontrol.SelectedIndex == 0)//items
                {
                    Ultima.TileData.ImportItemDataFromCSV(dialog.FileName);
                    AfterSelectTreeViewItem(this, new TreeViewEventArgs(treeViewItem.SelectedNode));
                }
                else
                {
                    Ultima.TileData.ImportLandDataFromCSV(dialog.FileName);
                    AfterSelectTreeViewLand(this, new TreeViewEventArgs(treeViewLand.SelectedNode));
                }
            }
            dialog.Dispose();
        }

        private TileDataFilter filterform = null;
        private void OnClickSetFilter(object sender, EventArgs e)
        {
            if ((filterform == null) || (filterform.IsDisposed))
            {
                filterform = new TileDataFilter();
                filterform.TopMost = true;
                filterform.Show();
            }
        }

        private void OnItemDataNodeExpanded(object sender, TreeViewCancelEventArgs e)
        {
            if(treeViewItem.Nodes.Count == 3) // workaround for 65536 items'microsoft bug
                treeViewItem.CollapseAll();
        }
    }
}
