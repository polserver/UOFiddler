/***************************************************************************
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
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.MultiEditor.Tools;
using UoFiddler.Plugin.MultiEditor.Editing;
using UoFiddler.Plugin.MultiEditor.Styles;

namespace UoFiddler.Plugin.MultiEditor.UI
{
    internal sealed partial class TileBrowserPanel
    {
        // ---------------------------------------------------------------- house page

        /// <summary>The kinds of house parts, in the order you build with them.</summary>
        private static readonly (HousePartKind Kind, string Title, string IconSlot, string Hint)[] _houseKinds =
        {
            (HousePartKind.Wall, "Walls", "Corner", "Drag a rectangle for a room, or along one row for a single wall. Shift: straight wall. Ctrl: erase."),
            (HousePartKind.Floor, "Floors", "F1", "Drag an area to lay the floor. Flat tiles already there are replaced."),
            (HousePartKind.Door, "Doors", "Piece3", "Click a wall to hang the door. Click the door again to turn it."),
            (HousePartKind.Stair, "Stairs", "North", "Press on the bottom step and drag the way the stairs climb. They rise one floor."),
            (HousePartKind.Roof, "Roofs", "EWCrosspiece", "Click a walled room to roof it, or drag over the walls to size it by hand. Ridge, gables and fill are in the options bar.")
        };

        private const string _foundationHint = "Drag the house area: a low wall around a dirt floor 7 above it.";

        private readonly Dictionary<HousePartKind, HouseStyle> _lastStyleOfKind = new Dictionary<HousePartKind, HouseStyle>();
        private readonly Dictionary<HousePartKind, ToolStripButton> _houseKindButtons = new Dictionary<HousePartKind, ToolStripButton>();
        private HouseStyles _houseStyles;
        private HouseStyle _houseStyle;
        private HousePartKind _houseKind = HousePartKind.Wall;
        private bool _foundationPicked;
        private const string _swapText = "Swap selection to this style";
        private bool _patternsShown;
        private FloorPattern _pattern;
        private TileBrowserGrid _housePieces;
        private Label _housePiecesLabel;
        private int _houseStyleCount;
        private bool _houseLoaded;

        private static bool IsFoundationStyle(HouseStyle style) =>
            style.Kind == HousePartKind.Wall && style.Get("Post") == FoundationTool.FoundationPost;

        /// <summary>
        /// The House page: a row of part kinds, the styles of the chosen kind as pictures, and
        /// under them the single pieces of the chosen style. The designer holds the page's fixed
        /// parts; the kind buttons (one per <see cref="_houseKinds"/> entry) and the pieces grid
        /// are added here.
        /// </summary>
        private void BuildHousePage()
        {
            // Kind buttons go between Foundation and Patterns.
            foreach (var (kind, title, _, hint) in _houseKinds)
            {
                var button = new ToolStripButton(title) { ToolTipText = hint };
                button.Click += (_, _) => ShowHouseKind(kind, activate: true);
                _houseKindButtons[kind] = button;
                _houseKindBar.Items.Insert(_houseKindBar.Items.IndexOf(_patternsButton), button);
            }

            _familyButton = new ToolStripDropDownButton("Family")
            {
                Alignment = ToolStripItemAlignment.Right,
                ToolTipText = "Shows only the styles of one material. A kind without styles of it shows all of its own."
            };
            _familyButton.DropDownOpening += (_, _) => FillFamilies();
            _houseKindBar.Items.Add(_familyButton);

            var edit = new ToolStripButton("Edit...")
            {
                Alignment = ToolStripItemAlignment.Right,
                ToolTipText = "Names, groups, order and pieces of the house styles."
            };
            edit.Click += (_, _) => EditStylesRequested?.Invoke(_houseStyle);
            _houseKindBar.Items.Add(edit);

            // The pieces of the chosen style: click one to draw with exactly that tile.
            // Roughly half the page each by default; the splitter above the pieces moves the line.
            _housePieces = new TileBrowserGrid(_thumbnails)
            {
                Dock = DockStyle.Bottom,
                Height = Math.Clamp(EditorSettings.Current.HousePiecesHeight, 80, 800),
                EmptyText = "Pick a style to see its pieces."
            };
            _housePieces.CellSize = 64;
            _housePieces.TileActivated += (_, id) => ChooseTile(id);
            _housePieces.TileContextMenuRequested += ShowTileMenu;
            _housePiecesLabel = new Label { Dock = DockStyle.Bottom, Height = 18, Text = "Pieces", ForeColor = SystemColors.GrayText };

            // Docking runs from the last added control: the pieces sit right after the splitter,
            // so they dock below it and above the swap button.
            _housePieces.TabIndex = 2;
            _housePage.Controls.Add(_housePieces);
            _housePage.Controls.SetChildIndex(_housePieces, _housePage.Controls.GetChildIndex(_piecesSplitter) + 1);
        }

        private void FoundationButton_Click(object sender, EventArgs e)
        {
            PickFoundation();
        }

        private void PatternsButton_Click(object sender, EventArgs e)
        {
            ShowPatterns(null);
        }

        private void HouseGallery_TagChosen(object tag)
        {
            if (tag is FloorPattern pattern)
            {
                PickPattern(pattern);
            }
        }

        private void SwapButton_Click(object sender, EventArgs e)
        {
            if (_patternsShown)
            {
                DeletePattern();
            }
            else if (_houseStyle != null)
            {
                SwapToHouseStyleRequested?.Invoke(_houseStyle);
            }
        }

        private void PiecesSplitter_SplitterMoved(object sender, SplitterEventArgs e)
        {
            EditorSettings settings = EditorSettings.Current.Clone();
            settings.HousePiecesHeight = _housePieces.Height;
            settings.Apply();
        }

        /// <summary>
        /// Opens the House page on a kind of part (or the foundation) and picks its style, as if the
        /// kind button had been pressed. Called when a house tool is chosen from the tool palette.
        /// </summary>
        public void ShowHousePage(HousePartKind kind, bool foundation)
        {
            EnsureLoaded();
            if (_tabs.SelectedTab != _housePage)
            {
                _tabs.SelectedTab = _housePage;
            }

            EnsureHouse();
            if (foundation)
            {
                if (_foundationButton.Enabled)
                {
                    PickFoundation();
                }
            }
            else if (_houseKindButtons[kind].Enabled)
            {
                ShowHouseKind(kind, activate: true);
            }
        }

        private void EnsureHouse()
        {
            if (!_houseLoaded)
            {
                _houseLoaded = true;
                RebuildHouseTree();
            }
        }

        private void ReloadHouseIcons()
        {
            if (_houseLoaded)
            {
                RebuildHouseTree();
            }
        }

        private bool HouseStyleMatches(HouseStyle style)
        {
            TileBrowserQuery query = _query;
            return query == null ||
                   (query.IsIdQuery ? style.Slots.Values.Any(id => query.MatchesId(id)) : query.MatchesText(style.Group + " " + style.Name + " " + style.Family));
        }

        // The family the House page is narrowed to, or null for all, and its button.
        private string _houseFamily;
        private ToolStripDropDownButton _familyButton;

        /// <summary>True for a style of the chosen family. A kind that has no style of that family shows all of its styles.</summary>
        private bool InHouseFamily(HouseStyle style, ICollection<HousePartKind> kindsWithFamily)
        {
            return _houseFamily == null || style.Family == _houseFamily || !kindsWithFamily.Contains(style.Kind);
        }

        /// <summary>Lists the families of the kind shown, to narrow the page to one.</summary>
        private void FillFamilies()
        {
            _familyButton.DropDownItems.Clear();
            void Add(string text, string family)
            {
                var item = new ToolStripMenuItem(text) { Checked = _houseFamily == family };
                item.Click += (_, _) =>
                {
                    _houseFamily = family;
                    _familyButton.Text = family == null ? "Family" : "Family: " + family;
                    RebuildHouseTree();
                };
                _familyButton.DropDownItems.Add(item);
            }

            Add("All families", null);
            if (_houseStyles == null)
            {
                return;
            }

            foreach (string family in _houseStyles.Styles.Where(st => st.Kind == _houseKind && !st.Hidden && st.Family.Length > 0).Select(st => st.Family).Distinct())
            {
                Add(family, family);
            }
        }

        /// <summary>
        /// Shows the styles of the current kind that match the search (by style or material name,
        /// or by a piece's id), and how many each kind has.
        /// </summary>
        private void RebuildHouseTree()
        {
            if (_patternsShown)
            {
                RebuildPatterns();
                return;
            }

            _patternsButton.Checked = false;
            _swapButton.Text = _swapText;
            _swapButton.Enabled = true;
            List<HouseStyle> styles = _houseStyles?.Styles.Where(st => !st.Hidden).ToList() ?? new List<HouseStyle>();
            if (_houseFamily != null)
            {
                var kindsWithFamily = new HashSet<HousePartKind>(styles.Where(st => st.Family == _houseFamily).Select(st => st.Kind));
                styles = styles.Where(st => InHouseFamily(st, kindsWithFamily)).ToList();
            }

            _houseStyleCount = 0;

            foreach (var (kind, title, _, _) in _houseKinds)
            {
                int count = styles.Count(st => st.Kind == kind && HouseStyleMatches(st));
                _houseStyleCount += count;
                ToolStripButton button = _houseKindButtons[kind];
                button.Text = $"{title} ({count})";
                button.Enabled = count > 0;
                button.Checked = !_foundationPicked && kind == _houseKind;
            }

            _foundationButton.Enabled = styles.Any(IsFoundationStyle);
            _foundationButton.Checked = _foundationPicked;

            // A search that empties the current kind moves on to the first kind with matches.
            if (!_houseKindButtons[_houseKind].Enabled)
            {
                foreach (var (kind, _, _, _) in _houseKinds)
                {
                    if (_houseKindButtons[kind].Enabled)
                    {
                        _houseKind = kind;
                        _houseKindButtons[kind].Checked = !_foundationPicked;
                        break;
                    }
                }
            }

            string iconSlot = _houseKinds.First(k => k.Kind == _houseKind).IconSlot;
            var sections = new List<HouseGallerySection>();
            foreach (IGrouping<string, HouseStyle> group in styles.Where(st => st.Kind == _houseKind && HouseStyleMatches(st)).GroupBy(st => st.Group))
            {
                var items = new List<HouseGalleryItem>();
                foreach (HouseStyle style in group)
                {
                    int icon = style.Get(iconSlot) is var preferred and not 0 ? preferred : style.IconId;
                    items.Add(new HouseGalleryItem(style, ShortStyleName(style), icon));
                }

                sections.Add(new HouseGallerySection(group.Key, items));
            }

            // Headings help when a material has several styles (walls). Where most groups hold a
            // single style (floors, doors), they only repeat its name and waste a row each.
            if (sections.Count == 1 || sections.Count(section => section.Items.Count == 1) * 2 > sections.Count)
            {
                var all = new List<HouseGalleryItem>();
                foreach (HouseGallerySection section in sections)
                {
                    all.AddRange(section.Items);
                }

                sections.Clear();
                sections.Add(new HouseGallerySection(null, all));
            }

            _houseGallery.EmptyText = _houseStyles == null || styles.Count == 0
                ? "No house design tables in this client."
                : _query != null ? "No house styles match." : "No styles of this kind.";
            _houseGallery.SetSections(sections);
            _houseGallery.Selected = _foundationPicked ? null : _houseStyle;
            _houseGallery.ScrollToSelected();

            ShowHousePieces();
            _houseVersion = _filterVersion;
            UpdateStatus();
        }

        /// <summary>The style name without the words its section heading or kind button already say.</summary>
        private static string ShortStyleName(HouseStyle style)
        {
            string name = style.Name;
            foreach (string word in new[] { " Walls", " Wall", " Floors", " Floor", " Doors", " Door", " Stairs", " Roofs", " Roof" })
            {
                if (name.EndsWith(word, StringComparison.OrdinalIgnoreCase) && name.Length > word.Length)
                {
                    name = name[..^word.Length];
                    break;
                }
            }

            return name;
        }

        private void ShowHousePieces()
        {
            HouseStyle style = _houseStyle;
            var pieces = new List<int>();
            if (style != null)
            {
                foreach (KeyValuePair<string, ushort> slot in style.Slots)
                {
                    // Stair styles also list stair multis; those are multi ids, not art.
                    if (!slot.Key.StartsWith("Multi", StringComparison.Ordinal) && !pieces.Contains(slot.Value))
                    {
                        pieces.Add(slot.Value);
                    }
                }
            }

            _housePieces.SetItems(pieces);
            _housePiecesLabel.Text = style != null ? $"Pieces of {style.Name} (click one to draw with it)" : "Pieces";
            _houseHint.Text = _foundationPicked ? _foundationHint : _houseKinds.First(k => k.Kind == _houseKind).Hint;
        }

        /// <summary>
        /// Switches the gallery to a kind. With <paramref name="activate"/> the style last used for
        /// that kind (or its first one) is picked straight away, so the tool is ready to drag.
        /// </summary>
        private void ShowHouseKind(HousePartKind kind, bool activate)
        {
            _houseKind = kind;
            _foundationPicked = false;
            _patternsShown = false;
            if (activate)
            {
                _lastStyleOfKind.TryGetValue(kind, out HouseStyle last);
                _houseStyle = last;
            }

            RebuildHouseTree();
            if (!activate)
            {
                return;
            }

            HouseStyle pick = _houseStyle != null && HouseStyleMatches(_houseStyle) ? _houseStyle : _houseGallery.First();
            if (pick != null)
            {
                PickHouseStyle(pick);
            }
        }

        private void PickHouseStyle(HouseStyle style)
        {
            _foundationPicked = false;
            _patternsShown = false;
            _houseStyle = style;
            _houseKind = style.Kind;
            _lastStyleOfKind[style.Kind] = style;
            foreach (KeyValuePair<HousePartKind, ToolStripButton> button in _houseKindButtons)
            {
                button.Value.Checked = button.Key == style.Kind;
            }

            _foundationButton.Checked = false;
            _houseGallery.Selected = style;
            ShowHousePieces();
            UpdateStatus();
            HouseStyleChosen?.Invoke(style);
        }

        private void PickFoundation()
        {
            _foundationPicked = true;
            _patternsShown = false;
            _patternsButton.Checked = false;
            _swapButton.Text = _swapText;
            _houseStyle = FoundationStyle;
            foreach (ToolStripButton button in _houseKindButtons.Values)
            {
                button.Checked = false;
            }

            _foundationButton.Checked = true;
            _houseGallery.Selected = null;
            ShowHousePieces();
            UpdateStatus();
            FoundationChosen?.Invoke();
        }

        // ---------------------------------------------------------------- floor patterns

        /// <summary>The user picked a floor pattern; the shell switches to the floor pattern tool.</summary>
        public event Action<FloorPattern> FloorPatternChosen;

        /// <summary>
        /// Shows the floor patterns on the House page, re-read from disk, and picks one (the given
        /// one, the last used, or the first).
        /// </summary>
        public void ShowPatterns(FloorPattern select)
        {
            EnsureLoaded();
            if (_tabs.SelectedTab != _housePage)
            {
                _tabs.SelectedTab = _housePage;
            }

            EnsureHouse();
            _patternsShown = true;
            _foundationPicked = false;
            FloorPatterns.Reset();
            List<FloorPattern> all = FloorPatterns.All();
            FloorPattern pick = select != null ? all.Find(p => p.Name == select.Name)
                : _pattern != null ? all.Find(p => p.Name == _pattern.Name)
                : null;
            pick ??= all.Count > 0 ? all[0] : null;
            _pattern = pick;
            RebuildPatterns();
            if (pick != null)
            {
                FloorPatternChosen?.Invoke(pick);
            }
        }

        private void RebuildPatterns()
        {
            foreach (ToolStripButton button in _houseKindButtons.Values)
            {
                button.Checked = false;
            }

            _foundationButton.Checked = false;
            _patternsButton.Checked = true;

            TileBrowserQuery query = _query;
            var mine = new List<HouseGalleryItem>();
            var shipped = new List<HouseGalleryItem>();
            foreach (FloorPattern pattern in FloorPatterns.All())
            {
                if (query != null && !(query.IsIdQuery ? pattern.Pieces().Any(id => query.MatchesId(id)) : query.MatchesText(pattern.Name)))
                {
                    continue;
                }

                (pattern.BuiltIn ? shipped : mine).Add(new HouseGalleryItem(null, pattern.Name, pattern.IconId, pattern));
            }

            _houseGallery.EmptyText = "No floor patterns yet. Lay out one bordered floor by hand, select it, " +
                                      "and use File > Save selection as floor pattern.";
            _houseGallery.SetSections(new List<HouseGallerySection>
            {
                new HouseGallerySection("Your patterns", mine),
                new HouseGallerySection("Built in", shipped)
            });
            _houseGallery.Selected = null;
            _houseGallery.SelectedTag = _pattern;
            _houseGallery.ScrollToSelected();

            _housePieces.SetItems(_pattern != null ? _pattern.Pieces().Select(id => (int)id).ToList() : new List<int>());
            _housePiecesLabel.Text = _pattern != null ? $"Pieces of {_pattern.Name}" : "Pieces";
            _houseHint.Text = "Drag an area: corners, sides and fill are laid as in the pattern, repeated to fit.";
            _swapButton.Text = "Delete this pattern";
            _swapButton.Enabled = _pattern != null && !_pattern.BuiltIn;
            _houseVersion = _filterVersion;
            UpdateStatus();
        }

        private void PickPattern(FloorPattern pattern)
        {
            _pattern = pattern;
            RebuildPatterns();
            FloorPatternChosen?.Invoke(pattern);
        }

        private void DeletePattern()
        {
            if (_pattern == null || _pattern.BuiltIn ||
                MessageBox.Show(this, $"Delete the floor pattern \"{_pattern.Name}\"?", _messageTitle, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
            {
                return;
            }

            FloorPatterns.Delete(_pattern);
            _pattern = null;
            ShowPatterns(null);
        }

        private void ActivateFirstHouseNode()
        {
            if (_houseGallery.First() is { } style)
            {
                PickHouseStyle(style);
            }
        }
    }
}
