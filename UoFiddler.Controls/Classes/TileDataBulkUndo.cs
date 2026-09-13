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

using Ultima;

namespace UoFiddler.Controls.Classes
{
    /// <summary>
    /// Snapshot of the tiledata entries a single bulk apply was about to overwrite,
    /// so one misplaced "apply to 4000 entries" can be taken back without reloading
    /// tiledata.mul and losing every other unsaved edit.
    /// <para>
    /// Only one level is kept - this is an escape hatch for the misclick, not an
    /// edit history.
    /// </para>
    /// </summary>
    public sealed class TileDataBulkUndo
    {
        private TileDataBulkUndo(bool land, int[] ids, ItemData[] items, LandData[] lands, string description)
        {
            Land = land;
            Ids = ids;
            Items = items;
            Lands = lands;
            Description = description;
        }

        public bool Land { get; }

        /// <summary>Graphic ids that were overwritten, in the order they were applied.</summary>
        public int[] Ids { get; }

        /// <summary>Pre-edit item entries, parallel to <see cref="Ids"/>. Null when <see cref="Land"/>.</summary>
        public ItemData[] Items { get; }

        /// <summary>Pre-edit land entries, parallel to <see cref="Ids"/>. Null unless <see cref="Land"/>.</summary>
        public LandData[] Lands { get; }

        /// <summary>What the apply changed, e.g. "Weight, +Impassable" - shown in the undo prompt.</summary>
        public string Description { get; }

        public int Count => Ids.Length;

        public static TileDataBulkUndo ForItems(int[] ids, string description)
        {
            var snapshot = new ItemData[ids.Length];
            for (int i = 0; i < ids.Length; ++i)
            {
                snapshot[i] = TileData.ItemTable[ids[i]];
            }

            return new TileDataBulkUndo(false, ids, snapshot, null, description);
        }

        public static TileDataBulkUndo ForLand(int[] ids, string description)
        {
            var snapshot = new LandData[ids.Length];
            for (int i = 0; i < ids.Length; ++i)
            {
                snapshot[i] = TileData.LandTable[ids[i]];
            }

            return new TileDataBulkUndo(true, ids, null, snapshot, description);
        }
    }
}
