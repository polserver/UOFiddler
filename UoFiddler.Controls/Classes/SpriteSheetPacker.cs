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
using System.Drawing.Imaging;
using System.IO;
using System.Text.Json;

namespace UoFiddler.Controls.Classes
{
    /// <summary>
    /// Places images left to right in rows on a sprite sheet, starting a new row when the next image
    /// would pass the maximum width. Every image is surrounded by <c>spacing</c> pixels.
    /// </summary>
    public sealed class SpriteSheetLayout
    {
        private readonly int _maxWidth;
        private readonly int _spacing;
        private int _x;
        private int _y;
        private int _rowHeight;

        public SpriteSheetLayout(int maxWidth, int spacing)
        {
            _maxWidth = maxWidth;
            _spacing = Math.Max(0, spacing);
            _x = _spacing;
            _y = _spacing;
        }

        public int Width { get; private set; }

        public int Height { get; private set; }

        /// <summary>
        /// Starts a new row unless the current one is still empty.
        /// </summary>
        public void NewRow()
        {
            if (_x <= _spacing)
            {
                return;
            }

            _y += _rowHeight + _spacing;
            _x = _spacing;
            _rowHeight = 0;
        }

        /// <param name="wrap">Start a new row when the image would pass the maximum width.</param>
        public PackedRect Place(int width, int height, bool wrap = true)
        {
            // An image wider than the sheet still goes into an empty row rather than leaving a blank one
            if (wrap && _x > _spacing && _x + width > _maxWidth)
            {
                NewRow();
            }

            var rect = new PackedRect { X = _x, Y = _y, W = width, H = height };

            _rowHeight = Math.Max(_rowHeight, height);
            _x += width + _spacing;

            Width = Math.Max(Width, _x);
            Height = Math.Max(Height, _y + _rowHeight + _spacing);

            return rect;
        }
    }

    /// <summary>
    /// Shared sprite sheet work for packed animation and item exports: composing and reading the
    /// sheet image, the guide image and the JSON descriptor.
    /// </summary>
    public static class SpriteSheetPacker
    {
        public const string ImageFormatName = "RGBA8888";

        private static string _lastFolder;

        /// <summary>
        /// Folder of the last sheet saved or opened in this session, shared by all sheet dialogs so an
        /// import starts where the export went. Starts in the profile's output path.
        /// </summary>
        public static string LastFolder
        {
            get => string.IsNullOrEmpty(_lastFolder) ? Options.OutputPath : _lastFolder;
            set => _lastFolder = value;
        }

        private static readonly JsonSerializerOptions _writeOptions = new JsonSerializerOptions { WriteIndented = true };

        // Case-insensitive so item sheets written by the original PR (PascalCase names) still load
        private static readonly JsonSerializerOptions _readOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        /// <summary>
        /// Draws <paramref name="images"/> onto a transparent sheet at <paramref name="rects"/>.
        /// </summary>
        public static Bitmap Compose(int width, int height, IReadOnlyList<Bitmap> images, IReadOnlyList<PackedRect> rects)
        {
            var sheet = new Bitmap(Math.Max(1, width), Math.Max(1, height), PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(sheet))
            {
                g.Clear(Color.Transparent);
                for (int i = 0; i < images.Count; ++i)
                {
                    PackedRect rect = rects[i];
                    g.DrawImage(images[i], new Rectangle(rect.X, rect.Y, rect.W, rect.H));
                }
            }

            return sheet;
        }

        public static PackedMeta CreateMeta(string imageFile, Bitmap sheet)
        {
            return new PackedMeta
            {
                Image = Path.GetFileName(imageFile),
                Size = new PackedSize { W = sheet.Width, H = sheet.Height },
                Format = ImageFormatName
            };
        }

        public static void WriteJson<T>(string fileName, T value)
        {
            File.WriteAllText(fileName, JsonSerializer.Serialize(value, _writeOptions));
        }

        public static T ReadJson<T>(string fileName) where T : class
        {
            T value;
            try
            {
                value = JsonSerializer.Deserialize<T>(File.ReadAllText(fileName), _readOptions);
            }
            catch (JsonException ex)
            {
                throw new InvalidDataException($"{Path.GetFileName(fileName)} is not a valid packed sheet: {ex.Message}", ex);
            }

            return value ?? throw new InvalidDataException($"{Path.GetFileName(fileName)} is empty.");
        }

        /// <summary>
        /// Loads the sheet image named in <paramref name="meta"/>. Only the file name is used, so the image
        /// is always read from the JSON file's folder.
        /// </summary>
        public static Bitmap LoadSheet(string jsonFile, PackedMeta meta)
        {
            if (string.IsNullOrWhiteSpace(meta?.Image))
            {
                throw new InvalidDataException($"{Path.GetFileName(jsonFile)} does not name a sheet image.");
            }

            string imageFile = Path.Combine(Path.GetDirectoryName(jsonFile) ?? string.Empty, Path.GetFileName(meta.Image));
            if (!File.Exists(imageFile))
            {
                throw new FileNotFoundException($"Sheet image not found: {imageFile}", imageFile);
            }

            using (var loaded = new Bitmap(imageFile))
            {
                // Copy so the image file is not kept locked
                return loaded.Clone(new Rectangle(0, 0, loaded.Width, loaded.Height), PixelFormat.Format32bppArgb);
            }
        }

        public static bool IsInside(PackedRect rect, Bitmap sheet)
        {
            return rect != null && rect.W > 0 && rect.H > 0 && rect.X >= 0 && rect.Y >= 0 &&
                   rect.X + rect.W <= sheet.Width && rect.Y + rect.H <= sheet.Height;
        }

        /// <summary>
        /// Copies <paramref name="rect"/> out of <paramref name="sheet"/> as a 32bpp ARGB bitmap.
        /// </summary>
        public static Bitmap Crop(Bitmap sheet, PackedRect rect)
        {
            return sheet.Clone(new Rectangle(rect.X, rect.Y, rect.W, rect.H), PixelFormat.Format32bppArgb);
        }

        /// <summary>
        /// Converts <paramref name="rect"/> of <paramref name="sheet"/> to an ARGB1555 animation frame.
        /// Pixels under half alpha become transparent; black is kept visible as 0,0,8 because 0,0,0 is
        /// transparent in the client (same rule as the Animation Editor import).
        /// </summary>
        /// <param name="mirror">Flip the frame horizontally.</param>
        public static unsafe Bitmap CropAnimationFrame(Bitmap sheet, PackedRect rect, bool mirror = false)
        {
            var frame = new Bitmap(rect.W, rect.H, PixelFormat.Format16bppArgb1555);

            BitmapData src = sheet.LockBits(new Rectangle(rect.X, rect.Y, rect.W, rect.H), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData dst = frame.LockBits(new Rectangle(0, 0, rect.W, rect.H), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);
            try
            {
                for (int y = 0; y < rect.H; ++y)
                {
                    uint* srcLine = (uint*)((byte*)src.Scan0 + (y * src.Stride));
                    ushort* dstLine = (ushort*)((byte*)dst.Scan0 + (y * dst.Stride));

                    for (int x = 0; x < rect.W; ++x)
                    {
                        int dstX = mirror ? rect.W - 1 - x : x;
                        uint argb = srcLine[x];
                        if ((argb >> 24) < 0x80)
                        {
                            dstLine[dstX] = 0;
                            continue;
                        }

                        int color = 0x8000 | (int)(((argb >> 9) & 0x7C00) | ((argb >> 6) & 0x03E0) | ((argb >> 3) & 0x001F));
                        dstLine[dstX] = (ushort)(color == 0x8000 ? 0x8001 : color);
                    }
                }
            }
            finally
            {
                sheet.UnlockBits(src);
                frame.UnlockBits(dst);
            }

            return frame;
        }

        /// <summary>
        /// Saves a copy of <paramref name="sheet"/> with every rect outlined in red, a green cross on each
        /// anchor and each label in the rect's top-left corner.
        /// </summary>
        public static void SaveGuide(string fileName, Bitmap sheet, IReadOnlyList<PackedRect> rects, IReadOnlyList<Point?> anchors = null,
            IReadOnlyList<string> labels = null, Font font = null)
        {
            using (var guide = new Bitmap(sheet))
            using (var g = Graphics.FromImage(guide))
            using (var rectPen = new Pen(Color.Red, 1))
            using (var anchorPen = new Pen(Color.Lime, 1))
            {
                for (int i = 0; i < rects.Count; ++i)
                {
                    PackedRect rect = rects[i];
                    g.DrawRectangle(rectPen, rect.X, rect.Y, rect.W - 1, rect.H - 1);

                    if (anchors?[i] is { } anchor)
                    {
                        g.DrawLine(anchorPen, anchor.X - 3, anchor.Y, anchor.X + 3, anchor.Y);
                        g.DrawLine(anchorPen, anchor.X, anchor.Y - 3, anchor.X, anchor.Y + 3);
                    }

                    if (labels != null && font != null)
                    {
                        g.DrawString(labels[i], font, Brushes.Black, rect.X + 1, rect.Y + 1);
                    }
                }

                guide.Save(fileName, ImageFormat.Png);
            }
        }
    }
}
