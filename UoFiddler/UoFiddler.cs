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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
using Host;

namespace UoFiddler
{
    public partial class UoFiddler : Form
    {
        public static string Version = "4.6";
        private FiddlerControls.ItemShowAlternative controlItemShowAlt;
        private FiddlerControls.TextureAlternative controlTextureAlt;
        private FiddlerControls.LandTilesAlternative controlLandTilesAlt;
        private static UoFiddler refmarker;

        public UoFiddler()
        {
            refmarker = this;
            InitializeComponent();
            if (Options.StoreFormState)
            {
                if (Options.MaximisedForm)
                {
                    this.StartPosition = FormStartPosition.Manual;
                    this.WindowState = FormWindowState.Maximized;
                }
                else
                {
                    if (isOkFormStateLocation(Options.FormPosition, Options.FormSize))
                    {
                        this.StartPosition = FormStartPosition.Manual;
                        this.WindowState = FormWindowState.Normal;
                        this.Location = Options.FormPosition;
                        this.Size = Options.FormSize;
                    }
                }
            }
            this.Icon = FiddlerControls.Options.GetFiddlerIcon();
            Versionlabel.Text = "Version " + Version;
            Versionlabel.Left = Start.Size.Width - Versionlabel.Width - 5;
            ChangeDesign();
            LoadExternToolStripMenu();
            GlobalPlugins.Plugins.FindPlugins(Application.StartupPath + @"\plugins");

            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            foreach (Host.Types.AvailablePlugin plug in GlobalPlugins.Plugins.AvailablePlugins)
            {
                if (plug.Loaded)
                {
                    plug.Instance.ModifyPluginToolStrip(this.toolStripDropDownButtonPlugins);
                    plug.Instance.ModifyTabPages(this.TabPanel);
                }
            }

            foreach (TabPage tab in TabPanel.TabPages)
            {
                if (((int)tab.Tag >= 0) && ((int)tab.Tag < FiddlerControls.Options.ChangedViewState.Count))
                {
                    if (!FiddlerControls.Options.ChangedViewState[(int)tab.Tag])
                        ToggleView(tab);
                }
            }
        }

        private PathSettings m_Path = new PathSettings();
        private void click_path(object sender, EventArgs e)
        {
            if (m_Path.IsDisposed)
                m_Path = new PathSettings();
            else
                m_Path.Focus();
            m_Path.TopMost = true;
            m_Path.Show();
        }

        private void onClickAlwaysTop(object sender, EventArgs e)
        {
            this.TopMost = AlwaysOnTopMenuitem.Checked;
            FiddlerControls.Events.FireAlwaysOnTopChangeEvent(this.TopMost);
        }

        private void Restart(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            Ultima.Verdata.Initialize();
            if (FiddlerControls.Options.LoadedUltimaClass["TileData"])
                Ultima.TileData.Initialize();
            if (FiddlerControls.Options.LoadedUltimaClass["Hues"])
                Ultima.Hues.Initialize();
            if (FiddlerControls.Options.LoadedUltimaClass["ASCIIFont"])
                Ultima.ASCIIText.Initialize();
            if (FiddlerControls.Options.LoadedUltimaClass["UnicodeFont"])
                Ultima.UnicodeFonts.Initialize();
            if (FiddlerControls.Options.LoadedUltimaClass["Animdata"])
                Ultima.Animdata.Initialize();
            if (FiddlerControls.Options.LoadedUltimaClass["Light"])
                Ultima.Light.Reload();
            if (FiddlerControls.Options.LoadedUltimaClass["Skills"])
                Ultima.Skills.Reload();
            if (FiddlerControls.Options.LoadedUltimaClass["Sound"])
                Ultima.Sounds.Initialize();
            if (FiddlerControls.Options.LoadedUltimaClass["Texture"])
                Ultima.Textures.Reload();
            if (FiddlerControls.Options.LoadedUltimaClass["Gumps"])
                Ultima.Gumps.Reload();
            if (FiddlerControls.Options.LoadedUltimaClass["Animations"])
                Ultima.Animations.Reload();
            if (FiddlerControls.Options.LoadedUltimaClass["Art"])
                Ultima.Art.Reload();
            if (FiddlerControls.Options.LoadedUltimaClass["RadarColor"])
                Ultima.RadarCol.Initialize();
            if (FiddlerControls.Options.LoadedUltimaClass["Map"])
            {
                Ultima.Files.CheckForNewMapSize();
                Ultima.Map.Reload();
            }
            if (FiddlerControls.Options.LoadedUltimaClass["Multis"])
                Ultima.Multis.Reload();
            if (FiddlerControls.Options.LoadedUltimaClass["Speech"])
                Ultima.SpeechList.Initialize();
            if (FiddlerControls.Options.LoadedUltimaClass["AnimationEdit"])
                Ultima.AnimationEdit.Reload();

            FiddlerControls.Events.FireFilePathChangeEvent();

            Cursor.Current = Cursors.Default;
        }

        private void OnClickAbout(object sender, EventArgs e)
        {
            new AboutBox().Show();
        }

        /// <summary>
        /// Reloads the Extern Tools DropDown <see cref="Options.ExternTools"/>
        /// </summary>
        public static void LoadExternToolStripMenu()
        {
            refmarker.ExternToolsDropDown.DropDownItems.Clear();
            ToolStripMenuItem item = new ToolStripMenuItem();
            item.Text = "Manage..";
            item.Click += new System.EventHandler(refmarker.onClickToolManage);
            refmarker.ExternToolsDropDown.DropDownItems.Add(item);
            refmarker.ExternToolsDropDown.DropDownItems.Add(new ToolStripSeparator());
            for (int i = 0; i < Options.ExternTools.Count; i++)
            {
                ExternTool tool = Options.ExternTools[i];
                item = new ToolStripMenuItem();
                item.Text = tool.Name;
                item.Tag = i;
                item.DropDownItemClicked += new ToolStripItemClickedEventHandler(refmarker.ExternTool_ItemClicked);
                ToolStripMenuItem sub = new ToolStripMenuItem();
                sub.Text = "Start";
                sub.Tag = -1;
                item.DropDownItems.Add(sub);
                item.DropDownItems.Add(new ToolStripSeparator());
                for (int j = 0; j < tool.Args.Count; j++)
                {
                    ToolStripMenuItem arg = new ToolStripMenuItem();
                    arg.Text = tool.ArgsName[j];
                    arg.Tag = j;
                    item.DropDownItems.Add(arg);
                }
                refmarker.ExternToolsDropDown.DropDownItems.Add(item);
            }
        }

        private void ExternTool_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            int arginfo = (int)e.ClickedItem.Tag;
            int toolinfo = (int)e.ClickedItem.OwnerItem.Tag;

            if (toolinfo >= 0)
            {
                if (arginfo >= -1)
                {
                    Process P = new Process();
                    ExternTool tool = Options.ExternTools[toolinfo];
                    P.StartInfo.FileName = tool.FileName;
                    if (arginfo >= 0)
                        P.StartInfo.Arguments = tool.Args[arginfo];
                    try
                    {
                        P.Start();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error starting tool",
                            MessageBoxButtons.OK, MessageBoxIcon.Error,
                            MessageBoxDefaultButton.Button1);
                    }
                }
            }
        }

        private ManageTools manageform;
        private void onClickToolManage(object sender, EventArgs e)
        {
            if ((manageform == null) || (manageform.IsDisposed))
            {
                manageform = new ManageTools();
                manageform.TopMost = true;
                manageform.Show();
            }
        }

        private OptionsForm optionsform;
        private void OnClickOptions(object sender, EventArgs e)
        {
            if ((optionsform == null) || (optionsform.IsDisposed))
            {
                optionsform = new OptionsForm();
                optionsform.TopMost = true;
                optionsform.Show();
            }
        }

        /// <summary>
        /// switches Alternative Design aka Hack'n'Slay attack damn...
        /// </summary>
        public static void ChangeDesign()
        {
            if (FiddlerControls.Options.DesignAlternative)
            {
                refmarker.controlItemShowAlt = new FiddlerControls.ItemShowAlternative();
                refmarker.controlItemShowAlt.Dock = System.Windows.Forms.DockStyle.Fill;
                refmarker.controlItemShowAlt.Location = new System.Drawing.Point(3, 3);
                refmarker.controlItemShowAlt.Name = "controlItemShow";
                refmarker.controlItemShowAlt.Size = new System.Drawing.Size(613, 318);
                refmarker.controlItemShowAlt.TabIndex = 0;
                Control parent = refmarker.controlItemShow.Parent;
                parent.Controls.Clear();
                parent.Controls.Add(refmarker.controlItemShowAlt);
                parent.PerformLayout();
                refmarker.controlItemShow.Dispose();

                refmarker.controlTextureAlt = new FiddlerControls.TextureAlternative();
                refmarker.controlTextureAlt.Dock = System.Windows.Forms.DockStyle.Fill;
                refmarker.controlTextureAlt.Location = new System.Drawing.Point(3, 3);
                refmarker.controlTextureAlt.Name = "controlTexture";
                refmarker.controlTextureAlt.Size = new System.Drawing.Size(613, 318);
                refmarker.controlTextureAlt.TabIndex = 0;
                parent = refmarker.controlTexture.Parent;
                parent.Controls.Clear();
                parent.Controls.Add(refmarker.controlTextureAlt);
                parent.PerformLayout();
                refmarker.controlTexture.Dispose();

                refmarker.controlLandTilesAlt = new FiddlerControls.LandTilesAlternative();
                refmarker.controlLandTilesAlt.Dock = System.Windows.Forms.DockStyle.Fill;
                refmarker.controlLandTilesAlt.Location = new System.Drawing.Point(3, 3);
                refmarker.controlLandTilesAlt.Name = "controlLandTiles";
                refmarker.controlLandTilesAlt.Size = new System.Drawing.Size(613, 318);
                refmarker.controlLandTilesAlt.TabIndex = 0;
                parent = refmarker.controlLandTiles.Parent;
                parent.Controls.Clear();
                parent.Controls.Add(refmarker.controlLandTilesAlt);
                parent.PerformLayout();
                refmarker.controlLandTiles.Dispose();
            }
            else
            {
                if (refmarker.controlItemShowAlt == null)
                    return;

                refmarker.controlItemShow = new FiddlerControls.ItemShow();
                refmarker.controlItemShow.Dock = System.Windows.Forms.DockStyle.Fill;
                refmarker.controlItemShow.Location = new System.Drawing.Point(3, 3);
                refmarker.controlItemShow.Name = "controlItemShow";
                refmarker.controlItemShow.Size = new System.Drawing.Size(613, 318);
                refmarker.controlItemShow.TabIndex = 0;
                Control parent = refmarker.controlItemShowAlt.Parent;
                parent.Controls.Clear();
                parent.Controls.Add(refmarker.controlItemShow);
                parent.PerformLayout();
                refmarker.controlItemShowAlt.Dispose();

                refmarker.controlTexture = new FiddlerControls.Texture();
                refmarker.controlTexture.Dock = System.Windows.Forms.DockStyle.Fill;
                refmarker.controlTexture.Location = new System.Drawing.Point(3, 3);
                refmarker.controlTexture.Name = "controlTexture";
                refmarker.controlTexture.Size = new System.Drawing.Size(613, 318);
                refmarker.controlTexture.TabIndex = 0;
                parent = refmarker.controlTextureAlt.Parent;
                parent.Controls.Clear();
                parent.Controls.Add(refmarker.controlTexture);
                parent.PerformLayout();
                refmarker.controlTextureAlt.Dispose();

                refmarker.controlLandTiles = new FiddlerControls.LandTiles();
                refmarker.controlLandTiles.Dock = System.Windows.Forms.DockStyle.Fill;
                refmarker.controlLandTiles.Location = new System.Drawing.Point(3, 3);
                refmarker.controlLandTiles.Name = "controlLandTiles";
                refmarker.controlLandTiles.Size = new System.Drawing.Size(613, 318);
                refmarker.controlLandTiles.TabIndex = 0;
                parent = refmarker.controlLandTilesAlt.Parent;
                parent.Controls.Clear();
                parent.Controls.Add(refmarker.controlLandTiles);
                parent.PerformLayout();
                refmarker.controlLandTilesAlt.Dispose();
            }
        }

        /// <summary>
        /// Reloads Itemtab
        /// </summary>
        public static void ReloadItemTab()
        {
            if (FiddlerControls.Options.DesignAlternative)
                refmarker.controlItemShowAlt.ChangeTileSize();
            else
                refmarker.controlItemShow.ChangeTileSize();
        }

        /// <summary>
        /// Updates Map tab
        /// </summary>
        public static void ChangeMapSize()
        {
            if (FiddlerControls.Options.LoadedUltimaClass["Map"])
                Ultima.Map.Reload();
            FiddlerControls.Events.FireMapSizeChangeEvent();
        }

        private void OnClickUndock(object sender, EventArgs e)
        {
            int tag = (int)TabPanel.SelectedTab.Tag;
            if (tag > 0)
            {
                new UnDocked(TabPanel.SelectedTab).Show();
                TabPanel.TabPages.Remove(TabPanel.SelectedTab);
            }
        }

        /// <summary>
        /// ReDocks closed Form
        /// </summary>
        /// <param name="contr"></param>
        /// <param name="index"></param>
        /// <param name="name"></param>
        public static void ReDock(TabPage oldtab)
        {
            bool done = false;
            foreach (TabPage page in refmarker.TabPanel.TabPages)
            {
                if ((int)page.Tag > (int)oldtab.Tag)
                {
                    refmarker.TabPanel.TabPages.Insert(refmarker.TabPanel.TabPages.IndexOf(page), oldtab);
                    done = true;
                    break;
                }
            }
            if (!done)
                refmarker.TabPanel.TabPages.Add(oldtab);
            refmarker.TabPanel.SelectedTab = oldtab;
        }

        private ManagePlugins pluginsform;
        private void onClickManagePlugins(object sender, EventArgs e)
        {
            if ((pluginsform == null) || (pluginsform.IsDisposed))
            {
                pluginsform = new ManagePlugins();
                pluginsform.TopMost = true;
                pluginsform.Show();
            }
        }

        private void OnClosing(object sender, FormClosingEventArgs e)
        {
            string files = "";
            foreach (KeyValuePair<string, bool> key in FiddlerControls.Options.ChangedUltimaClass)
            {
                if (key.Value)
                    files += key.Key + " ";
            }
            if (files.Length > 0)
            {
                DialogResult result =
                        MessageBox.Show(String.Format("Are you sure you want to quit?\r\n{0}", files),
                        "UnSaved Changes",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }
            Options.MaximisedForm = this.WindowState == FormWindowState.Maximized;
            Options.FormPosition = this.Location;
            Options.FormSize = this.Size;

            GlobalPlugins.Plugins.ClosePlugins();
        }

        private void OnClickHelp(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(@"http://uofiddler.polserver.com/help.html");
        }

        private static bool isOkFormStateLocation(Point loc, Size size)
        {
            if (loc.X < 0 || loc.Y < 0)
                return false;
            else if (loc.X + size.Width > Screen.PrimaryScreen.WorkingArea.Width)
                return false;
            else if (loc.Y + size.Height > Screen.PrimaryScreen.WorkingArea.Height)
                return false;
            return true;
        }

        #region View Menu Code
        private void ToggleView(object sender, EventArgs e)
        {
            ToolStripMenuItem themenuitem = (ToolStripMenuItem)sender;
            TabPage thepage = TabFromTag((int)themenuitem.Tag);
            int tag = (int)thepage.Tag;
            if (themenuitem.Checked)
            {
                if (!TabPanel.TabPages.Contains(thepage))
                    return;
                themenuitem.Checked = false;
                TabPanel.TabPages.Remove(thepage);
                FiddlerControls.Options.ChangedViewState[tag] = false;
            }
            else
            {
                themenuitem.Checked = true;
                bool done = false;
                foreach (TabPage page in refmarker.TabPanel.TabPages)
                {
                    if ((int)page.Tag > tag)
                    {
                        refmarker.TabPanel.TabPages.Insert(refmarker.TabPanel.TabPages.IndexOf(page), thepage);
                        done = true;
                        break;
                    }
                }
                if (!done)
                    refmarker.TabPanel.TabPages.Add(thepage);
                FiddlerControls.Options.ChangedViewState[tag] = true;
            }
        }

        private void ToggleView(TabPage thepage)
        {
            int tag = (int)thepage.Tag;
            ToolStripMenuItem themenuitem = MenuFromTag(tag);

            if (themenuitem.Checked)
            {
                if (!TabPanel.TabPages.Contains(thepage))
                    return;
                themenuitem.Checked = false;
                TabPanel.TabPages.Remove(thepage);
                FiddlerControls.Options.ChangedViewState[tag] = false;
            }
            else
            {
                themenuitem.Checked = true;
                bool done = false;
                foreach (TabPage page in refmarker.TabPanel.TabPages)
                {
                    if ((int)page.Tag > tag)
                    {
                        refmarker.TabPanel.TabPages.Insert(refmarker.TabPanel.TabPages.IndexOf(page), thepage);
                        done = true;
                        break;
                    }
                }
                if (!done)
                    refmarker.TabPanel.TabPages.Add(thepage);
                FiddlerControls.Options.ChangedViewState[tag] = true;
            }
        }

        private TabPage TabFromTag(int tag)
        {
            switch (tag)
            {
                case 0: return Start;
                case 1: return Multis;
                case 2: return Animation;
                case 3: return Items;
                case 4: return LandTiles;
                case 5: return Texture;
                case 6: return Gumps;
                case 7: return Sounds;
                case 8: return Hue;
                case 9: return fonts;
                case 10: return Cliloc;
                case 11: return map;
                case 12: return Light;
                case 13: return speech;
                case 14: return Skills;
                case 15: return AnimData;
                case 16: return multimap;
                case 17: return Dress;
                case 18: return TileDatas;
                case 19: return RadarCol;
                case 20: return SkillGrp;
                default: return Start;
            }
        }

        private ToolStripMenuItem MenuFromTag(int tag)
        {
            switch (tag)
            {
                case 0: return ToggleViewStart;
                case 1: return ToggleViewMulti;
                case 2: return ToggleViewAnimations;
                case 3: return ToggleViewItems;
                case 4: return ToggleViewLandTiles;
                case 5: return ToggleViewTexture;
                case 6: return ToggleViewGumps;
                case 7: return ToggleViewSounds;
                case 8: return ToggleViewHue;
                case 9: return ToggleViewFonts;
                case 10: return ToggleViewCliloc;
                case 11: return ToggleViewMap;
                case 12: return ToggleViewLight;
                case 13: return ToggleViewSpeech;
                case 14: return ToggleViewSkills;
                case 15: return ToggleViewAnimData;
                case 16: return ToggleViewMultiMap;
                case 17: return ToggleViewDress;
                case 18: return ToggleViewTileData;
                case 19: return ToggleViewRadarColor;
                case 20: return ToggleViewSkillGrp;
                default: return ToggleViewStart;
            }
        }
        #endregion
    }
}