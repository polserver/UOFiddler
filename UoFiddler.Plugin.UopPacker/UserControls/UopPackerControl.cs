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

            multype.DataSource = uoptype.DataSource = Enum.GetValues(typeof(FileType));
            mulmaptype.ReadOnly = uopmaptype.ReadOnly = true;

            Dock = DockStyle.Fill;
        }

        public UopPackerControl(string version) : this()
        {
            versionlabel.Text = version;
        }

        private void Inmulselect(object sender, EventArgs e)
        {
            if (selectfile.ShowDialog() == DialogResult.OK)
            {
                inmul.Text = selectfile.FileName;
            }
        }

        private void Inidxselect(object sender, EventArgs e)
        {
            if (selectfile.ShowDialog() == DialogResult.OK)
            {
                inidx.Text = selectfile.FileName;
            }
        }

        private void Outuopselect(object sender, EventArgs e)
        {
            if (selectfile.ShowDialog() == DialogResult.OK)
            {
                outuop.Text = selectfile.FileName;
            }
        }

        private void Touop(object sender, EventArgs e)
        {
            if (inmul.Text?.Length == 0 || inmul.Text == null)
            {
                MessageBox.Show("You must specify the input mul");
                return;
            }

            if (inidx.Text?.Length == 0 || inidx.Text == null)
            {
                MessageBox.Show("You must specify the input idx");
                return;
            }

            if (outuop.Text?.Length == 0 || outuop.Text == null)
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
                MessageBox.Show("output file already exists");
                return;
            }

            try
            {
                multouop.Text = "Converting...";
                multouop.Enabled = false;

                Enum.TryParse(multype.SelectedValue.ToString(), out FileType status);
                LegacyMulFileConverter.ToUOP(inmul.Text, inidx.Text, outuop.Text, status, (int)mulmaptype.Value);
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

        private void Outmulselect(object sender, EventArgs e)
        {
            if (selectfile.ShowDialog() == DialogResult.OK)
            {
                outmul.Text = selectfile.FileName;
            }
        }

        private void Outidxselect(object sender, EventArgs e)
        {
            if (selectfile.ShowDialog() == DialogResult.OK)
            {
                outidx.Text = selectfile.FileName;
            }
        }

        private void Inuopselect(object sender, EventArgs e)
        {
            if (selectfile.ShowDialog() == DialogResult.OK)
            {
                inuop.Text = selectfile.FileName;
            }
        }

        private void Tomul(object sender, EventArgs e)
        {
            if (outmul.Text?.Length == 0 || outmul.Text == null)
            {
                MessageBox.Show("You must specify the output mul");
                return;
            }

            if (outidx.Text?.Length == 0 || outidx.Text == null)
            {
                MessageBox.Show("You must specify the output idx");
                return;
            }

            if (inuop.Text?.Length == 0 || inuop.Text == null)
            {
                MessageBox.Show("You must specify the input uop");
                return;
            }

            if (!File.Exists(inuop.Text))
            {
                MessageBox.Show("The input file does not exists");
                return;
            }

            if (File.Exists(inmul.Text))
            {
                MessageBox.Show("input mul already exists");
                return;
            }

            if (File.Exists(inidx.Text))
            {
                MessageBox.Show("inidx file already exists");
                return;
            }

            try
            {
                uoptomul.Text = "Converting...";
                uoptomul.Enabled = false;

                Enum.TryParse(uoptype.SelectedValue.ToString(), out FileType status);
                _conv.FromUOP(inuop.Text, outmul.Text, outidx.Text, status, (int)uopmaptype.Value);
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

        private void Selectfolderbtn_Click(object sender, EventArgs e)
        {
            if (selectfolder.ShowDialog() == DialogResult.OK)
            {
                inputfolder.Text = selectfolder.SelectedPath;
            }
        }

        private int _total;
        private int _success;

        private void Extract(string inFile, string outFile, string outIdx, FileType type, int typeIndex)
        {
            try
            {
                statustext.Text = inFile;
                Refresh();
                inFile = FixPath(inFile);

                if (!File.Exists(inFile))
                {
                    return;
                }

                outFile = FixPath(outFile);

                if (File.Exists(outFile))
                {
                    return;
                }

                outIdx = FixPath(outIdx);
                ++_total;

                _conv.FromUOP(inFile, outFile, outIdx, type, typeIndex);

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
                    return;
                }

                outFile = FixPath(outFile);

                if (File.Exists(outFile))
                {
                    return;
                }

                inIdx = FixPath(inIdx);
                ++_total;

                LegacyMulFileConverter.ToUOP(inFile, inIdx, outFile, type, typeIndex);

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

        private void Startfolder_Click(object sender, EventArgs e)
        {
            if (inputfolder.Text?.Length == 0 || inputfolder.Text == null)
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
