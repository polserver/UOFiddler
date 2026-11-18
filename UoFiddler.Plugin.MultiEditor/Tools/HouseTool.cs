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
using System.Linq;
using System.Xml.Linq;
using UoFiddler.Plugin.MultiEditor.Classes;

namespace UoFiddler.Plugin.MultiEditor.Tools
{
    /// <summary>
    /// A whole house, designed before it is placed. Drag a rectangle of wall cells to add a wing;
    /// wings are selected, moved, resized and removed as <see cref="WingDraftTool"/> describes.
    /// Each wing has a number of storeys and a roof; each storey of the house has a wall style
    /// and a floor style. The house is shown as it will be built and nothing is written to the
    /// multi until it is placed, on the layers Foundation, Walls, Floors, Stairs and Roof.
    /// While the draft is open the wall, door, floor and stair tools change the draft
    /// (<see cref="DraftEditTool"/>), and every change is a step of the draft's undo history.
    /// </summary>
    internal sealed class HouseTool : WingDraftTool
    {
        private readonly RoofTool _roof;
        private HouseDraft _draft = new HouseDraft();
        private int _storeys = 1;
        private int _rooms = 1;
        private RoofShape _shape = RoofShape.Gable;
        private bool _noRoof;
        private RoofRidge _ridge = RoofRidge.Auto;

        // The last build, kept while nothing it depends on changes: a house is thousands of tiles.
        private string _builtFor;
        private (List<MultiTile> Remove, List<(ushort Id, int X, int Y, int Z, string Layer)> Add) _built;
        private int _version;

        public HouseTool(IEditorContext context, RoofTool roof) : base(context)
        {
            _roof = roof;
        }

        /// <summary>The materials and hand-made changes of the house. Change it through this tool, so the history follows.</summary>
        public HouseDraft Draft => _draft;

        /// <summary>Z of the ground floor: the first wing's, which the others share.</summary>
        public int BaseZ => Wings.Count > 0 ? Wings[0].EaveZ : _draft.BaseZ;

        /// <summary>Storeys of the tallest wing; 1 before there is one.</summary>
        public int TopStorey => Math.Max(1, HouseDraft.Top(Wings));

        /// <summary>Storeys of the selected wing, else of the wings drawn next.</summary>
        public int Storeys => SelectedWing?.Storeys ?? _storeys;

        /// <summary>Rooms of the selected wing, else of the wings drawn next.</summary>
        public int Rooms => SelectedWing?.Rooms ?? _rooms;

        public RoofShape Shape => SelectedWing?.Shape ?? _shape;

        public bool NoRoof => SelectedWing?.NoRoof ?? _noRoof;

        public RoofRidge Ridge => SelectedWing is { } wing
            ? wing.RidgeAuto ? RoofRidge.Auto : wing.RidgeAlongY ? RoofRidge.AlongY : RoofRidge.AlongX
            : _ridge;

        protected override bool Ready
        {
            get
            {
                EnsureMaterials();
                return _draft.WallStyles[0] != null;
            }
        }

        protected override string ActionName => "House";

        protected override string DefaultLayer => HouseDraft.WallsLayer;

        protected override bool PreviewsFirstClick => false;

        protected override bool GhostsFollowView => true;

        // What the preview shows. A view choice: it is no step of the history and what is placed
        // is always the whole house.
        private bool _showWalls = true;
        private bool _showFloors = true;
        private bool _showRoof = true;

        /// <summary>Walls, doors, windows, stairs and the foundation in the preview.</summary>
        public bool ShowWalls
        {
            get => _showWalls;
            set => SetShown(ref _showWalls, value);
        }

        /// <summary>Floors in the preview: off to look at the walls of a storey without the floor above.</summary>
        public bool ShowFloors
        {
            get => _showFloors;
            set => SetShown(ref _showFloors, value);
        }

        /// <summary>The roof and its gable ends in the preview.</summary>
        public bool ShowRoof
        {
            get => _showRoof;
            set => SetShown(ref _showRoof, value);
        }

        private void SetShown(ref bool field, bool value)
        {
            if (field != value)
            {
                field = value;
                RaiseChanged();
            }
        }

        protected override bool ShowsInPreview(string layer)
        {
            return layer == HouseDraft.FloorsLayer ? _showFloors : layer == RoofTool.LayerName ? _showRoof : _showWalls;
        }

        /// <summary>
        /// Storeys without a material take the one below; the ground floor and the roof take the
        /// first style the client has, until one is chosen.
        /// </summary>
        private void EnsureMaterials()
        {
            if (Context.HouseStyles == null)
            {
                return;
            }

            HouseStyle First(HousePartKind kind) => Context.HouseStyles.Styles.FirstOrDefault(s => s.Kind == kind && s.IconId != 0 && !s.Hidden);
            _draft.WallStyles[0] ??= Context.HouseStyles.Styles.FirstOrDefault(s =>
                s.Kind == HousePartKind.Wall && s.Get("South2") is var id and not 0 && Ultima.TileData.ItemTable[id].Height >= 15) ?? First(HousePartKind.Wall);
            _draft.FloorStyles[0] ??= First(HousePartKind.Floor);
            _draft.RoofStyle ??= First(HousePartKind.Roof);

            // The floors table has flat roof tiles (stone roof, marble roof): a flat roof starts with those.
            _draft.FlatRoofStyle ??= Context.HouseStyles.Styles.FirstOrDefault(s => s.Kind == HousePartKind.Floor && s.Slots.Values.Any(id =>
                (Ultima.TileData.ItemTable[id].Name ?? string.Empty).Contains("roof", StringComparison.OrdinalIgnoreCase)));
            for (int k = 1; k < HouseDraft.MaxStoreys; ++k)
            {
                _draft.WallStyles[k] ??= _draft.WallStyles[k - 1];
                _draft.FloorStyles[k] ??= _draft.FloorStyles[k - 1];
            }
        }

        // ---------------------------------------------------------------- wings

        protected override void StartCell(out int x, out int y, out int z)
        {
            Context.CellAt(Context.MouseLoc, out x, out y, out z);
            if (Context.WorkZ is { } baseZ)
            {
                z = baseZ;
            }

            Context.PlaneCell(Context.MouseLoc, z, out x, out y);
        }

        protected override int NextWingZ => BaseZ;

        /// <summary>A wing is the dragged rectangle of wall cells; it needs room for a room.</summary>
        protected override List<RoofWing> WingsFor(int startX, int startY, int endX, int endY, int z, out bool fitted)
        {
            fitted = false;
            var wings = new List<RoofWing>();
            int x0 = Math.Max(0, Math.Min(startX, endX)), x1 = Math.Max(startX, endX);
            int y0 = Math.Max(0, Math.Min(startY, endY)), y1 = Math.Max(startY, endY);
            if (x1 - x0 < 2 || y1 - y0 < 2)
            {
                return wings;
            }

            var wing = new RoofWing
            {
                X0 = x0, Y0 = y0, X1 = x1, Y1 = y1, EaveZ = z, Storeys = _storeys, Rooms = _rooms,
                Shape = _shape, NoRoof = _noRoof, RidgeAuto = _ridge == RoofRidge.Auto
            };
            Snap(wing, moved: false);
            wing.RidgeAlongY = wing.RidgeAuto ? RoofPlanner.RidgeAlongY(RoofBox(wing)) : _ridge == RoofRidge.AlongY;
            wings.Add(wing);
            return wings;
        }

        private static (int X0, int Y0, int X1, int Y1) RoofBox(RoofWing wing) => (wing.X0 + 1, wing.Y0 + 1, wing.X1 + 1, wing.Y1 + 1);

        /// <summary>
        /// A wing drawn or moved to stand right beside another would leave two walls back to back
        /// and its roof a cell short of the other's wall. It is pulled in by that cell, so the two
        /// share the wall between them: one house inside, and the lower roof runs up to the
        /// higher wall.
        /// </summary>
        protected override void Snap(RoofWing wing, bool moved)
        {
            foreach (RoofWing other in Wings)
            {
                if (ReferenceEquals(other, wing))
                {
                    continue;
                }

                bool besideX = wing.Y0 <= other.Y1 && wing.Y1 >= other.Y0;
                bool besideY = wing.X0 <= other.X1 && wing.X1 >= other.X0;
                if (besideX && wing.X0 == other.X1 + 1)
                {
                    wing.X0 -= 1;
                    wing.X1 -= moved ? 1 : 0;
                }
                else if (besideX && wing.X1 == other.X0 - 1)
                {
                    wing.X1 += 1;
                    wing.X0 += moved ? 1 : 0;
                }

                if (besideY && wing.Y0 == other.Y1 + 1)
                {
                    wing.Y0 -= 1;
                    wing.Y1 -= moved ? 1 : 0;
                }
                else if (besideY && wing.Y1 == other.Y0 - 1)
                {
                    wing.Y1 += 1;
                    wing.Y0 += moved ? 1 : 0;
                }
            }
        }

        protected override void WingResized(RoofWing wing)
        {
            if (wing.RidgeAuto)
            {
                wing.RidgeAlongY = RoofPlanner.RidgeAlongY(RoofBox(wing));
            }
        }

        protected override void Turn(RoofWing wing)
        {
            wing.RidgeAlongY = !wing.RidgeAlongY;
            wing.RidgeAuto = false;
        }

        protected override object CaptureExtra() => _draft.Clone();

        protected override void RestoreExtra(object extra)
        {
            _draft = extra is HouseDraft saved ? saved.Clone() : _draft.Fresh();
            ++_version;
        }

        // ---------------------------------------------------------------- changes to the draft

        /// <summary>Runs a change to the draft as one step of its history.</summary>
        private void Edit(Action change)
        {
            Remember();
            change();
            ++_version;
            RaiseChanged();
        }

        /// <summary>Sets the storeys of the selected wing, and of the wings drawn next.</summary>
        public void SetStoreys(int storeys)
        {
            storeys = Math.Clamp(storeys, 1, HouseDraft.MaxStoreys);
            _storeys = storeys;
            if (SelectedWing is { } wing && wing.Storeys != storeys)
            {
                Edit(() => wing.Storeys = storeys);
            }
            else
            {
                RaiseChanged();
            }
        }

        /// <summary>Sets the roof of the selected wing, and of the wings drawn next: a shape, or none.</summary>
        public void SetRoof(RoofShape shape, bool none)
        {
            (_shape, _noRoof) = (shape, none);
            if (SelectedWing is { } wing && (wing.Shape != shape || wing.NoRoof != none))
            {
                Edit(() => (wing.Shape, wing.NoRoof) = (shape, none));
            }
            else
            {
                RaiseChanged();
            }
        }

        public void SetRidge(RoofRidge ridge)
        {
            _ridge = ridge;
            if (SelectedWing is { } wing)
            {
                Edit(() =>
                {
                    wing.RidgeAuto = ridge == RoofRidge.Auto;
                    wing.RidgeAlongY = wing.RidgeAuto ? RoofPlanner.RidgeAlongY(RoofBox(wing)) : ridge == RoofRidge.AlongY;
                });
            }
            else
            {
                RaiseChanged();
            }
        }

        /// <summary>Moves the whole house up or down.</summary>
        public void SetBaseZ(int z)
        {
            z = Math.Clamp(z, -100, 100);
            if (Wings.Count == 0 || z == BaseZ)
            {
                return;
            }

            Edit(() =>
            {
                foreach (RoofWing wing in Wings)
                {
                    wing.EaveZ = z;
                }
            });
        }

        /// <summary>Divides the selected wing, and the wings drawn next, into this many rooms on every storey (1: undivided).</summary>
        public void SetRooms(int rooms)
        {
            rooms = Math.Clamp(rooms, 1, HouseDraft.MaxRooms);
            _rooms = rooms;
            if (SelectedWing is { } wing && wing.Rooms != rooms)
            {
                Edit(() => wing.Rooms = rooms);
            }
            else
            {
                RaiseChanged();
            }
        }

        // ---------------------------------------------------------------- layers

        private static readonly string[] _parts =
        {
            HouseDraft.FoundationLayer, HouseDraft.WallsLayer, HouseDraft.FloorsLayer, HouseDraft.StairsLayer, RoofTool.LayerName
        };

        // The name the house being placed gets: of the house it was reopened from, else a free one.
        private string _placeName;
        private string _reopenedName;

        /// <summary>Gives a name no placed house and no layer of the multi has yet.</summary>
        public Func<string> FreeName { get; set; }

        /// <summary>True when a placed house's parts each go on a layer of their own instead of one layer named after the house.</summary>
        public static bool LayerPerPart => !UI.EditorSettings.Current.HouseOnOneLayer;

        private string PlaceName => _placeName ??= _reopenedName ?? FreeName?.Invoke() ?? "House";

        /// <summary>The house's parts go on the layer named after the house; a tile that names another layer (a re-pieced older roof) keeps it.</summary>
        protected override string LayerFor(string layer)
        {
            return !LayerPerPart && (layer == null || Array.IndexOf(_parts, layer) >= 0) ? PlaceName : layer;
        }

        // ---------------------------------------------------------------- drafts kept outside the tool

        /// <summary>The house last placed, as it is kept with the multi.</summary>
        public XElement LastPlaced { get; private set; }

        /// <summary>Raised when a house was placed, with what is kept of it.</summary>
        public event Action<XElement> Placed;

        /// <summary>The draft as XML: what a house draft file holds.</summary>
        public XElement ToXml(string name)
        {
            return HouseDraftFile.ToXml(Wings, _draft, name);
        }

        /// <summary>
        /// Makes a saved house the draft, in place of the one being worked on (a step of the
        /// history). A house reaching past the multi's first row or column is moved inside.
        /// </summary>
        public void OpenXml(XElement house, bool keepName = false)
        {
            EnsureMaterials();
            _reopenedName = keepName ? (string)house.Attribute("name") : null;
            var (wings, draft, _) = HouseDraftFile.FromXml(house, Context.HouseStyles, _draft);
            if (wings.Count == 0)
            {
                throw new System.IO.InvalidDataException("The house draft has no wings.");
            }

            int dx = Math.Max(0, -wings.Min(w => w.X0)), dy = Math.Max(0, -wings.Min(w => w.Y0));
            if (dx != 0 || dy != 0)
            {
                foreach (RoofWing wing in wings)
                {
                    (wing.X0, wing.X1, wing.Y0, wing.Y1) = (wing.X0 + dx, wing.X1 + dx, wing.Y0 + dy, wing.Y1 + dy);
                }

                draft.Shift(dx, dy);
            }

            LoadWings(wings);
            _draft = draft;
            ++_version;
            RaiseChanged();
        }

        /// <summary>The tiles a saved house builds, each with its layer: what placing it put into the multi.</summary>
        public List<(ushort Id, int X, int Y, int Z, string Layer)> TilesOf(XElement house)
        {
            EnsureMaterials();
            var (wings, draft, _) = HouseDraftFile.FromXml(house, Context.HouseStyles, _draft);
            HouseDraft own = _draft;
            _draft = draft;
            ++_version;
            try
            {
                return new List<(ushort Id, int X, int Y, int Z, string Layer)>(Plan(wings).Add);
            }
            finally
            {
                _draft = own;
                ++_version;
            }
        }

        protected override void OnPlaced()
        {
            LastPlaced = ToXml(PlaceName);
            _placeName = null;
            _reopenedName = null;
            Placed?.Invoke(LastPlaced);
        }

        public void SetFoundation(bool on)
        {
            if (_draft.Foundation != on)
            {
                Edit(() => _draft.Foundation = on);
            }
        }

        /// <summary>
        /// Sets a wall, floor or roof style. A wall or floor style goes to one storey, or to
        /// every storey when <paramref name="storey"/> is null.
        /// </summary>
        public void SetMaterial(HouseStyle style, int? storey)
        {
            if (style == null)
            {
                return;
            }

            Edit(() =>
            {
                if (style.Kind == HousePartKind.Roof)
                {
                    _draft.RoofStyle = style;
                    return;
                }

                HouseStyle[] target = style.Kind == HousePartKind.Floor ? _draft.FloorStyles : style.Kind == HousePartKind.Wall ? _draft.WallStyles : null;
                for (int k = 0; target != null && k < HouseDraft.MaxStoreys; ++k)
                {
                    if (storey == null || storey == k)
                    {
                        target[k] = style;
                    }
                }
            });
        }

        /// <summary>Sets the floor style flat roofs are laid in.</summary>
        public void SetFlatRoofStyle(HouseStyle style)
        {
            if (style is { Kind: HousePartKind.Floor })
            {
                Edit(() => _draft.FlatRoofStyle = style);
            }
        }

        /// <summary>The storey the work plane is on (the chosen floor); the ground floor with all floors shown.</summary>
        public int StoreyFor(int? workZ)
        {
            if (workZ is not { } z)
            {
                return 0;
            }

            return Math.Clamp((int)Math.Round((z - BaseZ) / (double)HouseDraft.StoreyHeight), 0, TopStorey - 1);
        }

        /// <summary>The wall edges of a storey as the draft has them now.</summary>
        public void EdgesOf(int storey, out HashSet<(int X, int Y)> south, out HashSet<(int X, int Y)> east)
        {
            _draft.Edges(Wings, storey, out south, out east);
        }

        /// <summary>Adds wall edges to a storey (partitions), or takes edges away (openings).</summary>
        public void EditEdges(int storey, IReadOnlyCollection<(int X, int Y)> south, IReadOnlyCollection<(int X, int Y)> east, bool erase)
        {
            if (south.Count + east.Count == 0)
            {
                return;
            }

            Edit(() =>
            {
                if (erase)
                {
                    _draft.AddedSouth[storey].ExceptWith(south);
                    _draft.AddedEast[storey].ExceptWith(east);
                    _draft.ErasedSouth[storey].UnionWith(south);
                    _draft.ErasedEast[storey].UnionWith(east);
                }
                else
                {
                    _draft.ErasedSouth[storey].ExceptWith(south);
                    _draft.ErasedEast[storey].ExceptWith(east);
                    _draft.AddedSouth[storey].UnionWith(south);
                    _draft.AddedEast[storey].UnionWith(east);
                }
            });
        }

        /// <summary>Turns the wall in a cell into a window, or back. False when the cell has no straight wall.</summary>
        public bool ToggleWindow(int storey, int x, int y)
        {
            EdgesOf(storey, out HashSet<(int X, int Y)> south, out HashSet<(int X, int Y)> east);
            if (south.Contains((x, y)) == east.Contains((x, y)))
            {
                return false;
            }

            Edit(() =>
            {
                if (!_draft.Windows.Remove((storey, x, y)))
                {
                    _draft.Windows.Add((storey, x, y));
                    _draft.Doors.Remove((storey, x, y));
                }
            });
            return true;
        }

        // Hinge variants per wall direction, in the order a click cycles through them.
        private static readonly string[] _doorsAlongX = { "Piece3", "Piece1", "Piece4", "Piece2" };
        private static readonly string[] _doorsAlongY = { "Piece5", "Piece7", "Piece6", "Piece8" };

        /// <summary>
        /// Hangs a door in the wall of a cell, or turns the door there to its next hinge variant;
        /// with <paramref name="remove"/> takes it out. False when the cell has no straight wall.
        /// </summary>
        public bool CycleDoor(int storey, int x, int y, HouseStyle style, bool remove)
        {
            EdgesOf(storey, out HashSet<(int X, int Y)> south, out HashSet<(int X, int Y)> east);
            bool alongX = south.Contains((x, y));
            if (alongX == east.Contains((x, y)))
            {
                return false;
            }

            bool has = _draft.Doors.TryGetValue((storey, x, y), out (HouseStyle Style, string Piece) door);
            if (remove)
            {
                if (has)
                {
                    Edit(() => _draft.Doors.Remove((storey, x, y)));
                }

                return has;
            }

            if (style == null || style.Kind != HousePartKind.Door)
            {
                return false;
            }

            string[] variants = alongX ? _doorsAlongX : _doorsAlongY;
            int next = has && door.Style == style ? (Array.IndexOf(variants, door.Piece) + 1) % variants.Length : 0;
            for (int i = 0; i < variants.Length; ++i)
            {
                string piece = variants[(next + i) % variants.Length];
                if (style.Get(piece) != 0)
                {
                    Edit(() =>
                    {
                        _draft.Doors[(storey, x, y)] = (style, piece);
                        _draft.Windows.Remove((storey, x, y));
                    });
                    return true;
                }
            }

            return false;
        }

        /// <summary>Lays an area of a storey's floor in another style.</summary>
        public void AddFloorArea(int storey, int x0, int y0, int x1, int y1, HouseStyle style)
        {
            if (style is { Kind: HousePartKind.Floor })
            {
                Edit(() => _draft.FloorAreas.Add((storey, Math.Min(x0, x1), Math.Min(y0, y1), Math.Max(x0, x1), Math.Max(y0, y1), style)));
            }
        }

        /// <summary>Puts stairs on a storey, their bottom step in a cell; they open the floor above.</summary>
        public void AddStairs(int storey, int x, int y, string column, HouseStyle style)
        {
            if (style is { Kind: HousePartKind.Stair } && style.Get(column) != 0)
            {
                Edit(() => _draft.Stairs.Add((storey, x, y, column, style)));
            }
        }

        /// <summary>Takes out the stairs whose bottom step is in a cell.</summary>
        public bool RemoveStairs(int storey, int x, int y)
        {
            int index = _draft.Stairs.FindLastIndex(s => s.Storey == storey && s.X == x && s.Y == y);
            if (index < 0)
            {
                return false;
            }

            Edit(() => _draft.Stairs.RemoveAt(index));
            return true;
        }

        // ---------------------------------------------------------------- building

        protected override (List<MultiTile> Remove, List<(ushort Id, int X, int Y, int Z, string Layer)> Add) Plan(IReadOnlyList<RoofWing> wings)
        {
            if (wings.Count == 0)
            {
                return (new List<MultiTile>(), new List<(ushort, int, int, int, string)>());
            }

            EnsureMaterials();
            string key = string.Join(";", wings.Select(w =>
                             $"{w.X0},{w.Y0},{w.X1},{w.Y1},{w.EaveZ},{w.Storeys},{w.Rooms},{w.Shape},{w.NoRoof},{w.RidgeAlongY}")) +
                         $"|{_version}|{_roof.Gables}|{_roof.GableFill}|{Context.CompList.UndoList.Count}|{Context.CompList.RedoList.Count}";
            if (key == _builtFor)
            {
                return _built;
            }

            int baseZ = wings[0].EaveZ;
            _draft.BaseZ = baseZ;
            HouseDraft.Built built = _draft.Build(wings, Context.HouseStyles);
            var remove = new List<MultiTile>();
            var add = built.Tiles;

            // A new floor replaces the flat tiles lying where it goes, as the floor tool does.
            MultiEditorComponentList list = Context.CompList;
            foreach ((ushort _, int x, int y, int z, string layer) in built.Tiles)
            {
                if (layer != HouseDraft.FloorsLayer)
                {
                    continue;
                }

                foreach (MultiTile tile in list.GetTilesAt(x, y))
                {
                    if (tile.Z == z && tile.Height == 0 && list.IsShown(tile))
                    {
                        remove.Add(tile);
                    }
                }
            }

            // The roof: each wing's walls one cell further along X and Y, eaves on its top storey.
            var roofWings = new List<RoofWing>();
            foreach (RoofWing wing in wings)
            {
                if (wing.NoRoof || wing.Shape == RoofShape.Flat)
                {
                    continue;
                }

                RoofWing roof = wing.Clone();
                (roof.X0, roof.Y0, roof.X1, roof.Y1) = RoofBox(wing);
                roof.EaveZ = baseZ + (Math.Clamp(wing.Storeys, 1, HouseDraft.MaxStoreys) * HouseDraft.StoreyHeight);
                roofWings.Add(roof);
            }

            if (roofWings.Count > 0 && _draft.RoofStyle != null)
            {
                // A lower wing's roof stops at the walls of a taller one. A roof piece is seen half a
                // cell before its own cell, and the walls stand on the far edges of theirs: the
                // first row and column inside a north or west wall still reach it from outside.
                bool Hidden(int x, int y, int z)
                {
                    foreach (RoofWing wing in wings)
                    {
                        if (x >= wing.X0 + 2 && x <= wing.X1 && y >= wing.Y0 + 2 && y <= wing.Y1 &&
                            z < baseZ + (wing.Storeys * HouseDraft.StoreyHeight))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                // Wings of the same height share one roof; a lower wing has a roof of its own, which
                // lies under the eaves of a taller one where the two meet.
                foreach (IGrouping<int, RoofWing> level in roofWings.GroupBy(r => r.EaveZ))
                {
                    var (roofRemove, roofAdd) = _roof.PlanRoof(level.ToList(), _draft.RoofStyle, built.Walls, Hidden);
                    remove.AddRange(roofRemove.Where(t => !remove.Contains(t)));
                    foreach (var (id, x, y, z, layer) in roofAdd)
                    {
                        add.Add((id, x, y, z, layer ?? RoofTool.LayerName));
                    }
                }
            }

            _builtFor = key;
            _built = (remove, add);
            return _built;
        }
    }
}
