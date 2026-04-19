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
using System.Security.Cryptography;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.UserControls.TileView;
using UoFiddler.Plugin.Compare.Classes;

namespace UoFiddler.Plugin.Compare.UserControls
{
    public partial class CompareGumpControl : UserControl
    {
        public CompareGumpControl()
        {
            InitializeComponent();
        }

        private readonly Dictionary<int, bool> _compare = new Dictionary<int, bool>();
        private readonly SHA256 _sha256 = SHA256.Create();
        private readonly List<int> _displayIndices = new List<int>();
        private bool _syncingSelection;
        private bool _loaded;

        private void OnLoad(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Options.LoadedUltimaClass["Gumps"] = true;

            _displayIndices.Clear();
            for (int i = 0; i < 0x10000; i++)
            {
                _displayIndices.Add(i);
            }

            tileView1.VirtualListSize = _displayIndices.Count;
            tileView2.VirtualListSize = 0;

            if (_displayIndices.Count > 0)
            {
                tileView1.FocusIndex = 0;
            }

            if (!_loaded)
            {
                ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
            }

            _loaded = true;
            Cursor.Current = Cursors.Default;
        }

        private void OnFilePathChangeEvent()
        {
            Reload();
        }

        private void Reload()
        {
            if (_loaded)
            {
                OnLoad(this, EventArgs.Empty);
            }
        }

        private void OnTileViewSizeChanged(object sender, EventArgs e)
        {
            var tv = (TileViewControl)sender;
            int w = tv.DisplayRectangle.Width;
            if (w > 0 && tv.TileSize.Width != w)
            {
                tv.TileSize = new Size(w, tv.TileSize.Height);
            }
        }

        private void OnDrawItem1(object sender, TileViewControl.DrawTileListItemEventArgs e)
        {
            DrawGumpItem(e, _displayIndices[e.Index], isSecondary: false);
        }

        private void OnDrawItem2(object sender, TileViewControl.DrawTileListItemEventArgs e)
        {
            DrawGumpItem(e, _displayIndices[e.Index], isSecondary: true);
        }

        private void DrawGumpItem(DrawItemEventArgs e, int i, bool isSecondary)
        {
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                e.Graphics.FillRectangle(Brushes.LightSteelBlue, e.Bounds);
            }
            else
            {
                e.Graphics.FillRectangle(new SolidBrush(e.BackColor), e.Bounds);
            }

            bool valid = isSecondary ? SecondGump.IsValidIndex(i) : Gumps.IsValidIndex(i);
            Brush fontBrush = Brushes.Gray;

            if (valid)
            {
                Bitmap bmp = isSecondary ? SecondGump.GetGump(i) : Gumps.GetGump(i);
                if (bmp != null)
                {
                    if (tileView2.VirtualListSize > 0 && !Compare(i))
                    {
                        fontBrush = Brushes.Blue;
                    }

                    int width  = bmp.Width  > 80 ? 80 : bmp.Width;
                    int height = bmp.Height > 54 ? 54 : bmp.Height;
                    e.Graphics.DrawImage(bmp, new Rectangle(e.Bounds.X + 3, e.Bounds.Y + 3, width, height));
                }
                else
                {
                    fontBrush = Brushes.Red;
                }
            }
            else
            {
                fontBrush = Brushes.Red;
            }

            string label = $"0x{i:X}";
            float y = e.Bounds.Y + (e.Bounds.Height - e.Graphics.MeasureString(label, Font).Height) / 2f;
            e.Graphics.DrawString(label, Font, fontBrush, new PointF(85, y));
        }

        private void OnFocusChanged1(object sender, TileViewControl.ListViewFocusedItemSelectionChangedEventArgs e)
        {
            if (e.FocusedItemIndex < 0)
            {
                return;
            }

            int i = _displayIndices[e.FocusedItemIndex];

            if (tileView2.VirtualListSize > 0)
            {
                if (_syncingSelection)
                {
                    return;
                }

                _syncingSelection = true;
                try { tileView2.FocusIndex = e.FocusedItemIndex; }
                finally { _syncingSelection = false; }
            }

            UpdatePictureBox(pictureBox1, i, isSecondary: false);
            UpdatePictureBox(pictureBox2, i, isSecondary: true);
        }

        private void OnFocusChanged2(object sender, TileViewControl.ListViewFocusedItemSelectionChangedEventArgs e)
        {
            if (e.FocusedItemIndex < 0)
            {
                return;
            }

            int i = _displayIndices[e.FocusedItemIndex];

            if (_syncingSelection)
            {
                return;
            }

            _syncingSelection = true;
            try { tileView1.FocusIndex = e.FocusedItemIndex; }
            finally { _syncingSelection = false; }

            UpdatePictureBox(pictureBox1, i, isSecondary: false);
            UpdatePictureBox(pictureBox2, i, isSecondary: true);
        }

        private void UpdatePictureBox(PictureBox box, int i, bool isSecondary)
        {
            bool valid = isSecondary ? SecondGump.IsValidIndex(i) : Gumps.IsValidIndex(i);
            if (valid)
            {
                Bitmap bmp = isSecondary ? SecondGump.GetGump(i) : Gumps.GetGump(i);
                box.BackgroundImage = bmp;
            }
            else
            {
                box.BackgroundImage = null;
            }
        }

        private void Browse_OnClick(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory containing the gump files";
                dialog.ShowNewFolderButton = false;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    textBoxSecondDir.Text = dialog.SelectedPath;
                }
            }
        }

        private void Load_Click(object sender, EventArgs e)
        {
            if (textBoxSecondDir.Text == null)
            {
                return;
            }

            string path  = textBoxSecondDir.Text;
            string file  = Path.Combine(path, "gumpart.mul");
            string file2 = Path.Combine(path, "gumpidx.mul");
            if (File.Exists(file) && File.Exists(file2))
            {
                SecondGump.SetFileIndex(file2, file);
                LoadSecond();
            }
        }

        private void LoadSecond()
        {
            _compare.Clear();
            tileView2.VirtualListSize = _displayIndices.Count;
            tileView1.Invalidate();
        }

        private bool Compare(int index)
        {
            if (_compare.TryGetValue(index, out bool value))
            {
                return value;
            }

            byte[] org = Gumps.GetRawGump(index, out int width1, out int height1);
            byte[] sec = SecondGump.GetRawGump(index, out int width2, out int height2);
            bool res;

            if (org == null && sec == null)
            {
                res = true;
            }
            else if (org == null || sec == null || org.Length != sec.Length)
            {
                res = false;
            }
            else if (width1 != width2 || height1 != height2)
            {
                res = false;
            }
            else
            {
                string hash1 = BitConverter.ToString(_sha256.ComputeHash(org));
                string hash2 = BitConverter.ToString(_sha256.ComputeHash(sec));
                res = hash1 == hash2;
            }

            _compare[index] = res;
            return res;
        }

        private void ShowDiff_OnClick(object sender, EventArgs e)
        {
            if (tileView2.VirtualListSize == 0)
            {
                if (checkBox1.Checked)
                {
                    MessageBox.Show("Second Gump file is not loaded!");
                    checkBox1.Checked = false;
                }
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            _displayIndices.Clear();
            if (checkBox1.Checked)
            {
                for (int i = 0; i < 0x10000; i++)
                {
                    if (!Compare(i))
                    {
                        _displayIndices.Add(i);
                    }
                }
            }
            else
            {
                for (int i = 0; i < 0x10000; i++)
                {
                    _displayIndices.Add(i);
                }
            }

            tileView1.VirtualListSize = _displayIndices.Count;
            tileView2.VirtualListSize = _displayIndices.Count;
            Cursor.Current = Cursors.Default;
        }

        private void Export_Bmp(object sender, EventArgs e)
        {
            int focusIdx = tileView2.FocusIndex;
            if (focusIdx < 0)
            {
                return;
            }

            int i = _displayIndices[focusIdx];
            if (!SecondGump.IsValidIndex(i))
            {
                return;
            }

            string path     = Options.OutputPath;
            string fileName = Path.Combine(path, $"Gump(Sec) 0x{i:X}.bmp");
            SecondGump.GetGump(i).Save(fileName, ImageFormat.Bmp);
            MessageBox.Show($"Gump saved to {fileName}", "Saved",
                MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void Export_Tiff(object sender, EventArgs e)
        {
            int focusIdx = tileView2.FocusIndex;
            if (focusIdx < 0)
            {
                return;
            }

            int i = _displayIndices[focusIdx];
            if (!SecondGump.IsValidIndex(i))
            {
                return;
            }

            string path     = Options.OutputPath;
            string fileName = Path.Combine(path, $"Gump(Sec) 0x{i:X}.tiff");
            SecondGump.GetGump(i).Save(fileName, ImageFormat.Tiff);
            MessageBox.Show($"Gump saved to {fileName}", "Saved",
                MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void OnClickCopy(object sender, EventArgs e)
        {
            int focusIdx = tileView2.FocusIndex;
            if (focusIdx < 0)
            {
                return;
            }

            int i = _displayIndices[focusIdx];
            if (!SecondGump.IsValidIndex(i))
            {
                return;
            }

            Bitmap copy = new Bitmap(SecondGump.GetGump(i));
            Gumps.ReplaceGump(i, copy);
            Options.ChangedUltimaClass["Gumps"] = true;
            ControlEvents.FireGumpChangeEvent(this, i);
            _compare[i] = true;

            tileView1.Invalidate();
            tileView2.Invalidate();

            UpdatePictureBox(pictureBox1, i, isSecondary: false);
        }
    }
}
