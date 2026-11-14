namespace UoFiddler.Plugin.MultiEditor.UI
{
    partial class FloorsDialog
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
            hintLabel = new System.Windows.Forms.Label();
            _grid = new System.Windows.Forms.DataGridView();
            columnLevel = new System.Windows.Forms.DataGridViewTextBoxColumn();
            columnStart = new System.Windows.Forms.DataGridViewTextBoxColumn();
            buttonAdd = new System.Windows.Forms.Button();
            buttonRemove = new System.Windows.Forms.Button();
            presetsLabel = new System.Windows.Forms.Label();
            buttonGround = new System.Windows.Forms.Button();
            buttonFoundation = new System.Windows.Forms.Button();
            buttonDetected = new System.Windows.Forms.Button();
            buttonFitted = new System.Windows.Forms.Button();
            buttonOk = new System.Windows.Forms.Button();
            buttonCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)_grid).BeginInit();
            SuspendLayout();
            //
            // hintLabel
            //
            hintLabel.Location = new System.Drawing.Point(12, 10);
            hintLabel.Name = "hintLabel";
            hintLabel.Size = new System.Drawing.Size(446, 34);
            hintLabel.TabIndex = 0;
            hintLabel.Text = "Each level starts at its Z and runs up to the next one. Floors in the client are 20 high; a house on a foundation has its first floor at Z 7.";
            //
            // _grid
            //
            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToDeleteRows = false;
            _grid.AllowUserToResizeRows = false;
            _grid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            _grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { columnLevel, columnStart });
            _grid.Location = new System.Drawing.Point(12, 50);
            _grid.MultiSelect = false;
            _grid.Name = "_grid";
            _grid.RowHeadersVisible = false;
            _grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            _grid.Size = new System.Drawing.Size(270, 280);
            _grid.TabIndex = 1;
            //
            // columnLevel
            //
            columnLevel.FillWeight = 65F;
            columnLevel.HeaderText = "Level";
            columnLevel.Name = "columnLevel";
            //
            // columnStart
            //
            columnStart.FillWeight = 35F;
            columnStart.HeaderText = "Starts at Z";
            columnStart.Name = "columnStart";
            //
            // buttonAdd
            //
            buttonAdd.Location = new System.Drawing.Point(294, 50);
            buttonAdd.Name = "buttonAdd";
            buttonAdd.Size = new System.Drawing.Size(164, 26);
            buttonAdd.TabIndex = 2;
            buttonAdd.Text = "Add a level above";
            buttonAdd.UseVisualStyleBackColor = true;
            buttonAdd.Click += ButtonAdd_Click;
            //
            // buttonRemove
            //
            buttonRemove.Location = new System.Drawing.Point(294, 80);
            buttonRemove.Name = "buttonRemove";
            buttonRemove.Size = new System.Drawing.Size(164, 26);
            buttonRemove.TabIndex = 3;
            buttonRemove.Text = "Remove the selected level";
            buttonRemove.UseVisualStyleBackColor = true;
            buttonRemove.Click += ButtonRemove_Click;
            //
            // presetsLabel
            //
            presetsLabel.AutoSize = true;
            presetsLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            presetsLabel.Location = new System.Drawing.Point(294, 122);
            presetsLabel.Name = "presetsLabel";
            presetsLabel.Size = new System.Drawing.Size(45, 15);
            presetsLabel.TabIndex = 4;
            presetsLabel.Text = "Presets";
            //
            // buttonGround
            //
            buttonGround.Location = new System.Drawing.Point(294, 142);
            buttonGround.Name = "buttonGround";
            buttonGround.Size = new System.Drawing.Size(164, 26);
            buttonGround.TabIndex = 5;
            buttonGround.Text = "From the ground (Z 0)";
            buttonGround.UseVisualStyleBackColor = true;
            buttonGround.Click += ButtonGround_Click;
            //
            // buttonFoundation
            //
            buttonFoundation.Location = new System.Drawing.Point(294, 172);
            buttonFoundation.Name = "buttonFoundation";
            buttonFoundation.Size = new System.Drawing.Size(164, 26);
            buttonFoundation.TabIndex = 6;
            buttonFoundation.Text = "On a foundation (Z 7)";
            buttonFoundation.UseVisualStyleBackColor = true;
            buttonFoundation.Click += ButtonFoundation_Click;
            //
            // buttonDetected
            //
            buttonDetected.Location = new System.Drawing.Point(294, 202);
            buttonDetected.Name = "buttonDetected";
            buttonDetected.Size = new System.Drawing.Size(164, 26);
            buttonDetected.TabIndex = 7;
            buttonDetected.Text = "Detected first floor";
            buttonDetected.UseVisualStyleBackColor = true;
            buttonDetected.Click += ButtonDetected_Click;
            //
            // buttonFitted
            //
            buttonFitted.Location = new System.Drawing.Point(294, 232);
            buttonFitted.Name = "buttonFitted";
            buttonFitted.Size = new System.Drawing.Size(164, 26);
            buttonFitted.TabIndex = 8;
            buttonFitted.Text = "Fit to this design's roof";
            buttonFitted.UseVisualStyleBackColor = true;
            buttonFitted.Click += ButtonFitted_Click;
            //
            // buttonOk
            //
            buttonOk.Location = new System.Drawing.Point(298, 342);
            buttonOk.Name = "buttonOk";
            buttonOk.Size = new System.Drawing.Size(76, 23);
            buttonOk.TabIndex = 9;
            buttonOk.Text = "OK";
            buttonOk.UseVisualStyleBackColor = true;
            buttonOk.Click += ButtonOk_Click;
            //
            // buttonCancel
            //
            buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            buttonCancel.Location = new System.Drawing.Point(382, 342);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new System.Drawing.Size(76, 23);
            buttonCancel.TabIndex = 10;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            //
            // FloorsDialog
            //
            AcceptButton = buttonOk;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new System.Drawing.Size(470, 380);
            Controls.Add(hintLabel);
            Controls.Add(_grid);
            Controls.Add(buttonAdd);
            Controls.Add(buttonRemove);
            Controls.Add(presetsLabel);
            Controls.Add(buttonGround);
            Controls.Add(buttonFoundation);
            Controls.Add(buttonDetected);
            Controls.Add(buttonFitted);
            Controls.Add(buttonOk);
            Controls.Add(buttonCancel);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FloorsDialog";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Floors of this design";
            ((System.ComponentModel.ISupportInitialize)_grid).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label hintLabel;
        private System.Windows.Forms.DataGridView _grid;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnLevel;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnStart;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.Button buttonRemove;
        private System.Windows.Forms.Label presetsLabel;
        private System.Windows.Forms.Button buttonGround;
        private System.Windows.Forms.Button buttonFoundation;
        private System.Windows.Forms.Button buttonDetected;
        private System.Windows.Forms.Button buttonFitted;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
    }
}
