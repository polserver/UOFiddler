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
    partial class AnimationCopyActionForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            SourceLabel = new System.Windows.Forms.Label();
            BodyLabel = new System.Windows.Forms.Label();
            BodyNumericUpDown = new System.Windows.Forms.NumericUpDown();
            ActionLabel = new System.Windows.Forms.Label();
            ActionNumericUpDown = new System.Windows.Forms.NumericUpDown();
            CopyButton = new System.Windows.Forms.Button();
            CloseButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)BodyNumericUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)ActionNumericUpDown).BeginInit();
            SuspendLayout();
            //
            // SourceLabel
            //
            SourceLabel.AutoSize = true;
            SourceLabel.Location = new System.Drawing.Point(14, 10);
            SourceLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            SourceLabel.Name = "SourceLabel";
            SourceLabel.Size = new System.Drawing.Size(73, 15);
            SourceLabel.TabIndex = 0;
            SourceLabel.Text = "Copy action";
            //
            // BodyLabel
            //
            BodyLabel.AutoSize = true;
            BodyLabel.Location = new System.Drawing.Point(14, 44);
            BodyLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            BodyLabel.Name = "BodyLabel";
            BodyLabel.Size = new System.Drawing.Size(34, 15);
            BodyLabel.TabIndex = 1;
            BodyLabel.Text = "Body";
            //
            // BodyNumericUpDown
            //
            BodyNumericUpDown.Location = new System.Drawing.Point(74, 42);
            BodyNumericUpDown.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            BodyNumericUpDown.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            BodyNumericUpDown.Name = "BodyNumericUpDown";
            BodyNumericUpDown.Size = new System.Drawing.Size(200, 23);
            BodyNumericUpDown.TabIndex = 2;
            //
            // ActionLabel
            //
            ActionLabel.AutoSize = true;
            ActionLabel.Location = new System.Drawing.Point(14, 76);
            ActionLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            ActionLabel.Name = "ActionLabel";
            ActionLabel.Size = new System.Drawing.Size(42, 15);
            ActionLabel.TabIndex = 3;
            ActionLabel.Text = "Action";
            //
            // ActionNumericUpDown
            //
            ActionNumericUpDown.Location = new System.Drawing.Point(74, 74);
            ActionNumericUpDown.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ActionNumericUpDown.Maximum = new decimal(new int[] { 34, 0, 0, 0 });
            ActionNumericUpDown.Name = "ActionNumericUpDown";
            ActionNumericUpDown.Size = new System.Drawing.Size(200, 23);
            ActionNumericUpDown.TabIndex = 4;
            //
            // CopyButton
            //
            CopyButton.Location = new System.Drawing.Point(74, 110);
            CopyButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            CopyButton.Name = "CopyButton";
            CopyButton.Size = new System.Drawing.Size(96, 27);
            CopyButton.TabIndex = 5;
            CopyButton.Text = "Copy";
            CopyButton.UseVisualStyleBackColor = true;
            CopyButton.Click += OnClickCopy;
            //
            // CloseButton
            //
            CloseButton.Location = new System.Drawing.Point(178, 110);
            CloseButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            CloseButton.Name = "CloseButton";
            CloseButton.Size = new System.Drawing.Size(96, 27);
            CloseButton.TabIndex = 6;
            CloseButton.Text = "Cancel";
            CloseButton.UseVisualStyleBackColor = true;
            CloseButton.Click += OnClickCancel;
            //
            // AnimationCopyActionForm
            //
            AcceptButton = CopyButton;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = CloseButton;
            ClientSize = new System.Drawing.Size(290, 150);
            Controls.Add(CloseButton);
            Controls.Add(CopyButton);
            Controls.Add(ActionNumericUpDown);
            Controls.Add(ActionLabel);
            Controls.Add(BodyNumericUpDown);
            Controls.Add(BodyLabel);
            Controls.Add(SourceLabel);
            DoubleBuffered = true;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "AnimationCopyActionForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Copy Action";
            ((System.ComponentModel.ISupportInitialize)BodyNumericUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)ActionNumericUpDown).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label SourceLabel;
        private System.Windows.Forms.Label BodyLabel;
        private System.Windows.Forms.NumericUpDown BodyNumericUpDown;
        private System.Windows.Forms.Label ActionLabel;
        private System.Windows.Forms.NumericUpDown ActionNumericUpDown;
        private System.Windows.Forms.Button CopyButton;
        private System.Windows.Forms.Button CloseButton;
    }
}
