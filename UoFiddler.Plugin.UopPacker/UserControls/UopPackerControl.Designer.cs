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

namespace UoFiddler.Plugin.UopPacker.UserControls
{
    partial class UopPackerControl
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.inmul = new System.Windows.Forms.TextBox();
            this.inidx = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.inmulbtn = new System.Windows.Forms.Button();
            this.inidxbtn = new System.Windows.Forms.Button();
            this.multouop = new System.Windows.Forms.Button();
            this.selectfile = new System.Windows.Forms.OpenFileDialog();
            this.outuop = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.multype = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.mulmaptype = new System.Windows.Forms.NumericUpDown();
            this.outuopbtn = new System.Windows.Forms.Button();
            this.inuopbtn = new System.Windows.Forms.Button();
            this.uopmaptype = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.uoptype = new System.Windows.Forms.ComboBox();
            this.inuop = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.uoptomul = new System.Windows.Forms.Button();
            this.outidxbtn = new System.Windows.Forms.Button();
            this.outmulbtn = new System.Windows.Forms.Button();
            this.outidx = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.outmul = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.startfolder = new System.Windows.Forms.Button();
            this.statusStrip2 = new System.Windows.Forms.StatusStrip();
            this.statustext = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.pack = new System.Windows.Forms.RadioButton();
            this.extract = new System.Windows.Forms.RadioButton();
            this.label13 = new System.Windows.Forms.Label();
            this.inputfolder = new System.Windows.Forms.TextBox();
            this.selectfolderbtn = new System.Windows.Forms.Button();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.guilabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.versionlabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.selectfolder = new System.Windows.Forms.FolderBrowserDialog();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.mulmaptype)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.uopmaptype)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.statusStrip2.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 3);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(147, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Convert from MUL to UOP";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(35, 42);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 15);
            this.label2.TabIndex = 1;
            this.label2.Text = "Input MUL";
            // 
            // inmul
            // 
            this.inmul.BackColor = System.Drawing.Color.White;
            this.inmul.Location = new System.Drawing.Point(118, 38);
            this.inmul.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.inmul.Name = "inmul";
            this.inmul.Size = new System.Drawing.Size(241, 23);
            this.inmul.TabIndex = 2;
            // 
            // inidx
            // 
            this.inidx.BackColor = System.Drawing.Color.White;
            this.inidx.Location = new System.Drawing.Point(118, 68);
            this.inidx.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.inidx.Name = "inidx";
            this.inidx.Size = new System.Drawing.Size(241, 23);
            this.inidx.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(35, 72);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 15);
            this.label3.TabIndex = 3;
            this.label3.Text = "Input IDX";
            // 
            // inmulbtn
            // 
            this.inmulbtn.Location = new System.Drawing.Point(366, 38);
            this.inmulbtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.inmulbtn.Name = "inmulbtn";
            this.inmulbtn.Size = new System.Drawing.Size(31, 23);
            this.inmulbtn.TabIndex = 5;
            this.inmulbtn.Text = "...";
            this.inmulbtn.UseVisualStyleBackColor = true;
            this.inmulbtn.Click += new System.EventHandler(this.Inmulselect);
            // 
            // inidxbtn
            // 
            this.inidxbtn.Location = new System.Drawing.Point(366, 68);
            this.inidxbtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.inidxbtn.Name = "inidxbtn";
            this.inidxbtn.Size = new System.Drawing.Size(31, 23);
            this.inidxbtn.TabIndex = 6;
            this.inidxbtn.Text = "...";
            this.inidxbtn.UseVisualStyleBackColor = true;
            this.inidxbtn.Click += new System.EventHandler(this.Inidxselect);
            // 
            // multouop
            // 
            this.multouop.Location = new System.Drawing.Point(405, 38);
            this.multouop.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.multouop.Name = "multouop";
            this.multouop.Size = new System.Drawing.Size(102, 150);
            this.multouop.TabIndex = 7;
            this.multouop.Text = "Convert";
            this.multouop.UseVisualStyleBackColor = true;
            this.multouop.Click += new System.EventHandler(this.Touop);
            // 
            // selectfile
            // 
            this.selectfile.CheckFileExists = false;
            this.selectfile.Filter = "MUL|*.mul|UOP|*.uop|IDX|*.idx";
            // 
            // outuop
            // 
            this.outuop.BackColor = System.Drawing.Color.White;
            this.outuop.Location = new System.Drawing.Point(118, 165);
            this.outuop.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.outuop.Name = "outuop";
            this.outuop.Size = new System.Drawing.Size(241, 23);
            this.outuop.TabIndex = 17;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(35, 168);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(72, 15);
            this.label7.TabIndex = 16;
            this.label7.Text = "Output UOP";
            // 
            // multype
            // 
            this.multype.BackColor = System.Drawing.Color.White;
            this.multype.Location = new System.Drawing.Point(118, 99);
            this.multype.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.multype.Name = "multype";
            this.multype.Size = new System.Drawing.Size(241, 23);
            this.multype.TabIndex = 19;
            this.multype.Text = "File Type";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(35, 103);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(61, 15);
            this.label4.TabIndex = 20;
            this.label4.Text = "Input type";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(35, 137);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(56, 15);
            this.label5.TabIndex = 21;
            this.label5.Text = "Map n# ?";
            // 
            // mulmaptype
            // 
            this.mulmaptype.BackColor = System.Drawing.Color.White;
            this.mulmaptype.Location = new System.Drawing.Point(118, 135);
            this.mulmaptype.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.mulmaptype.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.mulmaptype.Name = "mulmaptype";
            this.mulmaptype.ReadOnly = true;
            this.mulmaptype.Size = new System.Drawing.Size(42, 23);
            this.mulmaptype.TabIndex = 22;
            // 
            // outuopbtn
            // 
            this.outuopbtn.Location = new System.Drawing.Point(366, 165);
            this.outuopbtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.outuopbtn.Name = "outuopbtn";
            this.outuopbtn.Size = new System.Drawing.Size(31, 23);
            this.outuopbtn.TabIndex = 23;
            this.outuopbtn.Text = "...";
            this.outuopbtn.UseVisualStyleBackColor = true;
            this.outuopbtn.Click += new System.EventHandler(this.Outuopselect);
            // 
            // inuopbtn
            // 
            this.inuopbtn.Location = new System.Drawing.Point(366, 365);
            this.inuopbtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.inuopbtn.Name = "inuopbtn";
            this.inuopbtn.Size = new System.Drawing.Size(31, 23);
            this.inuopbtn.TabIndex = 39;
            this.inuopbtn.Text = "...";
            this.inuopbtn.UseVisualStyleBackColor = true;
            this.inuopbtn.Click += new System.EventHandler(this.Inuopselect);
            // 
            // uopmaptype
            // 
            this.uopmaptype.BackColor = System.Drawing.Color.White;
            this.uopmaptype.Location = new System.Drawing.Point(118, 335);
            this.uopmaptype.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.uopmaptype.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.uopmaptype.Name = "uopmaptype";
            this.uopmaptype.ReadOnly = true;
            this.uopmaptype.Size = new System.Drawing.Size(42, 23);
            this.uopmaptype.TabIndex = 38;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(35, 337);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(56, 15);
            this.label6.TabIndex = 37;
            this.label6.Text = "Map n# ?";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(35, 302);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(61, 15);
            this.label8.TabIndex = 36;
            this.label8.Text = "Input type";
            // 
            // uoptype
            // 
            this.uoptype.BackColor = System.Drawing.Color.White;
            this.uoptype.FormattingEnabled = true;
            this.uoptype.Location = new System.Drawing.Point(118, 299);
            this.uoptype.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.uoptype.Name = "uoptype";
            this.uoptype.Size = new System.Drawing.Size(241, 23);
            this.uoptype.TabIndex = 35;
            this.uoptype.Text = "File Type";
            // 
            // inuop
            // 
            this.inuop.BackColor = System.Drawing.Color.White;
            this.inuop.Location = new System.Drawing.Point(118, 365);
            this.inuop.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.inuop.Name = "inuop";
            this.inuop.Size = new System.Drawing.Size(241, 23);
            this.inuop.TabIndex = 33;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(35, 368);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(62, 15);
            this.label9.TabIndex = 32;
            this.label9.Text = "Input UOP";
            // 
            // uoptomul
            // 
            this.uoptomul.Location = new System.Drawing.Point(405, 238);
            this.uoptomul.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.uoptomul.Name = "uoptomul";
            this.uoptomul.Size = new System.Drawing.Size(102, 150);
            this.uoptomul.TabIndex = 31;
            this.uoptomul.Text = "Convert";
            this.uoptomul.UseVisualStyleBackColor = true;
            this.uoptomul.Click += new System.EventHandler(this.Tomul);
            // 
            // outidxbtn
            // 
            this.outidxbtn.Location = new System.Drawing.Point(366, 268);
            this.outidxbtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.outidxbtn.Name = "outidxbtn";
            this.outidxbtn.Size = new System.Drawing.Size(31, 23);
            this.outidxbtn.TabIndex = 30;
            this.outidxbtn.Text = "...";
            this.outidxbtn.UseVisualStyleBackColor = true;
            this.outidxbtn.Click += new System.EventHandler(this.Outidxselect);
            // 
            // outmulbtn
            // 
            this.outmulbtn.Location = new System.Drawing.Point(366, 238);
            this.outmulbtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.outmulbtn.Name = "outmulbtn";
            this.outmulbtn.Size = new System.Drawing.Size(31, 23);
            this.outmulbtn.TabIndex = 29;
            this.outmulbtn.Text = "...";
            this.outmulbtn.UseVisualStyleBackColor = true;
            this.outmulbtn.Click += new System.EventHandler(this.Outmulselect);
            // 
            // outidx
            // 
            this.outidx.BackColor = System.Drawing.Color.White;
            this.outidx.Location = new System.Drawing.Point(118, 268);
            this.outidx.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.outidx.Name = "outidx";
            this.outidx.Size = new System.Drawing.Size(241, 23);
            this.outidx.TabIndex = 28;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(35, 271);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(66, 15);
            this.label10.TabIndex = 27;
            this.label10.Text = "Output IDX";
            // 
            // outmul
            // 
            this.outmul.BackColor = System.Drawing.Color.White;
            this.outmul.Location = new System.Drawing.Point(118, 238);
            this.outmul.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.outmul.Name = "outmul";
            this.outmul.Size = new System.Drawing.Size(241, 23);
            this.outmul.TabIndex = 26;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(35, 241);
            this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(73, 15);
            this.label11.TabIndex = 25;
            this.label11.Text = "Output MUL";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(7, 203);
            this.label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(147, 15);
            this.label12.TabIndex = 24;
            this.label12.Text = "Convert from UOP to MUL";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(644, 496);
            this.tabControl1.TabIndex = 43;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.startfolder);
            this.tabPage2.Controls.Add(this.statusStrip2);
            this.tabPage2.Controls.Add(this.pack);
            this.tabPage2.Controls.Add(this.extract);
            this.tabPage2.Controls.Add(this.label13);
            this.tabPage2.Controls.Add(this.inputfolder);
            this.tabPage2.Controls.Add(this.selectfolderbtn);
            this.tabPage2.Location = new System.Drawing.Point(4, 24);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage2.Size = new System.Drawing.Size(636, 468);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Every file";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // startfolder
            // 
            this.startfolder.Location = new System.Drawing.Point(10, 96);
            this.startfolder.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.startfolder.Name = "startfolder";
            this.startfolder.Size = new System.Drawing.Size(326, 27);
            this.startfolder.TabIndex = 12;
            this.startfolder.Text = "Start";
            this.startfolder.UseVisualStyleBackColor = true;
            this.startfolder.Click += new System.EventHandler(this.Startfolder_Click);
            // 
            // statusStrip2
            // 
            this.statusStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statustext,
            this.toolStripStatusLabel2});
            this.statusStrip2.Location = new System.Drawing.Point(4, 443);
            this.statusStrip2.Name = "statusStrip2";
            this.statusStrip2.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
            this.statusStrip2.Size = new System.Drawing.Size(628, 22);
            this.statusStrip2.TabIndex = 11;
            this.statusStrip2.Text = "statusStrip2";
            // 
            // statustext
            // 
            this.statustext.ForeColor = System.Drawing.Color.DarkRed;
            this.statustext.Name = "statustext";
            this.statustext.Size = new System.Drawing.Size(329, 17);
            this.statustext.Spring = true;
            this.statustext.Text = "Status";
            this.statustext.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(282, 17);
            this.toolStripStatusLabel2.Text = "The extraction/packing process may take some time";
            // 
            // pack
            // 
            this.pack.AutoSize = true;
            this.pack.Location = new System.Drawing.Point(56, 69);
            this.pack.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pack.Name = "pack";
            this.pack.Size = new System.Drawing.Size(119, 19);
            this.pack.TabIndex = 10;
            this.pack.TabStop = true;
            this.pack.Text = "Pack MUL to UOP";
            this.pack.UseVisualStyleBackColor = true;
            // 
            // extract
            // 
            this.extract.AutoSize = true;
            this.extract.Location = new System.Drawing.Point(56, 43);
            this.extract.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.extract.Name = "extract";
            this.extract.Size = new System.Drawing.Size(130, 19);
            this.extract.TabIndex = 9;
            this.extract.TabStop = true;
            this.extract.Text = "Extract UOP to MUL";
            this.extract.UseVisualStyleBackColor = true;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(7, 16);
            this.label13.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(40, 15);
            this.label13.TabIndex = 6;
            this.label13.Text = "Folder";
            // 
            // inputfolder
            // 
            this.inputfolder.BackColor = System.Drawing.Color.White;
            this.inputfolder.Location = new System.Drawing.Point(56, 13);
            this.inputfolder.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.inputfolder.Name = "inputfolder";
            this.inputfolder.Size = new System.Drawing.Size(241, 23);
            this.inputfolder.TabIndex = 7;
            // 
            // selectfolderbtn
            // 
            this.selectfolderbtn.Location = new System.Drawing.Point(304, 13);
            this.selectfolderbtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.selectfolderbtn.Name = "selectfolderbtn";
            this.selectfolderbtn.Size = new System.Drawing.Size(31, 23);
            this.selectfolderbtn.TabIndex = 8;
            this.selectfolderbtn.Text = "...";
            this.selectfolderbtn.UseVisualStyleBackColor = true;
            this.selectfolderbtn.Click += new System.EventHandler(this.Selectfolderbtn_Click);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.inmul);
            this.tabPage1.Controls.Add(this.inuopbtn);
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Controls.Add(this.uopmaptype);
            this.tabPage1.Controls.Add(this.inidx);
            this.tabPage1.Controls.Add(this.label6);
            this.tabPage1.Controls.Add(this.inmulbtn);
            this.tabPage1.Controls.Add(this.label8);
            this.tabPage1.Controls.Add(this.inidxbtn);
            this.tabPage1.Controls.Add(this.uoptype);
            this.tabPage1.Controls.Add(this.multouop);
            this.tabPage1.Controls.Add(this.inuop);
            this.tabPage1.Controls.Add(this.label7);
            this.tabPage1.Controls.Add(this.label9);
            this.tabPage1.Controls.Add(this.outuop);
            this.tabPage1.Controls.Add(this.uoptomul);
            this.tabPage1.Controls.Add(this.multype);
            this.tabPage1.Controls.Add(this.outidxbtn);
            this.tabPage1.Controls.Add(this.label4);
            this.tabPage1.Controls.Add(this.outmulbtn);
            this.tabPage1.Controls.Add(this.label5);
            this.tabPage1.Controls.Add(this.outidx);
            this.tabPage1.Controls.Add(this.mulmaptype);
            this.tabPage1.Controls.Add(this.label10);
            this.tabPage1.Controls.Add(this.outuopbtn);
            this.tabPage1.Controls.Add(this.outmul);
            this.tabPage1.Controls.Add(this.label12);
            this.tabPage1.Controls.Add(this.label11);
            this.tabPage1.Location = new System.Drawing.Point(4, 24);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage1.Size = new System.Drawing.Size(632, 468);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "One file";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Enabled = false;
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.guilabel,
            this.versionlabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 9);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
            this.statusStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.statusStrip1.Size = new System.Drawing.Size(644, 22);
            this.statusStrip1.Stretch = false;
            this.statusStrip1.TabIndex = 44;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 17);
            // 
            // guilabel
            // 
            this.guilabel.Name = "guilabel";
            this.guilabel.Size = new System.Drawing.Size(260, 17);
            this.guilabel.Text = "RunUO\'s LegacyMUL Converter v4 3rd party GUI";
            // 
            // versionlabel
            // 
            this.versionlabel.Name = "versionlabel";
            this.versionlabel.Size = new System.Drawing.Size(99, 17);
            this.versionlabel.Text = "Unknown Version";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tabControl1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.statusStrip1);
            this.splitContainer1.Size = new System.Drawing.Size(644, 532);
            this.splitContainer1.SplitterDistance = 496;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 40;
            // 
            // UopPackerControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Controls.Add(this.splitContainer1);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "UopPackerControl";
            this.Size = new System.Drawing.Size(644, 532);
            ((System.ComponentModel.ISupportInitialize)(this.mulmaptype)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.uopmaptype)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.statusStrip2.ResumeLayout(false);
            this.statusStrip2.PerformLayout();
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RadioButton extract;
        private System.Windows.Forms.ToolStripStatusLabel guilabel;
        private System.Windows.Forms.TextBox inidx;
        private System.Windows.Forms.Button inidxbtn;
        private System.Windows.Forms.TextBox inmul;
        private System.Windows.Forms.Button inmulbtn;
        private System.Windows.Forms.TextBox inputfolder;
        private System.Windows.Forms.TextBox inuop;
        private System.Windows.Forms.Button inuopbtn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.NumericUpDown mulmaptype;
        private System.Windows.Forms.Button multouop;
        private System.Windows.Forms.ComboBox multype;
        private System.Windows.Forms.TextBox outidx;
        private System.Windows.Forms.Button outidxbtn;
        private System.Windows.Forms.TextBox outmul;
        private System.Windows.Forms.Button outmulbtn;
        private System.Windows.Forms.TextBox outuop;
        private System.Windows.Forms.Button outuopbtn;
        private System.Windows.Forms.RadioButton pack;
        private System.Windows.Forms.OpenFileDialog selectfile;
        private System.Windows.Forms.FolderBrowserDialog selectfolder;
        private System.Windows.Forms.Button selectfolderbtn;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button startfolder;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.StatusStrip statusStrip2;
        private System.Windows.Forms.ToolStripStatusLabel statustext;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.NumericUpDown uopmaptype;
        private System.Windows.Forms.Button uoptomul;
        private System.Windows.Forms.ComboBox uoptype;
        private System.Windows.Forms.ToolStripStatusLabel versionlabel;
    }
}
