// /***************************************************************************
//  *
//  * $Author: Turley
//  *
//  * "THE BEER-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Threading;

namespace Ultima.Statics
{
    /// <summary>
    /// What to do with a static whose in-block offset is outside the 0..7 range the format allows.
    /// The client masks these with &amp; 7 when reading, so they show up in the wrong cell.
    /// </summary>
    public enum OutOfBlockAction
    {
        Drop,
        Mask,
        Keep
    }

    /// <summary>
    /// Byte pattern written for a block that ends up with no statics.
    /// </summary>
    public enum EmptyBlockStyle
    {
        /// <summary>lookup/length/extra all -1, as shipped by the client and written by UOFiddler today.</summary>
        NegativeOne,

        /// <summary>lookup -1, length 0, extra 0.</summary>
        Zero
    }

    /// <summary>
    /// Where the "this item id does not exist" ceiling comes from.
    /// </summary>
    public enum ItemIdCeiling
    {
        /// <summary>Highest id described by tiledata.mul. The count the client can actually resolve.</summary>
        TileData,

        /// <summary>Highest id reported by <see cref="Art.GetMaxItemId"/>.</summary>
        Art,

        /// <summary>0x3FFF, the pre-ML item id limit.</summary>
        Legacy
    }

    public sealed class StaticsDefragProgress
    {
        public int BlocksDone { get; init; }
        public int BlocksTotal { get; init; }
        public long TilesWritten { get; init; }
    }

    public sealed class StaticsDefragOptions
    {
        /// <summary>Facet file index, used to resolve staidx{N}.mul / statics{N}.mul.</summary>
        public int FileIndex { get; set; }

        /// <summary>Explicit source override. When null the files are resolved through <see cref="Files.GetFilePath"/>.</summary>
        public string SourceIndexPath { get; set; }

        public string SourceStaticsPath { get; set; }

        /// <summary>
        /// Optional. Supplies frozen/melted in-memory edits and the land tiles the below-terrain
        /// filter needs. Without it those features are unavailable.
        /// </summary>
        public Map Map { get; set; }

        public int BlockWidth { get; set; }

        public int BlockHeight { get; set; }

        /// <summary>
        /// Permits writing fewer blocks than the source index holds. Without it a source index
        /// that reaches past the configured map size is refused rather than truncated.
        /// </summary>
        public bool AllowGeometryTruncation { get; set; }

        public bool DropInvalidItemIds { get; set; } = true;

        public ItemIdCeiling ItemIdCeiling { get; set; } = ItemIdCeiling.TileData;

        /// <summary>Overrides <see cref="ItemIdCeiling"/> when greater than zero.</summary>
        public int MaxItemId { get; set; }

        public OutOfBlockAction OutOfBlockTiles { get; set; } = OutOfBlockAction.Drop;

        /// <summary>Drops statics at z == -128, the sentinel the client will not place.</summary>
        public bool DropInvalidZ { get; set; } = true;

        public bool NormalizeNegativeHue { get; set; } = true;

        /// <summary>Drops statics buried under the land tile. Requires <see cref="Map"/>.</summary>
        public bool DropBelowTerrain { get; set; }

        /// <summary>Removes statics sharing id, x, y and z. Hue is not part of the key.</summary>
        public bool RemoveDuplicates { get; set; }

        /// <summary>Adds hue to the duplicate key, which is what the old routine did.</summary>
        public bool DuplicatesCompareHue { get; set; }

        /// <summary>Collapses eligible statics sharing a cell down to one.</summary>
        public bool CollapseStacks { get; set; }

        /// <summary>A static is eligible for stack collapsing when any of these flags is set on its tile data.</summary>
        public TileFlag CollapseFlagMask { get; set; } = TileFlag.Wet;

        /// <summary>A static is also eligible when its id appears here.</summary>
        public HashSet<int> CollapseIds { get; } = new HashSet<int>();

        /// <summary>Groups stack candidates by x/y only, ignoring z.</summary>
        public bool CollapseIgnoreZ { get; set; }

        public EmptyBlockStyle EmptyBlocks { get; set; } = EmptyBlockStyle.NegativeOne;

        /// <summary>Carries the source index extra field through instead of always writing 0.</summary>
        public bool PreserveExtra { get; set; } = true;

        /// <summary>Emits each block's statics in a stable order. Off by default so a no-filter run is a pure compaction.</summary>
        public bool SortTiles { get; set; }

        public string OutputDirectory { get; set; }

        public bool DryRun { get; set; }

        public int RejectSampleLimit { get; set; } = 200;

        public IProgress<StaticsDefragProgress> Progress { get; set; }

        public CancellationToken CancellationToken { get; set; }

        /// <summary>
        /// Resolves the item id ceiling actually in force. Ids above it are considered invalid.
        /// </summary>
        public int ResolveMaxItemId()
        {
            if (MaxItemId > 0)
            {
                return MaxItemId;
            }

            switch (ItemIdCeiling)
            {
                case ItemIdCeiling.Legacy:
                    return 0x3FFF;

                case ItemIdCeiling.Art:
                    return Art.GetMaxItemId();

                default:
                    int count = TileData.ItemTable?.Length ?? 0;
                    return count > 0 ? count - 1 : Art.GetMaxItemId();
            }
        }

        /// <summary>
        /// Reproduces the call shape of the old <c>Map.DefragStatics</c> overload.
        /// </summary>
        public static StaticsDefragOptions Legacy(string outputDirectory, Map map, bool removeDuplicates)
        {
            return new StaticsDefragOptions
            {
                FileIndex = map.FileIndex,
                Map = map,
                OutputDirectory = outputDirectory,
                RemoveDuplicates = removeDuplicates
            };
        }
    }
}