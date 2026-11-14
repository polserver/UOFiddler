/***************************************************************************
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

namespace UoFiddler.Plugin.MultiEditor.UI
{
    /// <summary>
    /// Asks for whole numbers, one labelled box per field. Shown through <see cref="Prompt.Numbers"/>.
    /// The rows depend on the question, so they are added here; the form and its buttons are
    /// laid out for no rows and move down by one row height per field.
    /// </summary>
    internal partial class NumbersPromptForm : Form
    {
        private const int _rowHeight = 32;

        private NumericUpDown[] _boxes = Array.Empty<NumericUpDown>();

        public NumbersPromptForm()
        {
            InitializeComponent();
        }

        public NumbersPromptForm(string title, (string Label, int Value, int Min, int Max)[] fields)
            : this()
        {
            Text = title;

            SuspendLayout();
            int rows = fields.Length;
            ClientSize = new Size(ClientSize.Width, ClientSize.Height + (rows * _rowHeight));
            _boxes = new NumericUpDown[rows];
            for (int i = 0; i < rows; i++)
            {
                Controls.Add(new Label { Text = fields[i].Label, Left = 12, Top = 16 + (i * _rowHeight), Width = 120, TabIndex = i * 2 });
                _boxes[i] = new NumericUpDown
                {
                    Left = 140,
                    Top = 13 + (i * _rowHeight),
                    Width = 100,
                    Minimum = fields[i].Min,
                    Maximum = fields[i].Max,
                    Value = Math.Clamp(fields[i].Value, fields[i].Min, fields[i].Max),
                    TabIndex = (i * 2) + 1
                };
                Controls.Add(_boxes[i]);
            }

            _okButton.Top += rows * _rowHeight;
            _cancelButton.Top += rows * _rowHeight;
            _okButton.TabIndex = rows * 2;
            _cancelButton.TabIndex = (rows * 2) + 1;
            ResumeLayout(false);
        }

        /// <summary>The numbers as entered, in field order.</summary>
        public int[] Values => Array.ConvertAll(_boxes, b => (int)b.Value);
    }
}
