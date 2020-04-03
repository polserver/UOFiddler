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
    public partial class LandTiles : UserControl
    {
        public LandTiles()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            _refMarker = this;
        }

        private static LandTiles _refMarker;

        private bool IsLoaded { get; set; }
        private bool _showFreeSlots;

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

            const int index = 0;
            for (int i = index; i < _refMarker.listView1.Items.Count; ++i)
            {
                ListViewItem item = _refMarker.listView1.Items[i];
                if ((int)item.Tag != graphic && ((int)item.Tag != -1 || i != graphic))
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
                if (_refMarker.listView1.SelectedIndices.Count == 1)
                {
                    index = _refMarker.listView1.SelectedIndices[0] + 1;
                }

                if (index >= _refMarker.listView1.Items.Count)
                {
                    index = 0;
                }
            }

            Regex regex = new Regex(name, RegexOptions.IgnoreCase);
            for (int i = index; i < _refMarker.listView1.Items.Count; ++i)
            {
                ListViewItem item = _refMarker.listView1.Items[i];
                if ((int)item.Tag == -1)
                {
                    continue;
                }

                if (!regex.IsMatch(TileData.LandTable[(int)item.Tag].Name))
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

        private void OnLoad(object sender, EventArgs e)
        {
            if (IsLoaded && (!(e is MyEventArgs args) || args.Type != MyEventArgs.Types.ForceReload))
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            Options.LoadedUltimaClass["TileData"] = true;
            Options.LoadedUltimaClass["Art"] = true;

            listView1.BeginUpdate();
            listView1.Clear();
            var itemCache = new List<ListViewItem>();
            for (int i = 0; i < 0x4000; ++i)
            {
                if (!Art.IsValidLand(i))
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
            listView1.TileSize = new Size(49, 49);
            listView1.EndUpdate();
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
            if (!Options.DesignAlternative)
            {
                Reload();
            }
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

            if (id > 0x3FFF)
            {
                return;
            }

            if (listView1.SelectedItems.Count != 1)
            {
                return;
            }

            if ((int)listView1.SelectedItems[0].Tag != id)
            {
                return;
            }

            namelabel.Text = $"Name: {TileData.LandTable[id].Name}";
            FlagsLabel.Text = $"Flags: {TileData.LandTable[id].Flags}";
        }

        private void OnLandTileChangeEvent(object sender, int index)
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

            if (Art.IsValidLand(index))
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
                        if ((int)i.Tag != index)
                        {
                            continue;
                        }

                        listView1.Items.RemoveAt(i.Index);
                        break;
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
            if (listView1.SelectedItems.Count != 1)
            {
                return;
            }

            int i = (int)listView1.SelectedItems[0].Tag;
            if (i == -1)
            {
                namelabel.Text = "Name: FREE";
                graphiclabel.Text = string.Format("Graphic: 0x{0:X4} ({0})", listView1.SelectedIndices[0]);
                selectInTileDataTabToolStripMenuItem.Enabled = false;
            }
            else
            {
                namelabel.Text = $"Name: {TileData.LandTable[i].Name}";
                graphiclabel.Text = string.Format("ID: 0x{0:X4} ({0})", i);
                FlagsLabel.Text = $"Flags: {TileData.LandTable[i].Flags}";
            }
        }

        // TODO: unused brushes?
        //private static readonly Brush _brushWhite = Brushes.White;
        private static readonly Brush _brushLightBlue = Brushes.LightBlue;
        //private static readonly Brush _brushLightCoral = Brushes.LightCoral;
        private static readonly Brush _brushRed = Brushes.Red;
        private static readonly Pen _penGray = Pens.Gray;

        private void DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            int i = (int)listView1.Items[e.ItemIndex].Tag;
            if (i == -1)
            {
                if (e.Item.Selected)
                {
                    e.Graphics.FillRectangle(_brushLightBlue, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
                }
                else
                {
                    e.Graphics.DrawRectangle(_penGray, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
                }

                e.Graphics.FillRectangle(_brushRed, e.Bounds.X + 5, e.Bounds.Y + 5, e.Bounds.Width - 10, e.Bounds.Height - 10);
                return;
            }

            Bitmap bmp = Art.GetLand(i, out bool patched);
            if (bmp == null)
            {
                return;
            }

            if (listView1.SelectedItems.Contains(e.Item))
            {
                e.Graphics.FillRectangle(Brushes.LightBlue, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
            }
            else if (patched)
            {
                e.Graphics.FillRectangle(Brushes.LightCoral, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
            }
            else
            {
                e.Graphics.FillRectangle(Brushes.White, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
            }

            // TODO: unused variable? Why is it here?
            // int width = bmp.Width;
            // int height = bmp.Height;
            //
            // if (width > e.Bounds.Width)
            // {
            //     width = e.Bounds.Width - 2;
            // }
            //
            // if (height > e.Bounds.Height)
            // {
            //     height = e.Bounds.Height - 2;
            // }

            e.Graphics.DrawImage(bmp, e.Bounds.X + 1, e.Bounds.Y + 1,
                new Rectangle(0, 0, e.Bounds.Width - 1, e.Bounds.Height - 1),
                GraphicsUnit.Pixel);

            if (listView1.SelectedItems.Contains(e.Item))
            {
                e.DrawFocusRectangle();
            }
            else
            {
                e.Graphics.DrawRectangle(Pens.Gray, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
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
                if (bmp.Height != 44 || bmp.Width != 44)
                {
                    MessageBox.Show("Height or Width Invalid", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    return;
                }
                if (dialog.FileName.Contains(".bmp"))
                {
                    bmp = Utils.ConvertBmp(bmp);
                }

                int id = (int)listView1.SelectedItems[0].Tag;
                if (id == -1)
                {
                    listView1.SelectedItems[0].Tag = id = listView1.SelectedItems[0].Index;
                }

                Art.ReplaceLand(id, bmp);
                ControlEvents.FireLandTileChangeEvent(this, id);
                listView1.Invalidate();
                Options.ChangedUltimaClass["Art"] = true;
            }
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            DialogResult result =
                        MessageBox.Show("Are you sure? Will take a while", "Save",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes)
            {
                Cursor.Current = Cursors.WaitCursor;
                Art.Save(Options.OutputPath);
                Cursor.Current = Cursors.Default;
                MessageBox.Show(
                    $"Saved to {Options.OutputPath}",
                    "Save",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                Options.ChangedUltimaClass["Art"] = false;
            }
        }

        private void OnTextChanged_Insert(object sender, EventArgs e)
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

        private void OnKeyDown_Insert(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (Utils.ConvertStringToInt(InsertText.Text, out int index, 0, 0x3FFF))
                {
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
                        if (bmp.Height != 44 || bmp.Width != 44)
                        {
                            MessageBox.Show("Height or Width Invalid", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                            return;
                        }
                        if (dialog.FileName.Contains(".bmp"))
                        {
                            bmp = Utils.ConvertBmp(bmp);
                        }

                        Art.ReplaceLand(index, bmp);
                        ControlEvents.FireLandTileChangeEvent(this, index);
                        Options.ChangedUltimaClass["Art"] = true;
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

                        listView1.View = View.Details; // that works fascinating
                        listView1.View = View.Tile;

                        if (listView1.SelectedItems.Count == 1)
                        {
                            listView1.SelectedItems[0].Selected = false;
                        }

                        item.Selected = true;
                        item.Focused = true;
                        item.EnsureVisible();
                    }
                }
            }
        }

        private void OnClickRemove(object sender, EventArgs e)
        {
            int i = (int)listView1.SelectedItems[0].Tag;
            DialogResult result =
                        MessageBox.Show($"Are you sure to remove {i}", "Save",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return;
            }

            Art.RemoveLand(i);
            ControlEvents.FireLandTileChangeEvent(this, i);
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

        private void OnClickFindFree(object sender, EventArgs e)
        {
            if (_showFreeSlots)
            {
                int i = listView1.SelectedItems.Count > 0 ? listView1.SelectedItems[0].Index + 1 : 0;
                for (; i < listView1.Items.Count; ++i)
                {
                    if ((int)listView1.Items[i].Tag != -1)
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
        }

        private void OnClickShowFreeSlots(object sender, EventArgs e)
        {
            SuspendLayout();
            listView1.View = View.Details; // that works fascinating
            listView1.BeginUpdate();
            _showFreeSlots = !_showFreeSlots;
            if (_showFreeSlots)
            {
                for (int j = 0; j < 0x4000; ++j)
                {
                    ListViewItem item;
                    if (listView1.Items.Count > j)
                    {
                        if ((int)listView1.Items[j].Tag == j) continue;
                        item = new ListViewItem(j.ToString(), 0)
                        {
                            Tag = -1
                        };
                        listView1.Items.Insert(j, item);
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

        private void OnClickExportBmp(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count != 1)
            {
                return;
            }

            string path = Options.OutputPath;
            int i = (int)listView1.SelectedItems[0].Tag;
            if (i == -1)
            {
                return;
            }

            string fileName = Path.Combine(path, $"Landtile {i}.bmp");
            Bitmap bit = new Bitmap(Art.GetLand(i));
            bit.Save(fileName, ImageFormat.Bmp);
            bit.Dispose();
            MessageBox.Show($"Landtile saved to {fileName}", "Saved",
                MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void OnClickExportTiff(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count != 1)
            {
                return;
            }

            string path = Options.OutputPath;
            int i = (int)listView1.SelectedItems[0].Tag;
            if (i == -1)
            {
                return;
            }

            string fileName = Path.Combine(path, $"Landtile {i}.tiff");
            Bitmap bit = new Bitmap(Art.GetLand(i));
            bit.Save(fileName, ImageFormat.Tiff);
            bit.Dispose();
            MessageBox.Show($"Landtile saved to {fileName}", "Saved",
                MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void OnClickExportJpg(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count != 1)
            {
                return;
            }

            string path = Options.OutputPath;
            int i = (int)listView1.SelectedItems[0].Tag;
            if (i == -1)
            {
                return;
            }

            string fileName = Path.Combine(path, $"Landtile {i}.jpg");
            Bitmap bit = new Bitmap(Art.GetLand(i));
            bit.Save(fileName, ImageFormat.Jpeg);
            bit.Dispose();
            MessageBox.Show($"Landtile saved to {fileName}", "Saved",
                MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void OnClickSelectTiledata(object sender, EventArgs e)
        {
            int id = (int)listView1.SelectedItems[0].Tag;
            if (id == -1)
            {
                id = listView1.SelectedItems[0].Index;
            }

            TileDatas.Select(id, true);
        }

        private void OnClickSelectRadarCol(object sender, EventArgs e)
        {
            int id = (int)listView1.SelectedItems[0].Tag;
            if (id == -1)
            {
                id = listView1.SelectedItems[0].Index;
            }

            RadarColor.Select(id, true);
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

                for (int i = 0; i < listView1.Items.Count; ++i)
                {
                    int index = (int)listView1.Items[i].Tag;
                    if (index < 0)
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

                for (int i = 0; i < listView1.Items.Count; ++i)
                {
                    int index = (int)listView1.Items[i].Tag;
                    if (index < 0)
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

                for (int i = 0; i < listView1.Items.Count; ++i)
                {
                    int index = (int)listView1.Items[i].Tag;
                    if (index < 0)
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

        private void LandTiles_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.F || !e.Control)
            {
                return;
            }

            OnClickSearch(sender, e);
            e.SuppressKeyPress = true;
            e.Handled = true;
        }
    }
}
