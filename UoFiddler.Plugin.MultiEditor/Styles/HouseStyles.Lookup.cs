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
using UoFiddler.Plugin.MultiEditor.Rendering;

namespace UoFiddler.Plugin.MultiEditor.Styles
{
    internal sealed partial class HouseStyles
    {
        /// <summary>The style and column a tile id is listed under, when any.</summary>
        public bool TryGetRole(ushort id, out HouseStyle style, out string slot)
        {
            if (_roles.TryGetValue(id, out var role))
            {
                style = role.Style;
                slot = role.Slot;
                return true;
            }

            style = null;
            slot = null;
            return false;
        }

        /// <summary>
        /// The style and column of a tile, also when the tables list it under another id with the
        /// same art: the classic roof and stair tiles are copies of the custom house pieces, which
        /// is how their facing is known. <paramref name="exact"/> tells the two cases apart.
        /// </summary>
        public bool TryGetRoleByArt(ushort id, out HouseStyle style, out string slot, out bool exact)
        {
            exact = TryGetRole(id, out style, out slot);
            if (exact)
            {
                return true;
            }

            return TryGetListedTwin(id, out ushort listed) && TryGetRole(listed, out style, out slot);
        }

        /// <summary>
        /// The listed piece a tile copies. Either the art is the same, or the tile sits in a run of
        /// ids that copies a listed run piece by piece (the classic roof sets do; one or two of
        /// their pieces were redrawn, and those are found by their place in the run).
        /// </summary>
        public bool TryGetListedTwin(ushort id, out ushort listed)
        {
            if (_roles.ContainsKey(id))
            {
                listed = id;
                return true;
            }

            if (TryGetSameArt(id, out listed))
            {
                return true;
            }

            // Neighbours of the same kind that copy a listed piece say where this one's twin should be.
            if (id >= Ultima.TileData.ItemTable.Length)
            {
                listed = 0;
                return false;
            }

            string name = Ultima.TileData.ItemTable[id].Name;
            var votes = new Dictionary<(int Offset, HouseStyle Style), int>();
            for (int distance = 1; distance <= 6; distance++)
            {
                foreach (int neighbour in new[] { id - distance, id + distance })
                {
                    if (neighbour >= 0 && neighbour < Ultima.TileData.ItemTable.Length && !_roles.ContainsKey((ushort)neighbour) &&
                        SameSet(Ultima.TileData.ItemTable[neighbour].Name, name) &&
                        TryGetSameArt((ushort)neighbour, out ushort twin))
                    {
                        var key = (twin - neighbour, _roles[twin].Style);
                        votes[key] = votes.TryGetValue(key, out int count) ? count + 1 : 1;
                    }
                }
            }

            foreach (KeyValuePair<(int Offset, HouseStyle Style), int> vote in votes)
            {
                int candidate = id + vote.Key.Offset;
                if (vote.Value >= 3 && candidate >= 0 && candidate <= ushort.MaxValue &&
                    _roles.TryGetValue((ushort)candidate, out (HouseStyle Style, string Slot) role) &&
                    role.Style == vote.Key.Style)
                {
                    listed = (ushort)candidate;
                    return true;
                }
            }

            listed = 0;
            return false;
        }

        /// <summary>
        /// The four corner pieces of a stair set, in the order a left turn goes through them
        /// (south-east, north-east, north-west, south-west), and where <paramref name="id"/> is in
        /// it. stairs.txt names only the two front corners of a stair that climbs north: the one
        /// that is its own mirror image is the south-east one, the mirror twin of the other is the
        /// north-east one, and the fourth is the id next to those three.
        /// </summary>
        public bool TryGetStairCorner(ushort id, out ushort[] corners, out int index)
        {
            if (_stairCorners == null)
            {
                _stairCorners = new Dictionary<ushort, (ushort[], int)>();
                foreach (HouseStyle style in Styles)
                {
                    if (style.Kind != HousePartKind.Stair)
                    {
                        continue;
                    }

                    AddStairCorners(style.Get("Squared1"), style.Get("Squared2"));
                    AddStairCorners(style.Get("Rounded1"), style.Get("Rounded2"));
                }
            }

            if (_stairCorners.TryGetValue(id, out (ushort[] Corners, int Index) found))
            {
                corners = found.Corners;
                index = found.Index;
                return true;
            }

            corners = null;
            index = -1;
            return false;
        }

        private void AddStairCorners(ushort first, ushort second)
        {
            if (first == 0 || second == 0)
            {
                return;
            }

            bool firstSymmetric = ArtFacing.Transpose(first) == first;
            bool secondSymmetric = ArtFacing.Transpose(second) == second;
            if (firstSymmetric == secondSymmetric)
            {
                return;
            }

            ushort southEast = firstSymmetric ? first : second;
            ushort southWest = firstSymmetric ? second : first;
            ushort northEast = ArtFacing.Transpose(southWest);

            int low = Math.Min(southEast, Math.Min(southWest, northEast));
            int high = Math.Max(southEast, Math.Max(southWest, northEast));
            if (high - low != 2)
            {
                return;
            }

            foreach (int candidate in new[] { low - 1, high + 1 })
            {
                if (candidate < 0 || candidate > ushort.MaxValue || _roles.ContainsKey((ushort)candidate) ||
                    ArtFacing.ArtHash((ushort)candidate) == 0 || ArtFacing.Transpose((ushort)candidate) != candidate)
                {
                    continue;
                }

                ushort[] corners = { southEast, northEast, (ushort)candidate, southWest };
                for (int i = 0; i < corners.Length; i++)
                {
                    _stairCorners.TryAdd(corners[i], (corners, i));
                }

                return;
            }
        }

        /// <summary>Tiledata names of one set: equal, or one is the other with a letter more ("shingle", "shingles").</summary>
        private static bool SameSet(string a, string b)
        {
            a ??= string.Empty;
            b ??= string.Empty;
            return Math.Abs(a.Length - b.Length) <= 1 &&
                   (a.StartsWith(b, StringComparison.OrdinalIgnoreCase) || b.StartsWith(a, StringComparison.OrdinalIgnoreCase));
        }

        private bool TryGetSameArt(ushort id, out ushort listed)
        {
            if (_listedByArt == null)
            {
                _listedByArt = new Dictionary<ulong, ushort>();
                foreach (KeyValuePair<ushort, (HouseStyle Style, string Slot)> role in _roles)
                {
                    if (role.Value.Style.Kind is not (HousePartKind.Roof or HousePartKind.Stair))
                    {
                        continue;
                    }

                    ulong roleHash = ArtFacing.ArtHash(role.Key);
                    if (roleHash != 0)
                    {
                        _listedByArt.TryAdd(roleHash, role.Key);
                    }
                }
            }

            ulong hash = ArtFacing.ArtHash(id);
            listed = 0;
            return hash != 0 && _listedByArt.TryGetValue(hash, out listed);
        }
    }
}
