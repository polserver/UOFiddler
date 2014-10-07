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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Ultima;

namespace FiddlerControls
{
    public partial class Fonts : UserControl
    {
        public Fonts()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            refmarker = this;
            setOffsetsToolStripMenuItem.Visible = false;
        }

        private bool Loaded = false;
        private static Fonts refmarker;

        /// <summary>
        /// Reload when loaded (file changed)
        /// </summary>
        private void Reload()
        {
            if (Loaded)
                OnLoad(this, EventArgs.Empty);
        }

        /// <summary>
        /// Refreshs view if Offset of Unicode char is changed
        /// </summary>
        public static void RefreshOnCharChange()
        {
            if ((int)refmarker.treeView.SelectedNode.Parent.Tag == 1) // Unicode
            {
                refmarker.listView1.Invalidate();
                if (refmarker.listView1.SelectedItems.Count > 0)
                {
                    int i = int.Parse(refmarker.listView1.SelectedItems[0].Text.ToString());
                    refmarker.toolStripStatusLabel1.Text =
                        String.Format("'{0}' : {1} (0x{1:X}) XOffset: {2} YOffset: {3}",
                        (char)i, i,
                        UnicodeFonts.Fonts[(int)refmarker.treeView.SelectedNode.Tag].Chars[i].XOffset,
                        UnicodeFonts.Fonts[(int)refmarker.treeView.SelectedNode.Tag].Chars[i].YOffset);
                }
            }
        }

        private void OnLoad(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Options.LoadedUltimaClass["ASCIIFont"] = true;
            Options.LoadedUltimaClass["UnicodeFont"] = true;

            treeView.BeginUpdate();
            treeView.Nodes.Clear();
            TreeNode node = new TreeNode("ASCII");
            node.Tag = 0;
            treeView.Nodes.Add(node);
            for (int i = 0; i < ASCIIText.Fonts.Length; ++i)
            {
                node = new TreeNode(i.ToString());
                node.Tag = i;
                treeView.Nodes[0].Nodes.Add(node);
            }
            node = new TreeNode("Unicode");
            node.Tag = 1;
            treeView.Nodes.Add(node);
            for (int i = 0; i < UnicodeFonts.Fonts.Length; ++i)
            {
                if (UnicodeFonts.Fonts[i] == null)
                    continue;
                node = new TreeNode(i.ToString());
                node.Tag = i;
                treeView.Nodes[1].Nodes.Add(node);
            }
            treeView.ExpandAll();
            treeView.EndUpdate();
            treeView.SelectedNode = treeView.Nodes[0].Nodes[0];
            if (!Loaded)
                FiddlerControls.Events.FilePathChangeEvent += new FiddlerControls.Events.FilePathChangeHandler(OnFilePathChangeEvent);
            Loaded = true;
            Cursor.Current = Cursors.Default;
        }

        private void OnFilePathChangeEvent()
        {
            Reload();
        }

        private void onSelect(object sender, TreeViewEventArgs e)
        {
            if (treeView.SelectedNode.Parent == null)
                treeView.SelectedNode = treeView.SelectedNode.Nodes[0];

            int font = (int)treeView.SelectedNode.Tag;
            listView1.Clear();
            listView1.BeginUpdate();
            if ((int)treeView.SelectedNode.Parent.Tag == 1)
            {
                setOffsetsToolStripMenuItem.Visible = true;
                ListViewItem[] cache = new ListViewItem[0x10000];
                for (int i = 0; i < 0x10000; ++i)
                {
                    ListViewItem item = new ListViewItem(i.ToString(), 0);
                    item.Tag = i;
                    cache[i] = item;
                }
                listView1.Items.AddRange(cache);
            }
            else
            {
                setOffsetsToolStripMenuItem.Visible = false;
                if (ASCIIText.Fonts[font] != null)
                {
                    ListViewItem[] cache = new ListViewItem[ASCIIText.Fonts[font].Characters.Length];
                    for (int i = 0; i < ASCIIText.Fonts[font].Characters.Length; ++i)
                    {
                        ListViewItem item = new ListViewItem((i + 32).ToString(), 0);
                        item.Tag = ASCIIText.Fonts[font].Characters[i];
                        cache[i] = item;
                    }
                    listView1.Items.AddRange(cache);
                }
            }
            listView1.TileSize = new Size(30, 30);
            listView1.EndUpdate();
        }

        private void drawitem(object sender, DrawListViewItemEventArgs e)
        {
            int i = int.Parse(e.Item.Text.ToString());
            Bitmap bmp;
            char c = (char)i;
            if ((int)treeView.SelectedNode.Parent.Tag == 1) // Unicode
                bmp = UnicodeFonts.Fonts[(int)treeView.SelectedNode.Tag].Chars[i].GetImage();
            else
                bmp = (Bitmap)e.Item.Tag;

            if (listView1.SelectedItems.Contains(e.Item))
                e.Graphics.FillRectangle(Brushes.LightBlue, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
            e.Graphics.DrawString(c.ToString(), Fonts.DefaultFont, Brushes.Gray, e.Bounds.X + e.Bounds.Width / 2, e.Bounds.Y + e.Bounds.Height / 2);
            if (bmp != null)
            {
                int width = bmp.Width;
                int height = bmp.Height;

                if (width > e.Bounds.Width)
                    width = e.Bounds.Width - 2;

                if (height > e.Bounds.Height)
                    height = e.Bounds.Height - 2;

                e.Graphics.DrawImage(bmp, new Rectangle(e.Bounds.X + 2, e.Bounds.Y + 2, width, height));

            }
            e.Graphics.DrawRectangle(new Pen(Color.Gray), e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
        }

        private void onSelectChar(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
                toolStripStatusLabel1.Text = " : ()";
            else
            {
                int i = int.Parse(listView1.SelectedItems[0].Text.ToString());
                if ((int)treeView.SelectedNode.Parent.Tag == 1) // Unicode
                    toolStripStatusLabel1.Text = String.Format("'{0}' : {1} (0x{1:X}) XOffset: {2} YOffset: {3}", (char)i, i, UnicodeFonts.Fonts[(int)treeView.SelectedNode.Tag].Chars[i].XOffset, UnicodeFonts.Fonts[(int)treeView.SelectedNode.Tag].Chars[i].YOffset);
                else
                    toolStripStatusLabel1.Text = String.Format("'{0}' : {1} (0x{1:X})", (char)i, i);
            }
        }

        private void OnClickExport(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                string path = FiddlerControls.Options.OutputPath;
                string filetype;
                if ((int)treeView.SelectedNode.Parent.Tag == 1)
                    filetype = "Unicode";
                else
                    filetype = "ASCII";

                string filename = Path.Combine(path, String.Format("{0} {1} 0x{2:X}.tiff",
                    filetype,
                    (int)treeView.SelectedNode.Tag,
                    int.Parse(listView1.SelectedItems[0].Text.ToString())));

                if ((int)treeView.SelectedNode.Parent.Tag == 1)
                {
                    Bitmap bmp = UnicodeFonts.Fonts[(int)treeView.SelectedNode.Tag].Chars[(int)listView1.SelectedItems[0].Tag].GetImage(true);
                    if (bmp == null)
                        bmp = new Bitmap(10, 10);
                    bmp.Save(filename, ImageFormat.Tiff);
                }
                else
                    ((Bitmap)listView1.SelectedItems[0].Tag).Save(filename, ImageFormat.Tiff);
                MessageBox.Show(
                    String.Format("Character saved to {0}", filename),
                    "Saved",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1);
            }
        }

        private void OnClickImport(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                using (OpenFileDialog dialog = new OpenFileDialog())
                {
                    dialog.Multiselect = false;
                    dialog.Title = "Choose an imagefile to import";
                    dialog.CheckFileExists = true;
                    dialog.Filter = "image files (*.tiff;*.bmp)|*.tiff;*.bmp";
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        Bitmap import = new Bitmap(dialog.FileName);
                        if ((import.Height > 255) || (import.Width > 255))
                        {
                            MessageBox.Show("Image Height or Width exceeds 255", "Import", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                            return;
                        }
                        int font = (int)treeView.SelectedNode.Tag;
                        int character = int.Parse(listView1.SelectedItems[0].Text.ToString()) - 32;
                        if ((int)treeView.SelectedNode.Parent.Tag == 1)
                        {
                            UnicodeFonts.Fonts[font].Chars[(int)listView1.SelectedItems[0].Tag].SetBuffer(import);
                            Options.ChangedUltimaClass["UnicodeFont"] = true;
                        }
                        else
                        {
                            ASCIIText.Fonts[font].ReplaceCharacter(character, import);
                            listView1.SelectedItems[0].Tag = import;
                            Options.ChangedUltimaClass["ASCIIFont"] = true;
                        }
                        listView1.Invalidate();
                    }
                }
            }
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            string path = FiddlerControls.Options.OutputPath;
            if ((int)treeView.SelectedNode.Parent.Tag == 1)
            {
                string FileName = UnicodeFonts.Save(path, (int)treeView.SelectedNode.Tag);
                MessageBox.Show(
                    String.Format("Unicode saved to {0}", FileName),
                    "Save",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1);
                Options.ChangedUltimaClass["UnicodeFont"] = false;
            }
            else
            {
                string FileName = Path.Combine(path, "fonts.mul");
                ASCIIText.Save(FileName);
                MessageBox.Show(
                    String.Format("Fonts saved to {0}", FileName),
                    "Save",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1);
                Options.ChangedUltimaClass["ASCIIFont"] = false;
            }
        }

        private FontOffset form;
        private void OnClickSetOffsets(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                int font = (int)treeView.SelectedNode.Tag;
                int cha = (int)listView1.SelectedItems[0].Tag;
                if ((form == null) || (form.IsDisposed))
                {
                    form = new FontOffset(font, cha);
                    form.TopMost = true;
                    form.Show();
                }
            }
        }

        private void OnClickWriteText(object sender, EventArgs e)
        {
            int type = (int)treeView.SelectedNode.Parent.Tag;
            int font = (int)treeView.SelectedNode.Tag;
            new FontText(type, font).Show();
        }
    }
}
