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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using FiddlerControls.Helpers;
using Ultima;

namespace FiddlerControls
{
    public partial class ItemShow : UserControl
    {
        public ItemShow()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            RefMarker = this;
            if (!Files.CacheData)
            {
                PreloadItems.Visible = false;
            }

            ProgressBar.Visible = false;
            DetailPictureBox.Tag = -1;
            DetailTextBox.AddBasicContextMenu();
        }

        private bool _showFreeSlots;

        public static ItemShow RefMarker { get; private set; }
        public static ListView ItemListView => RefMarker.listView1;
        public bool IsLoaded { get; private set; }

        /// <summary>
        /// Updates if TileSize is changed
        /// </summary>
        public void ChangeTileSize()
        {
            listView1.TileSize = new Size(Options.ArtItemSizeWidth, Options.ArtItemSizeHeight);
            listView1.View = View.Details; // that works fascinating
            listView1.View = View.Tile;
        }

        private void MakeHashFile()
        {
            string path = Options.AppDataPath;
            string fileName = Path.Combine(path, "UOFiddlerArt.hash");
            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                using (BinaryWriter bin = new BinaryWriter(fs))
                {
                    byte[] md5 = Files.GetMd5(Files.GetFilePath("Art.mul"));
                    if (md5 == null)
                    {
                        return;
                    }

                    int length = md5.Length;
                    bin.Write(length);
                    bin.Write(md5);
                    foreach (ListViewItem item in listView1.Items)
                    {
                        bin.Write((int)item.Tag);
                    }
                }
            }
        }

        /// <summary>
        /// Searches Objtype and Select
        /// </summary>
        /// <param name="graphic"></param>
        /// <returns></returns>
        public static bool SearchGraphic(int graphic)
        {
            if (!RefMarker.IsLoaded)
            {
                RefMarker.OnLoad(RefMarker, EventArgs.Empty);
            }

            const int index = 0;
            for (int i = index; i < RefMarker.listView1.Items.Count; ++i)
            {
                ListViewItem item = RefMarker.listView1.Items[i];
                if ((int)item.Tag == graphic || (int)item.Tag == -1 && i == graphic)
                {
                    if (RefMarker.listView1.SelectedItems.Count == 1)
                    {
                        RefMarker.listView1.SelectedItems[0].Selected = false;
                    }

                    item.Selected = true;
                    item.Focused = true;
                    item.EnsureVisible();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Searches for name and selects
        /// </summary>
        /// <param name="name"></param>
        /// <param name="next">starting from current selected</param>
        /// <returns></returns>
        public static bool SearchName(string name, bool next)
        {
            int index = 0;
            if (next)
            {
                if (RefMarker.listView1.SelectedIndices.Count == 1)
                {
                    index = RefMarker.listView1.SelectedIndices[0] + 1;
                }

                if (index >= RefMarker.listView1.Items.Count)
                {
                    index = 0;
                }
            }

            Regex regex = new Regex(name, RegexOptions.IgnoreCase);
            for (int i = index; i < RefMarker.listView1.Items.Count; ++i)
            {
                ListViewItem item = RefMarker.listView1.Items[i];
                if ((int)item.Tag == -1)
                {
                    continue;
                }

                if (regex.IsMatch(TileData.ItemTable[(int)item.Tag].Name))
                {
                    if (RefMarker.listView1.SelectedItems.Count == 1)
                    {
                        RefMarker.listView1.SelectedItems[0].Selected = false;
                    }

                    item.Selected = true;
                    item.Focused = true;
                    item.EnsureVisible();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// ReLoads if loaded
        /// </summary>
        private void Reload()
        {
            if (IsLoaded)
            {
                OnLoad(this, new MyEventArgs(MyEventArgs.Types.ForceReload));
            }
        }

        public void OnLoad(object sender, EventArgs e)
        {
            MyEventArgs args = e as MyEventArgs;
            if (IsLoaded && (args == null || args.Type != MyEventArgs.Types.ForceReload))
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            Options.LoadedUltimaClass["TileData"] = true;
            Options.LoadedUltimaClass["Art"] = true;
            Options.LoadedUltimaClass["Animdata"] = true;
            Options.LoadedUltimaClass["Hues"] = true;
            if (!IsLoaded) // only once
            {
                PluginInterface.Events.FireModifyItemShowContextMenuEvent(contextMenuStrip1);
            }

            _showFreeSlots = false;
            showFreeSlotsToolStripMenuItem.Checked = false;
            listView1.BeginUpdate();
            listView1.Clear();
            List<ListViewItem> itemcache = new List<ListViewItem>();
            if (Files.UseHashFile && Files.CompareHashFile("Art", Options.AppDataPath) && !Art.Modified)
            {
                string path = Options.AppDataPath;
                string fileName = Path.Combine(path, "UOFiddlerArt.hash");
                if (File.Exists(fileName))
                {
                    using (FileStream bin = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        unsafe
                        {
                            byte[] buffer = new byte[bin.Length];
                            bin.Read(buffer, 0, (int)bin.Length);
                            fixed (byte* bf = buffer)
                            {
                                int* poffset = (int*)bf;
                                int offset = *poffset + 4;
                                int* dat = (int*)(bf + offset);
                                int i = offset;
                                while (i < buffer.Length)
                                {
                                    int j = *dat++;
                                    ListViewItem item = new ListViewItem(j.ToString(), 0)
                                    {
                                        Tag = j
                                    };
                                    itemcache.Add(item);
                                    i += 4;
                                }
                            }
                        }
                    }
                    listView1.Items.AddRange(itemcache.ToArray());
                }
            }
            else
            {
                int staticlength = Art.GetMaxItemId() + 1;
                for (int i = 0; i < staticlength; ++i)
                {
                    if (Art.IsValidStatic(i))
                    {
                        ListViewItem item = new ListViewItem(i.ToString(), 0)
                        {
                            Tag = i
                        };
                        itemcache.Add(item);
                    }
                }
                listView1.Items.AddRange(itemcache.ToArray());

                if (Files.UseHashFile)
                {
                    MakeHashFile();
                }
            }

            listView1.TileSize = new Size(Options.ArtItemSizeWidth, Options.ArtItemSizeHeight);
            listView1.EndUpdate();

            if (!IsLoaded)
            {
                FiddlerControls.Events.FilePathChangeEvent += OnFilePathChangeEvent;
                FiddlerControls.Events.ItemChangeEvent += OnItemChangeEvent;
                FiddlerControls.Events.TileDataChangeEvent += OnTileDataChangeEvent;
            }
            IsLoaded = true;

            Cursor.Current = Cursors.Default;
        }

        private void OnTileDataChangeEvent(object sender, int id)
        {
            if (Options.DesignAlternative)
            {
                return;
            }

            if (!IsLoaded)
            {
                return;
            }

            if (sender.Equals(this))
            {
                return;
            }

            if (id < 0x4000)
            {
                return;
            }

            id -= 0x4000;
            if (listView1.SelectedItems.Count == 1)
            {
                if ((int)listView1.SelectedItems[0].Tag == id)
                {
                    namelabel.Text = $"Name: {TileData.ItemTable[id].Name}";
                    UpdateDetail(id);
                }
            }
        }

        private void OnFilePathChangeEvent()
        {
            if (!Options.DesignAlternative)
            {
                Reload();
            }
        }

        private void OnItemChangeEvent(object sender, int index)
        {
            if (Options.DesignAlternative)
            {
                return;
            }

            if (!IsLoaded)
            {
                return;
            }

            if (sender.Equals(this))
            {
                return;
            }

            if (Art.IsValidStatic(index))
            {
                ListViewItem item = new ListViewItem(index.ToString(), 0)
                {
                    Tag = index
                };
                if (_showFreeSlots)
                {
                    listView1.Items[index] = item;
                    listView1.Invalidate();
                }
                else
                {
                    bool done = false;
                    foreach (ListViewItem i in listView1.Items)
                    {
                        if ((int)i.Tag > index)
                        {
                            listView1.Items.Insert(i.Index, item);
                            done = true;
                            break;
                        }
                        if ((int)i.Tag == index)
                        {
                            done = true;
                            break;
                        }
                    }
                    if (!done)
                    {
                        listView1.Items.Add(item);
                    }
                }
                listView1.View = View.Details; // that works fascinating
                listView1.View = View.Tile;
            }
            else
            {
                if (!_showFreeSlots)
                {
                    foreach (ListViewItem i in listView1.Items)
                    {
                        if ((int)i.Tag == index)
                        {
                            listView1.Items.RemoveAt(i.Index);
                            break;
                        }
                    }
                }
                else
                {
                    listView1.Items[index].Tag = -1;
                }

                listView1.Invalidate();
            }
        }

        private void ListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                int i = (int)listView1.SelectedItems[0].Tag;
                if (i == -1)
                {
                    namelabel.Text = "Name: FREE";
                    graphiclabel.Text = string.Format("Graphic: 0x{0:X4} ({0})", listView1.SelectedIndices[0]);
                    UpdateDetail(listView1.SelectedIndices[0]);
                    selectInGumpsTabToolStripMenuItem.Enabled = false;
                }
                else
                {
                    namelabel.Text = $"Name: {TileData.ItemTable[i].Name}";
                    graphiclabel.Text = string.Format("Graphic: 0x{0:X4} ({0})", i);
                    UpdateDetail(i);
                    selectInGumpsTabToolStripMenuItem.Enabled = TileData.ItemTable[i].Animation != 0;
                }
            }
        }

        private static readonly Brush BrushWhite = Brushes.White;
        private static readonly Brush BrushLightBlue = Brushes.LightBlue;
        private static readonly Brush BrushLightCoral = Brushes.LightCoral;
        private static readonly Brush BrushRed = Brushes.Red;
        private static readonly Pen PenGray = Pens.Gray;

        private void DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            int i = (int)e.Item.Tag;
            if (i == -1)
            {
                if (e.Item.Selected)
                {
                    e.Graphics.FillRectangle(BrushLightBlue, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
                }
                else
                {
                    e.Graphics.DrawRectangle(PenGray, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
                }

                e.Graphics.FillRectangle(BrushRed, e.Bounds.X + 5, e.Bounds.Y + 5, e.Bounds.Width - 10, e.Bounds.Height - 10);
                return;
            }

            Bitmap bmp = Art.GetStatic(i, out bool patched);

            if (bmp != null)
            {
                if (e.Item.Selected)
                {
                    e.Graphics.FillRectangle(BrushLightBlue, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
                }
                else if (patched)
                {
                    e.Graphics.FillRectangle(BrushLightCoral, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
                }
                else
                {
                    e.Graphics.FillRectangle(BrushWhite, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
                }

                if (Options.ArtItemClip)
                {
                    e.Graphics.DrawImage(bmp, e.Bounds.X + 1, e.Bounds.Y + 1,
                                         new Rectangle(0, 0, e.Bounds.Width - 1, e.Bounds.Height - 1),
                                         GraphicsUnit.Pixel);
                }
                else
                {
                    int width = bmp.Width;
                    int height = bmp.Height;
                    if (width > e.Bounds.Width)
                    {
                        width = e.Bounds.Width;
                        height = e.Bounds.Height * bmp.Height / bmp.Width;
                    }
                    if (height > e.Bounds.Height)
                    {
                        height = e.Bounds.Height;
                        width = e.Bounds.Width * bmp.Width / bmp.Height;
                    }
                    e.Graphics.DrawImage(bmp,
                                         new Rectangle(e.Bounds.X + 1, e.Bounds.Y + 1, width, height));
                }
                if (!e.Item.Selected)
                {
                    e.Graphics.DrawRectangle(PenGray, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
                }
            }
        }

        public void ListView_DoubleClicked(object sender, MouseEventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                ItemDetail f = (int)listView1.SelectedItems[0].Tag == -1
                    ? new ItemDetail(listView1.SelectedIndices[0])
                    : new ItemDetail((int)listView1.SelectedItems[0].Tag);
                f.TopMost = true;
                f.Show();
            }
        }

        private ItemSearch _showform;

        private void Search_Click(object sender, EventArgs e)
        {
            if (_showform?.IsDisposed != false)
            {
                _showform = new ItemSearch
                {
                    TopMost = true
                };
                _showform.Show();
            }
        }

        private void UpdateDetail(int id)
        {
            ItemData item = TileData.ItemTable[id];
            Bitmap bit = Art.GetStatic(id);
            splitContainer2.SplitterDistance = bit?.Size.Height + 10 ?? 10;

            int xMin = 0;
            int xMax = 0;
            int yMin = 0;
            int yMax = 0;

            if (bit != null)
            {
                Art.Measure(bit, out xMin, out yMin, out xMax, out yMax);
            }

            DetailPictureBox.Tag = id;
            DetailPictureBox.Invalidate();

            DetailTextBox.Clear();
            DetailTextBox.AppendText($"Name: {item.Name}\n");
            DetailTextBox.AppendText($"Graphic: 0x{id:X4}\n");
            DetailTextBox.AppendText($"Height/Capacity: {item.Height}\n");
            DetailTextBox.AppendText($"Weight: {item.Weight}\n");
            DetailTextBox.AppendText($"Animation: {item.Animation}\n");
            DetailTextBox.AppendText($"Quality/Layer/Light: {item.Quality}\n");
            DetailTextBox.AppendText($"Quantity: {item.Quantity}\n");
            DetailTextBox.AppendText($"Hue: {item.Hue}\n");
            DetailTextBox.AppendText($"StackingOffset/Unk4: {item.StackingOffset}\n");
            DetailTextBox.AppendText($"Flags: {item.Flags}\n");
            DetailTextBox.AppendText($"Graphic pixel size width, height: {bit?.Width ?? 0} {bit?.Height ?? 0} \n");
            DetailTextBox.AppendText($"Graphic pixel offset xMin, yMin, xMax, yMax: {xMin} {yMin} {xMax} {yMax}\n");

            if ((item.Flags & TileFlag.Animation) != 0)
            {
                Animdata.Data info = Animdata.GetAnimData(id);
                if (info != null)
                {
                    DetailTextBox.AppendText(
                        $"Animation FrameCount: {info.FrameCount} Interval: {info.FrameInterval}\n");
                }
            }
        }

        private void DetailPictureBox_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.White);
            int id = (int)DetailPictureBox.Tag;
            if (id >= 0)
            {
                Bitmap bit = Art.GetStatic(id);
                if (bit != null)
                {
                    e.Graphics.DrawImage(bit, (e.ClipRectangle.Width - bit.Width) / 2, 5);
                }
            }
        }

        private void DetailSplitContainer_SizeChange(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                int id = (int)DetailPictureBox.Tag;
                if (id >= 0)
                {
                    Bitmap bit = Art.GetStatic(id);
                    splitContainer2.SplitterDistance = bit == null ? 10 : bit.Size.Height + 10;

                    DetailPictureBox.Invalidate();
                }
            }
        }

        private void OnResize(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                listView1.SelectedItems[0].EnsureVisible();
                int i = (int)listView1.SelectedItems[0].Tag;
                if (i == -1)
                {
                    UpdateDetail(listView1.SelectedItems[0].Index);
                }
                else
                {
                    UpdateDetail(i);
                }
            }
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            DialogResult result =
                        MessageBox.Show("Are you sure? Will take a while",
                        "Save",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning,
                        MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes)
            {
                ProgressBar bar = new ProgressBar(Art.GetIdxLength(), "Save");
                Cursor.Current = Cursors.WaitCursor;
                Art.Save(Options.OutputPath);
                bar.Dispose();
                Cursor.Current = Cursors.Default;
                Options.ChangedUltimaClass["Art"] = false;
                MessageBox.Show($"Saved to {Options.OutputPath}",
                    "Save",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1);
            }
        }

        private void OnClickReplace(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                using (OpenFileDialog dialog = new OpenFileDialog())
                {
                    dialog.Multiselect = false;
                    dialog.Title = "Choose image file to replace";
                    dialog.CheckFileExists = true;
                    dialog.Filter = "Image files (*.tif;*.tiff;*.bmp)|*.tif;*.tiff;*.bmp";
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        Bitmap bmp = new Bitmap(dialog.FileName);
                        if (dialog.FileName.Contains(".bmp"))
                        {
                            bmp = Utils.ConvertBmp(bmp);
                        }

                        int id = (int)listView1.SelectedItems[0].Tag;
                        if (id == -1)
                        {
                            listView1.SelectedItems[0].Tag = id = listView1.SelectedItems[0].Index;
                        }

                        Art.ReplaceStatic(id, bmp);
                        FiddlerControls.Events.FireItemChangeEvent(this, id);
                        listView1.Invalidate();
                        UpdateDetail(id);
                        Options.ChangedUltimaClass["Art"] = true;
                    }
                }
            }
        }

        private void OnTextChanged_Insert(object sender, EventArgs e)
        {
            if (Utils.ConvertStringToInt(InsertText.Text, out int index, 0, Art.GetMaxItemId()))
            {
                InsertText.ForeColor = Art.IsValidStatic(index) ? Color.Red : Color.Black;
            }
            else
            {
                InsertText.ForeColor = Color.Red;
            }
        }

        private void OnKeyDown_Insert(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (Utils.ConvertStringToInt(InsertText.Text, out int index, 0, Art.GetMaxItemId()))
                {
                    if (Art.IsValidStatic(index))
                    {
                        return;
                    }

                    contextMenuStrip1.Close();
                    using (OpenFileDialog dialog = new OpenFileDialog())
                    {
                        dialog.Multiselect = false;
                        dialog.Title = $"Choose image file to insert at 0x{index:X}";
                        dialog.CheckFileExists = true;
                        dialog.Filter = "Image files (*.tif;*.tiff;*.bmp)|*.tif;*.tiff;*.bmp";
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            Bitmap bmp = new Bitmap(dialog.FileName);
                            if (dialog.FileName.Contains(".bmp"))
                            {
                                bmp = Utils.ConvertBmp(bmp);
                            }

                            Art.ReplaceStatic(index, bmp);
                            FiddlerControls.Events.FireItemChangeEvent(this, index);
                            ListViewItem item = new ListViewItem(index.ToString(), 0)
                            {
                                Tag = index
                            };
                            if (_showFreeSlots)
                            {
                                listView1.Items[index] = item;
                                listView1.Invalidate();
                            }
                            else
                            {
                                bool done = false;
                                foreach (ListViewItem i in listView1.Items)
                                {
                                    if ((int)i.Tag > index)
                                    {
                                        listView1.Items.Insert(i.Index, item);
                                        done = true;
                                        break;
                                    }
                                }
                                if (!done)
                                {
                                    listView1.Items.Add(item);
                                }
                            }
                            listView1.View = View.Details; // that works faszinating
                            listView1.View = View.Tile;
                            if (listView1.SelectedItems.Count == 1)
                            {
                                listView1.SelectedItems[0].Selected = false;
                            }

                            item.Selected = true;
                            item.Focused = true;
                            item.EnsureVisible();
                            Options.ChangedUltimaClass["Art"] = true;
                        }
                    }
                }
            }
        }

        private void OnClickRemove(object sender, EventArgs e)
        {
            int i = (int)listView1.SelectedItems[0].Tag;
            if (i == -1)
            {
                return;
            }

            DialogResult result =
                        MessageBox.Show($"Are you sure to remove 0x{i:X}",
                        "Save",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes)
            {
                Art.RemoveStatic(i);
                FiddlerControls.Events.FireItemChangeEvent(this, i);
                i = listView1.SelectedItems[0].Index;
                if (!_showFreeSlots)
                {
                    listView1.SelectedItems[0].Selected = false;
                    listView1.Items.RemoveAt(i);
                }
                else
                {
                    listView1.Items[i].Tag = -1;
                }

                listView1.Invalidate();
                Options.ChangedUltimaClass["Art"] = true;
            }
        }

        private void OnClickFindFree(object sender, EventArgs e)
        {
            if (_showFreeSlots)
            {
                int i = listView1.SelectedItems.Count > 0 ? listView1.SelectedItems[0].Index + 1 : 0;
                for (; i < listView1.Items.Count; ++i)
                {
                    if ((int)listView1.Items[i].Tag == -1)
                    {
                        ListViewItem item = listView1.Items[i];
                        if (listView1.SelectedItems.Count == 1)
                        {
                            listView1.SelectedItems[0].Selected = false;
                        }

                        item.Selected = true;
                        item.Focused = true;
                        item.EnsureVisible();
                        break;
                    }
                }
            }
            else
            {
                int id, i;
                if (listView1.SelectedItems.Count > 0)
                {
                    id = (int)listView1.SelectedItems[0].Tag + 1;
                    i = listView1.SelectedItems[0].Index + 1;
                }
                else
                {
                    id = 0;
                    i = 0;
                }
                for (; i < listView1.Items.Count; ++i, ++id)
                {
                    if (id < (int)listView1.Items[i].Tag)
                    {
                        ListViewItem item = listView1.Items[i];
                        if (listView1.SelectedItems.Count == 1)
                        {
                            listView1.SelectedItems[0].Selected = false;
                        }

                        item.Selected = true;
                        item.Focused = true;
                        item.EnsureVisible();
                        break;
                    }
                }
            }
        }

        private void OnClickShowFreeSlots(object sender, EventArgs e)
        {
            SuspendLayout();
            listView1.View = View.Details; // that works fascinating
            listView1.BeginUpdate();
            _showFreeSlots = !_showFreeSlots;
            if (_showFreeSlots)
            {
                ListViewItem item;
                for (int j = 0; j < Art.GetMaxItemId() + 1; ++j)
                {
                    if (listView1.Items.Count > j)
                    {
                        if ((int)listView1.Items[j].Tag != j)
                        {
                            item = new ListViewItem(j.ToString(), 0)
                            {
                                Tag = -1
                            };
                            listView1.Items.Insert(j, item);
                        }
                    }
                    else
                    {
                        item = new ListViewItem(j.ToString(), 0)
                        {
                            Tag = -1
                        };
                        listView1.Items.Insert(j, item);
                    }
                }
            }
            else
            {
                Reload();
            }
            listView1.EndUpdate();
            ResumeLayout(false);

            listView1.View = View.Tile;
        }

        private void Extract_Image_ClickBmp(object sender, EventArgs e)
        {
            int i = (int)listView1.SelectedItems[0].Tag;
            if (i == -1)
            {
                return;
            }

            if (!Art.IsValidStatic(i))
            {
                return;
            }

            string path = Options.OutputPath;
            string fileName = Path.Combine(path, $"Item 0x{i:X}.bmp");
            Bitmap bit = new Bitmap(Art.GetStatic(i));
            bit?.Save(fileName, ImageFormat.Bmp);
            bit.Dispose();
            MessageBox.Show(
                $"Item saved to {fileName}",
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void Extract_Image_ClickTiff(object sender, EventArgs e)
        {
            int i = (int)listView1.SelectedItems[0].Tag;
            if (i == -1)
            {
                return;
            }

            if (!Art.IsValidStatic(i))
            {
                return;
            }

            string path = Options.OutputPath;
            string fileName = Path.Combine(path, $"Item 0x{i:X}.tiff");
            Bitmap bit = new Bitmap(Art.GetStatic(i));
            bit?.Save(fileName, ImageFormat.Tiff);
            bit.Dispose();
            MessageBox.Show(
                $"Item saved to {fileName}",
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void Extract_Image_ClickJpg(object sender, EventArgs e)
        {
            int i = (int)listView1.SelectedItems[0].Tag;
            if (i == -1)
            {
                return;
            }

            if (!Art.IsValidStatic(i))
            {
                return;
            }

            string path = Options.OutputPath;
            string fileName = Path.Combine(path, $"Item 0x{i:X}.jpg");
            Bitmap bit = new Bitmap(Art.GetStatic(i));
            bit?.Save(fileName, ImageFormat.Jpeg);
            bit.Dispose();
            MessageBox.Show(
                $"Item saved to {fileName}",
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void OnClickSelectTiledata(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                int id = (int)listView1.SelectedItems[0].Tag;
                if (id == -1)
                {
                    id = listView1.SelectedItems[0].Index;
                }

                TileDatas.Select(id, false);
            }
        }

        private void OnClickSelectRadarCol(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                int id = (int)listView1.SelectedItems[0].Tag;
                if (id == -1)
                {
                    id = listView1.SelectedItems[0].Index;
                }

                RadarColor.Select(id, false);
            }
        }

        private void OnClickSelectGump(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                int id = (int)listView1.SelectedItems[0].Tag;
                if (id == -1)
                {
                    id = listView1.SelectedItems[0].Index;
                }

                ItemData item = TileData.ItemTable[id];
                Gump.Select(item.Animation + 50000);
            }
        }

        private void OnClick_SaveAllBmp(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    Cursor.Current = Cursors.WaitCursor;
                    ProgressBar bar = new ProgressBar(listView1.Items.Count, "Export to bmp", false);
                    for (int i = 0; i < listView1.Items.Count; ++i)
                    {
                        FiddlerControls.Events.FireProgressChangeEvent();
                        Application.DoEvents();
                        int index = (int)listView1.Items[i].Tag;
                        if (index >= 0)
                        {
                            string fileName = Path.Combine(dialog.SelectedPath, $"Item 0x{index:X}.bmp");
                            Bitmap bit = new Bitmap(Art.GetStatic(index));
                            bit?.Save(fileName, ImageFormat.Bmp);
                            bit.Dispose();
                        }
                    }
                    bar.Dispose();
                    Cursor.Current = Cursors.Default;
                    MessageBox.Show($"All Item saved to {dialog.SelectedPath}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                }
            }
        }

        private void OnClick_SaveAllTiff(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    Cursor.Current = Cursors.WaitCursor;
                    ProgressBar bar = new ProgressBar(listView1.Items.Count, "Export to tiff", false);
                    for (int i = 0; i < listView1.Items.Count; ++i)
                    {
                        FiddlerControls.Events.FireProgressChangeEvent();
                        Application.DoEvents();
                        int index = (int)listView1.Items[i].Tag;
                        if (index >= 0)
                        {
                            string fileName = Path.Combine(dialog.SelectedPath, $"Item 0x{index:X}.tiff");
                            Bitmap bit = new Bitmap(Art.GetStatic(index));
                            bit?.Save(fileName, ImageFormat.Tiff);
                            bit.Dispose();
                        }
                    }
                    bar.Dispose();
                    Cursor.Current = Cursors.Default;
                    MessageBox.Show($"All Item saved to {dialog.SelectedPath}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                }
            }
        }

        private void OnClick_SaveAllJpg(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    Cursor.Current = Cursors.WaitCursor;
                    ProgressBar bar = new ProgressBar(listView1.Items.Count, "Export to jpeg", false);
                    for (int i = 0; i < listView1.Items.Count; ++i)
                    {
                        FiddlerControls.Events.FireProgressChangeEvent();
                        Application.DoEvents();
                        int index = (int)listView1.Items[i].Tag;
                        if (index >= 0)
                        {
                            string fileName = Path.Combine(dialog.SelectedPath, $"Item 0x{index:X}.jpg");
                            Bitmap bit = new Bitmap(Art.GetStatic(index));
                            bit?.Save(fileName, ImageFormat.Jpeg);
                            bit.Dispose();
                        }
                    }
                    bar.Dispose();
                    Cursor.Current = Cursors.Default;
                    MessageBox.Show($"All Item saved to {dialog.SelectedPath}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                }
            }
        }

        private void OnClickPreload(object sender, EventArgs e)
        {
            if (PreLoader.IsBusy)
            {
                return;
            }

            ProgressBar.Minimum = 1;
            ProgressBar.Maximum = listView1.Items.Count;
            ProgressBar.Step = 1;
            ProgressBar.Value = 1;
            ProgressBar.Visible = true;
            PreLoader.RunWorkerAsync(ProgressBar);
        }

        private void PreLoaderDoWork(object sender, DoWorkEventArgs e)
        {
            foreach (ListViewItem item in listView1.Items)
            {
                Art.GetStatic((int)item.Tag);
            }
        }

        private void PreLoaderProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressBar.PerformStep();
        }

        private void PreLoaderCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ProgressBar.Visible = false;
        }

        private void ItemShow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F && e.Control)
            {
                Search_Click(sender, e);
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }
    }
}
