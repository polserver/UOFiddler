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
using Ultima;
using UoFiddler.Plugin.MultiEditor.Classes;

namespace UoFiddler.Plugin.MultiEditor.Tools
{
    /// <summary>Which way a gable roof's ridge runs.</summary>
    internal enum RoofRidge
    {
        /// <summary>As the official houses: along the longer side, or so the ridge gets its crosspiece.</summary>
        Auto,
        AlongX,
        AlongY
    }

    /// <summary>Which gable ends of a roof are closed.</summary>
    internal enum RoofGables
    {
        /// <summary>The south or east end, as the official houses do.</summary>
        Front,
        Both,
        None
    }

    /// <summary>What closes a gable.</summary>
    internal enum RoofGableFill
    {
        /// <summary>3 high cap pieces matching the walls.</summary>
        Caps,

        /// <summary>Whole wall pieces as far as they fit under the slope, caps for the rest: for big, high roofs.</summary>
        Walls
    }

    /// <summary>One rectangle of a roof being shaped: where it lies, how high its eaves are and how it slopes.</summary>
    internal sealed class RoofWing
    {
        public int X0 { get; set; }

        public int Y0 { get; set; }

        public int X1 { get; set; }

        public int Y1 { get; set; }

        public int EaveZ { get; set; }

        public RoofShape Shape { get; set; }

        public bool RidgeAlongY { get; set; }

        /// <summary>True while the ridge follows the box's proportions as it is resized.</summary>
        public bool RidgeAuto { get; set; }

        public (int X0, int Y0, int X1, int Y1) Box => (X0, Y0, X1, Y1);

        public RoofWing Clone() => (RoofWing)MemberwiseClone();

        public void CopyFrom(RoofWing other)
        {
            (X0, Y0, X1, Y1, EaveZ) = (other.X0, other.Y0, other.X1, other.Y1, other.EaveZ);
            (Shape, RidgeAlongY, RidgeAuto) = (other.Shape, other.RidgeAlongY, other.RidgeAuto);
        }
    }

    /// <summary>
    /// Roofs, shaped before they are placed. The first click on (or inside) a walled room fits a
    /// roof to its walls, part by part on an L or T shaped house; a drag over the walls, the floor
    /// inside them or the area the roof will cover fits it too, and any other drag is a roof of
    /// that size on the work plane. The roof then stays pending, shown as it will be built:
    /// <list type="bullet">
    /// <item>click a wing to select it, drag it to move it, drag an edge of the selected wing to
    /// resize it;</item>
    /// <item>drag anywhere else to add a wing, which joins the others with valleys and T pieces;</item>
    /// <item>Shift+click turns a wing's ridge, the Shape and Ridge options
    /// change the selected wing, Delete removes it;</item>
    /// <item>Enter, or switching to another tool, places the roof on the layer named
    /// <see cref="LayerName"/> in one undo step; Esc drops it.</item>
    /// </list>
    /// A fitted roof sits on the wall tops whatever floor is chosen, overhangs the south and east
    /// walls by a cell and grows the multi when that cell lies outside it. Slopes rise 3 per cell
    /// like the official house roofs. A roof already there is replaced when it lies wholly under
    /// the new one and joined otherwise.
    /// </summary>
    internal sealed class RoofTool : EditorTool
    {
        /// <summary>The layer a placed roof goes on; made on first use.</summary>
        public const string LayerName = "Roof";

        private const int _rise = RoofPlanner.Rise;
        private const int _storey = 20;
        private const string _capStyles = "Miscellaneous Roof Pieces";

        private enum DragKind
        {
            None,
            New,
            Move,
            Resize
        }

        private readonly List<RoofWing> _wings = new List<RoofWing>();
        private int _selected = -1;
        private HouseStyle _lastStyle;
        private RoofShape _shape = RoofShape.Gable;
        private RoofRidge _ridge = RoofRidge.Auto;

        private DragKind _drag;
        private int _startX;
        private int _startY;
        private int _endX;
        private int _endY;
        private int _z;
        private RoofWing _before;
        private int _grabX;
        private int _grabY;
        private bool _west;
        private bool _east;
        private bool _north;
        private bool _south;

        public RoofTool(IEditorContext context) : base(context)
        {
        }

        /// <summary>Raised when the pending roof starts, changes or ends.</summary>
        public event Action Changed;

        /// <summary>Raised after a roof was placed.</summary>
        public event Action Committed;

        /// <summary>The shape of new wings; also sets the selected wing's.</summary>
        public RoofShape Shape
        {
            get => _shape;
            set
            {
                _shape = value;
                if (SelectedWing is { } wing)
                {
                    wing.Shape = value;
                }
            }
        }

        /// <summary>The ridge direction of new wings; also sets the selected wing's.</summary>
        public RoofRidge Ridge
        {
            get => _ridge;
            set
            {
                _ridge = value;
                if (SelectedWing is { } wing)
                {
                    wing.RidgeAuto = value == RoofRidge.Auto;
                    wing.RidgeAlongY = value == RoofRidge.Auto ? RoofPlanner.RidgeAlongY(wing.Box) : value == RoofRidge.AlongY;
                }
            }
        }

        public RoofGables Gables { get; set; } = RoofGables.Front;

        public RoofGableFill GableFill { get; set; } = RoofGableFill.Caps;

        /// <summary>True while a roof is being shaped and not yet placed.</summary>
        public bool HasPending => _wings.Count > 0;

        public bool HasSelectedWing => SelectedWing != null;

        public IReadOnlyList<RoofWing> Wings => _wings;

        public override bool IsDragging => _drag != DragKind.None;

        public override bool TracksCursorCell => true;

        private RoofWing SelectedWing => _selected >= 0 && _selected < _wings.Count ? _wings[_selected] : null;

        /// <summary>The roof style chosen in the House tab; a pending roof keeps the last one.</summary>
        private HouseStyle Style
        {
            get
            {
                if (Context.HouseStyle is { Kind: HousePartKind.Roof } style)
                {
                    _lastStyle = style;
                    return style;
                }

                return HasPending ? _lastStyle : null;
            }
        }

        // ---------------------------------------------------------------- shaping

        public override void OnMouseDown(MouseEventArgs e)
        {
            if (Style == null)
            {
                return;
            }

            if (HasPending)
            {
                int hit = HitWing(out int cx, out int cy);
                if (hit >= 0)
                {
                    RoofWing wing = _wings[hit];
                    bool wasSelected = hit == _selected;
                    _selected = hit;
                    if (ShiftHeld)
                    {
                        Turn(wing);
                        Changed?.Invoke();
                        return;
                    }

                    _before = wing.Clone();
                    (_grabX, _grabY) = (cx, cy);

                    // The selected wing is resized by its outermost cells; a side too short to
                    // have a middle moves the wing instead.
                    bool wide = wing.X1 - wing.X0 >= 2, deep = wing.Y1 - wing.Y0 >= 2;
                    _west = wasSelected && wide && cx == wing.X0;
                    _east = wasSelected && wide && cx == wing.X1;
                    _north = wasSelected && deep && cy == wing.Y0;
                    _south = wasSelected && deep && cy == wing.Y1;
                    _drag = _west || _east || _north || _south ? DragKind.Resize : DragKind.Move;
                    Changed?.Invoke();
                    return;
                }

                // A new wing, level with the wing worked on last.
                _z = (SelectedWing ?? _wings[^1]).EaveZ;
                Context.PlaneCell(Context.MouseLoc, _z, out _startX, out _startY);
            }
            else
            {
                BaseCell(out _startX, out _startY, out _z);
            }

            (_endX, _endY) = (_startX, _startY);
            _drag = DragKind.New;
        }

        public override void OnMouseMove(MouseEventArgs e)
        {
            switch (_drag)
            {
                case DragKind.New:
                    Context.PlaneCell(Context.MouseLoc, _z, out _endX, out _endY);
                    break;
                case DragKind.Move:
                case DragKind.Resize:
                    DragWing();
                    break;
            }
        }

        public override void OnMouseUp(MouseEventArgs e)
        {
            DragKind drag = _drag;
            if (drag is DragKind.Move or DragKind.Resize)
            {
                DragWing();
            }

            _drag = DragKind.None;
            if (drag == DragKind.New)
            {
                Context.PlaneCell(Context.MouseLoc, _z, out _endX, out _endY);
                List<RoofWing> wings = WingsFor(_startX, _startY, _endX, _endY, _z, out bool fitted);
                if (HasPending && !fitted && _startX == _endX && _startY == _endY)
                {
                    // A click beside the roof lets go of the selected wing.
                    _selected = -1;
                }
                else
                {
                    _wings.AddRange(wings);
                    _selected = _wings.Count - wings.Count;
                }

                Changed?.Invoke();
            }
            else if (drag != DragKind.None)
            {
                _before = null;
                Changed?.Invoke();
            }
        }

        /// <summary>Stops a drag: a wing being moved or resized goes back to where it was.</summary>
        public override void Cancel()
        {
            if (_drag is DragKind.Move or DragKind.Resize && _before != null && SelectedWing is { } wing)
            {
                wing.CopyFrom(_before);
            }

            _before = null;
            _drag = DragKind.None;
        }

        /// <summary>Drops the pending roof.</summary>
        public void Discard()
        {
            bool had = HasPending;
            _wings.Clear();
            _selected = -1;
            _before = null;
            _drag = DragKind.None;
            if (had)
            {
                Changed?.Invoke();
            }
        }

        /// <summary>Removes the selected wing from the pending roof.</summary>
        public void DeleteWing()
        {
            if (SelectedWing == null || IsDragging)
            {
                return;
            }

            _wings.RemoveAt(_selected);
            _selected = _wings.Count - 1;
            Changed?.Invoke();
        }

        /// <summary>
        /// Places the pending roof: its tiles go on the <see cref="LayerName"/> layer, pieces of
        /// an older roof that had to change stay on their own layer, all in one undo step.
        /// </summary>
        public void Commit()
        {
            Cancel();
            if (!HasPending)
            {
                return;
            }

            if (Style != null)
            {
                var (remove, add) = Plan(_wings);
                if (remove.Count > 0 || add.Count > 0)
                {
                    Context.CompList.ReplaceTilesOnLayer(remove, add, "Roof", LayerName);
                    Context.AfterEdit(add.Count > 0 ? add.Max(a => a.Z) : null);
                    Committed?.Invoke();
                }
            }

            Discard();
        }

        /// <summary>Lays a roof in one go, as a press at one cell and a release at another would shape it. For scripts and tests.</summary>
        public void Place(int startX, int startY, int endX, int endY, int z)
        {
            if (Style == null)
            {
                return;
            }

            _wings.AddRange(WingsFor(startX, startY, endX, endY, z, out _));
            Commit();
        }

        /// <summary>
        /// The eaves go on the work plane, or on top of what the mouse points at (a wall top).
        /// </summary>
        private void BaseCell(out int x, out int y, out int z)
        {
            Context.CellAt(Context.MouseLoc, out x, out y, out z);
            if (Context.WorkZ is { } baseZ)
            {
                z = baseZ;
                Context.PlaneCell(Context.MouseLoc, z, out x, out y);
            }
        }

        /// <summary>The pending wing under the mouse, the selected one first, and the cell pointed at on its eaves.</summary>
        private int HitWing(out int cx, out int cy)
        {
            (cx, cy) = (0, 0);
            for (int pass = 0; pass < 2; ++pass)
            {
                for (int i = _wings.Count - 1; i >= 0; --i)
                {
                    if ((pass == 0) != (i == _selected))
                    {
                        continue;
                    }

                    RoofWing wing = _wings[i];
                    Context.PlaneCell(Context.MouseLoc, wing.EaveZ, out cx, out cy);
                    if (cx >= wing.X0 && cx <= wing.X1 && cy >= wing.Y0 && cy <= wing.Y1)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        private void DragWing()
        {
            if (_before == null || SelectedWing is not { } wing)
            {
                return;
            }

            Context.PlaneCell(Context.MouseLoc, _before.EaveZ, out int cx, out int cy);
            int dx = cx - _grabX, dy = cy - _grabY;
            if (_drag == DragKind.Move)
            {
                dx = Math.Max(dx, -_before.X0);
                dy = Math.Max(dy, -_before.Y0);
                (wing.X0, wing.X1, wing.Y0, wing.Y1) = (_before.X0 + dx, _before.X1 + dx, _before.Y0 + dy, _before.Y1 + dy);
                return;
            }

            wing.X0 = _west ? Math.Clamp(_before.X0 + dx, 0, _before.X1) : _before.X0;
            wing.X1 = _east ? Math.Max(_before.X1 + dx, _before.X0) : _before.X1;
            wing.Y0 = _north ? Math.Clamp(_before.Y0 + dy, 0, _before.Y1) : _before.Y0;
            wing.Y1 = _south ? Math.Max(_before.Y1 + dy, _before.Y0) : _before.Y1;
            if (wing.RidgeAuto)
            {
                wing.RidgeAlongY = RoofPlanner.RidgeAlongY(wing.Box);
            }
        }

        /// <summary>Turns a wing's ridge the other way.</summary>
        private static void Turn(RoofWing wing)
        {
            wing.RidgeAlongY = !wing.RidgeAlongY;
            wing.RidgeAuto = false;
        }

        /// <summary>
        /// The wings a stroke makes. Walls sit on the +X/+Y edges of their cells, so the roof over
        /// them is their box one cell further along X and Y (as in the official houses, which
        /// overhang their east and south walls by a cell). A click takes the walls of the room it
        /// points at, each part of an L or T shaped house as a wing of its own; a drag that comes
        /// within a cell of such a roof on every side means that roof. Any other stroke is the
        /// roof itself, at <paramref name="z"/>. Shift turns a gable's ridge.
        /// </summary>
        private List<RoofWing> WingsFor(int startX, int startY, int endX, int endY, int z, out bool fitted)
        {
            int x0 = Math.Min(startX, endX), x1 = Math.Max(startX, endX);
            int y0 = Math.Min(startY, endY), y1 = Math.Max(startY, endY);
            bool click = x0 == x1 && y0 == y1;
            var wings = new List<RoofWing>();
            fitted = FitToWalls(x0, y0, x1, y1, z, out WallBox walls);
            if (fitted)
            {
                (x0, y0, x1, y1) = (walls.X0 + 1, walls.Y0 + 1, walls.X1 + 1, walls.Y1 + 1);
                z = walls.Top;
                if (click && Shape == RoofShape.Gable)
                {
                    var parts = RoofPlanner.Wings(RoofRegion(walls));
                    if (parts.Count > 1)
                    {
                        foreach (((int X0, int Y0, int X1, int Y1) box, bool alongY) in parts)
                        {
                            wings.Add(new RoofWing { X0 = box.X0, Y0 = box.Y0, X1 = box.X1, Y1 = box.Y1, EaveZ = z, RidgeAlongY = alongY });
                        }

                        return wings;
                    }
                }
            }

            bool auto = Ridge == RoofRidge.Auto;
            bool ridgeAlongY = auto ? RoofPlanner.RidgeAlongY((x0, y0, x1, y1)) : Ridge == RoofRidge.AlongY;
            if (ShiftHeld && Shape == RoofShape.Gable)
            {
                (ridgeAlongY, auto) = (!ridgeAlongY, false);
            }

            wings.Add(new RoofWing
            {
                X0 = x0, Y0 = y0, X1 = x1, Y1 = y1, EaveZ = z, Shape = Shape,
                RidgeAlongY = ridgeAlongY, RidgeAuto = auto
            });
            return wings;
        }

        // ---------------------------------------------------------------- overlay

        public override void PaintOverlay(IToolOverlay overlay)
        {
            if (Style == null)
            {
                return;
            }

            // What is shown is what Enter would build: the pending wings, with the one being
            // dragged out; before there is a roof, the one a click here would start.
            var wings = new List<RoofWing>(_wings);
            if (_drag == DragKind.New)
            {
                wings.AddRange(WingsFor(_startX, _startY, _endX, _endY, _z, out _));
            }
            else if (!HasPending)
            {
                BaseCell(out int x, out int y, out int z);
                wings.AddRange(WingsFor(x, y, x, y, z, out _));
            }

            var (remove, add) = Plan(wings);
            foreach (MultiTile tile in remove)
            {
                overlay.Highlight(tile, TileHighlight.Remove);
            }

            foreach (var (id, x, y, z, _) in add)
            {
                overlay.Ghost(id, x, y, z);
            }

            for (int i = 0; i < _wings.Count; ++i)
            {
                RoofWing wing = _wings[i];
                Point top = Corner(wing.X0, wing.Y0, wing.EaveZ), right = Corner(wing.X1 + 1, wing.Y0, wing.EaveZ);
                Point bottom = Corner(wing.X1 + 1, wing.Y1 + 1, wing.EaveZ), left = Corner(wing.X0, wing.Y1 + 1, wing.EaveZ);
                overlay.Polyline(new[] { top, right, bottom, left }, closed: true);
                if (i != _selected)
                {
                    continue;
                }

                // Grips in the middle of the selected wing's sides.
                foreach ((Point a, Point b) in new[] { (top, right), (right, bottom), (bottom, left), (left, top) })
                {
                    var mid = new Point((a.X + b.X) / 2, (a.Y + b.Y) / 2);
                    overlay.Polyline(new[]
                    {
                        new Point(mid.X - 5, mid.Y - 5), new Point(mid.X + 5, mid.Y - 5),
                        new Point(mid.X + 5, mid.Y + 5), new Point(mid.X - 5, mid.Y + 5)
                    }, closed: true);
                }
            }
        }

        /// <summary>Canvas position of the top corner of a cell's diamond at height z.</summary>
        private static Point Corner(int x, int y, int z)
        {
            return new Point(((x - y) * 22) + MultiEditorComponentList.GapXMod, ((x + y) * 22) - (z * 4) - 22);
        }

        // ---------------------------------------------------------------- planning

        /// <summary>
        /// The tiles a roof of these wings takes away and adds. An added tile with no layer goes
        /// on the roof layer; a re-pieced tile of an older roof names the layer it stays on.
        /// </summary>
        private (List<MultiTile> Remove, List<(ushort Id, int X, int Y, int Z, TileLayer Layer)> Add) Plan(IReadOnlyList<RoofWing> wings)
        {
            var remove = new List<MultiTile>();
            var add = new List<(ushort Id, int X, int Y, int Z, TileLayer Layer)>();
            HouseStyle style = Style;
            if (wings.Count == 0 || style == null)
            {
                return (remove, add);
            }

            MultiEditorComponentList list = Context.CompList;
            var heights = new Dictionary<(int X, int Y), int>();
            var fixedPieces = new Dictionary<(int X, int Y), string>();
            foreach (RoofWing wing in wings)
            {
                RoofPlanner.AddWing(heights, fixedPieces, wing.Box, wing.RidgeAlongY, wing.Shape, wing.EaveZ);
            }

            var footprint = new HashSet<(int X, int Y)>(heights.Keys);
            JoinExisting(footprint, heights, fixedPieces, remove, add);

            foreach ((int x, int y) in footprint)
            {
                if (!fixedPieces.TryGetValue((x, y), out string column))
                {
                    column = RoofPlanner.Piece(heights, x, y, out string fallback) ?? (wings[0].RidgeAlongY ? "West" : "North");
                    if (style.Get(column) == 0 && fallback != null)
                    {
                        column = fallback;
                    }
                }

                ushort id = style.Get(column);
                if (id != 0)
                {
                    add.Add((id, x, y, heights[(x, y)], null));
                }
            }

            // Caps from an earlier roof over these walls go too, so a turned ridge leaves none behind.
            foreach (RoofWing wing in wings)
            {
                for (int x = wing.X0 - 1; x <= wing.X1; ++x)
                {
                    for (int y = wing.Y0 - 1; y <= wing.Y1; ++y)
                    {
                        foreach (MultiTile tile in list.GetTilesAt(x, y))
                        {
                            if (IsCap(tile.Id) && list.IsShown(tile) && !remove.Contains(tile))
                            {
                                remove.Add(tile);
                            }
                        }
                    }
                }
            }

            // Every gable wing closes the ends it shows.
            var caps = new HashSet<(ushort, int, int, int)>();
            foreach (RoofWing wing in wings)
            {
                if (wing.Shape == RoofShape.Gable)
                {
                    AddGables(wing, heights, caps, add);
                }
            }

            return (remove, add);
        }

        /// <summary>
        /// Closes a gable's ends: in the wall row at the gable end, each wall cell is filled up
        /// to where the wing's slope has risen over it. An end that runs into another wing shows
        /// no gable and gets none: only cells with no roof beyond them are filled.
        /// </summary>
        private void AddGables(RoofWing wing, Dictionary<(int X, int Y), int> heights, HashSet<(ushort, int, int, int)> placed,
            List<(ushort Id, int X, int Y, int Z, TileLayer Layer)> add)
        {
            MultiEditorComponentList list = Context.CompList;
            bool ridgeAlongY = wing.RidgeAlongY;
            int z = wing.EaveZ;
            int wallX0 = wing.X0 - 1, wallX1 = wing.X1 - 1;
            int wallY0 = wing.Y0 - 1, wallY1 = wing.Y1 - 1;
            int across0 = ridgeAlongY ? wing.X0 : wing.Y0;
            int across1 = ridgeAlongY ? wing.X1 : wing.Y1;

            var gableRows = new List<(int Row, bool Front)>();
            if (Gables != RoofGables.None)
            {
                gableRows.Add((ridgeAlongY ? wallY1 : wallX1, true));
            }

            if (Gables == RoofGables.Both)
            {
                gableRows.Add((ridgeAlongY ? wallY0 : wallX0, false));
            }

            foreach ((int row, bool front) in gableRows)
            {
                ushort wall = GableWall(row, ridgeAlongY, wallX0, wallX1, wallY0, wallY1, z);
                ushort cap = wall != 0 ? CapFor(wall, ridgeAlongY) : (ushort)0;

                // Whole walls: the plain piece of the wall's own style that runs along the gable.
                ushort block = 0;
                int blockHeight = 0;
                if (GableFill == RoofGableFill.Walls && wall != 0)
                {
                    block = wall;
                    if (Context.HouseStyles.TryGetRole(wall, out HouseStyle wallStyle, out _) &&
                        wallStyle.Get(ridgeAlongY ? "South2" : "East2") is var plain and not 0)
                    {
                        block = plain;
                    }

                    blockHeight = TileData.ItemTable[block].Height;
                }

                if (cap == 0 && blockHeight <= 0)
                {
                    continue;
                }

                for (int a = across0; a <= across1; ++a)
                {
                    int x = ridgeAlongY ? a : row;
                    int y = ridgeAlongY ? row : a;

                    // The front wall stands a cell inside the roof's last row, the back wall a
                    // cell before its first; the end shows where no roof lies past it.
                    (int X, int Y) beyond = front ? (ridgeAlongY ? (x, y + 2) : (x + 2, y)) : (x, y);
                    if (!list.InBounds(x, y) || heights.ContainsKey(beyond) || !HasStoreyWall(x, y, z))
                    {
                        continue;
                    }

                    int top = _rise * Math.Min(a - across0, across1 - a);
                    int filled = 0;
                    while (blockHeight > 0 && top - filled >= blockHeight)
                    {
                        if (placed.Add((block, x, y, z + filled)))
                        {
                            add.Add((block, x, y, z + filled, null));
                        }

                        filled += blockHeight;
                    }

                    while (cap != 0 && top - filled >= _rise)
                    {
                        if (placed.Add((cap, x, y, z + filled)))
                        {
                            add.Add((cap, x, y, z + filled, null));
                        }

                        filled += _rise;
                    }
                }
            }
        }

        /// <summary>A connected run of wall cells and the Z its walls end at.</summary>
        private readonly record struct WallBox(int X0, int Y0, int X1, int Y1, int Top, HashSet<(int X, int Y)> Cells);

        /// <summary>
        /// The cells a roof over these walls covers: the wall cells and everything they enclose,
        /// one cell further along X and Y. For walls that do not close it is the walls alone.
        /// </summary>
        private static HashSet<(int X, int Y)> RoofRegion(WallBox walls)
        {
            // Flood the outside, from a ring one cell around the box.
            var outside = new HashSet<(int X, int Y)>();
            var queue = new Queue<(int X, int Y)>();
            outside.Add((walls.X0 - 1, walls.Y0 - 1));
            queue.Enqueue((walls.X0 - 1, walls.Y0 - 1));
            while (queue.Count > 0)
            {
                (int x, int y) = queue.Dequeue();
                foreach ((int dx, int dy) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) })
                {
                    (int X, int Y) next = (x + dx, y + dy);
                    if (next.X >= walls.X0 - 1 && next.X <= walls.X1 + 1 && next.Y >= walls.Y0 - 1 && next.Y <= walls.Y1 + 1 &&
                        !walls.Cells.Contains(next) && outside.Add(next))
                    {
                        queue.Enqueue(next);
                    }
                }
            }

            var region = new HashSet<(int X, int Y)>();
            for (int x = walls.X0; x <= walls.X1; ++x)
            {
                for (int y = walls.Y0; y <= walls.Y1; ++y)
                {
                    if (!outside.Contains((x, y)))
                    {
                        region.Add((x + 1, y + 1));
                    }
                }
            }

            return region;
        }

        /// <summary>
        /// Fits a new roof to the roof already there. A roof lying wholly under the new one is
        /// replaced. One that reaches beyond it is joined: each cell keeps the higher of the two,
        /// and the old cells next to the new roof get the piece their place now calls for (a
        /// valley, a T where the ridges meet), in their own style. Cells the old roof keeps are
        /// taken out of <paramref name="footprint"/>.
        /// </summary>
        private void JoinExisting(HashSet<(int X, int Y)> footprint, Dictionary<(int X, int Y), int> heights,
            Dictionary<(int X, int Y), string> fixedPieces, List<MultiTile> remove, List<(ushort Id, int X, int Y, int Z, TileLayer Layer)> add)
        {
            MultiEditorComponentList list = Context.CompList;
            var existing = new Dictionary<(int X, int Y), List<MultiTile>>();
            foreach (MultiTile tile in list.RealTiles)
            {
                if (!list.IsShown(tile) || !IsRoofPiece(tile.Id))
                {
                    continue;
                }

                if (!existing.TryGetValue((tile.X, tile.Y), out List<MultiTile> tiles))
                {
                    existing[(tile.X, tile.Y)] = tiles = new List<MultiTile>();
                }

                tiles.Add(tile);
            }

            var seen = new HashSet<(int X, int Y)>();
            var queue = new Queue<(int X, int Y)>();
            var kept = new Dictionary<(int X, int Y), MultiTile>();
            foreach ((int X, int Y) start in new List<(int X, int Y)>(footprint))
            {
                if (!existing.ContainsKey(start) || !seen.Add(start))
                {
                    continue;
                }

                var roof = new List<(int X, int Y)>();
                bool inside = true;
                queue.Enqueue(start);
                while (queue.Count > 0)
                {
                    (int x, int y) = queue.Dequeue();
                    roof.Add((x, y));
                    inside &= footprint.Contains((x, y)) || kept.ContainsKey((x, y));
                    foreach ((int dx, int dy) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) })
                    {
                        if (existing.ContainsKey((x + dx, y + dy)) && seen.Add((x + dx, y + dy)))
                        {
                            queue.Enqueue((x + dx, y + dy));
                        }
                    }
                }

                foreach ((int X, int Y) cell in roof)
                {
                    List<MultiTile> tiles = existing[cell];
                    MultiTile top = tiles[0];
                    foreach (MultiTile tile in tiles)
                    {
                        if (tile.Z > top.Z)
                        {
                            top = tile;
                        }
                    }

                    if (inside || (footprint.Contains(cell) && heights[cell] > top.Z))
                    {
                        remove.AddRange(tiles);
                        continue;
                    }

                    heights[cell] = top.Z;
                    fixedPieces.Remove(cell);
                    footprint.Remove(cell);
                    kept[cell] = top;
                }
            }

            foreach (((int x, int y), MultiTile tile) in kept)
            {
                bool beside = false;
                for (int dx = -2; dx <= 2 && !beside; ++dx)
                {
                    for (int dy = -2; dy <= 2 && !beside; ++dy)
                    {
                        beside = footprint.Contains((x + dx, y + dy));
                    }
                }

                string fallback = null;
                string column = beside ? RoofPlanner.Piece(heights, x, y, out fallback) : null;
                if (column == null)
                {
                    continue;
                }

                HouseStyle style = Style;
                string now = null;
                if (Context.HouseStyles.TryGetRoleByArt(tile.Id, out HouseStyle owner, out string slot, out _) && owner.Kind == HousePartKind.Roof)
                {
                    (style, now) = (owner, slot);
                }

                if (now == column || (style.Get(column) == 0 && now == fallback))
                {
                    continue;
                }

                ushort id = style.Get(column) != 0 ? style.Get(column) : style.Get(fallback ?? column);
                if (id != 0 && id != tile.Id)
                {
                    remove.Add(tile);
                    add.Add((id, x, y, tile.Z, tile.Layer));
                }
            }
        }

        private bool IsRoofPiece(ushort id)
        {
            return Context.HouseStyles.TryGetRole(id, out HouseStyle owner, out _)
                ? owner.Kind == HousePartKind.Roof
                : (TileData.ItemTable[id].Flags & TileFlag.Roof) != 0;
        }

        /// <summary>
        /// The walls a stroke means, among the walls standing on or just below
        /// <paramref name="z"/>. A click (one cell) takes the smallest box of connected wall cells
        /// around it. A drag takes the box whose roof it matches to within a cell on every side.
        /// False when no walls fit.
        /// </summary>
        private bool FitToWalls(int x0, int y0, int x1, int y1, int z, out WallBox box)
        {
            box = default;
            bool click = x0 == x1 && y0 == y1;
            long best = long.MaxValue;
            foreach (WallBox walls in WallBoxes(z))
            {
                long score;
                if (click)
                {
                    if (x0 < walls.X0 || x0 > walls.X1 || y0 < walls.Y0 || y0 > walls.Y1)
                    {
                        continue;
                    }

                    score = (long)(walls.X1 - walls.X0 + 1) * (walls.Y1 - walls.Y0 + 1);
                }
                else
                {
                    int dx0 = Math.Abs(x0 - (walls.X0 + 1)), dx1 = Math.Abs(x1 - (walls.X1 + 1));
                    int dy0 = Math.Abs(y0 - (walls.Y0 + 1)), dy1 = Math.Abs(y1 - (walls.Y1 + 1));
                    if (Math.Max(Math.Max(dx0, dx1), Math.Max(dy0, dy1)) > 1)
                    {
                        continue;
                    }

                    score = dx0 + dx1 + dy0 + dy1;
                }

                if (score < best)
                {
                    best = score;
                    box = walls;
                }
            }

            return best != long.MaxValue;
        }

        /// <summary>
        /// The boxes of connected wall cells of the storey that stands on or just below
        /// <paramref name="z"/> (the work plane, or the top of what the mouse points at), each with
        /// the Z its walls end at. Walls a little short of a storey count as a full one, as the
        /// official houses roof their 19 high plaster walls at 20.
        /// </summary>
        private List<WallBox> WallBoxes(int z)
        {
            MultiEditorComponentList list = Context.CompList;
            var tops = new Dictionary<(int, int), int>();
            foreach (MultiTile tile in list.RealTiles)
            {
                if (tile.Z > z || tile.Z <= z - _storey - 5 || !list.IsShown(tile) || !IsStoreyWall(tile))
                {
                    continue;
                }

                int height = tile.Height >= _storey - 3 ? Math.Max(tile.Height, _storey) : tile.Height;
                int top = tile.Z + height;
                if (!tops.TryGetValue((tile.X, tile.Y), out int known) || top > known)
                {
                    tops[(tile.X, tile.Y)] = top;
                }
            }

            var boxes = new List<WallBox>();
            var seen = new HashSet<(int, int)>();
            var queue = new Queue<(int X, int Y)>();
            var counts = new Dictionary<int, int>();
            foreach ((int, int) start in tops.Keys)
            {
                if (!seen.Add(start))
                {
                    continue;
                }

                int x0 = start.Item1, y0 = start.Item2, x1 = x0, y1 = y0;
                var cells = new HashSet<(int X, int Y)>();
                counts.Clear();
                queue.Enqueue(start);
                while (queue.Count > 0)
                {
                    (int x, int y) = queue.Dequeue();
                    cells.Add((x, y));
                    x0 = Math.Min(x0, x);
                    y0 = Math.Min(y0, y);
                    x1 = Math.Max(x1, x);
                    y1 = Math.Max(y1, y);
                    int top = tops[(x, y)];
                    counts[top] = counts.GetValueOrDefault(top) + 1;
                    foreach ((int dx, int dy) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) })
                    {
                        (int, int) next = (x + dx, y + dy);
                        if (tops.ContainsKey(next) && seen.Add(next))
                        {
                            queue.Enqueue(next);
                        }
                    }
                }

                if (cells.Count > 1)
                {
                    int common = counts.OrderByDescending(c => c.Value).ThenByDescending(c => c.Key).First().Key;
                    boxes.Add(new WallBox(x0, y0, x1, y1, common, cells));
                }
            }

            return boxes;
        }

        /// <summary>True when a wall of the storey below <paramref name="z"/> stands in the cell.</summary>
        private bool HasStoreyWall(int x, int y, int z)
        {
            foreach (MultiTile tile in Context.CompList.GetTilesAt(x, y))
            {
                if (tile.Z < z && tile.Z > z - _storey - 5 && IsStoreyWall(tile))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>A wall, window or door at least half a storey high; not a gable cap or a roof piece.</summary>
        private bool IsStoreyWall(MultiTile tile)
        {
            if (tile.Height < _storey / 2)
            {
                return false;
            }

            if (Context.HouseStyles.TryGetRole(tile.Id, out HouseStyle owner, out _))
            {
                return owner.Kind is HousePartKind.Wall or HousePartKind.Door;
            }

            TileFlag flags = TileData.ItemTable[tile.Id].Flags;
            return (flags & (TileFlag.Wall | TileFlag.Window | TileFlag.Door)) != 0 && (flags & TileFlag.Roof) == 0;
        }

        private bool IsCap(ushort id)
        {
            return Context.HouseStyles.TryGetRole(id, out HouseStyle owner, out _) &&
                   owner.Kind == HousePartKind.Misc && owner.Group.StartsWith(_capStyles, StringComparison.Ordinal);
        }

        /// <summary>A wall tile below the eaves in a gable wall row, or 0 when the row has none.</summary>
        private ushort GableWall(int row, bool alongX, int wallX0, int wallX1, int wallY0, int wallY1, int z)
        {
            MultiEditorComponentList list = Context.CompList;
            ushort wall = 0;
            int from = alongX ? wallX0 : wallY0, to = alongX ? wallX1 : wallY1;
            for (int a = from; a <= to && wall == 0; ++a)
            {
                foreach (MultiTile tile in list.GetTilesAt(alongX ? a : row, alongX ? row : a))
                {
                    if (tile.Z < z && Context.HouseStyles.TryGetRole(tile.Id, out HouseStyle owner, out _) && owner.Kind == HousePartKind.Wall)
                    {
                        wall = tile.Id;
                        break;
                    }
                }
            }

            return wall;
        }

        /// <summary>
        /// The cap for a gable over a wall: the misc.txt roof piece with the wall's tiledata name and
        /// the id nearest its own. Odd pieces close walls along X, even pieces walls along Y (as
        /// placed in the official multis). 0 when no cap matches.
        /// </summary>
        private ushort CapFor(ushort wall, bool alongX)
        {
            string name = TileData.ItemTable[wall].Name;
            ushort best = 0;
            foreach (HouseStyle style in Context.HouseStyles.Styles)
            {
                if (style.Kind != HousePartKind.Misc || !style.Group.StartsWith(_capStyles, StringComparison.Ordinal))
                {
                    continue;
                }

                for (int pair = 1; pair <= 7; pair += 2)
                {
                    ushort first = style.Get($"Piece{pair}");
                    ushort second = style.Get($"Piece{pair + 1}");
                    ushort cap = alongX ? first : second;
                    if (first == 0 || second == 0 || TileData.ItemTable[cap].Height > 4 ||
                        !string.Equals(TileData.ItemTable[cap].Name, name, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (best == 0 || Math.Abs(cap - wall) < Math.Abs(best - wall))
                    {
                        best = cap;
                    }
                }
            }

            return best;
        }
    }
}
