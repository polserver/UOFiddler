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
using System.Text.Json;
using System.Text.Json.Serialization;
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
            // Add handlers for new context menu items
            packFramesToolStripMenuItem.Click += OnPackFramesClick;
            unpackFramesToolStripMenuItem.Click += OnUnpackFramesClick;
            bulkUnpackFramesToolStripMenuItem.Click += OnBulkUnpackFramesClick;
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

        private void AddUopActionNodes(TreeNode parent, int body, int actionType)
        {
            // UOP animations are not bounded by the legacy per-type group counts (Low=13, High=22,
            // People=34): a UOP body can define action bins well past them (the client scans up to
            // MaxAnimActions - "gargoyle is like 78"). List every defined UOP action, naming those within
            // the type's table and falling back to a generic "ActionN" for the higher UOP-specific bins.
            var definedActions = Animations.GetUopDefinedActions(body);
            foreach (int i in definedActions)
            {
                string actionName = i < GetActionNames[actionType].Length
                    ? GetActionNames[actionType][i]
                    : $"Action{i}";

                parent.Nodes.Add(new TreeNode($"{i} {actionName}") { Tag = i });
            }
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
                if (Animations.IsUopBody(graphic))
                {
                    AddUopActionNodes(bodyNode, graphic, actionType);
                }
                else
                {
                    for (int i = 0; i < GetActionNames[actionType].GetLength(0); ++i)
                    {
                        if (!Animations.IsActionDefined(graphic, i, 0))
                        {
                            continue;
                        }

                        bodyNode.Nodes.Add(new TreeNode($"{i} {GetActionNames[actionType][i]}") { Tag = i });
                    }
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

            for (int i = 0; i < MainPictureBox.Frames?.Count; ++i)
            {
                var frameBitmap = MainPictureBox.Frames[i].Bitmap;
                using (Bitmap newBitmap = new Bitmap(frameBitmap.Width, frameBitmap.Height))
                {
                    using (Graphics newGraph = Graphics.FromImage(newBitmap))
                    {
                        newGraph.FillRectangle(Brushes.White, 0, 0, newBitmap.Width, newBitmap.Height);
                        newGraph.DrawImage(frameBitmap, new Point(0, 0));
                        newGraph.Save();
                    }

                    newBitmap.Save($"{fileName}-{i}.{fileExtension}", imageFormat);
                }
            }

            FileSavedDialog.Show(FindForm(), Options.OutputPath, $"Files with following format {fileName}-X.{fileExtension} saved successfully.");
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

        private async void OnPackFramesClick(object? sender, EventArgs e)
        {
            if (_currentSelect == 0)
            {
                MessageBox.Show("No graphic selected.", "Pack Frames", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // show pack options dialog
            using var optionsForm = new PackOptionsForm();
            if (optionsForm.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            var selectedDirections = optionsForm.SelectedDirections; // list<int>
            int maxWidth = optionsForm.MaxWidth;
            bool oneRowPerDirection = optionsForm.OneRowPerDirection;
            int spacing = optionsForm.FrameSpacing;
            bool exportAll = optionsForm.ExportAllAnimations;

            // Ask for output base name/location
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.Description = "Select folder to save packed sprite and JSON";
                dlg.ShowNewFolderButton = true;
                if (dlg.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                string outDir = dlg.SelectedPath;

                try
                {
                    Cursor.Current = Cursors.WaitCursor;

                    if (exportAll)
                    {
                        int exportedCount = 0;
                        TreeNode? bodyNode = null;

                        // Find the body node based on current selection
                        if (TreeViewMobs.SelectedNode != null)
                        {
                            if (TreeViewMobs.SelectedNode.Tag is int[]) // It's a body node
                            {
                                bodyNode = TreeViewMobs.SelectedNode;
                            }
                            else if (TreeViewMobs.SelectedNode.Parent != null && TreeViewMobs.SelectedNode.Parent.Tag is int[]) // It's an action node
                            {
                                bodyNode = TreeViewMobs.SelectedNode.Parent;
                            }
                        }

                        if (bodyNode != null)
                        {
                            foreach (TreeNode node in bodyNode.Nodes)
                            {
                                if (node.Tag is int action)
                                {
                                    var result = PackSingleAnimation(outDir, _currentSelect, action, selectedDirections, maxWidth, oneRowPerDirection, spacing);
                                    if (result != null && result.Count > 0)
                                    {
                                        exportedCount++;
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Fallback if tree node structure is unexpected (shouldn't happen if _currentSelect is valid)
                            // Try to export just the current action or maybe a small range? 
                            // But better to warn or just do nothing if we can't find the nodes.
                            MessageBox.Show("Could not determine animation list from selection.", "Pack Frames", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }

                        if (exportedCount == 0)
                        {
                            MessageBox.Show("No animations found to export.", "Pack Frames", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show($"Exported {exportedCount} animations.", "Pack Frames", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else
                    {
                        var result = PackSingleAnimation(outDir, _currentSelect, _currentSelectAction, selectedDirections, maxWidth, oneRowPerDirection, spacing);
                        if (result != null && result.Count > 0)
                        {
                            string msg = $"Saved sprite: {result[0]}\nSaved JSON: {result[1]}";
                            if (result.Count > 2)
                            {
                                msg += $"\nSaved Info: {result[2]}";
                            }
                            MessageBox.Show(msg, "Pack Frames", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("No frames found to pack.", "Pack Frames", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to pack frames: {ex.Message}", "Pack Frames", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
            }
        }

        private List<string>? PackSingleAnimation(string outDir, int body, int action, List<int> selectedDirections, int maxWidth, bool oneRowPerDirection, int spacing)
        {
            // Collect frames for directions 0..4 (common editable directions)
            var packedFrames = new List<PackedFrameEntry>();
            var images = new List<Bitmap>();

            int currentX = spacing, currentY = spacing, rowHeight = 0, canvasWidth = 0, canvasHeight = 0;
            var rowMapping = new System.Text.StringBuilder();
            int rowIndex = 0;

            foreach (int dir in selectedDirections)
            {
                int localHue = 0;
                var frames = Animations.GetAnimation(body, action, dir, ref localHue, false, false);
                if (frames == null || frames.Length == 0)
                {
                    continue;
                }

                if (oneRowPerDirection)
                {
                    if (currentX > spacing)
                    {
                        currentY += rowHeight + spacing;
                        currentX = spacing;
                        rowHeight = 0;
                    }
                    rowMapping.AppendLine($"Row {rowIndex++}: Facing {GetDirectionName(dir)}");
                }

                for (int fi = 0; fi < frames.Length; fi++)
                {
                    var anim = frames[fi];
                    if (anim?.Bitmap == null)
                    {
                        continue;
                    }

                    // determine size
                    int w = anim.Bitmap.Width;
                    int h = anim.Bitmap.Height;

                    if (!oneRowPerDirection && currentX + w > maxWidth)
                    {
                        currentY += rowHeight + spacing;
                        currentX = spacing;
                        rowHeight = 0;
                    }

                    if (currentX == spacing)
                        rowHeight = h;
                    else
                        rowHeight = Math.Max(rowHeight, h);

                    var entry = new PackedFrameEntry
                    {
                        Direction = dir,
                        Index = fi,
                        Frame = new Rect { X = currentX, Y = currentY, W = w, H = h },
                        Center = new PointStruct { X = anim.Center.X, Y = anim.Center.Y }
                    };

                    packedFrames.Add(entry);

                    // store image copy
                    images.Add(new Bitmap(anim.Bitmap));

                    canvasWidth = Math.Max(canvasWidth, currentX + w + spacing); // Include right margin
                    currentX += w + spacing;
                    canvasHeight = Math.Max(canvasHeight, currentY + rowHeight + spacing); // Include bottom margin
                }
            }

            if (images.Count == 0)
            {
                return null;
            }

            // Create sprite sheet and paste images
            using (var sprite = new Bitmap(Math.Max(1, canvasWidth), Math.Max(1, canvasHeight)))
            using (var g = Graphics.FromImage(sprite))
            {
                g.Clear(Color.Transparent);

                for (int i = 0; i < images.Count; i++)
                {
                    var img = images[i];
                    var rect = packedFrames[i].Frame;
                    g.DrawImage(img, rect.X, rect.Y, rect.W, rect.H);
                    img.Dispose();
                }

                string baseName = $"anim_{body}_{action}";
                string imageFile = Path.Combine(outDir, baseName + ".png");
                sprite.Save(imageFile, ImageFormat.Png);

                // prepare JSON
                var outObj = new PackedOutput
                {
                    Meta = new PackedMeta { Image = Path.GetFileName(imageFile), Size = new SizeStruct { W = sprite.Width, H = sprite.Height }, Format = "RGBA8888" },
                    Frames = packedFrames
                };

                string jsonFile = Path.Combine(outDir, baseName + ".json");
                var jsOptions = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(outObj, jsOptions);
                File.WriteAllText(jsonFile, json);

                // Generate Debug Image
                string debugImageFile = Path.Combine(outDir, $"{baseName}_guide.png");
                AnimationDebugHelper.CreateDebugImage(debugImageFile, sprite, packedFrames);

                var result = new List<string> { imageFile, jsonFile, debugImageFile };

                if (oneRowPerDirection)
                {
                    string txtFile = Path.Combine(outDir, baseName + "_rows.txt");
                    File.WriteAllText(txtFile, rowMapping.ToString());
                    result.Add(txtFile);
                }

                return result;
            }
        }

        private void OnUnpackFramesClick(object? sender, EventArgs e)
        {
            if (_currentSelect == 0)
            {
                MessageBox.Show("No graphic selected.", "Unpack Frames", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                ofd.Title = "Select packing JSON file";
                if (ofd.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                string jsonFile = ofd.FileName;
                try
                {
                    UnpackAnimation(jsonFile, _currentSelect, _currentSelectAction, true);
                    MessageBox.Show("Import finished. Remember to save animations via AnimationEdit.Save if needed.", "Unpack Frames", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to unpack/import frames: {ex.Message}", "Unpack Frames", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OnBulkUnpackFramesClick(object? sender, EventArgs e)
        {
            if (_currentSelect == 0)
            {
                MessageBox.Show("No graphic selected.", "Bulk Unpack Frames", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dlg = new FolderBrowserDialog())
            {
                dlg.Description = "Select folder containing exported animations (JSON + PNG)";
                dlg.ShowNewFolderButton = false;

                if (dlg.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                string[] jsonFiles = Directory.GetFiles(dlg.SelectedPath, "*.json");
                if (jsonFiles.Length == 0)
                {
                    MessageBox.Show("No JSON files found in selected directory.", "Bulk Unpack Frames", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                int importedCount = 0;
                int errorCount = 0;

                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    // Ask user once for overwrite preference
                    DialogResult globalChoice = MessageBox.Show(
                        "Overwrite existing frames for all imported animations?\n\nYes = overwrite\nNo = append\nCancel = abort import",
                        "Bulk Unpack Frames", MessageBoxButtons.YesNoCancel);

                    if (globalChoice == DialogResult.Cancel)
                        return;

                    bool overwriteAll = (globalChoice == DialogResult.Yes);

                    foreach (string jsonFile in jsonFiles)
                    {
                        // Expected format: anim_{body}_{action}.json
                        string fileName = Path.GetFileNameWithoutExtension(jsonFile);
                        string[] parts = fileName.Split('_');

                        if (parts.Length >= 3 && parts[0] == "anim" && int.TryParse(parts[1], out int body) && int.TryParse(parts[2], out int action))
                        {
                            // Only import if body matches current selection (optional, but safer)
                            if (body == _currentSelect)
                            {
                                try
                                {
                                    UnpackAnimation(jsonFile, body, action, false, overwriteAll);
                                    importedCount++;
                                }
                                catch
                                {
                                    errorCount++;
                                }
                            }
                        }
                    }
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }

                MessageBox.Show($"Bulk import finished.\nImported: {importedCount}\nErrors: {errorCount}\n\nRemember to save animations via AnimationEdit.Save if needed.", "Bulk Unpack Frames", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void UnpackAnimation(string jsonFile, int body, int action, bool promptOverwrite, bool overwriteAll = false)
        {
            string json = File.ReadAllText(jsonFile);
            var doc = JsonSerializer.Deserialize<PackedOutput>(json);
            if (doc == null)
            {
                throw new Exception("Invalid JSON file.");
            }

            string spritePath = Path.Combine(Path.GetDirectoryName(jsonFile) ?? string.Empty, doc.Meta.Image);
            if (!File.Exists(spritePath))
            {
                throw new FileNotFoundException($"Sprite sheet not found: {spritePath}");
            }

            using (var sprite = new Bitmap(spritePath))
            {
                // determine body/fileType for import
                int bodyTrans = body;
                Animations.Translate(ref bodyTrans);
                int fileType = BodyConverter.Convert(ref bodyTrans);

                // Group frames by direction
                var groups = doc.Frames.GroupBy(f => f.Direction).ToDictionary(g => g.Key, g => g.OrderBy(f => f.Index).ToList());

                if (promptOverwrite)
                {
                    // Ask the user once how to handle existing frames
                    DialogResult globalChoice = MessageBox.Show(
                        "Overwrite existing frames for all directions?\n\nYes = overwrite\nNo = append\nCancel = abort import",
                        "Unpack Frames", MessageBoxButtons.YesNoCancel);

                    if (globalChoice == DialogResult.Cancel)
                        return;

                    overwriteAll = (globalChoice == DialogResult.Yes);
                }

                // Build palette once 
                var allRects = doc.Frames.Select(f => new RectangleF(f.Frame.X, f.Frame.Y, f.Frame.W, f.Frame.H)).ToList();
                var importPalette = BuildPaletteFromFrames(sprite, allRects, alphaThreshold: 4);

                foreach (var kv in groups)
                {
                    int dir = kv.Key;
                    if (dir > 4)
                    {
                        continue;
                    }
                    var framesList = kv.Value;

                    var animIdx = RequireAnimIdx(fileType, bodyTrans, action, dir);

                    if (overwriteAll) animIdx.ClearFrames();

                    animIdx.ReplacePalette(importPalette); // key for proper color mapping

                    int imported = 0;
                    foreach (var frameEntry in framesList)
                    {
                        var r = frameEntry.Frame;
                        // bounds guard to avoid GDI+ OutOfMemory on bad rects
                        if (r.W <= 0 || r.H <= 0 || r.X < 0 || r.Y < 0 ||
                            r.X + r.W > sprite.Width || r.Y + r.H > sprite.Height)
                            continue;

                        using (var bit16 = Extract1555Region(sprite, new Rectangle(r.X, r.Y, r.W, r.H), alphaThreshold: 4))
                        {
                            animIdx.AddFrame(bit16, frameEntry.Center.X, frameEntry.Center.Y);
                        }

                        // light GC throttle for very large imports
                        if ((++imported & 127) == 0)
                        {
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                        }
                    }
                }

                Options.ChangedUltimaClass["Animations"] = true;
                if (body == _currentSelect)
                {
                    CurrentSelect = CurrentSelect;   // this calls SetPicture() and repopulates frames
                }
            }
        }

        // --- Reflection helpers (safe & cached) ---
        private static ushort[] BuildPaletteFromFrames(Bitmap sprite, IEnumerable<RectangleF> rects, byte alphaThreshold = 4)
        {
            var freq = new Dictionary<ushort, int>(capacity: 4096);

            // Lock the whole sprite once (32bpp ARGB)
            var sheetRect = new Rectangle(0, 0, sprite.Width, sprite.Height);
            var sData = sprite.LockBits(sheetRect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                unsafe
                {
                    byte* basePtr = (byte*)sData.Scan0;
                    int stride = sData.Stride;

                    foreach (var rf in rects)
                    {
                        var r = Rectangle.Round(rf);
                        if (r.Width <= 0 || r.Height <= 0) continue;
                        if (r.X < 0 || r.Y < 0 || r.Right > sprite.Width || r.Bottom > sprite.Height) continue;

                        for (int y = 0; y < r.Height; y++)
                        {
                            byte* src = basePtr + (r.Y + y) * stride + r.X * 4;
                            for (int x = 0; x < r.Width; x++)
                            {
                                byte b = src[0], g = src[1], r8 = src[2], a = src[3];
                                src += 4;

                                if (a <= alphaThreshold) continue; // transparent -> skip

                                ushort col =
                                    (ushort)(0x8000 | ((r8 >> 3) << 10) | ((g >> 3) << 5) | (b >> 3)); // A1R5G5B5

                                if (freq.TryGetValue(col, out int n)) freq[col] = n + 1;
                                else freq[col] = 1;
                            }
                        }
                    }
                }
            }
            finally
            {
                sprite.UnlockBits(sData);
            }

            var palette = new ushort[256];
            int i = 0;
            foreach (var kv in freq.OrderByDescending(kv => kv.Value).Take(256))
                palette[i++] = kv.Key;

            return palette;
        }

        private static Bitmap Extract1555Region(Bitmap sprite, Rectangle rect, byte alphaThreshold = 4)
        {
            // Bounds guard
            if (rect.Width <= 0 || rect.Height <= 0) throw new ArgumentException("Empty region.");
            if (rect.X < 0 || rect.Y < 0 || rect.Right > sprite.Width || rect.Bottom > sprite.Height)
                throw new ArgumentOutOfRangeException(nameof(rect), "Region outside sprite bounds.");

            // Dest: 16bpp A1R5G5B5
            Bitmap dst16 = new Bitmap(rect.Width, rect.Height, PixelFormat.Format16bppArgb1555);

            var sheetRect = new Rectangle(0, 0, sprite.Width, sprite.Height);
            var sData = sprite.LockBits(sheetRect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var dData = dst16.LockBits(new Rectangle(0, 0, rect.Width, rect.Height), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);

            try
            {
                unsafe
                {
                    byte* sBase = (byte*)sData.Scan0;
                    int sStride = sData.Stride;

                    byte* dBase = (byte*)dData.Scan0;
                    int dStride = dData.Stride;

                    for (int y = 0; y < rect.Height; y++)
                    {
                        byte* sp = sBase + (rect.Y + y) * sStride + rect.X * 4;
                        ushort* dp = (ushort*)(dBase + y * dStride);

                        for (int x = 0; x < rect.Width; x++)
                        {
                            byte b = sp[0], g = sp[1], r = sp[2], a = sp[3];
                            sp += 4;

                            if (a <= alphaThreshold)
                                dp[x] = 0; // transparent
                            else
                                dp[x] = (ushort)(0x8000 | ((r >> 3) << 10) | ((g >> 3) << 5) | (b >> 3));
                        }
                    }
                }
            }
            finally
            {
                sprite.UnlockBits(sData);
                dst16.UnlockBits(dData);
            }

            return dst16;
        }

        private static Bitmap ToArgb1555From32(Bitmap src32, byte alphaThreshold = 8)
        {
            if (src32.PixelFormat != PixelFormat.Format32bppArgb)
                throw new ArgumentException("src32 must be 32bpp ARGB.", nameof(src32));

            int w = src32.Width, h = src32.Height;
            var rect = new Rectangle(0, 0, w, h);
            Bitmap dst16 = new Bitmap(w, h, PixelFormat.Format16bppArgb1555);

            var sData = src32.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var dData = dst16.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);

            try
            {
                unsafe
                {
                    byte* sp = (byte*)sData.Scan0;
                    byte* dp = (byte*)dData.Scan0;
                    int sStride = sData.Stride, dStride = dData.Stride;

                    for (int y = 0; y < h; y++)
                    {
                        byte* sRow = sp + y * sStride;
                        ushort* dRow = (ushort*)(dp + y * dStride);
                        for (int x = 0; x < w; x++)
                        {
                            byte b = sRow[0], g = sRow[1], r = sRow[2], a = sRow[3];
                            if (a <= alphaThreshold)
                            {
                                dRow[x] = 0; // A=0 transparent
                            }
                            else
                            {
                                ushort A = 0x8000;
                                ushort R = (ushort)((r >> 3) << 10);
                                ushort G = (ushort)((g >> 3) << 5);
                                ushort B = (ushort)(b >> 3);
                                dRow[x] = (ushort)(A | R | G | B);
                            }
                            sRow += 4;
                        }
                    }
                }
            }
            finally
            {
                src32.UnlockBits(sData);
                dst16.UnlockBits(dData);
            }
            return dst16;
        }



        // Helper types for JSON
        // Replace the entire EnsureAnimIdx(..) with this:
        private static AnimIdx RequireAnimIdx(int fileType, int body, int action, int dir)
        {
            var anim = AnimationEdit.GetAnimation(fileType, body, action, dir);
            if (anim == null)
                throw new InvalidOperationException(
                    $"Target animation missing (fileType={fileType}, body={body}, action={action}, dir={dir}). " +
                    "Create the action/direction in Animation Edit first, then re-run the import."
                );
            return anim;
        }

        private static Bitmap UnPremultiply(Bitmap src32)

        {
            // expects PixelFormat.Format32bppArgb
            var rect = new Rectangle(0, 0, src32.Width, src32.Height);
            var data = src32.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                Bitmap dst = new Bitmap(src32.Width, src32.Height, PixelFormat.Format32bppArgb);
                var ddata = dst.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                try
                {
                    unsafe
                    {
                        byte* sp = (byte*)data.Scan0;
                        byte* dp = (byte*)ddata.Scan0;
                        int sw = data.Stride, dw = ddata.Stride;
                        for (int y = 0; y < src32.Height; y++)
                        {
                            byte* srow = sp + y * sw;
                            byte* drow = dp + y * dw;
                            for (int x = 0; x < src32.Width; x++)
                            {
                                byte b = srow[0], g = srow[1], r = srow[2], a = srow[3];
                                if (a == 0)
                                {
                                    drow[0] = drow[1] = drow[2] = 0; drow[3] = 0;
                                }
                                else if (a == 255)
                                {
                                    drow[0] = b; drow[1] = g; drow[2] = r; drow[3] = 255;
                                }
                                else
                                {
                                    // convert from premultiplied to straight alpha
                                    drow[0] = (byte)Math.Min(255, (b * 255 + (a >> 1)) / a);
                                    drow[1] = (byte)Math.Min(255, (g * 255 + (a >> 1)) / a);
                                    drow[2] = (byte)Math.Min(255, (r * 255 + (a >> 1)) / a);
                                    drow[3] = a;
                                }
                                srow += 4; drow += 4;
                            }
                        }
                    }
                }
                finally { dst.UnlockBits(ddata); }
                return dst;
            }
            finally { src32.UnlockBits(data); }
        }

        private string GetDirectionName(int dir)
        {
            switch (dir)
            {
                case 0: return "South";
                case 1: return "South West";
                case 2: return "West";
                case 3: return "North West";
                case 4: return "North";
                case 5: return "North East";
                case 6: return "East";
                case 7: return "South East";
                default: return "Unknown";
            }
        }



        private class AlphaSorter : IComparer
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
}
