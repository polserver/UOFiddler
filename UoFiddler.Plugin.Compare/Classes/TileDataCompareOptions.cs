using Ultima;

namespace UoFiddler.Plugin.Compare.Classes
{
    /// <summary>
    /// Controls which fields and flags participate in TileData comparison.
    /// All fields included and no flags ignored by default.
    /// </summary>
    public class TileDataCompareOptions
    {
        // ── Land tile fields ─────────────────────────────────────────────────────
        public bool LandCompareName      { get; set; } = true;
        public bool LandCompareTextureId { get; set; } = true;
        public bool LandCompareFlags     { get; set; } = true;

        // ── Item tile fields ─────────────────────────────────────────────────────
        public bool ItemCompareName           { get; set; } = true;
        public bool ItemCompareFlags          { get; set; } = true;
        public bool ItemCompareAnimation      { get; set; } = true;
        public bool ItemCompareWeight         { get; set; } = true;
        public bool ItemCompareQuality        { get; set; } = true;
        public bool ItemCompareQuantity       { get; set; } = true;
        public bool ItemCompareHue            { get; set; } = true;
        public bool ItemCompareStackingOffset { get; set; } = true;
        public bool ItemCompareValue          { get; set; } = true;
        public bool ItemCompareHeight         { get; set; } = true;
        public bool ItemCompareMiscData       { get; set; } = true;
        public bool ItemCompareUnk2           { get; set; } = true;
        public bool ItemCompareUnk3           { get; set; } = true;

        // ── Flag exclusions ──────────────────────────────────────────────────────
        /// <summary>
        /// Flags OR'd into this mask are excluded from comparison for both land and item tiles.
        /// </summary>
        public TileFlag IgnoredFlags { get; set; } = TileFlag.None;

        public void ResetToDefaults()
        {
            LandCompareName      = true;
            LandCompareTextureId = true;
            LandCompareFlags     = true;

            ItemCompareName           = true;
            ItemCompareFlags          = true;
            ItemCompareAnimation      = true;
            ItemCompareWeight         = true;
            ItemCompareQuality        = true;
            ItemCompareQuantity       = true;
            ItemCompareHue            = true;
            ItemCompareStackingOffset = true;
            ItemCompareValue          = true;
            ItemCompareHeight         = true;
            ItemCompareMiscData       = true;
            ItemCompareUnk2           = true;
            ItemCompareUnk3           = true;

            IgnoredFlags = TileFlag.None;
        }
    }
}
