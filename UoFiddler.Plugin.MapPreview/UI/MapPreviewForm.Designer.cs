namespace UoFiddler.Plugin.MapPreview.UI
{
    partial class MapPreviewForm
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
            _navStrip = new System.Windows.Forms.ToolStrip();
            _gotoLabel = new System.Windows.Forms.ToolStripLabel();
            _separator5 = new System.Windows.Forms.ToolStripSeparator();
            _viewStrip = new System.Windows.Forms.ToolStrip();
            _landButton = new System.Windows.Forms.ToolStripButton();
            _staticsButton = new System.Windows.Forms.ToolStripButton();
            _stretchButton = new System.Windows.Forms.ToolStripButton();
            _lightingButton = new System.Windows.Forms.ToolStripButton();
            _huesButton = new System.Windows.Forms.ToolStripButton();
            _roofsButton = new System.Windows.Forms.ToolStripButton();
            _separator1 = new System.Windows.Forms.ToolStripSeparator();
            _maxZLabel = new System.Windows.Forms.ToolStripLabel();
            _autoZButton = new System.Windows.Forms.ToolStripButton();
            _cutLandButton = new System.Windows.Forms.ToolStripButton();
            _separator2 = new System.Windows.Forms.ToolStripSeparator();
            _fitButton = new System.Windows.Forms.ToolStripButton();
            _oneToOneButton = new System.Windows.Forms.ToolStripButton();
            _zoomLabel = new System.Windows.Forms.ToolStripLabel();
            _separator3 = new System.Windows.Forms.ToolStripSeparator();
            _moveLabel = new System.Windows.Forms.ToolStripLabel();
            _moveLeftButton = new System.Windows.Forms.ToolStripButton();
            _moveUpButton = new System.Windows.Forms.ToolStripButton();
            _moveDownButton = new System.Windows.Forms.ToolStripButton();
            _moveRightButton = new System.Windows.Forms.ToolStripButton();
            _stepBox = new System.Windows.Forms.ToolStripComboBox();
            _separator4 = new System.Windows.Forms.ToolStripSeparator();
            _exportButton = new System.Windows.Forms.ToolStripButton();
            _refreshButton = new System.Windows.Forms.ToolStripButton();
            _settingsButton = new System.Windows.Forms.ToolStripButton();
            _gotoButton = new System.Windows.Forms.ToolStripButton();
            _gotoBox = new System.Windows.Forms.ToolStripTextBox();
            _statusStrip = new System.Windows.Forms.StatusStrip();
            _statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            _navStrip.SuspendLayout();
            _viewStrip.SuspendLayout();
            _statusStrip.SuspendLayout();
            SuspendLayout();
            //
            // _navStrip
            //
            _navStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            _navStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { _gotoLabel, _gotoBox, _gotoButton, _separator1, _moveLabel, _moveLeftButton, _moveUpButton, _moveDownButton, _moveRightButton, _stepBox, _separator2, _fitButton, _oneToOneButton, _zoomLabel, _settingsButton, _separator5, _refreshButton, _exportButton, _separator3 });
            _navStrip.Location = new System.Drawing.Point(0, 0);
            _navStrip.Name = "_navStrip";
            _navStrip.Size = new System.Drawing.Size(1100, 25);
            _navStrip.TabIndex = 0;
            //
            // _viewStrip
            //
            _viewStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            _viewStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { _landButton, _staticsButton, _stretchButton, _lightingButton, _huesButton, _roofsButton, _separator4, _maxZLabel, _autoZButton, _cutLandButton });
            _viewStrip.Location = new System.Drawing.Point(0, 25);
            _viewStrip.Name = "_viewStrip";
            _viewStrip.Size = new System.Drawing.Size(1100, 25);
            _viewStrip.TabIndex = 1;
            //
            // _gotoLabel
            //
            _gotoLabel.Name = "_gotoLabel";
            _gotoLabel.Text = "Go to:";
            //
            // _separator1
            //
            _separator1.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            _separator1.Name = "_separator1";
            //
            // _separator2
            //
            _separator2.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            _separator2.Name = "_separator2";
            //
            // _separator3
            //
            _separator3.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            _separator3.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            _separator3.Name = "_separator3";
            //
            // _separator4
            //
            _separator4.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            _separator4.Name = "_separator4";
            //
            // _separator5
            //
            _separator5.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            _separator5.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            _separator5.Name = "_separator5";
            //
            // _landButton
            //
            _landButton.Checked = true;
            _landButton.CheckOnClick = true;
            _landButton.CheckState = System.Windows.Forms.CheckState.Checked;
            _landButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            _landButton.Name = "_landButton";
            _landButton.Text = "Land";
            //
            // _staticsButton
            //
            _staticsButton.Checked = true;
            _staticsButton.CheckOnClick = true;
            _staticsButton.CheckState = System.Windows.Forms.CheckState.Checked;
            _staticsButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            _staticsButton.Name = "_staticsButton";
            _staticsButton.Text = "Statics";
            //
            // _stretchButton
            //
            _stretchButton.Checked = true;
            _stretchButton.CheckOnClick = true;
            _stretchButton.CheckState = System.Windows.Forms.CheckState.Checked;
            _stretchButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            _stretchButton.Name = "_stretchButton";
            _stretchButton.Text = "Stretched land";
            _stretchButton.ToolTipText = "Draw sloped land with its texmap stretched over the corner heights";
            //
            // _lightingButton
            //
            _lightingButton.Checked = true;
            _lightingButton.CheckOnClick = true;
            _lightingButton.CheckState = System.Windows.Forms.CheckState.Checked;
            _lightingButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            _lightingButton.Name = "_lightingButton";
            _lightingButton.Text = "Lighting";
            _lightingButton.ToolTipText = "Shade stretched land by its slope";
            //
            // _huesButton
            //
            _huesButton.Checked = true;
            _huesButton.CheckOnClick = true;
            _huesButton.CheckState = System.Windows.Forms.CheckState.Checked;
            _huesButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            _huesButton.Name = "_huesButton";
            _huesButton.Text = "Hues";
            //
            // _roofsButton
            //
            _roofsButton.CheckOnClick = true;
            _roofsButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            _roofsButton.Name = "_roofsButton";
            _roofsButton.Text = "Hide roofs";
            //
            // _maxZLabel
            //
            _maxZLabel.Name = "_maxZLabel";
            _maxZLabel.Text = "Max Z: all";
            //
            // _autoZButton
            //
            _autoZButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            _autoZButton.Name = "_autoZButton";
            _autoZButton.Text = "Auto";
            _autoZButton.ToolTipText = "Cut everything 16 above the ground at the centre of the area, as the client does under a roof";
            //
            // _cutLandButton
            //
            _cutLandButton.CheckOnClick = true;
            _cutLandButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            _cutLandButton.Name = "_cutLandButton";
            _cutLandButton.Text = "Cut land";
            _cutLandButton.ToolTipText = "Max Z hides land too, to show statics under the terrain (the client never cuts land)";
            //
            // _fitButton
            //
            _fitButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            _fitButton.Name = "_fitButton";
            _fitButton.Text = "Fit";
            //
            // _oneToOneButton
            //
            _oneToOneButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            _oneToOneButton.Name = "_oneToOneButton";
            _oneToOneButton.Text = "1:1";
            _oneToOneButton.ToolTipText = "Zoom to 100%";
            //
            // _zoomLabel
            //
            _zoomLabel.Name = "_zoomLabel";
            _zoomLabel.Text = "100%";
            //
            // _moveLabel
            //
            _moveLabel.Name = "_moveLabel";
            _moveLabel.Text = "Move:";
            //
            // _moveLeftButton
            //
            _moveLeftButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            _moveLeftButton.Name = "_moveLeftButton";
            _moveLeftButton.Text = "◀";
            _moveLeftButton.ToolTipText = "Move the area left (Left arrow)";
            //
            // _moveUpButton
            //
            _moveUpButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            _moveUpButton.Name = "_moveUpButton";
            _moveUpButton.Text = "▲";
            _moveUpButton.ToolTipText = "Move the area up (Up arrow)";
            //
            // _moveDownButton
            //
            _moveDownButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            _moveDownButton.Name = "_moveDownButton";
            _moveDownButton.Text = "▼";
            _moveDownButton.ToolTipText = "Move the area down (Down arrow)";
            //
            // _moveRightButton
            //
            _moveRightButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            _moveRightButton.Name = "_moveRightButton";
            _moveRightButton.Text = "▶";
            _moveRightButton.ToolTipText = "Move the area right (Right arrow)";
            //
            // _stepBox
            //
            _stepBox.AutoSize = false;
            _stepBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            _stepBox.Name = "_stepBox";
            _stepBox.Size = new System.Drawing.Size(70, 25);
            _stepBox.ToolTipText = "How far one move goes";
            //
            // _exportButton
            //
            _exportButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            _exportButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            _exportButton.Name = "_exportButton";
            _exportButton.Text = "Export PNG...";
            //
            // _refreshButton
            //
            _refreshButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            _refreshButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            _refreshButton.Name = "_refreshButton";
            _refreshButton.Text = "Refresh";
            _refreshButton.ToolTipText = "Read the area and art again, after edits";
            //
            // _settingsButton
            //
            _settingsButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            _settingsButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            _settingsButton.Name = "_settingsButton";
            _settingsButton.Text = "Settings...";
            //
            // _gotoButton
            //
            _gotoButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            _gotoButton.Name = "_gotoButton";
            _gotoButton.Text = "Go";
            _gotoButton.ToolTipText = "Preview the area around these coordinates";
            //
            // _gotoBox
            //
            _gotoBox.AutoSize = false;
            _gotoBox.Name = "_gotoBox";
            _gotoBox.Size = new System.Drawing.Size(90, 25);
            _gotoBox.ToolTipText = "Go to X, Y (Ctrl+G). Separate with a comma or a space; a Z after them is ignored.";
            //
            // _statusStrip
            //
            _statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { _statusLabel });
            _statusStrip.Location = new System.Drawing.Point(0, 738);
            _statusStrip.Name = "_statusStrip";
            _statusStrip.Size = new System.Drawing.Size(1100, 22);
            _statusStrip.TabIndex = 1;
            //
            // _statusLabel
            //
            _statusLabel.Name = "_statusLabel";
            _statusLabel.Spring = true;
            _statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // MapPreviewForm
            //
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1100, 760);
            Controls.Add(_statusStrip);
            Controls.Add(_viewStrip);
            Controls.Add(_navStrip);
            KeyPreview = true;
            MinimumSize = new System.Drawing.Size(800, 500);
            Name = "MapPreviewForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "In-game preview";
            _navStrip.ResumeLayout(false);
            _navStrip.PerformLayout();
            _viewStrip.ResumeLayout(false);
            _viewStrip.PerformLayout();
            _statusStrip.ResumeLayout(false);
            _statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip _navStrip;
        private System.Windows.Forms.ToolStripLabel _gotoLabel;
        private System.Windows.Forms.ToolStripSeparator _separator5;
        private System.Windows.Forms.ToolStrip _viewStrip;
        private System.Windows.Forms.ToolStripButton _landButton;
        private System.Windows.Forms.ToolStripButton _staticsButton;
        private System.Windows.Forms.ToolStripButton _stretchButton;
        private System.Windows.Forms.ToolStripButton _lightingButton;
        private System.Windows.Forms.ToolStripButton _huesButton;
        private System.Windows.Forms.ToolStripButton _roofsButton;
        private System.Windows.Forms.ToolStripSeparator _separator1;
        private System.Windows.Forms.ToolStripLabel _maxZLabel;
        private System.Windows.Forms.ToolStripButton _autoZButton;
        private System.Windows.Forms.ToolStripButton _cutLandButton;
        private System.Windows.Forms.ToolStripSeparator _separator2;
        private System.Windows.Forms.ToolStripButton _fitButton;
        private System.Windows.Forms.ToolStripButton _oneToOneButton;
        private System.Windows.Forms.ToolStripLabel _zoomLabel;
        private System.Windows.Forms.ToolStripSeparator _separator3;
        private System.Windows.Forms.ToolStripLabel _moveLabel;
        private System.Windows.Forms.ToolStripButton _moveLeftButton;
        private System.Windows.Forms.ToolStripButton _moveUpButton;
        private System.Windows.Forms.ToolStripButton _moveDownButton;
        private System.Windows.Forms.ToolStripButton _moveRightButton;
        private System.Windows.Forms.ToolStripComboBox _stepBox;
        private System.Windows.Forms.ToolStripSeparator _separator4;
        private System.Windows.Forms.ToolStripButton _exportButton;
        private System.Windows.Forms.ToolStripButton _refreshButton;
        private System.Windows.Forms.ToolStripButton _settingsButton;
        private System.Windows.Forms.ToolStripButton _gotoButton;
        private System.Windows.Forms.ToolStripTextBox _gotoBox;
        private System.Windows.Forms.StatusStrip _statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel _statusLabel;
    }
}
