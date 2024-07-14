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

using System.Drawing;
using System.Windows.Forms;

namespace UoFiddler.Controls.UserControls
{
    partial class TileDataControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private ToolTip toolTipComponent;

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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TileDataControl));
            toolTipComponent = new ToolTip(components);
            tabcontrol = new TabControl();
            tabPageItems = new TabPage();
            splitContainer1 = new SplitContainer();
            splitContainer2 = new SplitContainer();
            treeViewItem = new TreeView();
            ItemsContextMenuStrip = new ContextMenuStrip(components);
            selectInItemsToolStripMenuItem = new ToolStripMenuItem();
            selectRadarColorToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            selectInGumpsTabMaleToolStripMenuItem = new ToolStripMenuItem();
            selectInGumpsTabFemaleToolStripMenuItem = new ToolStripMenuItem();
            pictureBoxItem = new PictureBox();
            splitContainer3 = new SplitContainer();
            textBoxName = new TextBox();
            label9 = new Label();
            textBoxUnk2 = new TextBox();
            textBoxUnk1 = new TextBox();
            label10 = new Label();
            textBoxHeigth = new TextBox();
            label11 = new Label();
            textBoxValue = new TextBox();
            label6 = new Label();
            label7 = new Label();
            textBoxStackOff = new TextBox();
            textBoxHue = new TextBox();
            textBoxWeight = new TextBox();
            label8 = new Label();
            label5 = new Label();
            textBoxQuantity = new TextBox();
            textBoxQuality = new TextBox();
            label4 = new Label();
            label3 = new Label();
            label1 = new Label();
            textBoxUnk3 = new TextBox();
            label12 = new Label();
            textBoxAnim = new TextBox();
            label2 = new Label();
            checkedListBox1 = new CheckedListBox();
            namelabel = new Label();
            animlabel = new Label();
            weightlabel = new Label();
            layerlabel = new Label();
            quantitylabel = new Label();
            valuelabel = new Label();
            offsetlabel = new Label();
            huelabel = new Label();
            unknownlabel2 = new Label();
            miscdata = new Label();
            heightlabel = new Label();
            unknownlabel3 = new Label();
            tabPageLand = new TabPage();
            splitContainer5 = new SplitContainer();
            splitContainer6 = new SplitContainer();
            treeViewLand = new TreeView();
            LandTilesContextMenuStrip = new ContextMenuStrip(components);
            selectInLandtilesToolStripMenuItem = new ToolStripMenuItem();
            selToolStripMenuItem = new ToolStripMenuItem();
            pictureBoxLand = new PictureBox();
            label23 = new Label();
            textBoxNameLand = new TextBox();
            splitContainer7 = new SplitContainer();
            textBoxTexID = new TextBox();
            label24 = new Label();
            checkedListBox2 = new CheckedListBox();
            textIDlabal = new Label();
            namelandlabael = new Label();
            MainToolStrip = new ToolStrip();
            toolStripLabel1 = new ToolStripLabel();
            searchByIdToolStripTextBox = new ToolStripTextBox();
            toolStripLabel2 = new ToolStripLabel();
            searchByNameToolStripTextBox = new ToolStripTextBox();
            searchByNameToolStripButton = new ToolStripButton();
            toolStripSeparator5 = new ToolStripSeparator();
            toolStripDropDownButton1 = new ToolStripDropDownButton();
            memorySaveWarningToolStripMenuItem = new ToolStripMenuItem();
            saveDirectlyOnChangesToolStripMenuItem = new ToolStripMenuItem();
            setFilterToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator4 = new ToolStripSeparator();
            setTextureOnDoubleClickToolStripMenuItem = new ToolStripMenuItem();
            setTexturesToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            toolStripButton1 = new ToolStripButton();
            toolStripButton5 = new ToolStripButton();
            toolStripSeparator2 = new ToolStripSeparator();
            toolStripButton4 = new ToolStripButton();
            toolStripButton3 = new ToolStripButton();
            tabcontrol.SuspendLayout();
            tabPageItems.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            ItemsContextMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxItem).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitContainer3).BeginInit();
            splitContainer3.Panel1.SuspendLayout();
            splitContainer3.Panel2.SuspendLayout();
            splitContainer3.SuspendLayout();
            tabPageLand.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer5).BeginInit();
            splitContainer5.Panel1.SuspendLayout();
            splitContainer5.Panel2.SuspendLayout();
            splitContainer5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer6).BeginInit();
            splitContainer6.Panel1.SuspendLayout();
            splitContainer6.Panel2.SuspendLayout();
            splitContainer6.SuspendLayout();
            LandTilesContextMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxLand).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitContainer7).BeginInit();
            splitContainer7.Panel1.SuspendLayout();
            splitContainer7.Panel2.SuspendLayout();
            splitContainer7.SuspendLayout();
            MainToolStrip.SuspendLayout();
            SuspendLayout();
            // 
            // tabcontrol
            // 
            tabcontrol.Controls.Add(tabPageItems);
            tabcontrol.Controls.Add(tabPageLand);
            tabcontrol.Dock = DockStyle.Fill;
            tabcontrol.Location = new Point(0, 25);
            tabcontrol.Margin = new Padding(4, 3, 4, 3);
            tabcontrol.Name = "tabcontrol";
            tabcontrol.SelectedIndex = 0;
            tabcontrol.Size = new Size(770, 375);
            tabcontrol.TabIndex = 0;
            // 
            // tabPageItems
            // 
            tabPageItems.Controls.Add(splitContainer1);
            tabPageItems.Location = new Point(4, 24);
            tabPageItems.Margin = new Padding(4, 3, 4, 3);
            tabPageItems.Name = "tabPageItems";
            tabPageItems.Padding = new Padding(4, 3, 4, 3);
            tabPageItems.Size = new Size(762, 347);
            tabPageItems.TabIndex = 0;
            tabPageItems.Text = "Items";
            tabPageItems.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(4, 3);
            splitContainer1.Margin = new Padding(4, 3, 4, 3);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(splitContainer3);
            splitContainer1.Size = new Size(754, 341);
            splitContainer1.SplitterDistance = 245;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 0;
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer2.Location = new Point(0, 0);
            splitContainer2.Margin = new Padding(4, 3, 4, 3);
            splitContainer2.Name = "splitContainer2";
            splitContainer2.Orientation = Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(treeViewItem);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(pictureBoxItem);
            splitContainer2.Size = new Size(245, 341);
            splitContainer2.SplitterDistance = 207;
            splitContainer2.SplitterWidth = 5;
            splitContainer2.TabIndex = 0;
            // 
            // treeViewItem
            // 
            treeViewItem.ContextMenuStrip = ItemsContextMenuStrip;
            treeViewItem.Dock = DockStyle.Fill;
            treeViewItem.HideSelection = false;
            treeViewItem.Location = new Point(0, 0);
            treeViewItem.Margin = new Padding(4, 3, 4, 3);
            treeViewItem.Name = "treeViewItem";
            treeViewItem.Size = new Size(245, 207);
            treeViewItem.TabIndex = 0;
            treeViewItem.BeforeExpand += OnItemDataNodeExpanded;
            treeViewItem.AfterSelect += AfterSelectTreeViewItem;
            // 
            // ItemsContextMenuStrip
            // 
            ItemsContextMenuStrip.Items.AddRange(new ToolStripItem[] { selectInItemsToolStripMenuItem, selectRadarColorToolStripMenuItem, toolStripSeparator3, selectInGumpsTabMaleToolStripMenuItem, selectInGumpsTabFemaleToolStripMenuItem });
            ItemsContextMenuStrip.Name = "contextMenuStrip1";
            ItemsContextMenuStrip.Size = new Size(201, 98);
            ItemsContextMenuStrip.Opening += ItemsContextMenuStrip_Opening;
            // 
            // selectInItemsToolStripMenuItem
            // 
            selectInItemsToolStripMenuItem.Name = "selectInItemsToolStripMenuItem";
            selectInItemsToolStripMenuItem.Size = new Size(200, 22);
            selectInItemsToolStripMenuItem.Text = "Select In Items tab";
            selectInItemsToolStripMenuItem.Click += OnClickSelectItem;
            // 
            // selectRadarColorToolStripMenuItem
            // 
            selectRadarColorToolStripMenuItem.Name = "selectRadarColorToolStripMenuItem";
            selectRadarColorToolStripMenuItem.Size = new Size(200, 22);
            selectRadarColorToolStripMenuItem.Text = "Select In RadarColor tab";
            selectRadarColorToolStripMenuItem.Click += OnClickSelectRadarItem;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(197, 6);
            // 
            // selectInGumpsTabMaleToolStripMenuItem
            // 
            selectInGumpsTabMaleToolStripMenuItem.Name = "selectInGumpsTabMaleToolStripMenuItem";
            selectInGumpsTabMaleToolStripMenuItem.Size = new Size(200, 22);
            selectInGumpsTabMaleToolStripMenuItem.Text = "Select in Gumps (M)";
            selectInGumpsTabMaleToolStripMenuItem.Click += SelectInGumpsTabMaleToolStripMenuItem_Click;
            // 
            // selectInGumpsTabFemaleToolStripMenuItem
            // 
            selectInGumpsTabFemaleToolStripMenuItem.Name = "selectInGumpsTabFemaleToolStripMenuItem";
            selectInGumpsTabFemaleToolStripMenuItem.Size = new Size(200, 22);
            selectInGumpsTabFemaleToolStripMenuItem.Text = "Select in Gumps (F)";
            selectInGumpsTabFemaleToolStripMenuItem.Click += SelectInGumpsTabFemaleToolStripMenuItem_Click;
            // 
            // pictureBoxItem
            // 
            pictureBoxItem.Dock = DockStyle.Fill;
            pictureBoxItem.Location = new Point(0, 0);
            pictureBoxItem.Margin = new Padding(4, 3, 4, 3);
            pictureBoxItem.Name = "pictureBoxItem";
            pictureBoxItem.Size = new Size(245, 129);
            pictureBoxItem.TabIndex = 0;
            pictureBoxItem.TabStop = false;
            // 
            // splitContainer3
            // 
            splitContainer3.Dock = DockStyle.Fill;
            splitContainer3.FixedPanel = FixedPanel.Panel1;
            splitContainer3.IsSplitterFixed = true;
            splitContainer3.Location = new Point(0, 0);
            splitContainer3.Margin = new Padding(4, 3, 4, 3);
            splitContainer3.Name = "splitContainer3";
            splitContainer3.Orientation = Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            splitContainer3.Panel1.Controls.Add(textBoxName);
            splitContainer3.Panel1.Controls.Add(label9);
            splitContainer3.Panel1.Controls.Add(textBoxUnk2);
            splitContainer3.Panel1.Controls.Add(textBoxUnk1);
            splitContainer3.Panel1.Controls.Add(label10);
            splitContainer3.Panel1.Controls.Add(textBoxHeigth);
            splitContainer3.Panel1.Controls.Add(label11);
            splitContainer3.Panel1.Controls.Add(textBoxValue);
            splitContainer3.Panel1.Controls.Add(label6);
            splitContainer3.Panel1.Controls.Add(label7);
            splitContainer3.Panel1.Controls.Add(textBoxStackOff);
            splitContainer3.Panel1.Controls.Add(textBoxHue);
            splitContainer3.Panel1.Controls.Add(textBoxWeight);
            splitContainer3.Panel1.Controls.Add(label8);
            splitContainer3.Panel1.Controls.Add(label5);
            splitContainer3.Panel1.Controls.Add(textBoxQuantity);
            splitContainer3.Panel1.Controls.Add(textBoxQuality);
            splitContainer3.Panel1.Controls.Add(label4);
            splitContainer3.Panel1.Controls.Add(label3);
            splitContainer3.Panel1.Controls.Add(label1);
            splitContainer3.Panel1.Controls.Add(textBoxUnk3);
            splitContainer3.Panel1.Controls.Add(label12);
            splitContainer3.Panel1.Controls.Add(textBoxAnim);
            splitContainer3.Panel1.Controls.Add(label2);
            splitContainer3.Panel1.Controls.Add(namelabel);
            splitContainer3.Panel1.Controls.Add(animlabel);
            splitContainer3.Panel1.Controls.Add(weightlabel);
            splitContainer3.Panel1.Controls.Add(layerlabel);
            splitContainer3.Panel1.Controls.Add(quantitylabel);
            splitContainer3.Panel1.Controls.Add(valuelabel);
            splitContainer3.Panel1.Controls.Add(offsetlabel);
            splitContainer3.Panel1.Controls.Add(huelabel);
            splitContainer3.Panel1.Controls.Add(unknownlabel2);
            splitContainer3.Panel1.Controls.Add(miscdata);
            splitContainer3.Panel1.Controls.Add(heightlabel);
            splitContainer3.Panel1.Controls.Add(unknownlabel3);
            splitContainer3.Panel1.Paint += splitContainer3_Panel1_Paint;
            // 
            // splitContainer3.Panel2
            // 
            splitContainer3.Panel2.Controls.Add(checkedListBox1);
            splitContainer3.Size = new Size(504, 341);
            splitContainer3.SplitterDistance = 157;
            splitContainer3.SplitterWidth = 2;
            splitContainer3.TabIndex = 25;
            // 
            // textBoxName
            // 
            textBoxName.Location = new Point(51, 3);
            textBoxName.Margin = new Padding(4, 3, 4, 3);
            textBoxName.MaxLength = 20;
            textBoxName.Name = "textBoxName";
            textBoxName.Size = new Size(157, 23);
            textBoxName.TabIndex = 0;
            textBoxName.TextChanged += OnTextChangedItemName;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(264, 97);
            label9.Margin = new Padding(4, 0, 4, 0);
            label9.Name = "label9";
            label9.Size = new Size(34, 15);
            label9.TabIndex = 21;
            label9.Text = "Unk2";
            // 
            // textBoxUnk2
            // 
            textBoxUnk2.Location = new Point(325, 93);
            textBoxUnk2.Margin = new Padding(4, 3, 4, 3);
            textBoxUnk2.Name = "textBoxUnk2";
            textBoxUnk2.Size = new Size(59, 23);
            textBoxUnk2.TabIndex = 20;
            textBoxUnk2.TextChanged += OnTextChangedItemUnk2;
            // 
            // textBoxUnk1
            // 
            textBoxUnk1.Location = new Point(188, 93);
            textBoxUnk1.Margin = new Padding(4, 3, 4, 3);
            textBoxUnk1.Name = "textBoxUnk1";
            textBoxUnk1.Size = new Size(59, 23);
            textBoxUnk1.TabIndex = 18;
            textBoxUnk1.TextChanged += OnTextChangedItemMiscData;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(128, 97);
            label10.Margin = new Padding(4, 0, 4, 0);
            label10.Name = "label10";
            label10.Size = new Size(56, 15);
            label10.TabIndex = 19;
            label10.Text = "MiscData";
            // 
            // textBoxHeigth
            // 
            textBoxHeigth.Location = new Point(52, 93);
            textBoxHeigth.Margin = new Padding(4, 3, 4, 3);
            textBoxHeigth.Name = "textBoxHeigth";
            textBoxHeigth.Size = new Size(59, 23);
            textBoxHeigth.TabIndex = 16;
            textBoxHeigth.TextChanged += OnTextChangedItemHeight;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(5, 97);
            label11.Margin = new Padding(4, 0, 4, 0);
            label11.Name = "label11";
            label11.Size = new Size(43, 15);
            label11.TabIndex = 17;
            label11.Text = "Height";
            // 
            // textBoxValue
            // 
            textBoxValue.Location = new Point(325, 63);
            textBoxValue.Margin = new Padding(4, 3, 4, 3);
            textBoxValue.Name = "textBoxValue";
            textBoxValue.Size = new Size(59, 23);
            textBoxValue.TabIndex = 14;
            textBoxValue.TextChanged += OnTextChangedItemValue;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(264, 67);
            label6.Margin = new Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new Size(35, 15);
            label6.TabIndex = 15;
            label6.Text = "Value";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(128, 67);
            label7.Margin = new Padding(4, 0, 4, 0);
            label7.Name = "label7";
            label7.Size = new Size(52, 15);
            label7.TabIndex = 13;
            label7.Text = "StackOff";
            // 
            // textBoxStackOff
            // 
            textBoxStackOff.Location = new Point(188, 63);
            textBoxStackOff.Margin = new Padding(4, 3, 4, 3);
            textBoxStackOff.Name = "textBoxStackOff";
            textBoxStackOff.Size = new Size(59, 23);
            textBoxStackOff.TabIndex = 12;
            textBoxStackOff.TextChanged += OnTextChangedItemStackOff;
            // 
            // textBoxHue
            // 
            textBoxHue.Location = new Point(52, 63);
            textBoxHue.Margin = new Padding(4, 3, 4, 3);
            textBoxHue.Name = "textBoxHue";
            textBoxHue.Size = new Size(59, 23);
            textBoxHue.TabIndex = 10;
            textBoxHue.TextChanged += OnTextChangedItemHue;
            // 
            // textBoxWeight
            // 
            textBoxWeight.Location = new Point(52, 33);
            textBoxWeight.Margin = new Padding(4, 3, 4, 3);
            textBoxWeight.Name = "textBoxWeight";
            textBoxWeight.Size = new Size(59, 23);
            textBoxWeight.TabIndex = 4;
            textBoxWeight.TextChanged += OnTextChangedItemWeight;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(5, 67);
            label8.Margin = new Padding(4, 0, 4, 0);
            label8.Name = "label8";
            label8.Size = new Size(29, 15);
            label8.TabIndex = 11;
            label8.Text = "Hue";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(264, 37);
            label5.Margin = new Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new Size(53, 15);
            label5.TabIndex = 9;
            label5.Text = "Quantity";
            // 
            // textBoxQuantity
            // 
            textBoxQuantity.Location = new Point(325, 33);
            textBoxQuantity.Margin = new Padding(4, 3, 4, 3);
            textBoxQuantity.Name = "textBoxQuantity";
            textBoxQuantity.Size = new Size(59, 23);
            textBoxQuantity.TabIndex = 8;
            textBoxQuantity.TextChanged += OnTextChangedItemQuantity;
            // 
            // textBoxQuality
            // 
            textBoxQuality.Location = new Point(188, 33);
            textBoxQuality.Margin = new Padding(4, 3, 4, 3);
            textBoxQuality.Name = "textBoxQuality";
            textBoxQuality.Size = new Size(59, 23);
            textBoxQuality.TabIndex = 6;
            textBoxQuality.TextChanged += OnTextChangedItemQuality;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(128, 37);
            label4.Margin = new Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new Size(35, 15);
            label4.TabIndex = 7;
            label4.Text = "Layer";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(5, 37);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(45, 15);
            label3.TabIndex = 5;
            label3.Text = "Weight";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(4, 7);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(39, 15);
            label1.TabIndex = 1;
            label1.Text = "Name";
            // 
            // textBoxUnk3
            // 
            textBoxUnk3.Location = new Point(52, 123);
            textBoxUnk3.Margin = new Padding(4, 3, 4, 3);
            textBoxUnk3.Name = "textBoxUnk3";
            textBoxUnk3.Size = new Size(59, 23);
            textBoxUnk3.TabIndex = 22;
            textBoxUnk3.TextChanged += OnTextChangedItemUnk3;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(5, 127);
            label12.Margin = new Padding(4, 0, 4, 0);
            label12.Name = "label12";
            label12.Size = new Size(34, 15);
            label12.TabIndex = 23;
            label12.Text = "Unk3";
            // 
            // textBoxAnim
            // 
            textBoxAnim.Location = new Point(268, 4);
            textBoxAnim.Margin = new Padding(4, 3, 4, 3);
            textBoxAnim.Name = "textBoxAnim";
            textBoxAnim.Size = new Size(116, 23);
            textBoxAnim.TabIndex = 2;
            textBoxAnim.TextChanged += OnTextChangedItemAnim;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(233, 7);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(36, 15);
            label2.TabIndex = 3;
            label2.Text = "Anim";
            // 
            // checkedListBox1
            // 
            checkedListBox1.Dock = DockStyle.Fill;
            checkedListBox1.FormattingEnabled = true;
            checkedListBox1.Location = new Point(0, 0);
            checkedListBox1.Margin = new Padding(4, 3, 4, 3);
            checkedListBox1.MultiColumn = true;
            checkedListBox1.Name = "checkedListBox1";
            checkedListBox1.Size = new Size(504, 182);
            checkedListBox1.TabIndex = 0;
            checkedListBox1.ItemCheck += OnFlagItemCheckItems;
            // 
            // label1Info
            // 
            namelabel.Location = new Point(0, 0);
            namelabel.Name = "label1Info";
            namelabel.Size = new Size(100, 23);
            namelabel.TabIndex = 24;
            // 
            // label2Info
            // 
            animlabel.Location = new Point(0, 0);
            animlabel.Name = "label2Info";
            animlabel.Size = new Size(100, 23);
            animlabel.TabIndex = 25;
            // 
            // label3Info
            // 
            weightlabel.Location = new Point(0, 0);
            weightlabel.Name = "label3Info";
            weightlabel.Size = new Size(100, 23);
            weightlabel.TabIndex = 26;
            // 
            // label4Info
            // 
            layerlabel.Location = new Point(0, 0);
            layerlabel.Name = "label4Info";
            layerlabel.Size = new Size(100, 23);
            layerlabel.TabIndex = 27;
            // 
            // label5Info
            // 
            quantitylabel.Location = new Point(0, 0);
            quantitylabel.Name = "label5Info";
            quantitylabel.Size = new Size(100, 23);
            quantitylabel.TabIndex = 28;
            // 
            // label6Info
            // 
            valuelabel.Location = new Point(0, 0);
            valuelabel.Name = "label6Info";
            valuelabel.Size = new Size(100, 23);
            valuelabel.TabIndex = 29;
            // 
            // label7Info
            // 
            offsetlabel.Location = new Point(0, 0);
            offsetlabel.Name = "label7Info";
            offsetlabel.Size = new Size(100, 23);
            offsetlabel.TabIndex = 30;
            // 
            // label8Info
            // 
            huelabel.Location = new Point(0, 0);
            huelabel.Name = "label8Info";
            huelabel.Size = new Size(100, 23);
            huelabel.TabIndex = 31;
            // 
            // label9Info
            // 
            unknownlabel2.Location = new Point(0, 0);
            unknownlabel2.Name = "label9Info";
            unknownlabel2.Size = new Size(100, 23);
            unknownlabel2.TabIndex = 32;
            // 
            // label10Info
            // 
            miscdata.Location = new Point(0, 0);
            miscdata.Name = "label10Info";
            miscdata.Size = new Size(100, 23);
            miscdata.TabIndex = 33;
            // 
            // label11Info
            // 
            heightlabel.Location = new Point(0, 0);
            heightlabel.Name = "label11Info";
            heightlabel.Size = new Size(100, 23);
            heightlabel.TabIndex = 34;
            // 
            // label12Info
            // 
            unknownlabel3.Location = new Point(0, 0);
            unknownlabel3.Name = "label12Info";
            unknownlabel3.Size = new Size(100, 23);
            unknownlabel3.TabIndex = 35;
            // 
            // tabPageLand
            // 
            tabPageLand.Controls.Add(splitContainer5);
            tabPageLand.Location = new Point(4, 24);
            tabPageLand.Margin = new Padding(4, 3, 4, 3);
            tabPageLand.Name = "tabPageLand";
            tabPageLand.Padding = new Padding(4, 3, 4, 3);
            tabPageLand.Size = new Size(762, 347);
            tabPageLand.TabIndex = 1;
            tabPageLand.Text = "Land Tiles";
            tabPageLand.UseVisualStyleBackColor = true;
            // 
            // splitContainer5
            // 
            splitContainer5.Dock = DockStyle.Fill;
            splitContainer5.Location = new Point(4, 3);
            splitContainer5.Margin = new Padding(4, 3, 4, 3);
            splitContainer5.Name = "splitContainer5";
            // 
            // splitContainer5.Panel1
            // 
            splitContainer5.Panel1.Controls.Add(splitContainer6);
            // 
            // splitContainer5.Panel2
            // 
            splitContainer5.Panel2.Controls.Add(label23);
            splitContainer5.Panel2.Controls.Add(textBoxNameLand);
            splitContainer5.Panel2.Controls.Add(splitContainer7);
            splitContainer5.Panel2.Controls.Add(namelandlabael);
            splitContainer5.Size = new Size(754, 341);
            splitContainer5.SplitterDistance = 245;
            splitContainer5.SplitterWidth = 5;
            splitContainer5.TabIndex = 1;
            // 
            // splitContainer6
            // 
            splitContainer6.Dock = DockStyle.Fill;
            splitContainer6.Location = new Point(0, 0);
            splitContainer6.Margin = new Padding(4, 3, 4, 3);
            splitContainer6.Name = "splitContainer6";
            splitContainer6.Orientation = Orientation.Horizontal;
            // 
            // splitContainer6.Panel1
            // 
            splitContainer6.Panel1.Controls.Add(treeViewLand);
            // 
            // splitContainer6.Panel2
            // 
            splitContainer6.Panel2.Controls.Add(pictureBoxLand);
            splitContainer6.Size = new Size(245, 341);
            splitContainer6.SplitterDistance = 205;
            splitContainer6.SplitterWidth = 5;
            splitContainer6.TabIndex = 0;
            // 
            // treeViewLand
            // 
            treeViewLand.ContextMenuStrip = LandTilesContextMenuStrip;
            treeViewLand.Dock = DockStyle.Fill;
            treeViewLand.HideSelection = false;
            treeViewLand.Location = new Point(0, 0);
            treeViewLand.Margin = new Padding(4, 3, 4, 3);
            treeViewLand.Name = "treeViewLand";
            treeViewLand.Size = new Size(245, 205);
            treeViewLand.TabIndex = 0;
            treeViewLand.AfterSelect += AfterSelectTreeViewLand;
            // 
            // LandTilesContextMenuStrip
            // 
            LandTilesContextMenuStrip.Items.AddRange(new ToolStripItem[] { selectInLandtilesToolStripMenuItem, selToolStripMenuItem });
            LandTilesContextMenuStrip.Name = "contextMenuStrip2";
            LandTilesContextMenuStrip.Size = new Size(201, 48);
            // 
            // selectInLandtilesToolStripMenuItem
            // 
            selectInLandtilesToolStripMenuItem.Name = "selectInLandtilesToolStripMenuItem";
            selectInLandtilesToolStripMenuItem.Size = new Size(200, 22);
            selectInLandtilesToolStripMenuItem.Text = "Select In Land Tiles tab";
            selectInLandtilesToolStripMenuItem.Click += OnClickSelectInLandTiles;
            // 
            // selToolStripMenuItem
            // 
            selToolStripMenuItem.Name = "selToolStripMenuItem";
            selToolStripMenuItem.Size = new Size(200, 22);
            selToolStripMenuItem.Text = "Select In RadarColor tab";
            selToolStripMenuItem.Click += OnClickSelectRadarLand;
            // 
            // pictureBoxLand
            // 
            pictureBoxLand.Dock = DockStyle.Fill;
            pictureBoxLand.Location = new Point(0, 0);
            pictureBoxLand.Margin = new Padding(4, 3, 4, 3);
            pictureBoxLand.Name = "pictureBoxLand";
            pictureBoxLand.Size = new Size(245, 131);
            pictureBoxLand.TabIndex = 0;
            pictureBoxLand.TabStop = false;
            // 
            // label23
            // 
            label23.AutoSize = true;
            label23.Location = new Point(4, 7);
            label23.Margin = new Padding(4, 0, 4, 0);
            label23.Name = "label23";
            label23.Size = new Size(39, 15);
            label23.TabIndex = 1;
            label23.Text = "Name";
            // 
            // textBoxNameLand
            // 
            textBoxNameLand.Location = new Point(51, 3);
            textBoxNameLand.Margin = new Padding(4, 3, 4, 3);
            textBoxNameLand.MaxLength = 20;
            textBoxNameLand.Name = "textBoxNameLand";
            textBoxNameLand.Size = new Size(143, 23);
            textBoxNameLand.TabIndex = 0;
            textBoxNameLand.TextChanged += OnTextChangedLandName;
            // 
            // splitContainer7
            // 
            splitContainer7.Dock = DockStyle.Fill;
            splitContainer7.FixedPanel = FixedPanel.Panel1;
            splitContainer7.IsSplitterFixed = true;
            splitContainer7.Location = new Point(0, 0);
            splitContainer7.Margin = new Padding(4, 3, 4, 3);
            splitContainer7.Name = "splitContainer7";
            splitContainer7.Orientation = Orientation.Horizontal;
            // 
            // splitContainer7.Panel1
            // 
            splitContainer7.Panel1.Controls.Add(textBoxTexID);
            splitContainer7.Panel1.Controls.Add(label24);
            splitContainer7.Panel1.Controls.Add(textIDlabal);
            // 
            // splitContainer7.Panel2
            // 
            splitContainer7.Panel2.Controls.Add(checkedListBox2);
            splitContainer7.Size = new Size(504, 341);
            splitContainer7.SplitterDistance = 27;
            splitContainer7.SplitterWidth = 2;
            splitContainer7.TabIndex = 25;
            // 
            // textBoxTexID
            // 
            textBoxTexID.Location = new Point(251, 3);
            textBoxTexID.Margin = new Padding(4, 3, 4, 3);
            textBoxTexID.Name = "textBoxTexID";
            textBoxTexID.Size = new Size(67, 23);
            textBoxTexID.TabIndex = 2;
            textBoxTexID.TextChanged += OnTextChangedLandTexID;
            textBoxTexID.DoubleClick += TextBoxTexID_DoubleClick;
            // 
            // label24
            // 
            label24.AutoSize = true;
            label24.Location = new Point(220, 7);
            label24.Margin = new Padding(4, 0, 4, 0);
            label24.Name = "label24";
            label24.Size = new Size(35, 15);
            label24.TabIndex = 3;
            label24.Text = "TexID";
            // 
            // checkedListBox2
            // 
            checkedListBox2.Dock = DockStyle.Fill;
            checkedListBox2.FormattingEnabled = true;
            checkedListBox2.Location = new Point(0, 0);
            checkedListBox2.Margin = new Padding(4, 3, 4, 3);
            checkedListBox2.MultiColumn = true;
            checkedListBox2.Name = "checkedListBox2";
            checkedListBox2.Size = new Size(504, 312);
            checkedListBox2.TabIndex = 0;
            checkedListBox2.ItemCheck += OnFlagItemCheckLandTiles;
            // 
            // label24Info
            // 
            textIDlabal.Location = new Point(0, 0);
            textIDlabal.Name = "label24Info";
            textIDlabal.Size = new Size(100, 23);
            textIDlabal.TabIndex = 4;
            // 
            // label23Info
            // 
            namelandlabael.Location = new Point(0, 0);
            namelandlabael.Name = "label23Info";
            namelandlabael.Size = new Size(100, 23);
            namelandlabael.TabIndex = 26;
            // 
            // MainToolStrip
            // 
            MainToolStrip.GripStyle = ToolStripGripStyle.Hidden;
            MainToolStrip.Items.AddRange(new ToolStripItem[] { toolStripLabel1, searchByIdToolStripTextBox, toolStripLabel2, searchByNameToolStripTextBox, searchByNameToolStripButton, toolStripSeparator5, toolStripDropDownButton1, toolStripSeparator1, toolStripButton1, toolStripButton5, toolStripSeparator2, toolStripButton4, toolStripButton3 });
            MainToolStrip.Location = new Point(0, 0);
            MainToolStrip.Name = "MainToolStrip";
            MainToolStrip.RenderMode = ToolStripRenderMode.System;
            MainToolStrip.Size = new Size(770, 25);
            MainToolStrip.TabIndex = 1;
            MainToolStrip.Text = "toolStrip1";
            // 
            // toolStripLabel1
            // 
            toolStripLabel1.Name = "toolStripLabel1";
            toolStripLabel1.Size = new Size(39, 22);
            toolStripLabel1.Text = "Index:";
            // 
            // searchByIdToolStripTextBox
            // 
            searchByIdToolStripTextBox.Name = "searchByIdToolStripTextBox";
            searchByIdToolStripTextBox.Size = new Size(100, 25);
            searchByIdToolStripTextBox.KeyUp += SearchByIdToolStripTextBox_KeyUp;
            // 
            // toolStripLabel2
            // 
            toolStripLabel2.Name = "toolStripLabel2";
            toolStripLabel2.Size = new Size(42, 22);
            toolStripLabel2.Text = "Name:";
            // 
            // searchByNameToolStripTextBox
            // 
            searchByNameToolStripTextBox.Name = "searchByNameToolStripTextBox";
            searchByNameToolStripTextBox.Size = new Size(100, 25);
            searchByNameToolStripTextBox.KeyUp += SearchByNameToolStripTextBox_KeyUp;
            // 
            // searchByNameToolStripButton
            // 
            searchByNameToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
            searchByNameToolStripButton.Image = (Image)resources.GetObject("searchByNameToolStripButton.Image");
            searchByNameToolStripButton.ImageTransparentColor = Color.Magenta;
            searchByNameToolStripButton.Name = "searchByNameToolStripButton";
            searchByNameToolStripButton.Size = new Size(60, 22);
            searchByNameToolStripButton.Text = "Find next";
            searchByNameToolStripButton.Click += SearchByNameToolStripButton_Click;
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new Size(6, 25);
            // 
            // toolStripDropDownButton1
            // 
            toolStripDropDownButton1.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripDropDownButton1.DropDownItems.AddRange(new ToolStripItem[] { memorySaveWarningToolStripMenuItem, saveDirectlyOnChangesToolStripMenuItem, setFilterToolStripMenuItem, toolStripSeparator4, setTextureOnDoubleClickToolStripMenuItem, setTexturesToolStripMenuItem });
            toolStripDropDownButton1.ImageTransparentColor = Color.Magenta;
            toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            toolStripDropDownButton1.Size = new Size(45, 22);
            toolStripDropDownButton1.Text = "Misc";
            // 
            // memorySaveWarningToolStripMenuItem
            // 
            memorySaveWarningToolStripMenuItem.Checked = true;
            memorySaveWarningToolStripMenuItem.CheckOnClick = true;
            memorySaveWarningToolStripMenuItem.CheckState = CheckState.Checked;
            memorySaveWarningToolStripMenuItem.Name = "memorySaveWarningToolStripMenuItem";
            memorySaveWarningToolStripMenuItem.Size = new Size(205, 22);
            memorySaveWarningToolStripMenuItem.Text = "Memory save warning";
            // 
            // saveDirectlyOnChangesToolStripMenuItem
            // 
            saveDirectlyOnChangesToolStripMenuItem.CheckOnClick = true;
            saveDirectlyOnChangesToolStripMenuItem.Name = "saveDirectlyOnChangesToolStripMenuItem";
            saveDirectlyOnChangesToolStripMenuItem.Size = new Size(205, 22);
            saveDirectlyOnChangesToolStripMenuItem.Text = "Save directly on changes";
            // 
            // setFilterToolStripMenuItem
            // 
            setFilterToolStripMenuItem.Name = "setFilterToolStripMenuItem";
            setFilterToolStripMenuItem.Size = new Size(205, 22);
            setFilterToolStripMenuItem.Text = "Set Filter";
            setFilterToolStripMenuItem.Click += OnClickSetFilter;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(202, 6);
            // 
            // setTextureOnDoubleClickToolStripMenuItem
            // 
            setTextureOnDoubleClickToolStripMenuItem.CheckOnClick = true;
            setTextureOnDoubleClickToolStripMenuItem.Name = "setTextureOnDoubleClickToolStripMenuItem";
            setTextureOnDoubleClickToolStripMenuItem.Size = new Size(205, 22);
            setTextureOnDoubleClickToolStripMenuItem.Text = "Set TexID on double click";
            // 
            // setTexturesToolStripMenuItem
            // 
            setTexturesToolStripMenuItem.Name = "setTexturesToolStripMenuItem";
            setTexturesToolStripMenuItem.Size = new Size(205, 22);
            setTexturesToolStripMenuItem.Text = "Set Textures";
            setTexturesToolStripMenuItem.Click += SetTextureMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 25);
            // 
            // toolStripButton1
            // 
            toolStripButton1.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton1.ImageTransparentColor = Color.Magenta;
            toolStripButton1.Name = "toolStripButton1";
            toolStripButton1.Size = new Size(84, 22);
            toolStripButton1.Text = "Export To CSV";
            toolStripButton1.Click += OnClickExport;
            // 
            // toolStripButton5
            // 
            toolStripButton5.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton5.ImageTransparentColor = Color.Magenta;
            toolStripButton5.Name = "toolStripButton5";
            toolStripButton5.Size = new Size(102, 22);
            toolStripButton5.Text = "Import From CSV";
            toolStripButton5.Click += OnClickImport;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 25);
            // 
            // toolStripButton4
            // 
            toolStripButton4.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton4.ImageTransparentColor = Color.Magenta;
            toolStripButton4.Name = "toolStripButton4";
            toolStripButton4.Size = new Size(84, 22);
            toolStripButton4.Text = "Save Changes";
            toolStripButton4.Click += OnClickSaveChanges;
            // 
            // toolStripButton3
            // 
            toolStripButton3.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripButton3.ImageTransparentColor = Color.Magenta;
            toolStripButton3.Name = "toolStripButton3";
            toolStripButton3.Size = new Size(79, 22);
            toolStripButton3.Text = "Save Tiledata";
            toolStripButton3.Click += OnClickSaveTiledata;
            // 
            // TileDataControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tabcontrol);
            Controls.Add(MainToolStrip);
            DoubleBuffered = true;
            Margin = new Padding(4, 3, 4, 3);
            Name = "TileDataControl";
            Size = new Size(770, 400);
            Load += OnLoad;
            tabcontrol.ResumeLayout(false);
            tabPageItems.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            ItemsContextMenuStrip.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBoxItem).EndInit();
            splitContainer3.Panel1.ResumeLayout(false);
            splitContainer3.Panel1.PerformLayout();
            splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer3).EndInit();
            splitContainer3.ResumeLayout(false);
            tabPageLand.ResumeLayout(false);
            splitContainer5.Panel1.ResumeLayout(false);
            splitContainer5.Panel2.ResumeLayout(false);
            splitContainer5.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer5).EndInit();
            splitContainer5.ResumeLayout(false);
            splitContainer6.Panel1.ResumeLayout(false);
            splitContainer6.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer6).EndInit();
            splitContainer6.ResumeLayout(false);
            LandTilesContextMenuStrip.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBoxLand).EndInit();
            splitContainer7.Panel1.ResumeLayout(false);
            splitContainer7.Panel1.PerformLayout();
            splitContainer7.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer7).EndInit();
            splitContainer7.ResumeLayout(false);
            MainToolStrip.ResumeLayout(false);
            MainToolStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

            // Initialize additional "?" Labels
            this.namelabel = new System.Windows.Forms.Label();
            this.animlabel = new System.Windows.Forms.Label();
            this.weightlabel = new System.Windows.Forms.Label();
            this.layerlabel = new System.Windows.Forms.Label();
            this.quantitylabel = new System.Windows.Forms.Label();
            this.valuelabel = new System.Windows.Forms.Label();
            this.offsetlabel = new System.Windows.Forms.Label();
            this.huelabel = new System.Windows.Forms.Label();
            this.unknownlabel2 = new System.Windows.Forms.Label();
            this.miscdata = new System.Windows.Forms.Label();
            this.heightlabel = new System.Windows.Forms.Label();
            this.unknownlabel3 = new System.Windows.Forms.Label();
            this.namelandlabael = new System.Windows.Forms.Label();
            this.textIDlabal = new System.Windows.Forms.Label();

            // Setup "?" Labels
            SetupInfoLabel(this.namelabel, new System.Drawing.Point(210, 8));// Name
            SetupInfoLabel(this.animlabel, new System.Drawing.Point(385, 8)); // Anim
            SetupInfoLabel(this.weightlabel, new System.Drawing.Point(110, 36)); // Weight
            SetupInfoLabel(this.layerlabel, new System.Drawing.Point(250, 36)); // layer
            SetupInfoLabel(this.quantitylabel, new System.Drawing.Point(385, 36)); // Quantity
            SetupInfoLabel(this.valuelabel, new System.Drawing.Point(385, 66)); // Value
            SetupInfoLabel(this.offsetlabel, new System.Drawing.Point(250, 66)); // Offset
            SetupInfoLabel(this.huelabel, new System.Drawing.Point(110, 66)); // Hue
            SetupInfoLabel(this.unknownlabel2, new System.Drawing.Point(385, 96)); // Unknown
            SetupInfoLabel(this.miscdata, new System.Drawing.Point(250, 96)); // MiscData
            SetupInfoLabel(this.heightlabel, new System.Drawing.Point(110, 96)); // Height
            SetupInfoLabel(this.unknownlabel3, new System.Drawing.Point(110, 126)); // Unknown
            SetupInfoLabel(this.namelandlabael, new System.Drawing.Point(199, 8)); // Name of Land
            SetupInfoLabel(this.textIDlabal, new System.Drawing.Point(320, 8)); // Text ID

            // Add "?" Labels to the Controls
            this.splitContainer3.Panel1.Controls.Add(this.namelabel);
            this.splitContainer3.Panel1.Controls.Add(this.animlabel);
            this.splitContainer3.Panel1.Controls.Add(this.weightlabel);
            this.splitContainer3.Panel1.Controls.Add(this.layerlabel);
            this.splitContainer3.Panel1.Controls.Add(this.quantitylabel);
            this.splitContainer3.Panel1.Controls.Add(this.valuelabel);
            this.splitContainer3.Panel1.Controls.Add(this.offsetlabel);
            this.splitContainer3.Panel1.Controls.Add(this.huelabel);
            this.splitContainer3.Panel1.Controls.Add(this.unknownlabel2);
            this.splitContainer3.Panel1.Controls.Add(this.miscdata);
            this.splitContainer3.Panel1.Controls.Add(this.heightlabel);
            this.splitContainer3.Panel1.Controls.Add(this.unknownlabel3);
            this.splitContainer7.Panel1.Controls.Add(this.namelandlabael);
            this.splitContainer7.Panel1.Controls.Add(this.textIDlabal);
        }

        private void SetupInfoLabel(Label label, Point location)
        {
            label.AutoSize = true;
            label.Location = location;
            label.Name = "labelInfo";
            label.Size = new Size(13, 13);
            label.TabIndex = 24;
            label.Text = "?";
            label.Cursor = Cursors.Hand;
            label.Click += new System.EventHandler(this.InfoLabel_Click);
        }


        // Method to initialize tooltips
        private void InitializeToolTips()
        {
            this.toolTipComponent.SetToolTip(this.namelabel, "Enter the name of the item (max 20 characters).");
            this.toolTipComponent.SetToolTip(this.animlabel, "Enter the animation ID for the item.");
            this.toolTipComponent.SetToolTip(this.weightlabel, "Enter the weight of the item.");
            this.toolTipComponent.SetToolTip(this.layerlabel, "Enter the layer of the item.");
            this.toolTipComponent.SetToolTip(this.quantitylabel, "Enter the quantity of the item.");
            this.toolTipComponent.SetToolTip(this.valuelabel, "Enter the value of the item.");
            this.toolTipComponent.SetToolTip(this.offsetlabel, "Enter the stacking offset for the item.");
            this.toolTipComponent.SetToolTip(this.huelabel, "Enter the hue of the item.");
            this.toolTipComponent.SetToolTip(this.unknownlabel2, "Enter the second unknown value.");
            this.toolTipComponent.SetToolTip(this.miscdata, "old UO Demo weapon template def");
            this.toolTipComponent.SetToolTip(this.heightlabel, "Enter the height of the item.");
            this.toolTipComponent.SetToolTip(this.unknownlabel3, "Enter the third unknown value.");
            this.toolTipComponent.SetToolTip(this.namelandlabael, "Enter the name of the land (max 20 characters).");
            this.toolTipComponent.SetToolTip(this.textIDlabal, "Enter the texture ID for the land.");
        }

        #endregion

        private System.Windows.Forms.CheckedListBox checkedListBox1;
        private System.Windows.Forms.CheckedListBox checkedListBox2;
        private System.Windows.Forms.ContextMenuStrip ItemsContextMenuStrip;
        private System.Windows.Forms.ContextMenuStrip LandTilesContextMenuStrip;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ToolStripMenuItem memorySaveWarningToolStripMenuItem;
        private System.Windows.Forms.PictureBox pictureBoxItem;
        private System.Windows.Forms.PictureBox pictureBoxLand;
        private System.Windows.Forms.ToolStripMenuItem saveDirectlyOnChangesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectInItemsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectInLandtilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectRadarColorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setFilterToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.SplitContainer splitContainer5;
        private System.Windows.Forms.SplitContainer splitContainer6;
        private System.Windows.Forms.SplitContainer splitContainer7;
        private System.Windows.Forms.TabControl tabcontrol;
        private System.Windows.Forms.TabPage tabPageItems;
        private System.Windows.Forms.TabPage tabPageLand;
        private System.Windows.Forms.TextBox textBoxAnim;
        private System.Windows.Forms.TextBox textBoxHeigth;
        private System.Windows.Forms.TextBox textBoxHue;
        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.TextBox textBoxNameLand;
        private System.Windows.Forms.TextBox textBoxQuality;
        private System.Windows.Forms.TextBox textBoxQuantity;
        private System.Windows.Forms.TextBox textBoxStackOff;
        private System.Windows.Forms.TextBox textBoxTexID;
        private System.Windows.Forms.TextBox textBoxUnk1;
        private System.Windows.Forms.TextBox textBoxUnk2;
        private System.Windows.Forms.TextBox textBoxUnk3;
        private System.Windows.Forms.TextBox textBoxValue;
        private System.Windows.Forms.TextBox textBoxWeight;
        private System.Windows.Forms.ToolStrip MainToolStrip;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripButton toolStripButton3;
        private System.Windows.Forms.ToolStripButton toolStripButton4;
        private System.Windows.Forms.ToolStripButton toolStripButton5;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.TreeView treeViewItem;
        private System.Windows.Forms.TreeView treeViewLand;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem selectInGumpsTabMaleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectInGumpsTabFemaleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setTexturesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem setTextureOnDoubleClickToolStripMenuItem;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripTextBox searchByIdToolStripTextBox;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripTextBox searchByNameToolStripTextBox;
        private System.Windows.Forms.ToolStripButton searchByNameToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;

        // Additional Labels for "?" marks
        private System.Windows.Forms.Label namelabel;
        private System.Windows.Forms.Label animlabel;
        private System.Windows.Forms.Label weightlabel;
        private System.Windows.Forms.Label layerlabel;
        private System.Windows.Forms.Label quantitylabel;
        private System.Windows.Forms.Label valuelabel;
        private System.Windows.Forms.Label offsetlabel;
        private System.Windows.Forms.Label huelabel;
        private System.Windows.Forms.Label unknownlabel2;
        private System.Windows.Forms.Label miscdata;
        private System.Windows.Forms.Label heightlabel;
        private System.Windows.Forms.Label unknownlabel3;
        private System.Windows.Forms.Label namelandlabael;
        private System.Windows.Forms.Label textIDlabal;
    }
}
