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
using System.Drawing;
using System.Windows.Forms;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.Forms
{
    public partial class MultisHelpForm : Form
    {
        private Func<string, string?>? _localizationGetter;

        public MultisHelpForm()
        {
            InitializeComponent();
        }

        public void SetLocalization(Func<string, string?> getLocalized)
        {
            _localizationGetter = getLocalized;
            ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            if (_localizationGetter == null)
            {
                // 如果没有汉化，使用默认英文
                PopulateShortcuts();
                return;
            }

            // 应用窗口标题
            Text = _localizationGetter("Forms.MultisControl.HelpForm.Title") ?? "Multis — Keyboard Shortcuts & Controls";

            // 应用列标题
            _columnKey.Text = _localizationGetter("Forms.MultisControl.HelpForm.ColumnShortcut") ?? "Shortcut / Control";
            _columnAction.Text = _localizationGetter("Forms.MultisControl.HelpForm.ColumnAction") ?? "Action";

            // 应用按钮文本
            _btnClose.Text = _localizationGetter("Forms.MultisControl.HelpForm.ButtonClose") ?? "Close";

            // 清除并重新填充列表
            _listView.Items.Clear();
            PopulateShortcuts();
        }

        private void PopulateShortcuts()
        {
            // Preview 部分
            AddHeader(GetLocalizedString("Forms.MultisControl.HelpForm.HeaderPreview", "Preview"));
            Add(GetLocalizedString("Forms.MultisControl.HelpForm.PreviewFit", "Fit preview to window"),
                GetLocalizedString("Forms.MultisControl.HelpForm.PreviewFitDesc", "Toggle button on toolbar — scales to fit or shows at 100% with scrollbars"));

            // Zoom 部分
            AddHeader(GetLocalizedString("Forms.MultisControl.HelpForm.HeaderZoom", "Zoom (100% mode only)"));
            Add(GetLocalizedString("Forms.MultisControl.HelpForm.ZoomCtrlWheel", "Ctrl + Mouse Wheel"),
                GetLocalizedString("Forms.MultisControl.HelpForm.ZoomCtrlWheelDesc", "Zoom In / Out"));
            Add(GetLocalizedString("Forms.MultisControl.HelpForm.ZoomShiftPlus", "Shift + = (Plus key)"),
                GetLocalizedString("Forms.MultisControl.HelpForm.ZoomShiftPlusDesc", "Zoom In"));
            Add(GetLocalizedString("Forms.MultisControl.HelpForm.ZoomMinus", "- (Minus key)"),
                GetLocalizedString("Forms.MultisControl.HelpForm.ZoomMinusDesc", "Zoom Out"));
            Add(GetLocalizedString("Forms.MultisControl.HelpForm.ZoomNumpadPlus", "Numpad +"),
                GetLocalizedString("Forms.MultisControl.HelpForm.ZoomNumpadPlusDesc", "Zoom In"));
            Add(GetLocalizedString("Forms.MultisControl.HelpForm.ZoomNumpadMinus", "Numpad -"),
                GetLocalizedString("Forms.MultisControl.HelpForm.ZoomNumpadMinusDesc", "Zoom Out"));
            Add(GetLocalizedString("Forms.MultisControl.HelpForm.ZoomCtrl0", "Ctrl + 0"),
                GetLocalizedString("Forms.MultisControl.HelpForm.ZoomCtrl0Desc", "Reset zoom to 100%"));

            // Panning 部分
            AddHeader(GetLocalizedString("Forms.MultisControl.HelpForm.HeaderPanning", "Panning (100% mode only)"));
            Add(GetLocalizedString("Forms.MultisControl.HelpForm.PanningLeftDrag", "Left-click drag"),
                GetLocalizedString("Forms.MultisControl.HelpForm.PanningLeftDragDesc", "Pan the view"));
        }

        private string GetLocalizedString(string key, string fallback)
        {
            return _localizationGetter?.Invoke(key) ?? fallback;
        }

        private void AddHeader(string text)
        {
            var item = new ListViewItem(text)
            {
                Font = new Font(_listView.Font, FontStyle.Bold),
                ForeColor = Options.DarkMode ? Color.OrangeRed : Color.MediumBlue,
            };
            item.SubItems.Add(string.Empty);
            _listView.Items.Add(item);
        }

        private void Add(string key, string action)
        {
            var item = new ListViewItem(key);
            item.SubItems.Add(action);
            _listView.Items.Add(item);
        }
    }
}
