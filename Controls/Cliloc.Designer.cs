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

namespace FiddlerControls
{
    partial class Cliloc
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Cliloc));
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyCliLocNumberToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyCliLocTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.addEntryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteEntryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.LangComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.GotoEntry = new System.Windows.Forms.ToolStripTextBox();
            this.GotoButton = new System.Windows.Forms.ToolStripButton();
            this.FindEntry = new System.Windows.Forms.ToolStripTextBox();
            this.FindButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.ClilocExportButton = new System.Windows.Forms.ToolStripButton();
            this.ClilocImportButton = new System.Windows.Forms.ToolStripButton();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView1.ContextMenuStrip = this.contextMenuStrip1;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnF2;
            this.dataGridView1.Location = new System.Drawing.Point(0, 25);
            this.dataGridView1.MultiSelect = false;
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.dataGridView1.RowHeadersWidth = 30;
            this.dataGridView1.Size = new System.Drawing.Size(619, 299);
            this.dataGridView1.TabIndex = 1;
            this.dataGridView1.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.OnHeaderClicked);
            this.dataGridView1.CellContentDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.onCell_dbClick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyCliLocNumberToolStripMenuItem,
            this.copyCliLocTextToolStripMenuItem,
            this.toolStripSeparator3,
            this.addEntryToolStripMenuItem,
            this.deleteEntryToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(181, 98);
            // 
            // copyCliLocNumberToolStripMenuItem
            // 
            this.copyCliLocNumberToolStripMenuItem.Name = "copyCliLocNumberToolStripMenuItem";
            this.copyCliLocNumberToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.copyCliLocNumberToolStripMenuItem.Text = "Copy CliLoc Number";
            this.copyCliLocNumberToolStripMenuItem.Click += new System.EventHandler(this.OnCLick_CopyClilocNumber);
            // 
            // copyCliLocTextToolStripMenuItem
            // 
            this.copyCliLocTextToolStripMenuItem.Name = "copyCliLocTextToolStripMenuItem";
            this.copyCliLocTextToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.copyCliLocTextToolStripMenuItem.Text = "Copy CliLoc Text";
            this.copyCliLocTextToolStripMenuItem.Click += new System.EventHandler(this.OnCLick_CopyClilocText);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(177, 6);
            // 
            // addEntryToolStripMenuItem
            // 
            this.addEntryToolStripMenuItem.Name = "addEntryToolStripMenuItem";
            this.addEntryToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.addEntryToolStripMenuItem.Text = "Add Entry";
            this.addEntryToolStripMenuItem.Click += new System.EventHandler(this.OnClick_AddEntry);
            // 
            // deleteEntryToolStripMenuItem
            // 
            this.deleteEntryToolStripMenuItem.Name = "deleteEntryToolStripMenuItem";
            this.deleteEntryToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.deleteEntryToolStripMenuItem.Text = "Delete Entry";
            this.deleteEntryToolStripMenuItem.Click += new System.EventHandler(this.OnClick_DeleteEntry);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.LangComboBox,
            this.toolStripSeparator1,
            this.GotoEntry,
            this.GotoButton,
            this.FindEntry,
            this.FindButton,
            this.toolStripSeparator2,
            this.toolStripButton1,
            this.toolStripSeparator5,
            this.ClilocExportButton,
            this.ClilocImportButton});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip1.Size = new System.Drawing.Size(619, 25);
            this.toolStrip1.TabIndex = 2;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // LangComboBox
            // 
            this.LangComboBox.Items.AddRange(new object[] {
            "English",
            "German",
            "Custom 1",
            "Custom 2"});
            this.LangComboBox.Name = "LangComboBox";
            this.LangComboBox.Size = new System.Drawing.Size(121, 25);
            this.LangComboBox.SelectedIndexChanged += new System.EventHandler(this.onLangChange);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // GotoEntry
            // 
            this.GotoEntry.MaxLength = 10;
            this.GotoEntry.Name = "GotoEntry";
            this.GotoEntry.Size = new System.Drawing.Size(100, 25);
            this.GotoEntry.Text = "Goto Nr..";
            // 
            // GotoButton
            // 
            this.GotoButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.GotoButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.GotoButton.Name = "GotoButton";
            this.GotoButton.Size = new System.Drawing.Size(34, 22);
            this.GotoButton.Text = "Goto";
            this.GotoButton.Click += new System.EventHandler(this.GotoNr);
            // 
            // FindEntry
            // 
            this.FindEntry.AcceptsTab = true;
            this.FindEntry.Name = "FindEntry";
            this.FindEntry.ShortcutsEnabled = false;
            this.FindEntry.Size = new System.Drawing.Size(100, 25);
            this.FindEntry.Text = "Entry";
            // 
            // FindButton
            // 
            this.FindButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.FindButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.FindButton.Name = "FindButton";
            this.FindButton.Size = new System.Drawing.Size(31, 22);
            this.FindButton.Text = "Find";
            this.FindButton.Click += new System.EventHandler(this.FindEntryClick);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(35, 22);
            this.toolStripButton1.Text = "Save";
            this.toolStripButton1.Click += new System.EventHandler(this.OnClickSave);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 25);
            // 
            // ClilocExportButton
            // 
            this.ClilocExportButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.ClilocExportButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ClilocExportButton.Name = "ClilocExportButton";
            this.ClilocExportButton.Size = new System.Drawing.Size(43, 22);
            this.ClilocExportButton.Text = "Export";
            this.ClilocExportButton.Click += new System.EventHandler(this.OnClickExportCSV);
            // 
            // ClilocImportButton
            // 
            this.ClilocImportButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.ClilocImportButton.Image = ((System.Drawing.Image)(resources.GetObject("ClilocImportButton.Image")));
            this.ClilocImportButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ClilocImportButton.Name = "ClilocImportButton";
            this.ClilocImportButton.Size = new System.Drawing.Size(43, 22);
            this.ClilocImportButton.Text = "Import";
            this.ClilocImportButton.Click += new System.EventHandler(this.OnClickImportCSV);
            // 
            // Cliloc
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.toolStrip1);
            this.DoubleBuffered = true;
            this.Name = "Cliloc";
            this.Size = new System.Drawing.Size(619, 324);
            this.Load += new System.EventHandler(this.OnLoad);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripComboBox LangComboBox;
        private System.Windows.Forms.ToolStripTextBox GotoEntry;
        private System.Windows.Forms.ToolStripButton GotoButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripTextBox FindEntry;
        private System.Windows.Forms.ToolStripButton FindButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem addEntryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteEntryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyCliLocNumberToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyCliLocTextToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripButton ClilocExportButton;
        private System.Windows.Forms.ToolStripButton ClilocImportButton;
    }
}
