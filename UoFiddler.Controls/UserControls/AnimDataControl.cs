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
            MainPictureBox.FrameChanged += MainPictureBox_FrameChanged;
        }

        private static bool _loaded;
        private Animdata.AnimdataEntry _selAnimdataEntry;
        private int _currentSelect;
        private int _currentFrame;
        private AnimDataImportForm _importForm;
        private AnimDataExportForm _exportForm;
        private HuePopUpForm _showForm;
        private int _customHue = 0;
        private bool _hueOnlyGray = false;

        [Browsable(false),
DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private int CurrFrame
        {
            get => _currentFrame;
            set
            {
                if (_currentFrame != value)
                {
                    var newGraphic = _currentSelect + value;

                    toolStripStatusGraphic.Text = $"Graphic: {newGraphic} (0x{newGraphic:X})";
                    MainPictureBox.FrameIndex = value;
                    _currentFrame = value;
                }
            }
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
                    toolStripStatusGraphic.Text = $"Graphic: {value} (0x{value:X})";

                    _currentSelect = value;

                    SetPicture();
                }
                numericUpDownFrameDelay.Value = _selAnimdataEntry.FrameInterval;
                numericUpDownStartDelay.Value = _selAnimdataEntry.FrameStart;
            }
        }

        private void MainPictureBox_FrameChanged(object sender, EventArgs e)
        {
            var newGraphic = _currentSelect + MainPictureBox.FrameIndex;

            toolStripStatusGraphic.Text = $"Graphic: {newGraphic} (0x{newGraphic:X})";
        }

        private void SetPicture()
        {
            if (_selAnimdataEntry == null)
            {
                return;
            }

            List<AnimatedFrame> frames = [];
            Size maxImageSize = new Size(0, 0);

            // Each frame's lower left corner will be drawn at the same position, taking the size of the largest frame.
            for (int i = 0; i < _selAnimdataEntry.FrameCount; ++i)
            {
                int graphic = _currentSelect + _selAnimdataEntry.FrameData[i];
                var frame = Art.GetStatic(graphic);
                if (frame == null)
                    continue;
                maxImageSize.Width = Math.Max(maxImageSize.Width, frame.Width);
                maxImageSize.Height = Math.Max(maxImageSize.Height, frame.Height);
            }

            for (int i = 0; i < _selAnimdataEntry.FrameCount; ++i)
            {
                int graphic = _currentSelect + _selAnimdataEntry.FrameData[i];
                var frame = Art.GetStatic(graphic);
                if (frame == null)
                {
                    continue;
                }

                var center = new Point((frame.Width - maxImageSize.Width) / 2, 0);

                if (_customHue > 0)
                {
                    frame = new Bitmap(frame);
                    Hue hueObject = Hues.List[_customHue - 1];
                    hueObject.ApplyTo(frame, _hueOnlyGray);
                }

                frames.Add(new AnimatedFrame(frame, center));
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
            MainPictureBox.FrameDelay = (100 * _selAnimdataEntry.FrameInterval) + 1;

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


                // Each frame's bottom edge will be drawn at the same position, taking the height of the largest frame in `maxImageSize`.
                // Determine the point where to draw each frame's left edge in `maxCenterX`.
                Size maxImageSize = new Size(0, 0);
                var maxCenterX = 0;
                foreach (var frame in MainPictureBox.Frames)
                {
                    maxImageSize.Width = Math.Max(maxImageSize.Width, frame.Bitmap.Width);
                    maxImageSize.Height = Math.Max(maxImageSize.Height, frame.Bitmap.Height);
                    maxCenterX = Math.Max(maxCenterX, frame.Center.X);
                }

                {
                    using var gif = AnimatedGif.AnimatedGif.Create(outputFile, delay: 150);
                    foreach (var frame in MainPictureBox.Frames)
                    {
                        if (frame?.Bitmap == null)
                        {
                            continue;
                        }

                        var location = new Point(
                            maxCenterX - frame.Center.X,
                            maxImageSize.Height - frame.Bitmap.Height);

                        using Bitmap target = new Bitmap(maxImageSize.Width, maxImageSize.Height);
                        using Graphics g = Graphics.FromImage(target);
                        g.DrawImage(frame.Bitmap, location);
#if DEBUG
                        g.DrawRectangle(new Pen(Color.Red), new Rectangle(location, frame.Bitmap.Size));
#endif
                        gif.AddFrame(target, delay: -1, quality: GifQuality.Bit8);
                    }
                }

                MessageBox.Show($"Saved to {outputFile}");
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
