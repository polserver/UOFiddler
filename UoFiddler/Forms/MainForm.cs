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
using System.Drawing;
using System.Windows.Forms;
using UoFiddler.Classes;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Plugin;
using UoFiddler.Controls.Plugin.Types;
using UoFiddler.Controls.UserControls;

namespace UoFiddler.Forms
{
    public partial class MainForm : Form
    {
        private ItemShowAlternative _controlItemShowAlt;
        private TextureAlternative _controlTextureAlt;
        private LandTilesAlternative _controlLandTilesAlt;
        private static MainForm _refMarker;

        public MainForm()
        {
            _refMarker = this;
            InitializeComponent();

            if (FiddlerOptions.StoreFormState)
            {
                if (FiddlerOptions.MaximisedForm)
                {
                    StartPosition = FormStartPosition.Manual;
                    WindowState = FormWindowState.Maximized;
                }
                else
                {
                    if (IsOkFormStateLocation(FiddlerOptions.FormPosition, FiddlerOptions.FormSize))
                    {
                        StartPosition = FormStartPosition.Manual;
                        WindowState = FormWindowState.Normal;
                        Location = FiddlerOptions.FormPosition;
                        Size = FiddlerOptions.FormSize;
                    }
                }
            }

            Icon = Options.GetFiddlerIcon();

            Versionlabel.Text = $"Version {FiddlerOptions.AppVersion.Major}.{FiddlerOptions.AppVersion.Minor}.{FiddlerOptions.AppVersion.Build}";
            Versionlabel.Left = Start.Size.Width - Versionlabel.Width - 5;

            ChangeDesign();
            LoadExternToolStripMenu();
            GlobalPlugins.Plugins.FindPlugins($@"{Application.StartupPath}\plugins");

            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            foreach (AvailablePlugin plug in GlobalPlugins.Plugins.AvailablePlugins)
            {
                if (!plug.Loaded)
                {
                    continue;
                }

                plug.Instance.ModifyPluginToolStrip(toolStripDropDownButtonPlugins);
                plug.Instance.ModifyTabPages(TabPanel);
            }

            foreach (TabPage tab in TabPanel.TabPages)
            {
                if ((int)tab.Tag >= 0 && (int)tab.Tag < Options.ChangedViewState.Count &&
                    !Options.ChangedViewState[(int)tab.Tag])
                {
                    ToggleView(tab);
                }
            }
        }

        private PathSettings _pathSettings = new PathSettings();

        private void Click_path(object sender, EventArgs e)
        {
            if (_pathSettings.IsDisposed)
            {
                _pathSettings = new PathSettings();
            }
            else
            {
                _pathSettings.Focus();
            }

            _pathSettings.TopMost = true;
            _pathSettings.Show();
        }

        private void OnClickAlwaysTop(object sender, EventArgs e)
        {
            TopMost = AlwaysOnTopMenuitem.Checked;
            ControlEvents.FireAlwaysOnTopChangeEvent(TopMost);
        }

        private void Restart(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            Ultima.Verdata.Initialize();

            if (Options.LoadedUltimaClass["TileData"])
            {
                Ultima.TileData.Initialize();
            }

            if (Options.LoadedUltimaClass["Hues"])
            {
                Ultima.Hues.Initialize();
            }

            if (Options.LoadedUltimaClass["ASCIIFont"])
            {
                Ultima.ASCIIText.Initialize();
            }

            if (Options.LoadedUltimaClass["UnicodeFont"])
            {
                Ultima.UnicodeFonts.Initialize();
            }

            if (Options.LoadedUltimaClass["Animdata"])
            {
                Ultima.Animdata.Initialize();
            }

            if (Options.LoadedUltimaClass["Light"])
            {
                Ultima.Light.Reload();
            }

            if (Options.LoadedUltimaClass["Skills"])
            {
                Ultima.Skills.Reload();
            }

            if (Options.LoadedUltimaClass["Sound"])
            {
                Ultima.Sounds.Initialize();
            }

            if (Options.LoadedUltimaClass["Texture"])
            {
                Ultima.Textures.Reload();
            }

            if (Options.LoadedUltimaClass["Gumps"])
            {
                Ultima.Gumps.Reload();
            }

            if (Options.LoadedUltimaClass["Animations"])
            {
                Ultima.Animations.Reload();
            }

            if (Options.LoadedUltimaClass["Art"])
            {
                Ultima.Art.Reload();
            }

            if (Options.LoadedUltimaClass["RadarColor"])
            {
                Ultima.RadarCol.Initialize();
            }

            if (Options.LoadedUltimaClass["Map"])
            {
                Ultima.Files.CheckForNewMapSize();
                Ultima.Map.Reload();
            }

            if (Options.LoadedUltimaClass["Multis"])
            {
                Ultima.Multis.Reload();
            }

            if (Options.LoadedUltimaClass["Speech"])
            {
                Ultima.SpeechList.Initialize();
            }

            if (Options.LoadedUltimaClass["AnimationEdit"])
            {
                Ultima.AnimationEdit.Reload();
            }

            ControlEvents.FireFilePathChangeEvent();

            Cursor.Current = Cursors.Default;
        }

        private void OnClickAbout(object sender, EventArgs e)
        {
            new AboutBox().Show();
        }

        /// <summary>
        /// Reloads the Extern Tools DropDown <see cref="FiddlerOptions.ExternTools"/>
        /// </summary>
        public static void LoadExternToolStripMenu()
        {
            _refMarker.ExternToolsDropDown.DropDownItems.Clear();
            ToolStripMenuItem item = new ToolStripMenuItem
            {
                Text = "Manage.."
            };
            item.Click += _refMarker.OnClickToolManage;

            _refMarker.ExternToolsDropDown.DropDownItems.Add(item);
            _refMarker.ExternToolsDropDown.DropDownItems.Add(new ToolStripSeparator());

            if (FiddlerOptions.ExternTools is null)
            {
                return;
            }

            for (int i = 0; i < FiddlerOptions.ExternTools.Count; i++)
            {
                ExternTool tool = FiddlerOptions.ExternTools[i];
                item = new ToolStripMenuItem
                {
                    Text = tool.Name,
                    Tag = i
                };
                item.DropDownItemClicked += ExternTool_ItemClicked;

                ToolStripMenuItem sub = new ToolStripMenuItem
                {
                    Text = "Start",
                    Tag = -1
                };
                item.DropDownItems.Add(sub);
                item.DropDownItems.Add(new ToolStripSeparator());
                for (int j = 0; j < tool.Args.Count; j++)
                {
                    ToolStripMenuItem arg = new ToolStripMenuItem
                    {
                        Text = tool.ArgsName[j],
                        Tag = j
                    };
                    item.DropDownItems.Add(arg);
                }
                _refMarker.ExternToolsDropDown.DropDownItems.Add(item);
            }
        }

        private static void ExternTool_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            int argInfo = (int)e.ClickedItem.Tag;
            int toolInfo = (int)e.ClickedItem.OwnerItem.Tag;

            if (toolInfo < 0 || argInfo < -1)
            {
                return;
            }

            Process process = new Process();
            ExternTool tool = FiddlerOptions.ExternTools[toolInfo];
            process.StartInfo.FileName = tool.FileName;
            if (argInfo >= 0)
            {
                process.StartInfo.Arguments = tool.Args[argInfo];
            }

            try
            {
                process.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error starting tool",
                    MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1);
            }
        }

        private ManageTools _manageForm;

        private void OnClickToolManage(object sender, EventArgs e)
        {
            if (_manageForm?.IsDisposed == false)
            {
                return;
            }

            _manageForm = new ManageTools
            {
                TopMost = true
            };
            _manageForm.Show();
        }

        private OptionsForm _optionsForm;

        private void OnClickOptions(object sender, EventArgs e)
        {
            if (_optionsForm?.IsDisposed == false)
            {
                return;
            }

            _optionsForm = new OptionsForm
            {
                TopMost = true
            };
            _optionsForm.Show();
        }

        /// <summary>
        /// switches Alternative Design aka Hack'n'Slay attack damn...
        /// </summary>
        public static void ChangeDesign()
        {
            if (Options.DesignAlternative)
            {
                _refMarker._controlItemShowAlt = new ItemShowAlternative
                {
                    Dock = DockStyle.Fill,
                    Location = new Point(3, 3),
                    Name = "controlItemShow",
                    Size = new Size(613, 318),
                    TabIndex = 0
                };

                Control parent = _refMarker.controlItemShow.Parent;
                parent.Controls.Clear();
                parent.Controls.Add(_refMarker._controlItemShowAlt);
                parent.PerformLayout();

                _refMarker.controlItemShow.Dispose();
                _refMarker._controlTextureAlt = new TextureAlternative
                {
                    Dock = DockStyle.Fill,
                    Location = new Point(3, 3),
                    Name = "controlTexture",
                    Size = new Size(613, 318),
                    TabIndex = 0
                };

                parent = _refMarker.controlTexture.Parent;
                parent.Controls.Clear();
                parent.Controls.Add(_refMarker._controlTextureAlt);
                parent.PerformLayout();

                _refMarker.controlTexture.Dispose();

                _refMarker._controlLandTilesAlt = new LandTilesAlternative
                {
                    Dock = DockStyle.Fill,
                    Location = new Point(3, 3),
                    Name = "controlLandTiles",
                    Size = new Size(613, 318),
                    TabIndex = 0
                };

                parent = _refMarker.controlLandTiles.Parent;
                parent.Controls.Clear();
                parent.Controls.Add(_refMarker._controlLandTilesAlt);
                parent.PerformLayout();

                _refMarker.controlLandTiles.Dispose();
            }
            else
            {
                if (_refMarker._controlItemShowAlt == null)
                {
                    return;
                }

                _refMarker.controlItemShow = new ItemShow
                {
                    Dock = DockStyle.Fill,
                    Location = new Point(3, 3),
                    Name = "controlItemShow",
                    Size = new Size(613, 318),
                    TabIndex = 0
                };

                Control parent = _refMarker._controlItemShowAlt.Parent;
                parent.Controls.Clear();
                parent.Controls.Add(_refMarker.controlItemShow);
                parent.PerformLayout();

                _refMarker._controlItemShowAlt.Dispose();

                _refMarker.controlTexture = new Texture
                {
                    Dock = DockStyle.Fill,
                    Location = new Point(3, 3),
                    Name = "controlTexture",
                    Size = new Size(613, 318),
                    TabIndex = 0
                };

                parent = _refMarker._controlTextureAlt.Parent;
                parent.Controls.Clear();
                parent.Controls.Add(_refMarker.controlTexture);
                parent.PerformLayout();

                _refMarker._controlTextureAlt.Dispose();

                _refMarker.controlLandTiles = new LandTiles
                {
                    Dock = DockStyle.Fill,
                    Location = new Point(3, 3),
                    Name = "controlLandTiles",
                    Size = new Size(613, 318),
                    TabIndex = 0
                };

                parent = _refMarker._controlLandTilesAlt.Parent;
                parent.Controls.Clear();
                parent.Controls.Add(_refMarker.controlLandTiles);
                parent.PerformLayout();

                _refMarker._controlLandTilesAlt.Dispose();
            }
        }

        /// <summary>
        /// Reloads Item tab
        /// </summary>
        public static void ReloadItemTab()
        {
            if (Options.DesignAlternative)
            {
                _refMarker._controlItemShowAlt.ChangeTileSize();
            }
            else
            {
                _refMarker.controlItemShow.ChangeTileSize();
            }
        }

        /// <summary>
        /// Updates Map tab
        /// </summary>
        public static void ChangeMapSize()
        {
            if (Options.LoadedUltimaClass["Map"])
            {
                Ultima.Map.Reload();
            }

            ControlEvents.FireMapSizeChangeEvent();
        }

        private void OnClickUnDock(object sender, EventArgs e)
        {
            int tag = (int)TabPanel.SelectedTab.Tag;
            if (tag <= 0)
            {
                return;
            }

            new UnDocked(TabPanel.SelectedTab).Show();
            TabPanel.TabPages.Remove(TabPanel.SelectedTab);
        }

        /// <summary>
        /// ReDocks closed Form
        /// </summary>
        /// <param name="oldTab"></param>
        public static void ReDock(TabPage oldTab)
        {
            bool done = false;
            foreach (TabPage page in _refMarker.TabPanel.TabPages)
            {
                if ((int)page.Tag <= (int)oldTab.Tag)
                {
                    continue;
                }

                _refMarker.TabPanel.TabPages.Insert(_refMarker.TabPanel.TabPages.IndexOf(page), oldTab);
                done = true;
                break;
            }

            if (!done)
            {
                _refMarker.TabPanel.TabPages.Add(oldTab);
            }

            _refMarker.TabPanel.SelectedTab = oldTab;
        }

        private ManagePlugins _pluginsForm;

        private void OnClickManagePlugins(object sender, EventArgs e)
        {
            if (_pluginsForm?.IsDisposed == false)
            {
                return;
            }

            _pluginsForm = new ManagePlugins
            {
                TopMost = true
            };
            _pluginsForm.Show();
        }

        private void OnClosing(object sender, FormClosingEventArgs e)
        {
            string files = "";

            foreach (KeyValuePair<string, bool> key in Options.ChangedUltimaClass)
            {
                if (key.Value)
                {
                    files += $"{key.Key} ";
                }
            }

            if (files.Length > 0)
            {
                DialogResult result = MessageBox.Show($"Are you sure you want to quit?\r\n{files}",
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

            FiddlerOptions.MaximisedForm = WindowState == FormWindowState.Maximized;
            FiddlerOptions.FormPosition = Location;
            FiddlerOptions.FormSize = Size;

            GlobalPlugins.Plugins.ClosePlugins();
        }

        private void OnClickHelp(object sender, EventArgs e)
        {
            Process.Start("http://uofiddler.polserver.com/help.html");
        }

        private static bool IsOkFormStateLocation(Point loc, Size size)
        {
            if (loc.X < 0 || loc.Y < 0)
            {
                return false;
            }

            if (loc.X + size.Width > Screen.PrimaryScreen.WorkingArea.Width)
            {
                return false;
            }

            return loc.Y + size.Height <= Screen.PrimaryScreen.WorkingArea.Height;
        }

        private void ToggleView(object sender, EventArgs e)
        {
            ToolStripMenuItem theMenuItem = (ToolStripMenuItem)sender;
            TabPage thePage = TabFromTag((int)theMenuItem.Tag);

            int tag = (int)thePage.Tag;

            if (theMenuItem.Checked)
            {
                if (!TabPanel.TabPages.Contains(thePage))
                {
                    return;
                }

                theMenuItem.Checked = false;
                TabPanel.TabPages.Remove(thePage);
                Options.ChangedViewState[tag] = false;
            }
            else
            {
                theMenuItem.Checked = true;
                bool done = false;
                foreach (TabPage page in _refMarker.TabPanel.TabPages)
                {
                    if ((int)page.Tag <= tag)
                    {
                        continue;
                    }

                    _refMarker.TabPanel.TabPages.Insert(_refMarker.TabPanel.TabPages.IndexOf(page), thePage);
                    done = true;
                    break;
                }
                if (!done)
                {
                    _refMarker.TabPanel.TabPages.Add(thePage);
                }

                Options.ChangedViewState[tag] = true;
            }
        }

        private void ToggleView(TabPage thePage)
        {
            int tag = (int)thePage.Tag;
            ToolStripMenuItem theMenuItem = MenuFromTag(tag);

            if (theMenuItem.Checked)
            {
                if (!TabPanel.TabPages.Contains(thePage))
                {
                    return;
                }

                theMenuItem.Checked = false;
                TabPanel.TabPages.Remove(thePage);
                Options.ChangedViewState[tag] = false;
            }
            else
            {
                theMenuItem.Checked = true;
                bool done = false;
                foreach (TabPage page in _refMarker.TabPanel.TabPages)
                {
                    if ((int)page.Tag <= tag)
                    {
                        continue;
                    }

                    _refMarker.TabPanel.TabPages.Insert(_refMarker.TabPanel.TabPages.IndexOf(page), thePage);
                    done = true;
                    break;
                }

                if (!done)
                {
                    _refMarker.TabPanel.TabPages.Add(thePage);
                }

                Options.ChangedViewState[tag] = true;
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
    }
}