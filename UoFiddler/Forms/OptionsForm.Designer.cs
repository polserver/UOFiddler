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

namespace UoFiddler.Forms
{
    partial class OptionsForm
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.numericUpDownItemSizeHeight = new System.Windows.Forms.NumericUpDown();
            this.checkBoxItemClip = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.numericUpDownItemSizeWidth = new System.Windows.Forms.NumericUpDown();
            this.checkBoxCacheData = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkBoxPolSoundIdOffset = new System.Windows.Forms.CheckBox();
            this.checkBoxPanelSoundsDesign = new System.Windows.Forms.CheckBox();
            this.checkBoxuseDiff = new System.Windows.Forms.CheckBox();
            this.checkBoxNewMapSize = new System.Windows.Forms.CheckBox();
            this.buttonApply = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.FocusColorLabel = new System.Windows.Forms.Label();
            this.SelectedColorLabel = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.map5Nametext = new System.Windows.Forms.TextBox();
            this.argstext = new System.Windows.Forms.TextBox();
            this.cmdtext = new System.Windows.Forms.TextBox();
            this.map4Nametext = new System.Windows.Forms.TextBox();
            this.map3Nametext = new System.Windows.Forms.TextBox();
            this.map2Nametext = new System.Windows.Forms.TextBox();
            this.map1Nametext = new System.Windows.Forms.TextBox();
            this.map0Nametext = new System.Windows.Forms.TextBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.button2 = new System.Windows.Forms.Button();
            this.textBoxOutputPath = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.ColorsGroupBox = new System.Windows.Forms.GroupBox();
            this.DefaultColorsButton = new System.Windows.Forms.Button();
            this.TileSelectionColorComboBox = new System.Windows.Forms.ComboBox();
            this.TileFocusColorComboBox = new System.Windows.Forms.ComboBox();
            this.buttonClose = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownItemSizeHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownItemSizeWidth)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.ColorsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.numericUpDownItemSizeHeight);
            this.groupBox1.Controls.Add(this.checkBoxItemClip);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.numericUpDownItemSizeWidth);
            this.groupBox1.Location = new System.Drawing.Point(16, 164);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox1.Size = new System.Drawing.Size(258, 115);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Item Tab";
            // 
            // numericUpDownItemSizeHeight
            // 
            this.numericUpDownItemSizeHeight.Location = new System.Drawing.Point(155, 22);
            this.numericUpDownItemSizeHeight.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.numericUpDownItemSizeHeight.Maximum = new decimal(new int[] {
            512,
            0,
            0,
            0});
            this.numericUpDownItemSizeHeight.Minimum = new decimal(new int[] {
            48,
            0,
            0,
            0});
            this.numericUpDownItemSizeHeight.Name = "numericUpDownItemSizeHeight";
            this.numericUpDownItemSizeHeight.Size = new System.Drawing.Size(71, 23);
            this.numericUpDownItemSizeHeight.TabIndex = 3;
            this.toolTip1.SetToolTip(this.numericUpDownItemSizeHeight, "Height");
            this.numericUpDownItemSizeHeight.Value = new decimal(new int[] {
            96,
            0,
            0,
            0});
            // 
            // checkBoxItemClip
            // 
            this.checkBoxItemClip.AutoSize = true;
            this.checkBoxItemClip.Location = new System.Drawing.Point(10, 54);
            this.checkBoxItemClip.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBoxItemClip.Name = "checkBoxItemClip";
            this.checkBoxItemClip.Size = new System.Drawing.Size(74, 19);
            this.checkBoxItemClip.TabIndex = 2;
            this.checkBoxItemClip.Text = "Item Clip";
            this.toolTip1.SetToolTip(this.checkBoxItemClip, "ItemClip images in items tab shrinked or clipped");
            this.checkBoxItemClip.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 24);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 15);
            this.label1.TabIndex = 1;
            this.label1.Text = "Item Size";
            this.toolTip1.SetToolTip(this.label1, "ItemSize controls the size of images in items tab");
            // 
            // numericUpDownItemSizeWidth
            // 
            this.numericUpDownItemSizeWidth.Location = new System.Drawing.Point(72, 22);
            this.numericUpDownItemSizeWidth.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.numericUpDownItemSizeWidth.Maximum = new decimal(new int[] {
            512,
            0,
            0,
            0});
            this.numericUpDownItemSizeWidth.Minimum = new decimal(new int[] {
            48,
            0,
            0,
            0});
            this.numericUpDownItemSizeWidth.Name = "numericUpDownItemSizeWidth";
            this.numericUpDownItemSizeWidth.Size = new System.Drawing.Size(76, 23);
            this.numericUpDownItemSizeWidth.TabIndex = 0;
            this.toolTip1.SetToolTip(this.numericUpDownItemSizeWidth, "Width");
            this.numericUpDownItemSizeWidth.Value = new decimal(new int[] {
            96,
            0,
            0,
            0});
            // 
            // checkBoxCacheData
            // 
            this.checkBoxCacheData.AutoSize = true;
            this.checkBoxCacheData.Location = new System.Drawing.Point(7, 23);
            this.checkBoxCacheData.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBoxCacheData.Name = "checkBoxCacheData";
            this.checkBoxCacheData.Size = new System.Drawing.Size(86, 19);
            this.checkBoxCacheData.TabIndex = 2;
            this.checkBoxCacheData.Text = "Cache Data";
            this.toolTip1.SetToolTip(this.checkBoxCacheData, "CacheData should mul entries be cached for faster load");
            this.checkBoxCacheData.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.checkBoxPolSoundIdOffset);
            this.groupBox2.Controls.Add(this.checkBoxPanelSoundsDesign);
            this.groupBox2.Controls.Add(this.checkBoxuseDiff);
            this.groupBox2.Controls.Add(this.checkBoxNewMapSize);
            this.groupBox2.Controls.Add(this.checkBoxCacheData);
            this.groupBox2.Location = new System.Drawing.Point(16, 6);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox2.Size = new System.Drawing.Size(258, 157);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Misc";
            // 
            // checkBoxPolSoundIdOffset
            // 
            this.checkBoxPolSoundIdOffset.AutoSize = true;
            this.checkBoxPolSoundIdOffset.Location = new System.Drawing.Point(7, 129);
            this.checkBoxPolSoundIdOffset.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBoxPolSoundIdOffset.Name = "checkBoxPolSoundIdOffset";
            this.checkBoxPolSoundIdOffset.Size = new System.Drawing.Size(217, 19);
            this.checkBoxPolSoundIdOffset.TabIndex = 7;
            this.checkBoxPolSoundIdOffset.Text = "Offset Sound Id by 1 (POL emulator)";
            this.toolTip1.SetToolTip(this.checkBoxPolSoundIdOffset, "UO Sounds are indexed from 0 but POL uses +1 offset.\r\nWhen this option is checked" +
        " Sounds tab will display sound indexes starting from 1 instead of 0.\r\nThis optio" +
        "n also affects the export sound list.");
            this.checkBoxPolSoundIdOffset.UseVisualStyleBackColor = true;
            // 
            // checkBoxPanelSoundsDesign
            // 
            this.checkBoxPanelSoundsDesign.AutoSize = true;
            this.checkBoxPanelSoundsDesign.Location = new System.Drawing.Point(7, 103);
            this.checkBoxPanelSoundsDesign.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBoxPanelSoundsDesign.Name = "checkBoxPanelSoundsDesign";
            this.checkBoxPanelSoundsDesign.Size = new System.Drawing.Size(160, 19);
            this.checkBoxPanelSoundsDesign.TabIndex = 5;
            this.checkBoxPanelSoundsDesign.Text = "Right panel in sounds tab";
            this.toolTip1.SetToolTip(this.checkBoxPanelSoundsDesign, "Show right panel in Sounds tab.");
            this.checkBoxPanelSoundsDesign.UseVisualStyleBackColor = true;
            // 
            // checkBoxuseDiff
            // 
            this.checkBoxuseDiff.AutoSize = true;
            this.checkBoxuseDiff.Location = new System.Drawing.Point(7, 76);
            this.checkBoxuseDiff.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBoxuseDiff.Name = "checkBoxuseDiff";
            this.checkBoxuseDiff.Size = new System.Drawing.Size(119, 19);
            this.checkBoxuseDiff.TabIndex = 4;
            this.checkBoxuseDiff.Text = "Use Map diff Files";
            this.toolTip1.SetToolTip(this.checkBoxuseDiff, "Should map diff files be used");
            this.checkBoxuseDiff.UseVisualStyleBackColor = true;
            // 
            // checkBoxNewMapSize
            // 
            this.checkBoxNewMapSize.AutoSize = true;
            this.checkBoxNewMapSize.Location = new System.Drawing.Point(7, 50);
            this.checkBoxNewMapSize.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBoxNewMapSize.Name = "checkBoxNewMapSize";
            this.checkBoxNewMapSize.Size = new System.Drawing.Size(100, 19);
            this.checkBoxNewMapSize.TabIndex = 3;
            this.checkBoxNewMapSize.Text = "New Map Size";
            this.toolTip1.SetToolTip(this.checkBoxNewMapSize, "NewMapSize Felucca/Trammel width 7168?");
            this.checkBoxNewMapSize.UseVisualStyleBackColor = true;
            // 
            // buttonApply
            // 
            this.buttonApply.Location = new System.Drawing.Point(321, 440);
            this.buttonApply.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonApply.Name = "buttonApply";
            this.buttonApply.Size = new System.Drawing.Size(88, 27);
            this.buttonApply.TabIndex = 4;
            this.buttonApply.Text = "Apply";
            this.buttonApply.UseVisualStyleBackColor = true;
            this.buttonApply.Click += new System.EventHandler(this.OnClickApply);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 23);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 15);
            this.label2.TabIndex = 1;
            this.label2.Text = "map0 Name";
            this.toolTip1.SetToolTip(this.label2, "Defines the map name");
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 53);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(72, 15);
            this.label3.TabIndex = 3;
            this.label3.Text = "map1 Name";
            this.toolTip1.SetToolTip(this.label3, "Defines the map name");
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 83);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(72, 15);
            this.label4.TabIndex = 5;
            this.label4.Text = "map2 Name";
            this.toolTip1.SetToolTip(this.label4, "Defines the map name");
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 113);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(72, 15);
            this.label5.TabIndex = 7;
            this.label5.Text = "map3 Name";
            this.toolTip1.SetToolTip(this.label5, "Defines the map name");
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 142);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(72, 15);
            this.label6.TabIndex = 9;
            this.label6.Text = "map4 Name";
            this.toolTip1.SetToolTip(this.label6, "Defines the map name");
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(7, 211);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(33, 15);
            this.label7.TabIndex = 11;
            this.label7.Text = "Cmd";
            this.toolTip1.SetToolTip(this.label7, "Defines the cmd to send Client to loc");
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(7, 241);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(31, 15);
            this.label8.TabIndex = 13;
            this.label8.Text = "Args";
            this.toolTip1.SetToolTip(this.label8, "{1} = x, {2} = y, {3} = z, {4} = mapid, {5} = mapname");
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(7, 172);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(72, 15);
            this.label9.TabIndex = 15;
            this.label9.Text = "map5 Name";
            this.toolTip1.SetToolTip(this.label9, "Defines the map name");
            // 
            // FocusColorLabel
            // 
            this.FocusColorLabel.AutoSize = true;
            this.FocusColorLabel.Location = new System.Drawing.Point(14, 25);
            this.FocusColorLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.FocusColorLabel.Name = "FocusColorLabel";
            this.FocusColorLabel.Size = new System.Drawing.Size(59, 15);
            this.FocusColorLabel.TabIndex = 11;
            this.FocusColorLabel.Text = "Tile Focus";
            this.toolTip1.SetToolTip(this.FocusColorLabel, "ItemSize controls the size of images in items tab");
            // 
            // SelectedColorLabel
            // 
            this.SelectedColorLabel.AutoSize = true;
            this.SelectedColorLabel.Location = new System.Drawing.Point(14, 57);
            this.SelectedColorLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.SelectedColorLabel.Name = "SelectedColorLabel";
            this.SelectedColorLabel.Size = new System.Drawing.Size(76, 15);
            this.SelectedColorLabel.TabIndex = 14;
            this.SelectedColorLabel.Text = "Tile Selection";
            this.toolTip1.SetToolTip(this.SelectedColorLabel, "ItemSize controls the size of images in items tab");
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label9);
            this.groupBox3.Controls.Add(this.map5Nametext);
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Controls.Add(this.argstext);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.cmdtext);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.map4Nametext);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.map3Nametext);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.map2Nametext);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.map1Nametext);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.map0Nametext);
            this.groupBox3.Location = new System.Drawing.Point(282, 6);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox3.Size = new System.Drawing.Size(220, 273);
            this.groupBox3.TabIndex = 5;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Map";
            // 
            // map5Nametext
            // 
            this.map5Nametext.Location = new System.Drawing.Point(89, 168);
            this.map5Nametext.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.map5Nametext.Name = "map5Nametext";
            this.map5Nametext.Size = new System.Drawing.Size(116, 23);
            this.map5Nametext.TabIndex = 14;
            // 
            // argstext
            // 
            this.argstext.Location = new System.Drawing.Point(89, 238);
            this.argstext.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.argstext.Name = "argstext";
            this.argstext.Size = new System.Drawing.Size(116, 23);
            this.argstext.TabIndex = 12;
            // 
            // cmdtext
            // 
            this.cmdtext.Location = new System.Drawing.Point(89, 208);
            this.cmdtext.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cmdtext.Name = "cmdtext";
            this.cmdtext.Size = new System.Drawing.Size(116, 23);
            this.cmdtext.TabIndex = 10;
            // 
            // map4Nametext
            // 
            this.map4Nametext.Location = new System.Drawing.Point(89, 138);
            this.map4Nametext.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.map4Nametext.Name = "map4Nametext";
            this.map4Nametext.Size = new System.Drawing.Size(116, 23);
            this.map4Nametext.TabIndex = 8;
            // 
            // map3Nametext
            // 
            this.map3Nametext.Location = new System.Drawing.Point(89, 110);
            this.map3Nametext.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.map3Nametext.Name = "map3Nametext";
            this.map3Nametext.Size = new System.Drawing.Size(116, 23);
            this.map3Nametext.TabIndex = 6;
            // 
            // map2Nametext
            // 
            this.map2Nametext.Location = new System.Drawing.Point(89, 80);
            this.map2Nametext.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.map2Nametext.Name = "map2Nametext";
            this.map2Nametext.Size = new System.Drawing.Size(116, 23);
            this.map2Nametext.TabIndex = 4;
            // 
            // map1Nametext
            // 
            this.map1Nametext.Location = new System.Drawing.Point(89, 50);
            this.map1Nametext.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.map1Nametext.Name = "map1Nametext";
            this.map1Nametext.Size = new System.Drawing.Size(116, 23);
            this.map1Nametext.TabIndex = 2;
            // 
            // map0Nametext
            // 
            this.map0Nametext.Location = new System.Drawing.Point(89, 20);
            this.map0Nametext.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.map0Nametext.Name = "map0Nametext";
            this.map0Nametext.Size = new System.Drawing.Size(116, 23);
            this.map0Nametext.TabIndex = 0;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.button2);
            this.groupBox4.Controls.Add(this.textBoxOutputPath);
            this.groupBox4.Controls.Add(this.label10);
            this.groupBox4.Location = new System.Drawing.Point(16, 382);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox4.Size = new System.Drawing.Size(486, 51);
            this.groupBox4.TabIndex = 6;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Path";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(443, 16);
            this.button2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(28, 27);
            this.button2.TabIndex = 2;
            this.button2.Text = "...";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.OnClickBrowseOutputPath);
            // 
            // textBoxOutputPath
            // 
            this.textBoxOutputPath.Location = new System.Drawing.Point(89, 20);
            this.textBoxOutputPath.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textBoxOutputPath.Name = "textBoxOutputPath";
            this.textBoxOutputPath.Size = new System.Drawing.Size(347, 23);
            this.textBoxOutputPath.TabIndex = 1;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(7, 23);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(72, 15);
            this.label10.TabIndex = 0;
            this.label10.Text = "Output Path";
            // 
            // ColorsGroupBox
            // 
            this.ColorsGroupBox.Controls.Add(this.DefaultColorsButton);
            this.ColorsGroupBox.Controls.Add(this.SelectedColorLabel);
            this.ColorsGroupBox.Controls.Add(this.TileSelectionColorComboBox);
            this.ColorsGroupBox.Controls.Add(this.FocusColorLabel);
            this.ColorsGroupBox.Controls.Add(this.TileFocusColorComboBox);
            this.ColorsGroupBox.Location = new System.Drawing.Point(16, 286);
            this.ColorsGroupBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ColorsGroupBox.Name = "ColorsGroupBox";
            this.ColorsGroupBox.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ColorsGroupBox.Size = new System.Drawing.Size(486, 89);
            this.ColorsGroupBox.TabIndex = 7;
            this.ColorsGroupBox.TabStop = false;
            this.ColorsGroupBox.Text = "Colors";
            // 
            // DefaultColorsButton
            // 
            this.DefaultColorsButton.Location = new System.Drawing.Point(355, 51);
            this.DefaultColorsButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.DefaultColorsButton.Name = "DefaultColorsButton";
            this.DefaultColorsButton.Size = new System.Drawing.Size(117, 27);
            this.DefaultColorsButton.TabIndex = 15;
            this.DefaultColorsButton.Text = "Set default colors";
            this.DefaultColorsButton.UseVisualStyleBackColor = true;
            this.DefaultColorsButton.Click += new System.EventHandler(this.DefaultColorsButton_Click);
            // 
            // TileSelectionColorComboBox
            // 
            this.TileSelectionColorComboBox.FormattingEnabled = true;
            this.TileSelectionColorComboBox.Location = new System.Drawing.Point(104, 53);
            this.TileSelectionColorComboBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.TileSelectionColorComboBox.Name = "TileSelectionColorComboBox";
            this.TileSelectionColorComboBox.Size = new System.Drawing.Size(153, 23);
            this.TileSelectionColorComboBox.TabIndex = 13;
            // 
            // TileFocusColorComboBox
            // 
            this.TileFocusColorComboBox.FormattingEnabled = true;
            this.TileFocusColorComboBox.Location = new System.Drawing.Point(104, 22);
            this.TileFocusColorComboBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.TileFocusColorComboBox.Name = "TileFocusColorComboBox";
            this.TileFocusColorComboBox.Size = new System.Drawing.Size(153, 23);
            this.TileFocusColorComboBox.TabIndex = 9;
            // 
            // buttonClose
            // 
            this.buttonClose.Location = new System.Drawing.Point(415, 440);
            this.buttonClose.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(88, 27);
            this.buttonClose.TabIndex = 8;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.OnClickClose);
            // 
            // OptionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(519, 475);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.ColorsGroupBox);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.buttonApply);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OptionsForm";
            this.Text = "Options";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownItemSizeHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownItemSizeWidth)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ColorsGroupBox.ResumeLayout(false);
            this.ColorsGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox argstext;
        private System.Windows.Forms.Button buttonApply;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.CheckBox checkBoxCacheData;
        private System.Windows.Forms.CheckBox checkBoxItemClip;
        private System.Windows.Forms.CheckBox checkBoxNewMapSize;
        private System.Windows.Forms.CheckBox checkBoxuseDiff;
        private System.Windows.Forms.TextBox cmdtext;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox map0Nametext;
        private System.Windows.Forms.TextBox map1Nametext;
        private System.Windows.Forms.TextBox map2Nametext;
        private System.Windows.Forms.TextBox map3Nametext;
        private System.Windows.Forms.TextBox map4Nametext;
        private System.Windows.Forms.TextBox map5Nametext;
        private System.Windows.Forms.NumericUpDown numericUpDownItemSizeHeight;
        private System.Windows.Forms.NumericUpDown numericUpDownItemSizeWidth;
        private System.Windows.Forms.TextBox textBoxOutputPath;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.CheckBox checkBoxPanelSoundsDesign;
        private System.Windows.Forms.GroupBox ColorsGroupBox;
        private System.Windows.Forms.Button DefaultColorsButton;
        private System.Windows.Forms.Label SelectedColorLabel;
        private System.Windows.Forms.ComboBox TileSelectionColorComboBox;
        private System.Windows.Forms.Label FocusColorLabel;
        private System.Windows.Forms.ComboBox TileFocusColorComboBox;
        private System.Windows.Forms.CheckBox checkBoxPolSoundIdOffset;
        private System.Windows.Forms.Button buttonClose;
    }
}