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

namespace Ultima.Uop
{
    /// <summary>
    /// The names the client gives each packable file type, and the compression its shipped UOPs use.
    /// One table, so a save, a pack and a batch repack cannot disagree about what to write.
    /// </summary>
    public static class UopFileNames
    {
        /// <summary>
        /// The conventional file names for a type: the mul, its idx (null for maps, which have none)
        /// and the uop that replaces the pair.
        /// </summary>
        public static (string Mul, string Idx, string Uop) For(FileType type, int mapIndex = 0)
        {
            return type switch
            {
                FileType.ArtLegacyMul => ("art.mul", "artidx.mul", "artLegacyMUL.uop"),
                FileType.GumpartLegacyMul => ("gumpart.mul", "gumpidx.mul", "gumpartLegacyMUL.uop"),
                FileType.MapLegacyMul => ($"map{mapIndex}.mul", null, $"map{mapIndex}LegacyMUL.uop"),
                FileType.SoundLegacyMul => ("sound.mul", "soundidx.mul", "soundLegacyMUL.uop"),
                FileType.MultiCollection => ("multi.mul", "multi.idx", "MultiCollection.uop"),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown file type.")
            };
        }

        /// <summary>
        /// Asks the loaded client for the type's uop. <see cref="Files.MulPath"/> is keyed case
        /// insensitively, so the conventional name doubles as the lookup key.
        /// </summary>
        public static string ClientUopPath(FileType type, int mapIndex = 0)
        {
            return Files.GetFilePath(For(type, mapIndex).Uop);
        }

        /// <summary>
        /// Asks the loaded client for the type's mul.
        /// </summary>
        public static string ClientMulPath(FileType type, int mapIndex = 0)
        {
            return Files.GetFilePath(For(type, mapIndex).Mul);
        }

        /// <summary>
        /// What the shipped clients compress a type with.
        /// </summary>
        /// <remarks>
        /// Every entry of every shipped MultiCollection.uop is zlib compressed: packing it uncompressed
        /// produces a file several times larger than the original, and Mythic is not a valid compression
        /// for this type at all, so the choice is fixed rather than merely defaulted
        /// (see <see cref="IsCompressionFixed"/>).
        /// Every art, map and sound entry of every shipped client is stored uncompressed, and UOFiddler's
        /// own map reader can only address stored entries. Gumpart is stored in the shipped files too, but
        /// the client does accept zlib and Mythic there, so that one is a default rather than a rule.
        /// </remarks>
        public static CompressionFlag DefaultCompression(FileType type)
        {
            return type == FileType.MultiCollection ? CompressionFlag.Zlib : CompressionFlag.None;
        }

        /// <summary>
        /// True when the type accepts only <see cref="DefaultCompression"/> and the caller must not offer
        /// a choice.
        /// </summary>
        public static bool IsCompressionFixed(FileType type)
        {
            return type == FileType.MultiCollection;
        }
    }
}