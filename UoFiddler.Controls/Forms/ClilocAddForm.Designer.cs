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
    partial class ClilocAddForm
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
            NumberBox = new System.Windows.Forms.TextBox();
            label1 = new System.Windows.Forms.Label();
            Add_Button = new System.Windows.Forms.Button();
            Cancel_Button = new System.Windows.Forms.Button();
            NextFree_Button = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // NumberBox
            // 
            NumberBox.Location = new System.Drawing.Point(45, 14);
            NumberBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            NumberBox.Name = "NumberBox";
            NumberBox.Size = new System.Drawing.Size(95, 23);
            NumberBox.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(14, 17);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(23, 15);
            label1.TabIndex = 1;
            label1.Text = "Nr:";
            // 
            // Add_Button
            // 
            Add_Button.Location = new System.Drawing.Point(16, 43);
            Add_Button.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Add_Button.Name = "Add_Button";
            Add_Button.Size = new System.Drawing.Size(94, 27);
            Add_Button.TabIndex = 2;
            Add_Button.Text = "Add";
            Add_Button.UseVisualStyleBackColor = true;
            Add_Button.Click += OnClickAdd;
            // 
            // Cancel_Button
            // 
            Cancel_Button.Location = new System.Drawing.Point(118, 43);
            Cancel_Button.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Cancel_Button.Name = "Cancel_Button";
            Cancel_Button.Size = new System.Drawing.Size(94, 27);
            Cancel_Button.TabIndex = 3;
            Cancel_Button.Text = "Cancel";
            Cancel_Button.UseVisualStyleBackColor = true;
            Cancel_Button.Click += OnClickCancel;
            // 
            // NextFree_Button
            // 
            NextFree_Button.Location = new System.Drawing.Point(148, 12);
            NextFree_Button.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            NextFree_Button.Name = "NextFree_Button";
            NextFree_Button.Size = new System.Drawing.Size(64, 25);
            NextFree_Button.TabIndex = 1;
            NextFree_Button.Text = "Next free";
            NextFree_Button.UseVisualStyleBackColor = true;
            NextFree_Button.Click += OnClickNextFree;
            // 
            // ClilocAddForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(224, 81);
            Controls.Add(Cancel_Button);
            Controls.Add(Add_Button);
            Controls.Add(NextFree_Button);
            Controls.Add(label1);
            Controls.Add(NumberBox);
            DoubleBuffered = true;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "ClilocAddForm";
            Text = "Add CliLoc";
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Add_Button;
        private System.Windows.Forms.Button Cancel_Button;
        private System.Windows.Forms.Button NextFree_Button;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox NumberBox;
    }
}