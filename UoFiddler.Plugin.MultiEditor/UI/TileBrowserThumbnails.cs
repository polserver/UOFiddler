/***************************************************************************
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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Text;
using Ultima;

namespace UoFiddler.Plugin.MultiEditor.UI
{
    /// <summary>
    /// Scaled-down copies of static art for the tile grids, in premultiplied ARGB because GDI+
    /// draws that format far faster than the 16 bit art. Art is never scaled up.
    /// </summary>
    internal sealed class TileBrowserThumbnails : IDisposable
    {
        // Enough for several screens of 64 px cells; a full clear past it is cheap and rare.
        private const int _maxEntries = 3000;

        /// <summary>The tiledata flags that matter when building a multi, with the names shown for them.</summary>
        private static readonly (TileFlag Flag, string Name)[] _buildFlags =
        {
            (TileFlag.Impassable, "Impassable"),
            (TileFlag.Wall, "Wall"),
            (TileFlag.NoShoot, "No shoot"),
            (TileFlag.Surface, "Surface"),
            (TileFlag.Bridge, "Bridge"),
            (TileFlag.StairBack, "Stair back"),
            (TileFlag.StairRight, "Stair right"),
            (TileFlag.Door, "Door"),
            (TileFlag.Window, "Window"),
            (TileFlag.Roof, "Roof"),
            (TileFlag.Background, "Background"),
            (TileFlag.Foliage, "Foliage"),
            (TileFlag.HoverOver, "Hover over")
        };

        /// <summary>The names of the building flags a tile has, in a fixed order.</summary>
        public static List<string> BuildFlagNames(TileFlag flags)
        {
            var names = new List<string>();
            foreach ((TileFlag flag, string name) in _buildFlags)
            {
                if ((flags & flag) != 0)
                {
                    names.Add(name);
                }
            }

            return names;
        }

        private readonly Dictionary<(int Id, int Box), Bitmap> _cache = new Dictionary<(int, int), Bitmap>();

        /// <summary>The art fitted into a box of the given size, or null when the tile has no art.</summary>
        public Bitmap Get(int id, int box)
        {
            if (_cache.TryGetValue((id, box), out Bitmap thumb))
            {
                return thumb;
            }

            Bitmap art = Art.GetStatic(id);
            if (art == null || art.Width <= 0 || art.Height <= 0)
            {
                _cache[(id, box)] = null;
                return null;
            }

            if (_cache.Count >= _maxEntries)
            {
                Clear();
            }

            float scale = Math.Min(1f, Math.Min((float)box / art.Width, (float)box / art.Height));
            int w = Math.Max(1, (int)Math.Round(art.Width * scale));
            int h = Math.Max(1, (int)Math.Round(art.Height * scale));
            thumb = new Bitmap(w, h, PixelFormat.Format32bppPArgb);
            using (Graphics g = Graphics.FromImage(thumb))
            {
                if (scale < 1f)
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    using var attributes = new ImageAttributes();
                    // Keeps the bicubic filter from pulling transparent black into the edge pixels.
                    attributes.SetWrapMode(WrapMode.TileFlipXY);
                    g.DrawImage(art, new Rectangle(0, 0, w, h), 0, 0, art.Width, art.Height, GraphicsUnit.Pixel, attributes);
                }
                else
                {
                    g.DrawImageUnscaled(art, 0, 0);
                }
            }

            _cache[(id, box)] = thumb;
            return thumb;
        }

        public void Clear()
        {
            foreach (Bitmap bitmap in _cache.Values)
            {
                bitmap?.Dispose();
            }

            _cache.Clear();
        }

        public void Dispose() => Clear();

        public static bool HasItemData(int id) => TileData.ItemTable != null && id >= 0 && id < TileData.ItemTable.Length;

        public static string NameOf(int id) => HasItemData(id) ? TileData.ItemTable[id].Name?.Trim() ?? string.Empty : string.Empty;

        /// <summary>Tooltip text: id, name, then height and the flags that matter when building.</summary>
        public static string Describe(int id)
        {
            var text = new StringBuilder();
            text.Append(CultureInfo.InvariantCulture, $"0x{id:X4} ({id})");
            string name = NameOf(id);
            if (name.Length > 0)
            {
                text.Append("  ").Append(name);
            }

            if (!HasItemData(id))
            {
                return text.ToString();
            }

            ItemData data = TileData.ItemTable[id];
            text.AppendLine().Append(CultureInfo.InvariantCulture, $"Height {data.Height}");
            if ((data.Flags & TileFlag.Bridge) != 0)
            {
                text.Append(CultureInfo.InvariantCulture, $" (calc {data.CalcHeight})");
            }

            List<string> flags = BuildFlagNames(data.Flags);
            if (flags.Count > 0)
            {
                text.AppendLine();
                int lineLength = 0;
                for (int i = 0; i < flags.Count; i++)
                {
                    if (i > 0)
                    {
                        // Wraps long flag lists so the tooltip stays narrow.
                        if (lineLength + flags[i].Length > 40)
                        {
                            text.AppendLine();
                            lineLength = 0;
                        }
                        else
                        {
                            text.Append(", ");
                            lineLength += 2;
                        }
                    }

                    text.Append(flags[i]);
                    lineLength += flags[i].Length;
                }
            }

            return text.ToString();
        }
    }
}
