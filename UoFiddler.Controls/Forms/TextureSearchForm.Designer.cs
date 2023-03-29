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
    partial class TextureSearchForm
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
            searchButton = new System.Windows.Forms.Button();
            graphicTextbox = new System.Windows.Forms.TextBox();
            SuspendLayout();
            // 
            // searchButton
            // 
            searchButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            searchButton.AutoSize = true;
            searchButton.Location = new System.Drawing.Point(116, 45);
            searchButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            searchButton.Name = "searchButton";
            searchButton.Size = new System.Drawing.Size(112, 27);
            searchButton.TabIndex = 11;
            searchButton.Text = "Search ID";
            searchButton.UseVisualStyleBackColor = true;
            searchButton.Click += SearchGraphic;
            // 
            // graphicTextbox
            // 
            graphicTextbox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            graphicTextbox.Location = new System.Drawing.Point(21, 14);
            graphicTextbox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            graphicTextbox.Name = "graphicTextbox";
            graphicTextbox.Size = new System.Drawing.Size(302, 23);
            graphicTextbox.TabIndex = 10;
            graphicTextbox.KeyDown += OnKeyDownSearch;
            // 
            // TextureSearchForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(344, 81);
            Controls.Add(searchButton);
            Controls.Add(graphicTextbox);
            DoubleBuffered = true;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "TextureSearchForm";
            Text = "Texture Search";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button searchButton;
        private System.Windows.Forms.TextBox graphicTextbox;
    }
}