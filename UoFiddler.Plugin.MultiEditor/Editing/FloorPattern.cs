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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Plugin.MultiEditor.Classes;
using UoFiddler.Plugin.MultiEditor.Tools;

namespace UoFiddler.Plugin.MultiEditor.Editing
{
    /// <summary>
    /// A floor with a border: four corner pieces, a run of pieces for each side and a block of
    /// fill tiles for the inside. Laid over a rectangle of any size, the sides and the fill repeat.
    /// Sides are named by the world: north is the low Y row, west the low X column.
    /// </summary>
    internal sealed class FloorPattern
    {
        public string Name { get; set; }

        /// <summary>Shipped with the editor, not saved to or deleted from the user's file.</summary>
        public bool BuiltIn { get; set; }

        public ushort NorthWest { get; set; }

        public ushort NorthEast { get; set; }

        public ushort SouthWest { get; set; }

        public ushort SouthEast { get; set; }

        /// <summary>Pieces along each side between the corners, in order of rising X (north, south) or Y (west, east).</summary>
        public List<ushort> North { get; set; } = new List<ushort>();

        public List<ushort> South { get; set; } = new List<ushort>();

        public List<ushort> West { get; set; } = new List<ushort>();

        public List<ushort> East { get; set; } = new List<ushort>();

        /// <summary>The inside as a block of tiles, row by row (Y major), <see cref="FillWidth"/> wide.</summary>
        public List<ushort> Fill { get; set; } = new List<ushort>();

        public int FillWidth { get; set; } = 1;

        public int FillHeight => FillWidth > 0 ? Math.Max(1, Fill.Count / FillWidth) : 1;

        /// <summary>A tile to show for the pattern in lists.</summary>
        public ushort IconId => NorthWest != 0 ? NorthWest : Fill.FirstOrDefault();

        /// <summary>Every distinct tile of the pattern, corners first.</summary>
        public IEnumerable<ushort> Pieces()
        {
            var seen = new HashSet<ushort>();
            foreach (ushort id in new[] { NorthWest, NorthEast, SouthWest, SouthEast }.Concat(North).Concat(East).Concat(South).Concat(West).Concat(Fill))
            {
                if (id != 0 && seen.Add(id))
                {
                    yield return id;
                }
            }
        }

        /// <summary>
        /// The tile for each cell of a rectangle. A rectangle one cell wide or high has no room for
        /// a border and gets fill only.
        /// </summary>
        public IEnumerable<(ushort Id, int X, int Y)> Layout(int x0, int y0, int x1, int y1)
        {
            bool border = x1 > x0 && y1 > y0;
            for (int x = x0; x <= x1; x++)
            {
                for (int y = y0; y <= y1; y++)
                {
                    ushort id = 0;
                    if (border)
                    {
                        bool west = x == x0, east = x == x1, north = y == y0, south = y == y1;
                        id = north && west ? NorthWest
                            : north && east ? NorthEast
                            : south && west ? SouthWest
                            : south && east ? SouthEast
                            : north ? At(North, x - x0 - 1)
                            : south ? At(South, x - x0 - 1)
                            : west ? At(West, y - y0 - 1)
                            : east ? At(East, y - y0 - 1)
                            : (ushort)0;
                    }

                    if (id == 0 && Fill.Count > 0)
                    {
                        // The fill block starts at the first inner cell, so it sits the same way at any size.
                        int fx = Mod(x - x0 - (border ? 1 : 0), FillWidth);
                        int fy = Mod(y - y0 - (border ? 1 : 0), FillHeight);
                        id = Fill[Math.Min(Fill.Count - 1, (fy * FillWidth) + fx)];
                    }

                    if (id != 0)
                    {
                        yield return (id, x, y);
                    }
                }
            }
        }

        private static ushort At(List<ushort> run, int index) => run.Count == 0 ? (ushort)0 : run[Mod(index, run.Count)];

        private static int Mod(int value, int by) => by <= 0 ? 0 : ((value % by) + by) % by;

        /// <summary>
        /// Learns a pattern from a floor laid out by hand: the selected tiles must fill a rectangle
        /// of at least 3 by 3 cells, one tile per cell (the highest selected one counts). Returns
        /// null with a reason when they do not.
        /// </summary>
        public static FloorPattern Learn(IReadOnlyCollection<MultiTile> selection, string name, out string problem)
        {
            problem = null;
            if (selection.Count == 0)
            {
                problem = "Select the floor tiles of one bordered rectangle first.";
                return null;
            }

            int x0 = selection.Min(t => t.X), x1 = selection.Max(t => t.X);
            int y0 = selection.Min(t => t.Y), y1 = selection.Max(t => t.Y);
            if (x1 - x0 < 2 || y1 - y0 < 2)
            {
                problem = "The selection must be at least 3 by 3 cells: corners, sides and something inside.";
                return null;
            }

            var top = new Dictionary<(int, int), MultiTile>();
            foreach (MultiTile tile in selection)
            {
                if (!top.TryGetValue((tile.X, tile.Y), out MultiTile have) || tile.Z > have.Z)
                {
                    top[(tile.X, tile.Y)] = tile;
                }
            }

            for (int x = x0; x <= x1; x++)
            {
                for (int y = y0; y <= y1; y++)
                {
                    if (!top.ContainsKey((x, y)))
                    {
                        problem = $"The selection is not a full rectangle: cell {x}, {y} has no selected tile.";
                        return null;
                    }
                }
            }

            ushort Id(int x, int y) => top[(x, y)].Id;
            var pattern = new FloorPattern
            {
                Name = name,
                NorthWest = Id(x0, y0),
                NorthEast = Id(x1, y0),
                SouthWest = Id(x0, y1),
                SouthEast = Id(x1, y1),
                FillWidth = x1 - x0 - 1
            };

            for (int x = x0 + 1; x < x1; x++)
            {
                pattern.North.Add(Id(x, y0));
                pattern.South.Add(Id(x, y1));
            }

            for (int y = y0 + 1; y < y1; y++)
            {
                pattern.West.Add(Id(x0, y));
                pattern.East.Add(Id(x1, y));
            }

            for (int y = y0 + 1; y < y1; y++)
            {
                for (int x = x0 + 1; x < x1; x++)
                {
                    pattern.Fill.Add(Id(x, y));
                }
            }

            pattern.Shrink();
            return pattern;
        }

        /// <summary>Drops repeats: a side that is one piece over and over is stored as that piece, likewise the fill block.</summary>
        private void Shrink()
        {
            North = Period(North);
            South = Period(South);
            West = Period(West);
            East = Period(East);

            int width = FillWidth, height = FillHeight;
            int pw = width, ph = height;
            for (int w = 1; w <= width; w++)
            {
                if (Enumerable.Range(0, Fill.Count).All(i => Fill[i] == Fill[(i / width * width) + (i % width % w)]))
                {
                    pw = w;
                    break;
                }
            }

            for (int h = 1; h <= height; h++)
            {
                if (Enumerable.Range(0, Fill.Count).All(i => Fill[i] == Fill[(i / width % h * width) + (i % width)]))
                {
                    ph = h;
                    break;
                }
            }

            var block = new List<ushort>();
            for (int y = 0; y < ph; y++)
            {
                for (int x = 0; x < pw; x++)
                {
                    block.Add(Fill[(y * width) + x]);
                }
            }

            Fill = block;
            FillWidth = pw;
        }

        private static List<ushort> Period(List<ushort> run)
        {
            for (int length = 1; length < run.Count; length++)
            {
                if (Enumerable.Range(0, run.Count).All(i => run[i] == run[i % length]))
                {
                    return run.GetRange(0, length);
                }
            }

            return run;
        }
    }

    /// <summary>
    /// The floor patterns: a few shipped ones for tile sets whose border pieces are known, and the
    /// ones the user saved (MultiEditorFloorPatterns.xml next to the other multi editor files).
    /// </summary>
    internal static class FloorPatterns
    {
        private static List<FloorPattern> _user;

        public static string FilePath => Path.Combine(Options.AppDataPath ?? string.Empty, "plugins", "MultiEditorFloorPatterns.xml");

        /// <summary>User patterns first (newest last), then the shipped ones this client has the art for.</summary>
        public static List<FloorPattern> All()
        {
            _user ??= LoadUser();
            var all = new List<FloorPattern>(_user);
            foreach (FloorPattern pattern in BuiltIn())
            {
                if (pattern.Pieces().All(id => Art.IsValidStatic(id)) && all.All(p => p.Name != pattern.Name))
                {
                    all.Add(pattern);
                }
            }

            return all;
        }

        public static void Save(FloorPattern pattern)
        {
            _user ??= LoadUser();
            _user.RemoveAll(p => string.Equals(p.Name, pattern.Name, StringComparison.OrdinalIgnoreCase));
            _user.Add(pattern);
            SaveUser();
        }

        public static void Delete(FloorPattern pattern)
        {
            _user ??= LoadUser();
            if (_user.Remove(pattern))
            {
                SaveUser();
            }
        }

        /// <summary>Forgets what was read, after the profile or client changed.</summary>
        public static void Reset()
        {
            _user = null;
        }

        /// <summary>
        /// Sets read off the client's art: the piece with its band on a side goes on that side of
        /// the floor. Checked by eye against the 7.0.114.4 art.
        /// </summary>
        private static IEnumerable<FloorPattern> BuiltIn()
        {
            yield return new FloorPattern
            {
                Name = "Gargish Green, bordered",
                BuiltIn = true,
                NorthWest = 16821, NorthEast = 16823, SouthWest = 16826, SouthEast = 16828,
                North = { 16822 }, West = { 16824 }, East = { 16825 }, South = { 16827 },
                Fill = { 16815, 16816, 16817, 16818 },
                FillWidth = 2
            };
            yield return new FloorPattern
            {
                Name = "Gargish Motif, bordered",
                BuiltIn = true,
                NorthWest = 16860, NorthEast = 16862, SouthWest = 16866, SouthEast = 16868,
                North = { 16861 }, West = { 16863 }, East = { 16865 }, South = { 16867 },
                Fill = { 16864 },
                FillWidth = 1
            };
        }

        private static List<FloorPattern> LoadUser()
        {
            var result = new List<FloorPattern>();
            try
            {
                if (!File.Exists(FilePath))
                {
                    return result;
                }

                var dom = new XmlDocument();
                dom.Load(FilePath);
                foreach (XmlElement node in dom.SelectNodes("/FloorPatterns/Pattern"))
                {
                    var pattern = new FloorPattern
                    {
                        Name = node.GetAttribute("name"),
                        NorthWest = One(node, "nw"),
                        NorthEast = One(node, "ne"),
                        SouthWest = One(node, "sw"),
                        SouthEast = One(node, "se"),
                        North = Many(node, "north"),
                        South = Many(node, "south"),
                        West = Many(node, "west"),
                        East = Many(node, "east"),
                        Fill = Many(node, "fill"),
                        FillWidth = Math.Max(1, (int)One(node, "fillWidth"))
                    };
                    if (!string.IsNullOrWhiteSpace(pattern.Name) && pattern.Pieces().Any())
                    {
                        result.Add(pattern);
                    }
                }
            }
            catch (Exception ex) when (ex is IOException or XmlException or UnauthorizedAccessException)
            {
                // A damaged file must not keep the editor from opening; the patterns in it are skipped.
            }

            return result;
        }

        private static void SaveUser()
        {
            try
            {
                var dom = new XmlDocument();
                XmlElement root = dom.CreateElement("FloorPatterns");
                dom.AppendChild(root);
                foreach (FloorPattern pattern in _user)
                {
                    XmlElement node = dom.CreateElement("Pattern");
                    node.SetAttribute("name", pattern.Name);
                    node.SetAttribute("nw", Text(pattern.NorthWest));
                    node.SetAttribute("ne", Text(pattern.NorthEast));
                    node.SetAttribute("sw", Text(pattern.SouthWest));
                    node.SetAttribute("se", Text(pattern.SouthEast));
                    node.SetAttribute("north", Text(pattern.North));
                    node.SetAttribute("south", Text(pattern.South));
                    node.SetAttribute("west", Text(pattern.West));
                    node.SetAttribute("east", Text(pattern.East));
                    node.SetAttribute("fillWidth", pattern.FillWidth.ToString(CultureInfo.InvariantCulture));
                    node.SetAttribute("fill", Text(pattern.Fill));
                    root.AppendChild(node);
                }

                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                dom.Save(FilePath);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                MessageBox.Show("The floor patterns could not be saved: " + ex.Message, "Multi Editor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static string Text(ushort id) => "0x" + id.ToString("X4", CultureInfo.InvariantCulture);

        private static string Text(IEnumerable<ushort> ids) => string.Join(",", ids.Select(Text));

        private static ushort One(XmlElement node, string attribute) => Many(node, attribute).FirstOrDefault();

        private static List<ushort> Many(XmlElement node, string attribute)
        {
            var result = new List<ushort>();
            foreach (string part in node.GetAttribute(attribute).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                bool hex = part.StartsWith("0x", StringComparison.OrdinalIgnoreCase);
                if (ushort.TryParse(hex ? part.AsSpan(2) : part.AsSpan(), hex ? NumberStyles.HexNumber : NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out ushort id))
                {
                    result.Add(id);
                }
            }

            return result;
        }
    }

    /// <summary>
    /// Lays the session's floor pattern over a dragged rectangle on the work plane (or on top of
    /// what the mouse points at). Flat tiles already at that Z in those cells are replaced.
    /// </summary>
    internal sealed class FloorPatternTool : EditorTool
    {
        private readonly EditorSession _session;
        private bool _dragging;
        private int _z;
        private int _startX;
        private int _startY;
        private int _endX;
        private int _endY;

        public FloorPatternTool(EditorSession session) : base(session)
        {
            _session = session;
        }

        public override bool IsDragging => _dragging;

        public override bool TracksCursorCell => true;

        public override void OnMouseDown(MouseEventArgs e)
        {
            if (_session.FloorPattern == null)
            {
                return;
            }

            BaseCell(out _startX, out _startY, out _z);
            _endX = _startX;
            _endY = _startY;
            _dragging = true;
        }

        public override void OnMouseMove(MouseEventArgs e)
        {
            if (_dragging)
            {
                Context.PlaneCell(Context.MouseLoc, _z, out _endX, out _endY);
            }
        }

        public override void OnMouseUp(MouseEventArgs e)
        {
            if (!_dragging)
            {
                return;
            }

            _dragging = false;
            Context.PlaneCell(Context.MouseLoc, _z, out _endX, out _endY);
            var (remove, add) = Plan(_startX, _startY, _endX, _endY, _z);
            if (remove.Count > 0 || add.Count > 0)
            {
                Context.CompList.ReplaceTiles(remove, add, "Floor pattern");
                Context.AfterEdit(_z);
            }
        }

        public override void Cancel()
        {
            _dragging = false;
        }

        public override void PaintOverlay(IToolOverlay overlay)
        {
            if (_session.FloorPattern == null)
            {
                return;
            }

            int sx, sy, ex, ey, z;
            if (_dragging)
            {
                (sx, sy, ex, ey, z) = (_startX, _startY, _endX, _endY, _z);
            }
            else
            {
                BaseCell(out sx, out sy, out z);
                (ex, ey) = (sx, sy);
            }

            var (remove, add) = Plan(sx, sy, ex, ey, z);
            foreach (MultiTile tile in remove)
            {
                overlay.Highlight(tile, TileHighlight.Remove);
            }

            foreach ((ushort id, int x, int y, int tz) in add)
            {
                overlay.Ghost(id, x, y, tz);
            }
        }

        private void BaseCell(out int x, out int y, out int z)
        {
            Context.CellAt(Context.MouseLoc, out x, out y, out z);
            if (Context.FloorVisible)
            {
                z = Context.FloorZ;
                Context.PlaneCell(Context.MouseLoc, z, out x, out y);
            }
        }

        private (List<MultiTile> Remove, List<(ushort Id, int X, int Y, int Z)> Add) Plan(int startX, int startY, int endX, int endY, int z)
        {
            var remove = new List<MultiTile>();
            var add = new List<(ushort, int, int, int)>();
            MultiEditorComponentList list = Context.CompList;
            int x0 = Math.Min(startX, endX), x1 = Math.Max(startX, endX);
            int y0 = Math.Min(startY, endY), y1 = Math.Max(startY, endY);

            foreach ((ushort id, int x, int y) in _session.FloorPattern.Layout(x0, y0, x1, y1))
            {
                if (!list.InBounds(x, y))
                {
                    continue;
                }

                bool there = false;
                foreach (MultiTile tile in list.GetTilesAt(x, y))
                {
                    if (tile.Z != z || tile.Height != 0)
                    {
                        continue;
                    }

                    if (tile.Id == id)
                    {
                        there = true;
                    }
                    else
                    {
                        remove.Add(tile);
                    }
                }

                if (!there)
                {
                    add.Add((id, x, y, z));
                }
            }

            return (remove, add);
        }
    }
}
