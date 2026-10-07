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

            _refMarker = this;
        }

        private static AnimDataControl _refMarker;
        private static bool _loaded;
        private Animdata.AnimdataEntry _selAnimdataEntry;
        private int _currentSelect;
        private int _currentFrame;
        private AnimDataImportForm _importForm;
        private AnimDataExportForm _exportForm;
        private HuePopUpForm _showForm;
        private int _customHue = 0;
        private bool _hueOnlyGray = false;
        private Func<string, string?>? _localizationGetter;
        private string _baseGraphicLabelPrefix = "Base Graphic: ";
        private string _graphicLabelPrefix = "Graphic: ";
        private string _hueLabelPrefix = "Hue: ";

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private int CurrFrame
        {
            get => _currentFrame;
            set
            {
                if (_currentFrame != value)
                {
                    var newGraphic = _currentSelect + value;

                    toolStripStatusGraphic.Text = $"{_graphicLabelPrefix}{newGraphic} (0x{newGraphic:X})";
                    MainPictureBox.FrameIndex = value;
                    _currentFrame = value;
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

                    toolStripStatusBaseGraphic.Text = $"{_baseGraphicLabelPrefix}{value} (0x{value:X})";
                    toolStripStatusGraphic.Text = $"{_graphicLabelPrefix}{value} (0x{value:X})";

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

            toolStripStatusGraphic.Text = $"{_graphicLabelPrefix}{newGraphic} (0x{newGraphic:X})";
        }

        private void SetPicture()
        {
            if (_selAnimdataEntry == null)
            {
                return;
            }

            List<AnimatedFrame> frames = [];
            Size maxImageSize = new Size(0, 0);

            // Each frame's lower edge will be drawn at the same position, taking the height of the largest frame.
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

                // The frame's left edge will be padding with enough space to compensate for the width of the largest frame.
                var center = new Point((frame.Width - maxImageSize.Width) / 2, 0);

                if (_customHue > 0)
                {
                    frame = new Bitmap(frame);
                    Hue hueObject = Hues.List[_customHue - 1];
                    hueObject.ApplyTo(frame, _hueOnlyGray);
                }
                else
                {
                    // Art.GetStatic returns cache-owned bitmaps; clone so the
                    // picture box can own and dispose its frames safely.
                    frame = new Bitmap(frame);
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
                MainPictureBox.Reset();
                animateToolStripMenuItem.Checked = false;
                showFrameBoundsToolStripMenuItem.Checked = false;
                _loaded = false;
                OnLoad(this, EventArgs.Empty);
            }
        }

        private void OnLoad(object sender, EventArgs e)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            if (_loaded)
            {
                return;
            }

            using (new WaitCursorScope(this))
            {
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
                        node.ForeColor = Options.DarkMode ? Color.OrangeRed : Color.Red;
                    }
                    else if ((TileData.ItemTable[id].Flags & TileFlag.Animation) == 0)
                    {
                        node.ForeColor = Options.DarkMode ? Color.CornflowerBlue : Color.Blue;
                    }

                    // TODO: find a better approach to this
                    // we need to fix invalid entries as there cannot be more than 64 frames
                    if (animdataEntry.FrameCount > 64)
                    {
                        animdataEntry.FrameCount = 64;
                        Options.ChangedUltimaClass["Animdata"] = true;
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
            }
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
            _exportForm.SetLocalization(_localizationGetter);
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
            _importForm.SetLocalization(_localizationGetter);
            _importForm.Show();
        }

        private void OnClickStartStop(object sender, EventArgs e)
        {
            MainPictureBox.Animate = !MainPictureBox.Animate;

            animateToolStripMenuItem.Checked = MainPictureBox.Animate;
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
                textBoxAddFrame.ForeColor = !Art.IsValidStatic(index)
                    ? (Options.DarkMode ? Color.OrangeRed : Color.Red)
                    : (Options.DarkMode ? Color.White : Color.Black);
            }
            else
            {
                textBoxAddFrame.ForeColor = Options.DarkMode ? Color.OrangeRed : Color.Red;
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
            using (new WaitCursorScope(this))
            {
                Animdata.Save(Options.OutputPath);
                Options.ChangedUltimaClass["Animdata"] = false;
            }

            // 获取汉化的保存成功消息
            string message = _localizationGetter?.Invoke("Forms.FileSavedDialog.SaveSuccess") ?? "File saved successfully.";
            string title = _localizationGetter?.Invoke("Forms.FileSavedDialog.Title") ?? "Save Success";
            
            FileSavedDialog.Show(FindForm(), Options.OutputPath, message, title, _localizationGetter ?? (k => null));
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
                AddTextBox.ForeColor = Animdata.GetAnimData(index) != null
                    ? (Options.DarkMode ? Color.OrangeRed : Color.Red)
                    : (Options.DarkMode ? Color.White : Color.Black);
            }
            else
            {
                AddTextBox.ForeColor = Options.DarkMode ? Color.OrangeRed : Color.Red;
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
                node.ForeColor = Options.DarkMode ? Color.CornflowerBlue : Color.Blue;
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

            _showForm.SetLocalization(key => _localizationGetter?.Invoke(key));
            _showForm.TopMost = true;
            _showForm.Show();
        }

        private void OnClick_ExportAsGif(object sender, EventArgs e)
        {
            if (_selAnimdataEntry != null)
            {
                var outputFile = Path.Combine(Options.OutputPath, $"AnimData {Utils.FormatExportId(_currentSelect)}.gif");
                MainPictureBox.Frames.ToGif(outputFile, delay: 150, showFrameBounds: MainPictureBox.ShowFrameBounds);
                
                var title = _localizationGetter?.Invoke("Forms.AnimDataControl.ExportGifDialog.Title") ?? "Saved";
                var message = _localizationGetter?.Invoke("Forms.AnimDataControl.ExportGifDialog.Message") ?? "Saved to {0}";
                message = string.Format(message, outputFile);
                
                MessageBox.Show(message, title);
            }
        }

        private void OnClickShowFrameBounds(object sender, EventArgs e)
        {
            MainPictureBox.ShowFrameBounds = !MainPictureBox.ShowFrameBounds;
            showFrameBoundsToolStripMenuItem.Checked = MainPictureBox.ShowFrameBounds;
        }

        private void SearchByIdToolStripTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (!Utils.ConvertStringToInt(searchByIdToolStripTextBox.Text, out int indexValue, 0, Art.GetMaxItemId()))
            {
                return;
            }

            foreach (TreeNode node in treeView1.Nodes)
            {
                if ((int)node.Tag != indexValue)
                {
                    continue;
                }

                treeView1.SelectedNode = node;
                node.EnsureVisible();
                return;
            }
        }

        /// <summary>
        /// 设置本地化（从 MainForm 调用）
        /// </summary>
        public void SetLocalization(Func<string, string?> getLocalized)
        {
            _localizationGetter = getLocalized;
            ApplyLocalization();
        }

        /// <summary>
        /// 应用汉化
        /// </summary>
        private void ApplyLocalization()
        {
            if (_localizationGetter == null) return;

            // GroupBox 标题
            groupBox1.Text = _localizationGetter("Forms.AnimDataControl.PreviewGroupBox") ?? "Preview";
            groupBox2.Text = _localizationGetter("Forms.AnimDataControl.DataGroupBox") ?? "Data";
            groupBox4.Text = _localizationGetter("Forms.AnimDataControl.FramesGroupBox") ?? "Frames";

            // Label
            label1.Text = _localizationGetter("Forms.AnimDataControl.StartDelayLabel") ?? "Start Delay";
            label2.Text = _localizationGetter("Forms.AnimDataControl.FrameDelayLabel") ?? "Frame Delay";

            // Button
            button2.Text = _localizationGetter("Forms.AnimDataControl.AddButton") ?? "Add";
            button5.Text = _localizationGetter("Forms.AnimDataControl.RemoveButton") ?? "Remove";
            button6.Text = _localizationGetter("Forms.AnimDataControl.SaveButton") ?? "Save";
            button7.Text = _localizationGetter("Forms.AnimDataControl.ImportButton") ?? "Import...";
            button8.Text = _localizationGetter("Forms.AnimDataControl.ExportButton") ?? "Export...";

            // ToolStripMenuItem
            addToolStripMenuItem.Text = _localizationGetter("Forms.AnimDataControl.AddMenuItem") ?? "Add";
            removeToolStripMenuItem.Text = _localizationGetter("Forms.AnimDataControl.RemoveMenuItem") ?? "Remove";
            hueToolStripMenuItem.Text = _localizationGetter("Forms.AnimDataControl.HueMenuItem") ?? "Hue";
            animateToolStripMenuItem.Text = _localizationGetter("Forms.AnimDataControl.AnimateMenuItem") ?? "Animate";
            showFrameBoundsToolStripMenuItem.Text = _localizationGetter("Forms.AnimDataControl.ShowFrameBoundsMenuItem") ?? "Show frame bounds";
            exportAsAnimatedGifToolStripMenuItem.Text = _localizationGetter("Forms.AnimDataControl.ExportGifMenuItem") ?? "Export as animated Gif";

            // ToolStrip
            searchByIdToolStripLabel.Text = _localizationGetter("Forms.AnimDataControl.IndexLabel") ?? "Index:";
            toolStripDropDownButton1.Text = _localizationGetter("Forms.AnimDataControl.SettingsButton") ?? "Settings";

            // CheckBox
            checkBoxRelative.Text = _localizationGetter("Forms.AnimDataControl.RelativeCheckBox") ?? "Relative";

            // 状态栏标签前缀
            _baseGraphicLabelPrefix = _localizationGetter("Forms.AnimDataControl.BaseGraphicLabel") ?? "Base Graphic: ";
            _graphicLabelPrefix = _localizationGetter("Forms.AnimDataControl.GraphicLabel") ?? "Graphic: ";
            _hueLabelPrefix = _localizationGetter("Forms.AnimDataControl.HueLabel") ?? "Hue: ";
            
            // 设置初始状态栏显示
            toolStripStatusBaseGraphic.Text = $"{_baseGraphicLabelPrefix}0 (0x0)";
            toolStripStatusGraphic.Text = $"{_graphicLabelPrefix}0 (0x0)";
            toolStripStatusHue.Text = _hueLabelPrefix + "0";
        }

        public static bool Select(int graphic)
        {
            if (_refMarker == null)
            {
                return false;
            }

            if (!_loaded)
            {
                _refMarker.OnLoad(_refMarker, EventArgs.Empty);
            }

            TabPageNavigator.ActivateOwningTabPage(_refMarker);

            foreach (TreeNode node in _refMarker.treeView1.Nodes)
            {
                if ((int)node.Tag != graphic)
                {
                    continue;
                }

                _refMarker.treeView1.SelectedNode = node;
                node.EnsureVisible();
                _refMarker.treeView1.Focus();
                return true;
            }

            return false;
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
