namespace UoFiddler.Plugin.MultiEditor.UI
{
    partial class MapExtractForm
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
            labelFacet = new System.Windows.Forms.Label();
            _facet = new System.Windows.Forms.ComboBox();
            labelX = new System.Windows.Forms.Label();
            _x = new System.Windows.Forms.NumericUpDown();
            labelY = new System.Windows.Forms.Label();
            _y = new System.Windows.Forms.NumericUpDown();
            labelWidth = new System.Windows.Forms.Label();
            _width = new System.Windows.Forms.NumericUpDown();
            labelHeight = new System.Windows.Forms.Label();
            _height = new System.Windows.Forms.NumericUpDown();
            labelMinZ = new System.Windows.Forms.Label();
            _minZ = new System.Windows.Forms.NumericUpDown();
            labelMaxZ = new System.Windows.Forms.Label();
            _maxZ = new System.Windows.Forms.NumericUpDown();
            _newDesign = new System.Windows.Forms.CheckBox();
            buttonOk = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)_x).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_y).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_width).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_height).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_minZ).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_maxZ).BeginInit();
            SuspendLayout();
            // 
            // labelFacet
            // 
            labelFacet.AutoSize = true;
            labelFacet.Location = new System.Drawing.Point(10, 13);
            labelFacet.Name = "labelFacet";
            labelFacet.Size = new System.Drawing.Size(41, 15);
            labelFacet.TabIndex = 0;
            labelFacet.Text = "Facet:";
            // 
            // _facet
            // 
            _facet.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            _facet.Location = new System.Drawing.Point(110, 10);
            _facet.Name = "_facet";
            _facet.Size = new System.Drawing.Size(180, 23);
            _facet.TabIndex = 1;
            // 
            // labelX
            // 
            labelX.AutoSize = true;
            labelX.Location = new System.Drawing.Point(10, 43);
            labelX.Name = "labelX";
            labelX.Size = new System.Drawing.Size(90, 15);
            labelX.TabIndex = 2;
            labelX.Text = "X (west edge):";
            // 
            // _x
            // 
            _x.Location = new System.Drawing.Point(110, 40);
            _x.Maximum = new decimal(new int[] { 32767, 0, 0, 0 });
            _x.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            _x.Name = "_x";
            _x.Size = new System.Drawing.Size(180, 23);
            _x.TabIndex = 3;
            // 
            // labelY
            // 
            labelY.AutoSize = true;
            labelY.Location = new System.Drawing.Point(10, 73);
            labelY.Name = "labelY";
            labelY.Size = new System.Drawing.Size(97, 15);
            labelY.TabIndex = 4;
            labelY.Text = "Y (north edge):";
            // 
            // _y
            // 
            _y.Location = new System.Drawing.Point(110, 70);
            _y.Maximum = new decimal(new int[] { 32767, 0, 0, 0 });
            _y.Minimum = new decimal(new int[] { 0, 0, 0, 0 });
            _y.Name = "_y";
            _y.Size = new System.Drawing.Size(180, 23);
            _y.TabIndex = 5;
            // 
            // labelWidth
            // 
            labelWidth.AutoSize = true;
            labelWidth.Location = new System.Drawing.Point(10, 103);
            labelWidth.Name = "labelWidth";
            labelWidth.Size = new System.Drawing.Size(41, 15);
            labelWidth.TabIndex = 6;
            labelWidth.Text = "Width:";
            // 
            // _width
            // 
            _width.Location = new System.Drawing.Point(110, 100);
            _width.Maximum = new decimal(new int[] { 256, 0, 0, 0 });
            _width.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            _width.Name = "_width";
            _width.Size = new System.Drawing.Size(180, 23);
            _width.TabIndex = 7;
            // 
            // labelHeight
            // 
            labelHeight.AutoSize = true;
            labelHeight.Location = new System.Drawing.Point(10, 133);
            labelHeight.Name = "labelHeight";
            labelHeight.Size = new System.Drawing.Size(47, 15);
            labelHeight.TabIndex = 8;
            labelHeight.Text = "Height:";
            // 
            // _height
            // 
            _height.Location = new System.Drawing.Point(110, 130);
            _height.Maximum = new decimal(new int[] { 256, 0, 0, 0 });
            _height.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            _height.Name = "_height";
            _height.Size = new System.Drawing.Size(180, 23);
            _height.TabIndex = 9;
            // 
            // labelMinZ
            // 
            labelMinZ.AutoSize = true;
            labelMinZ.Location = new System.Drawing.Point(10, 163);
            labelMinZ.Name = "labelMinZ";
            labelMinZ.Size = new System.Drawing.Size(59, 15);
            labelMinZ.TabIndex = 10;
            labelMinZ.Text = "Lowest Z:";
            // 
            // _minZ
            // 
            _minZ.Location = new System.Drawing.Point(110, 160);
            _minZ.Maximum = new decimal(new int[] { 127, 0, 0, 0 });
            _minZ.Minimum = new decimal(new int[] { 128, 0, 0, -2147483648 });
            _minZ.Name = "_minZ";
            _minZ.Size = new System.Drawing.Size(180, 23);
            _minZ.TabIndex = 11;
            // 
            // labelMaxZ
            // 
            labelMaxZ.AutoSize = true;
            labelMaxZ.Location = new System.Drawing.Point(10, 193);
            labelMaxZ.Name = "labelMaxZ";
            labelMaxZ.Size = new System.Drawing.Size(66, 15);
            labelMaxZ.TabIndex = 12;
            labelMaxZ.Text = "Highest Z:";
            // 
            // _maxZ
            // 
            _maxZ.Location = new System.Drawing.Point(110, 190);
            _maxZ.Maximum = new decimal(new int[] { 127, 0, 0, 0 });
            _maxZ.Minimum = new decimal(new int[] { 128, 0, 0, -2147483648 });
            _maxZ.Name = "_maxZ";
            _maxZ.Size = new System.Drawing.Size(180, 23);
            _maxZ.TabIndex = 13;
            // 
            // _newDesign
            // 
            _newDesign.AutoSize = true;
            _newDesign.Location = new System.Drawing.Point(10, 222);
            _newDesign.Name = "_newDesign";
            _newDesign.Size = new System.Drawing.Size(142, 19);
            _newDesign.TabIndex = 14;
            _newDesign.Text = "Open as a new design";
            _newDesign.UseVisualStyleBackColor = true;
            // 
            // buttonOk
            // 
            buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            buttonOk.Location = new System.Drawing.Point(215, 218);
            buttonOk.Name = "buttonOk";
            buttonOk.Size = new System.Drawing.Size(75, 23);
            buttonOk.TabIndex = 15;
            buttonOk.Text = "Extract";
            buttonOk.UseVisualStyleBackColor = true;
            // 
            // MapExtractForm
            // 
            AcceptButton = buttonOk;
            ClientSize = new System.Drawing.Size(300, 250);
            Controls.Add(labelFacet);
            Controls.Add(_facet);
            Controls.Add(labelX);
            Controls.Add(_x);
            Controls.Add(labelY);
            Controls.Add(_y);
            Controls.Add(labelWidth);
            Controls.Add(_width);
            Controls.Add(labelHeight);
            Controls.Add(_height);
            Controls.Add(labelMinZ);
            Controls.Add(_minZ);
            Controls.Add(labelMaxZ);
            Controls.Add(_maxZ);
            Controls.Add(_newDesign);
            Controls.Add(buttonOk);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "MapExtractForm";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Extract statics from the map";
            ((System.ComponentModel.ISupportInitialize)_x).EndInit();
            ((System.ComponentModel.ISupportInitialize)_y).EndInit();
            ((System.ComponentModel.ISupportInitialize)_width).EndInit();
            ((System.ComponentModel.ISupportInitialize)_height).EndInit();
            ((System.ComponentModel.ISupportInitialize)_minZ).EndInit();
            ((System.ComponentModel.ISupportInitialize)_maxZ).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label labelFacet;
        private System.Windows.Forms.ComboBox _facet;
        private System.Windows.Forms.Label labelX;
        private System.Windows.Forms.NumericUpDown _x;
        private System.Windows.Forms.Label labelY;
        private System.Windows.Forms.NumericUpDown _y;
        private System.Windows.Forms.Label labelWidth;
        private System.Windows.Forms.NumericUpDown _width;
        private System.Windows.Forms.Label labelHeight;
        private System.Windows.Forms.NumericUpDown _height;
        private System.Windows.Forms.Label labelMinZ;
        private System.Windows.Forms.NumericUpDown _minZ;
        private System.Windows.Forms.Label labelMaxZ;
        private System.Windows.Forms.NumericUpDown _maxZ;
        private System.Windows.Forms.CheckBox _newDesign;
        private System.Windows.Forms.Button buttonOk;
    }
}
