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
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using Ultima;
using Ultima.Helpers;
using Ultima.Maps;
using Ultima.Statics;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.UserControls;

namespace UoFiddler.Controls.Forms
{
    public partial class MapReplaceForm : Form
    {
        private readonly Map _workingMap;
        private Func<string, string?>? _localizationGetter;

        private CancellationTokenSource _cancellation;
        private MapSize _detectedSize;
        private bool _detectionKnown;
        private string _detectionEvidence;

        /// <summary>Built on the browsed folder so the source panel renders that install, not this one.</summary>
        private Map _sourceMap;

        /// <summary>Guards the round trip between a drag on a panel and the spinners it writes to.</summary>
        private bool _syncingPreview;

        /// <summary>
        /// Reads the z of the region so the dialog can say what a z adjustment would do to it before
        /// the copy runs. The tally is per region, not per adjustment, so turning the spinner
        /// re-answers the question without touching the files again.
        /// </summary>
        private readonly BackgroundWorker _zWorker = new BackgroundWorker();

        private readonly System.Windows.Forms.Timer _zDebounce =
            new System.Windows.Forms.Timer { Interval = 350 };

        private MapRegionZSurvey _zSurvey;
        private string _zSurveyKey;
        private ZSurveyRequest _zPending;
        private CancellationTokenSource _zCancellation;

        public void SetLocalization(Func<string, string?> getLocalized)
        {
            _localizationGetter = getLocalized;
            ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            if (_localizationGetter == null) return;

            this.Text = _localizationGetter("Forms.MapReplaceForm.Title") ?? $"Map and Statics Copy - into map {_workingMap.FileIndex}";
            groupBoxSource.Text = _localizationGetter("Forms.MapReplaceForm.GroupBoxSource") ?? "Copy from";
            labelFolder.Text = _localizationGetter("Forms.MapReplaceForm.LabelFolder") ?? "Folder:";
            buttonBrowse.Text = _localizationGetter("Forms.MapReplaceForm.ButtonBrowse") ?? "Browse...";
            labelMap.Text = _localizationGetter("Forms.MapReplaceForm.LabelMap") ?? "Map:";
            groupBoxWhat.Text = _localizationGetter("Forms.MapReplaceForm.GroupBoxWhat") ?? "Copy";
            checkBoxMap.Text = _localizationGetter("Forms.MapReplaceForm.CheckBoxMap") ?? "Map";
            labelMapFormat.Text = _localizationGetter("Forms.MapReplaceForm.LabelMapFormat") ?? "written as:";
            checkBoxStatics.Text = _localizationGetter("Forms.MapReplaceForm.CheckBoxStatics") ?? "Statics";
            RemoveDupl.Text = _localizationGetter("Forms.MapReplaceForm.RemoveDupl") ?? "remove duplicates";
            checkBoxDuplicatesHue.Text = _localizationGetter("Forms.MapReplaceForm.CheckBoxDuplicatesHue") ?? "comparing hue too (legacy)";
            groupBoxFrom.Text = _localizationGetter("Forms.MapReplaceForm.GroupBoxFrom") ?? "From region, in source map tiles";
            label1.Text = _localizationGetter("Forms.MapReplaceForm.Label1") ?? "X1";
            label2.Text = _localizationGetter("Forms.MapReplaceForm.Label2") ?? "Y1";
            label3.Text = _localizationGetter("Forms.MapReplaceForm.Label3") ?? "X2";
            label4.Text = _localizationGetter("Forms.MapReplaceForm.Label4") ?? "Y2";
            groupBoxTo.Text = _localizationGetter("Forms.MapReplaceForm.GroupBoxTo") ?? "To position, in this map's tiles";
            label6.Text = _localizationGetter("Forms.MapReplaceForm.Label6") ?? "X";
            label7.Text = _localizationGetter("Forms.MapReplaceForm.Label7") ?? "Y";
            labelZAdjust.Text = _localizationGetter("Forms.MapReplaceForm.LabelZAdjust") ?? "Z adjust";
            checkBoxZClamp.Text = _localizationGetter("Forms.MapReplaceForm.CheckBoxZClamp") ?? "hold what passes the limit";
            groupBoxPreview.Text = _localizationGetter("Forms.MapReplaceForm.GroupBoxPreview") ?? "What will be copied";
            labelSourcePreview.Text = _localizationGetter("Forms.MapReplaceForm.LabelSourcePreview") ?? "From - drag to choose the region";
            labelTargetPreview.Text = _localizationGetter("Forms.MapReplaceForm.LabelTargetPreview") ?? "To - drag to place it";
            checkBoxPreviewStatics.Text = _localizationGetter("Forms.MapReplaceForm.CheckBoxPreviewStatics") ?? "Show statics";
            checkBoxPreviewOverlay.Text = _localizationGetter("Forms.MapReplaceForm.CheckBoxPreviewOverlay") ?? "Show the piece in place";
            buttonCopy.Text = _localizationGetter("Forms.MapReplaceForm.ButtonCopy") ?? "Copy";
            buttonCancel.Text = _localizationGetter("Forms.MapReplaceForm.ButtonCancel") ?? "Cancel";
            buttonClose.Text = _localizationGetter("Forms.MapReplaceForm.ButtonClose") ?? "Close";

            // 汉化 ComboBox 项（类型 2 - 动态列表项汉化）
            ApplyComboBoxLocalization();
        }

        private void ApplyComboBoxLocalization()
        {
            if (_localizationGetter == null) return;

            bool uop = _workingMap.Tiles.IsUOPFormat;

            // 清空并重新添加翻译后的项
            comboBoxMapFormat.Items.Clear();

            // 第一项：与客户端相同格式
            string sameFormatKey = uop ? "SameFormatUOP" : "SameFormatMUL";
            string sameFormatText = _localizationGetter($"Forms.MapReplaceForm.ComboBoxFormats.{sameFormatKey}")
                ?? (uop ? "the same format as this client (.uop)" : "the same format as this client (.mul)");
            comboBoxMapFormat.Items.Add(sameFormatText);

            // 第二项：map{0}.mul
            string mapMulTemplate = _localizationGetter("Forms.MapReplaceForm.ComboBoxFormats.MapMUL") 
                ?? "map{0}.mul";
            comboBoxMapFormat.Items.Add(string.Format(mapMulTemplate, _workingMap.FileIndex));

            // 第三项：map{0}LegacyMUL.uop
            string legacyTemplate = _localizationGetter("Forms.MapReplaceForm.ComboBoxFormats.LegacyMUL")
                ?? "map{0}LegacyMUL.uop";
            comboBoxMapFormat.Items.Add(string.Format(legacyTemplate, _workingMap.FileIndex));

            comboBoxMapFormat.SelectedIndex = ClientFileSaveFormats.DefaultIndex(Options.SaveFormat);
        }

        public MapReplaceForm(Map currentMap)
        {
            InitializeComponent();

            Icon = Options.GetFiddlerIcon();

            _workingMap = currentMap ?? throw new ArgumentNullException(nameof(currentMap));

            Text = $"Map and Statics Copy - into map {_workingMap.FileIndex}";

            comboBoxMapID.BeginUpdate();
            comboBoxMapID.Items.Add(new SupportedMap(0, Options.MapNames[0] + " (old)", 6144, 4096));
            comboBoxMapID.Items.Add(new SupportedMap(0, Options.MapNames[0], 7168, 4096));
            comboBoxMapID.Items.Add(new SupportedMap(1, Options.MapNames[1] + " (old)", 6144, 4096));
            comboBoxMapID.Items.Add(new SupportedMap(1, Options.MapNames[1], 7168, 4096));
            comboBoxMapID.Items.Add(new SupportedMap(2, Options.MapNames[2], 2304, 1600));
            comboBoxMapID.Items.Add(new SupportedMap(3, Options.MapNames[3], 2560, 2048));
            comboBoxMapID.Items.Add(new SupportedMap(4, Options.MapNames[4], 1448, 1448));
            comboBoxMapID.Items.Add(new SupportedMap(5, Options.MapNames[5], 1280, 4096));
            comboBoxMapID.EndUpdate();
            comboBoxMapID.SelectedIndex = 0;

            bool uop = _workingMap.Tiles.IsUOPFormat;

            comboBoxMapFormat.Items.Add(uop
                ? "the same format as this client (.uop)"
                : "the same format as this client (.mul)");
            comboBoxMapFormat.Items.Add($"map{_workingMap.FileIndex}.mul");
            comboBoxMapFormat.Items.Add($"map{_workingMap.FileIndex}LegacyMUL.uop");
            comboBoxMapFormat.SelectedIndex = ClientFileSaveFormats.DefaultIndex(Options.SaveFormat);

            checkBoxMap.Checked = true;
            checkBoxStatics.Checked = true;

            numericUpDownToX1.Maximum = Math.Max(0, _workingMap.Width - 1);
            numericUpDownToY1.Maximum = Math.Max(0, _workingMap.Height - 1);

            textBoxFolder.TextChanged += OnFolderChanged;

            checkBoxPreviewStatics.Checked = true;
            checkBoxPreviewOverlay.Checked = true;

            previewSource.Mode = MapPreviewMode.Rectangle;
            previewSource.SelectionChanged += OnSourcePreviewChanged;

            previewTarget.Mode = MapPreviewMode.MoveFixedSize;
            previewTarget.Map = _workingMap;
            previewTarget.MapSize = new MapSize(_workingMap.Width, _workingMap.Height);
            previewTarget.SelectionChanged += OnTargetPreviewChanged;

            groupBoxPreview.SizeChanged += (sender, e) => LayoutPreviewPanels();
            LayoutPreviewPanels();

            _zDebounce.Tick += OnZDebounceTick;
            _zWorker.DoWork += OnZWorkerDoWork;
            _zWorker.RunWorkerCompleted += OnZWorkerCompleted;

            OnSourceMapChanged(this, EventArgs.Empty);
            OnOptionChanged(this, EventArgs.Empty);

            ActiveControl = buttonBrowse;
        }

        private SupportedMap SelectedMap => comboBoxMapID.SelectedItem as SupportedMap;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            FormLayout.FitToScreen(this);
        }

        /// <summary>
        /// The two panels split the group box between them. Anchors cannot express that - they would
        /// grow one panel and leave the other - so the split is arithmetic, redone whenever the group
        /// box resizes with the form.
        /// </summary>
        private void LayoutPreviewPanels()
        {
            const int margin = 16;
            const int gap = 16;

            int width = (groupBoxPreview.ClientSize.Width - (margin * 2) - gap) / 2;
            // The checkbox row and the summary sit below the panels, and the group box needs a
            // bottom margin of its own.
            int height = groupBoxPreview.ClientSize.Height - previewSource.Top - 72;

            if (width < 40 || height < 40)
            {
                return;
            }

            int right = margin + width + gap;

            labelSourcePreview.Width = width;
            labelTargetPreview.Left = right;
            labelTargetPreview.Width = width;

            previewSource.Size = new Size(width, height);
            previewTarget.Location = new Point(right, previewTarget.Top);
            previewTarget.Size = new Size(width, height);

            int below = previewSource.Bottom + 8;

            checkBoxPreviewStatics.Top = below;
            checkBoxPreviewOverlay.Top = below;

            textBoxPreview.Location = new Point(right, below - 4);
            textBoxPreview.Width = width;
        }

        private void OnClickBrowse(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog
            {
                Description = "Select the folder holding the map files to copy from",
                ShowNewFolderButton = false,
                SelectedPath = Directory.Exists(textBoxFolder.Text) ? textBoxFolder.Text : string.Empty
            })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    textBoxFolder.Text = dialog.SelectedPath;
                }
            }
        }

        private void OnFolderChanged(object sender, EventArgs e)
        {
            Detect();
        }

        /// <summary>
        /// Rebinds the from-region spinners to the selected source map. They used to be bound to the
        /// destination map, which is a different size, so a valid source coordinate could be
        /// unreachable or a reachable one rejected.
        /// </summary>
        private void OnSourceMapChanged(object sender, EventArgs e)
        {
            SupportedMap map = SelectedMap;

            if (map == null)
            {
                return;
            }

            SetMaximum(numericUpDownX1, map.Width - 1);
            SetMaximum(numericUpDownX2, map.Width - 1);
            SetMaximum(numericUpDownY1, map.Height - 1);
            SetMaximum(numericUpDownY2, map.Height - 1);

            Detect();
        }

        private static void SetMaximum(NumericUpDown control, int maximum)
        {
            control.Maximum = Math.Max(0, maximum);

            if (control.Value > control.Maximum)
            {
                control.Value = control.Maximum;
            }
        }

        /// <summary>
        /// Measures the chosen folder and says so, refusing later if it disagrees with the picked
        /// entry. Nothing used to check the two against each other, so a wrong guess read the wrong
        /// blocks or ran off the end of the index.
        /// </summary>
        private void Detect()
        {
            SupportedMap map = SelectedMap;

            _detectionKnown = false;
            _detectionEvidence = null;
            labelDetected.Text = string.Empty;
            labelSizeWarning.Text = string.Empty;

            if (map == null || !Directory.Exists(textBoxFolder.Text))
            {
                RebuildSourceMap(null);
                UpdatePreview();

                return;
            }

            _detectionKnown = MapSizes.TryDetect(textBoxFolder.Text, map.Id, out _detectedSize, out _detectionEvidence);

            RebuildSourceMap(map);

            labelDetected.Text = _detectionKnown ? $"folder holds {_detectedSize}" : "size not recognised";

            if (_detectionKnown && _detectedSize.Width == map.Width && _detectedSize.Height == map.Height)
            {
                labelSizeWarning.ForeColor = SystemColors.ControlText;
                labelSizeWarning.Text = _detectionEvidence;
            }
            else
            {
                labelSizeWarning.ForeColor = Options.DarkMode ? Color.OrangeRed : Color.Red;
                labelSizeWarning.Text = _detectionKnown
                    ? $"This folder holds {_detectedSize} for map {map.Id}, not the {map.Width}x{map.Height} selected.{Environment.NewLine}Pick the entry that matches."
                    : _detectionEvidence;
            }

            UpdatePreview();
        }

        private void OnOptionChanged(object sender, EventArgs e)
        {
            comboBoxMapFormat.Enabled = checkBoxMap.Checked;
            labelMapFormat.Enabled = checkBoxMap.Checked;
            RemoveDupl.Enabled = checkBoxStatics.Checked;
            checkBoxDuplicatesHue.Enabled = checkBoxStatics.Checked && RemoveDupl.Checked;

            UpdatePreview();
        }

        private void OnRegionChanged(object sender, EventArgs e)
        {
            UpdatePreview();
        }

        private sealed class ZSurveyRequest
        {
            public string Directory { get; init; }

            public int FileIndex { get; init; }

            public MapSize Size { get; init; }

            public BlockRectangle Region { get; init; }

            public bool Land { get; init; }

            public bool Statics { get; init; }

            public CancellationToken CancellationToken { get; set; }

            /// <summary>Everything the answer depends on. The adjustment is deliberately not part of it.</summary>
            public string Key => string.Format(CultureInfo.InvariantCulture, "{0}|{1}|{2},{3}-{4},{5}|{6}{7}",
                Directory, FileIndex, Region.BlockX1, Region.BlockY1, Region.BlockX2, Region.BlockY2,
                Land ? "L" : string.Empty, Statics ? "S" : string.Empty);
        }

        private sealed class ZSurveyAnswer
        {
            public string Key { get; init; }

            public MapRegionZSurvey Survey { get; init; }
        }

        private void UpdatePreview()
        {
            UpdatePreviewRectangles();
            UpdateZ();
        }

        /// <summary>
        /// Shows the block-snapped rectangles that will really be copied. The tile to block
        /// conversion rounds out to whole 8-tile blocks, which used to happen silently.
        /// </summary>
        private void UpdatePreviewRectangles()
        {
            SupportedMap map = SelectedMap;

            if (map == null)
            {
                textBoxPreview.Text = _localizationGetter?.Invoke("Forms.MapReplaceForm.PreviewMessages.ChooseFolderAndMap") ?? "Choose a folder and a map.";

                return;
            }

            int x1 = (int)numericUpDownX1.Value;
            int y1 = (int)numericUpDownY1.Value;
            int x2 = (int)numericUpDownX2.Value;
            int y2 = (int)numericUpDownY2.Value;

            if (x1 > x2)
            {
                (x1, x2) = (x2, x1);
            }

            if (y1 > y2)
            {
                (y1, y2) = (y2, y1);
            }

            int toX = (int)numericUpDownToX1.Value;
            int toY = (int)numericUpDownToY1.Value;

            var source = new BlockRectangle(x1 >> 3, y1 >> 3, x2 >> 3, y2 >> 3);
            var destination = new BlockRectangle(toX >> 3, toY >> 3,
                (toX >> 3) + source.BlockWidth - 1, (toY >> 3) + source.BlockHeight - 1);

            var sb = new StringBuilder();

            // The panels carry the shape now, so this is only the numbers.
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "from {0},{1} - {2},{3}   {4} x {5} blocks",
                source.TileX1, source.TileY1, source.TileX2, source.TileY2, source.BlockWidth, source.BlockHeight));
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "to   {0},{1} - {2},{3}",
                destination.TileX1, destination.TileY1, destination.TileX2, destination.TileY2));

            bool snapped = source.TileX1 != x1 || source.TileY1 != y1 || source.TileX2 != x2 || source.TileY2 != y2;

            if (snapped)
            {
                sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                    "{0},{1} - {2},{3} widened to whole blocks", x1, y1, x2, y2));
            }

            textBoxPreview.Text = sb.ToString();

            if (_syncingPreview)
            {
                return;
            }

            _syncingPreview = true;

            try
            {
                previewSource.Selection = source;
                previewTarget.Selection = destination;
                previewTarget.OverlaySelection = source;
            }
            finally
            {
                _syncingPreview = false;
            }
        }

        /// <summary>
        /// Points the source panel at the browsed folder. The panel renders through a Map of its
        /// own so it shows that install rather than the one loaded in the app.
        /// </summary>
        private void RebuildSourceMap(SupportedMap map)
        {
            _sourceMap?.Tiles.CloseStreams();
            _sourceMap = null;

            if (map != null && Directory.Exists(textBoxFolder.Text))
            {
                _sourceMap = new Map(textBoxFolder.Text, map.Id, map.Id, map.Width, map.Height);
            }

            previewSource.Map = _sourceMap;
            previewSource.MapSize = map == null ? default : new MapSize(map.Width, map.Height);
            previewSource.Message = _sourceMap == null ? (_localizationGetter?.Invoke("Forms.MapReplaceForm.PreviewMessages.ChooseFolderToCopyFrom") ?? "Choose a folder to copy from") : null;

            previewTarget.OverlayMap = checkBoxPreviewOverlay.Checked ? _sourceMap : null;
        }

        private void OnPreviewOptionChanged(object sender, EventArgs e)
        {
            previewSource.ShowStatics = checkBoxPreviewStatics.Checked;
            previewTarget.ShowStatics = checkBoxPreviewStatics.Checked;
            previewTarget.OverlayMap = checkBoxPreviewOverlay.Checked ? _sourceMap : null;

            UpdatePreview();
        }

        /// <summary>A drag on the source panel writes the region back into the spinners.</summary>
        private void OnSourcePreviewChanged(object sender, EventArgs e)
        {
            if (_syncingPreview)
            {
                return;
            }

            BlockRectangle selection = previewSource.Selection;

            _syncingPreview = true;

            try
            {
                numericUpDownX1.Value = Clamp(numericUpDownX1, selection.TileX1);
                numericUpDownY1.Value = Clamp(numericUpDownY1, selection.TileY1);
                numericUpDownX2.Value = Clamp(numericUpDownX2, selection.TileX2);
                numericUpDownY2.Value = Clamp(numericUpDownY2, selection.TileY2);
            }
            finally
            {
                _syncingPreview = false;
            }

            UpdatePreview();
        }

        /// <summary>A drag on the destination panel writes the paste position back.</summary>
        private void OnTargetPreviewChanged(object sender, EventArgs e)
        {
            if (_syncingPreview)
            {
                return;
            }

            BlockRectangle selection = previewTarget.Selection;

            _syncingPreview = true;

            try
            {
                numericUpDownToX1.Value = Clamp(numericUpDownToX1, selection.TileX1);
                numericUpDownToY1.Value = Clamp(numericUpDownToY1, selection.TileY1);
            }
            finally
            {
                _syncingPreview = false;
            }

            UpdatePreview();
        }

        private int ZAdjust => (int)numericUpDownZ.Value;

        /// <summary>
        /// Works out what the region's z is, and says whether the adjustment asked for still fits in
        /// the -128..127 a map or statics file can hold.
        /// </summary>
        private void UpdateZ()
        {
            SupportedMap map = SelectedMap;

            if (map == null || !_detectionKnown || !Directory.Exists(textBoxFolder.Text) ||
                (!checkBoxMap.Checked && !checkBoxStatics.Checked))
            {
                _zSurvey = null;
                _zSurveyKey = null;
                _zDebounce.Stop();
                labelZRange.Text = string.Empty;

                return;
            }

            var region = new BlockRectangle(
                (int)numericUpDownX1.Value >> 3, (int)numericUpDownY1.Value >> 3,
                (int)numericUpDownX2.Value >> 3, (int)numericUpDownY2.Value >> 3);

            var request = new ZSurveyRequest
            {
                Directory = textBoxFolder.Text,
                FileIndex = map.Id,
                Size = new MapSize(map.Width, map.Height),
                Region = Normalise(region),
                Land = checkBoxMap.Checked,
                Statics = checkBoxStatics.Checked
            };

            if (_zSurveyKey == request.Key)
            {
                ShowZ();

                return;
            }

            _zSurvey = null;
            _zSurveyKey = null;
            _zPending = request;

            labelZRange.ForeColor = SystemColors.ControlText;
            labelZRange.Text = "reading the heights in this region...";

            _zDebounce.Stop();
            _zDebounce.Start();
        }

        private static BlockRectangle Normalise(BlockRectangle region)
        {
            return new BlockRectangle(
                Math.Min(region.BlockX1, region.BlockX2), Math.Min(region.BlockY1, region.BlockY2),
                Math.Max(region.BlockX1, region.BlockX2), Math.Max(region.BlockY1, region.BlockY2));
        }

        private void OnZDebounceTick(object sender, EventArgs e)
        {
            _zDebounce.Stop();

            if (_zPending == null || IsDisposed)
            {
                return;
            }

            if (_zWorker.IsBusy)
            {
                // Whatever is running is for a region nobody is asking about any more.
                _zCancellation?.Cancel();
                _zDebounce.Start();

                return;
            }

            _zCancellation?.Dispose();
            _zCancellation = new CancellationTokenSource();
            _zPending.CancellationToken = _zCancellation.Token;

            _zWorker.RunWorkerAsync(_zPending);
        }

        private static void OnZWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            var request = (ZSurveyRequest)e.Argument;

            e.Result = new ZSurveyAnswer
            {
                Key = request.Key,
                Survey = MapRegionZSurvey.Survey(request.Directory, request.FileIndex, request.Size,
                    request.Region, request.Land, request.Statics, null, request.CancellationToken)
            };
        }

        private void OnZWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (IsDisposed)
            {
                return;
            }

            if (e.Error is OperationCanceledException)
            {
                // Cancelled because the region moved. Whatever replaced it is already queued.
                _zDebounce.Start();

                return;
            }

            if (e.Error != null)
            {
                labelZRange.ForeColor = Options.DarkMode ? Color.OrangeRed : Color.Red;
                labelZRange.Text = "the heights in this region could not be read: " + e.Error.Message;

                return;
            }

            if (e.Result is ZSurveyAnswer answer)
            {
                _zSurvey = answer.Survey;
                _zSurveyKey = answer.Key;
            }

            if (_zPending != null && _zPending.Key != _zSurveyKey)
            {
                // The region moved while that was in flight.
                _zDebounce.Start();

                return;
            }

            ShowZ();
        }

        /// <summary>Says what the region's z is, and what the adjustment does to it.</summary>
        private void ShowZ()
        {
            if (_zSurvey == null)
            {
                return;
            }

            int adjust = ZAdjust;

            var sb = new StringBuilder();

            // Kept short: this sits on one line beside the spinners, and an ellipsis in the middle
            // of the warning is worse than no warning at all.
            sb.Append(CultureInfo.InvariantCulture,
                $"region z  land {Span(_zSurvey.Land, 0)}, statics {Span(_zSurvey.Statics, 0)}");

            if (adjust != 0)
            {
                sb.Append(CultureInfo.InvariantCulture,
                    $"   ->   land {Span(_zSurvey.Land, adjust)}, statics {Span(_zSurvey.Statics, adjust)}");
            }

            long past = _zSurvey.OutOfRange(adjust);

            if (past == 0)
            {
                labelZRange.ForeColor = SystemColors.ControlText;
                labelZRange.Text = sb.ToString();

                return;
            }

            sb.Append(CultureInfo.InvariantCulture,
                $"   -   {past:N0} past the limit, {(checkBoxZClamp.Checked ? "held there" : "refused")}");

            labelZRange.ForeColor = Options.DarkMode ? Color.OrangeRed : Color.Red;
            labelZRange.Text = sb.ToString();
        }

        private static string Span(ZHistogram z, int adjust)
        {
            if (!z.HasTiles)
            {
                return "none";
            }

            return string.Format(CultureInfo.InvariantCulture, "{0}..{1}",
                Math.Clamp(z.Min + adjust, ZHistogram.MinZ, ZHistogram.MaxZ),
                Math.Clamp(z.Max + adjust, ZHistogram.MinZ, ZHistogram.MaxZ));
        }

        private static decimal Clamp(NumericUpDown control, int value)
        {
            return Math.Clamp(value, (int)control.Minimum, (int)control.Maximum);
        }

        private void OnClickCopy(object sender, EventArgs e)
        {
            if (worker.IsBusy)
            {
                return;
            }

            SupportedMap map = SelectedMap;

            if (map == null)
            {
                return;
            }

            if (!Directory.Exists(textBoxFolder.Text))
            {
                Fail("Choose the folder holding the map files to copy from.");

                return;
            }

            if (!checkBoxMap.Checked && !checkBoxStatics.Checked)
            {
                Fail("Nothing is selected to copy.");

                return;
            }

            if (!_detectionKnown)
            {
                Fail($"The size of map {map.Id} in that folder could not be worked out.{Environment.NewLine}{Environment.NewLine}{_detectionEvidence}");

                return;
            }

            if (_detectedSize.Width != map.Width || _detectedSize.Height != map.Height)
            {
                Fail($"That folder holds {_detectedSize} for map {map.Id}, but {map} is selected." +
                     $"{Environment.NewLine}{Environment.NewLine}{_detectionEvidence}" +
                     $"{Environment.NewLine}{Environment.NewLine}Pick the entry that matches, or the wrong blocks will be read.");

                return;
            }

            MapRegionCopyOptions options = BuildOptions(map);

            _cancellation?.Dispose();
            _cancellation = new CancellationTokenSource();
            options.CancellationToken = _cancellation.Token;
            options.Progress = new Progress<MapCopyProgress>(OnProgress);

            SetRunning(true);
            progressBar1.Value = 0;
            labelStatus.Text = "Copying...";

            worker.RunWorkerAsync(options);
        }

        private MapRegionCopyOptions BuildOptions(SupportedMap map)
        {
            int x1 = (int)numericUpDownX1.Value;
            int y1 = (int)numericUpDownY1.Value;
            int x2 = (int)numericUpDownX2.Value;
            int y2 = (int)numericUpDownY2.Value;

            var options = new MapRegionCopyOptions
            {
                SourceDirectory = textBoxFolder.Text,
                SourceFileIndex = map.Id,
                SourceSize = new MapSize(map.Width, map.Height),
                Destination = _workingMap,
                SourceX1 = x1,
                SourceY1 = y1,
                SourceX2 = x2,
                SourceY2 = y2,
                DestinationX = (int)numericUpDownToX1.Value,
                DestinationY = (int)numericUpDownToY1.Value,
                CopyLand = checkBoxMap.Checked,
                CopyStatics = checkBoxStatics.Checked,
                MapFormat = ResolveFormat(),
                ZAdjust = ZAdjust,
                ZOverflow = checkBoxZClamp.Checked ? ZOverflowAction.Clamp : ZOverflowAction.Refuse,
                OutputDirectory = Options.OutputPath
            };

            if (checkBoxStatics.Checked)
            {
                options.StaticsFilter = new StaticsTileFilter
                {
                    // The same rules this feature has always applied, so a copy keeps its old shape.
                    DropInvalidItemIds = true,
                    MaxItemId = Art.GetMaxItemId(),
                    OutOfBlockTiles = OutOfBlockAction.Keep,
                    DropInvalidZ = false,
                    NormalizeNegativeHue = true,
                    RemoveDuplicates = RemoveDupl.Checked,
                    DuplicatesCompareHue = RemoveDupl.Checked && checkBoxDuplicatesHue.Checked
                };
            }

            return options;
        }

        private MapOutputFormat ResolveFormat()
        {
            switch (comboBoxMapFormat.SelectedIndex)
            {
                case 1: return MapOutputFormat.Mul;
                case 2: return MapOutputFormat.Uop;
                default: return _workingMap.Tiles.IsUOPFormat ? MapOutputFormat.Uop : MapOutputFormat.Mul;
            }
        }

        private void OnProgress(MapCopyProgress progress)
        {
            if (progress.BlocksTotal <= 0)
            {
                return;
            }

            progressBar1.Value = Math.Min(100, Math.Max(0, (int)(progress.BlocksDone * 100L / progress.BlocksTotal)));
            labelStatus.Text = string.Format(CultureInfo.InvariantCulture, "{0}: {1:N0} of {2:N0} blocks",
                progress.Stage, progress.BlocksDone, progress.BlocksTotal);
        }

        private void OnWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = MapRegionCopier.Run((MapRegionCopyOptions)e.Argument);
        }

        private void OnWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            SetRunning(false);

            if (e.Error is OperationCanceledException)
            {
                progressBar1.Value = 0;
                labelStatus.Text = "Cancelled. Nothing was written.";

                return;
            }

            if (e.Error != null)
            {
                progressBar1.Value = 0;
                labelStatus.Text = "Failed.";
                ShowError("Map and Statics Copy", e.Error);

                return;
            }

            var result = (MapRegionCopyResult)e.Result;

            progressBar1.Value = 100;
            labelStatus.Text = "Done.";

            using (var form = new MapRegionCopyResultForm(result))
            {
                // 通过反射尝试获取 LocalizationService 的 GetString 方法
                try
                {
                    var localizationServiceType = Type.GetType("UoFiddler.Localization.LocalizationService, UoFiddler");
                    if (localizationServiceType != null)
                    {
                        var getStringMethod = localizationServiceType.GetMethod("GetString", 
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                        if (getStringMethod != null)
                        {
                            form.SetLocalization(key => (string?)getStringMethod.Invoke(null, new object[] { key }));
                        }
                    }
                }
                catch { }
                
                form.ShowDialog(this);
            }
        }

        /// <summary>
        /// Shows what actually went wrong. A bare "Object reference not set to an instance of an
        /// object" tells a user nothing and tells whoever gets the bug report even less, so the
        /// exception type and the place it came from go in the dialog and the whole thing goes to
        /// the log.
        /// </summary>
        private void ShowError(string title, Exception error)
        {
            AppLog.For(GetType()).LogError(error, "{Title} failed.", title);

            var sb = new StringBuilder();

            for (Exception current = error; current != null; current = current.InnerException)
            {
                sb.AppendLine(current.Message);

                if (current.InnerException != null)
                {
                    sb.AppendLine();
                }
            }

            sb.AppendLine();
            sb.AppendLine(error.GetType().FullName);

            string where = error.StackTrace?.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim();

            if (!string.IsNullOrEmpty(where))
            {
                sb.AppendLine(where);
            }

            MessageBox.Show(this, sb.ToString(), title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void OnClickCancel(object sender, EventArgs e)
        {
            _cancellation?.Cancel();
            labelStatus.Text = "Cancelling...";
        }

        private void OnClickClose(object sender, EventArgs e)
        {
            Close();
        }

        private void SetRunning(bool running)
        {
            buttonCopy.Enabled = !running;
            buttonCancel.Enabled = running;
            buttonClose.Enabled = !running;
            groupBoxSource.Enabled = !running;
            groupBoxWhat.Enabled = !running;
            groupBoxFrom.Enabled = !running;
            groupBoxTo.Enabled = !running;
        }

        private void Fail(string message)
        {
            MessageBox.Show(this, message, "Map and Statics Copy", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (worker.IsBusy)
            {
                _cancellation?.Cancel();
                e.Cancel = true;

                return;
            }

            base.OnFormClosing(e);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _cancellation?.Dispose();
            _cancellation = null;

            _zDebounce.Stop();
            _zDebounce.Dispose();
            _zCancellation?.Cancel();
            _zCancellation?.Dispose();
            _zCancellation = null;
            _zWorker.Dispose();

            _sourceMap?.Tiles.CloseStreams();
            _sourceMap = null;

            base.OnFormClosed(e);
        }

        /// <summary>
        /// One entry of the source-map dropdown. The sizes are the shapes a facet is known to ship
        /// in; what the chosen folder actually holds is measured separately and has to agree.
        /// </summary>
        private sealed class SupportedMap
        {
            public SupportedMap(int id, string name, int width, int height)
            {
                Id = id;
                Name = name;
                Width = width;
                Height = height;
            }

            public int Id { get; }

            private string Name { get; }

            public int Width { get; }

            public int Height { get; }

            public override string ToString()
            {
                return $"{Id} - {Name} : {Width}x{Height}";
            }
        }
    }
}