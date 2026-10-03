using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace UoFiddler.Localization
{
    /// <summary>
    /// 本地化服务 - 管理UI控件的多语言翻译
    /// </summary>
    public static class LocalizationService
    {
        private static StringDictionary? _currentDictionary;
        private static string _dictionariesPath = "";
        private static string _currentLanguage = "en-US";

        /// <summary>
        /// 初始化本地化服务
        /// </summary>
        public static void Initialize(string dictionariesPath)
        {
            _dictionariesPath = dictionariesPath;
            Console.WriteLine($"[LocalizationService] ========================================");
            Console.WriteLine($"[LocalizationService] 初始化本地化服务");
            Console.WriteLine($"[LocalizationService] 字典路径: {_dictionariesPath}");
            Console.WriteLine($"[LocalizationService] 路径是否存在: {Directory.Exists(_dictionariesPath)}");
            Console.WriteLine($"[LocalizationService] ========================================");
        }

        /// <summary>
        /// 设置当前语言并加载对应字典
        /// </summary>
        public static void SetLanguage(string languageCode)
        {
            _currentLanguage = languageCode;
            _currentDictionary = new StringDictionary(languageCode);

            string dictionaryFile = Path.Combine(_dictionariesPath, $"{languageCode}.json");
            Console.WriteLine($"[LocalizationService] 设置语言: {languageCode}");
            Console.WriteLine($"[LocalizationService] 字典文件: {dictionaryFile}");
            Console.WriteLine($"[LocalizationService] 文件是否存在: {File.Exists(dictionaryFile)}");
            
            _currentDictionary.LoadFromFile(dictionaryFile);
            
            Console.WriteLine($"[LocalizationService] 字典加载完成");
        }

        /// <summary>
        /// 获取翻译字符串
        /// </summary>
        public static string? GetString(string key)
        {
            return _currentDictionary?.GetString(key);
        }

        /// <summary>
        /// 对整个Form进行本地化处理
        /// </summary>
        public static void LocalizeForm(Form form)
        {
            if (_currentDictionary == null)
            {
                Console.WriteLine("[LocalizationService] 字典未初始化");
                return;
            }

            try
            {
                Console.WriteLine($"[LocalizationService] 开始本地化Form: {form.Name}");
                
                // 本地化Form标题
                string? formTitle = GetString($"Forms.{form.Name}.Title");
                if (!string.IsNullOrEmpty(formTitle))
                {
                    form.Text = formTitle;
                    Console.WriteLine($"[LocalizationService] Form标题已本地化: {formTitle}");
                }

                // 递归本地化所有控件
                LocalizeControls(form.Controls, form.Name);

                // 本地化菜单栏
                if (form.MainMenuStrip != null)
                {
                    Console.WriteLine($"[LocalizationService] 检测到MainMenuStrip，开始本地化菜单");
                    LocalizeMenuStrip(form.MainMenuStrip, form.Name);
                }
                else
                {
                    Console.WriteLine($"[LocalizationService] 警告: Form没有MainMenuStrip");
                }

                // 本地化工具栏中的菜单按钮 (用于ToolStrip中的ToolStripDropDownButton)
                foreach (var toolStrip in form.Controls.OfType<ToolStrip>())
                {
                    Console.WriteLine($"[LocalizationService] 检测到ToolStrip，开始本地化其中的菜单按钮");
                    LocalizeToolStripItems(toolStrip.Items, form.Name);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalizationService] 本地化Form失败: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// 本地化ToolStrip中的所有项
        /// </summary>
        private static void LocalizeToolStripItems(ToolStripItemCollection items, string formName)
        {
            foreach (var item in items)
            {
                if (item is ToolStripDropDownButton dropDownBtn)
                {
                    Console.WriteLine($"[LocalizationService] 处理ToolStripDropDownButton: {dropDownBtn.Name}");
                    LocalizeToolStripItem(dropDownBtn, formName);
                }
                else if (item is ToolStripMenuItem menuItem)
                {
                    Console.WriteLine($"[LocalizationService] 处理ToolStripMenuItem: {menuItem.Name}");
                    LocalizeMenuStripItem(menuItem, formName);
                }
                else if (item is ToolStripButton button)
                {
                    Console.WriteLine($"[LocalizationService] 处理ToolStripButton: {button.Name}");
                    LocalizeToolStripButton(button, formName);
                }
            }
        }

        /// <summary>
        /// 本地化ToolStripDropDownButton和其他ToolStripItem
        /// </summary>
        private static void LocalizeToolStripItem(ToolStripDropDownButton item, string formName)
        {
            if (!string.IsNullOrEmpty(item.Name))
            {
                string key = $"Forms.{formName}.Menus.{item.Name}";
                string? localizedText = GetString(key);
                if (!string.IsNullOrEmpty(localizedText))
                {
                    Console.WriteLine($"[LocalizationService] 菜单按钮 {item.Name} → {localizedText}");
                    item.Text = localizedText;
                }
            }

            // 递归处理子项
            foreach (ToolStripMenuItem subItem in item.DropDownItems.OfType<ToolStripMenuItem>())
            {
                LocalizeMenuStripItem(subItem, formName);
            }
        }

        /// <summary>
        /// 本地化ToolStripButton
        /// </summary>
        private static void LocalizeToolStripButton(ToolStripButton button, string formName)
        {
            if (!string.IsNullOrEmpty(button.Name))
            {
                string? localizedText = GetString($"Forms.{formName}.{button.Name}");
                if (!string.IsNullOrEmpty(localizedText))
                {
                    Console.WriteLine($"[LocalizationService] ToolStripButton {button.Name} → {localizedText}");
                    button.Text = localizedText;
                }
            }
        }

        /// <summary>
        /// 本地化菜单条
        /// </summary>
        private static void LocalizeMenuStrip(MenuStrip menuStrip, string formName)
        {
            foreach (ToolStripMenuItem item in menuStrip.Items.OfType<ToolStripMenuItem>())
            {
                LocalizeMenuStripItem(item, formName);
            }
        }

        /// <summary>
        /// 递归本地化菜单项
        /// </summary>
        private static void LocalizeMenuStripItem(ToolStripMenuItem menuItem, string formName)
        {
            if (!string.IsNullOrEmpty(menuItem.Name))
            {
                string? localizedText = GetString($"Forms.{formName}.Menus.{menuItem.Name}");
                if (!string.IsNullOrEmpty(localizedText))
                {
                    menuItem.Text = localizedText;
                }
            }

            // 递归处理子菜单
            foreach (ToolStripMenuItem subItem in menuItem.DropDownItems.OfType<ToolStripMenuItem>())
            {
                LocalizeMenuStripItem(subItem, formName);
            }
        }

        /// <summary>
        /// 递归本地化控件集合
        /// </summary>
        private static void LocalizeControls(Control.ControlCollection controls, string formName)
        {
            foreach (Control control in controls)
            {
                LocalizeControl(control, formName);

                // 递归处理子控件
                if (control.HasChildren)
                {
                    LocalizeControls(control.Controls, formName);
                }
            }
        }

        /// <summary>
        /// 本地化单个控件
        /// </summary>
        private static void LocalizeControl(Control control, string formName)
        {
            if (string.IsNullOrEmpty(control.Name) || control.Name.StartsWith("_"))
                return;

            // 按类型处理不同的控件
            if (control is TabControl tabControl)
            {
                LocalizeTabPages(tabControl, formName);
            }
            else if (control is Button || control is CheckBox || control is Label || 
                     control is GroupBox || control is RadioButton)
            {
                // 普通文本控件 - 尝试两种键格式
                string? localizedText = GetString($"Forms.{formName}.{control.Name}.Text");
                if (string.IsNullOrEmpty(localizedText))
                {
                    // 如果没有找到 .Text 后缀的键，尝试直接使用控件名
                    localizedText = GetString($"Forms.{formName}.{control.Name}");
                }
                
                if (!string.IsNullOrEmpty(localizedText))
                {
                    Console.WriteLine($"[LocalizationService] 控件 {control.Name} → {localizedText}");
                    control.Text = localizedText;
                }
            }
        }

        /// <summary>
        /// 本地化TabControl的标签页
        /// </summary>
        private static void LocalizeTabPages(TabControl tabControl, string formName)
        {
            Console.WriteLine($"[LocalizationService] 本地化TabControl，共{tabControl.TabPages.Count}个标签页");
            
            foreach (TabPage tabPage in tabControl.TabPages)
            {
                if (!string.IsNullOrEmpty(tabPage.Name))
                {
                    string key = $"Forms.{formName}.Tabs.{tabPage.Name}";
                    string? localizedText = GetString(key);
                    if (!string.IsNullOrEmpty(localizedText))
                    {
                        Console.WriteLine($"[LocalizationService] 标签页 {tabPage.Name} → {localizedText}");
                        tabPage.Text = localizedText;
                    }
                }

                // 本地化标签页内的控件
                LocalizeControls(tabPage.Controls, formName);
            }
        }

        /// <summary>
        /// 获取当前语言代码
        /// </summary>
        public static string GetCurrentLanguage() => _currentLanguage;
    }
}
