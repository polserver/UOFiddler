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
    partial class AnimDataExportForm
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
            components = new System.ComponentModel.Container();
            button2 = new System.Windows.Forms.Button();
            checkBox1 = new System.Windows.Forms.CheckBox();
            checkBox2 = new System.Windows.Forms.CheckBox();
            SuspendLayout();
            //
            // button2
            //
            button2.Location = new System.Drawing.Point(168, 73);
            button2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(88, 27);
            button2.TabIndex = 18;
            button2.Text = "Export";
            button2.UseVisualStyleBackColor = true;
            //
            // checkBox1
            //
            checkBox1.AutoSize = true;
            checkBox1.Location = new System.Drawing.Point(17, 11);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new System.Drawing.Size(278, 19);
            checkBox1.TabIndex = 19;
            checkBox1.Text = "Include missing/invalid statics (red-listed items)\r\n";
            checkBox1.UseVisualStyleBackColor = true;
            //
            // checkBox2
            //
            checkBox2.AutoSize = true;
            checkBox2.Location = new System.Drawing.Point(17, 36);
            checkBox2.Name = "checkBox2";
            checkBox2.Size = new System.Drawing.Size(338, 19);
            checkBox2.TabIndex = 20;
            checkBox2.Text = "Include tiles with missing animation flag (blue-listed items)";
            checkBox2.UseVisualStyleBackColor = true;
            //
            // AnimDataExportForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(415, 112);
            Controls.Add(checkBox2);
            Controls.Add(checkBox1);
            Controls.Add(button2);
            DoubleBuffered = true;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "AnimDataExportForm";
            Text = "Export AnimData";
            ResumeLayout(false);
            PerformLayout();
        }

        private System.Windows.Forms.Button button2;

        #endregion

        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.CheckBox checkBox2;
    }
}