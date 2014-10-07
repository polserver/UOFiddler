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
using Ultima;

namespace FiddlerControls
{
    public partial class ItemShowAlternative : UserControl
    {
        public ItemShowAlternative()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            refMarker = this;
            pictureBox.MouseWheel += new MouseEventHandler(OnMouseWheel);
        }

        private static ItemShowAlternative refMarker = null;
        private List<int> ItemList = new List<int>();
        private int col;
        private int row;
        private int selected = -1;
        private bool Loaded = false;
        private bool ShowFreeSlots = false;

        public static ItemShowAlternative RefMarker { get { return refMarker; } }
        public static PictureBox ItemPictureBox { get { return refMarker.pictureBox; } }
        public bool isLoaded { get { return Loaded; } }

        public int Selected
        {
            get { return selected; }
            set
            {
                selected = value;
                if (!Art.IsValidStatic(value))
                    namelabel.Text = "Name: FREE";
                else
                    namelabel.Text = String.Format("Name: {0}", TileData.ItemTable[value].Name);
                graphiclabel.Text = String.Format("Graphic: 0x{0:X4} ({0})", value);

                UpdateDetail(value);
                pictureBox.Invalidate();
            }
        }

        /// <summary>
        /// Updates if TileSize is changed
        /// </summary>
        public void ChangeTileSize()
        {
            col = pictureBox.Width / Options.ArtItemSizeWidth;
            row = pictureBox.Height / Options.ArtItemSizeHeight;
            vScrollBar.Maximum = ItemList.Count / col + 1;
            vScrollBar.Minimum = 1;
            vScrollBar.SmallChange = 1;
            vScrollBar.LargeChange = row;
            pictureBox.Invalidate();
            if (selected != -1)
                UpdateDetail(selected);
        }

        private void MakeHashFile()
        {
            string path = FiddlerControls.Options.AppDataPath;
            string FileName = Path.Combine(path, "UOFiddlerArt.hash");
            using (FileStream fs = new FileStream(FileName, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                using (BinaryWriter bin = new BinaryWriter(fs))
                {
                    byte[] md5 = Files.GetMD5(Files.GetFilePath("Art.mul"));
                    if (md5 == null)
                        return;
                    int length = md5.Length;
                    bin.Write(length);
                    bin.Write(md5);
                    foreach (int item in ItemList)
                        bin.Write(item);
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
            if (!refMarker.isLoaded)
                refMarker.OnLoad(refMarker, EventArgs.Empty);
            for (int i = 0; i < refMarker.ItemList.Count; ++i)
            {
                if (refMarker.ItemList[i] == graphic)
                {
                    refMarker.vScrollBar.Value = i / refMarker.col + 1;
                    refMarker.Selected = graphic;
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
                if (refMarker.selected >= 0)
                    index = refMarker.ItemList.IndexOf(refMarker.selected) + 1;
                if (index >= refMarker.ItemList.Count)
                    index = 0;
            }

            Regex regex = new Regex(@name, RegexOptions.IgnoreCase);
            for (int i = index; i < refMarker.ItemList.Count; ++i)
            {
                if (regex.IsMatch(TileData.ItemTable[refMarker.ItemList[i]].Name))
                {
                    refMarker.vScrollBar.Value = i / refMarker.col + 1;
                    refMarker.Selected = refMarker.ItemList[i];
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
            Options.LoadedUltimaClass["Animdata"] = true;
            Options.LoadedUltimaClass["Hues"] = true;
            if (!Loaded) // only once
            {
                PluginInterface.Events.FireModifyItemShowContextMenuEvent(this.contextMenuStrip1);
            }

            ShowFreeSlots = false;
            showFreeSlotsToolStripMenuItem.Checked = false;
            ItemList = new List<int>();
            if ((Files.UseHashFile) && (Files.CompareHashFile("Art", FiddlerControls.Options.AppDataPath)) && (!Ultima.Art.Modified))
            {
                string path = FiddlerControls.Options.AppDataPath;
                string FileName = Path.Combine(path, "UOFiddlerArt.hash");
                if (File.Exists(FileName))
                {
                    using (FileStream bin = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
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
                                    ItemList.Add(j);
                                    i += 4;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                int staticlength = Ultima.Art.GetMaxItemID() + 1;
                for (int i = 0; i < staticlength; ++i)
                {
                    if (Art.IsValidStatic(i))
                        ItemList.Add(i);
                }
                if (Files.UseHashFile)
                    MakeHashFile();
            }
            vScrollBar.Maximum = ItemList.Count / col + 1;
            pictureBox.Invalidate();
            if (!Loaded)
            {
                FiddlerControls.Events.FilePathChangeEvent += new FiddlerControls.Events.FilePathChangeHandler(OnFilePathChangeEvent);
                FiddlerControls.Events.ItemChangeEvent += new FiddlerControls.Events.ItemChangeHandler(OnItemChangeEvent);
                FiddlerControls.Events.TileDataChangeEvent += new FiddlerControls.Events.TileDataChangeHandler(OnTileDataChangeEvent);
            }
            Loaded = true;
            Cursor.Current = Cursors.Default;
        }
        private void OnFilePathChangeEvent()
        {
            if (FiddlerControls.Options.DesignAlternative)
                Reload();
        }

        void OnTileDataChangeEvent(object sender, int id)
        {
            if (!FiddlerControls.Options.DesignAlternative)
                return;
            if (!Loaded)
                return;
            if (sender.Equals(this))
                return;
            if (id < 0x4000)
                return;
            id -= 0x4000;
            if (selected == id)
            {
                graphiclabel.Text = String.Format("Graphic: 0x{0:X4} ({0})", id);
                UpdateDetail(id);
            }
        }

        private void OnItemChangeEvent(object sender, int index)
        {
            if (!FiddlerControls.Options.DesignAlternative)
                return;
            if (!Loaded)
                return;
            if (sender.Equals(this))
                return;
            if (Ultima.Art.IsValidStatic(index))
            {
                bool done = false;
                for (int i = 0; i < ItemList.Count; ++i)
                {
                    if (index < ItemList[i])
                    {
                        ItemList.Insert(i, index);
                        done = true;
                        break;
                    }
                    if (index == ItemList[i])
                    {
                        done = true;
                        break;
                    }
                }
                if (!done)
                    ItemList.Add(index);
                vScrollBar.Maximum = ItemList.Count / col + 1;
            }
            else
            {
                if (!ShowFreeSlots)
                {
                    ItemList.Remove(index);
                    vScrollBar.Maximum = ItemList.Count / col + 1;
                }
            }
        }

        private int GetIndex(int x, int y)
        {
            int value = Math.Max(0, ((col * (vScrollBar.Value - 1)) + (x + (y * col))));
            if (ItemList.Count > value)
                return ItemList[value];
            else
                return -1;
        }

        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta < 0)
            {
                if (vScrollBar.Value < vScrollBar.Maximum)
                {
                    vScrollBar.Value++;
                    pictureBox.Invalidate();
                }
            }
            else
            {
                if (vScrollBar.Value > 1)
                {
                    vScrollBar.Value--;
                    pictureBox.Invalidate();
                }
            }
        }

        private void OnScroll(object sender, ScrollEventArgs e)
        {
            pictureBox.Invalidate();
        }

        static Brush BrushLightBlue = Brushes.LightBlue;
        static Brush BrushLightCoral = Brushes.LightCoral;
        static Brush BrushRed = Brushes.Red;
        static Pen PenGray = Pens.Gray;

        private void OnPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.White);

            for (int x = 0; x <= col; ++x)
            {
                e.Graphics.DrawLine(PenGray, new Point(x * Options.ArtItemSizeWidth, 0),
                    new Point(x * Options.ArtItemSizeWidth, row * Options.ArtItemSizeHeight));
            }

            for (int y = 0; y <= row; ++y)
            {
                e.Graphics.DrawLine(PenGray, new Point(0, y * Options.ArtItemSizeHeight),
                    new Point(col * Options.ArtItemSizeWidth, y * Options.ArtItemSizeHeight));
            }

            for (int y = 0; y < row; ++y)
            {
                for (int x = 0; x < col; ++x)
                {
                    int index = GetIndex(x, y);
                    if (index >= 0)
                    {
                        bool patched;
                        Bitmap b = Art.GetStatic(index, out patched);

                        if (b != null)
                        {
                            Point loc = new Point((x * Options.ArtItemSizeWidth) + 1, (y * Options.ArtItemSizeHeight) + 1);
                            Size size = new Size(Options.ArtItemSizeWidth - 1, Options.ArtItemSizeHeight - 1);
                            Rectangle rect = new Rectangle(loc, size);

                            e.Graphics.Clip = new Region(rect);

                            if (index == selected)
                                e.Graphics.FillRectangle(BrushLightBlue, rect);
                            else if (patched)
                                e.Graphics.FillRectangle(BrushLightCoral, rect);

                            if (Options.ArtItemClip)
                                e.Graphics.DrawImage(b, loc);
                            else
                            {
                                int width = b.Width;
                                int height = b.Height;
                                if (width > size.Width)
                                {
                                    width = size.Width;
                                    height = size.Height * b.Height / b.Width;
                                }
                                if (height > size.Height)
                                {
                                    height = size.Height;
                                    width = size.Width * b.Width / b.Height;
                                }
                                e.Graphics.DrawImage(b, new Rectangle(loc, new Size(width, height)));
                            }
                        }
                        else
                        {
                            Point loc = new Point((x * Options.ArtItemSizeWidth) + 1, (y * Options.ArtItemSizeHeight) + 1);
                            Size size = new Size(Options.ArtItemSizeWidth - 1, Options.ArtItemSizeHeight - 1);
                            Rectangle rect = new Rectangle(loc, size);

                            e.Graphics.Clip = new Region(rect);
                            if (index == selected)
                                e.Graphics.FillRectangle(BrushLightBlue, rect);
                            rect.X += 5;
                            rect.Y += 5;
                            rect.Width -= 10;
                            rect.Height -= 10;
                            e.Graphics.FillRectangle(BrushRed, rect);
                        }
                    }
                }
            }
        }

        private void OnResize(object sender, EventArgs e)
        {
            if ((pictureBox.Height == 0) || (pictureBox.Width == 0))
                return;
            col = pictureBox.Width / Options.ArtItemSizeWidth;
            row = pictureBox.Height / Options.ArtItemSizeHeight + 1;
            vScrollBar.Maximum = ItemList.Count / col + 1;
            vScrollBar.Minimum = 1;
            vScrollBar.SmallChange = 1;
            vScrollBar.LargeChange = row;
            pictureBox.Invalidate();
            if (selected != -1)
                UpdateDetail(selected);
        }

        private void OnMouseClick(object sender, MouseEventArgs e)
        {
            pictureBox.Focus();
            int x = e.X / (Options.ArtItemSizeWidth - 1);
            int y = e.Y / (Options.ArtItemSizeHeight - 1);
            int index = GetIndex(x, y);
            if (index >= 0)
                Selected = index;
        }

        private void UpdateDetail(int id)
        {
            Ultima.ItemData item = Ultima.TileData.ItemTable[id];
            Bitmap bit = Ultima.Art.GetStatic(id);
            if (bit == null)
            {
                splitContainer2.SplitterDistance = 10;
                Bitmap newbit = new Bitmap(DetailPictureBox.Size.Width, DetailPictureBox.Size.Height);
                Graphics newgraph = Graphics.FromImage(newbit);
                newgraph.Clear(Color.FromArgb(-1));
                DetailPictureBox.Image = newbit;
            }
            else
            {
                splitContainer2.SplitterDistance = bit.Size.Height + 10;
                Bitmap newbit = new Bitmap(DetailPictureBox.Size.Width, DetailPictureBox.Size.Height);
                Graphics newgraph = Graphics.FromImage(newbit);
                newgraph.Clear(Color.FromArgb(-1));
                newgraph.DrawImage(bit, (DetailPictureBox.Size.Width - bit.Width) / 2, 5);
                DetailPictureBox.Image = newbit;
            }

            DetailTextBox.Clear();
            DetailTextBox.AppendText(String.Format("Name: {0}\n", item.Name));
            DetailTextBox.AppendText(String.Format("Graphic: 0x{0:X4}\n", id));
            DetailTextBox.AppendText(String.Format("Height/Capacity: {0}\n", item.Height));
            DetailTextBox.AppendText(String.Format("Weight: {0}\n", item.Weight));
            DetailTextBox.AppendText(String.Format("Animation: {0}\n", item.Animation));
            DetailTextBox.AppendText(String.Format("Quality/Layer/Light: {0}\n", item.Quality));
            DetailTextBox.AppendText(String.Format("Quantity: {0}\n", item.Quantity));
            DetailTextBox.AppendText(String.Format("Hue: {0}\n", item.Hue));
            DetailTextBox.AppendText(String.Format("StackingOffset/Unk4: {0}\n", item.StackingOffset));
            DetailTextBox.AppendText(String.Format("Flags: {0}\n", item.Flags));
            if ((item.Flags & TileFlag.Animation) != 0)
            {
                Animdata.Data info = Animdata.GetAnimData(id);
                if (info != null)
                    DetailTextBox.AppendText(String.Format("Animation FrameCount: {0} Interval: {1}\n", info.FrameCount, info.FrameInterval));
            }
        }

        public void OnMouseDoubleClick(object sender, MouseEventArgs e)
        {
            Point m = PointToClient(Control.MousePosition);
            int x = m.X / (Options.ArtItemSizeWidth - 1);
            int y = m.Y / (Options.ArtItemSizeHeight - 1);
            int index = GetIndex(x, y);
            if (index >= 0)
            {
                ItemDetail f = new ItemDetail(index);
                f.TopMost = true;
                f.Show();
            }
        }

        private ItemSearch showform = null;
        private void OnSearchClick(object sender, EventArgs e)
        {
            if ((showform == null) || (showform.IsDisposed))
            {
                showform = new ItemSearch();
                showform.TopMost = true;
                showform.Show();
            }
        }

        private void onClickFindFree(object sender, EventArgs e)
        {
            if (ShowFreeSlots)
            {
                int i;
                if (selected > -1)
                    i = ItemList.IndexOf(selected) + 1;
                else
                    i = 0;
                for (; i < ItemList.Count; ++i)
                {
                    if (!Art.IsValidStatic(ItemList[i]))
                    {
                        vScrollBar.Value = i / refMarker.col + 1;
                        Selected = ItemList[i];
                        break;
                    }
                }
            }
            else
            {
                int id, i;
                if (selected > -1)
                {
                    id = selected + 1;
                    i = ItemList.IndexOf(selected) + 1;
                }
                else
                {
                    id = 0;
                    i = 0;
                }
                for (; i < ItemList.Count; ++i, ++id)
                {
                    if (id < ItemList[i])
                    {
                        vScrollBar.Value = i / refMarker.col + 1;
                        Selected = ItemList[i];
                        break;
                    }
                }
            }
        }

        private void OnClickReplace(object sender, EventArgs e)
        {
            if (selected >= 0)
            {
                using (OpenFileDialog dialog = new OpenFileDialog())
                {
                    dialog.Multiselect = false;
                    dialog.Title = "Choose image file to replace";
                    dialog.CheckFileExists = true;
                    dialog.Filter = "image files (*.tiff;*.bmp)|*.tiff;*.bmp";
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        Bitmap bmp = new Bitmap(dialog.FileName);
                        if (dialog.FileName.Contains(".bmp"))
                            bmp = Utils.ConvertBmp(bmp);
                        Art.ReplaceStatic(selected, bmp);
                        FiddlerControls.Events.FireItemChangeEvent(this, selected);
                        pictureBox.Invalidate();
                        Options.ChangedUltimaClass["Art"] = true;
                    }
                }
            }
        }

        private void OnClickRemove(object sender, EventArgs e)
        {
            if (!Art.IsValidStatic(selected))
                return;
            DialogResult result =
                        MessageBox.Show(String.Format("Are you sure to remove 0x{0:X}", selected),
                        "Save",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes)
            {
                Art.RemoveStatic(selected);
                FiddlerControls.Events.FireItemChangeEvent(this, selected);
                if (!ShowFreeSlots)
                    ItemList.Remove(selected);
                --selected;
                pictureBox.Invalidate();
                Options.ChangedUltimaClass["Art"] = true;
            }
        }

        private void onTextChangedInsert(object sender, EventArgs e)
        {
            int index;
            if (Utils.ConvertStringToInt(InsertText.Text, out index, 0, Ultima.Art.GetMaxItemID()))
            {
                if (Art.IsValidStatic(index))
                    InsertText.ForeColor = Color.Red;
                else
                    InsertText.ForeColor = Color.Black;
            }
            else
                InsertText.ForeColor = Color.Red;
        }

        private void onKeyDownInsertText(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int index;
                if (Utils.ConvertStringToInt(InsertText.Text, out index, 0, Ultima.Art.GetMaxItemID()))
                {
                    if (Art.IsValidStatic(index))
                        return;
                    contextMenuStrip1.Close();
                    using (OpenFileDialog dialog = new OpenFileDialog())
                    {
                        dialog.Multiselect = false;
                        dialog.Title = String.Format("Choose image file to insert at 0x{0:X}", index);
                        dialog.CheckFileExists = true;
                        dialog.Filter = "image files (*.tiff;*.bmp)|*.tiff;*.bmp";
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            Bitmap bmp = new Bitmap(dialog.FileName);
                            if (dialog.FileName.Contains(".bmp"))
                                bmp = Utils.ConvertBmp(bmp);
                            Art.ReplaceStatic(index, bmp);
                            FiddlerControls.Events.FireItemChangeEvent(this, index);
                            Options.ChangedUltimaClass["Art"] = true;
                            if (ShowFreeSlots)
                            {
                                selected = index;
                                vScrollBar.Value = index / refMarker.col + 1;
                                namelabel.Text = String.Format("Name: {0}", TileData.ItemTable[selected].Name);
                                graphiclabel.Text = String.Format("Graphic: 0x{0:X4} ({0})", selected);
                                UpdateDetail(selected);
                                pictureBox.Invalidate();
                            }
                            else
                            {
                                bool done = false;
                                for (int i = 0; i < ItemList.Count; ++i)
                                {
                                    if (index < ItemList[i])
                                    {
                                        ItemList.Insert(i, index);
                                        vScrollBar.Value = i / refMarker.col + 1;
                                        done = true;
                                        break;
                                    }
                                }
                                if (!done)
                                {
                                    ItemList.Add(index);
                                    vScrollBar.Value = ItemList.Count / refMarker.col + 1;
                                }
                                selected = index;
                                namelabel.Text = String.Format("Name: {0}", TileData.ItemTable[selected].Name);
                                graphiclabel.Text = String.Format("Graphic: 0x{0:X4} ({0})", selected);
                                UpdateDetail(selected);
                                pictureBox.Invalidate();
                            }
                        }
                    }
                }
            }
        }

        private void onClickSave(object sender, EventArgs e)
        {
            DialogResult result =
                        MessageBox.Show("Are you sure? Will take a while", "Save",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning,
                        MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes)
            {
                Cursor.Current = Cursors.WaitCursor;
                ProgressBar bar = new ProgressBar(Art.GetIdxLength(), "Save");
                Art.Save(FiddlerControls.Options.OutputPath);
                bar.Dispose();
                Cursor.Current = Cursors.Default;
                Options.ChangedUltimaClass["Art"] = false;
                MessageBox.Show(
                    String.Format("Saved to {0}", FiddlerControls.Options.OutputPath),
                    "Save",
                    MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
        }

        private void onClickShowFreeSlots(object sender, EventArgs e)
        {
            ShowFreeSlots = !ShowFreeSlots;
            if (ShowFreeSlots)
            {
                for (int j = 0; j < Ultima.Art.GetMaxItemID() + 1; ++j)
                {
                    if (ItemList.Count > j)
                    {
                        if (ItemList[j] != j)
                            ItemList.Insert(j, j);
                    }
                    else
                        ItemList.Insert(j, j);
                }
                vScrollBar.Maximum = ItemList.Count / col + 1;
                pictureBox.Invalidate();
            }
            else
            {
                Reload();
            }
        }

        private void extract_Image_ClickBmp(object sender, EventArgs e)
        {
            if (selected == -1)
                return;
            if (!Art.IsValidStatic(selected))
                return;
            string path = FiddlerControls.Options.OutputPath;
            string FileName = Path.Combine(path, String.Format("Item 0x{0:X}.bmp", selected));
            Bitmap bit = new Bitmap(Ultima.Art.GetStatic(selected));
            if (bit != null)
                bit.Save(FileName, ImageFormat.Bmp);
            bit.Dispose();
            MessageBox.Show(
                String.Format("Item saved to {0}", FileName),
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void extract_Image_ClickTiff(object sender, EventArgs e)
        {
            if (selected == -1)
                return;
            if (!Art.IsValidStatic(selected))
                return;
            string path = FiddlerControls.Options.OutputPath;
            string FileName = Path.Combine(path, String.Format("Item 0x{0:X}.tiff", selected));
            Bitmap bit = new Bitmap(Ultima.Art.GetStatic(selected));
            if (bit != null)
                bit.Save(FileName, ImageFormat.Tiff);
            bit.Dispose();
            MessageBox.Show(
                String.Format("Item saved to {0}", FileName),
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void extract_Image_ClickJpg(object sender, EventArgs e)
        {
            if (selected == -1)
                return;
            if (!Art.IsValidStatic(selected))
                return;
            string path = FiddlerControls.Options.OutputPath;
            string FileName = Path.Combine(path, String.Format("Item 0x{0:X}.jpg", selected));
            Bitmap bit = new Bitmap(Ultima.Art.GetStatic(selected));
            if (bit != null)
                bit.Save(FileName, ImageFormat.Jpeg);
            bit.Dispose();
            MessageBox.Show(
                String.Format("Item saved to {0}", FileName),
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void OnClickSelectTiledata(object sender, EventArgs e)
        {
            if (selected >= 0)
                FiddlerControls.TileDatas.Select(selected, false);
        }

        private void OnClickSelectRadarCol(object sender, EventArgs e)
        {
            if (selected >= 0)
                FiddlerControls.RadarColor.Select(selected, false);
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
                    ProgressBar bar = new ProgressBar(ItemList.Count, "Export to bmp", false);
                    for (int i = 0; i < ItemList.Count; ++i)
                    {
                        FiddlerControls.Events.FireProgressChangeEvent();
                        Application.DoEvents();
                        int index = ItemList[i];
                        if (Art.IsValidStatic(index))
                        {
                            string FileName = Path.Combine(dialog.SelectedPath, String.Format("Item 0x{0:X}.bmp", index));
                            Bitmap bit = new Bitmap(Ultima.Art.GetStatic(index));
                            if (bit != null)
                                bit.Save(FileName, ImageFormat.Bmp);
                            bit.Dispose();
                        }
                    }
                    bar.Dispose();
                    Cursor.Current = Cursors.Default;
                    MessageBox.Show(String.Format("All Item saved to {0}", dialog.SelectedPath), "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
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
                    ProgressBar bar = new ProgressBar(ItemList.Count, "Export to tiff", false);
                    for (int i = 0; i < ItemList.Count; ++i)
                    {
                        FiddlerControls.Events.FireProgressChangeEvent();
                        Application.DoEvents();
                        int index = ItemList[i];
                        if (Art.IsValidStatic(index))
                        {
                            string FileName = Path.Combine(dialog.SelectedPath, String.Format("Item 0x{0:X}.tiff", index));
                            Bitmap bit = new Bitmap(Ultima.Art.GetStatic(index));
                            if (bit != null)
                                bit.Save(FileName, ImageFormat.Tiff);
                            bit.Dispose();
                        }
                    }
                    bar.Dispose();
                    Cursor.Current = Cursors.Default;
                    MessageBox.Show(String.Format("All Item saved to {0}", dialog.SelectedPath), "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
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
                    ProgressBar bar = new ProgressBar(ItemList.Count, "Export to jpeg", false);
                    for (int i = 0; i < ItemList.Count; ++i)
                    {
                        FiddlerControls.Events.FireProgressChangeEvent();
                        Application.DoEvents();
                        int index = ItemList[i];
                        if (Art.IsValidStatic(index))
                        {
                            string FileName = Path.Combine(dialog.SelectedPath, String.Format("Item 0x{0:X}.Jpg", index));
                            Bitmap bit = new Bitmap(Ultima.Art.GetStatic(index));
                            if (bit != null)
                                bit.Save(FileName, ImageFormat.Jpeg);
                            bit.Dispose();
                        }
                    }
                    bar.Dispose();
                    Cursor.Current = Cursors.Default;
                    MessageBox.Show(String.Format("All Item saved to {0}", dialog.SelectedPath), "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                }
            }
        }

        #region Preloader
        private void OnClickPreload(object sender, EventArgs e)
        {
            if (PreLoader.IsBusy)
                return;
            ProgressBar.Minimum = 1;
            ProgressBar.Maximum = ItemList.Count;
            ProgressBar.Step = 1;
            ProgressBar.Value = 1;
            ProgressBar.Visible = true;
            PreLoader.RunWorkerAsync();
        }

        private void PreLoaderDoWork(object sender, DoWorkEventArgs e)
        {
            foreach (int item in ItemList)
            {
                Art.GetStatic(item);
                PreLoader.ReportProgress(1);
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
        #endregion


    }
}
