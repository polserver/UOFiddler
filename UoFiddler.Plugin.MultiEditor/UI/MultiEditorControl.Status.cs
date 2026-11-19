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
using System.Linq;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Plugin.MultiEditor.Editing;
using UoFiddler.Plugin.MultiEditor.Model;

namespace UoFiddler.Plugin.MultiEditor.UI
{
    public partial class MultiEditorControl
    {
        // What the status bar says for a while in place of the tool's hint.
        private string _told;
        private long _messageUntil;

        /// <summary>Says something in the status bar. It stays a few seconds, or until the next command, whatever the mouse does.</summary>
        private void Tell(string text)
        {
            _told = text;
            _messageUntil = Environment.TickCount64 + 5000;
            _statusHint.Text = text;
        }

        /// <summary>What the tool in use does to the open house draft.</summary>
        private static string DraftHint(EditorSession session)
        {
            string storey = $"House draft, storey {session.DraftStorey + 1} (the floor buttons choose it): ";
            return session.Tool switch
            {
                ToolKind.Wall => storey + "drag to add walls, Ctrl+drag to take walls away, click a wall for a window.",
                ToolKind.Door => storey + "click a wall to hang the chosen door, again to turn it, Ctrl+click to take it out.",
                ToolKind.Floor => storey + "drag an area to lay it in the chosen floor style.",
                ToolKind.Stair => storey + "press on the bottom step and drag the way the stairs climb; Ctrl+click a bottom step removes them.",
                _ => "A house draft is open. This tool works on the multi; House, Walls, Doors, Floors and Stairs work on the draft."
            };
        }

        private void Detach()
        {
            if (_active == null)
            {
                return;
            }

            EditorSession session = _active.Session;
            session.CancelTool();
            session.DocumentChanged -= OnDocumentChanged;
            session.SelectionChanged -= OnSelectionChanged;
            session.HoverChanged -= UpdateStatus;
            session.CursorCellChanged -= UpdateStatus;
            session.ToolChanged -= OnToolChanged;
            session.DrawTileChanged -= OnDrawTileChanged;
            session.ViewChanged -= OnViewChanged;
            session.TileUsed -= _browser.RecordRecent;
            _active = null;
        }

        private void Activate(int index)
        {
            EditorDocument document = index >= 0 && index < _documents.Count ? _documents[index] : null;
            if (document == _active && document != null)
            {
                return;
            }

            Detach();
            _active = document;
            _emptyHint.Visible = document == null;
            _docTabs.Visible = _documents.Count > 0;

            if (document == null)
            {
                _canvas.Show(null, null);
                _inspector.Bind(null);
                _housePanel.Bind(null);
            }
            else
            {
                EditorSession session = document.Session;
                session.DocumentChanged += OnDocumentChanged;
                session.SelectionChanged += OnSelectionChanged;
                session.HoverChanged += UpdateStatus;
                session.CursorCellChanged += UpdateStatus;
                session.ToolChanged += OnToolChanged;
                session.DrawTileChanged += OnDrawTileChanged;
                session.ViewChanged += OnViewChanged;
                session.TileUsed += _browser.RecordRecent;

                _canvas.Show(session, document.Camera);

                _inspector.Bind(session);
                _housePanel.Bind(session);
                if (session.DrawTileId != 0)
                {
                    _browser.SetCurrentTile(session.DrawTileId);
                }
            }

            SyncOptions();
            UpdateTitle();
            UpdateStatus();
            UpdateZoomLabel();
            _commands.RefreshStates();
        }

        // ---------------------------------------------------------------- keeping the UI in line

        private void OnDocumentChanged()
        {
            SyncLevels();
            UpdateTitle();
            UpdateStatus();
            _commands.RefreshStates();
        }

        private void OnSelectionChanged()
        {
            UpdateStatus();
            _commands.RefreshStates();
        }

        private void OnToolChanged()
        {
            SyncOptions();
            UpdateStatus();
            _commands.RefreshStates();
        }

        private void OnDrawTileChanged()
        {
            _browser.SetCurrentTile(Session.DrawTileId);
            SyncOptions();
        }

        private void OnViewChanged()
        {
            SyncLevels();
            _commands.RefreshStates();
            UpdateStatus();
        }

        /// <summary>
        /// One button per floor in the options bar, the active one pressed. Every floor has a
        /// button whether it holds tiles or not.
        /// </summary>
        private void SyncLevels()
        {
            var names = new List<(string Short, string Tip)> { ("All", "All floors (Ctrl+Home)") };
            if (Session != null)
            {
                names.AddRange(Session.Levels.Select(l => (l.ShortName, l.Description)));
            }

            if (_floorButtons.Count != names.Count || !_floorButtons.Select(b => b.Text).SequenceEqual(names.Select(n => n.Short)))
            {
                foreach (ToolStripButton old in _floorButtons)
                {
                    _optionsStrip.Items.Remove(old);
                    old.Dispose();
                }

                _floorButtons.Clear();
                int at = _optionsStrip.Items.IndexOf(_floorLabel) + 1;
                for (int i = 0; i < names.Count; i++)
                {
                    int index = i;
                    var button = new ToolStripButton(names[i].Short)
                    {
                        DisplayStyle = ToolStripItemDisplayStyle.Text,
                        ToolTipText = names[i].Tip,
                        AutoToolTip = false,
                        AutoSize = false,
                        Width = names[i].Short.Length > 2 ? 40 : 26,
                        Margin = new Padding(1, 1, 0, 2)
                    };
                    button.Click += (_, _) =>
                    {
                        if (Session != null)
                        {
                            Session.LevelIndex = index;
                            _canvas.Focus();
                        }
                    };
                    _floorButtons.Add(button);
                    _optionsStrip.Items.Insert(at + i, button);
                }
            }

            int active = Session?.LevelIndex ?? 0;
            for (int i = 0; i < _floorButtons.Count; i++)
            {
                _floorButtons[i].Checked = i == active;
                _floorButtons[i].Enabled = Session != null;
                _floorButtons[i].ToolTipText = i < names.Count ? names[i].Tip : string.Empty;
            }
        }

        private void SyncOptions()
        {
            SyncLevels();
            SyncRoofOptions();
            _syncing = true;
            try
            {
                EditorSession session = Session;
                bool has = session != null;
                _zStepCombo.Enabled = has;
                if (!has)
                {
                    _toolLabel.Text = string.Empty;
                    _drawTileLabel.Text = string.Empty;
                    return;
                }

                _toolLabel.Text = session.Pasting ? session.StampLabel : _toolCommands[session.Tool].Text;
                int zIndex = _zStepCombo.IndexOf(session.ZStep.ToString());
                _zStepCombo.SelectedIndex = Math.Max(0, zIndex);

                if (session.Tool == ToolKind.FloorPattern)
                {
                    _drawTileLabel.Text = session.FloorPattern != null ? $"Pattern: {session.FloorPattern.Name}" : "Pattern: pick one in the House page";
                }
                else if (IsHouseTool(session.Tool) && session.Tool != ToolKind.Foundation)
                {
                    _drawTileLabel.Text = session.HouseStyle != null ? $"Style: {session.HouseStyle.Name}" : "Style: pick one in the House page";
                }
                else if (session.Tool == ToolKind.Foundation)
                {
                    _drawTileLabel.Text = "Foundation";
                }
                else
                {
                    ushort id = session.DrawTileId;
                    _drawTileLabel.Text = id == 0 ? "Tile: pick one in the tile browser" : $"Tile: 0x{id:X4} {TileName(id)}";
                }
            }
            finally
            {
                _syncing = false;
            }
        }

        private static string TileName(ushort id)
        {
            return id < TileData.ItemTable.Length ? TileData.ItemTable[id].Name : string.Empty;
        }

        private void UpdateTitle()
        {
            for (int i = 0; i < _documents.Count; i++)
            {
                EditorSession session = _documents[i].Session;
                string text = session.Title + (session.Modified ? " *" : string.Empty);
                if (_docTabs.TabPages[i].Text != text)
                {
                    _docTabs.TabPages[i].Text = text;
                }
            }
        }

        private void UpdateZoomLabel()
        {
            if (_zoomLabel != null)
            {
                _zoomLabel.Text = $"{_canvas.Camera.Zoom * 100:0}%";
            }
        }

        private void UpdateStatus()
        {
            EditorSession session = Session;
            if (session == null)
            {
                _statusHint.Text = string.Empty;
                _statusCell.Text = _statusPlane.Text = _statusHover.Text = _statusSelection.Text = _statusSize.Text = string.Empty;
                return;
            }

            _statusHint.Text = _housePick != null ? _pickHint
                : _told != null && Environment.TickCount64 < _messageUntil ? _told
                : session.PreviewMode ? "Preview: editing is off. Right click picks the ground. F5 returns to the editor."
                : session.PreviewingRepeat ? "The copies are shown as they would be placed."
                : session.Pasting ? "Click to place the tiles, as often as you like. Esc or right click stops."
                : session.ShapingRoof && session.WingTool.FloatingWing ? "Click to drop the wing. Esc or right click takes it away."
                : session.ShapingRoof ? "Shaping: drag a wing to move it, a marked edge of the selected wing to resize it, anywhere else to add a wing. " +
                                        "Ctrl+drag moves all of it, Shift+corner drag makes a wing square, Shift+click turns a ridge, Delete removes a wing, " +
                                        "Ctrl+C / Ctrl+V copy and paste one, Ctrl+D duplicates it. Enter places it, Esc drops it."
                : session.House.HasPending ? DraftHint(session)
                : ToolHint(session.Tool);

            (int x, int y, int z) = session.CursorCell;
            _statusCell.Text = session.ActiveTool.DragSize is { } drag ? $"{drag.Width} x {drag.Height} cells"
                : session.MouseInside && session.CompList.InBounds(x, y) ? $"Cell {x}, {y}, Z {z}" : string.Empty;
            int offset = session.WorkPlaneOffset;
            _statusPlane.Text = $"Work plane Z {session.WorkPlaneZ}" + (offset != 0 ? $" (floor {offset:+0;-0})" : string.Empty) +
                                (session.ShowWorkPlane ? string.Empty : " (hidden)");

            MultiTile hover = session.HoverTile;
            _statusHover.Text = hover != null ? $"0x{hover.Id:X4} {TileName(hover.Id)}  Z {hover.Z}  H {hover.Height}" : string.Empty;

            int count = session.Selection.Count;
            _statusSelection.Text = count == 0 ? string.Empty : $"{count} selected";
            _statusSize.Text = $"{session.CompList.Width} x {session.CompList.Height}, {session.CompList.TileCount} tiles";
        }

        private static string ToolHint(ToolKind kind)
        {
            return kind switch
            {
                ToolKind.Select => "Click a tile to select, Ctrl+click adds, drag a box over tiles, drag the selection to move it.",
                ToolKind.Lasso => "Draw an outline around tiles to select them. Ctrl adds to the selection.",
                ToolKind.Draw => "Click to place the chosen tile. Placement mode (M) decides the Z. Alt+click picks a tile.",
                ToolKind.Remove => "Click a tile to remove it, drag to remove an area.",
                ToolKind.Altitude => "Click raises the tile by the Z step, Shift+click lowers it.",
                ToolKind.Pipette => "Click a tile to draw with it; the draw tool comes back.",
                ToolKind.Transparent => "Click a tile to see through it, click again to restore.",
                ToolKind.RectFill => "Drag a rectangle to fill with the chosen tile. Alt+click picks a tile.",
                ToolKind.Line => "Drag a line of the chosen tile. Alt+click picks a tile.",
                ToolKind.Wall => "Drag along the cells to build walls in the chosen style. Corners and posts are picked for you.",
                ToolKind.Floor => "Drag an area to lay the chosen floor style.",
                ToolKind.Door => "Click a wall cell to hang the chosen door.",
                ToolKind.Stair => "Drag in the direction the stairs climb.",
                ToolKind.Roof => "Click a walled room to start its roof, or drag out a roof of any size. It stays open for shaping until Enter places it.",
                ToolKind.Foundation => "Drag an area to lay a foundation under it.",
                ToolKind.House => "Drag out the wall cells of a wing to start a house. Storeys, roof and materials are in the House panel; nothing is placed until Enter.",
                ToolKind.FloorPattern => "Drag an area to lay the bordered floor: corners, sides and fill repeat to fit.",
                _ => string.Empty
            };
        }
    }
}
