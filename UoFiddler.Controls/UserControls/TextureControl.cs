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
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;
using UoFiddler.Controls.Helpers;

namespace UoFiddler.Controls.UserControls
{
    public partial class TextureControl : UserControl
    {
        public TextureControl()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            _refMarker = this;
        }

        private static TextureControl _refMarker;
        private bool _loaded;

        private void Reload()
        {
            if (_loaded)
            {
                OnLoad(this, EventArgs.Empty);
            }
        }

        public static bool SearchGraphic(int graphic)
        {
            const int index = 0;

            for (int i = index; i < _refMarker.listView1.Items.Count; ++i)
            {
                ListViewItem item = _refMarker.listView1.Items[i];
                if ((int)item.Tag != graphic)
                {
                    continue;
                }

                if (_refMarker.listView1.SelectedItems.Count == 1)
                {
                    _refMarker.listView1.SelectedItems[0].Selected = false;
                }

                item.Selected = true;
                item.Focused = true;
                item.EnsureVisible();
                return true;
            }
            return false;
        }

        private void DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            int i = (int)e.Item.Tag;

            Bitmap bmp = Textures.GetTexture(i, out bool patched);

            if (bmp == null)
            {
                return;
            }

            int width = bmp.Width;
            int height = bmp.Height;

            if (width >= e.Bounds.Width)
            {
                width = e.Bounds.Width - 2;
            }

            if (height >= e.Bounds.Height)
            {
                height = e.Bounds.Height - 2;
            }

            e.Graphics.DrawImage(bmp, new Rectangle(e.Bounds.X + 1, e.Bounds.Y + 1, width, height));

            if (listView1.SelectedItems.Contains(e.Item))
            {
                e.DrawFocusRectangle();
            }
            else if (patched)
            {
                e.Graphics.DrawRectangle(Pens.LightCoral, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
            }
            else
            {
                e.Graphics.DrawRectangle(Pens.Gray, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
            }
        }

        private void ListView_SelectedIndexChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                GraphicLabel.Text = string.Format("Graphic: 0x{0:X4} ({0}) [{1}x{1}]", (int)listView1.SelectedItems[0].Tag, Textures.GetTexture((int)listView1.SelectedItems[0].Tag).Width);
            }
        }

        private void OnLoad(object sender, EventArgs e)
        {
            if (FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            Options.LoadedUltimaClass["Texture"] = true;

            listView1.BeginUpdate();
            listView1.Clear();
            var itemCache = new List<ListViewItem>();

            for (int i = 0; i < Textures.GetIdxLength(); ++i)
            {
                if (!Textures.TestTexture(i))
                {
                    continue;
                }

                ListViewItem item = new ListViewItem(i.ToString(), 0)
                {
                    Tag = i
                };
                itemCache.Add(item);
            }
            listView1.Items.AddRange(itemCache.ToArray());
            listView1.TileSize = new Size(64, 64);
            listView1.EndUpdate();
            if (!_loaded)
            {
                ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
                ControlEvents.TextureChangeEvent += OnTextureChangeEvent;
            }
            _loaded = true;
            Cursor.Current = Cursors.Default;
        }

        private void OnFilePathChangeEvent()
        {
            if (!Options.DesignAlternative)
            {
                Reload();
            }
        }

        private void OnTextureChangeEvent(object sender, int index)
        {
            if (Options.DesignAlternative)
            {
                return;
            }

            if (!_loaded)
            {
                return;
            }

            if (sender.Equals(this))
            {
                return;
            }

            if (Textures.TestTexture(index))
            {
                ListViewItem item = new ListViewItem(index.ToString(), 0)
                {
                    Tag = index
                };
                bool done = false;
                foreach (ListViewItem i in listView1.Items)
                {
                    if ((int)i.Tag > index)
                    {
                        listView1.Items.Insert(i.Index, item);
                        done = true;
                        break;
                    }

                    if ((int)i.Tag != index)
                    {
                        continue;
                    }

                    done = true;
                    break;
                }
                if (!done)
                {
                    listView1.Items.Add(item);
                }

                listView1.View = View.Details; // that works fascinating
                listView1.View = View.Tile;
            }
            else
            {
                foreach (ListViewItem i in listView1.Items)
                {
                    if ((int)i.Tag != index)
                    {
                        continue;
                    }

                    listView1.Items.RemoveAt(i.Index);
                    break;
                }
                listView1.Invalidate();
            }
        }

        private TextureSearchForm _showForm;

        private void OnClickSearch(object sender, EventArgs e)
        {
            if (_showForm?.IsDisposed == false)
            {
                return;
            }

            _showForm = new TextureSearchForm
            {
                TopMost = true
            };
            _showForm.Show();
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Textures.Save(Options.OutputPath);
            Cursor.Current = Cursors.Default;
            MessageBox.Show($"Saved to {Options.OutputPath}", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
            Options.ChangedUltimaClass["Texture"] = false;
        }

        private void OnClickFindNext(object sender, EventArgs e)
        {
            int id, i;
            if (listView1.SelectedItems.Count > 0)
            {
                id = (int)listView1.SelectedItems[0].Tag + 1;
                i = listView1.SelectedItems[0].Index + 1;
            }
            else
            {
                id = 1;
                i = 0;
            }

            for (; i < listView1.Items.Count; ++i, ++id)
            {
                if (id >= (int)listView1.Items[i].Tag)
                {
                    continue;
                }

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

        private void OnClickRemove(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                return;
            }

            int i = (int)listView1.SelectedItems[0].Tag;
            DialogResult result = MessageBox.Show($"Are you sure to remove 0x{i:X}", "Save", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return;
            }

            Textures.Remove(i);
            ControlEvents.FireTextureChangeEvent(this, i);
            i = listView1.SelectedItems[0].Index;
            listView1.SelectedItems[0].Selected = false;
            listView1.Items.RemoveAt(i);
            listView1.Invalidate();
            Options.ChangedUltimaClass["Texture"] = true;
        }

        private void OnClickReplace(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count != 1)
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
                if ((bmp.Width == 64 && bmp.Height == 64) || (bmp.Width == 128 && bmp.Height == 128))
                {
                    if (dialog.FileName.Contains(".bmp"))
                    {
                        bmp = Utils.ConvertBmp(bmp);
                    }

                    int i = (int)listView1.SelectedItems[0].Tag;
                    Textures.Replace(i, bmp);
                    ControlEvents.FireTextureChangeEvent(this, i);
                    listView1.Invalidate();
                    ListView_SelectedIndexChanged(this, null);
                    Options.ChangedUltimaClass["Texture"] = true;
                }
                else
                {
                    MessageBox.Show("Height or Width Invalid", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1);
                }
            }
        }

        private void OnTextChangedInsert(object sender, EventArgs e)
        {
            if (Utils.ConvertStringToInt(InsertText.Text, out int index, 0, Textures.GetIdxLength()))
            {
                InsertText.ForeColor = Textures.TestTexture(index) ? Color.Red : Color.Black;
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

            if (!Utils.ConvertStringToInt(InsertText.Text, out int index, 0, Textures.GetIdxLength()))
            {
                return;
            }

            if (Textures.TestTexture(index))
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
                if ((bmp.Width == 64 && bmp.Height == 64) || (bmp.Width == 128 && bmp.Height == 128))
                {
                    Textures.Replace(index, bmp);
                    ControlEvents.FireTextureChangeEvent(this, index);
                    ListViewItem item = new ListViewItem(index.ToString(), 0)
                    {
                        Tag = index
                    };
                    bool done = false;
                    foreach (ListViewItem i in listView1.Items)
                    {
                        if ((int)i.Tag <= index)
                        {
                            continue;
                        }

                        listView1.Items.Insert(i.Index, item);
                        done = true;
                        break;
                    }
                    if (!done)
                    {
                        listView1.Items.Add(item);
                    }

                    listView1.View = View.Details; // that works fascinating
                    listView1.View = View.Tile;

                    if (listView1.SelectedItems.Count == 1)
                    {
                        listView1.SelectedItems[0].Selected = false;
                    }

                    item.Selected = true;
                    item.Focused = true;
                    item.EnsureVisible();
                    Options.ChangedUltimaClass["Texture"] = true;
                }
                else
                {
                    MessageBox.Show("Height or Width Invalid", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1);
                }
            }
        }

        private void OnClickExportBmp(object sender, EventArgs e)
        {
            int i = (int)listView1.SelectedItems[0].Tag;
            ExportTextureImage(i, ImageFormat.Bmp);
        }

        private void OnClickExportTiff(object sender, EventArgs e)
        {
            int i = (int)listView1.SelectedItems[0].Tag;
            ExportTextureImage(i, ImageFormat.Tiff);
        }

        private void OnClickExportJpg(object sender, EventArgs e)
        {
            int i = (int)listView1.SelectedItems[0].Tag;
            ExportTextureImage(i, ImageFormat.Jpeg);
        }

        private void OnClickExportPng(object sender, EventArgs e)
        {
            int i = (int)listView1.SelectedItems[0].Tag;
            ExportTextureImage(i, ImageFormat.Png);
        }

        private void ExportTextureImage(int index, ImageFormat imageFormat)
        {
            string fileExtension = Utils.GetFileExtensionFor(imageFormat);
            string fileName = Path.Combine(Options.OutputPath, $"Texture 0x{index:X4}.{fileExtension}");

            using (Bitmap bit = new Bitmap(Textures.GetTexture(index)))
            {
                bit.Save(fileName, imageFormat);
            }

            MessageBox.Show($"Texture saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void Texture_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.F || !e.Control)
            {
                return;
            }

            OnClickSearch(sender, e);
            e.SuppressKeyPress = true;
            e.Handled = true;
        }

        private void ExportAllAsBmp_Click(object sender, EventArgs e)
        {
            ExportAllTextures(ImageFormat.Bmp);
        }

        private void ExportAllAsTiff_Click(object sender, EventArgs e)
        {
            ExportAllTextures(ImageFormat.Tiff);
        }

        private void ExportAllAsJpeg_Click(object sender, EventArgs e)
        {
            ExportAllTextures(ImageFormat.Jpeg);
        }

        private void ExportAllAsPng_Click(object sender, EventArgs e)
        {
            ExportAllTextures(ImageFormat.Png);
        }

        private void ExportAllTextures(ImageFormat imageFormat)
        {
            string fileExtension = Utils.GetFileExtensionFor(imageFormat);

            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                Cursor.Current = Cursors.WaitCursor;

                for (int i = 0; i < listView1.Items.Count; ++i)
                {
                    int index = (int)listView1.Items[i].Tag;
                    if (index < 0)
                    {
                        continue;
                    }

                    string fileName = Path.Combine(dialog.SelectedPath, $"Texture 0x{index:X4}.{fileExtension}");
                    using (Bitmap bit = new Bitmap(Textures.GetTexture(index)))
                    {
                        bit.Save(fileName, imageFormat);
                    }
                }

                Cursor.Current = Cursors.Default;

                MessageBox.Show($"All textures saved to {dialog.SelectedPath}", "Saved", MessageBoxButtons.OK,
                    MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
        }

        private void StartingPosTb_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            if (!Utils.ConvertStringToInt(StartingPosTb.Text, out int index, 0, Textures.GetIdxLength()))
            {
                return;
            }

            contextMenuStrip1.Close();
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = true;
                dialog.Title = $"Choose image file to insert at 0x{index:X}";
                dialog.CheckFileExists = true;
                dialog.Filter = "Image files (*.tif;*.tiff;*.bmp)|*.tif;*.tiff;*.bmp";
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                var fileCount = dialog.FileNames.Length;

                if (CheckForIndexes(index, fileCount))
                {
                    for (int i = 0; i < fileCount; i++)
                    {
                        AddSingleTexture(dialog.FileNames[i], index + i);
                    }
                }

                listView1.View = View.Details; // that works fascinating
                listView1.View = View.Tile;

                if (listView1.SelectedItems.Count == 1)
                {
                    listView1.SelectedItems[0].Selected = false;
                }
                Options.ChangedUltimaClass["Texture"] = true;
            }
        }

        /// <summary>
        /// Check if all the indexes from baseIndex to baseIndex + count are valid
        /// </summary>
        /// <param name="baseIndex">Starting Index</param>
        /// <param name="count">Number of the indexes to check.</param>
        /// <returns></returns>
        private bool CheckForIndexes(int baseIndex, int count)
        {
            for (int i = baseIndex; i < baseIndex + count; i++)
            {
                if (Textures.TestTexture(i))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Adds a single Texture.
        /// </summary>
        /// <param name="fileName">Filename of the image to add.</param>
        /// <param name="index">Index where the texture will be added.</param>
        private void AddSingleTexture(string fileName, int index)
        {
            Bitmap bmp = new Bitmap(fileName);
            if ((bmp.Width == 64 && bmp.Height == 64) || (bmp.Width == 128 && bmp.Height == 128))
            {
                Textures.Replace(index, bmp);
                ControlEvents.FireTextureChangeEvent(this, index);
                ListViewItem item = new ListViewItem(index.ToString(), 0)
                {
                    Tag = index
                };
                bool done = false;
                foreach (ListViewItem i in listView1.Items)
                {
                    if ((int)i.Tag <= index)
                    {
                        continue;
                    }

                    listView1.Items.Insert(i.Index, item);
                    done = true;
                    break;
                }
                if (!done)
                {
                    listView1.Items.Add(item);
                }
            }
            else
            {
                MessageBox.Show("Height or Width Invalid", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1);
                return;
            }
        }
    }
}