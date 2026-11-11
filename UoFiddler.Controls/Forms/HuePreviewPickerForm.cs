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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml.Linq;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.UserControls.TileView;

namespace UoFiddler.Controls.Forms
{
    /// <summary>What a <see cref="HuePreviewPickerForm"/> lists.</summary>
    public enum HuePreviewPickKind
    {
        Item,
        Gump,
        Body,
    }

    /// <summary>
    /// Lets the hue editor's preview pick an item, gump or animation body from a grid of
    /// thumbnails, filtered by id or name.
    /// </summary>
    public partial class HuePreviewPickerForm : Form
    {
        private const int _captionHeight = 16;

        private readonly HuePreviewPickKind _kind;
        private readonly int _facing;
        private readonly int _initialId;
        private readonly List<int> _allIds;
        private readonly Dictionary<int, string> _names;
        private readonly Dictionary<int, Bitmap> _thumbnails = [];
        private List<int> _shownIds = [];

        public HuePreviewPickerForm(HuePreviewPickKind kind, int currentId, int facing)
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();

            _kind = kind;
            _facing = facing;
            _initialId = currentId;

            Text = kind switch
            {
                HuePreviewPickKind.Item => "Pick an item",
                HuePreviewPickKind.Gump => "Pick a gump",
                _ => "Pick an animation body",
            };

            int side = kind == HuePreviewPickKind.Item ? 80 : 110;
            tileView.TileSize = new Size(side, side + _captionHeight);
            tileView.BackColor = Options.PreviewBackgroundColor;
            tileView.TileBackgroundColor = Options.PreviewBackgroundColor;
            tileView.TileBorderColor = Options.RemoveTileBorder ? Color.Transparent : Color.Gray;
            tileView.TileFocusColor = Options.TileFocusColor;
            tileView.TileHighlightColor = Options.TileSelectionColor;

            (_allIds, _names) = kind switch
            {
                HuePreviewPickKind.Item => (ListItems(), null),
                HuePreviewPickKind.Gump => (ListGumps(), null),
                _ => (ListBodies(), LoadBodyNames()),
            };

            ApplyFilter();
        }

        /// <summary>The picked id, or -1.</summary>
        public int SelectedId { get; private set; } = -1;

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            // The constructor already focused the current id, before the grid had a size to
            // scroll in; focus it again now so it is scrolled into view.
            int index = _shownIds.IndexOf(_initialId);
            if (index >= 0)
            {
                tileView.FocusIndex = -1;
                tileView.FocusIndex = index;
            }

            ActiveControl = searchTextBox;
        }

        // ---- Lists ------------------------------------------------------------------------

        private static List<int> ListItems()
        {
            var ids = new List<int>();
            int max = Art.GetMaxItemId();
            for (int i = 0; i <= max; i++)
            {
                if (Art.IsValidStatic(i))
                {
                    ids.Add(i);
                }
            }

            return ids;
        }

        private static List<int> ListGumps()
        {
            var ids = new List<int>();
            int count = Gumps.GetCount();
            for (int i = 0; i < count; i++)
            {
                if (Gumps.IsValidIndex(i))
                {
                    ids.Add(i);
                }
            }

            return ids;
        }

        /// <summary>
        /// Bodies the preview can show: those with action 0, the same check the editor uses. MUL
        /// bodies are probed up to 0xFFF; UOP and mobtypes bodies are added by id.
        /// </summary>
        private static List<int> ListBodies()
        {
            var candidates = new SortedSet<int>();
            for (int body = 0; body < 0x1000; body++)
            {
                candidates.Add(body);
            }

            candidates.UnionWith(Animations.GetAllUopBodies());
            candidates.UnionWith(Animations.GetAllMobTypeBodies());

            var ids = new List<int>();
            foreach (int body in candidates)
            {
                if (body >= 0 && body < 0x10000 && Animations.IsActionDefined(body, 0, 0))
                {
                    ids.Add(body);
                }
            }

            return ids;
        }

        /// <summary>Body names from the Animations tab's Animationlist.xml, when there is one.</summary>
        private static Dictionary<int, string> LoadBodyNames()
        {
            var names = new Dictionary<int, string>();
            string fileName = Path.Combine(Options.AppDataPath, "Animationlist.xml");
            if (!File.Exists(fileName))
            {
                return names;
            }

            try
            {
                foreach (XElement element in XDocument.Load(fileName).Descendants())
                {
                    if (element.Name.LocalName is not ("Mob" or "Equip"))
                    {
                        continue;
                    }

                    string name = (string)element.Attribute("name");
                    if (!string.IsNullOrEmpty(name)
                        && int.TryParse((string)element.Attribute("body"), NumberStyles.Integer, CultureInfo.InvariantCulture, out int body))
                    {
                        names.TryAdd(body, name);
                    }
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Xml.XmlException)
            {
                // Names are optional; the grid still lists every body by id.
            }

            return names;
        }

        private string GetName(int id)
        {
            if (_kind == HuePreviewPickKind.Item)
            {
                return TileData.ItemTable != null && id < TileData.ItemTable.Length ? TileData.ItemTable[id].Name : null;
            }

            return _names != null && _names.TryGetValue(id, out string name) ? name : null;
        }

        private string FormatId(int id) => _kind == HuePreviewPickKind.Body ? id.ToString() : $"0x{id:X4}";

        // ---- Filter -----------------------------------------------------------------------

        private void OnSearchChanged(object sender, EventArgs e) => ApplyFilter();

        private void OnSearchKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode is Keys.Down or Keys.PageDown && _shownIds.Count > 0)
            {
                tileView.Focus();
                if (tileView.FocusIndex < 0)
                {
                    tileView.FocusIndex = 0;
                }

                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        /// <summary>
        /// Keeps ids that start with the typed number (hex with or without 0x, or decimal) and
        /// names that contain the typed text.
        /// </summary>
        private void ApplyFilter()
        {
            int keep = tileView.FocusIndex >= 0 && tileView.FocusIndex < _shownIds.Count ? _shownIds[tileView.FocusIndex] : _initialId;

            string text = searchTextBox.Text.Trim();
            if (text.Length == 0)
            {
                _shownIds = _allIds;
            }
            else
            {
                bool hexPrefix = text.StartsWith("0x", StringComparison.OrdinalIgnoreCase);
                string hex = hexPrefix ? text[2..] : text;
                bool isHex = hex.Length > 0 && int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _);
                bool isDecimal = !hexPrefix && int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out _);

                _shownIds = [];
                foreach (int id in _allIds)
                {
                    if ((isHex && id.ToString("X", CultureInfo.InvariantCulture).StartsWith(hex, StringComparison.OrdinalIgnoreCase))
                        || (isDecimal && id.ToString(CultureInfo.InvariantCulture).StartsWith(text, StringComparison.Ordinal))
                        || (!hexPrefix && GetName(id)?.Contains(text, StringComparison.OrdinalIgnoreCase) == true))
                    {
                        _shownIds.Add(id);
                    }
                }
            }

            // Changing the size clears the selection but can leave the focus index as it was, and
            // setting the same focus index again does nothing; reset it so the tile is reselected.
            tileView.FocusIndex = -1;
            tileView.VirtualListSize = _shownIds.Count;
            if (_shownIds.Count == 0)
            {
                // TileView keeps the old scroll height for an empty list.
                tileView.AutoScrollMinSize = Size.Empty;
            }

            int index = _shownIds.IndexOf(keep);
            if (index < 0 && _shownIds.Count > 0 && text.Length > 0)
            {
                index = 0;
            }

            if (index >= 0)
            {
                tileView.FocusIndex = index;
            }

            countLabel.Text = _shownIds.Count == _allIds.Count
                ? $"{_allIds.Count} {KindName(_allIds.Count)}"
                : $"{_shownIds.Count} of {_allIds.Count} {KindName(_allIds.Count)}";
            tileView.Invalidate();
            UpdateStatus();
        }

        private string KindName(int count) => (_kind, count == 1) switch
        {
            (HuePreviewPickKind.Item, true) => "item",
            (HuePreviewPickKind.Item, false) => "items",
            (HuePreviewPickKind.Gump, true) => "gump",
            (HuePreviewPickKind.Gump, false) => "gumps",
            (_, true) => "body",
            _ => "bodies",
        };

        // ---- Grid -------------------------------------------------------------------------

        /// <summary>
        /// Item art is used as it is. Gumps and animation frames are cropped to their visible
        /// pixels (paperdoll gumps sit small in a large transparent canvas) and kept as our own
        /// copies, since Gumps and Animations cache and may dispose theirs.
        /// </summary>
        private Bitmap GetThumbnail(int id)
        {
            if (_kind == HuePreviewPickKind.Item)
            {
                return Art.GetStatic(id);
            }

            if (_thumbnails.TryGetValue(id, out Bitmap cached))
            {
                return cached;
            }

            Bitmap source;
            if (_kind == HuePreviewPickKind.Gump)
            {
                source = Gumps.GetGump(id);
            }
            else
            {
                int hue = 0;
                AnimationFrame[] frames = Animations.GetAnimation(id, 0, _facing, ref hue, false, true);
                source = frames is { Length: > 0 } ? frames[0]?.Bitmap : null;
            }

            Bitmap thumbnail = source == null ? null : CropToContent(source);
            _thumbnails[id] = thumbnail;
            return thumbnail;
        }

        private static Bitmap CropToContent(Bitmap source)
        {
            int width = source.Width;
            int height = source.Height;
            if (width <= 0 || height <= 0)
            {
                return null;
            }

            int left = width;
            int top = height;
            int right = -1;
            int bottom = -1;
            BitmapData data = source.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                var row = new int[width];
                for (int y = 0; y < height; y++)
                {
                    Marshal.Copy(data.Scan0 + (y * data.Stride), row, 0, width);
                    for (int x = 0; x < width; x++)
                    {
                        if ((row[x] >>> 24) == 0)
                        {
                            continue;
                        }

                        left = Math.Min(left, x);
                        right = Math.Max(right, x);
                        top = Math.Min(top, y);
                        bottom = Math.Max(bottom, y);
                    }
                }
            }
            finally
            {
                source.UnlockBits(data);
            }

            Rectangle bounds = right < 0 ? new Rectangle(0, 0, width, height) : Rectangle.FromLTRB(left, top, right + 1, bottom + 1);
            return source.Clone(bounds, PixelFormat.Format32bppArgb);
        }

        private void OnDrawTile(object sender, TileViewControl.DrawTileListItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= _shownIds.Count)
            {
                return;
            }

            int id = _shownIds[e.Index];
            var tile = new Rectangle(
                new Point(e.Bounds.X + tileView.TilePadding.Left, e.Bounds.Y + tileView.TilePadding.Top),
                tileView.TileSize);
            bool selected = tileView.SelectedIndices.Contains(e.Index);

            using Region previousClip = e.Graphics.Clip;
            using var clip = new Region(tile);
            e.Graphics.Clip = clip;

            if (!selected)
            {
                using var background = new SolidBrush(Options.PreviewBackgroundColor);
                e.Graphics.FillRectangle(background, tile);
            }

            var imageArea = new Rectangle(tile.X + 2, tile.Y + 2, tile.Width - 4, tile.Height - _captionHeight - 4);
            Bitmap bitmap = GetThumbnail(id);
            if (bitmap != null && bitmap.Width > 0 && bitmap.Height > 0)
            {
                // Shown at their own size; only images too big for the tile shrink to fit.
                double scale = Math.Min(1.0, Math.Min(imageArea.Width / (double)bitmap.Width, imageArea.Height / (double)bitmap.Height));
                int width = Math.Max(1, (int)(bitmap.Width * scale));
                int height = Math.Max(1, (int)(bitmap.Height * scale));
                var target = new Rectangle(
                    imageArea.X + ((imageArea.Width - width) / 2),
                    imageArea.Y + ((imageArea.Height - height) / 2),
                    width,
                    height);
                e.Graphics.InterpolationMode = scale < 1 ? InterpolationMode.HighQualityBicubic : InterpolationMode.NearestNeighbor;
                e.Graphics.DrawImage(bitmap, target);
            }

            Color textColor = selected
                ? SystemColors.HighlightText
                : Options.PreviewBackgroundColor.GetBrightness() < 0.5f ? Color.White : Color.Black;
            var captionArea = new RectangleF(tile.X, tile.Bottom - _captionHeight, tile.Width, _captionHeight);
            using var textBrush = new SolidBrush(textColor);
            using var format = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter,
                FormatFlags = StringFormatFlags.NoWrap,
            };
            e.Graphics.DrawString(FormatId(id), Font, textBrush, captionArea, format);

            e.Graphics.Clip = previousClip;
        }

        private void OnTileFocusChanged(object sender, TileViewControl.ListViewFocusedItemSelectionChangedEventArgs e) => UpdateStatus();

        private void OnTileDoubleClick(object sender, MouseEventArgs e)
        {
            int index = tileView.GetIndexAtLocation(e.Location);
            if (index >= 0 && index < _shownIds.Count)
            {
                tileView.FocusIndex = index;
                Accept();
            }
        }

        private void OnTileKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Accept();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void UpdateStatus()
        {
            int index = tileView.FocusIndex;
            if (index < 0 || index >= _shownIds.Count)
            {
                statusLabel.Text = _shownIds.Count == 0 ? "Nothing matches." : "Click a tile to pick it, double-click to use it.";
                okButton.Enabled = false;
                return;
            }

            int id = _shownIds[index];
            string name = GetName(id);
            string ids = _kind == HuePreviewPickKind.Body ? $"Body {id} (0x{id:X})" : $"0x{id:X4} ({id})";
            statusLabel.Text = string.IsNullOrEmpty(name) ? ids : $"{ids}  {name}";
            okButton.Enabled = true;
        }

        private void OnClickOk(object sender, EventArgs e) => Accept();

        private void Accept()
        {
            int index = tileView.FocusIndex;
            if (index < 0 || index >= _shownIds.Count)
            {
                return;
            }

            SelectedId = _shownIds[index];
            DialogResult = DialogResult.OK;
            Close();
        }

        private void DisposeThumbnails()
        {
            foreach (Bitmap bitmap in _thumbnails.Values)
            {
                bitmap?.Dispose();
            }

            _thumbnails.Clear();
        }
    }
}
