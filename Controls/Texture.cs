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

namespace FiddlerControls
{
    public partial class Texture : UserControl
    {
        public Texture()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            refMarker = this;
        }

        private static Texture refMarker = null;
        private bool Loaded = false;
        private void Reload()
        {
            if (Loaded)
                OnLoad(this, EventArgs.Empty);
        }

        public static bool SearchGraphic(int graphic)
        {
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

        private void drawitem(object sender, DrawListViewItemEventArgs e)
        {
            int i = (int)e.Item.Tag;

            bool patched;
            Bitmap bmp = Textures.GetTexture(i, out patched);

            if (bmp != null)
            {
                int width = bmp.Width;
                int height = bmp.Height;

                if (width >= e.Bounds.Width)
                    width = e.Bounds.Width - 2;

                if (height >= e.Bounds.Height)
                    height = e.Bounds.Height - 2;

                e.Graphics.DrawImage(bmp, new Rectangle(e.Bounds.X + 1, e.Bounds.Y + 1, width, height));

                if (listView1.SelectedItems.Contains(e.Item))
                    e.DrawFocusRectangle();
                else if (patched)
                    e.Graphics.DrawRectangle(Pens.LightCoral, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
                else
                    e.Graphics.DrawRectangle(Pens.Gray, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
            }
        }

        private void listView_SelectedIndexChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
                GraphicLabel.Text = String.Format("Graphic: 0x{0:X4} ({0}) [{1}x{1}]", (int)listView1.SelectedItems[0].Tag, Textures.GetTexture((int)listView1.SelectedItems[0].Tag).Width);
        }

        private void OnLoad(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Options.LoadedUltimaClass["Texture"] = true;

            listView1.BeginUpdate();
            listView1.Clear();
            List<ListViewItem> itemcache = new List<ListViewItem>();

            for (int i = 0; i < Textures.GetIdxLength(); ++i)
            {
                if (Textures.TestTexture(i))
                {
                    ListViewItem item = new ListViewItem(i.ToString(), 0);
                    item.Tag = i;
                    itemcache.Add(item);
                }
            }
            listView1.Items.AddRange(itemcache.ToArray());
            listView1.TileSize = new Size(64, 64);
            listView1.EndUpdate();
            if (!Loaded)
            {
                FiddlerControls.Events.FilePathChangeEvent += new FiddlerControls.Events.FilePathChangeHandler(OnFilePathChangeEvent);
                FiddlerControls.Events.TextureChangeEvent += new FiddlerControls.Events.TextureChangeHandler(OnTextureChangeEvent);
            }
            Loaded = true;
            Cursor.Current = Cursors.Default;
        }

        private void OnFilePathChangeEvent()
        {
            if (!FiddlerControls.Options.DesignAlternative)
                Reload();
        }

        private void OnTextureChangeEvent(object sender, int index)
        {
            if (FiddlerControls.Options.DesignAlternative)
                return;
            if (!Loaded)
                return;
            if (sender.Equals(this))
                return;

            if (Ultima.Textures.TestTexture(index))
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

        private TextureSearch showform = null;
        private void OnClickSearch(object sender, EventArgs e)
        {
            if ((showform == null) || (showform.IsDisposed))
            {
                showform = new TextureSearch();
                showform.TopMost = true;
                showform.Show();
            }
        }

        private void onClickSave(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Textures.Save(FiddlerControls.Options.OutputPath);
            Cursor.Current = Cursors.Default;
            MessageBox.Show(
                String.Format("Saved to {0}", FiddlerControls.Options.OutputPath),
                "Save",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            Options.ChangedUltimaClass["Texture"] = false;
        }

        private void onClickFindNext(object sender, EventArgs e)
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

        private void onClickRemove(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
                return;
            int i = (int)listView1.SelectedItems[0].Tag;
            DialogResult result =
                        MessageBox.Show(String.Format("Are you sure to remove 0x{0:X}", i),
                        "Save",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes)
            {
                Textures.Remove(i);
                FiddlerControls.Events.FireTextureChangeEvent(this, i);
                i = listView1.SelectedItems[0].Index;
                listView1.SelectedItems[0].Selected = false;
                listView1.Items.RemoveAt(i);
                listView1.Invalidate();
                Options.ChangedUltimaClass["Texture"] = true;
            }
        }

        private void onClickReplace(object sender, EventArgs e)
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
                        if (((bmp.Width == 64) && (bmp.Height == 64)) || ((bmp.Width == 128) && (bmp.Height == 128)))
                        {
                            if (dialog.FileName.Contains(".bmp"))
                                bmp = Utils.ConvertBmp(bmp);
                            int i = (int)listView1.SelectedItems[0].Tag;
                            Textures.Replace(i, bmp);
                            FiddlerControls.Events.FireTextureChangeEvent(this, i);
                            listView1.Invalidate();
                            listView_SelectedIndexChanged(this, (ListViewItemSelectionChangedEventArgs)null);
                            Options.ChangedUltimaClass["Texture"] = true;
                        }
                        else
                            MessageBox.Show("Height or Width Invalid", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    }
                }
            }
        }

        private void onTextChangedInsert(object sender, EventArgs e)
        {
            int index;
            if (Utils.ConvertStringToInt(InsertText.Text, out index, 0, Textures.GetIdxLength()))
            {
                if (Textures.TestTexture(index))
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
                if (Utils.ConvertStringToInt(InsertText.Text, out index, 0, Textures.GetIdxLength()))
                {
                    if (Textures.TestTexture(index))
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
                            if (((bmp.Width == 64) && (bmp.Height == 64)) || ((bmp.Width == 128) && (bmp.Height == 128)))
                            {
                                Textures.Replace(index, bmp);
                                FiddlerControls.Events.FireTextureChangeEvent(this, index);
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
                                Options.ChangedUltimaClass["Texture"] = true;
                            }
                            else
                                MessageBox.Show("Height or Width Invalid", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                        }
                    }
                }
            }
        }

        private void onClickExportBmp(object sender, EventArgs e)
        {
            string path = FiddlerControls.Options.OutputPath;
            int i = (int)listView1.SelectedItems[0].Tag;
            string FileName = Path.Combine(path, String.Format("Texture {0}.bmp", i));
            Bitmap bit = new Bitmap(Textures.GetTexture(i));
            if (bit != null)
                bit.Save(FileName, ImageFormat.Bmp);
            bit.Dispose();
            MessageBox.Show(
                String.Format("Texture saved to {0}", FileName),
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void onClickExportTiff(object sender, EventArgs e)
        {
            string path = FiddlerControls.Options.OutputPath;
            int i = (int)listView1.SelectedItems[0].Tag;
            string FileName = Path.Combine(path, String.Format("Texture {0}.tiff", i));
            Bitmap bit = new Bitmap(Textures.GetTexture(i));
            if (bit != null)
                bit.Save(FileName, ImageFormat.Tiff);
            bit.Dispose();
            MessageBox.Show(
                String.Format("Texture saved to {0}", FileName),
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void onClickExportJpg(object sender, EventArgs e)
        {
            string path = FiddlerControls.Options.OutputPath;
            int i = (int)listView1.SelectedItems[0].Tag;
            string FileName = Path.Combine(path, String.Format("Texture {0}.jpg", i));
            Bitmap bit = new Bitmap(Textures.GetTexture(i));
            if (bit != null)
                bit.Save(FileName, ImageFormat.Jpeg);
            bit.Dispose();
            MessageBox.Show(
                String.Format("Texture saved to {0}", FileName),
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

    }
}
