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
using Ultima;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.Forms
{
    /// <summary>
    /// Picks the body and action to copy an action to. Closes with OK only for a valid target.
    /// </summary>
    public partial class AnimationCopyActionForm : Form
    {
        private readonly int _fileType;
        private readonly int _sourceBody;
        private readonly int _sourceAction;

        public AnimationCopyActionForm(int fileType, int sourceBody, int sourceAction)
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();

            _fileType = fileType;
            _sourceBody = sourceBody;
            _sourceAction = sourceAction;

            SourceLabel.Text = $"Copy body {sourceBody}, action {sourceAction} (all directions) to:";
            BodyNumericUpDown.Maximum = Math.Max(0, Animations.GetAnimCount(fileType) - 1);
            BodyNumericUpDown.Value = Math.Min(sourceBody, BodyNumericUpDown.Maximum);
            ActionNumericUpDown.Value = sourceAction;
        }

        public int TargetBody => (int)BodyNumericUpDown.Value;

        public int TargetAction => (int)ActionNumericUpDown.Value;

        private void OnClickCopy(object sender, EventArgs e)
        {
            int body = TargetBody;
            int action = TargetAction;

            if (body == _sourceBody && action == _sourceAction)
            {
                MessageBox.Show("Choose a different body or action than the source.", "Copy Action",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!AnimationEdit.IsActionInRange(_fileType, body, action))
            {
                MessageBox.Show($"Body {body} has no action {action} in this anim file.", "Copy Action",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (AnimationEdit.IsActionDefined(_fileType, body, action) &&
                MessageBox.Show($"Action {action} of body {body} already has frames. Replace them?", "Copy Action",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
            {
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void OnClickCancel(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
