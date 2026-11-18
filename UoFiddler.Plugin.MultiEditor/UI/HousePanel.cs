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
        private EditorSession _session;
        private bool _updating;

        public HousePanel()
        {
            InitializeComponent();
            _wallButtons = new[] { _wall1, _wall2, _wall3, _wall4 };
            _floorButtons = new[] { _floor1, _floor2, _floor3, _floor4 };
            _storeyLabels = new[] { _storey1, _storey2, _storey3, _storey4 };
            _boldFont = new Font(Font, FontStyle.Bold);
            Disposed += (_, _) => _boldFont.Dispose();

            for (int k = 0; k < HouseDraft.MaxStoreys; ++k)
            {
                int storey = k;
                _wallButtons[k].Click += (_, _) => PickStyle?.Invoke(HousePartKind.Wall, storey);
                _floorButtons[k].Click += (_, _) => PickStyle?.Invoke(HousePartKind.Floor, storey);
            }

            _roofStyle.Click += (_, _) =>
            {
                bool flat = _session != null && !_session.House.NoRoof && _session.House.Shape == RoofShape.Flat;
                PickStyle?.Invoke(flat ? HousePartKind.Floor : HousePartKind.Roof, flat ? FlatRoof : 0);
            };
        }

        /// <summary>The storey a pick names when it is for the flat roof's floor style.</summary>
        public const int FlatRoof = -1;

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
                _wingList.BeginUpdate();
                _wingList.Items.Clear();
                for (int i = 0; i < house.Wings.Count; ++i)
                {
                    RoofWing wing = house.Wings[i];
                    string roof = wing.NoRoof ? "no roof" : wing.Shape.ToString().ToLowerInvariant();
                    _wingList.Items.Add($"Wing {i + 1}: {wing.X1 - wing.X0 + 1} x {wing.Y1 - wing.Y0 + 1}, {wing.Storeys} storey{(wing.Storeys == 1 ? string.Empty : "s")}, {(wing.Rooms > 1 ? $"{wing.Rooms} rooms, " : string.Empty)}{roof}");
                }

                _wingList.SelectedIndex = house.SelectedIndex;
                _wingList.EndUpdate();

                _storeys.Value = Math.Clamp(house.Storeys, 1, HouseDraft.MaxStoreys);
                // The drop-down lists the roof shapes in their order, then None.
                _roofShape.SelectedIndex = house.NoRoof ? _roofShape.Items.Count - 1 : (int)house.Shape;
                _ridge.SelectedIndex = (int)house.Ridge;
                _ridge.Enabled = !house.NoRoof && house.Shape == RoofShape.Gable;
                _removeWing.Enabled = house.HasSelectedWing;
                _wingHint.Text = house.HasSelectedWing ? "Selected wing:" : "New wings:";

                int top = house.TopStorey;
                int active = house.HasPending ? _session.DraftStorey : -1;
                for (int k = 0; k < HouseDraft.MaxStoreys; ++k)
                {
                    bool used = k < top || k < house.Storeys;
                    _storeyLabels[k].Visible = _wallButtons[k].Visible = _floorButtons[k].Visible = used;
                    _wallButtons[k].Text = house.Draft.WallStyles[k]?.Name ?? "(walls)";
                    _floorButtons[k].Text = house.Draft.FloorStyles[k]?.Name ?? "(floor)";

                    // The storey the chosen floor means: where the other tools work.
                    bool current = k == active;
                    _storeyLabels[k].Font = current ? _boldFont : Font;
                    _storeyLabels[k].BackColor = current ? Color.FromArgb(255, 214, 64) : Color.Transparent;
                    _storeyLabels[k].ForeColor = current ? Color.Black : ForeColor;
                }

                // A flat roof is laid in a floor style of its own; the button picks that one then.
                bool flat = !house.NoRoof && house.Shape == RoofShape.Flat;
                _roofStyle.Text = flat ? house.Draft.FlatRoofStyle?.Name ?? "(top floor's style)" : house.Draft.RoofStyle?.Name ?? "(roof)";
                _foundation.Checked = house.Draft.Foundation;
                _rooms.Value = Math.Clamp(house.Rooms, 1, HouseDraft.MaxRooms);
                _showWalls.Checked = house.ShowWalls;
                _showFloors.Checked = house.ShowFloors;
                _showRoof.Checked = house.ShowRoof;
                _saveDraft.Enabled = _blueprint.Enabled = house.HasPending;

                // The houses already placed in this design, which can be opened as drafts again.
                object chosen = _placed.SelectedItem;
                _placed.Items.Clear();
                foreach (System.Xml.Linq.XElement placed in _session.PlacedHouses)
                {
                    _placed.Items.Add(new PlacedHouse(placed));
                }

                _placed.SelectedIndex = _placed.Items.Count == 0 ? -1
                    : Math.Max(0, _placed.Items.Cast<PlacedHouse>().ToList().FindIndex(p => chosen is PlacedHouse c && c.House == p.House));
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

        private void Rooms_ValueChanged(object sender, EventArgs e)
        {
            if (!_updating && _session != null)
            {
                _session.House.SetRooms((int)_rooms.Value);
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

        private void Foundation_CheckedChanged(object sender, EventArgs e)
        {
            Run(house => house.SetFoundation(_foundation.Checked));
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
