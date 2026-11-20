namespace UoFiddler.Plugin.MapPreview.UI
{
    partial class MapPreviewSettingsDialog
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
            _sizeLabel = new System.Windows.Forms.Label();
            _sizeBox = new System.Windows.Forms.ComboBox();
            _hintLabel = new System.Windows.Forms.Label();
            _showGroup = new System.Windows.Forms.GroupBox();
            _landBox = new System.Windows.Forms.CheckBox();
            _staticsBox = new System.Windows.Forms.CheckBox();
            _stretchBox = new System.Windows.Forms.CheckBox();
            _lightingBox = new System.Windows.Forms.CheckBox();
            _huesBox = new System.Windows.Forms.CheckBox();
            _roofsBox = new System.Windows.Forms.CheckBox();
            _autoCutBox = new System.Windows.Forms.CheckBox();
            _okButton = new System.Windows.Forms.Button();
            _cancelButton = new System.Windows.Forms.Button();
            _showGroup.SuspendLayout();
            SuspendLayout();
            //
            // _sizeLabel
            //
            _sizeLabel.Location = new System.Drawing.Point(12, 15);
            _sizeLabel.Name = "_sizeLabel";
            _sizeLabel.Size = new System.Drawing.Size(110, 20);
            _sizeLabel.TabIndex = 0;
            _sizeLabel.Text = "Area (tiles)";
            //
            // _sizeBox
            //
            _sizeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            _sizeBox.Location = new System.Drawing.Point(128, 13);
            _sizeBox.Name = "_sizeBox";
            _sizeBox.Size = new System.Drawing.Size(164, 23);
            _sizeBox.TabIndex = 1;
            //
            // _hintLabel
            //
            _hintLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            _hintLabel.Location = new System.Drawing.Point(12, 42);
            _hintLabel.Name = "_hintLabel";
            _hintLabel.Size = new System.Drawing.Size(300, 20);
            _hintLabel.TabIndex = 4;
            //
            // _showGroup
            //
            _showGroup.Controls.Add(_landBox);
            _showGroup.Controls.Add(_staticsBox);
            _showGroup.Controls.Add(_stretchBox);
            _showGroup.Controls.Add(_lightingBox);
            _showGroup.Controls.Add(_huesBox);
            _showGroup.Controls.Add(_roofsBox);
            _showGroup.Controls.Add(_autoCutBox);
            _showGroup.Location = new System.Drawing.Point(12, 68);
            _showGroup.Name = "_showGroup";
            _showGroup.Size = new System.Drawing.Size(300, 200);
            _showGroup.TabIndex = 5;
            _showGroup.TabStop = false;
            _showGroup.Text = "Show";
            //
            // _landBox
            //
            _landBox.Location = new System.Drawing.Point(12, 22);
            _landBox.Name = "_landBox";
            _landBox.Size = new System.Drawing.Size(130, 22);
            _landBox.TabIndex = 0;
            _landBox.Text = "Land";
            //
            // _staticsBox
            //
            _staticsBox.Location = new System.Drawing.Point(150, 22);
            _staticsBox.Name = "_staticsBox";
            _staticsBox.Size = new System.Drawing.Size(130, 22);
            _staticsBox.TabIndex = 1;
            _staticsBox.Text = "Statics";
            //
            // _stretchBox
            //
            _stretchBox.Location = new System.Drawing.Point(12, 48);
            _stretchBox.Name = "_stretchBox";
            _stretchBox.Size = new System.Drawing.Size(130, 22);
            _stretchBox.TabIndex = 2;
            _stretchBox.Text = "Stretched land";
            //
            // _lightingBox
            //
            _lightingBox.Location = new System.Drawing.Point(150, 48);
            _lightingBox.Name = "_lightingBox";
            _lightingBox.Size = new System.Drawing.Size(130, 22);
            _lightingBox.TabIndex = 3;
            _lightingBox.Text = "Lighting";
            //
            // _huesBox
            //
            _huesBox.Location = new System.Drawing.Point(12, 74);
            _huesBox.Name = "_huesBox";
            _huesBox.Size = new System.Drawing.Size(130, 22);
            _huesBox.TabIndex = 4;
            _huesBox.Text = "Hues";
            //
            // _roofsBox
            //
            _roofsBox.Location = new System.Drawing.Point(150, 74);
            _roofsBox.Name = "_roofsBox";
            _roofsBox.Size = new System.Drawing.Size(130, 22);
            _roofsBox.TabIndex = 5;
            _roofsBox.Text = "Hide roofs";
            //
            // _autoCutBox
            //
            _autoCutBox.Location = new System.Drawing.Point(12, 106);
            _autoCutBox.Name = "_autoCutBox";
            _autoCutBox.Size = new System.Drawing.Size(280, 84);
            _autoCutBox.TabIndex = 6;
            _autoCutBox.Text = "Cut above the clicked spot on every preview (Max Z = the ground or floor there + 16), as the client does under a roof";
            _autoCutBox.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            _autoCutBox.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            //
            // _okButton
            //
            _okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            _okButton.Location = new System.Drawing.Point(156, 280);
            _okButton.Name = "_okButton";
            _okButton.Size = new System.Drawing.Size(76, 23);
            _okButton.TabIndex = 6;
            _okButton.Text = "OK";
            //
            // _cancelButton
            //
            _cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            _cancelButton.Location = new System.Drawing.Point(236, 280);
            _cancelButton.Name = "_cancelButton";
            _cancelButton.Size = new System.Drawing.Size(76, 23);
            _cancelButton.TabIndex = 7;
            _cancelButton.Text = "Cancel";
            //
            // MapPreviewSettingsDialog
            //
            AcceptButton = _okButton;
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = _cancelButton;
            ClientSize = new System.Drawing.Size(324, 315);
            Controls.Add(_sizeLabel);
            Controls.Add(_sizeBox);
            Controls.Add(_hintLabel);
            Controls.Add(_showGroup);
            Controls.Add(_okButton);
            Controls.Add(_cancelButton);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "MapPreviewSettingsDialog";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "In-game preview settings";
            _showGroup.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label _sizeLabel;
        private System.Windows.Forms.ComboBox _sizeBox;
        private System.Windows.Forms.Label _hintLabel;
        private System.Windows.Forms.GroupBox _showGroup;
        private System.Windows.Forms.CheckBox _landBox;
        private System.Windows.Forms.CheckBox _staticsBox;
        private System.Windows.Forms.CheckBox _stretchBox;
        private System.Windows.Forms.CheckBox _lightingBox;
        private System.Windows.Forms.CheckBox _huesBox;
        private System.Windows.Forms.CheckBox _roofsBox;
        private System.Windows.Forms.CheckBox _autoCutBox;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
    }
}
