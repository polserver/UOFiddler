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
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;

namespace UoFiddler.Controls.UserControls
{
    public partial class LandTilesAlternative : UserControl
    {
        public LandTilesAlternative()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            pictureBox.MouseWheel += OnMouseWheel;
            _refMarker = this;
        }

        private List<int> _tileList = new List<int>();
        private int _col;
        private int _row;
        private int _selected = -1;

        public bool IsLoaded { get; private set; }

        private static LandTilesAlternative _refMarker;

        public int Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                namelabel.Text = $"Name: {TileData.LandTable[value].Name}";
                graphiclabel.Text = string.Format("ID: 0x{0:X4} ({0})", value);
                FlagsLabel.Text = $"Flags: {TileData.LandTable[value].Flags}";
                pictureBox.Invalidate();
            }
        }

        /// <summary>
        /// Searches Objtype and Select
        /// </summary>
        /// <param name="graphic"></param>
        /// <returns></returns>
        public static bool SearchGraphic(int graphic)
        {
            if (!_refMarker.IsLoaded)
            {
                _refMarker.OnLoad(_refMarker, EventArgs.Empty);
            }

            for (int i = 0; i < _refMarker._tileList.Count; ++i)
            {
                if (_refMarker._tileList[i] != graphic)
                {
                    continue;
                }

                _refMarker.vScrollBar.Value = (i / _refMarker._col) + 1;
                _refMarker.Selected = graphic;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Searches for name and selects
        /// </summary>
        /// <param name="name"></param>
        /// <param name="next">private bool Loaded = false;</param>
        /// <returns></returns>
        public static bool SearchName(string name, bool next)
        {
            int index = 0;
            if (next)
            {
                if (_refMarker._selected >= 0)
                {
                    index = _refMarker._tileList.IndexOf(_refMarker._selected) + 1;
                }

                if (index >= _refMarker._tileList.Count)
                {
                    index = 0;
                }
            }

            Regex regex = new Regex(name, RegexOptions.IgnoreCase);
            for (int i = index; i < _refMarker._tileList.Count; ++i)
            {
                if (!regex.IsMatch(TileData.LandTable[_refMarker._tileList[i]].Name))
                {
                    continue;
                }

                _refMarker.vScrollBar.Value = (i / _refMarker._col) + 1;
                _refMarker.Selected = _refMarker._tileList[i];
                return true;
            }
            return false;
        }

        /// <summary>
        /// ReLoads if loaded
        /// </summary>
        private void Reload()
        {
            if (!IsLoaded)
            {
                return;
            }

            _tileList = new List<int>();
            _selected = -1;
            OnLoad(this, new MyEventArgs(MyEventArgs.Types.ForceReload));
        }

        private int GetIndex(int x, int y)
        {
            int value = Math.Max(0, (_col * (vScrollBar.Value - 1)) + x + (y * _col));
            return _tileList.Count > value ? _tileList[value] : -1;
        }

        private void OnLoad(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Options.LoadedUltimaClass["TileData"] = true;
            Options.LoadedUltimaClass["Art"] = true;

            for (int i = 0; i < 0x4000; ++i)
            {
                if (Art.IsValidLand(i))
                {
                    _tileList.Add(i);
                }
            }
            vScrollBar.Maximum = (_tileList.Count / _col) + 1;
            pictureBox.Invalidate();
            if (!IsLoaded)
            {
                ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
                ControlEvents.LandTileChangeEvent += OnLandTileChangeEvent;
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

            if (id > 0x3FFF)
            {
                return;
            }

            if (_selected != id)
            {
                return;
            }

            namelabel.Text = $"Name: {TileData.LandTable[id].Name}";
            FlagsLabel.Text = $"Flags: {TileData.LandTable[id].Flags}";
        }

        private void OnLandTileChangeEvent(object sender, int index)
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

            if (Art.IsValidLand(index))
            {
                bool done = false;
                for (int i = 0; i < _tileList.Count; ++i)
                {
                    if (index < _tileList[i])
                    {
                        _tileList.Insert(i, index);
                        done = true;
                        break;
                    }

                    if (index != _tileList[i])
                    {
                        continue;
                    }

                    done = true;
                    break;
                }
                if (!done)
                {
                    _tileList.Add(index);
                }

                vScrollBar.Maximum = (_tileList.Count / _col) + 1;
            }
            else
            {
                _tileList.Remove(index);
                vScrollBar.Maximum = (_tileList.Count / _col) + 1;
            }
        }

        private void OnScroll(object sender, ScrollEventArgs e)
        {
            pictureBox.Invalidate();
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

        private void OnPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.White);

            for (int x = 0; x <= _col; ++x)
            {
                e.Graphics.DrawLine(Pens.Gray, new Point(x * 49, 0),
                    new Point(x * 49, _row * 49));
            }

            for (int y = 0; y <= _row; ++y)
            {
                e.Graphics.DrawLine(Pens.Gray, new Point(0, y * 49),
                    new Point(_col * 49, y * 49));
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

                    Bitmap b = Art.GetLand(index, out bool patched);
                    if (b == null)
                    {
                        continue;
                    }

                    Point loc = new Point((x * 49) + 1, (y * 49) + 1);
                    Size size = new Size(49 - 1, 49 - 1);
                    Rectangle rect = new Rectangle(loc, size);

                    e.Graphics.Clip = new Region(rect);
                    if (index == _selected)
                    {
                        e.Graphics.FillRectangle(Brushes.LightBlue, rect);
                    }
                    else if (patched)
                    {
                        e.Graphics.FillRectangle(Brushes.LightCoral, rect);
                    }

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
        }

        private void OnResize(object sender, EventArgs e)
        {
            if (pictureBox.Width == 0 || pictureBox.Height == 0)
            {
                return;
            }

            _col = pictureBox.Width / 49;
            _row = (pictureBox.Height / 49) + 1;
            vScrollBar.Maximum = (_tileList.Count / _col) + 1;
            vScrollBar.Minimum = 1;
            vScrollBar.SmallChange = 1;
            vScrollBar.LargeChange = _row;
            pictureBox.Invalidate();
        }

        private void OnMouseClick(object sender, MouseEventArgs e)
        {
            pictureBox.Focus();
            int x = e.X / (49 - 1);
            int y = e.Y / (49 - 1);
            int index = GetIndex(x, y);
            if (index < 0)
            {
                return;
            }

            if (_selected != index)
            {
                Selected = index;
            }
        }

        private LandTileSearch _showForm;

        private void OnClickSearch(object sender, EventArgs e)
        {
            if (_showForm?.IsDisposed == false)
            {
                return;
            }

            _showForm = new LandTileSearch
            {
                TopMost = true
            };
            _showForm.Show();
        }

        private void OnClickFindFree(object sender, EventArgs e)
        {
            int id = _selected;
            ++id;
            for (int i = _tileList.IndexOf(_selected) + 1; i < _tileList.Count; ++i, ++id)
            {
                if (id >= _tileList[i])
                {
                    continue;
                }

                vScrollBar.Value = (i / _refMarker._col) + 1;
                Selected = _tileList[i];
                break;
            }
        }

        private void OnClickRemove(object sender, EventArgs e)
        {
            DialogResult result =
                        MessageBox.Show($"Are you sure to remove {_selected}", "Save",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
            if (result != DialogResult.Yes)
            {
                return;
            }

            Art.RemoveLand(_selected);
            ControlEvents.FireLandTileChangeEvent(this, _selected);
            _tileList.Remove(_selected);
            --_selected;
            pictureBox.Invalidate();
            Options.ChangedUltimaClass["Art"] = true;
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

                Art.ReplaceLand(_selected, bmp);
                ControlEvents.FireLandTileChangeEvent(this, _selected);
                pictureBox.Invalidate();
                Options.ChangedUltimaClass["Art"] = true;
            }
        }

        private void OnTextChangedInsert(object sender, EventArgs e)
        {
            if (Utils.ConvertStringToInt(InsertText.Text, out int index, 0, 0x3FFF))
            {
                InsertText.ForeColor = Art.IsValidLand(index) ? Color.Red : Color.Black;
            }
            else
            {
                InsertText.ForeColor = Color.Red;
            }
        }

        private void OnKeyDownInsert(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            if (!Utils.ConvertStringToInt(InsertText.Text, out int index, 0, 0x3FFF))
            {
                return;
            }

            if (Art.IsValidLand(index))
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

                Art.ReplaceLand(index, bmp);
                ControlEvents.FireLandTileChangeEvent(this, index);
                bool done = false;
                for (int i = 0; i < _tileList.Count; ++i)
                {
                    if (index >= _tileList[i])
                    {
                        continue;
                    }

                    _tileList.Insert(i, index);
                    vScrollBar.Value = (i / _refMarker._col) + 1;
                    done = true;
                    break;
                }
                if (!done)
                {
                    _tileList.Add(index);
                    vScrollBar.Value = (_tileList.Count / _refMarker._col) + 1;
                }
                Selected = index;
                Options.ChangedUltimaClass["Art"] = true;
            }
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            DialogResult result =
                        MessageBox.Show("Are you sure? Will take a while", "Save",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            Art.Save(Options.OutputPath);
            Cursor.Current = Cursors.Default;
            MessageBox.Show(
                $"Saved to {Options.OutputPath}",
                "Save",
                MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            Options.ChangedUltimaClass["Art"] = false;
        }

        private void OnClickExportBmp(object sender, EventArgs e)
        {
            if (_selected < 0)
            {
                return;
            }

            string path = Options.OutputPath;
            string fileName = Path.Combine(path, $"Landtile {_selected}.bmp");
            Bitmap bit = new Bitmap(Art.GetLand(_selected));
            bit.Save(fileName, ImageFormat.Bmp);
            bit.Dispose();
            MessageBox.Show($"Landtile saved to {fileName}", "Saved",
                MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void OnClickExportTiff(object sender, EventArgs e)
        {
            if (_selected < 0)
            {
                return;
            }

            string path = Options.OutputPath;
            string fileName = Path.Combine(path, $"Landtile {_selected}.tiff");
            Bitmap bit = new Bitmap(Art.GetLand(_selected));
            bit.Save(fileName, ImageFormat.Tiff);
            bit.Dispose();
            MessageBox.Show($"Landtile saved to {fileName}", "Saved",
                MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void OnClickExportJpg(object sender, EventArgs e)
        {
            if (_selected < 0)
            {
                return;
            }

            string path = Options.OutputPath;
            string fileName = Path.Combine(path, $"Landtile {_selected}.jpg");
            Bitmap bit = new Bitmap(Art.GetLand(_selected));
            bit.Save(fileName, ImageFormat.Jpeg);
            bit.Dispose();
            MessageBox.Show($"Landtile saved to {fileName}", "Saved",
                MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void OnClickSelectTiledata(object sender, EventArgs e)
        {
            if (_selected >= 0)
            {
                TileDatas.Select(_selected, true);
            }
        }

        private void OnClickSelectRadarCol(object sender, EventArgs e)
        {
            if (_selected >= 0)
            {
                RadarColor.Select(_selected, true);
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

                for (int i = 0; i < _tileList.Count; ++i)
                {
                    int index = _tileList[i];
                    if (!Art.IsValidStatic(index))
                    {
                        continue;
                    }

                    string fileName = Path.Combine(dialog.SelectedPath, $"Landtile {index}.bmp");
                    Bitmap bit = new Bitmap(Art.GetLand(index));
                    bit.Save(fileName, ImageFormat.Bmp);
                    bit.Dispose();
                }
                MessageBox.Show($"All LandTiles saved to {dialog.SelectedPath}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
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

                for (int i = 0; i < _tileList.Count; ++i)
                {
                    int index = _tileList[i];
                    if (!Art.IsValidStatic(index))
                    {
                        continue;
                    }

                    string fileName = Path.Combine(dialog.SelectedPath, $"Landtile {index}.tiff");
                    Bitmap bit = new Bitmap(Art.GetLand(index));
                    bit.Save(fileName, ImageFormat.Tiff);
                    bit.Dispose();
                }
                MessageBox.Show($"All LandTiles saved to {dialog.SelectedPath}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
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

                for (int i = 0; i < _tileList.Count; ++i)
                {
                    int index = _tileList[i];
                    if (!Art.IsValidStatic(index))
                    {
                        continue;
                    }

                    string fileName = Path.Combine(dialog.SelectedPath, $"Landtile {index}.jpg");
                    Bitmap bit = new Bitmap(Art.GetLand(index));
                    bit.Save(fileName, ImageFormat.Jpeg);
                    bit.Dispose();
                }
                MessageBox.Show($"All LandTiles saved to {dialog.SelectedPath}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
        }
    }
}
