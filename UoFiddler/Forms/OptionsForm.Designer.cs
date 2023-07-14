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
            components = new System.ComponentModel.Container();
            groupBox1 = new System.Windows.Forms.GroupBox();
            checkBoxOverrideBackgroundColorFromTile = new System.Windows.Forms.CheckBox();
            numericUpDownItemSizeHeight = new System.Windows.Forms.NumericUpDown();
            checkBoxItemClip = new System.Windows.Forms.CheckBox();
            label1 = new System.Windows.Forms.Label();
            numericUpDownItemSizeWidth = new System.Windows.Forms.NumericUpDown();
            checkBoxCacheData = new System.Windows.Forms.CheckBox();
            groupBox2 = new System.Windows.Forms.GroupBox();
            checkBoxPolSoundIdOffset = new System.Windows.Forms.CheckBox();
            checkBoxuseDiff = new System.Windows.Forms.CheckBox();
            checkBoxNewMapSize = new System.Windows.Forms.CheckBox();
            buttonApply = new System.Windows.Forms.Button();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            label7 = new System.Windows.Forms.Label();
            label8 = new System.Windows.Forms.Label();
            label9 = new System.Windows.Forms.Label();
            FocusColorLabel = new System.Windows.Forms.Label();
            SelectedColorLabel = new System.Windows.Forms.Label();
            groupBox3 = new System.Windows.Forms.GroupBox();
            map5Nametext = new System.Windows.Forms.TextBox();
            argstext = new System.Windows.Forms.TextBox();
            cmdtext = new System.Windows.Forms.TextBox();
            map4Nametext = new System.Windows.Forms.TextBox();
            map3Nametext = new System.Windows.Forms.TextBox();
            map2Nametext = new System.Windows.Forms.TextBox();
            map1Nametext = new System.Windows.Forms.TextBox();
            map0Nametext = new System.Windows.Forms.TextBox();
            groupBox4 = new System.Windows.Forms.GroupBox();
            button2 = new System.Windows.Forms.Button();
            textBoxOutputPath = new System.Windows.Forms.TextBox();
            label10 = new System.Windows.Forms.Label();
            ColorsGroupBox = new System.Windows.Forms.GroupBox();
            checkboxRemoveTileBorder = new System.Windows.Forms.CheckBox();
            RestoreDefaultsButton = new System.Windows.Forms.Button();
            TileSelectionColorComboBox = new System.Windows.Forms.ComboBox();
            TileFocusColorComboBox = new System.Windows.Forms.ComboBox();
            buttonClose = new System.Windows.Forms.Button();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDownItemSizeHeight).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownItemSizeWidth).BeginInit();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            groupBox4.SuspendLayout();
            ColorsGroupBox.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(checkBoxOverrideBackgroundColorFromTile);
            groupBox1.Controls.Add(numericUpDownItemSizeHeight);
            groupBox1.Controls.Add(checkBoxItemClip);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(numericUpDownItemSizeWidth);
            groupBox1.Location = new System.Drawing.Point(16, 164);
            groupBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox1.Size = new System.Drawing.Size(258, 130);
            groupBox1.TabIndex = 1;
            groupBox1.TabStop = false;
            groupBox1.Text = "Item Tab";
            // 
            // checkBoxOverrideBackgroundColorFromTile
            // 
            checkBoxOverrideBackgroundColorFromTile.Location = new System.Drawing.Point(10, 80);
            checkBoxOverrideBackgroundColorFromTile.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBoxOverrideBackgroundColorFromTile.Name = "checkBoxOverrideBackgroundColorFromTile";
            checkBoxOverrideBackgroundColorFromTile.Size = new System.Drawing.Size(225, 36);
            checkBoxOverrideBackgroundColorFromTile.TabIndex = 17;
            checkBoxOverrideBackgroundColorFromTile.Text = "Set background color same as tile background";
            checkBoxOverrideBackgroundColorFromTile.UseVisualStyleBackColor = true;
            // 
            // numericUpDownItemSizeHeight
            // 
            numericUpDownItemSizeHeight.Location = new System.Drawing.Point(155, 22);
            numericUpDownItemSizeHeight.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            numericUpDownItemSizeHeight.Maximum = new decimal(new int[] { 512, 0, 0, 0 });
            numericUpDownItemSizeHeight.Minimum = new decimal(new int[] { 48, 0, 0, 0 });
            numericUpDownItemSizeHeight.Name = "numericUpDownItemSizeHeight";
            numericUpDownItemSizeHeight.Size = new System.Drawing.Size(71, 23);
            numericUpDownItemSizeHeight.TabIndex = 3;
            toolTip1.SetToolTip(numericUpDownItemSizeHeight, "Height");
            numericUpDownItemSizeHeight.Value = new decimal(new int[] { 96, 0, 0, 0 });
            // 
            // checkBoxItemClip
            // 
            checkBoxItemClip.AutoSize = true;
            checkBoxItemClip.Location = new System.Drawing.Point(10, 54);
            checkBoxItemClip.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBoxItemClip.Name = "checkBoxItemClip";
            checkBoxItemClip.Size = new System.Drawing.Size(74, 19);
            checkBoxItemClip.TabIndex = 2;
            checkBoxItemClip.Text = "Item Clip";
            toolTip1.SetToolTip(checkBoxItemClip, "ItemClip images in items tab shrinked or clipped");
            checkBoxItemClip.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(7, 24);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(54, 15);
            label1.TabIndex = 1;
            label1.Text = "Item Size";
            toolTip1.SetToolTip(label1, "ItemSize controls the size of images in items tab");
            // 
            // numericUpDownItemSizeWidth
            // 
            numericUpDownItemSizeWidth.Location = new System.Drawing.Point(72, 22);
            numericUpDownItemSizeWidth.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            numericUpDownItemSizeWidth.Maximum = new decimal(new int[] { 512, 0, 0, 0 });
            numericUpDownItemSizeWidth.Minimum = new decimal(new int[] { 48, 0, 0, 0 });
            numericUpDownItemSizeWidth.Name = "numericUpDownItemSizeWidth";
            numericUpDownItemSizeWidth.Size = new System.Drawing.Size(76, 23);
            numericUpDownItemSizeWidth.TabIndex = 0;
            toolTip1.SetToolTip(numericUpDownItemSizeWidth, "Width");
            numericUpDownItemSizeWidth.Value = new decimal(new int[] { 96, 0, 0, 0 });
            // 
            // checkBoxCacheData
            // 
            checkBoxCacheData.AutoSize = true;
            checkBoxCacheData.Location = new System.Drawing.Point(7, 23);
            checkBoxCacheData.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBoxCacheData.Name = "checkBoxCacheData";
            checkBoxCacheData.Size = new System.Drawing.Size(86, 19);
            checkBoxCacheData.TabIndex = 2;
            checkBoxCacheData.Text = "Cache Data";
            toolTip1.SetToolTip(checkBoxCacheData, "CacheData should mul entries be cached for faster load");
            checkBoxCacheData.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(checkBoxPolSoundIdOffset);
            groupBox2.Controls.Add(checkBoxuseDiff);
            groupBox2.Controls.Add(checkBoxNewMapSize);
            groupBox2.Controls.Add(checkBoxCacheData);
            groupBox2.Location = new System.Drawing.Point(16, 6);
            groupBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox2.Size = new System.Drawing.Size(258, 157);
            groupBox2.TabIndex = 3;
            groupBox2.TabStop = false;
            groupBox2.Text = "Misc";
            // 
            // checkBoxPolSoundIdOffset
            // 
            checkBoxPolSoundIdOffset.AutoSize = true;
            checkBoxPolSoundIdOffset.Location = new System.Drawing.Point(7, 98);
            checkBoxPolSoundIdOffset.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBoxPolSoundIdOffset.Name = "checkBoxPolSoundIdOffset";
            checkBoxPolSoundIdOffset.Size = new System.Drawing.Size(217, 19);
            checkBoxPolSoundIdOffset.TabIndex = 7;
            checkBoxPolSoundIdOffset.Text = "Offset Sound Id by 1 (POL emulator)";
            toolTip1.SetToolTip(checkBoxPolSoundIdOffset, "UO Sounds are indexed from 0 but POL uses +1 offset.\r\nWhen this option is checked Sounds tab will display sound indexes starting from 1 instead of 0.\r\nThis option also affects the export sound list.");
            checkBoxPolSoundIdOffset.UseVisualStyleBackColor = true;
            // 
            // checkBoxuseDiff
            // 
            checkBoxuseDiff.AutoSize = true;
            checkBoxuseDiff.Location = new System.Drawing.Point(7, 73);
            checkBoxuseDiff.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBoxuseDiff.Name = "checkBoxuseDiff";
            checkBoxuseDiff.Size = new System.Drawing.Size(119, 19);
            checkBoxuseDiff.TabIndex = 4;
            checkBoxuseDiff.Text = "Use Map diff Files";
            toolTip1.SetToolTip(checkBoxuseDiff, "Should map diff files be used");
            checkBoxuseDiff.UseVisualStyleBackColor = true;
            // 
            // checkBoxNewMapSize
            // 
            checkBoxNewMapSize.AutoSize = true;
            checkBoxNewMapSize.Location = new System.Drawing.Point(7, 48);
            checkBoxNewMapSize.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBoxNewMapSize.Name = "checkBoxNewMapSize";
            checkBoxNewMapSize.Size = new System.Drawing.Size(100, 19);
            checkBoxNewMapSize.TabIndex = 3;
            checkBoxNewMapSize.Text = "New Map Size";
            toolTip1.SetToolTip(checkBoxNewMapSize, "NewMapSize Felucca/Trammel width 7168?");
            checkBoxNewMapSize.UseVisualStyleBackColor = true;
            // 
            // buttonApply
            // 
            buttonApply.Location = new System.Drawing.Point(321, 463);
            buttonApply.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonApply.Name = "buttonApply";
            buttonApply.Size = new System.Drawing.Size(88, 27);
            buttonApply.TabIndex = 4;
            buttonApply.Text = "Apply";
            buttonApply.UseVisualStyleBackColor = true;
            buttonApply.Click += OnClickApply;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(7, 23);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(72, 15);
            label2.TabIndex = 1;
            label2.Text = "map0 Name";
            toolTip1.SetToolTip(label2, "Defines the map name");
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(7, 53);
            label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(72, 15);
            label3.TabIndex = 3;
            label3.Text = "map1 Name";
            toolTip1.SetToolTip(label3, "Defines the map name");
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(7, 83);
            label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(72, 15);
            label4.TabIndex = 5;
            label4.Text = "map2 Name";
            toolTip1.SetToolTip(label4, "Defines the map name");
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(7, 113);
            label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(72, 15);
            label5.TabIndex = 7;
            label5.Text = "map3 Name";
            toolTip1.SetToolTip(label5, "Defines the map name");
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(7, 142);
            label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(72, 15);
            label6.TabIndex = 9;
            label6.Text = "map4 Name";
            toolTip1.SetToolTip(label6, "Defines the map name");
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(7, 211);
            label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(33, 15);
            label7.TabIndex = 11;
            label7.Text = "Cmd";
            toolTip1.SetToolTip(label7, "Defines the cmd to send Client to loc");
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new System.Drawing.Point(7, 241);
            label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(31, 15);
            label8.TabIndex = 13;
            label8.Text = "Args";
            toolTip1.SetToolTip(label8, "{1} = x, {2} = y, {3} = z, {4} = mapid, {5} = mapname");
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new System.Drawing.Point(7, 172);
            label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(72, 15);
            label9.TabIndex = 15;
            label9.Text = "map5 Name";
            toolTip1.SetToolTip(label9, "Defines the map name");
            // 
            // FocusColorLabel
            // 
            FocusColorLabel.AutoSize = true;
            FocusColorLabel.Location = new System.Drawing.Point(14, 25);
            FocusColorLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            FocusColorLabel.Name = "FocusColorLabel";
            FocusColorLabel.Size = new System.Drawing.Size(59, 15);
            FocusColorLabel.TabIndex = 11;
            FocusColorLabel.Text = "Tile Focus";
            toolTip1.SetToolTip(FocusColorLabel, "ItemSize controls the size of images in items tab");
            // 
            // SelectedColorLabel
            // 
            SelectedColorLabel.AutoSize = true;
            SelectedColorLabel.Location = new System.Drawing.Point(14, 57);
            SelectedColorLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            SelectedColorLabel.Name = "SelectedColorLabel";
            SelectedColorLabel.Size = new System.Drawing.Size(76, 15);
            SelectedColorLabel.TabIndex = 14;
            SelectedColorLabel.Text = "Tile Selection";
            toolTip1.SetToolTip(SelectedColorLabel, "ItemSize controls the size of images in items tab");
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(label9);
            groupBox3.Controls.Add(map5Nametext);
            groupBox3.Controls.Add(label8);
            groupBox3.Controls.Add(argstext);
            groupBox3.Controls.Add(label7);
            groupBox3.Controls.Add(cmdtext);
            groupBox3.Controls.Add(label6);
            groupBox3.Controls.Add(map4Nametext);
            groupBox3.Controls.Add(label5);
            groupBox3.Controls.Add(map3Nametext);
            groupBox3.Controls.Add(label4);
            groupBox3.Controls.Add(map2Nametext);
            groupBox3.Controls.Add(label3);
            groupBox3.Controls.Add(map1Nametext);
            groupBox3.Controls.Add(label2);
            groupBox3.Controls.Add(map0Nametext);
            groupBox3.Location = new System.Drawing.Point(282, 6);
            groupBox3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox3.Name = "groupBox3";
            groupBox3.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox3.Size = new System.Drawing.Size(220, 288);
            groupBox3.TabIndex = 5;
            groupBox3.TabStop = false;
            groupBox3.Text = "Map";
            // 
            // map5Nametext
            // 
            map5Nametext.Location = new System.Drawing.Point(89, 168);
            map5Nametext.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            map5Nametext.Name = "map5Nametext";
            map5Nametext.Size = new System.Drawing.Size(116, 23);
            map5Nametext.TabIndex = 14;
            // 
            // argstext
            // 
            argstext.Location = new System.Drawing.Point(89, 238);
            argstext.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            argstext.Name = "argstext";
            argstext.Size = new System.Drawing.Size(116, 23);
            argstext.TabIndex = 12;
            // 
            // cmdtext
            // 
            cmdtext.Location = new System.Drawing.Point(89, 208);
            cmdtext.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cmdtext.Name = "cmdtext";
            cmdtext.Size = new System.Drawing.Size(116, 23);
            cmdtext.TabIndex = 10;
            // 
            // map4Nametext
            // 
            map4Nametext.Location = new System.Drawing.Point(89, 138);
            map4Nametext.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            map4Nametext.Name = "map4Nametext";
            map4Nametext.Size = new System.Drawing.Size(116, 23);
            map4Nametext.TabIndex = 8;
            // 
            // map3Nametext
            // 
            map3Nametext.Location = new System.Drawing.Point(89, 110);
            map3Nametext.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            map3Nametext.Name = "map3Nametext";
            map3Nametext.Size = new System.Drawing.Size(116, 23);
            map3Nametext.TabIndex = 6;
            // 
            // map2Nametext
            // 
            map2Nametext.Location = new System.Drawing.Point(89, 80);
            map2Nametext.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            map2Nametext.Name = "map2Nametext";
            map2Nametext.Size = new System.Drawing.Size(116, 23);
            map2Nametext.TabIndex = 4;
            // 
            // map1Nametext
            // 
            map1Nametext.Location = new System.Drawing.Point(89, 50);
            map1Nametext.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            map1Nametext.Name = "map1Nametext";
            map1Nametext.Size = new System.Drawing.Size(116, 23);
            map1Nametext.TabIndex = 2;
            // 
            // map0Nametext
            // 
            map0Nametext.Location = new System.Drawing.Point(89, 20);
            map0Nametext.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            map0Nametext.Name = "map0Nametext";
            map0Nametext.Size = new System.Drawing.Size(116, 23);
            map0Nametext.TabIndex = 0;
            // 
            // groupBox4
            // 
            groupBox4.Controls.Add(button2);
            groupBox4.Controls.Add(textBoxOutputPath);
            groupBox4.Controls.Add(label10);
            groupBox4.Location = new System.Drawing.Point(16, 405);
            groupBox4.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox4.Name = "groupBox4";
            groupBox4.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox4.Size = new System.Drawing.Size(486, 51);
            groupBox4.TabIndex = 6;
            groupBox4.TabStop = false;
            groupBox4.Text = "Path";
            // 
            // button2
            // 
            button2.Location = new System.Drawing.Point(443, 16);
            button2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(28, 27);
            button2.TabIndex = 2;
            button2.Text = "...";
            button2.UseVisualStyleBackColor = true;
            button2.Click += OnClickBrowseOutputPath;
            // 
            // textBoxOutputPath
            // 
            textBoxOutputPath.Location = new System.Drawing.Point(89, 20);
            textBoxOutputPath.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            textBoxOutputPath.Name = "textBoxOutputPath";
            textBoxOutputPath.Size = new System.Drawing.Size(347, 23);
            textBoxOutputPath.TabIndex = 1;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new System.Drawing.Point(7, 23);
            label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label10.Name = "label10";
            label10.Size = new System.Drawing.Size(72, 15);
            label10.TabIndex = 0;
            label10.Text = "Output Path";
            // 
            // ColorsGroupBox
            // 
            ColorsGroupBox.Controls.Add(checkboxRemoveTileBorder);
            ColorsGroupBox.Controls.Add(RestoreDefaultsButton);
            ColorsGroupBox.Controls.Add(SelectedColorLabel);
            ColorsGroupBox.Controls.Add(TileSelectionColorComboBox);
            ColorsGroupBox.Controls.Add(FocusColorLabel);
            ColorsGroupBox.Controls.Add(TileFocusColorComboBox);
            ColorsGroupBox.Location = new System.Drawing.Point(16, 301);
            ColorsGroupBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ColorsGroupBox.Name = "ColorsGroupBox";
            ColorsGroupBox.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ColorsGroupBox.Size = new System.Drawing.Size(486, 97);
            ColorsGroupBox.TabIndex = 7;
            ColorsGroupBox.TabStop = false;
            ColorsGroupBox.Text = "Tile view settings";
            // 
            // checkboxRemoveTileBorder
            // 
            checkboxRemoveTileBorder.AutoSize = true;
            checkboxRemoveTileBorder.Location = new System.Drawing.Point(337, 27);
            checkboxRemoveTileBorder.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkboxRemoveTileBorder.Name = "checkboxRemoveTileBorder";
            checkboxRemoveTileBorder.Size = new System.Drawing.Size(126, 19);
            checkboxRemoveTileBorder.TabIndex = 17;
            checkboxRemoveTileBorder.Text = "Remove tile border";
            checkboxRemoveTileBorder.UseVisualStyleBackColor = true;
            // 
            // RestoreDefaultsButton
            // 
            RestoreDefaultsButton.Location = new System.Drawing.Point(355, 53);
            RestoreDefaultsButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            RestoreDefaultsButton.Name = "RestoreDefaultsButton";
            RestoreDefaultsButton.Size = new System.Drawing.Size(117, 27);
            RestoreDefaultsButton.TabIndex = 15;
            RestoreDefaultsButton.Text = "Restore defaults";
            RestoreDefaultsButton.UseVisualStyleBackColor = true;
            RestoreDefaultsButton.Click += RestoreDefaultsButton_Click;
            // 
            // TileSelectionColorComboBox
            // 
            TileSelectionColorComboBox.FormattingEnabled = true;
            TileSelectionColorComboBox.Location = new System.Drawing.Point(104, 53);
            TileSelectionColorComboBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            TileSelectionColorComboBox.Name = "TileSelectionColorComboBox";
            TileSelectionColorComboBox.Size = new System.Drawing.Size(153, 23);
            TileSelectionColorComboBox.TabIndex = 13;
            // 
            // TileFocusColorComboBox
            // 
            TileFocusColorComboBox.FormattingEnabled = true;
            TileFocusColorComboBox.Location = new System.Drawing.Point(104, 22);
            TileFocusColorComboBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            TileFocusColorComboBox.Name = "TileFocusColorComboBox";
            TileFocusColorComboBox.Size = new System.Drawing.Size(153, 23);
            TileFocusColorComboBox.TabIndex = 9;
            // 
            // buttonClose
            // 
            buttonClose.Location = new System.Drawing.Point(415, 463);
            buttonClose.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonClose.Name = "buttonClose";
            buttonClose.Size = new System.Drawing.Size(88, 27);
            buttonClose.TabIndex = 8;
            buttonClose.Text = "Close";
            buttonClose.UseVisualStyleBackColor = true;
            buttonClose.Click += OnClickClose;
            // 
            // OptionsForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(518, 501);
            Controls.Add(buttonClose);
            Controls.Add(ColorsGroupBox);
            Controls.Add(groupBox4);
            Controls.Add(groupBox3);
            Controls.Add(buttonApply);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            DoubleBuffered = true;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "OptionsForm";
            Text = "Options";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDownItemSizeHeight).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownItemSizeWidth).EndInit();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            ColorsGroupBox.ResumeLayout(false);
            ColorsGroupBox.PerformLayout();
            ResumeLayout(false);
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
        private System.Windows.Forms.GroupBox ColorsGroupBox;
        private System.Windows.Forms.Button RestoreDefaultsButton;
        private System.Windows.Forms.Label SelectedColorLabel;
        private System.Windows.Forms.ComboBox TileSelectionColorComboBox;
        private System.Windows.Forms.Label FocusColorLabel;
        private System.Windows.Forms.ComboBox TileFocusColorComboBox;
        private System.Windows.Forms.CheckBox checkBoxPolSoundIdOffset;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.CheckBox checkboxRemoveTileBorder;
        private System.Windows.Forms.CheckBox checkBoxOverrideBackgroundColorFromTile;
    }
}