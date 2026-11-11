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
using System.Linq;
using System.Windows.Forms;
using Ultima;
using Ultima.Helpers;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.UserControls;

namespace UoFiddler.Controls.Forms
{
    /// <summary>
    /// Edits one hue. Edits go to a working copy with an undo history; Save writes it back to the
    /// hue list. Tools act on the strip's selected range, or on the whole hue when one cell is
    /// selected. The preview hues an item, a gump and an animation as you edit.
    /// </summary>
    public partial class HueEditForm : Form
    {
        private readonly Hue _hue;
        private readonly ushort[] _colors = new ushort[HueRamp.ColorCount];
        private readonly HueEditHistory _history;
        private readonly IReadOnlyList<HuePreviewSubject> _presets;
        private ushort[] _savedColors;
        private string _savedName;
        private bool _updating;

        private Color _gradientFrom;
        private Color _gradientTo;
        private bool _gradientFromCustom;
        private bool _gradientToCustom;
        private Color _rampBase;
        private bool _rampBaseCustom;

        private Bitmap _itemSource;
        private int _itemSourceId = -1;
        private Bitmap _gumpSource;
        private List<AnimatedFrame> _animSources = [];

        public HueEditForm(int index)
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();

            _hue = Hues.GetHue(index);
            _hue.Colors.CopyTo(_colors, 0);
            _savedColors = (ushort[])_colors.Clone();
            _savedName = _hue.Name;

            _history = new HueEditHistory(_colors, _hue.Name);
            _history.Changed += (_, _) => RefreshHistory();

            _updating = true;
            textBoxName.Text = _hue.Name;
            _updating = false;

            gradientCurveComboBox.Items.AddRange(["Linear", "Ease in", "Ease out", "Smooth"]);
            gradientCurveComboBox.SelectedIndex = 0;

            hueStrip.SetColors(_colors);
            hueStrip.SetOriginal(_colors);

            _presets = HuePreviewPresets.Build();
            SetupPreview();

            RefreshHistory();
            OnStripSelectionChanged(this, EventArgs.Empty);
            UpdateTitle();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            ActiveControl = hueStrip;
        }

        // ---- Range and edits --------------------------------------------------------------

        /// <summary>The cells the tools act on: the selected range, or the whole hue.</summary>
        private (int Start, int End) ToolRange => hueStrip.HasRange
            ? (hueStrip.SelectionStart, hueStrip.SelectionEnd)
            : (0, HueRamp.ColorCount - 1);

        private void ApplyEdit(string label, Action<ushort[]> edit)
        {
            edit(_colors);
            _history.Record(_colors, textBoxName.Text, label);
            _history.BreakMerge();
            AfterColorsChanged(updatePicker: true);
        }

        private void AfterColorsChanged(bool updatePicker)
        {
            hueStrip.SetColors(_colors);
            if (updatePicker)
            {
                colorPicker.SetColor(HueHelpers.HueToColor(_colors[hueStrip.AnchorCell]), resetOriginal: false);
            }

            RefreshToolSwatches();
            SchedulePreview();
            UpdateTitle();
        }

        private bool IsDirty => textBoxName.Text != _savedName || !_colors.AsSpan().SequenceEqual(_savedColors);

        private void UpdateTitle()
        {
            Text = $"Hue Editor - {_hue.Index} / 0x{_hue.Index:X}{(IsDirty ? " *" : string.Empty)}";
            undoButton.Enabled = _history.CanUndo;
            redoButton.Enabled = _history.CanRedo;
            revertButton.Enabled = _history.Position != 0;
        }

        private void OnStripSelectionChanged(object sender, EventArgs e)
        {
            _history.BreakMerge();
            colorPicker.SetColor(HueHelpers.HueToColor(_colors[hueStrip.AnchorCell]), resetOriginal: true);
            colorGroupBox.Text = $"Color {hueStrip.AnchorCell}";

            (int start, int end) = ToolRange;
            rangeLabel.Text = hueStrip.HasRange
                ? $"Selected cells {start} to {end} ({end - start + 1} colors)"
                : "Whole hue (cells 0 to 31): drag on the strip to work on part of it";

            _gradientFromCustom = false;
            _gradientToCustom = false;
            _rampBaseCustom = false;
            RefreshToolSwatches();
        }

        private void OnStripCellDoubleClicked(object sender, int index)
        {
            colorPicker.Focus();
        }

        private void RefreshToolSwatches()
        {
            (int start, int end) = ToolRange;
            if (!_gradientFromCustom)
            {
                _gradientFrom = HueHelpers.HueToColor(_colors[start]);
            }

            if (!_gradientToCustom)
            {
                _gradientTo = HueHelpers.HueToColor(_colors[end]);
            }

            if (!_rampBaseCustom)
            {
                _rampBase = HueHelpers.HueToColor(_colors[hueStrip.AnchorCell]);
            }

            gradientStartSwatch.BackColor = _gradientFrom;
            gradientEndSwatch.BackColor = _gradientTo;
            rampSwatch.BackColor = _rampBase;
        }

        // ---- Color picker -----------------------------------------------------------------

        private void OnPickerColorChanging(object sender, Color color)
        {
            SetAnchorColor(color);
        }

        private void OnPickerColorCommitted(object sender, Color color)
        {
            SetAnchorColor(color);
            _history.BreakMerge();
        }

        private void SetAnchorColor(Color color)
        {
            int anchor = hueStrip.AnchorCell;
            ushort packed = HueRamp.Pack(color);
            if (_colors[anchor] == packed)
            {
                return;
            }

            _colors[anchor] = packed;
            _history.Record(_colors, textBoxName.Text, $"Color {anchor} = {HueColorText.ToHex(HueHelpers.HueToColor(packed))}", $"color:{anchor}");
            AfterColorsChanged(updatePicker: false);
        }

        // ---- Tools ------------------------------------------------------------------------

        private HueGradientCurve SelectedCurve => (HueGradientCurve)Math.Max(0, gradientCurveComboBox.SelectedIndex);

        private void OnClickGradient(object sender, EventArgs e)
        {
            (int start, int end) = ToolRange;
            Color from = _gradientFrom;
            Color to = _gradientTo;
            HueGradientCurve curve = SelectedCurve;
            ApplyEdit($"Gradient {start}-{end} ({gradientCurveComboBox.Text})", colors => HueRamp.Gradient(colors, start, end, from, to, curve));
            _gradientFromCustom = false;
            _gradientToCustom = false;
            RefreshToolSwatches();
        }

        private void OnClickReverse(object sender, EventArgs e)
        {
            (int start, int end) = ToolRange;
            ApplyEdit($"Reverse {start}-{end}", colors => HueRamp.Reverse(colors, start, end));
        }

        private void OnClickRamp(object sender, EventArgs e)
        {
            Color baseColor = _rampBase;
            double dark = (double)rampDarkNumeric.Value / 100;
            double light = (double)rampLightNumeric.Value / 100;
            (int start, int end) = ToolRange;
            ApplyEdit($"Build {start}-{end} from {HueColorText.ToHex(baseColor)}", colors =>
            {
                // The ramp is laid out over all 32 cells; only the range takes its cells.
                var ramp = new ushort[HueRamp.ColorCount];
                HueRamp.FromBaseColor(ramp, baseColor, dark, light);
                Array.Copy(ramp, start, colors, start, end - start + 1);
            });
            _rampBaseCustom = false;
            RefreshToolSwatches();
        }

        private void OnClickRampFromCell(object sender, EventArgs e)
        {
            _rampBase = HueHelpers.HueToColor(_colors[hueStrip.AnchorCell]);
            _rampBaseCustom = true;
            OnClickRamp(sender, e);
        }

        private void OnClickAdjust(object sender, EventArgs e)
        {
            double hue = (double)adjustHueNumeric.Value;
            double sat = (double)adjustSatNumeric.Value;
            double light = (double)adjustLightNumeric.Value;
            int red = (int)adjustRedNumeric.Value;
            int green = (int)adjustGreenNumeric.Value;
            int blue = (int)adjustBlueNumeric.Value;
            if (hue == 0 && sat == 0 && light == 0 && red == 0 && green == 0 && blue == 0)
            {
                return;
            }

            (int start, int end) = ToolRange;
            ApplyEdit($"Adjust {start}-{end}", colors => HueRamp.Adjust(colors, start, end, hue, sat, light, red, green, blue));
        }

        private void OnClickGradientStartSwatch(object sender, EventArgs e)
        {
            HueColorPicker.ShowDropDown(gradientStartSwatch, _gradientFrom, color =>
            {
                _gradientFrom = color;
                _gradientFromCustom = true;
                gradientStartSwatch.BackColor = color;
            });
        }

        private void OnClickGradientEndSwatch(object sender, EventArgs e)
        {
            HueColorPicker.ShowDropDown(gradientEndSwatch, _gradientTo, color =>
            {
                _gradientTo = color;
                _gradientToCustom = true;
                gradientEndSwatch.BackColor = color;
            });
        }

        private void OnClickRampSwatch(object sender, EventArgs e)
        {
            HueColorPicker.ShowDropDown(rampSwatch, _rampBase, color =>
            {
                _rampBase = color;
                _rampBaseCustom = true;
                rampSwatch.BackColor = color;
            });
        }

        private void OnStripMenuOpening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            gradientMenuItem.Enabled = ToolRange.End - ToolRange.Start >= 1;
            pasteMenuItem.Enabled = Clipboard.ContainsText();
        }

        private void OnClickSelectAll(object sender, EventArgs e)
        {
            hueStrip.SelectRange(0, HueRamp.ColorCount - 1);
        }

        // ---- Copy, paste and text ---------------------------------------------------------

        private void OnClickCopy(object sender, EventArgs e)
        {
            (int start, int end) = ToolRange;
            Clipboard.SetText(HueColorText.FormatList(_colors[start..(end + 1)]));
        }

        private void OnClickPaste(object sender, EventArgs e)
        {
            if (!Clipboard.ContainsText())
            {
                return;
            }

            if (!HueColorText.TryParseLines(Clipboard.GetText(), out List<ushort> pasted, out string error))
            {
                MessageBox.Show(this, $"Nothing was pasted. {error}", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (pasted.Count == 0)
            {
                MessageBox.Show(this, "The clipboard has no colors in it (one #RRGGBB, r,g,b or 0x7FFF per line).", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int start = hueStrip.SelectionStart;
            int end = hueStrip.SelectionEnd;
            string label;
            if (pasted.Count == 1)
            {
                label = $"Paste {HueColorText.ToHex(HueHelpers.HueToColor(pasted[0]))} into {start}-{end}";
                ApplyEdit(label, colors => Array.Fill(colors, pasted[0], start, end - start + 1));
                return;
            }

            if (pasted.Count == HueRamp.ColorCount)
            {
                start = 0;
            }

            int count = Math.Min(pasted.Count, HueRamp.ColorCount - start);
            label = $"Paste {count} colors at {start}";
            ApplyEdit(label, colors => pasted.CopyTo(0, colors, start, count));
        }

        private void OnClickEditAsText(object sender, EventArgs e)
        {
            string text = string.Join(Environment.NewLine, _colors.Select((value, i) =>
                $"{HueColorText.ToHex(HueHelpers.HueToColor(value))}  // {i}"));

            using var dialog = new Form
            {
                Text = $"Hue {_hue.Index} colors",
                ClientSize = new Size(340, 580),
                MinimumSize = new Size(300, 300),
                FormBorderStyle = FormBorderStyle.SizableToolWindow,
                StartPosition = FormStartPosition.CenterParent,
                ShowInTaskbar = false,
            };
            var help = new Label
            {
                Dock = DockStyle.Top,
                Height = 48,
                Padding = new Padding(6),
                Text = "One color per line, index 0 (darkest) first. Use #RRGGBB, r,g,b or a raw 0x7FFF. Text after // is ignored.",
            };
            var editor = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                AcceptsReturn = true,
                Font = new Font(FontFamily.GenericMonospace, 9.5f),
                Text = text,
            };
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 36, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(4) };
            var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel };
            var ok = new Button { Text = "Apply" };
            buttons.Controls.Add(cancel);
            buttons.Controls.Add(ok);
            dialog.Controls.Add(editor);
            dialog.Controls.Add(help);
            dialog.Controls.Add(buttons);
            dialog.CancelButton = cancel;

            ushort[] parsed = null;
            ok.Click += (_, _) =>
            {
                if (HueColorText.TryParseList(editor.Text, HueRamp.ColorCount, out parsed, out string error))
                {
                    dialog.DialogResult = DialogResult.OK;
                }
                else
                {
                    MessageBox.Show(dialog, error, dialog.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };

            if (dialog.ShowDialog(this) == DialogResult.OK && parsed != null)
            {
                ApplyEdit("Edit as text", colors => parsed.CopyTo(colors, 0));
            }
        }

        // ---- History ----------------------------------------------------------------------

        private void RefreshHistory()
        {
            _updating = true;
            try
            {
                historyListBox.BeginUpdate();
                historyListBox.Items.Clear();
                foreach (HueEditSnapshot entry in _history.Entries)
                {
                    historyListBox.Items.Add(entry);
                }

                historyListBox.SelectedIndex = _history.Position;
                historyListBox.EndUpdate();
            }
            finally
            {
                _updating = false;
            }

            UpdateTitle();
        }

        private void OnDrawHistoryItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
            {
                return;
            }

            e.DrawBackground();
            bool undone = e.Index > _history.Position;
            bool selected = (e.State & DrawItemState.Selected) != 0;
            Color color = selected ? SystemColors.HighlightText : undone ? SystemColors.GrayText : e.ForeColor;
            string text = $"{e.Index}. {historyListBox.Items[e.Index]}";
            TextRenderer.DrawText(e.Graphics, text, e.Font, e.Bounds, color, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            e.DrawFocusRectangle();
        }

        private void OnHistorySelected(object sender, EventArgs e)
        {
            if (_updating || historyListBox.SelectedIndex < 0 || historyListBox.SelectedIndex == _history.Position)
            {
                return;
            }

            RestoreSnapshot(_history.JumpTo(historyListBox.SelectedIndex));
        }

        private void OnClickUndo(object sender, EventArgs e) => RestoreSnapshot(_history.Undo());

        private void OnClickRedo(object sender, EventArgs e) => RestoreSnapshot(_history.Redo());

        private void OnClickRevert(object sender, EventArgs e) => RestoreSnapshot(_history.JumpTo(0));

        private void RestoreSnapshot(HueEditSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return;
            }

            snapshot.Colors.CopyTo(_colors, 0);
            _updating = true;
            textBoxName.Text = snapshot.Name;
            _updating = false;
            _gradientFromCustom = false;
            _gradientToCustom = false;
            _rampBaseCustom = false;
            colorPicker.SetColor(HueHelpers.HueToColor(_colors[hueStrip.AnchorCell]), resetOriginal: true);
            AfterColorsChanged(updatePicker: false);
        }

        private void OnNameChanged(object sender, EventArgs e)
        {
            if (_updating)
            {
                return;
            }

            _history.Record(_colors, textBoxName.Text, $"Rename to \"{textBoxName.Text}\"", "name");
            UpdateTitle();
        }

        // ---- Save and close ---------------------------------------------------------------

        private void OnClickSave(object sender, EventArgs e) => Save();

        private void Save()
        {
            _colors.CopyTo(_hue.Colors, 0);
            _hue.Name = textBoxName.Text;
            _hue.TableStart = _colors[0];
            _hue.TableEnd = (ushort)(_colors[_hue.Colors.Length - 1] + 1057);
            ControlEvents.FireHueChangeEvent();
            Options.ChangedUltimaClass["Hues"] = true;

            _savedColors = (ushort[])_colors.Clone();
            _savedName = _hue.Name;
            UpdateTitle();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!e.Cancel && IsDirty)
            {
                DialogResult result = MessageBox.Show(this, $"Hue {_hue.Index} has changes that are not saved. Save them?", Text,
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
                else if (result == DialogResult.Yes)
                {
                    Save();
                }
            }

            if (!e.Cancel)
            {
                previewTimer.Stop();
                animPictureBox.Animate = false;
                RememberPreview();
            }

            base.OnFormClosing(e);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            bool textFocused = IsTextInputFocused();
            switch (keyData)
            {
                case Keys.Control | Keys.S:
                    if (IsDirty)
                    {
                        Save();
                    }

                    return true;
                case Keys.Control | Keys.Z when !textFocused:
                    OnClickUndo(this, EventArgs.Empty);
                    return true;
                case Keys.Control | Keys.Y when !textFocused:
                case Keys.Control | Keys.Shift | Keys.Z when !textFocused:
                    OnClickRedo(this, EventArgs.Empty);
                    return true;
                case Keys.Control | Keys.C when hueStrip.Focused:
                    OnClickCopy(this, EventArgs.Empty);
                    return true;
                case Keys.Control | Keys.V when hueStrip.Focused:
                    OnClickPaste(this, EventArgs.Empty);
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private bool IsTextInputFocused()
        {
            Control control = ActiveControl;
            while (control is ContainerControl container && container.ActiveControl != null && control is not NumericUpDown)
            {
                control = container.ActiveControl;
            }

            return control is TextBoxBase or NumericUpDown or ComboBox;
        }

        // ---- Preview ----------------------------------------------------------------------

        private void SetupPreview()
        {
            foreach (Control pane in new Control[] { itemPictureBox, gumpPictureBox, animPictureBox })
            {
                pane.BackColor = Options.PreviewBackgroundColor;
            }

            _updating = true;
            // Same mapping as the Animations tab's facing bar.
            facingBar.Value = (Math.Clamp(Options.HueEditorPreviewFacing, 0, 7) + 3) & 7;
            _updating = false;
            presetComboBox.Items.AddRange(_presets.Cast<object>().ToArray());
            presetComboBox.Items.Add(HuePreviewPresets.CustomName);

            int presetIndex = _presets.ToList().FindIndex(p => p.Name == Options.HueEditorPreviewPreset);
            _updating = true;
            if (presetIndex >= 0)
            {
                presetComboBox.SelectedIndex = presetIndex;
                ShowSubjectIds(_presets[presetIndex]);
            }
            else if (Options.HueEditorPreviewPreset == HuePreviewPresets.CustomName)
            {
                presetComboBox.SelectedIndex = presetComboBox.Items.Count - 1;
                ShowSubjectIds(new HuePreviewSubject(HuePreviewPresets.CustomName,
                    Options.HueEditorPreviewItemId, Options.HueEditorPreviewGumpId, Options.HueEditorPreviewBodyId));
                onlyGreyCheckBox.Checked = HuePreviewPresets.IsPartialHue(Options.HueEditorPreviewItemId);
            }
            else
            {
                presetComboBox.SelectedIndex = 0;
                ShowSubjectIds(_presets[0]);
            }

            _updating = false;
            LoadPreviewSources();
        }

        private void ShowSubjectIds(HuePreviewSubject subject)
        {
            itemIdTextBox.Text = FormatId(subject.ItemId);
            gumpIdTextBox.Text = FormatId(subject.GumpId);
            bodyIdTextBox.Text = subject.BodyId < 0 ? string.Empty : subject.BodyId.ToString();
            if (subject.Name != HuePreviewPresets.CustomName)
            {
                onlyGreyCheckBox.Checked = HuePreviewPresets.IsPartialHue(subject.ItemId);
            }

            static string FormatId(int id) => id < 0 ? string.Empty : $"0x{id:X4}";
        }

        private void OnPresetChanged(object sender, EventArgs e)
        {
            if (_updating || presetComboBox.SelectedIndex < 0 || presetComboBox.SelectedIndex >= _presets.Count)
            {
                return;
            }

            _updating = true;
            ShowSubjectIds(_presets[presetComboBox.SelectedIndex]);
            _updating = false;
            LoadPreviewSources();
        }

        private void OnPreviewIdKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            e.Handled = true;
            e.SuppressKeyPress = true;
            ApplyTypedPreviewIds();
        }

        private void OnPreviewIdLeave(object sender, EventArgs e) => ApplyTypedPreviewIds();

        private void ApplyTypedPreviewIds()
        {
            int item = ParseId(itemIdTextBox);
            int gump = ParseId(gumpIdTextBox);
            int body = ParseId(bodyIdTextBox);

            int index = presetComboBox.SelectedIndex;
            bool matchesPreset = index >= 0 && index < _presets.Count
                && _presets[index].ItemId == item && _presets[index].GumpId == gump && _presets[index].BodyId == body;
            if (!matchesPreset && presetComboBox.SelectedIndex != presetComboBox.Items.Count - 1)
            {
                _updating = true;
                presetComboBox.SelectedIndex = presetComboBox.Items.Count - 1;
                _updating = false;
            }

            if (item != _itemSourceId)
            {
                _updating = true;
                onlyGreyCheckBox.Checked = HuePreviewPresets.IsPartialHue(item);
                _updating = false;
            }

            LoadPreviewSources();
        }

        private void OnClickBrowseItem(object sender, EventArgs e) => BrowsePreviewId(HuePreviewPickKind.Item, itemIdTextBox);

        private void OnClickBrowseGump(object sender, EventArgs e) => BrowsePreviewId(HuePreviewPickKind.Gump, gumpIdTextBox);

        private void OnClickBrowseBody(object sender, EventArgs e) => BrowsePreviewId(HuePreviewPickKind.Body, bodyIdTextBox);

        private void BrowsePreviewId(HuePreviewPickKind kind, TextBox box)
        {
            using var picker = new HuePreviewPickerForm(kind, ParseId(box), Facing);
            if (picker.ShowDialog(this) != DialogResult.OK || picker.SelectedId < 0)
            {
                return;
            }

            box.Text = kind == HuePreviewPickKind.Body ? picker.SelectedId.ToString() : $"0x{picker.SelectedId:X4}";
            ApplyTypedPreviewIds();
        }

        private static int ParseId(TextBox box)
        {
            string text = box.Text.Trim();
            return text.Length > 0 && Utils.ConvertStringToInt(text, out int value, 0, 0xFFFFF) ? value : -1;
        }

        private void OnOnlyGreyChanged(object sender, EventArgs e)
        {
            if (!_updating)
            {
                SchedulePreview();
            }
        }

        /// <summary>The UO direction (0-7) the facing bar points at.</summary>
        private int Facing => (facingBar.Value - 3) & 7;

        private void OnFacingChanged(object sender, EventArgs e)
        {
            if (!_updating)
            {
                LoadPreviewSources();
            }
        }

        private void LoadPreviewSources()
        {
            DisposePreviewSources();
            Color invalid = Options.DarkMode ? Color.OrangeRed : Color.Red;

            int item = ParseId(itemIdTextBox);
            bool itemOk = item >= 0 && item <= Art.GetMaxItemId() && Art.IsValidStatic(item);
            _itemSourceId = item;
            if (itemOk)
            {
                _itemSource = CloneBitmap(Art.GetStatic(item));
            }

            string itemName = itemOk && TileData.ItemTable != null && item < TileData.ItemTable.Length
                ? $" {TileData.ItemTable[item].Name}"
                : string.Empty;
            itemIdTextBox.ForeColor = item < 0 || itemOk ? SystemColors.WindowText : invalid;
            itemCaptionLabel.Text = item < 0 ? "Item: none" : itemOk ? $"Item 0x{item:X4}{itemName}" : $"Item 0x{item:X4}: not found";

            int gump = ParseId(gumpIdTextBox);
            bool gumpOk = gump >= 0 && gump <= 0xFFFE && Gumps.IsValidIndex(gump);
            if (gumpOk)
            {
                _gumpSource = CloneBitmap(Gumps.GetGump(gump));
            }

            gumpIdTextBox.ForeColor = gump < 0 || gumpOk ? SystemColors.WindowText : invalid;
            gumpCaptionLabel.Text = gump < 0 ? "Gump: none" : gumpOk ? $"Gump 0x{gump:X4}" : $"Gump 0x{gump:X4}: not found";

            int body = ParseId(bodyIdTextBox);
            bool bodyOk = body >= 0 && body < 0x10000 && Animations.IsActionDefined(body, 0, 0);
            if (bodyOk)
            {
                int hue = 0;
                int facing = Facing;
                AnimationFrame[] frames = Animations.GetAnimation(body, 0, facing, ref hue, false, false);
                _animSources = frames?
                    .Where(f => f?.Bitmap != null)
                    .Select(f => new AnimatedFrame(CloneBitmap(f.Bitmap), f.Center))
                    .ToList() ?? [];
                bodyOk = _animSources.Count > 0;
            }

            bodyIdTextBox.ForeColor = body < 0 || bodyOk ? SystemColors.WindowText : invalid;
            animCaptionLabel.Text = body < 0 ? "Animation: none" : bodyOk ? $"Body {body}, {_animSources.Count} frames" : $"Body {body}: not found";

            RenderPreview();
        }

        private static Bitmap CloneBitmap(Bitmap source)
        {
            return source.Clone(new Rectangle(0, 0, source.Width, source.Height), source.PixelFormat);
        }

        private void DisposePreviewSources()
        {
            _itemSource?.Dispose();
            _itemSource = null;
            _gumpSource?.Dispose();
            _gumpSource = null;
            foreach (AnimatedFrame frame in _animSources)
            {
                frame.Bitmap?.Dispose();
            }

            _animSources = [];
        }

        private void DisposePreview()
        {
            DisposePreviewSources();
            foreach (PictureBox pane in new[] { itemPictureBox, gumpPictureBox })
            {
                Image image = pane.Image;
                pane.Image = null;
                image?.Dispose();
            }

            animPictureBox.Animate = false;
            animPictureBox.Frames = null;
        }

        private void SchedulePreview()
        {
            previewTimer.Stop();
            previewTimer.Start();
        }

        private void OnPreviewTimerTick(object sender, EventArgs e)
        {
            previewTimer.Stop();
            RenderPreview();
        }

        private void RenderPreview()
        {
            bool onlyGrey = onlyGreyCheckBox.Checked;

            SetPaneImage(itemPictureBox, _itemSource == null ? null : Hued(_itemSource, onlyGrey));
            SetPaneImage(gumpPictureBox, _gumpSource == null ? null : Hued(_gumpSource, onlyGrey));

            int frameIndex = animPictureBox.FrameIndex;
            animPictureBox.Frames = _animSources.Select(f => new AnimatedFrame(Hued(f.Bitmap, onlyGrey), f.Center)).ToList();
            if (animPictureBox.Frames.Count > 0)
            {
                animPictureBox.FrameIndex = frameIndex;
                animPictureBox.Animate = true;
            }
        }

        private Bitmap Hued(Bitmap source, bool onlyGrey)
        {
            Bitmap copy = CloneBitmap(source);
            Hues.ApplyTo(copy, _colors, onlyGrey);
            return copy;
        }

        private static void SetPaneImage(PictureBox pane, Bitmap image)
        {
            Image old = pane.Image;
            pane.Image = image;
            old?.Dispose();
            FitPane(pane);
        }

        private void OnPreviewPaneResize(object sender, EventArgs e) => FitPane((PictureBox)sender);

        /// <summary>Shows the image at its own size, shrinking it only when it doesn't fit.</summary>
        private static void FitPane(PictureBox pane)
        {
            Image image = pane.Image;
            pane.SizeMode = image != null && (image.Width > pane.ClientSize.Width || image.Height > pane.ClientSize.Height)
                ? PictureBoxSizeMode.Zoom
                : PictureBoxSizeMode.CenterImage;
        }

        private void RememberPreview()
        {
            Options.HueEditorPreviewPreset = presetComboBox.SelectedItem?.ToString() ?? string.Empty;
            Options.HueEditorPreviewItemId = ParseId(itemIdTextBox);
            Options.HueEditorPreviewGumpId = ParseId(gumpIdTextBox);
            Options.HueEditorPreviewBodyId = ParseId(bodyIdTextBox);
            Options.HueEditorPreviewFacing = Facing;
        }
    }
}
