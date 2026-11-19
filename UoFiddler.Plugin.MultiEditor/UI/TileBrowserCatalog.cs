/***************************************************************************
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
using System.Linq;
using System.Xml;
using Ultima;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Plugin.MultiEditor.UI
{
    /// <summary>
    /// The tile categories of multieditor.xml plus every valid static, read without throwing:
    /// a missing or broken file leaves the tree empty.
    /// </summary>
    internal sealed class TileBrowserCatalog
    {
        private List<int> _allTiles;
        private Dictionary<int, TileBrowserCategory> _firstCategory;

        private TileBrowserCatalog()
        {
        }

        public static string FilePath => System.IO.Path.Combine(Options.AppDataPath ?? string.Empty, "plugins", "multieditor.xml");

        public List<TileBrowserCategory> Groups { get; } = new List<TileBrowserCategory>();

        /// <summary>Why the file could not be read, or null.</summary>
        public string Error { get; private set; }

        /// <summary>Every valid static, built on first use so it reflects the art loaded then.</summary>
        public IReadOnlyList<int> AllTiles
        {
            get
            {
                if (_allTiles == null)
                {
                    int maxItemId = Art.GetMaxItemId();
                    _allTiles = new List<int>(maxItemId + 1);
                    for (int i = 0; i <= maxItemId; ++i)
                    {
                        if (Art.IsValidStatic(i))
                        {
                            _allTiles.Add(i);
                        }
                    }
                }

                return _allTiles;
            }
        }

        public static TileBrowserCatalog Load()
        {
            var catalog = new TileBrowserCatalog();
            string fileName = FilePath;
            try
            {
                if (!File.Exists(fileName))
                {
                    return catalog;
                }

                var dom = new XmlDocument();
                dom.Load(fileName);
                XmlElement root = dom["TileGroups"];
                if (root == null)
                {
                    return catalog;
                }

                foreach (XmlElement xGroup in root.ChildNodes.OfType<XmlElement>())
                {
                    var group = new TileBrowserCategory(xGroup.GetAttribute("name"), null);
                    var seen = new HashSet<int>();
                    foreach (XmlElement xSub in xGroup.ChildNodes.OfType<XmlElement>())
                    {
                        if (xSub.Name != "subgroup")
                        {
                            continue;
                        }

                        var sub = new TileBrowserCategory(xSub.GetAttribute("name"), group);
                        foreach (XmlElement xEntry in xSub.ChildNodes.OfType<XmlElement>())
                        {
                            if (int.TryParse(xEntry.GetAttribute("index"), NumberStyles.Integer, CultureInfo.InvariantCulture, out int id) &&
                                Art.IsValidStatic(id))
                            {
                                sub.Tiles.Add(id);
                                if (seen.Add(id))
                                {
                                    group.Tiles.Add(id);
                                }
                            }
                        }

                        group.Children.Add(sub);
                    }

                    catalog.Groups.Add(group);
                }
            }
            catch (Exception ex) when (ex is IOException or XmlException or UnauthorizedAccessException)
            {
                catalog.Groups.Clear();
                catalog.Error = ex.Message;
            }

            return catalog;
        }

        /// <summary>The first subgroup that lists the tile, or null.</summary>
        public TileBrowserCategory FindCategoryOf(int id)
        {
            if (_firstCategory == null)
            {
                _firstCategory = new Dictionary<int, TileBrowserCategory>();
                foreach (TileBrowserCategory sub in Groups.SelectMany(g => g.Children))
                {
                    foreach (int tile in sub.Tiles)
                    {
                        _firstCategory.TryAdd(tile, sub);
                    }
                }
            }

            return _firstCategory.TryGetValue(id, out TileBrowserCategory category) ? category : null;
        }
    }
}
