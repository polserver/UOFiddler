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
    partial class AnimDataImportForm
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
            label5 = new System.Windows.Forms.Label();
            button1 = new System.Windows.Forms.Button();
            textBox1 = new System.Windows.Forms.TextBox();
            button2 = new System.Windows.Forms.Button();
            comboBoxUpsertAction = new System.Windows.Forms.ComboBox();
            label1 = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(10, 10);
            label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(75, 15);
            label5.TabIndex = 17;
            label5.Text = "Import from:";
            // 
            // button1
            // 
            button1.AutoSize = true;
            button1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            button1.Location = new System.Drawing.Point(369, 7);
            button1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(26, 25);
            button1.TabIndex = 16;
            button1.Text = "...";
            button1.UseVisualStyleBackColor = true;
            button1.Click += OnClickBrowse;
            // 
            // textBox1
            // 
            textBox1.Location = new System.Drawing.Point(103, 9);
            textBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            textBox1.Name = "textBox1";
            textBox1.Size = new System.Drawing.Size(258, 23);
            textBox1.TabIndex = 15;
            // 
            // button2
            // 
            button2.Location = new System.Drawing.Point(168, 93);
            button2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(88, 27);
            button2.TabIndex = 18;
            button2.Text = "Import";
            button2.UseVisualStyleBackColor = true;
            button2.Click += OnClickImport;
            // 
            // comboBoxUpsertAction
            // 
            comboBoxUpsertAction.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxUpsertAction.FormattingEnabled = true;
            comboBoxUpsertAction.Items.AddRange(new object[] { "skip", "overwrite" });
            comboBoxUpsertAction.Location = new System.Drawing.Point(103, 48);
            comboBoxUpsertAction.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            comboBoxUpsertAction.Name = "comboBoxUpsertAction";
            comboBoxUpsertAction.Size = new System.Drawing.Size(140, 23);
            comboBoxUpsertAction.TabIndex = 21;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(15, 52);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(69, 15);
            label1.TabIndex = 22;
            label1.Text = "On conflict:";
            // 
            // AnimDataImportForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(415, 134);
            Controls.Add(label1);
            Controls.Add(comboBoxUpsertAction);
            Controls.Add(button2);
            Controls.Add(label5);
            Controls.Add(button1);
            Controls.Add(textBox1);
            DoubleBuffered = true;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "AnimDataImportForm";
            Text = "Import AnimData";
            ResumeLayout(false);
            PerformLayout();
        }

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox1;

        #endregion
        private System.Windows.Forms.ComboBox comboBoxUpsertAction;
        private System.Windows.Forms.Label label1;
    }
}