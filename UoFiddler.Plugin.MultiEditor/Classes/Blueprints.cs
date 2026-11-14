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
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Plugin.MultiEditor.Tools;

namespace UoFiddler.Plugin.MultiEditor.Classes
{
    /// <summary>
    /// A blueprint to write: its name, how many tiles it has (as far as known before reading it,
    /// for the size warning) and its tiles, read only when written.
    /// </summary>
    internal sealed record BlueprintSource(string Name, int TileCount, Func<List<ClipboardTile>> Tiles);

    /// <summary>What a <see cref="Blueprints.SaveMany"/> call wrote and what it could not.</summary>
    internal sealed record BlueprintSaveResult(int Saved, List<string> Failures);

    /// <summary>
    /// Saved groups of tiles to stamp again ("blueprints"), one multi text file each in the profile's
    /// MultiEditorBlueprints folder (the same lines the clipboard uses). A subfolder is a group. The
    /// first line starting with # holds the blueprint's name; files without one go by their file name.
    /// </summary>
    internal static class Blueprints
    {
        /// <summary>Blueprints above this many tiles are confirmed before they are saved.</summary>
        public const int LargeBlueprintTiles = 2000;

        private const string _folderName = "MultiEditorBlueprints";

        /// <summary>The folder blueprints were kept in when they were called prefabs.</summary>
        private const string _oldFolderName = "MultiEditorPrefabs";

        private static string _migratedFrom;

        /// <summary>The blueprint folder. The old prefab folder is moved into it the first time it is asked for.</summary>
        public static string Folder
        {
            get
            {
                string folder = Path.Combine(Options.AppDataPath, _folderName);
                string old = Path.Combine(Options.AppDataPath, _oldFolderName);
                if (_migratedFrom != old)
                {
                    _migratedFrom = old;
                    MoveOldFolder(old, folder);
                }

                return folder;
            }
        }

        /// <summary>
        /// Moves the old prefab folder to the blueprint folder: renamed when there is no blueprint
        /// folder yet, else merged file by file (groups too), leaving anything whose name is taken.
        /// The old folder is removed once it is empty. Failures leave the files where they are.
        /// </summary>
        private static void MoveOldFolder(string old, string folder)
        {
            try
            {
                if (!Directory.Exists(old))
                {
                    return;
                }

                if (!Directory.Exists(folder))
                {
                    Directory.Move(old, folder);
                    return;
                }

                MergeFolder(old, folder);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // The old files stay; the next start tries again.
            }
        }

        private static void MergeFolder(string from, string to)
        {
            Directory.CreateDirectory(to);
            foreach (string file in Directory.EnumerateFiles(from).ToList())
            {
                string target = Path.Combine(to, Path.GetFileName(file));
                if (!File.Exists(target))
                {
                    File.Move(file, target);
                }
            }

            foreach (string sub in Directory.EnumerateDirectories(from).ToList())
            {
                MergeFolder(sub, Path.Combine(to, Path.GetFileName(sub)));
            }

            if (!Directory.EnumerateFileSystemEntries(from).Any())
            {
                Directory.Delete(from);
            }
        }

        /// <summary>The folder of a group; the blueprint folder itself for no group.</summary>
        public static string GroupFolder(string group)
        {
            return string.IsNullOrEmpty(group) ? Folder : Path.Combine(Folder, group);
        }

        /// <summary>The blueprint groups (subfolders), read from disk.</summary>
        public static List<string> Groups()
        {
            try
            {
                string folder = Folder;
                return Directory.Exists(folder)
                    ? Directory.EnumerateDirectories(folder).Select(Path.GetFileName).OrderBy(f => f, StringComparer.OrdinalIgnoreCase).ToList()
                    : new List<string>();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                return new List<string>();
            }
        }

        /// <summary>The group the last bulk save went to, offered first next time; empty for no group.</summary>
        public static string LastGroup { get; set; } = string.Empty;

        /// <summary>A file name made safe from what the user typed.</summary>
        public static string SafeName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }

            return name.Trim();
        }

        /// <summary>
        /// Reads a blueprint file: its tiles (null when the file is missing or holds a line that is
        /// neither a tile nor a # line) and its name: the first # line, else the file name.
        /// </summary>
        public static (List<ClipboardTile> Tiles, string Name) Read(string path)
        {
            string fileName = Path.GetFileNameWithoutExtension(path);
            try
            {
                if (!File.Exists(path))
                {
                    return (null, fileName);
                }

                (List<ClipboardTile> tiles, string name) = Parse(File.ReadAllText(path));
                return (tiles, name.Length > 0 ? name : fileName);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                return (null, fileName);
            }
        }

        /// <summary>Blueprint file text: its tiles and the name from its first # line (empty without one).</summary>
        public static (List<ClipboardTile> Tiles, string Name) Parse(string text)
        {
            string name = null;
            var lines = new StringBuilder();
            foreach (string raw in text.Split('\n'))
            {
                string line = raw.Trim();
                if (line.StartsWith('#'))
                {
                    name ??= line[1..].Trim();
                }
                else if (line.Length > 0)
                {
                    lines.Append(line).Append('\n');
                }
            }

            return (ClipboardTile.Parse(lines.ToString()), name ?? string.Empty);
        }

        /// <summary>The file text: the name (on one line) when there is one, then the tile lines.</summary>
        public static string Format(IEnumerable<ClipboardTile> tiles, string name)
        {
            string text = ClipboardTile.Format(tiles);
            string line = OneLine(name);
            return line.Length == 0 ? text : $"# {line}\r\n{text}";
        }

        public static void Write(string path, IEnumerable<ClipboardTile> tiles, string name)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, Format(tiles, name));
        }

        /// <summary>
        /// A free name for a blueprint in a folder and the file for it: the name as given, or with
        /// " (2)", " (3)"... added when its file is taken. <paramref name="own"/> is a file that
        /// does not count as taken (the blueprint being renamed).
        /// </summary>
        public static (string Path, string Name) UniqueName(string folder, string name, string own = null)
        {
            string baseName = OneLine(name);
            if (SafeName(baseName).Length == 0)
            {
                baseName = "Blueprint";
            }

            string candidate = baseName;
            string path = Path.Combine(folder, SafeName(candidate) + ".txt");
            for (int n = 2; File.Exists(path) && !string.Equals(Path.GetFullPath(path), own == null ? null : Path.GetFullPath(own), StringComparison.OrdinalIgnoreCase); ++n)
            {
                candidate = string.Create(CultureInfo.InvariantCulture, $"{baseName} ({n})");
                path = Path.Combine(folder, SafeName(candidate) + ".txt");
            }

            return (path, candidate);
        }

        /// <summary>
        /// How much room the tiles take: cells along X and Y, and Z from the lowest tile to the top
        /// of the highest (its tiledata height included).
        /// </summary>
        public static (int Width, int Depth, int Height) Footprint(IReadOnlyCollection<ClipboardTile> tiles)
        {
            if (tiles == null || tiles.Count == 0)
            {
                return (0, 0, 0);
            }

            int minX = int.MaxValue, minY = int.MaxValue, minZ = int.MaxValue;
            int maxX = int.MinValue, maxY = int.MinValue, top = int.MinValue;
            foreach (ClipboardTile tile in tiles)
            {
                minX = Math.Min(minX, tile.Dx);
                minY = Math.Min(minY, tile.Dy);
                minZ = Math.Min(minZ, tile.Dz);
                maxX = Math.Max(maxX, tile.Dx);
                maxY = Math.Max(maxY, tile.Dy);
                int height = TileData.ItemTable != null && tile.Id < TileData.ItemTable.Length ? TileData.ItemTable[tile.Id].Height : 0;
                top = Math.Max(top, tile.Dz + height);
            }

            return (maxX - minX + 1, maxY - minY + 1, top - minZ);
        }

        /// <summary>A footprint as text: "12×9", with the height when <paramref name="withHeight"/>.</summary>
        public static string FootprintText((int Width, int Depth, int Height) size, bool withHeight)
        {
            string cells = string.Create(CultureInfo.InvariantCulture, $"{size.Width}×{size.Depth}");
            return withHeight ? string.Create(CultureInfo.InvariantCulture, $"{cells} cells, {size.Height} Z high") : cells;
        }

        /// <summary>
        /// A multi's tiles as blueprint tiles from its lowest X, Y and Z, keeping each tile's
        /// visibility flag and extra field the way the editor reads them.
        /// </summary>
        public static List<ClipboardTile> FromComponents(MultiComponentList list)
        {
            var tiles = new List<ClipboardTile>();
            if (list == null || list == MultiComponentList.Empty)
            {
                return tiles;
            }

            for (int x = 0; x < list.Width; ++x)
            {
                for (int y = 0; y < list.Height; ++y)
                {
                    foreach (MTile tile in list.Tiles[x][y])
                    {
                        tiles.Add(new ClipboardTile(tile.Id, x, y, tile.Z, tile.Flag == 0, tile.Unk1));
                    }
                }
            }

            if (tiles.Count == 0)
            {
                return tiles;
            }

            int minX = tiles.Min(t => t.Dx), minY = tiles.Min(t => t.Dy), minZ = tiles.Min(t => t.Dz);
            return tiles.Select(t => t with { Dx = t.Dx - minX, Dy = t.Dy - minY, Dz = t.Dz - minZ }).ToList();
        }

        /// <summary>
        /// Writes each source into a group's folder under a free name; a source that fails or has
        /// no tiles is listed with the reason and the rest are still written.
        /// </summary>
        /// <param name="groupOf">The group for each source; empty for no group.</param>
        public static BlueprintSaveResult SaveMany(IEnumerable<BlueprintSource> sources, Func<BlueprintSource, string> groupOf)
        {
            int saved = 0;
            var failures = new List<string>();
            foreach (BlueprintSource source in sources)
            {
                try
                {
                    List<ClipboardTile> tiles = source.Tiles();
                    if (tiles == null || tiles.Count == 0)
                    {
                        failures.Add($"{source.Name}: no tiles");
                        continue;
                    }

                    string folder = GroupFolder(SafeName(groupOf(source) ?? string.Empty));
                    Directory.CreateDirectory(folder);
                    (string path, string name) = UniqueName(folder, source.Name);
                    Write(path, tiles, name);
                    ++saved;
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException or EndOfStreamException or ArgumentException)
                {
                    failures.Add($"{source.Name}: {ex.Message}");
                }
            }

            return new BlueprintSaveResult(saved, failures);
        }

        /// <summary>
        /// A small picture of the blueprint: its visible tiles drawn the way the editor draws them, scaled to fit.
        /// </summary>
        public static Bitmap Thumbnail(IReadOnlyList<ClipboardTile> tiles, int size)
        {
            var placed = new List<(ClipboardTile Tile, Bitmap Art, int X, int Y)>();
            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
            foreach (ClipboardTile tile in tiles)
            {
                Bitmap art = tile.Invisible ? null : Art.GetStatic(tile.Id);
                if (art == null)
                {
                    continue;
                }

                int x = ((tile.Dx - tile.Dy) * 22) - (art.Width / 2);
                int y = ((tile.Dx + tile.Dy) * 22) - (tile.Dz * 4) - art.Height;
                placed.Add((tile, art, x, y));
                minX = Math.Min(minX, x);
                minY = Math.Min(minY, y);
                maxX = Math.Max(maxX, x + art.Width);
                maxY = Math.Max(maxY, y + art.Height);
            }

            var thumb = new Bitmap(size, size);
            if (placed.Count == 0)
            {
                return thumb;
            }

            using var full = new Bitmap(Math.Max(1, maxX - minX), Math.Max(1, maxY - minY));
            using (Graphics g = Graphics.FromImage(full))
            {
                // The editor's stacking order (MultiTile.CompareTo): back to front by cell diagonal, then
                // Z raised by the sort threshold, then the threshold, then file order (OrderBy is stable).
                foreach (var p in placed.OrderBy(p => p.Tile.Dx + p.Tile.Dy)
                             .ThenBy(p => p.Tile.Dz + MultiTile.SortThresholdOf(p.Tile.Id))
                             .ThenBy(p => MultiTile.SortThresholdOf(p.Tile.Id)))
                {
                    g.DrawImageUnscaled(p.Art, p.X - minX, p.Y - minY);
                }
            }

            using (Graphics g = Graphics.FromImage(thumb))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                float scale = Math.Min(1f, Math.Min((float)size / full.Width, (float)size / full.Height));
                int w = Math.Max(1, (int)(full.Width * scale));
                int h = Math.Max(1, (int)(full.Height * scale));
                g.DrawImage(full, (size - w) / 2, (size - h) / 2, w, h);
            }

            return thumb;
        }

        private static string OneLine(string text)
        {
            return string.IsNullOrWhiteSpace(text) ? string.Empty : string.Join(' ', text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)).Trim();
        }
    }
}
