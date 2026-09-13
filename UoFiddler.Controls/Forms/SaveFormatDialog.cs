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
using System.Windows.Forms;
using Ultima.Uop;

namespace UoFiddler.Controls.Forms
{
    /// <summary>
    /// Asks which container a save should write, for the file types the client ships in either.
    /// Shown only when the save format option is set to ask every time.
    /// </summary>
    public sealed partial class SaveFormatDialog : Form
    {
        private readonly FileType _type;
        private readonly int _mapIndex;
        private readonly string _outputDirectory;

        public SaveFormatDialog(FileType type, string outputDirectory, ContainerFormat suggested, int mapIndex = 0)
        {
            _type = type;
            _mapIndex = mapIndex;
            _outputDirectory = outputDirectory ?? string.Empty;

            InitializeComponent();

            iconPictureBox.Image = SystemIcons.Question.ToBitmap();

            var (mulName, idxName, uopName) = UopFileNames.For(type, mapIndex);
            bool clientUsesUop = ClientFileSaver.ClientUsesUop(type, mapIndex);

            string mulItem = idxName == null ? mulName : $"{mulName} + {idxName}";

            comboBoxFormat.Items.Add(Describe(mulItem, !clientUsesUop));
            comboBoxFormat.Items.Add(Describe(uopName, clientUsesUop));
            comboBoxFormat.SelectedIndex = suggested == ContainerFormat.Uop ? 1 : 0;

            pathLabel.Text = _outputDirectory;

            UpdateWillCreate();
        }

        /// <summary>The container the user picked. Only meaningful on <see cref="DialogResult.OK"/>.</summary>
        public ContainerFormat SelectedFormat =>
            comboBoxFormat.SelectedIndex == 1 ? ContainerFormat.Uop : ContainerFormat.Mul;

        private static string Describe(string files, bool isWhatTheClientUses)
        {
            return isWhatTheClientUses ? $"{files}  (the same format as this client)" : files;
        }

        private void OnFormatChanged(object sender, EventArgs e)
        {
            UpdateWillCreate();
        }

        private void UpdateWillCreate()
        {
            var (mulName, idxName, uopName) = UopFileNames.For(_type, _mapIndex);

            string files = SelectedFormat == ContainerFormat.Uop
                ? uopName
                : idxName == null ? mulName : $"{mulName}, {idxName}";

            willCreateLabel.Text = "Will create: " + files;
        }
    }
}