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
using System.IO;
using System.Windows.Forms;
using Serilog;
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

            RefreshMulTypeUi();
            RefreshUopTypeUi();

            Dock = DockStyle.Fill;
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

        private void ToUop(object sender, EventArgs e)
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

            CompressionFlag selectedCompressionMethod = Enum.Parse<CompressionFlag>(compressionBox.SelectedItem.ToString());

            bool succeeded = false;
            try
            {
                multouop.Text = "Converting...";
                multouop.Enabled = false;

                LegacyMulFileConverter.ToUop(inmul.Text, inidx.Text, outUopPath, fileType, (int)mulMapIndex.Value, selectedCompressionMethod, housingBin);
                succeeded = true;
            }
            catch (Exception ex)
            {
                LogConverterError(ex, nameof(ToUop), inmul.Text, outUopPath, fileType);
                MessageBox.Show($"An error occurred.\r\n{ex.Message}");
            }
            finally
            {
                multouop.Text = "Convert";
                multouop.Enabled = true;
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

        private void ToMul(object sender, EventArgs e)
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
            if (File.Exists(outMulPath)) conflicts.Add(mulName);
            if (outIdxPath != null && File.Exists(outIdxPath)) conflicts.Add(idxName);
            if (!string.IsNullOrEmpty(housingBinPath) && File.Exists(housingBinPath)) conflicts.Add("housing.bin");

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
            try
            {
                uoptomul.Text = "Converting...";
                uoptomul.Enabled = false;

                _conv.FromUop(inuop.Text, outMulPath, outIdxPath, fileType, mapIdx, housingBinPath);
                succeeded = true;
            }
            catch (Exception ex)
            {
                LogConverterError(ex, nameof(ToMul), inuop.Text, outMulPath, fileType);
                MessageBox.Show($"An error occurred.\r\n{ex.Message}");
            }
            finally
            {
                uoptomul.Text = "Convert";
                uoptomul.Enabled = true;
            }

            if (succeeded)
            {
                var written = new System.Collections.Generic.List<string> { Path.GetFileName(outMulPath) };
                if (!string.IsNullOrEmpty(outIdxPath)) written.Add(Path.GetFileName(outIdxPath));
                if (!string.IsNullOrEmpty(housingBinPath)) written.Add(Path.GetFileName(housingBinPath));

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
            }
        }

        private int _total;
        private int _success;

        private void Extract(string inFile, string outFile, string outIdx, FileType type, int typeIndex, string housingBinFile = "")
        {
            try
            {
                statustext.Text = inFile;
                Refresh();
                inFile = FixPath(inFile);

                if (!File.Exists(inFile))
                {
                    MessageBox.Show($"Input file {inFile} doesn't exist");
                    return;
                }

                outFile = FixPath(outFile);

                if (File.Exists(outFile))
                {
                    MessageBox.Show($"Output file {outFile} already exists");
                    return;
                }

                if (!string.IsNullOrWhiteSpace(housingBinFile))
                {
                    housingBinFile = FixPath(housingBinFile);
                    if (File.Exists(housingBinFile))
                    {
                        MessageBox.Show($"Output file {housingBinFile} already exists");
                        return;
                    }
                }

                outIdx = FixPath(outIdx);
                ++_total;

                _conv.FromUop(inFile, outFile, outIdx, type, typeIndex, housingBinFile);

                ++_success;
            }
            catch (Exception e)
            {
                LogConverterError(e, nameof(Extract), inFile, outFile, type);
                MessageBox.Show($"An error occurred while performing the action.\r\n{e.Message}");
            }
        }

        private void Pack(string inFile, string inIdx, string outFile, FileType type, int typeIndex, string housingBinFile = "")
        {
            try
            {
                statustext.Text = inFile;
                Refresh();
                inFile = FixPath(inFile);

                if (!File.Exists(inFile))
                {
                    MessageBox.Show($"Input file {inFile} doesn't exist");
                    return;
                }

                outFile = FixPath(outFile);

                if (File.Exists(outFile))
                {
                    MessageBox.Show($"Output file {outFile} already exists");
                    return;
                }

                inIdx = FixPath(inIdx);

                if (!string.IsNullOrWhiteSpace(housingBinFile))
                {
                    housingBinFile = FixPath(housingBinFile);
                }

                ++_total;
                CompressionFlag selectedCompressionMethod = Enum.Parse<CompressionFlag>(compressionBox.SelectedItem.ToString());

                LegacyMulFileConverter.ToUop(inFile, inIdx, outFile, type, typeIndex, selectedCompressionMethod, housingBinFile ?? string.Empty);

                ++_success;
            }
            catch (Exception e)
            {
                LogConverterError(e, nameof(Pack), inFile, outFile, type);
                MessageBox.Show($"An error occurred while performing the action.\r\n{e.Message}");
            }
        }

        private static void LogConverterError(Exception ex, string operation, string input, string output, FileType type)
        {
            ILogger logger = Options.Logger;

            logger?.Error(ex, "UopPacker {Operation} failed (type={FileType}, input={Input}, output={Output})",
                operation, type, input, output);
        }

        private string FixPath(string file)
        {
            return (file == null) ? null : Path.Combine(inputfolder.Text, file);
        }

        private void StartFolderButtonClick(object sender, EventArgs e)
        {
            if (inputfolder.Text.Length == 0)
            {
                MessageBox.Show("You must specify the input folder");
                return;
            }

            if (extract.Checked)
            {
                _success = _total = 0;

                Extract("artLegacyMUL.uop", "art.mul", "artidx.mul", FileType.ArtLegacyMul, 0);
                Extract("gumpartLegacyMUL.uop", "gumpart.mul", "gumpidx.mul", FileType.GumpartLegacyMul, 0);
                Extract("soundLegacyMUL.uop", "sound.mul", "soundidx.mul", FileType.SoundLegacyMul, 0);
                Extract("MultiCollection.uop", "multi-unpacked.mul", "multi-unpacked.idx", FileType.MultiCollection, 0, "housing.bin");

                for (int i = 0; i <= 5; ++i)
                {
                    string map = $"map{i}";

                    Extract(map + "LegacyMUL.uop", map + ".mul", null, FileType.MapLegacyMul, i);
                    Extract(map + "xLegacyMUL.uop", map + "x.mul", null, FileType.MapLegacyMul, i);
                }

                string extractMessage = $"Done ({_success}/{_total} files extracted)";
                statustext.Text = extractMessage;
                FileSavedDialog.Show(FindForm(), inputfolder.Text, extractMessage, "Extraction complete");
            }
            else if (pack.Checked)
            {
                _success = _total = 0;

                Pack("art.mul", "artidx.mul", "artLegacyMUL.uop", FileType.ArtLegacyMul, 0);
                Pack("gumpart.mul", "gumpidx.mul", "gumpartLegacyMUL.uop", FileType.GumpartLegacyMul, 0);
                Pack("sound.mul", "soundidx.mul", "soundLegacyMUL.uop", FileType.SoundLegacyMul, 0);
                Pack("multi-unpacked.mul", "multi-unpacked.idx", "MultiCollection.uop", FileType.MultiCollection, 0, "housing.bin");

                for (int i = 0; i <= 5; ++i)
                {
                    string map = $"map{i}";

                    Pack(map + ".mul", null, map + "LegacyMUL.uop", FileType.MapLegacyMul, i);
                    Pack(map + "x.mul", null, map + "xLegacyMUL.uop", FileType.MapLegacyMul, i);
                }

                string packMessage = $"Done ({_success}/{_total} files packed)";
                statustext.Text = packMessage;
                FileSavedDialog.Show(FindForm(), inputfolder.Text, packMessage, "Pack complete");
            }
            else
            {
                MessageBox.Show("You must select an option");
            }
        }
    }
}
