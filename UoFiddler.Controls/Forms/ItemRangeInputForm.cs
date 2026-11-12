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
using System.Windows.Forms;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.Forms
{
    /// <summary>
    /// Asks for an inclusive item id range, in decimal or 0x hex.
    /// </summary>
    public partial class ItemRangeInputForm : Form
    {
        /// <summary>Most items one sheet may hold.</summary>
        public const int MaxRangeLength = 1024;

        private readonly int _maxId;

        public ItemRangeInputForm(int maxId, int initialStart, int initialEnd)
        {
            InitializeComponent();

            Icon = Options.GetFiddlerIcon();
            _maxId = maxId;

            if (initialStart >= 0 && initialEnd >= initialStart)
            {
                RangeTextBox.Text = $"0x{initialStart:X4}-0x{initialEnd:X4}";
                RangeTextBox.SelectAll();
            }
        }

        public int StartId { get; private set; }

        public int EndId { get; private set; }

        private void OnClickOk(object sender, EventArgs e)
        {
            string[] parts = RangeTextBox.Text.Split('-', StringSplitOptions.TrimEntries);
            if (parts.Length != 2 ||
                !Utils.ConvertStringToInt(parts[0], out int start, 0, _maxId) ||
                !Utils.ConvertStringToInt(parts[1], out int end, 0, _maxId))
            {
                ShowError($"Enter the range as start-end, each between 0 and 0x{_maxId:X} (e.g. 100-200 or 0x64-0xC8).");
                return;
            }

            if (start > end)
            {
                ShowError("The start of the range is past its end.");
                return;
            }

            if (end - start + 1 > MaxRangeLength)
            {
                ShowError($"A sheet can hold at most {MaxRangeLength} items.");
                return;
            }

            StartId = start;
            EndId = end;
            DialogResult = DialogResult.OK;
            Close();
        }

        private static void ShowError(string message)
        {
            MessageBox.Show(message, "Export Items to Sheet", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
