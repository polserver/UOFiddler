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
    /// <summary>A node of the multieditor.xml tree: a group (union of its subgroups) or a subgroup.</summary>
    internal sealed class TileBrowserCategory
    {
        public TileBrowserCategory(string name, TileBrowserCategory parent)
        {
            Name = name;
            Parent = parent;
        }

        public string Name { get; }

        public TileBrowserCategory Parent { get; }

        public List<TileBrowserCategory> Children { get; } = new List<TileBrowserCategory>();

        public List<int> Tiles { get; } = new List<int>();

        public string Path => Parent == null ? Name : Parent.Path + " / " + Name;

        public override string ToString() => Name;
    }

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

    /// <summary>
    /// What the search box asks for: either ids (decimal, 0x or $ hex, ranges like 0x100-0x1FF,
    /// several separated by spaces or commas) or name words that must all occur.
    /// </summary>
    internal sealed class TileBrowserQuery
    {
        private static readonly char[] _separators = { ' ', ',', ';', '\t' };

        private readonly List<(int Low, int High)> _ranges;
        private readonly string[] _words;

        private TileBrowserQuery(List<(int, int)> ranges, string[] words)
        {
            _ranges = ranges;
            _words = words;
        }

        public bool IsIdQuery => _ranges != null;

        /// <summary>The query, or null when the text is blank.</summary>
        public static TileBrowserQuery Parse(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            string[] tokens = text.Split(_separators, StringSplitOptions.RemoveEmptyEntries);
            var ranges = new List<(int, int)>();
            foreach (string token in tokens)
            {
                if (!TryParseRange(token, out int low, out int high))
                {
                    ranges = null;
                    break;
                }

                ranges.Add((low, high));
            }

            return ranges != null ? new TileBrowserQuery(ranges, null) : new TileBrowserQuery(null, tokens);
        }

        public bool MatchesId(int id)
        {
            if (_ranges == null)
            {
                return false;
            }

            foreach ((int low, int high) in _ranges)
            {
                if (id >= low && id <= high)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>True when every word occurs in the text (ignoring case). Id queries match no text.</summary>
        public bool MatchesText(string text)
        {
            if (_words == null || string.IsNullOrEmpty(text))
            {
                return false;
            }

            foreach (string word in _words)
            {
                if (text.IndexOf(word, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    return false;
                }
            }

            return true;
        }

        public bool MatchesTile(int id) => IsIdQuery ? MatchesId(id) : MatchesText(TileBrowserThumbnails.NameOf(id));

        private static bool TryParseRange(string token, out int low, out int high)
        {
            int dash = token.IndexOf('-', 1);
            if (dash > 0 && dash < token.Length - 1)
            {
                if (TryParseId(token[..dash], out low) && TryParseId(token[(dash + 1)..], out high))
                {
                    if (low > high)
                    {
                        (low, high) = (high, low);
                    }

                    return true;
                }

                high = 0;
                return false;
            }

            bool ok = TryParseId(token, out low);
            high = low;
            return ok;
        }

        private static bool TryParseId(string text, out int id)
        {
            if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return int.TryParse(text.AsSpan(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out id);
            }

            if (text.StartsWith('$'))
            {
                return int.TryParse(text.AsSpan(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out id);
            }

            return int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out id);
        }
    }
}
