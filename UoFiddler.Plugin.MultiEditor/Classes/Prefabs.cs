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
using System.IO;
using System.Linq;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Plugin.MultiEditor.Tools;

namespace UoFiddler.Plugin.MultiEditor.Classes
{
    /// <summary>
    /// Saved groups of tiles to stamp again, one multi text file per prefab in the profile's
    /// MultiEditorPrefabs folder (the same lines the clipboard uses).
    /// </summary>
    internal static class Prefabs
    {
        public static string Folder => Path.Combine(Options.AppDataPath, "MultiEditorPrefabs");

        public static IEnumerable<string> Names()
        {
            return Directory.Exists(Folder)
                ? Directory.EnumerateFiles(Folder, "*.txt").Select(Path.GetFileNameWithoutExtension).OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                : Enumerable.Empty<string>();
        }

        public static List<ClipboardTile> Load(string name)
        {
            string path = PathOf(name);
            return File.Exists(path) ? ClipboardTile.Parse(File.ReadAllText(path)) : null;
        }

        public static void Save(string name, IEnumerable<ClipboardTile> tiles)
        {
            Directory.CreateDirectory(Folder);
            File.WriteAllText(PathOf(name), ClipboardTile.Format(tiles));
        }

        public static void Delete(string name)
        {
            string path = PathOf(name);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

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
        /// A small picture of the prefab: its tiles drawn the way the editor draws them, scaled to fit.
        /// </summary>
        public static Bitmap Thumbnail(IReadOnlyList<ClipboardTile> tiles, int size)
        {
            var placed = new List<(ClipboardTile Tile, Bitmap Art, int X, int Y)>();
            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
            foreach (ClipboardTile tile in tiles)
            {
                Bitmap art = Art.GetStatic(tile.Id);
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
                foreach (var p in placed.OrderBy(p => p.Tile.Dx + p.Tile.Dy).ThenBy(p => p.Tile.Dz).ThenBy(p => TileData.ItemTable[p.Tile.Id].Height))
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

        private static string PathOf(string name) => Path.Combine(Folder, SafeName(name) + ".txt");
    }
}
