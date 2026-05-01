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

using System.IO;
using System.Xml;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Classes
{
    /// <summary>
    /// Profile-agnostic application settings stored separately from profiles.
    /// </summary>
    public static class AppSettings
    {
        private static string FilePath => Path.Combine(Options.AppDataPath, "app_settings.xml");

        public static bool DarkMode { get; set; } = false;

        public static void Load()
        {
            if (!File.Exists(FilePath))
            {
                return;
            }

            XmlDocument dom = new XmlDocument();
            dom.Load(FilePath);
            XmlElement root = dom["AppSettings"];

            XmlElement elem = (XmlElement)root?.SelectSingleNode("DarkMode");
            if (elem != null)
            {
                DarkMode = bool.Parse(elem.GetAttribute("active"));
            }
        }

        public static void Save()
        {
            if (!Directory.Exists(Options.AppDataPath))
            {
                Directory.CreateDirectory(Options.AppDataPath);
            }

            XmlDocument dom = new XmlDocument();
            XmlDeclaration decl = dom.CreateXmlDeclaration("1.0", "utf-8", null);
            dom.AppendChild(decl);
            XmlElement root = dom.CreateElement("AppSettings");

            XmlElement elem = dom.CreateElement("DarkMode");
            elem.SetAttribute("active", DarkMode.ToString());
            root.AppendChild(elem);

            dom.AppendChild(root);
            dom.Save(FilePath);
        }
    }
}
