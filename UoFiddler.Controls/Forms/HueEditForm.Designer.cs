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
    partial class HueEditForm
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
            this.components = new System.ComponentModel.Container();
            this.textBoxName = new System.Windows.Forms.TextBox();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.pictureBoxIndex = new System.Windows.Forms.PictureBox();
            this.colorDialog = new System.Windows.Forms.ColorDialog();
            this.ColorPickerButton = new System.Windows.Forms.Button();
            this.SaveButton = new System.Windows.Forms.Button();
            this.Spread = new System.Windows.Forms.Button();
            this.numericUpDownR = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownG = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownB = new System.Windows.Forms.NumericUpDown();
            this.pictureBoxPreview = new System.Windows.Forms.PictureBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.itemToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.TextBoxArt = new System.Windows.Forms.ToolStripTextBox();
            this.animationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.TextBoxAnim = new System.Windows.Forms.ToolStripTextBox();
            this.gumpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.TextBoxGump = new System.Windows.Forms.ToolStripTextBox();
            this.hueOnlyGreyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.InverseButton = new System.Windows.Forms.Button();
            this.numericUpDownB_R = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownG_R = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownR_R = new System.Windows.Forms.NumericUpDown();
            this.ModifyRangeButton = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.ExpGradientButton = new System.Windows.Forms.Button();
            this.RLabel = new System.Windows.Forms.Label();
            this.GLabel = new System.Windows.Forms.Label();
            this.BLabel = new System.Windows.Forms.Label();
            this.SetColorButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxIndex)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownR)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownG)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownB)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownB_R)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownG_R)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownR_R)).BeginInit();
            this.SuspendLayout();
            // 
            // textBoxName
            // 
            this.textBoxName.Location = new System.Drawing.Point(12, 12);
            this.textBoxName.MaxLength = 20;
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(197, 20);
            this.textBoxName.TabIndex = 0;
            // 
            // pictureBox
            // 
            this.pictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox.Location = new System.Drawing.Point(12, 38);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(552, 38);
            this.pictureBox.TabIndex = 1;
            this.pictureBox.TabStop = false;
            this.toolTip1.SetToolTip(this.pictureBox, "Select Second per Right Mouse");
            this.pictureBox.Paint += new System.Windows.Forms.PaintEventHandler(this.OnPaintPicture);
            this.pictureBox.MouseClick += new System.Windows.Forms.MouseEventHandler(this.OnMouseClick);
            // 
            // pictureBoxIndex
            // 
            this.pictureBoxIndex.Location = new System.Drawing.Point(12, 82);
            this.pictureBoxIndex.Name = "pictureBoxIndex";
            this.pictureBoxIndex.Size = new System.Drawing.Size(100, 73);
            this.pictureBoxIndex.TabIndex = 2;
            this.pictureBoxIndex.TabStop = false;
            this.pictureBoxIndex.Paint += new System.Windows.Forms.PaintEventHandler(this.OnPaintIndexColor);
            // 
            // ColorPickerButton
            // 
            this.ColorPickerButton.Location = new System.Drawing.Point(12, 161);
            this.ColorPickerButton.Name = "ColorPickerButton";
            this.ColorPickerButton.Size = new System.Drawing.Size(100, 23);
            this.ColorPickerButton.TabIndex = 3;
            this.ColorPickerButton.Text = "Color Picker";
            this.ColorPickerButton.UseVisualStyleBackColor = true;
            this.ColorPickerButton.Click += new System.EventHandler(this.OnClickColorPicker);
            // 
            // SaveButton
            // 
            this.SaveButton.Location = new System.Drawing.Point(492, 373);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(75, 23);
            this.SaveButton.TabIndex = 6;
            this.SaveButton.Text = "Save";
            this.SaveButton.UseVisualStyleBackColor = true;
            this.SaveButton.Click += new System.EventHandler(this.OnClickSave);
            // 
            // Spread
            // 
            this.Spread.AutoSize = true;
            this.Spread.Location = new System.Drawing.Point(304, 82);
            this.Spread.Name = "Spread";
            this.Spread.Size = new System.Drawing.Size(89, 23);
            this.Spread.TabIndex = 7;
            this.Spread.Text = "Linear Gradient";
            this.Spread.UseVisualStyleBackColor = true;
            this.Spread.Click += new System.EventHandler(this.OnClickSpread);
            // 
            // numericUpDownR
            // 
            this.numericUpDownR.Location = new System.Drawing.Point(152, 82);
            this.numericUpDownR.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDownR.Name = "numericUpDownR";
            this.numericUpDownR.Size = new System.Drawing.Size(57, 20);
            this.numericUpDownR.TabIndex = 8;
            // 
            // numericUpDownG
            // 
            this.numericUpDownG.Location = new System.Drawing.Point(152, 108);
            this.numericUpDownG.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDownG.Name = "numericUpDownG";
            this.numericUpDownG.Size = new System.Drawing.Size(57, 20);
            this.numericUpDownG.TabIndex = 9;
            // 
            // numericUpDownB
            // 
            this.numericUpDownB.Location = new System.Drawing.Point(152, 134);
            this.numericUpDownB.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDownB.Name = "numericUpDownB";
            this.numericUpDownB.Size = new System.Drawing.Size(57, 20);
            this.numericUpDownB.TabIndex = 10;
            // 
            // pictureBoxPreview
            // 
            this.pictureBoxPreview.BackColor = System.Drawing.Color.White;
            this.pictureBoxPreview.ContextMenuStrip = this.contextMenuStrip1;
            this.pictureBoxPreview.Location = new System.Drawing.Point(12, 190);
            this.pictureBoxPreview.Name = "pictureBoxPreview";
            this.pictureBoxPreview.Size = new System.Drawing.Size(267, 206);
            this.pictureBoxPreview.TabIndex = 11;
            this.pictureBoxPreview.TabStop = false;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.itemToolStripMenuItem,
            this.animationToolStripMenuItem,
            this.gumpToolStripMenuItem,
            this.hueOnlyGreyToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(150, 92);
            // 
            // itemToolStripMenuItem
            // 
            this.itemToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TextBoxArt});
            this.itemToolStripMenuItem.Name = "itemToolStripMenuItem";
            this.itemToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.itemToolStripMenuItem.Text = "Art";
            // 
            // TextBoxArt
            // 
            this.TextBoxArt.Name = "TextBoxArt";
            this.TextBoxArt.Size = new System.Drawing.Size(100, 23);
            this.TextBoxArt.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnKeyDownArt);
            this.TextBoxArt.TextChanged += new System.EventHandler(this.OnTextChangedArt);
            // 
            // animationToolStripMenuItem
            // 
            this.animationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TextBoxAnim});
            this.animationToolStripMenuItem.Name = "animationToolStripMenuItem";
            this.animationToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.animationToolStripMenuItem.Text = "Animation";
            // 
            // TextBoxAnim
            // 
            this.TextBoxAnim.Name = "TextBoxAnim";
            this.TextBoxAnim.Size = new System.Drawing.Size(100, 23);
            this.TextBoxAnim.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnKeyDownAnim);
            this.TextBoxAnim.TextChanged += new System.EventHandler(this.OnTextChangedAnim);
            // 
            // gumpToolStripMenuItem
            // 
            this.gumpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TextBoxGump});
            this.gumpToolStripMenuItem.Name = "gumpToolStripMenuItem";
            this.gumpToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.gumpToolStripMenuItem.Text = "Gump";
            // 
            // TextBoxGump
            // 
            this.TextBoxGump.Name = "TextBoxGump";
            this.TextBoxGump.Size = new System.Drawing.Size(100, 23);
            this.TextBoxGump.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnKeyDownGump);
            this.TextBoxGump.TextChanged += new System.EventHandler(this.OnTextChangedGump);
            // 
            // hueOnlyGreyToolStripMenuItem
            // 
            this.hueOnlyGreyToolStripMenuItem.CheckOnClick = true;
            this.hueOnlyGreyToolStripMenuItem.Name = "hueOnlyGreyToolStripMenuItem";
            this.hueOnlyGreyToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.hueOnlyGreyToolStripMenuItem.Text = "Hue only Grey";
            this.hueOnlyGreyToolStripMenuItem.Click += new System.EventHandler(this.OnClickHueOnlyGrey);
            // 
            // InverseButton
            // 
            this.InverseButton.Location = new System.Drawing.Point(304, 140);
            this.InverseButton.Name = "InverseButton";
            this.InverseButton.Size = new System.Drawing.Size(89, 23);
            this.InverseButton.TabIndex = 13;
            this.InverseButton.Text = "Inverse";
            this.InverseButton.UseVisualStyleBackColor = true;
            this.InverseButton.Click += new System.EventHandler(this.OnClickInverse);
            // 
            // numericUpDownB_R
            // 
            this.numericUpDownB_R.Location = new System.Drawing.Point(404, 140);
            this.numericUpDownB_R.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDownB_R.Minimum = new decimal(new int[] {
            255,
            0,
            0,
            -2147483648});
            this.numericUpDownB_R.Name = "numericUpDownB_R";
            this.numericUpDownB_R.Size = new System.Drawing.Size(57, 20);
            this.numericUpDownB_R.TabIndex = 16;
            // 
            // numericUpDownG_R
            // 
            this.numericUpDownG_R.Location = new System.Drawing.Point(404, 111);
            this.numericUpDownG_R.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDownG_R.Minimum = new decimal(new int[] {
            255,
            0,
            0,
            -2147483648});
            this.numericUpDownG_R.Name = "numericUpDownG_R";
            this.numericUpDownG_R.Size = new System.Drawing.Size(57, 20);
            this.numericUpDownG_R.TabIndex = 15;
            // 
            // numericUpDownR_R
            // 
            this.numericUpDownR_R.Location = new System.Drawing.Point(404, 82);
            this.numericUpDownR_R.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDownR_R.Minimum = new decimal(new int[] {
            255,
            0,
            0,
            -2147483648});
            this.numericUpDownR_R.Name = "numericUpDownR_R";
            this.numericUpDownR_R.Size = new System.Drawing.Size(57, 20);
            this.numericUpDownR_R.TabIndex = 14;
            // 
            // ModifyRangeButton
            // 
            this.ModifyRangeButton.AutoSize = true;
            this.ModifyRangeButton.Location = new System.Drawing.Point(477, 111);
            this.ModifyRangeButton.Name = "ModifyRangeButton";
            this.ModifyRangeButton.Size = new System.Drawing.Size(90, 23);
            this.ModifyRangeButton.TabIndex = 17;
            this.ModifyRangeButton.Text = "Modify Range";
            this.ModifyRangeButton.UseVisualStyleBackColor = true;
            this.ModifyRangeButton.Click += new System.EventHandler(this.OnClickModifyRange);
            // 
            // ExpGradientButton
            // 
            this.ExpGradientButton.AutoSize = true;
            this.ExpGradientButton.Location = new System.Drawing.Point(304, 111);
            this.ExpGradientButton.Name = "ExpGradientButton";
            this.ExpGradientButton.Size = new System.Drawing.Size(89, 23);
            this.ExpGradientButton.TabIndex = 18;
            this.ExpGradientButton.Text = "Exp Gradient";
            this.ExpGradientButton.UseVisualStyleBackColor = true;
            this.ExpGradientButton.Click += new System.EventHandler(this.OnClickEpxGradient);
            // 
            // RLabel
            // 
            this.RLabel.AutoSize = true;
            this.RLabel.Location = new System.Drawing.Point(131, 84);
            this.RLabel.Name = "RLabel";
            this.RLabel.Size = new System.Drawing.Size(15, 13);
            this.RLabel.TabIndex = 19;
            this.RLabel.Text = "R";
            // 
            // GLabel
            // 
            this.GLabel.AutoSize = true;
            this.GLabel.Location = new System.Drawing.Point(131, 110);
            this.GLabel.Name = "GLabel";
            this.GLabel.Size = new System.Drawing.Size(15, 13);
            this.GLabel.TabIndex = 20;
            this.GLabel.Text = "G";
            // 
            // BLabel
            // 
            this.BLabel.AutoSize = true;
            this.BLabel.Location = new System.Drawing.Point(131, 136);
            this.BLabel.Name = "BLabel";
            this.BLabel.Size = new System.Drawing.Size(14, 13);
            this.BLabel.TabIndex = 21;
            this.BLabel.Text = "B";
            // 
            // SetColorButton
            // 
            this.SetColorButton.Location = new System.Drawing.Point(215, 105);
            this.SetColorButton.Name = "SetColorButton";
            this.SetColorButton.Size = new System.Drawing.Size(64, 23);
            this.SetColorButton.TabIndex = 22;
            this.SetColorButton.Text = "Set color";
            this.SetColorButton.UseVisualStyleBackColor = true;
            this.SetColorButton.Click += new System.EventHandler(this.SetColorButton_Click);
            // 
            // HueEditForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(576, 408);
            this.Controls.Add(this.SetColorButton);
            this.Controls.Add(this.BLabel);
            this.Controls.Add(this.GLabel);
            this.Controls.Add(this.RLabel);
            this.Controls.Add(this.ModifyRangeButton);
            this.Controls.Add(this.ExpGradientButton);
            this.Controls.Add(this.numericUpDownB_R);
            this.Controls.Add(this.numericUpDownG_R);
            this.Controls.Add(this.numericUpDownR_R);
            this.Controls.Add(this.pictureBoxPreview);
            this.Controls.Add(this.numericUpDownB);
            this.Controls.Add(this.InverseButton);
            this.Controls.Add(this.numericUpDownG);
            this.Controls.Add(this.numericUpDownR);
            this.Controls.Add(this.Spread);
            this.Controls.Add(this.ColorPickerButton);
            this.Controls.Add(this.SaveButton);
            this.Controls.Add(this.pictureBox);
            this.Controls.Add(this.pictureBoxIndex);
            this.Controls.Add(this.textBoxName);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "HueEditForm";
            this.Text = "HueEdit";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxIndex)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownR)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownG)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownB)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownB_R)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownG_R)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownR_R)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStripMenuItem animationToolStripMenuItem;
        private System.Windows.Forms.Button ColorPickerButton;
        private System.Windows.Forms.Button InverseButton;
        private System.Windows.Forms.Button SaveButton;
        private System.Windows.Forms.Button ModifyRangeButton;
        private System.Windows.Forms.Button ExpGradientButton;
        private System.Windows.Forms.ColorDialog colorDialog;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem gumpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hueOnlyGreyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem itemToolStripMenuItem;
        private System.Windows.Forms.NumericUpDown numericUpDownB;
        private System.Windows.Forms.NumericUpDown numericUpDownB_R;
        private System.Windows.Forms.NumericUpDown numericUpDownG;
        private System.Windows.Forms.NumericUpDown numericUpDownG_R;
        private System.Windows.Forms.NumericUpDown numericUpDownR;
        private System.Windows.Forms.NumericUpDown numericUpDownR_R;
        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.PictureBox pictureBoxIndex;
        private System.Windows.Forms.PictureBox pictureBoxPreview;
        private System.Windows.Forms.Button Spread;
        private System.Windows.Forms.ToolStripTextBox TextBoxAnim;
        private System.Windows.Forms.ToolStripTextBox TextBoxArt;
        private System.Windows.Forms.ToolStripTextBox TextBoxGump;
        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Label RLabel;
        private System.Windows.Forms.Label GLabel;
        private System.Windows.Forms.Label BLabel;
        private System.Windows.Forms.Button SetColorButton;
    }
}