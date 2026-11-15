/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Windows.Forms;

namespace UoFiddler.Plugin.MultiEditor.UI
{
    /// <summary>
    /// Asks for the number of copies and the step between them for the "Repeat selection" command.
    /// The caller shows a live preview and updates it from <see cref="ValuesChanged"/>.
    /// </summary>
    internal partial class RepeatSelectionDialog : Form
    {
        public RepeatSelectionDialog()
        {
            InitializeComponent();

            foreach (NumericUpDown box in new[] { _copiesBox, _stepXBox, _stepYBox, _stepZBox })
            {
                box.ValueChanged += (_, _) => ValuesChanged?.Invoke();
                box.KeyUp += OnBoxKeyUp;
            }
        }

        public RepeatSelectionDialog(int stepX)
            : this()
        {
            _stepXBox.Value = Math.Clamp(stepX, (int)_stepXBox.Minimum, (int)_stepXBox.Maximum);
        }

        public int Copies => (int)_copiesBox.Value;

        public int StepX => (int)_stepXBox.Value;

        public int StepY => (int)_stepYBox.Value;

        public int StepZ => (int)_stepZBox.Value;

        /// <summary>Raised when any of the four numbers changes, also while typing or spinning.</summary>
        public event Action ValuesChanged;

        /// <summary>Sets the summary line under the boxes.</summary>
        public void SetSummary(string text)
        {
            _summaryLabel.Text = text;
        }

        private static void OnBoxKeyUp(object sender, KeyEventArgs e)
        {
            // Only text that already reads as the number: setting Value rewrites the text, and a
            // rewrite of "05" or "-" would move the caret under the typing.
            var box = (NumericUpDown)sender;
            if (int.TryParse(box.Text, out int parsed) && parsed.ToString() == box.Text &&
                parsed >= box.Minimum && parsed <= box.Maximum && parsed != box.Value)
            {
                box.Value = parsed;
            }
        }
    }
}
