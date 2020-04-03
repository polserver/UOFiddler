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
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;
using UoFiddler.Controls.Helpers;
using ProgressBar = UoFiddler.Controls.Forms.ProgressBar;

namespace UoFiddler.Controls.UserControls
{
    public partial class ItemShowAlternative : UserControl
    {
        public ItemShowAlternative()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            RefMarker = this;
            pictureBox.MouseWheel += OnMouseWheel;
            DetailTextBox.AddBasicContextMenu();
        }

        private List<int> _itemList = new List<int>();
        private int _col;
        private int _row;
        private int _selected = -1;
        private bool _showFreeSlots;

        public static ItemShowAlternative RefMarker { get; private set; }
        public static PictureBox ItemPictureBox => RefMarker.pictureBox;
        public bool IsLoaded { get; private set; }

        public int Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                namelabel.Text = !Art.IsValidStatic(value) ? "Name: FREE" : $"Name: {TileData.ItemTable[value].Name}";

                graphiclabel.Text = string.Format("Graphic: 0x{0:X4} ({0})", value);

                UpdateDetail(value);
                pictureBox.Invalidate();
            }
        }

        /// <summary>
        /// Updates if TileSize is changed
        /// </summary>
        public void ChangeTileSize()
        {
            _col = pictureBox.Width / Options.ArtItemSizeWidth;
            _row = pictureBox.Height / Options.ArtItemSizeHeight;
            vScrollBar.Maximum = (_itemList.Count / _col) + 1;
            vScrollBar.Minimum = 1;
            vScrollBar.SmallChange = 1;
            vScrollBar.LargeChange = _row;
            pictureBox.Invalidate();
            if (_selected != -1)
            {
                UpdateDetail(_selected);
            }
        }

        private void MakeHashFile()
        {
            string path = Options.AppDataPath;
            string fileName = Path.Combine(path, "UOFiddlerArt.hash");
            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                using (BinaryWriter bin = new BinaryWriter(fs))
                {
                    byte[] md5 = Files.GetMD5(Files.GetFilePath("Art.mul"));
                    if (md5 == null)
                    {
                        return;
                    }

                    int length = md5.Length;
                    bin.Write(length);
                    bin.Write(md5);
                    foreach (int item in _itemList)
                    {
                        bin.Write(item);
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

            for (int i = 0; i < RefMarker._itemList.Count; ++i)
            {
                if (RefMarker._itemList[i] == graphic)
                {
                    RefMarker.vScrollBar.Value = (i / RefMarker._col) + 1;
                    RefMarker.Selected = graphic;
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
                if (RefMarker._selected >= 0)
                {
                    index = RefMarker._itemList.IndexOf(RefMarker._selected) + 1;
                }

                if (index >= RefMarker._itemList.Count)
                {
                    index = 0;
                }
            }

            Regex regex = new Regex(name, RegexOptions.IgnoreCase);
            for (int i = index; i < RefMarker._itemList.Count; ++i)
            {
                if (regex.IsMatch(TileData.ItemTable[RefMarker._itemList[i]].Name))
                {
                    RefMarker.vScrollBar.Value = (i / RefMarker._col) + 1;
                    RefMarker.Selected = RefMarker._itemList[i];
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
                Plugin.PluginEvents.FireModifyItemShowContextMenuEvent(contextMenuStrip1);
            }

            _showFreeSlots = false;
            showFreeSlotsToolStripMenuItem.Checked = false;
            _itemList = new List<int>();
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
                                    _itemList.Add(j);
                                    i += 4;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                int staticlength = Art.GetMaxItemID() + 1;
                for (int i = 0; i < staticlength; ++i)
                {
                    if (Art.IsValidStatic(i))
                    {
                        _itemList.Add(i);
                    }
                }
                if (Files.UseHashFile)
                {
                    MakeHashFile();
                }
            }
            vScrollBar.Maximum = (_itemList.Count / _col) + 1;
            pictureBox.Invalidate();
            if (!IsLoaded)
            {
                ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
                ControlEvents.ItemChangeEvent += OnItemChangeEvent;
                ControlEvents.TileDataChangeEvent += OnTileDataChangeEvent;
            }
            IsLoaded = true;
            Cursor.Current = Cursors.Default;
        }

        private void OnFilePathChangeEvent()
        {
            if (Options.DesignAlternative)
            {
                Reload();
            }
        }

        private void OnTileDataChangeEvent(object sender, int id)
        {
            if (!Options.DesignAlternative)
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
            if (_selected == id)
            {
                graphiclabel.Text = string.Format("Graphic: 0x{0:X4} ({0})", id);
                UpdateDetail(id);
            }
        }

        private void OnItemChangeEvent(object sender, int index)
        {
            if (!Options.DesignAlternative)
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
                bool done = false;
                for (int i = 0; i < _itemList.Count; ++i)
                {
                    if (index < _itemList[i])
                    {
                        _itemList.Insert(i, index);
                        done = true;
                        break;
                    }
                    if (index == _itemList[i])
                    {
                        done = true;
                        break;
                    }
                }
                if (!done)
                {
                    _itemList.Add(index);
                }

                vScrollBar.Maximum = (_itemList.Count / _col) + 1;
            }
            else
            {
                if (!_showFreeSlots)
                {
                    _itemList.Remove(index);
                    vScrollBar.Maximum = (_itemList.Count / _col) + 1;
                }
            }
        }

        private int GetIndex(int x, int y)
        {
            int value = Math.Max(0, (_col * (vScrollBar.Value - 1)) + x + (y * _col));
            return _itemList.Count > value ? _itemList[value] : -1;
        }

        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta < 0)
            {
                if (vScrollBar.Value >= vScrollBar.Maximum)
                {
                    return;
                }

                vScrollBar.Value++;
                pictureBox.Invalidate();
            }
            else
            {
                if (vScrollBar.Value <= 1)
                {
                    return;
                }

                vScrollBar.Value--;
                pictureBox.Invalidate();
            }
        }

        private void OnScroll(object sender, ScrollEventArgs e)
        {
            pictureBox.Invalidate();
        }

        private static readonly Brush _brushLightBlue = Brushes.LightBlue;
        private static readonly Brush _brushLightCoral = Brushes.LightCoral;
        private static readonly Brush _brushRed = Brushes.Red;
        private static readonly Pen _penGray = Pens.Gray;

        private void OnPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.White);

            for (int x = 0; x <= _col; ++x)
            {
                e.Graphics.DrawLine(_penGray, new Point(x * Options.ArtItemSizeWidth, 0),
                    new Point(x * Options.ArtItemSizeWidth, _row * Options.ArtItemSizeHeight));
            }

            for (int y = 0; y <= _row; ++y)
            {
                e.Graphics.DrawLine(_penGray, new Point(0, y * Options.ArtItemSizeHeight),
                    new Point(_col * Options.ArtItemSizeWidth, y * Options.ArtItemSizeHeight));
            }

            for (int y = 0; y < _row; ++y)
            {
                for (int x = 0; x < _col; ++x)
                {
                    int index = GetIndex(x, y);
                    if (index < 0)
                    {
                        continue;
                    }

                    Bitmap b = Art.GetStatic(index, out bool patched);

                    if (b != null)
                    {
                        Point loc = new Point((x * Options.ArtItemSizeWidth) + 1, (y * Options.ArtItemSizeHeight) + 1);
                        Size size = new Size(Options.ArtItemSizeWidth - 1, Options.ArtItemSizeHeight - 1);
                        Rectangle rect = new Rectangle(loc, size);

                        e.Graphics.Clip = new Region(rect);

                        if (index == _selected)
                        {
                            e.Graphics.FillRectangle(_brushLightBlue, rect);
                        }
                        else if (patched)
                        {
                            e.Graphics.FillRectangle(_brushLightCoral, rect);
                        }

                        if (Options.ArtItemClip)
                        {
                            e.Graphics.DrawImage(b, loc);
                        }
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
                        if (index == _selected)
                        {
                            e.Graphics.FillRectangle(_brushLightBlue, rect);
                        }

                        rect.X += 5;
                        rect.Y += 5;
                        rect.Width -= 10;
                        rect.Height -= 10;
                        e.Graphics.FillRectangle(_brushRed, rect);
                    }
                }
            }
        }

        private void OnResize(object sender, EventArgs e)
        {
            if (pictureBox.Height == 0 || pictureBox.Width == 0)
            {
                return;
            }

            _col = pictureBox.Width / Options.ArtItemSizeWidth;
            _row = (pictureBox.Height / Options.ArtItemSizeHeight) + 1;
            vScrollBar.Maximum = (_itemList.Count / _col) + 1;
            vScrollBar.Minimum = 1;
            vScrollBar.SmallChange = 1;
            vScrollBar.LargeChange = _row;
            pictureBox.Invalidate();
            if (_selected != -1)
            {
                UpdateDetail(_selected);
            }
        }

        private void OnMouseClick(object sender, MouseEventArgs e)
        {
            pictureBox.Focus();
            int x = e.X / (Options.ArtItemSizeWidth - 1);
            int y = e.Y / (Options.ArtItemSizeHeight - 1);
            int index = GetIndex(x, y);
            if (index >= 0)
            {
                Selected = index;
            }
        }

        private void UpdateDetail(int id)
        {
            ItemData item = TileData.ItemTable[id];
            Bitmap bit = Art.GetStatic(id);

            int xMin = 0;
            int xMax = 0;
            int yMin = 0;
            int yMax = 0;

            if (bit == null)
            {
                splitContainer2.SplitterDistance = 10;
                Bitmap newBit = new Bitmap(DetailPictureBox.Size.Width, DetailPictureBox.Size.Height);
                Graphics newGraph = Graphics.FromImage(newBit);
                newGraph.Clear(Color.FromArgb(-1));
                DetailPictureBox.Image = newBit;
            }
            else
            {
                splitContainer2.SplitterDistance = bit.Size.Height + 10;
                Bitmap newBit = new Bitmap(DetailPictureBox.Size.Width, DetailPictureBox.Size.Height);
                Graphics newGraph = Graphics.FromImage(newBit);
                newGraph.Clear(Color.FromArgb(-1));
                newGraph.DrawImage(bit, (DetailPictureBox.Size.Width - bit.Width) / 2, 5);
                DetailPictureBox.Image = newBit;

                Art.Measure(bit, out xMin, out yMin, out xMax, out yMax);
            }

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

            if ((item.Flags & TileFlag.Animation) == 0)
            {
                return;
            }

            Animdata.Data info = Animdata.GetAnimData(id);
            if (info != null)
            {
                DetailTextBox.AppendText(
                    $"Animation FrameCount: {info.FrameCount} Interval: {info.FrameInterval}\n");
            }
        }

        public void OnMouseDoubleClick(object sender, MouseEventArgs e)
        {
            Point m = PointToClient(MousePosition);
            int x = m.X / (Options.ArtItemSizeWidth - 1);
            int y = m.Y / (Options.ArtItemSizeHeight - 1);
            int index = GetIndex(x, y);
            if (index < 0)
            {
                return;
            }

            ItemDetail f = new ItemDetail(index)
            {
                TopMost = true
            };
            f.Show();
        }

        private ItemSearch _showForm;

        private void OnSearchClick(object sender, EventArgs e)
        {
            if (_showForm?.IsDisposed == false)
            {
                return;
            }

            _showForm = new ItemSearch
            {
                TopMost = true
            };
            _showForm.Show();
        }

        private void OnClickFindFree(object sender, EventArgs e)
        {
            if (_showFreeSlots)
            {
                int i = _selected > -1 ? _itemList.IndexOf(_selected) + 1 : 0;
                for (; i < _itemList.Count; ++i)
                {
                    if (Art.IsValidStatic(_itemList[i]))
                    {
                        continue;
                    }

                    vScrollBar.Value = (i / RefMarker._col) + 1;
                    Selected = _itemList[i];
                    break;
                }
            }
            else
            {
                int id, i;
                if (_selected > -1)
                {
                    id = _selected + 1;
                    i = _itemList.IndexOf(_selected) + 1;
                }
                else
                {
                    id = 0;
                    i = 0;
                }
                for (; i < _itemList.Count; ++i, ++id)
                {
                    if (id >= _itemList[i])
                    {
                        continue;
                    }

                    vScrollBar.Value = (i / RefMarker._col) + 1;
                    Selected = _itemList[i];
                    break;
                }
            }
        }

        private void OnClickReplace(object sender, EventArgs e)
        {
            if (_selected < 0)
            {
                return;
            }

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = false;
                dialog.Title = "Choose image file to replace";
                dialog.CheckFileExists = true;
                dialog.Filter = "Image files (*.tif;*.tiff;*.bmp)|*.tif;*.tiff;*.bmp";
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                Bitmap bmp = new Bitmap(dialog.FileName);
                if (dialog.FileName.Contains(".bmp"))
                {
                    bmp = Utils.ConvertBmp(bmp);
                }

                Art.ReplaceStatic(_selected, bmp);
                ControlEvents.FireItemChangeEvent(this, _selected);
                pictureBox.Invalidate();
                Options.ChangedUltimaClass["Art"] = true;
            }
        }

        private void OnClickRemove(object sender, EventArgs e)
        {
            if (!Art.IsValidStatic(_selected))
            {
                return;
            }

            DialogResult result =
                        MessageBox.Show($"Are you sure to remove 0x{_selected:X}",
                        "Save",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return;
            }

            Art.RemoveStatic(_selected);
            ControlEvents.FireItemChangeEvent(this, _selected);
            if (!_showFreeSlots)
            {
                _itemList.Remove(_selected);
            }

            --_selected;
            pictureBox.Invalidate();
            Options.ChangedUltimaClass["Art"] = true;
        }

        private void OnTextChangedInsert(object sender, EventArgs e)
        {
            if (Utils.ConvertStringToInt(InsertText.Text, out int index, 0, Art.GetMaxItemID()))
            {
                InsertText.ForeColor = Art.IsValidStatic(index) ? Color.Red : Color.Black;
            }
            else
            {
                InsertText.ForeColor = Color.Red;
            }
        }

        private void OnKeyDownInsertText(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            if (!Utils.ConvertStringToInt(InsertText.Text, out int index, 0, Art.GetMaxItemID()))
            {
                return;
            }

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
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                Bitmap bmp = new Bitmap(dialog.FileName);
                if (dialog.FileName.Contains(".bmp"))
                {
                    bmp = Utils.ConvertBmp(bmp);
                }

                Art.ReplaceStatic(index, bmp);
                ControlEvents.FireItemChangeEvent(this, index);
                Options.ChangedUltimaClass["Art"] = true;
                if (_showFreeSlots)
                {
                    _selected = index;
                    vScrollBar.Value = (index / RefMarker._col) + 1;
                    namelabel.Text = $"Name: {TileData.ItemTable[_selected].Name}";
                    graphiclabel.Text = string.Format("Graphic: 0x{0:X4} ({0})", _selected);
                    UpdateDetail(_selected);
                    pictureBox.Invalidate();
                }
                else
                {
                    bool done = false;
                    for (int i = 0; i < _itemList.Count; ++i)
                    {
                        if (index >= _itemList[i])
                        {
                            continue;
                        }

                        _itemList.Insert(i, index);
                        vScrollBar.Value = (i / RefMarker._col) + 1;
                        done = true;
                        break;
                    }

                    if (!done)
                    {
                        _itemList.Add(index);
                        vScrollBar.Value = (_itemList.Count / RefMarker._col) + 1;
                    }
                    _selected = index;
                    namelabel.Text = $"Name: {TileData.ItemTable[_selected].Name}";
                    graphiclabel.Text = string.Format("Graphic: 0x{0:X4} ({0})", _selected);
                    UpdateDetail(_selected);
                    pictureBox.Invalidate();
                }
            }
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            DialogResult result =
                        MessageBox.Show("Are you sure? Will take a while", "Save",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning,
                        MessageBoxDefaultButton.Button2);

            if (result != DialogResult.Yes)
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            ProgressBar bar = new ProgressBar(Art.GetIdxLength(), "Save");
            Art.Save(Options.OutputPath);
            bar.Dispose();
            Cursor.Current = Cursors.Default;
            Options.ChangedUltimaClass["Art"] = false;
            MessageBox.Show(
                $"Saved to {Options.OutputPath}",
                "Save",
                MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void OnClickShowFreeSlots(object sender, EventArgs e)
        {
            _showFreeSlots = !_showFreeSlots;
            if (_showFreeSlots)
            {
                for (int j = 0; j < Art.GetMaxItemID() + 1; ++j)
                {
                    if (_itemList.Count > j)
                    {
                        if (_itemList[j] != j)
                        {
                            _itemList.Insert(j, j);
                        }
                    }
                    else
                    {
                        _itemList.Insert(j, j);
                    }
                }
                vScrollBar.Maximum = (_itemList.Count / _col) + 1;
                pictureBox.Invalidate();
            }
            else
            {
                Reload();
            }
        }

        private void Extract_Image_ClickBmp(object sender, EventArgs e)
        {
            if (_selected == -1)
            {
                return;
            }

            if (!Art.IsValidStatic(_selected))
            {
                return;
            }

            string path = Options.OutputPath;
            string fileName = Path.Combine(path, $"Item 0x{_selected:X}.bmp");
            Bitmap bit = new Bitmap(Art.GetStatic(_selected));
            bit.Save(fileName, ImageFormat.Bmp);
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
            if (_selected == -1)
            {
                return;
            }

            if (!Art.IsValidStatic(_selected))
            {
                return;
            }

            string path = Options.OutputPath;
            string fileName = Path.Combine(path, $"Item 0x{_selected:X}.tiff");
            Bitmap bit = new Bitmap(Art.GetStatic(_selected));
            bit.Save(fileName, ImageFormat.Tiff);
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
            if (_selected == -1)
            {
                return;
            }

            if (!Art.IsValidStatic(_selected))
            {
                return;
            }

            string path = Options.OutputPath;
            string fileName = Path.Combine(path, $"Item 0x{_selected:X}.jpg");
            Bitmap bit = new Bitmap(Art.GetStatic(_selected));
            bit.Save(fileName, ImageFormat.Jpeg);
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
            if (_selected >= 0)
            {
                TileDatas.Select(_selected, false);
            }
        }

        private void OnClickSelectRadarCol(object sender, EventArgs e)
        {
            if (_selected >= 0)
            {
                RadarColor.Select(_selected, false);
            }
        }

        private void OnClick_SaveAllBmp(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                Cursor.Current = Cursors.WaitCursor;
                ProgressBar bar = new ProgressBar(_itemList.Count, "Export to bmp", false);
                for (int i = 0; i < _itemList.Count; ++i)
                {
                    ControlEvents.FireProgressChangeEvent();
                    Application.DoEvents();
                    int index = _itemList[i];
                    if (!Art.IsValidStatic(index))
                    {
                        continue;
                    }

                    string fileName = Path.Combine(dialog.SelectedPath, $"Item 0x{index:X}.bmp");
                    Bitmap bit = new Bitmap(Art.GetStatic(index));
                    bit.Save(fileName, ImageFormat.Bmp);
                    bit.Dispose();
                }
                bar.Dispose();
                Cursor.Current = Cursors.Default;
                MessageBox.Show($"All Item saved to {dialog.SelectedPath}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
        }

        private void OnClick_SaveAllTiff(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                Cursor.Current = Cursors.WaitCursor;
                ProgressBar bar = new ProgressBar(_itemList.Count, "Export to tiff", false);
                for (int i = 0; i < _itemList.Count; ++i)
                {
                    ControlEvents.FireProgressChangeEvent();
                    Application.DoEvents();
                    int index = _itemList[i];
                    if (!Art.IsValidStatic(index))
                    {
                        continue;
                    }

                    string fileName = Path.Combine(dialog.SelectedPath, $"Item 0x{index:X}.tiff");
                    Bitmap bit = new Bitmap(Art.GetStatic(index));
                    bit.Save(fileName, ImageFormat.Tiff);
                    bit.Dispose();
                }
                bar.Dispose();
                Cursor.Current = Cursors.Default;
                MessageBox.Show($"All Item saved to {dialog.SelectedPath}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
        }

        private void OnClick_SaveAllJpg(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                Cursor.Current = Cursors.WaitCursor;
                ProgressBar bar = new ProgressBar(_itemList.Count, "Export to jpeg", false);
                for (int i = 0; i < _itemList.Count; ++i)
                {
                    ControlEvents.FireProgressChangeEvent();
                    Application.DoEvents();
                    int index = _itemList[i];
                    if (!Art.IsValidStatic(index))
                    {
                        continue;
                    }

                    string fileName = Path.Combine(dialog.SelectedPath, $"Item 0x{index:X}.Jpg");
                    Bitmap bit = new Bitmap(Art.GetStatic(index));
                    bit.Save(fileName, ImageFormat.Jpeg);
                    bit.Dispose();
                }
                bar.Dispose();
                Cursor.Current = Cursors.Default;
                MessageBox.Show($"All Item saved to {dialog.SelectedPath}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
        }

        private void OnClickPreload(object sender, EventArgs e)
        {
            if (PreLoader.IsBusy)
            {
                return;
            }

            ProgressBar.Minimum = 1;
            ProgressBar.Maximum = _itemList.Count;
            ProgressBar.Step = 1;
            ProgressBar.Value = 1;
            ProgressBar.Visible = true;
            PreLoader.RunWorkerAsync();
        }

        private void PreLoaderDoWork(object sender, DoWorkEventArgs e)
        {
            foreach (int item in _itemList)
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
    }
}
