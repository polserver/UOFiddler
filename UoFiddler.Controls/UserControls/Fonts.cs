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
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;

namespace UoFiddler.Controls.UserControls
{
    public partial class Fonts : UserControl
    {
        public Fonts()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            _refMarker = this;
            setOffsetsToolStripMenuItem.Visible = false;
        }

        private bool _loaded;
        private static Fonts _refMarker;

        /// <summary>
        /// Reload when loaded (file changed)
        /// </summary>
        private void Reload()
        {
            if (_loaded)
            {
                OnLoad(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Refreshes view if Offset of Unicode char is changed
        /// </summary>
        public static void RefreshOnCharChange()
        {
            if ((int)_refMarker.treeView.SelectedNode.Parent.Tag == 1) // Unicode
            {
                _refMarker.listView1.Invalidate();
                if (_refMarker.listView1.SelectedItems.Count > 0)
                {
                    int i = int.Parse(_refMarker.listView1.SelectedItems[0].Text);
                    _refMarker.toolStripStatusLabel1.Text =
                        string.Format("'{0}' : {1} (0x{1:X}) XOffset: {2} YOffset: {3}",
                        (char)i, i,
                        UnicodeFonts.Fonts[(int)_refMarker.treeView.SelectedNode.Tag].Chars[i].XOffset,
                        UnicodeFonts.Fonts[(int)_refMarker.treeView.SelectedNode.Tag].Chars[i].YOffset);
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
            TreeNode node = new TreeNode("ASCII")
            {
                Tag = 0
            };
            treeView.Nodes.Add(node);
            for (int i = 0; i < ASCIIText.Fonts.Length; ++i)
            {
                node = new TreeNode(i.ToString())
                {
                    Tag = i
                };
                treeView.Nodes[0].Nodes.Add(node);
            }
            node = new TreeNode("Unicode")
            {
                Tag = 1
            };
            treeView.Nodes.Add(node);
            for (int i = 0; i < UnicodeFonts.Fonts.Length; ++i)
            {
                if (UnicodeFonts.Fonts[i] == null)
                {
                    continue;
                }

                node = new TreeNode(i.ToString())
                {
                    Tag = i
                };
                treeView.Nodes[1].Nodes.Add(node);
            }
            treeView.ExpandAll();
            treeView.EndUpdate();
            treeView.SelectedNode = treeView.Nodes[0].Nodes[0];
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

        private void OnSelect(object sender, TreeViewEventArgs e)
        {
            if (treeView.SelectedNode.Parent == null)
            {
                treeView.SelectedNode = treeView.SelectedNode.Nodes[0];
            }

            int font = (int)treeView.SelectedNode.Tag;
            listView1.Clear();
            listView1.BeginUpdate();
            if ((int)treeView.SelectedNode.Parent.Tag == 1)
            {
                setOffsetsToolStripMenuItem.Visible = true;
                ListViewItem[] cache = new ListViewItem[0x10000];
                for (int i = 0; i < 0x10000; ++i)
                {
                    cache[i] = new ListViewItem(i.ToString(), 0)
                    {
                        Tag = i
                    };
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
                        cache[i] = new ListViewItem((i + 32).ToString(), 0)
                        {
                            Tag = ASCIIText.Fonts[font].Characters[i]
                        };
                    }
                    listView1.Items.AddRange(cache);
                }
            }
            listView1.TileSize = new Size(30, 30);
            listView1.EndUpdate();
        }

        private void Drawitem(object sender, DrawListViewItemEventArgs e)
        {
            int i = int.Parse(e.Item.Text);
            char c = (char)i;
            var bmp = (int)treeView.SelectedNode.Parent.Tag == 1
                ? UnicodeFonts.Fonts[(int)treeView.SelectedNode.Tag].Chars[i].GetImage()
                : (Bitmap)e.Item.Tag;

            if (listView1.SelectedItems.Contains(e.Item))
            {
                e.Graphics.FillRectangle(Brushes.LightBlue, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
            }

            e.Graphics.DrawString(c.ToString(), DefaultFont, Brushes.Gray, e.Bounds.X + e.Bounds.Width / 2, e.Bounds.Y + e.Bounds.Height / 2);
            if (bmp != null)
            {
                int width = bmp.Width;
                int height = bmp.Height;

                if (width > e.Bounds.Width)
                {
                    width = e.Bounds.Width - 2;
                }

                if (height > e.Bounds.Height)
                {
                    height = e.Bounds.Height - 2;
                }

                e.Graphics.DrawImage(bmp, new Rectangle(e.Bounds.X + 2, e.Bounds.Y + 2, width, height));
            }
            e.Graphics.DrawRectangle(new Pen(Color.Gray), e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
        }

        private void OnSelectChar(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                toolStripStatusLabel1.Text = " : ()";
            }
            else
            {
                int i = int.Parse(listView1.SelectedItems[0].Text);
                toolStripStatusLabel1.Text = (int)treeView.SelectedNode.Parent.Tag == 1
                    ? string.Format("'{0}' : {1} (0x{1:X}) XOffset: {2} YOffset: {3}", (char)i, i, UnicodeFonts.Fonts[(int)treeView.SelectedNode.Tag].Chars[i].XOffset, UnicodeFonts.Fonts[(int)treeView.SelectedNode.Tag].Chars[i].YOffset)
                    : string.Format("'{0}' : {1} (0x{1:X})", (char)i, i);
            }
        }

        private void OnClickExport(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count <= 0)
            {
                return;
            }

            string path = Options.OutputPath;
            string fileType = (int)treeView.SelectedNode.Parent.Tag == 1 ? "Unicode" : "ASCII";
            string fileName = Path.Combine(path,
                $"{fileType} {(int)treeView.SelectedNode.Tag} 0x{int.Parse(listView1.SelectedItems[0].Text):X}.tiff");

            if ((int)treeView.SelectedNode.Parent.Tag == 1)
            {
                Bitmap bmp = UnicodeFonts.Fonts[(int)treeView.SelectedNode.Tag].Chars[(int)listView1.SelectedItems[0].Tag].GetImage(true)
                             ?? new Bitmap(10, 10);

                bmp.Save(fileName, ImageFormat.Tiff);
            }
            else
            {
                ((Bitmap)listView1.SelectedItems[0].Tag).Save(fileName, ImageFormat.Tiff);
            }

            MessageBox.Show(
                $"Character saved to {fileName}",
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void OnClickImport(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count <= 0)
            {
                return;
            }

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = false;
                dialog.Title = "Choose an image file to import";
                dialog.CheckFileExists = true;
                dialog.Filter = "Image files (*.tif;*.tiff;*.bmp)|*.tif;*.tiff;*.bmp";
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                Bitmap import = new Bitmap(dialog.FileName);
                if (import.Height > 255 || import.Width > 255)
                {
                    MessageBox.Show("Image Height or Width exceeds 255", "Import", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    return;
                }
                int font = (int)treeView.SelectedNode.Tag;
                int character = int.Parse(listView1.SelectedItems[0].Text) - 32;
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

        private void OnClickSave(object sender, EventArgs e)
        {
            string path = Options.OutputPath;
            if ((int)treeView.SelectedNode.Parent.Tag == 1)
            {
                string fileName = UnicodeFonts.Save(path, (int)treeView.SelectedNode.Tag);
                MessageBox.Show(
                    $"Unicode saved to {fileName}",
                    "Save",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1);
                Options.ChangedUltimaClass["UnicodeFont"] = false;
            }
            else
            {
                string fileName = Path.Combine(path, "fonts.mul");
                ASCIIText.Save(fileName);
                MessageBox.Show(
                    $"Fonts saved to {fileName}",
                    "Save",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1);
                Options.ChangedUltimaClass["ASCIIFont"] = false;
            }
        }

        private FontOffset _form;

        private void OnClickSetOffsets(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count <= 0)
            {
                return;
            }

            int font = (int)treeView.SelectedNode.Tag;
            int cha = (int)listView1.SelectedItems[0].Tag;
            if (_form?.IsDisposed == false)
            {
                return;
            }

            _form = new FontOffset(font, cha)
            {
                TopMost = true
            };
            _form.Show();
        }

        private void OnClickWriteText(object sender, EventArgs e)
        {
            int type = (int)treeView.SelectedNode.Parent.Tag;
            int font = (int)treeView.SelectedNode.Tag;
            new FontText(type, font).Show();
        }
    }
}
