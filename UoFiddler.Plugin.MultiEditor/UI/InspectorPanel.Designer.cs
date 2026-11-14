namespace UoFiddler.Plugin.MultiEditor.UI
{
    partial class InspectorPanel
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
            floorsBox = new System.Windows.Forms.GroupBox();
            floorsPage = new System.Windows.Forms.Panel();
            _firstFloor = new System.Windows.Forms.NumericUpDown();
            firstFloorLabel = new System.Windows.Forms.Label();
            _showHidden = new System.Windows.Forms.Button();
            _editFloors = new System.Windows.Forms.Button();
            _limitBar = new System.Windows.Forms.TrackBar();
            _limitLabel = new System.Windows.Forms.Label();
            _limit = new System.Windows.Forms.CheckBox();
            _planeZ = new System.Windows.Forms.NumericUpDown();
            _showPlane = new System.Windows.Forms.CheckBox();
            _hideWalls = new System.Windows.Forms.CheckBox();
            _hideAbove = new System.Windows.Forms.CheckBox();
            _floorList = new System.Windows.Forms.ListBox();
            selectionBox = new System.Windows.Forms.GroupBox();
            selectionPage = new System.Windows.Forms.Panel();
            _single = new System.Windows.Forms.Panel();
            _stack = new System.Windows.Forms.ListView();
            columnId = new System.Windows.Forms.ColumnHeader();
            columnZ = new System.Windows.Forms.ColumnHeader();
            columnName = new System.Windows.Forms.ColumnHeader();
            _icons = new System.Windows.Forms.ImageList(components);
            stackButtons = new System.Windows.Forms.FlowLayoutPanel();
            stackZPlusButton = new System.Windows.Forms.Button();
            stackZMinusButton = new System.Windows.Forms.Button();
            stackDeleteButton = new System.Windows.Forms.Button();
            stackPickButton = new System.Windows.Forms.Button();
            tilesInCellLabel = new System.Windows.Forms.Label();
            _dynamic = new System.Windows.Forms.CheckBox();
            xLabel = new System.Windows.Forms.Label();
            yLabel = new System.Windows.Forms.Label();
            zLabel = new System.Windows.Forms.Label();
            _x = new System.Windows.Forms.NumericUpDown();
            _y = new System.Windows.Forms.NumericUpDown();
            _z = new System.Windows.Forms.NumericUpDown();
            _infoLabel = new System.Windows.Forms.Label();
            _idLabel = new System.Windows.Forms.Label();
            _thumb = new System.Windows.Forms.PictureBox();
            _group = new System.Windows.Forms.Panel();
            _groupLabel = new System.Windows.Forms.Label();
            groupZMinus5Button = new System.Windows.Forms.Button();
            groupZMinus1Button = new System.Windows.Forms.Button();
            groupZPlus1Button = new System.Windows.Forms.Button();
            groupZPlus5Button = new System.Windows.Forms.Button();
            groupZEqualsLabel = new System.Windows.Forms.Label();
            _groupZ = new System.Windows.Forms.NumericUpDown();
            groupSetZButton = new System.Windows.Forms.Button();
            groupDeleteButton = new System.Windows.Forms.Button();
            groupHideButton = new System.Windows.Forms.Button();
            _hint = new System.Windows.Forms.Label();
            floorsBox.SuspendLayout();
            floorsPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_firstFloor).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_limitBar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_planeZ).BeginInit();
            selectionBox.SuspendLayout();
            selectionPage.SuspendLayout();
            _single.SuspendLayout();
            stackButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_x).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_y).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_z).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_thumb).BeginInit();
            _group.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_groupZ).BeginInit();
            SuspendLayout();
            // 
            // floorsBox
            // 
            floorsBox.Controls.Add(floorsPage);
            floorsBox.Dock = System.Windows.Forms.DockStyle.Top;
            floorsBox.Location = new System.Drawing.Point(0, 0);
            floorsBox.Name = "floorsBox";
            floorsBox.Size = new System.Drawing.Size(150, 316);
            floorsBox.TabIndex = 1;
            floorsBox.TabStop = false;
            floorsBox.Text = "Floors (PgUp / PgDn)";
            // 
            // floorsPage
            // 
            floorsPage.Controls.Add(_firstFloor);
            floorsPage.Controls.Add(firstFloorLabel);
            floorsPage.Controls.Add(_showHidden);
            floorsPage.Controls.Add(_editFloors);
            floorsPage.Controls.Add(_limitBar);
            floorsPage.Controls.Add(_limitLabel);
            floorsPage.Controls.Add(_limit);
            floorsPage.Controls.Add(_planeZ);
            floorsPage.Controls.Add(_showPlane);
            floorsPage.Controls.Add(_hideWalls);
            floorsPage.Controls.Add(_hideAbove);
            floorsPage.Controls.Add(_floorList);
            floorsPage.Dock = System.Windows.Forms.DockStyle.Fill;
            floorsPage.Location = new System.Drawing.Point(3, 19);
            floorsPage.Name = "floorsPage";
            floorsPage.Size = new System.Drawing.Size(144, 294);
            floorsPage.TabIndex = 0;
            // 
            // _firstFloor
            // 
            _firstFloor.Location = new System.Drawing.Point(224, 186);
            _firstFloor.Name = "_firstFloor";
            _firstFloor.Size = new System.Drawing.Size(52, 23);
            _firstFloor.TabIndex = 0;
            _firstFloor.ValueChanged += FirstFloor_ValueChanged;
            // 
            // firstFloorLabel
            // 
            firstFloorLabel.AutoSize = true;
            firstFloorLabel.Location = new System.Drawing.Point(4, 189);
            firstFloorLabel.Name = "firstFloorLabel";
            firstFloorLabel.Size = new System.Drawing.Size(172, 15);
            firstFloorLabel.TabIndex = 1;
            firstFloorLabel.Text = "First floor Z (7 on a foundation)";
            // 
            // _showHidden
            // 
            _showHidden.Location = new System.Drawing.Point(4, 264);
            _showHidden.Name = "_showHidden";
            _showHidden.Size = new System.Drawing.Size(160, 26);
            _showHidden.TabIndex = 2;
            _showHidden.Text = "Show hidden tiles";
            _showHidden.UseVisualStyleBackColor = true;
            _showHidden.Click += ShowHidden_Click;
            // 
            // _editFloors
            // 
            _editFloors.Location = new System.Drawing.Point(170, 264);
            _editFloors.Name = "_editFloors";
            _editFloors.Size = new System.Drawing.Size(106, 26);
            _editFloors.TabIndex = 3;
            _editFloors.Text = "Edit floors...";
            _editFloors.UseVisualStyleBackColor = true;
            _editFloors.Click += EditFloors_Click;
            // 
            // _limitBar
            // 
            _limitBar.AutoSize = false;
            _limitBar.Location = new System.Drawing.Point(4, 234);
            _limitBar.Name = "_limitBar";
            _limitBar.Size = new System.Drawing.Size(276, 26);
            _limitBar.TabIndex = 4;
            _limitBar.TickStyle = System.Windows.Forms.TickStyle.None;
            _limitBar.ValueChanged += LimitBar_ValueChanged;
            // 
            // _limitLabel
            // 
            _limitLabel.Location = new System.Drawing.Point(224, 214);
            _limitLabel.Name = "_limitLabel";
            _limitLabel.Size = new System.Drawing.Size(52, 18);
            _limitLabel.TabIndex = 5;
            _limitLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _limit
            // 
            _limit.AutoSize = true;
            _limit.Location = new System.Drawing.Point(4, 212);
            _limit.Name = "_limit";
            _limit.Size = new System.Drawing.Size(90, 19);
            _limit.TabIndex = 6;
            _limit.Text = "Limit height";
            _limit.UseVisualStyleBackColor = true;
            _limit.CheckedChanged += Limit_CheckedChanged;
            // 
            // _planeZ
            // 
            _planeZ.Location = new System.Drawing.Point(224, 162);
            _planeZ.Maximum = new decimal(new int[] { 127, 0, 0, 0 });
            _planeZ.Minimum = new decimal(new int[] { 128, 0, 0, int.MinValue });
            _planeZ.Name = "_planeZ";
            _planeZ.Size = new System.Drawing.Size(52, 23);
            _planeZ.TabIndex = 7;
            _planeZ.ValueChanged += PlaneZ_ValueChanged;
            // 
            // _showPlane
            // 
            _showPlane.AutoSize = true;
            _showPlane.Location = new System.Drawing.Point(4, 164);
            _showPlane.Name = "_showPlane";
            _showPlane.Size = new System.Drawing.Size(116, 19);
            _showPlane.TabIndex = 8;
            _showPlane.Text = "Show work plane";
            _showPlane.UseVisualStyleBackColor = true;
            _showPlane.CheckedChanged += ShowPlane_CheckedChanged;
            // 
            // _hideWalls
            // 
            _hideWalls.AutoSize = true;
            _hideWalls.Location = new System.Drawing.Point(150, 140);
            _hideWalls.Name = "_hideWalls";
            _hideWalls.Size = new System.Drawing.Size(80, 19);
            _hideWalls.TabIndex = 9;
            _hideWalls.Text = "Hide walls";
            _hideWalls.UseVisualStyleBackColor = true;
            _hideWalls.CheckedChanged += HideWalls_CheckedChanged;
            // 
            // _hideAbove
            // 
            _hideAbove.AutoSize = true;
            _hideAbove.Location = new System.Drawing.Point(4, 140);
            _hideAbove.Name = "_hideAbove";
            _hideAbove.Size = new System.Drawing.Size(119, 19);
            _hideAbove.TabIndex = 10;
            _hideAbove.Text = "Hide floors above";
            _hideAbove.UseVisualStyleBackColor = true;
            _hideAbove.CheckedChanged += HideAbove_CheckedChanged;
            // 
            // _floorList
            // 
            _floorList.Dock = System.Windows.Forms.DockStyle.Top;
            _floorList.IntegralHeight = false;
            _floorList.Location = new System.Drawing.Point(0, 0);
            _floorList.Name = "_floorList";
            _floorList.Size = new System.Drawing.Size(144, 134);
            _floorList.TabIndex = 11;
            _floorList.SelectedIndexChanged += FloorList_SelectedIndexChanged;
            // 
            // selectionBox
            // 
            selectionBox.Controls.Add(selectionPage);
            selectionBox.Dock = System.Windows.Forms.DockStyle.Fill;
            selectionBox.Location = new System.Drawing.Point(0, 316);
            selectionBox.Name = "selectionBox";
            selectionBox.Size = new System.Drawing.Size(150, 0);
            selectionBox.TabIndex = 0;
            selectionBox.TabStop = false;
            selectionBox.Text = "Selection";
            // 
            // selectionPage
            // 
            selectionPage.Controls.Add(_single);
            selectionPage.Controls.Add(_group);
            selectionPage.Controls.Add(_hint);
            selectionPage.Dock = System.Windows.Forms.DockStyle.Fill;
            selectionPage.Location = new System.Drawing.Point(3, 19);
            selectionPage.Name = "selectionPage";
            selectionPage.Size = new System.Drawing.Size(144, 0);
            selectionPage.TabIndex = 0;
            // 
            // _single
            // 
            _single.Controls.Add(_stack);
            _single.Controls.Add(stackButtons);
            _single.Controls.Add(tilesInCellLabel);
            _single.Controls.Add(_dynamic);
            _single.Controls.Add(xLabel);
            _single.Controls.Add(yLabel);
            _single.Controls.Add(zLabel);
            _single.Controls.Add(_x);
            _single.Controls.Add(_y);
            _single.Controls.Add(_z);
            _single.Controls.Add(_infoLabel);
            _single.Controls.Add(_idLabel);
            _single.Controls.Add(_thumb);
            _single.Dock = System.Windows.Forms.DockStyle.Fill;
            _single.Location = new System.Drawing.Point(0, 0);
            _single.Name = "_single";
            _single.Size = new System.Drawing.Size(144, 0);
            _single.TabIndex = 0;
            _single.Visible = false;
            // 
            // _stack
            // 
            _stack.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            _stack.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnId, columnZ, columnName });
            _stack.FullRowSelect = true;
            _stack.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            _stack.Location = new System.Drawing.Point(4, 154);
            _stack.MultiSelect = false;
            _stack.Name = "_stack";
            _stack.Size = new System.Drawing.Size(280, 100);
            _stack.SmallImageList = _icons;
            _stack.TabIndex = 0;
            _stack.UseCompatibleStateImageBehavior = false;
            _stack.View = System.Windows.Forms.View.Details;
            _stack.SelectedIndexChanged += Stack_SelectedIndexChanged;
            // 
            // columnId
            // 
            columnId.Text = "Id";
            columnId.Width = 90;
            // 
            // columnZ
            // 
            columnZ.Text = "Z";
            columnZ.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            columnZ.Width = 40;
            // 
            // columnName
            // 
            columnName.Text = "Name";
            columnName.Width = 120;
            // 
            // _icons
            // 
            _icons.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            _icons.ImageSize = new System.Drawing.Size(32, 32);
            _icons.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // stackButtons
            // 
            stackButtons.Controls.Add(stackZPlusButton);
            stackButtons.Controls.Add(stackZMinusButton);
            stackButtons.Controls.Add(stackDeleteButton);
            stackButtons.Controls.Add(stackPickButton);
            stackButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            stackButtons.Location = new System.Drawing.Point(0, -30);
            stackButtons.Name = "stackButtons";
            stackButtons.Size = new System.Drawing.Size(144, 30);
            stackButtons.TabIndex = 1;
            // 
            // stackZPlusButton
            // 
            stackZPlusButton.Location = new System.Drawing.Point(1, 1);
            stackZPlusButton.Margin = new System.Windows.Forms.Padding(1);
            stackZPlusButton.Name = "stackZPlusButton";
            stackZPlusButton.Size = new System.Drawing.Size(56, 26);
            stackZPlusButton.TabIndex = 0;
            stackZPlusButton.Text = "Z +1";
            stackZPlusButton.UseVisualStyleBackColor = true;
            stackZPlusButton.Click += StackZPlusButton_Click;
            // 
            // stackZMinusButton
            // 
            stackZMinusButton.Location = new System.Drawing.Point(59, 1);
            stackZMinusButton.Margin = new System.Windows.Forms.Padding(1);
            stackZMinusButton.Name = "stackZMinusButton";
            stackZMinusButton.Size = new System.Drawing.Size(56, 26);
            stackZMinusButton.TabIndex = 1;
            stackZMinusButton.Text = "Z -1";
            stackZMinusButton.UseVisualStyleBackColor = true;
            stackZMinusButton.Click += StackZMinusButton_Click;
            // 
            // stackDeleteButton
            // 
            stackDeleteButton.Location = new System.Drawing.Point(1, 29);
            stackDeleteButton.Margin = new System.Windows.Forms.Padding(1);
            stackDeleteButton.Name = "stackDeleteButton";
            stackDeleteButton.Size = new System.Drawing.Size(56, 26);
            stackDeleteButton.TabIndex = 2;
            stackDeleteButton.Text = "Delete";
            stackDeleteButton.UseVisualStyleBackColor = true;
            stackDeleteButton.Click += StackDeleteButton_Click;
            // 
            // stackPickButton
            // 
            stackPickButton.Location = new System.Drawing.Point(59, 29);
            stackPickButton.Margin = new System.Windows.Forms.Padding(1);
            stackPickButton.Name = "stackPickButton";
            stackPickButton.Size = new System.Drawing.Size(56, 26);
            stackPickButton.TabIndex = 3;
            stackPickButton.Text = "Pick";
            stackPickButton.UseVisualStyleBackColor = true;
            stackPickButton.Click += StackPickButton_Click;
            // 
            // tilesInCellLabel
            // 
            tilesInCellLabel.AutoSize = true;
            tilesInCellLabel.Location = new System.Drawing.Point(4, 134);
            tilesInCellLabel.Name = "tilesInCellLabel";
            tilesInCellLabel.Size = new System.Drawing.Size(138, 15);
            tilesInCellLabel.TabIndex = 2;
            tilesInCellLabel.Text = "Tiles in this cell (top first)";
            // 
            // _dynamic
            // 
            _dynamic.AutoSize = true;
            _dynamic.Location = new System.Drawing.Point(4, 104);
            _dynamic.Name = "_dynamic";
            _dynamic.Size = new System.Drawing.Size(127, 19);
            _dynamic.TabIndex = 3;
            _dynamic.Text = "Dynamic (invisible)";
            _dynamic.UseVisualStyleBackColor = true;
            _dynamic.CheckedChanged += Dynamic_CheckedChanged;
            // 
            // xLabel
            // 
            xLabel.AutoSize = true;
            xLabel.Location = new System.Drawing.Point(4, 80);
            xLabel.Name = "xLabel";
            xLabel.Size = new System.Drawing.Size(14, 15);
            xLabel.TabIndex = 4;
            xLabel.Text = "X";
            // 
            // yLabel
            // 
            yLabel.AutoSize = true;
            yLabel.Location = new System.Drawing.Point(96, 80);
            yLabel.Name = "yLabel";
            yLabel.Size = new System.Drawing.Size(14, 15);
            yLabel.TabIndex = 5;
            yLabel.Text = "Y";
            // 
            // zLabel
            // 
            zLabel.AutoSize = true;
            zLabel.Location = new System.Drawing.Point(188, 80);
            zLabel.Name = "zLabel";
            zLabel.Size = new System.Drawing.Size(14, 15);
            zLabel.TabIndex = 6;
            zLabel.Text = "Z";
            // 
            // _x
            // 
            _x.Location = new System.Drawing.Point(18, 76);
            _x.Maximum = new decimal(new int[] { 0, 0, 0, 0 });
            _x.Name = "_x";
            _x.Size = new System.Drawing.Size(52, 23);
            _x.TabIndex = 7;
            _x.ValueChanged += X_ValueChanged;
            // 
            // _y
            // 
            _y.Location = new System.Drawing.Point(110, 76);
            _y.Maximum = new decimal(new int[] { 0, 0, 0, 0 });
            _y.Name = "_y";
            _y.Size = new System.Drawing.Size(52, 23);
            _y.TabIndex = 8;
            _y.ValueChanged += Y_ValueChanged;
            // 
            // _z
            // 
            _z.Location = new System.Drawing.Point(202, 76);
            _z.Maximum = new decimal(new int[] { 127, 0, 0, 0 });
            _z.Minimum = new decimal(new int[] { 128, 0, 0, int.MinValue });
            _z.Name = "_z";
            _z.Size = new System.Drawing.Size(52, 23);
            _z.TabIndex = 9;
            _z.ValueChanged += Z_ValueChanged;
            // 
            // _infoLabel
            // 
            _infoLabel.AutoEllipsis = true;
            _infoLabel.Location = new System.Drawing.Point(74, 24);
            _infoLabel.Name = "_infoLabel";
            _infoLabel.Size = new System.Drawing.Size(210, 44);
            _infoLabel.TabIndex = 10;
            // 
            // _idLabel
            // 
            _idLabel.AutoEllipsis = true;
            _idLabel.Location = new System.Drawing.Point(74, 4);
            _idLabel.Name = "_idLabel";
            _idLabel.Size = new System.Drawing.Size(210, 18);
            _idLabel.TabIndex = 11;
            // 
            // _thumb
            // 
            _thumb.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            _thumb.Location = new System.Drawing.Point(4, 4);
            _thumb.Name = "_thumb";
            _thumb.Size = new System.Drawing.Size(64, 64);
            _thumb.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            _thumb.TabIndex = 12;
            _thumb.TabStop = false;
            // 
            // _group
            // 
            _group.Controls.Add(_groupLabel);
            _group.Controls.Add(groupZMinus5Button);
            _group.Controls.Add(groupZMinus1Button);
            _group.Controls.Add(groupZPlus1Button);
            _group.Controls.Add(groupZPlus5Button);
            _group.Controls.Add(groupZEqualsLabel);
            _group.Controls.Add(_groupZ);
            _group.Controls.Add(groupSetZButton);
            _group.Controls.Add(groupDeleteButton);
            _group.Controls.Add(groupHideButton);
            _group.Dock = System.Windows.Forms.DockStyle.Fill;
            _group.Location = new System.Drawing.Point(0, 0);
            _group.Name = "_group";
            _group.Size = new System.Drawing.Size(144, 0);
            _group.TabIndex = 1;
            _group.Visible = false;
            // 
            // _groupLabel
            // 
            _groupLabel.AutoEllipsis = true;
            _groupLabel.Location = new System.Drawing.Point(4, 6);
            _groupLabel.Name = "_groupLabel";
            _groupLabel.Size = new System.Drawing.Size(280, 36);
            _groupLabel.TabIndex = 0;
            // 
            // groupZMinus5Button
            // 
            groupZMinus5Button.Location = new System.Drawing.Point(4, 48);
            groupZMinus5Button.Margin = new System.Windows.Forms.Padding(1);
            groupZMinus5Button.Name = "groupZMinus5Button";
            groupZMinus5Button.Size = new System.Drawing.Size(56, 26);
            groupZMinus5Button.TabIndex = 1;
            groupZMinus5Button.Text = "Z -5";
            groupZMinus5Button.UseVisualStyleBackColor = true;
            groupZMinus5Button.Click += GroupZMinus5Button_Click;
            // 
            // groupZMinus1Button
            // 
            groupZMinus1Button.Location = new System.Drawing.Point(64, 48);
            groupZMinus1Button.Margin = new System.Windows.Forms.Padding(1);
            groupZMinus1Button.Name = "groupZMinus1Button";
            groupZMinus1Button.Size = new System.Drawing.Size(56, 26);
            groupZMinus1Button.TabIndex = 2;
            groupZMinus1Button.Text = "Z -1";
            groupZMinus1Button.UseVisualStyleBackColor = true;
            groupZMinus1Button.Click += GroupZMinus1Button_Click;
            // 
            // groupZPlus1Button
            // 
            groupZPlus1Button.Location = new System.Drawing.Point(124, 48);
            groupZPlus1Button.Margin = new System.Windows.Forms.Padding(1);
            groupZPlus1Button.Name = "groupZPlus1Button";
            groupZPlus1Button.Size = new System.Drawing.Size(56, 26);
            groupZPlus1Button.TabIndex = 3;
            groupZPlus1Button.Text = "Z +1";
            groupZPlus1Button.UseVisualStyleBackColor = true;
            groupZPlus1Button.Click += GroupZPlus1Button_Click;
            // 
            // groupZPlus5Button
            // 
            groupZPlus5Button.Location = new System.Drawing.Point(184, 48);
            groupZPlus5Button.Margin = new System.Windows.Forms.Padding(1);
            groupZPlus5Button.Name = "groupZPlus5Button";
            groupZPlus5Button.Size = new System.Drawing.Size(56, 26);
            groupZPlus5Button.TabIndex = 4;
            groupZPlus5Button.Text = "Z +5";
            groupZPlus5Button.UseVisualStyleBackColor = true;
            groupZPlus5Button.Click += GroupZPlus5Button_Click;
            // 
            // groupZEqualsLabel
            // 
            groupZEqualsLabel.AutoSize = true;
            groupZEqualsLabel.Location = new System.Drawing.Point(4, 112);
            groupZEqualsLabel.Name = "groupZEqualsLabel";
            groupZEqualsLabel.Size = new System.Drawing.Size(25, 15);
            groupZEqualsLabel.TabIndex = 5;
            groupZEqualsLabel.Text = "Z =";
            // 
            // _groupZ
            // 
            _groupZ.Location = new System.Drawing.Point(70, 108);
            _groupZ.Maximum = new decimal(new int[] { 127, 0, 0, 0 });
            _groupZ.Minimum = new decimal(new int[] { 128, 0, 0, int.MinValue });
            _groupZ.Name = "_groupZ";
            _groupZ.Size = new System.Drawing.Size(52, 23);
            _groupZ.TabIndex = 6;
            // 
            // groupSetZButton
            // 
            groupSetZButton.Location = new System.Drawing.Point(128, 106);
            groupSetZButton.Margin = new System.Windows.Forms.Padding(1);
            groupSetZButton.Name = "groupSetZButton";
            groupSetZButton.Size = new System.Drawing.Size(56, 26);
            groupSetZButton.TabIndex = 7;
            groupSetZButton.Text = "Set Z";
            groupSetZButton.UseVisualStyleBackColor = true;
            groupSetZButton.Click += GroupSetZButton_Click;
            // 
            // groupDeleteButton
            // 
            groupDeleteButton.Location = new System.Drawing.Point(4, 146);
            groupDeleteButton.Margin = new System.Windows.Forms.Padding(1);
            groupDeleteButton.Name = "groupDeleteButton";
            groupDeleteButton.Size = new System.Drawing.Size(56, 26);
            groupDeleteButton.TabIndex = 8;
            groupDeleteButton.Text = "Delete";
            groupDeleteButton.UseVisualStyleBackColor = true;
            groupDeleteButton.Click += GroupDeleteButton_Click;
            // 
            // groupHideButton
            // 
            groupHideButton.Location = new System.Drawing.Point(64, 146);
            groupHideButton.Margin = new System.Windows.Forms.Padding(1);
            groupHideButton.Name = "groupHideButton";
            groupHideButton.Size = new System.Drawing.Size(56, 26);
            groupHideButton.TabIndex = 9;
            groupHideButton.Text = "Hide";
            groupHideButton.UseVisualStyleBackColor = true;
            groupHideButton.Click += GroupHideButton_Click;
            // 
            // _hint
            // 
            _hint.Dock = System.Windows.Forms.DockStyle.Fill;
            _hint.ForeColor = System.Drawing.SystemColors.GrayText;
            _hint.Location = new System.Drawing.Point(0, 0);
            _hint.Name = "_hint";
            _hint.Size = new System.Drawing.Size(144, 0);
            _hint.TabIndex = 2;
            _hint.Text = "Nothing selected";
            _hint.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // InspectorPanel
            // 
            Controls.Add(selectionBox);
            Controls.Add(floorsBox);
            Name = "InspectorPanel";
            floorsBox.ResumeLayout(false);
            floorsPage.ResumeLayout(false);
            floorsPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)_firstFloor).EndInit();
            ((System.ComponentModel.ISupportInitialize)_limitBar).EndInit();
            ((System.ComponentModel.ISupportInitialize)_planeZ).EndInit();
            selectionBox.ResumeLayout(false);
            selectionPage.ResumeLayout(false);
            _single.ResumeLayout(false);
            _single.PerformLayout();
            stackButtons.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)_x).EndInit();
            ((System.ComponentModel.ISupportInitialize)_y).EndInit();
            ((System.ComponentModel.ISupportInitialize)_z).EndInit();
            ((System.ComponentModel.ISupportInitialize)_thumb).EndInit();
            _group.ResumeLayout(false);
            _group.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)_groupZ).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.GroupBox floorsBox;
        private System.Windows.Forms.Panel floorsPage;
        private System.Windows.Forms.NumericUpDown _firstFloor;
        private System.Windows.Forms.Label firstFloorLabel;
        private System.Windows.Forms.Button _showHidden;
        private System.Windows.Forms.Button _editFloors;
        private System.Windows.Forms.TrackBar _limitBar;
        private System.Windows.Forms.Label _limitLabel;
        private System.Windows.Forms.CheckBox _limit;
        private System.Windows.Forms.NumericUpDown _planeZ;
        private System.Windows.Forms.CheckBox _showPlane;
        private System.Windows.Forms.CheckBox _hideWalls;
        private System.Windows.Forms.CheckBox _hideAbove;
        private System.Windows.Forms.ListBox _floorList;
        private System.Windows.Forms.GroupBox selectionBox;
        private System.Windows.Forms.Panel selectionPage;
        private System.Windows.Forms.Panel _single;
        private System.Windows.Forms.ListView _stack;
        private System.Windows.Forms.ColumnHeader columnId;
        private System.Windows.Forms.ColumnHeader columnZ;
        private System.Windows.Forms.ColumnHeader columnName;
        private System.Windows.Forms.ImageList _icons;
        private System.Windows.Forms.FlowLayoutPanel stackButtons;
        private System.Windows.Forms.Button stackZPlusButton;
        private System.Windows.Forms.Button stackZMinusButton;
        private System.Windows.Forms.Button stackDeleteButton;
        private System.Windows.Forms.Button stackPickButton;
        private System.Windows.Forms.Label tilesInCellLabel;
        private System.Windows.Forms.CheckBox _dynamic;
        private System.Windows.Forms.Label xLabel;
        private System.Windows.Forms.Label yLabel;
        private System.Windows.Forms.Label zLabel;
        private System.Windows.Forms.NumericUpDown _x;
        private System.Windows.Forms.NumericUpDown _y;
        private System.Windows.Forms.NumericUpDown _z;
        private System.Windows.Forms.Label _infoLabel;
        private System.Windows.Forms.Label _idLabel;
        private System.Windows.Forms.PictureBox _thumb;
        private System.Windows.Forms.Panel _group;
        private System.Windows.Forms.Label _groupLabel;
        private System.Windows.Forms.Button groupZMinus5Button;
        private System.Windows.Forms.Button groupZMinus1Button;
        private System.Windows.Forms.Button groupZPlus1Button;
        private System.Windows.Forms.Button groupZPlus5Button;
        private System.Windows.Forms.Label groupZEqualsLabel;
        private System.Windows.Forms.NumericUpDown _groupZ;
        private System.Windows.Forms.Button groupSetZButton;
        private System.Windows.Forms.Button groupDeleteButton;
        private System.Windows.Forms.Button groupHideButton;
        private System.Windows.Forms.Label _hint;
    }
}
