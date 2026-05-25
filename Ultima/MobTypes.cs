/***************************************************************************
 *
 * $Author: UOFiddler Contributors
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

namespace Ultima
{
    public enum MobType
    {
        Monster = 0,
        Sea = 1,
        Animal = 2,
        Human = 3,
        Equipment = 4
    }

    /// <summary>
    /// Single source of truth for <c>mobtypes.txt</c>.
    ///
    /// The client uses this file (when present) to decide a body's category
    /// (MONSTER / ANIMAL / SEA_MONSTER / HUMAN / EQUIPMENT) and per-body
    /// optional-action flags. Without it, UOFiddler falls back to the
    /// historical body-id range convention (0–199 = monster, 200–399 = animal,
    /// 400+ = human/equipment).
    /// </summary>
    public static class MobTypes
    {
        public const int MaxBody = 2047;

        // Internal mobtypes.txt layout: monster, sea_monster, animal, human, equipment.
        // This matches UOFiddler's pre-existing AnimationListControl ordering
        // (Monster=0, Sea=1, Animal=2, Human=3, Equipment=4).
        private static readonly string[] _typeNames =
        {
            "monster",
            "sea_monster",
            "animal",
            "human",
            "equipment"
        };

        // Action counts per category. Equipment composites onto a humanoid so
        // shares the human action set size.
        private static readonly int[] _actionCounts = { 22, 9, 13, 35, 35 };

        private static readonly Dictionary<int, Entry> _entries = new();

        private struct Entry
        {
            public MobType Type;
            public uint Flags;
        }

        public static bool IsLoaded { get; private set; }

        static MobTypes()
        {
            Reload();
        }

        public static void Reload()
        {
            _entries.Clear();
            IsLoaded = false;

            string path = Files.GetFilePath("mobtypes.txt");
            if (path == null)
            {
                return;
            }

            try
            {
                foreach (string rawLine in File.ReadLines(path))
                {
                    string line = rawLine.Trim();
                    if (line.Length == 0 || line[0] == '#' || !char.IsDigit(line[0]))
                    {
                        continue;
                    }

                    string[] parts = line.Split(new[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 3)
                    {
                        continue;
                    }

                    if (!int.TryParse(parts[0], out int id))
                    {
                        continue;
                    }

                    string typeName = parts[1].ToLowerInvariant();

                    string flagStr = parts[2];
                    int commentIdx = flagStr.IndexOf('#');
                    if (commentIdx == 0)
                    {
                        continue;
                    }

                    if (commentIdx > 0)
                    {
                        flagStr = flagStr.Substring(0, commentIdx).Trim();
                    }

                    flagStr = flagStr.Replace("0x", "").Replace("0X", "");
                    if (!uint.TryParse(flagStr, NumberStyles.HexNumber, null, out uint flags))
                    {
                        continue;
                    }

                    int typeIdx = Array.IndexOf(_typeNames, typeName);
                    if (typeIdx < 0)
                    {
                        continue;
                    }

                    _entries[id] = new Entry { Type = (MobType)typeIdx, Flags = flags };
                }

                IsLoaded = _entries.Count > 0;
            }
            catch
            {
                // mobtypes.txt is optional; parsing failures are non-fatal.
                _entries.Clear();
                IsLoaded = false;
            }
        }

        public static bool TryGet(int body, out MobType type, out uint flags)
        {
            if (_entries.TryGetValue(body, out Entry entry))
            {
                type = entry.Type;
                flags = entry.Flags;
                return true;
            }

            type = MobType.Monster;
            flags = 0;
            return false;
        }

        /// <summary>
        /// Returns the mobtype for a body, or <see cref="MobType.Monster"/> if
        /// the body has no entry (per user-confirmed plan choice).
        /// </summary>
        public static MobType GetTypeOrDefault(int body)
        {
            return _entries.TryGetValue(body, out Entry entry) ? entry.Type : MobType.Monster;
        }

        public static uint GetFlags(int body)
        {
            return _entries.TryGetValue(body, out Entry entry) ? entry.Flags : 0u;
        }

        public static int GetActionCount(MobType type)
        {
            int idx = (int)type;
            return (uint)idx < _actionCounts.Length ? _actionCounts[idx] : 22;
        }

        /// <summary>
        /// idx records per body for a given category (5 directions × action count).
        /// </summary>
        public static int GetIdxStride(MobType type)
        {
            return GetActionCount(type) * 5;
        }

        public static IEnumerable<int> GetDefinedBodies()
        {
            return _entries.Keys;
        }
    }
}
