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
    public partial class LandTiles : UserControl
    {
        public LandTiles()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            refMarker = this;
        }

        private static LandTiles refMarker = null;
        private bool Loaded = false;
        public bool isLoaded { get { return Loaded; } }

        /// <summary>
        /// Searches Objtype and Select
        /// </summary>
        /// <param name="graphic"></param>
        /// <returns></returns>
        public static bool SearchGraphic(int graphic)
        {
            if (!refMarker.isLoaded)
                refMarker.OnLoad(refMarker, EventArgs.Empty);
            int index = 0;
            for (int i = index; i < refMarker.listView1.Items.Count; ++i)
            {
                ListViewItem item = refMarker.listView1.Items[i];
                if ((int)item.Tag == graphic)
                {
                    if (refMarker.listView1.SelectedItems.Count == 1)
                        refMarker.listView1.SelectedItems[0].Selected = false;
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
                if (refMarker.listView1.SelectedIndices.Count == 1)
                    index = refMarker.listView1.SelectedIndices[0] + 1;
                if (index >= refMarker.listView1.Items.Count)
                    index = 0;
            }

            Regex regex = new Regex(@name, RegexOptions.IgnoreCase);
            for (int i = index; i < refMarker.listView1.Items.Count; ++i)
            {
                ListViewItem item = refMarker.listView1.Items[i];
                if (regex.IsMatch(TileData.LandTable[(int)item.Tag].Name))
                {
                    if (refMarker.listView1.SelectedItems.Count == 1)
                        refMarker.listView1.SelectedItems[0].Selected = false;
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

            listView1.BeginUpdate();
            listView1.Clear();
            List<ListViewItem> itemcache = new List<ListViewItem>();
            for (int i = 0; i < 0x4000; ++i)
            {
                if (Art.IsValidLand(i))
                {
                    ListViewItem item = new ListViewItem(i.ToString(), 0);
                    item.Tag = i;
                    itemcache.Add(item);
                }
            }
            listView1.Items.AddRange(itemcache.ToArray());
            listView1.TileSize = new Size(49, 49);
            listView1.EndUpdate();
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
            if (!FiddlerControls.Options.DesignAlternative)
                Reload();
        }

        void OnTileDataChangeEvent(object sender, int id)
        {
            if (FiddlerControls.Options.DesignAlternative)
                return;
            if (!Loaded)
                return;
            if (sender.Equals(this))
                return;
            if (id > 0x3FFF)
                return;
            if (listView1.SelectedItems.Count == 1)
            {
                if ((int)listView1.SelectedItems[0].Tag == id)
                {
                    namelabel.Text = String.Format("Name: {0}", TileData.LandTable[id].Name);
                    FlagsLabel.Text = String.Format("Flags: {0}", TileData.LandTable[id].Flags);
                }
            }
        }

        private void OnLandTileChangeEvent(object sender, int index)
        {
            if (FiddlerControls.Options.DesignAlternative)
                return;
            if (!Loaded)
                return;
            if (sender.Equals(this))
                return;
            if (Ultima.Art.IsValidLand(index))
            {
                ListViewItem item = new ListViewItem(index.ToString(), 0);
                item.Tag = index;
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
                    listView1.Items.Add(item);
                listView1.View = View.Details; // that works faszinating
                listView1.View = View.Tile;
            }
            else
            {
                foreach (ListViewItem i in listView1.Items)
                {
                    if ((int)i.Tag == index)
                    {
                        listView1.Items.RemoveAt(i.Index);
                        break;
                    }
                }
                listView1.Invalidate();
            }
        }

        private void listView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                int i = (int)listView1.SelectedItems[0].Tag;
                namelabel.Text = String.Format("Name: {0}", TileData.LandTable[i].Name);
                graphiclabel.Text = String.Format("ID: 0x{0:X4} ({0})", i);
                FlagsLabel.Text = String.Format("Flags: {0}", TileData.LandTable[i].Flags);
            }
        }

        private void drawitem(object sender, DrawListViewItemEventArgs e)
        {
            int i = (int)listView1.Items[e.ItemIndex].Tag;

            bool patched;
            Bitmap bmp = Art.GetLand(i, out patched);

            if (bmp != null)
            {
                if (listView1.SelectedItems.Contains(e.Item))
                    e.Graphics.FillRectangle(Brushes.LightBlue, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
                else if (patched)
                    e.Graphics.FillRectangle(Brushes.LightCoral, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
                else
                    e.Graphics.FillRectangle(Brushes.White, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);

                int width = bmp.Width;
                int height = bmp.Height;

                if (width > e.Bounds.Width)
                    width = e.Bounds.Width - 2;

                if (height > e.Bounds.Height)
                    height = e.Bounds.Height - 2;

                e.Graphics.DrawImage(bmp, e.Bounds.X + 1, e.Bounds.Y + 1,
                                     new Rectangle(0, 0, e.Bounds.Width - 1, e.Bounds.Height - 1),
                                     GraphicsUnit.Pixel);

                if (listView1.SelectedItems.Contains(e.Item))
                    e.DrawFocusRectangle();
                else
                    e.Graphics.DrawRectangle(Pens.Gray, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
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

        private void OnClickReplace(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
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
                        if ((bmp.Height != 44) || (bmp.Width != 44))
                        {
                            MessageBox.Show("Height or Width Invalid", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                            return;
                        }
                        if (dialog.FileName.Contains(".bmp"))
                            bmp = Utils.ConvertBmp(bmp);
                        Art.ReplaceLand((int)listView1.SelectedItems[0].Tag, bmp);
                        FiddlerControls.Events.FireLandTileChangeEvent(this, (int)listView1.SelectedItems[0].Tag);
                        listView1.Invalidate();
                        Options.ChangedUltimaClass["Art"] = true;
                    }
                }
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
                Art.Save(FiddlerControls.Options.OutputPath);
                Cursor.Current = Cursors.Default;
                MessageBox.Show(
                    String.Format("Saved to {0}", FiddlerControls.Options.OutputPath),
                    "Save",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                Options.ChangedUltimaClass["Art"] = false;
            }
        }

        private void onTextChanged_Insert(object sender, EventArgs e)
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

        private void OnKeyDown_Insert(object sender, KeyEventArgs e)
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
                            if ((bmp.Height != 44) || (bmp.Width != 44))
                            {
                                MessageBox.Show("Height or Width Invalid", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                                return;
                            }
                            if (dialog.FileName.Contains(".bmp"))
                                bmp = Utils.ConvertBmp(bmp);
                            Art.ReplaceLand(index, bmp);
                            FiddlerControls.Events.FireLandTileChangeEvent(this, index);
                            Options.ChangedUltimaClass["Art"] = true;
                            ListViewItem item = new ListViewItem(index.ToString(), 0);
                            item.Tag = index;
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
                                listView1.Items.Add(item);
                            listView1.View = View.Details; // that works faszinating
                            listView1.View = View.Tile;
                            if (listView1.SelectedItems.Count == 1)
                                listView1.SelectedItems[0].Selected = false;
                            item.Selected = true;
                            item.Focused = true;
                            item.EnsureVisible();
                        }
                    }
                }
            }
        }

        private void onClickRemove(object sender, EventArgs e)
        {
            int i = (int)listView1.SelectedItems[0].Tag;
            DialogResult result =
                        MessageBox.Show(String.Format("Are you sure to remove {0}", i), "Save",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes)
            {
                Art.RemoveLand(i);
                FiddlerControls.Events.FireLandTileChangeEvent(this, i);
                i = listView1.SelectedItems[0].Index;
                listView1.SelectedItems[0].Selected = false;
                listView1.Items.RemoveAt(i);
                listView1.Invalidate();
                Options.ChangedUltimaClass["Art"] = true;
            }
        }

        private void onClickFindFree(object sender, EventArgs e)
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
                        listView1.SelectedItems[0].Selected = false;
                    item.Selected = true;
                    item.Focused = true;
                    item.EnsureVisible();
                    break;
                }
            }
        }

        private void onClickExportBmp(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                string path = FiddlerControls.Options.OutputPath;
                int i = (int)listView1.SelectedItems[0].Tag;
                string FileName = Path.Combine(path, String.Format("Landtile {0}.bmp", i));
                Bitmap bit = new Bitmap(Ultima.Art.GetLand(i));
                if (bit != null)
                    bit.Save(FileName, ImageFormat.Bmp);
                bit.Dispose();
                MessageBox.Show(String.Format("Landtile saved to {0}", FileName), "Saved",
                    MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
        }

        private void onClickExportTiff(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                string path = FiddlerControls.Options.OutputPath;
                int i = (int)listView1.SelectedItems[0].Tag;
                string FileName = Path.Combine(path, String.Format("Landtile {0}.tiff", i));
                Bitmap bit = new Bitmap(Ultima.Art.GetLand(i));
                if (bit != null)
                    bit.Save(FileName, ImageFormat.Tiff);
                bit.Dispose();
                MessageBox.Show(String.Format("Landtile saved to {0}", FileName), "Saved",
                    MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
        }

        private void onClickExportJpg(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                string path = FiddlerControls.Options.OutputPath;
                int i = (int)listView1.SelectedItems[0].Tag;
                string FileName = Path.Combine(path, String.Format("Landtile {0}.jpg", i));
                Bitmap bit = new Bitmap(Ultima.Art.GetLand(i));
                if (bit != null)
                    bit.Save(FileName, ImageFormat.Jpeg);
                bit.Dispose();
                MessageBox.Show(String.Format("Landtile saved to {0}", FileName), "Saved",
                    MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
        }

        private void OnClickSelectTiledata(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
                FiddlerControls.TileDatas.Select((int)listView1.SelectedItems[0].Tag, true);
        }

        private void OnClickSelectRadarCol(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
                FiddlerControls.RadarColor.Select((int)listView1.SelectedItems[0].Tag, true);
        }

        private void OnClick_SaveAllBmp(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    for (int i = 0; i < listView1.Items.Count; ++i)
                    {
                        int index = (int)listView1.Items[i].Tag;
                        if (index >= 0)
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
                    for (int i = 0; i < listView1.Items.Count; ++i)
                    {
                        int index = (int)listView1.Items[i].Tag;
                        if (index >= 0)
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
                    for (int i = 0; i < listView1.Items.Count; ++i)
                    {
                        int index = (int)listView1.Items[i].Tag;
                        if (index >= 0)
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
