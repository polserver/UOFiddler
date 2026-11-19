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
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Controls.Classes;
using UoFiddler.Plugin.MultiEditor.Editing;
using UoFiddler.Plugin.MultiEditor.House;

namespace UoFiddler.Plugin.MultiEditor.UI
{
    public partial class MultiEditorControl
    {
        // ---------------------------------------------------------------- layout pieces

        /// <summary>Fills the designer's main strip: the menus, and the command buttons after each separator.</summary>
        private void BuildMainStrip()
        {
            ToolStrip strip = _mainStrip;

            Menu(_fileMenu, _new, _open, _openDesign, _import, _importFolder, null, _saveDesign, _saveDesignAs, _saveClient, _export, _saveBlueprint, _savePattern, null, _resize, _settingsCommand, null, _close, _closeAll);
            _undoHistory = new ToolStripMenuItem("Undo back to");
            _editMenu.DropDownOpening += (_, _) => FillUndoHistory(_undoHistory);
            Menu(_editMenu, _undo, _redo);
            _editMenu.DropDownItems.Add(_undoHistory);
            Menu(_editMenu, null, _cut, _copy, _paste, _duplicate, _repeat, _delete, null,
                _rotateLeft, _rotateRight, _mirrorX, _mirrorY, _swapStyle, null, _zUp, _zDown, _hide, _showHidden,
                null, _newLayer, _moveToLayer, _selectLayer);
            Menu(_selectMenu, _selectAll, _invert, _selectConnected, _selectSameId, null, _replaceId);
            ToolStripDropDownButton view = Menu(_viewMenu, _preview, _planView, null, _workPlane, _grid, _hideWalls, _hideAbove, _walkable, _doubleSurface, null,
                _allFloors, _levelUp, _levelDown, _planeUp, _planeDown, null, _showBrowser, _showInspector, null, _fit, _center, _zoomIn, _zoomOut, _zoomReset, null, _moveReference, null, _performanceInfo);
            var terrainMenu = new ToolStripMenuItem("Preview ground");
            foreach (EditorCommand terrain in _terrainCommands)
            {
                ToolStripMenuItem item = _commands.MenuItem(terrain);
                item.Text = terrain.Text.Replace("Preview on: ", string.Empty);
                terrainMenu.DropDownItems.Add(item);
            }

            view.DropDownItems.Insert(1, terrainMenu);

            var reference = new ToolStripMenuItem("Reference underlay");
            reference.DropDownItems.Add(new ToolStripMenuItem("None"));
            view.DropDownItems.Insert(view.DropDownItems.Count - 1, reference);
            view.DropDownOpening += (_, _) => FillReferenceMenu(reference);

            InsertButtons(strip, _mainSeparator1, _new, _open, _saveClient);
            InsertButtons(strip, _mainSeparator2, _undo, _redo);

            // The steps Undo would take back, by name, beside the Undo and Redo buttons.
            _undoDrop = new ToolStripDropDownButton("History")
            {
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                ToolTipText = "Undo back to a step of the history"
            };
            _undoDrop.DropDownOpening += (_, _) => FillUndoHistory(_undoDrop);
            strip.Items.Insert(strip.Items.IndexOf(_mainSeparator2), _undoDrop);
            InsertButtons(strip, _mainSeparator3, _cut, _copy, _paste, _delete);
            InsertButtons(strip, _mainSeparator4, _workPlane, _grid, _hideWalls);

            // Zoom, then help, after the other groups: zoom out, the zoom, zoom in, 100%, fit and centre.
            int at = strip.Items.IndexOf(_zoomLabel);
            strip.Items.Insert(at++, new ToolStripSeparator());
            strip.Items.Insert(at++, _commands.Button(_zoomOut));
            at++; // the zoom label
            foreach (EditorCommand command in new[] { _zoomIn, _zoomReset, _fit, _center })
            {
                strip.Items.Insert(at++, _commands.Button(command));
            }

            strip.Items.Insert(at++, new ToolStripSeparator());
            strip.Items.Insert(at, _commands.Button(_shortcuts));

            StyleSeparators(strip);
        }

        /// <summary>Puts a button for each command into a strip, right after a designer item.</summary>
        private void InsertButtons(ToolStrip strip, ToolStripItem after, params EditorCommand[] commands)
        {
            int at = strip.Items.IndexOf(after) + 1;
            foreach (EditorCommand command in commands)
            {
                strip.Items.Insert(at++, _commands.Button(command));
            }
        }

        /// <summary>Fills one of the designer's menus with command items; null is a separator.</summary>
        private ToolStripDropDownButton Menu(ToolStripDropDownButton menu, params EditorCommand[] commands)
        {
            foreach (EditorCommand command in commands)
            {
                menu.DropDownItems.Add(command == null ? new ToolStripSeparator() : _commands.MenuItem(command));
            }

            return menu;
        }

        private ToolStripMenuItem _undoHistory;
        private ToolStripDropDownButton _undoDrop;

        /// <summary>Lists the last steps of the history by name; choosing one takes it and every later step back.</summary>
        private void FillUndoHistory(ToolStripDropDownItem menu)
        {
            menu.DropDownItems.Clear();
            IReadOnlyList<string> steps = Session?.UndoSteps(25) ?? Array.Empty<string>();
            if (menu is ToolStripMenuItem)
            {
                menu.Enabled = steps.Count > 0;
            }
            else if (steps.Count == 0)
            {
                menu.DropDownItems.Add(new ToolStripMenuItem("Nothing to undo") { Enabled = false });
            }

            for (int i = 0; i < steps.Count; ++i)
            {
                int count = i + 1;
                var item = new ToolStripMenuItem($"{count}. {steps[i]}");
                item.Click += (_, _) =>
                {
                    for (int n = 0; n < count && Session != null && Session.Undo(); ++n)
                    {
                    }

                    _commands.RefreshStates();
                    Tell(count == 1 ? "1 step undone." : $"{count} steps undone.");
                };
                menu.DropDownItems.Add(item);
            }
        }

        private void Menu_DropDownOpening(object sender, EventArgs e)
        {
            _commands.RefreshStates();
        }

        /// <summary>Draws a strip's separators so they show on the dark theme too, with some room on both sides.</summary>
        private static void StyleSeparators(ToolStrip strip)
        {
            foreach (ToolStripSeparator separator in strip.Items.OfType<ToolStripSeparator>())
            {
                separator.Margin = new Padding(4, 0, 4, 0);
                separator.Paint += PaintSeparator;
            }
        }

        /// <summary>A separator as a plain line in a colour that shows on the light and the dark theme.</summary>
        private static void PaintSeparator(object sender, PaintEventArgs e)
        {
            var separator = (ToolStripSeparator)sender;
            int x = separator.Width / 2;
            using var pen = new Pen(Options.DarkMode ? Color.FromArgb(105, 105, 112) : SystemColors.ControlDark);
            e.Graphics.DrawLine(pen, x, 4, x, separator.Height - 5);
        }

        /// <summary>Fills the designer's options strip: the brush buttons and the roof option buttons.</summary>
        private void BuildOptionsStrip()
        {
            ToolStrip strip = _optionsStrip;
            _toolLabel.Font = new Font(Font, FontStyle.Bold);

            // Where new tiles go: one button per mode, the pressed one in use (M steps through them).
            int at = strip.Items.IndexOf(_placeLabel) + 1;
            foreach ((EditorCommand command, string text, string tip) in _placementCommands)
            {
                ToolStripButton button = _commands.Button(command);
                button.Text = text;
                button.ToolTipText = $"{tip} (M)";
                button.Margin = new Padding(1, 1, 0, 2);
                strip.Items.Insert(at++, button);
            }

            // Separators between the groups: Place, Brush, Z step, Floor, ...
            strip.Items.Insert(strip.Items.IndexOf(_brushLabel), new ToolStripSeparator());
            strip.Items.Insert(strip.Items.IndexOf(_zStepLabel), new ToolStripSeparator());
            StyleSeparators(strip);

            at = strip.Items.IndexOf(_brushLabel) + 1;
            foreach (EditorCommand brush in _brushCommands)
            {
                // One press each instead of a dropdown; the pressed one is the size in use.
                ToolStripButton button = _commands.Button(brush);
                button.Text = brush.Text.Replace("Brush ", string.Empty);
                button.AutoSize = false;
                button.Width = 34;
                button.Margin = new Padding(1, 1, 0, 2);
                strip.Items.Insert(at++, button);
            }

            // Roof and draft options get a row of their own under the tool options, shown only
            // while they apply: beside the other options they ended up far to the right.
            _roofStrip = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, Dock = DockStyle.Top, Visible = false, Renderer = strip.Renderer, RenderMode = strip.RenderMode };
            foreach (ToolStripItem item in new ToolStripItem[]
                     {
                         _shapeSeparator, _shapeLabel, _ridgeSeparator, _ridgeLabel, _gablesSeparator, _gablesLabel, _gableFillSeparator, _gableFillLabel
                     })
            {
                _roofStrip.Items.Add(item);
            }

            Controls.Add(_roofStrip);
            Controls.SetChildIndex(_roofStrip, Controls.GetChildIndex(_optionsStrip));
            RoofGroup(_roofStrip, _shapeSeparator, _shapeLabel,
                ("Gable", () => Session.Roof.Shape == RoofShape.Gable, () => Session.Roof.Shape = RoofShape.Gable),
                ("Hip", () => Session.Roof.Shape == RoofShape.Hip, () => Session.Roof.Shape = RoofShape.Hip));
            RoofGroup(_roofStrip, _ridgeSeparator, _ridgeLabel,
                ("Auto", () => Session.Roof.Ridge == RoofRidge.Auto, () => Session.Roof.Ridge = RoofRidge.Auto),
                ("X", () => Session.Roof.Ridge == RoofRidge.AlongX, () => Session.Roof.Ridge = RoofRidge.AlongX),
                ("Y", () => Session.Roof.Ridge == RoofRidge.AlongY, () => Session.Roof.Ridge = RoofRidge.AlongY));
            RoofGroup(_roofStrip, _gablesSeparator, _gablesLabel,
                ("Front", () => Session.Roof.Gables == RoofGables.Front, () => Session.Roof.Gables = RoofGables.Front),
                ("Both", () => Session.Roof.Gables == RoofGables.Both, () => Session.Roof.Gables = RoofGables.Both),
                ("None", () => Session.Roof.Gables == RoofGables.None, () => Session.Roof.Gables = RoofGables.None));
            RoofGroup(_roofStrip, _gableFillSeparator, _gableFillLabel,
                ("Caps", () => Session.Roof.GableFill == RoofGableFill.Caps, () => Session.Roof.GableFill = RoofGableFill.Caps),
                ("Walls", () => Session.Roof.GableFill == RoofGableFill.Walls, () => Session.Roof.GableFill = RoofGableFill.Walls));

            // Shown while a roof is being shaped.
            _placeRoofButton = RoofAction(_roofStrip, "Place", "Places what is being shaped: the roof on the Roof layer, the house on its layers (Enter).", () => Session.DraftInFocus?.Commit());
            _dropRoofButton = RoofAction(_roofStrip, "Drop", "Drops what is being shaped (Esc). Undo brings it back.", () => Session.DraftInFocus?.Discard());
            StyleSeparators(_roofStrip);
        }

        private void ZStepCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            Apply(() => Session.ZStep = int.Parse(_zStepCombo.Text));
        }

        private ToolStrip _roofStrip;
        private EditorCommand _placeRoof;
        private ToolStripButton _placeRoofButton;
        private ToolStripButton _dropRoofButton;
        private readonly List<ToolStripItem> _roofItems = new List<ToolStripItem>();
        private readonly List<(ToolStripButton Button, Func<bool> IsOn)> _roofButtons = new List<(ToolStripButton, Func<bool>)>();

        /// <summary>
        /// A labelled group of pick-one buttons for a roof option. The separator and the caption
        /// come from the designer; the buttons go after the caption and share its tooltip.
        /// </summary>
        private void RoofGroup(ToolStrip strip, ToolStripSeparator separator, ToolStripLabel caption, params (string Text, Func<bool> IsOn, Action Set)[] choices)
        {
            string tip = caption.ToolTipText;
            int at = strip.Items.IndexOf(caption) + 1;
            _roofItems.Add(separator);
            _roofItems.Add(caption);
            foreach ((string text, Func<bool> isOn, Action set) in choices)
            {
                var button = new ToolStripButton(text)
                {
                    DisplayStyle = ToolStripItemDisplayStyle.Text,
                    ToolTipText = tip,
                    AutoToolTip = false,
                    Margin = new Padding(1, 1, 0, 2)
                };
                button.Click += (_, _) =>
                {
                    if (Session == null)
                    {
                        return;
                    }

                    set();
                    Session.ToolOptionsChanged();
                    SyncRoofOptions();
                    _canvas.Focus();
                };
                strip.Items.Insert(at++, button);
                _roofItems.Add(button);
                _roofButtons.Add((button, isOn));
            }
        }

        private ToolStripButton RoofAction(ToolStrip strip, string text, string tip, Action run)
        {
            var button = new ToolStripButton(text)
            {
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                ToolTipText = tip,
                AutoToolTip = false,
                Margin = new Padding(6, 1, 0, 2),
                Visible = false
            };
            button.Click += (_, _) =>
            {
                if (Session?.DraftInFocus != null)
                {
                    run();
                    _commands.RefreshStates();
                    _canvas.Focus();
                }
            };
            strip.Items.Add(button);
            return button;
        }

        private void SyncRoofOptions()
        {
            bool show = Session != null && !Session.Pasting && Session.Tool == ToolKind.Roof;
            foreach (ToolStripItem item in _roofItems)
            {
                item.Visible = show;
            }

            bool draft = Session != null && !Session.Pasting && Session.DraftInFocus != null;
            _placeRoofButton.Visible = _dropRoofButton.Visible = draft;

            // The first group needs no line before it on a row of its own.
            _shapeSeparator.Visible = false;
            if (_roofStrip.Visible != (show || draft))
            {
                _roofStrip.Visible = show || draft;
            }

            // The House page comes to the front when the House tool is taken up or a draft is
            // opened, and gives way again when both are over; in between the pages are yours.
            bool house = Session != null && (Session.Tool == ToolKind.House || Session.House.HasPending);
            _housePanel.RefreshDraft();
            if (house != _houseWasShown)
            {
                _houseWasShown = house;
                _sideTabs.SelectedTab = house ? _housePage : _designPage;
            }

            foreach ((ToolStripButton button, Func<bool> isOn) in _roofButtons)
            {
                button.Checked = show && isOn();
            }
        }

        /// <summary>Runs a change coming from an options control, unless the control is being filled from the session.</summary>
        private void Apply(Action change)
        {
            if (_syncing || Session == null)
            {
                return;
            }

            change();
            _canvas.Focus();
        }

        /// <summary>Fills the designer's tool palette: the tool buttons under each section label.</summary>
        private void BuildToolPalette()
        {
            ToolStrip strip = _toolPalette;

            void Section(ToolStripLabel title, params ToolKind[] kinds)
            {
                int at = strip.Items.IndexOf(title) + 1;
                foreach (ToolKind kind in kinds)
                {
                    ToolStripButton button = _commands.Button(_toolCommands[kind], showText: true);
                    button.ImageAlign = ContentAlignment.MiddleLeft;
                    button.TextAlign = ContentAlignment.MiddleLeft;
                    button.AutoSize = false;
                    button.Size = new Size(92, 26);
                    strip.Items.Insert(at++, button);
                }
            }

            Section(_editToolsLabel, ToolKind.Select, ToolKind.Lasso, ToolKind.Altitude, ToolKind.Pipette, ToolKind.Transparent, ToolKind.Remove);
            Section(_drawToolsLabel, ToolKind.Draw, ToolKind.RectFill, ToolKind.Line);
            Section(_houseToolsLabel, ToolKind.Wall, ToolKind.Floor, ToolKind.Door, ToolKind.Stair, ToolKind.Roof, ToolKind.Foundation, ToolKind.House);
        }
    }
}
