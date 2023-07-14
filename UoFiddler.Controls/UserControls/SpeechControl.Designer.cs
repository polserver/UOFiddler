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
    partial class SpeechControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SpeechControl));
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            IDEntry = new System.Windows.Forms.ToolStripTextBox();
            IDButton = new System.Windows.Forms.ToolStripButton();
            IDNextButton = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            KeyWordEntry = new System.Windows.Forms.ToolStripTextBox();
            KeyWordButton = new System.Windows.Forms.ToolStripButton();
            KeyWordNextButton = new System.Windows.Forms.ToolStripButton();
            toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            dataGridView1 = new System.Windows.Forms.DataGridView();
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            addEntryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            deleteEntryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // toolStrip1
            // 
            toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { IDEntry, IDButton, IDNextButton, toolStripSeparator1, KeyWordEntry, KeyWordButton, KeyWordNextButton, toolStripButton1, toolStripSeparator3, toolStripSeparator2, toolStripDropDownButton1 });
            toolStrip1.Location = new System.Drawing.Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            toolStrip1.Size = new System.Drawing.Size(736, 25);
            toolStrip1.TabIndex = 3;
            toolStrip1.Text = "toolStrip1";
            // 
            // IDEntry
            // 
            IDEntry.MaxLength = 10;
            IDEntry.Name = "IDEntry";
            IDEntry.Size = new System.Drawing.Size(116, 25);
            IDEntry.Text = "Find ID...";
            IDEntry.Enter += IDEntry_Enter;
            // 
            // IDButton
            // 
            IDButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            IDButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            IDButton.Name = "IDButton";
            IDButton.Size = new System.Drawing.Size(34, 22);
            IDButton.Text = "Find";
            IDButton.Click += OnClickFindID;
            // 
            // IDNextButton
            // 
            IDNextButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            IDNextButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            IDNextButton.Name = "IDNextButton";
            IDNextButton.Size = new System.Drawing.Size(62, 22);
            IDNextButton.Text = "Find Next";
            IDNextButton.Click += OnClickNextID;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // KeyWordEntry
            // 
            KeyWordEntry.Name = "KeyWordEntry";
            KeyWordEntry.Size = new System.Drawing.Size(116, 25);
            KeyWordEntry.Text = "KeyWord...";
            KeyWordEntry.Enter += KeyWordEntry_Enter;
            // 
            // KeyWordButton
            // 
            KeyWordButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            KeyWordButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            KeyWordButton.Name = "KeyWordButton";
            KeyWordButton.Size = new System.Drawing.Size(34, 22);
            KeyWordButton.Text = "Find";
            KeyWordButton.Click += OnClickFindKeyWord;
            // 
            // KeyWordNextButton
            // 
            KeyWordNextButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            KeyWordNextButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            KeyWordNextButton.Name = "KeyWordNextButton";
            KeyWordNextButton.Size = new System.Drawing.Size(62, 22);
            KeyWordNextButton.Text = "Find Next";
            KeyWordNextButton.Click += OnClickNextKeyWord;
            // 
            // toolStripButton1
            // 
            toolStripButton1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButton1.Name = "toolStripButton1";
            toolStripButton1.Size = new System.Drawing.Size(35, 22);
            toolStripButton1.Text = "Save";
            toolStripButton1.Click += OnClickSave;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // dataGridView1
            // 
            dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.ContextMenuStrip = contextMenuStrip1;
            dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            dataGridView1.Location = new System.Drawing.Point(0, 25);
            dataGridView1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            dataGridView1.MultiSelect = false;
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.Size = new System.Drawing.Size(736, 355);
            dataGridView1.TabIndex = 4;
            dataGridView1.CellValueChanged += OnCellValueChanged;
            dataGridView1.ColumnHeaderMouseClick += OnHeaderClick;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { addEntryToolStripMenuItem, deleteEntryToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(138, 48);
            // 
            // addEntryToolStripMenuItem
            // 
            addEntryToolStripMenuItem.Name = "addEntryToolStripMenuItem";
            addEntryToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            addEntryToolStripMenuItem.Text = "Add Entry";
            addEntryToolStripMenuItem.Click += OnAddEntry;
            // 
            // deleteEntryToolStripMenuItem
            // 
            deleteEntryToolStripMenuItem.Name = "deleteEntryToolStripMenuItem";
            deleteEntryToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            deleteEntryToolStripMenuItem.Text = "Delete Entry";
            deleteEntryToolStripMenuItem.Click += OnDeleteEntry;
            // 
            // toolStripDropDownButton1
            // 
            toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripMenuItem2, toolStripMenuItem1 });
            toolStripDropDownButton1.Image = (System.Drawing.Image)resources.GetObject("toolStripDropDownButton1.Image");
            toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            toolStripDropDownButton1.Size = new System.Drawing.Size(45, 22);
            toolStripDropDownButton1.Text = "Misc";
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new System.Drawing.Size(163, 22);
            toolStripMenuItem1.Text = "Import from CSV";
            toolStripMenuItem1.Click += OnClickImport;
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.Size = new System.Drawing.Size(163, 22);
            toolStripMenuItem2.Text = "Export to CSV";
            toolStripMenuItem2.Click += OnClickExport;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // SpeechControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(dataGridView1);
            Controls.Add(toolStrip1);
            DoubleBuffered = true;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "SpeechControl";
            Size = new System.Drawing.Size(736, 380);
            Load += OnLoad;
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStripMenuItem addEntryToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.ToolStripMenuItem deleteEntryToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton IDButton;
        private System.Windows.Forms.ToolStripTextBox IDEntry;
        private System.Windows.Forms.ToolStripButton IDNextButton;
        private System.Windows.Forms.ToolStripButton KeyWordButton;
        private System.Windows.Forms.ToolStripTextBox KeyWordEntry;
        private System.Windows.Forms.ToolStripButton KeyWordNextButton;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
    }
}
