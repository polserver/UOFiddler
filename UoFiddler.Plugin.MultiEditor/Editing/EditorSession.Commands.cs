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
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Plugin.MultiEditor.Classes;
using UoFiddler.Plugin.MultiEditor.Tools;

namespace UoFiddler.Plugin.MultiEditor.Editing
{
    /// <summary>
    /// A floor of the multi: tiles from Floor up to (not including) Ceiling. BaseZ is the Z new
    /// tiles get on it. The lowest level also takes what lies below its base, the highest what
    /// lies above.
    /// </summary>
    internal readonly record struct EditorLevel(string Name, string ShortName, int Floor, int Ceiling, int BaseZ)
    {
        public string Description => Ceiling > sbyte.MaxValue ? $"{Name} (Z {BaseZ} and up)" : $"{Name} (Z {BaseZ} to {Ceiling - 1})";
    }

    /// <summary>How the user splits a design into levels: a name and the Z the level starts at.</summary>
    internal readonly record struct LevelDefinition(string Name, int StartZ);

    /// <summary>The ground a previewed multi stands on.</summary>
    internal enum PreviewTerrain
    {
        None,
        Grass,
        Dirt,
        Sand,
        Snow
    }

    /// <summary>
    /// The edit commands of a session: clipboard, selection edits, group transforms and levels.
    /// </summary>
    internal sealed partial class EditorSession
    {
        // Shared by every open document, so a copy in one tab pastes into another.
        private static List<ClipboardTile> _clipboard;
        private static RotationTable _rotationTable;

        private readonly List<EditorLevel> _levels = new List<EditorLevel>();
        private int _levelIndex;
        private int _firstFloorZ;
        private readonly List<LevelDefinition> _levelDefinitions = new List<LevelDefinition>();

        /// <summary>Highest Z a first floor is looked for at; above that it is a second storey.</summary>
        public const int MaxFirstFloorZ = 12;

        /// <summary>Storeys between the foundation and the roof, as in the client's house designer.</summary>
        public const int FloorCount = 4;
        private (bool Visible, int Z)? _floorBeforeLevels;

        // ---------------------------------------------------------------- clipboard

        /// <summary>
        /// Copies the selection, relative to its lowest X, Y and Z, for pasting in any document and
        /// as multi text lines to the Windows clipboard, so another editor or a text file can take it.
        /// </summary>
        public bool Copy()
        {
            if (_selection.Count == 0)
            {
                return false;
            }

            _clipboard = ToClipboard(_selection);
            try
            {
                Clipboard.SetText(ClipboardTile.Format(_clipboard));
            }
            catch (ExternalException)
            {
                // Clipboard busy in another program: the editor's own copy still works.
            }

            return true;
        }

        public void Cut()
        {
            if (Copy())
            {
                DeleteSelection();
            }
        }

        /// <summary>Starts placing the clipboard tiles with the mouse.</summary>
        public void Paste()
        {
            BeginPaste(ReadClipboard());
        }

        public bool CanPaste => _clipboard != null || ClipboardHasTiles();

        /// <summary>The selection as tiles relative to its lowest X, Y and Z (also used to save a prefab).</summary>
        public List<ClipboardTile> SelectionAsClipboard()
        {
            return _selection.Count == 0 ? null : ToClipboard(_selection);
        }

        private static List<ClipboardTile> ToClipboard(IReadOnlyCollection<MultiTile> tiles)
        {
            int ox = tiles.Min(t => t.X);
            int oy = tiles.Min(t => t.Y);
            int oz = tiles.Min(t => t.Z);
            return tiles
                .OrderBy(t => t.X).ThenBy(t => t.Y).ThenBy(t => t.Z)
                .Select(t => new ClipboardTile(t.Id, t.X - ox, t.Y - oy, t.Z - oz, t.Invisible, t.Extra))
                .ToList();
        }

        /// <summary>Tiles from the Windows clipboard when it holds multi text lines, else the editor's own copy.</summary>
        private static List<ClipboardTile> ReadClipboard()
        {
            try
            {
                if (Clipboard.ContainsText() && ClipboardTile.Parse(Clipboard.GetText()) is { } fromText)
                {
                    return fromText;
                }
            }
            catch (ExternalException)
            {
                // Clipboard busy: fall back to the editor's own copy.
            }

            return _clipboard;
        }

        private static bool ClipboardHasTiles()
        {
            try
            {
                return Clipboard.ContainsText();
            }
            catch (ExternalException)
            {
                return false;
            }
        }

        // ---------------------------------------------------------------- selection edits

        public void SelectAll()
        {
            SetSelection(CompList.RealTiles.Where(IsEditable), add: false);
        }

        public void InvertSelection()
        {
            SetSelection(CompList.RealTiles.Where(t => IsEditable(t) && !_selection.Contains(t)).ToList(), add: false);
        }

        /// <summary>Selects every editable tile for which <paramref name="match"/> holds.</summary>
        public void SelectWhere(Func<MultiTile, bool> match, bool add)
        {
            SetSelection(CompList.RealTiles.Where(t => IsEditable(t) && match(t)).ToList(), add);
        }

        /// <summary>
        /// Grows the selection to every tile of the same id connected to it through neighbouring
        /// cells (the eight around), within the editable tiles.
        /// </summary>
        public void SelectConnected()
        {
            if (_selection.Count == 0)
            {
                return;
            }

            var ids = new HashSet<ushort>(_selection.Select(t => t.Id));
            var result = new HashSet<MultiTile>(_selection);
            var queue = new Queue<MultiTile>(_selection);
            while (queue.Count > 0)
            {
                MultiTile from = queue.Dequeue();
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        foreach (MultiTile tile in CompList.GetTilesAt(from.X + dx, from.Y + dy))
                        {
                            if (tile.Id == from.Id && ids.Contains(tile.Id) && IsEditable(tile) && result.Add(tile))
                            {
                                queue.Enqueue(tile);
                            }
                        }
                    }
                }
            }

            SetSelection(result, add: false);
        }

        public void DeleteSelection()
        {
            if (_selection.Count == 0)
            {
                return;
            }

            CompList.TileRemoveBatch(_selection);
            _selection.Clear();
            SelectionChanged?.Invoke();
            AfterEdit();
        }

        /// <summary>Moves the selection by cells and Z in one undo step. Nothing moves when a tile would leave the multi.</summary>
        public void NudgeSelection(int dx, int dy, int dz)
        {
            if (_selection.Count == 0)
            {
                return;
            }

            if (CompList.TileMoveBatch(_selection.ToList(), dx, dy, dz, "Nudge Selection"))
            {
                AfterEdit(dz > 0 ? _selection.Max(t => t.Z) : null);
                SelectionChanged?.Invoke();
            }
        }

        public void ShiftSelectionZ(int delta)
        {
            if (_selection.Count == 0)
            {
                return;
            }

            CompList.TileZMod(_selection, delta);
            AfterEdit(delta > 0 ? _selection.Max(t => t.Z) : null);
            SelectionChanged?.Invoke();
        }

        /// <summary>Gives every selected tile with one of the ids a new id, in one undo step. Returns the number changed.</summary>
        public int ReplaceTileId(ushort from, ushort to, bool selectionOnly)
        {
            IEnumerable<MultiTile> scope = selectionOnly ? _selection : CompList.RealTiles;
            List<(MultiTile, ushort)> changes = scope.Where(t => t.Id == from).Select(t => (t, to)).ToList();
            if (changes.Count == 0 || from == to)
            {
                return 0;
            }

            CompList.TileSetIds(changes, $"Replace 0x{from:X4} with 0x{to:X4}");
            AfterEdit();
            SelectionChanged?.Invoke();
            return changes.Count;
        }

        /// <summary>
        /// Rotates or mirrors the selection in place, or the whole design (swapping width and
        /// height on a quarter turn) when nothing is selected. Returns a message when it cannot.
        /// </summary>
        public string TransformGroup(GroupTransformKind kind)
        {
            bool whole = _selection.Count == 0;
            List<MultiTile> group = whole ? CompList.RealTiles.ToList() : _selection.ToList();
            if (group.Count == 0)
            {
                return null;
            }

            _rotationTable ??= RotationTable.LoadDefault();
            var add = GroupTransform.Apply(group, HouseStyles, _rotationTable, kind);
            int width = CompList.Width;
            int height = CompList.Height;

            if (whole)
            {
                if (GroupTransform.SwapsAxes(kind))
                {
                    (width, height) = (height, width);
                }

                // New wall posts may reach a cell past the old edge: keep everything inside.
                int shiftX = Math.Max(0, -add.Min(t => t.X));
                int shiftY = Math.Max(0, -add.Min(t => t.Y));
                add = add.Select(t => (t.Id, t.X + shiftX, t.Y + shiftY, t.Z, t.Invisible, t.Extra)).ToList();
                width = Math.Max(width, add.Max(t => t.X) + 1);
                height = Math.Max(height, add.Max(t => t.Y) + 1);
            }
            else if (add.Any(t => !CompList.InBounds(t.X, t.Y)))
            {
                return "The turned selection would not fit inside the multi. Move it away from the edge first.";
            }

            string name = kind switch
            {
                GroupTransformKind.RotateLeft => "Rotate Left",
                GroupTransformKind.RotateRight => "Rotate Right",
                GroupTransformKind.MirrorX => "Mirror X",
                _ => "Mirror Y"
            };

            List<MultiTile> added = CompList.ReplaceTiles(group, add, name, width, height);
            if (!whole)
            {
                _selection.Clear();
                foreach (MultiTile tile in added)
                {
                    _selection.Add(tile);
                }

                SelectionChanged?.Invoke();
            }

            AfterEdit();
            return null;
        }

        /// <summary>
        /// Swaps every selected piece of the chosen house style's kind to the same column of that
        /// style (South2 stays South2, F7 stays F7). Windows the style lacks become plain walls.
        /// </summary>
        public void SwapSelectionToHouseStyle()
        {
            HouseStyle style = HouseStyle;
            if (style == null || _selection.Count == 0)
            {
                return;
            }

            var changes = new List<(MultiTile, ushort)>();
            foreach (MultiTile tile in _selection)
            {
                if (!HouseStyles.TryGetRole(tile.Id, out HouseStyle owner, out string slot) || owner.Kind != style.Kind || owner == style)
                {
                    continue;
                }

                ushort id = style.Get(slot);
                if (id == 0)
                {
                    string fallback = slot switch
                    {
                        "WindowS" or "AltWindowS" or "SecondAltWindowS" => "South2",
                        "WindowE" or "AltWindowE" or "SecondAltWindowE" => "East2",
                        _ when style.Kind == HousePartKind.Floor => "F1",
                        _ => null
                    };
                    id = fallback != null ? style.Get(fallback) : (ushort)0;

                    // Floor styles are one colour of a row each and need not hold F1: any piece will do.
                    if (id == 0 && style.Kind == HousePartKind.Floor)
                    {
                        id = style.IconId;
                    }
                }

                if (id != 0 && id != tile.Id)
                {
                    changes.Add((tile, id));
                }
            }

            if (changes.Count == 0)
            {
                return;
            }

            CompList.TileSetIds(changes, $"Swap to {style.Name}");
            AfterEdit();
        }

        public void Resize(int width, int height)
        {
            CompList.Resize(width, height);
            AfterEdit();
        }

        // ---------------------------------------------------------------- preview and reference

        private bool _previewMode;
        private MultiEditorComponentList _reference;
        private (int X, int Y, int Z) _referenceOffset;

        /// <summary>
        /// Shows the multi as the client would: no editor marks, translucent and animated tiles
        /// live. Nothing can be edited while it is on.
        /// </summary>
        public bool PreviewMode
        {
            get => _previewMode;
            set
            {
                if (_previewMode == value)
                {
                    return;
                }

                CancelTool();
                _previewMode = value;
                OnViewChanged();
            }
        }

        private PreviewTerrain _previewTerrain = PreviewTerrain.Grass;

        /// <summary>Land drawn under and around the multi in preview mode, to judge it as placed in the world.</summary>
        public PreviewTerrain PreviewTerrain
        {
            get => _previewTerrain;
            set => SetView(ref _previewTerrain, value);
        }

        /// <summary>Another multi drawn faintly under this one, to build against. Not editable, not saved.</summary>
        public MultiEditorComponentList Reference => _reference;

        /// <summary>Where the reference sits, in cells and Z, relative to this multi.</summary>
        public (int X, int Y, int Z) ReferenceOffset
        {
            get => _referenceOffset;
            set => SetView(ref _referenceOffset, value);
        }

        public void SetReference(MultiEditorComponentList reference)
        {
            _reference = reference;
            _referenceOffset = (0, 0, 0);
            ViewChanged?.Invoke();
            Invalidated?.Invoke();
        }

        // ---------------------------------------------------------------- floor plane

        private bool _showFloorPlane;

        /// <summary>
        /// Shows the footprint of the multi at the base of the floor being built, as a fixed
        /// reference under the tiles. Unlike the work plane it does not move with the placement Z
        /// and takes no part in editing.
        /// </summary>
        public bool ShowFloorPlane
        {
            get => _showFloorPlane;
            set => SetView(ref _showFloorPlane, value);
        }

        /// <summary>Z of the floor plane: the active floor's base, or the first floor with all floors shown.</summary>
        public int FloorPlaneZ => _levelIndex > 0 ? _levels[_levelIndex - 1].BaseZ : _firstFloorZ;

        // ---------------------------------------------------------------- plan view

        private bool _planMode;

        /// <summary>
        /// Top-down view: every cell a square in the colour of its top tile, walls and doors as
        /// lines on the edges they stand on. The same tools work on it.
        /// </summary>
        public bool PlanMode
        {
            get => _planMode;
            set
            {
                if (_planMode == value)
                {
                    return;
                }

                CancelTool();
                _planMode = value;
                OnViewChanged();
            }
        }

        /// <summary>Lowest Z you work on: the work plane when it is shown.</summary>
        public int EditMinZ => _floorVisible ? _floorZ : int.MinValue;

        public PlanView Plan => new PlanView(CompList, PlanShapeOf);

        /// <summary>
        /// How a tile shows in the plan view, from the client's house tables: walls and doors as
        /// lines on the edges they stand on, posts at the corner, everything else as the cell fill.
        /// </summary>
        public PlanShape PlanShapeOf(ushort id)
        {
            if (HouseStyles == null || !HouseStyles.TryGetRole(id, out HouseStyle style, out string slot))
            {
                return PlanShape.Fill;
            }

            if (style.Kind == HousePartKind.Door)
            {
                return PlanShape.Door | (WallPlanner.DoorAlongX(slot) ? PlanShape.South : PlanShape.East);
            }

            if (style.Kind != HousePartKind.Wall)
            {
                return PlanShape.Fill;
            }

            return slot switch
            {
                "Corner" => PlanShape.South | PlanShape.East,
                "Post" => PlanShape.Post,
                _ when slot.StartsWith("South", StringComparison.Ordinal) || slot.EndsWith('S') => PlanShape.South,
                _ when slot.StartsWith("East", StringComparison.Ordinal) || slot.EndsWith('E') => PlanShape.East,
                _ => PlanShape.Fill
            };
        }

        /// <summary>World point a tile is picked by in area selections: the middle of its cell.</summary>
        public PointF TileAnchor(MultiTile tile)
        {
            if (_planMode)
            {
                Point origin = PlanView.CellOrigin(tile.X, tile.Y);
                return new PointF(origin.X + (PlanView.Cell / 2f), origin.Y + (PlanView.Cell / 2f));
            }

            PointF corner = CellCorner(tile.X, tile.Y, tile.Z);
            return new PointF(corner.X, corner.Y + 22);
        }

        // ---------------------------------------------------------------- levels

        /// <summary>The floors of the multi, bottom to top.</summary>
        public IReadOnlyList<EditorLevel> Levels => _levels;

        /// <summary>0 shows all levels; n works on <c>Levels[n - 1]</c>.</summary>
        public int LevelIndex
        {
            get => _levelIndex;
            set
            {
                value = Math.Clamp(value, 0, _levels.Count);
                if (value == _levelIndex)
                {
                    return;
                }

                _levelIndex = value;
                ApplyLevel();
            }
        }

        public void StepLevel(int step)
        {
            if (_levels.Count == 0)
            {
                return;
            }

            // From "all levels", stepping up starts at the bottom level.
            LevelIndex = _levelIndex == 0 && step > 0 ? 1 : _levelIndex + step;
        }

        /// <summary>
        /// Z of the first floor. 0 for a multi built from the ground; above 0 the multi stands on a
        /// foundation, which is then a level of its own below the floors. Custom house foundations
        /// and most classic houses use 7, some classic houses 4, 5, 6 or 8. Found from the tiles on
        /// load. Setting it puts the levels back to the standard split from that Z.
        /// </summary>
        public int FirstFloorZ
        {
            get => _firstFloorZ;
            set
            {
                value = Math.Clamp(value, 0, MaxFirstFloorZ);
                if (_firstFloorZ == value && !CustomLevels)
                {
                    return;
                }

                _firstFloorZ = value;
                ApplyLevelDefinitions(StandardLevels(value), custom: false);
            }
        }

        public bool HasFoundation => _firstFloorZ > 0;

        /// <summary>True when the user set the levels by hand, so they are no longer the standard split.</summary>
        public bool CustomLevels { get; private set; }

        /// <summary>The levels as defined: name and starting Z, lowest first.</summary>
        public IReadOnlyList<LevelDefinition> LevelDefinitions => _levelDefinitions;

        /// <summary>
        /// The standard split: an optional foundation below the first floor, four floors 20 high,
        /// and a roof level above them. These are the client's house designer numbers.
        /// </summary>
        public static List<LevelDefinition> StandardLevels(int firstFloorZ)
        {
            var levels = new List<LevelDefinition>();
            if (firstFloorZ > 0)
            {
                levels.Add(new LevelDefinition("Foundation", 0));
            }

            for (int n = 1; n <= FloorCount; n++)
            {
                levels.Add(new LevelDefinition($"Floor {n}", firstFloorZ + ((n - 1) * 20)));
            }

            levels.Add(new LevelDefinition("Roof", firstFloorZ + (FloorCount * 20)));
            return levels;
        }

        /// <summary>
        /// A split fitted to this design: floors 20 high from the first floor up to where its roof
        /// tiles begin, then the roof. Without roof tiles it is the standard split.
        /// </summary>
        public List<LevelDefinition> FittedLevels()
        {
            int roof = int.MaxValue;
            foreach (MultiTile tile in CompList.RealTiles)
            {
                if ((TileData.ItemTable[tile.Id].Flags & TileFlag.Roof) != 0 && tile.Z > _firstFloorZ)
                {
                    roof = Math.Min(roof, tile.Z);
                }
            }

            if (roof == int.MaxValue)
            {
                return StandardLevels(_firstFloorZ);
            }

            var levels = new List<LevelDefinition>();
            if (_firstFloorZ > 0)
            {
                levels.Add(new LevelDefinition("Foundation", 0));
            }

            // A floor needs most of a storey below the roof to count; a roof starting a few Z into
            // the next band still belongs over the floor below.
            int n = 1;
            for (int floor = _firstFloorZ; floor == _firstFloorZ || floor + 10 <= roof; floor += 20)
            {
                levels.Add(new LevelDefinition($"Floor {n++}", floor));
            }

            levels.Add(new LevelDefinition("Roof", Math.Max(roof, levels[^1].StartZ + 1)));
            return levels;
        }

        /// <summary>Sets the levels by hand. Starts are kept within a multi's Z range and must differ; at least one level stays.</summary>
        public void SetLevelDefinitions(IEnumerable<LevelDefinition> levels)
        {
            ApplyLevelDefinitions(levels, custom: true);
        }

        private void ApplyLevelDefinitions(IEnumerable<LevelDefinition> levels, bool custom)
        {
            var cleaned = new List<LevelDefinition>();
            foreach (LevelDefinition level in levels.OrderBy(l => l.StartZ))
            {
                int start = Math.Clamp(level.StartZ, sbyte.MinValue, sbyte.MaxValue);
                string name = string.IsNullOrWhiteSpace(level.Name) ? $"Level {cleaned.Count + 1}" : level.Name.Trim();
                if (cleaned.Count == 0 || cleaned[^1].StartZ != start)
                {
                    cleaned.Add(new LevelDefinition(name, start));
                }
            }

            if (cleaned.Count == 0)
            {
                cleaned = StandardLevels(_firstFloorZ);
                custom = false;
            }

            _levelDefinitions.Clear();
            _levelDefinitions.AddRange(cleaned);
            CustomLevels = custom;
            RebuildLevels();
            if (_levelIndex > 0)
            {
                ApplyLevel();
            }
            else
            {
                OnViewChanged();
            }
        }

        /// <summary>
        /// The first floor is the lowest Z, up to <see cref="MaxFirstFloorZ"/>, that holds a real
        /// share of the flat walkable tiles. One stray flat tile low down does not make a floor.
        /// </summary>
        private int DetectFirstFloorZ()
        {
            var flat = new SortedDictionary<int, int>();
            foreach (MultiTile tile in CompList.RealTiles)
            {
                if (tile.Height == 0 && (TileData.ItemTable[tile.Id].Flags & TileFlag.Surface) != 0)
                {
                    flat[tile.Z] = flat.GetValueOrDefault(tile.Z) + 1;
                }
            }

            if (flat.Count == 0)
            {
                return 0;
            }

            int needed = Math.Max(4, flat.Values.Max() / 4);
            foreach ((int z, int count) in flat)
            {
                if (z >= 0 && z <= MaxFirstFloorZ && count >= needed)
                {
                    return z;
                }
            }

            return 0;
        }

        /// <summary>A short label for a level's button: the number of "Floor 2", else the start of the name.</summary>
        private static string ShortNameOf(string name)
        {
            if (name.StartsWith("Floor ", StringComparison.OrdinalIgnoreCase) && name.Length > 6)
            {
                return name.Substring(6);
            }

            return name.Equals("Foundation", StringComparison.OrdinalIgnoreCase) ? "Fnd" : name.Length <= 5 ? name : name.Substring(0, 5);
        }

        /// <summary>
        /// Builds the levels from their definitions. Every level is there whether it holds tiles or
        /// not, so an empty floor can be chosen and built on.
        /// </summary>
        public void RebuildLevels()
        {
            var levels = new List<EditorLevel>();
            if (HasDocument)
            {
                if (_levelDefinitions.Count == 0)
                {
                    _levelDefinitions.AddRange(StandardLevels(_firstFloorZ));
                }

                for (int i = 0; i < _levelDefinitions.Count; i++)
                {
                    LevelDefinition level = _levelDefinitions[i];
                    int from = i == 0 ? sbyte.MinValue : level.StartZ;
                    int to = i + 1 < _levelDefinitions.Count ? _levelDefinitions[i + 1].StartZ : sbyte.MaxValue + 1;
                    levels.Add(new EditorLevel(level.Name, ShortNameOf(level.Name), from, to, level.StartZ));
                }
            }

            if (levels.SequenceEqual(_levels))
            {
                return;
            }

            _levels.Clear();
            _levels.AddRange(levels);
            if (_levelIndex > _levels.Count)
            {
                _levelIndex = 0;
                ApplyLevel();
            }
            else if (_levelIndex > 0)
            {
                // Same index, possibly a different Z band.
                _activeLevel = (_levels[_levelIndex - 1].Floor, _levels[_levelIndex - 1].Ceiling);
            }
        }

        /// <summary>Puts the work plane on the chosen level, so drawing lands there.</summary>
        private void ApplyLevel()
        {
            if (_levelIndex > 0)
            {
                EditorLevel level = _levels[_levelIndex - 1];

                // Entering level mode: remember the plane as the user had it.
                _floorBeforeLevels ??= (_floorVisible, _floorZ);
                _activeLevel = (level.Floor, level.Ceiling);
                _floorVisible = true;
                _floorZ = level.BaseZ;
                CompList.SetFloorZ(_floorZ);
            }
            else
            {
                _activeLevel = null;
                if (_floorBeforeLevels is { } before)
                {
                    _floorBeforeLevels = null;
                    _floorVisible = before.Visible;
                    _floorZ = before.Z;
                    CompList.SetFloorZ(_floorZ);
                }
            }

            OnViewChanged();
        }
    }
}
