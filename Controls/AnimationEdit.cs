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
    public partial class AnimationEdit : Form
    {
        public AnimationEdit()
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();
            _fileType = 0;
            _currDir = 0;
            toolStripComboBox1.SelectedIndex = 0;
            _framePoint = new Point(pictureBox1.Width / 2, pictureBox1.Height / 2);
            _showOnlyValid = false;
            _loaded = false;
            listView1.MultiSelect = true;
        }

        private static readonly int[] AnimCx = new int[5];
        private static readonly int[] AnimCy = new int[5];
        private bool _loaded;
        private int _fileType;
        private int _currAction;
        private int _currBody;
        private int _currDir;
        private Point _framePoint;
        private bool _showOnlyValid;
        private static bool _drawEmpty;
        private static bool _drawFull;
        private static Pen _blackUndraw = new Pen(Color.FromArgb(255, 0, 0, 0), 1);
        private static SolidBrush _whiteUndraw = new SolidBrush(Color.FromArgb(255, 255, 255, 255));
        private static readonly SolidBrush WhiteTrasparent = new SolidBrush(Color.FromArgb(160, 255, 255, 255));
        private static readonly Color WhiteConvert = Color.FromArgb(255, 255, 255, 255);
        private static readonly Color GreyConvert = Color.FromArgb(255, 170, 170, 170);

        private void OnLoad(object sender, EventArgs e)
        {
            Options.LoadedUltimaClass["AnimationEdit"] = true;
            treeView1.BeginUpdate();
            treeView1.Nodes.Clear();
            if (_fileType != 0)
            {
                int count = Animations.GetAnimCount(_fileType);
                List<TreeNode> nodes = new List<TreeNode>();
                for (int i = 0; i < count; ++i)
                {
                    int animlength = Animations.GetAnimLength(i, _fileType);
                    string type = animlength == 22 ? "H" : animlength == 13 ? "L" : "P";
                    TreeNode node = new TreeNode
                    {
                        Tag = i,
                        Text = string.Format("{0}: {1} ({2})", type, i, BodyConverter.GetTrueBody(_fileType, i))
                    };
                    bool valid = false;
                    for (int j = 0; j < animlength; ++j)
                    {
                        TreeNode subnode = new TreeNode
                        {
                            Tag = j,
                            Text = j.ToString()
                        };
                        if (Ultima.AnimationEdit.IsActionDefinied(_fileType, i, j))
                        {
                            valid = true;
                        }
                        else
                        {
                            subnode.ForeColor = Color.Red;
                        }

                        node.Nodes.Add(subnode);
                    }
                    if (!valid)
                    {
                        if (_showOnlyValid)
                        {
                            continue;
                        }

                        node.ForeColor = Color.Red;
                    }
                    nodes.Add(node);
                }
                treeView1.Nodes.AddRange(nodes.ToArray());
            }
            treeView1.EndUpdate();
            if (treeView1.Nodes.Count > 0)
            {
                treeView1.SelectedNode = treeView1.Nodes[0];
            }

            if (!_loaded)
            {
                FiddlerControls.Events.FilePathChangeEvent += OnFilePathChangeEvent;
            }

            _loaded = true;
        }

        private void OnFilePathChangeEvent()
        {
            if (!_loaded)
            {
                return;
            }

            _fileType = 0;
            _currDir = 0;
            _currAction = 0;
            _currBody = 0;
            toolStripComboBox1.SelectedIndex = 0;
            _framePoint = new Point(pictureBox1.Width / 2, pictureBox1.Height / 2);
            _showOnlyValid = false;
            showOnlyValidToolStripMenuItem.Checked = false;
            OnLoad(null);
        }

        private TreeNode GetNode(int tag)
        {
            if (_showOnlyValid)
            {
                foreach (TreeNode node in treeView1.Nodes)
                {
                    if ((int)node.Tag == tag)
                    {
                        return node;
                    }
                }
                return null;
            }
            else
            {
                return treeView1.Nodes[tag];
            }
        }

        private unsafe void SetPaletteBox()
        {
            if (_fileType != 0)
            {
                AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currBody, _currAction, _currDir);
                Bitmap bmp = new Bitmap(0x100, pictureBoxPalette.Height, PixelFormat.Format16bppArgb1555);
                if (edit != null)
                {
                    BitmapData bd = bmp.LockBits(new Rectangle(0, 0, 0x100, pictureBoxPalette.Height), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);
                    ushort* line = (ushort*)bd.Scan0;
                    int delta = bd.Stride >> 1;
                    for (int y = 0; y < bd.Height; ++y, line += delta)
                    {
                        ushort* cur = line;
                        for (int i = 0; i < 0x100; ++i)
                        {
                            *cur++ = edit.Palette[i];
                        }
                    }
                    bmp.UnlockBits(bd);
                }
                pictureBoxPalette.Image = bmp;
            }
        }

        private void AfterSelectTreeView(object sender, TreeViewEventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                if (treeView1.SelectedNode.Parent == null)
                {
                    if (treeView1.SelectedNode.Tag != null)
                    {
                        _currBody = (int)treeView1.SelectedNode.Tag;
                    }

                    _currAction = 0;
                }
                else
                {
                    if (treeView1.SelectedNode.Parent.Tag != null)
                    {
                        _currBody = (int)treeView1.SelectedNode.Parent.Tag;
                    }

                    _currAction = (int)treeView1.SelectedNode.Tag;
                }
                listView1.BeginUpdate();
                listView1.Clear();
                AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currBody, _currAction, _currDir);
                if (edit != null)
                {
                    int width = 80;
                    int height = 110;
                    Bitmap[] currbits = edit.GetFrames();
                    if (currbits != null)
                    {
                        for (int i = 0; i < currbits.Length; ++i)
                        {
                            if (currbits[i] == null)
                            {
                                continue;
                            }

                            ListViewItem item = new ListViewItem(i.ToString(), 0)
                            {
                                Tag = i
                            };
                            listView1.Items.Add(item);
                            if (currbits[i].Width > width)
                            {
                                width = currbits[i].Width;
                            }

                            if (currbits[i].Height > height)
                            {
                                height = currbits[i].Height;
                            }
                        }
                        listView1.TileSize = new Size(width + 5, height + 5);
                        trackBar2.Maximum = currbits.Length - 1;
                        trackBar2.Value = 0;
                        trackBar2.Invalidate();

                        numericUpDownCx.Value = edit.Frames[trackBar2.Value].Center.X;
                        numericUpDownCy.Value = edit.Frames[trackBar2.Value].Center.Y;
                    }
                    //Soulblighter Modification
                    else
                    {
                        trackBar2.Maximum = 0;
                        trackBar2.Value = 0;
                        trackBar2.Invalidate();
                    }
                    //End of Soulblighter Modification
                }
                listView1.EndUpdate();
                pictureBox1.Invalidate();
                SetPaletteBox();
            }
        }

        private void DrawFrameItem(object sender, DrawListViewItemEventArgs e)
        {
            AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currBody, _currAction, _currDir);
            Bitmap[] currbits = edit.GetFrames();
            Bitmap bmp = currbits[(int)e.Item.Tag];
            int width = bmp.Width;
            int height = bmp.Height;

            if (listView1.SelectedItems.Contains(e.Item))
            {
                e.Graphics.DrawRectangle(new Pen(Color.Red), e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
            }
            else
            {
                e.Graphics.DrawRectangle(new Pen(Color.Gray), e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
            }

            e.Graphics.DrawImage(bmp, e.Bounds.X, e.Bounds.Y, width, height);
            e.DrawText(TextFormatFlags.Bottom | TextFormatFlags.HorizontalCenter);
        }

        private void OnAnimChanged(object sender, EventArgs e)
        {
            if (toolStripComboBox1.SelectedIndex != _fileType)
            {
                _fileType = toolStripComboBox1.SelectedIndex;
                OnLoad(this, EventArgs.Empty);
            }
        }

        private void OnDirectionChanged(object sender, EventArgs e)
        {
            _currDir = trackBar1.Value;
            AfterSelectTreeView(null, null);
        }

        private void OnSizeChangedPictureBox(object sender, EventArgs e)
        {
            _framePoint = new Point(pictureBox1.Width / 2, pictureBox1.Height / 2);
            pictureBox1.Invalidate();
        }
        //Soulblighter Modification

        private void OnPaintFrame(object sender, PaintEventArgs e)
        {
            AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currBody, _currAction, _currDir);
            if (edit != null)
            {
                Bitmap[] currbits = edit.GetFrames();

                e.Graphics.Clear(Color.LightGray);
                e.Graphics.DrawLine(Pens.Black, new Point(_framePoint.X, 0), new Point(_framePoint.X, pictureBox1.Height));
                e.Graphics.DrawLine(Pens.Black, new Point(0, _framePoint.Y), new Point(pictureBox1.Width, _framePoint.Y));

                if (currbits?.Length > 0 && currbits[trackBar2.Value] != null)
                {
                    int varW;
                    int varH;
                    if (!_drawEmpty)
                    {
                        varW = 0;
                        varH = 0;
                    }
                    else
                    {
                        varW = currbits[trackBar2.Value].Width;
                        varH = currbits[trackBar2.Value].Height;
                    }

                    int varFw;
                    int varFh;
                    if (!_drawFull)
                    {
                        varFw = 0;
                        varFh = 0;
                    }
                    else
                    {
                        varFw = currbits[trackBar2.Value].Width;
                        varFh = currbits[trackBar2.Value].Height;
                    }

                    int x = _framePoint.X - edit.Frames[trackBar2.Value].Center.X;
                    int y = _framePoint.Y - edit.Frames[trackBar2.Value].Center.Y - currbits[trackBar2.Value].Height;

                    e.Graphics.FillRectangle(WhiteTrasparent, new Rectangle(x, y, varFw, varFh));
                    e.Graphics.DrawRectangle(Pens.Red, new Rectangle(x, y, varW, varH));
                    e.Graphics.DrawImage(currbits[trackBar2.Value], x, y);
                    //e.Graphics.DrawLine(Pens.Red, new Point(0, 335-(int)numericUpDown1.Value), new Point(pictureBox1.Width, 335-(int)numericUpDown1.Value));
                }

                //Draw Referencial Point Arrow
                Point point1 = new Point(418 - (int)numericUpDown2.Value, 335 - (int)numericUpDown1.Value);
                Point point2 = new Point(418 - (int)numericUpDown2.Value, 352 - (int)numericUpDown1.Value);
                Point point3 = new Point(422 - (int)numericUpDown2.Value, 348 - (int)numericUpDown1.Value);
                Point point4 = new Point(425 - (int)numericUpDown2.Value, 353 - (int)numericUpDown1.Value);
                Point point5 = new Point(427 - (int)numericUpDown2.Value, 352 - (int)numericUpDown1.Value);
                Point point6 = new Point(425 - (int)numericUpDown2.Value, 347 - (int)numericUpDown1.Value);
                Point point7 = new Point(430 - (int)numericUpDown2.Value, 347 - (int)numericUpDown1.Value);
                Point[] arrayPoints = { point1, point2, point3, point4, point5, point6, point7 };
                e.Graphics.FillPolygon(_whiteUndraw, arrayPoints);
                e.Graphics.DrawPolygon(_blackUndraw, arrayPoints);
            }
        }
        //End of Soulblighter Modification
        //Soulblighter Modification
        private void OnFrameCountBarChanged(object sender, EventArgs e)
        {
            if (_fileType != 0)
            {
                AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currBody, _currAction, _currDir);
                if (edit != null)
                {
                    if (edit.Frames.Count >= trackBar2.Value)
                    {
                        numericUpDownCx.Value = edit.Frames[trackBar2.Value].Center.X;
                        numericUpDownCy.Value = edit.Frames[trackBar2.Value].Center.Y;
                    }
                }
                pictureBox1.Invalidate();
            }
        }
        //End of Soulblighter Modification

        private void OnCenterXValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (_fileType != 0)
                {
                    AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currBody, _currAction, _currDir);
                    if (edit != null)
                    {
                        if (edit.Frames.Count >= trackBar2.Value)
                        {
                            FrameEdit frame = edit.Frames[trackBar2.Value];
                            if (numericUpDownCx.Value != frame.Center.X)
                            {
                                frame.ChangeCenter((int)numericUpDownCx.Value, frame.Center.Y);
                                Options.ChangedUltimaClass["Animations"] = true;
                                pictureBox1.Invalidate();
                            }
                        }
                    }
                }
            }
            catch (NullReferenceException) { }
        }

        private void OnCenterYValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (_fileType != 0)
                {
                    AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currBody, _currAction, _currDir);
                    if (edit != null)
                    {
                        if (edit.Frames.Count >= trackBar2.Value)
                        {
                            FrameEdit frame = edit.Frames[trackBar2.Value];
                            if (numericUpDownCy.Value != frame.Center.Y)
                            {
                                frame.ChangeCenter(frame.Center.X, (int)numericUpDownCy.Value);
                                Options.ChangedUltimaClass["Animations"] = true;
                                pictureBox1.Invalidate();
                            }
                        }
                    }
                }
            }
            catch (NullReferenceException) { }
        }

        private void OnClickExtractImages(object sender, EventArgs e)
        {
            if (_fileType != 0)
            {
                ToolStripMenuItem menu = (ToolStripMenuItem)sender;
                ImageFormat format = ImageFormat.Bmp;
                if ((string)menu.Tag == ".tiff")
                {
                    format = ImageFormat.Tiff;
                }

                string path = Options.OutputPath;
                int body, action;
                if (treeView1.SelectedNode.Parent == null)
                {
                    body = (int)treeView1.SelectedNode.Tag;
                    action = -1;
                }
                else
                {
                    body = (int)treeView1.SelectedNode.Parent.Tag;
                    action = (int)treeView1.SelectedNode.Tag;
                }

                if (action == -1)
                {
                    for (int a = 0; a < Animations.GetAnimLength(body, _fileType); ++a)
                    {
                        for (int i = 0; i < 5; ++i)
                        {
                            AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, body, a, i);
                            if (edit != null)
                            {
                                Bitmap[] bits = edit.GetFrames();
                                if (bits != null)
                                {
                                    for (int j = 0; j < bits.Length; ++j)
                                    {
                                        string filename = string.Format("anim{5}_{0}_{1}_{2}_{3}{4}", body, a, i, j, menu.Tag, _fileType);
                                        string file = Path.Combine(path, filename);
                                        Bitmap bit = new Bitmap(bits[j]);
                                        bit?.Save(file, format);
                                        bit.Dispose();
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < 5; ++i)
                    {
                        AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, body, action, i);
                        if (edit != null)
                        {
                            Bitmap[] bits = edit.GetFrames();
                            if (bits != null)
                            {
                                for (int j = 0; j < bits.Length; ++j)
                                {
                                    string filename = string.Format("anim{5}_{0}_{1}_{2}_{3}{4}", body, action, i, j, menu.Tag, _fileType);
                                    string file = Path.Combine(path, filename);
                                    Bitmap bit = new Bitmap(bits[j]);
                                    bit?.Save(file, format);
                                    bit.Dispose();
                                }
                            }
                        }
                    }
                }
                MessageBox.Show(
                        string.Format("Frames saved to {0}", path),
                        "Saved",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information,
                        MessageBoxDefaultButton.Button1);
            }
        }

        private void OnClickRemoveAction(object sender, EventArgs e)
        {
            if (_fileType != 0)
            {
                if (treeView1.SelectedNode.Parent == null)
                {
                    DialogResult result =
                           MessageBox.Show(string.Format("Are you sure to remove animation {0}", _currBody),
                           "Remove",
                           MessageBoxButtons.YesNo,
                           MessageBoxIcon.Question,
                           MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.Yes)
                    {
                        treeView1.SelectedNode.ForeColor = Color.Red;
                        for (int i = 0; i < treeView1.SelectedNode.Nodes.Count; ++i)
                        {
                            treeView1.SelectedNode.Nodes[i].ForeColor = Color.Red;
                            for (int d = 0; d < 5; ++d)
                            {
                                AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currBody, i, d);
                                edit?.ClearFrames();
                            }
                        }
                        if (_showOnlyValid)
                        {
                            treeView1.SelectedNode.Remove();
                        }

                        Options.ChangedUltimaClass["Animations"] = true;
                        AfterSelectTreeView(this, null);
                    }
                }
                else
                {
                    DialogResult result =
                           MessageBox.Show(string.Format("Are you sure to remove action {0}", _currAction),
                           "Remove",
                           MessageBoxButtons.YesNo,
                           MessageBoxIcon.Question,
                           MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.Yes)
                    {
                        for (int i = 0; i < 5; ++i)
                        {
                            AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currBody, _currAction, i);
                            edit?.ClearFrames();
                        }
                        treeView1.SelectedNode.Parent.Nodes[_currAction].ForeColor = Color.Red;
                        bool valid = false;
                        foreach (TreeNode node in treeView1.SelectedNode.Parent.Nodes)
                        {
                            if (node.ForeColor != Color.Red)
                            {
                                valid = true;
                                break;
                            }
                        }
                        if (!valid)
                        {
                            if (_showOnlyValid)
                            {
                                treeView1.SelectedNode.Parent.Remove();
                            }
                            else
                            {
                                treeView1.SelectedNode.Parent.ForeColor = Color.Red;
                            }
                        }
                        Options.ChangedUltimaClass["Animations"] = true;
                        AfterSelectTreeView(this, null);
                    }
                }
            }
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            if (_fileType != 0)
            {
                Ultima.AnimationEdit.Save(_fileType, Options.OutputPath);
                MessageBox.Show(
                        string.Format("AnimationFile saved to {0}", Options.OutputPath),
                        "Saved",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information,
                        MessageBoxDefaultButton.Button1);
                Options.ChangedUltimaClass["Animations"] = false;
            }
        }
        //My Soulblighter Modification
        private void OnClickRemoveFrame(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                int corrector = 0;
                int[] frameindex = new int[listView1.SelectedItems.Count];
                for (int i = 0; i < listView1.SelectedItems.Count; i++)
                {
                    frameindex[i] = (int)listView1.SelectedIndices[i] - corrector;
                    corrector++;
                }
                for (int i = 0; i < frameindex.Length; i++)
                {
                    AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currBody, _currAction, _currDir);
                    if (edit != null)
                    {
                        edit.RemoveFrame(frameindex[i]);
                        listView1.Items.RemoveAt(listView1.Items.Count - 1);
                        if (edit.Frames.Count != 0)
                        {
                            trackBar2.Maximum = edit.Frames.Count - 1;
                        }
                        else
                        {
                            TreeNode node = GetNode(_currBody); // TODO: unused variable?
                            trackBar2.Maximum = 0;
                        }
                        listView1.Invalidate();
                        Options.ChangedUltimaClass["Animations"] = true;
                    }
                }
            }
        }

        //End of Soulblighter Modification
        private void OnClickReplace(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                using (OpenFileDialog dialog = new OpenFileDialog())
                {
                    int frameindex = (int)listView1.SelectedItems[0].Tag;
                    dialog.Multiselect = false;
                    dialog.Title = string.Format("Choose image file to replace at {0}", frameindex);
                    dialog.CheckFileExists = true;
                    dialog.Filter = "Image files (*.tif;*.tiff;*.bmp)|*.tif;*.tiff;*.bmp";
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        Bitmap bmp = new Bitmap(dialog.FileName);
                        if (dialog.FileName.Contains(".bmp"))
                        {
                            bmp = Utils.ConvertBmpAnim(bmp, (int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                        }

                        AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currBody, _currAction, _currDir);
                        if (edit != null)
                        {
                            edit.ReplaceFrame(bmp, frameindex);
                            listView1.Invalidate();
                            Options.ChangedUltimaClass["Animations"] = true;
                        }
                    }
                }
            }
        }

        private void OnClickAdd(object sender, EventArgs e)
        {
            if (_fileType != 0)
            {
                using (OpenFileDialog dialog = new OpenFileDialog())
                {
                    dialog.Multiselect = true;
                    dialog.Title = "Choose image file to add";
                    dialog.CheckFileExists = true;
                    dialog.Filter = "Gif files (*.gif;)|*.gif; |Bitmap files (*.bmp;)|*.bmp; |Tiff files (*.tif;*.tiff)|*.tif;*.tiff; |Png files (*.png;)|*.png; |Jpeg files (*.jpeg;*.jpg;)|*.jpeg;*.jpg;";
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        listView1.BeginUpdate();
                        //My Soulblighter Modifications
                        for (int w = 0; w < dialog.FileNames.Length; w++)
                        {
                            Bitmap bmp = new Bitmap(dialog.FileNames[w]); // dialog.Filename replaced by dialog.FileNames[w]
                            AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currBody, _currAction, _currDir);
                            if (dialog.FileName.Contains(".bmp") | dialog.FileName.Contains(".tiff") | dialog.FileName.Contains(".png") | dialog.FileName.Contains(".jpeg") | dialog.FileName.Contains(".jpg"))
                            {
                                bmp = Utils.ConvertBmpAnim(bmp, (int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                                //edit.GetImagePalette(bmp);
                            }

                            if (edit != null)
                            {
                                //Gif Especial Properties
                                if (dialog.FileName.Contains(".gif"))
                                {
                                    FrameDimension dimension = new FrameDimension(bmp.FrameDimensionsList[0]);
                                    // Number of frames 
                                    int frameCount = bmp.GetFrameCount(dimension);
                                    Bitmap[] bitbmp = new Bitmap[frameCount];
                                    bmp.SelectActiveFrame(dimension, 0);
                                    edit.GetGifPalette(bmp);
                                    progressBar1.Maximum = frameCount;
                                    // Return an Image at a certain index 
                                    for (int index = 0; index < frameCount; index++)
                                    {
                                        bitbmp[index] = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format16bppArgb1555);
                                        bmp.SelectActiveFrame(dimension, index);
                                        bitbmp[index] = (Bitmap)bmp;
                                        bitbmp[index] = Utils.ConvertBmpAnim(bitbmp[index], (int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                                        edit.AddFrame(bitbmp[index]);
                                        TreeNode node = GetNode(_currBody);
                                        if (node != null)
                                        {
                                            node.ForeColor = Color.Black;
                                            node.Nodes[_currAction].ForeColor = Color.Black;
                                        }
                                        ListViewItem item;
                                        int i = edit.Frames.Count - 1;
                                        item = new ListViewItem(i.ToString(), 0)
                                        {
                                            Tag = i
                                        };
                                        listView1.Items.Add(item);
                                        int width = listView1.TileSize.Width - 5;
                                        if (bmp.Width > listView1.TileSize.Width)
                                        {
                                            width = bmp.Width;
                                        }

                                        int height = listView1.TileSize.Height - 5;
                                        if (bmp.Height > listView1.TileSize.Height)
                                        {
                                            height = bmp.Height;
                                        }

                                        listView1.TileSize = new Size(width + 5, height + 5);
                                        trackBar2.Maximum = i;
                                        Options.ChangedUltimaClass["Animations"] = true;
                                        if (progressBar1.Value < progressBar1.Maximum)
                                        {
                                            progressBar1.Value++;
                                            progressBar1.Invalidate();
                                        }
                                    }
                                    progressBar1.Value = 0;
                                    progressBar1.Invalidate();
                                    SetPaletteBox();
                                    numericUpDownCx.Value = edit.Frames[trackBar2.Value].Center.X;
                                    numericUpDownCy.Value = edit.Frames[trackBar2.Value].Center.Y;
                                    Options.ChangedUltimaClass["Animations"] = true;
                                }
                                //End of Soulblighter Modifications
                                else
                                {
                                    edit.AddFrame(bmp);
                                    TreeNode node = GetNode(_currBody);
                                    if (node != null)
                                    {
                                        node.ForeColor = Color.Black;
                                        node.Nodes[_currAction].ForeColor = Color.Black;
                                    }
                                    ListViewItem item;
                                    int i = edit.Frames.Count - 1;
                                    item = new ListViewItem(i.ToString(), 0)
                                    {
                                        Tag = i
                                    };
                                    listView1.Items.Add(item);
                                    int width = listView1.TileSize.Width - 5;
                                    if (bmp.Width > listView1.TileSize.Width)
                                    {
                                        width = bmp.Width;
                                    }

                                    int height = listView1.TileSize.Height - 5;
                                    if (bmp.Height > listView1.TileSize.Height)
                                    {
                                        height = bmp.Height;
                                    }

                                    listView1.TileSize = new Size(width + 5, height + 5);
                                    trackBar2.Maximum = i;
                                    numericUpDownCx.Value = edit.Frames[trackBar2.Value].Center.X;
                                    numericUpDownCy.Value = edit.Frames[trackBar2.Value].Center.Y;
                                    Options.ChangedUltimaClass["Animations"] = true;
                                }
                            }
                        }
                        listView1.EndUpdate();
                        listView1.Invalidate();
                    }
                }
            }
            //Refresh List
            _currDir = trackBar1.Value;
            AfterSelectTreeView(null, null);
        }

        private void OnClickExtractPalette(object sender, EventArgs e)
        {
            if (_fileType != 0)
            {
                ToolStripMenuItem menu = (ToolStripMenuItem)sender;
                AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currBody, _currAction, _currDir);
                if (edit != null)
                {
                    string name = string.Format("palette_anim{0}_{1}_{2}_{3}", _fileType, _currBody, _currAction, _currDir);
                    if ((string)menu.Tag == "txt")
                    {
                        string path = Path.Combine(Options.OutputPath, name + ".txt");
                        edit.ExportPalette(path, 0);
                    }
                    else
                    {
                        string path = Path.Combine(Options.OutputPath, name + "." + (string)menu.Tag);
                        if ((string)menu.Tag == "bmp")
                        {
                            edit.ExportPalette(path, 1);
                        }
                        else
                        {
                            edit.ExportPalette(path, 2);
                        }
                    }
                    MessageBox.Show(
                        string.Format("Palette saved to {0}", Options.OutputPath),
                        "Saved",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information,
                        MessageBoxDefaultButton.Button1);
                }
            }
        }

        private void OnClickImportPalette(object sender, EventArgs e)
        {
            if (_fileType != 0)
            {
                using (OpenFileDialog dialog = new OpenFileDialog())
                {
                    dialog.Multiselect = false;
                    dialog.Title = "Choose palette file";
                    dialog.CheckFileExists = true;
                    dialog.Filter = "txt files (*.txt)|*.txt";
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currBody, _currAction, _currDir);
                        if (edit != null)
                        {
                            using (StreamReader sr = new StreamReader(dialog.FileName))
                            {
                                string line;
                                ushort[] palette = new ushort[0x100];
                                int i = 0;
                                while ((line = sr.ReadLine()) != null)
                                {
                                    if ((line = line.Trim()).Length == 0 || line.StartsWith("#"))
                                    {
                                        continue;
                                    }

                                    palette[i++] = ushort.Parse(line);
                                    //My Soulblighter Modification
                                    if (palette[i++] == 32768)
                                    {
                                        palette[i++] = 32769;
                                    }
                                    //End of Soulblighter Modification
                                    if (i >= 0x100)
                                    {
                                        break;
                                    }
                                }
                                edit.ReplacePalette(palette);
                            }
                            SetPaletteBox();
                            listView1.Invalidate();
                            Options.ChangedUltimaClass["Animations"] = true;
                        }
                    }
                }
            }
        }

        private void OnClickImportFromVD(object sender, EventArgs e)
        {
            if (_fileType != 0)
            {
                using (OpenFileDialog dialog = new OpenFileDialog())
                {
                    dialog.Multiselect = false;
                    dialog.Title = "Choose palette file";
                    dialog.CheckFileExists = true;
                    dialog.Filter = "vd files (*.vd)|*.vd";
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        int animlength = Animations.GetAnimLength(_currBody, _fileType);
                        int currtype = animlength == 22 ? 0 : animlength == 13 ? 1 : 2;
                        using (FileStream fs = new FileStream(dialog.FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            using (BinaryReader bin = new BinaryReader(fs))
                            {
                                int filetype = bin.ReadInt16();
                                int animtype = bin.ReadInt16();
                                if (filetype != 6)
                                {
                                    MessageBox.Show(
                                        "Not an Anim File.",
                                        "Import",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Information,
                                        MessageBoxDefaultButton.Button1);
                                    return;
                                }

                                if (animtype != currtype)
                                {
                                    MessageBox.Show(
                                        "Wrong Anim Id ( Type )",
                                        "Import",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Information,
                                        MessageBoxDefaultButton.Button1);
                                    return;
                                }
                                Ultima.AnimationEdit.LoadFromVd(_fileType, _currBody, bin);
                            }
                        }

                        bool valid = false;
                        TreeNode node = GetNode(_currBody);
                        if (node != null)
                        {
                            for (int j = 0; j < animlength; ++j)
                            {
                                if (Ultima.AnimationEdit.IsActionDefinied(_fileType, _currBody, j))
                                {
                                    node.Nodes[j].ForeColor = Color.Black;
                                    valid = true;
                                }
                                else
                                {
                                    node.Nodes[j].ForeColor = Color.Red;
                                }
                            }
                            node.ForeColor = valid ? Color.Black : Color.Red;
                        }

                        Options.ChangedUltimaClass["Animations"] = true;
                        AfterSelectTreeView(this, null);
                        MessageBox.Show(
                                        "Finished",
                                        "Import",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Information,
                                        MessageBoxDefaultButton.Button1);
                    }
                }
            }
        }

        private void OnClickExportToVD(object sender, EventArgs e)
        {
            if (_fileType != 0)
            {
                string path = Options.OutputPath;
                string fileName = Path.Combine(path, string.Format("anim{0}_0x{1:X}.vd", _fileType, _currBody));
                Ultima.AnimationEdit.ExportToVd(_fileType, _currBody, fileName);
                MessageBox.Show(
                        string.Format("Animation saved to {0}", Options.OutputPath),
                        "Export",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information,
                        MessageBoxDefaultButton.Button1);
            }
        }

        private void OnClickShowOnlyValid(object sender, EventArgs e)
        {
            _showOnlyValid = !_showOnlyValid;
            if (_showOnlyValid)
            {
                treeView1.BeginUpdate();
                for (int i = treeView1.Nodes.Count - 1; i >= 0; --i)
                {
                    if (treeView1.Nodes[i].ForeColor == Color.Red)
                    {
                        treeView1.Nodes[i].Remove();
                    }
                }
                treeView1.EndUpdate();
            }
            else
            {
                OnLoad(null);
            }
        }

        private void ToolStripLabel2_Click(object sender, EventArgs e)
        {
            // TODO: why this is empty?
        }

        //My Soulblighter Modification
        private void Button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (_fileType != 0)
                {
                    AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currBody, _currAction, _currDir);
                    if (edit != null && edit.Frames.Count >= trackBar2.Value)
                    {
                        FrameEdit[] frame = new FrameEdit[edit.Frames.Count];
                        for (int index = 0; index < edit.Frames.Count; index++)
                        {
                            frame[index] = edit.Frames[index];
                            frame[index].ChangeCenter((int)numericUpDownCx.Value, (int)numericUpDownCy.Value);
                            Options.ChangedUltimaClass["Animations"] = true;
                            pictureBox1.Invalidate();
                        }
                    }
                }
            }
            catch (NullReferenceException) { }
        }
        //End of Soulblighter Modification

        //My Soulblighter Modification
        private void FromGifToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_fileType != 0)
            {
                using (OpenFileDialog dialog = new OpenFileDialog())
                {
                    dialog.Multiselect = false;
                    dialog.Title = "Choose palette file";
                    dialog.CheckFileExists = true;
                    dialog.Filter = "Gif files (*.gif)|*.gif";
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        Bitmap bit = new Bitmap(dialog.FileName);
                        AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currBody, _currAction, _currDir);
                        if (edit != null)
                        {
                            FrameDimension dimension = new FrameDimension(bit.FrameDimensionsList[0]);
                            // Number of frames 
                            int frameCount = bit.GetFrameCount(dimension); // TODO: unused variable?
                            bit.SelectActiveFrame(dimension, 0);
                            edit.GetGifPalette(bit);
                            SetPaletteBox();
                            listView1.Invalidate();
                            Options.ChangedUltimaClass["Animations"] = true;
                        }
                    }
                }
            }
        }

        private void ReferencialPointX(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }

        private void ReferencialPointY(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }

        private static bool _lockbutton; //Lock button variable

        private void PictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (!_lockbutton && toolStripButton6.Enabled)
            {
                numericUpDown2.Value = 418 - e.X;
                numericUpDown1.Value = 335 - e.Y;
                pictureBox1.Invalidate();
            }
        }

        //Change center of frame on key press
        private void TxtSendData_KeyDown(object sender, KeyEventArgs e)
        {
            if (!timer1.Enabled)
            {
                if (e.KeyCode == Keys.Right)
                {
                    numericUpDownCx.Value--;
                    numericUpDownCx.Invalidate();
                }
                else if (e.KeyCode == Keys.Left)
                {
                    numericUpDownCx.Value++;
                    numericUpDownCx.Invalidate();
                }
                else if (e.KeyCode == Keys.Up)
                {
                    numericUpDownCy.Value++;
                    numericUpDownCy.Invalidate();
                }
                else if (e.KeyCode == Keys.Down)
                {
                    numericUpDownCy.Value--;
                    numericUpDownCy.Invalidate();
                }
                pictureBox1.Invalidate();
            }
        }

        //Change center of Referencial Point on key press
        private void TxtSendData_KeyDown2(object sender, KeyEventArgs e)
        {
            if (!_lockbutton && toolStripButton6.Enabled)
            {
                if (e.KeyCode == Keys.Right)
                {
                    numericUpDown2.Value--;
                    numericUpDown2.Invalidate();
                }
                else if (e.KeyCode == Keys.Left)
                {
                    numericUpDown2.Value++;
                    numericUpDown2.Invalidate();
                }
                else if (e.KeyCode == Keys.Up)
                {
                    numericUpDown1.Value++;
                    numericUpDown1.Invalidate();
                }
                else if (e.KeyCode == Keys.Down)
                {
                    numericUpDown1.Value--;
                    numericUpDown1.Invalidate();
                }
                pictureBox1.Invalidate();
            }
        }

        //Lock Button
        private void ToolStripButton6_Click(object sender, EventArgs e)
        {
            _lockbutton = !_lockbutton;
            numericUpDown2.Enabled = !_lockbutton;
            numericUpDown1.Enabled = !_lockbutton;
        }

        //Add in all Directions
        private void AllDirectionsAddToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_fileType != 0)
            {
                using (OpenFileDialog dialog = new OpenFileDialog())
                {
                    dialog.Multiselect = true;
                    dialog.Title = "Choose 5 Gifs to add";
                    dialog.CheckFileExists = true;
                    dialog.Filter = "Gif files (*.gif;)|*.gif;";
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        trackBar1.Enabled = false;
                        if (dialog.FileNames.Length == 5)
                        {
                            trackBar1.Value = 0;
                            for (int w = 0; w < dialog.FileNames.Length; w++)
                            {
                                if (w < 5)
                                {
                                    Bitmap bmp = new Bitmap(dialog.FileNames[w]); // dialog.Filename replaced by dialog.FileNames[w]
                                    AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currBody, _currAction, _currDir);
                                    if (edit != null)
                                    {
                                        //Gif Especial Properties
                                        if (dialog.FileName.Contains(".gif"))
                                        {
                                            FrameDimension dimension = new FrameDimension(bmp.FrameDimensionsList[0]);
                                            // Number of frames 
                                            int frameCount = bmp.GetFrameCount(dimension);
                                            progressBar1.Maximum = frameCount;
                                            bmp.SelectActiveFrame(dimension, 0);
                                            edit.GetGifPalette(bmp);
                                            Bitmap[] bitbmp = new Bitmap[frameCount];
                                            // Return an Image at a certain index 
                                            for (int index = 0; index < frameCount; index++)
                                            {
                                                bitbmp[index] = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format16bppArgb1555);
                                                bmp.SelectActiveFrame(dimension, index);
                                                bitbmp[index] = (Bitmap)bmp;
                                                bitbmp[index] = Utils.ConvertBmpAnim(bitbmp[index], (int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                                                edit.AddFrame(bitbmp[index]);
                                                TreeNode node = GetNode(_currBody);
                                                if (node != null)
                                                {
                                                    node.ForeColor = Color.Black;
                                                    node.Nodes[_currAction].ForeColor = Color.Black;
                                                }
                                                ListViewItem item;
                                                int i = edit.Frames.Count - 1;
                                                item = new ListViewItem(i.ToString(), 0)
                                                {
                                                    Tag = i
                                                };
                                                listView1.Items.Add(item);
                                                int width = listView1.TileSize.Width - 5;
                                                if (bmp.Width > listView1.TileSize.Width)
                                                {
                                                    width = bmp.Width;
                                                }

                                                int height = listView1.TileSize.Height - 5;
                                                if (bmp.Height > listView1.TileSize.Height)
                                                {
                                                    height = bmp.Height;
                                                }

                                                listView1.TileSize = new Size(width + 5, height + 5);
                                                trackBar2.Maximum = i;
                                                Options.ChangedUltimaClass["Animations"] = true;
                                                if (progressBar1.Value < progressBar1.Maximum)
                                                {
                                                    progressBar1.Value++;
                                                    progressBar1.Invalidate();
                                                }
                                            }
                                            progressBar1.Value = 0;
                                            progressBar1.Invalidate();
                                            SetPaletteBox();
                                            listView1.Invalidate();
                                            numericUpDownCx.Value = edit.Frames[trackBar2.Value].Center.X;
                                            numericUpDownCy.Value = edit.Frames[trackBar2.Value].Center.Y;
                                            Options.ChangedUltimaClass["Animations"] = true;
                                        }
                                    }
                                    if ((w < 4) & (w < dialog.FileNames.Length - 1))
                                    {
                                        trackBar1.Value++;
                                    }
                                }
                            }
                        }
                        //Looping if dialog.FileNames.Length != 5
                        while (dialog.FileNames.Length != 5)
                        {
                            if (dialog.ShowDialog() == DialogResult.Cancel)
                            {
                                break;
                            }

                            if (dialog.FileNames.Length != 5)
                            {
                                dialog.ShowDialog();
                            }

                            if (dialog.FileNames.Length == 5)
                            {
                                trackBar1.Value = 0;
                                for (int w = 0; w < dialog.FileNames.Length; w++)
                                {
                                    if (w < 5)
                                    {
                                        Bitmap bmp = new Bitmap(dialog.FileNames[w]); // dialog.Filename replaced by dialog.FileNames[w]
                                        AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currBody, _currAction, _currDir);
                                        if (edit != null)
                                        {
                                            //Gif Especial Properties
                                            if (dialog.FileName.Contains(".gif"))
                                            {
                                                FrameDimension dimension = new FrameDimension(bmp.FrameDimensionsList[0]);
                                                // Number of frames 
                                                int frameCount = bmp.GetFrameCount(dimension);
                                                progressBar1.Maximum = frameCount;
                                                bmp.SelectActiveFrame(dimension, 0);
                                                edit.GetGifPalette(bmp);
                                                Bitmap[] bitbmp = new Bitmap[frameCount];
                                                // Return an Image at a certain index 
                                                for (int index = 0; index < frameCount; index++)
                                                {
                                                    bitbmp[index] = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format16bppArgb1555);
                                                    bmp.SelectActiveFrame(dimension, index);
                                                    bitbmp[index] = (Bitmap)bmp;
                                                    bitbmp[index] = Utils.ConvertBmpAnim(bitbmp[index], (int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                                                    edit.AddFrame(bitbmp[index]);
                                                    TreeNode node = GetNode(_currBody);
                                                    if (node != null)
                                                    {
                                                        node.ForeColor = Color.Black;
                                                        node.Nodes[_currAction].ForeColor = Color.Black;
                                                    }
                                                    ListViewItem item;
                                                    int i = edit.Frames.Count - 1;
                                                    item = new ListViewItem(i.ToString(), 0)
                                                    {
                                                        Tag = i
                                                    };
                                                    listView1.Items.Add(item);
                                                    int width = listView1.TileSize.Width - 5;
                                                    if (bmp.Width > listView1.TileSize.Width)
                                                    {
                                                        width = bmp.Width;
                                                    }

                                                    int height = listView1.TileSize.Height - 5;
                                                    if (bmp.Height > listView1.TileSize.Height)
                                                    {
                                                        height = bmp.Height;
                                                    }

                                                    listView1.TileSize = new Size(width + 5, height + 5);
                                                    trackBar2.Maximum = i;
                                                    Options.ChangedUltimaClass["Animations"] = true;
                                                    if (progressBar1.Value < progressBar1.Maximum)
                                                    {
                                                        progressBar1.Value++;
                                                        progressBar1.Invalidate();
                                                    }
                                                }
                                                progressBar1.Value = 0;
                                                progressBar1.Invalidate();
                                                SetPaletteBox();
                                                listView1.Invalidate();
                                                numericUpDownCx.Value = edit.Frames[trackBar2.Value].Center.X;
                                                numericUpDownCy.Value = edit.Frames[trackBar2.Value].Center.Y;
                                                Options.ChangedUltimaClass["Animations"] = true;
                                            }
                                        }
                                        if ((w < 4) & (w < dialog.FileNames.Length - 1))
                                        {
                                            trackBar1.Value++;
                                        }
                                    }
                                }
                            }
                        }
                        trackBar1.Enabled = true;
                    }
                }
            }
            //Refresh List
            _currDir = trackBar1.Value;
            AfterSelectTreeView(null, null);
        }

        private void ToolStripButton10_Click(object sender, EventArgs e)
        {
            _drawEmpty = !_drawEmpty;
            pictureBox1.Invalidate();
        }

        private void ToolStripButton7_Click(object sender, EventArgs e)
        {
            _drawFull = !_drawFull;
            pictureBox1.Invalidate();
        }

        private void AnimationEdit_FormClosing(object sender, FormClosingEventArgs e)
        {
            _drawFull = false;
            _drawEmpty = false;
            _lockbutton = false;
            timer1.Enabled = false;
            _blackUndraw = new Pen(Color.FromArgb(255, 0, 0, 0), 1);
            _whiteUndraw = new SolidBrush(Color.FromArgb(255, 255, 255, 255));
            _loaded = false;
            FiddlerControls.Events.FilePathChangeEvent -= OnFilePathChangeEvent;
        }

        //Play Button Timer
        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (trackBar2.Value < trackBar2.Maximum)
            {
                trackBar2.Value++;
            }
            else
            {
                trackBar2.Value = 0;
            }

            pictureBox1.Invalidate();
        }

        //Play Button
        private void ToolStripButton11_Click(object sender, EventArgs e)
        {
            if (timer1.Enabled)
            {
                timer1.Enabled = false;
                numericUpDownCx.Enabled = true;
                numericUpDownCy.Enabled = true;
                trackBar2.Enabled = true;
                button2.Enabled = true; //Same Center button
                toolStripButton12.Enabled = true; //Undraw Referencial Point button
                if (toolStripButton12.Checked)
                {
                    toolStripButton6.Enabled = false; //Lock button
                    _blackUndraw = new Pen(Color.FromArgb(0, 0, 0, 0), 1);
                    _whiteUndraw = new SolidBrush(Color.FromArgb(0, 255, 255, 255));
                }
                else
                {
                    toolStripButton6.Enabled = true;
                    _blackUndraw = new Pen(Color.FromArgb(255, 0, 0, 0), 1);
                    _whiteUndraw = new SolidBrush(Color.FromArgb(255, 255, 255, 255));
                }
                if (toolStripButton6.Checked || toolStripButton12.Checked)
                {
                    numericUpDown2.Enabled = false;
                    numericUpDown1.Enabled = false;
                }
                else
                {
                    numericUpDown2.Enabled = true;
                    numericUpDown1.Enabled = true;
                }
            }
            else
            {
                timer1.Enabled = true;
                numericUpDownCx.Enabled = false;
                numericUpDownCy.Enabled = false;
                numericUpDown2.Enabled = false;
                numericUpDown1.Enabled = false;
                trackBar2.Enabled = false;
                button2.Enabled = false; //Same Center button
                toolStripButton12.Enabled = false; //Undraw Referencial Point button
                toolStripButton6.Enabled = false; //Lock button
                _blackUndraw = new Pen(Color.FromArgb(0, 0, 0, 0), 1);
                _whiteUndraw = new SolidBrush(Color.FromArgb(0, 255, 255, 255));
            }
            pictureBox1.Invalidate();
        }

        //Animation Speed
        private void TrackBar3_ValueChanged(object sender, EventArgs e)
        {
            timer1.Interval = 50 + trackBar3.Value * 30;
        }

        private void ToolStripButton12_Click(object sender, EventArgs e)
        {
            if (!toolStripButton12.Checked)
            {
                _blackUndraw = new Pen(Color.FromArgb(255, 0, 0, 0), 1);
                _whiteUndraw = new SolidBrush(Color.FromArgb(255, 255, 255, 255));
                toolStripButton6.Enabled = true;
                if (toolStripButton6.Checked)
                {
                    numericUpDown2.Enabled = false;
                    numericUpDown1.Enabled = false;
                }
                else
                {
                    numericUpDown2.Enabled = true;
                    numericUpDown1.Enabled = true;
                }
            }
            else
            {
                _blackUndraw = new Pen(Color.FromArgb(0, 0, 0, 0), 1);
                _whiteUndraw = new SolidBrush(Color.FromArgb(0, 255, 255, 255));
                toolStripButton6.Enabled = false;
                numericUpDown2.Enabled = false;
                numericUpDown1.Enabled = false;
            }
            pictureBox1.Invalidate();
        }

        //All Directions with Canvas
        private void AllDirectionsAddWithCanvasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (_fileType != 0)
                {
                    using (OpenFileDialog dialog = new OpenFileDialog())
                    {
                        dialog.Multiselect = true;
                        dialog.Title = "Choose 5 Gifs to add";
                        dialog.CheckFileExists = true;
                        dialog.Filter = "Gif files (*.gif;)|*.gif;";
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            Color customConvert = Color.FromArgb(255, (int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                            trackBar1.Enabled = false;
                            if (dialog.FileNames.Length == 5)
                            {
                                trackBar1.Value = 0;
                                for (int w = 0; w < dialog.FileNames.Length; w++)
                                {
                                    if (w < 5)
                                    {
                                        Bitmap bmp = new Bitmap(dialog.FileNames[w]); // dialog.Filename replaced by dialog.FileNames[w]
                                        AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currBody, _currAction, _currDir);
                                        if (edit != null)
                                        {
                                            //Gif Especial Properties
                                            if (dialog.FileName.Contains(".gif"))
                                            {
                                                FrameDimension dimension = new FrameDimension(bmp.FrameDimensionsList[0]);
                                                // Number of frames 
                                                int frameCount = bmp.GetFrameCount(dimension);
                                                progressBar1.Maximum = frameCount;
                                                bmp.SelectActiveFrame(dimension, 0);
                                                edit.GetGifPalette(bmp);
                                                Bitmap[] bitbmp = new Bitmap[frameCount];
                                                // Return an Image at a certain index 
                                                for (int index = 0; index < frameCount; index++)
                                                {
                                                    bitbmp[index] = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format16bppArgb1555);
                                                    bmp.SelectActiveFrame(dimension, index);
                                                    bitbmp[index] = (Bitmap)bmp;
                                                }
                                                //Canvas algorithm
                                                int top = 0;
                                                int bot = 0;
                                                int left = 0;
                                                int right = 0;
                                                int regressT = -1;
                                                int regressB = -1;
                                                int regressL = -1;
                                                int regressR = -1;
                                                bool var = true;
                                                bool breakOk = false;
                                                //Top
                                                for (int yf = 0; yf < bitbmp[0].Height; yf++)
                                                {
                                                    for (int fram = 0; fram < frameCount; fram++)
                                                    {
                                                        bitbmp[fram].SelectActiveFrame(dimension, fram);
                                                        for (int xf = 0; xf < bitbmp[0].Width; xf++)
                                                        {
                                                            Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                            if (pixel == WhiteConvert | pixel == customConvert | pixel.A == 0)
                                                            {
                                                                var = true;
                                                            }

                                                            if (pixel != WhiteConvert & pixel != customConvert & pixel.A != 0)
                                                            {
                                                                var = false;
                                                                if (yf != 0)
                                                                {
                                                                    regressT++;
                                                                    yf -= 1;
                                                                    xf = -1;
                                                                    fram = 0;
                                                                }
                                                                else
                                                                {
                                                                    breakOk = true;
                                                                    break;
                                                                }
                                                            }
                                                            if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount - 1 && regressT == -1 && yf < bitbmp[0].Height - 9)
                                                            {
                                                                top += 10;
                                                            }

                                                            if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount - 1 && regressT == -1 && yf >= bitbmp[0].Height - 9)
                                                            {
                                                                top++;
                                                            }

                                                            if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount - 1 && regressT != -1)
                                                            {
                                                                top -= regressT;
                                                                breakOk = true;
                                                                break;
                                                            }
                                                        }
                                                        if (breakOk)
                                                        {
                                                            break;
                                                        }
                                                    }
                                                    if (yf < bitbmp[0].Height - 9)
                                                    {
                                                        yf += 9; // 1 of for + 9
                                                    }

                                                    if (breakOk)
                                                    {
                                                        breakOk = false;
                                                        break;
                                                    }
                                                }
                                                //Bot
                                                for (int yf = bitbmp[0].Height - 1; yf > 0; yf--)
                                                {
                                                    for (int fram = 0; fram < frameCount; fram++)
                                                    {
                                                        bitbmp[fram].SelectActiveFrame(dimension, fram);
                                                        for (int xf = 0; xf < bitbmp[0].Width; xf++)
                                                        {
                                                            Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                            if (pixel == WhiteConvert | pixel == customConvert | pixel.A == 0)
                                                            {
                                                                var = true;
                                                            }

                                                            if (pixel != WhiteConvert & pixel != customConvert & pixel.A != 0)
                                                            {
                                                                var = false;
                                                                if (yf != bitbmp[0].Height - 1)
                                                                {
                                                                    regressB++;
                                                                    yf += 1;
                                                                    xf = -1;
                                                                    fram = 0;
                                                                }
                                                                else
                                                                {
                                                                    breakOk = true;
                                                                    break;
                                                                }
                                                            }
                                                            if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount - 1 && regressB == -1 && yf > 9)
                                                            {
                                                                bot += 10;
                                                            }

                                                            if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount - 1 && regressB == -1 && yf <= 9)
                                                            {
                                                                bot++;
                                                            }

                                                            if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount - 1 && regressB != -1)
                                                            {
                                                                bot -= regressB;
                                                                breakOk = true;
                                                                break;
                                                            }
                                                        }
                                                        if (breakOk)
                                                        {
                                                            break;
                                                        }
                                                    }
                                                    if (yf > 9)
                                                    {
                                                        yf -= 9; // 1 of for + 9
                                                    }

                                                    if (breakOk)
                                                    {
                                                        breakOk = false;
                                                        break;
                                                    }
                                                }
                                                //Left

                                                for (int xf = 0; xf < bitbmp[0].Width; xf++)
                                                {
                                                    for (int fram = 0; fram < frameCount; fram++)
                                                    {
                                                        bitbmp[fram].SelectActiveFrame(dimension, fram);
                                                        for (int yf = 0; yf < bitbmp[0].Height; yf++)
                                                        {
                                                            Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                            if (pixel == WhiteConvert | pixel == customConvert | pixel.A == 0)
                                                            {
                                                                var = true;
                                                            }

                                                            if (pixel != WhiteConvert & pixel != customConvert & pixel.A != 0)
                                                            {
                                                                var = false;
                                                                if (xf != 0)
                                                                {
                                                                    regressL++;
                                                                    xf -= 1;
                                                                    yf = -1;
                                                                    fram = 0;
                                                                }
                                                                else
                                                                {
                                                                    breakOk = true;
                                                                    break;
                                                                }
                                                            }
                                                            if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount - 1 && regressL == -1 && xf < bitbmp[0].Width - 9)
                                                            {
                                                                left += 10;
                                                            }

                                                            if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount - 1 && regressL == -1 && xf >= bitbmp[0].Width - 9)
                                                            {
                                                                left++;
                                                            }

                                                            if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount - 1 && regressL != -1)
                                                            {
                                                                left -= regressL;
                                                                breakOk = true;
                                                                break;
                                                            }
                                                        }
                                                        if (breakOk)
                                                        {
                                                            break;
                                                        }
                                                    }
                                                    if (xf < bitbmp[0].Width - 9)
                                                    {
                                                        xf += 9; // 1 of for + 9
                                                    }

                                                    if (breakOk)
                                                    {
                                                        breakOk = false;
                                                        break;
                                                    }
                                                }
                                                //Right
                                                for (int xf = bitbmp[0].Width - 1; xf > 0; xf--)
                                                {
                                                    for (int fram = 0; fram < frameCount; fram++)
                                                    {
                                                        bitbmp[fram].SelectActiveFrame(dimension, fram);
                                                        for (int yf = 0; yf < bitbmp[0].Height; yf++)
                                                        {
                                                            Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                            if (pixel == WhiteConvert | pixel == customConvert | pixel.A == 0)
                                                            {
                                                                var = true;
                                                            }

                                                            if (pixel != WhiteConvert & pixel != customConvert & pixel.A != 0)
                                                            {
                                                                var = false;
                                                                if (xf != bitbmp[0].Width - 1)
                                                                {
                                                                    regressR++;
                                                                    xf += 1;
                                                                    yf = -1;
                                                                    fram = 0;
                                                                }
                                                                else
                                                                {
                                                                    breakOk = true;
                                                                    break;
                                                                }
                                                            }
                                                            if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount - 1 && regressR == -1 && xf > 9)
                                                            {
                                                                right += 10;
                                                            }

                                                            if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount - 1 && regressR == -1 && xf <= 9)
                                                            {
                                                                right++;
                                                            }

                                                            if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount - 1 && regressR != -1)
                                                            {
                                                                right -= regressR;
                                                                breakOk = true;
                                                                break;
                                                            }
                                                        }
                                                        if (breakOk)
                                                        {
                                                            break;
                                                        }
                                                    }
                                                    if (xf > 9)
                                                    {
                                                        xf -= 9; // 1 of for + 9
                                                    }

                                                    if (breakOk)
                                                    {
                                                        breakOk = false;
                                                        break;
                                                    }
                                                }

                                                for (int index = 0; index < frameCount; index++)
                                                {
                                                    Rectangle rect = new Rectangle(left, top, bitbmp[index].Width - left - right, bitbmp[index].Height - top - bot);
                                                    bitbmp[index].SelectActiveFrame(dimension, index);
                                                    bitbmp[index] = bitbmp[index].Clone(rect, PixelFormat.Format16bppArgb1555);
                                                }

                                                //End of Canvas
                                                for (int index = 0; index < frameCount; index++)
                                                {
                                                    bitbmp[index].SelectActiveFrame(dimension, index);
                                                    bitbmp[index] = Utils.ConvertBmpAnim(bitbmp[index], (int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                                                    edit.AddFrame(bitbmp[index]);
                                                    TreeNode node = GetNode(_currBody);
                                                    if (node != null)
                                                    {
                                                        node.ForeColor = Color.Black;
                                                        node.Nodes[_currAction].ForeColor = Color.Black;
                                                    }
                                                    ListViewItem item;
                                                    int i = edit.Frames.Count - 1;
                                                    item = new ListViewItem(i.ToString(), 0)
                                                    {
                                                        Tag = i
                                                    };
                                                    listView1.Items.Add(item);
                                                    int width = listView1.TileSize.Width - 5;
                                                    if (bmp.Width > listView1.TileSize.Width)
                                                    {
                                                        width = bmp.Width;
                                                    }

                                                    int height = listView1.TileSize.Height - 5;
                                                    if (bmp.Height > listView1.TileSize.Height)
                                                    {
                                                        height = bmp.Height;
                                                    }

                                                    listView1.TileSize = new Size(width + 5, height + 5);
                                                    trackBar2.Maximum = i;
                                                    Options.ChangedUltimaClass["Animations"] = true;
                                                    if (progressBar1.Value < progressBar1.Maximum)
                                                    {
                                                        progressBar1.Value++;
                                                        progressBar1.Invalidate();
                                                    }
                                                }
                                                progressBar1.Value = 0;
                                                progressBar1.Invalidate();
                                                SetPaletteBox();
                                                listView1.Invalidate();
                                                numericUpDownCx.Value = edit.Frames[trackBar2.Value].Center.X;
                                                numericUpDownCy.Value = edit.Frames[trackBar2.Value].Center.Y;
                                                Options.ChangedUltimaClass["Animations"] = true;
                                            }
                                        }
                                        if ((w < 4) & (w < dialog.FileNames.Length - 1))
                                        {
                                            trackBar1.Value++;
                                        }
                                    }
                                }
                            }

                            //Looping if dialog.FileNames.Length != 5
                            while (dialog.FileNames.Length != 5)
                            {
                                if (dialog.ShowDialog() == DialogResult.Cancel)
                                {
                                    break;
                                }

                                if (dialog.FileNames.Length != 5)
                                {
                                    dialog.ShowDialog();
                                }

                                if (dialog.FileNames.Length == 5)
                                {
                                    trackBar1.Value = 0;
                                    for (int w = 0; w < dialog.FileNames.Length; w++)
                                    {
                                        if (w < 5)
                                        {
                                            Bitmap bmp = new Bitmap(dialog.FileNames[w]); // dialog.Filename replaced by dialog.FileNames[w]
                                            AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currBody, _currAction, _currDir);
                                            if (edit != null)
                                            {
                                                //Gif Especial Properties
                                                if (dialog.FileName.Contains(".gif"))
                                                {
                                                    FrameDimension dimension = new FrameDimension(bmp.FrameDimensionsList[0]);
                                                    // Number of frames 
                                                    int frameCount = bmp.GetFrameCount(dimension);
                                                    bmp.SelectActiveFrame(dimension, 0);
                                                    edit.GetGifPalette(bmp);
                                                    Bitmap[] bitbmp = new Bitmap[frameCount];
                                                    progressBar1.Maximum = frameCount;
                                                    // Return an Image at a certain index 
                                                    for (int index = 0; index < frameCount; index++)
                                                    {
                                                        bitbmp[index] = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format16bppArgb1555);
                                                        bmp.SelectActiveFrame(dimension, index);
                                                        bitbmp[index] = (Bitmap)bmp;
                                                    }
                                                    //Canvas algorithm
                                                    int top = 0;
                                                    int bot = 0;
                                                    int left = 0;
                                                    int right = 0;
                                                    int regressT = -1;
                                                    int regressB = -1;
                                                    int regressL = -1;
                                                    int regressR = -1;
                                                    bool var = true;
                                                    bool breakOk = false;
                                                    //Top
                                                    for (int yf = 0; yf < bitbmp[0].Height; yf++)
                                                    {
                                                        for (int fram = 0; fram < frameCount; fram++)
                                                        {
                                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                                            for (int xf = 0; xf < bitbmp[0].Width; xf++)
                                                            {
                                                                Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                                if (pixel == WhiteConvert | pixel == customConvert | pixel.A == 0)
                                                                {
                                                                    var = true;
                                                                }

                                                                if (pixel != WhiteConvert & pixel != customConvert & pixel.A != 0)
                                                                {
                                                                    var = false;
                                                                    if (yf != 0)
                                                                    {
                                                                        regressT++;
                                                                        yf -= 1;
                                                                        xf = -1;
                                                                        fram = 0;
                                                                    }
                                                                    else
                                                                    {
                                                                        breakOk = true;
                                                                        break;
                                                                    }
                                                                }
                                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount - 1 && regressT == -1 && yf < bitbmp[0].Height - 9)
                                                                {
                                                                    top += 10;
                                                                }

                                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount - 1 && regressT == -1 && yf >= bitbmp[0].Height - 9)
                                                                {
                                                                    top++;
                                                                }

                                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount - 1 && regressT != -1)
                                                                {
                                                                    top -= regressT;
                                                                    breakOk = true;
                                                                    break;
                                                                }
                                                            }
                                                            if (breakOk)
                                                            {
                                                                break;
                                                            }
                                                        }
                                                        if (yf < bitbmp[0].Height - 9)
                                                        {
                                                            yf += 9; // 1 of for + 9
                                                        }

                                                        if (breakOk)
                                                        {
                                                            breakOk = false;
                                                            break;
                                                        }
                                                    }
                                                    //Bot
                                                    for (int yf = bitbmp[0].Height - 1; yf > 0; yf--)
                                                    {
                                                        for (int fram = 0; fram < frameCount; fram++)
                                                        {
                                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                                            for (int xf = 0; xf < bitbmp[0].Width; xf++)
                                                            {
                                                                Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                                if (pixel == WhiteConvert | pixel == customConvert | pixel.A == 0)
                                                                {
                                                                    var = true;
                                                                }

                                                                if (pixel != WhiteConvert & pixel != customConvert & pixel.A != 0)
                                                                {
                                                                    var = false;
                                                                    if (yf != bitbmp[0].Height - 1)
                                                                    {
                                                                        regressB++;
                                                                        yf += 1;
                                                                        xf = -1;
                                                                        fram = 0;
                                                                    }
                                                                    else
                                                                    {
                                                                        breakOk = true;
                                                                        break;
                                                                    }
                                                                }
                                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount - 1 && regressB == -1 && yf > 9)
                                                                {
                                                                    bot += 10;
                                                                }

                                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount - 1 && regressB == -1 && yf <= 9)
                                                                {
                                                                    bot++;
                                                                }

                                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount - 1 && regressB != -1)
                                                                {
                                                                    bot -= regressB;
                                                                    breakOk = true;
                                                                    break;
                                                                }
                                                            }
                                                            if (breakOk)
                                                            {
                                                                break;
                                                            }
                                                        }
                                                        if (yf > 9)
                                                        {
                                                            yf -= 9; // 1 of for + 9
                                                        }

                                                        if (breakOk)
                                                        {
                                                            breakOk = false;
                                                            break;
                                                        }
                                                    }
                                                    //Left

                                                    for (int xf = 0; xf < bitbmp[0].Width; xf++)
                                                    {
                                                        for (int fram = 0; fram < frameCount; fram++)
                                                        {
                                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                                            for (int yf = 0; yf < bitbmp[0].Height; yf++)
                                                            {
                                                                Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                                if (pixel == WhiteConvert | pixel == customConvert | pixel.A == 0)
                                                                {
                                                                    var = true;
                                                                }

                                                                if (pixel != WhiteConvert & pixel != customConvert & pixel.A != 0)
                                                                {
                                                                    var = false;
                                                                    if (xf != 0)
                                                                    {
                                                                        regressL++;
                                                                        xf -= 1;
                                                                        yf = -1;
                                                                        fram = 0;
                                                                    }
                                                                    else
                                                                    {
                                                                        breakOk = true;
                                                                        break;
                                                                    }
                                                                }
                                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount - 1 && regressL == -1 && xf < bitbmp[0].Width - 9)
                                                                {
                                                                    left += 10;
                                                                }

                                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount - 1 && regressL == -1 && xf >= bitbmp[0].Width - 9)
                                                                {
                                                                    left++;
                                                                }

                                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount - 1 && regressL != -1)
                                                                {
                                                                    left -= regressL;
                                                                    breakOk = true;
                                                                    break;
                                                                }
                                                            }
                                                            if (breakOk)
                                                            {
                                                                break;
                                                            }
                                                        }
                                                        if (xf < bitbmp[0].Width - 9)
                                                        {
                                                            xf += 9; // 1 of for + 9
                                                        }

                                                        if (breakOk)
                                                        {
                                                            breakOk = false;
                                                            break;
                                                        }
                                                    }
                                                    //Right
                                                    for (int xf = bitbmp[0].Width - 1; xf > 0; xf--)
                                                    {
                                                        for (int fram = 0; fram < frameCount; fram++)
                                                        {
                                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                                            for (int yf = 0; yf < bitbmp[0].Height; yf++)
                                                            {
                                                                Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                                if (pixel == WhiteConvert | pixel == customConvert | pixel.A == 0)
                                                                {
                                                                    var = true;
                                                                }

                                                                if (pixel != WhiteConvert & pixel != customConvert & pixel.A != 0)
                                                                {
                                                                    var = false;
                                                                    if (xf != bitbmp[0].Width - 1)
                                                                    {
                                                                        regressR++;
                                                                        xf += 1;
                                                                        yf = -1;
                                                                        fram = 0;
                                                                    }
                                                                    else
                                                                    {
                                                                        breakOk = true;
                                                                        break;
                                                                    }
                                                                }
                                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount - 1 && regressR == -1 && xf > 9)
                                                                {
                                                                    right += 10;
                                                                }

                                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount - 1 && regressR == -1 && xf <= 9)
                                                                {
                                                                    right++;
                                                                }

                                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount - 1 && regressR != -1)
                                                                {
                                                                    right -= regressR;
                                                                    breakOk = true;
                                                                    break;
                                                                }
                                                            }
                                                            if (breakOk)
                                                            {
                                                                break;
                                                            }
                                                        }
                                                        if (xf > 9)
                                                        {
                                                            xf -= 9; // 1 of for + 9
                                                        }

                                                        if (breakOk)
                                                        {
                                                            breakOk = false;
                                                            break;
                                                        }
                                                    }

                                                    for (int index = 0; index < frameCount; index++)
                                                    {
                                                        Rectangle rect = new Rectangle(left, top, bitbmp[index].Width - left - right, bitbmp[index].Height - top - bot);
                                                        bitbmp[index].SelectActiveFrame(dimension, index);
                                                        bitbmp[index] = bitbmp[index].Clone(rect, PixelFormat.Format16bppArgb1555);
                                                    }

                                                    //End of Canvas
                                                    for (int index = 0; index < frameCount; index++)
                                                    {
                                                        bitbmp[index].SelectActiveFrame(dimension, index);
                                                        bitbmp[index] = Utils.ConvertBmpAnim(bitbmp[index], (int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                                                        edit.AddFrame(bitbmp[index]);
                                                        TreeNode node = GetNode(_currBody);
                                                        if (node != null)
                                                        {
                                                            node.ForeColor = Color.Black;
                                                            node.Nodes[_currAction].ForeColor = Color.Black;
                                                        }
                                                        ListViewItem item;
                                                        int i = edit.Frames.Count - 1;
                                                        item = new ListViewItem(i.ToString(), 0)
                                                        {
                                                            Tag = i
                                                        };
                                                        listView1.Items.Add(item);
                                                        int width = listView1.TileSize.Width - 5;
                                                        if (bmp.Width > listView1.TileSize.Width)
                                                        {
                                                            width = bmp.Width;
                                                        }

                                                        int height = listView1.TileSize.Height - 5;
                                                        if (bmp.Height > listView1.TileSize.Height)
                                                        {
                                                            height = bmp.Height;
                                                        }

                                                        listView1.TileSize = new Size(width + 5, height + 5);
                                                        trackBar2.Maximum = i;
                                                        Options.ChangedUltimaClass["Animations"] = true;
                                                        if (progressBar1.Value < progressBar1.Maximum)
                                                        {
                                                            progressBar1.Value++;
                                                            progressBar1.Invalidate();
                                                        }
                                                    }
                                                    progressBar1.Value = 0;
                                                    progressBar1.Invalidate();
                                                    SetPaletteBox();
                                                    listView1.Invalidate();
                                                    numericUpDownCx.Value = edit.Frames[trackBar2.Value].Center.X;
                                                    numericUpDownCy.Value = edit.Frames[trackBar2.Value].Center.Y;
                                                    Options.ChangedUltimaClass["Animations"] = true;
                                                }
                                            }
                                            if ((w < 4) & (w < dialog.FileNames.Length - 1))
                                            {
                                                trackBar1.Value++;
                                            }
                                        }
                                    }
                                }
                            }
                            trackBar1.Enabled = true;
                        }
                    }
                }
                //Refresh List after Canvas reduction
                _currDir = trackBar1.Value;
                AfterSelectTreeView(null, null);
            }
            catch (OutOfMemoryException) { }
        }
        //Add with Canvas
        private void AddWithCanvasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (_fileType != 0)
                {
                    using (OpenFileDialog dialog = new OpenFileDialog())
                    {
                        dialog.Multiselect = false;
                        dialog.Title = "Choose image file to add";
                        dialog.CheckFileExists = true;
                        dialog.Filter = "Gif files (*.gif;)|*.gif;";
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            Color customConvert = Color.FromArgb(255, (int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                            //My Soulblighter Modifications
                            for (int w = 0; w < dialog.FileNames.Length; w++)
                            {
                                Bitmap bmp = new Bitmap(dialog.FileNames[w]); // dialog.Filename replaced by dialog.FileNames[w]
                                AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currBody, _currAction, _currDir);
                                if (edit != null)
                                {
                                    //Gif Especial Properties
                                    if (dialog.FileName.Contains(".gif"))
                                    {
                                        FrameDimension dimension = new FrameDimension(bmp.FrameDimensionsList[0]);
                                        // Number of frames 
                                        int frameCount = bmp.GetFrameCount(dimension);
                                        bmp.SelectActiveFrame(dimension, 0);
                                        edit.GetGifPalette(bmp);
                                        Bitmap[] bitbmp = new Bitmap[frameCount];
                                        progressBar1.Maximum = frameCount;
                                        // Return an Image at a certain index 
                                        for (int index = 0; index < frameCount; index++)
                                        {
                                            bitbmp[index] = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format16bppArgb1555);
                                            bmp.SelectActiveFrame(dimension, index);
                                            bitbmp[index] = (Bitmap)bmp;
                                        }
                                        //Canvas algorithm
                                        int top = 0;
                                        int bot = 0;
                                        int left = 0;
                                        int right = 0;
                                        int regressT = -1;
                                        int regressB = -1;
                                        int regressL = -1;
                                        int regressR = -1;
                                        bool var = true;
                                        bool breakOk = false;
                                        //Top
                                        for (int yf = 0; yf < bitbmp[0].Height; yf++)
                                        {
                                            for (int fram = 0; fram < frameCount; fram++)
                                            {
                                                bitbmp[fram].SelectActiveFrame(dimension, fram);
                                                for (int xf = 0; xf < bitbmp[0].Width; xf++)
                                                {
                                                    Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                    if (pixel == WhiteConvert | pixel == customConvert | pixel.A == 0)
                                                    {
                                                        var = true;
                                                    }

                                                    if (pixel != WhiteConvert & pixel != customConvert & pixel.A != 0)
                                                    {
                                                        var = false;
                                                        if (yf != 0)
                                                        {
                                                            regressT++;
                                                            yf -= 1;
                                                            xf = -1;
                                                            fram = 0;
                                                        }
                                                        else
                                                        {
                                                            breakOk = true;
                                                            break;
                                                        }
                                                    }
                                                    if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount - 1 && regressT == -1 && yf < bitbmp[0].Height - 9)
                                                    {
                                                        top += 10;
                                                    }

                                                    if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount - 1 && regressT == -1 && yf >= bitbmp[0].Height - 9)
                                                    {
                                                        top++;
                                                    }

                                                    if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount - 1 && regressT != -1)
                                                    {
                                                        top -= regressT;
                                                        breakOk = true;
                                                        break;
                                                    }
                                                }
                                                if (breakOk)
                                                {
                                                    break;
                                                }
                                            }
                                            if (yf < bitbmp[0].Height - 9)
                                            {
                                                yf += 9; // 1 of for + 9
                                            }

                                            if (breakOk)
                                            {
                                                breakOk = false;
                                                break;
                                            }
                                        }
                                        //Bot
                                        for (int yf = bitbmp[0].Height - 1; yf > 0; yf--)
                                        {
                                            for (int fram = 0; fram < frameCount; fram++)
                                            {
                                                bitbmp[fram].SelectActiveFrame(dimension, fram);
                                                for (int xf = 0; xf < bitbmp[0].Width; xf++)
                                                {
                                                    Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                    if (pixel == WhiteConvert | pixel == customConvert | pixel.A == 0)
                                                    {
                                                        var = true;
                                                    }

                                                    if (pixel != WhiteConvert & pixel != customConvert & pixel.A != 0)
                                                    {
                                                        var = false;
                                                        if (yf != bitbmp[0].Height - 1)
                                                        {
                                                            regressB++;
                                                            yf += 1;
                                                            xf = -1;
                                                            fram = 0;
                                                        }
                                                        else
                                                        {
                                                            breakOk = true;
                                                            break;
                                                        }
                                                    }
                                                    if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount - 1 && regressB == -1 && yf > 9)
                                                    {
                                                        bot += 10;
                                                    }

                                                    if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount - 1 && regressB == -1 && yf <= 9)
                                                    {
                                                        bot++;
                                                    }

                                                    if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount - 1 && regressB != -1)
                                                    {
                                                        bot -= regressB;
                                                        breakOk = true;
                                                        break;
                                                    }
                                                }
                                                if (breakOk)
                                                {
                                                    break;
                                                }
                                            }
                                            if (yf > 9)
                                            {
                                                yf -= 9; // 1 of for + 9
                                            }

                                            if (breakOk)
                                            {
                                                breakOk = false;
                                                break;
                                            }
                                        }
                                        //Left

                                        for (int xf = 0; xf < bitbmp[0].Width; xf++)
                                        {
                                            for (int fram = 0; fram < frameCount; fram++)
                                            {
                                                bitbmp[fram].SelectActiveFrame(dimension, fram);
                                                for (int yf = 0; yf < bitbmp[0].Height; yf++)
                                                {
                                                    Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                    if (pixel == WhiteConvert | pixel == customConvert | pixel.A == 0)
                                                    {
                                                        var = true;
                                                    }

                                                    if (pixel != WhiteConvert & pixel != customConvert & pixel.A != 0)
                                                    {
                                                        var = false;
                                                        if (xf != 0)
                                                        {
                                                            regressL++;
                                                            xf -= 1;
                                                            yf = -1;
                                                            fram = 0;
                                                        }
                                                        else
                                                        {
                                                            breakOk = true;
                                                            break;
                                                        }
                                                    }
                                                    if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount - 1 && regressL == -1 && xf < bitbmp[0].Width - 9)
                                                    {
                                                        left += 10;
                                                    }

                                                    if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount - 1 && regressL == -1 && xf >= bitbmp[0].Width - 9)
                                                    {
                                                        left++;
                                                    }

                                                    if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount - 1 && regressL != -1)
                                                    {
                                                        left -= regressL;
                                                        breakOk = true;
                                                        break;
                                                    }
                                                }
                                                if (breakOk)
                                                {
                                                    break;
                                                }
                                            }
                                            if (xf < bitbmp[0].Width - 9)
                                            {
                                                xf += 9; // 1 of for + 9
                                            }

                                            if (breakOk)
                                            {
                                                breakOk = false;
                                                break;
                                            }
                                        }
                                        //Right
                                        for (int xf = bitbmp[0].Width - 1; xf > 0; xf--)
                                        {
                                            for (int fram = 0; fram < frameCount; fram++)
                                            {
                                                bitbmp[fram].SelectActiveFrame(dimension, fram);
                                                for (int yf = 0; yf < bitbmp[0].Height; yf++)
                                                {
                                                    Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                    if (pixel == WhiteConvert | pixel == customConvert | pixel.A == 0)
                                                    {
                                                        var = true;
                                                    }

                                                    if (pixel != WhiteConvert & pixel != customConvert & pixel.A != 0)
                                                    {
                                                        var = false;
                                                        if (xf != bitbmp[0].Width - 1)
                                                        {
                                                            regressR++;
                                                            xf += 1;
                                                            yf = -1;
                                                            fram = 0;
                                                        }
                                                        else
                                                        {
                                                            breakOk = true;
                                                            break;
                                                        }
                                                    }
                                                    if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount - 1 && regressR == -1 && xf > 9)
                                                    {
                                                        right += 10;
                                                    }

                                                    if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount - 1 && regressR == -1 && xf <= 9)
                                                    {
                                                        right++;
                                                    }

                                                    if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount - 1 && regressR != -1)
                                                    {
                                                        right -= regressR;
                                                        breakOk = true;
                                                        break;
                                                    }
                                                }
                                                if (breakOk)
                                                {
                                                    break;
                                                }
                                            }
                                            if (xf > 9)
                                            {
                                                xf -= 9; // 1 of for + 9
                                            }

                                            if (breakOk)
                                            {
                                                breakOk = false;
                                                break;
                                            }
                                        }

                                        for (int index = 0; index < frameCount; index++)
                                        {
                                            Rectangle rect = new Rectangle(left, top, bitbmp[index].Width - left - right, bitbmp[index].Height - top - bot);
                                            bitbmp[index].SelectActiveFrame(dimension, index);
                                            bitbmp[index] = bitbmp[index].Clone(rect, PixelFormat.Format16bppArgb1555);
                                        }

                                        //End of Canvas
                                        for (int index = 0; index < frameCount; index++)
                                        {
                                            bitbmp[index].SelectActiveFrame(dimension, index);
                                            bitbmp[index] = Utils.ConvertBmpAnim(bitbmp[index], (int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                                            edit.AddFrame(bitbmp[index]);
                                            TreeNode node = GetNode(_currBody);
                                            if (node != null)
                                            {
                                                node.ForeColor = Color.Black;
                                                node.Nodes[_currAction].ForeColor = Color.Black;
                                            }
                                            ListViewItem item;
                                            int i = edit.Frames.Count - 1;
                                            item = new ListViewItem(i.ToString(), 0)
                                            {
                                                Tag = i
                                            };
                                            listView1.Items.Add(item);
                                            int width = listView1.TileSize.Width - 5;
                                            if (bmp.Width > listView1.TileSize.Width)
                                            {
                                                width = bmp.Width;
                                            }

                                            int height = listView1.TileSize.Height - 5;
                                            if (bmp.Height > listView1.TileSize.Height)
                                            {
                                                height = bmp.Height;
                                            }

                                            listView1.TileSize = new Size(width + 5, height + 5);
                                            trackBar2.Maximum = i;
                                            Options.ChangedUltimaClass["Animations"] = true;
                                            if (progressBar1.Value < progressBar1.Maximum)
                                            {
                                                progressBar1.Value++;
                                                progressBar1.Invalidate();
                                            }
                                        }
                                        progressBar1.Value = 0;
                                        progressBar1.Invalidate();
                                        SetPaletteBox();
                                        listView1.Invalidate();
                                        numericUpDownCx.Value = edit.Frames[trackBar2.Value].Center.X;
                                        numericUpDownCy.Value = edit.Frames[trackBar2.Value].Center.Y;
                                        Options.ChangedUltimaClass["Animations"] = true;
                                    }
                                    //End of Soulblighter Modifications
                                }
                            }
                        }
                    }
                }
                //Refresh List after Canvas reduction
                _currDir = trackBar1.Value;
                AfterSelectTreeView(null, null);
            }
            catch (OutOfMemoryException) { }
        }

        private unsafe void OnClickGeneratePalette(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = true;
                dialog.Title = "Choose images to generate from";
                dialog.CheckFileExists = true;
                dialog.Filter = "Image files (*.tif;*.tiff;*.bmp;*.png;*.jpg;*.jpeg)|*.tif;*.tiff;*.bmp;*.png;*.jpg;*.jpeg";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (string filename in dialog.FileNames)
                    {
                        Bitmap bit = new Bitmap(filename);
                        AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currBody, _currAction, _currDir);
                        if (edit != null)
                        {
                            bit = Utils.ConvertBmpAnim(bit, (int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                            edit.GetImagePalette(bit);
                        }
                        SetPaletteBox();
                        listView1.Invalidate();
                        Options.ChangedUltimaClass["Animations"] = true;
                        SetPaletteBox();
                    }
                }
            }
        }
        //End of Soulblighter Modification

        private void OnClickExportAllToVD(object sender, EventArgs e)
        {
            if (_fileType != 0)
            {
                using (FolderBrowserDialog dialog = new FolderBrowserDialog())
                {
                    dialog.Description = "Select directory";
                    dialog.ShowNewFolderButton = true;
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        for (int i = 0; i < treeView1.Nodes.Count; ++i)
                        {
                            int index = (int)treeView1.Nodes[i].Tag;
                            if (index >= 0 && treeView1.Nodes[i].Parent == null && treeView1.Nodes[i].ForeColor != Color.Red)
                            {
                                string fileName = Path.Combine(dialog.SelectedPath, string.Format("anim{0}_0x{1:X}.vd", _fileType, index));
                                Ultima.AnimationEdit.ExportToVd(_fileType, index, fileName);
                            }
                        }
                        MessageBox.Show(string.Format("All Animations saved to {0}", dialog.SelectedPath),
                                "Export", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                    }
                }
            }
        }

        //Get position of all animations in array
        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                trackBar1.Enabled = false;
                trackBar2.Value = 0;
                button1.Enabled = true;
                for (int count = 0; count < 5;)
                {
                    if (trackBar1.Value < 4)
                    {
                        AnimCx[trackBar1.Value] = (int)numericUpDownCx.Value;
                        AnimCy[trackBar1.Value] = (int)numericUpDownCy.Value;
                        trackBar1.Value++;
                        count++;
                    }
                    else
                    {
                        AnimCx[trackBar1.Value] = (int)numericUpDownCx.Value;
                        AnimCy[trackBar1.Value] = (int)numericUpDownCy.Value;
                        trackBar1.Value = 0;
                        count++;
                    }
                }
                toolStripLabel8.Text = "1: " + AnimCx[0] + "/" + AnimCy[0];
                toolStripLabel9.Text = "2: " + AnimCx[1] + "/" + AnimCy[1];
                toolStripLabel10.Text = "3: " + AnimCx[2] + "/" + AnimCy[2];
                toolStripLabel11.Text = "4: " + AnimCx[3] + "/" + AnimCy[3];
                toolStripLabel12.Text = "5: " + AnimCx[4] + "/" + AnimCy[4];
                trackBar1.Enabled = true;
            }
            else
            {
                toolStripLabel8.Text = "1:    /     ";
                toolStripLabel9.Text = "2:    /     ";
                toolStripLabel10.Text = "3:    /     ";
                toolStripLabel11.Text = "4:    /     ";
                toolStripLabel12.Text = "5:    /     ";
                button1.Enabled = false;
            }
        }

        //Set Button
        private void Button1_Click(object sender, EventArgs e)
        {
            trackBar1.Value = 0;
            trackBar1.Enabled = false;
            for (int i = 0; i <= trackBar1.Maximum; i++)
            {
                try
                {
                    if (_fileType != 0)
                    {
                        AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currBody, _currAction, _currDir);
                        if (edit != null)
                        {
                            if (edit.Frames.Count >= trackBar2.Value)
                            {
                                for (int index = 0; index < edit.Frames.Count; index++)
                                {
                                    edit.Frames[index].ChangeCenter(AnimCx[i], AnimCy[i]);
                                    Options.ChangedUltimaClass["Animations"] = true;
                                    pictureBox1.Invalidate();
                                }
                            }
                        }
                    }
                }
                catch (NullReferenceException) { }
                if (trackBar1.Value < trackBar1.Maximum)
                {
                    trackBar1.Value++;
                }
                else
                {
                    trackBar1.Value = 0;
                }
            }
            trackBar1.Enabled = true;
        }

        //Add Directions with Canvas ( CV5 style GIF )
        private void AddDirectionsAddWithCanvasUniqueImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (_fileType != 0)
                {
                    using (OpenFileDialog dialog = new OpenFileDialog())
                    {
                        dialog.Multiselect = false;
                        dialog.Title = "Choose 1 Gif ( with all directions in CV5 Style ) to add";
                        dialog.CheckFileExists = true;
                        dialog.Filter = "Gif files (*.gif;)|*.gif;";
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            Color customConvert = Color.FromArgb(255, (int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                            trackBar1.Enabled = false;
                            trackBar1.Value = 0;
                            Bitmap bmp = new Bitmap(dialog.FileName);
                            AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currBody, _currAction, _currDir);
                            if (edit != null)
                            {
                                //Gif Especial Properties
                                if (dialog.FileName.Contains(".gif"))
                                {
                                    FrameDimension dimension = new FrameDimension(bmp.FrameDimensionsList[0]);
                                    // Number of frames 
                                    int frameCount = bmp.GetFrameCount(dimension);
                                    progressBar1.Maximum = frameCount;
                                    bmp.SelectActiveFrame(dimension, 0);
                                    edit.GetGifPalette(bmp);
                                    Bitmap[] bitbmp = new Bitmap[frameCount];
                                    // Return an Image at a certain index 
                                    for (int index = 0; index < frameCount; index++)
                                    {
                                        bitbmp[index] = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format16bppArgb1555);
                                        bmp.SelectActiveFrame(dimension, index);
                                        bitbmp[index] = (Bitmap)bmp;
                                    }
                                    //Canvas algorithm
                                    int top = 0;
                                    int bot = 0;
                                    int left = 0;
                                    int right = 0;
                                    int regressT = -1;
                                    int regressB = -1;
                                    int regressL = -1;
                                    int regressR = -1;
                                    bool var = true;
                                    bool breakOk = false;
                                    // position 0
                                    //Top
                                    for (int yf = 0; yf < bitbmp[frameCount / 8 * 4].Height; yf++)
                                    {
                                        for (int fram = frameCount / 8 * 4; fram < frameCount / 8 * 5; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int xf = 0; xf < bitbmp[frameCount / 8 * 4].Width; xf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                if (pixel == WhiteConvert | pixel == GreyConvert | pixel == customConvert | pixel.A == 0)
                                                {
                                                    var = true;
                                                }

                                                if (pixel != WhiteConvert & pixel != GreyConvert & pixel != customConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (yf != 0)
                                                    {
                                                        regressT++;
                                                        yf -= 1;
                                                        xf = -1;
                                                        fram = frameCount / 8 * 4;
                                                    }
                                                    else
                                                    {
                                                        breakOk = true;
                                                        break;
                                                    }
                                                }
                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 8 * 5 - 1 && regressT == -1 && yf < bitbmp[0].Height - 9)
                                                {
                                                    top += 10;
                                                }

                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 8 * 5 - 1 && regressT == -1 && yf >= bitbmp[0].Height - 9)
                                                {
                                                    top++;
                                                }

                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 8 * 5 - 1 && regressT != -1)
                                                {
                                                    top -= regressT;
                                                    breakOk = true;
                                                    break;
                                                }
                                            }
                                            if (breakOk)
                                            {
                                                break;
                                            }
                                        }
                                        if (yf < bitbmp[frameCount / 8 * 4].Height - 9)
                                        {
                                            yf += 9; // 1 of for + 9
                                        }

                                        if (breakOk)
                                        {
                                            breakOk = false;
                                            break;
                                        }
                                    }
                                    //Bot
                                    for (int yf = bitbmp[frameCount / 8 * 4].Height - 1; yf > 0; yf--)
                                    {
                                        for (int fram = frameCount / 8 * 4; fram < frameCount / 8 * 5; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int xf = 0; xf < bitbmp[frameCount / 8 * 4].Width; xf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                if (pixel == WhiteConvert | pixel == GreyConvert | pixel == customConvert | pixel.A == 0)
                                                {
                                                    var = true;
                                                }

                                                if (pixel != WhiteConvert & pixel != GreyConvert & pixel != customConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (yf != bitbmp[frameCount / 8 * 4].Height - 1)
                                                    {
                                                        regressB++;
                                                        yf += 1;
                                                        xf = -1;
                                                        fram = frameCount / 8 * 4;
                                                    }
                                                    else
                                                    {
                                                        breakOk = true;
                                                        break;
                                                    }
                                                }
                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 8 * 5 - 1 && regressB == -1 && yf > 9)
                                                {
                                                    bot += 10;
                                                }

                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 8 * 5 - 1 && regressB == -1 && yf <= 9)
                                                {
                                                    bot++;
                                                }

                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 8 * 5 - 1 && regressB != -1)
                                                {
                                                    bot -= regressB;
                                                    breakOk = true;
                                                    break;
                                                }
                                            }
                                            if (breakOk)
                                            {
                                                break;
                                            }
                                        }
                                        if (yf > 9)
                                        {
                                            yf -= 9; // 1 of for + 9
                                        }

                                        if (breakOk)
                                        {
                                            breakOk = false;
                                            break;
                                        }
                                    }
                                    //Left

                                    for (int xf = 0; xf < bitbmp[frameCount / 8 * 4].Width; xf++)
                                    {
                                        for (int fram = frameCount / 8 * 4; fram < frameCount / 8 * 5; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int yf = 0; yf < bitbmp[frameCount / 8 * 4].Height; yf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                if (pixel == WhiteConvert | pixel == GreyConvert | pixel == customConvert | pixel.A == 0)
                                                {
                                                    var = true;
                                                }

                                                if (pixel != WhiteConvert & pixel != GreyConvert & pixel != customConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (xf != 0)
                                                    {
                                                        regressL++;
                                                        xf -= 1;
                                                        yf = -1;
                                                        fram = frameCount / 8 * 4;
                                                    }
                                                    else
                                                    {
                                                        breakOk = true;
                                                        break;
                                                    }
                                                }
                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 8 * 5 - 1 && regressL == -1 && xf < bitbmp[0].Width - 9)
                                                {
                                                    left += 10;
                                                }

                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 8 * 5 - 1 && regressL == -1 && xf >= bitbmp[0].Width - 9)
                                                {
                                                    left++;
                                                }

                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 8 * 5 - 1 && regressL != -1)
                                                {
                                                    left -= regressL;
                                                    breakOk = true;
                                                    break;
                                                }
                                            }
                                            if (breakOk)
                                            {
                                                break;
                                            }
                                        }
                                        if (xf < bitbmp[frameCount / 8 * 4].Width - 9)
                                        {
                                            xf += 9; // 1 of for + 9
                                        }

                                        if (breakOk)
                                        {
                                            breakOk = false;
                                            break;
                                        }
                                    }
                                    //Right
                                    for (int xf = bitbmp[frameCount / 8 * 4].Width - 1; xf > 0; xf--)
                                    {
                                        for (int fram = frameCount / 8 * 4; fram < frameCount / 8 * 5; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int yf = 0; yf < bitbmp[frameCount / 8 * 4].Height; yf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                if (pixel == WhiteConvert | pixel == GreyConvert | pixel == customConvert | pixel.A == 0)
                                                {
                                                    var = true;
                                                }

                                                if (pixel != WhiteConvert & pixel != GreyConvert & pixel != customConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (xf != bitbmp[frameCount / 8 * 4].Width - 1)
                                                    {
                                                        regressR++;
                                                        xf += 1;
                                                        yf = -1;
                                                        fram = frameCount / 8 * 4;
                                                    }
                                                    else
                                                    {
                                                        breakOk = true;
                                                        break;
                                                    }
                                                }
                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 8 * 5 - 1 && regressR == -1 && xf > 9)
                                                {
                                                    right += 10;
                                                }

                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 8 * 5 - 1 && regressR == -1 && xf <= 9)
                                                {
                                                    right++;
                                                }

                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 8 * 5 - 1 && regressR != -1)
                                                {
                                                    right -= regressR;
                                                    breakOk = true;
                                                    break;
                                                }
                                            }
                                            if (breakOk)
                                            {
                                                break;
                                            }
                                        }
                                        if (xf > 9)
                                        {
                                            xf -= 9; // 1 of for + 9
                                        }

                                        if (breakOk)
                                        {
                                            breakOk = false;
                                            break;
                                        }
                                    }
                                    for (int index = frameCount / 8 * 4; index < frameCount / 8 * 5; index++)
                                    {
                                        Rectangle rect = new Rectangle(left, top, bitbmp[index].Width - left - right, bitbmp[index].Height - top - bot);
                                        bitbmp[index].SelectActiveFrame(dimension, index);
                                        bitbmp[index] = bitbmp[index].Clone(rect, PixelFormat.Format16bppArgb1555);
                                    }

                                    //Reseta cordenadas
                                    top = 0;
                                    bot = 0;
                                    left = 0;
                                    right = 0;
                                    regressT = -1;
                                    regressB = -1;
                                    regressL = -1;
                                    regressR = -1;
                                    var = true;
                                    breakOk = false;
                                    // position 1
                                    //Top
                                    for (int yf = 0; yf < bitbmp[0].Height; yf++)
                                    {
                                        for (int fram = 0; fram < frameCount / 8; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int xf = 0; xf < bitbmp[0].Width; xf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                if (pixel == WhiteConvert | pixel == GreyConvert | pixel == customConvert | pixel.A == 0)
                                                {
                                                    var = true;
                                                }

                                                if (pixel != WhiteConvert & pixel != GreyConvert & pixel != customConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (yf != 0)
                                                    {
                                                        regressT++;
                                                        yf -= 1;
                                                        xf = -1;
                                                        fram = 0;
                                                    }
                                                    else
                                                    {
                                                        breakOk = true;
                                                        break;
                                                    }
                                                }
                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 8 - 1 && regressT == -1 && yf < bitbmp[0].Height - 9)
                                                {
                                                    top += 10;
                                                }

                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 8 - 1 && regressT == -1 && yf >= bitbmp[0].Height - 9)
                                                {
                                                    top++;
                                                }

                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 8 - 1 && regressT != -1)
                                                {
                                                    top -= regressT;
                                                    breakOk = true;
                                                    break;
                                                }
                                            }
                                            if (breakOk)
                                            {
                                                break;
                                            }
                                        }
                                        if (yf < bitbmp[0].Height - 9)
                                        {
                                            yf += 9; // 1 of for + 9
                                        }

                                        if (breakOk)
                                        {
                                            breakOk = false;
                                            break;
                                        }
                                    }
                                    //Bot
                                    for (int yf = bitbmp[0].Height - 1; yf > 0; yf--)
                                    {
                                        for (int fram = 0; fram < frameCount / 8; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int xf = 0; xf < bitbmp[0].Width; xf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                if (pixel == WhiteConvert | pixel == GreyConvert | pixel == customConvert | pixel.A == 0)
                                                {
                                                    var = true;
                                                }

                                                if (pixel != WhiteConvert & pixel != GreyConvert & pixel != customConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (yf != bitbmp[0].Height - 1)
                                                    {
                                                        regressB++;
                                                        yf += 1;
                                                        xf = -1;
                                                        fram = 0;
                                                    }
                                                    else
                                                    {
                                                        breakOk = true;
                                                        break;
                                                    }
                                                }
                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 8 - 1 && regressB == -1 && yf > 9)
                                                {
                                                    bot += 10;
                                                }

                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 8 - 1 && regressB == -1 && yf <= 9)
                                                {
                                                    bot++;
                                                }

                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 8 - 1 && regressB != -1)
                                                {
                                                    bot -= regressB;
                                                    breakOk = true;
                                                    break;
                                                }
                                            }
                                            if (breakOk)
                                            {
                                                break;
                                            }
                                        }
                                        if (yf > 9)
                                        {
                                            yf -= 9; // 1 of for + 9
                                        }

                                        if (breakOk)
                                        {
                                            breakOk = false;
                                            break;
                                        }
                                    }
                                    //Left

                                    for (int xf = 0; xf < bitbmp[0].Width; xf++)
                                    {
                                        for (int fram = 0; fram < frameCount / 8; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int yf = 0; yf < bitbmp[0].Height; yf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                if (pixel == WhiteConvert | pixel == GreyConvert | pixel == customConvert | pixel.A == 0)
                                                {
                                                    var = true;
                                                }

                                                if (pixel != WhiteConvert & pixel != GreyConvert & pixel != customConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (xf != 0)
                                                    {
                                                        regressL++;
                                                        xf -= 1;
                                                        yf = -1;
                                                        fram = 0;
                                                    }
                                                    else
                                                    {
                                                        breakOk = true;
                                                        break;
                                                    }
                                                }
                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 8 - 1 && regressL == -1 && xf < bitbmp[0].Width - 9)
                                                {
                                                    left += 10;
                                                }

                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 8 - 1 && regressL == -1 && xf >= bitbmp[0].Width - 9)
                                                {
                                                    left++;
                                                }

                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 8 - 1 && regressL != -1)
                                                {
                                                    left -= regressL;
                                                    breakOk = true;
                                                    break;
                                                }
                                            }
                                            if (breakOk)
                                            {
                                                break;
                                            }
                                        }
                                        if (xf < bitbmp[0].Width - 9)
                                        {
                                            xf += 9; // 1 of for + 9
                                        }

                                        if (breakOk)
                                        {
                                            breakOk = false;
                                            break;
                                        }
                                    }
                                    //Right
                                    for (int xf = bitbmp[0].Width - 1; xf > 0; xf--)
                                    {
                                        for (int fram = 0; fram < frameCount / 8; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int yf = 0; yf < bitbmp[0].Height; yf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                if (pixel == WhiteConvert | pixel == GreyConvert | pixel == customConvert | pixel.A == 0)
                                                {
                                                    var = true;
                                                }

                                                if (pixel != WhiteConvert & pixel != GreyConvert & pixel != customConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (xf != bitbmp[0].Width - 1)
                                                    {
                                                        regressR++;
                                                        xf += 1;
                                                        yf = -1;
                                                        fram = 0;
                                                    }
                                                    else
                                                    {
                                                        breakOk = true;
                                                        break;
                                                    }
                                                }
                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 8 - 1 && regressR == -1 && xf > 9)
                                                {
                                                    right += 10;
                                                }

                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 8 - 1 && regressR == -1 && xf <= 9)
                                                {
                                                    right++;
                                                }

                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 8 - 1 && regressR != -1)
                                                {
                                                    right -= regressR;
                                                    breakOk = true;
                                                    break;
                                                }
                                            }
                                            if (breakOk)
                                            {
                                                break;
                                            }
                                        }
                                        if (xf > 9)
                                        {
                                            xf -= 9; // 1 of for + 9
                                        }

                                        if (breakOk)
                                        {
                                            breakOk = false;
                                            break;
                                        }
                                    }
                                    for (int index = 0; index < frameCount / 8; index++)
                                    {
                                        Rectangle rect = new Rectangle(left, top, bitbmp[index].Width - left - right, bitbmp[index].Height - top - bot);
                                        bitbmp[index].SelectActiveFrame(dimension, index);
                                        bitbmp[index] = bitbmp[index].Clone(rect, PixelFormat.Format16bppArgb1555);
                                    }

                                    //Reseta cordenadas
                                    top = 0;
                                    bot = 0;
                                    left = 0;
                                    right = 0;
                                    regressT = -1;
                                    regressB = -1;
                                    regressL = -1;
                                    regressR = -1;
                                    var = true;
                                    breakOk = false;
                                    // position 2
                                    //Top
                                    for (int yf = 0; yf < bitbmp[frameCount / 8 * 5].Height; yf++)
                                    {
                                        for (int fram = frameCount / 8 * 5; fram < frameCount / 8 * 6; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int xf = 0; xf < bitbmp[frameCount / 8 * 5].Width; xf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                if (pixel == WhiteConvert | pixel == GreyConvert | pixel == customConvert | pixel.A == 0)
                                                {
                                                    var = true;
                                                }

                                                if (pixel != WhiteConvert & pixel != GreyConvert & pixel != customConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (yf != 0)
                                                    {
                                                        regressT++;
                                                        yf -= 1;
                                                        xf = -1;
                                                        fram = frameCount / 8 * 5;
                                                    }
                                                    else
                                                    {
                                                        breakOk = true;
                                                        break;
                                                    }
                                                }
                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 8 * 6 - 1 && regressT == -1 && yf < bitbmp[0].Height - 9)
                                                {
                                                    top += 10;
                                                }

                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 8 * 6 - 1 && regressT == -1 && yf >= bitbmp[0].Height - 9)
                                                {
                                                    top++;
                                                }

                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 8 * 6 - 1 && regressT != -1)
                                                {
                                                    top -= regressT;
                                                    breakOk = true;
                                                    break;
                                                }
                                            }
                                            if (breakOk)
                                            {
                                                break;
                                            }
                                        }
                                        if (yf < bitbmp[frameCount / 8 * 5].Height - 9)
                                        {
                                            yf += 9; // 1 of for + 9
                                        }

                                        if (breakOk)
                                        {
                                            breakOk = false;
                                            break;
                                        }
                                    }
                                    //Bot
                                    for (int yf = bitbmp[frameCount / 8 * 5].Height - 1; yf > 0; yf--)
                                    {
                                        for (int fram = frameCount / 8 * 5; fram < frameCount / 8 * 6; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int xf = 0; xf < bitbmp[frameCount / 8 * 5].Width; xf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                if (pixel == WhiteConvert | pixel == GreyConvert | pixel == customConvert | pixel.A == 0)
                                                {
                                                    var = true;
                                                }

                                                if (pixel != WhiteConvert & pixel != GreyConvert & pixel != customConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (yf != bitbmp[frameCount / 8 * 5].Height - 1)
                                                    {
                                                        regressB++;
                                                        yf += 1;
                                                        xf = -1;
                                                        fram = frameCount / 8 * 5;
                                                    }
                                                    else
                                                    {
                                                        breakOk = true;
                                                        break;
                                                    }
                                                }
                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 8 * 6 - 1 && regressB == -1 && yf > 9)
                                                {
                                                    bot += 10;
                                                }

                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 8 * 6 - 1 && regressB == -1 && yf <= 9)
                                                {
                                                    bot++;
                                                }

                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 8 * 6 - 1 && regressB != -1)
                                                {
                                                    bot -= regressB;
                                                    breakOk = true;
                                                    break;
                                                }
                                            }
                                            if (breakOk)
                                            {
                                                break;
                                            }
                                        }
                                        if (yf > 9)
                                        {
                                            yf -= 9; // 1 of for + 9
                                        }

                                        if (breakOk)
                                        {
                                            breakOk = false;
                                            break;
                                        }
                                    }
                                    //Left

                                    for (int xf = 0; xf < bitbmp[frameCount / 8 * 5].Width; xf++)
                                    {
                                        for (int fram = frameCount / 8 * 5; fram < frameCount / 8 * 6; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int yf = 0; yf < bitbmp[frameCount / 8 * 5].Height; yf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                if (pixel == WhiteConvert | pixel == GreyConvert | pixel == customConvert | pixel.A == 0)
                                                {
                                                    var = true;
                                                }

                                                if (pixel != WhiteConvert & pixel != GreyConvert & pixel != customConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (xf != 0)
                                                    {
                                                        regressL++;
                                                        xf -= 1;
                                                        yf = -1;
                                                        fram = frameCount / 8 * 5;
                                                    }
                                                    else
                                                    {
                                                        breakOk = true;
                                                        break;
                                                    }
                                                }
                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 8 * 6 - 1 && regressL == -1 && xf < bitbmp[0].Width - 9)
                                                {
                                                    left += 10;
                                                }

                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 8 * 6 - 1 && regressL == -1 && xf >= bitbmp[0].Width - 9)
                                                {
                                                    left++;
                                                }

                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 8 * 6 - 1 && regressL != -1)
                                                {
                                                    left -= regressL;
                                                    breakOk = true;
                                                    break;
                                                }
                                            }
                                            if (breakOk)
                                            {
                                                break;
                                            }
                                        }
                                        if (xf < bitbmp[frameCount / 8 * 5].Width - 9)
                                        {
                                            xf += 9; // 1 of for + 9
                                        }

                                        if (breakOk)
                                        {
                                            breakOk = false;
                                            break;
                                        }
                                    }
                                    //Right
                                    for (int xf = bitbmp[frameCount / 8 * 5].Width - 1; xf > 0; xf--)
                                    {
                                        for (int fram = frameCount / 8 * 5; fram < frameCount / 8 * 6; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int yf = 0; yf < bitbmp[frameCount / 8 * 5].Height; yf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                if (pixel == WhiteConvert | pixel == GreyConvert | pixel == customConvert | pixel.A == 0)
                                                {
                                                    var = true;
                                                }

                                                if (pixel != WhiteConvert & pixel != GreyConvert & pixel != customConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (xf != bitbmp[frameCount / 8 * 5].Width - 1)
                                                    {
                                                        regressR++;
                                                        xf += 1;
                                                        yf = -1;
                                                        fram = frameCount / 8 * 5;
                                                    }
                                                    else
                                                    {
                                                        breakOk = true;
                                                        break;
                                                    }
                                                }
                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 8 * 6 - 1 && regressR == -1 && xf > 9)
                                                {
                                                    right += 10;
                                                }

                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 8 * 6 - 1 && regressR == -1 && xf <= 9)
                                                {
                                                    right++;
                                                }

                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 8 * 6 - 1 && regressR != -1)
                                                {
                                                    right -= regressR;
                                                    breakOk = true;
                                                    break;
                                                }
                                            }
                                            if (breakOk)
                                            {
                                                break;
                                            }
                                        }
                                        if (xf > 9)
                                        {
                                            xf -= 9; // 1 of for + 9
                                        }

                                        if (breakOk)
                                        {
                                            breakOk = false;
                                            break;
                                        }
                                    }
                                    for (int index = frameCount / 8 * 5; index < frameCount / 8 * 6; index++)
                                    {
                                        Rectangle rect = new Rectangle(left, top, bitbmp[index].Width - left - right, bitbmp[index].Height - top - bot);
                                        bitbmp[index].SelectActiveFrame(dimension, index);
                                        bitbmp[index] = bitbmp[index].Clone(rect, PixelFormat.Format16bppArgb1555);
                                    }

                                    //Reseta cordenadas
                                    top = 0;
                                    bot = 0;
                                    left = 0;
                                    right = 0;
                                    regressT = -1;
                                    regressB = -1;
                                    regressL = -1;
                                    regressR = -1;
                                    var = true;
                                    breakOk = false;
                                    // position 3
                                    //Top
                                    for (int yf = 0; yf < bitbmp[frameCount / 8 * 1].Height; yf++)
                                    {
                                        for (int fram = frameCount / 8 * 1; fram < frameCount / 8 * 2; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int xf = 0; xf < bitbmp[frameCount / 8 * 1].Width; xf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                if (pixel == WhiteConvert | pixel == GreyConvert | pixel == customConvert | pixel.A == 0)
                                                {
                                                    var = true;
                                                }

                                                if (pixel != WhiteConvert & pixel != GreyConvert & pixel != customConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (yf != 0)
                                                    {
                                                        regressT++;
                                                        yf -= 1;
                                                        xf = -1;
                                                        fram = frameCount / 8 * 1;
                                                    }
                                                    else
                                                    {
                                                        breakOk = true;
                                                        break;
                                                    }
                                                }
                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 8 * 2 - 1 && regressT == -1 && yf < bitbmp[0].Height - 9)
                                                {
                                                    top += 10;
                                                }

                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 8 * 2 - 1 && regressT == -1 && yf >= bitbmp[0].Height - 9)
                                                {
                                                    top++;
                                                }

                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 8 * 2 - 1 && regressT != -1)
                                                {
                                                    top -= regressT;
                                                    breakOk = true;
                                                    break;
                                                }
                                            }
                                            if (breakOk)
                                            {
                                                break;
                                            }
                                        }
                                        if (yf < bitbmp[frameCount / 8 * 1].Height - 9)
                                        {
                                            yf += 9; // 1 of for + 9
                                        }

                                        if (breakOk)
                                        {
                                            breakOk = false;
                                            break;
                                        }
                                    }
                                    //Bot
                                    for (int yf = bitbmp[frameCount / 8 * 1].Height - 1; yf > 0; yf--)
                                    {
                                        for (int fram = frameCount / 8 * 1; fram < frameCount / 8 * 2; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int xf = 0; xf < bitbmp[frameCount / 8 * 1].Width; xf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                if (pixel == WhiteConvert | pixel == GreyConvert | pixel == customConvert | pixel.A == 0)
                                                {
                                                    var = true;
                                                }

                                                if (pixel != WhiteConvert & pixel != GreyConvert & pixel != customConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (yf != bitbmp[frameCount / 8 * 1].Height - 1)
                                                    {
                                                        regressB++;
                                                        yf += 1;
                                                        xf = -1;
                                                        fram = frameCount / 8 * 1;
                                                    }
                                                    else
                                                    {
                                                        breakOk = true;
                                                        break;
                                                    }
                                                }
                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 8 * 2 - 1 && regressB == -1 && yf > 9)
                                                {
                                                    bot += 10;
                                                }

                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 8 * 2 - 1 && regressB == -1 && yf <= 9)
                                                {
                                                    bot++;
                                                }

                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 8 * 2 - 1 && regressB != -1)
                                                {
                                                    bot -= regressB;
                                                    breakOk = true;
                                                    break;
                                                }
                                            }
                                            if (breakOk)
                                            {
                                                break;
                                            }
                                        }
                                        if (yf > 9)
                                        {
                                            yf -= 9; // 1 of for + 9
                                        }

                                        if (breakOk)
                                        {
                                            breakOk = false;
                                            break;
                                        }
                                    }
                                    //Left

                                    for (int xf = 0; xf < bitbmp[frameCount / 8 * 1].Width; xf++)
                                    {
                                        for (int fram = frameCount / 8 * 1; fram < frameCount / 8 * 2; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int yf = 0; yf < bitbmp[frameCount / 8 * 1].Height; yf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                if (pixel == WhiteConvert | pixel == GreyConvert | pixel == customConvert | pixel.A == 0)
                                                {
                                                    var = true;
                                                }

                                                if (pixel != WhiteConvert & pixel != GreyConvert & pixel != customConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (xf != 0)
                                                    {
                                                        regressL++;
                                                        xf -= 1;
                                                        yf = -1;
                                                        fram = frameCount / 8 * 1;
                                                    }
                                                    else
                                                    {
                                                        breakOk = true;
                                                        break;
                                                    }
                                                }
                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 8 * 2 - 1 && regressL == -1 && xf < bitbmp[0].Width - 9)
                                                {
                                                    left += 10;
                                                }

                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 8 * 2 - 1 && regressL == -1 && xf >= bitbmp[0].Width - 9)
                                                {
                                                    left++;
                                                }

                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 8 * 2 - 1 && regressL != -1)
                                                {
                                                    left -= regressL;
                                                    breakOk = true;
                                                    break;
                                                }
                                            }
                                            if (breakOk)
                                            {
                                                break;
                                            }
                                        }
                                        if (xf < bitbmp[frameCount / 8 * 1].Width - 9)
                                        {
                                            xf += 9; // 1 of for + 9
                                        }

                                        if (breakOk)
                                        {
                                            breakOk = false;
                                            break;
                                        }
                                    }
                                    //Right
                                    for (int xf = bitbmp[frameCount / 8 * 1].Width - 1; xf > 0; xf--)
                                    {
                                        for (int fram = frameCount / 8 * 1; fram < frameCount / 8 * 2; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int yf = 0; yf < bitbmp[frameCount / 8 * 1].Height; yf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                if (pixel == WhiteConvert | pixel == GreyConvert | pixel == customConvert | pixel.A == 0)
                                                {
                                                    var = true;
                                                }

                                                if (pixel != WhiteConvert & pixel != GreyConvert & pixel != customConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (xf != bitbmp[frameCount / 8 * 1].Width - 1)
                                                    {
                                                        regressR++;
                                                        xf += 1;
                                                        yf = -1;
                                                        fram = frameCount / 8 * 1;
                                                    }
                                                    else
                                                    {
                                                        breakOk = true;
                                                        break;
                                                    }
                                                }
                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 8 * 2 - 1 && regressR == -1 && xf > 9)
                                                {
                                                    right += 10;
                                                }

                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 8 * 2 - 1 && regressR == -1 && xf <= 9)
                                                {
                                                    right++;
                                                }

                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 8 * 2 - 1 && regressR != -1)
                                                {
                                                    right -= regressR;
                                                    breakOk = true;
                                                    break;
                                                }
                                            }
                                            if (breakOk)
                                            {
                                                break;
                                            }
                                        }
                                        if (xf > 9)
                                        {
                                            xf -= 9; // 1 of for + 9
                                        }

                                        if (breakOk)
                                        {
                                            breakOk = false;
                                            break;
                                        }
                                    }
                                    for (int index = frameCount / 8 * 1; index < frameCount / 8 * 2; index++)
                                    {
                                        Rectangle rect = new Rectangle(left, top, bitbmp[index].Width - left - right, bitbmp[index].Height - top - bot);
                                        bitbmp[index].SelectActiveFrame(dimension, index);
                                        bitbmp[index] = bitbmp[index].Clone(rect, PixelFormat.Format16bppArgb1555);
                                    }

                                    //Reseta cordenadas
                                    top = 0;
                                    bot = 0;
                                    left = 0;
                                    right = 0;
                                    regressT = -1;
                                    regressB = -1;
                                    regressL = -1;
                                    regressR = -1;
                                    var = true;
                                    breakOk = false;
                                    // position 4
                                    //Top
                                    for (int yf = 0; yf < bitbmp[frameCount / 8 * 6].Height; yf++)
                                    {
                                        for (int fram = frameCount / 8 * 6; fram < frameCount / 8 * 7; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int xf = 0; xf < bitbmp[frameCount / 8 * 6].Width; xf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                if (pixel == WhiteConvert | pixel == GreyConvert | pixel == customConvert | pixel.A == 0)
                                                {
                                                    var = true;
                                                }

                                                if (pixel != WhiteConvert & pixel != GreyConvert & pixel != customConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (yf != 0)
                                                    {
                                                        regressT++;
                                                        yf -= 1;
                                                        xf = -1;
                                                        fram = frameCount / 8 * 6;
                                                    }
                                                    else
                                                    {
                                                        breakOk = true;
                                                        break;
                                                    }
                                                }
                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 8 * 7 - 1 && regressT == -1 && yf < bitbmp[0].Height - 9)
                                                {
                                                    top += 10;
                                                }

                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 8 * 7 - 1 && regressT == -1 && yf >= bitbmp[0].Height - 9)
                                                {
                                                    top++;
                                                }

                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 8 * 7 - 1 && regressT != -1)
                                                {
                                                    top -= regressT;
                                                    breakOk = true;
                                                    break;
                                                }
                                            }
                                            if (breakOk)
                                            {
                                                break;
                                            }
                                        }
                                        if (yf < bitbmp[frameCount / 8 * 6].Height - 9)
                                        {
                                            yf += 9; // 1 of for + 9
                                        }

                                        if (breakOk)
                                        {
                                            breakOk = false;
                                            break;
                                        }
                                    }
                                    //Bot
                                    for (int yf = bitbmp[frameCount / 8 * 6].Height - 1; yf > 0; yf--)
                                    {
                                        for (int fram = frameCount / 8 * 6; fram < frameCount / 8 * 7; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int xf = 0; xf < bitbmp[frameCount / 8 * 6].Width; xf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                if (pixel == WhiteConvert | pixel == GreyConvert | pixel == customConvert | pixel.A == 0)
                                                {
                                                    var = true;
                                                }

                                                if (pixel != WhiteConvert & pixel != GreyConvert & pixel != customConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (yf != bitbmp[frameCount / 8 * 6].Height - 1)
                                                    {
                                                        regressB++;
                                                        yf += 1;
                                                        xf = -1;
                                                        fram = frameCount / 8 * 6;
                                                    }
                                                    else
                                                    {
                                                        breakOk = true;
                                                        break;
                                                    }
                                                }
                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 8 * 7 - 1 && regressB == -1 && yf > 9)
                                                {
                                                    bot += 10;
                                                }

                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 8 * 7 - 1 && regressB == -1 && yf <= 9)
                                                {
                                                    bot++;
                                                }

                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 8 * 7 - 1 && regressB != -1)
                                                {
                                                    bot -= regressB;
                                                    breakOk = true;
                                                    break;
                                                }
                                            }
                                            if (breakOk)
                                            {
                                                break;
                                            }
                                        }
                                        if (yf > 9)
                                        {
                                            yf -= 9; // 1 of for + 9
                                        }

                                        if (breakOk)
                                        {
                                            breakOk = false;
                                            break;
                                        }
                                    }
                                    //Left

                                    for (int xf = 0; xf < bitbmp[frameCount / 8 * 6].Width; xf++)
                                    {
                                        for (int fram = frameCount / 8 * 6; fram < frameCount / 8 * 7; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int yf = 0; yf < bitbmp[frameCount / 8 * 6].Height; yf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                if (pixel == WhiteConvert | pixel == GreyConvert | pixel == customConvert | pixel.A == 0)
                                                {
                                                    var = true;
                                                }

                                                if (pixel != WhiteConvert & pixel != GreyConvert & pixel != customConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (xf != 0)
                                                    {
                                                        regressL++;
                                                        xf -= 1;
                                                        yf = -1;
                                                        fram = frameCount / 8 * 6;
                                                    }
                                                    else
                                                    {
                                                        breakOk = true;
                                                        break;
                                                    }
                                                }
                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 8 * 7 - 1 && regressL == -1 && xf < bitbmp[0].Width - 9)
                                                {
                                                    left += 10;
                                                }

                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 8 * 7 - 1 && regressL == -1 && xf >= bitbmp[0].Width - 9)
                                                {
                                                    left++;
                                                }

                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 8 * 7 - 1 && regressL != -1)
                                                {
                                                    left -= regressL;
                                                    breakOk = true;
                                                    break;
                                                }
                                            }
                                            if (breakOk)
                                            {
                                                break;
                                            }
                                        }
                                        if (xf < bitbmp[frameCount / 8 * 6].Width - 9)
                                        {
                                            xf += 9; // 1 of for + 9
                                        }

                                        if (breakOk)
                                        {
                                            breakOk = false;
                                            break;
                                        }
                                    }
                                    //Right
                                    for (int xf = bitbmp[frameCount / 8 * 6].Width - 1; xf > 0; xf--)
                                    {
                                        for (int fram = frameCount / 8 * 6; fram < frameCount / 8 * 7; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int yf = 0; yf < bitbmp[frameCount / 8 * 6].Height; yf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                if (pixel == WhiteConvert | pixel == GreyConvert | pixel == customConvert | pixel.A == 0)
                                                {
                                                    var = true;
                                                }

                                                if (pixel != WhiteConvert & pixel != GreyConvert & pixel != customConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (xf != bitbmp[frameCount / 8 * 6].Width - 1)
                                                    {
                                                        regressR++;
                                                        xf += 1;
                                                        yf = -1;
                                                        fram = frameCount / 8 * 6;
                                                    }
                                                    else
                                                    {
                                                        breakOk = true;
                                                        break;
                                                    }
                                                }
                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 8 * 7 - 1 && regressR == -1 && xf > 9)
                                                {
                                                    right += 10;
                                                }

                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 8 * 7 - 1 && regressR == -1 && xf <= 9)
                                                {
                                                    right++;
                                                }

                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 8 * 7 - 1 && regressR != -1)
                                                {
                                                    right -= regressR;
                                                    breakOk = true;
                                                    break;
                                                }
                                            }
                                            if (breakOk)
                                            {
                                                break;
                                            }
                                        }
                                        if (xf > 9)
                                        {
                                            xf -= 9; // 1 of for + 9
                                        }

                                        if (breakOk)
                                        {
                                            breakOk = false;
                                            break;
                                        }
                                    }
                                    for (int index = frameCount / 8 * 6; index < frameCount / 8 * 7; index++)
                                    {
                                        Rectangle rect = new Rectangle(left, top, bitbmp[index].Width - left - right, bitbmp[index].Height - top - bot);
                                        bitbmp[index].SelectActiveFrame(dimension, index);
                                        bitbmp[index] = bitbmp[index].Clone(rect, PixelFormat.Format16bppArgb1555);
                                    }

                                    //End of Canvas
                                    //posicao 0
                                    for (int index = frameCount / 8 * 4; index < frameCount / 8 * 5; index++)
                                    {
                                        bitbmp[index].SelectActiveFrame(dimension, index);
                                        bitbmp[index] = Utils.ConvertBmpAnimCv5(bitbmp[index], (int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                                        edit.AddFrame(bitbmp[index]);
                                        TreeNode node = GetNode(_currBody);
                                        if (node != null)
                                        {
                                            node.ForeColor = Color.Black;
                                            node.Nodes[_currAction].ForeColor = Color.Black;
                                        }
                                        ListViewItem item;
                                        int i = edit.Frames.Count - 1;
                                        item = new ListViewItem(i.ToString(), 0)
                                        {
                                            Tag = i
                                        };
                                        listView1.Items.Add(item);
                                        int width = listView1.TileSize.Width - 5;
                                        if (bmp.Width > listView1.TileSize.Width)
                                        {
                                            width = bmp.Width;
                                        }

                                        int height = listView1.TileSize.Height - 5;
                                        if (bmp.Height > listView1.TileSize.Height)
                                        {
                                            height = bmp.Height;
                                        }

                                        listView1.TileSize = new Size(width + 5, height + 5);
                                        trackBar2.Maximum = i;
                                        Options.ChangedUltimaClass["Animations"] = true;
                                        if (progressBar1.Value < progressBar1.Maximum)
                                        {
                                            progressBar1.Value++;
                                            progressBar1.Invalidate();
                                        }
                                        if (index == frameCount / 8 * 5 - 1)
                                        {
                                            trackBar1.Value++;
                                        }
                                    }
                                    //posicao 1
                                    edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currBody, _currAction, _currDir);
                                    bmp.SelectActiveFrame(dimension, 0);
                                    edit.GetGifPalette(bmp);
                                    for (int index = 0; index < frameCount / 8; index++)
                                    {
                                        bitbmp[index].SelectActiveFrame(dimension, index);
                                        bitbmp[index] = Utils.ConvertBmpAnimCv5(bitbmp[index], (int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                                        edit.AddFrame(bitbmp[index]);
                                        TreeNode node = GetNode(_currBody);
                                        if (node != null)
                                        {
                                            node.ForeColor = Color.Black;
                                            node.Nodes[_currAction].ForeColor = Color.Black;
                                        }
                                        ListViewItem item;
                                        int i = edit.Frames.Count - 1;
                                        item = new ListViewItem(i.ToString(), 0)
                                        {
                                            Tag = i
                                        };
                                        listView1.Items.Add(item);
                                        int width = listView1.TileSize.Width - 5;
                                        if (bmp.Width > listView1.TileSize.Width)
                                        {
                                            width = bmp.Width;
                                        }

                                        int height = listView1.TileSize.Height - 5;
                                        if (bmp.Height > listView1.TileSize.Height)
                                        {
                                            height = bmp.Height;
                                        }

                                        listView1.TileSize = new Size(width + 5, height + 5);
                                        trackBar2.Maximum = i;
                                        Options.ChangedUltimaClass["Animations"] = true;
                                        if (progressBar1.Value < progressBar1.Maximum)
                                        {
                                            progressBar1.Value++;
                                            progressBar1.Invalidate();
                                        }
                                        if (index == frameCount / 8 - 1)
                                        {
                                            trackBar1.Value++;
                                        }
                                    }
                                    //posicao 2
                                    edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currBody, _currAction, _currDir);
                                    bmp.SelectActiveFrame(dimension, 0);
                                    edit.GetGifPalette(bmp);
                                    for (int index = frameCount / 8 * 5; index < frameCount / 8 * 6; index++)
                                    {
                                        bitbmp[index].SelectActiveFrame(dimension, index);
                                        bitbmp[index] = Utils.ConvertBmpAnimCv5(bitbmp[index], (int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                                        edit.AddFrame(bitbmp[index]);
                                        TreeNode node = GetNode(_currBody);
                                        if (node != null)
                                        {
                                            node.ForeColor = Color.Black;
                                            node.Nodes[_currAction].ForeColor = Color.Black;
                                        }
                                        ListViewItem item;
                                        int i = edit.Frames.Count - 1;
                                        item = new ListViewItem(i.ToString(), 0)
                                        {
                                            Tag = i
                                        };
                                        listView1.Items.Add(item);
                                        int width = listView1.TileSize.Width - 5;
                                        if (bmp.Width > listView1.TileSize.Width)
                                        {
                                            width = bmp.Width;
                                        }

                                        int height = listView1.TileSize.Height - 5;
                                        if (bmp.Height > listView1.TileSize.Height)
                                        {
                                            height = bmp.Height;
                                        }

                                        listView1.TileSize = new Size(width + 5, height + 5);
                                        trackBar2.Maximum = i;
                                        Options.ChangedUltimaClass["Animations"] = true;
                                        if (progressBar1.Value < progressBar1.Maximum)
                                        {
                                            progressBar1.Value++;
                                            progressBar1.Invalidate();
                                        }
                                        if (index == frameCount / 8 * 6 - 1)
                                        {
                                            trackBar1.Value++;
                                        }
                                    }
                                    //posicao 3
                                    edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currBody, _currAction, _currDir);
                                    bmp.SelectActiveFrame(dimension, 0);
                                    edit.GetGifPalette(bmp);
                                    for (int index = frameCount / 8 * 1; index < frameCount / 8 * 2; index++)
                                    {
                                        bitbmp[index].SelectActiveFrame(dimension, index);
                                        bitbmp[index] = Utils.ConvertBmpAnimCv5(bitbmp[index], (int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                                        edit.AddFrame(bitbmp[index]);
                                        TreeNode node = GetNode(_currBody);
                                        if (node != null)
                                        {
                                            node.ForeColor = Color.Black;
                                            node.Nodes[_currAction].ForeColor = Color.Black;
                                        }
                                        ListViewItem item;
                                        int i = edit.Frames.Count - 1;
                                        item = new ListViewItem(i.ToString(), 0)
                                        {
                                            Tag = i
                                        };
                                        listView1.Items.Add(item);
                                        int width = listView1.TileSize.Width - 5;
                                        if (bmp.Width > listView1.TileSize.Width)
                                        {
                                            width = bmp.Width;
                                        }

                                        int height = listView1.TileSize.Height - 5;
                                        if (bmp.Height > listView1.TileSize.Height)
                                        {
                                            height = bmp.Height;
                                        }

                                        listView1.TileSize = new Size(width + 5, height + 5);
                                        trackBar2.Maximum = i;
                                        Options.ChangedUltimaClass["Animations"] = true;
                                        if (progressBar1.Value < progressBar1.Maximum)
                                        {
                                            progressBar1.Value++;
                                            progressBar1.Invalidate();
                                        }
                                        if (index == frameCount / 8 * 2 - 1)
                                        {
                                            trackBar1.Value++;
                                        }
                                    }
                                    //posicao 4
                                    edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currBody, _currAction, _currDir);
                                    bmp.SelectActiveFrame(dimension, 0);
                                    edit.GetGifPalette(bmp);
                                    for (int index = frameCount / 8 * 6; index < frameCount / 8 * 7; index++)
                                    {
                                        bitbmp[index].SelectActiveFrame(dimension, index);
                                        bitbmp[index] = Utils.ConvertBmpAnimCv5(bitbmp[index], (int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                                        edit.AddFrame(bitbmp[index]);
                                        TreeNode node = GetNode(_currBody);
                                        if (node != null)
                                        {
                                            node.ForeColor = Color.Black;
                                            node.Nodes[_currAction].ForeColor = Color.Black;
                                        }
                                        ListViewItem item;
                                        int i = edit.Frames.Count - 1;
                                        item = new ListViewItem(i.ToString(), 0)
                                        {
                                            Tag = i
                                        };
                                        listView1.Items.Add(item);
                                        int width = listView1.TileSize.Width - 5;
                                        if (bmp.Width > listView1.TileSize.Width)
                                        {
                                            width = bmp.Width;
                                        }

                                        int height = listView1.TileSize.Height - 5;
                                        if (bmp.Height > listView1.TileSize.Height)
                                        {
                                            height = bmp.Height;
                                        }

                                        listView1.TileSize = new Size(width + 5, height + 5);
                                        trackBar2.Maximum = i;
                                        Options.ChangedUltimaClass["Animations"] = true;
                                        if (progressBar1.Value < progressBar1.Maximum)
                                        {
                                            progressBar1.Value++;
                                            progressBar1.Invalidate();
                                        }
                                    }
                                    progressBar1.Value = 0;
                                    progressBar1.Invalidate();
                                    SetPaletteBox();
                                    listView1.Invalidate();
                                    numericUpDownCx.Value = edit.Frames[trackBar2.Value].Center.X;
                                    numericUpDownCy.Value = edit.Frames[trackBar2.Value].Center.Y;
                                    Options.ChangedUltimaClass["Animations"] = true;
                                }
                            }
                            trackBar1.Enabled = true;
                        }
                    }
                }
                //Refresh List after Canvas reduction
                _currDir = trackBar1.Value;
                AfterSelectTreeView(null, null);
            }
            catch (NullReferenceException) { trackBar1.Enabled = true; }
        }

        private void CheckBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkBox2.Checked)
            {
                numericUpDown3.Enabled = true;
                numericUpDown4.Enabled = true;
                numericUpDown5.Enabled = true;
            }
            else
            {
                numericUpDown3.Enabled = false;
                numericUpDown4.Enabled = false;
                numericUpDown5.Enabled = false;
                numericUpDown3.Value = 255;
                numericUpDown4.Value = 255;
                numericUpDown5.Value = 255;
            }
        }

        //All directions Add KRframeViewer 
        private void AllDirectionsAddWithCanvasKRframeEditorColorCorrectorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (_fileType != 0)
                {
                    using (OpenFileDialog dialog = new OpenFileDialog())
                    {
                        dialog.Multiselect = false;
                        dialog.Title = "Choose 1 Gif ( with all directions in KRframeViewer Style ) to add";
                        dialog.CheckFileExists = true;
                        dialog.Filter = "Gif files (*.gif;)|*.gif;";
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            Color customConvert = Color.FromArgb(255, (int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                            trackBar1.Enabled = false;
                            trackBar1.Value = 0;
                            Bitmap bmp = new Bitmap(dialog.FileName);
                            AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currBody, _currAction, _currDir);
                            if (edit != null)
                            {
                                //Gif Especial Properties
                                if (dialog.FileName.Contains(".gif"))
                                {
                                    FrameDimension dimension = new FrameDimension(bmp.FrameDimensionsList[0]);
                                    // Number of frames 
                                    int frameCount = bmp.GetFrameCount(dimension);
                                    progressBar1.Maximum = frameCount;
                                    bmp.SelectActiveFrame(dimension, 0);
                                    edit.GetGifPalette(bmp);
                                    Bitmap[] bitbmp = new Bitmap[frameCount];
                                    // Return an Image at a certain index 
                                    for (int index = 0; index < frameCount; index++)
                                    {
                                        bitbmp[index] = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format16bppArgb1555);
                                        bmp.SelectActiveFrame(dimension, index);
                                        bitbmp[index] = (Bitmap)bmp;
                                    }
                                    //Canvas algorithm
                                    int top = 0;
                                    int bot = 0;
                                    int left = 0;
                                    int right = 0;
                                    int regressT = -1;
                                    int regressB = -1;
                                    int regressL = -1;
                                    int regressR = -1;
                                    bool var = true;
                                    bool breakOk = false;
                                    // position 0
                                    //Top
                                    for (int yf = 0; yf < bitbmp[frameCount / 5 * 0].Height; yf++)
                                    {
                                        for (int fram = frameCount / 5 * 0; fram < frameCount / 5 * 1; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int xf = 0; xf < bitbmp[frameCount / 5 * 0].Width; xf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                if (pixel == WhiteConvert | pixel == customConvert | pixel.A == 0)
                                                {
                                                    var = true;
                                                }

                                                if (pixel != WhiteConvert & pixel != customConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (yf != 0)
                                                    {
                                                        regressT++;
                                                        yf -= 1;
                                                        xf = -1;
                                                        fram = frameCount / 5 * 0;
                                                    }
                                                    else
                                                    {
                                                        breakOk = true;
                                                        break;
                                                    }
                                                }
                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 5 * 1 - 1 && regressT == -1 && yf < bitbmp[0].Height - 9)
                                                {
                                                    top += 10;
                                                }

                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 5 * 1 - 1 && regressT == -1 && yf >= bitbmp[0].Height - 9)
                                                {
                                                    top++;
                                                }

                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 5 * 1 - 1 && regressT != -1)
                                                {
                                                    top -= regressT;
                                                    breakOk = true;
                                                    break;
                                                }
                                            }
                                            if (breakOk)
                                            {
                                                break;
                                            }
                                        }
                                        if (yf < bitbmp[frameCount / 5 * 0].Height - 9)
                                        {
                                            yf += 9; // 1 of for + 9
                                        }

                                        if (breakOk)
                                        {
                                            breakOk = false;
                                            break;
                                        }
                                    }
                                    //Bot
                                    for (int yf = bitbmp[frameCount / 5 * 0].Height - 1; yf > 0; yf--)
                                    {
                                        for (int fram = frameCount / 5 * 0; fram < frameCount / 5 * 1; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int xf = 0; xf < bitbmp[frameCount / 5 * 0].Width; xf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                if (pixel == WhiteConvert | pixel == customConvert | pixel.A == 0)
                                                {
                                                    var = true;
                                                }

                                                if (pixel != WhiteConvert & pixel != customConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (yf != bitbmp[frameCount / 5 * 0].Height - 1)
                                                    {
                                                        regressB++;
                                                        yf += 1;
                                                        xf = -1;
                                                        fram = frameCount / 5 * 0;
                                                    }
                                                    else
                                                    {
                                                        breakOk = true;
                                                        break;
                                                    }
                                                }
                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 5 * 1 - 1 && regressB == -1 && yf > 9)
                                                {
                                                    bot += 10;
                                                }

                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 5 * 1 - 1 && regressB == -1 && yf <= 9)
                                                {
                                                    bot++;
                                                }

                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 5 * 1 - 1 && regressB != -1)
                                                {
                                                    bot -= regressB;
                                                    breakOk = true;
                                                    break;
                                                }
                                            }
                                            if (breakOk)
                                            {
                                                break;
                                            }
                                        }
                                        if (yf > 9)
                                        {
                                            yf -= 9; // 1 of for + 9
                                        }

                                        if (breakOk)
                                        {
                                            breakOk = false;
                                            break;
                                        }
                                    }
                                    //Left

                                    for (int xf = 0; xf < bitbmp[frameCount / 5 * 0].Width; xf++)
                                    {
                                        for (int fram = frameCount / 5 * 0; fram < frameCount / 5 * 1; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int yf = 0; yf < bitbmp[frameCount / 5 * 0].Height; yf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                if (pixel == WhiteConvert | pixel == customConvert | pixel.A == 0)
                                                {
                                                    var = true;
                                                }

                                                if (pixel != WhiteConvert & pixel != customConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (xf != 0)
                                                    {
                                                        regressL++;
                                                        xf -= 1;
                                                        yf = -1;
                                                        fram = frameCount / 5 * 0;
                                                    }
                                                    else
                                                    {
                                                        breakOk = true;
                                                        break;
                                                    }
                                                }
                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 5 * 1 - 1 && regressL == -1 && xf < bitbmp[0].Width - 9)
                                                {
                                                    left += 10;
                                                }

                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 5 * 1 - 1 && regressL == -1 && xf >= bitbmp[0].Width - 9)
                                                {
                                                    left++;
                                                }

                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 5 * 1 - 1 && regressL != -1)
                                                {
                                                    left -= regressL;
                                                    breakOk = true;
                                                    break;
                                                }
                                            }
                                            if (breakOk)
                                            {
                                                break;
                                            }
                                        }
                                        if (xf < bitbmp[frameCount / 5 * 0].Width - 9)
                                        {
                                            xf += 9; // 1 of for + 9
                                        }

                                        if (breakOk)
                                        {
                                            breakOk = false;
                                            break;
                                        }
                                    }
                                    //Right
                                    for (int xf = bitbmp[frameCount / 5 * 0].Width - 1; xf > 0; xf--)
                                    {
                                        for (int fram = frameCount / 5 * 0; fram < frameCount / 5 * 1; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int yf = 0; yf < bitbmp[frameCount / 5 * 0].Height; yf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                if (pixel == WhiteConvert | pixel == customConvert | pixel.A == 0)
                                                {
                                                    var = true;
                                                }

                                                if (pixel != WhiteConvert & pixel != customConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (xf != bitbmp[frameCount / 5 * 0].Width - 1)
                                                    {
                                                        regressR++;
                                                        xf += 1;
                                                        yf = -1;
                                                        fram = frameCount / 5 * 0;
                                                    }
                                                    else
                                                    {
                                                        breakOk = true;
                                                        break;
                                                    }
                                                }
                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 5 * 1 - 1 && regressR == -1 && xf > 9)
                                                {
                                                    right += 10;
                                                }

                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 5 * 1 - 1 && regressR == -1 && xf <= 9)
                                                {
                                                    right++;
                                                }

                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 5 * 1 - 1 && regressR != -1)
                                                {
                                                    right -= regressR;
                                                    breakOk = true;
                                                    break;
                                                }
                                            }
                                            if (breakOk)
                                            {
                                                break;
                                            }
                                        }
                                        if (xf > 9)
                                        {
                                            xf -= 9; // 1 of for + 9
                                        }

                                        if (breakOk)
                                        {
                                            breakOk = false;
                                            break;
                                        }
                                    }
                                    for (int index = frameCount / 5 * 0; index < frameCount / 5 * 1; index++)
                                    {
                                        Rectangle rect = new Rectangle(left, top, bitbmp[index].Width - left - right, bitbmp[index].Height - top - bot);
                                        bitbmp[index].SelectActiveFrame(dimension, index);
                                        bitbmp[index] = bitbmp[index].Clone(rect, PixelFormat.Format16bppArgb1555);
                                    }

                                    //Reseta cordenadas
                                    top = 0;
                                    bot = 0;
                                    left = 0;
                                    right = 0;
                                    regressT = -1;
                                    regressB = -1;
                                    regressL = -1;
                                    regressR = -1;
                                    var = true;
                                    breakOk = false;
                                    // position 1
                                    //Top
                                    for (int yf = 0; yf < bitbmp[frameCount / 5 * 1].Height; yf++)
                                    {
                                        for (int fram = frameCount / 5 * 1; fram < frameCount / 5 * 2; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int xf = 0; xf < bitbmp[frameCount / 5 * 1].Width; xf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                if (pixel == WhiteConvert | pixel == customConvert | pixel.A == 0)
                                                {
                                                    var = true;
                                                }

                                                if (pixel != WhiteConvert & pixel != customConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (yf != 0)
                                                    {
                                                        regressT++;
                                                        yf -= 1;
                                                        xf = -1;
                                                        fram = frameCount / 5 * 1;
                                                    }
                                                    else
                                                    {
                                                        breakOk = true;
                                                        break;
                                                    }
                                                }
                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 5 * 2 - 1 && regressT == -1 && yf < bitbmp[0].Height - 9)
                                                {
                                                    top += 10;
                                                }

                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 5 * 2 - 1 && regressT == -1 && yf >= bitbmp[0].Height - 9)
                                                {
                                                    top++;
                                                }

                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 5 * 2 - 1 && regressT != -1)
                                                {
                                                    top -= regressT;
                                                    breakOk = true;
                                                    break;
                                                }
                                            }
                                            if (breakOk)
                                            {
                                                break;
                                            }
                                        }
                                        if (yf < bitbmp[frameCount / 5 * 1].Height - 9)
                                        {
                                            yf += 9; // 1 of for + 9
                                        }

                                        if (breakOk)
                                        {
                                            breakOk = false;
                                            break;
                                        }
                                    }
                                    //Bot
                                    for (int yf = bitbmp[frameCount / 5 * 1].Height - 1; yf > 0; yf--)
                                    {
                                        for (int fram = frameCount / 5 * 1; fram < frameCount / 5 * 2; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int xf = 0; xf < bitbmp[frameCount / 5 * 1].Width; xf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                if (pixel == WhiteConvert | pixel == customConvert | pixel.A == 0)
                                                {
                                                    var = true;
                                                }

                                                if (pixel != WhiteConvert & pixel != customConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (yf != bitbmp[frameCount / 5 * 1].Height - 1)
                                                    {
                                                        regressB++;
                                                        yf += 1;
                                                        xf = -1;
                                                        fram = frameCount / 5 * 1;
                                                    }
                                                    else
                                                    {
                                                        breakOk = true;
                                                        break;
                                                    }
                                                }
                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 5 * 2 - 1 && regressB == -1 && yf > 9)
                                                {
                                                    bot += 10;
                                                }

                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 5 * 2 - 1 && regressB == -1 && yf <= 9)
                                                {
                                                    bot++;
                                                }

                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 5 * 2 - 1 && regressB != -1)
                                                {
                                                    bot -= regressB;
                                                    breakOk = true;
                                                    break;
                                                }
                                            }
                                            if (breakOk)
                                            {
                                                break;
                                            }
                                        }
                                        if (yf > 9)
                                        {
                                            yf -= 9; // 1 of for + 9
                                        }

                                        if (breakOk)
                                        {
                                            breakOk = false;
                                            break;
                                        }
                                    }
                                    //Left

                                    for (int xf = 0; xf < bitbmp[frameCount / 5 * 1].Width; xf++)
                                    {
                                        for (int fram = frameCount / 5 * 1; fram < frameCount / 5 * 2; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int yf = 0; yf < bitbmp[frameCount / 5 * 1].Height; yf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                if (pixel == WhiteConvert | pixel == customConvert | pixel.A == 0)
                                                {
                                                    var = true;
                                                }

                                                if (pixel != WhiteConvert & pixel != customConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (xf != 0)
                                                    {
                                                        regressL++;
                                                        xf -= 1;
                                                        yf = -1;
                                                        fram = frameCount / 5 * 1;
                                                    }
                                                    else
                                                    {
                                                        breakOk = true;
                                                        break;
                                                    }
                                                }
                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 5 * 2 - 1 && regressL == -1 && xf < bitbmp[0].Width - 9)
                                                {
                                                    left += 10;
                                                }

                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 5 * 2 - 1 && regressL == -1 && xf >= bitbmp[0].Width - 9)
                                                {
                                                    left++;
                                                }

                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 5 * 2 - 1 && regressL != -1)
                                                {
                                                    left -= regressL;
                                                    breakOk = true;
                                                    break;
                                                }
                                            }
                                            if (breakOk)
                                            {
                                                break;
                                            }
                                        }
                                        if (xf < bitbmp[frameCount / 5 * 1].Width - 9)
                                        {
                                            xf += 9; // 1 of for + 9
                                        }

                                        if (breakOk)
                                        {
                                            breakOk = false;
                                            break;
                                        }
                                    }
                                    //Right
                                    for (int xf = bitbmp[frameCount / 5 * 1].Width - 1; xf > 0; xf--)
                                    {
                                        for (int fram = frameCount / 5 * 1; fram < frameCount / 5 * 2; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int yf = 0; yf < bitbmp[frameCount / 5 * 1].Height; yf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                if (pixel == WhiteConvert | pixel == customConvert | pixel.A == 0)
                                                {
                                                    var = true;
                                                }

                                                if (pixel != WhiteConvert & pixel != customConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (xf != bitbmp[frameCount / 5 * 1].Width - 1)
                                                    {
                                                        regressR++;
                                                        xf += 1;
                                                        yf = -1;
                                                        fram = frameCount / 5 * 1;
                                                    }
                                                    else
                                                    {
                                                        breakOk = true;
                                                        break;
                                                    }
                                                }
                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 5 * 2 - 1 && regressR == -1 && xf > 9)
                                                {
                                                    right += 10;
                                                }

                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 5 * 2 - 1 && regressR == -1 && xf <= 9)
                                                {
                                                    right++;
                                                }

                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 5 * 2 - 1 && regressR != -1)
                                                {
                                                    right -= regressR;
                                                    breakOk = true;
                                                    break;
                                                }
                                            }
                                            if (breakOk)
                                            {
                                                break;
                                            }
                                        }
                                        if (xf > 9)
                                        {
                                            xf -= 9; // 1 of for + 9
                                        }

                                        if (breakOk)
                                        {
                                            breakOk = false;
                                            break;
                                        }
                                    }
                                    for (int index = frameCount / 5 * 1; index < frameCount / 5 * 2; index++)
                                    {
                                        Rectangle rect = new Rectangle(left, top, bitbmp[index].Width - left - right, bitbmp[index].Height - top - bot);
                                        bitbmp[index].SelectActiveFrame(dimension, index);
                                        bitbmp[index] = bitbmp[index].Clone(rect, PixelFormat.Format16bppArgb1555);
                                    }

                                    //Reseta cordenadas
                                    top = 0;
                                    bot = 0;
                                    left = 0;
                                    right = 0;
                                    regressT = -1;
                                    regressB = -1;
                                    regressL = -1;
                                    regressR = -1;
                                    var = true;
                                    breakOk = false;
                                    // position 2
                                    //Top
                                    for (int yf = 0; yf < bitbmp[frameCount / 5 * 2].Height; yf++)
                                    {
                                        for (int fram = frameCount / 5 * 2; fram < frameCount / 5 * 3; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int xf = 0; xf < bitbmp[frameCount / 5 * 2].Width; xf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                if (pixel == WhiteConvert | pixel == customConvert | pixel.A == 0)
                                                {
                                                    var = true;
                                                }

                                                if (pixel != WhiteConvert & pixel != customConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (yf != 0)
                                                    {
                                                        regressT++;
                                                        yf -= 1;
                                                        xf = -1;
                                                        fram = frameCount / 5 * 2;
                                                    }
                                                    else
                                                    {
                                                        breakOk = true;
                                                        break;
                                                    }
                                                }
                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 5 * 3 - 1 && regressT == -1 && yf < bitbmp[0].Height - 9)
                                                {
                                                    top += 10;
                                                }

                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 5 * 3 - 1 && regressT == -1 && yf >= bitbmp[0].Height - 9)
                                                {
                                                    top++;
                                                }

                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 5 * 3 - 1 && regressT != -1)
                                                {
                                                    top -= regressT;
                                                    breakOk = true;
                                                    break;
                                                }
                                            }
                                            if (breakOk)
                                            {
                                                break;
                                            }
                                        }
                                        if (yf < bitbmp[frameCount / 5 * 2].Height - 9)
                                        {
                                            yf += 9; // 1 of for + 9
                                        }

                                        if (breakOk)
                                        {
                                            breakOk = false;
                                            break;
                                        }
                                    }
                                    //Bot
                                    for (int yf = bitbmp[frameCount / 5 * 2].Height - 1; yf > 0; yf--)
                                    {
                                        for (int fram = frameCount / 5 * 2; fram < frameCount / 5 * 3; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int xf = 0; xf < bitbmp[frameCount / 5 * 2].Width; xf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                if (pixel == WhiteConvert | pixel == customConvert | pixel.A == 0)
                                                {
                                                    var = true;
                                                }

                                                if (pixel != WhiteConvert & pixel != customConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (yf != bitbmp[frameCount / 5 * 2].Height - 1)
                                                    {
                                                        regressB++;
                                                        yf += 1;
                                                        xf = -1;
                                                        fram = frameCount / 5 * 2;
                                                    }
                                                    else
                                                    {
                                                        breakOk = true;
                                                        break;
                                                    }
                                                }
                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 5 * 3 - 1 && regressB == -1 && yf > 9)
                                                {
                                                    bot += 10;
                                                }

                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 5 * 3 - 1 && regressB == -1 && yf <= 9)
                                                {
                                                    bot++;
                                                }

                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 5 * 3 - 1 && regressB != -1)
                                                {
                                                    bot -= regressB;
                                                    breakOk = true;
                                                    break;
                                                }
                                            }
                                            if (breakOk)
                                            {
                                                break;
                                            }
                                        }
                                        if (yf > 9)
                                        {
                                            yf -= 9; // 1 of for + 9
                                        }

                                        if (breakOk)
                                        {
                                            breakOk = false;
                                            break;
                                        }
                                    }
                                    //Left

                                    for (int xf = 0; xf < bitbmp[frameCount / 5 * 2].Width; xf++)
                                    {
                                        for (int fram = frameCount / 5 * 2; fram < frameCount / 5 * 3; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int yf = 0; yf < bitbmp[frameCount / 5 * 2].Height; yf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                if (pixel == WhiteConvert | pixel == customConvert | pixel.A == 0)
                                                {
                                                    var = true;
                                                }

                                                if (pixel != WhiteConvert & pixel != customConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (xf != 0)
                                                    {
                                                        regressL++;
                                                        xf -= 1;
                                                        yf = -1;
                                                        fram = frameCount / 5 * 2;
                                                    }
                                                    else
                                                    {
                                                        breakOk = true;
                                                        break;
                                                    }
                                                }
                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 5 * 3 - 1 && regressL == -1 && xf < bitbmp[0].Width - 9)
                                                {
                                                    left += 10;
                                                }

                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 5 * 3 - 1 && regressL == -1 && xf >= bitbmp[0].Width - 9)
                                                {
                                                    left++;
                                                }

                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 5 * 3 - 1 && regressL != -1)
                                                {
                                                    left -= regressL;
                                                    breakOk = true;
                                                    break;
                                                }
                                            }
                                            if (breakOk)
                                            {
                                                break;
                                            }
                                        }
                                        if (xf < bitbmp[frameCount / 5 * 2].Width - 9)
                                        {
                                            xf += 9; // 1 of for + 9
                                        }

                                        if (breakOk)
                                        {
                                            breakOk = false;
                                            break;
                                        }
                                    }
                                    //Right
                                    for (int xf = bitbmp[frameCount / 5 * 2].Width - 1; xf > 0; xf--)
                                    {
                                        for (int fram = frameCount / 5 * 2; fram < frameCount / 5 * 3; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int yf = 0; yf < bitbmp[frameCount / 5 * 2].Height; yf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                if (pixel == WhiteConvert | pixel == customConvert | pixel.A == 0)
                                                {
                                                    var = true;
                                                }

                                                if (pixel != WhiteConvert & pixel != customConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (xf != bitbmp[frameCount / 5 * 2].Width - 1)
                                                    {
                                                        regressR++;
                                                        xf += 1;
                                                        yf = -1;
                                                        fram = frameCount / 5 * 2;
                                                    }
                                                    else
                                                    {
                                                        breakOk = true;
                                                        break;
                                                    }
                                                }
                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 5 * 3 - 1 && regressR == -1 && xf > 9)
                                                {
                                                    right += 10;
                                                }

                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 5 * 3 - 1 && regressR == -1 && xf <= 9)
                                                {
                                                    right++;
                                                }

                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 5 * 3 - 1 && regressR != -1)
                                                {
                                                    right -= regressR;
                                                    breakOk = true;
                                                    break;
                                                }
                                            }
                                            if (breakOk)
                                            {
                                                break;
                                            }
                                        }
                                        if (xf > 9)
                                        {
                                            xf -= 9; // 1 of for + 9
                                        }

                                        if (breakOk)
                                        {
                                            breakOk = false;
                                            break;
                                        }
                                    }
                                    for (int index = frameCount / 5 * 2; index < frameCount / 5 * 3; index++)
                                    {
                                        Rectangle rect = new Rectangle(left, top, bitbmp[index].Width - left - right, bitbmp[index].Height - top - bot);
                                        bitbmp[index].SelectActiveFrame(dimension, index);
                                        bitbmp[index] = bitbmp[index].Clone(rect, PixelFormat.Format16bppArgb1555);
                                    }

                                    //Reseta cordenadas
                                    top = 0;
                                    bot = 0;
                                    left = 0;
                                    right = 0;
                                    regressT = -1;
                                    regressB = -1;
                                    regressL = -1;
                                    regressR = -1;
                                    var = true;
                                    breakOk = false;
                                    // position 3
                                    //Top
                                    for (int yf = 0; yf < bitbmp[frameCount / 5 * 3].Height; yf++)
                                    {
                                        for (int fram = frameCount / 5 * 3; fram < frameCount / 5 * 4; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int xf = 0; xf < bitbmp[frameCount / 5 * 3].Width; xf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                if (pixel == WhiteConvert | pixel == customConvert | pixel.A == 0)
                                                {
                                                    var = true;
                                                }

                                                if (pixel != WhiteConvert & pixel != customConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (yf != 0)
                                                    {
                                                        regressT++;
                                                        yf -= 1;
                                                        xf = -1;
                                                        fram = frameCount / 5 * 3;
                                                    }
                                                    else
                                                    {
                                                        breakOk = true;
                                                        break;
                                                    }
                                                }
                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 5 * 4 - 1 && regressT == -1 && yf < bitbmp[0].Height - 9)
                                                {
                                                    top += 10;
                                                }

                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 5 * 4 - 1 && regressT == -1 && yf >= bitbmp[0].Height - 9)
                                                {
                                                    top++;
                                                }

                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 5 * 4 - 1 && regressT != -1)
                                                {
                                                    top -= regressT;
                                                    breakOk = true;
                                                    break;
                                                }
                                            }
                                            if (breakOk)
                                            {
                                                break;
                                            }
                                        }
                                        if (yf < bitbmp[frameCount / 5 * 3].Height - 9)
                                        {
                                            yf += 9; // 1 of for + 9
                                        }

                                        if (breakOk)
                                        {
                                            breakOk = false;
                                            break;
                                        }
                                    }
                                    //Bot
                                    for (int yf = bitbmp[frameCount / 5 * 3].Height - 1; yf > 0; yf--)
                                    {
                                        for (int fram = frameCount / 5 * 3; fram < frameCount / 5 * 4; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int xf = 0; xf < bitbmp[frameCount / 5 * 3].Width; xf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                if (pixel == WhiteConvert | pixel == customConvert | pixel.A == 0)
                                                {
                                                    var = true;
                                                }

                                                if (pixel != WhiteConvert & pixel != customConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (yf != bitbmp[frameCount / 5 * 3].Height - 1)
                                                    {
                                                        regressB++;
                                                        yf += 1;
                                                        xf = -1;
                                                        fram = frameCount / 5 * 3;
                                                    }
                                                    else
                                                    {
                                                        breakOk = true;
                                                        break;
                                                    }
                                                }
                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 5 * 4 - 1 && regressB == -1 && yf > 9)
                                                {
                                                    bot += 10;
                                                }

                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 5 * 4 - 1 && regressB == -1 && yf <= 9)
                                                {
                                                    bot++;
                                                }

                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 5 * 4 - 1 && regressB != -1)
                                                {
                                                    bot -= regressB;
                                                    breakOk = true;
                                                    break;
                                                }
                                            }
                                            if (breakOk)
                                            {
                                                break;
                                            }
                                        }
                                        if (yf > 9)
                                        {
                                            yf -= 9; // 1 of for + 9
                                        }

                                        if (breakOk)
                                        {
                                            breakOk = false;
                                            break;
                                        }
                                    }
                                    //Left

                                    for (int xf = 0; xf < bitbmp[frameCount / 5 * 3].Width; xf++)
                                    {
                                        for (int fram = frameCount / 5 * 3; fram < frameCount / 5 * 4; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int yf = 0; yf < bitbmp[frameCount / 5 * 3].Height; yf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                if (pixel == WhiteConvert | pixel == customConvert | pixel.A == 0)
                                                {
                                                    var = true;
                                                }

                                                if (pixel != WhiteConvert & pixel != customConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (xf != 0)
                                                    {
                                                        regressL++;
                                                        xf -= 1;
                                                        yf = -1;
                                                        fram = frameCount / 5 * 3;
                                                    }
                                                    else
                                                    {
                                                        breakOk = true;
                                                        break;
                                                    }
                                                }
                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 5 * 4 - 1 && regressL == -1 && xf < bitbmp[0].Width - 9)
                                                {
                                                    left += 10;
                                                }

                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 5 * 4 - 1 && regressL == -1 && xf >= bitbmp[0].Width - 9)
                                                {
                                                    left++;
                                                }

                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 5 * 4 - 1 && regressL != -1)
                                                {
                                                    left -= regressL;
                                                    breakOk = true;
                                                    break;
                                                }
                                            }
                                            if (breakOk)
                                            {
                                                break;
                                            }
                                        }
                                        if (xf < bitbmp[frameCount / 5 * 3].Width - 9)
                                        {
                                            xf += 9; // 1 of for + 9
                                        }

                                        if (breakOk)
                                        {
                                            breakOk = false;
                                            break;
                                        }
                                    }
                                    //Right
                                    for (int xf = bitbmp[frameCount / 5 * 3].Width - 1; xf > 0; xf--)
                                    {
                                        for (int fram = frameCount / 5 * 3; fram < frameCount / 5 * 4; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int yf = 0; yf < bitbmp[frameCount / 5 * 3].Height; yf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                if (pixel == WhiteConvert | pixel == customConvert | pixel.A == 0)
                                                {
                                                    var = true;
                                                }

                                                if (pixel != WhiteConvert & pixel != customConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (xf != bitbmp[frameCount / 5 * 3].Width - 1)
                                                    {
                                                        regressR++;
                                                        xf += 1;
                                                        yf = -1;
                                                        fram = frameCount / 5 * 3;
                                                    }
                                                    else
                                                    {
                                                        breakOk = true;
                                                        break;
                                                    }
                                                }
                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 5 * 4 - 1 && regressR == -1 && xf > 9)
                                                {
                                                    right += 10;
                                                }

                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 5 * 4 - 1 && regressR == -1 && xf <= 9)
                                                {
                                                    right++;
                                                }

                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 5 * 4 - 1 && regressR != -1)
                                                {
                                                    right -= regressR;
                                                    breakOk = true;
                                                    break;
                                                }
                                            }
                                            if (breakOk)
                                            {
                                                break;
                                            }
                                        }
                                        if (xf > 9)
                                        {
                                            xf -= 9; // 1 of for + 9
                                        }

                                        if (breakOk)
                                        {
                                            breakOk = false;
                                            break;
                                        }
                                    }
                                    for (int index = frameCount / 5 * 3; index < frameCount / 5 * 4; index++)
                                    {
                                        Rectangle rect = new Rectangle(left, top, bitbmp[index].Width - left - right, bitbmp[index].Height - top - bot);
                                        bitbmp[index].SelectActiveFrame(dimension, index);
                                        bitbmp[index] = bitbmp[index].Clone(rect, PixelFormat.Format16bppArgb1555);
                                    }

                                    //Reseta cordenadas
                                    top = 0;
                                    bot = 0;
                                    left = 0;
                                    right = 0;
                                    regressT = -1;
                                    regressB = -1;
                                    regressL = -1;
                                    regressR = -1;
                                    var = true;
                                    breakOk = false;
                                    // position 4
                                    //Top
                                    for (int yf = 0; yf < bitbmp[frameCount / 5 * 4].Height; yf++)
                                    {
                                        for (int fram = frameCount / 5 * 4; fram < frameCount / 5 * 5; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int xf = 0; xf < bitbmp[frameCount / 5 * 4].Width; xf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                if (pixel == WhiteConvert | pixel == customConvert | pixel.A == 0)
                                                {
                                                    var = true;
                                                }

                                                if (pixel != WhiteConvert & pixel != customConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (yf != 0)
                                                    {
                                                        regressT++;
                                                        yf -= 1;
                                                        xf = -1;
                                                        fram = frameCount / 5 * 4;
                                                    }
                                                    else
                                                    {
                                                        breakOk = true;
                                                        break;
                                                    }
                                                }
                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 5 * 5 - 1 && regressT == -1 && yf < bitbmp[0].Height - 9)
                                                {
                                                    top += 10;
                                                }

                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 5 * 5 - 1 && regressT == -1 && yf >= bitbmp[0].Height - 9)
                                                {
                                                    top++;
                                                }

                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 5 * 5 - 1 && regressT != -1)
                                                {
                                                    top -= regressT;
                                                    breakOk = true;
                                                    break;
                                                }
                                            }
                                            if (breakOk)
                                            {
                                                break;
                                            }
                                        }
                                        if (yf < bitbmp[frameCount / 5 * 4].Height - 9)
                                        {
                                            yf += 9; // 1 of for + 9
                                        }

                                        if (breakOk)
                                        {
                                            breakOk = false;
                                            break;
                                        }
                                    }
                                    //Bot
                                    for (int yf = bitbmp[frameCount / 5 * 4].Height - 1; yf > 0; yf--)
                                    {
                                        for (int fram = frameCount / 5 * 4; fram < frameCount / 5 * 5; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int xf = 0; xf < bitbmp[frameCount / 5 * 4].Width; xf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                if (pixel == WhiteConvert | pixel == customConvert | pixel.A == 0)
                                                {
                                                    var = true;
                                                }

                                                if (pixel != WhiteConvert & pixel != customConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (yf != bitbmp[frameCount / 5 * 4].Height - 1)
                                                    {
                                                        regressB++;
                                                        yf += 1;
                                                        xf = -1;
                                                        fram = frameCount / 5 * 4;
                                                    }
                                                    else
                                                    {
                                                        breakOk = true;
                                                        break;
                                                    }
                                                }
                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 5 * 5 - 1 && regressB == -1 && yf > 9)
                                                {
                                                    bot += 10;
                                                }

                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 5 * 5 - 1 && regressB == -1 && yf <= 9)
                                                {
                                                    bot++;
                                                }

                                                if (var && xf == bitbmp[fram].Width - 1 && fram == frameCount / 5 * 5 - 1 && regressB != -1)
                                                {
                                                    bot -= regressB;
                                                    breakOk = true;
                                                    break;
                                                }
                                            }
                                            if (breakOk)
                                            {
                                                break;
                                            }
                                        }
                                        if (yf > 9)
                                        {
                                            yf -= 9; // 1 of for + 9
                                        }

                                        if (breakOk)
                                        {
                                            breakOk = false;
                                            break;
                                        }
                                    }
                                    //Left

                                    for (int xf = 0; xf < bitbmp[frameCount / 5 * 4].Width; xf++)
                                    {
                                        for (int fram = frameCount / 5 * 4; fram < frameCount / 5 * 5; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int yf = 0; yf < bitbmp[frameCount / 5 * 4].Height; yf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                if (pixel == WhiteConvert | pixel == customConvert | pixel.A == 0)
                                                {
                                                    var = true;
                                                }

                                                if (pixel != WhiteConvert & pixel != customConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (xf != 0)
                                                    {
                                                        regressL++;
                                                        xf -= 1;
                                                        yf = -1;
                                                        fram = frameCount / 5 * 4;
                                                    }
                                                    else
                                                    {
                                                        breakOk = true;
                                                        break;
                                                    }
                                                }
                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 5 * 5 - 1 && regressL == -1 && xf < bitbmp[0].Width - 9)
                                                {
                                                    left += 10;
                                                }

                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 5 * 5 - 1 && regressL == -1 && xf >= bitbmp[0].Width - 9)
                                                {
                                                    left++;
                                                }

                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 5 * 5 - 1 && regressL != -1)
                                                {
                                                    left -= regressL;
                                                    breakOk = true;
                                                    break;
                                                }
                                            }
                                            if (breakOk)
                                            {
                                                break;
                                            }
                                        }
                                        if (xf < bitbmp[frameCount / 5 * 4].Width - 9)
                                        {
                                            xf += 9; // 1 of for + 9
                                        }

                                        if (breakOk)
                                        {
                                            breakOk = false;
                                            break;
                                        }
                                    }
                                    //Right
                                    for (int xf = bitbmp[frameCount / 5 * 4].Width - 1; xf > 0; xf--)
                                    {
                                        for (int fram = frameCount / 5 * 4; fram < frameCount / 5 * 5; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int yf = 0; yf < bitbmp[frameCount / 5 * 4].Height; yf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(xf, yf);
                                                if (pixel == WhiteConvert | pixel == customConvert | pixel.A == 0)
                                                {
                                                    var = true;
                                                }

                                                if (pixel != WhiteConvert & pixel != customConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (xf != bitbmp[frameCount / 5 * 4].Width - 1)
                                                    {
                                                        regressR++;
                                                        xf += 1;
                                                        yf = -1;
                                                        fram = frameCount / 5 * 4;
                                                    }
                                                    else
                                                    {
                                                        breakOk = true;
                                                        break;
                                                    }
                                                }
                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 5 * 5 - 1 && regressR == -1 && xf > 9)
                                                {
                                                    right += 10;
                                                }

                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 5 * 5 - 1 && regressR == -1 && xf <= 9)
                                                {
                                                    right++;
                                                }

                                                if (var && yf == bitbmp[fram].Height - 1 && fram == frameCount / 5 * 5 - 1 && regressR != -1)
                                                {
                                                    right -= regressR;
                                                    breakOk = true;
                                                    break;
                                                }
                                            }
                                            if (breakOk)
                                            {
                                                break;
                                            }
                                        }
                                        if (xf > 9)
                                        {
                                            xf -= 9; // 1 of for + 9
                                        }

                                        if (breakOk)
                                        {
                                            breakOk = false;
                                            break;
                                        }
                                    }
                                    for (int index = frameCount / 5 * 4; index < frameCount / 5 * 5; index++)
                                    {
                                        Rectangle rect = new Rectangle(left, top, bitbmp[index].Width - left - right, bitbmp[index].Height - top - bot);
                                        bitbmp[index].SelectActiveFrame(dimension, index);
                                        bitbmp[index] = bitbmp[index].Clone(rect, PixelFormat.Format16bppArgb1555);
                                    }

                                    //End of Canvas
                                    //posicao 0
                                    for (int index = frameCount / 5 * 0; index < frameCount / 5 * 1; index++)
                                    {
                                        bitbmp[index].SelectActiveFrame(dimension, index);
                                        bitbmp[index] = Utils.ConvertBmpAnimKr(bitbmp[index], (int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                                        edit.AddFrame(bitbmp[index]);
                                        TreeNode node = GetNode(_currBody);
                                        if (node != null)
                                        {
                                            node.ForeColor = Color.Black;
                                            node.Nodes[_currAction].ForeColor = Color.Black;
                                        }
                                        ListViewItem item;
                                        int i = edit.Frames.Count - 1;
                                        item = new ListViewItem(i.ToString(), 0)
                                        {
                                            Tag = i
                                        };
                                        listView1.Items.Add(item);
                                        int width = listView1.TileSize.Width - 5;
                                        if (bmp.Width > listView1.TileSize.Width)
                                        {
                                            width = bmp.Width;
                                        }

                                        int height = listView1.TileSize.Height - 5;
                                        if (bmp.Height > listView1.TileSize.Height)
                                        {
                                            height = bmp.Height;
                                        }

                                        listView1.TileSize = new Size(width + 5, height + 5);
                                        trackBar2.Maximum = i;
                                        Options.ChangedUltimaClass["Animations"] = true;
                                        if (progressBar1.Value < progressBar1.Maximum)
                                        {
                                            progressBar1.Value++;
                                            progressBar1.Invalidate();
                                        }
                                        if (index == frameCount / 5 * 1 - 1)
                                        {
                                            trackBar1.Value++;
                                        }
                                    }
                                    //posicao 1
                                    edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currBody, _currAction, _currDir);
                                    bmp.SelectActiveFrame(dimension, 0);
                                    edit.GetGifPalette(bmp);
                                    for (int index = frameCount / 5 * 1; index < frameCount / 5 * 2; index++)
                                    {
                                        bitbmp[index].SelectActiveFrame(dimension, index);
                                        bitbmp[index] = Utils.ConvertBmpAnimKr(bitbmp[index], (int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                                        edit.AddFrame(bitbmp[index]);
                                        TreeNode node = GetNode(_currBody);
                                        if (node != null)
                                        {
                                            node.ForeColor = Color.Black;
                                            node.Nodes[_currAction].ForeColor = Color.Black;
                                        }
                                        ListViewItem item;
                                        int i = edit.Frames.Count - 1;
                                        item = new ListViewItem(i.ToString(), 0)
                                        {
                                            Tag = i
                                        };
                                        listView1.Items.Add(item);
                                        int width = listView1.TileSize.Width - 5;
                                        if (bmp.Width > listView1.TileSize.Width)
                                        {
                                            width = bmp.Width;
                                        }

                                        int height = listView1.TileSize.Height - 5;
                                        if (bmp.Height > listView1.TileSize.Height)
                                        {
                                            height = bmp.Height;
                                        }

                                        listView1.TileSize = new Size(width + 5, height + 5);
                                        trackBar2.Maximum = i;
                                        Options.ChangedUltimaClass["Animations"] = true;
                                        if (progressBar1.Value < progressBar1.Maximum)
                                        {
                                            progressBar1.Value++;
                                            progressBar1.Invalidate();
                                        }
                                        if (index == frameCount / 5 * 2 - 1)
                                        {
                                            trackBar1.Value++;
                                        }
                                    }
                                    //posicao 2
                                    edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currBody, _currAction, _currDir);
                                    bmp.SelectActiveFrame(dimension, 0);
                                    edit.GetGifPalette(bmp);
                                    for (int index = frameCount / 5 * 2; index < frameCount / 5 * 3; index++)
                                    {
                                        bitbmp[index].SelectActiveFrame(dimension, index);
                                        bitbmp[index] = Utils.ConvertBmpAnimKr(bitbmp[index], (int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                                        edit.AddFrame(bitbmp[index]);
                                        TreeNode node = GetNode(_currBody);
                                        if (node != null)
                                        {
                                            node.ForeColor = Color.Black;
                                            node.Nodes[_currAction].ForeColor = Color.Black;
                                        }
                                        ListViewItem item;
                                        int i = edit.Frames.Count - 1;
                                        item = new ListViewItem(i.ToString(), 0)
                                        {
                                            Tag = i
                                        };
                                        listView1.Items.Add(item);
                                        int width = listView1.TileSize.Width - 5;
                                        if (bmp.Width > listView1.TileSize.Width)
                                        {
                                            width = bmp.Width;
                                        }

                                        int height = listView1.TileSize.Height - 5;
                                        if (bmp.Height > listView1.TileSize.Height)
                                        {
                                            height = bmp.Height;
                                        }

                                        listView1.TileSize = new Size(width + 5, height + 5);
                                        trackBar2.Maximum = i;
                                        Options.ChangedUltimaClass["Animations"] = true;
                                        if (progressBar1.Value < progressBar1.Maximum)
                                        {
                                            progressBar1.Value++;
                                            progressBar1.Invalidate();
                                        }
                                        if (index == frameCount / 5 * 3 - 1)
                                        {
                                            trackBar1.Value++;
                                        }
                                    }
                                    //posicao 3
                                    edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currBody, _currAction, _currDir);
                                    bmp.SelectActiveFrame(dimension, 0);
                                    edit.GetGifPalette(bmp);
                                    for (int index = frameCount / 5 * 3; index < frameCount / 5 * 4; index++)
                                    {
                                        bitbmp[index].SelectActiveFrame(dimension, index);
                                        bitbmp[index] = Utils.ConvertBmpAnimKr(bitbmp[index], (int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                                        edit.AddFrame(bitbmp[index]);
                                        TreeNode node = GetNode(_currBody);
                                        if (node != null)
                                        {
                                            node.ForeColor = Color.Black;
                                            node.Nodes[_currAction].ForeColor = Color.Black;
                                        }
                                        ListViewItem item;
                                        int i = edit.Frames.Count - 1;
                                        item = new ListViewItem(i.ToString(), 0)
                                        {
                                            Tag = i
                                        };
                                        listView1.Items.Add(item);
                                        int width = listView1.TileSize.Width - 5;
                                        if (bmp.Width > listView1.TileSize.Width)
                                        {
                                            width = bmp.Width;
                                        }

                                        int height = listView1.TileSize.Height - 5;
                                        if (bmp.Height > listView1.TileSize.Height)
                                        {
                                            height = bmp.Height;
                                        }

                                        listView1.TileSize = new Size(width + 5, height + 5);
                                        trackBar2.Maximum = i;
                                        Options.ChangedUltimaClass["Animations"] = true;
                                        if (progressBar1.Value < progressBar1.Maximum)
                                        {
                                            progressBar1.Value++;
                                            progressBar1.Invalidate();
                                        }
                                        if (index == frameCount / 5 * 4 - 1)
                                        {
                                            trackBar1.Value++;
                                        }
                                    }
                                    //posicao 4
                                    edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currBody, _currAction, _currDir);
                                    bmp.SelectActiveFrame(dimension, 0);
                                    edit.GetGifPalette(bmp);
                                    for (int index = frameCount / 5 * 4; index < frameCount / 5 * 5; index++)
                                    {
                                        bitbmp[index].SelectActiveFrame(dimension, index);
                                        bitbmp[index] = Utils.ConvertBmpAnimKr(bitbmp[index], (int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                                        edit.AddFrame(bitbmp[index]);
                                        TreeNode node = GetNode(_currBody);
                                        if (node != null)
                                        {
                                            node.ForeColor = Color.Black;
                                            node.Nodes[_currAction].ForeColor = Color.Black;
                                        }
                                        ListViewItem item;
                                        int i = edit.Frames.Count - 1;
                                        item = new ListViewItem(i.ToString(), 0)
                                        {
                                            Tag = i
                                        };
                                        listView1.Items.Add(item);
                                        int width = listView1.TileSize.Width - 5;
                                        if (bmp.Width > listView1.TileSize.Width)
                                        {
                                            width = bmp.Width;
                                        }

                                        int height = listView1.TileSize.Height - 5;
                                        if (bmp.Height > listView1.TileSize.Height)
                                        {
                                            height = bmp.Height;
                                        }

                                        listView1.TileSize = new Size(width + 5, height + 5);
                                        trackBar2.Maximum = i;
                                        Options.ChangedUltimaClass["Animations"] = true;
                                        if (progressBar1.Value < progressBar1.Maximum)
                                        {
                                            progressBar1.Value++;
                                            progressBar1.Invalidate();
                                        }
                                    }
                                    progressBar1.Value = 0;
                                    progressBar1.Invalidate();
                                    SetPaletteBox();
                                    listView1.Invalidate();
                                    numericUpDownCx.Value = edit.Frames[trackBar2.Value].Center.X;
                                    numericUpDownCy.Value = edit.Frames[trackBar2.Value].Center.Y;
                                    Options.ChangedUltimaClass["Animations"] = true;
                                }
                            }
                            trackBar1.Enabled = true;
                        }
                    }
                }
                //Refresh List after Canvas reduction
                _currDir = trackBar1.Value;
                AfterSelectTreeView(null, null);
            }
            catch (NullReferenceException) { trackBar1.Enabled = true; }
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            for (int x = 0; x < 5; x++)
            {
                //RGB
                if (radioButton1.Checked)
                {
                    AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currBody, _currAction, _currDir);
                    edit?.PaletteConversor(1);
                }
                //RBG
                if (radioButton2.Checked)
                {
                    AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currBody, _currAction, _currDir);
                    edit?.PaletteConversor(2);
                }
                //GRB
                if (radioButton3.Checked)
                {
                    AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currBody, _currAction, _currDir);
                    edit?.PaletteConversor(3);
                }
                //GBR
                if (radioButton4.Checked)
                {
                    AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currBody, _currAction, _currDir);
                    edit?.PaletteConversor(4);
                }
                //BGR
                if (radioButton5.Checked)
                {
                    AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currBody, _currAction, _currDir);
                    edit?.PaletteConversor(5);
                }
                //BRG
                if (radioButton6.Checked)
                {
                    AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currBody, _currAction, _currDir);
                    edit?.PaletteConversor(6);
                }
                SetPaletteBox();
                listView1.Invalidate();
                Options.ChangedUltimaClass["Animations"] = true;
                SetPaletteBox();
                if (trackBar1.Value != trackBar1.Maximum)
                {
                    trackBar1.Value++;
                }
                else
                {
                    trackBar1.Value = 0;
                }
            }
        }

        private void RadioButton2_CheckedChanged(object sender, EventArgs e)
        {
            for (int x = 0; x < 5; x++)
            {
                AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currBody, _currAction, _currDir);
                edit?.PaletteConversor(2);
                SetPaletteBox();
                listView1.Invalidate();
                Options.ChangedUltimaClass["Animations"] = true;
                SetPaletteBox();
                if (trackBar1.Value != trackBar1.Maximum)
                {
                    trackBar1.Value++;
                }
                else
                {
                    trackBar1.Value = 0;
                }
            }
        }

        private void RadioButton3_CheckedChanged(object sender, EventArgs e)
        {
            for (int x = 0; x < 5; x++)
            {
                AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currBody, _currAction, _currDir);
                edit?.PaletteConversor(3);
                SetPaletteBox();
                listView1.Invalidate();
                Options.ChangedUltimaClass["Animations"] = true;
                SetPaletteBox();
                if (trackBar1.Value != trackBar1.Maximum)
                {
                    trackBar1.Value++;
                }
                else
                {
                    trackBar1.Value = 0;
                }
            }
        }

        private void RadioButton4_CheckedChanged(object sender, EventArgs e)
        {
            if (!radioButton4.Checked)
            {
                for (int x = 0; x < 5; x++)
                {
                    AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currBody, _currAction, _currDir);
                    if (edit != null)
                    {
                        edit.PaletteConversor(4);
                        edit.PaletteConversor(4);
                    }
                    SetPaletteBox();
                    listView1.Invalidate();
                    Options.ChangedUltimaClass["Animations"] = true;
                    SetPaletteBox();
                    if (trackBar1.Value != trackBar1.Maximum)
                    {
                        trackBar1.Value++;
                    }
                    else
                    {
                        trackBar1.Value = 0;
                    }
                }
            }
            else
            {
                for (int x = 0; x < 5; x++)
                {
                    AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currBody, _currAction, _currDir);
                    edit?.PaletteConversor(4);
                    SetPaletteBox();
                    listView1.Invalidate();
                    Options.ChangedUltimaClass["Animations"] = true;
                    SetPaletteBox();
                    if (trackBar1.Value != trackBar1.Maximum)
                    {
                        trackBar1.Value++;
                    }
                    else
                    {
                        trackBar1.Value = 0;
                    }
                }
            }
        }

        private void RadioButton5_CheckedChanged(object sender, EventArgs e)
        {
            for (int x = 0; x < 5; x++)
            {
                AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currBody, _currAction, _currDir);
                edit?.PaletteConversor(5);
                SetPaletteBox();
                listView1.Invalidate();
                Options.ChangedUltimaClass["Animations"] = true;
                SetPaletteBox();
                if (trackBar1.Value != trackBar1.Maximum)
                {
                    trackBar1.Value++;
                }
                else
                {
                    trackBar1.Value = 0;
                }
            }
        }

        private void RadioButton6_CheckedChanged(object sender, EventArgs e)
        {
            if (!radioButton6.Checked)
            {
                for (int x = 0; x < 5; x++)
                {
                    AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currBody, _currAction, _currDir);
                    if (edit != null)
                    {
                        edit.PaletteConversor(6);
                        edit.PaletteConversor(6);
                    }
                    SetPaletteBox();
                    listView1.Invalidate();
                    Options.ChangedUltimaClass["Animations"] = true;
                    SetPaletteBox();
                    if (trackBar1.Value != trackBar1.Maximum)
                    {
                        trackBar1.Value++;
                    }
                    else
                    {
                        trackBar1.Value = 0;
                    }
                }
            }
            else
            {
                for (int x = 0; x < 5; x++)
                {
                    AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currBody, _currAction, _currDir);
                    edit?.PaletteConversor(6);
                    SetPaletteBox();
                    listView1.Invalidate();
                    Options.ChangedUltimaClass["Animations"] = true;
                    SetPaletteBox();
                    if (trackBar1.Value != trackBar1.Maximum)
                    {
                        trackBar1.Value++;
                    }
                    else
                    {
                        trackBar1.Value = 0;
                    }
                }
            }
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            for (int x = 0; x < 5; x++)
            {
                AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currBody, _currAction, _currDir);
                edit?.PaletteReductor((int)numericUpDown6.Value, (int)numericUpDown7.Value, (int)numericUpDown8.Value);
                SetPaletteBox();
                listView1.Invalidate();
                Options.ChangedUltimaClass["Animations"] = true;
                SetPaletteBox();
                if (trackBar1.Value != trackBar1.Maximum)
                {
                    trackBar1.Value++;
                }
                else
                {
                    trackBar1.Value = 0;
                }
            }
        }
    }
}
