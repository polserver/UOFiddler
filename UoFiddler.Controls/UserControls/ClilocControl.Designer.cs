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

namespace UoFiddler.Controls.UserControls
{
    partial class ClilocControl
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            dataGridView1 = new System.Windows.Forms.DataGridView();
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            copyCliLocNumberToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            copyCliLocTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            addEntryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            deleteEntryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            LangComboBox = new System.Windows.Forms.ToolStripComboBox();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            GotoEntry = new System.Windows.Forms.ToolStripTextBox();
            GotoButton = new System.Windows.Forms.ToolStripButton();
            FindEntry = new System.Windows.Forms.ToolStripTextBox();
            FindButton = new System.Windows.Forms.ToolStripButton();
            RegexToolStripButton = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            ClilocExportButton = new System.Windows.Forms.ToolStripButton();
            toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            cSVToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            tileDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            contextMenuStrip1.SuspendLayout();
            toolStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToResizeRows = false;
            dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.ContextMenuStrip = contextMenuStrip1;
            dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            dataGridView1.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnF2;
            dataGridView1.Location = new System.Drawing.Point(0, 25);
            dataGridView1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            dataGridView1.MultiSelect = false;
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.RowHeadersWidth = 30;
            dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.Size = new System.Drawing.Size(902, 417);
            dataGridView1.TabIndex = 1;
            dataGridView1.CellContentDoubleClick += OnCell_dbClick;
            dataGridView1.ColumnHeaderMouseClick += OnHeaderClicked;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { copyCliLocNumberToolStripMenuItem, copyCliLocTextToolStripMenuItem, toolStripSeparator3, addEntryToolStripMenuItem, deleteEntryToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(186, 98);
            // 
            // copyCliLocNumberToolStripMenuItem
            // 
            copyCliLocNumberToolStripMenuItem.Name = "copyCliLocNumberToolStripMenuItem";
            copyCliLocNumberToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            copyCliLocNumberToolStripMenuItem.Text = "Copy CliLoc Number";
            copyCliLocNumberToolStripMenuItem.Click += OnCLick_CopyClilocNumber;
            // 
            // copyCliLocTextToolStripMenuItem
            // 
            copyCliLocTextToolStripMenuItem.Name = "copyCliLocTextToolStripMenuItem";
            copyCliLocTextToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            copyCliLocTextToolStripMenuItem.Text = "Copy CliLoc Text";
            copyCliLocTextToolStripMenuItem.Click += OnCLick_CopyClilocText;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(182, 6);
            // 
            // addEntryToolStripMenuItem
            // 
            addEntryToolStripMenuItem.Name = "addEntryToolStripMenuItem";
            addEntryToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            addEntryToolStripMenuItem.Text = "Add Entry";
            addEntryToolStripMenuItem.Click += OnClick_AddEntry;
            // 
            // deleteEntryToolStripMenuItem
            // 
            deleteEntryToolStripMenuItem.Name = "deleteEntryToolStripMenuItem";
            deleteEntryToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            deleteEntryToolStripMenuItem.Text = "Delete Entry";
            deleteEntryToolStripMenuItem.Click += OnClick_DeleteEntry;
            // 
            // toolStrip1
            // 
            toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { LangComboBox, toolStripSeparator1, GotoEntry, GotoButton, FindEntry, FindButton, RegexToolStripButton, toolStripSeparator2, toolStripButton1, toolStripSeparator5, ClilocExportButton, toolStripDropDownButton1 });
            toolStrip1.Location = new System.Drawing.Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            toolStrip1.Size = new System.Drawing.Size(902, 25);
            toolStrip1.TabIndex = 2;
            toolStrip1.Text = "toolStrip1";
            // 
            // LangComboBox
            // 
            LangComboBox.Items.AddRange(new object[] { "English", "German", "Custom 1", "Custom 2" });
            LangComboBox.Name = "LangComboBox";
            LangComboBox.Size = new System.Drawing.Size(140, 25);
            LangComboBox.SelectedIndexChanged += OnLangChange;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // GotoEntry
            // 
            GotoEntry.MaxLength = 10;
            GotoEntry.Name = "GotoEntry";
            GotoEntry.Size = new System.Drawing.Size(116, 25);
            GotoEntry.Text = "Enter Number";
            GotoEntry.Enter += GotoEntry_Enter;
            GotoEntry.KeyDown += GotoEntry_KeyDown;
            // 
            // GotoButton
            // 
            GotoButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            GotoButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            GotoButton.Name = "GotoButton";
            GotoButton.Size = new System.Drawing.Size(37, 22);
            GotoButton.Text = "Goto";
            GotoButton.Click += GotoNr;
            // 
            // FindEntry
            // 
            FindEntry.AcceptsTab = true;
            FindEntry.Name = "FindEntry";
            FindEntry.Size = new System.Drawing.Size(163, 25);
            FindEntry.Text = "Enter Text";
            FindEntry.Enter += FindEntry_Enter;
            FindEntry.KeyDown += FindEntry_KeyDown;
            // 
            // FindButton
            // 
            FindButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            FindButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            FindButton.Name = "FindButton";
            FindButton.Size = new System.Drawing.Size(34, 22);
            FindButton.Text = "Find";
            FindButton.Click += FindEntryClick;
            // 
            // RegexToolStripButton
            // 
            RegexToolStripButton.CheckOnClick = true;
            RegexToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            RegexToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            RegexToolStripButton.Name = "RegexToolStripButton";
            RegexToolStripButton.Size = new System.Drawing.Size(129, 22);
            RegexToolStripButton.Text = "Use regular expression";
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButton1
            // 
            toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButton1.Name = "toolStripButton1";
            toolStripButton1.Size = new System.Drawing.Size(35, 22);
            toolStripButton1.Text = "Save";
            toolStripButton1.Click += OnClickSave;
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new System.Drawing.Size(6, 25);
            // 
            // ClilocExportButton
            // 
            ClilocExportButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            ClilocExportButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            ClilocExportButton.Name = "ClilocExportButton";
            ClilocExportButton.Size = new System.Drawing.Size(45, 22);
            ClilocExportButton.Text = "Export";
            ClilocExportButton.Click += OnClickExportCSV;
            // 
            // toolStripDropDownButton1
            // 
            toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { cSVToolStripMenuItem, tileDataToolStripMenuItem });
            toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            toolStripDropDownButton1.Size = new System.Drawing.Size(56, 22);
            toolStripDropDownButton1.Text = "Import";
            // 
            // cSVToolStripMenuItem
            // 
            cSVToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            cSVToolStripMenuItem.Name = "cSVToolStripMenuItem";
            cSVToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            cSVToolStripMenuItem.Text = "CSV";
            cSVToolStripMenuItem.Click += OnClickImportCSV;
            // 
            // tileDataToolStripMenuItem
            // 
            tileDataToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            tileDataToolStripMenuItem.Name = "tileDataToolStripMenuItem";
            tileDataToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            tileDataToolStripMenuItem.Text = "TileData";
            tileDataToolStripMenuItem.Click += TileDataToolStripMenuItem_Click;
            // 
            // ClilocControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(dataGridView1);
            Controls.Add(toolStrip1);
            DoubleBuffered = true;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "ClilocControl";
            Size = new System.Drawing.Size(902, 442);
            Load += OnLoad;
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            contextMenuStrip1.ResumeLayout(false);
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStripMenuItem addEntryToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton ClilocExportButton;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem copyCliLocNumberToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyCliLocTextToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cSVToolStripMenuItem;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.ToolStripMenuItem deleteEntryToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton FindButton;
        private System.Windows.Forms.ToolStripTextBox FindEntry;
        private System.Windows.Forms.ToolStripButton GotoButton;
        private System.Windows.Forms.ToolStripTextBox GotoEntry;
        private System.Windows.Forms.ToolStripComboBox LangComboBox;
        private System.Windows.Forms.ToolStripMenuItem tileDataToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripButton RegexToolStripButton;
    }
}
