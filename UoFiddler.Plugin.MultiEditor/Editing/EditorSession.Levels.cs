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
using System.Linq;
using Ultima;
using UoFiddler.Plugin.MultiEditor.Model;

namespace UoFiddler.Plugin.MultiEditor.Editing
{
    internal sealed partial class EditorSession
    {
        private readonly List<EditorLevel> _levels = new List<EditorLevel>();
        private int _levelIndex;
        private int _firstFloorZ;
        private readonly List<LevelDefinition> _levelDefinitions = new List<LevelDefinition>();

        /// <summary>Highest Z a first floor is looked for at; above that it is a second storey.</summary>
        public const int MaxFirstFloorZ = 12;

        /// <summary>Storeys between the foundation and the roof, as in the client's house designer.</summary>
        public const int FloorCount = 4;

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
        /// A design without a floor has it where most of its walls stand.
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

            if (flat.Count > 0)
            {
                int needed = Math.Max(4, flat.Values.Max() / 4);
                foreach ((int z, int count) in flat)
                {
                    if (z >= 0 && z <= MaxFirstFloorZ && count >= needed)
                    {
                        return z;
                    }
                }
            }

            // No floor laid (a building lifted off the map stands on terrain): where its walls stand.
            var walls = new Dictionary<int, int>();
            foreach (MultiTile tile in CompList.RealTiles)
            {
                if (tile.Z >= 0 && tile.Z <= MaxFirstFloorZ && tile.Height >= 10 &&
                    (TileData.ItemTable[tile.Id].Flags & TileFlag.Wall) != 0)
                {
                    walls[tile.Z] = walls.GetValueOrDefault(tile.Z) + 1;
                }
            }

            int best = 0, most = 3;
            foreach ((int z, int count) in walls)
            {
                if (count > most)
                {
                    (best, most) = (z, count);
                }
            }

            return best;
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

        /// <summary>
        /// Limits the view to the chosen level and puts the work plane on its base, where drawing
        /// then lands (<see cref="WorkZ"/>).
        /// </summary>
        private void ApplyLevel()
        {
            _workPlaneOffset = 0;
            if (_levelIndex > 0)
            {
                EditorLevel level = _levels[_levelIndex - 1];
                _activeLevel = (level.Floor, level.Ceiling);
            }
            else
            {
                _activeLevel = null;
            }

            OnViewChanged();
        }
    }
}
