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
using System.Text;
using System.Windows.Forms;
using Ultima;
using Ultima.Maps;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.Forms
{
    public sealed partial class MapRegionCopyResultForm : Form
    {
        private readonly MapRegionCopyResult _result;

        public MapRegionCopyResultForm(MapRegionCopyResult result)
        {
            InitializeComponent();

            Icon = Options.GetFiddlerIcon();

            _result = result;

            reportTextBox.Text = result.ToReport();

            buttonVerify.Enabled = result.OutputMapPath != null;
            buttonOpenFolder.Enabled = result.OutputMapPath != null || result.OutputIndexPath != null;
        }

        /// <summary>
        /// Reads the written map back and checks it block by block: inside the pasted rectangle it
        /// has to match the source, everywhere else the map it was made from.
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

            MapSize destination = _result.DestinationSize;
            MapSize source = _result.SourceSize;

            string outputDirectory = Path.GetDirectoryName(_result.OutputMapPath);

            // Three views: what was written, where the region came from, and the map it replaced.
            var written = new TileMatrix(_result.DestinationFileIndex, _result.DestinationFileIndex,
                destination.Width, destination.Height, outputDirectory);
            var from = new TileMatrix(_result.SourceFileIndex, _result.SourceFileIndex,
                source.Width, source.Height, _result.SourceDirectory);
            var original = new TileMatrix(_result.DestinationFileIndex, _result.DestinationFileIndex,
                destination.Width, destination.Height, null);

            try
            {
                var actual = new byte[TileMatrix.MapBlockSize];
                var expected = new byte[TileMatrix.MapBlockSize];

                long inRegion = 0;
                long carried = 0;
                long mismatches = 0;
                string firstMismatch = null;

                for (int x = 0; x < destination.BlockWidth; ++x)
                {
                    for (int y = 0; y < destination.BlockHeight; ++y)
                    {
                        bool copied = x >= _result.DestinationRegion.BlockX1 && x <= _result.DestinationRegion.BlockX2 &&
                                      y >= _result.DestinationRegion.BlockY1 && y <= _result.DestinationRegion.BlockY2;

                        written.ReadLandBlockBytes(x, y, actual);

                        if (copied)
                        {
                            ++inRegion;
                            from.ReadLandBlockBytes(
                                x - _result.DestinationRegion.BlockX1 + _result.Source.BlockX1,
                                y - _result.DestinationRegion.BlockY1 + _result.Source.BlockY1,
                                expected);
                        }
                        else
                        {
                            ++carried;
                            original.ReadLandBlockBytes(x, y, expected);
                        }

                        if (actual.AsSpan().SequenceEqual(expected))
                        {
                            continue;
                        }

                        ++mismatches;

                        firstMismatch ??= Line("block {0},{1} (world {2},{3}) {4}",
                            x, y, x << 3, y << 3, copied ? "does not match the source" : "does not match the original map");
                    }
                }

                bool countsAgree = inRegion == _result.LandBlocksCopied && carried == _result.LandBlocksCarried;

                sb.AppendLine(mismatches == 0 && countsAgree ? "PASSED" : "FAILED");
                sb.AppendLine();
                sb.AppendLine(Line("Read back        : {0}", _result.OutputMapPath));
                sb.AppendLine(Line("Blocks compared  : {0:N0}", inRegion + carried));
                sb.AppendLine(Line("  from the source: {0:N0}", inRegion));
                sb.AppendLine(Line("  carried over   : {0:N0}", carried));
                sb.AppendLine(Line("Blocks differing : {0:N0}", mismatches));

                if (firstMismatch != null)
                {
                    sb.AppendLine(Line("First difference : {0}", firstMismatch));
                }

                if (!countsAgree)
                {
                    sb.AppendLine(Line("Counts disagree with the copy report: it says {0:N0} copied and {1:N0} carried.",
                        _result.LandBlocksCopied, _result.LandBlocksCarried));
                }
            }
            finally
            {
                written.CloseStreams();
                from.CloseStreams();
                original.CloseStreams();
            }

            return sb.ToString();
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog
            {
                Title = "Save the copy report",
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                FileName = "map-copy.txt",
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