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
            this.Icon = FiddlerControls.Options.GetFiddlerIcon();
            FileType = 0;
            CurrDir = 0;
            toolStripComboBox1.SelectedIndex = 0;
            FramePoint = new Point(pictureBox1.Width / 2, pictureBox1.Height / 2);
            ShowOnlyValid = false;
            Loaded = false;
            listView1.MultiSelect = true;
        }
        static int[] AnimCx = new int[5];
        static int[] AnimCy = new int[5];
        private bool Loaded;
        private int FileType;
        int CurrAction;
        int CurrBody;
        private int CurrDir;
        private Point FramePoint;
        private bool ShowOnlyValid;
        static bool DrawEmpty = false;
        static bool DrawFull = false;
        static Pen BlackUndraw = new Pen(Color.FromArgb(255, 0, 0, 0), 1);
        static SolidBrush WhiteUndraw = new SolidBrush(Color.FromArgb(255, 255, 255, 255));
        static SolidBrush WhiteTrasparent = new SolidBrush(Color.FromArgb(160, 255, 255, 255));
        static readonly Color WhiteConvert = Color.FromArgb(255, 255, 255, 255);
        static readonly Color GreyConvert = Color.FromArgb(255, 170, 170, 170);

        private void onLoad(object sender, EventArgs e)
        {
            Options.LoadedUltimaClass["AnimationEdit"] = true;
            treeView1.BeginUpdate();
            treeView1.Nodes.Clear();
            if (FileType != 0)
            {
                int count = Animations.GetAnimCount(FileType);
                List<TreeNode> nodes = new List<TreeNode>();
                for (int i = 0; i < count; ++i)
                {
                    int animlength = Animations.GetAnimLength(i, FileType);
                    string type = animlength == 22 ? "H" : animlength == 13 ? "L" : "P";
                    TreeNode node = new TreeNode();
                    node.Tag = i;
                    node.Text = String.Format("{0}: {1} ({2})", type, i, BodyConverter.GetTrueBody(FileType, i));
                    bool valid = false;
                    for (int j = 0; j < animlength; ++j)
                    {
                        TreeNode subnode = new TreeNode();
                        subnode.Tag = j;
                        subnode.Text = j.ToString();
                        if (Ultima.AnimationEdit.IsActionDefinied(FileType, i, j))
                            valid = true;
                        else
                            subnode.ForeColor = Color.Red;
                        node.Nodes.Add(subnode);
                    }
                    if (!valid)
                    {
                        if (ShowOnlyValid)
                            continue;
                        node.ForeColor = Color.Red;
                    }
                    nodes.Add(node);
                }
                treeView1.Nodes.AddRange(nodes.ToArray());
            }
            treeView1.EndUpdate();
            if (treeView1.Nodes.Count > 0)
                treeView1.SelectedNode = treeView1.Nodes[0];
            if (!Loaded)
                FiddlerControls.Events.FilePathChangeEvent += new FiddlerControls.Events.FilePathChangeHandler(OnFilePathChangeEvent);
            Loaded = true;
        }

        private void OnFilePathChangeEvent()
        {
            if (!Loaded)
                return;
            FileType = 0;
            CurrDir = 0;
            CurrAction = 0;
            CurrBody = 0;
            toolStripComboBox1.SelectedIndex = 0;
            FramePoint = new Point(pictureBox1.Width / 2, pictureBox1.Height / 2);
            ShowOnlyValid = false;
            showOnlyValidToolStripMenuItem.Checked = false;
            OnLoad(null);
        }

        private TreeNode GetNode(int tag)
        {
            if (ShowOnlyValid)
            {
                foreach (TreeNode node in treeView1.Nodes)
                {
                    if ((int)node.Tag == tag)
                        return node;
                }
                return null;
            }
            else
                return treeView1.Nodes[tag];
        }

        private unsafe void SetPaletteBox()
        {
            if (FileType != 0)
            {
                AnimIdx edit = Ultima.AnimationEdit.GetAnimation(FileType, CurrBody, CurrAction, CurrDir);
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
                        CurrBody = (int)treeView1.SelectedNode.Tag;
                    CurrAction = 0;
                }
                else
                {
                    if (treeView1.SelectedNode.Parent.Tag != null)
                        CurrBody = (int)treeView1.SelectedNode.Parent.Tag;
                    CurrAction = (int)treeView1.SelectedNode.Tag;
                }
                listView1.BeginUpdate();
                listView1.Clear();
                AnimIdx edit = Ultima.AnimationEdit.GetAnimation(FileType, CurrBody, CurrAction, CurrDir);
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
                                continue;
                            ListViewItem item;
                            item = new ListViewItem(i.ToString(), 0);
                            item.Tag = i;
                            listView1.Items.Add(item);
                            if (currbits[i].Width > width)
                                width = currbits[i].Width;
                            if (currbits[i].Height > height)
                                height = currbits[i].Height;
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
            AnimIdx edit = Ultima.AnimationEdit.GetAnimation(FileType, CurrBody, CurrAction, CurrDir);
            Bitmap[] currbits = edit.GetFrames();
            Bitmap bmp = currbits[(int)e.Item.Tag];
            int width = bmp.Width;
            int height = bmp.Height;

            if (listView1.SelectedItems.Contains(e.Item))
                e.Graphics.DrawRectangle(new Pen(Color.Red), e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
            else
                e.Graphics.DrawRectangle(new Pen(Color.Gray), e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
            e.Graphics.DrawImage(bmp, e.Bounds.X, e.Bounds.Y, width, height);
            e.DrawText(TextFormatFlags.Bottom | TextFormatFlags.HorizontalCenter);
        }

        private void onAnimChanged(object sender, EventArgs e)
        {
            if (toolStripComboBox1.SelectedIndex != FileType)
            {
                FileType = toolStripComboBox1.SelectedIndex;
                onLoad(this, EventArgs.Empty);
            }
        }

        private void OnDirectionChanged(object sender, EventArgs e)
        {
            CurrDir = trackBar1.Value;
            AfterSelectTreeView(null, null);
        }

        private void OnSizeChangedPictureBox(object sender, EventArgs e)
        {
            FramePoint = new Point(pictureBox1.Width / 2, pictureBox1.Height / 2);
            pictureBox1.Invalidate();
        }
        //Soulblighter Modification

        private void onPaintFrame(object sender, PaintEventArgs e)
        {
            AnimIdx edit = Ultima.AnimationEdit.GetAnimation(FileType, CurrBody, CurrAction, CurrDir);
            if (edit != null)
            {
                Bitmap[] currbits = edit.GetFrames();
                int varW = 0;
                int varH = 0;
                int varFW = 0;
                int varFH = 0;
                e.Graphics.Clear(Color.LightGray);
                e.Graphics.DrawLine(Pens.Black, new Point(FramePoint.X, 0), new Point(FramePoint.X, pictureBox1.Height));
                e.Graphics.DrawLine(Pens.Black, new Point(0, FramePoint.Y), new Point(pictureBox1.Width, FramePoint.Y));
                if ((currbits != null) && (currbits.Length > 0))
                {
                    if (currbits[trackBar2.Value] != null)
                    {
                        if (!DrawEmpty)
                        {
                            varW = 0;
                            varH = 0;
                        }
                        else
                        {
                            varW = currbits[trackBar2.Value].Width;
                            varH = currbits[trackBar2.Value].Height;
                        }
                        if (!DrawFull)
                        {
                            varFW = 0;
                            varFH = 0;
                        }
                        else
                        {
                            varFW = currbits[trackBar2.Value].Width;
                            varFH = currbits[trackBar2.Value].Height;
                        }
                        int x = FramePoint.X - edit.Frames[trackBar2.Value].Center.X;
                        int y = FramePoint.Y - edit.Frames[trackBar2.Value].Center.Y - currbits[trackBar2.Value].Height;
                        e.Graphics.FillRectangle(WhiteTrasparent, new Rectangle(x, y, varFW, varFH));
                        e.Graphics.DrawRectangle(Pens.Red, new Rectangle(x, y, varW, varH));
                        e.Graphics.DrawImage(currbits[trackBar2.Value], x, y);
                    }
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
                e.Graphics.FillPolygon(WhiteUndraw, arrayPoints);
                e.Graphics.DrawPolygon(BlackUndraw, arrayPoints);
            }
        }
        //End of Soulblighter Modification
        //Soulblighter Modification
        private void onFrameCountBarChanged(object sender, EventArgs e)
        {
            if (FileType != 0)
            {
                AnimIdx edit = Ultima.AnimationEdit.GetAnimation(FileType, CurrBody, CurrAction, CurrDir);
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
                if (FileType != 0)
                {
                    AnimIdx edit = Ultima.AnimationEdit.GetAnimation(FileType, CurrBody, CurrAction, CurrDir);
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
            catch (System.NullReferenceException) { }
        }

        private void OnCenterYValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (FileType != 0)
                {
                    AnimIdx edit = Ultima.AnimationEdit.GetAnimation(FileType, CurrBody, CurrAction, CurrDir);
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
            catch (System.NullReferenceException) { }
        }

        private void onClickExtractImages(object sender, EventArgs e)
        {
            if (FileType != 0)
            {
                ToolStripMenuItem menu = (ToolStripMenuItem)sender;
                ImageFormat format = ImageFormat.Bmp;
                if (((string)menu.Tag) == ".tiff")
                    format = ImageFormat.Tiff;
                string path = FiddlerControls.Options.OutputPath;
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
                    for (int a = 0; a < Animations.GetAnimLength(body, FileType); ++a)
                    {
                        for (int i = 0; i < 5; ++i)
                        {
                            AnimIdx edit = Ultima.AnimationEdit.GetAnimation(FileType, body, a, i);
                            if (edit != null)
                            {
                                Bitmap[] bits = edit.GetFrames();
                                if (bits != null)
                                {
                                    for (int j = 0; j < bits.Length; ++j)
                                    {
                                        string filename = String.Format("anim{5}_{0}_{1}_{2}_{3}{4}", body, a, i, j, menu.Tag, FileType);
                                        string file = Path.Combine(path, filename);
                                        Bitmap bit = new Bitmap(bits[j]);
                                        if (bit != null)
                                            bit.Save(file, format);
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
                        AnimIdx edit = Ultima.AnimationEdit.GetAnimation(FileType, body, action, i);
                        if (edit != null)
                        {
                            Bitmap[] bits = edit.GetFrames();
                            if (bits != null)
                            {
                                for (int j = 0; j < bits.Length; ++j)
                                {
                                    string filename = String.Format("anim{5}_{0}_{1}_{2}_{3}{4}", body, action, i, j, menu.Tag, FileType);
                                    string file = Path.Combine(path, filename);
                                    Bitmap bit = new Bitmap(bits[j]);
                                    if (bit != null)
                                        bit.Save(file, format);
                                    bit.Dispose();
                                }
                            }
                        }
                    }

                }
                MessageBox.Show(
                        String.Format("Frames saved to {0}", path),
                        "Saved",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information,
                        MessageBoxDefaultButton.Button1);
            }
        }

        private void OnClickRemoveAction(object sender, EventArgs e)
        {
            if (FileType != 0)
            {
                if (treeView1.SelectedNode.Parent == null)
                {
                    DialogResult result =
                           MessageBox.Show(String.Format("Are you sure to remove animation {0}", CurrBody),
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
                                AnimIdx edit = Ultima.AnimationEdit.GetAnimation(FileType, CurrBody, i, d);
                                if (edit != null)
                                    edit.ClearFrames();
                            }
                        }
                        if (ShowOnlyValid)
                            treeView1.SelectedNode.Remove();
                        Options.ChangedUltimaClass["Animations"] = true;
                        AfterSelectTreeView(this, null);
                    }
                }
                else
                {
                    DialogResult result =
                           MessageBox.Show(String.Format("Are you sure to remove action {0}", CurrAction),
                           "Remove",
                           MessageBoxButtons.YesNo,
                           MessageBoxIcon.Question,
                           MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.Yes)
                    {
                        for (int i = 0; i < 5; ++i)
                        {
                            AnimIdx edit = Ultima.AnimationEdit.GetAnimation(FileType, CurrBody, CurrAction, i);
                            if (edit != null)
                                edit.ClearFrames();
                        }
                        treeView1.SelectedNode.Parent.Nodes[CurrAction].ForeColor = Color.Red;
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
                            if (ShowOnlyValid)
                                treeView1.SelectedNode.Parent.Remove();
                            else
                                treeView1.SelectedNode.Parent.ForeColor = Color.Red;
                        }
                        Options.ChangedUltimaClass["Animations"] = true;
                        AfterSelectTreeView(this, null);
                    }
                }
            }
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            if (FileType != 0)
            {
                Ultima.AnimationEdit.Save(FileType, FiddlerControls.Options.OutputPath);
                MessageBox.Show(
                        String.Format("AnimationFile saved to {0}", FiddlerControls.Options.OutputPath),
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
                int Corrector = 0;
                int[] frameindex = new int[listView1.SelectedItems.Count];
                for (int i = 0; i < listView1.SelectedItems.Count; i++)
                {
                    frameindex[i] = (int)listView1.SelectedIndices[i] - Corrector;
                    Corrector++;
                }
                for (int i = 0; i < frameindex.Length; i++)
                {
                    AnimIdx edit = Ultima.AnimationEdit.GetAnimation(FileType, CurrBody, CurrAction, CurrDir);
                    if (edit != null)
                    {
                        edit.RemoveFrame(frameindex[i]);
                        listView1.Items.RemoveAt(listView1.Items.Count - 1);
                        if (edit.Frames.Count != 0)
                            trackBar2.Maximum = edit.Frames.Count - 1;
                        else
                        {
                            TreeNode node = GetNode(CurrBody);
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
                    dialog.Title = String.Format("Choose image file to replace at {0}", frameindex);
                    dialog.CheckFileExists = true;
                    dialog.Filter = "image files (*.tiff;*.bmp)|*.tiff;*.bmp";
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        Bitmap bmp = new Bitmap(dialog.FileName);
                        if (dialog.FileName.Contains(".bmp"))
                            bmp = Utils.ConvertBmpAnim(bmp, (int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                        AnimIdx edit = Ultima.AnimationEdit.GetAnimation(FileType, CurrBody, CurrAction, CurrDir);
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
            if (FileType != 0)
            {
                using (OpenFileDialog dialog = new OpenFileDialog())
                {
                    dialog.Multiselect = true;
                    dialog.Title = "Choose image file to add";
                    dialog.CheckFileExists = true;
                    dialog.Filter = "Gif files (*.gif;)|*.gif; |Bitmap files (*.bmp;)|*.bmp; |Tiff files (*.tiff;)|*tiff; |Png files (*.png;)|*.png; |Jpeg files (*.jpeg;*.jpg;)|*.jpeg;*.jpg;";
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        listView1.BeginUpdate();
                        //My Soulblighter Modifications
                        for (int w = 0; w < dialog.FileNames.Length; w++)
                        {
                            Bitmap bmp = new Bitmap(dialog.FileNames[w]); // dialog.Filename replaced by dialog.FileNames[w]
                            AnimIdx edit = Ultima.AnimationEdit.GetAnimation(FileType, CurrBody, CurrAction, CurrDir);
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
                                        TreeNode node = GetNode(CurrBody);
                                        if (node != null)
                                        {
                                            node.ForeColor = Color.Black;
                                            node.Nodes[CurrAction].ForeColor = Color.Black;
                                        }
                                        ListViewItem item;
                                        int i = edit.Frames.Count - 1;
                                        item = new ListViewItem(i.ToString(), 0);
                                        item.Tag = i;
                                        listView1.Items.Add(item);
                                        int width = listView1.TileSize.Width - 5;
                                        if (bmp.Width > listView1.TileSize.Width)
                                            width = bmp.Width;
                                        int height = listView1.TileSize.Height - 5;
                                        if (bmp.Height > listView1.TileSize.Height)
                                            height = bmp.Height;

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
                                    TreeNode node = GetNode(CurrBody);
                                    if (node != null)
                                    {
                                        node.ForeColor = Color.Black;
                                        node.Nodes[CurrAction].ForeColor = Color.Black;
                                    }
                                    ListViewItem item;
                                    int i = edit.Frames.Count - 1;
                                    item = new ListViewItem(i.ToString(), 0);
                                    item.Tag = i;
                                    listView1.Items.Add(item);
                                    int width = listView1.TileSize.Width - 5;
                                    if (bmp.Width > listView1.TileSize.Width)
                                        width = bmp.Width;
                                    int height = listView1.TileSize.Height - 5;
                                    if (bmp.Height > listView1.TileSize.Height)
                                        height = bmp.Height;

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
            CurrDir = trackBar1.Value;
            AfterSelectTreeView(null, null);
        }

        private void onClickExtractPalette(object sender, EventArgs e)
        {
            if (FileType != 0)
            {
                ToolStripMenuItem menu = (ToolStripMenuItem)sender;
                AnimIdx edit = Ultima.AnimationEdit.GetAnimation(FileType, CurrBody, CurrAction, CurrDir);
                if (edit != null)
                {
                    string name = String.Format("palette_anim{0}_{1}_{2}_{3}", FileType, CurrBody, CurrAction, CurrDir);
                    if (((string)menu.Tag) == "txt")
                    {
                        string path = Path.Combine(FiddlerControls.Options.OutputPath, name + ".txt");
                        edit.ExportPalette(path, 0);
                    }
                    else
                    {
                        string path = Path.Combine(FiddlerControls.Options.OutputPath, name + "." + (string)menu.Tag);
                        if (((string)menu.Tag) == "bmp")
                            edit.ExportPalette(path, 1);
                        else
                            edit.ExportPalette(path, 2);
                    }
                    MessageBox.Show(
                        String.Format("Palette saved to {0}", FiddlerControls.Options.OutputPath),
                        "Saved",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information,
                        MessageBoxDefaultButton.Button1);
                }
            }
        }

        private void onClickImportPalette(object sender, EventArgs e)
        {
            if (FileType != 0)
            {
                using (OpenFileDialog dialog = new OpenFileDialog())
                {
                    dialog.Multiselect = false;
                    dialog.Title = "Choose palette file";
                    dialog.CheckFileExists = true;
                    dialog.Filter = "txt files (*.txt)|*.txt";
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        AnimIdx edit = Ultima.AnimationEdit.GetAnimation(FileType, CurrBody, CurrAction, CurrDir);
                        if (edit != null)
                        {
                            using (StreamReader sr = new StreamReader(dialog.FileName))
                            {
                                string line;
                                ushort[] Palette = new ushort[0x100];
                                int i = 0;
                                while ((line = sr.ReadLine()) != null)
                                {
                                    if ((line = line.Trim()).Length == 0 || line.StartsWith("#"))
                                        continue;
                                    Palette[i++] = ushort.Parse(line);
                                    //My Soulblighter Modification
                                    if (Palette[i++] == 32768)
                                        Palette[i++] = 32769;
                                    //End of Soulblighter Modification
                                    if (i >= 0x100)
                                        break;
                                }
                                edit.ReplacePalette(Palette);
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
            if (FileType != 0)
            {
                using (OpenFileDialog dialog = new OpenFileDialog())
                {
                    dialog.Multiselect = false;
                    dialog.Title = "Choose palette file";
                    dialog.CheckFileExists = true;
                    dialog.Filter = "vd files (*.vd)|*.vd";
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        int animlength = Animations.GetAnimLength(CurrBody, FileType);
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
                                Ultima.AnimationEdit.LoadFromVD(FileType, CurrBody, bin);
                            }
                        }

                        bool valid = false;
                        TreeNode node = GetNode(CurrBody);
                        if (node != null)
                        {
                            for (int j = 0; j < animlength; ++j)
                            {
                                if (Ultima.AnimationEdit.IsActionDefinied(FileType, CurrBody, j))
                                {
                                    node.Nodes[j].ForeColor = Color.Black;
                                    valid = true;
                                }
                                else
                                    node.Nodes[j].ForeColor = Color.Red;
                            }
                            if (valid)
                                node.ForeColor = Color.Black;
                            else
                                node.ForeColor = Color.Red;
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
            if (FileType != 0)
            {
                string path = FiddlerControls.Options.OutputPath;
                string FileName = Path.Combine(path, String.Format("anim{0}_0x{1:X}.vd", FileType, CurrBody));
                Ultima.AnimationEdit.ExportToVD(FileType, CurrBody, FileName);
                MessageBox.Show(
                        String.Format("Animation saved to {0}", FiddlerControls.Options.OutputPath),
                        "Export",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information,
                        MessageBoxDefaultButton.Button1);
            }
        }

        private void OnClickShowOnlyValid(object sender, EventArgs e)
        {
            ShowOnlyValid = !ShowOnlyValid;
            if (ShowOnlyValid)
            {
                treeView1.BeginUpdate();
                for (int i = treeView1.Nodes.Count - 1; i >= 0; --i)
                {
                    if (treeView1.Nodes[i].ForeColor == Color.Red)
                        treeView1.Nodes[i].Remove();
                }
                treeView1.EndUpdate();
            }
            else
                OnLoad(null);
        }

        private void toolStripLabel2_Click(object sender, EventArgs e)
        {

        }
        //My Soulblighter Modification
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (FileType != 0)
                {
                    AnimIdx edit = Ultima.AnimationEdit.GetAnimation(FileType, CurrBody, CurrAction, CurrDir);
                    if (edit != null)
                    {
                        if (edit.Frames.Count >= trackBar2.Value)
                        {
                            FrameEdit[] frame = new FrameEdit[edit.Frames.Count];
                            for (int Index = 0; Index < edit.Frames.Count; Index++)
                            {
                                frame[Index] = edit.Frames[Index];
                                frame[Index].ChangeCenter((int)numericUpDownCx.Value, (int)numericUpDownCy.Value);
                                Options.ChangedUltimaClass["Animations"] = true;
                                pictureBox1.Invalidate();
                            }
                        }
                    }
                }
            }
            catch (System.NullReferenceException) { }
        }
        //End of Soulblighter Modification

        //My Soulblighter Modification
        private void fromGifToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FileType != 0)
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
                        AnimIdx edit = Ultima.AnimationEdit.GetAnimation(FileType, CurrBody, CurrAction, CurrDir);
                        if (edit != null)
                        {
                            FrameDimension dimension = new FrameDimension(bit.FrameDimensionsList[0]);
                            // Number of frames 
                            int frameCount = bit.GetFrameCount(dimension);
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

        static bool lockbutton = false; //Lock button variable
        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (!lockbutton && toolStripButton6.Enabled)
            {
                numericUpDown2.Value = 418 - e.X;
                numericUpDown1.Value = 335 - e.Y;
                pictureBox1.Invalidate();
            }
        }
        //Change center of frame on key press
        private void txtSendData_KeyDown(object sender, KeyEventArgs e)
        {
            if (timer1.Enabled == false)
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
        private void txtSendData_KeyDown2(object sender, KeyEventArgs e)
        {
            if (lockbutton == false && toolStripButton6.Enabled == true)
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
        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            lockbutton = !lockbutton;
            numericUpDown2.Enabled = !lockbutton;
            numericUpDown1.Enabled = !lockbutton;
        }
        //Add in all Directions
        private void allDirectionsAddToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FileType != 0)
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
                                    AnimIdx edit = Ultima.AnimationEdit.GetAnimation(FileType, CurrBody, CurrAction, CurrDir);
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
                                                TreeNode node = GetNode(CurrBody);
                                                if (node != null)
                                                {
                                                    node.ForeColor = Color.Black;
                                                    node.Nodes[CurrAction].ForeColor = Color.Black;
                                                }
                                                ListViewItem item;
                                                int i = edit.Frames.Count - 1;
                                                item = new ListViewItem(i.ToString(), 0);
                                                item.Tag = i;
                                                listView1.Items.Add(item);
                                                int width = listView1.TileSize.Width - 5;
                                                if (bmp.Width > listView1.TileSize.Width)
                                                    width = bmp.Width;
                                                int height = listView1.TileSize.Height - 5;
                                                if (bmp.Height > listView1.TileSize.Height)
                                                    height = bmp.Height;

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
                                        trackBar1.Value++;
                                }
                            }
                        }
                        //Looping if dialog.FileNames.Length != 5
                        while (dialog.FileNames.Length != 5)
                        {
                            if (dialog.ShowDialog() == DialogResult.Cancel)
                                break;
                            if (dialog.FileNames.Length != 5)
                                dialog.ShowDialog();
                            if (dialog.FileNames.Length == 5)
                            {
                                trackBar1.Value = 0;
                                for (int w = 0; w < dialog.FileNames.Length; w++)
                                {
                                    if (w < 5)
                                    {
                                        Bitmap bmp = new Bitmap(dialog.FileNames[w]); // dialog.Filename replaced by dialog.FileNames[w]
                                        AnimIdx edit = Ultima.AnimationEdit.GetAnimation(FileType, CurrBody, CurrAction, CurrDir);
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
                                                    TreeNode node = GetNode(CurrBody);
                                                    if (node != null)
                                                    {
                                                        node.ForeColor = Color.Black;
                                                        node.Nodes[CurrAction].ForeColor = Color.Black;
                                                    }
                                                    ListViewItem item;
                                                    int i = edit.Frames.Count - 1;
                                                    item = new ListViewItem(i.ToString(), 0);
                                                    item.Tag = i;
                                                    listView1.Items.Add(item);
                                                    int width = listView1.TileSize.Width - 5;
                                                    if (bmp.Width > listView1.TileSize.Width)
                                                        width = bmp.Width;
                                                    int height = listView1.TileSize.Height - 5;
                                                    if (bmp.Height > listView1.TileSize.Height)
                                                        height = bmp.Height;

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
                                            trackBar1.Value++;
                                    }
                                }
                            }
                        }
                        trackBar1.Enabled = true;
                    }
                }
            }
            //Refresh List
            CurrDir = trackBar1.Value;
            AfterSelectTreeView(null, null);
        }

        private void toolStripButton10_Click(object sender, EventArgs e)
        {
            DrawEmpty = !DrawEmpty;
            pictureBox1.Invalidate();
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            DrawFull = !DrawFull;
            pictureBox1.Invalidate();
        }
        //Closing window
        private void AnimationEdit_FormClosing(object sender, FormClosingEventArgs e)
        {
            DrawFull = false;
            DrawEmpty = false;
            lockbutton = false;
            timer1.Enabled = false;
            BlackUndraw = new Pen(Color.FromArgb(255, 0, 0, 0), 1);
            WhiteUndraw = new SolidBrush(Color.FromArgb(255, 255, 255, 255));
            Loaded = false;
            FiddlerControls.Events.FilePathChangeEvent -= new FiddlerControls.Events.FilePathChangeHandler(OnFilePathChangeEvent);
        }
        //Play Button Timer
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (trackBar2.Value < trackBar2.Maximum)
                trackBar2.Value++;
            else
                trackBar2.Value = 0;
            pictureBox1.Invalidate();
        }
        //Play Button
        private void toolStripButton11_Click(object sender, EventArgs e)
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
                    BlackUndraw = new Pen(Color.FromArgb(0, 0, 0, 0), 1);
                    WhiteUndraw = new SolidBrush(Color.FromArgb(0, 255, 255, 255));
                }
                else
                {
                    toolStripButton6.Enabled = true;
                    BlackUndraw = new Pen(Color.FromArgb(255, 0, 0, 0), 1);
                    WhiteUndraw = new SolidBrush(Color.FromArgb(255, 255, 255, 255));
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
                BlackUndraw = new Pen(Color.FromArgb(0, 0, 0, 0), 1);
                WhiteUndraw = new SolidBrush(Color.FromArgb(0, 255, 255, 255));
            }
            pictureBox1.Invalidate();
        }
        //Animation Speed
        private void trackBar3_ValueChanged(object sender, EventArgs e)
        {
            timer1.Interval = 50 + (trackBar3.Value * 30);
        }

        private void toolStripButton12_Click(object sender, EventArgs e)
        {
            if (!toolStripButton12.Checked)
            {
                BlackUndraw = new Pen(Color.FromArgb(255, 0, 0, 0), 1);
                WhiteUndraw = new SolidBrush(Color.FromArgb(255, 255, 255, 255));
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
                BlackUndraw = new Pen(Color.FromArgb(0, 0, 0, 0), 1);
                WhiteUndraw = new SolidBrush(Color.FromArgb(0, 255, 255, 255));
                toolStripButton6.Enabled = false;
                numericUpDown2.Enabled = false;
                numericUpDown1.Enabled = false;
            }
            pictureBox1.Invalidate();
        }
        //All Directions with Canvas
        private void allDirectionsAddWithCanvasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (FileType != 0)
                {
                    using (OpenFileDialog dialog = new OpenFileDialog())
                    {
                        dialog.Multiselect = true;
                        dialog.Title = "Choose 5 Gifs to add";
                        dialog.CheckFileExists = true;
                        dialog.Filter = "Gif files (*.gif;)|*.gif;";
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            Color CustomConvert = Color.FromArgb(255, (int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                            trackBar1.Enabled = false;
                            if (dialog.FileNames.Length == 5)
                            {
                                trackBar1.Value = 0;
                                for (int w = 0; w < dialog.FileNames.Length; w++)
                                {
                                    if (w < 5)
                                    {
                                        Bitmap bmp = new Bitmap(dialog.FileNames[w]); // dialog.Filename replaced by dialog.FileNames[w]
                                        AnimIdx edit = Ultima.AnimationEdit.GetAnimation(FileType, CurrBody, CurrAction, CurrDir);
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
                                                int RegressT = -1;
                                                int RegressB = -1;
                                                int RegressL = -1;
                                                int RegressR = -1;
                                                bool var = true;
                                                bool breakOK = false;
                                                //Top
                                                for (int Yf = 0; Yf < bitbmp[0].Height; Yf++)
                                                {
                                                    for (int fram = 0; fram < frameCount; fram++)
                                                    {
                                                        bitbmp[fram].SelectActiveFrame(dimension, fram);
                                                        for (int Xf = 0; Xf < bitbmp[0].Width; Xf++)
                                                        {
                                                            Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                            if (pixel == WhiteConvert | pixel == CustomConvert | pixel.A == 0)
                                                                var = true;
                                                            if (pixel != WhiteConvert & pixel != CustomConvert & pixel.A != 0)
                                                            {
                                                                var = false;
                                                                if (Yf != 0)
                                                                {
                                                                    RegressT++;
                                                                    Yf -= 1;
                                                                    Xf = -1;
                                                                    fram = 0;
                                                                }
                                                                else
                                                                {
                                                                    breakOK = true;
                                                                    break;
                                                                }
                                                            }
                                                            if (var == true && Xf == bitbmp[fram].Width - 1 && fram == frameCount - 1 && RegressT == -1 && Yf < bitbmp[0].Height - 9)
                                                                top += 10;
                                                            if (var == true && Xf == bitbmp[fram].Width - 1 && fram == frameCount - 1 && RegressT == -1 && Yf >= bitbmp[0].Height - 9)
                                                                top++;
                                                            if (var == true && Xf == bitbmp[fram].Width - 1 && fram == frameCount - 1 && RegressT != -1)
                                                            {
                                                                top = top - RegressT;
                                                                breakOK = true;
                                                                break;
                                                            }
                                                        }
                                                        if (breakOK == true)
                                                            break;
                                                    }
                                                    if (Yf < bitbmp[0].Height - 9)
                                                        Yf += 9; // 1 of for + 9
                                                    if (breakOK == true)
                                                    {
                                                        breakOK = false;
                                                        break;
                                                    }
                                                }
                                                //Bot
                                                for (int Yf = bitbmp[0].Height - 1; Yf > 0; Yf--)
                                                {
                                                    for (int fram = 0; fram < frameCount; fram++)
                                                    {
                                                        bitbmp[fram].SelectActiveFrame(dimension, fram);
                                                        for (int Xf = 0; Xf < bitbmp[0].Width; Xf++)
                                                        {
                                                            Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                            if (pixel == WhiteConvert | pixel == CustomConvert | pixel.A == 0)
                                                                var = true;
                                                            if (pixel != WhiteConvert & pixel != CustomConvert & pixel.A != 0)
                                                            {
                                                                var = false;
                                                                if (Yf != bitbmp[0].Height - 1)
                                                                {
                                                                    RegressB++;
                                                                    Yf += 1;
                                                                    Xf = -1;
                                                                    fram = 0;
                                                                }
                                                                else
                                                                {
                                                                    breakOK = true;
                                                                    break;
                                                                }
                                                            }
                                                            if (var == true && Xf == bitbmp[fram].Width - 1 && fram == frameCount - 1 && RegressB == -1 && Yf > 9)
                                                                bot += 10;
                                                            if (var == true && Xf == bitbmp[fram].Width - 1 && fram == frameCount - 1 && RegressB == -1 && Yf <= 9)
                                                                bot++;
                                                            if (var == true && Xf == bitbmp[fram].Width - 1 && fram == frameCount - 1 && RegressB != -1)
                                                            {
                                                                bot = bot - RegressB;
                                                                breakOK = true;
                                                                break;
                                                            }
                                                        }
                                                        if (breakOK == true)
                                                            break;
                                                    }
                                                    if (Yf > 9)
                                                        Yf -= 9; // 1 of for + 9
                                                    if (breakOK == true)
                                                    {
                                                        breakOK = false;
                                                        break;
                                                    }
                                                }
                                                //Left

                                                for (int Xf = 0; Xf < bitbmp[0].Width; Xf++)
                                                {
                                                    for (int fram = 0; fram < frameCount; fram++)
                                                    {
                                                        bitbmp[fram].SelectActiveFrame(dimension, fram);
                                                        for (int Yf = 0; Yf < bitbmp[0].Height; Yf++)
                                                        {
                                                            Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                            if (pixel == WhiteConvert | pixel == CustomConvert | pixel.A == 0)
                                                                var = true;
                                                            if (pixel != WhiteConvert & pixel != CustomConvert & pixel.A != 0)
                                                            {
                                                                var = false;
                                                                if (Xf != 0)
                                                                {
                                                                    RegressL++;
                                                                    Xf -= 1;
                                                                    Yf = -1;
                                                                    fram = 0;
                                                                }
                                                                else
                                                                {
                                                                    breakOK = true;
                                                                    break;
                                                                }
                                                            }
                                                            if (var == true && Yf == bitbmp[fram].Height - 1 && fram == frameCount - 1 && RegressL == -1 && Xf < bitbmp[0].Width - 9)
                                                                left += 10;
                                                            if (var == true && Yf == bitbmp[fram].Height - 1 && fram == frameCount - 1 && RegressL == -1 && Xf >= bitbmp[0].Width - 9)
                                                                left++;
                                                            if (var == true && Yf == bitbmp[fram].Height - 1 && fram == frameCount - 1 && RegressL != -1)
                                                            {
                                                                left = left - RegressL;
                                                                breakOK = true;
                                                                break;
                                                            }
                                                        }
                                                        if (breakOK == true)
                                                            break;
                                                    }
                                                    if (Xf < bitbmp[0].Width - 9)
                                                        Xf += 9; // 1 of for + 9
                                                    if (breakOK == true)
                                                    {
                                                        breakOK = false;
                                                        break;
                                                    }
                                                }
                                                //Right
                                                for (int Xf = bitbmp[0].Width - 1; Xf > 0; Xf--)
                                                {
                                                    for (int fram = 0; fram < frameCount; fram++)
                                                    {
                                                        bitbmp[fram].SelectActiveFrame(dimension, fram);
                                                        for (int Yf = 0; Yf < bitbmp[0].Height; Yf++)
                                                        {
                                                            Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                            if (pixel == WhiteConvert | pixel == CustomConvert | pixel.A == 0)
                                                                var = true;
                                                            if (pixel != WhiteConvert & pixel != CustomConvert & pixel.A != 0)
                                                            {
                                                                var = false;
                                                                if (Xf != bitbmp[0].Width - 1)
                                                                {
                                                                    RegressR++;
                                                                    Xf += 1;
                                                                    Yf = -1;
                                                                    fram = 0;
                                                                }
                                                                else
                                                                {
                                                                    breakOK = true;
                                                                    break;
                                                                }
                                                            }
                                                            if (var == true && Yf == bitbmp[fram].Height - 1 && fram == frameCount - 1 && RegressR == -1 && Xf > 9)
                                                                right += 10;
                                                            if (var == true && Yf == bitbmp[fram].Height - 1 && fram == frameCount - 1 && RegressR == -1 && Xf <= 9)
                                                                right++;
                                                            if (var == true && Yf == bitbmp[fram].Height - 1 && fram == frameCount - 1 && RegressR != -1)
                                                            {
                                                                right = right - RegressR;
                                                                breakOK = true;
                                                                break;
                                                            }
                                                        }
                                                        if (breakOK == true)
                                                            break;
                                                    }
                                                    if (Xf > 9)
                                                        Xf -= 9; // 1 of for + 9
                                                    if (breakOK == true)
                                                    {
                                                        breakOK = false;
                                                        break;
                                                    }
                                                }

                                                for (int index = 0; index < frameCount; index++)
                                                {
                                                    Rectangle rect = new Rectangle(left, top, (bitbmp[index].Width - left - right), (bitbmp[index].Height - top - bot));
                                                    bitbmp[index].SelectActiveFrame(dimension, index);
                                                    Bitmap myImage2 = bitbmp[index].Clone(rect, System.Drawing.Imaging.PixelFormat.Format16bppArgb1555);
                                                    bitbmp[index] = myImage2;
                                                }

                                                //End of Canvas
                                                for (int index = 0; index < frameCount; index++)
                                                {
                                                    bitbmp[index].SelectActiveFrame(dimension, index);
                                                    bitbmp[index] = Utils.ConvertBmpAnim(bitbmp[index], (int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                                                    edit.AddFrame(bitbmp[index]);
                                                    TreeNode node = GetNode(CurrBody);
                                                    if (node != null)
                                                    {
                                                        node.ForeColor = Color.Black;
                                                        node.Nodes[CurrAction].ForeColor = Color.Black;
                                                    }
                                                    ListViewItem item;
                                                    int i = edit.Frames.Count - 1;
                                                    item = new ListViewItem(i.ToString(), 0);
                                                    item.Tag = i;
                                                    listView1.Items.Add(item);
                                                    int width = listView1.TileSize.Width - 5;
                                                    if (bmp.Width > listView1.TileSize.Width)
                                                        width = bmp.Width;
                                                    int height = listView1.TileSize.Height - 5;
                                                    if (bmp.Height > listView1.TileSize.Height)
                                                        height = bmp.Height;

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
                                            trackBar1.Value++;
                                    }
                                }
                            }
                            //Looping if dialog.FileNames.Length != 5
                            while (dialog.FileNames.Length != 5)
                            {
                                if (dialog.ShowDialog() == DialogResult.Cancel)
                                    break;
                                if (dialog.FileNames.Length != 5)
                                    dialog.ShowDialog();
                                if (dialog.FileNames.Length == 5)
                                {
                                    trackBar1.Value = 0;
                                    for (int w = 0; w < dialog.FileNames.Length; w++)
                                    {
                                        if (w < 5)
                                        {
                                            Bitmap bmp = new Bitmap(dialog.FileNames[w]); // dialog.Filename replaced by dialog.FileNames[w]
                                            AnimIdx edit = Ultima.AnimationEdit.GetAnimation(FileType, CurrBody, CurrAction, CurrDir);
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
                                                    int RegressT = -1;
                                                    int RegressB = -1;
                                                    int RegressL = -1;
                                                    int RegressR = -1;
                                                    bool var = true;
                                                    bool breakOK = false;
                                                    //Top
                                                    for (int Yf = 0; Yf < bitbmp[0].Height; Yf++)
                                                    {
                                                        for (int fram = 0; fram < frameCount; fram++)
                                                        {
                                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                                            for (int Xf = 0; Xf < bitbmp[0].Width; Xf++)
                                                            {
                                                                Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                                if (pixel == WhiteConvert | pixel == CustomConvert | pixel.A == 0)
                                                                    var = true;
                                                                if (pixel != WhiteConvert & pixel != CustomConvert & pixel.A != 0)
                                                                {
                                                                    var = false;
                                                                    if (Yf != 0)
                                                                    {
                                                                        RegressT++;
                                                                        Yf -= 1;
                                                                        Xf = -1;
                                                                        fram = 0;
                                                                    }
                                                                    else
                                                                    {
                                                                        breakOK = true;
                                                                        break;
                                                                    }
                                                                }
                                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == frameCount - 1 && RegressT == -1 && Yf < bitbmp[0].Height - 9)
                                                                    top += 10;
                                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == frameCount - 1 && RegressT == -1 && Yf >= bitbmp[0].Height - 9)
                                                                    top++;
                                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == frameCount - 1 && RegressT != -1)
                                                                {
                                                                    top = top - RegressT;
                                                                    breakOK = true;
                                                                    break;
                                                                }
                                                            }
                                                            if (breakOK == true)
                                                                break;
                                                        }
                                                        if (Yf < bitbmp[0].Height - 9)
                                                            Yf += 9; // 1 of for + 9
                                                        if (breakOK == true)
                                                        {
                                                            breakOK = false;
                                                            break;
                                                        }
                                                    }
                                                    //Bot
                                                    for (int Yf = bitbmp[0].Height - 1; Yf > 0; Yf--)
                                                    {
                                                        for (int fram = 0; fram < frameCount; fram++)
                                                        {
                                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                                            for (int Xf = 0; Xf < bitbmp[0].Width; Xf++)
                                                            {
                                                                Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                                if (pixel == WhiteConvert | pixel == CustomConvert | pixel.A == 0)
                                                                    var = true;
                                                                if (pixel != WhiteConvert & pixel != CustomConvert & pixel.A != 0)
                                                                {
                                                                    var = false;
                                                                    if (Yf != bitbmp[0].Height - 1)
                                                                    {
                                                                        RegressB++;
                                                                        Yf += 1;
                                                                        Xf = -1;
                                                                        fram = 0;
                                                                    }
                                                                    else
                                                                    {
                                                                        breakOK = true;
                                                                        break;
                                                                    }
                                                                }
                                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == frameCount - 1 && RegressB == -1 && Yf > 9)
                                                                    bot += 10;
                                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == frameCount - 1 && RegressB == -1 && Yf <= 9)
                                                                    bot++;
                                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == frameCount - 1 && RegressB != -1)
                                                                {
                                                                    bot = bot - RegressB;
                                                                    breakOK = true;
                                                                    break;
                                                                }
                                                            }
                                                            if (breakOK == true)
                                                                break;
                                                        }
                                                        if (Yf > 9)
                                                            Yf -= 9; // 1 of for + 9
                                                        if (breakOK == true)
                                                        {
                                                            breakOK = false;
                                                            break;
                                                        }
                                                    }
                                                    //Left

                                                    for (int Xf = 0; Xf < bitbmp[0].Width; Xf++)
                                                    {
                                                        for (int fram = 0; fram < frameCount; fram++)
                                                        {
                                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                                            for (int Yf = 0; Yf < bitbmp[0].Height; Yf++)
                                                            {
                                                                Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                                if (pixel == WhiteConvert | pixel == CustomConvert | pixel.A == 0)
                                                                    var = true;
                                                                if (pixel != WhiteConvert & pixel != CustomConvert & pixel.A != 0)
                                                                {
                                                                    var = false;
                                                                    if (Xf != 0)
                                                                    {
                                                                        RegressL++;
                                                                        Xf -= 1;
                                                                        Yf = -1;
                                                                        fram = 0;
                                                                    }
                                                                    else
                                                                    {
                                                                        breakOK = true;
                                                                        break;
                                                                    }
                                                                }
                                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == frameCount - 1 && RegressL == -1 && Xf < bitbmp[0].Width - 9)
                                                                    left += 10;
                                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == frameCount - 1 && RegressL == -1 && Xf >= bitbmp[0].Width - 9)
                                                                    left++;
                                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == frameCount - 1 && RegressL != -1)
                                                                {
                                                                    left = left - RegressL;
                                                                    breakOK = true;
                                                                    break;
                                                                }
                                                            }
                                                            if (breakOK == true)
                                                                break;
                                                        }
                                                        if (Xf < bitbmp[0].Width - 9)
                                                            Xf += 9; // 1 of for + 9
                                                        if (breakOK == true)
                                                        {
                                                            breakOK = false;
                                                            break;
                                                        }
                                                    }
                                                    //Right
                                                    for (int Xf = bitbmp[0].Width - 1; Xf > 0; Xf--)
                                                    {
                                                        for (int fram = 0; fram < frameCount; fram++)
                                                        {
                                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                                            for (int Yf = 0; Yf < bitbmp[0].Height; Yf++)
                                                            {
                                                                Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                                if (pixel == WhiteConvert | pixel == CustomConvert | pixel.A == 0)
                                                                    var = true;
                                                                if (pixel != WhiteConvert & pixel != CustomConvert & pixel.A != 0)
                                                                {
                                                                    var = false;
                                                                    if (Xf != bitbmp[0].Width - 1)
                                                                    {
                                                                        RegressR++;
                                                                        Xf += 1;
                                                                        Yf = -1;
                                                                        fram = 0;
                                                                    }
                                                                    else
                                                                    {
                                                                        breakOK = true;
                                                                        break;
                                                                    }
                                                                }
                                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == frameCount - 1 && RegressR == -1 && Xf > 9)
                                                                    right += 10;
                                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == frameCount - 1 && RegressR == -1 && Xf <= 9)
                                                                    right++;
                                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == frameCount - 1 && RegressR != -1)
                                                                {
                                                                    right = right - RegressR;
                                                                    breakOK = true;
                                                                    break;
                                                                }
                                                            }
                                                            if (breakOK == true)
                                                                break;
                                                        }
                                                        if (Xf > 9)
                                                            Xf -= 9; // 1 of for + 9
                                                        if (breakOK == true)
                                                        {
                                                            breakOK = false;
                                                            break;
                                                        }
                                                    }

                                                    for (int index = 0; index < frameCount; index++)
                                                    {
                                                        Rectangle rect = new Rectangle(left, top, (bitbmp[index].Width - left - right), (bitbmp[index].Height - top - bot));
                                                        bitbmp[index].SelectActiveFrame(dimension, index);
                                                        Bitmap myImage2 = bitbmp[index].Clone(rect, System.Drawing.Imaging.PixelFormat.Format16bppArgb1555);
                                                        bitbmp[index] = myImage2;
                                                    }

                                                    //End of Canvas
                                                    for (int index = 0; index < frameCount; index++)
                                                    {
                                                        bitbmp[index].SelectActiveFrame(dimension, index);
                                                        bitbmp[index] = Utils.ConvertBmpAnim(bitbmp[index], (int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                                                        edit.AddFrame(bitbmp[index]);
                                                        TreeNode node = GetNode(CurrBody);
                                                        if (node != null)
                                                        {
                                                            node.ForeColor = Color.Black;
                                                            node.Nodes[CurrAction].ForeColor = Color.Black;
                                                        }
                                                        ListViewItem item;
                                                        int i = edit.Frames.Count - 1;
                                                        item = new ListViewItem(i.ToString(), 0);
                                                        item.Tag = i;
                                                        listView1.Items.Add(item);
                                                        int width = listView1.TileSize.Width - 5;
                                                        if (bmp.Width > listView1.TileSize.Width)
                                                            width = bmp.Width;
                                                        int height = listView1.TileSize.Height - 5;
                                                        if (bmp.Height > listView1.TileSize.Height)
                                                            height = bmp.Height;

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
                                                trackBar1.Value++;
                                        }
                                    }
                                }
                            }
                            trackBar1.Enabled = true;
                        }
                    }
                }
                //Refresh List after Canvas reduction
                CurrDir = trackBar1.Value;
                AfterSelectTreeView(null, null);
            }
            catch (System.OutOfMemoryException) { }
        }
        //Add with Canvas
        private void addWithCanvasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (FileType != 0)
                {
                    using (OpenFileDialog dialog = new OpenFileDialog())
                    {
                        dialog.Multiselect = false;
                        dialog.Title = "Choose image file to add";
                        dialog.CheckFileExists = true;
                        dialog.Filter = "Gif files (*.gif;)|*.gif;";
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            Color CustomConvert = Color.FromArgb(255, (int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                            //My Soulblighter Modifications
                            for (int w = 0; w < dialog.FileNames.Length; w++)
                            {
                                Bitmap bmp = new Bitmap(dialog.FileNames[w]); // dialog.Filename replaced by dialog.FileNames[w]
                                AnimIdx edit = Ultima.AnimationEdit.GetAnimation(FileType, CurrBody, CurrAction, CurrDir);
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
                                        int RegressT = -1;
                                        int RegressB = -1;
                                        int RegressL = -1;
                                        int RegressR = -1;
                                        bool var = true;
                                        bool breakOK = false;
                                        //Top
                                        for (int Yf = 0; Yf < bitbmp[0].Height; Yf++)
                                        {
                                            for (int fram = 0; fram < frameCount; fram++)
                                            {
                                                bitbmp[fram].SelectActiveFrame(dimension, fram);
                                                for (int Xf = 0; Xf < bitbmp[0].Width; Xf++)
                                                {
                                                    Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                    if (pixel == WhiteConvert | pixel == CustomConvert | pixel.A == 0)
                                                        var = true;
                                                    if (pixel != WhiteConvert & pixel != CustomConvert & pixel.A != 0)
                                                    {
                                                        var = false;
                                                        if (Yf != 0)
                                                        {
                                                            RegressT++;
                                                            Yf -= 1;
                                                            Xf = -1;
                                                            fram = 0;
                                                        }
                                                        else
                                                        {
                                                            breakOK = true;
                                                            break;
                                                        }
                                                    }
                                                    if (var == true && Xf == bitbmp[fram].Width - 1 && fram == frameCount - 1 && RegressT == -1 && Yf < bitbmp[0].Height - 9)
                                                        top += 10;
                                                    if (var == true && Xf == bitbmp[fram].Width - 1 && fram == frameCount - 1 && RegressT == -1 && Yf >= bitbmp[0].Height - 9)
                                                        top++;
                                                    if (var == true && Xf == bitbmp[fram].Width - 1 && fram == frameCount - 1 && RegressT != -1)
                                                    {
                                                        top = top - RegressT;
                                                        breakOK = true;
                                                        break;
                                                    }
                                                }
                                                if (breakOK == true)
                                                    break;
                                            }
                                            if (Yf < bitbmp[0].Height - 9)
                                                Yf += 9; // 1 of for + 9
                                            if (breakOK == true)
                                            {
                                                breakOK = false;
                                                break;
                                            }
                                        }
                                        //Bot
                                        for (int Yf = bitbmp[0].Height - 1; Yf > 0; Yf--)
                                        {
                                            for (int fram = 0; fram < frameCount; fram++)
                                            {
                                                bitbmp[fram].SelectActiveFrame(dimension, fram);
                                                for (int Xf = 0; Xf < bitbmp[0].Width; Xf++)
                                                {
                                                    Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                    if (pixel == WhiteConvert | pixel == CustomConvert | pixel.A == 0)
                                                        var = true;
                                                    if (pixel != WhiteConvert & pixel != CustomConvert & pixel.A != 0)
                                                    {
                                                        var = false;
                                                        if (Yf != bitbmp[0].Height - 1)
                                                        {
                                                            RegressB++;
                                                            Yf += 1;
                                                            Xf = -1;
                                                            fram = 0;
                                                        }
                                                        else
                                                        {
                                                            breakOK = true;
                                                            break;
                                                        }
                                                    }
                                                    if (var == true && Xf == bitbmp[fram].Width - 1 && fram == frameCount - 1 && RegressB == -1 && Yf > 9)
                                                        bot += 10;
                                                    if (var == true && Xf == bitbmp[fram].Width - 1 && fram == frameCount - 1 && RegressB == -1 && Yf <= 9)
                                                        bot++;
                                                    if (var == true && Xf == bitbmp[fram].Width - 1 && fram == frameCount - 1 && RegressB != -1)
                                                    {
                                                        bot = bot - RegressB;
                                                        breakOK = true;
                                                        break;
                                                    }
                                                }
                                                if (breakOK == true)
                                                    break;
                                            }
                                            if (Yf > 9)
                                                Yf -= 9; // 1 of for + 9
                                            if (breakOK == true)
                                            {
                                                breakOK = false;
                                                break;
                                            }
                                        }
                                        //Left

                                        for (int Xf = 0; Xf < bitbmp[0].Width; Xf++)
                                        {
                                            for (int fram = 0; fram < frameCount; fram++)
                                            {
                                                bitbmp[fram].SelectActiveFrame(dimension, fram);
                                                for (int Yf = 0; Yf < bitbmp[0].Height; Yf++)
                                                {
                                                    Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                    if (pixel == WhiteConvert | pixel == CustomConvert | pixel.A == 0)
                                                        var = true;
                                                    if (pixel != WhiteConvert & pixel != CustomConvert & pixel.A != 0)
                                                    {
                                                        var = false;
                                                        if (Xf != 0)
                                                        {
                                                            RegressL++;
                                                            Xf -= 1;
                                                            Yf = -1;
                                                            fram = 0;
                                                        }
                                                        else
                                                        {
                                                            breakOK = true;
                                                            break;
                                                        }
                                                    }
                                                    if (var == true && Yf == bitbmp[fram].Height - 1 && fram == frameCount - 1 && RegressL == -1 && Xf < bitbmp[0].Width - 9)
                                                        left += 10;
                                                    if (var == true && Yf == bitbmp[fram].Height - 1 && fram == frameCount - 1 && RegressL == -1 && Xf >= bitbmp[0].Width - 9)
                                                        left++;
                                                    if (var == true && Yf == bitbmp[fram].Height - 1 && fram == frameCount - 1 && RegressL != -1)
                                                    {
                                                        left = left - RegressL;
                                                        breakOK = true;
                                                        break;
                                                    }
                                                }
                                                if (breakOK == true)
                                                    break;
                                            }
                                            if (Xf < bitbmp[0].Width - 9)
                                                Xf += 9; // 1 of for + 9
                                            if (breakOK == true)
                                            {
                                                breakOK = false;
                                                break;
                                            }
                                        }
                                        //Right
                                        for (int Xf = bitbmp[0].Width - 1; Xf > 0; Xf--)
                                        {
                                            for (int fram = 0; fram < frameCount; fram++)
                                            {
                                                bitbmp[fram].SelectActiveFrame(dimension, fram);
                                                for (int Yf = 0; Yf < bitbmp[0].Height; Yf++)
                                                {
                                                    Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                    if (pixel == WhiteConvert | pixel == CustomConvert | pixel.A == 0)
                                                        var = true;
                                                    if (pixel != WhiteConvert & pixel != CustomConvert & pixel.A != 0)
                                                    {
                                                        var = false;
                                                        if (Xf != bitbmp[0].Width - 1)
                                                        {
                                                            RegressR++;
                                                            Xf += 1;
                                                            Yf = -1;
                                                            fram = 0;
                                                        }
                                                        else
                                                        {
                                                            breakOK = true;
                                                            break;
                                                        }
                                                    }
                                                    if (var == true && Yf == bitbmp[fram].Height - 1 && fram == frameCount - 1 && RegressR == -1 && Xf > 9)
                                                        right += 10;
                                                    if (var == true && Yf == bitbmp[fram].Height - 1 && fram == frameCount - 1 && RegressR == -1 && Xf <= 9)
                                                        right++;
                                                    if (var == true && Yf == bitbmp[fram].Height - 1 && fram == frameCount - 1 && RegressR != -1)
                                                    {
                                                        right = right - RegressR;
                                                        breakOK = true;
                                                        break;
                                                    }
                                                }
                                                if (breakOK == true)
                                                    break;
                                            }
                                            if (Xf > 9)
                                                Xf -= 9; // 1 of for + 9
                                            if (breakOK == true)
                                            {
                                                breakOK = false;
                                                break;
                                            }
                                        }

                                        for (int index = 0; index < frameCount; index++)
                                        {
                                            Rectangle rect = new Rectangle(left, top, (bitbmp[index].Width - left - right), (bitbmp[index].Height - top - bot));
                                            bitbmp[index].SelectActiveFrame(dimension, index);
                                            Bitmap myImage2 = bitbmp[index].Clone(rect, System.Drawing.Imaging.PixelFormat.Format16bppArgb1555);
                                            bitbmp[index] = myImage2;
                                        }

                                        //End of Canvas
                                        for (int index = 0; index < frameCount; index++)
                                        {
                                            bitbmp[index].SelectActiveFrame(dimension, index);
                                            bitbmp[index] = Utils.ConvertBmpAnim(bitbmp[index], (int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                                            edit.AddFrame(bitbmp[index]);
                                            TreeNode node = GetNode(CurrBody);
                                            if (node != null)
                                            {
                                                node.ForeColor = Color.Black;
                                                node.Nodes[CurrAction].ForeColor = Color.Black;
                                            }
                                            ListViewItem item;
                                            int i = edit.Frames.Count - 1;
                                            item = new ListViewItem(i.ToString(), 0);
                                            item.Tag = i;
                                            listView1.Items.Add(item);
                                            int width = listView1.TileSize.Width - 5;
                                            if (bmp.Width > listView1.TileSize.Width)
                                                width = bmp.Width;
                                            int height = listView1.TileSize.Height - 5;
                                            if (bmp.Height > listView1.TileSize.Height)
                                                height = bmp.Height;

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
                CurrDir = trackBar1.Value;
                AfterSelectTreeView(null, null);
            }
            catch (System.OutOfMemoryException) { }
        }

        private unsafe void OnClickGeneratePalette(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = true;
                dialog.Title = "Choose images to generate from";
                dialog.CheckFileExists = true;
                dialog.Filter = "image files (*.tiff;*.bmp;*.png;*.jpg;*.jpeg)|*.tiff;*.bmp;*.png;*.jpg;*.jpeg";
                if (dialog.ShowDialog() == DialogResult.OK)
                {

                    foreach (string filename in dialog.FileNames)
                    {
                        Bitmap bit = new Bitmap(filename);
                        AnimIdx edit = Ultima.AnimationEdit.GetAnimation(FileType, CurrBody, CurrAction, CurrDir);
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
            if (FileType != 0)
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
                                string FileName = Path.Combine(dialog.SelectedPath, String.Format("anim{0}_0x{1:X}.vd", FileType, index));
                                Ultima.AnimationEdit.ExportToVD(FileType, index, FileName);
                            }
                        }
                        MessageBox.Show(String.Format("All Animations saved to {0}", dialog.SelectedPath.ToString()),
                                "Export", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                    }
                }
            }

        }

        //Get position of all animations in array
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                trackBar1.Enabled = false;
                trackBar2.Value = 0;
                button1.Enabled = true;
                for (int count = 0; count < 5; )
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
        private void button1_Click(object sender, EventArgs e)
        {
            trackBar1.Value = 0;
            trackBar1.Enabled = false;
            for (int i = 0; i <= trackBar1.Maximum; i++)
            {
                try
                {
                    if (FileType != 0)
                    {
                        AnimIdx edit = Ultima.AnimationEdit.GetAnimation(FileType, CurrBody, CurrAction, CurrDir);
                        if (edit != null)
                        {
                            if (edit.Frames.Count >= trackBar2.Value)
                            {
                                for (int Index = 0; Index < edit.Frames.Count; Index++)
                                {
                                    edit.Frames[Index].ChangeCenter(AnimCx[i], AnimCy[i]);
                                    Options.ChangedUltimaClass["Animations"] = true;
                                    pictureBox1.Invalidate();
                                }
                            }
                        }
                    }
                }
                catch (System.NullReferenceException) { }
                if (trackBar1.Value < trackBar1.Maximum)
                {
                    trackBar1.Value++;
                }
                else
                    trackBar1.Value = 0;
            }
            trackBar1.Enabled = true;
        }
        //Add Directions with Canvas ( CV5 style GIF )
        private void addDirectionsAddWithCanvasUniqueImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (FileType != 0)
                {
                    using (OpenFileDialog dialog = new OpenFileDialog())
                    {
                        dialog.Multiselect = false;
                        dialog.Title = "Choose 1 Gif ( with all directions in CV5 Style ) to add";
                        dialog.CheckFileExists = true;
                        dialog.Filter = "Gif files (*.gif;)|*.gif;";
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            Color CustomConvert = Color.FromArgb(255, (int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                            trackBar1.Enabled = false;
                            trackBar1.Value = 0;
                            Bitmap bmp = new Bitmap(dialog.FileName);
                            AnimIdx edit = Ultima.AnimationEdit.GetAnimation(FileType, CurrBody, CurrAction, CurrDir);
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
                                    int RegressT = -1;
                                    int RegressB = -1;
                                    int RegressL = -1;
                                    int RegressR = -1;
                                    bool var = true;
                                    bool breakOK = false;
                                    // position 0
                                    //Top
                                    for (int Yf = 0; Yf < bitbmp[(frameCount / 8) * 4].Height; Yf++)
                                    {
                                        for (int fram = (frameCount / 8) * 4; fram < (frameCount / 8) * 5; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int Xf = 0; Xf < bitbmp[(frameCount / 8) * 4].Width; Xf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                if (pixel == WhiteConvert | pixel == GreyConvert | pixel == CustomConvert | pixel.A == 0)
                                                    var = true;
                                                if (pixel != WhiteConvert & pixel != GreyConvert & pixel != CustomConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (Yf != 0)
                                                    {
                                                        RegressT++;
                                                        Yf -= 1;
                                                        Xf = -1;
                                                        fram = (frameCount / 8) * 4;
                                                    }
                                                    else
                                                    {
                                                        breakOK = true;
                                                        break;
                                                    }
                                                }
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 8) * 5) - 1) && RegressT == -1 && Yf < bitbmp[0].Height - 9)
                                                    top += 10;
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 8) * 5) - 1) && RegressT == -1 && Yf >= bitbmp[0].Height - 9)
                                                    top++;
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 8) * 5) - 1) && RegressT != -1)
                                                {
                                                    top = top - RegressT;
                                                    breakOK = true;
                                                    break;
                                                }
                                            }
                                            if (breakOK == true)
                                                break;
                                        }
                                        if (Yf < bitbmp[(frameCount / 8) * 4].Height - 9)
                                            Yf += 9; // 1 of for + 9
                                        if (breakOK == true)
                                        {
                                            breakOK = false;
                                            break;
                                        }
                                    }
                                    //Bot
                                    for (int Yf = bitbmp[(frameCount / 8) * 4].Height - 1; Yf > 0; Yf--)
                                    {
                                        for (int fram = (frameCount / 8) * 4; fram < (frameCount / 8) * 5; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int Xf = 0; Xf < bitbmp[(frameCount / 8) * 4].Width; Xf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                if (pixel == WhiteConvert | pixel == GreyConvert | pixel == CustomConvert | pixel.A == 0)
                                                    var = true;
                                                if (pixel != WhiteConvert & pixel != GreyConvert & pixel != CustomConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (Yf != bitbmp[(frameCount / 8) * 4].Height - 1)
                                                    {
                                                        RegressB++;
                                                        Yf += 1;
                                                        Xf = -1;
                                                        fram = (frameCount / 8) * 4;
                                                    }
                                                    else
                                                    {
                                                        breakOK = true;
                                                        break;
                                                    }
                                                }
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 8) * 5) - 1) && RegressB == -1 && Yf > 9)
                                                    bot += 10;
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 8) * 5) - 1) && RegressB == -1 && Yf <= 9)
                                                    bot++;
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 8) * 5) - 1) && RegressB != -1)
                                                {
                                                    bot = bot - RegressB;
                                                    breakOK = true;
                                                    break;
                                                }
                                            }
                                            if (breakOK == true)
                                                break;
                                        }
                                        if (Yf > 9)
                                            Yf -= 9; // 1 of for + 9
                                        if (breakOK == true)
                                        {
                                            breakOK = false;
                                            break;
                                        }
                                    }
                                    //Left

                                    for (int Xf = 0; Xf < bitbmp[(frameCount / 8) * 4].Width; Xf++)
                                    {
                                        for (int fram = (frameCount / 8) * 4; fram < (frameCount / 8) * 5; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int Yf = 0; Yf < bitbmp[(frameCount / 8) * 4].Height; Yf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                if (pixel == WhiteConvert | pixel == GreyConvert | pixel == CustomConvert | pixel.A == 0)
                                                    var = true;
                                                if (pixel != WhiteConvert & pixel != GreyConvert & pixel != CustomConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (Xf != 0)
                                                    {
                                                        RegressL++;
                                                        Xf -= 1;
                                                        Yf = -1;
                                                        fram = (frameCount / 8) * 4;
                                                    }
                                                    else
                                                    {
                                                        breakOK = true;
                                                        break;
                                                    }
                                                }
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 8) * 5) - 1) && RegressL == -1 && Xf < bitbmp[0].Width - 9)
                                                    left += 10;
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 8) * 5) - 1) && RegressL == -1 && Xf >= bitbmp[0].Width - 9)
                                                    left++;
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 8) * 5) - 1) && RegressL != -1)
                                                {
                                                    left = left - RegressL;
                                                    breakOK = true;
                                                    break;
                                                }
                                            }
                                            if (breakOK == true)
                                                break;
                                        }
                                        if (Xf < bitbmp[(frameCount / 8) * 4].Width - 9)
                                            Xf += 9; // 1 of for + 9
                                        if (breakOK == true)
                                        {
                                            breakOK = false;
                                            break;
                                        }
                                    }
                                    //Right
                                    for (int Xf = bitbmp[(frameCount / 8) * 4].Width - 1; Xf > 0; Xf--)
                                    {
                                        for (int fram = (frameCount / 8) * 4; fram < (frameCount / 8) * 5; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int Yf = 0; Yf < bitbmp[(frameCount / 8) * 4].Height; Yf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                if (pixel == WhiteConvert | pixel == GreyConvert | pixel == CustomConvert | pixel.A == 0)
                                                    var = true;
                                                if (pixel != WhiteConvert & pixel != GreyConvert & pixel != CustomConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (Xf != bitbmp[(frameCount / 8) * 4].Width - 1)
                                                    {
                                                        RegressR++;
                                                        Xf += 1;
                                                        Yf = -1;
                                                        fram = (frameCount / 8) * 4;
                                                    }
                                                    else
                                                    {
                                                        breakOK = true;
                                                        break;
                                                    }
                                                }
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 8) * 5) - 1) && RegressR == -1 && Xf > 9)
                                                    right += 10;
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 8) * 5) - 1) && RegressR == -1 && Xf <= 9)
                                                    right++;
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 8) * 5) - 1) && RegressR != -1)
                                                {
                                                    right = right - RegressR;
                                                    breakOK = true;
                                                    break;
                                                }
                                            }
                                            if (breakOK == true)
                                                break;
                                        }
                                        if (Xf > 9)
                                            Xf -= 9; // 1 of for + 9
                                        if (breakOK == true)
                                        {
                                            breakOK = false;
                                            break;
                                        }
                                    }
                                    for (int index = (frameCount / 8) * 4; index < (frameCount / 8) * 5; index++)
                                    {
                                        Rectangle rect = new Rectangle(left, top, (bitbmp[index].Width - left - right), (bitbmp[index].Height - top - bot));
                                        bitbmp[index].SelectActiveFrame(dimension, index);
                                        Bitmap myImage2 = bitbmp[index].Clone(rect, System.Drawing.Imaging.PixelFormat.Format16bppArgb1555);
                                        bitbmp[index] = myImage2;
                                    }

                                    //Reseta cordenadas
                                    top = 0;
                                    bot = 0;
                                    left = 0;
                                    right = 0;
                                    RegressT = -1;
                                    RegressB = -1;
                                    RegressL = -1;
                                    RegressR = -1;
                                    var = true;
                                    breakOK = false;
                                    // position 1
                                    //Top
                                    for (int Yf = 0; Yf < bitbmp[0].Height; Yf++)
                                    {
                                        for (int fram = 0; fram < frameCount / 8; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int Xf = 0; Xf < bitbmp[0].Width; Xf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                if (pixel == WhiteConvert | pixel == GreyConvert | pixel == CustomConvert | pixel.A == 0)
                                                    var = true;
                                                if (pixel != WhiteConvert & pixel != GreyConvert & pixel != CustomConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (Yf != 0)
                                                    {
                                                        RegressT++;
                                                        Yf -= 1;
                                                        Xf = -1;
                                                        fram = 0;
                                                    }
                                                    else
                                                    {
                                                        breakOK = true;
                                                        break;
                                                    }
                                                }
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 8)) - 1) && RegressT == -1 && Yf < bitbmp[0].Height - 9)
                                                    top += 10;
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 8)) - 1) && RegressT == -1 && Yf >= bitbmp[0].Height - 9)
                                                    top++;
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 8)) - 1) && RegressT != -1)
                                                {
                                                    top = top - RegressT;
                                                    breakOK = true;
                                                    break;
                                                }
                                            }
                                            if (breakOK == true)
                                                break;
                                        }
                                        if (Yf < bitbmp[0].Height - 9)
                                            Yf += 9; // 1 of for + 9
                                        if (breakOK == true)
                                        {
                                            breakOK = false;
                                            break;
                                        }
                                    }
                                    //Bot
                                    for (int Yf = bitbmp[0].Height - 1; Yf > 0; Yf--)
                                    {
                                        for (int fram = 0; fram < frameCount / 8; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int Xf = 0; Xf < bitbmp[0].Width; Xf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                if (pixel == WhiteConvert | pixel == GreyConvert | pixel == CustomConvert | pixel.A == 0)
                                                    var = true;
                                                if (pixel != WhiteConvert & pixel != GreyConvert & pixel != CustomConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (Yf != bitbmp[0].Height - 1)
                                                    {
                                                        RegressB++;
                                                        Yf += 1;
                                                        Xf = -1;
                                                        fram = 0;
                                                    }
                                                    else
                                                    {
                                                        breakOK = true;
                                                        break;
                                                    }
                                                }
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 8)) - 1) && RegressB == -1 && Yf > 9)
                                                    bot += 10;
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 8)) - 1) && RegressB == -1 && Yf <= 9)
                                                    bot++;
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 8)) - 1) && RegressB != -1)
                                                {
                                                    bot = bot - RegressB;
                                                    breakOK = true;
                                                    break;
                                                }
                                            }
                                            if (breakOK == true)
                                                break;
                                        }
                                        if (Yf > 9)
                                            Yf -= 9; // 1 of for + 9
                                        if (breakOK == true)
                                        {
                                            breakOK = false;
                                            break;
                                        }
                                    }
                                    //Left

                                    for (int Xf = 0; Xf < bitbmp[0].Width; Xf++)
                                    {
                                        for (int fram = 0; fram < frameCount / 8; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int Yf = 0; Yf < bitbmp[0].Height; Yf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                if (pixel == WhiteConvert | pixel == GreyConvert | pixel == CustomConvert | pixel.A == 0)
                                                    var = true;
                                                if (pixel != WhiteConvert & pixel != GreyConvert & pixel != CustomConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (Xf != 0)
                                                    {
                                                        RegressL++;
                                                        Xf -= 1;
                                                        Yf = -1;
                                                        fram = 0;
                                                    }
                                                    else
                                                    {
                                                        breakOK = true;
                                                        break;
                                                    }
                                                }
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 8)) - 1) && RegressL == -1 && Xf < bitbmp[0].Width - 9)
                                                    left += 10;
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 8)) - 1) && RegressL == -1 && Xf >= bitbmp[0].Width - 9)
                                                    left++;
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 8)) - 1) && RegressL != -1)
                                                {
                                                    left = left - RegressL;
                                                    breakOK = true;
                                                    break;
                                                }
                                            }
                                            if (breakOK == true)
                                                break;
                                        }
                                        if (Xf < bitbmp[0].Width - 9)
                                            Xf += 9; // 1 of for + 9
                                        if (breakOK == true)
                                        {
                                            breakOK = false;
                                            break;
                                        }
                                    }
                                    //Right
                                    for (int Xf = bitbmp[0].Width - 1; Xf > 0; Xf--)
                                    {
                                        for (int fram = 0; fram < frameCount / 8; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int Yf = 0; Yf < bitbmp[0].Height; Yf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                if (pixel == WhiteConvert | pixel == GreyConvert | pixel == CustomConvert | pixel.A == 0)
                                                    var = true;
                                                if (pixel != WhiteConvert & pixel != GreyConvert & pixel != CustomConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (Xf != bitbmp[0].Width - 1)
                                                    {
                                                        RegressR++;
                                                        Xf += 1;
                                                        Yf = -1;
                                                        fram = 0;
                                                    }
                                                    else
                                                    {
                                                        breakOK = true;
                                                        break;
                                                    }
                                                }
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 8)) - 1) && RegressR == -1 && Xf > 9)
                                                    right += 10;
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 8)) - 1) && RegressR == -1 && Xf <= 9)
                                                    right++;
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 8)) - 1) && RegressR != -1)
                                                {
                                                    right = right - RegressR;
                                                    breakOK = true;
                                                    break;
                                                }
                                            }
                                            if (breakOK == true)
                                                break;
                                        }
                                        if (Xf > 9)
                                            Xf -= 9; // 1 of for + 9
                                        if (breakOK == true)
                                        {
                                            breakOK = false;
                                            break;
                                        }
                                    }
                                    for (int index = 0; index < frameCount / 8; index++)
                                    {
                                        Rectangle rect = new Rectangle(left, top, (bitbmp[index].Width - left - right), (bitbmp[index].Height - top - bot));
                                        bitbmp[index].SelectActiveFrame(dimension, index);
                                        Bitmap myImage2 = bitbmp[index].Clone(rect, System.Drawing.Imaging.PixelFormat.Format16bppArgb1555);
                                        bitbmp[index] = myImage2;
                                    }

                                    //Reseta cordenadas
                                    top = 0;
                                    bot = 0;
                                    left = 0;
                                    right = 0;
                                    RegressT = -1;
                                    RegressB = -1;
                                    RegressL = -1;
                                    RegressR = -1;
                                    var = true;
                                    breakOK = false;
                                    // position 2
                                    //Top
                                    for (int Yf = 0; Yf < bitbmp[((frameCount / 8) * 5)].Height; Yf++)
                                    {
                                        for (int fram = ((frameCount / 8) * 5); fram < (frameCount / 8) * 6; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int Xf = 0; Xf < bitbmp[((frameCount / 8) * 5)].Width; Xf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                if (pixel == WhiteConvert | pixel == GreyConvert | pixel == CustomConvert | pixel.A == 0)
                                                    var = true;
                                                if (pixel != WhiteConvert & pixel != GreyConvert & pixel != CustomConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (Yf != 0)
                                                    {
                                                        RegressT++;
                                                        Yf -= 1;
                                                        Xf = -1;
                                                        fram = ((frameCount / 8) * 5);
                                                    }
                                                    else
                                                    {
                                                        breakOK = true;
                                                        break;
                                                    }
                                                }
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 8) * 6) - 1) && RegressT == -1 && Yf < bitbmp[0].Height - 9)
                                                    top += 10;
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 8) * 6) - 1) && RegressT == -1 && Yf >= bitbmp[0].Height - 9)
                                                    top++;
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 8) * 6) - 1) && RegressT != -1)
                                                {
                                                    top = top - RegressT;
                                                    breakOK = true;
                                                    break;
                                                }
                                            }
                                            if (breakOK == true)
                                                break;
                                        }
                                        if (Yf < bitbmp[((frameCount / 8) * 5)].Height - 9)
                                            Yf += 9; // 1 of for + 9
                                        if (breakOK == true)
                                        {
                                            breakOK = false;
                                            break;
                                        }
                                    }
                                    //Bot
                                    for (int Yf = bitbmp[((frameCount / 8) * 5)].Height - 1; Yf > 0; Yf--)
                                    {
                                        for (int fram = ((frameCount / 8) * 5); fram < (frameCount / 8) * 6; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int Xf = 0; Xf < bitbmp[((frameCount / 8) * 5)].Width; Xf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                if (pixel == WhiteConvert | pixel == GreyConvert | pixel == CustomConvert | pixel.A == 0)
                                                    var = true;
                                                if (pixel != WhiteConvert & pixel != GreyConvert & pixel != CustomConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (Yf != bitbmp[((frameCount / 8) * 5)].Height - 1)
                                                    {
                                                        RegressB++;
                                                        Yf += 1;
                                                        Xf = -1;
                                                        fram = ((frameCount / 8) * 5);
                                                    }
                                                    else
                                                    {
                                                        breakOK = true;
                                                        break;
                                                    }
                                                }
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 8) * 6) - 1) && RegressB == -1 && Yf > 9)
                                                    bot += 10;
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 8) * 6) - 1) && RegressB == -1 && Yf <= 9)
                                                    bot++;
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 8) * 6) - 1) && RegressB != -1)
                                                {
                                                    bot = bot - RegressB;
                                                    breakOK = true;
                                                    break;
                                                }
                                            }
                                            if (breakOK == true)
                                                break;
                                        }
                                        if (Yf > 9)
                                            Yf -= 9; // 1 of for + 9
                                        if (breakOK == true)
                                        {
                                            breakOK = false;
                                            break;
                                        }
                                    }
                                    //Left

                                    for (int Xf = 0; Xf < bitbmp[((frameCount / 8) * 5)].Width; Xf++)
                                    {
                                        for (int fram = ((frameCount / 8) * 5); fram < (frameCount / 8) * 6; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int Yf = 0; Yf < bitbmp[((frameCount / 8) * 5)].Height; Yf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                if (pixel == WhiteConvert | pixel == GreyConvert | pixel == CustomConvert | pixel.A == 0)
                                                    var = true;
                                                if (pixel != WhiteConvert & pixel != GreyConvert & pixel != CustomConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (Xf != 0)
                                                    {
                                                        RegressL++;
                                                        Xf -= 1;
                                                        Yf = -1;
                                                        fram = ((frameCount / 8) * 5);
                                                    }
                                                    else
                                                    {
                                                        breakOK = true;
                                                        break;
                                                    }
                                                }
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 8) * 6) - 1) && RegressL == -1 && Xf < bitbmp[0].Width - 9)
                                                    left += 10;
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 8) * 6) - 1) && RegressL == -1 && Xf >= bitbmp[0].Width - 9)
                                                    left++;
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 8) * 6) - 1) && RegressL != -1)
                                                {
                                                    left = left - RegressL;
                                                    breakOK = true;
                                                    break;
                                                }
                                            }
                                            if (breakOK == true)
                                                break;
                                        }
                                        if (Xf < bitbmp[((frameCount / 8) * 5)].Width - 9)
                                            Xf += 9; // 1 of for + 9
                                        if (breakOK == true)
                                        {
                                            breakOK = false;
                                            break;
                                        }
                                    }
                                    //Right
                                    for (int Xf = bitbmp[((frameCount / 8) * 5)].Width - 1; Xf > 0; Xf--)
                                    {
                                        for (int fram = ((frameCount / 8) * 5); fram < (frameCount / 8) * 6; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int Yf = 0; Yf < bitbmp[((frameCount / 8) * 5)].Height; Yf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                if (pixel == WhiteConvert | pixel == GreyConvert | pixel == CustomConvert | pixel.A == 0)
                                                    var = true;
                                                if (pixel != WhiteConvert & pixel != GreyConvert & pixel != CustomConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (Xf != bitbmp[((frameCount / 8) * 5)].Width - 1)
                                                    {
                                                        RegressR++;
                                                        Xf += 1;
                                                        Yf = -1;
                                                        fram = ((frameCount / 8) * 5);
                                                    }
                                                    else
                                                    {
                                                        breakOK = true;
                                                        break;
                                                    }
                                                }
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 8) * 6) - 1) && RegressR == -1 && Xf > 9)
                                                    right += 10;
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 8) * 6) - 1) && RegressR == -1 && Xf <= 9)
                                                    right++;
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 8) * 6) - 1) && RegressR != -1)
                                                {
                                                    right = right - RegressR;
                                                    breakOK = true;
                                                    break;
                                                }
                                            }
                                            if (breakOK == true)
                                                break;
                                        }
                                        if (Xf > 9)
                                            Xf -= 9; // 1 of for + 9
                                        if (breakOK == true)
                                        {
                                            breakOK = false;
                                            break;
                                        }
                                    }
                                    for (int index = ((frameCount / 8) * 5); index < (frameCount / 8) * 6; index++)
                                    {
                                        Rectangle rect = new Rectangle(left, top, (bitbmp[index].Width - left - right), (bitbmp[index].Height - top - bot));
                                        bitbmp[index].SelectActiveFrame(dimension, index);
                                        Bitmap myImage2 = bitbmp[index].Clone(rect, System.Drawing.Imaging.PixelFormat.Format16bppArgb1555);
                                        bitbmp[index] = myImage2;
                                    }

                                    //Reseta cordenadas
                                    top = 0;
                                    bot = 0;
                                    left = 0;
                                    right = 0;
                                    RegressT = -1;
                                    RegressB = -1;
                                    RegressL = -1;
                                    RegressR = -1;
                                    var = true;
                                    breakOK = false;
                                    // position 3
                                    //Top
                                    for (int Yf = 0; Yf < bitbmp[((frameCount / 8) * 1)].Height; Yf++)
                                    {
                                        for (int fram = ((frameCount / 8) * 1); fram < (frameCount / 8) * 2; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int Xf = 0; Xf < bitbmp[((frameCount / 8) * 1)].Width; Xf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                if (pixel == WhiteConvert | pixel == GreyConvert | pixel == CustomConvert | pixel.A == 0)
                                                    var = true;
                                                if (pixel != WhiteConvert & pixel != GreyConvert & pixel != CustomConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (Yf != 0)
                                                    {
                                                        RegressT++;
                                                        Yf -= 1;
                                                        Xf = -1;
                                                        fram = ((frameCount / 8) * 1);
                                                    }
                                                    else
                                                    {
                                                        breakOK = true;
                                                        break;
                                                    }
                                                }
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 8) * 2) - 1) && RegressT == -1 && Yf < bitbmp[0].Height - 9)
                                                    top += 10;
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 8) * 2) - 1) && RegressT == -1 && Yf >= bitbmp[0].Height - 9)
                                                    top++;
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 8) * 2) - 1) && RegressT != -1)
                                                {
                                                    top = top - RegressT;
                                                    breakOK = true;
                                                    break;
                                                }
                                            }
                                            if (breakOK == true)
                                                break;
                                        }
                                        if (Yf < bitbmp[((frameCount / 8) * 1)].Height - 9)
                                            Yf += 9; // 1 of for + 9
                                        if (breakOK == true)
                                        {
                                            breakOK = false;
                                            break;
                                        }
                                    }
                                    //Bot
                                    for (int Yf = bitbmp[((frameCount / 8) * 1)].Height - 1; Yf > 0; Yf--)
                                    {
                                        for (int fram = ((frameCount / 8) * 1); fram < (frameCount / 8) * 2; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int Xf = 0; Xf < bitbmp[((frameCount / 8) * 1)].Width; Xf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                if (pixel == WhiteConvert | pixel == GreyConvert | pixel == CustomConvert | pixel.A == 0)
                                                    var = true;
                                                if (pixel != WhiteConvert & pixel != GreyConvert & pixel != CustomConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (Yf != bitbmp[((frameCount / 8) * 1)].Height - 1)
                                                    {
                                                        RegressB++;
                                                        Yf += 1;
                                                        Xf = -1;
                                                        fram = ((frameCount / 8) * 1);
                                                    }
                                                    else
                                                    {
                                                        breakOK = true;
                                                        break;
                                                    }
                                                }
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 8) * 2) - 1) && RegressB == -1 && Yf > 9)
                                                    bot += 10;
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 8) * 2) - 1) && RegressB == -1 && Yf <= 9)
                                                    bot++;
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 8) * 2) - 1) && RegressB != -1)
                                                {
                                                    bot = bot - RegressB;
                                                    breakOK = true;
                                                    break;
                                                }
                                            }
                                            if (breakOK == true)
                                                break;
                                        }
                                        if (Yf > 9)
                                            Yf -= 9; // 1 of for + 9
                                        if (breakOK == true)
                                        {
                                            breakOK = false;
                                            break;
                                        }
                                    }
                                    //Left

                                    for (int Xf = 0; Xf < bitbmp[((frameCount / 8) * 1)].Width; Xf++)
                                    {
                                        for (int fram = ((frameCount / 8) * 1); fram < (frameCount / 8) * 2; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int Yf = 0; Yf < bitbmp[((frameCount / 8) * 1)].Height; Yf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                if (pixel == WhiteConvert | pixel == GreyConvert | pixel == CustomConvert | pixel.A == 0)
                                                    var = true;
                                                if (pixel != WhiteConvert & pixel != GreyConvert & pixel != CustomConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (Xf != 0)
                                                    {
                                                        RegressL++;
                                                        Xf -= 1;
                                                        Yf = -1;
                                                        fram = ((frameCount / 8) * 1);
                                                    }
                                                    else
                                                    {
                                                        breakOK = true;
                                                        break;
                                                    }
                                                }
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 8) * 2) - 1) && RegressL == -1 && Xf < bitbmp[0].Width - 9)
                                                    left += 10;
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 8) * 2) - 1) && RegressL == -1 && Xf >= bitbmp[0].Width - 9)
                                                    left++;
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 8) * 2) - 1) && RegressL != -1)
                                                {
                                                    left = left - RegressL;
                                                    breakOK = true;
                                                    break;
                                                }
                                            }
                                            if (breakOK == true)
                                                break;
                                        }
                                        if (Xf < bitbmp[((frameCount / 8) * 1)].Width - 9)
                                            Xf += 9; // 1 of for + 9
                                        if (breakOK == true)
                                        {
                                            breakOK = false;
                                            break;
                                        }
                                    }
                                    //Right
                                    for (int Xf = bitbmp[((frameCount / 8) * 1)].Width - 1; Xf > 0; Xf--)
                                    {
                                        for (int fram = ((frameCount / 8) * 1); fram < (frameCount / 8) * 2; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int Yf = 0; Yf < bitbmp[((frameCount / 8) * 1)].Height; Yf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                if (pixel == WhiteConvert | pixel == GreyConvert | pixel == CustomConvert | pixel.A == 0)
                                                    var = true;
                                                if (pixel != WhiteConvert & pixel != GreyConvert & pixel != CustomConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (Xf != bitbmp[((frameCount / 8) * 1)].Width - 1)
                                                    {
                                                        RegressR++;
                                                        Xf += 1;
                                                        Yf = -1;
                                                        fram = ((frameCount / 8) * 1);
                                                    }
                                                    else
                                                    {
                                                        breakOK = true;
                                                        break;
                                                    }
                                                }
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 8) * 2) - 1) && RegressR == -1 && Xf > 9)
                                                    right += 10;
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 8) * 2) - 1) && RegressR == -1 && Xf <= 9)
                                                    right++;
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 8) * 2) - 1) && RegressR != -1)
                                                {
                                                    right = right - RegressR;
                                                    breakOK = true;
                                                    break;
                                                }
                                            }
                                            if (breakOK == true)
                                                break;
                                        }
                                        if (Xf > 9)
                                            Xf -= 9; // 1 of for + 9
                                        if (breakOK == true)
                                        {
                                            breakOK = false;
                                            break;
                                        }
                                    }
                                    for (int index = ((frameCount / 8) * 1); index < (frameCount / 8) * 2; index++)
                                    {
                                        Rectangle rect = new Rectangle(left, top, (bitbmp[index].Width - left - right), (bitbmp[index].Height - top - bot));
                                        bitbmp[index].SelectActiveFrame(dimension, index);
                                        Bitmap myImage2 = bitbmp[index].Clone(rect, System.Drawing.Imaging.PixelFormat.Format16bppArgb1555);
                                        bitbmp[index] = myImage2;
                                    }

                                    //Reseta cordenadas
                                    top = 0;
                                    bot = 0;
                                    left = 0;
                                    right = 0;
                                    RegressT = -1;
                                    RegressB = -1;
                                    RegressL = -1;
                                    RegressR = -1;
                                    var = true;
                                    breakOK = false;
                                    // position 4
                                    //Top
                                    for (int Yf = 0; Yf < bitbmp[((frameCount / 8) * 6)].Height; Yf++)
                                    {
                                        for (int fram = ((frameCount / 8) * 6); fram < (frameCount / 8) * 7; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int Xf = 0; Xf < bitbmp[((frameCount / 8) * 6)].Width; Xf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                if (pixel == WhiteConvert | pixel == GreyConvert | pixel == CustomConvert | pixel.A == 0)
                                                    var = true;
                                                if (pixel != WhiteConvert & pixel != GreyConvert & pixel != CustomConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (Yf != 0)
                                                    {
                                                        RegressT++;
                                                        Yf -= 1;
                                                        Xf = -1;
                                                        fram = ((frameCount / 8) * 6);
                                                    }
                                                    else
                                                    {
                                                        breakOK = true;
                                                        break;
                                                    }
                                                }
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 8) * 7) - 1) && RegressT == -1 && Yf < bitbmp[0].Height - 9)
                                                    top += 10;
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 8) * 7) - 1) && RegressT == -1 && Yf >= bitbmp[0].Height - 9)
                                                    top++;
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 8) * 7) - 1) && RegressT != -1)
                                                {
                                                    top = top - RegressT;
                                                    breakOK = true;
                                                    break;
                                                }
                                            }
                                            if (breakOK == true)
                                                break;
                                        }
                                        if (Yf < bitbmp[((frameCount / 8) * 6)].Height - 9)
                                            Yf += 9; // 1 of for + 9
                                        if (breakOK == true)
                                        {
                                            breakOK = false;
                                            break;
                                        }
                                    }
                                    //Bot
                                    for (int Yf = bitbmp[((frameCount / 8) * 6)].Height - 1; Yf > 0; Yf--)
                                    {
                                        for (int fram = ((frameCount / 8) * 6); fram < (frameCount / 8) * 7; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int Xf = 0; Xf < bitbmp[((frameCount / 8) * 6)].Width; Xf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                if (pixel == WhiteConvert | pixel == GreyConvert | pixel == CustomConvert | pixel.A == 0)
                                                    var = true;
                                                if (pixel != WhiteConvert & pixel != GreyConvert & pixel != CustomConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (Yf != bitbmp[((frameCount / 8) * 6)].Height - 1)
                                                    {
                                                        RegressB++;
                                                        Yf += 1;
                                                        Xf = -1;
                                                        fram = ((frameCount / 8) * 6);
                                                    }
                                                    else
                                                    {
                                                        breakOK = true;
                                                        break;
                                                    }
                                                }
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 8) * 7) - 1) && RegressB == -1 && Yf > 9)
                                                    bot += 10;
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 8) * 7) - 1) && RegressB == -1 && Yf <= 9)
                                                    bot++;
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 8) * 7) - 1) && RegressB != -1)
                                                {
                                                    bot = bot - RegressB;
                                                    breakOK = true;
                                                    break;
                                                }
                                            }
                                            if (breakOK == true)
                                                break;
                                        }
                                        if (Yf > 9)
                                            Yf -= 9; // 1 of for + 9
                                        if (breakOK == true)
                                        {
                                            breakOK = false;
                                            break;
                                        }
                                    }
                                    //Left

                                    for (int Xf = 0; Xf < bitbmp[((frameCount / 8) * 6)].Width; Xf++)
                                    {
                                        for (int fram = ((frameCount / 8) * 6); fram < (frameCount / 8) * 7; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int Yf = 0; Yf < bitbmp[((frameCount / 8) * 6)].Height; Yf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                if (pixel == WhiteConvert | pixel == GreyConvert | pixel == CustomConvert | pixel.A == 0)
                                                    var = true;
                                                if (pixel != WhiteConvert & pixel != GreyConvert & pixel != CustomConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (Xf != 0)
                                                    {
                                                        RegressL++;
                                                        Xf -= 1;
                                                        Yf = -1;
                                                        fram = ((frameCount / 8) * 6);
                                                    }
                                                    else
                                                    {
                                                        breakOK = true;
                                                        break;
                                                    }
                                                }
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 8) * 7) - 1) && RegressL == -1 && Xf < bitbmp[0].Width - 9)
                                                    left += 10;
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 8) * 7) - 1) && RegressL == -1 && Xf >= bitbmp[0].Width - 9)
                                                    left++;
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 8) * 7) - 1) && RegressL != -1)
                                                {
                                                    left = left - RegressL;
                                                    breakOK = true;
                                                    break;
                                                }
                                            }
                                            if (breakOK == true)
                                                break;
                                        }
                                        if (Xf < bitbmp[((frameCount / 8) * 6)].Width - 9)
                                            Xf += 9; // 1 of for + 9
                                        if (breakOK == true)
                                        {
                                            breakOK = false;
                                            break;
                                        }
                                    }
                                    //Right
                                    for (int Xf = bitbmp[((frameCount / 8) * 6)].Width - 1; Xf > 0; Xf--)
                                    {
                                        for (int fram = ((frameCount / 8) * 6); fram < (frameCount / 8) * 7; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int Yf = 0; Yf < bitbmp[((frameCount / 8) * 6)].Height; Yf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                if (pixel == WhiteConvert | pixel == GreyConvert | pixel == CustomConvert | pixel.A == 0)
                                                    var = true;
                                                if (pixel != WhiteConvert & pixel != GreyConvert & pixel != CustomConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (Xf != bitbmp[((frameCount / 8) * 6)].Width - 1)
                                                    {
                                                        RegressR++;
                                                        Xf += 1;
                                                        Yf = -1;
                                                        fram = ((frameCount / 8) * 6);
                                                    }
                                                    else
                                                    {
                                                        breakOK = true;
                                                        break;
                                                    }
                                                }
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 8) * 7) - 1) && RegressR == -1 && Xf > 9)
                                                    right += 10;
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 8) * 7) - 1) && RegressR == -1 && Xf <= 9)
                                                    right++;
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 8) * 7) - 1) && RegressR != -1)
                                                {
                                                    right = right - RegressR;
                                                    breakOK = true;
                                                    break;
                                                }
                                            }
                                            if (breakOK == true)
                                                break;
                                        }
                                        if (Xf > 9)
                                            Xf -= 9; // 1 of for + 9
                                        if (breakOK == true)
                                        {
                                            breakOK = false;
                                            break;
                                        }
                                    }
                                    for (int index = ((frameCount / 8) * 6); index < (frameCount / 8) * 7; index++)
                                    {
                                        Rectangle rect = new Rectangle(left, top, (bitbmp[index].Width - left - right), (bitbmp[index].Height - top - bot));
                                        bitbmp[index].SelectActiveFrame(dimension, index);
                                        Bitmap myImage2 = bitbmp[index].Clone(rect, System.Drawing.Imaging.PixelFormat.Format16bppArgb1555);
                                        bitbmp[index] = myImage2;
                                    }

                                    //End of Canvas
                                    //posicao 0
                                    for (int index = ((frameCount / 8) * 4); index < (frameCount / 8) * 5; index++)
                                    {
                                        bitbmp[index].SelectActiveFrame(dimension, index);
                                        bitbmp[index] = Utils.ConvertBmpAnimCV5(bitbmp[index], (int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                                        edit.AddFrame(bitbmp[index]);
                                        TreeNode node = GetNode(CurrBody);
                                        if (node != null)
                                        {
                                            node.ForeColor = Color.Black;
                                            node.Nodes[CurrAction].ForeColor = Color.Black;
                                        }
                                        ListViewItem item;
                                        int i = edit.Frames.Count - 1;
                                        item = new ListViewItem(i.ToString(), 0);
                                        item.Tag = i;
                                        listView1.Items.Add(item);
                                        int width = listView1.TileSize.Width - 5;
                                        if (bmp.Width > listView1.TileSize.Width)
                                            width = bmp.Width;
                                        int height = listView1.TileSize.Height - 5;
                                        if (bmp.Height > listView1.TileSize.Height)
                                            height = bmp.Height;

                                        listView1.TileSize = new Size(width + 5, height + 5);
                                        trackBar2.Maximum = i;
                                        Options.ChangedUltimaClass["Animations"] = true;
                                        if (progressBar1.Value < progressBar1.Maximum)
                                        {
                                            progressBar1.Value++;
                                            progressBar1.Invalidate();
                                        }
                                        if (index == ((frameCount / 8) * 5) - 1)
                                            trackBar1.Value++;
                                    }
                                    //posicao 1
                                    edit = Ultima.AnimationEdit.GetAnimation(FileType, CurrBody, CurrAction, CurrDir);
                                    bmp.SelectActiveFrame(dimension, 0);
                                    edit.GetGifPalette(bmp);
                                    for (int index = 0; index < (frameCount / 8); index++)
                                    {
                                        bitbmp[index].SelectActiveFrame(dimension, index);
                                        bitbmp[index] = Utils.ConvertBmpAnimCV5(bitbmp[index], (int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                                        edit.AddFrame(bitbmp[index]);
                                        TreeNode node = GetNode(CurrBody);
                                        if (node != null)
                                        {
                                            node.ForeColor = Color.Black;
                                            node.Nodes[CurrAction].ForeColor = Color.Black;
                                        }
                                        ListViewItem item;
                                        int i = edit.Frames.Count - 1;
                                        item = new ListViewItem(i.ToString(), 0);
                                        item.Tag = i;
                                        listView1.Items.Add(item);
                                        int width = listView1.TileSize.Width - 5;
                                        if (bmp.Width > listView1.TileSize.Width)
                                            width = bmp.Width;
                                        int height = listView1.TileSize.Height - 5;
                                        if (bmp.Height > listView1.TileSize.Height)
                                            height = bmp.Height;

                                        listView1.TileSize = new Size(width + 5, height + 5);
                                        trackBar2.Maximum = i;
                                        Options.ChangedUltimaClass["Animations"] = true;
                                        if (progressBar1.Value < progressBar1.Maximum)
                                        {
                                            progressBar1.Value++;
                                            progressBar1.Invalidate();
                                        }
                                        if (index == (frameCount / 8) - 1)
                                            trackBar1.Value++;
                                    }
                                    //posicao 2
                                    edit = Ultima.AnimationEdit.GetAnimation(FileType, CurrBody, CurrAction, CurrDir);
                                    bmp.SelectActiveFrame(dimension, 0);
                                    edit.GetGifPalette(bmp);
                                    for (int index = ((frameCount / 8) * 5); index < (frameCount / 8) * 6; index++)
                                    {
                                        bitbmp[index].SelectActiveFrame(dimension, index);
                                        bitbmp[index] = Utils.ConvertBmpAnimCV5(bitbmp[index], (int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                                        edit.AddFrame(bitbmp[index]);
                                        TreeNode node = GetNode(CurrBody);
                                        if (node != null)
                                        {
                                            node.ForeColor = Color.Black;
                                            node.Nodes[CurrAction].ForeColor = Color.Black;
                                        }
                                        ListViewItem item;
                                        int i = edit.Frames.Count - 1;
                                        item = new ListViewItem(i.ToString(), 0);
                                        item.Tag = i;
                                        listView1.Items.Add(item);
                                        int width = listView1.TileSize.Width - 5;
                                        if (bmp.Width > listView1.TileSize.Width)
                                            width = bmp.Width;
                                        int height = listView1.TileSize.Height - 5;
                                        if (bmp.Height > listView1.TileSize.Height)
                                            height = bmp.Height;

                                        listView1.TileSize = new Size(width + 5, height + 5);
                                        trackBar2.Maximum = i;
                                        Options.ChangedUltimaClass["Animations"] = true;
                                        if (progressBar1.Value < progressBar1.Maximum)
                                        {
                                            progressBar1.Value++;
                                            progressBar1.Invalidate();
                                        }
                                        if (index == ((frameCount / 8) * 6) - 1)
                                            trackBar1.Value++;
                                    }
                                    //posicao 3
                                    edit = Ultima.AnimationEdit.GetAnimation(FileType, CurrBody, CurrAction, CurrDir);
                                    bmp.SelectActiveFrame(dimension, 0);
                                    edit.GetGifPalette(bmp);
                                    for (int index = ((frameCount / 8) * 1); index < (frameCount / 8) * 2; index++)
                                    {
                                        bitbmp[index].SelectActiveFrame(dimension, index);
                                        bitbmp[index] = Utils.ConvertBmpAnimCV5(bitbmp[index], (int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                                        edit.AddFrame(bitbmp[index]);
                                        TreeNode node = GetNode(CurrBody);
                                        if (node != null)
                                        {
                                            node.ForeColor = Color.Black;
                                            node.Nodes[CurrAction].ForeColor = Color.Black;
                                        }
                                        ListViewItem item;
                                        int i = edit.Frames.Count - 1;
                                        item = new ListViewItem(i.ToString(), 0);
                                        item.Tag = i;
                                        listView1.Items.Add(item);
                                        int width = listView1.TileSize.Width - 5;
                                        if (bmp.Width > listView1.TileSize.Width)
                                            width = bmp.Width;
                                        int height = listView1.TileSize.Height - 5;
                                        if (bmp.Height > listView1.TileSize.Height)
                                            height = bmp.Height;

                                        listView1.TileSize = new Size(width + 5, height + 5);
                                        trackBar2.Maximum = i;
                                        Options.ChangedUltimaClass["Animations"] = true;
                                        if (progressBar1.Value < progressBar1.Maximum)
                                        {
                                            progressBar1.Value++;
                                            progressBar1.Invalidate();
                                        }
                                        if (index == ((frameCount / 8) * 2) - 1)
                                            trackBar1.Value++;
                                    }
                                    //posicao 4
                                    edit = Ultima.AnimationEdit.GetAnimation(FileType, CurrBody, CurrAction, CurrDir);
                                    bmp.SelectActiveFrame(dimension, 0);
                                    edit.GetGifPalette(bmp);
                                    for (int index = ((frameCount / 8) * 6); index < (frameCount / 8) * 7; index++)
                                    {
                                        bitbmp[index].SelectActiveFrame(dimension, index);
                                        bitbmp[index] = Utils.ConvertBmpAnimCV5(bitbmp[index], (int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                                        edit.AddFrame(bitbmp[index]);
                                        TreeNode node = GetNode(CurrBody);
                                        if (node != null)
                                        {
                                            node.ForeColor = Color.Black;
                                            node.Nodes[CurrAction].ForeColor = Color.Black;
                                        }
                                        ListViewItem item;
                                        int i = edit.Frames.Count - 1;
                                        item = new ListViewItem(i.ToString(), 0);
                                        item.Tag = i;
                                        listView1.Items.Add(item);
                                        int width = listView1.TileSize.Width - 5;
                                        if (bmp.Width > listView1.TileSize.Width)
                                            width = bmp.Width;
                                        int height = listView1.TileSize.Height - 5;
                                        if (bmp.Height > listView1.TileSize.Height)
                                            height = bmp.Height;

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
                CurrDir = trackBar1.Value;
                AfterSelectTreeView(null, null);
            }
            catch (System.NullReferenceException) { trackBar1.Enabled = true; }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
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
        private void allDirectionsAddWithCanvasKRframeEditorColorCorrectorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (FileType != 0)
                {
                    using (OpenFileDialog dialog = new OpenFileDialog())
                    {
                        dialog.Multiselect = false;
                        dialog.Title = "Choose 1 Gif ( with all directions in KRframeViewer Style ) to add";
                        dialog.CheckFileExists = true;
                        dialog.Filter = "Gif files (*.gif;)|*.gif;";
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            Color CustomConvert = Color.FromArgb(255, (int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                            trackBar1.Enabled = false;
                            trackBar1.Value = 0;
                            Bitmap bmp = new Bitmap(dialog.FileName);
                            AnimIdx edit = Ultima.AnimationEdit.GetAnimation(FileType, CurrBody, CurrAction, CurrDir);
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
                                    int RegressT = -1;
                                    int RegressB = -1;
                                    int RegressL = -1;
                                    int RegressR = -1;
                                    bool var = true;
                                    bool breakOK = false;
                                    // position 0
                                    //Top
                                    for (int Yf = 0; Yf < bitbmp[(frameCount / 5) * 0].Height; Yf++)
                                    {
                                        for (int fram = (frameCount / 5) * 0; fram < (frameCount / 5) * 1; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int Xf = 0; Xf < bitbmp[(frameCount / 5) * 0].Width; Xf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                if (pixel == WhiteConvert | pixel == CustomConvert | pixel.A == 0)
                                                    var = true;
                                                if (pixel != WhiteConvert & pixel != CustomConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (Yf != 0)
                                                    {
                                                        RegressT++;
                                                        Yf -= 1;
                                                        Xf = -1;
                                                        fram = (frameCount / 5) * 0;
                                                    }
                                                    else
                                                    {
                                                        breakOK = true;
                                                        break;
                                                    }
                                                }
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 5) * 1) - 1) && RegressT == -1 && Yf < bitbmp[0].Height - 9)
                                                    top += 10;
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 5) * 1) - 1) && RegressT == -1 && Yf >= bitbmp[0].Height - 9)
                                                    top++;
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 5) * 1) - 1) && RegressT != -1)
                                                {
                                                    top = top - RegressT;
                                                    breakOK = true;
                                                    break;
                                                }
                                            }
                                            if (breakOK == true)
                                                break;
                                        }
                                        if (Yf < bitbmp[(frameCount / 5) * 0].Height - 9)
                                            Yf += 9; // 1 of for + 9
                                        if (breakOK == true)
                                        {
                                            breakOK = false;
                                            break;
                                        }
                                    }
                                    //Bot
                                    for (int Yf = bitbmp[(frameCount / 5) * 0].Height - 1; Yf > 0; Yf--)
                                    {
                                        for (int fram = (frameCount / 5) * 0; fram < (frameCount / 5) * 1; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int Xf = 0; Xf < bitbmp[(frameCount / 5) * 0].Width; Xf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                if (pixel == WhiteConvert | pixel == CustomConvert | pixel.A == 0)
                                                    var = true;
                                                if (pixel != WhiteConvert & pixel != CustomConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (Yf != bitbmp[(frameCount / 5) * 0].Height - 1)
                                                    {
                                                        RegressB++;
                                                        Yf += 1;
                                                        Xf = -1;
                                                        fram = (frameCount / 5) * 0;
                                                    }
                                                    else
                                                    {
                                                        breakOK = true;
                                                        break;
                                                    }
                                                }
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 5) * 1) - 1) && RegressB == -1 && Yf > 9)
                                                    bot += 10;
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 5) * 1) - 1) && RegressB == -1 && Yf <= 9)
                                                    bot++;
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 5) * 1) - 1) && RegressB != -1)
                                                {
                                                    bot = bot - RegressB;
                                                    breakOK = true;
                                                    break;
                                                }
                                            }
                                            if (breakOK == true)
                                                break;
                                        }
                                        if (Yf > 9)
                                            Yf -= 9; // 1 of for + 9
                                        if (breakOK == true)
                                        {
                                            breakOK = false;
                                            break;
                                        }
                                    }
                                    //Left

                                    for (int Xf = 0; Xf < bitbmp[(frameCount / 5) * 0].Width; Xf++)
                                    {
                                        for (int fram = (frameCount / 5) * 0; fram < (frameCount / 5) * 1; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int Yf = 0; Yf < bitbmp[(frameCount / 5) * 0].Height; Yf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                if (pixel == WhiteConvert | pixel == CustomConvert | pixel.A == 0)
                                                    var = true;
                                                if (pixel != WhiteConvert & pixel != CustomConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (Xf != 0)
                                                    {
                                                        RegressL++;
                                                        Xf -= 1;
                                                        Yf = -1;
                                                        fram = (frameCount / 5) * 0;
                                                    }
                                                    else
                                                    {
                                                        breakOK = true;
                                                        break;
                                                    }
                                                }
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 5) * 1) - 1) && RegressL == -1 && Xf < bitbmp[0].Width - 9)
                                                    left += 10;
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 5) * 1) - 1) && RegressL == -1 && Xf >= bitbmp[0].Width - 9)
                                                    left++;
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 5) * 1) - 1) && RegressL != -1)
                                                {
                                                    left = left - RegressL;
                                                    breakOK = true;
                                                    break;
                                                }
                                            }
                                            if (breakOK == true)
                                                break;
                                        }
                                        if (Xf < bitbmp[(frameCount / 5) * 0].Width - 9)
                                            Xf += 9; // 1 of for + 9
                                        if (breakOK == true)
                                        {
                                            breakOK = false;
                                            break;
                                        }
                                    }
                                    //Right
                                    for (int Xf = bitbmp[(frameCount / 5) * 0].Width - 1; Xf > 0; Xf--)
                                    {
                                        for (int fram = (frameCount / 5) * 0; fram < (frameCount / 5) * 1; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int Yf = 0; Yf < bitbmp[(frameCount / 5) * 0].Height; Yf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                if (pixel == WhiteConvert | pixel == CustomConvert | pixel.A == 0)
                                                    var = true;
                                                if (pixel != WhiteConvert & pixel != CustomConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (Xf != bitbmp[(frameCount / 5) * 0].Width - 1)
                                                    {
                                                        RegressR++;
                                                        Xf += 1;
                                                        Yf = -1;
                                                        fram = (frameCount / 5) * 0;
                                                    }
                                                    else
                                                    {
                                                        breakOK = true;
                                                        break;
                                                    }
                                                }
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 5) * 1) - 1) && RegressR == -1 && Xf > 9)
                                                    right += 10;
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 5) * 1) - 1) && RegressR == -1 && Xf <= 9)
                                                    right++;
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 5) * 1) - 1) && RegressR != -1)
                                                {
                                                    right = right - RegressR;
                                                    breakOK = true;
                                                    break;
                                                }
                                            }
                                            if (breakOK == true)
                                                break;
                                        }
                                        if (Xf > 9)
                                            Xf -= 9; // 1 of for + 9
                                        if (breakOK == true)
                                        {
                                            breakOK = false;
                                            break;
                                        }
                                    }
                                    for (int index = (frameCount / 5) * 0; index < (frameCount / 5) * 1; index++)
                                    {
                                        Rectangle rect = new Rectangle(left, top, (bitbmp[index].Width - left - right), (bitbmp[index].Height - top - bot));
                                        bitbmp[index].SelectActiveFrame(dimension, index);
                                        Bitmap myImage2 = bitbmp[index].Clone(rect, System.Drawing.Imaging.PixelFormat.Format16bppArgb1555);
                                        bitbmp[index] = myImage2;
                                    }

                                    //Reseta cordenadas
                                    top = 0;
                                    bot = 0;
                                    left = 0;
                                    right = 0;
                                    RegressT = -1;
                                    RegressB = -1;
                                    RegressL = -1;
                                    RegressR = -1;
                                    var = true;
                                    breakOK = false;
                                    // position 1
                                    //Top
                                    for (int Yf = 0; Yf < bitbmp[(frameCount / 5) * 1].Height; Yf++)
                                    {
                                        for (int fram = (frameCount / 5) * 1; fram < (frameCount / 5) * 2; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int Xf = 0; Xf < bitbmp[(frameCount / 5) * 1].Width; Xf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                if (pixel == WhiteConvert | pixel == CustomConvert | pixel.A == 0)
                                                    var = true;
                                                if (pixel != WhiteConvert & pixel != CustomConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (Yf != 0)
                                                    {
                                                        RegressT++;
                                                        Yf -= 1;
                                                        Xf = -1;
                                                        fram = (frameCount / 5) * 1;
                                                    }
                                                    else
                                                    {
                                                        breakOK = true;
                                                        break;
                                                    }
                                                }
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 5) * 2) - 1) && RegressT == -1 && Yf < bitbmp[0].Height - 9)
                                                    top += 10;
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 5) * 2) - 1) && RegressT == -1 && Yf >= bitbmp[0].Height - 9)
                                                    top++;
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 5) * 2) - 1) && RegressT != -1)
                                                {
                                                    top = top - RegressT;
                                                    breakOK = true;
                                                    break;
                                                }
                                            }
                                            if (breakOK == true)
                                                break;
                                        }
                                        if (Yf < bitbmp[(frameCount / 5) * 1].Height - 9)
                                            Yf += 9; // 1 of for + 9
                                        if (breakOK == true)
                                        {
                                            breakOK = false;
                                            break;
                                        }
                                    }
                                    //Bot
                                    for (int Yf = bitbmp[(frameCount / 5) * 1].Height - 1; Yf > 0; Yf--)
                                    {
                                        for (int fram = (frameCount / 5) * 1; fram < (frameCount / 5) * 2; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int Xf = 0; Xf < bitbmp[(frameCount / 5) * 1].Width; Xf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                if (pixel == WhiteConvert | pixel == CustomConvert | pixel.A == 0)
                                                    var = true;
                                                if (pixel != WhiteConvert & pixel != CustomConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (Yf != bitbmp[(frameCount / 5) * 1].Height - 1)
                                                    {
                                                        RegressB++;
                                                        Yf += 1;
                                                        Xf = -1;
                                                        fram = (frameCount / 5) * 1;
                                                    }
                                                    else
                                                    {
                                                        breakOK = true;
                                                        break;
                                                    }
                                                }
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 5) * 2) - 1) && RegressB == -1 && Yf > 9)
                                                    bot += 10;
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 5) * 2) - 1) && RegressB == -1 && Yf <= 9)
                                                    bot++;
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 5) * 2) - 1) && RegressB != -1)
                                                {
                                                    bot = bot - RegressB;
                                                    breakOK = true;
                                                    break;
                                                }
                                            }
                                            if (breakOK == true)
                                                break;
                                        }
                                        if (Yf > 9)
                                            Yf -= 9; // 1 of for + 9
                                        if (breakOK == true)
                                        {
                                            breakOK = false;
                                            break;
                                        }
                                    }
                                    //Left

                                    for (int Xf = 0; Xf < bitbmp[(frameCount / 5) * 1].Width; Xf++)
                                    {
                                        for (int fram = (frameCount / 5) * 1; fram < (frameCount / 5) * 2; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int Yf = 0; Yf < bitbmp[(frameCount / 5) * 1].Height; Yf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                if (pixel == WhiteConvert | pixel == CustomConvert | pixel.A == 0)
                                                    var = true;
                                                if (pixel != WhiteConvert & pixel != CustomConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (Xf != 0)
                                                    {
                                                        RegressL++;
                                                        Xf -= 1;
                                                        Yf = -1;
                                                        fram = (frameCount / 5) * 1;
                                                    }
                                                    else
                                                    {
                                                        breakOK = true;
                                                        break;
                                                    }
                                                }
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 5) * 2) - 1) && RegressL == -1 && Xf < bitbmp[0].Width - 9)
                                                    left += 10;
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 5) * 2) - 1) && RegressL == -1 && Xf >= bitbmp[0].Width - 9)
                                                    left++;
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 5) * 2) - 1) && RegressL != -1)
                                                {
                                                    left = left - RegressL;
                                                    breakOK = true;
                                                    break;
                                                }
                                            }
                                            if (breakOK == true)
                                                break;
                                        }
                                        if (Xf < bitbmp[(frameCount / 5) * 1].Width - 9)
                                            Xf += 9; // 1 of for + 9
                                        if (breakOK == true)
                                        {
                                            breakOK = false;
                                            break;
                                        }
                                    }
                                    //Right
                                    for (int Xf = bitbmp[(frameCount / 5) * 1].Width - 1; Xf > 0; Xf--)
                                    {
                                        for (int fram = (frameCount / 5) * 1; fram < (frameCount / 5) * 2; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int Yf = 0; Yf < bitbmp[(frameCount / 5) * 1].Height; Yf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                if (pixel == WhiteConvert | pixel == CustomConvert | pixel.A == 0)
                                                    var = true;
                                                if (pixel != WhiteConvert & pixel != CustomConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (Xf != bitbmp[(frameCount / 5) * 1].Width - 1)
                                                    {
                                                        RegressR++;
                                                        Xf += 1;
                                                        Yf = -1;
                                                        fram = (frameCount / 5) * 1;
                                                    }
                                                    else
                                                    {
                                                        breakOK = true;
                                                        break;
                                                    }
                                                }
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 5) * 2) - 1) && RegressR == -1 && Xf > 9)
                                                    right += 10;
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 5) * 2) - 1) && RegressR == -1 && Xf <= 9)
                                                    right++;
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 5) * 2) - 1) && RegressR != -1)
                                                {
                                                    right = right - RegressR;
                                                    breakOK = true;
                                                    break;
                                                }
                                            }
                                            if (breakOK == true)
                                                break;
                                        }
                                        if (Xf > 9)
                                            Xf -= 9; // 1 of for + 9
                                        if (breakOK == true)
                                        {
                                            breakOK = false;
                                            break;
                                        }
                                    }
                                    for (int index = (frameCount / 5) * 1; index < (frameCount / 5) * 2; index++)
                                    {
                                        Rectangle rect = new Rectangle(left, top, (bitbmp[index].Width - left - right), (bitbmp[index].Height - top - bot));
                                        bitbmp[index].SelectActiveFrame(dimension, index);
                                        Bitmap myImage2 = bitbmp[index].Clone(rect, System.Drawing.Imaging.PixelFormat.Format16bppArgb1555);
                                        bitbmp[index] = myImage2;
                                    }

                                    //Reseta cordenadas
                                    top = 0;
                                    bot = 0;
                                    left = 0;
                                    right = 0;
                                    RegressT = -1;
                                    RegressB = -1;
                                    RegressL = -1;
                                    RegressR = -1;
                                    var = true;
                                    breakOK = false;
                                    // position 2
                                    //Top
                                    for (int Yf = 0; Yf < bitbmp[(frameCount / 5) * 2].Height; Yf++)
                                    {
                                        for (int fram = (frameCount / 5) * 2; fram < (frameCount / 5) * 3; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int Xf = 0; Xf < bitbmp[(frameCount / 5) * 2].Width; Xf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                if (pixel == WhiteConvert | pixel == CustomConvert | pixel.A == 0)
                                                    var = true;
                                                if (pixel != WhiteConvert & pixel != CustomConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (Yf != 0)
                                                    {
                                                        RegressT++;
                                                        Yf -= 1;
                                                        Xf = -1;
                                                        fram = (frameCount / 5) * 2;
                                                    }
                                                    else
                                                    {
                                                        breakOK = true;
                                                        break;
                                                    }
                                                }
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 5) * 3) - 1) && RegressT == -1 && Yf < bitbmp[0].Height - 9)
                                                    top += 10;
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 5) * 3) - 1) && RegressT == -1 && Yf >= bitbmp[0].Height - 9)
                                                    top++;
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 5) * 3) - 1) && RegressT != -1)
                                                {
                                                    top = top - RegressT;
                                                    breakOK = true;
                                                    break;
                                                }
                                            }
                                            if (breakOK == true)
                                                break;
                                        }
                                        if (Yf < bitbmp[(frameCount / 5) * 2].Height - 9)
                                            Yf += 9; // 1 of for + 9
                                        if (breakOK == true)
                                        {
                                            breakOK = false;
                                            break;
                                        }
                                    }
                                    //Bot
                                    for (int Yf = bitbmp[(frameCount / 5) * 2].Height - 1; Yf > 0; Yf--)
                                    {
                                        for (int fram = (frameCount / 5) * 2; fram < (frameCount / 5) * 3; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int Xf = 0; Xf < bitbmp[(frameCount / 5) * 2].Width; Xf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                if (pixel == WhiteConvert | pixel == CustomConvert | pixel.A == 0)
                                                    var = true;
                                                if (pixel != WhiteConvert & pixel != CustomConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (Yf != bitbmp[(frameCount / 5) * 2].Height - 1)
                                                    {
                                                        RegressB++;
                                                        Yf += 1;
                                                        Xf = -1;
                                                        fram = (frameCount / 5) * 2;
                                                    }
                                                    else
                                                    {
                                                        breakOK = true;
                                                        break;
                                                    }
                                                }
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 5) * 3) - 1) && RegressB == -1 && Yf > 9)
                                                    bot += 10;
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 5) * 3) - 1) && RegressB == -1 && Yf <= 9)
                                                    bot++;
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 5) * 3) - 1) && RegressB != -1)
                                                {
                                                    bot = bot - RegressB;
                                                    breakOK = true;
                                                    break;
                                                }
                                            }
                                            if (breakOK == true)
                                                break;
                                        }
                                        if (Yf > 9)
                                            Yf -= 9; // 1 of for + 9
                                        if (breakOK == true)
                                        {
                                            breakOK = false;
                                            break;
                                        }
                                    }
                                    //Left

                                    for (int Xf = 0; Xf < bitbmp[(frameCount / 5) * 2].Width; Xf++)
                                    {
                                        for (int fram = (frameCount / 5) * 2; fram < (frameCount / 5) * 3; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int Yf = 0; Yf < bitbmp[(frameCount / 5) * 2].Height; Yf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                if (pixel == WhiteConvert | pixel == CustomConvert | pixel.A == 0)
                                                    var = true;
                                                if (pixel != WhiteConvert & pixel != CustomConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (Xf != 0)
                                                    {
                                                        RegressL++;
                                                        Xf -= 1;
                                                        Yf = -1;
                                                        fram = (frameCount / 5) * 2;
                                                    }
                                                    else
                                                    {
                                                        breakOK = true;
                                                        break;
                                                    }
                                                }
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 5) * 3) - 1) && RegressL == -1 && Xf < bitbmp[0].Width - 9)
                                                    left += 10;
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 5) * 3) - 1) && RegressL == -1 && Xf >= bitbmp[0].Width - 9)
                                                    left++;
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 5) * 3) - 1) && RegressL != -1)
                                                {
                                                    left = left - RegressL;
                                                    breakOK = true;
                                                    break;
                                                }
                                            }
                                            if (breakOK == true)
                                                break;
                                        }
                                        if (Xf < bitbmp[(frameCount / 5) * 2].Width - 9)
                                            Xf += 9; // 1 of for + 9
                                        if (breakOK == true)
                                        {
                                            breakOK = false;
                                            break;
                                        }
                                    }
                                    //Right
                                    for (int Xf = bitbmp[(frameCount / 5) * 2].Width - 1; Xf > 0; Xf--)
                                    {
                                        for (int fram = (frameCount / 5) * 2; fram < (frameCount / 5) * 3; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int Yf = 0; Yf < bitbmp[(frameCount / 5) * 2].Height; Yf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                if (pixel == WhiteConvert | pixel == CustomConvert | pixel.A == 0)
                                                    var = true;
                                                if (pixel != WhiteConvert & pixel != CustomConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (Xf != bitbmp[(frameCount / 5) * 2].Width - 1)
                                                    {
                                                        RegressR++;
                                                        Xf += 1;
                                                        Yf = -1;
                                                        fram = (frameCount / 5) * 2;
                                                    }
                                                    else
                                                    {
                                                        breakOK = true;
                                                        break;
                                                    }
                                                }
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 5) * 3) - 1) && RegressR == -1 && Xf > 9)
                                                    right += 10;
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 5) * 3) - 1) && RegressR == -1 && Xf <= 9)
                                                    right++;
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 5) * 3) - 1) && RegressR != -1)
                                                {
                                                    right = right - RegressR;
                                                    breakOK = true;
                                                    break;
                                                }
                                            }
                                            if (breakOK == true)
                                                break;
                                        }
                                        if (Xf > 9)
                                            Xf -= 9; // 1 of for + 9
                                        if (breakOK == true)
                                        {
                                            breakOK = false;
                                            break;
                                        }
                                    }
                                    for (int index = (frameCount / 5) * 2; index < (frameCount / 5) * 3; index++)
                                    {
                                        Rectangle rect = new Rectangle(left, top, (bitbmp[index].Width - left - right), (bitbmp[index].Height - top - bot));
                                        bitbmp[index].SelectActiveFrame(dimension, index);
                                        Bitmap myImage2 = bitbmp[index].Clone(rect, System.Drawing.Imaging.PixelFormat.Format16bppArgb1555);
                                        bitbmp[index] = myImage2;
                                    }

                                    //Reseta cordenadas
                                    top = 0;
                                    bot = 0;
                                    left = 0;
                                    right = 0;
                                    RegressT = -1;
                                    RegressB = -1;
                                    RegressL = -1;
                                    RegressR = -1;
                                    var = true;
                                    breakOK = false;
                                    // position 3
                                    //Top
                                    for (int Yf = 0; Yf < bitbmp[(frameCount / 5) * 3].Height; Yf++)
                                    {
                                        for (int fram = (frameCount / 5) * 3; fram < (frameCount / 5) * 4; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int Xf = 0; Xf < bitbmp[(frameCount / 5) * 3].Width; Xf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                if (pixel == WhiteConvert | pixel == CustomConvert | pixel.A == 0)
                                                    var = true;
                                                if (pixel != WhiteConvert & pixel != CustomConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (Yf != 0)
                                                    {
                                                        RegressT++;
                                                        Yf -= 1;
                                                        Xf = -1;
                                                        fram = (frameCount / 5) * 3;
                                                    }
                                                    else
                                                    {
                                                        breakOK = true;
                                                        break;
                                                    }
                                                }
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 5) * 4) - 1) && RegressT == -1 && Yf < bitbmp[0].Height - 9)
                                                    top += 10;
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 5) * 4) - 1) && RegressT == -1 && Yf >= bitbmp[0].Height - 9)
                                                    top++;
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 5) * 4) - 1) && RegressT != -1)
                                                {
                                                    top = top - RegressT;
                                                    breakOK = true;
                                                    break;
                                                }
                                            }
                                            if (breakOK == true)
                                                break;
                                        }
                                        if (Yf < bitbmp[(frameCount / 5) * 3].Height - 9)
                                            Yf += 9; // 1 of for + 9
                                        if (breakOK == true)
                                        {
                                            breakOK = false;
                                            break;
                                        }
                                    }
                                    //Bot
                                    for (int Yf = bitbmp[(frameCount / 5) * 3].Height - 1; Yf > 0; Yf--)
                                    {
                                        for (int fram = (frameCount / 5) * 3; fram < (frameCount / 5) * 4; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int Xf = 0; Xf < bitbmp[(frameCount / 5) * 3].Width; Xf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                if (pixel == WhiteConvert | pixel == CustomConvert | pixel.A == 0)
                                                    var = true;
                                                if (pixel != WhiteConvert & pixel != CustomConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (Yf != bitbmp[(frameCount / 5) * 3].Height - 1)
                                                    {
                                                        RegressB++;
                                                        Yf += 1;
                                                        Xf = -1;
                                                        fram = (frameCount / 5) * 3;
                                                    }
                                                    else
                                                    {
                                                        breakOK = true;
                                                        break;
                                                    }
                                                }
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 5) * 4) - 1) && RegressB == -1 && Yf > 9)
                                                    bot += 10;
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 5) * 4) - 1) && RegressB == -1 && Yf <= 9)
                                                    bot++;
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 5) * 4) - 1) && RegressB != -1)
                                                {
                                                    bot = bot - RegressB;
                                                    breakOK = true;
                                                    break;
                                                }
                                            }
                                            if (breakOK == true)
                                                break;
                                        }
                                        if (Yf > 9)
                                            Yf -= 9; // 1 of for + 9
                                        if (breakOK == true)
                                        {
                                            breakOK = false;
                                            break;
                                        }
                                    }
                                    //Left

                                    for (int Xf = 0; Xf < bitbmp[(frameCount / 5) * 3].Width; Xf++)
                                    {
                                        for (int fram = (frameCount / 5) * 3; fram < (frameCount / 5) * 4; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int Yf = 0; Yf < bitbmp[(frameCount / 5) * 3].Height; Yf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                if (pixel == WhiteConvert | pixel == CustomConvert | pixel.A == 0)
                                                    var = true;
                                                if (pixel != WhiteConvert & pixel != CustomConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (Xf != 0)
                                                    {
                                                        RegressL++;
                                                        Xf -= 1;
                                                        Yf = -1;
                                                        fram = (frameCount / 5) * 3;
                                                    }
                                                    else
                                                    {
                                                        breakOK = true;
                                                        break;
                                                    }
                                                }
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 5) * 4) - 1) && RegressL == -1 && Xf < bitbmp[0].Width - 9)
                                                    left += 10;
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 5) * 4) - 1) && RegressL == -1 && Xf >= bitbmp[0].Width - 9)
                                                    left++;
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 5) * 4) - 1) && RegressL != -1)
                                                {
                                                    left = left - RegressL;
                                                    breakOK = true;
                                                    break;
                                                }
                                            }
                                            if (breakOK == true)
                                                break;
                                        }
                                        if (Xf < bitbmp[(frameCount / 5) * 3].Width - 9)
                                            Xf += 9; // 1 of for + 9
                                        if (breakOK == true)
                                        {
                                            breakOK = false;
                                            break;
                                        }
                                    }
                                    //Right
                                    for (int Xf = bitbmp[(frameCount / 5) * 3].Width - 1; Xf > 0; Xf--)
                                    {
                                        for (int fram = (frameCount / 5) * 3; fram < (frameCount / 5) * 4; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int Yf = 0; Yf < bitbmp[(frameCount / 5) * 3].Height; Yf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                if (pixel == WhiteConvert | pixel == CustomConvert | pixel.A == 0)
                                                    var = true;
                                                if (pixel != WhiteConvert & pixel != CustomConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (Xf != bitbmp[(frameCount / 5) * 3].Width - 1)
                                                    {
                                                        RegressR++;
                                                        Xf += 1;
                                                        Yf = -1;
                                                        fram = (frameCount / 5) * 3;
                                                    }
                                                    else
                                                    {
                                                        breakOK = true;
                                                        break;
                                                    }
                                                }
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 5) * 4) - 1) && RegressR == -1 && Xf > 9)
                                                    right += 10;
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 5) * 4) - 1) && RegressR == -1 && Xf <= 9)
                                                    right++;
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 5) * 4) - 1) && RegressR != -1)
                                                {
                                                    right = right - RegressR;
                                                    breakOK = true;
                                                    break;
                                                }
                                            }
                                            if (breakOK == true)
                                                break;
                                        }
                                        if (Xf > 9)
                                            Xf -= 9; // 1 of for + 9
                                        if (breakOK == true)
                                        {
                                            breakOK = false;
                                            break;
                                        }
                                    }
                                    for (int index = (frameCount / 5) * 3; index < (frameCount / 5) * 4; index++)
                                    {
                                        Rectangle rect = new Rectangle(left, top, (bitbmp[index].Width - left - right), (bitbmp[index].Height - top - bot));
                                        bitbmp[index].SelectActiveFrame(dimension, index);
                                        Bitmap myImage2 = bitbmp[index].Clone(rect, System.Drawing.Imaging.PixelFormat.Format16bppArgb1555);
                                        bitbmp[index] = myImage2;
                                    }

                                    //Reseta cordenadas
                                    top = 0;
                                    bot = 0;
                                    left = 0;
                                    right = 0;
                                    RegressT = -1;
                                    RegressB = -1;
                                    RegressL = -1;
                                    RegressR = -1;
                                    var = true;
                                    breakOK = false;
                                    // position 4
                                    //Top
                                    for (int Yf = 0; Yf < bitbmp[(frameCount / 5) * 4].Height; Yf++)
                                    {
                                        for (int fram = (frameCount / 5) * 4; fram < (frameCount / 5) * 5; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int Xf = 0; Xf < bitbmp[(frameCount / 5) * 4].Width; Xf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                if (pixel == WhiteConvert | pixel == CustomConvert | pixel.A == 0)
                                                    var = true;
                                                if (pixel != WhiteConvert & pixel != CustomConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (Yf != 0)
                                                    {
                                                        RegressT++;
                                                        Yf -= 1;
                                                        Xf = -1;
                                                        fram = (frameCount / 5) * 4;
                                                    }
                                                    else
                                                    {
                                                        breakOK = true;
                                                        break;
                                                    }
                                                }
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 5) * 5) - 1) && RegressT == -1 && Yf < bitbmp[0].Height - 9)
                                                    top += 10;
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 5) * 5) - 1) && RegressT == -1 && Yf >= bitbmp[0].Height - 9)
                                                    top++;
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 5) * 5) - 1) && RegressT != -1)
                                                {
                                                    top = top - RegressT;
                                                    breakOK = true;
                                                    break;
                                                }
                                            }
                                            if (breakOK == true)
                                                break;
                                        }
                                        if (Yf < bitbmp[(frameCount / 5) * 4].Height - 9)
                                            Yf += 9; // 1 of for + 9
                                        if (breakOK == true)
                                        {
                                            breakOK = false;
                                            break;
                                        }
                                    }
                                    //Bot
                                    for (int Yf = bitbmp[(frameCount / 5) * 4].Height - 1; Yf > 0; Yf--)
                                    {
                                        for (int fram = (frameCount / 5) * 4; fram < (frameCount / 5) * 5; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int Xf = 0; Xf < bitbmp[(frameCount / 5) * 4].Width; Xf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                if (pixel == WhiteConvert | pixel == CustomConvert | pixel.A == 0)
                                                    var = true;
                                                if (pixel != WhiteConvert & pixel != CustomConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (Yf != bitbmp[(frameCount / 5) * 4].Height - 1)
                                                    {
                                                        RegressB++;
                                                        Yf += 1;
                                                        Xf = -1;
                                                        fram = (frameCount / 5) * 4;
                                                    }
                                                    else
                                                    {
                                                        breakOK = true;
                                                        break;
                                                    }
                                                }
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 5) * 5) - 1) && RegressB == -1 && Yf > 9)
                                                    bot += 10;
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 5) * 5) - 1) && RegressB == -1 && Yf <= 9)
                                                    bot++;
                                                if (var == true && Xf == bitbmp[fram].Width - 1 && fram == (((frameCount / 5) * 5) - 1) && RegressB != -1)
                                                {
                                                    bot = bot - RegressB;
                                                    breakOK = true;
                                                    break;
                                                }
                                            }
                                            if (breakOK == true)
                                                break;
                                        }
                                        if (Yf > 9)
                                            Yf -= 9; // 1 of for + 9
                                        if (breakOK == true)
                                        {
                                            breakOK = false;
                                            break;
                                        }
                                    }
                                    //Left

                                    for (int Xf = 0; Xf < bitbmp[(frameCount / 5) * 4].Width; Xf++)
                                    {
                                        for (int fram = (frameCount / 5) * 4; fram < (frameCount / 5) * 5; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int Yf = 0; Yf < bitbmp[(frameCount / 5) * 4].Height; Yf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                if (pixel == WhiteConvert | pixel == CustomConvert | pixel.A == 0)
                                                    var = true;
                                                if (pixel != WhiteConvert & pixel != CustomConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (Xf != 0)
                                                    {
                                                        RegressL++;
                                                        Xf -= 1;
                                                        Yf = -1;
                                                        fram = (frameCount / 5) * 4;
                                                    }
                                                    else
                                                    {
                                                        breakOK = true;
                                                        break;
                                                    }
                                                }
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 5) * 5) - 1) && RegressL == -1 && Xf < bitbmp[0].Width - 9)
                                                    left += 10;
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 5) * 5) - 1) && RegressL == -1 && Xf >= bitbmp[0].Width - 9)
                                                    left++;
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 5) * 5) - 1) && RegressL != -1)
                                                {
                                                    left = left - RegressL;
                                                    breakOK = true;
                                                    break;
                                                }
                                            }
                                            if (breakOK == true)
                                                break;
                                        }
                                        if (Xf < bitbmp[(frameCount / 5) * 4].Width - 9)
                                            Xf += 9; // 1 of for + 9
                                        if (breakOK == true)
                                        {
                                            breakOK = false;
                                            break;
                                        }
                                    }
                                    //Right
                                    for (int Xf = bitbmp[(frameCount / 5) * 4].Width - 1; Xf > 0; Xf--)
                                    {
                                        for (int fram = (frameCount / 5) * 4; fram < (frameCount / 5) * 5; fram++)
                                        {
                                            bitbmp[fram].SelectActiveFrame(dimension, fram);
                                            for (int Yf = 0; Yf < bitbmp[(frameCount / 5) * 4].Height; Yf++)
                                            {
                                                Color pixel = bitbmp[fram].GetPixel(Xf, Yf);
                                                if (pixel == WhiteConvert | pixel == CustomConvert | pixel.A == 0)
                                                    var = true;
                                                if (pixel != WhiteConvert & pixel != CustomConvert & pixel.A != 0)
                                                {
                                                    var = false;
                                                    if (Xf != bitbmp[(frameCount / 5) * 4].Width - 1)
                                                    {
                                                        RegressR++;
                                                        Xf += 1;
                                                        Yf = -1;
                                                        fram = (frameCount / 5) * 4;
                                                    }
                                                    else
                                                    {
                                                        breakOK = true;
                                                        break;
                                                    }
                                                }
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 5) * 5) - 1) && RegressR == -1 && Xf > 9)
                                                    right += 10;
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 5) * 5) - 1) && RegressR == -1 && Xf <= 9)
                                                    right++;
                                                if (var == true && Yf == bitbmp[fram].Height - 1 && fram == (((frameCount / 5) * 5) - 1) && RegressR != -1)
                                                {
                                                    right = right - RegressR;
                                                    breakOK = true;
                                                    break;
                                                }
                                            }
                                            if (breakOK == true)
                                                break;
                                        }
                                        if (Xf > 9)
                                            Xf -= 9; // 1 of for + 9
                                        if (breakOK == true)
                                        {
                                            breakOK = false;
                                            break;
                                        }
                                    }
                                    for (int index = (frameCount / 5) * 4; index < (frameCount / 5) * 5; index++)
                                    {
                                        Rectangle rect = new Rectangle(left, top, (bitbmp[index].Width - left - right), (bitbmp[index].Height - top - bot));
                                        bitbmp[index].SelectActiveFrame(dimension, index);
                                        Bitmap myImage2 = bitbmp[index].Clone(rect, System.Drawing.Imaging.PixelFormat.Format16bppArgb1555);
                                        bitbmp[index] = myImage2;
                                    }

                                    //End of Canvas
                                    //posicao 0
                                    for (int index = ((frameCount / 5) * 0); index < (frameCount / 5) * 1; index++)
                                    {
                                        bitbmp[index].SelectActiveFrame(dimension, index);
                                        bitbmp[index] = Utils.ConvertBmpAnimKR(bitbmp[index], (int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                                        edit.AddFrame(bitbmp[index]);
                                        TreeNode node = GetNode(CurrBody);
                                        if (node != null)
                                        {
                                            node.ForeColor = Color.Black;
                                            node.Nodes[CurrAction].ForeColor = Color.Black;
                                        }
                                        ListViewItem item;
                                        int i = edit.Frames.Count - 1;
                                        item = new ListViewItem(i.ToString(), 0);
                                        item.Tag = i;
                                        listView1.Items.Add(item);
                                        int width = listView1.TileSize.Width - 5;
                                        if (bmp.Width > listView1.TileSize.Width)
                                            width = bmp.Width;
                                        int height = listView1.TileSize.Height - 5;
                                        if (bmp.Height > listView1.TileSize.Height)
                                            height = bmp.Height;

                                        listView1.TileSize = new Size(width + 5, height + 5);
                                        trackBar2.Maximum = i;
                                        Options.ChangedUltimaClass["Animations"] = true;
                                        if (progressBar1.Value < progressBar1.Maximum)
                                        {
                                            progressBar1.Value++;
                                            progressBar1.Invalidate();
                                        }
                                        if (index == ((frameCount / 5) * 1) - 1)
                                            trackBar1.Value++;
                                    }
                                    //posicao 1
                                    edit = Ultima.AnimationEdit.GetAnimation(FileType, CurrBody, CurrAction, CurrDir);
                                    bmp.SelectActiveFrame(dimension, 0);
                                    edit.GetGifPalette(bmp);
                                    for (int index = ((frameCount / 5) * 1); index < ((frameCount / 5) * 2); index++)
                                    {
                                        bitbmp[index].SelectActiveFrame(dimension, index);
                                        bitbmp[index] = Utils.ConvertBmpAnimKR(bitbmp[index], (int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                                        edit.AddFrame(bitbmp[index]);
                                        TreeNode node = GetNode(CurrBody);
                                        if (node != null)
                                        {
                                            node.ForeColor = Color.Black;
                                            node.Nodes[CurrAction].ForeColor = Color.Black;
                                        }
                                        ListViewItem item;
                                        int i = edit.Frames.Count - 1;
                                        item = new ListViewItem(i.ToString(), 0);
                                        item.Tag = i;
                                        listView1.Items.Add(item);
                                        int width = listView1.TileSize.Width - 5;
                                        if (bmp.Width > listView1.TileSize.Width)
                                            width = bmp.Width;
                                        int height = listView1.TileSize.Height - 5;
                                        if (bmp.Height > listView1.TileSize.Height)
                                            height = bmp.Height;

                                        listView1.TileSize = new Size(width + 5, height + 5);
                                        trackBar2.Maximum = i;
                                        Options.ChangedUltimaClass["Animations"] = true;
                                        if (progressBar1.Value < progressBar1.Maximum)
                                        {
                                            progressBar1.Value++;
                                            progressBar1.Invalidate();
                                        }
                                        if (index == ((frameCount / 5) * 2) - 1)
                                            trackBar1.Value++;
                                    }
                                    //posicao 2
                                    edit = Ultima.AnimationEdit.GetAnimation(FileType, CurrBody, CurrAction, CurrDir);
                                    bmp.SelectActiveFrame(dimension, 0);
                                    edit.GetGifPalette(bmp);
                                    for (int index = ((frameCount / 5) * 2); index < (frameCount / 5) * 3; index++)
                                    {
                                        bitbmp[index].SelectActiveFrame(dimension, index);
                                        bitbmp[index] = Utils.ConvertBmpAnimKR(bitbmp[index], (int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                                        edit.AddFrame(bitbmp[index]);
                                        TreeNode node = GetNode(CurrBody);
                                        if (node != null)
                                        {
                                            node.ForeColor = Color.Black;
                                            node.Nodes[CurrAction].ForeColor = Color.Black;
                                        }
                                        ListViewItem item;
                                        int i = edit.Frames.Count - 1;
                                        item = new ListViewItem(i.ToString(), 0);
                                        item.Tag = i;
                                        listView1.Items.Add(item);
                                        int width = listView1.TileSize.Width - 5;
                                        if (bmp.Width > listView1.TileSize.Width)
                                            width = bmp.Width;
                                        int height = listView1.TileSize.Height - 5;
                                        if (bmp.Height > listView1.TileSize.Height)
                                            height = bmp.Height;

                                        listView1.TileSize = new Size(width + 5, height + 5);
                                        trackBar2.Maximum = i;
                                        Options.ChangedUltimaClass["Animations"] = true;
                                        if (progressBar1.Value < progressBar1.Maximum)
                                        {
                                            progressBar1.Value++;
                                            progressBar1.Invalidate();
                                        }
                                        if (index == ((frameCount / 5) * 3) - 1)
                                            trackBar1.Value++;
                                    }
                                    //posicao 3
                                    edit = Ultima.AnimationEdit.GetAnimation(FileType, CurrBody, CurrAction, CurrDir);
                                    bmp.SelectActiveFrame(dimension, 0);
                                    edit.GetGifPalette(bmp);
                                    for (int index = ((frameCount / 5) * 3); index < (frameCount / 5) * 4; index++)
                                    {
                                        bitbmp[index].SelectActiveFrame(dimension, index);
                                        bitbmp[index] = Utils.ConvertBmpAnimKR(bitbmp[index], (int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                                        edit.AddFrame(bitbmp[index]);
                                        TreeNode node = GetNode(CurrBody);
                                        if (node != null)
                                        {
                                            node.ForeColor = Color.Black;
                                            node.Nodes[CurrAction].ForeColor = Color.Black;
                                        }
                                        ListViewItem item;
                                        int i = edit.Frames.Count - 1;
                                        item = new ListViewItem(i.ToString(), 0);
                                        item.Tag = i;
                                        listView1.Items.Add(item);
                                        int width = listView1.TileSize.Width - 5;
                                        if (bmp.Width > listView1.TileSize.Width)
                                            width = bmp.Width;
                                        int height = listView1.TileSize.Height - 5;
                                        if (bmp.Height > listView1.TileSize.Height)
                                            height = bmp.Height;

                                        listView1.TileSize = new Size(width + 5, height + 5);
                                        trackBar2.Maximum = i;
                                        Options.ChangedUltimaClass["Animations"] = true;
                                        if (progressBar1.Value < progressBar1.Maximum)
                                        {
                                            progressBar1.Value++;
                                            progressBar1.Invalidate();
                                        }
                                        if (index == ((frameCount / 5) * 4) - 1)
                                            trackBar1.Value++;
                                    }
                                    //posicao 4
                                    edit = Ultima.AnimationEdit.GetAnimation(FileType, CurrBody, CurrAction, CurrDir);
                                    bmp.SelectActiveFrame(dimension, 0);
                                    edit.GetGifPalette(bmp);
                                    for (int index = ((frameCount / 5) * 4); index < (frameCount / 5) * 5; index++)
                                    {
                                        bitbmp[index].SelectActiveFrame(dimension, index);
                                        bitbmp[index] = Utils.ConvertBmpAnimKR(bitbmp[index], (int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value);
                                        edit.AddFrame(bitbmp[index]);
                                        TreeNode node = GetNode(CurrBody);
                                        if (node != null)
                                        {
                                            node.ForeColor = Color.Black;
                                            node.Nodes[CurrAction].ForeColor = Color.Black;
                                        }
                                        ListViewItem item;
                                        int i = edit.Frames.Count - 1;
                                        item = new ListViewItem(i.ToString(), 0);
                                        item.Tag = i;
                                        listView1.Items.Add(item);
                                        int width = listView1.TileSize.Width - 5;
                                        if (bmp.Width > listView1.TileSize.Width)
                                            width = bmp.Width;
                                        int height = listView1.TileSize.Height - 5;
                                        if (bmp.Height > listView1.TileSize.Height)
                                            height = bmp.Height;

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
                CurrDir = trackBar1.Value;
                AfterSelectTreeView(null, null);
            }
            catch (System.NullReferenceException) { trackBar1.Enabled = true; }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            for (int x = 0; x < 5; x++)
            {
                //RGB
                if (radioButton1.Checked == true)
                {
                    AnimIdx edit = Ultima.AnimationEdit.GetAnimation(FileType, CurrBody, CurrAction, CurrDir);
                    if (edit != null)
                    {
                        edit.PaletteConversor(1);
                    }
                }
                //RBG
                if (radioButton2.Checked == true)
                {
                    AnimIdx edit = Ultima.AnimationEdit.GetAnimation(FileType, CurrBody, CurrAction, CurrDir);
                    if (edit != null)
                    {
                        edit.PaletteConversor(2);
                    }
                }
                //GRB
                if (radioButton3.Checked == true)
                {
                    AnimIdx edit = Ultima.AnimationEdit.GetAnimation(FileType, CurrBody, CurrAction, CurrDir);
                    if (edit != null)
                    {
                        edit.PaletteConversor(3);
                    }
                }
                //GBR
                if (radioButton4.Checked == true)
                {
                    AnimIdx edit = Ultima.AnimationEdit.GetAnimation(FileType, CurrBody, CurrAction, CurrDir);
                    if (edit != null)
                    {
                        edit.PaletteConversor(4);
                    }
                }
                //BGR
                if (radioButton5.Checked == true)
                {
                    AnimIdx edit = Ultima.AnimationEdit.GetAnimation(FileType, CurrBody, CurrAction, CurrDir);
                    if (edit != null)
                    {
                        edit.PaletteConversor(5);
                    }
                }
                //BRG
                if (radioButton6.Checked == true)
                {
                    AnimIdx edit = Ultima.AnimationEdit.GetAnimation(FileType, CurrBody, CurrAction, CurrDir);
                    if (edit != null)
                    {
                        edit.PaletteConversor(6);
                    }
                }
                SetPaletteBox();
                listView1.Invalidate();
                Options.ChangedUltimaClass["Animations"] = true;
                SetPaletteBox();
                if (trackBar1.Value != trackBar1.Maximum)
                    trackBar1.Value++;
                else
                    trackBar1.Value = 0;
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {

            for (int x = 0; x < 5; x++)
            {
                AnimIdx edit = Ultima.AnimationEdit.GetAnimation(FileType, CurrBody, CurrAction, CurrDir);
                if (edit != null)
                {
                    edit.PaletteConversor(2);
                }
                SetPaletteBox();
                listView1.Invalidate();
                Options.ChangedUltimaClass["Animations"] = true;
                SetPaletteBox();
                if (trackBar1.Value != trackBar1.Maximum)
                    trackBar1.Value++;
                else
                    trackBar1.Value = 0;
            }

        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            for (int x = 0; x < 5; x++)
            {
                AnimIdx edit = Ultima.AnimationEdit.GetAnimation(FileType, CurrBody, CurrAction, CurrDir);
                if (edit != null)
                {
                    edit.PaletteConversor(3);
                }
                SetPaletteBox();
                listView1.Invalidate();
                Options.ChangedUltimaClass["Animations"] = true;
                SetPaletteBox();
                if (trackBar1.Value != trackBar1.Maximum)
                    trackBar1.Value++;
                else
                    trackBar1.Value = 0;
            }
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton4.Checked == false)
            {
                for (int x = 0; x < 5; x++)
                {
                    AnimIdx edit = Ultima.AnimationEdit.GetAnimation(FileType, CurrBody, CurrAction, CurrDir);
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
                        trackBar1.Value++;
                    else
                        trackBar1.Value = 0;
                }
            }
            else
            {
                for (int x = 0; x < 5; x++)
                {
                    AnimIdx edit = Ultima.AnimationEdit.GetAnimation(FileType, CurrBody, CurrAction, CurrDir);
                    if (edit != null)
                    {
                        edit.PaletteConversor(4);
                    }
                    SetPaletteBox();
                    listView1.Invalidate();
                    Options.ChangedUltimaClass["Animations"] = true;
                    SetPaletteBox();
                    if (trackBar1.Value != trackBar1.Maximum)
                        trackBar1.Value++;
                    else
                        trackBar1.Value = 0;
                }
            }
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            for (int x = 0; x < 5; x++)
            {
                AnimIdx edit = Ultima.AnimationEdit.GetAnimation(FileType, CurrBody, CurrAction, CurrDir);
                if (edit != null)
                {
                    edit.PaletteConversor(5);
                }
                SetPaletteBox();
                listView1.Invalidate();
                Options.ChangedUltimaClass["Animations"] = true;
                SetPaletteBox();
                if (trackBar1.Value != trackBar1.Maximum)
                    trackBar1.Value++;
                else
                    trackBar1.Value = 0;
            }
        }

        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton6.Checked == false)
            {
                for (int x = 0; x < 5; x++)
                {
                    AnimIdx edit = Ultima.AnimationEdit.GetAnimation(FileType, CurrBody, CurrAction, CurrDir);
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
                        trackBar1.Value++;
                    else
                        trackBar1.Value = 0;
                }
            }
            else
            {
                for (int x = 0; x < 5; x++)
                {
                    AnimIdx edit = Ultima.AnimationEdit.GetAnimation(FileType, CurrBody, CurrAction, CurrDir);
                    if (edit != null)
                    {
                        edit.PaletteConversor(6);
                    }
                    SetPaletteBox();
                    listView1.Invalidate();
                    Options.ChangedUltimaClass["Animations"] = true;
                    SetPaletteBox();
                    if (trackBar1.Value != trackBar1.Maximum)
                        trackBar1.Value++;
                    else
                        trackBar1.Value = 0;
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            for (int x = 0; x < 5; x++)
            {
                AnimIdx edit = Ultima.AnimationEdit.GetAnimation(FileType, CurrBody, CurrAction, CurrDir);
                if (edit != null)
                {
                    edit.PaletteReductor((int)numericUpDown6.Value, (int)numericUpDown7.Value, (int)numericUpDown8.Value);
                }
                SetPaletteBox();
                listView1.Invalidate();
                Options.ChangedUltimaClass["Animations"] = true;
                SetPaletteBox();
                if (trackBar1.Value != trackBar1.Maximum)
                    trackBar1.Value++;
                else
                    trackBar1.Value = 0;
            }
        }
    }
}
