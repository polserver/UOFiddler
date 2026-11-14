/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System.Windows.Forms;

namespace UoFiddler.Plugin.MultiEditor.UI
{
    /// <summary>Asks for one line of text. Shown through <see cref="Prompt.Text"/>.</summary>
    internal partial class TextPromptForm : Form
    {
        public TextPromptForm()
        {
            InitializeComponent();
        }

        public TextPromptForm(string title, string label, string value)
            : this()
        {
            Text = title;
            _label.Text = label;
            _valueBox.Text = value;
        }

        /// <summary>The text as typed.</summary>
        public string Value => _valueBox.Text;
    }
}
