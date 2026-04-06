namespace UoFiddler.Controls.UserControls
{
    partial class VerdataControl
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            toolStrip = new System.Windows.Forms.ToolStrip();
            buttonReload = new System.Windows.Forms.ToolStripButton();
            buttonLoadFile = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            labelCurrentFile = new System.Windows.Forms.ToolStripLabel();
            splitContainerMain = new System.Windows.Forms.SplitContainer();
            splitContainerLeft = new System.Windows.Forms.SplitContainer();
            panelTypeList = new System.Windows.Forms.Panel();
            listBoxType = new System.Windows.Forms.ListBox();
            labelType = new System.Windows.Forms.Label();
            panelPatchList = new System.Windows.Forms.Panel();
            listBoxPatches = new System.Windows.Forms.ListBox();
            labelCount = new System.Windows.Forms.Label();
            splitContainerRight = new System.Windows.Forms.SplitContainer();
            panelInfo = new System.Windows.Forms.Panel();
            labelDecoded = new System.Windows.Forms.Label();
            labelExtra = new System.Windows.Forms.Label();
            labelLength = new System.Windows.Forms.Label();
            labelLookup = new System.Windows.Forms.Label();
            labelIndex = new System.Windows.Forms.Label();
            labelFile = new System.Windows.Forms.Label();
            panelPreview = new System.Windows.Forms.Panel();
            panelAnimation = new System.Windows.Forms.Panel();
            splitContainerAnim = new System.Windows.Forms.SplitContainer();
            panelActionList = new System.Windows.Forms.Panel();
            listBoxActions = new System.Windows.Forms.ListBox();
            labelActions = new System.Windows.Forms.Label();
            animatedPictureBox = new AnimatedPictureBox();
            panelAnimControls = new System.Windows.Forms.Panel();
            labelDirValue = new System.Windows.Forms.Label();
            trackBarDirection = new System.Windows.Forms.TrackBar();
            labelDir = new System.Windows.Forms.Label();
            buttonPlayStop = new System.Windows.Forms.Button();
            pictureBoxPreview = new System.Windows.Forms.PictureBox();
            richTextBoxDetails = new System.Windows.Forms.RichTextBox();
            panelMulti = new System.Windows.Forms.Panel();
            tabControlMulti = new System.Windows.Forms.TabControl();
            tabPageMultiPreview = new System.Windows.Forms.TabPage();
            panelMultiScroll = new System.Windows.Forms.Panel();
            pictureBoxMulti = new System.Windows.Forms.PictureBox();
            tabPageMultiComponents = new System.Windows.Forms.TabPage();
            richTextBoxMultiComponents = new System.Windows.Forms.RichTextBox();
            toolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).BeginInit();
            splitContainerMain.Panel1.SuspendLayout();
            splitContainerMain.Panel2.SuspendLayout();
            splitContainerMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerLeft).BeginInit();
            splitContainerLeft.Panel1.SuspendLayout();
            splitContainerLeft.Panel2.SuspendLayout();
            splitContainerLeft.SuspendLayout();
            panelTypeList.SuspendLayout();
            panelPatchList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerRight).BeginInit();
            splitContainerRight.Panel1.SuspendLayout();
            splitContainerRight.Panel2.SuspendLayout();
            splitContainerRight.SuspendLayout();
            panelInfo.SuspendLayout();
            panelPreview.SuspendLayout();
            panelAnimation.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerAnim).BeginInit();
            splitContainerAnim.Panel1.SuspendLayout();
            splitContainerAnim.Panel2.SuspendLayout();
            splitContainerAnim.SuspendLayout();
            panelActionList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)animatedPictureBox).BeginInit();
            panelAnimControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarDirection).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxPreview).BeginInit();
            panelMulti.SuspendLayout();
            tabControlMulti.SuspendLayout();
            tabPageMultiPreview.SuspendLayout();
            panelMultiScroll.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxMulti).BeginInit();
            tabPageMultiComponents.SuspendLayout();
            SuspendLayout();
            // 
            // toolStrip
            // 
            toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { buttonReload, buttonLoadFile, toolStripSeparator1, labelCurrentFile });
            toolStrip.Location = new System.Drawing.Point(0, 0);
            toolStrip.Name = "toolStrip";
            toolStrip.Size = new System.Drawing.Size(900, 25);
            toolStrip.TabIndex = 0;
            // 
            // buttonReload
            // 
            buttonReload.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            buttonReload.Name = "buttonReload";
            buttonReload.Size = new System.Drawing.Size(47, 22);
            buttonReload.Text = "Reload";
            buttonReload.Click += OnClickReload;
            //
            // buttonLoadFile
            //
            buttonLoadFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            buttonLoadFile.Name = "buttonLoadFile";
            buttonLoadFile.Size = new System.Drawing.Size(60, 22);
            buttonLoadFile.Text = "Load File…";
            buttonLoadFile.Click += OnClickLoadFile;
            //
            // toolStripSeparator1
            //
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            //
            // labelCurrentFile
            //
            labelCurrentFile.Name = "labelCurrentFile";
            labelCurrentFile.Size = new System.Drawing.Size(38, 22);
            labelCurrentFile.Text = "";
            //
            // splitContainerMain
            // 
            splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainerMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            splitContainerMain.Location = new System.Drawing.Point(0, 25);
            splitContainerMain.Name = "splitContainerMain";
            // 
            // splitContainerMain.Panel1
            // 
            splitContainerMain.Panel1.Controls.Add(splitContainerLeft);
            // 
            // splitContainerMain.Panel2
            // 
            splitContainerMain.Panel2.Controls.Add(splitContainerRight);
            splitContainerMain.Size = new System.Drawing.Size(900, 575);
            splitContainerMain.SplitterDistance = 300;
            splitContainerMain.TabIndex = 1;
            // 
            // splitContainerLeft
            // 
            splitContainerLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainerLeft.Location = new System.Drawing.Point(0, 0);
            splitContainerLeft.Name = "splitContainerLeft";
            splitContainerLeft.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerLeft.Panel1
            // 
            splitContainerLeft.Panel1.Controls.Add(panelTypeList);
            // 
            // splitContainerLeft.Panel2
            // 
            splitContainerLeft.Panel2.Controls.Add(panelPatchList);
            splitContainerLeft.Size = new System.Drawing.Size(300, 575);
            splitContainerLeft.SplitterDistance = 140;
            splitContainerLeft.TabIndex = 0;
            // 
            // panelTypeList
            // 
            panelTypeList.Controls.Add(listBoxType);
            panelTypeList.Controls.Add(labelType);
            panelTypeList.Dock = System.Windows.Forms.DockStyle.Fill;
            panelTypeList.Location = new System.Drawing.Point(0, 0);
            panelTypeList.Name = "panelTypeList";
            panelTypeList.Padding = new System.Windows.Forms.Padding(4);
            panelTypeList.Size = new System.Drawing.Size(300, 140);
            panelTypeList.TabIndex = 0;
            // 
            // listBoxType
            // 
            listBoxType.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            listBoxType.FormattingEnabled = true;
            listBoxType.IntegralHeight = false;
            listBoxType.ItemHeight = 15;
            listBoxType.Location = new System.Drawing.Point(4, 22);
            listBoxType.Name = "listBoxType";
            listBoxType.Size = new System.Drawing.Size(292, 114);
            listBoxType.TabIndex = 1;
            listBoxType.SelectedIndexChanged += OnTypeSelected;
            // 
            // labelType
            // 
            labelType.AutoSize = true;
            labelType.Location = new System.Drawing.Point(4, 4);
            labelType.Name = "labelType";
            labelType.Size = new System.Drawing.Size(67, 15);
            labelType.TabIndex = 0;
            labelType.Text = "Patch Type:";
            // 
            // panelPatchList
            // 
            panelPatchList.Controls.Add(listBoxPatches);
            panelPatchList.Controls.Add(labelCount);
            panelPatchList.Dock = System.Windows.Forms.DockStyle.Fill;
            panelPatchList.Location = new System.Drawing.Point(0, 0);
            panelPatchList.Name = "panelPatchList";
            panelPatchList.Padding = new System.Windows.Forms.Padding(4);
            panelPatchList.Size = new System.Drawing.Size(300, 431);
            panelPatchList.TabIndex = 0;
            // 
            // listBoxPatches
            // 
            listBoxPatches.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            listBoxPatches.FormattingEnabled = true;
            listBoxPatches.IntegralHeight = false;
            listBoxPatches.ItemHeight = 15;
            listBoxPatches.Location = new System.Drawing.Point(4, 22);
            listBoxPatches.Name = "listBoxPatches";
            listBoxPatches.Size = new System.Drawing.Size(292, 405);
            listBoxPatches.TabIndex = 1;
            listBoxPatches.SelectedIndexChanged += OnPatchSelected;
            // 
            // labelCount
            // 
            labelCount.AutoSize = true;
            labelCount.Location = new System.Drawing.Point(4, 4);
            labelCount.Name = "labelCount";
            labelCount.Size = new System.Drawing.Size(51, 15);
            labelCount.TabIndex = 0;
            labelCount.Text = "Patches:";
            // 
            // splitContainerRight
            // 
            splitContainerRight.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainerRight.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            splitContainerRight.Location = new System.Drawing.Point(0, 0);
            splitContainerRight.Name = "splitContainerRight";
            splitContainerRight.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerRight.Panel1
            // 
            splitContainerRight.Panel1.Controls.Add(panelInfo);
            // 
            // splitContainerRight.Panel2
            // 
            splitContainerRight.Panel2.Controls.Add(panelPreview);
            splitContainerRight.Size = new System.Drawing.Size(596, 575);
            splitContainerRight.SplitterDistance = 135;
            splitContainerRight.TabIndex = 0;
            // 
            // panelInfo
            // 
            panelInfo.Controls.Add(labelDecoded);
            panelInfo.Controls.Add(labelExtra);
            panelInfo.Controls.Add(labelLength);
            panelInfo.Controls.Add(labelLookup);
            panelInfo.Controls.Add(labelIndex);
            panelInfo.Controls.Add(labelFile);
            panelInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            panelInfo.Location = new System.Drawing.Point(0, 0);
            panelInfo.Name = "panelInfo";
            panelInfo.Padding = new System.Windows.Forms.Padding(8);
            panelInfo.Size = new System.Drawing.Size(596, 135);
            panelInfo.TabIndex = 0;
            // 
            // labelDecoded
            // 
            labelDecoded.AutoSize = true;
            labelDecoded.Font = new System.Drawing.Font("Consolas", 9F);
            labelDecoded.ForeColor = System.Drawing.Color.DarkBlue;
            labelDecoded.Location = new System.Drawing.Point(8, 98);
            labelDecoded.Name = "labelDecoded";
            labelDecoded.Size = new System.Drawing.Size(0, 14);
            labelDecoded.TabIndex = 5;
            // 
            // labelExtra
            // 
            labelExtra.AutoSize = true;
            labelExtra.Font = new System.Drawing.Font("Consolas", 9F);
            labelExtra.Location = new System.Drawing.Point(8, 80);
            labelExtra.Name = "labelExtra";
            labelExtra.Size = new System.Drawing.Size(49, 14);
            labelExtra.TabIndex = 4;
            labelExtra.Text = "Extra:";
            // 
            // labelLength
            // 
            labelLength.AutoSize = true;
            labelLength.Font = new System.Drawing.Font("Consolas", 9F);
            labelLength.Location = new System.Drawing.Point(8, 62);
            labelLength.Name = "labelLength";
            labelLength.Size = new System.Drawing.Size(56, 14);
            labelLength.TabIndex = 3;
            labelLength.Text = "Length:";
            // 
            // labelLookup
            // 
            labelLookup.AutoSize = true;
            labelLookup.Font = new System.Drawing.Font("Consolas", 9F);
            labelLookup.Location = new System.Drawing.Point(8, 44);
            labelLookup.Name = "labelLookup";
            labelLookup.Size = new System.Drawing.Size(56, 14);
            labelLookup.TabIndex = 2;
            labelLookup.Text = "Lookup:";
            // 
            // labelIndex
            // 
            labelIndex.AutoSize = true;
            labelIndex.Font = new System.Drawing.Font("Consolas", 9F);
            labelIndex.Location = new System.Drawing.Point(8, 26);
            labelIndex.Name = "labelIndex";
            labelIndex.Size = new System.Drawing.Size(49, 14);
            labelIndex.TabIndex = 1;
            labelIndex.Text = "Index:";
            // 
            // labelFile
            // 
            labelFile.AutoSize = true;
            labelFile.Font = new System.Drawing.Font("Consolas", 9F);
            labelFile.Location = new System.Drawing.Point(8, 8);
            labelFile.Name = "labelFile";
            labelFile.Size = new System.Drawing.Size(42, 14);
            labelFile.TabIndex = 0;
            labelFile.Text = "File:";
            // 
            // panelPreview
            // 
            panelPreview.Controls.Add(panelMulti);
            panelPreview.Controls.Add(panelAnimation);
            panelPreview.Controls.Add(pictureBoxPreview);
            panelPreview.Controls.Add(richTextBoxDetails);
            panelPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            panelPreview.Location = new System.Drawing.Point(0, 0);
            panelPreview.Name = "panelPreview";
            panelPreview.Size = new System.Drawing.Size(596, 436);
            panelPreview.TabIndex = 0;
            // 
            // panelAnimation
            // 
            panelAnimation.Controls.Add(splitContainerAnim);
            panelAnimation.Controls.Add(panelAnimControls);
            panelAnimation.Dock = System.Windows.Forms.DockStyle.Fill;
            panelAnimation.Location = new System.Drawing.Point(0, 0);
            panelAnimation.Name = "panelAnimation";
            panelAnimation.Size = new System.Drawing.Size(596, 436);
            panelAnimation.TabIndex = 2;
            panelAnimation.Visible = false;
            // 
            // splitContainerAnim
            // 
            splitContainerAnim.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainerAnim.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            splitContainerAnim.Location = new System.Drawing.Point(0, 30);
            splitContainerAnim.Name = "splitContainerAnim";
            // 
            // splitContainerAnim.Panel1
            // 
            splitContainerAnim.Panel1.Controls.Add(panelActionList);
            // 
            // splitContainerAnim.Panel2
            // 
            splitContainerAnim.Panel2.Controls.Add(animatedPictureBox);
            splitContainerAnim.Size = new System.Drawing.Size(596, 406);
            splitContainerAnim.SplitterDistance = 130;
            splitContainerAnim.TabIndex = 1;
            // 
            // panelActionList
            // 
            panelActionList.Controls.Add(listBoxActions);
            panelActionList.Controls.Add(labelActions);
            panelActionList.Dock = System.Windows.Forms.DockStyle.Fill;
            panelActionList.Location = new System.Drawing.Point(0, 0);
            panelActionList.Name = "panelActionList";
            panelActionList.Padding = new System.Windows.Forms.Padding(2);
            panelActionList.Size = new System.Drawing.Size(130, 406);
            panelActionList.TabIndex = 0;
            // 
            // listBoxActions
            // 
            listBoxActions.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            listBoxActions.FormattingEnabled = true;
            listBoxActions.IntegralHeight = false;
            listBoxActions.ItemHeight = 15;
            listBoxActions.Location = new System.Drawing.Point(2, 20);
            listBoxActions.Name = "listBoxActions";
            listBoxActions.Size = new System.Drawing.Size(126, 384);
            listBoxActions.TabIndex = 1;
            listBoxActions.SelectedIndexChanged += OnActionSelected;
            // 
            // labelActions
            // 
            labelActions.AutoSize = true;
            labelActions.Location = new System.Drawing.Point(2, 2);
            labelActions.Name = "labelActions";
            labelActions.Size = new System.Drawing.Size(50, 15);
            labelActions.TabIndex = 0;
            labelActions.Text = "Actions:";
            // 
            // animatedPictureBox
            // 
            animatedPictureBox.Animate = false;
            animatedPictureBox.BackColor = System.Drawing.Color.White;
            animatedPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            animatedPictureBox.FrameDelay = 150;
            animatedPictureBox.FrameIndex = 0;
            animatedPictureBox.Location = new System.Drawing.Point(0, 0);
            animatedPictureBox.Name = "animatedPictureBox";
            animatedPictureBox.ShowFrameBounds = false;
            animatedPictureBox.Size = new System.Drawing.Size(462, 406);
            animatedPictureBox.TabIndex = 0;
            animatedPictureBox.TabStop = false;
            // 
            // panelAnimControls
            // 
            panelAnimControls.Controls.Add(labelDirValue);
            panelAnimControls.Controls.Add(trackBarDirection);
            panelAnimControls.Controls.Add(labelDir);
            panelAnimControls.Controls.Add(buttonPlayStop);
            panelAnimControls.Dock = System.Windows.Forms.DockStyle.Top;
            panelAnimControls.Location = new System.Drawing.Point(0, 0);
            panelAnimControls.Name = "panelAnimControls";
            panelAnimControls.Size = new System.Drawing.Size(596, 30);
            panelAnimControls.TabIndex = 0;
            // 
            // labelDirValue
            // 
            labelDirValue.AutoSize = true;
            labelDirValue.Location = new System.Drawing.Point(258, 8);
            labelDirValue.Name = "labelDirValue";
            labelDirValue.Size = new System.Drawing.Size(13, 15);
            labelDirValue.TabIndex = 3;
            labelDirValue.Text = "0";
            // 
            // trackBarDirection
            // 
            trackBarDirection.AutoSize = false;
            trackBarDirection.LargeChange = 1;
            trackBarDirection.Location = new System.Drawing.Point(132, 3);
            trackBarDirection.Maximum = 4;
            trackBarDirection.Name = "trackBarDirection";
            trackBarDirection.Size = new System.Drawing.Size(120, 24);
            trackBarDirection.TabIndex = 2;
            trackBarDirection.Scroll += OnScrollDirection;
            // 
            // labelDir
            // 
            labelDir.AutoSize = true;
            labelDir.Location = new System.Drawing.Point(68, 8);
            labelDir.Name = "labelDir";
            labelDir.Size = new System.Drawing.Size(58, 15);
            labelDir.TabIndex = 1;
            labelDir.Text = "Direction:";
            // 
            // buttonPlayStop
            // 
            buttonPlayStop.Location = new System.Drawing.Point(0, 3);
            buttonPlayStop.Name = "buttonPlayStop";
            buttonPlayStop.Size = new System.Drawing.Size(60, 24);
            buttonPlayStop.TabIndex = 0;
            buttonPlayStop.Text = "Play";
            buttonPlayStop.UseVisualStyleBackColor = true;
            buttonPlayStop.Click += OnClickPlayStop;
            // 
            // pictureBoxPreview
            // 
            pictureBoxPreview.BackColor = System.Drawing.Color.White;
            pictureBoxPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            pictureBoxPreview.Location = new System.Drawing.Point(0, 0);
            pictureBoxPreview.Name = "pictureBoxPreview";
            pictureBoxPreview.Size = new System.Drawing.Size(596, 436);
            pictureBoxPreview.TabIndex = 1;
            pictureBoxPreview.TabStop = false;
            pictureBoxPreview.Visible = false;
            // 
            // richTextBoxDetails
            // 
            richTextBoxDetails.BackColor = System.Drawing.SystemColors.Window;
            richTextBoxDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            richTextBoxDetails.Font = new System.Drawing.Font("Consolas", 9F);
            richTextBoxDetails.Location = new System.Drawing.Point(0, 0);
            richTextBoxDetails.Name = "richTextBoxDetails";
            richTextBoxDetails.ReadOnly = true;
            richTextBoxDetails.Size = new System.Drawing.Size(596, 436);
            richTextBoxDetails.TabIndex = 0;
            richTextBoxDetails.Text = "";
            //
            // panelMulti
            //
            panelMulti.Controls.Add(tabControlMulti);
            panelMulti.Dock = System.Windows.Forms.DockStyle.Fill;
            panelMulti.Location = new System.Drawing.Point(0, 0);
            panelMulti.Name = "panelMulti";
            panelMulti.Size = new System.Drawing.Size(596, 436);
            panelMulti.TabIndex = 3;
            panelMulti.Visible = false;
            //
            // tabControlMulti
            //
            tabControlMulti.Controls.Add(tabPageMultiPreview);
            tabControlMulti.Controls.Add(tabPageMultiComponents);
            tabControlMulti.Dock = System.Windows.Forms.DockStyle.Fill;
            tabControlMulti.Location = new System.Drawing.Point(0, 0);
            tabControlMulti.Name = "tabControlMulti";
            tabControlMulti.Size = new System.Drawing.Size(596, 436);
            tabControlMulti.TabIndex = 0;
            //
            // tabPageMultiPreview
            //
            tabPageMultiPreview.Controls.Add(panelMultiScroll);
            tabPageMultiPreview.Location = new System.Drawing.Point(4, 24);
            tabPageMultiPreview.Name = "tabPageMultiPreview";
            tabPageMultiPreview.Size = new System.Drawing.Size(588, 408);
            tabPageMultiPreview.TabIndex = 0;
            tabPageMultiPreview.Text = "Preview";
            tabPageMultiPreview.UseVisualStyleBackColor = true;
            //
            // panelMultiScroll
            //
            panelMultiScroll.AutoScroll = true;
            panelMultiScroll.BackColor = System.Drawing.Color.White;
            panelMultiScroll.Controls.Add(pictureBoxMulti);
            panelMultiScroll.Dock = System.Windows.Forms.DockStyle.Fill;
            panelMultiScroll.Location = new System.Drawing.Point(0, 0);
            panelMultiScroll.Name = "panelMultiScroll";
            panelMultiScroll.Size = new System.Drawing.Size(588, 408);
            panelMultiScroll.TabIndex = 0;
            //
            // pictureBoxMulti
            //
            pictureBoxMulti.Location = new System.Drawing.Point(0, 0);
            pictureBoxMulti.Name = "pictureBoxMulti";
            pictureBoxMulti.Size = new System.Drawing.Size(100, 100);
            pictureBoxMulti.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            pictureBoxMulti.TabIndex = 0;
            pictureBoxMulti.TabStop = false;
            //
            // tabPageMultiComponents
            //
            tabPageMultiComponents.Controls.Add(richTextBoxMultiComponents);
            tabPageMultiComponents.Location = new System.Drawing.Point(4, 24);
            tabPageMultiComponents.Name = "tabPageMultiComponents";
            tabPageMultiComponents.Size = new System.Drawing.Size(588, 408);
            tabPageMultiComponents.TabIndex = 1;
            tabPageMultiComponents.Text = "Components";
            tabPageMultiComponents.UseVisualStyleBackColor = true;
            //
            // richTextBoxMultiComponents
            //
            richTextBoxMultiComponents.BackColor = System.Drawing.SystemColors.Window;
            richTextBoxMultiComponents.Dock = System.Windows.Forms.DockStyle.Fill;
            richTextBoxMultiComponents.Font = new System.Drawing.Font("Consolas", 9F);
            richTextBoxMultiComponents.Location = new System.Drawing.Point(0, 0);
            richTextBoxMultiComponents.Name = "richTextBoxMultiComponents";
            richTextBoxMultiComponents.ReadOnly = true;
            richTextBoxMultiComponents.Size = new System.Drawing.Size(588, 408);
            richTextBoxMultiComponents.TabIndex = 0;
            richTextBoxMultiComponents.Text = "";
            //
            // VerdataControl
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(splitContainerMain);
            Controls.Add(toolStrip);
            Name = "VerdataControl";
            Size = new System.Drawing.Size(900, 600);
            Load += OnLoad;
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            splitContainerMain.Panel1.ResumeLayout(false);
            splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).EndInit();
            splitContainerMain.ResumeLayout(false);
            splitContainerLeft.Panel1.ResumeLayout(false);
            splitContainerLeft.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerLeft).EndInit();
            splitContainerLeft.ResumeLayout(false);
            panelTypeList.ResumeLayout(false);
            panelTypeList.PerformLayout();
            panelPatchList.ResumeLayout(false);
            panelPatchList.PerformLayout();
            splitContainerRight.Panel1.ResumeLayout(false);
            splitContainerRight.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerRight).EndInit();
            splitContainerRight.ResumeLayout(false);
            panelInfo.ResumeLayout(false);
            panelInfo.PerformLayout();
            panelPreview.ResumeLayout(false);
            panelAnimation.ResumeLayout(false);
            splitContainerAnim.Panel1.ResumeLayout(false);
            splitContainerAnim.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerAnim).EndInit();
            splitContainerAnim.ResumeLayout(false);
            panelActionList.ResumeLayout(false);
            panelActionList.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)animatedPictureBox).EndInit();
            panelAnimControls.ResumeLayout(false);
            panelAnimControls.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trackBarDirection).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxPreview).EndInit();
            panelMulti.ResumeLayout(false);
            tabControlMulti.ResumeLayout(false);
            tabPageMultiPreview.ResumeLayout(false);
            panelMultiScroll.ResumeLayout(false);
            panelMultiScroll.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxMulti).EndInit();
            tabPageMultiComponents.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton buttonReload;
        private System.Windows.Forms.ToolStripButton buttonLoadFile;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripLabel labelCurrentFile;
        private System.Windows.Forms.SplitContainer splitContainerMain;
        private System.Windows.Forms.SplitContainer splitContainerLeft;
        private System.Windows.Forms.Panel panelTypeList;
        private System.Windows.Forms.Label labelType;
        private System.Windows.Forms.ListBox listBoxType;
        private System.Windows.Forms.Panel panelPatchList;
        private System.Windows.Forms.Label labelCount;
        private System.Windows.Forms.ListBox listBoxPatches;
        private System.Windows.Forms.SplitContainer splitContainerRight;
        private System.Windows.Forms.Panel panelInfo;
        private System.Windows.Forms.Label labelFile;
        private System.Windows.Forms.Label labelIndex;
        private System.Windows.Forms.Label labelLookup;
        private System.Windows.Forms.Label labelLength;
        private System.Windows.Forms.Label labelExtra;
        private System.Windows.Forms.Label labelDecoded;
        private System.Windows.Forms.Panel panelPreview;
        private System.Windows.Forms.RichTextBox richTextBoxDetails;
        private System.Windows.Forms.PictureBox pictureBoxPreview;
        private System.Windows.Forms.Panel panelAnimation;
        private System.Windows.Forms.Panel panelAnimControls;
        private System.Windows.Forms.Button buttonPlayStop;
        private System.Windows.Forms.Label labelDir;
        private System.Windows.Forms.TrackBar trackBarDirection;
        private System.Windows.Forms.Label labelDirValue;
        private System.Windows.Forms.SplitContainer splitContainerAnim;
        private System.Windows.Forms.Panel panelActionList;
        private System.Windows.Forms.Label labelActions;
        private System.Windows.Forms.ListBox listBoxActions;
        private AnimatedPictureBox animatedPictureBox;
        private System.Windows.Forms.Panel panelMulti;
        private System.Windows.Forms.TabControl tabControlMulti;
        private System.Windows.Forms.TabPage tabPageMultiPreview;
        private System.Windows.Forms.Panel panelMultiScroll;
        private System.Windows.Forms.PictureBox pictureBoxMulti;
        private System.Windows.Forms.TabPage tabPageMultiComponents;
        private System.Windows.Forms.RichTextBox richTextBoxMultiComponents;
    }
}
