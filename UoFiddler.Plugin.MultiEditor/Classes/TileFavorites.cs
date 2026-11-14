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
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Plugin.MultiEditor.Classes
{
    /// <summary>A named page of favorite tiles.</summary>
    internal sealed class FavoritePage
    {
        public FavoritePage(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        /// <summary>Tile ids in the order they were added. The tile grid shows this list.</summary>
        public List<int> Tiles { get; } = new List<int>();
    }

    /// <summary>
    /// Favorite tiles on named pages, saved in the profile folder (MultiEditorFavorites.xml).
    /// UOHE bookmark files (an [Page] header, then $hex ids one per line) can be imported.
    /// </summary>
    internal sealed class TileFavorites
    {
        private const string _fileName = "MultiEditorFavorites.xml";

        public List<FavoritePage> Pages { get; } = new List<FavoritePage>();

        private static string FilePath => Path.Combine(Options.AppDataPath, _fileName);

        public static TileFavorites Load()
        {
            var favorites = new TileFavorites();
            try
            {
                if (File.Exists(FilePath))
                {
                    var dom = new XmlDocument();
                    dom.Load(FilePath);
                    foreach (XmlElement xPage in dom.GetElementsByTagName("Page"))
                    {
                        var page = new FavoritePage(xPage.GetAttribute("name"));
                        foreach (XmlElement xTile in xPage.GetElementsByTagName("Tile"))
                        {
                            if (TryParseId(xTile.GetAttribute("id"), out int id))
                            {
                                page.Tiles.Add(id);
                            }
                        }

                        favorites.Pages.Add(page);
                    }
                }
            }
            catch (Exception ex) when (ex is IOException or XmlException or UnauthorizedAccessException)
            {
                // A broken file starts the favorites over rather than stopping the editor.
                favorites.Pages.Clear();
            }

            if (favorites.Pages.Count == 0)
            {
                favorites.Pages.Add(new FavoritePage("Favorites"));
            }

            return favorites;
        }

        public void Save()
        {
            var dom = new XmlDocument();
            dom.AppendChild(dom.CreateXmlDeclaration("1.0", "utf-8", null));
            XmlElement root = dom.CreateElement("Favorites");
            dom.AppendChild(root);
            foreach (FavoritePage page in Pages)
            {
                XmlElement xPage = dom.CreateElement("Page");
                xPage.SetAttribute("name", page.Name);
                foreach (int id in page.Tiles)
                {
                    XmlElement xTile = dom.CreateElement("Tile");
                    xTile.SetAttribute("id", $"0x{id:X4}");
                    xPage.AppendChild(xTile);
                }

                root.AppendChild(xPage);
            }

            dom.Save(FilePath);
        }

        /// <summary>
        /// Reads a UOHE bookmark file (book_sys.ini, bookmark.ini): each [Section] becomes a page,
        /// each $hex line a tile. Returns the number of pages added.
        /// </summary>
        public int ImportUoheBookmarks(string path)
        {
            var imported = new List<FavoritePage>();
            FavoritePage page = null;
            foreach (string raw in File.ReadAllLines(path, Encoding.Latin1))
            {
                string line = raw.Trim();
                if (line.StartsWith('[') && line.EndsWith(']'))
                {
                    page = new FavoritePage(line[1..^1].Trim());
                    imported.Add(page);
                }
                else if (page != null && line.StartsWith('$') &&
                         int.TryParse(line[1..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int id) &&
                         !page.Tiles.Contains(id))
                {
                    page.Tiles.Add(id);
                }
            }

            // Sections without tiles are not worth a page.
            imported.RemoveAll(p => p.Tiles.Count == 0);
            Pages.AddRange(imported);
            return imported.Count;
        }

        private static bool TryParseId(string text, out int id)
        {
            id = 0;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            return text.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                ? int.TryParse(text[2..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out id)
                : int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out id);
        }
    }
}
