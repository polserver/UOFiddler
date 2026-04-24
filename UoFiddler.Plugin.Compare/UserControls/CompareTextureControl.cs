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
using UoFiddler.Controls.Forms;
using UoFiddler.Controls.UserControls.TileView;
using UoFiddler.Plugin.Compare.Classes;

namespace UoFiddler.Plugin.Compare.UserControls
{
    public partial class CompareTextureControl : UserControl
    {
        public CompareTextureControl()
        {
            InitializeComponent();
        }

        private readonly Dictionary<int, bool> _compare = new Dictionary<int, bool>();
        private readonly SHA256 _sha256 = SHA256.Create();
        private readonly ImageConverter _ic = new ImageConverter();
        private readonly List<int> _displayIndices = new List<int>();
        private bool _syncingSelection;
        private bool _secondLoaded;

        private void OnLoad(object sender, EventArgs e)
        {
            _displayIndices.Clear();
            for (int i = 0; i < 0x4000; i++)
            {
                _displayIndices.Add(i);
            }

            tileViewOrg.VirtualListSize = _displayIndices.Count;
            tileViewSec.VirtualListSize = 0;

            ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
        }

        private void OnFilePathChangeEvent()
        {
            _compare.Clear();
            tileViewOrg.Invalidate();
            tileViewSec.Invalidate();
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

        private void OnDrawItemOrg(object sender, TileViewControl.DrawTileListItemEventArgs e)
        {
            DrawListItem(e, _displayIndices[e.Index], isSecondary: false);
        }

        private void OnDrawItemSec(object sender, TileViewControl.DrawTileListItemEventArgs e)
        {
            DrawListItem(e, _displayIndices[e.Index], isSecondary: true);
        }

        private void DrawListItem(DrawItemEventArgs e, int i, bool isSecondary)
        {
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                e.Graphics.FillRectangle(Brushes.LightSteelBlue, e.Bounds);
            }
            else
            {
                e.Graphics.FillRectangle(new SolidBrush(e.BackColor), e.Bounds);
            }

            Brush fontBrush = Brushes.Gray;
            bool valid = isSecondary ? SecondTexture.IsValidTexture(i) : Textures.TestTexture(i);

            if (!valid)
            {
                fontBrush = Brushes.Red;
            }
            else if (tileViewSec.VirtualListSize > 0 && !Compare(i))
            {
                fontBrush = Brushes.Blue;
            }

            string label = $"0x{i:X}";
            float y = e.Bounds.Y + (e.Bounds.Height - e.Graphics.MeasureString(label, Font).Height) / 2f;
            e.Graphics.DrawString(label, Font, fontBrush, new PointF(5, y));
        }

        private void OnFocusChangedOrg(object sender, TileViewControl.ListViewFocusedItemSelectionChangedEventArgs e)
        {
            if (e.FocusedItemIndex < 0)
            {
                return;
            }

            int i = _displayIndices[e.FocusedItemIndex];

            if (tileViewSec.VirtualListSize > 0)
            {
                if (_syncingSelection)
                {
                    return;
                }

                _syncingSelection = true;
                try { tileViewSec.FocusIndex = e.FocusedItemIndex; }
                finally { _syncingSelection = false; }
            }

            pictureBoxOrg.BackgroundImage = Textures.TestTexture(i) ? Textures.GetTexture(i) : null;
            pictureBoxSec.BackgroundImage = SecondTexture.IsValidTexture(i) ? SecondTexture.GetTexture(i) : null;
            tileViewOrg.Invalidate();
        }

        private void OnFocusChangedSec(object sender, TileViewControl.ListViewFocusedItemSelectionChangedEventArgs e)
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
            try { tileViewOrg.FocusIndex = e.FocusedItemIndex; }
            finally { _syncingSelection = false; }

            pictureBoxOrg.BackgroundImage = Textures.TestTexture(i) ? Textures.GetTexture(i) : null;
            pictureBoxSec.BackgroundImage = SecondTexture.IsValidTexture(i) ? SecondTexture.GetTexture(i) : null;
            tileViewSec.Invalidate();
        }

        private void OnClickLoadSecond(object sender, EventArgs e)
        {
            if (textBoxSecondDir.Text == null)
            {
                return;
            }

            string path  = textBoxSecondDir.Text;
            string file  = Path.Combine(path, "texmaps.mul");
            string file2 = Path.Combine(path, "texidx.mul");
            if (File.Exists(file) && File.Exists(file2))
            {
                SecondTexture.SetFileIndex(file2, file);
                LoadSecond();
            }
        }

        private void LoadSecond()
        {
            _secondLoaded = true;
            _compare.Clear();
            tileViewSec.VirtualListSize = _displayIndices.Count;
            tileViewOrg.Invalidate();
        }

        private bool Compare(int index)
        {
            if (_compare.ContainsKey(index))
            {
                return _compare[index];
            }

            Bitmap bitorg = Textures.GetTexture(index);
            Bitmap bitsec = SecondTexture.GetTexture(index);
            if (bitorg == null && bitsec == null) { _compare[index] = true;  return true; }
            if (bitorg == null || bitsec == null || bitorg.Size != bitsec.Size) { _compare[index] = false; return false; }

            byte[] b1 = (byte[])_ic.ConvertTo(bitorg, typeof(byte[]));
            byte[] b2 = (byte[])_ic.ConvertTo(bitsec, typeof(byte[]));
            bool res = BitConverter.ToString(_sha256.ComputeHash(b1)) == BitConverter.ToString(_sha256.ComputeHash(b2));
            _compare[index] = res;
            return res;
        }

        private void OnChangeShowDiff(object sender, EventArgs e)
        {
            if (!_secondLoaded)
            {
                if (checkBox1.Checked)
                {
                    MessageBox.Show("Second Texture file is not loaded!");
                    checkBox1.Checked = false;
                }
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            _displayIndices.Clear();
            if (checkBox1.Checked)
            {
                for (int i = 0; i < 0x4000; i++)
                {
                    if (!Compare(i))
                    {
                        _displayIndices.Add(i);
                    }
                }
            }
            else
            {
                for (int i = 0; i < 0x4000; i++)
                {
                    _displayIndices.Add(i);
                }
            }

            tileViewOrg.VirtualListSize = _displayIndices.Count;
            tileViewSec.VirtualListSize = _displayIndices.Count;
            Cursor.Current = Cursors.Default;
        }

        private void ExportAsBmp(object sender, EventArgs e)
        {
            int focusIdx = tileViewSec.FocusIndex;
            if (focusIdx < 0)
            {
                return;
            }

            int i = _displayIndices[focusIdx];
            if (!SecondTexture.IsValidTexture(i))
            {
                return;
            }

            string fileName = Path.Combine(Options.OutputPath, $"Texture(Sec) 0x{i:X}.bmp");
            SecondTexture.GetTexture(i).Save(fileName, ImageFormat.Bmp);
            FileSavedDialog.Show(FindForm(), fileName, "Texture saved successfully.");
        }

        private void ExportAsTiff(object sender, EventArgs e)
        {
            int focusIdx = tileViewSec.FocusIndex;
            if (focusIdx < 0)
            {
                return;
            }

            int i = _displayIndices[focusIdx];
            if (!SecondTexture.IsValidTexture(i))
            {
                return;
            }

            string fileName = Path.Combine(Options.OutputPath, $"Texture(Sec) 0x{i:X}.tiff");
            SecondTexture.GetTexture(i).Save(fileName, ImageFormat.Tiff);
            FileSavedDialog.Show(FindForm(), fileName, "Texture saved successfully.");
        }

        private void BrowseOnClick(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory containing the texture files";
                dialog.ShowNewFolderButton = false;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    textBoxSecondDir.Text = dialog.SelectedPath;
                }
            }
        }

        private void OnClickCopy(object sender, EventArgs e)
        {
            int focusIdx = tileViewSec.FocusIndex;
            if (focusIdx < 0)
            {
                return;
            }

            int i = _displayIndices[focusIdx];
            if (!SecondTexture.IsValidTexture(i))
            {
                return;
            }

            Bitmap copy = new Bitmap(SecondTexture.GetTexture(i));
            Textures.Replace(i, copy);
            Options.ChangedUltimaClass["Texture"] = true;
            ControlEvents.FireTextureChangeEvent(this, i);
            _compare[i] = true;

            if (checkBox1.Checked)
            {
                _displayIndices.RemoveAt(focusIdx);
                tileViewOrg.VirtualListSize = _displayIndices.Count;
                tileViewSec.VirtualListSize = _displayIndices.Count;
            }

            tileViewOrg.Invalidate();
            tileViewSec.Invalidate();
            pictureBoxOrg.BackgroundImage = Textures.TestTexture(i) ? Textures.GetTexture(i) : null;
        }

        private void CopyToLeft_Click(object sender, MouseEventArgs e)
        {
            OnClickCopy(sender, e);
        }

        private void CopyAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 0x4000; i++)
            {
                if (!SecondTexture.IsValidTexture(i) || Compare(i))
                {
                    continue;
                }

                Bitmap copy = new Bitmap(SecondTexture.GetTexture(i));
                Textures.Replace(i, copy);
                ControlEvents.FireTextureChangeEvent(this, i);
            }

            Options.ChangedUltimaClass["Texture"] = true;
            _compare.Clear();

            if (checkBox1.Checked)
            {
                _displayIndices.Clear();
                for (int i = 0; i < 0x4000; i++)
                {
                    if (!Compare(i))
                    {
                        _displayIndices.Add(i);
                    }
                }
            }
            else
            {
                _displayIndices.Clear();
                for (int i = 0; i < 0x4000; i++)
                {
                    _displayIndices.Add(i);
                }
            }

            tileViewOrg.VirtualListSize = _displayIndices.Count;
            tileViewSec.VirtualListSize = _displayIndices.Count;
        }

        private void CopyAddOnly_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 0x4000; i++)
            {
                if (!SecondTexture.IsValidTexture(i) || Textures.TestTexture(i))
                {
                    continue;
                }

                Bitmap copy = new Bitmap(SecondTexture.GetTexture(i));
                Textures.Replace(i, copy);
                ControlEvents.FireTextureChangeEvent(this, i);
            }

            Options.ChangedUltimaClass["Texture"] = true;
            _compare.Clear();

            if (checkBox1.Checked)
            {
                _displayIndices.Clear();
                for (int i = 0; i < 0x4000; i++)
                {
                    if (!Compare(i))
                    {
                        _displayIndices.Add(i);
                    }
                }
            }
            else
            {
                _displayIndices.Clear();
                for (int i = 0; i < 0x4000; i++)
                {
                    _displayIndices.Add(i);
                }
            }

            tileViewOrg.VirtualListSize = _displayIndices.Count;
            tileViewSec.VirtualListSize = _displayIndices.Count;
        }
    }
}
