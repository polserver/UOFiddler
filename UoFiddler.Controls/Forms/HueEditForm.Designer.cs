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
            if (disposing)
            {
                components?.Dispose();
                DisposePreview();
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
            toolTip = new System.Windows.Forms.ToolTip(components);
            previewTimer = new System.Windows.Forms.Timer(components);
            rootLayout = new System.Windows.Forms.TableLayoutPanel();
            topLayout = new System.Windows.Forms.TableLayoutPanel();
            nameLabel = new System.Windows.Forms.Label();
            textBoxName = new System.Windows.Forms.TextBox();
            undoButton = new System.Windows.Forms.Button();
            redoButton = new System.Windows.Forms.Button();
            revertButton = new System.Windows.Forms.Button();
            saveButton = new System.Windows.Forms.Button();
            hueStrip = new UoFiddler.Controls.UserControls.HueStripControl();
            stripContextMenu = new System.Windows.Forms.ContextMenuStrip(components);
            gradientMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            reverseMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            rampMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            menuSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            copyMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            pasteMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            editAsTextMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            menuSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            selectAllMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            hintLabel = new System.Windows.Forms.Label();
            bodyLayout = new System.Windows.Forms.TableLayoutPanel();
            colorGroupBox = new System.Windows.Forms.GroupBox();
            colorPicker = new UoFiddler.Controls.UserControls.HueColorPicker();
            toolsGroupBox = new System.Windows.Forms.GroupBox();
            toolsLayout = new System.Windows.Forms.TableLayoutPanel();
            rangeLabel = new System.Windows.Forms.Label();
            gradientLabel = new System.Windows.Forms.Label();
            gradientStartLabel = new System.Windows.Forms.Label();
            gradientStartSwatch = new System.Windows.Forms.Panel();
            gradientEndLabel = new System.Windows.Forms.Label();
            gradientEndSwatch = new System.Windows.Forms.Panel();
            gradientCurveLabel = new System.Windows.Forms.Label();
            gradientCurveComboBox = new System.Windows.Forms.ComboBox();
            gradientApplyButton = new System.Windows.Forms.Button();
            rampLabel = new System.Windows.Forms.Label();
            rampBaseLabel = new System.Windows.Forms.Label();
            rampSwatch = new System.Windows.Forms.Panel();
            rampDarkLabel = new System.Windows.Forms.Label();
            rampDarkNumeric = new System.Windows.Forms.NumericUpDown();
            rampLightLabel = new System.Windows.Forms.Label();
            rampLightNumeric = new System.Windows.Forms.NumericUpDown();
            rampApplyButton = new System.Windows.Forms.Button();
            adjustLabel = new System.Windows.Forms.Label();
            adjustHueLabel = new System.Windows.Forms.Label();
            adjustHueNumeric = new System.Windows.Forms.NumericUpDown();
            adjustSatLabel = new System.Windows.Forms.Label();
            adjustSatNumeric = new System.Windows.Forms.NumericUpDown();
            adjustLightLabel = new System.Windows.Forms.Label();
            adjustLightNumeric = new System.Windows.Forms.NumericUpDown();
            adjustRedLabel = new System.Windows.Forms.Label();
            adjustRedNumeric = new System.Windows.Forms.NumericUpDown();
            adjustGreenLabel = new System.Windows.Forms.Label();
            adjustGreenNumeric = new System.Windows.Forms.NumericUpDown();
            adjustBlueLabel = new System.Windows.Forms.Label();
            adjustBlueNumeric = new System.Windows.Forms.NumericUpDown();
            adjustApplyButton = new System.Windows.Forms.Button();
            orderLabel = new System.Windows.Forms.Label();
            orderFlow = new System.Windows.Forms.FlowLayoutPanel();
            reverseButton = new System.Windows.Forms.Button();
            copyButton = new System.Windows.Forms.Button();
            pasteButton = new System.Windows.Forms.Button();
            editAsTextButton = new System.Windows.Forms.Button();
            historyGroupBox = new System.Windows.Forms.GroupBox();
            historyListBox = new System.Windows.Forms.ListBox();
            bottomLayout = new System.Windows.Forms.TableLayoutPanel();
            previewGroupBox = new System.Windows.Forms.GroupBox();
            previewLayout = new System.Windows.Forms.TableLayoutPanel();
            previewOptionsFlow = new System.Windows.Forms.FlowLayoutPanel();
            presetLabel = new System.Windows.Forms.Label();
            presetComboBox = new System.Windows.Forms.ComboBox();
            itemIdLabel = new System.Windows.Forms.Label();
            itemIdTextBox = new System.Windows.Forms.TextBox();
            itemBrowseButton = new System.Windows.Forms.Button();
            gumpIdLabel = new System.Windows.Forms.Label();
            gumpIdTextBox = new System.Windows.Forms.TextBox();
            gumpBrowseButton = new System.Windows.Forms.Button();
            bodyIdLabel = new System.Windows.Forms.Label();
            bodyIdTextBox = new System.Windows.Forms.TextBox();
            bodyBrowseButton = new System.Windows.Forms.Button();
            facingLabel = new System.Windows.Forms.Label();
            facingBar = new System.Windows.Forms.TrackBar();
            onlyGreyCheckBox = new System.Windows.Forms.CheckBox();
            itemCaptionLabel = new System.Windows.Forms.Label();
            gumpCaptionLabel = new System.Windows.Forms.Label();
            animCaptionLabel = new System.Windows.Forms.Label();
            itemPictureBox = new System.Windows.Forms.PictureBox();
            gumpPictureBox = new System.Windows.Forms.PictureBox();
            animPictureBox = new UoFiddler.Controls.UserControls.AnimatedPictureBox();
            rootLayout.SuspendLayout();
            topLayout.SuspendLayout();
            stripContextMenu.SuspendLayout();
            bodyLayout.SuspendLayout();
            colorGroupBox.SuspendLayout();
            toolsGroupBox.SuspendLayout();
            toolsLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)rampDarkNumeric).BeginInit();
            ((System.ComponentModel.ISupportInitialize)rampLightNumeric).BeginInit();
            ((System.ComponentModel.ISupportInitialize)adjustHueNumeric).BeginInit();
            ((System.ComponentModel.ISupportInitialize)adjustSatNumeric).BeginInit();
            ((System.ComponentModel.ISupportInitialize)adjustLightNumeric).BeginInit();
            ((System.ComponentModel.ISupportInitialize)adjustRedNumeric).BeginInit();
            ((System.ComponentModel.ISupportInitialize)adjustGreenNumeric).BeginInit();
            ((System.ComponentModel.ISupportInitialize)adjustBlueNumeric).BeginInit();
            orderFlow.SuspendLayout();
            historyGroupBox.SuspendLayout();
            bottomLayout.SuspendLayout();
            previewGroupBox.SuspendLayout();
            previewLayout.SuspendLayout();
            previewOptionsFlow.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)itemPictureBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gumpPictureBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)animPictureBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)facingBar).BeginInit();
            SuspendLayout();
            //
            // previewTimer
            //
            previewTimer.Interval = 80;
            previewTimer.Tick += OnPreviewTimerTick;
            //
            // rootLayout
            //
            rootLayout.ColumnCount = 1;
            rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            rootLayout.Controls.Add(topLayout, 0, 0);
            rootLayout.Controls.Add(hueStrip, 0, 1);
            rootLayout.Controls.Add(hintLabel, 0, 2);
            rootLayout.Controls.Add(bodyLayout, 0, 3);
            rootLayout.Controls.Add(bottomLayout, 0, 4);
            rootLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            rootLayout.Location = new System.Drawing.Point(0, 0);
            rootLayout.Name = "rootLayout";
            rootLayout.Padding = new System.Windows.Forms.Padding(8);
            rootLayout.RowCount = 5;
            rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 276F));
            rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            rootLayout.Size = new System.Drawing.Size(1064, 741);
            rootLayout.TabIndex = 0;
            //
            // topLayout
            //
            topLayout.ColumnCount = 6;
            topLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            topLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            topLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            topLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            topLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            topLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            topLayout.Controls.Add(nameLabel, 0, 0);
            topLayout.Controls.Add(textBoxName, 1, 0);
            topLayout.Controls.Add(undoButton, 2, 0);
            topLayout.Controls.Add(redoButton, 3, 0);
            topLayout.Controls.Add(revertButton, 4, 0);
            topLayout.Controls.Add(saveButton, 5, 0);
            topLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            topLayout.Margin = new System.Windows.Forms.Padding(0);
            topLayout.Name = "topLayout";
            topLayout.RowCount = 1;
            topLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            topLayout.TabIndex = 0;
            //
            // nameLabel
            //
            nameLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            nameLabel.AutoSize = true;
            nameLabel.Name = "nameLabel";
            nameLabel.Text = "Name";
            //
            // textBoxName
            //
            textBoxName.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxName.MaxLength = 20;
            textBoxName.Name = "textBoxName";
            textBoxName.TabIndex = 1;
            toolTip.SetToolTip(textBoxName, "The hue's name, up to 20 characters");
            textBoxName.TextChanged += OnNameChanged;
            //
            // undoButton
            //
            undoButton.Dock = System.Windows.Forms.DockStyle.Fill;
            undoButton.Name = "undoButton";
            undoButton.TabIndex = 2;
            undoButton.Text = "Undo";
            toolTip.SetToolTip(undoButton, "Undo (Ctrl+Z)");
            undoButton.Click += OnClickUndo;
            //
            // redoButton
            //
            redoButton.Dock = System.Windows.Forms.DockStyle.Fill;
            redoButton.Name = "redoButton";
            redoButton.TabIndex = 3;
            redoButton.Text = "Redo";
            toolTip.SetToolTip(redoButton, "Redo (Ctrl+Y)");
            redoButton.Click += OnClickRedo;
            //
            // revertButton
            //
            revertButton.Dock = System.Windows.Forms.DockStyle.Fill;
            revertButton.Name = "revertButton";
            revertButton.TabIndex = 4;
            revertButton.Text = "Revert";
            toolTip.SetToolTip(revertButton, "Go back to the hue as it was opened (this is kept in the history, so it can be undone)");
            revertButton.Click += OnClickRevert;
            //
            // saveButton
            //
            saveButton.Dock = System.Windows.Forms.DockStyle.Fill;
            saveButton.Name = "saveButton";
            saveButton.TabIndex = 5;
            saveButton.Text = "Save";
            toolTip.SetToolTip(saveButton, "Write the hue back to the hue list (Ctrl+S)");
            saveButton.Click += OnClickSave;
            //
            // hueStrip
            //
            hueStrip.ContextMenuStrip = stripContextMenu;
            hueStrip.Dock = System.Windows.Forms.DockStyle.Fill;
            hueStrip.Name = "hueStrip";
            hueStrip.TabIndex = 1;
            hueStrip.SelectionChanged += OnStripSelectionChanged;
            hueStrip.CellDoubleClicked += OnStripCellDoubleClicked;
            //
            // stripContextMenu
            //
            stripContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { gradientMenuItem, reverseMenuItem, rampMenuItem, menuSeparator1, copyMenuItem, pasteMenuItem, editAsTextMenuItem, menuSeparator2, selectAllMenuItem });
            stripContextMenu.Name = "stripContextMenu";
            stripContextMenu.Opening += OnStripMenuOpening;
            //
            // gradientMenuItem
            //
            gradientMenuItem.Name = "gradientMenuItem";
            gradientMenuItem.Text = "Gradient between the ends";
            gradientMenuItem.Click += OnClickGradient;
            //
            // reverseMenuItem
            //
            reverseMenuItem.Name = "reverseMenuItem";
            reverseMenuItem.Text = "Reverse";
            reverseMenuItem.Click += OnClickReverse;
            //
            // rampMenuItem
            //
            rampMenuItem.Name = "rampMenuItem";
            rampMenuItem.Text = "Build hue from this color";
            rampMenuItem.Click += OnClickRampFromCell;
            //
            // copyMenuItem
            //
            copyMenuItem.Name = "copyMenuItem";
            copyMenuItem.ShortcutKeyDisplayString = "Ctrl+C";
            copyMenuItem.Text = "Copy colors";
            copyMenuItem.Click += OnClickCopy;
            //
            // pasteMenuItem
            //
            pasteMenuItem.Name = "pasteMenuItem";
            pasteMenuItem.ShortcutKeyDisplayString = "Ctrl+V";
            pasteMenuItem.Text = "Paste colors";
            pasteMenuItem.Click += OnClickPaste;
            //
            // editAsTextMenuItem
            //
            editAsTextMenuItem.Name = "editAsTextMenuItem";
            editAsTextMenuItem.Text = "Edit as text...";
            editAsTextMenuItem.Click += OnClickEditAsText;
            //
            // selectAllMenuItem
            //
            selectAllMenuItem.Name = "selectAllMenuItem";
            selectAllMenuItem.ShortcutKeyDisplayString = "Ctrl+A";
            selectAllMenuItem.Text = "Select all";
            selectAllMenuItem.Click += OnClickSelectAll;
            //
            // hintLabel
            //
            hintLabel.AutoEllipsis = true;
            hintLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            hintLabel.Name = "hintLabel";
            hintLabel.Text = "Click a cell to edit it. Drag or Shift+click for a range, right-click for tools. Tools use the range, or the whole hue when one cell is selected. Thin strip: the hue as opened.";
            //
            // bodyLayout
            //
            bodyLayout.ColumnCount = 2;
            bodyLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 476F));
            bodyLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            bodyLayout.Controls.Add(colorGroupBox, 0, 0);
            bodyLayout.Controls.Add(toolsGroupBox, 1, 0);
            bodyLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            bodyLayout.Margin = new System.Windows.Forms.Padding(0);
            bodyLayout.Name = "bodyLayout";
            bodyLayout.RowCount = 1;
            bodyLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            bodyLayout.TabIndex = 2;
            //
            // colorGroupBox
            //
            colorGroupBox.Controls.Add(colorPicker);
            colorGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            colorGroupBox.Name = "colorGroupBox";
            colorGroupBox.Padding = new System.Windows.Forms.Padding(10, 6, 10, 8);
            colorGroupBox.TabIndex = 0;
            colorGroupBox.TabStop = false;
            colorGroupBox.Text = "Color";
            //
            // colorPicker
            //
            colorPicker.Dock = System.Windows.Forms.DockStyle.Fill;
            colorPicker.Name = "colorPicker";
            colorPicker.TabIndex = 0;
            colorPicker.ColorChanging += OnPickerColorChanging;
            colorPicker.ColorCommitted += OnPickerColorCommitted;
            //
            // toolsGroupBox
            //
            toolsGroupBox.Controls.Add(toolsLayout);
            toolsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            toolsGroupBox.Name = "toolsGroupBox";
            toolsGroupBox.Padding = new System.Windows.Forms.Padding(10, 4, 10, 8);
            toolsGroupBox.TabIndex = 1;
            toolsGroupBox.TabStop = false;
            toolsGroupBox.Text = "Tools";
            //
            // toolsLayout
            //
            toolsLayout.ColumnCount = 9;
            toolsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            toolsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            toolsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 54F));
            toolsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 52F));
            toolsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 54F));
            toolsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 58F));
            toolsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 54F));
            toolsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            toolsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            toolsLayout.Controls.Add(rangeLabel, 0, 0);
            toolsLayout.Controls.Add(gradientLabel, 0, 1);
            toolsLayout.Controls.Add(gradientStartLabel, 1, 1);
            toolsLayout.Controls.Add(gradientStartSwatch, 2, 1);
            toolsLayout.Controls.Add(gradientEndLabel, 3, 1);
            toolsLayout.Controls.Add(gradientEndSwatch, 4, 1);
            toolsLayout.Controls.Add(gradientCurveLabel, 5, 1);
            toolsLayout.Controls.Add(gradientCurveComboBox, 6, 1);
            toolsLayout.Controls.Add(gradientApplyButton, 8, 1);
            toolsLayout.Controls.Add(rampLabel, 0, 2);
            toolsLayout.Controls.Add(rampBaseLabel, 1, 2);
            toolsLayout.Controls.Add(rampSwatch, 2, 2);
            toolsLayout.Controls.Add(rampDarkLabel, 3, 2);
            toolsLayout.Controls.Add(rampDarkNumeric, 4, 2);
            toolsLayout.Controls.Add(rampLightLabel, 5, 2);
            toolsLayout.Controls.Add(rampLightNumeric, 6, 2);
            toolsLayout.Controls.Add(rampApplyButton, 8, 2);
            toolsLayout.Controls.Add(adjustLabel, 0, 3);
            toolsLayout.Controls.Add(adjustHueLabel, 1, 3);
            toolsLayout.Controls.Add(adjustHueNumeric, 2, 3);
            toolsLayout.Controls.Add(adjustSatLabel, 3, 3);
            toolsLayout.Controls.Add(adjustSatNumeric, 4, 3);
            toolsLayout.Controls.Add(adjustLightLabel, 5, 3);
            toolsLayout.Controls.Add(adjustLightNumeric, 6, 3);
            toolsLayout.Controls.Add(adjustRedLabel, 1, 4);
            toolsLayout.Controls.Add(adjustRedNumeric, 2, 4);
            toolsLayout.Controls.Add(adjustGreenLabel, 3, 4);
            toolsLayout.Controls.Add(adjustGreenNumeric, 4, 4);
            toolsLayout.Controls.Add(adjustBlueLabel, 5, 4);
            toolsLayout.Controls.Add(adjustBlueNumeric, 6, 4);
            toolsLayout.Controls.Add(adjustApplyButton, 8, 4);
            toolsLayout.Controls.Add(orderLabel, 0, 5);
            toolsLayout.Controls.Add(orderFlow, 1, 5);
            toolsLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            toolsLayout.Name = "toolsLayout";
            toolsLayout.RowCount = 7;
            toolsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            toolsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            toolsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            toolsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            toolsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            toolsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            toolsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            toolsLayout.TabIndex = 0;
            //
            // rangeLabel
            //
            rangeLabel.AutoEllipsis = true;
            toolsLayout.SetColumnSpan(rangeLabel, 9);
            rangeLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            rangeLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            rangeLabel.Name = "rangeLabel";
            rangeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            toolTip.SetToolTip(rangeLabel, "The cells the tools below change. Drag or Shift+click on the strip to pick a range.");
            //
            // gradientLabel
            //
            gradientLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            gradientLabel.AutoSize = true;
            gradientLabel.Name = "gradientLabel";
            gradientLabel.Text = "Gradient";
            toolTip.SetToolTip(gradientLabel, "Blend the range from one color to another");
            //
            // gradientStartLabel
            //
            gradientStartLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            gradientStartLabel.AutoSize = true;
            gradientStartLabel.Name = "gradientStartLabel";
            gradientStartLabel.Text = "From";
            toolTip.SetToolTip(gradientStartLabel, "Color at the start of the range: click the swatch to change");
            //
            // gradientStartSwatch
            //
            gradientStartSwatch.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            gradientStartSwatch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            gradientStartSwatch.Cursor = System.Windows.Forms.Cursors.Hand;
            gradientStartSwatch.Name = "gradientStartSwatch";
            gradientStartSwatch.Size = new System.Drawing.Size(50, 23);
            toolTip.SetToolTip(gradientStartSwatch, "Color at the start of the range: click to change");
            gradientStartSwatch.Click += OnClickGradientStartSwatch;
            //
            // gradientEndLabel
            //
            gradientEndLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            gradientEndLabel.AutoSize = true;
            gradientEndLabel.Name = "gradientEndLabel";
            gradientEndLabel.Text = "To";
            toolTip.SetToolTip(gradientEndLabel, "Color at the end of the range: click the swatch to change");
            //
            // gradientEndSwatch
            //
            gradientEndSwatch.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            gradientEndSwatch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            gradientEndSwatch.Cursor = System.Windows.Forms.Cursors.Hand;
            gradientEndSwatch.Name = "gradientEndSwatch";
            gradientEndSwatch.Size = new System.Drawing.Size(50, 23);
            toolTip.SetToolTip(gradientEndSwatch, "Color at the end of the range: click to change");
            gradientEndSwatch.Click += OnClickGradientEndSwatch;
            //
            // gradientCurveLabel
            //
            gradientCurveLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            gradientCurveLabel.AutoSize = true;
            gradientCurveLabel.Name = "gradientCurveLabel";
            gradientCurveLabel.Text = "Curve";
            toolTip.SetToolTip(gradientCurveLabel, "How the colors move from start to end");
            //
            // gradientCurveComboBox
            //
            gradientCurveComboBox.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            toolsLayout.SetColumnSpan(gradientCurveComboBox, 2);
            gradientCurveComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            gradientCurveComboBox.Name = "gradientCurveComboBox";
            gradientCurveComboBox.Size = new System.Drawing.Size(90, 23);
            toolTip.SetToolTip(gradientCurveComboBox, "How the colors move from start to end: evenly (Linear), slow then fast (Ease in), fast then slow (Ease out), or slow at both ends (Smooth)");
            //
            // gradientApplyButton
            //
            gradientApplyButton.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            gradientApplyButton.Name = "gradientApplyButton";
            gradientApplyButton.Size = new System.Drawing.Size(78, 25);
            gradientApplyButton.Text = "Apply";
            toolTip.SetToolTip(gradientApplyButton, "Fill the range with the gradient");
            gradientApplyButton.Click += OnClickGradient;
            //
            // rampLabel
            //
            rampLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            rampLabel.AutoSize = true;
            rampLabel.Name = "rampLabel";
            rampLabel.Text = "Build";
            toolTip.SetToolTip(rampLabel, "Build shades from one base color: darker towards cell 0, lighter towards cell 31");
            //
            // rampBaseLabel
            //
            rampBaseLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            rampBaseLabel.AutoSize = true;
            rampBaseLabel.Name = "rampBaseLabel";
            rampBaseLabel.Text = "Base";
            toolTip.SetToolTip(rampBaseLabel, "The base color the hue is built around: click the swatch to change");
            //
            // rampSwatch
            //
            rampSwatch.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            rampSwatch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            rampSwatch.Cursor = System.Windows.Forms.Cursors.Hand;
            rampSwatch.Name = "rampSwatch";
            rampSwatch.Size = new System.Drawing.Size(50, 23);
            toolTip.SetToolTip(rampSwatch, "The base color the hue is built around: click to change");
            rampSwatch.Click += OnClickRampSwatch;
            //
            // rampDarkLabel
            //
            rampDarkLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            rampDarkLabel.AutoSize = true;
            rampDarkLabel.Name = "rampDarkLabel";
            rampDarkLabel.Text = "Dark %";
            toolTip.SetToolTip(rampDarkLabel, "How far cell 0 is pulled towards black");
            //
            // rampDarkNumeric
            //
            rampDarkNumeric.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            rampDarkNumeric.Name = "rampDarkNumeric";
            rampDarkNumeric.Size = new System.Drawing.Size(50, 23);
            rampDarkNumeric.Value = new decimal(new int[] { 85, 0, 0, 0 });
            toolTip.SetToolTip(rampDarkNumeric, "How far cell 0 is pulled towards black");
            //
            // rampLightLabel
            //
            rampLightLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            rampLightLabel.AutoSize = true;
            rampLightLabel.Name = "rampLightLabel";
            rampLightLabel.Text = "Light %";
            toolTip.SetToolTip(rampLightLabel, "How far cell 31 is pulled towards white");
            //
            // rampLightNumeric
            //
            rampLightNumeric.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            rampLightNumeric.Name = "rampLightNumeric";
            rampLightNumeric.Size = new System.Drawing.Size(50, 23);
            rampLightNumeric.Value = new decimal(new int[] { 60, 0, 0, 0 });
            toolTip.SetToolTip(rampLightNumeric, "How far cell 31 is pulled towards white");
            //
            // rampApplyButton
            //
            rampApplyButton.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            rampApplyButton.Name = "rampApplyButton";
            rampApplyButton.Size = new System.Drawing.Size(78, 25);
            rampApplyButton.Text = "Build hue";
            toolTip.SetToolTip(rampApplyButton, "Replace the range with shades of the base color. Each cell gets the shade it has in a full 32-cell hue.");
            rampApplyButton.Click += OnClickRamp;
            //
            // adjustLabel
            //
            adjustLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            adjustLabel.AutoSize = true;
            adjustLabel.Name = "adjustLabel";
            adjustLabel.Text = "Adjust";
            toolTip.SetToolTip(adjustLabel, "Shift every color in the range. Zero leaves that part unchanged.");
            //
            // adjustHueLabel
            //
            adjustHueLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            adjustHueLabel.AutoSize = true;
            adjustHueLabel.Name = "adjustHueLabel";
            adjustHueLabel.Text = "Hue°";
            toolTip.SetToolTip(adjustHueLabel, "Rotate the hue by this many degrees");
            //
            // adjustHueNumeric
            //
            adjustHueNumeric.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            adjustHueNumeric.Maximum = new decimal(new int[] { 180, 0, 0, 0 });
            adjustHueNumeric.Minimum = new decimal(new int[] { 180, 0, 0, int.MinValue });
            adjustHueNumeric.Name = "adjustHueNumeric";
            adjustHueNumeric.Size = new System.Drawing.Size(50, 23);
            toolTip.SetToolTip(adjustHueNumeric, "Rotate the hue by this many degrees");
            //
            // adjustSatLabel
            //
            adjustSatLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            adjustSatLabel.AutoSize = true;
            adjustSatLabel.Name = "adjustSatLabel";
            adjustSatLabel.Text = "Sat %";
            toolTip.SetToolTip(adjustSatLabel, "Change saturation by this percentage of itself");
            //
            // adjustSatNumeric
            //
            adjustSatNumeric.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            adjustSatNumeric.Maximum = new decimal(new int[] { 200, 0, 0, 0 });
            adjustSatNumeric.Minimum = new decimal(new int[] { 100, 0, 0, int.MinValue });
            adjustSatNumeric.Name = "adjustSatNumeric";
            adjustSatNumeric.Size = new System.Drawing.Size(50, 23);
            toolTip.SetToolTip(adjustSatNumeric, "Change saturation by this percentage of itself");
            //
            // adjustLightLabel
            //
            adjustLightLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            adjustLightLabel.AutoSize = true;
            adjustLightLabel.Name = "adjustLightLabel";
            adjustLightLabel.Text = "Light ±";
            toolTip.SetToolTip(adjustLightLabel, "Add this many lightness points (-100 to 100)");
            //
            // adjustLightNumeric
            //
            adjustLightNumeric.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            adjustLightNumeric.Minimum = new decimal(new int[] { 100, 0, 0, int.MinValue });
            adjustLightNumeric.Name = "adjustLightNumeric";
            adjustLightNumeric.Size = new System.Drawing.Size(50, 23);
            toolTip.SetToolTip(adjustLightNumeric, "Add this many lightness points (-100 to 100)");
            //
            // adjustRedLabel
            //
            adjustRedLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            adjustRedLabel.AutoSize = true;
            adjustRedLabel.Name = "adjustRedLabel";
            adjustRedLabel.Text = "R ±";
            toolTip.SetToolTip(adjustRedLabel, "Add this to red (0-255 scale, applied after hue, saturation and lightness)");
            //
            // adjustRedNumeric
            //
            adjustRedNumeric.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            adjustRedNumeric.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            adjustRedNumeric.Minimum = new decimal(new int[] { 255, 0, 0, int.MinValue });
            adjustRedNumeric.Name = "adjustRedNumeric";
            adjustRedNumeric.Size = new System.Drawing.Size(50, 23);
            toolTip.SetToolTip(adjustRedNumeric, "Add this to red (0-255 scale, applied after hue, saturation and lightness)");
            //
            // adjustGreenLabel
            //
            adjustGreenLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            adjustGreenLabel.AutoSize = true;
            adjustGreenLabel.Name = "adjustGreenLabel";
            adjustGreenLabel.Text = "G ±";
            toolTip.SetToolTip(adjustGreenLabel, "Add this to green (0-255 scale, applied after hue, saturation and lightness)");
            //
            // adjustGreenNumeric
            //
            adjustGreenNumeric.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            adjustGreenNumeric.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            adjustGreenNumeric.Minimum = new decimal(new int[] { 255, 0, 0, int.MinValue });
            adjustGreenNumeric.Name = "adjustGreenNumeric";
            adjustGreenNumeric.Size = new System.Drawing.Size(50, 23);
            toolTip.SetToolTip(adjustGreenNumeric, "Add this to green (0-255 scale, applied after hue, saturation and lightness)");
            //
            // adjustBlueLabel
            //
            adjustBlueLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            adjustBlueLabel.AutoSize = true;
            adjustBlueLabel.Name = "adjustBlueLabel";
            adjustBlueLabel.Text = "B ±";
            toolTip.SetToolTip(adjustBlueLabel, "Add this to blue (0-255 scale, applied after hue, saturation and lightness)");
            //
            // adjustBlueNumeric
            //
            adjustBlueNumeric.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            adjustBlueNumeric.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            adjustBlueNumeric.Minimum = new decimal(new int[] { 255, 0, 0, int.MinValue });
            adjustBlueNumeric.Name = "adjustBlueNumeric";
            adjustBlueNumeric.Size = new System.Drawing.Size(50, 23);
            toolTip.SetToolTip(adjustBlueNumeric, "Add this to blue (0-255 scale, applied after hue, saturation and lightness)");
            //
            // adjustApplyButton
            //
            adjustApplyButton.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            adjustApplyButton.Name = "adjustApplyButton";
            adjustApplyButton.Size = new System.Drawing.Size(78, 25);
            adjustApplyButton.Text = "Apply";
            toolTip.SetToolTip(adjustApplyButton, "Apply the hue, saturation, lightness and R/G/B changes to the range");
            adjustApplyButton.Click += OnClickAdjust;
            //
            // orderLabel
            //
            orderLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            orderLabel.AutoSize = true;
            orderLabel.Name = "orderLabel";
            orderLabel.Text = "Colors";
            toolTip.SetToolTip(orderLabel, "Reorder the range, or move colors in and out as text");
            //
            // orderFlow
            //
            toolsLayout.SetColumnSpan(orderFlow, 8);
            orderFlow.Controls.Add(reverseButton);
            orderFlow.Controls.Add(copyButton);
            orderFlow.Controls.Add(pasteButton);
            orderFlow.Controls.Add(editAsTextButton);
            orderFlow.Dock = System.Windows.Forms.DockStyle.Fill;
            orderFlow.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            orderFlow.Name = "orderFlow";
            orderFlow.WrapContents = false;
            //
            // reverseButton
            //
            reverseButton.Name = "reverseButton";
            reverseButton.Size = new System.Drawing.Size(76, 25);
            reverseButton.Text = "Reverse";
            toolTip.SetToolTip(reverseButton, "Reverse the order of the colors in the range");
            reverseButton.Click += OnClickReverse;
            //
            // copyButton
            //
            copyButton.Name = "copyButton";
            copyButton.Size = new System.Drawing.Size(76, 25);
            copyButton.Text = "Copy";
            toolTip.SetToolTip(copyButton, "Copy the range as text, one #RRGGBB per line (Ctrl+C)");
            copyButton.Click += OnClickCopy;
            //
            // pasteButton
            //
            pasteButton.Name = "pasteButton";
            pasteButton.Size = new System.Drawing.Size(76, 25);
            pasteButton.Text = "Paste";
            toolTip.SetToolTip(pasteButton, "Paste colors: one fills the range, as many as the range replaces it, 32 replace the hue (Ctrl+V)");
            pasteButton.Click += OnClickPaste;
            //
            // editAsTextButton
            //
            editAsTextButton.Name = "editAsTextButton";
            editAsTextButton.Size = new System.Drawing.Size(100, 25);
            editAsTextButton.Text = "Edit as text...";
            toolTip.SetToolTip(editAsTextButton, "Type or paste all 32 colors at once");
            editAsTextButton.Click += OnClickEditAsText;
            //
            // historyGroupBox
            //
            historyGroupBox.Controls.Add(historyListBox);
            historyGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            historyGroupBox.Name = "historyGroupBox";
            historyGroupBox.Padding = new System.Windows.Forms.Padding(8, 6, 8, 8);
            historyGroupBox.TabIndex = 2;
            historyGroupBox.TabStop = false;
            historyGroupBox.Text = "History";
            //
            // historyListBox
            //
            historyListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            historyListBox.IntegralHeight = false;
            historyListBox.Name = "historyListBox";
            historyListBox.TabIndex = 0;
            toolTip.SetToolTip(historyListBox, "Click a step to go back to it. Grey steps can be redone.");
            historyListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            historyListBox.DrawItem += OnDrawHistoryItem;
            historyListBox.SelectedIndexChanged += OnHistorySelected;
            //
            // bottomLayout
            //
            bottomLayout.ColumnCount = 2;
            bottomLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            bottomLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 230F));
            bottomLayout.Controls.Add(previewGroupBox, 0, 0);
            bottomLayout.Controls.Add(historyGroupBox, 1, 0);
            bottomLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            bottomLayout.Margin = new System.Windows.Forms.Padding(0);
            bottomLayout.Name = "bottomLayout";
            bottomLayout.RowCount = 1;
            bottomLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            bottomLayout.TabIndex = 3;
            //
            // previewGroupBox
            //
            previewGroupBox.Controls.Add(previewLayout);
            previewGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            previewGroupBox.Name = "previewGroupBox";
            previewGroupBox.Padding = new System.Windows.Forms.Padding(8, 4, 8, 8);
            previewGroupBox.TabIndex = 3;
            previewGroupBox.TabStop = false;
            previewGroupBox.Text = "Preview";
            //
            // previewLayout
            //
            previewLayout.ColumnCount = 3;
            previewLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            previewLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            previewLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.34F));
            previewLayout.Controls.Add(previewOptionsFlow, 0, 0);
            previewLayout.Controls.Add(itemCaptionLabel, 0, 1);
            previewLayout.Controls.Add(gumpCaptionLabel, 1, 1);
            previewLayout.Controls.Add(animCaptionLabel, 2, 1);
            previewLayout.Controls.Add(itemPictureBox, 0, 2);
            previewLayout.Controls.Add(gumpPictureBox, 1, 2);
            previewLayout.Controls.Add(animPictureBox, 2, 2);
            previewLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            previewLayout.Name = "previewLayout";
            previewLayout.RowCount = 3;
            previewLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            previewLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            previewLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            previewLayout.TabIndex = 0;
            //
            // previewOptionsFlow
            //
            previewLayout.SetColumnSpan(previewOptionsFlow, 3);
            previewOptionsFlow.Controls.Add(presetLabel);
            previewOptionsFlow.Controls.Add(presetComboBox);
            previewOptionsFlow.Controls.Add(itemIdLabel);
            previewOptionsFlow.Controls.Add(itemIdTextBox);
            previewOptionsFlow.Controls.Add(itemBrowseButton);
            previewOptionsFlow.Controls.Add(gumpIdLabel);
            previewOptionsFlow.Controls.Add(gumpIdTextBox);
            previewOptionsFlow.Controls.Add(gumpBrowseButton);
            previewOptionsFlow.Controls.Add(bodyIdLabel);
            previewOptionsFlow.Controls.Add(bodyIdTextBox);
            previewOptionsFlow.Controls.Add(bodyBrowseButton);
            previewOptionsFlow.SetFlowBreak(bodyBrowseButton, true);
            previewOptionsFlow.Controls.Add(facingLabel);
            previewOptionsFlow.Controls.Add(facingBar);
            previewOptionsFlow.Controls.Add(onlyGreyCheckBox);
            previewOptionsFlow.AutoSize = true;
            previewOptionsFlow.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            previewOptionsFlow.Dock = System.Windows.Forms.DockStyle.Fill;
            previewOptionsFlow.Margin = new System.Windows.Forms.Padding(0);
            previewOptionsFlow.Name = "previewOptionsFlow";
            //
            // presetLabel
            //
            presetLabel.AutoSize = true;
            presetLabel.Margin = new System.Windows.Forms.Padding(3, 7, 0, 0);
            presetLabel.Name = "presetLabel";
            presetLabel.Text = "Show";
            toolTip.SetToolTip(presetLabel, "What to preview the hue on: a ready-made item, gump and animation set, or Custom for the ids typed or picked on the right");
            //
            // presetComboBox
            //
            presetComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            presetComboBox.Margin = new System.Windows.Forms.Padding(3, 4, 12, 3);
            presetComboBox.Name = "presetComboBox";
            presetComboBox.Size = new System.Drawing.Size(150, 23);
            toolTip.SetToolTip(presetComboBox, "What to preview the hue on: a ready-made item, gump and animation set, or Custom for the ids typed or picked on the right");
            presetComboBox.SelectedIndexChanged += OnPresetChanged;
            //
            // itemIdLabel
            //
            itemIdLabel.AutoSize = true;
            itemIdLabel.Margin = new System.Windows.Forms.Padding(3, 7, 0, 0);
            itemIdLabel.Name = "itemIdLabel";
            itemIdLabel.Text = "Item";
            //
            // itemIdTextBox
            //
            itemIdTextBox.Margin = new System.Windows.Forms.Padding(3, 4, 0, 3);
            itemIdTextBox.Name = "itemIdTextBox";
            itemIdTextBox.Size = new System.Drawing.Size(64, 23);
            toolTip.SetToolTip(itemIdTextBox, "Item id (decimal or 0x hex), Enter to show. Empty: none.");
            itemIdTextBox.KeyDown += OnPreviewIdKeyDown;
            itemIdTextBox.Leave += OnPreviewIdLeave;
            //
            // itemBrowseButton
            //
            itemBrowseButton.Margin = new System.Windows.Forms.Padding(2, 3, 12, 3);
            itemBrowseButton.Name = "itemBrowseButton";
            itemBrowseButton.Size = new System.Drawing.Size(28, 25);
            itemBrowseButton.Text = "...";
            toolTip.SetToolTip(itemBrowseButton, "Pick an item from a grid of thumbnails");
            itemBrowseButton.Click += OnClickBrowseItem;
            //
            // gumpIdLabel
            //
            gumpIdLabel.AutoSize = true;
            gumpIdLabel.Margin = new System.Windows.Forms.Padding(3, 7, 0, 0);
            gumpIdLabel.Name = "gumpIdLabel";
            gumpIdLabel.Text = "Gump";
            //
            // gumpIdTextBox
            //
            gumpIdTextBox.Margin = new System.Windows.Forms.Padding(3, 4, 0, 3);
            gumpIdTextBox.Name = "gumpIdTextBox";
            gumpIdTextBox.Size = new System.Drawing.Size(64, 23);
            toolTip.SetToolTip(gumpIdTextBox, "Gump id (decimal or 0x hex), Enter to show. Empty: none.");
            gumpIdTextBox.KeyDown += OnPreviewIdKeyDown;
            gumpIdTextBox.Leave += OnPreviewIdLeave;
            //
            // gumpBrowseButton
            //
            gumpBrowseButton.Margin = new System.Windows.Forms.Padding(2, 3, 12, 3);
            gumpBrowseButton.Name = "gumpBrowseButton";
            gumpBrowseButton.Size = new System.Drawing.Size(28, 25);
            gumpBrowseButton.Text = "...";
            toolTip.SetToolTip(gumpBrowseButton, "Pick a gump from a grid of thumbnails");
            gumpBrowseButton.Click += OnClickBrowseGump;
            //
            // bodyIdLabel
            //
            bodyIdLabel.AutoSize = true;
            bodyIdLabel.Margin = new System.Windows.Forms.Padding(3, 7, 0, 0);
            bodyIdLabel.Name = "bodyIdLabel";
            bodyIdLabel.Text = "Body";
            //
            // bodyIdTextBox
            //
            bodyIdTextBox.Margin = new System.Windows.Forms.Padding(3, 4, 0, 3);
            bodyIdTextBox.Name = "bodyIdTextBox";
            bodyIdTextBox.Size = new System.Drawing.Size(64, 23);
            toolTip.SetToolTip(bodyIdTextBox, "Animation body id (decimal or 0x hex), Enter to show. Empty: none.");
            bodyIdTextBox.KeyDown += OnPreviewIdKeyDown;
            bodyIdTextBox.Leave += OnPreviewIdLeave;
            //
            // bodyBrowseButton
            //
            bodyBrowseButton.Margin = new System.Windows.Forms.Padding(2, 3, 12, 3);
            bodyBrowseButton.Name = "bodyBrowseButton";
            bodyBrowseButton.Size = new System.Drawing.Size(28, 25);
            bodyBrowseButton.Text = "...";
            toolTip.SetToolTip(bodyBrowseButton, "Pick an animation body from a grid of thumbnails");
            bodyBrowseButton.Click += OnClickBrowseBody;
            //
            // facingLabel
            //
            facingLabel.AutoSize = true;
            facingLabel.Margin = new System.Windows.Forms.Padding(3, 7, 0, 0);
            facingLabel.Name = "facingLabel";
            facingLabel.Text = "Facing";
            toolTip.SetToolTip(facingLabel, "Which way the animation faces (the same bar as the Animations tab)");
            //
            // facingBar
            //
            facingBar.AutoSize = false;
            facingBar.LargeChange = 1;
            facingBar.Margin = new System.Windows.Forms.Padding(0, 4, 8, 3);
            facingBar.Maximum = 7;
            facingBar.Name = "facingBar";
            facingBar.Size = new System.Drawing.Size(120, 23);
            toolTip.SetToolTip(facingBar, "Which way the animation faces (the same bar as the Animations tab)");
            facingBar.Scroll += OnFacingChanged;
            //
            // onlyGreyCheckBox
            //
            onlyGreyCheckBox.AutoSize = true;
            onlyGreyCheckBox.Margin = new System.Windows.Forms.Padding(6, 6, 3, 3);
            onlyGreyCheckBox.Name = "onlyGreyCheckBox";
            onlyGreyCheckBox.Text = "Hue only grey pixels";
            toolTip.SetToolTip(onlyGreyCheckBox, "Partial hue: only grey pixels take the hue, colored details keep their own colors. This is how the client draws items with the PartialHue tiledata flag. It is set from the item's tiledata when you pick an item, and you can change it.");
            onlyGreyCheckBox.CheckedChanged += OnOnlyGreyChanged;
            //
            // itemCaptionLabel
            //
            itemCaptionLabel.AutoEllipsis = true;
            itemCaptionLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            itemCaptionLabel.Name = "itemCaptionLabel";
            itemCaptionLabel.Text = "Item";
            itemCaptionLabel.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            //
            // gumpCaptionLabel
            //
            gumpCaptionLabel.AutoEllipsis = true;
            gumpCaptionLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            gumpCaptionLabel.Name = "gumpCaptionLabel";
            gumpCaptionLabel.Text = "Gump";
            gumpCaptionLabel.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            //
            // animCaptionLabel
            //
            animCaptionLabel.AutoEllipsis = true;
            animCaptionLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            animCaptionLabel.Name = "animCaptionLabel";
            animCaptionLabel.Text = "Animation";
            animCaptionLabel.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            //
            // itemPictureBox
            //
            itemPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            itemPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            itemPictureBox.Name = "itemPictureBox";
            itemPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            itemPictureBox.TabStop = false;
            itemPictureBox.Resize += OnPreviewPaneResize;
            //
            // gumpPictureBox
            //
            gumpPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            gumpPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            gumpPictureBox.Name = "gumpPictureBox";
            gumpPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            gumpPictureBox.TabStop = false;
            gumpPictureBox.Resize += OnPreviewPaneResize;
            //
            // animPictureBox
            //
            animPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            animPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            animPictureBox.Name = "animPictureBox";
            animPictureBox.TabStop = false;
            //
            // HueEditForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1064, 741);
            Controls.Add(rootLayout);
            DoubleBuffered = true;
            KeyPreview = true;
            MinimumSize = new System.Drawing.Size(1030, 700);
            Name = "HueEditForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "HueEdit";
            rootLayout.ResumeLayout(false);
            topLayout.ResumeLayout(false);
            topLayout.PerformLayout();
            stripContextMenu.ResumeLayout(false);
            bodyLayout.ResumeLayout(false);
            colorGroupBox.ResumeLayout(false);
            toolsGroupBox.ResumeLayout(false);
            toolsLayout.ResumeLayout(false);
            toolsLayout.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)rampDarkNumeric).EndInit();
            ((System.ComponentModel.ISupportInitialize)rampLightNumeric).EndInit();
            ((System.ComponentModel.ISupportInitialize)adjustHueNumeric).EndInit();
            ((System.ComponentModel.ISupportInitialize)adjustSatNumeric).EndInit();
            ((System.ComponentModel.ISupportInitialize)adjustLightNumeric).EndInit();
            ((System.ComponentModel.ISupportInitialize)adjustRedNumeric).EndInit();
            ((System.ComponentModel.ISupportInitialize)adjustGreenNumeric).EndInit();
            ((System.ComponentModel.ISupportInitialize)adjustBlueNumeric).EndInit();
            orderFlow.ResumeLayout(false);
            orderFlow.PerformLayout();
            historyGroupBox.ResumeLayout(false);
            bottomLayout.ResumeLayout(false);
            previewGroupBox.ResumeLayout(false);
            previewLayout.ResumeLayout(false);
            previewOptionsFlow.ResumeLayout(false);
            previewOptionsFlow.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)itemPictureBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)gumpPictureBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)animPictureBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)facingBar).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.Timer previewTimer;
        private System.Windows.Forms.TableLayoutPanel rootLayout;
        private System.Windows.Forms.TableLayoutPanel topLayout;
        private System.Windows.Forms.Label nameLabel;
        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.Button undoButton;
        private System.Windows.Forms.Button redoButton;
        private System.Windows.Forms.Button revertButton;
        private System.Windows.Forms.Button saveButton;
        private UoFiddler.Controls.UserControls.HueStripControl hueStrip;
        private System.Windows.Forms.ContextMenuStrip stripContextMenu;
        private System.Windows.Forms.ToolStripMenuItem gradientMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reverseMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rampMenuItem;
        private System.Windows.Forms.ToolStripSeparator menuSeparator1;
        private System.Windows.Forms.ToolStripMenuItem copyMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editAsTextMenuItem;
        private System.Windows.Forms.ToolStripSeparator menuSeparator2;
        private System.Windows.Forms.ToolStripMenuItem selectAllMenuItem;
        private System.Windows.Forms.Label hintLabel;
        private System.Windows.Forms.TableLayoutPanel bodyLayout;
        private System.Windows.Forms.GroupBox colorGroupBox;
        private UoFiddler.Controls.UserControls.HueColorPicker colorPicker;
        private System.Windows.Forms.GroupBox toolsGroupBox;
        private System.Windows.Forms.TableLayoutPanel toolsLayout;
        private System.Windows.Forms.Label rangeLabel;
        private System.Windows.Forms.Label gradientLabel;
        private System.Windows.Forms.Label gradientStartLabel;
        private System.Windows.Forms.Panel gradientStartSwatch;
        private System.Windows.Forms.Label gradientEndLabel;
        private System.Windows.Forms.Panel gradientEndSwatch;
        private System.Windows.Forms.Label gradientCurveLabel;
        private System.Windows.Forms.ComboBox gradientCurveComboBox;
        private System.Windows.Forms.Button gradientApplyButton;
        private System.Windows.Forms.Label rampLabel;
        private System.Windows.Forms.Label rampBaseLabel;
        private System.Windows.Forms.Panel rampSwatch;
        private System.Windows.Forms.Label rampDarkLabel;
        private System.Windows.Forms.NumericUpDown rampDarkNumeric;
        private System.Windows.Forms.Label rampLightLabel;
        private System.Windows.Forms.NumericUpDown rampLightNumeric;
        private System.Windows.Forms.Button rampApplyButton;
        private System.Windows.Forms.Label adjustLabel;
        private System.Windows.Forms.Label adjustHueLabel;
        private System.Windows.Forms.NumericUpDown adjustHueNumeric;
        private System.Windows.Forms.Label adjustSatLabel;
        private System.Windows.Forms.NumericUpDown adjustSatNumeric;
        private System.Windows.Forms.Label adjustLightLabel;
        private System.Windows.Forms.NumericUpDown adjustLightNumeric;
        private System.Windows.Forms.Label adjustRedLabel;
        private System.Windows.Forms.NumericUpDown adjustRedNumeric;
        private System.Windows.Forms.Label adjustGreenLabel;
        private System.Windows.Forms.NumericUpDown adjustGreenNumeric;
        private System.Windows.Forms.Label adjustBlueLabel;
        private System.Windows.Forms.NumericUpDown adjustBlueNumeric;
        private System.Windows.Forms.Button adjustApplyButton;
        private System.Windows.Forms.Label orderLabel;
        private System.Windows.Forms.FlowLayoutPanel orderFlow;
        private System.Windows.Forms.Button reverseButton;
        private System.Windows.Forms.Button copyButton;
        private System.Windows.Forms.Button pasteButton;
        private System.Windows.Forms.Button editAsTextButton;
        private System.Windows.Forms.GroupBox historyGroupBox;
        private System.Windows.Forms.ListBox historyListBox;
        private System.Windows.Forms.TableLayoutPanel bottomLayout;
        private System.Windows.Forms.GroupBox previewGroupBox;
        private System.Windows.Forms.TableLayoutPanel previewLayout;
        private System.Windows.Forms.FlowLayoutPanel previewOptionsFlow;
        private System.Windows.Forms.Label presetLabel;
        private System.Windows.Forms.ComboBox presetComboBox;
        private System.Windows.Forms.Label itemIdLabel;
        private System.Windows.Forms.TextBox itemIdTextBox;
        private System.Windows.Forms.Button itemBrowseButton;
        private System.Windows.Forms.Label gumpIdLabel;
        private System.Windows.Forms.TextBox gumpIdTextBox;
        private System.Windows.Forms.Button gumpBrowseButton;
        private System.Windows.Forms.Label bodyIdLabel;
        private System.Windows.Forms.TextBox bodyIdTextBox;
        private System.Windows.Forms.Button bodyBrowseButton;
        private System.Windows.Forms.Label facingLabel;
        private System.Windows.Forms.TrackBar facingBar;
        private System.Windows.Forms.CheckBox onlyGreyCheckBox;
        private System.Windows.Forms.Label itemCaptionLabel;
        private System.Windows.Forms.Label gumpCaptionLabel;
        private System.Windows.Forms.Label animCaptionLabel;
        private System.Windows.Forms.PictureBox itemPictureBox;
        private System.Windows.Forms.PictureBox gumpPictureBox;
        private UoFiddler.Controls.UserControls.AnimatedPictureBox animPictureBox;
    }
}
