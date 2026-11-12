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
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using Ultima;
using Ultima.Helpers;
using UoFiddler.Classes;
using UoFiddler.Controls.Classes;
using UoFiddler.Properties;
using UoFiddler.Controls.Plugin;

namespace UoFiddler.Forms
{
    public partial class MainForm : Form
    {
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hWnd, out NativeRect lpRect);

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeRect { public int Left, Top, Right, Bottom; }

        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONUP   = 0x0202;
        private const int MK_LBUTTON     = 0x0001;

        private readonly ILogger<MainForm> _log;

        private StatusStrip _statusStrip;
        private ToolStripStatusLabel _statusLabel;
        private ToolStripProgressBar _statusProgress;
        private readonly Dictionary<TabPage, string> _tabTitles = new();

        public MainForm() : this(AppLog.For<MainForm>()) { }

        public MainForm(ILogger<MainForm> logger)
        {
            _log = logger;
            InitializeComponent();
            InitializeStatusStrip();
            InitializeUnsavedTabMarkers();

            darkModeMenuItem.Checked = AppSettings.DarkMode;

            if (AppSettings.DarkMode)
            {
                StartTab.BackColor = Color.FromArgb(32, 32, 32);
                StartTab.BackgroundImage = Resources.UOFiddler_bk;
                reloadFilesMenuItem.ForeColor = Color.OrangeRed;
            }

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

            TabPanel.MouseWheel += TabPanel_MouseWheel;

            Icon = Options.GetFiddlerIcon();

            Versionlabel.Text = $"Version {FiddlerOptions.AppVersion.Major}.{FiddlerOptions.AppVersion.Minor}.{FiddlerOptions.AppVersion.Build}";
            Versionlabel.Left = StartTab.Size.Width - Versionlabel.Width - 5;

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

        private PathSettingsForm _pathSettingsForm = new PathSettingsForm();

        private void Click_path(object sender, EventArgs e)
        {
            if (_pathSettingsForm.IsDisposed)
            {
                _pathSettingsForm = new PathSettingsForm();
            }
            else
            {
                _pathSettingsForm.Focus();
            }

            _pathSettingsForm.TopMost = true;
            _pathSettingsForm.Show();
        }

        private void OnClickAlwaysTop(object sender, EventArgs e)
        {
            TopMost = AlwaysOnTopMenuitem.Checked;
            ControlEvents.FireAlwaysOnTopChangeEvent(TopMost);
        }

        private void OnClickDarkMode(object sender, EventArgs e)
        {
            AppSettings.DarkMode = darkModeMenuItem.Checked;
            AppSettings.Save();
            MessageBox.Show("Dark mode setting will take effect after restarting the application.", "Restart Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ReloadFiles(object sender, EventArgs e)
        {
            string unsavedFiles = GetUnsavedFiles();
            if (unsavedFiles.Length > 0)
            {
                DialogResult result =
                    MessageBox.Show($"Reload files and discard unsaved changes?\r\n\r\nThere are unsaved files:\r\n{unsavedFiles}",
                        "Unsaved Changes", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No)
                {
                    return;
                }
            }

            // Order matters: art has to be reloaded before tiledata for the latter to load properly.
            var steps = new (string Label, bool Run, Action Reload)[]
            {
                ("Verdata", true, Verdata.Initialize),
                ("Art and TileData", Options.LoadedUltimaClass["Art"] || Options.LoadedUltimaClass["TileData"], () =>
                {
                    Art.Reload();
                    TileData.Initialize();
                }),
                ("Hues", Options.LoadedUltimaClass["Hues"], Hues.Initialize),
                ("ASCII fonts", Options.LoadedUltimaClass["ASCIIFont"], AsciiText.Initialize),
                ("Unicode fonts", Options.LoadedUltimaClass["UnicodeFont"], UnicodeFonts.Initialize),
                ("Animdata", Options.LoadedUltimaClass["Animdata"], Animdata.Initialize),
                ("Light", Options.LoadedUltimaClass["Light"], Light.Reload),
                ("Skills", Options.LoadedUltimaClass["Skills"], Skills.Reload),
                ("Sounds", Options.LoadedUltimaClass["Sound"], Sounds.Initialize),
                ("Textures", Options.LoadedUltimaClass["Texture"], Textures.Reload),
                ("Gumps", Options.LoadedUltimaClass["Gumps"], Gumps.Reload),
                ("Animations", Options.LoadedUltimaClass["Animations"], Animations.Reload),
                ("Radar colors", Options.LoadedUltimaClass["RadarColor"], RadarCol.Initialize),
                ("Map", Options.LoadedUltimaClass["Map"], () =>
                {
                    MapHelper.CheckForNewMapSize();
                    Map.Reload();
                }),
                ("Multis", Options.LoadedUltimaClass["Multis"], Multis.Reload),
                ("Speech", Options.LoadedUltimaClass["Speech"], SpeechList.Initialize),
                ("Animation editor", Options.LoadedUltimaClass["AnimationEdit"], AnimationEdit.Reload),
            };

            var toRun = steps.Where(step => step.Run).ToList();

            using (new WaitCursorScope(this))
            {
                _statusProgress.Maximum = toRun.Count + 1;
                _statusProgress.Value = 0;
                _statusProgress.Visible = true;

                foreach (var step in toRun)
                {
                    SetStatus($"Reloading {step.Label}...");
                    step.Reload();
                    _statusProgress.Value++;
                }

                SetStatus("Refreshing tabs...");
                ControlEvents.FireFilePathChangeEvent();

                // Every tab has re-read its data from disk, so nothing is unsaved any more.
                foreach (string key in Options.ChangedUltimaClass.Keys.ToList())
                {
                    Options.ChangedUltimaClass[key] = false;
                }
                UpdateUnsavedTabMarkers();

                _statusProgress.Visible = false;
                SetStatus("Files reloaded.");
            }
        }

        private void InitializeStatusStrip()
        {
            _statusLabel = new ToolStripStatusLabel
            {
                Spring = true,
                TextAlign = ContentAlignment.MiddleLeft
            };

            _statusProgress = new ToolStripProgressBar
            {
                Visible = false,
                Size = new Size(160, 16)
            };

            _statusStrip = new StatusStrip
            {
                SizingGrip = false
            };
            _statusStrip.Items.Add(_statusLabel);
            _statusStrip.Items.Add(_statusProgress);

            // Added last so it docks before the tab panel fills the rest of the window.
            Controls.Add(_statusStrip);
        }

        /// <summary>
        /// Shows a message in the status bar and repaints it straight away, since the callers keep
        /// the UI thread busy.
        /// </summary>
        private void SetStatus(string message)
        {
            _statusLabel.Text = message;
            _statusStrip.Refresh();
        }

        /// <summary>
        /// Reloads the Extern Tools DropDown <see cref="FiddlerOptions.ExternTools"/>
        /// </summary>
        public void LoadExternToolStripMenu()
        {
            ExternToolsDropDown.DropDownItems.Clear();

            ToolStripMenuItem item = new ToolStripMenuItem
            {
                Text = "Manage.."
            };
            item.Click += OnClickToolManage;

            ExternToolsDropDown.DropDownItems.Add(item);
            ExternToolsDropDown.DropDownItems.Add(new ToolStripSeparator());

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

                ExternToolsDropDown.DropDownItems.Add(item);
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

            using (Process process = new Process())
            {
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
        }

        private ManageToolsForm _manageForm;

        private void OnClickToolManage(object sender, EventArgs e)
        {
            if (_manageForm?.IsDisposed == false)
            {
                return;
            }

            _manageForm = new ManageToolsForm(LoadExternToolStripMenu)
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

            _optionsForm = new OptionsForm(
                UpdateAllTileViews,
                UpdateItemsTab,
                UpdateSoundTab,
                UpdateMapTab)
            {
                TopMost = true
            };
            _optionsForm.Show();
        }

        /// <summary>
        /// Updates all tile view tabs
        /// </summary>
        private void UpdateAllTileViews()
        {
            UpdateItemsTab();
            UpdateLandTilesTab();
            UpdateTexturesTab();
            UpdateFontsTab();
        }

        /// <summary>
        /// Updates Item tab
        /// </summary>
        private void UpdateItemsTab()
        {
            itemShowControl.UpdateTileView();
        }

        /// <summary>
        /// Updates Land tiles tab
        /// </summary>
        private void UpdateLandTilesTab()
        {
            landTilesControl.UpdateTileView();
        }

        /// <summary>
        /// Updates Textures tab
        /// </summary>
        private void UpdateTexturesTab()
        {
            textureControl.UpdateTileView();
        }

        /// <summary>
        /// Updates Fonts tab
        /// </summary>
        private void UpdateFontsTab()
        {
            fontsControl.UpdateTileView();
        }

        /// <summary>
        /// Updates Map tab
        /// </summary>
        private void UpdateMapTab()
        {
            if (Options.LoadedUltimaClass["Map"])
            {
                Map.Reload();
            }

            ControlEvents.FireMapSizeChangeEvent();
        }

        /// <summary>
        /// Updates Sounds tab
        /// </summary>
        private void UpdateSoundTab()
        {
            soundControl.Reload();
        }

        private void OnClickUnDock(object sender, EventArgs e)
        {
            int tag = (int)TabPanel.SelectedTab.Tag;
            if (tag <= 0)
            {
                return;
            }

            new UnDockedForm(TabPanel.SelectedTab, ReDock).Show();
            TabPanel.TabPages.Remove(TabPanel.SelectedTab);
        }

        /// <summary>
        /// ReDocks closed Form
        /// </summary>
        /// <param name="oldTab"></param>
        public void ReDock(TabPage oldTab)
        {
            bool done = false;
            foreach (TabPage page in TabPanel.TabPages)
            {
                if ((int)page.Tag <= (int)oldTab.Tag)
                {
                    continue;
                }

                TabPanel.TabPages.Insert(TabPanel.TabPages.IndexOf(page), oldTab);
                done = true;
                break;
            }

            if (!done)
            {
                TabPanel.TabPages.Add(oldTab);
            }

            TabPanel.SelectedTab = oldTab;
        }

        private ManagePluginsForm _pluginsFormForm;

        private void OnClickManagePlugins(object sender, EventArgs e)
        {
            if (_pluginsFormForm?.IsDisposed == false)
            {
                return;
            }

            _pluginsFormForm = new ManagePluginsForm
            {
                TopMost = true
            };
            _pluginsFormForm.Show();
        }

        /// <summary>
        /// Lists the files with unsaved changes, one "- name" line each, or returns an empty string.
        /// </summary>
        private static string GetUnsavedFiles()
        {
            return Options.ChangedUltimaClass
                          .Where(key => key.Value)
                          .Aggregate(string.Empty, (current, key) => current + $"- {key.Key} \r\n");
        }

        private void OnClosing(object sender, FormClosingEventArgs e)
        {
            _log.LogInformation("MainForm - OnClosing - start");
            string files = GetUnsavedFiles();

            if (files.Length > 0)
            {
                DialogResult result =
                    MessageBox.Show($"Are you sure you want to quit?\r\n\r\nThere are unsaved files:\r\n{files}",
                        "UnSaved Changes", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
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

            _log.LogInformation("MainForm - OnClosing - unloading plugins");
            GlobalPlugins.Plugins.ClosePlugins();

            _log.LogInformation("MainForm - OnClosing - done");
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
                foreach (TabPage page in TabPanel.TabPages)
                {
                    if ((int)page.Tag <= tag)
                    {
                        continue;
                    }

                    TabPanel.TabPages.Insert(TabPanel.TabPages.IndexOf(page), thePage);
                    done = true;
                    break;
                }

                if (!done)
                {
                    TabPanel.TabPages.Add(thePage);
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
                foreach (TabPage page in TabPanel.TabPages)
                {
                    if ((int)page.Tag <= tag)
                    {
                        continue;
                    }

                    TabPanel.TabPages.Insert(TabPanel.TabPages.IndexOf(page), thePage);
                    done = true;
                    break;
                }

                if (!done)
                {
                    TabPanel.TabPages.Add(thePage);
                }

                Options.ChangedViewState[tag] = true;
            }
        }

        private TabPage TabFromTag(int tag)
        {
            switch (tag)
            {
                case 0: return StartTab;
                case 1: return MultisTab;
                case 2: return AnimationTab;
                case 3: return ItemsTab;
                case 4: return LandTilesTab;
                case 5: return TextureTab;
                case 6: return GumpsTab;
                case 7: return SoundsTab;
                case 8: return HuesTab;
                case 9: return FontsTab;
                case 10: return ClilocTab;
                case 11: return MapTab;
                case 12: return LightTab;
                case 13: return SpeechTab;
                case 14: return SkillsTab;
                case 15: return AnimDataTab;
                case 16: return MultiMapTab;
                case 17: return DressTab;
                case 18: return TileDataTab;
                case 19: return RadarColTab;
                case 20: return SkillGrpTab;
                case 21: return VerdataTab;
                case 22: return TileGroupsTab;
                default: return StartTab;
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
                case 21: return ToggleViewVerdata;
                case 22: return ToggleViewTileGroups;
                default: return ToggleViewStart;
            }
        }

        private IntPtr _tabScrollUpDown = IntPtr.Zero;

        private void TabPanel_MouseWheel(object sender, MouseEventArgs e)
        {
            // Only scroll tabs if the mouse is over the tab headers, not the tab content
            bool isOverTabHeader = false;
            for (int i = 0; i < TabPanel.TabCount; i++)
            {
                Rectangle tabRect = TabPanel.GetTabRect(i);
                if (tabRect.Contains(e.Location))
                {
                    isOverTabHeader = true;
                    break;
                }
            }

            if (!isOverTabHeader)
            {
                return; // Mouse is over tab content, let the content handle scrolling
            }

            // Cache the handle — it doesn't change while the form is open
            if (_tabScrollUpDown == IntPtr.Zero)
            {
                _tabScrollUpDown = FindWindowEx(TabPanel.Handle, IntPtr.Zero, "msctls_updown32", null);
            }

            if (_tabScrollUpDown == IntPtr.Zero)
            {
                return; // Scroll arrows not present — all tabs fit
            }

            GetClientRect(_tabScrollUpDown, out NativeRect rect);
            int midY = (rect.Bottom - rect.Top) / 2;
            // Left half = left arrow (scroll left), right half = right arrow (scroll right)
            int clickX = e.Delta < 0
                ? (rect.Right - rect.Left) * 3 / 4   // right button → scroll right
                : (rect.Right - rect.Left) / 4;       // left button → scroll left

            IntPtr lParam = new IntPtr((midY << 16) | (clickX & 0xFFFF));
            SendMessage(_tabScrollUpDown, WM_LBUTTONDOWN, new IntPtr(MK_LBUTTON), lParam);
            SendMessage(_tabScrollUpDown, WM_LBUTTONUP, IntPtr.Zero, lParam);
        }

        private void ToolStripMenuItemHelp_Click(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://uofiddler.polserver.com/",
                UseShellExecute = true
            });
        }

        private void ToolStripMenuItemAbout_Click(object sender, EventArgs e)
        {
            using (AboutBoxForm aboutBoxForm = new AboutBoxForm())
            {
                aboutBoxForm.ShowDialog(this);
            }
        }

        /// <summary>
        /// The <see cref="Options.ChangedUltimaClass"/> keys whose unsaved changes a tab shows,
        /// by tab tag.
        /// </summary>
        private static readonly Dictionary<int, string[]> _tabChangeKeys = new()
        {
            { 1, new[] { "Multis" } },
            { 2, new[] { "Animations" } },
            { 3, new[] { "Art" } },
            { 4, new[] { "Art" } },
            { 5, new[] { "Texture" } },
            { 6, new[] { "Gumps" } },
            { 7, new[] { "Sound" } },
            { 8, new[] { "Hues" } },
            { 9, new[] { "ASCIIFont", "UnicodeFont" } },
            { 10, new[] { "CliLoc" } },
            { 11, new[] { "Map" } },
            { 12, new[] { "Light" } },
            { 13, new[] { "Speech" } },
            { 14, new[] { "Skills" } },
            { 15, new[] { "Animdata" } },
            { 18, new[] { "TileData" } },
            { 19, new[] { "RadarCol" } },
            { 20, new[] { "SkillGrp" } },
            { 22, new[] { "TileGroups" } },
        };

        /// <summary>
        /// Prefixes a tab's title with "* " while the data it edits has unsaved changes. The change
        /// flags are plain dictionary entries set all over the controls, so they are polled.
        /// </summary>
        private void InitializeUnsavedTabMarkers()
        {
            var timer = new Timer(components) { Interval = 500 };
            timer.Tick += (_, _) => UpdateUnsavedTabMarkers();
            timer.Start();
        }

        private void UpdateUnsavedTabMarkers()
        {
            foreach (TabPage page in TabPanel.TabPages)
            {
                if (page.Tag is not int tag || !_tabChangeKeys.TryGetValue(tag, out string[] keys))
                {
                    continue;
                }

                if (!_tabTitles.TryGetValue(page, out string title))
                {
                    title = page.Text;
                    _tabTitles[page] = title;
                }

                bool unsaved = keys.Any(key => Options.ChangedUltimaClass.TryGetValue(key, out bool changed) && changed);
                string text = unsaved ? "* " + title : title;
                if (page.Text != text)
                {
                    page.Text = text;
                }
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Ctrl+1..Ctrl+9 select the first nine visible tabs.
            Keys key = keyData & Keys.KeyCode;
            if ((keyData & Keys.Modifiers) == Keys.Control)
            {
                int index = key is >= Keys.D1 and <= Keys.D9 ? key - Keys.D1
                    : key is >= Keys.NumPad1 and <= Keys.NumPad9 ? key - Keys.NumPad1
                    : -1;

                if (index >= 0 && index < TabPanel.TabCount)
                {
                    TabPanel.SelectedIndex = index;
                    return true;
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}