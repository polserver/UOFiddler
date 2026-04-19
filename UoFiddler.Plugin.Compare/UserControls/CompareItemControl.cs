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
    public partial class CompareItemControl : UserControl
    {
        public CompareItemControl()
        {
            InitializeComponent();
        }

        private readonly Dictionary<int, bool> _compare = new Dictionary<int, bool>();
        private readonly ImageConverter _ic = new ImageConverter();
        private readonly SHA256 _sha256 = SHA256.Create();
        private readonly List<int> _displayIndices = new List<int>();
        private bool _syncingSelection;
        private bool _secondLoaded;

        private void OnLoad(object sender, EventArgs e)
        {
            _displayIndices.Clear();
            int count = Art.GetMaxItemId() + 1;
            for (int i = 0; i < count; i++)
            {
                _displayIndices.Add(i);
            }

            tileViewOrg.VirtualListSize = _displayIndices.Count;
            tileViewSec.VirtualListSize = 0;

            SecondArt.FileIndexChanged += OnSecondArtChanged;
            ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
        }

        private void OnFilePathChangeEvent()
        {
            _compare.Clear();
            tileViewOrg.Invalidate();
            tileViewSec.Invalidate();
        }

        private void OnSecondArtChanged()
        {
            if (!_secondLoaded)
            {
                return;
            }

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
            bool valid = isSecondary ? SecondArt.IsValidStatic(i) : Art.IsValidStatic(i);

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
                try
                {
                    int secIdx = _displayIndices.IndexOf(i);
                    if (secIdx >= 0 && secIdx < tileViewSec.VirtualListSize)
                    {
                        tileViewSec.FocusIndex = secIdx;
                    }
                }
                finally
                {
                    _syncingSelection = false;
                }
            }

            pictureBoxOrg.BackgroundImage = Art.IsValidStatic(i) ? Art.GetStatic(i) : null;
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
            try
            {
                tileViewOrg.FocusIndex = e.FocusedItemIndex;
            }
            finally
            {
                _syncingSelection = false;
            }

            pictureBoxSec.BackgroundImage = SecondArt.IsValidStatic(i) ? SecondArt.GetStatic(i) : null;
            tileViewSec.Invalidate();
        }

        private void OnClickLoadSecond(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxSecondDir.Text))
            {
                return;
            }

            string path = textBoxSecondDir.Text;
            string file = Path.Combine(path, "art.mul");
            string file2 = Path.Combine(path, "artidx.mul");
            if (File.Exists(file) && File.Exists(file2))
            {
                SecondArt.SetFileIndex(file2, file);
                LoadSecond();
            }
        }

        private void LoadSecond()
        {
            _secondLoaded = true;
            _compare.Clear();
            int secMax = SecondArt.GetMaxItemId() + 1;
            if (secMax > _displayIndices.Count)
            {
                for (int i = _displayIndices.Count; i < secMax; i++)
                {
                    _displayIndices.Add(i);
                }

                tileViewOrg.VirtualListSize = _displayIndices.Count;
            }
            tileViewSec.VirtualListSize = _displayIndices.Count;
            tileViewOrg.Invalidate();
        }

        private bool Compare(int index)
        {
            if (_compare.ContainsKey(index))
            {
                return _compare[index];
            }

            Bitmap bitorg = Art.GetStatic(index);
            Bitmap bitsec = SecondArt.GetStatic(index);
            if (bitorg == null && bitsec == null)
            {
                _compare[index] = true;
                return true;
            }

            if (bitorg == null || bitsec == null || bitorg.Size != bitsec.Size)
            {
                _compare[index] = false;
                return false;
            }

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
                    MessageBox.Show("Second Item file is not loaded!");
                    checkBox1.Checked = false;
                }
                return;
            }

            int maxId = Math.Max(Art.GetMaxItemId(), SecondArt.GetMaxItemId());
            _displayIndices.Clear();
            if (checkBox1.Checked)
            {
                for (int i = 0; i < maxId; i++)
                {
                    if (!Compare(i))
                    {
                        _displayIndices.Add(i);
                    }
                }
            }
            else
            {
                for (int i = 0; i < maxId; i++)
                {
                    _displayIndices.Add(i);
                }
            }

            tileViewOrg.VirtualListSize = _displayIndices.Count;
            tileViewSec.VirtualListSize = _displayIndices.Count;
        }

        private void ExportAsBmp(object sender, EventArgs e)
        {
            int focusIdx = tileViewSec.FocusIndex;
            if (focusIdx < 0)
            {
                return;
            }

            int i = _displayIndices[focusIdx];
            if (!SecondArt.IsValidStatic(i))
            {
                return;
            }

            string fileName = Path.Combine(Options.OutputPath, $"Item(Sec) 0x{i:X}.bmp");
            SecondArt.GetStatic(i).Save(fileName, ImageFormat.Bmp);
            MessageBox.Show($"Item saved to {fileName}", "Saved",
                MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void ExportAsTiff(object sender, EventArgs e)
        {
            int focusIdx = tileViewSec.FocusIndex;
            if (focusIdx < 0)
            {
                return;
            }

            int i = _displayIndices[focusIdx];
            if (!SecondArt.IsValidStatic(i))
            {
                return;
            }

            string fileName = Path.Combine(Options.OutputPath, $"Item(Sec) 0x{i:X}.tiff");
            SecondArt.GetStatic(i).Save(fileName, ImageFormat.Tiff);
            MessageBox.Show($"Item saved to {fileName}", "Saved",
                MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void OnClickCopy(object sender, EventArgs e)
        {
            int focusIdx = tileViewSec.FocusIndex;
            if (focusIdx < 0)
            {
                return;
            }

            int i = _displayIndices[focusIdx];
            if (!SecondArt.IsValidStatic(i))
            {
                return;
            }

            if (i >= Art.GetMaxItemId() + 1)
            {
                return;
            }

            Bitmap copy = new Bitmap(SecondArt.GetStatic(i));
            Art.ReplaceStatic(i, copy);
            Options.ChangedUltimaClass["Art"] = true;
            ControlEvents.FireItemChangeEvent(this, i);
            _compare[i] = true;

            if (checkBox1.Checked)
            {
                _displayIndices.RemoveAt(focusIdx);
                tileViewOrg.VirtualListSize = _displayIndices.Count;
                tileViewSec.VirtualListSize = _displayIndices.Count;
            }

            tileViewOrg.Invalidate();
            tileViewSec.Invalidate();
            pictureBoxOrg.BackgroundImage = Art.IsValidStatic(i) ? Art.GetStatic(i) : null;
        }

        private void OnDoubleClickSec(object sender, MouseEventArgs e)
        {
            OnClickCopy(sender, e);
        }

        private void OnClickCopyAllDiff(object sender, EventArgs e)
        {
            if (!_secondLoaded)
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            int maxId = Art.GetMaxItemId() + 1;
            for (int i = 0; i < maxId; i++)
            {
                if (!SecondArt.IsValidStatic(i) || Compare(i))
                {
                    continue;
                }

                Bitmap copy = new Bitmap(SecondArt.GetStatic(i));
                Art.ReplaceStatic(i, copy);
                ControlEvents.FireItemChangeEvent(this, i);
                _compare[i] = true;
            }

            Options.ChangedUltimaClass["Art"] = true;

            if (checkBox1.Checked)
            {
                _displayIndices.Clear();
                for (int i = 0; i < maxId; i++)
                {
                    if (!Compare(i))
                    {
                        _displayIndices.Add(i);
                    }
                }
                tileViewOrg.VirtualListSize = _displayIndices.Count;
                tileViewSec.VirtualListSize = _displayIndices.Count;
            }

            tileViewOrg.Invalidate();
            tileViewSec.Invalidate();
            Cursor.Current = Cursors.Default;
        }

        private void OnClickBrowse(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory containing the art files";
                dialog.ShowNewFolderButton = false;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    textBoxSecondDir.Text = dialog.SelectedPath;
                }
            }
        }
    }
}
