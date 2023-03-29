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
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;
using UoFiddler.Controls.Helpers;

namespace UoFiddler.Controls.UserControls
{
    public partial class DressControl : UserControl
    {
        public DressControl()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);

            ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;

            _lastNodeIndex = 0;
            treeViewItems.HideSelection = false;
        }

        private static readonly int[] _drawOrder ={
            0x00,// - Background
            0x14,// - Back (Cloak)
            0x05,// - Chest Clothing/Female Chest Armor
            0x04,// - Pants
            0x03,// - Foot Covering/Armor
            0x18,// - Legs (inner)(Leg Armor)
            0x13,// - Arm Covering/Armor
            0x0D,// - Torso (inner)(Chest Armor)
            0x11,// - Torso (Middle)(Surcoat, Tunic, Full Apron, Sash)
            0x08,// - Ring
            0x09,// - Talisman
            0x0E,// - Bracelet
            0x07,// - Gloves
            0x17,// - Legs (outer)(Skirt/Kilt)
            0x0A,// - Neck Covering/Armor
            0x0B,// - Hair
            0x0C,// - Waist (Half-Apron)
            0x16,// - Torso (outer)(Robe)
            0x10,// - Facial Hair
            0x12,// - Earrings
            0x06,// - Head Covering/Armor
            0x01,// - Single-Hand item/weapon
            0x02,// - Two-Hand item/weapon (including Shield)
            0x15 // - BackPack
        };

        private static readonly int[] _drawOrder2 ={
            0x00,// - Background
            0x05,// - Chest Clothing/Female Chest Armor
            0x04,// - Pants
            0x03,// - Foot Covering/Armor
            0x18,// - Legs (inner)(Leg Armor)
            0x13,// - Arm Covering/Armor
            0x0D,// - Torso (inner)(Chest Armor)
            0x11,// - Torso (Middle)(Surcoat, Tunic, Full Apron, Sash)
            0x08,// - Ring
            0x09,// - Talisman
            0x0E,// - Bracelet
            0x07,// - Gloves
            0x17,// - Legs (outer)(Skirt/Kilt)
            0x0A,// - Neck Covering/Armor
            0x0B,// - Hair
            0x0C,// - Waist (Half-Apron)
            0x16,// - Torso (outer)(Robe)
            0x10,// - Facial Hair
            0x12,// - Earrings
            0x06,// - Head Covering/Armor
            0x01,// - Single-Hand item/weapon
            0x02,// - Two-Hand item/weapon (including Shield)
            0x14,// - Back (Cloak)
            0x15 // - BackPack
        };

        private readonly Point _drawPoint = new Point(0, 0);
        private Point _drawPointAni = new Point(100, 100);

        private object[] _layers = new object[25];
        private readonly bool[] _layerVisible = new bool[25];
        private bool _female;
        private bool _human = true;
        private bool _elve;
        private bool _gargoyle;
        private bool _showPd = true;
        private bool _animate;
        private Timer _mTimer;
        private Bitmap[] _animation;
        private int _mFrameIndex;
        private int _facing = 1;
        private int _action = 1;
        private bool _loaded;
        private readonly int[] _hues = new int[25];
        private int _mount;

        public void SetHue(int index, int color)
        {
            _hues[index] = color;
        }

        /// <summary>
        /// Reload when loaded
        /// </summary>
        private void Reload()
        {
            if (!_loaded)
            {
                return;
            }

            _loaded = false;
            _layers = new object[25];
            _female = false;
            _human = true;
            _elve = false;
            _gargoyle = false;
            _showPd = true;
            _animate = false;

            _facing = 1;
            _action = 1;

            if (_mTimer != null)
            {
                if (_mTimer.Enabled)
                {
                    _mTimer.Stop();
                }

                _mTimer.Dispose();
                _mTimer = null;
            }

            if (_animation != null)
            {
                foreach (var frame in _animation)
                {
                    frame?.Dispose();
                }
            }

            _animation = null;
            _mFrameIndex = 0;

            EquipTable.Initialize();
            GumpTable.Initialize();

            OnLoad(this, EventArgs.Empty);
        }

        private void OnLoad(object sender, EventArgs e)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            Options.LoadedUltimaClass["TileData"] = true;
            Options.LoadedUltimaClass["Art"] = true;
            Options.LoadedUltimaClass["Hues"] = true;
            Options.LoadedUltimaClass["Animations"] = true;
            Options.LoadedUltimaClass["Gumps"] = true;

            checkBoxGargoyle.Visible = Art.IsUOAHS();

            extractAnimationToolStripMenuItem.Visible = false;
            DressPic.Image = new Bitmap(DressPic.Width, DressPic.Height);
            pictureBoxDress.Image = new Bitmap(pictureBoxDress.Width, pictureBoxDress.Height);

            checkedListBoxWear.BeginUpdate();
            checkedListBoxWear.Items.Clear();
            for (int i = 0; i < _layers.Length; ++i)
            {
                _layers[i] = 0;
                checkedListBoxWear.Items.Add($"0x{i:X2}", true);
                _layerVisible[i] = true;
            }
            checkedListBoxWear.EndUpdate();

            checkBoxHuman.Checked = true;
            checkBoxElve.Checked = false;
            checkBoxGargoyle.Checked = false;
            checkBoxfemale.Checked = false;

            groupBoxAnimate.Visible = false;
            animateToolStripMenuItem.Visible = false;
            FacingBar.Value = (_facing + 3) & 7;
            ActionBar.Value = _action;
            toolTip1.SetToolTip(FacingBar, FacingBar.Value.ToString());
            BuildDressList();
            DrawPaperdoll();
            _loaded = true;
            Cursor.Current = Cursors.Default;
        }

        private void OnFilePathChangeEvent()
        {
            Reload();
        }

        private void DrawPaperdoll()
        {
            if (!_showPd)
            {
                DrawAnimation();
                return;
            }
            using (Graphics graphPic = Graphics.FromImage(DressPic.Image))
            {
                graphPic.Clear(Color.Black);
                if (_layerVisible[0])
                {
                    Bitmap background = null;
                    if (!_female)
                    {
                        if (_human)
                        {
                            background = new Bitmap(Gumps.GetGump(0xC));
                        }
                        if (_elve)
                        {
                            background = new Bitmap(Gumps.GetGump(0xE));
                        }
                        if (_gargoyle)
                        {
                            background = new Bitmap(Gumps.GetGump(0x29A));
                        }
                    }
                    else
                    {
                        if (_human)
                        {
                            background = new Bitmap(Gumps.GetGump(0xD));
                        }
                        if (_elve)
                        {
                            background = new Bitmap(Gumps.GetGump(0xF));
                        }
                        if (_gargoyle)
                        {
                            background = new Bitmap(Gumps.GetGump(0x299));
                        }
                    }

                    if (background != null)
                    {
                        if (_hues[0] > 0)
                        {
                            Bitmap b = new Bitmap(background);
                            int hue = _hues[0];
                            bool gray = false;
                            if ((hue & 0x8000) != 0)
                            {
                                hue ^= 0x8000;
                                gray = true;
                            }
                            Hues.List[hue].ApplyTo(b, gray);
                            background = b;
                        }
                        graphPic.DrawImage(background, _drawPoint);
                    }
                }
                for (int i = 1; i < _drawOrder.Length; ++i)
                {
                    if ((int)_layers[_drawOrder[i]] == 0)
                    {
                        continue;
                    }

                    if (!_layerVisible[_drawOrder[i]])
                    {
                        continue;
                    }

                    int ani = TileData.ItemTable[(int)_layers[_drawOrder[i]]].Animation;
                    int gump = ani + 50000;
                    int hue = 0;

                    ConvertBody(ref ani, ref gump, ref hue);

                    if (_female)
                    {
                        gump += 10000;
                        if (!Gumps.IsValidIndex(gump))  // female gump.def entry?
                        {
                            ConvertGump(ref gump, ref hue);
                        }

                        if (!Gumps.IsValidIndex(gump)) // nope so male gump
                        {
                            gump -= 10000;
                        }
                    }

                    if (!Gumps.IsValidIndex(gump)) // male (or invalid female)
                    {
                        ConvertGump(ref gump, ref hue);
                    }

                    if (!Gumps.IsValidIndex(gump))
                    {
                        continue;
                    }

                    Bitmap bmp = new Bitmap(Gumps.GetGump(gump));
                    if (_hues[_drawOrder[i]] > 0)
                    {
                        hue = _hues[_drawOrder[i]];
                    }

                    bool onlyHueGrayPixels = (hue & 0x8000) != 0;
                    hue = (hue & 0x3FFF) - 1;
                    if (hue >= 0 && hue < Hues.List.Length)
                    {
                        Hue hueObject = Hues.List[hue];
                        hueObject.ApplyTo(bmp, onlyHueGrayPixels);
                    }
                    graphPic.DrawImage(bmp, _drawPoint);
                }
            }
            DressPic.Invalidate();
        }

        private void DrawAnimation()
        {
            if (_animate)
            {
                DoAnimation();
                return;
            }
            using (Graphics graphPic = Graphics.FromImage(DressPic.Image))
            {
                graphPic.Clear(Color.WhiteSmoke);
                int hue = 0;
                int back = 0;
                if (_layerVisible[0])
                {
                    if (!_female)
                    {
                        if (_human)
                        {
                            back = 400;
                        }
                        if (_elve)
                        {
                            back = 605;
                        }
                        if (_gargoyle)
                        {
                            back = 666;
                        }
                    }
                    else
                    {
                        if (_human)
                        {
                            back = 401;
                        }
                        if (_elve)
                        {
                            back = 606;
                        }
                        if (_gargoyle)
                        {
                            back = 667;
                        }
                    }
                }
                AnimationFrame[] background;
                if (_hues[0] > 0)
                {
                    hue = _hues[0];
                    background = Animations.GetAnimation(back, _action, _facing, ref hue, true, true);
                }
                else
                {
                    background = Animations.GetAnimation(back, _action, _facing, ref hue, false, true);
                }

                Point draw = new Point();
                if (_mount != 0)
                {
                    if (_action >= 23 && _action <= 29) //mount animations
                    {
                        int mountAction;
                        switch (_action)
                        {
                            case 23:
                                mountAction = 0;
                                break;
                            case 24:
                                mountAction = 1;
                                break;
                            case 25:
                                mountAction = 2;
                                break;
                            default:
                                mountAction = 5;
                                break;
                        }
                        if (Animations.IsActionDefined(_mount, mountAction, _facing))
                        {
                            hue = 0;
                            AnimationFrame[] mountFrame = Animations.GetAnimation(_mount, mountAction, _facing, ref hue, false, false);
                            if (mountFrame.Length > 0 && mountFrame[0].Bitmap != null)
                            {
                                draw.X = _drawPointAni.X - mountFrame[0].Center.X;
                                draw.Y = _drawPointAni.Y - mountFrame[0].Center.Y - mountFrame[0].Bitmap.Height;
                                graphPic.DrawImage(mountFrame[0].Bitmap, draw);
                            }
                        }
                    }
                }
                if (background != null)
                {
                    draw.X = _drawPointAni.X - background[0].Center.X;
                    draw.Y = _drawPointAni.Y - background[0].Center.Y - background[0].Bitmap.Height;
                    graphPic.DrawImage(background[0].Bitmap, draw);
                }
                int[] animOrder = _drawOrder2;
                if (((_facing - 3) & 7) >= 4 && ((_facing - 3) & 7) <= 6)
                {
                    animOrder = _drawOrder;
                }

                for (int i = 1; i < _drawOrder.Length; ++i)
                {
                    if ((int)_layers[animOrder[i]] == 0 || !_layerVisible[animOrder[i]])
                    {
                        continue;
                    }

                    if (TileData.ItemTable == null)
                    {
                        break;
                    }

                    int ani = TileData.ItemTable[(int)_layers[animOrder[i]]].Animation;
                    int gump = ani + 50000;
                    hue = 0;
                    ConvertBody(ref ani, ref gump, ref hue);
                    if (!Animations.IsActionDefined(ani, _action, _facing))
                    {
                        continue;
                    }

                    AnimationFrame[] frames;
                    if (_hues[animOrder[i]] > 0)
                    {
                        hue = _hues[animOrder[i]];
                        frames = Animations.GetAnimation(ani, _action, _facing, ref hue, true, true);
                    }
                    else
                    {
                        frames = Animations.GetAnimation(ani, _action, _facing, ref hue, false, true);
                    }

                    Bitmap bmp = frames[0].Bitmap;
                    if (bmp == null)
                    {
                        continue;
                    }

                    draw.X = _drawPointAni.X - frames[0].Center.X;
                    draw.Y = _drawPointAni.Y - frames[0].Center.Y - frames[0].Bitmap.Height;

                    graphPic.DrawImage(bmp, draw);
                }
            }
            DressPic.Invalidate();
        }

        private void DoAnimation()
        {
            if (_mTimer != null)
            {
                return;
            }

            int hue = 0;
            int back = 0;
            if (!_female)
            {
                if (_human)
                {
                    back = 400;
                }
                if (_elve)
                {
                    back = 605;
                }
                if (_gargoyle)
                {
                    back = 666;
                }
            }
            else
            {
                if (_human)
                {
                    back = 401;
                }
                if (_elve)
                {
                    back = 606;
                }
                if (_gargoyle)
                {
                    back = 667;
                }
            }

            AnimationFrame[] mobile;
            if (_hues[0] > 0)
            {
                hue = _hues[0];
                mobile = Animations.GetAnimation(back, _action, _facing, ref hue, true, false);
            }
            else
            {
                mobile = Animations.GetAnimation(back, _action, _facing, ref hue, false, false);
            }

            Point draw = new Point();

            int count = mobile.Length;
            _animation = new Bitmap[count];
            int[] animOrder = _drawOrder2;
            if (((_facing - 3) & 7) >= 4 && ((_facing - 3) & 7) <= 6)
            {
                animOrder = _drawOrder;
            }

            for (int i = 0; i < count; ++i)
            {
                _animation[i] = new Bitmap(DressPic.Width, DressPic.Height);
                using (Graphics graph = Graphics.FromImage(_animation[i]))
                {
                    graph.Clear(Color.WhiteSmoke);
                    if (_mount != 0)
                    {
                        if (_action >= 23 && _action <= 29) //mount animations
                        {
                            int mountAction;
                            switch (_action)
                            {
                                case 23:
                                    mountAction = 0;
                                    break;
                                case 24:
                                    mountAction = 1;
                                    break;
                                case 25:
                                    mountAction = 2;
                                    break;
                                default:
                                    mountAction = 5;
                                    break;
                            }
                            if (Animations.IsActionDefined(_mount, mountAction, _facing))
                            {
                                hue = 0;
                                AnimationFrame[] mountFrame = Animations.GetAnimation(_mount, mountAction, _facing, ref hue, false, false);
                                if (mountFrame.Length > i && mountFrame[i].Bitmap != null)
                                {
                                    draw.X = _drawPointAni.X - mountFrame[i].Center.X;
                                    draw.Y = _drawPointAni.Y - mountFrame[i].Center.Y - mountFrame[i].Bitmap.Height;
                                    graph.DrawImage(mountFrame[i].Bitmap, draw);
                                }
                            }
                        }
                    }
                    draw.X = _drawPointAni.X - mobile[i].Center.X;
                    draw.Y = _drawPointAni.Y - mobile[i].Center.Y - mobile[i].Bitmap.Height;
                    graph.DrawImage(mobile[i].Bitmap, draw);
                    for (int j = 1; j < animOrder.Length; ++j)
                    {
                        if ((int)_layers[animOrder[j]] == 0 || !_layerVisible[animOrder[j]])
                        {
                            continue;
                        }

                        int ani = TileData.ItemTable[(int)_layers[animOrder[j]]].Animation;
                        int gump = ani + 50000;
                        hue = 0;
                        ConvertBody(ref ani, ref gump, ref hue);
                        if (!Animations.IsActionDefined(ani, _action, _facing))
                        {
                            continue;
                        }

                        AnimationFrame[] frames;
                        if (_hues[animOrder[j]] > 0)
                        {
                            hue = _hues[animOrder[j]];
                            frames = Animations.GetAnimation(ani, _action, _facing, ref hue, true, false);
                        }
                        else
                        {
                            frames = Animations.GetAnimation(ani, _action, _facing, ref hue, false, false);
                        }

                        if (frames.Length < i || frames[i].Bitmap == null)
                        {
                            continue;
                        }

                        draw.X = _drawPointAni.X - frames[i].Center.X;
                        draw.Y = _drawPointAni.Y - frames[i].Center.Y - frames[i].Bitmap.Height;

                        graph.DrawImage(frames[i].Bitmap, draw);
                    }
                }
            }
            _mFrameIndex = 0;
            _mTimer = new Timer
            {
                Interval = 150// 1000 / count;
            };
            _mTimer.Tick += AnimTick;
            _mTimer.Start();
        }

        private void AnimTick(object sender, EventArgs e)
        {
            ++_mFrameIndex;

            if (_mFrameIndex >= _animation.Length)
            {
                _mFrameIndex = 0;
            }

            if (_animation?[_mFrameIndex] == null)
            {
                return;
            }

            using (Graphics graph = Graphics.FromImage(DressPic.Image))
            {
                graph.DrawImage(_animation[_mFrameIndex], _drawPoint);
            }
            DressPic.Invalidate();
        }

        private void AfterSelectTreeView(object sender, TreeViewEventArgs e)
        {
            int ani = TileData.ItemTable[(int)e.Node.Tag].Animation;
            int gump = ani + 50000;
            int gumpOrig = gump;
            int hue = 0;

            Animations.Translate(ref ani);

            ConvertBody(ref ani, ref gump, ref hue);

            if (_female)
            {
                gump += 10000;

                if (!Gumps.IsValidIndex(gump))  // female gump.def entry?
                {
                    ConvertGump(ref gump, ref hue);
                }

                if (!Gumps.IsValidIndex(gump)) // nope so male gump
                {
                    gump -= 10000;
                }
            }

            if (!Gumps.IsValidIndex(gump)) // male (or invalid female)
            {
                ConvertGump(ref gump, ref hue);
            }

            using (Graphics graph = Graphics.FromImage(pictureBoxDress.Image))
            {
                graph.Clear(Color.Transparent);

                Bitmap bmp = Gumps.GetGump(gump);

                if (bmp != null)
                {
                    bool onlyHueGrayPixels = (hue & 0x8000) != 0;

                    hue = (hue & 0x3FFF) - 1;

                    if (hue >= 0 && hue < Hues.List.Length)
                    {
                        Hue hueObject = Hues.List[hue];
                        hueObject.ApplyTo(bmp, onlyHueGrayPixels);
                    }

                    graph.DrawImage(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));
                }
            }

            pictureBoxDress.Invalidate();

            TextBox.Clear();
            TextBox.AppendText(
                $"Objtype: 0x{(int)e.Node.Tag:X4}\nLayer: 0x{TileData.ItemTable[(int)e.Node.Tag].Quality:X2}\n");
            TextBox.AppendText($"GumpID: 0x{gump:X4} (0x{gumpOrig:X4})\nHue: {hue + 1}\n");
            TextBox.AppendText($"Animation: 0x{ani:X4} (0x{TileData.ItemTable[(int)e.Node.Tag].Animation:X4})\n");
            TextBox.AppendText(
                $"ValidGump: {Gumps.IsValidIndex(gump)}\nValidAnim: {Animations.IsActionDefined(ani, 0, 0)}\n");
            TextBox.AppendText(
                $"ValidLayer: {Array.IndexOf(_drawOrder, TileData.ItemTable[(int)e.Node.Tag].Quality) != -1}");
        }

        private void OnClick_Animate(object sender, EventArgs e)
        {
            _animate = !_animate;

            extractAnimationToolStripMenuItem.Visible = _animate;

            RefreshDrawing();
        }

        private void OnChangeFemale(object sender, EventArgs e)
        {
            _female = !_female;

            if (_loaded)
            {
                RefreshDrawing();
            }
        }

        private void OnChangeHuman(object sender, EventArgs e)
        {
            _human = checkBoxHuman.Checked;

            if (checkBoxHuman.Checked && _loaded)
            {
                RefreshDrawing();
            }
        }

        private void OnChangeElve(object sender, EventArgs e)
        {
            _elve = checkBoxElve.Checked;

            if (checkBoxElve.Checked && _loaded)
            {
                RefreshDrawing();
            }
        }

        private void OnChangeGargoyle(object sender, EventArgs e)
        {
            _gargoyle = checkBoxGargoyle.Checked;

            if (checkBoxGargoyle.Checked && _loaded)
            {
                RefreshDrawing();
            }
        }

        private void OnClick_Dress(object sender, EventArgs e)
        {
            DressItem();
        }

        private void DressItem()
        {
            if (treeViewItems.SelectedNode == null)
            {
                return;
            }

            int objType = (int) treeViewItems.SelectedNode.Tag;

            int layer = TileData.ItemTable[objType].Quality;

            if (Array.IndexOf(_drawOrder, layer) == -1)
            {
                return;
            }

            _layers[layer] = objType;

            checkedListBoxWear.BeginUpdate();
            checkedListBoxWear.Items[layer] = $"0x{layer:X2} {TileData.ItemTable[objType].Name}";
            checkedListBoxWear.EndUpdate();

            RefreshDrawing();
        }

        private void OnClick_UnDress(object sender, EventArgs e)
        {
            if (checkedListBoxWear.SelectedIndex == -1)
            {
                return;
            }

            int layer = checkedListBoxWear.SelectedIndex;

            checkedListBoxWear.Items[checkedListBoxWear.SelectedIndex] = $"0x{layer:X2}";

            _layers[layer] = 0;

            RefreshDrawing();
        }

        private void OnClickUndressAll(object sender, EventArgs e)
        {
            for (int i = 0; i < _layers.Length; ++i)
            {
                _layers[i] = 0;

                checkedListBoxWear.Items[i] = $"0x{i:X2}";
            }

            RefreshDrawing();
        }

        private void CheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (!_loaded)
            {
                return;
            }

            _layerVisible[e.Index] = e.NewValue == CheckState.Checked;

            RefreshDrawing();
        }

        private void CheckedListBox_Change(object sender, EventArgs e)
        {
            if (checkedListBoxWear.SelectedIndex == -1)
            {
                return;
            }

            int layer = checkedListBoxWear.SelectedIndex;
            int objType = (int)_layers[layer];
            int ani = TileData.ItemTable[objType].Animation;
            int gumpIdOrig = ani + 50000;
            int gumpId = gumpIdOrig;
            int hue = 0;

            Animations.Translate(ref ani);

            ConvertBody(ref ani, ref gumpId, ref hue);

            if (_female)
            {
                gumpId += 10000;

                if (!Gumps.IsValidIndex(gumpId))  // female gump.def entry?
                {
                    ConvertGump(ref gumpId, ref hue);
                }

                if (!Gumps.IsValidIndex(gumpId)) // nope so male gump
                {
                    gumpId -= 10000;
                }
            }

            if (!Gumps.IsValidIndex(gumpId)) // male (or invalid female)
            {
                ConvertGump(ref gumpId, ref hue);
            }

            TextBox.Clear();
            TextBox.AppendText($"Objtype: 0x{objType:X4}  Layer: 0x{layer:X2}\n");
            TextBox.AppendText($"GumpID: 0x{gumpId:X4} (0x{gumpIdOrig:X4}) Hue: {hue}\n");
            TextBox.AppendText($"Animation: 0x{ani:X4} (0x{TileData.ItemTable[objType].Animation:X4})\n");
            TextBox.AppendText($"ValidGump: {Gumps.IsValidIndex(gumpId)} ValidAnim: {Animations.IsActionDefined(ani, 0, 0)}");
        }

        private void OnChangeSort(object sender, EventArgs e)
        {
            treeViewItems.TreeViewNodeSorter = LayerSort.Checked ? new LayerSorter() : (IComparer)new ObjTypeSorter();
        }

        private void OnClick_ChangeDisplay(object sender, EventArgs e)
        {
            _showPd = !_showPd;

            if (_showPd)
            {
                groupBoxAnimate.Visible = false;
                animateToolStripMenuItem.Visible = false;
                showAnimationToolStripMenuItem.Text = "Show Animation";
            }
            else
            {
                groupBoxAnimate.Visible = true;
                animateToolStripMenuItem.Visible = true;
                showAnimationToolStripMenuItem.Text = "Show Paperdoll";
            }

            RefreshDrawing();
        }

        private void BuildDressList()
        {
            treeViewItems.BeginUpdate();
            treeViewItems.Nodes.Clear();

            if (TileData.ItemTable != null)
            {
                for (int i = 0; i < TileData.ItemTable.Length; ++i)
                {
                    if (!TileData.ItemTable[i].Wearable)
                    {
                        continue;
                    }

                    int ani = TileData.ItemTable[i].Animation;
                    if (ani == 0)
                    {
                        continue;
                    }

                    int hue = 0;
                    int gump = ani + 50000;

                    ConvertBody(ref ani, ref gump, ref hue);

                    if (!Gumps.IsValidIndex(gump))
                    {
                        ConvertGump(ref gump, ref hue);
                    }

                    bool hasAnimation = Animations.IsActionDefined(ani, 0, 0);

                    bool hasGump = Gumps.IsValidIndex(gump);

                    TreeNode node = new TreeNode($"0x{i:X4} (0x{TileData.ItemTable[i].Quality:X2}) {TileData.ItemTable[i].Name}")
                    {
                        Tag = i
                    };

                    if (Array.IndexOf(_drawOrder, TileData.ItemTable[i].Quality) == -1)
                    {
                        node.ForeColor = Color.DarkRed;
                    }
                    else if (!hasAnimation)
                    {
                        node.ForeColor = !hasGump ? Color.Red : Color.Orange;
                    }
                    else if (!hasGump)
                    {
                        node.ForeColor = Color.Blue;
                    }

                    treeViewItems.Nodes.Add(node);
                }
            }

            treeViewItems.EndUpdate();
        }

        public void RefreshDrawing()
        {
            if (_mTimer != null)
            {
                if (_mTimer.Enabled)
                {
                    _mTimer.Stop();
                }

                _mTimer.Dispose();
                _mTimer = null;
            }

            if (_animation != null)
            {
                foreach (var frame in _animation)
                {
                    frame?.Dispose();
                }
            }

            _animation = null;
            _mFrameIndex = 0;

            DrawPaperdoll();
        }

        private void OnScroll_Facing(object sender, EventArgs e)
        {
            _facing = (FacingBar.Value - 3) & 7;

            toolTip1.SetToolTip(FacingBar, FacingBar.Value.ToString());

            RefreshDrawing();
        }

        private void OnScroll_Action(object sender, EventArgs e)
        {
            string[] tip =
            {
                "Walk_01",
                "WalkStaff_01",
                "Run_01",
                "RunStaff_01",
                "Idle_01",
                "Idle_01",
                "Fidget_Yawn_Stretch_01",
                "CombatIdle1H_01",
                "CombatIdle1H_01",
                "AttackSlash1H_01",
                "AttackPierce1H_01",
                "AttackBash1H_01",
                "AttackBash2H_01",
                "AttackSlash2H_01",
                "AttackPierce2H_01",
                "CombatAdvance_1H_01",
                "Spell1",
                "Spell2",
                "AttackBow_01",
                "AttackCrossbow_01",
                "GetHit_Fr_Hi_01",
                "Die_Hard_Fwd_01",
                "Die_Hard_Back_01",
                "Horse_Walk_01",
                "Horse_Run_01",
                "Horse_Idle_01",
                "Horse_Attack1H_SlashRight_01",
                "Horse_AttackBow_01",
                "Horse_AttackCrossbow_01",
                "Horse_Attack2H_SlashRight_01",
                "Block_Shield_Hard_01",
                "Punch_Punch_Jab_01",
                "Bow_Lesser_01",
                "Salute_Armed1h_01",
                "Ingest_Eat_01"
            };

            toolTip1.SetToolTip(ActionBar, ActionBar.Value + " " + tip[ActionBar.Value]);
            _action = ActionBar.Value;

            RefreshDrawing();
        }

        private void OnResizePictureDress(object sender, EventArgs e)
        {
            if (treeViewItems.SelectedNode == null)
            {
                return;
            }

            pictureBoxDress.Image = new Bitmap(pictureBoxDress.Width, pictureBoxDress.Height);
            AfterSelectTreeView(this, new TreeViewEventArgs(treeViewItems.SelectedNode));
        }

        private void OnResizeDressPic(object sender, EventArgs e)
        {
            DressPic.Image = new Bitmap(DressPic.Width, DressPic.Height);
            if (_loaded) // initial event
            {
                RefreshDrawing();
            }
        }

        private static void ConvertGump(ref int gumpId, ref int hue)
        {
            if (!GumpTable.Entries.ContainsKey(gumpId))
            {
                return;
            }

            GumpTableEntry entry = GumpTable.Entries[gumpId];
            hue = entry.NewHue;
            gumpId = entry.NewId;
        }

        private void ConvertBody(ref int animId, ref int gumpId, ref int hue)
        {
            if (!_elve)
            {
                if (!_female)
                {
                    if (!EquipTable.HumanMale.ContainsKey(animId))
                    {
                        return;
                    }

                    EquipTableEntry entry = EquipTable.HumanMale[animId];
                    gumpId = entry.NewId;
                    hue = entry.NewHue;
                    animId = entry.NewAnim;
                }
                else
                {
                    if (!EquipTable.HumanFemale.ContainsKey(animId))
                    {
                        return;
                    }

                    EquipTableEntry entry = EquipTable.HumanFemale[animId];
                    gumpId = entry.NewId;
                    hue = entry.NewHue;
                    animId = entry.NewAnim;
                }
            }
            else
            {
                if (!_female)
                {
                    if (!EquipTable.ElvenMale.ContainsKey(animId))
                    {
                        return;
                    }

                    EquipTableEntry entry = EquipTable.ElvenMale[animId];
                    gumpId = entry.NewId;
                    hue = entry.NewHue;
                    animId = entry.NewAnim;
                }
                else
                {
                    if (!EquipTable.ElvenFemale.ContainsKey(animId))
                    {
                        return;
                    }

                    EquipTableEntry entry = EquipTable.ElvenFemale[animId];
                    gumpId = entry.NewId;
                    hue = entry.NewHue;
                    animId = entry.NewAnim;
                }
            }
        }

        private HuePopUpDress _showForm;

        private void OnClickHue(object sender, EventArgs e)
        {
            if (checkedListBoxWear.SelectedIndex == -1)
            {
                return;
            }

            if (_showForm?.IsDisposed == false)
            {
                return;
            }

            _showForm = new HuePopUpDress(ChangeHue, _hues[checkedListBoxWear.SelectedIndex])
            {
                TopMost = true
            };
            _showForm.Show();
        }

        private void ChangeHue(int hue)
        {
            SetHue(checkedListBoxWear.SelectedIndex, hue);
            RefreshDrawing();
        }

        private void OnKeyDownHue(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            if (checkedListBoxWear.SelectedIndex == -1)
            {
                return;
            }

            if (!Utils.ConvertStringToInt(toolStripTextBox1.Text, out int index, 0, Hues.List.Length))
            {
                return;
            }

            _hues[checkedListBoxWear.SelectedIndex] = index;
            RefreshDrawing();
        }

        private void OnClickExtractImageBmp(object sender, EventArgs e)
        {
            ExportImage(ImageFormat.Bmp);
        }

        private void OnClickExtractImageTiff(object sender, EventArgs e)
        {
            ExportImage(ImageFormat.Tiff);
        }

        private void OnClickExtractImageJpg(object sender, EventArgs e)
        {
            ExportImage(ImageFormat.Jpeg);
        }

        private void OnClickExtractImagePng(object sender, EventArgs e)
        {
            ExportImage(ImageFormat.Png);
        }

        private void ExportImage(ImageFormat imageFormat)
        {
            string outputPath = Options.OutputPath;
            string fileExtension = Utils.GetFileExtensionFor(imageFormat);

            if (_showPd)
            {
                string fileName = Path.Combine(outputPath, $"Dress PD.{fileExtension}");
                DressPic.Image.Save(fileName, imageFormat);
                MessageBox.Show($"Paperdoll saved to {fileName}", "Saved", MessageBoxButtons.OK,
                    MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
            else
            {
                string fileName = Path.Combine(outputPath, $"Dress IG.{fileExtension}");
                if (_animate)
                {
                    _animation[0].Save(fileName, imageFormat);
                }
                else
                {
                    DressPic.Image.Save(fileName, imageFormat);
                }

                MessageBox.Show($"InGame saved to {fileName}", "Saved", MessageBoxButtons.OK,
                    MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
        }

        private void OnClickExtractAnimBmp(object sender, EventArgs e)
        {
            ExportAnimation(ImageFormat.Bmp);
        }

        private void OnClickExtractAnimTiff(object sender, EventArgs e)
        {
            ExportAnimation(ImageFormat.Tiff);
        }

        private void OnClickExtractAnimJpg(object sender, EventArgs e)
        {
            ExportAnimation(ImageFormat.Jpeg);
        }

        private void OnClickExtractAnimPng(object sender, EventArgs e)
        {
            ExportAnimation(ImageFormat.Png);
        }

        private void ExportAnimation(ImageFormat imageFormat)
        {
            const string fileName = "Dress Anim";

            string path = Options.OutputPath;
            string fileExtension = Utils.GetFileExtensionFor(imageFormat);

            for (int i = 0; i < _animation.Length; ++i)
            {
                _animation[i].Save(Path.Combine(path, $"{fileName}-{i}.{fileExtension}"), imageFormat);
            }

            MessageBox.Show($"InGame Anim saved to '{fileName}-X.{fileExtension}'", "Saved", MessageBoxButtons.OK,
                MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void OnClickBuildAnimationList(object sender, EventArgs e)
        {
            const int maximumAnimationValue = 2048;
            AnimEntry[] animEntries = new AnimEntry[maximumAnimationValue + 1];
            for (int i = 0; i < animEntries.Length; ++i)
            {
                animEntries[i] = new AnimEntry
                {
                    Animation = i,
                    FirstGump = i + 50000,
                    FirstGumpFemale = i + 60000
                };

                if (EquipTable.HumanMale.ContainsKey(i))
                {
                    animEntries[i].EquipTable[400] = EquipTable.HumanMale[i];
                }

                if (EquipTable.HumanFemale.ContainsKey(i))
                {
                    animEntries[i].EquipTable[401] = EquipTable.HumanFemale[i];
                }

                if (EquipTable.ElvenMale.ContainsKey(i))
                {
                    animEntries[i].EquipTable[605] = EquipTable.ElvenMale[i];
                }

                if (EquipTable.ElvenFemale.ContainsKey(i))
                {
                    animEntries[i].EquipTable[606] = EquipTable.ElvenFemale[i];
                }

                if (EquipTable.Misc.ContainsKey(i))
                {
                    foreach (var valuePair in EquipTable.Misc[i])
                    {
                        animEntries[i].EquipTable[valuePair.Key] = valuePair.Value;
                    }
                }

                if (animEntries[i].EquipTable.Count == 0)
                {
                    if (GumpTable.Entries.ContainsKey(animEntries[i].FirstGump))
                    {
                        animEntries[i].GumpDef[0] = GumpTable.Entries[animEntries[i].FirstGump];
                    }
                }
                else
                {
                    foreach (var kv in animEntries[i].EquipTable)
                    {
                        if (GumpTable.Entries.ContainsKey(kv.Value.NewId))
                        {
                            animEntries[i].GumpDef[kv.Key] = GumpTable.Entries[kv.Value.NewId];
                        }
                    }
                }

                if (animEntries[i].EquipTable.Count == 0)
                {
                    int tmp = i;
                    animEntries[i].TranslateAnim[0] = new TranslateAnimEntry
                    {
                        BodyDef = BodyTable.Entries.ContainsKey(tmp)
                    };

                    Animations.Translate(ref tmp);

                    animEntries[i].TranslateAnim[0].FileIndex = BodyConverter.Convert(ref tmp);
                    animEntries[i].TranslateAnim[0].BodyAndConf = tmp;
                }
                else
                {
                    foreach (var kv in animEntries[i].EquipTable)
                    {
                        int tmp = kv.Value.NewAnim;

                        animEntries[i].TranslateAnim[kv.Key] = new TranslateAnimEntry
                        {
                            BodyDef = BodyTable.Entries.ContainsKey(tmp)
                        };

                        Animations.Translate(ref tmp);

                        animEntries[i].TranslateAnim[kv.Key].FileIndex = BodyConverter.Convert(ref tmp);
                        animEntries[i].TranslateAnim[kv.Key].BodyAndConf = tmp;
                    }
                }
            }

            string fileName = Path.Combine(Options.OutputPath, "animationlist.html");
            using (StreamWriter tex = new StreamWriter(new FileStream(fileName, FileMode.Create, FileAccess.Write), System.Text.Encoding.GetEncoding(1252)))
            {
                tex.WriteLine("<html> <body> <table border='1' rules='all' cellpadding='2'>");
                tex.WriteLine("<tr>");
                tex.WriteLine("<td>Anim</td>");
                tex.WriteLine("<td>Gump male/female</td>");
                tex.WriteLine("<td>equipconv<br/>model:anim,gump,hue</td>");
                tex.WriteLine("<td>gump.def<br/>gump,hue</td>");
                tex.WriteLine("<td>body.def/bodyconv<br/>[model:]fileindex,anim</td>");
                tex.WriteLine("<td>tiledata def</td>");
                tex.WriteLine("</tr>");
                for (int i = 1; i < animEntries.Length; ++i)
                {
                    tex.WriteLine("<tr>");
                    tex.Write("<td>");
                    bool openFont = false;

                    if (!Animations.IsActionDefined(i, 0, 0))
                    {
                        tex.Write("<font color=#FF0000>");
                        openFont = true;
                    }

                    tex.Write(i);
                    if (openFont)
                    {
                        tex.Write("</font>");
                    }

                    tex.Write("</td>");
                    if (i >= 400)
                    {
                        tex.Write("<td>");
                        openFont = false;
                        if (!Gumps.IsValidIndex(animEntries[i].FirstGump))
                        {
                            tex.Write("<font color=#FF0000>");
                            openFont = true;
                        }
                        tex.Write(animEntries[i].FirstGump);
                        if (openFont)
                        {
                            tex.Write("</font>");
                        }

                        tex.Write("/");
                        openFont = false;
                        if (!Gumps.IsValidIndex(animEntries[i].FirstGumpFemale))
                        {
                            tex.Write("<font color=#FF0000>");
                            openFont = true;
                        }
                        tex.Write(animEntries[i].FirstGumpFemale);
                        if (openFont)
                        {
                            tex.Write("</font>");
                        }

                        tex.Write("</td>");
                    }
                    else
                    {
                        tex.Write("<td></td>");
                    }

                    tex.Write("<td>");
                    foreach (var valuePair in animEntries[i].EquipTable)
                    {
                        if (valuePair.Key != 0)
                        {
                            tex.Write(valuePair.Key + ":");
                        }

                        openFont = false;

                        if (animEntries[i].TranslateAnim.ContainsKey(valuePair.Key))
                        {
                            if (!Animations.IsAnimDefined(animEntries[i].TranslateAnim[valuePair.Key].BodyAndConf, 0, 0, animEntries[i].TranslateAnim[valuePair.Key].FileIndex))
                            {
                                tex.Write("<font color=#FF0000>");
                                openFont = true;
                            }
                        }

                        tex.Write(valuePair.Value.NewAnim);

                        if (openFont)
                        {
                            tex.Write("</font>");
                        }

                        tex.Write(",");
                        openFont = false;
                        if (!Gumps.IsValidIndex(animEntries[i].FirstGumpFemale))
                        {
                            tex.Write("<font color=#FF0000>");
                            openFont = true;
                        }
                        tex.Write(valuePair.Value.NewId);
                        if (openFont)
                        {
                            tex.Write("</font>");
                        }

                        tex.Write(",");
                        tex.Write(valuePair.Value.NewHue);
                        tex.Write("<br/>");
                    }
                    tex.Write("</td>");

                    tex.Write("<td>");
                    foreach (var valuePair in animEntries[i].GumpDef)
                    {
                        if (valuePair.Key != 0)
                        {
                            tex.Write(valuePair.Key + ":");
                        }

                        openFont = false;
                        if (!Gumps.IsValidIndex(valuePair.Value.NewId))
                        {
                            tex.Write("<font color=#FF0000>");
                            openFont = true;
                        }

                        tex.Write(valuePair.Value.NewId);

                        if (openFont)
                        {
                            tex.Write("</font>");
                        }

                        tex.Write("," + valuePair.Value.NewHue + "<br/>");
                    }
                    tex.Write("</td>");

                    tex.Write("<td>");
                    foreach (var valuePair in animEntries[i].TranslateAnim)
                    {
                        if (valuePair.Value.FileIndex == 1 && !valuePair.Value.BodyDef)
                        {
                            continue;
                        }

                        if (valuePair.Key != 0)
                        {
                            tex.Write(valuePair.Key + ":");
                        }

                        tex.Write($"{valuePair.Value.FileIndex},{valuePair.Value.BodyAndConf}<br/>");
                    }
                    tex.Write("</td>");

                    tex.Write("<td>");
                    if (i >= 400)
                    {
                        for (int j = 0; j < TileData.ItemTable.Length; ++j)
                        {
                            if (TileData.ItemTable[j].Animation == i)
                            {
                                tex.Write("0x{0:X4} {1}<br/>", j, TileData.ItemTable[j].Name);
                            }
                        }
                    }
                    tex.Write("</td>");
                    tex.WriteLine("</tr>");
                }
                tex.WriteLine("</table> </body> </html>");
            }

            MessageBox.Show($"Report saved to '{fileName}'", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void MountTextBoxOnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            if (!Utils.ConvertStringToInt(textBoxMount.Text, out int index, 0, 0xFFFF))
            {
                return;
            }

            if (!Animations.IsActionDefined(index, 0, 0))
            {
                return;
            }

            _mount = index;
            RefreshDrawing();
        }

        private readonly List<TreeNode> _searchResults = new List<TreeNode>();

        private int _lastNodeIndex;

        private string _lastSearchText;

        private void SearchByName()
        {
            var searchText = SearchItemTextBox.Text.Trim();

            if (string.IsNullOrEmpty(searchText))
            {
                return;
            }

            if (_lastSearchText != searchText)
            {
                _searchResults.Clear();

                _lastSearchText = searchText;

                _lastNodeIndex = 0;

                SearchNodes(searchText, treeViewItems.Nodes[0]);
            }

            if (_lastNodeIndex < 0 || _searchResults.Count == 0)
            {
                return;
            }

            if (_lastNodeIndex >= _searchResults.Count)
            {
                _lastNodeIndex = 0;
            }

            TreeNode selectedNode = _searchResults[_lastNodeIndex];

            _lastNodeIndex++;

            treeViewItems.SelectedNode = selectedNode;
        }

        private void SearchNodes(string searchText, TreeNode startNode)
        {
            while (startNode != null)
            {
                if (startNode.Text.ContainsCaseInsensitive(searchText))
                {
                    _searchResults.Add(startNode);
                }

                startNode = startNode.NextNode;
            }
        }

        private void SearchItemTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            SearchByName();
        }

        private void FindNextItemButton_Click(object sender, EventArgs e)
        {
            SearchByName();
        }

        private void TreeViewItems_DoubleClick(object sender, EventArgs e)
        {
            DressItem();
        }
    }

    public class TranslateAnimEntry
    {
        public int FileIndex { get; set; }
        public int BodyAndConf { get; set; }
        public bool BodyDef { get; set; }
    }

    public class AnimEntry
    {
        //public struct EquipTableDef { public int Gump; public int Anim; } // TODO: unused?
        public int Animation { get; set; }
        public int FirstGump { get; set; } //+50000
        public int FirstGumpFemale { get; set; }//+60000
        public Dictionary<int, EquipTableEntry> EquipTable { get; set; } //equipconv.def with model
        public Dictionary<int, GumpTableEntry> GumpDef { get; set; } //gump.def if gump invalid (only for paperdoll)
        public Dictionary<int, TranslateAnimEntry> TranslateAnim { get; set; }//body.def or bodyconv.def

        public AnimEntry()
        {
            EquipTable = new Dictionary<int, EquipTableEntry>();
            GumpDef = new Dictionary<int, GumpTableEntry>();
            TranslateAnim = new Dictionary<int, TranslateAnimEntry>();
        }
    }

    public class ObjTypeSorter : IComparer
    {
        public int Compare(object x, object y)
        {
            TreeNode tx = x as TreeNode;
            TreeNode ty = y as TreeNode;
            return string.CompareOrdinal(tx?.Text, ty?.Text);
        }
    }

    public class LayerSorter : IComparer
    {
        public int Compare(object x, object y)
        {
            TreeNode tx = x as TreeNode;
            TreeNode ty = y as TreeNode;

            int layerX = TileData.ItemTable[(int)tx.Tag].Quality;
            int layerY = TileData.ItemTable[(int)ty.Tag].Quality;

            if (layerX == layerY)
            {
                return 0;
            }

            if (layerX < layerY)
            {
                return -1;
            }

            return 1;
        }
    }

    public static class GumpTable
    {
        public static Dictionary<int, GumpTableEntry> Entries { get; }

        // Seems only used if Gump is invalid
        static GumpTable()
        {
            Entries = new Dictionary<int, GumpTableEntry>();
            Initialize();
        }

        public static void Initialize()
        {
            string path = Files.GetFilePath("gump.def");
            if (path == null)
            {
                return;
            }

            using (StreamReader ip = new StreamReader(path))
            {
                string line;
                while ((line = ip.ReadLine()) != null)
                {
                    if ((line = line.Trim()).Length == 0 || line.StartsWith("#"))
                    {
                        continue;
                    }

                    try
                    {
                        // <ORIG BODY> {<NEW BODY>} <NEW HUE>
                        int index1 = line.IndexOf("{", StringComparison.Ordinal);
                        int index2 = line.IndexOf("}", StringComparison.Ordinal);

                        string param1 = line.Substring(0, index1);
                        string param2 = line.Substring(index1 + 1, index2 - index1 - 1);
                        string param3 = line.Substring(index2 + 1);

                        int iParam1 = Convert.ToInt32(param1.Trim());
                        int iParam2 = Convert.ToInt32(param2.Trim());
                        int iParam3 = Convert.ToInt32(param3.Trim());

                        Entries[iParam1] = new GumpTableEntry(iParam1, iParam2, iParam3);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
        }
    }

    public class GumpTableEntry
    {
        public int OldId { get; }
        public int NewId { get; }
        public int NewHue { get; }

        public GumpTableEntry(int oldId, int newId, int newHue)
        {
            OldId = oldId;
            NewId = newId;
            NewHue = newHue;
        }
    }

    public static class EquipTable
    {
        public static Dictionary<int, EquipTableEntry> HumanMale { get; }
        public static Dictionary<int, EquipTableEntry> HumanFemale { get; }
        public static Dictionary<int, EquipTableEntry> ElvenMale { get; }
        public static Dictionary<int, EquipTableEntry> ElvenFemale { get; }
        public static Dictionary<int, EquipTableEntry> GargoyleMale { get; }
        public static Dictionary<int, EquipTableEntry> GargoyleFemale { get; }
        public static Dictionary<int, Dictionary<int, EquipTableEntry>> Misc { get; }

        static EquipTable()
        {
            HumanMale = new Dictionary<int, EquipTableEntry>();
            HumanFemale = new Dictionary<int, EquipTableEntry>();
            ElvenMale = new Dictionary<int, EquipTableEntry>();
            ElvenFemale = new Dictionary<int, EquipTableEntry>();
            GargoyleMale = new Dictionary<int, EquipTableEntry>();
            GargoyleFemale = new Dictionary<int, EquipTableEntry>();
            Misc = new Dictionary<int, Dictionary<int, EquipTableEntry>>();
            Initialize();
        }

        public static void Initialize()
        {
            string path = Files.GetFilePath("equipconv.def");
            if (path == null)
            {
                return;
            }

            using (StreamReader ip = new StreamReader(path))
            {
                string line;
                while ((line = ip.ReadLine()) != null)
                {
                    if ((line = line.Trim()).Length == 0
                            || line.StartsWith("#")
                            || line.StartsWith("\""))
                    {
                        continue;
                    }
                    //#bodyType	#equipmentID	#convertToID	#GumpIDToUse	#hue
                    //GumpID (0 = equipmentID + 50000, -1 = convertToID + 50000, other numbers are the actual gumpID )

                    try
                    {
                        string[] split = Regex.Split(line, @"\s+");

                        int bodyType = Convert.ToInt32(split[0]);
                        int animId = Convert.ToInt32(split[1]);
                        int convertId = Convert.ToInt32(split[2]);
                        int gumpId = Convert.ToInt32(split[3]);
                        int hue = Convert.ToInt32(split[4]);

                        if (gumpId == 0)
                        {
                            gumpId = animId + 50000;
                        }
                        else if (gumpId == -1)
                        {
                            gumpId = convertId + 50000;
                        }

                        EquipTableEntry entry = new EquipTableEntry(gumpId, hue, convertId);
                        switch (bodyType)
                        {
                            case 400:
                                HumanMale[animId] = entry;
                                break;
                            case 401:
                                HumanFemale[animId] = entry;
                                break;
                            case 605:
                                ElvenMale[animId] = entry;
                                break;
                            case 606:
                                ElvenFemale[animId] = entry;
                                break;
                            case 666:
                                GargoyleMale[animId] = entry;
                                break;
                            case 667:
                                GargoyleFemale[animId] = entry;
                                break;
                            default:
                                if (!Misc.ContainsKey(animId))
                                {
                                    Misc[animId] = new Dictionary<int, EquipTableEntry>();
                                }

                                Misc[animId][bodyType] = entry;
                                break;
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
        }
    }

    public class EquipTableEntry
    {
        public int NewId { get; }
        public int NewHue { get; }
        public int NewAnim { get; }

        public EquipTableEntry(int newId, int newHue, int newAnim)
        {
            NewId = newId;
            NewHue = newHue;
            NewAnim = newAnim;
        }
    }
}
