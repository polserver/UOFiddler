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

using System.IO;

namespace UoFiddler.Plugin.Compare.Classes
{
    /// Resolves a user's "Auto/MUL/UOP" mode pick into actual file paths for
    /// the SecondFileIndex constructor. Returns false with an error message
    /// when the chosen mode cannot be satisfied by the files on disk.
    internal static class SecondLoadHelper
    {
        public static bool TryResolveArtPaths(string mode, string idxFile, string mulFile, string uopFile,
            out string resolvedIdx, out string resolvedMul, out string resolvedUop, out string error)
        {
            return TryResolvePaths(mode, idxFile, mulFile, uopFile, "art.mul + artidx.mul", "artLegacyMUL.uop",
                out resolvedIdx, out resolvedMul, out resolvedUop, out error);
        }

        public static bool TryResolveGumpPaths(string mode, string idxFile, string mulFile, string uopFile,
            out string resolvedIdx, out string resolvedMul, out string resolvedUop, out string error)
        {
            return TryResolvePaths(mode, idxFile, mulFile, uopFile, "gumpart.mul + gumpidx.mul", "gumpartLegacyMUL.uop",
                out resolvedIdx, out resolvedMul, out resolvedUop, out error);
        }

        private static bool TryResolvePaths(string mode, string idxFile, string mulFile, string uopFile,
            string mulDescription, string uopDescription,
            out string resolvedIdx, out string resolvedMul, out string resolvedUop, out string error)
        {
            resolvedIdx = null;
            resolvedMul = null;
            resolvedUop = null;
            error = null;

            bool mulExists = File.Exists(idxFile) && File.Exists(mulFile);
            bool uopExists = File.Exists(uopFile);

            switch (mode)
            {
                case "MUL":
                    if (!mulExists)
                    {
                        error = $"Could not find {mulDescription} in the selected directory.";
                        return false;
                    }
                    resolvedIdx = idxFile;
                    resolvedMul = mulFile;
                    return true;

                case "UOP":
                    if (!uopExists)
                    {
                        error = $"Could not find {uopDescription} in the selected directory.";
                        return false;
                    }
                    resolvedUop = uopFile;
                    return true;

                default: // "Auto" or anything unknown
                    if (uopExists)
                    {
                        resolvedUop = uopFile;
                        return true;
                    }

                    if (mulExists)
                    {
                        resolvedIdx = idxFile;
                        resolvedMul = mulFile;
                        return true;
                    }

                    error = $"Could not find {uopDescription} or {mulDescription} in the selected directory.";
                    return false;
            }
        }
    }
}
