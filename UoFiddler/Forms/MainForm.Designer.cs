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

using UoFiddler.Controls.UserControls;

namespace UoFiddler.Forms
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            this.TabPanel = new System.Windows.Forms.TabControl();
            this.contextMenuStripMainForm = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.unDockToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.StartTab = new System.Windows.Forms.TabPage();
            this.Versionlabel = new System.Windows.Forms.Label();
            this.MultisTab = new System.Windows.Forms.TabPage();
            this.multisControl = new UoFiddler.Controls.UserControls.MultisControl();
            this.AnimationTab = new System.Windows.Forms.TabPage();
            this.animationsControl = new UoFiddler.Controls.UserControls.AnimationListControl();
            this.ItemsTab = new System.Windows.Forms.TabPage();
            this.itemShowControl = new UoFiddler.Controls.UserControls.ItemShowControl();
            this.LandTilesTab = new System.Windows.Forms.TabPage();
            this.landTilesControl = new UoFiddler.Controls.UserControls.LandTilesControl();
            this.TextureTab = new System.Windows.Forms.TabPage();
            this.textureControl = new UoFiddler.Controls.UserControls.TextureControl();
            this.GumpsTab = new System.Windows.Forms.TabPage();
            this.gumpsControl = new UoFiddler.Controls.UserControls.GumpControl();
            this.SoundsTab = new System.Windows.Forms.TabPage();
            this.soundControl = new UoFiddler.Controls.UserControls.SoundsControl();
            this.HueTab = new System.Windows.Forms.TabPage();
            this.hueControl = new UoFiddler.Controls.UserControls.HuesControl();
            this.FontsTab = new System.Windows.Forms.TabPage();
            this.fontsControl = new UoFiddler.Controls.UserControls.FontsControl();
            this.ClilocTab = new System.Windows.Forms.TabPage();
            this.clilocControl = new UoFiddler.Controls.UserControls.ClilocControl();
            this.MapTab = new System.Windows.Forms.TabPage();
            this.mapControl = new UoFiddler.Controls.UserControls.MapControl();
            this.LightTab = new System.Windows.Forms.TabPage();
            this.lightControl = new UoFiddler.Controls.UserControls.LightControl();
            this.SpeechTab = new System.Windows.Forms.TabPage();
            this.speechControl = new UoFiddler.Controls.UserControls.SpeechControl();
            this.SkillsTab = new System.Windows.Forms.TabPage();
            this.skillsControl = new UoFiddler.Controls.UserControls.SkillsControl();
            this.AnimDataTab = new System.Windows.Forms.TabPage();
            this.animdataControl = new UoFiddler.Controls.UserControls.AnimDataControl();
            this.MultiMapTab = new System.Windows.Forms.TabPage();
            this.multimapControl = new UoFiddler.Controls.UserControls.MultiMapControl();
            this.DressTab = new System.Windows.Forms.TabPage();
            this.dressControl = new UoFiddler.Controls.UserControls.DressControl();
            this.TileDataTab = new System.Windows.Forms.TabPage();
            this.tileDataControl = new UoFiddler.Controls.UserControls.TileDataControl();
            this.RadarColTab = new System.Windows.Forms.TabPage();
            this.radarColControl = new UoFiddler.Controls.UserControls.RadarColorControl();
            this.SkillGrpTab = new System.Windows.Forms.TabPage();
            this.skillGroupControl = new UoFiddler.Controls.UserControls.SkillGroupControl();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.tsMainMenu = new System.Windows.Forms.ToolStrip();
            this.SettingsMenu = new System.Windows.Forms.ToolStripDropDownButton();
            this.AlwaysOnTopMenuitem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pathSettingsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tsSettingsSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.restartNeededMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripDropDownButtonView = new System.Windows.Forms.ToolStripDropDownButton();
            this.ToggleViewStart = new System.Windows.Forms.ToolStripMenuItem();
            this.ToggleViewMulti = new System.Windows.Forms.ToolStripMenuItem();
            this.ToggleViewAnimations = new System.Windows.Forms.ToolStripMenuItem();
            this.ToggleViewItems = new System.Windows.Forms.ToolStripMenuItem();
            this.ToggleViewLandTiles = new System.Windows.Forms.ToolStripMenuItem();
            this.ToggleViewTexture = new System.Windows.Forms.ToolStripMenuItem();
            this.ToggleViewGumps = new System.Windows.Forms.ToolStripMenuItem();
            this.ToggleViewSounds = new System.Windows.Forms.ToolStripMenuItem();
            this.ToggleViewHue = new System.Windows.Forms.ToolStripMenuItem();
            this.ToggleViewFonts = new System.Windows.Forms.ToolStripMenuItem();
            this.ToggleViewCliloc = new System.Windows.Forms.ToolStripMenuItem();
            this.ToggleViewMap = new System.Windows.Forms.ToolStripMenuItem();
            this.ToggleViewLight = new System.Windows.Forms.ToolStripMenuItem();
            this.ToggleViewSpeech = new System.Windows.Forms.ToolStripMenuItem();
            this.ToggleViewSkills = new System.Windows.Forms.ToolStripMenuItem();
            this.ToggleViewAnimData = new System.Windows.Forms.ToolStripMenuItem();
            this.ToggleViewMultiMap = new System.Windows.Forms.ToolStripMenuItem();
            this.ToggleViewDress = new System.Windows.Forms.ToolStripMenuItem();
            this.ToggleViewTileData = new System.Windows.Forms.ToolStripMenuItem();
            this.ToggleViewRadarColor = new System.Windows.Forms.ToolStripMenuItem();
            this.ToggleViewSkillGrp = new System.Windows.Forms.ToolStripMenuItem();
            this.ExternToolsDropDown = new System.Windows.Forms.ToolStripDropDownButton();
            this.manageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripDropDownButtonPlugins = new System.Windows.Forms.ToolStripDropDownButton();
            this.manageToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.tsPluginsSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripDropDownButtonHelp = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripMenuItemHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.TabPanel.SuspendLayout();
            this.contextMenuStripMainForm.SuspendLayout();
            this.StartTab.SuspendLayout();
            this.MultisTab.SuspendLayout();
            this.AnimationTab.SuspendLayout();
            this.ItemsTab.SuspendLayout();
            this.LandTilesTab.SuspendLayout();
            this.TextureTab.SuspendLayout();
            this.GumpsTab.SuspendLayout();
            this.SoundsTab.SuspendLayout();
            this.HueTab.SuspendLayout();
            this.FontsTab.SuspendLayout();
            this.ClilocTab.SuspendLayout();
            this.MapTab.SuspendLayout();
            this.LightTab.SuspendLayout();
            this.SpeechTab.SuspendLayout();
            this.SkillsTab.SuspendLayout();
            this.AnimDataTab.SuspendLayout();
            this.MultiMapTab.SuspendLayout();
            this.DressTab.SuspendLayout();
            this.TileDataTab.SuspendLayout();
            this.RadarColTab.SuspendLayout();
            this.SkillGrpTab.SuspendLayout();
            this.tsMainMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // TabPanel
            // 
            this.TabPanel.ContextMenuStrip = this.contextMenuStripMainForm;
            this.TabPanel.Controls.Add(this.StartTab);
            this.TabPanel.Controls.Add(this.MultisTab);
            this.TabPanel.Controls.Add(this.AnimationTab);
            this.TabPanel.Controls.Add(this.ItemsTab);
            this.TabPanel.Controls.Add(this.LandTilesTab);
            this.TabPanel.Controls.Add(this.TextureTab);
            this.TabPanel.Controls.Add(this.GumpsTab);
            this.TabPanel.Controls.Add(this.SoundsTab);
            this.TabPanel.Controls.Add(this.HueTab);
            this.TabPanel.Controls.Add(this.FontsTab);
            this.TabPanel.Controls.Add(this.ClilocTab);
            this.TabPanel.Controls.Add(this.MapTab);
            this.TabPanel.Controls.Add(this.LightTab);
            this.TabPanel.Controls.Add(this.SpeechTab);
            this.TabPanel.Controls.Add(this.SkillsTab);
            this.TabPanel.Controls.Add(this.AnimDataTab);
            this.TabPanel.Controls.Add(this.MultiMapTab);
            this.TabPanel.Controls.Add(this.DressTab);
            this.TabPanel.Controls.Add(this.TileDataTab);
            this.TabPanel.Controls.Add(this.RadarColTab);
            this.TabPanel.Controls.Add(this.SkillGrpTab);
            this.TabPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TabPanel.Location = new System.Drawing.Point(0, 25);
            this.TabPanel.Margin = new System.Windows.Forms.Padding(0);
            this.TabPanel.Name = "TabPanel";
            this.TabPanel.SelectedIndex = 0;
            this.TabPanel.Size = new System.Drawing.Size(784, 536);
            this.TabPanel.TabIndex = 1;
            this.TabPanel.Tag = "20";
            // 
            // contextMenuStripMainForm
            // 
            this.contextMenuStripMainForm.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.unDockToolStripMenuItem});
            this.contextMenuStripMainForm.Name = "contextMenuStrip1";
            this.contextMenuStripMainForm.Size = new System.Drawing.Size(117, 26);
            // 
            // unDockToolStripMenuItem
            // 
            this.unDockToolStripMenuItem.Name = "unDockToolStripMenuItem";
            this.unDockToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            this.unDockToolStripMenuItem.Text = "UnDock";
            this.unDockToolStripMenuItem.Click += new System.EventHandler(this.OnClickUnDock);
            // 
            // StartTab
            // 
            this.StartTab.BackColor = System.Drawing.Color.White;
            this.StartTab.BackgroundImage = global::UoFiddler.Properties.Resources.UOFiddler;
            this.StartTab.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.StartTab.Controls.Add(this.Versionlabel);
            this.StartTab.Location = new System.Drawing.Point(4, 22);
            this.StartTab.Name = "StartTab";
            this.StartTab.Padding = new System.Windows.Forms.Padding(3);
            this.StartTab.Size = new System.Drawing.Size(776, 510);
            this.StartTab.TabIndex = 10;
            this.StartTab.Tag = 0;
            this.StartTab.Text = "Start";
            // 
            // Versionlabel
            // 
            this.Versionlabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Versionlabel.AutoSize = true;
            this.Versionlabel.Location = new System.Drawing.Point(726, 492);
            this.Versionlabel.Name = "Versionlabel";
            this.Versionlabel.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.Versionlabel.Size = new System.Drawing.Size(42, 13);
            this.Versionlabel.TabIndex = 1;
            this.Versionlabel.Text = "Version";
            this.Versionlabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // MultisTab
            // 
            this.MultisTab.Controls.Add(this.multisControl);
            this.MultisTab.Location = new System.Drawing.Point(4, 22);
            this.MultisTab.Name = "MultisTab";
            this.MultisTab.Padding = new System.Windows.Forms.Padding(3);
            this.MultisTab.Size = new System.Drawing.Size(776, 510);
            this.MultisTab.TabIndex = 1;
            this.MultisTab.Tag = 1;
            this.MultisTab.Text = "Multis";
            this.MultisTab.UseVisualStyleBackColor = true;
            // 
            // multisControl
            // 
            this.multisControl.Cursor = System.Windows.Forms.Cursors.Default;
            this.multisControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.multisControl.Location = new System.Drawing.Point(3, 3);
            this.multisControl.Name = "multisControl";
            this.multisControl.Size = new System.Drawing.Size(770, 504);
            this.multisControl.TabIndex = 0;
            // 
            // AnimationTab
            // 
            this.AnimationTab.Controls.Add(this.animationsControl);
            this.AnimationTab.Location = new System.Drawing.Point(4, 22);
            this.AnimationTab.Name = "AnimationTab";
            this.AnimationTab.Padding = new System.Windows.Forms.Padding(3);
            this.AnimationTab.Size = new System.Drawing.Size(776, 510);
            this.AnimationTab.TabIndex = 0;
            this.AnimationTab.Tag = 2;
            this.AnimationTab.Text = "Animations";
            this.AnimationTab.UseVisualStyleBackColor = true;
            // 
            // animationsControl
            // 
            this.animationsControl.Cursor = System.Windows.Forms.Cursors.Default;
            this.animationsControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.animationsControl.Location = new System.Drawing.Point(3, 3);
            this.animationsControl.Name = "animationsControl";
            this.animationsControl.Size = new System.Drawing.Size(770, 504);
            this.animationsControl.TabIndex = 0;
            // 
            // ItemsTab
            // 
            this.ItemsTab.Controls.Add(this.itemShowControl);
            this.ItemsTab.Location = new System.Drawing.Point(4, 22);
            this.ItemsTab.Name = "ItemsTab";
            this.ItemsTab.Padding = new System.Windows.Forms.Padding(3);
            this.ItemsTab.Size = new System.Drawing.Size(776, 510);
            this.ItemsTab.TabIndex = 2;
            this.ItemsTab.Tag = 3;
            this.ItemsTab.Text = "Items";
            this.ItemsTab.UseVisualStyleBackColor = true;
            // 
            // itemShowControl
            // 
            this.itemShowControl.Cursor = System.Windows.Forms.Cursors.Default;
            this.itemShowControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.itemShowControl.Location = new System.Drawing.Point(3, 3);
            this.itemShowControl.Name = "itemShowControl";
            this.itemShowControl.Size = new System.Drawing.Size(770, 504);
            this.itemShowControl.TabIndex = 0;
            // 
            // LandTilesTab
            // 
            this.LandTilesTab.Controls.Add(this.landTilesControl);
            this.LandTilesTab.Location = new System.Drawing.Point(4, 22);
            this.LandTilesTab.Name = "LandTilesTab";
            this.LandTilesTab.Padding = new System.Windows.Forms.Padding(3);
            this.LandTilesTab.Size = new System.Drawing.Size(776, 510);
            this.LandTilesTab.TabIndex = 3;
            this.LandTilesTab.Tag = 4;
            this.LandTilesTab.Text = "LandTiles";
            this.LandTilesTab.UseVisualStyleBackColor = true;
            // 
            // landTilesControl
            // 
            this.landTilesControl.Cursor = System.Windows.Forms.Cursors.Default;
            this.landTilesControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.landTilesControl.Location = new System.Drawing.Point(3, 3);
            this.landTilesControl.Name = "landTilesControl";
            this.landTilesControl.Size = new System.Drawing.Size(770, 504);
            this.landTilesControl.TabIndex = 0;
            // 
            // TextureTab
            // 
            this.TextureTab.Controls.Add(this.textureControl);
            this.TextureTab.Location = new System.Drawing.Point(4, 22);
            this.TextureTab.Name = "TextureTab";
            this.TextureTab.Padding = new System.Windows.Forms.Padding(3);
            this.TextureTab.Size = new System.Drawing.Size(776, 510);
            this.TextureTab.TabIndex = 11;
            this.TextureTab.Tag = 5;
            this.TextureTab.Text = "Texture";
            this.TextureTab.UseVisualStyleBackColor = true;
            // 
            // textureControl
            // 
            this.textureControl.Cursor = System.Windows.Forms.Cursors.Default;
            this.textureControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textureControl.Location = new System.Drawing.Point(3, 3);
            this.textureControl.Name = "textureControl";
            this.textureControl.Size = new System.Drawing.Size(770, 504);
            this.textureControl.TabIndex = 0;
            // 
            // GumpsTab
            // 
            this.GumpsTab.Controls.Add(this.gumpsControl);
            this.GumpsTab.Location = new System.Drawing.Point(4, 22);
            this.GumpsTab.Name = "GumpsTab";
            this.GumpsTab.Padding = new System.Windows.Forms.Padding(3);
            this.GumpsTab.Size = new System.Drawing.Size(776, 510);
            this.GumpsTab.TabIndex = 4;
            this.GumpsTab.Tag = 6;
            this.GumpsTab.Text = "Gumps";
            this.GumpsTab.UseVisualStyleBackColor = true;
            // 
            // gumpsControl
            // 
            this.gumpsControl.Cursor = System.Windows.Forms.Cursors.Default;
            this.gumpsControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gumpsControl.Location = new System.Drawing.Point(3, 3);
            this.gumpsControl.Name = "gumpsControl";
            this.gumpsControl.Size = new System.Drawing.Size(770, 504);
            this.gumpsControl.TabIndex = 0;
            // 
            // SoundsTab
            // 
            this.SoundsTab.Controls.Add(this.soundControl);
            this.SoundsTab.Location = new System.Drawing.Point(4, 22);
            this.SoundsTab.Name = "SoundsTab";
            this.SoundsTab.Padding = new System.Windows.Forms.Padding(3);
            this.SoundsTab.Size = new System.Drawing.Size(776, 510);
            this.SoundsTab.TabIndex = 5;
            this.SoundsTab.Tag = 7;
            this.SoundsTab.Text = "Sounds";
            this.SoundsTab.UseVisualStyleBackColor = true;
            // 
            // soundControl
            // 
            this.soundControl.Cursor = System.Windows.Forms.Cursors.Default;
            this.soundControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.soundControl.Location = new System.Drawing.Point(3, 3);
            this.soundControl.Name = "soundControl";
            this.soundControl.Size = new System.Drawing.Size(770, 504);
            this.soundControl.TabIndex = 0;
            // 
            // HueTab
            // 
            this.HueTab.Controls.Add(this.hueControl);
            this.HueTab.Location = new System.Drawing.Point(4, 22);
            this.HueTab.Name = "HueTab";
            this.HueTab.Padding = new System.Windows.Forms.Padding(3);
            this.HueTab.Size = new System.Drawing.Size(776, 510);
            this.HueTab.TabIndex = 6;
            this.HueTab.Tag = 8;
            this.HueTab.Text = "Hue";
            this.HueTab.UseVisualStyleBackColor = true;
            // 
            // hueControl
            // 
            this.hueControl.Cursor = System.Windows.Forms.Cursors.Default;
            this.hueControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hueControl.ForeColor = System.Drawing.SystemColors.ControlText;
            this.hueControl.Location = new System.Drawing.Point(3, 3);
            this.hueControl.Name = "hueControl";
            this.hueControl.Padding = new System.Windows.Forms.Padding(1);
            this.hueControl.Size = new System.Drawing.Size(770, 504);
            this.hueControl.TabIndex = 0;
            // 
            // FontsTab
            // 
            this.FontsTab.Controls.Add(this.fontsControl);
            this.FontsTab.Location = new System.Drawing.Point(4, 22);
            this.FontsTab.Name = "FontsTab";
            this.FontsTab.Padding = new System.Windows.Forms.Padding(3);
            this.FontsTab.Size = new System.Drawing.Size(776, 510);
            this.FontsTab.TabIndex = 7;
            this.FontsTab.Tag = 9;
            this.FontsTab.Text = "Fonts";
            this.FontsTab.UseVisualStyleBackColor = true;
            // 
            // fontsControl
            // 
            this.fontsControl.Cursor = System.Windows.Forms.Cursors.Default;
            this.fontsControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fontsControl.Location = new System.Drawing.Point(3, 3);
            this.fontsControl.Name = "fontsControl";
            this.fontsControl.Size = new System.Drawing.Size(770, 504);
            this.fontsControl.TabIndex = 0;
            // 
            // ClilocTab
            // 
            this.ClilocTab.Controls.Add(this.clilocControl);
            this.ClilocTab.Location = new System.Drawing.Point(4, 22);
            this.ClilocTab.Name = "ClilocTab";
            this.ClilocTab.Padding = new System.Windows.Forms.Padding(3);
            this.ClilocTab.Size = new System.Drawing.Size(776, 510);
            this.ClilocTab.TabIndex = 8;
            this.ClilocTab.Tag = 10;
            this.ClilocTab.Text = "CliLoc";
            this.ClilocTab.UseVisualStyleBackColor = true;
            // 
            // clilocControl
            // 
            this.clilocControl.Cursor = System.Windows.Forms.Cursors.Default;
            this.clilocControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.clilocControl.Location = new System.Drawing.Point(3, 3);
            this.clilocControl.Name = "clilocControl";
            this.clilocControl.Size = new System.Drawing.Size(770, 504);
            this.clilocControl.TabIndex = 0;
            // 
            // MapTab
            // 
            this.MapTab.Controls.Add(this.mapControl);
            this.MapTab.Location = new System.Drawing.Point(4, 22);
            this.MapTab.Name = "MapTab";
            this.MapTab.Padding = new System.Windows.Forms.Padding(3);
            this.MapTab.Size = new System.Drawing.Size(776, 510);
            this.MapTab.TabIndex = 9;
            this.MapTab.Tag = 11;
            this.MapTab.Text = "Map";
            this.MapTab.UseVisualStyleBackColor = true;
            // 
            // mapControl
            // 
            this.mapControl.Cursor = System.Windows.Forms.Cursors.Default;
            this.mapControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mapControl.Location = new System.Drawing.Point(3, 3);
            this.mapControl.Margin = new System.Windows.Forms.Padding(0);
            this.mapControl.Name = "mapControl";
            this.mapControl.Size = new System.Drawing.Size(770, 504);
            this.mapControl.TabIndex = 0;
            // 
            // LightTab
            // 
            this.LightTab.Controls.Add(this.lightControl);
            this.LightTab.Location = new System.Drawing.Point(4, 22);
            this.LightTab.Name = "LightTab";
            this.LightTab.Size = new System.Drawing.Size(776, 510);
            this.LightTab.TabIndex = 12;
            this.LightTab.Tag = 12;
            this.LightTab.Text = "Light";
            this.LightTab.UseVisualStyleBackColor = true;
            // 
            // lightControl
            // 
            this.lightControl.Cursor = System.Windows.Forms.Cursors.Default;
            this.lightControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lightControl.Location = new System.Drawing.Point(0, 0);
            this.lightControl.Name = "lightControl";
            this.lightControl.Size = new System.Drawing.Size(776, 510);
            this.lightControl.TabIndex = 0;
            // 
            // SpeechTab
            // 
            this.SpeechTab.Controls.Add(this.speechControl);
            this.SpeechTab.Location = new System.Drawing.Point(4, 22);
            this.SpeechTab.Name = "SpeechTab";
            this.SpeechTab.Size = new System.Drawing.Size(776, 510);
            this.SpeechTab.TabIndex = 17;
            this.SpeechTab.Tag = 13;
            this.SpeechTab.Text = "Speech";
            this.SpeechTab.UseVisualStyleBackColor = true;
            // 
            // speechControl
            // 
            this.speechControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.speechControl.Location = new System.Drawing.Point(0, 0);
            this.speechControl.Name = "speechControl";
            this.speechControl.Size = new System.Drawing.Size(776, 510);
            this.speechControl.TabIndex = 0;
            // 
            // SkillsTab
            // 
            this.SkillsTab.Controls.Add(this.skillsControl);
            this.SkillsTab.Location = new System.Drawing.Point(4, 22);
            this.SkillsTab.Name = "SkillsTab";
            this.SkillsTab.Padding = new System.Windows.Forms.Padding(3);
            this.SkillsTab.Size = new System.Drawing.Size(776, 510);
            this.SkillsTab.TabIndex = 15;
            this.SkillsTab.Tag = 14;
            this.SkillsTab.Text = "Skills";
            this.SkillsTab.UseVisualStyleBackColor = true;
            // 
            // skillsControl
            // 
            this.skillsControl.Cursor = System.Windows.Forms.Cursors.Default;
            this.skillsControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.skillsControl.Location = new System.Drawing.Point(3, 3);
            this.skillsControl.Name = "skillsControl";
            this.skillsControl.Size = new System.Drawing.Size(770, 504);
            this.skillsControl.TabIndex = 0;
            // 
            // AnimDataTab
            // 
            this.AnimDataTab.Controls.Add(this.animdataControl);
            this.AnimDataTab.Location = new System.Drawing.Point(4, 22);
            this.AnimDataTab.Name = "AnimDataTab";
            this.AnimDataTab.Size = new System.Drawing.Size(776, 510);
            this.AnimDataTab.TabIndex = 18;
            this.AnimDataTab.Tag = 15;
            this.AnimDataTab.Text = "AnimData";
            this.AnimDataTab.UseVisualStyleBackColor = true;
            // 
            // animdataControl
            // 
            this.animdataControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.animdataControl.Location = new System.Drawing.Point(0, 0);
            this.animdataControl.Name = "animdataControl";
            this.animdataControl.Size = new System.Drawing.Size(776, 510);
            this.animdataControl.TabIndex = 0;
            // 
            // MultiMapTab
            // 
            this.MultiMapTab.Controls.Add(this.multimapControl);
            this.MultiMapTab.Location = new System.Drawing.Point(4, 22);
            this.MultiMapTab.Name = "MultiMapTab";
            this.MultiMapTab.Padding = new System.Windows.Forms.Padding(3);
            this.MultiMapTab.Size = new System.Drawing.Size(776, 510);
            this.MultiMapTab.TabIndex = 14;
            this.MultiMapTab.Tag = 16;
            this.MultiMapTab.Text = "MultiMap/Facets";
            this.MultiMapTab.UseVisualStyleBackColor = true;
            // 
            // multimapControl
            // 
            this.multimapControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.multimapControl.Location = new System.Drawing.Point(3, 3);
            this.multimapControl.Name = "multimapControl";
            this.multimapControl.Size = new System.Drawing.Size(770, 504);
            this.multimapControl.TabIndex = 0;
            // 
            // DressTab
            // 
            this.DressTab.Controls.Add(this.dressControl);
            this.DressTab.Location = new System.Drawing.Point(4, 22);
            this.DressTab.Name = "DressTab";
            this.DressTab.Padding = new System.Windows.Forms.Padding(3);
            this.DressTab.Size = new System.Drawing.Size(776, 510);
            this.DressTab.TabIndex = 13;
            this.DressTab.Tag = 17;
            this.DressTab.Text = "Dress";
            this.DressTab.UseVisualStyleBackColor = true;
            // 
            // dressControl
            // 
            this.dressControl.Cursor = System.Windows.Forms.Cursors.Default;
            this.dressControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dressControl.Location = new System.Drawing.Point(3, 3);
            this.dressControl.Name = "dressControl";
            this.dressControl.Size = new System.Drawing.Size(770, 504);
            this.dressControl.TabIndex = 0;
            // 
            // TileDataTab
            // 
            this.TileDataTab.Controls.Add(this.tileDataControl);
            this.TileDataTab.Location = new System.Drawing.Point(4, 22);
            this.TileDataTab.Name = "TileDataTab";
            this.TileDataTab.Size = new System.Drawing.Size(776, 510);
            this.TileDataTab.TabIndex = 16;
            this.TileDataTab.Tag = 18;
            this.TileDataTab.Text = "TileData";
            this.TileDataTab.UseVisualStyleBackColor = true;
            // 
            // tileDataControl
            // 
            this.tileDataControl.Cursor = System.Windows.Forms.Cursors.Default;
            this.tileDataControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tileDataControl.Location = new System.Drawing.Point(0, 0);
            this.tileDataControl.Name = "tileDataControl";
            this.tileDataControl.Size = new System.Drawing.Size(776, 510);
            this.tileDataControl.TabIndex = 0;
            // 
            // RadarColTab
            // 
            this.RadarColTab.Controls.Add(this.radarColControl);
            this.RadarColTab.Location = new System.Drawing.Point(4, 22);
            this.RadarColTab.Name = "RadarColTab";
            this.RadarColTab.Size = new System.Drawing.Size(776, 510);
            this.RadarColTab.TabIndex = 19;
            this.RadarColTab.Tag = 19;
            this.RadarColTab.Text = "RadarColor";
            this.RadarColTab.UseVisualStyleBackColor = true;
            // 
            // radarColControl
            // 
            this.radarColControl.Cursor = System.Windows.Forms.Cursors.Default;
            this.radarColControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.radarColControl.Location = new System.Drawing.Point(0, 0);
            this.radarColControl.Name = "radarColControl";
            this.radarColControl.Size = new System.Drawing.Size(776, 510);
            this.radarColControl.TabIndex = 0;
            // 
            // SkillGrpTab
            // 
            this.SkillGrpTab.Controls.Add(this.skillGroupControl);
            this.SkillGrpTab.Location = new System.Drawing.Point(4, 22);
            this.SkillGrpTab.Name = "SkillGrpTab";
            this.SkillGrpTab.Padding = new System.Windows.Forms.Padding(3);
            this.SkillGrpTab.Size = new System.Drawing.Size(776, 510);
            this.SkillGrpTab.TabIndex = 20;
            this.SkillGrpTab.Tag = 20;
            this.SkillGrpTab.Text = "SkillGrp";
            this.SkillGrpTab.UseVisualStyleBackColor = true;
            // 
            // skillGroupControl
            // 
            this.skillGroupControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.skillGroupControl.Location = new System.Drawing.Point(3, 3);
            this.skillGroupControl.Name = "skillGroupControl";
            this.skillGroupControl.Size = new System.Drawing.Size(770, 504);
            this.skillGroupControl.TabIndex = 0;
            // 
            // tsMainMenu
            // 
            this.tsMainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SettingsMenu,
            this.toolStripDropDownButtonView,
            this.ExternToolsDropDown,
            this.toolStripDropDownButtonPlugins,
            this.toolStripDropDownButtonHelp});
            this.tsMainMenu.Location = new System.Drawing.Point(0, 0);
            this.tsMainMenu.Name = "tsMainMenu";
            this.tsMainMenu.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.tsMainMenu.Size = new System.Drawing.Size(784, 25);
            this.tsMainMenu.TabIndex = 2;
            this.tsMainMenu.Text = "toolStrip1";
            // 
            // SettingsMenu
            // 
            this.SettingsMenu.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.SettingsMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AlwaysOnTopMenuitem,
            this.optionsToolStripMenuItem,
            this.pathSettingsMenuItem,
            this.tsSettingsSeparator,
            this.restartNeededMenuItem});
            this.SettingsMenu.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SettingsMenu.Name = "SettingsMenu";
            this.SettingsMenu.Size = new System.Drawing.Size(62, 22);
            this.SettingsMenu.Text = "Settings";
            // 
            // AlwaysOnTopMenuitem
            // 
            this.AlwaysOnTopMenuitem.CheckOnClick = true;
            this.AlwaysOnTopMenuitem.Name = "AlwaysOnTopMenuitem";
            this.AlwaysOnTopMenuitem.Size = new System.Drawing.Size(152, 22);
            this.AlwaysOnTopMenuitem.Text = "Always On Top";
            this.AlwaysOnTopMenuitem.Click += new System.EventHandler(this.OnClickAlwaysTop);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.optionsToolStripMenuItem.Text = "Options..";
            this.optionsToolStripMenuItem.Click += new System.EventHandler(this.OnClickOptions);
            // 
            // pathSettingsMenuItem
            // 
            this.pathSettingsMenuItem.Name = "pathSettingsMenuItem";
            this.pathSettingsMenuItem.Size = new System.Drawing.Size(152, 22);
            this.pathSettingsMenuItem.Text = "Path Settings..";
            this.pathSettingsMenuItem.Click += new System.EventHandler(this.Click_path);
            // 
            // tsSettingsSeparator
            // 
            this.tsSettingsSeparator.Name = "tsSettingsSeparator";
            this.tsSettingsSeparator.Size = new System.Drawing.Size(149, 6);
            // 
            // restartNeededMenuItem
            // 
            this.restartNeededMenuItem.ForeColor = System.Drawing.Color.DarkRed;
            this.restartNeededMenuItem.Name = "restartNeededMenuItem";
            this.restartNeededMenuItem.Size = new System.Drawing.Size(152, 22);
            this.restartNeededMenuItem.Text = "Reload Files";
            this.restartNeededMenuItem.Click += new System.EventHandler(this.Restart);
            // 
            // toolStripDropDownButtonView
            // 
            this.toolStripDropDownButtonView.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButtonView.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToggleViewStart,
            this.ToggleViewMulti,
            this.ToggleViewAnimations,
            this.ToggleViewItems,
            this.ToggleViewLandTiles,
            this.ToggleViewTexture,
            this.ToggleViewGumps,
            this.ToggleViewSounds,
            this.ToggleViewHue,
            this.ToggleViewFonts,
            this.ToggleViewCliloc,
            this.ToggleViewMap,
            this.ToggleViewLight,
            this.ToggleViewSpeech,
            this.ToggleViewSkills,
            this.ToggleViewAnimData,
            this.ToggleViewMultiMap,
            this.ToggleViewDress,
            this.ToggleViewTileData,
            this.ToggleViewRadarColor,
            this.ToggleViewSkillGrp});
            this.toolStripDropDownButtonView.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButtonView.Name = "toolStripDropDownButtonView";
            this.toolStripDropDownButtonView.Size = new System.Drawing.Size(45, 22);
            this.toolStripDropDownButtonView.Text = "View";
            // 
            // ToggleViewStart
            // 
            this.ToggleViewStart.Checked = true;
            this.ToggleViewStart.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ToggleViewStart.Name = "ToggleViewStart";
            this.ToggleViewStart.Size = new System.Drawing.Size(164, 22);
            this.ToggleViewStart.Tag = 0;
            this.ToggleViewStart.Text = "Start";
            this.ToggleViewStart.Click += new System.EventHandler(this.ToggleView);
            // 
            // ToggleViewMulti
            // 
            this.ToggleViewMulti.Checked = true;
            this.ToggleViewMulti.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ToggleViewMulti.Name = "ToggleViewMulti";
            this.ToggleViewMulti.Size = new System.Drawing.Size(164, 22);
            this.ToggleViewMulti.Tag = 1;
            this.ToggleViewMulti.Text = "Multi";
            this.ToggleViewMulti.Click += new System.EventHandler(this.ToggleView);
            // 
            // ToggleViewAnimations
            // 
            this.ToggleViewAnimations.Checked = true;
            this.ToggleViewAnimations.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ToggleViewAnimations.Name = "ToggleViewAnimations";
            this.ToggleViewAnimations.Size = new System.Drawing.Size(164, 22);
            this.ToggleViewAnimations.Tag = 2;
            this.ToggleViewAnimations.Text = "Animations";
            this.ToggleViewAnimations.Click += new System.EventHandler(this.ToggleView);
            // 
            // ToggleViewItems
            // 
            this.ToggleViewItems.Checked = true;
            this.ToggleViewItems.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ToggleViewItems.Name = "ToggleViewItems";
            this.ToggleViewItems.Size = new System.Drawing.Size(164, 22);
            this.ToggleViewItems.Tag = 3;
            this.ToggleViewItems.Text = "Items";
            this.ToggleViewItems.Click += new System.EventHandler(this.ToggleView);
            // 
            // ToggleViewLandTiles
            // 
            this.ToggleViewLandTiles.Checked = true;
            this.ToggleViewLandTiles.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ToggleViewLandTiles.Name = "ToggleViewLandTiles";
            this.ToggleViewLandTiles.Size = new System.Drawing.Size(164, 22);
            this.ToggleViewLandTiles.Tag = 4;
            this.ToggleViewLandTiles.Text = "LandTiles";
            this.ToggleViewLandTiles.Click += new System.EventHandler(this.ToggleView);
            // 
            // ToggleViewTexture
            // 
            this.ToggleViewTexture.Checked = true;
            this.ToggleViewTexture.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ToggleViewTexture.Name = "ToggleViewTexture";
            this.ToggleViewTexture.Size = new System.Drawing.Size(164, 22);
            this.ToggleViewTexture.Tag = 5;
            this.ToggleViewTexture.Text = "Texture";
            this.ToggleViewTexture.Click += new System.EventHandler(this.ToggleView);
            // 
            // ToggleViewGumps
            // 
            this.ToggleViewGumps.Checked = true;
            this.ToggleViewGumps.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ToggleViewGumps.Name = "ToggleViewGumps";
            this.ToggleViewGumps.Size = new System.Drawing.Size(164, 22);
            this.ToggleViewGumps.Tag = 6;
            this.ToggleViewGumps.Text = "Gumps";
            this.ToggleViewGumps.Click += new System.EventHandler(this.ToggleView);
            // 
            // ToggleViewSounds
            // 
            this.ToggleViewSounds.Checked = true;
            this.ToggleViewSounds.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ToggleViewSounds.Name = "ToggleViewSounds";
            this.ToggleViewSounds.Size = new System.Drawing.Size(164, 22);
            this.ToggleViewSounds.Tag = 7;
            this.ToggleViewSounds.Text = "Sounds";
            this.ToggleViewSounds.Click += new System.EventHandler(this.ToggleView);
            // 
            // ToggleViewHue
            // 
            this.ToggleViewHue.Checked = true;
            this.ToggleViewHue.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ToggleViewHue.Name = "ToggleViewHue";
            this.ToggleViewHue.Size = new System.Drawing.Size(164, 22);
            this.ToggleViewHue.Tag = 8;
            this.ToggleViewHue.Text = "Hue";
            this.ToggleViewHue.Click += new System.EventHandler(this.ToggleView);
            // 
            // ToggleViewFonts
            // 
            this.ToggleViewFonts.Checked = true;
            this.ToggleViewFonts.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ToggleViewFonts.Name = "ToggleViewFonts";
            this.ToggleViewFonts.Size = new System.Drawing.Size(164, 22);
            this.ToggleViewFonts.Tag = 9;
            this.ToggleViewFonts.Text = "Fonts";
            this.ToggleViewFonts.Click += new System.EventHandler(this.ToggleView);
            // 
            // ToggleViewCliloc
            // 
            this.ToggleViewCliloc.Checked = true;
            this.ToggleViewCliloc.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ToggleViewCliloc.Name = "ToggleViewCliloc";
            this.ToggleViewCliloc.Size = new System.Drawing.Size(164, 22);
            this.ToggleViewCliloc.Tag = 10;
            this.ToggleViewCliloc.Text = "Cliloc";
            this.ToggleViewCliloc.Click += new System.EventHandler(this.ToggleView);
            // 
            // ToggleViewMap
            // 
            this.ToggleViewMap.Checked = true;
            this.ToggleViewMap.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ToggleViewMap.Name = "ToggleViewMap";
            this.ToggleViewMap.Size = new System.Drawing.Size(164, 22);
            this.ToggleViewMap.Tag = 11;
            this.ToggleViewMap.Text = "Map";
            this.ToggleViewMap.Click += new System.EventHandler(this.ToggleView);
            // 
            // ToggleViewLight
            // 
            this.ToggleViewLight.Checked = true;
            this.ToggleViewLight.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ToggleViewLight.Name = "ToggleViewLight";
            this.ToggleViewLight.Size = new System.Drawing.Size(164, 22);
            this.ToggleViewLight.Tag = 12;
            this.ToggleViewLight.Text = "Light";
            this.ToggleViewLight.Click += new System.EventHandler(this.ToggleView);
            // 
            // ToggleViewSpeech
            // 
            this.ToggleViewSpeech.Checked = true;
            this.ToggleViewSpeech.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ToggleViewSpeech.Name = "ToggleViewSpeech";
            this.ToggleViewSpeech.Size = new System.Drawing.Size(164, 22);
            this.ToggleViewSpeech.Tag = 13;
            this.ToggleViewSpeech.Text = "Speech";
            this.ToggleViewSpeech.Click += new System.EventHandler(this.ToggleView);
            // 
            // ToggleViewSkills
            // 
            this.ToggleViewSkills.Checked = true;
            this.ToggleViewSkills.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ToggleViewSkills.Name = "ToggleViewSkills";
            this.ToggleViewSkills.Size = new System.Drawing.Size(164, 22);
            this.ToggleViewSkills.Tag = 14;
            this.ToggleViewSkills.Text = "Skills";
            this.ToggleViewSkills.Click += new System.EventHandler(this.ToggleView);
            // 
            // ToggleViewAnimData
            // 
            this.ToggleViewAnimData.Checked = true;
            this.ToggleViewAnimData.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ToggleViewAnimData.Name = "ToggleViewAnimData";
            this.ToggleViewAnimData.Size = new System.Drawing.Size(164, 22);
            this.ToggleViewAnimData.Tag = 15;
            this.ToggleViewAnimData.Text = "AnimData";
            this.ToggleViewAnimData.Click += new System.EventHandler(this.ToggleView);
            // 
            // ToggleViewMultiMap
            // 
            this.ToggleViewMultiMap.Checked = true;
            this.ToggleViewMultiMap.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ToggleViewMultiMap.Name = "ToggleViewMultiMap";
            this.ToggleViewMultiMap.Size = new System.Drawing.Size(164, 22);
            this.ToggleViewMultiMap.Tag = 16;
            this.ToggleViewMultiMap.Text = "MultiMap/Facets";
            this.ToggleViewMultiMap.Click += new System.EventHandler(this.ToggleView);
            // 
            // ToggleViewDress
            // 
            this.ToggleViewDress.Checked = true;
            this.ToggleViewDress.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ToggleViewDress.Name = "ToggleViewDress";
            this.ToggleViewDress.Size = new System.Drawing.Size(164, 22);
            this.ToggleViewDress.Tag = 17;
            this.ToggleViewDress.Text = "Dress";
            this.ToggleViewDress.Click += new System.EventHandler(this.ToggleView);
            // 
            // ToggleViewTileData
            // 
            this.ToggleViewTileData.Checked = true;
            this.ToggleViewTileData.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ToggleViewTileData.Name = "ToggleViewTileData";
            this.ToggleViewTileData.Size = new System.Drawing.Size(164, 22);
            this.ToggleViewTileData.Tag = 18;
            this.ToggleViewTileData.Text = "TileData";
            this.ToggleViewTileData.Click += new System.EventHandler(this.ToggleView);
            // 
            // ToggleViewRadarColor
            // 
            this.ToggleViewRadarColor.Checked = true;
            this.ToggleViewRadarColor.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ToggleViewRadarColor.Name = "ToggleViewRadarColor";
            this.ToggleViewRadarColor.Size = new System.Drawing.Size(164, 22);
            this.ToggleViewRadarColor.Tag = 19;
            this.ToggleViewRadarColor.Text = "RadarColor";
            this.ToggleViewRadarColor.Click += new System.EventHandler(this.ToggleView);
            // 
            // ToggleViewSkillGrp
            // 
            this.ToggleViewSkillGrp.Checked = true;
            this.ToggleViewSkillGrp.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ToggleViewSkillGrp.Name = "ToggleViewSkillGrp";
            this.ToggleViewSkillGrp.Size = new System.Drawing.Size(164, 22);
            this.ToggleViewSkillGrp.Tag = 20;
            this.ToggleViewSkillGrp.Text = "SkillGrp";
            this.ToggleViewSkillGrp.Click += new System.EventHandler(this.ToggleView);
            // 
            // ExternToolsDropDown
            // 
            this.ExternToolsDropDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.ExternToolsDropDown.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.manageToolStripMenuItem});
            this.ExternToolsDropDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ExternToolsDropDown.Name = "ExternToolsDropDown";
            this.ExternToolsDropDown.Size = new System.Drawing.Size(83, 22);
            this.ExternToolsDropDown.Text = "Extern Tools";
            // 
            // manageToolStripMenuItem
            // 
            this.manageToolStripMenuItem.Name = "manageToolStripMenuItem";
            this.manageToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
            this.manageToolStripMenuItem.Text = "Manage..";
            this.manageToolStripMenuItem.Click += new System.EventHandler(this.OnClickToolManage);
            // 
            // toolStripDropDownButtonPlugins
            // 
            this.toolStripDropDownButtonPlugins.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButtonPlugins.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.manageToolStripMenuItem1,
            this.tsPluginsSeparator});
            this.toolStripDropDownButtonPlugins.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButtonPlugins.Name = "toolStripDropDownButtonPlugins";
            this.toolStripDropDownButtonPlugins.Size = new System.Drawing.Size(59, 22);
            this.toolStripDropDownButtonPlugins.Text = "Plugins";
            // 
            // manageToolStripMenuItem1
            // 
            this.manageToolStripMenuItem1.Name = "manageToolStripMenuItem1";
            this.manageToolStripMenuItem1.Size = new System.Drawing.Size(123, 22);
            this.manageToolStripMenuItem1.Text = "Manage..";
            this.manageToolStripMenuItem1.Click += new System.EventHandler(this.OnClickManagePlugins);
            // 
            // tsPluginsSeparator
            // 
            this.tsPluginsSeparator.Name = "tsPluginsSeparator";
            this.tsPluginsSeparator.Size = new System.Drawing.Size(120, 6);
            // 
            // toolStripDropDownButtonHelp
            // 
            this.toolStripDropDownButtonHelp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButtonHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemHelp,
            this.toolStripSeparator1,
            this.toolStripMenuItemAbout});
            this.toolStripDropDownButtonHelp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButtonHelp.Name = "toolStripDropDownButtonHelp";
            this.toolStripDropDownButtonHelp.Size = new System.Drawing.Size(45, 22);
            this.toolStripDropDownButtonHelp.Text = "Help";
            this.toolStripDropDownButtonHelp.ToolTipText = "Help";
            // 
            // toolStripMenuItemHelp
            // 
            this.toolStripMenuItemHelp.Name = "toolStripMenuItemHelp";
            this.toolStripMenuItemHelp.Size = new System.Drawing.Size(107, 22);
            this.toolStripMenuItemHelp.Text = "Help";
            this.toolStripMenuItemHelp.Click += new System.EventHandler(this.ToolStripMenuItemHelp_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(104, 6);
            // 
            // toolStripMenuItemAbout
            // 
            this.toolStripMenuItemAbout.Name = "toolStripMenuItemAbout";
            this.toolStripMenuItemAbout.Size = new System.Drawing.Size(107, 22);
            this.toolStripMenuItemAbout.Text = "About";
            this.toolStripMenuItemAbout.Click += new System.EventHandler(this.ToolStripMenuItemAbout_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.TabPanel);
            this.Controls.Add(this.tsMainMenu);
            this.DoubleBuffered = true;
            this.MinimumSize = new System.Drawing.Size(640, 480);
            this.Name = "MainForm";
            this.Text = "UOFiddler";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnClosing);
            this.TabPanel.ResumeLayout(false);
            this.contextMenuStripMainForm.ResumeLayout(false);
            this.StartTab.ResumeLayout(false);
            this.StartTab.PerformLayout();
            this.MultisTab.ResumeLayout(false);
            this.AnimationTab.ResumeLayout(false);
            this.ItemsTab.ResumeLayout(false);
            this.LandTilesTab.ResumeLayout(false);
            this.TextureTab.ResumeLayout(false);
            this.GumpsTab.ResumeLayout(false);
            this.SoundsTab.ResumeLayout(false);
            this.HueTab.ResumeLayout(false);
            this.FontsTab.ResumeLayout(false);
            this.ClilocTab.ResumeLayout(false);
            this.MapTab.ResumeLayout(false);
            this.LightTab.ResumeLayout(false);
            this.SpeechTab.ResumeLayout(false);
            this.SkillsTab.ResumeLayout(false);
            this.AnimDataTab.ResumeLayout(false);
            this.MultiMapTab.ResumeLayout(false);
            this.DressTab.ResumeLayout(false);
            this.TileDataTab.ResumeLayout(false);
            this.RadarColTab.ResumeLayout(false);
            this.SkillGrpTab.ResumeLayout(false);
            this.tsMainMenu.ResumeLayout(false);
            this.tsMainMenu.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl TabPanel;
        private System.Windows.Forms.TabPage AnimationTab;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.TabPage ItemsTab;
        private ItemShowControl itemShowControl;
        private AnimationListControl animationsControl;
        private System.Windows.Forms.TabPage LandTilesTab;
        private LandTilesControl landTilesControl;
        private System.Windows.Forms.TabPage GumpsTab;
        private GumpControl gumpsControl;
        private System.Windows.Forms.TabPage SoundsTab;
        private SoundsControl soundControl;
        private System.Windows.Forms.ToolStrip tsMainMenu;
        private System.Windows.Forms.TabPage MultisTab;
        private MultisControl multisControl;
        private System.Windows.Forms.TabPage HueTab;
        private HuesControl hueControl;
        private System.Windows.Forms.TabPage FontsTab;
        private FontsControl fontsControl;
        private System.Windows.Forms.TabPage ClilocTab;
        private ClilocControl clilocControl;
        private System.Windows.Forms.TabPage MapTab;
        private MapControl mapControl;
        private System.Windows.Forms.TabPage StartTab;
        private System.Windows.Forms.TabPage TextureTab;
        private TextureControl textureControl;
        private System.Windows.Forms.TabPage LightTab;
        private LightControl lightControl;
        private System.Windows.Forms.Label Versionlabel;
        private System.Windows.Forms.TabPage DressTab;
        private DressControl dressControl;
        private System.Windows.Forms.ToolStripDropDownButton SettingsMenu;
        private System.Windows.Forms.ToolStripMenuItem AlwaysOnTopMenuitem;
        private System.Windows.Forms.ToolStripMenuItem pathSettingsMenuItem;
        private System.Windows.Forms.ToolStripSeparator tsSettingsSeparator;
        private System.Windows.Forms.ToolStripMenuItem restartNeededMenuItem;
        private System.Windows.Forms.TabPage MultiMapTab;
        private MultiMapControl multimapControl;
        private System.Windows.Forms.TabPage SkillsTab;
        private SkillsControl skillsControl;
        private System.Windows.Forms.TabPage TileDataTab;
        private TileDataControl tileDataControl;
        private System.Windows.Forms.TabPage SpeechTab;
        private SpeechControl speechControl;
        private System.Windows.Forms.ToolStripDropDownButton ExternToolsDropDown;
        private System.Windows.Forms.ToolStripMenuItem manageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripMainForm;
        private System.Windows.Forms.ToolStripMenuItem unDockToolStripMenuItem;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButtonPlugins;
        private System.Windows.Forms.ToolStripMenuItem manageToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator tsPluginsSeparator;
        private System.Windows.Forms.TabPage AnimDataTab;
        private AnimDataControl animdataControl;
        private System.Windows.Forms.TabPage RadarColTab;
        private RadarColorControl radarColControl;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButtonView;
        private System.Windows.Forms.ToolStripMenuItem ToggleViewStart;
        private System.Windows.Forms.ToolStripMenuItem ToggleViewMulti;
        private System.Windows.Forms.ToolStripMenuItem ToggleViewAnimations;
        private System.Windows.Forms.ToolStripMenuItem ToggleViewItems;
        private System.Windows.Forms.ToolStripMenuItem ToggleViewLandTiles;
        private System.Windows.Forms.ToolStripMenuItem ToggleViewTexture;
        private System.Windows.Forms.ToolStripMenuItem ToggleViewGumps;
        private System.Windows.Forms.ToolStripMenuItem ToggleViewSounds;
        private System.Windows.Forms.ToolStripMenuItem ToggleViewHue;
        private System.Windows.Forms.ToolStripMenuItem ToggleViewFonts;
        private System.Windows.Forms.ToolStripMenuItem ToggleViewCliloc;
        private System.Windows.Forms.ToolStripMenuItem ToggleViewMap;
        private System.Windows.Forms.ToolStripMenuItem ToggleViewLight;
        private System.Windows.Forms.ToolStripMenuItem ToggleViewSpeech;
        private System.Windows.Forms.ToolStripMenuItem ToggleViewSkills;
        private System.Windows.Forms.ToolStripMenuItem ToggleViewAnimData;
        private System.Windows.Forms.ToolStripMenuItem ToggleViewMultiMap;
        private System.Windows.Forms.ToolStripMenuItem ToggleViewDress;
        private System.Windows.Forms.ToolStripMenuItem ToggleViewTileData;
        private System.Windows.Forms.ToolStripMenuItem ToggleViewRadarColor;
        private System.Windows.Forms.TabPage SkillGrpTab;
        private SkillGroupControl skillGroupControl;
        private System.Windows.Forms.ToolStripMenuItem ToggleViewSkillGrp;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButtonHelp;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemHelp;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemAbout;
    }
}

