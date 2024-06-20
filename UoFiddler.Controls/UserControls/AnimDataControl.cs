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
        private int _currAnim;
        private int _curFrame;
        private Timer _mTimer;
        private int _timerFrame;
        private AnimDataImportForm _importForm;
        private AnimDataExportForm _exportForm;
        private HuePopUpForm _showForm;
        private int _customHue = 0;
        private bool _hueOnlyGray = false;

        // Must be non-null for synchronization. Can contain null values for invalid tiles.
        private List<Bitmap> _huedFrames = [];

        [Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private int CurrFrame
        {
            get => _curFrame;
            set
            {
                _curFrame = value;
                var graphic = _currAnim + (_curFrame == -1 ? 0 : _selAnimdataEntry.FrameData[_curFrame]);
                toolStripStatusGraphic.Text = $"Graphic: {graphic} (0x{graphic:X})";
            }
        }

        [Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private int CurrAnim
        {
            get => _currAnim;
            set
            {
                if (!_loaded)
                {
                    return;
                }

                _selAnimdataEntry = Animdata.AnimData[value];
                if (_currAnim != value)
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

                    _currAnim = value;

                    if (_customHue > 0)
                    {
                        GenerateFramedHues();
                    }
                }

                if (_mTimer == null)
                {
                    if (_customHue > 0)
                    {
                        int index = Math.Max(0, _curFrame);

                        // Not in a lock, as event handlers run on main thread, and the value-writing
                        // to _huedFrames above will not happen simultaneously as this read.
                        MainPictureBox.Image = _huedFrames[index];
                    }
                    else
                    {
                        int graphic = value;
                        if (_curFrame > -1)
                        {
                            graphic += _selAnimdataEntry.FrameData[_curFrame];
                        }

                        MainPictureBox.Image = Art.GetStatic(graphic);
                    }
                }
                numericUpDownFrameDelay.Value = _selAnimdataEntry.FrameInterval;
                numericUpDownStartDelay.Value = _selAnimdataEntry.FrameStart;
            }
        }

        private void GenerateFramedHues()
        {
            if (_selAnimdataEntry != null)
            {
                // In a lock, since the Timer may read values from the list
                lock (_huedFrames)
                {
                    foreach (var huedFrame in _huedFrames)
                    {
                        huedFrame?.Dispose();
                    }
                    _huedFrames.Clear();

                    for (int i = 0; i < _selAnimdataEntry.FrameCount; ++i)
                    {
                        int graphic = _currAnim + _selAnimdataEntry.FrameData[i];
                        var huedFrame = Art.GetStatic(graphic);
                        if (huedFrame != null)
                        {
                            huedFrame = new Bitmap(huedFrame);
                            Hue hueObject = Hues.List[_customHue - 1];
                            hueObject.ApplyTo(huedFrame, _hueOnlyGray);
                        }
                        _huedFrames.Add(huedFrame);
                    }
                }
            }
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
                _currAnim = (int)treeView1.Nodes[0].Tag;
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
                CurrAnim = (int)treeView1.SelectedNode.Tag;
                CurrFrame = -1;
            }
            else
            {
                CurrAnim = (int)treeView1.SelectedNode.Parent.Tag;
                CurrFrame = treeView1.SelectedNode.Index;
            }
        }

        private void AfterSelectTreeViewFrames(object sender, TreeViewEventArgs e)
        {
            CurrFrame = treeViewFrames.SelectedNode.Index;
            if (_mTimer != null)
            {
                StopTimer();
            }

            CurrAnim = CurrAnim;
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
            if (_mTimer != null)
            {
                StopTimer();
            }
            else if (_selAnimdataEntry != null)
            {
                _mTimer = new Timer
                {
                    Interval = (100 * _selAnimdataEntry.FrameInterval) + 1
                };
                _mTimer.Tick += Timer_Tick;
                _timerFrame = 0;
                _mTimer.Start();
            }

            animateToolStripMenuItem.Checked = _mTimer != null;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            ++_timerFrame;
            if (_timerFrame >= _selAnimdataEntry.FrameCount)
            {
                _timerFrame = 0;
            }

            var graphic = CurrAnim + _selAnimdataEntry.FrameData[_timerFrame];
            toolStripStatusGraphic.Text = $"Graphic: {graphic} (0x{graphic:X})";

            if (_customHue == 0)
            {
                var bit = Art.GetStatic(graphic);
                Art.Measure(bit, out var xMin, out var yMin, out var xMax, out var yMax);
                MainPictureBox.Image = bit;
            }
            else
            {
                // In a lock, since GenerateHuedFrames() can write to this list.
                lock (_huedFrames)
                {
                    MainPictureBox.Image = _huedFrames[_timerFrame];
                }
            }
        }

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
            if (_mTimer != null)
            {
                _mTimer.Interval = (100 * _selAnimdataEntry.FrameInterval) + 1;
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
            int frame = CurrAnim + _selAnimdataEntry.FrameData[index];
            treeViewFrames.Nodes[index].Text = $"0x{frame:X4} {TileData.ItemTable[frame].Name}";
            listNode.Nodes[index].Text = $"0x{frame:X4} {TileData.ItemTable[frame].Name}";

            frame = CurrAnim + _selAnimdataEntry.FrameData[index + 1];
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
            int frame = CurrAnim + _selAnimdataEntry.FrameData[index];
            treeViewFrames.Nodes[index].Text = $"0x{frame:X4} {TileData.ItemTable[frame].Name}";
            listNode.Nodes[index].Text = $"0x{frame:X4} {TileData.ItemTable[frame].Name}";

            frame = CurrAnim + _selAnimdataEntry.FrameData[index - 1];
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
                index += CurrAnim;
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
                index += CurrAnim;
            }

            if (index > Art.GetMaxItemId() || index < 0)
            {
                canDone = false;
            }

            if (!canDone || !Art.IsValidStatic(index))
            {
                return;
            }

            _selAnimdataEntry.FrameData[_selAnimdataEntry.FrameCount] = (sbyte)(index - CurrAnim);
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
                int frame = CurrAnim + _selAnimdataEntry.FrameData[i];
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

            Animdata.AnimData.Remove(CurrAnim);
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

        private void StopTimer()
        {
            if (_mTimer.Enabled)
            {
                _mTimer.Stop();
            }

            _mTimer.Dispose();
            _mTimer = null;
            _timerFrame = 0;
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
                if (_customHue != 0)
                {
                    GenerateFramedHues();
                }
                else
                {
                    // In a lock, since the Timer may read from this list.
                    lock (_huedFrames)
                    {
                        foreach (var huedFrame in _huedFrames)
                        {
                            huedFrame?.Dispose();
                        }
                        _huedFrames.Clear();
                    }
                }

                if (_selAnimdataEntry != null && _mTimer == null)
                {
                    if (_customHue > 0)
                    {
                        var frame = Math.Max(0, _curFrame);
                        MainPictureBox.Image = _huedFrames[frame];

                    }
                    else
                    {
                        var graphic = _currAnim + (_curFrame == -1 ? 0 : _selAnimdataEntry.FrameData[_curFrame]);
                        MainPictureBox.Image = Art.GetStatic(graphic);
                    }
                }
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
                var outputFile = Path.Combine(Options.OutputPath, $"AnimData 0x{_currAnim:X}.gif");
                var delay = (100 * _selAnimdataEntry.FrameInterval) + 1;

                {
                    using var gif = AnimatedGif.AnimatedGif.Create(outputFile, delay);
                    if (_customHue > 0)
                    {
                        // Not in a lock, since event runs on main thread, which is the only place it can be written.
                        foreach (var huedFrame in _huedFrames)
                        {
                            if (huedFrame != null)
                            {
                                gif.AddFrame(huedFrame, delay: -1, quality: GifQuality.Bit8);
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < _selAnimdataEntry.FrameCount; ++i)
                        {
                            int graphic = _currAnim + _selAnimdataEntry.FrameData[i];
                            gif.AddFrame(Art.GetStatic(graphic), delay: -1, quality: GifQuality.Bit8);
                        }
                    }
                }

                MessageBox.Show($"Saved to {outputFile}");
            }
        }

        private Bitmap _mainPicture;
        private bool _imageInvalidated = true;
        private bool _animate;
        private bool Animate
        {
            get => _animate;
            set
            {
                if (_animate == value)
                {
                    return;
                }

                _animate = value;
                //extractAnimationToolStripMenuItem.Visible = _animate;
                //StopAnimation();
                _imageInvalidated = true;
                MainPictureBox.Invalidate();
            }
        }
        private void SetPicture()
        {
            _mainPicture?.Dispose();
            if (_currAnim == 0)
            {
                return;
            }

            if (Animate)
            {
                _mainPicture = DoAnimation();
            }
            else
            {
                
                // if (hue != 0)
                // {
                //     _frames = Animations.GetAnimation(_currentSelect, _currentSelectAction, _facing, ref hue, true, false);
                // }
                // else
                // {
                //     _frames = Animations.GetAnimation(_currentSelect, _currentSelectAction, _facing, ref hue, false, false);
                //     _defHue = hue;
                // }

                // if (_frames != null)
                // {
                //     if (_frames[0].Bitmap != null)
                //     {
                //         _mainPicture = new Bitmap(_frames[0].Bitmap);
                //         BaseGraphicLabel.Text = $"BaseGraphic: {body} (0x{body:X})";
                //         GraphicLabel.Text = $"Graphic: {_currentSelect} (0x{_currentSelect:X})";
                //         HueLabel.Text = $"Hue: {hue + 1} (0x{hue + 1:X})";
                //     }
                //     else
                //     {
                //         _mainPicture = null;
                //     }
                // }
                // else
                {
                    _mainPicture = null;
                }
            }
        }

        private Bitmap DoAnimation()
        {
            if (_mTimer != null)
            {
                return _huedFrames[_curFrame] != null
                    ? new Bitmap(_huedFrames[_curFrame])
                    : null;
            }
            return null;

            // int body = _currentSelect;
            // Animations.Translate(ref body);
            // int hue = _customHue;
            // if (hue != 0)
            // {
            //     _frames = Animations.GetAnimation(_currentSelect, _currentSelectAction, _facing, ref hue, true, false);
            // }
            // else
            // {
            //     _frames = Animations.GetAnimation(_currentSelect, _currentSelectAction, _facing, ref hue, false, false);
            //     _defHue = hue;
            // }

            // if (_frames == null)
            // {
            //     return null;
            // }

            // BaseGraphicLabel.Text = $"BaseGraphic: {body} (0x{body:X})";
            // GraphicLabel.Text = $"Graphic: {_currentSelect} (0x{_currentSelect:X})";
            // HueLabel.Text = $"Hue: {hue + 1} (0x{hue + 1:X})";
            // int count = _frames.Length;
            // _animationList = new Bitmap[count];

            // for (int i = 0; i < count; ++i)
            // {
            //     _animationList[i] = _frames[i].Bitmap;
            // }

            // // TODO: avoid division by 0 - needs checking if this is valid logic for count.
            // if (count <= 0)
            // {
            //     count = 1;
            // }

            // _timer = new Timer
            // {
            //     Interval = 1000 / count
            // };
            // _timer.Tick += AnimTick;
            // _timer.Start();

            // _frameIndex = 0;

            // LoadListViewFrames(); // Reload FrameTab

            // return _animationList[0] != null ? new Bitmap(_animationList[0]) : null;
        }

       private void OnPaint_MainPicture(object sender, PaintEventArgs e)
        {
            if (_imageInvalidated)
            {
                SetPicture();
            }

            if (_mainPicture != null)
            {
                Point location = Point.Empty;
                Size size = _mainPicture.Size;
                // location.X = (MainPictureBox.Width / 2) - _frames[_frameIndex].Center.X;
                // location.Y = (MainPictureBox.Height / 2) - _frames[_frameIndex].Center.Y - _frames[_frameIndex].Bitmap.Height / 2;

                Rectangle destRect = new Rectangle(location, size);

                e.Graphics.DrawImage(_mainPicture, destRect, 0, 0, _mainPicture.Width, _mainPicture.Height, GraphicsUnit.Pixel);
            }
            else
            {
                _mainPicture = null;
            }
        }

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
