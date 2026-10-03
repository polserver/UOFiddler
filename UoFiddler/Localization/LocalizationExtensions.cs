using System.Windows.Forms;

namespace UoFiddler.Localization
{
    /// <summary>
    /// 本地化扩展方法
    /// </summary>
    public static class LocalizationExtensions
    {
        /// <summary>
        /// 为Form启用本地化
        /// </summary>
        public static void ApplyLocalization(this Form form)
        {
            LocalizationService.LocalizeForm(form);
        }

        /// <summary>
        /// 获取本地化字符串
        /// </summary>
        public static string GetLocalized(this string key)
        {
            return LocalizationService.GetString(key) ?? key;
        }
    }
}
