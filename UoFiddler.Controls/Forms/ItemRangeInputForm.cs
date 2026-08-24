using System;
using System.Windows.Forms;

namespace UoFiddler.Controls.Forms
{
    public partial class ItemRangeInputForm : Form
    {
        private TextBox _rangeTextBox;
        private Button _okButton;
        private Button _cancelButton;
        private Label _instructionLabel;

        public int StartIndex { get; private set; }
        public int EndIndex { get; private set; }

        public ItemRangeInputForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            _rangeTextBox = new TextBox();
            _okButton = new Button();
            _cancelButton = new Button();
            _instructionLabel = new Label();
            SuspendLayout();
            // 
            // _rangeTextBox
            // 
            _rangeTextBox.Location = new System.Drawing.Point(12, 29);
            _rangeTextBox.Name = "_rangeTextBox";
            _rangeTextBox.Size = new System.Drawing.Size(260, 31);
            _rangeTextBox.TabIndex = 0;
            // 
            // _okButton
            // 
            _okButton.DialogResult = DialogResult.OK;
            _okButton.Location = new System.Drawing.Point(116, 66);
            _okButton.Name = "_okButton";
            _okButton.Size = new System.Drawing.Size(75, 40);
            _okButton.TabIndex = 1;
            _okButton.Text = "OK";
            _okButton.UseVisualStyleBackColor = true;
            _okButton.Click += OkButton_Click;
            // 
            // _cancelButton
            // 
            _cancelButton.DialogResult = DialogResult.Cancel;
            _cancelButton.Location = new System.Drawing.Point(197, 66);
            _cancelButton.Name = "_cancelButton";
            _cancelButton.Size = new System.Drawing.Size(75, 40);
            _cancelButton.TabIndex = 2;
            _cancelButton.Text = "Cancel";
            _cancelButton.UseVisualStyleBackColor = true;
            // 
            // _instructionLabel
            // 
            _instructionLabel.AutoSize = true;
            _instructionLabel.Location = new System.Drawing.Point(12, 9);
            _instructionLabel.Name = "_instructionLabel";
            _instructionLabel.Size = new System.Drawing.Size(230, 25);
            _instructionLabel.TabIndex = 3;
            _instructionLabel.Text = "Enter Range (e.g., 100-200):";
            // 
            // ItemRangeInputForm
            // 
            AcceptButton = _okButton;
            CancelButton = _cancelButton;
            ClientSize = new System.Drawing.Size(307, 117);
            Controls.Add(_instructionLabel);
            Controls.Add(_cancelButton);
            Controls.Add(_okButton);
            Controls.Add(_rangeTextBox);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ItemRangeInputForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Export Items Range";
            ResumeLayout(false);
            PerformLayout();

        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            string input = _rangeTextBox.Text.Trim();
            string[] parts = input.Split('-');

            if (parts.Length != 2)
            {
                MessageBox.Show("Invalid format. Please use 'Start-End' (e.g., 100-200).", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.None;
                return;
            }

            if (int.TryParse(parts[0], out int start) && int.TryParse(parts[1], out int end))
            {
                if (start > end)
                {
                    MessageBox.Show("Start index cannot be greater than end index.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.DialogResult = DialogResult.None;
                    return;
                }

                if (end - start + 1 > 100)
                {
                    MessageBox.Show("Range cannot exceed 100 items.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.DialogResult = DialogResult.None;
                    return;
                }

                StartIndex = start;
                EndIndex = end;
            }
            else
            {
                MessageBox.Show("Invalid numbers.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.None;
            }
        }
    }
}
