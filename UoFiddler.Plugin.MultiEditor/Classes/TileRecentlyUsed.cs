using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Plugin.MultiEditor.Classes
{
    public static class TileRecentlyUsed
    {
        public const int MaxRecentTiles = 40;

        private static string GetFilePath()
            => Path.Combine(Options.AppDataPath, "MultiEditorRecentTiles.xml");

        public static List<int> Load()
        {
            var result = new List<int>();
            string path = GetFilePath();
            if (!File.Exists(path))
            {
                return result;
            }

            try
            {
                var doc = XDocument.Load(path);
                foreach (var elem in doc.Root?.Elements("Tile") ?? Array.Empty<XElement>())
                {
                    string idStr = (string)elem.Attribute("id");
                    if (idStr != null && int.TryParse(idStr, System.Globalization.NumberStyles.HexNumber, null, out int id))
                    {
                        result.Add(id);
                    }
                }
            }
            catch
            {
                // Silently ignore corrupt file
            }

            return result;
        }

        public static void Save(List<int> tiles)
        {
            var doc = new XDocument(new XElement("RecentTiles"));
            foreach (int id in tiles)
            {
                doc.Root.Add(new XElement("Tile", new XAttribute("id", id.ToString("X"))));
            }

            try
            {
                doc.Save(GetFilePath());
            }
            catch
            {
                // Silently ignore write errors
            }
        }
    }
}
