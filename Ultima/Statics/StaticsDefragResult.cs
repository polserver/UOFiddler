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
using System.Globalization;
using System.Text;

namespace Ultima.Statics
{
    public enum RejectReason
    {
        InvalidItemId,
        OutOfBlockOffset,
        InvalidZ,
        BelowTerrain,
        Duplicate,
        CollapsedStack
    }

    public readonly struct RejectedStaticTile
    {
        public RejectedStaticTile(int blockX, int blockY, StaticTile tile, RejectReason reason)
        {
            BlockX = blockX;
            BlockY = blockY;
            Tile = tile;
            Reason = reason;
        }

        public int BlockX { get; }

        public int BlockY { get; }

        public StaticTile Tile { get; }

        public RejectReason Reason { get; }

        public int WorldX => (BlockX << 3) + (Tile.X & 0x7);

        public int WorldY => (BlockY << 3) + (Tile.Y & 0x7);

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "block {0},{1} tile 0x{2:X4} at {3},{4},{5} hue {6} - {7}",
                BlockX, BlockY, Tile.Id, WorldX, WorldY, Tile.Z, Tile.Hue, Reason);
        }
    }

    public sealed class StaticsDefragResult : IStaticsFilterStats
    {
        public int FileIndex { get; set; }

        public bool DryRun { get; set; }

        public string SourceIndexPath { get; set; }

        public string SourceStaticsPath { get; set; }

        public string OutputIndexPath { get; set; }

        public string OutputStaticsPath { get; set; }

        public int BlockWidth { get; set; }

        public int BlockHeight { get; set; }

        public int BlockCount => BlockWidth * BlockHeight;

        public long SourceIndexEntries { get; set; }

        public string GeometryEvidence { get; set; }

        public int MaxItemIdUsed { get; set; }

        public int HighestItemIdSeen { get; set; }

        public long SourceStaticsBytes { get; set; }

        public long OutputStaticsBytes { get; set; }

        public int BlocksProcessed { get; set; }

        public int SourceBlocksWithStatics { get; set; }

        public int OutputBlocksWithStatics { get; set; }

        public int BlocksClearedByRemove { get; set; }

        public int BlocksWithBadLength { get; set; }

        public int BlocksWithBadLookup { get; set; }

        public long TilesRead { get; set; }

        public long TilesWritten { get; set; }

        public long PendingTilesAdded { get; set; }

        public long DroppedInvalidItemId { get; set; }

        public long DroppedOutOfBlock { get; set; }

        public long MaskedOutOfBlock { get; set; }

        public long DroppedInvalidZ { get; set; }

        public long DroppedBelowTerrain { get; set; }

        public long DuplicatesRemoved { get; set; }

        public long StacksCollapsed { get; set; }

        public long HuesNormalized { get; set; }

        public TimeSpan Elapsed { get; set; }

        public List<string> Warnings { get; } = new List<string>();

        public List<RejectedStaticTile> RejectSamples { get; } = new List<RejectedStaticTile>();

        /// <summary>
        /// Every source tile the output does not carry, as accounted for by the filters. When a real
        /// comparison of the two files disagrees with this number the engine lost or invented data.
        /// </summary>
        public long TilesAccountedFor => DroppedInvalidItemId + DroppedOutOfBlock + DroppedInvalidZ +
                                         DroppedBelowTerrain + DuplicatesRemoved + StacksCollapsed;

        void IStaticsFilterStats.TileRejected(int blockX, int blockY, StaticTile tile, RejectReason reason)
        {
            switch (reason)
            {
                case RejectReason.InvalidItemId:
                    ++DroppedInvalidItemId;
                    break;

                case RejectReason.OutOfBlockOffset:
                    ++DroppedOutOfBlock;
                    break;

                case RejectReason.InvalidZ:
                    ++DroppedInvalidZ;
                    break;

                case RejectReason.BelowTerrain:
                    ++DroppedBelowTerrain;
                    break;

                case RejectReason.Duplicate:
                    ++DuplicatesRemoved;
                    break;

                case RejectReason.CollapsedStack:
                    ++StacksCollapsed;
                    break;
            }

            if (RejectSamples.Count < RejectSampleLimit)
            {
                RejectSamples.Add(new RejectedStaticTile(blockX, blockY, tile, reason));
            }
        }

        void IStaticsFilterStats.HueNormalized()
        {
            ++HuesNormalized;
        }

        void IStaticsFilterStats.OutOfBlockMasked()
        {
            ++MaskedOutOfBlock;
        }

        /// <summary>How many removed statics to keep as examples.</summary>
        public int RejectSampleLimit { get; set; } = 200;

        public string ToReport()
        {
            var sb = new StringBuilder();

            if (DryRun)
            {
                sb.AppendLine("DRY RUN - no files were written.");
                sb.AppendLine();
            }

            sb.AppendLine(Line("Facet            : {0}", FileIndex));
            sb.AppendLine(Line("Source index     : {0}", SourceIndexPath));
            sb.AppendLine(Line("Source statics   : {0} ({1:N0} bytes)", SourceStaticsPath, SourceStaticsBytes));

            if (!DryRun)
            {
                sb.AppendLine(Line("Output index     : {0}", OutputIndexPath));
                sb.AppendLine(Line("Output statics   : {0}", OutputStaticsPath));
            }

            sb.AppendLine(Line("Output size      : {0:N0} bytes ({1:+#,0;-#,0;0} vs source)",
                OutputStaticsBytes, OutputStaticsBytes - SourceStaticsBytes));
            sb.AppendLine();

            sb.AppendLine(Line("Block grid       : {0} x {1} = {2:N0} blocks", BlockWidth, BlockHeight, BlockCount));
            sb.AppendLine(Line("Source index     : {0:N0} entries", SourceIndexEntries));

            if (!string.IsNullOrEmpty(GeometryEvidence))
            {
                sb.AppendLine(Line("Geometry         : {0}", GeometryEvidence));
            }

            sb.AppendLine();
            sb.AppendLine(Line("Item id ceiling  : 0x{0:X4} (highest id present: 0x{1:X4})", MaxItemIdUsed, HighestItemIdSeen));
            sb.AppendLine();
            sb.AppendLine(Line("Tiles read       : {0:N0}", TilesRead));
            sb.AppendLine(Line("Tiles written    : {0:N0}", TilesWritten));

            if (PendingTilesAdded > 0)
            {
                sb.AppendLine(Line("  of which added : {0:N0} (frozen in memory)", PendingTilesAdded));
            }

            sb.AppendLine();
            sb.AppendLine("Removed:");
            sb.AppendLine(Line("  invalid item id  : {0:N0}", DroppedInvalidItemId));
            sb.AppendLine(Line("  out-of-block x/y : {0:N0} dropped, {1:N0} masked", DroppedOutOfBlock, MaskedOutOfBlock));
            sb.AppendLine(Line("  invalid z        : {0:N0}", DroppedInvalidZ));
            sb.AppendLine(Line("  below terrain    : {0:N0}", DroppedBelowTerrain));
            sb.AppendLine(Line("  duplicates       : {0:N0}", DuplicatesRemoved));
            sb.AppendLine(Line("  collapsed stacks : {0:N0}", StacksCollapsed));
            sb.AppendLine(Line("  hues normalized  : {0:N0}", HuesNormalized));
            sb.AppendLine();

            sb.AppendLine(Line("Blocks processed : {0:N0}", BlocksProcessed));
            sb.AppendLine(Line("  with statics   : {0:N0} source -> {1:N0} output", SourceBlocksWithStatics, OutputBlocksWithStatics));

            if (BlocksClearedByRemove > 0)
            {
                sb.AppendLine(Line("  cleared        : {0:N0} (removed in memory)", BlocksClearedByRemove));
            }

            if (BlocksWithBadLength > 0 || BlocksWithBadLookup > 0)
            {
                sb.AppendLine(Line("  damaged index  : {0:N0} bad lookup, {1:N0} bad length", BlocksWithBadLookup, BlocksWithBadLength));
            }

            sb.AppendLine();
            sb.AppendLine(Line("Elapsed          : {0:hh\\:mm\\:ss\\.fff}", Elapsed));

            if (Warnings.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine(Line("Warnings ({0}):", Warnings.Count));

                foreach (string warning in Warnings)
                {
                    sb.AppendLine("  " + warning);
                }
            }

            if (RejectSamples.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine(Line("Removed tile samples (first {0}):", RejectSamples.Count));

                foreach (RejectedStaticTile sample in RejectSamples)
                {
                    sb.AppendLine("  " + sample);
                }
            }

            return sb.ToString();
        }

        private static string Line(string format, params object[] args)
        {
            return string.Format(CultureInfo.InvariantCulture, format, args);
        }
    }
}