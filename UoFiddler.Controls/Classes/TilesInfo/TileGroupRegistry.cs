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
using System.IO;
using Ultima;

namespace UoFiddler.Controls.Classes.TilesInfo
{
    public static class TileGroupRegistry
    {
        public static readonly (string FileName, string DisplayName)[] KnownFiles =
        {
            ("walls.txt",    "Walls"),
            ("floors.txt",   "Floors"),
            ("roof.txt",     "Roof"),
            ("doors.txt",    "Doors"),
            ("misc.txt",     "Misc"),
            ("stairs.txt",   "Stairs"),
            ("teleprts.txt", "Teleporters")
        };

        /// <summary>
        /// Loads every known file the client has. Files that are missing are skipped; files that
        /// exist but can't be read are listed in <paramref name="errors"/>.
        /// </summary>
        public static List<TileGroupFile> LoadAll(out List<string> errors)
        {
            var list = new List<TileGroupFile>(KnownFiles.Length);
            errors = [];

            foreach (var (fileName, displayName) in KnownFiles)
            {
                string fullPath = Files.GetFilePath(fileName);
                if (string.IsNullOrEmpty(fullPath) || !File.Exists(fullPath))
                {
                    continue;
                }

                try
                {
                    TileGroupFile file = TileGroupParser.Parse(fullPath, displayName);
                    if (file != null)
                    {
                        list.Add(file);
                    }
                    else
                    {
                        errors.Add($"{fileName}: no type row and header row");
                    }
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    errors.Add($"{fileName}: {ex.Message}");
                }
            }

            return list;
        }
    }
}
