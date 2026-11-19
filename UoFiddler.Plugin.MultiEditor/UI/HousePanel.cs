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
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.MultiEditor.Classes;
using UoFiddler.Plugin.MultiEditor.Editing;
using UoFiddler.Plugin.MultiEditor.Tools;

namespace UoFiddler.Plugin.MultiEditor.UI
{
    /// <summary>
    /// The house draft of the session: its wings, the storeys and roof of the selected wing, the
    /// wall and floor style of each storey, the roof style, foundation and base Z, and the
    /// draft's undo, redo, place and drop. The row of the storey that the chosen floor means is
    /// marked, as that is the storey the wall, door, floor and stair tools work on.
    /// </summary>
    internal sealed partial class HousePanel : UserControl
    {
        private readonly Button[] _wallButtons;
        private readonly Button[] _floorButtons;
        private readonly Label[] _storeyLabels;
        private readonly Font _boldFont;
        private readonly TileBrowserThumbnails _thumbnails = new TileBrowserThumbnails();
        private EditorSession _session;
        private bool _updating;

        public HousePanel()
        {
            InitializeComponent();
            _wallButtons = new[] { _wall1, _wall2, _wall3, _wall4 };
            _floorButtons = new[] { _floor1, _floor2, _floor3, _floor4 };
            _storeyLabels = new[] { _storey1, _storey2, _storey3, _storey4 };
            _boldFont = new Font(Font, FontStyle.Bold);
            Disposed += (_, _) =>
            {
                _boldFont.Dispose();
                _thumbnails.Dispose();
            };

            for (int k = 0; k < HouseDraft.MaxStoreys; ++k)
            {
                int storey = k;
                _wallButtons[k].Click += (_, _) => PickStyle?.Invoke(HousePartKind.Wall, storey);
                _floorButtons[k].Click += (_, _) => PickStyle?.Invoke(HousePartKind.Floor, storey);
            }

            _foundationKind.DropDownWidth = 240;
            _doorStyle.Click += (_, _) => PickStyle?.Invoke(HousePartKind.Door, 0);
            _stairStyle.Click += (_, _) => PickStyle?.Invoke(HousePartKind.Stair, 0);
            _foundationStyle.Click += (_, _) => PickStyle?.Invoke(HousePartKind.Wall, FoundationPick);
            _roofStyle.Click += (_, _) =>
            {
                bool flat = _session != null && !_session.House.NoRoof && _session.House.Shape == RoofShape.Flat;
                PickStyle?.Invoke(flat ? HousePartKind.Floor : HousePartKind.Roof, flat ? FlatRoof : 0);
            };
        }

        /// <summary>The storey a pick names when it is for the flat roof's floor style.</summary>
        public const int FlatRoof = -1;

        /// <summary>The storey a pick names when it is for the foundation's wall style.</summary>
        public const int FoundationPick = -2;

        private const string _customTheme = "(your own mix)";

        /// <summary>Asks for the draft's styles to be saved as a theme, or for a theme of the user's to be deleted.</summary>
        public event Action SaveTheme;

        public event Action<HouseTheme> DeleteTheme;

        /// <summary>Raised when a draft was started from the panel: the House tool is the one to shape it with.</summary>
        public event Action HouseToolWanted;

        /// <summary>Something to tell the user in the status bar.</summary>
        public event Action<string> Message;

        /// <summary>Asks for a style to be picked on the House page for a storey's walls or floor, or for the roof.</summary>
        public event Action<HousePartKind, int> PickStyle;

        /// <summary>Asks for the draft to be saved to, or opened from, a house draft file.</summary>
        public event Action SaveDraft;

        public event Action OpenDraft;

        /// <summary>Asks for the house as built to be saved to the blueprints.</summary>
        public event Action SaveBlueprint;

        /// <summary>Asks for a placed house to be opened as a draft again.</summary>
        public event Action<System.Xml.Linq.XElement> EditHouse;

        /// <summary>Raised after a button of the panel was used, so the canvas can take the keyboard back.</summary>
        public event Action Used;

        public void Bind(EditorSession session)
        {
            if (_session != null)
            {
                _session.DraftChanged -= RefreshDraft;
                _session.ViewChanged -= RefreshDraft;
            }

            _session = session;
            if (_session != null)
            {
                _session.DraftChanged += RefreshDraft;
                _session.ViewChanged += RefreshDraft;
            }

            RefreshDraft();
        }

        /// <summary>Fills the panel from the session's house draft.</summary>
        public void RefreshDraft()
        {
            HouseTool house = _session?.House;
            Enabled = house != null;
            if (house == null)
            {
                return;
            }

            _updating = true;
            try
            {
                // Rows are changed in place: a list emptied and filled again on every step of a drag flickers.
                _wingList.BeginUpdate();
                while (_wingList.Items.Count > house.Wings.Count)
                {
                    _wingList.Items.RemoveAt(_wingList.Items.Count - 1);
                }

                for (int i = 0; i < house.Wings.Count; ++i)
                {
                    RoofWing wing = house.Wings[i];
                    string roof = wing.NoRoof ? "no roof" : wing.Shape.ToString().ToLowerInvariant();
                    string divided = wing.Rooms > 1 ? $"{wing.Rooms} rooms{(wing.Layout == RoomLayout.Row ? string.Empty : " (" + wing.Layout.ToString().ToLowerInvariant() + ")")}, " : string.Empty;
                    string text = $"Wing {i + 1}: {wing.X1 - wing.X0 + 1} x {wing.Y1 - wing.Y0 + 1}, {wing.Storeys} storey{(wing.Storeys == 1 ? string.Empty : "s")}, {divided}{roof}";
                    if (i >= _wingList.Items.Count)
                    {
                        _wingList.Items.Add(text);
                    }
                    else if (!text.Equals(_wingList.Items[i]))
                    {
                        _wingList.Items[i] = text;
                    }
                }

                if (_wingList.SelectedIndex != house.SelectedIndex)
                {
                    _wingList.SelectedIndex = house.SelectedIndex;
                }

                _wingList.EndUpdate();
                _gables.SelectedIndex = (int)house.Draft.Gables;
                _gableFill.SelectedIndex = (int)house.Draft.GableFill;

                _storeys.Value = Math.Clamp(house.Storeys, 1, HouseDraft.MaxStoreys);
                _walled.Checked = house.Walled;
                _preset.SelectedIndex = 0;
                // The drop-down lists the roof shapes in their order, then None.
                _roofShape.SelectedIndex = house.NoRoof ? _roofShape.Items.Count - 1 : (int)house.Shape;
                _ridge.SelectedIndex = (int)house.Ridge;
                _ridge.Enabled = !house.NoRoof && house.Shape == RoofShape.Gable;
                _removeWing.Enabled = house.HasSelectedWing;
                // The copy follows the mouse of the House tool: under the draft's other tools nothing would drop it.
                _duplicateWing.Enabled = house.HasSelectedWing && !house.FloatingWing && ReferenceEquals(_session.WingTool, house);
                _wingHint.Text = house.HasSelectedWing ? "Selected wing:" : "New wings:";

                int top = house.TopStorey;
                int active = house.HasPending ? _session.DraftStorey : -1;
                for (int k = 0; k < HouseDraft.MaxStoreys; ++k)
                {
                    bool used = k < top || k < house.Storeys;
                    _storeyLabels[k].Visible = _wallButtons[k].Visible = _floorButtons[k].Visible = used;
                    Show(_wallButtons[k], house.Draft.WallStyles[k], "(walls)", "South2");
                    Show(_floorButtons[k], house.Draft.FloorStyles[k], "(floor)", null);

                    // The storey the chosen floor means: where the other tools work.
                    bool current = k == active;
                    _storeyLabels[k].Font = current ? _boldFont : Font;
                    _storeyLabels[k].BackColor = current ? Color.FromArgb(255, 214, 64) : Color.Transparent;
                    _storeyLabels[k].ForeColor = current ? Color.Black : ForeColor;
                }

                // A flat roof is laid in a floor style of its own; the button picks that one then.
                bool flat = !house.NoRoof && house.Shape == RoofShape.Flat;
                Show(_roofStyle, flat ? house.Draft.FlatRoofStyle : house.Draft.RoofStyle, flat ? "(top floor's style)" : "(roof)", null);
                RefreshFoundations(house);
                Show(_doorStyle, house.Draft.DoorStyle, "(doors)", HouseDraft.DoorAlongX);
                Show(_stairStyle, house.Draft.StairStyle, "(stairs)", null);
                Show(_foundationStyle, house.Draft.FoundationStyle, "(light brick)", "South2");
                _foundationStyle.Enabled = house.Draft.Foundation;
                RefreshThemes(house);
                // Rooms are those of the storey the chosen floor means.
                int roomStorey = RoomStorey;
                (int rooms, RoomLayout layout) = house.RoomsOn(roomStorey);
                _rooms.Value = Math.Clamp(rooms, 1, HouseDraft.MaxRooms);
                _layout.SelectedIndex = (int)layout;
                _roomsPerStorey.Enabled = house.HasSelectedWing && roomStorey >= 1;
                _roomsPerStorey.Checked = house.HasOwnRooms(roomStorey);
                _roomsPerStorey.Text = roomStorey >= 1 ? $"Storey {roomStorey + 1} only" : "This storey";
                _autoDoors.Checked = house.Draft.AutoDoors;
                _parapet.Checked = house.Draft.Parapet;
                _frontDoor.SelectedIndex = (int)house.Draft.FrontDoor;
                _windowSpacing.Value = Math.Clamp(house.Draft.WindowSpacing, 0, (int)_windowSpacing.Maximum);
                _showWalls.Checked = house.ShowWalls;
                _showFloors.Checked = house.ShowFloors;
                _showRoof.Checked = house.ShowRoof;
                _saveDraft.Enabled = _blueprint.Enabled = house.HasPending;

                // The houses already placed in this design, which can be opened as drafts again.
                List<System.Xml.Linq.XElement> placedNow = _session.PlacedHouses.ToList();
                if (!_placed.Items.Cast<PlacedHouse>().Select(p => p.House).SequenceEqual(placedNow))
                {
                    object chosen = _placed.SelectedItem;
                    _placed.Items.Clear();
                    foreach (System.Xml.Linq.XElement placed in placedNow)
                    {
                        _placed.Items.Add(new PlacedHouse(placed));
                    }

                    _placed.SelectedIndex = _placed.Items.Count == 0 ? -1
                        : Math.Max(0, _placed.Items.Cast<PlacedHouse>().ToList().FindIndex(p => chosen is PlacedHouse c && c.House == p.House));
                }

                _placed.Enabled = _placed.Items.Count > 0;
                _edit.Enabled = _placed.Items.Count > 0 && !house.HasPending;
                _baseZ.Enabled = house.HasPending;
                _baseZ.Value = Math.Clamp(house.BaseZ, (int)_baseZ.Minimum, (int)_baseZ.Maximum);

                _undo.Enabled = house.CanUndoDraft;
                _redo.Enabled = house.CanRedoDraft;
                _place.Enabled = _drop.Enabled = house.HasPending;

                if (!house.HasPending)
                {
                    _status.Text = "Drag on the canvas to draw the first wing.";
                }
                else
                {
                    int x0 = house.Wings.Min(w => w.X0), x1 = house.Wings.Max(w => w.X1);
                    int y0 = house.Wings.Min(w => w.Y0), y1 = house.Wings.Max(w => w.Y1);
                    _status.Text = $"{house.Wings.Count} wing{(house.Wings.Count == 1 ? string.Empty : "s")}, {x1 - x0 + 1} x {y1 - y0 + 1} cells, " +
                                   $"{top} storey{(top == 1 ? string.Empty : "s")}. Not in the multi until placed.";
                }
            }
            finally
            {
                _updating = false;
            }
        }

        /// <summary>Shows a style on its button: a piece of it, its name, and the whole name as the tip.</summary>
        private void Show(Button button, HouseStyle style, string none, string iconSlot)
        {
            string text = style?.Name ?? none;
            if (button.Text != text)
            {
                button.Text = text;
                _toolTip.SetToolTip(button, style == null ? null : $"{style.Name} ({style.Group}). Click to pick another on the House page.");
            }

            ushort icon = style == null ? (ushort)0 : iconSlot != null && style.Get(iconSlot) is var piece and not 0 ? piece : style.IconId;
            Image image = icon == 0 ? null : _thumbnails.Get(icon, 20);
            if (!ReferenceEquals(button.Image, image))
            {
                button.Image = image;
                button.ImageAlign = ContentAlignment.MiddleLeft;
                button.TextImageRelation = TextImageRelation.ImageBeforeText;
            }
        }

        private void Gables_SelectedIndexChanged(object sender, EventArgs e)
        {
            Run(house => house.SetGables((RoofGables)Math.Max(0, _gables.SelectedIndex), (RoofGableFill)Math.Max(0, _gableFill.SelectedIndex)));
        }

        /// <summary>Lists the themes this client can build and marks the one the draft's styles are.</summary>
        private void RefreshThemes(HouseTool house)
        {
            IReadOnlyList<HouseTheme> themes = house.Themes;
            bool same = _theme.Items.Count == themes.Count + 1;
            for (int i = 0; same && i < themes.Count; ++i)
            {
                same = ReferenceEquals(_theme.Items[i + 1], themes[i]);
            }

            if (!same)
            {
                _theme.BeginUpdate();
                _theme.Items.Clear();
                _theme.Items.Add(_customTheme);
                foreach (HouseTheme theme in themes)
                {
                    _theme.Items.Add(theme);
                }

                _theme.EndUpdate();
            }

            HouseTheme current = house.CurrentTheme();
            _theme.SelectedIndex = current == null ? 0 : _theme.Items.IndexOf(current);
            _theme.Enabled = themes.Count > 0;
            _deleteTheme.Enabled = current is { IsUser: true };
        }

        private void Theme_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (_updating || _session == null || _theme.SelectedItem is not HouseTheme theme)
            {
                return;
            }

            List<string> missing = _session.House.ApplyTheme(theme);
            Message?.Invoke(missing.Count == 0
                ? $"Theme \"{theme.Name}\" applied to the house."
                : $"Theme \"{theme.Name}\" applied; this client has no {string.Join(", ", missing)} for it.");
            Used?.Invoke();
        }

        private void SaveTheme_Click(object sender, EventArgs e)
        {
            SaveTheme?.Invoke();
            Used?.Invoke();
        }

        private void DeleteTheme_Click(object sender, EventArgs e)
        {
            if (_theme.SelectedItem is HouseTheme { IsUser: true } theme)
            {
                DeleteTheme?.Invoke(theme);
                Used?.Invoke();
            }
        }

        /// <summary>Marks the button whose style is being picked on the House page; null clears the mark.</summary>
        public void MarkPick(HousePartKind? kind, int storey)
        {
            Button picked = kind switch
            {
                null => null,
                HousePartKind.Roof => _roofStyle,
                HousePartKind.Door => _doorStyle,
                HousePartKind.Stair => _stairStyle,
                HousePartKind.Wall when storey == FoundationPick => _foundationStyle,
                HousePartKind.Floor when storey == FlatRoof => _roofStyle,
                HousePartKind.Wall => storey >= 0 && storey < _wallButtons.Length ? _wallButtons[storey] : null,
                HousePartKind.Floor => storey >= 0 && storey < _floorButtons.Length ? _floorButtons[storey] : null,
                _ => null
            };
            foreach (Button button in _wallButtons.Concat(_floorButtons).Concat(new[] { _roofStyle, _doorStyle, _stairStyle, _foundationStyle }))
            {
                if (button == picked)
                {
                    button.BackColor = Color.FromArgb(255, 214, 64);
                    button.ForeColor = Color.Black;
                }
                else
                {
                    button.ForeColor = ForeColor;
                    button.UseVisualStyleBackColor = true;
                }
            }
        }

        /// <summary>A placed house in the list, shown by its name and size.</summary>
        private sealed class PlacedHouse
        {
            public PlacedHouse(System.Xml.Linq.XElement house)
            {
                House = house;
            }

            public System.Xml.Linq.XElement House { get; }

            public override string ToString()
            {
                int wings = House.Elements("Wing").Count();
                return $"{(string)House.Attribute("name") ?? "House"} ({wings} wing{(wings == 1 ? string.Empty : "s")})";
            }
        }

        /// <summary>The storey the rooms shown are of: the one the chosen floor means, the ground floor with all floors shown.</summary>
        private int RoomStorey => _session != null && _session.House.HasPending && _session.WorkZ != null ? _session.DraftStorey : 0;

        private void Rooms_ValueChanged(object sender, EventArgs e)
        {
            if (!_updating && _session != null)
            {
                _session.House.SetRooms((int)_rooms.Value, (RoomLayout)Math.Max(0, _layout.SelectedIndex), RoomStorey);
            }
        }

        private void Layout_SelectedIndexChanged(object sender, EventArgs e)
        {
            Run(house => house.SetRooms((int)_rooms.Value, (RoomLayout)Math.Max(0, _layout.SelectedIndex), RoomStorey));
        }

        private void RoomsPerStorey_CheckedChanged(object sender, EventArgs e)
        {
            Run(house => house.SetOwnRooms(RoomStorey, _roomsPerStorey.Checked));
        }

        private void Walled_CheckedChanged(object sender, EventArgs e)
        {
            Run(house => house.SetWalled(_walled.Checked));
        }

        private void Preset_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (_updating || _session == null || _preset.SelectedIndex <= 0)
            {
                return;
            }

            string preset = (string)_preset.SelectedItem;
            HouseTool house = _session.House;
            if (house.HasPending && MessageBox.Show(this, $"Replace the draft with a {preset.ToLowerInvariant()}? Undo brings it back.", "House draft",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
            {
                _preset.SelectedIndex = 0;
                return;
            }

            // In the stone theme when the client has it: these are stone buildings.
            string problem = house.LoadPreset(preset);
            if (problem == null && house.Themes.FirstOrDefault(theme => theme.Name == "Weathered Stone Keep") is { } stone)
            {
                house.ApplyTheme(stone);
            }

            if (problem == null)
            {
                HouseToolWanted?.Invoke();
            }

            Message?.Invoke(problem ?? $"{preset} drafted. Drag a wing to move it, its edge to resize it, Ctrl+drag to move all of it. Place puts it in the multi.");
            _preset.SelectedIndex = 0;
            Used?.Invoke();
        }

        private void Parapet_CheckedChanged(object sender, EventArgs e)
        {
            Run(house => house.SetParapet(_parapet.Checked));
        }

        private void AutoDoors_CheckedChanged(object sender, EventArgs e)
        {
            Run(house => house.SetAutoDoors(_autoDoors.Checked));
        }

        private void FrontDoor_SelectedIndexChanged(object sender, EventArgs e)
        {
            Run(house => house.SetFrontDoor((FrontDoorSide)Math.Max(0, _frontDoor.SelectedIndex)));
        }

        private void WindowSpacing_ValueChanged(object sender, EventArgs e)
        {
            if (!_updating && _session != null)
            {
                _session.House.SetWindowSpacing((int)_windowSpacing.Value);
            }
        }

        private void Show_CheckedChanged(object sender, EventArgs e)
        {
            Run(house => (house.ShowWalls, house.ShowFloors, house.ShowRoof) = (_showWalls.Checked, _showFloors.Checked, _showRoof.Checked));
        }

        private void SaveDraft_Click(object sender, EventArgs e)
        {
            SaveDraft?.Invoke();
            Used?.Invoke();
        }

        private void OpenDraft_Click(object sender, EventArgs e)
        {
            OpenDraft?.Invoke();
            Used?.Invoke();
        }

        private void Blueprint_Click(object sender, EventArgs e)
        {
            SaveBlueprint?.Invoke();
            Used?.Invoke();
        }

        private void Edit_Click(object sender, EventArgs e)
        {
            if (_placed.SelectedItem is PlacedHouse placed)
            {
                EditHouse?.Invoke(placed.House);
                Used?.Invoke();
            }
        }

        private void Run(Action<HouseTool> action)
        {
            if (_updating || _session == null)
            {
                return;
            }

            action(_session.House);
            Used?.Invoke();
        }

        private void WingList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_updating && _session != null)
            {
                _session.House.SelectedIndex = _wingList.SelectedIndex;
            }
        }

        private void Storeys_ValueChanged(object sender, EventArgs e)
        {
            if (!_updating && _session != null)
            {
                _session.House.SetStoreys((int)_storeys.Value);
            }
        }

        private void RoofShape_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = _roofShape.SelectedIndex;
            bool none = index == _roofShape.Items.Count - 1;
            Run(house => house.SetRoof(!none && Enum.IsDefined((RoofShape)index) ? (RoofShape)index : RoofShape.Gable, none));
        }

        private void Ridge_SelectedIndexChanged(object sender, EventArgs e)
        {
            Run(house => house.SetRidge((RoofRidge)Math.Max(0, _ridge.SelectedIndex)));
        }

        private void RemoveWing_Click(object sender, EventArgs e)
        {
            Run(house => house.DeleteWing());
        }

        private void DuplicateWing_Click(object sender, EventArgs e)
        {
            Run(house => house.DuplicateWing());
        }

        private const string _noFoundation = "None";
        private const string _ownFoundation = "(your own)";

        /// <summary>Lists the kinds of foundation this client can build and marks the one the house has.</summary>
        private void RefreshFoundations(HouseTool house)
        {
            IReadOnlyList<HouseFoundation> kinds = house.Foundations;
            bool same = _foundationKind.Items.Count == kinds.Count + 2;
            for (int i = 0; same && i < kinds.Count; ++i)
            {
                same = ReferenceEquals(_foundationKind.Items[i + 1], kinds[i]);
            }

            if (!same)
            {
                _foundationKind.BeginUpdate();
                _foundationKind.Items.Clear();
                _foundationKind.Items.Add(_noFoundation);
                foreach (HouseFoundation kind in kinds)
                {
                    _foundationKind.Items.Add(kind);
                }

                _foundationKind.Items.Add(_ownFoundation);
                _foundationKind.EndUpdate();
            }

            HouseFoundation current = house.CurrentFoundation();
            _foundationKind.SelectedIndex = !house.Draft.Foundation ? 0 : current != null ? _foundationKind.Items.IndexOf(current) : _foundationKind.Items.Count - 1;
        }

        private void FoundationKind_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (_updating || _session == null)
            {
                return;
            }

            if (_foundationKind.SelectedItem is HouseFoundation kind)
            {
                Run(house => house.SetFoundation(kind));
            }
            else
            {
                // "(your own)" keeps what the house has and turns it on: the style button below changes it.
                Run(house => house.SetFoundation(_foundationKind.SelectedIndex != 0));
            }
        }

        private void BaseZ_ValueChanged(object sender, EventArgs e)
        {
            if (!_updating && _session != null)
            {
                _session.House.SetBaseZ((int)_baseZ.Value);
            }
        }

        private void Undo_Click(object sender, EventArgs e)
        {
            Run(house => house.UndoDraft());
        }

        private void Redo_Click(object sender, EventArgs e)
        {
            Run(house => house.RedoDraft());
        }

        private void Place_Click(object sender, EventArgs e)
        {
            Run(house => house.Commit());
        }

        private void Drop_Click(object sender, EventArgs e)
        {
            Run(house => house.Discard());
        }
    }
}
