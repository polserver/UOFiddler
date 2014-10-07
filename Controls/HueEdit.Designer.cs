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

namespace FiddlerControls
{
    partial class HueEdit
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
            this.button1 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
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
            this.button2 = new System.Windows.Forms.Button();
            this.numericUpDownB_R = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownG_R = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownR_R = new System.Windows.Forms.NumericUpDown();
            this.button4 = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.button5 = new System.Windows.Forms.Button();
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
            this.textBoxName.Location = new System.Drawing.Point(13, 13);
            this.textBoxName.MaxLength = 20;
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(100, 20);
            this.textBoxName.TabIndex = 0;
            // 
            // pictureBox
            // 
            this.pictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox.Location = new System.Drawing.Point(13, 40);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(537, 38);
            this.pictureBox.TabIndex = 1;
            this.pictureBox.TabStop = false;
            this.toolTip1.SetToolTip(this.pictureBox, "Select Second per Right Mouse");
            this.pictureBox.MouseClick += new System.Windows.Forms.MouseEventHandler(this.OnMouseClick);
            this.pictureBox.Paint += new System.Windows.Forms.PaintEventHandler(this.OnPaintPicture);
            // 
            // pictureBoxIndex
            // 
            this.pictureBoxIndex.Location = new System.Drawing.Point(13, 84);
            this.pictureBoxIndex.Name = "pictureBoxIndex";
            this.pictureBoxIndex.Size = new System.Drawing.Size(100, 66);
            this.pictureBoxIndex.TabIndex = 2;
            this.pictureBoxIndex.TabStop = false;
            this.pictureBoxIndex.Paint += new System.Windows.Forms.PaintEventHandler(this.onPaintIndexColor);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(118, 106);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "Color Picker";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.OnClickColorPicker);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(475, 323);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 6;
            this.button3.Text = "Save";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.OnClickSave);
            // 
            // Spread
            // 
            this.Spread.AutoSize = true;
            this.Spread.Location = new System.Drawing.Point(304, 84);
            this.Spread.Name = "Spread";
            this.Spread.Size = new System.Drawing.Size(89, 23);
            this.Spread.TabIndex = 7;
            this.Spread.Text = "Linear Gradient";
            this.Spread.UseVisualStyleBackColor = true;
            this.Spread.Click += new System.EventHandler(this.OnClickSpread);
            // 
            // numericUpDownR
            // 
            this.numericUpDownR.Location = new System.Drawing.Point(220, 84);
            this.numericUpDownR.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDownR.Name = "numericUpDownR";
            this.numericUpDownR.Size = new System.Drawing.Size(57, 20);
            this.numericUpDownR.TabIndex = 8;
            this.numericUpDownR.ValueChanged += new System.EventHandler(this.onChangeRGB);
            // 
            // numericUpDownG
            // 
            this.numericUpDownG.Location = new System.Drawing.Point(220, 108);
            this.numericUpDownG.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDownG.Name = "numericUpDownG";
            this.numericUpDownG.Size = new System.Drawing.Size(57, 20);
            this.numericUpDownG.TabIndex = 9;
            this.numericUpDownG.ValueChanged += new System.EventHandler(this.onChangeRGB);
            // 
            // numericUpDownB
            // 
            this.numericUpDownB.Location = new System.Drawing.Point(220, 132);
            this.numericUpDownB.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDownB.Name = "numericUpDownB";
            this.numericUpDownB.Size = new System.Drawing.Size(57, 20);
            this.numericUpDownB.TabIndex = 10;
            this.numericUpDownB.ValueChanged += new System.EventHandler(this.onChangeRGB);
            // 
            // pictureBoxPreview
            // 
            this.pictureBoxPreview.BackColor = System.Drawing.Color.White;
            this.pictureBoxPreview.ContextMenuStrip = this.contextMenuStrip1;
            this.pictureBoxPreview.Location = new System.Drawing.Point(13, 157);
            this.pictureBoxPreview.Name = "pictureBoxPreview";
            this.pictureBoxPreview.Size = new System.Drawing.Size(264, 189);
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
            this.contextMenuStrip1.Size = new System.Drawing.Size(149, 92);
            // 
            // itemToolStripMenuItem
            // 
            this.itemToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TextBoxArt});
            this.itemToolStripMenuItem.Name = "itemToolStripMenuItem";
            this.itemToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
            this.itemToolStripMenuItem.Text = "Art";
            // 
            // TextBoxArt
            // 
            this.TextBoxArt.Name = "TextBoxArt";
            this.TextBoxArt.Size = new System.Drawing.Size(100, 21);
            this.TextBoxArt.KeyDown += new System.Windows.Forms.KeyEventHandler(this.onKeyDownArt);
            this.TextBoxArt.TextChanged += new System.EventHandler(this.onTextChangedArt);
            // 
            // animationToolStripMenuItem
            // 
            this.animationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TextBoxAnim});
            this.animationToolStripMenuItem.Name = "animationToolStripMenuItem";
            this.animationToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
            this.animationToolStripMenuItem.Text = "Animation";
            // 
            // TextBoxAnim
            // 
            this.TextBoxAnim.Name = "TextBoxAnim";
            this.TextBoxAnim.Size = new System.Drawing.Size(100, 21);
            this.TextBoxAnim.KeyDown += new System.Windows.Forms.KeyEventHandler(this.onKeyDownAnim);
            this.TextBoxAnim.TextChanged += new System.EventHandler(this.onTextChangedAnim);
            // 
            // gumpToolStripMenuItem
            // 
            this.gumpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TextBoxGump});
            this.gumpToolStripMenuItem.Name = "gumpToolStripMenuItem";
            this.gumpToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
            this.gumpToolStripMenuItem.Text = "Gump";
            // 
            // TextBoxGump
            // 
            this.TextBoxGump.Name = "TextBoxGump";
            this.TextBoxGump.Size = new System.Drawing.Size(100, 21);
            this.TextBoxGump.KeyDown += new System.Windows.Forms.KeyEventHandler(this.onKeyDownGump);
            this.TextBoxGump.TextChanged += new System.EventHandler(this.onTextChangedGump);
            // 
            // hueOnlyGreyToolStripMenuItem
            // 
            this.hueOnlyGreyToolStripMenuItem.CheckOnClick = true;
            this.hueOnlyGreyToolStripMenuItem.Name = "hueOnlyGreyToolStripMenuItem";
            this.hueOnlyGreyToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
            this.hueOnlyGreyToolStripMenuItem.Text = "Hue only Grey";
            this.hueOnlyGreyToolStripMenuItem.Click += new System.EventHandler(this.onClickHueOnlyGrey);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(304, 132);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(89, 23);
            this.button2.TabIndex = 13;
            this.button2.Text = "Inverse";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.OnClickInverse);
            // 
            // numericUpDownB_R
            // 
            this.numericUpDownB_R.Location = new System.Drawing.Point(404, 133);
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
            this.numericUpDownG_R.Location = new System.Drawing.Point(404, 109);
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
            this.numericUpDownR_R.Location = new System.Drawing.Point(404, 85);
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
            // button4
            // 
            this.button4.AutoSize = true;
            this.button4.Location = new System.Drawing.Point(467, 108);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(83, 23);
            this.button4.TabIndex = 17;
            this.button4.Text = "Modify Range";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.OnClickModifyRange);
            // 
            // button5
            // 
            this.button5.AutoSize = true;
            this.button5.Location = new System.Drawing.Point(304, 108);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(89, 23);
            this.button5.TabIndex = 18;
            this.button5.Text = "Exp Gradient";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.onClickEpxGradient);
            // 
            // HueEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(562, 358);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.numericUpDownB_R);
            this.Controls.Add(this.numericUpDownG_R);
            this.Controls.Add(this.numericUpDownR_R);
            this.Controls.Add(this.pictureBoxPreview);
            this.Controls.Add(this.numericUpDownB);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.numericUpDownG);
            this.Controls.Add(this.numericUpDownR);
            this.Controls.Add(this.Spread);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.pictureBox);
            this.Controls.Add(this.pictureBoxIndex);
            this.Controls.Add(this.textBoxName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "HueEdit";
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

        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.PictureBox pictureBoxIndex;
        private System.Windows.Forms.ColorDialog colorDialog;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button Spread;
        private System.Windows.Forms.NumericUpDown numericUpDownR;
        private System.Windows.Forms.NumericUpDown numericUpDownG;
        private System.Windows.Forms.NumericUpDown numericUpDownB;
        private System.Windows.Forms.PictureBox pictureBoxPreview;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem itemToolStripMenuItem;
        private System.Windows.Forms.ToolStripTextBox TextBoxArt;
        private System.Windows.Forms.ToolStripMenuItem animationToolStripMenuItem;
        private System.Windows.Forms.ToolStripTextBox TextBoxAnim;
        private System.Windows.Forms.ToolStripMenuItem hueOnlyGreyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gumpToolStripMenuItem;
        private System.Windows.Forms.ToolStripTextBox TextBoxGump;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.NumericUpDown numericUpDownB_R;
        private System.Windows.Forms.NumericUpDown numericUpDownG_R;
        private System.Windows.Forms.NumericUpDown numericUpDownR_R;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button button5;
    }
}