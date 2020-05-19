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

namespace UoFiddler.Controls.Forms
{
    public partial class AnimationEdit : Form
    {
        private static readonly int[] _animCx = new int[5];
        private static readonly int[] _animCy = new int[5];
        private bool _loaded;
        private int _fileType;
        private int _currentAction;
        private int _currentBody;
        private int _currentDir;
        private Point _framePoint;
        private bool _showOnlyValid;
        private static bool _drawEmpty;
        private static bool _drawFull;
        private static Pen _blackUnDraw = new Pen(Color.FromArgb(255, 0, 0, 0), 1);
        private static SolidBrush _whiteUnDraw = new SolidBrush(Color.FromArgb(255, 255, 255, 255));
        private static readonly SolidBrush _whiteTransparent = new SolidBrush(Color.FromArgb(160, 255, 255, 255));
        private static readonly Color _whiteConvert = Color.FromArgb(255, 255, 255, 255);

        public AnimationEdit()
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();

            toolStripComboBox1.SelectedIndex = 0;
            listView1.MultiSelect = true;

            _fileType = 0;
            _currentDir = 0;
            _framePoint = new Point(pictureBox1.Width / 2, pictureBox1.Height / 2);
            _showOnlyValid = false;
            _loaded = false;
        }

        private void OnLoad(object sender, EventArgs e)
        {
            Options.LoadedUltimaClass["AnimationEdit"] = true;

            treeView1.BeginUpdate();
            try
            {
                treeView1.Nodes.Clear();
                if (_fileType != 0)
                {
                    int count = Animations.GetAnimCount(_fileType);
                    List<TreeNode> nodes = new List<TreeNode>();
                    for (int i = 0; i < count; ++i)
                    {
                        int animLength = Animations.GetAnimLength(i, _fileType);
                        string type = animLength == 22 ? "H" : animLength == 13 ? "L" : "P";
                        TreeNode node = new TreeNode
                        {
                            Tag = i,
                            Text = $"{type}: {i} ({BodyConverter.GetTrueBody(_fileType, i)})"
                        };

                        bool valid = false;
                        for (int j = 0; j < animLength; ++j)
                        {
                            TreeNode treeNode = new TreeNode
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
                                treeNode.ForeColor = Color.Red;
                            }

                            node.Nodes.Add(treeNode);
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
            }
            finally
            {
                treeView1.EndUpdate();
            }

            if (treeView1.Nodes.Count > 0)
            {
                treeView1.SelectedNode = treeView1.Nodes[0];
            }

            if (!_loaded)
            {
                ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
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
            _currentDir = 0;
            _currentAction = 0;
            _currentBody = 0;
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
            if (_fileType == 0)
            {
                return;
            }

            AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
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

        private void AfterSelectTreeView(object sender, TreeViewEventArgs e)
        {
            if (treeView1.SelectedNode == null)
            {
                return;
            }

            if (treeView1.SelectedNode.Parent == null)
            {
                if (treeView1.SelectedNode.Tag != null)
                {
                    _currentBody = (int)treeView1.SelectedNode.Tag;
                }

                _currentAction = 0;
            }
            else
            {
                if (treeView1.SelectedNode.Parent.Tag != null)
                {
                    _currentBody = (int)treeView1.SelectedNode.Parent.Tag;
                }

                _currentAction = (int)treeView1.SelectedNode.Tag;
            }

            listView1.BeginUpdate();
            try
            {
                listView1.Clear();
                AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                if (edit != null)
                {
                    int width = 80;
                    int height = 110;
                    Bitmap[] currentBits = edit.GetFrames();
                    if (currentBits != null)
                    {
                        for (int i = 0; i < currentBits.Length; ++i)
                        {
                            if (currentBits[i] == null)
                            {
                                continue;
                            }

                            ListViewItem item = new ListViewItem(i.ToString(), 0)
                            {
                                Tag = i
                            };
                            listView1.Items.Add(item);
                            if (currentBits[i].Width > width)
                            {
                                width = currentBits[i].Width;
                            }

                            if (currentBits[i].Height > height)
                            {
                                height = currentBits[i].Height;
                            }
                        }
                        listView1.TileSize = new Size(width + 5, height + 5);
                        trackBar2.Maximum = currentBits.Length - 1;
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
            }
            finally
            {
                listView1.EndUpdate();
            }

            pictureBox1.Invalidate();
            SetPaletteBox();
        }

        private void DrawFrameItem(object sender, DrawListViewItemEventArgs e)
        {
            AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
            Bitmap[] currentBits = edit.GetFrames();
            Bitmap bmp = currentBits[(int)e.Item.Tag];
            var penColor = listView1.SelectedItems.Contains(e.Item) ? Color.Red : Color.Gray;
            e.Graphics.DrawRectangle(new Pen(penColor), e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
            e.Graphics.DrawImage(bmp, e.Bounds.X, e.Bounds.Y, bmp.Width,  bmp.Height);
            e.DrawText(TextFormatFlags.Bottom | TextFormatFlags.HorizontalCenter);
        }

        private void OnAnimChanged(object sender, EventArgs e)
        {
            if (toolStripComboBox1.SelectedIndex == _fileType)
            {
                return;
            }

            _fileType = toolStripComboBox1.SelectedIndex;
            OnLoad(this, EventArgs.Empty);
        }

        private void OnDirectionChanged(object sender, EventArgs e)
        {
            _currentDir = trackBar1.Value;
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
            AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
            if (edit == null)
            {
                return;
            }

            Bitmap[] currentBits = edit.GetFrames();

            e.Graphics.Clear(Color.LightGray);
            e.Graphics.DrawLine(Pens.Black, new Point(_framePoint.X, 0), new Point(_framePoint.X, pictureBox1.Height));
            e.Graphics.DrawLine(Pens.Black, new Point(0, _framePoint.Y), new Point(pictureBox1.Width, _framePoint.Y));

            if (currentBits?.Length > 0 && currentBits[trackBar2.Value] != null)
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
                    varW = currentBits[trackBar2.Value].Width;
                    varH = currentBits[trackBar2.Value].Height;
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
                    varFw = currentBits[trackBar2.Value].Width;
                    varFh = currentBits[trackBar2.Value].Height;
                }

                int x = _framePoint.X - edit.Frames[trackBar2.Value].Center.X;
                int y = _framePoint.Y - edit.Frames[trackBar2.Value].Center.Y - currentBits[trackBar2.Value].Height;

                e.Graphics.FillRectangle(_whiteTransparent, new Rectangle(x, y, varFw, varFh));
                e.Graphics.DrawRectangle(Pens.Red, new Rectangle(x, y, varW, varH));
                e.Graphics.DrawImage(currentBits[trackBar2.Value], x, y);
                //e.Graphics.DrawLine(Pens.Red, new Point(0, 335-(int)numericUpDown1.Value), new Point(pictureBox1.Width, 335-(int)numericUpDown1.Value));
            }

            // Draw Reference Point Arrow
            Point[] arrayPoints = {
                new Point(418 - (int)numericUpDown2.Value, 335 - (int)numericUpDown1.Value),
                new Point(418 - (int)numericUpDown2.Value, 352 - (int)numericUpDown1.Value),
                new Point(422 - (int)numericUpDown2.Value, 348 - (int)numericUpDown1.Value),
                new Point(425 - (int)numericUpDown2.Value, 353 - (int)numericUpDown1.Value),
                new Point(427 - (int)numericUpDown2.Value, 352 - (int)numericUpDown1.Value),
                new Point(425 - (int)numericUpDown2.Value, 347 - (int)numericUpDown1.Value),
                new Point(430 - (int)numericUpDown2.Value, 347 - (int)numericUpDown1.Value)
            };

            e.Graphics.FillPolygon(_whiteUnDraw, arrayPoints);
            e.Graphics.DrawPolygon(_blackUnDraw, arrayPoints);
        }
        //End of Soulblighter Modification

        //Soulblighter Modification
        private void OnFrameCountBarChanged(object sender, EventArgs e)
        {
            if (_fileType == 0)
            {
                return;
            }

            AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
            if (edit != null && edit.Frames.Count >= trackBar2.Value)
            {
                numericUpDownCx.Value = edit.Frames[trackBar2.Value].Center.X;
                numericUpDownCy.Value = edit.Frames[trackBar2.Value].Center.Y;
            }

            pictureBox1.Invalidate();
        }
        //End of Soulblighter Modification

        private void OnCenterXValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (_fileType == 0)
                {
                    return;
                }

                AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                if (edit == null || edit.Frames.Count < trackBar2.Value)
                {
                    return;
                }

                FrameEdit frame = edit.Frames[trackBar2.Value];
                if (numericUpDownCx.Value == frame.Center.X)
                {
                    return;
                }

                frame.ChangeCenter((int)numericUpDownCx.Value, frame.Center.Y);
                Options.ChangedUltimaClass["Animations"] = true;
                pictureBox1.Invalidate();
            }
            catch (NullReferenceException)
            {
                // TODO: add logging or fix?
                // ignored
            }
        }

        private void OnCenterYValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (_fileType == 0)
                {
                    return;
                }

                AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                if (edit == null || edit.Frames.Count < trackBar2.Value)
                {
                    return;
                }

                FrameEdit frame = edit.Frames[trackBar2.Value];
                if (numericUpDownCy.Value == frame.Center.Y)
                {
                    return;
                }

                frame.ChangeCenter(frame.Center.X, (int)numericUpDownCy.Value);
                Options.ChangedUltimaClass["Animations"] = true;
                pictureBox1.Invalidate();
            }
            catch (NullReferenceException)
            {
                // TODO: add logging or fix?
                // ignored
            }
        }

        private void OnClickExtractImages(object sender, EventArgs e)
        {
            if (_fileType == 0)
            {
                return;
            }

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
                        Bitmap[] bits = edit?.GetFrames();
                        if (bits == null)
                        {
                            continue;
                        }

                        for (int j = 0; j < bits.Length; ++j)
                        {
                            string filename = string.Format("anim{5}_{0}_{1}_{2}_{3}{4}", body, a, i, j, menu.Tag, _fileType);
                            string file = Path.Combine(path, filename);
                            using (Bitmap bit = new Bitmap(bits[j]))
                            {
                                bit.Save(file, format);
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
                    Bitmap[] bits = edit?.GetFrames();
                    if (bits == null)
                    {
                        continue;
                    }

                    for (int j = 0; j < bits.Length; ++j)
                    {
                        string filename = string.Format("anim{5}_{0}_{1}_{2}_{3}{4}", body, action, i, j, menu.Tag, _fileType);
                        string file = Path.Combine(path, filename);
                        using (Bitmap bit = new Bitmap(bits[j]))
                        {
                            bit.Save(file, format);
                        }
                    }
                }
            }

            MessageBox.Show($"Frames saved to {path}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void OnClickRemoveAction(object sender, EventArgs e)
        {
            if (_fileType == 0)
            {
                return;
            }

            if (treeView1.SelectedNode.Parent == null)
            {
                DialogResult result = MessageBox.Show($"Are you sure to remove animation {_currentBody}", "Remove",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    return;
                }

                treeView1.SelectedNode.ForeColor = Color.Red;
                for (int i = 0; i < treeView1.SelectedNode.Nodes.Count; ++i)
                {
                    treeView1.SelectedNode.Nodes[i].ForeColor = Color.Red;
                    for (int d = 0; d < 5; ++d)
                    {
                        AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currentBody, i, d);
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
            else
            {
                DialogResult result = MessageBox.Show($"Are you sure to remove action {_currentAction}", "Remove",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    return;
                }

                for (int i = 0; i < 5; ++i)
                {
                    AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, i);
                    edit?.ClearFrames();
                }

                treeView1.SelectedNode.Parent.Nodes[_currentAction].ForeColor = Color.Red;
                bool valid = false;
                foreach (TreeNode node in treeView1.SelectedNode.Parent.Nodes)
                {
                    if (node.ForeColor == Color.Red)
                    {
                        continue;
                    }

                    valid = true;
                    break;
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

        private void OnClickSave(object sender, EventArgs e)
        {
            if (_fileType == 0)
            {
                return;
            }

            Ultima.AnimationEdit.Save(_fileType, Options.OutputPath);
            Options.ChangedUltimaClass["Animations"] = false;

            MessageBox.Show($"AnimationFile saved to {Options.OutputPath}", "Saved", MessageBoxButtons.OK,
                MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        //My Soulblighter Modification
        private void OnClickRemoveFrame(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count <= 0)
            {
                return;
            }

            int corrector = 0;
            int[] frameIndex = new int[listView1.SelectedItems.Count];
            for (int i = 0; i < listView1.SelectedItems.Count; i++)
            {
                frameIndex[i] = listView1.SelectedIndices[i] - corrector;
                corrector++;
            }

            for (int i = 0; i < frameIndex.Length; i++)
            {
                AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                if (edit == null)
                {
                    continue;
                }

                edit.RemoveFrame(frameIndex[i]);
                listView1.Items.RemoveAt(listView1.Items.Count - 1);
                trackBar2.Maximum = edit.Frames.Count != 0 ? edit.Frames.Count - 1 : 0;
                listView1.Invalidate();
                Options.ChangedUltimaClass["Animations"] = true;
            }
        }
        //End of Soulblighter Modification

        private void OnClickReplace(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count <= 0)
            {
                return;
            }

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                int frameIndex = (int)listView1.SelectedItems[0].Tag;
                dialog.Multiselect = false;
                dialog.Title = $"Choose image file to replace at {frameIndex}";
                dialog.CheckFileExists = true;
                dialog.Filter = "Image files (*.tif;*.tiff;*.bmp)|*.tif;*.tiff;*.bmp";
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                Bitmap bmp = new Bitmap(dialog.FileName);
                if (dialog.FileName.Contains(".bmp"))
                {
                    bmp = ConvertBmpAnim(bmp, (int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                }

                AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                if (edit == null)
                {
                    return;
                }

                edit.ReplaceFrame(bmp, frameIndex);
                listView1.Invalidate();
                Options.ChangedUltimaClass["Animations"] = true;
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
                        try
                        {
                            //My Soulblighter Modifications
                            for (int w = 0; w < dialog.FileNames.Length; w++)
                            {
                                Bitmap bmp = new Bitmap(dialog.FileNames[w]); // dialog.Filename replaced by dialog.FileNames[w]
                                AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                                if (dialog.FileName.Contains(".bmp") || dialog.FileName.Contains(".tiff") || dialog.FileName.Contains(".png") || dialog.FileName.Contains(".jpeg") || dialog.FileName.Contains(".jpg"))
                                {
                                    bmp = ConvertBmpAnim(bmp, (int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                                    //edit.GetImagePalette(bmp);
                                }

                                if (edit == null)
                                {
                                    continue;
                                }

                                //Gif Especial Properties
                                if (dialog.FileName.Contains(".gif"))
                                {
                                    FrameDimension dimension = new FrameDimension(bmp.FrameDimensionsList[0]);
                                    // Number of frames 
                                    int frameCount = bmp.GetFrameCount(dimension);
                                    Bitmap[] bitBmp = new Bitmap[frameCount];
                                    bmp.SelectActiveFrame(dimension, 0);
                                    edit.GetGifPalette(bmp);
                                    progressBar1.Maximum = frameCount;
                                    // Return an Image at a certain index 
                                    for (int index = 0; index < frameCount; index++)
                                    {
                                        bitBmp[index] = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format16bppArgb1555);
                                        bmp.SelectActiveFrame(dimension, index);
                                        bitBmp[index] = bmp;
                                        bitBmp[index] = ConvertBmpAnim(bitBmp[index], (int)numericUpDown3.Value,
                                            (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                                        edit.AddFrame(bitBmp[index]);
                                        TreeNode node = GetNode(_currentBody);
                                        if (node != null)
                                        {
                                            node.ForeColor = Color.Black;
                                            node.Nodes[_currentAction].ForeColor = Color.Black;
                                        }

                                        int i = edit.Frames.Count - 1;
                                        var item = new ListViewItem(i.ToString(), 0)
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
                                    TreeNode node = GetNode(_currentBody);
                                    if (node != null)
                                    {
                                        node.ForeColor = Color.Black;
                                        node.Nodes[_currentAction].ForeColor = Color.Black;
                                    }

                                    int i = edit.Frames.Count - 1;
                                    var item = new ListViewItem(i.ToString(), 0)
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
                        finally
                        {
                            listView1.EndUpdate();
                        }

                        listView1.Invalidate();
                    }
                }
            }

            // Refresh List
            _currentDir = trackBar1.Value;
            AfterSelectTreeView(null, null);
        }

        private void OnClickExtractPalette(object sender, EventArgs e)
        {
            if (_fileType == 0)
            {
                return;
            }

            ToolStripMenuItem menu = (ToolStripMenuItem)sender;
            AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
            if (edit == null)
            {
                return;
            }

            string name = $"palette_anim{_fileType}_{_currentBody}_{_currentAction}_{_currentDir}";
            if ((string)menu.Tag == "txt")
            {
                string path = Path.Combine(Options.OutputPath, name + ".txt");
                edit.ExportPalette(path, 0);
            }
            else
            {
                string path = Path.Combine(Options.OutputPath, name + "." + (string)menu.Tag);
                edit.ExportPalette(path, (string)menu.Tag == "bmp" ? 1 : 2);
            }

            MessageBox.Show($"Palette saved to {Options.OutputPath}", "Saved", MessageBoxButtons.OK,
                MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void OnClickImportPalette(object sender, EventArgs e)
        {
            if (_fileType == 0)
            {
                return;
            }

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = false;
                dialog.Title = "Choose palette file";
                dialog.CheckFileExists = true;
                dialog.Filter = "txt files (*.txt)|*.txt";
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                if (edit == null)
                {
                    return;
                }

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

        private void OnClickImportFromVD(object sender, EventArgs e)
        {
            if (_fileType == 0)
            {
                return;
            }

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = false;
                dialog.Title = "Choose palette file";
                dialog.CheckFileExists = true;
                dialog.Filter = "vd files (*.vd)|*.vd";
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                int animLength = Animations.GetAnimLength(_currentBody, _fileType);
                int currentType;
                if (animLength == 22)
                {
                    currentType = 0;
                }
                else
                {
                    currentType = animLength == 13 ? 1 : 2;
                }

                using (FileStream fs = new FileStream(dialog.FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (BinaryReader bin = new BinaryReader(fs))
                    {
                        int fileType = bin.ReadInt16();
                        int animType = bin.ReadInt16();
                        if (fileType != 6)
                        {
                            MessageBox.Show("Not an Anim File.", "Import", MessageBoxButtons.OK,
                                MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                            return;
                        }

                        if (animType != currentType)
                        {
                            MessageBox.Show("Wrong Anim Id ( Type )", "Import", MessageBoxButtons.OK,
                                MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                            return;
                        }
                        Ultima.AnimationEdit.LoadFromVD(_fileType, _currentBody, bin);
                    }
                }

                bool valid = false;
                TreeNode node = GetNode(_currentBody);
                if (node != null)
                {
                    for (int j = 0; j < animLength; ++j)
                    {
                        if (Ultima.AnimationEdit.IsActionDefinied(_fileType, _currentBody, j))
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

                MessageBox.Show("Finished", "Import", MessageBoxButtons.OK, MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1);
            }
        }

        private void OnClickExportToVD(object sender, EventArgs e)
        {
            if (_fileType == 0)
            {
                return;
            }

            string path = Options.OutputPath;
            string fileName = Path.Combine(path, $"anim{_fileType}_0x{_currentBody:X}.vd");
            Ultima.AnimationEdit.ExportToVD(_fileType, _currentBody, fileName);

            MessageBox.Show($"Animation saved to {Options.OutputPath}", "Export", MessageBoxButtons.OK,
                MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void OnClickShowOnlyValid(object sender, EventArgs e)
        {
            _showOnlyValid = !_showOnlyValid;

            if (_showOnlyValid)
            {
                treeView1.BeginUpdate();
                try
                {
                    for (int i = treeView1.Nodes.Count - 1; i >= 0; --i)
                    {
                        if (treeView1.Nodes[i].ForeColor == Color.Red)
                        {
                            treeView1.Nodes[i].Remove();
                        }
                    }
                }
                finally
                {
                    treeView1.EndUpdate();
                }
            }
            else
            {
                OnLoad(null);
            }
        }

        //My Soulblighter Modification
        private void Button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (_fileType == 0)
                {
                    return;
                }

                AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                if (edit == null || edit.Frames.Count < trackBar2.Value)
                {
                    return;
                }

                FrameEdit[] frame = new FrameEdit[edit.Frames.Count];
                for (int index = 0; index < edit.Frames.Count; index++)
                {
                    frame[index] = edit.Frames[index];
                    frame[index].ChangeCenter((int)numericUpDownCx.Value, (int)numericUpDownCy.Value);
                    Options.ChangedUltimaClass["Animations"] = true;
                    pictureBox1.Invalidate();
                }
            }
            catch (NullReferenceException)
            {
                // TODO: add logging or fix?
                // ignored
            }
        }
        //End of Soulblighter Modification

        //My Soulblighter Modification
        private void FromGifToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_fileType == 0)
            {
                return;
            }

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = false;
                dialog.Title = "Choose palette file";
                dialog.CheckFileExists = true;
                dialog.Filter = "Gif files (*.gif)|*.gif";
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                Bitmap bit = new Bitmap(dialog.FileName);
                AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                if (edit == null)
                {
                    return;
                }

                FrameDimension dimension = new FrameDimension(bit.FrameDimensionsList[0]);
                // Number of frames
                //int frameCount = bit.GetFrameCount(dimension); // TODO: unused variable?
                bit.SelectActiveFrame(dimension, 0);
                edit.GetGifPalette(bit);
                SetPaletteBox();
                listView1.Invalidate();
                Options.ChangedUltimaClass["Animations"] = true;
            }
        }

        private void ReferencePointX(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }

        private void ReferencePointY(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }

        private static bool _lockButton;

        private void PictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (_lockButton || !toolStripButton6.Enabled)
            {
                return;
            }

            numericUpDown2.Value = 418 - e.X;
            numericUpDown1.Value = 335 - e.Y;

            pictureBox1.Invalidate();
        }

        // Change center of frame on key press
        private void TxtSendData_KeyDown(object sender, KeyEventArgs e)
        {
            if (timer1.Enabled)
            {
                return;
            }

            switch (e.KeyCode)
            {
                case Keys.Right:
                {
                    numericUpDownCx.Value--;
                    numericUpDownCx.Invalidate();
                    break;
                }
                case Keys.Left:
                {
                    numericUpDownCx.Value++;
                    numericUpDownCx.Invalidate();
                    break;
                }
                case Keys.Up:
                {
                    numericUpDownCy.Value++;
                    numericUpDownCy.Invalidate();
                    break;
                }
                case Keys.Down:
                {
                    numericUpDownCy.Value--;
                    numericUpDownCy.Invalidate();
                    break;
                }
            }
            pictureBox1.Invalidate();
        }

        // Change center of Reference Point on key press
        private void TxtSendData_KeyDown2(object sender, KeyEventArgs e)
        {
            if (_lockButton || !toolStripButton6.Enabled)
            {
                return;
            }

            switch (e.KeyCode)
            {
                case Keys.Right:
                {
                    numericUpDown2.Value--;
                    numericUpDown2.Invalidate();
                    break;
                }
                case Keys.Left:
                {
                    numericUpDown2.Value++;
                    numericUpDown2.Invalidate();
                    break;
                }
                case Keys.Up:
                {
                    numericUpDown1.Value++;
                    numericUpDown1.Invalidate();
                    break;
                }
                case Keys.Down:
                {
                    numericUpDown1.Value--;
                    numericUpDown1.Invalidate();
                    break;
                }
            }
            pictureBox1.Invalidate();
        }

        // Lock Button
        private void ToolStripButton6_Click(object sender, EventArgs e)
        {
            _lockButton = !_lockButton;
            numericUpDown2.Enabled = !_lockButton;
            numericUpDown1.Enabled = !_lockButton;
        }

        // Add in all Directions
        private void AllDirectionsAddToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_fileType != 0)
            {
                using (OpenFileDialog dialog = new OpenFileDialog())
                {
                    dialog.Multiselect = true;
                    dialog.Title = "Choose 5 GIFs to add";
                    dialog.CheckFileExists = true;
                    dialog.Filter = "Gif files (*.gif;)|*.gif;";
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        trackBar1.Enabled = false;
                        if (dialog.FileNames.Length == 5)
                        {
                            trackBar1.Value = 0;

                            AddFilesAllDirections(dialog);
                        }

                        // Looping if dialog.FileNames.Length != 5
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

                            if (dialog.FileNames.Length != 5)
                            {
                                continue;
                            }

                            trackBar1.Value = 0;

                            AddFilesAllDirections(dialog);
                        }
                        trackBar1.Enabled = true;
                    }
                }
            }

            // Refresh List
            _currentDir = trackBar1.Value;
            AfterSelectTreeView(null, null);
        }

        private void AddFilesAllDirections(OpenFileDialog dialog)
        {
            for (int w = 0; w < dialog.FileNames.Length; w++)
            {
                if (w >= 5)
                {
                    continue;
                }

                // dialog.Filename replaced by dialog.FileNames[w]
                Bitmap bmp = new Bitmap(dialog.FileNames[w]);
                AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                if (edit != null)
                {
                    // Gif Especial Properties
                    if (dialog.FileName.Contains(".gif"))
                    {
                        FrameDimension dimension = new FrameDimension(bmp.FrameDimensionsList[0]);
                        // Number of frames 
                        int frameCount = bmp.GetFrameCount(dimension);
                        progressBar1.Maximum = frameCount;
                        bmp.SelectActiveFrame(dimension, 0);
                        edit.GetGifPalette(bmp);
                        Bitmap[] bitBmp = new Bitmap[frameCount];
                        // Return an Image at a certain index 
                        for (int index = 0; index < frameCount; index++)
                        {
                            bitBmp[index] = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format16bppArgb1555);
                            bmp.SelectActiveFrame(dimension, index);
                            bitBmp[index] = bmp;
                            bitBmp[index] = ConvertBmpAnim(bitBmp[index], (int) numericUpDown3.Value,
                                (int) numericUpDown4.Value, (int) numericUpDown5.Value);
                            edit.AddFrame(bitBmp[index]);
                            TreeNode node = GetNode(_currentBody);
                            if (node != null)
                            {
                                node.ForeColor = Color.Black;
                                node.Nodes[_currentAction].ForeColor = Color.Black;
                            }

                            int i = edit.Frames.Count - 1;
                            var item = new ListViewItem(i.ToString(), 0)
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

                if ((w < 4) && (w < dialog.FileNames.Length - 1))
                {
                    trackBar1.Value++;
                }
            }
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
            _lockButton = false;
            timer1.Enabled = false;
            _blackUnDraw = new Pen(Color.FromArgb(255, 0, 0, 0), 1);
            _whiteUnDraw = new SolidBrush(Color.FromArgb(255, 255, 255, 255));
            _loaded = false;
            ControlEvents.FilePathChangeEvent -= OnFilePathChangeEvent;
        }

        // Play Button Timer
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

        // Play Button
        private void ToolStripButton11_Click(object sender, EventArgs e)
        {
            if (timer1.Enabled)
            {
                timer1.Enabled = false;
                numericUpDownCx.Enabled = true;
                numericUpDownCy.Enabled = true;
                trackBar2.Enabled = true;
                button2.Enabled = true; // Same Center button
                toolStripButton12.Enabled = true; // UnDraw Reference Point button

                if (toolStripButton12.Checked)
                {
                    toolStripButton6.Enabled = false; // Lock button
                    _blackUnDraw = new Pen(Color.FromArgb(0, 0, 0, 0), 1);
                    _whiteUnDraw = new SolidBrush(Color.FromArgb(0, 255, 255, 255));
                }
                else
                {
                    toolStripButton6.Enabled = true;
                    _blackUnDraw = new Pen(Color.FromArgb(255, 0, 0, 0), 1);
                    _whiteUnDraw = new SolidBrush(Color.FromArgb(255, 255, 255, 255));
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
                button2.Enabled = false; // Same Center button
                toolStripButton12.Enabled = false; // UnDraw Reference Point button
                toolStripButton6.Enabled = false; // Lock button
                _blackUnDraw = new Pen(Color.FromArgb(0, 0, 0, 0), 1);
                _whiteUnDraw = new SolidBrush(Color.FromArgb(0, 255, 255, 255));
            }

            pictureBox1.Invalidate();
        }

        // Animation Speed
        private void TrackBar3_ValueChanged(object sender, EventArgs e)
        {
            timer1.Interval = 50 + (trackBar3.Value * 30);
        }

        private void ToolStripButton12_Click(object sender, EventArgs e)
        {
            if (!toolStripButton12.Checked)
            {
                _blackUnDraw = new Pen(Color.FromArgb(255, 0, 0, 0), 1);
                _whiteUnDraw = new SolidBrush(Color.FromArgb(255, 255, 255, 255));
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
                _blackUnDraw = new Pen(Color.FromArgb(0, 0, 0, 0), 1);
                _whiteUnDraw = new SolidBrush(Color.FromArgb(0, 255, 255, 255));
                toolStripButton6.Enabled = false;
                numericUpDown2.Enabled = false;
                numericUpDown1.Enabled = false;
            }
            pictureBox1.Invalidate();
        }

        // All Directions with Canvas
        private void AllDirectionsAddWithCanvasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (_fileType != 0)
                {
                    using (OpenFileDialog dialog = new OpenFileDialog())
                    {
                        dialog.Multiselect = true;
                        dialog.Title = "Choose 5 GIFs to add";
                        dialog.CheckFileExists = true;
                        dialog.Filter = "Gif files (*.gif;)|*.gif;";
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            Color customConvert = Color.FromArgb(255, (int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                            trackBar1.Enabled = false;
                            if (dialog.FileNames.Length == 5)
                            {
                                trackBar1.Value = 0;
                                AddSelectedFiles(dialog, customConvert);
                            }

                            // Looping if dialog.FileNames.Length != 5
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

                                if (dialog.FileNames.Length != 5)
                                {
                                    continue;
                                }

                                trackBar1.Value = 0;
                                AddSelectedFiles(dialog, customConvert);
                            }

                            trackBar1.Enabled = true;
                        }
                    }
                }

                // Refresh List after Canvas reduction
                _currentDir = trackBar1.Value;
                AfterSelectTreeView(null, null);
            }
            catch (OutOfMemoryException)
            {
                // TODO: add logging or fix?
                // ignored
            }
        }

        private void AddSelectedFiles(OpenFileDialog dialog, Color customConvert)
        {
            for (int w = 0; w < dialog.FileNames.Length; w++)
            {
                if (w >= 5)
                {
                    continue;
                }

                // dialog.Filename replaced by dialog.FileNames[w]
                Bitmap bmp = new Bitmap(dialog.FileNames[w]);

                // TODO: fix checking file extension
                // Gif Especial Properties
                if (!dialog.FileNames[w].Contains(".gif"))
                {
                    continue;
                }

                AddAnimationX1(customConvert, bmp);

                if ((w < 4) && (w < dialog.FileNames.Length - 1))
                {
                    trackBar1.Value++;
                }
            }
        }

        private void AddAnimationX1(Color customConvert, Bitmap bmp)
        {
            AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
            if (edit == null)
            {
                return;
            }

            FrameDimension dimension = new FrameDimension(bmp.FrameDimensionsList[0]);

            // Number of frames
            int frameCount = bmp.GetFrameCount(dimension);
            progressBar1.Maximum = frameCount;
            bmp.SelectActiveFrame(dimension, 0);
            edit.GetGifPalette(bmp);

            // Return an Image at a certain index
            Bitmap[] bitBmp = new Bitmap[frameCount];
            for (int index = 0; index < frameCount; index++)
            {
                bitBmp[index] = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format16bppArgb1555);
                bmp.SelectActiveFrame(dimension, index);
                bitBmp[index] = bmp;
            }

            // Canvas algorithm
            int top = 0;
            int bottom = 0;
            int left = 0;
            int right = 0;

            int regressT = -1;
            int regressB = -1;
            int regressL = -1;
            int regressR = -1;

            bool var = true;
            bool breakOk = false;

            // Top
            for (int yf = 0; yf < bitBmp[0].Height; yf++)
            {
                for (int frameIdx = 0; frameIdx < frameCount; frameIdx++)
                {
                    bitBmp[frameIdx].SelectActiveFrame(dimension, frameIdx);
                    for (int xf = 0; xf < bitBmp[0].Width; xf++)
                    {
                        Color pixel = bitBmp[frameIdx].GetPixel(xf, yf);
                        if (pixel == _whiteConvert || pixel == customConvert || pixel.A == 0)
                        {
                            var = true;
                        }

                        if (pixel != _whiteConvert && pixel != customConvert && pixel.A != 0)
                        {
                            var = false;
                            if (yf != 0)
                            {
                                regressT++;
                                yf -= 1;
                                xf = -1;
                                frameIdx = 0;
                            }
                            else
                            {
                                breakOk = true;
                                break;
                            }
                        }

                        if (var && xf == bitBmp[frameIdx].Width - 1 && frameIdx == frameCount - 1 && regressT == -1 &&
                            yf < bitBmp[0].Height - 9)
                        {
                            top += 10;
                        }

                        if (var && xf == bitBmp[frameIdx].Width - 1 && frameIdx == frameCount - 1 && regressT == -1 &&
                            yf >= bitBmp[0].Height - 9)
                        {
                            top++;
                        }

                        if (var && xf == bitBmp[frameIdx].Width - 1 && frameIdx == frameCount - 1 && regressT != -1)
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

                if (yf < bitBmp[0].Height - 9)
                {
                    yf += 9; // 1 of for + 9
                }

                if (breakOk)
                {
                    breakOk = false;
                    break;
                }
            }

            // Bottom
            for (int yf = bitBmp[0].Height - 1; yf > 0; yf--)
            {
                for (int frameIdx = 0; frameIdx < frameCount; frameIdx++)
                {
                    bitBmp[frameIdx].SelectActiveFrame(dimension, frameIdx);
                    for (int xf = 0; xf < bitBmp[0].Width; xf++)
                    {
                        Color pixel = bitBmp[frameIdx].GetPixel(xf, yf);
                        if (pixel == _whiteConvert || pixel == customConvert || pixel.A == 0)
                        {
                            var = true;
                        }

                        if (pixel != _whiteConvert && pixel != customConvert && pixel.A != 0)
                        {
                            var = false;
                            if (yf != bitBmp[0].Height - 1)
                            {
                                regressB++;
                                yf += 1;
                                xf = -1;
                                frameIdx = 0;
                            }
                            else
                            {
                                breakOk = true;
                                break;
                            }
                        }

                        if (var && xf == bitBmp[frameIdx].Width - 1 && frameIdx == frameCount - 1 && regressB == -1 &&
                            yf > 9)
                        {
                            bottom += 10;
                        }

                        if (var && xf == bitBmp[frameIdx].Width - 1 && frameIdx == frameCount - 1 && regressB == -1 &&
                            yf <= 9)
                        {
                            bottom++;
                        }

                        if (var && xf == bitBmp[frameIdx].Width - 1 && frameIdx == frameCount - 1 && regressB != -1)
                        {
                            bottom -= regressB;
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

            // Left
            for (int xf = 0; xf < bitBmp[0].Width; xf++)
            {
                for (int frameIdx = 0; frameIdx < frameCount; frameIdx++)
                {
                    bitBmp[frameIdx].SelectActiveFrame(dimension, frameIdx);
                    for (int yf = 0; yf < bitBmp[0].Height; yf++)
                    {
                        Color pixel = bitBmp[frameIdx].GetPixel(xf, yf);
                        if (pixel == _whiteConvert || pixel == customConvert || pixel.A == 0)
                        {
                            var = true;
                        }

                        if (pixel != _whiteConvert && pixel != customConvert && pixel.A != 0)
                        {
                            var = false;
                            if (xf != 0)
                            {
                                regressL++;
                                xf -= 1;
                                yf = -1;
                                frameIdx = 0;
                            }
                            else
                            {
                                breakOk = true;
                                break;
                            }
                        }

                        if (var && yf == bitBmp[frameIdx].Height - 1 && frameIdx == frameCount - 1 && regressL == -1 &&
                            xf < bitBmp[0].Width - 9)
                        {
                            left += 10;
                        }

                        if (var && yf == bitBmp[frameIdx].Height - 1 && frameIdx == frameCount - 1 && regressL == -1 &&
                            xf >= bitBmp[0].Width - 9)
                        {
                            left++;
                        }

                        if (var && yf == bitBmp[frameIdx].Height - 1 && frameIdx == frameCount - 1 && regressL != -1)
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

                if (xf < bitBmp[0].Width - 9)
                {
                    xf += 9; // 1 of for + 9
                }

                if (breakOk)
                {
                    breakOk = false;
                    break;
                }
            }

            // Right
            for (int xf = bitBmp[0].Width - 1; xf > 0; xf--)
            {
                for (int frameIdx = 0; frameIdx < frameCount; frameIdx++)
                {
                    bitBmp[frameIdx].SelectActiveFrame(dimension, frameIdx);
                    for (int yf = 0; yf < bitBmp[0].Height; yf++)
                    {
                        Color pixel = bitBmp[frameIdx].GetPixel(xf, yf);
                        if (pixel == _whiteConvert || pixel == customConvert || pixel.A == 0)
                        {
                            var = true;
                        }

                        if (pixel != _whiteConvert && pixel != customConvert && pixel.A != 0)
                        {
                            var = false;
                            if (xf != bitBmp[0].Width - 1)
                            {
                                regressR++;
                                xf += 1;
                                yf = -1;
                                frameIdx = 0;
                            }
                            else
                            {
                                breakOk = true;
                                break;
                            }
                        }

                        if (var && yf == bitBmp[frameIdx].Height - 1 && frameIdx == frameCount - 1 && regressR == -1 &&
                            xf > 9)
                        {
                            right += 10;
                        }

                        if (var && yf == bitBmp[frameIdx].Height - 1 && frameIdx == frameCount - 1 && regressR == -1 &&
                            xf <= 9)
                        {
                            right++;
                        }

                        if (var && yf == bitBmp[frameIdx].Height - 1 && frameIdx == frameCount - 1 && regressR != -1)
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
                Rectangle rect = new Rectangle(left, top, bitBmp[index].Width - left - right, bitBmp[index].Height - top - bottom);
                bitBmp[index].SelectActiveFrame(dimension, index);
                bitBmp[index] = bitBmp[index].Clone(rect, PixelFormat.Format16bppArgb1555);
            }

            // End of Canvas algorithm

            for (int index = 0; index < frameCount; index++)
            {
                bitBmp[index].SelectActiveFrame(dimension, index);
                bitBmp[index] = ConvertBmpAnim(bitBmp[index], (int) numericUpDown3.Value,
                    (int) numericUpDown4.Value, (int) numericUpDown5.Value);
                edit.AddFrame(bitBmp[index]);
                TreeNode node = GetNode(_currentBody);
                if (node != null)
                {
                    node.ForeColor = Color.Black;
                    node.Nodes[_currentAction].ForeColor = Color.Black;
                }

                int i = edit.Frames.Count - 1;
                var item = new ListViewItem(i.ToString(), 0)
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
                            Color customConvert = Color.FromArgb(255, (int)numericUpDown3.Value,
                                (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                            //My Soulblighter Modifications
                            for (int w = 0; w < dialog.FileNames.Length; w++)
                            {
                                // dialog.Filename replaced by dialog.FileNames[w]
                                Bitmap bmp = new Bitmap(dialog.FileNames[w]);

                                // TODO: fix checking file extension
                                // Gif Especial Properties
                                if (!dialog.FileNames[w].Contains(".gif"))
                                {
                                    continue;
                                }

                                AddAnimationX1(customConvert, bmp);
                            }
                        }
                    }
                }

                // Refresh List after Canvas reduction
                _currentDir = trackBar1.Value;
                AfterSelectTreeView(null, null);
            }
            catch (OutOfMemoryException)
            {
                // TODO: add logging or fix?
                // ignored
            }
        }

        private void OnClickGeneratePalette(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = true;
                dialog.Title = "Choose images to generate from";
                dialog.CheckFileExists = true;
                dialog.Filter = "Image files (*.tif;*.tiff;*.bmp;*.png;*.jpg;*.jpeg)|*.tif;*.tiff;*.bmp;*.png;*.jpg;*.jpeg";
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                foreach (string filename in dialog.FileNames)
                {
                    Bitmap bit = new Bitmap(filename);
                    AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                    if (edit != null)
                    {
                        bit = ConvertBmpAnim(bit, (int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                        edit.GetImagePalette(bit);
                    }
                    SetPaletteBox();
                    listView1.Invalidate();
                    Options.ChangedUltimaClass["Animations"] = true;
                    SetPaletteBox();
                }
            }
        }
        //End of Soulblighter Modification

        private static unsafe Bitmap ConvertBmpAnim(Bitmap bmp, int red, int green, int blue)
        {
            //Extra background
            int extraBack = (red / 8 * 1024) + (green / 8 * 32) + (blue / 8) + 32768;

            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
            ushort* line = (ushort*)bd.Scan0;
            int delta = bd.Stride >> 1;

            Bitmap bmpNew = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format16bppArgb1555);
            BitmapData bdNew = bmpNew.LockBits(new Rectangle(0, 0, bmpNew.Width, bmpNew.Height), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);

            ushort* lineNew = (ushort*)bdNew.Scan0;
            int deltaNew = bdNew.Stride >> 1;

            for (int y = 0; y < bmp.Height; ++y, line += delta, lineNew += deltaNew)
            {
                ushort* cur = line;
                ushort* curNew = lineNew;
                for (int x = 0; x < bmp.Width; ++x)
                {
                    //My Soulblighter Modification
                    // Convert color 0,0,0 to 0,0,8
                    if (cur[x] == 32768)
                    {
                        curNew[x] = 32769;
                    }

                    if (cur[x] != 65535 && cur[x] != extraBack && cur[x] > 32768) //True White == BackGround
                    {
                        curNew[x] = cur[x];
                    }
                    //End of Soulblighter Modification
                }
            }
            bmp.UnlockBits(bd);
            bmpNew.UnlockBits(bdNew);
            return bmpNew;
        }

        private void OnClickExportAllToVD(object sender, EventArgs e)
        {
            if (_fileType == 0)
            {
                return;
            }

            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                for (int i = 0; i < treeView1.Nodes.Count; ++i)
                {
                    int index = (int)treeView1.Nodes[i].Tag;
                    if (index < 0 || treeView1.Nodes[i].Parent != null ||
                        treeView1.Nodes[i].ForeColor == Color.Red)
                    {
                        continue;
                    }

                    string fileName = Path.Combine(dialog.SelectedPath, $"anim{_fileType}_0x{index:X}.vd");
                    Ultima.AnimationEdit.ExportToVD(_fileType, index, fileName);
                }

                MessageBox.Show($"All Animations saved to {dialog.SelectedPath}",
                    "Export", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
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
                        _animCx[trackBar1.Value] = (int)numericUpDownCx.Value;
                        _animCy[trackBar1.Value] = (int)numericUpDownCy.Value;
                        trackBar1.Value++;
                        count++;
                    }
                    else
                    {
                        _animCx[trackBar1.Value] = (int)numericUpDownCx.Value;
                        _animCy[trackBar1.Value] = (int)numericUpDownCy.Value;
                        trackBar1.Value = 0;
                        count++;
                    }
                }
                toolStripLabel8.Text = $"1: {_animCx[0]}/{_animCy[0]}";
                toolStripLabel9.Text = $"2: {_animCx[1]}/{_animCy[1]}";
                toolStripLabel10.Text = $"3: {_animCx[2]}/{_animCy[2]}";
                toolStripLabel11.Text = $"4: {_animCx[3]}/{_animCy[3]}";
                toolStripLabel12.Text = $"5: {_animCx[4]}/{_animCy[4]}";
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

        // Set Button
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
                        AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                        if (edit != null && edit.Frames.Count >= trackBar2.Value)
                        {
                            for (int index = 0; index < edit.Frames.Count; index++)
                            {
                                edit.Frames[index].ChangeCenter(_animCx[i], _animCy[i]);
                                Options.ChangedUltimaClass["Animations"] = true;
                                pictureBox1.Invalidate();
                            }
                        }
                    }
                }
                catch (NullReferenceException)
                {
                    // TODO: add logging or fix?
                    // ignored
                }

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

        // Add Directions with Canvas ( CV5 style GIF )
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
                            Color customConvert = Color.FromArgb(255, (int)numericUpDown3.Value,
                                (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                            trackBar1.Enabled = false;
                            trackBar1.Value = 0;
                            Bitmap bmp = new Bitmap(dialog.FileName);
                            AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction,
                                _currentDir);
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
                                    Bitmap[] bitBmp = new Bitmap[frameCount];
                                    // Return an Image at a certain index 
                                    for (int index = 0; index < frameCount; index++)
                                    {
                                        bitBmp[index] = new Bitmap(bmp.Width, bmp.Height,
                                            PixelFormat.Format16bppArgb1555);
                                        bmp.SelectActiveFrame(dimension, index);
                                        bitBmp[index] = bmp;
                                    }

                                    Cv5CanvasAlgorithm(bitBmp, frameCount, dimension, customConvert);

                                    edit = Cv5AnimIdxPositions(frameCount, bitBmp, dimension, edit, bmp);

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

                // Refresh List after Canvas reduction
                _currentDir = trackBar1.Value;
                AfterSelectTreeView(null, null);
            }
            catch (NullReferenceException)
            {
                // TODO: add logging or fix?
                trackBar1.Enabled = true;
            }
        }

        private AnimIdx Cv5AnimIdxPositions(int frameCount, Bitmap[] bitBmp, FrameDimension dimension, AnimIdx edit, Bitmap bmp)
        {
            // position 0
            for (int index = frameCount / 8 * 4; index < frameCount / 8 * 5; index++)
            {
                bitBmp[index].SelectActiveFrame(dimension, index);
                bitBmp[index] = ConvertBmpAnimCv5(bitBmp[index], (int) numericUpDown3.Value, (int) numericUpDown4.Value,
                    (int) numericUpDown5.Value);
                edit.AddFrame(bitBmp[index]);
                TreeNode node = GetNode(_currentBody);
                if (node != null)
                {
                    node.ForeColor = Color.Black;
                    node.Nodes[_currentAction].ForeColor = Color.Black;
                }

                int i = edit.Frames.Count - 1;
                var item = new ListViewItem(i.ToString(), 0)
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

                if (index == (frameCount / 8 * 5) - 1)
                {
                    trackBar1.Value++;
                }
            }

            // position 1
            edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
            bmp.SelectActiveFrame(dimension, 0);
            edit.GetGifPalette(bmp);
            for (int index = 0; index < frameCount / 8; index++)
            {
                bitBmp[index].SelectActiveFrame(dimension, index);
                bitBmp[index] = ConvertBmpAnimCv5(bitBmp[index], (int) numericUpDown3.Value, (int) numericUpDown4.Value,
                    (int) numericUpDown5.Value);
                edit.AddFrame(bitBmp[index]);
                TreeNode node = GetNode(_currentBody);
                if (node != null)
                {
                    node.ForeColor = Color.Black;
                    node.Nodes[_currentAction].ForeColor = Color.Black;
                }

                int i = edit.Frames.Count - 1;
                ListViewItem item = new ListViewItem(i.ToString(), 0)
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

                if (index == (frameCount / 8) - 1)
                {
                    trackBar1.Value++;
                }
            }

            // position 2
            edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
            bmp.SelectActiveFrame(dimension, 0);
            edit.GetGifPalette(bmp);
            for (int index = frameCount / 8 * 5; index < frameCount / 8 * 6; index++)
            {
                bitBmp[index].SelectActiveFrame(dimension, index);
                bitBmp[index] = ConvertBmpAnimCv5(bitBmp[index], (int) numericUpDown3.Value, (int) numericUpDown4.Value,
                    (int) numericUpDown5.Value);
                edit.AddFrame(bitBmp[index]);
                TreeNode node = GetNode(_currentBody);
                if (node != null)
                {
                    node.ForeColor = Color.Black;
                    node.Nodes[_currentAction].ForeColor = Color.Black;
                }

                int i = edit.Frames.Count - 1;
                var item = new ListViewItem(i.ToString(), 0)
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

                if (index == (frameCount / 8 * 6) - 1)
                {
                    trackBar1.Value++;
                }
            }

            // position 3
            edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
            bmp.SelectActiveFrame(dimension, 0);
            edit.GetGifPalette(bmp);
            for (int index = frameCount / 8 * 1; index < frameCount / 8 * 2; index++)
            {
                bitBmp[index].SelectActiveFrame(dimension, index);
                bitBmp[index] = ConvertBmpAnimCv5(bitBmp[index], (int) numericUpDown3.Value, (int) numericUpDown4.Value,
                    (int) numericUpDown5.Value);
                edit.AddFrame(bitBmp[index]);
                TreeNode node = GetNode(_currentBody);
                if (node != null)
                {
                    node.ForeColor = Color.Black;
                    node.Nodes[_currentAction].ForeColor = Color.Black;
                }

                int i = edit.Frames.Count - 1;
                var item = new ListViewItem(i.ToString(), 0)
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

                if (index == (frameCount / 8 * 2) - 1)
                {
                    trackBar1.Value++;
                }
            }

            // position 4
            edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
            bmp.SelectActiveFrame(dimension, 0);
            edit.GetGifPalette(bmp);
            for (int index = frameCount / 8 * 6; index < frameCount / 8 * 7; index++)
            {
                bitBmp[index].SelectActiveFrame(dimension, index);
                bitBmp[index] = ConvertBmpAnimCv5(bitBmp[index], (int) numericUpDown3.Value, (int) numericUpDown4.Value,
                    (int) numericUpDown5.Value);
                edit.AddFrame(bitBmp[index]);
                TreeNode node = GetNode(_currentBody);
                if (node != null)
                {
                    node.ForeColor = Color.Black;
                    node.Nodes[_currentAction].ForeColor = Color.Black;
                }

                int i = edit.Frames.Count - 1;
                var item = new ListViewItem(i.ToString(), 0)
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

            return edit;
        }

        private static unsafe Bitmap ConvertBmpAnimCv5(Bitmap bmp, int red, int green, int blue)
        {
            //Extra background
            int extraBack = (red / 8 * 1024) + (green / 8 * 32) + (blue / 8) + 32768;

            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
            ushort* line = (ushort*)bd.Scan0;
            int delta = bd.Stride >> 1;

            Bitmap bmpNew = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format16bppArgb1555);
            BitmapData bdNew = bmpNew.LockBits(new Rectangle(0, 0, bmpNew.Width, bmpNew.Height), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);

            ushort* lineNew = (ushort*)bdNew.Scan0;
            int deltaNew = bdNew.Stride >> 1;

            for (int y = 0; y < bmp.Height; ++y, line += delta, lineNew += deltaNew)
            {
                ushort* cur = line;
                ushort* curNew = lineNew;
                for (int x = 0; x < bmp.Width; ++x)
                {
                    //My Soulblighter Modification
                    // Convert color 0,0,0 to 0,0,8
                    if (cur[x] == 32768)
                    {
                        curNew[x] = 32769;
                    }

                    if (cur[x] != 65535 && cur[x] != 54965 && cur[x] != extraBack && cur[x] > 32768) //True White == BackGround
                    {
                        curNew[x] = cur[x];
                    }
                    //End of Soulblighter Modification
                }
            }
            bmp.UnlockBits(bd);
            bmpNew.UnlockBits(bdNew);
            return bmpNew;
        }

        private static readonly Color _greyConvert = Color.FromArgb(255, 170, 170, 170);

        private static void Cv5CanvasAlgorithm(Bitmap[] bitBmp, int frameCount, FrameDimension dimension, Color customConvert)
        {
            // TODO: Needs better names
            // TODO: This code needs documentation. This algorithm is not really readable

            // Order of calls looks important
            // Looks like it is import for Gif/bmps from Diablo cv5 format
            // Some materials about Diablo formats:
            // - https://d2mods.info/resources/infinitum/tut_files/dcc_tutorial/
            // - https://d2mods.info/resources/infinitum/tut_files/dcc_tutorial/chapter4.html
            //
            const int frameDivider = 8;

            // position 0
            Cv5ProcessFrames(bitBmp, dimension, customConvert, GetInitialFrameIndex(frameCount, frameDivider, 4), GetMaximumFrameIndex(frameCount, frameDivider, 4));
            // position 1
            Cv5ProcessFrames(bitBmp, dimension, customConvert, GetInitialFrameIndex(frameCount, frameDivider, 0), GetMaximumFrameIndex(frameCount, frameDivider, 0));
            // position 2
            Cv5ProcessFrames(bitBmp, dimension, customConvert, GetInitialFrameIndex(frameCount, frameDivider, 5), GetMaximumFrameIndex(frameCount, frameDivider, 5));
            // position 3
            Cv5ProcessFrames(bitBmp, dimension, customConvert, GetInitialFrameIndex(frameCount, frameDivider, 1), GetMaximumFrameIndex(frameCount, frameDivider, 1));
            // position 4
            Cv5ProcessFrames(bitBmp, dimension, customConvert, GetInitialFrameIndex(frameCount, frameDivider, 6), GetMaximumFrameIndex(frameCount, frameDivider, 6));
        }

        private static int GetInitialFrameIndex(int frameCount, int frameDivider, int position)
        {
            return frameCount / frameDivider * position;
        }

        private static int GetMaximumFrameIndex(int frameCount, int frameDivider, int position)
        {
            return frameCount / frameDivider * (position + 1);
        }

        private static void Cv5ProcessFrames(Bitmap[] bitBmp, FrameDimension dimension, Color customConvert, int initialFrameIndex, int maximumFrameIndex)
        {
            int top = 0;
            int bottom = 0;
            int left = 0;
            int right = 0;

            int regressT = -1;
            int regressB = -1;
            int regressL = -1;
            int regressR = -1;

            bool var = true;
            bool breakOk = false;

            // Top
            for (int yf = 0; yf < bitBmp[initialFrameIndex].Height; yf++)
            {
                for (int frameIdx = initialFrameIndex; frameIdx < maximumFrameIndex; frameIdx++)
                {
                    bitBmp[frameIdx].SelectActiveFrame(dimension, frameIdx);
                    for (int xf = 0; xf < bitBmp[initialFrameIndex].Width; xf++)
                    {
                        Color pixel = bitBmp[frameIdx].GetPixel(xf, yf);
                        if (pixel == _whiteConvert || pixel == _greyConvert || pixel == customConvert || pixel.A == 0)
                        {
                            var = true;
                        }

                        if (pixel != _whiteConvert && pixel != _greyConvert && pixel != customConvert && pixel.A != 0)
                        {
                            var = false;
                            if (yf != 0)
                            {
                                regressT++;
                                yf -= 1;
                                xf = -1;
                                frameIdx = initialFrameIndex;
                            }
                            else
                            {
                                breakOk = true;
                                break;
                            }
                        }

                        if (var && xf == bitBmp[frameIdx].Width - 1 && frameIdx == (maximumFrameIndex) - 1 && regressT == -1 &&
                            yf < bitBmp[0].Height - 9)
                        {
                            top += 10;
                        }

                        if (var && xf == bitBmp[frameIdx].Width - 1 && frameIdx == (maximumFrameIndex) - 1 && regressT == -1 &&
                            yf >= bitBmp[0].Height - 9)
                        {
                            top++;
                        }

                        if (var && xf == bitBmp[frameIdx].Width - 1 && frameIdx == (maximumFrameIndex) - 1 && regressT != -1)
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

                if (yf < bitBmp[initialFrameIndex].Height - 9)
                {
                    yf += 9; // 1 of for + 9
                }

                if (breakOk)
                {
                    breakOk = false;
                    break;
                }
            }

            // Bottom
            for (int yf = bitBmp[initialFrameIndex].Height - 1; yf > 0; yf--)
            {
                for (int frameIdx = initialFrameIndex; frameIdx < maximumFrameIndex; frameIdx++)
                {
                    bitBmp[frameIdx].SelectActiveFrame(dimension, frameIdx);
                    for (int xf = 0; xf < bitBmp[initialFrameIndex].Width; xf++)
                    {
                        Color pixel = bitBmp[frameIdx].GetPixel(xf, yf);
                        if (pixel == _whiteConvert || pixel == _greyConvert || pixel == customConvert || pixel.A == 0)
                        {
                            var = true;
                        }

                        if (pixel != _whiteConvert && pixel != _greyConvert && pixel != customConvert && pixel.A != 0)
                        {
                            var = false;
                            if (yf != bitBmp[initialFrameIndex].Height - 1)
                            {
                                regressB++;
                                yf += 1;
                                xf = -1;
                                frameIdx = initialFrameIndex;
                            }
                            else
                            {
                                breakOk = true;
                                break;
                            }
                        }

                        if (var && xf == bitBmp[frameIdx].Width - 1 && frameIdx == (maximumFrameIndex) - 1 && regressB == -1 &&
                            yf > 9)
                        {
                            bottom += 10;
                        }

                        if (var && xf == bitBmp[frameIdx].Width - 1 && frameIdx == (maximumFrameIndex) - 1 && regressB == -1 &&
                            yf <= 9)
                        {
                            bottom++;
                        }

                        if (var && xf == bitBmp[frameIdx].Width - 1 && frameIdx == (maximumFrameIndex) - 1 && regressB != -1)
                        {
                            bottom -= regressB;
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

            // Left
            for (int xf = 0; xf < bitBmp[initialFrameIndex].Width; xf++)
            {
                for (int frameIdx = initialFrameIndex; frameIdx < maximumFrameIndex; frameIdx++)
                {
                    bitBmp[frameIdx].SelectActiveFrame(dimension, frameIdx);
                    for (int yf = 0; yf < bitBmp[initialFrameIndex].Height; yf++)
                    {
                        Color pixel = bitBmp[frameIdx].GetPixel(xf, yf);
                        if (pixel == _whiteConvert || pixel == _greyConvert || pixel == customConvert || pixel.A == 0)
                        {
                            var = true;
                        }

                        if (pixel != _whiteConvert && pixel != _greyConvert && pixel != customConvert && pixel.A != 0)
                        {
                            var = false;
                            if (xf != 0)
                            {
                                regressL++;
                                xf -= 1;
                                yf = -1;
                                frameIdx = initialFrameIndex;
                            }
                            else
                            {
                                breakOk = true;
                                break;
                            }
                        }

                        if (var && yf == bitBmp[frameIdx].Height - 1 && frameIdx == (maximumFrameIndex) - 1 &&
                            regressL == -1 &&
                            xf < bitBmp[0].Width - 9)
                        {
                            left += 10;
                        }

                        if (var && yf == bitBmp[frameIdx].Height - 1 && frameIdx == (maximumFrameIndex) - 1 &&
                            regressL == -1 &&
                            xf >= bitBmp[0].Width - 9)
                        {
                            left++;
                        }

                        if (var && yf == bitBmp[frameIdx].Height - 1 && frameIdx == (maximumFrameIndex) - 1 && regressL != -1)
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

                if (xf < bitBmp[initialFrameIndex].Width - 9)
                {
                    xf += 9; // 1 of for + 9
                }

                if (breakOk)
                {
                    breakOk = false;
                    break;
                }
            }

            // Right
            for (int xf = bitBmp[initialFrameIndex].Width - 1; xf > 0; xf--)
            {
                for (int frameIdx = initialFrameIndex; frameIdx < maximumFrameIndex; frameIdx++)
                {
                    bitBmp[frameIdx].SelectActiveFrame(dimension, frameIdx);
                    for (int yf = 0; yf < bitBmp[initialFrameIndex].Height; yf++)
                    {
                        Color pixel = bitBmp[frameIdx].GetPixel(xf, yf);
                        if (pixel == _whiteConvert || pixel == _greyConvert || pixel == customConvert || pixel.A == 0)
                        {
                            var = true;
                        }

                        if (pixel != _whiteConvert && pixel != _greyConvert && pixel != customConvert && pixel.A != 0)
                        {
                            var = false;
                            if (xf != bitBmp[initialFrameIndex].Width - 1)
                            {
                                regressR++;
                                xf += 1;
                                yf = -1;
                                frameIdx = initialFrameIndex;
                            }
                            else
                            {
                                breakOk = true;
                                break;
                            }
                        }

                        if (var && yf == bitBmp[frameIdx].Height - 1 && frameIdx == (maximumFrameIndex) - 1 &&
                            regressR == -1 &&
                            xf > 9)
                        {
                            right += 10;
                        }

                        if (var && yf == bitBmp[frameIdx].Height - 1 && frameIdx == (maximumFrameIndex) - 1 &&
                            regressR == -1 &&
                            xf <= 9)
                        {
                            right++;
                        }

                        if (var && yf == bitBmp[frameIdx].Height - 1 && frameIdx == (maximumFrameIndex) - 1 && regressR != -1)
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

            for (int index = initialFrameIndex; index < maximumFrameIndex; index++)
            {
                Rectangle rect = new Rectangle(left, top, bitBmp[index].Width - left - right, bitBmp[index].Height - top - bottom);
                bitBmp[index].SelectActiveFrame(dimension, index);
                bitBmp[index] = bitBmp[index].Clone(rect, PixelFormat.Format16bppArgb1555);
            }
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

        // All directions Add KRFrameViewer
        private void AllDirectionsAddWithCanvasKRFrameEditorColorCorrectorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (_fileType != 0)
                {
                    using (OpenFileDialog dialog = new OpenFileDialog())
                    {
                        dialog.Multiselect = false;
                        dialog.Title = "Choose 1 Gif ( with all directions in KRFrameViewer Style ) to add";
                        dialog.CheckFileExists = true;
                        dialog.Filter = "Gif files (*.gif;)|*.gif;";
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            Color customConvert = Color.FromArgb(255, (int)numericUpDown3.Value,
                                (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                            trackBar1.Enabled = false;
                            trackBar1.Value = 0;
                            Bitmap bmp = new Bitmap(dialog.FileName);
                            AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction,
                                _currentDir);
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
                                    Bitmap[] bitBmp = new Bitmap[frameCount];
                                    // Return an Image at a certain index 
                                    for (int index = 0; index < frameCount; index++)
                                    {
                                        bitBmp[index] = new Bitmap(bmp.Width, bmp.Height,
                                            PixelFormat.Format16bppArgb1555);
                                        bmp.SelectActiveFrame(dimension, index);
                                        bitBmp[index] = bmp;
                                    }

                                    KrCanvasAlgorithm(bitBmp, frameCount, dimension, customConvert);

                                    edit = KrAnimIdxPositions(frameCount, bitBmp, dimension, edit, bmp);

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

                // Refresh List after Canvas reduction
                _currentDir = trackBar1.Value;
                AfterSelectTreeView(null, null);
            }
            catch (NullReferenceException)
            {
                // TODO: add logging or fix?
                trackBar1.Enabled = true;
            }
        }

        private AnimIdx KrAnimIdxPositions(int frameCount, Bitmap[] bitBmp, FrameDimension dimension, AnimIdx edit, Bitmap bmp)
        {
            // position 0
            for (int index = frameCount / 5 * 0; index < frameCount / 5 * 1; index++)
            {
                bitBmp[index].SelectActiveFrame(dimension, index);
                bitBmp[index] = ConvertBmpAnimKr(bitBmp[index], (int) numericUpDown3.Value, (int) numericUpDown4.Value,
                    (int) numericUpDown5.Value);
                edit.AddFrame(bitBmp[index]);
                TreeNode node = GetNode(_currentBody);
                if (node != null)
                {
                    node.ForeColor = Color.Black;
                    node.Nodes[_currentAction].ForeColor = Color.Black;
                }

                int i = edit.Frames.Count - 1;
                var item = new ListViewItem(i.ToString(), 0)
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

                if (index == (frameCount / 5 * 1) - 1)
                {
                    trackBar1.Value++;
                }
            }

            // position 1
            edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
            bmp.SelectActiveFrame(dimension, 0);
            edit.GetGifPalette(bmp);
            for (int index = frameCount / 5 * 1; index < frameCount / 5 * 2; index++)
            {
                bitBmp[index].SelectActiveFrame(dimension, index);
                bitBmp[index] = ConvertBmpAnimKr(bitBmp[index], (int) numericUpDown3.Value, (int) numericUpDown4.Value,
                    (int) numericUpDown5.Value);
                edit.AddFrame(bitBmp[index]);
                TreeNode node = GetNode(_currentBody);
                if (node != null)
                {
                    node.ForeColor = Color.Black;
                    node.Nodes[_currentAction].ForeColor = Color.Black;
                }

                int i = edit.Frames.Count - 1;
                var item = new ListViewItem(i.ToString(), 0)
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

                if (index == (frameCount / 5 * 2) - 1)
                {
                    trackBar1.Value++;
                }
            }

            // position 2
            edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
            bmp.SelectActiveFrame(dimension, 0);
            edit.GetGifPalette(bmp);
            for (int index = frameCount / 5 * 2; index < frameCount / 5 * 3; index++)
            {
                bitBmp[index].SelectActiveFrame(dimension, index);
                bitBmp[index] = ConvertBmpAnimKr(bitBmp[index], (int) numericUpDown3.Value, (int) numericUpDown4.Value,
                    (int) numericUpDown5.Value);
                edit.AddFrame(bitBmp[index]);
                TreeNode node = GetNode(_currentBody);
                if (node != null)
                {
                    node.ForeColor = Color.Black;
                    node.Nodes[_currentAction].ForeColor = Color.Black;
                }

                int i = edit.Frames.Count - 1;
                var item = new ListViewItem(i.ToString(), 0)
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

                if (index == (frameCount / 5 * 3) - 1)
                {
                    trackBar1.Value++;
                }
            }

            // position 3
            edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
            bmp.SelectActiveFrame(dimension, 0);
            edit.GetGifPalette(bmp);
            for (int index = frameCount / 5 * 3; index < frameCount / 5 * 4; index++)
            {
                bitBmp[index].SelectActiveFrame(dimension, index);
                bitBmp[index] = ConvertBmpAnimKr(bitBmp[index], (int) numericUpDown3.Value, (int) numericUpDown4.Value,
                    (int) numericUpDown5.Value);
                edit.AddFrame(bitBmp[index]);
                TreeNode node = GetNode(_currentBody);
                if (node != null)
                {
                    node.ForeColor = Color.Black;
                    node.Nodes[_currentAction].ForeColor = Color.Black;
                }

                int i = edit.Frames.Count - 1;
                var item = new ListViewItem(i.ToString(), 0)
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

                if (index == (frameCount / 5 * 4) - 1)
                {
                    trackBar1.Value++;
                }
            }

            // position 4
            edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
            bmp.SelectActiveFrame(dimension, 0);
            edit.GetGifPalette(bmp);
            for (int index = frameCount / 5 * 4; index < frameCount / 5 * 5; index++)
            {
                bitBmp[index].SelectActiveFrame(dimension, index);
                bitBmp[index] = ConvertBmpAnimKr(bitBmp[index], (int) numericUpDown3.Value, (int) numericUpDown4.Value,
                    (int) numericUpDown5.Value);
                edit.AddFrame(bitBmp[index]);
                TreeNode node = GetNode(_currentBody);
                if (node != null)
                {
                    node.ForeColor = Color.Black;
                    node.Nodes[_currentAction].ForeColor = Color.Black;
                }

                int i = edit.Frames.Count - 1;
                var item = new ListViewItem(i.ToString(), 0)
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

            return edit;
        }

        private static unsafe Bitmap ConvertBmpAnimKr(Bitmap bmp, int red, int green, int blue)
        {
            //Extra background
            int extraBack = (red / 8 * 1024) + (green / 8 * 32) + (blue / 8) + 32768;
            //
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
            ushort* line = (ushort*)bd.Scan0;
            int delta = bd.Stride >> 1;

            Bitmap bmpNew = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format16bppArgb1555);
            BitmapData bdNew = bmpNew.LockBits(new Rectangle(0, 0, bmpNew.Width, bmpNew.Height), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);

            ushort* lineNew = (ushort*)bdNew.Scan0;
            int deltaNew = bdNew.Stride >> 1;

            for (int y = 0; y < bmp.Height; ++y, line += delta, lineNew += deltaNew)
            {
                ushort* cur = line;
                ushort* curNew = lineNew;
                for (int x = 0; x < bmp.Width; ++x)
                {
                    //if (cur[X] != 53235)
                    //{
                    // Convert back to RGB
                    int blueTemp = (cur[x] - 32768) / 32;
                    blueTemp *= 32;
                    blueTemp = cur[x] - 32768 - blueTemp;
                    int greenTemp = (cur[x] - 32768) / 1024;
                    greenTemp *= 1024;
                    greenTemp = cur[x] - 32768 - greenTemp - blueTemp;
                    greenTemp /= 32;
                    int redTemp = (cur[x] - 32768) / 1024;
                    // remove green colors
                    if (greenTemp > blueTemp && greenTemp > redTemp && greenTemp > 10)
                    {
                        cur[x] = 65535;
                    }
                    //}

                    //My Soulblighter Modification
                    // Convert color 0,0,0 to 0,0,8
                    if (cur[x] == 32768)
                    {
                        curNew[x] = 32769;
                    }

                    if (cur[x] != 65535 && cur[x] != 54965 && cur[x] != extraBack && cur[x] > 32768) //True White == BackGround
                    {
                        curNew[x] = cur[x];
                    }
                    //End of Soulblighter Modification
                }
            }
            bmp.UnlockBits(bd);
            bmpNew.UnlockBits(bdNew);
            return bmpNew;
        }

        private static void KrCanvasAlgorithm(Bitmap[] bitBmp, int frameCount, FrameDimension dimension, Color customConvert)
        {
            /*
             * TODO: both methods needs better names.
             *
             *      Duplication here was huge. Now it is reduced to one method with parameter.
             *      It still needs further reducing.
             *      It may be possible to merge code with CV5 routines.
             */
            // TODO: Needs better names
            // TODO: This code needs documentation. This algorithm is not really readable

            // Order of calls looks important
            // Looks like it is import for Gif/bmps from KR client format
            const int frameDivider = 5;

            KrProcessFrames(bitBmp, dimension, customConvert, GetInitialFrameIndex(frameCount, frameDivider, 0), GetMaximumFrameIndex(frameCount, frameDivider, 0));
            KrProcessFrames(bitBmp, dimension, customConvert, GetInitialFrameIndex(frameCount, frameDivider, 1), GetMaximumFrameIndex(frameCount, frameDivider, 1));
            KrProcessFrames(bitBmp, dimension, customConvert, GetInitialFrameIndex(frameCount, frameDivider, 2), GetMaximumFrameIndex(frameCount, frameDivider, 2));
            KrProcessFrames(bitBmp, dimension, customConvert, GetInitialFrameIndex(frameCount, frameDivider, 3), GetMaximumFrameIndex(frameCount, frameDivider, 3));
            KrProcessFrames(bitBmp, dimension, customConvert, GetInitialFrameIndex(frameCount, frameDivider, 4), GetMaximumFrameIndex(frameCount, frameDivider, 4));
        }

        private static void KrProcessFrames(Bitmap[] bitBmp, FrameDimension dimension, Color customConvert, int initialFrameIndex, int maximumFrameIndex)
        {
            int top = 0;
            int bottom = 0;
            int left = 0;
            int right = 0;

            int regressT = -1;
            int regressB = -1;
            int regressL = -1;
            int regressR = -1;

            bool var = true;
            bool breakOk = false;

            // Top
            for (int yf = 0; yf < bitBmp[initialFrameIndex].Height; yf++)
            {
                for (int frameIdx = initialFrameIndex; frameIdx < maximumFrameIndex; frameIdx++)
                {
                    bitBmp[frameIdx].SelectActiveFrame(dimension, frameIdx);
                    for (int xf = 0; xf < bitBmp[initialFrameIndex].Width; xf++)
                    {
                        Color pixel = bitBmp[frameIdx].GetPixel(xf, yf);
                        if (pixel == _whiteConvert || pixel == customConvert || pixel.A == 0)
                        {
                            var = true;
                        }

                        if (pixel != _whiteConvert && pixel != customConvert && pixel.A != 0)
                        {
                            var = false;
                            if (yf != 0)
                            {
                                regressT++;
                                yf -= 1;
                                xf = -1;
                                frameIdx = initialFrameIndex;
                            }
                            else
                            {
                                breakOk = true;
                                break;
                            }
                        }

                        if (var && xf == bitBmp[frameIdx].Width - 1 && frameIdx == (maximumFrameIndex) - 1 && regressT == -1 &&
                            yf < bitBmp[0].Height - 9)
                        {
                            top += 10;
                        }

                        if (var && xf == bitBmp[frameIdx].Width - 1 && frameIdx == (maximumFrameIndex) - 1 && regressT == -1 &&
                            yf >= bitBmp[0].Height - 9)
                        {
                            top++;
                        }

                        if (var && xf == bitBmp[frameIdx].Width - 1 && frameIdx == (maximumFrameIndex) - 1 && regressT != -1)
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

                if (yf < bitBmp[initialFrameIndex].Height - 9)
                {
                    yf += 9; // 1 of for + 9
                }

                if (breakOk)
                {
                    breakOk = false;
                    break;
                }
            }

            // Bottom
            for (int yf = bitBmp[initialFrameIndex].Height - 1; yf > 0; yf--)
            {
                for (int frameIdx = initialFrameIndex; frameIdx < maximumFrameIndex; frameIdx++)
                {
                    bitBmp[frameIdx].SelectActiveFrame(dimension, frameIdx);
                    for (int xf = 0; xf < bitBmp[initialFrameIndex].Width; xf++)
                    {
                        Color pixel = bitBmp[frameIdx].GetPixel(xf, yf);
                        if (pixel == _whiteConvert || pixel == customConvert || pixel.A == 0)
                        {
                            var = true;
                        }

                        if (pixel != _whiteConvert && pixel != customConvert && pixel.A != 0)
                        {
                            var = false;
                            if (yf != bitBmp[initialFrameIndex].Height - 1)
                            {
                                regressB++;
                                yf += 1;
                                xf = -1;
                                frameIdx = initialFrameIndex;
                            }
                            else
                            {
                                breakOk = true;
                                break;
                            }
                        }

                        if (var && xf == bitBmp[frameIdx].Width - 1 && frameIdx == (maximumFrameIndex) - 1 && regressB == -1 &&
                            yf > 9)
                        {
                            bottom += 10;
                        }

                        if (var && xf == bitBmp[frameIdx].Width - 1 && frameIdx == (maximumFrameIndex) - 1 && regressB == -1 &&
                            yf <= 9)
                        {
                            bottom++;
                        }

                        if (var && xf == bitBmp[frameIdx].Width - 1 && frameIdx == (maximumFrameIndex) - 1 && regressB != -1)
                        {
                            bottom -= regressB;
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

            // Left
            for (int xf = 0; xf < bitBmp[initialFrameIndex].Width; xf++)
            {
                for (int frameIdx = initialFrameIndex; frameIdx < maximumFrameIndex; frameIdx++)
                {
                    bitBmp[frameIdx].SelectActiveFrame(dimension, frameIdx);
                    for (int yf = 0; yf < bitBmp[initialFrameIndex].Height; yf++)
                    {
                        Color pixel = bitBmp[frameIdx].GetPixel(xf, yf);
                        if (pixel == _whiteConvert || pixel == customConvert || pixel.A == 0)
                        {
                            var = true;
                        }

                        if (pixel != _whiteConvert && pixel != customConvert && pixel.A != 0)
                        {
                            var = false;
                            if (xf != 0)
                            {
                                regressL++;
                                xf -= 1;
                                yf = -1;
                                frameIdx = initialFrameIndex;
                            }
                            else
                            {
                                breakOk = true;
                                break;
                            }
                        }

                        if (var && yf == bitBmp[frameIdx].Height - 1 && frameIdx == (maximumFrameIndex) - 1 &&
                            regressL == -1 &&
                            xf < bitBmp[0].Width - 9)
                        {
                            left += 10;
                        }

                        if (var && yf == bitBmp[frameIdx].Height - 1 && frameIdx == (maximumFrameIndex) - 1 &&
                            regressL == -1 &&
                            xf >= bitBmp[0].Width - 9)
                        {
                            left++;
                        }

                        if (var && yf == bitBmp[frameIdx].Height - 1 && frameIdx == (maximumFrameIndex) - 1 && regressL != -1)
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

                if (xf < bitBmp[initialFrameIndex].Width - 9)
                {
                    xf += 9; // 1 of for + 9
                }

                if (breakOk)
                {
                    breakOk = false;
                    break;
                }
            }

            // Right
            for (int xf = bitBmp[initialFrameIndex].Width - 1; xf > 0; xf--)
            {
                for (int frameIdx = initialFrameIndex; frameIdx < maximumFrameIndex; frameIdx++)
                {
                    bitBmp[frameIdx].SelectActiveFrame(dimension, frameIdx);
                    for (int yf = 0; yf < bitBmp[initialFrameIndex].Height; yf++)
                    {
                        Color pixel = bitBmp[frameIdx].GetPixel(xf, yf);
                        if (pixel == _whiteConvert || pixel == customConvert || pixel.A == 0)
                        {
                            var = true;
                        }

                        if (pixel != _whiteConvert && pixel != customConvert && pixel.A != 0)
                        {
                            var = false;
                            if (xf != bitBmp[initialFrameIndex].Width - 1)
                            {
                                regressR++;
                                xf += 1;
                                yf = -1;
                                frameIdx = initialFrameIndex;
                            }
                            else
                            {
                                breakOk = true;
                                break;
                            }
                        }

                        if (var && yf == bitBmp[frameIdx].Height - 1 && frameIdx == (maximumFrameIndex) - 1 &&
                            regressR == -1 &&
                            xf > 9)
                        {
                            right += 10;
                        }

                        if (var && yf == bitBmp[frameIdx].Height - 1 && frameIdx == (maximumFrameIndex) - 1 &&
                            regressR == -1 &&
                            xf <= 9)
                        {
                            right++;
                        }

                        if (var && yf == bitBmp[frameIdx].Height - 1 && frameIdx == (maximumFrameIndex) - 1 && regressR != -1)
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

            for (int index = initialFrameIndex; index < maximumFrameIndex; index++)
            {
                Rectangle rect = new Rectangle(left, top, bitBmp[index].Width - left - right, bitBmp[index].Height - top - bottom);
                bitBmp[index].SelectActiveFrame(dimension, index);
                bitBmp[index] = bitBmp[index].Clone(rect, PixelFormat.Format16bppArgb1555);
            }
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            for (int x = 0; x < 5; x++)
            {
                //RGB
                if (radioButton1.Checked)
                {
                    AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                    edit?.PaletteConversor(1);
                }
                //RBG
                if (radioButton2.Checked)
                {
                    AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                    edit?.PaletteConversor(2);
                }
                //GRB
                if (radioButton3.Checked)
                {
                    AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                    edit?.PaletteConversor(3);
                }
                //GBR
                if (radioButton4.Checked)
                {
                    AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                    edit?.PaletteConversor(4);
                }
                //BGR
                if (radioButton5.Checked)
                {
                    AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                    edit?.PaletteConversor(5);
                }
                //BRG
                if (radioButton6.Checked)
                {
                    AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
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
                AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
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
                AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
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
                    AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                    if (edit != null)
                    {
                        // TODO: why this is called 2 times?
                        //edit.PaletteConversor(4);
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
                    AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
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
                AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
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
                    AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
                    if (edit != null)
                    {
                        // TODO: why this is called 2 times?
                        //edit.PaletteConversor(6);
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
                    AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
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
                AnimIdx edit = Ultima.AnimationEdit.GetAnimation(_fileType, _currentBody, _currentAction, _currentDir);
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
