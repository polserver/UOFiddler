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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;
using UoFiddler.Controls.Helpers;
using UoFiddler.Controls.UserControls.TileView;

namespace UoFiddler.Controls.UserControls
{
    public partial class AnimationListControl : UserControl
    {
        public AnimationListControl()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            // TODO can this be moved into the control itself?
            listView1.Height += SystemInformation.HorizontalScrollBarHeight;
        }

        public string[][] GetActionNames { get; } = {
            // Monster
            new[]
            {
                "Walk",
                "Idle",
                "Die1",
                "Die2",
                "Attack1",
                "Attack2",
                "Attack3",
                "AttackBow",
                "AttackCrossBow",
                "AttackThrow",
                "GetHit",
                "Pillage",
                "Stomp",
                "Cast2",
                "Cast3",
                "BlockRight",
                "BlockLeft",
                "Idle",
                "Fidget",
                "Fly",
                "TakeOff",
                "GetHitInAir"
            },
            // Sea
            new[]
            {
                "Walk",
                "Run",
                "Idle",
                "Idle",
                "Fidget",
                "Attack1",
                "Attack2",
                "GetHit",
                "Die1"
            },
            // Animal
            new[]
            {
                "Walk",
                "Run",
                "Idle",
                "Eat",
                "Alert",
                "Attack1",
                "Attack2",
                "GetHit",
                "Die1",
                "Idle",
                "Fidget",
                "LieDown",
                "Die2"
            },
            // Human
            new[]
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
            }
        };

        // Tag of the throwaway child node added under a body so the expander ([+]) shows. The real
        // action nodes replace it the first time the body is expanded (see TreeViewMobs_BeforeExpand).
        private const int PlaceholderActionTag = -1;

        private int _currentSelect;
        private int _currentSelectAction;
        private int _customHue;
        private int _defHue;
        private int _facing = 1;
        private bool _sortAlpha;
        private int _displayType;
        private bool _loaded;
        private readonly List<int> _listViewGraphics = new List<int>();
        // Tree nodes backing each thumbnail, parallel to _listViewGraphics, so selection and per-tile
        // rendering can map back to the correct node.
        private readonly List<TreeNode> _listViewNodes = new List<TreeNode>();

        /// <summary>
        /// ReLoads if loaded
        /// </summary>
        private void Reload()
        {
            if (!_loaded)
            {
                return;
            }

            _currentSelect = 0;
            _currentSelectAction = 0;
            _customHue = 0;
            _defHue = 0;
            _facing = 1;
            _sortAlpha = false;
            _displayType = 0;
            MainPictureBox.Reset();
            AnimateCheckBox.Checked = false;
            ShowFrameBoundsCheckBox.Checked = false;

            OnLoad(this, EventArgs.Empty);
        }

        private void OnLoad(object sender, EventArgs e)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            using (new WaitCursorScope(this))
            {
                Options.LoadedUltimaClass["Animations"] = true;
                Options.LoadedUltimaClass["Hues"] = true;
                // Keep the sorter detached while populating - assigning it up front makes every
                // node insertion re-sort its siblings (O(n^2) over hundreds of bodies). The body
                // lists are pre-sorted by graphic in-memory before they are attached, so the native
                // TreeView.Sort() is only needed for the alphabetical view.
                TreeViewMobs.TreeViewNodeSorter = null;
                if (!LoadXml())
                {
                    return;
                }

                if (_sortAlpha)
                {
                    TreeViewMobs.BeginUpdate();
                    try
                    {
                        TreeViewMobs.TreeViewNodeSorter = new AlphaSorter();
                        TreeViewMobs.Sort();
                    }
                    finally
                    {
                        TreeViewMobs.EndUpdate();
                    }
                }

                LoadListView();

                _currentSelect = 0;
                _currentSelectAction = 0;
                if (TreeViewMobs.Nodes[0].Nodes.Count > 0)
                {
                    TreeViewMobs.SelectedNode = TreeViewMobs.Nodes[0].Nodes[0];
                }

                FacingBar.Value = (_facing + 3) & 7;
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

        /// <summary>
        /// Changes Hue of current Mob
        /// </summary>
        /// <param name="select"></param>
        public void ChangeHue(int select)
        {
            _customHue = select + 1;
            CurrentSelect = CurrentSelect;
        }

        /// <summary>
        /// Is Graphic already in TreeView
        /// </summary>
        /// <param name="graphic"></param>
        /// <returns></returns>
        public bool IsAlreadyDefined(int graphic)
        {
            return TreeViewMobs.Nodes[0].Nodes.Cast<TreeNode>().Any(node => ((int[])node.Tag)[0] == graphic) ||
                   TreeViewMobs.Nodes[1].Nodes.Cast<TreeNode>().Any(node => ((int[])node.Tag)[0] == graphic);
        }

        /// <summary>
        /// Adds Graphic with type and name to List
        /// </summary>
        /// <param name="graphic"></param>
        /// <param name="type"></param>
        /// <param name="name"></param>
        public void AddGraphic(int graphic, int type, string name)
        {
            TreeViewMobs.BeginUpdate();
            TreeViewMobs.TreeViewNodeSorter = null;

            int firstAction = GetFirstDefinedAction(graphic, type);
            TreeNode nodeParent = new TreeNode(name)
            {
                Tag = new[] { graphic, type, firstAction }
            };

            TreeViewMobs.Nodes[type == (int)MobType.Equipment ? 1 : 0].Nodes.Add(nodeParent);

            // The freshly added body is selected and scrolled into view immediately below, so build
            // its action nodes now rather than deferring to the first expand.
            PopulateActionNodes(nodeParent);

            TreeViewMobs.TreeViewNodeSorter = !_sortAlpha
                ? new GraphicSorter()
                : (IComparer)new AlphaSorter();

            TreeViewMobs.Sort();
            TreeViewMobs.EndUpdate();
            LoadListView();
            TreeViewMobs.SelectedNode = nodeParent;
            nodeParent.EnsureVisible();
        }

        private bool Animate
        {
            get => MainPictureBox.Animate;
            set => MainPictureBox.Animate = value;
        }

        private int CurrentSelect
        {
            get => _currentSelect;
            set
            {
                _currentSelect = value;
                SetPicture();
            }
        }

        private void SetPicture()
        {
            if (_currentSelect == 0)
            {
                ClearPicture();
                return;
            }

            int body = _currentSelect;
            Animations.Translate(ref body);
            int hue = _customHue;
            bool preserveHue = hue != 0;

            List<AnimatedFrame> frames = null;
            try
            {
                // GetAnimation returns cache-owned bitmaps; clone them so the
                // picture box can own and dispose its frames without corrupting the cache.
                // Skip any frame without a bitmap so the projection below cannot throw on it.
                frames = Animations.GetAnimation(_currentSelect, _currentSelectAction, _facing, ref hue, preserveHue, false)
                    ?.Where(animation => animation?.Bitmap != null)
                    .Select(animation => new AnimatedFrame(new Bitmap(animation.Bitmap), animation.Center)).ToList();
            }
            catch
            {
                frames = null;
            }

            MainPictureBox.Frames = frames;

            if (MainPictureBox.FirstFrame == null)
            {
                // The selected entry has no animation frames — clear the right-hand side
                // instead of leaving the previously shown animation on screen.
                ClearPicture();
                return;
            }

            if (!preserveHue)
            {
                _defHue = hue;
            }

            BaseGraphicLabel.Text = $"BaseGraphic: {body} (0x{body:X})";
            GraphicLabel.Text = $"Graphic: {_currentSelect} (0x{_currentSelect:X})";
            HueLabel.Text = $"Hue: {hue + 1} (0x{hue + 1:X})";

            LoadListViewFrames();
        }

        /// <summary>
        /// Clears the right-hand preview: empties the animation picture box, the frame list and the
        /// info labels. Used when the selected entry has no animation frames so nothing stale remains.
        /// </summary>
        private void ClearPicture()
        {
            MainPictureBox.Frames = null;
            LoadListViewFrames();

            BaseGraphicLabel.Text = "BaseGraphic:";
            GraphicLabel.Text = "Graphic: ";
            HueLabel.Text = "Hue:";
        }

        private void TreeViewMobs_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Parent != null)
            {
                if (e.Node.Parent.Name == "Mobs" || e.Node.Parent.Name == "Equipment")
                {
                    // Action 0 is not necessarily defined for this body (e.g. equipment such as
                    // body 322). Use the first defined action recorded in the node Tag so this works
                    // whether or not the body has been expanded (action nodes are built lazily).
                    int firstAction = ((int[])e.Node.Tag)[2];
                    _currentSelectAction = firstAction >= 0 ? firstAction : 0;
                    CurrentSelect = ((int[])e.Node.Tag)[0];
                    if (e.Node.Parent.Name == "Mobs" && _displayType == 1)
                    {
                        _displayType = 0;
                        LoadListView();
                    }
                    else if (e.Node.Parent.Name == "Equipment" && _displayType == 0)
                    {
                        _displayType = 1;
                        LoadListView();
                    }
                }
                else
                {
                    _currentSelectAction = (int)e.Node.Tag;
                    CurrentSelect = ((int[])e.Node.Parent.Tag)[0];
                    if (e.Node.Parent.Parent.Name == "Mobs" && _displayType == 1)
                    {
                        _displayType = 0;
                        LoadListView();
                    }
                    else if (e.Node.Parent.Parent.Name == "Equipment" && _displayType == 0)
                    {
                        _displayType = 1;
                        LoadListView();
                    }
                }
            }
            else
            {
                if (e.Node.Name == "Mobs" && _displayType == 1)
                {
                    _displayType = 0;
                    LoadListView();
                }
                else if (e.Node.Name == "Equipment" && _displayType == 0)
                {
                    _displayType = 1;
                    LoadListView();
                }
                TreeViewMobs.SelectedNode = e.Node.Nodes[0];
            }
        }

        private bool LoadXml()
        {
            string fileName = Path.Combine(Options.AppDataPath, "Animationlist.xml");
            if (!File.Exists(fileName))
            {
                return false;
            }

            var skipped = new List<string>();

            TreeViewMobs.BeginUpdate();
            try
            {
                TreeViewMobs.Nodes.Clear();

                XmlDocument dom = new XmlDocument();
                dom.Load(fileName);

                XmlElement xMobs = dom["Graphics"];

                TreeNode mobsRoot = new TreeNode("Mobs")
                {
                    Name = "Mobs",
                    Tag = -1
                };
                TreeNode equipRoot = new TreeNode("Equipment")
                {
                    Name = "Equipment",
                    Tag = -2
                };

                // Bodies are collected detached and sorted by graphic in-memory before being attached
                // once per root (see below). This avoids the native TreeView.Sort() over thousands of
                // nodes that the managed GraphicSorter would otherwise drive on every load.
                var mobNodes = new List<TreeNode>();
                var equipNodes = new List<TreeNode>();

                foreach (XmlElement xMob in xMobs.SelectNodes("Mob"))
                {
                    string name = xMob.GetAttribute("name");
                    int value = int.Parse(xMob.GetAttribute("body"));
                    int type = int.Parse(xMob.GetAttribute("type"));
                    if (type < 0 || type >= GetActionNames.Length)
                    {
                        skipped.Add($"Mob \"{name}\" (body=0x{value:X}) — invalid type {type}");
                        continue;
                    }

                    int firstAction = GetFirstDefinedAction(value, type);
                    var node = new TreeNode($"{name} (0x{value:X})")
                    {
                        Tag = new[] { value, type, firstAction }
                    };
                    mobNodes.Add(node);
                    AddActionPlaceholder(node, firstAction);
                }

                foreach (XmlElement xMob in xMobs.SelectNodes("Equip"))
                {
                    string name = xMob.GetAttribute("name");
                    int value = int.Parse(xMob.GetAttribute("body"));
                    int type = int.Parse(xMob.GetAttribute("type"));
                    if (type < 0 || type >= GetActionNames.Length)
                    {
                        skipped.Add($"Equip \"{name}\" (body=0x{value:X}) — invalid type {type}");
                        continue;
                    }

                    int firstAction = GetFirstDefinedAction(value, type);
                    var node = new TreeNode(name)
                    {
                        Tag = new[] { value, type, firstAction }
                    };
                    equipNodes.Add(node);
                    AddActionPlaceholder(node, firstAction);
                }

                LoadFromMobTypes(mobNodes, equipNodes);

                mobNodes.Sort(CompareNodeByGraphic);
                equipNodes.Sort(CompareNodeByGraphic);
                mobsRoot.Nodes.AddRange(mobNodes.ToArray());
                equipRoot.Nodes.AddRange(equipNodes.ToArray());
                TreeViewMobs.Nodes.AddRange(new[] { mobsRoot, equipRoot });
            }
            finally
            {
                TreeViewMobs.EndUpdate();
            }

            if (skipped.Count > 0)
            {
                string list = string.Join(Environment.NewLine, skipped);
                MessageBox.Show(
                    $"The following entries were skipped due to an invalid type value (valid range: 0–{GetActionNames.Length - 1}):{Environment.NewLine}{Environment.NewLine}{list}{Environment.NewLine}{Environment.NewLine}File: {fileName}",
                    "Animationlist.xml — Skipped Entries",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            return true;
        }

        /// <summary>
        /// Appends the mobtypes.txt/UOP bodies that are not already present in <paramref name="mobNodes"/>
        /// or <paramref name="equipNodes"/> to the appropriate list. The nodes are left detached - the
        /// caller sorts and attaches them. A HashSet of the already-defined graphics is built once so the
        /// duplicate check is O(1) per body instead of an O(n) scan (the loop runs over thousands of
        /// bodies). Action nodes are added lazily on expand.
        /// </summary>
        private void LoadFromMobTypes(List<TreeNode> mobNodes, List<TreeNode> equipNodes)
        {
            var definedGraphics = new HashSet<int>(mobNodes.Count + equipNodes.Count);
            foreach (TreeNode node in mobNodes)
            {
                definedGraphics.Add(((int[])node.Tag)[0]);
            }
            foreach (TreeNode node in equipNodes)
            {
                definedGraphics.Add(((int[])node.Tag)[0]);
            }

            foreach (int body in Animations.GetAllUopBodies())
            {
                if (definedGraphics.Contains(body))
                {
                    continue;
                }

                int type = (int)MobTypes.GetTypeOrDefault(body);
                bool isEquip = type == (int)MobType.Equipment;
                if (!isEquip && (type < 0 || type >= GetActionNames.Length))
                {
                    type = 0;
                }

                string name = $"Body 0x{body:X}";

                int firstAction = GetFirstDefinedAction(body, type);
                TreeNode nodeParent = new TreeNode($"{name} (0x{body:X})")
                {
                    Tag = new[] { body, type, firstAction }
                };
                AddActionPlaceholder(nodeParent, firstAction);

                (isEquip ? equipNodes : mobNodes).Add(nodeParent);
            }
        }

        /// <summary>
        /// Orders body nodes by their graphic id (Tag[0]) - the default (non-alphabetical) tree order.
        /// Used to pre-sort the detached body lists in-memory before they are attached, avoiding a native
        /// TreeView.Sort() driven by the managed <see cref="GraphicSorter"/> over the whole tree.
        /// </summary>
        private static int CompareNodeByGraphic(TreeNode x, TreeNode y)
        {
            return ((int[])x.Tag)[0].CompareTo(((int[])y.Tag)[0]);
        }

        /// <summary>
        /// Lists the actions defined for <paramref name="body"/>: every defined UOP action for UOP bodies,
        /// the named-action table for MUL bodies.
        /// </summary>
        private List<int> GetDefinedActions(int body, int actionType)
        {
            // UOP animations are not bounded by the legacy per-type group counts (Low=13, High=22,
            // People=34): a UOP body can define action bins well past them (the client scans up to
            // MaxAnimActions - "gargoyle is like 78").
            if (Animations.IsUopBody(body))
            {
                return Animations.GetUopDefinedActions(body);
            }

            var actions = new List<int>();
            for (int i = 0; i < GetActionNames[actionType].GetLength(0); ++i)
            {
                if (Animations.IsActionDefined(body, i, 0))
                {
                    actions.Add(i);
                }
            }

            return actions;
        }

        /// <summary>
        /// Maps a stored node type to a valid index into <see cref="GetActionNames"/>. Equipment (4)
        /// has no action-name table of its own and falls back to Human (3); any out-of-range value
        /// falls back to Monster (0).
        /// </summary>
        private int GetActionNameType(int type)
        {
            if (type == (int)MobType.Equipment)
            {
                return (int)MobType.Human;
            }

            return type < 0 || type >= GetActionNames.Length ? 0 : type;
        }

        /// <summary>
        /// Returns the lowest action index defined for <paramref name="body"/>, or -1 if the body has
        /// no animation. The scan early-exits on the first hit (animated bodies usually define action 0),
        /// so it is far cheaper than enumerating every action - the full list is only built when a body
        /// is expanded. The probe range matches what <see cref="PopulateActionNodes"/> builds: the
        /// named-action table for MUL bodies, the full UOP action range for UOP bodies (which are not
        /// bounded by the legacy per-type group counts).
        /// </summary>
        private int GetFirstDefinedAction(int body, int type)
        {
            int limit = Animations.IsUopBody(body)
                ? Animations.MaxAnimActions
                : GetActionNames[GetActionNameType(type)].GetLength(0);

            for (int i = 0; i < limit; ++i)
            {
                if (Animations.IsActionDefined(body, i, 0))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Adds a single placeholder child so the expander ([+]) shows for a body that has animations.
        /// The placeholder is replaced with the real action nodes the first time the body is expanded
        /// (see <see cref="TreeViewMobs_BeforeExpand"/>). A body with no defined action gets no
        /// placeholder and therefore no expander.
        /// </summary>
        private static void AddActionPlaceholder(TreeNode bodyNode, int firstAction)
        {
            if (firstAction != -1)
            {
                bodyNode.Nodes.Add(new TreeNode { Tag = PlaceholderActionTag });
            }
        }

        /// <summary>
        /// Builds the action child nodes for a body node from its Tag. UOP bodies enumerate their
        /// defined actions; MUL bodies use the named-action table. Called lazily on first expand and
        /// eagerly by <see cref="AddGraphic"/> for the freshly added body.
        /// </summary>
        private void PopulateActionNodes(TreeNode bodyNode)
        {
            int graphic = ((int[])bodyNode.Tag)[0];
            int actionType = GetActionNameType(((int[])bodyNode.Tag)[1]);

            TreeViewMobs.BeginUpdate();
            try
            {
                // Actions past the type's name table (UOP-specific bins) get a generic "ActionN"
                foreach (int i in GetDefinedActions(graphic, actionType))
                {
                    string actionName = i < GetActionNames[actionType].Length
                        ? GetActionNames[actionType][i]
                        : $"Action{i}";

                    bodyNode.Nodes.Add(new TreeNode($"{i} {actionName}") { Tag = i });
                }
            }
            finally
            {
                TreeViewMobs.EndUpdate();
            }
        }

        private void TreeViewMobs_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            TreeNode node = e.Node;
            // Only body nodes (direct children of the Mobs/Equipment roots) carry a placeholder. Replace
            // it with the real action nodes on first expand; once built, leave the node alone.
            if (node.Parent == null ||
                (node.Parent.Name != "Mobs" && node.Parent.Name != "Equipment"))
            {
                return;
            }

            if (node.Nodes.Count == 1 && node.Nodes[0].Tag is int tag && tag == PlaceholderActionTag)
            {
                node.Nodes.Clear();
                PopulateActionNodes(node);
            }
        }

        /// <summary>
        /// Computes the body's source file name for the tooltip lazily on first hover. Building it for
        /// every node up front is expensive (the UOP path probes up to <see cref="Animations.MaxAnimActions"/>
        /// hashes per body), yet the tooltip is only ever shown on hover. Only body nodes carry an int[]
        /// Tag; root and action nodes are skipped. The result is cached on the node so the lookup runs once.
        /// </summary>
        private void TreeViewMobs_NodeMouseHover(object sender, TreeNodeMouseHoverEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Node.ToolTipText) && e.Node.Tag is int[] tag)
            {
                e.Node.ToolTipText = Animations.GetFileName(tag[0]);
            }
        }

        private void SearchToolStripTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            string text = searchToolStripTextBox.Text;
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            // A numeric value (decimal or 0x hex) searches by body id; anything else is a
            // case-insensitive substring match against the displayed body name.
            bool byId = Utils.ConvertStringToInt(text, out int id, 0, Animations.MaxAnimationValue);

            foreach (TreeNode root in TreeViewMobs.Nodes)
            {
                foreach (TreeNode node in root.Nodes)
                {
                    bool match = byId
                        ? ((int[])node.Tag)[0] == id
                        : node.Text.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0;
                    if (!match)
                    {
                        continue;
                    }

                    TreeViewMobs.SelectedNode = node;
                    node.EnsureVisible();
                    return;
                }
            }
        }

        private void LoadListView()
        {
            _listViewGraphics.Clear();
            _listViewNodes.Clear();
            foreach (TreeNode node in TreeViewMobs.Nodes[_displayType].Nodes)
            {
                _listViewGraphics.Add(((int[])node.Tag)[0]);
                _listViewNodes.Add(node);
            }
            listView.VirtualListSize = _listViewGraphics.Count;
            listView.Invalidate();
        }

        private void SelectChanged_listView(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (!e.IsSelected)
            {
                return;
            }

            if (e.ItemIndex < 0 || e.ItemIndex >= _listViewNodes.Count)
            {
                return;
            }

            TreeViewMobs.SelectedNode = _listViewNodes[e.ItemIndex];
        }

        private void ListView_DoubleClick(object sender, MouseEventArgs e)
        {
            tabControl1.SelectTab(tabPage1);
        }

        private void ListViewDrawItem(object sender, TileViewControl.DrawTileListItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= _listViewGraphics.Count)
            {
                return;
            }

            int graphic = _listViewGraphics[e.Index];
            // Action 0 is not necessarily defined (e.g. equipment such as bodies 320/321). Use the first
            // defined action recorded in the node Tag so this works without expanding the body (action
            // nodes are built lazily). A body with no animation (-1) falls back to 0 and simply draws nothing.
            int action = ((int[])_listViewNodes[e.Index].Tag)[2];
            if (action < 0)
            {
                action = 0;
            }
            Point itemPoint = new Point(e.Bounds.X + listView.TilePadding.Left, e.Bounds.Y + listView.TilePadding.Top);
            Rectangle tileRect = new Rectangle(itemPoint, listView.TileSize);
            using var previousClip = e.Graphics.Clip;
            using var clipRegion = new Region(tileRect);
            e.Graphics.Clip = clipRegion;

            if (!listView.SelectedIndices.Contains(e.Index))
            {
                using var bgBrush = new SolidBrush(listView.BackColor);
                e.Graphics.FillRectangle(bgBrush, tileRect);
            }

            int hue = 0;
            // Cache-owned bitmap — borrowed for drawing only, never disposed here.
            Bitmap bmp = Animations.GetAnimation(graphic, action, 1, ref hue, false, true)?[0].Bitmap;
            if (bmp != null)
            {
                int maxW = tileRect.Width;
                int maxH = tileRect.Height - 18;
                int drawWidth = bmp.Width;
                int drawHeight = bmp.Height;
                if (drawWidth > maxW || drawHeight > maxH)
                {
                    float scale = Math.Min((float)maxW / drawWidth, (float)maxH / drawHeight);
                    drawWidth = (int)(drawWidth * scale);
                    drawHeight = (int)(drawHeight * scale);
                }
                int drawX = tileRect.X + (tileRect.Width - drawWidth) / 2;
                int drawY = tileRect.Y + Math.Max(0, (tileRect.Height - 18 - drawHeight) / 2);
                e.Graphics.DrawImage(bmp, drawX, drawY, drawWidth, drawHeight);
            }

            using var stringFormat = new StringFormat();
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.LineAlignment = StringAlignment.Far;

            e.Graphics.DrawString($"({graphic})", listView.Font, SystemBrushes.ControlText,
                new RectangleF(tileRect.X, tileRect.Y, tileRect.Width, tileRect.Height), stringFormat);

            e.Graphics.Clip = previousClip;
        }

        private HuePopUpForm _showForm;

        private void OnClick_Hue(object sender, EventArgs e)
        {
            if (_showForm?.IsDisposed == false)
            {
                return;
            }

            _showForm = _customHue == 0
                ? new HuePopUpForm(ChangeHue, _defHue + 1)
                : new HuePopUpForm(ChangeHue, _customHue - 1);

            _showForm.TopMost = true;
            _showForm.Show();
        }

        private void LoadListViewFrames()
        {
            listView1.BeginUpdate();
            try
            {
                listView1.Clear();
                for (int frame = 0; frame < MainPictureBox.Frames?.Count; ++frame)
                {
                    ListViewItem item = new ListViewItem(frame.ToString(), 0)
                    {
                        Tag = frame
                    };
                    listView1.Items.Add(item);
                }
            }
            finally
            {
                listView1.EndUpdate();
            }
        }

        private void Frames_ListView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            if (MainPictureBox.Frames == null)
            {
                return;
            }

            int frameIndex = (int)e.Item.Tag;
            if (frameIndex < 0 || frameIndex >= MainPictureBox.Frames.Count)
            {
                return;
            }

            Bitmap bmp = MainPictureBox.Frames[frameIndex].Bitmap;
            int width = bmp.Width;
            int height = bmp.Height;

            if (width > e.Bounds.Width)
            {
                width = e.Bounds.Width;
            }

            if (height > e.Bounds.Height)
            {
                height = e.Bounds.Height;
            }

            if (listView1.SelectedItems.Contains(e.Item))
            {
                using var highlightBrush = new SolidBrush(SystemColors.Highlight);
                e.Graphics.FillRectangle(highlightBrush, e.Bounds);
            }

            e.Graphics.DrawImage(bmp, e.Bounds.X, e.Bounds.Y, width, height);
            TextRenderer.DrawText(e.Graphics, e.Item.Text, listView1.Font, e.Bounds, SystemColors.ControlText, TextFormatFlags.Bottom | TextFormatFlags.HorizontalCenter);

            using (var pen = new Pen(SystemColors.ControlText))
            {
                e.Graphics.DrawRectangle(pen, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
            }
        }

        private void OnScrollFacing(object sender, EventArgs e)
        {
            _facing = (FacingBar.Value - 3) & 7;
            CurrentSelect = CurrentSelect;
        }

        private void OnClick_Sort(object sender, EventArgs e)
        {
            _sortAlpha = !_sortAlpha;

            TreeViewMobs.BeginUpdate();
            try
            {
                TreeViewMobs.TreeViewNodeSorter = !_sortAlpha ? new GraphicSorter() : (IComparer)new AlphaSorter();
                TreeViewMobs.Sort();
            }
            finally
            {
                TreeViewMobs.EndUpdate();
            }

            LoadListView();
        }

        private void OnClickRemove(object sender, EventArgs e)
        {
            TreeNode node = TreeViewMobs.SelectedNode;
            if (node?.Parent == null)
            {
                return;
            }

            if (node.Parent.Name != "Mobs" && node.Parent.Name != "Equipment")
            {
                node = node.Parent;
            }

            node.Remove();
            LoadListView();
        }

        private AnimationEditForm _animEditFormEntry;

        private void OnClickAnimationEdit(object sender, EventArgs e)
        {
            if (_animEditFormEntry?.IsDisposed == false)
            {
                return;
            }

            _animEditFormEntry = new AnimationEditForm();
            //animEditEntry.TopMost = true; // TODO: should it be topMost?
            _animEditFormEntry.Show();
        }

        private AnimationListNewEntriesForm _animNewEntryForm;

        private void OnClickFindNewEntries(object sender, EventArgs e)
        {
            if (_animNewEntryForm?.IsDisposed == false)
            {
                return;
            }

            _animNewEntryForm = new AnimationListNewEntriesForm(IsAlreadyDefined, AddGraphic, GetActionNames)
            {
                TopMost = true
            };
            _animNewEntryForm.Show();
        }

        private void RewriteXml(object sender, EventArgs e)
        {
            string fileName = Path.Combine(Options.AppDataPath, "Animationlist.xml");

            using (new WaitCursorScope(this))
            {
                // Only the top-level body nodes are written, and only their graphic order matters.
                // Sorting the live TreeView would recursively reorder every action child node and force a
                // costly native re-layout/repaint, so sort lightweight in-memory snapshots instead.
                // Stray bodies that are neither defined in mobtypes.txt nor have any animation frames are
                // dropped so they are not persisted back into the XML.
                var mobNodes = TreeViewMobs.Nodes[0].Nodes.Cast<TreeNode>()
                    .Where(ShouldWriteNode)
                    .OrderBy(node => ((int[])node.Tag)[0]).ToList();
                var equipNodes = TreeViewMobs.Nodes[1].Nodes.Cast<TreeNode>()
                    .Where(ShouldWriteNode)
                    .OrderBy(node => ((int[])node.Tag)[0]).ToList();

                XmlDocument dom = new XmlDocument();
                XmlDeclaration decl = dom.CreateXmlDeclaration("1.0", "utf-8", null);
                dom.AppendChild(decl);
                XmlElement sr = dom.CreateElement("Graphics");
                XmlComment comment = dom.CreateComment("Entries in Mob tab");
                sr.AppendChild(comment);
                comment = dom.CreateComment("Name=Displayed name");
                sr.AppendChild(comment);
                comment = dom.CreateComment("body=Graphic");
                sr.AppendChild(comment);
                comment = dom.CreateComment("type=0:Monster, 1:Sea, 2:Animal, 3:Human/Equipment");
                sr.AppendChild(comment);

                XmlElement elem;
                foreach (TreeNode node in mobNodes)
                {
                    elem = dom.CreateElement("Mob");
                    elem.SetAttribute("name", GetXmlName(node.Text, ((int[])node.Tag)[0]));
                    elem.SetAttribute("body", ((int[])node.Tag)[0].ToString());
                    elem.SetAttribute("type", NormalizeXmlType(((int[])node.Tag)[1]).ToString());

                    sr.AppendChild(elem);
                }

                foreach (TreeNode node in equipNodes)
                {
                    elem = dom.CreateElement("Equip");
                    elem.SetAttribute("name", GetXmlName(node.Text, ((int[])node.Tag)[0]));
                    elem.SetAttribute("body", ((int[])node.Tag)[0].ToString());
                    elem.SetAttribute("type", NormalizeXmlType(((int[])node.Tag)[1]).ToString());
                    sr.AppendChild(elem);
                }
                dom.AppendChild(sr);
                dom.Save(fileName);
            }

            MessageBox.Show("XML saved", "Rewrite", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        /// <summary>
        /// Maps the internal node type to a value valid for Animationlist.xml.
        /// Equipment (4) is stored under the &lt;Equip&gt; element and written as Human/Equipment (3);
        /// only 0-3 are valid in the XML and any other value would be skipped on reload.
        /// </summary>
        private static int NormalizeXmlType(int type)
        {
            return type == (int)MobType.Equipment ? (int)MobType.Human : type;
        }

        /// <summary>
        /// Decides whether a body node should be persisted to Animationlist.xml. A node is kept only when
        /// it actually has animation frames - recorded as the first-defined-action element of its Tag
        /// (-1 means none). This works without expanding the node (action child nodes are built lazily)
        /// and drops bodies with no animations (undefined bodies and paperdoll-only equipment), which
        /// should not be written even when they have a mobtypes.txt entry.
        /// </summary>
        private static bool ShouldWriteNode(TreeNode node)
        {
            return ((int[])node.Tag)[2] != -1;
        }

        /// <summary>
        /// Returns the display name without the trailing " (0x{body:X})" suffix that is appended for the
        /// tree view. The body is already stored in its own attribute, so the hex value must not be saved
        /// into the name (it would otherwise accumulate across repeated rewrite/reload cycles).
        /// </summary>
        private static string GetXmlName(string nodeText, int body)
        {
            string suffix = $" (0x{body:X})";
            return nodeText.EndsWith(suffix, StringComparison.Ordinal)
                ? nodeText.Substring(0, nodeText.Length - suffix.Length)
                : nodeText;
        }

        private void Extract_Image_ClickBmp(object sender, EventArgs e)
        {
            ExtractImage(ImageFormat.Bmp);
        }

        private void Extract_Image_ClickTiff(object sender, EventArgs e)
        {
            ExtractImage(ImageFormat.Tiff);
        }

        private void Extract_Image_ClickJpg(object sender, EventArgs e)
        {
            ExtractImage(ImageFormat.Jpeg);
        }

        private void Extract_Image_ClickPng(object sender, EventArgs e)
        {
            ExtractImage(ImageFormat.Png);
        }

        private void ExtractImage(ImageFormat imageFormat)
        {
            string what = "Mob";
            if (_displayType == 1)
            {
                what = "Equipment";
            }

            string fileExtension = Utils.GetFileExtensionFor(imageFormat);
            string fileName = Path.Combine(Options.OutputPath, $"{what} {Utils.FormatExportId(_currentSelect)}.{fileExtension}");

            Bitmap sourceBitmap = MainPictureBox.CurrentFrame?.Bitmap;

            if (sourceBitmap == null)
            {
                return;
            }

            using (Bitmap newBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height))
            {
                using (Graphics newGraph = Graphics.FromImage(newBitmap))
                {
                    newGraph.FillRectangle(Brushes.White, 0, 0, newBitmap.Width, newBitmap.Height);
                    newGraph.DrawImage(sourceBitmap, new Point(0, 0));
                    newGraph.Save();
                }

                newBitmap.Save(fileName, imageFormat);
            }

            MessageBox.Show($"{what} saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void OnClickExtractAnimBmp(object sender, EventArgs e)
        {
            ExportAnimationFrames(ImageFormat.Bmp);
        }

        private void OnClickExtractAnimTiff(object sender, EventArgs e)
        {
            ExportAnimationFrames(ImageFormat.Tiff);
        }

        private void OnClickExtractAnimJpg(object sender, EventArgs e)
        {
            ExportAnimationFrames(ImageFormat.Jpeg);
        }

        private void OnClickExtractAnimPng(object sender, EventArgs e)
        {
            ExportAnimationFrames(ImageFormat.Png);
        }

        private void ExportAnimationFrames(ImageFormat imageFormat)
        {
            string what = "Mob";
            if (_displayType == 1)
            {
                what = "Equipment";
            }

            string fileExtension = Utils.GetFileExtensionFor(imageFormat);
            string fileName = Path.Combine(Options.OutputPath, $"{what} {Utils.FormatExportId(_currentSelect)}");

            var frames = MainPictureBox.Frames;
            if (frames == null || frames.Count == 0)
            {
                return;
            }

            // Aligned: every frame on one canvas, anchored at its center like the Gif export
            bool align = alignFramesToolStripMenuItem.Checked;
            var (drawCenter, canvasSize) = align ? frames.GetAnimationDetails() : (Point.Empty, Size.Empty);
            if (align && (canvasSize.Width <= 0 || canvasSize.Height <= 0))
            {
                return;
            }

            // Bmp and Jpg have no alpha
            bool transparent = align && (imageFormat.Equals(ImageFormat.Png) || imageFormat.Equals(ImageFormat.Tiff));

            for (int i = 0; i < frames.Count; ++i)
            {
                var frame = frames[i];
                if (frame?.Bitmap == null)
                {
                    continue;
                }

                Size size = align ? canvasSize : frame.Bitmap.Size;
                Point location = align ? frame.GetAlignedLocation(drawCenter) : Point.Empty;
                using (Bitmap newBitmap = new Bitmap(size.Width, size.Height))
                {
                    using (Graphics newGraph = Graphics.FromImage(newBitmap))
                    {
                        newGraph.Clear(transparent ? Color.Transparent : Color.White);
                        newGraph.DrawImage(frame.Bitmap, location);
                    }

                    newBitmap.Save($"{fileName}-{i}.{fileExtension}", imageFormat);
                }
            }

            string aligned = align ? $", aligned on a {canvasSize.Width}x{canvasSize.Height} canvas," : string.Empty;
            FileSavedDialog.Show(FindForm(), Options.OutputPath, $"Files with following format {fileName}-X.{fileExtension}{aligned} saved successfully.");
        }

        private void OnClickExportFrameBmp(object sender, EventArgs e)
        {
            ExportSingleFrame(ImageFormat.Bmp);
        }

        private void OnClickExportFrameTiff(object sender, EventArgs e)
        {
            ExportSingleFrame(ImageFormat.Tiff);
        }

        private void OnClickExportFrameJpg(object sender, EventArgs e)
        {
            ExportSingleFrame(ImageFormat.Jpeg);
        }

        private void OnClickExportFramePng(object sender, EventArgs e)
        {
            ExportSingleFrame(ImageFormat.Png);
        }

        private void ExportSingleFrame(ImageFormat imageFormat)
        {
            if (listView1.SelectedItems.Count < 1)
            {
                return;
            }

            string what = "Mob";
            if (_displayType == 1)
            {
                what = "Equipment";
            }

            string fileExtension = Utils.GetFileExtensionFor(imageFormat);
            string fileName = Path.Combine(Options.OutputPath, $"{what} {Utils.FormatExportId(_currentSelect)}");

            Bitmap bit = MainPictureBox.Frames[(int)listView1.SelectedItems[0].Tag].Bitmap;
            using (Bitmap newBitmap = new Bitmap(bit.Width, bit.Height))
            {
                using (Graphics newGraph = Graphics.FromImage(newBitmap))
                {
                    newGraph.FillRectangle(Brushes.White, 0, 0, newBitmap.Width, newBitmap.Height);
                    newGraph.DrawImage(bit, new Point(0, 0));
                    newGraph.Save();
                }

                newBitmap.Save($"{fileName}-{(int)listView1.SelectedItems[0].Tag}.{fileExtension}", imageFormat);
            }
        }

        private void OnClickExportAllThumbnailsBmp(object sender, EventArgs e)
        {
            ExportAllThumbnails(ImageFormat.Bmp);
        }

        private void OnClickExportAllThumbnailsTiff(object sender, EventArgs e)
        {
            ExportAllThumbnails(ImageFormat.Tiff);
        }

        private void OnClickExportAllThumbnailsJpg(object sender, EventArgs e)
        {
            ExportAllThumbnails(ImageFormat.Jpeg);
        }

        private void OnClickExportAllThumbnailsPng(object sender, EventArgs e)
        {
            ExportAllThumbnails(ImageFormat.Png);
        }

        private void ExportAllThumbnails(ImageFormat imageFormat)
        {
            if (_listViewGraphics.Count == 0)
            {
                return;
            }

            string thumbnailPath = Path.Combine(Options.AppDataPath, "thumbnails");
            if (!Directory.Exists(thumbnailPath))
            {
                Directory.CreateDirectory(thumbnailPath);
            }

            string what = _displayType == 1 ? "Equipment" : "Mob";
            string fileExtension = Utils.GetFileExtensionFor(imageFormat);

            using (new WaitCursorScope(this))
            using (new ProgressBarDialog(_listViewGraphics.Count, $"Export to {fileExtension}", false))
            {
                for (int i = 0; i < _listViewGraphics.Count; ++i)
                {
                    ControlEvents.FireProgressChangeEvent();
                    Application.DoEvents();

                    int graphic = _listViewGraphics[i];
                    int action = ((int[])_listViewNodes[i].Tag)[2];
                    if (action < 0)
                    {
                        action = 0;
                    }

                    int hue = 0;
                    // Cache-owned bitmap — clone before saving, never save/dispose it directly.
                    Bitmap sourceBitmap = Animations.GetAnimation(graphic, action, 1, ref hue, false, true)?[0].Bitmap;
                    if (sourceBitmap == null)
                    {
                        continue;
                    }

                    string fileName = Path.Combine(thumbnailPath, $"{what} {Utils.FormatExportId(graphic)}.{fileExtension}");

                    if (imageFormat == ImageFormat.Png)
                    {
                        // Only Png preserves transparency - clone as-is instead of flattening onto white.
                        using (Bitmap bit = new Bitmap(sourceBitmap))
                        {
                            bit.Save(fileName, imageFormat);
                        }

                        continue;
                    }

                    using (Bitmap newBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height))
                    {
                        using (Graphics newGraph = Graphics.FromImage(newBitmap))
                        {
                            newGraph.FillRectangle(Brushes.White, 0, 0, newBitmap.Width, newBitmap.Height);
                            newGraph.DrawImage(sourceBitmap, new Point(0, 0));
                            newGraph.Save();
                        }

                        newBitmap.Save(fileName, imageFormat);
                    }
                }
            }

            FileSavedDialog.Show(FindForm(), thumbnailPath, "All thumbnails saved successfully.");
        }

        private void ExportAnimatedGif(bool looping)
        {
            if (MainPictureBox.Frames == null)
            {
                return;
            }

            var outputFile = Path.Combine(Options.OutputPath, $"{(_displayType == 1 ? "Equipment" : "Mob")} {_currentSelect}.gif");
            MainPictureBox.Frames.ToGif(outputFile, looping: looping, delay: 150, showFrameBounds: MainPictureBox.ShowFrameBounds);

            FileSavedDialog.Show(FindForm(), outputFile, "InGame Anim saved successfully.");
        }

        private void OnClickExtractAnimGifLooping(object sender, EventArgs e)
        {
            ExportAnimatedGif(true);
        }

        private void OnClickExtractAnimGifNoLooping(object sender, EventArgs e)
        {
            ExportAnimatedGif(false);
        }

        private void Frames_ListView_Click(object sender, EventArgs e)
        {
            var index = listView1.SelectedIndices.Count > 0 ? listView1.SelectedIndices[0] : 0;
            MainPictureBox.FrameIndex = index;
        }

        private void AnimateCheckBox_Click(object sender, EventArgs e)
        {
            MainPictureBox.Animate = !MainPictureBox.Animate;
            AnimateCheckBox.Checked = MainPictureBox.Animate;
        }

        private void ShowFrameBoundsCheckBox_Click(object sender, EventArgs e)
        {
            MainPictureBox.ShowFrameBounds = !MainPictureBox.ShowFrameBounds;
            ShowFrameBoundsCheckBox.Checked = MainPictureBox.ShowFrameBounds;
        }

        /// <summary>
        /// The action-name table of the body node for <see cref="_currentSelect"/>, found from the tree
        /// selection or, when the body was picked from the thumbnail list, by searching the tree.
        /// </summary>
        private int GetSelectedBodyActionType()
        {
            TreeNode node = TreeViewMobs.SelectedNode;
            if (node?.Tag is int && node.Parent?.Tag is int[])
            {
                node = node.Parent;
            }

            if (node?.Tag is int[] selectedTag && selectedTag[0] == _currentSelect)
            {
                return GetActionNameType(selectedTag[1]);
            }

            string rootName = _displayType == 1 ? "Equipment" : "Mobs";
            foreach (TreeNode root in TreeViewMobs.Nodes)
            {
                if (root.Name != rootName)
                {
                    continue;
                }

                foreach (TreeNode bodyNode in root.Nodes)
                {
                    if (bodyNode.Tag is int[] tag && tag[0] == _currentSelect)
                    {
                        return GetActionNameType(tag[1]);
                    }
                }
            }

            // Largest name table, so no defined MUL action is missed
            return (int)MobType.Human;
        }

        private void OnClickPackFrames(object sender, EventArgs e)
        {
            if (_currentSelect == 0)
            {
                MessageBox.Show("No animation selected.", "Pack Frames", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var optionsForm = new PackOptionsForm();
            if (optionsForm.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            using var folderDialog = new FolderBrowserDialog
            {
                Description = "Select the folder to save the packed sheets in",
                ShowNewFolderButton = true,
                SelectedPath = SpriteSheetPacker.LastFolder
            };
            if (folderDialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            SpriteSheetPacker.LastFolder = folderDialog.SelectedPath;

            string outDir = folderDialog.SelectedPath;
            int body = _currentSelect;

            var saved = new List<string>();
            try
            {
                using (new WaitCursorScope(this))
                {
                    List<int> actions = optionsForm.ExportAllActions
                        ? GetDefinedActions(body, GetSelectedBodyActionType())
                        : new List<int> { _currentSelectAction };

                    foreach (int action in actions)
                    {
                        string baseName = PackAnimation(outDir, body, action, optionsForm);
                        if (baseName != null)
                        {
                            saved.Add(baseName);
                        }
                    }
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Runtime.InteropServices.ExternalException)
            {
                MessageBox.Show($"Failed to pack frames: {ex.Message}", "Pack Frames", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (saved.Count == 0)
            {
                MessageBox.Show("No frames found to pack.", "Pack Frames", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string message = saved.Count == 1
                ? $"Packed sheet {Path.GetFileName(saved[0])} (.png, .json and _guide.png) saved successfully."
                : $"{saved.Count} packed sheets anim_{body}_<action> (.png, .json and _guide.png) saved successfully.";
            FileSavedDialog.Show(FindForm(), outDir, message);
        }

        /// <summary>
        /// Packs the frames of the chosen directions of one action into <c>anim_{body}_{action}.png</c>
        /// with a <c>.json</c> descriptor, a <c>_guide.png</c> showing frame bounds and anchors and, with one
        /// row per direction, a <c>_rows.txt</c> naming the rows.
        /// </summary>
        /// <returns>The output path without extension, or null if the action has no frames.</returns>
        private static string PackAnimation(string outDir, int body, int action, PackOptionsForm options)
        {
            var layout = new SpriteSheetLayout(options.MaxWidth, options.Spacing);
            var entries = new List<PackedFrameEntry>();
            var images = new List<Bitmap>();
            var rows = new StringBuilder();
            int rowCount = 0;

            try
            {
                foreach (int direction in options.SelectedDirections)
                {
                    // No hue: pack the stored colours, not a body.def hue, so the sheet imports back unchanged
                    int hue = 0;
                    AnimationFrame[] frames = Animations.GetAnimation(body, action, direction, ref hue, true, false);
                    if (frames == null || !frames.Any(frame => frame?.Bitmap != null))
                    {
                        continue;
                    }

                    if (options.OneRowPerDirection)
                    {
                        layout.NewRow();
                        rows.AppendLine($"Row {rowCount++}: {direction} {PackOptionsForm.DirectionNames[direction]}");
                    }

                    for (int i = 0; i < frames.Length; ++i)
                    {
                        AnimationFrame frame = frames[i];
                        if (frame?.Bitmap == null)
                        {
                            continue;
                        }

                        PackedRect rect = layout.Place(frame.Bitmap.Width, frame.Bitmap.Height, wrap: !options.OneRowPerDirection);
                        entries.Add(new PackedFrameEntry
                        {
                            Direction = direction,
                            Index = i,
                            Frame = rect,
                            Center = new PackedPoint { X = frame.Center.X, Y = frame.Center.Y }
                        });

                        // The frames are cache-owned
                        images.Add(new Bitmap(frame.Bitmap));
                    }
                }

                if (entries.Count == 0)
                {
                    return null;
                }

                string baseName = Path.Combine(outDir, $"anim_{body}_{action}");
                string imageFile = baseName + ".png";
                List<PackedRect> rects = entries.Select(entry => entry.Frame).ToList();

                using (Bitmap sheet = SpriteSheetPacker.Compose(layout.Width, layout.Height, images, rects))
                {
                    sheet.Save(imageFile, ImageFormat.Png);
                    SpriteSheetPacker.WriteJson(baseName + ".json", new PackedAnimation
                    {
                        Meta = SpriteSheetPacker.CreateMeta(imageFile, sheet),
                        Frames = entries
                    });

                    // The anchor is where the frame's center is drawn, as in GetAlignedLocation
                    List<Point?> anchors = entries
                        .Select(entry => (Point?)new Point(entry.Frame.X + entry.Center.X, entry.Frame.Y + entry.Frame.H + entry.Center.Y))
                        .ToList();
                    SpriteSheetPacker.SaveGuide(baseName + "_guide.png", sheet, rects, anchors);
                }

                if (options.OneRowPerDirection)
                {
                    File.WriteAllText(baseName + "_rows.txt", rows.ToString());
                }

                return baseName;
            }
            finally
            {
                foreach (Bitmap image in images)
                {
                    image.Dispose();
                }
            }
        }

        private const string SaveAnimationsHint =
            "Save with Settings > Animation Edit > Misc > Save; this tab shows the new frames after the files are reloaded.";

        private static readonly Regex _packedAnimationFileName = new Regex(@"^anim_(\d+)_(\d+)$", RegexOptions.CultureInvariant);

        /// <summary>
        /// Body and action from a packed sheet file name <c>anim_{body}_{action}.json</c>.
        /// </summary>
        private static bool TryParsePackedAnimationName(string fileName, out int body, out int action)
        {
            body = action = 0;
            Match match = _packedAnimationFileName.Match(Path.GetFileNameWithoutExtension(fileName));
            return match.Success &&
                   int.TryParse(match.Groups[1].Value, out body) &&
                   int.TryParse(match.Groups[2].Value, out action);
        }

        private bool CanUnpackInto(int body, string title)
        {
            if (body == 0)
            {
                MessageBox.Show("No animation selected.", title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            // AnimationEdit only writes the anim.mul files
            if (Animations.IsUopBody(body))
            {
                MessageBox.Show($"Body {body} is stored in a UOP file. Frames can only be imported into bodies from the anim.mul files.",
                    title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            return true;
        }

        /// <returns>True to replace the existing frames, false to append, null to cancel.</returns>
        private static bool? AskReplaceFrames(string title, string what)
        {
            DialogResult result = MessageBox.Show(
                $"{what}\n\nYes = replace the existing frames\nNo = append to the existing frames\nCancel = abort",
                title, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button3);

            return result switch
            {
                DialogResult.Yes => true,
                DialogResult.No => false,
                _ => null
            };
        }

        private static bool IsUnpackException(Exception ex)
        {
            // Bitmap reports an unreadable image as ArgumentException or OutOfMemoryException
            return ex is IOException or InvalidDataException or UnauthorizedAccessException or ArgumentException or
                OutOfMemoryException or System.Runtime.InteropServices.ExternalException;
        }

        private void OnClickUnpackFrames(object sender, EventArgs e)
        {
            const string title = "Unpack Frames";

            int body = _currentSelect;
            int action = _currentSelectAction;
            if (!CanUnpackInto(body, title))
            {
                return;
            }

            using var fileDialog = new OpenFileDialog
            {
                InitialDirectory = SpriteSheetPacker.LastFolder,
                Filter = "Packed sheet (*.json)|*.json|All files (*.*)|*.*",
                Title = "Select the Json file of a packed sheet"
            };
            if (fileDialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            SpriteSheetPacker.LastFolder = Path.GetDirectoryName(fileDialog.FileName);

            string what = $"Import {Path.GetFileName(fileDialog.FileName)} into body {body}, action {action}?";
            if (TryParsePackedAnimationName(fileDialog.FileName, out int packedBody, out int packedAction) &&
                (packedBody != body || packedAction != action))
            {
                what += $"\nThe sheet was packed from body {packedBody}, action {packedAction}.";
            }

            bool? replace = AskReplaceFrames(title, what);
            if (replace == null)
            {
                return;
            }

            UnpackResult result;
            try
            {
                using (new WaitCursorScope(this))
                {
                    result = UnpackAnimation(fileDialog.FileName, body, action, replace.Value);
                }
            }
            catch (Exception ex) when (IsUnpackException(ex))
            {
                MessageBox.Show($"Failed to import frames: {ex.Message}", title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Options.ChangedUltimaClass["Animations"] = true;

            MessageBox.Show($"Imported {result.Frames} frames in {result.Directions} direction(s) into body {body}, action {action}.{result.Notes}\n\n{SaveAnimationsHint}",
                title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnClickBulkUnpackFrames(object sender, EventArgs e)
        {
            const string title = "Bulk Unpack Frames";

            int body = _currentSelect;
            if (!CanUnpackInto(body, title))
            {
                return;
            }

            using var folderDialog = new FolderBrowserDialog
            {
                Description = $"Select the folder holding the packed sheets anim_{body}_<action>.json",
                ShowNewFolderButton = false,
                SelectedPath = SpriteSheetPacker.LastFolder
            };
            if (folderDialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            SpriteSheetPacker.LastFolder = folderDialog.SelectedPath;

            // Only sheets of the selected body are imported; their actions come from the file names
            var sheets = new SortedDictionary<int, string>();
            int otherFiles = 0;
            foreach (string file in Directory.GetFiles(folderDialog.SelectedPath, "*.json"))
            {
                if (TryParsePackedAnimationName(file, out int packedBody, out int packedAction) && packedBody == body)
                {
                    sheets[packedAction] = file;
                }
                else
                {
                    otherFiles++;
                }
            }

            if (sheets.Count == 0)
            {
                MessageBox.Show($"No packed sheets anim_{body}_<action>.json found in {folderDialog.SelectedPath}.",
                    title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool? replace = AskReplaceFrames(title, $"Import {sheets.Count} packed sheet(s) into body {body}, actions {string.Join(", ", sheets.Keys)}?");
            if (replace == null)
            {
                return;
            }

            int imported = 0;
            int frames = 0;
            var errors = new List<string>();
            using (new WaitCursorScope(this))
            {
                foreach (var (action, file) in sheets)
                {
                    try
                    {
                        UnpackResult result = UnpackAnimation(file, body, action, replace.Value);
                        imported++;
                        frames += result.Frames;
                        if (result.Notes.Length > 0)
                        {
                            errors.Add($"{Path.GetFileName(file)}:{result.Notes.Replace('\n', ' ')}");
                        }
                    }
                    catch (Exception ex) when (IsUnpackException(ex))
                    {
                        errors.Add($"{Path.GetFileName(file)}: {ex.Message}");
                    }
                }
            }

            if (imported > 0)
            {
                Options.ChangedUltimaClass["Animations"] = true;
            }

            string message = $"Imported {imported} of {sheets.Count} sheet(s), {frames} frames, into body {body}.";
            if (otherFiles > 0)
            {
                message += $"\nIgnored {otherFiles} other Json file(s).";
            }

            if (errors.Count > 0)
            {
                const int maxShown = 10;
                message += "\n\n" + string.Join("\n", errors.Take(maxShown));
                if (errors.Count > maxShown)
                {
                    message += $"\n... and {errors.Count - maxShown} more.";
                }
            }

            if (imported > 0)
            {
                message += $"\n\n{SaveAnimationsHint}";
            }

            MessageBox.Show(message, title, MessageBoxButtons.OK, errors.Count > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }

        private sealed class UnpackResult
        {
            public int Frames { get; set; }
            public int Directions { get; set; }

            /// <summary>What was skipped, each line starting with a line break; empty if nothing was.</summary>
            public string Notes { get; set; } = string.Empty;
        }

        /// <summary>
        /// Imports a packed sheet into <paramref name="action"/> of <paramref name="body"/> (a MUL body).
        /// The sheet is read and checked before anything changes, so a failure leaves the animation as it was.
        /// </summary>
        /// <param name="replace">Replace each imported direction's frames instead of appending to them.</param>
        private static UnpackResult UnpackAnimation(string jsonFile, int body, int action, bool replace)
        {
            string name = Path.GetFileName(jsonFile);
            PackedAnimation packed = SpriteSheetPacker.ReadJson<PackedAnimation>(jsonFile);
            if (packed.Frames == null || packed.Frames.Count == 0)
            {
                throw new InvalidDataException($"{name} lists no frames.");
            }

            int fileBody = body;
            Animations.Translate(ref fileBody);
            int fileType = BodyConverter.Convert(ref fileBody);
            if (action < 0 || action >= Animations.GetActionCapacity(fileBody, fileType))
            {
                throw new InvalidDataException($"Body {body} has no action {action}.");
            }

            var result = new UnpackResult();
            var skippedDirections = new SortedSet<int>();
            int skippedFrames = 0;
            var directions = new SortedDictionary<int, List<(Bitmap Bitmap, PackedPoint Center)>>();

            try
            {
                using (Bitmap sheet = SpriteSheetPacker.LoadSheet(jsonFile, packed.Meta))
                {
                    foreach (PackedFrameEntry entry in packed.Frames.Where(entry => entry != null).OrderBy(entry => entry.Index))
                    {
                        // 5-7 are drawn mirrored from 3-1 and are not stored
                        if (entry.Direction < 0 || entry.Direction >= PackOptionsForm.DirectionNames.Length)
                        {
                            skippedDirections.Add(entry.Direction);
                            continue;
                        }

                        if (!SpriteSheetPacker.IsInside(entry.Frame, sheet))
                        {
                            skippedFrames++;
                            continue;
                        }

                        if (!directions.TryGetValue(entry.Direction, out var frames))
                        {
                            directions[entry.Direction] = frames = new List<(Bitmap, PackedPoint)>();
                        }

                        frames.Add((SpriteSheetPacker.CropAnimationFrame(sheet, entry.Frame), entry.Center));
                    }
                }

                if (directions.Count == 0)
                {
                    throw new InvalidDataException($"No frame of {name} could be imported.");
                }

                var targets = new Dictionary<int, AnimIdx>();
                foreach (int direction in directions.Keys)
                {
                    targets[direction] = GetAnimationEditTarget(fileType, fileBody, action, direction, body);
                }

                foreach (var (direction, frames) in directions)
                {
                    AnimIdx edit = targets[direction];
                    if (replace)
                    {
                        edit.ClearFrames();
                    }

                    // One palette for the whole direction, fitted next to any frames that stay
                    edit.FitPalette(frames.Select(frame => frame.Bitmap));
                    foreach (var (bitmap, center) in frames)
                    {
                        edit.AddFrame(bitmap, center?.X ?? 0, center?.Y ?? 0);
                    }

                    result.Frames += frames.Count;
                    result.Directions++;
                }
            }
            finally
            {
                foreach (var frames in directions.Values)
                {
                    foreach (var (bitmap, _) in frames)
                    {
                        bitmap.Dispose();
                    }
                }
            }

            if (skippedDirections.Count > 0)
            {
                result.Notes += $"\nSkipped direction(s) {string.Join(", ", skippedDirections)}: only 0-4 are stored.";
            }

            if (skippedFrames > 0)
            {
                result.Notes += $"\nSkipped {skippedFrames} frame(s) outside the sheet image.";
            }

            return result;
        }

        private static AnimIdx GetAnimationEditTarget(int fileType, int fileBody, int action, int direction, int body)
        {
            try
            {
                return AnimationEdit.GetAnimation(fileType, fileBody, action, direction);
            }
            catch (Exception ex) when (ex is NullReferenceException or IndexOutOfRangeException)
            {
                // The anim file of this body is missing or too short for it
                throw new InvalidDataException($"Body {body} (anim{(fileType == 1 ? string.Empty : fileType.ToString())}.mul body {fileBody}) has no entry for action {action}, direction {direction}.", ex);
            }
        }
    }

    public class AlphaSorter : IComparer
    {
        public int Compare(object x, object y)
        {
            TreeNode tx = x as TreeNode;
            TreeNode ty = y as TreeNode;
            if (tx.Parent == null) // don't change Mob and Equipment
            {
                return (int)tx.Tag == -1 ? -1 : 1;
            }
            if (tx.Parent.Parent != null)
            {
                return (int)tx.Tag - (int)ty.Tag;
            }

            return string.CompareOrdinal(tx.Text, ty.Text);
        }
    }

    public class GraphicSorter : IComparer
    {
        public int Compare(object x, object y)
        {
            TreeNode tx = x as TreeNode;
            TreeNode ty = y as TreeNode;
            if (tx.Parent == null)
            {
                return (int)tx.Tag == -1 ? -1 : 1;
            }

            if (tx.Parent.Parent != null)
            {
                return (int)tx.Tag - (int)ty.Tag;
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
