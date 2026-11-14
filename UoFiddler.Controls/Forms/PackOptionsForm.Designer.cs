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

namespace UoFiddler.Controls.Forms
{
    partial class PackOptionsForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            DirectionsLabel = new System.Windows.Forms.Label();
            DirectionsListBox = new System.Windows.Forms.CheckedListBox();
            MaxWidthLabel = new System.Windows.Forms.Label();
            MaxWidthUpDown = new System.Windows.Forms.NumericUpDown();
            Preset1024Button = new System.Windows.Forms.Button();
            Preset2048Button = new System.Windows.Forms.Button();
            Preset4096Button = new System.Windows.Forms.Button();
            SpacingLabel = new System.Windows.Forms.Label();
            SpacingUpDown = new System.Windows.Forms.NumericUpDown();
            OneRowPerDirectionCheckBox = new System.Windows.Forms.CheckBox();
            ExportAllActionsCheckBox = new System.Windows.Forms.CheckBox();
            ScaleLabel = new System.Windows.Forms.Label();
            ScaleUpDown = new System.Windows.Forms.NumericUpDown();
            FormatLabel = new System.Windows.Forms.Label();
            SheetFormatRadioButton = new System.Windows.Forms.RadioButton();
            FramesXmlFormatRadioButton = new System.Windows.Forms.RadioButton();
            OkButton = new System.Windows.Forms.Button();
            CancelDialogButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)MaxWidthUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)SpacingUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)ScaleUpDown).BeginInit();
            SuspendLayout();
            //
            // DirectionsLabel
            //
            DirectionsLabel.AutoSize = true;
            DirectionsLabel.Location = new System.Drawing.Point(12, 12);
            DirectionsLabel.Name = "DirectionsLabel";
            DirectionsLabel.Size = new System.Drawing.Size(63, 15);
            DirectionsLabel.Text = "Directions:";
            //
            // DirectionsListBox
            //
            DirectionsListBox.CheckOnClick = true;
            DirectionsListBox.IntegralHeight = false;
            DirectionsListBox.Location = new System.Drawing.Point(12, 30);
            DirectionsListBox.Name = "DirectionsListBox";
            DirectionsListBox.Size = new System.Drawing.Size(190, 140);
            DirectionsListBox.TabIndex = 0;
            //
            // MaxWidthLabel
            //
            MaxWidthLabel.AutoSize = true;
            MaxWidthLabel.Location = new System.Drawing.Point(212, 12);
            MaxWidthLabel.Name = "MaxWidthLabel";
            MaxWidthLabel.Size = new System.Drawing.Size(98, 15);
            MaxWidthLabel.Text = "Max sheet width:";
            //
            // MaxWidthUpDown
            //
            MaxWidthUpDown.Increment = new decimal(new int[] { 64, 0, 0, 0 });
            MaxWidthUpDown.Location = new System.Drawing.Point(212, 30);
            MaxWidthUpDown.Maximum = new decimal(new int[] { 8192, 0, 0, 0 });
            MaxWidthUpDown.Minimum = new decimal(new int[] { 256, 0, 0, 0 });
            MaxWidthUpDown.Name = "MaxWidthUpDown";
            MaxWidthUpDown.Size = new System.Drawing.Size(162, 23);
            MaxWidthUpDown.TabIndex = 1;
            MaxWidthUpDown.ThousandsSeparator = true;
            MaxWidthUpDown.Value = new decimal(new int[] { 2048, 0, 0, 0 });
            //
            // Preset1024Button
            //
            Preset1024Button.Location = new System.Drawing.Point(212, 59);
            Preset1024Button.Name = "Preset1024Button";
            Preset1024Button.Size = new System.Drawing.Size(50, 25);
            Preset1024Button.TabIndex = 2;
            Preset1024Button.Text = "1024";
            Preset1024Button.UseVisualStyleBackColor = true;
            Preset1024Button.Click += OnClickPreset;
            //
            // Preset2048Button
            //
            Preset2048Button.Location = new System.Drawing.Point(268, 59);
            Preset2048Button.Name = "Preset2048Button";
            Preset2048Button.Size = new System.Drawing.Size(50, 25);
            Preset2048Button.TabIndex = 3;
            Preset2048Button.Text = "2048";
            Preset2048Button.UseVisualStyleBackColor = true;
            Preset2048Button.Click += OnClickPreset;
            //
            // Preset4096Button
            //
            Preset4096Button.Location = new System.Drawing.Point(324, 59);
            Preset4096Button.Name = "Preset4096Button";
            Preset4096Button.Size = new System.Drawing.Size(50, 25);
            Preset4096Button.TabIndex = 4;
            Preset4096Button.Text = "4096";
            Preset4096Button.UseVisualStyleBackColor = true;
            Preset4096Button.Click += OnClickPreset;
            //
            // SpacingLabel
            //
            SpacingLabel.AutoSize = true;
            SpacingLabel.Location = new System.Drawing.Point(212, 92);
            SpacingLabel.Name = "SpacingLabel";
            SpacingLabel.Size = new System.Drawing.Size(52, 15);
            SpacingLabel.Text = "Spacing:";
            //
            // SpacingUpDown
            //
            SpacingUpDown.Location = new System.Drawing.Point(212, 110);
            SpacingUpDown.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
            SpacingUpDown.Name = "SpacingUpDown";
            SpacingUpDown.Size = new System.Drawing.Size(80, 23);
            SpacingUpDown.TabIndex = 5;
            //
            // OneRowPerDirectionCheckBox
            //
            OneRowPerDirectionCheckBox.AutoSize = true;
            OneRowPerDirectionCheckBox.Location = new System.Drawing.Point(12, 182);
            OneRowPerDirectionCheckBox.Name = "OneRowPerDirectionCheckBox";
            OneRowPerDirectionCheckBox.Size = new System.Drawing.Size(146, 19);
            OneRowPerDirectionCheckBox.TabIndex = 6;
            OneRowPerDirectionCheckBox.Text = "One row per direction";
            OneRowPerDirectionCheckBox.UseVisualStyleBackColor = true;
            OneRowPerDirectionCheckBox.CheckedChanged += OnCheckedChangedOneRowPerDirection;
            //
            // ExportAllActionsCheckBox
            //
            ExportAllActionsCheckBox.AutoSize = true;
            ExportAllActionsCheckBox.Location = new System.Drawing.Point(12, 207);
            ExportAllActionsCheckBox.Name = "ExportAllActionsCheckBox";
            ExportAllActionsCheckBox.Size = new System.Drawing.Size(209, 19);
            ExportAllActionsCheckBox.TabIndex = 7;
            ExportAllActionsCheckBox.Text = "Export all actions of the body";
            ExportAllActionsCheckBox.UseVisualStyleBackColor = true;
            // 
            // ScaleLabel
            // 
            ScaleLabel.AutoSize = true;
            ScaleLabel.Location = new System.Drawing.Point(212, 142);
            ScaleLabel.Name = "ScaleLabel";
            ScaleLabel.Size = new System.Drawing.Size(88, 15);
            ScaleLabel.Text = "Frame size (%):";
            // 
            // ScaleUpDown
            // 
            ScaleUpDown.Increment = new decimal(new int[] { 10, 0, 0, 0 });
            ScaleUpDown.Location = new System.Drawing.Point(212, 160);
            ScaleUpDown.Maximum = new decimal(new int[] { 400, 0, 0, 0 });
            ScaleUpDown.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            ScaleUpDown.Name = "ScaleUpDown";
            ScaleUpDown.Size = new System.Drawing.Size(80, 23);
            ScaleUpDown.TabIndex = 8;
            ScaleUpDown.Value = new decimal(new int[] { 100, 0, 0, 0 });
            // 
            // FormatLabel
            // 
            FormatLabel.AutoSize = true;
            FormatLabel.Location = new System.Drawing.Point(12, 236);
            FormatLabel.Name = "FormatLabel";
            FormatLabel.Size = new System.Drawing.Size(48, 15);
            FormatLabel.Text = "Format:";
            // 
            // SheetFormatRadioButton
            // 
            SheetFormatRadioButton.AutoSize = true;
            SheetFormatRadioButton.Checked = true;
            SheetFormatRadioButton.Location = new System.Drawing.Point(12, 254);
            SheetFormatRadioButton.Name = "SheetFormatRadioButton";
            SheetFormatRadioButton.Size = new System.Drawing.Size(170, 19);
            SheetFormatRadioButton.TabIndex = 9;
            SheetFormatRadioButton.TabStop = true;
            SheetFormatRadioButton.Text = "Sprite sheet (.png + .json)";
            SheetFormatRadioButton.UseVisualStyleBackColor = true;
            SheetFormatRadioButton.CheckedChanged += OnCheckedChangedFormat;
            // 
            // FramesXmlFormatRadioButton
            // 
            FramesXmlFormatRadioButton.AutoSize = true;
            FramesXmlFormatRadioButton.Location = new System.Drawing.Point(12, 279);
            FramesXmlFormatRadioButton.Name = "FramesXmlFormatRadioButton";
            FramesXmlFormatRadioButton.Size = new System.Drawing.Size(232, 19);
            FramesXmlFormatRadioButton.TabIndex = 10;
            FramesXmlFormatRadioButton.Text = "One image per frame (.png + .xml)";
            FramesXmlFormatRadioButton.UseVisualStyleBackColor = true;
            //
            // OkButton
            //
            OkButton.Location = new System.Drawing.Point(218, 312);
            OkButton.Name = "OkButton";
            OkButton.Size = new System.Drawing.Size(75, 25);
            OkButton.TabIndex = 11;
            OkButton.Text = "OK";
            OkButton.UseVisualStyleBackColor = true;
            OkButton.Click += OnClickOk;
            //
            // CancelDialogButton
            //
            CancelDialogButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            CancelDialogButton.Location = new System.Drawing.Point(299, 312);
            CancelDialogButton.Name = "CancelDialogButton";
            CancelDialogButton.Size = new System.Drawing.Size(75, 25);
            CancelDialogButton.TabIndex = 12;
            CancelDialogButton.Text = "Cancel";
            CancelDialogButton.UseVisualStyleBackColor = true;
            //
            // PackOptionsForm
            //
            AcceptButton = OkButton;
            CancelButton = CancelDialogButton;
            ClientSize = new System.Drawing.Size(386, 349);
            Controls.Add(DirectionsLabel);
            Controls.Add(DirectionsListBox);
            Controls.Add(MaxWidthLabel);
            Controls.Add(MaxWidthUpDown);
            Controls.Add(Preset1024Button);
            Controls.Add(Preset2048Button);
            Controls.Add(Preset4096Button);
            Controls.Add(SpacingLabel);
            Controls.Add(SpacingUpDown);
            Controls.Add(OneRowPerDirectionCheckBox);
            Controls.Add(ExportAllActionsCheckBox);
            Controls.Add(ScaleLabel);
            Controls.Add(ScaleUpDown);
            Controls.Add(FormatLabel);
            Controls.Add(SheetFormatRadioButton);
            Controls.Add(FramesXmlFormatRadioButton);
            Controls.Add(OkButton);
            Controls.Add(CancelDialogButton);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "PackOptionsForm";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Pack Frames";
            ((System.ComponentModel.ISupportInitialize)MaxWidthUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)SpacingUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)ScaleUpDown).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label DirectionsLabel;
        private System.Windows.Forms.CheckedListBox DirectionsListBox;
        private System.Windows.Forms.Label MaxWidthLabel;
        private System.Windows.Forms.NumericUpDown MaxWidthUpDown;
        private System.Windows.Forms.Button Preset1024Button;
        private System.Windows.Forms.Button Preset2048Button;
        private System.Windows.Forms.Button Preset4096Button;
        private System.Windows.Forms.Label SpacingLabel;
        private System.Windows.Forms.NumericUpDown SpacingUpDown;
        private System.Windows.Forms.CheckBox OneRowPerDirectionCheckBox;
        private System.Windows.Forms.CheckBox ExportAllActionsCheckBox;
        private System.Windows.Forms.Label ScaleLabel;
        private System.Windows.Forms.NumericUpDown ScaleUpDown;
        private System.Windows.Forms.Label FormatLabel;
        private System.Windows.Forms.RadioButton SheetFormatRadioButton;
        private System.Windows.Forms.RadioButton FramesXmlFormatRadioButton;
        private System.Windows.Forms.Button OkButton;
        private System.Windows.Forms.Button CancelDialogButton;
    }
}
