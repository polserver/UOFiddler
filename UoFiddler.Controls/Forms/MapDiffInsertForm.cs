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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using Ultima;
using Ultima.Helpers;
using Ultima.Maps;
using Ultima.Statics;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.Forms
{
    public partial class MapDiffInsertForm : Form
    {
        private readonly Map _workingMap;

        private CancellationTokenSource _cancellation;

        public MapDiffInsertForm(Map currentMap)
        {
            InitializeComponent();

            Icon = Options.GetFiddlerIcon();

            _workingMap = currentMap ?? throw new ArgumentNullException(nameof(currentMap));

            Text = $"Diff to Map Copy - map {_workingMap.FileIndex}";

            bool uop = _workingMap.Tiles.IsUOPFormat;

            comboBoxMapFormat.Items.Add(uop
                ? "the same format as this client (.uop)"
                : "the same format as this client (.mul)");
            comboBoxMapFormat.Items.Add($"map{_workingMap.FileIndex}.mul");
            comboBoxMapFormat.Items.Add($"map{_workingMap.FileIndex}LegacyMUL.uop");
            comboBoxMapFormat.SelectedIndex = 0;

            checkBoxMap.Text = "Map";
            checkBoxStatics.Text = "Statics";
            checkBoxMap.Checked = true;
            checkBoxStatics.Checked = true;

            // Exclusive bounds: a map of width W has tiles 0..W-1, and the old check let W through,
            // which becomes a block index one past the end once it is shifted.
            numericUpDownX1.Maximum = Math.Max(0, _workingMap.Width - 1);
            numericUpDownX2.Maximum = Math.Max(0, _workingMap.Width - 1);
            numericUpDownY1.Maximum = Math.Max(0, _workingMap.Height - 1);
            numericUpDownY2.Maximum = Math.Max(0, _workingMap.Height - 1);
            numericUpDownX2.Value = numericUpDownX2.Maximum;
            numericUpDownY2.Value = numericUpDownY2.Maximum;

            OnOptionChanged(this, EventArgs.Empty);

            ActiveControl = buttonCopy;
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

        /// <summary>
        /// Shows the block-snapped rectangle that will really be patched, and how much diff data
        /// there is to patch with. The tile to block conversion rounds out to whole 8-tile blocks,
        /// which used to happen silently.
        /// </summary>
        private void UpdatePreview()
        {
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

            var region = new BlockRectangle(x1 >> 3, y1 >> 3, x2 >> 3, y2 >> 3);

            TileMatrixPatch patch = _workingMap.Tiles.Patch;

            var sb = new StringBuilder();

            sb.AppendLine($"region  {region}");
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                "diff data loaded: {0:N0} land blocks, {1:N0} static blocks",
                patch.LandBlocksCount, patch.StaticBlocksCount));

            if (region.TileX1 != x1 || region.TileY1 != y1 || region.TileX2 != x2 || region.TileY2 != y2)
            {
                sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                    "the request {0},{1} - {2},{3} was widened to whole 8-tile blocks", x1, y1, x2, y2));
            }

            textBoxPreview.Text = sb.ToString();
        }

        private void OnClickCopy(object sender, EventArgs e)
        {
            if (worker.IsBusy)
            {
                return;
            }

            if (!checkBoxMap.Checked && !checkBoxStatics.Checked)
            {
                MessageBox.Show(this, "Nothing is selected to insert.", "Diff to Map Copy",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }

            var options = new MapDiffApplyOptions
            {
                Map = _workingMap,
                X1 = (int)numericUpDownX1.Value,
                Y1 = (int)numericUpDownY1.Value,
                X2 = (int)numericUpDownX2.Value,
                Y2 = (int)numericUpDownY2.Value,
                ApplyLand = checkBoxMap.Checked,
                ApplyStatics = checkBoxStatics.Checked,
                MapFormat = ResolveFormat(),
                OutputDirectory = Options.OutputPath
            };

            if (checkBoxStatics.Checked)
            {
                options.StaticsFilter = new StaticsTileFilter
                {
                    // The rules this feature has always applied, so its output keeps its old shape.
                    DropInvalidItemIds = true,
                    MaxItemId = Art.GetMaxItemId(),
                    OutOfBlockTiles = OutOfBlockAction.Keep,
                    DropInvalidZ = false,
                    NormalizeNegativeHue = true,
                    RemoveDuplicates = RemoveDupl.Checked,
                    DuplicatesCompareHue = RemoveDupl.Checked && checkBoxDuplicatesHue.Checked
                };
            }

            _cancellation?.Dispose();
            _cancellation = new CancellationTokenSource();
            options.CancellationToken = _cancellation.Token;
            options.Progress = new Progress<MapCopyProgress>(OnProgress);

            SetRunning(true);
            progressBar1.Value = 0;
            labelStatus.Text = "Inserting...";

            worker.RunWorkerAsync(options);
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
            e.Result = MapDiffApplier.Run((MapDiffApplyOptions)e.Argument);
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

                ShowError("Diff to Map Copy", e.Error);

                return;
            }

            var result = (MapDiffApplyResult)e.Result;

            progressBar1.Value = 100;
            labelStatus.Text = "Done.";

            using (var form = new MapDiffApplyResultForm(result))
            {
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
            groupBoxWhat.Enabled = !running;
            groupBoxFrom.Enabled = !running;
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
