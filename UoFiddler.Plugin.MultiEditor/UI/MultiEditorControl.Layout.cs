/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Drawing;
using System.Windows.Forms;

namespace UoFiddler.Plugin.MultiEditor.UI
{
    public partial class MultiEditorControl
    {
        // ---------------------------------------------------------------- window resizing

        private Form _hostForm;
        private Timer _resizeTimer;
        private bool _sidebarsHeld;

        /// <summary>
        /// Starts OpenGL's windowing library ahead of the first show. It takes around 400 ms once
        /// per process, which is better spent while the application sits idle after start-up than
        /// when the tab is first clicked.
        /// </summary>
        internal static void WarmUpOpenGl()
        {
            try
            {
                OpenTK.Windowing.Desktop.GLFWProvider.EnsureInitialized();
            }
            catch (Exception)
            {
                // The canvas reports the problem when it is created.
            }
        }

        /// <summary>The tab can be undocked into its own window, so the form is looked up again each time.</summary>
        private void WatchHostForm()
        {
            Form form = FindForm();
            if (form == _hostForm)
            {
                return;
            }

            if (_hostForm != null)
            {
                _hostForm.ResizeBegin -= HostForm_ResizeBegin;
                _hostForm.ResizeEnd -= HostForm_ResizeEnd;
                _hostForm.FormClosing -= HostForm_FormClosing;
            }

            _hostForm = form;
            if (form != null)
            {
                form.ResizeBegin += HostForm_ResizeBegin;
                form.ResizeEnd += HostForm_ResizeEnd;
                form.FormClosing += HostForm_FormClosing;
            }
        }

        /// <summary>
        /// While the window border is dragged, the two sidebars (hundreds of nested controls, most
        /// of a resize's cost) are laid out a few times a second instead of on every mouse step.
        /// The canvas and toolbars keep following the window live.
        /// </summary>
        private void HostForm_ResizeBegin(object sender, EventArgs e)
        {
            if (!_built || _sidebarsHeld)
            {
                return;
            }

            // Undocked, the sidebars keep their size and just ride along with their panel, which
            // costs nothing; docked, every step would lay out everything inside them.
            _sidebarsHeld = true;
            _sideSplit.Dock = DockStyle.None;
            _resizeTimer ??= new Timer { Interval = 300 };
            _resizeTimer.Tick -= ResizeTimer_Tick;
            _resizeTimer.Tick += ResizeTimer_Tick;
            _resizeTimer.Start();
        }

        private void ResizeTimer_Tick(object sender, EventArgs e)
        {
            if (_sidebarsHeld && _sideSplit.Parent != null && _sideSplit.Bounds != _sideSplit.Parent.ClientRectangle)
            {
                _sideSplit.Bounds = _sideSplit.Parent.ClientRectangle;
            }
        }

        private void HostForm_ResizeEnd(object sender, EventArgs e)
        {
            if (!_sidebarsHeld)
            {
                return;
            }

            _sidebarsHeld = false;
            _resizeTimer?.Stop();
            _sideSplit.Dock = DockStyle.Fill;
            LayoutSidebars();
        }

        /// <summary>
        /// The sidebars sit in a panel docked right with a plain splitter, not in a SplitContainer:
        /// they keep their width when the window is resized, and docked panels lay out far more cheaply.
        /// Between the two sidebars (the tile browser at full height, and beside it floors and
        /// selection) is a SplitContainer.
        /// </summary>
        private void SideSplitter_SplitterMoved(object sender, SplitterEventArgs e)
        {
            RememberSidebarWidths();
        }

        private void SideSplit_SplitterMoved(object sender, SplitterEventArgs e)
        {
            RememberSidebarWidths();
        }

        private bool _browserShown = true;
        private bool _inspectorShown = true;
        private bool _layingOutSidebars;
        private bool _applyingOwnSettings;

        private EditorCommand _performanceInfo;
        private PerformancePanel _performancePanel;
        private bool _performanceShown;

        /// <summary>Narrowest a sidebar gets: below this its controls no longer fit.</summary>
        private const int _minSidebar = 240;

        /// <summary>
        /// Gives each sidebar its width from the settings, never below the minimum; a hidden one
        /// hands its space to the canvas. When the window is too narrow for both at their saved
        /// widths, the tile browser gives way first, then the other one, and the canvas keeps a
        /// strip. Safe to call at any time: it is run again whenever the editor changes size,
        /// because at start-up the first layout happens before the window has its real width.
        /// </summary>
        private void LayoutSidebars()
        {
            if (_sideSplit == null || _sidePanel == null)
            {
                return;
            }

            const int paletteWidth = 96;
            const int minCanvas = 200;
            int browser = Math.Max(_minSidebar, EditorSettings.Current.TileBrowserWidth);
            int inspector = Math.Max(_minSidebar, EditorSettings.Current.InspectorWidth);
            bool both = _browserShown && _inspectorShown;
            int bar = both ? _sideSplit.SplitterWidth : 0;

            _layingOutSidebars = true;
            try
            {
                int available = Width - paletteWidth - minCanvas - _sideSplitter.Width - bar;
                int over = (_browserShown ? browser : 0) + (_inspectorShown ? inspector : 0) - available;
                if (over > 0 && Width > 0)
                {
                    if (_browserShown)
                    {
                        int cut = Math.Min(over, browser - _minSidebar);
                        browser -= cut;
                        over -= cut;
                    }

                    if (over > 0 && _inspectorShown)
                    {
                        inspector -= Math.Min(over, inspector - _minSidebar);
                    }
                }

                int width = (_browserShown ? browser : 0) + (_inspectorShown ? inspector : 0) + bar;
                _sidePanel.Visible = _sideSplitter.Visible = width > 0;
                if (width == 0)
                {
                    return;
                }

                // Minimums off while the panels are resized: a split container refuses a width or a
                // distance that breaks them, and the old values belong to the old size.
                _sideSplit.Panel1MinSize = 25;
                _sideSplit.Panel2MinSize = 25;
                _sideSplit.Panel1Collapsed = !_browserShown;
                _sideSplit.Panel2Collapsed = !_inspectorShown;
                _sidePanel.Width = width;
                _sidePanel.PerformLayout();

                if (both)
                {
                    int distance = _sideSplit.Width - inspector - _sideSplit.SplitterWidth;
                    if (distance >= 25 && distance <= _sideSplit.Width - 25 - _sideSplit.SplitterWidth)
                    {
                        _sideSplit.SplitterDistance = distance;
                    }

                    if (_sideSplit.Width >= (2 * _minSidebar) + _sideSplit.SplitterWidth)
                    {
                        _sideSplit.Panel1MinSize = _minSidebar;
                        _sideSplit.Panel2MinSize = _minSidebar;
                    }
                }
            }
            catch (InvalidOperationException)
            {
                // The split container was too small for the numbers; the next resize tries again.
            }
            finally
            {
                _layingOutSidebars = false;
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (_built && !_sidebarsHeld)
            {
                LayoutSidebars();
            }
        }

        /// <summary>A splitter was dragged: the sidebar widths go into the profile.</summary>
        private void RememberSidebarWidths()
        {
            if (_layingOutSidebars || !_built || _sidebarsHeld || !Visible)
            {
                return;
            }

            // Only a real drag is remembered: widths below the minimum come from a layout in
            // progress (the window still opening), not from the user.
            EditorSettings settings = EditorSettings.Current.Clone();
            if (_browserShown && _inspectorShown)
            {
                if (_sideSplit.Panel1.Width < _minSidebar || _sideSplit.Panel2.Width < _minSidebar)
                {
                    return;
                }

                settings.TileBrowserWidth = _sideSplit.Panel1.Width;
                settings.InspectorWidth = _sideSplit.Panel2.Width;
            }
            else if (_sidePanel.Width < _minSidebar)
            {
                return;
            }
            else if (_browserShown)
            {
                settings.TileBrowserWidth = _sidePanel.Width;
            }
            else if (_inspectorShown)
            {
                settings.InspectorWidth = _sidePanel.Width;
            }

            ApplyOwnSettings(settings);
        }

        /// <summary>Applies settings this control changed itself, which it then need not react to.</summary>
        private void ApplyOwnSettings(EditorSettings settings)
        {
            _applyingOwnSettings = true;
            try
            {
                settings.Apply();
            }
            finally
            {
                _applyingOwnSettings = false;
            }
        }

        private void OnSettingsChanged()
        {
            if (!_built || _applyingOwnSettings)
            {
                return;
            }

            _emptyHint.BackColor = EditorSettings.Current.EffectiveBackground;
            _emptyHint.ForeColor = EditorSettings.Current.BackgroundText;
            _canvas.Invalidate();
        }

        private void ShowSettings()
        {
            using var dialog = new EditorSettingsDialog(EditorSettings.Current);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                dialog.Result.Apply();
            }
        }

        private void ToggleSidebar(bool browser)
        {
            if (browser)
            {
                _browserShown = !_browserShown;
            }
            else
            {
                _inspectorShown = !_inspectorShown;
            }

            EditorSettings settings = EditorSettings.Current.Clone();
            settings.ShowTileBrowser = _browserShown;
            settings.ShowInspector = _inspectorShown;
            ApplyOwnSettings(settings);
            LayoutSidebars();
        }

        /// <summary>Shows or hides the performance box; timings are only collected while it is shown.</summary>
        private void TogglePerformanceInfo()
        {
            _performanceShown = !_performanceShown;
            _canvas.CollectStats = _performanceShown;
            _performancePanel.Visible = _performanceShown;
            if (_performanceShown)
            {
                _performancePanel.UpdateNumbers();
                PlacePerformancePanel();
            }

            _commands.RefreshStates();
        }

        private void PlacePerformancePanel()
        {
            if (_performancePanel == null)
            {
                return;
            }

            int margin = LogicalToDeviceUnits(8);
            _performancePanel.Location = new Point(_centre.ClientSize.Width - _performancePanel.Width - margin, _canvas.Top + margin);
        }
    }
}
