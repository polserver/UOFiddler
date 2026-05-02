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
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;
using UoFiddler.Plugin.UopPacker.Classes;

namespace UoFiddler.Plugin.UopPacker.UserControls
{
    public partial class UopPackerControl : UserControl
    {
        private readonly LegacyMulFileConverter _conv;

        private UopPackerControl()
        {
            InitializeComponent();

            _conv = new LegacyMulFileConverter();

            var fileTypes = Enum.GetNames(typeof(FileType));

            multype.SelectedIndexChanged += OnMulTypeChanged;
            uoptype.SelectedIndexChanged += OnUopTypeChanged;
            mulMapIndex.ValueChanged += OnMulTypeChanged;
            uopMapIndex.ValueChanged += OnUopTypeChanged;

            uoptype.DataSource = new System.Collections.Generic.List<string>(fileTypes);
            multype.DataSource = new System.Collections.Generic.List<string>(fileTypes);

            mulMapIndex.ReadOnly = uopMapIndex.ReadOnly = true;

            // Single output folder for both directions; individual filenames are derived from the selected FileType.
            outfolder.PlaceholderText = "folder where .mul/.idx will be written";
            outuopfolder.PlaceholderText = "folder where .uop will be written";

            packAllGumpCompressionBox.SelectedIndex = 0;
            compressionBox.SelectedIndex = 0;
            extract.CheckedChanged += OnPackAllModeChanged;
            pack.CheckedChanged += OnPackAllModeChanged;
            UpdatePackAllCompressionVisibility();

            RefreshMulTypeUi();
            RefreshUopTypeUi();

            ApplyDarkModeIfNeeded();

            Dock = DockStyle.Fill;
        }

        private void ApplyDarkModeIfNeeded()
        {
            if (!Options.DarkMode)
            {
                return;
            }

            Color tabBg = Color.FromArgb(32, 32, 32);
            ExtractAllFilesTabPage.UseVisualStyleBackColor = false;
            ExtractAllFilesTabPage.BackColor = tabBg;
            ExtractSingleFileTabPage.UseVisualStyleBackColor = false;
            ExtractSingleFileTabPage.BackColor = tabBg;

            // Reset hardcoded white BackColors so dark mode visual styles apply.
            TextBox[] whiteTextBoxes =
            {
                inmul, inidx, inhousingbin, outuopfolder, inuop, outfolder, inputfolder, outputfolder
            };
            foreach (var tb in whiteTextBoxes)
            {
                tb.BackColor = SystemColors.Window;
            }

            multype.BackColor = SystemColors.Window;
            uoptype.BackColor = SystemColors.Window;
            mulMapIndex.BackColor = SystemColors.Window;
            uopMapIndex.BackColor = SystemColors.Window;
            packAllGumpCompressionBox.BackColor = SystemColors.Window;
            packAllHousingBin.BackColor = SystemColors.Window;
            compressionBox.BackColor = SystemColors.Window;

            statustext.ForeColor = Color.OrangeRed;
        }

        private void OnPackAllModeChanged(object sender, EventArgs e) => UpdatePackAllCompressionVisibility();

        private void UpdatePackAllCompressionVisibility()
        {
            bool show = pack.Checked;
            packAllGumpCompressionLabel.Visible = show;
            packAllGumpCompressionBox.Visible = show;
            packAllHousingBinLabel.Visible = show;
            packAllHousingBin.Visible = show;
            packAllHousingBinBtn.Visible = show;
        }

        private static (string mul, string idx, string uop) GetConventionalNames(FileType type, int mapIndex)
        {
            return type switch
            {
                FileType.ArtLegacyMul => ("art.mul", "artidx.mul", "artLegacyMUL.uop"),
                FileType.GumpartLegacyMul => ("gumpart.mul", "gumpidx.mul", "gumpartLegacyMUL.uop"),
                FileType.MapLegacyMul => ($"map{mapIndex}.mul", null, $"map{mapIndex}LegacyMUL.uop"),
                FileType.SoundLegacyMul => ("sound.mul", "soundidx.mul", "soundLegacyMUL.uop"),
                FileType.MultiCollection => ("multi.mul", "multi.idx", "MultiCollection.uop"),
                _ => ("", "", "")
            };
        }

        private void OnMulTypeChanged(object sender, EventArgs e) => RefreshMulTypeUi();

        private void OnUopTypeChanged(object sender, EventArgs e) => RefreshUopTypeUi();

        private void RefreshMulTypeUi()
        {
            if (multype == null || !Enum.TryParse(multype.SelectedValue?.ToString() ?? string.Empty, out FileType type))
            {
                return;
            }

            bool isMap = type == FileType.MapLegacyMul;
            bool isMulti = type == FileType.MultiCollection;

            inidx.Enabled = inidxbtn.Enabled = !isMap;
            mulMapIndex.Enabled = isMap;

            inhousingbin.Visible = inhousingbinbtn.Visible = labelHousingBin.Visible = isMulti;

            // Previously-picked paths belong to the old type; clear them so the user can't accidentally
            // run a conversion against the wrong file.
            inmul.Text = string.Empty;
            inidx.Text = string.Empty;
            inhousingbin.Text = string.Empty;

            var (mulName, idxName, uopName) = GetConventionalNames(type, (int)mulMapIndex.Value);
            inmul.PlaceholderText = mulName;
            inidx.PlaceholderText = idxName ?? string.Empty;
            inhousingbin.PlaceholderText = "housing.bin";

            outputUopFileLabel.Text = string.IsNullOrEmpty(uopName) ? string.Empty : "Will create: " + uopName;
        }

        private void RefreshUopTypeUi()
        {
            if (uoptype == null || !Enum.TryParse(uoptype.SelectedValue?.ToString() ?? string.Empty, out FileType type))
            {
                return;
            }

            bool isMap = type == FileType.MapLegacyMul;
            bool isMulti = type == FileType.MultiCollection;

            uopMapIndex.Enabled = isMap;

            inuop.Text = string.Empty;

            var (mulName, idxName, uopName) = GetConventionalNames(type, (int)uopMapIndex.Value);
            inuop.PlaceholderText = uopName;

            // Preview what will be written under the output folder.
            string preview = idxName != null ? $"{mulName}, {idxName}" : mulName;
            if (isMulti)
            {
                preview += ", housing.bin";
            }
            outputFilesLabel.Text = "Will create: " + preview;
        }

        public UopPackerControl(string version) : this()
        {
            VersionLabel.Text = version;
        }

        private void InputMulSelect(object sender, EventArgs e)
        {
            FileDialog.FilterIndex = 1;

            if (FileDialog.ShowDialog() == DialogResult.OK)
            {
                inmul.Text = FileDialog.FileName;
            }
        }

        private void InputIdxSelect(object sender, EventArgs e)
        {
            FileDialog.FilterIndex = 3;

            if (FileDialog.ShowDialog() == DialogResult.OK)
            {
                inidx.Text = FileDialog.FileName;
            }
        }

        private void OutputUopFolderSelect(object sender, EventArgs e)
        {
            if (FolderDialog.ShowDialog() == DialogResult.OK)
            {
                outuopfolder.Text = FolderDialog.SelectedPath;
            }
        }

        private async void ToUop(object sender, EventArgs e)
        {
            var selectedFileType = multype?.SelectedValue?.ToString() ?? string.Empty;
            if (!Enum.TryParse(selectedFileType, out FileType fileType))
            {
                MessageBox.Show("You must specify input type");
                return;
            }

            if (inmul.Text.Length == 0)
            {
                MessageBox.Show("You must specify the input mul");
                return;
            }

            if (!File.Exists(inmul.Text))
            {
                MessageBox.Show("The input mul does not exist");
                return;
            }

            if (inidx.Text.Length == 0 && fileType != FileType.MapLegacyMul)
            {
                MessageBox.Show("You must specify the input idx");
                return;
            }

            if (fileType != FileType.MapLegacyMul && !File.Exists(inidx.Text))
            {
                MessageBox.Show("The input idx does not exist");
                return;
            }

            if (outuopfolder.Text.Length == 0)
            {
                MessageBox.Show("You must specify the output folder");
                return;
            }

            if (!Directory.Exists(outuopfolder.Text))
            {
                MessageBox.Show("The output folder does not exist");
                return;
            }

            string housingBin = string.Empty;
            if (fileType == FileType.MultiCollection)
            {
                housingBin = inhousingbin.Text;
                if (!string.IsNullOrWhiteSpace(housingBin) && !File.Exists(housingBin))
                {
                    MessageBox.Show("The input housing.bin does not exist");
                    return;
                }
            }

            var (_, _, uopName) = GetConventionalNames(fileType, (int)mulMapIndex.Value);
            string outUopPath = Path.Combine(outuopfolder.Text, uopName);
            string inIdxPath = fileType == FileType.MapLegacyMul ? null : inidx.Text;

            if (File.Exists(outUopPath))
            {
                var prompt = MessageBox.Show(
                    $"{uopName} already exists in the output folder and will be overwritten.\n\nProceed?",
                    "Overwrite existing file",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                if (prompt != DialogResult.Yes)
                {
                    return;
                }
                // ToUop opens output with FileMode.Create, which overwrites.
            }

            CompressionFlag selectedCompressionMethod = CompressionFlag.None;
            if (compressionBox.SelectedItem != null)
            {
                Enum.TryParse(compressionBox.SelectedItem.ToString(), out selectedCompressionMethod);
            }

            bool succeeded = false;
            string inMul = inmul.Text;
            int mapIdx = (int)mulMapIndex.Value;
            try
            {
                multouop.Text = "Converting...";
                multouop.Enabled = false;
                uoptomul.Enabled = false;
                singleFileProgressBar.Value = 0;
                singleFileProgressBar.Visible = true;
                label10.Visible = true;

                var progress = new Progress<int>(p => singleFileProgressBar.Value = Math.Min(100, Math.Max(0, p)));

                await Task.Run(() => LegacyMulFileConverter.ToUop(inMul, inIdxPath, outUopPath, fileType, mapIdx, selectedCompressionMethod, housingBin, progress));
                succeeded = true;
            }
            catch (Exception ex)
            {
                LogConverterError(ex, nameof(ToUop), inMul, outUopPath, fileType);
                MessageBox.Show($"An error occurred.\r\n{ex.Message}");
            }
            finally
            {
                multouop.Text = "Convert";
                multouop.Enabled = true;
                uoptomul.Enabled = true;
                singleFileProgressBar.Visible = false;
                label10.Visible = false;
            }

            if (succeeded)
            {
                FileSavedDialog.Show(FindForm(), outUopPath, "UOP file saved successfully.", "Conversion complete");
            }
        }

        private void InputHousingBinSelect(object sender, EventArgs e)
        {
            FileDialog.FilterIndex = 4;

            if (FileDialog.ShowDialog() == DialogResult.OK)
            {
                inhousingbin.Text = FileDialog.FileName;
            }
        }

        private void OutFolderSelect(object sender, EventArgs e)
        {
            if (FolderDialog.ShowDialog() == DialogResult.OK)
            {
                outfolder.Text = FolderDialog.SelectedPath;
            }
        }

        private void InputUopSelect(object sender, EventArgs e)
        {
            FileDialog.FilterIndex = 2;

            if (FileDialog.ShowDialog() == DialogResult.OK)
            {
                inuop.Text = FileDialog.FileName;
            }
        }

        private async void ToMul(object sender, EventArgs e)
        {
            var selectedFileType = uoptype?.SelectedValue?.ToString() ?? string.Empty;
            if (!Enum.TryParse(selectedFileType, out FileType fileType))
            {
                MessageBox.Show("You must specify input type");
                return;
            }

            if (inuop.Text.Length == 0)
            {
                MessageBox.Show("You must specify the input uop");
                return;
            }

            if (!File.Exists(inuop.Text))
            {
                MessageBox.Show("The input file does not exist");
                return;
            }

            if (outfolder.Text.Length == 0)
            {
                MessageBox.Show("You must specify the output folder");
                return;
            }

            if (!Directory.Exists(outfolder.Text))
            {
                MessageBox.Show("The output folder does not exist");
                return;
            }

            int mapIdx = (int)uopMapIndex.Value;
            var (mulName, idxName, _) = GetConventionalNames(fileType, mapIdx);

            string outMulPath = Path.Combine(outfolder.Text, mulName);
            string outIdxPath = idxName != null ? Path.Combine(outfolder.Text, idxName) : null;
            string housingBinPath = fileType == FileType.MultiCollection
                ? Path.Combine(outfolder.Text, "housing.bin")
                : string.Empty;

            var conflicts = new System.Collections.Generic.List<string>();
            if (File.Exists(outMulPath))
            {
                conflicts.Add(mulName);
            }

            if (outIdxPath != null && File.Exists(outIdxPath))
            {
                conflicts.Add(idxName);
            }

            if (!string.IsNullOrEmpty(housingBinPath) && File.Exists(housingBinPath))
            {
                conflicts.Add("housing.bin");
            }

            if (conflicts.Count > 0)
            {
                var prompt = MessageBox.Show(
                    $"These files already exist in the output folder and will be overwritten:\n\n  {string.Join("\n  ", conflicts)}\n\nProceed?",
                    "Overwrite existing files",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                if (prompt != DialogResult.Yes)
                {
                    return;
                }
                // FromUop opens outputs with FileMode.Create, which overwrites.
            }

            bool succeeded = false;
            string inUop = inuop.Text;
            try
            {
                uoptomul.Text = "Converting...";
                uoptomul.Enabled = false;
                multouop.Enabled = false;
                singleFileProgressBar.Value = 0;
                singleFileProgressBar.Visible = true;
                label10.Visible = true;

                var progress = new Progress<int>(p => singleFileProgressBar.Value = Math.Min(100, Math.Max(0, p)));

                await Task.Run(() => _conv.FromUop(inUop, outMulPath, outIdxPath, fileType, mapIdx, housingBinPath, progress));
                succeeded = true;
            }
            catch (Exception ex)
            {
                LogConverterError(ex, nameof(ToMul), inUop, outMulPath, fileType);
                MessageBox.Show($"An error occurred.\r\n{ex.Message}");
            }
            finally
            {
                uoptomul.Text = "Convert";
                uoptomul.Enabled = true;
                multouop.Enabled = true;
                singleFileProgressBar.Visible = false;
                label10.Visible = false;
            }

            if (succeeded)
            {
                var written = new System.Collections.Generic.List<string> { Path.GetFileName(outMulPath) };
                if (!string.IsNullOrEmpty(outIdxPath))
                {
                    written.Add(Path.GetFileName(outIdxPath));
                }

                if (!string.IsNullOrEmpty(housingBinPath))
                {
                    written.Add(Path.GetFileName(housingBinPath));
                }

                FileSavedDialog.Show(FindForm(), outfolder.Text,
                    $"Saved: {string.Join(", ", written)}",
                    "Conversion complete");
            }
        }

        private void SelectFolder_Click(object sender, EventArgs e)
        {
            if (FolderDialog.ShowDialog() == DialogResult.OK)
            {
                inputfolder.Text = FolderDialog.SelectedPath;

                if (string.IsNullOrWhiteSpace(packAllHousingBin.Text))
                {
                    string candidate = Path.Combine(FolderDialog.SelectedPath, "housing.bin");
                    if (File.Exists(candidate))
                    {
                        packAllHousingBin.Text = candidate;
                    }
                }
            }
        }

        private void SelectOutputFolder_Click(object sender, EventArgs e)
        {
            if (FolderDialog.ShowDialog() == DialogResult.OK)
            {
                outputfolder.Text = FolderDialog.SelectedPath;
            }
        }

        private void PackAllHousingBinSelect(object sender, EventArgs e)
        {
            FileDialog.FilterIndex = 4;

            if (FileDialog.ShowDialog() == DialogResult.OK)
            {
                packAllHousingBin.Text = FileDialog.FileName;
            }
        }

        private int _total;
        private int _success;
        private int _skippedExists;
        private int _missingInput;

        private void Extract(string inputBase, string outputBase, string inFile, string outFile, string outIdx, FileType type, int typeIndex, IProgress<int> progress, IProgress<string> status, string housingBinFile = "")
        {
            try
            {
                status?.Report(inFile);
                inFile = FixInputPath(inputBase, inFile);

                if (!File.Exists(inFile))
                {
                    ++_missingInput;
                    return;
                }

                outFile = FixOutputPath(inputBase, outputBase, outFile);

                if (File.Exists(outFile))
                {
                    ++_skippedExists;
                    return;
                }

                if (!string.IsNullOrWhiteSpace(housingBinFile))
                {
                    housingBinFile = FixOutputPath(inputBase, outputBase, housingBinFile);
                    if (File.Exists(housingBinFile))
                    {
                        ++_skippedExists;
                        return;
                    }
                }

                outIdx = FixOutputPath(inputBase, outputBase, outIdx);
                ++_total;

                _conv.FromUop(inFile, outFile, outIdx, type, typeIndex, housingBinFile, progress);

                ++_success;
            }
            catch (Exception e)
            {
                LogConverterError(e, nameof(Extract), inFile, outFile, type);
            }
        }

        private void Pack(string inputBase, string outputBase, string inFile, string inIdx, string outFile, FileType type, int typeIndex, CompressionFlag compression, IProgress<int> progress, IProgress<string> status, string housingBinFile = "")
        {
            try
            {
                status?.Report(inFile);
                inFile = FixInputPath(inputBase, inFile);

                if (!File.Exists(inFile))
                {
                    ++_missingInput;
                    return;
                }

                outFile = FixOutputPath(inputBase, outputBase, outFile);

                if (File.Exists(outFile))
                {
                    ++_skippedExists;
                    return;
                }

                inIdx = FixInputPath(inputBase, inIdx);

                if (!string.IsNullOrWhiteSpace(housingBinFile))
                {
                    housingBinFile = FixInputPath(inputBase, housingBinFile);
                }

                ++_total;

                LegacyMulFileConverter.ToUop(inFile, inIdx, outFile, type, typeIndex, compression, housingBinFile ?? string.Empty, progress);

                ++_success;
            }
            catch (Exception e)
            {
                LogConverterError(e, nameof(Pack), inFile, outFile, type);
            }
        }

        private static void LogConverterError(Exception ex, string operation, string input, string output, FileType type)
        {
            ILogger logger = AppLog.For(typeof(UopPackerControl));

            logger.LogError(ex, "UopPacker {Operation} failed (type={FileType}, input={Input}, output={Output})",
                operation, type, input, output);
        }

        private static string FixInputPath(string inputBase, string file)
        {
            return (file == null) ? null : Path.Combine(inputBase, file);
        }

        private static string FixOutputPath(string inputBase, string outputBase, string file)
        {
            if (file == null)
            {
                return null;
            }
            string baseFolder = string.IsNullOrWhiteSpace(outputBase) ? inputBase : outputBase;
            return Path.Combine(baseFolder, file);
        }

        private async void StartFolderButtonClick(object sender, EventArgs e)
        {
            if (inputfolder.Text.Length == 0)
            {
                MessageBox.Show("You must specify the input folder");
                return;
            }

            if (!string.IsNullOrWhiteSpace(outputfolder.Text) && !Directory.Exists(outputfolder.Text))
            {
                var create = MessageBox.Show(
                    $"The output folder does not exist:\r\n{outputfolder.Text}\r\n\r\nCreate it?",
                    "Output folder",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                if (create != DialogResult.Yes)
                {
                    return;
                }
                try
                {
                    Directory.CreateDirectory(outputfolder.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not create output folder.\r\n{ex.Message}");
                    return;
                }
            }

            string inputBase = inputfolder.Text;
            string outputBase = outputfolder.Text;

            if (extract.Checked)
            {
                await RunExtractAllAsync(inputBase, outputBase);
            }
            else if (pack.Checked)
            {
                CompressionFlag gumpCompression = CompressionFlag.None;
                if (packAllGumpCompressionBox.SelectedItem != null)
                {
                    Enum.TryParse(packAllGumpCompressionBox.SelectedItem.ToString(), out gumpCompression);
                }

                string housingBinPath = string.IsNullOrWhiteSpace(packAllHousingBin.Text)
                    ? "housing.bin"
                    : packAllHousingBin.Text;

                if (!string.IsNullOrWhiteSpace(packAllHousingBin.Text))
                {
                    string resolved = Path.IsPathRooted(housingBinPath)
                        ? housingBinPath
                        : Path.Combine(inputBase, housingBinPath);
                    if (!File.Exists(resolved))
                    {
                        MessageBox.Show($"The specified housing.bin does not exist:\r\n{resolved}");
                        return;
                    }
                }

                await RunPackAllAsync(inputBase, outputBase, gumpCompression, housingBinPath);
            }
            else
            {
                MessageBox.Show("You must select an option");
            }
        }

        private async Task RunExtractAllAsync(string inputBase, string outputBase)
        {
            _success = _total = _skippedExists = _missingInput = 0;

            var (overallProgress, statusProgress) = BeginBatchUi();
            try
            {
                const int totalFiles = 4 + 6 * 2;
                int fileIndex = 0;

                IProgress<int> Per() => new ScaledProgress(overallProgress, fileIndex, totalFiles);

                await Task.Run(() =>
                {
                    Extract(inputBase, outputBase, "artLegacyMUL.uop", "art.mul", "artidx.mul", FileType.ArtLegacyMul, 0, Per(), statusProgress); ++fileIndex;
                    Extract(inputBase, outputBase, "gumpartLegacyMUL.uop", "gumpart.mul", "gumpidx.mul", FileType.GumpartLegacyMul, 0, Per(), statusProgress); ++fileIndex;
                    Extract(inputBase, outputBase, "soundLegacyMUL.uop", "sound.mul", "soundidx.mul", FileType.SoundLegacyMul, 0, Per(), statusProgress); ++fileIndex;
                    Extract(inputBase, outputBase, "MultiCollection.uop", "multi-unpacked.mul", "multi-unpacked.idx", FileType.MultiCollection, 0, Per(), statusProgress, "housing.bin"); ++fileIndex;

                    for (int i = 0; i <= 5; ++i)
                    {
                        string map = $"map{i}";
                        Extract(inputBase, outputBase, map + "LegacyMUL.uop", map + ".mul", null, FileType.MapLegacyMul, i, Per(), statusProgress); ++fileIndex;
                        Extract(inputBase, outputBase, map + "xLegacyMUL.uop", map + "x.mul", null, FileType.MapLegacyMul, i, Per(), statusProgress); ++fileIndex;
                    }
                });

                string extractMessage = BuildBatchSummary("extracted");
                statustext.Text = extractMessage;
                string writtenTo = string.IsNullOrWhiteSpace(outputBase) ? inputBase : outputBase;
                FileSavedDialog.Show(FindForm(), writtenTo, extractMessage, "Extraction complete");
            }
            finally
            {
                EndBatchUi();
            }
        }

        private async Task RunPackAllAsync(string inputBase, string outputBase, CompressionFlag gumpCompression, string housingBinPath)
        {
            _success = _total = _skippedExists = _missingInput = 0;

            var (overallProgress, statusProgress) = BeginBatchUi();
            try
            {
                int totalFiles = 4 + 6 * 2;
                int fileIndex = 0;

                IProgress<int> Per() => new ScaledProgress(overallProgress, fileIndex, totalFiles);

                await Task.Run(() =>
                {
                    Pack(inputBase, outputBase, "art.mul", "artidx.mul", "artLegacyMUL.uop", FileType.ArtLegacyMul, 0, CompressionFlag.None, Per(), statusProgress); ++fileIndex;
                    Pack(inputBase, outputBase, "gumpart.mul", "gumpidx.mul", "gumpartLegacyMUL.uop", FileType.GumpartLegacyMul, 0, gumpCompression, Per(), statusProgress); ++fileIndex;
                    Pack(inputBase, outputBase, "sound.mul", "soundidx.mul", "soundLegacyMUL.uop", FileType.SoundLegacyMul, 0, CompressionFlag.None, Per(), statusProgress); ++fileIndex;
                    Pack(inputBase, outputBase, "multi-unpacked.mul", "multi-unpacked.idx", "MultiCollection.uop", FileType.MultiCollection, 0, CompressionFlag.Zlib, Per(), statusProgress, housingBinPath); ++fileIndex;

                    for (int i = 0; i <= 5; ++i)
                    {
                        string map = $"map{i}";
                        Pack(inputBase, outputBase, map + ".mul", null, map + "LegacyMUL.uop", FileType.MapLegacyMul, i, CompressionFlag.None, Per(), statusProgress); ++fileIndex;
                        Pack(inputBase, outputBase, map + "x.mul", null, map + "xLegacyMUL.uop", FileType.MapLegacyMul, i, CompressionFlag.None, Per(), statusProgress); ++fileIndex;
                    }
                });

                string packMessage = BuildBatchSummary("packed");
                statustext.Text = packMessage;
                string writtenTo = string.IsNullOrWhiteSpace(outputBase) ? inputBase : outputBase;
                FileSavedDialog.Show(FindForm(), writtenTo, packMessage, "Pack complete");
            }
            finally
            {
                EndBatchUi();
            }
        }

        private string BuildBatchSummary(string verb)
        {
            var parts = new System.Collections.Generic.List<string> { $"{_success}/{_total} files {verb}" };
            if (_skippedExists > 0)
            {
                parts.Add($"{_skippedExists} skipped (output exists)");
            }
            if (_missingInput > 0)
            {
                parts.Add($"{_missingInput} missing input");
            }
            return $"Done ({string.Join(", ", parts)})";
        }

        private (IProgress<int> overall, IProgress<string> status) BeginBatchUi()
        {
            StartFolderButton.Enabled = false;
            everyFileProgressBar.Value = 0;
            everyFileProgressBar.Visible = true;

            var overall = new Progress<int>(p => everyFileProgressBar.Value = Math.Min(100, Math.Max(0, p)));
            var status = new Progress<string>(s => statustext.Text = s);
            return (overall, status);
        }

        private void EndBatchUi()
        {
            everyFileProgressBar.Visible = false;
            StartFolderButton.Enabled = true;
        }

        private sealed class ScaledProgress : IProgress<int>
        {
            private readonly IProgress<int> _outer;
            private readonly int _fileIndex;
            private readonly int _totalFiles;

            public ScaledProgress(IProgress<int> outer, int fileIndex, int totalFiles)
            {
                _outer = outer;
                _fileIndex = fileIndex;
                _totalFiles = totalFiles;
            }

            public void Report(int innerPct)
            {
                int overall = (_fileIndex * 100 + innerPct) / _totalFiles;
                _outer.Report(overall);
            }
        }
    }
}
