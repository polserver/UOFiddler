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
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.Forms
{
    public partial class AnimationListNewEntriesForm : Form
    {
        private readonly string[][] _actionNames;
        private readonly Func<int, bool> _isAlreadyDefinedCallback;
        private readonly Action<int, int, string> _addGraphicAction;

        private int _currentSelect;
        private int _facing = 1;
        private bool _animate;
        private Timer _timer;
        private int _frameIndex;
        private AnimationFrame[] _animation;

        public AnimationListNewEntriesForm(Func<int, bool> isAlreadyDefinedCallback,
            Action<int, int, string> addGraphicAction, string[][] actionNames)
        {
            InitializeComponent();

            Icon = Options.GetFiddlerIcon();

            _actionNames = actionNames;
            _addGraphicAction = addGraphicAction;
            _isAlreadyDefinedCallback = isAlreadyDefinedCallback;
        }

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

                StopAnimation();

                if (_animate)
                {
                    SetAnimation();
                }
            }
        }

        private int CurrentSelectAction { get; set; }

        private int CurrentSelect
        {
            get => _currentSelect;
            set
            {
                StopAnimation();
                _currentSelect = value;
                if (_animate)
                {
                    SetAnimation();
                }

                pictureBox1.Invalidate();
            }
        }

        private void StopAnimation()
        {
            if (_timer != null)
            {
                if (_timer.Enabled)
                {
                    _timer.Stop();
                }

                _timer.Dispose();
                _timer = null;
            }

            _animation = null;
            _frameIndex = 0;
        }

        private void SetAnimation()
        {
            int body = _currentSelect;
            Animations.Translate(ref body);

            int hue = 0;

            _animation = Animations.GetAnimation(_currentSelect, CurrentSelectAction, _facing, ref hue, false, false);

            if (_animation == null)
            {
                return;
            }

            _frameIndex = 0;
            _timer = new Timer
            {
                Interval = 1000 / _animation.Length
            };

            _timer.Tick += AnimTick;
            _timer.Start();
        }

        private void AnimTick(object sender, EventArgs e)
        {
            ++_frameIndex;

            if (_frameIndex == _animation.Length)
            {
                _frameIndex = 0;
            }

            pictureBox1.Invalidate();
        }

        private void OnLoad(object sender, EventArgs e)
        {
            facingbar.Value = (_facing + 3) & 7;
            tvAnimationList.BeginUpdate();
            tvAnimationList.Nodes.Clear();
            tvAnimationList.TreeViewNodeSorter = new AnimNewListGraphicSorter();

            MobTypes();

            foreach (var key in BodyTable.Entries) //body.def
            {
                BodyTableEntry entry = key.Value;
                if (AlreadyFound(entry.NewId))
                {
                    continue;
                }

                if (_isAlreadyDefinedCallback(entry.NewId))
                {
                    continue;
                }

                TreeNode node = new TreeNode(entry.NewId.ToString())
                {
                    Tag = entry.NewId,
                    ToolTipText = $"Found in body.def {Animations.GetFileName(entry.NewId)}"
                };
                node.Tag = new[] { entry.NewId, 0 };
                tvAnimationList.Nodes.Add(node);
                SetActionType(node, entry.NewId, 0);
            }

            if (BodyConverter.Table1 != null)
            {
                ProcessBodyConverterTable(BodyConverter.Table1);
            }

            if (BodyConverter.Table2 != null)
            {
                ProcessBodyConverterTable(BodyConverter.Table2);
            }

            if (BodyConverter.Table3 != null)
            {
                ProcessBodyConverterTable(BodyConverter.Table3);
            }

            if (BodyConverter.Table4 != null)
            {
                ProcessBodyConverterTable(BodyConverter.Table4);
            }

            tvAnimationList.EndUpdate();
        }

        private void ProcessBodyConverterTable(IEnumerable<int> table)
        {
            // bodyconv.def
            foreach (int entry in table)
            {
                if (entry == -1)
                {
                    continue;
                }

                if (AlreadyFound(entry))
                {
                    continue;
                }

                if (_isAlreadyDefinedCallback(entry))
                {
                    continue;
                }

                TreeNode node = new TreeNode(entry.ToString())
                {
                    ToolTipText = $"Found in bodyconv.def {Animations.GetFileName(entry)}",
                    Tag = entry
                };
                node.Tag = new[] { entry, 0 };
                tvAnimationList.Nodes.Add(node);
                SetActionType(node, entry, 0);
            }
        }

        private bool AlreadyFound(int graphic)
        {
            foreach (TreeNode node in tvAnimationList.Nodes)
            {
                if (((int[])node.Tag)[0] == graphic)
                {
                    return true;
                }
            }

            return false;
        }

        private void MobTypes()
        {
            string filePath = Files.GetFilePath("mobtypes.txt");

            if (filePath == null)
            {
                return;
            }

            using (StreamReader def = new StreamReader(filePath))
            {
                string line;

                while ((line = def.ReadLine()) != null)
                {
                    if ((line = line.Trim()).Length == 0 || line.StartsWith("#"))
                    {
                        continue;
                    }

                    try
                    {
                        string[] split = line.Split('\t');
                        if (int.TryParse(split[0], out int graphic) && !AlreadyFound(graphic) && !_isAlreadyDefinedCallback(graphic))
                        {
                            TreeNode node = new TreeNode(graphic.ToString())
                            {
                                ToolTipText = $"Found in mobtype.txt {Animations.GetFileName(graphic)}"
                            };

                            int type = 0;
                            switch (split[1])
                            {
                                case "MONSTER":
                                    type = 0;
                                    break;
                                case "SEA_MONSTER":
                                    type = 1;
                                    break;
                                case "ANIMAL":
                                    type = 2;
                                    break;
                                case "HUMAN":
                                case "EQUIPMENT":
                                    type = 3;
                                    break;
                            }

                            node.Tag = new[] { graphic, type };
                            tvAnimationList.Nodes.Add(node);
                            SetActionType(node, graphic, type);
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
        }

        private void SetActionType(TreeNode parent, int graphic, int type)
        {
            parent.Nodes.Clear();
            if (type == 4) // Equipment == Human
            {
                type = 3;
            }

            for (int i = 0; i < _actionNames[type].GetLength(0); ++i)
            {
                if (!Animations.IsActionDefined(graphic, i, 0))
                {
                    continue;
                }

                TreeNode node = new TreeNode($"{i} {_actionNames[type][i]}")
                {
                    Tag = i
                };
                parent.Nodes.Add(node);
            }
        }

        private void OnAfterSelectTreeView(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Parent == null)
            {
                CurrentSelectAction = 0;
                CurrentSelect = ((int[])e.Node.Tag)[0];
                ComboBoxActionType.SelectedIndex = ((int[])e.Node.Tag)[1];
            }
            else
            {
                CurrentSelectAction = (int)e.Node.Tag;
                CurrentSelect = ((int[])e.Node.Parent.Tag)[0];
                ComboBoxActionType.SelectedIndex = ((int[])e.Node.Parent.Tag)[1];
            }
        }

        private void OnPaint_PictureBox(object sender, PaintEventArgs e)
        {
            if (CurrentSelect == 0)
            {
                return;
            }

            if (_animate)
            {
                if (_animation?[_frameIndex].Bitmap == null)
                {
                    return;
                }

                Point loc = new Point
                {
                    X = (pictureBox1.Width - _animation[_frameIndex].Bitmap.Width) / 2,
                    Y = (pictureBox1.Height - _animation[_frameIndex].Bitmap.Height) / 2
                };
                e.Graphics.DrawImage(_animation[_frameIndex].Bitmap, loc);
            }
            else
            {
                int body = _currentSelect;
                Animations.Translate(ref body);
                int hue = 0;
                AnimationFrame[] frames = Animations.GetAnimation(_currentSelect, CurrentSelectAction, _facing, ref hue, false, false);
                if (frames?[0].Bitmap == null)
                {
                    return;
                }

                Point loc = new Point
                {
                    X = (pictureBox1.Width - frames[0].Bitmap.Width) / 2,
                    Y = (pictureBox1.Height - frames[0].Bitmap.Height) / 2
                };

                e.Graphics.DrawImage(frames[0].Bitmap, loc);
            }
        }

        private void OnClickAnimate(object sender, EventArgs e)
        {
            Animate = !Animate;
        }

        private void OnChangeType(object sender, EventArgs e)
        {
            if (tvAnimationList.SelectedNode == null)
            {
                return;
            }

            TreeNode node = tvAnimationList.SelectedNode;
            if (node.Parent != null)
            {
                node = node.Parent;
            }

            ((int[])node.Tag)[1] = ComboBoxActionType.SelectedIndex;

            SetActionType(node, ((int[])node.Tag)[0], ComboBoxActionType.SelectedIndex);
        }

        private void OnScrollFacing(object sender, EventArgs e)
        {
            _facing = (facingbar.Value - 3) & 7;

            CurrentSelect = CurrentSelect;
        }

        private void OnClickAdd(object sender, EventArgs e)
        {
            if (tvAnimationList.SelectedNode == null)
            {
                return;
            }

            TreeNode node = tvAnimationList.SelectedNode;
            if (node.Parent != null)
            {
                node = node.Parent;
            }

            _addGraphicAction(((int[])node.Tag)[0], ((int[])node.Tag)[1], node.Text);

            tvAnimationList.SelectedNode.Remove();
        }
    }

    public class AnimNewListGraphicSorter : IComparer
    {
        public int Compare(object x, object y)
        {
            TreeNode tx = x as TreeNode;
            TreeNode ty = y as TreeNode;
            if (tx.Parent != null)
            {
                return 0;
            }

            if (ty.Parent != null)
            {
                return 0;
            }

            int[] ix = (int[])tx.Tag;
            int[] iy = (int[])ty.Tag;
            if (ix[0] == iy[0])
            {
                return 0;
            }

            if (ix[0] < iy[0])
            {
                return -1;
            }

            return 1;
        }
    }
}
