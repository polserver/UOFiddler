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
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Ultima;
using Ultima.Statics;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.Forms
{
    public sealed partial class MapDefragStaticsForm : Form
    {
        private const int IndexRecordSize = 12;

        /// <summary>Water, which is what the stack filter was written for.</summary>
        private const string DefaultCollapseIds = "0x1797-0x179C";

        private readonly Map _map;

        private CancellationTokenSource _cancellation;
        private bool _lastRunUsedFilters;
        private bool _truncationAvailable;

        public MapDefragStaticsForm(Map map, string outputPath)
        {
            InitializeComponent();

            Icon = Options.GetFiddlerIcon();

            _map = map ?? throw new ArgumentNullException(nameof(map));

            comboBoxIdCeiling.Items.AddRange(new object[] { "tiledata", "art", "legacy 0x3FFF" });
            comboBoxIdCeiling.SelectedIndex = 0;

            comboBoxOutOfBlock.Items.AddRange(new object[] { "Drop", "Mask to 0-7", "Keep" });
            comboBoxOutOfBlock.SelectedIndex = 0;

            checkBoxDropInvalidIds.Checked = true;
            checkBoxDropInvalidZ.Checked = true;
            checkBoxNormalizeHue.Checked = true;
            checkBoxRemoveDuplicates.Checked = true;
            checkBoxCollapseWet.Checked = true;
            textBoxCollapseIds.Text = DefaultCollapseIds;

            textBoxOutput.Text = outputPath;

            DescribeSource();
            OnFilterChanged(this, EventArgs.Empty);

            ActiveControl = buttonAnalyze;
        }

        /// <summary>
        /// Shows what is about to be read and how it lines up with the configured map size. The
        /// engine performs the authoritative check; this is here so the mismatch is visible before
        /// anyone presses a button.
        /// </summary>
        private void DescribeSource()
        {
            string indexPath = Files.GetFilePath($"staidx{_map.FileIndex}.mul");
            string staticsPath = Files.GetFilePath($"statics{_map.FileIndex}.mul");

            var sb = new StringBuilder();

            sb.AppendLine($"Facet {_map.FileIndex}   map size {_map.Width} x {_map.Height}   blocks {_map.Width >> 3} x {_map.Height >> 3}");

            if (indexPath == null || staticsPath == null)
            {
                sb.AppendLine($"staidx{_map.FileIndex}.mul or statics{_map.FileIndex}.mul was not found in the loaded client.");

                textBoxSource.Text = sb.ToString();
                buttonAnalyze.Enabled = false;
                buttonDefrag.Enabled = false;

                return;
            }

            long indexLength = new FileInfo(indexPath).Length;
            long staticsLength = new FileInfo(staticsPath).Length;

            sb.AppendLine($"{indexPath}  ({indexLength:N0} bytes)");
            sb.AppendLine($"{staticsPath}  ({staticsLength:N0} bytes)");

            if (File.Exists(Path.Combine(Path.GetDirectoryName(indexPath) ?? string.Empty, $"staidx{_map.FileIndex}x.mul")))
            {
                sb.AppendLine($"staidx{_map.FileIndex}x.mul is present - the client prefers those override files over this pair.");
            }

            textBoxSource.Text = sb.ToString();

            long indexBlocks = indexLength / IndexRecordSize;
            long configuredBlocks = (long)(_map.Width >> 3) * (_map.Height >> 3);

            if (indexBlocks > configuredBlocks)
            {
                labelGeometry.ForeColor = Options.DarkMode ? Color.OrangeRed : Color.Red;
                labelGeometry.Text = string.Format(CultureInfo.InvariantCulture,
                    "staidx holds {0:N0} blocks but the configured map size covers only {1:N0}." + Environment.NewLine +
                    "The statics in the surplus blocks would be discarded.",
                    indexBlocks, configuredBlocks);

                _truncationAvailable = true;
                checkBoxAllowTruncation.Enabled = true;
            }
            else if (indexBlocks < configuredBlocks)
            {
                labelGeometry.ForeColor = SystemColors.ControlText;
                labelGeometry.Text = string.Format(CultureInfo.InvariantCulture,
                    "staidx holds {0:N0} blocks, the configured map size covers {1:N0}." + Environment.NewLine +
                    "The blocks past the end of the index will be written empty.",
                    indexBlocks, configuredBlocks);
            }
            else
            {
                labelGeometry.ForeColor = SystemColors.ControlText;
                labelGeometry.Text = string.Format(CultureInfo.InvariantCulture,
                    "staidx holds {0:N0} blocks, matching the configured map size.", indexBlocks);
            }
        }

        private void OnFilterChanged(object sender, EventArgs e)
        {
            comboBoxIdCeiling.Enabled = checkBoxDropInvalidIds.Checked;
            labelCeilingValue.Text = checkBoxDropInvalidIds.Checked
                ? $"= 0x{BuildOptions(true).ResolveMaxItemId():X4}"
                : string.Empty;

            checkBoxDuplicatesHue.Enabled = checkBoxRemoveDuplicates.Checked;

            bool collapse = checkBoxCollapseStacks.Checked;
            checkBoxCollapseWet.Enabled = collapse;
            checkBoxCollapseSurface.Enabled = collapse;
            checkBoxCollapseIgnoreZ.Enabled = collapse;
            textBoxCollapseIds.Enabled = collapse;
        }

        private StaticsDefragOptions BuildOptions(bool dryRun)
        {
            var options = new StaticsDefragOptions
            {
                FileIndex = _map.FileIndex,
                Map = _map,
                BlockWidth = _map.Width >> 3,
                BlockHeight = _map.Height >> 3,
                AllowGeometryTruncation = checkBoxAllowTruncation.Checked,
                OutputDirectory = textBoxOutput.Text,
                DryRun = dryRun,
                DropInvalidItemIds = checkBoxDropInvalidIds.Checked,
                ItemIdCeiling = comboBoxIdCeiling.SelectedIndex switch
                {
                    1 => ItemIdCeiling.Art,
                    2 => ItemIdCeiling.Legacy,
                    _ => ItemIdCeiling.TileData
                },
                OutOfBlockTiles = comboBoxOutOfBlock.SelectedIndex switch
                {
                    1 => OutOfBlockAction.Mask,
                    2 => OutOfBlockAction.Keep,
                    _ => OutOfBlockAction.Drop
                },
                DropInvalidZ = checkBoxDropInvalidZ.Checked,
                NormalizeNegativeHue = checkBoxNormalizeHue.Checked,
                DropBelowTerrain = checkBoxBelowTerrain.Checked,
                RemoveDuplicates = checkBoxRemoveDuplicates.Checked,
                DuplicatesCompareHue = checkBoxRemoveDuplicates.Checked && checkBoxDuplicatesHue.Checked,
                CollapseStacks = checkBoxCollapseStacks.Checked,
                CollapseIgnoreZ = checkBoxCollapseIgnoreZ.Checked,
                SortTiles = checkBoxSortTiles.Checked
            };

            if (options.CollapseStacks)
            {
                TileFlag mask = 0;

                if (checkBoxCollapseWet.Checked)
                {
                    mask |= TileFlag.Wet;
                }

                if (checkBoxCollapseSurface.Checked)
                {
                    mask |= TileFlag.Surface;
                }

                options.CollapseFlagMask = mask;
                options.CollapseIds.UnionWith(ParseIds(textBoxCollapseIds.Text));
            }
            else
            {
                options.CollapseFlagMask = 0;
            }

            return options;
        }

        /// <summary>
        /// Accepts decimal and 0x values separated by commas or spaces, plus inclusive ranges
        /// written with a dash, so "0x1797-0x179C, 6100" works.
        /// </summary>
        private static IEnumerable<int> ParseIds(string text)
        {
            var ids = new List<int>();

            if (string.IsNullOrWhiteSpace(text))
            {
                return ids;
            }

            foreach (string part in text.Split(new[] { ',', ';', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries))
            {
                int dash = part.IndexOf('-', 1);

                if (dash > 0)
                {
                    if (TryParseId(part.Substring(0, dash), out int from) &&
                        TryParseId(part.Substring(dash + 1), out int to) && to >= from)
                    {
                        for (int id = from; id <= to; ++id)
                        {
                            ids.Add(id);
                        }
                    }

                    continue;
                }

                if (TryParseId(part, out int single))
                {
                    ids.Add(single);
                }
            }

            return ids;
        }

        private static bool TryParseId(string text, out int id)
        {
            text = text.Trim();

            if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return int.TryParse(text.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out id);
            }

            return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out id);
        }

        private void OnClickBrowse(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog
            {
                Description = "Choose where the rewritten statics files go",
                SelectedPath = Directory.Exists(textBoxOutput.Text) ? textBoxOutput.Text : Options.OutputPath
            })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    textBoxOutput.Text = dialog.SelectedPath;
                }
            }
        }

        private void OnClickAnalyze(object sender, EventArgs e)
        {
            Start(true);
        }

        private void OnClickDefrag(object sender, EventArgs e)
        {
            Start(false);
        }

        private void Start(bool dryRun)
        {
            if (worker.IsBusy)
            {
                return;
            }

            StaticsDefragOptions options;

            try
            {
                options = BuildOptions(dryRun);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Defrag Statics", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _lastRunUsedFilters = UsesFilters(options);

            _cancellation?.Dispose();
            _cancellation = new CancellationTokenSource();
            options.CancellationToken = _cancellation.Token;
            options.Progress = new Progress<StaticsDefragProgress>(OnProgress);

            SetRunning(true);

            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.Value = 0;
            labelStatus.Text = dryRun ? "Analyzing..." : "Defragging...";

            worker.RunWorkerAsync(options);
        }

        /// <summary>
        /// Whether the run could remove anything. Decides which mode the verification runs in.
        /// </summary>
        private static bool UsesFilters(StaticsDefragOptions options)
        {
            return options.DropInvalidItemIds ||
                   options.OutOfBlockTiles != OutOfBlockAction.Keep ||
                   options.DropInvalidZ ||
                   options.NormalizeNegativeHue ||
                   options.DropBelowTerrain ||
                   options.RemoveDuplicates ||
                   options.CollapseStacks;
        }

        private void OnProgress(StaticsDefragProgress progress)
        {
            if (progress.BlocksTotal <= 0)
            {
                return;
            }

            int percent = (int)(progress.BlocksDone * 100L / progress.BlocksTotal);

            progressBar.Value = Math.Min(100, Math.Max(0, percent));
            labelStatus.Text = string.Format(CultureInfo.InvariantCulture,
                "{0:N0} of {1:N0} blocks, {2:N0} statics written", progress.BlocksDone, progress.BlocksTotal, progress.TilesWritten);
        }

        private void OnWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = StaticsDefragmenter.Defrag((StaticsDefragOptions)e.Argument);
        }

        private void OnWorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = Math.Min(100, Math.Max(0, e.ProgressPercentage));
        }

        private void OnWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            SetRunning(false);

            if (e.Error is OperationCanceledException)
            {
                labelStatus.Text = "Cancelled. Nothing was written.";
                progressBar.Value = 0;
                return;
            }

            if (e.Error != null)
            {
                labelStatus.Text = "Failed.";
                progressBar.Value = 0;

                MessageBox.Show(this, e.Error.Message, "Defrag Statics", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var result = (StaticsDefragResult)e.Result;

            progressBar.Value = 100;
            labelStatus.Text = result.DryRun
                ? "Analyzed. Nothing was written."
                : $"Done. {result.TilesWritten:N0} statics written.";

            using (var form = new MapDefragStaticsResultForm(result, _lastRunUsedFilters))
            {
                form.ShowDialog(this);
            }
        }

        private void OnClickCancel(object sender, EventArgs e)
        {
            _cancellation?.Cancel();
            labelStatus.Text = "Cancelling...";
        }

        private void SetRunning(bool running)
        {
            buttonAnalyze.Enabled = !running;
            buttonDefrag.Enabled = !running;
            buttonClose.Enabled = !running;
            buttonCancel.Enabled = running;
            groupBoxFilters.Enabled = !running;
            groupBoxOutput.Enabled = !running;
            checkBoxAllowTruncation.Enabled = !running && _truncationAvailable;
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

            base.OnFormClosed(e);
        }
    }
}