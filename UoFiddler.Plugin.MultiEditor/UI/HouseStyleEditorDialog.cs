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
using System.Windows.Forms;
using UoFiddler.Plugin.MultiEditor.Classes;

namespace UoFiddler.Plugin.MultiEditor.UI
{
    /// <summary>
    /// Edits the house styles: their names, groups, families, order and pieces. Changes go to the
    /// user's copy of the style file (<see cref="HouseStyleConfig.UserPath"/>), which is laid over
    /// the shipped one, and the styles are read again. Only what was changed is written.
    /// </summary>
    internal sealed partial class HouseStyleEditorDialog : Form
    {
        /// <summary>A style as the dialog shows and changes it.</summary>
        private sealed class Row
        {
            public HouseStyle Style;
            public string Name;
            public string Group;
            public string Family;
            public bool Hidden;
            public string Layout;
            public List<(string Role, ushort Id)> Pieces;
            public bool MetaChanged;
            public bool PiecesChanged;

            public override string ToString() => (Hidden ? "(hidden) " : string.Empty) + Name + (MetaChanged || PiecesChanged ? " *" : string.Empty);
        }

        private static readonly HousePartKind[] _kinds =
        {
            HousePartKind.Wall, HousePartKind.Floor, HousePartKind.Roof, HousePartKind.Door, HousePartKind.Stair, HousePartKind.Misc
        };

        private readonly HouseStyles _styles;
        private readonly Func<int> _currentTile;
        private readonly TileBrowserThumbnails _thumbnails = new TileBrowserThumbnails();
        private readonly Dictionary<HousePartKind, List<Row>> _rows = new Dictionary<HousePartKind, List<Row>>();
        private readonly HashSet<HousePartKind> _reordered = new HashSet<HousePartKind>();
        private bool _updating;

        public HouseStyleEditorDialog()
        {
            InitializeComponent();
        }

        public HouseStyleEditorDialog(HouseStyles styles, HouseStyle start, Func<int> currentTile) : this()
        {
            _styles = styles;
            _currentTile = currentTile;
            _fileHint.Text = "Your changes are kept in " + (HouseStyleConfig.UserPath ?? "(no profile folder)");
            foreach (HousePartKind kind in _kinds)
            {
                _kind.Items.Add(kind);
            }

            LoadRows();
            Choose(start?.Kind ?? HousePartKind.Wall, start?.Anchor ?? 0);
            Disposed += (_, _) =>
            {
                _thumbnails.Dispose();
                _preview.Image?.Dispose();
            };
        }

        /// <summary>True when the styles were saved and read again while the dialog was open.</summary>
        public bool Saved { get; private set; }

        private HousePartKind Kind => _kind.SelectedItem is HousePartKind kind ? kind : HousePartKind.Wall;

        private Row Current => _styleList.SelectedItem as Row;

        private bool HasChanges => _reordered.Count > 0 || _rows.Values.Any(rows => rows.Any(row => row.MetaChanged || row.PiecesChanged));

        private void LoadRows()
        {
            _rows.Clear();
            _reordered.Clear();
            foreach (HousePartKind kind in _kinds)
            {
                _rows[kind] = new List<Row>();
            }

            foreach (HouseStyle style in _styles.Styles)
            {
                if (!_rows.TryGetValue(style.Kind, out List<Row> rows))
                {
                    continue;
                }

                rows.Add(new Row
                {
                    Style = style, Name = style.Name, Group = style.Group, Family = style.Family, Hidden = style.Hidden,
                    Layout = style.Paired ? HouseStyleEntry.LayoutPaired : style.CycleSouth > 0 ? HouseStyleEntry.LayoutCycle : HouseStyleEntry.LayoutRun,
                    Pieces = style.Slots.Select(piece => (piece.Key, piece.Value)).ToList()
                });
            }
        }

        private void Choose(HousePartKind kind, ushort anchor)
        {
            _kind.SelectedItem = kind;
            ShowKind();
            int index = _rows[Kind].FindIndex(row => anchor != 0 && row.Style.Anchor == anchor);
            if (_styleList.Items.Count > 0)
            {
                _styleList.SelectedIndex = Math.Max(0, index);
            }
        }

        private void ShowKind()
        {
            _updating = true;
            try
            {
                _styleList.BeginUpdate();
                _styleList.Items.Clear();
                foreach (Row row in _rows[Kind])
                {
                    _styleList.Items.Add(row);
                }

                _styleList.EndUpdate();

                // The roles the styles of this kind use, to choose from.
                _role.Items.Clear();
                foreach (string role in _rows[Kind].SelectMany(row => row.Pieces.Select(piece => piece.Role)).Distinct().Take(80))
                {
                    _role.Items.Add(role);
                }

                _layout.Enabled = _layoutLabel.Enabled = Kind == HousePartKind.Wall;
            }
            finally
            {
                _updating = false;
            }

            ShowRow();
        }

        private void ShowRow()
        {
            Row row = Current;
            _updating = true;
            try
            {
                _name.Enabled = _group.Enabled = _family.Enabled = _hidden.Enabled = _pieces.Enabled = _reset.Enabled = row != null;
                _setPiece.Enabled = _removePiece.Enabled = _role.Enabled = _pieceId.Enabled = _fromBrowser.Enabled = row != null;
                _name.Text = row?.Name ?? string.Empty;
                _group.Text = row?.Group ?? string.Empty;
                _family.Text = row?.Family ?? string.Empty;
                _hidden.Checked = row?.Hidden ?? false;
                _layout.SelectedItem = row?.Layout ?? HouseStyleEntry.LayoutRun;
                _up.Enabled = row != null && _styleList.SelectedIndex > 0;
                _down.Enabled = row != null && _styleList.SelectedIndex < _styleList.Items.Count - 1;
                ShowPieces(row, null);
            }
            finally
            {
                _updating = false;
            }

            DrawPreview();
        }

        private void ShowPieces(Row row, string selectRole)
        {
            _pieces.BeginUpdate();
            _pieces.Items.Clear();
            _pieceImages.Images.Clear();
            if (row != null)
            {
                foreach ((string role, ushort id) in row.Pieces)
                {
                    string key = id.ToString("X4", CultureInfo.InvariantCulture);
                    if (!_pieceImages.Images.ContainsKey(key) && _thumbnails.Get(id, 32) is { } thumb)
                    {
                        _pieceImages.Images.Add(key, thumb);
                    }

                    var item = new ListViewItem(new[] { role, "0x" + key, TileBrowserThumbnails.NameOf(id) }) { ImageKey = key, Tag = role };
                    _pieces.Items.Add(item);
                    item.Selected = role == selectRole;
                }
            }

            _pieces.EndUpdate();
        }

        private void DrawPreview()
        {
            Row row = Current;
            int width = Math.Max(120, _preview.ClientSize.Width), height = Math.Max(120, _preview.ClientSize.Height);
            var image = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(image))
            {
                g.Clear(Color.FromArgb(40, 44, 52));
                if (row != null && row.Pieces.Count > 0)
                {
                    HouseStyle style = HouseStyles.Compose(Kind, row.Group, row.Name, row.Pieces, row.Layout);
                    if (Kind == HousePartKind.Wall)
                    {
                        DrawRoom(g, style, width);
                    }

                    DrawPieces(g, row, Kind == HousePartKind.Wall ? 300 : 8, width, height);
                }
            }

            Image old = _preview.Image;
            _preview.Image = image;
            old?.Dispose();
        }

        /// <summary>A small room in the style, with a window in each wall where the style has one.</summary>
        private static void DrawRoom(Graphics g, HouseStyle style, int width)
        {
            var region = new HashSet<(int X, int Y)>();
            for (int x = 1; x <= 5; ++x)
            {
                for (int y = 1; y <= 4; ++y)
                {
                    region.Add((x, y));
                }
            }

            var south = new HashSet<(int X, int Y)>();
            var east = new HashSet<(int X, int Y)>();
            HouseDraft.Outline(region, south, east);
            var windows = new HashSet<(int X, int Y)> { (3, 4), (5, 2), (3, 0), (0, 2) };
            var tiles = new List<(ushort Id, int X, int Y)>();
            for (int x = 0; x <= 5; ++x)
            {
                for (int y = 0; y <= 4; ++y)
                {
                    string column = WallPlanner.PieceFor(style, south, east, x, y);
                    if (column == null)
                    {
                        continue;
                    }

                    ushort id = style.Get(column);
                    if (windows.Contains((x, y)) && column != "Corner" && column != "Post")
                    {
                        ushort window = style.Get(south.Contains((x, y)) ? "WindowS" : "WindowE");
                        id = window != 0 ? window : id;
                    }

                    if (id != 0)
                    {
                        tiles.Add((id, x, y));
                    }
                }
            }

            int cx = (width / 2) - 10, cy = 70;
            foreach ((ushort id, int x, int y) in tiles.OrderBy(t => t.X + t.Y).ThenBy(t => t.X))
            {
                Bitmap art = Ultima.Art.GetStatic(id);
                if (art != null)
                {
                    g.DrawImage(art, cx + ((x - y) * 22) - (art.Width / 2), cy + ((x + y) * 22) + 44 - art.Height);
                }
            }
        }

        /// <summary>The style's pieces in rows, each with its role under it.</summary>
        private void DrawPieces(Graphics g, Row row, int top, int width, int height)
        {
            const int cell = 62;
            int x = 6, y = top;
            foreach (IGrouping<ushort, (string Role, ushort Id)> piece in row.Pieces.GroupBy(p => p.Id))
            {
                if (y + cell > height)
                {
                    break;
                }

                if (_thumbnails.Get(piece.Key, 44) is { } thumb)
                {
                    g.DrawImage(thumb, x + ((cell - 8 - thumb.Width) / 2), y + (44 - thumb.Height));
                }

                string label = piece.First().Role + (piece.Count() > 1 ? " +" + (piece.Count() - 1).ToString(CultureInfo.InvariantCulture) : string.Empty);
                TextRenderer.DrawText(g, label, Font, new Rectangle(x - 2, y + 45, cell, 15), Color.Gainsboro, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
                x += cell;
                if (x + cell > width)
                {
                    x = 6;
                    y += cell + 4;
                }
            }
        }

        private void Kind_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_updating)
            {
                ShowKind();
                if (_styleList.Items.Count > 0)
                {
                    _styleList.SelectedIndex = 0;
                }
            }
        }

        private void StyleList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_updating)
            {
                ShowRow();
            }
        }

        /// <summary>Shows a row's line in the list again after it changed.</summary>
        private void RefreshLine()
        {
            int index = _styleList.SelectedIndex;
            if (index < 0)
            {
                return;
            }

            _updating = true;
            try
            {
                _styleList.Items[index] = _styleList.Items[index];
            }
            finally
            {
                _updating = false;
            }
        }

        private void Meta_Changed(object sender, EventArgs e)
        {
            Row row = Current;
            if (_updating || row == null)
            {
                return;
            }

            string layout = _layout.SelectedItem as string ?? HouseStyleEntry.LayoutRun;
            bool redraw = layout != row.Layout;
            (row.Name, row.Group, row.Family, row.Hidden, row.Layout) = (_name.Text.Trim(), _group.Text.Trim(), _family.Text.Trim(), _hidden.Checked, layout);
            row.MetaChanged = true;
            RefreshLine();
            if (redraw)
            {
                DrawPreview();
            }
        }

        private void Pieces_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_pieces.SelectedItems.Count == 1)
            {
                _role.Text = _pieces.SelectedItems[0].SubItems[0].Text;
                _pieceId.Text = _pieces.SelectedItems[0].SubItems[1].Text;
            }
        }

        private void FromBrowser_Click(object sender, EventArgs e)
        {
            int tile = _currentTile?.Invoke() ?? -1;
            if (tile >= 0)
            {
                _pieceId.Text = "0x" + tile.ToString("X4", CultureInfo.InvariantCulture);
            }
            else
            {
                _status.Text = "No tile is chosen in the tile browser.";
            }
        }

        private void SetPiece_Click(object sender, EventArgs e)
        {
            Row row = Current;
            string role = _role.Text.Trim();
            if (row == null)
            {
                return;
            }

            if (role.Length == 0 || role.Any(char.IsWhiteSpace))
            {
                _status.Text = "Give the piece a role: South1, Corner, WindowS ...";
                return;
            }

            if (!HouseStyleConfig.TryParseId(_pieceId.Text, out ushort id) || id == 0 || id >= Ultima.TileData.ItemTable.Length || !Ultima.Art.IsValidStatic(id))
            {
                _status.Text = "That is not a tile this client has art for. Ids are written as 0x1234.";
                return;
            }

            int index = row.Pieces.FindIndex(piece => piece.Role == role);
            if (index >= 0)
            {
                row.Pieces[index] = (role, id);
            }
            else
            {
                row.Pieces.Add((role, id));
            }

            row.PiecesChanged = true;
            _status.Text = string.Empty;
            RefreshLine();
            ShowPieces(row, role);
            DrawPreview();
        }

        private void RemovePiece_Click(object sender, EventArgs e)
        {
            Row row = Current;
            if (row == null || _pieces.SelectedItems.Count != 1)
            {
                return;
            }

            string role = (string)_pieces.SelectedItems[0].Tag;
            if (row.Pieces.Count <= 1)
            {
                _status.Text = "A style needs a piece. Hide the style to take it out of the lists.";
                return;
            }

            row.Pieces.RemoveAll(piece => piece.Role == role);
            row.PiecesChanged = true;
            RefreshLine();
            ShowPieces(row, null);
            DrawPreview();
        }

        private void Up_Click(object sender, EventArgs e) => MoveRow(-1);

        private void Down_Click(object sender, EventArgs e) => MoveRow(1);

        private void MoveRow(int by)
        {
            List<Row> rows = _rows[Kind];
            int index = _styleList.SelectedIndex, to = index + by;
            if (index < 0 || to < 0 || to >= rows.Count)
            {
                return;
            }

            (rows[index], rows[to]) = (rows[to], rows[index]);
            _reordered.Add(Kind);
            _updating = true;
            try
            {
                (_styleList.Items[index], _styleList.Items[to]) = (_styleList.Items[to], _styleList.Items[index]);
                _styleList.SelectedIndex = to;
            }
            finally
            {
                _updating = false;
            }

            _up.Enabled = to > 0;
            _down.Enabled = to < rows.Count - 1;
        }

        private void Reset_Click(object sender, EventArgs e)
        {
            Row row = Current;
            if (row == null || row.Style.Anchor == 0 ||
                MessageBox.Show(this, $"Drop your changes to \"{row.Style.Name}\"?" + (HasChanges ? "\r\nChanges not saved yet are lost too." : string.Empty),
                    Text, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
            {
                return;
            }

            try
            {
                HouseStyleConfig user = HouseStyleConfig.LoadFile(HouseStyleConfig.UserPath);
                user.Styles.RemoveAll(entry => entry.Kind == row.Style.Kind && entry.Anchor == row.Style.Anchor);
                user.Save(HouseStyleConfig.UserPath);
                Reload(row.Style.Kind, row.Style.Anchor);
                _status.Text = "The style is as shipped again.";
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
            {
                MessageBox.Show(this, "The style file could not be written: " + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Save_Click(object sender, EventArgs e)
        {
            if (!HasChanges)
            {
                _status.Text = "Nothing was changed.";
                return;
            }

            if (HouseStyleConfig.UserPath == null)
            {
                MessageBox.Show(this, "There is no profile folder to keep the changes in.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Row current = Current;
            int skipped = 0;
            try
            {
                HouseStyleConfig user = HouseStyleConfig.LoadFile(HouseStyleConfig.UserPath);
                foreach (Row row in _rows.Values.SelectMany(rows => rows).Where(row => row.MetaChanged || row.PiecesChanged))
                {
                    // A style is known by its anchor piece; one without (every piece listed by an earlier style) cannot be named.
                    if (row.Style.Anchor == 0)
                    {
                        ++skipped;
                        continue;
                    }

                    HouseStyleEntry entry = user.Styles.FirstOrDefault(other => other.Kind == row.Style.Kind && other.Anchor == row.Style.Anchor && !other.IsNew);
                    if (entry == null)
                    {
                        entry = new HouseStyleEntry { Kind = row.Style.Kind, Anchor = row.Style.Anchor };
                        user.Styles.Add(entry);
                    }

                    if (row.MetaChanged)
                    {
                        (entry.Name, entry.Group, entry.Family, entry.Hidden) = (row.Name, row.Group, row.Family, row.Hidden);
                        if (row.Style.Kind == HousePartKind.Wall)
                        {
                            entry.Layout = row.Layout;
                        }
                    }

                    if (row.PiecesChanged)
                    {
                        entry.Pieces = new List<(string Role, ushort Id)>(row.Pieces);
                    }
                }

                foreach (HousePartKind kind in _reordered)
                {
                    user.Order[kind] = _rows[kind].Where(row => row.Style.Anchor != 0).Select(row => row.Style.Anchor).ToList();
                }

                user.Save(HouseStyleConfig.UserPath);
                Reload(current?.Style.Kind ?? Kind, current?.Style.Anchor ?? 0);
                _status.Text = skipped == 0 ? "Saved." : $"Saved. {skipped} style(s) share all their pieces with others and could not be changed.";
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
            {
                MessageBox.Show(this, "The style file could not be written: " + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>Reads the styles again and shows them, on the style that was shown.</summary>
        private void Reload(HousePartKind kind, ushort anchor)
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                _styles.Reload();
                Saved = true;
                LoadRows();
                Choose(kind, anchor);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void Close_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Dialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (HasChanges && MessageBox.Show(this, "Close without saving your changes to the styles?", Text, MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Question) != DialogResult.OK)
            {
                e.Cancel = true;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (_styles != null && IsHandleCreated)
            {
                DrawPreview();
            }
        }
    }
}
