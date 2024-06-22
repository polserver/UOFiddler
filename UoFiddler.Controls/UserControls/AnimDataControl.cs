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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using AnimatedGif;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;
using UoFiddler.Controls.Helpers;

namespace UoFiddler.Controls.UserControls
{
    public partial class AnimDataControl : UserControl
    {
        public AnimDataControl()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        }

        private static bool _loaded;
        private Animdata.AnimdataEntry _selAnimdataEntry;
        private int _currentSelect;
        private AnimDataImportForm _importForm;
        private AnimDataExportForm _exportForm;
        private HuePopUpForm _showForm;
        private int _customHue = 0;
        private bool _hueOnlyGray = false;

        [Browsable(false),
DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private int CurrFrame
        {
            get => MainPictureBox.FrameIndex;
            set => MainPictureBox.FrameIndex = value;
        }

        [Browsable(false),
                DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private int CurrentSelect
        {
            get => _currentSelect;
            set
            {
                if (!_loaded)
                {
                    return;
                }

                _selAnimdataEntry = Animdata.AnimData[value];
                if (_currentSelect != value)
                {
                    treeViewFrames.BeginUpdate();
                    treeViewFrames.Nodes.Clear();
                    for (int i = 0; i < _selAnimdataEntry.FrameCount; ++i)
                    {
                        TreeNode node = new TreeNode();
                        int frame = value + _selAnimdataEntry.FrameData[i];
                        node.Text = $"0x{frame:X4} {TileData.ItemTable[frame].Name}";
                        treeViewFrames.Nodes.Add(node);
                    }
                    treeViewFrames.EndUpdate();

                    toolStripStatusBaseGraphic.Text = $"Base Graphic: {value} (0x{value:X})";

                    _currentSelect = value;

                    SetPicture();
                }
                numericUpDownFrameDelay.Value = _selAnimdataEntry.FrameInterval;
                numericUpDownStartDelay.Value = _selAnimdataEntry.FrameStart;
            }
        }

        private void SetPicture()
        {
            if (_selAnimdataEntry == null)
            {
                return;
            }

            List<AnimatedFrame> frames = [];

            for (int i = 0; i < _selAnimdataEntry.FrameCount; ++i)
            {
                int graphic = _currentSelect + _selAnimdataEntry.FrameData[i];
                var frame = Art.GetStatic(graphic);
                if (frame == null)
                {
                    continue;
                }

                if (_customHue > 0)
                {
                    frame = new Bitmap(frame);
                    Hue hueObject = Hues.List[_customHue - 1];
                    hueObject.ApplyTo(frame, _hueOnlyGray);

                    frames.Add(new AnimatedFrame(frame));
                }
                else
                {
                    frames.Add(new AnimatedFrame(frame));
                }
            }

            MainPictureBox.Frames = frames;
        }

        /// <summary>
        /// ReLoads if loaded
        /// </summary>
        private void Reload()
        {
            if (_loaded)
            {
                OnLoad(this, EventArgs.Empty);
            }
        }

        private void OnLoad(object sender, EventArgs e)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            Options.LoadedUltimaClass["Animdata"] = true;
            Options.LoadedUltimaClass["TileData"] = true;
            Options.LoadedUltimaClass["Art"] = true;
            treeView1.BeginUpdate();
            treeView1.Nodes.Clear();

            treeView1.TreeViewNodeSorter = new AnimdataSorter();

            foreach (int id in Animdata.AnimData.Keys)
            {
                Animdata.AnimdataEntry animdataEntry = Animdata.AnimData[id];
                TreeNode node = new TreeNode
                {
                    Tag = id,
                    Text = $"0x{id:X4} {TileData.ItemTable[id].Name}"
                };

                if (!Art.IsValidStatic(id))
                {
                    node.ForeColor = Color.Red;
                }
                else if ((TileData.ItemTable[id].Flags & TileFlag.Animation) == 0)
                {
                    node.ForeColor = Color.Blue;
                }

                treeView1.Nodes.Add(node);
                for (int i = 0; i < animdataEntry.FrameCount; ++i)
                {
                    int frame = id + animdataEntry.FrameData[i];
                    if (Art.IsValidStatic(frame))
                    {
                        TreeNode subNode = new TreeNode
                        {
                            Text = $"0x{frame:X4} {TileData.ItemTable[frame].Name}"
                        };
                        node.Nodes.Add(subNode);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            treeView1.EndUpdate();

            if (treeView1.Nodes.Count > 0)
            {
                treeView1.SelectedNode = treeView1.Nodes[0];
                _currentSelect = (int)treeView1.Nodes[0].Tag;
            }

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

        private void AfterNodeSelect(object sender, TreeViewEventArgs e)
        {
            if (treeView1.SelectedNode == null)
            {
                return;
            }

            if (treeView1.SelectedNode.Parent == null)
            {
                CurrentSelect = (int)treeView1.SelectedNode.Tag;
                //CurrFrame = -1;
            }
            else
            {
                CurrentSelect = (int)treeView1.SelectedNode.Parent.Tag;
                CurrFrame = treeView1.SelectedNode.Index;
            }
        }

        private void AfterSelectTreeViewFrames(object sender, TreeViewEventArgs e)
        {
            CurrFrame = treeViewFrames.SelectedNode.Index;
            CurrentSelect = CurrentSelect;
        }

        private void OnClickExport(object sender, EventArgs e)
        {
            if (_exportForm?.IsDisposed == false)
            {
                _exportForm.Focus();
                return;
            }

            _exportForm = new AnimDataExportForm
            {
                TopMost = true
            };
            _exportForm.Show();
        }

        private void OnClickImport(object sender, EventArgs e)
        {
            if (_importForm?.IsDisposed == false)
            {
                _importForm.Focus();
                return;
            }

            _importForm = new AnimDataImportForm
            {
                TopMost = true,
                OnAfterImport = Reload
            };
            _importForm.Show();
        }

        private void OnClickStartStop(object sender, EventArgs e)
        {
            MainPictureBox.Animate = !MainPictureBox.Animate;

            animateToolStripMenuItem.Checked = !MainPictureBox.Animate;
        }

        //private void Timer_Tick(object sender, EventArgs e)
        //{
        //    ++_timerFrame;
        //    if (_timerFrame >= _selAnimdataEntry.FrameCount)
        //    {
        //        _timerFrame = 0;
        //    }

        //    var graphic = CurrentSelect + _selAnimdataEntry.FrameData[_timerFrame];
        //    toolStripStatusGraphic.Text = $"Graphic: {graphic} (0x{graphic:X})";

        //    if (_customHue == 0)
        //    {
        //        var bit = Art.GetStatic(graphic);
        //        Art.Measure(bit, out var xMin, out var yMin, out var xMax, out var yMax);
        //        MainPictureBox.Image = bit;
        //    }
        //    else
        //    {
        //        // In a lock, since GenerateHuedFrames() can write to this list.
        //        lock (_huedFrames)
        //        {
        //            MainPictureBox.Image = _huedFrames[_timerFrame];
        //        }
        //    }
        //}

        private void OnValueChangedStartDelay(object sender, EventArgs e)
        {
            if (_selAnimdataEntry == null)
            {
                return;
            }

            if (_selAnimdataEntry.FrameStart == (byte)numericUpDownStartDelay.Value)
            {
                return;
            }

            _selAnimdataEntry.FrameStart = (byte)numericUpDownStartDelay.Value;
            Options.ChangedUltimaClass["Animdata"] = true;
        }

        private void OnValueChangedFrameDelay(object sender, EventArgs e)
        {
            if (_selAnimdataEntry == null)
            {
                return;
            }

            if (_selAnimdataEntry.FrameInterval == (byte)numericUpDownFrameDelay.Value)
            {
                return;
            }

            _selAnimdataEntry.FrameInterval = (byte)numericUpDownFrameDelay.Value;
            //if (_mTimer != null)
            {
                MainPictureBox.FrameDelay = (100 * _selAnimdataEntry.FrameInterval) + 1;
            }

            Options.ChangedUltimaClass["Animdata"] = true;
        }

        private void OnClickFrameDown(object sender, EventArgs e)
        {
            if (_selAnimdataEntry == null)
            {
                return;
            }

            if (treeViewFrames.Nodes.Count <= 1)
            {
                return;
            }

            if (treeViewFrames.SelectedNode == null)
            {
                return;
            }

            int index = treeViewFrames.SelectedNode.Index;
            if (index >= _selAnimdataEntry.FrameCount - 1)
            {
                return;
            }

            sbyte temp = _selAnimdataEntry.FrameData[index];
            _selAnimdataEntry.FrameData[index] = _selAnimdataEntry.FrameData[index + 1];
            _selAnimdataEntry.FrameData[index + 1] = temp;

            TreeNode listNode = treeView1.SelectedNode.Parent ?? treeView1.SelectedNode;
            int frame = CurrentSelect + _selAnimdataEntry.FrameData[index];
            treeViewFrames.Nodes[index].Text = $"0x{frame:X4} {TileData.ItemTable[frame].Name}";
            listNode.Nodes[index].Text = $"0x{frame:X4} {TileData.ItemTable[frame].Name}";

            frame = CurrentSelect + _selAnimdataEntry.FrameData[index + 1];
            treeViewFrames.Nodes[index + 1].Text = $"0x{frame:X4} {TileData.ItemTable[frame].Name}";
            listNode.Nodes[index + 1].Text = $"0x{frame:X4} {TileData.ItemTable[frame].Name}";

            treeViewFrames.SelectedNode = treeViewFrames.Nodes[index + 1];
            Options.ChangedUltimaClass["Animdata"] = true;
        }

        private void OnClickFrameUp(object sender, EventArgs e)
        {
            if (_selAnimdataEntry == null)
            {
                return;
            }

            if (treeViewFrames.Nodes.Count <= 1)
            {
                return;
            }

            if (treeViewFrames.SelectedNode == null)
            {
                return;
            }

            int index = treeViewFrames.SelectedNode.Index;
            if (index <= 0)
            {
                return;
            }

            sbyte temp = _selAnimdataEntry.FrameData[index];
            _selAnimdataEntry.FrameData[index] = _selAnimdataEntry.FrameData[index - 1];
            _selAnimdataEntry.FrameData[index - 1] = temp;

            TreeNode listNode = treeView1.SelectedNode.Parent ?? treeView1.SelectedNode;
            int frame = CurrentSelect + _selAnimdataEntry.FrameData[index];
            treeViewFrames.Nodes[index].Text = $"0x{frame:X4} {TileData.ItemTable[frame].Name}";
            listNode.Nodes[index].Text = $"0x{frame:X4} {TileData.ItemTable[frame].Name}";

            frame = CurrentSelect + _selAnimdataEntry.FrameData[index - 1];
            treeViewFrames.Nodes[index - 1].Text = $"0x{frame:X4} {TileData.ItemTable[frame].Name}";
            listNode.Nodes[index - 1].Text = $"0x{frame:X4} {TileData.ItemTable[frame].Name}";
            treeViewFrames.SelectedNode = treeViewFrames.Nodes[index - 1];

            Options.ChangedUltimaClass["Animdata"] = true;
        }

        private void OnTextChanged(object sender, EventArgs e)
        {
            bool canDone = Utils.ConvertStringToInt(textBoxAddFrame.Text, out int index);
            if (checkBoxRelative.Checked)
            {
                index += CurrentSelect;
            }

            if (index > Art.GetMaxItemId() || index < 0)
            {
                canDone = false;
            }

            if (canDone)
            {
                textBoxAddFrame.ForeColor = !Art.IsValidStatic(index) ? Color.Red : Color.Black;
            }
            else
            {
                textBoxAddFrame.ForeColor = Color.Red;
            }
        }

        private void OnCheckChange(object sender, EventArgs e)
        {
            OnTextChanged(this, EventArgs.Empty);
        }

        private void OnClickAdd(object sender, EventArgs e)
        {
            if (_selAnimdataEntry == null)
            {
                return;
            }

            bool canDone = Utils.ConvertStringToInt(textBoxAddFrame.Text, out int index);
            if (checkBoxRelative.Checked)
            {
                index += CurrentSelect;
            }

            if (index > Art.GetMaxItemId() || index < 0)
            {
                canDone = false;
            }

            if (!canDone || !Art.IsValidStatic(index))
            {
                return;
            }

            _selAnimdataEntry.FrameData[_selAnimdataEntry.FrameCount] = (sbyte)(index - CurrentSelect);
            _selAnimdataEntry.FrameCount++;

            TreeNode node = new TreeNode
            {
                Text = $"0x{index:X4} {TileData.ItemTable[index].Name}"
            };
            treeViewFrames.Nodes.Add(node);

            TreeNode subNode = new TreeNode
            {
                Tag = _selAnimdataEntry.FrameCount - 1,
                Text = $"0x{index:X4} {TileData.ItemTable[index].Name}"
            };

            if (treeView1.SelectedNode.Parent == null)
            {
                treeView1.SelectedNode.Nodes.Add(subNode);
            }
            else
            {
                treeView1.SelectedNode.Parent.Nodes.Add(subNode);
            }

            Options.ChangedUltimaClass["Animdata"] = true;
        }

        private void OnClickRemove(object sender, EventArgs e)
        {
            if (_selAnimdataEntry == null || treeViewFrames.SelectedNode == null)
            {
                return;
            }

            int index = treeViewFrames.SelectedNode.Index;
            int i;
            for (i = index; i < _selAnimdataEntry.FrameCount - 1; ++i)
            {
                _selAnimdataEntry.FrameData[i] = _selAnimdataEntry.FrameData[i + 1];
            }

            for (; i < _selAnimdataEntry.FrameData.Length; ++i)
            {
                _selAnimdataEntry.FrameData[i] = 0;
            }

            _selAnimdataEntry.FrameCount--;
            treeView1.BeginUpdate();
            treeViewFrames.BeginUpdate();
            treeViewFrames.Nodes.Clear();
            TreeNode node = treeView1.SelectedNode.Parent ?? treeView1.SelectedNode;
            node.Nodes.Clear();
            for (i = 0; i < _selAnimdataEntry.FrameCount; ++i)
            {
                int frame = CurrentSelect + _selAnimdataEntry.FrameData[i];
                if (Art.IsValidStatic(frame))
                {
                    TreeNode subNode = new TreeNode
                    {
                        Text = $"0x{frame:X4} {TileData.ItemTable[frame].Name}"
                    };
                    node.Nodes.Add(subNode);
                    treeViewFrames.Nodes.Add((TreeNode)subNode.Clone());
                }
                else
                {
                    break;
                }
            }
            treeViewFrames.EndUpdate();
            treeView1.EndUpdate();
            Options.ChangedUltimaClass["Animdata"] = true;
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Animdata.Save(Options.OutputPath);
            Cursor.Current = Cursors.Default;
            MessageBox.Show($"Saved to {Options.OutputPath}",
                "Save",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
            Options.ChangedUltimaClass["Animdata"] = false;
        }

        private void OnClickRemoveAnim(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null)
            {
                return;
            }

            Animdata.AnimData.Remove(CurrentSelect);
            Options.ChangedUltimaClass["Animdata"] = true;
            treeView1.SelectedNode.Remove();
        }

        private void OnClickNode(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                treeView1.SelectedNode = e.Node;
            }
        }

        private void OnTextChangeAdd(object sender, EventArgs e)
        {
            if (Utils.ConvertStringToInt(AddTextBox.Text, out int index, 0, Art.GetMaxItemId()))
            {
                AddTextBox.ForeColor = Animdata.GetAnimData(index) != null ? Color.Red : Color.Black;
            }
            else
            {
                AddTextBox.ForeColor = Color.Red;
            }
        }

        private void OnKeyDownAdd(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            if (!Utils.ConvertStringToInt(AddTextBox.Text, out int index, 0, Art.GetMaxItemId()))
            {
                return;
            }

            if (Animdata.GetAnimData(index) != null)
            {
                return;
            }

            Animdata.AnimData[index] = new Animdata.AnimdataEntry(new sbyte[64], 0, 1, 0, 0);
            TreeNode node = new TreeNode
            {
                Tag = index,
                Text = $"0x{index:X4} {TileData.ItemTable[index].Name}"
            };

            if ((TileData.ItemTable[index].Flags & TileFlag.Animation) == 0)
            {
                node.ForeColor = Color.Blue;
            }
            treeView1.Nodes.Add(node);

            TreeNode subNode = new TreeNode
            {
                Text = $"0x{index:X4} {TileData.ItemTable[index].Name}"
            };
            node.Nodes.Add(subNode);
            node.EnsureVisible();
            treeView1.SelectedNode = node;
            Options.ChangedUltimaClass["Animdata"] = true;
        }

        private void ContextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            var enabled = treeView1.SelectedNode.Parent == null;

            removeToolStripMenuItem.Enabled = enabled;
            addToolStripMenuItem.Enabled = enabled;
        }

        public void ChangeHue(int select)
        {
            select += 1;
            var newHue = select & ~0x8000;
            var newHueOnlyGray = (select & 0x8000) != 0;

            if (_customHue != newHue || _hueOnlyGray != newHueOnlyGray)
            {
                _customHue = newHue;
                _hueOnlyGray = newHueOnlyGray;
                toolStripStatusHue.Text = $"Hue: {_customHue}";
                SetPicture();
            }
        }

        private void OnClick_Hue(object sender, EventArgs e)
        {
            if (_showForm?.IsDisposed == false)
            {
                return;
            }

            _showForm = _customHue == 0
                ? new HuePopUpForm(ChangeHue, 1)
                : new HuePopUpForm(ChangeHue, _customHue - 1);

            _showForm.TopMost = true;
            _showForm.Show();
        }

        private void OnClick_ExportAsGif(object sender, EventArgs e)
        {
            if (_selAnimdataEntry != null)
            {
                var outputFile = Path.Combine(Options.OutputPath, $"AnimData 0x{_currentSelect:X}.gif");
                var delay = (100 * _selAnimdataEntry.FrameInterval) + 1;

                var maxFrameSize = new Size(0, 0);

                foreach (var frame in MainPictureBox.Frames)
                {
                    maxFrameSize.Width = Math.Max(maxFrameSize.Width, frame.Bitmap.Width);
                    maxFrameSize.Height = Math.Max(maxFrameSize.Height, frame.Bitmap.Height);
                }



                {
                    using var gif = AnimatedGif.AnimatedGif.Create(outputFile, delay: 150);
                    foreach (var frame in MainPictureBox.Frames)
                    {
                        if (frame == null || frame.Bitmap == null)
                        {
                            continue;
                        }

                        //Art.Measure(frame.Bitmap, out int xMin, out int yMin, out int xMax, out int yMax);
                        Point location = Point.Empty;
                        Size size = frame.Bitmap.Size;
                        location.X = maxFrameSize.Width - frame.Bitmap.Width; // (maxFrameSize.Width / 2) - frame.Center.X - xMin;
                        location.Y = maxFrameSize.Height - frame.Bitmap.Height; //  (maxFrameSize.Height / 2) - frame.Center.Y - frame.Bitmap.Height / 2; //  - frame.Bitmap.Height - yMin;
                        var destRect = new Rectangle(location, size);

                        using Bitmap target = new Bitmap(maxFrameSize.Width, maxFrameSize.Height);
                        using Graphics g = Graphics.FromImage(target);
                        //

                        //g.DrawImage(frame.Bitmap, maxFrameSize.Width - xMax,  );
                        //g.DrawImage(frame.Bitmap, 0, yMax - yMin); //  maxFrameSize.Height - yMax);

                        g.DrawRectangle(new Pen(Color.Red), destRect);

                        g.DrawImage(frame.Bitmap, destRect, 0, 0, frame.Bitmap.Width, frame.Bitmap.Height, GraphicsUnit.Pixel);

                        gif.AddFrame(target, delay: -1, quality: GifQuality.Bit8);
                    }
                }

                //if (!looping)
                //{
                //    using var stream = new FileStream(outputFile, FileMode.Open, FileAccess.Write);
                //    stream.Seek(28, SeekOrigin.Begin);
                //    stream.WriteByte(0);
                //}

                //{
                //    using var gif = AnimatedGif.AnimatedGif.Create(outputFile, delay);
                //    if (_customHue > 0)
                //    {
                //        // Not in a lock, since event runs on main thread, which is the only place it can be written.
                //        foreach (var huedFrame in MainPictureBox.Frames)
                //        {
                //            if (huedFrame != null)
                //            {
                //                gif.AddFrame(huedFrame.Bitmap, delay: -1, quality: GifQuality.Bit8);
                //            }
                //        }
                //    }
                //    else
                //    {
                //        for (int i = 0; i < _selAnimdataEntry.FrameCount; ++i)
                //        {
                //            int graphic = _currentSelect + _selAnimdataEntry.FrameData[i];
                //            gif.AddFrame(Art.GetStatic(graphic), delay: -1, quality: GifQuality.Bit8);
                //        }
                //    }
                //}

                MessageBox.Show($"Saved to {outputFile}");
            }
        }

        private bool Animate
        {
            get => MainPictureBox.Animate;
            set => MainPictureBox.Animate = value;
        }
        //private void SetPicture()
        //{
        //    _mainPicture?.Dispose();
        //    if (_currentSelect == 0)
        //    {
        //        return;
        //    }

        //    if (Animate)
        //    {
        //        _mainPicture = DoAnimation();
        //    }
        //    else
        //    {

        //        // if (hue != 0)
        //        // {
        //        //     _frames = Animations.GetAnimation(_currentSelect, _currentSelectAction, _facing, ref hue, true, false);
        //        // }
        //        // else
        //        // {
        //        //     _frames = Animations.GetAnimation(_currentSelect, _currentSelectAction, _facing, ref hue, false, false);
        //        //     _defHue = hue;
        //        // }

        //        // if (_frames != null)
        //        // {
        //        //     if (_frames[0].Bitmap != null)
        //        //     {
        //        //         _mainPicture = new Bitmap(_frames[0].Bitmap);
        //        //         BaseGraphicLabel.Text = $"BaseGraphic: {body} (0x{body:X})";
        //        //         GraphicLabel.Text = $"Graphic: {_currentSelect} (0x{_currentSelect:X})";
        //        //         HueLabel.Text = $"Hue: {hue + 1} (0x{hue + 1:X})";
        //        //     }
        //        //     else
        //        //     {
        //        //         _mainPicture = null;
        //        //     }
        //        // }
        //        // else
        //        {
        //            _mainPicture = null;
        //        }
        //    }
        //}

        //private Bitmap DoAnimation()
        //{
        //    if (_mTimer != null)
        //    {
        //        return _huedFrames[_curFrame] != null
        //            ? new Bitmap(_huedFrames[_curFrame])
        //            : null;
        //    }
        //    return null;

        //    // int body = _currentSelect;
        //    // Animations.Translate(ref body);
        //    // int hue = _customHue;
        //    // if (hue != 0)
        //    // {
        //    //     _frames = Animations.GetAnimation(_currentSelect, _currentSelectAction, _facing, ref hue, true, false);
        //    // }
        //    // else
        //    // {
        //    //     _frames = Animations.GetAnimation(_currentSelect, _currentSelectAction, _facing, ref hue, false, false);
        //    //     _defHue = hue;
        //    // }

        //    // if (_frames == null)
        //    // {
        //    //     return null;
        //    // }

        //    // BaseGraphicLabel.Text = $"BaseGraphic: {body} (0x{body:X})";
        //    // GraphicLabel.Text = $"Graphic: {_currentSelect} (0x{_currentSelect:X})";
        //    // HueLabel.Text = $"Hue: {hue + 1} (0x{hue + 1:X})";
        //    // int count = _frames.Length;
        //    // _animationList = new Bitmap[count];

        //    // for (int i = 0; i < count; ++i)
        //    // {
        //    //     _animationList[i] = _frames[i].Bitmap;
        //    // }

        //    // // TODO: avoid division by 0 - needs checking if this is valid logic for count.
        //    // if (count <= 0)
        //    // {
        //    //     count = 1;
        //    // }

        //    // _timer = new Timer
        //    // {
        //    //     Interval = 1000 / count
        //    // };
        //    // _timer.Tick += AnimTick;
        //    // _timer.Start();

        //    // _frameIndex = 0;

        //    // LoadListViewFrames(); // Reload FrameTab

        //    // return _animationList[0] != null ? new Bitmap(_animationList[0]) : null;
        //}
    }

    public class AnimdataSorter : IComparer
    {
        public int Compare(object x, object y)
        {
            TreeNode tx = x as TreeNode;
            TreeNode ty = y as TreeNode;
            if (tx.Parent != null)
            {
                return 0;
            }

            int ix = (int)tx.Tag;
            int iy = (int)ty.Tag;
            if (ix == iy)
            {
                return 0;
            }
            else if (ix < iy)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }
    }
}
