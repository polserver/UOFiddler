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

namespace FiddlerControls
{
    public partial class LandTilesAlternative : UserControl
    {
        public LandTilesAlternative()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            pictureBox.MouseWheel += new MouseEventHandler(OnMouseWheel);
            refMarker = this;
        }

        private List<int> TileList = new List<int>();
        private int col;
        private int row;
        private int selected = -1;
        private bool Loaded = false;
        public bool isLoaded { get { return Loaded; } }

        private static LandTilesAlternative refMarker = null;

        public int Selected
        {
            get { return selected; }
            set
            {
                selected = value;
                namelabel.Text = String.Format("Name: {0}", TileData.LandTable[value].Name);
                graphiclabel.Text = String.Format("ID: 0x{0:X4} ({0})", value);
                FlagsLabel.Text = String.Format("Flags: {0}", TileData.LandTable[value].Flags);
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
            if (!refMarker.isLoaded)
                refMarker.OnLoad(refMarker, EventArgs.Empty);
            for (int i = 0; i < refMarker.TileList.Count; ++i)
            {
                if (refMarker.TileList[i] == graphic)
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
        /// <param name="next">private bool Loaded = false;</param>
        /// <returns></returns>
        public static bool SearchName(string name, bool next)
        {
            int index = 0;
            if (next)
            {
                if (refMarker.selected >= 0)
                    index = refMarker.TileList.IndexOf(refMarker.selected) + 1;
                if (index >= refMarker.TileList.Count)
                    index = 0;
            }

            Regex regex = new Regex(@name, RegexOptions.IgnoreCase);
            for (int i = index; i < refMarker.TileList.Count; ++i)
            {
                if (regex.IsMatch(TileData.LandTable[refMarker.TileList[i]].Name))
                {
                    refMarker.vScrollBar.Value = i / refMarker.col + 1;
                    refMarker.Selected = refMarker.TileList[i];
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
            if (!Loaded)
                return;
            TileList = new List<int>();
            selected = -1;
            OnLoad(this, new MyEventArgs(MyEventArgs.TYPES.FORCERELOAD));
        }

        private int GetIndex(int x, int y)
        {
            int value = Math.Max(0, ((col * (vScrollBar.Value - 1)) + (x + (y * col))));
            if (TileList.Count > value)
                return TileList[value];
            else
                return -1;
        }

        public void OnLoad(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Options.LoadedUltimaClass["TileData"] = true;
            Options.LoadedUltimaClass["Art"] = true;

            for (int i = 0; i < 0x4000; ++i)
            {
                if (Art.IsValidLand(i))
                    TileList.Add(i);
            }
            vScrollBar.Maximum = TileList.Count / col + 1;
            pictureBox.Invalidate();
            if (!Loaded)
            {
                FiddlerControls.Events.FilePathChangeEvent += new FiddlerControls.Events.FilePathChangeHandler(OnFilePathChangeEvent);
                FiddlerControls.Events.LandTileChangeEvent += new FiddlerControls.Events.LandTileChangeHandler(OnLandTileChangeEvent);
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
            if (id > 0x3FFF)
                return;
            if (selected == id)
            {
                namelabel.Text = String.Format("Name: {0}", TileData.LandTable[id].Name);
                FlagsLabel.Text = String.Format("Flags: {0}", TileData.LandTable[id].Flags);
            }
        }

        private void OnLandTileChangeEvent(object sender, int index)
        {
            if (!FiddlerControls.Options.DesignAlternative)
                return;
            if (!Loaded)
                return;
            if (sender.Equals(this))
                return;
            if (Ultima.Art.IsValidLand(index))
            {
                bool done = false;
                for (int i = 0; i < TileList.Count; ++i)
                {
                    if (index < TileList[i])
                    {
                        TileList.Insert(i, index);
                        done = true;
                        break;
                    }
                    if (index == TileList[i])
                    {
                        done = true;
                        break;
                    }
                }
                if (!done)
                    TileList.Add(index);
                vScrollBar.Maximum = TileList.Count / col + 1;
            }
            else
            {
                TileList.Remove(index);
                vScrollBar.Maximum = TileList.Count / col + 1;
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

        private void OnPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.White);

            for (int x = 0; x <= col; ++x)
            {
                e.Graphics.DrawLine(Pens.Gray, new Point(x * 49, 0),
                    new Point(x * 49, row * 49));
            }

            for (int y = 0; y <= row; ++y)
            {
                e.Graphics.DrawLine(Pens.Gray, new Point(0, y * 49),
                    new Point(col * 49, y * 49));
            }

            for (int y = 0; y < row; ++y)
            {
                for (int x = 0; x < col; ++x)
                {
                    int index = GetIndex(x, y);
                    if (index >= 0)
                    {
                        bool patched;
                        Bitmap b = Art.GetLand(index, out patched);

                        if (b != null)
                        {
                            Point loc = new Point((x * 49) + 1, (y * 49) + 1);
                            Size size = new Size(49 - 1, 49 - 1);
                            Rectangle rect = new Rectangle(loc, size);

                            e.Graphics.Clip = new Region(rect);
                            if (index == selected)
                                e.Graphics.FillRectangle(Brushes.LightBlue, rect);
                            else if (patched)
                                e.Graphics.FillRectangle(Brushes.LightCoral, rect);

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
            }
        }

        private void OnResize(object sender, EventArgs e)
        {
            if ((pictureBox.Width == 0) || (pictureBox.Height == 0))
                return;
            col = pictureBox.Width / 49;
            row = pictureBox.Height / 49 + 1;
            vScrollBar.Maximum = TileList.Count / col + 1;
            vScrollBar.Minimum = 1;
            vScrollBar.SmallChange = 1;
            vScrollBar.LargeChange = row;
            pictureBox.Invalidate();
        }

        private void OnMouseClick(object sender, MouseEventArgs e)
        {
            pictureBox.Focus();
            int x = e.X / (49 - 1);
            int y = e.Y / (49 - 1);
            int index = GetIndex(x, y);
            if (index >= 0)
            {
                if (selected != index)
                    Selected = index;
            }
        }

        private LandTileSearch showform = null;
        private void OnClickSearch(object sender, EventArgs e)
        {
            if ((showform == null) || (showform.IsDisposed))
            {
                showform = new LandTileSearch();
                showform.TopMost = true;
                showform.Show();
            }
        }

        private void onClickFindFree(object sender, EventArgs e)
        {
            int id = selected;
            ++id;
            for (int i = TileList.IndexOf(selected) + 1; i < TileList.Count; ++i, ++id)
            {
                if (id < TileList[i])
                {
                    vScrollBar.Value = i / refMarker.col + 1;
                    Selected = TileList[i];
                    break;
                }
            }
        }

        private void onClickRemove(object sender, EventArgs e)
        {
            DialogResult result =
                        MessageBox.Show(String.Format("Are you sure to remove {0}", selected), "Save",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
            if (result == DialogResult.Yes)
            {
                Art.RemoveLand(selected);
                FiddlerControls.Events.FireLandTileChangeEvent(this, selected);
                TileList.Remove(selected);
                --selected;
                pictureBox.Invalidate();
                Options.ChangedUltimaClass["Art"] = true;
            }
        }

        private void onClickReplace(object sender, EventArgs e)
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
                        Art.ReplaceLand(selected, bmp);
                        FiddlerControls.Events.FireLandTileChangeEvent(this, selected);
                        pictureBox.Invalidate();
                        Options.ChangedUltimaClass["Art"] = true;
                    }
                }
            }
        }

        private void onTextChangedInsert(object sender, EventArgs e)
        {
            int index;
            if (Utils.ConvertStringToInt(InsertText.Text, out index, 0, 0x3FFF))
            {
                if (Art.IsValidLand(index))
                    InsertText.ForeColor = Color.Red;
                else
                    InsertText.ForeColor = Color.Black;
            }
            else
                InsertText.ForeColor = Color.Red;
        }

        private void onKeyDownInsert(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int index;
                if (Utils.ConvertStringToInt(InsertText.Text, out index, 0, 0x3FFF))
                {
                    if (Art.IsValidLand(index))
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
                            Art.ReplaceLand(index, bmp);
                            FiddlerControls.Events.FireLandTileChangeEvent(this, index);
                            bool done = false;
                            for (int i = 0; i < TileList.Count; ++i)
                            {
                                if (index < TileList[i])
                                {
                                    TileList.Insert(i, index);
                                    vScrollBar.Value = i / refMarker.col + 1;
                                    done = true;
                                    break;
                                }
                            }
                            if (!done)
                            {
                                TileList.Add(index);
                                vScrollBar.Value = TileList.Count / refMarker.col + 1;
                            }
                            Selected = index;
                            Options.ChangedUltimaClass["Art"] = true;
                        }
                    }
                }
            }
        }

        private void onClickSave(object sender, EventArgs e)
        {
            DialogResult result =
                        MessageBox.Show("Are you sure? Will take a while", "Save",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes)
            {
                Cursor.Current = Cursors.WaitCursor;
                Art.Save(FiddlerControls.Options.OutputPath);
                Cursor.Current = Cursors.Default;
                MessageBox.Show(
                    String.Format("Saved to {0}", FiddlerControls.Options.OutputPath),
                    "Save",
                    MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                Options.ChangedUltimaClass["Art"] = false;
            }
        }

        private void onClickExportBmp(object sender, EventArgs e)
        {
            if (selected >= 0)
            {
                string path = FiddlerControls.Options.OutputPath;
                string FileName = Path.Combine(path, String.Format("Landtile {0}.bmp", selected));
                Bitmap bit = new Bitmap(Ultima.Art.GetLand(selected));
                if (bit != null)
                    bit.Save(FileName, ImageFormat.Bmp);
                bit.Dispose();
                MessageBox.Show(String.Format("Landtile saved to {0}", FileName), "Saved",
                    MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
        }

        private void onClickExportTiff(object sender, EventArgs e)
        {
            if (selected >= 0)
            {
                string path = FiddlerControls.Options.OutputPath;
                string FileName = Path.Combine(path, String.Format("Landtile {0}.tiff", selected));
                Bitmap bit = new Bitmap(Ultima.Art.GetLand(selected));
                if (bit != null)
                    bit.Save(FileName, ImageFormat.Tiff);
                bit.Dispose();
                MessageBox.Show(String.Format("Landtile saved to {0}", FileName), "Saved",
                    MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
        }

        private void onClickExportJpg(object sender, EventArgs e)
        {
            if (selected >= 0)
            {
                string path = FiddlerControls.Options.OutputPath;
                string FileName = Path.Combine(path, String.Format("Landtile {0}.jpg", selected));
                Bitmap bit = new Bitmap(Ultima.Art.GetLand(selected));
                if (bit != null)
                    bit.Save(FileName, ImageFormat.Jpeg);
                bit.Dispose();
                MessageBox.Show(String.Format("Landtile saved to {0}", FileName), "Saved",
                    MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
        }

        private void OnClickSelectTiledata(object sender, EventArgs e)
        {
            if (selected >= 0)
                FiddlerControls.TileDatas.Select(selected, true);
        }

        private void OnClickSelectRadarCol(object sender, EventArgs e)
        {
            if (selected >= 0)
                FiddlerControls.RadarColor.Select(selected, true);
        }

        private void OnClick_SaveAllBmp(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    for (int i = 0; i < TileList.Count; ++i)
                    {
                        int index = TileList[i];
                        if (Art.IsValidStatic(index))
                        {
                            string FileName = Path.Combine(dialog.SelectedPath, String.Format("Landtile {0}.bmp", index));
                            Bitmap bit = new Bitmap(Ultima.Art.GetLand(index));
                            if (bit != null)
                                bit.Save(FileName, ImageFormat.Bmp);
                            bit.Dispose();
                        }
                    }
                    MessageBox.Show(String.Format("All LandTiles saved to {0}", dialog.SelectedPath), "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
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
                    for (int i = 0; i < TileList.Count; ++i)
                    {
                        int index = TileList[i];
                        if (Art.IsValidStatic(index))
                        {
                            string FileName = Path.Combine(dialog.SelectedPath, String.Format("Landtile {0}.tiff", index));
                            Bitmap bit = new Bitmap(Ultima.Art.GetLand(index));
                            if (bit != null)
                                bit.Save(FileName, ImageFormat.Tiff);
                            bit.Dispose();
                        }
                    }
                    MessageBox.Show(String.Format("All LandTiles saved to {0}", dialog.SelectedPath), "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
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
                    for (int i = 0; i < TileList.Count; ++i)
                    {
                        int index = TileList[i];
                        if (Art.IsValidStatic(index))
                        {
                            string FileName = Path.Combine(dialog.SelectedPath, String.Format("Landtile {0}.jpg", index));
                            Bitmap bit = new Bitmap(Ultima.Art.GetLand(index));
                            if (bit != null)
                                bit.Save(FileName, ImageFormat.Jpeg);
                            bit.Dispose();
                        }
                    }
                    MessageBox.Show(String.Format("All LandTiles saved to {0}", dialog.SelectedPath), "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                }
            }
        }

    }
}
