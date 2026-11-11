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
    partial class HuePreviewPickerForm
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
                DisposeThumbnails();
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
            rootLayout = new System.Windows.Forms.TableLayoutPanel();
            searchLayout = new System.Windows.Forms.TableLayoutPanel();
            searchLabel = new System.Windows.Forms.Label();
            searchTextBox = new System.Windows.Forms.TextBox();
            countLabel = new System.Windows.Forms.Label();
            tileView = new UoFiddler.Controls.UserControls.TileView.TileViewControl();
            buttonLayout = new System.Windows.Forms.TableLayoutPanel();
            statusLabel = new System.Windows.Forms.Label();
            okButton = new System.Windows.Forms.Button();
            cancelButton = new System.Windows.Forms.Button();
            rootLayout.SuspendLayout();
            searchLayout.SuspendLayout();
            buttonLayout.SuspendLayout();
            SuspendLayout();
            //
            // rootLayout
            //
            rootLayout.ColumnCount = 1;
            rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            rootLayout.Controls.Add(searchLayout, 0, 0);
            rootLayout.Controls.Add(tileView, 0, 1);
            rootLayout.Controls.Add(buttonLayout, 0, 2);
            rootLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            rootLayout.Name = "rootLayout";
            rootLayout.Padding = new System.Windows.Forms.Padding(8);
            rootLayout.RowCount = 3;
            rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            rootLayout.TabIndex = 0;
            //
            // searchLayout
            //
            searchLayout.ColumnCount = 3;
            searchLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            searchLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            searchLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            searchLayout.Controls.Add(searchLabel, 0, 0);
            searchLayout.Controls.Add(searchTextBox, 1, 0);
            searchLayout.Controls.Add(countLabel, 2, 0);
            searchLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            searchLayout.Margin = new System.Windows.Forms.Padding(0);
            searchLayout.Name = "searchLayout";
            searchLayout.RowCount = 1;
            searchLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            searchLayout.TabIndex = 0;
            //
            // searchLabel
            //
            searchLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            searchLabel.AutoSize = true;
            searchLabel.Name = "searchLabel";
            searchLabel.Text = "Filter";
            //
            // searchTextBox
            //
            searchTextBox.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            searchTextBox.Name = "searchTextBox";
            searchTextBox.PlaceholderText = "Show only ids (0x1F03, 1F03 or 7939) or names (robe) that match";
            searchTextBox.TabIndex = 0;
            toolTip.SetToolTip(searchTextBox, "Ids that start with the typed number (hex with or without 0x, or decimal) and names that contain the text. Down moves into the grid, Enter uses the highlighted tile.");
            searchTextBox.TextChanged += OnSearchChanged;
            searchTextBox.KeyDown += OnSearchKeyDown;
            //
            // countLabel
            //
            countLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            countLabel.AutoSize = true;
            countLabel.Name = "countLabel";
            //
            // tileView
            //
            tileView.Dock = System.Windows.Forms.DockStyle.Fill;
            tileView.FocusIndex = -1;
            tileView.MultiSelect = false;
            tileView.Name = "tileView";
            tileView.TabIndex = 1;
            tileView.TileBorderWidth = 1F;
            tileView.TileMargin = new System.Windows.Forms.Padding(2, 2, 0, 0);
            tileView.TilePadding = new System.Windows.Forms.Padding(1);
            tileView.TileSize = new System.Drawing.Size(80, 96);
            tileView.VirtualListSize = 0;
            tileView.DrawItem += OnDrawTile;
            tileView.FocusSelectionChanged += OnTileFocusChanged;
            tileView.MouseDoubleClick += OnTileDoubleClick;
            tileView.KeyDown += OnTileKeyDown;
            //
            // buttonLayout
            //
            buttonLayout.ColumnCount = 3;
            buttonLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            buttonLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            buttonLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            buttonLayout.Controls.Add(statusLabel, 0, 0);
            buttonLayout.Controls.Add(okButton, 1, 0);
            buttonLayout.Controls.Add(cancelButton, 2, 0);
            buttonLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonLayout.Margin = new System.Windows.Forms.Padding(0);
            buttonLayout.Name = "buttonLayout";
            buttonLayout.RowCount = 1;
            buttonLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            buttonLayout.TabIndex = 2;
            //
            // statusLabel
            //
            statusLabel.AutoEllipsis = true;
            statusLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            statusLabel.Name = "statusLabel";
            statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // okButton
            //
            okButton.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            okButton.Name = "okButton";
            okButton.Size = new System.Drawing.Size(84, 27);
            okButton.TabIndex = 0;
            okButton.Text = "Use";
            toolTip.SetToolTip(okButton, "Show the highlighted tile in the preview (Enter or double-click)");
            okButton.Click += OnClickOk;
            //
            // cancelButton
            //
            cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new System.Drawing.Size(84, 27);
            cancelButton.TabIndex = 1;
            cancelButton.Text = "Cancel";
            //
            // HuePreviewPickerForm
            //
            AcceptButton = okButton;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = cancelButton;
            ClientSize = new System.Drawing.Size(784, 561);
            Controls.Add(rootLayout);
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(480, 360);
            Name = "HuePreviewPickerForm";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Pick";
            rootLayout.ResumeLayout(false);
            searchLayout.ResumeLayout(false);
            searchLayout.PerformLayout();
            buttonLayout.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.TableLayoutPanel rootLayout;
        private System.Windows.Forms.TableLayoutPanel searchLayout;
        private System.Windows.Forms.Label searchLabel;
        private System.Windows.Forms.TextBox searchTextBox;
        private System.Windows.Forms.Label countLabel;
        private UoFiddler.Controls.UserControls.TileView.TileViewControl tileView;
        private System.Windows.Forms.TableLayoutPanel buttonLayout;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
    }
}
