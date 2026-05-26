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
using System.IO;
using Ultima;

namespace UoFiddler.Plugin.Compare.Classes
{
    /// <summary>
    /// Helpers to guard against comparing a file against itself in the compare plugin.
    /// </summary>
    public static class CompareFiles
    {
        /// <summary>
        /// Returns true when both paths resolve to the same physical file (case-insensitive, normalized).
        /// </summary>
        public static bool IsSamePath(string a, string b)
        {
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
            {
                return false;
            }

            try
            {
                return string.Equals(Path.GetFullPath(a), Path.GetFullPath(b), StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Returns true when the chosen path matches the currently loaded client file resolved
        /// from any of the given client file names (via <see cref="Files.GetFilePath"/>).
        /// </summary>
        public static bool IsLoadedClientFile(string chosenPath, params string[] clientFileNames)
        {
            if (string.IsNullOrEmpty(chosenPath))
            {
                return false;
            }

            foreach (string name in clientFileNames)
            {
                if (IsSamePath(Files.GetFilePath(name), chosenPath))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
