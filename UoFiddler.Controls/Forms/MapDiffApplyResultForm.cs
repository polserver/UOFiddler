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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Ultima;
using Ultima.Maps;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.Forms
{
    public sealed partial class MapDiffApplyResultForm : Form
    {
        private readonly MapDiffApplyResult _result;

        public MapDiffApplyResultForm(MapDiffApplyResult result)
        {
            InitializeComponent();

            Icon = Options.GetFiddlerIcon();

            _result = result;

            reportTextBox.Text = result.ToReport();

            buttonVerify.Enabled = result.OutputMapPath != null;
            buttonOpenFolder.Enabled = result.OutputMapPath != null || result.OutputIndexPath != null;
        }

        /// <summary>
        /// Reads the written map back and checks every block against what it should hold: the patch
        /// where the diff covers a block inside the region, the unpatched map everywhere else.
        /// </summary>
        private void OnClickVerify(object sender, EventArgs e)
        {
            using (new WaitCursorScope(this))
            {
                try
                {
                    reportTextBox.Text = Verify() + Environment.NewLine +
                                         new string('-', 60) + Environment.NewLine + _result.ToReport();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "Verify failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private string Verify()
        {
            var sb = new StringBuilder();

            MapSize size = _result.MapSize;
            string outputDirectory = Path.GetDirectoryName(_result.OutputMapPath);

            var written = new TileMatrix(_result.FileIndex, _result.FileIndex, size.Width, size.Height, outputDirectory);
            var original = new TileMatrix(_result.FileIndex, _result.FileIndex, size.Width, size.Height, null);

            try
            {
                var actual = new byte[TileMatrix.MapBlockSize];
                var expected = new byte[TileMatrix.MapBlockSize];

                long fromDiff = 0;
                long untouched = 0;
                long mismatches = 0;
                string firstMismatch = null;

                TileMatrixPatch patch = original.Patch;

                for (int x = 0; x < size.BlockWidth; ++x)
                {
                    for (int y = 0; y < size.BlockHeight; ++y)
                    {
                        bool inRegion = x >= _result.Region.BlockX1 && x <= _result.Region.BlockX2 &&
                                        y >= _result.Region.BlockY1 && y <= _result.Region.BlockY2;

                        bool patched = inRegion && patch.IsLandBlockPatched(x, y);

                        written.ReadLandBlockBytes(x, y, actual);

                        if (patched)
                        {
                            ++fromDiff;

                            Array.Clear(expected);
                            MemoryMarshal.AsBytes(patch.GetLandBlock(x, y).AsSpan())
                                .CopyTo(expected.AsSpan(TileMatrix.BlockHeaderSize));
                        }
                        else
                        {
                            ++untouched;
                            original.ReadLandBlockBytes(x, y, expected);
                        }

                        if (actual.AsSpan().SequenceEqual(expected))
                        {
                            continue;
                        }

                        ++mismatches;

                        firstMismatch ??= Line("block {0},{1} (world {2},{3}) {4}",
                            x, y, x << 3, y << 3,
                            patched ? "does not match the diff" : "does not match the unpatched map");
                    }
                }

                bool countsAgree = fromDiff == _result.LandBlocksApplied;

                sb.AppendLine(mismatches == 0 && countsAgree ? "PASSED" : "FAILED");
                sb.AppendLine();
                sb.AppendLine(Line("Read back        : {0}", _result.OutputMapPath));
                sb.AppendLine(Line("Blocks compared  : {0:N0}", fromDiff + untouched));
                sb.AppendLine(Line("  from the diff  : {0:N0}", fromDiff));
                sb.AppendLine(Line("  left alone     : {0:N0}", untouched));
                sb.AppendLine(Line("Blocks differing : {0:N0}", mismatches));

                if (firstMismatch != null)
                {
                    sb.AppendLine(Line("First difference : {0}", firstMismatch));
                }

                if (!countsAgree)
                {
                    sb.AppendLine(Line("Counts disagree with the insert report, which says {0:N0} blocks came from the diff.",
                        _result.LandBlocksApplied));
                }
            }
            finally
            {
                written.CloseStreams();
                original.CloseStreams();
            }

            return sb.ToString();
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog
            {
                Title = "Save the insert report",
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                FileName = "map-diff-insert.txt",
                InitialDirectory = Options.OutputPath
            })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    File.WriteAllText(dialog.FileName, reportTextBox.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "Save failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OnClickCopy(object sender, EventArgs e)
        {
            if (reportTextBox.TextLength > 0)
            {
                Clipboard.SetText(reportTextBox.Text);
            }
        }

        private void OnClickOpenFolder(object sender, EventArgs e)
        {
            string folder = Path.GetDirectoryName(_result.OutputMapPath ?? _result.OutputIndexPath);

            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
            {
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo { FileName = folder, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Unable to open folder: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string Line(string format, params object[] args)
        {
            return string.Format(CultureInfo.InvariantCulture, format, args);
        }
    }
}