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
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            inmul = new System.Windows.Forms.TextBox();
            inidx = new System.Windows.Forms.TextBox();
            label3 = new System.Windows.Forms.Label();
            inmulbtn = new System.Windows.Forms.Button();
            inidxbtn = new System.Windows.Forms.Button();
            multouop = new System.Windows.Forms.Button();
            FileDialog = new System.Windows.Forms.OpenFileDialog();
            outuop = new System.Windows.Forms.TextBox();
            label7 = new System.Windows.Forms.Label();
            multype = new System.Windows.Forms.ComboBox();
            label4 = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            mulMapIndex = new System.Windows.Forms.NumericUpDown();
            outuopbtn = new System.Windows.Forms.Button();
            inuopbtn = new System.Windows.Forms.Button();
            uopMapIndex = new System.Windows.Forms.NumericUpDown();
            label6 = new System.Windows.Forms.Label();
            label8 = new System.Windows.Forms.Label();
            uoptype = new System.Windows.Forms.ComboBox();
            inuop = new System.Windows.Forms.TextBox();
            label9 = new System.Windows.Forms.Label();
            uoptomul = new System.Windows.Forms.Button();
            outidxbtn = new System.Windows.Forms.Button();
            outmulbtn = new System.Windows.Forms.Button();
            outidx = new System.Windows.Forms.TextBox();
            label10 = new System.Windows.Forms.Label();
            outmul = new System.Windows.Forms.TextBox();
            label11 = new System.Windows.Forms.Label();
            label12 = new System.Windows.Forms.Label();
            OperationTypeTabControl = new System.Windows.Forms.TabControl();
            ExtractAllFilesTabPage = new System.Windows.Forms.TabPage();
            StartFolderButton = new System.Windows.Forms.Button();
            ExtractionStatusStrip = new System.Windows.Forms.StatusStrip();
            statustext = new System.Windows.Forms.ToolStripStatusLabel();
            toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            pack = new System.Windows.Forms.RadioButton();
            extract = new System.Windows.Forms.RadioButton();
            label13 = new System.Windows.Forms.Label();
            inputfolder = new System.Windows.Forms.TextBox();
            SelectFolderButton = new System.Windows.Forms.Button();
            ExtractSingleFileTabPage = new System.Windows.Forms.TabPage();
            compressionBox = new System.Windows.Forms.ComboBox();
            MainStatusStrip = new System.Windows.Forms.StatusStrip();
            toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            guilabel = new System.Windows.Forms.ToolStripStatusLabel();
            VersionLabel = new System.Windows.Forms.ToolStripStatusLabel();
            FolderDialog = new System.Windows.Forms.FolderBrowserDialog();
            splitContainer = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)mulMapIndex).BeginInit();
            ((System.ComponentModel.ISupportInitialize)uopMapIndex).BeginInit();
            OperationTypeTabControl.SuspendLayout();
            ExtractAllFilesTabPage.SuspendLayout();
            ExtractionStatusStrip.SuspendLayout();
            ExtractSingleFileTabPage.SuspendLayout();
            MainStatusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(7, 3);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(147, 15);
            label1.TabIndex = 0;
            label1.Text = "Convert from MUL to UOP";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(35, 42);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(63, 15);
            label2.TabIndex = 1;
            label2.Text = "Input MUL";
            // 
            // inmul
            // 
            inmul.BackColor = System.Drawing.Color.White;
            inmul.Location = new System.Drawing.Point(118, 38);
            inmul.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            inmul.Name = "inmul";
            inmul.Size = new System.Drawing.Size(241, 23);
            inmul.TabIndex = 2;
            // 
            // inidx
            // 
            inidx.BackColor = System.Drawing.Color.White;
            inidx.Location = new System.Drawing.Point(118, 68);
            inidx.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            inidx.Name = "inidx";
            inidx.Size = new System.Drawing.Size(241, 23);
            inidx.TabIndex = 4;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(35, 72);
            label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(56, 15);
            label3.TabIndex = 3;
            label3.Text = "Input IDX";
            // 
            // inmulbtn
            // 
            inmulbtn.Location = new System.Drawing.Point(366, 38);
            inmulbtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            inmulbtn.Name = "inmulbtn";
            inmulbtn.Size = new System.Drawing.Size(31, 23);
            inmulbtn.TabIndex = 5;
            inmulbtn.Text = "...";
            inmulbtn.UseVisualStyleBackColor = true;
            inmulbtn.Click += InputMulSelect;
            // 
            // inidxbtn
            // 
            inidxbtn.Location = new System.Drawing.Point(366, 68);
            inidxbtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            inidxbtn.Name = "inidxbtn";
            inidxbtn.Size = new System.Drawing.Size(31, 23);
            inidxbtn.TabIndex = 6;
            inidxbtn.Text = "...";
            inidxbtn.UseVisualStyleBackColor = true;
            inidxbtn.Click += InputIdxSelect;
            // 
            // multouop
            // 
            multouop.Location = new System.Drawing.Point(405, 38);
            multouop.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            multouop.Name = "multouop";
            multouop.Size = new System.Drawing.Size(102, 150);
            multouop.TabIndex = 7;
            multouop.Text = "Convert";
            multouop.UseVisualStyleBackColor = true;
            multouop.Click += ToUop;
            // 
            // FileDialog
            // 
            FileDialog.CheckFileExists = false;
            FileDialog.Filter = "MUL|*.mul|UOP|*.uop|IDX|*.idx";
            // 
            // outuop
            // 
            outuop.BackColor = System.Drawing.Color.White;
            outuop.Location = new System.Drawing.Point(118, 165);
            outuop.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            outuop.Name = "outuop";
            outuop.Size = new System.Drawing.Size(241, 23);
            outuop.TabIndex = 17;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new System.Drawing.Point(35, 168);
            label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(72, 15);
            label7.TabIndex = 16;
            label7.Text = "Output UOP";
            // 
            // multype
            // 
            multype.BackColor = System.Drawing.Color.White;
            multype.Location = new System.Drawing.Point(118, 99);
            multype.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            multype.Name = "multype";
            multype.Size = new System.Drawing.Size(241, 23);
            multype.TabIndex = 19;
            multype.Text = "File Type";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(35, 103);
            label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(61, 15);
            label4.TabIndex = 20;
            label4.Text = "Input type";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(35, 137);
            label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(56, 15);
            label5.TabIndex = 21;
            label5.Text = "Map n# ?";
            // 
            // mulMapIndex
            // 
            mulMapIndex.BackColor = System.Drawing.Color.White;
            mulMapIndex.Location = new System.Drawing.Point(118, 135);
            mulMapIndex.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            mulMapIndex.Maximum = new decimal(new int[] { 5, 0, 0, 0 });
            mulMapIndex.Name = "mulMapIndex";
            mulMapIndex.ReadOnly = true;
            mulMapIndex.Size = new System.Drawing.Size(42, 23);
            mulMapIndex.TabIndex = 22;
            // 
            // outuopbtn
            // 
            outuopbtn.Location = new System.Drawing.Point(366, 165);
            outuopbtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            outuopbtn.Name = "outuopbtn";
            outuopbtn.Size = new System.Drawing.Size(31, 23);
            outuopbtn.TabIndex = 23;
            outuopbtn.Text = "...";
            outuopbtn.UseVisualStyleBackColor = true;
            outuopbtn.Click += OutputUopSelect;
            // 
            // inuopbtn
            // 
            inuopbtn.Location = new System.Drawing.Point(366, 365);
            inuopbtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            inuopbtn.Name = "inuopbtn";
            inuopbtn.Size = new System.Drawing.Size(31, 23);
            inuopbtn.TabIndex = 39;
            inuopbtn.Text = "...";
            inuopbtn.UseVisualStyleBackColor = true;
            inuopbtn.Click += InputUopSelect;
            // 
            // uopMapIndex
            // 
            uopMapIndex.BackColor = System.Drawing.Color.White;
            uopMapIndex.Location = new System.Drawing.Point(118, 335);
            uopMapIndex.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            uopMapIndex.Maximum = new decimal(new int[] { 5, 0, 0, 0 });
            uopMapIndex.Name = "uopMapIndex";
            uopMapIndex.ReadOnly = true;
            uopMapIndex.Size = new System.Drawing.Size(42, 23);
            uopMapIndex.TabIndex = 38;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(35, 337);
            label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(56, 15);
            label6.TabIndex = 37;
            label6.Text = "Map n# ?";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new System.Drawing.Point(35, 302);
            label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(61, 15);
            label8.TabIndex = 36;
            label8.Text = "Input type";
            // 
            // uoptype
            // 
            uoptype.BackColor = System.Drawing.Color.White;
            uoptype.FormattingEnabled = true;
            uoptype.Location = new System.Drawing.Point(118, 299);
            uoptype.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            uoptype.Name = "uoptype";
            uoptype.Size = new System.Drawing.Size(241, 23);
            uoptype.TabIndex = 35;
            uoptype.Text = "File Type";
            // 
            // inuop
            // 
            inuop.BackColor = System.Drawing.Color.White;
            inuop.Location = new System.Drawing.Point(118, 365);
            inuop.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            inuop.Name = "inuop";
            inuop.Size = new System.Drawing.Size(241, 23);
            inuop.TabIndex = 33;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new System.Drawing.Point(35, 368);
            label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(62, 15);
            label9.TabIndex = 32;
            label9.Text = "Input UOP";
            // 
            // uoptomul
            // 
            uoptomul.Location = new System.Drawing.Point(405, 238);
            uoptomul.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            uoptomul.Name = "uoptomul";
            uoptomul.Size = new System.Drawing.Size(102, 150);
            uoptomul.TabIndex = 31;
            uoptomul.Text = "Convert";
            uoptomul.UseVisualStyleBackColor = true;
            uoptomul.Click += ToMul;
            // 
            // outidxbtn
            // 
            outidxbtn.Location = new System.Drawing.Point(366, 268);
            outidxbtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            outidxbtn.Name = "outidxbtn";
            outidxbtn.Size = new System.Drawing.Size(31, 23);
            outidxbtn.TabIndex = 30;
            outidxbtn.Text = "...";
            outidxbtn.UseVisualStyleBackColor = true;
            outidxbtn.Click += OutputIdxSelect;
            // 
            // outmulbtn
            // 
            outmulbtn.Location = new System.Drawing.Point(366, 238);
            outmulbtn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            outmulbtn.Name = "outmulbtn";
            outmulbtn.Size = new System.Drawing.Size(31, 23);
            outmulbtn.TabIndex = 29;
            outmulbtn.Text = "...";
            outmulbtn.UseVisualStyleBackColor = true;
            outmulbtn.Click += OutMulSelect;
            // 
            // outidx
            // 
            outidx.BackColor = System.Drawing.Color.White;
            outidx.Location = new System.Drawing.Point(118, 268);
            outidx.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            outidx.Name = "outidx";
            outidx.Size = new System.Drawing.Size(241, 23);
            outidx.TabIndex = 28;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new System.Drawing.Point(35, 271);
            label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label10.Name = "label10";
            label10.Size = new System.Drawing.Size(66, 15);
            label10.TabIndex = 27;
            label10.Text = "Output IDX";
            // 
            // outmul
            // 
            outmul.BackColor = System.Drawing.Color.White;
            outmul.Location = new System.Drawing.Point(118, 238);
            outmul.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            outmul.Name = "outmul";
            outmul.Size = new System.Drawing.Size(241, 23);
            outmul.TabIndex = 26;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new System.Drawing.Point(35, 241);
            label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label11.Name = "label11";
            label11.Size = new System.Drawing.Size(73, 15);
            label11.TabIndex = 25;
            label11.Text = "Output MUL";
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new System.Drawing.Point(7, 203);
            label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label12.Name = "label12";
            label12.Size = new System.Drawing.Size(147, 15);
            label12.TabIndex = 24;
            label12.Text = "Convert from UOP to MUL";
            // 
            // OperationTypeTabControl
            // 
            OperationTypeTabControl.Controls.Add(ExtractAllFilesTabPage);
            OperationTypeTabControl.Controls.Add(ExtractSingleFileTabPage);
            OperationTypeTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            OperationTypeTabControl.Location = new System.Drawing.Point(0, 0);
            OperationTypeTabControl.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            OperationTypeTabControl.Name = "OperationTypeTabControl";
            OperationTypeTabControl.SelectedIndex = 0;
            OperationTypeTabControl.Size = new System.Drawing.Size(645, 470);
            OperationTypeTabControl.TabIndex = 43;
            // 
            // ExtractAllFilesTabPage
            // 
            ExtractAllFilesTabPage.Controls.Add(StartFolderButton);
            ExtractAllFilesTabPage.Controls.Add(ExtractionStatusStrip);
            ExtractAllFilesTabPage.Controls.Add(pack);
            ExtractAllFilesTabPage.Controls.Add(extract);
            ExtractAllFilesTabPage.Controls.Add(label13);
            ExtractAllFilesTabPage.Controls.Add(inputfolder);
            ExtractAllFilesTabPage.Controls.Add(SelectFolderButton);
            ExtractAllFilesTabPage.Location = new System.Drawing.Point(4, 24);
            ExtractAllFilesTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ExtractAllFilesTabPage.Name = "ExtractAllFilesTabPage";
            ExtractAllFilesTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ExtractAllFilesTabPage.Size = new System.Drawing.Size(637, 442);
            ExtractAllFilesTabPage.TabIndex = 1;
            ExtractAllFilesTabPage.Text = "Every file";
            ExtractAllFilesTabPage.UseVisualStyleBackColor = true;
            // 
            // StartFolderButton
            // 
            StartFolderButton.Location = new System.Drawing.Point(10, 96);
            StartFolderButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            StartFolderButton.Name = "StartFolderButton";
            StartFolderButton.Size = new System.Drawing.Size(326, 27);
            StartFolderButton.TabIndex = 12;
            StartFolderButton.Text = "Start";
            StartFolderButton.UseVisualStyleBackColor = true;
            StartFolderButton.Click += StartFolderButtonClick;
            // 
            // ExtractionStatusStrip
            // 
            ExtractionStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { statustext, toolStripStatusLabel2 });
            ExtractionStatusStrip.Location = new System.Drawing.Point(4, 417);
            ExtractionStatusStrip.Name = "ExtractionStatusStrip";
            ExtractionStatusStrip.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
            ExtractionStatusStrip.Size = new System.Drawing.Size(629, 22);
            ExtractionStatusStrip.TabIndex = 11;
            ExtractionStatusStrip.Text = "statusStrip2";
            // 
            // statustext
            // 
            statustext.ForeColor = System.Drawing.Color.DarkRed;
            statustext.Name = "statustext";
            statustext.Size = new System.Drawing.Size(330, 17);
            statustext.Spring = true;
            statustext.Text = "Status";
            statustext.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripStatusLabel2
            // 
            toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            toolStripStatusLabel2.Size = new System.Drawing.Size(282, 17);
            toolStripStatusLabel2.Text = "The extraction/packing process may take some time";
            // 
            // pack
            // 
            pack.AutoSize = true;
            pack.Location = new System.Drawing.Point(56, 69);
            pack.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pack.Name = "pack";
            pack.Size = new System.Drawing.Size(119, 19);
            pack.TabIndex = 10;
            pack.TabStop = true;
            pack.Text = "Pack MUL to UOP";
            pack.UseVisualStyleBackColor = true;
            // 
            // extract
            // 
            extract.AutoSize = true;
            extract.Location = new System.Drawing.Point(56, 43);
            extract.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            extract.Name = "extract";
            extract.Size = new System.Drawing.Size(130, 19);
            extract.TabIndex = 9;
            extract.TabStop = true;
            extract.Text = "Extract UOP to MUL";
            extract.UseVisualStyleBackColor = true;
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new System.Drawing.Point(7, 16);
            label13.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label13.Name = "label13";
            label13.Size = new System.Drawing.Size(40, 15);
            label13.TabIndex = 6;
            label13.Text = "Folder";
            // 
            // inputfolder
            // 
            inputfolder.BackColor = System.Drawing.Color.White;
            inputfolder.Location = new System.Drawing.Point(56, 13);
            inputfolder.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            inputfolder.Name = "inputfolder";
            inputfolder.Size = new System.Drawing.Size(241, 23);
            inputfolder.TabIndex = 7;
            // 
            // SelectFolderButton
            // 
            SelectFolderButton.Location = new System.Drawing.Point(304, 13);
            SelectFolderButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SelectFolderButton.Name = "SelectFolderButton";
            SelectFolderButton.Size = new System.Drawing.Size(31, 23);
            SelectFolderButton.TabIndex = 8;
            SelectFolderButton.Text = "...";
            SelectFolderButton.UseVisualStyleBackColor = true;
            SelectFolderButton.Click += SelectFolder_Click;
            // 
            // ExtractSingleFileTabPage
            // 
            ExtractSingleFileTabPage.Controls.Add(compressionBox);
            ExtractSingleFileTabPage.Controls.Add(label1);
            ExtractSingleFileTabPage.Controls.Add(label2);
            ExtractSingleFileTabPage.Controls.Add(inmul);
            ExtractSingleFileTabPage.Controls.Add(inuopbtn);
            ExtractSingleFileTabPage.Controls.Add(label3);
            ExtractSingleFileTabPage.Controls.Add(uopMapIndex);
            ExtractSingleFileTabPage.Controls.Add(inidx);
            ExtractSingleFileTabPage.Controls.Add(label6);
            ExtractSingleFileTabPage.Controls.Add(inmulbtn);
            ExtractSingleFileTabPage.Controls.Add(label8);
            ExtractSingleFileTabPage.Controls.Add(inidxbtn);
            ExtractSingleFileTabPage.Controls.Add(uoptype);
            ExtractSingleFileTabPage.Controls.Add(multouop);
            ExtractSingleFileTabPage.Controls.Add(inuop);
            ExtractSingleFileTabPage.Controls.Add(label7);
            ExtractSingleFileTabPage.Controls.Add(label9);
            ExtractSingleFileTabPage.Controls.Add(outuop);
            ExtractSingleFileTabPage.Controls.Add(uoptomul);
            ExtractSingleFileTabPage.Controls.Add(multype);
            ExtractSingleFileTabPage.Controls.Add(outidxbtn);
            ExtractSingleFileTabPage.Controls.Add(label4);
            ExtractSingleFileTabPage.Controls.Add(outmulbtn);
            ExtractSingleFileTabPage.Controls.Add(label5);
            ExtractSingleFileTabPage.Controls.Add(outidx);
            ExtractSingleFileTabPage.Controls.Add(mulMapIndex);
            ExtractSingleFileTabPage.Controls.Add(label10);
            ExtractSingleFileTabPage.Controls.Add(outuopbtn);
            ExtractSingleFileTabPage.Controls.Add(outmul);
            ExtractSingleFileTabPage.Controls.Add(label12);
            ExtractSingleFileTabPage.Controls.Add(label11);
            ExtractSingleFileTabPage.Location = new System.Drawing.Point(4, 24);
            ExtractSingleFileTabPage.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ExtractSingleFileTabPage.Name = "ExtractSingleFileTabPage";
            ExtractSingleFileTabPage.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ExtractSingleFileTabPage.Size = new System.Drawing.Size(637, 442);
            ExtractSingleFileTabPage.TabIndex = 0;
            ExtractSingleFileTabPage.Text = "One file";
            ExtractSingleFileTabPage.UseVisualStyleBackColor = true;
            // 
            // compressionBox
            // 
            compressionBox.BackColor = System.Drawing.Color.White;
            compressionBox.Items.AddRange(new object[] { "None", "Zlib", "Mythic" });
            compressionBox.Location = new System.Drawing.Point(168, 134);
            compressionBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            compressionBox.Name = "compressionBox";
            compressionBox.Size = new System.Drawing.Size(191, 23);
            compressionBox.TabIndex = 40;
            compressionBox.Text = "None";
            // 
            // MainStatusStrip
            // 
            MainStatusStrip.Enabled = false;
            MainStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripStatusLabel1, guilabel, VersionLabel });
            MainStatusStrip.Location = new System.Drawing.Point(0, 8);
            MainStatusStrip.Name = "MainStatusStrip";
            MainStatusStrip.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
            MainStatusStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            MainStatusStrip.Size = new System.Drawing.Size(645, 22);
            MainStatusStrip.Stretch = false;
            MainStatusStrip.TabIndex = 44;
            MainStatusStrip.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new System.Drawing.Size(0, 17);
            // 
            // guilabel
            // 
            guilabel.Name = "guilabel";
            guilabel.Size = new System.Drawing.Size(260, 17);
            guilabel.Text = "RunUO's LegacyMUL Converter v4 3rd party GUI";
            // 
            // VersionLabel
            // 
            VersionLabel.Name = "VersionLabel";
            VersionLabel.Size = new System.Drawing.Size(99, 17);
            VersionLabel.Text = "Unknown Version";
            // 
            // splitContainer
            // 
            splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer.IsSplitterFixed = true;
            splitContainer.Location = new System.Drawing.Point(0, 0);
            splitContainer.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            splitContainer.Name = "splitContainer";
            splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer.Panel1
            // 
            splitContainer.Panel1.Controls.Add(OperationTypeTabControl);
            // 
            // splitContainer.Panel2
            // 
            splitContainer.Panel2.Controls.Add(MainStatusStrip);
            splitContainer.Size = new System.Drawing.Size(645, 505);
            splitContainer.SplitterDistance = 470;
            splitContainer.SplitterWidth = 5;
            splitContainer.TabIndex = 40;
            // 
            // UopPackerControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.Transparent;
            BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            Controls.Add(splitContainer);
            DoubleBuffered = true;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "UopPackerControl";
            Size = new System.Drawing.Size(645, 505);
            ((System.ComponentModel.ISupportInitialize)mulMapIndex).EndInit();
            ((System.ComponentModel.ISupportInitialize)uopMapIndex).EndInit();
            OperationTypeTabControl.ResumeLayout(false);
            ExtractAllFilesTabPage.ResumeLayout(false);
            ExtractAllFilesTabPage.PerformLayout();
            ExtractionStatusStrip.ResumeLayout(false);
            ExtractionStatusStrip.PerformLayout();
            ExtractSingleFileTabPage.ResumeLayout(false);
            ExtractSingleFileTabPage.PerformLayout();
            MainStatusStrip.ResumeLayout(false);
            MainStatusStrip.PerformLayout();
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel2.ResumeLayout(false);
            splitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            ResumeLayout(false);
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
        private System.Windows.Forms.NumericUpDown mulMapIndex;
        private System.Windows.Forms.Button multouop;
        private System.Windows.Forms.ComboBox multype;
        private System.Windows.Forms.TextBox outidx;
        private System.Windows.Forms.Button outidxbtn;
        private System.Windows.Forms.TextBox outmul;
        private System.Windows.Forms.Button outmulbtn;
        private System.Windows.Forms.TextBox outuop;
        private System.Windows.Forms.Button outuopbtn;
        private System.Windows.Forms.RadioButton pack;
        private System.Windows.Forms.OpenFileDialog FileDialog;
        private System.Windows.Forms.FolderBrowserDialog FolderDialog;
        private System.Windows.Forms.Button SelectFolderButton;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.Button StartFolderButton;
        private System.Windows.Forms.StatusStrip MainStatusStrip;
        private System.Windows.Forms.StatusStrip ExtractionStatusStrip;
        private System.Windows.Forms.ToolStripStatusLabel statustext;
        private System.Windows.Forms.TabControl OperationTypeTabControl;
        private System.Windows.Forms.TabPage ExtractSingleFileTabPage;
        private System.Windows.Forms.TabPage ExtractAllFilesTabPage;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.NumericUpDown uopMapIndex;
        private System.Windows.Forms.Button uoptomul;
        private System.Windows.Forms.ComboBox uoptype;
        private System.Windows.Forms.ToolStripStatusLabel VersionLabel;
        private System.Windows.Forms.ComboBox compressionBox;
    }
}
