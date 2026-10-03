using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace UoFiddler.Localization
{
    /// <summary>
    /// 字符串字典 - 从JSON文件加载多语言字符串
    /// </summary>
    public class StringDictionary
    {
        private readonly Dictionary<string, string> _strings = new();
        private readonly string _languageCode;

        public StringDictionary(string languageCode)
        {
            _languageCode = languageCode;
        }

        /// <summary>
        /// 从JSON文件加载字符串字典
        /// </summary>
        public void LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"[LocalizationService] 字典文件不存在: {filePath}");
                return;
            }

            try
            {
                string jsonContent = File.ReadAllText(filePath);
                using (JsonDocument doc = JsonDocument.Parse(jsonContent))
                {
                    FlattenJsonElement(doc.RootElement, "");
                }
                Console.WriteLine($"[LocalizationService] 已加载 {_languageCode} 字典，共 {_strings.Count} 个条目");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LocalizationService] 加载字典失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 递归展开JSON对象，生成点号路径的键值对
        /// </summary>
        private void FlattenJsonElement(JsonElement element, string prefix)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in element.EnumerateObject())
                {
                    string key = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";
                    FlattenJsonElement(property.Value, key);
                }
            }
            else if (element.ValueKind == JsonValueKind.String)
            {
                _strings[prefix] = element.GetString() ?? "";
            }
        }

        /// <summary>
        /// 获取翻译字符串，如果不存在则返回null
        /// </summary>
        public string? GetString(string key)
        {
            return _strings.TryGetValue(key, out var value) ? value : null;
        }

        /// <summary>
        /// 获取所有字符串条目数
        /// </summary>
        public int Count => _strings.Count;
    }
}
