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
using System.Linq;
using System.Windows.Forms;
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

            uoptype.DataSource = fileTypes;
            multype.DataSource = fileTypes.SkipLast(1).ToArray(); // remove multi collection from ToUOP() conversion (not supported yet)

            mulMapIndex.ReadOnly = uopMapIndex.ReadOnly = true;

            Dock = DockStyle.Fill;
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

        private void OutputUopSelect(object sender, EventArgs e)
        {
            FileDialog.FilterIndex = 2;

            if (FileDialog.ShowDialog() == DialogResult.OK)
            {
                outuop.Text = FileDialog.FileName;
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

            if (inidx.Text.Length == 0)
            {
                MessageBox.Show("You must specify the input idx");
                return;
            }

            if (outuop.Text.Length == 0)
            {
                MessageBox.Show("You must specify the output uop");
                return;
            }

            if (!File.Exists(inmul.Text))
            {
                MessageBox.Show("The input mul does not exists");
                return;
            }

            if (!File.Exists(inidx.Text))
            {
                MessageBox.Show("The input idx does not exists");
                return;
            }

            if (File.Exists(outuop.Text))
            {
                MessageBox.Show("Output file already exists");
                return;
            }

            try
            {
                multouop.Text = "Converting...";
                multouop.Enabled = false;

                LegacyMulFileConverter.ToUop(inmul.Text, inidx.Text, outuop.Text, fileType, (int)mulMapIndex.Value);
            }
            catch
            {
                MessageBox.Show("An error occurred");
            }
            finally
            {
                multouop.Text = "Convert";
                multouop.Enabled = true;
            }
        }

        private void OutMulSelect(object sender, EventArgs e)
        {
            FileDialog.FilterIndex = 1;

            if (FileDialog.ShowDialog() == DialogResult.OK)
            {
                outmul.Text = FileDialog.FileName;
            }
        }

        private void OutputIdxSelect(object sender, EventArgs e)
        {
            FileDialog.FilterIndex = 3;

            if (FileDialog.ShowDialog() == DialogResult.OK)
            {
                outidx.Text = FileDialog.FileName;
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

            if (outmul.Text.Length == 0)
            {
                MessageBox.Show("You must specify the output mul");
                return;
            }

            if (outidx.Text.Length == 0 && fileType != FileType.MapLegacyMul)
            {
                MessageBox.Show("You must specify the output idx");
                return;
            }

            if (inuop.Text.Length == 0)
            {
                MessageBox.Show("You must specify the input uop");
                return;
            }

            if (!File.Exists(inuop.Text))
            {
                MessageBox.Show("The input file does not exists");
                return;
            }

            if (File.Exists(outmul.Text))
            {
                MessageBox.Show("Output mul file already exists");
                return;
            }

            if (File.Exists(outidx.Text) && fileType != FileType.MapLegacyMul)
            {
                MessageBox.Show("Output index file already exists");
                return;
            }

            try
            {
                uoptomul.Text = "Converting...";
                uoptomul.Enabled = false;

                _conv.FromUop(inuop.Text, outmul.Text, outidx.Text, fileType, (int)uopMapIndex.Value);
            }
            catch
            {
                MessageBox.Show("An error occurred");
            }
            finally
            {
                uoptomul.Text = "Convert";
                uoptomul.Enabled = true;
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
                MessageBox.Show($"An error occurred while performing the action.\r\n{e.Message}");
            }
        }

        private void Pack(string inFile, string inIdx, string outFile, FileType type, int typeIndex)
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
                ++_total;

                LegacyMulFileConverter.ToUop(inFile, inIdx, outFile, type, typeIndex);

                ++_success;
            }
            catch (Exception e)
            {
                MessageBox.Show($"An error occurred while performing the action.\r\n{e.Message}");
            }
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

                statustext.Text = $"Done ({_success}/{_total} files extracted)";
            }
            else if (pack.Checked)
            {
                _success = _total = 0;

                Pack("art.mul", "artidx.mul", "artLegacyMUL.uop", FileType.ArtLegacyMul, 0);
                Pack("gumpart.mul", "gumpidx.mul", "gumpartLegacyMUL.uop", FileType.GumpartLegacyMul, 0);
                Pack("sound.mul", "soundidx.mul", "soundLegacyMUL.uop", FileType.SoundLegacyMul, 0);

                for (int i = 0; i <= 5; ++i)
                {
                    string map = $"map{i}";

                    Pack(map + ".mul", null, map + "LegacyMUL.uop", FileType.MapLegacyMul, i);
                    Pack(map + "x.mul", null, map + "xLegacyMUL.uop", FileType.MapLegacyMul, i);
                }

                statustext.Text = $"Done ({_success}/{_total} files packed)";
            }
            else
            {
                MessageBox.Show("You must select an option");
            }
        }
    }
}
